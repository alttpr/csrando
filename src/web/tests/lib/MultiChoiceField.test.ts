import { render, waitFor } from "@testing-library/svelte";
import { describe, expect, it } from "vitest";
import MultiChoiceField from "$lib/components/config/fields/MultiChoiceField.svelte";

const items = {
  "0": "0",
  "1": "1",
  "2": "2",
  Dungeons: "Dungeons",
};

describe("MultiChoiceField", () => {
  it("preserves an auxiliary choice subset loaded from a preset", async () => {
    const { getByLabelText } = render(MultiChoiceField, {
      props: {
        items,
        selected: ["0", "2"],
        isOptionsFor: true,
      },
    });

    await waitFor(() => {
      expect((getByLabelText("0") as HTMLInputElement).checked).toBe(true);
      expect((getByLabelText("1") as HTMLInputElement).checked).toBe(false);
      expect((getByLabelText("2") as HTMLInputElement).checked).toBe(true);
      expect((getByLabelText("Dungeons") as HTMLInputElement).checked).toBe(
        false,
      );
    });
  });

  it("enables every auxiliary choice when no selection exists", async () => {
    const { getByLabelText } = render(MultiChoiceField, {
      props: {
        items,
        selected: [],
        isOptionsFor: true,
      },
    });

    await waitFor(() => {
      for (const label of Object.keys(items)) {
        expect((getByLabelText(label) as HTMLInputElement).checked).toBe(true);
      }
    });
  });
});
