import Vue from 'vue'
import VueRouter from 'vue-router';

import Login from './components/Login'
import UpdateKey from './components/UpdateKey'
import CreatePaymentRequest from './components/CreatePaymentRequest'
import App from './App'

Vue.use(VueRouter);
Vue.config.productionTip = false

const router = new VueRouter({
  routes: [
    { path: '/', component: Login },
    { path: '/app/login', component: Login },
    { path: '/app/apikey', component: UpdateKey },
    { path: '/app/transactions', component: CreatePaymentRequest },
  ]
})

new Vue({
  router: router,
  render: h => h(App)
}).$mount('#app')
