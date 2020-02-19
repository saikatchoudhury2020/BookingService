using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.DTO;


namespace WebAngularRAC.Controllers
{
    public class AllBookingListController : ApiController
    {
        //DatabaseContext _DatabaseContext;
        //public AllBookingListController(DatabaseContext databasecontext)
        //{
        //    _DatabaseContext = databasecontext;
        //}

        // GET: api/values

        public IEnumerable<BookingDTO> Get()
        {
            try
            {
                //var ListofBooking = (from book in _DatabaseContext.BookingTB
                //                     select new BookingTB {
                //                         Amount = book.Amount,
                //                         BankName = book.BankName,
                //                         BookingID = book.BookingID,
                //                         Carname = book.Carname,
                //                         Contact_No = book.Contact_No,
                //                         CreatedOn = book.CreatedOn,
                //                         C_Id = book.C_Id,
                //                         D_address = book.D_address,
                //                         Email_Id = book.Email_Id,
                //                         FromDate = book.FromDate,
                //                         ModelName = book.ModelName,
                //                         Name = book.Name,
                //                         PaymentStatus = book.PaymentStatus,
                //                         Status = book.PaymentStatus == "D" ? "Completed" : book.PaymentStatus == "C" ? "Cancel" : book.PaymentStatus == "P" ? "Pending" : "Unknown"
                //                     }).ToList();

               // return ListofBooking.ToArray();

                List<BookingDTO> booking = new List<BookingDTO>
                {
                    new BookingDTO { Id = 1, Name = "meeing room 1" },
                    new BookingDTO { Id = 2, Name = "meeing room 2" },
                    new BookingDTO { Id = 3, Name = "meeing room 3" },
                    new BookingDTO { Id = 4, Name = "meeing room 4" }
                };

                return booking.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
