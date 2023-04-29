import React, { Component } from 'react';
import {
  Form, FormGroup, Label, Input,
  Nav, NavItem, NavLink, Tooltip,
  Button, Spinner
} from 'reactstrap';
import { Link } from 'react-router-dom';
import { FaInfoCircle } from 'react-icons/fa';
import { get, post } from '../utils/api';
import './AppDefinition.css';

export class AppDefinition extends Component {
  static displayName = AppDefinition.name;

  constructor(props) {
    super(props);
    this.state = {
      application: {},
      isInfoTooltipOpen: false,
    }

    this.toggleInfoTooltip = this.toggleInfoTooltip.bind(this);
    this.deployApplication = this.deployApplication.bind(this);
  }

  componentDidMount() {
    get(`/ApplicationDefinition/GetApplicationDefinition/${this.props.match.params.id}`)
      .then(response => response.json())
      .then(data => this.setState({ application: data }));
  }

  toggleInfoTooltip() {
    this.setState(prevState => ({ isInfoTooltipOpen: !prevState.isInfoTooltipOpen }));
  }

  deployApplication() {
    this.setState({ deployingApplication: true });
    const appId = this.props.match.params.id;

    post('/ApplicationDefinition/DeployApplication', { appId: appId })
      .then(response => response.json())
      .then(data => {
        this.setState({ deployingApplication: false });
        if (data.success) {
          this.props.global.openSuccessAlert(this.props.global.glp('AppDefinition_DeployApplication_Success'));
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      })
      .catch(error => {
        this.setState({ deployingApplication: false });
        this.props.global.openDangerAlert(this.props.global.glp('AppDefinition_DeployApplication_InternalServerError'));
      });
  }

  render() {
    const glp = this.props.global.glp;
    const appId = this.props.match.params.id;

    return (
      <>
        <Form id={'app-info-form'}>
          <FormGroup>
            <Label>{glp('AppDefinition_AppName')}</Label>
            <Input defaultValue={this.state.application.name || ''} disabled />
          </FormGroup>
          <FormGroup>
            <Label>{glp('AppDefinition_CreatedAt')}</Label>
            <Input defaultValue={this.state.application.createdAt || ''} disabled />
          </FormGroup>
        </Form>

        <div>{glp('AppDefinition_Modules')} <FaInfoCircle id={'modules-info'} /></div>
        <Tooltip placement={'bottom'} target={'modules-info'} isOpen={this.state.isInfoTooltipOpen} toggle={this.toggleInfoTooltip}>
          {glp('AppDefinition_Tooltip')}
        </Tooltip>
        <Nav vertical>
          <NavItem><NavLink tag={Link} to={`/view-designer/${appId}`} disabled>{glp('AppDefinition_PresentationModule')}</NavLink></NavItem>
          <NavItem><NavLink tag={Link} to={`/flow-definitions/${appId}`}>{glp('AppDefinition_BusinessLogicModule')}</NavLink></NavItem>
          <NavItem><NavLink tag={Link} to={`/data-designer/${appId}`} disabled>{glp('AppDefinition_DataAccessModule')}</NavLink></NavItem>
          <NavItem><NavLink tag={Link} to={`/identity-designer/${appId}`} disabled>{glp('AppDefinition_IdentityModule')}</NavLink></NavItem>
          <NavItem><NavLink tag={Link} to={`/deploy-designer/${appId}`} disabled>{glp('AppDefinition_DeploymentModule')}</NavLink></NavItem>     
        </Nav>
        <br />

        <div id={'deploy-button-wrapper'}>
          <Button id={'deploy-button'} disabled={this.state.deployingApplication}
            onClick={this.deployApplication}>{glp('AppDefinition_Deploy')}</Button>
          {this.state.deployingApplication && <Spinner />}
        </div>
      </>
    );
  }
}
