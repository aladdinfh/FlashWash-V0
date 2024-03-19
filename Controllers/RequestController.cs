// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashWash.Data;
using FlashWash.Models;

// Define the RequestController class
public class RequestController : Controller
{
    private readonly AppDbContext _db;

    // Constructor to initialize the controller with dependencies
    public RequestController(AppDbContext db)
    {
        _db = db;
    }

    // Action method to display all requests (accessible to all users)
    [SessionCheck] // Custom action filter to check session
    public IActionResult AllRequests()
    {
        // Retrieve all requests from the database including related offer and user information and pass them to the view
        var requests = _db.Requests.Include(r => r.Offer).Include(o => o.User).ToList();
        return View(requests);
    }

    // Action method to display all requests associated with a specific wash station (accessible to wash station users)
    [WashStationSessionCheck] // Custom action filter to check session
    public IActionResult AllRequestsRWashStation()
    {
        // Retrieve the wash station id from session
        int? washStationId = HttpContext.Session.GetInt32("washStationId");

        // Redirect to login/register page if wash station id is not found in session
        if (washStationId == null)
        {
            return RedirectToAction("LogRegWash", "WashStation");
        }

        // Retrieve requests associated with the current wash station including related user and offer information and pass them to the view
        var requests = _db.Requests
            .Include(r => r.User)
            .Include(r => r.Offer)
            .ThenInclude(o => o.WashStation)
            .Where(r => r.Offer.WashStationId == washStationId)
            .ToList();

        return View(requests);
    }

    // Action method to display the form for creating a request (HTTP GET)
    [HttpGet]
    [SessionCheck] // Custom action filter to check session
    public IActionResult CreateRequest(int offerId)
    {
        // Retrieve the user id from session
        int userId = (int)HttpContext.Session.GetInt32("userId");

        // Check if the user has already sent a request for this offer
        var existingRequest = _db.Requests.FirstOrDefault(r => r.UserId == userId && r.OfferId == offerId);

        // Pass a flag indicating whether there is an existing request for this offer to the view
        ViewBag.HasExistingRequest = existingRequest != null;

        // Create a new request object and pass it to the view
        Request newRequest = new() { OfferId = offerId, UserId = userId };
        return View(newRequest);
    }

    // Action method to handle the submission of a request (HTTP POST)
    [HttpPost]
    [SessionCheck] // Custom action filter to check session
    [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery (CSRF) attacks
    public IActionResult SendRequest(Request request)
    {
        // Retrieve the user id from session
        int userId = (int)HttpContext.Session.GetInt32("userId");

        // Use RequestCheck method to validate the request
        bool isValid = RequestCheck(request);

        if (isValid)
        {
            // If the request is valid, add it to the database and redirect to the all requests page
            _db.Requests.Add(request);
            _db.SaveChanges();
            return RedirectToAction("AllRequests");
        }

        // If the request is not valid, redirect to the create request page with the offerId
        return RedirectToAction("CreateRequest", new { offerId = request.OfferId });
    }

    // Method to validate a request
    private bool RequestCheck(Request request)
    {
        // Check if the request is null or if the start time is in the past
        if (request == null || request.StartTime < DateTime.Now)
        {
            return false;
        }
        else
        {
            // Retrieve the offer associated with the request from the database
            Offer offer = _db.Offers
                .Include(o => o.Requests)
                .Include(o => o.WashStation)
                .FirstOrDefault(o => o.OfferId == request.OfferId);

            // Retrieve the hours and minutes from the request start time
            int hours = request.StartTime.Hour;
            int minutes = request.StartTime.Minute;
            TimeSpan ts = new TimeSpan(hours, minutes, 0);

            // Check if the request start time is within the operating hours of the associated wash station and if there are no overlapping requests
            if (ts < offer.WashStation.MorningStartTime || ts > offer.WashStation.EveningEndTime ||
                (ts > offer.WashStation.MorningEndTime && ts < offer.WashStation.EveningStartTime) ||
                offer.Requests.Any(r => r.StartTime < request.StartTime.AddMinutes(offer.DurationMinutes) && r.StartTime > request.StartTime.AddMinutes(-offer.DurationMinutes)))
            {
                return false;
            }
        }
        return true;
    }

    // Action method to handle the acceptance of a request by a wash station
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery (CSRF) attacks
    public IActionResult AcceptRequest(int requestId)
    {
        // Retrieve the request from the database
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        // If the request exists and is pending, update its status to accepted
        if (request != null && request.Status == RequestStatus.Pending)
        {
            request.Status = RequestStatus.Accepted;
            _db.SaveChanges();
        }

        return RedirectToAction("DashboardWash", "WashStation");
    }

    // Action method to handle the cancellation of a request by a wash station
    [HttpPost]
    [WashStationSessionCheck] // Custom action filter to check session
    [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery (CSRF) attacks
    public IActionResult CancelRequest(int requestId)
    {
        // Retrieve the request from the database
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        // If the request exists and is pending, remove it from the database
        if (request != null && request.Status == RequestStatus.Pending)
        {
            _db.Requests.Remove(request);
            _db.SaveChanges();
            return RedirectToAction("AllRequestsRWashStation", "WashStation");
        }
        else
        {
            // If the request cannot be canceled, return an error message
            ModelState.AddModelError(string.Empty, "This request cannot be canceled.");
            return RedirectToAction("DashboardWash", "WashStation");
        }
    }

    // Action method to handle the cancellation of a request by a user
    [HttpPost]
    [SessionCheck] // Custom action filter to check session
    [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery (CSRF) attacks
    public IActionResult CancelRequestUser(int requestId)
    {
        // Retrieve the request from the database
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        // If the request exists and is pending, remove it from the database
        if (request != null && request.Status == RequestStatus.Pending)
        {
            _db.Requests.Remove(request);
            _db.SaveChanges();
            return RedirectToAction("Dashboard", "User"); // Redirect to user dashboard
        }
        else
        {
            // If the request cannot be canceled, return an error message
            ModelState.AddModelError(string.Empty, "This request cannot be canceled.");
            return RedirectToAction("AllRequests"); // Redirect to all requests page
        }
    }
}
