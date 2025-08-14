---
author: Carlo Salaroglio
document_type: Document
email: salaroglio@hotmail.com
title: 
date: 13/08/2025
word_section:
  write_toc: false
  document_header: ''
  template_section:
    inherit_from_template: ''
    custom_template: ''
    template_type: default
  predefined_pages: 
---
# Windows Credential Store Target Analysis

## Come LibGit2Sharp cerca le credenziali

Dall'analisi del codice `WindowsCredentialStoreResolver.cs`, ecco esattamente come vengono costruiti i target names:

### URL dal log di errore:
```
https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test
```

### Come il nostro resolver costruisce il target (linea 124):
```csharp
var uri = new Uri(url);
return $"{CredentialTargetPrefix}{uri.Scheme}://{uri.Host}";
```

**Risultato**: `git:https://dbs-svn.dedagroup.it`

### Sequenza di ricerca del nostro resolver:

1. **Target primario**: `git:https://dbs-svn.dedagroup.it`
2. **Target fallback**: `git:https://github.com` (linea 68)

## Possibili problemi identificati:

### 1. **Porta mancante nel target**
- Il nostro target: `git:https://dbs-svn.dedagroup.it`
- Possibile target corretto: `git:https://dbs-svn.dedagroup.it:8443`

### 2. **Format del target diverso da Git nativo**
Git nativo potrebbe salvare credenziali con formati diversi:
- `https://dbs-svn.dedagroup.it:8443`
- `dbs-svn.dedagroup.it:8443`
- `git:https://dbs-svn.dedagroup.it:8443/scm/repo/BCCSi/bccsi-e2e-test`

### 3. **Tipo di credenziale**
Il nostro resolver cerca solo `CRED_TYPE.GENERIC`, ma Git nativo potrebbe usare:
- `CRED_TYPE.DOMAIN_PASSWORD`
- `CRED_TYPE.DOMAIN_VISIBLE_PASSWORD`

## Test da eseguire

### Script per testare target variations:
```powershell
# Test vari formati di target
$targets = @(
    "git:https://dbs-svn.dedagroup.it",
    "git:https://dbs-svn.dedagroup.it:8443", 
    "https://dbs-svn.dedagroup.it",
    "https://dbs-svn.dedagroup.it:8443",
    "dbs-svn.dedagroup.it",
    "dbs-svn.dedagroup.it:8443"
)

foreach ($target in $targets) {
    Write-Host "Testing target: $target"
    cmdkey /list:$target
}
```

### Verifica come Visual Studio Code salva le credenziali:
1. Cancellare tutte le credenziali esistenti per il dominio
2. Fare login con Visual Studio Code
3. Verificare che target viene creato
4. Confrontare con quello che cerca il nostro resolver