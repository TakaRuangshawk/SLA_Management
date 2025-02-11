using Microsoft.AspNetCore.Http.Features;
using SLA_Management.Commons.SignalR;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDistributedMemoryCache(); // ใช้ Memory Cache สำหรับ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // อายุ Session 30 นาที
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<FormOptions>(option =>
{
    option.MultipartBodyLengthLimit = 2028 * 1024 * 1024;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession(); // เปิดใช้งาน Session
app.UseRouting();

app.UseAuthorization();
app.MapHub<RPTHub>("/JobRPTHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginMain}/{action=Login}/{id?}");

app.Run();
