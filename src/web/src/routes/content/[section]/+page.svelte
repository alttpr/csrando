<script lang="ts">
    import { siteNameForMode } from "$lib/config/site";
    import type { SiteMode } from "$lib/config/site";
    import type { PageData } from "./$types";

    let { data } = $props<{ data: PageData }>();
    const Component = $derived(data?.content);
    const contentKey = $derived(data?.contentKey ?? data?.section);
    const metadata = $derived(data?.metadata ?? null);
    const pageTitle = $derived(metadata?.title ?? "Help");
    const siteMode = $derived((data?.siteMode ?? "all") as SiteMode);
    const siteName = $derived(siteNameForMode(siteMode));
    const fullTitle = $derived(`${pageTitle} — ${siteName}`);
    const pageDescription = $derived(metadata?.description ?? null);
</script>

<svelte:head>
    <title>{fullTitle}</title>
    {#if pageDescription}
        <meta name="description" content={pageDescription} />
    {/if}
</svelte:head>

{#if Component}
    <section class="mx-auto max-w-4xl px-4 py-10">
        <article class="prose prose-slate dark:prose-invert max-w-none">
            {#key contentKey ?? Component}
                <Component />
            {/key}
        </article>
    </section>
{:else}
    <p
        class="mx-auto max-w-4xl px-4 py-10 text-sm text-surface-600 dark:text-surface-300"
    >
        We couldn't find that content. Please check the URL or try again later.
    </p>
{/if}
