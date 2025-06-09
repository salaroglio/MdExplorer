# Readme

' appunti per il deploy'

1-compila

2-crea il package

3-copia dentro binaries (tanto non è dentro git perché troppo grande, va copiato a mano dentro l'ambiente di compilazione)

4-per provare i cambiamenti, eseguire npm start e cambiare la porta, quando viene fatto partire in debug

npm build
electron-packager . ElectronMdExplorer --platform=win32 --out=release-builds --overwrite
xcopy release-builds\ElectronMdExplorer-win32-x64 ..\Binaries\ElectronMdExplorer /E /I /H /Y /Q

<br />

<br />

**npm start** "<http://127.0.0.1:55820/client2/main/navigation/document>"
