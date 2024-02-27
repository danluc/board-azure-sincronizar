using APP.Configuracoes;
using Back.Data.Context;
using Back.Servico.Hubs.Notificacoes;
using ElectronNET.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace APP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            services.AddServicosConfig(Configuration);
            services.AddRepositoryConfig();

            services.AddControllers();

            //Roda a migration
            using (var serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<BancoDBContext>();
                context.Database.Migrate();
            }
           
            services.AddSignalR();

            //services.AddMvc(options => options.EnableEndpointRouting = false);
            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.AddServicosJob();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        private async void ElectronStatup()
        {
            var window = await Electron.WindowManager.CreateWindowAsync();

            window.OnClosed += () => {
                Electron.App.Quit();
            };
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (HybridSupport.IsElectronActive)
            {
                ElectronStatup();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SINCRONIZAR BOARD");
                c.RoutePrefix = "swagger";
            });

            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseSpaStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/api/hubs/notification");
            });

            app.UseSignalR(configure =>
            {
                configure.MapHub<NotificationHub>("/api/hubs/notification");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.EnvironmentName.Contains("Development"))
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
