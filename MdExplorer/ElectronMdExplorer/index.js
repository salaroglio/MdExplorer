console.log("Hello from Electron")
const { app, BrowserWindow } = require('electron')

const createWindow = () => {
  const win = new BrowserWindow({
    width: 800,
    height: 600,
    //frame:false,
    webPreferences: {
      nodeIntegration: false, // It's recommended to disable nodeIntegration for security
      contextIsolation: true, // Protect against prototype pollution
      enableRemoteModule: false, // Turn off remote
    }
  })
  console.log(process.argv);

  const args = process.argv;
  
  win.loadURL(args);//'http://127.0.0.1:52414/client2/index.html');

  //win.loadFile('index.html')
}

app.whenReady().then(() => {
  createWindow()
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) 
    createWindow()
  })
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
  })