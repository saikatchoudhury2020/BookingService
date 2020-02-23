import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AppService } from 'src/app/app.service';
import { ToastService } from 'src/app/toast.service';

@Component({
  selector: 'dm-customer',
  templateUrl: './customer.component.html',
  styleUrls: ['./customer.component.css']
})
export class CustomerComponent implements OnInit,OnChanges{

  @Input() isOpen = false;
  @Output() saved = new EventEmitter<Array<any>>();
  @Output() closed = new EventEmitter<any>();

  customerForm:FormGroup;
  display='none';
  ControlName='search';
  displayCustomerWarning='';
  pageSize:0;
  bufferSize=20;
  CustomerData:any=[];
  Customer:any;
  constructor(private fb: FormBuilder,private _customerService: AppService, private _toastService:ToastService) { }

  ngOnInit() {
    
    
    this.customerForm=this.fb.group({
      search:'',
      IsPrivate:false,
      Id:'',
      Name:['', [Validators.required]],
      Email: '',
      Mobile: '',
      Land:['', [Validators.required]],
      Address1:['', [Validators.required]],
      Address2:'',
      Pincode:['',[Validators.required]],
      Country:''

  });

  // this.customerForm.get('Id').valueChanges.subscribe(val => {
  //   if(val){
  //     this._customerService.IsCustomerExists(val).subscribe(data=>{
  //       if(Object.keys(data).length === 0 && data.constructor === Object){
  //           this._customerService.GetCustomerFromOutside(val).subscribe(res=>{
  //               this.Customer=res;
  //            });
         
  //       }
  //       else{

  //        // this._toastService.showError('Customer Already Exists in Digimaker','Error');
  //          // this.displayCustomerWarning="Customer Already Exists in Digimaker";
  //       }
  //   })
  //   }

  // });

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

  getSearch=(data:any)=>{
    this.CustomerData=[];
    this.pageSize=0;
     this.getCustomerData(data);
 }
 onScroll=(data:any)=>{
    if(data.allow){
        this.pageSize+=1;
        this.getCustomerData(data.search);
    }
 }

 getSelectedCustomer=(data:any)=>{


   if(data){
  
    this._customerService.IsCustomerExists(data.organisasjonsnummer).subscribe(isExist=>{
      if(Object.keys(isExist).length === 0 && isExist.constructor === Object){
        this.customerForm.patchValue({
          Id:data.organisasjonsnummer,
          Name:data.navn,
          IsPrivate:false,
          Email:'',
          Mobile:'',
          Land:data.forretningsadresse.land,
          Address1:data.forretningsadresse.adresse[0],
          Address2:'',
          Pincode:data.forretningsadresse.postnummer,
          Country:data.forretningsadresse.poststed
          
        });
      }
      else{
        this._toastService.showError('Customer Already Exists in Digimaker','Error','top-center');
       // this.displayCustomerWarning="Customer Already Exists in Digimaker";
      }
    });
    
    

   }
 }
 public getCustomerData=(val:any)=>{

  this._customerService.GetCustomerFromOutsideByName(val,this.pageSize,this.bufferSize).subscribe(res=>{
      if(this.CustomerData.length===0){
          this.CustomerData =res._embedded.enheter;
      }
      else{
          this.CustomerData =this.CustomerData.concat([...res._embedded.enheter]);
      }
     
  })
        
}
ResetForm=()=>{
  this.customerForm.patchValue({
    search:'',
    IsPrivate:false,
    Id:'',
    Name:'', 
    Email: '',
    Mobile: '',
    Land:'',
    Address1:'',
    Address2:'',
    Pincode:'',
    Country:''
  });

  this.displayCustomerWarning='';
}

clearSearch=(data:any)=>{
 this.ResetForm();

}

SaveNewCustomer=()=>{

  let isAllow=true;
  if(this.customerForm.get('IsPrivate').value==false){
    if(!this.customerForm.get('Id').value){
      this.customerForm.get("Id").setErrors({'required': true});
      isAllow=false;
    }
  }
if(isAllow){
  this._customerService.SaveCustomer(this.customerForm.value).subscribe(res=>{
    if(res!=0){
        this.displayCustomerWarning="Created Customer Successfully";
        this.saved.emit(res);
     
      
    }
    
    
      });
}
}

}
