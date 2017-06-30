using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace WebAPI_Client
{
    class Program
    {
        public static string BaseAddress = ConfigurationManager.AppSettings["WebAPIBaseAddress"];
        public static string AccessToken;
        public static string RefreshToken;
        public static string TokenExpiresInSeconds;
        public static string TokenExpiresInDateTime;
        public static string ClientId;
        public static string ClientSecretKey;
        public static string userName;

        static void Main(string[] args)
        {



            Console.WriteLine("Enter Username: ");
            userName = "john@cxcnetwork.com";
            Console.WriteLine("Enter Password: ");
            string password = "Test@123";
            Console.WriteLine("Enter ClientId: ");
            ClientId = "ConsoleApp";
            //Console.WriteLine("Enter ClientSecret: ");
            ClientSecretKey = "SECRET";


            bool isUserLoggedIn = Login(userName, password, ClientId, ClientSecretKey);
            if (isUserLoggedIn == true)
            {
                Console.WriteLine("You have sucessfully logged in !!");
            }
            else
            {
                Console.WriteLine("Login is failed");
                Console.ReadLine();
                return;
            }



            if (isUserLoggedIn)
                Console.WriteLine("You have sucessfully logged in");
            else
                Console.WriteLine("Login failed");

            Console.WriteLine("1. Login");
            Console.WriteLine("2. Get Authenticated Provider User Name (Authentication Required & Authenticated UserRole should be Provider)");
            Console.WriteLine("3. Get Authenticated Patient User Name (Authentication Required & Authenticated UserRole should be Patient)");
            Console.WriteLine("4. Get Guest User Name (Authentication Not Required)");
            Console.WriteLine("5. Change Password (Authentication Required)");
            Console.WriteLine("6. Reset Password");
            Console.WriteLine("0. Logout");


            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Enter option number from above: ");
            string caseNumber = Console.ReadLine();
            ExecuteTask(caseNumber);
            

        }

        public static void ExecuteTask(string caseNumber)
        {
            if (caseNumber == "2")
            {
                string authenticatedProviderUsername = Helper.GetAuthenticatedProviderUserName(GetAccessToken());
                Console.WriteLine(authenticatedProviderUsername);
                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }
            if (caseNumber == "3")
            {
                string authenticatedPatientUsername = Helper.GetAuthenticatedPatientUserName(GetAccessToken());
                Console.WriteLine(authenticatedPatientUsername);
                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }
            if (caseNumber == "4")
            {
                string authenticatedProviderUsername = Helper.GetGuestUserName();
                Console.WriteLine(authenticatedProviderUsername);
                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }
            if (caseNumber == "5")
            {
                Console.WriteLine("Enter old Password: ");
                string oldPassword = Console.ReadLine();
                Console.WriteLine("Enter new Password: ");
                string newPassword = Console.ReadLine();
                Console.WriteLine("Enter confirm Password: ");
                string confirmPassword = Console.ReadLine();
                Console.WriteLine(ChangePassword(oldPassword, newPassword, confirmPassword));
                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }
            if (caseNumber == "6")
            {
                Console.WriteLine("Enter username: ");
                string userName = Console.ReadLine();
                Console.WriteLine(ForgetPassword(userName));
                Console.WriteLine("Enter temporary password: ");
                string tempPassword = Console.ReadLine();
                Console.WriteLine("Enter new password: ");
                string newPassword = Console.ReadLine();
                Console.WriteLine("Enter confirm password: ");
                string confirmNewPassword = Console.ReadLine();

                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }

            if (caseNumber == "0")
            {
                //AccessToken = "";
                Console.WriteLine(Logout(RefreshToken, userName));
                Console.WriteLine("Enter you choice: ");
                caseNumber = Console.ReadLine();
                ExecuteTask(caseNumber);
            }


            Console.ReadLine();

        }
        public static bool Login(string userName, string password, string clientId, string clientSecretKey)
        {

            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "password" ),
                            new KeyValuePair<string, string>( "userName", userName ),
                            new KeyValuePair<string, string> ( "password", password ),
                            new KeyValuePair<string, string> ( "client_id", clientId ),
                            new KeyValuePair<string, string> ( "client_secret", clientSecretKey )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(BaseAddress + "token", content).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    AccessToken = (string)responseJson["access_token"];
                    RefreshToken = (string)responseJson["refresh_token"];
                    string accessTokenExpiresIn = (string)responseJson["expires_in"];
                    int tokenExpiresInSeconds = Convert.ToInt32(accessTokenExpiresIn);
                    TokenExpiresInDateTime = DateTime.Now.AddSeconds(tokenExpiresInSeconds).ToString("MM/dd/yyyy HH:mm:ss");

                    return true;
                }
                return false;
            }
        }


        

        private static bool IsAccessTokenValid()
        {


            
            DateTime currentDateTime = DateTime.Now;
            DateTime accessTokenExpireDateTime = DateTime.ParseExact(TokenExpiresInDateTime,
                                 "MM/dd/yyyy HH:mm:ss",
                                 new CultureInfo("en-US"));
            if (accessTokenExpireDateTime > currentDateTime)
            {
                return true;
            }
            return false;

        }

        public static bool RenewAccessToken()
        {
            
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "grant_type", "refresh_token" ),
                            new KeyValuePair<string, string>( "refresh_token", RefreshToken ),
                            new KeyValuePair<string, string> ( "client_id", ClientId ),
                            new KeyValuePair<string, string> ( "client_secret", ClientSecretKey )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(BaseAddress + "token", content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseJson = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    AccessToken = (string)responseJson["access_token"];
                    RefreshToken = (string)responseJson["refresh_token"];
                    TokenExpiresInSeconds = (string)responseJson["expires_in"];
                    int tokenExpiresInSeconds = Convert.ToInt32(TokenExpiresInSeconds);
                    TokenExpiresInDateTime = DateTime.Now.AddSeconds(tokenExpiresInSeconds).ToString("MM/dd/yyyy HH:mm:ss");

                    return true;
                }
                else
                {
                    throw new Exception("Error Code: " + response.StatusCode + " " + response.ReasonPhrase);
                }

            }
        }


        public static string ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>( "oldPassword", oldPassword ),
                            new KeyValuePair<string, string>( "newPassword", newPassword ),
                            new KeyValuePair<string, string> ( "ConfirmPassword", confirmPassword  )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
                var response = client.PostAsync(BaseAddress + "api/data/ChangePassword", content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return "Password sucessfully changed";
                }
                return "Error: Response Code: " + response.StatusCode + " Password changed failed";

            }
        }


        //public static string ChangeResetPassword(string userName, string tempPassword, string newPassword, string confirmPassword)
        //{
        //    var pairs = new List<KeyValuePair<string, string>>
        //                {
        //                    new KeyValuePair<string, string>( "oldPassword", oldPassword ),
        //                    new KeyValuePair<string, string>( "newPassword", newPassword ),
        //                    new KeyValuePair<string, string> ( "ConfirmPassword", confirmPassword  )
        //                };
        //    var content = new FormUrlEncodedContent(pairs);
        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
        //        var response = client.PostAsync(BaseAddress + "api/data/ChangePassword", content).Result;
        //        if (response.StatusCode == HttpStatusCode.OK)
        //        {
        //            return "Password sucessfully changed";
        //        }
        //        return "Error: Response Code: " + response.StatusCode + " Password changed failed";

        //    }
        //}

        public static string ForgetPassword(string userName)
        {
            var pairs = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string> ( "userName", userName  )
                        };
            var content = new FormUrlEncodedContent(pairs);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());
                var response = client.PostAsync(BaseAddress + "api/data/ForgetPassword?userName=" + userName, content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return "Password sucessfully changed";
                }
                return "Error: Response Code: " + response.StatusCode + " Password changed failed";

            }
        }


        public static string Logout(string refreshToken, string userName)
        {
            StringContent content = new StringContent("");
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(BaseAddress + "api/data/Logout?refreshToken=" + refreshToken + "&userName=" + userName, content).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return "You have sucessfully logged out";
                }
                return "Error: Response Code: " + response.StatusCode + " Password changed failed";

            }
        }

        public static string GetAccessToken()
        {
            string tokenValue = string.Empty;

            if (IsAccessTokenValid())
            {
                tokenValue = AccessToken;
            }
            else
            {

                RenewAccessToken();
                tokenValue = AccessToken;



            }

            return tokenValue;
        }

    }
}
