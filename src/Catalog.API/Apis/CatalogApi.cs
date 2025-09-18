using Catalog.API.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Catalog.API.Apis
{
    public static class CatalogApi
    {
        public static IEndpointRouteBuilder MapCatalogApiV1(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("api/catalog").HasApiVersion(1.0);

            app.MapGet("/items", GetAllItems);
            return app;
        }
        public static async Task<Results<Ok<PaginatedItems<CatalogItem>>, BadRequest<string>>> GetAllItems(
            [AsParameters] PaginationRequest paginationRequest,
            [AsParameters] CatalogServices services)
        {
            var pageSize = paginationRequest.pageSize;
            var pageIndex = paginationRequest.pageIndex;

            var totalItems = await services.Context.CatalogItems.LongCountAsync();

            var itemsOnPage = await services.Context.CatalogItems
                .OrderBy(c => c.Name)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return TypedResults.Ok(new PaginatedItems<CatalogItem> (pageIndex, pageSize, totalItems, itemsOnPage));
        }
    }
}
