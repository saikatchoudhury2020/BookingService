import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AppService } from 'src/app/app.service';
import { ToastService } from 'src/app/toast.service';
@Component({
  selector: 'dm-user',
  templateUrl: './user.component.html',
  styleUrls: ['./user.component.css']
})
export class UserComponent implements OnInit,OnChanges {
  @Input() isOpen = false;
  @Input() CustomerNo:any;
  @Output() saved = new EventEmitter<Array<any>>();
  @Output() closed = new EventEmitter<any>();
  
  userForm:FormGroup;
  display='none';

  constructor(private fb: FormBuilder,private _userService: AppService, private _toastService:ToastService) { }

  ngOnInit() {
    this.userForm=this.fb.group({
       
      FirstName:['', [Validators.required]],
      LastName: ['', [Validators.required]],
     
      Email:['', [Validators.required, Validators.email]],
      Mobile:['', [Validators.required]]
  });
  }

  ngOnChanges(changes){
    if(changes['isOpen'] ){
        if(this.isOpen){
          this.ResetForm();
          this.display='block';
        }
        else{
          this.close();
        }
    }
  }

  close() {
    this.display = 'none';
  
  
      if( this.isOpen )
    {
      this.isOpen = false;
         // this.parameters = undefined;
         this.ResetForm();
      this.closed.emit();
    }
  
  }

  ResetForm=()=>{
    this.userForm.reset();
    // this.userForm.patchValue({
    //   FirstName:'',
    //   LastName: '', 
    //   FullName: '',
    //   Email:'', 
    //   Mobile:''
    // });
  
    
  }

  SaveNewUser=()=>{
    let userData=this.userForm.value;
    userData["CustomerId"] = this.CustomerNo;
   
     this._userService.CreateNewUser(userData).subscribe(res=>{
        if(res!=0){
          this.saved.emit(res);
        }
        else{
          this._toastService.showError('User not saved Succesfully!!','Error','top-center');
        }
         
     });
 }

}
