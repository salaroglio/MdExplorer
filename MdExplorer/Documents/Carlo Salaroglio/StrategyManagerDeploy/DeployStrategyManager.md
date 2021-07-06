# Deploy strategy manager

### Prerequisite
Install-Module VSSetup -Scope CurrentUser
	
Find-Module psake | Install-Module

Invoke-psake .\PsakeSample.ps1
Import-Module PackageManagement
Find-Package Pester
Install-Module Pester -Force
Update-Module Pester -Force

https://www.howtogeek.com/266621/how-to-make-windows-10-accept-file-paths-over-260-characters/
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem "longpathenabled" = 1

