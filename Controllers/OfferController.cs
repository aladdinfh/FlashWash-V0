// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FlashWash.Data;
using FlashWash.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Define the OfferController class
public class OfferController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<OfferController> _logger;

    // Constructor to initialize the controller with dependencies
    public OfferController(AppDbContext db, ILogger<OfferController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Action method to display the form for creating an offer (HTTP GET)
    [HttpGet]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult CreateOffer()
    {
        // Retrieve all offer types from the database and pass them to the view
        ViewBag.Ids = _db.OfferTypes.ToList();
        return View();
    }

    // Action method to handle the creation of an offer (HTTP POST)
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult CreateOffer(Offer offer)
    {
        // If model state is valid, add the offer to the database and redirect to dashboard
        if (ModelState.IsValid)
        {
            _db.Offers.Add(offer);
            _db.SaveChanges();
            return RedirectToAction("DashboardWash", "WashStation");
        }

        // If model state is not valid, repopulate OfferTypes for the dropdown and return the view with errors
        ViewBag.Ids = _db.OfferTypes.ToList();
        return View(offer);
    }

    // Action method to handle the creation of an offer type (HTTP POST)
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult CreateOfferType(OfferType viewModel)
    {
        // If model state is valid, add the offer type to the database and redirect to action to get all offer types
        if (ModelState.IsValid)
        {
            _db.OfferTypes.Add(viewModel);
            _db.SaveChangesAsync();
            return RedirectToAction("GetAllOfferTypes");
        }

        // If model state is not valid, return the view with errors
        return View(viewModel);
    }

    // Action method to display all offer types
    [HttpGet]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult GetAllOfferTypes()
    {
        // Retrieve all offer types from the database and pass them to the view
        List<OfferType> allOfferTypes = _db.OfferTypes.ToList();
        return View(allOfferTypes);
    }

    // Action method to display the form for updating an offer (HTTP GET)
    [HttpGet]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult UpdateOffer(int id)
    {
        // Retrieve the offer with the specified id from the database and pass it to the view
        Offer offer = _db.Offers.SingleOrDefault(o => o.OfferId == id);
        return View(offer);
    }

    // Action method to handle the update of an offer (HTTP POST)
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    public ActionResult UpdateOffer(int id, Offer offer)
    {
        // Set the OfferId of the offer object to the specified id
        offer.OfferId = id;

        // Retrieve the offer with the specified id from the database
        Offer offerFromDb = _db.Offers.FirstOrDefault(o => o.OfferId == id);

        // If the offer is not found in the database, return a not found response
        if (offerFromDb == null)
        {
            return NotFound("Offer not found");
        }

        // If model state is valid, update the offer in the database and redirect to dashboard
        if (ModelState.IsValid)
        {
            offerFromDb.Title = offer.Title;
            offerFromDb.Description = offer.Description;
            offerFromDb.Price = offer.Price;
            offerFromDb.ImageUrl = offer.ImageUrl;
            offerFromDb.UpdatedAt = DateTime.Now;
            _db.SaveChanges();
            return RedirectToAction("DashboardWash", "WashStation");
        }

        // If model state is not valid, return the view with errors
        return View(offer);
    }

    // Action method to handle the deletion of an offer (HTTP POST)
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult DeleteOffer(int id)
    {
        // Retrieve the offer with the specified id from the database
        Offer offer = _db.Offers.FirstOrDefault(o => o.OfferId == id);

        // If the offer is found, remove it from the database and redirect to dashboard
        if (offer != null)
        {
            _db.Offers.Remove(offer);
            _db.SaveChanges();
            return RedirectToAction("DashboardWash", "WashStation");
        }

        // If the offer is not found, return a view with all offers
        return View("GetAllOffers");
    }

    // Action method to display all offers
    [HttpGet]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult GetAllOffers()
    {
        // Retrieve all offers from the database and pass them to the view
        List<Offer> allOffers = _db.Offers.ToList();
        return View(allOffers);
    }

    // Action method to display offers by type
    [HttpGet]
    public IActionResult GetOffersByType(int typeId)
    {
        // Retrieve offers with the specified type id from the database and pass them to the view
        List<Offer> offersByType = _db.Offers.Where(o => o.OfferTypeId == typeId).ToList();
        return View(offersByType);
    }

    // Action method to display offers by wash station
    [HttpGet]
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult GetOffersByWashStation(int washStationId)
    {
        // Retrieve offers with the specified wash station id from the database and pass them to the view
        List<Offer> offersByWashStation = _db.Offers.Where(o => o.WashStationId == washStationId).ToList();
        return View(offersByWashStation);
    }
}
