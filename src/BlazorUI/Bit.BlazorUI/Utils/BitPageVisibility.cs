namespace Bit.BlazorUI;

/// <summary>
/// The Page Visibility API provides events you can watch for to know when a document becomes visible or hidden.
/// <br />
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API"/>
/// </summary>
public class BitPageVisibility(IJSRuntime js) : IAsyncDisposable
{
    private bool _isInitialized;
    private DotNetObjectReference<BitPageVisibility>? _dotnetObj;



    /// <summary>
    /// Fires when the content of the document has become visible or hidden.
    /// </summary>
    public event Func<bool, Task>? OnChange;

    /// <summary>
    /// Fires with <c>true</c> when the window has lost the focus and with <c>false</c> when it has got it back.
    /// </summary>
    /// <remarks>
    /// A window that is covered by another one, or whose focus went to the dev tools or to an iframe, is not
    /// hidden - <see cref="OnChange"/> never fires for it. This is the event to watch to hold something back
    /// while the page is not the one being worked in.
    /// </remarks>
    public event Func<bool, Task>? OnWindowFocusChange;



    /// <summary>
    /// Initializes the js api of the page visibility utility.
    /// </summary>
    public async Task Init()
    {
        if (_isInitialized) return;

        _isInitialized = true;

        _dotnetObj = DotNetObjectReference.Create(this);

        await js.InvokeVoid("BitBlazorUI.PageVisibility.init", _dotnetObj);
    }



    [JSInvokable("VisibilityChanged")]
    public async Task _VisibilityChanged(bool hidden)
    {
        var onChange = OnChange;
        if (onChange is not null)
        {
            await onChange(hidden);
        }
    }

    [JSInvokable("WindowFocusChanged")]
    public async Task _WindowFocusChanged(bool blurred)
    {
        var onWindowFocusChange = OnWindowFocusChange;
        if (onWindowFocusChange is not null)
        {
            await onWindowFocusChange(blurred);
        }
    }



    public async ValueTask DisposeAsync()
    {
        if (_isInitialized)
        {
            // Awaits the JS teardown so the global visibilitychange/blur/focus listeners are actually
            // cleared before this instance goes away, rather than left behind by a fire-and-forget call.
            // The JS-side init guard is reset too, so a future instance can re-init.
            try
            {
                await js.InvokeVoid("BitBlazorUI.PageVisibility.dispose");
            }
            catch (Exception ex) when (ex is JSDisconnectedException or JSException or ObjectDisposedException or OperationCanceledException)
            {
                // Disposal must never throw: this runs from the DI container's scope disposal, where an
                // exception escaping here would abort the disposal of everything else in the scope. Every
                // exception caught means the same thing - the JS listeners can no longer be reached: the
                // circuit is already gone (JSDisconnectedException), the teardown itself failed in the
                // browser (JSException), the runtime has been torn down (ObjectDisposedException), or the
                // interop call timed out (OperationCanceledException). The local cleanup in the finally
                // below is then all that is left to do.
            }
            finally
            {
                // Local cleanup runs in finally so it always executes even if the JS teardown throws
                // something outside the caught set above (e.g. InvalidOperationException), guaranteeing
                // deterministic disposal.
                _isInitialized = false;
                _dotnetObj?.Dispose();
            }
        }

        GC.SuppressFinalize(this);
    }
}
