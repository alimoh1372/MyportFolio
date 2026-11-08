using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PM.Net8.Presentation.Api.Controllers;

namespace PM.Net8.Presentation.Api.Extensions;

public static class DependencyInjection
{
	public static WebApplicationBuilder AddPortfolioApi(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddControllers()
			.AddApplicationPart(typeof(PortfolioController).Assembly);

		return builder;
	}
}