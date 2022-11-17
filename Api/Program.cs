using Api;
using Api.Configs;
using Api.Services;
using Api.Utils.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var authSection = builder.Configuration.GetSection(AuthConfig.Position);
var authConfig = authSection.Get<AuthConfig>();

builder.Services.Configure<AuthConfig>(authSection);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });

    o.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },
                Scheme = "oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
            },
            new List<string>{}
        }
    });
});

builder.Services.AddDbContext<DAL.DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"), sql => { });
});

builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PostService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = authConfig.Issuer,

        ValidateAudience = true,
        ValidAudience = authConfig.Audience,

        ValidateLifetime = true,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = authConfig.GetSymmetricSecurityKey(),

        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("ValidAccessToken", p =>
    {
        p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        p.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

using (var scope = ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IServiceScopeFactory>()?.CreateScope())
{
    if(scope != null)
    {
        var context = scope.ServiceProvider.GetRequiredService<DAL.DataContext>();
        context.Database.Migrate();
    }
}
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseTokenValidator();

app.MapControllers();

app.Run();
