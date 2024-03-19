using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashWash.Data;
using FlashWash.Models;
using Microsoft.Extensions.Logging;

namespace FlashWash.Controllers
{
    public class AddressController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AddressController> _logger;

        // Constructor to inject dependencies
        public AddressController(AppDbContext db, ILogger<AddressController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: Address/Create - Display the form to create a new address
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Address/Create - Handle the creation of a new address
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Address address)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Log information about the created address
                _logger.LogInformation($"Created address: {address.Street}, {address.City}, {address.State}, {address.PostalCode}");

                // Add the address to the database
                _db.Addresses.Add(address);
                _db.SaveChanges();

                // Redirect to the address list or any other action
                return RedirectToAction("Index");
            }

            // If model state is not valid, return to the create view with validation errors
            return View(address);
        }

        // GET: Address/Update/5 - Display the form to update an existing address
        [HttpGet]
        public IActionResult Update(int? id)
        {
            // Check if the id is null
            if (id == null)
            {
                return NotFound();
            }

            // Find the address with the given id
            Address address = _db.Addresses.Find(id);

            // Check if the address is not found
            if (address == null)
            {
                return NotFound();
            }

            // Return the update view with the found address
            return View(address);
        }

        // POST: Address/Update/5 - Handle the update of an existing address
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int id, Address address)
        {
            // Check if the id in the route parameters matches the id in the model
            if (id != address.AddressId)
            {
                return NotFound();
            }

            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Log information about the updated address
                    _logger.LogInformation($"Updated address with ID {id}: {address.Street}, {address.City}, {address.State}, {address.PostalCode}");

                    // Update the address in the database
                    _db.Entry(address).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if the address doesn't exist
                    if (!AddressExists(address.AddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirect to the address list or any other action
                return RedirectToAction("Index");
            }

            // If model state is not valid, return to the update view with validation errors
            return View(address);
        }

        // Helper method to check if an address with a given id exists
        private bool AddressExists(int id)
        {
            return _db.Addresses.Any(a => a.AddressId == id);
        }
    }
}
