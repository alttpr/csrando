<script lang="ts">
	import type { Snippet } from "svelte";

	interface Props {
		variant?: "neutral" | "info" | "success" | "warning" | "danger";
		// Show a leading status dot so state is not conveyed by color alone
		// in combination with the required text label.
		dot?: boolean;
		title?: string;
		children?: Snippet;
	}

	let {
		variant = "neutral",
		dot = false,
		title = undefined,
		children = undefined,
		class: classAttr = "",
	}: Props & { class?: string } = $props();

	const variantClasses = {
		neutral:
			"bg-slate-100 text-slate-700 border-slate-300 dark:bg-slate-700 dark:text-slate-200 dark:border-slate-600",
		info: "bg-blue-100 text-blue-800 border-blue-300 dark:bg-blue-900 dark:text-blue-200 dark:border-blue-700",
		success:
			"bg-green-100 text-green-800 border-green-300 dark:bg-green-900 dark:text-green-200 dark:border-green-700",
		warning:
			"bg-yellow-100 text-yellow-800 border-yellow-300 dark:bg-yellow-900 dark:text-yellow-200 dark:border-yellow-700",
		danger:
			"bg-red-100 text-red-800 border-red-300 dark:bg-red-900 dark:text-red-200 dark:border-red-700",
	} as const;

	const dotClasses = {
		neutral: "bg-slate-500 dark:bg-slate-400",
		info: "bg-blue-500 dark:bg-blue-400",
		success: "bg-green-500 dark:bg-green-400",
		warning: "bg-yellow-500 dark:bg-yellow-400",
		danger: "bg-red-500 dark:bg-red-400",
	} as const;
</script>

<span
	class="inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap {variantClasses[
		variant
	]} {classAttr}"
	{title}
>
	{#if dot}
		<span
			class="inline-block h-1.5 w-1.5 rounded-full {dotClasses[variant]}"
			aria-hidden="true"
		></span>
	{/if}
	{@render children?.()}
</span>
