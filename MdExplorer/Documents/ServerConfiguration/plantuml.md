
# Configurazione del server plantuml m↓Branch()
m↓Contributors()

l'ideale sarebbe quello di installare WSL 2
e poi eseguire le linee di codice
- sudo dockerd
- docker run -d -p 8080:8080 plantuml/plantuml-server:jetty

e poi lasciare i processi aperti, che ti diano il server per generare i file
``` plantuml
@startuml
actor Carlo 
participant "Visual studio Code" as VSC #cyan
participant File #white
participant MdClient
participant MdServer
participant PlantumlServer

Carlo->VSC
VSC->File: write on file
File->MdServer: trigger change
MdServer-->MdServer: transform Md into HTML
MdServer-->PlantumlServer: transform diagram in SVG
MdServer->MdClient: send changes
MdClient->Carlo: show new html


@enduml
```