namespace Randomizer.ApiControllers;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Randomizer.Graph;
using Randomizer.RomModifications;

[ApiController]
[Route("[controller]")]
public sealed class RandomizeController(ILogger<RandomizeController> logger) : ControllerBase
{
    [HttpPost]
    public IResult Post(RandomizeRequest request)
    {
        if (request.Configs is not [_, ..])
            return Results.Problem("At least one world config is required.");

        logger.LogTrace("Seed {Seed} called with {Settings}", request.Seed, request.Configs);

        var worldConfigs = request.Configs;

        var randomizer = RandomizerFactory.Create(worldConfigs, request.Seed);

        try
        {
            randomizer.Randomize();
            if (!randomizer.IsWinnable())
            {
                logger.LogError("API generated unwinnable game for seed: {Seed}", request.Seed);
                return Results.Problem("Generated game is unwinnable.");
            }

            var loggedRomBroker = new LoggedRomBroker();
            randomizer.Write(loggedRomBroker);

            var response = new RandomizeResponse(
                randomizer.PRNG.Seed,
                loggedRomBroker.Worlds.ToDictionary(k => k.Key, v => new RandomizerWorldPatches(toBase64(v.Value.BpsPatch), toBase64(v.Value.IpsPatch))),
                request.IncludeSpoiler ? randomizer.SpoilerLog?.Spoiler : null
            );

            logger.LogInformation("API randomization successful for seed: {Seed}", response.Seed);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during API randomization for seed: {Seed}", request.Seed);
            return Results.Problem($"An error occurred during randomization: {ex.Message}");
        }

        [return: NotNullIfNotNull(nameof(patchData))]
        static string? toBase64(byte[]? patchData)
        {
            if (patchData is null)
                return null;

            return Convert.ToBase64String(patchData);
        }
    }
}

public record RandomizeRequest(
    int? Seed,
    bool IncludeSpoiler,
    WorldConfig[] Configs
);
public record RandomizerWorldPatches(string? BpsPatch, string IpsPatch);
public record RandomizeResponse(
    int Seed,
    Dictionary<string, RandomizerWorldPatches> Worlds,
    Dictionary<string, Dictionary<string, string>>? SpoilerLog
);
