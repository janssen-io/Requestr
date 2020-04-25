<template>
    <div>
      <div v-if="isSelecting">
        <TransactionList v-model="transactions"></TransactionList>
        <button v-on:click="isSelecting = false">Create request</button>
      </div>
      <div v-else>
        <div>Total: {{ total }}</div>
        <input type="email" v-model="recipient">
        <input type="number" v-model="numberOfPeople">
        <textarea v-model="description"></textarea>
        <button v-on:click="isSelecting = true">Change selection</button>
        <button v-on:click="sendPaymentRequest">Send payment request</button>
        {{ message }}
      </div>
    </div>
</template>

<script>
import TransactionList from './TransactionList';

export default { 
  name: 'CreatePaymentRequest',
  components: { TransactionList },
  data: function() {
    return {
      transactions: [],
      recipient: "",
      description: "",
      isSelecting: true,
      numberOfPeople: 1,
      message: ""
    }
  },
  watch: {
    numberOfPeople: function() {
      if (this.numberOfPeople < 1){
        this.numberOfPeople = 1;
      }
    }
  },
  computed: {
      total: function() { 
          let sum = this.transactions.reduce((acc, curr) => acc - curr.amount, 0)
          let dividedSum = sum / this.numberOfPeople; 
          return dividedSum.toFixed(2);
        }
  },
  methods: {
    sendPaymentRequest: async function () {
      let data = {
        description: this.description,
        amount: +this.total,
        currency: "EUR",
        toEmail: this.recipient
      };
      let token = window.localStorage.getItem('token');
      let url = new URL(`${process.env.VUE_APP_API}/api/requests`);
      let response = await fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        mode: 'cors',
        body: JSON.stringify(data),
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        }
      });
      let result = await response.json();
      if (response.ok) {
        this.message = `Payment request: ${result.link}. E-mail sent: ${result.isMailSent}.`;
      }
      else {
        console.error(result);
        this.message = result;
      }
    }
  }
}
</script>