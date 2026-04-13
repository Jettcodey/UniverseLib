#if CPP
using System;
using System.Reflection;

#if INTEROP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
#else
using UnhollowerBaseLib;
#endif

namespace UniverseLib.Utility;


public static class Il2CppPointerExtension
{
    /// <summary>
    /// Returns the Pointer to any given Il2Cpp Object.
    /// </summary>
    internal static IntPtr ToIl2CppPointer<T>(this T obj)
        where T : Il2CppObjectBase
    {
        // Get Pointer from Unhollower/Il2CppInterop instead of .Pointer
        // This ensures greater compatibility with any variation in behavior
        return IL2CPP.Il2CppObjectBaseToPtr(obj);
    }

    // Il2CppInterop changed parameters from uint to nint
    // We call IL2CPP.il2cpp_gchandle_get_target using Reflection to automatically handle value conversion
    private static MethodInfo _gcHandleGetTarget = typeof(IL2CPP).GetMethod(nameof(IL2CPP.il2cpp_gchandle_get_target), BindingFlags.Public | BindingFlags.Static);
    internal static IntPtr GetTargetPtr(this IntPtr gcHandle)
        => (IntPtr)_gcHandleGetTarget.Invoke(null, [gcHandle]);
}

#endif