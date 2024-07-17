import { readAsArrayBuffer } from '../util';

import { parseRdc } from '../game/rdc';
import { snesToPc } from '../game/snes';
import { bigText } from '../game/big_text_table';

import { inflate } from 'pako';
import localForage from 'localforage';
import { parse, apply } from 'bps';

import each from 'lodash/each';
import range from 'lodash/range';
import defaultTo from 'lodash/defaultTo';
import isPlainObject from 'lodash/isPlainObject';

const legalCharacters = /[A-Z0-9]/;
const illegalCharacters = /[^A-Z0-9]/g;
const continousSpace = / +/g;

export async function prepareRom(worldPatch, settings, baseBps) {
    let rom = null;
    const smRom = new Uint8Array(await readAsArrayBuffer(await localForage.getItem("baseRomSM")));
    const lttpRom = new Uint8Array(await readAsArrayBuffer(await localForage.getItem("baseRomLTTP")));
    const m1Rom = new Uint8Array(await readAsArrayBuffer(await localForage.getItem("baseRomM1")));
    const z1Rom = new Uint8Array(await readAsArrayBuffer(await localForage.getItem("baseRomZ1")));


    /* Replace Z1 and M1 headers with the ones used for patch generation */
    const m1Header = [0x4E, 0x45, 0x53, 0x1A, 0x08, 0x00, 0x11, 0x00, 0x00, 0x00, 0x4E, 0x49, 0x20, 0x31, 0x2E, 0x33];
    const z1Header = [0x4E, 0x45, 0x53, 0x1A, 0x08, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

    for (let i = 0; i < 0x10; i++)
    {
        m1Rom[i] = m1Header[i];
        z1Rom[i] = z1Header[i];
    }

    rom = mergeRoms(smRom, lttpRom, m1Rom, z1Rom);

    const basePatch = Uint8Array.from(atob(baseBps), c => c.charCodeAt(0));
    worldPatch = Uint8Array.from(atob(worldPatch), c => c.charCodeAt(0));

    const mapping = 'sa1rom';

    rom = applyBps(rom, basePatch);

    applySeed(rom, worldPatch);

    await applySprite(rom, mapping, 'link_sprite', settings.z3Sprite);
    await applySprite(rom, mapping, 'samus_sprite', settings.smSprite);

    if (settings.smSpinjumps) {
        smSpinjumps(rom, mapping);
    }

    z3HeartColor(rom, mapping, settings.z3HeartColor);
    z3HeartBeep(rom, settings.z3HeartBeep);
    z3QuickSwap(rom, settings.z3QuickSwap);

    if (!settings.smEnergyBeep) {
        smEnergyBeepOff(rom, mapping);
    }

    return rom;
}

async function applySprite(rom, mapping, block, sprite) {
    if (sprite.path) {
        const url = `/sprites/${sprite.path}`;
        const rdc = maybeCompressed(new Uint8Array(await (await fetch(url)).arrayBuffer()));
        const [blocks, author] = parseRdc(rdc);
        blocks[block] && blocks[block](rom, mapping);
        //applySpriteAuthor(rom, mapping, block, author);
    }
}

function applySpriteAuthor(rom, mapping, block, author) {
    author = author.toUpperCase();
    /* Author field that is empty or has no accepted characters */
    if (!author.match(legalCharacters))
        return;

    author = formatAuthor(author);
    const width = 32;
    const pad = (width - author.length) >> 1; /* shift => div + floor */

    const addrs = {
        link_sprite: [0xF47002, 0xFD1480],
        samus_sprite: { exhirom: [0xF47004, 0xFD1600], lorom: [0xCEFF02, 0xCEC740] },
    }[block];
    const [enable, tilemap] = isPlainObject(addrs) ? addrs[mapping] : addrs;

    rom[snesToPc(mapping, enable)] = 0x01;
    each(author, (char, i) => {
        const bytes = bigText[char];
        rom[snesToPc(mapping, tilemap + 2 * (pad + i))] = bytes[0];
        rom[snesToPc(mapping, tilemap + 2 * (pad + i + 32))] = bytes[1];
    });
}

function formatAuthor(author) {
    author = author.replace(illegalCharacters, ' ');
    author = author.replace(continousSpace, ' ');
    /* Keep at most 30 non-whitespace characters */
    /* A limit of 30 guarantee a margin at the edges */
    return author.trimStart().slice(0, 30).trimEnd();
}

/* Enables separate spinjump behavior */
function smSpinjumps(rom, mapping) {
    rom[snesToPc(mapping, 0xDF0204)] = 0x01;
}

function z3HeartColor(rom, mapping, setting) {
    const b = setting === 'blue' ? 0x01 : setting === 'green' ? 0x02 : setting === 'yellow' ? 0x03 : 0x00;
    rom[0x587020] = b;
}

function z3HeartBeep(rom, setting) {
    const values = {
        off: 0x00,
        double: 0x10,
        normal: 0x20,
        half: 0x40,
        quarter: 0x80
    };
    /* Redirected to low bank $40 in combo */
    rom[0x580033] = defaultTo(values[setting], values.half);
}

function z3QuickSwap(rom, setting) {
    rom[0x58004B] = setting ? 0x01 : 0x00;
}

function smEnergyBeepOff(rom, mapping) {
    each([
        [0x90EA9B, 0x80],
        [0x90F337, 0x80],
        [0x91E6D5, 0x80]
    ],
        ([addr, value]) => rom[snesToPc(mapping, addr)] = value
    );
}

function maybeCompressed(data) {
    const big = false;
    const isGzip = new DataView(data.buffer).getUint16(0, big) === 0x1f8b;
    return isGzip ? inflate(data) : data;
}

function mergeRoms(smRom, z3Rom, m1Rom, z1Rom) {
    let mergedRom = new Uint8Array(0);
    let roms = [smRom, z3Rom, m1Rom, z1Rom];

    for (let i = 0; i <= 3; i++) {
        let rom = roms[i];
        mergedRom = concatenateUint8Arrays(mergedRom, rom);
    }

    return mergedRom;
}

function concatenateUint8Arrays(arr1, arr2) {
    let result = new Uint8Array(arr1.length + arr2.length);
    result.set(arr1);
    result.set(arr2, arr1.length);
    return result;
}

function applyIps(rom, patch) {
    const big = false;
    let offset = 5;
    const footer = 3;
    const view = new DataView(patch.buffer);
    while (offset + footer < patch.length) {
        const dest = (patch[offset] << 16) + view.getUint16(offset + 1, big);
        const length = view.getUint16(offset + 3, big);
        offset += 5;
        if (length > 0) {
            rom.set(patch.slice(offset, offset + length), dest);
            offset += length;
        } else {
            const rleLength = view.getUint16(offset, big);
            const rleByte = patch[offset + 2];
            rom.set(Uint8Array.from(new Array(rleLength), () => rleByte), dest);
            offset += 3;
        }
    }
}

function applyBps(rom, patch) {
    const { instructions, checksum } = parse(new Uint8Array(patch));
    const patchedRom = apply(instructions, new Uint8Array(rom));
    return patchedRom;
}

function applySeed(rom, patch) {
    const little = true;
    let offset = 0;
    const view = new DataView(patch.buffer);
    while (offset < patch.length) {
        let dest = view.getUint32(offset, little);
        let length = view.getUint16(offset + 4, little);
        offset += 6;

        let patchData = patch.slice(offset, offset + length);

        rom.set(patchData, dest);
        offset += length;
    }
}
