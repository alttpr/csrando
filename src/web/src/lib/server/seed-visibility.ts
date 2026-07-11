export function shouldHideSpoiler(options: unknown): boolean {
  if (!options || typeof options !== "object") {
    return false;
  }

  const request = options as { IncludeSpoiler?: unknown };
  return request.IncludeSpoiler === false;
}
