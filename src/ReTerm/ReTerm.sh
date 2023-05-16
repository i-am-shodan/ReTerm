#!/bin/bash
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY=false
cd /home/root
/opt/bin/rm2fb-client /home/root/dotnet/dotnet /home/root/ReTerm/ReTerm.dll
