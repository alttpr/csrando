import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { metadataApi } from "$lib/services/api";

export const GET: RequestHandler = async () => {
  const metadata = await metadataApi.getAll();
  return json(metadata);
};
