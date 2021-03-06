using System;
using System.Net.Http.Headers;
using System.Text;
using API.Middleware;
using Application.Interfaces;
using Application.Interfaces.Social;
using Application.User;
using Data;
using Data.Models;
using FluentValidation.AspNetCore;
using Infrastructure.Security;
using Infrastructure.Security.Social;
using Infrastructure.User;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            // DB Setup
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
            // DB Setup
            services.AddDbContext<DataContext>(opt =>
            {
                opt.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            });

            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {

            // CORS
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("WWW-Authenticate")
                        .WithOrigins(Configuration["ActivitiesApp"]).AllowCredentials();
                });
            });

            //MEDIATR
            services.AddMediatR(typeof(Register.Handler).Assembly);
            services.AddControllers()
            .AddFluentValidation(cfg =>
            {
                cfg.RegisterValidatorsFromAssemblyContaining<Register>();
            });

            // IDENTITY SETUP
            services.AddDefaultIdentity<AppUser>()
                .AddEntityFrameworkStores<DataContext>();

            //Authentication
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });


            services.AddHttpClient("activities", c =>
             {
                 c.BaseAddress = new Uri(Configuration["GatewayAPIBaseURL"]);
             });

            //DI
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();
            services.AddScoped<IFacebookAccessor, FacebookAccessor>();
            services.AddScoped<IUserActivitiesApp, UserActivitiesApp>();

            //Configs
            services.Configure<FacebookAppSettings>(Configuration.GetSection("Authentication:Facebook"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMiddleware<ErrorHandlerMiddleware>();
            //app.UseHttpsRedirection();
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opt => opt.NoReferrer());
            app.UseXXssProtection(opt => opt.EnabledWithBlockMode());
            app.UseXfo(opt => opt.Deny());
            app.UseCspReportOnly(opt => opt
               .BlockAllMixedContent()
               .StyleSources(s => s.Self())
               .FontSources(s => s.Self())
               .FormActions(s => s.Self())
               .FrameAncestors(s => s.Self())
               .ImageSources(s => s.Self())
               .ScriptSources(s => s.Self())
                );

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("SUPER HERO AUTH API IS RUNNING! MYSQL!");
                });
            });
        }
    }
}
