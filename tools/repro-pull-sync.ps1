# ─────────────────────────────────────────────────────────────────────────────
# Harness di repro per la sync md-tree ↔ filesystem su git pull
# (vedi docs-internal/md-tree-evolution2/passo-pull-sync-overhaul.md, Fase G)
#
# Crea in C:\temp\mde-pull-repro:
#   remote.git   — bare repo che fa da "origin"
#   editor\      — clone di lavoro "dell'altro collega" (dove si committa)
#   workspace\   — clone da aprire in MdExplorer (dove si fa Pull dalla UI)
#
# Uso:
#   .\repro-pull-sync.ps1 -Init            # crea tutto da zero
#   .\repro-pull-sync.ps1 -PushBurst 30    # committa 30 file nuovi sul remote
#   .\repro-pull-sync.ps1 -PushMixed       # mix: nuovi + modifiche + delete + rename + cartelle annidate
#   .\repro-pull-sync.ps1 -TouchOpenDoc    # modifica docs\index.md (da tenere aperto in MDE)
#
# Procedura di verifica (criteri G2 del piano):
#   1. -Init, poi apri workspace\ come progetto in MdExplorer, espandi qualche cartella
#   2. -PushBurst 30 → premi Pull nella UI
#      ATTESO: albero aggiornato senza uscire/rientrare, expansion preservata,
#              nessun nodo duplicato (criteri 1-2)
#   3. Ripeti pull 5 volte + esci/rientra 5 volte → in console i log
#      "[SignalR] markdownFileCreated" devono restare 1 per evento (criterio 3)
#   4. Sporca il working tree di workspace\ (edita un file senza committare),
#      -PushBurst 10 → Pull → stesso comportamento (criterio 4)
#   5. -PushBurst 200 subito dopo l'apertura del progetto (indicizzazione in corso)
#      → Pull → nei log backend: una sola pipeline attiva, la precedente CANCELLED (criterio 5)
#   6. Cambia branch dalla UI tra master e un branch divergente → stesso contratto (criterio 6)
#   7. Apri docs\index.md in MDE, -TouchOpenDoc → Pull → il documento si ricarica (criterio 7)
# ─────────────────────────────────────────────────────────────────────────────
param(
    [switch]$Init,
    [int]$PushBurst = 0,
    [switch]$PushMixed,
    [switch]$TouchOpenDoc
)

$ErrorActionPreference = 'Stop'
$root      = 'C:\temp\mde-pull-repro'
$remote    = Join-Path $root 'remote.git'
$editor    = Join-Path $root 'editor'
$workspace = Join-Path $root 'workspace'

function Commit-And-Push([string]$message) {
    Push-Location $editor
    git add -A | Out-Null
    git commit -m $message | Out-Null
    git push origin master | Out-Null
    Pop-Location
    Write-Host "✅ pushed: $message" -ForegroundColor Green
}

if ($Init) {
    if (Test-Path $root) { Remove-Item -Recurse -Force $root -Confirm:$false }
    New-Item -ItemType Directory -Force $root | Out-Null

    # NB: niente redirect `2>$null` sui comandi git — con ErrorActionPreference=Stop
    # PowerShell trasforma lo stderr informativo di git in errore terminante.
    git init --bare $remote | Out-Null
    git clone $remote $editor

    Push-Location $editor
    git checkout -b master
    New-Item -ItemType Directory -Force (Join-Path $editor 'docs') | Out-Null
    Set-Content (Join-Path $editor 'README.md') "# Repro pull-sync`n"
    Set-Content (Join-Path $editor 'docs\index.md') "# Index`n`nDocumento da tenere aperto durante i test.`n"
    1..5 | ForEach-Object { Set-Content (Join-Path $editor "docs\doc$_.md") "# Doc $_`n" }
    Pop-Location
    Commit-And-Push 'init: struttura base'

    git clone $remote $workspace
    Write-Host "🏁 Pronto. Apri come progetto MdExplorer: $workspace" -ForegroundColor Cyan
    return
}

if ($PushBurst -gt 0) {
    $stamp = Get-Date -Format 'HHmmss'
    $dir = Join-Path $editor "burst-$stamp\nested\deep"
    New-Item -ItemType Directory -Force $dir | Out-Null
    1..$PushBurst | ForEach-Object {
        Set-Content (Join-Path $dir "file$_.md") "# Burst $stamp file $_`n"
    }
    Commit-And-Push "burst: $PushBurst file in burst-$stamp/nested/deep"
    return
}

if ($PushMixed) {
    $stamp = Get-Date -Format 'HHmmss'
    # nuovi in cartella annidata nuova
    $newDir = Join-Path $editor "mixed-$stamp\a\b"
    New-Item -ItemType Directory -Force $newDir | Out-Null
    1..3 | ForEach-Object { Set-Content (Join-Path $newDir "new$_.md") "# New $_`n" }
    # modifica
    Add-Content (Join-Path $editor 'docs\doc1.md') "`nModificato alle $stamp"
    # delete
    $toDelete = Join-Path $editor 'docs\doc2.md'
    if (Test-Path $toDelete) { Remove-Item $toDelete -Confirm:$false }
    # rename
    $toRename = Join-Path $editor 'docs\doc3.md'
    if (Test-Path $toRename) {
        Push-Location $editor
        git mv 'docs/doc3.md' "docs/doc3-renamed-$stamp.md" | Out-Null
        Pop-Location
    }
    Commit-And-Push "mixed: nuovi+modifica+delete+rename ($stamp)"
    return
}

if ($TouchOpenDoc) {
    Add-Content (Join-Path $editor 'docs\index.md') "`nRiga aggiunta alle $(Get-Date -Format 'HH:mm:ss') — se MDE lo mostra dopo la pull, il criterio 7 passa."
    Commit-And-Push 'touch: docs/index.md (documento aperto)'
    return
}

Write-Host "Nessuna azione. Usa -Init, -PushBurst N, -PushMixed o -TouchOpenDoc." -ForegroundColor Yellow