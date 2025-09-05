using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RealEstate.Application.Commands;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;
using RealEstate.Infrastructure.Repositories;
using RealEstate.Infrastructure.Services;
using RealState_Million.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var cs = builder.Configuration.GetConnectionString("RealEstateDb");
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(cs));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.Configure<FileStorageOptions>(opt => {
    opt.WebRootPath = builder.Environment.WebRootPath;
    opt.BaseRelativePath = "uploads/properties";
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme) 
    .AddJwtBearer(o => {
        var jwt = builder.Configuration.GetSection("Jwt");
        var raw = jwt["Key"]!;

        byte[] keyBytes = raw.StartsWith("base64:", StringComparison.OrdinalIgnoreCase)
            ? Convert.FromBase64String(raw["base64:".Length..])
            : Encoding.UTF8.GetBytes(raw);

        o.TokenValidationParameters = new TokenValidationParameters {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("properties:read", p => p.RequireClaim("perm", "properties:read"));
    options.AddPolicy("properties:write", p => p.RequireClaim("perm", "properties:write"));
    options.AddPolicy("properties:price", p => p.RequireClaim("perm", "properties:price"));
    options.AddPolicy("properties:trace", p => p.RequireClaim("perm", "properties:trace"));
});

builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealEstate API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
      {
        new OpenApiSecurityScheme { Reference = new OpenApiReference {
            Type = ReferenceType.SecurityScheme, Id = "Bearer" }}, new string[] { }
      }
    });
});

builder.Services.AddAutoMapper(typeof(CreatePropertyHandler).Assembly);
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<CreatePropertyHandler>();
    cfg.RegisterServicesFromAssemblyContaining<UploadPropertyImagesHandler>();
});
builder.Services.AddAutoMapper(typeof(CreatePropertyHandler).Assembly);
builder.Services.AddScoped<IFileStorageService, FileSystemStorageService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.EnableAnnotations();
});

var app = builder.Build();
app.UseApiExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
