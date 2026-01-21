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

using Meta.VirtualCameraSystem;
using Meta.XR.Samples;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[MetaCodeSample("VirtualCameraPublisher-VirtualCameraPublisherSample")]
public class UnityVideoCapture : MonoBehaviour, IVirtualCameraObserver
{
    private int cameraId = 1;

    [SerializeField]
    private Camera droneCamera;

    [SerializeField]
    private Camera closeCamera;

    void IVirtualCameraObserver.OnCaptureStarted(int id)
    {
        Debug.Log($"VirtualCameraPublisher Sample: Capture started for camera {id}");
    }

    void IVirtualCameraObserver.OnCaptureStopped(int id)
    {
        Debug.Log($"VirtualCameraPublisher Sample: Capture stopped for camera {id}");
    }

    void Start()
    {
        Texture2D far = Resources.Load<Texture2D>("thumbnails/far");
        Texture2D close = Resources.Load<Texture2D>("thumbnails/close");
        VirtualCameraManager.RegisterVirtualCamera("1440p Far @ 45fps", cameraId, 2560, 1440, 45, droneCamera, far);
        VirtualCameraManager.RegisterVirtualCamera("4k Far @ 60fps", cameraId + 1, 3840, 2160, 60, droneCamera, far);
        VirtualCameraManager.RegisterVirtualCamera("1080p Close @ 30fps", cameraId + 2, 1920, 1080, 30, closeCamera, close);
        VirtualCameraManager.AddObserver(this);
    }

    void Update()
    {
        VirtualCameraManager.Update();
    }

    void OnApplicationQuit()
    {
        VirtualCameraManager.RemoveObserver(this);
        VirtualCameraManager.UnregisterVirtualCamera(cameraId);
        VirtualCameraManager.UnregisterVirtualCamera(cameraId + 1);
        VirtualCameraManager.UnregisterVirtualCamera(cameraId + 2);
    }
}
