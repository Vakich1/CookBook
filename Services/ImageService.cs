using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace CookBook.Services;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile imageFile,  string recipeTitle);
    void DeleteImage(string imagePath);
}

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _imagesFolder;
    private readonly int _targetWidth = 800;
    private readonly int _targetHeight = 600;
    private readonly int _thumbnaiWidth = 300;
    private readonly int _thumbnaiHeight = 200;

    public ImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _imagesFolder = Path.Combine(_environment.WebRootPath, "uploads", "recipes");
        
        if (!Directory.Exists(_imagesFolder))
            Directory.CreateDirectory(_imagesFolder);
            
    }

    public async Task<string> SaveImageAsync(IFormFile imageFile, string recipeTitle)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new InvalidOperationException("Недопустимый формат файла.Разрешены: jpg, jpeg, png, gif");

        var safeTitle = string.Join("_", recipeTitle.Split(Path.GetInvalidFileNameChars()));
        var fileName = $"{safeTitle}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_imagesFolder, fileName);

        using (var stream = new MemoryStream())
        {
            await imageFile.CopyToAsync(stream);
            stream.Position = 0;

            using (var originalImage = Image.FromStream(stream))
            {
                using (var resizedImage = ResizeImage(originalImage, _targetWidth, _targetHeight))
                {
                    resizedImage.Save(filePath, GetImageFormat(fileExtension));
                }
                
                var thumbnailPath = Path.Combine(_imagesFolder, "thumb_" + fileName);
                using (var thumbnail = ResizeImage(originalImage, _targetWidth, _targetHeight))
                {
                    thumbnail.Save(thumbnailPath, GetImageFormat(fileExtension));
                }
            }
        }
        
        return $"/uploads/recipes/{fileName}";
    }

    public void DeleteImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;
        
        var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        
        var thumbnailPath = Path.Combine(Path.GetDirectoryName(fullPath), "thumb_" +  Path.GetFileName(fullPath));
        if (File.Exists(thumbnailPath))
            File.Delete(thumbnailPath);
    }

    private Image ResizeImage(Image image, int targetWidth, int targetHeight)
    {
        var destRect = new Rectangle(0, 0, targetWidth, targetHeight);
        var destImage = new Bitmap(targetWidth, targetHeight);
        
        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        
        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }
        
        return destImage;
    }

    private ImageFormat GetImageFormat(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".gif" => ImageFormat.Gif,
            _ => ImageFormat.Jpeg
        };
    }
}