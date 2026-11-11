using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Domain;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class HoldsController: ControllerBase
{
    private readonly AppDbContext db;
    private readonly IConfiguration _configuration;

    public HoldsController(AppDbContext db, IConfiguration configuration)
    {
        this.db = db;
        this._configuration = configuration;
    }
    
    [HttpPost("classes/{classId}/holds")]
    public async Task<IActionResult> Create(string classId, [FromHeader(Name = "X-Student-Id")] string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {    
            return BadRequest(new { message = "X-Student-Id header is required." });
        }

        var now = DateTime.UtcNow;
        var holdSeconds = int.TryParse(_configuration.GetValue<string>("HOLD_DURATION_SECONDS"), out var s) ? s : 600;

        await using (var tx = await db.Database.BeginTransactionAsync()) {

            // Lock the class row. A second request for the SAME class waits here
            var cls = await db.Classes
                .FromSqlInterpolated($"SELECT * FROM \"Classes\" WHERE \"Id\" = {classId} FOR UPDATE")
                .SingleOrDefaultAsync();

            if (cls is null) 
            {
                return NotFound(new { message = "Class not found." });
            }

            // Clear this student's stale holds
            await db.Holds
                .Where(h => h.ClassId == classId && h.StudentId == studentId
                            && h.Status == HoldStatuses.Held && h.ExpiresAt <= now)
                .ExecuteUpdateAsync(u => u.SetProperty(h => h.Status, HoldStatuses.Expired));

            var held = await db.Holds.CountAsync(h =>
                h.ClassId == classId && h.Status == HoldStatuses.Held && h.ExpiresAt > now);
            var confirmed = await db.Holds.CountAsync(h =>
                h.ClassId == classId && h.Status == HoldStatuses.Confirmed);

            if (held + confirmed >= cls.TotalSeats)
            {
                return Conflict(new { message = "No seats available for this class." });
            }
        

            var alreadyApplied = await db.Holds.AnyAsync(h => h.ClassId == classId && h.StudentId == studentId
                && (h.Status == HoldStatuses.Confirmed || (h.Status == HoldStatuses.Held && h.ExpiresAt > now)));

            if (alreadyApplied)
            {
                return Conflict(new { message = "You already have an active application for this class." });
            }
            var hold = new Hold
            {
                ClassId = classId,
                StudentId = studentId,
                Status = HoldStatuses.Held,
                ExpiresAt = now.AddSeconds(holdSeconds),
            };
            db.Holds.Add(hold);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Created($"/holds/{hold.Id}",
                new HoldCreatedResponse(hold.Id, hold.ClassId, hold.StudentId, hold.Status, hold.ExpiresAt));
        }    
    }

    [HttpGet("holds/{holdId}")]
    public async Task<IActionResult> GetOne(string holdId)
    {
        var hold = await db.Holds.AsNoTracking().FirstOrDefaultAsync(h => h.Id == holdId);
        if (hold is null) 
        {
            return NotFound(new { message = "Application not found." });
        }
        
        var status = hold.Status == HoldStatuses.Held && hold.ExpiresAt <= DateTime.UtcNow
            ? HoldStatuses.Expired
            : hold.Status;

        return Ok(new HoldResponse(hold.Id, hold.ClassId, status, hold.ExpiresAt));
    }

    [HttpPost("holds/{holdId}/confirm")]
    public async Task<IActionResult> Confirm(string holdId, [FromHeader(Name = "X-Student-Id")] string studentId)
    { 
        var hold = await db.Holds.FirstOrDefaultAsync(h => h.Id == holdId);
        if (hold is null || hold.StudentId != studentId)
        {
            return NotFound(new { message = "Application not found." });
        }

        if (hold.Status == HoldStatuses.Confirmed)
        {
            return Ok(new ConfirmResponse(hold.Id, hold.Status));
        }

        if (hold.Status != HoldStatuses.Held || hold.ExpiresAt <= DateTime.UtcNow)
        {
            return Conflict(new { message = "This application can no longer be confirmed." });
        }

        hold.Status = HoldStatuses.Confirmed;
        await db.SaveChangesAsync();
        return Ok(new ConfirmResponse(hold.Id, hold.Status));
    }

    [HttpDelete("holds/{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, [FromHeader(Name = "X-Student-Id")] string studentId)
    { 
        var hold = await db.Holds.FirstOrDefaultAsync(h => h.Id == holdId);
        if (hold is null || hold.StudentId != studentId)
        {
            return NotFound(new { message = "Application not found." });
        }

        if (hold.Status == HoldStatuses.Cancelled) 
        {
            return NoContent();
        }
        if (hold.Status == HoldStatuses.Confirmed)
        {   
            return Conflict(new { message = "A confirmed application cannot be cancelled." });
        }

        hold.Status = HoldStatuses.Cancelled;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("me/holds")]
    public async Task<IActionResult> GetMine([FromHeader(Name = "X-Student-Id")] string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return BadRequest(new { message = "X-Student-Id header is required." });
        }
        var now = DateTime.UtcNow;
        var rows = await db.Holds.AsNoTracking()
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new { h.Id, h.ClassId, ClassName = h.Class.Name, h.Status, h.ExpiresAt })
            .ToListAsync();

        var result = rows.Select(h => new MyHoldResponse(
            h.Id, h.ClassId, h.ClassName,
            h.Status == HoldStatuses.Held && h.ExpiresAt <= now ? HoldStatuses.Expired : h.Status,
            h.ExpiresAt));

        return Ok(result);
    }
}