using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UJStudentGorvenanceStudentWeb.Helper;
using UJStudentGorvenanceStudentWeb.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using static UJStudentGorvenanceStudentWeb.Helper.AuthModel;
using System.Data;
using StudentGovernanceStudentWeb.Helper;

namespace UJStudentGorvenanceStudentWeb.Controllers
{
    [Authorize]
    public class StudentAccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IApplicationSession _applicationSession;

        public StudentAccountController(IConfiguration configuration, IApplicationSession applicationSession)
        {
            _configuration = configuration;
            _applicationSession = applicationSession;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return PartialView("_StudentLoginModal");
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult?> Login(StudentLoginViewModel model, string? returnUrl = null)
        {
            Console.WriteLine($"DEBUG: Login started - Student: {model.StudentNumber}");

            // Get selectedApplicationType from form data
            var selectedApplicationType = HttpContext.Request.Form["selectedApplicationType"].FirstOrDefault();
            Console.WriteLine($"DEBUG: Selected Application Type from form: '{selectedApplicationType}'");

            // Determine flow type
            bool isLoginOnlyFlow = string.IsNullOrEmpty(selectedApplicationType) || selectedApplicationType == "LOGIN_ONLY";
            string flowType = isLoginOnlyFlow ? "login" : "apply";

            Console.WriteLine($"DEBUG: Flow Type: {flowType}, IsLoginOnly: {isLoginOnlyFlow}");

            // Pass flow type AND application type to API
            var response = HttpRequests.GetJsonToken(model.StudentNumber, model.IdNumber, flowType, selectedApplicationType);

            if (!response.IsValid || string.IsNullOrEmpty(response.Message))
            {
                var parsedResponse = JsonConvert.DeserializeObject<dynamic>(response.Message);
                string errorMessage = parsedResponse?.message ?? "An error occurred. Please try again.";
                Console.WriteLine($"DEBUG: Token request failed - {errorMessage}");
                return Json(new { success = false, errorMessage });
            }

            try
            {
                var responseObj = JsonConvert.DeserializeObject<LoginResponseObj>(response.Message);
                if (responseObj is not { IsValid: true })
                {
                    var parsedResponse = JsonConvert.DeserializeObject<dynamic>(response.Message);
                    string errorMessage = parsedResponse?.message ?? "An error occurred. Please try again.";
                    Console.WriteLine($"DEBUG: Token validation failed - {errorMessage}");
                    return Json(new { success = false, errorMessage });
                }

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(responseObj.Message);
                var studentNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "studentNumber")?.Value;
                var studentName = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var studentIdNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

                Console.WriteLine($"DEBUG: Student authenticated - Number: {studentNumber}, Name: {studentName}");

                if (string.IsNullOrEmpty(studentNumber))
                    return Json(new { success = false, errorMessage = "Failed to retrieve student information." });

                _applicationSession.Token = responseObj.Message;
                _applicationSession.StudentNumber = studentNumber;
                _applicationSession.StudentName = studentName;
                _applicationSession.StudentIdNumber = studentIdNumber;

                // CRITICAL FIX: For login-only flow, we need to determine the application type from existing applications
                if (!isLoginOnlyFlow && !string.IsNullOrEmpty(selectedApplicationType))
                {
                    // Apply flow - use the selected application type
                    _applicationSession.ApplicationType = selectedApplicationType;
                    Console.WriteLine($"DEBUG: Set ApplicationType in session (Apply flow): {selectedApplicationType}");
                }
                else
                {
                    // Login-only flow - we need to get the application type from existing applications
                    Console.WriteLine($"DEBUG: Login-only flow - determining application type from existing applications");

                    var initialAppsCheck = await CheckStudentApplications(studentNumber);
                    if (initialAppsCheck.isValid && initialAppsCheck.applicationTypes != null && initialAppsCheck.applicationTypes.Any())
                    {
                        // Use the first application type found, or implement logic to handle multiple types
                        var applicationType = initialAppsCheck.applicationTypes.First();
                        _applicationSession.ApplicationType = applicationType;
                        Console.WriteLine($"DEBUG: Set ApplicationType from existing applications: {applicationType}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: No existing applications found for login-only flow");
                        // If no applications found, we might need a default or let the user choose
                    }
                }

                var claims = new[]
                {
            new Claim("studentName", studentName ?? string.Empty),
            new Claim("studentNumber", studentNumber),
            new Claim("studentIdNumber", studentIdNumber ?? string.Empty)
        };

                var identity = new ClaimsIdentity(claims, "jwt");
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

                // Use a different variable name for the second call
                var finalAppsCheck = await CheckStudentApplications(studentNumber);
                Console.WriteLine($"DEBUG: HasApplications Response - IsValid: {finalAppsCheck.isValid}, HasApps: {finalAppsCheck.hasApplications}, AppTypes Count: {finalAppsCheck.applicationTypes?.Count ?? 0}");
                if (finalAppsCheck.applicationTypes != null)
                {
                    Console.WriteLine($"DEBUG: Application Types: {string.Join(", ", finalAppsCheck.applicationTypes)}");
                }

                if (!finalAppsCheck.isValid)
                    return Json(new { success = false, errorMessage = finalAppsCheck.message });

                // Handle redirects based on flow type
                bool isApplyFlow = !isLoginOnlyFlow && !string.IsNullOrEmpty(selectedApplicationType);
                Console.WriteLine($"DEBUG: IsApplyFlow: {isApplyFlow}, SelectedAppType: '{selectedApplicationType}'");

            

                // **FIXED LOGIC: For login-only flow, always go to submitted applications**
                if (isLoginOnlyFlow)
                {
                    // Pass "ALL" or null to indicate we want all application types
                    var submittedApplicationsUrl = Url.Action("SubmittedApplications", "Application", new
                    {
                        studentNumber,
                        applicationType = "ALL"  // Or pass null to trigger the "get all" logic
                    });
                    Console.WriteLine($"DEBUG: Login-only flow - Redirecting to: {submittedApplicationsUrl}");
                    return Json(new { success = true, redirectUrl = submittedApplicationsUrl });
                }

                // **Apply Flow Logic** (only for actual application flows)
                // **Apply Flow Logic** (only for actual application flows)
                if (isApplyFlow)
                {
                    // **FIXED: Check if student has NO applications at all OR doesn't have this specific type**
                    bool hasThisApplicationType = finalAppsCheck.applicationTypes != null &&
                                                finalAppsCheck.applicationTypes.Any(at =>
                                                    at.Equals(selectedApplicationType, StringComparison.OrdinalIgnoreCase));

                    Console.WriteLine($"DEBUG: HasThisApplicationType '{selectedApplicationType}': {hasThisApplicationType}");

                    // If student has NO applications OR doesn't have this specific application type, go to application form
                    if (!finalAppsCheck.hasApplications || !hasThisApplicationType)
                    {
                        var newApplicationUrl = Url.Action("ApplicationForm", "Application", new { applicationType = selectedApplicationType });
                        Console.WriteLine($"DEBUG: New application flow - Redirecting to: {newApplicationUrl}");
                        return Json(new { success = true, redirectUrl = newApplicationUrl });
                    }
                    else
                    {
                        // If they already have this application type, go to submitted applications
                        // BUT pass the specific application type as parameter
                        var submittedApplicationsUrl = Url.Action("SubmittedApplications", "Application", new
                        {
                            studentNumber,
                            applicationType = selectedApplicationType  // Pass the specific type
                        });
                        Console.WriteLine($"DEBUG: Existing application - Redirecting to: {submittedApplicationsUrl}");
                        Console.WriteLine($"DEBUG: Application type parameter: {selectedApplicationType}");
                        return Json(new { success = true, redirectUrl = submittedApplicationsUrl });
                    }
                }

                // **Fallback: Always go to submitted applications for any other case**
                var fallbackApplicationsUrl = Url.Action("SubmittedApplications", "Application", new { studentNumber });
                Console.WriteLine($"DEBUG: Fallback - Redirecting to: {fallbackApplicationsUrl}");
                return Json(new { success = true, redirectUrl = fallbackApplicationsUrl });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception occurred: {ex.Message}");
                return Json(new { success = false, errorMessage = $"An error occurred: {ex.Message}" });
            }
        }






        public class ResponseDto<T>
        {
            public string? Message { get; set; }
            public T Data { get; set; }
            public bool IsValid { get; set; }
        }

     
        private async Task<(bool isValid, bool hasApplications, List<string>? applicationTypes, string message)>
    CheckStudentApplications(string studentNumber)
        {
            try
            {
                var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_applicationSession.Token}" }
        };

                var response = HttpRequests.Request(
                    $"StudentAccount/hasStudentApplications/{studentNumber}",
                    HttpVerb.Get, null, null);

                if (!response.IsValid)
                    return (false, false, null, response.Message ?? "Failed to retrieve student applications.");

                if (string.IsNullOrEmpty(response.Message))
                    return (false, false, null, "No content returned from the API.");

                var responseObj = JsonConvert.DeserializeObject<ResponseDto<StudentApplicationsDto>>(response.Message);
                if (responseObj == null || responseObj.Data == null)
                    return (false, false, null, "Invalid response structure.");

                return (responseObj.IsValid, responseObj.Data.HasApplications, responseObj.Data.ApplicationTypes, responseObj.Message ?? "");
            }
            catch (JsonException ex)
            {
                return (false, false, null, $"Error processing response data: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, false, null, $"An error occurred: {ex.Message}");
            }
        }


    }
}