namespace Randomizer.Games.SuperMetroid.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Randomizer.Graph;

public record ResourceTypeCount(string Type, int Count);
public record Drop(string Enemy, int Count);

[JsonConverter(typeof(RequirementConverter))]
public abstract record Requirement
{
    public record Always : Requirement;
    public record Never : Requirement;
    public record Single(string Req) : Requirement;
    public record SingleItem(IItem Item) : Requirement;
    public record And(Requirement[] Reqs) : Requirement;
    public record Not(Requirement Req) : Requirement;
    public record Or(Requirement[] Reqs) : Requirement;
    public record Ammo(string Type, int Count) : Requirement;
    public record AmmoDrain(string Type, int Count) : Requirement;
    public record Refill(string[] Resources) : Requirement;
    public record PartialRefill(string Resources, int Limit) : Requirement;
    public record EnemyKill(
        string[][] Enemies,
        HashSet<string>? ExplicitWeapons = null,
        HashSet<string>? ExcludedWeapons = null,
        string[]? FarmableAmmo = null
    ) : Requirement;
    public record AcidFrames(int Frames) : Requirement;
    public record GravitylessAcidFrames(int Frames) : Requirement;
    public record DraygonElectricityFrames(int Frames) : Requirement;
    public record EnemyDamage(string Enemy, string Type, int Hits) : Requirement;
    public record HeatFrames(int Frames) : Requirement;
    public record HeatFramesWithEnergyDrops(int Frames, Drop[] Drops) : Requirement;
    public record GravitylessHeatFrames(int Frames) : Requirement;
    public record HibashiHits(int Hits) : Requirement;
    public record LavaFrames(int Frames) : Requirement;
    public record GravitylessLavaFrames(int Frames) : Requirement;
    public record SamusEaterFrames(int Frames) : Requirement;
    public record MetroidFrames(int Frames) : Requirement;
    public record EnergyAtMost(int Energy) : Requirement;
    public record AutoReserveTrigger(int MinReserveEnergy, int MaxReserveEnergy) : Requirement;
    public record SpikeHits(int Hits) : Requirement;
    public record ThornHits(int Hits) : Requirement;
    public record ElectricityHits(int Hits) : Requirement;
    public record DoorUnlockedAtNode(int Node) : Requirement;
    public record ObstaclesCleared(string[] Obstacles) : Requirement;
    public record ObstaclesNotCleared(string[] Obstacles) : Requirement;
    public record ResourceCapacity(ResourceTypeCount[] Capacity) : Requirement;
    public record ResourceAvailable(ResourceTypeCount[] Available) : Requirement;
    public record ResourceMissingAtMost(ResourceTypeCount[] Missing) : Requirement;
    public record ResourceAtMost(ResourceTypeCount[] Resources) : Requirement;
    public record CanShineCharge(
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : Requirement;
    public record GetBlueSpeed(
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : Requirement;
    public record SpeedBall(
        decimal Length,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : Requirement;
    public record Shinespark(int Frames, int? ExcessFrames = null) : Requirement;
    public record ShineChargeFrames(int Frames) : Requirement;
    public record ResetRoom(
        int[] Nodes,
        int[]? NodesToAvoid = null,
        bool? MustStayPut = null
    ) : Requirement;
    public record ItemNotCollectedAtNode(int Node) : Requirement;
    public record GainFlashSuit() : Requirement;
    public record UseFlashSuit() : Requirement;
    public record NoFlashSuit() : Requirement;
    public record Tech(string TechRequirement) : Requirement;
    public record Notable(string NotableName) : Requirement;
    public record DisableEquipment(string Equipment) : Requirement;
    public record ResourceConsumed(ResourceTypeCount[] Resources) : Requirement;
    public record CycleFrames(int cycleFrames) : Requirement;

    public Requirement ModifyObstacleState(string[] obstaclesCleared)
    {
        return this switch
        {
            // Check if all obstacles is in the obstaclesCleared list, in that case return Always, otherwise Never
            ObstaclesCleared obstacles => obstacles.Obstacles.All(obstacle => obstaclesCleared.Contains(obstacle)) ? new Always() : new Never(),
            And reqs => new And(reqs.Reqs.Select(req => req.ModifyObstacleState(obstaclesCleared)).ToArray()),
            Or reqs => new Or(reqs.Reqs.Select(req => req.ModifyObstacleState(obstaclesCleared)).ToArray()),
            _ => this
        };
    }


}

public class RequirementConverter : JsonConverter<Requirement>
{
    public override Requirement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public Requirement? ParseElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return new Requirement.Single(element.GetString()!);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            return new Requirement.And(element.EnumerateArray().Select(req => ParseElement(req)!).ToArray());
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            // Get the first property of the object
            var property = element.EnumerateObject().First();
            return property.Name switch
            {
                "always" => new Requirement.Always(),
                "never" => new Requirement.Never(),
                "and" => new Requirement.And(property.Value.EnumerateArray().Select(req => ParseElement(req)!).ToArray()),
                "not" => new Requirement.Not(ParseElement(property.Value)!),
                "or" => new Requirement.Or(property.Value.EnumerateArray().Select(req => ParseElement(req)!).ToArray()),
                "ammo" => new Requirement.Ammo(property.Value.GetProperty("type").GetString()!, property.Value.GetProperty("count").GetInt32()),
                "ammoDrain" => new Requirement.AmmoDrain(property.Value.GetProperty("type").GetString()!, property.Value.GetProperty("count").GetInt32()),
                "refill" => new Requirement.Refill(property.Value.EnumerateArray().Select(r => r.GetString()!).ToArray()),
                "partialRefill" => new Requirement.PartialRefill(property.Value.GetProperty("type").GetString()!, property.Value.GetProperty("limit").GetInt32()),
                "enemyKill" => new Requirement.EnemyKill(
                    property.Value.GetProperty("enemies").EnumerateArray().Select(e => e.EnumerateArray().Select(ee => ee.GetString()!).ToArray()).ToArray(),
                    property.Value.TryGetProperty("explicitWeapons", out var explicitWeapons) ? explicitWeapons.EnumerateArray().Select(e => e.GetString()!).ToHashSet() : null!,
                    property.Value.TryGetProperty("excludedWeapons", out var excludedWeapons) ? excludedWeapons.EnumerateArray().Select(e => e.GetString()!).ToHashSet() : null!,
                    property.Value.TryGetProperty("farmableAmmo", out var farmableAmmo) ? farmableAmmo.EnumerateArray().Select(e => e.GetString()!).ToArray() : null!),
                "acidFrames" => new Requirement.AcidFrames(property.Value.GetInt32()),
                "gravitylessAcidFrames" => new Requirement.GravitylessAcidFrames(property.Value.GetInt32()),
                "draygonElectricityFrames" => new Requirement.DraygonElectricityFrames(property.Value.GetInt32()),
                "enemyDamage" => new Requirement.EnemyDamage(property.Value.GetProperty("enemy").GetString()!, property.Value.GetProperty("type").GetString()!, property.Value.GetProperty("hits").GetInt32()),
                "heatFrames" => new Requirement.HeatFrames(property.Value.GetInt32()),
                "heatFramesWithEnergyDrops" => new Requirement.HeatFramesWithEnergyDrops(
                    property.Value.GetProperty("frames").GetInt32(),
                    property.Value.GetProperty("drops").EnumerateArray().Select(d => new Drop(d.GetProperty("enemy").GetString()!, d.GetProperty("count").GetInt32())).ToArray()),
                "gravitylessHeatFrames" => new Requirement.GravitylessHeatFrames(property.Value.GetInt32()),
                "hibashiHits" => new Requirement.HibashiHits(property.Value.GetInt32()),
                "lavaFrames" => new Requirement.LavaFrames(property.Value.GetInt32()),
                "gravitylessLavaFrames" => new Requirement.GravitylessLavaFrames(property.Value.GetInt32()),
                "samusEaterFrames" => new Requirement.SamusEaterFrames(property.Value.GetInt32()),
                "metroidFrames" => new Requirement.MetroidFrames(property.Value.GetInt32()),
                "energyAtMost" => new Requirement.EnergyAtMost(property.Value.GetInt32()),
                "autoReserveTrigger" => new Requirement.AutoReserveTrigger(
                    property.Value.TryGetProperty("minReserveEnergy", out var minReserveEnergy) ? minReserveEnergy.GetInt32() : 1,
                    property.Value.TryGetProperty("maxReserveEnergy", out var maxReserveEnergy) ? maxReserveEnergy.GetInt32() : 400
                ),
                "spikeHits" => new Requirement.SpikeHits(property.Value.GetInt32()),
                "thornHits" => new Requirement.ThornHits(property.Value.GetInt32()),
                "electricityHits" => new Requirement.ElectricityHits(property.Value.GetInt32()),
                "doorUnlockedAtNode" => new Requirement.DoorUnlockedAtNode(property.Value.GetInt32()),
                "obstaclesCleared" => new Requirement.ObstaclesCleared(property.Value.EnumerateArray().Select(o => o.GetString()!).ToArray()),
                "obstaclesNotCleared" => new Requirement.ObstaclesNotCleared(property.Value.EnumerateArray().Select(o => o.GetString()!).ToArray()),
                "resourceCapacity" => new Requirement.ResourceCapacity(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "resourceAvailable" => new Requirement.ResourceAvailable(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "resourceMissingAtMost" => new Requirement.ResourceMissingAtMost(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "resourceAtMost" => new Requirement.ResourceAtMost(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "canShineCharge" => new Requirement.CanShineCharge(
                    property.Value.GetProperty("usedTiles").GetDecimal(),
                    property.Value.GetProperty("openEnd").GetDecimal(),
                    property.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null!),
                "getBlueSpeed" => new Requirement.GetBlueSpeed(
                    property.Value.GetProperty("usedTiles").GetDecimal(),
                    property.Value.GetProperty("openEnd").GetDecimal(),
                    property.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null!),
                "speedBall" => new Requirement.SpeedBall(
                    property.Value.GetProperty("length").GetDecimal(),
                    property.Value.GetProperty("openEnd").GetDecimal(),
                    property.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null!),
                "shinespark" => new Requirement.Shinespark(property.Value.GetProperty("frames").GetInt32(), property.Value.TryGetProperty("excessFrames", out var excessFrames) ? excessFrames.GetInt32() : null!),
                "shineChargeFrames" => new Requirement.ShineChargeFrames(property.Value.GetInt32()),
                "resetRoom" => new Requirement.ResetRoom(
                    property.Value.GetProperty("nodes").EnumerateArray().Select(n => n.GetInt32()).ToArray(),
                    property.Value.TryGetProperty("nodesToAvoid", out var nodesToAvoid) ? nodesToAvoid.EnumerateArray().Select(n => n.GetInt32()).ToArray() : null!,
                    property.Value.TryGetProperty("mustStayPut", out var mustStayPut) ? mustStayPut.GetBoolean() : null!),
                "itemNotCollectedAtNode" => new Requirement.ItemNotCollectedAtNode(property.Value.GetInt32()),
                "gainFlashSuit" => new Requirement.GainFlashSuit(),
                "useFlashSuit" => new Requirement.UseFlashSuit(),
                "noFlashSuit" => new Requirement.NoFlashSuit(),
                "tech" => new Requirement.Tech(property.Value.GetString()!),
                "notable" => new Requirement.Notable(property.Value.GetString()!),
                "disableEquipment" => new Requirement.DisableEquipment(property.Value.GetString()!),
                "resourceConsumed" => new Requirement.ResourceConsumed(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "cycleFrames" => new Requirement.CycleFrames(property.Value.GetInt32()),

                _ => throw new Exception($"Unknown requirement: {property.Name}")
            };
        }
        else
        {
            throw new Exception($"Unknown requirement object type: {element.ValueKind}");
        }
    }

    public override void Write(Utf8JsonWriter writer, Requirement value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}


