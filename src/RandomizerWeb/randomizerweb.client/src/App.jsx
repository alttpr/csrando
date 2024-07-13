import React, { Suspense, lazy } from 'react';
import { Route, Switch } from 'react-router';
import { Container } from 'reactstrap';

import GlobalStyle from './site/GlobalStyle';
import Home from './site/Home';
import { NavMenu, NavMenuItem, NavMenuDropdown } from './ui/NavMenu';

//import MultiworldInstructionsMd from './resources/markdown/mwinstructions.md';
import ResourcesMd from './resources/markdown/resources.md';
import InformationMd from './resources/markdown/information.md';
import ChangelogMd from './resources/markdown/changelog.md';

const Markdown = lazy(() => import('./ui/Markdown'));
const Configure = lazy(() => import('./generate/Configure'));
const Permalink = lazy(() => import('./generate/Permalink'));
//const Multiworld = lazy(() => import('./multiworld'));

export default function App() { 
  return (
    <>
          <GlobalStyle />
          <NavMenu
              brand={<NavMenuItem to="/">Home</NavMenuItem>}
              nav={<NavMenuItem to="/configure">Generate randomized game</NavMenuItem>}
              dropdown={
                  <NavMenuDropdown title="Help">
                      <NavMenuItem to="/information">Information</NavMenuItem>
                      { /* <NavMenuItem to="/mwinstructions">Multiworld instructions</NavMenuItem> */ }
                      <NavMenuItem to="/resources">Resources</NavMenuItem>
                      <NavMenuItem to="/changelog">Changes</NavMenuItem>
                  </NavMenuDropdown>
              }
          />
          <Container className="mb-5">
              <Suspense fallback={<div></div>}>
                  <Switch>
                      <Route exact path="/" component={Home} />
                      {/*
                      <Route path="/mwinstructions"
                          render={props => <Markdown {...props} source={MultiworldInstructionsMd} />}
                      /> */ }
                      <Route path="/resources"
                          render={props => <Markdown {...props} source={ResourcesMd} />}
                      />
                      <Route path="/information"
                          render={props => <Markdown {...props} source={InformationMd} />}
                      />
                      <Route path="/changelog"
                          render={props => <Markdown {...props} source={ChangelogMd} />}
                      />
                      <Route exact path="/configure" component={Configure} />
                      { /* <Route path="/multiworld/:sessionSlug" component={Multiworld} /> */}
                      <Route path="/seed/:seedSlug" component={Permalink} />
                  </Switch>
              </Suspense>
           </Container>
     </>
    );
}
