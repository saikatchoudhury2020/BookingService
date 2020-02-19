using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.DTO;
using WebAPI.Models;
using WebAPI.Filters;
using System.Data.SqlClient;
using System.Data;
using BookingService.WebAPI.Models;
using System.Transactions;
using System.Data.Entity;
using DMBase.Core;
using System.Configuration;
using Digimaker.Schemas.Web;
using Newtonsoft.Json;
using BookingService.WebAPI.Enums;
using logWriter;

namespace WebAPI.Controllers
{

    public class BookingController : ApiController
    {
        [HttpGet]
        [ActionName("SavePreference")]
        public int SavePreference(string key, string value)
        {
            var userId = Digimaker.Directory.Person.Current.PersonID;

            using (TransactionScope scope = new TransactionScope())
            {
                var entities = new BraathenEiendomEntities();
                var preference = entities.Preferences.Where(w => w.UserId == userId).FirstOrDefault();
                if (preference == null)
                {
                    var _perference = new Preference();
                    _perference.UserId = userId;

                    var data = new Dictionary<string, string>();
                    data.Add(key, value);
                    _perference.Settings = JsonConvert.SerializeObject(data);
                    entities.Preferences.Add(_perference);
                    entities.SaveChanges();
                }
                else
                {
                    var data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(preference.Settings);
                    if (data[key] != null)
                    {
                        data[key] = value;
                    }
                    else
                    {
                        data.Add(key, value);
                    }
                    preference.Settings = JsonConvert.SerializeObject(data);
                    entities.SaveChanges();
                }
                scope.Complete();
            }
            return 1;
        }

        public int RemovePreference(string key)
        {
            return this.SavePreference(key, "");
        }

        [HttpGet]
        [ActionName("GetPreferences")]
        public Dictionary<string, string> GetPreferences(string keys = "") //keys, separated by ,
        {
            var userId = Digimaker.Directory.Person.Current.PersonID;
            var entities = new BraathenEiendomEntities();
            var list = entities.Preferences.Where(w => w.UserId == userId).ToList();
            var result = new Dictionary<string, string>();
            if (list.Count > 0)
            {
                var perference = list[0];
                var data = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(perference.Settings);

                string[] keyArray = keys.Split(',');
                foreach (var item in data)
                {
                    if (keys == "" || keyArray.Contains(item.Key))
                    {
                        result.Add(item.Key, item.Value.ToString());
                    }
                }

            }
            return result;
        }

        //[HttpPost]
        //public BookingResponse Post([FromBody]BookingTB bookingtb)
        //{
        //    string message = "";
        //    BookingResponse response =new BookingResponse();

        //    var days = Math.Round((Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).TotalDays);
        //    var hours = (Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).Hours;
        //    var currenttime = DateTime.Now;
        //    var booktime = Convert.ToDateTime(bookingtb.FromDate);

        //    if (!ModelState.IsValid)
        //    {

        //        var errorMessage = string.Join(" | ", ModelState.Values
        //                    .SelectMany(v => v.Errors)
        //                    .Select(e => e.ErrorMessage));
        //        response.data = errorMessage;
        //        response.errorType =(int) ErrorType.Error;
        //        return response;
        //    }
        //    else
        //    { 
        //        BookingDetail _bookingDetail = new BookingDetail();
        //        BookingServiceList _foodDetail;
        //        using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
        //        {
        //            using (var dbContextTransaction = entity.Database.BeginTransaction())
        //            {
        //                try
        //                {

        //                    var fdate = Convert.ToDateTime(bookingtb.FromDate);
        //                    TimeSpan fTime = TimeSpan.Parse(bookingtb.FromTimer);
        //                    var fromdate = fdate.Add(fTime);
        //                    var tdate = Convert.ToDateTime(bookingtb.ToDate);
        //                    TimeSpan tTime = TimeSpan.Parse(bookingtb.ToTimer);
        //                    var todate = tdate.Add(tTime);
        //                    // var validate = DateTime.Compare(booktime, currenttime);
        //                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                    {
        //                        var followDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                        if (followDate > fdate)
        //                        {

        //                            response.data = "Bekreftelse should be before Booking Date";
        //                            response.errorType = (int)ErrorType.Error;
        //                            return response;
        //                        }
        //                    }
        //                    if (todate < fromdate)
        //                    {
        //                        response.data = "Todate ikke mindre enn FromDate";
        //                        response.errorType = (int)ErrorType.Error;
        //                        return response;
        //                    }
        //                   int personcapacity= new  ArticlesController().GetMemberForService(bookingtb.MeetingRoomId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_AddOnMemberFormID"].ToString()));
        //                    if (personcapacity != 0)
        //                    {
        //                        if (bookingtb.NoOfPeople > personcapacity) {
        //                            response.data = "Maksimal kapasitet til Person er"+ personcapacity;
        //                            response.errorType = (int)ErrorType.Error;
        //                            return response;
        //                        }
        //                    }

        //                    var bookingPriceDetail = getBookingPriceDetail(bookingtb.MeetingRoomId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), bookingtb.FromDate.ToString(), bookingtb.ToDate.ToString(), bookingtb.FromTimer, bookingtb.ToTimer);
        //                    //else if (validate<0) {
        //                    //     message = "Booking Not Allowed" ;
        //                    // }
        //                    string articleName = entity.Articles.Where(w => w.MainID == bookingtb.MeetingRoomId && w.Status == 0).Select(s => s.Headline).FirstOrDefault().ToString();
        //                    if (bookingtb.BookingID != 0)
        //                    {
        //                        var bookedEvent = entity.BookingDetails.Where(w => w.BookingID == bookingtb.BookingID).FirstOrDefault();
        //                        if (bookedEvent != null)
        //                        {
        //                            bool isConfirmed = false;
        //                            bool isNew = false;
        //                            bool notConfirmed = false;
        //                            //using (TransactionScope scope = new TransactionScope())
        //                            //{
        //                            bookedEvent.BuildingID = bookingtb.PropertyId;
        //                            bookedEvent.CreatedOn = DateTime.Now;
        //                            bookedEvent.NoOfPeople = bookingtb.NoOfPeople;
        //                            bookedEvent.UserID = bookingtb.UserID;
        //                            bookedEvent.BookingName = bookingtb.nameOfbook;
        //                            bookedEvent.ServiceID = bookingtb.MeetingRoomId;
        //                            bookedEvent.IsFoodOrder = bookingtb.IsFoodOrder;
        //                            bookedEvent.FromDate = fromdate;
        //                            bookedEvent.ToDate = todate;
        //                            bookedEvent.ServiceType = bookingtb.PropertyServiceId;

        //                            bookedEvent.SendMessageType = bookingtb.SendMessageType;
        //                            bookedEvent.Ordering = bookingtb.BookOrderName;
        //                            bookedEvent.Customer = bookingtb.CompanyPayer;
        //                            bookedEvent.Note = bookingtb.Note;
        //                            if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                            {
        //                                bookedEvent.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                            }
        //                            else
        //                            {
        //                                bookedEvent.FollowUpDate = null;
        //                            }
        //                            if (bookingtb.IsConfirmed)
        //                            {
        //                                if (bookedEvent.Status == 0)
        //                                {
        //                                    isConfirmed = true;
        //                                    bookedEvent.Status = 1;
        //                                }
        //                                else
        //                                {
        //                                    isNew = true;
        //                                    bookedEvent.Status = 1;
        //                                }





        //                            }
        //                            else
        //                            {
        //                                notConfirmed = true;
        //                                bookedEvent.Status = 0;
        //                            }
        //                            bookedEvent.Quantity = bookingPriceDetail.Quantity;
        //                            bookedEvent.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();

        //                            if (isNew)
        //                            {
        //                                Orderhead bookedOrder = new Orderhead();




        //                                bookedOrder.Orderdate = DateTime.Now;
        //                                bookedOrder.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                bookedOrder.Status = "Confirmed";
        //                                bookedOrder.VAT = 0;
        //                                bookedOrder.CustomerName = bookingtb.nameOfbook;
        //                                bookedOrder.CustomerNo = bookingtb.CompanyPayer.ToString();
        //                                bookedOrder.EmailAddress = "";
        //                                bookedOrder.ERPClient = "";
        //                                bookedOrder.YourReference = getUserName(bookingtb.UserID);
        //                                bookedOrder.OurReference = Digimaker.User.Identity.DisplayName;
        //                                bookedOrder.NoOFPeople = bookingtb.NoOfPeople;
        //                                bookedOrder.FromDate = fromdate;
        //                                bookedOrder.Todate = todate;
        //                                bookedOrder.IsClean = false;
        //                                bookedOrder.IsDeliver = false;
        //                                bookedOrder.ISVismaOrder = false;
        //                                bookedOrder.bookingId = bookedEvent.BookingID;
        //                                entity.Orderheads.Add(bookedOrder);


        //                                entity.SaveChanges();
        //                                // orderList.Add(order);
        //                                if (bookingtb.foods.Count > 0)
        //                                {

        //                                    List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                    foreach (var item in bookingtb.foods)
        //                                    {
        //                                        _foodDetail = new BookingServiceList();
        //                                        if (item.OrderHeadId == 0)
        //                                        {


        //                                            var existOrderline = bookedEvent.BookingServiceLists.Where(x => x.ID == item.OrderListId).FirstOrDefault();
        //                                            if (existOrderline == null)
        //                                            {
        //                                                _foodDetail.FoodID = item.FoodId;
        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.ServiceName = item.ServiceText;
        //                                                _foodDetail.OrderHeadId = bookedOrder.OrderNo;
        //                                                _foodDetail.BookingID = bookedEvent.BookingID;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsMainService = false;
        //                                                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                                _foodDetail.UnitText = PriceUnit.UnitText;
        //                                                _foodDetail.IsVat = item.IsVatApply;
        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.VATPercent = 0;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = item.ArticleId;
        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = "0";
        //                                                }

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;
        //                                                flist.Add(_foodDetail);
        //                                            }

        //                                        }



        //                                    }
        //                                     entity.BookingServiceLists.AddRange(flist);

        //                                    entity.SaveChanges();
        //                                }
        //                            }
        //                            if (isConfirmed)
        //                            {

        //                                Orderhead bookedOrder = new Orderhead();




        //                                bookedOrder.Orderdate = DateTime.Now;
        //                                bookedOrder.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                bookedOrder.Status = "Confirmed";
        //                                bookedOrder.VAT = 0;
        //                                bookedOrder.CustomerName = bookingtb.nameOfbook;
        //                                bookedOrder.CustomerNo = bookingtb.CompanyPayer.ToString();
        //                                bookedOrder.EmailAddress = "";
        //                                bookedOrder.ERPClient = "";
        //                                bookedOrder.YourReference = getUserName(bookingtb.UserID);
        //                                bookedOrder.OurReference = Digimaker.User.Identity.DisplayName;
        //                                bookedOrder.NoOFPeople = bookingtb.NoOfPeople;
        //                                bookedOrder.FromDate = fromdate;
        //                                bookedOrder.Todate = todate;
        //                                bookedOrder.IsClean = false;
        //                                bookedOrder.IsDeliver = false;
        //                                bookedOrder.ISVismaOrder = false;
        //                                bookedOrder.bookingId = bookedEvent.BookingID;
        //                                entity.Orderheads.Add(bookedOrder);


        //                                 entity.SaveChanges();
        //                                // orderList.Add(order);
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    bool isService = true;
        //                                    List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                    foreach (var item in bookingtb.foods)
        //                                    {
        //                                        _foodDetail = new BookingServiceList();
        //                                        if (item.OrderHeadId == 0)
        //                                        {
        //                                            int mainServiceID = 0;
        //                                            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                            {
        //                                                mainServiceID = Convert.ToInt32(item.MainServiceId);

        //                                                var existOrderline = bookedEvent.BookingServiceLists.Where(x => x.ID==item.OrderListId && x.Status == false).FirstOrDefault();
        //                                                if (existOrderline != null)
        //                                                {
        //                                                    existOrderline.OrderHeadId = bookedOrder.OrderNo;
        //                                                    existOrderline.IsKitchen = item.IsKitchenOrder;
        //                                                    existOrderline.Qty = item.Qty;
        //                                                    existOrderline.Status = false;
        //                                                    existOrderline.Sum = item.Sum;
        //                                                    existOrderline.Tekst = item.Tekst;
        //                                                    existOrderline.ServiceName = item.ServiceText;
        //                                                    existOrderline.Price = item.Price;
        //                                                    if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                    {
        //                                                        existOrderline.ArticleId = item.ArticleId;
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        existOrderline.ArticleId = "0";
        //                                                    }
        //                                                    entity.Entry(existOrderline).State = EntityState.Modified;
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = item.FoodId;
        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.ServiceName = item.ServiceText;
        //                                                _foodDetail.OrderHeadId = bookedOrder.OrderNo;
        //                                                _foodDetail.BookingID = bookedEvent.BookingID;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsMainService = false;
        //                                                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                                _foodDetail.UnitText = PriceUnit.UnitText;
        //                                                _foodDetail.IsVat = item.IsVatApply;
        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.VATPercent = 0;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = item.ArticleId;
        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = "0";
        //                                                }

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;
        //                                                flist.Add(_foodDetail);
        //                                            }



        //                                        }



        //                                    }
        //                                     entity.BookingServiceLists.AddRange(flist);

        //                                    entity.SaveChanges();
        //                                }


        //                            }
        //                            if(notConfirmed)
        //                            {
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    if (bookedEvent.BookingServiceLists.Count > 0)
        //                                    {
        //                                        foreach (var item in bookedEvent.BookingServiceLists)
        //                                        {
        //                                            item.Status = true;
        //                                        }
        //                                        //  entity.Entry(bookedEvent.BookingServiceLists).State = EntityState.Modified;
        //                                    }
        //                                    bool isService = true;
        //                                    foreach (var item in bookingtb.foods)
        //                                    {
        //                                        _foodDetail = new BookingServiceList();

        //                                        if (isService)
        //                                        {
        //                                            isService = false;
        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;


        //                                            _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;

        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";

        //                                            }
        //                                            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                            {
        //                                                _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = 0;
        //                                            }



        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;
        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
        //                                            // _foodDetail.
        //                                            _foodDetail.IsMainService = true;
        //                                            _foodDetail.OrderHeadId = 0;
        //                                            _foodDetail.IsVat = item.IsVatApply;

        //                                            _foodDetail.BookingID = bookedEvent.BookingID;
        //                                            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            _foodDetail.Price = item.Price;

        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.FoodID = item.FoodId;
        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;

        //                                            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                            _foodDetail.UnitText = PriceUnit.UnitText;
        //                                            _foodDetail.IsMainService = false;
        //                                            _foodDetail.OrderHeadId = 0;
        //                                            _foodDetail.IsVat = item.IsVatApply;
        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;
        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
        //                                            // _foodDetail.
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";
        //                                            }

        //                                            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            _foodDetail.Price = item.Price;
        //                                        }


        //                                        bookedEvent.BookingServiceLists.Add(_foodDetail);

        //                                    }

        //                                    entity.SaveChanges();

        //                                }
        //                                else
        //                                {
        //                                    if (bookedEvent.BookingServiceLists.Count > 0)
        //                                    {
        //                                        foreach (var item in bookedEvent.BookingServiceLists)
        //                                        {
        //                                            item.Status = true;
        //                                        }
        //                                    }
        //                                    //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
        //                                    entity.SaveChanges();                                            
        //                                }
        //                            }

        //                            var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
        //                          .Where(w => w.aa.MainID == bookedEvent.ServiceID).Select(s => s.aa).ToList();
        //                            if (relatedRooms != null)
        //                            {
        //                                //  string articleName = entity.Articles.Where(w => w.ArticleID == bookingtb.MeetingRoomId).Select(s => s.Headline).FirstOrDefault().ToString();
        //                                foreach (var i in relatedRooms)
        //                                {
        //                                    var subBooking = entity.BookingDetails.Where(w => w.ServiceID == i.RelatedMainID && w.MainBookingId == bookedEvent.BookingID).Select(s => s).FirstOrDefault();
        //                                    subBooking.BuildingID = bookingtb.PropertyId;
        //                                    subBooking.CreatedOn = DateTime.Now;
        //                                    subBooking.NoOfPeople = bookingtb.NoOfPeople;
        //                                    subBooking.UserID = bookingtb.UserID;
        //                                    subBooking.BookingName = articleName;
        //                                    subBooking.ServiceID = i.RelatedMainID;
        //                                    subBooking.IsFoodOrder = bookingtb.IsFoodOrder;
        //                                    subBooking.FromDate = Convert.ToDateTime(bookingtb.FromDate);
        //                                    subBooking.ToDate = Convert.ToDateTime(bookingtb.ToDate);
        //                                    subBooking.ServiceType = bookingtb.PropertyServiceId;
        //                                    subBooking.SendMessageType = bookingtb.SendMessageType;
        //                                    subBooking.Status = 99;
        //                                    subBooking.Ordering = bookingtb.BookOrderName;
        //                                    subBooking.Customer = bookingtb.CompanyPayer;
        //                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                                    {
        //                                        subBooking.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                    }
        //                                    else
        //                                    {
        //                                        subBooking.FollowUpDate = null;
        //                                    }
        //                                    subBooking.Quantity = bookingPriceDetail.Quantity;
        //                                    subBooking.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();


        //                                    //if (bookingtb.foods.Count > 0)
        //                                    //{
        //                                    //    if (subBooking.BookingServiceLists.Count > 0)
        //                                    //    {
        //                                    //        foreach (var item in subBooking.BookingServiceLists)
        //                                    //        {
        //                                    //            item.Status = true;
        //                                    //        }
        //                                    //        //entity.Entry(bookedEvent.FoodServices).State = EntityState.Modified;
        //                                    //    }
        //                                    //    bool isService = true;
        //                                    //    foreach (var item in bookingtb.foods)
        //                                    //    {

        //                                    //        _foodDetail = new BookingServiceList();

        //                                    //        if (isService)
        //                                    //        {
        //                                    //            _foodDetail.Qty = item.Qty;
        //                                    //            _foodDetail.Status = false;
        //                                    //            _foodDetail.Sum = item.Sum;
        //                                    //            _foodDetail.Tekst = item.Tekst;
        //                                    //            _foodDetail.IsKitchen = item.IsKitchenOrder;


        //                                    //            _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                    //            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = item.ArticleId;

        //                                    //            }
        //                                    //            else
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = "0";

        //                                    //            }
        //                                    //            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                    //            {
        //                                    //                _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                    //            }
        //                                    //            {
        //                                    //                _foodDetail.FoodID = 0;
        //                                    //            }

        //                                    //            _foodDetail.NetAmount = item.Sum;
        //                                    //            _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                    //            // _foodDetail.

        //                                    //            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                    //            _foodDetail.Price = item.Price;
        //                                    //            isService = false;
        //                                    //        }
        //                                    //        else
        //                                    //        {
        //                                    //            _foodDetail.FoodID = item.FoodId;
        //                                    //            _foodDetail.Qty = item.Qty;
        //                                    //            _foodDetail.Status = false;
        //                                    //            _foodDetail.Sum = item.Sum;
        //                                    //            _foodDetail.Tekst = item.Tekst;
        //                                    //            _foodDetail.IsKitchen = item.IsKitchenOrder;

        //                                    //            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                    //            _foodDetail.UnitText = PriceUnit.UnitText;

        //                                    //            _foodDetail.NetAmount = item.Sum;
        //                                    //            _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                    //            // _foodDetail.
        //                                    //            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = item.ArticleId;
        //                                    //            }
        //                                    //            else
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = "0";
        //                                    //            }

        //                                    //            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                    //            _foodDetail.Price = item.Price;
        //                                    //        }


        //                                    //        subBooking.BookingServiceLists.Add(_foodDetail);

        //                                    //    }
        //                                    //    entity.SaveChanges();

        //                                    //}
        //                                    //else
        //                                    //{
        //                                    //    if (subBooking.BookingServiceLists.Count > 0)
        //                                    //    {
        //                                    //        foreach (var item in subBooking.BookingServiceLists)
        //                                    //        {
        //                                    //            item.Status = true;
        //                                    //        }
        //                                    //    }
        //                                    //    //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
        //                                    //    entity.SaveChanges();
        //                                    //}
        //                                }
        //                            }

        //                            // scope.Complete();
        //                            message = "Booking Succesfully Updated";
        //                            // }
        //                            this.createUpdateBookingMessage( entity, bookedEvent.BookingID, bookingtb );
        //                            //send message
        //                            if (bookedEvent.UserID != 0)
        //                            {
        //                                var user = DMBase.Core.ContentModel.FetchPersonById(bookedEvent.UserID);
        //                                SendBookingMessage(bookedEvent, user, "update");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var IsalreadyBooked = (from book in entity.BookingDetails
        //                                               where
        //                                                book.FromDate < todate && fromdate < book.ToDate
        //                                                && book.ServiceID == bookingtb.MeetingRoomId
        //                                               select book.BookingID).Count();
        //                        if (IsalreadyBooked > 0)
        //                        {



        //                            response.data = "Allerede booket!!";
        //                            response.errorType = (int)ErrorType.Error;
        //                            return response;
        //                        }
        //                        else
        //                        {
        //                            //using (TransactionScope scope = new TransactionScope())
        //                            //{
        //                            bool isConfirmed = false;
        //                            _bookingDetail.BuildingID = bookingtb.PropertyId;
        //                            _bookingDetail.CreatedOn = DateTime.Now;
        //                            _bookingDetail.NoOfPeople = bookingtb.NoOfPeople;
        //                            _bookingDetail.UserID = bookingtb.UserID;


        //                            _bookingDetail.BookingName = bookingtb.nameOfbook;
        //                            _bookingDetail.ServiceID = bookingtb.MeetingRoomId;
        //                            _bookingDetail.IsFoodOrder = bookingtb.IsFoodOrder;
        //                            _bookingDetail.FromDate = fromdate;
        //                            _bookingDetail.ToDate = todate;
        //                            _bookingDetail.ServiceType = bookingtb.PropertyServiceId;
        //                            _bookingDetail.SendMessageType = bookingtb.SendMessageType;
        //                            _bookingDetail.Ordering = bookingtb.BookOrderName;
        //                            _bookingDetail.Customer = bookingtb.CompanyPayer;
        //                            _bookingDetail.Note = bookingtb.Note;
        //                            _bookingDetail.IsClean = false;
        //                            _bookingDetail.IsDeliver = false;
        //                            if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                            {
        //                                _bookingDetail.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                            }
        //                            else
        //                            {
        //                                _bookingDetail.FollowUpDate = null;
        //                            }
        //                            _bookingDetail.Quantity = bookingPriceDetail.Quantity;
        //                            _bookingDetail.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();

        //                            if (bookingtb.IsConfirmed)
        //                            {
        //                                _bookingDetail.Status = 1;
        //                                isConfirmed = true;

        //                            }
        //                            else
        //                            {
        //                                _bookingDetail.Status = 0;
        //                            }
        //                            entity.BookingDetails.Add(_bookingDetail);
        //                            entity.SaveChanges();

        //                            this.createUpdateBookingMessage( entity, _bookingDetail.BookingID, bookingtb );




        //                            if (isConfirmed)
        //                            {

        //                                Orderhead bookedOrder = new Orderhead();




        //                                bookedOrder.Orderdate = DateTime.Now;
        //                                bookedOrder.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                bookedOrder.Status = "Confirmed";
        //                                bookedOrder.VAT = 0;
        //                                bookedOrder.CustomerName = bookingtb.nameOfbook;
        //                                bookedOrder.CustomerNo = bookingtb.UserID.ToString();
        //                                bookedOrder.EmailAddress = "";
        //                                bookedOrder.ERPClient = "";
        //                                bookedOrder.YourReference = getUserName(bookingtb.UserID);
        //                                bookedOrder.OurReference = Digimaker.User.Identity.DisplayName;
        //                                bookedOrder.NoOFPeople = bookingtb.NoOfPeople;
        //                                bookedOrder.FromDate = fromdate;
        //                                bookedOrder.Todate = todate;
        //                                bookedOrder.IsClean = false;
        //                                bookedOrder.IsDeliver = false;
        //                                bookedOrder.ISVismaOrder = false;
        //                                bookedOrder.bookingId = _bookingDetail.BookingID;
        //                                entity.Orderheads.Add(bookedOrder);


        //                                 entity.SaveChanges();
        //                                // orderList.Add(order);
        //                               // List<BookingServiceList> orderlines = new List<BookingServiceList>();
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    bool isService = true;
        //                                    List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                    foreach (var item in bookingtb.foods)
        //                                    {
        //                                        _foodDetail = new BookingServiceList();
        //                                        if (isService)
        //                                        {

        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                            _foodDetail.IsVat = item.IsVatApply;
        //                                            _foodDetail.OrderHeadId = bookedOrder.OrderNo;
        //                                            _foodDetail.BookingID = _bookingDetail.BookingID;
        //                                            _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;

        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";

        //                                            }
        //                                            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                            {
        //                                                _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = 0;
        //                                            }


        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;
        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                            _foodDetail.Price = item.Price;
        //                                            _foodDetail.IsMainService = true;
        //                                            isService = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.FoodID = item.FoodId;
        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.OrderHeadId = bookedOrder.OrderNo;
        //                                            _foodDetail.BookingID = _bookingDetail.BookingID;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                            _foodDetail.IsMainService = false;
        //                                            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                            _foodDetail.UnitText = PriceUnit.UnitText;
        //                                            _foodDetail.IsVat = item.IsVatApply;
        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;
        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";
        //                                            }

        //                                            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            _foodDetail.Price = item.Price;
        //                                        }

        //                                        flist.Add(_foodDetail);

        //                                    }
        //                                    entity.BookingServiceLists.AddRange(flist);
        //                                    //entity.SaveChanges();
        //                                }

        //                                entity.SaveChanges();
        //                            }
        //                            else
        //                            {
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    bool isService = true;
        //                                    List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                    foreach (var item in bookingtb.foods)
        //                                    {
        //                                        _foodDetail = new BookingServiceList();
        //                                        if (isService)
        //                                        {

        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.OrderHeadId =0;
        //                                            _foodDetail.BookingID= _bookingDetail.BookingID;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                            _foodDetail.IsVat = item.IsVatApply;

        //                                            _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;

        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";

        //                                            }
        //                                            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                            {
        //                                                _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = 0;
        //                                            }


        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;

        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                            _foodDetail.Price = item.Price;
        //                                            _foodDetail.IsMainService = true;
        //                                            isService = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.FoodID = item.FoodId;
        //                                            _foodDetail.Qty = item.Qty;
        //                                            _foodDetail.Status = false;
        //                                            _foodDetail.Sum = item.Sum;
        //                                            _foodDetail.OrderHeadId = 0;
        //                                            _foodDetail.Tekst = item.Tekst;
        //                                            _foodDetail.ServiceName = item.ServiceText;
        //                                            _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                            _foodDetail.IsMainService = false;
        //                                            _foodDetail.BookingID = _bookingDetail.BookingID;
        //                                            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                            _foodDetail.UnitText = PriceUnit.UnitText;
        //                                            _foodDetail.IsVat = item.IsVatApply;
        //                                            _foodDetail.NetAmount = item.Sum;
        //                                            _foodDetail.VATPercent = 0;
        //                                            _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                _foodDetail.ArticleId = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.ArticleId = "0";
        //                                            }

        //                                            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            _foodDetail.Price = item.Price;
        //                                        }

        //                                        flist.Add(_foodDetail);

        //                                    }
        //                                    entity.BookingServiceLists.AddRange(flist);
        //                                    entity.SaveChanges();
        //                                }
        //                            }




        //                            var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
        //                            .Where(w => w.aa.MainID == bookingtb.MeetingRoomId).Select(s => s.aa).ToList();
        //                            if (relatedRooms.Count > 0)
        //                            {
        //                                BookingDetail book;

        //                                foreach (var i in relatedRooms)
        //                                {
        //                                    book = new BookingDetail();
        //                                    book.BuildingID = bookingtb.PropertyId;
        //                                    book.CreatedOn = DateTime.Now;
        //                                    book.NoOfPeople = bookingtb.NoOfPeople;
        //                                    book.UserID = bookingtb.UserID;


        //                                    book.BookingName = articleName;
        //                                    book.ServiceID = i.RelatedMainID;
        //                                    book.IsFoodOrder = bookingtb.IsFoodOrder;
        //                                    book.FromDate = fromdate;
        //                                    book.ToDate = todate;
        //                                    book.ServiceType = bookingtb.PropertyServiceId;
        //                                    book.SendMessageType = bookingtb.SendMessageType;
        //                                    book.Status = 99;
        //                                    book.Quantity = bookingPriceDetail.Quantity;


        //                                    book.MainBookingId = _bookingDetail.BookingID;
        //                                    book.Ordering = bookingtb.BookOrderName;
        //                                    book.Customer = bookingtb.CompanyPayer;
        //                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                                    {
        //                                        book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                    }
        //                                    else
        //                                    {
        //                                        book.FollowUpDate = null;
        //                                    }
        //                                    // book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                    //if (bookingtb.foods.Count > 0)
        //                                    //{
        //                                    //    bool isService = true;
        //                                    //    List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                    //    foreach (var item in bookingtb.foods)
        //                                    //    {
        //                                    //        _foodDetail = new BookingServiceList();
        //                                    //        if (isService)
        //                                    //        {

        //                                    //            _foodDetail.Qty = item.Qty;
        //                                    //            _foodDetail.Status = false;
        //                                    //            _foodDetail.Sum = item.Sum;
        //                                    //            _foodDetail.Tekst = item.Tekst;
        //                                    //            _foodDetail.IsKitchen = item.IsKitchenOrder;


        //                                    //            _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                    //            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = item.ArticleId;

        //                                    //            }
        //                                    //            else
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = "0";

        //                                    //            }
        //                                    //            if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                    //            {
        //                                    //                _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                    //            }
        //                                    //            {
        //                                    //                _foodDetail.FoodID = 0;
        //                                    //            }


        //                                    //            _foodDetail.NetAmount = item.Sum;
        //                                    //            _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                    //            _foodDetail.IsVat = item.IsVatApply;

        //                                    //            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                    //            _foodDetail.Price = item.Price;
        //                                    //            isService = false;
        //                                    //        }
        //                                    //        else
        //                                    //        {
        //                                    //            _foodDetail.FoodID = item.FoodId;
        //                                    //            _foodDetail.Qty = item.Qty;
        //                                    //            _foodDetail.Status = false;
        //                                    //            _foodDetail.Sum = item.Sum;
        //                                    //            _foodDetail.Tekst = item.Tekst;
        //                                    //            _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                    //            _foodDetail.IsVat = item.IsVatApply;
        //                                    //            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                    //            _foodDetail.UnitText = PriceUnit.UnitText;

        //                                    //            _foodDetail.NetAmount = item.Sum;
        //                                    //            _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                    //            // _foodDetail.
        //                                    //            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = item.ArticleId;
        //                                    //            }
        //                                    //            else
        //                                    //            {
        //                                    //                _foodDetail.ArticleId = "0";
        //                                    //            }

        //                                    //            //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                    //            _foodDetail.Price = item.Price;




        //                                    //        }

        //                                    //        flist.Add(_foodDetail);

        //                                    //    }
        //                                    //    entity.BookingServiceLists.AddRange(flist);
        //                                    //    // entity.SaveChanges();
        //                                    //}

        //                                    book.IsClean = false;
        //                                    book.IsDeliver = false;
        //                                    book.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                    entity.BookingDetails.Add(book);
        //                                    entity.SaveChanges();
        //                                }

        //                            }


        //                            //    scope.Complete();
        //                            //}
        //                            //send message
        //                            if (_bookingDetail.UserID != 0)
        //                            {

        //                                var user = DMBase.Core.ContentModel.FetchPersonById(_bookingDetail.UserID);
        //                                SendBookingMessage(_bookingDetail, user, "new");
        //                            }



        //                            response.data = "Bestilling vellykket lagret!!";
        //                            response.errorType = (int)ErrorType.Success;

        //                        }


        //                    }





        //                    dbContextTransaction.Commit();
        //                }
        //                catch (System.Data.Entity.Validation.DbEntityValidationException e)
        //                {
        //                    foreach (var eve in e.EntityValidationErrors)
        //                    {
        //                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //                        foreach (var ve in eve.ValidationErrors)
        //                        {
        //                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
        //                                ve.PropertyName, ve.ErrorMessage);
        //                        }
        //                    }
        //                    response.data = "Booking ikke lagret!!";
        //                    response.errorType = (int)ErrorType.Error;
        //                    dbContextTransaction.Rollback();
        //                    throw;
        //                    // throw new Exception(e.Message + ":" + e.StackTrace);
        //                }
        //                catch (Exception e)
        //                {
        //                    response.data = "Booking ikke lagret!!";
        //                    response.errorType = (int)ErrorType.Error;
        //                    dbContextTransaction.Rollback();
        //                }

        //            }

        //        }

        //    }





        //    return response;
        //}

        [HttpPost]
        
        public BookingResponse SaveOrderline([FromBody]OrderTb order)
        {  
            BookingResponse response = new BookingResponse();
            LogWriter log = new LogWriter();
            try
            {

                if (!ModelState.IsValid)
                {
                    var errorMessage = string.Join(" | ", ModelState.Values
                                               .SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage));
                    response.data = errorMessage;
                    response.errorType = (int)ErrorType.Error;
                    return response;
                }
                else
                {
                    using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                    {
                        var result = entity.Orderlines.Where(w => w.OrderHeadId == order.OrderHeadId && w.Status == false).ToList();

                        if (order.OrderLines.Count > 0)
                        {
                            Orderline _foodDetail;
                            bool isService = true;
                            List<Orderline> flist = new List<Orderline>();
                            foreach (var item in order.OrderLines)
                            {
                                var isAvailable = result.Where(x => x.ID == item.OrderListId && x.Status == false).Count();
                                if (isAvailable == 0)
                                {
                                    _foodDetail = new Orderline();
                                    _foodDetail.Qty = item.Qty;
                                    _foodDetail.Status = item.Status;
                                    _foodDetail.Sum = item.Sum;
                                    _foodDetail.Tekst = item.Tekst;

                                    _foodDetail.OrderHeadId = order.OrderHeadId;
                                    _foodDetail.BookingID = item.BookingID;
                                    _foodDetail.IsKitchen = item.IsKitchenOrder;
                                    _foodDetail.IsVat = item.IsVatApply;
                                    _foodDetail.CostPrice = item.CostPrice;
                                    _foodDetail.CostTotal = item.CostTotal;

                                    _foodDetail.NetAmount = item.Sum;
                                    _foodDetail.VATPercent = 0;
                                    _foodDetail.Time = item.Time;
                                    _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;



                                    _foodDetail.Price = item.Price;
                                    _foodDetail.IsMainService = false;

                                    _foodDetail.ArticleId = item.ArticleId;
                                    _foodDetail.FoodID = item.FoodId;







                                    flist.Add(_foodDetail);
                                }
                              



                            }

                            entity.Orderlines.AddRange(flist);
                            entity.SaveChanges();
                            response.data = "Bestilling vellykket lagret!!";
                            response.errorType = (int)ErrorType.Success;
                        }
                        return response;
                    }
                       
                }
            }
            catch (Exception e)
            {

                response.data = "Order ikke lagret!!";
                response.errorType = (int)ErrorType.Error;
                log.LogWrite("order Failed request:- " + e.Message);
                return response;
            }
               
        }
        [HttpPost]
        public BookingResponse Post([FromBody]BookingTB bookingtb)
        {
            string message = "";
            BookingResponse response = new BookingResponse();
            LogWriter log = new LogWriter();
            //  var days = Math.Round((Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).TotalDays);
            // var hours = (Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).Hours;
            var currenttime = DateTime.Now;
            var booktime = Convert.ToDateTime(bookingtb.FromDate);

            if (!ModelState.IsValid)
            {

                var errorMessage = string.Join(" | ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage));
                response.data = errorMessage;
                response.errorType = (int)ErrorType.Error;
                return response;
            }
            else
            {
                BookingDetail _bookingDetail = new BookingDetail();
                BookingServiceList _foodDetail;
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    using (var dbContextTransaction = entity.Database.BeginTransaction())
                    {
                        try
                        {
                            DateTime fromdate = DateTime.Now;
                            DateTime todate = DateTime.Now;
                            if (bookingtb.IsBooking)
                            {
                                var fdate = Convert.ToDateTime(bookingtb.FromDate);
                                TimeSpan fTime = TimeSpan.Parse(bookingtb.FromTimer);
                                fromdate = fdate.Add(fTime);
                                var tdate = Convert.ToDateTime(bookingtb.ToDate);
                                TimeSpan tTime = TimeSpan.Parse(bookingtb.ToTimer);
                                todate = tdate.Add(tTime);
                                // var validate = DateTime.Compare(booktime, currenttime);
                                if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                {
                                    var followDate = Convert.ToDateTime(bookingtb.FollowDate);
                                    if (followDate > fdate)
                                    {

                                        response.data = "Bekreftelse should be before Booking Date";
                                        response.errorType = (int)ErrorType.Error;
                                        return response;
                                    }
                                }
                                if (todate < fromdate)
                                {
                                    response.data = "Todate ikke mindre enn FromDate";
                                    response.errorType = (int)ErrorType.Error;
                                    return response;
                                }
                                int personcapacity = new ArticlesController().GetMemberForService(bookingtb.MeetingRoomId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_AddOnMemberFormID"].ToString()));
                                if (personcapacity != 0)
                                {
                                    if (bookingtb.NoOfPeople > personcapacity)
                                    {
                                        response.data = "Maksimal kapasitet til Person er" + new string(' ', 1) + personcapacity;
                                        response.errorType = (int)ErrorType.Error;
                                        return response;
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(bookingtb.FromDate))
                                {
                                    fromdate = Convert.ToDateTime(bookingtb.FromDate);
                                    todate = Convert.ToDateTime(bookingtb.FromDate);
                                }

                            }

                            BookingUnitPriceDetail bookingPriceDetail = new BookingUnitPriceDetail();
                            if (bookingtb.IsBooking)
                            {
                                bookingPriceDetail = getBookingPriceDetail(bookingtb.MeetingRoomId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), bookingtb.FromDate.ToString(), bookingtb.ToDate.ToString(), bookingtb.FromTimer, bookingtb.ToTimer,bookingtb.IsInternal,bookingtb.NoOfPeople);
                            }

                            //else if (validate<0) {
                            //     message = "Booking Not Allowed" ;
                            // }
                            //string articleName = entity.Articles.Where(w => w.MainID == bookingtb.MeetingRoomId && w.Status == 0).Select(s => s.Headline).FirstOrDefault().ToString();
                            if (bookingtb.BookingID != 0)
                            {
                                var bookedEvent = entity.BookingDetails.Where(w => w.BookingID == bookingtb.BookingID).FirstOrDefault();
                                if (bookedEvent != null)
                                {
                                    var OldBookingServiceId = bookedEvent.ServiceID;
                                    //using (TransactionScope scope = new TransactionScope())
                                    //{
                                    bookedEvent.BuildingID = bookingtb.PropertyId;
                                    bookedEvent.CreatedOn = DateTime.Now;
                                    bookedEvent.NoOfPeople = bookingtb.NoOfPeople;
                                    bookedEvent.UserID = bookingtb.UserID;
                                    bookedEvent.BookingName = bookingtb.nameOfbook;
                                    bookedEvent.ServiceID = bookingtb.MeetingRoomId;
                                    bookedEvent.IsFoodOrder = bookingtb.IsFoodOrder;
                                    bookedEvent.IsInternal = bookingtb.IsInternal;
                                    bookedEvent.IsMVA = bookingtb.IsMVA;
                                    bookedEvent.FromDate = fromdate;
                                    bookedEvent.ToDate = todate;
                                    bookedEvent.ServiceType = bookingtb.PropertyServiceId;

                                    bookedEvent.SendMessageType = bookingtb.SendMessageType;
                                    bookedEvent.Ordering = bookingtb.BookOrderName;
                                    bookedEvent.Customer = bookingtb.CompanyPayer;
                                    bookedEvent.Note = bookingtb.Note;
                                    bookedEvent.InvoMessage = bookingtb.InvoMessage;
                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                    {
                                        bookedEvent.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
                                    }
                                    else
                                    {
                                        bookedEvent.FollowUpDate = null;
                                    }
                                    //if (bookedEvent.Status == 1)
                                    //{
                                    //    isNew = true;
                                    //    bookedEvent.Status = 1;
                                    //}
                                    //else
                                    //{
                                    //    notConfirmed = true;
                                    //    bookedEvent.Status = 0;
                                    //}
                                    if (bookingPriceDetail != null)
                                    {
                                        bookedEvent.Quantity = bookingPriceDetail.Quantity;
                                    }
                                   
                                   
                                    bookedEvent.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
                                    var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
                                 .Where(w => w.aa.MainID == OldBookingServiceId).Select(s => s.aa).ToList();
                                    if (relatedRooms.Count > 0)
                                    {
                                        if (OldBookingServiceId == bookedEvent.ServiceID)
                                        {
                                            foreach (var i in relatedRooms)
                                            {
                                                var subBooking = entity.BookingDetails.Where(w => w.ServiceID == i.RelatedMainID && w.MainBookingId == bookedEvent.BookingID).Select(s => s).FirstOrDefault();
                                                if (subBooking != null)
                                                {
                                                    subBooking.BuildingID = bookingtb.PropertyId;
                                                    //  subBooking.CreatedOn = DateTime.Now;
                                                    subBooking.NoOfPeople = bookingtb.NoOfPeople;
                                                    subBooking.UserID = bookingtb.UserID;
                                                    subBooking.BookingName = bookingtb.nameOfbook;
                                                    subBooking.ServiceID = i.RelatedMainID;
                                                    subBooking.IsFoodOrder = bookingtb.IsFoodOrder;
                                                    subBooking.FromDate = fromdate;
                                                    subBooking.ToDate = todate;
                                                    subBooking.ServiceType = bookingtb.PropertyServiceId;
                                                    subBooking.SendMessageType = bookingtb.SendMessageType;
                                                    subBooking.Status = 99;
                                                    subBooking.Ordering = bookingtb.BookOrderName;
                                                    subBooking.Customer = bookingtb.CompanyPayer;
                                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                                    {
                                                        subBooking.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
                                                    }
                                                    else
                                                    {
                                                        subBooking.FollowUpDate = null;
                                                    }
                                                    subBooking.Quantity = bookingPriceDetail.Quantity;


                                                    subBooking.IsInternal = bookingtb.IsInternal;
                                                    subBooking.IsMVA = bookingtb.IsMVA;
                                                    subBooking.IsClean = bookedEvent.IsClean;
                                                    subBooking.IsDeliver = bookedEvent.IsDeliver;
                                                    subBooking.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();

                                                }

                                            }
                                        }
                                        else
                                        {
                                            foreach (var i in relatedRooms)
                                            {
                                                var subBooking = entity.BookingDetails.Where(w => w.ServiceID == i.RelatedMainID && w.MainBookingId == bookedEvent.BookingID).Select(s => s).FirstOrDefault();
                                                if (subBooking != null)
                                                {
                                                    entity.BookingDetails.Remove(subBooking);
                                                }






                                            }
                                        }
                                        //  string articleName = entity.Articles.Where(w => w.ArticleID == bookingtb.MeetingRoomId).Select(s => s.Headline).FirstOrDefault().ToString();

                                        entity.SaveChanges();
                                    }
                                    else
                                    {
                                        var relatedRoomsForNewService = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
                               .Where(w => w.aa.MainID == bookedEvent.ServiceID).Select(s => s.aa).ToList();
                                        if (relatedRoomsForNewService.Count > 0)
                                        {
                                            BookingDetail book;

                                            foreach (var i in relatedRoomsForNewService)
                                            {
                                                book = new BookingDetail();
                                                book.BuildingID = bookingtb.PropertyId;
                                                book.CreatedOn = DateTime.Now;
                                                book.NoOfPeople = bookingtb.NoOfPeople;
                                                book.UserID = bookingtb.UserID;
                                                book.IsInternal = bookingtb.IsInternal;
                                                book.IsMVA = bookingtb.IsMVA;
                                                book.BookingName = bookingtb.nameOfbook;
                                                book.ServiceID = i.RelatedMainID;
                                                book.IsFoodOrder = bookingtb.IsFoodOrder;
                                                book.FromDate = fromdate;
                                                book.ToDate = todate;
                                                book.ServiceType = bookingtb.PropertyServiceId;
                                                book.SendMessageType = bookingtb.SendMessageType;
                                                book.Status = 99;
                                                book.Quantity = bookingPriceDetail.Quantity;


                                                book.MainBookingId = bookedEvent.BookingID;
                                                book.Ordering = bookingtb.BookOrderName;
                                                book.Customer = bookingtb.CompanyPayer;
                                                if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                                {
                                                    book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
                                                }
                                                else
                                                {
                                                    book.FollowUpDate = null;
                                                }


                                                book.IsClean = false;
                                                book.IsDeliver = false;
                                                book.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
                                                entity.BookingDetails.Add(book);
                                                entity.SaveChanges();
                                            }

                                        }
                                    }
                                    if (bookingtb.foods.Count > 0)
                                    {
                                        if (bookedEvent.BookingServiceLists.Count > 0)
                                        {
                                            foreach (var item in bookedEvent.BookingServiceLists)
                                            {
                                                if (item.OrderHeadId == 0)
                                                {
                                                    item.Status = true;
                                                }

                                            }
                                            //  entity.Entry(bookedEvent.BookingServiceLists).State = EntityState.Modified;
                                        }
                                        bool isService = true;
                                        foreach (var item in bookingtb.foods)
                                        {
                                            _foodDetail = new BookingServiceList();

                                            if (isService)
                                            {
                                                isService = false;
                                                if (item.OrderHeadId == 0)
                                                {
                                                    _foodDetail.Qty = item.Qty;
                                                    _foodDetail.Status = false;
                                                    _foodDetail.Sum = item.Sum;
                                                    _foodDetail.CostPrice = item.CostPrice;
                                                    _foodDetail.CostTotal = item.CostTotal;
                                                    _foodDetail.Tekst = item.Tekst;
                                                    _foodDetail.ServiceName = item.ServiceText;
                                                    _foodDetail.IsKitchen = false;
                                                    _foodDetail.UnitText = bookingPriceDetail.UnitText;
                                                    if (!string.IsNullOrEmpty(item.ArticleId))
                                                    {
                                                        _foodDetail.ArticleId = item.ArticleId;

                                                    }
                                                    else
                                                    {
                                                        _foodDetail.ArticleId = "0";

                                                    }
                                                    if (!string.IsNullOrEmpty(item.MainServiceId))
                                                    {
                                                        _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
                                                    }
                                                    else
                                                    {
                                                        _foodDetail.FoodID = 0;
                                                    }



                                                    _foodDetail.NetAmount = item.Sum;
                                                    _foodDetail.VATPercent = 0;
                                                    _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
                                                    // _foodDetail.
                                                    _foodDetail.IsMainService = true;
                                                    _foodDetail.OrderHeadId = 0;
                                                    _foodDetail.Time = item.Time;
                                                    _foodDetail.IsVat = item.IsVatApply;

                                                    _foodDetail.BookingID = bookedEvent.BookingID;
                                                    //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                                    _foodDetail.Price = item.Price;
                                                    bookedEvent.BookingServiceLists.Add(_foodDetail);
                                                }




                                            }
                                            else
                                            {
                                                if (item.OrderHeadId == 0)
                                                {
                                                    _foodDetail.FoodID = item.FoodId;
                                                    _foodDetail.Qty = item.Qty;
                                                    _foodDetail.Status = false;
                                                    _foodDetail.Sum = item.Sum;
                                                    _foodDetail.CostPrice = item.CostPrice;
                                                    _foodDetail.CostTotal = item.CostTotal;
                                                    _foodDetail.Tekst = item.Tekst;
                                                    _foodDetail.ServiceName = item.ServiceText;
                                                    _foodDetail.IsKitchen = item.IsKitchenOrder;

                                                    var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null,false,0);
                                                    _foodDetail.UnitText = PriceUnit.UnitText;
                                                    _foodDetail.IsMainService = false;
                                                    _foodDetail.OrderHeadId = 0;
                                                    _foodDetail.IsVat = item.IsVatApply;
                                                    _foodDetail.NetAmount = item.Sum;
                                                    _foodDetail.VATPercent = 0;
                                                    _foodDetail.Time = item.Time;
                                                    _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
                                                    // _foodDetail.
                                                    if (!string.IsNullOrEmpty(item.ArticleId))
                                                    {
                                                        _foodDetail.ArticleId = item.ArticleId;
                                                    }
                                                    else
                                                    {
                                                        _foodDetail.ArticleId = "0";
                                                    }

                                                    //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                                    _foodDetail.Price = item.Price;
                                                    bookedEvent.BookingServiceLists.Add(_foodDetail);
                                                }
                                            }




                                        }

                                        entity.SaveChanges();

                                    }
                                    else
                                    {
                                        if (bookedEvent.BookingServiceLists.Count > 0)
                                        {
                                            foreach (var item in bookedEvent.BookingServiceLists)
                                            {
                                                item.Status = true;
                                            }
                                        }
                                        //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
                                        entity.SaveChanges();
                                    }

                                    //if (isNew)
                                    //{


                                    //    if (bookingtb.foods.Count > 0)
                                    //    {

                                    //        List<BookingServiceList> flist = new List<BookingServiceList>();

                                    //        foreach(var item in bookedEvent.BookingServiceLists)
                                    //        {
                                    //            if (item.OrderHeadId == 0)
                                    //            {
                                    //                item.Status = true;
                                    //            }
                                    //        }

                                    //        foreach(var orderline in bookingtb.foods)
                                    //        {
                                    //            _foodDetail = new BookingServiceList();
                                    //            if (orderline.OrderHeadId == 0)
                                    //            {
                                    //                _foodDetail.FoodID = orderline.FoodId;
                                    //                _foodDetail.Qty = orderline.Qty;
                                    //                _foodDetail.Status = false;
                                    //                _foodDetail.Sum = orderline.Sum;
                                    //                _foodDetail.Tekst = orderline.Tekst;
                                    //                _foodDetail.ServiceName = orderline.ServiceText;
                                    //                _foodDetail.OrderHeadId = 0;
                                    //                _foodDetail.BookingID = bookedEvent.BookingID;
                                    //                _foodDetail.IsKitchen = orderline.IsKitchenOrder;
                                    //                _foodDetail.IsMainService = false;
                                    //                var PriceUnit = getBookingPriceDetail(orderline.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
                                    //                _foodDetail.UnitText = PriceUnit.UnitText;
                                    //                _foodDetail.IsVat = orderline.IsVatApply;
                                    //                _foodDetail.NetAmount = orderline.Sum;
                                    //                _foodDetail.VATPercent = 0;
                                    //                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

                                    //                if (!string.IsNullOrEmpty(orderline.ArticleId))
                                    //                {
                                    //                    _foodDetail.ArticleId = orderline.ArticleId;
                                    //                }
                                    //                else
                                    //                {
                                    //                    _foodDetail.ArticleId = "0";
                                    //                }

                                    //                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                    //                _foodDetail.Price = orderline.Price;
                                    //                flist.Add(_foodDetail);
                                    //            }
                                    //         }

                                    //        entity.BookingServiceLists.AddRange(flist);

                                    //        entity.SaveChanges();
                                    //    }
                                    //}

                                    //if (notConfirmed)
                                    //{
                                    //    if (bookingtb.foods.Count > 0)
                                    //    {
                                    //        if (bookedEvent.BookingServiceLists.Count > 0)
                                    //        {
                                    //            foreach (var item in bookedEvent.BookingServiceLists)
                                    //            {
                                    //                item.Status = true;
                                    //            }
                                    //            //  entity.Entry(bookedEvent.BookingServiceLists).State = EntityState.Modified;
                                    //        }
                                    //        bool isService = true;
                                    //        foreach (var item in bookingtb.foods)
                                    //        {
                                    //            _foodDetail = new BookingServiceList();

                                    //            if (isService)
                                    //            {
                                    //                isService = false;
                                    //                _foodDetail.Qty = item.Qty;
                                    //                _foodDetail.Status = false;
                                    //                _foodDetail.Sum = item.Sum;
                                    //                _foodDetail.Tekst = item.Tekst;
                                    //                _foodDetail.ServiceName = item.ServiceText;
                                    //                _foodDetail.IsKitchen = item.IsKitchenOrder;


                                    //                _foodDetail.UnitText = bookingPriceDetail.UnitText;
                                    //                if (!string.IsNullOrEmpty(item.ArticleId))
                                    //                {
                                    //                    _foodDetail.ArticleId = item.ArticleId;

                                    //                }
                                    //                else
                                    //                {
                                    //                    _foodDetail.ArticleId = "0";

                                    //                }
                                    //                if (!string.IsNullOrEmpty(item.MainServiceId))
                                    //                {
                                    //                    _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
                                    //                }
                                    //                else
                                    //                {
                                    //                    _foodDetail.FoodID = 0;
                                    //                }



                                    //                _foodDetail.NetAmount = item.Sum;
                                    //                _foodDetail.VATPercent = 0;
                                    //                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
                                    //                // _foodDetail.
                                    //                _foodDetail.IsMainService = true;
                                    //                _foodDetail.OrderHeadId = 0;
                                    //                _foodDetail.IsVat = item.IsVatApply;

                                    //                _foodDetail.BookingID = bookedEvent.BookingID;
                                    //                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                    //                _foodDetail.Price = item.Price;

                                    //            }
                                    //            else
                                    //            {
                                    //                _foodDetail.FoodID = item.FoodId;
                                    //                _foodDetail.Qty = item.Qty;
                                    //                _foodDetail.Status = false;
                                    //                _foodDetail.Sum = item.Sum;
                                    //                _foodDetail.Tekst = item.Tekst;
                                    //                _foodDetail.ServiceName = item.ServiceText;
                                    //                _foodDetail.IsKitchen = item.IsKitchenOrder;

                                    //                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
                                    //                _foodDetail.UnitText = PriceUnit.UnitText;
                                    //                _foodDetail.IsMainService = false;
                                    //                _foodDetail.OrderHeadId = 0;
                                    //                _foodDetail.IsVat = item.IsVatApply;
                                    //                _foodDetail.NetAmount = item.Sum;
                                    //                _foodDetail.VATPercent = 0;
                                    //                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;
                                    //                // _foodDetail.
                                    //                if (!string.IsNullOrEmpty(item.ArticleId))
                                    //                {
                                    //                    _foodDetail.ArticleId = item.ArticleId;
                                    //                }
                                    //                else
                                    //                {
                                    //                    _foodDetail.ArticleId = "0";
                                    //                }

                                    //                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                    //                _foodDetail.Price = item.Price;
                                    //            }


                                    //            bookedEvent.BookingServiceLists.Add(_foodDetail);

                                    //        }

                                    //        entity.SaveChanges();

                                    //    }
                                    //    else
                                    //    {
                                    //        if (bookedEvent.BookingServiceLists.Count > 0)
                                    //        {
                                    //            foreach (var item in bookedEvent.BookingServiceLists)
                                    //            {
                                    //                item.Status = true;
                                    //            }
                                    //        }
                                    //        //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
                                    //        entity.SaveChanges();
                                    //    }
                                    //}



                                    // scope.Complete();
                                    response.data = "Bestillingen ble oppdatert!!";
                                    response.errorType = (int)ErrorType.Success;

                                    // }
                                    this.createUpdateBookingMessage(entity, bookedEvent.BookingID, bookingtb);
                                    //send message
                                    if (bookedEvent.UserID != 0)
                                    {
                                        var user = DMBase.Core.ContentModel.FetchPersonById(bookedEvent.UserID);
                                        SendBookingMessage(bookedEvent, user, "update");
                                    }
                                }
                            }
                            else
                            {
                                int IsalreadyBooked = 0;
                                if (bookingtb.IsBooking)
                                {
                                    IsalreadyBooked = (from book in entity.BookingDetails
                                                       where
                                                        book.FromDate < todate && fromdate < book.ToDate
                                                        && book.ServiceID == bookingtb.MeetingRoomId
                                                       select book.BookingID).Count();
                                }

                                if (IsalreadyBooked > 0)
                                {



                                    response.data = "Allerede booket!!";
                                    response.errorType = (int)ErrorType.Error;
                                    log.LogWrite("Failed request:- " + response.data + " Response:- " + response.errorType);
                                    return response;
                                }
                                else
                                {
                                    //using (TransactionScope scope = new TransactionScope())
                                    //{

                                    _bookingDetail.BuildingID = bookingtb.PropertyId;
                                    _bookingDetail.CreatedOn = DateTime.Now;
                                    _bookingDetail.NoOfPeople = bookingtb.NoOfPeople;
                                    _bookingDetail.UserID = bookingtb.UserID;



                                    _bookingDetail.ServiceID = bookingtb.MeetingRoomId;
                                    _bookingDetail.IsFoodOrder = bookingtb.IsFoodOrder;
                                    _bookingDetail.FromDate = fromdate;
                                    _bookingDetail.ToDate = todate;
                                    _bookingDetail.ServiceType = bookingtb.PropertyServiceId;
                                    _bookingDetail.SendMessageType = bookingtb.SendMessageType;
                                    _bookingDetail.Ordering = bookingtb.BookOrderName;
                                    _bookingDetail.Customer = bookingtb.CompanyPayer;
                                    _bookingDetail.Note = bookingtb.Note;
                                    _bookingDetail.InvoMessage = bookingtb.InvoMessage;
                                    _bookingDetail.IsInternal = bookingtb.IsInternal;
                                    _bookingDetail.IsMVA = bookingtb.IsMVA;
                                    _bookingDetail.IsClean = false;
                                    _bookingDetail.IsDeliver = false;
                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                    {
                                        _bookingDetail.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
                                    }
                                    else
                                    {
                                        _bookingDetail.FollowUpDate = null;
                                    }
                                    _bookingDetail.Quantity = bookingPriceDetail.Quantity;
                                    _bookingDetail.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
                                    _bookingDetail.BookingName = bookingtb.nameOfbook;
                                    if (bookingtb.IsBooking)
                                    {
                                        _bookingDetail.Status = 0;
                                       
                                    }
                                    else
                                    {
                                        _bookingDetail.Status = 98;
                                      
                                    }

                                  
                                    entity.BookingDetails.Add(_bookingDetail);
                                    entity.SaveChanges();

                                    this.createUpdateBookingMessage(entity, _bookingDetail.BookingID, bookingtb);





                                    if (bookingtb.foods.Count > 0)
                                    {
                                        bool isService = true;
                                        List<BookingServiceList> flist = new List<BookingServiceList>();
                                        foreach (var item in bookingtb.foods)
                                        {
                                            _foodDetail = new BookingServiceList();
                                            if (isService)
                                            {

                                                _foodDetail.Qty = item.Qty;
                                                _foodDetail.Status = false;
                                                _foodDetail.Sum = item.Sum;
                                                _foodDetail.Tekst = item.Tekst;
                                                _foodDetail.ServiceName = item.ServiceText;
                                                _foodDetail.OrderHeadId = 0;
                                                _foodDetail.BookingID = _bookingDetail.BookingID;
                                                _foodDetail.IsKitchen = false;
                                                _foodDetail.IsVat = item.IsVatApply;
                                                _foodDetail.CostPrice = item.CostPrice;
                                                _foodDetail.CostTotal = item.CostTotal;
                                                _foodDetail.UnitText = bookingPriceDetail.UnitText;
                                                if (!string.IsNullOrEmpty(item.ArticleId))
                                                {
                                                    _foodDetail.ArticleId = item.ArticleId;

                                                }
                                                else
                                                {
                                                    _foodDetail.ArticleId = "0";

                                                }
                                                if (!string.IsNullOrEmpty(item.MainServiceId))
                                                {
                                                    _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
                                                }
                                                else
                                                {
                                                    _foodDetail.FoodID = 0;
                                                }


                                                _foodDetail.NetAmount = item.Sum;
                                                _foodDetail.VATPercent = 0;
                                                _foodDetail.Time = item.Time;
                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

                                                _foodDetail.Price = item.Price;
                                                _foodDetail.IsMainService = true;
                                                isService = false;
                                            }
                                            else
                                            {
                                                _foodDetail.FoodID = item.FoodId;
                                                _foodDetail.Qty = item.Qty;
                                                _foodDetail.Status = false;
                                                _foodDetail.Sum = item.Sum;

                                                _foodDetail.OrderHeadId = 0;
                                                _foodDetail.Tekst = item.Tekst;
                                                _foodDetail.ServiceName = item.ServiceText;
                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
                                                _foodDetail.IsMainService = false;
                                                _foodDetail.Time = item.Time;
                                                _foodDetail.BookingID = _bookingDetail.BookingID;
                                                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null,false,0);
                                                _foodDetail.UnitText = PriceUnit.UnitText;
                                                _foodDetail.IsVat = item.IsVatApply;
                                                _foodDetail.NetAmount = item.Sum;
                                                _foodDetail.VATPercent = 0;
                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - (decimal)_foodDetail.VATPercent * 0) / 100;

                                                if (!string.IsNullOrEmpty(item.ArticleId))
                                                {
                                                    _foodDetail.ArticleId = item.ArticleId;
                                                }
                                                else
                                                {
                                                    _foodDetail.ArticleId = "0";
                                                }

                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
                                                _foodDetail.Price = item.Price;
                                                _foodDetail.CostPrice = item.CostPrice;
                                                _foodDetail.CostTotal = item.CostTotal;
                                            }


                                            flist.Add(_foodDetail);



                                        }
                                        if (!bookingtb.IsBooking)
                                        {
                                            var fval = flist.Where(w => w.IsMainService == true).FirstOrDefault();
                                            flist.Remove(fval);
                                        }
                                        entity.BookingServiceLists.AddRange(flist);
                                        entity.SaveChanges();
                                    }





                                    var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
                                    .Where(w => w.aa.MainID == bookingtb.MeetingRoomId).Select(s => s.aa).ToList();
                                    if (relatedRooms.Count > 0)
                                    {
                                        BookingDetail book;

                                        foreach (var i in relatedRooms)
                                        {
                                            book = new BookingDetail();
                                            book.BuildingID = bookingtb.PropertyId;
                                            book.CreatedOn = DateTime.Now;
                                            book.NoOfPeople = bookingtb.NoOfPeople;
                                            book.UserID = bookingtb.UserID;


                                            book.BookingName = bookingtb.nameOfbook;
                                            book.ServiceID = i.RelatedMainID;
                                            book.IsFoodOrder = bookingtb.IsFoodOrder;
                                            book.FromDate = fromdate;
                                            book.ToDate = todate;
                                            book.ServiceType = bookingtb.PropertyServiceId;
                                            book.SendMessageType = bookingtb.SendMessageType;
                                            book.Status = 99;
                                            book.Quantity = bookingPriceDetail.Quantity;


                                            book.MainBookingId = _bookingDetail.BookingID;
                                            book.Ordering = bookingtb.BookOrderName;
                                            book.Customer = bookingtb.CompanyPayer;
                                            if (!string.IsNullOrEmpty(bookingtb.FollowDate))
                                            {
                                                book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
                                            }
                                            else
                                            {
                                                book.FollowUpDate = null;
                                            }


                                            book.IsClean = false;
                                            book.IsDeliver = false;
                                            book.IsInternal = bookingtb.IsInternal;
                                            book.IsMVA = bookingtb.IsMVA;
                                            book.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
                                            entity.BookingDetails.Add(book);
                                            entity.SaveChanges();
                                        }

                                    }



                                    //send message
                                    if (_bookingDetail.UserID != 0)
                                    {

                                        var user = DMBase.Core.ContentModel.FetchPersonById(_bookingDetail.UserID);
                                        SendBookingMessage(_bookingDetail, user, "new");
                                    }



                                    response.data = "Bestilling vellykket lagret!!";
                                    response.errorType = (int)ErrorType.Success;

                                }


                            }





                            dbContextTransaction.Commit();
                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    log.LogWrite("BookingSave Failed request:- " + ve.ErrorMessage);
                                }
                            }
                            response.data = "Booking ikke lagret!!";
                            response.errorType = (int)ErrorType.Error;

                            dbContextTransaction.Rollback();
                            throw;
                            // throw new Exception(e.Message + ":" + e.StackTrace);
                        }
                        catch (Exception e)
                        {
                            response.data = "Booking ikke lagret!!";
                            response.errorType = (int)ErrorType.Error;
                            log.LogWrite("BookingSave Failed request:- " + e.Message);
                            dbContextTransaction.Rollback();
                        }

                    }

                }

            }





            return response;
        }
        private void createUpdateBookingMessage(BraathenEiendomEntities entities, int bookingID, BookingTB bookingTB)
        {
            if (bookingTB.MessageList != null)
            {
                foreach (var message in bookingTB.MessageList)
                {
                    //remove
                    if (message.ID < 0)
                    {
                        var bookingMessage = entities.BookingMessages.Where(w => w.ID == -message.ID).FirstOrDefault();
                        if (bookingMessage != null)
                        {
                            entities.BookingMessages.Remove(bookingMessage);
                            entities.SaveChanges();
                        }
                    }
                    else
                    {
                        var bookingMessage = entities.BookingMessages.Where(w => w.ID == message.ID).ToList().FirstOrDefault();
                        //add
                        if (bookingMessage == null)
                        {
                            bookingMessage = new BookingMessage();
                            entities.BookingMessages.Add(bookingMessage);
                            bookingMessage.Status = 0;
                        }
                        //update
                        bookingMessage.BookingID = bookingID;
                        bookingMessage.Type = message.Type;
                        bookingMessage.To = message.To;
                        bookingMessage.Subject = message.Subject;
                        bookingMessage.Body = message.Body;
                        bookingMessage.Attachments = message.Attachments;
                        if (message.SendTime != null)
                        {
                            bookingMessage.SendTime = DateTime.Parse(message.SendTime);
                        }
                    }

                }
                //save
                entities.SaveChanges();

                //send message now.
                //var task = new TaskController();
                //task.BookingSendMessage( bookingID ); // log is in message log feature.
            }
        }

        [HttpGet]
        public List<BookingMessage> GetMessageList(int bookingID)
        {
            var enetity = new BraathenEiendomEntities();
            var list = enetity.BookingMessages.Where(w => w.BookingID == bookingID
                                                          && w.Status == 0).ToList();
            return list;
        }

        [HttpGet]
        public List<object> GetMessageTemplateList()
        {
            var articleList = SiteBuilder.Content.Article.ByMenuIds(Digimaker.Config.Custom.Get("MessageTemplateFolder"), Digimaker.Data.Content.ArticleSortOrder.Default, 0, false, SiteBuilder.Content.Article.ReturnValues.AbstractAndFullstory, 50).Article.Rows;

            var result = new List<object>();
            foreach (ArticleViewData.ArticleRow row in articleList)
            {
                var data = new
                {
                    id = row.MainID,
                    type = row.Ismeta_keywordsNull() ? "" : row.meta_keywords,
                    name = row.Headline,
                    subject = row.Abstract,
                    body = row.Fullstory
                };
                result.Add(data);
            }
            return result;
        }


        // POST api/values
        //[HttpPost]
        //public BookingResponse Post([FromBody]BookingTB bookingtb)
        //{
        //    string message = "";
        //    Orderline orders;
        //    var days = Math.Round((Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).TotalDays);
        //    var hours = (Convert.ToDateTime(bookingtb.ToDate) - Convert.ToDateTime(bookingtb.FromDate)).Hours;
        //    var currenttime = DateTime.Now;
        //    var booktime = Convert.ToDateTime(bookingtb.FromDate);

        //        if (ModelState.IsValid)
        //        {

        //            BookingDetail _bookingDetail = new BookingDetail();
        //            BookingServiceList _foodDetail;
        //            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
        //            {
        //                using (var dbContextTransaction = entity.Database.BeginTransaction())
        //                {
        //                try
        //                {

        //                    var fdate = Convert.ToDateTime(bookingtb.FromDate);
        //                    TimeSpan fTime = TimeSpan.Parse(bookingtb.FromTimer);
        //                    var fromdate = fdate.Add(fTime);
        //                    var tdate = Convert.ToDateTime(bookingtb.ToDate);
        //                    TimeSpan tTime = TimeSpan.Parse(bookingtb.ToTimer);
        //                    var todate = tdate.Add(tTime);
        //                    // var validate = DateTime.Compare(booktime, currenttime);


        //                    var bookingPriceDetail = getBookingPriceDetail(bookingtb.MeetingRoomId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), bookingtb.FromDate.ToString(), bookingtb.ToDate.ToString(), bookingtb.FromTimer, bookingtb.ToTimer);
        //                    //else if (validate<0) {
        //                    //     message = "Booking Not Allowed" ;
        //                    // }
        //                    string articleName = entity.Articles.Where(w => w.MainID == bookingtb.MeetingRoomId && w.Status == 0).Select(s => s.Headline).FirstOrDefault().ToString();
        //                    if (bookingtb.BookingID != 0)
        //                    {
        //                        var bookedEvent = entity.BookingDetails.Where(w => w.BookingID == bookingtb.BookingID).FirstOrDefault();
        //                        if (bookedEvent != null)
        //                        {
        //                            bool isConfirmed = false;
        //                            //using (TransactionScope scope = new TransactionScope())
        //                            //{
        //                                bookedEvent.BuildingID = bookingtb.PropertyId;
        //                                bookedEvent.CreatedOn = DateTime.Now;
        //                                bookedEvent.NoOfPeople = bookingtb.NoOfPeople;
        //                                bookedEvent.UserID = bookingtb.UserID;
        //                                bookedEvent.BookingName = bookingtb.nameOfbook;
        //                                bookedEvent.ServiceID = bookingtb.MeetingRoomId;
        //                                bookedEvent.IsFoodOrder = bookingtb.IsFoodOrder;
        //                                bookedEvent.FromDate = fromdate;
        //                                bookedEvent.ToDate = todate;
        //                                bookedEvent.ServiceType = bookingtb.PropertyServiceId;
        //                                bookedEvent.SendMessageType = bookingtb.SendMessageType;
        //                                bookedEvent.Ordering = bookingtb.BookOrderName;
        //                                bookedEvent.Customer = bookingtb.CompanyPayer;
        //                                if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                                {
        //                                    bookedEvent.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                }
        //                                else
        //                                {
        //                                    bookedEvent.FollowUpDate = null;
        //                                }
        //                                if (bookingtb.IsConfirmed)
        //                                {
        //                                if (bookedEvent.Status == 0)
        //                                {
        //                                    isConfirmed = true;
        //                                    bookedEvent.Status = 1;
        //                                }




        //                                }
        //                                else
        //                                {
        //                                    bookedEvent.Status = 0;
        //                                }
        //                                bookedEvent.Quantity = bookingPriceDetail.Quantity;
        //                            bookedEvent.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                            //  bookedEvent.UnitText = bookingPriceDetail.UnitText;
        //                            // bookedEvent.UnitPrice = bookingPriceDetail.UnitPrice;
        //                            //  bookedEvent.DiscountPercent = bookingPriceDetail.DiscountPercent;
        //                            //  bookedEvent.VATPercent = bookingPriceDetail.VATPercent;
        //                            //  bookedEvent.NetAmount = bookingPriceDetail.NetAmount;
        //                            // bookedEvent.Amount = bookingPriceDetail.Amount;
        //                            //   int price = bookingtb.foods.Where(w => w.ArticleId.Equals(bookingtb.MeetingRoomId.ToString())).Select(s => s.Sum).FirstOrDefault();

        //                            //   bookedEvent.NetAmount = price;
        //                            //  _bookingDetail.Amount = bookingPriceDetail.Amount;
        //                            //  bookedEvent.Amount = _bookingDetail.NetAmount * (100 - _bookingDetail.VATPercent * 0) / 100;
        //                            if (bookingtb.foods.Count > 0)
        //                                {
        //                                    if (bookedEvent.BookingServiceLists.Count > 0)
        //                                    {
        //                                        foreach (var item in bookedEvent.BookingServiceLists)
        //                                        {
        //                                            item.Status = true;
        //                                        }
        //                                      //  entity.Entry(bookedEvent.BookingServiceLists).State = EntityState.Modified;
        //                                    }
        //                                bool isService = true;
        //                                foreach (var item in bookingtb.foods)
        //                                    {
        //                                    _foodDetail = new BookingServiceList();

        //                                    if (isService)
        //                                    {
        //                                        isService = false;
        //                                        _foodDetail.Qty = item.Qty;
        //                                        _foodDetail.Status = false;
        //                                        _foodDetail.Sum = item.Sum;
        //                                        _foodDetail.Tekst = item.Tekst;
        //                                        _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                        _foodDetail.IsVat = item.IsVatApply;

        //                                        _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                        if (!string.IsNullOrEmpty(item.ArticleId))
        //                                        {
        //                                            _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);

        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.ArticleId = 0;

        //                                        }
        //                                        if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                        {
        //                                            _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                        }
        //                                        {
        //                                            _foodDetail.FoodID = 0;
        //                                        }
        //                                        _foodDetail.VatType = new ArticlesController().getVatType(_foodDetail.FoodID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));

        //                                        if (item.IsVatApply)
        //                                        {
        //                                            _foodDetail.VATPercent = 0;
        //                                        }
        //                                        else
        //                                        {
        //                                            //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                            _foodDetail.VATPercent = 0;
        //                                        }

        //                                        _foodDetail.NetAmount = item.Sum;
        //                                        _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                        // _foodDetail.

        //                                        //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                        _foodDetail.Price = item.Price;

        //                                    }
        //                                    else
        //                                    {
        //                                        _foodDetail.FoodID = item.FoodId;
        //                                        _foodDetail.Qty = item.Qty;
        //                                        _foodDetail.Status = false;
        //                                        _foodDetail.Sum = item.Sum;
        //                                        _foodDetail.Tekst = item.Tekst;
        //                                        _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                        _foodDetail.IsVat = item.IsVatApply;
        //                                        var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                        _foodDetail.UnitText = PriceUnit.UnitText;
        //                                        _foodDetail.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                        //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                        _foodDetail.VATPercent = 0;
        //                                        _foodDetail.NetAmount = item.Sum;
        //                                        _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                        // _foodDetail.
        //                                        if (!string.IsNullOrEmpty(item.ArticleId))
        //                                        {
        //                                            _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);
        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.ArticleId = 0;
        //                                        }

        //                                        //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                        _foodDetail.Price = item.Price;
        //                                    }


        //                                        bookedEvent.BookingServiceLists.Add(_foodDetail);

        //                                    }

        //                                    entity.SaveChanges();

        //                                }
        //                                else
        //                                {
        //                                    if (bookedEvent.BookingServiceLists.Count > 0)
        //                                    {
        //                                        foreach (var item in bookedEvent.BookingServiceLists)
        //                                        {
        //                                            item.Status = true;
        //                                        }
        //                                    }
        //                                    //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
        //                                    entity.SaveChanges();
        //                                }
        //                            if (isConfirmed)
        //                            {
        //                                List<Orderline> orderList = new List<Orderline>();
        //                                Orderhead bookedOrder = new Orderhead();
        //                                Orderline order = new Orderline();



        //                                bookedOrder.Orderdate = DateTime.Now;
        //                                bookedOrder.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                bookedOrder.Status = "Confirmed";
        //                                bookedOrder.VAT = 0;
        //                                bookedOrder.CustomerName = bookingtb.nameOfbook;
        //                                bookedOrder.CustomerNo = bookingtb.UserID.ToString();
        //                                bookedOrder.EmailAddress = "abc";
        //                                bookedOrder.ERPClient = "yes";
        //                                bookedOrder.YourReference = "no";
        //                                bookedOrder.OurReference = "yes";
        //                                bookedOrder.NoOFPeople = bookingtb.NoOfPeople;
        //                                bookedOrder.FromDate = fromdate;
        //                                bookedOrder.Todate = todate;
        //                                bookedOrder.IsClean = false;
        //                                bookedOrder.IsDeliver = false;
        //                                bookedOrder.bookingId = bookedEvent.BookingID;
        //                                entity.Orderheads.Add(bookedOrder);


        //                                // entity.SaveChanges();
        //                                // orderList.Add(order);
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    bool isService = true;
        //                                    foreach (var item in bookingtb.foods)
        //                                    {

        //                                        orders = new Orderline();
        //                                        if (isService)
        //                                        {
        //                                            // order.Article = bookingtb.MeetingRoomId.ToString();
        //                                            // order.Text = articleName;
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                orders.Article = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                orders.Article = "0";
        //                                            }
        //                                            //  order.Article = item.ArticleId;
        //                                            orders.Text = item.Tekst;
        //                                            orders.Quantity = item.Qty;
        //                                            orders.UnitText = bookingPriceDetail.UnitText;
        //                                            orders.UnitPrice = (double)item.Price;
        //                                            orders.DiscountPercent = bookingPriceDetail.DiscountPercent;
        //                                            orders.IsVat = item.IsVatApply;
        //                                            orders.VatType = new ArticlesController().getVatType(Convert.ToInt32(item.ArticleId), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                            if (item.IsVatApply)
        //                                            {
        //                                                orders.VATPercent = 0;
        //                                            }
        //                                            else
        //                                            {
        //                                                //update api here for vat percent
        //                                                orders.VATPercent = 0;
        //                                            }

        //                                            orders.NetAmount = item.Sum;
        //                                            orders.Amount = item.Sum * (100 - order.VATPercent * 0) / 100;
        //                                            orders.IsKitchen = item.IsKitchenOrder;
        //                                            orders.IsMainService = true;

        //                                            //  entity.Orderlines.Add(order);
        //                                            isService = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            // orders.Article = item.FoodId.ToString();
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                orders.Article = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                orders.Article = "0";
        //                                            }
        //                                            // order.Article = item.ArticleId;
        //                                            //  orders.Text = entity.Articles.Where(w => w.MainID == item.FoodId && w.Status == 0).Select(s => s.Headline).FirstOrDefault().ToString();
        //                                            orders.Text = item.Tekst;
        //                                            orders.Quantity = item.Qty;

        //                                            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                            orders.UnitText = PriceUnit.UnitText;
        //                                            // orders.UnitPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            orders.UnitPrice = (double)item.Price;
        //                                            orders.IsKitchen = item.IsKitchenOrder;
        //                                            orders.IsMainService = false;
        //                                            orders.IsVat = item.IsVatApply;
        //                                            orders.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                            // orders.IsDeliver = false;
        //                                            // orders.IsClean = false;

        //                                            //ToDo : discountrate get from CE .as of now passing zero
        //                                            orders.DiscountPercent = 0;
        //                                            // orders.NetAmount = item.Qty * orders.UnitPrice * (100 - 0) / 100;
        //                                            orders.NetAmount = item.Sum;
        //                                            //ToDo :vatpercent get from CE. as of now zero
        //                                            // orders.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), orders.VatType ?? 0);
        //                                            orders.VATPercent = 0;
        //                                            // orders.Amount = orders.NetAmount * (100 - 0 * 0) / 100;
        //                                            orders.Amount = item.Sum * (100 - orders.VATPercent * 0) / 100;
        //                                        }

        //                                        orderList.Add(orders);
        //                                    }
        //                                    entity.Orderlines.AddRange(orderList);
        //                                }
        //                                entity.SaveChanges();
        //                            }
        //                            var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
        //                          .Where(w => w.aa.MainID == bookedEvent.ServiceID).Select(s => s.aa).ToList();
        //                                if (relatedRooms != null)
        //                                {
        //                                    //  string articleName = entity.Articles.Where(w => w.ArticleID == bookingtb.MeetingRoomId).Select(s => s.Headline).FirstOrDefault().ToString();
        //                                    foreach (var i in relatedRooms)
        //                                    {
        //                                        var subBooking = entity.BookingDetails.Where(w => w.ServiceID == i.RelatedMainID && w.MainBookingId == bookedEvent.BookingID).Select(s => s).FirstOrDefault();
        //                                        subBooking.BuildingID = bookingtb.PropertyId;
        //                                        subBooking.CreatedOn = DateTime.Now;
        //                                        subBooking.NoOfPeople = bookingtb.NoOfPeople;
        //                                        subBooking.UserID = bookingtb.UserID;
        //                                        subBooking.BookingName = articleName;
        //                                        subBooking.ServiceID = i.RelatedMainID;
        //                                        subBooking.IsFoodOrder = bookingtb.IsFoodOrder;
        //                                        subBooking.FromDate = Convert.ToDateTime(bookingtb.FromDate);
        //                                        subBooking.ToDate = Convert.ToDateTime(bookingtb.ToDate);
        //                                        subBooking.ServiceType = bookingtb.PropertyServiceId;
        //                                        subBooking.SendMessageType = bookingtb.SendMessageType;
        //                                        subBooking.Status = 99;
        //                                        subBooking.Ordering = bookingtb.BookOrderName;
        //                                        subBooking.Customer = bookingtb.CompanyPayer;
        //                                        if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                                        {
        //                                            subBooking.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                        }
        //                                        else
        //                                        {
        //                                            subBooking.FollowUpDate = null;
        //                                        }
        //                                        subBooking.Quantity = bookingPriceDetail.Quantity;
        //                                    subBooking.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                    //subBooking.UnitText = bookingPriceDetail.UnitText;
        //                                    //subBooking.UnitPrice = bookingPriceDetail.UnitPrice;
        //                                    //subBooking.DiscountPercent = bookingPriceDetail.DiscountPercent;
        //                                    //subBooking.VATPercent = bookingPriceDetail.VATPercent;
        //                                    //subBooking.NetAmount = bookingPriceDetail.NetAmount;
        //                                    //subBooking.Amount = bookingPriceDetail.Amount;

        //                                    if (bookingtb.foods.Count > 0)
        //                                        {
        //                                            if (subBooking.BookingServiceLists.Count > 0)
        //                                            {
        //                                                foreach (var item in subBooking.BookingServiceLists)
        //                                                {
        //                                                    item.Status = true;
        //                                                }
        //                                                //entity.Entry(bookedEvent.FoodServices).State = EntityState.Modified;
        //                                            }
        //                                        bool isService = true;
        //                                        foreach (var item in bookingtb.foods)
        //                                            {

        //                                            _foodDetail = new BookingServiceList();

        //                                            if (isService)
        //                                            {
        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsVat = item.IsVatApply;

        //                                                _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);
        //                                                    _foodDetail.FoodID = Convert.ToInt32(item.ArticleId);
        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = 0;
        //                                                    _foodDetail.FoodID = 0;
        //                                                }
        //                                                _foodDetail.VatType = new ArticlesController().getVatType(_foodDetail.FoodID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));

        //                                                if (item.IsVatApply)
        //                                                {
        //                                                    _foodDetail.VATPercent = 0;
        //                                                }
        //                                                else
        //                                                {
        //                                                    //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                                    _foodDetail.VATPercent = 0;
        //                                                }

        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                                // _foodDetail.

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;
        //                                                isService = false;
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = item.FoodId;
        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsVat = item.IsVatApply;
        //                                                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                                _foodDetail.UnitText = PriceUnit.UnitText;
        //                                                _foodDetail.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                                //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                                _foodDetail.VATPercent = 0;
        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                                // _foodDetail.
        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);
        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = 0;
        //                                                }

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;
        //                                            }


        //                                                subBooking.BookingServiceLists.Add(_foodDetail);

        //                                            }
        //                                            entity.SaveChanges();

        //                                        }
        //                                        else
        //                                        {
        //                                            if (subBooking.BookingServiceLists.Count > 0)
        //                                            {
        //                                                foreach (var item in subBooking.BookingServiceLists)
        //                                                {
        //                                                    item.Status = true;
        //                                                }
        //                                            }
        //                                            //entity.Entry(bookedEvent.FoodServices).State= EntityState.Modified;
        //                                            entity.SaveChanges();
        //                                        }
        //                                    }
        //                                }

        //                           // scope.Complete();
        //                            message = "Booking Succesfully Updated";
        //                           // }
        //                            //send message
        //                            if (bookedEvent.UserID != 0)
        //                            {
        //                                var user = DMBase.Core.ContentModel.FetchPersonById(bookedEvent.UserID);
        //                                SendBookingMessage(bookedEvent, user, "update");
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var IsalreadyBooked = (from book in entity.BookingDetails
        //                                               where
        //                                                book.FromDate < todate && fromdate < book.ToDate
        //                                                && book.ServiceID == bookingtb.MeetingRoomId
        //                                               select book.BookingID).Count();
        //                        if (IsalreadyBooked > 0)
        //                        {
        //                            message = "AlreadyBooked";
        //                        }
        //                        else
        //                        {
        //                            //using (TransactionScope scope = new TransactionScope())
        //                            //{
        //                            bool isConfirmed = false;
        //                            _bookingDetail.BuildingID = bookingtb.PropertyId;
        //                            _bookingDetail.CreatedOn = DateTime.Now;
        //                            _bookingDetail.NoOfPeople = bookingtb.NoOfPeople;
        //                            _bookingDetail.UserID = bookingtb.UserID;


        //                            _bookingDetail.BookingName = bookingtb.nameOfbook;
        //                            _bookingDetail.ServiceID = bookingtb.MeetingRoomId;
        //                            _bookingDetail.IsFoodOrder = bookingtb.IsFoodOrder;
        //                            _bookingDetail.FromDate = fromdate;
        //                            _bookingDetail.ToDate = todate;
        //                            _bookingDetail.ServiceType = bookingtb.PropertyServiceId;
        //                            _bookingDetail.SendMessageType = bookingtb.SendMessageType;
        //                            _bookingDetail.Ordering = bookingtb.BookOrderName;
        //                            _bookingDetail.Customer = bookingtb.CompanyPayer;
        //                            if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                            {
        //                                _bookingDetail.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                            }
        //                            else
        //                            {
        //                                _bookingDetail.FollowUpDate = null;
        //                            }
        //                            _bookingDetail.Quantity = bookingPriceDetail.Quantity;

        //                            if (bookingtb.foods.Count > 0)
        //                            {
        //                                bool isService = true;
        //                                List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                foreach (var item in bookingtb.foods)
        //                                {
        //                                    _foodDetail = new BookingServiceList();
        //                                    if (isService)
        //                                    {

        //                                        _foodDetail.Qty = item.Qty;
        //                                        _foodDetail.Status = false;
        //                                        _foodDetail.Sum = item.Sum;
        //                                        _foodDetail.Tekst = item.Tekst;
        //                                        _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                        _foodDetail.IsVat = item.IsVatApply;

        //                                        _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                        if (!string.IsNullOrEmpty(item.ArticleId))
        //                                        {
        //                                            _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);

        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.ArticleId = 0;

        //                                        }
        //                                        if (!string.IsNullOrEmpty(item.MainServiceId))
        //                                        {
        //                                            _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.FoodID = 0;
        //                                        }
        //                                        _foodDetail.VatType = new ArticlesController().getVatType(_foodDetail.FoodID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));

        //                                        if (item.IsVatApply)
        //                                        {
        //                                            _foodDetail.VATPercent = 0;
        //                                        }
        //                                        else
        //                                        {
        //                                            //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                            _foodDetail.VATPercent = 0;
        //                                        }

        //                                        _foodDetail.NetAmount = item.Sum;
        //                                        _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                        // _foodDetail.

        //                                        //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                        _foodDetail.Price = item.Price;
        //                                        isService = false;
        //                                    }
        //                                    else
        //                                    {
        //                                        _foodDetail.FoodID = item.FoodId;
        //                                        _foodDetail.Qty = item.Qty;
        //                                        _foodDetail.Status = false;
        //                                        _foodDetail.Sum = item.Sum;
        //                                        _foodDetail.Tekst = item.Tekst;
        //                                        _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                        _foodDetail.IsVat = item.IsVatApply;
        //                                        var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                        _foodDetail.UnitText = PriceUnit.UnitText;
        //                                        _foodDetail.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                        //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                        _foodDetail.VATPercent = 0;
        //                                        _foodDetail.NetAmount = item.Sum;
        //                                        _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                        // _foodDetail.
        //                                        if (!string.IsNullOrEmpty(item.ArticleId))
        //                                        {
        //                                            _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);
        //                                        }
        //                                        else
        //                                        {
        //                                            _foodDetail.ArticleId = 0;
        //                                        }

        //                                        //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                        _foodDetail.Price = item.Price;
        //                                    }

        //                                    flist.Add(_foodDetail);

        //                                }
        //                                entity.BookingServiceLists.AddRange(flist);
        //                                //entity.SaveChanges();
        //                            }


        //                            _bookingDetail.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();

        //                            if (bookingtb.IsConfirmed)
        //                            {
        //                                _bookingDetail.Status = 1;
        //                                isConfirmed = true;

        //                            }
        //                            else
        //                            {
        //                                _bookingDetail.Status = 0;
        //                            }
        //                            entity.BookingDetails.Add(_bookingDetail);
        //                            entity.SaveChanges();

        //                            if (isConfirmed)
        //                            {
        //                                List<Orderline> orderList = new List<Orderline>();
        //                                Orderhead bookedOrder = new Orderhead();
        //                                Orderline order = new Orderline();



        //                                bookedOrder.Orderdate = DateTime.Now;
        //                                bookedOrder.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                bookedOrder.Status = "Confirmed";
        //                                bookedOrder.VAT = 0;
        //                                bookedOrder.CustomerName = bookingtb.nameOfbook;
        //                                bookedOrder.CustomerNo = bookingtb.UserID.ToString();
        //                                bookedOrder.EmailAddress = "abc";
        //                                bookedOrder.ERPClient = "yes";
        //                                bookedOrder.YourReference = "no";
        //                                bookedOrder.OurReference = "yes";
        //                                bookedOrder.NoOFPeople = bookingtb.NoOfPeople;
        //                                bookedOrder.FromDate = fromdate;
        //                                bookedOrder.Todate = todate;
        //                                bookedOrder.IsClean = false;
        //                                bookedOrder.IsDeliver = false;
        //                                bookedOrder.bookingId = _bookingDetail.BookingID;
        //                                entity.Orderheads.Add(bookedOrder);


        //                                // entity.SaveChanges();
        //                                // orderList.Add(order);
        //                                if (bookingtb.foods.Count > 0)
        //                                {
        //                                    bool isService = true;
        //                                    foreach (var item in bookingtb.foods)
        //                                    {

        //                                        orders = new Orderline();
        //                                        if (isService)
        //                                        {
        //                                            // order.Article = bookingtb.MeetingRoomId.ToString();
        //                                            // order.Text = articleName;
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                orders.Article = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                orders.Article = "0";
        //                                            }
        //                                            //  order.Article = item.ArticleId;
        //                                            orders.Text = item.Tekst;
        //                                            orders.Quantity = item.Qty;
        //                                            orders.UnitText = bookingPriceDetail.UnitText;
        //                                            orders.UnitPrice = (double)item.Price;
        //                                            orders.DiscountPercent = bookingPriceDetail.DiscountPercent;
        //                                            orders.IsVat = item.IsVatApply;
        //                                            orders.VatType = new ArticlesController().getVatType(Convert.ToInt32(item.ArticleId), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                            if (item.IsVatApply)
        //                                            {
        //                                                orders.VATPercent = 0;
        //                                            }
        //                                            else
        //                                            {
        //                                                //update api here for vat percent
        //                                                orders.VATPercent = 0;
        //                                            }

        //                                            orders.NetAmount = item.Sum;
        //                                            orders.Amount = item.Sum * (100 - order.VATPercent * 0) / 100;
        //                                            orders.IsKitchen = item.IsKitchenOrder;
        //                                            orders.IsMainService = true;

        //                                            //  entity.Orderlines.Add(order);
        //                                            isService = false;
        //                                        }
        //                                        else
        //                                        {
        //                                            // orders.Article = item.FoodId.ToString();
        //                                            if (!string.IsNullOrEmpty(item.ArticleId))
        //                                            {
        //                                                orders.Article = item.ArticleId;
        //                                            }
        //                                            else
        //                                            {
        //                                                orders.Article = "0";
        //                                            }
        //                                            // order.Article = item.ArticleId;
        //                                            //  orders.Text = entity.Articles.Where(w => w.MainID == item.FoodId && w.Status == 0).Select(s => s.Headline).FirstOrDefault().ToString();
        //                                            orders.Text = item.Tekst;
        //                                            orders.Quantity = item.Qty;

        //                                            var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                            orders.UnitText = PriceUnit.UnitText;
        //                                            // orders.UnitPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                            orders.UnitPrice = (double)item.Price;
        //                                            orders.IsKitchen = item.IsKitchenOrder;
        //                                            orders.IsMainService = false;
        //                                            orders.IsVat = item.IsVatApply;
        //                                            orders.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                            // orders.IsDeliver = false;
        //                                            // orders.IsClean = false;

        //                                            //ToDo : discountrate get from CE .as of now passing zero
        //                                            orders.DiscountPercent = 0;
        //                                            // orders.NetAmount = item.Qty * orders.UnitPrice * (100 - 0) / 100;
        //                                            orders.NetAmount = item.Sum;
        //                                            //ToDo :vatpercent get from CE. as of now zero
        //                                            // orders.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), orders.VatType ?? 0);
        //                                            orders.VATPercent = 0;
        //                                            // orders.Amount = orders.NetAmount * (100 - 0 * 0) / 100;
        //                                            orders.Amount = item.Sum * (100 - orders.VATPercent * 0) / 100;
        //                                        }

        //                                        orderList.Add(orders);
        //                                    }
        //                                    entity.Orderlines.AddRange(orderList);
        //                                }
        //                                entity.SaveChanges();
        //                            }




        //                            var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
        //                            .Where(w => w.aa.MainID == bookingtb.MeetingRoomId).Select(s => s.aa).ToList();
        //                            if (relatedRooms.Count > 0)
        //                            {
        //                                BookingDetail book;

        //                                foreach (var i in relatedRooms)
        //                                {
        //                                    book = new BookingDetail();
        //                                    book.BuildingID = bookingtb.PropertyId;
        //                                    book.CreatedOn = DateTime.Now;
        //                                    book.NoOfPeople = bookingtb.NoOfPeople;
        //                                    book.UserID = bookingtb.UserID;


        //                                    book.BookingName = articleName;
        //                                    book.ServiceID = i.RelatedMainID;
        //                                    book.IsFoodOrder = bookingtb.IsFoodOrder;
        //                                    book.FromDate = fromdate;
        //                                    book.ToDate = todate;
        //                                    book.ServiceType = bookingtb.PropertyServiceId;
        //                                    book.SendMessageType = bookingtb.SendMessageType;
        //                                    book.Status = 99;
        //                                    book.Quantity = bookingPriceDetail.Quantity;
        //                                    //book.UnitText = bookingPriceDetail.UnitText;
        //                                    //book.UnitPrice = bookingPriceDetail.UnitPrice;
        //                                    //book.DiscountPercent = bookingPriceDetail.DiscountPercent;
        //                                    //book.VATPercent = bookingPriceDetail.VATPercent;
        //                                    //book.NetAmount = _bookingDetail.NetAmount;
        //                                    //book.Amount = _bookingDetail.Amount;

        //                                    book.MainBookingId = _bookingDetail.BookingID;
        //                                    book.Ordering = bookingtb.BookOrderName;
        //                                    book.Customer = bookingtb.CompanyPayer;
        //                                    if (!string.IsNullOrEmpty(bookingtb.FollowDate))
        //                                    {
        //                                        book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                    }
        //                                    else
        //                                    {
        //                                        book.FollowUpDate = null;
        //                                    }
        //                                    // book.FollowUpDate = Convert.ToDateTime(bookingtb.FollowDate);
        //                                    if (bookingtb.foods.Count > 0)
        //                                    {
        //                                        bool isService = true;
        //                                        List<BookingServiceList> flist = new List<BookingServiceList>();
        //                                        foreach (var item in bookingtb.foods)
        //                                        {
        //                                            _foodDetail = new BookingServiceList();
        //                                            if (isService)
        //                                            {

        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsVat = item.IsVatApply;

        //                                                _foodDetail.UnitText = bookingPriceDetail.UnitText;
        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);

        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = 0;

        //                                                }
        //                                                if(!string.IsNullOrEmpty(item.MainServiceId))
        //                                                {
        //                                                    _foodDetail.FoodID = Convert.ToInt32(item.MainServiceId);
        //                                                }
        //                                                {
        //                                                    _foodDetail.FoodID = 0;
        //                                                }
        //                                                _foodDetail.VatType = new ArticlesController().getVatType(_foodDetail.FoodID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));

        //                                                if (item.IsVatApply)
        //                                                {
        //                                                    _foodDetail.VATPercent = 0;
        //                                                }
        //                                                else
        //                                                {
        //                                                    //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                                    _foodDetail.VATPercent = 0;
        //                                                }

        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                                // _foodDetail.

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;
        //                                                isService = false;
        //                                            }
        //                                            else
        //                                            {
        //                                                _foodDetail.FoodID = item.FoodId;
        //                                                _foodDetail.Qty = item.Qty;
        //                                                _foodDetail.Status = false;
        //                                                _foodDetail.Sum = item.Sum;
        //                                                _foodDetail.Tekst = item.Tekst;
        //                                                _foodDetail.IsKitchen = item.IsKitchenOrder;
        //                                                _foodDetail.IsVat = item.IsVatApply;
        //                                                var PriceUnit = getBookingPriceDetail(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null);
        //                                                _foodDetail.UnitText = PriceUnit.UnitText;
        //                                                _foodDetail.VatType = new ArticlesController().getVatType(item.FoodId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatMasterFormID"].ToString()));
        //                                                //  _foodDetail.VATPercent = new ArticlesController().getVatTypeValue(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_VatTypeFormID"].ToString()), _foodDetail.VatType??0);
        //                                                _foodDetail.VATPercent = 0;
        //                                                _foodDetail.NetAmount = item.Sum;
        //                                                _foodDetail.Amount = _foodDetail.NetAmount * (100 - _foodDetail.VATPercent * 0) / 100;
        //                                                // _foodDetail.
        //                                                if (!string.IsNullOrEmpty(item.ArticleId))
        //                                                {
        //                                                    _foodDetail.ArticleId = Convert.ToInt32(item.ArticleId);
        //                                                }
        //                                                else
        //                                                {
        //                                                    _foodDetail.ArticleId = 0;
        //                                                }

        //                                                //  var foodPrice = new ArticlesController().getFoodPrice(item.FoodId, bookingtb.FoodFormId);
        //                                                _foodDetail.Price = item.Price;




        //                                            }

        //                                            flist.Add(_foodDetail);

        //                                        }
        //                                        entity.BookingServiceLists.AddRange(flist);
        //                                        // entity.SaveChanges();
        //                                    }


        //                                    book.BookingType = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.PropertyServiceId).Select(s => s.MenuItemName).FirstOrDefault();
        //                                    entity.BookingDetails.Add(book);
        //                                    entity.SaveChanges();
        //                                }

        //                            }


        //                            //    scope.Complete();
        //                            //}
        //                            //send message
        //                            if (_bookingDetail.UserID != 0)
        //                            {

        //                                var user = DMBase.Core.ContentModel.FetchPersonById(_bookingDetail.UserID);
        //                                SendBookingMessage(_bookingDetail, user, "new");
        //                            }
        //                            message = "Booking Succesfully Saved";
        //                        }


        //                    }





        //                    dbContextTransaction.Commit();
        //                }
        //                catch (System.Data.Entity.Validation.DbEntityValidationException e)
        //                {
        //                    foreach (var eve in e.EntityValidationErrors)
        //                    {
        //                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //                        foreach (var ve in eve.ValidationErrors)
        //                        {
        //                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
        //                                ve.PropertyName, ve.ErrorMessage);
        //                        }
        //                    }
        //                    dbContextTransaction.Rollback();
        //                    throw;
        //                    // throw new Exception(e.Message + ":" + e.StackTrace);
        //                }

        //            }

        //            }

        //        }





        //    return new BookingResponse { data = message };
        //}

        [HttpGet]
        [ActionName("DeleteBooking")]
        public BookingResponse DeleteBooking(int bookingId,bool isDeleteCalender)
        {
            string message = "";
            int type = 0;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                try
                {
                    List<BookingServiceList> foodData = new List<BookingServiceList>();  
                    var bookingData = entity.BookingDetails.Where(w => w.BookingID == bookingId).Select(s => s).FirstOrDefault();
                    if (bookingData != null)
                    {
                        if (bookingData.BookingServiceLists.Count > 0)
                        {
                            foodData = entity.BookingServiceLists.Where(w => w.BookingID == bookingId).ToList();
                        }
                        var relatedRooms = entity.Articles.Join(entity.Article_Article, a => a.ArticleID, aa => aa.MainID, (A, AA) => new { a = A, aa = AA })
                              .Where(w => w.aa.MainID == bookingData.ServiceID).Select(s => s.aa).ToList();
                        if (isDeleteCalender)
                        {
                            if (foodData.Count > 0)
                            {
                                foreach (var item in foodData)
                                {
                                    
                                        item.IsKitchen = false;
                                }
                            }
                            bookingData.ServiceID = 0;

                            entity.SaveChanges();
                        }
                        else
                        {
                            if (bookingData.BookingServiceLists.Count > 0)
                            {
                                if (foodData.Count > 0)
                                {
                                    entity.BookingServiceLists.RemoveRange(foodData);
                                }
                                //var foodData = entity.BookingServiceLists.Where(w => w.BookingID == bookingId).ToList();
                                //foreach (var item in bookingData.FoodServices)
                                //{
                                //    entity.FoodServices.Remove(item);
                                //    entity.SaveChanges();
                                //}
                              
                            }
                            entity.BookingDetails.Remove(bookingData);
                            entity.SaveChanges();
                        }

                        if (relatedRooms.Count > 0)
                        {
                            foreach (var i in relatedRooms)
                            {
                                var subBooking = entity.BookingDetails.Where(w => w.ServiceID == i.RelatedMainID && w.MainBookingId == bookingId).Select(s => s).FirstOrDefault();

                                //if (subBooking.BookingServiceLists.Count > 0)
                                //{
                                //    var fData = entity.BookingServiceLists.Where(w => w.BookingID == subBooking.BookingID).ToList();
                                //    //foreach (var item in bookingData.FoodServices)
                                //    //{
                                //    //    entity.FoodServices.Remove(item);
                                //    //    entity.SaveChanges();
                                //    //}
                                //    entity.BookingServiceLists.RemoveRange(fData);
                                //}
                                if (isDeleteCalender)
                                {
                                    subBooking.ServiceID = 0;
                                }
                                else
                                {
                                    entity.BookingDetails.Remove(subBooking);
                                }
                                   
                                entity.SaveChanges();
                            }

                        }
                        type = (int)ErrorType.Success;
                        message = "Delete Successfully";
                    }
                   
                   

                }
                catch (Exception)
                {
                    type = (int)ErrorType.Error; ;
                    message = "Delete UnSuccessfully";
                }
            }
            return new BookingResponse { data = message, errorType = type };
        }

        // 1st version
        //[HttpGet]
        //[ActionName("getBookingExternalPriceDetail")]
        //public BookingUnitPriceDetail getBookingExternalPriceDetail(bool isInternal,int NoOfPerson, string fromDate, string toDate, string fTimer, string tTimer) {
        //    BookingUnitPriceDetail bm = new BookingUnitPriceDetail();
        //    try
        //    {
        //        DateTime fromdate = new DateTime();
        //        DateTime todate = new DateTime();
        //        using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
        //        {
        //            if (fromDate != null)
        //            {
        //                var fdate = Convert.ToDateTime(fromDate);
        //                if (fTimer != null)
        //                {
        //                    TimeSpan fTime = TimeSpan.Parse(fTimer);
        //                    fromdate = fdate.Add(fTime);
        //                }
        //            }
        //            if (toDate != null)
        //            {
        //                var tdate = Convert.ToDateTime(toDate);
        //                if (tTimer != null)
        //                {
        //                    TimeSpan tTime = TimeSpan.Parse(tTimer);
        //                     todate = tdate.Add(tTime);
        //                }
        //            }

        //            TimeSpan diff = todate - fromdate;
        //            double hours = diff.TotalHours;

        //            var result = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && NoOfPerson >= w.From && NoOfPerson <= w.To).FirstOrDefault();
        //            if (result != null)
        //            {
        //                double qty = 0;
        //                double price = 0;

        //                if (result.UnitId == 1)
        //                {
        //                    if (hours == 0)
        //                    {
        //                        qty = NoOfPerson;
        //                    }
        //                    else
        //                    {
        //                        qty = hours * NoOfPerson;
        //                    }
        //                }
        //                else
        //                {
        //                    qty = NoOfPerson;
        //                }

        //                var sum =  qty * Convert.ToDouble(result.Price);
        //                if (sum >= 20000)
        //                {
        //                    qty = 1;
        //                    price = 20000;
        //                }
        //                else
        //                {
        //                    price = Convert.ToDouble(result.Price);
        //                }
        //                if (isInternal)
        //                {
        //                    bm.UnitPrice = price;
        //                    bm.SecondaryPrice = 0;
        //                }
        //                else
        //                {
        //                    bm.SecondaryPrice = price;
        //                    bm.UnitPrice = 0;
        //                }

        //                bm.Quantity = qty;
        //                bm.Article1 = result.Article1;
        //                bm.Article2 = result.Article2;
        //                bm.CostPrice1 = 0;
        //                bm.CostPrice2 = 0;
        //                bm.IsMutiplePrice = true;
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        //        {
        //            Content = new StringContent(string.Format(ex.Message)),
        //            ReasonPhrase = "Price calculation has issue!!"
        //        };

        //        throw new HttpResponseException(response);
        //    }
        //    return bm ;
        //}

        //2nd version
        //[HttpGet]
        //[ActionName("getBookingExternalPriceDetail")]
        //public BookingUnitPriceDetail getBookingExternalPriceDetail(bool isInternal, int NoOfPerson, string fromDate, string toDate, string fTimer, string tTimer)
        //{
        //    BookingUnitPriceDetail bm = new BookingUnitPriceDetail();
        //    try
        //    {
        //        DateTime fromdate = new DateTime();
        //        DateTime todate = new DateTime();
        //        using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
        //        {
        //            if (fromDate != null)
        //            {
        //                var fdate = Convert.ToDateTime(fromDate);
        //                if (fTimer != null)
        //                {
        //                    TimeSpan fTime = TimeSpan.Parse(fTimer);
        //                    fromdate = fdate.Add(fTime);
        //                }
        //            }
        //            if (toDate != null)
        //            {
        //                var tdate = Convert.ToDateTime(toDate);
        //                if (tTimer != null)
        //                {
        //                    TimeSpan tTime = TimeSpan.Parse(tTimer);
        //                    todate = tdate.Add(tTime);
        //                }
        //            }

        //            TimeSpan diff = todate - fromdate;
        //            double hours =  diff.TotalHours;
        //            double qty = 0;
        //            double price = 0;

        //            if (isInternal)
        //            {
        //                var result = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && NoOfPerson >= w.From && NoOfPerson <= w.To).FirstOrDefault();
        //                if (result != null)
        //                {


        //                    if (result.UnitId == 1)
        //                    {
        //                        if (hours == 0)
        //                        {
        //                            qty = NoOfPerson;
        //                        }
        //                        else
        //                        {
        //                            qty =  hours * NoOfPerson;
        //                        }
        //                    }


        //                    var sum = qty * Convert.ToDouble(result.Price);
        //                    if (sum >= 20000)
        //                    {
        //                        qty = 1;
        //                        price = 20000;
        //                    }
        //                    else
        //                    {
        //                        price = Convert.ToDouble(result.Price);
        //                    }

        //                        bm.UnitPrice = price;
        //                        bm.SecondaryPrice = 0;
        //                        bm.Article1 = result.Article1;
        //                        bm.Article2 = result.Article2;




        //                }
        //            }
        //            else
        //            {
        //                var result = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && hours >= w.From && hours <= w.To).FirstOrDefault();
        //                if (result != null)
        //                {
        //                    if (result.UnitId == 2)
        //                    {
        //                        qty = NoOfPerson;

        //                    }
        //                    var sum = qty * Convert.ToDouble(result.Price);
        //                    if (sum >= 20000)
        //                    {
        //                        qty = 1;
        //                        price = 20000;
        //                    }
        //                    else
        //                    {
        //                        price = Convert.ToDouble(result.Price);
        //                    }
        //                    bm.SecondaryPrice = price;
        //                    bm.UnitPrice = 0;
        //                    bm.Article1 = result.Article1;
        //                    bm.Article2 = result.Article2;
        //                }
        //            }
        //            bm.Quantity = qty;
        //            bm.CostPrice1 = 0;
        //            bm.CostPrice2 = 0;
        //            bm.IsMutiplePrice = true;


        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        //        {
        //            Content = new StringContent(string.Format(ex.Message)),
        //            ReasonPhrase = "Price calculation has issue!!"
        //        };

        //        throw new HttpResponseException(response);
        //    }
        //    return bm;
        //}

        [HttpGet]
        [ActionName("getBookingExternalPriceDetail")]
        public BookingUnitPriceDetail getBookingExternalPriceDetail(bool isInternal, int NoOfPerson, string fromDate, string toDate, string fTimer, string tTimer)
        {
            BookingUnitPriceDetail bm = new BookingUnitPriceDetail();
            try
            {
                DateTime fromdate = new DateTime();
                DateTime todate = new DateTime();
                BookingPriceDetail bookdetail;
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    if (fromDate != null)
                    {
                        var fdate = Convert.ToDateTime(fromDate);
                        if (fTimer != null)
                        {
                            TimeSpan fTime = TimeSpan.Parse(fTimer);
                            fromdate = fdate.Add(fTime);
                        }
                    }
                    if (toDate != null)
                    {
                        var tdate = Convert.ToDateTime(toDate);
                        if (tTimer != null)
                        {
                            TimeSpan tTime = TimeSpan.Parse(tTimer);
                            todate = tdate.Add(tTime);
                        }
                    }

                    TimeSpan diff = todate - fromdate;
                    double hours = diff.TotalHours;
                    double qty = 0;
                    double price = 0;
                   
                    if (isInternal)
                    {
                        bookdetail = new BookingPriceDetail();
                        if (NoOfPerson >= 20)
                        {
                            bookdetail = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && NoOfPerson >= w.From && NoOfPerson <= w.To).FirstOrDefault();

                        }
                        else
                        {
                            bookdetail = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && hours >= w.From && hours <= w.To).FirstOrDefault();
                        }
                       
                        if (bookdetail != null)
                        {


                            if (bookdetail.UnitId == 1)
                            {
                                if (hours == 0)
                                {
                                    qty = NoOfPerson;
                                }
                                else
                                {
                                    qty = hours * NoOfPerson;
                                }
                            }


                            var sum = qty * Convert.ToDouble(bookdetail.Price);
                            if (sum >= 20000)
                            {
                                qty = 1;
                                price = 20000;
                            }
                            else
                            {
                                price = Convert.ToDouble(bookdetail.Price);
                            }

                            bm.UnitPrice = price;
                            bm.SecondaryPrice = 0;
                            bm.Article1 = bookdetail.Article1;
                            bm.Article2 = bookdetail.Article2;




                        }
                    }
                    else
                    {
                        var result = entity.BookingPriceDetails.Where(w => w.IsInternal == isInternal && hours >= w.From && hours <= w.To).FirstOrDefault();
                        if (result != null)
                        {
                            if (result.UnitId == 2)
                            {
                                qty = NoOfPerson;

                            }
                            var sum = qty * Convert.ToDouble(result.Price);
                            if (sum >= 20000)
                            {
                                qty = 1;
                                price = 20000;
                            }
                            else
                            {
                                price = Convert.ToDouble(result.Price);
                            }
                            bm.SecondaryPrice = price;
                            bm.UnitPrice = 0;
                            bm.Article1 = result.Article1;
                            bm.Article2 = result.Article2;
                        }
                    }
                    bm.Quantity = qty;
                    bm.CostPrice1 = 0;
                    bm.CostPrice2 = 0;
                    bm.IsMutiplePrice = true;


                }
            }
            catch (Exception ex)
            {

                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format(ex.Message)),
                    ReasonPhrase = "Price calculation has issue!!"
                };

                throw new HttpResponseException(response);
            }
            return bm;
        }
        [HttpGet]
        [ActionName("getBookingPriceDetail")]
        public BookingUnitPriceDetail getBookingPriceDetail(int articleId, int FormID, string fromDate, string toDate, string fTimer, string tTimer,bool isInternal,int noOfPerson)
        {

            DateTime fromdate = new DateTime();
            DateTime todate = new DateTime();
            BookingUnitPriceDetail priceDetail = new BookingUnitPriceDetail();
            try
            {
                if (fromDate != null)
                {
                    var fdate = Convert.ToDateTime(fromDate);
                    if (fTimer != null)
                    {
                        TimeSpan fTime = TimeSpan.Parse(fTimer);
                        fromdate = fdate.Add(fTime);
                    }



                }
                if (toDate != null)
                {
                    var tdate = Convert.ToDateTime(toDate);
                    if (tTimer != null)
                    {
                        TimeSpan tTime = TimeSpan.Parse(tTimer);
                        todate = tdate.Add(tTime);
                    }

                }


                bool IsMutiplePrice = IsMutiplePricecalculation(articleId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ServiceFormID"].ToString()));
                priceDetail.IsMutiplePrice = IsMutiplePrice;
                if (IsMutiplePrice)
                {
                    priceDetail = getBookingExternalPriceDetail(isInternal, noOfPerson, fromDate, toDate, fTimer, tTimer);

                }
                else
                {
                    SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
                    DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

                    if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                    {
                        string unit = dsContentExtension.Tables[1].Rows[0]["unit"].ToString();

                        if (unit.ToLower().Equals("hours"))
                        {
                            var hours = (todate - fromdate).TotalHours;

                            foreach (DataRow item in dsContentExtension.Tables[1].Rows)
                            {
                                if (hours >= Convert.ToInt32(item["from"]) && hours <= Convert.ToInt32(item["to"]))
                                {
                                    if (item["unitText"] != null)
                                    {
                                        priceDetail.UnitText = item["unitText"].ToString();
                                    }

                                    priceDetail.Quantity = hours;
                                    if (item["price1"] != null)
                                    {
                                        priceDetail.UnitPrice = Convert.ToDouble(item["price1"]);
                                    }
                                    if (item["costPrice1"] != null)
                                    {
                                        priceDetail.CostPrice1 = Convert.ToDouble(item["costPrice1"]);
                                    }
                                    if (item["costPrice2"] != null)
                                    {
                                        priceDetail.CostPrice2 = Convert.ToDouble(item["costPrice2"]);
                                    }
                                    if (item["price2"] != null)
                                    {
                                        priceDetail.SecondaryPrice = Convert.ToDouble(item["price2"]);
                                    }
                                    if (item["Article1"] != null)
                                        if (item["Article1"] != null)
                                        {
                                            priceDetail.Article1 = item["Article1"].ToString();
                                        }
                                    if (item["Article2"] != null)
                                    {
                                        priceDetail.Article2 = item["Article2"].ToString();
                                    }

                                    //  priceDetail.DiscountPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["discountPercent"].ToString());
                                    // priceDetail.NetAmount = priceDetail.UnitPrice * (100 - priceDetail.DiscountPercent) / 100;
                                    //  priceDetail.VATPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["vatPercent"].ToString());
                                    //  priceDetail.Amount = priceDetail.NetAmount * (100 - priceDetail.VATPercent * 0) / 100;

                                    break;
                                }
                            }
                        }
                        else if (unit.ToLower().Equals("perhour"))
                        {
                            if (todate != null && fromdate != null)
                            {
                                var hours = Math.Ceiling((todate - fromdate).TotalHours);

                                priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                                priceDetail.Quantity = (todate - fromdate).TotalHours;
                                priceDetail.UnitPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price1"].ToString());
                                if (dsContentExtension.Tables[1].Rows[0]["price2"].ToString() != null)
                                {
                                    priceDetail.SecondaryPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price2"]);
                                }
                                if (dsContentExtension.Tables[1].Rows[0]["costPrice1"].ToString() != null)
                                {
                                    priceDetail.CostPrice1 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice1"]);
                                }
                                if (dsContentExtension.Tables[1].Rows[0]["costPrice2"].ToString() != null)
                                {
                                    priceDetail.CostPrice2 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice2"]);
                                }

                                priceDetail.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                                priceDetail.Article2 = dsContentExtension.Tables[1].Rows[0]["Article2"].ToString();
                            }
                            else
                            {
                                priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                                priceDetail.UnitPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price1"].ToString());
                                if (dsContentExtension.Tables[1].Rows[0]["price2"].ToString() != null)
                                {
                                    priceDetail.SecondaryPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price2"]);
                                }
                                if (dsContentExtension.Tables[1].Rows[0]["costPrice1"].ToString() != null)
                                {
                                    priceDetail.CostPrice1 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice1"]);
                                }
                                if (dsContentExtension.Tables[1].Rows[0]["costPrice2"].ToString() != null)
                                {
                                    priceDetail.CostPrice2 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice2"]);
                                }
                                priceDetail.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                                priceDetail.Article2 = dsContentExtension.Tables[1].Rows[0]["Article2"].ToString();


                            }
                            // priceDetail.DiscountPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["discountPercent"].ToString());
                            //  priceDetail.NetAmount = hours * priceDetail.UnitPrice * (100 - priceDetail.DiscountPercent) / 100;
                            //  priceDetail.VATPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["vatPercent"].ToString());
                            // priceDetail.Amount = priceDetail.NetAmount * (100 - priceDetail.VATPercent * 0) / 100;
                        }
                        else if (unit.ToLower().Equals("permonth"))
                        {
                            // var months = (toDate.Year * 12 + toDate.Month) - (fromDate.Year * 12 + fromDate.Month);
                            var months = Math.Ceiling(todate.Subtract(fromdate).Days / (365.25 / 12));
                            priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                            priceDetail.Quantity = todate.Subtract(fromdate).Days / (365.25 / 12);
                            priceDetail.UnitPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price1"].ToString());
                            if (dsContentExtension.Tables[1].Rows[0]["price2"].ToString() != null)
                            {
                                priceDetail.SecondaryPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price2"]);
                            }
                            if (dsContentExtension.Tables[1].Rows[0]["costPrice1"].ToString() != null)
                            {
                                priceDetail.CostPrice1 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice1"]);
                            }
                            if (dsContentExtension.Tables[1].Rows[0]["costPrice2"].ToString() != null)
                            {
                                priceDetail.CostPrice2 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice2"]);
                            }
                            priceDetail.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                            priceDetail.Article2 = dsContentExtension.Tables[1].Rows[0]["Article2"].ToString();
                            //  priceDetail.DiscountPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["discountPercent"].ToString());
                            //  priceDetail.NetAmount = months * priceDetail.UnitPrice * (100 - priceDetail.DiscountPercent) / 100;
                            //  priceDetail.VATPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["vatPercent"].ToString());
                            //  priceDetail.Amount = priceDetail.NetAmount * (100 - priceDetail.VATPercent * 0) / 100;

                        }
                        else if (unit.ToLower().Equals("pris"))
                        {
                            priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                            priceDetail.UnitPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price1"].ToString());
                            if (dsContentExtension.Tables[1].Rows[0]["price2"].ToString() != null)
                            {
                                priceDetail.SecondaryPrice = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["price2"]);
                            }
                            if (dsContentExtension.Tables[1].Rows[0]["costPrice1"].ToString() != null)
                            {
                                priceDetail.CostPrice1 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice1"]);
                            }
                            if (dsContentExtension.Tables[1].Rows[0]["costPrice2"].ToString() != null)
                            {
                                priceDetail.CostPrice2 = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["costPrice2"]);
                            }
                            priceDetail.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                            priceDetail.Article2 = dsContentExtension.Tables[1].Rows[0]["Article2"].ToString();


                        }
                        else
                        {

                            // var months = (toDate.Year * 12 + toDate.Month)-(fromDate.Year*12+ fromDate.Month);
                            var months = todate.Subtract(fromdate).Days / (365.25 / 12);
                            // var n= toDate.Subtract(fromDate).Days / 30;
                            foreach (DataRow item in dsContentExtension.Tables[1].Rows)
                            {
                                if (months >= Convert.ToInt32(item["from"]) && months <= Convert.ToInt32(item["to"]))
                                {
                                    priceDetail.UnitText = item["unitText"].ToString();
                                    priceDetail.Quantity = months;
                                    priceDetail.UnitPrice = Convert.ToDouble(item["price1"]);
                                    priceDetail.SecondaryPrice = Convert.ToDouble(item["price2"]);
                                    priceDetail.CostPrice1 = Convert.ToDouble(item["costPrice1"]);
                                    priceDetail.CostPrice2 = Convert.ToDouble(item["costPrice2"]);
                                    priceDetail.Article2 = item["Article2"].ToString();
                                    priceDetail.Article1 = item["Article1"].ToString();
                                    //  priceDetail.DiscountPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["discountPercent"].ToString());
                                    //  priceDetail.NetAmount = priceDetail.UnitPrice * (100 - priceDetail.DiscountPercent) / 100;
                                    //  priceDetail.VATPercent = Convert.ToDouble(dsContentExtension.Tables[1].Rows[0]["vatPercent"].ToString());
                                    // priceDetail.Amount = priceDetail.NetAmount * (100 - priceDetail.VATPercent * 0) / 100;

                                    break;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format(ex.Message)),
                    ReasonPhrase = "Price calculation has issue!!"
                };

                throw new HttpResponseException(response);
            }
           
            return priceDetail;
        }

       
        [HttpGet]
        [ActionName("getServiceIsMutiplePrice")]
        public bool IsMutiplePricecalculation(int articleId,int formId ) {
            bool isVal = false;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { formId.ToString() });
            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
               if(dsContentExtension.Tables[1].Rows[0]["ismultiple"] != null)
                {
                    isVal = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["ismultiple"].ToString());
                }
            }
                return isVal;
        }
        [HttpGet]
        [ActionName("getServiceVatArticles")]
        public BookingUnitPriceDetail GetVatArticles(int articleId, int FormID)
        {
            BookingUnitPriceDetail priceDetail = new BookingUnitPriceDetail();
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                if (dsContentExtension.Tables[1].Rows[0]["Article1"] != null)
                {
                    priceDetail.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                }
                if (dsContentExtension.Tables[1].Rows[0]["Article2"] != null)
                {
                    priceDetail.Article2 = dsContentExtension.Tables[1].Rows[0]["Article2"].ToString();
                }

            }
            return priceDetail;
        }
        //public BookingUnitPriceDetail getBookingPriceDetail(int articleId, int FormID,DateTime fromDate, DateTime toDate)
        //{
        //    BookingUnitPriceDetail priceDetail = new BookingUnitPriceDetail();
        //    SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
        //    DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

        //    if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
        //    {

        //        foreach (DataRow item in dsContentExtension.Tables[1].Rows)
        //        {


        //            if (item["unitText"].ToString().ToLower().Equals("perhour"))
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                if (item["unit"].ToString().ToLower().Equals("hours"))
        //                {
        //                    var hours = (toDate - fromDate).Hours;
        //                    if(hours>=Convert.ToInt32(item["from"]) && hours<= Convert.ToInt32(item["to"]))
        //                    {
        //                        priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
        //                        priceDetail.Quantity = hours;
        //                        priceDetail.UnitPrice = Convert.ToDouble(item["price"]);
        //                        break;
        //                    }

        //                }
        //                else
        //                {

        //                }
        //                priceDetail.From = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["from"].ToString());
        //                priceDetail.To = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["to"].ToString());

        //            }
        //        }
        //        priceDetail.Unit = dsContentExtension.Tables[1].Rows[0]["unit"].ToString();
        //        priceDetail.UnitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
        //        priceDetail.From = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["from"].ToString());
        //        priceDetail.To = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["to"].ToString());
        //        priceDetail.UnitPrice= Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["price"].ToString());
        //    }

        //    return priceDetail;
        //}

        private string SaveingBookingData(BookingTB bookingtb)
        {
            int result = 0;
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.sel_UserRegistration", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                //cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50).Value = UserId.ToString();
                //cmd.Parameters.Add("@Password", SqlDbType.NVarChar, 50).Value = Password.ToString();
                //cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar, -1).Value = DeviceId.ToString();
                //cmd.Parameters.Add("@PlatformType", SqlDbType.Int).Value = Convert.ToInt32(PlatformType);
                // open connection, execute command, close connection
                conn.Open();
                result = (int)cmd.ExecuteScalar();
                conn.Close();

            }
            if (result == 1)
                return "1";
            else
                return "0";
        }
        [HttpGet]
        public BookingDetail GetCurrentBookingDetail(int bookingId)
        {
            BookingDetail booking;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                booking = entity.BookingDetails.Where(w => w.BookingID == bookingId).Select(s => s).FirstOrDefault();



            }
            return booking;
        }

        /**
         * ActionType: new/update/cancel
         * 
         * Configuration: 
         * BookingEmailnew, BookingEmailupdate
         * BookingEmailFrom
         * 
         * BookingSMSnew, BookingSMSupdate
         * "sms from" is configured in sms related setting.
         * 
         * //todo: error handling
         * //todo: support cancel.
         */

        protected static void SendBookingMessage(BookingDetail booking, PersonViewData.PersonRow user, string ActionType = "new")
        {
            if (booking.SendMessageType == 3 || booking.SendMessageType == 1)
            {
                var successMessageEmail = ConfigurationManager.AppSettings["BookingEmail" + ActionType];
                var successMessageEmailTitle = ConfigurationManager.AppSettings["BookingEmailTitle" + ActionType];
                var emailFrom = ConfigurationManager.AppSettings["BookingEmailFrom"];
                var userEmail = user.Email;
                var title = WashBookingMessage(successMessageEmailTitle, userEmail, booking);
                var message = WashBookingMessage(successMessageEmail, userEmail, booking);

                Message.SendEmail(userEmail, title, message, null, booking.UserID.ToString(), null, true);
            }
            if (booking.SendMessageType == 3 || booking.SendMessageType == 2)
            {
                var successMessageSMS = ConfigurationManager.AppSettings["BookingSMS" + ActionType];
                var phoneNo = user.MobilePhone;
                var message = WashBookingMessage(successMessageSMS, phoneNo, booking);
                Message.SendSMS(phoneNo, message, null, booking.UserID.ToString(), true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endtime"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionName("getBookingPriceDetail")]
        private List<string> getTimerList(string startTime, string endtime, int interval)
        {

            List<String> timer = new List<string>();
            DateTime aDay = DateTime.Now;
            //  aDay.AddMinutes(15);
            timer.Add(aDay.AddMinutes(15).ToString());
            return timer;
        }
        /**
         * Wash message 'template' with booking variable.
         */

        protected static string WashBookingMessage(string message, string to, BookingDetail booking)
        {
            var washedMessage = Util.WashContent(message, new Dictionary<string, string>() {
                                                                                { "to", to },
                                                                                { "booking_id", booking.BookingID.ToString() },
                                                                                { "booking_name" , booking.BookingName },
                                                                                { "booking_from" , booking.FromDate.ToString() },
                                                                                { "booking_to" , booking.ToDate.ToString() }
                                                                                });
            return washedMessage;
        }

        public string getUserName(int userId)
        {
            string userName = string.Empty;
            try
            {
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    userName = entity.People.Where(w => w.PersonID == userId).Select(s => s.DisplayName).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return userName;
        }

        [HttpGet]
        [ActionName("IsCustomerExist")]
        public int IsCustomerExist(string customerNo)
        {
            var result = new AccountController().CheckOrganizationNumber(customerNo);
            return result;
        }

      
    }
}