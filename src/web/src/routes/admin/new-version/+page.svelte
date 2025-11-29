<script lang="ts">
  import Card from "$lib/components/ui/Card.svelte";
  import Button from "$lib/components/ui/Button.svelte";
  import type { ActionData, PageData } from "./$types";

  const { data, form } = $props<{
    data: PageData;
    form: ActionData | null;
  }>();

  type CreateSuccessAction = {
    type: "create";
    success: true;
    result: {
      versionId: string;
      versionTag: string;
      randomizerId: string | null;
      metadataId: string;
      canonicalMetadataId: string;
      activated: boolean;
      patchSha256: string;
      gitCommitHash: string | null;
      buildDate: string;
    };
  };
  type CreateFailureAction = {
    type: "create";
    success: false;
    message: string;
    fieldErrors?: Record<string, string>;
    values?: {
      baseVersion?: string;
      randomizerId?: string;
      metadataId?: string;
      tag?: string;
      activate?: boolean;
    };
  };

  const createSuccessAction = $derived<CreateSuccessAction | null>(
    form && form.type === "create" && form.success === true
      ? (form as CreateSuccessAction)
      : null,
  );
  const createSuccess = $derived(createSuccessAction?.result ?? null);

  const createFailure = $derived<CreateFailureAction | null>(
    form && form.type === "create" && form.success === false
      ? (form as CreateFailureAction)
      : null,
  );
  const createErrorMessage = $derived(createFailure?.message ?? null);
  const createFieldErrors = $derived(createFailure?.fieldErrors ?? {});
  const createValues = $derived(
    (createFailure?.values ?? {}) as {
      baseVersion?: string;
      randomizerId?: string;
      metadataId?: string;
      tag?: string;
      activate?: boolean;
      gitCommitHash?: string;
      buildDate?: string;
    },
  );
  const authError = $derived(
    form && form.type === "authenticate" && !form.success ? form.message : null,
  );

  function formatTimestamp(value: string): string {
    const asDate = new Date(value);
    if (Number.isNaN(asDate.getTime())) return value;
    return asDate.toLocaleString();
  }
</script>

<svelte:head>
  <title>Randomizer version management</title>
</svelte:head>

<div class="container mx-auto max-w-5xl px-6 py-10 space-y-6">
  <div
    class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between"
  >
    <div>
      <h1 class="text-3xl font-bold text-slate-900 dark:text-slate-50">
        Remote version management
      </h1>
      <p class="mt-1 text-slate-600 dark:text-slate-400">
        Select a base IPS or BPS patch and capture metadata from the backend
        without a full deployment.
      </p>
    </div>
    {#if data.authorized}
      <form method="post" action="?/logout">
        <Button type="submit" variant="secondary">Sign out</Button>
      </form>
    {/if}
  </div>

  {#if !data.secretConfigured}
    <Card
      title="Admin access not configured"
      subtitle="Set PRIVATE_ADMIN_VERSION_TOKEN in the environment and reload to enable remote version creation."
    >
      <p class="text-sm text-slate-600 dark:text-slate-300">
        For now, a shared one-time token protects this page. Configure
        <code class="font-mono">PRIVATE_ADMIN_VERSION_TOKEN</code> on the server,
        then refresh and enter the token to unlock the form.
      </p>
    </Card>
  {:else if !data.authorized}
    <Card
      title="Admin access"
      subtitle="Enter the shared secret token to unlock remote version management."
    >
      {#if authError}
        <div
          class="mb-4 rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700 dark:border-red-400/40 dark:bg-red-900/40 dark:text-red-200"
        >
          {authError}
        </div>
      {/if}
      <form method="post" action="?/authenticate" class="space-y-4">
        <div>
          <label
            for="token"
            class="block text-sm font-medium text-slate-700 dark:text-slate-200"
            >Access token</label
          >
          <input
            id="token"
            name="token"
            type="password"
            required
            class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            autocomplete="off"
          />
          <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
            The token is stored in an HTTP-only cookie for 12 hours on this
            device.
          </p>
        </div>
        <Button type="submit">Unlock</Button>
      </form>
    </Card>
  {:else}
    {#if createSuccess}
      <div
        class="rounded-md border border-green-200 bg-green-50 p-4 text-sm text-green-800 dark:border-green-400/40 dark:bg-green-900/30 dark:text-green-100"
      >
        <h2 class="text-lg font-semibold">
          Version {createSuccess.versionTag} saved
        </h2>
        <ul class="mt-2 space-y-1 font-mono text-xs">
          <li>
            <span class="font-semibold">Version ID:</span>
            {createSuccess.versionId}
          </li>
          <li>
            <span class="font-semibold">Randomizer:</span>
            {createSuccess.randomizerId ?? "(not set)"}
          </li>
          <li>
            <span class="font-semibold">Metadata lookup:</span>
            {createSuccess.metadataId} → {createSuccess.canonicalMetadataId}
          </li>
          <li>
            <span class="font-semibold">Patch sha256:</span>
            {createSuccess.patchSha256}
          </li>
          <li>
            <span class="font-semibold">Build date:</span>
            {formatTimestamp(createSuccess.buildDate)}
          </li>
          <li>
            <span class="font-semibold">Git commit:</span>
            {createSuccess.gitCommitHash ?? "—"}
          </li>
          <li>
            <span class="font-semibold">Activated now:</span>
            {createSuccess.activated ? "yes" : "no"}
          </li>
        </ul>
      </div>
    {/if}

    {#if createErrorMessage}
      <div
        class="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-700 dark:border-red-400/40 dark:bg-red-900/40 dark:text-red-200"
      >
        {createErrorMessage}
      </div>
    {/if}

    <Card
      title="Create a new randomizer version"
      subtitle="Provide the IPS or BPS base patch used by the backend and snapshot the latest metadata for this randomizer."
    >
      <form
        method="post"
        action="?/create"
        enctype="multipart/form-data"
        class="space-y-6"
      >
        <div class="grid gap-6 md:grid-cols-2">
          <div>
            <label
              for="baseVersion"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Base version label</label
            >
            <input
              id="baseVersion"
              name="baseVersion"
              type="text"
              required
              value={createValues.baseVersion ?? ""}
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Example: <code class="font-mono">v2024-05-15</code>. The final
              version tag will append the randomizer id unless overridden.
            </p>
            {#if createFieldErrors.baseVersion}
              <p
                class="mt-2 text-xs font-medium text-red-600 dark:text-red-300"
              >
                {createFieldErrors.baseVersion}
              </p>
            {/if}
          </div>
          <div>
            <label
              for="randomizerId"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Randomizer ID</label
            >
            <input
              id="randomizerId"
              name="randomizerId"
              type="text"
              required
              value={createValues.randomizerId ?? ""}
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Matches the backend game identifier (e.g. <code class="font-mono"
                >alttpr</code
              >, <code class="font-mono">z1r</code>).
            </p>
            {#if createFieldErrors.randomizerId}
              <p
                class="mt-2 text-xs font-medium text-red-600 dark:text-red-300"
              >
                {createFieldErrors.randomizerId}
              </p>
            {/if}
          </div>
        </div>
        <div class="grid gap-6 md:grid-cols-2">
          <div>
            <label
              for="metadataId"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Metadata ID (optional)</label
            >
            <input
              id="metadataId"
              name="metadataId"
              type="text"
              value={createValues.metadataId ?? ""}
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Leave blank to reuse the randomizer id. The backend index resolves
              aliases automatically.
            </p>
          </div>
          <div>
            <label
              for="tag"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Version tag override (optional)</label
            >
            <input
              id="tag"
              name="tag"
              type="text"
              value={createValues.tag ?? ""}
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Provide a custom version tag if the automatic <code
                class="font-mono">&lt;base&gt;-&lt;randomizer&gt;</code
              > suffix should not be used.
            </p>
            {#if createFieldErrors.versionTag}
              <p
                class="mt-2 text-xs font-medium text-red-600 dark:text-red-300"
              >
                {createFieldErrors.versionTag}
              </p>
            {/if}
          </div>
        </div>
        <div class="grid gap-6 md:grid-cols-2">
          <div>
            <label
              for="gitCommitHash"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Git commit hash (optional)</label
            >
            <input
              id="gitCommitHash"
              name="gitCommitHash"
              type="text"
              value={createValues.gitCommitHash ?? ""}
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Provide the backend commit associated with this build (7-40 hex
              characters).
            </p>
            {#if createFieldErrors.gitCommitHash}
              <p
                class="mt-2 text-xs font-medium text-red-600 dark:text-red-300"
              >
                {createFieldErrors.gitCommitHash}
              </p>
            {/if}
          </div>
          <div>
            <label
              for="buildDate"
              class="block text-sm font-medium text-slate-700 dark:text-slate-200"
              >Build date (optional)</label
            >
            <input
              id="buildDate"
              name="buildDate"
              type="text"
              value={createValues.buildDate ?? ""}
              placeholder="now or 2025-11-04T12:34:56Z"
              class="mt-1 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-slate-900 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-400 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-100"
            />
            <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
              Leave blank to default to the current time. Accepts <code
                class="font-mono">now</code
              > or an ISO 8601 timestamp.
            </p>
            {#if createFieldErrors.buildDate}
              <p
                class="mt-2 text-xs font-medium text-red-600 dark:text-red-300"
              >
                {createFieldErrors.buildDate}
              </p>
            {/if}
          </div>
        </div>
        <div>
          <label
            for="basePatch"
            class="block text-sm font-medium text-slate-700 dark:text-slate-200"
            >IPS or BPS base patch</label
          >
          <input
            id="basePatch"
            name="basePatch"
            type="file"
            accept=".ips,.bps"
            required
            class="mt-1 block w-full text-sm text-slate-600 file:mr-4 file:rounded-md file:border-0 file:bg-indigo-50 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-indigo-700 hover:file:bg-indigo-100 dark:text-slate-300 dark:file:bg-indigo-500/20 dark:file:text-indigo-200"
          />
          <p class="mt-1 text-xs text-slate-500 dark:text-slate-400">
            Select the IPS or BPS base patch produced by the backend. It is
            stored in the database as base64 for future seeds.
          </p>
          {#if createFieldErrors.basePatch}
            <p class="mt-2 text-xs font-medium text-red-600 dark:text-red-300">
              {createFieldErrors.basePatch}
            </p>
          {/if}
        </div>
        <label
          class="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-200"
        >
          <input
            type="checkbox"
            name="activate"
            value="on"
            checked={createValues.activate ?? false}
            class="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500"
          />
          <span
            >Mark this version active for the selected randomizer immediately.</span
          >
        </label>
        <Button type="submit">Create version</Button>
      </form>
    </Card>

    <Card title="Recent versions">
      {#if data.recentVersions.length === 0}
        <p class="text-sm text-slate-600 dark:text-slate-300">
          No versions have been stored yet.
        </p>
      {:else}
        <div class="overflow-x-auto">
          <table
            class="min-w-full divide-y divide-slate-200 dark:divide-slate-700 text-sm"
          >
            <thead class="bg-slate-50 dark:bg-slate-800/60">
              <tr>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Version tag
                </th>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Randomizer
                </th>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Active
                </th>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Build date
                </th>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Git commit
                </th>
                <th
                  class="px-4 py-2 text-left font-semibold text-slate-600 dark:text-slate-300"
                >
                  Created
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 dark:divide-slate-800">
              {#each data.recentVersions as version (version.id)}
                <tr class="hover:bg-slate-50 dark:hover:bg-slate-800/50">
                  <td class="px-4 py-2 font-mono">{version.versionTag}</td>
                  <td class="px-4 py-2 font-mono text-xs">
                    {version.randomizerId ?? "—"}
                  </td>
                  <td class="px-4 py-2">
                    {#if version.isActive}
                      <span
                        class="rounded-full bg-green-100 px-2 py-0.5 text-xs font-semibold text-green-700 dark:bg-green-500/20 dark:text-green-200"
                      >
                        Active
                      </span>
                    {:else}
                      <span
                        class="rounded-full bg-slate-200 px-2 py-0.5 text-xs font-semibold text-slate-600 dark:bg-slate-700 dark:text-slate-300"
                      >
                        Inactive
                      </span>
                    {/if}
                  </td>
                  <td
                    class="px-4 py-2 text-xs text-slate-600 dark:text-slate-300"
                  >
                    {formatTimestamp(version.buildDate)}
                  </td>
                  <td class="px-4 py-2 font-mono text-xs">
                    {version.gitCommitHash ?? "—"}
                  </td>
                  <td
                    class="px-4 py-2 text-xs text-slate-600 dark:text-slate-300"
                  >
                    {formatTimestamp(version.createdAt)}
                  </td>
                </tr>
              {/each}
            </tbody>
          </table>
        </div>
      {/if}
    </Card>
  {/if}
</div>
