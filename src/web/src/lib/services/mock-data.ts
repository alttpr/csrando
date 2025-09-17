import { readFileSync } from "fs";
import { join } from "path";
import type { Metadata } from "$lib/types";
import { parseMetadata } from "$lib/schemas/metadata";

/**
 * Mock data service for local testing mode
 * Serves static metadata files without requiring a backend
 */
class MockDataService {
  private mockMetadata: Map<string, Metadata> = new Map();

  constructor() {
    this.loadMockData();
  }

  /**
   * Load mock metadata files from the project root
   */
  private loadMockData() {
    try {
      // Load the example Zelda 3 metadata
      const settingsPath = join(process.cwd(), "settings_zelda3.json");
      const rawData = readFileSync(settingsPath, "utf-8");
      const parsedData = JSON.parse(rawData);

      const parsed = parseMetadata(parsedData);
      if (parsed.success) {
        this.mockMetadata.set("alttp", parsed.data);
        // Also set up aliases for different ID formats
        this.mockMetadata.set("zelda3", parsed.data);
        this.mockMetadata.set("alttpr", parsed.data); // Common abbreviation for A Link to the Past Randomizer
      } else {
        console.warn("Failed to parse mock metadata for zelda3:", parsed.error);
      }
    } catch (error) {
      console.warn("Failed to load mock metadata files:", error);
    }
  }

  /**
   * Get all available metadata (returns array of all loaded metadata)
   */
  getAllMetadata(): Metadata[] {
    return Array.from(this.mockMetadata.values());
  }

  /**
   * Get metadata by ID
   */
  getMetadataById(id: string): Metadata | null {
    return this.mockMetadata.get(id) || null;
  }

  /**
   * Get available game IDs
   */
  getAvailableGameIds(): string[] {
    return Array.from(this.mockMetadata.keys());
  }

  /**
   * Add or update mock metadata for a game ID
   */
  addMockMetadata(id: string, metadata: Metadata): void {
    this.mockMetadata.set(id, metadata);
  }
}

export const mockDataService = new MockDataService();
