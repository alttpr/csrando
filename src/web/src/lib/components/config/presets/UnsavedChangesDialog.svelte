<script lang="ts">
	import * as m from "$lib/paraglide/messages";
	import Modal from "$lib/components/ui/Modal.svelte";
	import Button from "$lib/components/ui/Button.svelte";

	interface Props {
		open?: boolean;
		// "switch": changing presets; "leave": navigating away from the page.
		mode?: "switch" | "leave";
		// The selected preset accepts new revisions from this user.
		canSaveChanges?: boolean;
		// The user can create presets (authenticated).
		canSaveAsNew?: boolean;
		saving?: boolean;
		onsave?: () => void;
		onsaveasnew?: () => void;
		ondiscard: () => void;
		oncancel: () => void;
	}

	let {
		open = $bindable(false),
		mode = "switch",
		canSaveChanges = false,
		canSaveAsNew = false,
		saving = false,
		onsave = undefined,
		onsaveasnew = undefined,
		ondiscard,
		oncancel,
	}: Props = $props();
</script>

<Modal
	bind:open
	title={m.seed_preset_unsaved_title()}
	onclose={oncancel}
	closeOnBackdrop={false}
>
	<p>
		{mode === "leave"
			? m.seed_preset_unsaved_leave_message()
			: m.seed_preset_unsaved_message()}
	</p>
	{#snippet footer()}
		<Button
			type="button"
			variant="secondary"
			size="sm"
			data-autofocus
			onclick={() => {
				open = false;
				oncancel();
			}}
		>
			{m.seed_preset_unsaved_cancel()}
		</Button>
		<Button
			type="button"
			variant="danger"
			size="sm"
			onclick={() => {
				open = false;
				ondiscard();
			}}
		>
			{mode === "leave"
				? m.seed_preset_unsaved_leave()
				: m.seed_preset_unsaved_discard()}
		</Button>
		{#if canSaveAsNew && onsaveasnew && mode === "switch"}
			<Button
				type="button"
				variant="secondary"
				size="sm"
				disabled={saving}
				onclick={() => {
					open = false;
					onsaveasnew();
				}}
			>
				{m.seed_preset_unsaved_save_as_new_and_switch()}
			</Button>
		{/if}
		{#if canSaveChanges && onsave}
			<Button
				type="button"
				variant="primary"
				size="sm"
				disabled={saving}
				onclick={() => {
					open = false;
					onsave();
				}}
			>
				{mode === "leave"
					? m.seed_preset_save_changes()
					: m.seed_preset_unsaved_save_and_switch()}
			</Button>
		{/if}
	{/snippet}
</Modal>
