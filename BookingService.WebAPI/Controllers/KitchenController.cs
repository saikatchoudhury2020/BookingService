using BookingService.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Controllers;
using WebAPI.DTO;
using Tripletex;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WebAPI.Models;

namespace BookingService.WebAPI.Controllers
{
    public class KitchenController : ApiController
    {
        protected void CheckAccess()
        {
            var kitchenRoleID = Digimaker.Config.Custom.Get("KitchenRoleID");
            var hasAccess = UserAccess.HasAccessToRole(int.Parse(kitchenRoleID));
            if (!hasAccess)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        [ActionName("UpdateDelivered")]
        public int UpdateDelivered(int OrderNo)
        {
            this.CheckAccess();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var query =
                                 from ord in entity.BookingDetails
                                 where ord.BookingID == OrderNo
                                 select ord;

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (var ord in query)
                        {
                            ord.IsDeliver = true;
                            // Insert any additional changes to column values.
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();

                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
            return 1;
        }

        [HttpGet]
        [ActionName("UpdateClean")]
        public async Task<ApiResponse> UpdateClean(int OrderNo)
        {
            this.CheckAccess();
            ApiResponse response = new ApiResponse();
            var rsp = await orderAsync(OrderNo);
            if (rsp.errorType == 1)
            {
                return rsp;
            }
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var query =
                                 from ord in entity.BookingDetails
                                 where ord.BookingID == OrderNo
                                 select ord;

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (var ord in query)
                        {
                            ord.IsClean = true;
                            // Insert any additional changes to column values.
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();

                        response.message = ex.Message;
                        response.errorType = (int)Enums.ErrorType.Error;
                        return response;
                    }
                }
            }
            response.message = "Successfully Clean";
            response.errorType = (int)Enums.ErrorType.Success;
            return response;
        }

        [HttpGet]
        [ActionName("GetKitchenDisplay")]
        public KitchenDisplay GetKitchenDisplay(DateTime TodayDate, int buildingid)
        {
            this.CheckAccess();
            DateTime TodayDate1 = TodayDate.Date;// System.DateTime.Now.Date;
            KitchenDisplay kitchenDisplay = new KitchenDisplay();


            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                // Food Not Cleaned List
                List<FoodNotCleanedList> notcleanedList = new List<FoodNotCleanedList>();
                FoodNotCleanedList notclean;
                var result = from ord in entity.BookingDetails
                             where ord.BuildingID == buildingid && DbFunctions.TruncateTime(ord.FromDate) == TodayDate1 && ord.IsClean == false && ord.IsDeliver == true && ord.BookingServiceLists.Count(s => s.IsKitchen == true) > 0
                             select ord;

                foreach (var item in result)
                {
                    notclean = new FoodNotCleanedList();
                    //string text = entity.BookingServiceLists.Where(w => w.BookingID == item.BookingID && w.IsMainService == true).Select(s => s.Tekst).FirstOrDefault();
                    string text = entity.Articles.Where(w => w.MainID == item.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                    notclean.OrderNo = item.BookingID;
                    notclean.Text = text + " " + item.BookingName;
                    notclean.Todate = item.ToDate;
                    notclean.Fromdate = item.FromDate;
                    notclean.NoOFPeople = item.NoOfPeople;
                    notcleanedList.Add(notclean);
                }
                kitchenDisplay.FoodNotCleanedList = notcleanedList;

                // Food Not Delivered List
                List<FoodNotDeliveredList> notdeliveredList = new List<FoodNotDeliveredList>();
                FoodNotDeliveredList notdelivered;
                FoodList food;
                var res = from ord in entity.BookingDetails
                          where ord.BuildingID == buildingid && DbFunctions.TruncateTime(ord.FromDate) == TodayDate1 && ord.IsClean == false && ord.IsDeliver == false && ord.BookingServiceLists.Count(s => s.IsKitchen == true) > 0
                          select ord;
                foreach (var item in res)
                {
                    notdelivered = new FoodNotDeliveredList();
                    List<FoodList> foodList = new List<FoodList>();
                    //string text = entity.BookingServiceLists.Where(w => w.BookingID == item.BookingID && w.IsMainService == true).Select(s => s.Tekst).FirstOrDefault();
                    string text = entity.Articles.Where(w => w.MainID == item.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                    notdelivered.OrderNo = item.BookingID;
                    notdelivered.Text = text + " " + item.BookingName;
                    notdelivered.Todate = item.ToDate;
                    notdelivered.Fromdate = item.FromDate;
                    notdelivered.NoOFPeople = item.NoOfPeople;

                    //// food list
                    var resfood = from ord in entity.BookingServiceLists
                                  where ord.BookingID == item.BookingID && ord.IsKitchen == true && ord.Status == false
                                  select ord;
                    bool isFoodservice = false;
                    foreach (var it in resfood)
                    {
                        food = new FoodList();
                        food.Text = it.Tekst;
                        food.Quantity = it.Qty;
                        food.DeliverTime = it.Time;
                        foodList.Add(food);
                        isFoodservice = true;

                    }
                    if (isFoodservice)
                    {
                        notdelivered.FoodList = foodList;
                        notdeliveredList.Add(notdelivered);
                    }
                }
                kitchenDisplay.FoodNotDeliveredList = notdeliveredList;
            }
            return kitchenDisplay;
        }

        public async Task<ApiResponse> orderAsync(int OrderNo)
        {
            ApiResponse response = new ApiResponse();
            using (var session = new TripletexSession())
            {
                try
                {
                    var consumerKey = Digimaker.Config.Custom.AppSettings["consumerKey"].ToString();
                    var employeeKey = Digimaker.Config.Custom.AppSettings["employeeKey"].ToString();
                    var customerNumber = Digimaker.Config.Custom.AppSettings["CustomerNo"].ToString();
                    // var productNumber = CloudConfigurationManager.GetSetting("TripletexProductNumber");

                    if (await session.CreateSessionToken(consumerKey, employeeKey))
                    {
                        using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                        {
                            //// check the Product Number
                            var resfood1 = from ord in entity.BookingServiceLists where ord.BookingID == OrderNo && ord.IsKitchen == true && ord.Status == false select ord;
                            foreach (var it in resfood1)
                            {
                                if (it.ArticleId != "0")
                                {
                                    var product1 = await session.GetProductFromNumberChecking(it.ArticleId, "id", "name", "vatType(id)", "priceExcludingVatCurrency");
                                    if (product1 == 0)
                                    {
                                        response.message = "Article id does not exist in Tripletex, article id :-" + it.ArticleId;
                                        response.errorType = (int)Enums.ErrorType.Error;
                                        return response;
                                    }
                                }

                            }
                            ////

                            var customerNo = entity.BookingDetails.Where(w => w.BookingID == OrderNo).Select(s => s).FirstOrDefault();
                            // 14008116
                            //var customer = await session.GetCustomerFromCustomerNo((int)customerNo.Customer, "id", "name");
                            var customer = await session.GetCustomerFromCustomerNo(Convert.ToInt32(customerNumber), "id", "name");
                            if (customer != null)
                            {
                                var order = new JObject();
                                order["orderDate"] = ((DateTime)customerNo.FromDate).ToString("yyyy-MM-dd"); // "2019-04-25";
                                order["deliveryDate"] = ((DateTime)customerNo.FromDate).ToString("yyyy-MM-dd");
                                order["customer"] = JObject.FromObject(new { id = customer["id"] });
                                order["reference"] = "BookingID: " + customerNo.BookingID.ToString();
                                int departmentid = GetKitchenDepID(Convert.ToInt32(customerNo.BuildingID));
                                if (departmentid > 0)
                                {
                                    var department = await session.GetdepartmentFromNumber(departmentid.ToString(), "id");
                                    order["department"] = JObject.FromObject(new { id = department["id"] });
                                }
                                var rsp = await session.CreateOrder(order);
                                // product
                                //// food list
                                var resfood = from ord in entity.BookingServiceLists where ord.BookingID == OrderNo && ord.IsKitchen == true && ord.Status == false select ord;
                                foreach (var it in resfood)
                                {
                                    var orderLine = new JObject();
                                    if (it.ArticleId != "0")
                                    {
                                        var product = await session.GetProductFromNumber(it.ArticleId, "id", "name", "vatType(id)", "priceExcludingVatCurrency");
                                        if (product != null)
                                        {
                                            orderLine["product"] = JObject.FromObject(new { id = product["id"] });
                                            orderLine["vatType"] = JObject.FromObject(new { id = product["vatType"]["id"] });
                                            orderLine["unitPriceExcludingVatCurrency"] = it.CostPrice;
                                            orderLine["count"] = it.Qty;
                                        }
                                    }
                                    orderLine["description"] = it.Tekst;
                                    orderLine["order"] = JObject.FromObject(rsp);
                                    var rsp1 = await session.CreateOrderLine(orderLine);
                                }
                            }
                        }


                        //            try
                        //            {
                        //               // var invoice = await session.CreateInvoice(order["id"].Value<int>(), lastDayOfMonth);
                        //                //log.Info($"Created and sent invoice {invoice["invoiceNumber"]}");
                        //                //return invoice;
                        //            }
                        //            catch (Exception e)
                        //            {
                        //               // log.Error($"Could not create invoice of order {order["orderNumber"]}");
                        //            }

                    }
                    else
                    {

                        response.message = await session.CreateSessionTokenCheck(consumerKey, employeeKey);
                        response.errorType = (int)Enums.ErrorType.Error;
                        return response;
                    }
                }
                catch (Exception e)
                {
                    response.message = e.Message;
                    response.errorType = (int)Enums.ErrorType.Error;
                    return response;
                }
                finally
                {
                    await session.DestroySessionToken();
                }
            }
            response.message = "Successfully order";
            response.errorType = (int)Enums.ErrorType.Success;
            return response;
        }

        public int GetKitchenDepID(int buildingid)
        {
            try
            {
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    string KitchenDepID = entity.MenuItems.Where(w => w.MenuItemID == buildingid).Select(s => s.MetaDescription).FirstOrDefault();
                    if (!string.IsNullOrEmpty(KitchenDepID))
                    {
                        if (KitchenDepID.IndexOf('=') > 0)
                        {
                            string DepID = KitchenDepID.Substring(KitchenDepID.IndexOf('=') + 1, KitchenDepID.Length - (KitchenDepID.IndexOf('=') + 1));
                            return Convert.ToInt32(DepID);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return 0;
            }
            return 0;
        }

        [HttpGet]
        [ActionName("GetKitchenNotification")]
        public List<KitchenNotification> GetKitchenNotification(int buildingid)
        {
            DateTime TodayDate = System.DateTime.Now.Date;
            List<KitchenNotification> kitchenNotificationList = new List<KitchenNotification>();
            KitchenNotification kitchenNotification;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var notDeliveredCount = entity.BookingDetails.Where(ord => ord.BuildingID == buildingid && DbFunctions.TruncateTime(ord.FromDate) <= TodayDate && ord.IsClean == false && ord.IsDeliver == false && ord.BookingServiceLists.Count(s => s.IsKitchen == true && s.Status == false) > 0).GroupBy(info => DbFunctions.TruncateTime(info.FromDate)).Select(group => new { FromDate = DbFunctions.TruncateTime(group.Key), NotDeliveredCount = group.Count(), NotClearUpCount = 0 }).OrderBy(x => DbFunctions.TruncateTime(x.FromDate));
                var notClearUpCount = entity.BookingDetails.Where(ord => ord.BuildingID == buildingid && DbFunctions.TruncateTime(ord.FromDate) <= TodayDate && ord.IsClean == false && ord.IsDeliver == true && ord.BookingServiceLists.Count(s => s.IsKitchen == true && s.Status == false) > 0).GroupBy(info => DbFunctions.TruncateTime(info.FromDate)).Select(group => new { FromDate = DbFunctions.TruncateTime(group.Key), NotDeliveredCount = 0, NotClearUpCount = group.Count() }).OrderBy(x => DbFunctions.TruncateTime(x.FromDate));

                var result = notDeliveredCount.Union(notClearUpCount);
                var groupby = result.GroupBy(info => DbFunctions.TruncateTime(info.FromDate)).Select(group => new { FromDate = DbFunctions.TruncateTime(group.Key), NotDeliveredCount = group.Sum(x => x.NotDeliveredCount), NotClearUpCount = group.Sum(x => x.NotClearUpCount) }).OrderBy(x => DbFunctions.TruncateTime(x.FromDate));
                foreach (var line in groupby)
                {
                    kitchenNotification = new KitchenNotification();
                    kitchenNotification.Fromdate = line.FromDate;
                    kitchenNotification.NotDeliveredCount = line.NotDeliveredCount;
                    kitchenNotification.NotClearUpCount = line.NotClearUpCount;
                    kitchenNotificationList.Add(kitchenNotification);
                }
            }
            return kitchenNotificationList;
        }
    }
}
