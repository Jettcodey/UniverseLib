#if CPP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UniverseLib.Config;

#if INTEROP
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#else
using UnhollowerBaseLib;
#endif

namespace UniverseLib.Runtime.Il2Cpp
{
    internal class Il2CppTextureHelper : TextureHelper
    {
        internal delegate IntPtr d_EncodeToPNG(IntPtr tex);

        internal delegate void d_EncodeToPNG_Injected(IntPtr tex, out BlittableArrayWrapper ret);

        internal delegate void d_Blit2(IntPtr source, IntPtr dest);

        internal delegate IntPtr d_CreateSprite(IntPtr texture, ref Rect rect, ref Vector2 pivot, float pixelsPerUnit,
            uint extrude, int meshType, ref Vector4 border, bool generateFallbackPhysicsShape, IntPtr secondaryTexture);

        internal delegate void d_CopyTexture_Region(IntPtr src, int srcElement, int srcMip, int srcX, int srcY,
            int srcWidth, int srcHeight, IntPtr dst, int dstElement, int dstMip, int dstX, int dstY);

        // Unity 6 iCalls are _Injected and take m_CachedPtr, asked for by name since MelonLoader shims the old ones
        private static readonly bool nativeObjectICalls = IL2CPP.il2cpp_resolve_icall("UnityEngine.Graphics::Blit2_Injected") != IntPtr.Zero;

        protected internal override Texture2D Internal_NewTexture2D(int width, int height)
        {
            return new(width, height, TextureFormat.RGBA32, false);
        }

        protected internal override Texture2D Internal_NewTexture2D(int width, int height, TextureFormat textureFormat, bool mipChain)
        {
            return new(width, height, textureFormat, mipChain);
        }

        protected internal override void Internal_Blit(Texture tex, RenderTexture rt)
        {
            if (ConfigManager.Bypass_UniverseLib_ICall)
            {
                Graphics.Blit(tex, rt);
            }
            else
            {
                ICallManager.GetICall<d_Blit2>(nativeObjectICalls ? "UnityEngine.Graphics::Blit2_Injected" : "UnityEngine.Graphics::Blit2")
                    .Invoke(nativeObjectICalls ? tex.m_CachedPtr : tex.ToIl2CppPointer(), nativeObjectICalls ? rt.m_CachedPtr : rt.ToIl2CppPointer());
            }
        }

        protected internal override byte[] Internal_EncodeToPNG(Texture2D tex)
        {
            if (nativeObjectICalls)
            {
                ICallManager.GetICall<d_EncodeToPNG_Injected>("UnityEngine.ImageConversion::EncodeToPNG_Injected")
                    .Invoke(tex.m_CachedPtr, out BlittableArrayWrapper ret);
                return ret.ToByteArray();
            }

            IntPtr arrayPtr = ICallManager.GetICall<d_EncodeToPNG>("UnityEngine.ImageConversion::EncodeToPNG")
                .Invoke(tex.ToIl2CppPointer());

            return arrayPtr == IntPtr.Zero ? null : new Il2CppStructArray<byte>(arrayPtr);
        }

        protected internal override Sprite Internal_CreateSprite(Texture2D texture)
        {
            var rect = new Rect(0, 0, texture.width, texture.height);
            return
                ConfigManager.Bypass_UniverseLib_ICall ?
                Sprite.Create(texture, rect, Vector2.zero, 100f, 0u, SpriteMeshType.Tight, Vector4.zero) :
                CreateSpriteImpl(texture, rect, Vector2.zero, 100f, 0u, Vector4.zero);
        }

        protected internal override Sprite Internal_CreateSprite(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, Vector4 border)
        {
            return
                ConfigManager.Bypass_UniverseLib_ICall ?
                Sprite.Create(texture, rect, pivot, pixelsPerUnit, extrude, SpriteMeshType.Tight, border) :
                CreateSpriteImpl(texture, rect, pivot, pixelsPerUnit, extrude, border);
        }

        internal static Sprite CreateSpriteImpl(Texture texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, Vector4 border)
        {
            // Unity 2022 added the secondaryTexture array, older versions ignore the extra argument
            IntPtr spritePtr = ICallManager.GetICall<d_CreateSprite>("UnityEngine.Sprite::CreateSprite_Injected")
                .Invoke(nativeObjectICalls ? texture.m_CachedPtr : texture.ToIl2CppPointer(), ref rect, ref pivot, pixelsPerUnit, extrude, 1, ref border, false, IntPtr.Zero);

            // Unity 6 returns a GC handle to the Sprite
            if (nativeObjectICalls && spritePtr != IntPtr.Zero)
                spritePtr = spritePtr.GetTargetPtr();

            return spritePtr == IntPtr.Zero ? null : new Sprite(spritePtr);
        }

        internal override bool Internal_CanForceReadCubemaps => true;

        internal override Texture Internal_CopyTexture(Texture src, int srcElement, int srcMip, int srcX, int srcY,
            int srcWidth, int srcHeight, Texture dst, int dstElement, int dstMip, int dstX, int dstY)
        {
            if (ConfigManager.Bypass_UniverseLib_ICall)
            {
                Graphics.CopyTexture(
                    src, srcElement, srcMip, srcX, srcY, srcWidth, srcHeight,
                    dst, dstElement, dstMip, dstX, dstY);
            }
            else
            {
                ICallManager.GetICall<d_CopyTexture_Region>(nativeObjectICalls ? "UnityEngine.Graphics::CopyTexture_Region_Injected" : "UnityEngine.Graphics::CopyTexture_Region")
                    .Invoke(nativeObjectICalls ? src.m_CachedPtr : src.ToIl2CppPointer(), srcElement, srcMip, srcX, srcY, srcWidth, srcHeight,
                        nativeObjectICalls ? dst.m_CachedPtr : dst.ToIl2CppPointer(), dstElement, dstMip, dstX, dstY);
            }
            return dst;
        }
    }
}
#endif