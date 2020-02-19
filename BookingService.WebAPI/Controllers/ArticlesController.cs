using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Models;
using WebAPI.Filters;
using System.IO;
using System.Text;


using System.Web;
using System.Web.Handlers;
using System.Web.Caching;
using System.Drawing;
using BookingService.WebAPI.Models;
using WebAPI.DTO;
using System.Data;
using BookingService.WebAPI.DTO;
using BookingService.WebAPI.Filters;
using System.Threading.Tasks;
using System.Configuration;
using DMBase.Core;
using SiteBuilder.WebControls;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using BookingService.WebAPI.Utils;
using Digimaker.Data.Directory;
using System.Globalization;
using logWriter;
using Digimaker.Data.Forms;
using Digimaker.Schemas.Content;
using Digimaker.Schemas.Forms;
using Digimaker.Data.Content;
using Newtonsoft.Json;
using Digimaker.Web.Controls;
using BookingService.WebAPI.Enums;

namespace WebAPI.Controllers
{

    public class ArticlesController : ApiController
    {
        Digimaker.Web.Controls.FormDisplay form = new Digimaker.Web.Controls.FormDisplay();
        List<Article> articles = new List<Article>();
        MenuItemsController menuController;
        LogWriter log = new LogWriter();
        //http://localhost:8082/DigimakerWebApi/api/Articles/GetAllArticles?menuItemId=3
        [HttpGet]
        public IEnumerable<Article> GetAllArticles(string menuItemId)
        {

            GetArticles(menuItemId);
            return articles;
        }

        [HttpGet]
        public object GetMeetingroomInformation(int meetingroomId)
        {
            //building name,
            //prices
            var rows = SiteBuilder.Content.Article.ByMainId(meetingroomId.ToString()).Article.Rows;
            if (rows.Count == 0)
            {
                return 0;
            }
            else
            {
                var result = new Dictionary<string, dynamic>();
                //meeting room info
                var article = (Digimaker.Schemas.Web.ArticleViewData.ArticleRow)rows[0];
                var name = article.Headline;
                result.Add("name", name);
                result.Add("description", article.Fullstory);

                SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
                var formId = Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString();
                DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { meetingroomId }, new string[] { formId });
                var hasPrice = dsContentExtension.Tables[1].Rows.Count > 0;
                result.Add("hasPrice", hasPrice);

                //building info
                var parentId = article.GetArticle_MenuItemRows()[0].MenuItemID;
                var parent = (Digimaker.Schemas.Web.MenuItemViewData.MenuItemRow)SiteBuilder.Content.MenuItem.Get(parentId.ToString(), new int[] { 1 }).MenuItem.Rows[0];
                result.Add("propertyId", parentId);


                var buildingId = parent.ParentID;
                var building = (Digimaker.Schemas.Web.MenuItemViewData.MenuItemRow)SiteBuilder.Content.MenuItem.Get(buildingId.ToString(), new int[] { 1 }).MenuItem.Rows[0];

                result.Add("buildingId", buildingId);
                result.Add("buildingName", building.MenuItemName);
                return result;
            }
        }

        private void GetArticles(string menuItemId)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                int MenuItemID = Convert.ToInt16(menuItemId);
                var serviceSubGrList = entity.MenuItems.Where(w => w.MenuItemID == MenuItemID).Select(s => s).ToList();
                foreach (var item in serviceSubGrList.ToList())
                {
                    var secoundlist = item.MenuItem1.Count();
                    if (secoundlist > 0)
                    {
                        foreach (var lockerservice in item.MenuItem1.ToList())
                        {

                            var lockerarticles = new ArticlesController().GetAllArticles(lockerservice.MenuItemID.ToString());
                            foreach (var article in lockerarticles)
                            {
                                if (article.Status == 0)
                                {
                                    articles.Add(new Article { ArticleID = Convert.ToInt32(article.ArticleID), Headline = article.Headline, Abstract = article.Abstract, Fullstory = article.Fullstory, meta_keywords = lockerservice.MenuItemName, MainID = article.MainID });
                                }


                            }
                        }
                    }
                    else
                    {

                        int menuId = 0;
                        if (!string.IsNullOrEmpty(menuItemId))
                        {
                            menuId = Convert.ToInt32(menuItemId);
                        }

                        var articleData = entity.Articles.Join(entity.Article_MenuItem, a => a.ArticleID, m => m.ArticleID, (A, M) => new { a = A, m = M })
                                                  .Where(w => w.m.MenuItemID == menuId && w.a.Status == 0).Select(s => s.a).OrderBy( o => o.Priority ).ToList();

                        if (articleData.Count > 0)
                        {
                            foreach (var article in articleData)
                            {
                                articles.Add(new Article { ArticleID = article.ArticleID, Headline = article.Headline, Abstract = article.Abstract, Fullstory = article.Fullstory, MainID = article.MainID });
                            }
                        }

                    }
                }
            }


        }
        //private void GetArticles(string menuItemId)
        //{
        //    int MaxData = 100;
        //    Digimaker.Schemas.Web.ArticleViewData articleDataSet = SiteBuilder.Content.Article.ByMenuIds(menuItemId, Digimaker.Data.Content.ArticleSortOrder.Default, int.MaxValue, false, SiteBuilder.Content.Article.ReturnValues.AbstractAndFullstory, MaxData);
        //    for (int i = 0; i < articleDataSet.Tables[0].Rows.Count; i++)
        //    {
        //        //string imgBase64 = ConvertImageURLToBase64(articleDataSet.Tables[0].Rows[i]["Filepath"].ToString());
        //        articles.Add(new Article { ArticleID = Convert.ToInt32(articleDataSet.Tables[0].Rows[i]["ArticleID"]), Headline = articleDataSet.Tables[0].Rows[i]["Headline"].ToString(), Abstract = articleDataSet.Tables[0].Rows[i]["Abstract"].ToString(), Fullstory = articleDataSet.Tables[0].Rows[i]["Fullstory"].ToString() });
        //    }
        //}
        //http://localhost:8082/DigimakerWebApi/api/Articles/GetSingleArticle?menuItemId=3&articleId=26
        public IEnumerable<Article> GetSingleArticle(string menuItemId, int articleId)
        {
            GetArticles(menuItemId);

            if (articles.Count() > 0)
            {
                return articles.Where(p => p.ArticleID == articleId);
            }
            return null;
        }

        private String ConvertImageURLToBase641(String url)
        {
            FileInfo file = null;
            file = new FileInfo(Path.Combine(Digimaker.Config.ConfigDirectory.FullName.ToString() + "dm_pictures", url));

            //StringBuilder _sb = new StringBuilder();
            // Check if the file physically exist on the disk.
            if (file.Exists)
            {
                using (System.Drawing.Image image = System.Drawing.Image.FromFile(file.FullName))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();

                        // Convert byte[] to Base64 String
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
            return null;
        }

        [HttpGet]
        public BookingViewModel GetBookingData(int userid = 0, int menuId = 0)
        {


            BookingViewModel bookingVm = new BookingViewModel();
            List<BookedEvents> events = new List<BookedEvents>();
            bookingVm.resources = BuildingListWithService(menuId);


            // bookingVm.events = BookingList();

            return bookingVm;
        }
        //[HttpGet]

        //[ActionName("GetBookingsData")]
        //public BookingViewModel GetBookingsData( int rootId, int propertyId,int orgId,string propertyServiceId, int userid = 0)
        //{

        //    menuController = new MenuItemsController();
        //    BookingViewModel bookingVm = new BookingViewModel();
        //    List<BookedEvents> events = new List<BookedEvents>();
        //    bookingVm.resources = BuildingListWithService();


        //    bookingVm.events = BookingList();
        //    bookingVm.Properties = menuController.GetMenu(rootId);
        //    bookingVm.PropertyServices = menuController.GetMenu(propertyId);
        //    bookingVm.ServiceList = GetAllArticles(propertyServiceId);
        //    bookingVm.UserList = new AccountController().GetPersonList(orgId);


        //    return bookingVm;
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //[CacheFilter(TimeDuration = 100)]
        //[ActionName("GetBookingsData")]
        //public async Task<IHttpActionResult> GetBookingsData(int rootId, int propertyId, int orgId, string propertyServiceId, int userid = 0)
        //{

        //    menuController = new MenuItemsController();
        //    BookingViewModel bookingVm = new BookingViewModel();
        //    List<BookedEvents> events = new List<BookedEvents>();
        //    bookingVm.resources = BuildingListWithService();


        //    bookingVm.events = BookingList();
        //    bookingVm.Properties = menuController.GetMenu(rootId);
        //    bookingVm.PropertyServices = menuController.GetMenu(propertyId);
        //    bookingVm.ServiceList = GetAllArticles(propertyServiceId);
        //    bookingVm.UserList = new AccountController().GetPersonList(orgId);

        //    return await Task.FromResult(Ok(bookingVm));
        //    // return Ok(bookingVm);
        //}
        [HttpGet]

        // [CacheFilter(TimeDuration = 100)]
        // [CompressFilter]
        [ActionName("GetBookingsData")]
        // public async Task<IHttpActionResult> GetBookingsData(int rootId, int propertyId, int orgId, string propertyServiceId, int userid = 0)
        public IHttpActionResult GetBookingsData(int rootId, int propertyId, int orgId, string propertyServiceId, int userid = 0)
        {


            try
            {
                menuController = new MenuItemsController();
                string prefix = "CalendarResource";
                List<string> key = GetCacheKeys(rootId);
                BookingViewModel bookingVm = new BookingViewModel();
               
                var cachedObject = DMUtil.GetCachedObject(prefix, key, typeof(BookingViewModel));
                if (cachedObject == null)
                {
                   
                    List<BookedEvents> events = new List<BookedEvents>();
                    bookingVm.resources = BuildingListWithService(rootId);

                    bookingVm.FollowupEvents = BookingList(followupDate: DateTime.Today.ToString("yyyy-MM-dd"));
                    bookingVm.Properties = menuController.GetMenu(rootId);
                    bookingVm.PropertyServices = menuController.GetMenu(propertyId, ConfigurationManager.AppSettings["BookingServiceContentExtensionId"]);
                    bookingVm.Timers = GetTimer(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingInitTime"].ToString()), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingEndTime"].ToString()), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingTimeInterval"].ToString()));
                   
                    DMUtil.WriteToCache(bookingVm, prefix, key);
                }
                else
                {
                    bookingVm = (BookingViewModel)cachedObject;

                }
                bookingVm.currentUserID = Digimaker.Directory.Person.Current.PersonID;
                bookingVm.Preferences = new BookingController().GetPreferences();

                return Ok(bookingVm);
            }
            catch (Exception e)
            {
                log.LogWrite("GetBookingsData Failed request:- " + e.Message);
                throw;
            }
        }


        public bool IsMultiplePriceCalculation()
        {
            return false;
        }
        [HttpGet]
        [ActionName("AddOnServiceList")]
        public List<AddOnService> AddOnServiceList(int menuid,int articleId,int formId)
        {
            try
            {
                string prefix = "AddedServices";
                List<string> key = GetCacheKeys(menuid, articleId);
                List<AddOnService> services = new List<AddOnService>();
                var cachedObject = DMUtil.GetCachedObject(prefix, key, typeof(List<AddOnService>));
                if (cachedObject == null)
                {
                    AddOnService addOn;

                    string service = string.Empty;
                    SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
                    DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { formId.ToString() });

                    if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                    {
                        service = dsContentExtension.Tables[1].Rows[0]["additionalservice"].ToString();
                        if (!string.IsNullOrEmpty(service))
                        {
                            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                            {
                                List<int> menuIds = service.Split(',').Select(int.Parse).ToList();
                                foreach (var id in menuIds)
                                {
                                    addOn = new AddOnService();
                                    var result = entity.MenuItems.Where(w => w.MenuItemID == id).Select(s => s).FirstOrDefault();
                                    if (result != null)
                                    {
                                        addOn.ServiceId = result.MenuItemID;
                                        addOn.ServiceName = result.MenuItemName;
                                        addOn.ServiceList = entity.Articles.Join(entity.Article_MenuItem, a => a.ArticleID, m => m.ArticleID, (A, M) => new { a = A, m = M })
                                                           .Where(w => w.m.MenuItemID == id && w.a.Status == 0).Select(s => new ServiceAdd { SId = s.a.ArticleID, SName = s.a.Headline, MainId = s.a.MainID }).ToList();
                                        foreach (var item in addOn.ServiceList)
                                        {
                                            // item.Price = getFoodPrice(item.SId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_AddOnPriceFormID"].ToString()));
                                            var priceresult = new BookingController().getBookingPriceDetail(item.MainId ?? default(int), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString()), null, null, null, null,false,0);
                                            item.Price = priceresult.UnitPrice;
                                            item.SecondaryPrice = priceresult.SecondaryPrice;
                                            item.CostPrice1 = priceresult.CostPrice1;
                                            item.CostPrice2 = priceresult.CostPrice2;
                                            item.ArticleId = priceresult.Article1;
                                            item.ArticleId2 = priceresult.Article2;
                                            item.isKitchen = GetArticleHasIsKitchen(item.MainId ?? default(int), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_AddOnPriceFormID"].ToString()));
                                        }
                                        services.Add(addOn);
                                    }

                                }

                            }
                            DMUtil.WriteToCache(services, prefix, key);
                        }
                    }
                }
                else
                {
                    services = (List<AddOnService>)cachedObject;
                }

                return services;
            }
            catch (Exception e)
            {
                log.LogWrite(" AddOnServiceList Failed request:- " + e.Message);
                throw;
            }
        }


        private bool GetArticleHasIsKitchen(int articleId, int FormID)
        {
            bool isKitchen = false;
            try
            {
                SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
                DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

                if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                {
                    isKitchen = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["kitchen"]);
                }
            }
            catch (Exception)
            {
                isKitchen = false;
                // throw;
            }

            return isKitchen;
        }
        [HttpGet]
        [ActionName("BookingList")]
        public List<BookedEvents> BookingList(int serviceId = 0, string from = null, string to = null, string followupDate = "")
        {
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                DateTime date = new DateTime();
                if (followupDate != "")
                {
                    date = DateTime.Parse(followupDate);
                }


                DateTime fromDate = new DateTime();
                if (from != null)
                {
                    fromDate = DateTime.Parse(from);
                }

                DateTime toDate = new DateTime();
                if (to != null)
                {
                    toDate = DateTime.Parse(to);
                }

                var bookingList = entity.BookingDetails.Where(w =>
                                                                 ( userServices.Contains(0) || !userServices.Contains(0) && userServices.Contains(w.ServiceID)) &&
                                                                (serviceId == 0 || serviceId != 0 && w.ServiceID == serviceId)
                                                                && (followupDate == "" || followupDate != "" && w.FollowUpDate.HasValue && w.FollowUpDate.Value == date)
                                                                && (from == null || from != null && w.FromDate.Value >= fromDate)
                                                                && (to == null || to != null && w.ToDate.Value <= toDate)
                                                                )
                                                        .OrderBy(o => o.FromDate)
                                                        .Select(s => s).ToList();
                //var bookingList = entity.BookingDetails
                //   .Select(s => s).ToList();
                List<BookedEvents> BookingTB = new List<BookedEvents>();
                foreach (var booking in bookingList.ToList())
                {
                    BookedEvents _bookingDetail = new BookedEvents();
                    BookedService _food;
                    List<BookedService> foodList = new List<BookedService>();
                    _bookingDetail.id = booking.BookingID;
                    _bookingDetail.resourceId = booking.ServiceID;
                    //var eventname = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                    //         .Where(w => w.p.PersonID == booking.UserID).AsEnumerable().Select(s => new { eventname = string.Format("{0} [{1}]", s.o.Person.GivenName, s.o.OrganizationUnit.OrganizationUnitName) }).FirstOrDefault();
                    //if (eventname != null)
                    //{
                    //    _bookingDetail.title = eventname.eventname;
                    //}
                    //else
                    //{
                    _bookingDetail.title = booking.BookingName;
                    // }

                    _bookingDetail.buildingId = booking.BuildingID;
                    _bookingDetail.PropertyServiceId = booking.ServiceType;
                    _bookingDetail.ServiceType = entity.MenuItems.Where(w => w.MenuItemID == booking.ServiceType).Select(s => s.MenuItemName).FirstOrDefault();
                    _bookingDetail.numOfPeople = booking.NoOfPeople;
                    _bookingDetail.MeetingRoomId = booking.ServiceID;
                    _bookingDetail.Service = entity.Articles.Where(w => w.MainID == booking.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                    _bookingDetail.UserID = booking.UserID.ToString();
                    _bookingDetail.nameOfbook = booking.BookingName;
                    _bookingDetail.IsFoodOrder = booking.IsFoodOrder;
                    _bookingDetail.start = (DateTime)booking.FromDate;
                    _bookingDetail.end = (DateTime)booking.ToDate;
                    _bookingDetail.SendMessageType = booking.SendMessageType;
                    _bookingDetail.Amount = booking.Amount;
                    _bookingDetail.Status = booking.Status;
                    _bookingDetail.Quantity = booking.Quantity;
                    _bookingDetail.UnitPrice = booking.UnitPrice;
                    _bookingDetail.UnitText = booking.UnitText;
                    _bookingDetail.DiscountPercent = booking.DiscountPercent;
                    _bookingDetail.VATPercent = booking.VATPercent;
                    _bookingDetail.NetAmount = booking.NetAmount;
                    _bookingDetail.CompanyPayer = booking.Customer.ToString();
                    _bookingDetail.BookOrderName = booking.Ordering.ToString();
                    _bookingDetail.FollowDate = booking.FollowUpDate;
                    _bookingDetail.Note = booking.Note;

                    if (booking.UserID != 0)
                    {
                        var ename = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                                                  .Where(w => w.p.PersonID == booking.UserID).AsEnumerable().Select(s => new { userName = s.o.Person.GivenName, orgname = s.o.OrganizationUnit.OrganizationUnitName }).FirstOrDefault();
                        if (ename != null)
                        {
                            _bookingDetail.UserName = ename.userName;
                            _bookingDetail.OrgName = ename.orgname;
                        }

                    }
                    else
                    {
                        _bookingDetail.UserName = booking.BookingName;
                        _bookingDetail.OrgName = "";
                    }

                    _bookingDetail.buildingName = entity.MenuItems.Where(w => w.MenuItemID == booking.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                    foreach (var item in booking.BookingServiceLists)
                    {
                        if (!item.Status)
                        {
                            _food = new BookedService();
                            _food.FoodId = item.FoodID;
                            _food.Qty = item.Qty;
                            _food.ArticleId = item.ArticleId;
                            _food.Sum = item.Sum ?? 0;
                            _food.MainServiceId = item.FoodID.ToString();
                            _food.Tekst = item.Tekst;
                            _food.ServiceText = item.ServiceName;
                            _food.OrderHeadId = item.OrderHeadId ?? 0;
                            _food.OrderListId = item.ID;
                            _food.Time = item.Time;
                            _food.IsKitchenOrder = item.IsKitchen ?? false;
                            _food.Price = item.Price;
                            _food.IsVatApply = item.IsVat ?? false;
                            foodList.Add(_food);
                        }


                    }
                    _bookingDetail.foods = foodList;
                    BookingTB.Add(_bookingDetail);
                }
                return BookingTB;
            }
        }

        public List<string> GetCacheKeys(int? menuId=0,int? articleId=0)
        {
           
           
            var roleslist = UserAccess.GetRoleList();
            List<string> results = new List<string>();
            foreach (var item in roleslist)
            {
                results.Add(item.ToString());
            }


            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                if (articleId != 0)
                {
                    var result = entity.Articles.Where(w => w.MainID == articleId).Select(s => s.MainID).FirstOrDefault();
                    if (result != null)
                    {
                        results.Add(result.ToString());

                    }
                }
                if ( menuId!=0)
                {
                
            
                    var result = entity.MenuItems.Where(w => w.MenuItemID == menuId).Select(s => s.Modified).FirstOrDefault();
                    if (result != null)
                    {
                      
                        results.Add(result.ToString("ddmmyyHmm"));
                    }
                }
               
            }




            return results;
        }
        public List<Building> BuildingListWithService(int menuId)
        {
           
            var userServiceList = UserAccess.GetCurrentUserServices();
           
            
            List<Building> buildinglist = new List<Building>();
           
         
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    Building buildingInfo;
                    Children child;
                    SubChildren subchild;
                    List<Children> childList;
                    List<SubChildren> subchildList;

                    var buildingList = entity.MenuItems.Where(w => w.MenuItemID == menuId).Select(s => s).ToList();
                    foreach (var item in buildingList.ToList())
                    {

                        var buildings = item.MenuItem1.ToList();
                        foreach (var build in buildings)
                        {
                            var buildingService = build.MenuItem1.Where(w => w.Type != 3).Select(s => s).OrderBy( o => o.Priority ).ToList();
                            //foreach (var service in build.MenuItem1.ToList())
                            foreach (var service in buildingService.ToList())
                            {
                                childList = new List<Children>();

                                buildingInfo = new Building();
                                buildingInfo.id = service.MenuItemID;
                                buildingInfo.building = build.MenuItemName;
                                buildingInfo.title = service.MenuItemName;
                                buildingInfo.properties = ContentExtension.GetValues(Convert.ToInt32(ConfigurationManager.AppSettings["BookingServiceContentExtensionId"]),
                                                                                       service.MenuItemID);

                                // for 3nd level tree - saikat

                                var secoundlist = service.MenuItem1.Count();
                                if (secoundlist > 0)
                                {
                                    foreach (var lockerservice in service.MenuItem1.OrderBy(o=>o.Priority).ToList())
                                    {
                                        child = new Children();
                                        subchildList = new List<SubChildren>();
                                        child.id = lockerservice.MenuItemID;
                                        //buildingInfo.building = build.MenuItemName;
                                        child.title = lockerservice.MenuItemName;

                                        var lockerarticles = new ArticlesController().GetAllArticles(lockerservice.MenuItemID.ToString());
                                        foreach (var article in lockerarticles)
                                        {

                                        if (userServiceList.Contains(0) || userServiceList.Contains(article.ArticleID))
                                        {
                                            subchild = new SubChildren();
                                            subchild.id = Convert.ToInt32(article.MainID);
                                            subchild.title = article.Headline;
                                            // child.eventColor = "green";
                                            subchild.eventColor = getColor(article.ArticleID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ColorFormID"].ToString()));

                                            subchild.description = article.Abstract;
                                            var extended = ContentExtension.GetValues(Convert.ToInt32(Digimaker.Config.Custom.Get("CE_ServiceFormID")), article.MainID ?? default(int));
                                            subchild.ExtendedProperties = extended;

                                            subchildList.Add(subchild);
                                           }
                                            child.children = subchildList;
                                        }
                                        childList.Add(child);
                                        //buildingInfo.children = childList;
                                    }
                                    if (childList.Count > 0)
                                    {
                                        buildingInfo.children = childList;
                                        buildinglist.Add(buildingInfo);
                                    }
                                }
                                else
                                {
                                    var articles = new ArticlesController().GetAllArticles(service.MenuItemID.ToString());
                                    foreach (var article in articles)
                                    {
                                    if (userServiceList.Contains(0) || userServiceList.Contains(article.ArticleID))
                                    {
                                        child = new Children();
                                        child.id = Convert.ToInt32(article.MainID);

                                        child.title = article.Headline;



                                        child.eventColor = getColor(article.ArticleID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ColorFormID"].ToString()));

                                        child.description = article.Abstract;

                                        var extended = ContentExtension.GetValues(Convert.ToInt32(Digimaker.Config.Custom.Get("CE_ServiceFormID")), article.MainID ?? default(int));
                                        child.ExtendedProperties = extended;
                                        childList.Add(child);
                                        }
                                    }
                                    if (childList.Count > 0)
                                    {
                                        buildingInfo.children = childList;
                                        buildinglist.Add(buildingInfo);
                                    }
                                }

                            }
                        }
                        // var buildingService = entity.MenuItems.Where(w => w.MenuItemID == item.MenuItemID).Select(s => s).ToList();

                    }
                }
               
            
           
            return buildinglist;
        }

        public List<Building> BuildingListWithServiceForCalendar(int menuId)
        {
            var userServiceList = UserAccess.GetCurrentUserServices();

            List<Building> buildinglist = new List<Building>();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                Building buildingInfo;
                Children child;
                SubChildren subchild;
                List<Children> childList;
                List<SubChildren> subchildList;

                var buildingList = entity.MenuItems.Where(w => w.MenuItemID == menuId).Select(s => s).ToList();
                foreach (var item in buildingList.ToList())
                {

                    var buildings = item.MenuItem1.ToList();
                    foreach (var build in buildings)
                    {
                        var buildingService = build.MenuItem1.Where(w => w.Type != 3).Select(s => s).ToList();
                        //foreach (var service in build.MenuItem1.ToList())
                        foreach (var service in buildingService.ToList())
                        {
                            childList = new List<Children>();

                            buildingInfo = new Building();
                            buildingInfo.id = service.MenuItemID;
                            buildingInfo.building = build.MenuItemName;
                            buildingInfo.title = service.MenuItemName;
                            buildingInfo.properties = ContentExtension.GetValues(Convert.ToInt32(ConfigurationManager.AppSettings["BookingServiceContentExtensionId"]),
                                                                                   service.MenuItemID);

                            // for 3nd level tree - saikat

                            var secoundlist = service.MenuItem1.Count();
                            if (secoundlist > 0)
                            {
                                foreach (var lockerservice in service.MenuItem1.ToList())
                                {
                                    child = new Children();
                                    subchildList = new List<SubChildren>();
                                    child.id = lockerservice.MenuItemID;
                                    //buildingInfo.building = build.MenuItemName;
                                    child.title = lockerservice.MenuItemName;

                                    var lockerarticles = new ArticlesController().GetAllArticles(lockerservice.MenuItemID.ToString());
                                    foreach (var article in lockerarticles)
                                    {

                                        if (userServiceList.Contains(0) || userServiceList.Contains(article.ArticleID))
                                        {
                                            subchild = new SubChildren();
                                        subchild.id = Convert.ToInt32(article.MainID);
                                        subchild.title = article.Headline;
                                        // child.eventColor = "green";
                                        subchild.eventColor = getColor(article.ArticleID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ColorFormID"].ToString()));
                                        subchildList.Add(subchild);
                                        }
                                        child.children = subchildList;
                                    }
                                    childList.Add(child);
                                    //buildingInfo.children = childList;
                                }
                                if (childList.Count > 0)
                                {
                                    buildingInfo.children = childList;
                                    buildinglist.Add(buildingInfo);
                                }
                            }
                            else
                            {
                                var articles = new ArticlesController().GetAllArticles(service.MenuItemID.ToString());
                                foreach (var article in articles)
                                {
                                    if (userServiceList.Contains(0) || userServiceList.Contains(article.ArticleID))
                                    {
                                        child = new Children();
                                    child.id = Convert.ToInt32(article.MainID);

                                    child.title = article.Headline;



                                    child.eventColor = getColor(article.ArticleID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ColorFormID"].ToString()));

                                    child.description = article.Abstract;

                                    // var extended = ContentExtension.GetValues(Convert.ToInt32(Digimaker.Config.Custom.Get("CE_ServiceFormID")), article.MainID ?? default(int));
                                    //  child.ExtendedProperties = extended;
                                    childList.Add(child);
                                     }
                                }
                                if (childList.Count > 0)
                                {
                                    buildingInfo.children = childList;
                                    buildinglist.Add(buildingInfo);
                                }
                            }

                        }
                    }
                    // var buildingService = entity.MenuItems.Where(w => w.MenuItemID == item.MenuItemID).Select(s => s).ToList();

                }
            }

            return buildinglist;
        }
        [HttpGet]
        public BookingDetail BookingDetail(int bookingId)
        {
            BookingDetail booking;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                booking = entity.BookingDetails.Where(w => w.BookingID == bookingId).Select(s => s).FirstOrDefault();



            }
            return booking;
        }

        [HttpGet]

        [ActionName("BulidingList")]
        public IEnumerable<BuildingModel> BulidingList(int menuID)
        {
            List<BuildingModel> buildings = new List<BuildingModel>();
            BuildingModel building;
            ServiceGroup serviceGrp;
            Service service;
            List<ServiceGroup> buildingServiceList;
            List<OrgUnit> orglist;
            OrgUnit orgUnit;
            List<OrganizationPerson> personlist;
            OrganizationPerson person;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var buildingList = entity.MenuItems.Where(w => w.MenuItemID == menuID).Select(s => s).ToList();
                foreach (var i in buildingList)
                {


                    var buildingLists = i.MenuItem1.ToList();
                    foreach (var item in buildingLists)
                    {
                        buildingServiceList = new List<ServiceGroup>();
                        orglist = new List<OrgUnit>();
                        building = new BuildingModel();
                        building.id = item.MenuItemID;
                        building.name = item.MenuItemName;
                        if (item.MetaKey != null)
                        {
                            var array = item.MetaKey.Split(',');
                            if (array.Length >= 2)
                            {
                                building.organizationId = int.Parse(array[0]);
                                building.informationId = int.Parse(array[1]);
                                building.informationTranslationId = int.Parse(array[2]);
                                building.kitchenMenuId = int.Parse(array[3]);
                            }
                        }
                        foreach (var serviceGroup in item.MenuItem1.ToList())
                        {
                            List<Service> articles = new List<Service>();
                            serviceGrp = new ServiceGroup();
                            serviceGrp.id = serviceGroup.MenuItemID;
                            serviceGrp.name = serviceGroup.MenuItemName;
                            var result = new ArticlesController().GetAllArticles(serviceGroup.MenuItemID.ToString());
                            foreach (var article in result)
                            {
                                service = new Service();
                                service.id = article.ArticleID;
                                int member = GetMemberForService(article.ArticleID, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_AddOnMemberFormID"].ToString()));
                                if (member != 0)
                                {
                                    service.name = article.Headline + "(" + member + "" + new string(' ', 1) + "personer)";
                                }
                                else
                                {
                                    service.name = article.Headline;
                                }
                                //  service.name = article.Headline;
                                articles.Add(service);
                            }
                            serviceGrp.services = articles;
                            buildingServiceList.Add(serviceGrp);

                        }
                        building.serviceList = buildingServiceList;
                        int buildingId = 0;
                        if (item.MetaKey != null)
                        {
                            buildingId = Convert.ToInt32(item.MetaKey.Split(',')[0]);
                        }

                        orglist = GetOrganizationList(buildingId.ToString());
                        foreach (var org in orglist)
                        {

                            //2nd lavel adding company and person 
                            var orglist1 = GetOrganizationList(org.id.ToString());
                            
                            foreach (var org1 in orglist1)
                            {
                                
                                personlist = new List<OrganizationPerson>();
                                var persons1 = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                                 .Where(w => w.o.OrganizationUnitID == org1.id).Select(s => s.p).
                                 OrderBy(o => o.DisplayName).ToList();
                                if (persons1 != null)
                                {
                                    foreach (var p in persons1.Where(p => p.Custom2 == null || p.Custom2 == ""))
                                    {
                                        person = new OrganizationPerson();
                                        person.id = p.PersonID;
                                        person.name = p.DisplayName;
                                        personlist.Add(person);
                                    }
                                }
                                org1.personList=personlist;
                                //org.orglist.p.Add(personlist);
                                org.orglist.Add(org1);
                            }
                            // 2nd lavel adding company and person 

                            personlist = new List<OrganizationPerson>();
                            var persons = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                             .Where(w => w.o.OrganizationUnitID == org.id).Select(s => s.p).
                             OrderBy(o => o.DisplayName).ToList();
                            if (persons != null)
                            {
                                foreach (var p in persons.Where(p => p.Custom2 == null || p.Custom2 == ""))
                                {
                                    person = new OrganizationPerson();
                                    person.id = p.PersonID;
                                    person.name = p.DisplayName;
                                    personlist.Add(person);
                                }
                            }
                            org.personList = personlist;
                            // org_person and //person
                        }
                        building.organizationList = orglist;
                        buildings.Add(building);
                    }

                }
            }
            return buildings;
        }

        
        [HttpGet]
        public List<OrgUnit> GetOrganizationList(string parentId)
        {
            BraathenEiendomEntities entity = new BraathenEiendomEntities();
            var intParentId = Int32.Parse(parentId);
            var orgResult = entity.OrganizationUnits.Where(w => w.ParentID == intParentId).
                                                     Select(s => s).
                                                     OrderBy(c => c.OrganizationUnitName).
                                                     ToList();
            List<OrgUnit> orglist = new List<OrgUnit>();
            foreach (var org in orgResult)
            {
                var orgUnit = new OrgUnit();
                orgUnit.id = org.OrganizationUnitID;
                orgUnit.name = org.OrganizationUnitName;
                orglist.Add(orgUnit);
            }
            return orglist;
        }

        public List<OrgUnit> GetOrganizationList1(string parentId)
        {
            BraathenEiendomEntities entity = new BraathenEiendomEntities();
            var intParentId = Int32.Parse(parentId);
            var orgResult = entity.OrganizationUnits.Where(w => w.ParentID == intParentId).
                                                     Select(s => s).
                                                     OrderBy(c => c.OrganizationUnitName).
                                                     ToList();
            List<OrgUnit> orglist = new List<OrgUnit>();
            foreach (var org in orgResult)
            {
                var orgUnit = new OrgUnit();
                orgUnit.id = org.OrganizationUnitID;
                orgUnit.name = org.OrganizationUnitName;
                orglist.Add(orgUnit);
            }
            return orglist;
        }
        [HttpGet]
        [ActionName("GetArticleDetail")]
        public ArticleModel GetArticleDetail(string articleId)
        {
            ArticleModel article;
            int id = 0;
            if (articleId != null)
            {
                id = Convert.ToInt32(articleId);
            }

            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                article = new ArticleModel();
                var result = entity.Articles.Where(w => w.ArticleID == id).Select(s => s).FirstOrDefault();
                article.ArticleId = result.ArticleID;
                article.ArticleName = result.Headline;
                article.ArticleDesc = result.Abstract;
                if (result.PictureID != null)
                {
                    article.PicturePath = entity.PictureProperties.Where(w => w.PictureMainID == result.PictureID).Select(s => s.Filepath).ToString();
                }
            }
            return article;
        }

        [HttpGet]
        public IEnumerable<DTO.Food> FoodServiceBuldingWise(int menuId, int FormId)
        {
            List<DTO.Food> foodlist = new List<DTO.Food>();
            DTO.Food foodItem;
            int MaxData = 100;
            Digimaker.Schemas.Web.ArticleViewData articleDataSet = SiteBuilder.Content.Article.ByMenuIds(menuId.ToString(), Digimaker.Data.Content.ArticleSortOrder.Default, int.MaxValue, false, SiteBuilder.Content.Article.ReturnValues.AbstractAndFullstory, MaxData);
            for (int i = 0; i < articleDataSet.Tables[0].Rows.Count; i++)
            {
                foodItem = new DTO.Food();
                foodItem.FoodId = Convert.ToInt32(articleDataSet.Tables[0].Rows[i]["ArticleID"]);

                SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
                DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { foodItem.FoodId }, new string[] { FormId.ToString() });

                if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                {
                    foodItem.FoodName = articleDataSet.Tables[0].Rows[i]["Headline"].ToString() + "[" + Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["price"].ToString()) + "]";
                    foodItem.price = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["price"].ToString());
                }
                foodlist.Add(foodItem);
            }


            return foodlist;

        }

        public int getFoodPrice(int articleId, int FormID)
        {
            int price = 0;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dsContentExtension.Tables[1].Rows[0]["price"].ToString()))
                {
                    price = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["price"].ToString());
                }
            }


            return price;
        }

        [HttpGet]
        [ActionName("GetMemberForService")]
        public int GetMemberForService(int articleId, int FormID)
        {
            int member = 0;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dsContentExtension.Tables[1].Rows[0]["capacity"].ToString()))
                {
                    member = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["capacity"].ToString());
                }
            }


            return member;
        }
        public string getColor(int articleId, int FormID)
        {
            string color = string.Empty;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                color = dsContentExtension.Tables[1].Rows[0]["color"].ToString();
            }
            if (String.IsNullOrWhiteSpace(color))
            {
                color = "green";
            }
            return color;
        }


        public List<string> GetTimer(int sTime, int eTime, int interval)
        {

            DateTime now = DateTime.Now;
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, sTime, 0, 0);
            DateTime endTime = new DateTime(now.Year, now.Month, now.Day, eTime, 0, 0);
            List<string> timer = new List<string>();
            while (startTime <= endTime)
            {
                var a = startTime.ToShortTimeString();
                var b  = a.Replace(".", ":");
                timer.Add(b);
                startTime = startTime.AddMinutes(interval);
            }
            return timer;
        }

        public int getVatType(int articleId, int FormID)
        {
            int vatType = 0;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dsContentExtension.Tables[1].Rows[0]["mvaType"].ToString()))
                {
                    vatType = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["mvaType"].ToString());
                }
            }


            return vatType;
        }
        public float getVatTypeValue(int FormID, int vatType)
        {
            float vatPercent = 0;
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.None, new int[] { }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dsContentExtension.Tables[1].Rows[0]["mvaType"].ToString()))
                {
                    vatPercent = float.Parse(dsContentExtension.Tables[1].Rows[0]["mvaType"].ToString());
                }
            }


            return vatPercent;
        }

        [ActionName("GetBookingLists")]
        public IHttpActionResult GetBookingLists()
        {
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var bookingList = entity.BookingDetails
                                                         .Where(w => (userServices.Contains(0) || !userServices.Contains(0) && userServices.Contains(w.ServiceID)))
                                                       .OrderBy(o => o.FromDate)
                                                       .Select(s => s).ToList();
                List<BookedEvents> BookingTB = new List<BookedEvents>();
                foreach (var booking in bookingList.ToList())
                {
                    BookedEvents _bookingDetail = new BookedEvents();
                    _bookingDetail.id = booking.BookingID;
                    _bookingDetail.title = booking.BookingName;
                    _bookingDetail.start = (DateTime)booking.FromDate;
                    _bookingDetail.end = (DateTime)booking.ToDate;
                    _bookingDetail.Note = booking.Note;
                    BookingTB.Add(_bookingDetail);
                }
                return Ok(BookingTB);
            }

        }



        [ActionName("GetBookingListsByView")]
        public IHttpActionResult GetBookingListsByView(string fromDate, string toDate)
        {
            DateTime fdate = new DateTime();
            DateTime tdate = new DateTime();
            try
            {
                if (!string.IsNullOrEmpty(fromDate))
                {
                    fdate = Convert.ToDateTime(fromDate);
                }
                if (!string.IsNullOrEmpty(toDate))
                {
                    tdate = Convert.ToDateTime(toDate);
                }

                var userServices = UserAccess.GetCurrentUserServices();
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    var bookingList = from ord in entity.BookingDetails
                                      where
                                        (userServices.Contains(0) || !userServices.Contains(0) && userServices.Contains(ord.ServiceID)) &&
                                      DbFunctions.TruncateTime(ord.FromDate) >= fdate && DbFunctions.TruncateTime(ord.FromDate) <= tdate
                                      select ord;
                    var bookings = from booking in entity.BookingDetails
                                   where
                                       (userServices.Contains(0) || !userServices.Contains(0) && userServices.Contains(booking.ServiceID)) &&
                                      fdate >= DbFunctions.TruncateTime(booking.FromDate) && fdate <= DbFunctions.TruncateTime(booking.ToDate)
                                   select booking;
                 List < BookedEvents > BookingTB = new List<BookedEvents>();
                    foreach (var booking in bookingList.ToList())
                    {
                        BookedEvents _bookingDetail = new BookedEvents();
                        _bookingDetail.id = booking.BookingID;
                        _bookingDetail.title = booking.BookingName;
                        _bookingDetail.start = (DateTime)booking.FromDate;
                        _bookingDetail.end = (DateTime)booking.ToDate;
                        _bookingDetail.Note = booking.Note;
                        _bookingDetail.Status = booking.Status;
                        _bookingDetail.resourceId = booking.ServiceID;

                        BookingTB.Add(_bookingDetail);
                    }
                    if (bookings != null)
                    {
                        foreach (var item in bookings.ToList())
                        {
                            var isexist = BookingTB.Where(w => w.id == item.BookingID).FirstOrDefault();
                            if (isexist == null)
                            {
                                BookedEvents _bookingDetail = new BookedEvents();
                                _bookingDetail.id = item.BookingID;
                                _bookingDetail.title = item.BookingName;
                                _bookingDetail.start = (DateTime)item.FromDate;
                                _bookingDetail.end = (DateTime)item.ToDate;
                                _bookingDetail.Note = item.Note;
                                _bookingDetail.Status = item.Status;
                                _bookingDetail.resourceId = item.ServiceID;

                                BookingTB.Add(_bookingDetail);
                            }
                           
                        }
                    }
                    return Ok(BookingTB.Distinct());
                }

            }
            catch (Exception e)
            {
                log.LogWrite(" GetBookingListsByView Failed request:- " + e.Message);
                throw;
            }
        }

        [ActionName("GetBookingDetailByBookingId")]
        public IHttpActionResult GetBookingDetailByBookingId(int bookingId)
        {
            BookedEvents _bookingDetail = new BookedEvents();
            BookedService _food;
            List<BookedService> foodList = new List<BookedService>();
            try
            {
                if (bookingId != 0)
                {
                    using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                    {

                        var booking = entity.BookingDetails.Where(w => w.BookingID == bookingId).FirstOrDefault();
                        if (booking != null)
                        {
                            _bookingDetail.id = booking.BookingID;
                            _bookingDetail.resourceId = booking.ServiceID;

                            _bookingDetail.title = booking.BookingName;


                            _bookingDetail.buildingId = booking.BuildingID;
                            _bookingDetail.PropertyServiceId = booking.ServiceType;
                            _bookingDetail.ServiceType = entity.MenuItems.Where(w => w.MenuItemID == booking.ServiceType).Select(s => s.MenuItemName).FirstOrDefault();
                            _bookingDetail.numOfPeople = booking.NoOfPeople;
                            _bookingDetail.MeetingRoomId = booking.ServiceID;
                            _bookingDetail.Service = entity.Articles.Where(w => w.MainID == booking.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                            _bookingDetail.UserID = booking.UserID.ToString();
                            _bookingDetail.nameOfbook = booking.BookingName;
                            _bookingDetail.IsFoodOrder = booking.IsFoodOrder;
                            _bookingDetail.start = (DateTime)booking.FromDate;
                            _bookingDetail.end = (DateTime)booking.ToDate;
                            _bookingDetail.SendMessageType = booking.SendMessageType;
                            _bookingDetail.Amount = booking.Amount;
                            _bookingDetail.Status = booking.Status;
                            _bookingDetail.Quantity = booking.Quantity;
                            _bookingDetail.UnitPrice = booking.UnitPrice;
                            _bookingDetail.UnitText = booking.UnitText;
                            _bookingDetail.DiscountPercent = booking.DiscountPercent;
                            _bookingDetail.VATPercent = booking.VATPercent;
                            _bookingDetail.NetAmount = booking.NetAmount;
                            _bookingDetail.CompanyPayer = booking.Customer.ToString();
                            _bookingDetail.BookOrderName = booking.Ordering.ToString();
                            _bookingDetail.FollowDate = booking.FollowUpDate;
                            _bookingDetail.Note = booking.Note;
                            _bookingDetail.InvoMessage = booking.InvoMessage;
                            _bookingDetail.IsMVA = booking.IsMVA;
                            _bookingDetail.IsInternal = booking.IsInternal;
                            _bookingDetail.Status = booking.Status;
                            if (booking.UserID != 0)
                            {
                                var ename = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                                                          .Where(w => w.p.PersonID == booking.UserID).AsEnumerable().Select(s => new { userName = s.o.Person.GivenName, orgname = s.o.OrganizationUnit.OrganizationUnitName }).FirstOrDefault();
                                if (ename != null)
                                {
                                    _bookingDetail.UserName = ename.userName;
                                    _bookingDetail.OrgName = ename.orgname;
                                }
                            }
                            else
                            {
                                _bookingDetail.UserName = booking.BookingName;
                                _bookingDetail.OrgName = "";
                            }

                            _bookingDetail.buildingName = entity.MenuItems.Where(w => w.MenuItemID == booking.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                            foreach (var item in booking.BookingServiceLists)
                            {
                                if (!item.Status)
                                {
                                    _food = new BookedService();
                                    _food.FoodId = item.FoodID;
                                    _food.Qty = item.Qty;
                                    _food.ArticleId = item.ArticleId;
                                    _food.Sum = item.Sum ?? 0;
                                    _food.MainServiceId = item.FoodID.ToString();
                                    _food.Tekst = item.Tekst;
                                    _food.ServiceText = item.ServiceName;
                                    _food.OrderHeadId = item.OrderHeadId ?? 0;
                                    _food.OrderListId = item.ID;
                                    _food.Time = item.Time;
                                    _food.IsKitchenOrder = item.IsKitchen ?? false;
                                    _food.Price = item.Price;
                                    _food.CostPrice = item.CostPrice;
                                    _food.CostTotal = item.CostTotal;
                                    _food.IsVatApply = item.IsVat ?? false;
                                    foodList.Add(_food);
                                }


                            }
                            _bookingDetail.foods = foodList;
                        }
                        else
                        {
                            var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new StringContent("Booking Data Not available for bookingID=" + bookingId)
                            };
                            throw new HttpResponseException(message);
                        }
                    }
                }
                else
                {
                    return InternalServerError(new Exception("Booing Id Invalid"));
                }
                return Ok(_bookingDetail);
            }
            catch (Exception e)
            {
                log.LogWrite("GetBookingDetailByBookingId Failed request:- " + e.Message);
                throw;
            }
        }

        [ActionName("GetOrderDetailByOrderId")]
        public IHttpActionResult GetOrderDetailByOrderId(int orderId)
        {
            BookedEvents _bookingDetail = new BookedEvents();
            BookedService _food;
            List<BookedService> foodList = new List<BookedService>();
            try
            {
                if (orderId != 0)
                {
                    using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                    {

                        var order = entity.Orderheads.Where(w => w.OrderNo == orderId).FirstOrDefault();
                        if (order != null)
                        {
                            //_bookingDetail.id = order.BookingID;
                            //_bookingDetail.resourceId = order.ServiceID;

                            //_bookingDetail.title = order.BookingName;


                            //_bookingDetail.buildingId = order.BuildingID;
                            //_bookingDetail.PropertyServiceId = order.ServiceType;
                            //_bookingDetail.ServiceType = entity.MenuItems.Where(w => w.MenuItemID == order.ServiceType).Select(s => s.MenuItemName).FirstOrDefault();
                            //_bookingDetail.numOfPeople = order.NoOfPeople;
                            //_bookingDetail.MeetingRoomId = order.ServiceID;
                            //_bookingDetail.Service = entity.Articles.Where(w => w.MainID == order.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                            //_bookingDetail.UserID = order.UserID.ToString();
                            //_bookingDetail.nameOfbook = order.BookingName;
                            //_bookingDetail.IsFoodOrder = order.IsFoodOrder;
                            //_bookingDetail.start = (DateTime)order.FromDate;
                            //_bookingDetail.end = (DateTime)order.ToDate;
                            //_bookingDetail.SendMessageType = order.SendMessageType;
                            //_bookingDetail.Amount = order.Amount;
                            //_bookingDetail.Status = order.Status;
                            //_bookingDetail.Quantity = order.Quantity;
                            //_bookingDetail.UnitPrice = order.UnitPrice;
                            //_bookingDetail.UnitText = order.UnitText;
                            //_bookingDetail.DiscountPercent = order.DiscountPercent;
                            //_bookingDetail.VATPercent = order.VATPercent;
                            //_bookingDetail.NetAmount = order.NetAmount;
                            //_bookingDetail.CompanyPayer = order.Customer.ToString();
                            //_bookingDetail.BookOrderName = order.Ordering.ToString();
                            //_bookingDetail.FollowDate = order.FollowUpDate;
                            //_bookingDetail.Note = order.Note;
                            //_bookingDetail.InvoMessage = order.InvoMessage;
                            //_bookingDetail.IsMVA = order.IsMVA;
                            //_bookingDetail.IsInternal = order.IsInternal;

                            //if (order.UserID != 0)
                            //{
                            //    var ename = entity.People.Join(entity.OrganizationUnit_Person, p => p.PersonID, o => o.PersonID, (P, O) => new { p = P, o = O })
                            //                              .Where(w => w.p.PersonID == order.UserID).AsEnumerable().Select(s => new { userName = s.o.Person.GivenName, orgname = s.o.OrganizationUnit.OrganizationUnitName }).FirstOrDefault();
                            //    if (ename != null)
                            //    {
                            //        _bookingDetail.UserName = ename.userName;
                            //        _bookingDetail.OrgName = ename.orgname;
                            //    }
                            //}
                            //else
                            //{
                            //    _bookingDetail.UserName = order.CustomerName;
                            //    _bookingDetail.OrgName = "";
                            //}

                          //  _bookingDetail.buildingName = entity.MenuItems.Where(w => w.MenuItemID == order.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                            foreach (var item in order.Orderlines.OrderBy(s => s.SortOrder))
                            {
                                if (!item.Status)
                                {
                                    _food = new BookedService();
                                    _food.FoodId = item.FoodID;
                                    _food.Qty = item.Qty;
                                    _food.ArticleId = item.ArticleId;
                                    _food.Sum = item.Sum??0;
                                    _food.MainServiceId = item.FoodID.ToString();
                                    _food.Tekst = item.Tekst;
                                    _food.ServiceText = item.ServiceName;
                                    _food.OrderHeadId = item.OrderHeadId ;
                                    _food.OrderListId = item.ID;
                                    _food.Time = item.Time;
                                    _food.IsKitchenOrder = item.IsKitchen ?? false;
                                    _food.Price = item.Price;
                                    _food.CostPrice = item.CostPrice;
                                    _food.CostTotal = item.CostTotal;
                                    _food.IsVatApply = item.IsVat ?? false;

                                    foodList.Add(_food);
                                }


                            }
                            _bookingDetail.foods = foodList;
                        }
                        else
                        {
                            var message = new HttpResponseMessage(HttpStatusCode.BadRequest)
                            {
                                Content = new StringContent("Order Data Not available for bookingID=" + orderId)
                            };
                            throw new HttpResponseException(message);
                        }
                    }
                }
                else
                {
                    return InternalServerError(new Exception("Order Id Invalid"));
                }
                return Ok(_bookingDetail);
            }
            catch (Exception e)
            {
                log.LogWrite("GetOrderDetailByBookingId Failed request:- " + e.Message);
                throw;
            }
        }
        [ActionName("GetDataforCreateBooking")]
        public IHttpActionResult GetDataforCreateBooking(int rootId, int propertyId, int orgId, string propertyServiceId)
        {
            menuController = new MenuItemsController();
            try
            {
                string prefix = "CreateBooking";
                List<string> key = GetCacheKeys(rootId);
                BookingViewModel bookingVm = new BookingViewModel();
       

                var cachedObject = DMUtil.GetCachedObject(prefix, key, typeof(BookingViewModel));
                if (cachedObject == null)
                {
                    bookingVm.Properties = menuController.GetMenu(rootId);
                    bookingVm.PropertyServices = menuController.GetMenu(propertyId, ConfigurationManager.AppSettings["BookingServiceContentExtensionId"]);
                    bookingVm.ServiceList = GetAllArticles(propertyServiceId);
                    bookingVm.UserList = new AccountController().GetPersonList(orgId);
                   
                    bookingVm.Timers = GetTimer(Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingInitTime"].ToString()), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingEndTime"].ToString()), Convert.ToInt32(Digimaker.Config.Custom.AppSettings["BookingTimeInterval"].ToString()));
                    int articleId = bookingVm.ServiceList.Select(s => s.ArticleID).FirstOrDefault();
                    bookingVm.AddonServiceList = AddOnServiceList(rootId, articleId, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ServiceFormID"].ToString()));

                    DMUtil.WriteToCache(bookingVm, prefix, key);
                }
                else
                {
                    bookingVm = (BookingViewModel)cachedObject;

                }
                bookingVm.CustomerList = new AccountController().GetCustomerList();
                return Ok(bookingVm);
            }
            catch (Exception e)
            {
                log.LogWrite("GetDataforCreateBooking Failed request:- " + e.Message);
                throw;
            }
        }
        [HttpGet]
        [ActionName("test")]
        public IHttpActionResult Test(int rootId)
        {
            return Ok("hi");
        }
        [HttpGet]
        [ActionName("CreateArticle")]
        //[IdentityBasicAuthentication]
        //[Authorize]
        public IHttpActionResult CreateArticle(int menuId, int articleId,  string aname, int pAId, decimal price1, decimal price2, decimal cPrice1, decimal cPrice2,string unit="", string unitName="",string ingrese = "", string body = "", string author = "", string publishDate = "" )
       {
            int tempMainId;
          
            int[] roles = { 1 };
            string mainId = string.Empty;
            ApiResponse response = new ApiResponse();
            FormEditorHandler articleHandler = new FormEditorHandler();
            ArticleEditorData Articledata = new ArticleEditorData();

            try
            {
                if (string.IsNullOrEmpty(aname))
                {
                    response.message = "Article Name should not be null";
                    response.errorType = (int)ErrorType.Error;
                    return Ok(response);
                }

                if (menuId != 0)
                {
                    if (articleId == 0)
                    {
                        Articledata = ArticleEditorHandler.NewArticle(); ;
                    }
                    else
                    {
                        Articledata = ArticleEditorHandler.EditArticle(articleId, roles);
                    }

                    //
                    ArticleEditorData.ArticleRow objArticleRow;
                    if (Articledata.Article.Rows.Count > 0)
                    {
                        int articleMenuID = Convert.ToInt32(Articledata.Article_MenuItem.Rows[0]["MenuItemID"]);
                        if (menuId != articleMenuID)
                        {
                            response.message = "Article not found in Folder";
                            response.errorType = (int)ErrorType.Error;
                            return Ok(response);
                        }
                        DataRow articleRow;
                        articleRow = Articledata.Article.Rows[0];
                        if (!string.IsNullOrEmpty(aname))
                        {
                            articleRow["Headline"] = aname;
                        }
                        if (!string.IsNullOrEmpty(ingrese))
                        {
                            articleRow["Abstract"] = ingrese;
                        }
                        if (!string.IsNullOrEmpty(body))
                        {
                            articleRow["Fullstory"] = body;
                        }
                        if (!string.IsNullOrEmpty(publishDate))
                        {
                            articleRow["ShowDate"] = Convert.ToDateTime(publishDate);
                        }

                        if (!string.IsNullOrEmpty(author))
                        {
                            articleRow["Author"] = author;
                        }


                        tempMainId = Convert.ToInt32(articleRow["MainID"]);
                    }
                    else
                    {
                        objArticleRow = Articledata.Article.NewArticleRow();
                        objArticleRow.Headline = aname;
                        objArticleRow.Abstract = ingrese;
                        objArticleRow.Fullstory = body;
                        objArticleRow.PublishStatus = 3;
                        objArticleRow.Priority = 1;
                        objArticleRow.PersonID = 1;
                        if (!string.IsNullOrEmpty(publishDate))
                        {
                            objArticleRow.ShowDate = Convert.ToDateTime(publishDate);
                        }
                        else
                        {
                            objArticleRow.ShowDate = System.DateTime.Now;
                        }
                        objArticleRow.Author = author;

                        objArticleRow.CreateDate = System.DateTime.Now;
                        tempMainId = -1;
                        Articledata.Article.AddArticleRow(objArticleRow);
                    }

                    if (Articledata.Article_MenuItem.Rows.Count > 0)
                        foreach (DataRow row in Articledata.Article_MenuItem.Rows)
                            row.Delete();

                    ArticleEditorData.Article_MenuItemRow objMenuItemRow; objMenuItemRow = Articledata.Article_MenuItem.NewArticle_MenuItemRow();

                    objMenuItemRow.MainID = tempMainId;
                    objMenuItemRow.Priority = 1;
                    objMenuItemRow.AssociationID = 3;
                    objMenuItemRow.MenuItemID = menuId;

                    Articledata.Article_MenuItem.AddArticle_MenuItemRow(objMenuItemRow);
                    ArticleEditorHandler data = new ArticleEditorHandler();

                    ArticleEditorHandler.Update(Articledata, roles, 1);

                     mainId = Articledata.Article.Rows[0]["MainID"].ToString();

                    var result = AddPriceForArticle(menuId.ToString(), mainId, pAId.ToString(), price1, price2, cPrice1, cPrice2, unit, unitName);
                    if (result == false)
                    {
                        response.message = "Price detail not Saved";
                        response.errorType = (int)ErrorType.Error;
                        return Ok(response);
                    }
                }
                else
                {
                    response.message = "MenuId should not be zero";
                    response.errorType = (int)ErrorType.Error;
                    return Ok(response);
                }

                if (articleId != 0)
                {
                    response.message = "Article Updated";
                    response.errorType = (int)ErrorType.Success;
                    return Ok(response);
                }
                else
                {
                    response.message = mainId+ new string(' ', 1) + "Article saved";
                    response.errorType = (int)ErrorType.Success;
                    response.data = mainId;
                    return Ok(response);
                }
               

            }
            catch (Exception)
            {

                response.message = "Article Not updated";
                response.errorType = (int)ErrorType.Error;
                return Ok(response);
            }
            
        }

        private bool AddPriceForArticle(string menuId, string articleId,string cArticleNo, decimal price1, decimal price2,decimal cPrice1, decimal cPrice2,string unit, string unitName)
        {

            try
            {
                PriceContent price = new PriceContent();
                bool IsPriceUpdate = false;
                int recordId = 0;
               
                SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
              
               
                int formId = Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString());
                int articleval = Convert.ToInt32(articleId);
                DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { articleval }, new string[] { formId.ToString() });

                if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                {
                    if (dsContentExtension.Tables[1].Rows.Count > 1)
                    {
                        IsPriceUpdate = false;
                    }
                    else
                    {
                        IsPriceUpdate = true;
                        recordId = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0][0]);

                        if(!string.IsNullOrEmpty(cArticleNo))
                        {
                            price.Article1 = cArticleNo;
                        }
                        else
                        {
                            price.Article1 = dsContentExtension.Tables[1].Rows[0]["Article1"].ToString();
                        }
                       
                        price.price1 = price1;
                        price.price2 = price2;
                        price.costPrice1 = cPrice1;
                        price.costPrice2 = cPrice2;
                      
                        if (!string.IsNullOrEmpty(unit))
                        {
                            price.unit = unit;
                        }
                        else
                        {
                            price.unit = dsContentExtension.Tables[1].Rows[0]["unit"].ToString();
                        }
                        if (!string.IsNullOrEmpty(unitName))
                        {
                            price.unitText = unitName;
                        }
                        else
                        {
                            price.unitText = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                        }

                       
                        price.from = 0;
                        price.to = 1;
                    }
                }
                else
                {
                    IsPriceUpdate = true;
                    recordId = 0;
                    price.Article1 = cArticleNo;
                    price.price1 = price1;
                    price.price2 = price2;
                    price.costPrice1 = cPrice1;
                    price.costPrice2 = cPrice2;
                    price.unit = unit;
                    price.unitText = unitName;
                    price.from = 0;
                    price.to = 1;
                }
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    string FormDesign = entity.DataObjectPages.Where(w => w.DataObjectID == formId).Select(s => s.PageXML).FirstOrDefault().ToString();
                    string formData = JsonConvert.SerializeObject(price);
                    if (IsPriceUpdate)
                    {
                        var result = form.SaveJsonData(formData, formId, FormDesign, recordId, articleId, menuId, true, "ContentExtension");
                    }

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
               
        }

        [HttpGet]
        [ActionName("CreateTest")]
        [IdentityBasicAuthentication]
        [Authorize]
        public IHttpActionResult CreateTest()
        {
            return Ok("hello");
        }
        [HttpGet]
        [ActionName("getArticleListBymenu")]
        [IdentityBasicAuthentication]
        [Authorize]
        public IHttpActionResult GetArticleListBymenu(int menuId)
        {

            ApiResponse response = new ApiResponse();
            if (menuId != 0)
            {
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {
                    SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();


                    string formId = Digimaker.Config.Custom.AppSettings["CE_BookingPriceFormID"].ToString();
                     var  articleData = entity.Articles.Join(entity.Article_MenuItem, a => a.ArticleID, m => m.ArticleID, (A, M) => new { a = A, m = M })
                                                 .Where(w => w.m.MenuItemID == menuId && w.a.Status == 0).Select(s => s.a).ToList();

                    if (articleData.Count > 0) {
                        List<ArticleItem> articleList = new List<ArticleItem>();
                        ArticleItem article;

                        
                        foreach (var item in articleData)
                        {
                            article = new ArticleItem();
                            article.ArticleId = item.MainID??0;
                            article.ArticleName = item.Headline;
                            article.Ingrese = item.Abstract;
                            article.Body = item.Fullstory;
                            article.MenuId = menuId;
                            article.Author = item.Author;
                            article.PublishDate = item.ShowDate.ToString();
                            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { item.MainID??0 }, new string[] { formId });

                            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
                            {
                                if(dsContentExtension.Tables[1].Rows.Count > 1)
                                {
                                    article.Message = "Article has :" + dsContentExtension.Tables[1].Rows.Count + new string(' ', 1) + "price values";
                                }
                                else
                                {
                                    if (dsContentExtension.Tables[1].Rows[0]["Article1"] != null)
                                    {
                                        article.PaId = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["Article1"].ToString());
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["unit"] != null)
                                    {
                                       article.Unit= dsContentExtension.Tables[1].Rows[0]["unit"].ToString();
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["unitText"] != null)
                                    {
                                        article.UnitName = dsContentExtension.Tables[1].Rows[0]["unitText"].ToString();
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["price1"] != null)
                                    {
                                        article.Price1 = Convert.ToDecimal( dsContentExtension.Tables[1].Rows[0]["price1"].ToString());
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["price2"] != null)
                                    {
                                        article.Price2 = Convert.ToDecimal(dsContentExtension.Tables[1].Rows[0]["price2"].ToString());
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["costPrice1"] != null)
                                    {
                                        article.CPrice1 = Convert.ToDecimal(dsContentExtension.Tables[1].Rows[0]["costPrice1"].ToString());
                                    }
                                    if (dsContentExtension.Tables[1].Rows[0]["costPrice2"] != null)
                                    {
                                        article.CPrice2 = Convert.ToDecimal(dsContentExtension.Tables[1].Rows[0]["costPrice2"].ToString());
                                    }
                                }
                               
                               
                            }
                            else
                            {
                                article.Message = "Article has no price values";
                            }
                                articleList.Add(article);
                        }
                        return Ok(articleList);
                    }
                    else
                    {
                        response.message = "No data found";
                        response.errorType = (int)ErrorType.Error;
                        return Ok(response);
                    }
                   
                }
                   
            }
            else
            {
                response.message = "please enter valid menuid";
                response.errorType = (int)ErrorType.Error;
                return Ok(response);
            }
            
        }

     }

}
