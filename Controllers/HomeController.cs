using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using UJStudentGorvenanceStudentWeb.Helper;
using UJStudentGorvenanceStudentWeb.Models;
using static UJStudentGorvenanceStudentWeb.Helper.AuthModel;

namespace UJStudentGorvenanceStudentWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> HasStudentApplications(string studentNumber)
        {
            try
            {
              
                var response =  HttpRequests.Request($"StudentAccount/hasStudentApplications/{studentNumber}", HttpVerb.Get, null, null);

                if (!response.IsValid) return Json(new { isValid = false, message = response.Message });

                if (response.Message != null)
                {
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(response.Message);

                    if (responseObj == null)
                    {
                        return Json(new { isValid = false, message = "Response data is invalid." });
                    }

                    bool hasApplications = responseObj.Data;

                    return Json(new { isValid = true, hasApplications = hasApplications, message = responseObj.Message });
                }
            }
            catch (JsonException ex)
            {
                return Json(new { isValid = false, message = "Error processing response data: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, message = "An error occurred: " + ex.Message });
            }

            return Json(new { isValid = false, message = "Unexpected error." });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> IsApplicationOpen(string applicationType)
        {
            try
            {
                var response =  HttpRequests.Request($"StudentAccount/isApplicationOpen/{applicationType}", HttpVerb.Get, null, null);

                if (!response.IsValid) return Json(new { isValid = false, message = response.Message });
                if (response.Message != null)
                {
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(response.Message);

                    if (responseObj == null) return Json(new { isValid = false, message = "Response data is invalid." });
                    bool isOpen = responseObj.isOpen;
                    bool isValid = responseObj.isValid;

                    return Json(new { isValid = isValid, isOpen = isOpen });
                }
            }
            catch (JsonException ex)
            {
                return Json(new { isValid = false, message = "Error processing response data: " + ex.Message });
            }

            return Json(new { isValid = false, message = "Unexpected error." });
        }
    }
}
