using NJsonSchema;
using NJsonSchema.Generation.TypeMappers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using SanDoku.Util;

// make very old beatmap files work
LegacyDifficultyCalculatorBeatmapDecoder.Register();

// explicitly set the RulesetStore so we don't get a warning later on about it
Decoder.RegisterDependencies(new CustomRulesetStore(RulesetUtil.GetAllAvailableRulesetInfos()));

// setup web app
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks();
builder.Services.AddRequestDecompression();
builder.Services.AddControllers(o =>
{
    o.InputFormatters.Add(new OsuInputFormatter());
    o.AllowEmptyInputInBodyModelBinding = true;
});
builder.Services.AddSwaggerDocument(options =>
{
    options.SchemaSettings.TypeMappers.Add(new PrimitiveTypeMapper(typeof(Beatmap), s =>
    {
        s.Type = JsonObjectType.String;
        s.Format = "osu-beatmap";
    }));
    options.Version = "v1";
    options.Title = nameof(SanDoku);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseOpenApi();
app.UseSwaggerUi();

app.UseRouting();

app.UseAuthorization();

app.UseRequestDecompression();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
