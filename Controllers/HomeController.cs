using System.Diagnostics;
using CookBook.Data;
using Microsoft.AspNetCore.Mvc;
using CookBook.Models;
using Microsoft.EntityFrameworkCore;

namespace CookBook.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public IActionResult Index(string searchQuery, string sortOrder, string categoryIds, int page = 1)
    {
        int pageSize = 8;
        
        var allRecipes = _context.Recipes
            .Include(r => r.User)
            .Include(r => r.Category)
            .ToList();
        
        var allCategories = _context.Categories.OrderBy(c => c.Name).ToList();
        ViewBag.AllCategories = allCategories;
        
        List<int> selectedCategoryIds = new List<int>();
        if (!string.IsNullOrWhiteSpace(categoryIds))
        {
            selectedCategoryIds = categoryIds.Split(',')
                .Where(id => int.TryParse(id, out _))
                .Select(int.Parse)
                .ToList();
        }
        ViewBag.SelectedCategoryIds = selectedCategoryIds;
    
        bool isSearching = !string.IsNullOrWhiteSpace(searchQuery);
        IEnumerable<Recipe> filteredRecipes = allRecipes;
    
        if (isSearching)
        {
            var lowerQuery = searchQuery.Trim().ToLower();
            filteredRecipes = allRecipes
                .Where(r => r.Title != null && r.Title.Trim().ToLower().Contains(lowerQuery));
        }
        
        if (selectedCategoryIds.Any())
            filteredRecipes = filteredRecipes.Where(r => selectedCategoryIds.Contains(r.CategoryId));
        
        
        ViewBag.IsSearching = isSearching;
        ViewBag.SearchQuery = searchQuery;
        ViewBag.CurrentSort = sortOrder;
        ViewBag.CategoryIds = categoryIds;

        switch (sortOrder)
        {
            case "newest":
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.CreatedAt);
                break;
            case "oldest":
                filteredRecipes = filteredRecipes.OrderBy(r => r.CreatedAt);
                break;
            case "top_rated":
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.Rating ?? 0);
                break;
            case "most_rated":
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.RatingCount).ThenByDescending(r => r.Rating ?? 0);
                break;
            case "time_asc":
                filteredRecipes = filteredRecipes.OrderBy(r => r.CookingTime).ThenByDescending(r => r.Rating ?? 0);
                break;
            case "time_desc":
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.CookingTime).ThenByDescending(r => r.Rating ?? 0);
                break;
            default:
                filteredRecipes = filteredRecipes.OrderByDescending(r => r.Rating ?? 0);
                break;
        }
    
        var recipes = filteredRecipes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    
        int totalItems = filteredRecipes.Count();
    
        var paginationInfo = new PaginationInfo
        {
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            PageSize = pageSize,
            TotalItems = totalItems
        };
    
        ViewBag.Pagination = paginationInfo;
    
        return View(recipes);
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
}