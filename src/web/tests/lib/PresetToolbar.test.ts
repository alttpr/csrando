import {
  fireEvent,
  render,
  waitFor,
  within,
  type RenderResult,
} from "@testing-library/svelte";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { tick } from "svelte";
import type { SvelteComponent } from "svelte";
import PresetToolbarHarness from "./helpers/PresetToolbarHarness.svelte";
import { PresetState } from "$lib/config/preset-state.svelte";
import { normalizeConfig, type NormalizedConfig } from "$lib/config/normalize";
import { CONFIG_SCHEMA_VERSION } from "$lib/config/constants";
import type { PresetRevisionDto, PresetSummaryDto } from "$lib/schemas/presets";
import {
  defaultFormStateFixture,
  loadMetadataFixture,
} from "../fixtures/metadata";

vi.mock("$app/navigation", () => ({ goto: vi.fn() }));

// ---- In-memory fake presets backend behind global fetch ----

interface StoredPreset {
  summary: PresetSummaryDto;
  revisions: PresetRevisionDto[];
}

const store = new Map<string, StoredPreset>();
let idCounter = 0;

function nextId(prefix: string): string {
  return `${prefix}-${++idCounter}`;
}

function makePreset(
  name: string,
  scope: "official" | "user",
  settings: NormalizedConfig,
  extras: Partial<PresetSummaryDto> = {},
): StoredPreset {
  const presetId = nextId("preset");
  const revisionId = nextId("revision");
  const revision: PresetRevisionDto = {
    id: revisionId,
    presetId,
    revisionNumber: 1,
    configSchemaVersion: CONFIG_SCHEMA_VERSION,
    settings,
    changeSummary: null,
    createdAt: new Date().toISOString(),
  };
  const summary: PresetSummaryDto = {
    id: presetId,
    scope,
    slug: scope === "official" ? name.toLowerCase() : null,
    configId: "combo",
    name,
    description: scope === "official" ? "The recommended defaults" : null,
    currentRevisionId: revisionId,
    revisionNumber: 1,
    configSchemaVersion: CONFIG_SCHEMA_VERSION,
    selectedGames: settings.selectedGames,
    gameTags: null,
    difficultyTag: scope === "official" ? "Beginner" : null,
    isRecommended: scope === "official",
    featured: false,
    archived: false,
    displayOrder: 0,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    ...extras,
  };
  const stored = { summary, revisions: [revision] };
  store.set(presetId, stored);
  return stored;
}

function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json" },
  });
}

async function fakeFetch(
  input: RequestInfo | URL,
  init?: RequestInit,
): Promise<Response> {
  const url = new URL(String(input), "http://localhost");
  const method = (init?.method ?? "GET").toUpperCase();
  const body = init?.body ? JSON.parse(String(init.body)) : null;

  if (url.pathname === "/api/user/preset-preferences" && method === "PUT") {
    return json({ preferences: body });
  }
  if (url.pathname === "/api/user/preset-favorites" && method === "PUT") {
    return json({ ok: true });
  }

  if (url.pathname === "/api/presets" && method === "GET") {
    const all = [...store.values()].map((p) => p.summary);
    return json({
      officials: all.filter((p) => p.scope === "official"),
      mine: all.filter((p) => p.scope === "user"),
      preferences: {
        defaultPresetId: null,
        lastUsedPresetId: null,
        favorites: [],
      },
      recommendedId: all.find((p) => p.isRecommended)?.id ?? null,
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
    });
  }

  if (url.pathname === "/api/presets" && method === "POST") {
    const duplicate = [...store.values()].some(
      (p) =>
        p.summary.scope === "user" &&
        p.summary.name.toLowerCase() === String(body.name).toLowerCase(),
    );
    if (duplicate) {
      return json(
        {
          message: "Fix the highlighted errors and try again.",
          fieldErrors: { name: "You already have a preset with this name" },
        },
        400,
      );
    }
    const scope = body.scope === "official" ? "official" : "user";
    const created = makePreset(body.name, scope, body.settings, {
      description: body.description ?? null,
      slug: scope === "official" ? (body.slug ?? null) : null,
      isRecommended: scope === "official" && !!body.isRecommended,
    });
    return json(
      { preset: created.summary, revision: created.revisions[0] },
      201,
    );
  }

  const detailMatch = url.pathname.match(/^\/api\/presets\/([^/]+)$/);
  if (detailMatch && method === "PATCH") {
    const stored = store.get(detailMatch[1]);
    if (!stored) return json({ message: "Preset not found" }, 404);
    stored.summary = { ...stored.summary, ...body };
    return json({ preset: stored.summary });
  }
  if (detailMatch && method === "GET") {
    const stored = store.get(detailMatch[1]);
    if (!stored) return json({ message: "Preset not found" }, 404);
    const revisionId =
      url.searchParams.get("revision") ?? stored.summary.currentRevisionId;
    const revision = stored.revisions.find((r) => r.id === revisionId);
    if (!revision) return json({ message: "Preset revision not found" }, 404);
    return json({ preset: stored.summary, revision });
  }

  const revisionsMatch = url.pathname.match(
    /^\/api\/presets\/([^/]+)\/revisions$/,
  );
  if (revisionsMatch && method === "POST") {
    const stored = store.get(revisionsMatch[1]);
    if (!stored) return json({ message: "Preset not found" }, 404);
    if ((body.baseRevisionId ?? null) !== stored.summary.currentRevisionId) {
      return json({ message: "This preset was updated elsewhere." }, 409);
    }
    const revision: PresetRevisionDto = {
      id: nextId("revision"),
      presetId: stored.summary.id,
      revisionNumber: stored.revisions.length + 1,
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
      settings: body.settings,
      changeSummary: body.changeSummary ?? null,
      createdAt: new Date().toISOString(),
    };
    stored.revisions.push(revision);
    stored.summary = {
      ...stored.summary,
      currentRevisionId: revision.id,
      revisionNumber: revision.revisionNumber,
      selectedGames: (body.settings as NormalizedConfig).selectedGames,
    };
    return json({ preset: stored.summary, revision }, 201);
  }

  return json({ message: `Unhandled ${method} ${url.pathname}` }, 500);
}

// ---- Test setup ----

const metadata = loadMetadataFixture();

function defaultSettings(): NormalizedConfig {
  return normalizeConfig(defaultFormStateFixture(metadata), metadata);
}

function setup(
  options: { isAuthenticated?: boolean; isAdmin?: boolean } = {},
): {
  view: RenderResult<SvelteComponent>;
  state: PresetState;
  official: StoredPreset;
} {
  const official = makePreset("Recommended", "official", defaultSettings());
  const state = new PresetState("combo", options.isAuthenticated ?? true);
  state.applyList({
    officials: [official.summary],
    mine: [],
    preferences: {
      defaultPresetId: null,
      lastUsedPresetId: null,
      favorites: [],
    },
    recommendedId: official.summary.id,
    configSchemaVersion: CONFIG_SCHEMA_VERSION,
  });
  const view = render(PresetToolbarHarness, {
    props: {
      state,
      metadata,
      isAuthenticated: options.isAuthenticated ?? true,
      isAdmin: options.isAdmin ?? false,
    },
  }) as unknown as { view: never } & RenderResult<SvelteComponent>;
  return { view, state, official };
}

async function selectPresetByName(
  view: RenderResult<SvelteComponent>,
  name: string,
) {
  const input = view.getByTestId("preset-combobox-input");
  await fireEvent.click(input);
  const listbox = await waitFor(() =>
    view.getByTestId("preset-combobox-listbox"),
  );
  const option = within(listbox)
    .getAllByRole("option")
    .find((el) => el.textContent?.includes(name));
  expect(option).toBeTruthy();
  await fireEvent.click(option!);
  await tick();
}

beforeEach(() => {
  store.clear();
  idCounter = 0;
  localStorage.clear();
  vi.stubGlobal("fetch", vi.fn(fakeFetch));
  Element.prototype.scrollIntoView = vi.fn();
});

describe("PresetToolbar end-to-end scenario", () => {
  it("copies official preset links using the readable slug", async () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, "clipboard", {
      configurable: true,
      value: { writeText },
    });
    const { view } = setup();

    await selectPresetByName(view, "Recommended");
    await fireEvent.click(view.getByTestId("preset-overflow-button"));
    await fireEvent.click(
      view.getByRole("menuitem", { name: "Copy share link" }),
    );

    await waitFor(() => {
      expect(writeText).toHaveBeenCalledWith(
        `${window.location.origin}/config/combo/recommended`,
      );
    });
  });

  it("loads a preset, tracks modifications, saves, updates and switches presets", async () => {
    const { view, state } = setup();

    // 1. Initial state: custom configuration.
    expect(view.getByTestId("preset-badges").textContent).toContain("Custom");

    // 2. Load the official preset from the combobox.
    await selectPresetByName(view, "Recommended");
    await waitFor(() => {
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      );
    });
    expect(view.getByTestId("preset-badges").textContent).not.toContain(
      "Modified",
    );
    expect(view.getByTestId("harness-swords").textContent).toBe("Randomized");

    // 3. Modify a setting: Modified status appears; official offers
    //    "Save as my preset".
    await fireEvent.click(view.getByTestId("harness-modify"));
    await waitFor(() => {
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Modified",
      );
    });
    expect(view.getByTestId("preset-save-as-mine")).toBeTruthy();

    // The closed combobox itself flags the divergence so the preset name is
    // never mistaken for the unmodified preset.
    expect(
      (view.getByTestId("preset-combobox-input") as HTMLInputElement).value,
    ).toBe("Recommended (modified)");

    // 4. Save it as a user preset through the dialog.
    await fireEvent.click(view.getByTestId("preset-save-as-mine"));
    const nameInput = await waitFor(() => view.getByLabelText("Name"));
    await fireEvent.input(nameInput, { target: { value: "My tweaks" } });
    await fireEvent.click(view.getByRole("button", { name: "Save" }));
    await waitFor(() => {
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "My preset",
      );
    });
    expect(view.getByTestId("preset-badges").textContent).not.toContain(
      "Modified",
    );
    expect(state.selected?.name).toBe("My tweaks");
    expect(
      (view.getByTestId("preset-combobox-input") as HTMLInputElement).value,
    ).toBe("My tweaks");
    const savedPresetId = state.selected!.id;

    // The stored revision holds the modified setting, and the seed-preset
    // baseline points at it.
    const storedSettings = store.get(savedPresetId)!.revisions[0]
      .settings as NormalizedConfig;
    expect(storedSettings.perGame.Alttpr.Swords).toBe("Assured");
    expect(state.selectedRevisionId).toBe(
      store.get(savedPresetId)!.summary.currentRevisionId,
    );

    // 5. The saved preset appears in the selector under "My presets".
    const input = view.getByTestId("preset-combobox-input");
    await fireEvent.click(input);
    const listbox = await waitFor(() =>
      view.getByTestId("preset-combobox-listbox"),
    );
    expect(listbox.textContent).toContain("My presets");
    expect(
      within(listbox)
        .getAllByRole("option")
        .some((el) => el.textContent?.includes("My tweaks")),
    ).toBe(true);
    await fireEvent.keyDown(input, { key: "Escape" });

    // 6. Re-applying the same value keeps the preset clean: dirty state is
    //    a semantic comparison, not an edit counter.
    await fireEvent.click(view.getByTestId("harness-modify"));
    await tick();
    expect(view.getByTestId("preset-badges").textContent).not.toContain(
      "Modified",
    );
  });

  it("moves pinned presets to a dedicated top group without duplicates", async () => {
    const { view, state, official } = setup();

    const input = view.getByTestId("preset-combobox-input");
    await fireEvent.click(input);
    let listbox = await waitFor(() =>
      view.getByTestId("preset-combobox-listbox"),
    );
    expect(listbox.textContent).not.toContain("Pinned");
    expect(listbox.textContent).toContain("Official presets");

    await fireEvent.click(within(listbox).getByLabelText("Pin Recommended"));
    await tick();

    expect(state.favorites).toContain(official.summary.id);
    listbox = view.getByTestId("preset-combobox-listbox");
    expect(listbox.textContent).toContain("Pinned");
    // The preset appears exactly once, under Pinned — not also in its
    // scope group (that group disappears when its only member is pinned).
    expect(
      within(listbox)
        .getAllByRole("option")
        .filter((el) => el.textContent?.includes("Recommended")),
    ).toHaveLength(1);
    expect(listbox.textContent).not.toContain("Official presets");
  });

  it("saves changes to an owned preset as a new revision", async () => {
    const { view, state } = setup();
    const mine = makePreset("Owned", "user", defaultSettings(), {
      isRecommended: false,
    });
    state.applyList({
      officials: [],
      mine: [mine.summary],
      preferences: {
        defaultPresetId: null,
        lastUsedPresetId: null,
        favorites: [],
      },
      recommendedId: null,
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
    });

    await selectPresetByName(view, "Owned");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "My preset",
      ),
    );

    await fireEvent.click(view.getByTestId("harness-modify"));
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Modified",
      ),
    );

    await fireEvent.click(view.getByTestId("preset-save-changes"));
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).not.toContain(
        "Modified",
      ),
    );
    expect(store.get(mine.summary.id)!.revisions).toHaveLength(2);
    expect(
      (store.get(mine.summary.id)!.revisions[1].settings as NormalizedConfig)
        .perGame.Alttpr.Swords,
    ).toBe("Assured");
  });

  it("guards preset switches while modified and supports discard", async () => {
    const { view, official } = setup();

    await selectPresetByName(view, "Recommended");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      ),
    );
    await fireEvent.click(view.getByTestId("harness-modify"));
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Modified",
      ),
    );
    expect(view.getByTestId("harness-swords").textContent).toBe("Assured");

    // Attempt to switch back to the same official preset while modified:
    // the unsaved-changes dialog must appear.
    await selectPresetByName(view, "Recommended");
    const dialog = await waitFor(() => view.getByRole("dialog"));
    expect(dialog.textContent).toContain("unsaved changes");

    // Cancel keeps the modifications.
    await fireEvent.click(
      within(dialog).getByRole("button", { name: "Cancel" }),
    );
    expect(view.getByTestId("harness-swords").textContent).toBe("Assured");
    expect(view.getByTestId("preset-badges").textContent).toContain("Modified");

    // Discard and switch reloads the clean preset settings.
    await selectPresetByName(view, "Recommended");
    const dialog2 = await waitFor(() => view.getByRole("dialog"));
    await fireEvent.click(
      within(dialog2).getByRole("button", { name: "Discard and switch" }),
    );
    await waitFor(() =>
      expect(view.getByTestId("harness-swords").textContent).toBe("Randomized"),
    );
    expect(view.getByTestId("preset-badges").textContent).not.toContain(
      "Modified",
    );
    void official;
  });

  it("shows a validation error inside the save dialog and stays modified", async () => {
    const { view } = setup();
    // Existing user preset with a conflicting name.
    makePreset("Taken", "user", defaultSettings(), { isRecommended: false });

    await selectPresetByName(view, "Recommended");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      ),
    );
    await fireEvent.click(view.getByTestId("harness-modify"));
    await waitFor(() => view.getByTestId("preset-save-as-mine"));
    await fireEvent.click(view.getByTestId("preset-save-as-mine"));

    const nameInput = await waitFor(() => view.getByLabelText("Name"));
    await fireEvent.input(nameInput, { target: { value: "Taken" } });
    await fireEvent.click(view.getByRole("button", { name: "Save" }));

    await waitFor(() => {
      expect(
        view.getByText("You already have a preset with this name"),
      ).toBeTruthy();
    });
    // Dialog stayed open, configuration still modified.
    expect(view.getByRole("dialog")).toBeTruthy();
    await fireEvent.click(view.getByRole("button", { name: "Cancel" }));
    expect(view.getByTestId("preset-badges").textContent).toContain("Modified");
    expect(view.getByTestId("harness-swords").textContent).toBe("Assured");
  });

  it("lets an admin create an official preset from the save dialog", async () => {
    const { view, state } = setup({ isAdmin: true });

    // Custom configuration -> Save as new preset.
    await fireEvent.click(view.getByTestId("preset-save-as-new"));
    const nameInput = await waitFor(() => view.getByLabelText("Name"));
    await fireEvent.input(nameInput, { target: { value: "Weekly Race" } });

    // The admin-only official option is present; enable it and fill the slug.
    const officialToggle = view.getByLabelText(
      "Save as official preset (visible to everyone)",
    );
    await fireEvent.click(officialToggle);
    const slugInput = await waitFor(() =>
      view.getByLabelText("Stable ID (lowercase letters, digits, dashes)"),
    );
    await fireEvent.input(slugInput, { target: { value: "weekly-race" } });
    await fireEvent.click(
      view.getByLabelText("Make this the recommended preset"),
    );
    await fireEvent.click(view.getByRole("button", { name: "Save" }));

    await waitFor(() => {
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      );
    });
    expect(state.selected?.scope).toBe("official");
    expect(state.selected?.slug).toBe("weekly-race");
    expect(state.recommendedId).toBe(state.selected?.id);

    // It joins the Official presets group in the selector.
    const input = view.getByTestId("preset-combobox-input");
    await fireEvent.click(input);
    const listbox = await waitFor(() =>
      view.getByTestId("preset-combobox-listbox"),
    );
    expect(
      within(listbox)
        .getAllByRole("option")
        .some((el) => el.textContent?.includes("Weekly Race")),
    ).toBe(true);
  });

  it("lets an admin edit an official preset's description and tags", async () => {
    const { view, state, official } = setup({ isAdmin: true });

    await selectPresetByName(view, "Recommended");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      ),
    );

    // Open the overflow menu and pick "Edit details".
    await fireEvent.click(view.getByTestId("preset-overflow-button"));
    const menu = await waitFor(() => view.getByTestId("preset-overflow-menu"));
    await fireEvent.click(
      within(menu).getByRole("menuitem", { name: "Edit details" }),
    );

    const dialog = await waitFor(() => view.getByRole("dialog"));
    // Curation fields are present for admins editing officials.
    const description = within(dialog).getByLabelText("Description (optional)");
    await fireEvent.input(description, {
      target: { value: "Curated weekly settings" },
    });
    await fireEvent.input(
      within(dialog).getByLabelText("Difficulty/audience (optional)"),
      { target: { value: "Expert" } },
    );
    await fireEvent.input(
      within(dialog).getByLabelText("Tags (comma separated, optional)"),
      { target: { value: "race, weekly" } },
    );
    await fireEvent.click(within(dialog).getByRole("button", { name: "Save" }));

    await waitFor(() => {
      expect(state.selected?.description).toBe("Curated weekly settings");
    });
    expect(state.selected?.difficultyTag).toBe("Expert");
    expect(state.selected?.gameTags).toEqual(["race", "weekly"]);

    // The difficulty and tag badges are visible in the toolbar row.
    const tags = view.getByTestId("preset-tags");
    expect(tags.textContent).toContain("Expert");
    expect(tags.textContent).toContain("race");
    expect(tags.textContent).toContain("weekly");
    expect(store.get(official.summary.id)!.summary.difficultyTag).toBe(
      "Expert",
    );
    // The officials list reflects the change too.
    expect(
      state.officials.find((p) => p.id === official.summary.id)?.difficultyTag,
    ).toBe("Expert");
  });

  it("does not offer curation fields when a user edits their own preset", async () => {
    const { view, state } = setup();
    const mine = makePreset("Owned", "user", defaultSettings(), {
      isRecommended: false,
    });
    state.applyList({
      officials: [],
      mine: [mine.summary],
      preferences: {
        defaultPresetId: null,
        lastUsedPresetId: null,
        favorites: [],
      },
      recommendedId: null,
      configSchemaVersion: CONFIG_SCHEMA_VERSION,
    });

    await selectPresetByName(view, "Owned");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "My preset",
      ),
    );
    await fireEvent.click(view.getByTestId("preset-overflow-button"));
    const menu = await waitFor(() => view.getByTestId("preset-overflow-menu"));
    await fireEvent.click(
      within(menu).getByRole("menuitem", { name: "Edit details" }),
    );
    const dialog = await waitFor(() => view.getByRole("dialog"));
    // Description is editable; curation fields are not offered.
    expect(
      within(dialog).getByLabelText("Description (optional)"),
    ).toBeTruthy();
    expect(
      within(dialog).queryByLabelText("Difficulty/audience (optional)"),
    ).toBeNull();
  });

  it("does not offer the official option to regular users", async () => {
    const { view } = setup();
    await fireEvent.click(view.getByTestId("preset-save-as-new"));
    await waitFor(() => view.getByLabelText("Name"));
    expect(
      view.queryByLabelText("Save as official preset (visible to everyone)"),
    ).toBeNull();
  });

  it("offers login instead of saving when logged out", async () => {
    const { view } = setup({ isAuthenticated: false });
    await selectPresetByName(view, "Recommended");
    await waitFor(() =>
      expect(view.getByTestId("preset-badges").textContent).toContain(
        "Official",
      ),
    );
    await fireEvent.click(view.getByTestId("harness-modify"));
    await waitFor(() => {
      expect(
        view.getByRole("link", { name: "Log in to save presets" }),
      ).toBeTruthy();
    });
    expect(view.queryByTestId("preset-save-as-mine")).toBeNull();
  });
});
