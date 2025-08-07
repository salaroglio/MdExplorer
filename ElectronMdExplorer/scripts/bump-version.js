const fs = require('fs');
const path = require('path');

// Leggi il package.json
const packagePath = path.join(__dirname, '..', 'package.json');
const packageJson = JSON.parse(fs.readFileSync(packagePath, 'utf8'));

// Estrai la versione corrente
const currentVersion = packageJson.version;
const versionParts = currentVersion.split('.');

// Incrementa la patch version (ultimo numero)
versionParts[2] = (parseInt(versionParts[2]) + 1).toString();
const newVersion = versionParts.join('.');

// Aggiorna il package.json
packageJson.version = newVersion;
fs.writeFileSync(packagePath, JSON.stringify(packageJson, null, 2) + '\n');

console.log(`Version bumped from ${currentVersion} to ${newVersion}`);