using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using CookBook.Models;
using CookBook.Data;
using CookBook.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CookBook.Controllers;

public class RecipeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IImageService _imageService;
    
    public RecipeController(ApplicationDbContext context,  UserManager<IdentityUser> userManager, IImageService imageService)
    {
        _context = context;
        _userManager = userManager;
        _imageService = imageService;
    }

    public IActionResult Index()
    {
        var  userId = _userManager.GetUserId(User);
        
        var recipes = _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        
        return View(recipes);
    }

    public async Task<IActionResult> Details(int? id, string returnUrl = null)
    {
        if (id == null)
            return NotFound();
        
        var recipe = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (recipe == null)
            return NotFound();
        
        var userId = _userManager.GetUserId(User);
        double? userRating = null;

        if (!string.IsNullOrEmpty(userId))
        {
            var existingRating = await  _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId &&  r.RecipeId == id);
            
            if (existingRating != null)
                userRating = existingRating.Value;
        }
            
        ViewBag.CanEdit = (userId == recipe.UserId);
        ViewBag.UserRating = userRating;
        ViewBag.ReturnUrl = returnUrl;
        
        return View(recipe);
    }
    
    [Authorize]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("Title,Instruction,Ingredients,CookingTime,CategoryId,ImageFile")] Recipe recipe)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);

            if (recipe.ImageFile != null)
            {
                try
                {
                    recipe.ImagePath = await _imageService.SaveImageAsync(recipe.ImageFile, recipe.Title); 
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                    ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
                    return View(recipe);
                }
            }
            
            recipe.UserId = userId;
            recipe.CreatedAt = DateTime.Now;
            _context.Add(recipe);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    
        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name");
        return View(recipe);
    }

    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var recipe = await _context.Recipes
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
    
        if (recipe == null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (recipe.UserId != userId)
            return Forbid();

        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", recipe.CategoryId);
    
        return View(recipe);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Instruction,Ingredients,CookingTime,CategoryId,ImageFile")] Recipe recipe)
    {
        if (id != recipe.Id)
            return NotFound();
        
        var originalRecipe = await _context.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        if (originalRecipe == null)
            return NotFound();
        
        var userId = _userManager.GetUserId(User);
        if (originalRecipe.UserId != userId)
            return Forbid();
        
        recipe.Rating = originalRecipe.Rating;
        recipe.RatingCount = originalRecipe.RatingCount;
        
        ModelState.Remove("ImageFile");
        ModelState.Remove("Category");
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            try
            {
                var oldImagePath = originalRecipe.ImagePath;

                if (recipe.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(oldImagePath))
                        _imageService.DeleteImage(oldImagePath);
                    
                    recipe.ImagePath = await _imageService.SaveImageAsync(recipe.ImageFile, recipe.Title);
                }
                else
                    recipe.ImagePath = originalRecipe.ImagePath;
                
                recipe.UserId = originalRecipe.UserId;
                recipe.CreatedAt = originalRecipe.CreatedAt;
                _context.Update(recipe);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Recipes.Any(r => r.Id == recipe.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", recipe.CategoryId);
        return View(recipe);
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
            return NotFound();
        
        var recipe = _context.Recipes
            .Include(r => r.Category)
            .Include(r =>  r.User)
            .FirstOrDefault(r => r.Id == id);

        if (recipe == null)
            return NotFound();
        
        var userId = _userManager.GetUserId(User);
        if (recipe.UserId != userId)
            return Forbid();
        
        return View(recipe);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe == null)
            return NotFound();
        
        var userId = _userManager.GetUserId(User);

        if (recipe.UserId != userId)
            return Forbid();
        
        if (!string.IsNullOrEmpty(recipe.ImagePath))
            _imageService.DeleteImage(recipe.ImagePath);
        
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate([FromBody] RateRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var recipe = await _context.Recipes
                .Include(r => r.Ratings)
                .FirstOrDefaultAsync(r => r.Id == request.RecipeId);
            
            if (recipe == null)
                return NotFound(new {error = "Рецепт не найден"});
            
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == request.RecipeId && r.UserId == userId);

            if (existingRating != null)
                return Conflict(new { error = "Вы уже оценивали этот рецепт" });

            var rating = new Rating
            {
                RecipeId = request.RecipeId,
                UserId = userId,
                Value = request.Rating,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Ratings.Add(rating);

            var allRatings = await _context.Ratings
                .Where(r => r.RecipeId == request.RecipeId)
                .Select(r => r.Value)
                .ToListAsync();
            
            allRatings.Add(request.Rating);
            
            recipe.Rating = Math.Round(allRatings.Average(),1);
            recipe.RatingCount = allRatings.Count;
            
            _context.Update(recipe);
            await _context.SaveChangesAsync();
            
            var responseRating = recipe.Rating.HasValue 
                ? Math.Round(recipe.Rating.Value, 1) 
                : 0;
            
            return Ok(new
            {
                averageRating = responseRating,
                ratingCount = recipe.RatingCount,
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public class RateRequest
    {
        public int RecipeId { get; set; }
        public double Rating { get; set; }
    }
}