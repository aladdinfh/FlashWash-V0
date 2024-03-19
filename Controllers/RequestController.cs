// Import necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashWash.Data;
using FlashWash.Models;

public class RequestController : Controller
{
    private readonly AppDbContext _db;

    public RequestController(AppDbContext db)
    {
        _db = db;
    }



    [SessionCheck]

    public IActionResult AllRequests()
    {
        var requests = _db.Requests.Include(r => r.Offer).Include(o => o.User).ToList();
        return View(requests);
    }
    [WashStationSessionCheck]

    public IActionResult AllRequestsRWashStation()
    {
        int? washStationId = HttpContext.Session.GetInt32("washStationId");

        if (washStationId == null)
        {
            return RedirectToAction("LogRegWash", "WashStation");
        }

        var requests = _db.Requests
            .Include(r => r.User)
            .Include(r => r.Offer)
            .ThenInclude(o => o.WashStation)
            .Where(r => r.Offer.WashStationId == washStationId)
            .ToList();

        return View(requests);
    }

    [HttpGet]
    [SessionCheck]

    public IActionResult CreateRequest(int offerId)
    {
        int userId = (int)HttpContext.Session.GetInt32("userId");

        // Check if the user has already sent a request for this offer
        var existingRequest = _db.Requests.FirstOrDefault(r => r.UserId == userId && r.OfferId == offerId);

   
        ViewBag.HasExistingRequest = existingRequest != null;

        Request newRequest = new() { OfferId = offerId, UserId = userId };
        return View(newRequest);
    }



    [HttpPost]
    [SessionCheck]
    //[ValidateAntiForgeryToken]
    public IActionResult SendRequest(Request request)
    {
        int userId = (int)HttpContext.Session.GetInt32("userId");

        // Use RequestCheck method to validate the request
        bool isValid = RequestCheck(request);

        //Console.WriteLine($"-----------------------\nRequest Created\nOfferId : {request.OfferId}\nUserId:{userId}\nStart Time: {request.StartTime}\nIs Valid : {isValid}\n------------------------------------------------------------------");

        if (isValid)
        {
            Console.WriteLine("TRUEEEEEEEEEEEEEEE");
            _db.Requests.Add(request);
            _db.SaveChanges();
            return RedirectToAction("AllRequests");
        }

        // Redirect to "CreateRequest" with offerId if the request is not valid
        return RedirectToAction("CreateRequest", new { offerId = request.OfferId });
    }



    private bool RequestCheck(Request request)
    {
        if (request == null)
            return false;
        if (request.StartTime < DateTime.Now)
        {
            return false;
        }
        else
        {
            Offer offer = _db.Offers
                .Include(o => o.Requests)
                .Include(o => o.WashStation)
                .FirstOrDefault(o => o.OfferId == request.OfferId);
            //Console.WriteLine($"*********************************************\nRequests From DB : {offer.Requests.Count()}\n*******************************************");
            //Console.WriteLine($"+++++++++++++++++++++++++++++++++++++++++++++++++++\nRequest Start Time: {request.StartTime.ToString()}\n+++++++++++++++++++++++++++++++++++++++++++++++++++");

            int hours = request.StartTime.Hour;
            int minutes = request.StartTime.Minute;
            TimeSpan ts = new TimeSpan(hours, minutes, 0);
            //Console.WriteLine($"+++++++++++++++++++++++++++++++++++++++++++++++++++\nTime Span : {ts}\n+++++++++++++++++++++++++++++++++++++++++++++++++++");
            if (ts < offer.WashStation.MorningStartTime || ts > offer.WashStation.EveningEndTime)
            {
                Console.WriteLine("FALSE===1===");
                return false;
            }
            else if (ts > offer.WashStation.MorningEndTime && ts < offer.WashStation.EveningStartTime)
            {
                Console.WriteLine("FALSE===2===");
                return false;
            }
            else if (offer.Requests.Any(r => r.StartTime < request.StartTime.AddMinutes(offer.DurationMinutes) && r.StartTime > request.StartTime.AddMinutes(-offer.DurationMinutes)))
            {
                Console.WriteLine("FALSE===3===");
                return false;
            }
        }
        return true;
    }

    [HttpPost]
    [WashStationSessionCheck]


    [ValidateAntiForgeryToken]
    public IActionResult AcceptRequest(int requestId)
    {
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        if (request != null && request.Status == RequestStatus.Pending)
        {
            // Code to accept the request
            request.Status = RequestStatus.Accepted;
            _db.SaveChanges();
        }

        return RedirectToAction("DashboardWash" , "WashStation");
    }

    [HttpPost]
    [WashStationSessionCheck]
    [ValidateAntiForgeryToken]
    public IActionResult CancelRequest(int requestId)
    {
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        if (request != null && request.Status == RequestStatus.Pending)
        {
            // Code to cancel the request
            _db.Requests.Remove(request);
            _db.SaveChanges();

            // Optionally, you may want to update the availability or status of the WashStation
            // or perform any additional cleanup.

            return RedirectToAction("AllRequestsRWashStation", "WashStation");
        }
        else
        {
            // The request cannot be canceled because it's not in a cancelable state
            // (e.g., it's already accepted or completed).
            ModelState.AddModelError(string.Empty, "This request cannot be canceled.");
            return RedirectToAction("DashboardWash", "WashStation"); // You may want to redirect to a different action or handle this case accordingly.
        }
    }


    [HttpPost]
    [SessionCheck]  // Assuming you have a similar attribute for client session check
    [ValidateAntiForgeryToken]
    public IActionResult CancelRequestUser(int requestId)
    {
        var request = _db.Requests.FirstOrDefault(r => r.RequestId == requestId);

        if (request != null && request.Status == RequestStatus.Pending)
        {
            // Code to cancel the request
            _db.Requests.Remove(request);
            _db.SaveChanges();

            // Optionally, you may want to perform any additional cleanup.

            return RedirectToAction("Dashboard", "User"); // Adjust the action and controller names accordingly
        }
        else
        {
            // The request cannot be canceled because it's not in a cancelable state
            // (e.g., it's already accepted or completed).
            ModelState.AddModelError(string.Empty, "This request cannot be canceled.");
            return RedirectToAction("AllRequests"); // You may want to redirect to a different action or handle this case accordingly.
        }
    }


}
