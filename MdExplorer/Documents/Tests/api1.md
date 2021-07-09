# OMNIA REST API
This document provides the latest class diagrams for the following objects:
+ Camera
+ Detection Unit
+ Detector
+ Lpr Unit
+ Measure
+ Status
+ Traffic Light Controller
+ Variable Message Sign
+ Vbid Unit
+ Sitation Pubblication
+ Network state

Provides also the following sequence diagrams:
+ Object list
+ Object Status
+ Measurement
+ Events
+ Network State

# Camera class diagram

```plantuml
@startuml
    class "Camera" {
    +String Source
    }


    class "EquipmentParameter" {
    +String Parameter
    +String Value
    }

    class "Equipment" {
    +String Code
    +String Description
    +String GUID
    +String LastUpdateTimeUTC
    +String Model
    +String Status
    +Int32 Subsystem
    +String Type
    +String Uri
    }

    interface "IParameters" {
    }

    class "GeoInfo" {
    +Nullable<Int32> Direction
    +Double Latitude
    +Double Longitude
    +String Projection
    }

"Camera" *-- "EquipmentParameter" : Has Many \nParameters
"Camera" -up-|> "Equipment" : Extends
"Camera" -up-|> "IParameters" : Extends
"Equipment" *-- "GeoInfo" : Has A \nGeoInfo
"IParameters" *-- "EquipmentParameter" : Has Many \nParameters
@enduml
```

# Detection Unit class diagram

```plantuml
@startuml
    class "DetectionUnit" {
    }

    class "EquipmentParameter" {
    +String Parameter
    +String Value
    }

    class "Equipment" {
    +String Code
    +String Description
    +String GUID
    +String LastUpdateTimeUTC
    +String Model
    +String Status
    +Int32 Subsystem
    +String Type
    +String Uri
    }

    interface "IParameters" {
    }

    class "GeoInfo" {
    +Nullable<Int32> Direction
    +Double Latitude
    +Double Longitude
    +String Projection
    }

"DetectionUnit" *-- "EquipmentParameter" : Has Many \nParameters
"DetectionUnit" -up-|> "Equipment" : Extends
"DetectionUnit" -up-|> "IParameters" : Extends
"Equipment" *-- "GeoInfo" : Has A \nGeoInfo
"IParameters" *-- "EquipmentParameter" : Has Many \nParameters
@enduml
```