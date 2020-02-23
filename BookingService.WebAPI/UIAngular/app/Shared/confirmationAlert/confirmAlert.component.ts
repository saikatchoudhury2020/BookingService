import { Component, Input, OnInit, OnChanges, HostBinding, Output, EventEmitter } from "@angular/core";
import { confirmAlertService } from "../confirmationAlert/service/confirmAlert.service";
@Component({
  selector: 'confirm-alert',
  templateUrl: 'confirmAlert.component.html',
  styleUrls: ['confirmAlert.component.css']
})
export class ConfirmAlertComponent implements OnInit {

  @Input() question: string;
  @Input() title: string;
  @Input() type: string;
  @Input() data: any;
  @Input() ConfirmButtonText: string;
  @Output() notify: EventEmitter<any> = new EventEmitter<any>();
  display = 'none';
  isDelCalval = false;
  constructor(private _confirmService: confirmAlertService) { }
  ngOnInit(): void {
    this.isDelCalval = false;
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
    this.notify.emit({ val: false, type: this.type, data: this.data, isDelcal: false });
  }
  onConfirm() {

    this.notify.emit({ val: true, type: this.type, data: this.data, isDelcal: this.isDelCalval });
  }

}
