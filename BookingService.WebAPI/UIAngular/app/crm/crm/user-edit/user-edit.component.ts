import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.css']
})
export class UserEditComponent implements OnInit {

  userData: any;
  imagePath: string = '';
  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
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

}
