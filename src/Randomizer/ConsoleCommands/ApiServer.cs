namespace Randomizer.ConsoleCommands;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

internal sealed class ApiServer : Command
{
    private readonly Option<bool> _enableOpenApi = new("--enable-open-api", "Enable OpenAPI document hosting");
    private readonly Argument<string[]> _others = new();

    public ApiServer()
        : base("api", "Run an API server for web use.")
    {
        Add(_enableOpenApi);
        Add(_others);

        this.SetHandler(context => context.ExitCode = Handle(context));
    }
    public int Handle(InvocationContext context)
    {
        var builder = WebApplication.CreateBuilder(context.ParseResult.GetValueForArgument(_others));
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddLogging();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        });

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.AllowTrailingCommas = true;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        var app = builder.Build();
        app.MapControllers();
        if (context.ParseResult.GetValueForOption(_enableOpenApi))
            app.MapOpenApi();

        app.Run();
        return 0;
    }
}
