# P2P Feature - End-to-End Testing Checklist

This document describes manual E2E tests for the P2P file sharing feature.

## Prerequisites

1. **Electron App Running**: MdExplorer must be running from Electron (not standalone .NET)
2. **P2P Addon Installed**: The p2p-plugin addon must be installed in `ElectronMdExplorer/addons/`
3. **P2P Enabled**: P2P sharing must be enabled in the tray menu
4. **Two Instances**: For transfer tests, you need two MdExplorer instances (can be on different PCs or VMs)

## Test Categories

### 1. P2P Service Availability

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 1.1 | Open Projects page | "P2P Sharing" card should appear in Quick Actions (green icon) |
| 1.2 | Click "P2P Sharing" card | P2P Manager dialog opens |
| 1.3 | Check stats bar | Shows download/upload speeds, peer count, active transfers |
| 1.4 | Disable P2P in tray menu, reload | "P2P Sharing" card should disappear |

### 2. Context Menu Integration

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 2.1 | Right-click on markdown file | Should see green "Share via P2P" button (cloud_upload icon) |
| 2.2 | Right-click on folder | Should see "Share folder via P2P" menu item |
| 2.3 | With P2P disabled, right-click | P2P share options should NOT appear |

### 3. File Sharing Flow

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 3.1 | Right-click file → "Share via P2P" | Snackbar shows "Creating P2P share..." |
| 3.2 | Wait for share to complete | Magnet link copied to clipboard, snackbar confirms |
| 3.3 | Check P2P Manager | New transfer appears in list with "seeding" status |
| 3.4 | Copy magnet link from transfer | Link copied to clipboard |

### 4. Folder Sharing Flow

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 4.1 | Right-click folder → "Share folder via P2P" | Snackbar shows "Creating P2P share..." |
| 4.2 | Wait for share to complete | Magnet link created for entire folder |
| 4.3 | Check file size in transfer list | Should show total folder size |

### 5. Download Flow

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 5.1 | Open P2P Manager → Download tab | Input field for magnet link visible |
| 5.2 | Paste valid magnet link | "Start Download" button enables |
| 5.3 | Click "Start Download" | Transfer starts, appears in Transfers tab |
| 5.4 | Watch progress | Progress bar updates, speed/ETA shown |
| 5.5 | Download completes | Snackbar notification, status changes to "completed" |

### 6. Transfer Controls

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 6.1 | Click pause button on active transfer | Transfer pauses, icon changes |
| 6.2 | Click resume button | Transfer resumes |
| 6.3 | Click stop button | Confirmation dialog appears |
| 6.4 | Confirm stop | Transfer removed from list |
| 6.5 | Stop with "delete files" option | Files deleted from disk |

### 7. Real-Time Updates (SignalR)

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 7.1 | Start download, keep dialog open | Progress bar updates in real-time |
| 7.2 | Download completes | Snackbar appears even if dialog closed |
| 7.3 | Transfer error occurs | Error snackbar with details |

### 8. Cross-Instance Transfer

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 8.1 | Instance A: Share a file | Get magnet link |
| 8.2 | Instance B: Download using magnet | Transfer starts |
| 8.3 | Both instances: Check peer count | Should show 1+ peers |
| 8.4 | Instance A: Check upload speed | Should show active upload |
| 8.5 | Instance B: Download completes | File matches original |

### 9. Error Handling

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 9.1 | Try to share non-existent file | Error message displayed |
| 9.2 | Enter invalid magnet link | "Invalid magnet URI" error |
| 9.3 | Stop P2P service mid-transfer | Graceful error, transfer paused |
| 9.4 | Network disconnect during transfer | Transfer pauses, resumes on reconnect |

### 10. GitIgnore Integration

| Test | Steps | Expected Result |
|------|-------|-----------------|
| 10.1 | Create new project with Git | .gitignore contains `.p2pshare/` |
| 10.2 | Download file via P2P | File goes to `.p2pshare/received/` |
| 10.3 | Run `git status` | P2P files not shown as untracked |

## Performance Tests

| Test | Expected |
|------|----------|
| Share 100MB file | < 5 seconds to create torrent |
| Share 1GB folder | < 30 seconds to create torrent |
| Transfer speed (LAN) | > 10 MB/s |
| Transfer speed (Internet) | Depends on connection |

## Regression Tests

After each change, verify:
- [ ] P2P card visibility works correctly
- [ ] Context menu shows/hides properly
- [ ] Existing MdExplorer features still work
- [ ] No console errors related to P2P

## Test Environment Setup

### Single Machine (Development)

```bash
# Terminal 1: Start .NET backend
cd MdExplorer
dotnet run

# Terminal 2: Start Electron
cd ElectronMdExplorer
npm start
```

### Two Machines (Full E2E)

1. Install MdExplorer on both machines
2. Ensure both are on same network (or use tracker server)
3. Run tests 8.1-8.5 between machines

## Troubleshooting

| Issue | Solution |
|-------|----------|
| P2P card not appearing | Check tray menu, ensure P2P enabled |
| "Service not available" | Electron P2P plugin not running |
| Transfers stuck at 0% | No peers found, check tracker |
| SignalR errors | Check browser console, restart app |

## Known Limitations

1. P2P only works in Electron (not standalone browser)
2. Requires tracker server for cross-network transfers
3. Large folders (>10GB) may take time to hash
