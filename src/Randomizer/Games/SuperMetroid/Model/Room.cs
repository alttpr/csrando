namespace Randomizer.Games.SuperMetroid.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Room
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Area { get; set; }
    public required string SubArea { get; set; }
    public string? SubSubArea { get; set; }
    public bool Playable { get; set; }
    public Note? Note { get; set; }
    public Note? DevNote { get; set; }
    public string? RoomAddress { get; set; }
    public RoomEnvironment[]? RoomEnvironments { get; set; }
    public required Node[] Nodes { get; set; }
    public required Link[] Links { get; set; }
    public required Strat[] Strats { get; set; }
    public Obstacle[]? Obstacles { get; set; }
    public RoomEnemy[]? Enemies { get; set; }
    public ReusableNotable[]? ReusableRoomwideNotable { get; set; }
}

public record ReusableNotable(string Name, Note? Note, Note? DevNote);


public class Node
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string NodeType { get; set; }
    public required string NodeSubType { get; set; }
    public string? NodeItem { get; set; }
    public string? NodeAddress { get; set; }
    public DoorEnvironment[]? DoorEnvironments { get; set; }
    public string? DoorOrientation { get; set; }
    public bool? UseImplicitDoorUnlocks { get; set; }
    public bool? UseImplicitLeaveNormally { get; set; }
    public bool? UseImplicitComeInNormally { get; set; }
    public bool? UseImplicitComeInWithMockball { get; set; }
    public Requirement? InteractionRequires { get; set; }
    public int? SpawnAt { get; set; }
    public NodeLock[]? Locks { get; set; }
    public TwinDoorAddress[]? TwinDoorAddresses { get; set; }
    public string[]? Utilities { get; set; }
    public ViewableNode[]? ViewableNodes { get; set; }
    public string[]? Yields { get; set; }
    public Note? Note { get; set; }
    public Note? DevNote { get; set; }
}

public record TwinDoorAddress(string RoomAddress, string DoorAddress);
public record ViewableNode(int Id, Strat[] Strats);

public class NodeLock
{
    public required string LockType { get; set; }
    public Requirement? Lock { get; set; }
    public required string Name { get; set; }
    public Strat[]? UnlockStrats { get; set; }
    public Note? Note { get; set; }
    public Note? DevNote { get; set; }
    public string[]? Yields { get; set; }
}

public record Link
(
    int From,
    LinkTo[] To
);

public record LinkTo
(
    int Id,
    Note? Note,
    Note? DevNote
);

public record Obstacle
(
    string Id,
    string Name,
    string ObstacleType,
    Note? Note,
    Note? DevNote
) : IComparable
{
    public int CompareTo([AllowNull] object other)
    {
        if (other == null)
        {
            return 1;
        }

        if (Id == ((Obstacle)other).Id)
        {
            return 0;
        }

        return Id.CompareTo(((Obstacle)other).Id);
    }
};

public record RoomEnemy
(
    string Id,
    string GroupName,
    string EnemyName,
    int Quantity,
    int[]? HomeNodes,
    int[]? BetweenNodes,
    Requirement? Spawn,
    Requirement? StopSpawn,
    Requirement? DropRequires,
    FarmCycle[]? FarmCycles,
    Note? Note,
    Note? DevNote
);

public record FarmCycle
(
    string Name,
    int CycleFrames,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

public record Runway
(
    int Length,
    int OpenEnd,
    int? GentleUpTiles,
    int? GentleDownTiles,
    int? SteepUpTiles,
    int? SteepDownTiles
);

public class Strat : ICloneable
{
    public int[]? Link { get; set; }
    public required string Name { get; set; }
    public bool? Notable { get; set; }
    public string? ReusableRoomwideNotable { get; set; }
    public EntranceCondition? EntranceCondition { get; set; }
    public required Requirement Requires { get; set; }
    public ExitCondition? ExitCondition { get; set; }
    public object? GModeRegainMobility { get; set; }
    public bool? BypassesDoorShell { get; set; }
    public UnlockDoorItem[]? UnlockDoors { get; set; }
    public string[]? ClearsObstacles { get; set; }
    public string[]? ResetsObstacles { get; set; }
    public string[]? SetsFlags { get; set; }
    public StratFailure[]? Failures { get; set; }
    public Note? Note { get; set; }
    public Note? DevNote { get; set; }

    public override string ToString()
    {
        // Return a format string with the strat name and links
        return $"{Name} ({(Link != null ? string.Join(", ", Link) : "")})";
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public record UnlockDoorItem
(
    int? NodeId,
    string[] Types,
    Requirement? Requires,
    bool? UseImplicitRequires,
    Note? Note,
    Note? DevNote
);

public record StratFailure
(
    string Name,
    int? LeadsToNode,
    Requirement? Cost,
    bool? Softlock,
    Note? Note,
    Note? DevNote
);

public record DoorEnvironment
(
    string Physics,
    int[]? EntranceNodes,
    Note? Note,
    Note? DevNote
);

public record RoomEnvironment
(
    bool Heated,
    List<int>? EntranceNodes,
    Note? Note,
    Note? DevNote
);


[JsonConverter(typeof(EntranceConditionConverter))]
abstract public record EntranceCondition
{
    public record ComeInNormally() : EntranceCondition;
    public record ComeInRunning(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    public record ComeInJumping(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    public record ComeInSpaceJumping(string SpeedBooster, decimal MinTiles, decimal? MaxTiles) : EntranceCondition;
    public record ComeInShineCharging(decimal Length, decimal OpenEnd, decimal? GentleUpTiles, decimal? GentleDownTiles, decimal? SteepUpTiles, decimal? SteepDownTiles) : EntranceCondition;
    public record ComeInGettingBlueSpeed(decimal Length, int OpenEnd, int? GentleUpTiles, int? GentleDownTiles, int? SteepUpTiles, int? SteepDownTiles, string? MinExtraRunSpeed, string? MaxExtraRunSpeed) : EntranceCondition;
    public record ComeInShineCharged(int FramesRequired) : EntranceCondition;
    public record ComeInShineChargedJumping(int FramesRequired) : EntranceCondition;
    public record ComeInWithSpark(string? Position) : EntranceCondition;
    public record ComeInStutterShineCharging(decimal MinTiles) : EntranceCondition;
    public record ComeInWithBombBoost() : EntranceCondition;
    public record ComeInWithDoorStuckSetup() : EntranceCondition;
    public record ComeInSpeedballing(Runway Runway) : EntranceCondition;
    public record ComeInWithTemporaryBlue() : EntranceCondition;
    public record ComeInSpinning(string SpeedBooster, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, decimal UnusableTiles) : EntranceCondition;
    public record ComeInBlueSpinning(string? MinExtraRunSpeed, string? MaxExtraRunSpeed, decimal UnusableTiles) : EntranceCondition;
    public record ComeInWithMockball(decimal? AdjacentMinTiles, decimal[][]? RemoteAndLandingMinTiles) : EntranceCondition;
    public record ComeInWithSpringBallBounce(string MovementType, decimal? AdjacentMinTiles, decimal[][]? RemoteAndLandingMinTiles) : EntranceCondition;
    public record ComeInWithBlueSpringBallBounce(string MovementType, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, decimal? MinLandingTiles) : EntranceCondition;
    public record ComeInWithStoredFallSpeed(int FallSpeedInTiles) : EntranceCondition;
    public record ComeInWithRMode() : EntranceCondition;
    public record ComeInWithGMode(string Mode, bool Morphed, string? Mobility) : EntranceCondition;
    public record ComeInWithWallJumpBelow(int MinHeight) : EntranceCondition;
    public record ComeInWithSpaceJumpBelow() : EntranceCondition;
    public record ComeInWithPlatformBelow(decimal? MinHeight, decimal? MaxHeight, decimal? MaxLeftPosition, decimal? MinRightPosition) : EntranceCondition;
    public record ComeInWithGrappleTeleport(int[][] BlockPositions) : EntranceCondition;
    public record ComesThroughToilet(string comesThroughToilet) : EntranceCondition;
}

public class EntranceConditionConverter : JsonConverter<EntranceCondition>
{
    public EntranceCondition? ParseElement(JsonElement element)
    {
        var entranceCondition = element.EnumerateObject().First();
        return entranceCondition.Name switch
        {
            "comeInNormally" => new EntranceCondition.ComeInNormally(),
            "comeInRunning" => new EntranceCondition.ComeInRunning(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInJumping" => new EntranceCondition.ComeInJumping(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInSpaceJumping" => new EntranceCondition.ComeInSpaceJumping(
                entranceCondition.Value.GetProperty("speedBooster").ToString(),
                entranceCondition.Value.GetProperty("minTiles").GetDecimal(),
                entranceCondition.Value.TryGetProperty("maxTiles", out var maxTiles) ? maxTiles.GetDecimal() : null),
            "comeInShinecharging" => new EntranceCondition.ComeInShineCharging(
                entranceCondition.Value.GetProperty("length").GetDecimal(),
                entranceCondition.Value.GetProperty("openEnd").GetDecimal(),
                entranceCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null),
            "comeInGettingBlueSpeed" => new EntranceCondition.ComeInGettingBlueSpeed(
                entranceCondition.Value.GetProperty("length").GetDecimal(),
                entranceCondition.Value.GetProperty("openEnd").GetInt32(),
                entranceCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetInt32() : null,
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null),
            "comeInShinecharged" => new EntranceCondition.ComeInShineCharged(0), //entranceCondition.Value.GetProperty("framesRequired").GetInt32()),
            "comeInShinechargedJumping" => new EntranceCondition.ComeInShineChargedJumping(entranceCondition.Value.GetProperty("framesRequired").GetInt32()),
            "comeInWithSpark" => new EntranceCondition.ComeInWithSpark(
                entranceCondition.Value.TryGetProperty("position", out var position) ? position.GetString() : null
            ),
            "comeInStutterShinecharging" => new EntranceCondition.ComeInStutterShineCharging(entranceCondition.Value.GetProperty("minTiles").GetDecimal()),
            "comeInWithBombBoost" => new EntranceCondition.ComeInWithBombBoost(),
            "comeInWithDoorStuckSetup" => new EntranceCondition.ComeInWithDoorStuckSetup(),
            "comeInSpeedballing" => new EntranceCondition.ComeInSpeedballing(
                JsonSerializer.Deserialize<Runway>(entranceCondition.Value.GetProperty("runway").GetRawText()) ?? new Runway(0, 0, null, null, null, null)),
            "comeInWithTemporaryBlue" => new EntranceCondition.ComeInWithTemporaryBlue(),
            "comeInSpinning" => new EntranceCondition.ComeInSpinning(
                entranceCondition.Value.GetProperty("speedBooster") switch
                {
                    JsonElement e when e.ValueKind == JsonValueKind.String => e.GetString() ?? "",
                    JsonElement e when e.ValueKind == JsonValueKind.True => "true",
                    JsonElement e when e.ValueKind == JsonValueKind.False => "false",
                    _ => throw new Exception($"Unknown speedBooster value: {entranceCondition.Value.GetProperty("speedBooster").GetRawText()}")
                },
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                entranceCondition.Value.GetProperty("unusableTiles").GetDecimal()),
            "comeInBlueSpinning" => new EntranceCondition.ComeInBlueSpinning(
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                entranceCondition.Value.GetProperty("unusableTiles").GetDecimal()),
            "comeInWithMockball" => new EntranceCondition.ComeInWithMockball(
                entranceCondition.Value.TryGetProperty("adjacentMinTiles", out var adjacentMinTiles) ? adjacentMinTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("remoteAndLandingMinTiles", out var remoteAndLandingMinTiles) ? remoteAndLandingMinTiles.EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetDecimal()).ToArray()).ToArray() : null),
            "comeInWithSpringBallBounce" => new EntranceCondition.ComeInWithSpringBallBounce(
                entranceCondition.Value.GetProperty("movementType").GetString() ?? "",
                entranceCondition.Value.TryGetProperty("adjacentMinTiles", out var adjacentMinTiles) ? adjacentMinTiles.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("remoteAndLandingMinTiles", out var remoteAndLandingMinTiles) ? remoteAndLandingMinTiles.EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetDecimal()).ToArray()).ToArray() : null),
            "comeInWithBlueSpringBallBounce" => new EntranceCondition.ComeInWithBlueSpringBallBounce(
                entranceCondition.Value.GetProperty("movementType").GetString() ?? "",
                entranceCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                entranceCondition.Value.TryGetProperty("minLandingTiles", out var minLandingTiles) ? minLandingTiles.GetDecimal() : null),
            "comeInWithStoredFallSpeed" => new EntranceCondition.ComeInWithStoredFallSpeed(entranceCondition.Value.GetProperty("fallSpeedInTiles").GetInt32()),
            "comeInWithRMode" => new EntranceCondition.ComeInWithRMode(),
            "comeInWithGMode" => new EntranceCondition.ComeInWithGMode(
                entranceCondition.Value.GetProperty("mode").GetString() ?? "",
                entranceCondition.Value.GetProperty("morphed").GetBoolean(),
                entranceCondition.Value.TryGetProperty("mobility", out var mobility) ? mobility.GetString() : null),
            "comeInWithWallJumpBelow" => new EntranceCondition.ComeInWithWallJumpBelow(entranceCondition.Value.GetProperty("minHeight").GetInt32()),
            "comeInWithSpaceJumpBelow" => new EntranceCondition.ComeInWithSpaceJumpBelow(),
            "comeInWithPlatformBelow" => new EntranceCondition.ComeInWithPlatformBelow(
                entranceCondition.Value.TryGetProperty("minHeight", out var minHeight) ? minHeight.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("maxHeight", out var maxHeight) ? maxHeight.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("maxLeftPosition", out var maxLeftPosition) ? maxLeftPosition.GetDecimal() : null,
                entranceCondition.Value.TryGetProperty("minRightPosition", out var minRightPosition) ? minRightPosition.GetDecimal() : null),
            "comeInWithGrappleTeleport" => new EntranceCondition.ComeInWithGrappleTeleport(
                entranceCondition.Value.GetProperty("blockPositions").EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetInt32()).ToArray()).ToArray()),
            "comesThroughToilet" => new EntranceCondition.ComesThroughToilet(entranceCondition.Value.GetString() ?? ""),
            _ => throw new Exception($"Unknown entrance condition: {entranceCondition.Name}")
        };
    }

    public override EntranceCondition? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, EntranceCondition value, System.Text.Json.JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

[JsonConverter(typeof(ExitConditionConverter))]
abstract public record ExitCondition
{
    public record LeaveNormally() : ExitCondition;
    public record LeaveWithRunway(decimal Length, decimal OpenEnd, decimal? GentleUpTiles, decimal? GentleDownTiles, decimal? SteepUpTiles, decimal? SteepDownTiles, decimal? StartingDownTiles) : ExitCondition;
    public record LeaveShineCharged(int FramesRequired) : ExitCondition;
    public record LeaveWithTemporaryBlue(string? Direction) : ExitCondition;
    public record LeaveWithSpark(string? Position) : ExitCondition;
    public record LeaveSpinning(Runway RemoteRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    public record LeaveWithMockball(Runway RemoteRunway, Runway LandingRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    public record LeaveWithSpringBallBounce(Runway RemoteRunway, Runway LandingRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue, string MovementType) : ExitCondition;
    public record LeaveSpaceJumping(Runway RemoteRunway, string? MinExtraRunSpeed, string? MaxExtraRunSpeed, string? Blue) : ExitCondition;
    public record LeaveWithStoredFallSpeed(int FallSpeedInTiles) : ExitCondition;
    public record LeaveWithGModeSetup(bool Knockback) : ExitCondition;
    public record LeaveWithGMode(bool Morphed) : ExitCondition;
    public record LeaveWithDoorFrameBelow(decimal Height) : ExitCondition;
    public record LeaveWithPlatformBelow(decimal Height, decimal LeftPosition, decimal RightPosition) : ExitCondition;
    public record LeaveWithGrappleTeleport(int[][] BlockPositions) : ExitCondition;
}

public class ExitConditionConverter : JsonConverter<ExitCondition>
{
    public ExitCondition? ParseElement(JsonElement element)
    {
        var exitCondition = element.EnumerateObject().First();
        return exitCondition.Name switch
        {
            "leaveNormally" => new ExitCondition.LeaveNormally(),
            "leaveWithRunway" => new ExitCondition.LeaveWithRunway(
                exitCondition.Value.GetProperty("length").GetDecimal(),
                exitCondition.Value.GetProperty("openEnd").GetDecimal(),
                exitCondition.Value.TryGetProperty("gentleUpTiles", out var gentleUpTiles) ? gentleUpTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("gentleDownTiles", out var gentleDownTiles) ? gentleDownTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("steepUpTiles", out var steepUpTiles) ? steepUpTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("steepDownTiles", out var steepDownTiles) ? steepDownTiles.GetDecimal() : null,
                exitCondition.Value.TryGetProperty("startingDownTiles", out var startingDownTiles) ? startingDownTiles.GetDecimal() : null),
            "leaveShinecharged" => new ExitCondition.LeaveShineCharged(
                //exitCondition.Value.GetProperty("framesRemaining").ValueKind switch
                //{
                //    JsonValueKind.Number => exitCondition.Value.GetProperty("framesRemaining").GetInt32(),
                //    JsonValueKind.String => exitCondition.Value.GetProperty("framesRemaining").GetString() switch
                //    {
                //        "auto" => -1,
                //        _ => throw new Exception($"Unknown shinecharged exit condition: {exitCondition.Value.GetProperty("framesRemaining").GetString()}")
                //    },
                //    _ => throw new Exception($"Unknown shinecharged exit condition: {exitCondition.Value.GetProperty("framesRemaining").GetString()}")
                //}
                0
            ),
            "leaveWithTemporaryBlue" => new ExitCondition.LeaveWithTemporaryBlue(
                exitCondition.Value.TryGetProperty("direction", out var direction) ? direction.GetString() : null
            ),
            "leaveWithSpark" => new ExitCondition.LeaveWithSpark(
                exitCondition.Value.TryGetProperty("position", out var position) ? position.GetString() : null
            ),
            "leaveSpinning" => new ExitCondition.LeaveSpinning(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithMockball" => new ExitCondition.LeaveWithMockball(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("landingRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithSpringBallBounce" => new ExitCondition.LeaveWithSpringBallBounce(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("landingRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null,
                exitCondition.Value.GetProperty("movementType").GetString() ?? ""
            ),
            "leaveSpaceJumping" => new ExitCondition.LeaveSpaceJumping(
                JsonSerializer.Deserialize<Runway>(exitCondition.Value.GetProperty("remoteRunway").GetRawText()) ?? new Runway(0, 0, null, null, null, null),
                exitCondition.Value.TryGetProperty("minExtraRunSpeed", out var minExtraRunSpeed) ? minExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("maxExtraRunSpeed", out var maxExtraRunSpeed) ? maxExtraRunSpeed.GetString() : null,
                exitCondition.Value.TryGetProperty("blue", out var blue) ? blue.GetString() : null
            ),
            "leaveWithStoredFallSpeed" => new ExitCondition.LeaveWithStoredFallSpeed(exitCondition.Value.GetProperty("fallSpeedInTiles").GetInt32()),
            "leaveWithGModeSetup" => new ExitCondition.LeaveWithGModeSetup(
                exitCondition.Value.TryGetProperty("knockback", out var knockback) ? knockback.GetBoolean() : true
             ),
            "leaveWithGMode" => new ExitCondition.LeaveWithGMode(exitCondition.Value.GetProperty("morphed").GetBoolean()),
            "leaveWithDoorFrameBelow" => new ExitCondition.LeaveWithDoorFrameBelow(exitCondition.Value.GetProperty("height").GetDecimal()),
            "leaveWithPlatformBelow" => new ExitCondition.LeaveWithPlatformBelow(
                exitCondition.Value.GetProperty("height").GetDecimal(),
                exitCondition.Value.GetProperty("leftPosition").GetDecimal(),
                exitCondition.Value.GetProperty("rightPosition").GetDecimal()),
            "leaveWithGrappleTeleport" => new ExitCondition.LeaveWithGrappleTeleport(
                exitCondition.Value.GetProperty("blockPositions").EnumerateArray().Select(e => e.EnumerateArray().Select(e => e.GetInt32()).ToArray()).ToArray()),
            _ => throw new Exception($"Unknown exit condition: {exitCondition.Name}")
        };
    }

    public override ExitCondition? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        return ParseElement(doc.RootElement);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, ExitCondition value, System.Text.Json.JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
