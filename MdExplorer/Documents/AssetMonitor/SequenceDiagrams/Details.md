```plantuml
@startuml

actor user
participant Browser AS browser
participant "WebDesktopApp\n(AssetMonitorHandler.ashx)" as WDA
participant "WebServicesApp\n(WS_AssetMonitor.asmx)" as WSA
database OMNIA 
participant "GISAssetAdapter" as GisAssetAdatper
queue "IBM Integration Bus" as IBM

user->browser: get details
group "Get Details"
    browser -> WDA: Get Details
    WDA --> WSA: get configuration proxy\n(the endpoint uri, which columns to request)
    WSA --> OMNIA: get proxy url
    OMNIA --> WSA
    WSA --> WDA
    WDA --> GisAssetAdatper: using WebClient,\nexecute a GET request\only first row
    GisAssetAdatper --> IBM
    IBM --> GisAssetAdatper
    GisAssetAdatper --> WDA: page data from GIS
end

@enduml
