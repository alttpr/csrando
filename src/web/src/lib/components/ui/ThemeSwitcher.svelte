<script lang="ts">
	import { theme, type Theme } from '$lib/services/theme';

	let isThemeDropdownOpen = $state(false);
	let rootEl: HTMLElement | null = null;

	function setTheme(newTheme: Theme) {
		theme.set(newTheme);
		isThemeDropdownOpen = false; // Close dropdown on selection
	}

	function toggleThemeDropdown() {
		isThemeDropdownOpen = !isThemeDropdownOpen;
	}

	function handleGlobalClick(e: MouseEvent) {
		if (!isThemeDropdownOpen) return;
		const t = e.target as Node | null;
		if (rootEl && t && !rootEl.contains(t)) isThemeDropdownOpen = false;
	}

	function handleGlobalKey(e: KeyboardEvent) {
		if (isThemeDropdownOpen && e.key === 'Escape') isThemeDropdownOpen = false;
	}

</script>

<svelte:body onclick={handleGlobalClick} onkeydown={handleGlobalKey} />

<div class="relative" bind:this={rootEl}>
	<button
		aria-label="Select theme"
		onclick={toggleThemeDropdown}
		class="flex items-center gap-1 p-1.5 rounded-md text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
	>
		{#if $theme === 'light'}
			<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
				<path
					d="M12 3a1 1 0 00-1 1v2a1 1 0 102 0V4a1 1 0 00-1-1zM5.636 5.636a1 1 0 00-1.414 0l-1.414 1.414a1 1 0 001.414 1.414L5.636 7.05A1 1 0 005.636 5.636zM18.364 5.636a1 1 0 000 1.414l1.414 1.414a1 1 0 001.414-1.414l-1.414-1.414a1 1 0 00-1.414 0zM12 19a1 1 0 001-1v-2a1 1 0 10-2 0v2a1 1 0 001 1zM5.636 18.364a1 1 0 001.414 0l1.414-1.414a1 1 0 00-1.414-1.414l-1.414 1.414a1 1 0 000 1.414zM18.364 18.364a1 1 0 000-1.414l-1.414-1.414a1 1 0 00-1.414 1.414l1.414 1.414a1 1 0 001.414 0zM4 11H2a1 1 0 100 2h2a1 1 0 100-2zm16 0h2a1 1 0 100 2h-2a1 1 0 100-2zM12 7a5 5 0 100 10 5 5 0 000-10z"
				/>
			</svg>
		{:else if $theme === 'dark'}
			<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
				<path
					fill-rule="evenodd"
					d="M9.528 1.718a.75.75 0 01.162.819A8.97 8.97 0 009 6a9 9 0 009 9 8.97 8.97 0 003.463-.69.75.75 0 01.981.98 10.503 10.503 0 01-9.694 6.46c-5.799 0-10.5-4.701-10.5-10.5 0-3.51 1.713-6.636 4.362-8.442a.75.75 0 01.819.162z"
					clip-rule="evenodd"
				/>
			</svg>
		{:else}
			<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
				<path
					fill-rule="evenodd"
					d="M2.25 5.25A3 3 0 015.25 2.25h13.5a3 3 0 013 3V15a3 3 0 01-3 3h-2.553v1.053a.75.75 0 101.5 0V18H7.803v1.053a.75.75 0 101.5 0V18H5.25a3 3 0 01-3-3V5.25zm1.5 0v9.75A1.5 1.5 0 005.25 16.5h13.5a1.5 1.5 0 001.5-1.5V5.25A1.5 1.5 0 0018.75 3.75H5.25A1.5 1.5 0 003.75 5.25z"
					clip-rule="evenodd"
				/>
			</svg>
		{/if}
	</button>

	{#if isThemeDropdownOpen}
		<div
			class="absolute right-0 mt-2 w-48 bg-white dark:bg-slate-800 rounded-md shadow-lg py-1 z-10 border border-gray-200 dark:border-gray-700"
		>
			<button
				onclick={() => setTheme('light')}
				class="flex items-center gap-2 w-full px-4 py-2 text-left text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
			>
				<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
					<path
						d="M12 3a1 1 0 00-1 1v2a1 1 0 102 0V4a1 1 0 00-1-1zM5.636 5.636a1 1 0 00-1.414 0l-1.414 1.414a1 1 0 001.414 1.414L5.636 7.05A1 1 0 005.636 5.636zM18.364 5.636a1 1 0 000 1.414l1.414 1.414a1 1 0 001.414-1.414l-1.414-1.414a1 1 0 00-1.414 0zM12 19a1 1 0 001-1v-2a1 1 0 10-2 0v2a1 1 0 001 1zM5.636 18.364a1 1 0 001.414 0l1.414-1.414a1 1 0 00-1.414-1.414l-1.414 1.414a1 1 0 000 1.414zM18.364 18.364a1 1 0 000-1.414l-1.414-1.414a1 1 0 00-1.414 1.414l1.414 1.414a1 1 0 001.414 0zM4 11H2a1 1 0 100 2h2a1 1 0 100-2zm16 0h2a1 1 0 100 2h-2a1 1 0 100-2zM12 7a5 5 0 100 10 5 5 0 000-10z"
					/>
				</svg>
				<span>Light</span>
				{#if $theme === 'light'}<span class="ml-auto">✓</span>{/if}
			</button>

			<button
				onclick={() => setTheme('dark')}
				class="flex items-center gap-2 w-full px-4 py-2 text-left text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
			>
				<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
					<path
						fill-rule="evenodd"
						d="M9.528 1.718a.75.75 0 01.162.819A8.97 8.97 0 009 6a9 9 0 009 9 8.97 8.97 0 003.463-.69.75.75 0 01.981.98 10.503 10.503 0 01-9.694 6.46c-5.799 0-10.5-4.701-10.5-10.5 0-3.51 1.713-6.636 4.362-8.442a.75.75 0 01.819.162z"
						clip-rule="evenodd"
					/>
				</svg>
				<span>Dark</span>
				{#if $theme === 'dark'}<span class="ml-auto">✓</span>{/if}
			</button>

			<button
				onclick={() => setTheme('system')}
				class="flex items-center gap-2 w-full px-4 py-2 text-left text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
			>
				<svg viewBox="0 0 24 24" fill="currentColor" class="w-5 h-5">
					<path
						fill-rule="evenodd"
						d="M2.25 5.25A3 3 0 015.25 2.25h13.5a3 3 0 013 3V15a3 3 0 01-3 3h-2.553v1.053a.75.75 0 101.5 0V18H7.803v1.053a.75.75 0 101.5 0V18H5.25a3 3 0 01-3-3V5.25zm1.5 0v9.75A1.5 1.5 0 005.25 16.5h13.5a1.5 1.5 0 001.5-1.5V5.25A1.5 1.5 0 0018.75 3.75H5.25A1.5 1.5 0 003.75 5.25z"
						clip-rule="evenodd"
					/>
				</svg>
				<span>System</span>
				{#if $theme === 'system'}<span class="ml-auto">✓</span>{/if}
			</button>
		</div>
	{/if}
</div>
