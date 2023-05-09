using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparklingHome.Data;
using SparklingHome.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SparklingHome.Areas.Identity.Data;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration; // appsettings.json

namespace SparklingHome.Controllers
{
    public class ReservationController : Controller
    {

        private readonly SparklingHomeContext _context;
        private readonly UserManager<SparklingHomeUser> _userManager;

        // give related bucket name
        private const string bucketname = "sparklinghomez";

        // make connection to AWS
        private List<string> getKeysConnection()
        {
            List<string> keysList = new List<string>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfiguration configure = builder.Build();

            keysList.Add(configure["keys:key1"]);
            keysList.Add(configure["keys:key2"]);
            keysList.Add(configure["keys:key3"]);

            return keysList;
        }

        public ReservationController(
            SparklingHomeContext context,
            UserManager<SparklingHomeUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            List<Reservation> reservationList = user.UserType == "Admin" ?
                await _context.Reservations.ToListAsync() :
                await _context.Reservations
                .Where(e => e.CustomerID == user.Id).ToListAsync();

            List<SelectListItem> postcodes = reservationList
                .Select(reservation => new SelectListItem
                {
                    Text = reservation.Postcode.ToString(),
                    Value = reservation.Postcode.ToString()
                })
                .GroupBy(item => item.Value)
                .Select(item => item.First())
                .ToList();

            List<Reservation> filteredReservations = null;

            if (Request.Query.ContainsKey("filteredReservations"))
            {
                filteredReservations = JsonConvert
                    .DeserializeObject<List<Reservation>>(Request.Query["filteredReservations"]);
            }

            List<Reservation> listToSend = filteredReservations ?? reservationList;

            Tuple<List<Reservation>, List<SelectListItem>> reservationPostcodeMap =
                new Tuple<List<Reservation>, List<SelectListItem>>(listToSend, postcodes);

            return View(reservationPostcodeMap);
        }

        public async Task<IActionResult> GetAllReservations()
        {
            List<Reservation> reservations = await _context.Reservations
                .OrderBy(e => e.ReservationStatus == false).ToListAsync();
            return View(reservations);
        }

        public async Task<IActionResult> FilterReservationsBy(
            int? Postcode,
            bool? ReservationStatus,
            TIMESLOT? Timeslot,
            SERVICE_TYPE? ServiceType
        )
        {
            var filterQuery = _context.Reservations.AsQueryable();

            if (Postcode.HasValue)
            {
                filterQuery = filterQuery
                    .Where(reservation => reservation.Postcode == Postcode);
            }

            if (ReservationStatus.HasValue)
            {
                filterQuery = filterQuery
                    .Where(reservation => reservation.ReservationStatus == ReservationStatus);
            }

            if (Timeslot.HasValue)
            {
                filterQuery = filterQuery
                    .Where(reservation => reservation.Timeslot == Timeslot);
            }

            if (ServiceType.HasValue)
            {
                filterQuery = filterQuery
                    .Where(reservation => reservation.ServiceType == ServiceType);
            }

            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user.UserType == "User")
            {
                filterQuery = filterQuery
                    .Where(reservation => reservation.CustomerID == user.Id);
            }

            List<Reservation> filteredReservations = await filterQuery.ToListAsync();
            string filteredReservationsJson = JsonConvert.SerializeObject(filteredReservations);

            return RedirectToAction("Index", "Reservation", new { filteredReservations = filteredReservationsJson });
        }

        public async Task<IActionResult> ReservationCreation()
        {
            var maidList = await _context.Maid.ToListAsync();
            ViewBag.maidList = maidList;
            return View();
        }

        public async Task<IActionResult> MakeReservation(Reservation newReservation)
        {
            string userUid = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            newReservation.CustomerID = userUid;

            if (ModelState.IsValid)
            {
                _context.Reservations.Add(newReservation);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "You created a new reservation with reference ID "
                    + newReservation.ReservationId +
                    ". You will be notified once the reservation status is confiemd by the admin.";
                return RedirectToAction("Index", "Reservation");
            }

            return View("ReservationCreation");
        }

        public async Task<IActionResult> ReservationDetails(string ReservationId)
        {
            if (ReservationId == null || string.IsNullOrEmpty(ReservationId))
            {
                return NotFound();
            }
            else
            {
                Reservation foundReservation = await _context.Reservations
                    .Include(e => e.Maid)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.ReservationId == int.Parse(ReservationId));

                var user = await _userManager.GetUserAsync(HttpContext.User);

                Tuple<Reservation, SparklingHomeUser> details =
                    new Tuple<Reservation, SparklingHomeUser>(foundReservation, user);

                return View(details);
            }
        }

        public async Task<IActionResult> UpdateReservationStatus(bool decision, string ReservationId, string CustomerId)
        {
            Reservation foundReservation = await _context.Reservations.FindAsync(int.Parse(ReservationId));

            foundReservation.ReservationStatus = decision;

            if (ModelState.IsValid)
            {
                NotificationController notificationController = new NotificationController();
                bool response = await notificationController.QueueMessage(ReservationId, CustomerId);

                if (response == true)
                {
                    _context.Reservations.Update(foundReservation);
                    await _context.SaveChangesAsync();

                    TempData["reservationUpdateMessage"] = "Reservation updated!";
                    return RedirectToAction("ReservationDetails", "Reservation", new { ReservationId });
                }
                else {
                    TempData["reservationErrorMessage"] = "Unable to queue message. Please try again later";
                    return RedirectToAction("ReservationDetails", "Reservation", new { ReservationId });
                }
            }
            else
            {
                TempData["reservationErrorMessage"] = "Unable to update reservation. Please try again later";
                return RedirectToAction("ReservationDetails", "Reservation", new { ReservationId });
            }

        }

    }
}
