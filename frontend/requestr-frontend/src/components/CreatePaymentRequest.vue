<template>
  <div>
    <div v-show="isSelecting">
      <TransactionList v-model="transactions"></TransactionList>
      <div class="has-text-centered">
        <button
          class="button is-medium is-primary"
          v-bind:class="{disabled: +total <= 0}"
          v-bind:disabled="+total <= 0"
          v-on:click="isSelecting=false"
        >Create request</button>
      </div>
    </div>
    <div v-show="!isSelecting">
      <button class="button" v-on:click="isSelecting=true">
        <span class="icon">
          <i class="fa fa-chevron-left"></i>
        </span>
        <span>Change selection</span>
      </button>

      <br />
      <br />

      <div class="field">
        <label class="label">Amount</label>
      </div>
      <div class="field is-horizontal">
        <div class="field-body">
          <div class="field">
            <div class="control has-icons-left">
              <span class="icon is-small is-left">
                <i class="fa fa-eur"></i>
              </span>
              <input class="input is-disabled" disabled type="text" v-bind:value="total.toFixed(2)" />
            </div>
          </div>
          <div class="field">
            <div class="control has-icons-left">
              <span class="icon is-small is-left">&divide;</span>
              <input class="input" type="number" v-model="numberOfPeople" />
            </div>
          </div>
          <div class="field">
            <div class="control has-icons-left">
              <span class="icon is-small is-left">=</span>
              <input
                class="input is-disabled"
                disabled
                type="text"
                v-bind:value="(total/numberOfPeople).toFixed(2)"
              />
            </div>
          </div>
        </div>
      </div>

      <div class="field">
        <label class="label">Description</label>
        <div class="control">
          <textarea class="textarea" v-model="description"></textarea>
        </div>
      </div>

      <div class="field">
        <label class="label">Recipients</label>
        <div class="control has-icons-left">
          <span class="icon is-small is-left">@</span>
          <input class="input" type="email" v-model="mainRecipient" />
        </div>
      </div>
      <div v-for="(param, index) in recipients" :key="index" class="field">
        <div class="control has-icons-left has-icons-right">
          <span class="icon is-small is-left">@</span>
          <input class="input" type="email" v-model="recipients[index]" />
          <span class="icon is-small is-right" v-on:click="removeRecipient(index)">
            <a class="delete"></a>
          </span>
        </div>
      </div>

      <div class="field">
        <div class="control has-text-right">
          <button class="button is-secondary" v-on:click="addRecipient">Add recipient</button>
        </div>
      </div>

      <div class="has-text-centered">
        <button class="button is-medium is-primary" v-on:click="sendPaymentRequest">
          <span class="icon">
            <i class="fa fa-send"></i>
          </span>
          <span>Send payment request</span>
        </button>
      </div>
      {{ message }}
    </div>
  </div>
</template>

<script>
import TransactionList from "./TransactionList";

export default {
  name: "CreatePaymentRequest",
  components: { TransactionList },
  data: function() {
    return {
      transactions: {},
      mainRecipient: '',
      recipients: [],
      description: "",
      isSelecting: true,
      numberOfPeople: 1,
      message: ""
    };
  },
  watch: {
    numberOfPeople: function() {
      if (this.numberOfPeople < 1) {
        this.numberOfPeople = 1;
      }
    }
  },
  computed: {
    total: function() {
      let sum = 0;
      for (let key in this.transactions) {
        sum = sum - this.transactions[key].amount;
      }
      return sum;
    }
  },
  methods: {
    addRecipient: function() {
      this.recipients.push("");
    },
    removeRecipient: function(index) {
      this.recipients.splice(index, 1);
    },
    sendPaymentRequest: async function() {
      let data = {
        description: this.description,
        amount: this.total / this.numberOfPeople,
        currency: "EUR",
        recipients: this.recipients.concat(this.mainRecipient)
      };
      let token = window.localStorage.getItem("token");
      let url = new URL(`${process.env.VUE_APP_API}/api/requests`);
      let response = await fetch(url, {
        method: "POST",
        credentials: "same-origin",
        mode: "cors",
        body: JSON.stringify(data),
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        }
      });
      let result = await response.json();
      if (response.ok) {
        this.message = `Payment request: ${result.link}. E-mail sent: ${result.isMailSent}.`;
      } else {
        console.error(result);
        this.message = result;
      }
    }
  }
};
</script>