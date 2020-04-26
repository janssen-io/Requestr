<template>
    <div>
        <div class="field">
          <label class="label">Username</label>
          <div class="control has-icons-left">
            <span class="icon is-small is-left">
              <i class="fa fa-user"></i>
            </span>
            <input class="input" type="text" v-model="username" />
          </div>
        </div>
        <div class="field">
          <label class="label">Password</label>
          <div class="control has-icons-left">
            <span class="icon is-small is-left">
              <i class="fa fa-key"></i>
            </span>
            <input class="input" type="password" v-model="password" />
          </div>
          <p class="help is-danger">{{ message }}</p>
        </div>
        <button 
          class="button is-primary" 
          v-bind:class="{ 'is-loading': isLoading }"
          type="submit" 
          v-on:click="authenticate">Login</button>
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
      message: "",
      isLoading: false
  }},
  methods: {
    authenticate: async function() {
      this.isLoading = true;
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
          this.isLoading = false;
          this.message = "Unable to login with these credentials.";
      }
    }
  }
}
</script>