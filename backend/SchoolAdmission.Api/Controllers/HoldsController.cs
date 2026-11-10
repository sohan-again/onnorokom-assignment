using Microsoft.AspNetCore.Mvc;

namespace SchoolAdmission.Api.Controllers;

public class HoldsController: ControllerBase
{
    [HttpPost("classes/{classId}/holds")]
    public Task<IActionResult> Create(string classId, [FromHeader(Name = "X-Student-Id")] string studentId)
        => throw new NotImplementedException();

    [HttpGet("holds/{holdId}")]
    public Task<IActionResult> GetOne(string holdId) => throw new NotImplementedException();

    [HttpPost("holds/{holdId}/confirm")]
    public Task<IActionResult> Confirm(string holdId, [FromHeader(Name = "X-Student-Id")] string studentId)
        => throw new NotImplementedException();

    [HttpDelete("holds/{holdId}")]
    public Task<IActionResult> Cancel(string holdId, [FromHeader(Name = "X-Student-Id")] string studentId)
        => throw new NotImplementedException();

    [HttpGet("me/holds")]
    public Task<IActionResult> GetMine([FromHeader(Name = "X-Student-Id")] string studentId)
        => throw new NotImplementedException();
}