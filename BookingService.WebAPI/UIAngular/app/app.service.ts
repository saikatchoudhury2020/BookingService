import { Injectable } from "@angular/core";
import { AppSettings } from "./appConfig";
import { Observable } from "rxjs";
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError, shareReplay } from 'rxjs/operators';
const CACHE_SIZE = 1;
@Injectable()
export class AppService {
  private actionGetUrl: string;
  private baseUrl: string = AppSettings.API;
  private cache$: Observable<any>;
  private token: string;
  private username: string;
  private password: string;

  constructor(private _http: Http) { }



  public send = (request: string, parameters: any = {}, method: string = 'get'): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/' + request;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders(), params: parameters })
      .pipe(map(this.extractData), catchError(this.handleError)); //todo: invoke a call back for handling error(eg. show message in ui based on different error message)
  }

  public getBookingMasterData = (rootId: number, propertyId: number, orgId: number, propertyServiceId: string): Observable<any> => {
    if (!this.cache$) {
      this.cache$ = this.GetBookingsDetail(rootId, propertyId, orgId, propertyServiceId).pipe(
        shareReplay(CACHE_SIZE)
      );
    }

    return this.cache$;

  }
  public getProperties = (rootId: number): Observable<any> => {

    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/menuitems/getmenu?menuItemId=' + rootId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public getMeetingRoomList = (menuId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetAllArticles?menuItemId=' + menuId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public getCurrentBookingDetail = (bookingId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/GetCurrentBookingDetail?bookingId=' + bookingId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));

  }
  public getPropertyServices = (menuId: number): Observable<any> => {


    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/menuitems/getmenu?menuItemId=' + menuId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }


  public getServices = (menuId: number): Observable<any> => {


    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/menuitems/getmenu?menuItemId=' + 4121;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
  public getAddtionalServiceList = (articleId: number, formId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/AddOnServiceList?articleId=' + articleId + "&formId=" + formId + "&menuid=" + AppSettings.RootPropertiesId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  //public getFoodList = (menuId: number): Observable<any> => {

  //    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetAllArticles?menuItemId=' + menuId;
  //    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
  //        .map(this.extractData).catch(this.handleError);
  //}

  public getFoodList = (menuId: number, formId: number): Observable<any> => {

    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/FoodServiceBuldingWise?menuId=' + menuId + '&FormId=' + formId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() }).pipe(
      map(this.extractData), catchError(this.handleError));
  }


  public getUserList = (orgId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Account/GetPersonList?OrgUnitId=' + orgId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public getUserListbyCustomer = (custId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Account/GetPersonListByCustomer?customerNo=' + custId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
  public GetBookingDetail = (): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetBookingData?menuId=' + AppSettings.RootPropertiesId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }




  public Book(bookingmodel: any) {

    return this._http.post(this.baseUrl + '/digimakerwebapi/api/Booking/Post', bookingmodel, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public OrderBook(ordermodel: any) {

    return this._http.post(this.baseUrl + '/digimakerwebapi/api/Booking/SaveOrderline', ordermodel, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public CreateNewUser(userModel: any) {

    return this._http.post(this.baseUrl + '/digimakerwebapi/api/Account/CreateNewUser', userModel, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public DeleteBookingById = (bookingId: number, isdelCal: boolean): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/DeleteBooking?bookingId=' + bookingId + '&isDeleteCalender=' + isdelCal;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));

  }
  public getBookingExternalPriceDetail = (isInternal: boolean, NoOfPerson: number, fromDate: string, toDate: string, fTimer: string, tTimer: string): Observable<any> => {


    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/getBookingExternalPriceDetail?isInternal=' + isInternal + '&NoOfPerson=' + NoOfPerson + '&fromDate=' + fromDate + '&toDate=' + toDate + '&fTimer=' + fTimer + '&tTimer=' + tTimer;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
  public getBookingPriceDetail = (serviceId: number, formId: number, fromDate: string, toDate: string, fTimer: string, tTimer: string, isInternal: boolean, NoOfPerson: number): Observable<any> => {
    if (!NoOfPerson) {
      NoOfPerson = 1;
    }
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/getBookingPriceDetail?articleId=' + serviceId + '&FormID=' + formId + '&fromDate=' + fromDate + '&toDate=' + toDate + '&fTimer=' + fTimer + '&tTimer=' + tTimer + '&isInternal=' + isInternal + '&noOfPerson=' + NoOfPerson;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
  public GetBookingsDetail = (rootId: number, propertyId: number, orgId: number, propertyServiceId: string): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetBookingsData?rootId=' + rootId + '&propertyId=' + propertyId + '&orgId=' + orgId + '&propertyServiceId=' + propertyServiceId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }
  public GetDataforCreateBooking = (rootId: number, propertyId: number, orgId: number, propertyServiceId: string): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetDataforCreateBooking?rootId=' + rootId + '&propertyId=' + propertyId + '&orgId=' + orgId + '&propertyServiceId=' + propertyServiceId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public GetBookingListByView = (fromDate: string, toDate: string): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetBookingListsByView?fromDate=' + fromDate + '&toDate=' + toDate;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }


  public SaveFilter(building: number, servicetype: number) {

    return this._http.get(this.baseUrl + '/digimakerwebapi/api/Booking/SavePreference?key=filter&value=' + building + ',' + servicetype, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }


  public GetBookingList(serviceId: number, from: string = '', to: string = '') {

    return this._http.get(this.baseUrl + '/digimakerwebapi/api/Articles/BookingList?serviceId=' + serviceId + '&from=' + from + '&to=' + to, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }
  public GetCustomerFromOutside(custNo: any) {
    return this._http.get('https://data.brreg.no/enhetsregisteret/api/enheter/' + custNo + '&page=1&size=5000', { headers: this.getJsonHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));

  }
  public GetCustomerFromOutsideByName = (custName: any = '', pageNo: number, limit: number): Observable<any> => {

    return this._http.get('https://data.brreg.no/enhetsregisteret/api/enheter?navn=' + custName + '&page=' + pageNo + '&size=' + limit, { headers: this.getJsonHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));

  }

  public SaveCustomer = (customer: any): Observable<any> => {

    return this._http.post(this.baseUrl + '/digimakerwebapi/api/Account/AddingNewCustomer', customer, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }
  public IsCustomerExists = (customerNo: string): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/IsCustomerExist?customerNo=' + customerNo;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public getCutomerList = (): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Account/GetCustomerList';
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }
  private getHeaders() {

    let headers = new Headers();
    headers.append('Accept', 'ng');
    headers.append('Authorization', 'Basic ' + btoa(AppSettings.Auth));
    return headers;
  }
  private getJsonHeaders() {

    let headers = new Headers();

    headers.append('Accept', 'application/json');

    return headers;
  }
  private extractData(res: Response) {
    let body = res.json();
    return body || {};
  }
  private handleError(error: Response) {
    console.error(error);
    return Observable.throw(error.json().error());
  }

  public getServiceVatArticles = (serviceId: number, formId: number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/getServiceVatArticles?articleId=' + serviceId + '&FormID=' + formId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public getserviceMembers(serviceId: number, formId: number) {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetMemberForService?articleId=' + serviceId + '&FormID=' + formId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
  public getServiceIsMutiplePrice(serviceId: number, formId: number) {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Booking/getServiceIsMutiplePrice?articleId=' + serviceId + '&formId=' + formId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public getBookingDetailByBookingId(bookingId: number) {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetBookingDetailByBookingId?bookingId=' + bookingId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public GetOrderDetailByOrderId(orderId: number) {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetOrderDetailByOrderId?orderId=' + orderId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }

  public clearCache(prefix: string) {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Crm/clearCache?filePrefix=' + prefix;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData), catchError(this.handleError));
  }
}
