using Microsoft.AspNetCore.Mvc;
using Pdf_App.Models;
using System.Diagnostics;



using System.Text;
using UglyToad.PdfPig;

using ChatGPT.Net;
using Microsoft.Data.SqlClient;
using Pdf_App.Models.Pdf_Gpt4.Models;
using System.Security.Claims;
using Pdf_App.Data;





namespace Pdf_App.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        public string ExtractTextFromPdf(Stream pdfStream)
        {
            StringBuilder textBuilder = new StringBuilder();

            using (PdfDocument document = PdfDocument.Open(pdfStream))
            {
                foreach (var page in document.GetPages())
                {
                    textBuilder.Append(page.Text);
                }
            }

            return textBuilder.ToString();
        }

        public async Task<IActionResult> InsertPdf(String name,string summarie ,string pdf_url )
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            var pdf = new Pdf
            {
                Name = name,
                Description = summarie,
                AuthorId = User.FindFirst(ClaimTypes.NameIdentifier).Value ,//user_id,
                AuthorName = User.Identity.Name,
                PdfUrl = pdf_url
            };

            using (var connection = new SqlConnection(connectionString))
            {
                var query = "INSERT INTO Pdf (Name, Description, AuthorId, AuthorName, PdfUrl) VALUES (@Name, @Description, @AuthorId, @AuthorName, @PdfUrl)";
                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Name", pdf.Name);
                command.Parameters.AddWithValue("@Description", pdf.Description);
                command.Parameters.AddWithValue("@AuthorId", pdf.AuthorId);
                command.Parameters.AddWithValue("@AuthorName", pdf.AuthorName);
                command.Parameters.AddWithValue("@PdfUrl", pdf.PdfUrl);

                connection.Open();
                await command.ExecuteNonQueryAsync();
                connection.Close();
            }

            return Content("PDF inserted successfully!");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            

            if (file != null && file.Length > 0 && file.ContentType == "application/pdf")
            {
                try
                {
                    string Pdf_text = "";
                    string fileName = Path.GetFileName(file.FileName);
                    string Uname = "Guest";
                    if (User.Identity.Name != null) {
                        Uname = User.Identity.Name;
                    }
                    string uploadDir = "~/"+ User.Identity.Name + "/Uploads";
                    string uploadsFolder = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, uploadDir), fileName);
                    // string uploadsFolder = Path.Combine(this., "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, Path.GetFileName(file.FileName));
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        
                        await file.CopyToAsync(fileStream);
                        fileStream.Position = 0;
                        Pdf_text = ExtractTextFromPdf(fileStream);
                    }

                    Console.WriteLine($"Extracted text length: {Pdf_text.Length}");


                    int tokenLimit = 4096; // Adjust based on your token limit
                    if (Pdf_text.Length > tokenLimit)
                    {
                        Pdf_text = Pdf_text.Substring(0, tokenLimit);
                    }

                    
                    if (Pdf_text == null)
                    {
                        return BadRequest("Failed to extract text from PDF.");
                    }

                    var bot = new ChatGpt("Your Api Key");

                    //var summary = await _openAIService.GetSummary(Pdf_text);
                    var summary = await bot.Ask($"Summarize the following text:\n{Pdf_text}\n");
                    if (summary == null)
                    {
                        _logger.LogError("Failed to generate summary from OpenAI.");
                        return StatusCode(500, "Failed to generate summary from OpenAI.");
                    }

                    InsertPdf(fileName, summary, uploadsFolder);
                    ViewBag.Message = summary;
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Message = "Please select a PDF file";
            }

            
            return View("Index");
        }
    }
}
