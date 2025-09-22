// Minimal ZIP reading utilities for browser environments
// Supports plain (store) and deflate entries; no ZIP64, no encryption.

export type ZipCentralDirectoryEntry = {
    fileName: string;
    compressionMethod: number;
    compressedSize: number;
    uncompressedSize: number;
    localHeaderOffset: number;
    generalPurposeFlag: number;
};

const ZIP_EOCD_SIGNATURE = 0x06054b50;
const ZIP_CENTRAL_DIR_SIGNATURE = 0x02014b50;
const ZIP_LOCAL_FILE_HEADER_SIGNATURE = 0x04034b50;

const zipUtf8Decoder = new TextDecoder("utf-8", { fatal: false });

function findZipEndOfCentralDirectory(bytes: Uint8Array): number {
    // Search backwards for EOCD signature
    for (let i = bytes.length - 22; i >= 0; i--) {
        if (
            bytes[i] === 0x50 &&
            bytes[i + 1] === 0x4b &&
            bytes[i + 2] === 0x05 &&
            bytes[i + 3] === 0x06
        ) {
            return i;
        }
    }
    throw new Error("ZIP archive is missing the end of central directory record.");
}

function decodeZipFileName(bytes: Uint8Array, isUtf8: boolean): string {
    if (isUtf8) return zipUtf8Decoder.decode(bytes);
    let result = "";
    for (let i = 0; i < bytes.length; i++) result += String.fromCharCode(bytes[i]);
    return result;
}

export function parseZipCentralDirectory(buffer: ArrayBuffer): ZipCentralDirectoryEntry[] {
    const bytes = new Uint8Array(buffer);
    const view = new DataView(buffer);
    const eocdOffset = findZipEndOfCentralDirectory(bytes);
    const totalEntries = view.getUint16(eocdOffset + 10, true);
    const directoryOffset = view.getUint32(eocdOffset + 16, true);
    const entries: ZipCentralDirectoryEntry[] = [];
    let offset = directoryOffset;
    for (let i = 0; i < totalEntries; i++) {
        const signature = view.getUint32(offset, true);
        if (signature !== ZIP_CENTRAL_DIR_SIGNATURE) {
            throw new Error("Invalid ZIP central directory header signature.");
        }
        const generalPurposeFlag = view.getUint16(offset + 8, true);
        const compressionMethod = view.getUint16(offset + 10, true);
        const compressedSize = view.getUint32(offset + 20, true);
        const uncompressedSize = view.getUint32(offset + 24, true);
        const fileNameLength = view.getUint16(offset + 28, true);
        const extraLength = view.getUint16(offset + 30, true);
        const commentLength = view.getUint16(offset + 32, true);
        const localHeaderOffset = view.getUint32(offset + 42, true);
        if (
            compressedSize === 0xffffffff ||
            uncompressedSize === 0xffffffff ||
            localHeaderOffset === 0xffffffff
        ) {
            throw new Error("ZIP64 archives are not supported.");
        }
        const nameStart = offset + 46;
        const nameEnd = nameStart + fileNameLength;
        const nameBytes = bytes.subarray(nameStart, nameEnd);
        const fileName = decodeZipFileName(nameBytes, (generalPurposeFlag & 0x0800) !== 0);
        entries.push({
            fileName,
            compressionMethod,
            compressedSize,
            uncompressedSize,
            localHeaderOffset,
            generalPurposeFlag,
        });
        offset = nameEnd + extraLength + commentLength;
    }
    return entries;
}

async function decompressZipDeflate(compressed: ArrayBuffer): Promise<ArrayBuffer> {
    const DecompressionStreamCtor = (globalThis as { DecompressionStream?: any }).DecompressionStream;
    if (!DecompressionStreamCtor) {
        throw new Error("Zip decompression is not supported in this browser.");
    }
    const stream = new Blob([compressed]).stream().pipeThrough(new DecompressionStreamCtor("deflate-raw"));
    return await new Response(stream).arrayBuffer();
}

export async function extractFirstFileByExtensions(
    zipBuffer: ArrayBuffer,
    allowedExtensions: string[],
): Promise<{ buffer: ArrayBuffer; fileName: string }> {
    const normalized = allowedExtensions.map((e) => e.trim().toLowerCase()).filter(Boolean);
    if (normalized.length === 0) {
        throw new Error("No target extensions configured for ZIP extraction.");
    }
    const entries = parseZipCentralDirectory(zipBuffer).filter((e) => !e.fileName.endsWith("/"));
    if (entries.length === 0) throw new Error("ZIP archive does not contain any files.");

    const extensionFromName = (fileName: string): string => {
        const normalized = fileName.toLowerCase().replace(/\\/g, "/");
        const base = normalized.split("/").pop() ?? normalized;
        const dotIndex = base.lastIndexOf(".");
        return dotIndex === -1 ? "" : base.slice(dotIndex);
    };

    const candidates = entries.filter((entry) => normalized.includes(extensionFromName(entry.fileName)));
    if (candidates.length === 0) {
        throw new Error(`ZIP archive does not contain a supported file. Expected one of: ${normalized.join(", ")}.`);
    }
    if (candidates.length > 1) {
        throw new Error("ZIP archive contains multiple matching files. Please include only one.");
    }
    const entry = candidates[0];
    if ((entry.generalPurposeFlag & 0x0001) !== 0) {
        throw new Error("Encrypted ZIP archives are not supported.");
    }
    const view = new DataView(zipBuffer);
    const signature = view.getUint32(entry.localHeaderOffset, true);
    if (signature !== ZIP_LOCAL_FILE_HEADER_SIGNATURE) {
        throw new Error("Invalid ZIP local file header signature.");
    }
    const fileNameLength = view.getUint16(entry.localHeaderOffset + 26, true);
    const extraFieldLength = view.getUint16(entry.localHeaderOffset + 28, true);
    const dataStart = entry.localHeaderOffset + 30 + fileNameLength + extraFieldLength;
    const dataEnd = dataStart + entry.compressedSize;
    if (dataEnd > zipBuffer.byteLength) throw new Error("ZIP entry data exceeds archive bounds.");
    const compressedData = zipBuffer.slice(dataStart, dataEnd);

    let fileBuffer: ArrayBuffer;
    switch (entry.compressionMethod) {
        case 0:
            fileBuffer = compressedData;
            break;
        case 8:
            fileBuffer = await decompressZipDeflate(compressedData);
            // Optional size check
            if (entry.uncompressedSize > 0 && entry.uncompressedSize !== fileBuffer.byteLength) {
                console.warn("ZIP entry uncompressed size mismatch.", entry.uncompressedSize, fileBuffer.byteLength);
            }
            break;
        default:
            throw new Error(`Unsupported ZIP compression method: ${entry.compressionMethod}.`);
    }

    const segments = entry.fileName.split("/").filter(Boolean);
    const displayName = segments.length > 0 ? segments[segments.length - 1] : entry.fileName;
    return { buffer: fileBuffer, fileName: displayName };
}
