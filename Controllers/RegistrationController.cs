using Microsoft.AspNetCore.Mvc;
using LoginPage.Models;
using LoginPage.Data;


namespace LoginPage.Controllers
{
	public class RegistrationController : Controller
	{
        private readonly ApplicationDbContext _context;

        public RegistrationController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
		{
			return View(); 
		}

		public IActionResult SignUp()
		{
			return View();
		}

        [HttpPost]
        public IActionResult SignUp(User usr)
        {
            if (usr.UserName == usr.Password)
            {
                ModelState.AddModelError("Password", "Password cannot be same as UserName");
            }

            var userName = _context.Users.FirstOrDefault(o => o.UserName == usr.UserName);

            if (userName != null && usr.UserName == userName.UserName)
            {
                ModelState.AddModelError("UserName", "UserName already taken...use Different UserName!!");
            }

            var email = _context.Users.FirstOrDefault(o => o.Email == usr.Email);
            if (email != null && usr.Email == email.Email)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View();
            }

            if (ModelState.IsValid)
            {
                usr.ResetPasswordToken = Guid.NewGuid().ToString();
                usr.UnlockAccountToken = Guid.NewGuid().ToString();

                _context.Users.Add(usr);
                _context.SaveChanges();
                return RedirectToAction("Login", "Login");
            }

            return View();
        }

    }
}
