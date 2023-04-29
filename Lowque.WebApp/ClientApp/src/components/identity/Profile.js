import React, { Component } from 'react';
import { Form, FormGroup, FormText, Label, Input, Button } from 'reactstrap'
import { getUsername } from '../../utils/identity';
import { post } from '../../utils/api';
import './Profile.css';

export class Profile extends Component {
  static displayName = Profile.name;

  constructor(props) {
    super(props);
    this.state = {
      passwordData: {}
    };
    this.changeFieldValue = this.changeFieldValue.bind(this);
    this.changePassword = this.changePassword.bind(this);
  }

  changeFieldValue(name, value) {
    this.setState(prevState => ({ passwordData: { ...prevState.passwordData, [name]: value } }));
  }

  changePassword(event) {
    event.preventDefault();
    post('/Identity/ChangePassword', this.state.passwordData)
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          this.setState({ passwordData: {} });
          this.props.global.openSuccessAlert(this.props.global.glp('Profile_Change_Success'));
        } else {
          this.props.global.openDangerAlert(data.error);
        }
      });
  }

  render() {
    const glp = this.props.global.glp;

    return (
      <Form id={'change-password-form'} onSubmit={this.changePassword}>
        <FormGroup>
          <Label>{glp('Profile_Login')}</Label>
          <Input defaultValue={getUsername()} disabled />
        </FormGroup>

        <FormGroup>
          <Label for={'currentPassword'}>{glp('Profile_CurrentPassword')}</Label>
          <Input name={'currentPassword'} value={this.state.passwordData.currentPassword || ''} type={'password'} required maxLength={50}
            onChange={event => this.changeFieldValue(event.target.name, event.target.value)} />
        </FormGroup>

        <FormGroup>
          <Label for={'newPassword'}>{glp('Profile_NewPassword')}</Label>
          <Input name={'newPassword'} value={this.state.passwordData.newPassword || ''} type={'password'} required maxLength={50}
            pattern={'^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*_=+-]).{8,}$'}
            onChange={event => this.changeFieldValue(event.target.name, event.target.value)} />
          <FormText>{glp('Profile_PasswordRules')}</FormText>
        </FormGroup>

        <FormGroup>
          <Label for={'newPasswordConfirmation'}>{glp('Profile_NewPasswordConfirmation')}</Label>
          <Input name={'newPasswordConfirmation'} value={this.state.passwordData.newPasswordConfirmation || ''} type={'password'} required maxLength={50}
            pattern={'^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*_=+-]).{8,}$'}
            onChange={event => this.changeFieldValue(event.target.name, event.target.value)} />
        </FormGroup>

        <Button type={'submit'}>{glp('Profile_Submit')}</Button>
      </Form>
    );
  }
}
