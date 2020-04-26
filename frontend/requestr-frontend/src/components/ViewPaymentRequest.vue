<template>
  <div>
    <h2 class="title is-size-3">View Payment Request</h2>
    <section v-if="!paymentRequest.amount">
      <div v-show="!isCodeSent">
        <p class="subtitle is-size-5">Please enter your e-mail address to receive the pass code.</p>
        <div class="field has-addons">
          <div class="control has-icons-left">
            <span class="icon is-small is-left"> @ </span>
            <input 
              class="input" 
              v-bind:class="{ 'is-disabled': isCodeSent }"
              type="text" 
              v-model="viewerEmail" />
          </div>
          <div class="control">
            <button class="button is-primary" v-on:click="getCode">Send the code</button>
          </div>
        </div>
      </div>
      <div v-show="isCodeSent">
        <p class="subtitle is-size-5">Enter the code from your inbox.</p>
        <div class="field has-addons">
          <div class="control has-icons-left">
            <span class="icon is-small is-left">
              <i class="fa fa-key"></i>
            </span>
            <input 
              class="input" 
              type="text" 
              v-model="oneTimePassword" />
          </div>
          <div class="control">
            <button class="button is-primary" v-on:click="getRequest">View request</button>
          </div>
        </div>
      </div>
  </section>
  <section v-if="paymentRequest.amount">
    <div class="level">
      <div class="level-left">
        <div class="level-item">
          <p><b class="is-size-7">Amount:</b> &euro; {{paymentRequest.amount.toFixed(2)}}</p>
        </div>
      </div>
      <div class="level-right">
        <div class="level-item">
          <p>
            <b class="is-size-7">Link:</b>
            <a v-bind:href="paymentRequest.link">{{paymentRequest.link}}</a>
          </p>
        </div>
      </div>
    </div>
  </section>
  </div>
</template>

<script>
export default { 
  props: ['id'],
  name: 'ViewPaymentRequest',
  components: { },
  data: function() {
    return {
      isCodeSent: false,
      paymentRequest: {},
      message: '',
      oneTimePassword: '',
      viewerEmail: ''
    }
  },
  methods: {
    getCode: async function () {
      this.isLoading = true;
      let data = {
        email: this.viewerEmail
      };
      let url = new URL(`${process.env.VUE_APP_API}/api/requests/${this.id}/code`);
      let authResult = await fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        mode: 'cors',
        body: JSON.stringify(data),
        headers: { 'Content-Type': 'application/json' },
      });
      this.isLoading = false;
      if (authResult.ok) {
        this.isCodeSent = true;
      }
      else {
        this.message = "Invalid e-mail address.";
      }
    },
    getRequest: async function () {
      this.isLoading = true;
      let data = {
        oneTimePassword: this.oneTimePassword
      };
      let url = new URL(`${process.env.VUE_APP_API}/api/requests/${this.id}`);
      Object.keys(data).forEach(key => url.searchParams.append(key, data[key]))
      let result = await fetch(url, {
        method: 'GET',
        credentials: 'same-origin',
        mode: 'cors',
        headers: { 'Content-Type': 'application/json' },
      });
      var response = await result.json();
      this.isLoading = false;
      if (result.ok) {
        this.paymentRequest = response;
      }
      else {
        this.message = response;
      }
    }
  }
}
</script>