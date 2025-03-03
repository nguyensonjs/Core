using Core.Application.Constants;
using Core.Infra.DataContexts;
using Microsoft.EntityFrameworkCore;
using Core.Application.Extensions;
using Core.Application.InterfaceServices;
using Core.Application.ImplementServices;
using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Infra.Repo;
using Core.Application.Handle.HandleEmail;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString(AppSettingKeys.DEFAULT_CONNECTION));
});
builder.Services.AddControllers();
builder.Services.Configure<EmailConfiguration>(
    builder.Configuration.GetSection("EmailConfiguration")
);
builder.Services.AddScoped<IDbContext, AppDbContext>();
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBaseRepo<User>, BaseRepo<User>>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

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
