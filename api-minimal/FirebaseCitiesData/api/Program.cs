using api;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var projectId = "dotnet7-firebase-demo";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options=>
    {
        var isProduction = builder.Environment.IsProduction();
        var issuer = $"https://securetoken.google.com/{projectId}";
        options.Authority = issuer;
        options.TokenValidationParameters.ValidAudience = projectId;
        options.TokenValidationParameters.ValidIssuer = issuer;
        options.TokenValidationParameters.ValidateIssuer = isProduction;
        options.TokenValidationParameters.ValidateAudience = isProduction;
        options.TokenValidationParameters.ValidateLifetime = isProduction;
        options.TokenValidationParameters.RequireSignedTokens = isProduction;

        if (isProduction)
        {
            var jwtKeySetUrl = "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";
            options.TokenValidationParameters.IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) => {
                // get JsonWebKeySet from AWS
                var keyset = new HttpClient()
                    .GetFromJsonAsync<Dictionary<string, string>>(jwtKeySetUrl).Result;

                // serialize the result
                var keys = keyset!.Values.Select(
                    d => new X509SecurityKey(new X509Certificate2(Encoding.UTF8.GetBytes(d))));

                // cast the result to be the type expected by IssuerSigningKeyResolver
                return keys;
            };
        }
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();




app.MapGet("/city/add/{state}/{name}",
  [Authorize] async (string state, string name) =>
  {
      var firestore = new FirestoreDbBuilder
      {
          ProjectId = projectId,
          EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
      }
      .Build();

      var collection = firestore.Collection("cities");
      await collection.Document(Guid.NewGuid().ToString("N")).SetAsync(
          new City(name, state)
      );
  })
  .WithName("AddCity");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
