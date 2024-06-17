using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pdf_App.Models.Pdf_Gpt4.Models;

namespace Pdf_App.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Pdf_App.Models.Pdf_Gpt4.Models.Pdf> Pdf { get; set; } = default!;
    }
}
