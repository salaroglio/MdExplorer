
# Asset Monitor
## Documento di riferimento
### [USE CASE Reference document](CRMS-EAMS-RMCSS_Integr_UseCase_20210216_CGML.docx)

## [1] GIS MATCH
Aggiungere uno nuovo stato: GIS MATCH come Parameters


m↓ShowMd(AssetMonitor\StateDiagrams\AssetMonitorStates.md)

### Icons

![alt text](CustomFiles/plus.png "Title") NEW ![alt text](CustomFiles/linked.png "Title") LINKED ![alt text](CustomFiles/no-linked.png "Title") MISMATCH 
![alt text](CustomFiles/multimatch.png "Title") MULTI_MATCH ![alt text](CustomFiles/no-match.png "Title") NO_MATCH ![alt text](CustomFiles/question.png "Title") LOST


## [2] GIS LAST UPDATE
Supportare (individuare) un campo __gis last update__ presentato nella UI che serva all'operatore come _"ultimio aggiornamento"_ del dato come Parameters. 

Puo essere anche usato dal uservice per gestire gli allineamenti con il GIS?


NEW --> GIS LU NULL
LINKED --> GIS LU when link has made
NO_MATCH --> GIS LU
MULTI_MATCH --> GIS LU
MISMATCH --> GIS LU
LOST --> GIS LU


Nel caso di connection lost con IBM integration Bus allora abbiamo che da UI deve essere segnalato all'utente un messaggio di errore:
__IBM Integration Bus not reachable__  (fallisce la chiamata al GISAssetAdapter)

Invece lato EAMS / GIS Adapter nel caso di connection lost con IBM integration Bus non seguono azioni, perchè si vuole così evidenziare tramite il campo __gis last update__ che la freschezza del dato sta diminuendo nel tempo.

---

Non si usano tabelle di assetu_ui_parameters perchè è il proxy GisAssetAdatper che in configurazione gestisce i 5 fratelli + description e gis last update, mentre per la vista di dettagliopassa sempre l'asterisco (*)

---

Gli asset non gestiti da RMC tipo il palo della luce avrà il gis last update valorizzato quando faccio la query al gis endpoint in realtime

### Esempio
Per quel specifico endpoint sono riuscito a parlarci l'ultima volta ieri alle 15, mi aspetto di vedere quella data nel campo last update per tutti gli asset gestiti da quel endpoint.
<!-- blank line -->


m↓ShowMd(AssetMonitor\SequenceDiagrams\Grid.md)

m↓ShowMd(AssetMonitor\SequenceDiagrams\Details.md)

---


## [3] GIS MATCH

Tabelle coinvolte nell'inserimento del GIS Match:

|  |  Cameras* | DetectionUnits*| Detectors*  | EnvironmentSensors* | LprUnits* | MeasurementStations** | ParkingHouses* | Controllers* | VariableMessageSigns* | VbidUnits*
| -----| :-----:|  :-----:| :-----:| :-----:| :-----:| :-----:| :-----:| :-----:| :-----:| :-----:|
trafficCameras | X |   
speedCameras  |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
trafficSignalControlBox |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
trafficDetectors |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
RWIS |  | X |  |   |  |  |  |  |  |  |  |  |  |  |  |
electronicSigns |  |   |  |   |  |  |  |  | X |  |  |  |  |  |  |
lowBridge |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
trafficSignal |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
roadParking |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
roadBarriers |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
roadTunnels |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
weighInMotion |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
trafficSignalJunction |  |   |  |   |  |  |  |  |  |  |  |  |  |  |  |
