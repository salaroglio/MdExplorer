const { contextBridge, ipcRenderer } = require('electron');

// It's good practice to use contextBridge to expose APIs to the renderer,
// but for this specific task of sending an event, it's not strictly necessary
// to expose anything if the preload script handles the event listener itself.
contextBridge.exposeInMainWorld('electronAPI', {
  sendZoomEvent: (args) => ipcRenderer.send('zoom-mouse-wheel', args),
  onServiceOutput: (callback) => ipcRenderer.on('service-output', (event, ...args) => callback(...args)),
  onServiceReady: (callback) => ipcRenderer.on('service-ready', (event, ...args) => callback(...args))
});

window.addEventListener('wheel', (event) => {
  if (event.ctrlKey) {
    // Prevent the default scroll action in the renderer when Ctrl is pressed.
    // This is important to stop the page from scrolling.
    event.preventDefault(); 
    
    // Send the deltaY (scroll direction) to the main process.
    ipcRenderer.send('zoom-mouse-wheel', { deltaY: event.deltaY });
    // console.log(`[Preload] Ctrl+Wheel detected, deltaY: ${event.deltaY}`); // For debugging in renderer console
  }
}, { passive: false }); // 'passive: false' is required for 'preventDefault()' to work.
