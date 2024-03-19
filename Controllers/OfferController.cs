using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FlashWash.Data;
using FlashWash.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OfferController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<OfferController> _logger;

    public OfferController(AppDbContext db, ILogger<OfferController> logger)
    {
        _db = db;
        _logger = logger;
    }
    [HttpGet]
    [WashStationSessionCheck]

    public IActionResult CreateOffer()
    {
        ViewBag.Ids = _db.OfferTypes.ToList();


        return View();
    }
    [HttpPost]
    [WashStationSessionCheck]

    public IActionResult CreateOffer(Offer offer)
    {
        if (ModelState.IsValid)
        {
            _db.Offers.Add(offer);
            _db.SaveChanges();

            return RedirectToAction("DashboardWash", "WashStation");
        }

        // Repopulate OfferTypes for the dropdown
        ViewBag.Ids = _db.OfferTypes.ToList();

        return View(offer);
    }







    [HttpPost]
    [WashStationSessionCheck]

    public IActionResult CreateOfferType(OfferType viewModel)
    {
        if (ModelState.IsValid)
        {
            _db.OfferTypes.Add(viewModel);
            _db.SaveChangesAsync();


            return RedirectToAction("GetAllOfferTypes");
        }

        return View(viewModel);
    }

    [HttpGet]
    [WashStationSessionCheck]

    public IActionResult GetAllOfferTypes()
    {
        List<OfferType> allOfferTypes = _db.OfferTypes.ToList();

        return View(allOfferTypes);
    }

    [HttpGet]
    [WashStationSessionCheck]

    public IActionResult UpdateOffer(int id)
    {
        Offer offer = _db.Offers.SingleOrDefault(o => o.OfferId == id);
        return View(offer);
    }

    [HttpPost]
    [WashStationSessionCheck]

    public ActionResult UpdateOffer(int id, Offer offer)
    {
        offer.OfferId = id; 

        Offer offerFromDb = _db.Offers.FirstOrDefault(o => o.OfferId == id);

        if (offerFromDb == null)
        {
            return NotFound("Offer not found");
        }

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
        return View(offer);
    }


    [HttpPost]
    [WashStationSessionCheck]

    public IActionResult DeleteOffer(int id)
    {
        Offer offer = _db.Offers.FirstOrDefault(o => o.OfferId == id);

        if (offer != null)
        {
            _db.Offers.Remove(offer);
            _db.SaveChanges();


            return RedirectToAction("DashboardWash", "WashStation");
        }

        return View("GetAllOffers");
    }


   [HttpGet]
   [WashStationSessionCheck]

    public IActionResult GetAllOffers()
    {
        List<Offer> allOffers = _db.Offers.ToList();

        return View(allOffers); 
    }

    [HttpGet]
    public IActionResult GetOffersByType(int typeId)
    {
        List<Offer> offersByType = _db.Offers.Where(o => o.OfferTypeId == typeId).ToList();

        return View(offersByType); 
    }
    
    [HttpGet]
    [WashStationSessionCheck]

    public IActionResult GetOffersByWashStation(int washStationId)
    {
        List<Offer> offersByWashStation = _db.Offers.Where(o => o.WashStationId == washStationId).ToList();

        return View(offersByWashStation); 
    }
}
