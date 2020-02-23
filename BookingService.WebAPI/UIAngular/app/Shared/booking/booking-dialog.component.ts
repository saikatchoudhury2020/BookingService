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
import { confirmAlertService } from '../confirmationAlert/service/confirmAlert.service';
import { BookingDialogService } from './booking-dialog.service';
import { BookingMode } from 'src/app/model/bookingMode';
import { Observable, Subject, concat, of, merge, zip, BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap, switchMap, catchError, map } from 'rxjs/operators';
import { async } from 'q';
import { ToastService } from 'src/app/toast.service';

declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-booking-dialog',
  templateUrl: './booking-dialog.component.html',
  styleUrls: ['./booking-dialog.component.css']
})
export class BookingDialogComponent implements OnInit, OnChanges {
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
  serviceArticle: any;
  serviceCostPrice: number;
  foodArray: Array<any> = [];
  isEventConfirmed: boolean = false;
  bookingForm: FormGroup;
  userForm: FormGroup;
  serviceArticleVal: any;
  serviceArticlePrice: any;
  properties: Property[];
  userList: Array<any> = [];
  companyList: Array<any> = [];
  userOrderList: Array<any> = [];
  priceValue: any;
  isbuildfoodValuesShow: boolean = true;
  selectedUser: string;
  selectedCustomer: any;
  selUser: any;
  propertyServices: PropertyService[];
  meetingRooms: Array<MeetingRoom> = [];
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
    return <FormArray>this.bookingForm.get('Foods');
  }
  constructor(private _bookingService: AppService, private fb: FormBuilder, private _confirmService: confirmAlertService, private _bookingDialogService: BookingDialogService, private _toastService: ToastService) {


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
        this.initCount = false;
        this.initMva = false;
        this.UpdateFormValue(this.parameters);
        this.foodArray = this.parameters.foods;
        this.isEventConfirmed = this.parameters.Status;


        const control = <FormArray>this.bookingForm.controls['Foods'];
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
              food.Sum = this.setTwoNumberDecimal(food.Sum);
              food.Sum = this.setTwoNumberDecimal(food.Sum);
              food.CostTotal = this.setTwoNumberDecimal(food.CostTotal);
            })
            this.bookingForm.get('Foods').setValue(this.parameters.foods);
          }

        }
        else {
          this.addFood();
        }

      }
      this.totalValue();
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
      this.currentUserID = data.currentUserID;

      this.properties = data.Properties;
      this.propertyServices = data.PropertyServices;
      // this.getUserList(data.UserList);

      this.getCompanyList(data.CustomerList);
      this.getUserOrderList(data.UserList);
      this.GetFoodService(data.PropertyServices);
      this.getTimerList(data.Timers);

    });







    this.bookingForm = this.fb.group({
      bookingId: 0,
      FoodFormId: AppSettings.FoodFormId,
      PropertyId: [AppSettings.InitProperty, [Validators.required]],
      PropertyServiceId: [AppSettings.InitPropertyService, [Validators.required]],

      UserID: '',
      nameOfbook: ['', [Validators.required]],
      FromDate: ["", [Validators.required]],
      ToDate: ["", [Validators.required]],
      FromTimer: ['', [Validators.required]],
      ToTimer: ["", [Validators.required]],
      MeetingRoomId: [AppSettings.InitMeetingRoom, [Validators.required]],
      NoOfPeople: [0, [Validators.required, Validators.maxLength(2)]],
      FollowDate: "",
      BookOrderName: ['', [Validators.required]],
      CompanyPayer: ['', [Validators.required]],
      IsFoodOrder: false,
      IsInternal: false,
      IsMVA: false,
      // meetingRoomData: this.buidMeeting() ,
      Foods: this.fb.array([this.buildFood()]),
      SumTotal: '0',
      CostSumTotal: '0',
      SendMessageType: '0',
      Note: '',
      InvoMessage: ''
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
    // this.getServiceArticleByMeetingRoomId();
    this.onCustomerChange();
    this.IsMVAUpdate();
    this.onServiceChanges();
    this.onToTimeChange(0);
    this.onToTimerChange(0);
    this.onFromTimeChange(0);
    this.onFromTimerChange(0);
    this.onAddvalChanges(0, 0);
    //    this.onIsInternamValueChange();

    this.bookingForm.get('IsInternal').valueChanges.subscribe(IsIntval => {


      if (this.initCount) {
        if (!this.IsInternamUpdate()) {
          this.setConfirmDialogValue('Skal priser oppdateres?', 'Endre', 'Dersom du ønsker at prisene skal oppdateres, velge Endre. Dersom du ikke ønsker at prisene skal oppdateres velg Avbryt', 'Endre', null)
          this._confirmService.toggle();
        }

      }
      else {
        this.ChangePriceCommand();
      }
      this.initCount = true;


    });

    this.bookingForm.get('IsMVA').valueChanges.subscribe(val => {


      if (this.initMva) {
        if (!this.IsMVAUpdate()) {
          this.setConfirmDialogValue('Skal MVA oppdateres?', 'MVA', 'Skal MVA oppdateres?', 'Endre', null)
          this._confirmService.toggle();
        }


      }
      else {
        //this.changeMvaCommand();
      }

      this.initMva = true;
    });

    //this.onVatChanges(false);

    this.OnPriceChange;
    this.OnQtyChange;
    this.OnArticleChange;
    this.OnCostPriceChange;
    //  this.propertyServices = this._bookingService.getPropertySerives().filter((item) => item.propertyid == this.bookingForm.get('propertyId').value);
    this.bookingForm.get("FollowDate").valueChanges.subscribe(val => {
      if (this.bookingForm.get("FollowDate").value) {
        if (moment(this.bookingForm.get("FromDate").value, 'DD.MM.YYYY').isBefore(moment(val, 'DD.MM.YYYY'))) {

          this.bookingForm.get("FollowDate").setErrors({ 'incorrect': true });
        }
      }

    });

    this.bookingForm.get("NoOfPeople").valueChanges.subscribe(newval => {
      if (newval) {
        this.selectedServiceMember = 0;
        if (this.bookingForm.get("MeetingRoomId").value) {
          this._bookingService.getserviceMembers(this.bookingForm.get("MeetingRoomId").value, AppSettings.serviceFormId)
            .subscribe(val => {
              if (val != 0) {
                if (newval > val) {
                  this.selectedServiceMember = val;
                  this.bookingForm.get("NoOfPeople").setErrors({ 'incorrect': true });
                }
              }
              this._bookingService.getServiceIsMutiplePrice(this.bookingForm.get("MeetingRoomId").value, AppSettings.serviceFormId)
                .subscribe(nval => {
                  const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
                  let values = servicesValue.value;
                  if (values[0].OrderHeadId == 0) {
                    let service = _.find(this.meetingRooms, x => x.MainID == this.bookingForm.get("MeetingRoomId").value);
                    if (service) {
                      values[0].Tekst = this.getHeadLineValue(service.Headline);

                      if (nval === true) {
                        values[0].Qty = 1;
                        this._bookingService.getBookingExternalPriceDetail(this.bookingForm.get("IsInternal").value, newval, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value)
                          .subscribe(extData => {
                            if (this.bookingForm.get("IsInternal").value) {
                              values[0].Price = this.setTwoNumberDecimal(extData.UnitPrice);
                            }
                            else {
                              values[0].Price = this.setTwoNumberDecimal(extData.SecondaryPrice);
                            }
                            if (!this.bookingForm.get("IsMVA").value) {
                              values[0].Price = values[0].Price * AppSettings.mvaPrice;
                            }

                            values[0].Qty = extData.Quantity;
                            values[0].Sum = this.setTwoNumberDecimal(values[0].Price * values[0].Qty);
                            values[0].CostTotal = this.setTwoNumberDecimal(values[0].CostPrice * values[0].Qty);

                            this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                            this.totalValue();
                          });



                      }
                      else {
                        this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                      }







                    }
                  }
                });

            })
        }

      }

    });
  }
  isArrayEqual = (x, y): any => {
    return _(x).differenceWith(y, _.isEqual).isEmpty();
  }
  IsInternamUpdate = () => {
    let isval = true;

    const control = <FormArray>this.bookingForm.controls['Foods'];

    _.forEach(control.value, (item) => {

      if (item.MainServiceId === this.bookingForm.get("MeetingRoomId").value) {
        if (item.OrderHeadId == 0) {
          if (this.serviceArticlePrice) {
            if (this.serviceArticlePrice.UnitPrice !== this.serviceArticlePrice.SecondaryPrice) {
              isval = false;
              return isval;
            }
          }

        }
      }
      else {

        _.forEach(this.additionalService, (service) => {
          const subService = _.find(service.ServiceList, x => x.SId == item.MainServiceId);
          if (_.find(service.ServiceList, x => x.SId == item.MainServiceId)) {
            if (subService.UnitPrice !== subService.SecondaryPrice) {
              isval = false;
              return isval;
            }
          }

        })
      }


    });


    return isval;
  }
  IsMVAUpdate = () => {
    let isval = true;

    const control = <FormArray>this.bookingForm.controls['Foods'];

    _.forEach(control.value, (item) => {

      if (item.MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
        if (item.OrderHeadId == 0) {
          if (this.serviceArticleVal) {
            if (this.serviceArticleVal.Article1 !== this.serviceArticleVal.Article2) {
              isval = false;
              return isval;
            }
          } else {
            isval = false;
            return isval;

          }

        }
      }
      else {

        _.forEach(this.additionalService, (service) => {
          const subService = _.find(service.ServiceList, x => x.SId == item.MainServiceId);
          if (_.find(service.ServiceList, x => x.SId == item.MainServiceId)) {
            if (subService.ArticleId !== subService.ArticleId2) {
              isval = false;
              return isval;
            }
          }

        })
      }


    });


    return isval;
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
      this.bookingForm.get('UserID').setValue(this.selectedUser);
    }
    //this.masterUserData=[...this.userList];
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
    this.setConfirmDialogValue('Slett', 'SlettBooking', 'Er du sikker på at du vil slette?', 'Slett', null)
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
    //this.initCount = false;
    // this.initMva = false;
    this.displayWarning = '';
    this.selectedUser = '';
    this.selUser = '';
    this.selectedCustomer = '';
    let propertyId = AppSettings.InitProperty;
    let propertyServiceId = AppSettings.InitPropertyService;
    let propertyValuesId = AppSettings.InitMeetingRoom;
    let tDate = moment(new Date()).format("DD.MM.YYYY");
    let fTime = AppSettings.InitTime;
    let tTime = AppSettings.InitTime;
    let bookordername = '';
    if (data) {
      if (data.PropertyId) {
        propertyId = data.PropertyId;
        propertyServiceId = 0;
        propertyValuesId = 0;
      }
      if (data.PropertyServiceId) {
        propertyServiceId = data.PropertyServiceId;
        if (propertyServiceId == 6191 || propertyServiceId == 6192) {
          tDate = '31.12.2099';
          fTime = '08:00';
          tTime = '08:00';
        }
      }
      bookordername = data.BookOrderName;
    }
    this.bookingForm.patchValue({
      UserID: '0',
      nameOfbook: '',
      bookingId: 0,
      PropertyId: propertyId,
      PropertyServiceId: propertyServiceId,
      NoOfPeople: '',
      MeetingRoomId: propertyValuesId,
      FromDate: moment(new Date()).format("DD.MM.YYYY"),
      ToDate: tDate,
      FollowDate: '',
      FromTimer: fTime,
      ToTimer: tTime,
      CompanyPayer: '0',
      BookOrderName: bookordername,
      IsFoodOrder: false,

      SendMessageType: '0',
      Note: '',
      InvoMessage: ''
    });
    this.servicePrice = 0;
    this.serviceCostPrice = 0;
    const control = <FormArray>this.bookingForm.controls['Foods'];

    for (let i = control.length - 1; i >= 0; i--) {
      control.removeAt(i);
    }
  }

  onChanges(): void {
    this.bookingForm.get('PropertyId').valueChanges.subscribe(val => {
      if (val) {
        this._bookingService.getPropertyServices(val).subscribe(data => {
          this.propertyServices = data;
          _.remove(this.propertyServices, function (e) {
            return e.MenuId == AppSettings.InitServiceType;
          });
          //savan
          //this.meetingRooms=[];
          //this.bookingForm.get('PropertyServiceId').setValue(this.propertyServices[0].MenuId);
          //savan end
          this.GetFoodService(data);
        });
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
        this.bookingForm.get('PropertyId').setValue(data.MenuId);
      }
    });
  }
  onUserChange(): void {

    this.bookingForm.get('UserID').valueChanges.subscribe(val => {
      // this.userList.find(x => x.value==val);
      let company = _.find(this.companyList, x => x.value == this.bookingForm.get('CompanyPayer').value);
      let user = _.find(this.userList, x => x.value == val);
      if (user) {
        if (company) {
          const nbookval = company.label + this.repeatstr(" ", 1) + '(' + user.label + ')';
          if (this.bookingForm.get("bookingId").value == 0) {
            this.bookingForm.get('nameOfbook').setValue(nbookval);
          }
          else {
            if (this.selUser != val) {

              this.bookingForm.get('nameOfbook').setValue(nbookval);
              this.selUser = val;
            }
          }

        }

        // this.bookingForm.get('CompanyPayer').setValue(user.cvalue);
      }
      //   this.userList.forEach(data => {
      //       if (val === data.value) {
      //           this.bookingForm.get('nameOfbook').setValue(data.label);
      //           this.bookingForm.get('CompanyPayer').setValue(data.cvalue);
      //       }
      //   });
    })
  }


  onCustomerChange(): void {
    this.bookingForm.get('CompanyPayer').valueChanges.subscribe(val => {
      if (val) {
        if (val != 0) {
          let company = _.find(this.companyList, x => x.value == val);
          // this.initCount=false;
          if (this.bookingForm.get("bookingId").value == 0) {
            this.bookingForm.get('nameOfbook').setValue(company.label);
          }
          else {
            if (this.selectedCustomer != val) {
              this.bookingForm.get('nameOfbook').setValue(company.label);
              this.selectedCustomer = val;
            }
          }

          this.getPersonListByCustomer(val);
          let existingval = this.bookingForm.get('IsInternal').value;
          let existingMvaVal = this.bookingForm.get('IsMVA').value;

          if (company.IsMVA) {
            if (existingMvaVal === true) {
              this.initMva = false;
            }
            this.bookingForm.get('IsMVA').setValue(true);
          }
          else {
            if (existingMvaVal === false) {
              this.initMva = false;
            }
            else {
              this.initMva = true;
            }

            this.bookingForm.get('IsMVA').setValue(false);
          }
          if (company.isPrimary) {

            if (existingval === true) {
              this.initCount = false;

            }

            this.bookingForm.get('IsInternal').setValue(true);


          }
          else {
            if (existingval === false) {
              this.initCount = false;

            }
            else {
              this.initCount = true;
            }


            this.bookingForm.get('IsInternal').setValue(false);

          }
          //     if(this.initCount){
          // if(existingval!==this.bookingForm.get('IsInternal').value){
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
  onToTimeChange(newVal: any): void {
    if (newVal != 0) {

      if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToDate").value) {
        if (this.bookingForm.get("FromDate").value > newVal) {

          this.bookingForm.get("ToDate").setErrors({ 'incorrect': true });
        }
        else {
          this.bookingForm.get("FromDate").setErrors(null);
        }
      }
      if (this.bookingForm.get("MeetingRoomId").value) {
        if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
          if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
            this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
              if (this.bookingForm.get("CompanyPayer").value) {
                let isIntCustomer = this.bookingForm.get("IsInternal").value;
                const isMVACustomer = this.bookingForm.get('IsMVA').value;
                let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                if (isIntCustomer) {
                  this.servicePrice = data.UnitPrice;
                  this.serviceCostPrice = data.CostPrice1;
                }
                else {
                  this.servicePrice = data.SecondaryPrice;
                  this.serviceCostPrice = data.CostPrice2;
                }
                if (isMVACustomer) {
                  this.serviceArticle = data.Article1;
                }
                else {
                  this.serviceArticle = data.Article2;
                }
              }
              else {
                this.serviceArticle = data.Article2;
                this.servicePrice = data.SecondaryPrice;
                this.serviceCostPrice = data.CostPrice2;
              }

              const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
              let values = servicesValue.value;
              for (let j = 0; j < values.length; j++) {
                if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
                  if (values[j].OrderHeadId == 0) {
                    values[j].ArticleId = this.serviceArticle;
                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].IsVatApply = values[j].IsVatApply ? true : false;
                    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                    this.totalValue();
                  }

                }

              }
            })


          }

        }
      }
    }
  }
  onToTimerChange(newVal: any): void {
    if (newVal) {
      if (this.bookingForm.get("MeetingRoomId").value) {
        if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
          if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
            this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
              if (this.bookingForm.get("CompanyPayer").value) {
                let isIntCustomer = this.bookingForm.get("IsInternal").value;
                const isMVACustomer = this.bookingForm.get('IsMVA').value;
                let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                if (isIntCustomer) {
                  this.servicePrice = data.UnitPrice;
                  this.serviceCostPrice = data.CostPrice1;
                }
                else {
                  this.servicePrice = data.SecondaryPrice;
                  this.serviceCostPrice = data.CostPrice2;
                }
                if (isMVACustomer) {
                  this.serviceArticle = data.Article1;
                }
                else {
                  this.serviceArticle = data.Article2;
                }
              }
              else {
                this.serviceArticle = data.Article2;
                this.servicePrice = data.SecondaryPrice;
                this.serviceCostPrice = data.CostPrice2;
              }

              const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
              let values = servicesValue.value;
              for (let j = 0; j < values.length; j++) {
                if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
                  if (values[j].OrderHeadId == 0) {
                    if (data.IsMutiplePrice) {
                      if (!this.bookingForm.get('IsMVA').value) {
                        this.servicePrice = this.servicePrice * AppSettings.mvaPrice;
                      }

                      values[j].Qty = data.Quantity;
                      values[j].Sum = this.setTwoNumberDecimal(this.servicePrice * data.Quantity);
                    }
                    else {
                      values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                    }
                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);

                    values[j].ArticleId = this.serviceArticle;
                    values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].IsVatApply = values[j].IsVatApply ? true : false;
                    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                    this.totalValue();
                  }
                }

              }
            })



          }

        }
      }
    }
  }
  onFromTimerChange(newVal: any): void {
    if (newVal) {

      if (this.bookingForm.get("MeetingRoomId").value) {
        if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
          if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
            this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
              if (this.bookingForm.get("CompanyPayer").value) {
                let isIntCustomer = this.bookingForm.get("IsInternal").value;
                const isMVACustomer = this.bookingForm.get('IsMVA').value;
                let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                if (isIntCustomer) {
                  this.servicePrice = data.UnitPrice;
                  this.serviceCostPrice = data.CostPrice1;
                }
                else {
                  this.servicePrice = data.SecondaryPrice;
                  this.serviceCostPrice = data.CostPrice2;
                }
                if (isMVACustomer) {
                  this.serviceArticle = data.Article1;
                }
                else {
                  this.serviceArticle = data.Article2;
                }
              }
              else {
                this.serviceArticle = data.Article2;
                this.servicePrice = data.SecondaryPrice;
                this.serviceCostPrice = data.CostPrice2;
              }

              const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
              let values = servicesValue.value;
              for (let j = 0; j < values.length; j++) {
                if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
                  if (values[j].OrderHeadId == 0) {
                    if (data.IsMutiplePrice) {
                      if (!this.bookingForm.get('IsMVA').value) {
                        this.servicePrice = this.servicePrice * AppSettings.mvaPrice;
                      }

                      values[j].Qty = data.Quantity;
                      values[j].Sum = this.setTwoNumberDecimal(this.servicePrice * data.Quantity);
                    }
                    else {
                      values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                    }
                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);

                    values[j].ArticleId = this.serviceArticle;
                    values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].IsVatApply = values[j].IsVatApply ? true : false;
                    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                    this.totalValue();
                  }
                  // if (values[j].OrderHeadId == 0) {
                  //   values[j].ArticleId = this.serviceArticle;
                  //   values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                  //   values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                  //   values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                  //   values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                  //   values[j].IsVatApply = values[j].IsVatApply ? true : false;
                  //   this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                  //   this.totalValue();
                  // }
                }

              }
            })


          }

        }
      }
    }
  }
  onFromTimeChange(newVal: any): void {
    if (newVal) {
      if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToDate").value) {
        if (this.bookingForm.get("ToDate").value < newVal) {

          this.bookingForm.get("FromDate").setErrors({ 'incorrect': true });

        }
        else {
          this.bookingForm.get("ToDate").setErrors(null);
        }

      }
      if (this.bookingForm.get("MeetingRoomId").value) {
        if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
          if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
            this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
              if (this.bookingForm.get("CompanyPayer").value) {
                let isIntCustomer = this.bookingForm.get("IsInternal").value;
                const isMVACustomer = this.bookingForm.get('IsMVA').value;
                let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                if (isIntCustomer) {
                  this.servicePrice = data.UnitPrice;
                  this.serviceCostPrice = data.CostPrice1;

                }
                else {
                  this.servicePrice = data.SecondaryPrice;
                  this.serviceCostPrice = data.CostPrice2;
                }
                if (isMVACustomer) {
                  this.serviceArticle = data.Article1;
                }
                else {
                  this.serviceArticle = data.Article2;
                }
              }
              else {
                this.serviceArticle = data.Article2;
                this.servicePrice = data.SecondaryPrice;
                this.serviceCostPrice = data.CostPrice2;
              }

              const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
              let values = servicesValue.value;
              for (let j = 0; j < values.length; j++) {
                if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
                  if (values[j].OrderHeadId == 0) {
                    values[j].ArticleId = this.serviceArticle;
                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[j].IsVatApply = values[j].IsVatApply ? true : false;
                    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                    this.totalValue();
                  }
                }

              }
            })


          }

        }
      }
    }
  }

  clearFormArray = (formArray: FormArray) => {
    while (formArray.length !== 0) {
      formArray.removeAt(0)
    }
  }
  onServiceChanges = (): void => {

    this.bookingForm.get('PropertyServiceId').valueChanges.subscribe(val => {
      if (val) {
        this._bookingService.getMeetingRoomList(val).subscribe(data => {
          this.meetingRooms = data;
          this.subServiceData.length = 0;
          for (var i = 0; i < data.length; i++) {
            if (this.subServiceData.indexOf(data[i].meta_keywords) === -1) {
              this.subServiceData.push(data[i].meta_keywords);
            }
          }
          if (this.mode == 'add') {
            if (val == 6191 || val == 6192) {
              this.bookingForm.patchValue({
                ToDate: '31.12.2099',
                FromTimer: '08:00',
                ToTimer: '08:00',
                MeetingRoomId: this.meetingRooms[0].MainID
              });
            }
            else {
              this.bookingForm.patchValue({
                ToDate: moment(new Date()).format("DD.MM.YYYY"),
                FromTimer: AppSettings.InitTime,
                ToTimer: AppSettings.InitTime,
                MeetingRoomId: this.meetingRooms[0].MainID
              });
            }
            // this.bookingForm.get('MeetingRoomId').setValue(this.meetingRooms[0].MainID);
          }


          //end
        });
      }



    });
  }
  onServicesvalChanges = () => {
    this.bookingForm.get("MeetingRoomId").valueChanges.subscribe(val => {
      if (val != 0) {
        this.getServiceArticleByMeetingRoomId();
        const prvalue = this.bookingForm.get('PropertyServiceId').value;

        this._bookingService.getMeetingRoomList(prvalue).subscribe(data => {
          this.meetingRooms = data;

          if (val != null) {
            this._bookingService.getAddtionalServiceList(val, AppSettings.serviceFormId).subscribe(data => {
              this.additionalService.length = 0;
              this.additionalService = data;


              if (this.bookingForm.get("bookingId").value == 0) {
                this.bookingForm.get('IsFoodOrder').setValue(false);
                const control = <FormArray>this.bookingForm.controls['Foods'];

                // for (let i = control.length - 1; i >= 0; i--) {
                //     control.removeAt(i);
                // }
                // this.addFood();
                const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
                let values = servicesValue.value;

                let service = _.find(this.meetingRooms, x => x.MainID == val);

                if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
                  // this._bookingService.getServiceVatArticles(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {
                  // this.serviceArticleVal = articleData;
                  // let valofarticle = 0;
                  // if (articleData.Article1) {
                  //   valofarticle = articleData.Article1.toString();
                  // }

                  if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
                    this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                      this.serviceArticlePrice = data;
                      if (this.bookingForm.get("CompanyPayer").value) {
                        let isIntCustomer = this.bookingForm.get("IsInternal").value;
                        const isMVACustomer = this.bookingForm.get('IsMVA').value;
                        let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                        if (isIntCustomer) {

                          this.servicePrice = data.UnitPrice;
                          this.serviceCostPrice = data.CostPrice1;
                        }
                        else {
                          this.servicePrice = data.SecondaryPrice;
                          this.serviceCostPrice = data.CostPrice2;
                        }
                        if (isMVACustomer) {
                          this.serviceArticle = data.Article1;
                        }
                        else {
                          if (data.IsMutiplePrice) {
                            this.servicePrice = this.servicePrice * AppSettings.mvaPrice;
                          }
                          this.serviceArticle = data.Article2;
                        }
                      }
                      else {

                        if (data.IsMutiplePrice) {
                          this.servicePrice = data.SecondaryPrice * AppSettings.mvaPrice;
                        }
                        else {
                          this.servicePrice = data.SecondaryPrice;
                        }

                        this.serviceCostPrice = data.CostPrice2;
                        this.serviceArticle = data.Article2;
                      }

                      values[0].Price = this.setTwoNumberDecimal(this.servicePrice);
                      if (data.IsMutiplePrice) {
                        values[0].Qty = data.Quantity;
                        values[0].Sum = this.setTwoNumberDecimal(this.servicePrice * data.Quantity);
                      }
                      else {
                        values[0].Qty = 1;
                        values[0].Sum = this.setTwoNumberDecimal(this.servicePrice);
                      }

                      values[0].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                      values[0].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                      values[0].MainServiceId = val;
                      values[0].ArticleId = this.serviceArticle;
                      //  values[0].ArticleId = valofarticle;
                      values[0].Tekst = this.getHeadLineValue(service.Headline);

                      values[0].ServiceText = service.Headline;
                      // if (data.IsMutiplePrice) {
                      //   values[0].Qty = 1 * this.bookingForm.get('NoOfPeople').value;
                      // }
                      // else {
                      //   values[0].Qty = 1;
                      // }

                      values[0].IsVatApply = true;
                      values[0].IsKitchenOrder = false;
                      // if(values[0].IsVatApply){
                      //     values[0].ArticleId=articleData.Article1;
                      // }
                      // else{
                      //     values[0].ArticleId=articleData.Article2;
                      // }

                      this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                      this.totalValue();
                    });
                  }
                  else {
                    // if(values.length>0){
                    values[0].ArticleId = this.serviceArticle;
                    values[0].Tekst = this.getHeadLineValue(service.Headline);
                    values[0].ServiceText = service.Headline;
                    values[0].Qty = 1;
                    values[0].MainServiceId = val;
                    values[0].Price = this.setTwoNumberDecimal(this.servicePrice);
                    values[0].Sum = this.setTwoNumberDecimal(this.servicePrice);
                    values[0].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[0].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                    values[0].IsVatApply = true;
                    values[0].IsKitchenOrder = false;
                    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                    // }

                    // if(values[0].IsVatApply){
                    //     values[0].ArticleId=articleData.Article1;
                    // }
                    // else{
                    //     values[0].ArticleId=articleData.Article2;
                    // }


                  }
                  // })

                }

                //  for(let j=0;j<values.length;j++){



                // }
                //   this.addFood();
                this.totalValue();
              }
              else {
                var aa = this.bookingForm.get("MeetingRoomId").value;

                if (this.updatedEventService != val) {
                  this.bookingForm.get('IsFoodOrder').setValue(false);
                  const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
                  let values = servicesValue.value;
                  if (values[0].OrderHeadId == 0) {
                    let service = _.find(this.meetingRooms, x => x.MainID == val);
                    if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
                      // this._bookingService.getServiceVatArticles(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {

                      // let valofarticle = 0;
                      // if (articleData.Article1) {
                      //   valofarticle = articleData.Article1.toString();
                      // }

                      if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
                        this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                          if (this.bookingForm.get("CompanyPayer").value) {
                            let isIntCustomer = this.bookingForm.get("IsInternal").value;
                            const isMVACustomer = this.bookingForm.get('IsMVA').value;
                            let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
                            if (isIntCustomer) {
                              this.servicePrice = data.UnitPrice;
                              this.serviceCostPrice = data.CostPrice1;
                            }
                            else {
                              this.servicePrice = data.SecondaryPrice;
                              this.serviceCostPrice = data.CostPrice2;
                            }
                            if (isMVACustomer) {
                              this.serviceArticle = data.Article1;
                            }
                            else {
                              this.serviceArticle = data.Article2;
                            }
                          }
                          else {
                            this.serviceArticle = data.Article2;
                            this.servicePrice = data.SecondaryPrice;
                            this.serviceCostPrice = data.CostPrice2;
                          }

                          values[0].Price = this.setTwoNumberDecimal(this.servicePrice);
                          values[0].Sum = this.setTwoNumberDecimal(this.servicePrice);
                          values[0].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                          values[0].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                          values[0].MainServiceId = val;
                          values[0].ArticleId = this.serviceArticle;
                          values[0].Tekst = this.getHeadLineValue(service.Headline);
                          values[0].ServiceText = service.Headline;
                          values[0].Qty = 1;
                          values[0].IsVatApply = true;
                          values[0].IsKitchenOrder = false;
                          // if(values[0].IsVatApply){
                          //     values[0].ArticleId=articleData.Article1;
                          // }
                          // else{
                          //     values[0].ArticleId=articleData.Article2;
                          // }

                          this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                          this.totalValue();
                        });
                      }
                      else {
                        values[0].ArticleId = this.serviceArticle;
                        values[0].Tekst = this.getHeadLineValue(service.Headline);
                        values[0].ServiceText = service.Headline;
                        values[0].Qty = 1;
                        values[0].MainServiceId = val;
                        values[0].Price = this.setTwoNumberDecimal(this.servicePrice);
                        values[0].Sum = this.setTwoNumberDecimal(this.servicePrice);
                        values[0].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                        values[0].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                        values[0].IsVatApply = true;
                        values[0].IsKitchenOrder = false;
                        // if(values[0].IsVatApply){
                        //     values[0].ArticleId=articleData.Article1;
                        // }
                        // else{
                        //     values[0].ArticleId=articleData.Article2;
                        // }
                        this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });

                      }
                      // })

                    }
                    this.updatedEventService = val;
                    // this.addFood();
                    this.totalValue();
                  }

                }
              }
            });


          }
        })
      }




    });
  }

  save = (): void => {
    var inputData = this.bookingForm.value;
    inputData["MessageList"] = this.bookingMessageList;
    inputData["IsBooking"] = true;
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
  // onVatChanges(newval: any): void {

  //   this._bookingService.getServiceVatArticles(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {
  //     let val = "";
  //     if (newval == true) {
  //       val = articleData.Article1.toString();
  //     }
  //     if (newval == false) {
  //       val = articleData.Article2.toString();
  //     }
  //     const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
  //     let values = servicesValue.value;
  //     values[0].ArticleId = val;

  //     //  values[0].IsVatApply=values[0].IsVatApply?true:false;


  //     this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
  //   });
  // }

  // onAddvalChanges(newval: any, indexval: any): void {
  //   if (newval != 0) {

  //     const control = <FormArray>this.bookingForm.controls['Foods'];
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
  //                 if (this.bookingForm.get("CompanyPayer").value) {
  //                   let isIntCustomer = this.bookingForm.get("IsInternal").value;
  //                   const isMVACustomer = this.bookingForm.get('IsMVA').value;
  //                   let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
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
  //                   if (isMVACustomer) {
  //                     control.value[i].ArticleId = addedsubService.ArticleId;
  //                   }
  //                   else {
  //                     control.value[i].ArticleId = addedsubService.ArticleId2;
  //                   }
  //                 }
  //                 else {

  //                   control.value[i].Price = this.setTwoNumberDecimal(addedsubService.SecondaryPrice);
  //                   control.value[i].ArticleId = addedsubService.ArticleId2;
  //                   control.value[i].Sum = this.setTwoNumberDecimal(addedsubService.SecondaryPrice * control.value[i].Qty);
  //                   control.value[i].CostPrice = this.setTwoNumberDecimal(addedsubService.CostPrice2);
  //                   control.value[i].CostTotal = this.setTwoNumberDecimal(addedsubService.CostPrice2 * control.value[i].Qty);


  //                 }
  //                 // control.value[i].Price = addedsubService.Price;
  //                 // control.value[i].Sum = addedsubService.Price * control.value[i].Qty;
  //                 control.value[i].Tekst = addedsubService.SName;
  //                 // control.value[i].ArticleId = addedsubService.ArticleId;
  //                 control.value[i].MainServiceId = addedsubService.SId;
  //                 control.value[i].IsKitchenOrder = control.value[i].IsKitchenOrder ? true : false;
  //                 //}


  //                 this.bookingForm.get("Foods").patchValue(control.value, { onlySelf: true, emitEvent: false });
  //               }
  //             }


  //             // this.bookingForm.get("Foods").updateValueAndValidity();
  //             this.totalValue();
  //           }


  //         }
  //       })

  //     }
  //   }
  // }
  onAddvalChanges(newval: any, indexval: any): void {
    var arrayControl = this.bookingForm.get('Foods') as FormArray;

    if (newval != 0) {
      const addedsubService = _(this.additionalService)
        .thru(function (coll) {
          return _.union(coll, _.map(coll, 'ServiceList'));
        })
        .flatten().find(x => x.SId == Number(newval.target.value));


      const Qty = arrayControl.at(indexval).get('Qty').value;

      if (addedsubService) {
        if (this.bookingForm.get("CompanyPayer").value) {
          let isIntCustomer = this.bookingForm.get("IsInternal").value;
          const isMVACustomer = this.bookingForm.get('IsMVA').value;
          let company = _.find(this.companyList, x => x.value == this.bookingForm.get("CompanyPayer").value);
          if (isIntCustomer) {
            arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.Price));
            arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.Price * Qty));
            arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice1));
            arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice1 * Qty));

          }
          else {
            arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice));
            arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice * Qty));
            arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2));
            arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2 * Qty));



          }
          if (isMVACustomer) {
            arrayControl.at(indexval).get('ArticleId').setValue(addedsubService.ArticleId);

          }
          else {
            arrayControl.at(indexval).get('ArticleId').setValue(addedsubService.ArticleId2);

          }
        }
        else {

          arrayControl.at(indexval).get('Price').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice));
          arrayControl.at(indexval).get('ArticleId').setValue(addedsubService.ArticleId2);
          arrayControl.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(addedsubService.SecondaryPrice * Qty));
          arrayControl.at(indexval).get('CostPrice').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2));
          arrayControl.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(addedsubService.CostPrice2 * Qty));




        }


        arrayControl.at(indexval).get('Tekst').setValue(addedsubService.SName);
        arrayControl.at(indexval).get('MainServiceId').setValue(addedsubService.SId);
        arrayControl.at(indexval).get('IsKitchenOrder').setValue(addedsubService.isKitchen);
        //  let IsKitchenOrder = arrayControl.at(indexval).get('IsKitchenOrder').value;

        // arrayControl.at(indexval).get('IsKitchenOrder').setValue(IsKitchenOrder ? true : false);


      }

      this.totalValue();

    }
  }
  OnPriceChange(newval: any, indexval: any): void {

    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(indexval).get('Price').setValue(newval.target.value);

    formsval.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('Qty').value * newval.target.value));

    this.totalValue();


  }

  OnCostPriceChange(newval: any, indexval: any): void {
    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(indexval).get('CostPrice').setValue(newval.target.value);


    formsval.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('Qty').value * newval.target.value));
    this.totalValue();
  }
  OnQtyChange(newval: any, indexval: any): void {

    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(indexval).get('Qty').setValue(newval.target.value);

    formsval.at(indexval).get('Sum').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('Price').value * newval.target.value));
    formsval.at(indexval).get('CostTotal').setValue(this.setTwoNumberDecimal(formsval.at(indexval).get('CostPrice').value * newval.target.value));
    this.totalValue();

  }
  OnArticleChange(newVal: any, index: any): void {
    const formVals = this.bookingForm.get('Foods') as FormArray;
    const addedval= formVals.at(index).value;
    addedval['index']=index;
    if (newVal.target.value == 0 || !newVal.target.value) {
      this.setConfirmDialogValue('Skal priser oppdateres?', 'ArticleChange', 'Når det ikke er et artikkelnummer kan det ikke belastes kunden, vil du fjerne antall og beløp', 'Ja', addedval)
      this._confirmService.toggle();
    }
    

  }
  // OnPriceQtyChange(): void {
  //   this.bookingForm.get("Foods").valueChanges.subscribe(val => {
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
  //                     this.bookingForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
  //                     this.totalValue();
  //                   }
  //                 }
  //                 else {
  //                   val[i].Sum = this.setTwoNumberDecimal(val[i].Price * val[i].Qty);
  //                   val[i].CostTotal = this.setTwoNumberDecimal(val[i].CostPrice * val[i].Qty);
  //                   this.bookingForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
  //                   this.totalValue();
  //                 }

  //               }
  //               else {

  //                 // this.isRead=false;
  //                 //  var arrayControl = this.bookingForm.get('Foods') as FormArray;
  //                 //  arrayControl.at(i).get("Qty").disable();
  //                 // var b=arrayControl.at(i);
  //                 //   val[i].Sum = val[i].Price * val[i].Qty;
  //                 //  this.bookingForm.get("Foods").patchValue(arrayControl, {onlySelf:true,emitEvent: false});
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
  //             this.bookingForm.get("Foods").patchValue(val, { onlySelf: true, emitEvent: false });
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
    const control = <FormArray>this.bookingForm.controls['Foods']
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
    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(index).get('Price').setValue(this.setTwoNumberDecimal(event.target.value));
  }
  focusCostPriceFunction(event: any, index: any): void {
    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(index).get('CostPrice').setValue(this.setTwoNumberDecimal(event.target.value));
  }
  focusPriceSumFunction(event: any, index: any): void {
    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(index).get('Sum').setValue(this.setTwoNumberDecimal(event.target.value));
  }
  focusCostPriceSumFunction(event: any, index: any): void {
    let formsval = this.bookingForm.get('Foods') as FormArray;
    formsval.at(index).get('CostTotal').setValue(this.setTwoNumberDecimal(event.target.value));
  }
  buildFood(): FormGroup {
    let ikitchen = false;
    if (this.bookingForm) {
      if (this.bookingForm.get('MeetingRoomId').value == 0 && this.bookingForm.get('bookingId').value != 0) {
        ikitchen = false;
        this.isbuildfoodValuesShow = true;
      }
    }

    return this.fb.group({
      FoodId: 0,
      Qty: 1,
      ArticleId: '',

      MainServiceId: '',
      IsKitchenOrder: ikitchen,
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

    let building = _.find(this.properties, x => x.MenuId == this.bookingForm.get("PropertyId").value);
    return building.MenuName + " " + headline + " " + this.bookingForm.get("FromDate").value + "," + this.bookingForm.get("NoOfPeople").value + " " + "personer";
  }
  GetFoodService(data: Array<any>): void {
    let opts = new Array();
    for (let i = 0; i < data.length; i++) {
      if (data[i].MenuName == 'Tilleggstjenester') {
        this._bookingService.getFoodList(data[i].MenuId, AppSettings.FoodFormId).subscribe(data => this.foodList = data);
      }

    }

  }

  ConfirmMVAChange = (value: boolean) => {
    if (value) {
      const isMVAInit = this.bookingForm.get('IsMVA').value;
      const isInt = this.bookingForm.get('IsInternal').value;
      const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
      let values = servicesValue.value;
      let aPrice = 0;
      for (let j = 0; j < values.length; j++) {

        if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
          if (values[j].OrderHeadId == 0) {
            if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
              if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
                this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                  if (isInt) {
                    aPrice = data.UnitPrice;
                  }
                  else {
                    aPrice = data.SecondaryPrice;
                  }
                  if (isMVAInit) {
                    this.serviceArticle = data.Article1;
                    if (data.IsMutiplePrice) {


                      const pval = values[j].Price / AppSettings.mvaPrice;
                      values[j].Price = this.setTwoNumberDecimal(pval);
                      values[j].Sum = this.setTwoNumberDecimal(pval * values[j].Qty);
                    }


                  }
                  else {
                    this.serviceArticle = data.Article2;
                    if (data.IsMutiplePrice) {

                      const pval = values[j].Price * AppSettings.mvaPrice;
                      values[j].Price = this.setTwoNumberDecimal(pval);
                      values[j].Sum = this.setTwoNumberDecimal(pval * values[j].Qty);

                    }


                  }
                  values[j].ArticleId = this.serviceArticle;


                  this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                  this.totalValue();
                });
              }
            }
          }


        }
        else {
          this.additionalService.forEach((item, index) => {

            this.addedServices = item.ServiceList;
            this.addedServices.forEach((ix, ind) => {
              if (ix.SId == values[j].FoodId) {
                if (values[j].OrderHeadId == 0) {
                  if (isMVAInit) {
                    //savan
                    values[j].ArticleId = ix.ArticleId;

                  }
                  else {
                    //savan
                    values[j].ArticleId = ix.ArticleId2;

                  }
                  // values[j].Tekst = ix.SName;

                  // values[j].MainServiceId = ix.SId;

                  // values[j].IsKitchenOrder = values[j].IsKitchenOrder ? true : false;
                  //  values[j].IsVatApply=values[j].IsVatApply?true:false;
                }



              }

            });


          })
        }

      }
      this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
      this._confirmService.toggle();
    }
    else {
      this.bookingForm.get('IsMVA').setValue(!this.bookingForm.get('IsMVA').value, { onlySelf: true, emitEvent: false });
    }
  }
  ConfirmChangeCommand = (value: boolean) => {
    if (value) {
      let isIntCustomer = this.bookingForm.get("IsInternal").value;
      const isMvaCust = this.bookingForm.get('IsMVA').value;
      const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
      let values = servicesValue.value;
      for (let j = 0; j < values.length; j++) {

        if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
          if (values[j].OrderHeadId == 0) {
            if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
              if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
                this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                  if (isIntCustomer) {
                    this.servicePrice = data.UnitPrice;
                    this.serviceCostPrice = data.CostPrice1;

                  }
                  else {
                    this.servicePrice = data.SecondaryPrice;
                    this.serviceCostPrice = data.CostPrice2;

                  }

                  if (data.IsMutiplePrice) {
                    values[j].Qty = data.Quantity;
                    if (!isMvaCust) {
                      this.servicePrice = this.servicePrice * AppSettings.mvaPrice;
                    }


                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].Sum = this.setTwoNumberDecimal(this.servicePrice * values[j].Qty);
                  }
                  else {
                    values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                    values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                  }

                  values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                  values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                  values[j].IsVatApply = values[j].IsVatApply ? true : false;
                  this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                  this.totalValue();
                });
              }
            }
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

      }
      this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
      this.totalValue();
      this._confirmService.toggle();
    }
    else {
      this.bookingForm.get('IsInternal').setValue(!this.bookingForm.get("IsInternal").value, { onlySelf: true, emitEvent: false });
      // this._confirmService.toggle();
    }
  }

  changeMvaCommand = () => {
    const isMVAInit = this.bookingForm.get('IsMVA').value;
    const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
    let values = servicesValue.value;
    for (let j = 0; j < values.length; j++) {

      if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
        if (values[j].OrderHeadId == 0) {
          if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
            if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
              this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                if (isMVAInit) {
                  this.serviceArticle = data.Article1;


                }
                else {
                  this.serviceArticle = data.Article2;

                }
                values[j].ArticleId = this.serviceArticle;

                this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });

              });
            }
          }
        }


      }
      else {
        this.additionalService.forEach((item, index) => {

          this.addedServices = item.ServiceList;
          this.addedServices.forEach((ix, ind) => {
            if (ix.SId == values[j].FoodId) {
              if (values[j].OrderHeadId == 0) {
                if (isMVAInit) {
                  //savan
                  values[j].ArticleId = ix.ArticleId;

                }
                else {
                  //savan
                  values[j].ArticleId = ix.ArticleId2;

                }
                // values[j].Tekst = ix.SName;

                // values[j].MainServiceId = ix.SId;

                // values[j].IsKitchenOrder = values[j].IsKitchenOrder ? true : false;
                //  values[j].IsVatApply=values[j].IsVatApply?true:false;
              }



            }

          });


        })
      }

    }
    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });

  }
  ChangePriceCommand = () => {
    let isIntCustomer = this.bookingForm.get("IsInternal").value;
    const isMVAInit = this.bookingForm.get('IsMVA').value;
    const servicesValue = <FormArray>this.bookingForm.controls['Foods'];
    let values = servicesValue.value;
    for (let j = 0; j < values.length; j++) {

      if (values[j].MainServiceId == this.bookingForm.get("MeetingRoomId").value) {
        if (values[j].OrderHeadId == 0) {
          if (this.bookingForm.get("FromDate").value && this.bookingForm.get("ToTimer").value && this.bookingForm.get("FromTimer").value && this.bookingForm.get("ToDate").value) {
            if (this.bookingForm.get("ToTimer").value > this.bookingForm.get("FromTimer").value) {
              this._bookingService.getBookingPriceDetail(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId, this.bookingForm.get("FromDate").value, this.bookingForm.get("ToDate").value, this.bookingForm.get("FromTimer").value, this.bookingForm.get("ToTimer").value, this.bookingForm.get("IsInternal").value, this.bookingForm.get("NoOfPeople").value).subscribe(data => {
                if (isIntCustomer) {
                  this.servicePrice = data.UnitPrice;
                  this.serviceCostPrice = data.CostPrice1;

                }
                else {
                  this.servicePrice = data.SecondaryPrice;
                  this.serviceCostPrice = data.CostPrice2;

                }
                if (isMVAInit) {
                  this.serviceArticle = data.Article1;


                }
                else {

                  this.serviceArticle = data.Article2;
                  if (data.IsMutiplePrice) {
                    this.servicePrice = this.servicePrice * AppSettings.mvaPrice;
                  }

                }
                if (data.IsMutiplePrice) {

                  values[j].Qty = data.Quantity;

                  values[j].Sum = this.setTwoNumberDecimal(this.servicePrice * data.Quantity);
                }
                else {

                  values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                }
                values[j].ArticleId = this.serviceArticle;
                values[j].Price = this.setTwoNumberDecimal(this.servicePrice);
                // values[j].Sum = this.setTwoNumberDecimal(this.servicePrice);
                values[j].CostPrice = this.setTwoNumberDecimal(this.serviceCostPrice);
                values[j].CostTotal = this.setTwoNumberDecimal(this.serviceCostPrice);
                values[j].IsVatApply = values[j].IsVatApply ? true : false;
                this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
                this.totalValue();
              });
            }
          }
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
                if (isMVAInit) {
                  //savan
                  values[j].ArticleId = ix.ArticleId;

                }
                else {
                  //savan
                  values[j].ArticleId = ix.ArticleId2;

                }
                values[j].Tekst = ix.SName;
                // values[j].ArticleId =ix.SId;
                //  values[j].ArticleId = ix.ArticleId;
                values[j].MainServiceId = ix.SId;

                values[j].IsKitchenOrder = values[j].IsKitchenOrder ? true : false;
                //  values[j].IsVatApply=values[j].IsVatApply?true:false;
              }



            }

          });


        })
      }

    }
    this.bookingForm.get("Foods").patchValue(values, { onlySelf: true, emitEvent: false });
    this.totalValue();

  }
  ConfirmCommand(value: any): void {

    if (value.type === 'SlettBooking') {
      this.Confirmdelete(value.val, value.isDelcal);
    }
    if (value.type === 'Endre') {
      this.ConfirmChangeCommand(value.val);
    }
    if (value.type === 'MVA') {
      this.ConfirmMVAChange(value.val);
    }
    if (value.type === 'FoodDelete') {
      this.FoodDelete(value.val, value.data);
    }
    if(value.type==='ArticleChange'){
       this.ArticleChange(value.val,value.data);
    }
    if (value.type === 'RemoveOrderline') {
      this.OrderLineConfirmDelete(value.val, value.data);
    }

  }

  OrderLineConfirmDelete = (value: boolean, data: any) => {
    if (!value) {
      const control = <FormArray>this.bookingForm.controls['Foods'];

      const result = control.at(data);
      result.value.IsKitchenOrder = true;
      this.bookingForm.get("Foods").patchValue(control.value, { onlySelf: true, emitEvent: false });

    }
    else {
      this._confirmService.toggle();
    }
  }
  Confirmdelete = (value: boolean, isdelcal: boolean) => {
    if (value) {
      this._bookingService.DeleteBookingById(this.deletedbookingId, isdelcal).subscribe(data => {


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

      this._bookingService.getUserListbyCustomer(this.bookingForm.get('CompanyPayer').value).subscribe(res => {
        this.getUserList(res);
        this.userOpen = false;
      }, (err) => console.error(err), () => {

        setTimeout(() => {

          this.bookingForm.get('UserID').patchValue(data.toString())
        }, 1000);


      });



    }
  }
  SavedCustomer = (data: any) => {
    if (data) {
      this._toastService.showSuccess('Customer added successfully', 'Success!');

      this._bookingService.getCutomerList().subscribe(resData => {

        this.getCompanyList(resData);
        this.bookingForm.get('CompanyPayer').patchValue(data.toString());

        this.customerOpen = false;
      }, (err) => console.error(err), () => { setTimeout(() => { console.log(this.companyList); this.bookingForm.get('CompanyPayer').patchValue(data.toString()) }, 3000); });
    }
  }

  updateMode(mode: string) {
    this.mode = mode;
    if (this.mode == 'add') {
      this.dialogTitle = "Legg til booking";
      this.buttonTitle = 'Lagre';

      this.modeConfig = AppSettings.DefaultMode;
    }
    else if (this.mode == 'selectAdd') {
      this.dialogTitle = "Legg til booking";
      this.buttonTitle = 'Lagre';
      this.modeConfig = AppSettings.DefaultMode;
    }
    else if (this.mode == 'edit') {
      this.dialogTitle = "Oppdater booking (" + this.parameters.bookingId + ")";
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
    this.bookingForm.patchValue({
      bookingId: event.bookingId,
      UserID: event.UserID,
      PropertyId: event.PropertyId,
      PropertyServiceId: event.PropertyServiceId,
      MeetingRoomId: event.MeetingRoomId,
      CompanyPayer: event.CompanyPayer,
      nameOfbook: event.nameOfbook,
      BookOrderName: event.BookOrderName,

      NoOfPeople: event.NoOfPeople,

      FromDate: event.FromDate,
      ToDate: event.ToDate,
      FromTimer: event.FromTimer,
      ToTimer: event.ToTimer,
      FollowDate: event.FollowDate,
      IsFoodOrder: event.IsFoodOrder,

      SendMessageType: event.SendMessageType,
      Note: event.Note,
      InvoMessage: event.InvoMessage

    });
    //this.bookingForm.get('PropertyServiceId').setValue(event.PropertyServiceId, { onlySelf: true, emitEvent: false });
    //this.bookingForm.get('MeetingRoomId').setValue(event.MeetingRoomId, { onlySelf: true, emitEvent: false });

    this.bookingForm.get('IsMVA').setValue(event.IsMVA, { onlySelf: true, emitEvent: false });
    this.bookingForm.get('IsInternal').setValue(event.IsInternal, { onlySelf: true, emitEvent: false });
  }

  ResetCustomerForm = () => {
    this.searchedCustomerNo = '';
    this.displayCustomerWarning = '';
    this.customerData = '';
  }
  ArticleChange=(value:Boolean,data:any)=>{

    

    const formVals = this.bookingForm.get('Foods') as FormArray;
   
    if(value){
      formVals.at(data.index).get('Qty').setValue(0);
      formVals.at(data.index).get('Price').setValue(this.setTwoNumberDecimal(0));
      formVals.at(data.index).get('Sum').setValue(this.setTwoNumberDecimal(0));
      formVals.at(data.index).get('CostPrice').setValue(this.setTwoNumberDecimal(0));
      formVals.at(data.index).get('CostTotal').setValue(this.setTwoNumberDecimal(0));
      this._confirmService.toggle();
    }
    else{

      let currentService = _(this.additionalService)
      .thru(function (coll) {
        return _.union(coll, _.map(coll, 'ServiceList'));
      })
      .flatten().find(x => x.SId == Number(formVals.at(data.index).get('FoodId').value));
      formVals.at(data.index).get('Qty').setValue(1);
      if (this.bookingForm.get('IsMVA').value) {
        formVals.at(data.index).get('ArticleId').setValue(currentService.ArticleId);

      }
      else {
        formVals.at(data.index).get('ArticleId').setValue(currentService.ArticleId2);

      }
      if (this.bookingForm.get("IsInternal").value) {
        formVals.at(data.index).get('Price').setValue(this.setTwoNumberDecimal(currentService.Price));
        formVals.at(data.index).get('Sum').setValue(this.setTwoNumberDecimal(currentService.Price * 1));
        formVals.at(data.index).get('CostPrice').setValue(this.setTwoNumberDecimal(currentService.CostPrice1));
        formVals.at(data.index).get('CostTotal').setValue(this.setTwoNumberDecimal(currentService.CostPrice1 * 1));

      }
      else {
        formVals.at(data.index).get('Price').setValue(this.setTwoNumberDecimal(currentService.SecondaryPrice));
        formVals.at(data.index).get('Sum').setValue(this.setTwoNumberDecimal(currentService.SecondaryPrice * 1));
        formVals.at(data.index).get('CostPrice').setValue(this.setTwoNumberDecimal(currentService.CostPrice2));
        formVals.at(data.index).get('CostTotal').setValue(this.setTwoNumberDecimal(currentService.CostPrice2 * 1));



      }
     
     
    
    }
    this.totalValue();
  }
  FoodDelete = (value: boolean, data: any) => {
    if (value) {
      const control = <FormArray>this.bookingForm.controls['Foods'];
      // remove the chosen row
      control.removeAt(data);
      this.totalValue();
      this._confirmService.toggle();
    }
    else {
      //  this._confirmService.toggle();
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
  repeatstr = (ch: any, n: any): any => {
    var result = "";

    while (n-- > 0)
      result += ch;

    return result;
  }
  onIsKitchenChange = (data: any): void => {
    console.log(data.srcElement.attributes.id.value.substring(9));
    var currentID = data.srcElement.attributes.id.value.substring(9);
    const control = <FormArray>this.bookingForm.controls['Foods'];
    var result = control.at(currentID);
    console.log(result);
    if (this.mode === 'orderlineEdit') {
      this.setConfirmDialogValue(' Slett', 'RemoveOrderline', 'Er du sikker på at du vil slette?', 'Slett', currentID)
      //   this.setConfirmDialogValue(' Slett?','RemoveOrderline','Er du sikker på at du vil slette?',null)
      this._confirmService.toggle();
    }

  }
  getServiceArticleByMeetingRoomId = (): void => {

    this._bookingService.getServiceVatArticles(this.bookingForm.get("MeetingRoomId").value, AppSettings.BookingPriceDetailFormId).subscribe(articleData => {
      this.serviceArticleVal = articleData;

    }, (err) => console.error(err));

  }
}
