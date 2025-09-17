<script lang="ts">
	// Static privacy policy page
	const updatedAt = '2025-09-04';
</script>

<div class="container mx-auto px-4 py-10 max-w-3xl">
	<h1 class="text-3xl font-bold mb-6">Privacy Policy</h1>
	<p class="text-slate-700 dark:text-slate-300 mb-4">
		This page describes what personal data the application processes, the purposes and legal bases,
		how long data is retained, and the rights available to users under applicable data protection
		laws (such as the GDPR).
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Summary</h2>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>
			<strong>Account data:</strong> We store your chosen username and a password hash (not the plaintext
			password).
		</li>
		<li>
			<strong>Session data:</strong> We store an opaque session ID and expiry to keep you signed in.
		</li>
		<li>
			<strong>Seed data:</strong> We store your seed configuration and generated seed metadata to let
			you revisit and download results.
		</li>
		<li>
			<strong>On-device storage:</strong> We store theme preference, uploaded ROMs, and sprite selections
			in your browser storage to provide requested features.
		</li>
		<li>
			<strong>No trackers:</strong> We do not use analytics, advertising cookies, or third‑party trackers.
		</li>
	</ul>

	<h2 class="text-xl font-semibold mt-8 mb-3">Data We Process</h2>
	<p class="text-slate-700 dark:text-slate-300">
		<strong>Account</strong> — When you register, we store:
	</p>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>Username</li>
		<li>Password hash generated with Argon2id (never the plaintext)</li>
	</ul>
	<p class="text-slate-700 dark:text-slate-300 mt-2">
		<strong>Sessions</strong> — When you log in, we create a server‑side session record (ID, user ID,
		expiry) and set a browser cookie containing the session ID.
	</p>
	<p class="text-slate-700 dark:text-slate-300">
		<strong>Seeds</strong> — When you generate a seed, we save the configuration options, generated patch
		data, and timestamps. If you are logged in, we associate seeds with your account.
	</p>
	<p class="text-slate-700 dark:text-slate-300">
		<strong>Browser storage</strong> — We save a few items locally on your device to deliver functionality
		you request:
	</p>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>Theme preference in <code>localStorage</code> (key: <code>theme</code>).</li>
		<li>
			Sprite selections in <code>localStorage</code> / IndexedDB (<code
				>sprite_selection_&lt;gameId&gt;</code
			>).
		</li>
		<li>
			Uploaded ROM data in IndexedDB via LocalForage (<code>rom_global_&lt;gameId&gt;</code>).
		</li>
	</ul>

	<h2 class="text-xl font-semibold mt-8 mb-3">Purposes and Legal Bases</h2>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>
			<strong>Provide the service:</strong> Account, session, and seed data are necessary to provide
			requested features (legal basis: contract or legitimate interests).
		</li>
		<li>
			<strong>Security:</strong> We use secure cookies and standard safeguards to protect accounts (legal
			basis: legitimate interests).
		</li>
	</ul>

	<h2 class="text-xl font-semibold mt-8 mb-3">Retention</h2>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>
			<strong>Sessions:</strong> Expire automatically based on the session expiry set by the server.
		</li>
		<li>
			<strong>Seeds:</strong> Retained so you can revisit results. You may delete your account to remove
			associations; contact us for deletion if needed.
		</li>
		<li>
			<strong>On‑device storage:</strong> Stored until you clear it or use in‑app controls to remove
			items.
		</li>
	</ul>

	<h2 class="text-xl font-semibold mt-8 mb-3">Data Sharing</h2>
	<p class="text-slate-700 dark:text-slate-300">
		The app calls a backend randomizer service to generate seeds. We forward only the configuration
		options required to create the seed. We do not send your password or session cookie to third
		parties. If external processors are used, we ensure appropriate agreements and safeguards are in
		place.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Your Rights</h2>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>Access your data</li>
		<li>Rectify inaccuracies</li>
		<li>Erase your account and associated seeds (subject to technical constraints)</li>
		<li>Object to or restrict processing where applicable</li>
		<li>Data portability for information you provided</li>
	</ul>
	<p class="text-slate-700 dark:text-slate-300">
		To exercise these rights, contact the project maintainers via the repository or app support
		channel.
	</p>

	<h2 class="text-xl font-semibold mt-8 mb-3">Security</h2>
	<ul class="list-disc list-inside space-y-1 text-slate-700 dark:text-slate-300">
		<li>
			Session cookie is <code>HttpOnly</code>, <code>Secure</code> in production, and
			<code>SameSite=Lax</code>.
		</li>
		<li>Passwords are hashed with Argon2id.</li>
		<li>All traffic should be served over TLS/HTTPS in production.</li>
	</ul>

	<h2 class="text-xl font-semibold mt-8 mb-3">Contact</h2>
	<p class="text-slate-700 dark:text-slate-300">
		For privacy questions or requests, please open an issue on the project or use the support
		contact provided in the repository.
	</p>

	<p class="text-sm text-slate-500 mt-8">Last updated: {updatedAt}</p>
</div>
