import { Injectable } from '@angular/core';
import { AppSettings } from 'src/app/appConfig';
import { Observable } from "rxjs";
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserGroupService {

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
      return Observable.throw(error.json().error());
  }
  public GetUserGroupList = (): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/CustomerCompany/GetUserGroup';
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }

  public UserWiseGroupList = (userId: number): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/CustomerCompany/UserWiseGroupList?userId=' + userId;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }

  public AssignUserGrouptoUser(userGroupModel: any) {

      return this._http.post(this.baseUrl + '/digimakerwebapi/api/CustomerCompany/AddToUserGroupList', userGroupModel, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }
}
