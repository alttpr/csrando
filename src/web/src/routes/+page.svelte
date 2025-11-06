<script lang="ts">
	/* eslint-disable svelte/no-at-html-tags */
	import * as m from "$lib/paraglide/messages";
	import Button from "$lib/components/ui/Button.svelte";
	import Card from "$lib/components/ui/Card.svelte";
	import { page } from "$app/state";

	const siteMode = $derived(page.data.siteMode);
	const siteFeatures = $derived(page.data.siteFeatures);
</script>

<div class="container mx-auto px-4 py-8 text-center max-w-4xl">
	<div class="mb-10">
		<h1
			class="text-5xl font-bold mb-4 text-primary-500 dark:text-primary-400"
		>
			{#if siteMode === "all"}
				{m.landing_title()}
			{:else if siteMode === "alttpr"}
				{m.landing_title_alttpr()}
			{:else if siteMode === "combo"}
				{m.landing_title_combo()}
			{/if}
		</h1>
		<p class="text-xl mb-8 text-slate-700 dark:text-slate-300">
			{#if siteMode === "all"}
				{@html m.landing_intro()}
			{:else if siteMode === "alttpr"}
				{@html m.landing_intro_alttpr()}
			{:else if siteMode === "combo"}
				{@html m.landing_intro_combo()}
			{/if}
		</p>
		{#if siteFeatures.showAlttpr}
			<Button
				href="/config/alttpr"
				variant="primary"
				size="lg"
				className="transform hover:-translate-y-1"
			>
				{m.landing_alttpr()}
			</Button>
		{/if}
		&nbsp;
		{#if siteFeatures.showCombo}
			<Button
				href="/config/combo"
				variant="primary"
				size="lg"
				className="transform hover:-translate-y-1"
			>
				{m.landing_combo()}
			</Button>
		{/if}
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-12">
		<Card title="Get Help" className="text-left">
			<p class="text-slate-700 dark:text-slate-300">
				If this is your first time playing or you're looking for more
				information about the randomizer go here:<br />
				<a
					href="/content/information"
					class="text-blue-600 dark:text-blue-400 underline"
					>Information</a
				>
			</p>
		</Card>

		<Card title="Get Involved" className="text-left">
			<p class="text-slate-700 dark:text-slate-300">
				If you're looking to get involved with the randomizer community,
				take a look at the resources for more information.<br />
				<a
					href="/content/resources"
					class="text-blue-600 dark:text-blue-400 underline"
					>Resources</a
				>
			</p>
		</Card>
	</div>
</div>
