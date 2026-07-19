import { json, error as svelteError } from "@sveltejs/kit";
import { z } from "zod";
import type { RequestHandler } from "./$types";
import { requireSessionUser } from "$lib/server/auth-guards";
import {
  countApiKeys,
  createApiKey,
  listApiKeys,
  MAX_API_KEYS_PER_USER,
  MAX_API_KEY_NAME_LENGTH,
} from "$lib/server/api-keys";

const CreateApiKeyRequestSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(MAX_API_KEY_NAME_LENGTH, "Name is too long"),
});

export const GET: RequestHandler = async ({ locals }) => {
  const user = requireSessionUser(locals);
  return json({ keys: await listApiKeys(user.id) });
};

export const POST: RequestHandler = async ({ request, locals }) => {
  const user = requireSessionUser(locals);

  let body: unknown;
  try {
    body = await request.json();
  } catch {
    throw svelteError(400, { message: "Invalid request body" });
  }

  const parsed = CreateApiKeyRequestSchema.safeParse(body);
  if (!parsed.success) {
    throw svelteError(400, {
      message: "Fix the highlighted errors and try again.",
      fieldErrors: { name: parsed.error.issues[0]?.message ?? "Invalid name" },
    });
  }

  if ((await countApiKeys(user.id)) >= MAX_API_KEYS_PER_USER) {
    throw svelteError(400, {
      message: `You have reached the maximum of ${MAX_API_KEYS_PER_USER} API keys. Revoke one to create another.`,
    });
  }

  // The plain secret is returned exactly once and never stored.
  const { key, secret } = await createApiKey(user.id, parsed.data.name);
  return json({ key, secret }, { status: 201 });
};
