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

On your x64 Windows PC (For Linux look below)
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
I'm going to assume that Linux users are going to be a little more savvy and will know how to deal with firewalls and such. I cannot go through every possible distro and hardware variation out there in the Linux space. The following procedure worked on a 10ZIG thin client freshly installed with Lubuntu 22.04. The thin client cost me £15 on eBay and is about a decade old. So if it works on that, it should work on most things. Depending on your distro and hardware you may need to work some stuff out for yourself, if that's too much for you then I'd suggest that Linux isn't the OS for you, stick to Windows.   
  
Let's first deal with some pre-requisites.
- Open a bash console/terminal
- Install .NET 6.0 runtime package. ( Info: https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual )
  - wget ht</b>tps://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  - chmod +x ./dotnet-install.sh
  - ./dotnet-install.sh --channel 6.0
- Install ASound libraries
  - sudo apt install libasound2-dev
  - (Non-Debian based distros may have other package management commands)  

To find your IP Address
- In the terminal type
  - ip -4 address  

Now for QDNH
- Fetch the zip file from the latest release https://github.com/nicsure/QDNH/releases
  - (Make sure you select the correct architecture for your device/PC, x64 or ARM64)
- Unzip this to a folder of your choice.
- In the terminal navigate into this folder
- Make the startup script executable
  - chmod +x ./qdnh.sh
- Run QDNH by executing the startup script
  - ./qdnh.sh


