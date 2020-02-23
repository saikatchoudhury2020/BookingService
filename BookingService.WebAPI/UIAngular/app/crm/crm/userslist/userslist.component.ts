import { Component, OnInit } from '@angular/core';
import { CrmService } from '../crm.service';
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { confirmAlertService } from '../../../Shared/confirmationAlert/service/confirmAlert.service';
import { UserListService } from '../../../Shared/user-list/user-list.service';

@Component({
  selector: 'app-userslist',
  templateUrl: './userslist.component.html',
  styleUrls: ['./userslist.component.css']
})
export class UserslistComponent implements OnInit {

  userGroupName: string;
  userGroupData: any;
  deletedUserGroupId: string;
  display = 'none';
  alert: string;
  imagePath: string = '';
  
  
  selectedGroups:{};
  selectedGroupsInfo:Array<object>;
  messageDisplay = 'none';
  messageType :string = '';
  messageSubject: string;
  messageBody: string;

  sending:boolean= false;
  errorMessage: string;
  
  constructor(private _userListService: UserListService, private fb: FormBuilder, private _route: ActivatedRoute,
      private _router: Router,
      private _confirmService:confirmAlertService ,
      private _crmService: CrmService) {
  }
  ngOnInit() {
          this.onGetUserGroup();
          this.onCloseHandled();

  }
  save(): void {
      console.log(this.userGroupName);
      this._crmService.CreateNewUserGroup(this.userGroupName).subscribe(data => {
          this.onCloseHandled();
          this.onGetUserGroup();
      });
  }
  
  openModal() {
      this.display = 'block';
  }
  onCloseHandled() {
      this.display = 'none';
  }
  onGetUserGroup() {
      this._crmService.GetUserGroup().subscribe(data => {
          this.userGroupData = data;
		  
		  var selectedGroupsObject = {};
		  for (var index in data) {
              selectedGroupsObject[index] = false;
          }
		  this.selectedGroups = selectedGroupsObject;
          console.log(this.userGroupData);
      });
  }

  
  DeleteUserGroup(UserGroupId: string): void {
      this.deletedUserGroupId = UserGroupId;
      this._confirmService.toggle();
      }
  ConfirmCommand(value: any): void {
      if (value.val) {
          this._crmService.DeleteUserGroup(this.deletedUserGroupId).subscribe(data => {
              this.onGetUserGroup();
              this._confirmService.toggle();
          });
         
      }
     
  }
  
  
  getSelectedGroups(): Array<object> {
      var result = [];
      for (var index in this.selectedGroups)
      {
          if (this.selectedGroups[index])
          {
              result.push(this.userGroupData[index]);
          }
      }
	  this.selectedGroupsInfo = result;
      return result;
  }
  
  
  sendMessageDialog(type: string) {
      var result = this.getSelectedGroups();
      if (this.selectedGroupsInfo.length > 0)
      {
          this.messageSubject = '';
          this.messageBody = '';
          this.errorMessage = '';
          this.messageType = type;
          this.messageDisplay = 'block';
      }
      else
      {
          //todo: generic user friend prompt dialog
      }
  }
  
  onSendMessage() {
	  if( this.messageBody !='' )
	  {
		  this.sending = true;
		  var groupIds :any[] =[];
		  for (var index in this.selectedGroupsInfo) {
				  groupIds.push(this.selectedGroupsInfo[index]['userGroupId']);
		  }

		  var messageTypeInt = this.messageType == 'email' ? 1 : 2;

		  this._userListService.SendMessage(messageTypeInt, 1, groupIds, this.messageSubject, this.messageBody).subscribe(data => {
			  if (data == "1") {
				  this.sending = false;
				  this.onCloseMessage();
			  }else
			  {
				  this.errorMessage = data;
			  }
		  }, error => {
			  this.sending = false;
			  this.errorMessage = error;
		  });
	  }

	}
  
    onCloseMessage() {
      this.messageDisplay = 'none';
  }

}
