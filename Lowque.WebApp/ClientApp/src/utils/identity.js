import jwt_decode from 'jwt-decode';

const login = (jwt) => {
  localStorage.setItem('jwt', jwt);
};

const logout = () => {
  localStorage.removeItem('jwt');
}

const isAuthenticated = () => {
  const jwt = localStorage.getItem('jwt');
  if (!jwt) {
    return false;
  }
  const currentDate = new Date();
  const jwtExpirationDate = new Date(0);
  jwtExpirationDate.setUTCSeconds(parseInt(jwt_decode(jwt).exp));
  return currentDate < jwtExpirationDate;
}

const isInRole = (role) => {
  if (!isAuthenticated()) {
    return false;
  }

  const jwt = localStorage.getItem('jwt');
  const currentUserRoles = jwt_decode(jwt).roles;
  return Array.isArray(currentUserRoles)
    ? currentUserRoles.includes(role)
    : currentUserRoles === role;
}

const getUsername = () => {
  const jwt = localStorage.getItem('jwt');
  if (!jwt) {
    return '';
  }

  return jwt_decode(jwt).sub;
}

export { login, logout, isAuthenticated, isInRole, getUsername };
