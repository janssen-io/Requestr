<template>
    <div>
          <div class="field">
            <label class="label">API key</label>
            <div class="control has-icons-left">
              <span class="icon is-small is-left">
                <i class="fa fa-key"></i>
              </span>
              <input class="input" type="password" v-model="apikey" />
            </div>
          </div>
          <p class="help is-danger">{{ message }}</p>
          <button 
            class="button is-primary" 
            v-bind:class="{ 'is-loading': isLoading }"
            type="submit" 
            v-on:click="updateKey">Update</button>
    </div>
</template>

<script>
export default { 
  name: 'UpdateKey',
  components: { },
  data: function() {
    return {
        apikey: "",
        message: "",
        isLoading: false,
    }
  },
  methods: {
    updateKey: async function () {
      this.isLoading = true;
      this.message = "";
      let token = window.localStorage.getItem('token');
      let url = new URL(`${process.env.VUE_APP_API}/api/users/apikey`);
      let response = await fetch(url, {
        method: 'PUT',
        credentials: 'same-origin',
        mode: 'cors',
        body: `"${this.apikey}"`,
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      });
      this.isLoading = false;
      if (response.ok) {
          this.$router.push('/app/createPaymentRequest');
      }
      else {
          this.message = await response.text();
      }
    }
  }
}
</script>