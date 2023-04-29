import React, { Component } from 'react';
import { Form, FormGroup, Label, Input, Button } from 'reactstrap';
import { login } from '../../utils/identity';
import { post } from '../../utils/api';
import './Login.css';

export class Login extends Component {
  static displayName = Login.name;

  constructor(props) {
    super(props);

    this.state = {
      username: '',
      password: '',
    };

    this.changeUsername = this.changeUsername.bind(this);
    this.changePassword = this.changePassword.bind(this);
    this.login = this.login.bind(this);
  }

  changeUsername(newUsername) {
    this.setState({ username: newUsername });
  }

  changePassword(newPassword) {
    this.setState({ password: newPassword });
  }

  login() {
    post('/Identity/Login', {
      email: this.state.username,
      password: this.state.password
    })
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          login(data.jwt);
          this.props.global.setIsAuthenticated(true);
          this.props.global.openSuccessAlert(this.props.global.glp('Login_Success'));
          this.props.history.push(`/`);
        } else {
          this.props.global.openDangerAlert(this.props.global.glp('Login_Failed'));
        }
      });
  }

  render() {
    const glp = this.props.global.glp;

    return (
      <Form id={'login-form'}>
        <FormGroup>
          <Label htmlFor={'login-username'}>{glp('Login_Username')}</Label>
          <Input id={'login-username'} name={'login-username'} value={this.state.username}
            onChange={event => this.changeUsername(event.target.value)} />
        </FormGroup>
        <FormGroup>
          <Label htmlFor={'login-password'}>{glp('Login_Password')}</Label>
          <Input id={'login-password'} name={'login-password'} type={'password'} value={this.state.password}
            onChange={event => this.changePassword(event.target.value)} />
        </FormGroup>
        <Button id={'login-submit'} onClick={this.login}>{glp('Login_Submit')}</Button>
      </Form>
    );
  }
}
