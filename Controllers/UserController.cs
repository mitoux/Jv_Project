using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Jovan_Project.Models;
using EmployerService.Models;

namespace Jovan_Project.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet("IsAdmin")]
        public IActionResult IsAdmin()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var isAdmin = userClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == UserRoles.Admin);
                return Ok(isAdmin);
            }
            return Ok(false);
        }
    }
}