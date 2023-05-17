#!/bin/bash
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY=false

if [ -f "/home/root/ReTerm/ReTerm" ]; then
	chmod +x /home/root/ReTerm/ReTerm
	/opt/bin/rm2fb-client /home/root/ReTerm/ReTerm
else
	/opt/bin/rm2fb-client /home/root/dotnet/dotnet /home/root/ReTerm/ReTerm.dll
fi
