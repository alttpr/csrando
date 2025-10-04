import { drizzle } from "drizzle-orm/better-sqlite3";
import Database from "better-sqlite3";
import * as schema from "./schema";
import { env } from "$env/dynamic/private";
import { migrate } from "drizzle-orm/better-sqlite3/migrator";
import { building } from "$app/environment";

const databaseUrl = env.DATABASE_URL || "local.db";
const client = new Database(databaseUrl);
export const db = drizzle(client, { schema });

// Skip migrations during build to avoid running DB logic at compile time.
if (!building) {
  try {
    await migrate(db, { migrationsFolder: "drizzle" });
    console.log("[drizzle] migrations applied");
  } catch (err) {
    console.error("[drizzle] migration failed:", err);
    // Do not crash the app in production if a race or transient file issue occurs.
  }
}
