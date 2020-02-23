import { Injectable } from '@angular/core';
import { ToastrManager } from 'ng6-toastr-notifications';
@Injectable({
  providedIn: 'root'
})
export class ToastService {

  constructor(private toastr: ToastrManager) { }

showSuccess(message:string,headermsg:string) {
    this.toastr.successToastr(message, headermsg);
}

showError(message:string,headermsg:string,place:any) {
    this.toastr.errorToastr(message, headermsg, {
      position: place
  });
}

showWarning(message:string,headermsg:string,place:any) {
  this.toastr.warningToastr(message, headermsg, {
    position: place
});
}

showInfo(message:string,headermsg:string,place:any) {
  this.toastr.infoToastr(message, headermsg, {
    position: place
});
}

}
