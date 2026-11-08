using RaceDayPOE2.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//Register the RaceDay API service
builder.Services.AddHttpClient<RaceDayApiService>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var apiUrl = configuration["ApiSettings:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiUrl) ||
        !Uri.TryCreate(apiUrl, UriKind.Absolute, out var baseAddress))
    {
        throw new InvalidOperationException(
            "The RaceDay API base URL is missing or invalid.");
    }

    client.BaseAddress = baseAddress;

    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
            "application/json"));
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
