using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Models
{
    public class CatalogBrand
    {
        public int Id { get; set; }

        [Required]
        public string Brand { get; set; }
    }
}
