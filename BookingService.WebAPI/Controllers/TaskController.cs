using BookingService.WebAPI.Models;
using DMBase.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebAPI.Controllers
{
    //Can invoke this from background script locally(visit locally) or from server api. 
    //todo: make it local and from api
    public class TaskController : ApiController
    {
        [HttpGet]
        public string BookingSendMessage( int bookingID = 0 )
        {
            //get list of message
            var result = "";
            BraathenEiendomEntities entity = new BraathenEiendomEntities();
            var list = entity.BookingMessages.Where( w => w.Status == 0
                                                          && ( bookingID == 0 || bookingID !=0 && w.BookingID == bookingID )
                                                          && ( w.SendTime == null || w.SendTime <= DateTime.Now ) ).ToList();

            result += "Found records: " + list.Count + ".";
            foreach (var message in list)
            {
                result += "Sending " + message.ID + ".";
                var messageType = 0;
                if (message.Type == "email")
                {
                    messageType = 1;
                }
                else if (message.Type == "sms")
                {
                    messageType = 2;
                }

                var toArray = message.To.Split( ';' );
                var to = new Dictionary<string, string[]>();
                foreach( var item in toArray )
                {
                    to.Add( item, new string[]{ null } );
                }

                var parameters = new Dictionary<string, string>();
                if (messageType != 0)
                {
                    //send
                    var sendResult = Message.SendMessage(type: messageType,
                                                         to: to,
                                                         title: message.Subject,
                                                         body: message.Body,
                                                         parameters: parameters);
                    if (sendResult == "1")
                    {
                        message.Status = 1;
                        result += "[" + message.ID + "]Sent to " + message.To;
                    }
                    else
                    {
                        message.Status = 2;
                        result += "[" + message.ID + "]Not send to" + message.To + ". Error: " + sendResult;
                    }
                    entity.SaveChanges();
                }
            }

            return result;
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        
    }
}