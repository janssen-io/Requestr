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
      <p class="help is-danger">{{ message }}</p>
        <div id="transaction-list">
            <div class="date-group" v-for="(groupedTransactions, date) in groupedTransactions" :key="date">
              <h4 class="title is-size-7">{{ date }}</h4>
              <TransactionRow
                v-for="t in groupedTransactions" :key="t.id"
                v-bind:value="t"
                v-model="selectedTransactions"
                ></TransactionRow>
            </div> 
        </div>
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
        upTo: this.addDays(new Date(), +1).toISOString().substring(0, 10),
        transactions: [],
        selectedTransactions: [],
        isLoading: false,
        message: ""
    }
  },
  watch: {
    selectedTransactions: function () {
      this.emitSelection();
    }
  },
  computed: {
    groupedTransactions: function () {
      return this.groupByDate(this.transactions);
    }
  },
  methods: {
      groupByDate: function(transactions) {
        return transactions.reduce(function(rv, trx) {
          let key = new Date(trx.createdOn).toLocaleString().substring(0, 10);
          (rv[key] = rv[key] || []).push(trx);
          return rv;
        }, {});
      },
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
        let transactionRequest = await fetch(url, {
          method: 'GET',
          credentials: 'same-origin',
          mode: 'cors',
          headers: {
              'Content-Type': 'application/json', 
              'Authorization': `Bearer ${token}`
          },
        });
        this.isLoading = false;
        if (transactionRequest.ok) {
          this.transactions = await transactionRequest.json();
        }
        else {
          this.message = await transactionRequest.text();
        }
    }
  }
}
</script>

<style scoped>
.date-group h4 {
  margin: 30px 0 0 0;
  padding: 0;
}
</style>