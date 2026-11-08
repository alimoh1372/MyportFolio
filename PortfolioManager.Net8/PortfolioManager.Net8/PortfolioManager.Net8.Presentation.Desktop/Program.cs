using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Photino.NET;
using PM.Net8.Infrastructure.Contexts;
using PM.Net8.Infrastructure.Extensions;
using PM.Net8.Presentation.Api.Extensions;



internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		var baseDirectory = AppContext.BaseDirectory;

		var configuration = new ConfigurationBuilder()
			.SetBasePath(baseDirectory)
			.AddJsonFile("appsettings.json", optional: false)
			.AddEnvironmentVariables()
			.Build();

		var webRootPath = configuration["Web:WebRootPath"];

		if (string.IsNullOrWhiteSpace(webRootPath))
			throw new InvalidOperationException(
				"Web:WebRootPath is not configured.");

		var fullWebRootPath = Path.GetFullPath(webRootPath, baseDirectory);

		if (!Directory.Exists(fullWebRootPath))
			throw new DirectoryNotFoundException(
				$"WebRootPath does not exist: {fullWebRootPath}");

// غیرفعال کردن Static Web Assets
		Environment.SetEnvironmentVariable(
			"ASPNETCORE_STATICWEBASSETS_ENABLED",
			"false");

		var builder = WebApplication.CreateBuilder(
			new WebApplicationOptions
			{
				Args = args,
				ContentRootPath = baseDirectory,
				WebRootPath = fullWebRootPath
			});


		builder.WebHost.UseUrls("http://127.0.0.1:5199");


		builder.Services.AddPortfolioInfrastructure(builder.Configuration);

		builder.AddPortfolioApi();

		builder.Services.AddCors(o =>
			o.AddDefaultPolicy(p => p.WithOrigins("http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod())
		);

		var app = builder.Build();


		Console.WriteLine($"Environment WebRootPath: {app.Environment.WebRootPath}");

		var indexPath = Path.Combine(app.Environment.WebRootPath!, "index.html");

		Console.WriteLine($"Index exists: {File.Exists(indexPath)}");
		Console.WriteLine($"Index path: {indexPath}");

		foreach (var file in Directory.GetFiles(app.Environment.WebRootPath!, "assets"))
		{
			Console.WriteLine(file);
		}


		if (builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
		{
			using var scope = app.Services.CreateScope();
			scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.Migrate();
		}

		app.UseCors();

		#region Mapping the methods api

		
//app.MapGet("/api/portfolio", async (PortfolioService s, CancellationToken ct) =>
//{
//    return Results.Ok(await s.GetSummaryAsync(ct));
//});

//app.MapGet("/api/operations", async (int? take, PortfolioService s, CancellationToken ct) =>
//    Results.Ok(await s.GetOperationsAsync(take ?? 100, ct)));

//app.MapPost("/api/partners", async (CreatePartnerRequest r, PortfolioService s, CancellationToken ct) =>
//{
//    try { return Results.Ok(new { id = await s.CreatePartnerAsync(r.Code, r.FirstName, r.LastName, ct) }); }

//    catch (Exception e) when (e is ArgumentException or InvalidOperationException)
//    { return Results.BadRequest(new { error = e.Message }); }

//});

//app.MapPost("/api/deposits", async (CapitalRequest r, PortfolioService s, CancellationToken ct) =>
//{
//    try
//    {
//        await s.DepositAsync(r.PartnerCode, r.CurrentCashValueRial, r.CurrentStockValueRial, r.AmountRial,
//            r.Note, ct);

//        return Results.Ok();
//    }
//    catch (Exception e) when (e is ArgumentException or InvalidOperationException)
//    {
//        return Results.BadRequest(new { error = e.Message });
//    }
//});

//app.MapPost("/api/withdrawals", async (CapitalRequest r, PortfolioService s, CancellationToken ct) =>
//{
//    try { await s.WithdrawAsync(r.PartnerCode, r.CurrentCashValueRial, r.CurrentStockValueRial, r.AmountRial, r.Note, ct); return Results.Ok(); }
//    catch (Exception e) when (e is ArgumentException or InvalidOperationException)
//    { return Results.BadRequest(new { error = e.Message }); }
//});

//app.MapPut("/api/portfolio/valuation", async (ValuationRequest r, PortfolioService s, CancellationToken ct) =>
//{
//    try
//    {
//        await s.UpdateValuationAsync(r.CashValueRial, r.StockValueRial, r.Note, ct); 

//        return Results.Ok();
//    }
//    catch (Exception e) when (e is ArgumentException or InvalidOperationException)
//    { return Results.BadRequest(new { error = e.Message }); }
//});


		#endregion


		app.UseDefaultFiles();

		app.UseStaticFiles(new StaticFileOptions
		{
			OnPrepareResponse = context =>
			{
				if (context.File.Name.Equals(
					    "index.html",
					    StringComparison.OrdinalIgnoreCase))
				{
					context.Context.Response.Headers.CacheControl =
						"no-cache, no-store, must-revalidate";
				}
			}
		});

		app.MapControllers();

		app.MapFallbackToFile("index.html");

		app.Start();

		var frontendUrl = builder.Configuration["Frontend:DevServerUrl"];

		var url = string.IsNullOrWhiteSpace(frontendUrl) ? "http://127.0.0.1:5199" : frontendUrl;

		#region Test Content

		/*
		  using var client = new HttpClient();
		   
		   var response = client.GetAsync("http://127.0.0.1:5199/").GetAwaiter().GetResult();
		   
		   Console.WriteLine($"HTTP Status: {response.StatusCode}");
		   
		   var html = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
		   
		   Console.WriteLine($"HTML Length: {html.Length}");
		   Console.WriteLine(html);
		 */
		

		#endregion
		

		Console.WriteLine($"Photino URL: {url}");

		var webViewDataPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"PortfolioManager",
			"WebView2-Test");

		Directory.CreateDirectory(webViewDataPath);

		var window = new PhotinoWindow()
				.SetTitle("مدیریت پرتفو")
				.SetUseOsDefaultSize(false)
				.SetSize(new Size(1280, 850))
				.Center()
				.SetResizable(true)
				.SetTemporaryFilesPath(webViewDataPath)
				//.SetContextMenuEnabled(true)
				//.SetBrowserControlInitParameters("--remote-debugging-port=9222")
				//.SetDevToolsEnabled(true)
			;


		//window.WebMessageReceived += (sender, message) => { Console.WriteLine($"WebMessage: {message}"); };

		window.Load(url);

		window.WaitForClose();

		app.StopAsync().GetAwaiter().GetResult();
	}
}
