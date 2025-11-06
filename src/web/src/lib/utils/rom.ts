import { extractFirstFileByExtensions } from "./zip";

export const DEFAULT_ROM_EXTENSIONS = [".rom", ".sfc", ".smc", ".zip"] as const;

export function applyForcedHeaderBytes(
  buffer: ArrayBuffer,
  forcedHeaderBytes?: Uint8Array,
): ArrayBuffer {
  if (!forcedHeaderBytes || forcedHeaderBytes.length === 0) {
    return buffer;
  }
  if (buffer.byteLength < forcedHeaderBytes.length) {
    throw new Error(
      `Uploaded ROM is too small (${buffer.byteLength} bytes) for the required header length (${forcedHeaderBytes.length} bytes).`,
    );
  }
  const normalized = buffer.slice(0);
  const target = new Uint8Array(normalized);
  target.set(forcedHeaderBytes);
  return normalized;
}

export function normalizeRomExtensions(value: string | undefined): string[] {
  if (!value) return [...DEFAULT_ROM_EXTENSIONS];
  const normalized = value
    .split(",")
    .map((ext) => ext.trim().toLowerCase())
    .filter(Boolean);
  if (normalized.length === 0) return [...DEFAULT_ROM_EXTENSIONS];
  return Array.from(new Set(normalized));
}

export function extensionFromName(fileName: string): string {
  const normalized = fileName.toLowerCase().replace(/\\/g, "/");
  const base = normalized.split("/").pop() ?? normalized;
  const dotIndex = base.lastIndexOf(".");
  return dotIndex === -1 ? "" : base.slice(dotIndex);
}

export async function resolveUploadedRomBuffer(
  fileName: string,
  buffer: ArrayBuffer,
  allowedExtensions: string[],
  forcedHeaderBytes?: Uint8Array,
): Promise<{ buffer: ArrayBuffer; fileName: string }> {
  const normalized = allowedExtensions.map((ext) => ext.toLowerCase());
  const extension = extensionFromName(fileName);
  if (extension === ".zip") {
    const innerAllowed = normalized.filter((ext) => ext !== ".zip");
    if (innerAllowed.length === 0) {
      throw new Error("ZIP uploads are not supported for this game.");
    }
    const extracted = await extractFirstFileByExtensions(buffer, innerAllowed);
    return {
      buffer: applyForcedHeaderBytes(extracted.buffer, forcedHeaderBytes),
      fileName: extracted.fileName,
    };
  }
  if (!normalized.includes(extension)) {
    if (normalized.length === 0) {
      throw new Error("No supported ROM extensions are configured.");
    }
    throw new Error(
      `Unsupported file type. Expected one of: ${normalized.join(", ")}.`,
    );
  }
  return {
    buffer: applyForcedHeaderBytes(buffer, forcedHeaderBytes),
    fileName,
  };
}
