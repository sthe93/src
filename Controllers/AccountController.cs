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

namespace UJStudentGorvenanceStudentWeb.Controllers
{
    [Authorize]
    public class AccountController(IApplicationSession applicationSession) : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
     
            return PartialView("_StaffLoginModal");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var response = HttpRequests.GetJsonTokenForStaff(model.Username, model.Password);

                if (response.IsValid)
                {
                    var responseObj = JsonConvert.DeserializeObject<ResponseDto>(response.Message!);

                    if (responseObj is { IsValid: true })
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(responseObj.Message);

                        var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "typ")?.Value;
                        var email = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                        var fullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "fullname")?.Value;
                        var staffNumber = jwtToken.Claims.FirstOrDefault(c => c.Type == "staffNumber")?.Value;

                        if (role != null && email != null && fullName != null)
                        {
                            applicationSession.Role = role;
                            applicationSession.Email = email;
                            applicationSession.Token = responseObj.Message;
                            applicationSession.FullName = fullName;
                            applicationSession.StaffNumber = staffNumber!;

                            var claims = new[]
                            {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("fullname", fullName)
                    };

                            var identity = new ClaimsIdentity(claims, "jwt");
                            var principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync(principal);

                            return Json(new { success = true, redirectUrl = Url.Action("ReferralApplication", "ReferralApplication") });
                        }

                        return Json(new { success = false, errorMessage = "Failed to retrieve user information from token." });
                    }

                    return Json(new { success = false, errorMessage = "Invalid Credentials!" });
                }

                return Json(new { success = false, errorMessage = "Invalid Credentials!" });
            }

            return Json(new { success = false, errorMessage = "Invalid model state." });
        }


        [Authorize]
        [HttpPost]
        public new async Task<IActionResult> SignOut()
        {
            
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }





    }
}
