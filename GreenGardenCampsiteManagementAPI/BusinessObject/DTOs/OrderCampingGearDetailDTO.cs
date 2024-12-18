﻿namespace BusinessObject.DTOs
{
    public class OrderCampingGearDetailDTO
    {
        public int GearId { get; set; }
        public int OrderId { get; set; }

        public string Name { get; set; }

        public string? ImgUrl { get; set; }
        public int? Quantity { get; set; }
        public decimal Price { get; set; }

        public string? Description { get; set; }
    }
    public class OrderCampingGearAddDTO
    {
        public int GearId { get; set; }
        public int OrderId { get; set; }



        public int? Quantity { get; set; }

    }
    public class OrderCampingGearByUsageDateDTO
    {
        public int GearId { get; set; }
        public int? Quantity { get; set; }
    }
}
