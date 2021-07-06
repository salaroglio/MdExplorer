# Pacchettizzazione OmniaWeb

Attenzione è molto imoportante che la compilazione e la publish sia di
- WebDesktopApp
- WebServicesApp
  si trovi rispettivamente in 
- sia dentro il folder src\WebDesktopApp\Publish
- sia dentro il folder src\WebServicesApp\Publish


### Creazione dei folder
```plantuml
@startjson
{
    "DeployTree": [
            {
                "Action": "<color:red>CreateFolder",
                "Name": "Binary",
                "Childrens": [
                    {
                        "Action": "<color:red>CreateFolder",
                        "Name": "OmniaSetupWebEnv",
                        "Childrens": [
                            {
                                "Action": "<color:red>CreateFolder",
                                "Name": "Web"
                            }
                        ]
                    }
                ]
            }
    ]
}
@endjson
```

### Action per pacchettizare OmniaWeb
```plantuml
@startjson

#highlight "Childrens"/"0"/"Repository"
#highlight "Childrens"/"0"/"Source"
#highlight "Childrens"/"0"/"Childrens"/"0"/"Source"
#highlight "Childrens"/"1"/"Repository"
#highlight "Childrens"/"1"/"Source"
#highlight "Childrens"/"1"/"Childrens"/"0"/"Source"
#highlight "Childrens"/"2"/"Repository"
#highlight "Childrens"/"3"/"Repository"
#highlight "Childrens"/"4"/"Repository"
{
     
    "Childrens": [
        {
            "Action": "<color:red>CopyDirectoryAndZip",
            "ZipName": "WebDesktopApp.zip",
            "Source": "src\\WebDesktopApp\\Publish",
            "DestinationDirectoryName":"WebDesktopApp",
            "Repository": "GitUser-Interface",
            "Childrens": [
                {
                    "Action": "<color:red>ReplaceFileAndFolderInCurrentZip",
                    "ZipNameToAlter": "WebDesktopApp.zip",
                    "Source": "ZippedFiles\\WebDesktopApp\\web.config"
                }
            ]
        },
        {
            "Action": "<color:red>CopyDirectoryAndZip",
            "ZipName": "WebServicesApp.zip",
            "Source": "src\\WebServicesApp\\Publish",
            "DestinationDirectoryName":"WebServicesApp",
            "Repository": "GitUser-Interface",
            "Childrens": [
                {
                    "Action": "<color:red>ReplaceFileAndFolderInCurrentZip",
                    "ZipNameToAlter": "WebServicesApp.zip",
                    "Source": "ZippedFiles\\WebServicesApp\\web.config"
                }
            ]
        },
        {
            "Action": "<color:red>CopyDirectory",
            "DestinationDirectoryName": "WebData",
            "Source": "src\\WebData",
            "Repository": "GitUser-Interface"
        },
        {
            "Action": "<color:red>CopyFile",
            "Source": "src\\web.config",
            "Repository": "GitUser-Interface"
        },
        {
            "Action": "<color:red>CopyFile",
            "Source": "src\\default.aspx",
            "Repository": "GitUser-Interface"
        }
    ]
                        
}
@endjson
```