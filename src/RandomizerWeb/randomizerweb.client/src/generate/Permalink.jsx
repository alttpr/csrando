import React, { useState, useEffect, useContext } from 'react';
import { useParams } from 'react-router-dom';
import { Container, Row, Col, Card, CardHeader, CardBody } from 'reactstrap';

import MessageCard from '../ui/MessageCard';

import Patch from '../patch';
import Spoiler from '../spoiler';

import { adjustHostname } from '../site/domain';

import { decode } from 'slugid';

import { tryParseJson } from '../util';
import defaults from 'lodash/defaults';
import attempt from 'lodash/attempt';

import styled from 'styled-components';

const SpoilerWarning = styled.p`
    font-size: 1.2em;
    color: red;
    font-weight: bold;
`;

export default function Permalink() {
    const { seedSlug } = useParams();
    const seedGuid = decode(seedSlug);

    const [seed, setSeed] = useState(null);
    const [errorMessage, setErrorMessage] = useState(null);

    const game = { id: 'quad' }

    useEffect(() => {
        attempt(async () => {
            try {
                var response = await fetch(`/api/seed/${seedGuid}`);
                if (response && response.ok) {
                    var result = await response.json();
                    setSeed(result);
                } else {
                    setErrorMessage('Cannot load metadata for the specified seed.');
                }
            } catch (error) {
                setErrorMessage(`${error}`);
            }
        });
    }, []); /* eslint-disable-line react-hooks/exhaustive-deps */

    const gameMismatch = seed && seed.gameId !== game.id;
    const world = seed && seed.worlds[0];
    const settings = world && tryParseJson(world.settings);
      if (settings) {
          defaults(settings, {
              opentower: 'sevencrystals',
              ganonvulnerable: 'sevencrystals',
              opensmtourian: 'fourbosses',
              z1triforces: 'eighttriforces',
              z1entranceshuffle: 'none',
              keyshuffle: 'none',
          });
      }

    const content = seed && !gameMismatch ? (
        <>
            <Card className="mb-3">
                <CardHeader className="bg-primary text-white">
                    {seed.gameName} - {seed.gameVersion}
                </CardHeader>
                <CardBody>
                    {settings && settings.spoilerKey && settings.spoilerKey === "true" && (
                        <Row>
                            <Col md="12"><SpoilerWarning>Warning: The creator of this seed is able to access the spoiler log.</SpoilerWarning></Col>
                        </Row>
                    )}
                    <Row>
                        <Col md="3">Seed:</Col><Col> {seedSlug}</Col>
                    </Row>
                    {seed.seedNumber && (
                        <Row>
                            <Col md="3">Seed number:</Col><Col> {seed.seedNumber}</Col>
                        </Row>
                    )}
                    {settings && (
                        <>
                          <Row>
                              <Col md="3">Goal:</Col><Col> {{
                                  defeatall: 'Defeat all four end-bosses',
                                  triforcehunt: 'Triforce hunt'
                              }[settings.goal]
                              }</Col>
                          </Row>
                          <Row>
                                <Col md="3">Key Shuffle:</Col><Col> {{
                                    none: 'None',
                                    keysanity: 'ALTTP Keys and SM Keycards',
                                    z3keys: 'ALTTP Keys',
                                    smkeycards: 'SM Keycards',
                                }[settings.keyshuffle]
                                }</Col>
                          </Row>
                          <Row>
                              <Col md="3">Open Ganon's Tower:</Col><Col> {{
                                  random: 'Randomized',
                                  nocrystals: 'No Crystals',
                                  onecrystal: 'One Crystal',
                                  twocrystals: 'Two Crystals',
                                  threecrystals: 'Three Crystals',
                                  fourcrystals: 'Four Crystals',
                                  fivecrystals: 'Five Crystals',
                                  sixcrystals: 'Six Crystals',
                                  sevencrystals: 'Seven Crystals',
                              }[settings.opentower]
                              }</Col>
                          </Row>
                          <Row>
                              <Col md="3">Ganon Vulnerable:</Col><Col> {{
                                  random: 'Randomized',
                                  nocrystals: 'No Crystals',
                                  onecrystal: 'One Crystal',
                                  twocrystals: 'Two Crystals',
                                  threecrystals: 'Three Crystals',
                                  fourcrystals: 'Four Crystals',
                                  fivecrystals: 'Five Crystals',
                                  sixcrystals: 'Six Crystals',
                                  sevencrystals: 'Seven Crystals',
                              }[settings.ganonvulnerable]
                              }</Col>
                          </Row>
                          <Row>
                              <Col md="3">Open SM Tourian:</Col><Col> {{
                                  random: 'Randomized',
                                  nobosses: 'No Bosses',
                                  oneboss: 'One Boss',
                                  twobosses: 'Two Bosses',
                                  threebosses: 'Three Bosses',
                                  fourbosses: 'Four Bosses',
                              }[settings.opensmtourian]
                              }</Col>
                            </Row>
                            <Row>
                                <Col md="3">Open Z1 Level 9:</Col><Col> {{
                                    random: 'Randomized',
                                    notriforces: 'No Triforces',
                                    onetriforces: 'One Triforces',
                                    twotriforces: 'Two Triforces',
                                    threetriforces: 'Three Triforces',
                                    fourtriforces: 'Four Triforces',
                                    fivetriforces: 'Five Triforces',
                                    sixtriforces: 'Six Triforces',
                                    seventriforces: 'Seven Triforces',
                                    eighttriforces: 'Eight Triforces',
                                }[settings.z1triforces]
                                }</Col>
                            </Row>
                            <Row>
                                <Col md="3">Z1 Entrance Shuffle:</Col>
                                <Col>
                                    {{
                                        none: 'None',
                                        overworld: 'Overworld'
                                    }[settings.z1entranceshuffle]}
                                </Col>
                            </Row>
                        </>
                    )}
                    {settings && settings.race === 'true' && (
                        <Row>
                            <Col>Race Rom (no spoilers)</Col>
                        </Row>
                    )}
                    {settings && settings.initialitems && (
                        <Row>
                            <Col md="3">Starting items:</Col>
                            <Col>{settings.initialitems}</Col>
                        </Row>
                    )}

                    <Row className="mt-3">
                        <Col>
                            <Patch seed={seed} world={world} />
                        </Col>
                    </Row>
                </CardBody>
            </Card>
            {settings && settings.race === 'false' && (
                <Spoiler seedGuid={seed.guid} />
            )}
        </>) :
        errorMessage ? <MessageCard error={true} title="Something went wrong :(" msg={errorMessage} /> :
        gameMismatch ? (
            <MessageCard error={true} title="This is not quite right :O"
                msg={<a href={adjustHostname(document.location.href, seed.gameId)}>Go to the correct domain here</a>}
            />
        ) :
        <MessageCard title="Game information" msg="Please wait, loading..." />;

    return (
        <Container>
            <Row className="justify-content-md-center">
                <Col md="10">
                    {content}
                </Col>
            </Row>
        </Container>
    );
}
