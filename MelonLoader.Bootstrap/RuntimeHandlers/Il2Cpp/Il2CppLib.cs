using MelonLoader.Bootstrap.Logging;
using MelonLoader.Bootstrap.Utils;
using System.Runtime.InteropServices;

namespace MelonLoader.Bootstrap.RuntimeHandlers.Il2Cpp;

internal class Il2CppLib(Il2CppLib.MethodGetNameFn methodGetName)
{
    private const string libName = // Gotta specify the file extension in lower-case, otherwise Il2CppInterop brainfarts itself
#if WINDOWS
        "GameAssembly.dll";
#elif LINUX
        "GameAssembly.so";
#endif

    public required nint Handle { get; init; }

    public required nint InitPtr { get; init; }
    public required nint RuntimeInvokePtr { get; init; }

    public static Il2CppLib? TryLoad()
    {
        if (!NativeLibrary.TryLoad(libName, out var hRuntime))
        {
            MelonLogger.LogError($"Load {libName} failed.");
            return null;
        }

        MelonLogger.LogWarning($"Successfully Loaded {libName} - Address: 0x{hRuntime.ToInt64():X}");

        // il2cpp_init
        if (!NativeLibrary.TryGetExport(hRuntime, "il2cpp_init", out var initPtr))
        {
            MelonLogger.LogError($"[il2cpp_init] Failed to locate export using original name.");
            MelonLogger.LogWarning($"[il2cpp_init] Attempting fallback using NameTranslations mapping...");

            if (!NativeLibrary.TryGetExport(hRuntime, NameTranslations.NameMappings["il2cpp_init"], out initPtr))
            {
                MelonLogger.LogError($"[il2cpp_init] Failed to locate export using NameTranslations fallback: {NameTranslations.NameMappings["il2cpp_init"]}");
                return null;
            } else
            {
                MelonLogger.LogWarning($"[il2cpp_init] Successfully resolved using NameTranslations.");
                MelonLogger.LogWarning($"[il2cpp_init] Resolved name: {NameTranslations.NameMappings["il2cpp_init"]}");
                MelonLogger.LogWarning($"[il2cpp_init] Export address: 0x{initPtr.ToInt64():X}");
            }
        } else
        {
            MelonLogger.LogWarning($"[il2cpp_init] Successfully located export at: 0x{initPtr.ToInt64():X}");
        }

        // il2cpp_runtime_invoke
        if (!NativeLibrary.TryGetExport(hRuntime, "il2cpp_runtime_invoke", out var runtimeInvokePtr))
        {
            MelonLogger.LogError($"[il2cpp_runtime_invoke] Failed to locate export using original name.");
            MelonLogger.LogWarning($"[il2cpp_runtime_invoke] Attempting fallback using NameTranslations mapping...");

            if (!NativeLibrary.TryGetExport(hRuntime, NameTranslations.NameMappings["il2cpp_runtime_invoke"], out runtimeInvokePtr))
            {
                MelonLogger.LogError($"[il2cpp_runtime_invoke] Failed to locate export using NameTranslations fallback: {NameTranslations.NameMappings["il2cpp_runtime_invoke"]}");
                return null;
            } else
            {
                MelonLogger.LogWarning($"[il2cpp_runtime_invoke] Successfully resolved using NameTranslations.");
                MelonLogger.LogWarning($"[il2cpp_runtime_invoke] Resolved name: {NameTranslations.NameMappings["il2cpp_runtime_invoke"]}");
                MelonLogger.LogWarning($"[il2cpp_runtime_invoke] Export address: 0x{runtimeInvokePtr.ToInt64():X}");
            }
        } else
        {
            MelonLogger.LogWarning($"[il2cpp_runtime_invoke] Successfully located export at: 0x{runtimeInvokePtr.ToInt64():X}");
        }

        // il2cpp_method_get_name
        if (!NativeFunc.GetExport<MethodGetNameFn>(hRuntime, "il2cpp_method_get_name", out var methodGetName))
        {
            MelonLogger.LogError($"[il2cpp_method_get_name] Failed to locate export using original name.");
            MelonLogger.LogWarning($"[il2cpp_method_get_name] Attempting fallback using NameTranslations mapping...");

            if (!NativeFunc.GetExport<MethodGetNameFn>(hRuntime, NameTranslations.NameMappings["il2cpp_method_get_name"], out methodGetName))
            {
                MelonLogger.LogError($"[il2cpp_method_get_name] Failed to locate export using NameTranslations fallback: {NameTranslations.NameMappings["il2cpp_method_get_name"]}");
                return null;
            } else
            {
                MelonLogger.LogWarning($"[il2cpp_method_get_name] Successfully resolved using NameTranslations.");
                MelonLogger.LogWarning($"[il2cpp_method_get_name] Resolved name: {NameTranslations.NameMappings["il2cpp_method_get_name"]}");
                MelonLogger.LogWarning($"[il2cpp_method_get_name] Delegate created at function pointer.");
            }
        } else
        {
            MelonLogger.LogWarning($"[il2cpp_method_get_name] Successfully located export and created delegate.");
        }

        return new(methodGetName)
        {
            Handle = hRuntime,
            InitPtr = initPtr,
            RuntimeInvokePtr = runtimeInvokePtr
        };
    }

    public string? GetMethodName(nint method)
    {
        return method == 0 ? null : Marshal.PtrToStringAnsi(methodGetName(method));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint InitFn(nint a);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint RuntimeInvokeFn(nint method, nint obj, nint args, nint exc);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate nint MethodGetNameFn(nint method);
}
