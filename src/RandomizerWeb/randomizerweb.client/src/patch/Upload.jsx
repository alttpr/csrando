import React, { useState, useRef, useContext } from 'react';
import { Form, Row, Col, Button } from 'reactstrap';
import { readAsArrayBuffer } from '../util';
import { h32 } from 'xxhashjs';

import localForage from 'localforage';

import some from 'lodash/some';
import map from 'lodash/map';
import compact from 'lodash/compact';
import hasIn from 'lodash/hasIn';

/* All quad fours */
const HashSeed = 0x44444444;

const Z3Hash = 0x77AB97C2;
const SMHash = 0xD4AC7204;
const M1Hash = 0x5BE7D501;
const Z1Hash = 0x2842295F;

export default function Upload(props) {
    const [canUpload, setCanUpload] = useState(false);
    const fileInputSM = useRef(null);
    const fileInputZ3 = useRef(null);
    const fileInputZ1 = useRef(null);
    const fileInputM1 = useRef(null);

    async function onSubmitRom() {
        const smFile = fileInputSM.current !== null ? fileInputSM.current.files[0] : null;
        const z3File = fileInputZ3.current !== null ? fileInputZ3.current.files[0] : null;
        const z1File = fileInputZ1.current !== null ? fileInputZ1.current.files[0] : null;
        const m1File = fileInputM1.current !== null ? fileInputM1.current.files[0] : null;

        let fileDataSM = null;
        let fileDataZ3 = null;
        let fileDataZ1 = null;
        let fileDataM1 = null;

        const mismatch = {};

        try {
            fileDataSM = new Uint8Array(await readAsArrayBuffer(smFile));
            if (fileDataSM.length % 0x1000 === 0x200) {
                fileDataSM = fileDataSM.slice(0x200);
            }
            mismatch.SM = h32(fileDataSM.buffer, HashSeed).toNumber() !== SMHash;
        } catch (error) {
            console.log("Could not read uploaded SM file data:", error);
            return;
        }

        try {
            fileDataZ3 = new Uint8Array(await readAsArrayBuffer(z3File));
            if (fileDataZ3.length % 0x1000 === 0x200) {
                fileDataZ3 = fileDataZ3.slice(0x200);
            }
            mismatch.ALTTP = h32(fileDataZ3.buffer, HashSeed).toNumber() !== Z3Hash;
        } catch (error) {
            console.log("Could not read uploaded ALTTP file data:", error);
            return;
        }

        try {
            fileDataM1 = new Uint8Array(await readAsArrayBuffer(m1File));
            mismatch.M1 = h32(fileDataM1.buffer.slice(0x10), HashSeed).toNumber() !== M1Hash;
        } catch (error) {
            console.log("Could not read uploaded M1 file data:", error);
            return;
        }

        try {
            fileDataZ1 = new Uint8Array(await readAsArrayBuffer(z1File));
            mismatch.Z1 = h32(fileDataZ1.buffer.slice(0x10), HashSeed).toNumber() !== Z1Hash;
        } catch (error) {
            console.log("Could not read uploaded Z1 file data:", error);
            return;
        }


        if (some(mismatch)) {
            const games = compact(map(mismatch, (truth, name) => truth ? name : null));
            alert(`Incorrect ${games.join(', ')} rom file(s)`);
            return;
        }

        try {
            if (fileDataSM)
                await localForage.setItem('baseRomSM', new Blob([fileDataSM]));
            if (fileDataZ3)
                await localForage.setItem('baseRomLTTP', new Blob([fileDataZ3]));
            if (fileDataM1)
                await localForage.setItem('baseRomM1', new Blob([fileDataM1]));
            if (fileDataZ1)
                await localForage.setItem('baseRomZ1', new Blob([fileDataZ1]));
        } catch (error) {
            console.log("Could not store file to localforage:", error);
            return;
        }

        props.onUpload();
    }

    const onFileSelect = () => {
        setCanUpload(
            hasIn(fileInputSM.current, 'files[0]') && hasIn(fileInputZ3.current, 'files[0]') && hasIn(fileInputZ1.current, 'files[0]') && hasIn(fileInputM1.current, 'files[0]')
        );
    };

    return (
        <Form onSubmit={(e) => { e.preventDefault(); onSubmitRom(); }}>
            <h6>No ROM uploaded, please upload a valid ROM file.</h6>
            <Row className="justify-content-between">
                <Col md="6">A Link to the Past ROM: <input type="file" ref={fileInputZ3} onChange={onFileSelect} /></Col>
                <Col md="6">Super Metroid ROM: <input type="file" ref={fileInputSM} onChange={onFileSelect} /></Col>
            </Row>
            <Row className="justify-content-between">
                <Col md="6">Zelda 1 ROM: <input type="file" ref={fileInputZ1} onChange={onFileSelect} /></Col>
                <Col md="6">Metroid 1 ROM: <input type="file" ref={fileInputM1} onChange={onFileSelect} /></Col>
            </Row>
            <Row className="mt-3">
                <Col md="6">
                    <Button type="submit" color="primary" disabled={!canUpload}>Upload Files</Button>
                </Col>
            </Row>
        </Form>
    );
}
