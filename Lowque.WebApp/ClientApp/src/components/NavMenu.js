import React, { Component } from 'react';
import { Collapse, Container, Nav, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import { logout } from '../utils/identity';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor(props) {
    super(props);

    this.state = {
      collapsed: true
    };

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.clickLogout = this.clickLogout.bind(this);
    this.renderMenuItems = this.renderMenuItems.bind(this);
  }

  toggleNavbar() {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  clickLogout() {
    logout();
    this.props.global.setIsAuthenticated(false);
  }

  renderMenuItems() {
    const glp = this.props.global.glp;
    return this.props.global.isAuthenticated
      ? <>
        <NavItem>
          <NavLink tag={Link} className="text-dark" to="/app-configuration">{glp('Menu_Configuration')}</NavLink>
        </NavItem>
        <NavItem>
          <NavLink tag={Link} className="text-dark" to="/profile">{glp('Menu_Profile')}</NavLink>
        </NavItem>
        <NavItem>
          <NavLink tag={Link} className="text-dark" to="#" onClick={this.clickLogout}>{glp('Menu_Logout')}</NavLink>
        </NavItem>
      </>
      : <>
        <NavItem>
          <NavLink tag={Link} className="text-dark" to="/login">{glp('Menu_Login')}</NavLink>
        </NavItem>
      </>
  }

  render() {
    const glp = this.props.global.glp;
    return (
      <Navbar className={'navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3'} light>
        <Container fluid={true}>
          <NavbarBrand tag={Link} to="/">{glp('Menu_AppName')}</NavbarBrand>
          <NavbarToggler onClick={this.toggleNavbar} className={'mr-2'} />
          <Collapse className={'d-sm-inline-flex flex-sm-row-reverse'} isOpen={!this.state.collapsed} navbar>
            <Nav className={'navbar-nav flex-grow'}>
              {this.renderMenuItems()}
            </Nav>
          </Collapse>
        </Container>
      </Navbar>
    );
  }
}
