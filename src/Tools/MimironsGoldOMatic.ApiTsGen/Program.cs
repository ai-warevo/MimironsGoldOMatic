using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MimironsGoldOMatic.ApiTsGen;

internal static class Program
{
    private static readonly Dictionary<string, string> ResponseTypeFallback = new(StringComparer.OrdinalIgnoreCase)
    {
        ["GET /api/version"] = "VersionInfoDto",
        ["GET /api/roulette/state"] = "RouletteStateResponse",
        ["GET /api/pool/me"] = "PoolMeResponse",
        ["GET /api/payouts/my-last"] = "PayoutDto",
        ["POST /api/payouts/claim"] = "PayoutDto",
        ["GET /api/payouts/pending"] = "PayoutDto[]",
        ["PATCH /api/payouts/{id:guid}/status"] = "PayoutDto",
        ["POST /api/e2e/prepare-pending-payout"] = "E2ePreparePendingResponse",
    };

    public static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine(
                "Usage: MimironsGoldOMatic.ApiTsGen <sourceRoot1> [sourceRoot2 ...] <tsApiOutputDir>");
            return 1;
        }

        var outputDir = Path.GetFullPath(args[^1]);
        Directory.CreateDirectory(outputDir);

        var syntaxTrees = args[..^1]
            .Select(a => Path.GetFullPath(a))
            .SelectMany(LoadSyntaxTrees)
            .ToArray();

        var dtoMap = ParseDtoModels(syntaxTrees);
        var endpoints = ParseEndpoints(syntaxTrees);
        var referencedTypeNames = CollectReferencedTypes(endpoints, dtoMap);

        var modelsTs = RenderModels(dtoMap, referencedTypeNames);
        var clientTs = RenderClient(endpoints);

        WriteIfChanged(Path.Combine(outputDir, "models.ts"), modelsTs);
        WriteIfChanged(Path.Combine(outputDir, "client.ts"), clientTs);
        Console.WriteLine("Generated models.ts and client.ts");
        return 0;
    }

    private static IEnumerable<SyntaxTree> LoadSyntaxTrees(string root)
    {
        foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            yield return CSharpSyntaxTree.ParseText(File.ReadAllText(file), path: file);
        }
    }

    private static Dictionary<string, DtoModel> ParseDtoModels(IEnumerable<SyntaxTree> trees)
    {
        var models = new Dictionary<string, DtoModel>(StringComparer.Ordinal);
        foreach (var tree in trees)
        {
            var root = tree.GetRoot();

            foreach (var enumDecl in root.DescendantNodes().OfType<EnumDeclarationSyntax>())
            {
                var name = enumDecl.Identifier.Text;
                if (!ShouldEmitType(name))
                    continue;

                var values = enumDecl.Members.Select(m => m.Identifier.Text).ToArray();
                models[name] = new DtoModel(name, values);
            }

            foreach (var recordDecl in root.DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                var name = recordDecl.Identifier.Text;
                if (!ShouldEmitType(name))
                    continue;

                var properties = new List<DtoProperty>();

                if (recordDecl.ParameterList is not null)
                {
                    foreach (var p in recordDecl.ParameterList.Parameters)
                    {
                        var propName = GetJsonPropertyName(p.AttributeLists, p.Identifier.Text);
                        properties.Add(new DtoProperty(propName, NormalizeTypeName(p.Type), IsNullable(p.Type)));
                    }
                }

                foreach (var prop in recordDecl.Members.OfType<PropertyDeclarationSyntax>())
                {
                    var propName = GetJsonPropertyName(prop.AttributeLists, prop.Identifier.Text);
                    properties.Add(new DtoProperty(propName, NormalizeTypeName(prop.Type), IsNullable(prop.Type)));
                }

                models[name] = new DtoModel(name, properties);
            }

            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var name = classDecl.Identifier.Text;
                if (!ShouldEmitType(name))
                    continue;

                var properties = classDecl.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(p => new DtoProperty(
                        GetJsonPropertyName(p.AttributeLists, p.Identifier.Text),
                        NormalizeTypeName(p.Type),
                        IsNullable(p.Type)))
                    .ToList();

                if (properties.Count > 0)
                    models[name] = new DtoModel(name, properties);
            }
        }

        return models;
    }

    private static List<ApiEndpoint> ParseEndpoints(IEnumerable<SyntaxTree> trees)
    {
        var endpoints = new List<ApiEndpoint>();

        foreach (var tree in trees.Where(t => t.FilePath.Contains($"{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
        {
            var root = tree.GetRoot();
            foreach (var classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var classRoute = ReadRouteValue(classDecl.AttributeLists) ?? string.Empty;

                foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
                {
                    var http = ReadHttpMethod(method.AttributeLists);
                    if (http is null)
                        continue;

                    var methodRoute = ReadRouteValue(method.AttributeLists) ?? string.Empty;
                    var route = CombineRoute(classRoute, methodRoute);
                    var requestType = method.ParameterList.Parameters
                        .FirstOrDefault(p => HasAttribute(p.AttributeLists, "FromBody"))
                        ?.Type;
                    var requestTypeName = requestType is null ? null : NormalizeTypeName(requestType);

                    var responseType = ReadResponseTypeFromAttributes(method.AttributeLists)
                        ?? ReadResponseTypeFromReturnType(method.ReturnType);
                    if (responseType is "IActionResult" or "ActionResult")
                        responseType = null;

                    var key = $"{http} {route}";
                    if (responseType is null && ResponseTypeFallback.TryGetValue(key, out var fallback))
                        responseType = fallback;

                    endpoints.Add(new ApiEndpoint(
                        http,
                        route,
                        BuildMethodName(http, route),
                        requestTypeName,
                        responseType));
                }
            }
        }

        return endpoints
            .OrderBy(e => e.HttpMethod, StringComparer.Ordinal)
            .ThenBy(e => e.Route, StringComparer.Ordinal)
            .ToList();
    }

    private static HashSet<string> CollectReferencedTypes(IEnumerable<ApiEndpoint> endpoints, IReadOnlyDictionary<string, DtoModel> dtoMap)
    {
        var referenced = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();

        void EnqueueType(string? typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return;
            foreach (var token in ExtractTypeTokens(typeName))
            {
                if (dtoMap.ContainsKey(token) && referenced.Add(token))
                    queue.Enqueue(token);
            }
        }

        foreach (var endpoint in endpoints)
        {
            EnqueueType(endpoint.RequestTypeName);
            EnqueueType(endpoint.ResponseTypeName);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!dtoMap.TryGetValue(current, out var model))
                continue;
            foreach (var prop in model.Properties)
                EnqueueType(prop.CSharpType);
        }

        return referenced;
    }

    private static string RenderModels(IReadOnlyDictionary<string, DtoModel> dtoMap, HashSet<string> includedNames)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// AUTO-GENERATED FILE. DO NOT EDIT MANUALLY.");
        sb.AppendLine("// Generated by src/Tools/MimironsGoldOMatic.ApiTsGen.");
        sb.AppendLine("// Any manual changes will be overwritten by the next generation run.");
        sb.AppendLine();

        foreach (var model in dtoMap.Values.Where(m => includedNames.Contains(m.Name)).OrderBy(m => m.Name, StringComparer.Ordinal))
        {
            if (model.EnumValues.Count > 0)
            {
                sb.AppendLine($"export type {model.Name} =");
                for (var i = 0; i < model.EnumValues.Count; i++)
                {
                    var suffix = i == model.EnumValues.Count - 1 ? ";" : "";
                    sb.AppendLine($"  | '{model.EnumValues[i]}'{suffix}");
                }
                sb.AppendLine();
                continue;
            }

            sb.AppendLine($"export interface {model.Name} {{");
            foreach (var prop in model.Properties)
            {
                var tsType = MapTypeToTypeScript(prop.CSharpType, prop.Nullable);
                sb.AppendLine($"  {prop.Name}: {tsType}");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string RenderClient(IReadOnlyList<ApiEndpoint> endpoints)
    {
        var modelsToImport = endpoints
            .SelectMany(e => new[] { e.RequestTypeName, e.ResponseTypeName })
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .SelectMany(ExtractTypeTokens)
            .Where(IsModelToken)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine("// AUTO-GENERATED FILE. DO NOT EDIT MANUALLY.");
        sb.AppendLine("// Generated by src/Tools/MimironsGoldOMatic.ApiTsGen.");
        sb.AppendLine("// Any manual changes will be overwritten by the next generation run.");
        sb.AppendLine("import axios, { type AxiosInstance } from 'axios'");
        if (modelsToImport.Length > 0)
        {
            sb.AppendLine("import type {");
            foreach (var model in modelsToImport)
                sb.AppendLine($"  {model},");
            sb.AppendLine("} from './models'");
        }
        sb.AppendLine();
        sb.AppendLine("export class MimironsGoldOMaticApiClient {");
        sb.AppendLine("  private readonly client: AxiosInstance");
        sb.AppendLine();
        sb.AppendLine("  public constructor(");
        sb.AppendLine("    baseUrl: string,");
        sb.AppendLine("    tokenProvider: () => string | null,");
        sb.AppendLine("    axiosInstance?: AxiosInstance,");
        sb.AppendLine("  ) {");
        sb.AppendLine("    this.client = axiosInstance ?? axios.create({");
        sb.AppendLine("      baseURL: baseUrl.replace(/\\/$/, ''),");
        sb.AppendLine("      headers: {");
        sb.AppendLine("        'Content-Type': 'application/json',");
        sb.AppendLine("      },");
        sb.AppendLine("    })");
        sb.AppendLine();
        sb.AppendLine("    this.client.interceptors.request.use((config) => {");
        sb.AppendLine("      const token = tokenProvider()");
        sb.AppendLine("      if (token) {");
        sb.AppendLine("        config.headers.Authorization = `Bearer ${token}`");
        sb.AppendLine("      }");
        sb.AppendLine("      return config");
        sb.AppendLine("    })");
        sb.AppendLine("  }");
        sb.AppendLine();

        foreach (var endpoint in endpoints)
        {
            var response = string.IsNullOrWhiteSpace(endpoint.ResponseTypeName) ? "void" : MapTypeToTypeScript(endpoint.ResponseTypeName!, false);
            var requestTsType = string.IsNullOrWhiteSpace(endpoint.RequestTypeName) ? null : MapTypeToTypeScript(endpoint.RequestTypeName!, false);
            var methodLower = endpoint.HttpMethod.ToLowerInvariant();
            var routeParameters = ParseRouteParameters(endpoint.Route).ToArray();
            var routeExpression = BuildRouteExpression(endpoint.Route, routeParameters);
            var methodParameters = new List<string>();
            methodParameters.AddRange(routeParameters.Select(p => $"{p}: string"));
            if (requestTsType is not null)
                methodParameters.Add($"request: {requestTsType}");
            methodParameters.Add("signal?: AbortSignal");
            var signature = string.Join(", ", methodParameters);

            if (requestTsType is null)
            {
                sb.AppendLine($"  public async {endpoint.MethodName}({signature}): Promise<{response}> {{");
                if (response == "void")
                {
                    if (endpoint.HttpMethod == "POST" || endpoint.HttpMethod == "PATCH" || endpoint.HttpMethod == "PUT")
                        sb.AppendLine($"    await this.client.{methodLower}({routeExpression}, undefined, {{ signal }})");
                    else
                        sb.AppendLine($"    await this.client.{methodLower}({routeExpression}, {{ signal }})");
                    sb.AppendLine("  }");
                }
                else
                {
                    if (endpoint.HttpMethod == "POST" || endpoint.HttpMethod == "PATCH" || endpoint.HttpMethod == "PUT")
                        sb.AppendLine($"    const {{ data }} = await this.client.{methodLower}<{response}>({routeExpression}, undefined, {{ signal }})");
                    else
                        sb.AppendLine($"    const {{ data }} = await this.client.{methodLower}<{response}>({routeExpression}, {{ signal }})");
                    sb.AppendLine("    return data");
                    sb.AppendLine("  }");
                }
            }
            else
            {
                sb.AppendLine($"  public async {endpoint.MethodName}({signature}): Promise<{response}> {{");
                if (response == "void")
                {
                    sb.AppendLine($"    await this.client.{methodLower}({routeExpression}, request, {{ signal }})");
                    sb.AppendLine("  }");
                }
                else
                {
                    sb.AppendLine($"    const {{ data }} = await this.client.{methodLower}<{response}>({routeExpression}, request, {{ signal }})");
                    sb.AppendLine("    return data");
                    sb.AppendLine("  }");
                }
            }
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static bool IsModelToken(string token) =>
        token is not ("string" or "number" or "boolean" or "unknown" or "void" or "null");

    private static IEnumerable<string> ExtractTypeTokens(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return Enumerable.Empty<string>();
        var cleaned = typeName.Replace("?", "", StringComparison.Ordinal);
        cleaned = cleaned.Replace("[]", "", StringComparison.Ordinal);
        var separators = new[] { '<', '>', ',', ' ', '|', '.', ':' };
        return cleaned
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .Where(s => !s.Equals("global", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldEmitType(string name) =>
        name.EndsWith("Request", StringComparison.Ordinal)
        || name.EndsWith("Response", StringComparison.Ordinal)
        || name.EndsWith("Dto", StringComparison.Ordinal)
        || name.EndsWith("State", StringComparison.Ordinal)
        || name.EndsWith("Status", StringComparison.Ordinal)
        || name.StartsWith("Patch", StringComparison.Ordinal);

    private static string BuildMethodName(string httpMethod, string route)
    {
        var verb = httpMethod.ToLowerInvariant() switch
        {
            "get" => "get",
            "post" => "post",
            "patch" => "patch",
            "put" => "put",
            "delete" => "delete",
            _ => "call"
        };

        var parts = route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !p.Equals("api", StringComparison.OrdinalIgnoreCase))
            .Select(PartToPascalCase);
        return verb + string.Concat(parts);
    }

    private static string PartToPascalCase(string part)
    {
        var words = part.Split(new[] { '-', '_', ':', '{', '}', '.' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray()))
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToArray();
        if (words.Length == 0)
            return "Route";
        return string.Concat(words.Select(w => char.ToUpperInvariant(w[0]) + w[1..]));
    }

    private static string? ReadResponseTypeFromReturnType(TypeSyntax returnType)
    {
        if (returnType is GenericNameSyntax gen && (gen.Identifier.Text is "ActionResult" or "Task") && gen.TypeArgumentList.Arguments.Count > 0)
        {
            var inner = gen.TypeArgumentList.Arguments[0];
            if (inner is GenericNameSyntax nested && nested.Identifier.Text == "ActionResult" && nested.TypeArgumentList.Arguments.Count > 0)
                return NormalizeTypeName(nested.TypeArgumentList.Arguments[0]);
            return NormalizeTypeName(inner);
        }

        return null;
    }

    private static string? ReadResponseTypeFromAttributes(SyntaxList<AttributeListSyntax> attrs)
    {
        foreach (var attr in attrs.SelectMany(a => a.Attributes))
        {
            var name = attr.Name.ToString();
            if (!name.Contains("ProducesResponseType", StringComparison.Ordinal))
                continue;
            var first = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression as TypeOfExpressionSyntax;
            if (first?.Type is not null)
                return NormalizeTypeName(first.Type);
        }

        return null;
    }

    private static string? ReadHttpMethod(SyntaxList<AttributeListSyntax> attrs)
    {
        foreach (var attr in attrs.SelectMany(a => a.Attributes))
        {
            var name = attr.Name.ToString();
            if (name.Contains("HttpGet", StringComparison.Ordinal))
                return "GET";
            if (name.Contains("HttpPost", StringComparison.Ordinal))
                return "POST";
            if (name.Contains("HttpPatch", StringComparison.Ordinal))
                return "PATCH";
            if (name.Contains("HttpPut", StringComparison.Ordinal))
                return "PUT";
            if (name.Contains("HttpDelete", StringComparison.Ordinal))
                return "DELETE";
        }

        return null;
    }

    private static string? ReadRouteValue(SyntaxList<AttributeListSyntax> attrs)
    {
        foreach (var attr in attrs.SelectMany(a => a.Attributes))
        {
            var name = attr.Name.ToString();
            var isRoute = name.Contains("Route", StringComparison.Ordinal)
                          || name.Contains("HttpGet", StringComparison.Ordinal)
                          || name.Contains("HttpPost", StringComparison.Ordinal)
                          || name.Contains("HttpPatch", StringComparison.Ordinal)
                          || name.Contains("HttpPut", StringComparison.Ordinal)
                          || name.Contains("HttpDelete", StringComparison.Ordinal);
            if (!isRoute)
                continue;

            var expr = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                return literal.Token.ValueText;
        }

        return null;
    }

    private static string CombineRoute(string classRoute, string methodRoute)
    {
        var left = classRoute.Trim('/');
        var right = methodRoute.Trim('/');
        return "/" + string.Join("/", new[] { left, right }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static bool HasAttribute(SyntaxList<AttributeListSyntax> attrs, string attrName)
        => attrs.SelectMany(a => a.Attributes).Any(a => a.Name.ToString().Contains(attrName, StringComparison.Ordinal));

    private static string GetJsonPropertyName(SyntaxList<AttributeListSyntax> attrs, string fallback)
    {
        foreach (var attr in attrs.SelectMany(a => a.Attributes))
        {
            if (!attr.Name.ToString().Contains("JsonPropertyName", StringComparison.Ordinal))
                continue;
            var expr = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
            if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
                return lit.Token.ValueText;
        }

        return ToCamelCase(fallback);
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        if (value.Length == 1)
            return value.ToLowerInvariant();
        return char.ToLowerInvariant(value[0]) + value[1..];
    }

    private static bool IsNullable(TypeSyntax? type) => type?.ToString().EndsWith("?", StringComparison.Ordinal) == true;

    private static string NormalizeTypeName(TypeSyntax? type)
    {
        if (type is null)
            return "unknown";
        var full = type.ToString();
        return full.Split('.').Last();
    }

    private static IEnumerable<string> ParseRouteParameters(string route)
    {
        var matches = System.Text.RegularExpressions.Regex.Matches(route, "\\{([^}:]+)(:[^}]+)?\\}");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var name = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(name))
                yield return ToCamelCase(name);
        }
    }

    private static string BuildRouteExpression(string route, IReadOnlyCollection<string> parameters)
    {
        if (parameters.Count == 0)
            return $"'{route}'";

        var template = route;
        foreach (var parameter in parameters)
        {
            template = System.Text.RegularExpressions.Regex.Replace(
                template,
                $"\\{{{parameter}(:[^}}]+)?\\}}",
                $"${{{parameter}}}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return $"`{template}`";
    }

    private static string MapTypeToTypeScript(string csharpType, bool nullable)
    {
        var type = csharpType.Trim();
        var isNullable = nullable;
        if (type.EndsWith("?", StringComparison.Ordinal))
        {
            isNullable = true;
            type = type[..^1];
        }

        string mapped;
        if (type.EndsWith("[]", StringComparison.Ordinal))
        {
            mapped = $"{MapTypeToTypeScript(type[..^2], false)}[]";
        }
        else if (IsGeneric(type, out var genericName, out var genericArg) &&
                 genericName is "List" or "IReadOnlyList" or "IEnumerable" or "ICollection")
        {
            mapped = $"{MapTypeToTypeScript(genericArg!, false)}[]";
        }
        else
        {
            mapped = type switch
            {
                "int" or "long" or "float" or "double" or "decimal" => "number",
                "string" => "string",
                "bool" => "boolean",
                "DateTime" or "DateTimeOffset" => "string",
                "Guid" => "string",
                "object" => "unknown",
                _ => type
            };
        }

        return isNullable ? $"{mapped} | null" : mapped;
    }

    private static bool IsGeneric(string value, out string? name, out string? arg)
    {
        var start = value.IndexOf('<');
        var end = value.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            name = value[..start];
            arg = value[(start + 1)..end];
            return true;
        }

        name = null;
        arg = null;
        return false;
    }

    private static void WriteIfChanged(string path, string content)
    {
        if (File.Exists(path))
        {
            var existing = File.ReadAllText(path);
            if (string.Equals(existing, content, StringComparison.Ordinal))
                return;
        }

        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private sealed record DtoModel(string Name, IReadOnlyList<DtoProperty> Properties)
    {
        public DtoModel(string name, IReadOnlyList<string> enumValues) : this(name, Array.Empty<DtoProperty>())
        {
            EnumValues = enumValues;
        }

        public IReadOnlyList<string> EnumValues { get; } = Array.Empty<string>();
    }

    private sealed record DtoProperty(string Name, string CSharpType, bool Nullable);
    private sealed record ApiEndpoint(string HttpMethod, string Route, string MethodName, string? RequestTypeName, string? ResponseTypeName);
}
