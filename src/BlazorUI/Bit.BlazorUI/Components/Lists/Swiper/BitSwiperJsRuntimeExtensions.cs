using System.Diagnostics.CodeAnalysis;

namespace Bit.BlazorUI;

internal static class BitSwiperJsRuntimeExtensions
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BitSwiperState))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BitSwiperOptions))]
    internal static ValueTask BitSwiperSetup(this IJSRuntime jsRuntime,
                                             string id,
                                             ElementReference root,
                                             ElementReference container,
                                             DotNetObjectReference<BitSwiper> dotnetObj,
                                             BitSwiperOptions options)
    {
        // Deliberately not on the FastInvoke path, for the same reason as the dispose call below:
        // FastInvokeVoid swallows JSException on the in-process (WASM) runtime, which would hide a failed
        // setup. A failed setup means JS never registered the swiper and so never took ownership of the
        // DotNetObjectReference, and the JS dispose then silently no-ops for an unknown id - so the failure
        // has to surface for BitSwiper to release the reference itself instead of leaking it.
        return jsRuntime.InvokeVoid("BitBlazorUI.Swiper.setup", id, root, container, dotnetObj, options);
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BitSwiperOptions))]
    internal static ValueTask BitSwiperUpdate(this IJSRuntime jsRuntime, string id, BitSwiperOptions options)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.update", id, options);
    }

    internal static ValueTask BitSwiperRefresh(this IJSRuntime jsRuntime, string id)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.refresh", id);
    }

    internal static ValueTask BitSwiperGo(this IJSRuntime jsRuntime, string id, bool forward, int count)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.go", id, forward, count);
    }

    internal static ValueTask BitSwiperGoToItem(this IJSRuntime jsRuntime, string id, int index)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.goToItem", id, index);
    }

    internal static ValueTask BitSwiperGoToPage(this IJSRuntime jsRuntime, string id, int page)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.goToPage", id, page);
    }

    internal static ValueTask BitSwiperGoToEdge(this IJSRuntime jsRuntime, string id, bool end)
    {
        return jsRuntime.FastInvokeVoid("BitBlazorUI.Swiper.goToEdge", id, end);
    }

    // Deliberately not on the FastInvoke path: FastInvokeVoid swallows JSException on the in-process (WASM)
    // runtime, which would hide a failed JS dispose. BitSwiper.DisposeAsync relies on that exception
    // surfacing to know the JS side never took ownership of the DotNetObjectReference and must release it
    // itself, so the dispose call has to use the regular async path where the failure propagates.
    internal static ValueTask BitSwiperDispose(this IJSRuntime jsRuntime, string id)
    {
        return jsRuntime.InvokeVoid("BitBlazorUI.Swiper.dispose", id);
    }
}
