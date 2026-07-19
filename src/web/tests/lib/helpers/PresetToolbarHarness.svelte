<script lang="ts">
	import PresetToolbar from "$lib/components/config/presets/PresetToolbar.svelte";
	import type {
		PresetState,
		LoadedForm,
	} from "$lib/config/preset-state.svelte";
	import {
		hydrateFormState,
		normalizeConfig,
		type FormStateSnapshot,
	} from "$lib/config/normalize";
	import type { Metadata } from "$lib/types";

	interface Props {
		state: PresetState;
		metadata: Metadata;
		isAuthenticated?: boolean;
		isAdmin?: boolean;
	}

	let {
		state: presets,
		metadata,
		isAuthenticated = true,
		isAdmin = false,
	}: Props = $props();

	// Stand-in for the config page's form state.
	let form = $state<FormStateSnapshot>(hydrateFormState({}, metadata).form);
	const currentNormalized = $derived(normalizeConfig(form, metadata));

	function onapply(loaded: LoadedForm) {
		form = loaded.form;
	}
</script>

<PresetToolbar
	state={presets}
	{metadata}
	{currentNormalized}
	{isAuthenticated}
	{isAdmin}
	{onapply}
	onreset={() => {}}
/>

<button
	data-testid="harness-modify"
	onclick={() => {
		form = {
			...form,
			perGame: {
				...form.perGame,
				Alttpr: { ...form.perGame.Alttpr, Swords: "Assured" },
			},
		};
	}}
>
	modify
</button>
<span data-testid="harness-swords">{String(form.perGame.Alttpr?.Swords)}</span>
