#region Using Statements

using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ShepherdsInn.API.Data;
using ShepherdsInn.API.Configuration;
using ShepherdsInn.API.Services;

#endregion

// Create the builder
var builder = WebApplication.CreateBuilder(args);

// Create the API options
var appOptions = ShepherdsInnApiOptions.Create(builder.Environment);
builder.Services.AddSingleton(appOptions);
builder.Services.AddSingleton(appOptions.ContactForm);
builder.Services.AddSingleton(appOptions.Database);
builder.Services.AddSingleton(appOptions.EmailNotifications);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add contact notifcation service implementation
//builder.Services.AddScoped<IContactNotificationService, SmtpContactNotificationService>();
builder.Services.AddScoped<IContactNotificationService, NullContactNotificationService>();

// Configure CORS policy for the frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy(ApiPolicyNames.ShepherdsInnFrontendCors, policy =>
    {
        policy
            .WithOrigins(appOptions.ContactForm.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure rate limiting for the contact form
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(ApiPolicyNames.ContactFormRateLimit, httpContext =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = appOptions.ContactForm.RateLimit.PermitLimit,
                Window = TimeSpan.FromMinutes(appOptions.ContactForm.RateLimit.WindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
});

// Configure the database path and ensure the directory exists
var dbPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, appOptions.Database.RelativePath));
var dbFolder = Path.GetDirectoryName(dbPath);
if (!string.IsNullOrWhiteSpace(dbFolder))
{
    Directory.CreateDirectory(dbFolder);
}
builder.Services.AddDbContext<ShepherdsInnDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShepherdsInnDbContext>();
    db.Database.Migrate();
}

// Configure middleware
app.UseHttpsRedirection();
app.UseCors(ApiPolicyNames.ShepherdsInnFrontendCors);
app.UseRateLimiter();

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    app = "Shepherds Inn API",
    timestampUtc = DateTimeOffset.UtcNow
}));

// Map controller routes
app.MapControllers();

// Run the app
app.Run();
