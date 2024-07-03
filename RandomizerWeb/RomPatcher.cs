namespace RandomizerWeb;
using Microsoft.JSInterop;

public class RomPatcher : IAsyncDisposable
{
    private Lazy<IJSObjectReference> _patcherJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    public RomPatcher(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private async Task WaitForReference()
    {
        if (_patcherJsRef.IsValueCreated is false)
        {
            _patcherJsRef = new(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/RomPatcher.js"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_patcherJsRef.IsValueCreated)
        {
            await _patcherJsRef.Value.DisposeAsync();
        }
    }

    public async Task<bool> PatchRomAsync(string fileName, byte[] bpsPatch, Dictionary<int, byte[]> patchData)
    {
        await WaitForReference();
        return await _patcherJsRef.Value.InvokeAsync<bool>("patchRom", fileName, bpsPatch, patchData);
    }    
}
