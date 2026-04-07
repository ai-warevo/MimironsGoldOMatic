# Checks

- [x] Merge Domain + Services → Application; remove old projects from disk
- [x] Rename DataAccess → Infrastructure.Persistence; drop unused Domain project reference from persistence csproj
- [x] Fix Infrastructure DI (duplicate HelixSubscriberVerifier removed; MediatR comment; usings)
- [x] Application GlobalUsings: add `global using MediatR`
- [x] Update test project references; slnx; Api TS gen paths
- [x] Remove Class1 stubs and Application `.gitkeep` files
- [x] `dotnet build src/MimironsGoldOMatic.slnx`
- [x] `dotnet test src/MimironsGoldOMatic.slnx`
