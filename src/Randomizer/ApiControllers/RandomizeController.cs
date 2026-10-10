namespace Randomizer.ApiControllers;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Randomizer.Games;
using Randomizer.Graph;
using Randomizer.RomModifications;

[ApiController]
[Route("[controller]")]
public sealed class RandomizeController(
    ILogger<RandomizeController> logger,
    GenerationLimiter generationLimiter) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Post(
        RandomizeRequest request,
        CancellationToken requestAborted)
    {
        var seed = request.Seed == 0 ? null : request.Seed;

        if (request.Configs is not [_, ..])
            return Results.Problem("At least one world config is required.");

        // Deterministic config errors are rejected before the retry loop: they would
        // fail identically on every attempt and must not burn generation retries.
        if (WorldConfigValidator.Validate(request.Configs) is { } configError)
            return Results.Problem(configError, statusCode: StatusCodes.Status400BadRequest);

        logger.LogTrace("Seed {Seed} called with {Settings}", seed, request.Configs);

        var worldConfigs = request.Configs;
        int? activeSeed = seed;
        int currentAttempt = 0;
        string stage = "waiting for a generation slot";
        var stopwatch = Stopwatch.StartNew();

        // If the start location is moved, allow for more retries
        int maxAttempts = seed != null ? 1 : RequestsMovedStart(worldConfigs) ? 10 : 5;

        using var timeoutSource = new CancellationTokenSource(generationLimiter.Timeout);
        using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(
            requestAborted, timeoutSource.Token);
        IDisposable? generationLease = null;

        try
        {
            generationLease = await generationLimiter.EnterAsync(cancellationSource.Token);
            using (generationLease)
            using (GenerationContext.Begin(cancellationSource.Token))
            {
                for (int attempt = 1; ; attempt++)
                {
                    currentAttempt = attempt;
                    try
                    {
                        stage = "creating worlds";
                        var randomizer = RandomizerFactory.Create(worldConfigs, seed);
                        activeSeed = randomizer.PRNG.Seed;
                        stage = "placing items";
                        randomizer.Randomize();
                        stage = "validating the game";
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
                        stage = "writing patches";
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
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex) when (attempt < maxAttempts)
                    {
                        logger.LogWarning(ex, "Randomization attempt {Attempt}/{MaxAttempts} failed, retrying", attempt, maxAttempts);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (requestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "API randomization canceled by the caller for seed: {Seed}", activeSeed);
            return Results.StatusCode(499);
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            if (generationLease is null)
            {
                logger.LogWarning(
                    "API randomization timed out waiting for a generation slot for seed: {Seed}",
                    activeSeed);
                return Results.Problem(
                    "The generator is busy. Please try again.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            logger.LogError(
                "API randomization exceeded the {Timeout} generation limit after {Elapsed} "
                + "during {Stage} (attempt {Attempt}) for seed {Seed} with settings {Settings}",
                generationLimiter.Timeout, stopwatch.Elapsed, stage, currentAttempt,
                activeSeed, worldConfigs);
            return Results.Problem(
                $"Generation exceeded the {generationLimiter.Timeout.TotalSeconds:0}-second limit.",
                statusCode: StatusCodes.Status504GatewayTimeout);
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

    private static bool RequestsMovedStart(WorldConfig[] configs) => configs.Any(config =>
        config.SuperMetroid?.StartLocationRequested == true
        || config.Metroid?.StartAreaRequested == true);
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
