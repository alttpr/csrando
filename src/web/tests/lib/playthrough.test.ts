import { describe, expect, it } from "vitest";
import {
  condensedPlaythroughPath,
  parsePlaythrough,
  visiblePlaythroughPickups,
  type PlaythroughData,
  type PlaythroughPathStep,
} from "$lib/utils/playthrough";

describe("parsePlaythrough", () => {
  it("parses legacy and current route requirement fields", () => {
    const morph = { name: "Morph", game: "m1", world: 0, count: 1 };
    const missile = { name: "Missile", game: "m1", world: 0, count: 1 };
    const vertex = { name: "Morph Room", game: "m1", world: 0, type: "Item" };
    const result = parsePlaythrough(
      JSON.stringify({
        complete: true,
        victoryItems: ["DefeatedSilverTwo"],
        startingItems: [],
        warnings: [],
        spheres: [
          {
            number: 0,
            pickups: [
              {
                location: vertex,
                item: morph,
                requiredItems: [],
                path: [
                  { from: vertex, to: vertex, requirement: morph },
                  { from: vertex, to: vertex, requirements: [morph, missile] },
                ],
              },
            ],
          },
        ],
      }),
    );

    expect(result?.spheres[0].pickups[0].path[0].requirement?.name).toBe(
      "Morph",
    );
    expect(
      result?.spheres[0].pickups[0].path[1].requirements?.map(
        (requirement) => requirement.name,
      ),
    ).toEqual(["Morph", "Missile"]);
  });

  it("keeps strategy-only steps exclusive to the full route", () => {
    const vertex = (name: string) => ({
      name,
      game: "m1",
      world: 0,
      type: "Region",
    });
    const path: PlaythroughPathStep[] = [
      { from: vertex("Start"), to: vertex("Hall") },
      {
        from: vertex("Hall"),
        to: vertex("Tunnel"),
        requirement: { name: "Morph", game: "m1", world: 0, count: 1 },
      },
      { from: vertex("Tunnel"), to: vertex("Shaft") },
      {
        from: vertex("Shaft"),
        to: vertex("Item"),
        strategy: "Ice clip",
      },
      {
        from: vertex("Item"),
        to: vertex("Heated room"),
        strategy: "Base",
        resourcesSpent: [{ name: "Energy", amount: 30 }],
      },
    ];

    expect(condensedPlaythroughPath(path).map((step) => step.to.name)).toEqual([
      "Tunnel",
      "Heated room",
    ]);
    expect(
      condensedPlaythroughPath(path, true).map((step) => step.to.name),
    ).toEqual(["Tunnel", "Heated room"]);
  });

  it("hides initial logic settings from the default route", () => {
    const vertex = (name: string) => ({
      name,
      game: "sm",
      world: 0,
      type: "Meta",
    });
    const path: PlaythroughPathStep[] = [
      {
        from: vertex("Start"),
        to: vertex("Map clear"),
        requirements: [
          {
            name: "ConfigWorldLogicNormal",
            game: "sm",
            world: 0,
            count: 1,
            meta: true,
            initial: true,
          },
        ],
      },
    ];

    expect(condensedPlaythroughPath(path)).toEqual([]);
    expect(condensedPlaythroughPath(path, true)).toEqual(path);
  });

  it("keeps earned progression flags when they are consumed", () => {
    const vertex = (name: string) => ({
      name,
      game: "alttp",
      world: 0,
      type: "Meta",
    });
    const path: PlaythroughPathStep[] = [
      {
        from: vertex("Boss room"),
        to: vertex("Prize"),
        requirements: [
          {
            name: "DefeatHelmasaur",
            game: "alttp",
            world: 0,
            count: 1,
            meta: true,
            initial: false,
            simple: true,
          },
        ],
      },
    ];

    expect(condensedPlaythroughPath(path)).toEqual(path);
  });

  it("hides internal traversal flags from the default route", () => {
    const vertex = (name: string) => ({
      name,
      game: "alttp",
      world: 0,
      type: "Meta",
    });
    const path: PlaythroughPathStep[] = ["LostKiki", "LightHole"].map(
      (name) => ({
        from: vertex("Internal state"),
        to: vertex("Destination"),
        requirements: [
          {
            name,
            game: "alttp",
            world: 0,
            count: 1,
            meta: true,
            initial: false,
            simple: false,
          },
        ],
      }),
    );

    expect(condensedPlaythroughPath(path)).toEqual([]);
    expect(condensedPlaythroughPath(path, true)).toEqual(path);
  });

  it("keeps earned interaction capabilities in the default route", () => {
    const vertex = (name: string) => ({
      name,
      game: "alttp",
      world: 0,
      type: "Meta",
    });
    const path: PlaythroughPathStep[] = ["UseBomb", "OpenChest"].map(
      (name) => ({
        from: vertex("Approach"),
        to: vertex(name),
        requirements: [
          {
            name,
            game: "alttp",
            world: 0,
            count: 1,
            meta: true,
            initial: false,
            simple: true,
          },
        ],
      }),
    );

    expect(condensedPlaythroughPath(path)).toEqual(path);
  });

  it("keeps cross-game travel in the condensed route", () => {
    const vertex = (name: string, game: string) => ({
      name,
      game,
      world: 0,
      type: "Region",
    });
    const path: PlaythroughPathStep[] = [
      {
        from: vertex("Pyramid portal", "alttp"),
        to: vertex("Landing Site portal", "sm"),
        game: "alttp",
        crossGame: true,
      },
    ];

    expect(condensedPlaythroughPath(path)).toEqual(path);
  });

  it("keeps victory flags in the simplified pickup list", () => {
    const vertex = {
      name: "Goal",
      game: "sm",
      world: 0,
      type: "Meta",
    };
    const pickup = (name: string) => ({
      location: vertex,
      item: { name, game: "sm", world: 0, count: 1, meta: true },
      requiredItems: [],
      path: [],
      meta: true,
    });
    const victory = pickup("f_DefeatedMotherBrain");
    const intermediate = pickup("f_TourianOpen");
    const playthrough: PlaythroughData = {
      complete: true,
      victoryItems: [victory.item.name],
      startingItems: [],
      spheres: [],
      warnings: [],
    };

    expect(
      visiblePlaythroughPickups(playthrough, [intermediate, victory]),
    ).toEqual([victory]);
    expect(
      visiblePlaythroughPickups(playthrough, [intermediate, victory], true),
    ).toEqual([intermediate, victory]);
  });

  it("shows unused classified items only in the detailed pickup list", () => {
    const location = {
      name: "Kakariko Well",
      game: "alttp",
      world: 0,
      type: "Chest",
    };
    const shovel = {
      location,
      item: { name: "Shovel", game: "alttp", world: 0, count: 1 },
      requiredItems: [],
      path: [],
      meta: false,
      required: false,
    };
    const playthrough: PlaythroughData = {
      complete: true,
      victoryItems: ["Triforce"],
      startingItems: [],
      spheres: [],
      warnings: [],
    };

    expect(visiblePlaythroughPickups(playthrough, [shovel])).toEqual([]);
    expect(visiblePlaythroughPickups(playthrough, [shovel], true)).toEqual([
      shovel,
    ]);
  });

  it.each([undefined, "not json", "{}", '{"complete":true,"spheres":[]}'])(
    "rejects malformed data",
    (value) => expect(parsePlaythrough(value)).toBeNull(),
  );
});
