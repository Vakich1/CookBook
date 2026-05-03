namespace CookBook.Models;

public class PaginationInfo
{
    public  int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    
    public int StartPage => Math.Max(1, CurrentPage - 2);
    public int EndPage => Math.Min(TotalPages, CurrentPage + 2);
}