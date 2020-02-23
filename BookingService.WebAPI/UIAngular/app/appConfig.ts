import { BookingMode } from "./model/bookingMode";
import { OrderMode } from "./model/orderMode";
export class AppSettings {
  //public static API = "http://braathen.dev.digimaker.no";
  public static API = "http://localhost:8080";
  //public static API = "https://braatheneiendom.no";
  public static RootPropertiesId = 1023;

  public static OrgUnitId = 2;
  public static Auth = "Saikat:Pa$$w0rd";
  public static InitProperty = 1026;
  public static mvaPrice = 1.125;
  public static InitPropertyService = 1024;
  public static InitServiceType = 4121;
  public static InitService = 9295;
  public static FoodServiceId = 2003;
  public static InitMeetingRoom = 1170;
  public static FoodFormId = 1011;
  public static Success = "green";
  public static Error = "red";
  public static BookingPriceDetailFormId = 1019; //1019 for production, 1017
  public static serviceFormId = 1012; //prod 1012, dev: 1013
  public static InitTime = "08:30";
  public static EndTime = "08:30";
  public static RefreshTimeInterval = 10000;
  public static IpadRefreshTime = 60; //seconds

  public static AnonymousBookingUserID = 5;

  public static LoginPage = "/digimaker/login.aspx?ReturnUrl=";
  public static EditMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: true,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: true,
    orderlinetab: true,
    actionstab: true,
    TableIndex: true,
    ViewMode: false,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
  public static DefaultMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: true,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: true,
    orderlinetab: true,
    actionstab: true,
    TableIndex: true,
    ViewMode: false,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
  public static kitchenMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: false,
    BookingMainOrderline: false,
    TotalSum: false,
    bookingtab: false,
    orderlinetab: true,
    actionstab: false,
    TableIndex: false,
    ViewMode: false,
    PriceView: false,
    IsInternal: false,
    IsMVA: false
  };
  public static OrdersMode: OrderMode = {

    orderlines: true,
    DeleteBooking: false,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: false,
    orderlinetab: true,
    actionstab: false,
    TableIndex: true,
    ViewMode: false,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
  public static OrderMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: false,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: false,
    orderlinetab: true,
    actionstab: false,
    TableIndex: true,
    ViewMode: false,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };

  public static OrdersViewMode: OrderMode = {
    orderlines: true,
    DeleteBooking: false,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: false,
    orderlinetab: true,
    actionstab: false,
    TableIndex: true,
    ViewMode: true,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
  public static OrderViewMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: false,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: false,
    orderlinetab: true,
    actionstab: false,
    TableIndex: true,
    ViewMode: true,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
  public static BookingViewMode: BookingMode = {
    FromDate: true,
    ToDate: true,
    FromTimer: true,
    ToTimer: true,
    MeetingRoomId: true,
    PropertyServiceId: true,
    PropertyId: true,
    NoOfPeople: true,
    UserID: true,
    CompanyPayer: true,
    nameOfbook: true,
    BookOrderName: true,
    SendMessageType: true,
    Note: true,
    InvoMessage: true,
    FollowDate: true,
    foods: true,
    DeleteBooking: false,
    BookingMainOrderline: true,
    TotalSum: true,
    bookingtab: true,
    orderlinetab: true,
    actionstab: true,
    TableIndex: true,
    ViewMode: true,
    PriceView: true,
    IsInternal: true,
    IsMVA: true
  };
}
