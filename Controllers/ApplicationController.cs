using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UJStudentGorvenanceStudentWeb.Helper;
using UJStudentGorvenanceStudentWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using static UJStudentGorvenanceStudentWeb.Helper.AuthModel;
using Microsoft.AspNetCore.Authentication;
using StudentGovernanceStudentWeb.Helper;
using StudentGovernanceStudentWeb.Models;

namespace UJStudentGorvenanceStudentWeb.Controllers
{
    [Authorize]
    public class ApplicationController(IConfiguration configuration, IApplicationSession applicationSession)
        : Controller
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public Task<IActionResult> ApplicationForm()
        {
            var token = applicationSession.Token;
            var response = HttpRequests.Request("Application/getAllGuardianshipTypes", HttpVerb.Get, null, token);

            var model = new ApplicationFormViewModel
            {
                CurrentStudentNumber = applicationSession.StudentNumber,
                CurrentStudentName = applicationSession.StudentName,
                CurrentStudentIdNumber = applicationSession.StudentIdNumber
            };

            if (response.IsValid)
            {
                try
                {
                    if (response.Message != null)
                    {
                        var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);
                        if (responseObj is { IsValid: true, Data: not null })
                        {
                            model.GuardianshipTypes = JsonConvert.DeserializeObject<List<GuardianshipViewModel>>(JsonConvert.SerializeObject(responseObj.Data));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = responseObj?.Message ?? "No guardianship types found.";
                        }
                    }
                }
                catch (JsonException)
                {
                    TempData["ErrorMessage"] = "Error processing response data.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = response.Message;
            }

            return Task.FromResult<IActionResult>(View(model));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult?> AddApplication(ApplicationDto applicationDto, List<IFormFile> documents)
        {
            if (documents?.Count > 0)
            {
                applicationDto.Documents = await ProcessDocuments(documents);
            }

            var token = applicationSession.Token;
            var json = JsonConvert.SerializeObject(applicationDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = HttpRequests.Request("Application/AddApplication", HttpVerb.Post, content, token);

            if (!response.IsValid)
            {
                return Json(new { success = false, response.Message });
            }

            if (response.Message == null) return null;

            var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);

            return responseObj?.IsValid == true ?
                Json(new
                {
                    success = responseObj?.IsValid == true,
                    message = responseObj?.Message, 
                    redirectUrl = Url.Action("SubmittedApplications", "Application", new { studentNumber = applicationSession.StudentNumber })
                }) : Json(new { success = false, message = responseObj?.Message ?? "Failed to add application." });
        }

    
        private async Task<List<DocumentDto>> ProcessDocuments(IEnumerable<IFormFile> documents)
        {
            var documentDos = new List<DocumentDto>();

            var documentTypeIds = Request.Form["documentTypeIds[]"];

            var index = 0;
            foreach (var file in documents)
            {
                if (file.Length <= 0) continue;
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var documentTypeId = 0;
                if (index < documentTypeIds.Count)
                {
                    documentTypeId = int.TryParse(documentTypeIds[index], out var parsedId) ? parsedId : 0;
                }

                documentDos.Add(new DocumentDto
                {
                    FileName = file.FileName,
                    Document1 = Convert.ToBase64String(memoryStream.ToArray()),
                    DocumentStatus = "Pending",
                    IsReUpload = false,
                    DocumentTypeId = documentTypeId 
                });

                index++;
            }

            return documentDos;
        }
        [HttpGet]
        public IActionResult SubmittedApplications(string? studentNumber = null, string? applicationType = null)
        {
            studentNumber ??= applicationSession.StudentNumber;

            Console.WriteLine($"=== DEBUG: SubmittedApplications START ===");
            Console.WriteLine($"Student: {studentNumber}");
            Console.WriteLine($"ApplicationType parameter: '{applicationType}'");
            Console.WriteLine($"Session ApplicationType: '{applicationSession.ApplicationType}'");

            var token = applicationSession.Token;

            // Determine if this is a login-only flow or apply flow
            bool isLoginOnlyFlow = string.IsNullOrEmpty(applicationType) ||
                                  applicationType == "ALL" ||
                                  applicationType == "LOGIN_ONLY" ||
                                  // Check if session has LOGIN_ONLY or ALL
                                  applicationSession.ApplicationType == "LOGIN_ONLY" ||
                                  applicationSession.ApplicationType == "ALL";

            Console.WriteLine($"Is Login-Only Flow: {isLoginOnlyFlow}");

            string url;
            if (isLoginOnlyFlow)
            {
                // For login-only flow: Get ALL applications
                url = $"Application/getApplicationByStudentNumberAndType/{studentNumber}/";
                Console.WriteLine($"DEBUG: Login-only flow - Getting ALL applications");
            }
            else
            {
                // For apply flow: Get only applications for the specific type
                // Use the applicationType parameter OR session ApplicationType
                var specificApplicationType = !string.IsNullOrEmpty(applicationType) &&
                                              applicationType != "ALL" &&
                                              applicationType != "LOGIN_ONLY"
                    ? applicationType
                    : applicationSession.ApplicationType;

                Console.WriteLine($"DEBUG: Apply flow - Specific Application Type: '{specificApplicationType}'");

                // URL encode the application type to handle spaces
                var encodedApplicationType = Uri.EscapeDataString(specificApplicationType);
                url = $"Application/getApplicationByStudentNumberAndType/{studentNumber}/{encodedApplicationType}";
                Console.WriteLine($"DEBUG: Apply flow - URL encoded type: {encodedApplicationType}");
            }

            Console.WriteLine($"DEBUG: Final URL: {url}");

            var response = HttpRequests.Request(url, HttpVerb.Get, null, token);

            Console.WriteLine($"DEBUG: API Response - IsValid: {response.IsValid}");
            Console.WriteLine($"DEBUG: API Response Length: {response.Message?.Length ?? 0}");

            if (response.IsValid && !string.IsNullOrEmpty(response.Message))
            {
                try
                {
                    var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);

                    if (responseObj?.IsValid == true && responseObj.Data != null)
                    {
                        var applications = JsonConvert.DeserializeObject<List<ApplicationDto>>(JsonConvert.SerializeObject(responseObj.Data));
                        Console.WriteLine($"DEBUG: Found {applications?.Count ?? 0} applications");

                        // Log application types found
                        if (applications != null)
                        {
                            var distinctTypes = applications.Select(a => a.ApplicationType).Distinct();
                            Console.WriteLine($"DEBUG: Application Types Found: {string.Join(", ", distinctTypes)}");
                        }

                        ViewBag.ApplicationType = isLoginOnlyFlow ? "ALL" : applicationType ?? applicationSession.ApplicationType;
                        ViewBag.IsLoginOnlyFlow = isLoginOnlyFlow;
                        ViewBag.StudentNumber = studentNumber;
                        ViewBag.StudentName = applicationSession.StudentName;

                        return View("SubmittedApplications", applications ?? new List<ApplicationDto>());
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: API response not valid");
                        TempData["ErrorMessage"] = responseObj?.Message ?? "No applications found.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DEBUG: Error processing response: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                }
            }
            else
            {
                Console.WriteLine($"DEBUG: API call failed: {response.Message}");
                TempData["ErrorMessage"] = response.Message ?? "Failed to retrieve applications.";
            }

            ViewBag.ApplicationType = isLoginOnlyFlow ? "ALL" : applicationType ?? applicationSession.ApplicationType;
            ViewBag.IsLoginOnlyFlow = isLoginOnlyFlow;
            ViewBag.StudentNumber = studentNumber;
            ViewBag.StudentName = applicationSession.StudentName;

            return View("SubmittedApplications", new List<ApplicationDto>());
        }


        [HttpGet]
        public Task<IActionResult> GetAllGuardianshipTypes()
        {
            var token = applicationSession.Token;
            var response = HttpRequests.Request("Application/getAllGuardianshipTypes", HttpVerb.Get, null, token);

            if (!response.IsValid) return Task.FromResult<IActionResult>(Json(new { isValid = false, message = response.Message }));
            try
            {
                if (response.Message != null)
                {
                    var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);
                    if (responseObj?.IsValid != true)
                        return Task.FromResult<IActionResult>(Json(new
                            { isValid = false, message = responseObj?.Message ?? "No guardianship types found." }));
                    var guardianshipTypes = JsonConvert.DeserializeObject<List<GuardianshipViewModel>>(JsonConvert.SerializeObject(responseObj.Data));
                    return Task.FromResult<IActionResult>(Json(new { isValid = true, data = guardianshipTypes }));
                }
            }
            catch (JsonException ex)
            {
                return Task.FromResult<IActionResult>(Json(new { isValid = false, message = "Error processing response data. " + ex.Message }));
            }
            return Task.FromResult<IActionResult>(Json(new { isValid = false, message = response.Message }));
        }
        [HttpGet]
        public Task<IActionResult> GetApplicationDocuments(int applicationId)
        {
            var token = applicationSession.Token;
            var response = HttpRequests.Request($"Application/getApplicationDocuments/{applicationId}", HttpVerb.Get, null, token);

            if (!response.IsValid)
                return Task.FromResult<IActionResult>(Json(new { isValid = false, message = response.Message }));

            try
            {
                if (response.Message != null)
                {
                    var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);
                    if (responseObj?.IsValid != true)
                        return Task.FromResult<IActionResult>(Json(new
                        {
                            isValid = false,
                            message = responseObj?.Message ?? "No documents found for the provided application ID."
                        }));

                    var documents = JsonConvert.DeserializeObject<List<DocumentDetailDto>>(JsonConvert.SerializeObject(responseObj.Data));

                    // If the backend doesn't return DocumentBase64, you'll need to fetch it separately
                    // For now, let's assume your backend API returns the document data
                    // If not, you'll need to make additional API calls to get each document

                    return Task.FromResult<IActionResult>(Json(new { isValid = true, data = documents }));
                }
            }
            catch (JsonException ex)
            {
                return Task.FromResult<IActionResult>(Json(new { isValid = false, message = "Error processing response data. " + ex.Message }));
            }

            return Task.FromResult<IActionResult>(Json(new { isValid = false, message = response.Message }));
        }
        [HttpPost]
        public async Task<IActionResult> EditDocument(int applicationId, int documentId, List<IFormFile> updatedDocuments)
        {
            if (updatedDocuments == null || updatedDocuments.Count == 0)
            {
                return Json(new { success = false, message = "No documents provided for upload." });
            }

     
            var processedDocuments = await ProcessDocuments(updatedDocuments);

        
            var editDocumentDto = new EditDocumentDto
            {
                ApplicationId = applicationId,
                DocumentId = documentId,
                Documents = processedDocuments.Select(doc => new DocumentDto
                {
                    DocumentId = documentId, 
                    FileName = doc.FileName,
                    Document1 = doc.Document1,
                    DocumentStatus = "Pending", 
                }).ToList()
            };

            var token = applicationSession.Token;
            var json = JsonConvert.SerializeObject(editDocumentDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

           
            var response = HttpRequests.Request("Application/editDocument", HttpVerb.Put, content, token);

            if (!response.IsValid)
            {
                return Json(new { success = false, message = response.Message });
            }

            var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);

            return responseObj?.IsValid == true
                ? Json(new
                {
                    success = true,
                    message = responseObj.Message,
                    redirectUrl = Url.Action("SubmittedApplications", "Application", new { studentNumber = applicationSession.StudentNumber })
                })
                : Json(new { success = false, message = responseObj?.Message ?? "Failed to edit document." });
        }


        [HttpGet]
        public async Task<IActionResult> GetDeclineReasons(int applicationId)
        {
            try
            {
                var token = applicationSession.Token;
                var response = HttpRequests.Request($"Application/getDeclineReasons/{applicationId}", HttpVerb.Get, null, token);

                Console.WriteLine($"DEBUG: GetDeclineReasons for Application {applicationId}");
                Console.WriteLine($"DEBUG: API Response - IsValid: {response.IsValid}");
                Console.WriteLine($"DEBUG: API Response Message Length: {response.Message?.Length ?? 0}");

                if (response.IsValid && !string.IsNullOrEmpty(response.Message))
                {
                    Console.WriteLine($"DEBUG: Response preview: {response.Message.Substring(0, Math.Min(500, response.Message.Length))}");

                    var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message);

                    if (responseObj?.IsValid == true && responseObj.Data != null)
                    {
                        Console.WriteLine($"DEBUG: Successfully deserialized response");

                        // Try different deserialization approaches
                        try
                        {
                            // First try: Deserialize as List<DeclineReason>
                            var declineReasons = JsonConvert.DeserializeObject<List<DeclineReason>>(JsonConvert.SerializeObject(responseObj.Data));

                            if (declineReasons != null && declineReasons.Count > 0)
                            {
                                var declineReasonStrings = declineReasons.Select(dr => dr.Reason1).ToList();
                                Console.WriteLine($"DEBUG: Found {declineReasonStrings.Count} decline reasons: {string.Join(", ", declineReasonStrings)}");
                                return Json(new { success = true, data = declineReasonStrings });
                            }
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine($"DEBUG: First deserialization failed: {ex1.Message}");

                            try
                            {
                                // Second try: Deserialize as List<string>
                                var declineReasons = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(responseObj.Data));

                                if (declineReasons != null && declineReasons.Count > 0)
                                {
                                    Console.WriteLine($"DEBUG: Found {declineReasons.Count} decline reasons as strings: {string.Join(", ", declineReasons)}");
                                    return Json(new { success = true, data = declineReasons });
                                }
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine($"DEBUG: Second deserialization failed: {ex2.Message}");

                                try
                                {
                                    // Third try: Handle as dynamic object
                                    var data = JsonConvert.SerializeObject(responseObj.Data);
                                    var dynamicData = JsonConvert.DeserializeObject<dynamic>(data);

                                    if (dynamicData != null)
                                    {
                                        var reasonsList = new List<string>();

                                        // Check if it's an array
                                        if (dynamicData is Newtonsoft.Json.Linq.JArray array)
                                        {
                                            foreach (var item in array)
                                            {
                                                // Try to get Reason1 property or direct string
                                                if (item["reason1"] != null)
                                                {
                                                    reasonsList.Add(item["reason1"].ToString());
                                                }
                                                else if (item["Reason1"] != null)
                                                {
                                                    reasonsList.Add(item["Reason1"].ToString());
                                                }
                                                else if (item["reason"] != null)
                                                {
                                                    reasonsList.Add(item["reason"].ToString());
                                                }
                                                else if (item["Reason"] != null)
                                                {
                                                    reasonsList.Add(item["Reason"].ToString());
                                                }
                                                else
                                                {
                                                    reasonsList.Add(item.ToString());
                                                }
                                            }
                                        }

                                        if (reasonsList.Count > 0)
                                        {
                                            Console.WriteLine($"DEBUG: Found {reasonsList.Count} decline reasons dynamically: {string.Join(", ", reasonsList)}");
                                            return Json(new { success = true, data = reasonsList });
                                        }
                                    }
                                }
                                catch (Exception ex3)
                                {
                                    Console.WriteLine($"DEBUG: Third deserialization failed: {ex3.Message}");
                                }
                            }
                        }

                        Console.WriteLine($"DEBUG: No decline reasons found or unable to parse");
                        return Json(new { success = false, message = "No decline reasons found or unable to parse response." });
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Response not valid or data null");
                        return Json(new { success = false, message = responseObj?.Message ?? "No decline reasons found." });
                    }
                }

                Console.WriteLine($"DEBUG: API call failed or empty response");
                return Json(new { success = false, message = response.Message ?? "Failed to fetch decline reasons." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in GetDeclineReasons: {ex.Message}");
                Console.WriteLine($"DEBUG: StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }




        [AllowAnonymous]
        [HttpPost]
        public IActionResult SetApplicationType([FromBody] ApplicationTypeModel? model)
        {
            if (model == null || string.IsNullOrEmpty(model.ApplicationType))
            {
                return BadRequest("Invalid application type");
            }
            // Handle login-only flow
            if (model.ApplicationType == "LOGIN_ONLY")
            {
                applicationSession.ApplicationType = "ALL";
            }
            else
            {
                applicationSession.ApplicationType = model.ApplicationType;
            }
            applicationSession.ApplicationType = model.ApplicationType;

            return Ok();
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetApplicationType()
        {
            var applicationType = applicationSession.ApplicationType;

            // For login-only flow, return "ALL" or empty
            if (string.IsNullOrEmpty(applicationType) || applicationType == "LOGIN_ONLY")
            {
                return Json(new { applicationType = "ALL" });
            }

            return Json(new { applicationType });
        }

    }
}