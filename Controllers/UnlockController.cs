using LoginPage.Data;
using LoginPage.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using LoginPage.Models.ViewModels;

namespace LoginPage.Controllers
{
    public class UnlockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _emailSettings;

        public UnlockController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Email not found.");
                return View();
            }


            var unlockToken = Guid.NewGuid().ToString().Substring(0, 5);


            user.UnlockAccountToken = unlockToken;
            user.UnlockAccountTokenExpiration = DateTime.Now.AddMinutes(2);
            _context.SaveChanges();


            SendUnlockEmail(user.Email, "Unlock Account Token", $"Your unlock account token is: {unlockToken}");

            TempData["UnlockToken"] = unlockToken;

            return RedirectToAction("EnterUnlockToken", user);
        }


        public IActionResult ResetPasswordForUnlock()
        {

            string unlockToken = TempData["UnlockToken"]?.ToString();


            return View(new UnlockViewModel { UnlockToken = unlockToken });
        }

        [HttpPost]
        public IActionResult ResetPasswordForUnlock(UnlockViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.UnlockAccountToken == model.UnlockToken && u.UnlockAccountTokenExpiration > DateTime.Now);

                if (user == null)
                {
                    ModelState.AddModelError("UnlockToken", "Invalid or expired token.");
                    return View(model);
                }


                user.Password = model.NewPassword;
                user.ConfirmPassword = model.NewPassword;
                user.LockoutEndDate = null;
                _context.SaveChanges();

                user.UnlockAccountToken = null;
                user.UnlockAccountTokenExpiration = null;

                TempData["ResetSuccessMessage"] = "Password reset successfully!";
                TempData["UnlockSuccess"] = true;


                return RedirectToAction("Login", "Login");
            }

            return View(model);
        }


        private void SendUnlockEmail(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.SmtpPort,
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword),
                EnableSsl = true,
            };

            using (var mailMessage = new MailMessage(_emailSettings.SenderEmail, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            })
            {
                smtpClient.Send(mailMessage);
            }
        }



        public IActionResult EnterUnlockToken()
        {
            return View();
        }


        [HttpPost]
        public IActionResult VerifyUnlockToken(User user)
        {
            var userFromDb = _context.Users.FirstOrDefault(u => u.UnlockAccountToken == user.UnlockAccountToken && u.UnlockAccountTokenExpiration > DateTime.Now);

            if (userFromDb == null)
            {
                ModelState.AddModelError("UnlockAccountToken", "Invalid or expired token.");
                return View("EnterUnlockToken", user);

            }
            else
            {
                if (user.ChangePassword)
                {


                    user.LockoutEndDate = null;
                    return RedirectToAction("ResetPasswordForUnlock", new { email = userFromDb.Email });
                }


                user.LockoutEndDate = null;
                _context.SaveChanges();

                user.UnlockAccountToken = null;
                user.UnlockAccountTokenExpiration = null;


                TempData["UnlockSuccess"] = true;
                return RedirectToAction("Login", "Login");
                
            }
        }



    }
}
