using Immense.RemoteControl.Server.Abstractions;
using Immense.RemoteControl.Server.Extensions;
using ServerExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRemoteControlServer(config =>
{
    config.AddHubEventHandler<HubEventHandler>();
    config.AddViewerAuthorizer<ViewerAuthorizer>();
    config.AddViewerHubDataProvider<ViewerHubDataProvider>();
    config.AddViewerPageDataProvider<ViewerPageDataProvider>();
});

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

app.UseRouting();


app.UseAuthorization();

app.UseRemoteControlServer();

//app.MapRazorPages();

app.Run();
