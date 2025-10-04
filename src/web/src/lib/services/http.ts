export async function fetchWithTimeout(
  input: RequestInfo | URL,
  init?: RequestInit & { timeoutMs?: number },
) {
  const { timeoutMs = 15000, ...rest } = init || {};
  const controller = new AbortController();
  const to = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const resp = await fetch(input, { ...rest, signal: controller.signal });
    return resp;
  } finally {
    clearTimeout(to);
  }
}
