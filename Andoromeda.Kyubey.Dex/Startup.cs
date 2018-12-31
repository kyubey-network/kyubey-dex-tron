using Andoromeda.Kyubey.Dex.Hubs;
using Andoromeda.Kyubey.Dex.Middlewares;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Andoromeda.Kyubey.Dex
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddConfiguration(out var config, "appsettings");
            services.AddDbContext<KyubeyContext>(x => x.UseMySql(config["MySql"]));
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new Info() { Title = "Kyubey Dex", Version = "v1" });
                x.DocInclusionPredicate((docName, apiDesc) => apiDesc.HttpMethod != null);
                x.DescribeAllEnumsAsStrings();
            });
            services.AddMySqlLogger("kyubey-dex");

            services.AddNodeServices(x =>
                x.ProjectPath = "./Node"
            );
            services.AddEosSignatureValidator();

            services.AddCors(c => c.AddPolicy("Kyubey", x =>
                x.AllowCredentials()
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            ));

            services.AddEosNodeApiInvoker();

            services.AddTimedJob();

            services.AddNewsRepositoryFactory()
                .AddTokenRepositoryactory()
                .AddSlidesRepositoryFactory();

            services.AddAesCrypto();
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration configuration)
        {
            app.UseCors("Kyubey");
            app.UseErrorHandlingMiddleware();
            app.DexStaticFiles(env, configuration);
            
            app.UseSignalR(x =>
            {
                x.MapHub<SimpleWalletHub>("/signalr/simplewallet");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kyubey Dex"));

            app.UseMvcWithDefaultRoute();
            app.UseVueMiddleware();

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<KyubeyContext>().Database.EnsureCreated();
                app.UseTimedJob();
            }
        }
    }
}
