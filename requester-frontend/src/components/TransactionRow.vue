<template>
    <div>
        <section class="transaction" v-on:click="updateSelection" v-bind:class="{ selected: isSelected }">
            <div class="column">
                <i class="fa fa-check"></i>
            </div>
            <div class="column grow">
                <h1>{{ transaction.counterParty }}</h1>
                <h2>{{ transaction.counterPartyIban }}</h2>
                <p class="description">{{ transaction.description }}</p>
            </div>
            <div class="column" style="width: 150px">
                <p class="amount"><span class="has-text-small">{{transaction.currency}}</span> <span class="has-text-primary">{{ (+transaction.amount).toFixed(2) }}</span></p>
            </div>
        </section>
    </div>
</template>

<script>
export default {
    name: 'TransactionRow',
    props: ['value', 'modelValue', 'disabled'],
    model: {
        prop: 'modelValue',
        event: 'change'
    },
    data: function () {
        return {
            isSelected: this.modelValue === true,
            transaction: this.value
        }
    },
    methods: {
        updateSelection: function() {
            if (this.disabled) {
                return;
            }
            this.isSelected = !this.isSelected;
            if (Array.isArray(this.modelValue) || typeof this.modelValue === 'undefined') {
                if (this.isSelected) {
                    this.modelValue.push(this.transaction);
                } else {
                    this.modelValue.splice(this.modelValue.indexOf(this.transaction), 1);
                }
                this.$emit('change', this.modelValue);
            } else {
                this.$emit('change', this.isSelected);
            }
        }
    }
}
</script>

<style scoped>
.transaction {
    width: 100%;
    display: flex;
    flex-direction: row;
    flex-wrap: nowrap;
    justify-content: space-between;
    align-items: center;
    border-radius: 3px;
    box-shadow: 0 0 4px #ccc;
    margin: 8px 0;
    padding: 20px 10px;
}

.transaction.selected {
    box-shadow: 0 0 4px hsl(171, 50%, 38%);
}

.transaction h1 {
    font-weight: bold;
    line-height: 0.8em;
}

.transaction h2 {
    font-style: italic;
    font-size: 0.7em;
    color: #aaa;
}

.transaction p.description {
    line-height: 1.2em;
    padding: 0.5em 0;
    font-size: 0.9em;
}

.column {
    flex-grow: 0;
    white-space: nowrap;
    margin: 0px 10px;
    padding: 0 0;
}

.column.grow {
    flex-grow: 1;
}

i.fa-check {
    opacity: 0;
}
.selected i.fa-check {
    opacity: 1;
}

.left-column p {
    padding: 0;
    margin: 0;
    line-height: 0;
}

</style>