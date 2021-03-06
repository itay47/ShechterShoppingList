﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShechterShoppingList.Models;

namespace ShechterShoppingList
{
    public class Startup
    {
        public Startup (IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions> (options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews ().AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
            services.AddRazorPages ();

            Environment.SetEnvironmentVariable ("AWS_ACCESS_KEY_ID", Configuration["AWS:AccessKey"]);
            Environment.SetEnvironmentVariable ("AWS_SECRET_ACCESS_KEY", Configuration["AWS:SecretKey"]);
            Environment.SetEnvironmentVariable ("AWS_REGION", Configuration["AWS:Region"]);

            services.AddDefaultAWSOptions (Configuration.GetAWSOptions ());
            services.AddAWSService<IAmazonDynamoDB> ();
            //services.AddDbContext<>
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseRouting ();
            app.UseCors ();

            app.UseDeveloperExceptionPage ();

            app.UseExceptionHandler ("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts ();

            app.UseHttpsRedirection ();
            app.UseStaticFiles ();
            //app.UseCookiePolicy ();

            app.UseEndpoints (endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapControllerRoute (
                    name:"default",
                    pattern:"{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapRazorPages();
                //endpoints.MapDefaultControllerRoute();
            });

            /* app.UseMvc (routes =>
            {
                routes.MapRoute (
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            }); */
        }
    }
}