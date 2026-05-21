using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace FR_WS2_BaseLab.Controllers;
 
public class CategoryImagesController : Controller
{
    private readonly FrWs2BaselabContext _context;
    private readonly IWebHostEnvironment _env;

    //fichiers acceptés
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;
    public CategoryImagesController(FrWs2BaselabContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // POST upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int id, IFormFile file)
    {   //catégorie existe?
         var category = await _context.Categories.FindAsync(id);
         if (category == null)
            return NotFound(new { success = false, message = "Catégorie introuvable." });
        //valider le fichier
         if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "Aucun fichier reçu." });
 
        if (file.Length > MaxFileSizeInBytes)
            return BadRequest(new { success = false, message = "Le fichier ne doit pas dépasser 5 Mo." });
 
        if (!AllowedContentTypes.Contains(file.ContentType.ToLower()))
            return BadRequest(new { success = false, message = "Type de fichier non accepté. Utilisez JPG, PNG, GIF ou WEBP." });
 
        //création du dossier sil n'esxiste pas
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "categories");
        Directory.CreateDirectory(uploadsFolder);

        // méthode guid
        var extension = Path.GetExtension(file.FileName).ToLower();
        var newFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, newFileName);

        //load du fichier sur serveur
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        //enregistrement en base de données
        var categoryImage = new CategoryImage
        {
            CategoryId = id,
            FileName = newFileName,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow
        };

        _context.CategoryImages.Add(categoryImage);
        await _context.SaveChangesAsync();
        //retour de la réponse en JSON pour le FETCH côté client
        return Ok(new { success = true,
             id = categoryImage.Id,
            url = $"/uploads/categories/{newFileName}",
            originalFileName = file.FileName,
            sizeInBytes = file.Length
        });
    }

    // POST delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _context.CategoryImages.FindAsync(id);
        if (image == null)
            return NotFound(new { success = false });
 
        // Supprimer le fichier du serveur
        var filePath = Path.Combine(_env.WebRootPath, "uploads", "categories", image.FileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
 
        // Supprimer le fichier  de la BD
        _context.CategoryImages.Remove(image);
        await _context.SaveChangesAsync();
 
        return Ok(new { success = true });
    }
}


 