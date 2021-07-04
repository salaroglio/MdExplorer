# MD Explorer Docs
MDExplorer ha lo scopo di facilitare la VISUALIZZAZIONE dei file MD.
Eventualmente anche di editarli, ma deve essere uno strumento facile per visualizzare 
la documentazione. Lo sto sviluppando per studiare la possibilità di creare una
toolchain, che parta dalla 
definizione delle  specifiche di un applicativo,
definizione degli use case
il disegno di un mockup
definizione del diagramma  entità relazioni e relative spiegazioni
definizione degli test script  
definizione della architettura dell'applicativo 
* [Architecture](Architecture/MainArchitecture.md)
* Programming
	* [At startup of MDExplorer](Programming/MDExplorer.Service/startup.md) 
	* [How Works Autorefresh?](Programming/MDExplorer.Service/HowWorksAutoRefresh.md)
* [Configurazione](ServerConfiguration\plantuml.md)
## Features 	  
### Visualization  
at the moment it's showing the markdown's rendering
### Plantuml Integration
rendering with plantuml is available
### AutoRefresh  
sensing changes and autorefresh the page
### Change file name
F2 and change the file name of file
### Spot Edit 
Adding a sidebar just to update markdown text file
### GIT (gitlab) integration
managing branches and merge.
### Configurable plantuml server



m↓ShowMd(Architecture\MainArchitecture.md)

# E' un server

Già già... è server .net core 3.1 con dentro Angular 11.

Già fa schifo il template che viene messo in .net Core per angular 8... quindi ho trovato un modo di metterci l'ultima versione

```Code
-- terminal di visual studio
ng build --base-href /client/ --watch

test gghhdfdf

```
