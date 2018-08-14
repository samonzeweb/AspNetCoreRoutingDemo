using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreRoutingDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // API v1
            var v1RouteBuilder = new RouteBuilder(app);
            v1RouteBuilder.MapGet("api/v1/foo", async context => {
                await context.Response.WriteAsync("Foo v1");
            });
            v1RouteBuilder.MapGet("api/v1/bar", async context => {
                await context.Response.WriteAsync("Bar v1");
            });            
            app.UseRouter(v1RouteBuilder.Build());

            // API v2 (a bit complicated)
            var v2RouteBuilder = new RouteBuilder(app);
            v2RouteBuilder.MapMiddlewareRoute("api/v2/{*subpart}", nestedApp => {
                nestedApp.Use(async (context, next) => {
                    await next();
                    if (context.Response.StatusCode != StatusCodes.Status404NotFound)
                        await context.Response.WriteAsync(" -- v2 middleware footer");
                });

                var nestedv2RouteBuilder = new RouteBuilder(nestedApp);
                nestedv2RouteBuilder.MapGet("api/v2/foo", async context => {
                    await context.Response.WriteAsync("Foo v2");
                });
                nestedv2RouteBuilder.MapGet("api/v2/bar", async context => {
                    await context.Response.WriteAsync("Bar v2");
                });
                nestedApp.UseRouter(nestedv2RouteBuilder.Build());
            });     
            app.UseRouter(v2RouteBuilder.Build());

            // API v3 (simpler than v2)
            app.Map("/api/v3", nestedApp => {
                nestedApp.Use(async (context, next) => {
                    await next();
                    if (context.Response.StatusCode != StatusCodes.Status404NotFound)
                        await context.Response.WriteAsync(" -- v3 middleware footer");
                });

                var v3RouteBuilder = new RouteBuilder(nestedApp);
                v3RouteBuilder.MapGet("foo", async context => {
                    await context.Response.WriteAsync("Foo v3");
                });
                v3RouteBuilder.MapGet("bar", async context => {
                    await context.Response.WriteAsync("Bar v3");
                });
                nestedApp.UseRouter(v3RouteBuilder.Build());

                // Local fallback (not required)
                // nestedApp.Run(async (context) =>
                // {
                //     context.Response.StatusCode = StatusCodes.Status404NotFound;
                //     await context.Response.WriteAsync("Not Found");
                // });
            });

            // Global fallback (not required)
            // app.Run(async (context) =>
            // {
            //     context.Response.StatusCode = StatusCodes.Status404NotFound;
            //     await context.Response.WriteAsync("Global fallback route !");
            // });
        }
    }
}
