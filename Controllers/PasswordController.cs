using LoginPage.Data;
using LoginPage.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using LoginPage.Models.ViewModels;

namespace LoginPage.Controllers
{
	public class PasswordController : Controller
	{
        private readonly ApplicationDbContext _context;
        private readonly EmailSettings _emailSettings;

        public PasswordController(ApplicationDbContext context, IConfiguration configuration)
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


            var resetToken = Guid.NewGuid().ToString().Substring(0, 5);


            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiration = DateTime.Now.AddMinutes(2);
            _context.SaveChanges();


            SendEmail(user.Email, "Reset Password Token", $"Your reset password token is: {resetToken}");

            TempData["ResetToken"] = resetToken;

            return RedirectToAction("EnterToken", user);
        }


        public IActionResult ResetPassword()
        {

            string resetToken = TempData["ResetToken"]?.ToString();


            return View(new PasswordViewModel { ResetToken = resetToken });
        }

        [HttpPost]
        public IActionResult ResetPassword(PasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == model.ResetToken && u.ResetPasswordTokenExpiration > DateTime.Now);

                if (user == null)
                {
                    ModelState.AddModelError("ResetToken", "Invalid or expired token.");
                    return View(model);
                }


                user.Password = model.NewPassword;
                user.ConfirmPassword = model.NewPassword;
                _context.SaveChanges();

                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiration = null;

                TempData["ResetSuccessMessage"] = "Password reset successfully!";

                // Redirect to login page or any other appropriate page
                return RedirectToAction("Login", "Login");
            }

            return View(model);
        }

        private void SendEmail(string to, string subject, string body)
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


        public IActionResult EnterToken()
        {

            return View();
        }

        [HttpPost]
        public IActionResult VerifyToken(User user)
        {
            var userFromDb = _context.Users.FirstOrDefault(u => u.ResetPasswordToken == user.ResetPasswordToken && u.ResetPasswordTokenExpiration > DateTime.Now);

            if (userFromDb == null)
            {
                ModelState.AddModelError("ResetPasswordToken", "Invalid or expired token.");
                return View("EnterToken", user);
            }

            return RedirectToAction("ResetPassword", new { email = userFromDb.Email });
        }
    }
}
