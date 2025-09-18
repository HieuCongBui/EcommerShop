using Catalog.API.Apis;
using eServiceDefault;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddApplicationServices();
var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();
app.MapDefaultEndpoints();

app.NewVersionedApi("catalog")
    .MapCatalogApiV1();

app.Run();
