<script lang="ts">
	import * as m from "$lib/paraglide/messages";

	export interface MenuItem {
		id: string;
		label: string;
		danger?: boolean;
		onselect: () => void;
	}

	interface Props {
		items: MenuItem[];
		disabled?: boolean;
	}

	let { items, disabled = false }: Props = $props();

	let isOpen = $state(false);
	let rootEl: HTMLElement | null = null;
	let buttonEl: HTMLButtonElement | null = $state(null);

	function close(restoreFocus = false) {
		isOpen = false;
		if (restoreFocus) buttonEl?.focus();
	}

	function handleClickOutside(event: MouseEvent) {
		if (!isOpen) return;
		const target = event.target as Node | null;
		if (rootEl && target && !rootEl.contains(target)) close();
	}

	function menuItems(): HTMLElement[] {
		return Array.from(
			rootEl?.querySelectorAll<HTMLElement>('[role="menuitem"]') ?? [],
		);
	}

	function handleMenuKeydown(event: KeyboardEvent) {
		if (!isOpen) return;
		const elements = menuItems();
		const index = elements.indexOf(document.activeElement as HTMLElement);
		switch (event.key) {
			case "Escape":
				event.preventDefault();
				close(true);
				break;
			case "ArrowDown":
				event.preventDefault();
				elements[(index + 1) % elements.length]?.focus();
				break;
			case "ArrowUp":
				event.preventDefault();
				elements[(index - 1 + elements.length) % elements.length]?.focus();
				break;
			case "Home":
				event.preventDefault();
				elements[0]?.focus();
				break;
			case "End":
				event.preventDefault();
				elements[elements.length - 1]?.focus();
				break;
		}
	}

	$effect(() => {
		if (isOpen) {
			menuItems()[0]?.focus();
		}
	});
</script>

<svelte:body onclick={handleClickOutside} />

<div class="relative" bind:this={rootEl} onkeydown={handleMenuKeydown} role="presentation">
	<button
		bind:this={buttonEl}
		type="button"
		class="rounded-md border border-slate-300 bg-white p-1.5 text-slate-600 shadow-sm hover:bg-slate-50 hover:text-slate-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 disabled:opacity-60 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700 dark:hover:text-slate-100"
		aria-haspopup="menu"
		aria-expanded={isOpen}
		aria-label={m.seed_profile_more_actions()}
		{disabled}
		onclick={() => (isOpen = !isOpen)}
		data-testid="profile-overflow-button"
	>
		<svg
			class="h-4 w-4"
			xmlns="http://www.w3.org/2000/svg"
			viewBox="0 0 20 20"
			fill="currentColor"
			aria-hidden="true"
		>
			<path
				d="M10 3a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm0 5.5a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm0 5.5a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Z"
			/>
		</svg>
	</button>

	{#if isOpen}
		<div
			role="menu"
			aria-label={m.seed_profile_more_actions()}
			class="absolute right-0 z-50 mt-1 w-56 rounded-md border border-slate-200 bg-white py-1 text-sm shadow-lg dark:border-slate-700 dark:bg-slate-800"
			data-testid="profile-overflow-menu"
		>
			{#each items as item (item.id)}
				<button
					type="button"
					role="menuitem"
					class="block w-full px-3 py-1.5 text-left focus:outline-none focus-visible:bg-indigo-600 focus-visible:text-white hover:bg-indigo-600 hover:text-white {item.danger
						? 'text-red-600 dark:text-red-400 hover:bg-red-600 focus-visible:bg-red-600'
						: 'text-slate-700 dark:text-slate-200'}"
					onclick={() => {
						close(true);
						item.onselect();
					}}
				>
					{item.label}
				</button>
			{/each}
		</div>
	{/if}
</div>
