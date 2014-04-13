@echo on
rem IF EXIST MP-Domoticz_UNMERGED.dll del MP-Domoticz_UNMERGED.dll
rem ren MP-Domoticz.dll MP-Domoticz_UNMERGED.dll 
ilmerge /out:MPDomoticz.dll MP-Domoticz.dll Newtonsoft.Json.dll 
