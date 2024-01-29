using Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransfierObject
{
    public sealed record CreateProductDto
    {
        [Required(ErrorMessage = "product name is a required field.")]
        [MaxLength(150, ErrorMessage = "Maximum length for the Name is 150 characters.")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "product description is a required field.")]
        [MaxLength(250, ErrorMessage = "Maximum length for the description is 250 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "product price is a required field.")]
        [Range(1, int.MaxValue, ErrorMessage = "min value is 1 and max value is ... value")]
        public decimal Price { get; set; }
        public string? PictureUrl { get; set; }
        public ProductBrand? ProductBrand { get; set; } 
        public ProductType? ProductType { get; set; }
        public int? ClientId { get; set; }
        //public Client Client { get; set; } 
    }
}
