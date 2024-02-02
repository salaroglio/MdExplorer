console.log("Hello from Electron")
const { app, BrowserWindow } = require('electron')
const remote = require('electron').remote;
const path = require('path');

const createWindow = () => {
  const win = new BrowserWindow({
    width: 1024,
    height: 768,  
    icon: path.join(__dirname, 'assets/icons/IconReady.png'),  
    autoHideMenuBar:true,
    //frame:false,
    webPreferences: {
      nodeIntegration: false, // It's recommended to disable nodeIntegration for security
      contextIsolation: true, // Protect against prototype pollution
      //enableRemoteModule: false, // Turn off remote
    }
  })
  console.log(process.argv);

  const args = process.argv.slice(2);
  
  test = win.loadURL(args[0]);
}

app.whenReady().then(() => {
  createWindow()
  win.setIcon('./images/MdIcon2.ico');

  app.on('activate', () => {

    if (BrowserWindow.getAllWindows().length === 0) 
    createWindow()
  })
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
  })