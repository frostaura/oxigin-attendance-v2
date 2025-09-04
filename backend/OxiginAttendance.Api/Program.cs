using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OxiginAttendance.Api.Data;
using OxiginAttendance.Api.Models;
using OxiginAttendance.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-default-secret-key-that-is-at-least-32-characters-long";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IEmployeeRateService, EmployeeRateService>();
builder.Services.AddScoped<IServiceItemService, ServiceItemService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Oxigin Attendance API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        // Seed default roles
        await SeedRolesAsync(roleManager);
        
        // Seed default admin user
        await SeedDefaultAdminAsync(userManager);
        
        // Seed default users for all user types
        await SeedDefaultUsersAsync(userManager);
        
        // Seed service items
        await SeedServiceItemsAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

app.Run();

async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Administrator", "Manager", "Employee" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
{
    var adminEmail = "admin@oxigin.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmployeeId = "ADMIN001",
            Department = "IT",
            JobTitle = "System Administrator",
            HireDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Administrator");
    }
}

async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager)
{
    // Seed Manager user
    var managerEmail = "manager@oxigin.com";
    var managerUser = await userManager.FindByEmailAsync(managerEmail);
    
    if (managerUser == null)
    {
        managerUser = new ApplicationUser
        {
            UserName = managerEmail,
            Email = managerEmail,
            FirstName = "Demo",
            LastName = "Manager",
            EmployeeId = "MGR001",
            Department = "Operations",
            JobTitle = "Department Manager",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(managerUser, "Manager@123");
        await userManager.AddToRoleAsync(managerUser, "Manager");
    }
    
    // Seed Employee user
    var employeeEmail = "employee@oxigin.com";
    var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
    
    if (employeeUser == null)
    {
        employeeUser = new ApplicationUser
        {
            UserName = employeeEmail,
            Email = employeeEmail,
            FirstName = "Demo",
            LastName = "Employee",
            EmployeeId = "EMP001",
            Department = "Operations",
            JobTitle = "Staff Member",
            HireDate = DateTime.UtcNow.AddMonths(-3),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(employeeUser, "Employee@123");
        await userManager.AddToRoleAsync(employeeUser, "Employee");
    }
}

async Task SeedServiceItemsAsync(ApplicationDbContext context)
{
    // Check if service items already exist
    if (context.ServiceItems.Any())
    {
        return; // Already seeded
    }

    var serviceItems = new List<ServiceItem>
    {
        // Shift hours service items (6, 8, 10, 12, 14, 16 hours)
        new ServiceItem { Name = "6 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 240, BaseCost = 180, ShiftHours = 6, IsActive = true },
        new ServiceItem { Name = "8 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 320, BaseCost = 240, ShiftHours = 8, IsActive = true },
        new ServiceItem { Name = "10 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 400, BaseCost = 300, ShiftHours = 10, IsActive = true },
        new ServiceItem { Name = "12 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 480, BaseCost = 360, ShiftHours = 12, IsActive = true },
        new ServiceItem { Name = "14 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 560, BaseCost = 420, ShiftHours = 14, IsActive = true },
        new ServiceItem { Name = "16 Hour Shift", Type = ServiceItemType.ShiftHours, BasePrice = 640, BaseCost = 480, ShiftHours = 16, IsActive = true },
        
        // Permanent staff rates
        new ServiceItem { Name = "Normal Time", Type = ServiceItemType.NormalTime, BasePrice = 40, BaseCost = 30, IsActive = true },
        new ServiceItem { Name = "Overtime", Type = ServiceItemType.Overtime, BasePrice = 60, BaseCost = 45, IsActive = true },
        new ServiceItem { Name = "Double Time", Type = ServiceItemType.DoubleTime, BasePrice = 80, BaseCost = 60, IsActive = true }
    };

    context.ServiceItems.AddRange(serviceItems);
    await context.SaveChangesAsync();
}
