using BookingService.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models;

namespace WebAPI.DTO
{
    public class BookingViewModel
    {
      public List<Building> resources { get; set; }
      public List<BookedEvents> events { get; set; }
      public List<BookedEvents> FollowupEvents { get; set; }
      public IEnumerable<BookingService.WebAPI.DTO.Menu> Properties { get; set; }
      public IEnumerable<BookingService.WebAPI.DTO.Menu> PropertyServices { get; set; }
      public IEnumerable<Article> ServiceList { get; set; }
      public IEnumerable<AddOnService> AddonServiceList { get; set; }
      public IEnumerable<UserMaster> UserList { get; set; }
      public IEnumerable<CompanyMaster> CustomerList { get; set; }
        public IEnumerable<string> Timers { get; set; }
      public Dictionary<string, string> Preferences { get; set; }
      public int currentUserID { get; set; }
    }

    public class BookedEvents
    {
        public int id { get; set; }
        public int resourceId { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }
        public int buildingId { get; set; }
        public string buildingName { get; set; }
        public int? numOfPeople { get; set; }
        public int PropertyServiceId { get; set; }
        public int MeetingRoomId { get; set; }
        public string ServiceType { get; set; }
        public string Service { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public int orgId { get; set; }
        public string OrgName { get; set; }
        public string nameOfbook { get; set; }
        public bool? IsFoodOrder { get; set; }
        public int? Status { get; set; }
        public int SendMessageType { get; set; }
        public List<BookedService> foods { get; set; }
        public double? Quantity { get; set; }
        public double? UnitPrice { get; set; }
        public double? DiscountPercent { get; set; }
        public double? NetAmount { get; set; }
        public double? VATPercent { get; set; }
        public double? Amount { get; set; }
        public string UnitText { get; set; }
        public DateTime? FollowDate { get; set; }
        public string BookOrderName { get; set; }
        public string CompanyPayer { get; set; }
        public string Note { get; set; }
        public string InvoMessage { get; set; }
        public bool IsInternal { get; set; }
        public bool IsMVA { get; set; }

    }

    public class Building
    {
        public int id { get; set; }
        public string title { get; set; }
        public string building { get; set; }
        public Dictionary<string, string> properties { get; set; }
        public List<Children> children { get; set; }
    }
    public class Children
    {
        public int id { get; set; }
        public string title { get; set; }
        public string eventColor { get; set; }
        public List<SubChildren> children { get; set; }
        public string description { get; set; }
        public Dictionary<string, string> ExtendedProperties { get; set; }
    }

    public class SubChildren
    {
        public int id { get; set; }
        public string title { get; set; }
        public string eventColor { get; set; }
        public string description { get; set; }
        /*
            * Format: list of <property>: <value>
        */
        public Dictionary<string, string> ExtendedProperties { get; set; }

    }
    public class BookedService
    {
        public int FoodId { get; set; }
        public decimal Qty { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public decimal CostTotal { get; set; }
        public string ArticleId { get; set; }
        public string MainServiceId { get; set; }
        public string Tekst { get; set; }
        public string ServiceText { get; set; }
        public decimal Sum { get; set; }
        public int OrderListId { get; set; }
        public bool IsKitchenOrder { get; set; }
        public bool IsVatApply { get; set; }
        public int OrderHeadId { get; set; }
        public string Time { get; set; }

    }
    public class Food
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public int price { get; set; }
    }
    public class BookingUnitPriceDetail
    {
        public string Unit { get; set; }
        public string UnitText { get; set; }
        public int From { get; set; }
        public string  Article1 { get; set; }
        public string Article2 { get; set; }
        public int To { get; set; }
        public double Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double SecondaryPrice { get; set; }
        public double CostPrice1 { get; set; }
        public double CostPrice2 { get; set; }
        public double DiscountPercent { get; set; }
        public double NetAmount { get; set; }
        public double VATPercent { get; set; }
        public double Amount { get; set; }
        public bool IsMutiplePrice { get; set; }

    }

    public class AddOnService
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        
        public List<ServiceAdd> ServiceList { get; set; }
    }
    public class ServiceAdd
    {
        public int SId { get; set; }
        public string SName { get; set; }
        public double Price { get; set; }
        public double SecondaryPrice { get; set; }
        public double CostPrice1 { get; set; }
        public double CostPrice2 { get; set; }
        public string ArticleId { get; set; }
        public bool isKitchen { get; set; }
        public string ArticleId2 { get; set; }
        public int? MainId { get; set; }
    }
    public class KitchenNotification
    {
        public DateTime? Fromdate { get; set; }
        public int? NotDeliveredCount { get; set; }
        public int? NotClearUpCount { get; set; }
    }
    public class KitchenDisplay
    {
        public List<FoodNotCleanedList> FoodNotCleanedList { get; set; }
        public List<FoodNotDeliveredList> FoodNotDeliveredList { get; set; }
    }
    public class FoodNotCleanedList
    {
        public int OrderNo { get; set; }
        public DateTime? Todate { get; set; }
        public DateTime? Fromdate { get; set; }
        public string Text { get; set; }
        public int? NoOFPeople { get; set; }
    }
    public class FoodNotDeliveredList
    {
        public int OrderNo { get; set; }
        public DateTime? Todate { get; set; }
        public DateTime? Fromdate { get; set; }
        public string Text { get; set; }
        public int? NoOFPeople { get; set; }
        public List<FoodList> FoodList { get; set; }
    }
    public class FoodList
    {
        public decimal Quantity { get; set; }
        public string Text { get; set; }
        public string DeliverTime { get; set; }

    }

    public class OrderListDisplay
    {
        public int? VGOrderNo { get; set; }
        public int? OrderID { get; set; }
        public int? BookingID { get; set; }
        public string BuildingName { get; set; }
        public bool IsKitchen { get; set; }
        public bool? IsDelivere { get; set; }
        public bool? IsCleaned { get; set; }
        public string MeetingroomName { get; set; }
        public int? NoOfPerson { get; set; }
        public DateTime? FromDate { get; set; }
        public string Title { get; set; }
        public double? TotalOrderAmount { get; set; }
        public string VMOrderFailedErrorMessage { get; set; }

        public int? BuildingID { get; set; }
        public int? ServiceID { get; set; }
        public int? Customer { get; set; }
        public int? UserID { get; set; }
        public int? ServiceType { get; set; }

        public string User { get; set; }
        public string CustomerName { get; set; }
    }

    public class UserViewModel
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }

    public class CustomerViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Land { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public string Pincode { get; set; }

        public string Country { get; set; }

    }
}