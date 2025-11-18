<script lang="ts">
    import { page } from "$app/state";
    import Button from "$lib/components/ui/Button.svelte";
    import Select from "$lib/components/ui/Select.svelte";
    import type {
        ConfigOptions,
        ConfigPreset,
        PresetOptions,
    } from "$lib/types";
    import { onMount } from "svelte";

    interface Props {
        onApply: (options: PresetOptions) => void;
        currentOptions: ConfigOptions;
        selectedGames: string[];
    }

    let { onApply, currentOptions, selectedGames }: Props = $props();

    let presets = $state<ConfigPreset[]>([]);
    let selectedPresetId = $state("");
    let showSaveDialog = $state(false);
    let newPresetName = $state("");
    let newPresetDescription = $state("");
    let newPresetIsSystem = $state(false);

    const user = $derived(page.data.user);

    async function loadPresets() {
        try {
            const res = await fetch("/api/presets");
            if (res.ok) {
                const data = (await res.json()) as ConfigPreset[] | null;
                presets = Array.isArray(data) ? data : [];
            }
        } catch (e) {
            console.error("Failed to load presets", e);
        }
    }

    onMount(() => {
        loadPresets();
    });

    function applyPreset() {
        const preset = presets.find((p) => p.id === selectedPresetId);
        if (preset) {
            onApply(preset.options);
        }
    }

    async function savePreset() {
        if (!newPresetName) return;

        try {
            const res = await fetch("/api/presets", {
                method: "POST",
                body: JSON.stringify({
                    name: newPresetName,
                    description: newPresetDescription,
                    isSystem: newPresetIsSystem,
                    options: {
                        global: currentOptions.global,
                        perGame: currentOptions.perGame,
                        selectedGames: selectedGames,
                    },
                }),
            });
            if (res.ok) {
                showSaveDialog = false;
                loadPresets();
                newPresetName = "";
                newPresetDescription = "";
                newPresetIsSystem = false;
            } else {
                alert("Failed to save preset");
            }
        } catch (e) {
            console.error("Failed to save preset", e);
            alert("Failed to save preset");
        }
    }

    async function deletePreset() {
        if (!selectedPresetId) return;
        if (!confirm("Are you sure you want to delete this preset?")) return;

        try {
            const res = await fetch(`/api/presets/${selectedPresetId}`, {
                method: "DELETE",
            });
            if (res.ok) {
                selectedPresetId = "";
                loadPresets();
            } else {
                alert("Failed to delete preset");
            }
        } catch (e) {
            console.error("Failed to delete preset", e);
            alert("Failed to delete preset");
        }
    }
</script>

<div class="flex flex-wrap items-end gap-2">
    <div class="flex flex-col gap-1 min-w-[200px]">
        <label
            for="preset-select"
            class="text-xs text-slate-600 dark:text-slate-400">Presets</label
        >
        <Select
            id="preset-select"
            bind:value={selectedPresetId}
            items={presets.map((p) => ({
                value: p.id,
                name: p.name + (p.isSystem ? " (System)" : ""),
            }))}
            placeholder="Select a preset..."
            className="text-xs py-1.5 w-full"
        />
    </div>
    <div class="flex gap-1">
        <Button
            onclick={applyPreset}
            disabled={!selectedPresetId}
            size="xs"
            variant="primary"
            className="!py-1.5">Load</Button
        >
        {#if user}
            <Button
                onclick={() => (showSaveDialog = true)}
                size="xs"
                variant="secondary"
                className="!py-1.5">Save</Button
            >
        {/if}
        {#if selectedPresetId && user}
            {@const preset = presets.find((p) => p.id === selectedPresetId)}
            {#if preset && (preset.userId === user.id || (user.role === "admin" && preset.isSystem))}
                <Button
                    onclick={deletePreset}
                    size="xs"
                    variant="danger"
                    className="!py-1.5">Delete</Button
                >
            {/if}
        {/if}
    </div>
</div>

{#if showSaveDialog}
    <div
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4"
    >
        <div
            class="bg-white dark:bg-slate-800 p-6 rounded-lg shadow-xl w-full max-w-md"
        >
            <h2 class="text-xl font-bold mb-4 dark:text-white">Save Preset</h2>
            <div class="mb-4">
                <label
                    for="preset-name"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                    >Name</label
                >
                <input
                    type="text"
                    bind:value={newPresetName}
                    id="preset-name"
                    class="w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 dark:bg-slate-700 dark:border-slate-600 dark:text-white px-3 py-2"
                    placeholder="My Awesome Preset"
                />
            </div>
            <div class="mb-4">
                <label
                    for="preset-description"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1"
                    >Description</label
                >
                <textarea
                    bind:value={newPresetDescription}
                    id="preset-description"
                    class="w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 dark:bg-slate-700 dark:border-slate-600 dark:text-white px-3 py-2"
                    rows="3"
                    placeholder="Optional description"
                ></textarea>
            </div>
            {#if user.role === "admin"}
                <div class="mb-6 flex items-center">
                    <input
                        type="checkbox"
                        bind:checked={newPresetIsSystem}
                        id="system-preset"
                        class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                    />
                    <label
                        for="system-preset"
                        class="ml-2 block text-sm text-gray-900 dark:text-gray-300"
                        >System Preset (Visible to everyone)</label
                    >
                </div>
            {/if}
            <div class="flex justify-end gap-2">
                <Button
                    onclick={() => (showSaveDialog = false)}
                    variant="secondary">Cancel</Button
                >
                <Button onclick={savePreset} disabled={!newPresetName}
                    >Save</Button
                >
            </div>
        </div>
    </div>
{/if}
