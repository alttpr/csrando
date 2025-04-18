using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Randomizer.Graph;
using Randomizer.RomModifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

namespace Randomizer.Api;

public static class ApiServer
{
    public static void Start(ILogger logger, FileInfo? baseBPS, Func<WorldConfig[]> getDefaultWorldConfigs)
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IRomFactory, LoggedRomFactory>();

        // Configure JSON options to log deserialization errors
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
            options.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.SerializerOptions.AllowTrailingCommas = true;
            options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapPost("/randomize", (RandomizeRequest request, IRomFactory romFactory) =>
        {
            logger.LogInformation("API /randomize called with seed: {Seed}", request.Seed);

            // Use configs from request if provided, otherwise fallback
            var worldConfigs = (request.Configs != null && request.Configs.Length > 0)
                ? request.Configs
                : getDefaultWorldConfigs();

            var randomizer = RandomizerFactory.Create(
                worldConfigs,
                request.Seed,
                romFactory
            );

            try
            {
                randomizer.Randomize();
                if (!randomizer.IsWinnable())
                {
                    logger.LogError("API generated unwinnable game for seed: {Seed}", request.Seed);
                    return Results.Problem("Generated game is unwinnable.");
                }

                var loggedRom = (LoggedRom)randomizer.Write(null, baseBPS, null)!;

                // Get BPS patch data if available from the loggedRom
                var bpsPatchData = loggedRom.BasePatchData != null ? Convert.ToBase64String(loggedRom.BasePatchData) : null;
                var ipsPatchData = Convert.ToBase64String(loggedRom.GetIpsPatchData());                // Build location->item map for all worlds with game information
                var locationItemMap = new Dictionary<string, object>();
                foreach (var world in randomizer.Worlds)
                {
                    // Try to get the GameId property using reflection since not all world implementations have it
                    foreach (var loc in world.GetLocationsOfType(Randomizer.Graph.VertexType.Item))
                    {
                        if (loc.Item != null)
                        {
                            locationItemMap[$"{loc.Name}"] = new
                            {
                                Item = loc.Item.Name,
                                LocationGameId = loc.World.GameId,
                                LocationWorldId = world.Id,
                                ItemGameId = loc.Item.World.GameId,
                                ItemWorldId = loc.Item.World.Id
                            };
                        }
                    }
                }

                // Optional spoiler playthrough
                object? playthrough = null;
                if (request.IncludeSpoiler && randomizer.SpoilerLog != null && randomizer.SpoilerLog.Spoiler is not null)
                {
                    // Try to extract a playthrough if present
                    if (randomizer.SpoilerLog.Spoiler is IDictionary<string, object> dict && dict.TryGetValue("playthrough", out var pt))
                        playthrough = pt;
                    else if (randomizer.SpoilerLog.Spoiler?.GetType().GetProperty("playthrough") is var prop && prop != null)
                        playthrough = prop.GetValue(randomizer.SpoilerLog.Spoiler);
                }

                var response = new RandomizeResponse(
                    Seed: randomizer.PRNG.Seed.ToString(),
                    BpsPatch: bpsPatchData,
                    IpsPatch: ipsPatchData,
                    Locations: locationItemMap,
                    Playthrough: playthrough
                );

                logger.LogInformation("API randomization successful for seed: {Seed}", response.Seed);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during API randomization for seed: {Seed}", request.Seed);
                return Results.Problem($"An error occurred during randomization: {ex.Message}");
            }
        })
        .WithName("RandomizeGame")
        .WithOpenApi();

        app.Run();
    }

    public record RandomizeRequest(int? Seed, bool IncludeSpoiler, WorldConfig[]? Configs); public record RandomizeResponse(
        string Seed,
        string? BpsPatch,
        string IpsPatch,
        Dictionary<string, object> Locations,
        object? Playthrough
    );
}
