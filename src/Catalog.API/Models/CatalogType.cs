using System.ComponentModel.DataAnnotations;

namespace Catalog.API.Models
{
    public class CatalogType
    {
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }
    }
}
