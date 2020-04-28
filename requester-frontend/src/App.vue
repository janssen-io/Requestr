<template>
    <div id="container">
      <div class="level" id="header">
        <div class="level-left">
          <h1 class="title is-size-5">
            <router-link to="/app/transactions">Requestr for Bunq</router-link>
          </h1>
        </div>
        <div class="level-right">
          <router-link to="/app/apikey">API key</router-link> 
        </div>
      </div>
      <router-view/>
    </div>
</template>

<script>
export default {
  name: 'App',
  components: { },
  data: function() { return {}; },
  methods: { }
}
</script>

<style>
#container {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  margin: 20px auto;
  width: 720px;
}

#header {
  margin-bottom: 20px;
}
</style>
