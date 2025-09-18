// MdExplorer/ElectronMdExplorer/index.js
const { app, BrowserWindow, ipcMain } = require('electron'); // Added ipcMain
const path = require('path');
const net = require('net');
const { spawn } = require('child_process');
const fs = require('fs'); // Added fs module

// --- Start Logging Setup ---
const logFilePath = path.join(app.getPath('userData'), 'electron_main.log');
const logStream = fs.createWriteStream(logFilePath, { flags: 'a' });

const originalConsoleLog = console.log;
const originalConsoleError = console.error;

console.log = (...args) => {
  const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg) : arg).join(' ');
  logStream.write(`[LOG] ${new Date().toISOString()} ${message}\n`);
  originalConsoleLog(...args);
};

console.error = (...args) => {
  const message = args.map(arg => typeof arg === 'object' ? JSON.stringify(arg) : arg).join(' ');
  logStream.write(`[ERROR] ${new Date().toISOString()} ${message}\n`);
  originalConsoleError(...args);
};
// --- End Logging Setup ---

console.log("Hello from Electron - Modified");

// Function to find a free port
const findFreePort = () => {
  return new Promise((resolve, reject) => {
    const server = net.createServer();
    server.unref(); // Important to allow the program to exit if this is the only server running.
    server.on('error', (err) => {
      server.close(); // Ensure server is closed on error before rejecting
      reject(err);
    });
    server.listen(0, () => { // 0 tells the OS to give us any free port
      const port = server.address().port;
      server.close(() => { // Close the server once we have the port
        resolve(port);
      });
    });
  });
};

let mainWindow; // Keep a reference to the main window
let mdServiceProcess; // Keep a reference to the spawned service process
let isAppQuitting = false; // Flag to track if the app is quitting
let targetUrl; // Moved targetUrl to global scope

const createWindow = async () => {
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.focus();
    return;
  }

  mainWindow = new BrowserWindow({
    width: 1024,
    height: 768,
    frame: false, // Remove native title bar
    icon: path.join(__dirname, 'mdExplorer.png'), // MdExplorer custom icon
    autoHideMenuBar: true,
    webPreferences: {
      nodeIntegration: false, // Security best practice
      contextIsolation: true,  // Security best practice
      // enableRemoteModule: false, // Deprecated, ensure it's off
      preload: path.join(__dirname, 'preload.js') // Use the preload script
    }
  });

  console.log('Command line arguments:', process.argv);
  const cliArgs = process.argv.slice(2); // First two are executable and script path

  if (cliArgs.length > 0 && cliArgs[0]) {
    // URL is provided as a command line argument
    targetUrl = cliArgs[0];
    console.log(`URL provided via argument: ${targetUrl}`);
    try {
      await mainWindow.loadURL(targetUrl);
    } catch (error) {
      console.error(`Failed to load provided URL ${targetUrl}:`, error);
      mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Failed to load URL: ${targetUrl}</p><pre>${error.message}</pre>`);
    }
  } else {
    // No URL provided, attempt to start MdExplorer.Service on a free port
    console.log('No URL provided. Displaying loading screen and attempting to start MdExplorer.Service...');
    
    // Load the loading.html page first
    try {
      await mainWindow.loadFile(path.join(__dirname, 'loading.html'));
      console.log('Loading page displayed.');
    } catch (loadError) {
      console.error('Failed to load loading.html:', loadError);
      // Fallback or error display if loading.html itself fails
      mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Failed to load initial loading screen.</p>`);
      return; // Stop if we can't even show the loading screen
    }

    try {
      const freePort = await findFreePort();
      console.log(`Found free port: ${freePort}`);
      targetUrl = `http://localhost:${freePort}/client2/index.html`; // Default URL to load

      // Determine the path to MdExplorer.Service executable
      const executableName = process.platform === 'win32' ? 'MdExplorer.Service.exe' : 'MdExplorer.Service';
      const isPackaged = app.isPackaged;
      let serviceExecutablePath = '';
      let serviceWorkingDirectory = '';
      const absoluteSearchedPaths = []; // Array to store all absolute paths checked

      if (isPackaged) {
        // Packaged app: Service is in resources/app_service
        const baseResourcePath = process.resourcesPath; // Path to resources folder
        serviceExecutablePath = path.join(baseResourcePath, 'app_service', executableName);
        serviceWorkingDirectory = path.join(baseResourcePath, 'app_service');
        console.log(`Packaged app: Attempting to find service at ${serviceExecutablePath}`);
        absoluteSearchedPaths.push(serviceExecutablePath);
      } else {
        // Development mode:
        // Option A: Expect service in ElectronMdExplorer/service_payload/
        const devServicePayloadPath = path.join(__dirname, 'service_payload');
        serviceExecutablePath = path.join(devServicePayloadPath, executableName);
        serviceWorkingDirectory = devServicePayloadPath;
        console.log(`Development (using service_payload): Trying ${serviceExecutablePath}`);
        absoluteSearchedPaths.push(serviceExecutablePath);

        // Option B: Fallback to a direct path to MdExplorer/dist_service
        if (!fs.existsSync(serviceExecutablePath)) {
            const devDistServicePath = path.join(__dirname, '..', 'MdExplorer', 'dist_service');
            serviceExecutablePath = path.join(devDistServicePath, executableName);
            serviceWorkingDirectory = devDistServicePath;
            console.log(`Development (using MdExplorer/dist_service): Trying ${serviceExecutablePath}`);
            absoluteSearchedPaths.push(serviceExecutablePath);
        }
      }

      if (!serviceExecutablePath || !fs.existsSync(serviceExecutablePath)) {
        const searchedPathsForDisplayHTML = absoluteSearchedPaths.filter(p => p).join('<br>');
        const searchedPathsForConsole = absoluteSearchedPaths.filter(p => p).join('\n');
        const modeMsg = isPackaged ? "packaged resources" : "development paths";
        console.error(`${executableName} not found in ${modeMsg}. Searched paths:\n${searchedPathsForConsole}`);
        if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.loadURL(`data:text/html,<h1>Service Error</h1><p>${executableName} not found in ${modeMsg}.</p><pre>Searched paths:<br>${searchedPathsForDisplayHTML.replace(/\n/g, '<br>')}</pre>`);
        }
        return; // Stop further execution if executable not found
      }
      
      console.log(`Attempting to start MdExplorer.Service executable:`);
      console.log(`  Executable: ${serviceExecutablePath}`);
      console.log(`  Working Directory: ${serviceWorkingDirectory}`);
      console.log(`  Port: ${freePort}`);

      mdServiceProcess = spawn(
        serviceExecutablePath,    // Direct path to the executable
        [freePort.toString()],    // Arguments: only the port
        {
          cwd: serviceWorkingDirectory, // Set CWD to the executable's directory
          stdio: ['ignore', 'pipe', 'pipe'], 
        }
      );

      mdServiceProcess.stdout.on('data', async (data) => { // Made this callback async
        const message = data.toString().trim();
        console.log(`[Service Output] stdout: ${message}`);
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('service-output', message);
          if (message.includes('Application started.')) {
            console.log('[Service Output] "Application started." detected. Directly loading main URL.');
            if (mainWindow && !mainWindow.isDestroyed() && targetUrl) {
              try {
                await mainWindow.loadURL(targetUrl);
              } catch (error) {
                console.error(`Failed to load main URL ${targetUrl} after service ready:`, error);
                mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Failed to load main application after service started.</p><pre>${error.message}</pre>`);
              }
            } else {
              console.warn('[Service Output] Service ready, but mainWindow is destroyed or targetUrl is null.');
              if (mainWindow && !mainWindow.isDestroyed()) {
                mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Service ready, but target URL is missing or window destroyed.</p>`);
              }
            }
          }
        }
      });

      mdServiceProcess.stderr.on('data', (data) => {
        const message = data.toString().trim();
        console.error(`[Service Output] stderr: ${message}`);
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('service-output', `ERROR: ${message}`);
        }
      });

      mdServiceProcess.on('error', (err) => {
        console.error('Failed to start MdExplorer.Service process:', err);
        if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.loadURL(`data:text/html,<h1>Service Error</h1><p>Failed to start MdExplorer.Service.</p><pre>${err.message}</pre>`);
        }
      });

      mdServiceProcess.on('exit', (code, signal) => {
        console.log(`MdExplorer.Service process exited with code ${code} and signal ${signal}`);
        if (code !== 0 && !isAppQuitting) { // Use the flag here
            if (mainWindow && !mainWindow.isDestroyed()) {
                 mainWindow.loadURL(`data:text/html,<h1>Service Exited</h1><p>MdExplorer.Service process exited unexpectedly (code ${code}).</p>`);
            }
        }
      });
      
      // Removed the fixed 20-second timeout.
      // The transition to the main URL will now be triggered by the 'service-ready' IPC message.

    } catch (error) {
      console.error('Error during fallback service setup or URL loading:', error);
      if (mainWindow && !mainWindow.isDestroyed()) {
        mainWindow.loadURL(`data:text/html,<h1>Initialization Error</h1><p>Could not start the required service or find a port.</p><pre>${error.message}</pre>`);
      }
    }
  }

  // Listen for F12 to toggle DevTools
  mainWindow.webContents.on('before-input-event', (event, input) => {
    if (input.key === 'F12' && !input.alt && !input.control && !input.meta && !input.shift) {
      console.log('[DevTools] F12 pressed, toggling dev tools');
      mainWindow.webContents.toggleDevTools();
      event.preventDefault(); 
    }
  });

  // Listen for window control events
  ipcMain.on('minimize-window', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.minimize();
    }
  });

  ipcMain.on('maximize-window', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      if (mainWindow.isMaximized()) {
        mainWindow.unmaximize();
      } else {
        mainWindow.maximize();
      }
    }
  });

  ipcMain.on('close-window', () => {
    if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.close();
    }
  });

  // Listen for zoom events from preload script
  ipcMain.on('zoom-mouse-wheel', (ipcEvent, args) => {
    if (mainWindow && !mainWindow.isDestroyed() && mainWindow.webContents) {
      console.log(`[ZoomIPC] Received zoom-mouse-wheel event, deltaY: ${args.deltaY}`);
      let currentZoom = mainWindow.webContents.getZoomFactor();
      console.log(`[ZoomIPC] Current zoomFactor: ${currentZoom}`);

      if (args.deltaY < 0) { // Wheel "forward" / away from user -> Zoom In
        currentZoom += 0.1;
        console.log('[ZoomIPC] Attempting to Zoom In');
      } else if (args.deltaY > 0) { // Wheel "backward" / towards user -> Zoom Out
        currentZoom -= 0.1;
        console.log('[ZoomIPC] Attempting to Zoom Out');
      }

      currentZoom = Math.max(0.5, Math.min(currentZoom, 3.0)); // Clamp zoom
      console.log(`[ZoomIPC] Setting new zoomFactor: ${currentZoom}`);
      mainWindow.webContents.setZoomFactor(currentZoom);
      
      // Optional: Log actual zoom change
      // mainWindow.webContents.once('zoom-changed', (zoomChangeEvent, zoomDirection) => {
      //   console.log(`[ZoomIPC] Zoom actually changed. New factor: ${mainWindow.webContents.getZoomFactor()}, Direction: ${zoomDirection}`);
      // });
    }
  });

  mainWindow.on('closed', () => {
    mainWindow = null; 
  });
};

app.whenReady().then(async () => {
  await createWindow();

  // Listen for the 'service-ready' event from the main process (triggered by .NET service output)
  ipcMain.on('service-ready', async () => {
    console.log('[IPC] Received service-ready signal in main process listener.');
    if (mainWindow && !mainWindow.isDestroyed()) {
      if (targetUrl) {
        console.log(`[IPC] Service ready signal received. Loading main URL: ${targetUrl}`);
        try {
          await mainWindow.loadURL(targetUrl);
        } catch (error) {
          console.error(`Failed to load main URL ${targetUrl} after service ready:`, error);
          mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Failed to load main application after service started.</p><pre>${error.message}</pre>`);
        }
      } else {
        console.warn('[IPC] Service ready signal received, but targetUrl is not set. This should not happen.');
        mainWindow.loadURL(`data:text/html,<h1>Error</h1><p>Service ready, but target URL is missing.</p>`);
      }
    } else {
      console.warn('[IPC] Service ready signal received, but mainWindow is destroyed or null.');
    }
  });

  app.on('activate', async () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      await createWindow();
    } else if (mainWindow && !mainWindow.isDestroyed()) {
      mainWindow.focus();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('will-quit', () => {
  isAppQuitting = true; // Set the flag when app is about to quit
  if (mdServiceProcess && !mdServiceProcess.killed) {
    console.log('Attempting to kill MdExplorer.Service process...');
    mdServiceProcess.kill();
  }
});

process.on('SIGINT', () => {
  console.log('Received SIGINT. Quitting Electron app...');
  app.quit(); 
});
