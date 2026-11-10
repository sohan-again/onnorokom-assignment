using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController(AppDbContext db) : ControllerBase
{
    public Task<IActionResult> Create(CreateClassRequest request) => throw new NotImplementedException();
    
}