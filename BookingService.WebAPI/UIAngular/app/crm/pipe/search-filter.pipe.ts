import { Injectable, Pipe, PipeTransform } from '@angular/core';
import { timer } from 'rxjs';
import * as moment from 'moment';

@Pipe({
  name: 'searchfilter'
})

@Injectable()
export class SearchFilterPipe implements PipeTransform {
  transform(items: any, BuildingID: any, ServiceType: any, ServiceID: any, Customer: any, UserID: any, fromdate: any, todate: any): any {
    if (!items) return [];
    if (BuildingID != 0) {
      var items = items.filter(item => item.BuildingID == BuildingID);
    }
    if (ServiceType != 0) {
      var items = items.filter(item => item.ServiceType == ServiceType);
    }
    if (ServiceID != 0) {
      var items = items.filter(item => item.ServiceID == ServiceID);
    }
    if (Customer != 0) {
      var items = items.filter(item => item.Customer == Customer);
    }
    if (UserID != 0) {
      var items = items.filter(item => item.UserID == UserID);
    }
    if (fromdate && todate) {
      fromdate = moment(fromdate).format("YYYY-MM-DD")
      todate = moment(todate).format("YYYY-MM-DD")
      var items = items.filter(item => moment(item.FromDate).format("YYYY-MM-DD") >= fromdate && moment(item.FromDate).format("YYYY-MM-DD") <= todate);
    }
    return items;
  }
}
