#if CPP
using System;
using System.Runtime.InteropServices;

namespace UniverseLib.Runtime.Il2Cpp;

// Unity 6 struct for arrays returned from iCalls
[StructLayout(LayoutKind.Sequential)]
internal struct BlittableArrayWrapper
{
    public unsafe void* data;
    public int size;
    public int updateFlags;

    internal delegate void d_FreeNativeOwnedMemory(IntPtr ptr);

    // Copies the array out the same way Unity's BlittableArrayWrapper.Unmarshal does
    internal unsafe byte[] ToByteArray()
    {
        switch (updateFlags)
        {
            case 2: // DataIsNativePointer
                return Copy((IntPtr)data);
            case 3: // DataIsNativeOwnedMemory, data points to a NativeOwnedMemory whose first field is the data pointer
                byte[] bytes = Copy(*(IntPtr*)data);
                ICallManager.GetICall<d_FreeNativeOwnedMemory>("UnityEngine.Bindings.BindingsAllocator::FreeNativeOwnedMemory")
                    .Invoke((IntPtr)data);
                return bytes;
            case 4: // DataIsEmpty
                return new byte[0];
            default:
                return null;
        }
    }

    private byte[] Copy(IntPtr ptr)
    {
        byte[] bytes = new byte[size];
        Marshal.Copy(ptr, bytes, 0, size);
        return bytes;
    }
}
#endif
