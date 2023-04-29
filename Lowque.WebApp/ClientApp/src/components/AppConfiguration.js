import React, { Component } from 'react';
import { Form, FormGroup, Label, Input, InputGroup, InputGroupText, Tooltip, Spinner } from 'reactstrap';
import { FaInfoCircle } from 'react-icons/fa';
import { get } from '../utils/api';
import './AppConfiguration.css';

export class AppConfiguration extends Component {
  static displayName = AppConfiguration.name;

  constructor(props) {
    super(props);

    this.state = {
      configurationLoaded: false,
      tooltips: {
        isHeaderOpen: false,
        isWorkspaceOpen: false,
        isDotnetCliOpen: false,
      }
    };

    this.toggleTooltip = this.toggleTooltip.bind(this);
    this.renderTooltip = this.renderTooltip.bind(this);
  }

  componentDidMount() {
    get('/Application/GetConfiguration')
      .then(response => response.json())
      .then(data => this.setState({
        configurationLoaded: true,
        workspace: data.workspace,
        dotnetCli: data.dotnetCli,
      }));
  }

  toggleTooltip(name) {
    this.setState(prevState => ({
      tooltips: {
        ...prevState.tooltips,
        [name]: !prevState.tooltips[name]
      }
    }));
  }

  renderTooltip(name) {
    const propName = `is${name}Open`;
    return <Tooltip placement={'bottom'} target={`${name}-Info`}
      isOpen={this.state.tooltips[propName]} toggle={() => this.toggleTooltip(propName)}>
      {this.props.global.glp(`AppConfiguration_Tooltip_${name}`)}
    </Tooltip>
  }

  render() {
    const glp = this.props.global.glp;
    return (
      <>
        {!this.state.configurationLoaded && <Spinner />}
        {this.state.configurationLoaded &&
          <Form id={'configuration-form'}>
            {this.renderTooltip('Header')}
            <legend>{glp('AppConfiguration_Header')} <FaInfoCircle id={'Header-Info'} /></legend>
            <br />

            <FormGroup>
              {this.renderTooltip('Workspace')}
              <Label>{glp('AppConfiguration_Workspace')}</Label>
              <InputGroup>
                <Input defaultValue={this.state.workspace || ''} disabled />
                <InputGroupText><FaInfoCircle id={'Workspace-Info'} /></InputGroupText>
              </InputGroup>
            </FormGroup>

            <FormGroup>
              {this.renderTooltip('DotnetCli')}
              <Label>{glp('AppConfiguration_DotnetCli')}</Label>
              <InputGroup>
                <Input defaultValue={this.state.dotnetCli || ''} disabled />
                <InputGroupText><FaInfoCircle id={'DotnetCli-Info'} /></InputGroupText>
              </InputGroup>
            </FormGroup>
          </Form>}
      </>
    );
  }
}
