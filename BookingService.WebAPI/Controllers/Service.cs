using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using RestSharp;
using RestSharp.Deserializers;
using System.Web.Helpers;
using System.Web.Script.Serialization;
using logWriter;
using BookingService.WebAPI.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp.Serialization.Json;

namespace webServices
{
    public class WebServices
    {
        public string GetToken()
        {
            var client = new RestClient("https://api.braatheneiendom.no/VGOrderWS/Token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Postman-Token", "1480aca7-9357-45ec-9ed2-dad0d31b125b");
            request.AddHeader("cache-control", "no-cache");
            request.AddParameter("undefined", "grant_type=password&username=apiuser@digimaker.no&password=K7!0haTQ", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                TokenResponse accesstoken = new JsonDeserializer().Deserialize<TokenResponse>(response);
                return accesstoken.access_token;
            }
            return "";
        }

        public string GetCustomer(string accesstoken,string ERPClient,string CompanyNo)
        {
            var client = new RestClient("https://api.braatheneiendom.no/VGOrderWS/api/Customer/"+ ERPClient + "/"+ CompanyNo);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Postman-Token", "e338f17b-3307-4e67-953e-85b085d06f2a");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "Bearer "+ accesstoken);
            var response = client.Execute(request) as RestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                //CustomerResponse customer = new JsonDeserializer().Deserialize<CustomerResponse>(response.Content);
                //var customer = JsonConvert.DeserializeObject<JArray>(response.Content);
                var customer = JsonConvert.DeserializeObject(response.Content);
                var id = JsonConvert.DeserializeObject<CustomerResponse>(response.Content).CustomerNo;
                return id;
            }
            else if (response != null)
            {
                logWriter.LogWriter log = new LogWriter();
                log.LogWrite("Failed request:- " + string.Format("Status code is {0} ({1}); response status is {2}", response.StatusCode, response.StatusDescription, response.ResponseStatus));
                
            }
                return "";
        }
        public string PostOrder(string accesstoken, OrderHead ord)
        {
            var client = new RestClient("https://api.braatheneiendom.no/VGOrderWS/api/order");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Postman-Token", "e1748bb5-a2dc-4cb4-af7c-5f5ce0d9c910");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Authorization", "Bearer " + accesstoken);
            request.AddHeader("Content-Type", "application/json");
            var json = JsonConvert.SerializeObject(ord);
            request.AddParameter("undefined", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.Created)
            {
               
                var orderNo = JsonConvert.DeserializeObject<OrderResponse>(response.Content).OrderNo;
               // OrderResponse order = new JsonDeserializer().Deserialize<OrderResponse>(response);
             //   logWriter.LogWriter log = new LogWriter();
               // var js = new JavaScriptSerializer().Serialize(order);
              //  log.LogWrite("Success request:- " + json.ToString() + " Response:- " + orderNo);
                return orderNo;
            }
            else
            {
                logWriter.LogWriter log = new LogWriter();
               // OrderResponse order = new JsonDeserializer().Deserialize<OrderResponse>(response);
                CrmController cc = new CrmController();
                cc.UpdateVMOrderFailedErrorMessage(ord.OrderNo,response.Content);
              //  var js = new JavaScriptSerializer().Serialize(order);
               // log.LogWrite("Failed request:- " + json.ToString() + " Response:- " + js.ToString());
            }
            return "";
        }


        public class TokenResponse
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
            public string userName { get; set; }
            public string issued { get; set; }
            public string expires { get; set; }
        }
        public class CustomerResponse
        {
            public string CustomerNo { get; set; }
            public string Name { get; set; }
            public string CompanyNo { get; set; }
            public string EmailInvoice { get; set; }
            public string Email { get; set; }
            public string WWWAddress { get; set; }
            public string CustomerGroup { get; set; }
            public string CustomerProfile { get; set; }
            public string PrintProfile { get; set; }
            public string EDIProfile { get; set; }
            public string Department { get; set; }
            public bool Inactive { get; set; }
        }
        public class OrderResponse
        {
            public string OrderNo { get; set; }
            public string Message { get; set; }
        } 
        public class OrderHead
        {
            public int OrderNo { get; set; }
            public string CustomerNo { get; set; }
            public string CustomerGroupNo { get; set; }
            public string OrderDate { get; set; }
            public string OurReference { get; set; }
            public string YourReference { get; set; }
            public string EmailAddress { get; set; }
            public string ERPClient { get; set; }
            public int? CustomerPurchaseNo { get; set; }
            public string DeliveryDate { get; set; }

            public List<OrderLines> OrderLines { get; set; }
        }
        public class OrderLines
        {
            public string ArticleNo { get; set; }
            public string Text { get; set; }
            public double Quantity { get; set; }
            public double DiscountPercent { get; set; }
            public decimal? NetAmount { get; set; }
            public double GrossAmount { get; set; }

        }
    }
}