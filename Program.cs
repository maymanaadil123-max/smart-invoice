using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // ✅ إضافة Identity
using SmartInvoice.Data;
using SmartInvoice.Services;
using Hangfire;
using Hangfire.Storage.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// ✅ تسجيل DbContext مع SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ إضافة Identity لتسجيل الدخول
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ✅ إعداد مسار تسجيل الدخول ورفض الوصول
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";         // صفحة تسجيل الدخول
    options.AccessDeniedPath = "/Account/AccessDenied"; // صفحة رفض الوصول
});

// ✅ تسجيل EmailService
builder.Services.AddScoped<EmailService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
// ✅ تسجيل InvoiceReminderService
builder.Services.AddScoped<InvoiceReminderService>();

// ✅ إعداد Hangfire مع SQLite
builder.Services.AddHangfire(x =>
    x.UseSQLiteStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ إضافة Authentication قبل Authorization
app.UseAuthentication();
app.UseAuthorization();

// ✅ Hangfire Dashboard
app.UseHangfireDashboard();

// ✅ Recurring Job لتذكيرات الفواتير
RecurringJob.AddOrUpdate<InvoiceReminderService>(
    "send-invoice-reminders",
    service => service.SendRemindersAsync(),
    Cron.Daily); // يومياً

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();