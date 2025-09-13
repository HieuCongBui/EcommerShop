var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Catalog_API>("catalog-api");

builder.AddProject<Projects.Identity_API>("identity-api");

builder.Build().Run();
