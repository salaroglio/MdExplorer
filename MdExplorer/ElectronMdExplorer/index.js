// MdExplorer/ElectronMdExplorer/index.js
console.log("Hello from Electron - Modified");

const { app, BrowserWindow, ipcMain } = require('electron'); // Added ipcMain
const path = require('path');
const net = require('net');
const { spawn } = require('child_process');
const fs = require('fs'); // Added fs module

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

const createWindow = async () => {
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.focus();
    return;
  }

  mainWindow = new BrowserWindow({
    width: 1024,
    height: 768,
    icon: path.join(__dirname, 'assets/icons/IconReady.png'), // Ensure this icon exists in assets/icons/
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
  let targetUrl;

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

      // Determine the path to MdExplorer.Service.exe
      const tfmsToTry = [ 'netcoreapp3.1']; // Common TFMs
      let serviceExecutablePath = '';
      let serviceWorkingDirectory = '';
      const absoluteSearchedPaths = []; // Array to store all absolute paths checked

      for (const tfm of tfmsToTry) {
        const potentialPath = path.join(__dirname,'..','..', '..', '..',  '..', 'bin', 'Release', tfm,'win10-x64' ,'MdExplorer.Service.exe');
        absoluteSearchedPaths.push(potentialPath); // Store each absolute path tried
        if (fs.existsSync(potentialPath)) {
          serviceExecutablePath = potentialPath;
          serviceWorkingDirectory = path.dirname(serviceExecutablePath);
          console.log(`Found MdExplorer.Service.exe at: ${serviceExecutablePath}`);
          break;
        }
      }

      if (!serviceExecutablePath) {
        const searchedPathsForDisplayHTML = absoluteSearchedPaths.join('<br>');
        const searchedPathsForConsole = absoluteSearchedPaths.join('\n');
        console.error(`MdExplorer.Service.exe not found. Searched absolute paths:\n${searchedPathsForConsole}`);
        if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.loadURL(`data:text/html,<h1>Service Error</h1><p>MdExplorer.Service.exe not found. Please ensure it is compiled in a standard Release output folder (e.g., bin/Release/netX.X/winY-Z/ relative to MdExplorer.Service.csproj).</p><pre>Searched absolute paths:<br>${searchedPathsForDisplayHTML}</pre>`);
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

      mdServiceProcess.stdout.on('data', (data) => {
        console.log(`MdExplorer.Service stdout: ${data.toString().trim()}`);
      });

      mdServiceProcess.stderr.on('data', (data) => {
        console.error(`MdExplorer.Service stderr: ${data.toString().trim()}`);
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
      
      console.log(`Waiting a few seconds for the service to initialize...`);
      await new Promise(resolve => setTimeout(resolve, 5000)); // 5 seconds

      console.log(`Loading URL in Electron: ${targetUrl}`);
      await mainWindow.loadURL(targetUrl);

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
