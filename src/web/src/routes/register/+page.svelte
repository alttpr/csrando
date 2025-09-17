<script lang="ts">
	import { enhance } from '$app/forms';
	import * as m from '$lib/paraglide/messages';
	import { page } from '$app/state';

	let password = $state('');
	let confirmPassword = $state('');
	let passwordMismatch = $state(false);

	function checkPasswords() {
		passwordMismatch = password !== confirmPassword;
	}
</script>

<div
	class="min-h-screen flex flex-col items-center justify-center bg-slate-50 dark:bg-slate-900 py-12 px-4 sm:px-6 lg:px-8"
>
	<div class="max-w-md w-full space-y-8 bg-white dark:bg-slate-800 p-10 rounded-xl shadow-lg">
		<div>
			<h1 class="mt-6 text-center text-3xl font-extrabold text-primary-500 dark:text-primary-400">
				{m.register_title()}
			</h1>
		</div>

		{#if page.form?.error}
			<div
				class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 dark:bg-red-700 dark:text-red-200 dark:border-red-600"
				role="alert"
			>
				<p class="font-bold">{m.error_label()}</p>
				<p>{page.form.message || m.register_error_generic()}</p>
			</div>
		{/if}
		{#if passwordMismatch}
			<div
				class="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4 dark:bg-yellow-700 dark:text-yellow-200 dark:border-yellow-600"
				role="alert"
			>
				<p class="font-bold">{m.form_error_label()}</p>
				<p>{m.form_error_password_mismatch()}</p>
			</div>
		{/if}

		<form class="mt-8 space-y-6" method="POST" use:enhance onsubmit={() => checkPasswords()}>
			<div class="rounded-md shadow-sm -space-y-px">
				<div>
					<label for="username" class="sr-only">{m.register_username()}</label>
					<input
						id="username"
						name="username"
						type="text"
						autocomplete="username"
						required
						class="appearance-none rounded-none relative block w-full px-3 py-2 border border-slate-300 placeholder-slate-500 text-slate-900 rounded-t-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm dark:bg-slate-700 dark:border-slate-600 dark:placeholder-slate-400 dark:text-slate-100 dark:focus:ring-primary-500 dark:focus:border-primary-500 form-input"
						placeholder={m.register_username()}
						value={page.form?.username || ''}
					/>
				</div>
				<div>
					<label for="password" class="sr-only">{m.register_password()}</label>
					<input
						id="password"
						name="password"
						type="password"
						autocomplete="new-password"
						required
						bind:value={password}
						oninput={checkPasswords}
						class="appearance-none rounded-none relative block w-full px-3 py-2 border border-slate-300 placeholder-slate-500 text-slate-900 focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm dark:bg-slate-700 dark:border-slate-600 dark:placeholder-slate-400 dark:text-slate-100 dark:focus:ring-primary-500 dark:focus:border-primary-500 form-input"
						placeholder={m.register_password()}
					/>
				</div>
				<div>
					<label for="confirmPassword" class="sr-only">{m.register_confirm_password()}</label>
					<input
						id="confirmPassword"
						name="confirmPassword"
						type="password"
						autocomplete="new-password"
						required
						bind:value={confirmPassword}
						oninput={checkPasswords}
						class="appearance-none rounded-none relative block w-full px-3 py-2 border border-slate-300 placeholder-slate-500 text-slate-900 rounded-b-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 focus:z-10 sm:text-sm dark:bg-slate-700 dark:border-slate-600 dark:placeholder-slate-400 dark:text-slate-100 dark:focus:ring-primary-500 dark:focus:border-primary-500 form-input"
						placeholder={m.register_confirm_password()}
					/>
				</div>
			</div>

			<div>
				<button
					type="submit"
					disabled={passwordMismatch}
					class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-500 hover:bg-indigo-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 dark:focus:ring-offset-slate-800"
				>
					{m.register_submit()}
				</button>
			</div>
		</form>

		<div class="text-sm text-center">
			<p class="text-slate-600 dark:text-slate-400">
				{m.register_has_account()}
				<a
					href="/login"
					class="font-medium text-primary-600 hover:text-primary-500 dark:text-primary-400 dark:hover:text-primary-300"
				>
					{m.nav_login()}
				</a>
			</p>
		</div>
	</div>
</div>
