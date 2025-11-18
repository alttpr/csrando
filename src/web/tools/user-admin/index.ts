#!/usr/bin/env node
import yargs from "yargs";
import { hideBin } from "yargs/helpers";
import Database from "better-sqlite3";
import { drizzle } from "drizzle-orm/better-sqlite3";
import { eq } from "drizzle-orm";
import { users } from "../../src/lib/server/db/schema.js";

interface SetRoleArgs {
  username: string;
  role: "admin" | "user";
  database?: string;
}

async function setUserRole(args: SetRoleArgs) {
  const dbPath = args.database || process.env.DATABASE_URL || "./local.db";

  console.log(`Connecting to database: ${dbPath}`);
  const sqlite = new Database(dbPath);
  const db = drizzle(sqlite);

  try {
    const result = await db
      .update(users)
      .set({ role: args.role as "admin" | "user" })
      .where(eq(users.username, args.username))
      .returning();

    if (result.length === 0) {
      console.error(`❌ User "${args.username}" not found`);
      process.exit(1);
    }

    console.log(
      `✅ Successfully set role for user "${args.username}" to "${args.role}"`,
    );
    console.log(`User ID: ${result[0].id}`);
  } catch (error) {
    console.error("❌ Error updating user role:", error);
    process.exit(1);
  } finally {
    sqlite.close();
  }
}

async function listUsers(args: { database?: string }) {
  const dbPath = args.database || process.env.DATABASE_URL || "./local.db";

  console.log(`Connecting to database: ${dbPath}`);
  const sqlite = new Database(dbPath);
  const db = drizzle(sqlite);

  try {
    const allUsers = await db
      .select({
        id: users.id,
        username: users.username,
        role: users.role,
      })
      .from(users);

    if (allUsers.length === 0) {
      console.log("No users found in database");
      return;
    }

    console.log("\nUsers:");
    console.log("─".repeat(60));
    allUsers.forEach((user) => {
      const roleEmoji = user.role === "admin" ? "👑" : "👤";
      console.log(
        `${roleEmoji} ${user.username.padEnd(20)} Role: ${user.role.padEnd(8)} ID: ${user.id}`,
      );
    });
    console.log("─".repeat(60));
  } catch (error) {
    console.error("❌ Error listing users:", error);
    process.exit(1);
  } finally {
    sqlite.close();
  }
}

yargs(hideBin(process.argv))
  .command(
    "set-admin <username>",
    "Set a user's role to admin",
    (yargs) => {
      return yargs
        .positional("username", {
          describe: "Username to set as admin",
          type: "string",
          demandOption: true,
        })
        .option("database", {
          alias: "db",
          describe: "Path to SQLite database file",
          type: "string",
        });
    },
    (argv) => {
      setUserRole({
        username: argv.username as string,
        role: "admin",
        database: argv.database,
      });
    },
  )
  .command(
    "set-user <username>",
    "Set a user's role to regular user",
    (yargs) => {
      return yargs
        .positional("username", {
          describe: "Username to set as regular user",
          type: "string",
          demandOption: true,
        })
        .option("database", {
          alias: "db",
          describe: "Path to SQLite database file",
          type: "string",
        });
    },
    (argv) => {
      setUserRole({
        username: argv.username as string,
        role: "user",
        database: argv.database,
      });
    },
  )
  .command(
    "list",
    "List all users and their roles",
    (yargs) => {
      return yargs.option("database", {
        alias: "db",
        describe: "Path to SQLite database file",
        type: "string",
      });
    },
    (argv) => {
      listUsers({ database: argv.database });
    },
  )
  .demandCommand(1, "You must provide a command")
  .help()
  .alias("help", "h")
  .parse();
