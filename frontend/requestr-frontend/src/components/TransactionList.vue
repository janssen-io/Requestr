<template>
    <div>
        <input type="date" v-model="from">
        <input type="date" v-model="upTo">
        <input type="submit" v-on:click="fetchTransactions">
        <div id="transaction-list">
            <li v-for="t in transactions" :key="t.id">
                <input type="checkbox" v-bind:value="t" v-model="selectedTransactions" />{{t.description}}
            </li>
        </div>

        {{ total }}
    </div>
</template>

<script>
export default { 
  name: 'TransactionList',
  components: { },
  data: function() {
    return {
        from: undefined,
        upTo: undefined,
        transactions: [],
        selectedTransactions: []
    }
  },
  computed: {
      total: function() { 
          return this.selectedTransactions.reduce((acc, curr) => acc - curr.amount, 0).toFixed(2);
        }
  },
  methods: {
      fetchTransactions: async function() {
      let data = {
        from: this.from,
        upTo: this.upTo
      }
      let token = window.localStorage.getItem('token');
      let url = new URL('https://localhost:44352/api/transactions')
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