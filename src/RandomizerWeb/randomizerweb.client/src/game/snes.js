export function snesToPc(mapping, addr) {
    if (mapping === 'exhirom') {
        const ex = addr < 0x800000 ? 0x400000 : 0;
        const pc = addr & 0x3FFFFF;
        return ex | pc;
    }
    if (mapping === 'lorom') {
        return ((addr & 0x7F0000) >>> 1) | (addr & 0x7FFF);
    }
    if (mapping === 'sa1rom') {
        const lorom = ((addr & 0x7F0000) >>> 1) | (addr & 0x7FFF);
        if (addr >= 0x800000 && addr < 0xC00000) {
            return lorom;
        }
        else if (addr >= 0xC00000) {
            return (addr & 0x3FFFFF) | 0x200000;
        }
        else if (addr < 0x800000) {
            return lorom | 0x400000;
        }
    }
    throw new Error('No known addressing mode supplied');
}
