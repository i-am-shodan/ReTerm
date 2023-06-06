# ReTerm

ReTerm is a proof-of-concept landscape terminal for the Remarkable 2 e-paper tablet that uses the type folio cover for input. It is written in modern .NET6 using a forked version of Remarkable.NET. It can be installed as single monolithic executable to avoid the need to deploy the dotnet runtime to your device.

![Terminal](https://github.com/i-am-shodan/ReTerm/blob/232bb1c104c7a5e5061b687bb4b5ad6d01aedc00/docs/screenshot.png)

## Installation
### Installing from GitHub
1. Go to the releases page and download the latest release - choose release-no-dotnet-runtime.zip
1. Unzip to /home/root/ReTerm on your RM2 device
1. [Optional] From the repo install the Draft launcher shortcuts
1. Run `chmod +x /home/root/ReTerm/ReTerm.sh` on your device

If you haven't installed a launcher app
1. Shut down xochitl `systemctl stop xochitl`
1. Run `/home/root/ReTerm/ReTerm.sh` to launch the terminal program

### Installing from VS Code
1. `RM2Build` and `RM2BuildDeploy` can be used to deploy to your device

### Installing from source

1. Install the dotnet 6.0 runtime on your remarkable2 device
  * [Follow this guide but ensure you are using .NET6](https://www.hanselman.com/blog/how-to-install-net-core-on-your-remarkable-2-eink-tablet-with-remarkablenet)
1. Check out this repo, **ensure you pull the submodules!**
1. Ensure .NET SDK 6.0 is installed on your local machine

`dotnet build --no-self-contained -r linux-arm -c Release src/ReTerm/ReTerm.csproj`

1. Rename `src\ReTerm\bin\Release\net6.0\linux-arm` to `ReTerm`
1. Copy the newly renamed directory to your device `/home/root/ReTerm`
1. Copy the files in `draft\` to `/opt/etc/draft`
1. On your device run `chmod +x ReTerm/ReTerm.sh`
1. Run ReTerm from your launcher

## Debugging
Remarkable.NET, the SDK this project uses, has a decent emulator you can use to run the project on your dev machine (with cavets). You can also connect a remote debugger to ReTerm running on your RM2 device. See `Debugging/README.md` for details.

## Bugs

There are more bugs in this code than features. Only try this out if you are a developer!