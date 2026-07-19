<script lang="ts">
	import type { Snippet } from "svelte";

	interface Props {
		open?: boolean;
		title: string;
		// Called for every way the dialog closes (Escape, backdrop, close button).
		onclose?: () => void;
		closeOnBackdrop?: boolean;
		children?: Snippet;
		footer?: Snippet;
	}

	let {
		open = $bindable(false),
		title,
		onclose = undefined,
		closeOnBackdrop = true,
		children = undefined,
		footer = undefined,
	}: Props = $props();

	let dialogEl: HTMLElement | null = $state(null);
	let previouslyFocused: HTMLElement | null = null;

	const titleId = `modal-title-${Math.random().toString(36).slice(2, 8)}`;

	function close() {
		open = false;
		onclose?.();
	}

	function focusableElements(): HTMLElement[] {
		if (!dialogEl) return [];
		return Array.from(
			dialogEl.querySelectorAll<HTMLElement>(
				'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])',
			),
		).filter((el) => !el.hasAttribute("disabled"));
	}

	function handleKeydown(event: KeyboardEvent) {
		if (!open) return;
		if (event.key === "Escape") {
			event.preventDefault();
			close();
			return;
		}
		if (event.key === "Tab") {
			// Simple focus trap: cycle within the dialog.
			const focusable = focusableElements();
			if (focusable.length === 0) return;
			const first = focusable[0];
			const last = focusable[focusable.length - 1];
			const active = document.activeElement as HTMLElement | null;
			if (event.shiftKey && (active === first || !dialogEl?.contains(active))) {
				event.preventDefault();
				last.focus();
			} else if (
				!event.shiftKey &&
				(active === last || !dialogEl?.contains(active))
			) {
				event.preventDefault();
				first.focus();
			}
		}
	}

	$effect(() => {
		if (open && dialogEl) {
			previouslyFocused = document.activeElement as HTMLElement | null;
			// Focus the safest useful action: an element marked data-autofocus,
			// else the first focusable control.
			const preferred =
				dialogEl.querySelector<HTMLElement>("[data-autofocus]");
			(preferred ?? focusableElements()[0] ?? dialogEl).focus();
			return () => {
				previouslyFocused?.focus?.();
				previouslyFocused = null;
			};
		}
	});
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
	<div
		class="fixed inset-0 z-50 flex items-center justify-center p-4"
		role="presentation"
		data-testid="modal-backdrop"
		onclick={(event) => {
			if (closeOnBackdrop && event.target === event.currentTarget) close();
		}}
	>
		<div class="fixed inset-0 bg-slate-900/50" aria-hidden="true"></div>
		<div
			bind:this={dialogEl}
			role="dialog"
			aria-modal="true"
			aria-labelledby={titleId}
			tabindex="-1"
			class="relative w-full max-w-md rounded-lg bg-white p-4 shadow-xl outline-none dark:bg-slate-800 border border-slate-200 dark:border-slate-700"
		>
			<div class="mb-3 flex items-start justify-between gap-2">
				<h2
					id={titleId}
					class="text-base font-semibold text-slate-900 dark:text-slate-100"
				>
					{title}
				</h2>
				<button
					type="button"
					class="rounded p-1 text-slate-500 hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus-visible:ring-2 focus-visible:ring-indigo-500 dark:text-slate-400 dark:hover:bg-slate-700 dark:hover:text-slate-200"
					aria-label="Close dialog"
					onclick={close}
				>
					<svg
						class="h-4 w-4"
						xmlns="http://www.w3.org/2000/svg"
						viewBox="0 0 20 20"
						fill="currentColor"
						aria-hidden="true"
					>
						<path
							d="M6.28 5.22a.75.75 0 0 0-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 1 0 1.06 1.06L10 11.06l3.72 3.72a.75.75 0 1 0 1.06-1.06L11.06 10l3.72-3.72a.75.75 0 0 0-1.06-1.06L10 8.94 6.28 5.22Z"
						/>
					</svg>
				</button>
			</div>
			<div class="text-sm text-slate-700 dark:text-slate-300">
				{@render children?.()}
			</div>
			{#if footer}
				<div class="mt-4 flex flex-wrap justify-end gap-2">
					{@render footer()}
				</div>
			{/if}
		</div>
	</div>
{/if}
