using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MemeDump2.Data;
using MemeDump2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace MemeDump2.Controllers
{
    public class PostsController : Controller
    {
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Uploads");

        private readonly ApplicationDbContext dbContext;

        [ActivatorUtilitiesConstructor]
        public PostsController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public PostsController()
        {
            // Ellenőrzi, hogy az 'Uploads' mappa létezik-e, és hozza létre, ha nem
            if (!Directory.Exists(_imageUploadPath))
            {
                Directory.CreateDirectory(_imageUploadPath);
            }
        }

        public IActionResult Index()
        {
            var posts = dbContext.Post.ToList();
            return View(posts);
        }

        public IActionResult Search()
        {
            return View();
        }

        public async Task<IActionResult> Result(string phrase)
        {
            return View("Index", await dbContext.Post.Where(j => j.Title.Contains(phrase)).ToListAsync());
        }

        public IActionResult Details(int id)
        {
            Post post = dbContext.Post.FirstOrDefault(p => p.Id == id); ;
            return View(post);
            
        }


        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }


        [Authorize]
        [HttpPost]
        public IActionResult Create(Post model, IFormFile imageFile)
        {
            
                if (imageFile != null && imageFile.Length > 0)
                {
                    
                    string FileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(_imageUploadPath, FileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(fileStream);
                    }

                    var post = new Post
                    {
                        Title = model.Title,
                        Content = model.Content,
                        ImagePath = FileName
                    };

                    dbContext.Post.Add(post);
                    dbContext.SaveChanges();


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Képfájl nincs feltöltve.");
                }
            

            return View(model);
        }
        [Authorize]
        public IActionResult Delete(int id)
        {
            var post = dbContext.Post.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                // Kép törlése
                string filePath = Path.Combine(_imageUploadPath, post.ImagePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Törlés az adatbázisból
                dbContext.Post.Remove(post);
                dbContext.SaveChanges();
            }

            return View();
        }
        
        
        
    }
}