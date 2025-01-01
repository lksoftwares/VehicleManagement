using LkDataConnection;
using VehicleManagement.Classes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = "http://localhost:7148/",
            ValidAudience = "http://localhost:7148/",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("2Fsk5LBU5j1DrPldtFmLWeO8uZ8skUzwhe3ktVimUE8l="))
        };


    });
//var jwtSettings = builder.Configuration["Jwt:Key"];
//var jwt = builder.Configuration["Jwt:Issuer"];
//var Audience = builder.Configuration["Jwt:Audience"];



// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactConnection", policy =>
    {
        policy.WithOrigins("*")
        .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ConnectionClass>();

var app = builder.Build();

app.UseCors("ReactConnection");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
ConnectionClass connectionClass = new ConnectionClass(builder.Configuration);
LkDataConnection.Connection.ConnectionStr = connectionClass.GetSqlConnection().ConnectionString;
LkDataConnection.Connection.Connect();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();
app.MapControllers();

app.Run();
