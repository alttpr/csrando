import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import { metadataApi } from "$lib/services/api";

export const GET: RequestHandler = async ({ params }) => {
  const { id } = params;
  const metadata = await metadataApi.getById(id);
  return json(metadata);
};
