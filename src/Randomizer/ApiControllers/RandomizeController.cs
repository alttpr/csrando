namespace Randomizer.ApiControllers;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;

[ApiController]
[Route("[controller]")]
public sealed class RandomizeController(ILogger<RandomizeController> logger) : ControllerBase
{
    [HttpPost]
    public IResult Post(RandomizeRequest request)
    {
        var seed = request.Seed == 0 ? null : request.Seed;

        if (request.Configs is not [_, ..])
            return Results.Problem("At least one world config is required.");

        logger.LogTrace("Seed {Seed} called with {Settings}", seed, request.Configs);

        var worldConfigs = request.Configs;

        // A pinned seed is deterministic, so retrying it would only repeat the same
        // failure; randomly-seeded requests get a few attempts because some settings
        // (e.g. SM map rando) produce a fraction of unfillable or unwinnable seeds.
        int maxAttempts = seed == null ? 5 : 1;

        try
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    var randomizer = RandomizerFactory.Create(worldConfigs, seed);
                    randomizer.Randomize();
                    if (!randomizer.IsWinnable())
                    {
                        if (attempt == maxAttempts)
                        {
                            logger.LogError("API generated unwinnable game for seed: {Seed}", randomizer.PRNG.Seed);
                            return Results.Problem("Generated game is unwinnable.");
                        }

                        logger.LogWarning("Randomization attempt {Attempt}/{MaxAttempts} generated an unwinnable game for seed {Seed}, retrying", attempt, maxAttempts, randomizer.PRNG.Seed);
                        continue;
                    }

                    // ROM writing is part of generation from the API caller's perspective and
                    // can expose settings-dependent failures too, so keep it inside the retry
                    // boundary rather than returning an error after a successful fill.
                    var loggedRomBroker = new LoggedRomBroker();
                    randomizer.Write(loggedRomBroker);

                    var response = new RandomizeResponse(
                        randomizer.PRNG.Seed,
                        loggedRomBroker.Worlds.ToDictionary(k => k.Key, v => new RandomizerWorldPatches(toBase64(v.Value.BpsPatch), toBase64(v.Value.IpsPatch))),
                        request.IncludeSpoiler ? randomizer.SpoilerLog?.Spoiler : null
                    );

                    logger.LogInformation("API randomization successful for seed: {Seed} on attempt {Attempt}/{MaxAttempts}", response.Seed, attempt, maxAttempts);
                    return Results.Ok(response);
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    logger.LogWarning(ex, "Randomization attempt {Attempt}/{MaxAttempts} failed, retrying", attempt, maxAttempts);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during API randomization for seed: {Seed}", seed);
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
