# MdExplorer — Docker (experimental, scratch)

This folder is **ignored by git** (`/docker/` is in the project `.gitignore`).
It exists as a personal sandbox to try running MdExplorer headless inside a
Linux container, with the user accessing it from the host browser.

## What this is — and what it isn't

- ✅ **Web-only build**: only the .NET backend (`MdExplorer.Service`) + Angular
  client. **No Electron**, no native window, no system clipboard, no
  embedded `external-app` iframes that rely on spawning host processes.
- ✅ **Headless**: the browser auto-launch in `Startup.cs:374` is suppressed
  via the existing `ELECTRON_RUN_AS_NODE=1` kill-switch (zero changes to the
  committed code).
- ✅ **Loopback-only on the host**: `127.0.0.1:5000:5000`. Reachable only by
  the browser running on the same machine, mirroring Electron's posture.
- ❌ **NOT a multi-user server**. MdE has no authentication. Do not expose
  the container on the LAN (`ports: 5000:5000`) until auth is added.

## Layout

```
docker/
├── Dockerfile                  # 3-stage build (Angular → .NET publish → runtime)
├── Dockerfile.dockerignore     # BuildKit-aware ignore, keeps repo root clean
├── docker-compose.yml          # the only invocation you should need
├── data/                       # bind-mounted as /data — persistent app data
├── workspace/                  # bind-mounted as /workspace — your .md files
└── README.md
```

## Prerequisites

- **Docker 23.0+** (for the BuildKit `<dockerfile>.dockerignore` lookup)
- On Windows: **Docker Desktop with WSL2 backend**
- ~3 GB free disk for the image (Pandoc + Java JRE + Graphviz pull weight)

## Build & run

From this folder:

```bash
docker compose build
docker compose up -d
# then open: http://127.0.0.1:5000/client2/index.html
```

### macOS / Linux — nothing to configure
Bash/zsh propagate `$PWD` to child processes, so the compose fallback
`${PWD}/workspace` resolves to the bind-mount source automatically. You can
delete the `.env` file in this folder.

### Windows — set the host workspace path
PowerShell and cmd do NOT propagate `$PWD` as an environment variable to
docker-compose, so the fallback would be empty. The `.env` file in this
folder hard-codes `MDE_HOST_WORKSPACE` to the current absolute path. **If you
move the `docker/` folder, edit `.env`** to match the new location. This
value is what's substituted back into the `vscode://file/...` URLs so the
host VS Code knows which file to open.

Stop with `docker compose down`. First build is slow (Angular `npm ci` +
`dotnet publish`); subsequent builds use the layer cache and are fast unless
you touch source code.

## How the container sees the host filesystem

| Host path                | Container path | Purpose |
|--------------------------|----------------|---------|
| `docker/workspace/`      | `/workspace`   | Your markdown project(s). Open them from MdE's "Open project" dialog as `/workspace/<your-folder>`. |
| `docker/data/`           | `/data`        | Persistent: UserDB (`MdExplorer.db`), EngineDB (`MdEngine_*.db`), llama model cache, etc. Survives image rebuilds. |

Inside the container `XDG_CONFIG_HOME=/data`, so anything the backend writes
under `Environment.SpecialFolder.ApplicationData` lands on the bind mount.

## Known limits / things to verify

### FileSystemWatcher across the WSL2 boundary
On Windows hosts, Docker Desktop runs the container in a Linux VM (WSL2).
Bind mounts from `C:\...` use the `9p` filesystem — **inotify events from
host edits do NOT reliably propagate into the container**. If you edit `.md`
files in VSCode on the host, MdE inside the container may not see the
change until something else (e.g. another save, a refresh from the UI)
triggers a re-read.

Workarounds, in order of preference:
1. Put `workspace/` on a Linux-native location (WSL2 distro itself) and bind
   from there — inotify works.
2. Add a polling fallback in `FileSystemWatcherManager` (open question for
   later — would need a config flag).
3. Live with it: in the worst case, hit refresh from the tree.

### LLamaSharp native binaries
The backend downloads CPU/GPU `libllama.so` on demand. We've installed
`libsqlite3-0` but not CUDA. **GPU embedding will not work**; CPU embedding
should work but is slower. To pin the model so it isn't re-downloaded on
every rebuild, point AI-Models at `/data/AI-Models` (TODO: check the
backend's lookup path).

### Things that simply won't work
- **External app embedding** (`.mdeapps.json` → iframe with `electronAPI`):
  the renderer is now the user's browser, there's no `preload.js`. Native
  apps spawned via IPC are unavailable.
- **System clipboard / paste-from-clipboard image editor**: cross-origin
  clipboard access in a plain browser works only via Clipboard API
  permissions and only over HTTPS in many cases. May need https-proxy.
- **Git authentication via Windows credential manager**: replaced with `gh`
  CLI or SSH inside the container if needed.

## GitHub Copilot CLI (in-app AI chat)

The image ships **Node.js 22 + the `@github/copilot` CLI** and the
**`MdExplorer.Mcp`** stdio bridge (self-contained, isolated in `/app/mcp`). The
backend spawns `copilot --acp` for the AI chat exactly as the Electron build
does; MdE writes `~/.copilot/mcp-config.json` so Copilot can call MdE's tools.

You still have to **authenticate** Copilot. Two ways:

### A. Headless token (recommended)
Copilot reads a token from `COPILOT_GITHUB_TOKEN`, then `GH_TOKEN`, then
`GITHUB_TOKEN` (in that order) and authenticates in the background — no browser,
no prompt.

1. Create a token on github.com:
   - **Fine-grained PAT** (Settings → Developer settings → Fine-grained tokens)
     with the **"Copilot Requests"** permission, **or**
   - a **classic PAT** with the **`user:email`** and **`read:org`** scopes.
   - Your account needs an active Copilot subscription.
2. Put it in `docker/.env`:
   ```
   COPILOT_GITHUB_TOKEN=github_pat_xxxxx…
   ```
3. `docker compose up -d`. The chat works immediately.

The token is passed straight to the CLI as an env var; it is **not** persisted
to disk inside the container.

### B. Interactive login (device flow)
Leave `COPILOT_GITHUB_TOKEN` blank and log in once:

```bash
docker compose exec mde copilot     # then type: /login
```

Follow the device-code URL on your host browser. The stored OAuth credentials
land in `~/.copilot`, which the container **symlinks onto `./data`** — so the
login survives `docker compose down`/`up` and image rebuilds. Re-run `/login`
only if you wipe `./data`.

> Precedence note: if `COPILOT_GITHUB_TOKEN`/`GH_TOKEN`/`GITHUB_TOKEN` is set, it
> silently overrides any stored `/login` credentials.

## The one source-tree patch we make

The Dockerfile runs:

```sh
sed -i 's|http://127\.0\.0\.1|http://0.0.0.0|g' MdExplorer/Program.cs
```

This rewrites the default Kestrel binding **inside the build stage only**.
The committed source is untouched. Without this patch, Docker would publish
port 5000 to a process that listens only on the container's loopback —
unreachable from the host.

The host-side mapping in `docker-compose.yml` (`127.0.0.1:5000:5000`) keeps
the service accessible only to localhost-on-the-host, regardless.

## Open punch list (where this is honest about being half-baked)

- [ ] First-run UX: opening a project at `/workspace/<dir>` works in
      principle, but the "open folder" picker may behave oddly in a browser
      vs Electron's native dialog. May need a server-side directory picker.
- [ ] Llama model paths: confirm they resolve under `XDG_CONFIG_HOME` and
      survive rebuilds without redownload.
- [ ] Health endpoint for `docker compose`'s `healthcheck:` block.
- [ ] Tag/version: pin `mdexplorer:dev` to something more descriptive.
- [ ] Try without `--legacy-peer-deps` once Angular is upgraded.
