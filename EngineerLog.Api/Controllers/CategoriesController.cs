using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using EngineerLog.Api.Data;

   namespace EngineerLog.Api.Controllers;

   [ApiController]
   [Route("api/[controller]")]
   public class CategoriesController : ControllerBase
   {
       private readonly AppDbContext _db;

       public CategoriesController(AppDbContext db)
       {
           _db = db;
       }

       [HttpGet]
       public async Task<IActionResult> GetAll()
       {
           var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
           return Ok(categories);
       }
   }
