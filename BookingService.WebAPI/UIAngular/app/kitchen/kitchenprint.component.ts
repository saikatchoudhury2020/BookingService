import { Component, OnInit } from '@angular/core';
import { AppService } from '../app.service';
import * as moment from 'moment';
import { ActivatedRoute, Router } from "@angular/router";

@Component({
  selector: 'app-kitchenprint',
  templateUrl: './kitchenprint.component.html',
  styleUrls: ['./kitchenprint.component.css']
})
export class KitchenprintComponent implements OnInit {


  bookingId:number = 0;
  
  booking:any;
  
  error:string = '';

  sum:any;
  sum_total: number = 0;
  
  constructor( private _route: ActivatedRoute,
      private _router: Router, private _Service: AppService ) { 
  }

  ngOnInit() {
	  
	   this._route.params.subscribe(params => {
		   this._Service.send( 'Articles/GetBookingDetailByBookingId', { bookingId: params.bookingid } ).subscribe( data =>{
				this.booking = data;  
				
				var sum = [];
				var sum_total:number = 0;
				data.foods.forEach(function( food ){
					if( food.IsKitchenOrder )
					{
						var article = food.ArticleId;
						//var price = food.CostPrice;
						var number = food.Qty;
						var obj = {articleId: article, number: number, price: food.CostTotal }
			
					    var exist = false;
						var i = 0;
						for( ; i< sum.length; i++ ){
							if( obj.articleId ==sum[i].articleId )
							{
								exist = true;
								break;
							}
						}
						
						sum_total += obj.price;
						if( exist )
						{
							sum[i].number +=obj.number;
							sum[i].price =parseFloat( (sum[i].price+obj.price).toFixed(2) );
						}
						else
						{
							sum.push( obj );
						}
					}
			  
				});
				this.sum=sum;
				this.sum_total=parseFloat(sum_total.toFixed(2));
		  } );		   
	   });

	  
  }
  
  dateObject( date: string ):any{
	  return moment( date );
  }
  
   ngAfterViewInit(){
		//window.print();
	}

}
