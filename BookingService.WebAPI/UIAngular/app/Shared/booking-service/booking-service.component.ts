import { Component, OnInit, Output, EventEmitter, Input, OnChanges } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl, ValidatorFn, FormArray, FormControl } from '@angular/forms';
import { AppService } from '../../app.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { AppSettings } from '../../appConfig';
import { Property } from '../../model/Property';
import { PropertyService } from '../../model/PropertyService';
import { MeetingRoom } from '../../model/MeetingRoom';
import { Services } from '../../model/Services';
import { ServiceAlertService } from "../service-alert/service-alert.Service";
//import { confirmAlertService } from '../confirmationAlert/service/confirmAlert.service';
import { BookingDialogService } from '../booking/booking-dialog.service';
import { BookingMode } from 'src/app/model/bookingMode';
import { Observable, Subject, concat, of, merge, zip, BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap, switchMap, catchError, map } from 'rxjs/operators';
import { async } from 'q';
import { ToastService } from 'src/app/toast.service';

declare var jquery: any;
declare var $: any;

@Component({
  selector: "app-booking-service",
  templateUrl: "./booking-service.component.html",
  styleUrls: ["./booking-service.component.css"]
})
export class BookingServiceComponent implements OnInit, OnChanges {
  @Input() currentUserID: string;
  //data: <formdata>
  //mode: 'edit'
  @Input() parameters: any;
  @Input() isOpen: boolean = false;
  //when initialize(load)
  @Output() init = new EventEmitter<Array<any>>();

  //when opening the dialog
  //@Output() open = new EventEmitter<Array<any>>();

  @Output() saved = new EventEmitter<Array<any>>();

  //when close without saving
  @Output() closed = new EventEmitter<any>();

  //when submit
  @Output() submit = new EventEmitter<Array<any>>();

  @Output() deleted = new EventEmitter<Array<any>>();

  mode: string = 'add';

  dialogTitle = 'Add Booking';
  buttonTitle = 'Save';
  isActive = false;
  servicePrice: number;
  serviceCostPrice: number;
  foodArray: Array<any> = [];
  isEventConfirmed: boolean = false;
  serviceForm: FormGroup;
  userForm: FormGroup;
  selectedUser: string;
  selUser: any;
  selectedCustomer: any;
  properties: Property[];
  userList: Array<any> = [];
  companyList: Array<any> = [];
  userOrderList: Array<any> = [];
  priceValue: any;
  propertyServices: PropertyService[];
  meetingRooms: MeetingRoom[];
  selectedServiceMember: number;
  foodList: Services[];
  timerList: Array<any> = [];
  lFoodArray: Array<any> = [];
  displayWarning = '';
  displayCustomerWarning = '';
  displayUserWarning = '';
  deletedbookingId: number;
  display = 'none';
  displayCustomer = 'none';
  customerOpen = false;
  userOpen = false;
  displayUser = 'none';
  displayMessage = '';
  bookingMessageList: Array<any> = [];
  additionalService: Array<any> = [];
  addedServices: Array<any> = [];
  subServiceData: Array<any> = [];
  //masterUserData: Array<any>=[];
  searchedCustomerNo: any;
  updatedEventService: number;
  initCount: boolean = false;
  initMva: boolean = false;
  customerData: any;
  visiblities: any = { 'bookingtab': true, 'orderlinetab': true, 'actionstab': true };
  foodDisplay = 'block';
  outsideCustomer: Observable<any>;
  bufferSize = 20;
  numberOfItemsFromEndBeforeFetchingMore = 10;
  pageSize = 0;

  confirmData: any;

  AllCustomer: BehaviorSubject<any> = new BehaviorSubject<any>([]);

  addonPrice: number = 0;
  addonTotal: number = 0;
  addonCostTotal: number = 0;
  resourseData: any;
  modeConfig: BookingMode = AppSettings.DefaultMode;

  confirmTitle: string;
  confirmType: string;
  confirmQuestion: string;
  ConfirmButton: string;


  get Foods(): FormArray {
    return <FormArray>this.serviceForm.get('Foods');
  }
  constructor(private _bookingService: AppService, private fb: FormBuilder, private _confirmService: ServiceAlertService, private _bookingDialogService: BookingDialogService, private _toastService: ToastService) {


  }


  OpenCustomerDialog() {

    this.customerOpen = true;


  }



  OpenUserDialog() {

    this.userOpen = true;
  }
  open(parameters: any) {

    //$('.nav-tabs a:first')[0].click();
    this.display = 'block';
    var mode = parameters && parameters['mode'] ? parameters['mode'] : 'add';
    this.updateMode(mode);

    if (mode == 'add') {
      this.ResetForm(parameters);
      this.addFood();
    }
    else {

      if (parameters) {


        this.UpdateFormValue(this.parameters);
        this.foodArray = this.parameters.foods;
        this.isEventConfirmed = this.parameters.Status;


        const control = <FormArray>this.serviceForm.controls['Foods'];
        for (let i = control.length - 1; i >= 0; i--) {
          control.removeAt(i);

        }
        if (this.foodArray) {
          this.updatedEventService = this.parameters.MeetingRoomId;
          for (var i = 0; i < this.foodArray.length; i++) {
            this.addFood();
          }

          if (this.parameters.foods) {
            _.forEach(this.parameters.foods, (food): void => {
              let a = this.setTwoNumberDecimal(food.Price);
              let b = this.setTwoNumberDecimal(food.CostPrice);
              food.Price = this.setTwoNumberDecimal(food.Price);
              food.CostPrice = this.setTwoNumberDecimal(food.CostPrice);

            })
            this.serviceForm.get('Foods').setValue(this.parameters.foods);
          }

        }
        else {
          this.addFood();
        }

      }
    }

  }


  ngOnChanges(changes) {
    if (changes['isOpen']) {
      if (this.isOpen) {

        this.open(this.parameters);
      }
      else {
        this.close();
      }
    }

  }

  ngOnInit() {


    this._bookingService.GetDataforCreateBooking(AppSettings.RootPropertiesId, AppSettings.InitProperty, AppSettings.OrgUnitId, AppSettings.InitPropertyService.toString()).subscribe(data => {
      this.resourseData = data.resources;

      this.properties = data.Properties;
      this.propertyServices = data.PropertyServices;
      // this.getUserList(data.UserList);

      this.getCompanyList(data.CustomerList);
      this.getUserOrderList(data.UserList);
      this.GetFoodService(data.PropertyServices);
      this.getTimerList(data.Timers);
      this.currentUserID = data.currentUserID;
    });







    this.serviceForm = this.fb.group({
      bookingId: 0,
      FoodFormId: AppSettings.FoodFormId,
      PropertyId: [AppSettings.InitProperty, [Validators.required]],
      PropertyServiceId: [AppSettings.InitPropertyService, [Validators.required]],
      nameOfbook: ['', [Validators.required]],


      FromDate: ["", [Validators.required]],
      MeetingRoomId: [AppSettings.InitMeetingRoom, [Validators.required]],
      BookOrderName: ['', [Validators.required]],
      CompanyPayer: ['', [Validators.required]],
      UserID: '',
      IsFoodOrder: false,
      IsInternal: false,
      IsMVA: false,
      // meetingRoomData: this.buidMeeting() ,
      Foods: this.fb.array([this.buildFood()]),
      SumTotal: '0',
      CostSumTotal: '0',
      SendMessageType: '0',

    });
    // this.display = 'none';




    // this._bookingDialogService.change.subscribe(data => {
    //     this.display = data.isOpen;

    //     if (data.isOpen) {
    //         this.display = 'block';
    //         if(data.event){
    //          this.UpdateFormValue(data.event);
    //         }
    //     }
    //     else {
    //         this.display = 'none';
    //     }
    // });

    this.onServicesvalChanges();
    this.onChanges();
    this.onUserChange();
    this.onCustomerChange();

    this.onServiceChanges();
    //    this.onToTimeChange(0);
    //    this.onToTimerChange(0);
    //    this.onFromTimeChange(0);
    //    this.onFromTimerChange(0);
    this.onAddvalChanges(0, 0);
    this.OnQtyChange;
    this.OnPriceChange;
    //    this.onIsInternamValueChange();

    this.serviceForm.get('IsInternal').valueChanges.subscribe(IsIntval => {


      if (this.initCount) {
        this.setConfirmDialogValue('Skal priser oppdateres?', 'Endre', 'Dersom du ønsker at prisene skal oppdateres, velge Endre. Dersom du ikke ønsker at prisene skal oppdateres velg Avbryt', 'Endre', null)
        this._confirmService.toggle();
      }
      else {
        this.ChangePriceCommand();
      }
      this.initCount = true;


    });

    this.onVatChanges(false);

    //this.OnPriceQtyChange();
    //  this.propertyServices = this._bookingService.getPropertySerives().filter((item) => item.propertyid == this.serviceForm.get('propertyId').value);



  }

  getUserList(data: Array<any>): void {
    let opts = new Array();

    _.forEach(data, function (user) {
      opts.push({
        value: user.PersonID.toString(),
        label: user.DisplayName,

      });
    });
    //   for (let i = 0; i < data.length; i++) {

    //       opts[i] = {
    //           value: data[i].PersonID.toString(),
    //           label: data[i].DisplayName + " (" + data[i].OrganizationUnitName + ")",
    //           cvalue:data[i].OrganizationUnitId
    //       };
    //   }
    this.userList = opts.slice(0);
    if (this.selectedUser) {
      this.serviceForm.get('UserID').setValue(this.selectedUser);
    }

    //this.masterUserData=[...this.userList];
  }

  get formArray() {
    // Typecast, because: reasons
    // https://github.com/angular/angular-cli/issues/6099
    return <FormArray>this.serviceForm.get('Foods');
  }
  getUserOrderList(data: Array<any>): void {
    let opts = new Array();
    _.forEach(data, function (user) {
      opts.push({
        value: user.PersonID.toString(),
        label: user.DisplayName

      });
    });
    // for (let i = 0; i < data.length; i++) {

    //     opts[i] = {
    //         value: data[i].PersonID.toString(),
    //         label: data[i].DisplayName
    //     };
    // }
    this.userOrderList = opts.slice(0);
  }
  DeleteBooking(BookingId: number): void {
    this.deletedbookingId = BookingId;
    this.setConfirmDialogValue('Slett', 'Slett', 'Er du sikker p� at du vil slette?', 'Slett', null)
    this._confirmService.toggle();
  }

  setConfirmDialogValue = (title: string, type: string, question: string, confirmButton: string, data: any) => {

    this.confirmTitle = title;
    this.confirmType = type;
    this.confirmQuestion = question;
    this.confirmData = data;
    this.ConfirmButton = confirmButton;
  }
  getCompanyList(data: Array<any>): void {
    let opts = new Array();
    // let cList= _.uniqBy(data,"OrganizationUnitId");
    _.forEach(data, function (company) {
      opts.push({
        value: company.OrganizationUnitId.toString(),
        label: company.OrganizationUnitName.toString(),
        isPrimary: company.IsPrimary,
        IsMVA: company.IsMVA
      });
    });
    // for (let i = 0; i < cList.length; i++) {

    //     opts[i] = {
    //         value: cList[i].OrganizationUnitId.toString(),
    //         label: cList[i].OrganizationUnitName.toString()
    //     };
    // }
    this.companyList = opts.slice(0);

    // this.companyList= _.uniqBy(this.companyList,"value");
    // this.companyList = this.companyList.filter((value, index, array) =>
    //  !array.filter((v, i) => JSON.stringify(value) == JSON.stringify(v) && i < index).length);

  }
  ResetForm(data: any): void {
    this.displayWarning = '';
    this.selectedUser = '';
    this.selUser = '';
    this.selectedCustomer = '';
    let propertyId = AppSettings.InitProperty;
    let propertyServiceId = AppSettings.InitPropertyService;
    let propertyValuesId = AppSettings.InitService;
    let bookordername = '';
    if (data) {
      if (data.PropertyId) {
        propertyId = data.PropertyId;
        propertyServiceId = 0;
        propertyValuesId = 0;

      }

      bookordername = data.BookOrderName;
      // if(data.PropertyServiceId){
      //     propertyServiceId=data.PropertyServiceId;
      // }
    }
    this.serviceForm.patchValue({


      bookingId: 0,
      PropertyId: propertyId,
      PropertyServiceId: propertyServiceId,

      MeetingRoomId: propertyValuesId,
      FromDate: moment(new Date()).format("DD.MM.YYYY"),

      nameOfbook: '',
      CompanyPayer: '0',
      UserID: '0',
      BookOrderName: bookordername,
      IsFoodOrder: false,
      //IsInternal:false,
      SendMessageType: '0'


    });
    this.servicePrice = 0;
    this.serviceCostPrice = 0;
    const control = <FormArray>this.serviceForm.controls['Foods'];

    for (let i = control.length - 1; i >= 0; i--) {
      control.removeAt(i);
    }
  }

  onChanges(): void {
    this.serviceForm.get('PropertyId').valueChanges.subscribe(val => {
      if (val == '1026') {
        this._bookingService.getServices(val).subscribe(data => {
          const servicevals = [{ "MenuId": 4121, "MenuName": "Kontortjenester", "MenuDesc": null, "PicturePath": null, "ExtendedProperteis": {}, "properties": {}, "id": 4121, "Children": null }]

          this.propertyServices = servicevals;
          this.meetingRooms = [];
          this.serviceForm.get('PropertyServiceId').setValue(this.propertyServices[0].MenuId);
          this.GetFoodService(servicevals);
        });
      }
      else {
        const servicevals = [];
        this.propertyServices = servicevals;
        this.GetFoodService(servicevals);
      }


    });
  }

  //   getFilterUserList=(companyId :any)=>{

  //     return this.masterUserData.filter(x=>x.cvalue===companyId);
  //      }

  getPersonListByCustomer = (custId: number) => {
    this._bookingService.getUserListbyCustomer(custId).subscribe(res => {
      this.getUserList(res);
    })
  }
  getPropertyID(name: string): void {

    this.properties.forEach(data => {
      if (name === data.MenuName) {
        this.serviceForm.get('PropertyId').setValue(data.MenuId);
      }
    });
  }
  onUserChange(): void {

    this.serviceForm.get('UserID').valueChanges.subscribe(val => {
      // this.userList.find(x => x.value==val);
      let company = _.find(this.companyList, x => x.value == this.serviceForm.get('CompanyPayer').value);
      let user = _.find(this.userList, x => x.value == val);
      if (user) {
        if (company) {
          const nbookval = company.label + this.repeatstr(" ", 1) + '(' + user.label + ')';
          if (this.serviceForm.get("bookingId").value == 0) {

            this.serviceForm.get('nameOfbook').setValue(nbookval);
          }
          else {
            if (this.selUser != val) {

              this.serviceForm.get('nameOfbook').setValue(nbookval);
              this.selUser = val;
            }
          }
        }

        // this.bookingForm.get('CompanyPayer').setValue(user.cvalue);
      }
      //   this.userList.forEach(data => {
      //       if (val === data.value) {
      //           this.serviceForm.get('nameOfbook').setValue(data.label);
      //           this.serviceForm.get('CompanyPayer').setValue(data.cvalue);
      //       }
      //   });
    })
  }


  onCustomerChange(): void {
    this.serviceForm.get('CompanyPayer').valueChanges.subscribe(val => {
      if (val) {
        if (val != 0) {
          let company = _.find(this.companyList, x => x.value == val);
          // this.initCount=false;
          if (this.serviceForm.get("bookingId").value == 0) {
            this.serviceForm.get('nameOfbook').setValue(company.label);
          }
          else {
            if (this.selectedCustomer != val) {
              this.serviceForm.get('nameOfbook').setValue(company.label);
              this.selectedCustomer = val;
            }
          }

          this.getPersonListByCustomer(val);
          let existingval = this.serviceForm.get('IsInternal').value;
          let existingMvaVal = this.serviceForm.get('IsMVA').value;
          if (company.IsMVA) {
            if (existingMvaVal === true) {
              this.initMva = false;
            }
            this.serviceForm.get('IsMVA').setValue(true);
          }
          else {
            if (existingMvaVal === false) {
              this.initMva = false;
            }
            else {
              this.initMva = true;
            }

            this.serviceForm.get('IsMVA').setValue(false);
          }
          if (company.isPrimary) {

            if (existingval === true) {
              this.initCount = false;

            }

            this.serviceForm.get('IsInternal').setValue(true);


          }
          else {
            if (existingval === false) {
              this.initCount = false;

            }
            else {
              this.initCount = true;
            }


            this.serviceForm.get('IsInternal').setValue(false);

          }
          //     if(this.initCount){
          // if(existingval!==this.serviceForm.get('IsInternal').value){
          //     // this.setConfirmDialogValue('Skal priser oppdateres?','Endre','Dersom du ønsker at prisene skal oppdateres, velge Endre. Dersom du ikke ønsker at prisene skal oppdateres velg Avbryt')
          //     // this._confirmService.toggle();
          //     this.initCount=true;
          // }
          // else{
          //     this.initCount=false;
          // }
          // }

        }
      }



    });
  }


  clearFormArray = (formArray: FormArray) => {
    while (formArray.length !== 0) {
      formArray.removeAt(0)
    }
  }
  onServiceChanges(): void {

    this.serviceForm.get('PropertyServiceId').valueChanges.subscribe(val => {
      if (val) {
        this._bookingService.getMeetingRoomList(val).subscribe(data => {
          this.meetingRooms = data;
          this.subServiceData.length = 0;
          for (var i = 0; i < data.length; i++) {
            if (this.subServiceData.indexOf(data[i].meta_keywords) === -1) {
              this.subServiceData.push(data[i].meta_keywords);
            }
          }
          this.serviceForm.get('MeetingRoomId').setValue(this.meetingRooms[0].MainID);
        });
      }



    });
  }

  onServicesvalChanges(): void {
    this.serviceForm.get("MeetingRoomId").valueChanges.subscribe(val => {


      const prvalue = this.serviceForm.get('PropertyServiceId').value;
      this._bookingService.getMeetingRoomList(prvalue).subscribe(data => {
        this.meetingRooms = data;
        if (val != null) {
          this._bookingService.getAddtionalServiceList(val, AppSettings.serviceFormId).subscribe(data => {
            this.additionalService.length = 0;
            this.additionalService = data;


            if (this.serviceForm.get("bookingId").value == 0) {
              this.serviceForm.get('IsFoodOrder').setValue(false);
              const control = <FormArray>this.serviceForm.controls['Foods'];

              // for (let i = control.length - 1; i >= 0; i--) {
              //     control.removeAt(i);
              // }
              // this.addFood();
              const servicesValue = <FormArray>this.serviceForm.controls['Foods'];
              let values = servicesValue.value;

              let service = _.find(this.meetingRooms, x => x.MainID == val);

              //if(this.serviceForm.get("FromDate").value && this.serviceForm.get("ToTimer").value && this.serviceForm.get("FromTimer").value && this.serviceForm.get("ToDate").value){
              this._bookingService.getServiceVatArticles(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {

                let valofarticle = 0;
                if (articleData.Article1) {
                  valofarticle = articleData.Article1.toString();
                }

                //   if(this.serviceForm.get("ToTimer").value>this.serviceForm.get("FromTimer").value){
                //       this._bookingService.getBookingPriceDetail(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.serviceForm.get("FromDate").value, this.serviceForm.get("ToDate").value,this.serviceForm.get("FromTimer").value,this.serviceForm.get("ToTimer").value).subscribe(data => {
                //         //   if(this.serviceForm.get("CompanyPayer").value){
                //         //     let isIntCustomer=this.serviceForm.get("IsInternal").value;
                //         //       let company=_.find(this.companyList,x=>x.value==this.serviceForm.get("CompanyPayer").value);
                //         //     if(isIntCustomer){
                //         //         this.servicePrice=data.UnitPrice;
                //         //         this.serviceCostPrice=data.CostPrice1;
                //         //     }
                //         //     else{
                //         //       this.servicePrice=data.SecondaryPrice;
                //         //       this.serviceCostPrice=data.CostPrice2;
                //         //     }
                //         //   }
                //         //   else{
                //         //       this.servicePrice = data.SecondaryPrice;
                //         //       this.serviceCostPrice=data.CostPrice2;
                //         //   }

                //         //   values[0].Price=this.setTwoNumberDecimal(this.servicePrice);
                //         //   values[0].Sum=this.setTwoNumberDecimal(this.servicePrice);
                //         //   values[0].CostPrice=this.setTwoNumberDecimal(this.serviceCostPrice);
                //         //   values[0].CostTotal=this.setTwoNumberDecimal(this.serviceCostPrice);

                //           // if(values[0].IsVatApply){
                //           //     values[0].ArticleId=articleData.Article1;
                //           // }
                //           // else{
                //           //     values[0].ArticleId=articleData.Article2;
                //           // }
                //           values[0].MainServiceId=val;
                //           values[0].ArticleId=valofarticle;
                //           values[0].Tekst=this.getHeadLineValue(service.Headline);

                //           values[0].ServiceText=service.Headline;
                //           values[0].Qty=1;
                //           values[0].IsVatApply=true;
                //           values[0].IsKitchenOrder=false;
                //           this.serviceForm.get("Foods").patchValue(values, {onlySelf:true,emitEvent: false});
                //           this.totalValue();
                //       });
                //   }
                //  else{
                // if(values.length>0){
                values[0].ArticleId = valofarticle;
                values[0].Tekst = this.getHeadLineValue(service.Headline);
                values[0].ServiceText = service.Headline;
                values[0].Qty = 1;
                values[0].MainServiceId = val;
                values[0].IsVatApply = true;
                values[0].IsKitchenOrder = false;
                this.serviceForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                // }

                // if(values[0].IsVatApply){
                //     values[0].ArticleId=articleData.Article1;
                // }
                // else{
                //     values[0].ArticleId=articleData.Article2;
                // }


                //  }
              })

              // }

              //  for(let j=0;j<values.length;j++){



              // }
              //   this.addFood();
              this.totalValue();
            }
            else {
              var aa = this.serviceForm.get("MeetingRoomId").value;

              if (this.updatedEventService != val) {
                this.serviceForm.get('IsFoodOrder').setValue(false);
                const servicesValue = <FormArray>this.serviceForm.controls['Foods'];
                let values = servicesValue.value;
                if (values[0].OrderHeadId == 0) {
                  let service = _.find(this.meetingRooms, x => x.MainID == val);
                  // if(this.serviceForm.get("FromDate").value && this.serviceForm.get("ToTimer").value && this.serviceForm.get("FromTimer").value && this.serviceForm.get("ToDate").value){
                  this._bookingService.getServiceVatArticles(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {

                    let valofarticle = 0;
                    if (articleData.Article1) {
                      valofarticle = articleData.Article1.toString();
                    }

                    //   if(this.serviceForm.get("ToTimer").value>this.serviceForm.get("FromTimer").value){
                    //       this._bookingService.getBookingPriceDetail(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.serviceForm.get("FromDate").value, this.serviceForm.get("ToDate").value,this.serviceForm.get("FromTimer").value,this.serviceForm.get("ToTimer").value).subscribe(data => {


                    //           values[0].MainServiceId=val;
                    //           values[0].ArticleId=valofarticle;
                    //           values[0].Tekst=this.getHeadLineValue(service.Headline);
                    //           values[0].ServiceText=service.Headline;
                    //           values[0].Qty=1;
                    //           values[0].IsVatApply=true;
                    //           values[0].IsKitchenOrder=false;


                    //           this.serviceForm.get("Foods").patchValue(values, {onlySelf:true,emitEvent: false});
                    //           this.totalValue();
                    //       });
                    //   }
                    // else{
                    values[0].ArticleId = valofarticle;
                    values[0].Tekst = this.getHeadLineValue(service.Headline);
                    values[0].ServiceText = service.Headline;
                    values[0].Qty = 1;
                    values[0].MainServiceId = val;

                    values[0].IsVatApply = true;
                    values[0].IsKitchenOrder = false;

                    this.serviceForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });

                    //  }
                  })

                  //}
                  this.updatedEventService = val;
                  // this.addFood();
                  this.totalValue();
                }

              }
            }
          });
        }
      })



    });
  }
  save = (): void => {
    var inputData = this.serviceForm.value;
    inputData["MessageList"] = this.bookingMessageList;
    inputData["IsBooking"] = false;
    //return;
    this._bookingService.Book(inputData).subscribe(data => {
      if (data.errorType != 0) {
        this.displayWarning = data.data;
      }
      else {
        this.displayWarning = '';
        this.ResetForm(null);
        this.saved.emit(data);
      }

    }, error => { });

    //$.notify({
    //    title: "<strong>Welcome:</strong> ",
    //    message: "This plugin has been provided to you by Robert McIntosh aka <a href=\"https://twitter.com/Mouse0270\" target=\"_blank\">@mouse0270</a>"
    //});
  }

  closeCustomerModel() {
    this.displayCustomer = 'none';
    this.ResetCustomerForm();
  }
  closeUserModel() {
    this.displayUser = 'none';
  }
  close() {
    this.display = 'none';


    if (this.isOpen) {
      this.isOpen = false;
      // this.parameters = undefined;
      this.ResetForm(null);
      this.closed.emit();
    }

  }
  onVatChanges(newval: any): void {

    this._bookingService.getServiceVatArticles(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {
      let val = "";
      if (newval == true) {
        val = articleData.Article1.toString();
      }
      if (newval == false) {
        val = articleData.Article2.toString();
      }
      const servicesValue = <FormArray>this.serviceForm.controls['Foods'];
      let values = servicesValue.value;
      values[0].ArticleId = val;

      //  values[0].IsVatApply=values[0].IsVatApply?true:false;


      this.serviceForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
    });
  }

  // onAddvalChanges(newval: any, indexval: any): void {
  //   if (newval != 0) {

  //     const control = <FormArray>this.serviceForm.controls['Foods'];
  //     if (control.length > 0) {
  //       this.additionalService.forEach((item, index) => {
  //         //  this.addedServices.length = 0;
  //         this.addedServices = item.ServiceList;
  //         for (let i = 0; i < control.length; i++) {

  //           let price = control.value[i].Price;
  //           if (newval.target.value == control.value[i].FoodId) {
  //             let addedsubService = _.find(this.addedServices, x => x.SId == newval.target.value)
  //             // if(price==0){
  //             if (i = indexval) {
  //               if (addedsubService) {
  //                 if (this.serviceForm.get("CompanyPayer").value) {
  //                   let isIntCustomer = this.serviceForm.get("IsInternal").value;
  //                   let company = _.find(this.companyList, x => x.value == this.serviceForm.get("CompanyPayer").value);
  //                   if (isIntCustomer) {
  //                     control.value[i].Price = this.setTwoNumberDecimal(addedsubService.Price);
  //                     control.value[i].Sum = this.setTwoNumberDecimal(addedsubService.Price * control.value[i].Qty);
  //                     control.value[i].CostPrice = this.setTwoNumberDecimal(addedsubService.CostPrice1);
  //                     control.value[i].CostTotal = this.setTwoNumberDecimal(addedsubService.CostPrice1 * control.value[i].Qty);
  //                   }
  //                   else {
  //                     control.value[i].Price = this.setTwoNumberDecimal(addedsubService.SecondaryPrice);
  //                     control.value[i].Sum = this.setTwoNumberDecimal(addedsubService.SecondaryPrice * control.value[i].Qty);
  //                     control.value[i].CostPrice = this.setTwoNumberDecimal(addedsubService.CostPrice2);
  //                     control.value[i].CostTotal = this.setTwoNumberDecimal(addedsubService.CostPrice2 * control.value[i].Qty);

  //                   }
  //                 }
  //                 else {

  //                   control.value[i].Price = this.setTwoNumberDecimal(addedsubService.SecondaryPrice);
  //                   control.value[i].Sum = this.setTwoNumberDecimal(addedsubService.SecondaryPrice * control.value[i].Qty);
  //                   control.value[i].CostPrice = this.setTwoNumberDecimal(addedsubService.CostPrice2);
  //                   control.value[i].CostTotal = this.setTwoNumberDecimal(addedsubService.CostPrice2 * control.value[i].Qty);


  //                 }
  //                 // control.value[i].Price = addedsubService.Price;
  //                 // control.value[i].Sum = addedsubService.Price * control.value[i].Qty;
  //                 control.value[i].Tekst = addedsubService.SName;
  //                 control.value[i].ArticleId = addedsubService.ArticleId;
  //                 control.value[i].MainServiceId = addedsubService.SId;
  //                 control.value[i].IsKitchenOrder = control.value[i].IsKitchenOrder ? true : false;
  //                 //}


  //                 this.serviceForm.get("Foods").patchValue(control.value, { onlySelf: true, emitEvent: false });
  //               }
  //             }


  //             // this.serviceForm.get("Foods").updateValueAndValidity();
  //             this.totalValue();
  //           }


  //         }
  //       })

  //     }
  //   }
  // }
  onAddvalChanges(newval: any, indexval: any): void {
    if (newval != 0) {

      const addedsubService = _(this.additionalService)
        .thru(function (coll) {
          return _.union(coll, _.map(coll, 'ServiceList'));
        })
        .flatten().find(x => x.SId == Number(newval.target.value));
      var arrayControl = this.serviceForm.get('Foods') as FormArray;

      if (addedsubService) {
        if (this.serviceForm.get("CompanyPayer").value) {
          let isIntCustomer = this.serviceForm.get("IsInternal").value;
          let company = _.find(this.companyList, x => x.value == this.serviceForm.get("CompanyPayer").value);
          if (isIntCustomer) {

            arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.Price));
            arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.Price * arrayControl.at(indexval).get('Qty').value));
            arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice1));
            arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice1 * arrayControl.at(indexval).get('Qty').value));

          }
          else {
            arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice));
            arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice * arrayControl.at(indexval).get('Qty').value));
            arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2));
            arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2 * arrayControl.at(indexval).get('Qty').value));


          }
        }
        else {
          arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice));
          arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice * arrayControl.at(indexval).get('Qty').value));
          arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2));
          arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2 * arrayControl.at(indexval).get('Qty').value));



        }


        arrayControl.at(indexval).get('Tekst').setValue(addedsubService.SName);
        arrayControl.at(indexval).get('ArticleId').setValue(addedsubService.ArticleId);
        arrayControl.at(indexval).get('MainServiceId').setValue(addedsubService.SId);
        arrayControl.at(indexval).get('IsKitchenOrder').setValue(arrayControl.at(indexval).get('IsKitchenOrder').value ? true : false);




      }


      this.totalValue();
    }
  }
  OnPriceChange(newval: any, indexval: any): void {

    let formsval = this.serviceForm.get('Foods') as FormArray;
    formsval.at(indexval).get('Price').setValue(newval.target.value);

    formsval.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('Qty').value * newval.target.value));

    this.totalValue();


  }
  OnQtyChange(newval: any, indexval: any): void {

    let formsval = this.serviceForm.get('Foods') as FormArray;
    formsval.at(indexval).get('Qty').setValue(newval.target.value);

    formsval.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('Price').value * newval.target.value));
    formsval.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('CostPrice').value * newval.target.value));
    this.totalValue();

  }
  // OnPriceQtyChange(): void {
  //   this.serviceForm.get("Foods").valueChanges.subscribe(val => {
  //     if (val.length > 0) {
  //       if (this.additionalService.length > 0) {
  //         this.additionalService.forEach((item, index) => {
  //           //  this.addedServices.length = 0;
  //           this.addedServices = item.ServiceList;
  //           this.addedServices.forEach((ix, ind) => {

  //             for (let i = 0; i < val.length; i++) {
  //               if (val[i].ArticleId) {
  //                 let isExist = _.find(this.addedServices, x => x.SId == val[i].ArticleId);
  //                 if (isExist) {
  //                   if (ix.SId == val[i].FoodId) {

  //                     val[i].Sum = this.setTwoNumberDecimal(val[i].Price * val[i].Qty);
  //                     val[i].CostTotal = this.setTwoNumberDecimal(val[i].CostPrice * val[i].Qty);
  //                     this.serviceForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
  //                     this.totalValue();
  //                   }
  //                 }
  //                 else {
  //                   val[i].Sum = this.setTwoNumberDecimal(val[i].Price * val[i].Qty);
  //                   val[i].CostTotal = this.setTwoNumberDecimal(val[i].CostPrice * val[i].Qty);
  //                   this.serviceForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
  //                   this.totalValue();
  //                 }

  //               }
  //               else {

  //                 // this.isRead=false;
  //                 //  var arrayControl = this.serviceForm.get('Foods') as FormArray;
  //                 //  arrayControl.at(i).get("Qty").disable();
  //                 // var b=arrayControl.at(i);
  //                 //   val[i].Sum = val[i].Price * val[i].Qty;
  //                 //  this.serviceForm.get("Foods").patchValue(arrayControl, {onlySelf:true,emitEvent: false});
  //                 //  this.totalValue();


  //               }


  //             }


  //           });


  //         });
  //       }
  //       else {
  //         for (let i = 0; i < val.length; i++) {
  //           if (val[i].ArticleId) {


  //             val[i].Sum = this.setTwoNumberDecimal(val[i].Price * val[i].Qty);
  //             val[i].CostTotal = this.setTwoNumberDecimal(val[i].CostPrice * val[i].Qty);
  //             this.serviceForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
  //             this.totalValue();


  //           }



  //         }
  //       }

  //     }

  //   });
  // }

  deleteFood(index: number) {
    // control refers to your formarray
    this.setConfirmDialogValue(' Slett', 'FoodDelete', 'Er du sikker på at du vil slette?', 'Slett', index)
    this._confirmService.toggle();

  }


  getServicePrice = (serviceId: number, formId: number, fromDate: string, toDate: string, fTimer: string, tTimer: string): void => {

    this._bookingService.getBookingPriceDetail(serviceId, formId, fromDate, toDate, fTimer, tTimer, false, 0).subscribe(data => {
      this.servicePrice = data.SecondaryPrice;
    })

  }

  setTwoNumberDecimal(value: any): any {
    let a = parseFloat(value).toFixed(2);
    return (a);
  }
  totalValue() {
    this.addonTotal = 0;
    this.addonCostTotal = 0;
    let totalval = 0;
    let totalCostval = 0;
    const control = <FormArray>this.serviceForm.controls['Foods']
    const subcontrol = control.controls;

    for (let i = 0; i < subcontrol.length; i++) {

      var oh = <FormGroup>subcontrol[i];
      totalval += oh.value["Price"] * oh.value["Qty"];
      totalCostval += oh.value["CostPrice"] * oh.value["Qty"]
      this.addonTotal = this.setTwoNumberDecimal(totalval);
      this.addonCostTotal = this.setTwoNumberDecimal(totalCostval);
    }
  }

  addFood(): void {
    this.Foods.push(this.buildFood());

  }
  focusOutFunction(event: any, index: any): void {
    let formsval = this.serviceForm.get('Foods') as FormArray;
    formsval.at(index).get('Price').setValue(this.setTwoNumberDecimal(event.target.value));
  }
  focusCostPriceFunction(event: any, index: any): void {
    let formsval = this.serviceForm.get('Foods') as FormArray;
    formsval.at(index).get('CostPrice').setValue(this.setTwoNumberDecimal(event.target.value));

  }

  buildFood(): FormGroup {
    return this.fb.group({
      FoodId: 0,
      Qty: 1,
      ArticleId: '',
      MainServiceId: '',
      IsKitchenOrder: false,
      OrderHeadId: 0,
      OrderListId: 0,
      IsVatApply: false,
      Price: this.setTwoNumberDecimal(0),
      Sum: this.setTwoNumberDecimal(0),
      CostPrice: this.setTwoNumberDecimal(0),
      CostTotal: this.setTwoNumberDecimal(0),
      Tekst: '',
      Time: '',
      ServiceText: '',

    });
  }

  getHeadLineValue(headline: string): string {

    let building = _.find(this.properties, x => x.MenuId == this.serviceForm.get("PropertyId").value);
    return building.MenuName + " " + headline + " " + this.serviceForm.get("FromDate").value;
  }
  GetFoodService(data: Array<any>): void {
    let opts = new Array();
    for (let i = 0; i < data.length; i++) {
      if (data[i].MenuName == 'Tilleggstjenester') {
        this._bookingService.getFoodList(data[i].MenuId, AppSettings.FoodFormId).subscribe(data => this.foodList = data);
      }

    }

  }


  ChangePriceCommand = () => {
    let isIntCustomer = this.serviceForm.get("IsInternal").value;
    const servicesValue = <FormArray>this.serviceForm.controls['Foods'];
    let values = servicesValue.value;
    for (let j = 0; j < values.length; j++) {

      if (values[j].MainServiceId == this.serviceForm.get("MeetingRoomId").value) {
        if (values[j].OrderHeadId == 0) {
          // if(this.serviceForm.get("FromDate").value && this.serviceForm.get("ToTimer").value && this.serviceForm.get("FromTimer").value && this.serviceForm.get("ToDate").value){
          //     if(this.serviceForm.get("ToTimer").value>this.serviceForm.get("FromTimer").value){
          //         this._bookingService.getBookingPriceDetail(this.serviceForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.serviceForm.get("FromDate").value, this.serviceForm.get("ToDate").value,this.serviceForm.get("FromTimer").value,this.serviceForm.get("ToTimer").value).subscribe(data => {
          //             if(isIntCustomer){
          //                 this.servicePrice=data.UnitPrice;
          //                 this.serviceCostPrice=data.CostPrice1;

          //             }
          //             else{
          //               this.servicePrice=data.SecondaryPrice;
          //               this.serviceCostPrice=data.CostPrice2;

          //             }
          //             values[j].Price=this.setTwoNumberDecimal(this.servicePrice);
          //             values[j].Sum=this.setTwoNumberDecimal(this.servicePrice);
          //             values[j].CostPrice=this.setTwoNumberDecimal(this.serviceCostPrice);
          //             values[j].CostTotal=this.setTwoNumberDecimal(this.serviceCostPrice);
          //             values[j].IsVatApply=values[j].IsVatApply?true:false;
          //             this.serviceForm.get("Foods").patchValue(values, {onlySelf:true,emitEvent: false});
          //             this.totalValue();
          //         });
          //         }
          //     }
        }


      }
      else {
        this.additionalService.forEach((item, index) => {

          this.addedServices = item.ServiceList;
          this.addedServices.forEach((ix, ind) => {
            if (ix.SId == values[j].FoodId) {
              if (values[j].OrderHeadId == 0) {
                if (isIntCustomer) {
                  //savan
                  values[j].Price = this.setTwoNumberDecimal(ix.Price);
                  values[j].Sum = this.setTwoNumberDecimal(ix.Price * values[j].Qty);
                  values[j].CostPrice = this.setTwoNumberDecimal(ix.CostPrice1);
                  values[j].CostTotal = this.setTwoNumberDecimal(ix.CostPrice1 * values[j].Qty);
                }
                else {
                  //savan
                  values[j].Price = this.setTwoNumberDecimal(ix.SecondaryPrice);
                  values[j].Sum = this.setTwoNumberDecimal(ix.SecondaryPrice * values[j].Qty);
                  values[j].CostPrice = this.setTwoNumberDecimal(ix.CostPrice2);
                  values[j].CostTotal = this.setTwoNumberDecimal(ix.CostPrice2 * values[j].Qty);
                }
                values[j].Tekst = ix.SName;
                // values[j].ArticleId =ix.SId;
                values[j].ArticleId = ix.ArticleId;
                values[j].MainServiceId = ix.SId;

                values[j].IsKitchenOrder = values[j].IsKitchenOrder ? true : false;
                //  values[j].IsVatApply=values[j].IsVatApply?true:false;
              }



            }

          });


        })
      }

    }
    this.serviceForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
    this.totalValue();

  }
  ConfirmCommand(value: any): void {

    if (value.type === 'Slett') {
      this.Confirmdelete(value.val);
    }
    if (value.type === 'Endre') {
      this.ConfirmChangeCommand(value.val);
    }
    if (value.type === 'FoodDelete') {
      this.FoodDelete(value.val, value.data);
    }
    if (value.type === 'RemoveOrderline') {
      this.OrderLineConfirmDelete(value.val, value.data);
    }

  }

  OrderLineConfirmDelete = (value: boolean, data: any) => {
    if (!value) {
      const control = <FormArray>this.serviceForm.controls['Foods'];

      const result = control.at(data);
      result.value.IsKitchenOrder = true;
      this.serviceForm.get("Foods").patchValue(control.value, { onlySelf: true, emitEvent: false });

    }
    else {
      this._confirmService.toggle();
    }
  }
  ConfirmChangeCommand = (value: boolean) => {
    if (value) {
      let isIntCustomer = this.serviceForm.get("IsInternal").value;
      const servicesValue = <FormArray>this.serviceForm.controls['Foods'];
      let values = servicesValue.value;
      for (let j = 0; j < values.length; j++) {

        this.additionalService.forEach((item, index) => {

          this.addedServices = item.ServiceList;
          this.addedServices.forEach((ix, ind) => {
            if (ix.SId == values[j].FoodId) {
              if (values[j].OrderHeadId == 0) {
                if (isIntCustomer) {
                  //savan
                  values[j].Price = this.setTwoNumberDecimal(ix.Price);
                  values[j].Sum = this.setTwoNumberDecimal(ix.Price * values[j].Qty);
                  values[j].CostPrice = this.setTwoNumberDecimal(ix.CostPrice1);
                  values[j].CostTotal = this.setTwoNumberDecimal(ix.CostPrice1 * values[j].Qty);
                }
                else {
                  //savan
                  values[j].Price = this.setTwoNumberDecimal(ix.SecondaryPrice);
                  values[j].Sum = this.setTwoNumberDecimal(ix.SecondaryPrice * values[j].Qty);
                  values[j].CostPrice = this.setTwoNumberDecimal(ix.CostPrice2);
                  values[j].CostTotal = this.setTwoNumberDecimal(ix.CostPrice2 * values[j].Qty);
                }
                values[j].Tekst = ix.SName;
                // values[j].ArticleId =ix.SId;
                //savan commented
                // values[j].ArticleId = ix.ArticleId;
                //savan commented end
                values[j].MainServiceId = ix.SId;

                values[j].IsKitchenOrder = values[j].IsKitchenOrder ? true : false;
                //  values[j].IsVatApply=values[j].IsVatApply?true:false;
              }



            }

          });


        })

      }
      this.serviceForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
      this.totalValue();
      this._confirmService.toggle();
    }
    else {
      this.serviceForm.get('IsInternal').setValue(!this.serviceForm.get("IsInternal").value, { onlySelf: true, emitEvent: false });
      // this._confirmService.toggle();
    }
  }
  Confirmdelete = (value: boolean) => {
    if (value) {
      this._bookingService.DeleteBookingById(this.deletedbookingId, false).subscribe(data => {


        if (data.errorType != 0) {
          this.displayWarning = data.data;
        }
        else {
          this.deleted.emit(data);
        }
        this._confirmService.toggle();

      });
    }

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


  SavedUser = (data: any) => {
    if (data) {
      this._toastService.showSuccess('User added successfully', 'Success!');

      this._bookingService.getUserListbyCustomer(this.serviceForm.get('CompanyPayer').value).subscribe(res => {
        this.getUserList(res);
        this.userOpen = false;
      }, (err) => console.error(err), () => {

        setTimeout(() => {

          this.serviceForm.get('UserID').patchValue(data.toString())
        }, 1000);


      });



    }
  }
  SavedCustomer = (data: any) => {
    if (data) {
      this._toastService.showSuccess('Customer added successfully', 'Success!');

      this._bookingService.getCutomerList().subscribe(resData => {

        this.getCompanyList(resData);
        this.serviceForm.get('CompanyPayer').patchValue(data.toString());

        this.customerOpen = false;
      }, (err) => console.error(err), () => { setTimeout(() => { console.log(this.companyList); this.serviceForm.get('CompanyPayer').patchValue(data.toString()) }, 3000); });
    }
  }

  updateMode(mode: string) {
    this.mode = mode;
    if (this.mode == 'add') {
      this.dialogTitle = "Tjenestekjøp";
      this.buttonTitle = 'Lagre';

      this.modeConfig = AppSettings.DefaultMode;
    }
    else if (this.mode == 'edit') {
      this.dialogTitle = "tjenestekjøp (" + this.parameters.bookingId + ")";
      this.buttonTitle = 'Oppdater';

      this.modeConfig = AppSettings.EditMode;
    }
    else if (this.mode == 'orderlineEdit') {
      this.dialogTitle = "Oppdater booking (" + this.parameters.bookingId + ")";
      this.buttonTitle = 'Oppdater';


      this.modeConfig = AppSettings.kitchenMode;
    }
    else if (this.mode == 'orderEdit') {
      this.dialogTitle = "Oppdater order (" + this.parameters.OrderHeadId + ")";
      this.buttonTitle = 'Oppdater';
      this.modeConfig = AppSettings.OrderMode;
    }
    else if (this.mode == "OrderViewMode") {
      this.dialogTitle = "order (" + this.parameters.OrderHeadId + ")";
      this.buttonTitle = 'Oppdater';
      this.modeConfig = AppSettings.OrderViewMode;
    }
    else if (this.mode == "BookingViewMode") {
      this.dialogTitle = " booking (" + this.parameters.bookingId + ")";
      this.buttonTitle = 'Oppdater';

      this.modeConfig = AppSettings.BookingViewMode;
    }
  }

  UpdateFormValue(event: any): void {

    this.selectedCustomer = event.CompanyPayer;
    this.selUser = event.UserID;
    this.selectedUser = event.UserID;
    this.serviceForm.patchValue({
      bookingId: event.bookingId,

      PropertyId: event.PropertyId,
      PropertyServiceId: event.PropertyServiceId,
      MeetingRoomId: event.MeetingRoomId,
      CompanyPayer: event.CompanyPayer,

      BookOrderName: event.BookOrderName,
      nameOfbook: event.nameOfbook,


      FromDate: event.FromDate,

      IsFoodOrder: event.IsFoodOrder,

      SendMessageType: event.SendMessageType
      // UserID: event.UserID


    });
  }
  repeatstr = (ch: any, n: any): any => {
    var result = "";

    while (n-- > 0)
      result += ch;

    return result;
  }
  ResetCustomerForm = () => {
    this.searchedCustomerNo = '';
    this.displayCustomerWarning = '';
    this.customerData = '';
  }

  FoodDelete = (value: boolean, data: any) => {
    if (value) {
      const control = <FormArray>this.serviceForm.controls['Foods'];
      // remove the chosen row
      control.removeAt(data);
      this.totalValue();
      this._confirmService.toggle();
    }
    else {
      //this._confirmService.toggle();
    }
  }
  // getSearch=(data:any)=>{
  //    this.indCustomer=[];
  //    this.pageSize=0;
  //     this.getindData(data);
  // }
  // onScroll=(data:any)=>{
  //    if(data.allow){
  //        this.pageSize+=1;
  //        this.getindData(data.search);
  //    }
  // }

  onIsKitchenChange = (data: any): void => {
    console.log(data.srcElement.attributes.id.value.substring(9));
    var currentID = data.srcElement.attributes.id.value.substring(9);
    const control = <FormArray>this.serviceForm.controls['Foods'];
    var result = control.at(currentID);
    console.log(result);
    if (this.mode === 'orderlineEdit') {
      this.setConfirmDialogValue(' Slett', 'RemoveOrderline', 'Er du sikker på at du vil slette?', 'Slett', currentID)
      //   this.setConfirmDialogValue(' Slett?','RemoveOrderline','Er du sikker på at du vil slette?',null)
      this._confirmService.toggle();
    }

  }

}
