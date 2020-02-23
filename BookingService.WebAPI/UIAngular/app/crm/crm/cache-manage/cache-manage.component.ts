import { Component, OnInit } from '@angular/core';
import { Subscription, Observable } from 'rxjs';
import { timer } from 'rxjs';
@Component({
  selector: 'app-cache-manage',
  templateUrl: './cache-manage.component.html',
  styleUrls: ['./cache-manage.component.css']
})
export class CacheManageComponent implements OnInit {

  displayMessage='';
  color='';
  public showloader: boolean = false;
    private subscription: Subscription;
    private timer: Observable<any>;
  constructor() { }

  ngOnInit() {
  }
  ConfirmCommand(value:boolean) : void {
    if (value) {
     this.displayMessage="cache cleared !!";
     this.color='green';
      this.setTimer();
    }
    else{
      this.displayMessage="cache not cleared !!";
      this.color='red';
       this.setTimer();
    }
  }

  public setTimer() {

    // set showloader to true to show loading div on view
    this.showloader = true;
    this.timer = timer(3000);
    //savan this.timer = Observable.timer(3000); // 5000 millisecond means 5 seconds
    this.subscription = this.timer.subscribe(() => {
        // set showloader to false to hide loading div from view after 5 seconds
        this.showloader = false;
    });
}
}
