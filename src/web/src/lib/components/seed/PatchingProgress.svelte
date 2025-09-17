<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	import Card from '../ui/Card.svelte';
	import Button from '../ui/Button.svelte';

	interface Props {
		patchingProgress?: number;
		isPatching?: boolean;
		patchingError?: string | null;
		canPatch?: boolean;
	}

	let {
		patchingProgress = 0,
		isPatching = false,
		patchingError = null,
		canPatch = false
	}: Props = $props();
	let rootEl: HTMLElement | null = null;

	function handleStartPatching() {
		rootEl?.dispatchEvent(new CustomEvent('startPatching', { bubbles: true }));
	}

	function handleCancelPatching() {
		rootEl?.dispatchEvent(new CustomEvent('cancelPatching', { bubbles: true }));
	}

  // Declare events for consumers
  export type $$Events = {
    startPatching: Event;
    cancelPatching: Event;
  };
</script>

<div bind:this={rootEl}>
<Card title={m.seed_patching_title()}>
	{#if isPatching}
		<div class="mb-4">
			<div class="w-full bg-slate-200 dark:bg-slate-700 rounded-full h-4 mb-2">
				<div
					class="bg-indigo-500 h-4 rounded-full transition-all duration-300"
					style="width: {patchingProgress}%"
				></div>
			</div>
			<div class="text-center text-sm text-slate-600 dark:text-slate-400">
				{m.seed_patching_progress({ progress: Math.round(patchingProgress) })}
			</div>
		</div>
		<Button variant="danger" onclick={handleCancelPatching} className="w-full">
			{m.seed_cancel_patching()}
		</Button>
	{:else if patchingError}
		<div class="bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 p-4 rounded-md mb-4">
			<h3 class="font-bold mb-2">{m.seed_patching_error_title()}</h3>
			<p>{patchingError}</p>
		</div>
		<Button variant="primary" onclick={handleStartPatching} disabled={!canPatch} className="w-full">
			{m.seed_try_again_button()}
		</Button>
	{:else}
		<p class="mb-4 text-slate-700 dark:text-slate-300">
			{canPatch ? m.seed_ready_to_patch() : m.seed_upload_required_roms()}
		</p>
		<Button variant="primary" onclick={handleStartPatching} disabled={!canPatch} className="w-full">
			{m.seed_start_patching_button()}
		</Button>
	{/if}
</Card>

</div>
