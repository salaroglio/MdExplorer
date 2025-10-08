-- debug
npm build
npm start "indirizzo http mdexplorer"

-- Packaging
electron-packager . ElectronMdExplorer --platform=win32 --out=release-builds --overwrite --icon=MdIcon3.ico
xcopy release-builds\ElectronMdExplorer-win32-x64 ..\Binaries\ElectronMdExplorer /E /I /H /Y /Q

-- impostare a copy allways tutte le parti
