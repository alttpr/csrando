namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OneOf;

internal record ResourceTypeCount(string Type, int Count);

[JsonConverter(typeof(RequirementConverter))]
public abstract record Requirement
{
    internal record Always : Requirement;
    internal record Never : Requirement;
    internal record Single(string Req) : Requirement;
    internal record And(Requirement[] Reqs) : Requirement;
    internal record Not(Requirement Req) : Requirement;
    internal record Or(Requirement[] Reqs) : Requirement;
    internal record Ammo(string Type, int Count) : Requirement;
    internal record AmmoDrain(string Type, int Count) : Requirement;
    internal record Refill(string[] Resources) : Requirement;
    internal record EnemyKill(
        string[][] Enemies,
        string[]? ExplicitWeapons = null,
        string[]? ExcludedWeapons = null,
        string[]? FarmableAmmo = null
    ) : Requirement
    {
        internal bool CanKillWith(Dictionary<string, int> flags)
        {
            // Take our flags, and reduce them to the ones that are explicitly weapons, and the ones that are not excluded weapons
            var filteredFlags = ExplicitWeapons == null ? flags.AsEnumerable() : flags.Where(flags => ExplicitWeapons.Contains(flags.Key));
            filteredFlags = ExcludedWeapons == null ? filteredFlags : filteredFlags.Where(flags => !ExcludedWeapons.Contains(flags.Key));
            
            // Go through the enemies, and see if we can kill them with the filtered flags
            foreach(var enemy in Enemies.SelectMany(e => e))
            {
                // If we can't kill this enemy, return false

                // TODO: Implement this
                // if (!CanKillEnemy(enemy, filteredFlags))
                //      return false;
            }

            return true;
        }
    };
    internal record AcidFrames(int Frames) : Requirement;
    internal record GravitylessAcidFrames(int Frames) : Requirement;
    internal record DraygonElectricityFrames(int Frames) : Requirement;
    internal record EnemyDamage(string Enemy, string Type, int Hits) : Requirement;
    internal record HeatFrames(int Frames) : Requirement;
    internal record GravitylessHeatFrames(int Frames) : Requirement;
    internal record HibashiHits(int Hits) : Requirement;
    internal record LavaFrames(int Frames) : Requirement;
    internal record GravitylessLavaFrames(int Frames) : Requirement;
    internal record SamusEaterFrames(int Frames) : Requirement;
    internal record MetroidFrames(int Frames) : Requirement;
    internal record EnergyAtMost(int Energy) : Requirement;
    internal record AutoReserveTrigger(int MinReserveEnergy, int MaxReserveEnergy) : Requirement;
    internal record SpikeHits(int Hits) : Requirement;
    internal record ThornHits(int Hits) : Requirement;
    internal record DoorUnlockedAtNode(int Node) : Requirement;
    internal record ObstaclesCleared(string[] Obstacles) : Requirement;
    internal record ObstaclesNotCleared(string[] Obstacles) : Requirement;
    internal record ResourceCapacity(ResourceTypeCount[] Capacity) : Requirement;
    internal record CanShineCharge(
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : Requirement;
    internal record Shinespark(int Frames, int? ExcessFrames = null) : Requirement;
    internal record ResetRoom(
        int[] Nodes,
        int[]? NodesToAvoid = null,
        bool? MustStayPut = null
    ) : Requirement;    
    internal record ItemNotCollectedAtNode(int Node) : Requirement;

    internal bool Check(Dictionary<string, int> flags)
    {
        return this switch
        {
            Always => true,
            Never => false,
            Single req => flags.ContainsKey(req.Req),
            And reqs => reqs.Reqs.All(req => req.Check(flags)),
            Not req => !req.Req.Check(flags),
            Or reqs => reqs.Reqs.Any(req => req.Check(flags)),
            Ammo ammo => flags.TryGetValue(ammo.Type, out var count) && count >= ammo.Count,
            AmmoDrain ammo => true,
            Refill resources => true,
            EnemyKill enemies => enemies.CanKillWith(flags),
            AcidFrames frames => true,
            GravitylessAcidFrames frames => true,
            DraygonElectricityFrames frames => true,
            EnemyDamage enemy => true,
            HeatFrames frames => true,
            GravitylessHeatFrames frames => true,
            HibashiHits hits => true,
            LavaFrames frames => true,
            GravitylessLavaFrames frames => true,
            SamusEaterFrames frames => true,
            MetroidFrames frames => true,
            EnergyAtMost energy => true,
            AutoReserveTrigger minMax => true,
            SpikeHits hits => true,
            ThornHits hits => true,
            DoorUnlockedAtNode node => true,
            ObstaclesCleared obstacles => true,
            ObstaclesNotCleared obstacles => true,
            ResourceCapacity capacity => true,
            CanShineCharge usedTiles => true,
            Shinespark frames => true,
            ResetRoom nodes => true,
            ItemNotCollectedAtNode node => true,
            _ => throw new NotImplementedException()
        };
    }
}

internal class RequirementConverter : JsonConverter<Requirement>
{
    public override Requirement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public Requirement? ParseElement(JsonElement element)
    {
        if(element.ValueKind == JsonValueKind.String)
        {
            return new Requirement.Single(element.GetString()!);
        }
        else if(element.ValueKind == JsonValueKind.Array)
        {
            return new Requirement.And(element.EnumerateArray().Select(req => ParseElement(req)!).ToArray());
        }
        else if(element.ValueKind == JsonValueKind.Object)
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
                "enemyKill" => new Requirement.EnemyKill(
                    property.Value.GetProperty("enemies").EnumerateArray().Select(e => e.EnumerateArray().Select(ee => ee.GetString()!).ToArray()).ToArray(),
                    property.Value.TryGetProperty("explicitWeapons", out var explicitWeapons) ? explicitWeapons.EnumerateArray().Select(e => e.GetString()!).ToArray() : null!,
                    property.Value.TryGetProperty("excludedWeapons", out var excludedWeapons) ? excludedWeapons.EnumerateArray().Select(e => e.GetString()!).ToArray() : null!,
                    property.Value.TryGetProperty("farmableAmmo", out var farmableAmmo) ? farmableAmmo.EnumerateArray().Select(e => e.GetString()!).ToArray() : null!),
                "acidFrames" => new Requirement.AcidFrames(property.Value.GetInt32()),
                "gravitylessAcidFrames" => new Requirement.GravitylessAcidFrames(property.Value.GetInt32()),
                "draygonElectricityFrames" => new Requirement.DraygonElectricityFrames(property.Value.GetInt32()),
                "enemyDamage" => new Requirement.EnemyDamage(property.Value.GetProperty("enemy").GetString()!, property.Value.GetProperty("type").GetString()!, property.Value.GetProperty("hits").GetInt32()),
                "heatFrames" => new Requirement.HeatFrames(property.Value.GetInt32()),
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
                "doorUnlockedAtNode" => new Requirement.DoorUnlockedAtNode(property.Value.GetInt32()),
                "obstaclesCleared" => new Requirement.ObstaclesCleared(property.Value.EnumerateArray().Select(o => o.GetString()!).ToArray()),
                "obstaclesNotCleared" => new Requirement.ObstaclesNotCleared(property.Value.EnumerateArray().Select(o => o.GetString()!).ToArray()),
                "resourceCapacity" => new Requirement.ResourceCapacity(property.Value.EnumerateArray().Select(r => new ResourceTypeCount(r.GetProperty("type").GetString()!, r.GetProperty("count").GetInt32())).ToArray()),
                "canShineCharge" => new Requirement.CanShineCharge(
                    property.Value.GetProperty("usedTiles").GetDecimal(),
                    property.Value.GetProperty("openEnd").GetDecimal(),
                    property.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null!,
                    property.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null!),
                "shinespark" => new Requirement.Shinespark(property.Value.GetProperty("frames").GetInt32(), property.Value.TryGetProperty("excessFrames", out var excessFrames) ? excessFrames.GetInt32() : null!),
                "resetRoom" => new Requirement.ResetRoom(
                    property.Value.GetProperty("nodes").EnumerateArray().Select(n => n.GetInt32()).ToArray(),
                    property.Value.TryGetProperty("nodesToAvoid", out var nodesToAvoid) ? nodesToAvoid.EnumerateArray().Select(n => n.GetInt32()).ToArray() : null!,
                    property.Value.TryGetProperty("mustStayPut", out var mustStayPut) ? mustStayPut.GetBoolean() : null!),
                "itemNotCollectedAtNode" => new Requirement.ItemNotCollectedAtNode(property.Value.GetInt32()),
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


