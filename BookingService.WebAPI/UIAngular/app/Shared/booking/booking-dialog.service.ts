import { Injectable, EventEmitter, Output } from "@angular/core";


@Injectable()
export class BookingDialogService {
    isOpen = false;

    @Output() change: EventEmitter<any> = new EventEmitter();

    open(value:any) {
        this.isOpen = true;
        this.change.emit({isOpen:this.isOpen,event:value});
    }

}