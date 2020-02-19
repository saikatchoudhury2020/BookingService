using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace BookingService.WebAPI.Models
{
    public class OrderTb
    {   
        public int OrderHeadId { get; set; }
        public List<Food> OrderLines { get; set; }
    }
}