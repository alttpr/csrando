import { drizzle } from "drizzle-orm/better-sqlite3";
import Database from "better-sqlite3";
import * as schema from "./schema";
import { migrate } from "drizzle-orm/better-sqlite3/migrator";

// Simple Node-only DB initializer for CLI/tools (no SvelteKit $env/$app dependencies)
const databaseUrl = process.env.DATABASE_URL || "local.db";
const client = new Database(databaseUrl);
export const db = drizzle(client, { schema });

try {
  await migrate(db, { migrationsFolder: "drizzle" });
  console.log("[drizzle/node] migrations applied");
} catch (err) {
  console.error("[drizzle/node] migration failed:", err);
}
