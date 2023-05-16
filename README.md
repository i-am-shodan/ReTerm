
# ReTerm

ReTerm is a ALPHA quality/proof-of-concept landscape terminal for the Remarkable 2 e-paper tablet that uses the type folio cover for input.



# Building and installing

1. Install the dotnet 6.0 runtime on your remarkable2 device
  * [Follow this guide but ensure you are using .NET6](https://www.hanselman.com/blog/how-to-install-net-core-on-your-remarkable-2-eink-tablet-with-remarkablenet)
1. Check out this repo
1. Ensure .NET SDK 6.0 is installed on your local machine

`dotnet build --no-self-contained -r linux-arm -c Release src/ReTerm/ReTerm.csproj`

1. Rename `src\ReTerm\bin\Release\net6.0\linux-arm` to `ReTerm`
1. Copy the newly renamed directory to your device `/home/ReTerm`
1. Copy the files in `draft\` to `/opt/etc/draft`
1. On your device run `chmod +x ReTerm/ReTerm.sh`
1. Run ReTerm from your launcher

# Bugs

There are more bugs in this code than features. Only try this out if you are a developer!