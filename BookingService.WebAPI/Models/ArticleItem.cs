using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingService.WebAPI.Models
{
    public class ArticleItem
    {
        
        public int MenuId { get; set; }
        public int ArticleId { get; set; }
        public string ArticleName { get; set; }
        public string Ingrese { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public string PublishDate { get; set; }
        public int PaId { get; set; }
        public decimal Price1 { get; set; }
        public decimal Price2 { get; set; }
        public decimal CPrice1 { get; set; }
        public decimal CPrice2 { get; set; }

        public string Unit { get; set; }
        public string UnitName { get; set; }

        public string Message { get; set; }
    }
}