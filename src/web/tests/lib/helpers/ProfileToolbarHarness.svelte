<script lang="ts">
	import ProfileToolbar from "$lib/components/config/profiles/ProfileToolbar.svelte";
	import type {
		ProfileState,
		LoadedForm,
	} from "$lib/config/profile-state.svelte";
	import {
		hydrateFormState,
		normalizeConfig,
		type FormStateSnapshot,
	} from "$lib/config/normalize";
	import type { Metadata } from "$lib/types";

	interface Props {
		state: ProfileState;
		metadata: Metadata;
		isAuthenticated?: boolean;
		isAdmin?: boolean;
	}

	let {
		state: profiles,
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

<ProfileToolbar
	state={profiles}
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
