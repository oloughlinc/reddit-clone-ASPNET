using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RedditCloneASP.Models;
using RedditCloneASP.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use PostgreSQL through Entity Framework as native like service
// Register the application business data store
builder.Services.AddDbContext<RedditContext>( options => {
    // https://www.npgsql.org/efcore/
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

// Use PostgreSQL through EF and Identity
// Register the authentication database and JWT authorization
builder.Services.AddDbContext<AuthDbContext>( options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection-Auth"));
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

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
    .ServiceProvider.GetRequiredService<UserManager<IdentityUser>>());

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
