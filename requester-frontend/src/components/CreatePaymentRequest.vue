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
        >
          <span>Create request</span>
          <span class="icon">
            <i class="fa fa-chevron-right"></i>
          </span>
        </button>
      </div>
    </div>
    <div v-show="!isSelecting">
      <button class="button" v-on:click="isSelecting=true">
        <span class="icon">
          <i class="fa fa-chevron-left"></i>
        </span>
        <span>Change selection</span>
      </button>

      <TransactionRow
        v-for="t in transactions" :key="t.id"
        v-bind:value="t"
        v-bind:modelValue="true"
        v-bind:disabled="true"
        ></TransactionRow>

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
          <textarea class="textarea" maxlength="140" rows="2" v-model="description"></textarea>
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

      <div class="field">
        <div class="control">
          <label class="checkbox">
            <input type="checkbox" v-model="withStatement" />
            Add bank statement
          </label>
        </div>
      </div>

      <div 
        class="notification" 
        v-show="message" 
        v-bind:class="{ 'is-success': success && isMailSent, 'is-warning': success && !isMailSent, 'is-danger': !success }">
        <p v-show="success && !isMailSent">
          &nbsp; Unfortunately, the e-mail could not be sent. Please copy and share the link manually.
        </p>
        {{ message }}
      </div>
      <div class="has-text-centered" v-show="!success">
        <button class="button is-medium is-primary" v-bind:class="{ 'is-loading': isLoading }" v-on:click="sendPaymentRequest">
          <span class="icon">
            <i class="fa fa-send"></i>
          </span>
          <span>Send payment request</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import TransactionList from "./TransactionList";
import TransactionRow from "./TransactionRow";

export default {
  name: "CreatePaymentRequest",
  components: { TransactionList, TransactionRow },
  data: function() {
    return {
      transactions: {},
      mainRecipient: '',
      recipients: [],
      description: "",
      isSelecting: true,
      numberOfPeople: 1,
      withStatement: false,
      message: "",
      isMailSent: false,
      success: false,
      isLoading: false
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
      this.message = "";
      this.isLoading = true;
      let data = {
        description: this.description,
        amount: Math.round(this.total / this.numberOfPeople * 100) / 100, // bunq requires two digits
        currency: "EUR",
        recipients: this.recipients.filter(Boolean).concat(this.mainRecipient),
        withStatement: true,
        transactions: this.transactions
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
      this.isLoading = false;
      if (response.ok) {
        let result = await response.json();
        this.message = `Your payment request is located at: ${result.link}.`;
        this.success = true;
        this.isMailSent = result.isMailSent;
      } else {
        let result = await response.text();
        this.message = result;
        this.success = false;
      }
    }
  }
};
</script>