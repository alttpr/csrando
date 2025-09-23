<script lang="ts">
	import * as m from '$lib/paraglide/messages';
    import Card from '../ui/Card.svelte';
    import Progressbar from '../ui/Progressbar.svelte';

    interface Props {
        gameId: string;
        gameName: string;
        expectedFileExtensions?: string;
        hashStatus?: 'no_rom' | 'checking' | 'verified' | 'mismatch' | 'error' | 'uploaded_no_verify';
        calculatedHash?: string | undefined;
        expectedHash?: string | undefined;
        fileName?: string | null;
        useCard?: boolean;
        onFileSelected?: (payload: { gameId: string; file: File }) => void;
    }

    let {
        gameId,
        gameName,
        expectedFileExtensions = '.rom,.sfc,.smc,.zip',
        hashStatus = 'no_rom',
        calculatedHash = undefined,
        expectedHash = undefined,
        fileName = null,
        useCard = true,
        onFileSelected
    }: Props = $props();
    function handleFileSelect(event: Event) {
        const fileInput = event.target as HTMLInputElement;
        if (fileInput.files && fileInput.files.length > 0) {
            const file = fileInput.files[0];
            onFileSelected?.({ gameId, file });
            // Reset value to ensure selecting the same file again triggers change
            fileInput.value = '';
        }
    }
	function getStatusColor() {
		switch (hashStatus) {
			case 'verified':
				return 'text-green-500 dark:text-green-400';
			case 'mismatch':
				return 'text-red-500 dark:text-red-400';
			case 'checking':
				return 'text-yellow-500 dark:text-yellow-400';
			case 'error':
				return 'text-red-500 dark:text-red-400';
			case 'uploaded_no_verify':
				return 'text-blue-500 dark:text-blue-400';
			default:
				return 'text-slate-500 dark:text-slate-400';
		}
	}

	function getStatusMessage() {
		switch (hashStatus) {
			case 'verified':
				return m.seed_rom_verified();
			case 'mismatch':
				return m.seed_rom_hash_mismatch();
			case 'checking':
				return m.seed_checking_rom();
			case 'error':
				return m.seed_rom_hash_error();
			case 'uploaded_no_verify':
				return m.seed_rom_uploaded_no_verify();
			case 'no_rom':
				return m.seed_no_rom_uploaded();
			default:
				return '';
		}
	}

</script>

<div>
{#if useCard}
	<Card title={gameName} className="mb-4">
		<div class="content-wrapper">
			{#if hashStatus === 'verified'}
				<div class="mt-1 text-xs {getStatusColor()}">
					{getStatusMessage()}
				</div>
				{#if fileName}
					<p class="text-xs text-slate-600 dark:text-slate-400 truncate" title={fileName}>
						({fileName})
					</p>
				{/if}
			{:else}
				<div
					class="flex flex-col sm:flex-row items-start sm:items-center space-y-1 sm:space-y-0 sm:space-x-3"
				>
					<div class="flex-grow">
						<div class="relative">
							<input
								type="file"
								accept={expectedFileExtensions}
								onchange={handleFileSelect}
								class="absolute inset-0 opacity-0 w-full h-full cursor-pointer z-10"
								aria-describedby={`file_input_help_${gameId}`}
								id={`file_input_${gameId}`}
							/>
							<button
								type="button"
								class="w-full flex items-center justify-center px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 border border-slate-300 rounded-lg hover:bg-slate-200 dark:bg-slate-600 dark:text-slate-200 dark:border-slate-500 dark:hover:bg-slate-500 transition-colors"
							>
								<svg
									class="w-4 h-4 mr-2"
									fill="none"
									stroke="currentColor"
									viewBox="0 0 24 24"
									xmlns="http://www.w3.org/2000/svg"
								>
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
									></path>
								</svg>
								{fileName ? 'Change File' : 'Upload ROM'}
							</button>
						</div>
						<p
							class="mt-1.5 text-xs text-slate-600 dark:text-slate-300"
							id={`file_input_help_${gameId}`}
						>
							{m.seed_page_rom_file_type_hint
								? m.seed_page_rom_file_type_hint({
										extensions: expectedFileExtensions
									})
								: `Accepted file types: ${expectedFileExtensions}`}
						</p>
					</div>
					{#if fileName}
						<p class="text-2xs text-slate-700 dark:text-slate-300 truncate" title={fileName}>
							{fileName}
						</p>
					{/if}
				</div>

				<div class="mt-1 text-xs {getStatusColor()}">
					{getStatusMessage()}
				</div>

				{#if hashStatus === 'checking'}
					<Progressbar progress={50} color="yellow" className="mt-1 h-1.5 dark:bg-slate-600" />
				{/if}

				{#if hashStatus === 'mismatch' && calculatedHash && expectedHash}
					<div class="mt-0.5 text-2xs text-slate-600 dark:text-slate-300">
						<p>
							{m.seed_page_rom_expected_hash_value
								? m.seed_page_rom_expected_hash_value({ hash: expectedHash })
								: `${m.seed_expected_hash()} ${expectedHash}`}
						</p>
						<p>
							{m.seed_page_rom_calculated_hash_value
								? m.seed_page_rom_calculated_hash_value({
										hash: calculatedHash
									})
								: `${m.seed_calculated_hash()} ${calculatedHash}`}
						</p>
					</div>
				{/if}

				{#if hashStatus === 'uploaded_no_verify' && calculatedHash}
					<div class="mt-0.5 text-2xs text-slate-600 dark:text-slate-300">
						<p>
							{m.seed_page_rom_calculated_hash_value
								? m.seed_page_rom_calculated_hash_value({
										hash: calculatedHash
									})
								: `${m.seed_calculated_hash()} ${calculatedHash}`}
						</p>
					</div>
				{/if}
			{/if}
		</div>
	</Card>
{:else}
	<div
		class="mb-3 p-3 border border-slate-300 dark:border-slate-600 rounded-md bg-white dark:bg-slate-700"
	>
		<h4 class="text-base font-medium text-slate-800 dark:text-slate-200 mb-1">
			{gameName}
		</h4>
		{#if hashStatus === 'verified'}
			<div class="mt-1 text-xs {getStatusColor()}">
				{getStatusMessage()}
			</div>
			{#if fileName}
				<p class="text-xs text-slate-600 dark:text-slate-400 truncate" title={fileName}>
					({fileName})
				</p>
			{/if}
		{:else}
			<div
				class="flex flex-col sm:flex-row items-start sm:items-center space-y-1 sm:space-y-0 sm:space-x-3"
			>
				<div class="flex-grow">
					<div class="relative">
						<input
							type="file"
							accept={expectedFileExtensions}
							onchange={handleFileSelect}
							class="absolute inset-0 opacity-0 w-full h-full cursor-pointer z-10"
							aria-describedby={`file_input_help_${gameId}`}
							id={`file_input_${gameId}`}
						/>
						<button
							type="button"
							class="w-full flex items-center justify-center px-4 py-2 text-sm font-medium text-slate-700 bg-slate-100 border border-slate-300 rounded-lg hover:bg-slate-200 dark:bg-slate-600 dark:text-slate-200 dark:border-slate-500 dark:hover:bg-slate-500 transition-colors"
						>
							<svg
								class="w-4 h-4 mr-2"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
								xmlns="http://www.w3.org/2000/svg"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
								></path>
							</svg>
							{fileName ? 'Change File' : 'Upload ROM'}
						</button>
					</div>
					<p
						class="mt-1.5 text-xs text-slate-600 dark:text-slate-300"
						id={`file_input_help_${gameId}`}
					>
						{m.seed_page_rom_file_type_hint
							? m.seed_page_rom_file_type_hint({
									extensions: expectedFileExtensions
								})
							: `Accepted file types: ${expectedFileExtensions}`}
					</p>
				</div>
				{#if fileName}
					<p class="text-2xs text-slate-700 dark:text-slate-300 truncate" title={fileName}>
						{fileName}
					</p>
				{/if}
			</div>

			<div class="mt-1 text-xs {getStatusColor()}">
				{getStatusMessage()}
			</div>

			{#if hashStatus === 'checking'}
				<Progressbar progress={50} color="yellow" className="mt-1 h-1.5 dark:bg-slate-600" />
			{/if}

			{#if hashStatus === 'mismatch' && calculatedHash && expectedHash}
				<div class="mt-0.5 text-2xs text-slate-600 dark:text-slate-300">
					<p>
						{m.seed_page_rom_expected_hash_value
							? m.seed_page_rom_expected_hash_value({ hash: expectedHash })
							: `${m.seed_expected_hash()} ${expectedHash}`}
					</p>
					<p>
						{m.seed_page_rom_calculated_hash_value
							? m.seed_page_rom_calculated_hash_value({ hash: calculatedHash })
							: `${m.seed_calculated_hash()} ${calculatedHash}`}
					</p>
				</div>
			{/if}

			{#if hashStatus === 'uploaded_no_verify' && calculatedHash}
				<div class="mt-0.5 text-2xs text-slate-600 dark:text-slate-300">
					<p>
						{m.seed_page_rom_calculated_hash_value
							? m.seed_page_rom_calculated_hash_value({ hash: calculatedHash })
							: `${m.seed_calculated_hash()} ${calculatedHash}`}
					</p>
				</div>
			{/if}
		{/if}
	</div>
{/if}
</div>
