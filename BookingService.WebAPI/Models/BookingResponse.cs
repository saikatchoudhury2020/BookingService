using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class BookingResponse
    {
        public string data { get; set; }
        public int errorType { get; set; } // 0 no, 1 - validation error, 
    }

    public class ApiResponse
    {
        public string data { get; set; }
        public int errorType { get; set; }

        public string message { get; set; }
 
    }
}
