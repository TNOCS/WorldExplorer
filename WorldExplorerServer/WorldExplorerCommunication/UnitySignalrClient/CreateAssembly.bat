@echo off
cd .\bin\Debug\netstandard2.0
echo "Merge all ASP NET core signalr libraries to one dll."
"./../../../tools/ilmerge/ilmerge.exe" ^
/out:"./../../../../UnityDeployment/SignalR_AspNetCore_Client_Library.dll" ^
Microsoft.AspNetCore.SignalR.Client.dll ^
Microsoft.AspNetCore.Connections.Abstractions.dll ^
Microsoft.AspNetCore.Http.Connections.Client.dll ^
Microsoft.AspNetCore.Http.Connections.Common.dll ^
Microsoft.AspNetCore.Http.Features.dll ^
Microsoft.AspNetCore.SignalR.Client.Core.dll ^
Microsoft.AspNetCore.SignalR.Common.dll ^
Microsoft.AspNetCore.SignalR.Protocols.Json.dll ^
Microsoft.Extensions.Configuration.Abstractions.dll ^
Microsoft.Extensions.Configuration.Binder.dll ^
Microsoft.Extensions.Configuration.dll ^
Microsoft.Extensions.DependencyInjection.Abstractions.dll ^
Microsoft.Extensions.DependencyInjection.dll ^
Microsoft.Extensions.Logging.Abstractions.dll ^
Microsoft.Extensions.Logging.dll ^
Microsoft.Extensions.Options.dll ^
Microsoft.Extensions.Primitives.dll ^
Newtonsoft.Json.dll ^
System.Buffers.dll ^
System.ComponentModel.Annotations.dll ^
System.IO.Pipelines.dll ^
System.Memory.dll ^
System.Numerics.Vectors.dll ^
System.Runtime.CompilerServices.Unsafe.dll ^
System.Threading.Channels.dll ^
System.Threading.Tasks.Extensions.dll
echo "Copy files"
copy /y UnitySignalrClient.dll .\..\..\..\..\UnityDeployment\UnitySignalrClient.dll
pause
