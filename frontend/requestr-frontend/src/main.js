import Vue from 'vue'
import VueRouter from 'vue-router';

import Login from './components/Login'
import UpdateKey from './components/UpdateKey'
import CreatePaymentRequest from './components/CreatePaymentRequest'

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
  router: router
}).$mount('#app')
