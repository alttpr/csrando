namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.ApiControllers;

internal sealed class ApiServer : Command
{
    private readonly Option<bool> _enableOpenApi = new("--enable-open-api") { Description = "Enable OpenAPI document hosting" };
    private readonly Argument<string[]> _others = new("api-parameters") { DefaultValueFactory = _ => [] };

    public ApiServer()
        : base("api", "Run an API server for web use.")
    {
        Add(_enableOpenApi);
        Add(_others);

        SetAction(Handle);
    }
    public int Handle(ParseResult parseResult)
    {
        var builder = WebApplication.CreateBuilder(parseResult.GetValue(_others)!);
        bool enableOpenApi = parseResult.GetValue(_enableOpenApi);
        if (enableOpenApi)
            builder.Services.AddOpenApi();
        builder.Services.AddLogging();

        var generationLimits = new GenerationLimitsOptions();
        builder.Configuration
            .GetSection(GenerationLimitsOptions.SectionName)
            .Bind(generationLimits);
        builder.Services.AddSingleton(generationLimits);
        builder.Services.AddSingleton<GenerationLimiter>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        });

        builder.Services.ConfigureHttpJsonOptions(options => configureJsonOptions(options.SerializerOptions));
        builder.Services.AddControllers()
            .AddJsonOptions(options => configureJsonOptions(options.JsonSerializerOptions));

        var app = builder.Build();
        app.UseCors();
        app.MapControllers();
        if (enableOpenApi)
            app.MapOpenApi();

        app.Run();
        return 0;

        static void configureJsonOptions(JsonSerializerOptions serializerOptions)
        {
            serializerOptions.PropertyNameCaseInsensitive = true;
            serializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            serializerOptions.AllowTrailingCommas = true;
            serializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            serializerOptions.Converters.Add(new JsonStringEnumConverter());
            serializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }
    }
}
