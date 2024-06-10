using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using LoginPage.Models;
using LoginPage.Data;



namespace LoginPage.Controllers
{
	public class LoginController : Controller
	{
		private readonly ApplicationDbContext _context;

		public LoginController(ApplicationDbContext context)
		{
			_context = context;
		}

		public IActionResult Login()
		{
            ViewBag.ResetSuccessMessage = TempData["ResetSuccessMessage"] as string;
            ViewBag.UnlockSuccess = TempData["UnlockSuccess"] as bool? ?? false;
            ViewBag.IsAccountLocked = false;
            return View();
		}

        [HttpPost]
        public async Task<IActionResult> Login(User usrl)
        {
            ModelState.ClearValidationState("UserName");
            ModelState.ClearValidationState("Email");

            if (string.IsNullOrWhiteSpace(usrl.UserName) && string.IsNullOrWhiteSpace(usrl.Email))
            {
                ModelState.AddModelError("UserName", "Username or Email is required");
                ModelState.AddModelError("Email", "Username or Email is required");
                ViewBag.IsAccountLocked = false;
                return View();
            }
            else if(string.IsNullOrWhiteSpace(usrl.UserName) && !string.IsNullOrWhiteSpace(usrl.Email))
            {
                ModelState.ClearValidationState("UserName");
            }
            else if (!string.IsNullOrWhiteSpace(usrl.UserName) && string.IsNullOrWhiteSpace(usrl.Email))
            {
                ModelState.ClearValidationState("Email");
            }


            var user = _context.Users.FirstOrDefault(oo => oo.UserName == usrl.UserName || oo.Email == usrl.Email);

            if (user == null)
            {
               
                    if (!string.IsNullOrWhiteSpace(usrl.UserName))
                    {
                        ModelState.AddModelError("UserName", "Username Not Found!");
                        ModelState.ClearValidationState("Email");
                    }

                    if (!string.IsNullOrWhiteSpace(usrl.Email))
                    {
                        ModelState.AddModelError("Email", "Email Not Found!");
                        ModelState.ClearValidationState("UserName");
                    }

                    ViewBag.IsAccountLocked = false;
                

                return View();
            }

            else if (user.LockoutEndDate.HasValue && user.LockoutEndDate > DateTime.Now)
            {
               
                ViewBag.LockoutMessage = $"Account is locked";
                ViewBag.IsAccountLocked = true;  
                return View();
            }
            else if (user != null && (user.UserName == usrl.UserName || user.Email == usrl.Email) && user.Password == usrl.Password)
            {
              
                user.FailedLoginAttempts = 0;
                user.LockoutEndDate = null; 
                _context.SaveChanges();

                
                ViewBag.IsAccountLocked = false;

                return RedirectToAction("Index", "Registration");
            }
            //else if (user != null && user.Email == usrl.Email)
            //{
            //    user.FailedLoginAttempts = 0;
            //    user.LockoutEndDate = null; // Reset lockout if successful login
            //    _db.SaveChanges();

            //    // Set IsAccountLocked to false when the login is successful
            //    ViewBag.IsAccountLocked = false;

            //    return RedirectToAction("Index", "Registration");
            //}
            else
            {
               
                user.FailedLoginAttempts++;

            
                if (user.FailedLoginAttempts >= 3)
                {
                    user.LockoutEndDate = DateTime.Now.AddMinutes(15);
                    _context.SaveChanges();

                    ViewBag.LockoutMessage = $"Account locked";
                    ViewBag.IsAccountLocked = true;  
                    return View();
                }

                ModelState.AddModelError("Password", "Incorrect Password...Try Again!");
                _context.SaveChanges();

                ViewBag.IsAccountLocked = false;  
                return View();
            }
        }








        [HttpPost]

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			return RedirectToAction("Login", "Login");
		}


	}
}
