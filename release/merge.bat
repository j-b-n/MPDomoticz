@echo on
IF EXIST Newtonsoft.Json.dll del Newtonsoft.Json.dll
IF EXIST MPDomoticz.dll del MPDomoticz.dll
IF EXIST OxyPlot.dll del OxyPlot.dll
IF EXIST OxyPlot.WindowsForms.dll del OxyPlot.WindowsForms.dll
copy ..\MP-Domoticz\bin\Debug\MP-Domoticz.dll .
copy ..\MP-Domoticz\bin\Debug\Newtonsoft.Json.dll .
copy ..\MP-Domoticz\bin\Debug\OxyPlot.dll .
copy ..\MP-Domoticz\bin\Debug\OxyPlot.WindowsForms.dll .
rem ren MP-Domoticz.dll MP-Domoticz_UNMERGED.dll 
ilmerge /targetplatform:"v4,C:\Windows\Microsoft.NET\Framework64\v4.0.30319" /out:MPDomoticz.dll MP-Domoticz.dll Newtonsoft.Json.dll OxyPlot.dll OxyPlot.WindowsForms.dll /lib:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0" /lib:"C:\Program Files (x86)\Team MediaPortal\MediaPortal" /lib:"C:\Program Files (x86)\Team MediaPortal\MediaPortal\plugins\Windows"

