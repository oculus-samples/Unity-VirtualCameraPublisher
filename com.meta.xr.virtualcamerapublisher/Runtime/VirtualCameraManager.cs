/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#if UNITY_ANDROID && !UNITY_EDITOR
#define IS_SUPPORTED_PLATFORM
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Meta.VirtualCameraSystem
{
    // Mirror of MultiviewRenderingEvent in
    // VirtualCameraManager.cpp
    enum NativeRenderingEvent : int
    {
        Encode = 0,
    }

    public static class VirtualCameraManager
    {
        private static Dictionary<int, VirtualCamera> _cameras = new Dictionary<int, VirtualCamera>();

        private static bool _initialized = false;

        private static List<IVirtualCameraObserver> _observers = new List<IVirtualCameraObserver>();

        private const string _tag = "VirtualCameraManager";

        public static void AddObserver(IVirtualCameraObserver observer)
        {
            if (observer == null)
            {
                Debug.LogWarning($"{_tag}: Cannot add null observer");
                return;
            }
            _observers.Add(observer);
        }

        public static void RemoveObserver(IVirtualCameraObserver observer)
        {
            if (_observers.Count == 0 || observer == null)
            {
                Debug.LogWarning($"{_tag}: Cannot remove null observer");
                return;
            }
            _observers.Remove(observer);
        }

        public static bool RegisterVirtualCamera(string name, int id, int width, int height, int fps, Camera camera, Texture2D thumbnail = null)
        {
            if (!ValidateRegisterParameters(name, id, width, height, fps, camera))
            {
                return false;
            }

            camera.enabled = false;

            if (!InitializeSystem())
            {
                DLog("Failed to register virtual camera, system initialization failed");
                return false;
            }

            DLog($"RegisterVirtualCamera called for name={name} id={id} width={width} height={height} fps={fps}");

            VirtualCamera vc = new VirtualCamera(name, width, height, fps, camera, thumbnail);
            if (!RegisterCameraInternal(id, vc))
            {
                DLog("Failed to register virtual camera");
                return false;
            }

            return true;
        }

        public static void UnregisterVirtualCamera(int id)
        {
            DLog($"UnregisterVirtualCamera called for id={id}");
            if (!_cameras.TryGetValue(id, out VirtualCamera vc))
            {
                DLog($"Camera with id={id} not found, can't unregister!");
                return;
            }
            string thumbnailContentUri = vc.ThumbnailContentUri;

            UnregisterCamera(id);

            vc.Dispose();
            _cameras.Remove(id);

            // Do this after we have unregistered the camera in case any consumers are still using it
            VirtualCamera.DeleteThumbnail(thumbnailContentUri);
        }

        public static void UnregisterAllCameras()
        {
            var ids = new List<int>(_cameras.Keys);
            foreach (var id in ids)
            {
                UnregisterVirtualCamera(id);
            }
        }

        public static bool IsCapturing(int id)
        {
            if (!_cameras.ContainsKey(id))
            {
                return false;
            }

            return NativeIsCapturing(id);
        }

        public static void Update()
        {
            if (!_initialized)
            {
                return;
            }

            foreach (var id in _cameras.Keys)
            {
                UpdateCamera(id);
            }
        }

        private static void NotifyCaptureStarted(int id)
        {
            foreach (var observer in _observers)
            {
                observer.OnCaptureStarted(id);
            }
        }

        private static void NotifyCaptureStopped(int id)
        {
            foreach (var observer in _observers)
            {
                observer.OnCaptureStopped(id);
            }
        }

        private static void UpdateCamera(int id)
        {
            if (!_cameras.TryGetValue(id, out VirtualCamera vc))
            {
                return;
            }
            if (vc.Camera == null)
            {
                DLog("Camera not initialized");
                return;
            }

            if (!IsCapturing(id))
            {
                if (vc.LastIsCapturing)
                {
                    NotifyCaptureStopped(id);
                    vc.LastIsCapturing = false;
                }
                return;
            }

            if (!vc.LastIsCapturing)
            {
                NotifyCaptureStarted(id);
                vc.LastIsCapturing = true;
                // Give game 1 frame to start camera logic if needed, before rendering it
                return;
            }

            int drawTextureIndex = vc.FrameIndex % 2;
            int captureTextureIndex = 1 - drawTextureIndex;

            if (vc.HasFrameToBlit)
            {
                RenderTexture captureTexture = vc.Textures[captureTextureIndex];
                SetNextTextureToEncode(captureTexture.GetNativeTexturePtr());
                // Encode
                GL.IssuePluginEvent(GetRenderEventFunc(), (int)NativeRenderingEvent.Encode);
                vc.HasFrameToBlit = false;
            }

            if ((Now() - vc.LastRenderTime) > vc.MsPerFrame)
            {
                vc.Camera.targetTexture = vc.Textures[drawTextureIndex];
                vc.Camera.Render();
                vc.LastRenderTime = Now();
                vc.HasFrameToBlit = true;
            }
            else
            {
                vc.Camera.targetTexture = null;
            }

            vc.FrameIndex++;
        }

        private static bool InitializeSystem()
        {
            if (_initialized)
            {
                return true;
            }

#if !IS_SUPPORTED_PLATFORM
            Debug.LogWarning($"{_tag}: VirtualCameraManager is not supported in the Unity Editor");
            return false;
#else
            GraphicsDeviceType currentAPI = SystemInfo.graphicsDeviceType;
            DLog($"Initializing with graphicsApi={currentAPI}");
            SetGraphicsApiType((int)currentAPI);

            _initialized = true;
            return true;
#endif
        }

        private static bool RegisterCameraInternal(int id, VirtualCamera vc)
        {
            _cameras.Add(id, vc);
            return RegisterCamera(id, vc.Width, vc.Height, vc.Fps, vc.Name, vc.ThumbnailContentUri);
        }

        private static long Now()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static void DLog(string toLog)
        {
            Debug.Log($"{_tag}: {toLog}");
        }

        private static bool ValidateRegisterParameters(string name, int id, int width, int height, int fps, Camera camera)
        {
            if (camera == null)
            {
                Debug.LogError($"{_tag}: Camera cannot be null");
                return false;
            }
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"{_tag}: Camera name cannot be null or empty");
                return false;
            }
            if (width <= 0 || height <= 0)
            {
                Debug.LogError($"{_tag}: Invalid resolution {width}x{height}. Width and height must be greater than zero.");
                return false;
            }
            if (fps < 1 || fps > 60)
            {
                Debug.LogError($"{_tag}: Invalid FPS {fps}. FPS must be between 1 and 60.");
                return false;
            }
            if (_cameras.ContainsKey(id))
            {
                Debug.LogError($"{_tag}: Camera with id={id} is already registered");
                return false;
            }
            return true;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeEditorCallbacks()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                // Reset static fields to support disabled domain reloading
                ResetStaticFields();
            }
        }

        private static void ResetStaticFields()
        {
            // Dispose all cameras before clearing
            foreach (var kvp in _cameras)
            {
                kvp.Value.Dispose();
            }
            _cameras = new Dictionary<int, VirtualCamera>();
            _observers = new List<IVirtualCameraObserver>();
            _initialized = false;
        }
#endif

        #region Native Methods
#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem")]
        private static extern void SetNextTextureToEncode(IntPtr ptr);
#else
        private static void SetNextTextureToEncode(IntPtr ptr)
        {
        }
#endif

#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem", EntryPoint = "NativeRegisterCamera")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool RegisterCamera(int id, int width, int height, int fps, string name, string thumbnailContentUri);
#else
        private static bool RegisterCamera(int id, int width, int height, int fps, string name, string thumbnailContentUri)
        {
            return false;
        }
#endif

#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem", EntryPoint = "NativeUnregisterCamera")]
        private static extern void UnregisterCamera(int id);
#else
        private static void UnregisterCamera(int id)
        {
        }
#endif

#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem")]
        private static extern IntPtr GetRenderEventFunc();
#else
        private static IntPtr GetRenderEventFunc()
        {
            return IntPtr.Zero;
        }
#endif

#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem")]
        private static extern void SetGraphicsApiType(int type);
#else
        private static void SetGraphicsApiType(int type)
        {
        }
#endif

#if IS_SUPPORTED_PLATFORM
        [DllImport("VirtualCameraSystem")]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool NativeIsCapturing(int id);
#else
        private static bool NativeIsCapturing(int id)
        {
            return false;
        }
#endif
        #endregion
    }
} // namespace Meta.VirtualCameraSystem
