import React, { Component } from 'react';
import { Route, Redirect } from 'react-router';
import { Layout } from './components/Layout';
import { AppConfiguration } from './components/AppConfiguration';
import { AppDefinitions } from './components/AppDefinitions';
import { AppDefinition } from './components/AppDefinition';
import { FlowDefinitions } from './components/FlowDefinitions';
import { FlowDesigner } from './components/FlowDesigner';
import { Login } from './components/identity/Login';
import { Profile } from './components/identity/Profile';
import { isAuthenticated } from './utils/identity';
import { get } from './utils/api';
import { isInRole } from './utils/identity';
import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  constructor(props) {
    super(props);

    this.state = {
      alert: {
        state: {
          visible: false,
          color: 'danger',
          message: '',
        }
      },
      isAuthenticated: isAuthenticated(),
      localizedPhrases: {}
    };

    this.closeAlert = this.closeAlert.bind(this);
    this.openDangerAlert = this.openDangerAlert.bind(this);
    this.openSuccessAlert = this.openSuccessAlert.bind(this);
    this.getLocalizedPhrase = this.getLocalizedPhrase.bind(this);
    this.setIsAuthenticated = this.setIsAuthenticated.bind(this);
    this.hasAccess = this.hasAccess.bind(this);
    this.renderOrRedirect = this.renderOrRedirect.bind(this);
  }

  componentDidMount() {
    get('/Application/GetLocalizedPhrases')
      .then(response => response.json())
      .then(data => this.setState({
        isLocalizationLoaded: true,
        localizedPhrases: data.localizedPhrases
      }))

  }

  getLocalizedPhrase(key) {
    const localizedPhrase = this.state.localizedPhrases[key];
    return localizedPhrase === undefined ? key : localizedPhrase;
  }

  closeAlert() {
    this.setState(prevState => ({ alert: { ...prevState.alert, state: { ...prevState.alert.state, visible: false } } }));
  }

  openDangerAlert(message) {
    this.setState(prevState => ({ alert: { ...prevState.alert, state: { ...prevState.alert.state, visible: true, message: message, color: 'danger' } } }));
  }

  openSuccessAlert(message) {
    this.setState(prevState => ({ alert: { ...prevState.alert, state: { ...prevState.alert.state, visible: true, message: message, color: 'success' } } }));
  }

  setIsAuthenticated(isAuthenticated) {
    this.setState({ isAuthenticated: isAuthenticated });
  }

  hasAccess(allowedRoles) {
    if (!this.state.isAuthenticated) {
      return false;
    }

    const areRolesDefined = Boolean(allowedRoles);
    if (!areRolesDefined) {
      return true;
    }

    for (let i = 0; i < allowedRoles.length; i++) {
      const requiredRole = allowedRoles[i];
      if (isInRole(requiredRole)) {
        return true;
      }
    }
    return false;
  }

  renderOrRedirect(component) {
    return this.hasAccess() ? component : (<Redirect to='/login' />);
  }

  render() {
    const globalState = {
      alert: this.state.alert,
      openDangerAlert: this.openDangerAlert,
      openSuccessAlert: this.openSuccessAlert,
      closeAlert: this.closeAlert,
      glp: this.getLocalizedPhrase,
      isLocalizationLoaded: this.state.isLocalizationLoaded,
      isAuthenticated: this.state.isAuthenticated,
      setIsAuthenticated: this.setIsAuthenticated
    }

    return (
      <Layout global={globalState}>
        <Route exact path='/' render={(props) => this.renderOrRedirect(<AppDefinitions {...props} global={globalState} />)} />
        <Route path='/app-configuration/' render={(props) => this.renderOrRedirect(<AppConfiguration {...props} global={globalState} />)} />
        <Route path='/app-designer/:id' render={(props) => this.renderOrRedirect(<AppDefinition {...props} global={globalState} />)} />
        <Route path='/flow-definitions/:id' render={(props) => this.renderOrRedirect(<FlowDefinitions {...props} global={globalState} />)} />
        <Route path='/flow-designer/:id' render={(props) => this.renderOrRedirect(<FlowDesigner {...props} global={globalState} />)} />
        <Route path='/profile' render={(props) => this.renderOrRedirect(<Profile {...props} global={globalState} />)} />
        <Route path='/login' render={(props) => <Login {...props} global={globalState} />} />
      </Layout>
    );
  }
}
