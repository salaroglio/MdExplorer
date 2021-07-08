# Processo decisionale creazione nuova Release

Fogli excel di interazione con Antonella

![](../Images/excel.png)[OmniaPackagesDesign_Ver3_24](../assets/OmniaPackagesDesign_Ver3_24.xlsx)
![](../Images/excel.png)[NOTE_Progetto_OMNIA_RiferimentiArtifatti_v1](../assets/NOTE_Progetto_OMNIA_RiferimentiArtifatti_v1.0.xlsx)

```plantuml
@startuml
skinparam ParticipantPadding 20
skinparam BoxPadding 10

box "Developers/\nin charge of deploy<img:../Images/swarco.png>"
actor "Carlo" as carlo
end box

box "Devops<img:../Images/swarco.png>"
actor "Antonella" as anto
end box

actor "responsabili swarco" as res

carlo->anto : comunicazione che i pacchetti zip\rsono caricati sul folder SVN
carlo->anto : comunicazione dei nuovi servizi inseriti
anto->carlo :  <img:../Images/excel.png> richiesta di completamento di foglio excel
carlo->anto : compilazione e restituzione foglio excel <img:../Images/excel.png>
carlo->res : invio mail urbi et orbi<img:../Images/mail.png><img:../Images/mail.png>

@enduml
```