<template>
    <div>
      <div class="app-title ha-text-centered">
        <h1 class="title is-size-5">Requestr for Bunq</h1>
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
.app-title {
  margin-bottom: 20px;
}
</style>
