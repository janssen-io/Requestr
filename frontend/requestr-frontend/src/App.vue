<template>
    <div>
      <div v-if="!token">
        <input type="text" v-model="username" />
        <input type="password" v-model="password" />
        <input type="submit" v-on:click="authenticate" />
      </div>
      <div v-if="!isInit">
        <input type="password" v-model="apikey" />
        <input type="submit" v-on:click="putKey" />
      </div>

      <p><b>Token:</b> {{ token }}</p>
      <p><b>Init?:</b> {{ isInit }}</p>
    </div>
</template>

<script>

export default {
  name: 'App',
  components: { },
  data: function() {
    return {
      username: "",
      password: "",
      token: "",
      isInit: true,
      apikey: ""
  }},
  methods: {
    authenticate: async function() {
      let data = {
        username: this.username,
        password: this.password
      }
      console.log(data);
      let authResult = await fetch('https://localhost:44352/api/users/login',{
        method: 'POST',
        credentials: 'same-origin',
        mode: 'cors',
        body: JSON.stringify(data),
        headers: { 'Content-Type': 'application/json' },
      });
      if (authResult.ok) {
        var response = await authResult.json();
        console.log(response);
        this.token = response.token;
        this.isInit = response.isInitialized;
      }
    },
    putKey: async function () {
      let authResult = await fetch('https://localhost:44352/api/users/apikey',{
        method: 'PUT',
        credentials: 'same-origin',
        mode: 'cors',
        body: `"${this.apikey}"`,
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${this.token}`
        },
      });
      console.log(authResult);
      this.isInit = authResult.ok;
    }
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
