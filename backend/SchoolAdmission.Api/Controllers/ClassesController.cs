using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateClassRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) {
            return BadRequest(new { message = "Name is required." });
        }
        
        if (request.TotalSeats <= 0) {
            return BadRequest(new { message = "TotalSeats must be greater than 0." });
        }

        var cls = new SchoolClass { Name = request.Name.Trim(), TotalSeats = request.TotalSeats };
        db.Classes.Add(cls);
        await db.SaveChangesAsync();

        return Created($"/classes/{cls.Id}",
            new ClassResponse(cls.Id, cls.Name, cls.TotalSeats, 0, 0, cls.TotalSeats));
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var now = DateTime.UtcNow;
        var classes = await db.Classes.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        var result = new List<ClassResponse>();
        foreach (var c in classes)
        {
            var held = await db.Holds.CountAsync(h =>
                h.ClassId == c.Id && h.Status == HoldStatuses.Held && h.ExpiresAt > now);
            var confirmed = await db.Holds.CountAsync(h =>
                h.ClassId == c.Id && h.Status == HoldStatuses.Confirmed);

            result.Add(new ClassResponse(c.Id, c.Name, c.TotalSeats,
                held, confirmed, c.TotalSeats - held - confirmed));
        }
        return Ok(result);
    }
    
    [HttpGet("{classId}")]
    public async Task<IActionResult> GetOne(string classId)
    {
        var now = DateTime.UtcNow;
        var c = await db.Classes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == classId);
        if (c == null)
        {
            return NotFound(new { message = $"Class '{classId}' not found." });
        }
        
        var held = await db.Holds.CountAsync(h => 
            h.ClassId == c.Id && h.Status == HoldStatuses.Held && h.ExpiresAt > now);
        var confirmed = await db.Holds.CountAsync(h =>
            h.ClassId == c.Id && h.Status == HoldStatuses.Confirmed);
        
        var response = new ClassResponse(c.Id, c.Name, c.TotalSeats, 
            held, confirmed, c.TotalSeats - held - confirmed);

        return Ok(response);
    }
}