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
  routes: [
    { path: '/', component: Login },
    { path: '/app/login', component: Login },
    { path: '/app/apikey', component: UpdateKey },
    { path: '/app/transactions', component: CreatePaymentRequest },
    { path: '/app/transactions/:id', component: ViewPaymentRequest, props: true },
  ]
})

new Vue({
  router: router,
  render: h => h(App)
}).$mount('#app')
