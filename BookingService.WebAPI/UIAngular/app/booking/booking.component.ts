import { Component, OnInit, ViewChild } from '@angular/core';
import { AppSettings } from '../appConfig';
import { Property } from '../model/Property';
import { PropertyService } from '../model/PropertyService';
import { MeetingRoom } from '../model/MeetingRoom';
import { Services } from '../model/Services';
import { Booking } from '../model/booking';
import { AppService } from '../app.service';
import { confirmAlertService } from '../Shared/confirmationAlert/service/confirmAlert.service';
import { Subscription, Observable, from } from 'rxjs';

import { timer } from 'rxjs';
import * as moment from 'moment';

import * as _ from 'lodash';

import { BookingDialogService } from '../Shared/booking/booking-dialog.service';
import { element } from 'protractor';
// import * as $ from 'jquery';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-root',
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.css']
})
export class BookingComponent implements OnInit {
  calendarOptions: any;
  Object = Object;

  loaded = false;

  displayEvent: any;

  // @ViewChild(CalendarComponent) ucCalendar: CalendarComponent;

  filterbarDisplay = 'block';

  bookingopen: any;
  serviceOpen: any;
  eventData: any;
  properties: Property[];
  userList: Array<any> = [];
  companyList: Array<any> = [];
  userOrderList: Array<any> = [];
  propertyServices: PropertyService[];
  meetingRooms: MeetingRoom[];

  date: Date = new Date();
  isEventConfirmed: boolean = false;

  booingType: number;
  theme = 'theme-admin';
  private data: any;

  resourseData: any;




  //default options
  defaultOptions: any = { businessHoursStart: '08:00', businessHoursEnd: '18:00' };

  //parameters
  paramServiceId: number = 0;
  paramBuildingId: number = 0;

  //for filterbar in template
  servicetypeList: any = {};
  timerList: Array<any> = [];
  selectedBuilding: any = 0;
  selectedService: any = 0;
  bookingMessageList: Array<any> = [];
  additionalService: Array<any> = [];
  viewFromDate: string;
  viewToDate: string;
  viewName: string;
  //for filter by date
  filterTime: any = {
    fromDate: moment().format("DD.MM.YYYY"),
    fromTime: this.defaultOptions.businessHoursStart,
    toDate: moment().format("DD.MM.YYYY"),
    toTime: this.defaultOptions.businessHoursEnd
  };

  //notification from followup events
  followupEvents: Array<any> = [];

  isvalid: boolean = true;
  currentUserID: number = 0;
  displaytype: number;
  color = 'red';
  displayMessage = '';
  public showloader: boolean = false;
  private subscription: Subscription;
  private timer: Observable<any>;
  foodArray: Array<any> = [];
  lFoodArray: Array<any> = [];
  private events: Array<any> = [];
  private resorce: Array<any> = [];










  //this.data = JSON.parse(localStorage.getItem('currentUser'));
  constructor(private _bookingService: AppService, private _confirmService: confirmAlertService, private _bookingDialogService: BookingDialogService) {

  }


  tooltip(element, htmlContent): void {

    var content = '<div class="tooltip hide" style="position:fixed">' + htmlContent + '</div>';
    element.append(content);
    element.css('position', 'relative');

    var tooltip = element.find('.tooltip');
    tooltip.css('top', element.offset().top + element.height());
    tooltip.css('left', element.offset().left + element.width());


    element.mouseover(function (e) {
      tooltip.removeClass('hide');
    });


    element.mouseout(function (e) {
      tooltip.addClass('hide');
    })

  }

  ngOnInit(): void {


    moment.locale("nb");
    moment().format('L');

    var service = this.GetUrlParameter('service');
    var building = this.GetUrlParameter('building');
    if (service.trim() != '') {
      this.paramServiceId = parseInt(service.trim());
    }

    if (building.trim() != '') {
      this.paramBuildingId = parseInt(building.trim())
      this.selectedBuilding = this.paramBuildingId;
    }


    this._bookingService.GetBookingsDetail(AppSettings.RootPropertiesId, AppSettings.InitProperty, AppSettings.OrgUnitId, AppSettings.InitPropertyService.toString()).subscribe(data => {

      //building local resource data based on server side return.
      data.resources.forEach(function (value) {
        value.children.forEach(function (value2) {
          if (value2.ExtendedProperties && value2.ExtendedProperties.capacity && value2.ExtendedProperties.capacity != '0') {
            value2.title = value2.title + " (" + value2.ExtendedProperties.capacity + ")";
          }
          if (value2.children) {
            value2.children.forEach(function (value3) {
              if (value3.ExtendedProperties && value3.ExtendedProperties.capacity && value3.ExtendedProperties.capacity != '0') {
                value3.title = value3.title + " (" + value3.ExtendedProperties.capacity + ")";
              }
            });
          }

        });

      });

      this.resourseData = data.resources;

      this.currentUserID = data.currentUserID;

      this.properties = data.Properties;
      this.propertyServices = data.PropertyServices;

      if (data.FollowupEvents != null) {
        this.followupEvents = data.FollowupEvents;
      }
      var filter = data.Preferences.filter;
      if (filter) {
        var filterArray = filter.split(',');
        if (filter[0] && this.paramBuildingId == 0) {
          this.selectedBuilding = filterArray[0];
        }
        if (filter[1] && this.paramServiceId == 0) {
          this.selectedService = filterArray[1];
        }
      }



      this.getTimerList(data.Timers);

      this.meetingRooms = data.ServiceList;
      this.additionalService = data.AddonServiceList;

      //remove resource group if child is empty
      for (var i = data.resources.length - 1; i >= 0; i--) {
        if (data.resources[i].children.length == 0) {
          data.resources.splice(i, 1);
        }
      }

      var viewmode = 'timelineDay,week,timelineMonth,timelineYear';
      var defaultView = 'timelineDay';

      //support view one service
      var service = this.paramServiceId;
      if (service != 0) {
        this.filterbarDisplay = 'none';
        viewmode = '';
        defaultView = 'listDay';
        for (var i = 0; i < data.resources.length; i++) {
          var buildingData = data.resources[i];
          for (var j = 0; j < buildingData.children.length; j++) {
            if (buildingData.children[j].id == this.paramServiceId) {
              var serviceData = buildingData.children[j];
              var buildingResources = buildingData;
              buildingResources.children = Array(serviceData);
              data.resources = Array(buildingResources);
              break;
            }
          }
        }
        //              data.resources = Array(data.resources[0].children[0]);
        this.theme = 'theme-service-device';

      }


      //setup filterbar data
      for (var i = 0; i < data.resources.length; i++) {
        var item = data.resources[i];

        var servicetype = item.properties.servicetype;
        if (servicetype != undefined && this.servicetypeList[servicetype] == undefined && servicetype != '5') {
          this.servicetypeList[servicetype] = item.title;
        }

      }

      this.loaded = true;

      var self = this;
      this.calendarOptions = {
        schedulerLicenseKey: '0552136764-fcs-1519896585',
        now: this.date,
        navLinks: true,
        editable: false,
        locale: 'nb',
        aspectRatio: 1.8,
        contentHeight: 'auto',
        scrollTime: '07:00:00',
        minTime: '07:00:00',
        maxTime: '23:00:00',
        slotDuration: '00:15:00',
        businessHours: {
          dow: [1, 2, 3, 4, 5, 6, 0],
          start: '08:00',
          end: '18:00',
        },
        header: {
          left: 'prev,next today refresh, fromdate, todate',
          center: 'title',
          right: viewmode,
        },
        defaultView: defaultView,
        views: {
          day: {
            titleFormat: 'DD.MMMM YYYY, dddd',
          },
          timelineThreeDays: {
            type: 'timeline',
            duration: { days: 3 }
          },
          timelineDays: {
            type: 'timeline',
            duration: {},
            slotDuration: { days: 1 }
          },
          week: {
            type: 'timeline',
            duration: { days: 7 },
            slotDuration: '04:00',

          },
          timelineYear: {
            buttonText: 'År',
            slotDuration: { months: 1 }
          },
          agendaDay: {
            groupByResource: true,
          }
        },
        // eventOverlap: false,
        resourceAreaWidth: '300px',
        // resourceGroupField: 'title',
        slotWidth: 20,
        resourceColumns: [
          {
            group: true,
            labelText: 'Bygning',
            field: 'building'
          },
          {

            labelText: 'Tjenester',
            field: 'title'
          }//,

        ],
        resources: this.resourcesFct,

        selectable: true,
        selectHelper: true,
        resourcesInitiallyExpanded: true,

        customButtons: {
          refresh: {
            icon: 'refresh',
            click: (): void => {
              this._bookingService.GetBookingDetail().subscribe(data => {
                $('#calendar').fullCalendar('refetchEventSources', data.events);
                $('#calendar').fullCalendar('rerenderEvents');
              });
            }
          },

        },

        events: data.events,
        select: (start, end, jsEvent, view, resource): void => {

          //  this.modelTitle = 'Legg til tjeneste';
          // this.buttonTitle = 'Lagre';

          var bookingData = new Booking();
          if (resource) {
            this.isvalid = true;
            var overlap = $('#calendar').fullCalendar('clientEvents', (ev): void => {
              if (resource.id == ev.resourceId) {
                var eventDate = new Date(ev.start).setHours(0, 0, 0);
                var createdDate = new Date(start).setHours(0, 0, 0);
                if (eventDate == createdDate) {
                  var IsValidate = new Date(ev.start) < new Date(end) && new Date(start) < new Date(ev.end);


                  if (!IsValidate) {
                    this.isvalid = true;
                  }
                  else {
                    this.isvalid = false;

                  }
                }

              }
            });

            bookingData.mode = 'selectAdd';
            bookingData.bookingId = 0;
            bookingData.FromDate = moment(start).format("DD.MM.YYYY");
            bookingData.ToDate = moment(end).format("DD.MM.YYYY");
            bookingData.FromTimer = moment(start).format("HH:mm");
            bookingData.ToTimer = moment(end).format("HH:mm");
            bookingData.MeetingRoomId = resource.id;
            bookingData.PropertyServiceId = resource.parent.parent ? resource.parent.parent.id : resource.parent.id;
            if (bookingData.PropertyServiceId == 6191 || bookingData.PropertyServiceId == 6192) {
              bookingData.ToDate = '31.12.2099';
              bookingData.FromTimer = '08:00';
              bookingData.ToTimer = '08:00';
            }
            bookingData.PropertyId = this.getPropertyID(resource.parent.parent ? resource.parent.parent.building : resource.parent.building);
            bookingData.NoOfPeople = null,
              bookingData.IsFoodOrder = false;
            bookingData.UserID = '0';
            bookingData.IsConfirmed = false;
            bookingData.IsInternal = false;
            bookingData.IsMVA = false;
            bookingData.CompanyPayer = '0';
            bookingData.nameOfbook = '';
            bookingData.BookOrderName = this.currentUserID.toString();
            bookingData.FollowDate = '';
            bookingData.SendMessageType = '0';
            bookingData.Note = '';
            bookingData.InvoMessage = '';


          }
          else {
            bookingData.FromDate = moment(start).format("DD.MM.YYYY");
            bookingData.ToDate = moment(end).format("DD.MM.YYYY");
            bookingData.FromTimer = moment(start).format("HH:mm");
            bookingData.ToTimer = moment(end).format("HH:mm");
            this.isvalid = true;

          }

          if (this.isvalid) {
            this.displayMessage = "";
            this.openBooking(bookingData);
          }
          else {
            this.displayMessage = "Denne tjenesten allerede bestilt";
          }



        },
        eventClick: (event, jsEvent, view): void => {

          if (event.Status !== 99) {


            this._bookingService.getBookingDetailByBookingId(event.id).subscribe(event => {
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
                mode: 'edit',
                OrderHeadId: 0
              };
              this.openBooking(parameters);

              // this.foodArray = event.foods;
            })
            //   let  parameters:Booking  = {
            //         bookingId: event.id,
            //         FromDate: moment(event.start).format("DD.MM.YYYY"),
            //         ToDate:moment(event.end).format("DD.MM.YYYY"),
            //         FromTimer:moment(event.start).format("HH:mm"),
            //         ToTimer:moment(event.end).format("HH:mm"),
            //         MeetingRoomId: event.MeetingRoomId,
            //         PropertyServiceId: event.PropertyServiceId,
            //         PropertyId: event.buildingId,
            //         NoOfPeople: event.numOfPeople,
            //         IsFoodOrder: event.IsFoodOrder,
            //         UserID: event.UserID,
            //         CompanyPayer: event.CompanyPayer,
            //         nameOfbook: event.nameOfbook,
            //         BookOrderName: event.BookOrderName,
            //         FollowDate: event.FollowDate?moment(event.FollowDate).format("DD.MM.YYYY"):'',
            //         SendMessageType:event.SendMessageType.toString(),
            //         Note:event.Note,
            //         IsConfirmed:event.IsConfirmed,
            //         foods :event.foods,
            //         mode:'edit',
            //         OrderHeadId:0
            //     };







            this.isEventConfirmed = event.Status;


          }
        },
        eventMouseover: function (data, event, view) {

          var tooltip = '<div class="tooltiptopicevent tooltip" ><table>'
            + '<tr><td>Navn : </td><td>' + data.title + '</td></tr>'
            + '<tr><td>Fra  : </td><td>' + moment(data.start).format("DD.MM.YYYY HH:mm") + '</td></tr>'
            + '<tr><td>Til  : </td><td>' + moment(data.end).format("DD.MM.YYYY HH:mm") + '</td></tr>'
            + '<tr><td>Notater:</td><td>' + (data.Note ? data.Note : '') + '</td></tr></table></div>';


          $("body").append(tooltip);
          $(this).mouseover(function (e) {
            $(this).css('z-index', 10000);
            $('.tooltiptopicevent').fadeIn('500');
            $('.tooltiptopicevent').fadeTo(10, 1.9);
          }).mousemove(function (e) {
            $('.tooltiptopicevent').css('top', e.pageY + 10);
            $('.tooltiptopicevent').css('left', e.pageX + 20);
          });


        },
        eventMouseout: function (data, event, view) {
          $(this).css('z-index', 8);

          $('.tooltiptopicevent').remove();

        },

        eventLimit: true,

        viewDestroy: (view, element): void => {
          console.log(view);


        },
        viewRender: (view, element): void => {

          if (view.options.views.filterval) {
            view.options.views.filterval = null;

          }
          else {
            var $prevButton = $('.fc-prev-button');
            $prevButton.removeClass('fc-state-disabled');
            $prevButton.prop('disabled', true);

            var $nextButton = $('.fc-next-button');
            $nextButton.removeClass('fc-state-disabled');
            $nextButton.prop('disabled', true);

            this.getviewEventList(view.start, view.end, view.name);
          }





          // let fromDate=moment(view.start).format("YYYY-MM-DD");
          // let toDate=moment(view.end).format("YYYY-MM-DD");

          //     this._bookingService.GetBookingListByView(fromDate,toDate).subscribe(data => {
          //         $('#calendar').fullCalendar('removeEvents');
          //         $('#calendar').fullCalendar('addEventSource', data);
          //        $('#calendar').fullCalendar('rerenderEvents');
          //        $('#calendar').fullCalendar('changeView', view.name, view.start);
          //     })


          //   alert(moment(view.start).format("DD.MM.YYYY"));
        },
        //savan start
        // eventAfterAllRender : function (view) {
        //     if(view.name === 'listDay') {
        //         view.el.find('.fc-widget-header').append( '<span>&raquo; My Text</span>' );
        //     }
        // },
        // resourceRender: function (resourceObj, labelTds, bodyTds) {
        //     if (resourceObj.description) {
        //         $(labelTds).find('.fc-cell-text').append('&nbsp;<i class="fa fa-info-circle tooltip-description"></i>');
        //         self.tooltip($(labelTds).find('i.fa'), resourceObj.description);
        //     }
        // }
        //savan end
      };
      $(".fc-highlight").css("background", "red");
      $('#calendar').fullCalendar(this.calendarOptions);
    }

    );

    setInterval(() => {
      //  this.getBookingList();
      this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName);
    }, AppSettings.RefreshTimeInterval);


  }









  clickButton(model: any) {
    this.displayEvent = model;
  }









  filterList = (): void => {

    if (this.selectedService === '1') {
      $('#calendar').fullCalendar('changeView', 'timelineDay', this.date);
      //this.getviewEventList(this.date.toString(), this.date.toString(), 'day');
    }
    else if (this.selectedService === '2') {
      $('#calendar').fullCalendar('changeView', 'timelineDay', this.date);
      //this.getviewEventList(this.date.toString(), this.date.toString(), 'day');
    }
    else if (this.selectedService === '3') {
      $('#calendar').fullCalendar('changeView', 'timelineYear', this.viewFromDate);
    }
    else if (this.selectedService === '4') {
      $('#calendar').fullCalendar('changeView', 'timelineYear', this.viewFromDate);
    }
    else {

    }
    $('#calendar').fullCalendar('refetchResources');
  }

  openService(Data: any) {
    this.serviceOpen = true;
    if (this.serviceOpen) {
      if (Data != 0) {
        this.eventData = Data;
      }
      else {
        if (this.selectedBuilding != "0" || this.selectedService != 0) {
          var bookingData = new Booking();
          bookingData.PropertyId = this.selectedBuilding;
          // var propertyService= this.propertyServices[this.selectedService];
          bookingData.PropertyServiceId = AppSettings.InitServiceType;
          bookingData.BookOrderName = this.currentUserID.toString();
          this.eventData = bookingData;
        }
      }
    }
  }
  openBooking = (Data: any) => {
    // this._bookingDialogService.open(0);

    this.bookingopen = true;
    if (this.bookingopen) {
      if (Data != 0) {
        this.eventData = Data;
      }
      else {
        if (this.selectedBuilding != "0" || this.selectedService != 0) {
          var bookingData = new Booking();
          bookingData.PropertyId = this.selectedBuilding;


          const selbuildingObj = _.find(this.properties, x => x.MenuId == this.selectedBuilding);
          this.propertyServices = this.resourseData.filter(x => x.building == selbuildingObj.MenuName);


          var selService = _.find(this.propertyServices, x => x.properties.servicetype == this.selectedService);

          bookingData.PropertyServiceId = selService.id;
          bookingData.BookOrderName = this.currentUserID.toString();
          if (bookingData.PropertyServiceId == 6191 || bookingData.PropertyServiceId == 6192) {
            bookingData.ToDate = '31.12.2099';
            bookingData.FromTimer = '08:00';
            bookingData.ToTimer = '08:00';
          }
          this.eventData = bookingData;
        }

      }
    }

  }
  saveFilter = (): void => {
    this._bookingService.SaveFilter(this.selectedBuilding, this.selectedService).subscribe(data => {

      if (data == "1") {
        this.displayMessage = 'Filteret er lagret.';
        this.setTimer();
      }
    }

    );

  }




  getviewEventList(fromdata: string, todate: string, view: string) {
    if (fromdata) {

      this.viewFromDate = fromdata;
      this.viewToDate = todate;
      this.viewName = view;

      let fromDate = moment(fromdata, 'DD.MM.YYYY').format("YYYY-MM-DD");
      let toDate = moment(todate, 'DD.MM.YYYY').format("YYYY-MM-DD");

      this._bookingService.GetBookingListByView(fromDate, toDate).subscribe(data => {
        $('#calendar').fullCalendar('removeEvents');
        $('#calendar').fullCalendar('addEventSource', data);
        $('#calendar').fullCalendar('rerenderEvents');
        $('#calendar').fullCalendar('changeView', view, fromdata);

        setTimeout(() => {
          var $prevButton1 = $('.fc-prev-button');
          $prevButton1.removeClass('fc-state-disabled');
          $prevButton1.prop('disabled', false);

          var $nextButton1 = $('.fc-next-button');
          $nextButton1.removeClass('fc-state-disabled');
          $nextButton1.prop('disabled', false);

        }, 2000);

      })
    }

  }



  GetUrlParameter(name: string): string {
    name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(location.search);
    return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
  }

  resourcesFct = (callback: any): any => {
    let menuname = '';
    var filtered_resources = [];
    this.properties.forEach(data => {
      if (this.selectedBuilding == data.MenuId) {

        menuname = data.MenuName;
        return false;
      }


    });


    if (this.selectedBuilding == 0 && this.selectedService == 0) {
      filtered_resources = this.resourseData;
    }
    else {
      for (let i = 0; i < this.resourseData.length; i++) {
        if (this.selectedBuilding == 0 || this.selectedBuilding != 0 && this.resourseData[i].building.toLowerCase().indexOf(menuname.toLowerCase()) != -1) //building matches
        {
          var settings = this.resourseData[i].properties;
          if (this.selectedService == 0 || this.selectedService != 0 && settings != undefined && this.selectedService == settings['servicetype']) //service matches
          {
            filtered_resources.push(this.resourseData[i]);
          }
        }


      }
    }

    callback(filtered_resources);
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










  filterByDate(): void {
    var filterValue = this.filterTime;
    if (filterValue.fromDate != '' && filterValue.toDate == '') {
      filterValue.toDate = filterValue.fromDate;
    }
    if (filterValue.fromDate != '') {


      var fromTime = filterValue.fromTime == '' ? '00:00' : filterValue.fromTime; //todo: use default time
      var toTime = filterValue.toTime == '' ? '23:59' : filterValue.toTime; //todo: use default time

      var filterFrom = filterValue.fromDate + ' ' + fromTime;
      var filterTo = filterValue.toDate + ' ' + toTime;
      if (moment(filterFrom, 'DD.MM.YYYY hh:mm').unix() < moment(filterTo, 'DD.MM.YYYY hh:mm').unix()) {

        var views = $('#calendar').fullCalendar('option', 'views');
        views.filterval = true;

        $('#calendar').fullCalendar('option', 'businessHours', {
          start: filterValue.fromTime,
          end: filterValue.toTime,
        });
        if (filterValue.fromDate.toString() == filterValue.toDate.toString()) {
          $('#calendar').fullCalendar('changeView', 'timelineDay', filterValue.fromDate);
          this.viewName = 'timelineDay';
        }
        else {
          views.timelineDays.duration = { 'days': moment(filterValue.toDate).diff(filterValue.fromDate, 'days') + 1 };
          $('#calendar').fullCalendar('option', 'views', views);
          $('#calendar').fullCalendar('changeView', 'timelineDays', filterValue.fromDate);
          this.viewName = 'timelineDays';

        }
        this.viewFromDate = filterValue.fromDate;
        this.viewToDate = filterValue.toDate;
        this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName)


      }
    }
  }

  clearFilterByDate(): void {
    this.filterTime.fromDate = moment().format("DD.MM.YYYY");
    this.filterTime.toDate = moment().format("DD.MM.YYYY");
  }

  datetimeFormat(datetime: string): string {
    return moment(datetime).format("DD.MM.YYYY HH.mm");
  }
  getTimerList(data: Array<any>): void {
    let opts = new Array();
    _.forEach(data, function (time) {
      opts.push({
        value: time.toString(),
        label: time.toString()

      });
    });

    this.timerList = opts.slice(0);
  }

  getBookingListByView(fromdate: string, todate: string, mode: string): void {
    this._bookingService.GetBookingList(0).subscribe(data => {

      $(".fc-highlight").css("background", "red");


      $('#calendar').fullCalendar('removeEvents');
      $('#calendar').fullCalendar('addEventSource', data);
      $('#calendar').fullCalendar('rerenderEvents');
    }

    );
  }
  getBookingList(): void {
    this._bookingService.GetBookingList(0).subscribe(data => {

      $(".fc-highlight").css("background", "red");


      $('#calendar').fullCalendar('removeEvents');
      $('#calendar').fullCalendar('addEventSource', data);
      $('#calendar').fullCalendar('rerenderEvents');
    }

    );
  }

  SavedBooking(data: any): void {

    if (data.errorType == 0) {
      this.bookingopen = false;
      this.displayMessage = data.data;
      this.setTimer();
      // this.getBookingList();
      this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName);
    }

  }
  SavedService(data: any): void {

    if (data.errorType == 0) {
      this.serviceOpen = false;
      this.displayMessage = "Service added successfully";
      this.setTimer();
      // this.getBookingList();
      //  this.getviewEventList(this.viewFromDate,this.viewToDate,this.viewName);
    }
  }
  deleteBooking(data: any): void {
    if (data.errorType == 0) {
      this.bookingopen = false;
      this.displayMessage = data.data;
      this.setTimer();
      // this.getBookingList();
      this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName);
    }
  }
  deleteService(data: any): void {
    if (data.errorType == 0) {
      this.serviceOpen = false;
      this.displayMessage = data.data;
      this.setTimer();
      // this.getBookingList();
      // this.getviewEventList(this.viewFromDate, this.viewToDate, this.viewName);
    }
  }
  getPropertyID(name: string): number {
    let propertyId = 0;
    this.properties.forEach(data => {
      if (name === data.MenuName) {
        propertyId = data.MenuId;

      }
    });
    return propertyId;
  }
}
