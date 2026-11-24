using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using EngineerLog.Api.Data;

   namespace EngineerLog.Api.Controllers;

   [ApiController]
   [Route("api/[controller]")]
   public class TagsController : ControllerBase
   {
       private readonly AppDbContext _db;

       public TagsController(AppDbContext db)
       {
           _db = db;
       }

       [HttpGet]
       public async Task<IActionResult> GetAll()
       {
           var tags = await _db.Tags.OrderBy(t => t.Name).ToListAsync();
           return Ok(tags);
       }
   }
