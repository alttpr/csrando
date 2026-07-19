import {
  ApiError,
  createProfile,
  createProfileRevision,
  deleteProfile,
  duplicateProfile,
  fetchProfile,
  fetchProfiles,
  patchProfile,
  saveProfileFavorite,
  saveProfilePreferences,
} from "$lib/services/data";
import type {
  ProfileListResponseDto,
  ProfileRevisionDto,
  ProfileSummaryDto,
} from "$lib/schemas/profiles";
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
  recordRecentProfile,
  forgetProfile,
  type ConfigDraft,
} from "./profile-storage";

export type ProfileStatus = "custom" | "clean" | "modified";

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

// Client-side orchestration for the Seed Profile toolbar: profile lists,
// current selection, dirty baseline, and save/load flows. The config page
// keeps owning the form state; this class only hands hydrated form snapshots
// back to it.
export class ProfileState {
  readonly configId: string;
  isAuthenticated: boolean;

  officials = $state<ProfileSummaryDto[]>([]);
  mine = $state<ProfileSummaryDto[]>([]);
  favorites = $state<string[]>([]);
  defaultProfileId = $state<string | null>(null);
  recommendedId = $state<string | null>(null);

  selected = $state<ProfileSummaryDto | null>(null);
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

  applyList(list: ProfileListResponseDto): void {
    this.officials = list.officials;
    this.mine = list.mine;
    this.recommendedId = list.recommendedId;
    this.defaultProfileId = list.preferences?.defaultProfileId ?? null;
    this.favorites = (list.preferences?.favorites ?? []).map(
      (f) => f.profileId,
    );
  }

  async refreshList(): Promise<void> {
    try {
      this.applyList(await fetchProfiles(this.configId));
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
    }
  }

  allProfiles(): ProfileSummaryDto[] {
    return [...this.mine, ...this.officials];
  }

  findProfile(id: string): ProfileSummaryDto | null {
    return this.allProfiles().find((p) => p.id === id) ?? null;
  }

  status(currentNormalized: NormalizedConfig | null): ProfileStatus {
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

  // Load a profile revision and return the hydrated form for the page to
  // apply. Returns null when loading failed (error state is set).
  async select(
    profileId: string,
    metadata: Metadata,
    options: { revisionId?: string | null; recordUse?: boolean } = {},
  ): Promise<LoadedForm | null> {
    this.loading = true;
    this.error = null;
    this.conflict = false;
    try {
      const detail = await fetchProfile(profileId, options.revisionId);
      const migrated = this.migrateSettings(
        detail.revision.settings as NormalizedConfig,
        detail.revision.configSchemaVersion,
      );
      const { snapshot, loaded, report } = this.baselineFrom(
        migrated.settings,
        metadata,
      );

      this.selected = detail.profile;
      this.selectedRevisionId = detail.revision.id;
      this.loadedSnapshot = snapshot;
      const fullReport: LoadReport = {
        ...report,
        migrationSteps: migrated.steps,
      };
      this.loadReport = hasIssues(fullReport) ? fullReport : null;

      if (options.recordUse !== false) {
        this.recordUse(profileId);
      }
      return loaded;
    } catch (err) {
      if (err instanceof ConfigMigrationError) {
        this.error =
          "This profile was saved with a newer version of the randomizer and cannot be loaded here.";
      } else {
        this.error = err instanceof Error ? err.message : String(err);
        if (err instanceof ApiError && err.status === 404) {
          // Deleted in another session: drop it from local bookkeeping.
          forgetProfile(this.configId, profileId);
        }
      }
      return null;
    } finally {
      this.loading = false;
    }
  }

  // Restore an unsaved local draft. When the draft diverged from a known
  // profile revision, that revision stays the dirty baseline.
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

      if (draft.profileId) {
        try {
          const detail = await fetchProfile(
            draft.profileId,
            draft.profileRevisionId,
          );
          const baseMigrated = this.migrateSettings(
            detail.revision.settings as NormalizedConfig,
            detail.revision.configSchemaVersion,
          );
          const { snapshot } = this.baselineFrom(
            baseMigrated.settings,
            metadata,
          );
          this.selected = detail.profile;
          this.selectedRevisionId = detail.revision.id;
          this.loadedSnapshot = snapshot;
        } catch {
          // The base profile is gone; the draft continues as Custom.
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

  // Reset to "Custom configuration" (metadata defaults, no profile).
  clearSelection(): void {
    this.selected = null;
    this.selectedRevisionId = null;
    this.loadedSnapshot = null;
    this.loadReport = null;
    this.error = null;
    this.conflict = false;
  }

  private applySaved(
    profile: ProfileSummaryDto,
    revision: ProfileRevisionDto,
    metadata: Metadata,
  ): void {
    const { snapshot } = this.baselineFrom(
      revision.settings as NormalizedConfig,
      metadata,
    );
    this.selected = profile;
    this.selectedRevisionId = revision.id;
    this.loadedSnapshot = snapshot;
    this.conflict = false;
    const existing = this.mine.findIndex((p) => p.id === profile.id);
    if (profile.scope === "user") {
      if (existing >= 0) {
        this.mine[existing] = profile;
      } else {
        this.mine = [...this.mine, profile];
      }
    } else {
      const officialIndex = this.officials.findIndex(
        (p) => p.id === profile.id,
      );
      if (officialIndex >= 0) {
        this.officials[officialIndex] = profile;
      } else {
        this.officials = [...this.officials, profile];
      }
      if (profile.isRecommended) {
        this.recommendedId = profile.id;
        this.officials = this.officials.map((p) =>
          p.id === profile.id ? p : { ...p, isRecommended: false },
        );
      }
    }
    this.recordUse(profile.id);
  }

  // Save the current configuration as a new revision of the selected profile.
  async saveChanges(
    currentNormalized: NormalizedConfig,
    metadata: Metadata,
    changeSummary?: string,
  ): Promise<boolean> {
    if (!this.selected) return false;
    this.saving = true;
    this.error = null;
    try {
      const detail = await createProfileRevision(this.selected.id, {
        settings: currentNormalized,
        changeSummary,
        baseRevisionId: this.selectedRevisionId,
      });
      this.applySaved(detail.profile, detail.revision, metadata);
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

  // Save the current configuration as a brand-new profile (private, or an
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
      const detail = await createProfile({
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
      this.applySaved(detail.profile, detail.revision, metadata);
      if (input.setAsDefault) this.defaultProfileId = detail.profile.id;
      if (input.favorite && !this.favorites.includes(detail.profile.id)) {
        this.favorites = [...this.favorites, detail.profile.id];
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

  // Local last-used bookkeeping (recordRecentProfile also stamps the
  // logged-out last-used key) plus server-side preferences when signed in.
  recordUse(profileId: string): void {
    recordRecentProfile(this.configId, profileId);
    if (this.isAuthenticated) {
      saveProfilePreferences({ lastUsedProfileId: profileId }).catch(() => {
        // best-effort bookkeeping
      });
    }
  }

  async duplicate(
    profileId: string,
    metadata: Metadata,
    name?: string,
  ): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      const detail = await duplicateProfile(profileId, name);
      this.applySaved(detail.profile, detail.revision, metadata);
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  // Update profile metadata (name/description, plus curation fields on
  // official presets) and refresh local copies.
  async updateMeta(
    profileId: string,
    patch: Record<string, unknown>,
  ): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      const { profile } = await patchProfile(profileId, patch);
      const mineIndex = this.mine.findIndex((p) => p.id === profileId);
      if (mineIndex >= 0) this.mine[mineIndex] = profile;
      const officialIndex = this.officials.findIndex((p) => p.id === profileId);
      if (officialIndex >= 0) this.officials[officialIndex] = profile;
      if (this.selected?.id === profileId) this.selected = profile;
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  async remove(profileId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await deleteProfile(profileId);
      this.mine = this.mine.filter((p) => p.id !== profileId);
      forgetProfile(this.configId, profileId);
      if (this.defaultProfileId === profileId) this.defaultProfileId = null;
      this.favorites = this.favorites.filter((id) => id !== profileId);
      if (this.selected?.id === profileId) {
        // Keep the loaded settings on screen but detach the profile link.
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
  async archive(profileId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await patchProfile(profileId, { archived: true });
      await this.refreshList();
      if (this.selected?.id === profileId) {
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

  async setRecommended(profileId: string): Promise<boolean> {
    this.saving = true;
    this.error = null;
    try {
      await patchProfile(profileId, { isRecommended: true });
      await this.refreshList();
      return true;
    } catch (err) {
      this.error = err instanceof Error ? err.message : String(err);
      return false;
    } finally {
      this.saving = false;
    }
  }

  async setDefault(profileId: string | null): Promise<void> {
    const previous = this.defaultProfileId;
    this.defaultProfileId = profileId;
    try {
      await saveProfilePreferences({ defaultProfileId: profileId });
    } catch (err) {
      this.defaultProfileId = previous;
      this.error = err instanceof Error ? err.message : String(err);
    }
  }

  async togglePin(profileId: string): Promise<void> {
    const pinned = this.favorites.includes(profileId);
    this.favorites = pinned
      ? this.favorites.filter((id) => id !== profileId)
      : [...this.favorites, profileId];
    try {
      await saveProfileFavorite(profileId, !pinned);
    } catch (err) {
      // Roll back the optimistic update.
      this.favorites = pinned
        ? [...this.favorites, profileId]
        : this.favorites.filter((id) => id !== profileId);
      this.error = err instanceof Error ? err.message : String(err);
    }
  }
}
