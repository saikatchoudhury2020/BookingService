import { Component, Input, OnInit, OnChanges, EventEmitter, Output } from "@angular/core";

import { FormBuilder, FormArray, FormGroup, FormControl, AbstractControl } from "@angular/forms";
import { Router } from "@angular/router";
import { UserListService } from './user-list.service';
declare var jquery: any;
declare var $: any;
@Component({
  selector: 'user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit,OnChanges {

  @Input() buildingId: number;
  @Input() orgId: number;
  display = 'none';
  messageDisplay = 'none';
  messageType :string = '';

  isSelectedAll: boolean;

  selectedUsers :any;
  selectedUsersInfo = [];
  selectedUserId: number;
  selectedOrg: number;
  messageForm: any;
  messageSubject: string;
  messageBody: string;

  sending:boolean= false;
  errorMessage: string;

  userListData: any;
  constructor(private _userListService: UserListService, private _router: Router) { }
  ngOnInit(): void {
      if (!this.buildingId) {
          this.buildingId = 0;
      }
      if (!this.orgId) {
          this.orgId = 0;
      }
      this._userListService.UserListByBuilding(this.buildingId, this.orgId).subscribe(data => {
          this.userListData = data;
          var selectedUsersObject = {};
          for (var index in data) {
              selectedUsersObject[data[index]['id']] = false;
          }
          this.selectedUsers = selectedUsersObject;
      });

      //this._userListService.UserListByBuilding(this.buildingId).subscribe(
      //    data => {
      //        this.userListData = data;
      //        var selectedUsersObject = {};
      //        for (var index in data)
      //        {
      //            selectedUsersObject[data[index]['id']] = false;
      //        }
      //        this.selectedUsers = selectedUsersObject;
      //    });


  }
  ngOnChanges(): void {
      if (!this.buildingId) {
          this.buildingId = 0;
      }
      if (!this.orgId) {
          this.orgId = 0;
      }
      this._userListService.UserListByBuilding(this.buildingId, this.orgId).subscribe(data => {
          this.userListData = data;
          var selectedUsersObject = {};
          for (var index in data) {
              selectedUsersObject[data[index]['id']] = false;
          }
          this.selectedUsers =selectedUsersObject;
      });
  }
  getSelecteUsers(): Array<object> {
      var result = [] as object[];
      for (var index in this.userListData)
      {
          var person = this.userListData[index];
          if (this.selectedUsers[person.id])
          {
              result.push(person);
          }
      }
      return result;
  }


  onCloseHandled() {
      this.display = 'none';
  }

  sendMessageDialog(type: string) {
      this.selectedUsersInfo = this.getSelecteUsers();
      if (this.selectedUsersInfo.length > 0)
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
          var userids :any[] =[];
          for (var userid in this.selectedUsers) {
              if (this.selectedUsers[userid]) {
                  userids.push(userid);
              }
          }

          var messageTypeInt = this.messageType == 'email' ? 1 : 2;

          this._userListService.SendMessage(messageTypeInt, 2, userids, this.messageSubject, this.messageBody).subscribe(data => {
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

  selectAll() {
      for (var i in this.selectedUsers)
      {
          this.selectedUsers[i] = this.isSelectedAll;
      }
  }

  onCloseMessage() {
      this.messageDisplay = 'none';
  }

  openModal(userId: number) {

      this.selectedUserId = userId;
     
      this.display = 'block';
      $('.bookModal').show();

  }
  refresh(message: string): void {
    
      this._userListService.UserListByBuilding(this.buildingId, this.orgId).subscribe(
          data => {
              this.userListData = data;
              var selectedUsersObject = {};
              for (var index in data) {
                  selectedUsersObject[data[index]['id']] = false;
              }
              this.selectedUsers = selectedUsersObject;
          });

  }

}
