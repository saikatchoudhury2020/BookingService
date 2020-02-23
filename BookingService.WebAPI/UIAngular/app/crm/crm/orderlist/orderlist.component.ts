import { Component, OnInit } from "@angular/core";
import { CrmService } from "../crm.service";
import { ActivatedRoute, Router } from "@angular/router";

import * as moment from "moment";
import { AppService } from "src/app/app.service";
import { AppSettings } from "../../../appConfig";
import { Booking } from "src/app/model/booking";
import { SearchFilterPipe } from "src/app/crm/pipe/search-filter.pipe";
import * as _ from "lodash";
import { Observable, Subscription, timer } from "rxjs";
import { Order } from "src/app/model/order";
import { ExcelService } from "src/app/services/excel.service";

@Component({
  selector: "app-orderlist",
  templateUrl: "./orderlist.component.html",
  styleUrls: ["./orderlist.component.css"]
})
export class OrderlistComponent implements OnInit {
  selectedOrder: any;
  selectedOrderInfo = [];
  isSelectedAll: boolean;
  objectList: Array<string> = [];
  userGroupName: string;
  orderListData: any;
  SendToVismaOrderListData: any;
  bookingopen: any;
  serviceopen: any;
  orderOpen: any;
  eventData: any;
  serviceData: any;
  orderData: any;
  deletedUserGroupId: string;
  display = "none";
  alert: string;
  imagePath: string = "";

  message1: string = "";
  message2: string = "";
  message3: string = "";
  message: string = "";
  loading = false;
  selectedTab1Data: any;
  BookingsListDisplay: any;
  OrdersListDisplay: any;
  ConfirmedListDisplay: any;
  TransferedListDisplay: any;
  displayMessage = "";
  selectedOrderTab1: any;
  alltab1data: any;
  selectedOrderInfoTab1 = [];
  isSelectedAllTab1: boolean;

  resourseData: any;
  currentUserID: any;
  properties: any;
  propertyServices: any;
  meetingRooms: any;
  subServiceData: any;
  organizationListData: any;
  notvalidateCustomer: any;
  selectedOrderTab2: any;
  selectedOrderInfoTab2 = [];
  isSelectedAllTab2: boolean;
  tab1data: any;
  tab2data: any;
  //for filter
  BuildingID: any;
  ServiceType: any;
  ServiceID: any;
  Customer: any;
  UserID: any;
  AllTabData: any;
  displayWarning = '';
  isIncludeAgreement = false;
  ToDate: any;
  invoFile: File;
  agreementText: any;
  isSplitting = false;
  splitingText: number = 50;
  ToFileDate: any;
  isvalidate = false;
  isExecute = true;
  isvalidExecute = false;
  displayvalidatewindow = "none";
  validateMsg = '';
  sucessvalidateMsg = '';
  selectedErpClient = 'Braathen Eiendom Flyt Bjørvika TEST';
  fileText: string;
  //for filter by date
  filterTime: any = {
    fromDate: moment().format("DD.MM.YYYY"),
    toDate: moment().format("DD.MM.YYYY")
  };

  public showloader: boolean = false;
  private subscription: Subscription;
  private timer: Observable<any>;
  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _appService: AppService,
    private _crmService: CrmService,
    private _excelService: ExcelService
  ) { }
  ngOnInit() {

    const dateObj = new Date();
    this.ToDate = this.lastday(dateObj.getUTCFullYear(), dateObj.getUTCMonth()) + '.' + dateObj.getUTCMonth() + '.' + dateObj.getUTCFullYear();
    this.ToFileDate = this.lastday(dateObj.getUTCFullYear(), dateObj.getUTCMonth() + 2) + '.' + (dateObj.getUTCMonth() + 2) + '.' + dateObj.getUTCFullYear();
    this.selectedErpClient = 'Braathen Eiendom Flyt Bjørvika TEST';
    this.BuildingID = 0;
    this.ServiceType = 0;
    this.ServiceID = 0;
    this.Customer = 0;
    this.UserID = 0;
    this.filterTime.fromDate = moment(new Date())
      .subtract(30, "days")
      .format("DD.MM.YYYY");
    this.filterTime.toDate = moment(new Date()).format("DD.MM.YYYY");
    this.onGetOrderList();

    this.openbooking(0, false);
    this.OpenOrderline(0, 0, false);
  }

  openbooking(value: any, viewMode: boolean) {
    if (value != 0) {
      let modeValue = "";
      if (viewMode == true) {
        modeValue = "BookingViewMode";
      } else {
        modeValue = "edit";
      }
      this._appService.getBookingDetailByBookingId(value).subscribe(event => {
        console.log(event);
        let parameters: Booking = {
          bookingId: event.id,
          FromDate: moment(event.start).format("DD.MM.YYYY"),
          ToDate: moment(event.end).format("DD.MM.YYYY"),
          FromTimer: moment(event.start).format("HH:mm"),
          ToTimer: moment(event.end).format("HH:mm"),
          MeetingRoomId: event.MeetingRoomId,
          PropertyServiceId: event.PropertyServiceId,
          PropertyId: event.buildingId,
          NoOfPeople: event.numOfPeople,
          IsFoodOrder: event.IsFoodOrder,
          UserID: event.UserID,
          CompanyPayer: event.CompanyPayer,
          nameOfbook: event.nameOfbook,
          BookOrderName: event.BookOrderName,
          FollowDate: event.FollowDate
            ? moment(event.FollowDate).format("DD.MM.YYYY")
            : "",
          SendMessageType: event.SendMessageType.toString(),
          Note: event.Note,
          InvoMessage: event.InvoMessage,
          IsConfirmed: event.IsConfirmed,
          foods: event.foods,
          mode: modeValue,
          IsInternal: event.IsInternal,
          IsMVA: event.IsMVA,
          OrderHeadId: 0
        };


        if (event.Status == 98) {
          this.serviceopen = true;
          if (this.serviceopen) {
            this.serviceData = parameters;
          }
        }
        else {
          this.bookingopen = true;

          if (this.bookingopen) {
            this.eventData = parameters;
          }
        }



      });
    }
  }
  OpenOrderline(value: any, orderId: any, viewMode: boolean) {
    if (orderId != 0) {
      let modeValue = "";
      if (viewMode == true) {
        modeValue = "OrdersViewMode";
      } else {
        modeValue = "OrdersMode";
      }
      //this._appService.GetOrderDetailByOrderId(orderId)
      this._appService.GetOrderDetailByOrderId(orderId).subscribe(event => {
        console.log(event);
        this.orderOpen = true;
        let parameters: Order = {
          OrderHeadId: orderId,
          orderlines: event.foods,
          mode: modeValue,
        };
        if (this.orderOpen) {
          this.orderData = parameters;
        }
      });
    }
  }
  lastday = (y: number, m: number): number => {
    return new Date(y, m, 0).getDate();
  }
  onGetOrderList() {
    this._crmService.GetBookingsListDisplay().subscribe(data => {
      this.BookingsListDisplay = data;
      this.GetFilterSelectedOrderData();
      this.GetFilterData(data);
    });
    this._crmService.GetOrdersListDisplay().subscribe(data => {
      this.OrdersListDisplay = data;
      this.GetFilterSelectedOrderData();
      this.GetFilterData(data);
    });
    this._crmService.GetConfirmedListDisplay().subscribe(data => {
      this.ConfirmedListDisplay = data;
      this.GetFilterSelectedOrderData();
      this.GetFilterData(data);
    });

    this._crmService.GetTransferedListDisplay().subscribe(data => {
      this.TransferedListDisplay = data;
      console.log(this.TransferedListDisplay);
      this.GetFilterData(data);
    });
    console.log("this is all data" + this.AllTabData);
  }

  sendAll = (): void => {
    this.objectList = [];
    for (var orderid in this.selectedOrder) {
      if (this.selectedOrder[orderid]) {
        this.objectList.push(orderid);
      }
    }
    this._crmService.SendOrder(this.objectList).subscribe(
      data => {
        if (data == "1") {
          this.message3 = "Orders are sent.";
          this.onGetOrderList();
        } else {
          this.message3 = "Orders are not sent.";
        }
      },
      err => {
        this.message3 = "Order Not Sent!!";
        console.log(err);
      }
    );
  };
  deleteService(data: any): void {
    if (data.errorType == 0) {
      this.serviceopen = false;
      this.message = data.data;
      this.setTimer();
      this.onGetOrderList();
      // this.getBookingList();
      // this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName);
    }
  }
  SavedService(data: any): void {

    if (data.errorType == 0) {
      this.serviceopen = false;
      this.message = "Service Updated successfully";
      this.setTimer();
      // this.getBookingList();
      //  this.getviewEventList(this.viewFromDate,this.viewToDate,this.viewName);
    }
  }
  deleteBooking(data: any): void {
    if (data.errorType == 0) {
      this.bookingopen = false;
      this.message = data.data;
      this.setTimer();
      // this.getBookingList();
      this.onGetOrderList();
    }
  }
  sendAllBookingsConfirm = (): void => {
    this.objectList = [];

    for (var orderid in this.selectedOrderTab1) {
      if (this.selectedOrderTab1[orderid]) {
        this.objectList.push(orderid);
      }
    }
    this._crmService.SendBookingsConfirm(this.objectList, this.selectedErpClient).subscribe(data => {
      this.loading = false;
      if (data == "1") {
        this.message1 = "Orders are sent.";
        this.onGetOrderList();
      } else {
        this.message1 = "Orders are sent.";
      }
      this.isExecute = true;
      this.display = 'none';
    });
  };
  clearFile = (): void => {
    this.invoFile = null;
    this.isExecute = true;
  }
  handleFileInput = (files: FileList): void => {
    this.sucessvalidateMsg = '';
    this.invoFile = files.item(0);
    this.isExecute = false;
  }
  fileValidation = (): void => {
    this.isvalidExecute = true;
    this.loading = true;
    if (this.invoFile) {
      this.objectList = [];

      for (var orderid in this.selectedOrderTab1) {
        if (this.selectedOrderTab1[orderid]) {
          this.objectList.push(orderid);
        }
      }
      const formData = new FormData();
      formData.append('file', this.invoFile, this.invoFile.name);
      this._crmService.ValidateFile(this.objectList, formData).subscribe(data => {
        this.loading = false;
        if (data.data) {
          this.notvalidateCustomer = data.data;
          this.validateMsg = data.result;
          this.displayvalidatewindow = "block";


        }
        else {
          this.sucessvalidateMsg = data.result;
          this.isExecute = true;
        }

        this.isvalidExecute = false;

      })
    }
  }
  closeValidateWindow = (): void => {
    this.displayvalidatewindow = "none";
  }
  SavedOrderLine(data: any): void {
    if (data.errorType == 0) {
      this.bookingopen = false;
      this.message = data.data;
      this.setTimer();
      this.onGetOrderList();
    }
  }
  SavedOrdersLine(data: any): void {
    if (data.errorType == 0) {
      this.orderOpen = false;
      this.message = data.data;
      this.setTimer();
      this.onGetOrderList();
    }
  }
  public setTimer() {
    // set showloader to true to show loading div on view
    this.showloader = true;
    this.timer = timer(3000);
    //savan this.timer = Observable.timer(3000); // 5000 millisecond means 5 seconds
    this.subscription = this.timer.subscribe(() => {
      // set showloader to false to hide loading div from view after 5 seconds
      this.showloader = false;
    });
  }
  sendAllOrderSendToConfirm = (): void => {
    this.objectList = [];
    for (var orderid in this.selectedOrderTab2) {
      if (this.selectedOrderTab2[orderid]) {
        this.objectList.push(orderid);
      }
    }
    this._crmService.OrderSendToConfirm(this.objectList).subscribe(data => {
      if (data == "1") {
        this.message2 = "Orders are sent.";
        this.onGetOrderList();
      } else {
        this.message2 = "Orders are sent.";
      }
    });
  };
  selectAll() {
    for (var i in this.selectedOrder) {
      this.selectedOrder[i] = this.isSelectedAll;
    }
  }
  selectAllTab1() {
    console.log(this.BookingsListDisplay);
    for (var i in this.selectedOrderTab1) {
      this.selectedOrderTab1[i] = this.isSelectedAllTab1;
    }
  }
  selectAllTab2() {
    for (var i in this.selectedOrderTab2) {
      this.selectedOrderTab2[i] = this.isSelectedAllTab2;
    }
  }
  GetFilterData(data: any) {
    this.AllTabData = _.union(this.AllTabData, data);
    this.properties = _.uniqBy(this.AllTabData, "BuildingID");
    this.currentUserID = _.uniqBy(this.AllTabData, "UserID");
    this.organizationListData = _.uniqBy(this.AllTabData, "Customer");
  }
  GetFilterSelectedOrderData() {
    //
    this.selectedTab1Data = null;
    let tab1 = new SearchFilterPipe().transform(
      this.BookingsListDisplay,
      this.BuildingID,
      this.ServiceType,
      this.ServiceID,
      this.Customer,
      this.UserID,
      this.filterTime.fromDate,
      this.filterTime.toDate
    );
    this.selectedTab1Data = tab1;
    var selectedOrderObjectTab1 = {};
    for (var index in tab1) {
      selectedOrderObjectTab1[tab1[index]["BookingID"]] = false;
    }
    this.selectedOrderTab1 = selectedOrderObjectTab1;
    //
    let tab2 = new SearchFilterPipe().transform(
      this.OrdersListDisplay,
      this.BuildingID,
      this.ServiceType,
      this.ServiceID,
      this.Customer,
      this.UserID,
      this.filterTime.fromDate,
      this.filterTime.toDate
    );
    var selectedOrderObjectTab2 = {};
    for (var index in tab2) {
      selectedOrderObjectTab2[tab2[index]["OrderID"]] = false;
    }

    this.selectedOrderTab2 = selectedOrderObjectTab2;
    //
    let tab3 = new SearchFilterPipe().transform(
      this.ConfirmedListDisplay,
      this.BuildingID,
      this.ServiceType,
      this.ServiceID,
      this.Customer,
      this.UserID,
      this.filterTime.fromDate,
      this.filterTime.toDate
    );
    var selectedOrderObject = {};
    for (var index in tab3) {
      selectedOrderObject[tab3[index]["OrderID"]] = false;
    }
    this.selectedOrder = selectedOrderObject;
    //
  }
  onChanges(): void {
    this.GetFilterSelectedOrderData();
    this._appService.getPropertyServices(this.BuildingID).subscribe(data => {
      this.propertyServices = data;
    });
  }

  onServiceChanges(): void {
    this._appService.getMeetingRoomList(this.ServiceType).subscribe(data => {
      this.meetingRooms = data;
      this.subServiceData.length = 0;
      for (var i = 0; i < data.length; i++) {
        if (this.subServiceData.indexOf(data[i].meta_keywords) === -1) {
          this.subServiceData.push(data[i].meta_keywords);
        }
      }
    });
    this.GetFilterSelectedOrderData();
  }
  onServiceIDChanges(): void {
    this.GetFilterSelectedOrderData();
  }
  onCustomerChanges(): void {
    this.GetFilterSelectedOrderData();
  }
  onUserChanges(): void {
    this.GetFilterSelectedOrderData();
  }

  changeDate(newval: any): void {
    if (newval != 0) {
      this.filterTime.fromDate = moment(newval, "DD.MM.YYYY");
      this.GetFilterSelectedOrderData();
    }
  }
  changeToDate(newval: any): void {
    if (newval != 0) {
      this.filterTime.toDate = moment(newval, "DD.MM.YYYY");
      this.GetFilterSelectedOrderData();
    }
  }
  close() {
    this.display = 'none';
  }
  openConfirmwindow() {
    this.display = 'block';
  }
  CombineOrder = (): void => {
    let issplit = 0;
    let isagree = 0;
    const formData = new FormData();
    this.objectList = [];

    for (var orderid in this.selectedOrderTab1) {
      if (this.selectedOrderTab1[orderid]) {
        this.objectList.push(orderid);
      }
    }
    if (!this.isIncludeAgreement) {
      this.agreementText = '';
      this.ToDate = '';
      isagree = 0;

    }
    else {
      isagree = 1;
    }
    if (!this.invoFile) {
      this.fileText = '';
      this.ToFileDate = '';
    }
    if (!this.isSplitting) {
      this.splitingText = 0;
      issplit = 0;

    }
    else {
      issplit = 1;
    }

    if (this.invoFile) {

      formData.append('file', this.invoFile, this.invoFile.name);

    }

    this._crmService.SendCombineOrder(formData, this.objectList, isagree, issplit, this.agreementText, this.ToDate, this.fileText, this.ToFileDate, this.splitingText, this.selectedErpClient).subscribe(data => {
      this.loading = false;
      if (data == "1") {
        this.display = 'none';
        this.message1 = "Orders are sent.";
        this.onGetOrderList();
      } else {
        this.display = 'none';
        this.message1 = "Orders not sent.";
      }
      this.isExecute = true;
      this.isSplitting = false;
      this.isIncludeAgreement = false;
      this.invoFile = null;
      this.agreementText = '';
      this.fileText = '';
      this.sucessvalidateMsg = '';
    });
  }
  ExecuteOrder = (): void => {
    this.isExecute = false;
    this.loading = true;
    if (this.isIncludeAgreement || this.isSplitting || this.invoFile) {
      this.CombineOrder();
    }
    else {
      this.sendAllBookingsConfirm();
    }

  }
  exportAsXLSX = (): void => {
    var a = this.BookingsListDisplay;
    const data: any = [{
      eid: 'e101',
      ename: 'ravi',
      esal: 1000
    },
    {
      eid: 'e102',
      ename: 'ram',
      esal: 2000
    },
    {
      eid: 'e103',
      ename: 'rajesh',
      esal: 3000
    }];
    this._excelService.exportAsExcelFile(this.selectedTab1Data, 'sample');
  }
}
