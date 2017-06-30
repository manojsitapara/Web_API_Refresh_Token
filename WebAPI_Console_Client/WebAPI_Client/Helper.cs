using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI_Client
{


    static class Helper
    {




        

        public static string BaseAddress = ConfigurationManager.AppSettings["WebAPIBaseAddress"];



        public static string GetAuthenticatedProviderUserName(string accesToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesToken);
                var authorizedResponse = client.GetAsync(BaseAddress + "api/data/GetProviderUserName").Result;
                if (authorizedResponse.StatusCode == HttpStatusCode.OK)
                {
                    var resultString = authorizedResponse.Content.ReadAsStringAsync().Result;
                    return resultString;
                }
                else
                {
                    return "Error: Received status code: " + authorizedResponse.StatusCode;
                }

            }
        }


        public static string GetAuthenticatedPatientUserName(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var authorizedResponse = client.GetAsync(BaseAddress + "api/data/GetPatientUserName").Result;
                if (authorizedResponse.StatusCode == HttpStatusCode.OK)
                {
                    var resultString = authorizedResponse.Content.ReadAsStringAsync().Result;
                    return resultString;
                }
                else
                {
                    return "Error: Received status code: " + authorizedResponse.StatusCode;
                }


            }
        }


        public static string GetGuestUserName()
        {
            using (var client = new HttpClient())
            {

                var authorizedResponse = client.GetAsync(BaseAddress + "api/data/GetGuestUserName").Result;
                if (authorizedResponse.StatusCode == HttpStatusCode.OK)
                {
                    var resultString = authorizedResponse.Content.ReadAsStringAsync().Result;
                    return resultString;
                }
                else
                {
                    throw new Exception("Received status code: " + authorizedResponse.StatusCode);
                }


            }
        }










    }
}
