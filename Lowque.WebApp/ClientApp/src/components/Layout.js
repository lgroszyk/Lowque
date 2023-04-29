import React, { Component } from 'react';
import { Container, Alert, Spinner } from 'reactstrap';
import { NavMenu } from './NavMenu';
import './Layout.css';

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      this.props.global.isLocalizationLoaded
        ? <>
          <header>
            <NavMenu global={this.props.global} />
          </header>
          <main>
            <Container fluid>
              <Alert color={this.props.global.alert.state.color}
                isOpen={this.props.global.alert.state.visible}
                toggle={this.props.global.closeAlert}>
                {this.props.global.alert.state.message}
              </Alert>
              {this.props.children}
            </Container>
          </main>
        </>
        : <>
          <div id={'spinners-wrapper'}>
            <Spinner color={'dark'} type={'grow'} />
            <Spinner color={'dark'} type={'grow'} />
            <Spinner color={'dark'} type={'grow'} />
            <Spinner color={'dark'} type={'grow'} />
            <Spinner color={'dark'} type={'grow'} />
          </div>
        </>
    );
  }
}
