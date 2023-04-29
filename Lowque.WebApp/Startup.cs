using Lowque.BusinessLogic;
using Lowque.BusinessLogic.Services;
using Lowque.BusinessLogic.Services.Interfaces;
using Lowque.BusinessLogic.SolutionGeneration;
using Lowque.BusinessLogic.SolutionCompilation;
using Lowque.BusinessLogic.Types;
using Lowque.DataAccess;
using Lowque.DataAccess.Entities.Identity;
using Lowque.DataAccess.Internationalization;
using Lowque.DataAccess.Internationalization.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Lowque.DataAccess.Identity;
using Lowque.DataAccess.SolutionCompilation;

namespace Lowque.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true,
                      ValidateLifetime = true,
                      ValidateIssuerSigningKey = true,
                      ValidIssuer = Configuration["Jwt:Issuer"],
                      ValidAudience = Configuration["Jwt:Issuer"],
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                  };
              }
            );
            services.AddAuthorization();
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddHttpContextAccessor();
            services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddTransient<IJwtGenerator, JwtGenerator>();
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<ILocalizationContext, LocalizationContext>();
            services.AddTransient<ISystemTypesContext, SystemTypesContext>();
            services.AddTransient<ISolutionGenerator, SolutionGenerator>();
            services.AddTransient<ISolutionCompilator, SolutionCompilator>();
            services.AddTransient<ICompilationFilesCleaner, CompilationFilesCleaner>();
            services.AddTransient<ICompilationErrorsFormatter, CompilationErrorsFormatter>();
            services.AddTransient<ITypeSpecificationFormatter, TypeSpecificationFormatter>();

            services.AddTransient<IIdentityService, IdentityService>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IFlowDesignerService, FlowDesignerService>();
            services.AddTransient<IApplicationDefinitionService, ApplicationDefinitionService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext dbContext)
        {
            dbContext.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
