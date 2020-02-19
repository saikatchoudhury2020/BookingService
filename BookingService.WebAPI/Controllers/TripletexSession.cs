using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tripletex
{
    public class TripletexSession : IDisposable
    {
        private readonly CancellationToken _cancellation;
        private readonly HttpClient _webClient;

        public TripletexSession(CancellationToken cancellation = default(CancellationToken))
        {
            _cancellation = cancellation;
            HttpMessageHandler httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                AllowAutoRedirect = false,
                UseCookies = false
            };
            _webClient = new HttpClient(httpClientHandler, false)
            {
                BaseAddress = new Uri("https://tripletex.no/v2/"),
                DefaultRequestHeaders =
                {
                    Accept =
                    {
                        new MediaTypeWithQualityHeaderValue("application/json")
                    },
                    ExpectContinue = false
                }
            };
        }

        #region Token
        private string Token { get; set; }

        public async Task<bool> CreateSessionToken(string consumerToken, string employeeToken)
        {
            var uri =
                $"token/session/:create?consumerToken={consumerToken}&employeeToken={employeeToken}&expirationDate={DateTime.Now.AddDays(1):yyyy-MM-dd}";
            var response = await _webClient.PutAsync(uri, null, _cancellation);
            var data = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            Token = data["value"]?["token"]?.Value<string>();
            if (string.IsNullOrEmpty(Token))
                return false;
            _webClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"0:{Token}")));
            return true;
        }

        public async Task<string> CreateSessionTokenCheck(string consumerToken, string employeeToken)
        {
            var uri =
                $"token/session/:create?consumerToken={consumerToken}&employeeToken={employeeToken}&expirationDate={DateTime.Now.AddDays(1):yyyy-MM-dd}";
            var response = await _webClient.PutAsync(uri, null, _cancellation);
            var data = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
            Token = (string)data["validationMessages"][0]["message"]+ " requestId:- " + (string)data["requestId"];
            return Token;
        }
        public async Task DestroySessionToken()
        {
            if (!string.IsNullOrEmpty(Token))
            {
                var uri = $"token/session/{Token}";
                await _webClient.DeleteAsync(uri, _cancellation);
                Token = null;
            }
        }

        #endregion
        #region Customer
        public async Task<JObject> GetCustomerFromCustomerNo(int customerNo, params string[] fields)
        {
            var uri = $"customer?customerAccountNumber={customerNo}&isInactive=false&count=1";
            if (fields.Any())
                uri += $"&fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            var response = await _webClient.GetAsync(uri, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (JObject) JsonConvert.DeserializeObject<JObject>(data)["values"][0];
        }
        #endregion

        #region Product
        public async Task<JObject> GetProductFromNumber(string number, params string[] fields)
        {
            var uri = $"product?productNumber={number}&count=1";
            if (fields.Any())
                uri += $"&fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            var response = await _webClient.GetAsync(uri, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (JObject) JsonConvert.DeserializeObject<JObject>(data)["values"][0];
        }
        #endregion
        #region Product
        public async Task<int> GetProductFromNumberChecking(string number, params string[] fields)
        {
            var uri = $"product?productNumber={number}&count=1";
            if (fields.Any())
                uri += $"&fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            var response = await _webClient.GetAsync(uri, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (int)JsonConvert.DeserializeObject<JObject>(data)["count"];
        }
        #endregion
        #region Product
        public async Task<JObject> GetdepartmentFromNumber(string number, params string[] fields)
        {
            var uri = $"department?departmentNumber={number}&count=1";
            if (fields.Any())
                uri += $"&fields={string.Join(",", fields.Select(Uri.EscapeDataString))}";
            var response = await _webClient.GetAsync(uri, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (JObject)JsonConvert.DeserializeObject<JObject>(data)["values"][0];
        }
        #endregion
        #region Order
        public async Task<JObject> CreateOrder(JObject order)
        {
            var uri = "order";
            var response = await _webClient.PostAsJsonAsync(uri, order, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (JObject) JsonConvert.DeserializeObject<JObject>(data)["value"];
        }

        public async Task<JObject> CreateOrderLine(JObject line)
        {
            var uri = "order/orderline";
            var response = await _webClient.PostAsJsonAsync(uri, line, _cancellation);
            var data = await response.Content.ReadAsStringAsync();
            return (JObject) JsonConvert.DeserializeObject<JObject>(data)["value"];
        }

        public async Task<JObject> CreateInvoice(int orderNo, DateTime invoiceDate, bool sendToCustomer = true)
        {
            var url = $"order/{orderNo}/:invoice?invoiceDate={invoiceDate:yyyy-MM-dd}&sendToCustomer={sendToCustomer}";
            var response = await _webClient.PutAsync(url, new StringContent(""), _cancellation);
            return (JObject) JsonConvert.DeserializeObject<JObject>(
                await response.Content.ReadAsStringAsync())["value"];
        }
        #endregion
        

        public void Dispose()
        {
            if(!string.IsNullOrEmpty(Token))
                Trace.TraceWarning("Session not destroyed. Remember to call {0}().", nameof(DestroySessionToken));
            _webClient?.Dispose();
        }


    }
}