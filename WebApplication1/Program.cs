using Amazon.SQS;
//jwt things//
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetTopologySuite.IO.Converters;
using Pgvector.EntityFrameworkCore;
using System.Text;
using WebApplication1.Middleware;
//using WebApplication1.Services.Interfaces;
using WebApplication1.Repositories.implementations;
using WebApplication1.Repositories.implementations.DocumentProcessing;
using WebApplication1.Repositories.implementations.PropertyStarer;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Repositories.Interfaces.DocumentProcessing;
using WebApplication1.Seeder;
using WebApplication1.Services.Implementaions;
using WebApplication1.Services.Implementaions.RedisService;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Interfaces;
using WebApplication1.Services.Interfaces.AiServices;
using WebApplication1.Services.Interfaces.RedisService;
using Amazon.Extensions.NETCore.Setup;




var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
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
builder.Services.AddScoped<IVectorSearchInterface,VectorSearchService>();
builder.Services.AddScoped<IAuditLogsService, AuditLogsService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddHostedService<SQSDocumentConsumers>();

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
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:ConnectionString"];
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

//}]

app.UseMiddleware<ExceptionMiddleware>();

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
