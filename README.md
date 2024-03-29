# QDNH
- Quansheng Dock Network Host

Allows a radio to be hosted on a network (even over the internet) which Quansheng Dock or QDockX can then connect to.  

This is a core .NET console application.

For more information check out the entry on the QD Wiki here:  https://github.com/nicsure/QuanshengDock/wiki/60-%E2%80%90-Integrating-QD-with-other-programs#network-host-for-qd---qdnh

# QDockX
- A similar application to Quansheng Dock only aimed at portable devices designed specifically to operate with QDNH.

The Android APK for QDockX is available in the latest release section. You will NOT find this on the Play Store. Publishing and maintaining an App on the store is a nightmare. So it's distributed as an APK. In order to install it you have to enable installation of unknown apps on your device.  
Extra information QDockX is here: https://github.com/nicsure/QuanshengDock/wiki/45-%E2%80%90-QDockX-on-Android


# Quickstart Guide
On your Radio
- Install the current QDock firmware: https://github.com/nicsure/quansheng-dock-fw/releases
- Connect radio to PC via AIOC or equivalent audio in, audio out and serial adapter.

On your x64 Windows PC
- Open command prompt and enter: ipconfig
- Take note of your LAN IP address. It usually starts with 192.168....
- Download and install .NET 6.0 runtime x64. https://dotnet.microsoft.com/en-us/download/dotnet/6.0
- Download qdnh.zip from the latest release and unzip it to a folder of your choice. https://github.com/nicsure/QDNH/releases
- (QDNH can generate a false positive malware alert, sorry about that but I can't control the incompetence of security software developers)
- Run QDNH.exe and allow network access if prompted
- Using the console interface of QDNH select the correct devices for the AIOC (or equivalent)
- (The audio devices will have AIOC in the name, the COM port can be anything, but you'll know if you get the right one because the lights on the AIOC will start flashing once you select the correct COM port)

On your Android device
- Navigate to the latest release section https://github.com/nicsure/QDNH/releases
- Download the QDockX APK and open it.
- If prompted, allow installation of unknown apps.
- Once installed, open the QDockX app
- Allow access to Microphone if prompted.
- Tap the Settings Cog bottom left.
- In the settings page edit the Host, it should be 127.0.0.1 initially
- Change this to the IP of the Windows PC you took note of earlier.
- Tap Back.
- .
- Be aware that QDockX is in early development and is not perfect. There will undoubtedly be bugs and glitches. Also understand that App optimization for battery usage is a tricky thing, it may take me a few more versions to fine tune it, until then the app will most likely rape your phone's battery.

# Running QDNH on Linux
The next update has full Linux support. A guide on how to install and run QDNH on Linux will be here once the new release is out.
