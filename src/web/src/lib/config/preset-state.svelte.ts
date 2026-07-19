import {
  ApiError,
  createPreset,
  createPresetRevision,
  deletePreset,
  duplicatePreset,
  fetchPreset,
  fetchPresets,
  patchPreset,
  savePresetFavorite,
  savePresetPreferences,
} from "$lib/services/data";
import type {
  PresetListResponseDto,
  PresetRevisionDto,
  PresetSummaryDto,
} from "$lib/schemas/presets";
import type { Metadata } from "$lib/types";
import {
  configsEqual,
  hydrateFormState,
  normalizeConfig,
  type FormStateSnapshot,
  type HydrationReport,
  type NormalizedConfig,
} from "./normalize";
import { migrateConfig, ConfigMigrationError } from "./migrations";
import {
  recordRecentPreset,
  forgetPreset,
  type ConfigDraft,
} from "./preset-storage";

export type PresetStatus = "custom" | "clean" | "modified";

export interface LoadReport extends HydrationReport {
  migrationSteps: string[];
}

export interface LoadedForm {
  form: FormStateSnapshot;
  activeTab: string | null;
}

function hasIssues(report: LoadReport): boolean {
  return (
    report.removedKeys.length > 0 ||
    report.resetKeys.length > 0 ||
    report.migrationSteps.length > 0
  );
}

// Client-side orchestration for the Seed Preset toolbar: preset lists,
// current selection, dirty baseline, and save/load flows. The config page
// keeps owning the form state; this class only hands hydrated form snapshots
// back to it.
export class PresetState {
  readonly configId: string;
  isAuthenticated: boolean;

  officials = $state<PresetSummaryDto[]>([]);
  mine = $state<PresetSummaryDto[]>([]);
  favorites = $state<string[]>([]);
  defaultPresetId = $state<string | null>(null);
  recommendedId = $state<string | null>(null);

  selected = $state<PresetSummaryDto | null>(null);
  // Selected revision (the dirty-state baseline refers to it).
  selectedRevisionId = $state<string | null>(null);
  loadedSnapshot = $state<NormalizedConfig | null>(null);

  loading = $state(false);
  saving = $state(false);
  error = $state<string | null>(null);
  // A save hit a newer revision on the server (409).
  conflict = $state(false);
  loadReport = $state<LoadReport | null>(null);

  constructor(configId: string, isAuthenticated: boolean) {
    this.configId = configId;
    this.isAuthenticated = isAuthenticated;
  }

  applyList(list: PresetListResponseDto): void {
    this.officials = list.officials;
    this.mine = list.mine;
    this.recommendedId = list.recommendedId;
    this.defaultPresetId = list.preferences?.defaultPresetId ?? null;
    this.favorites = (list.preferences?.favorites ?? []).map((f) => f.presetId);
  }

  async refreshList(): Promise<void> {
    try {
      this.applyList(await fetchPresets(this.configId));
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
    }
  }

  allPresets(): PresetSummaryDto[] {
    return [...this.mine, ...this.officials];
  }

  findPreset(id: string): PresetSummaryDto | null {
    return this.allPresets().find((p) => p.id === id) ?? null;
  }

  status(currentNormalized: NormalizedConfig | null): PresetStatus {
    if (!this.selected || !this.loadedSnapshot) return "custom";
    if (!currentNormalized) return "clean";
    return configsEqual(currentNormalized, this.loadedSnapshot)
      ? "clean"
      : "modified";
  }

  // Rebuild the clean baseline from stored revision settings using the
  // page's metadata, so dirty comparison and live normalization agree.
  private baselineFrom(
    settings: NormalizedConfig,
    metadata: Metadata,
  ): {
    snapshot: NormalizedConfig;
    loaded: LoadedForm;
    report: HydrationReport;
  } {
    const { form, activeTab, report } = hydrateFormState(settings, metadata);
    return {
      snapshot: normalizeConfig(form, metadata),
      loaded: { form, activeTab },
      report,
    };
  }

  private migrateSettings(
    settings: NormalizedConfig,
    fromVersion: number,
  ): { settings: NormalizedConfig; steps: string[] } {
    const result = migrateConfig(settings, fromVersion);
    return { settings: result.settings, steps: result.appliedSteps };
  }

  // Load a preset revision and return the hydrated form for the page to
  // apply. Returns null when loading failed (error state is set).
  async select(
    presetId: string,
    metadata: Metadata,
    options: { revisionId?: string | null; recordUse?: boolean } = {},
  ): Promise<LoadedForm | null> {
    this.loading = true;
    this.error = null;
    this.conflict = false;
    try {
      const detail = await fetchPreset(presetId, options.revisionId);
      const migrated = this.migrateSettings(
        detail.revision.settings as NormalizedConfig,
        detail.revision.configSchemaVersion,
      );
      const { snapshot, loaded, report } = this.baselineFrom(
        migrated.settings,
        metadata,
      );

      this.selected = detail.preset;
      this.selectedRevisionId = detail.revision.id;
      this.loadedSnapshot = snapshot;
      const fullReport: LoadReport = {
        ...report,
        migrationSteps: migrated.steps,
      };
      this.loadReport = hasIssues(fullReport) ? fullReport : null;

      if (options.recordUse !== false) {
        this.recordUse(presetId);
      }
      return loaded;
    } catch (err) {
      if (err instanceof ConfigMigrationError) {
        this.error =
          "This preset was saved with a newer version of the randomizer and cannot be loaded here.";
      } else {
        this.error = err instanceof Error ? err.message : String(err);
        if (err instanceof ApiError && err.status === 404) {
          // Deleted in another session: drop it from local bookkeeping.
          forgetPreset(this.configId, presetId);
        }
      }
      return null;
    } finally {
      this.loading = false;
    }
  }

  // Restore an unsaved local draft. When the draft diverged from a known
  // preset revision, that revision stays the dirty baseline.
  async applyDraft(
    draft: ConfigDraft,
    metadata: Metadata,
  ): Promise<LoadedForm | null> {
    this.loading = true;
    this.error = null;
    try {
      let migratedDraft: NormalizedConfig;
      let steps: string[];
      try {
        const migrated = this.migrateSettings(
          draft.settings,
          draft.configSchemaVersion,
        );
        migratedDraft = migrated.settings;
        steps = migrated.steps;
      } catch {
        // Unmigratable draft: nothing to restore.
        return null;
      }

      if (draft.presetId) {
        try {
          const detail = await fetchPreset(
            draft.presetId,
            draft.presetRevisionId,
          );
          const baseMigrated = this.migrateSettings(
            detail.revision.settings as NormalizedConfig,
            detail.revision.configSchemaVersion,
          );
          const { snapshot } = this.baselineFrom(
            baseMigrated.settings,
            metadata,
          );
          this.selected = detail.preset;
          this.selectedRevisionId = detail.revision.id;
          this.loadedSnapshot = snapshot;
        } catch {
          // The base preset is gone; the draft continues as Custom.
          this.selected = null;
          this.selectedRevisionId = null;
          this.loadedSnapshot = null;
        }
      } else {
        this.selected = null;
        this.selectedRevisionId = null;
        this.loadedSnapshot = null;
      }

      const { form, activeTab, report } = hydrateFormState(
        migratedDraft,
        metadata,
      );
      const fullReport: LoadReport = { ...report, migrationSteps: steps };
      this.loadReport = hasIssues(fullReport) ? fullReport : null;
      return { form, activeTab };
    } finally {
      this.loading = false;
    }
  }

  // Apply settings received through a share link. The recipient gets an
  // unowned copy: selection becomes "Custom configuration".
  applySharedSettings(
    settings: NormalizedConfig,
    fromVersion: number,
    metadata: Metadata,
  ): LoadedForm | null {
    this.clearSelection();
    let migrated: { settings: NormalizedConfig; steps: string[] };
    try {
      migrated = this.migrateSettings(settings, fromVersion);
    } catch {
      this.error =
        "This shared configuration was made with a newer version of the randomizer and cannot be loaded here.";
      return null;
    }
    const { form, activeTab, report } = hydrateFormState(
      migrated.settings,
      metadata,
    );
    const fullReport: LoadReport = {
      ...report,
      migrationSteps: migrated.steps,
    };
    this.loadReport = hasIssues(fullReport) ? fullReport : null;
    return { form, activeTab };
  }

  // Reset to "Custom configuration" (metadata defaults, no preset).
  clearSelection(): void {
    this.selected = null;
    this.selectedRevisionId = null;
    this.loadedSnapshot = null;
    this.loadReport = null;
    this.error = null;
    this.conflict = false;
  }

  private applySaved(
    preset: PresetSummaryDto,
    revision: PresetRevisionDto,
    metadata: Metadata,
  ): void {
    const { snapshot } = this.baselineFrom(
      revision.settings as NormalizedConfig,
      metadata,
    );
    this.selected = preset;
    this.selectedRevisionId = revision.id;
    this.loadedSnapshot = snapshot;
    this.conflict = false;
    const existing = this.mine.findIndex((p) => p.id === preset.id);
    if (preset.scope === "user") {
      if (existing >= 0) {
        this.mine[existing] = preset;
      } else {
        this.mine = [...this.mine, preset];
      }
    } else {
      const officialIndex = this.officials.findIndex((p) => p.id === preset.id);
      if (officialIndex >= 0) {
        this.officials[officialIndex] = preset;
      } else {
        this.officials = [...this.officials, preset];
      }
      if (preset.isRecommended) {
        this.recommendedId = preset.id;
        this.officials = this.officials.map((p) =>
          p.id === preset.id ? p : { ...p, isRecommended: false },
        );
      }
    }
    this.recordUse(preset.id);
  }

  // Save the current configuration as a new revision of the selected preset.
  async saveChanges(
    currentNormalized: NormalizedConfig,
    metadata: Metadata,
    changeSummary?: string,
  ): Promise<boolean> {
    if (!this.selected) return false;
    this.saving = true;
    this.error = null;
    try {
      const detail = await createPresetRevision(this.selected.id, {
        settings: currentNormalized,
        changeSummary,
        baseRevisionId: this.selectedRevisionId,
      });
      this.applySaved(detail.preset, detail.revision, metadata);
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      if (err instanceof ApiError && err.status === 409) {
        this.conflict = true;
      }
      return false;
    } finally {
      this.saving = false;
    }
  }

  // Save the current configuration as a brand-new preset (private, or an
  // official preset when an admin requests it).
  async saveAsNew(
    input: {
      name: string;
      description?: string;
      setAsDefault?: boolean;
      favorite?: boolean;
      scope?: "official" | "user";
      slug?: string;
      isRecommended?: boolean;
    },
    currentNormalized: NormalizedConfig,
    metadata: Metadata,
  ): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      const detail = await createPreset({
        configId: this.configId,
        name: input.name,
        description: input.description,
        settings: currentNormalized,
        setAsDefault: input.setAsDefault,
        favorite: input.favorite,
        scope: input.scope,
        slug: input.slug,
        isRecommended: input.isRecommended,
      });
      this.applySaved(detail.preset, detail.revision, metadata);
      if (input.setAsDefault) this.defaultPresetId = detail.preset.id;
      if (input.favorite && !this.favorites.includes(detail.preset.id)) {
        this.favorites = [...this.favorites, detail.preset.id];
      }
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      throw err;
    } finally {
      this.saving = false;
    }
  }

  // Rebuild the form from the loaded revision (Revert).
  revert(metadata: Metadata): LoadedForm | null {
    if (!this.loadedSnapshot) return null;
    const { form, activeTab } = hydrateFormState(this.loadedSnapshot, metadata);
    this.error = null;
    this.conflict = false;
    return { form, activeTab };
  }

  // Local last-used bookkeeping (recordRecentPreset also stamps the
  // logged-out last-used key) plus server-side preferences when signed in.
  recordUse(presetId: string): void {
    recordRecentPreset(this.configId, presetId);
    if (this.isAuthenticated) {
      savePresetPreferences({ lastUsedPresetId: presetId }).catch(() => {
        // best-effort bookkeeping
      });
    }
  }

  async duplicate(
    presetId: string,
    metadata: Metadata,
    name?: string,
  ): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      const detail = await duplicatePreset(presetId, name);
      this.applySaved(detail.preset, detail.revision, metadata);
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  // Update preset metadata (name/description, plus curation fields on
  // official presets) and refresh local copies.
  async updateMeta(
    presetId: string,
    patch: Record<string, unknown>,
  ): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      const { preset } = await patchPreset(presetId, patch);
      const mineIndex = this.mine.findIndex((p) => p.id === presetId);
      if (mineIndex >= 0) this.mine[mineIndex] = preset;
      const officialIndex = this.officials.findIndex((p) => p.id === presetId);
      if (officialIndex >= 0) this.officials[officialIndex] = preset;
      if (this.selected?.id === presetId) this.selected = preset;
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  async remove(presetId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await deletePreset(presetId);
      this.mine = this.mine.filter((p) => p.id !== presetId);
      forgetPreset(this.configId, presetId);
      if (this.defaultPresetId === presetId) this.defaultPresetId = null;
      this.favorites = this.favorites.filter((id) => id !== presetId);
      if (this.selected?.id === presetId) {
        // Keep the loaded settings on screen but detach the preset link.
        this.selected = null;
        this.selectedRevisionId = null;
        this.loadedSnapshot = null;
      }
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  // Admin-only curation actions on official presets.
  async archive(presetId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await patchPreset(presetId, { archived: true });
      await this.refreshList();
      if (this.selected?.id === presetId) {
        this.selected = { ...this.selected, archived: true };
      }
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  async setRecommended(presetId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await patchPreset(presetId, { isRecommended: true });
      await this.refreshList();
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  async setDefault(presetId: string | null): Promise<void> {
    const previous = this.defaultPresetId;
    this.defaultPresetId = presetId;
    try {
      await savePresetPreferences({ defaultPresetId: presetId });
    } catch (err) {
      this.defaultPresetId = previous;
      this.error = err instanceof Error ? err.message : String(err);
    }
  }

  async togglePin(presetId: string): Promise<void> {
    const pinned = this.favorites.includes(presetId);
    this.favorites = pinned
      ? this.favorites.filter((id) => id !== presetId)
      : [...this.favorites, presetId];
    try {
      await savePresetFavorite(presetId, !pinned);
    } catch (err) {
      // Roll back the optimistic update.
      this.favorites = pinned
        ? [...this.favorites, presetId]
        : this.favorites.filter((id) => id !== presetId);
      this.error = err instanceof Error ? err.message : String(err);
    }
  }
}
