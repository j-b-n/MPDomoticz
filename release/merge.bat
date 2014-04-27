@echo on
IF EXIST MPDomoticz.dll del MPDomoticz.dll
rem ren MP-Domoticz.dll MP-Domoticz_UNMERGED.dll 
ilmerge /targetplatform:"v4,C:\Windows\Microsoft.NET\Framework64\v4.0.30319" /out:MPDomoticz.dll MP-Domoticz.dll Newtonsoft.Json.dll OxyPlot.dll OxyPlot.WindowsForms.dll /lib:"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0" /lib:"C:\Program Files (x86)\Team MediaPortal\MediaPortal" /lib:"C:\Program Files (x86)\Team MediaPortal\MediaPortal\plugins\Windows"
