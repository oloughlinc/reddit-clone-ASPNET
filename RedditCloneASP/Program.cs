using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;

using RedditCloneASP.Models;
using RedditCloneASP.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {

    // standard attributes for swagger
    options.SwaggerDoc("v1", new OpenApiInfo {
        Version = "v1",
        Title = "Reddit Clone ASP.NET",
        Description = "A Reddit-Style Public Forum API running on .NET 7",
        Contact = new OpenApiContact {
            Name = "Check out more from the creator on Github",
            Url = new Uri("https://github.com/oloughlinc?tab=repositories")
        }
    });
    
    // add the security type that we want to use in the swagger UI
    options.AddSecurityDefinition("JWT Bearer", new OpenApiSecurityScheme {
        Description = "This is a JWT bearer authentication scheme",
        In = ParameterLocation.Header,
        Scheme = "Bearer",
        Type = SecuritySchemeType.Http
    });
    options.OperationFilter<SwashbuckleSecurityRequirementFilter>();

    // add additional properties from XML markup: 
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "RedditCloneASP.xml"));
    
});

// Use PostgreSQL through Entity Framework as native like service
// Register the application business data store
builder.Services.AddDbContext<RedditContext>( options => {
    // https://www.npgsql.org/efcore/
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

// Use PostgreSQL through EF and Identity
// Register the authentication database, User Store
builder.Services.AddDbContext<AuthDbContext>( options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection-Auth"));
});
// Register JWT Bearer Authentication, we trust tokens with the following parameters
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters() {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "https://localhost:7023",
        ValidIssuer = "https://localhost:7023",
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MY_SUPER_SECRET_PRIVATE_KEY"))
    };
});
// Register the JWT Authorization Scheme
builder.Services.AddIdentity<RedditIdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Build Application
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed database.. 
// these seeders access context directly
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    CommentSeed.Initialize(services);
    PostSeed.Initialize(services);
}
// this seeder accesses context indirectly through Identity UserManager's async functions
// therefore, need an asynchronous scope to avoid context disposal
AuthSeed.Initialize(app.Services.CreateAsyncScope()
    .ServiceProvider.GetRequiredService<UserManager<RedditIdentityUser>>());

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
