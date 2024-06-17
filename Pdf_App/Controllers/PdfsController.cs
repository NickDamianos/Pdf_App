using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pdf_App.Data;
using Pdf_App.Models.Pdf_Gpt4.Models;

namespace Pdf_App.Controllers
{
    public class PdfsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PdfsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pdfs
        public async Task<IActionResult> Index()
        {

            return View(await _context.Pdf.Where(p=>p.AuthorName == User.Identity.Name).ToListAsync());
        }

        // GET: Pdfs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdf = await _context.Pdf
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pdf == null)
            {
                return NotFound();
            }

            return View(pdf);
        }

        // GET: Pdfs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pdfs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,AuthorId,AuthorName,PdfUrl")] Pdf pdf)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pdf);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pdf);
        }

        // GET: Pdfs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdf = await _context.Pdf.FindAsync(id);
            if (pdf == null)
            {
                return NotFound();
            }
            return View(pdf);
        }

        // POST: Pdfs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,AuthorId,AuthorName,PdfUrl")] Pdf pdf)
        {
            if (id != pdf.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pdf);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PdfExists(pdf.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pdf);
        }

        // GET: Pdfs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pdf = await _context.Pdf
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pdf == null)
            {
                return NotFound();
            }

            return View(pdf);
        }

        // POST: Pdfs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pdf = await _context.Pdf.FindAsync(id);
            if (pdf != null)
            {
                _context.Pdf.Remove(pdf);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PdfExists(int id)
        {
            return _context.Pdf.Any(e => e.Id == id);
        }
    }
}
