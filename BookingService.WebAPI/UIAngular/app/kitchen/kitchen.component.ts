import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { KitchenService } from './kitchen.service';
import * as moment from 'moment';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { AppService } from '../app.service';
import { Booking } from '../model/booking';
import { Observable, Subscription, timer } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './kitchen.component.html',
  styleUrls: ['./kitchen.component.css']
})
export class KitchenComponent implements OnInit {

  orderListData: any;
  KitchenNotificationData: Array<any> = [];
  NotificationTotal: number = 0;
  isMessageListShown: boolean = false;

  FromDate: string;
  kitchenopen: any;
  eventData: any;
  today: string = "";
  color = 'red';
  isDisable = false;
  displayMessage = '';
  buildingName: string = '';
  buildingId: number = 0;
  public showloader: boolean = false;
  private subscription: Subscription;
  private timer: Observable<any>;

  error: string = '';

  constructor(private _Service: KitchenService, private _appService: AppService) {

    var url = new URL(window.location.href.replace('#', ''));
    var buildingId = url.searchParams.get("building"); //todo: replace this with the router parameter way. 1028
    //var buildingId = '1028';
    if (buildingId) {
      this.buildingId = parseInt(buildingId);
    }
    else {
      //	this.error = 'Invalid parameters.';
    }
  }

  ngOnInit(): void {

    this.today = moment(new Date()).format("DD.MM.YYYY");
    this.FromDate = this.today;
    let date = moment(new Date()).format("YYYY-MM-DD");

    if (this.buildingId != 0) {
      this._Service.GetBuilding(this.buildingId).subscribe(data => {
        this.buildingName = data[0].MenuName;
      });
    }

    this._Service.GetOrderList(date, this.buildingId).subscribe(data =>

      this.orderListData = data
    );

    this._Service.GetKitchenNotification(this.buildingId).subscribe(data => {

      this.KitchenNotificationData = data;
      var count = 0;
      data.forEach(function (value) {
        count += value.NotDeliveredCount + value.NotClearUpCount;

      });
      this.NotificationTotal = count;

    });

    this.onClickUpdateDelivered(0);
    this.onClickUpdateClean(0);
    this.changeDate(0);
    this.OpenOrderline(0);
  }
  onClickUpdateDelivered(value: any): void {
    if (value != 0) {
      this._Service.UpdateDelivered(value).subscribe(data => {
        let currentDate = this.FromDate.toString();
        let date = moment(currentDate, 'DD.MM.YYYY').format("YYYY-MM-DD");
        // let date=moment(this.FromDate).format("YYYY-MM-DD");
        this._Service.GetOrderList(date, this.buildingId).subscribe(data =>

          this.orderListData = data
        );
      })
    }

    this._Service.GetKitchenNotification(this.buildingId).subscribe(data => {
      this.KitchenNotificationData = data;
      var count = 0;
      data.forEach(function (value) {
        count += value.NotDeliveredCount + value.NotClearUpCount;

      });
      this.NotificationTotal = count;

    });
  }

  OpenOrderline(value: any) {
    if (value != 0) {
      this._appService.getBookingDetailByBookingId(value).subscribe(event => {
        console.log(event);
        this.kitchenopen = true;
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
          FollowDate: event.FollowDate ? moment(event.FollowDate).format("DD.MM.YYYY") : '',
          SendMessageType: event.SendMessageType.toString(),
          Note: event.Note,
          InvoMessage: event.InvoMessage,
          IsConfirmed: event.IsConfirmed,
          foods: event.foods,
          IsInternal: event.IsInternal,
          IsMVA: event.IsMVA,
          mode: 'orderlineEdit',
          OrderHeadId: 0
        };
        if (this.kitchenopen) {

          this.eventData = parameters;

        }
      })
    }


  }
  onClickUpdateClean(value: any): void {
    if (value != 0) {
      this.isDisable = true;
      this._Service.UpdateClean(value).subscribe(data => {

        if (data.errorType === 0) {
          this.displayMessage = data.message;
          this.color = 'green';
          let currentDate = this.FromDate.toString();
          let date = moment(currentDate, 'DD.MM.YYYY').format("YYYY-MM-DD");
          this._Service.GetOrderList(date, this.buildingId).subscribe(data => {

            this.orderListData = data;
            this.getNotificationData();
          }


          );
          this.setTimer();
        }
        else {
          this.displayMessage = data.message;
          this.color = 'red';
          this.setTimer();
        }
        this.isDisable = false;


      })
    }
    this._Service.GetKitchenNotification(this.buildingId).subscribe(data => {
      this.KitchenNotificationData = data;
      var count = 0;
      data.forEach(function (value) {
        count += value.NotDeliveredCount + value.NotClearUpCount;

      });
      this.NotificationTotal = count;

    });
  }
  changeDate(newval: any): void {
    if (newval != 0) {
      let date = moment(newval, 'DD.MM.YYYY').format("YYYY-MM-DD");
      this._Service.GetOrderList(date, this.buildingId).subscribe(data =>

        this.orderListData = data
      );
    }
  }

  dateObject(date: string, format = 'DD.MM.YYYY'): any {
    return moment(date, format);
  }
  public setTimer() {

    // set showloader to true to show loading div on view
    this.showloader = true;
    this.timer = timer(6000);
    //savan this.timer = Observable.timer(3000); // 5000 millisecond means 5 seconds
    this.subscription = this.timer.subscribe(() => {
      // set showloader to false to hide loading div from view after 5 seconds
      this.showloader = false;
    });
  }
  SavedKitchen(data: any): void {

    if (data.errorType == 0) {
      this.kitchenopen = false;
      this.displayMessage = data.data;
      this.setTimer();
      let currentDate = this.FromDate.toString();
      let date = moment(currentDate, 'DD.MM.YYYY').format("YYYY-MM-DD");
      // let date=moment(this.FromDate).format("YYYY-MM-DD");
      this._Service.GetOrderList(date, this.buildingId).subscribe(data =>

        this.orderListData = data
      );

    }

  }

  getNotificationData = () => {
    this._Service.GetKitchenNotification(this.buildingId).subscribe(data => {

      this.KitchenNotificationData = data;
      var count = 0;
      data.forEach(function (value) {
        count += value.NotDeliveredCount + value.NotClearUpCount;

      });
      this.NotificationTotal = count;

    });
  }

}
