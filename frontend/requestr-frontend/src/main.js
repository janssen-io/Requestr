import Vue from 'vue'
import VueRouter from 'vue-router';

import Login from './components/Login'
import UpdateKey from './components/UpdateKey'
import CreatePaymentRequest from './components/CreatePaymentRequest'
import ViewPaymentRequest from './components/ViewPaymentRequest'
import App from './App'

Vue.use(VueRouter);
Vue.config.productionTip = false

const router = new VueRouter({
  mode: 'history',
  routes: [
    { path: '/', component: Login, name: 'Login' },
    { path: '/app/login', component: Login },
    { path: '/app/apikey', component: UpdateKey, name: 'UpdateKey' },
    { path: '/app/transactions', component: CreatePaymentRequest, name: 'CreateRequest' },
    { path: '/app/transactions/:id', component: ViewPaymentRequest, props: true, name: 'ViewRequest' },
  ]
});

function parseJwt (token) {
  if (!token) {
    return null;
  }

  var base64Url = token.split('.')[1];
  var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
  }).join(''));

  return JSON.parse(jsonPayload);
}

function hasValidToken() {
  let token = parseJwt(window.localStorage.getItem('token'));
  let now = new Date();

  return token 
    && new Date(token.exp * 1000) >= now
    && token.role.length > 0;
}

router.beforeEach((to, from, next) => {
  if (to.name == 'Login' || hasValidToken()) {
    next();
  } else {
    next({ name: 'Login' })
  }
});

new Vue({
  router: router,
  render: h => h(App)
}).$mount('#app')
