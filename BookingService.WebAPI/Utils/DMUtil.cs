using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace BookingService.WebAPI.Utils
{
    public class DMUtil
    {
        private static string GetCachePath(string key)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + ("cache/" + key + ".cache");
            return path;
        }

        /**
         * Use this method together with GetCachedObject.
         * 
         *  var key = "ddfsfs";
         *  
         *  var obj = DMUtil.GetCachedObject( key )
         *  if( obj == null )
         *  {
         *      var obj = <generate object>;
         *      DMUtil.WriteToCache( obj, key );
         *  }
         *  
         * 
         */
        public static void WriteToCache(object obj, string key)
        {
            var cacheFile = GetCachePath(key);
            string data = JsonConvert.SerializeObject(obj);
            File.WriteAllText(cacheFile, data); //Encoding.GetEncoding(28591)
        }

        /**
         * If expired return null.
         * expireTime - minutes, default is 15 mintues
         */
        public static object GetCachedObject(string key, Type type, int expireTime = 15)
        {
            var cacheFile = GetCachePath(key);

            Object obj = null;
            if (File.Exists(cacheFile) && DateTime.Now.Subtract(File.GetLastWriteTime(cacheFile)).TotalMinutes < expireTime)
            {
                var data = File.ReadAllText(cacheFile);
                obj = JsonConvert.DeserializeObject(data, type);
            }
            return obj;
        }

        public static object GetCachedObject(string prefix,List<string> keys,Type type) {
            Object obj = null;
            string cachedKey = GenerateCacheKey(prefix, keys.ToArray());
            var cacheFile = GetCachePath(cachedKey);
            if (File.Exists(cacheFile) )
            {
               
                var data = File.ReadAllText(cacheFile);
                obj = JsonConvert.DeserializeObject(data, type);
            }
            return obj;
        }


        public static void WriteToCache(object obj,string prefix, List<string> keys)
        {   
            string cacheKey = GenerateCacheKey(prefix, keys.ToArray());
            WriteToCache(obj, cacheKey);
           
        }

        protected static string GenerateCacheKey(string prefix, string[] keys)
        {
            var result = string.Join(",", keys);
            int hash = result.GetHashCode();
            return prefix + "-" + hash;
        }


    }
}