import { Injectable } from '@angular/core';
import { Subscription, Observable } from 'rxjs';
import {timer} from 'rxjs';
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError } from 'rxjs/operators';
import { AppSettings } from "../appConfig";
@Injectable({
  providedIn: 'root'
})
export class KitchenService {
  private actionGetUrl: string;
  private baseUrl: string = AppSettings.API;
  private token: string;

  constructor(private _http: Http) { }
  private getHeaders() {
    
            let headers = new Headers();
            headers.append('Accept', 'application/json');
            headers.append('Authorization', 'Basic ' + btoa(AppSettings.Auth));
            return headers;
        }
  public GetOrderList = (date:any, buildingId:number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Kitchen/GetKitchenDisplay?TodayDate='+date+'&buildingid='+buildingId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
    .pipe(map(this.extractData),catchError(this.handleError));
}


public GetBuilding = (buildingId:number): Observable<any> => {
    this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/MenuItems/GetMenuDetail?menuItemID='+buildingId;
    return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
    .pipe(map(this.extractData),catchError(this.handleError));
}

public UpdateDelivered = (OrderNo : number): Observable<any> => {
  this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Kitchen/UpdateDelivered?OrderNo='+OrderNo;
  return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
  .pipe(map(this.extractData),catchError(this.handleError));
}

public UpdateClean = (OrderNo : number): Observable<any> => {
  this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Kitchen/UpdateClean?OrderNo='+OrderNo;
  return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
  .pipe(map(this.extractData),catchError(this.handleError));
}
public GetKitchenNotification = (buildingId:number): Observable<any> => {
  this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Kitchen/GetKitchenNotification?buildingid='+buildingId;
  return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
  .pipe(map(this.extractData),catchError(this.handleError));
}
private extractData(res: Response) {
  let body = res.json();
  return body || {};
}

private handleError(error: Response) {	
	if( error.status == 403 )
	{
		window.location.href = AppSettings.LoginPage + window.location.href.replace( '#', '%23' );
	}
  console.error(error);
  return Observable.throw(error.json().error());
}
}
