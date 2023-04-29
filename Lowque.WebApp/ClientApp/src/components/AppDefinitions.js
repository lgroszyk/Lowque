import React, { Component } from 'react';
import { Table, Button, Form, FormGroup, Label, Input, Collapse, Spinner } from 'reactstrap';
import { get, post } from '../utils/api';
import './AppDefinitions.css';

export class AppDefinitions extends Component {
  static displayName = AppDefinitions.name;

  constructor(props) {
    super(props);

    this.state = {
      applications: [],
      templates: [],
      newApplication: {},
      existingApplicationLoaded: false,
      isNewApplicationFormOpened: false,
    };

    this.chooseApplicationFromList = this.chooseApplicationFromList.bind(this);
    this.toggleNewApplicationForm = this.toggleNewApplicationForm.bind(this);
    this.changeNewApplicationField = this.changeNewApplicationField.bind(this);
    this.addNewApplication = this.addNewApplication.bind(this);
  }

  componentDidMount() {
    get("ApplicationDefinition/GetExistingApplicationsAndTemplates")
      .then(response => response.json())
      .then(data => this.setState({
        templates: data.templates,
        applications: data.applications,
        newApplication: { template: data.templates[0] },
        existingApplicationLoaded: true,
      }));
  }

  chooseApplicationFromList(applicationId) {
    this.props.history.push(`/app-designer/${applicationId}`);
  }

  toggleNewApplicationForm() {
    this.setState(prevState => ({ isNewApplicationFormOpened: !prevState.isNewApplicationFormOpened }));
  }

  changeNewApplicationField(fieldName, fieldValue) {
    this.setState(prevState => ({ newApplication: { ...prevState.newApplication, [fieldName]: fieldValue } }));
  }

  addNewApplication(event) {
    event.preventDefault();
    post('/ApplicationDefinition/CreateApplicationDefinition', this.state.newApplication)
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          this.props.history.push(`/app-designer/${data.applicationId}`)
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      });
  }

  render() {
    const glp = this.props.global.glp;

    return (
      <>
        <div id={'new-app-panel'}>
          <Button color={'primary'} onClick={this.toggleNewApplicationForm}>{glp('AppDefinitions_NewApp_Prompt')}</Button>
          <Collapse isOpen={this.state.isNewApplicationFormOpened}>
            <Form id={'new-app-form'} onSubmit={this.addNewApplication}>
              <br />
              <FormGroup>
                <Label for={'template'}>{glp('AppDefinitions_NewApp_AppTemplate')}</Label>
                <Input name={'template'} type={'select'} value={this.state.newApplication.template || ''}
                  required minLength={3} maxLength={50} onChange={event => this.changeNewApplicationField(event.target.name, event.target.value)}>
                  {this.state.templates.map(temp => <option key={`appTemplate_${temp}`} value={temp}>{temp}</option>)}
                </Input>
              </FormGroup>
              <Button type={'submit'}>{glp('AppDefinitions_NewApp_Submit')}</Button>
            </Form>
          </Collapse>
        </div>

        <div id={'existing-apps-list'}>
          <legend>{glp('AppDefinitions_ExistingApps_Header')}</legend>
          <Table id={'app-definitions-table'} hover>
            <thead>
              <tr>
                <th>{glp('AppDefinitions_Table_Name')}</th>
                <th>{glp('AppDefinitions_Table_CreatedAt')}</th>
              </tr>
            </thead>

            <tbody>
              {this.state.existingApplicationLoaded && this.state.applications.map(app =>
                <tr key={`appDefinition_${app.id}`} onClick={() => this.chooseApplicationFromList(app.id)}>
                  <td>{app.name}</td>
                  <td>{app.createdAt}</td>
                </tr>)}
              {!this.state.existingApplicationLoaded &&
                <tr>
                  <td><Spinner /></td>
                  <td></td>
                </tr>}
            </tbody>
          </Table>
        </div>
      </>);
  }
}
