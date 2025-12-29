# Piano: Fix Team Chat - Notifiche, Presenza e Counter Progetto

## Problemi da Risolvere

1. **Notifiche messaggi non funzionanti** - Icona mancante, notifiche falliscono silenziosamente
2. **Presenza non sincronizzata tra PC** - Utenti su PC diversi non si vedono nella lista online
3. **Counter progetto errato** - Il counter deve mostrare chi ha aperto QUEL progetto, non chi ha la chat aperta
4. **Notifiche Electron agent mode** - Nessuna notifica quando app minimizzata/background

## Chiarimento Logica

### Due concetti distinti:
- **Lista Presenza (utenti online)** = Chi ha MdExplorer attivo (anche agent mode/minimizzato) → può ricevere notifiche
- **Counter (numero accanto campanella)** = Chi ha aperto **quel specifico progetto Git**

### Architettura Attuale
- Ogni utente ha backend locale (.NET)
- Firebase Realtime Database per sync cross-PC
- SignalR per comunicazione frontend-backend
- SSE (Server-Sent Events) per ricevere messaggi da Firebase

**Problema chiave**:
1. `FirebaseStreamingService` sottoscrive solo `/messages.json`, non `/presence.json`
2. La presenza viene tracciata solo quando l'utente apre la CHAT, non quando apre il PROGETTO

---

## Fase 1: Fix Notifiche Messaggi

### 1.1 Creare icona notifica
**Creare**: `MdExplorer/client2/src/assets/icons/chat-notification.png`
- Copiare/adattare `ElectronMdExplorer/mdExplorer.png` (48x48 o 64x64)

### 1.2 Aggiungere error handling
**Modificare**: `MdExplorer/client2/src/app/git-chat/services/git-chat.service.ts`
- Aggiungere `notification.onerror` handler per debug

---

## Fase 2: Tracciamento Apertura Progetto (Counter)

Il counter deve mostrare quanti utenti hanno aperto lo stesso progetto Git.

### 2.1 Creare endpoint per registrare apertura progetto
**Modificare**: `MdExplorer/Controllers/GitChat/GitChatController.cs`

Nuovi endpoint:
```csharp
// POST /api/GitChat/project-opened
// Chiamato quando l'utente apre un progetto
[HttpPost("project-opened")]
public async Task<IActionResult> ProjectOpened([FromBody] ProjectOpenedDto dto)
{
    // dto contiene: repositoryPath, userInfo
    // 1. Calcola roomId dal remote URL
    // 2. Registra presenza in Firebase /chatRooms/{roomId}/projectUsers/{oderId}
    // 3. Avvia SSE subscription se primo utente
}

// POST /api/GitChat/project-closed
// Chiamato quando l'utente chiude il progetto
[HttpPost("project-closed")]
public async Task<IActionResult> ProjectClosed([FromBody] ProjectClosedDto dto)
```

### 2.2 Registrare apertura progetto nel frontend
**Modificare**: `MdExplorer/client2/src/app/md-explorer/services/projects.service.ts`

Quando l'utente apre un progetto:
```typescript
// In setCurrentProject() o metodo equivalente
async setCurrentProject(project: MdProject): Promise<void> {
    // ... codice esistente ...

    // Notifica backend che progetto è stato aperto
    await this.http.post('/api/GitChat/project-opened', {
        repositoryPath: project.path,
        userInfo: await this.getUserInfo()
    }).toPromise();
}
```

### 2.3 Aggiungere SSE per projectUsers
**Modificare**: `MdExplorer/Services/TeamChat/FirebaseStreamingService.cs`

```csharp
// Nuova subscription per /chatRooms/{roomId}/projectUsers.json
public async Task SubscribeToProjectUsers(string roomId)
public void UnsubscribeFromProjectUsers(string roomId)
private async Task ListenToFirebaseProjectUsersSSE(string roomId)
```

### 2.4 Esporre counter nel frontend
**Modificare**: `git-chat.service.ts`

```typescript
// Nuovo observable per il numero di utenti con progetto aperto
private _projectUsersCount$ = new BehaviorSubject<number>(0);
public projectUsersCount$ = this._projectUsersCount$.asObservable();
```

---

## Fase 3: Fix Presenza Chat Cross-PC

La lista utenti nella chat (chi può ricevere notifiche).

### 3.1 Aggiungere SSE subscription per presenza chat
**Modificare**: `MdExplorer/Services/TeamChat/FirebaseStreamingService.cs`

```csharp
// SSE listener su /chatRooms/{roomId}/presence.json
public async Task SubscribeToRoomPresence(string roomId)
private async Task ListenToFirebasePresenceSSE(string roomId)
```

### 3.2 Broadcast presenza remota
**Modificare**: `MdExplorer/Services/TeamChat/TeamChatService.cs`

```csharp
// Nuovo metodo chiamato da FirebaseStreamingService quando arriva update presenza
public void UpdateRemotePresence(string roomId, PresenceInfoDto presence)

// GetPresence unisce locale + remoto (deduplica per email)
```

---

## Fase 4: Notifiche Native Electron (Agent Mode)

### 4.1 Esporre API notifiche in preload
**Modificare**: `ElectronMdExplorer/preload.js`

```javascript
contextBridge.exposeInMainWorld('electronAPI', {
  // ... esistenti ...
  showNotification: (title, body, options) =>
    ipcRenderer.send('show-notification', { title, body, options }),
  isWindowFocused: () => ipcRenderer.invoke('is-window-focused')
});
```

### 4.2 Gestire IPC in main process
**Modificare**: `ElectronMdExplorer/index.js`

```javascript
// In registerIpcHandlers():
ipcMain.on('show-notification', (event, { title, body, options }) => {
  const notification = new Notification({
    title, body,
    icon: path.join(__dirname, 'mdExplorer.png'),
    silent: options?.silent
  });
  notification.on('click', () => {
    BrowserWindow.fromWebContents(event.sender)?.show()?.focus();
  });
  notification.show();
});

ipcMain.handle('is-window-focused', (event) => {
  return BrowserWindow.fromWebContents(event.sender)?.isFocused() ?? false;
});
```

### 4.3 Usare notifiche native in Angular
**Modificare**: `MdExplorer/client2/src/app/git-chat/services/git-chat.service.ts`

```typescript
private async notifyNewMessage(message: ChatMessage): Promise<void> {
  // ... controlli esistenti ...

  if (this.isElectron) {
    const api = (window as any).electronAPI;
    const isFocused = await api.isWindowFocused?.();

    if (!isFocused) {
      api.showNotification?.(
        'Team Chat - MdExplorer',
        `${message.senderName}: ${truncatedContent}`,
        { silent: !settings.soundEnabled }
      );
      return;
    }
  }

  // Fallback Web Notification per finestra focused
  // ... codice esistente ...
}
```

---

## File da Modificare

| File | Fase | Modifiche |
|------|------|-----------|
| `client2/src/assets/icons/chat-notification.png` | 1 | Creare |
| `client2/src/app/git-chat/services/git-chat.service.ts` | 1, 4 | Error handling, notifiche native, counter |
| `Controllers/GitChat/GitChatController.cs` | 2 | Endpoint project-opened/closed |
| `client2/src/app/md-explorer/services/projects.service.ts` | 2 | Chiamare project-opened |
| `Services/TeamChat/FirebaseStreamingService.cs` | 2, 3 | SSE projectUsers + presenza |
| `Services/TeamChat/TeamChatService.cs` | 2, 3 | Gestione projectUsers + merge presenza |
| `ElectronMdExplorer/preload.js` | 4 | API notifiche |
| `ElectronMdExplorer/index.js` | 4 | IPC handlers |

---

## Ordine Implementazione

1. **Fase 1** - Icona + error handling (quick win)
2. **Fase 2** - Counter apertura progetto (cambiamento architetturale)
3. **Fase 3** - Presenza chat cross-PC
4. **Fase 4** - Notifiche native Electron (agent mode)

## Test

- **Fase 1**: Aprire chat, ricevere messaggio, verificare notifica con icona
- **Fase 2**: 2 PC aprono stesso progetto Git, verificare counter incrementa su entrambi
- **Fase 3**: 2 PC con chat aperta, verificare lista utenti online sincronizzata
- **Fase 4**: Avviare con `--agent`, minimizzare, ricevere messaggio, verificare notifica tray
