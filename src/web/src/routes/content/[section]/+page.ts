import { browser } from "$app/environment";
import { error } from "@sveltejs/kit";
import type { ComponentType } from "svelte";
import type { PageLoad } from "./$types";

const DEFAULT_LANG = "en";

type MarkdownMetadata = {
  title?: string;
  description?: string;
  [key: string]: unknown;
};

type MarkdownModule = {
  default: ComponentType;
  metadata?: MarkdownMetadata;
};

export const load: PageLoad = async ({ params }) => {
  const { section } = params;

  if (!section) {
    throw error(404, "Content section not specified");
  }

  const lang = browser
    ? new Intl.Locale(navigator.language).language
    : DEFAULT_LANG;

  const docs = import.meta.glob<MarkdownModule>("$lib/content/*.svx");
  const pathFor = (candidate: string) => `/src/lib/content/${candidate}.svx`;
  const loaderEntry = [
    pathFor(`${section}.${lang}`),
    pathFor(`${section}.${DEFAULT_LANG}`),
  ]
    .map((key) => {
      if (!Object.prototype.hasOwnProperty.call(docs, key)) {
        return null;
      }
      const loader = docs[key];
      return { key, loader };
    })
    .find(
      (
        entry,
      ): entry is { key: string; loader: () => Promise<MarkdownModule> } =>
        Boolean(entry),
    );

  if (!loaderEntry) {
    throw error(404, "Requested content not found");
  }

  try {
    const document = await loaderEntry.loader();
    return {
      content: document.default,
      contentKey: loaderEntry.key,
      metadata: document.metadata ?? null,
      section,
    };
  } catch (cause) {
    console.error("Failed to load markdown content", cause);
    throw error(500, "Failed to load content");
  }
};
