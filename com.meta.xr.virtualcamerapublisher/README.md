# **Virtual Camera Publisher SDK Onboarding Documentation**

DISCLAIMER: This feature is provided on an “as is” basis, subject to all
applicable terms set forth in the
[Meta Platform Technologies SDK License Agreement](https://developers.meta.com/horizon/licenses/).

## **What is Virtual Camera Publisher?**

Virtual Camera Publisher is a new feature which allows apps to publish custom
virtual cameras to the HzOS system. These cameras are then available for use
with the system recording, casting, and livestreaming features, and are
accessible to 3p apps via the Camera2 API\*\*.

New, engaging camera angles have been a top requested feature from media
creators for some time now, and with Virtual Camera Publisher it is now easy for
developers to make these available. **We estimate that you can get a prototype
working within a day\!**

\*\* _To access these cameras via the Camera2 API, the additional
horizonos.permission.APP_DEFINED_CAMERA permission is required. We will share
further documentation on how to consume these virtual cameras in the future._

## **Requirements and limitations with Virtual Camera Publisher**

1. Headset
   1. Requires HzOS v81+.
2. Your app
   1. Must be a Unity app (further platforms may be coming in the future
      depending on demand).
   2. Must use Android API Level 32+.
   3. You should have at least one virtual camera in your project.
3. Limitations
   1. Up to 3 cameras can be registered by a given app, at a single time.
   2. Max supported framerate is 60fps.

## **How to integrate with Virtual Camera Publisher**

### Installation

To install the Virtual Camera Publisher package into your Unity project, you can add it
locally, or via git:

#### Installing with Git

Use the Package Manager to
[add the Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html):

```txt
https://github.com/oculus-samples/Unity-VirtualCameraPublisher.git?path=com.meta.xr.virtualcamerapublisher
```

#### Installing locally

Follow the instructions here:
[Installing a package from a local folder](https://docs.unity3d.com/2020.1/Documentation/Manual/upm-ui-local.html)

### **Update AndroidManifest**

Add these lines to your AndroidManifest.xml file. Note: if you do not have one
already, you can follow the instructions
[here](https://developers.meta.com/horizon/documentation/unity/unity-android-manifest/)
to create one (requires Meta XR Core SDK to be
[installed](https://developers.meta.com/horizon/documentation/unity/unity-project-setup#import-the-meta-xr-core-sdk)).

Then add these lines to your AndroidManifest.xml file

- `<xmlns:horizonos="http://schemas.horizonos/sdk">`
  1. This is part of the top level `manifest` element, i.e.
     `<manifest ... xmlns:horizonos=...`

- `<uses-permission android:name="horizonos.permission.CREATE_VIRTUAL_CAMERA" />`
  1. This is a normal level permission which does not require user acceptance.

- `<horizonos:uses-horizonos-sdk horizonos:minSdkVersion="81" horizonos:targetSdkVersion="81" />`

Complete example:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest
   xmlns:android="http://schemas.android.com/apk/res/android"
   xmlns:tools="http://schemas.android.com/tools"
   xmlns:horizonos="http://schemas.horizonos/sdk">

   <application>
       <!-- ... -->
   </application>

   <uses-permission
      android:name="horizonos.permission.CREATE_VIRTUAL_CAMERA" />

   <horizonos:uses-horizonos-sdk
      horizonos:minSdkVersion="81"
      horizonos:targetSdkVersion="81" />

</manifest>
```

### **Integrate VirtualCameraManager into your code**

This section describes the steps necessary to integrate the virtual camera into
your system. An example Unity project is included in the top level
VirtualCameraPublisherSample folder of this project. See
[../VirtualCameraPublisherSample/README.md](../VirtualCameraPublisherSample/README.md)
for more info.

An easy way to get started with this feature is to:

1. Drag the
   [UnityVideoCapture.cs](../VirtualCameraPublisherSample/Assets/VirtualCameraPublisherSample/Scripts/UnityVideoCapture.cs)
   script from the Virtual Camera Publisher Sample into your project. It shows
   an E2E integration of VirtualCameraManager and will provide a working
   integration example.

2. Add the
   [UnityVideoCapture.cs](../VirtualCameraPublisherSample/Assets/VirtualCameraPublisherSample/Scripts/UnityVideoCapture.cs)
   script as component where it makes sense in your project (e.g. under the
   Camera Rig)

3. Add a virtual camera (Unity-\>Game Object-\>Camera) to your project and
   position it to follow what you want. Unity provides camera management with
   Cinemachine for more complex use cases, but even a simple camera will work.

4. Drag the virtual camera that you created into the Drone Camera field of the
   UnityVideoCapture component.
   1. Drag a second virtual camera to the Close Camera field in the script if
      desired, otherwise comment out / delete any code related to Close Camera.

5. Follow the steps below to complete the set-up, like including the thumbnail
   configuration.

For integrating directly into your code, please follow the following steps: The
first three steps are required while steps 4 and 5 are optional.

##### _Step 1: call VirtualCameraManager.RegisterVirtualCamera_

This will make your virtual camera visible to other apps on the system. You must
pass in a Unity Camera at registration time, however this camera will not render
until another app requests this camera feed (`Camera.enabled` will be set to
false by VirtualCameraManager).

**Parameters:**

- **`String name`**
  - Required: this is the name which will be associated with your camera in the
    camera picker UI. It should be clear and concise. You should not include
    your app’s name in the camera name. The app name will be automatically shown
    in the camera picker UI by the system.

- **`int id`**
  - Required: unique numeric id for this camera. It does not matter what it is,
    but it must be unique among the cameras you are registering. Make sure to
    save it as this will be the canonical identifier for this camera going
    forward.

- **`int width`**
  - Required: width (in pixels) that your camera should render at.

- **`int height`**
  - Required: height (in pixels) that your camera should render at.

- **`int fps`**
  - Required: the FPS your camera should be rendered at. Note that
    VirtualCameraManager will take care of rendering your camera when needed.
    Note: max supported framerate is 60fps.
  - Note: it is recommended to use a FPS which evenly divides your game’s render
    framerate, e.g. if your game renders at 90Hz, recommended FPS numbers would
    be 30, or 45.

- **`Camera camera`**
  - Required: the Unity Camera object associated with this virtual camera. It is
    the caller’s responsibility to configure the positioning of this camera
    (dynamically, if desired) and what it sees.

- **`Texture2D thumbnail`**
  - Optional: the thumbnail to be shown in the camera picker UI to give users an
    idea of what this camera will look like. If you do not pass this in, or pass
    in null, your app’s icon will be used as the thumbnail.

**How to include a thumbnail (optional)**

When you call RegisterVirtualCamera, you can provide a Thumbnail to be
associated with your camera as a Texture2D. This will be used in the view picker
to differentiate this camera from other cameras. If you do not wish to pass a
thumbnail, simply pass null and your app’s icon will be used as the thumbnail.

In order for your thumbnail to be accessible to apps consuming your camera, you
must add a provider element in your AndroidManifest.xml. The provider element
must be exactly as follows, and must be placed under the **application**
element:

```
<application>
    <!-- ... -->
    <provider
        android:name="com.meta.virtualcamerasystem.ThumbnailProvider"
        android:authorities="[INSERT your package name].thumbnailprovider"
        android:exported="true"
        android:grantUriPermissions="true">
    </provider>
    <!-- NOTE: your package name above should be of the form
         com.foo.MyAppName -->
    <!-- ... -->
</application>
```

The thumbnail you provide can be static, or dynamic/customized to the user/where
they are at in your game. If you want to use a static thumbnail, the easiest way
to do so is the following:

1. Save the thumbnail image to the Assets/Resources folder in your Unity project

2. In the project editor, make sure to enable read/write for the thumbnails, and
   to disable compression.

3. Load the thumbnail(s) using `Resources.Load<Texture2D>(path)`. Note that path
   should be the relative path to the thumbnail within the Resources directory,
   without the file extension. So if your asset is stored at
   Resources/thumbnails/foo.jpg, the path you’d pass in is `"thumbnails/foo"`.

4. Pass the Texture2D to RegisterVirtualCamera

##### _Step 2: Call VirtualCameraManager.Update_

This method should be called for every frame in your game (e.g. from
`MonoBehavior.Update`). VirtualCameraManager will only render your camera at up
to `fps` (specified in your call to RegisterVirtualCamera) frames per second.

**Parameters:** none

##### _Step 3: Call VirtualCameraManager.UnregisterVirtualCamera_

This should be called when you want your camera to disappear from the system. If
another app is capturing from this camera when you call UnregisterVirtualCamera,
calling UnregisterVirtualCamera will end that capture gracefully.

Note that your camera will automatically be removed from the system (and ongoing
captures will be ended) when your application is quit.

**Parameters:**

- **int id**
  - Required: id of the camera to be unregistered.

##### _Optional Step 4: Register an IVirtualCameraObserver_

An observer will allow you to be notified when one of your cameras starts/stops
being consumed. This is useful if you want to only position the camera while in
use. Note that the camera is not rendered when not in use, and this is handled
by VirtualCameraManager.

```
public interface IVirtualCameraObserver
{
    public void OnCaptureStarted(int id);
    public void OnCaptureStopped(int id);
}

// Then you can use VirtualCameraManager.AddObserver and VirtualCameraManager.RemoveObserver to
// add/remove observers as needed
```

##### _Optional Step 5: Call VirtualCameraManager.IsCapturing_

This method allows you to query whether any app is currently capturing from your
virtual camera – this could mean video recording, casting, or previewing the
camera feed. This field simply indicates whether your camera is being rendered
and shared with another app at a given time.

This information could be useful if you only want to avoid positioning your
camera when not in use. This method can be used instead of making an observer,
if that is easier.

**Parameters:**

- **`int id`**
  - Required: id of the camera to query.

## **How to test your changes**

1. Ensure your headset has HzOS v81+
2. Build/install your app (note that Virtual Camera Publisher does not yet work
   with LINK, and thus a full apk build/install is required for functional
   testing)
3. Open your app and go to the point in it where it registers a virtual camera
4. Press the Meta button to bring up the Universal Menu
5. Open the camera app
6. Open Camera Settings
7. Click “Camera View” from the settings menu (see screenshot below)
8. Choose your camera from the list
9. Start recording or casting\!

## License

This package is released under the Oculus SDK License. Please find details in
the [LICENSE](./LICENSE) file.
