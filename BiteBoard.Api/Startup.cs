using Asp.Versioning;
using Finbuckle.MultiTenant;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BiteBoard.API.Filters;
using BiteBoard.API.Helpers;
using BiteBoard.API.Permissions;
using BiteBoard.API.Services;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.API.Settings;
using BiteBoard.API.Swagger;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Interfaces;
using BiteBoard.ResultWrapper;
using System.Reflection;
using System.Text;
using Hangfire.PostgreSql;
using Finbuckle.MultiTenant.Abstractions;

namespace BiteBoard.API;

public class Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        JWTSettings jwtSettings = _configuration.GetSection(nameof(JWTSettings)).Get<JWTSettings>();
		services.AddSingleton(jwtSettings);
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
		services
			.AddDbContext<TenantDbContext>(optionsAction: options => options.UseNpgsql(connectionString))
			.AddMultiTenant<Tenant>()
			.WithHeaderStrategy("X-Tenant")
			.WithEFCoreStore<TenantDbContext, Tenant>()
			.Services
			.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
			{
				var tenantContext = serviceProvider.GetRequiredService<IMultiTenantContextAccessor<Tenant>>();
				var tenantInfo = tenantContext.MultiTenantContext?.TenantInfo;
				//if (tenantInfo == null || string.IsNullOrEmpty(tenantInfo.ConnectionString))
				//{
				//	//options.UseNpgsql(connectionString);
				//	throw new InvalidOperationException("Tenant information is missing or connection string not set.");
				//}
				options.UseNpgsql(tenantInfo.ConnectionString);
			})
			.AddDbContext<IdentityContext>((serviceProvider, options) =>
			{
				var tenantContext = serviceProvider.GetRequiredService<IMultiTenantContextAccessor<Tenant>>();
				var tenantInfo = tenantContext.MultiTenantContext?.TenantInfo;
				//if (tenantInfo == null || string.IsNullOrEmpty(tenantInfo.ConnectionString))
				//{
				//	//options.UseNpgsql(connectionString);
				//	throw new InvalidOperationException("Tenant information is missing or connection string not set.");
				//}
				options.UseNpgsql(tenantInfo.ConnectionString);
			});

		//services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
		//{
		//	options.UseNpgsql(connectionString);
		//});
		//services.AddDbContext<IdentityContext>((serviceProvider, options) =>
		//{
		//	options.UseNpgsql(connectionString);
		//});
		services.AddHangfire(opt => opt.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)).WithJobExpirationTimeout(TimeSpan.FromDays(7)));
        services.AddHangfireServer();
		services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();
        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });
        services.AddTransient<IAuthenticatedUserService, AuthenticatedUserService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
		services.AddFluentValidationAutoValidation();
		services.AddControllers(options =>
		{
			var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
			options.Filters.Add(new AuthorizeFilter(policy));
		})
			.ConfigureApiBehaviorOptions(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var result = Result.Fail("Fix the validation errors.");
					foreach (var (key, value) in context.ModelState)
					{
						foreach (var error in value.Errors)
						{
							result.AddError(key, error.ErrorMessage);
						}
					}
					return new BadRequestObjectResult(result);
				};
			});
		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.ReportApiVersions = true;
			options.ApiVersionReader = ApiVersionReader.Combine(
				new QueryStringApiVersionReader("api-version"),
				new HeaderApiVersionReader("X-Version"));
		});
		var mailSettings = _configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
		services.AddSingleton(mailSettings);
		services.AddFluentEmail(mailSettings.From, mailSettings.DisplayName)
			.AddSmtpSender(mailSettings.Host, mailSettings.Port, mailSettings.UserName, mailSettings.Password);
        services.AddTransient<IMailService, MailService>();
        services.AddTransient<IDateTimeService, SystemDateTimeService>();
        services.AddTransient<IJWTService, JWTService>();
        //services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<TenantInitializer>();

        services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "BiteBoard API",
				Version = "v1",
				Description = "BiteBoard.API",
				License = new OpenApiLicense()
				{
					Name = "BiteBoard",
					Url = new Uri("https://www.BiteBoard.com")
				}
			});
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "JWT Authentication",
				Description = "Input your Bearer token to access this API",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "Bearer",
				BearerFormat = "JWT"
			});
			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Id = "Bearer",
							Type = ReferenceType.SecurityScheme
						}
					},
					Array.Empty<string>()
				}
			});
			options.OperationFilter<DefaultValuesOperationFilter>();
		});
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
        //if (env.IsDevelopment())
        //{
        //	app.UseSwagger();
        //	app.UseSwaggerUI();
        //}
        app.UseSwagger();
        app.UseSwaggerUI();
        //app.UseHttpsRedirection();
        app.UseRouting();
        app.UseMultiTenant();
        app.UseAuthentication();
		app.UseAuthorization();
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });
        app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}
}