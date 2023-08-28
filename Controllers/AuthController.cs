using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using RedditCloneASP.Models;
using RedditCloneASP.Auth;

namespace RedditCloneASP.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {

        private readonly UserManager<IdentityUser> userManager;

        public AuthController(UserManager<IdentityUser> userManager) {
            this.userManager = userManager;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] Login loginInfo) {

            if (loginInfo?.Username == null) return BadRequest();
            if (loginInfo?.Password == null) return BadRequest();

            var user = await userManager.FindByNameAsync(loginInfo.Username);
            if (user == null) return Unauthorized();

            if (await userManager.CheckPasswordAsync(user, loginInfo.Password)) {
                return Ok(AuthService.GenerateToken(user));

            } else {
                return Unauthorized();
            }
        }

    }
}