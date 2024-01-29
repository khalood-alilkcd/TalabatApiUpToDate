﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public class Product : BaseEntity
    {
        public string? Name{ get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? PictureUrl { get; set; }
        public int ProductBrandId { get; set; }
        public ProductBrand? ProductBrand{ get; set; }
        public int ProductTypeId { get; set; }
        public ProductType? ProductType { get; set; }
        public int ClientId{ get; set; }
        public Client? Client{ get; set; }
    }
}