using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingService.WebAPI.DTO
{
    public class CrmViewModel
    {
        public List<BuildingModel> bulidingList { get; set; }
    }

    public class BuildingModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int informationId { get; set; }
        public int informationTranslationId { get; set; }
        public int organizationId { get; set; }
        public int kitchenMenuId { get; set; }
        public List<ServiceGroup> serviceList { get; set; }
        public List<OrgUnit> organizationList { get; set; }
    }

    public class ServiceGroup
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool mobileApp { get; set; }
        public string serviceType { get; set; }
        public bool requestOnly { get; set; }
        public string timeSpan { get; set; }
        public int capacity { get; set; }
        public int cleantime { get; set; }

        public string additionalOption { get; set; }
        public int preparationTime { get; set; }
        public int ryddetid { get; set; }
        public int cancellation { get; set; }
        public string reciever { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string picturePath { get; set; }
        public List<Service> services { get; set; }
    }


    public class Service
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool mobileApp { get; set; }
        public string serviceType { get; set; }
        public bool requestOnly { get; set; }
        public string timeSpan { get; set; }
        public int capacity { get; set; }
        public int cleantime { get; set; }

        public string additionalOption { get; set; }
        public int preparationTime { get; set; }
        public int ryddetid { get; set; }
        public int cancellation { get; set; }
        public string reciever { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string picturePath { get; set; }
        public List<BannerModel> banners { get; set; }
        public List<UserBookedEvents> bookedServiceEvents { get; set; }
    }

    public class Menu
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuDesc { get; set; }
        public string PicturePath { get; set; }
        /*
         * Format: list of <property>: <value>
        */
        public Dictionary<string, string> ExtendedProperteis { get; set; }
        public List<ArticleModel> Children { get; set; }

    }

    public class BannerModel
    {
        public int BannerId { get; set; }
        public string BannerName { get; set; }
    }
    public class User
    {
        public int UserId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string DisplayName { get; set; }
        public string UserEmail { get; set; }
        public string Mobile { get; set; }
        public string PicturePath { get; set; }
        public string EmployeeId { get; set; }
        public string PositionTitle { get; set; }
        public int Status { get; set; }
        public string Department { get; set; }
        public string Customer { get; set; }
        public string Hide { get; set; }
        public List<UserBookedEvents> bookedEvents { get; set; }
        public List<UserGroupList> groupList { get; set; }

    }
    public class ArticleModel
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; }
        public string ArticleDesc { get; set; }
        public string ArticleBody { get; set; }
        public string PicturePath { get; set; }


    }

    public class OrgUnit
    {
        public int id { get; set; }
        public string name { get; set; }

        public bool Code { get; set; }
        public string OrganizationNumber { get; set; }
        public string PicturePath { get; set; }
        public List<OrganizationPerson> personList { get; set; }
        public List<OrgUnit> orglist { get; set; }
        public OrgUnit()
        {
            orglist = new List<OrgUnit>();
        }
    }

    public class OrganizationPerson
    {
        public int id { get; set; }
        public string name { get; set; }
        public string telephone { get; set; }
        public string UserEmail { get; set; }
        public string Department { get; set; }
        public string UserGroup { get; set; }
        public string OrganizationName { get; set; }
    }

    public class UserBookedEvents
    {
        public int id { get; set; }
        public int resourceId { get; set; }
        public string resourceName { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }
        public int buildingId { get; set; }
        public string buildingName { get; set; }
        public int? numOfPeople { get; set; }
        public int PropertyServiceId { get; set; }
        public string ServiceName { get; set; }
        public int MeetingRoomId { get; set; }
        public string MeetingRoomName { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string nameOfbook { get; set; }
        public bool? IsFoodOrder { get; set; }


    }
    public class UserGroupList
    {
        public int userGroupId { get; set; }
        public string name { get; set; }
        public bool assigned { get; set; }
        public List<UserGroupListUser> personList { get; set; }
    }
    public class UserGroupListUser
    {
        public int id { get; set; }
        public string name { get; set; }
        public string telephone { get; set; }
        public string UserEmail { get; set; }
        public string PositionTitle { get; set; }
        public int companyId { get; set; }
        public string companyName { get; set; }
    }

    public class UserAssignedGroupModel
    {
        public int userId { get; set; }
        public int[] userGroup { get; set; }
    }

    public class MessageLogResult
    {
        public int totalCount { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public List<MessageLog> list { get; set; }

        public class MessageLog
        {
            public int id { get; set; }
            public string type { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public string title { get; set; }
            public string body { get; set; }
            public string log { get; set; }
            public string status { get; set; }
            public DateTime created { get; set; }
        }
    }
    public class NoteDisplayList
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public int? UserId { get; set; }
        public string NoteText { get; set; }
        public DateTime? CreateDate { get; set; }
    }
    public class Agreements
    {
        public int AgreementId { get; set; }
        public int Customer { get; set; }
        public int Contact { get; set; }
        public string ContactName { get; set; }
        public int AgreementTypeId { get; set; }
        public string AgreementTypeName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Article { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
        public int NumberOfMonths { get; set; }
        public DateTime? InvoicedToDate { get; set; }
    }
    public class AgreementTypes
    {
        public int AgreementTypeId { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
        public int NumberOfMonths { get; set; }
    }

    public class CustomerListForValid
    {
        public int OrganizationUnitID { get; set; }
        public string Custom1 { get; set; }
        public string OrgNumber { get; set; }
    }

    public class CustomerAgreement
    {
        public int AgreementId { get; set; }
        public string Article { get; set; }
        public string Text { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerBookingOrder
    {
        public string Article { get; set; }
        public decimal Amount { get; set; }
        public string SplitLine1Article { get; set; }
        public string SplitLine2Article { get; set; }
        public decimal SplitLine1Price { get; set; }
        public decimal SplitLine2Price { get; set; }
        public string Text { get; set; }
    }
    
    public class CustomerFenistraFile
    {
        public int customer { get; set; }
        public string Article { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerBookingOrderHead
    {
        public string bookingIDs { get; set; }
        public int OrderheadId { get; set; }
        public int CustomerNo { get; set; }
    }
}