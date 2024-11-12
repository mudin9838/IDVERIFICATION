using IDVERIFICATION.Data;
using IDVERIFICATION.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Imaging; // For ImageFormat
using ZXing;

namespace IDVERIFICATION.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Employees.Add(employee);
                    _context.SaveChanges(); // Save to get the Id populated

                    // Generate QR code after saving to get a valid ID
                    employee.QRCodeUrl = GenerateQRCodeUrl(employee.Id.ToString());

                    // Log the generated QR code URL for debugging
                    Console.WriteLine($"Generated QR Code URL: {employee.QRCodeUrl}");

                    // Save the QR code URL
                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saving employee: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again.");
                }
            }
            return View(employee);
        }

        public IActionResult Verify(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }


        private string GenerateQRCodeUrl(string id)
        {
            // Create the URL for the Verify action
            string url = Url.Action("Verify", "Employee", new { id }, Request.Scheme);

            // Log the generated URL for debugging
            Console.WriteLine($"Generated QR Code URL: {url}");

            // Create a BarcodeWriter instance (non-generic)
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300,
                    Margin = 1
                }
            };

            // Generate the QR code bitmap
            using var bitmap = writer.Write(url);

            // Ensure the directory exists
            string qrCodeDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qrcodes");
            if (!Directory.Exists(qrCodeDirectory))
            {
                Directory.CreateDirectory(qrCodeDirectory);
            }

            // Save the bitmap to a file
            string qrCodePath = Path.Combine(qrCodeDirectory, $"{id}.png");
            bitmap.Save(qrCodePath, ImageFormat.Png);

            return $"/qrcodes/{id}.png"; // Return the relative URL to the QR code image
        }
    }
}
