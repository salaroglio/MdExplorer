```plantuml
@startuml

start

:GetAlarmIdIfIsAlreadyCloseds; 

if (GetAlarmIdIfIsAlreadyClosed!=null) then (yes)
  :ReopenAlarm;
else (no)
  :InsertAlarm;
endif

stop

@enduml
```