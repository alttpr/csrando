/**
 * Shared helpers for resolving game/randomizer aliases.
 * These are used by both frontend display helpers and backend API clients
 * to keep canonical IDs consistent.
 */

type AliasGroup = readonly string[];

const RANDOMIZER_ALIAS_GROUPS: Record<string, AliasGroup> = Object.freeze({
  Alttpr: ["alttpr", "alttp", "zelda3", "z3"],
  Z1R: ["z1r", "z1", "zelda1"],
  G2R: ["g2r", "g2", "goonies2"],
});

const normalize = (value: string): string => value.trim().toLowerCase();

/**
 * Returns the canonical randomizer identifier for the given alias, if known.
 */
export function canonicalRandomizerId(id: string): string | undefined {
  const normalized = normalize(id);
  for (const [canonical, aliases] of Object.entries(RANDOMIZER_ALIAS_GROUPS)) {
    if (aliases.includes(normalized)) return canonical;
  }
  return undefined;
}

/**
 * Returns the list of lowercase alias candidates associated with the given id.
 * The original id (lowercased) is always included in the result.
 */
export function candidateGameIds(id: string): string[] {
  const normalized = normalize(id);
  const canonical = canonicalRandomizerId(id);
  if (!canonical) return [normalized];

  const aliases = RANDOMIZER_ALIAS_GROUPS[canonical];
  const unique = new Set<string>([normalized, ...aliases]);
  return Array.from(unique);
}

/**
 * Expose the alias groups for callers that need to iterate canonical entries.
 */
export const RANDOMIZER_ALIAS_GROUPS_READONLY = RANDOMIZER_ALIAS_GROUPS;
