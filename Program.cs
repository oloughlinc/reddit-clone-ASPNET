using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RedditCloneASP.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use PostgreSQL through Entity Framework as native like service
builder.Services.AddDbContext<RedditContext>( options => {
    // https://www.npgsql.org/efcore/
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed database.. 
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    CommentSeed.Initialize(services);
    PostSeed.Initialize(services);
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();


app.MapControllers();

app.Run();
