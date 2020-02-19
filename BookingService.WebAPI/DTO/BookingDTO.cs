using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.DTO
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OrderList
    {
        public string ArticleNo { get; set; }
        public string Text { get; set; }

        public decimal Qty { get; set; }

        public decimal? Sum { get; set; }
    }

    public class BookingOrderList
    {
        public List<OrderList> orderList { get; set; }
        public int bookingID { get; set; }
    }
}