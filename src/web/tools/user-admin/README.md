# User Admin Tool

A command-line tool for managing user roles in the csrando web application.

## Usage

### Set a user as admin

```bash
npm run user:admin <username>
```

Example:

```bash
npm run user:admin john_doe
```

### Set a user back to regular user role

```bash
npm run user:set-role set-user <username>
```

Example:

```bash
npm run user:set-role set-user john_doe
```

### List all users and their roles

```bash
npm run user:list
```

### Custom database path

All commands support a custom database path with the `--database` or `--db` flag:

```bash
npm run user:admin john_doe -- --database ./custom.db
npm run user:list -- --db ./custom.db
```

## Advanced Usage

You can run the tool directly with npx tsx for more control:

```bash
# Set admin
npx tsx tools/user-admin/index.ts set-admin <username>

# Set regular user
npx tsx tools/user-admin/index.ts set-user <username>

# List users
npx tsx tools/user-admin/index.ts list

# Show help
npx tsx tools/user-admin/index.ts --help
```

## Environment Variables

The tool respects the `DATABASE_URL` environment variable. If not set, it defaults to `./local.db`.

## Examples

```bash
# Make alice an admin
npm run user:admin alice
# ✅ Successfully set role for user "alice" to "admin"

# List all users
npm run user:list
# Users:
# ────────────────────────────────────────────────────────────
# 👑 alice               Role: admin    ID: usr_abc123
# 👤 bob                 Role: user     ID: usr_def456
# ────────────────────────────────────────────────────────────

# Remove admin from alice
npm run user:set-role set-user alice
# ✅ Successfully set role for user "alice" to "user"
```
