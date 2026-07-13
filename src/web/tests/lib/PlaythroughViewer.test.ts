import { fireEvent, render, within } from "@testing-library/svelte";
import { describe, expect, it } from "vitest";
import PlaythroughViewer from "$lib/components/seed/PlaythroughViewer.svelte";
import type { PlaythroughData } from "$lib/utils/playthrough";

const vertex = (name: string, world = 0) => ({
  name,
  game: "sm",
  world,
  type: "Region",
});

const playthrough: PlaythroughData = {
  complete: true,
  victoryItems: ["Victory"],
  startingItems: [],
  warnings: [],
  spheres: [0, 1].map((number) => ({
    number,
    pickups: [
      {
        location: vertex(`Location ${number + 1}`),
        item: {
          name: `Item ${number + 1}`,
          game: "sm",
          world: 0,
          count: 1,
        },
        requiredItems: [],
        path: [
          {
            from: vertex(`Start ${number + 1}`),
            to: vertex(`Location ${number + 1}`),
          },
        ],
        required: true,
      },
      ...(number === 1
        ? [
            {
              location: vertex("Location 2", 1),
              item: {
                name: "Item 2",
                game: "sm",
                world: 1,
                count: 1,
              },
              requiredItems: [],
              path: [
                {
                  from: vertex("Start 2", 1),
                  to: vertex("Location 2", 1),
                },
              ],
              required: false,
            },
          ]
        : []),
    ],
  })),
};

describe("PlaythroughViewer", () => {
  it("only shows a status warning for incomplete playthroughs", async () => {
    const { queryByText, rerender } = render(PlaythroughViewer, {
      playthrough,
    });

    expect(queryByText("Complete")).toBeNull();
    expect(queryByText("Incomplete")).toBeNull();

    await rerender({ playthrough: { ...playthrough, complete: false } });

    expect(queryByText("Incomplete")).not.toBeNull();
  });

  it("expands later-sphere routes when advanced pickups share names", async () => {
    const { getAllByText, getByRole, getByText } = render(PlaythroughViewer, {
      playthrough,
    });

    await fireEvent.click(getByText("Logical playthrough"));
    await fireEvent.click(
      getByRole("checkbox", {
        name: "Show logic events, flags, and full routes",
      }),
    );

    const pickupSummary = getAllByText("Item 2")[0].closest("summary");
    expect(pickupSummary).not.toBeNull();
    await fireEvent.click(pickupSummary!);

    const pickupDetails = pickupSummary!.closest("details");
    expect(pickupDetails?.open).toBe(true);
    const fullRouteSummary = within(pickupDetails!).getByText(
      "Full route (1 edges)",
    );
    await fireEvent.click(fullRouteSummary);

    expect(fullRouteSummary.closest("details")?.open).toBe(true);
  });
});
