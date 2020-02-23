import { Component, Input, OnInit, OnChanges, HostBinding, Output, EventEmitter } from "@angular/core";
import { ServiceAlertService } from "../service-alert/service-alert.Service";
@Component({
  selector: 'app-service-alert',
  templateUrl: './service-alert.component.html',
  styleUrls: ['./service-alert.component.css']
})
export class ServiceAlertComponent implements OnInit {

  @Input() question: string;
  @Input() title: string;
  @Input() type: string;
  @Input() data: any;
  @Input() ConfirmButtonText :string;
  @Output() notify: EventEmitter<any> = new EventEmitter<any>();
  display = 'none';
 
  constructor(private _confirmService: ServiceAlertService) {}
  ngOnInit(): void {
      this.display = 'none';
      console.log(this.title);
      this._confirmService.change.subscribe(isOpen => {
          this.display = isOpen;
          console.log(this.title);
          if (isOpen) {
              this.display = 'block';
          }
          else {
              this.display = 'none';
          }
      });
  }
  //ngOnChanges(): void {
  //    this.display = 'block';
  //}
  onCloseHandled() {
      //this.display = 'none';

      this._confirmService.toggle();
      this.notify.emit({val:false,type:this.type,data:this.data});
  }
  onConfirm() {
  this.notify.emit({val:true,type:this.type, data:this.data});
  }


}
