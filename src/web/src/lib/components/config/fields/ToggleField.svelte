<script lang="ts">
	import Toggle from "$lib/components/ui/Toggle.svelte";
	interface Props {
		id: string;
		value?: boolean;
		size?: "sm" | "md";
		disabled?: boolean;
	}
	let {
		id,
		value = $bindable(false),
		size = "sm",
		disabled = false,
	}: Props = $props();
</script>

<Toggle {id} bind:checked={value} {size} {disabled} />
