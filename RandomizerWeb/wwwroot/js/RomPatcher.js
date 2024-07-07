import { get } from "./IndexedDbAccessor.js"
import { applySprite } from "./rdc.js"
import { parse, apply } from 'https://cdn.jsdelivr.net/npm/bps@2.0.1/+esm'
import fileSaver from 'https://cdn.jsdelivr.net/npm/file-saver@2.0.5/+esm'


export async function patchRom(fileName, bpsPatch, patchData, spritePaths)
{

  let mergedRom = await mergeRoms();

  // Apply the the BPS patch to the merged rom
  const { instructions, checksum } = parse(new Uint8Array(bpsPatch));
  const quadRomArray = apply(instructions, new Uint8Array(mergedRom));

  // Loop through the patchData and apply the patches, it's a dict of (address, byte[])
  for (let address in patchData)
  {
    let patch = patchData[address];
    let intAddress = parseInt(address);
    for (let i = 0; i < patch.length; i++)
    {
      quadRomArray[intAddress + i] = patch[i];
    }
  }

  if (spritePaths)
  {
    if (spritePaths.sm) {
      var sprite = { path: spritePaths.sm };
      await applySprite(quadRomArray, 'sa1sm', 'samus_sprite', sprite);
    }

    if (spritePaths.z3) {
      var sprite = { path: spritePaths.z3 };
      await applySprite(quadRomArray, 'sa1z3', 'link_sprite', sprite);
    }

    // Set vanilla screw attack flag
    quadRomArray[0x3F0204] = 0x01;

  }

  

  fileSaver.saveAs(new Blob([quadRomArray]), fileName)

  return true;
}


async function mergeRoms() {
  let mergedRom = new Uint8Array(0);

  for (let i = 1; i <= 4; i++) {
    let romRecord = await get("Roms", i);

    if (!romRecord) {
      return false;
    }

    let rom = romRecord.data;
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
