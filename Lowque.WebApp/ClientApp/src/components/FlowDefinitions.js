import React, { Component } from 'react';
import {
  Table, Button, Form, FormGroup, Label, Input, Collapse,
  Modal, ModalHeader, ModalBody, ModalFooter,
} from 'reactstrap';
import { get, post, remove } from '../utils/api';
import { FaTrash } from 'react-icons/fa';
import { BsChevronDown } from "react-icons/bs";
import { nanoid } from 'nanoid';
import download from 'downloadjs';
import './FlowDefinitions.css';

export class FlowDefinitions extends Component {
  static displayName = FlowDefinitions.name;

  constructor(props) {
    super(props);

    this.state = {
      flows: [],
      spec: {},
      newFlow: { applicationId: this.props.match.params.id },
    }

    this.toggleNewFlowForm = this.toggleNewFlowForm.bind(this);
    this.chooseFlowFromList = this.chooseFlowFromList.bind(this);
    this.changeNewFlowField = this.changeNewFlowField.bind(this);
    this.addNewFlow = this.addNewFlow.bind(this);
    this.askForRemoveFlow = this.askForRemoveFlow.bind(this);
    this.removeFlow = this.removeFlow.bind(this);
    this.toggleRemoveFlowModal = this.toggleRemoveFlowModal.bind(this);
    this.toggleSpecModal = this.toggleSpecModal.bind(this);
    this.downloadSpecification = this.downloadSpecification.bind(this);
  }

  componentDidMount() {
    get(`/ApplicationDefinition/GetFlowDefinitions/${this.props.match.params.id}`)
      .then(response => response.json())
      .then(data => this.setState({
        flows: data.flows,
        spec: data.spec,
        collapseData: data.flows.map(flow => ({ flowId: `${flow.name}_${flow.area}`, collapsed: false }))
      }));
  }

  toggleNewFlowForm() {
    this.setState(prevState => ({ isNewFlowFormOpened: !prevState.isNewFlowFormOpened }));
  }

  chooseFlowFromList(flowId) {
    this.props.history.push(`/flow-designer/${flowId}`);
  }

  changeNewFlowField(fieldName, fieldValue) {
    this.setState(prevState => ({ newFlow: { ...prevState.newFlow, [fieldName]: fieldValue } }));
  }

  addNewFlow(event) {
    event.preventDefault();
    post('/ApplicationDefinition/CreateFlowDefinition', this.state.newFlow)
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          this.props.history.push(`/flow-designer/${data.flowId}`)
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      });
  }

  askForRemoveFlow(event, flowId) {
    event.stopPropagation();
    this.setState({ isRemoveFlowModalOpen: true, flowToRemove: flowId });
  }

  removeFlow(event) {
    this.setState({ isRemoveFlowModalOpen: false });
    remove('/ApplicationDefinition/DeleteFlowDefinition', this.state.flowToRemove)
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          this.setState(prevState => ({
            flows: prevState.flows.filter(flow => flow.id !== prevState.flowToRemove),
          }));
          this.props.global.openSuccessAlert(this.props.global.glp('FlowDefinitions_FlowRemoved'));
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      });

  }

  toggleRemoveFlowModal() {
    this.setState(prevState => ({ isRemoveFlowModalOpen: !prevState.isRemoveFlowModalOpen }));
  }

  toggleSpecModal() {
    this.setState(prevState => ({ isSpecModalOpen: !prevState.isSpecModalOpen }));
  }

  toggleSpecCollapse(id) {
    this.setState(prevState => ({
      collapseData: prevState.collapseData.map(datum =>
        datum.flowId == id ? { flowId: datum.flowId, collapsed: !datum.collapsed } : datum)
    }));
  }

  downloadSpecification() {
    get(`/ApplicationDefinition/DownloadSpecification/${this.props.match.params.id}`)
      .then(response => response.blob())
      .then(blob => download(blob, 'specification.html'));
  }

  render() {
    const glp = this.props.global.glp;

    return (
      <>
        <div id={'new-flow-form-wrapper'}>

          <Modal isOpen={this.state.isRemoveFlowModalOpen} toggle={this.toggleRemoveFlowModal}>
            <ModalHeader>{glp('FlowDefinitions_RemoveFlowModal_Header')}</ModalHeader>
            <ModalBody>{glp('FlowDefinitions_RemoveFlowModal_Body')}</ModalBody>
            <ModalFooter>
              <Button color={'danger'} onClick={this.removeFlow}>{glp('FlowDefinitions_RemoveFlowModal_Button_Delete')}</Button>
              <Button onClick={this.toggleRemoveFlowModal}>{glp('FlowDefinitions_RemoveFlowModal_Button_Cancel')}</Button>
            </ModalFooter>
          </Modal>

          <Button color={'primary'} onClick={this.toggleNewFlowForm}>{glp('FlowDefinitions_NewFlow_Prompt')}</Button>
          <Collapse isOpen={this.state.isNewFlowFormOpened}>
            <Form id={'new-flow-form'} onSubmit={this.addNewFlow}>
              <FormGroup>
                <Label for={'name'}>{glp('FlowDefinitions_NewFlow_Name')}</Label>
                <Input name={'name'} value={this.state.newFlow.name || ''} title={glp('FlowDefinitions_UsePascalCase')}
                  required minLength={3} maxLength={50} pattern={'[A-Z][a-z]+(?:[A-Z][a-z]+)*'} onChange={event => this.changeNewFlowField(event.target.name, event.target.value)} />
              </FormGroup>
              <FormGroup>
                <Label for={'area'}>{glp('FlowDefinitions_NewFlow_Area')}</Label>
                <Input name={'area'} value={this.state.newFlow.area || ''} title={glp('FlowDefinitions_UsePascalCase')}
                  required minLength={3} maxLength={50} pattern={'[A-Z][a-z]+(?:[A-Z][a-z]+)*'} onChange={event => this.changeNewFlowField(event.target.name, event.target.value)} />
              </FormGroup>
              <Button type={'submit'}>{glp('FlowDefinitions_NewFlow_Submit')}</Button>
            </Form>
          </Collapse>
        </div>
        <br />

        <div id={'flows-list-wrapper'}>

          <Modal id={'spec-modal'} isOpen={this.state.isSpecModalOpen} toggle={this.toggleSpecModal}>
            <ModalHeader toggle={this.toggleSpecModal}>{glp('FlowDefinitions_SpecModal_Header')}</ModalHeader>
            <ModalBody>
              <Button onClick={this.downloadSpecification}>{glp('FlowDefinitions_SpecModal_Download')}</Button><br/><br/>
              {Object.entries(this.state.spec).map(([area, flows]) => <div key={`$area_${area}`}>
                <div><h4>{area}</h4></div>
                {flows.map(flow => <div key={`$flow_${area}_${flow.flowName}`}>
                  <div className={'spec-flow-wrapper'}>
                    <h5 className={'spec-flow-name-header'}>{flow.flowName}</h5>
                    <span className={'spec-flow-expander'} onClick={() => this.toggleSpecCollapse(`${flow.flowName}_${area}`)}><BsChevronDown/></span>
                    <Collapse isOpen={this.state.collapseData.find(datum => datum.flowId === `${flow.flowName}_${area}`).collapsed}>

                      <br />
                      <div>{glp('FlowDefinitions_SpecModal_FlowHttpMethod')}: <strong>{flow.httpMethod}</strong></div>
                      <div>{glp('FlowDefinitions_SpecModal_RequestUrl')}: <strong>{flow.requestUrl}</strong></div>
                      <div>{glp('FlowDefinitions_SpecModal_RequestBodyType')}: <strong>{flow.requestBodyType}</strong></div>
                      {flow.requestBodySchema && <div>{glp('FlowDefinitions_SpecModal_RequestBodySchema')}: <strong>{flow.requestBodySchema}</strong></div>}
                      <div>{glp('FlowDefinitions_SpecModal_ResponseBodySchema')}: <strong>{flow.responseBodySchema}</strong></div>
                      <br />

                      <div>{glp('FlowDefinitions_SpecModal_Schemas')}:</div>
                      <br />

                      <div>
                        {flow.types.map(type =>
                          <div key={`$type_${area}_${flow.flowName}_${type.typeName}`}>
                            <div>{type.typeName}</div>
                            <div>{'{'}</div>
                            <div className={'spec-flow-type-props-wrapper'}>
                              {type.typeProperties.map(prop =>
                                <div key={`prop_${nanoid()}`}>
                                  {prop}
                                </div>)}
                            </div>
                            <div>{'}'}</div>
                            <br />
                          </div>)}
                      </div>
                    </Collapse>
                    <br />
                  </div>
                </div>)}
                <br/>
              </div>)}
            </ModalBody>
          </Modal>

          <legend>{glp('FlowDefinitions_ExistingFlows_Header')}</legend>
          <Button color={'primary'} onClick={this.toggleSpecModal}>{glp('FlowDefinitions_OpenSpecModal')}</Button>
          <br /><br />

          <Table id={'flows-list'} hover>
            <thead>
              <tr>
                <th>{glp('FlowDefinitions_Table_Name')}</th>
                <th>{glp('FlowDefinitions_Table_Area')}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {this.state.flows.map(flow =>
                <tr key={`flowDefinition_${flow.id}`} onClick={() => this.chooseFlowFromList(flow.id)}>
                  <td>{flow.name}</td>
                  <td>{flow.area}</td>
                  <td><Button size={'sm'} color={'danger'} onClick={(event) => this.askForRemoveFlow(event, flow.id)}><FaTrash /></Button></td>
                </tr>)}
            </tbody>
          </Table>
        </div>
      </>
    );
  }
}