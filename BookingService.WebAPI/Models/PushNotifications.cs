using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class PushNotifications
    {
        public string DeviceToken
        { get; set; }
        public string Message
        { get; set; }
    }
}