# syntax=docker/dockerfile:1.6
# MdExplorer — headless server (no Electron, no native browser launch)
# Build context MUST be the repo root (parent of this folder).
# See docker-compose.yml for the recommended invocation.

# ----------------------------------------------------------------------------
# Stage 1 — Angular client build
# Node 16 because the legacy build script targets it (see project memory).
# ----------------------------------------------------------------------------
FROM node:16-bullseye AS angular-build
WORKDIR /src/MdExplorer/client2

# Dependency layer (cached unless package*.json changes)
COPY MdExplorer/client2/package.json MdExplorer/client2/package-lock.json ./
RUN npm ci --legacy-peer-deps

# Sources + build (output lands in /src/MdExplorer/wwwroot/client2/ per angular.json)
COPY MdExplorer/client2/ ./
ENV NODE_OPTIONS=--max-old-space-size=4096
RUN node update-version.js && npx ng build --base-href /client2/

# ----------------------------------------------------------------------------
# Stage 2 — .NET backend build & publish
# Linux target — MdExplorer.Service.csproj already has a Linux conditional
# PropertyGroup; on non-Windows it builds as net8.0 (no WindowsForms).
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS dotnet-build
WORKDIR /src

# Copy all referenced project folders. We pull them individually so the
# Docker layer cache invalidates only on the projects that actually changed.
COPY Ad.Tools.Dal.Abstractions/ ./Ad.Tools.Dal.Abstractions/
COPY Ad.Tools.Dal/ ./Ad.Tools.Dal/
COPY Ad.Tools.FluentMigrator/ ./Ad.Tools.FluentMigrator/
COPY MdExplorer.Abstractions/ ./MdExplorer.Abstractions/
COPY MdExplorer.bll/ ./MdExplorer.bll/
COPY MDExplorer.dal/ ./MDExplorer.dal/
COPY MdExplorer.DataAccess.Engine/ ./MdExplorer.DataAccess.Engine/
COPY MdExplorer.DataAccess.Project/ ./MdExplorer.DataAccess.Project/
COPY MdExplorer.Migrations.EngineDb/ ./MdExplorer.Migrations.EngineDb/
COPY MdExplorer.Migrations.ProjectDb/ ./MdExplorer.Migrations.ProjectDb/
COPY MdExplorer.Migrations/ ./MdExplorer.Migrations/
COPY MdExplorer/ ./MdExplorer/

# MCP stdio server (Copilot ↔ MdExplorer bridge). It's a SEPARATE project with
# no ProjectReference from the Service, but the Service csproj's PostPublish
# target publishes it self-contained into ../ElectronMdExplorer/service_payload
# during `dotnet publish` and FAILS the whole publish if the sources aren't in
# the build context. So it must be copied here.
COPY MdExplorer.Mcp/ ./MdExplorer.Mcp/

# Scheduler satellite (cron triggers for *.agent.md while the Service is off).
# Same PostPublish pattern as MdExplorer.Mcp: no ProjectReference, but the
# Service csproj publishes it during `dotnet publish` and fails loudly if the
# sources are missing from the build context. Not used at runtime in Docker
# (the Service is always on here), but required for the publish to succeed.
COPY MdExplorer.Scheduler/ ./MdExplorer.Scheduler/

# Drop the prebuilt Angular bundle into the backend's wwwroot before publish
# so it's picked up as static content by UseStaticFiles().
RUN rm -rf /src/MdExplorer/wwwroot/client2
COPY --from=angular-build /src/MdExplorer/wwwroot/client2/ /src/MdExplorer/wwwroot/client2/

# Docker-only patch: bind on 0.0.0.0 inside the container. The host-side
# mapping in docker-compose.yml restricts exposure to 127.0.0.1, so this
# does NOT make the service reachable from the LAN.
RUN sed -i 's|http://127\.0\.0\.1|http://0.0.0.0|g' MdExplorer/Program.cs

# Docker-only patch: System.Drawing.{Icon,Bitmap,Graphics,...} are used by
# MdAppStoreController for Windows-only icon extraction. The calls are
# already gated by RuntimeInformation.IsOSPlatform(OSPlatform.Windows), so
# they never run on Linux — but net8.0 Linux can't even *compile* without
# the package because System.Drawing.Common isn't a transitive dep on Linux.
# We add it only inside the build stage; the committed csproj is untouched.
RUN sed -i '/<PackageReference Include="YamlDotNet"/a\    <PackageReference Include="System.Drawing.Common" Version="8.0.0" \/>' MdExplorer/MdExplorer.Service.csproj

# MdEngine.db and MdExplorer.db are listed as <EmbeddedResource> in the csproj
# but live in *.db (gitignored), so they don't reach the build context. Create
# empty placeholders — at runtime, FluentMigrator generates schemas from
# scratch (same code path as "user opens a brand-new project").
RUN touch /src/MdExplorer/MdEngine.db /src/MdExplorer/MdExplorer.db

WORKDIR /src/MdExplorer
RUN dotnet publish MdExplorer.Service.csproj \
        -c Release \
        -r linux-x64 \
        --no-self-contained \
        /p:UseAppHost=false \
        -o /app/publish

# ----------------------------------------------------------------------------
# Stage 3 — Runtime
# Installs MdExplorer runtime deps: pandoc (export), graphviz (PlantUML
# fallback), java (PlantUML jar), fonts (Pandoc/PDF), libsqlite3 (NHibernate).
# ----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime

RUN apt-get update && apt-get install -y --no-install-recommends \
        pandoc \
        graphviz \
        default-jre-headless \
        fontconfig \
        fonts-dejavu \
        fonts-liberation \
        libsqlite3-0 \
        curl \
        ca-certificates \
        locales \
        git \
        openssh-client \
    && sed -i 's/^# *\(en_US.UTF-8\)/\1/' /etc/locale.gen \
    && locale-gen \
    && rm -rf /var/lib/apt/lists/*

ENV LANG=en_US.UTF-8 LC_ALL=en_US.UTF-8

# ----------------------------------------------------------------------------
# GitHub Copilot CLI — the backend spawns `copilot --acp` (see
# CopilotAcpSession / CopilotProcessLauncher) to power the in-app AI chat.
# On Linux the launcher resolves a bare "copilot" via PATH, so the CLI must be
# installed globally. It's a Node package (@github/copilot) and requires
# Node.js 22+, so we pull the NodeSource 22.x repo first.
# ----------------------------------------------------------------------------
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get update && apt-get install -y --no-install-recommends nodejs \
    && npm install -g @github/copilot \
    && npm cache clean --force \
    && rm -rf /var/lib/apt/lists/*

# LibGit2Sharp reads user.name / user.email from ~/.gitconfig (or system).
# Without a default identity, GitService.GetCurrentUser does .First() on an
# empty config sequence and throws 500 on any code path that touches it
# (Create new .md file, commit, etc.). Provide a sensible default — the user
# can override later through MdE's git settings UI.
RUN printf '[user]\n\temail = mdexplorer@local\n\tname = MdExplorer\n[safe]\n\tdirectory = *\n' > /root/.gitconfig

# Headless container — these env vars short-circuit Electron-related code paths:
#   - skip auto browser launch (Startup.cs OpenUrl)
#   - skip file logging (Program.cs) — logs go to stdout, captured by `docker logs`
ENV ELECTRON_RUN_AS_NODE=1

# Redirect Environment.SpecialFolder.ApplicationData (which on Linux maps to
# $XDG_CONFIG_HOME, defaulting to ~/.config) onto a persistent volume.
# This is where the Service writes port.txt and where the MCP server looks for it.
ENV XDG_CONFIG_HOME=/data

# ...and the OTHER data root: CrossPlatformPath.GetAppDataDirectory() reads
# $XDG_DATA_HOME (falling back to ~/.local/share), NOT $XDG_CONFIG_HOME. Without
# this line the UserDB (MdExplorer.db), the engine DBs and the Data Protection
# keys landed in /root/.local/share/MdExplorer -- INSIDE the container layer, so
# `docker compose down && up` silently threw away the project list and, worse,
# the keys that decrypt the stored Atlassian API token. Both roots now resolve to
# /data/MdExplorer, on the volume.
ENV XDG_DATA_HOME=/data

WORKDIR /app
COPY --from=dotnet-build /app/publish/ ./

# --- MdExplorer MCP stdio server (isolated) ---------------------------------
# Published self-contained into its OWN folder so its assembly closure (newer
# System.Text.Json / Microsoft.Extensions.Hosting) can never collide with the
# framework-dependent Service that lives directly in /app. The Service's
# PostPublish target emitted it into service_payload during the build stage.
#
# ProjectsManager.CreateCopilotCliMcpConfig writes ~/.copilot/mcp-config.json
# pointing Copilot at "<AppContext.BaseDirectory>/MdExplorer.Mcp" (i.e.
# /app/MdExplorer.Mcp). We satisfy that lookup with a tiny wrapper that execs
# the real self-contained binary in /app/mcp — keeping the two closures apart.
#
# The Service's PostPublish target now emits the MCP into the isolated
# service_payload/mcp subfolder (McpPublishDest, see MdExplorer.Service.csproj),
# so we copy THAT subfolder — not the whole service_payload — straight into
# /app/mcp (otherwise the binary would land at /app/mcp/mcp/MdExplorer.Mcp).
COPY --from=dotnet-build /src/ElectronMdExplorer/service_payload/mcp/ /app/mcp/
RUN printf '#!/bin/sh\nexec /app/mcp/MdExplorer.Mcp "$@"\n' > /app/MdExplorer.Mcp \
    && chmod +x /app/MdExplorer.Mcp /app/mcp/MdExplorer.Mcp

RUN mkdir -p /data /workspace
VOLUME ["/data", "/workspace"]

EXPOSE 5000

# Entry point: pin ~/.copilot onto the persistent /data volume BEFORE the
# backend starts, so (a) the MCP config MdE writes there and (b) Copilot CLI
# credentials from an interactive `/login` both survive container recreation.
# HOME stays /root (keeps /root/.gitconfig identity), we just symlink the dir.
# First positional arg = port; combined with the sed patch above the host binds
# to 0.0.0.0:5000 inside the container.
RUN printf '%s\n' \
    '#!/bin/sh' \
    'set -e' \
    'mkdir -p /data/copilot' \
    'ln -sfn /data/copilot /root/.copilot' \
    'exec dotnet MdExplorer.Service.dll "$@"' \
    > /usr/local/bin/mde-entrypoint.sh \
    && chmod +x /usr/local/bin/mde-entrypoint.sh

ENTRYPOINT ["/usr/local/bin/mde-entrypoint.sh"]
CMD ["5000"]
