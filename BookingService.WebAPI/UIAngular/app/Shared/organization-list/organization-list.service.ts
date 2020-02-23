import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError } from 'rxjs/operators';
import { AppSettings } from 'src/app/appConfig';
@Injectable({
  providedIn: 'root'
})
export class OrganizationListService {

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


  public GetOrganizationList = (parentId: number): Observable<any> => {
      this.actionGetUrl = this.baseUrl + '/digimakerwebapi/api/Articles/GetOrganizationList?parentId=' + parentId;
      return this._http.get(this.actionGetUrl, { headers: this.getHeaders() })
      .pipe(map(this.extractData),catchError(this.handleError));
  }
}
