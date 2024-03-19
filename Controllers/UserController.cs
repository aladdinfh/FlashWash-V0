using FlashWash.Data;
using FlashWash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Controller for managing user actions such as registration, login, updating profile, etc.
public class UserController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserController> _logger;

    // Constructor to initialize the controller with required dependencies
    public UserController(AppDbContext db, ILogger<UserController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Login/Register View
    [HttpGet]
    public IActionResult LogReg()
    {
        // Check if user is already logged in, if so, redirect to Dashboard
        int? userId = HttpContext.Session.GetInt32("userId");
        return userId.HasValue ? RedirectToAction("Dashboard") : View();
    }

    // POST: Register User
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            // Check if the user's email already exists in the database
            var existingUser = _db.Users.FirstOrDefault(w => w.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View("LogReg", user);
            }

            // Hash the user's password before storing it
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password);

            // Save the user to the database
            _db.Users.Add(user);
            _db.SaveChanges();

            // Set session variable to indicate user is logged in
            HttpContext.Session.SetInt32("userId", user.UserId);

            // Redirect to Dashboard
            return RedirectToAction("Dashboard", new { id = user.UserId });
        }

        // If registration fails due to validation errors, return to registration page with errors
        return View("LogReg", ModelState);
    }

    // POST: User Login
    [HttpPost]
    public IActionResult Login(LoginUser loginuser)
    {
        if (ModelState.IsValid)
        {
            // Find user by email in the database
            var userFromDb = _db.Users.Include(w => w.Address).SingleOrDefault(w => w.Email == loginuser.LoginEmail);
            if (userFromDb == null)
            {
                ModelState.AddModelError("LoginEmail", "Email doesn't exist");
                return View("LogReg", loginuser);
            }

            // Verify user's password
            PasswordHasher<LoginUser> hasher = new();
            var result = hasher.VerifyHashedPassword(loginuser, userFromDb.Password, loginuser.LoginPassword);

            if (result == 0)
            {
                ModelState.AddModelError("LoginPassword", "Wrong Password.");
                return View("LogReg", ModelState);
            }

            // Set session variable to indicate user is logged in
            HttpContext.Session.SetInt32("userId", userFromDb.UserId);
            return RedirectToAction("Dashboard", new { id = userFromDb.UserId });
        }

        // If login fails due to validation errors, return to login page with errors
        return View("LogReg", ModelState);
    }

    // POST: Logout User
    [HttpPost]
    public IActionResult Logout()
    {
        // Clear session variables and redirect to Login/Register page
        HttpContext.Session.Clear();
        return RedirectToAction("LogReg");
    }

    // GET: Dashboard
    [HttpGet]
    [SessionCheck] // Custom action filter to check if user is logged in
    public IActionResult Dashboard()
    {
        // Retrieve wash stations and user details for displaying on dashboard
        List<WashStation> washStations = _db.WashStations.Include(w => w.Offers).ToList();
        User user = _db.Users.Include(a => a.Address).FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("userId"));

        // Check if user and address are not null before creating the view model
        if (user != null && user.Address != null)
        {
            var viewModel = new DashboardViewModel
            {
                WashStations = washStations,
                User = user
            };

            return View(viewModel);
        }
        else
        {
            // If user or address is null, display error page
            return View("Error");
        }
    }

    // GET: View details of a single offer
    [HttpGet]
    [SessionCheck] // Custom action filter to check if user is logged in
    public IActionResult ViewOneOffer(int offerId)
    {
        // Retrieve the offer details
        Offer offer = _db.Offers.Include(o => o.WashStation).FirstOrDefault(o => o.OfferId == offerId);

        if (offer != null)
        {
            // Check if the user has already sent a request for this offer
            int userId = (int)HttpContext.Session.GetInt32("userId");
            var existingRequest = _db.Requests.FirstOrDefault(r => r.UserId == userId && r.OfferId == offerId);

            // Pass information to the view to determine if the user has already requested this offer
            ViewBag.UserSentRequest = existingRequest != null;

            return View(offer);
        }

        // If offer is not found, redirect to Dashboard
        return RedirectToAction("Dashboard");
    }

    // GET: Update Password View
    [HttpGet]
    public IActionResult UpdatePassword()
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users.Find(userId);

            return View("UpdatePasswordView", user);
        }

        // If user is not logged in, redirect to Login/Register page
        return RedirectToAction("LogReg");
    }

    // POST: Update Password
    [HttpPost]
    public IActionResult UpdatePassword(string oldPassword, string newPassword)
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users.Find(userId);

            try
            {
                // Call the ChangePassword method on the user object
                user.ChangePassword(oldPassword, newPassword);

                // Hash the new password
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                user.Password = hasher.HashPassword(user, newPassword);

                // Save changes to the database
                _db.SaveChanges();

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("Dashboard");
            }
            catch (ArgumentException ex)
            {
                // If old password verification fails, add error to model state
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        // If user authentication fails or ModelState is not valid, return to the update password view with validation errors.
        return View("UpdatePasswordView");
    }

    // GET: Update Email View
    [HttpGet]
    public IActionResult UpdateEmail()
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users.Find(userId);

            var model = new EmailUpdateViewModel
            {
                // Add this line to set the UserEmail property
                NewEmail = user.Email
            };

            return View("UpdateEmailView", model);
        }

        // If user is not logged in, redirect to Login/Register page
        return RedirectToAction("LogReg");
    }

    // POST: Update Email
    [HttpPost]
    public async Task<IActionResult> UpdateEmail(EmailUpdateViewModel model)
    {
        // Validate the new email.
        if (ModelState.IsValid)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId.HasValue)
            {
                // Retrieve the current user from the database
                User user = _db.Users.Find(userId);

                // Update the user's email in the database.
                user.Email = model.NewEmail;

                // Save changes to the database
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} updated their email.");

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("Dashboard");
            }
        }

        // If ModelState is not valid, return to the update email view with validation errors.
        return View("UpdateEmailView", model);
    }

    // GET: Update Address View
    [HttpGet]
    public IActionResult UpdateAddress()
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users
                .Include(u => u.Address)
                .FirstOrDefault(u => u.UserId == userId);

            var model = new AddressUpdateViewModel
            {
                Street = user.Address.Street,
                City = user.Address.City,
                State = user.Address.State,
                PostalCode = user.Address.PostalCode
            };

            return View("UpdateAddressView", model);
        }

        // If user is not logged in, redirect to Login/Register page
        return RedirectToAction("LogReg");
    }

    // POST: Update Address
    [HttpPost]
    public async Task<IActionResult> UpdateAddress(AddressUpdateViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId.HasValue)
            {
                // Retrieve the current user from the database
                User user = _db.Users
                    .Include(u => u.Address)
                    .FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {
                    // Update the user's address in the database.
                    user.Address.Street = model.Street;
                    user.Address.City = model.City;
                    user.Address.State = model.State;
                    user.Address.PostalCode = model.PostalCode;

                    // Save changes to the database
                    _db.Entry(user).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"User {user.Email} updated their address.");

                    // Redirect to the dashboard or appropriate page.
                    return RedirectToAction("Dashboard");
                }
            }
        }

        // If ModelState is not valid, return to the update address view with validation errors.
        return View("UpdateAddressView", model);
    }

    // GET: Update Name View
    [HttpGet]
    public IActionResult UpdateName()
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users.Find(userId);

            var model = new NameUpdateViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View("UpdateNameView", model);
        }

        // If user is not logged in, redirect to Login/Register page
        return RedirectToAction("LogReg");
    }

    // POST: Update Name
    [HttpPost]
    public async Task<IActionResult> UpdateName(NameUpdateViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId.HasValue)
            {
                // Retrieve the current user from the database
                User user = _db.Users.Find(userId);

                // Update the user's name in the database.
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                // Save changes to the database
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} updated their name.");

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("Dashboard");
            }
        }

        // If ModelState is not valid, return to the update name view with validation errors.
        return View("UpdateNameView", model);
    }

    // GET: Update Telephone View
    [HttpGet]
    public IActionResult UpdateTelephone()
    {
        // Check if user is logged in
        int? userId = HttpContext.Session.GetInt32("userId");

        if (userId.HasValue)
        {
            User user = _db.Users.Find(userId);

            var model = new TelephoneUpdateViewModel
            {
                Telephone = user.Telephone
            };

            return View("UpdateTelephoneView", model);
        }

        // If user is not logged in, redirect to Login/Register page
        return RedirectToAction("LogReg");
    }

    // POST: Update Telephone
    [HttpPost]
    public async Task<IActionResult> UpdateTelephone(TelephoneUpdateViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? userId = HttpContext.Session.GetInt32("userId");

            if (userId.HasValue)
            {
                // Retrieve the current user from the database
                User user = _db.Users.Find(userId);

                // Update the user's telephone number in the database.
                user.Telephone = model.Telephone;

                // Save changes to the database
                _db.Entry(user).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User {user.Email} updated their telephone number.");

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("Dashboard");
            }
        }

        // If ModelState is not valid, return to the update telephone view with validation errors.
        return View("UpdateTelephoneView", model);
    }

    // Helper method to hash password and save user to the database
    private void HashPasswordAndSaveUser(User user)
    {
        PasswordHasher<User> hasher = new PasswordHasher<User>();
        user.Password = hasher.HashPassword(user, user.Password);

        user.Address = new Address
        {
            Street = user.Address.Street,
            City = user.Address.City,
            State = user.Address.State,
            PostalCode = user.Address.PostalCode
        };

        _db.Add(user);
        _db.SaveChanges();
    }
}
