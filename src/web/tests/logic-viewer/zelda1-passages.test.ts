import { describe, expect, it } from "vitest";

import { loadZeldaGraph } from "$lib/server/logic-viewer/zelda1";

describe("zelda1 stairs and passages", () => {
  it("connects stairs nodes to passage rooms", async () => {
    const graph = await loadZeldaGraph({ levelId: "level-01" });

    const stairs = graph.nodes.filter(
      (node) => node.metadata?.metaType === "Stairs",
    );

    expect(
      stairs.length,
      "expected stairs nodes to be present",
    ).toBeGreaterThan(0);

    for (const node of stairs) {
      const edges = graph.edges.filter(
        (edge) => edge.from === node.id || edge.to === node.id,
      );
      expect(
        edges.some((edge) => edge.from !== edge.to),
        `expected stairs node ${node.id} to connect to another room`,
      ).toBe(true);
      expect(
        (node.metadata?.stairsConnections as unknown[] | undefined)?.length ??
          0,
        `expected stairs node ${node.id} to include connection metadata`,
      ).toBeGreaterThan(0);
    }
  });

  it("includes overworld special items and start markers", async () => {
    const graph = await loadZeldaGraph({ levelId: "overworld" });

    const overworldItemNode = graph.nodes.find(
      (node) => node.metadata?.overworldItem === true,
    );
    expect(overworldItemNode, "expected overworld item node").toBeDefined();

    const armosItemNode = graph.nodes.find((node) =>
      (node.metadata?.specialCategories as unknown[] | undefined)?.includes(
        "armos-item",
      ),
    );
    expect(armosItemNode, "expected armos item node").toBeDefined();

    const startNode = graph.nodes.find((node) =>
      (node.metadata?.specialCategories as unknown[] | undefined)?.includes(
        "start",
      ),
    );
    expect(startNode, "expected overworld start node").toBeDefined();
  });
});
