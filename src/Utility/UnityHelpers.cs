using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UniverseLib.Utility
{
    public static class UnityHelpers
    {
        // Time helpers, can't use Time.time since timeScale will affect it.

        /// <summary>
        /// Returns true if the provided <paramref name="time"/> occured more than 10ms before <see cref="Time.realtimeSinceStartup"/>.
        /// </summary>
        /// <param name="time">Should be a value from <see cref="Time.realtimeSinceStartup"/> which you stored earlier.</param>
        public static bool OccuredEarlierThanDefault(this float time)
        {
            return Time.realtimeSinceStartup - 0.01f >= time;
        }

        /// <summary>
        /// Returns true if the provided <paramref name="time"/> occured at least <paramref name="secondsAgo"/> before <see cref="Time.realtimeSinceStartup"/>.
        /// </summary>
        /// <param name="time">Should be a value from <see cref="Time.realtimeSinceStartup"/> which you stored earlier.</param>
        public static bool OccuredEarlierThan(this float time, float secondsAgo)
        {
            return Time.realtimeSinceStartup - secondsAgo >= time;
        }

        /// <summary>
        /// Check if an object is null, and if it's a UnityEngine.Object then also check if it was destroyed.
        /// </summary>
        public static bool IsNullOrDestroyed(this object obj, bool suppressWarning = true)
        {
            try
            {
                if (obj == null)
                {
                    if (!suppressWarning)
                        Universe.LogWarning("The target instance is null!");

                    return true;
                }
                else if (obj is Object unityObj && !unityObj)
                {
                    if (!suppressWarning)
                        Universe.LogWarning("The target UnityEngine.Object was destroyed!");

                    return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Get the full Transform heirarchy path for this provided Transform.
        /// </summary>
        public static string GetTransformPath(this Transform transform, bool includeSelf = false)
        {
            StringBuilder sb = new();
            if (includeSelf)
                sb.Append(transform.name);

            while (transform.parent)
            {
                transform = transform.parent;
                sb.Insert(0, '/');
                sb.Insert(0, transform.name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts Color to 6-digit RGB hex code (without # symbol). Eg, RGBA(1,0,0,1) -> FF0000
        /// </summary>
        public static string ToHex(this Color color)
        {
            byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
            byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);

            return $"{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Assumes the string is a 6-digit RGB Hex color code (with optional leading #) which it will parse into a UnityEngine.Color.
        /// Eg, FF0000 -> RGBA(1,0,0,1)
        /// </summary>
        public static Color ToColor(this string _string)
        {
            _string = _string.Replace("#", "");

            if (_string.Length != 6)
                return Color.magenta;

            byte r = byte.Parse(_string.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(_string.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(_string.Substring(4, 2), NumberStyles.HexNumber);

            Color color = new()
            {
                r = (float)(r / (decimal)255),
                g = (float)(g / (decimal)255),
                b = (float)(b / (decimal)255),
                a = 1
            };

            return color;
        }

        private static PropertyInfo onEndEdit;

        /// <summary>
        /// Returns the onEndEdit event as a <see cref="UnityEvent{T0}"/> for greater compatibility with all Unity versions.
        /// </summary>
        public static UnityEvent<string> GetOnEndEdit(this InputField _this)
        {
            if (onEndEdit == null)
                onEndEdit = AccessTools.Property(typeof(InputField), "onEndEdit")
                            ?? throw new Exception("Could not get InputField.onEndEdit property!");

            return onEndEdit.GetValue(_this, null).TryCast<UnityEvent<string>>();
        }
        // Scene.m_Handle is an int before Unity 6.3, after that it's a SceneHandle holding an EntityId (m_Value.m_Data)
        private static readonly FieldInfo sceneHandleField = AccessTools.Field(typeof(Scene), "m_Handle");
        private static readonly FieldInfo sceneHandleValueField = sceneHandleField.FieldType == typeof(int) ? null : AccessTools.Field(sceneHandleField.FieldType, "m_Value");
        private static readonly FieldInfo entityIdDataField = sceneHandleValueField == null ? null : AccessTools.Field(sceneHandleValueField.FieldType, "m_Data");

        internal static bool SceneHandleIsStruct => sceneHandleValueField != null;

        /// <summary>
        /// Returns the handle of this Scene as an int on every Unity version (<see cref="Scene.handle"/> returns a SceneHandle from Unity 6.3).
        /// </summary>
        public static int GetHandle(this Scene scene)
        {
            object handle = sceneHandleField.GetValue(scene);
            return sceneHandleValueField == null ? (int)handle : (int)entityIdDataField.GetValue(sceneHandleValueField.GetValue(handle));
        }

        /// <summary>
        /// Converts an int scene handle into the type of Scene.m_Handle on this Unity version (int, or SceneHandle from Unity 6.3).
        /// </summary>
        public static object ToSceneHandle(int handle)
        {
            if (sceneHandleValueField == null)
                return handle;

            object entityId = Activator.CreateInstance(sceneHandleValueField.FieldType);
            entityIdDataField.SetValue(entityId, handle);
            object sceneHandle = Activator.CreateInstance(sceneHandleField.FieldType);
            sceneHandleValueField.SetValue(sceneHandle, entityId);
            return sceneHandle;
        }
    }
}
