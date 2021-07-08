# HOW TO COMPILE UTILITIES PROJECTS IN MICROSERVICES

### List of Projects not included in TeamCity compilation:

#### Repository SVN
- OmniaServer [-/Omnia/Source/Apps/OmniaServer]
- OmniaWCM [-/Omnia/Source/Apps/OmniaWCMService]
- QMDAdapter [-\Omnia\source\Apps\Adapters\QMD\Service]
- GenetecVideoManager [-/Omnia/Source/Apps/Adapters/Genetec]


#### Repository GIT
- User-Interface
- Microservices
  

## Installed Prerequisite
- Developer Powershell (community edition/professional edition) 
you should change the ps1 to adept to wich type you are using
- Visual studio 2019 last version
- Tortoise SVN
- GIT

We are referencing to projects still not compiled in TeamCity
- All projects that are under folder Utilities

## Description
below we are representing the sequence diagram of compiling and delivery

## Schema
``` plantuml
@startuml

actor user
participant "Tortoise svn" as svn
participant "Visual\rstudio\r2019" as vs
participant "Deploy-\rMicroservices-\rModule.ps1" as ps1
participant "DeployCore.ps1\r(contains actions)" as dcps1
participant "ConfigurationFiles \\\n DeployWithoutMicroservices.json" as jsonFile
participant "[BuildProfile\Production\] \rAdapters\rCoreServices\rUtilities\rZippedFiles" as folders
participant "version folder\n[svn]" as df

user->svn : SVN Update
user->vs : open visual studio 
user->vs : checkout release number
user->vs : compile in release \nmode Microservices Solution
user->vs : publish User-Interface
user->ps1 : start the script
ps1->jsonFile : load actions to do
ps1->ps1 : Analysis of action
ps1->dcps1: execution
dcps1->folders: get globalSettings.json \r get appSettings.json [not correct]
dcps1->dcps1: zip
dcps1->df: copy all artifacts in destination folder

@enduml
```
# Main json structure of ConfigurationFiles

- DeployConfigurationSettings
  - Repositories
    - name  -------------[Specifies the name of repository]
    - AbsolutePath ------[Specities the path on the filesystem where the root of repository start]
- BuildProfiles
  - Name ----------------[Name of folder where to search for appSettings.json and globalSettings.json]
  - Repositories
    - name --------------[Name of repositories of application where to search external files]
- DeployTree ------------[Node containing actions to do]

```plantuml
@startjson

#highlight "DeployConfigurationSettings"/"0"/"Repositories"/"0"/"AbsolutePath"
#highlight "DeployConfigurationSettings"/"0"/"Repositories"/"1"/"AbsolutePath"
#highlight "DeployConfigurationSettings"/"0"/"Repositories"/"2"/"AbsolutePath"
#highlight "DeployConfigurationSettings"/"0"/"BuildProfiles"/"0"/"Name"
{
 "DeployConfigurationSettings": [
        {
            "Repositories": [
                {
                    "Name": "GitMicroservices",
                    "AbsolutePath": "D:\\SviluppiGit\\microservices"
                },
                 {
                    "Name": "GitUser-Interface",
                    "AbsolutePath": "D:\\SviluppiGit\\user-interface"
                },
                {
                    "Name": "SVN-Apps",
                    "AbsolutePath": "D:\\Sviluppi\\Omnia\\Source\\Apps"
                }
            ],
            "BuildProfiles": [
                {
                    "Default": true,
                    "Name": "Production",
                    "Repositories": [
                        {
                            "Name": "GitMicroservices"
                        },
                        {
                            "Name": "GitUser-Interface"
                        }
                    ]
                }
            ]
        }
 ],
 "DeployTree": [
        {
            "Action": "CreateFolder",
            "Name": "Binary",
            "Childrens": []
        }
 ]
  }      
@endjson        
```

### Production Folder structure
```plantuml
@startsalt
{
    
{T+
 + <img:../../Images/folder3.png> BuildProfile 
 ++ <img:../../Images/folder3.png> Adapters
 +++ <img:../../Images/folder3.png> AdamAdapter
 ++++ <img:../../Images/json.png> appsettings.json 
 +++ <img:../../Images/folder3.png>ArpaVAdapter
 ++++ <img:../../Images/json.png> appsettings.json
 +++ <img:../../Images/folder3.png>FluxAdapter
 ++++ <img:../../Images/json.png> appsettings.json 
 +++ <img:../../Images/folder3.png>GenetecAdapter-sdk-5.7SR05
 ++++ <img:../../Images/json.png> appsettings.json
 +++ ....
 ++ <img:../../Images/folder3.png> CoreServices
 ++ <img:../../Images/folder3.png> Utilities
 ++ <img:../../Images/folder3.png> ZippedFiles
 ++ <img:../../Images/json.png> globalsettings.json
 ++ <img:../../Images/folder3.png> Test
 +++ ...
 +++ ...
}
}
@endsalt
```