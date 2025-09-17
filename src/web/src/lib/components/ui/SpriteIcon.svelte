<script lang="ts">
	import { loadAtlas, resolveSprite, spriteStyle, type SpriteRenderInfo } from '$lib/sprites/atlas';

	let {
		game,
		value,
		name = undefined,
		imagePath = undefined,
		size = undefined,
		className = '',
		eager = false,
		class: classAttr = ''
	} = $props<{
		game: string;
		value: string;
		name?: string;
		imagePath?: string;
		size?: number;
		className?: string;
		eager?: boolean;
		class?: string;
	}>();
	const mergedClass = $derived(`${className} ${classAttr}`.trim());

	let info = $state<SpriteRenderInfo | null>(null);
	let failed = $state(false);

	async function ensureAtlas() {
		if (info || failed || !game || !value) return;
		const atlas = await loadAtlas(game).catch(() => null);
		if (!atlas) {
			failed = true; // fallback path
			return;
		}
		const resolved = resolveSprite(game, value);
		if (resolved) info = resolved;
		else failed = true;
	}

	$effect.pre(() => {
		if (eager) ensureAtlas();
	});

	let lastKey = $state<string | null>(null);
	$effect(() => {
		// Only reset when identifying props change to avoid render loops
		const key = `${game}|${value}`;
		if (key !== lastKey) {
			lastKey = key;
			info = null;
			failed = false;
			if (eager) void ensureAtlas();
		}
	});

	function handleMouseEnter() {
		if (!info && !failed) void ensureAtlas();
	}
</script>

{#if !value}
	<!-- Friendly default icon for "Default sprite" -->
	<div
		class="sprite-icon default {mergedClass}"
		role="img"
		aria-label={name || 'Default sprite'}
		style={`width:${size || 24}px; height:${size || 24}px;`}
	>
		<svg viewBox="0 0 24 24" width="100%" height="100%" aria-hidden="true">
			<defs>
				<linearGradient id="spr-def-grad" x1="0" y1="0" x2="1" y2="1">
					<stop offset="0%" stop-color="#6366f1" />
					<stop offset="100%" stop-color="#22d3ee" />
				</linearGradient>
				<linearGradient id="spr-def-star" x1="0" y1="0" x2="1" y2="1">
					<stop offset="0%" stop-color="#fff" />
					<stop offset="100%" stop-color="#e0e7ff" />
				</linearGradient>
			</defs>
			<rect x="1" y="1" width="22" height="22" rx="5" fill="url(#spr-def-grad)" opacity="0.9" />
			<g transform="translate(12,12)">
				<path
					d="M0,-6 L1.6,-1.6 L6,0 L1.6,1.6 L0,6 L-1.6,1.6 L-6,0 L-1.6,-1.6 Z"
					fill="url(#spr-def-star)"
					stroke="#ffffff"
					stroke-opacity="0.6"
					stroke-width="0.6"
				/>
			</g>
		</svg>
	</div>
{:else if info}
	<!-- Render from atlas sheet -->
	<div
		class="sprite-icon {mergedClass}"
		role="img"
		aria-label={name || value}
		onmouseenter={handleMouseEnter}
		style={(() => {
			const st = spriteStyle(info!);
			if (size) {
				const scaleX = size / info!.w;
				const scaleY = size / info!.h;
				st['transform'] = `scale(${scaleX}, ${scaleY})`;
				st['transform-origin'] = 'top left';
				st['width'] = info!.w + 'px';
				st['height'] = info!.h + 'px';
			}
			return Object.entries(st)
				.map(([k, v]) => `${k}: ${v}`)
				.join(';');
		})()}
	></div>
{:else if !failed}
	<!-- Placeholder / loading skeleton -->
	<div
		class="sprite-icon placeholder animate-pulse bg-slate-200 dark:bg-slate-700 {mergedClass}"
		style={`width:${size || 24}px; height:${size || 24}px;`}
		aria-hidden="true"
		onmouseenter={handleMouseEnter}
	></div>
{:else}
	<!-- Fallback to individual PNG image -->
	{#if imagePath}
		<img
			src={imagePath}
			alt={name || value}
			class="sprite-icon-img {mergedClass}"
			width={size || undefined}
			height={size || undefined}
			loading="lazy"
			decoding="async"
		/>
	{:else}
		<div
			class="sprite-icon missing {className}"
			title={value}
			style={`width:${size || 24}px;height:${size || 24}px;`}
		></div>
	{/if}
{/if}

<style>
	.sprite-icon {
		display: inline-block;
		image-rendering: pixelated;
	}
	.sprite-icon.placeholder {
		border-radius: 4px;
	}
	.sprite-icon.missing {
		background: repeating-conic-gradient(#ccc 0% 25%, #eee 0% 50%) 50% / 8px 8px;
	}
	.sprite-icon-img {
		display: inline-block;
		image-rendering: pixelated;
	}
	.sprite-icon.default {
		border-radius: 4px;
		/* Tailwind-like ring via box-shadow */
		box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.06);
	}
	:global(.dark) .sprite-icon.missing {
		background: repeating-conic-gradient(#444 0% 25%, #666 0% 50%) 50% / 8px 8px;
	}
</style>
