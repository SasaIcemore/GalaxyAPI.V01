using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeiXin;
using WeiXinAccessToken;

namespace GalaxyAPI.V01
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //配置跨域处理
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
            services.AddSession();
            //添加依赖注入配置
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResourceResources())
                .AddInMemoryApiResources(Config.GetResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetTsetUsers());

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(c =>
                {
                    c.Authority = MyConfig.ConfigManager.TOKEN_URL;
                    c.RequireHttpsMetadata = false;
                    c.ApiName = "api";
                });
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, AccessTokenJob>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Index/Error");
            }
            
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseCors("any");
            app.UseSession();
            app.UseIdentityServer();
            app.UseAuthentication();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Login}/{id?}");

                routes.MapRoute(
                    name: "GalaxyAPI",
                    
                    template: "{area:exists}/{controller=Galaxy}/{action=API}/{apiName?}/{param?}");
            });
        }
    }
}
