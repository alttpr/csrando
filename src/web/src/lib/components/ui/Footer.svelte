<script>
	const currentYear = new Date().getFullYear();
</script>

<footer class="border-t border-slate-200 dark:border-slate-800 py-6 mt-8">
	<div class="container mx-auto px-4 flex flex-col sm:flex-row items-center justify-between gap-3">
		<p class="text-sm text-slate-500 dark:text-slate-400">© {currentYear}</p>
		<nav class="flex items-center gap-4 text-sm">
			<a
				href="/privacy"
				class="text-slate-600 hover:text-slate-800 dark:text-slate-300 dark:hover:text-white"
				>Privacy</a
			>
			<a
				href="/cookies"
				class="text-slate-600 hover:text-slate-800 dark:text-slate-300 dark:hover:text-white"
				>Cookies</a
			>
		</nav>
	</div>
</footer>
