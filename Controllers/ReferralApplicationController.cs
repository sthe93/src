using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UJStudentGorvenanceStudentWeb.Helper;
using UJStudentGorvenanceStudentWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using static UJStudentGorvenanceStudentWeb.Helper.AuthModel;
using Microsoft.AspNetCore.Authentication;

namespace UJStudentGorvenanceStudentWeb.Controllers
{
    [Authorize]
    public class ReferralApplicationController(IConfiguration configuration, IApplicationSession applicationSession)
        : Controller
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public IActionResult ReferralApplication()
        {
            var token = applicationSession.Token;
            var currentUserFullName = applicationSession.FullName;
            var currentUserStaffNumber = applicationSession.StaffNumber;

            ViewBag.CurrentUserFullName = currentUserFullName;
            ViewBag.CurrentUserStaffNumber = currentUserStaffNumber;

            var response = HttpRequests.Request("ReferralApplication/getUserApplications", HttpVerb.Get, null, token);

            
                    if (response.Message != null && response.IsValid)
                    {
                        var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);
                        if (responseObj?.IsValid == true)
                        {
                            var referralApplications = JsonConvert.DeserializeObject<List<ReferralApplicationDto>>(JsonConvert.SerializeObject(responseObj.Data));
                            if (referralApplications is { Count: > 0 })
                            {
                                return View(referralApplications);
                            }
                        }
                    }

          

            return View(new List<ReferralApplicationDto>());
        }

        [HttpPost]
        public async Task<IActionResult?> CreateApplication(ReferralApplicationDto referralApplicationDto, List<IFormFile> documents)
        {
            if (documents?.Count > 0)
            {
                referralApplicationDto.Documents = await ProcessDocuments(documents);
            }

            var token = applicationSession.Token;
            var json = JsonConvert.SerializeObject(referralApplicationDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = HttpRequests.Request("ReferralApplication/CreateApplication", HttpVerb.Post, content, token);

            if (!response.IsValid)
            {
                return Json(new { success = false, response.Message});
            }

            if (response.Message == null) return null;
            var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);
            return Json(new { success = responseObj?.IsValid == true, message = responseObj?.Message ?? "Failed to add Referral application." });

        }

        private static async Task<List<ReferralApplicationsDocumentDto>> ProcessDocuments(IEnumerable<IFormFile> documents)
        {
            var documentDos = new List<ReferralApplicationsDocumentDto>();
            foreach (var file in documents)
            {
                if (file.Length <= 0) continue;

                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);

                    // Validate that we actually have data
                    if (memoryStream.Length == 0)
                    {
                        continue;
                    }

                    var base64String = Convert.ToBase64String(memoryStream.ToArray());

                    // Basic validation of Base64 string
                    if (string.IsNullOrWhiteSpace(base64String) || base64String.Length % 4 != 0)
                    {
                        continue;
                    }

                    documentDos.Add(new ReferralApplicationsDocumentDto
                    {
                        DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
                        DocumentData = base64String,
                        DocumentTypeName = "application/pdf", // Force PDF type
                    });
                }
                catch (Exception ex)
                {
                    // Log the exception but continue processing other files
                    Console.WriteLine($"Error processing file {file.FileName}: {ex.Message}");
                    continue;
                }
            }
            return documentDos;
        }

    }
}
