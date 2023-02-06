using ApplicationSecurityAssignment.Models;
using ApplicationSecurityAssignment.Services;
using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Spoonful.Services;
using Spoonful.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
	options.Conventions.AllowAnonymousToPage("/Account/ForgetPassword");
	options.Conventions.AllowAnonymousToPage("/Account/ResetPassword");
	options.Conventions.AllowAnonymousToPage("/Account/Login");
	options.Conventions.AllowAnonymousToPage("/Account/Register");
	options.Conventions.AllowAnonymousToPage("/Privacy");
	options.Conventions.AllowAnonymousToPage("/Index");
	options.Conventions.AllowAnonymousToPage("/Error");
	options.Conventions.AllowAnonymousToPage("/ExternalLogin");
	options.Conventions.AllowAnonymousToFolder("/Error");

	options.Conventions.AuthorizePage("/Admin", "RequireAdministratorRole");

});
builder.Services.AddDataProtection();


builder.Services.AddDbContext<AuthDbContext>();

var emailConfig = builder.Configuration
		.GetSection("EmailConfiguration")
		.Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ApplicationUserService>();
builder.Services.AddScoped<AuditService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));
builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequireUppercase = true;
	options.Password.RequiredLength = 12;
	options.Password.RequiredUniqueChars = 1;
	options.User.RequireUniqueEmail = true;

	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 3;
	options.Lockout.AllowedForNewUsers = true;
});

builder.Services.ConfigureApplicationCookie(config =>
{
	config.LoginPath = "/Account/Login";
	config.LogoutPath = "/Account/Logout";
	config.ExpireTimeSpan = TimeSpan.FromMinutes(10);
	config.SlidingExpiration = true;
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromSeconds(10);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:client_id"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:client_secret"];
});

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build();

//    options.AddPolicy("RequireAdministratorRole",
//         policy => policy.RequireRole(Roles.Admin));
//});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithRedirects("/Error/{0}");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();
