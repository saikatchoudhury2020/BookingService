import { Injectable } from '@angular/core';
import { AppSettings } from 'src/app/appConfig';
import { Observable } from "rxjs";
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError } from 'rxjs/operators';
@Injectable({
  providedIn: 'root'
})
export class UserListService {

  private token: string;
  private actionGetUrl: string;
  private baseUrl: string = AppSettings.API;
  constructor(private _http: Http) {

  }

  private getHeaders() {

      let headers = new Headers();
      headers.append('Accept', 'application/json');
      headers.append('Authorization', 'Basic ' + btoa(AppSettings.Auth));
      return headers;
  }
  private extractData(res: Response) {
      let body = res.json();
      return body || {};
  }
  private handleError(error: Response) {
      console.error(error);
      return Observable.throw(error.text());
  }
 

  public SendMessage = (messageType: number, userType: number, list: number[], title: string, body: string): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Crm/SendMessage?messagetype=' + messageType +
                              '&usertype=' + userType + '&list=' + list.join(',') + '&title=' + title + '&body='+body;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }

  
  public UserListByBuilding = (buildingId: number, orgId: number): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/CustomerCompany/UserListbyBuilding?buildingId=' + buildingId + '&orgId=' + orgId;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }

  public MessageList = (type: number, offset: number, limit: number): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Crm/GetMessageLog?type=' + type + '&offset=' + offset + '&limit=' + limit;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }

  public MessageListByUser = (type:number,userId: number): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Crm/GetMessageLog?type=' + type + '&user=' + userId;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }
}
