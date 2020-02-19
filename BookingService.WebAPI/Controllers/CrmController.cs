using BookingService.WebAPI.DTO;
using BookingService.WebAPI.Models;
using DMBase.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI.Controllers;
using webServices;
using WebAPI.DTO;
using System.IO;
using System.Globalization;
using System.Web;
using System.Data.SqlClient;

namespace BookingService.WebAPI.Controllers
{
    public class CrmController : ApiController
    {
        [HttpGet]
        [ActionName("GetUserDetail")]
        public User GetUserDetail(string userId)
        {
            User user;
            UserBookedEvents bookedEvent;
            List<UserBookedEvents> bookedEvents = new List<UserBookedEvents>();
            List<UserGroupList> userGroupList;
            UserGroupList userGroup;
            int id = 0;
            if (userId != null)
            {
                id = Convert.ToInt32(userId);
            }

            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                user = new User();
                var result = entity.People.Where(w => w.PersonID == id).Select(s => s).FirstOrDefault();
                user.UserId = result.PersonID;
                user.UserFirstName = result.GivenName;
                user.UserLastName = result.FamilyName;
                user.DisplayName = result.DisplayName;
                user.UserEmail = result.Email;
                user.Mobile = result.MobilePhone;
                user.EmployeeId = result.EmployeeNo;
                user.Hide = result.Custom2;
                user.Status = result.Status;
                user.PositionTitle = result.Title;

                var Company = entity.OrganizationUnit_Person.Join(entity.OrganizationUnits, p => p.OrganizationUnitID, o => o.OrganizationUnitID, (P, O) => new { p = P, o = O })
                     .Where(w => w.p.PersonID == result.PersonID).Select(s => s.o.OrganizationUnitName).FirstOrDefault().ToString();
                user.Department = result.Title;
                user.Customer = Company;


                if (result.PictureID != null)
                {
                    user.PicturePath = entity.PictureProperties.Where(w => w.PicturePropertyID == result.PictureID).FirstOrDefault().Filepath.ToString();
                }
                var Events = entity.BookingDetails.Where(w => w.UserID == result.PersonID).Select(s => s).OrderByDescending(x => x.FromDate).ToList();
                if (Events != null)
                {
                    foreach (var item in Events)
                    {
                        bookedEvent = new UserBookedEvents();

                        bookedEvent.buildingName = entity.MenuItems.Where(w => w.MenuItemID == item.BuildingID).Select(s => s.MenuItemName).FirstOrDefault().ToString();
                        bookedEvent.numOfPeople = item.NoOfPeople;
                        bookedEvent.MeetingRoomName = entity.Articles.Where(w => w.ArticleID == item.ServiceID).Select(s => s.Headline).FirstOrDefault().ToString();
                        bookedEvent.ServiceName = item.BookingType;
                        bookedEvent.start = item.FromDate ?? DateTime.Now;
                        bookedEvent.end = item.ToDate ?? DateTime.Now;
                        bookedEvent.title = item.BookingName;
                        bookedEvents.Add(bookedEvent);
                    }
                    user.bookedEvents = bookedEvents;
                }

                var usrResult = entity.UserGroups.Select(s => s).ToList();
                userGroupList = new List<UserGroupList>();
                if (usrResult != null)
                {
                    foreach (var usr in usrResult)
                    {
                        userGroup = new UserGroupList();
                        userGroup.userGroupId = usr.UserGroupId;
                        userGroup.name = usr.UserGroupName;
                        var status = entity.UserGroup_User.Where(w => w.UserId == user.UserId && w.UserGroupId == usr.UserGroupId).FirstOrDefault();
                        if (status != null)
                        { userGroup.assigned = true; }
                        else
                        {
                            userGroup.assigned = false;
                        }
                        userGroupList.Add(userGroup);
                    }
                    user.groupList = userGroupList;
                }


            }
            return user;
        }

        [HttpGet]
        [ActionName("GetServiceGroupDetail")]
        public ServiceGroup GetServiceGroupDetail(string serviceGrpId)
        {
            ServiceGroup serviceGrp;
            int id = 0;
            if (serviceGrpId != null)
            {
                id = Convert.ToInt32(serviceGrpId);
            }

            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                serviceGrp = new ServiceGroup();
                List<Service> articles = new List<Service>();
                Service service;
                var result = entity.MenuItems.Where(w => w.MenuItemID == id).FirstOrDefault();
                serviceGrp.id = result.MenuItemID;
                serviceGrp.name = result.MenuItemName;
                if (result.Pictureid != null)
                {
                    serviceGrp.picturePath = entity.PictureProperties.Where(w => w.PicturePropertyID == result.Pictureid).FirstOrDefault().Filepath.ToString();
                }

                var serviceresult = new ArticlesController().GetAllArticles(result.MenuItemID.ToString());
                foreach (var article in serviceresult)
                {
                    service = new Service();
                    service.id = article.ArticleID;
                    service.name = article.Headline;
                    articles.Add(service);
                }
                serviceGrp.services = articles;
                var content = getServiceGroupContentExtData(serviceGrp.id, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ServiceGroupFormID"].ToString()));
                serviceGrp.mobileApp = content.mobileApp;
                serviceGrp.serviceType = content.serviceType;
                serviceGrp.timeSpan = content.timeSpan;
                serviceGrp.requestOnly = content.requestOnly;
                serviceGrp.reciever = content.reciever;
                serviceGrp.additionalOption = content.additionalOption;
                serviceGrp.capacity = content.capacity;
                serviceGrp.preparationTime = content.preparationTime;
                serviceGrp.cancellation = content.cancellation;
                serviceGrp.cleantime = content.cleantime;
                serviceGrp.from = content.from;
                serviceGrp.to = content.to;


            }
            return serviceGrp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messagetype">1-email, 2-sms</param>
        /// <param name="usertype">1-usergroup, 2-users</param>
        /// <param name="list">user group ids or user ids(sepearated by , )</param>
        /// <param name="body"></param>
        /// <param name="title"></param>
        /// <param name="appended">Appended email or phone numbers sepearated by ,</param>
        /// <returns></returns>
        [HttpGet]
        public string SendMessage(int messagetype, int usertype, string list, string body, string title = "", string appended = "")
        {
            var result = "0";
            // Validation
            if (messagetype == 1 && title.Trim() == "")
            {
                result = "Empty title";
                return result;
            }
            var entities = new BraathenEiendomEntities();
            int[] userIds;
            switch (usertype)
            {
                case 1:
                    var usergroupIds = Array.ConvertAll(list.Split(','), s => int.Parse(s));
                    userIds = entities.UserGroup_User.Where(w => usergroupIds.Contains((int)w.UserGroupId)).Select(s => s.UserId).Cast<int>().ToArray();
                    break;
                case 2:
                    userIds = Array.ConvertAll(list.Split(','), s => int.Parse(s));
                    break;
                default:
                    return "0";

            }

            var appendedList = appended.Trim() == "" ? new string[] { } : appended.Split(',');
            if (appendedList.Length > 0)
            {
                if (messagetype == 1)
                {
                    appendedList = entities.People.Where(w => appendedList.Contains(w.Email)).Select(s => s.Email).ToArray();
                }
                else if (messagetype == 2)
                {
                    appendedList = entities.People.Where(w => appendedList.Contains(w.MobilePhone)).Select(s => s.MobilePhone).ToArray();
                }
            }

            //Send message to list.
            if (userIds.Length > 0 || appended.Length > 0)
            {
                Dictionary<string, string[]> userList = new Dictionary<string, string[]>(); //string[]: 0 - name, 1 - user id
                if (messagetype == 1)
                {
                    userList = entities.People.Where(w => userIds.Contains((int)w.PersonID) && w.Email != "").
                                    Select(s => new { s.Email, s.DisplayName, s.PersonID, s.GivenName, s.FamilyName }).
                                    ToDictionary(t => t.Email, t => new string[] { t.DisplayName, t.PersonID.ToString(), t.GivenName, t.FamilyName });
                }
                else if (messagetype == 2)
                {
                    userList = entities.People.Where(w => userIds.Contains((int)w.PersonID) && w.MobilePhone != "").
                                                        Select(s => new { s.MobilePhone, s.DisplayName, s.PersonID, s.GivenName, s.FamilyName }).
                                                        ToDictionary(t => t.MobilePhone, t => new string[] { t.DisplayName, t.PersonID.ToString(), t.GivenName, t.FamilyName });
                }
                foreach (var item in appendedList)
                {
                    if (!userList.ContainsKey(item))
                    {
                        userList.Add(item, new string[] { null, null });
                    }
                }
                if (userList.Count > 0)
                {
                    if (Digimaker.Directory.Person.Current != null)
                    {
                        var person = (Digimaker.Schemas.Web.PersonViewData.PersonRow)SiteBuilder.Content.Person.ByPersonId(Digimaker.Directory.Person.Current.PersonID).Person.Rows[0];
                        result = Message.SendMessage(messagetype, userList, body, true, title, new string[] { person.Email, person.DisplayName });
                    }
                    else
                    {
                        return "User not login.";
                    }
                }
                else
                {
                    result = "Empty email/mobile to send to.";
                }
            }
            else
            {
                result = "Empty users.";
            }

            return result;
        }

        [HttpGet]
        public int DeleteMessage(int id)
        {
            var result = 0; //0: exception, 1: succeeded. 2: not found
            var entity = new DMBaseEntity();
            var list = entity.DMMessageLogs.Where(w => w.Id == id);
            if (list.Count() == 0)
            {
                result = 2;
            }
            else
            {
                entity.DMMessageLogs.Remove(list.First());
                entity.SaveChanges();
                result = 1;
            }
            return result;
        }

        [HttpGet]
        public MessageLogResult GetMessageLog(int type, int offset = 0, int limit = 0, int user = 0)
        {
            var entity = new DMBaseEntity();
            string typeText;

            if (type == 1)
            {
                typeText = "mail";
            }
            else if (type == 2)
            {
                typeText = "sms";
            }
            else
            {
                typeText = "";
            }

            var total = entity.DMMessageLogs.Where(w => typeText != "" && w.Type == typeText && (user > 0 && w.ToUserId == user || user == 0));
            var count = total.Count();

            List<DMMessageLog> list;
            if (offset > 0 && limit > 0)
            {
                list = total.Skip(offset).Take(limit).OrderByDescending(o => o.Id).ToList();
            }
            else
            {
                list = total.ToList();
            }
            var result = new MessageLogResult();
            result.limit = limit;
            result.offset = offset;
            result.totalCount = count;
            result.list = new List<MessageLogResult.MessageLog>();
            foreach (var messageLog in list)
            {
                var item = new MessageLogResult.MessageLog();
                item.id = messageLog.Id;
                item.type = messageLog.Type;
                item.status = messageLog.Status;
                item.title = messageLog.Title;
                item.body = messageLog.Body;
                item.from = messageLog.From;
                item.to = messageLog.To;
                item.log = messageLog.Log;
                item.created = messageLog.Created;
                result.list.Add(item);
            }

            return result;
        }


        [HttpGet]
        [ActionName("GetServiceDetail")]
        public Service GetServiceDetail(string serviceId)
        {
            Service service;
            int id = 0;
            UserBookedEvents bookedEvent;
            BannerModel banner;
            List<BannerModel> banners = new List<BannerModel>();
            List<UserBookedEvents> bookedEvents = new List<UserBookedEvents>();
            if (serviceId != null)
            {
                id = Convert.ToInt32(serviceId);
            }

            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                service = new Service();

                var result = entity.Articles.Where(w => w.ArticleID == id).FirstOrDefault();
                service.id = result.ArticleID;
                service.name = result.Headline;
                if (result.PictureID != null)
                {
                    service.picturePath = entity.PictureProperties.Where(w => w.PicturePropertyID == result.PictureID).FirstOrDefault().Filepath.ToString();
                }

                var BannerList = entity.Banners.Join(entity.Article_Banner, b => b.BannerId, a => a.BannerId, (B, A) => new { b = B, a = A }).
                    Where(w => w.a.ArticleId == service.id).Select(s => s.b).ToList();

                if (BannerList != null)
                {
                    foreach (var bitem in BannerList)
                    {
                        banner = new BannerModel();
                        banner.BannerId = bitem.BannerId;
                        banner.BannerName = bitem.BannerName;
                        banners.Add(banner);
                    }
                    service.banners = banners;
                }

                var bookingList = entity.BookingDetails.Where(w => w.ServiceID == service.id).Select(s => s).OrderByDescending(x => x.FromDate).ToList();
                if (bookingList != null)
                {
                    foreach (var item in bookingList)
                    {
                        bookedEvent = new UserBookedEvents();
                        if (item.UserID != 0)
                        {
                            var uname = entity.People.Where(w => w.PersonID == item.UserID).FirstOrDefault().DisplayName.ToString();
                            if (uname != null)
                            {
                                bookedEvent.UserName = uname;
                            }
                            else
                            {
                                bookedEvent.UserName = item.BookingName;
                            }
                        }
                        else
                        {
                            bookedEvent.UserName = item.BookingName;
                        }

                        bookedEvent.buildingName = entity.MenuItems.Where(w => w.MenuItemID == item.BuildingID).Select(s => s.MenuItemName).FirstOrDefault().ToString();
                        bookedEvent.numOfPeople = item.NoOfPeople;
                        bookedEvent.MeetingRoomName = entity.Articles.Where(w => w.ArticleID == item.ServiceID).Select(s => s.Headline).FirstOrDefault().ToString();
                        bookedEvent.ServiceName = item.BookingType;
                        bookedEvent.start = item.FromDate ?? DateTime.Now;
                        bookedEvent.end = item.ToDate ?? DateTime.Now;
                        bookedEvent.title = item.BookingName;
                        bookedEvents.Add(bookedEvent);
                    }
                    service.bookedServiceEvents = bookedEvents;
                }

                var content = getServiceContentExtData(service.id, Convert.ToInt32(Digimaker.Config.Custom.AppSettings["CE_ServiceFormID"].ToString()));
                if (content != null)
                {
                    service.mobileApp = content.mobileApp;
                    service.serviceType = content.serviceType;
                    service.timeSpan = content.timeSpan;
                    service.requestOnly = content.requestOnly;
                    service.reciever = content.reciever;
                    service.additionalOption = content.additionalOption;
                    service.capacity = content.capacity;
                    service.preparationTime = content.preparationTime;
                    service.cancellation = content.cancellation;
                    service.cleantime = content.cleantime;
                    service.from = content.from;
                    service.to = content.to;
                }



            }
            return service;
        }
        public ServiceGroup getServiceGroupContentExtData(int serviceGroupId, int FormID)
        {
            ServiceGroup serviceContentExt = new ServiceGroup();
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.MenuItem, new int[] { serviceGroupId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {

                if (dsContentExtension.Tables[1].Rows[0]["mobileapp"] != null)
                    serviceContentExt.mobileApp = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["mobileapp"].ToString());
                serviceContentExt.serviceType = dsContentExtension.Tables[1].Rows[0]["servicetype"].ToString();
                serviceContentExt.timeSpan = dsContentExtension.Tables[1].Rows[0]["timespan"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["requestonly"] != null)
                    serviceContentExt.requestOnly = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["requestonly"].ToString());
                serviceContentExt.reciever = dsContentExtension.Tables[1].Rows[0]["reciever"].ToString();
                serviceContentExt.additionalOption = dsContentExtension.Tables[1].Rows[0]["additionalservice"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["capacity"] != null)
                    serviceContentExt.capacity = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["capacity"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["preparetime"] != null)
                    serviceContentExt.preparationTime = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["preparetime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["cleantime"] != null)
                    serviceContentExt.cleantime = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["cleantime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["cancelationtime"] != null)
                    serviceContentExt.cancellation = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["cancelationtime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["openinghoursfrom"] != null)
                    serviceContentExt.from = dsContentExtension.Tables[1].Rows[0]["openinghoursfrom"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["openingshoursto"] != null)
                    serviceContentExt.to = dsContentExtension.Tables[1].Rows[0]["openingshoursto"].ToString();
            }

            return serviceContentExt;
        }

        public ServiceGroup getServiceContentExtData(int serviceGroupId, int FormID)
        {
            ServiceGroup serviceContentExt = new ServiceGroup();
            SiteBuilder.Content.Extension extData = new SiteBuilder.Content.Extension();
            DataSet dsContentExtension = extData.CeRecords(Digimaker.Schemas.ObjectType.Article, new int[] { serviceGroupId }, new string[] { FormID.ToString() });

            if (dsContentExtension != null && dsContentExtension.Tables[1].Rows.Count > 0)
            {

                if (dsContentExtension.Tables[1].Rows[0]["mobileapp"] != null)
                    serviceContentExt.mobileApp = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["mobileapp"].ToString());
                //serviceContentExt.serviceType = dsContentExtension.Tables[1].Rows[0]["servicetype"].ToString();
                serviceContentExt.timeSpan = dsContentExtension.Tables[1].Rows[0]["timespan"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["requestonly"] != null)
                    serviceContentExt.requestOnly = Convert.ToBoolean(dsContentExtension.Tables[1].Rows[0]["requestonly"].ToString());
                // serviceContentExt.reciever = dsContentExtension.Tables[1].Rows[0]["reciever"].ToString();
                serviceContentExt.additionalOption = dsContentExtension.Tables[1].Rows[0]["additionalservice"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["capacity"] != null)
                    serviceContentExt.capacity = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["capacity"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["preparetime"] != null)
                    serviceContentExt.preparationTime = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["preparetime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["cleantime"] != null)
                    serviceContentExt.cleantime = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["cleantime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["cancelationtime"] != null)
                    serviceContentExt.cancellation = Convert.ToInt32(dsContentExtension.Tables[1].Rows[0]["cancelationtime"].ToString());
                if (dsContentExtension.Tables[1].Rows[0]["openinghoursfrom"] != null)
                    serviceContentExt.from = dsContentExtension.Tables[1].Rows[0]["openinghoursfrom"].ToString();
                if (dsContentExtension.Tables[1].Rows[0]["penttil"] != null)
                    serviceContentExt.to = dsContentExtension.Tables[1].Rows[0]["penttil"].ToString();
            }

            return serviceContentExt;
        }

        [HttpGet]
        [ActionName("HideUser")]
        public int HideUser(int userId)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var query =
                                 from ur in entity.People
                                 where ur.PersonID == userId
                                 select ur;

                        // Execute the query, and change the column values
                        // you want to change
                        foreach (var urs in query)
                        {
                            urs.Custom2 = "hide";
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
        [ActionName("UpdateMVACustomer")]
        public int UpdateMVACustomer(bool isMva, int orgId)
        {
            int res = 0;
            try
            {
                using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
                {

                    var result = entity.OrganizationUnits.Where(x => x.OrganizationUnitID == orgId).FirstOrDefault();
                    if (result != null)
                    {
                        if (isMva)
                        {
                            if (!string.IsNullOrEmpty(result.Code))
                            {
                                result.Code = string.Empty;
                            }

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(result.Code))
                            {
                                result.Code = "NoMVA";
                            }

                        }

                        entity.SaveChanges();
                        res = 1;
                    }


                }
            }
            catch (Exception ex)
            {

                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(string.Format(ex.Message)),
                    ReasonPhrase = "MVA not set !!"
                };

                throw new HttpResponseException(response);
            }
            return res;
        }
        [HttpGet]
        [ActionName("OrderSendToVismaGlobal")]
        public int OrderSendToVismaGlobal(string orderIds)
        {
            int mOrderNo = 0;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                try
                {

                    List<int> TagIds = orderIds.Split(',').Select(int.Parse).ToList();
                    WebServices ws = new WebServices();
                    // GetToken
                    string token = ws.GetToken();
                    // ERPClient
                    string ERPClient = Digimaker.Config.Custom.AppSettings["ERPClient"].ToString();

                    List<WebServices.OrderHead> OrderHeadList = new List<WebServices.OrderHead>();
                    WebServices.OrderHead orderhead;
                    WebServices.OrderLines OrderLine;

                    var res = entity.Orderheads.Where(e => TagIds.Contains(e.OrderNo));
                    foreach (var item in res)
                    {
                        mOrderNo = item.OrderNo;
                        // cheking 
                        if (token == "")
                        {
                            UpdateVMOrderFailedErrorMessage(mOrderNo, "Token not generated");
                        }
                        else
                        {
                            orderhead = new WebServices.OrderHead();
                            List<WebServices.OrderLines> OrderLineList = new List<WebServices.OrderLines>();

                            // get CompanyNo
                            int mOrganizationUnitID = Convert.ToInt32(item.CustomerNo);
                            string CompanyNo = entity.OrganizationUnits.Where(w => w.OrganizationUnitID == mOrganizationUnitID).Select(s => s.OrgNumber).FirstOrDefault();

                            //string CompanyNo = "981175328";
                            // GetCustomer
                            string CustomerNo = ws.GetCustomer(token, item.ERPClient, CompanyNo);
                            orderhead.OrderNo = item.OrderNo;

                            orderhead.CustomerNo = CustomerNo;
                            orderhead.CustomerGroupNo = "null";
                            orderhead.OrderDate = item.Orderdate.ToString("yyyy-MM-dd HH:mm:ss.fff"); //"2018-11-18T10:54:40.4405083+01:00"
                            orderhead.OurReference = item.OurReference;
                            orderhead.YourReference = item.YourReference;
                            orderhead.EmailAddress = item.EmailAddress;
                            orderhead.ERPClient = item.ERPClient;
                            orderhead.CustomerPurchaseNo = item.bookingId;
                            DateTime mfromdate = (DateTime)item.FromDate;
                            orderhead.DeliveryDate = mfromdate.ToString("yyyy-MM-dd HH:mm:ss.fff"); //"2018-11-16T10:54:40.4405083+01:00"; // item.FromDate;

                            //// OrderLine
                            var resOrderLine = from ord in entity.Orderlines
                                               where ord.OrderHeadId == item.OrderNo && ord.Status == false
                                               orderby ord.SortOrder
                                               select ord;

                            foreach (var it in resOrderLine)
                            {
                                OrderLine = new WebServices.OrderLines();
                                if (it.ArticleId == "0")
                                {
                                    OrderLine.ArticleNo = "";
                                }
                                else
                                {
                                    OrderLine.ArticleNo = it.ArticleId;
                                }
                                OrderLine.Text = it.Tekst;
                                OrderLine.Quantity = Convert.ToDouble( it.Qty);
                                OrderLine.DiscountPercent = 0;
                                OrderLine.NetAmount = it.Price;
                                OrderLine.GrossAmount = 0;
                                OrderLineList.Add(OrderLine);
                            }
                            orderhead.OrderLines = OrderLineList;
                            // PostOrder - saikat
                            string OrderNo = ws.PostOrder(token, orderhead);
                            //  OrderHeadList.Add(orderhead);
                            if (!string.IsNullOrEmpty(OrderNo))
                            {
                                UpdateVismaOrderNo(item.OrderNo, OrderNo);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateVMOrderFailedErrorMessage(mOrderNo, ex.Message);
                    //exception hadle new way -savan
                    var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(string.Format(ex.Message)),
                        ReasonPhrase = "Order Not Sent"
                    };

                    throw new HttpResponseException(response);
                    //return 0;
                }
            }
            return 1;
        }
        public int UpdateVismaOrderNo(int OrderNo, string VismaOrderNo)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var query =
                                 from ord in entity.Orderheads
                                 where ord.OrderNo == OrderNo
                                 select ord;

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (var ord in query)
                        {
                            if (!string.IsNullOrEmpty(VismaOrderNo))
                            {
                                ord.VismaOrderNo = Convert.ToInt32(VismaOrderNo);
                            }

                            ord.VismaOrderDate = System.DateTime.Now;
                            ord.VMOrderFailedErrorMessage = "";
                            // Insert any additional changes to column values.
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
            return 1;
        }

        public int UpdateVMOrderFailedErrorMessage(int OrderNo, string VMOrderFailedErrorMessage)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var query = from ord in entity.Orderheads
                                    where ord.OrderNo == OrderNo
                                    select ord;
                        foreach (var ord in query)
                        {
                            ord.VMOrderFailedErrorMessage = VMOrderFailedErrorMessage;
                            // Insert any additional changes to column values.
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
            return 1;
        }
        [HttpGet]
        [ActionName("BookingsListDisplay")]
        public List<OrderListDisplay> BookingsListDisplay()
        {
            // bookings list Tab 1
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                //Basis is bookings where orderlines is related to bookingID and OrderID = 0 - Saikat

                var BookingServiceList = entity.BookingServiceLists.Where(w => w.OrderHeadId == 0 && w.Status == false ).GroupBy(g => g.BookingID).Where(gw => gw.Sum(s => s.Price *  s.Qty) != 0).Select(s => s.Key).ToList();
                var OrderheadsList = entity.BookingDetails.Where(e => BookingServiceList.Contains(e.BookingID));
                

                List<OrderListDisplay> Orderlists = new List<OrderListDisplay>();
                foreach (var order in OrderheadsList)
                {
                    OrderListDisplay Orderlist = new OrderListDisplay();
                    //var BookingDetails = entity.BookingDetails.Where(b => b.BookingID == order.BookingID).Select(s => s).SingleOrDefault();
                    //Orderlist.VGOrderNo = order.VismaOrderNo;
                    //Orderlist.OrderID = order.OrderNo;
                    Orderlist.BookingID = order.BookingID;
                    Orderlist.BuildingName = entity.MenuItems.Where(w => w.MenuItemID == order.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                    Orderlist.IsKitchen = (order.BookingServiceLists.Count(s => s.IsKitchen == true) > 0) ? true : false;
                    Orderlist.IsDelivere = order.IsDeliver;
                    Orderlist.IsCleaned = order.IsClean;
                    Orderlist.MeetingroomName = entity.Articles.Where(w => w.MainID == order.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                    Orderlist.NoOfPerson = order.NoOfPeople;
                    Orderlist.FromDate = order.FromDate;
                    Orderlist.Title = order.BookingName;
                    Orderlist.TotalOrderAmount = (double)order.BookingServiceLists.Where(w => w.Status == false && w.OrderHeadId == 0).Sum(s => s.NetAmount);

                    Orderlist.BuildingID = order.BuildingID;
                    Orderlist.ServiceID = order.ServiceID;
                    Orderlist.Customer = order.Customer;
                    Orderlist.UserID = order.UserID;
                    Orderlist.ServiceType = order.ServiceType;

                    Orderlist.CustomerName = entity.OrganizationUnits.Where(o => o.OrganizationUnitID == order.Customer).Select(s => s.OrganizationUnitName).FirstOrDefault();
                    Orderlist.User = entity.People.Where(p => p.PersonID == order.UserID).Select(s => s.DisplayName).FirstOrDefault() + " (" + Orderlist.CustomerName + ")";
                    Orderlists.Add(Orderlist);
                }
                return Orderlists;
            }
        }
        [HttpGet]
        [ActionName("OrdersListDisplay")]
        public List<OrderListDisplay> OrdersListDisplay()
        {
            // orders Tab 2
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                // Basis is orderheads where orderlines is related to OrderID and Orderhead is not Confirmed  - Saikat
                var BookingServiceList = entity.Orderlines.Where(w => w.OrderHeadId != 0 && w.Status == false).GroupBy(g => g.OrderHeadId).Where(gw => gw.Sum(s => s.Price *  s.Qty) != 0).Select(s => s.Key).ToList();
                var OrderheadsList = entity.Orderheads.Where(w => BookingServiceList.Contains(w.OrderNo) && w.IsOrderConfirm == false)
                                                        .OrderBy(o => o.OrderNo)
                                                        .Select(s => s).ToList();
                List<OrderListDisplay> Orderlists = new List<OrderListDisplay>();
                foreach (var order in OrderheadsList)
                {
                    OrderListDisplay Orderlist = new OrderListDisplay();
                    var BookingDetails = entity.BookingDetails.Where(b => b.BookingID == order.bookingId).Select(s => s).SingleOrDefault();
                    Orderlist.VGOrderNo = order.VismaOrderNo;
                    Orderlist.OrderID = order.OrderNo;
                    Orderlist.BookingID = order.bookingId;
                    if (BookingDetails != null)
                    {
                        Orderlist.BuildingName = entity.MenuItems.Where(w => w.MenuItemID == BookingDetails.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                        Orderlist.MeetingroomName = entity.Articles.Where(w => w.MainID == BookingDetails.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                        Orderlist.BuildingID = BookingDetails.BuildingID;
                        Orderlist.ServiceID = BookingDetails.ServiceID;
                        Orderlist.UserID = BookingDetails.UserID;
                        Orderlist.ServiceType = BookingDetails.ServiceType;
                    }

                    Orderlist.IsKitchen = false;
                    Orderlist.IsDelivere = order.IsDeliver;
                    Orderlist.IsCleaned = order.IsClean;
                   
                    Orderlist.NoOfPerson = order.NoOFPeople;
                    Orderlist.FromDate = order.FromDate;
                    Orderlist.Title = order.CustomerName;
                    Orderlist.TotalOrderAmount = (double)entity.Orderlines.Where(w => w.Status == false && w.OrderHeadId == order.OrderNo).Sum(s => s.NetAmount);


                   // 
                    Orderlist.Customer = Convert.ToInt32( order.CustomerNo);
                    // 

                    Orderlist.CustomerName = order.CustomerName;
                    Orderlist.User = order.OurReference + " (" + Orderlist.CustomerName + ")";
                    Orderlists.Add(Orderlist);
                }
                return Orderlists;
            }
        }
        [HttpGet]
        [ActionName("ConfirmedListDisplay")]
        public List<OrderListDisplay> ConfirmedListDisplay()
        {
            // Confirmed tab 3
            // Basis is orderheads where orderlines i related to OrderID and Orderhead is Confirmed and VG Orderno = 0 - saikat
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var OrderheadsList = entity.Orderheads.Where(w => w.IsOrderConfirm == true && w.VismaOrderNo == 0)
                                                        .OrderBy(o => o.OrderNo)
                                                        .Select(s => s).ToList();
                List<OrderListDisplay> Orderlists = new List<OrderListDisplay>();
                foreach (var order in OrderheadsList)
                {
                    OrderListDisplay Orderlist = new OrderListDisplay();
                    var BookingDetails = entity.BookingDetails.Where(b => b.BookingID == order.bookingId).Select(s => s).SingleOrDefault();
                    Orderlist.VGOrderNo = order.VismaOrderNo;
                    Orderlist.OrderID = order.OrderNo;
                    Orderlist.BookingID = order.bookingId;
                    if (BookingDetails != null)
                    {
                        Orderlist.BuildingName = entity.MenuItems.Where(w => w.MenuItemID == BookingDetails.BuildingID).Select(s => s.MenuItemName).FirstOrDefault(); ;
                        Orderlist.MeetingroomName = entity.Articles.Where(w => w.MainID == BookingDetails.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                        Orderlist.BuildingID = BookingDetails.BuildingID;
                        Orderlist.ServiceID = BookingDetails.ServiceID;
                        Orderlist.UserID = BookingDetails.UserID;
                        Orderlist.ServiceType = BookingDetails.ServiceType;
                    }

                    Orderlist.IsKitchen = false;
                    Orderlist.IsDelivere = order.IsDeliver;
                    Orderlist.IsCleaned = order.IsClean;
                  
                    Orderlist.NoOfPerson = order.NoOFPeople;
                    Orderlist.FromDate = order.FromDate;
                    Orderlist.Title = order.CustomerName;
                    Orderlist.TotalOrderAmount = (double)entity.Orderlines.Where(w => w.Status == false && w.OrderHeadId == order.OrderNo).Sum(s => s.NetAmount);
                    Orderlist.VMOrderFailedErrorMessage = order.VMOrderFailedErrorMessage;

                  
                    if (!string.IsNullOrEmpty(order.CustomerNo))
                    {
                        Orderlist.Customer =Convert.ToInt32( order.CustomerNo);
                    }

                  

                    Orderlist.CustomerName = order.CustomerName;
                    Orderlist.User = order.OurReference;
                    Orderlists.Add(Orderlist);
                }
                return Orderlists;
            }
        }
        [HttpGet]
        [ActionName("TransferedListDisplay")]
        public List<OrderListDisplay> TransferedListDisplay()
        {
            // Transfered tab 4
            var userServices = UserAccess.GetCurrentUserServices();
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var OrderheadsList = entity.Orderheads.Where(w => w.VismaOrderNo > 0)
                                                        .OrderBy(o => o.VismaOrderNo)
                                                        .Select(s => s).ToList();
                List<OrderListDisplay> Orderlists = new List<OrderListDisplay>();
                foreach (var order in OrderheadsList)
                {
                    OrderListDisplay Orderlist = new OrderListDisplay();
                    var BookingDetails = entity.BookingDetails.Where(b => b.BookingID == order.bookingId).Select(s => s).SingleOrDefault();
                    Orderlist.VGOrderNo = order.VismaOrderNo;
                    Orderlist.OrderID = order.OrderNo;
                    Orderlist.BookingID = order.bookingId;
                    if (BookingDetails!=null)
                    {
                        Orderlist.BuildingName = entity.MenuItems.Where(w => w.MenuItemID == BookingDetails.BuildingID).Select(s => s.MenuItemName).FirstOrDefault();
                        Orderlist.MeetingroomName = entity.Articles.Where(w => w.MainID == BookingDetails.ServiceID && w.Status == 0).Select(s => s.Headline).FirstOrDefault();
                        Orderlist.BuildingID = BookingDetails.BuildingID;
                        Orderlist.ServiceID = BookingDetails.ServiceID;
                        Orderlist.UserID = BookingDetails.UserID;
                        Orderlist.ServiceType = BookingDetails.ServiceType;
                    }
                    // 
                    Orderlist.IsKitchen = false;
                    Orderlist.IsDelivere = order.IsDeliver;
                    Orderlist.IsCleaned = order.IsClean;
                   //
                    Orderlist.NoOfPerson = order.NoOFPeople;
                    Orderlist.FromDate = order.FromDate;
                    Orderlist.Title = order.CustomerName;
                    Orderlist.TotalOrderAmount = (double)order.Orderlines.Where(w => w.Status == false).Sum(s => s.NetAmount);

                    //  
                    if (!string.IsNullOrEmpty(order.CustomerNo))
                    {
                        Orderlist.Customer = Convert.ToInt32(order.CustomerNo);
                    }
                    // 

                    Orderlist.CustomerName = order.CustomerName;
                    Orderlist.User = order.OurReference;
                    Orderlists.Add(Orderlist);
                }
                return Orderlists;
            }
        }



        [HttpGet]
        [ActionName("BookingsSendToConfirm")]
        public int BookingsSendToConfirm(string orderIds,string erpClient)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        List<int> TagIds = orderIds.Split(',').Select(int.Parse).ToList();
                        var query = entity.BookingDetails.Where(e => TagIds.Contains(e.BookingID));

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (var bookingtb in query)
                        {
                            Orderhead orderhead = new Orderhead();
                            orderhead.Orderdate = DateTime.Now;
                            orderhead.Ordertype = entity.MenuItems.Where(w => w.MenuItemID == bookingtb.ServiceType).Select(s => s.MenuItemName).FirstOrDefault();
                            orderhead.Status = "Confirmed";
                            orderhead.VAT = 0;
                            orderhead.CustomerName = bookingtb.BookingName;
                            orderhead.CustomerNo = bookingtb.Customer.ToString();
                            orderhead.EmailAddress = "";
                            orderhead.ERPClient = erpClient;
                            BookingController username = new BookingController();
                            orderhead.YourReference = username.getUserName(bookingtb.UserID);
                            orderhead.OurReference = Digimaker.User.Identity.DisplayName;
                            orderhead.NoOFPeople = bookingtb.NoOfPeople;
                            orderhead.FromDate = bookingtb.FromDate;
                            orderhead.Todate = bookingtb.ToDate;
                            orderhead.IsClean = false;
                            orderhead.IsDeliver = false;
                            orderhead.ISVismaOrder = false;
                            orderhead.IsOrderConfirm = false;
                            orderhead.VismaOrderNo = 0;
                            orderhead.VMOrderFailedErrorMessage = "";
                            orderhead.bookingId = bookingtb.BookingID;
                            entity.Orderheads.Add(orderhead);
                            entity.SaveChanges();

                            // update the BookingServiceLists table orderno
                            var bookinglist = entity.BookingServiceLists.Where(e => e.BookingID == bookingtb.BookingID && e.OrderHeadId == 0 && e.Status==false).ToList();

                            //savan start
                                if (bookinglist.Count > 0)
                                    {
                                        Orderline _foodDetail;
                                        bool isService = true;
                                        List<Orderline> flist = new List<Orderline>();
                                        foreach (var item in bookinglist)
                                        {
                                            _foodDetail = new Orderline();
                                    _foodDetail.Qty = item.Qty;
                                    _foodDetail.Status = item.Status;
                                    _foodDetail.Sum = item.Sum;
                                    _foodDetail.Tekst = item.Tekst; 
                                    _foodDetail.ServiceName = item.ServiceName;
                                    _foodDetail.OrderHeadId = orderhead.OrderNo;
                                    _foodDetail.BookingID = item.BookingID;
                                    _foodDetail.IsKitchen = item.IsKitchen;
                                    _foodDetail.IsVat = item.IsVat;
                                    _foodDetail.CostPrice = item.CostPrice;
                                    _foodDetail.CostTotal = item.CostTotal;
                                    _foodDetail.UnitText = item.UnitText;
                                    _foodDetail.NetAmount = item.NetAmount;
                                    _foodDetail.VATPercent = item.VATPercent;
                                    _foodDetail.Time = item.Time;
                                    _foodDetail.Amount = item.Amount;

                                    _foodDetail.Price = item.Price;
                                    _foodDetail.IsMainService = item.IsMainService;

                                    _foodDetail.ArticleId = item.ArticleId;
                                    _foodDetail.FoodID = item.FoodID;



                                 
                                            


                                            flist.Add(_foodDetail);



                                        }
                                       
                                        entity.Orderlines.AddRange(flist);
                                        entity.SaveChanges();
                                    }
                            //savan end
                            foreach (var ord in bookinglist)
                            {
                                ord.OrderHeadId = orderhead.OrderNo;
                            }
                            entity.SaveChanges();
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
            return 1;
        }


        [HttpGet]
        [ActionName("OrderSendToConfirm")]
        public int OrderSendToConfirm(string orderIds)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        List<int> TagIds = orderIds.Split(',').Select(int.Parse).ToList();
                        var query = entity.Orderheads.Where(e => TagIds.Contains(e.OrderNo));

                        // Execute the query, and change the column values
                        // you want to change.
                        foreach (var ord in query)
                        {
                            ord.IsOrderConfirm = true;
                            // Insert any additional changes to column values.
                        }
                        entity.SaveChanges();
                        dbContextTransaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return 0;
                    }
                }
            }
            return 1;
        }

        [HttpGet]
        [ActionName("GetNoteDisplay")]
        public List<NoteDisplayList> GetNoteDisplay(int CompanyId, int UserId)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {

                var lists = entity.Notes.Where(w => w.CompanyId == CompanyId && w.UserId == UserId)
                                        .OrderBy(o => o.CreateDate)
                                        .Select(s => s).ToList();

                List<NoteDisplayList> Notelists = new List<NoteDisplayList>();
                foreach (var lst in lists)
                {
                    NoteDisplayList list = new NoteDisplayList();
                    list.Id = lst.Id;
                    list.CompanyId = lst.CompanyId;
                    list.UserId = lst.UserId;
                    list.NoteText = lst.NoteText;
                    list.CreateDate = lst.CreateDate;
                    Notelists.Add(list);
                }
                return Notelists;
            }
        }

        [HttpGet]
        [ActionName("DeleteNote")]
        public int DeleteNote(int Id)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var itemToRemove = entity.Notes.SingleOrDefault(x => x.Id == Id);
                        if (itemToRemove != null)
                        {
                            entity.Notes.Remove(itemToRemove);
                            entity.SaveChanges();
                        }
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
        [ActionName("AddNote")]
        public int AddNote(int CompanyId, int UserId, string NoteText)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        var std = new Note()
                        {
                            CompanyId = CompanyId,
                            UserId = UserId,
                            NoteText = NoteText,
                            CreateDate = System.DateTime.Now
                        };
                        entity.Notes.Add(std);
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
        [ActionName("clearCache")]
        public bool ClearCache(string filePrefix)
        {
            bool status = false;
            string filesToDelete = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(filePrefix))
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + ("cache/");
                    System.IO.DirectoryInfo di = new DirectoryInfo(path);

                    if (filePrefix == "all")
                    {
                        filesToDelete = @"*.cache";
                    }
                    else { filesToDelete = @"*" + filePrefix + "*.cache"; }

                    var files = di.GetFiles(filesToDelete);
                    var filecount = di.GetFiles(filesToDelete).Length;
                    if (filecount > 0)
                    {
                        foreach (FileInfo file in di.GetFiles(filesToDelete))
                        {
                            file.Delete();
                        }
                        status = true;
                    }
                    else
                    {
                        status = false;
                    }
                }

            }
            catch (Exception)
            {

                status = false;
            }

            return status;


        }

        [HttpGet]
        [ActionName("AddUpdateAgreementTypes")]
        public int AddUpdateAgreementTypes(int AgreementTypeId, string Name, string Article, string Text, string Price, int NumberOfMonths)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        if (AgreementTypeId > 0)
                        {
                            var query =
                                     from ag in entity.AgreementTypes
                                     where ag.AgreementTypeId == AgreementTypeId
                                     select ag;

                            // Execute the query, and change the column values
                            // you want to change.
                            foreach (var ag in query)
                            {
                                ag.Name = Name;
                                ag.Article = Article;
                                ag.Text = Text;
                                string mPrice = Price.Replace(",", ".");
                                ag.Price = decimal.Parse(mPrice, CultureInfo.InvariantCulture);
                                ag.NumberOfMonths = NumberOfMonths;
                                // Insert any additional changes to column values.
                            }
                            entity.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            string mPrice = Price.Replace(",", ".");
                            var std = new AgreementType()
                            {
                                Name = Name,
                                Article = Article,
                                Text = Text,
                                Price = decimal.Parse(mPrice, CultureInfo.InvariantCulture),
                                NumberOfMonths = NumberOfMonths
                            };
                            entity.AgreementTypes.Add(std);
                            entity.SaveChanges();
                            dbContextTransaction.Commit();
                        }
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
        [ActionName("AddUpdateAgreements")]
        public int AddUpdateAgreements(int AgreementId, int Customer, int Contact, int AgreementTypeId, string FromDate, string ToDate, string Article, string Text, string Price, int NumberOfMonths, string InvoicedToDate)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    try
                    {
                        if (AgreementId > 0)
                        {
                            var query =
                                 from ag in entity.Agreements
                                 where ag.AgreementId == AgreementId
                                 select ag;

                            // Execute the query, and change the column values
                            // you want to change.
                            foreach (var ag in query)
                            {
                                if (Customer == 0)
                                {
                                    ag.Customer = entity.OrganizationUnit_Person.Where(w => w.PersonID == Contact).Select(s => s.OrganizationUnitID).FirstOrDefault();
                                }
                                else
                                {
                                    ag.Customer = Customer;
                                }
                                ag.Contact = Contact;
                                ag.AgreementTypeId = AgreementTypeId;
                                ag.FromDate = Convert.ToDateTime(FromDate);
                                if (!string.IsNullOrEmpty(ToDate) && ToDate != "null")
                                {
                                    ag.ToDate = Convert.ToDateTime(ToDate);
                                }
                                else
                                {
                                    ag.ToDate = null;
                                }
                                if (!string.IsNullOrEmpty(InvoicedToDate) && InvoicedToDate != "null")
                                {
                                    ag.InvoicedToDate = Convert.ToDateTime(InvoicedToDate);
                                }
                                else
                                {
                                    ag.InvoicedToDate = null;
                                }

                                ag.Article = Article;
                                ag.Text = Text;
                                string mPrice = Price.Replace(",", ".");
                                ag.Price = decimal.Parse(mPrice, CultureInfo.InvariantCulture);
                                ag.NumberOfMonths = NumberOfMonths;
                                // Insert any additional changes to column values.
                            }
                            entity.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            DateTime? dt;
                            if (!string.IsNullOrEmpty(ToDate))
                            {
                                dt = Convert.ToDateTime(ToDate);
                            }
                            else
                            {
                                dt = null;
                            }
                            DateTime? invoicedt;
                            DateTime invoicedtyesterday = Convert.ToDateTime(FromDate).AddDays(-1);
                            if (!string.IsNullOrEmpty(InvoicedToDate))
                            {
                                invoicedt = Convert.ToDateTime(InvoicedToDate);
                            }
                            else
                            {
                                invoicedt = invoicedtyesterday;
                            }
                            int mCustomer;
                            if (Customer == 0)
                            {
                                mCustomer = entity.OrganizationUnit_Person.Where(w => w.PersonID == Contact).Select(s => s.OrganizationUnitID).FirstOrDefault();
                            }
                            else
                            {
                                mCustomer = Customer;
                            }
                            string mPrice = Price.Replace(",", ".");
                            var std = new Agreement()
                            {
                                Customer = mCustomer,
                                Contact = Contact,
                                AgreementTypeId = AgreementTypeId,
                                FromDate = Convert.ToDateTime(FromDate),
                                ToDate = dt,
                                Article = Article,
                                Text = Text,
                                Price = decimal.Parse(mPrice, CultureInfo.InvariantCulture),
                                NumberOfMonths = NumberOfMonths,
                                InvoicedToDate = invoicedt
                            };
                            entity.Agreements.Add(std);
                            entity.SaveChanges();
                            dbContextTransaction.Commit();
                        }
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
        [ActionName("GetAgreementTypes")]
        public List<AgreementType> GetAgreementTypes()
        {
            List<AgreementType> AgreementTypeList = new List<AgreementType>();
            AgreementType AgreementType;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                var agType = entity.AgreementTypes.ToList();
                foreach (var line in agType)
                {
                    AgreementType = new AgreementType();
                    AgreementType.AgreementTypeId = line.AgreementTypeId;
                    AgreementType.Name = line.Name;
                    AgreementType.Article = line.Article;
                    AgreementType.Text = line.Text;
                    AgreementType.Price = line.Price;
                    AgreementType.NumberOfMonths = line.NumberOfMonths;
                    AgreementTypeList.Add(AgreementType);
                }
            }
            return AgreementTypeList;
        }

        [HttpGet]
        [ActionName("GetAgreements")]
        public List<Agreements> GetAgreements(int orgId, int userid)
        {
            List<Agreements> AgreementList = new List<Agreements>();
            Agreements Agreement;
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                //List<Agreements> agType = new List<Agreements>();
                var agType = entity.Agreements.ToList();
                if (orgId > 0)
                {
                    agType = entity.Agreements.Where(u => u.Customer == orgId).ToList();
                }
                else
                {
                    agType = entity.Agreements.Where(u => u.Contact == userid).ToList();
                }
                foreach (var line in agType)
                {
                    Agreement = new Agreements();
                    Agreement.AgreementId = line.AgreementId;
                    Agreement.Customer = (int)line.Customer;
                    Agreement.Contact = (int)line.Contact;
                    Agreement.ContactName = entity.People.Where(w => w.PersonID == line.Contact).Select(s => s.DisplayName).FirstOrDefault();
                    Agreement.AgreementTypeId = (int)line.AgreementTypeId;
                    Agreement.AgreementTypeName = entity.AgreementTypes.Where(w => w.AgreementTypeId == line.AgreementTypeId).Select(s => s.Name).FirstOrDefault();
                    Agreement.FromDate = (DateTime)line.FromDate;
                    Agreement.InvoicedToDate = line.InvoicedToDate;
                    if (line.ToDate != null)
                    {
                        Agreement.ToDate = (DateTime)line.ToDate;
                    }
                    else
                    {
                        Agreement.ToDate = null;
                    }
                    Agreement.Article = line.Article;
                    Agreement.Text = line.Text;
                    if (line.Price != null)
                    {
                        Agreement.Price = (Decimal)line.Price;
                    }
                    else
                    {
                        Agreement.Price = 0;
                    }
                    if (line.NumberOfMonths != null)
                    {
                        Agreement.NumberOfMonths = (int)line.NumberOfMonths;
                    }
                    else
                    {
                        Agreement.NumberOfMonths = 0;
                    }
                    AgreementList.Add(Agreement);
                }
            }
            return AgreementList;
        }

        public decimal FenistrafilePriceChangeByArticle(string Article, decimal Price )
        {
            decimal mPrice=0;
            if(Article == "3101")
            {
                mPrice = Price / (decimal)1.25;
            }
            if (Article == "3001")
            {
                mPrice = Price;
            }
            if (Article == "3102")
            {
                mPrice = Price / (decimal)1.25;
            }
            if (Article == "3002")
            {
                mPrice = Price;
            }
            return mPrice;
        }
        public string FenistrafileTextChangeByArticle(string Article, string FakturajournalText)
        {
            string mText = "";
            if (Article == "3102"  || Article == "3002")
            {
                mText = "Parkering " + new string(' ', 1) + FakturajournalText; 
            }
            else
            {
                mText = "Kontorleie" + new string(' ', 1) + FakturajournalText;
            }

            return mText;
        }

        [HttpPost]
        [ActionName("OrderExecute")]
        public int OrderExecute(string bookingIDs, string AgreementToDate, string FakturajournalToDate, decimal SplittPercent, string AgreementText, string FakturajournalText, int IsSplitting,int IsIncludeAgreement,string erpClient)
        {
            using (BraathenEiendomEntities entity = new BraathenEiendomEntities())
            {
                using (var dbContextTransaction = entity.Database.BeginTransaction())
                {
                    List<CustomerBookingOrderHead> customerwisebookingList =new List<CustomerBookingOrderHead>();
                    try
                    {
                        Orderhead orderhead;
                        Orderline orderline;
                        List<CustomerAgreement> CustomerAgreement;
                        List<CustomerAgreement> CustomerAgreementlines;
                        List<CustomerBookingOrder> CustomerBooking;
                        List<CustomerBookingOrder> CustomerBookingLines;
                        List<CustomerFenistraFile> CustomerFenistra;
                        List<CustomerFenistraFile> CustomerFenistraGroup;
                       
                        CustomerBookingOrderHead customerwisebooking;
                        List<CustomerFenistraFile> CustomerFenistraAllData=new List<CustomerFenistraFile>();
                        List<CustomerListForValid> CustomerDigimaker = CustomerListForValidFenistraFile(bookingIDs);
                        if (HttpContext.Current.Request.Files.Count > 0)
                        {
                           CustomerFenistraAllData = CustomerFenistraFile();
                        }
                       
                        foreach (var line in CustomerDigimaker)
                        {
                            int inc = 1;
                            List<Orderline> olist = new List<Orderline>();
                             CustomerAgreement = new List<CustomerAgreement>();
                             CustomerBooking = new List<CustomerBookingOrder>();
                            CustomerBookingLines = new List<CustomerBookingOrder>();
                            CustomerAgreementlines = new List<CustomerAgreement>();
                            CustomerFenistra = new List<CustomerFenistraFile>();
                            CustomerFenistraGroup = new List<CustomerFenistraFile>();

                          
                               // var OrganizationUnitID = entity.OrganizationUnits.Where(o => o.Custom1 == line.Custom1).Select(s => s.OrganizationUnitID).FirstOrDefault();
                                if (IsIncludeAgreement == 1)
                                {
                                    CustomerAgreement = CustomerAgreementDetail(AgreementToDate, AgreementToDate, AgreementToDate, line.OrganizationUnitID, AgreementText);
                                    CustomerAgreementlines = CustomerWiseAgreement(AgreementToDate, AgreementToDate, AgreementToDate, line.OrganizationUnitID);
                                }
                                if (!string.IsNullOrEmpty(bookingIDs))
                                {
                                    CustomerBooking = CustomerBookingOrder(bookingIDs, line.OrganizationUnitID, SplittPercent, IsSplitting);
                                    CustomerBookingLines = CustomerBookingOrderDetail(bookingIDs, line.OrganizationUnitID);
                                }
                               

                                orderhead = new Orderhead();
                                orderhead.Orderdate = DateTime.Now;
                                orderhead.Ordertype = "Samleordre";
                                orderhead.Status = "Confirmed";
                                orderhead.VAT = 0;
                                orderhead.CustomerName =entity.OrganizationUnits.Where(x=>x.OrganizationUnitID==line.OrganizationUnitID).Select(s=>s.OrganizationUnitName).FirstOrDefault();
                                orderhead.CustomerNo = line.OrganizationUnitID.ToString();
                                orderhead.EmailAddress = "";
                                orderhead.ERPClient = erpClient;
                                BookingController username = new BookingController();
                                orderhead.YourReference = "";
                                orderhead.OurReference = Digimaker.User.Identity.DisplayName;
                                orderhead.NoOFPeople = 0;
                                orderhead.FromDate = DateTime.Now;
                                orderhead.Todate = DateTime.Now;
                                orderhead.IsClean = false;
                                orderhead.IsDeliver = false;
                                orderhead.ISVismaOrder = false;
                                orderhead.IsOrderConfirm = false;
                                orderhead.VismaOrderNo = 0;
                                orderhead.VMOrderFailedErrorMessage = "";
                                orderhead.bookingId = 0;
                                entity.Orderheads.Add(orderhead);
                                entity.SaveChanges();
                                if (CustomerFenistraAllData.Count > 0)
                                {
                                // CustomerFenistra = CustomerFenistraAllData.Where(w => w.customer == Convert.ToInt32(line.Custom1)).ToList();
                                if (!string.IsNullOrEmpty(line.Custom1))
                                {
                                    //Christian - Only pick Article 3101 and Article 3001 , 3102 , 3002 
                                    var fileResult = CustomerFenistraAllData.Where(w => w.Article == "3101" || w.Article == "3001" || w.Article == "3102" || w.Article == "3002");
                                    var CustomerFenistraGroup1 = fileResult.GroupBy(g => new { g.customer, g.Article }).Select(group => new { Article = group.Key.Article, Price = group.Sum(a => a.Price), customer = group.Key.customer }).Where(w => w.customer == Convert.ToInt32(line.Custom1)).ToList();
                                    if (CustomerFenistraGroup1.Count > 0)
                                    {
                                        foreach (var item in CustomerFenistraGroup1)
                                        {
                                            decimal mAmount= FenistrafilePriceChangeByArticle(item.Article,item.Price);
                                            string mText= FenistrafileTextChangeByArticle(item.Article, FakturajournalText);

                                            orderline = new Orderline();
                                            orderline.Qty = 1;
                                            orderline.Status = false;
                                            orderline.Sum = mAmount; // item.Price;
                                            orderline.Tekst = mText; // "Kontorleie" + new string(' ', 1) + FakturajournalText;

                                            orderline.OrderHeadId = orderhead.OrderNo;
                                            orderline.BookingID = 0;
                                            orderline.IsKitchen = false;
                                            orderline.IsVat = false;
                                            orderline.CostPrice = 0;
                                            orderline.CostTotal = 0;

                                            orderline.NetAmount = mAmount;// item.Price;

                                            orderline.Amount = mAmount; // item.Price;

                                            orderline.Price = mAmount; // item.Price;
                                            orderline.IsMainService = false;

                                            orderline.ArticleId = item.Article;
                                            orderline.FoodID = 0;
                                            orderline.SortOrder = inc;
                                            olist.Add(orderline);
                                            inc++;
                                        }
                                    }
                                }
                               
                                }
                               
                                if (CustomerAgreementlines.Count > 0)
                                {
                                    foreach (var item in CustomerAgreementlines)
                                    {
                                        orderline = new Orderline();
                                        orderline.Qty = 1;
                                        orderline.Status = false;
                                        orderline.Sum = item.Price;
                                        orderline.Tekst = item.Text;

                                        orderline.OrderHeadId = orderhead.OrderNo;
                                        orderline.BookingID = 0;
                                        orderline.IsKitchen = false;
                                        orderline.IsVat = false;
                                        orderline.CostPrice = 0;
                                        orderline.CostTotal = 0;

                                        orderline.NetAmount = item.Price;

                                        orderline.Amount = item.Price;

                                        orderline.Price = item.Price;
                                        orderline.IsMainService = false;

                                        orderline.ArticleId = item.Article;
                                        orderline.FoodID = 0;
                                        orderline.SortOrder = inc;
                                        olist.Add(orderline);
                                        inc++;
                                    }
                                }
                                
                                if (CustomerBooking.Count > 0)
                                {
                                    foreach (var item in CustomerBooking)
                                    {
                                        if(!string.IsNullOrEmpty(item.SplitLine1Article))
                                        {
                                            if (!string.IsNullOrEmpty(item.SplitLine1Article))
                                            {

                                                orderline = new Orderline();
                                                orderline.Qty = 1;
                                                orderline.Status = false;
                                                orderline.Price = item.SplitLine1Price;
                                                orderline.Sum = item.SplitLine1Price;
                                                orderline.Tekst = "";
                                                // orderline.ServiceName = item.ServiceName;
                                                orderline.OrderHeadId = orderhead.OrderNo;
                                                orderline.BookingID = 0;
                                                 orderline.IsKitchen = false;
                                                orderline.IsVat = false;
                                                orderline.CostPrice = 0;
                                                orderline.CostTotal = 0;
                                                orderline.UnitText = null;
                                                orderline.NetAmount = item.SplitLine1Price;
                                                // orderline.VATPercent = item.VATPercent;
                                                // orderline.Time = item.Time;
                                                orderline.Amount = item.SplitLine1Price;

                                                // orderline.Price = item.Price;
                                                orderline.IsMainService = false;

                                                orderline.ArticleId = item.SplitLine1Article;
                                                orderline.FoodID = 0;
                                                orderline.SortOrder = inc;
                                                olist.Add(orderline);
                                                inc++;
                                            }
                                            if (!string.IsNullOrEmpty(item.SplitLine2Article))
                                            {
                                                orderline = new Orderline();
                                                orderline.Qty = 1;
                                                orderline.Status = false;
                                                orderline.Price = item.SplitLine2Price;
                                                orderline.Sum = item.SplitLine2Price;
                                                orderline.Tekst = "";
                                                // orderline.ServiceName = item.ServiceName;
                                                orderline.OrderHeadId = orderhead.OrderNo;
                                                orderline.BookingID = 0;
                                                 orderline.IsKitchen = false;
                                                orderline.IsVat = false;
                                                orderline.CostPrice = 0;
                                                orderline.CostTotal = 0;
                                                orderline.UnitText = null;
                                                orderline.NetAmount = item.SplitLine2Price;
                                                // orderline.VATPercent = item.VATPercent;
                                                // orderline.Time = item.Time;
                                                orderline.Amount = item.SplitLine2Price;

                                                // orderline.Price = item.Price;
                                                orderline.IsMainService = false;

                                                orderline.ArticleId = item.SplitLine2Article;
                                                orderline.FoodID = 0;
                                                orderline.SortOrder = inc;
                                                olist.Add(orderline);
                                                inc++;
                                            }
                                        }
                                          
                                        else
                                        {
                                            orderline = new Orderline();
                                            orderline.Qty = 1;
                                            orderline.Status = false;
                                            orderline.Price = item.Amount;
                                            orderline.Sum = item.Amount;
                                            orderline.Tekst = "";
                                            // orderline.ServiceName = item.ServiceName;
                                            orderline.OrderHeadId = orderhead.OrderNo;
                                            orderline.BookingID = 0;
                                             orderline.IsKitchen =false;
                                            orderline.IsVat = false;
                                            orderline.CostPrice = 0;
                                            orderline.CostTotal = 0;
                                            orderline.UnitText =null;
                                            orderline.NetAmount = item.Amount;
                                           // orderline.VATPercent = item.VATPercent;
                                           // orderline.Time = item.Time;
                                            orderline.Amount = item.Amount;

                                           // orderline.Price = item.Price;
                                            orderline.IsMainService = false;

                                            orderline.ArticleId = item.Article;
                                            orderline.FoodID = 0;
                                            orderline.SortOrder = inc;
                                            olist.Add(orderline);
                                            inc++;
                                        }
                                       
                                    }
                                    customerwisebooking = new CustomerBookingOrderHead();
                                    customerwisebooking.bookingIDs = bookingIDs;
                                    customerwisebooking.OrderheadId = orderhead.OrderNo;
                                    customerwisebooking.CustomerNo = line.OrganizationUnitID;
                                    customerwisebookingList.Add(customerwisebooking);
                                    UpdateBookingServiceList(bookingIDs, line.OrganizationUnitID, orderhead.OrderNo);

                                }

                                if (CustomerAgreement.Count > 0)
                                {
                                    foreach (var item in CustomerAgreement)
                                    {
                                        orderline = new Orderline();
                                        orderline.Qty = 0;
                                        orderline.Status = false;
                                        orderline.Sum = item.Price;
                                        orderline.Tekst = item.Text;

                                        orderline.OrderHeadId = orderhead.OrderNo;
                                        orderline.BookingID = 0;
                                        orderline.IsKitchen = false;
                                        orderline.IsVat = false;
                                        orderline.CostPrice = 0;
                                        orderline.CostTotal = 0;

                                        orderline.NetAmount = item.Price;

                                        orderline.Amount = item.Price;

                                        orderline.Price = item.Price;
                                        orderline.IsMainService = false;

                                        orderline.ArticleId = item.Article;
                                        orderline.FoodID = 0;
                                        orderline.SortOrder = inc;
                                        olist.Add(orderline);
                                        inc++;
                                        // Agreements 
                                        //var query =
                                        // from ur in entity.Agreements
                                        // where ur.AgreementId == item.AgreementId
                                        // select ur;

                                        //// Execute the query, and change the column values
                                        //// you want to change
                                        //foreach (var urs in query)
                                        //{
                                        //    urs.InvoicedToDate = Convert.ToDateTime(AgreementToDate);
                                        //    // Insert any additional changes to column values.
                                        //}
                                        //entity.SaveChanges();
                                        // Agreements
                                    }
                                }
                                if (CustomerBookingLines.Count > 0)
                                {
                                    foreach (var item in CustomerBookingLines)
                                    {
                                        orderline = new Orderline();
                                        orderline.Qty = 0;
                                        orderline.Status = false;
                                        orderline.Price = item.Amount;
                                        orderline.Sum = item.Amount;
                                        orderline.Tekst = item.Text;
                                        // orderline.ServiceName = item.ServiceName;
                                        orderline.OrderHeadId = orderhead.OrderNo;
                                        orderline.BookingID = 0;
                                        // orderline.IsKitchen = item.IsKitchen;
                                        orderline.IsVat = false;
                                        orderline.CostPrice = 0;
                                        orderline.CostTotal = 0;
                                        orderline.UnitText = null;
                                        orderline.NetAmount = item.Amount;
                                        // orderline.VATPercent = item.VATPercent;
                                        // orderline.Time = item.Time;
                                        orderline.Amount = item.Amount;

                                        // orderline.Price = item.Price;
                                        orderline.IsMainService = false;

                                        orderline.ArticleId = item.Article;
                                        orderline.FoodID = 0;
                                        orderline.SortOrder = inc;
                                        olist.Add(orderline);
                                        inc++;
                                    }

                                }
                                //if (CustomerBooking.Count > 0)
                                //{
                                //    List<int> TagIds = bookingIDs.Split(',').Select(int.Parse).ToList();

                                //    var bookinglist = entity.BookingServiceLists
                                //                     .Join(entity.BookingDetails, bd => bd.BookingID, bl => bl.BookingID, (BD, BL) => new { bd = BD, bl = BL })
                                //                     .Where(e => TagIds.Contains(e.bd.BookingID) && e.bd.OrderHeadId == 0 && e.bd.Status == false && e.bl.Customer == line.OrganizationUnitID)
                                //                     .Select(s => s.bd).ToList();


                                //    if (bookinglist.Count > 0)
                                //    {
                                //        foreach (var ord in bookinglist)
                                //        {
                                //            ord.OrderHeadId = orderhead.OrderNo;
                                //        }
                                //        entity.SaveChanges();
                                //    }
                                //}
                                if (olist.Count > 0)
                                {
                                    entity.Orderlines.AddRange(olist);
                                    entity.SaveChanges();
                                }
                               
                           // }
                        }

                           
                        dbContextTransaction.Commit();
                        CustomerAgreementUpdateInvoicedToDate(AgreementToDate);
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        foreach (var item in customerwisebookingList)
                        {
                            UpdateBookingServiceList(item.bookingIDs, item.CustomerNo, 0);
                        }
                        return 0;

                    }
                }
               // return 1;
            }
        }

        [HttpPost]
        public IHttpActionResult UploadFilesValidate(string bookingID)
        {
            List<CustomerListForValid> CustomerDigimaker = CustomerListForValidFenistraFile(bookingID);
            var httpRequest = HttpContext.Current.Request;

            StreamReader csvreader = new StreamReader(httpRequest.Files["file"].InputStream);

            List<CustomerFenistraFile> CustomerList = new List<CustomerFenistraFile>();
            CustomerFenistraFile Customer = null;

            while (!csvreader.EndOfStream)
            {
                Customer = new CustomerFenistraFile();
                var line = csvreader.ReadLine();
                if (line != null && line != String.Empty)
                {
                    var fields = line.Split(',');
                    Customer.customer = int.Parse(fields[20].Trim());
                    Customer.Article = fields[9].Trim();
                    Customer.Price = decimal.Parse(fields[14].Trim(), CultureInfo.InvariantCulture);
                    CustomerList.Add(Customer);
                }
            }
            string result = string.Empty;
            string data = string.Empty;
            int flag = 0;
            foreach (var line in CustomerList)
            {

                var count = CustomerDigimaker.Where(w => w.Custom1 == line.customer.ToString()).Count();
                if (count == 0)
                {
                    data+= line.customer.ToString() + " ,";
                    flag = 1;
                }

            }
            if (flag == 1)
            {
                result = "Finner ikke kunde(r) i Digimaker ";
                
            }
            else
            {
                result = "Customer validate successfully";
            }
            return Json(new { result = result, data = data });
        }

        public List<CustomerFenistraFile> CustomerFenistraFile()
        {
            var httpRequest = HttpContext.Current.Request;

            StreamReader csvreader = new StreamReader(httpRequest.Files["file"].InputStream);
            List<CustomerFenistraFile> CustomerList = new List<CustomerFenistraFile>();
            CustomerFenistraFile Customer = null;

            while (!csvreader.EndOfStream)
            {
                Customer = new CustomerFenistraFile();
                var line = csvreader.ReadLine();
                if (line != null && line != String.Empty)
                {
                    var fields = line.Split(',');
                    Customer.customer = int.Parse(fields[20].Trim());
                    Customer.Article = (int.Parse(fields[9].Trim())).ToString();
                    Customer.Price = decimal.Parse(fields[14].Trim(), CultureInfo.InvariantCulture);
                    CustomerList.Add(Customer);
                }
            }
            return CustomerList;
        }
        private void CustomerAgreementUpdateInvoicedToDate(string ToDate)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerAgreementUpdateInvoicedToDate", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter


                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");

                // open connection, execute command, close connection
                conn.Open();
                cmd.ExecuteScalar();
                conn.Close();
            }
        }

        private void UpdateBookingServiceList(string BookingIDs,int Customer,int orderHeadID)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerUpdateBookingServiceList", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@BookingIDs", SqlDbType.VarChar).Value = BookingIDs;
                cmd.Parameters.Add("@Customer", SqlDbType.Int).Value = Customer;
                cmd.Parameters.Add("@orderHeadID", SqlDbType.Int).Value = orderHeadID;

                // open connection, execute command, close connection
                conn.Open();
                cmd.ExecuteScalar();
                conn.Close();
            }
        }
        private List<CustomerListForValid> CustomerListForValidFenistraFile(string BookingIDs)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.SelCustomerListForValidFenistraFile", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@BookingIDs", SqlDbType.VarChar).Value = BookingIDs;
                // open connection, execute command, close connection
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerListForValid> CustomerList = new List<CustomerListForValid>();
                CustomerListForValid Customer = null;
                while (reader.Read())
                {
                    Customer = new CustomerListForValid();
                    Customer.OrganizationUnitID = int.Parse(reader["OrganizationUnitID"].ToString());
                    Customer.Custom1 = reader["Custom1"].ToString();
                    Customer.OrgNumber = reader["OrgNumber"].ToString();
                    CustomerList.Add(Customer);
                }
                conn.Close();
                return CustomerList;
            }
        }

        private List<CustomerAgreement> CustomerWiseAgreement(string FromDate, string ToDate, string InvoicedToDate, int OrganizationUnitID)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerAgreement", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Convert.ToDateTime(FromDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@InvoicedToDate", SqlDbType.VarChar).Value = Convert.ToDateTime(InvoicedToDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@OrganizationUnitID", SqlDbType.Int).Value = OrganizationUnitID;
                // open connection, execute command, close connection
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerAgreement> CustomerList = new List<CustomerAgreement>();
                CustomerAgreement Customer = null;

                while (reader.Read())
                {
                    Customer = new CustomerAgreement();
                    Customer.Article = reader["Article"].ToString();
                    Customer.Text = reader["Text"].ToString();
                    if (string.IsNullOrEmpty(reader["Price"].ToString()))
                    {
                        Customer.Price = 0;
                    }
                    else
                    {   //savandecimal
                        Customer.Price = (decimal)(reader["Price"]);
                        // Customer.Price = decimal.Parse(reader["Price"].ToString(), CultureInfo.InvariantCulture);
                    }
                    CustomerList.Add(Customer);
                }
                conn.Close();
                return CustomerList;
            }
        }


        private List<CustomerAgreement> CustomerAgreementDetail(string FromDate, string ToDate, string InvoicedToDate, int OrganizationUnitID, string Text)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerAgreementDetail", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                var dd = Convert.ToDateTime(FromDate);
                var aa = Convert.ToDateTime(FromDate).ToString("yyyy-mm-dd");

                cmd.Parameters.Add("@FromDate", SqlDbType.VarChar).Value = Convert.ToDateTime(FromDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@ToDate", SqlDbType.VarChar).Value = Convert.ToDateTime(ToDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@InvoicedToDate", SqlDbType.VarChar).Value = Convert.ToDateTime(InvoicedToDate).ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@Text", SqlDbType.VarChar).Value = Text;
                cmd.Parameters.Add("@OrganizationUnitID", SqlDbType.Int).Value = OrganizationUnitID;

                // open connection, execute command, close connection
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerAgreement> CustomerList = new List<CustomerAgreement>();
                CustomerAgreement Customer = null;

                while (reader.Read())
                {
                    Customer = new CustomerAgreement();
                    Customer.AgreementId = int.Parse(reader["AgreementId"].ToString());
                    Customer.Text = reader["Text"].ToString();
                    CustomerList.Add(Customer);
                }
                conn.Close();
                return CustomerList;
            }
        }

        private List<CustomerBookingOrder> CustomerBookingOrder(string bookingIDs, int CustomerNo,decimal SplittPercent,int IsSplitting)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerBookingOrder", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@BookingIDs", SqlDbType.VarChar).Value = bookingIDs;
                cmd.Parameters.Add("@Customer", SqlDbType.Int).Value = CustomerNo;
                cmd.Parameters.Add("@SplittPercent", SqlDbType.Decimal).Value = SplittPercent;
                cmd.Parameters.Add("@IsSplitting", SqlDbType.Int).Value = IsSplitting;

                // open connection, execute command, close connection
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerBookingOrder> CustomerList = new List<CustomerBookingOrder>();
                CustomerBookingOrder Customer = null;

                while (reader.Read())
                {
                    Customer = new CustomerBookingOrder();
                    Customer.Article = reader["ArticleId"].ToString();
                   
                    Customer.Amount = (decimal)(reader["Amount"]);
                    if (IsSplitting == 1) { 
                    Customer.SplitLine1Article = reader["SplitLine1Article"].ToString();
                    Customer.SplitLine2Article = reader["SplitLine2Article"].ToString();
                    Customer.SplitLine1Price = (decimal)(reader["SplitLine1Price"]);
                    Customer.SplitLine2Price = (decimal)(reader["SplitLine2Price"]);
                    }
                    CustomerList.Add(Customer);
                }
                conn.Close();
                return CustomerList;
            }
        }

        private List<CustomerBookingOrder> CustomerBookingOrderDetail(string bookingIDs, int CustomerNo)
        {
            // set up connection and command
            using (SqlConnection conn = new SqlConnection(Digimaker.Config.ConnectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.selCustomerBookingOrderDetail", conn))
            {
                // define command to be stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // add parameter
                cmd.Parameters.Add("@BookingIDs", SqlDbType.VarChar).Value = bookingIDs;
                cmd.Parameters.Add("@Customer", SqlDbType.Int).Value = CustomerNo;

                // open connection, execute command, close connection
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerBookingOrder> CustomerList = new List<CustomerBookingOrder>();
                CustomerBookingOrder Customer = null;

                while (reader.Read())
                {
                    Customer = new CustomerBookingOrder();

                    Customer.Text = reader["Text"].ToString();

                    CustomerList.Add(Customer);
                }
                conn.Close();
                return CustomerList;
            }
        }
    }
}
