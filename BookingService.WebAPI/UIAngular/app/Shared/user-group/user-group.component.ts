
import { UserGroupService } from './user-group.service';
import { Component, Input, OnInit, OnChanges, EventEmitter, Output } from "@angular/core";

import { FormBuilder, FormArray, FormGroup, FormControl, AbstractControl } from "@angular/forms";
import { Router } from "@angular/router";
declare var jquery: any;
declare var $: any;
@Component({
  selector: 'user-group',
  templateUrl: './user-group.component.html',
  styleUrls: ['./user-group.component.css']
})
export class UserGroupComponent implements OnInit,OnChanges {

  @Input() userId: number;
  @Input() orgId: number;
  @Output() notify: EventEmitter<string> = new EventEmitter<string>();
  userGroupData: any;
  allUserGroupselected: boolean = false;
  myForm: FormGroup;

  buttonTitle = 'Update';
  constructor(private _userGroupService: UserGroupService, private fb: FormBuilder, private _router: Router) { }
  ngOnInit(): void {
      this._userGroupService.GetUserGroupList().subscribe(data => this.userGroupData = data);
      this.myForm = this.fb.group({
          userGroup: this.fb.array([]),
          userId: ''
      });

  }
  ngOnChanges(): void {
      const userGroupFormArray = <FormArray>this.myForm.controls['userGroup'];

      this.clearForm(userGroupFormArray);
      this._userGroupService.UserWiseGroupList(this.userId).subscribe(data => {
          this.userGroupData.forEach(item => {
              for (let i = 0; i < data.length; i++) {
                  if (item.userGroupId == data[i].userGroupId) {
                      item.selected = true;
                      userGroupFormArray.push(new FormControl(item.userGroupId));
                  }
              }

          })
      });


  }
  onChange(userGroup: string, index: number, isChecked: boolean) {
      const userGroupFormArray = <FormArray>this.myForm.controls['userGroup'];

      if (this.allUserGroupselected) {
          this.allUserGroupselected = false;
      }

      if (isChecked) {
          userGroupFormArray.push(new FormControl(userGroup));
      } else {

          userGroupFormArray.removeAt(index);
      }


  }


  assignUserGroup = (): void => {
      this.myForm.get('userId').setValue(this.userId);
      console.log(this.myForm.value);
      this._userGroupService.AssignUserGrouptoUser(this.myForm.value).subscribe(data => {

          // this._router.navigate(['/user', 1019]);
          // this._router.navigate(['/organization', 2005]);
          this.notify.emit(data);
          //this._router.navigate(['/organization', this.orgId]);
          $('.bookModal').hide();

      });
  }

  toggleSelect = (isChecked: boolean): void => {
      const userGroupFormArray = <FormArray>this.myForm.controls['userGroup'];
      this.allUserGroupselected = isChecked;
      this.userGroupData.forEach(data => {
          let isduplicate = false;
          data.selected = isChecked;
          if (isChecked) {
              //userGroupFormArray.push(new FormControl(data.userGroupId));
              for (let i = userGroupFormArray.length - 1; i >= 0; i--) {

                  if (userGroupFormArray.at(i).value == data.userGroupId) {
                      isduplicate = true;
                  }

              }
              if (!isduplicate) {
                  userGroupFormArray.push(new FormControl(data.userGroupId));
              }
          } else {

              for (let i = userGroupFormArray.length - 1; i >= 0; i--) {
                  userGroupFormArray.removeAt(i);
              }
          }
      });



  }

  clearForm = (userGroupFormArray: FormArray): void => {
      this.allUserGroupselected = false;
      this.userGroupData.forEach(item => {
          for (let i = userGroupFormArray.length - 1; i >= 0; i--) {
              console.log(userGroupFormArray.value[i]);
              if (item.userGroupId == userGroupFormArray.value[i]) {
                  userGroupFormArray.removeAt(i);
                  item.selected = false;
              }

          }
      })
  }

}
