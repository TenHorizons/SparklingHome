using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparklingHome.Models;
using Microsoft.EntityFrameworkCore;
using SparklingHome.Data;
using System.IO;
using Amazon; // link to aws account
using Amazon.S3; // S3 bucket
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration; // appsettings.json
using Microsoft.AspNetCore.Http; // file transfer

namespace SparklingHome.Controllers
{
    public class MaidController : Controller
    {

        private readonly SparklingHomeContext _context;
        private readonly string bucketEndpoint = "https://sparklinghomezz.s3.amazonaws.com";

        // give related bucket name
        private const string bucketname = "sparklinghomezz";

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

        public MaidController(SparklingHomeContext context)
        {
            _context = context;
        }

        // display complete list of registered maids
        public async Task<IActionResult> Index(string msg)
        {
            ViewBag.msg = msg;
            List<Maid> maidList = await _context.Maid.ToListAsync();
            
            ViewBag.maidList = maidList;
            return View();
        }

        // create new maid record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> addNewEmployee(Maid maid, List<IFormFile> ImageUpload)
        {
            if (ModelState.IsValid)
            {
                _context.Maid.Add(maid);
                await _context.SaveChangesAsync();
                TempData["successMessage"] = "Maid details added successfully";
                // Call processImage method to upload the images to S3
                await processImage(ImageUpload, maid);
                return RedirectToAction("Index", "Maid");
            }
            else
            {
                return View("Index");
            }
        }

        // method to upload file to S3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> processImage(List<IFormFile> ImageUpload, Maid maid)
        {
            string message = "";
            List<string> keyValues = getKeysConnection();
            var S3connection = new AmazonS3Client(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);
            
            foreach(var singlefile in ImageUpload)
            {
                Console.WriteLine("Processing file: " + singlefile.FileName);
                if (singlefile.Length <= 0)
                {
                    return BadRequest("The picture you upload is empty!");
                }
                else if(singlefile.Length >= 1048576) // file cannot more than 1 MB
                {
                    return BadRequest(singlefile.FileName + "is larger than 1MB. ");
                }
                else if (singlefile.ContentType.ToLower() != "image/png" && singlefile.ContentType.ToLower() != "image/jpeg")
                {
                    return BadRequest(singlefile.FileName + " is not a valid type of image we accept!");
                }

                try
                {
                    PutObjectRequest request = new PutObjectRequest
                    {
                        InputStream = singlefile.OpenReadStream(),
                        BucketName = bucketname + "/maidImages",
                        Key = singlefile.FileName,
                        // set the public read in object once object upload
                        CannedACL = S3CannedACL.PublicRead
                    };
                    var x = await S3connection.PutObjectAsync(request);
                    maid.ImageURL = bucketEndpoint + "/maidImages/" + singlefile.FileName;
                    _context.Maid.Update(maid);
                    await _context.SaveChangesAsync();
                    message = message + singlefile.FileName + ",";
                }
                catch(AmazonS3Exception ex)
                {
                    Console.WriteLine("AmazonS3Exception: " + ex.Message);
                    return BadRequest(ex.Message);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return BadRequest(ex.Message);
                }
            }
            return RedirectToAction("Index", "Maid", new { msg = message + "successfully uploaded!" });
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
                string s3Key = maid.ImageURL.Substring(maid.ImageURL.LastIndexOf("maidImage"));
                await deleteImage(s3Key);
                _context.Maid.Remove(maid);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Maid", new { msg = "Maid record deleted" });

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Maid", new { msg = "An error occurred while attempting to remove maid data: " + ex.Message });
            }
        }

        public async Task deleteImage(string S3ImageKey)
        {
            string message = "Image of " + S3ImageKey + " is deleted from the S3 Bucket!";

            List<string> keyValues = getKeysConnection();
            var S3Connection =
                new AmazonS3Client(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);

            try
            {
                DeleteObjectRequest deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname,
                    Key = S3ImageKey
                };
                await S3Connection.DeleteObjectAsync(deleteRequest);
            }
            catch (AmazonS3Exception ex)
            {
                message = ex.Message;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
        }

        public async Task<IActionResult> displayImagesFromS3(string mssg)
        {
            ViewBag.mssg = mssg;
            List<string> keyValues = getKeysConnection();
            var S3Connection =
                new AmazonS3Client(keyValues[0], keyValues[1], keyValues[2], RegionEndpoint.USEast1);

            // create a list to keep the images return from the bucket
            List<S3Object> imagesList = new List<S3Object>();

            try
            {
                string token = null;
                do
                {
                    // get back the image object 1 by 1
                    ListObjectsRequest request = new ListObjectsRequest
                    {
                        BucketName = bucketname
                    };
                    ListObjectsResponse image = await S3Connection.ListObjectsAsync(request).ConfigureAwait(false);
                    token = image.NextMarker;
                    imagesList.AddRange(image.S3Objects);
                }
                while (token != null);
                return View(imagesList);
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

    }
}
