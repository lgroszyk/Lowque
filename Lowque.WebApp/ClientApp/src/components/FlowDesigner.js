import React, { Component } from 'react';
import {
  Row, Col, Table, ListGroup, ListGroupItem,
  Form, FormGroup, Input, InputGroup, Label, Button,
  Nav, NavItem, NavLink, TabContent, TabPane,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Spinner
} from 'reactstrap';
import ReactFlow from 'react-flow-renderer';
import { FaTrash, FaPlus } from 'react-icons/fa';
import { nanoid } from 'nanoid';
import { get, post } from '../utils/api';
import './FlowDesigner.css';

export class FlowDesigner extends Component {
  static displayName = FlowDesigner.name;

  constructor(props) {
    super(props);

    this.state = this.createInitialState();

    this.loadGraph = this.loadGraph.bind(this);
    this.changeTab = this.changeTab.bind(this);
    this.saveFlow = this.saveFlow.bind(this);

    this.addNode = this.addNode.bind(this);
    this.selectElement = this.selectElement.bind(this);
    this.moveNode = this.moveNode.bind(this);
    this.connectNodes = this.connectNodes.bind(this);
    this.deleteElements = this.deleteElements.bind(this);

    this.changeFlowType = this.changeFlowType.bind(this);
    this.changeUseResourceIdentifier = this.changeUseResourceIdentifier.bind(this);
    this.changeActionType = this.changeActionType.bind(this);
    this.changeActionLabel = this.changeActionLabel.bind(this);
    this.finishActionLabelChange = this.finishActionLabelChange.bind(this);

    this.beginParameterChange = this.beginParameterChange.bind(this);
    this.finishParameterChange = this.finishParameterChange.bind(this);
    this.changeParameterWithoutPredefinedValues = this.changeParameterWithoutPredefinedValues.bind(this);
    this.changeParameterWithPredefinedValues = this.changeParameterWithPredefinedValues.bind(this);
    this.updateSelectedNodeParameters = this.updateSelectedNodeParameters.bind(this);
    this.fetchActionParameters = this.fetchActionParameters.bind(this);

    this.renderParameterInput = this.renderParameterInput.bind(this);
    this.renderParameterHints = this.renderParameterHints.bind(this);

    this.openTypeEditor = this.openTypeEditor.bind(this);
    this.toggleTypeEditor = this.toggleTypeEditor.bind(this);
    this.submitTypeEditor = this.submitTypeEditor.bind(this);

    this.changeNewTypeName = this.changeNewTypeName.bind(this);
    this.removeType = this.removeType.bind(this);
    this.addNewProperty = this.addNewProperty.bind(this);
    this.removeProperty = this.removeProperty.bind(this);
    this.changeNewPropertyName = this.changeNewPropertyName.bind(this);
    this.changeEditedTypeProperty = this.changeEditedTypeProperty.bind(this);
    this.getAvailablePropertyTypes = this.getAvailablePropertyTypes.bind(this);
  }

  componentDidMount() {
    const flowId = this.props.match.params.id;
    get(`/FlowDesigner/GetFlow/${flowId}`)
      .then(response => response.json())
      .then(data => this.setState({
        ...this.createInitialState(),
        nodesData: data.flowData.nodesData,
        edgesData: data.flowData.edgesData,
        typesData: data.flowData.typesData,
        flowProperties: data.flowProperties,
      }));
  }

  createInitialState() {
    return {
      flowProperties: {}, savingFlow: false,
      nodesData: [], edgesData: [], typesData: [],
      formulaHints: [], availableFormulaHints: [],
      activeTab: 'properties', isTypeEditorOpen: false,
      selectedNode: null, selectedParameter: null,
      newTypeName: null, newPropertyError: null, newPropertyName: null, editedType: null,
    };
  }

  loadGraph(reactFlowInstance) {
    this.setState({ graph: reactFlowInstance });
  }

  changeTab(tab) {
    this.setState({ activeTab: tab });
  }

  addNode() {
    post("/FlowDesigner/CreateNewAction", { flowName: this.state.flowProperties.flowName })
      .then(response => response.json())
      .then(data => {
        const newNode = data.newNode;
        const graphPosition = this.state.graph.toObject().position;
        newNode.position.x = -graphPosition[0] + 5;
        newNode.position.y = -graphPosition[1] + 5;
        this.setState(prevState => ({
          nodesData: [...prevState.nodesData, newNode],
        }));
      });
  }

  moveNode(event, node) {
    this.setState(prevState => ({
      selectedNode: {
        ...prevState.selectedNode,
        position: { x: node.position.x, y: node.position.y }
      },
      nodesData: prevState.nodesData.map(nodeData => nodeData.id === node.id
        ? { ...nodeData, position: { x: node.position.x, y: node.position.y } }
        : nodeData),
    }));
  }

  connectNodes({ source, target }) {
    const sourceNodeOutEdges = this.state.edgesData.filter(edge => edge.source === source);
    const targetNodeInEdges = this.state.edgesData.filter(edge => edge.target === target);
    if (sourceNodeOutEdges.length > 1 || targetNodeInEdges.length > 1) {
      this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_TooManyEdges'));
      return;
    }

    const isSourceNodeOfTypeIfElseStart = this.state.nodesData.find(node => node.id === source).data.type === 'IfElseStart';
    const label = isSourceNodeOfTypeIfElseStart ? (sourceNodeOutEdges.length === 0 ? 'true' : 'false') : undefined;

    const newEdge = {
      id: `${source}:${target}`,
      source: source,
      target: target,
      label: label
    };
    this.setState(prevState => ({
      edgesData: [...prevState.edgesData, newEdge]
    }));
  }

  selectElement(elements) {
    const selectedElement = elements === null ? null : elements[0];
    const isNode = selectedElement?.hasOwnProperty('data');
    const selectedNode = selectedElement && isNode ? selectedElement : null;
    this.setState({ selectedNode: selectedNode, selectedParameter: null });
  }

  deleteElements(elements) {
    if (!elements) {
      return;
    }
    for (let i = 0; i < elements.length; i++) {
      const element = elements[i];
      this.setState(prevState => ({
        nodesData: prevState.nodesData.filter(node => node.id !== element.id),
        edgesData: prevState.edgesData.filter(edge => edge.id !== element.id),
        selectedNode: null,
      }));
    }
  }

  changeFlowType(newType) {
    post('/FlowDesigner/ModifyDtosDueToTypeChange', {
      newType: newType,
      typesData: this.state.typesData,
      flowProperties: this.state.flowProperties
    })
      .then(response => response.json())
      .then(data => {
        this.setState(prevState => ({
          flowProperties: {
            ...prevState.flowProperties,
            flowType: newType
          },
          typesData: data.typesData
        }));
      });
  }

  changeUseResourceIdentifier(useResourceIdentifier) {
    this.setState(prevState => ({
      flowProperties: {
        ...prevState.flowProperties,
        useResourceIdentifier: useResourceIdentifier
      }
    }));
  }

  changeActionLabel(newLabel) {
    this.setState(prevState => ({
      selectedNode: {
        ...prevState.selectedNode,
        data: {
          ...prevState.selectedNode.data,
          label: newLabel
        }
      }
    }));
  }

  finishActionLabelChange() {
    const newLabel = this.state.selectedNode.data.label;
    const isNewActionLabelValid = Boolean(newLabel);
    if (isNewActionLabelValid) {
      this.setState(prevState => ({
        nodesData: prevState.nodesData.map(node => node.id === prevState.selectedNode.id
          ? { ...node, data: { ...node.data, label: newLabel } } : node),
      }));
    }
    else {
      this.setState(prevState => ({ selectedNode: prevState.nodesData.find(node => node.id === prevState.selectedNode.id) }))
      this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_NewActionNameCannotBeEmpty'));
    }
  }

  changeActionType(newType) {
    this.setState(prevState => ({
      selectedNode: { ...prevState.selectedNode, data: { ...prevState.selectedNode.data, type: newType } },
      nodesData: prevState.nodesData.map(node => node.id === prevState.selectedNode.id
        ? { ...node, data: { ...node.data, type: newType } }
        : node),
    }), () => this.fetchActionParameters(null, null));
  }

  getGroupedActions() {
    return [
      { name: 'Controlling', actions: ['IfElseStart', 'IfElseEnd', 'LoopStart', 'LoopEnd', 'Return'] },
      { name: 'Variables', actions: ['CreateVariable', 'ChangeVariable'] },
      { name: 'Records', actions: ['AddRecord', 'EditRecord', 'DeleteRecord', 'GetRecords'] },
      { name: 'Elements', actions: ['AddElement', 'DeleteElement', 'GetElements'] },
      { name: 'Documents', actions: ['UploadDocument', 'DeleteDocument', 'GetDocumentPath'] },
      { name: 'Other', actions: ['GetCurrentUser', 'SendEmail', 'Localize'] }
    ];
  }

  beginParameterChange(parameter) {
    const selectedParameter = `${this.state.selectedNode.id}_${parameter.name}`;
    this.fetchActionParameters(parameter.value, selectedParameter);
  }

  finishParameterChange(parameter) {
    this.fetchActionParameters(parameter.value, null);
  }

  changeParameterWithoutPredefinedValues(paramName, paramValue) {
    this.updateSelectedNodeParameters(paramName, paramValue, false);
  }

  changeParameterWithPredefinedValues(paramName, paramValue) {
    this.updateSelectedNodeParameters(paramName, paramValue, true);
  }

  updateSelectedNodeParameters(parameterName, parameterValue, shouldFetchParameters) {
    this.setState(prevState => ({
      selectedNode: {
        ...prevState.selectedNode,
        data: {
          ...prevState.selectedNode.data,
          parameters: prevState.selectedNode.data.parameters.map(parameter =>
            parameter.name === parameterName
              ? { ...parameter, value: parameterValue, hasChanged: true }
              : parameter)
        },
      },
      nodesData: prevState.nodesData.map(nodeData => nodeData.id === prevState.selectedNode.id
        ? {
          ...nodeData, data: {
            ...nodeData.data, parameters: nodeData.data.parameters.map(
              parameter => parameter.name === parameterName ? { ...parameter, value: parameterValue, hasChanged: true } : parameter)
          }
        }
        : nodeData),
    }), () => {
      if (shouldFetchParameters) {
        this.fetchActionParameters(parameterValue, null);
      }
    });
  }

  fetchActionParameters(searchPhrase, selectedParameter) {
    post('/FlowDesigner/GetParameters', {
      flowProperties: this.state.flowProperties,
      actionId: this.state.selectedNode.id,
      actionType: this.state.selectedNode.data.type,
      flowData: {
        typesData: this.state.typesData,
        edgesData: this.state.edgesData,
        nodesData: this.state.nodesData.map(node => node.id === this.state.selectedNode.id
          ? { ...node, data: { ...node.data, type: this.state.selectedNode.data.type, parameters: this.state.selectedNode.data.parameters } } : node),
      },
    })
      .then(response => response.json())
      .then(data => {
        this.setState(prevState => ({
          selectedNode: prevState.selectedNode
            ? {
              ...prevState.selectedNode,
              data: {
                ...prevState.selectedNode.data,
                type: data.actionType,
                parameters: data.updatedParameters
              }
            }
            : prevState.selectedNode,
          nodesData: prevState.nodesData.map(nodeData => nodeData.id === data.actionId
            ? { ...nodeData, data: { ...nodeData.data, type: data.actionType, parameters: data.updatedParameters } }
            : nodeData),
          selectedParameter: selectedParameter,
          formulaHints: data.formulaHints,
        }));
      })
      .catch(error => {
        this.setState({ selectedParameter: null });
        this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_SaveFlow_GetParametersError'));
      });
  }

  saveFlow() {
    this.setState({ savingFlow: true });
    post('/FlowDesigner/SaveFlow', {
      flowData: {
        nodesData: this.state.nodesData,
        edgesData: this.state.edgesData,
        typesData: this.state.typesData,
      },
      flowProperties: this.state.flowProperties
    })
      .then(response => response.json())
      .then(data => {
        this.setState({ savingFlow: false });
        if (data.success) {
          this.props.global.openSuccessAlert(this.props.global.glp('FlowDesigner_SaveFlow_Success'));
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      })
      .catch(error => {
        this.setState({ savingFlow: false });
        this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_SaveFlow_SaveServerError'));
      });
  }

  renderParameterInput(parameter) {
    if (parameter.areValuesPredefined) {
      return <Input type={'select'} name={parameter.name} value={parameter.value || ''}
        onFocus={() => this.beginParameterChange(parameter)}
        onChange={event => this.changeParameterWithPredefinedValues(event.target.name, event.target.value)}>
        {parameter.predefinedValues.map(predefinedValue =>
          <option key={`${this.state.selectedNode.id}_${parameter.name}_${predefinedValue}`} value={predefinedValue}>
            {predefinedValue}
          </option>)}
      </Input>;
    } else {
      return <Input name={parameter.name} value={parameter.value || ''}
        onChange={event => this.changeParameterWithoutPredefinedValues(event.target.name, event.target.value)}
        onFocus={() => this.beginParameterChange(parameter)}
        onBlur={() => this.finishParameterChange(parameter)} />;
    }
  }

  renderParameterHints(parameter) {
    if (this.state.selectedParameter === `${this.state.selectedNode.id}_${parameter.name}`
      && !parameter.disableHints && !parameter.areValuesPredefined) {
      return <ListGroup className={'formula-hints'}>
        {this.state.formulaHints.map(hint =>
          <ListGroupItem key={`hint_${nanoid()}`} action>
            <div><strong>{hint.name}</strong>{hint.type}</div>
            {hint.properties &&
              <ul>
                {hint.properties.map(prop =>
                  <li key={`hint_prop_${nanoid()}`}>
                    {prop}
                  </li>)}
              </ul>}
          </ListGroupItem>)}
      </ListGroup>;
    }
    return false;
  }

  toggleTypeEditor() {
    this.setState(prevState => ({
      isTypeEditorOpen: !prevState.isTypeEditorOpen,
      newPropertyError: null
    }));
  }

  changeEditedTypeProperty(propName, fieldName, fieldValue) {
    this.setState(prevState => ({
      editedType: {
        ...prevState.editedType,
        properties: prevState.editedType.properties.map(prop =>
          prop.name === propName ? { ...prop, [fieldName]: fieldValue } : prop)
      }
    }));
  }

  changeNewPropertyName(newPropertyName) {
    this.setState({ newPropertyName: newPropertyName });
  }

  addNewProperty() {
    const newPropertyName = this.state.newPropertyName

    if (!newPropertyName) {
      this.setState({ newPropertyError: this.props.global.glp('FlowDesigner_NoNewPropertyName') });
      return;
    }

    const isNameTooLong = newPropertyName.length > 100;
    if (isNameTooLong) {
      this.setState({ newPropertyError: this.props.global.glp('FlowDesigner_NewPropertyNameTooLong') });
      return;
    }

    const isNamePascalCase = this.isPascalCase(newPropertyName);
    if (!isNamePascalCase) {
      this.setState({ newPropertyError: this.props.global.glp('FlowDesigner_NewPropertyNameNotPascalCase') });
      return;
    }

    const isNameNotUnique = this.state.editedType.properties.some(prop => prop.name === newPropertyName);
    if (isNameNotUnique) {
      this.setState({ newPropertyError: this.props.global.glp('FlowDesigner_NewPropertyNameNotUnique') });
      return;
    }

    this.setState(prevState => ({
      editedType: {
        ...prevState.editedType,
        properties: [...prevState.editedType.properties, {
          name: prevState.newPropertyName,
          type: 'IntegralNumber',
          required: false,
          list: false,
          nullable: false,
          maxLength: null,
        }]
      },
      newPropertyName: null,
      newPropertyError: null
    }));
  }

  getAvailablePropertyTypes() {
    const glp = this.props.global.glp;
    const commonTypes = [
      { name: glp('FlowDesigner_SimpleType_Binary'), value: 'Binary' },
      { name: glp('FlowDesigner_SimpleType_IntegralNumber'), value: 'IntegralNumber' },
      { name: glp('FlowDesigner_SimpleType_RealNumber'), value: 'RealNumber' },
      { name: glp('FlowDesigner_SimpleType_TextPhrase'), value: 'TextPhrase' },
      { name: glp('FlowDesigner_SimpleType_DateAndTime'), value: 'DateAndTime' },
      { name: glp('FlowDesigner_SimpleType_File'), value: 'File' }
    ];
    const customTypes = this.state.typesData.map(type =>
      ({ name: type.name, value: type.name }));
    return [...commonTypes, ...customTypes];
  }

  changeNewTypeName(newTypeName) {
    this.setState({ newTypeName: newTypeName });
  }

  openTypeEditor(existingTypeName) {
    const newTypeName = this.state.newTypeName;
    if (!existingTypeName && !newTypeName) {
      this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_NoNewTypeName'));
      return;
    }

    if (newTypeName) {
      const isNameTooLong = newTypeName.length > 100;
      if (isNameTooLong) {
        this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_NewTypeNameTooLong'));
        return;
      }

      const isNamePascalCase = this.isPascalCase(newTypeName);
      if (!isNamePascalCase) {
        this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_NewTypeNameNotPascalCase'));
        return;
      }

      const isNameNotUnique = this.state.typesData.some(type => type.name === newTypeName);
      if (isNameNotUnique) {
        this.props.global.openDangerAlert(this.props.global.glp('FlowDesigner_NewTypeNameNotUnique'));
        return;
      }
    }

    const typeToEdit = existingTypeName
      ? this.state.typesData.find(type => type.name === existingTypeName)
      : { name: this.state.newTypeName, preventDelete: false, preventEdit: false, properties: [] };
    this.setState({
      newTypeName: null,
      editedType: typeToEdit,
      isTypeEditorOpen: true
    });
  }

  isPascalCase(str) {
    return /^[A-Z][a-z]+(?:[A-Z][a-z]+)*$/.test(str);
  }

  submitTypeEditor() {
    this.setState(prevState => ({
      typesData: [prevState.editedType, ...prevState.typesData.filter(type => type.name !== prevState.editedType.name)],
      isTypeEditorOpen: false,
    }));
  }

  removeType(event, typeName) {
    event.stopPropagation();
    this.setState(prevState => ({
      typesData: prevState.typesData.filter(type => type.name !== typeName)
    }));
  }

  removeProperty(propName) {
    this.setState(prevState => ({
      editedType: {
        name: prevState.editedType.name,
        properties: prevState.editedType.properties.filter(prop => prop.name !== propName)
      }
    }))
  }

  render() {
    const glp = this.props.global.glp;

    return (
      <>
        <Row>
          <Col md={6}>
            <div id={'save-button-wrapper'}>
              <Button id={'save-button'} onClick={this.saveFlow} disabled={this.state.savingFlow}>{glp('FlowDesigner_SaveFlow')}</Button>
              {this.state.savingFlow && <Spinner />}
            </div>
            <br />

            <div id={'flow-wrapper-header'}>
              <Button size={'sm'} color={'success'} onClick={this.addNode}><FaPlus /></Button><br />
              <span><strong>{glp('FlowDesigner_DiagramHeader')}</strong></span>
            </div>
            <div id={'flow-wrapper'}>
              <ReactFlow
                elements={[...this.state.nodesData, ...this.state.edgesData]}
                nodesDraggable={true}
                nodesConnectable={true}
                onLoad={this.loadGraph}
                onConnect={this.connectNodes}
                onSelectionChange={this.selectElement}
                onNodeDragStop={this.moveNode}
                onElementsRemove={this.deleteElements}
                minZoom={1}
                maxZoom={1}
              />
            </div>
          </Col>

          <Col md={6}>
            <Nav id={'flow-designer-tabs'} tabs>
              <NavItem>
                <NavLink className={this.state.activeTab === 'properties' ? 'active' : ''} onClick={() => this.changeTab('properties')}>
                  {glp('FlowDesigner_Tab_Properties')}
                </NavLink>
              </NavItem>
              <NavItem>
                <NavLink className={this.state.activeTab === 'types' ? 'active' : ''} onClick={() => this.changeTab('types')}>
                  {glp('FlowDesigner_Tab_Types')}
                </NavLink>
              </NavItem>
            </Nav>

            <TabContent activeTab={this.state.activeTab}>
              <TabPane tabId={'properties'}>
                <br />
                {this.state.selectedNode &&
                  <Form autoComplete={'off'}>
                    <legend>{glp('FlowDesigner_ActionProperties_Header')}</legend>

                    <FormGroup>
                      <Label htmlFor={'label'}>{glp('FlowDesigner_ActionProperties_ActionName')}</Label>
                      <Input name={'label'} value={this.state.selectedNode.data.label}
                        onChange={event => this.changeActionLabel(event.target.value)} onBlur={this.finishActionLabelChange} />
                    </FormGroup>

                    <FormGroup>
                      <Label htmlFor={'type'}>{glp('FlowDesigner_ActionProperties_ActionType')}</Label>
                      <Input name={'type'} value={this.state.selectedNode.data.type}
                        type={'select'} onChange={event => this.changeActionType(event.target.value)}>

                        {this.getGroupedActions().map(group =>
                          <optgroup key={`ActionsGroup_${group.name}`} label={glp(`FlowDesigner_ActionsGroup_${group.name}`)}>
                            {group.actions.map(action =>
                              <option key={`ActionType_${action}`} value={action}>
                                {glp(`FlowDesigner_ActionType_${action}`)}
                              </option>)}
                          </optgroup>)}

                      </Input>
                    </FormGroup>

                    <hr />

                    {this.state.selectedNode.data.parameters.map(parameter =>
                      <FormGroup key={`${this.state.selectedNode.id}_${parameter.name}`}>
                        <Label htmlFor={parameter.name}>
                          <strong>{parameter.displayName}</strong> ({parameter.displayDescription})
                        </Label>
                        {this.renderParameterInput(parameter)}
                        {this.renderParameterHints(parameter)}
                      </FormGroup>)}
                  </Form>}

                {!this.state.selectedNode &&
                  <Form>
                    <legend>{glp('FlowDesigner_FlowProperties_Header')}</legend>

                    <FormGroup>
                      <Label>{glp('FlowDesigner_FlowProperties_Name')}</Label>
                      <Input defaultValue={this.state.flowProperties?.flowName || ''} disabled />
                    </FormGroup>

                    <FormGroup>
                      <Label>{glp('FlowDesigner_FlowProperties_Area')}</Label>
                      <Input defaultValue={this.state.flowProperties?.flowArea || ''} disabled />
                    </FormGroup>

                    <FormGroup>
                      <Label for={'flowType'}>{glp('FlowDesigner_FlowProperties_Type')}</Label>
                      <Input name={'flowType'} type={'select'} value={this.state.flowProperties?.flowType || ''} onChange={event => this.changeFlowType(event.target.value)}>
                        <option value={'GetResource'}>{glp('FlowDesigner_FlowProperties_Type_GetResource')}</option>
                        <option value={'ModifyResource'}>{glp('FlowDesigner_FlowProperties_Type_ModifyResource')}</option>
                        <option value={'DownloadFile'}>{glp('FlowDesigner_FlowProperties_Type_DownloadFile')}</option>
                        <option value={'UploadFile'}>{glp('FlowDesigner_FlowProperties_Type_UploadFile')}</option>
                      </Input>
                    </FormGroup>

                    {this.state?.flowProperties?.flowType === 'GetResource' &&
                      <FormGroup check>
                        <Input name={'useResourceIdentifier'} type={'checkbox'} checked={this.state.flowProperties?.useResourceIdentifier || ''}
                          onChange={event => this.changeUseResourceIdentifier(event.target.checked)} />
                        <Label for={'useResourceIdentifier'}>{glp('FlowDesigner_FlowProperties_UseResourceIdentifier')}</Label>
                      </FormGroup>}
                  </Form>}
              </TabPane>

              <TabPane tabId={'types'}>

                <Modal id={'type-modal'} isOpen={this.state.isTypeEditorOpen} toggle={this.toggleTypeEditor}>
                  <ModalHeader toggle={this.toggleTypeEditor}>{glp('FlowDesigner_Modal_Header')}</ModalHeader>
                  <ModalBody>
                    {this.state.editedType && <>
                      <h4>{this.state.editedType.name}</h4><br />
                      {this.state.newPropertyError && <div className={'modal-error'}>{this.state.newPropertyError}</div>}
                      {!this.state?.editedType?.preventEdit &&
                        <InputGroup>
                          <Input className={'types-name-input'} value={this.state.newPropertyName || ''}
                            onChange={event => this.changeNewPropertyName(event.target.value)} />
                          <Button onClick={this.addNewProperty}>{glp('FlowDesigner_Modal_AddNewProp')}</Button>
                        </InputGroup>}
                      <br />
                      <Table id={'props-list'}>
                        <thead>
                          <tr>
                            <th>{glp('FlowDesigner_Modal_List_Name')}</th>
                            <th>{glp('FlowDesigner_Modal_List_Type')}</th>
                            <th>{glp('FlowDesigner_Modal_List_IsList')}</th>
                            <th>{glp('FlowDesigner_Modal_List_IsRequired')}</th>
                            <th>{glp('FlowDesigner_Modal_List_IsNullable')}</th>
                            <th>{glp('FlowDesigner_Modal_List_MaxLength')}</th>
                            <th></th>
                          </tr>
                        </thead>
                        <tbody>
                          {this.state.editedType.properties.map(prop =>
                            <tr key={`${this.state.editedType.name}_${prop.name}`}>
                              <td>
                                <Input defaultValue={prop.name} disabled />
                              </td>
                              <td>
                                <Input type={'select'} value={prop.type} disabled={this.state.editedType.preventEdit}
                                  onChange={event => this.changeEditedTypeProperty(prop.name, 'type', event.target.value)}>
                                  {this.getAvailablePropertyTypes().map(propType =>
                                    <option key={`proptype_${propType.value}`} value={propType.value}>
                                      {propType.name}
                                    </option>)}
                                </Input>
                              </td>
                              <td>
                                <Input type={'select'} value={prop.list} disabled={this.state.editedType.preventEdit}
                                  onChange={event => this.changeEditedTypeProperty(prop.name, 'list', event.target.value === 'true')}>
                                  <option value={false}>{glp('FlowDesigner_False')}</option>
                                  <option value={true}>{glp('FlowDesigner_True')}</option>
                                </Input>
                              </td>
                              <td>
                                <Input type={'select'} value={prop.required} disabled={this.state.editedType.preventEdit}
                                  onChange={event => this.changeEditedTypeProperty(prop.name, 'required', event.target.value === 'true')}>
                                  <option value={false}>{glp('FlowDesigner_False')}</option>
                                  <option value={true}>{glp('FlowDesigner_True')}</option>
                                </Input>
                              </td>
                              <td>
                                {['Binary', 'IntegralNumber', 'RealNumber', 'DateAndTime'].includes(prop.type) &&
                                  <Input type={'select'} value={prop.nullable} disabled={this.state.editedType.preventEdit}
                                    onChange={event => this.changeEditedTypeProperty(prop.name, 'nullable', event.target.value === 'true')}>
                                    <option value={false}>{glp('FlowDesigner_False')}</option>
                                    <option value={true}>{glp('FlowDesigner_True')}</option>
                                  </Input>}
                              </td>
                              <td>
                                {prop.type === 'TextPhrase' && <Input type={'number'} value={prop.maxLength || ''} disabled={this.state.editedType.preventEdit}
                                  onChange={event => this.changeEditedTypeProperty(prop.name, 'maxLength', event.target.value)} />}
                              </td>
                              <td>
                                {!this.state.editedType.preventEdit && <Button size={'sm'} color={'danger'} onClick={() => this.removeProperty(prop.name)}><FaTrash /></Button>}
                              </td>
                            </tr>)}
                        </tbody>
                      </Table>
                    </>}

                  </ModalBody>
                  <ModalFooter>
                    {!this.state?.editedType?.preventEdit && <Button type={'submit'} onClick={this.submitTypeEditor}>{glp('FlowDesigner_Modal_Submit')}</Button>}
                  </ModalFooter>
                </Modal>

                <br />
                <legend>{glp('FlowDesigner_Types_Header')}</legend>

                <InputGroup>
                  <Input className={'types-name-input'} value={this.state.newTypeName || ''} onChange={event => this.changeNewTypeName(event.target.value)} />
                  <Button onClick={() => this.openTypeEditor(null)}>{glp('FlowDesigner_Types_AddNew')}</Button>
                </InputGroup>
                <br />

                <Table id={'types-list'} hover>
                  <thead>
                    <tr>
                      <th>{glp('FlowDesigner_Types_List_Name')}</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {this.state.typesData.map(type =>
                      <tr key={`type_${type.name}`} onClick={() => this.openTypeEditor(type.name)}>
                        <td>{type.name}</td>
                        <td align={'right'}>
                          {!type.preventDelete &&
                            <Button size={'sm'} color={'danger'} onClick={event => this.removeType(event, type.name)}>
                              <FaTrash />
                            </Button>}
                        </td>
                      </tr>)}
                  </tbody>
                </Table>

              </TabPane>

            </TabContent>
          </Col>
        </Row>
      </>);
  }
}
