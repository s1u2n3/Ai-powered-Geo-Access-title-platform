using Microsoft.EntityFrameworkCore;
using WebApplication1.Services.Interfaces;
//using WebApplication1.Services.Interfaces;
using WebApplication1.Repositories.implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Implementations;
using WebApplication1.Seeder;
using WebApplication1.Services.Implementaions;
using WebApplication1.Repositories.Implementations;
//jwt things//
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Pgvector.EntityFrameworkCore;

using NetTopologySuite.IO.Converters;
using WebApplication1.Repositories.Interfaces.DocumentProcessing;
using WebApplication1.Repositories.implementations.DocumentProcessing;
using WebApplication1.Repositories.implementations.PropertyStarer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();//this is for normal json reply 
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new GeoJsonConverterFactory());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter JWT Token"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType
                                .SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<PostgresDbContext>(
    options=>options.UseNpgsql(
            builder.Configuration
                .GetConnectionString(
                    "PostgresConnection"),
            x =>
            {
                x.UseNetTopologySuite();

                x.UseVector();
            }));
builder.Services.AddScoped<ICompanyService,CompanyService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<S3Service>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IDocumentChunkRepository, DocumentChunkRepository>();
builder.Services.AddScoped<EmbeddingsService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIAnalysisService,AIAnalysisService>();
builder.Services.AddScoped<IStarterRepository,StarterRepository>();
builder.Services.AddScoped<IGeoService, GeoService>();
builder.Services.AddScoped<IPropertyStarerRepository, PropertyStarterRepository>();
//jwt things start//
var jwtKey =
    builder.Configuration["Jwt:Key"];

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

  
    await StateAndCountySeeder
        .SeedStatesAndCounties(db);
}

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
  
//}

app.UseHttpsRedirection();
app.UseCors("AllowUI");
//jwt start //
app.UseAuthentication();

app.UseAuthorization();
//jwt end //

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

app.MapControllers();

app.Run();
