using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FlashWash.Data;
using FlashWash.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Policy;

public class WashStationController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<WashStationController> _logger;


    public WashStationController(AppDbContext db, ILogger<WashStationController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: Login/Register View
    [HttpGet]
    public IActionResult LogRegWash()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        // If logged in, redirect to Dashboard
        return washstationId.HasValue ? RedirectToAction("DashboardWash") : View("LogRegWash");
    }




    // POST: Register User
    [HttpPost]
    public IActionResult RegisterWash(WashStation washStation)
    {
        if (ModelState.IsValid)
        {
            var existingWashStation = _db.WashStations.FirstOrDefault(w => w.Email == washStation.Email);
            if (existingWashStation != null)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
                return View("LogRegWash", ModelState);
            }

            PasswordHasher<WashStation> hasher = new PasswordHasher<WashStation>();
            washStation.Password = hasher.HashPassword(washStation, washStation.Password);

            _db.WashStations.Add(washStation);
            _db.SaveChanges();

            HttpContext.Session.SetInt32("washStationId", washStation.WashStationId);

            return RedirectToAction("DashboardWash", new { id = washStation.WashStationId });
        }

        return View("LogRegWash", ModelState);
    }

    // POST: Login User
    [HttpPost]
    public IActionResult LoginWash(LoginWashStation loginwashstation)
    {
        if (ModelState.IsValid)
        {
            var washStationFromDb = _db.WashStations.Include(w => w.Address).SingleOrDefault(w => w.Email == loginwashstation.LoginEmail);
            if (washStationFromDb == null)
            {
                ModelState.AddModelError("LoginEmail", "Email doesn't exist");
                return View("LogRegWash", ModelState);
            }

            PasswordHasher<LoginWashStation> hasher = new();
            var result = hasher.VerifyHashedPassword(loginwashstation, washStationFromDb.Password, loginwashstation.LoginPassword);

            if (result == 0)
            {
                ModelState.AddModelError("LoginPassword", "Wrong Password.");
                return View("LogRegWash", ModelState);
            }

            HttpContext.Session.SetInt32("washStationId", washStationFromDb.WashStationId);
            return RedirectToAction("DashboardWash", new { id = washStationFromDb.WashStationId });
        }

        return View("LogRegWash", ModelState);
    }




    // POST: Logout User
    [HttpPost]
    public IActionResult LogoutWash()
    {
        // Clear session variables and redirect to Login/Register page
        HttpContext.Session.Clear();
        return RedirectToAction("LogRegWash");
    }

    // GET: Dashboard
    [HttpGet]
    [WashStationSessionCheck]
    public IActionResult DashboardWash()
    {
        var washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId is not null)
        {
            WashStation washstation = _db.WashStations
                .Include(u => u.Address).Include(u => u.Offers)
                .FirstOrDefault(u => u.WashStationId == (int)washstationId);


            return View("DashboardWash", washstation);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }



    [HttpGet]
    public IActionResult UpdateWashstationPassword()
    {
        int? washStationId = HttpContext.Session.GetInt32("washStationId");

        if (washStationId.HasValue)
        {
            // Retrieve the WashStation from the database based on the washStationId
            WashStation washStation = _db.WashStations.Find(washStationId);

            // Check if the WashStation object is not null
            if (washStation != null)
            {
                return View(washStation); // Pass the WashStation object to the view
            }
        }

        // If the user is not authenticated or WashStation object is null, redirect to login or registration page
        return RedirectToAction("LogRegWash", "WashStation");
    }



    [HttpPost]
    public IActionResult UpdatePasswordWash(string oldPassword, string newPassword)
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            try
            {
                // Call the ChangePassword method on the washstation object
                washstation.ChangePassword(oldPassword, newPassword);

                // Save changes to the database
                _db.SaveChanges();

                _logger.LogInformation($"Wash station {washstation.Name} updated their password.");

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash");
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










    [HttpGet]
    public IActionResult UpdateWashStationName()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdateNameWashViewModel
            {
                NewName = washstation.Name
            };

            return View("UpdateWashStationNameView", model);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationName(UpdateNameWashViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update wash station name
                washstation.Name = model.NewName;

                // Save changes to the database
                _db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated name.");


                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update wash station name view with validation errors.
        return View("UpdateWashStationNameView", model);
    }

    // Other actions...


    [HttpGet]
    public IActionResult UpdatePaymentOptions()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdatePaymentOptionsViewModel
            {
                AcceptCreditCard = washstation.AcceptCreditCard,
                AcceptCash = washstation.AcceptCash,
                AcceptMobilePayment = washstation.AcceptMobilePayment
            };

            return View("UpdatePaymentOptionsView", model);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePaymentOptions(UpdatePaymentOptionsViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update payment options
                washstation.AcceptCreditCard = model.AcceptCreditCard;
                washstation.AcceptCash = model.AcceptCash;
                washstation.AcceptMobilePayment = model.AcceptMobilePayment;

                // Save changes to the database
                _db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated payment options.");

                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update payment options view with validation errors.
        return View("UpdatePaymentOptionsView", model);
    }

    [HttpGet]
    public IActionResult UpdateWashStationAddress()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations
                .Include(u => u.Address)
                .FirstOrDefault(u => u.WashStationId == washstationId);

            var model = new UpdateAddressWashViewModel
            {
                Street = washstation.Address.Street,
                City = washstation.Address.City,
                State = washstation.Address.State,
                PostalCode = washstation.Address.PostalCode
            };

            return View("UpdateWashStationAddressView", model);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationAddress(UpdateAddressWashViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations
                    .Include(u => u.Address)
                    .FirstOrDefault(u => u.WashStationId == washstationId);

                if (washstation != null)
                {
                    // Update the wash station's address in the database.
                    washstation.Address.Street = model.Street;
                    washstation.Address.City = model.City;
                    washstation.Address.State = model.State;
                    washstation.Address.PostalCode = model.PostalCode;

                    // Save changes to the database
                    _db.Entry(washstation).State = EntityState.Modified;
                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"Wash station {washstation.Name} updated their address.");

                    // Redirect to the dashboard or appropriate page.
                    return RedirectToAction("DashboardWash");
                }
            }
        }

        // If ModelState is not valid, return to the update address view with validation errors.
        return View("UpdateWashStationAddressView", model);
    }


    [HttpGet]
    public IActionResult UpdateWashStationOwnerName()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdateOwnerNameViewModel
            {
                OwnerName = washstation.OwnerName
            };

            return View("UpdateWashStationOwnerNameView", model);
        }

        return RedirectToAction("LogRegWash");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationOwnerName(UpdateOwnerNameViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update wash station owner's name
                washstation.OwnerName = model.OwnerName;

                // Save changes to the database
                //_db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated owner's name.");


                // Redirect to the dashboard instead of "LogRegWash"
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update wash station owner's name view with validation errors.
        return View("UpdateWashStationOwnerNameView", model);
    }

    [HttpGet]
    public IActionResult UpdateWashStationEmail()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdateEmailWashViewModel
            {
                Email = washstation.Email
            };

            return View("UpdateWashStationEmailView", model);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationEmail(UpdateEmailWashViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update wash station email
                washstation.Email = model.Email;

                // Save changes to the database
                _db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated email.");


                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update wash station email view with validation errors.
        return View("UpdateWashStationEmailView", model);
    }

    // Other actions...

    [HttpGet]
    public IActionResult UpdateWashStationTelephone()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdateTelephoneWashViewModel
            {
                Telephone = washstation.Telephone
            };

            return View("UpdateWashStationTelephoneView", model);
        }

        return RedirectToAction("LogRegWash", "WashStation");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationTelephone(UpdateTelephoneWashViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update wash station telephone number
                washstation.Telephone = model.Telephone;

                // Save changes to the database
                _db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated telephone number.");
                //SetSessionVariables(washstation);


                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update wash station telephone view with validation errors.
        return View("UpdateWashStationTelephoneView", model);
    }

    // Other actions...

    [HttpGet]
    public IActionResult UpdateWashStationTime()
    {
        int? washstationId = HttpContext.Session.GetInt32("washStationId");

        if (washstationId.HasValue)
        {
            WashStation washstation = _db.WashStations.Find(washstationId);

            var model = new UpdateTimeViewModel
            {
                MorningStartTime = washstation.MorningStartTime,
                MorningEndTime = washstation.MorningEndTime,
                EveningStartTime = washstation.EveningStartTime,
                EveningEndTime = washstation.EveningEndTime
            };

            return View("UpdateWashStationTimeView", model);
        }

        return RedirectToAction("LogRegWash");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateWashStationTime(UpdateTimeViewModel model)
    {
        // Validate the model state and user authentication.
        if (ModelState.IsValid)
        {
            int? washstationId = HttpContext.Session.GetInt32("washStationId");

            if (washstationId.HasValue)
            {
                // Retrieve the current wash station from the database
                WashStation washstation = _db.WashStations.Find(washstationId);

                // Update wash station time properties
                washstation.MorningStartTime = model.MorningStartTime;
                washstation.MorningEndTime = model.MorningEndTime;
                washstation.EveningStartTime = model.EveningStartTime;
                washstation.EveningEndTime = model.EveningEndTime;

                // Save changes to the database
                _db.Entry(washstation).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Wash station {washstation.Name} updated time slots.");


                // Redirect to the dashboard or appropriate page.
                return RedirectToAction("DashboardWash", "WashStation");
            }
        }

        // If ModelState is not valid, return to the update wash station time view with validation errors.
        return View("UpdateWashStationTimeView", model);
    }


}

