using CookBook.Data;
using CookBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CookBook.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public IActionResult Index()
    {
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Name")] Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Add(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public IActionResult Details(int? id)
    {
        if (id == null)
            return NotFound();
        
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound();
        return View(category);
    }

    public IActionResult Edit(int? id)
    {
        if (id == null)
            return NotFound();
        
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Id,Name")] Category category)
    {
        if (id != category.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExist(category.Id))
                    return NotFound();
                else 
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
            return NotFound();
        
        var category = _context.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
            return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
    
    private bool CategoryExist(int id)
    {
        return _context.Categories.Any(c => c.Id == id);
    }
}