import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { ActivatedRoute, Router } from "@angular/router";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
@Component({
  selector: 'app-usersgrouplist',
  templateUrl: './usersgrouplist.component.html',
  styleUrls: ['./usersgrouplist.component.css']
})
export class UsersgrouplistComponent implements OnInit {

  userGroupData: any;
  //  userGroupDataForm: FormGroup;
  imagePath: string = '';
  constructor(private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
      private _crmService: CrmService) {
  }
  ngOnInit() {
      this.onGetUserGroup();
  }

  save(): void {
      console.log();
  }
  onGetUserGroup() {
      this._route.params.subscribe(params => {
          this._crmService.GetUserGroupList(params.id).subscribe(data => {
              this.userGroupData = data;
              console.log(this.userGroupData);
          });
      });
  }
  DeleteUserFromUserList(UserGroupId: string, userId: string): void {
      console.log(UserGroupId);
      console.log(userId);

      this._crmService.DeleteUserFromUserList(UserGroupId, userId).subscribe(data => {

          this.onGetUserGroup();
         
      });

  }

}
