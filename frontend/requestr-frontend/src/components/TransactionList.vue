<template>
    <div>
      <div class="field is-horizontal">
        <div class="field-label is-normal">
          <label>Range</label>
        </div>
        <div class="field-body">
          <div class="field">
            <input class="input" type="date" v-model="from">
          </div>
          <div class="field">
            <input class="input" type="date" v-model="upTo">
          </div>
          <div class="field">
            <button class="button is-primary" v-bind:class="{ 'is-loading': isLoading }" type="submit" v-on:click="fetchTransactions">Search</button>
          </div>
        </div>
      </div>
        <ul id="transaction-list">
            <TransactionRow
              v-for="t in transactions" :key="t.id"
              v-bind:value="t"
              v-model="selectedTransactions"
              ></TransactionRow>
        </ul>
    </div>
</template>

<script>
import TransactionRow from './TransactionRow';

export default { 
  name: 'TransactionList',
  prop: ['value'],
  components: { TransactionRow },
  data: function() {
    return {
        from: this.addDays(new Date(), -7).toISOString().substring(0, 10),
        upTo: new Date().toISOString().substring(0, 10),
        transactions: [],
        selectedTransactions: [],
        isLoading: false
    }
  },
  watch: {
    selectedTransactions: function () {
      this.emitSelection();
    }
  },
  methods: {
      addDays: function (date, days) {
        var result = new Date(date);
        result.setDate(result.getDate() + days);
        return result;
      },
      emitSelection: function () {
        this.$emit('input', this.selectedTransactions);
      },
      fetchTransactions: async function() {
        this.isLoading = true;
        this.selectedTransactions = [];
        let data = {
          from: this.from,
          upTo: this.upTo
        }
        let token = window.localStorage.getItem('token');
        let url = new URL(`${process.env.VUE_APP_API}/api/transactions`);
        Object.keys(data).forEach(key => url.searchParams.append(key, data[key]))
        let authResult = await fetch(url, {
          method: 'GET',
          credentials: 'same-origin',
          mode: 'cors',
          headers: {
              'Content-Type': 'application/json', 
              'Authorization': `Bearer ${token}`
          },
        });
        this.isLoading = false;
        if (authResult.ok) {
          var response = await authResult.json();
          console.log(response);
          this.transactions = response;
        }
        else {
            console.log(authResult);
        }
    }
  }
}
</script>

<style scoped>
ul {
  list-style-type: none;
}

</style>