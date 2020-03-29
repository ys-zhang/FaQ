using api.Controllers.AuthUtil;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api
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
            services.AddControllers().AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );
            services.AddDbContext<FaqChatBotDbContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("Debug")));
            services.AddCors(options =>
            {
                options.AddPolicy("Debug",
                    builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
            
            // Jwt
            var jwtSecret = Configuration.GetSection("JwtSettings").GetValue<string>("Secret");
            var jwtExpiration = Configuration.GetSection("JwtSettings").GetValue<int>("Expiration");
            services.AddSingleton<JwtDecoder>(provider => new JwtDecoder(jwtSecret));
            services.AddSingleton<JwtEncoder>(provider => new JwtEncoder(jwtSecret, jwtExpiration));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("Debug");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // app.Use(async (context, next) =>
            // {
            //     context.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            //     await next();
            // });

        }
    }
}
