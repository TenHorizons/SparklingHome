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
using Amazon; // link to aws account
using Amazon.S3; // S3 bucket
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration; // appsettings.json
using Microsoft.AspNetCore.Http; // file transfer

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
            if (ReservationId == null || String.IsNullOrEmpty(ReservationId))
            {
                return NotFound();
            }
            else
            {
                Reservation foundReservation = await _context.Reservations
                    .Include(e => e.Maid)
                    .FirstOrDefaultAsync(e => e.ReservationId == int.Parse(ReservationId));

                var user = await _userManager.GetUserAsync(HttpContext.User);

                Tuple<Reservation, SparklingHomeUser> details =
                    new Tuple<Reservation, SparklingHomeUser>(foundReservation, user);

                return View(details);
            }
        }

        public async Task<IActionResult> UpdateReservationStatus(bool decision, string ReservationId)
        {
            Reservation foundReservation = await _context.Reservations.FindAsync(int.Parse(ReservationId));

            foundReservation.ReservationStatus = decision;

            if (ModelState.IsValid)
            {
                _context.Reservations.Update(foundReservation);
                await _context.SaveChangesAsync();
                TempData["reservationUpdateMessage"] = "Reservation updated!";
                return RedirectToAction("ReservationDetails", "Reservation", new { ReservationId });
            }
            else
            {
                TempData["reservationErrorMessage"] = "Unable to update reservation. Please try again later";
                return RedirectToAction("ReservationDetails", "Reservation", new { ReservationId });
            }

        }

        public async Task<IActionResult> displayMaidPicture()
        {
            List<string> keyValues = getKeysConnection();
            var S3connection = new AmazonS3Client(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);
            List<S3Object> maidImagesList = new List<S3Object>();

            try
            {
                string token = null;
                do
                {
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = bucketname
                    };

                    ListObjectsResponse image = await S3connection.ListObjectsAsync(request).ConfigureAwait(false);
                    token = image.NextMarker;
                    maidImagesList.AddRange(image.S3Objects);
                }
                while (token != null);
                return View(maidImagesList);
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest("error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("error: " + ex.Message);
            }
        }

        public async Task<IActionResult> DisplayMaidPicture(int id)
        {
            List<string> keyValues = getKeysConnection();
            var S3connection = new AmazonS3Client(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);
            string imageName = $"maid_{id}";

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketname,
                    Key = imageName
                };

                using (GetObjectResponse response = await S3connection.GetObjectAsync(request))
                {
                    using (Stream stream = response.ResponseStream)
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        byte[] bytes = memoryStream.ToArray();
                        string extension = response.Headers["Content-Type"].Split('/')[1];
                        string imageBase64Data = Convert.ToBase64String(bytes);
                        string imageDataURL = string.Format($"data:image/{extension};base64,{imageBase64Data}");
                        ViewBag.ImageDataUrl = imageDataURL;
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
            return View();
        }
    }
}
