using Microsoft.AspNetCore.Http.Features;
using SLA_Management.Commons.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
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

app.UseRouting();

app.UseAuthorization();
app.MapHub<RPTHub>("/JobRPTHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Management}/{action=TicketManagement}/{id?}");

app.Run();
