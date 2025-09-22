// MdExplorer/ElectronMdExplorer/index.js
const { app, BrowserWindow, ipcMain, Tray, Menu, nativeImage } = require('electron'); // Added ipcMain, Tray, Menu, nativeImage
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
// Function to create system tray
const createTray = () => {
  // Use the same icon as the app
  const iconPath = path.join(__dirname, 'mdExplorer.png');
  const trayIcon = nativeImage.createFromPath(iconPath);

  // Resize icon for tray (16x16 or 32x32 is typical for tray icons)
  const resizedIcon = trayIcon.resize({ width: 16, height: 16 });

  tray = new Tray(resizedIcon);
  tray.setToolTip('MdExplorer');

  const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Apri nuova finestra MdExplorer',
      click: () => {
        createNewWindow();
      }
    },
    {
      label: mainWindow && mainWindow.isVisible() ? 'Nascondi finestra principale' : 'Mostra finestra principale',
      click: () => {
        if (mainWindow) {
          if (mainWindow.isVisible()) {
            mainWindow.hide();
          } else {
            mainWindow.show();
            mainWindow.focus();
          }
          // Update menu
          updateTrayMenu();
        }
      }
    },
    { type: 'separator' },
    {
      label: 'Informazioni',
      click: () => {
        const { dialog } = require('electron');
        const packageJson = require('./package.json');
        const serviceStatus = mdServiceProcess && !mdServiceProcess.killed ? 'Attivo' : 'Non attivo';
        const activeWindows = windows.filter(w => !w.isDestroyed()).length;
        dialog.showMessageBox({
          type: 'info',
          title: 'MdExplorer',
          message: `MdExplorer v${packageJson.version}`,
          detail: `Stato servizio: ${serviceStatus}\nFinestre attive: ${activeWindows}\nURL: ${targetUrl || 'Non disponibile'}`,
          buttons: ['OK']
        });
      }
    },
    { type: 'separator' },
    {
      label: 'Chiudi tutto e esci',
      click: () => {
        forceQuit = true;
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);

  // Double click on tray icon shows/hides main window
  tray.on('double-click', () => {
    if (mainWindow) {
      if (mainWindow.isVisible()) {
        mainWindow.hide();
      } else {
        mainWindow.show();
        mainWindow.focus();
      }
    }
  });
};

// Function to update tray menu (to update show/hide label)
const updateTrayMenu = () => {
  if (!tray) return;

  const contextMenu = Menu.buildFromTemplate([
    {
      label: 'Apri nuova finestra MdExplorer',
      click: () => {
        createNewWindow();
      }
    },
    {
      label: mainWindow && mainWindow.isVisible() ? 'Nascondi finestra principale' : 'Mostra finestra principale',
      click: () => {
        if (mainWindow) {
          if (mainWindow.isVisible()) {
            mainWindow.hide();
          } else {
            mainWindow.show();
            mainWindow.focus();
          }
          updateTrayMenu();
        }
      }
    },
    { type: 'separator' },
    {
      label: 'Informazioni',
      click: () => {
        const { dialog } = require('electron');
        const packageJson = require('./package.json');
        const serviceStatus = mdServiceProcess && !mdServiceProcess.killed ? 'Attivo' : 'Non attivo';
        const activeWindows = windows.filter(w => !w.isDestroyed()).length;
        dialog.showMessageBox({
          type: 'info',
          title: 'MdExplorer',
          message: `MdExplorer v${packageJson.version}`,
          detail: `Stato servizio: ${serviceStatus}\nFinestre attive: ${activeWindows}\nURL: ${targetUrl || 'Non disponibile'}`,
          buttons: ['OK']
        });
      }
    },
    { type: 'separator' },
    {
      label: 'Chiudi tutto e esci',
      click: () => {
        forceQuit = true;
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);
};

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
let windows = []; // Keep track of all windows
let tray = null; // System tray instance
let mdServiceProcess; // Keep a reference to the spawned service process
let isAppQuitting = false; // Flag to track if the app is quitting
let forceQuit = false; // Flag to track if we should really quit
let targetUrl; // Moved targetUrl to global scope

// Function to set up window-specific handlers
const setupWindowHandlers = (window) => {
  // Listen for F12 to toggle DevTools
  window.webContents.on('before-input-event', (event, input) => {
    if (input.key === 'F12' && !input.alt && !input.control && !input.meta && !input.shift) {
      console.log('[DevTools] F12 pressed, toggling dev tools');
      window.webContents.toggleDevTools();
      event.preventDefault();
    }
  });
};

// Function to create a new window (for multi-window support)
const createNewWindow = async () => {
  if (!targetUrl) {
    console.warn('Cannot create new window: targetUrl not set');
    return;
  }

  const newWindow = new BrowserWindow({
    width: 1024,
    height: 768,
    frame: false,
    icon: path.join(__dirname, 'mdExplorer.png'),
    autoHideMenuBar: true,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
      preload: path.join(__dirname, 'preload.js')
    }
  });

  windows.push(newWindow);

  await newWindow.loadURL(targetUrl);

  // Set up window-specific event handlers
  setupWindowHandlers(newWindow);

  newWindow.on('closed', () => {
    // Remove from windows array
    const index = windows.indexOf(newWindow);
    if (index > -1) {
      windows.splice(index, 1);
    }
    updateTrayMenu();
  });

  return newWindow;
};

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

  // Add main window to windows array
  windows.push(mainWindow);

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
  ipcMain.on('minimize-window', (event) => {
    const window = BrowserWindow.fromWebContents(event.sender);
    if (window && !window.isDestroyed()) {
      window.minimize();
    }
  });

  ipcMain.on('maximize-window', (event) => {
    const window = BrowserWindow.fromWebContents(event.sender);
    if (window && !window.isDestroyed()) {
      if (window.isMaximized()) {
        window.unmaximize();
      } else {
        window.maximize();
      }
    }
  });

  ipcMain.on('close-window', (event) => {
    // Find which window sent this event
    const window = BrowserWindow.fromWebContents(event.sender);
    if (window) {
      // If it's the main window and we have tray, just hide it
      if (window === mainWindow && tray && !forceQuit) {
        window.hide();
        updateTrayMenu();
      } else {
        // For other windows or when forceQuit is true, actually close
        window.close();
      }
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

  // Handle close event - minimize to tray instead of closing
  mainWindow.on('close', (event) => {
    if (!forceQuit && !isAppQuitting) {
      event.preventDefault();
      mainWindow.hide();
      updateTrayMenu();

      // Show notification that app is in tray (first time only)
      if (tray && !mainWindow.wasMinimizedToTrayBefore) {
        const { Notification } = require('electron');
        if (Notification.isSupported()) {
          const notification = new Notification({
            title: 'MdExplorer',
            body: 'L\'applicazione è stata minimizzata nella system tray',
            icon: path.join(__dirname, 'mdExplorer.png')
          });
          notification.show();
          mainWindow.wasMinimizedToTrayBefore = true;
        }
      }
    }
  });

  mainWindow.on('closed', () => {
    // Remove from windows array
    const index = windows.indexOf(mainWindow);
    if (index > -1) {
      windows.splice(index, 1);
    }
    mainWindow = null;
    updateTrayMenu();
  });
};

// Request single instance lock
const gotTheLock = app.requestSingleInstanceLock();

if (!gotTheLock) {
  // Another instance is already running, quit this one
  app.quit();
} else {
  // Handle second instance attempt
  app.on('second-instance', (event, commandLine, workingDirectory) => {
    // Someone tried to run a second instance, we should focus our window instead
    if (mainWindow) {
      if (mainWindow.isMinimized() || !mainWindow.isVisible()) {
        mainWindow.show();
      }
      mainWindow.focus();
    } else {
      // If no main window exists, create one
      createWindow();
    }
  });

  app.whenReady().then(async () => {
    // Create system tray
    createTray();

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
  // Don't quit when all windows are closed if we have a tray
  if (!tray || forceQuit) {
    if (process.platform !== 'darwin') {
      app.quit();
    }
  }
  // If we have a tray, the app keeps running
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
} // End of single instance lock else block
