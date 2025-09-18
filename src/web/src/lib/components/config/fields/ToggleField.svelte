<script lang="ts">
	import Toggle from '$lib/components/ui/Toggle.svelte';
	interface Props {
		id: string;
		description?: string;
		value?: boolean;
		size?: 'sm' | 'md';
		disabled?: boolean;
	}
	let {
		id,
		description = '',
		value = $bindable(false),
		size = 'sm',
		disabled = false
	}: Props = $props();
</script>

<Toggle {id} bind:checked={value} label={description} {size} {disabled} />
<input type="hidden" aria-hidden="true" value={value ? '1' : '0'} />
