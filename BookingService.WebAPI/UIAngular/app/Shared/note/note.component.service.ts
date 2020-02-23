import { Injectable } from '@angular/core';
import { AppSettings } from 'src/app/appConfig';
import { Observable } from "rxjs";
import { Http, Response, RequestOptions, Headers } from '@angular/http';
import { map, tap, catchError } from 'rxjs/operators';
@Injectable({
  providedIn: 'root'
})
export class NoteService {
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
}