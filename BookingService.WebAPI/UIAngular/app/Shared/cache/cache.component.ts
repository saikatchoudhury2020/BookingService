import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { AppService } from 'src/app/app.service';

@Component({
  selector: 'app-cache',
  templateUrl: './cache.component.html',
  styleUrls: ['./cache.component.css']
})
export class CacheComponent implements OnInit {
  selectedCache='';
  @Output() notify: EventEmitter<any> = new EventEmitter<any>();
  constructor(private _bookingService: AppService) { }

  ngOnInit() {
  }
  clearCache():void{
    if(this.selectedCache){
        this._bookingService.clearCache(this.selectedCache).subscribe(data => {

  if(data==true){
    this.notify.emit(true);
  }
  else{
    this.notify.emit(false);
  }
        });
    }
   
}

}
