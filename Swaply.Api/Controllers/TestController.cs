using Microsoft.AspNetCore.Mvc;

namespace Swaply.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Swagger is working!");
    }
}