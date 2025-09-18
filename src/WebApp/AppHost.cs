var builder = DistributedApplication.CreateBuilder(args);

var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api");

var orderingApi = builder.AddProject<Projects.Ordering_API>("ordering-api");

builder.Build().Run();
