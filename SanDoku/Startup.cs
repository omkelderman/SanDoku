using Anemonis.AspNetCore.RequestDecompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using osu.Game.Beatmaps;
using SanDoku.Util;

namespace SanDoku
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
            services.AddRequestDecompression(o =>
            {
                o.SkipUnsupportedEncodings = false;
                o.Providers.Add<BrotliDecompressionProvider>();
                o.Providers.Add<DeflateDecompressionProvider>();
                o.Providers.Add<GzipDecompressionProvider>();
            });
            services.AddControllers(o =>
            {
                o.InputFormatters.Add(new OsuInputFormatter());
            });
            services.AddSwaggerDocument(options =>
            {
                options.TypeMappers.Add(new PrimitiveTypeMapper(typeof(Beatmap), s =>
                {
                    s.Type = JsonObjectType.String;
                    s.Format = "osu-beatmap";
                }));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseRequestDecompression();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}