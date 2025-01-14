namespace Randomizer.Games.SuperMetroid;

using Randomizer.Games.SuperMetroid.Model;
using Randomizer.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RequirementResult
{
    public bool Met { get; set; }
    public RequirementCost? Cost { get; set; }
    public HashSet<string>? Missing { get; set; }

    public static RequirementResult Fail(params string[] missing) => new RequirementResult { Met = false, Cost = null, Missing = missing.Length > 0 ? new HashSet<string>(missing) : null };
    public static RequirementResult Success(RequirementCost cost) => new RequirementResult { Met = true, Cost = cost };

    public void MergeFail(RequirementResult other)
    {
        if(other.Missing == null || other.Missing.Count == 0)
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
        var weightedAmmoB = b.Missiles + b.SuperMissiles * 3 + a.PowerBombs * 4;

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
    private static Dictionary<string, Requirement> HelperTechs = new Dictionary<string, Requirement>();
    private static Dictionary<string, Enemy> Enemies = new Dictionary<string, Enemy>();

    public static void Initialize(JsonReader reader, World world, List<string> allowedTechs)
    {
        var preprocessor = new GraphPreprocessor(reader, world, allowedTechs);

        foreach (var helper in reader.Helpers.HelperCategories.SelectMany(h => h.Helpers))
        {
            HelperTechs[helper.Name] = preprocessor.OptimizeRequirement(helper.Requires);
        }

        foreach(var tech in reader.Techs.TechCategories.SelectMany(t => t.Techs))
        {
            AddTech(tech, allowedTechs, preprocessor);
            AddTech(tech, allowedTechs, preprocessor, true);
        }

        foreach(var enemy in reader.Enemies.SelectMany(e => e.Enemies))
        {
            Enemies[enemy.Name] = enemy;
        }
    }

    private static void AddTech(Tech tech, List<string> allowedTechs, GraphPreprocessor preprocessor, bool techOnly = false)
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

    public static RequirementResult HandleRequirement(Requirement req, VisitedState state, Inventory inventory, World world)
    {
        switch (req)
        {
            case Requirement.Always:
                return RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.Never:
                return RequirementResult.Fail("Never");

            case Requirement.Single single:
                if(HelperTechs.TryGetValue(single.Req, out var helper))
                {
                    return HandleRequirement(helper, state, inventory, world);
                }
                else
                {
                    if (inventory.Has(world.GetItem(single.Req)))
                        return RequirementResult.Success(RequirementCost.ZeroCost);
                    else
                        return RequirementResult.Fail(single.Req);
                }

            case Requirement.And and:
                var totalCost = RequirementCost.ZeroCost;
                var failResult = RequirementResult.Fail();
                bool failedAnd = false;
                // We'll store success by default, and if we find a fail, we’ll flip it.

                foreach (var subReq in and.Reqs)
                {
                    var subResult = HandleRequirement(subReq, state, inventory, world);

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
                    }
                }

                if (failedAnd)
                {
                    return failResult;
                }
                else
                {
                    return RequirementResult.Success(totalCost);
                }

            case Requirement.Or or:
                RequirementResult? bestSuccess = null;
                // Collect missing items from all failing sub-requirements
                var combinedFail = RequirementResult.Fail();

                foreach (var subReq in or.Reqs)
                {
                    var subResult = HandleRequirement(subReq, state, inventory, world);
                    if (subResult.Met)
                    {
                        if (bestSuccess == null)
                        {
                            bestSuccess = subResult;
                        }
                        else
                        {
                            var newCost = bestSuccess.Cost! | subResult.Cost!;
                            bestSuccess = RequirementResult.Success(newCost.Value);
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
                return HandleRequirement(not.Req, state, inventory, world) == null ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail();

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
                    });
                } else
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

            case Requirement.EnemyKill enemyKill:
                return RequirementResult.Success(RequirementCost.ZeroCost);

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
                if (!Enemies.TryGetValue(enemyDamage.Enemy, out var enemy))
                {
                    return RequirementResult.Fail("never");
                }
                
                var attack = enemy.Attacks.Where(a => a.Name == enemyDamage.Type).FirstOrDefault();
                if (attack == null)
                {
                    return RequirementResult.Fail("never");
                }

                var attackDamage = attack.BaseDamage * enemyDamage.Hits;
                if(attack.AffectedByVaria ?? true == true && inventory.Has(world.GetItem("Varia")))
                {
                    attackDamage /= 2;
                } else if (attack.AffectedByGravity ?? true == true && inventory.Has(world.GetItem("Gravity")))
                {
                    attackDamage /= 4;
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = attackDamage,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.HeatFrames heatFrames:
                var hasVaria = inventory.Has(world.GetItem("Varia"));
                var canHellrun = HelperTechs.ContainsKey("canHeatRun");

                if(!hasVaria && !canHellrun)
                {
                    return RequirementResult.Fail("Varia");
                }

                return RequirementResult.Success(new RequirementCost
                {
                    Energy = hasVaria ? 0 : heatFrames.Frames / 4,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            // TODO: Add drops into this
            case Requirement.HeatFramesWithEnergyDrops heatFramesWithEnergyDrops:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = (inventory.Has(world.GetItem("Varia")) ? 0 : heatFramesWithEnergyDrops.Frames / 4),
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

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

                    if(!hasResource)
                    {
                        failed = true;
                        if (resourceName != "")
                        {
                            failedResource.MergeFail(RequirementResult.Fail(resourceName));
                        }
                    }
                }

                if(failed)
                {
                    return failedResource;
                } 
                else
                {
                    return RequirementResult.Success(RequirementCost.ZeroCost);
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
                    Energy = hasVariaG ? 0 : gravitylessHeatFrames.Frames / 4,
                    Missiles = 0,
                    SuperMissiles = 0,
                    PowerBombs = 0
                });

            case Requirement.GravitylessLavaFrames gravitylessLavaFrames:
                return RequirementResult.Success(new RequirementCost
                {
                    Energy = inventory.Has(world.GetItem("Varia")) ? 0 : gravitylessLavaFrames.Frames / 4,
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
                    if(state.Energy - requiredEnergy <= 29)
                    {
                        return RequirementResult.Fail("ETank");
                    }

                    return RequirementResult.Success(new RequirementCost
                    {
                        Energy = shinespark.Frames,
                        Missiles = 0,
                        SuperMissiles = 0,
                        PowerBombs = 0
                    });
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
                    if (!inventory.HasAtLeast(world.GetItem(c.Type), c.Count))
                    {
                        failedCapacity.MergeFail(RequirementResult.Fail(c.Type));
                        failedCap = true;
                    }
                }

                return failedCap ? failedCapacity : RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.CanShineCharge canShineCharge:
                return inventory.Has(world.GetItem("SpeedBooster")) && canShineCharge.UsedTiles >= 25 ? RequirementResult.Success(RequirementCost.ZeroCost) : (canShineCharge.UsedTiles < 25 ? RequirementResult.Fail() : RequirementResult.Fail("SpeedBooster")); 

            case Requirement.GetBlueSpeed blueSpeed:
                return inventory.Has(world.GetItem("SpeedBooster")) ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail("SpeedBooster");

            case Requirement.Tech tech:
                if (HelperTechs.TryGetValue($"t_{tech.TechRequirement}", out var techRequirement))
                {
                    return HandleRequirement(techRequirement, state, inventory, world);
                }
                else
                {
                    return RequirementResult.Fail();
                }

            case Requirement.Notable notable:
                return RequirementResult.Success(RequirementCost.ZeroCost);

            case Requirement.DoorUnlockedAtNode doorUnlockedAtNode:
                return state.HasDoorUnlocked(doorUnlockedAtNode.Node) ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail();

            case Requirement.SpeedBall speedBall:
                if(speedBall.Length < 30)
                {
                    return RequirementResult.Fail();
                }

                return inventory.Has(world.GetItem("SpeedBooster")) ? RequirementResult.Success(RequirementCost.ZeroCost) : RequirementResult.Fail("SpeedBooster");

            default:
                return RequirementResult.Fail();
        }
    }

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
}
