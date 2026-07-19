<script lang="ts">
	import ConfigPage from "../+page.svelte";
	import type { PageData } from "./$types";

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();
</script>

<ConfigPage {data} />
