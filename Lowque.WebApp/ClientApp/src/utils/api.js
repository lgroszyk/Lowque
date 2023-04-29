const getJwt = () => {
  return localStorage.getItem('jwt');
}

const get = (path) => fetch(path, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${getJwt()}`
  },
});

const post = (path, data) => fetch(path, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json; charset=utf-8',
    'Authorization': `Bearer ${getJwt()}`
  },
  body: JSON.stringify(data),
});

const put = (path, id, data) => fetch(`${path}/${id}`, {
  method: 'PUT',
  headers: {
    'Content-Type': 'application/json; charset=utf-8',
    'Authorization': `Bearer ${getJwt()}`
  },
  body: JSON.stringify(data),
});

const remove = (path, id) => fetch(`${path}/${id}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${getJwt()}`
  },
});

export { get, post, put, remove };
