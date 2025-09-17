<script lang="ts">
	import * as m from '$lib/paraglide/messages';
	import Button from '$lib/components/ui/Button.svelte';
	import Card from '$lib/components/ui/Card.svelte';
</script>

<div class="container mx-auto px-4 py-8 text-center max-w-4xl">
	<div class="mb-10">
		<h1 class="text-5xl font-bold mb-4 text-primary-500 dark:text-primary-400">
			{m.landing_title()}
		</h1>
		<p class="text-xl mb-8 text-slate-700 dark:text-slate-300">{m.landing_intro()}</p>
		<Button
			href="/config/alttpr"
			variant="primary"
			size="lg"
			className="transform hover:-translate-y-1"
		>
			{m.landing_cta()}
		</Button>
	</div>

	<div class="grid grid-cols-1 md:grid-cols-2 gap-6 mt-12">
		<Card title="Easy Randomization" className="text-left">
			<p class="text-slate-700 dark:text-slate-300">
				Create randomized game seeds with a simple, easy-to-use interface. Perfect for both
				beginners and experienced players.
			</p>
		</Card>

		<Card title="Multi-Game Support" className="text-left">
			<p class="text-slate-700 dark:text-slate-300">
				Our platform supports multiple games and allows you to customize randomization settings for
				each one to create your perfect experience.
			</p>
		</Card>
	</div>
</div>
