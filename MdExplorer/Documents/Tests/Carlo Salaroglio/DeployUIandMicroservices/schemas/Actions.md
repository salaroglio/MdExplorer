# Description of available actions

Available actions

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
- CopyFileFromSource
- 
- CopyDirectoryFromSource
### DeletePointlessFile
Utilizzato per cancellare file inutili provenienti dalla compilazione
```plantuml
@startjson
#highlight "Childrens"/"0"/"Action"
#highlight "Childrens"/"1"/"Action"
{
    "Action": "PackCompiledProject",
    "Name": "ScatsMonitorLauncher",
    "Type": "Util",
    "ProjectType": "C++",
    "IsZipped": false,
    "BinariesPath": "Release",
    "Repository": "GitMicroservices",
    "Childrens": [
        {
            "Action": "DeletePointlessFile",
            "FileName": "ScatsMonitorLauncher.iobj"
        },
        {
            "Action": "DeletePointlessFile",
            "FileName": "ScatsMonitorLauncher.ipdb"
        }
    ]
}
@endjson
```

### CopyDirectoryAndZip
copia una directory inserita dentro source, gli da il nome ZipName, imposta la destinazione della directory.  
```plantuml
@startjson
#highlight "Action"
{
    "Action": "CopyDirectoryAndZip",
    "ZipName": "GenetecVideoManager-sdk-5.7.zip",
    "Source": "Adapters\\Genetec\\service5.7\\bin\\GenetecVideoManager5.7",
    "DestinationDirectoryName":"GenetecVideoManager5.7",                                            
    "Repository": "SVN-Apps",
    "IsZipped": true,
    "Childrens": [
        {
            "Action": "ReplaceFileAndFolderInCurrentZip",
            "ZipNameToAlter": "GenetecVideoManager-sdk-5.7.zip",
            "Source": "Adapters\\GenetecVideoManager-sdk-5.7\\GenetecVideoManager.exe.config"
        },
        {
            "Action": "ReplaceFileAndFolderInCurrentZip",
            "ZipNameToAlter": "GenetecVideoManager-sdk-5.7.zip",
            "Source": "Adapters\\GenetecVideoManager-sdk-5.7\\MediaDownload"
        }
    ]
}
@endjson
```
### ReplaceFileAndFolderInCurrentZip
Unzippa il file zip che prende da **ZipNameToAlter** e sostituisce/aggiunge il file indicato in **Source**
```plantuml
@startjson
#highlight "Childrens"/"0"/"Action"
{
    "Action": "CopyDirectoryAndZip",
    "ZipName": "GenetecVideoManager-sdk-5.7.zip",
    "Source": "Adapters\\Genetec\\service5.7\\bin\\GenetecVideoManager5.7",
    "DestinationDirectoryName":"GenetecVideoManager5.7",                                            
    "Repository": "SVN-Apps",
    "IsZipped": true,
    "Childrens": [
        {
            "Action": "ReplaceFileAndFolderInCurrentZip",
            "ZipNameToAlter": "GenetecVideoManager-sdk-5.7.zip",
            "Source": "Adapters\\GenetecVideoManager-sdk-5.7\\GenetecVideoManager.exe.config"
        },
        {
            "Action": "ReplaceFileAndFolderInCurrentZip",
            "ZipNameToAlter": "GenetecVideoManager-sdk-5.7.zip",
            "Source": "Adapters\\GenetecVideoManager-sdk-5.7\\MediaDownload"
        }
    ]
}
@endjson
```