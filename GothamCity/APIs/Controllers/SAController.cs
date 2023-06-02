using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using System.Text;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SAController : ControllerBase
    {
        private static readonly string TenantId = "e525031f-bb1f-4659-aa0e-c0f3fbfa832f";
        private static readonly string ClientId = "474bcd92-645b-4dc0-8303-16e7b0942b2d";
        private static readonly string ClientKey = "aqi8Q~YY7UiVVubwvQsTY0-o8RtHlQDOS51zedmv";
        public static string SubscriptionID = "3d69b768-e1e8-48c3-961f-e4c6762ac658";
        public static string Location = "eastus";
        public static string Kind = "Storage";
        public static string Name = "Standard_GRS";
        public static string AccountName = "deepikasa1";
        public static string ResourceGroupName = "deepikaRG";
        public static string Response { get; set; }

        // private IActionResult TestGetResourceInfo;
        private static async Task<string> GetAccessToken(string TenantId, string ClientId, string ClientKey)
        {
            Console.WriteLine("Begin GetAccessToken");
            string authContextURL = "https://login.windows.net/" + TenantId;
            var authenticationContext = new AuthenticationContext(authContextURL);
            var credential = new ClientCredential(ClientId, ClientKey);
            var result = await authenticationContext.AcquireTokenAsync("https://management.azure.com/", credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }

        public static async Task<string> MakeRequestAsync(HttpRequestMessage getRequest, HttpClient client)
        {
            var response = await client.SendAsync(getRequest).ConfigureAwait(false);
            var responseString = string.Empty;
            try
            {
                response.EnsureSuccessStatusCode();
                responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
            }
            return responseString;
        }

        public static async Task<string> CreateStorageAccount()
        {
            string token = await GetAccessToken(TenantId, ClientId, ClientKey);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + SubscriptionID + "/resourceGroups/" + ResourceGroupName + "/providers/Microsoft.Storage/storageAccounts/" + AccountName + "?api-version=2022-09-01");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress);
            //var body = $"{{\"location\": \"{Location}\"}}{{\"kind\": \"{Kind}\"}} {{\"sku\": {{\"name\": \"{Name}\"}}";
            var body = "{'kind':'Storage','location':'eastus','sku':{'name':'Standard_GRS'}}".Replace("'", "\"");
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            request.Content = content;
            var response = await MakeRequestAsync(request, client);
            Response = response;
            return Response;
        }

        [HttpPut]
        public async Task<string> CreateSA()
        {
           return await CreateStorageAccount();
        }
    }
}

