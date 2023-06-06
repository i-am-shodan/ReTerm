# Debugging

# On Device

1. Ensure ReTerm already runs on your device
1. Install VS Code with the C# extension on your dev machine
1. Install dotnet 6.0 ARM32 SDK onto your remarkable. Everything assumes your dotnet executable resides in `/home/root/dotnet/dotnet`
1. Ensure you have a working copy of `rm2fb-client`. You can install this with `opkg install display`. If other sideloaded apps seem to work you should be good to go.
1. Make sure you can access your rm2 device from Windows with key based authorization. `ssh root@YOUR_RM2_IP` needs to drop you straight into a shell.
1. SSH into your RM2 and run the following which will install the debugging tools:

`curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg"`

7. Copy the `vsdbg.sh` script to `~/vsdbg`. This script sets some environment variables the debugger needs in order to run on a rm2.
1. Edit `launch.json` and change the IP address from 172.16.1.189 to your rm2 IP.
1. Stop xocitl or whatever your launcher is. I use Remux so I run `systemctl stop remux`
1. In VS Code run the `Debug on Rm2 (remote)` debugging profile. You should see the binaries compiles and copied over to your device in the terminal Window.
1. Happy debugging 

# Emulator
Remarkable.NET, the SDK this project uses, has a decent emulator you can use to run the project on your dev machine. This currently uses bash running under WSL2 to give some console out. Due to the way that this process is started and data read from it there are some issues with display rendering.

1. Install WSL2
1. Install VS Code with the C# extension on your dev machine
1. In VS Code run the `Debug on Emulator` debugging profile.