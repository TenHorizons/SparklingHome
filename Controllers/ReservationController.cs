using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparklingHome.Data;
using SparklingHome.Models;
using Microsoft.EntityFrameworkCore;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SparklingHome.Controllers
{
    public class ReservationController : Controller
    {

        private readonly SparklingHomeContext _context;


        public ReservationController(SparklingHomeContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Reservation> reservationList = await _context.Reservations.ToListAsync();
            ViewBag.reservationList = reservationList;
            return View(reservationList);
        }

        public IActionResult ReservationCreation() {
            var maidList = _context.Maid.FromSqlRaw("select * from Maid").ToList<Maid>();
            ViewBag.maidList = maidList;
            return View();
        }






    }
}
