<script lang="ts">
	import type { Snippet } from 'svelte';
	interface ButtonProps {
		onclick?: (event: Event) => void;
		href?: string;
		type?: 'button' | 'submit' | 'reset';
		variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
		size?: 'xs' | 'sm' | 'md' | 'lg';
		disabled?: boolean;
		fullWidth?: boolean;
		className?: string;
		children?: Snippet;
	}

	let {
		onclick = undefined,
		href = undefined,
		type = 'button',
		variant = 'primary',
		size = 'md',
		disabled = false,
		fullWidth = false,
		className = '',
		children = undefined,
		class: classAttr = '',
		...otherProps
	}: ButtonProps & { class?: string } = $props<ButtonProps & { class?: string }>();

	const variantClasses = {
		primary:
			'bg-indigo-500 hover:bg-indigo-600 text-white shadow-md hover:shadow-lg focus:ring-indigo-300',
		secondary:
			'bg-slate-200 hover:bg-slate-300 text-slate-800 shadow-sm hover:shadow focus:ring-slate-300 dark:bg-slate-700 dark:hover:bg-slate-600 dark:text-slate-200',
		danger: 'bg-red-500 hover:bg-red-600 text-white shadow-md hover:shadow-lg focus:ring-red-300',
		ghost:
			'bg-transparent hover:bg-slate-100 text-slate-700 dark:text-slate-300 dark:hover:bg-slate-800 hover:text-slate-900 dark:hover:text-slate-100'
	} as const;

	const sizeClasses = {
		xs: 'text-xs py-1 px-2 rounded',
		sm: 'text-sm py-1.5 px-3 rounded',
		md: 'py-2.5 px-5 rounded-md',
		lg: 'text-lg py-3 px-6 rounded-lg'
	} as const;

	const baseClasses =
		'font-medium transition-all duration-300 ease-in-out focus:outline-none focus:ring-2 focus:ring-offset-2 dark:focus:ring-offset-slate-900';
	const classes = $derived(
		`${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${fullWidth ? 'w-full' : ''} ${disabled ? 'opacity-60 cursor-not-allowed' : ''} ${className} ${classAttr}`
	);
</script>

{#if href !== undefined}
	<a {href} {onclick} class={classes} {...otherProps}>
		{@render children?.()}
	</a>
{:else}
	<button {type} {disabled} {onclick} class={classes} {...otherProps}>
		{@render children?.()}
	</button>
{/if}
