import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { confirmAlertService } from '../../../Shared/confirmationAlert/service/confirmAlert.service';
@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css']
})
export class UserComponent implements OnInit {

  
  userData: any;
  imagePath: string = '';
  UserId: string;
  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
      private _confirmService:confirmAlertService,
      private _crmService: CrmService) {
  }
  ngOnInit() {
      this._route.params.subscribe(params => {
          this._crmService.GetUserDetail(params.id).subscribe(data => {

              this.userData = data;
              if (data.Status) {
                  this.userData.state = 'DeActive';

              }
              else {
                  this.userData.state = 'Active';
              }
              this.userData.access = 'BookingAccess'
              console.log(this.userData);
          });
      });


  }
  save(): void {

  }
  HideUser(UserId: string): void {
    this.UserId = UserId;
    this._confirmService.toggle();
    }
ConfirmCommand(value: any): void {
    if (value.val) {
        this._crmService.HideUser(this.UserId).subscribe(data => {
           
            this._confirmService.toggle();
        });
       
    }
   
}
}
