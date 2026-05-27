# VirtualCameraPublisher

This repo contains the VirtualCameraPublisher project. This includes a Unity
Package for VirtualCameraPublisher which is included in the
[com.meta.xr.virtualcamerapublisher](./com.meta.xr.virtualcamerapublisher)
directory, and a sample Unity app which uses this package, in the
[VirtualCameraPublisherSample](./VirtualCameraPublisherSample) directory. Please
find more details about those in their respective READMEs.

## Installation

To install the Virtual Camera Publisher package into your Unity project, you can add it
locally, or via git:

### Installing with Git

Use the Package Manager to
[add the Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html):

```txt
https://github.com/oculus-samples/Unity-VirtualCameraPublisher.git?path=com.meta.xr.virtualcamerapublisher
```

### Installing locally

Follow the instructions here:
[Installing a package from a local folder](https://docs.unity3d.com/2020.1/Documentation/Manual/upm-ui-local.html)

You can find full integration instructions in
[com.meta.xr.virtualcamerapublisher/README.md](./com.meta.xr.virtualcamerapublisher/README.md).

## License

This project is released under the Oculus SDK License. Please find details in
the [LICENSE](./LICENSE) file.

## Agent-ready development

This sample is configured for AI coding agents.

For the best experience in VS Code or Cursor, install the Meta Horizon extension:

https://marketplace.visualstudio.com/items?itemName=meta.meta-vr-dev

This repository also includes agent instructions and MCP configuration for Meta Quest Agentic Tools:

https://github.com/meta-quest/agentic-tools

Suggested first prompt:

```text
Read AGENTS.md, detect what type of Meta Quest sample this is, enable the hzdb MCP server if available, and explain how to build, run, and debug this sample on a Quest device.
```
