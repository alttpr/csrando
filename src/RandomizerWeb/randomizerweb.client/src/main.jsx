/* Polyfill features missing from IE11 and Edge */
//import 'react-app-polyfill/ie11';
//import 'react-app-polyfill/stable';
import './polyfill/TextDecoder';

import 'bootstrap/dist/css/bootstrap.css';

import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter as Router } from 'react-router-dom';

import App from './App';

//const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const baseUrl = "/";

ReactDOM.render(
  <Router basename={baseUrl}>
    <App />
  </Router>,
  document.getElementById('root')
);
