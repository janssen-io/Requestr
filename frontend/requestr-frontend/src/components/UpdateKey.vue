<template>
    <div>
        <input type="password" v-model="apikey" />
        <input type="submit" v-on:click="putKey" />
        <span class="error">{{ message }}</span>
    </div>
</template>

<script>
export default { 
  name: 'UpdateKey',
  components: { },
  data: function() {
    return {
        apikey: "",
        message: ""
    }
  },
  methods: {
    putKey: async function () {
      let token = window.localStorage.getItem('token');
      let response = await fetch('https://localhost:44352/api/users/apikey',{
        method: 'PUT',
        credentials: 'same-origin',
        mode: 'cors',
        body: `"${this.apikey}"`,
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      });
      if (response.ok) {
          this.$router.push('/app/transactions');
      }
      else {
          this.message = "API key was not updated.";
      }
    }
  }
}
</script>