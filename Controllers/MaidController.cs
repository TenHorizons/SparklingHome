using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparklingHome.Models;
using Microsoft.EntityFrameworkCore;
using SparklingHome.Data;

namespace SparklingHome.Controllers
{
    public class MaidController : Controller
    {

        private readonly SparklingHomeContext _context;

        public MaidController(SparklingHomeContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Maid> maidList = await _context.Maid.ToListAsync();
            ViewBag.maidList = maidList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<ActionResult> addNewEmployee(Maid maid) {
            if (ModelState.IsValid)
            {
                _context.Maid.Add(maid);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Maid");
            }
            else {
                return View("Index");
            }
        }


    }
}
