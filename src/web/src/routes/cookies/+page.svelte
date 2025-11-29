<script lang="ts">
	// Static cookie and local storage policy page
	const updatedAt = '2025-09-04';
</script>

<div class="container mx-auto px-4 py-10 max-w-3xl">
	<h1 class="text-3xl font-bold mb-6">Cookie & Local Storage Policy</h1>

	<p class="text-slate-700 dark:text-slate-300 mb-4">
		This app uses a minimal set of cookies and on‑device storage strictly necessary to provide
		requested features. No analytics or advertising cookies are used.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Cookies We Use</h2>
	<div class="overflow-x-auto">
		<table class="w-full text-left text-sm border border-slate-200 dark:border-slate-700">
			<thead>
				<tr class="bg-slate-50 dark:bg-slate-800/50">
					<th class="p-2">Name</th>
					<th class="p-2">Purpose</th>
					<th class="p-2">Type</th>
					<th class="p-2">Expiry</th>
					<th class="p-2">Attributes</th>
				</tr>
			</thead>
			<tbody>
				<tr class="border-t border-slate-200 dark:border-slate-700">
					<td class="p-2">Session cookie (set by Lucia)</td>
					<td class="p-2">Keeps you signed in and authenticates requests.</td>
					<td class="p-2">Strictly necessary, first‑party</td>
					<td class="p-2">Session expiry as configured by the server</td>
					<td class="p-2">HttpOnly, Secure (prod), SameSite=Lax, Path=/</td>
				</tr>
			</tbody>
		</table>
	</div>

	<p class="text-slate-700 dark:text-slate-300 mt-4">
		Because the session cookie is strictly necessary to provide the service, consent is not required
		under the ePrivacy Directive. It is disclosed here for transparency.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Local Storage and IndexedDB</h2>
	<p class="text-slate-700 dark:text-slate-300">
		We store the following items on your device to deliver features you request:
	</p>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>
			<strong>Theme preference</strong> — key <code>theme</code> in <code>localStorage</code>.
		</li>
		<li>
			<strong>Sprite selections</strong> — key <code>sprite_selection_&lt;gameId&gt;</code> in
			<code>localStorage</code> and IndexedDB.
		</li>
		<li>
			<strong>Post‑generation settings</strong> — key <code>postgen_selection_&lt;gameId&gt;</code>
			in
			<code>localStorage</code> and IndexedDB.
		</li>
		<li>
			<strong>Selected ROMs</strong> — key <code>rom_global_&lt;gameId&gt;</code> in IndexedDB via LocalForage.
		</li>
	</ul>
	<p class="text-slate-700 dark:text-slate-300">
		These are essential for functionality (e.g., reusing uploads, applying sprites, remembering your
		theme). You can clear them via your browser settings or in‑app actions that remove specific
		items.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Third‑Party Cookies</h2>
	<p class="text-slate-700 dark:text-slate-300">
		We do not set or use third‑party cookies, analytics, or advertising pixels.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Managing Cookies</h2>
	<p class="text-slate-700 dark:text-slate-300">
		You may block cookies in your browser; however, the app will not function without the session
		cookie when logged in. Local storage/IndexedDB can be cleared at any time through your browser.
	</p>

	<p class="text-sm text-slate-500 mt-8">Last updated: {updatedAt}</p>
</div>
