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

using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Meta.VirtualCameraSystem
{
    internal class VirtualCamera : IDisposable
    {
        public string Name;
        public int FrameIndex = 0;
        public int Width;
        public int Height;
        public int Fps;
        public long MsPerFrame;
        public string ThumbnailContentUri;
        public long LastRenderTime = 0;
        public bool HasFrameToBlit = false;
        public bool LastIsCapturing = false;
        public Camera Camera = null;
        public RenderTexture[] Textures = new RenderTexture[2];

        public VirtualCamera(string name, int width, int height, int fps, Camera camera, Texture2D thumbnail = null)
        {
            Name = name;
            Width = width;
            Height = height;
            Camera = camera;
            Fps = fps;
            MsPerFrame = 1000 / Fps;
            for (int i = 0; i < 2; i++)
            {
                Textures[i] = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
                Textures[i].enableRandomWrite = true;
                Textures[i].depthStencilFormat = GraphicsFormat.D16_UNorm;
                Textures[i].Create();
            }
            TrySaveThumbnail(thumbnail);
        }

        public void Dispose()
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                if (Textures[i] != null)
                {
                    Textures[i].Release();
                    Textures[i] = null;
                }
            }
            Camera = null;
        }

        private void TrySaveThumbnail(Texture2D thumbnail)
        {
            if (thumbnail == null)
            {
                return;
            }

            try
            {
                byte[] thumbnailBytes = thumbnail.EncodeToJPG();

                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaClass thumbnailProvider = new AndroidJavaClass("com.meta.virtualcamerasystem.ThumbnailProvider"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject appContext = activity.Call<AndroidJavaObject>("getApplicationContext"))
                {
                    ThumbnailContentUri = thumbnailProvider.CallStatic<string>("save", appContext, thumbnailBytes, "jpg");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"VirtualCameraManager: Failed to save thumbnail: {e.Message}. Camera will use app icon.");
            }
        }

        public static void DeleteThumbnail(string ThumbnailContentUri)
        {
            if (ThumbnailContentUri == null)
            {
                return;
            }
            using (AndroidJavaClass thumbnailProvider = new AndroidJavaClass("com.meta.virtualcamerasystem.ThumbnailProvider"))
            {
                thumbnailProvider.CallStatic("delete", ThumbnailContentUri);
            }
        }
    }
} // namespace Meta.VirtualCameraSystem
