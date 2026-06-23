using Microsoft.Extensions.Configuration;
using UJStudentGorvenanceStudentWeb.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using static UJStudentGorvenanceStudentWeb.Helper.AuthModel;

namespace UJStudentGorvenanceStudentWeb.Helper
{
    public class HttpRequests
    {
        public static HttpClient HttpClient(string? token)
        {
            var authValue = new AuthenticationHeaderValue("Bearer", token);

            var client = new HttpClient()
            {
                DefaultRequestHeaders = { Authorization = authValue }
            };
            return client;
        }

        // Updated method with flowType and applicationType parameters
        public static ResponseDto GetJsonToken(string studentNumber, string idNumber, string flowType = "login", string applicationType = "")
        {
            using var client = HttpClient(null);
            // Add both flowType and applicationType as query parameters
            var url = AppSettings.SrcApi + $"StudentAccount/login/studentNumber/{studentNumber}/idNumber/{idNumber}?flowType={flowType}&applicationType={applicationType}";
            var response = client.PostAsync(url, null).Result;
            return response.StatusCode != HttpStatusCode.OK ? new ResponseDto() { IsValid = false, Message = response?.Content.ReadAsStringAsync().Result } : new ResponseDto() { IsValid = true, Message = response?.Content.ReadAsStringAsync().Result };
        }

        // Keep the original method for backward compatibility
        public static ResponseDto GetJsonToken(string studentNumber, string idNumber)
        {
            return GetJsonToken(studentNumber, idNumber, "login", ""); // Default to login flow with no application type
        }

        public static ResponseDto GetJsonTokenForStaff(string username, string password)
        {
            using var client = HttpClient(null);
            var response = client.PostAsync(AppSettings.SrcApi + $"Account/login/username/{username}/password/{password}", null).Result;
            return response.StatusCode != HttpStatusCode.OK ? new ResponseDto() { IsValid = false, Message = response?.Content.ReadAsStringAsync().Result } : new ResponseDto() { IsValid = true, Message = response?.Content.ReadAsStringAsync().Result };
        }

        public static ResponseDto Request(string controller, string httpVerb, HttpContent? payLoad, string? token)
        {
            try
            {
                using var client = HttpClient(token);
                var url = AppSettings.SrcApi;

                var response = httpVerb switch
                {
                    HttpVerb.Post => client.PostAsync(url + controller, payLoad).Result,
                    HttpVerb.Put => client.PutAsync(url + controller, payLoad).Result,
                    HttpVerb.Get => client.GetAsync(url + controller).Result,
                    HttpVerb.Delete => client.DeleteAsync(url + controller).Result,
                    _ => throw new NotSupportedException($"HTTP verb {httpVerb} is not supported.")
                };

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return new ResponseDto
                    {
                        IsValid = true,
                        Message = responseContent
                    };
                }

                return new ResponseDto
                {
                    IsValid = false,
                    Message = $"Error: {response.ReasonPhrase}, Content: {responseContent}"
                };
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                return new ResponseDto
                {
                    IsValid = false,
                    Message = $"Exception: {e.Message}"
                };
            }
        }
    }
}