﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTOs
{
    public class OrderComboDetailDTO
    {
        public int ComboId { get; set; }
        public string Name { get; set; }
        
        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string? Description { get; set; }
    }
    public class OrderComboAddDTO
    {
        public int ComboId { get; set; }
        public int OrderId { get; set; }
        public int? Quantity { get; set; }
        public string? Description { get; set; }
    }
}
