using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using RedditCloneASP.Models;
using RedditCloneASP.Auth;
using NuGet.Common;
using Azure.Core;

namespace RedditCloneASP.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(AuthToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
    public class AuthController : ControllerBase {

        private readonly UserManager<RedditIdentityUser> userManager;

        public AuthController(UserManager<RedditIdentityUser> userManager) {
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

                var tokens = AuthService.GenerateTokens(user);
                await StoreRefreshToken(user, tokens);

                Response.Cookies.Append("refresh", tokens.Refresh ?? "", new CookieOptions() {
                    HttpOnly = true,
                    Secure = true,
                    Path = "api/Auth",
                    SameSite = SameSiteMode.None
                });

                return Ok(tokens);

            } else {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] Login loginInfo) {

            /* This endpoint is expecting refresh token as an http cookie in the request header */

            //if (loginInfo?.RefreshToken == null) return BadRequest();
            if (loginInfo?.Username == null) return BadRequest();

            var refreshToken = Request.Cookies["refresh"] ?? "";
            var user = await userManager.FindByNameAsync(loginInfo.Username);

            if (user == null) return Unauthorized(); // username doesnt exist
            //if (user.RefreshToken != loginInfo.RefreshToken) return Unauthorized(); // refresh token is wrong/revoked
            if (user.RefreshToken != refreshToken) return Unauthorized(); // refresh token is wrong/revoked
            if (user.RefreshTokenExpiry <= DateTimeOffset.UtcNow) return Unauthorized(); // refresh token is expired

            var tokens = AuthService.GenerateTokens(user);
            await StoreRefreshToken(user, tokens);
            return Ok(tokens);
        }

        private async Task StoreRefreshToken(RedditIdentityUser user, AuthToken tokens) {
            user.RefreshToken = tokens.Refresh;
            user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(1);
            await userManager.UpdateAsync(user);
        }


    }
}