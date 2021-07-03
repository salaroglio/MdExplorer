<style>
#overlay {
  position:relative;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  border:solid;
  /*background-color: lightgreen;  Black background with opacity */
  z-index: 2; /* Specify a stack order in case you're using a different order for other elements */
  cursor: pointer; /* Add a pointer on hover */
}
/*Important:*/
.link-spanner{
  position:absolute; 
  width:100%;
  height:100%;
  top:0;
  left: 0;
  z-index: 1;

  /* edit: fixes overlap error in IE7/8, 
     make sure you have an empty gif 
  background-image: url('empty.gif');*/
}  
</style>

# Cominciamo da zero

### Idee

- 2021-06-29 09:25:53 si tratta di segnarae tutte le idee che mi vengono in mente
- Mi servono le seguenti funzioni
  - m↓show-sql(path-file)
  - m↓show-plantuml(path-file)
  - m↓show-git-current-version(<path-file)
  - m↓show-image(path-file)
  - -m↓show-markdown(path-file)



## m↓show-xxx
il concetto è quello di visualizzare dentro l'html corrente il file indicato e di conservare
la navigabilità a quel file tramite click.
quindi devo coprire tutta l'area e renderla cliccabile con un link al file stesso.
Ci vorrebbe un layer cliccabile
Sembra che si possa fare con un div


<div id="overlay"  >
<html>
<head></head>
<body>


# Use case

## Creazione di una area di copertura del testo come link

### Problema
Si vogliono tenere separati i vari file, perché siano immediatamente "recuperati"
in tutti i contesti necessari.
Ex: file sql dovrebbe essere visualizzato ed anche eseguito
il file plantuml dovrebbe essere inserito nella analisi della architettura e magari contemporamente anche nello use case
che da una migliore comprensione delle dinamiche, evitando mille copia incolla dentro fogli word


### soluzione
rendere cliccabile una area di testo perché porti l'utente a "saltare" direttamente al file linkato
ma contemporaneamente deve mostrarne il contenuto renderizzato

</body>
</html>


<a href=".\features\usecase\ucshowanything.md">
        <span class="link-spanner"></span>
    </a>
</div>



fermati qui