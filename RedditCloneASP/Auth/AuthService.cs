using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace RedditCloneASP.Auth;

public static class AuthService {

    public static AuthToken GenerateToken(IdentityUser user) {

        if (user.UserName == null) return new AuthToken();
        var authClaims = new List<Claim> {
            new Claim("username", user.UserName),
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MY_SUPER_SECRET_PRIVATE_KEY"));

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7023",
            audience: "https://localhost:7023",
            expires: DateTime.Now.AddMinutes(60),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new AuthToken() {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiry = token.ValidTo.ToString()
        };
    }
}

public class AuthToken {
    public string? Token {get; set;}
    public string? Expiry {get; set;}
}

public class SwashbuckleSecurityRequirementFilter : IOperationFilter {

    public void Apply(OpenApiOperation operation, OperationFilterContext context) {

        var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        bool requiresAuth = actionMetadata.Any(item => item is AuthorizeAttribute);
        if (!requiresAuth) return;

        //if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();
        operation.Security = new List<OpenApiSecurityRequirement> {
            new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference {
                            Id = "JWT Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, new List<string>()
                }
            }
        };
    }
}