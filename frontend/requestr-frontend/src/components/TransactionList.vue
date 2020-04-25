<template>
    <div>
        <input type="date" v-model="from">
        <input type="date" v-model="upTo">
        <input type="submit" v-on:click="fetchTransactions">
        <div id="transaction-list">
            <li v-for="t in transactions" :key="t.id">
                <input 
                  type="checkbox" 
                  v-bind:value="t" 
                  v-model="selectedTransactions"
                />{{t.description}}
            </li>
        </div>
    </div>
</template>

<script>
export default { 
  name: 'TransactionList',
  prop: ['value'],
  components: { },
  data: function() {
    return {
        from: undefined,
        upTo: undefined,
        transactions: [],
        selectedTransactions: []
    }
  },
  watch: {
    selectedTransactions: function () {
      this.emitSelection();
    }
  },
  methods: {
      emitSelection: function () {
        this.$emit('input', this.selectedTransactions);
      },
      fetchTransactions: async function() {
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