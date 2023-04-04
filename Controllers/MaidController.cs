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

        public MaidController(SparklingHomeContext context)
        {
            _context = context;
        }

        // display complete list of registered maids
        public async Task<IActionResult> Index()
        {
            List<Maid> maidList = await _context.Maid.ToListAsync();
            ViewBag.maidList = maidList;
            return View();
        }

        // create new maid record
        [HttpPost]
        [ValidateAntiForgeryTokenAttribute]
        public async Task<ActionResult> addNewEmployee(Maid maid)
        {
            if (ModelState.IsValid)
            {
                _context.Maid.Add(maid);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Maid details added successfully";
                return RedirectToAction("Index", "Maid");
            }
            else
            {
                return View("Index");
            }
        }

        // method to redirect user to maid edit page
        public async Task<IActionResult> MaidDetailsPage(int? maidId)
        {
            if (maidId == null)
            {
                return NotFound();
            }
            Maid maidFound = await _context.Maid.FindAsync(maidId);
            /*ViewBag.ErrorMessages = errorMessages != null ? errorMessages : null;*/
            return View(maidFound);
        }

        // update maid record
        public async Task<IActionResult> UpdateMaidDetails(Maid maid)
        {
            string message = "Maid " + maid.MaidId + " details successfully updated!";
            if (ModelState.IsValid)
            {
                _context.Maid.Update(maid);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Maid", new { msg = message });
            }
            else
            {
                // NOTE: error messages not being added to list, null value returned to view
                /*List<string> errorMessages = new List<string>();*/
                string errorMessage = "Invalid maid details";

                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    if (error.ErrorMessage != "")
                    {
                        errorMessage = error.ErrorMessage;
                    }
                }
                TempData["errorMessage"] = errorMessage;
                return RedirectToAction("MaidDetailsPage", "Maid", new { maidId = maid.MaidId });
            }
        }

        // delete maid record
        public async Task<IActionResult> DeleteMaidRecord(int? maidId)
        {
            if (maidId == null)
            {
                return NotFound();
            }

            try
            {
                Maid maid = await _context.Maid.FindAsync(maidId);
                _context.Maid.Remove(maid);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Maid", new { msg = "Maid record deleted" });

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Maid", new { msg = "An error occurred while attempting to remove maid data: " + ex.Message });
            }
        }
    }
}
