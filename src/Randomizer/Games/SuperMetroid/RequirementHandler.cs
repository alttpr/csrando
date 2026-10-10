namespace Randomizer.Games.SuperMetroid;

using System;
using System.Collections.Generic;
using System.Linq;
using Randomizer.Games.Metadata;
using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;

public class RequirementResult
{
    public bool Met { get; set; }
    public RequirementCost? Cost { get; set; }
    public HashSet<string>? Missing { get; set; }
    public Dictionary<string, int>? UsedItems { get; set; }

    public static RequirementResult Fail(params string[] missing) => new RequirementResult { Met = false, Cost = null, Missing = missing.Length > 0 ? new HashSet<string>(missing) : null };
    public static RequirementResult Success(RequirementCost cost, string? item = null, int count = 1)
    {
        var result = new RequirementResult { Met = true, Cost = cost };
        if (item != null)
            result.UsedItems = new Dictionary<string, int> { [item] = count };
        return result;
    }

    public void MergeSuccess(RequirementResult other)
    {
        if (other.UsedItems == null)
            return;
        UsedItems ??= [];
        foreach (var (item, count) in other.UsedItems)
            UsedItems[item] = Math.Max(UsedItems.GetValueOrDefault(item), count);
    }

    public void MergeFail(RequirementResult other)
    {
        if (other.Missing == null || other.Missing.Count == 0)
            return;

        if (Missing == null)
        {
            Missing = new HashSet<string>(other.Missing);
        }
        else
        {
            Missing.UnionWith(other.Missing);
        }
    }

    public override string ToString()
    {
        if (Met)
            return $"[Met: Cost = {Cost}]";
        else
            return $"[Not Met: Missing = {string.Join(", ", Missing ?? [])}]";
    }
}

public struct RequirementCost
{
    public int Energy;
    public int Missiles;
    public int SuperMissiles;
    public int PowerBombs;

    public static RequirementCost ZeroCost => new RequirementCost { Energy = 0, Missiles = 0, SuperMissiles = 0, PowerBombs = 0 };
    public override string ToString() => $"Energy: {Energy}, Missiles: {Missiles}, SuperMissiles: {SuperMissiles}, PowerBombs: {PowerBombs}";

    public static RequirementCost operator +(RequirementCost a, RequirementCost b) => new RequirementCost
    {
        Energy = a.Energy + b.Energy,
        Missiles = a.Missiles + b.Missiles,
        SuperMissiles = a.SuperMissiles + b.SuperMissiles,
        PowerBombs = a.PowerBombs + b.PowerBombs
    };

    public static RequirementCost operator -(RequirementCost a, RequirementCost b) => new RequirementCost
    {
        Energy = a.Energy - b.Energy,
        Missiles = a.Missiles - b.Missiles,
        SuperMissiles = a.SuperMissiles - b.SuperMissiles,
        PowerBombs = a.PowerBombs - b.PowerBombs
    };

    public static RequirementCost operator |(RequirementCost a, RequirementCost b)
    {
        var weightedAmmoA = a.Missiles + a.SuperMissiles * 3 + a.PowerBombs * 4;
        var weightedAmmoB = b.Missiles + b.SuperMissiles * 3 + b.PowerBombs * 4;

        // Return the cheapest cost, with energy cost being the most important
        if (a.Energy < b.Energy)
        {
            return a;
        }
        else if (a.Energy > b.Energy)
        {
            return b;
        }
        else if (weightedAmmoA < weightedAmmoB)
        {
            return a;
        }
        else if (weightedAmmoA > weightedAmmoB)
        {
            return b;
        }
        else
        {
            return a;
        }
    }
}

public class RequirementHandler
{
    private readonly Dictionary<string, Requirement> HelperTechs = new Dictionary<string, Requirement>();
    private readonly Dictionary<string, Enemy> Enemies = new Dictionary<string, Enemy>();
    private readonly Dictionary<(string, string), Attack> EnemyDamage = new Dictionary<(string, string), Attack>();
    private readonly Dictionary<string, EnemyDrops> EnemyDropExpectations = new Dictionary<string, EnemyDrops>();
    private readonly HashSet<string> AllowedNotableStrategies = new HashSet<string>();
    private const decimal DropRateDenominator = 102m;
    private static readonly EnemyDrops ZeroEnemyDrops = new EnemyDrops(0, 0, 0, 0, 0, 0);
    // Vanilla SM drop contents per pickup; used to convert expected drop counts to resources.
    private const int SmallEnergyDropValue = 5;
    private const int BigEnergyDropValue = 20;
    private const int MissileDropValue = 1;
    private const int SuperMissileDropValue = 2;
    private const int PowerBombDropValue = 1;

    public void Initialize(JsonReader reader, World world)
    {
        var preprocessor = new GraphPreprocessor(reader, world);

        foreach (var helper in reader.Helpers.HelperCategories.SelectMany(h => h.Helpers))
        {
            HelperTechs[helper.Name] = preprocessor.OptimizeRequirement(helper.Requires);
        }

        foreach (var tech in reader.Techs.TechCategories.SelectMany(t => t.Techs))
        {
            AddTech(tech, world.AllowedTechs, preprocessor);
            AddTech(tech, world.AllowedTechs, preprocessor, true);
        }

        foreach (var enemy in reader.Enemies.SelectMany(e => e.Enemies))
        {
            Enemies[enemy.Name] = enemy;
            EnemyDropExpectations[enemy.Name] = CalculatePerEnemyDropExpectation(enemy);
            foreach (var attack in enemy.Attacks)
            {
                EnemyDamage[(enemy.Name, attack.Name)] = attack;
            }
        }

        foreach (var strategy in reader.NotableStrategies)
        {
            if (IsStrategyAllowed(strategy, world.Config.Logic))
            {
                AllowedNotableStrategies.Add(strategy.Name);
            }
        }
    }

    private void AddTech(Tech tech, List<string> allowedTechs, GraphPreprocessor preprocessor, bool techOnly = false)
    {
        if (allowedTechs.Contains(tech.Name))
        {
            HelperTechs[techOnly ? $"t_{tech.Name}" : tech.Name] = preprocessor.OptimizeRequirement(
                techOnly ? tech.TechRequires : new Requirement.And([tech.TechRequires, tech.OtherRequires]));
        }

        if (tech.ExtensionTechs != null)
        {
            foreach (var ext in tech.ExtensionTechs)
            {
                AddTech(ext, allowedTechs, preprocessor, techOnly);
            }
        }
    }

    public RequirementResult HandleRequirement(Requirement req, VisitedState state, Inventory inventory, World world, HashSet<Weapon> weapons)
    {
        switch (req)
        {
            case Requirement.Always:
                return RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.Never:
                return RequirementResult.Fail();

            case Requirement.Single single:
                if (HelperTechs.TryGetValue(single.Req, out var helper))
                {
                    return HandleRequirement(helper, state, inventory, world, weapons);
                }
                else
                {
                    if (inventory.Has(world.GetItem(single.Req)))
                        return RequirementResult.Success(RequirementCost.ZeroCost, single.Req);
                    else
                        return RequirementResult.Fail(single.Req);
                }

            case Requirement.SingleItem singleItem:
                if (inventory.Has(singleItem.Item))
                {
                    return RequirementResult.Success(RequirementCost.ZeroCost, singleItem.Item.Name);
                }
                else
                {
                    if (HelperTechs.TryGetValue(singleItem.Item.Name, out var singleItemHelper))
                    {
                        return HandleRequirement(singleItemHelper, state, inventory, world, weapons);
                    }
                    else
                    {
                        if (inventory.Has(world.GetItem(singleItem.Item.Name)))
                            return RequirementResult.Success(RequirementCost.ZeroCost, singleItem.Item.Name);
                        else
                            return RequirementResult.Fail(singleItem.Item.Name);
                    }
                }

            case Requirement.And and:
                var totalCost = RequirementCost.ZeroCost;
                var successResult = RequirementResult.Success(RequirementCost.ZeroCost);
                var failResult = RequirementResult.Fail();
                bool failedAnd = false;
                // We'll store success by default, and if we find a fail, we’ll flip it.

                foreach (var subReq in and.Reqs)
                {
                    var subResult = HandleRequirement(subReq, state, inventory, world, weapons);

                    if (!subResult.Met)
                    {
                        // Merge the missing items from the failing sub-requirement
                        failResult.MergeFail(subResult);
                        failedAnd = true;
                    }
                    else
                    {
                        // Accumulate cost
                        totalCost += subResult.Cost!.Value;
                        successResult.MergeSuccess(subResult);
                    }
                }

                if (failedAnd)
                {
                    return failResult;
                }
                else
                {
                    successResult.Cost = totalCost;
                    return successResult;
                }

            case Requirement.Or or:
                RequirementResult? bestSuccess = null;
                // Collect missing items from all failing sub-requirements
                var combinedFail = RequirementResult.Fail();

                foreach (var subReq in or.Reqs)
                {
                    var subResult = HandleRequirement(subReq, state, inventory, world, weapons);
                    if (subResult.Met)
                    {
                        if (bestSuccess == null)
                        {
                            bestSuccess = subResult;
                        }
                        else
                        {
                            var newCost = subResult.Cost! | bestSuccess.Cost!;
                            if (newCost.Value.Equals(subResult.Cost!.Value))
                                bestSuccess = subResult;
                        }
                    }
                    else
                    {
                        // Merge the missing items from that failing subReq
                        combinedFail.MergeFail(subResult);
                    }
                }

                if (bestSuccess == null)
                {
                    // All sub-reqs failed. Return all missing items from all of them.
                    return combinedFail;
                }
                else
                {
                    return bestSuccess;
                }

            case Requirement.Not not:
                return HandleRequirement(not.Req, state, inventory, world, weapons) == null ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail();

            case Requirement.ObstaclesNotCleared obstaclesNotCleared:
                return (state.ObstacleBitFlags & ObstacleMaskFromArray(obstaclesNotCleared.Obstacles)) == 0 ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail();

            case Requirement.ObstaclesCleared obstaclesCleared:
                return (state.ObstacleBitFlags & ObstacleMaskFromArray(obstaclesCleared.Obstacles)) == 0 ? RequirementResult.Fail() : RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.Ammo ammo:
                if (inventory.Has(world.GetItem(ammo.Type)))
                {
                    return RequirementResult.Success(new RequirementCost
                    {
                        Energy = 0,
                        Missiles = ammo.Type == "Missile" ? ammo.Count : 0,
                        SuperMissiles = ammo.Type == "Super" ? ammo.Count : 0,
                        PowerBombs = ammo.Type == "PowerBomb" ? ammo.Count : 0
                    }, ammo.Type);
                }
                else
                {
                    return RequirementResult.Fail(ammo.Type);
                }

            case Requirement.AmmoDrain ammoDrain:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = 0,
                    Missiles = ammoDrain.Type == "Missile" ? ammoDrain.Count | 0x8000 : 0,
                    SuperMissiles = ammoDrain.Type == "Super" ? ammoDrain.Count | 0x8000 : 0,
                    PowerBombs = ammoDrain.Type == "PowerBomb" ? ammoDrain.Count | 0x8000 : 0
                });

            case Requirement.PartialRefill partial:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = partial.Resources.Contains("Energy") ? -partial.Limit : 0,
                    Missiles = partial.Resources.Contains("Missile") ? -partial.Limit : 0,
                    SuperMissiles = partial.Resources.Contains("Super") ? -partial.Limit : 0,
                    PowerBombs = partial.Resources.Contains("PowerBomb") ? -partial.Limit : 0
                });

            case Requirement.Refill refill:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = refill.Resources.Contains("Energy") ? -99999 : 0,
                    Missiles = refill.Resources.Contains("Missile") ? -99999 : 0,
                    SuperMissiles = refill.Resources.Contains("Super") ? -99999 : 0,
                    PowerBombs = refill.Resources.Contains("PowerBomb") ? -99999 : 0
                });

            /*
             *           "enemyKill": {
                        "type": "object",
                        "title": "Enemy Kill",
                        "description": "Describes the need to be able to kill a set of enemies. By default, allows all non-situational weapons (provided they can damage the enemies)",
                        "required": ["enemies"],
                        "additionalProperties": false,
                        "properties": {
                          "enemies": {
                            "type": "array",
                            "title": "Enemy Groups",
                            "description": "An array of enemy groups that must be killed. All enemies in each group can be hit by the same attack from an area of effect weapon.",
                            "items": {
                              "type": "array",
                              "title": "Enemy Group",
                              "description": "A single group of enemies that can be hit by the same attack from an area of effect weapon.",
                              "items": {
                                "type": "string",
                                "title": "Enemy Name",
                                "description": "The name of an enemy, as found in the enemies file or the boss file."
                              }
                            }
                          },*/

            case Requirement.EnemyKill enemyKill:
                var candidateWeapons = new List<Weapon>();
                var usedWeapons = new HashSet<Weapon>();
                foreach (var weapon in weapons)
                {

                    if (enemyKill.ExcludedWeapons != null && enemyKill.ExcludedWeapons.Contains(weapon.Name))
                    {
                        continue;
                    }

                    if (enemyKill.ExplicitWeapons != null && !enemyKill.ExplicitWeapons.Contains(weapon.Name))
                    {
                        continue;
                    }

                    candidateWeapons.Add(weapon);
                }

                // If no weapons pass initial criteria, fail quickly
                if (candidateWeapons.Count == 0)
                {
                    return RequirementResult.Fail(new[]
                    {
                        "Missile", "Super", "PowerBomb", "Charge", "Ice",
                        "Spazer", "WaveBeam", "Plasma", "ScrewAttack", "Bombs"
                    });
                }


                // For each enemy group, check if at least one weapon can hurt them
                foreach (var enemyGroup in enemyKill.Enemies)
                {
                    var enemyType = enemyGroup.First();
                    var enemyCount = enemyGroup.Count();

                    var enemy = Enemies[enemyType];
                    // Possibly use a cached HashSet if performance is an issue
                    var invulSet = new HashSet<string>(enemy.Invul);

                    bool canKillEnemy = false;
                    foreach (var w in candidateWeapons)
                    {
                        if (!invulSet.Contains(w.Name))
                        {
                            canKillEnemy = true;
                            usedWeapons.Add(w);
                            break;
                        }
                    }

                    if (!canKillEnemy)
                    {
                        // If we can't kill an enemy of this type, fail immediately
                        return RequirementResult.Fail(new[]
                        {
                            "Missile", "Super", "PowerBomb", "Charge", "Ice",
                            "Spazer", "WaveBeam", "Plasma", "ScrewAttack", "Bombs"
                        });
                    }
                }

                // If we get here, we can kill at least one enemy in each group
                var enemyKillResult = RequirementResult.Success(RequirementCost.ZeroCost);
                foreach (var weapon in usedWeapons)
                {
                    enemyKillResult.MergeSuccess(HandleRequirement(
                        weapon.UseRequires, state, inventory, world, weapons));
                }

                return enemyKillResult;


            case Requirement.HibashiHits hibashiHits:
                var hibashiDamage = hibashiHits.Hits * 30;
                if (inventory.Has(world.GetItem("Varia")))
                {
                    hibashiDamage /= 2;
                }
                else if (inventory.Has(world.GetItem("Gravity")))
                {
                    hibashiDamage /= 4;
                }


                return RequirementResult.Success(new RequirementCost
                {
                    Energy = hibashiDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.SpikeHits spikeHits:
                var spikeDamage = spikeHits.Hits * 60;
                if (inventory.Has(world.GetItem("Varia")))
                {
                    spikeDamage /= 2;
                }
                else if (inventory.Has(world.GetItem("Gravity")))
                {
                    spikeDamage /= 4;
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = spikeDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.ThornHits thornHits:
                var thornDamage = thornHits.Hits * 16;
                if (inventory.Has(world.GetItem("Varia")))
                {
                    thornDamage /= 2;
                }
                else if (inventory.Has(world.GetItem("Gravity")))
                {
                    thornDamage /= 4;
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = thornDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.ElectricityHits electricityHits:
                var electricityDamage = electricityHits.Hits * 30;
                if (inventory.Has(world.GetItem("Varia")))
                {
                    electricityDamage /= 2;
                }
                else if (inventory.Has(world.GetItem("Gravity")))
                {
                    electricityDamage /= 4;
                }
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = electricityDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.EnemyDamage enemyDamage:
                if (!EnemyDamage.TryGetValue((enemyDamage.Enemy, enemyDamage.Type), out var attack))
                {
                    return RequirementResult.Fail();
                }

                var attackDamage = attack.BaseDamage * enemyDamage.Hits;
                if (attack.AffectedByVaria ?? true == true && inventory.Has(world.GetItem("Varia")))
                {
                    attackDamage /= 2;
                }
                else if (attack.AffectedByGravity ?? true == true && inventory.Has(world.GetItem("Gravity")))
                {
                    attackDamage /= 4;
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = (int)(attackDamage * world.Config.LogicSkillConfigs[world.Config.Logic].EnemyDamageMultiplier),
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.HeatFrames heatFrames:
                var hasVaria = inventory.Has(world.GetItem("Varia"));
                var canHellrun = HelperTechs.ContainsKey("canHeatRun");

                if (!hasVaria && !canHellrun)
                {
                    return RequirementResult.Fail("Varia");
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = hasVaria ? 0 : (int)((heatFrames.Frames / 4) * world.Config.LogicSkillConfigs[world.Config.Logic].HeatDamageMultiplier),
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                }, hasVaria ? "Varia" : null);

            case Requirement.HeatFramesWithEnergyDrops heatFramesWithEnergyDrops:
                var hasVariaHf = inventory.Has(world.GetItem("Varia"));
                var canHellrunHf = HelperTechs.ContainsKey("canHeatRun");

                if (!hasVariaHf && !canHellrunHf)
                {
                    return RequirementResult.Fail("Varia");
                }

                var baseHeatDamage = hasVariaHf ? 0 : (int)((heatFramesWithEnergyDrops.Frames / 4) * world.Config.LogicSkillConfigs[world.Config.Logic].HeatDamageMultiplier);
                if (state.Energy < baseHeatDamage)
                {
                    return RequirementResult.Fail();
                }

                var dropEnergy = GetResourceGainFromDrops(GetExpectedDrops(heatFramesWithEnergyDrops.Drops)).Energy;
                var netHeatDamage = Math.Max(0, baseHeatDamage - dropEnergy);

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = netHeatDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                }, hasVariaHf ? "Varia" : null);

            case Requirement.SamusEaterFrames samusEaterFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Gravity")) ? samusEaterFrames.Frames / 40 : inventory.Has(world.GetItem("Varia")) ? samusEaterFrames.Frames / 20 : samusEaterFrames.Frames / 10,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.MetroidFrames metroidFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Gravity")) ? metroidFrames.Frames : inventory.Has(world.GetItem("Varia")) ? metroidFrames.Frames / 2 : metroidFrames.Frames / 4,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            // TODO: Implement this
            case Requirement.ResourceAtMost resourceAtMost:
                return RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.ResourceAvailable resourceAvailable:
                var failedResource = RequirementResult.Fail();
                bool failed = false;
                foreach (var resource in resourceAvailable.Available)
                {
                    var (hasResource, resourceName) = resource.Type switch
                    {
                        // TODO: fix reserve
                        "ReserveEnergy" => (false, "ReserveTank"),
                        "Energy" => (state.Energy >= resource.Count, "ETank"),
                        "RegularEnergy" => (state.Energy >= resource.Count, "ETank"),
                        "Missile" => (state.Missiles >= resource.Count, "Missile"),
                        "Super" => (state.SuperMissiles >= resource.Count, "Super"),
                        "PowerBomb" => (state.PowerBombs >= resource.Count, "PowerBomb"),
                        _ => (false, "")
                    };

                    if (!hasResource)
                    {
                        failed = true;
                        if (resourceName != "")
                        {
                            failedResource.MergeFail(RequirementResult.Fail(resourceName));
                        }
                    }
                }

                if (failed)
                {
                    return failedResource;
                }
                else
                {
                    var availableResult = RequirementResult.Success(RequirementCost.ZeroCost);
                    foreach (var resource in resourceAvailable.Available)
                    {
                        var (item, count) = RequiredExpansion(
                            resource.Type, resource.Count);
                        if (count > 0)
                        {
                            availableResult.UsedItems ??= [];
                            availableResult.UsedItems[item] = count;
                        }
                    }
                    return availableResult;
                }

            case Requirement.CycleFrames cycleFrames:
                return RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.LavaFrames lavaFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Gravity")) ? 0 : inventory.Has(world.GetItem("Varia")) ? lavaFrames.Frames / 4 : lavaFrames.Frames / 2,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.AcidFrames acidFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Gravity")) ? acidFrames.Frames / 3 : inventory.Has(world.GetItem("Varia")) ? acidFrames.Frames : acidFrames.Frames * 2,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.GravitylessAcidFrames gravitylessAcidFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Varia")) ? gravitylessAcidFrames.Frames : gravitylessAcidFrames.Frames * 2,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.GravitylessHeatFrames gravitylessHeatFrames:
                var hasVariaG = inventory.Has(world.GetItem("Varia"));
                var canHellrunG = HelperTechs.ContainsKey("canHeatRun");

                if (!hasVariaG && !canHellrunG)
                {
                    return RequirementResult.Fail("Varia");
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = hasVariaG ? 0 : (int)((gravitylessHeatFrames.Frames / 4) * world.Config.LogicSkillConfigs[world.Config.Logic].HeatDamageMultiplier),
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                }, hasVariaG ? "Varia" : null);

            case Requirement.GravitylessLavaFrames gravitylessLavaFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Varia")) ? 0 : (int)((gravitylessLavaFrames.Frames / 4) * world.Config.LogicSkillConfigs[world.Config.Logic].HeatDamageMultiplier),
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.DraygonElectricityFrames draygonElectricityFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Gravity")) ? draygonElectricityFrames.Frames / 4 : inventory.Has(world.GetItem("Varia")) ? draygonElectricityFrames.Frames / 2 : draygonElectricityFrames.Frames,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.Shinespark shinespark:
                if (inventory.Has(world.GetItem("SpeedBooster")))
                {
                    var requiredEnergy = shinespark.Frames - shinespark.ExcessFrames;
                    if (state.Energy - requiredEnergy <= 29)
                    {
                        return RequirementResult.Fail("ETank");
                    }

                    return RequirementResult.Success(new RequirementCost
                    {
                        Energy = shinespark.Frames,
                        Missiles = 0,
                        SuperMissiles = 0,
                        PowerBombs = 0
                    }, "SpeedBooster");
                }
                else
                {
                    return RequirementResult.Fail("SpeedBooster");
                }

            case Requirement.ResourceCapacity capacity:
                var failedCapacity = RequirementResult.Fail();
                bool failedCap = false;
                foreach (var c in capacity.Capacity)
                {
                    var (item, count) = RequiredExpansion(c.Type, c.Count);
                    if (item == "" || !inventory.HasAtLeast(world.GetItem(item), count))
                    {
                        failedCapacity.MergeFail(RequirementResult.Fail(
                            item == "" ? c.Type : item));
                        failedCap = true;
                    }
                }

                if (failedCap)
                    return failedCapacity;
                var capacityResult = RequirementResult.Success(RequirementCost.ZeroCost);
                capacityResult.UsedItems = [];
                foreach (var c in capacity.Capacity)
                {
                    var (item, count) = RequiredExpansion(c.Type, c.Count);
                    if (count > 0)
                    {
                        capacityResult.UsedItems[item] = Math.Max(
                            capacityResult.UsedItems.GetValueOrDefault(item), count);
                    }
                }
                return capacityResult;

            case Requirement.CanShineCharge canShineCharge:
                return inventory.Has(world.GetItem("SpeedBooster")) && canShineCharge.UsedTiles >= world.Config.LogicSkillConfigs[world.Config.Logic].ShinechargeTiles ? RequirementResult.Success(RequirementCost.ZeroCost, "SpeedBooster") : (canShineCharge.UsedTiles < 25 ? RequirementResult.Fail() : RequirementResult.Fail("SpeedBooster"));

            case Requirement.GetBlueSpeed blueSpeed:
                if (blueSpeed.UsedTiles < world.Config.LogicSkillConfigs[world.Config.Logic].ShinechargeTiles)
                {
                    return RequirementResult.Fail();
                }
                return inventory.Has(world.GetItem("SpeedBooster")) ? RequirementResult.Success(RequirementCost.ZeroCost, "SpeedBooster") : RequirementResult.Fail("SpeedBooster");

            case Requirement.Tech tech:
                if (HelperTechs.TryGetValue($"t_{tech.TechRequirement}", out var techRequirement))
                {
                    return HandleRequirement(techRequirement, state, inventory, world, weapons);
                }
                else
                {
                    return RequirementResult.Fail();
                }

            case Requirement.Notable notable:
                if (AllowedNotableStrategies.Contains(notable.NotableName))
                {
                    return RequirementResult.Success(RequirementCost.ZeroCost);
                }
                return RequirementResult.Fail(notable.NotableName);

            case Requirement.DoorUnlockedAtNode doorUnlockedAtNode:
                return state.HasDoorUnlocked(doorUnlockedAtNode.Node) ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail();

            case Requirement.SpeedBall speedBall:
                if (speedBall.Length < world.Config.LogicSkillConfigs[world.Config.Logic].SpeedballTiles)
                {
                    return RequirementResult.Fail();
                }

                return inventory.Has(world.GetItem("SpeedBooster")) ? RequirementResult.Success(RequirementCost.ZeroCost, "SpeedBooster") : RequirementResult.Fail("SpeedBooster");

            default:
                return RequirementResult.Fail();
        }
    }

    internal static (string Item, int Count) RequiredExpansion(
        string resourceType, int resourceCount) => resourceType switch
        {
            "Energy" or "RegularEnergy" =>
                ("ETank", Math.Max(0, (int)Math.Ceiling((resourceCount - 99) / 100m))),
            "ReserveEnergy" => ("ReserveTank", (int)Math.Ceiling(resourceCount / 100m)),
            "Missile" => ("Missile", (int)Math.Ceiling(resourceCount / 5m)),
            "Super" => ("Super", (int)Math.Ceiling(resourceCount / 5m)),
            "PowerBomb" => ("PowerBomb", (int)Math.Ceiling(resourceCount / 5m)),
            _ => ("", 0),
        };

    public static int ObstacleMaskFromArray(string[] obstacles)
    {
        int mask = 0;
        foreach (var obstacle in obstacles)
        {
            if (obstacle.Length != 1 || obstacle[0] < 'A' || obstacle[0] > 'Z')
                throw new ArgumentException($"Invalid obstacle name '{obstacle}'. Must be single char A-Z.");

            mask |= 1 << (obstacle[0] - 'A');
        }
        return mask;
    }

    private static EnemyDrops CalculatePerEnemyDropExpectation(Enemy enemy)
    {
        if (enemy.AmountOfDrops <= 0)
        {
            return ZeroEnemyDrops;
        }

        var perDropMultiplier = enemy.AmountOfDrops / DropRateDenominator;
        var dropRates = enemy.Drops;

        return new EnemyDrops(
            dropRates.NoDrop * perDropMultiplier,
            dropRates.SmallEnergy * perDropMultiplier,
            dropRates.BigEnergy * perDropMultiplier,
            dropRates.Missile * perDropMultiplier,
            dropRates.Super * perDropMultiplier,
            dropRates.PowerBomb * perDropMultiplier);
    }

    private EnemyDrops GetExpectedDrops(Drop drop)
    {
        if (drop.Count <= 0)
        {
            return ZeroEnemyDrops;
        }

        if (!EnemyDropExpectations.TryGetValue(drop.Enemy, out var perEnemyDrops))
        {
            perEnemyDrops = Enemies.TryGetValue(drop.Enemy, out var enemy)
                ? CalculatePerEnemyDropExpectation(enemy)
                : ZeroEnemyDrops;

            EnemyDropExpectations[drop.Enemy] = perEnemyDrops;
        }

        return ScaleDrops(perEnemyDrops, drop.Count);
    }

    private EnemyDrops GetExpectedDrops(IEnumerable<Drop> drops)
    {
        if (drops == null)
        {
            return ZeroEnemyDrops;
        }

        var total = ZeroEnemyDrops;
        foreach (var drop in drops)
        {
            total = AddDrops(total, GetExpectedDrops(drop));
        }

        return total;
    }

    private static EnemyDrops AddDrops(EnemyDrops first, EnemyDrops second) => new EnemyDrops(
        first.NoDrop + second.NoDrop,
        first.SmallEnergy + second.SmallEnergy,
        first.BigEnergy + second.BigEnergy,
        first.Missile + second.Missile,
        first.Super + second.Super,
        first.PowerBomb + second.PowerBomb);

    private static EnemyDrops ScaleDrops(EnemyDrops drops, decimal multiplier)
    {
        if (multiplier == 1m)
        {
            return drops;
        }

        return new EnemyDrops(
            drops.NoDrop * multiplier,
            drops.SmallEnergy * multiplier,
            drops.BigEnergy * multiplier,
            drops.Missile * multiplier,
            drops.Super * multiplier,
            drops.PowerBomb * multiplier);
    }

    private static RequirementCost GetResourceGainFromDrops(EnemyDrops drops)
    {
        var energyGain = (int)Math.Floor(drops.SmallEnergy * SmallEnergyDropValue + drops.BigEnergy * BigEnergyDropValue);
        var missileGain = (int)Math.Floor(drops.Missile * MissileDropValue);
        var superGain = (int)Math.Floor(drops.Super * SuperMissileDropValue);
        var powerBombGain = (int)Math.Floor(drops.PowerBomb * PowerBombDropValue);

        return new RequirementCost
        {
            Energy = energyGain,
            Missiles = missileGain,
            SuperMissiles = superGain,
            PowerBombs = powerBombGain
        };
    }

    private bool IsStrategyAllowed(NotableStrategy strategy, Logic logic) =>
        strategy.Difficulty switch
        {
            "Basic" => logic == Logic.Basic,
            "Medium" => logic == Logic.Basic || logic == Logic.Medium,
            "Hard" => logic == Logic.Basic || logic == Logic.Medium || logic == Logic.Hard,
            _ => false,
        };
}
