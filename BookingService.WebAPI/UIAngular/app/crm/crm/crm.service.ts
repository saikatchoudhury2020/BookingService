import { Injectable } from "@angular/core";
import { Observable, throwError } from "rxjs";
import { Http, Response, RequestOptions, Headers } from "@angular/http";
import { map, tap, catchError } from "rxjs/operators";
import { AppSettings } from "src/app/appConfig";

@Injectable({
  providedIn: "root"
})
export class CrmService {
  private actionGetUrl: string;
  private baseUrl: string = AppSettings.API;
  private token: string;

  constructor(private _http: Http) { }

  private getHeaders() {
    let headers = new Headers();
    headers.append("Accept", "application/json");
    headers.append("Authorization", "Basic " + btoa(AppSettings.Auth));
    return headers;
  }
  private extractData(res: Response) {
    let body = res.json();
    return body || {};
  }
  private handleError(error: Response) {
    // console.error(error);
    //return Observable.throw(error);
    return throwError(error);
  }

  public GetBuildingListDetail = (id: number): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Articles/BulidingList?menuID=" + id;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public GetMenuDetail = (menuId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/menuitems/GetMenuDetail?menuItemID=" +
      menuId +
      "&includeArticle=1";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetUserDetail = (userId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/GetUserDetail?userId=" + userId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetArticleDetail = (articleId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/GetServiceDetail?serviceId=" +
      articleId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetServiceGrpDetail = (serviceGrpId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/GetServiceGroupDetail?serviceGrpId=" +
      serviceGrpId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetOrgDetail = (orgId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/CustomerCompany/GetCustomerCompanyDetail?orgId=" +
      orgId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  }
  public SetMVAToCustomer = (isMva: boolean, orgId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/UpdateMVACustomer?isMva=" + isMva + "&orgId=" +
      orgId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  }
  public GetUserGroup = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/CustomerCompany/GetUserGroup";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public GetUserGroupList = (userGroupId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/CustomerCompany/GetUserGroupList?userGroupId=" +
      userGroupId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public DeleteUserFromUserList = (
    userGroupId: string,
    userId: string
  ): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/CustomerCompany/DeleteUserGroupList?userGroupId=" +
      userGroupId +
      "&userId=" +
      userId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public CreateNewUserGroup = (userGroupName: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/CustomerCompany/CreateNewUserGroup?userGroupName=" +
      userGroupName;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public DeleteUserGroup = (userGroupId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/CustomerCompany/DeleteUserGroup?userGroupId=" +
      userGroupId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public GetOrderList = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Articles/BookingList";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public SendOrder = (value: any): Observable<any> => {
    let values = JSON.stringify(value);
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/OrderSendToVismaGlobal?orderIds=" +
      value;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public SendBookingsConfirm = (value: any, erpClient: string): Observable<any> => {
    let values = JSON.stringify(value);
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/BookingsSendToConfirm?orderIds=" +
      value + '&erpClient=' + erpClient;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public ValidateFile = (bookingids: any, value: any): Observable<any> => {
    return this._http.post(this.baseUrl + '/digimakerwebapi/api/Crm/UploadFilesValidate?bookingID=' + bookingids, value, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }

  public SendCombineOrder(data: any, objectList: any, isIncludeAgreement: any, isSplitting: any, agreementText: any, ToDate: any, fileText: any, ToFileDate: any, splitingText: any, erpClient: any) {
    const urlval = '/digimakerwebapi/api/Crm/OrderExecute?bookingIDs=' + objectList + '&AgreementToDate=' + ToDate + '&FakturajournalToDate=' + ToFileDate + '&SplittPercent=' + splitingText + '&AgreementText=' + agreementText + '&FakturajournalText=' + fileText + '&IsSplitting=' + isSplitting + '&IsIncludeAgreement=' + isIncludeAgreement + '&erpClient=' + erpClient;
    return this._http.post(this.baseUrl + urlval, data, { headers: this.getHeaders() })
      .pipe(map(this.extractData)
        , catchError(this.handleError));
  }
  public OrderSendToConfirm = (value: any): Observable<any> => {
    let values = JSON.stringify(value);
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Crm/OrderSendToConfirm?orderIds=" +
      value;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public HideUser = (userId: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/HideUser?userId=" + userId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public GetBookingsListDisplay = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/BookingsListDisplay";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetOrdersListDisplay = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/OrdersListDisplay";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetConfirmedListDisplay = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/ConfirmedListDisplay";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetTransferedListDisplay = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl + "/digimakerwebapi/api/Crm/TransferedListDisplay";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetDataforCreateBooking = (
    rootId: number,
    propertyId: number,
    orgId: number,
    propertyServiceId: string
  ): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Articles/GetDataforCreateBooking?rootId=" +
      rootId +
      "&propertyId=" +
      propertyId +
      "&orgId=" +
      orgId +
      "&propertyServiceId=" +
      propertyServiceId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetOrganizationList = (parentId: number): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/Articles/GetOrganizationList?parentId=" +
      parentId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public GetAgreementTypesList = (): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/crm/GetAgreementTypes";
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public GetAgreementsList = (orgId: number, userid: number): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/crm/GetAgreements?orgId=" + orgId + "&userid=" + userid;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };
  public AddUpdateAgreementTypes = (AgreementTypeId: number, Name: string, Article: string, Text: string, Price: string, NumberOfMonths: number): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/crm/AddUpdateAgreementTypes?Name=" +
      Name + "&Article=" + Article + "&Text=" + Text + "&Price=" + Price + "&NumberOfMonths=" + NumberOfMonths + "&AgreementTypeId=" + AgreementTypeId;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

  public AddUpdateAgreements = (AgreementId: number, Customer: number, Contact: number, AgreementTypeId: number, FromDate: string, ToDate: string, Article: string, Text: string, Price: string, NumberOfMonths: number, InvoicedToDate: string): Observable<any> => {
    this.actionGetUrl =
      this.baseUrl +
      "/digimakerwebapi/api/crm/AddUpdateAgreements?AgreementId=" +
      AgreementId + "&Customer=" + Customer + "&Contact=" + Contact + "&FromDate=" + FromDate + "&ToDate=" + ToDate + "&Article=" + Article + "&Text=" + Text + "&Price=" + Price + "&NumberOfMonths=" + NumberOfMonths + "&AgreementTypeId=" + AgreementTypeId + "&InvoicedToDate=" + InvoicedToDate;
    return this._http
      .get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(
        map(this.extractData),
        catchError(this.handleError)
      );
  };

}
