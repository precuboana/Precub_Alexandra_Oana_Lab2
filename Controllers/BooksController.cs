

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Precub_Alexandra_Oana_Lab2.Data;
using Precub_Alexandra_Oana_Lab2.Models;

namespace Precub_Alexandra_Oana_Lab2.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["TitleSortParm"] = String.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["AuthorSortParm"] = sortOrder == "Author" ? "author_desc" : "Author";
            ViewData["CurrentFilter"] = searchString;

            // Start from Book query and include navigation so ordering happens in DB
            var booksQuery = _context.Book
                .Include(b => b.AuthorRef)
                .Include(b => b.Genre)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "title_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.Title);
                    break;
                case "Price":
                    booksQuery = booksQuery.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.Price);
                    break;
                case "Author":
                    booksQuery = booksQuery.OrderBy(b => b.AuthorRef.LastName).ThenBy(b => b.AuthorRef.FirstName);
                    break;
                case "author_desc":
                    booksQuery = booksQuery.OrderByDescending(b => b.AuthorRef.LastName).ThenByDescending(b => b.AuthorRef.FirstName);
                    break;
                default:
                    booksQuery = booksQuery.OrderBy(b => b.Title);
                    break;
            }

            var books = booksQuery.Select(b => new BookViewModel
            {
                ID = b.ID,
                Title = b.Title,
                Price = b.Price,
                AuthorRef = b.AuthorRef,
                Genre = b.Genre
            });

            return View(await books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genre)
                .Include(b => b.AuthorRef)
                .Include(s => s.Orders)
                .ThenInclude(e => e.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name");
            ViewData["AuthorID"] = new SelectList(
                _context.Author
                    .Select(a => new { a.ID, FullName = a.FirstName + " " + a.LastName })
                    .ToList(),
                "ID",
                "FullName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("Title,Author,Price")] Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex*/)
            {

                ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists ");
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["GenreID"] = new SelectList(_context.Set<Genre>(), "ID", "Name", book.GenreID);
            ViewData["AuthorID"] = new SelectList(
                _context.Author
                    .Select(a => new { a.ID, FullName = a.FirstName + " " + a.LastName })
                    .ToList(),
                "ID",
                "FullName",
                book.AuthorID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var bookToUpdate = await _context.Book.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Book>(
            bookToUpdate,
            "",
            s => s.AuthorID, s => s.Title, s => s.Price, s => s.GenreID))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists");
                }
            }
            ViewData["AuthorID"] = new SelectList(_context.Author, "ID", "FullName",
           bookToUpdate.AuthorID);
            return View(bookToUpdate);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .Include(b => b.Genre)
                .Include(b => b.AuthorRef)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.ID == id);
        }
    }
}