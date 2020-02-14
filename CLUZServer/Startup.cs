using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CLUZServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using CLUZServer.Services;
using CLUZServer.Helpers;

namespace CLUZServer
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
            services.AddControllersWithViews();

            //services.AddControllers()
            //    .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);

            services.AddSignalR(hubOptions =>
                        {
                                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
                                hubOptions.KeepAliveInterval = TimeSpan.FromMinutes(15);
                            });
            //   .AddMessagePackProtocol();

            services.AddSingleton<GamePool>();

            services.AddSingleton<PlayerPool>();

            services.AddSingleton<Results>();

            services.AddSingleton<AllPlayersReady>();

            services.AddHostedService<Scavenger>();

            services.AddHostedService<Broadcaster>();

            services.AddHostedService<DayIncrementer>();


        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<PlayersHub>("/PlayersHub");
            });
        }
    }
}
