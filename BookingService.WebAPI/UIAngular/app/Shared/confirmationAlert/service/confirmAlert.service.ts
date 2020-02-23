import { Injectable, EventEmitter, Output } from "@angular/core";
import { Observable } from "rxjs";

@Injectable()
export class confirmAlertService {
    isOpen = false;

    @Output() change: EventEmitter<boolean> = new EventEmitter();

    toggle() {
        this.isOpen = !this.isOpen;
        this.change.emit(this.isOpen);
    }

}