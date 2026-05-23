dotnet publish MAMM.Signer.Cli -c Release
dotnet publish MAMM.Signer.Gui -c Release
dotnet publish MAMM.Signer.Interop -c Release -f net48
dotnet publish MAMM.Signer.Interop -c Release -f net10.0-windows --no-self-contained
