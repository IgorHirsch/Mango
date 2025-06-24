using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ICouponService, CouponService>();



// Retrieve the base URL for the Coupon API from configuration
var couponApiBase = builder.Configuration["ServiceURLs:CouponAPI"];
if (string.IsNullOrEmpty(couponApiBase))
{
    throw new InvalidOperationException("Die Konfiguration f�r 'ServiceURLs:CouponAPI' ist nicht vorhanden oder leer.");
}

builder.Services.AddHttpClient("MangoAPI", client =>
{
    client.BaseAddress = new Uri(couponApiBase);
});

SD.CouponAPIBase = couponApiBase;





builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<ICouponService, CouponService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
