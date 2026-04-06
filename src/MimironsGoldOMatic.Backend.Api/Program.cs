using MimironsGoldOMatic.Backend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMgmBackend(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

await using (var scope = app.Services.CreateAsyncScope())
{
    var store = scope.ServiceProvider.GetRequiredService<Marten.IDocumentStore>();
    await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
