using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Dtos;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController : ControllerBase
{
    public Task<IActionResult> Create(CreateClassRequest request) => throw new NotImplementedException();
    
}