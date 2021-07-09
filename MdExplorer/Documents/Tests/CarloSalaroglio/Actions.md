# Actions For Deploy in Strategy Manager

### Createfolder
Utilizzato per creare l'alberatura dei folder. Tutto quello che avviene dentro l'array **Childrens** viene creato dentro il folder stesso
```plantuml
@startjson
#highlight "Action"
{
    "Action": "CreateFolder",
    "Name": "OmniaSetupPadova2020Env",
    "Childrens": []
}
@endjson
```


### Copy directory
```plantuml
@startjson
#highlight "Action"
#highlight "Repository"

{
    "Action": "CopyDirectory",
    "DestinationDirectoryName": "Script",
    "Source": "Script",
    "Repository": "GitStrategymanager"
}
@endjson
```

### Compile Solution
diversamente dal deploy dei microservices, qui è stato implementata una libreria che è effettivamente
in grado di compilare i sorgenti. Non deve essere fatto esternamente come nell'altro caso
```plantuml
@startjson
#highlight "Action"
#highlight "Repository"
{
    "Action":"CompileSolution",
    "SolutionPath":"WinServices\\StrategyManagerEvaluationEngine\\StrategyManagerEvaluationEngine.sln",
    "CompileScript":"BuildFramework451.ps1",
    "Config":"Release",
    "Publish":false,
    "PublishProfile":"",
    "Repository":"GitStrategymanager",
    "Childrens": [
        {
            "Action": "CopyDirectoryAndZip",
            "ZipName": "StrategyManagerEvaluationEngine.zip",
            "Source": "WinServices\\StrategyManagerEvaluationEngine\\Service\\bin\\Release",
            "DestinationDirectoryName":"StrategyManagerEvaluationEngine",
            "Repository": "GitStrategymanager",
            "IsZipped": true,
            "Childrens": [
                {
                    "Action": "ReplaceFileAndFolderInCurrentZip",
                    "ZipNameToAlter": "StrategyManagerEvaluationEngine.zip",
                    "Source": "WinServices\\StrategyManagerEvaluationEngine\\StrategyManagerEvaluationEngine.exe.config"
                }
            ]
        }]
}
@endjson
```
### CopyDirectoryAndZip
Tipicamente questa azione avviene DOPO che è stato compilato con successo una solution
```plantuml
@startjson
#highlight "Childrens"/"0"/"Action"
#highlight "Childrens"/"0"/"Source"
#highlight "Repository"
{
    "Action":"CompileSolution",
    "SolutionPath":"WinServices\\StrategyManagerEvaluationEngine\\StrategyManagerEvaluationEngine.sln",
    "CompileScript":"BuildFramework451.ps1",
    "Config":"Release",
    "Publish":false,
    "PublishProfile":"",
    "Repository":"GitStrategymanager",
    "Childrens": [
        {
            "Action": "CopyDirectoryAndZip",
            "ZipName": "StrategyManagerEvaluationEngine.zip",
            "Source": "WinServices\\StrategyManagerEvaluationEngine\\Service\\bin\\Release",
            "DestinationDirectoryName":"StrategyManagerEvaluationEngine",
            "Repository": "GitStrategymanager",
            "IsZipped": true,
            "Childrens": [
                {
                    "Action": "ReplaceFileAndFolderInCurrentZip",
                    "ZipNameToAlter": "StrategyManagerEvaluationEngine.zip",
                    "Source": "WinServices\\StrategyManagerEvaluationEngine\\StrategyManagerEvaluationEngine.exe.config"
                }
            ]
        }
    ]
}
@endjson
```
### Replace Files and Folder in current zip
In questo caso viene inserito/sostituito il contenuto di una directory config dentro lo zip StrategyManagerMonitorService.zip
```plantuml
@startjson
#highlight "Action"
#highlight "ZipName"
#highlight "Childrens"/"0"/"Action"
#highlight "Repository"
{
    "Action": "CopyDirectoryAndZip",
    "ZipName": "StrategyManagerMonitorService.zip",
    "Source": "...path..\\bin\\StrategyManagerMonitorService",
    "DestinationDirectoryName":"StrategyManagerMonitorService",
    "Repository": "GitStrategymanager",
    "IsZipped": true,
    "Childrens": [
        {
            "Action": "ReplaceFileAndFolderInCurrentZip",
            "ZipNameToAlter": "StrategyManagerMonitorService.zip",
            "Source": "WinServices\\StrategyManagerMonitorService\\Config"
        }
    ]
}
@endjson
```

