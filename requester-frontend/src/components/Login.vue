<template>
    <div>
      <section v-if="!loggedIn">
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
                <i class="fa fa-lock"></i>
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
      </section>
      <section v-else>
          <div class="field">
            <label class="label">Pass code from e-mail</label>
            <div class="control has-icons-left">
              <span class="icon is-small is-left">
                <i class="fa fa-key"></i>
              </span>
              <input class="input" type="text" v-model="passcode" />
              <p class="help is-danger">{{ message }}</p>
            </div>
          </div>
          <button 
            class="button is-primary" 
            v-bind:class="{ 'is-loading': isLoading }"
            type="submit" 
            v-on:click="enterCode">Login</button>
      </section>
    </div>
</template>

<script>
export default { 
  name: 'Login',
  components: { },
  data: function() {
    return {
      loggedIn: false,
      username: "",
      password: "",
      message: "",
      passcode: "",
      isLoading: false
  }},
  methods: {
    authenticate: async function() {
      this.message = "";
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
      this.isLoading = false;
      if (authResult.ok) {
        var response = await authResult.json();
        window.localStorage.setItem('token', response.token);
        this.loggedIn = true;
      }
      else {
          this.isLoading = false;
          this.message = "Unable to login with these credentials.";
      }
    },
    enterCode: async function() {
      this.message = "";
      this.isLoading = true;
      let data = { oneTimePassword: this.passcode, }
      let token = window.localStorage.getItem('token');
      let url = new URL(`${process.env.VUE_APP_API}/api/users/token`);
      let authResult = await fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        mode: 'cors',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json' 
        },
        body: JSON.stringify(data),
      });
      if (authResult.ok) {
        var response = await authResult.json();
        window.localStorage.setItem('token', response.token);
        this.token = response.token;
        if(!response.isInitialized) {
            this.$router.push('/app/apikey');
        } else {
            this.$router.push('/app/createPaymentRequest')
        }
      }
      else {
          this.isLoading = false;
          this.message = await authResult.text();
      }
    }
  }
}
</script>