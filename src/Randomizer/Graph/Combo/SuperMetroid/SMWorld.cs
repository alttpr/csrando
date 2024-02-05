namespace Randomizer.Graph.Combo.SuperMetroid;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using ItemSet = Dictionary<ItemSetName, /* WeightedSet */ Dictionary<int, List<Item>>>;
using WeightedSet = Dictionary<int, List<Item>>;


// Represents a complex requirement for an edge
// This is more or less a copy of the requirement record from the SM model with some changes
// For example this will resolve item requirements to item objects for faster lookup later
public abstract record ComplexRequirement
{
    internal record Always : ComplexRequirement;
    internal record Never : ComplexRequirement;
    internal record Single(Item Item) : ComplexRequirement;
    internal record And(ComplexRequirement[] Reqs) : ComplexRequirement;
    internal record Not(ComplexRequirement Req) : ComplexRequirement;
    internal record Or(ComplexRequirement[] Reqs) : ComplexRequirement;
    internal record Ammo(Item Item, int Count) : ComplexRequirement;
    internal record AmmoDrain(string Type, int Count) : ComplexRequirement;
    internal record Refill(string[] Resources) : ComplexRequirement;
    internal record EnemyKill(
        string[][] Enemies,
        string[]? ExplicitWeapons = null,
        string[]? ExcludedWeapons = null,
        string[]? FarmableAmmo = null
    ) : ComplexRequirement
    {
        internal bool CanKillWith(Inventory inventory)
        {
            // Take our flags, and reduce them to the ones that are explicitly weapons, and the ones that are not excluded weapons
            //var filteredFlags = ExplicitWeapons == null ? flags.AsEnumerable() : flags.Where(flags => ExplicitWeapons.Contains(flags.Key));
            //filteredFlags = ExcludedWeapons == null ? filteredFlags : filteredFlags.Where(flags => !ExcludedWeapons.Contains(flags.Key));

            // Go through the enemies, and see if we can kill them with the filtered flags
            foreach (var enemy in Enemies.SelectMany(e => e))
            {
                // If we can't kill this enemy, return false

                // TODO: Implement this
                // if (!CanKillEnemy(enemy, filteredFlags))
                //      return false;
            }

            return true;
        }
    };
    internal record AcidFrames(int Frames) : ComplexRequirement;
    internal record GravitylessAcidFrames(int Frames) : ComplexRequirement;
    internal record DraygonElectricityFrames(int Frames) : ComplexRequirement;
    internal record EnemyDamage(string Enemy, string Type, int Hits) : ComplexRequirement;
    internal record HeatFrames(int Frames) : ComplexRequirement;
    internal record GravitylessHeatFrames(int Frames) : ComplexRequirement;
    internal record HibashiHits(int Hits) : ComplexRequirement;
    internal record LavaFrames(int Frames) : ComplexRequirement;
    internal record GravitylessLavaFrames(int Frames) : ComplexRequirement;
    internal record SamusEaterFrames(int Frames) : ComplexRequirement;
    internal record MetroidFrames(int Frames) : ComplexRequirement;
    internal record EnergyAtMost(int Energy) : ComplexRequirement;
    internal record AutoReserveTrigger(int MinReserveEnergy, int MaxReserveEnergy) : ComplexRequirement;
    internal record SpikeHits(int Hits) : ComplexRequirement;
    internal record ThornHits(int Hits) : ComplexRequirement;
    internal record DoorUnlockedAtNode(int Node) : ComplexRequirement;
    internal record ObstaclesCleared(string[] Obstacles) : ComplexRequirement;
    internal record ObstaclesNotCleared(string[] Obstacles) : ComplexRequirement;
    internal record ResourceCapacity((Item, int)[] Capacity) : ComplexRequirement;
    internal record CanShineCharge(
        World world,
        decimal UsedTiles,
        decimal OpenEnd,
        decimal? GentleUpTiles = null,
        decimal? GentleDownTiles = null,
        decimal? SteepUpTiles = null,
        decimal? SteepDownTiles = null,
        decimal? StartingDownTiles = null
    ) : ComplexRequirement;
    internal record Shinespark(int Frames, int? ExcessFrames = null) : ComplexRequirement;
    internal record ResetRoom(
        int[] Nodes,
        int[]? NodesToAvoid = null,
        bool? MustStayPut = null
    ) : ComplexRequirement;
    internal record ItemNotCollectedAtNode(int Node) : ComplexRequirement;

    public static ComplexRequirement FromRequirement(World world, Model.Requirement requirement)
    {
        return requirement switch
        {
            Model.Requirement.Always => new Always(),
            Model.Requirement.Never => new Never(),
            Model.Requirement.Single single => new Single(world.GetItem("SM" + single.Req, Game.SuperMetroid)),
            Model.Requirement.And and => new And(and.Reqs.Select(r => FromRequirement(world, r)).ToArray()),
            Model.Requirement.Not not => new Not(FromRequirement(world, not.Req)),
            Model.Requirement.Or or => new Or(or.Reqs.Select(r => FromRequirement(world, r)).ToArray()),
            Model.Requirement.Ammo ammo => new Ammo(world.GetItem("SM" + ammo.Type, Game.SuperMetroid), ammo.Count),
            Model.Requirement.AmmoDrain ammoDrain => new AmmoDrain(ammoDrain.Type, ammoDrain.Count),
            Model.Requirement.Refill refill => new Refill(refill.Resources),
            Model.Requirement.EnemyKill enemyKill => new EnemyKill(enemyKill.Enemies, enemyKill.ExplicitWeapons, enemyKill.ExcludedWeapons, enemyKill.FarmableAmmo),
            Model.Requirement.AcidFrames acidFrames => new AcidFrames(acidFrames.Frames),
            Model.Requirement.GravitylessAcidFrames gravitylessAcidFrames => new GravitylessAcidFrames(gravitylessAcidFrames.Frames),
            Model.Requirement.DraygonElectricityFrames draygonElectricityFrames => new DraygonElectricityFrames(draygonElectricityFrames.Frames),
            Model.Requirement.EnemyDamage enemyDamage => new EnemyDamage(enemyDamage.Enemy, enemyDamage.Type, enemyDamage.Hits),
            Model.Requirement.HeatFrames heatFrames => new Single(world.GetItem("SMVaria", Game.SuperMetroid)), //new HeatFrames(heatFrames.Frames),
            Model.Requirement.GravitylessHeatFrames gravitylessHeatFrames => new Single(world.GetItem("SMVaria", Game.SuperMetroid)), // new GravitylessHeatFrames(gravitylessHeatFrames.Frames),
            Model.Requirement.HibashiHits hibashiHits => new HibashiHits(hibashiHits.Hits),
            Model.Requirement.LavaFrames lavaFrames => new Single(world.GetItem("SMGravity", Game.SuperMetroid)),
            Model.Requirement.GravitylessLavaFrames gravitylessLavaFrames => new GravitylessLavaFrames(gravitylessLavaFrames.Frames),
            Model.Requirement.SamusEaterFrames samusEaterFrames => new SamusEaterFrames(samusEaterFrames.Frames),
            Model.Requirement.MetroidFrames metroidFrames => new MetroidFrames(metroidFrames.Frames),
            Model.Requirement.EnergyAtMost energyAtMost => new EnergyAtMost(energyAtMost.Energy),
            Model.Requirement.AutoReserveTrigger autoReserveTrigger => new AutoReserveTrigger(autoReserveTrigger.MinReserveEnergy, autoReserveTrigger.MaxReserveEnergy),
            Model.Requirement.SpikeHits spikeHits => new SpikeHits(spikeHits.Hits),
            Model.Requirement.ThornHits thornHits => new ThornHits(thornHits.Hits),
            Model.Requirement.DoorUnlockedAtNode doorUnlockedAtNode => new DoorUnlockedAtNode(doorUnlockedAtNode.Node),
            Model.Requirement.ObstaclesCleared obstaclesCleared => new ObstaclesCleared(obstaclesCleared.Obstacles),
            Model.Requirement.ObstaclesNotCleared obstaclesNotCleared => new ObstaclesNotCleared(obstaclesNotCleared.Obstacles),
            Model.Requirement.ResourceCapacity resourceCapacity => new ResourceCapacity(
                resourceCapacity.Capacity.Select(c => c.Type switch
                {
                    "Missile" => (world.GetItem("SMMissile", Game.SuperMetroid), c.Count / 5),
                    "Super" => (world.GetItem("SMSuper", Game.SuperMetroid), c.Count / 5),
                    "PowerBomb" => (world.GetItem("SMPowerBomb", Game.SuperMetroid), c.Count / 5),
                    "RegularEnergy" => (world.GetItem("SMETank", Game.SuperMetroid), c.Count / 100),
                    "ReserveEnergy" => (world.GetItem("SMReserveTank", Game.SuperMetroid), c.Count / 100),
                    _ => throw new NotImplementedException()
                }).ToArray()
            ),
            Model.Requirement.CanShineCharge canShineCharge => new CanShineCharge(world, canShineCharge.UsedTiles, canShineCharge.OpenEnd, canShineCharge.GentleUpTiles, canShineCharge.GentleDownTiles, canShineCharge.SteepUpTiles, canShineCharge.SteepDownTiles, canShineCharge.StartingDownTiles),
            Model.Requirement.Shinespark shinespark => new Shinespark(shinespark.Frames, shinespark.ExcessFrames),
            Model.Requirement.ResetRoom resetRoom => new ResetRoom(resetRoom.Nodes, resetRoom.NodesToAvoid, resetRoom.MustStayPut),
            Model.Requirement.ItemNotCollectedAtNode itemNotCollectedAtNode => new ItemNotCollectedAtNode(itemNotCollectedAtNode.Node),
            _ => throw new NotImplementedException()
        };
    }

    internal bool Check(Inventory inventory)
    {
        return this switch
        {
            Always => true,
            Never => false,
            Single req => inventory.Has(req.Item),
            And reqs => reqs.Reqs.All(req => req.Check(inventory)),
            Not req => !req.Req.Check(inventory),
            Or reqs => reqs.Reqs.Any(req => req.Check(inventory)),
            Ammo ammo => inventory.HasAtLeast(ammo.Item, ammo.Count / 5),
            AmmoDrain ammo => true,
            Refill resources => true,
            EnemyKill enemies => enemies.CanKillWith(inventory),
            AcidFrames frames => false,
            GravitylessAcidFrames frames => false,
            DraygonElectricityFrames frames => true,
            EnemyDamage enemy => true,
            HeatFrames frames => false,
            GravitylessHeatFrames frames => false,
            HibashiHits hits => true,
            LavaFrames frames => false,
            GravitylessLavaFrames frames => false,
            SamusEaterFrames frames => true,
            MetroidFrames frames => true,
            EnergyAtMost energy => true,
            AutoReserveTrigger minMax => false,
            SpikeHits hits => true,
            ThornHits hits => true,
            DoorUnlockedAtNode node => false,
            ObstaclesCleared obstacles => true,
            ObstaclesNotCleared obstacles => true,
            ResourceCapacity capacity => capacity.Capacity.All(c => inventory.HasAtLeast(c.Item1, c.Item2)),
            CanShineCharge usedTiles => usedTiles.UsedTiles switch
            {
                33 => inventory.Has(usedTiles.world.GetItem("SMcanShinespark")),
                _ => false,
            },
            Shinespark frames => true,
            ResetRoom nodes => false,
            ItemNotCollectedAtNode node => false,
            _ => throw new NotImplementedException()
        };
    }

    internal bool IsUnconditional()
    {
        // Returns true if this requirement passes with a blank inventory
        return this switch
        {
            Always => true,
            Never => false,
            Single req => false,
            And reqs => reqs.Reqs.All(req => req.IsUnconditional()),
            Not req => false,
            Or reqs => reqs.Reqs.Any(req => req.IsUnconditional()),
            Ammo ammo => false,
            AmmoDrain ammo => false,
            Refill resources => false,
            EnemyKill enemies => false,
            AcidFrames frames => false,
            GravitylessAcidFrames frames => false,
            DraygonElectricityFrames frames => false,
            EnemyDamage enemy => false,
            HeatFrames frames => false,
            GravitylessHeatFrames frames => false,
            HibashiHits hits => false,
            LavaFrames frames => false,
            GravitylessLavaFrames frames => false,
            SamusEaterFrames frames => false,
            MetroidFrames frames => false,
            EnergyAtMost energy => false,
            AutoReserveTrigger minMax => false,
            SpikeHits hits => false,
            ThornHits hits => false,
            DoorUnlockedAtNode node => false,
            ObstaclesCleared obstacles => false,
            ObstaclesNotCleared obstacles => false,
            ResourceCapacity capacity => false,
            CanShineCharge usedTiles => false,
            Shinespark frames => false,
            ResetRoom nodes => false,
            ItemNotCollectedAtNode node => false,
            _ => throw new NotImplementedException()
        };
    }
}

internal class SMWorld
{
    public static void AdjustWorld(World world)
    {
        var jsonReader = new SMJsonReader();
        jsonReader.Load();
        jsonReader.BuildGraph(world);

        var smVertices = jsonReader.GetVertices(world);

        // World built and connected, now place it into the actual main graph
        foreach (var vtx in smVertices)
        {
            var name = vtx.TryGetValue("name", out object? nameValue) ? (string)nameValue : throw new InvalidDataException("SM vertex without a name");
            var type = vtx.TryGetValue("type", out object? typeValue) ? (VertexType)typeValue : VertexType.Meta;
            var subtype = vtx.TryGetValue("subtype", out object? subtypeValue) ? (VertexType?)subtypeValue : (type == VertexType.Item ? VertexType.Standing : null);
            var item = vtx.TryGetValue("item", out object? itemValue) ? (string)itemValue : null;
            var itemset = vtx.TryGetValue("itemset", out object? itemsetValue) ? (string[])itemsetValue : null;
            var address = vtx.TryGetValue("address", out object? addressValue) ? (int?)addressValue : null;

            var vertex = new Vertex()
            {
                World = world,
                Name = name,
                Type = type,
                SubType = subtype,
                Item = item != null ? world.GetItem("SM" + item, Game.SuperMetroid) : null,
                ItemSet = itemset?.Select(i => new ItemSetName(i, world)).ToArray() ?? [],
                Addresses = address != null ? [(long)address.Value, (long)address.Value + 1, (long)address.Value + 5] : null,
                Game = Game.SuperMetroid
            };

            world.Graph.AddVertex(vertex);
            //Console.WriteLine($"Added vertex {vertex.Name}");
        }

        var smEdges = jsonReader.GetEdges(world);
        foreach (var edgeCollection in smEdges)
        {
            //var edgeCollectionData = edgeCollection.Key.Split(":").First().Split('|');
            //var requirementName = edgeCollectionData.First();
            //if (!requirementName.StartsWith("fixed"))
            //{
            //    requirementName = "SM" + requirementName;
            //}
            //var requirement = world.GetItem(requirementName, Game.SuperMetroid);
            //var requirementCount = int.Parse(edgeCollectionData.Skip(1).FirstOrDefault() ?? "1");

            var complexRequirement = ComplexRequirement.FromRequirement(world, edgeCollection.Key);
            var simpleRequirement = world.GetItem("SMComplexRequirement", Game.SuperMetroid);

            foreach (var edges in edgeCollection.Value.Directed)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, new ItemCondition(simpleRequirement, 1, complexRequirement));
                //Console.WriteLine($"Added directed edge from {from.Name} to {to.Name}");
            }

            foreach (var edges in edgeCollection.Value.Undirected)
            {
                var from = world.GetLocation(edges[0]);
                var to = world.GetLocation(edges[1]);
                if (from is null || to is null)
                {
                    throw new Exception("Name Connection Mismatch: " + $"({edges[0]}, {edges[1]}) => " + $"({from}, {to})");
                }

                world.Graph.AddDirected(from, to, new ItemCondition(simpleRequirement, 1, complexRequirement));
                world.Graph.AddDirected(to, from, new ItemCondition(simpleRequirement, 1, complexRequirement));
                //Console.WriteLine($"Added undirected edge from {from.Name} to {to.Name}");
            }
        }

        // Create a SM Meta node and hook up the helpers
        var smMeta = new Vertex()
        {
            World = world,
            Name = "SM - Meta",
            Type = VertexType.Meta,
            Game = Game.SuperMetroid
        };

        world.Graph.AddVertex(smMeta);

        foreach (var helper in jsonReader.Helpers.HelperCategories.SelectMany(h => h.Helpers))
        {
            var helperVertex = new Vertex()
            {
                World = world,
                Name = $"SM - Helper - {helper.Name}",
                Type = VertexType.Meta,
                Item = world.GetItem("SM" + helper.Name, Game.SuperMetroid),
                Game = Game.SuperMetroid
            };

            var helperRequirement = new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, ComplexRequirement.FromRequirement(world, helper.Requires));

            world.Graph.AddVertex(helperVertex);
            world.Graph.AddDirected(smMeta, helperVertex, helperRequirement);
        }

        // Hook up SM techs
        foreach (var tech in jsonReader.Techs.TechCategories.SelectMany(t => t.Techs))
        {
            AddTech(world, smMeta, tech);
        }

        // Connect SM to the main world graph
        world.Graph.AddDirected(world.GetLocation("start"), world.GetLocation("SM - Meta"), world.GetItem("fixed"));
        
         

        // Add undirected path between the games
        world.Graph.AddDirected(world.GetLocation("Lake Hylia North West Shore"), world.GetLocation("SM - Crateria - Parlor and Alcatraz - Bottom Right Door (On the Left Shaft)"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("SM - Crateria - Parlor and Alcatraz - Bottom Right Door (On the Left Shaft)"), world.GetLocation("Lake Hylia North West Shore"), world.GetItem("fixed"));

        // Add norfair map to death moutain portal
        world.Graph.AddDirected(world.GetLocation("SM - Norfair - Business Center - Middle Left Door"), world.GetLocation("West Death Mountain"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("West Death Mountain"), world.GetLocation("SM - Norfair - Business Center - Middle Left Door"), world.GetItem("fixed"));

        // Add maridia missile refill to dark world shopping mall
        world.Graph.AddDirected(world.GetLocation("SM - Maridia - Halfie Climb Room - Bottom Right Door"), world.GetLocation("Dark Shopping Mall"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("Dark Shopping Mall"), world.GetLocation("SM - Maridia - Halfie Climb Room - Bottom Right Door"), world.GetItem("fixed"));

        // Add lower norfair refill to mire area
        world.Graph.AddDirected(world.GetLocation("SM - Norfair - Screw Attack Room - Middle Right Door"), world.GetLocation("Mire"), world.GetItem("fixed"));
        world.Graph.AddDirected(world.GetLocation("Mire"), world.GetLocation("SM - Norfair - Screw Attack Room - Middle Right Door"), world.GetItem("fixed"));
        


        // Patch maridia main street (since we're cheating with shinespark nodes)
        world.Graph.AddDirected(
            world.GetLocation("SM - Maridia - Main Street - Bottom Door"), 
            world.GetLocation("SM - Maridia - Main Street - Speed Blocked Item"), 
            new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, new ComplexRequirement.And(
                [
                    new ComplexRequirement.Single(world.GetItem("SMcanShinespark", Game.SuperMetroid)),
                    new ComplexRequirement.Single(world.GetItem("SMSpeedBooster", Game.SuperMetroid)), 
                    new ComplexRequirement.Single(world.GetItem("SMGravity", Game.SuperMetroid))
                ]
            ))
        );

        world.StartingItems.AddItem(world.GetItem("SMf_ZebesAwake"), 1);
    }

    private static void AddTech(World world, Vertex meta, Model.Tech tech)
    {
        string[] allowedTechs = [
            "canMidAirMorph",
            "canUseGrapple",
            "canCrouchJump",
            "canWalljump",
            "canUnmorphBombBoost",
            "canIBJ",
            "canJumpIntoIBJ",
            "canShinespark",
            "canHorizontalShinespark",
            "canMidairShinespark",
            "canShinechargeMovement",
            "canUseSpeedEchoes",
            "canAwakenZebes",
            "canCarefulJump"
        ];

        if (!allowedTechs.Contains(tech.Name))
        {
            return;
        }

        var techVertex = new Vertex()
        {
            World = world,
            Name = $"SM - Tech - {tech.Name}",
            Type = VertexType.Meta,
            Item = world.GetItem("SM" + tech.Name, Game.SuperMetroid),
            Game = Game.SuperMetroid
        };

        var techRequirement = new ItemCondition(world.GetItem("SMComplexRequirement", Game.SuperMetroid), 1, ComplexRequirement.FromRequirement(world, tech.Requires));

        world.Graph.AddVertex(techVertex);
        world.Graph.AddDirected(meta, techVertex, techRequirement);

        foreach (var extTech in tech.ExtensionTechs ?? [])
        {
            AddTech(world, meta, extTech);
        }
    }

    public static ItemSet GetItemSet(World world)
    {
        return new ItemSet
        {
            { ItemSetName.DefaultSet, new WeightedSet
                {
                    { 3, [
                            world.GetItem("SMBombs", Game.SuperMetroid),
                            world.GetItem("SMVaria", Game.SuperMetroid),
                            world.GetItem("SMGravity", Game.SuperMetroid),
                            world.GetItem("SMCharge", Game.SuperMetroid),
                            world.GetItem("SMIce", Game.SuperMetroid),
                            world.GetItem("SMWave", Game.SuperMetroid),
                            world.GetItem("SMPlasma", Game.SuperMetroid),
                            world.GetItem("SMSpazer", Game.SuperMetroid),
                            world.GetItem("SMXRayScope", Game.SuperMetroid),
                            world.GetItem("SMGrapple", Game.SuperMetroid),
                            world.GetItem("SMSpringBall", Game.SuperMetroid),
                            world.GetItem("SMScrewAttack", Game.SuperMetroid),
                            world.GetItem("SMHiJump", Game.SuperMetroid),
                            world.GetItem("SMSpaceJump", Game.SuperMetroid),
                            world.GetItem("SMSpeedBooster", Game.SuperMetroid),
                            .. Enumerable.Repeat(world.GetItem("SMPowerBomb", Game.SuperMetroid), 3),
                            .. Enumerable.Repeat(world.GetItem("SMSuper", Game.SuperMetroid), 3),
                            .. Enumerable.Repeat(world.GetItem("SMMissile", Game.SuperMetroid), 8),
                            .. Enumerable.Repeat(world.GetItem("SMETank", Game.SuperMetroid), 5),
                            .. Enumerable.Repeat(world.GetItem("SMReserveTank", Game.SuperMetroid), 4)

                        ]
                    },
                    { 9001, [
                            .. Enumerable.Repeat(world.GetItem("SMMissile", Game.SuperMetroid), 32),
                            .. Enumerable.Repeat(world.GetItem("SMSuper", Game.SuperMetroid), 12),
                            .. Enumerable.Repeat(world.GetItem("SMPowerBomb", Game.SuperMetroid), 7),
                            .. Enumerable.Repeat(world.GetItem("SMETank", Game.SuperMetroid), 9),
                        ]
                    }
                }
            },
            { new ItemSetName("lw", world), new WeightedSet
                {
                    { 4, [ world.GetItem("SMMorph", Game.SuperMetroid)] }
                }
            }
        };
    }
}
