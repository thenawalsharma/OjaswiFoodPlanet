using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OFP.IdentityServer.Data;

var builder = WebApplication.CreateBuilder(args);


string connectionString = builder.Configuration.GetConnectionString("AppConfig");

// Load config and enable Key Vault access
builder.Configuration.AddAzureAppConfiguration(connectionString);

// Add services to the container.
builder.Services.AddDbContext<AuthDbContext>(options =>
{
     options.UseSqlServer(builder.Configuration["connectionString"]);
    //options.UseInMemoryDatabase("InMemoryDb"); // For testing purposes, use in-memory database
});

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.MapIdentityApi<IdentityUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
