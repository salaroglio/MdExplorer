# Sviluppo MD Explorer
Cioè ragazzi voglio il nobel !
dunque... divertiamoci a documentare questo progetto che serve appunto per la documentazione

# E' un server

Già già... è server .net core 3.1 con dentro Angular 11.

Già fa schifo il template che viene messo in .net Core per angular 8... quindi ho trovato un modo di metterci l'ultima versione

```Code
-- terminal di visual studio
ng build --base-href /client/ --watch

--ubuntu
docker run -d -p 8080:8080 plantuml/plantuml-server:jetty

```
