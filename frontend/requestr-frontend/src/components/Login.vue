<template>
    <div>
        <input type="text" v-model="username" />
        <input type="password" v-model="password" />
        <input type="submit" v-on:click="authenticate" />
        <span class="error">{{ message }}</span>
    </div>
</template>

<script>
export default { 
  name: 'Login',
  components: { },
  data: function() {
    return {
      username: "",
      password: "",
      message: ""
  }},
  methods: {
    authenticate: async function() {
      let data = {
        username: this.username,
        password: this.password
      }
      let url = new URL(`${process.env.VUE_APP_API}/api/users/login`);
      let authResult = await fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        mode: 'cors',
        body: JSON.stringify(data),
        headers: { 'Content-Type': 'application/json' },
      });
      if (authResult.ok) {
        var response = await authResult.json();
        window.localStorage.setItem('token', response.token);
        this.token = response.token;
        if(!response.isInitialized) {
            this.$router.push('/app/apikey');
        } else {
            this.$router.push('/app/transactions')
        }
      }
      else {
          this.message = "Unable to login with these credentials.";
      }
    }
  }
}
</script>