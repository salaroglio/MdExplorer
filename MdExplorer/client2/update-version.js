const fs = require('fs');
const path = require('path');

const packageJsonPath = path.join(__dirname, 'package.json');
const versionFilePath = path.join(__dirname, 'src', 'environments', 'version.ts');
const environmentsDir = path.dirname(versionFilePath);

// Assicurati che la directory environments esista
if (!fs.existsSync(environmentsDir)) {
    fs.mkdirSync(environmentsDir, { recursive: true });
}

// Leggi package.json
let packageJson;
try {
    packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
} catch (error) {
    console.error(`Errore nella lettura di ${packageJsonPath}:`, error);
    process.exit(1);
}

const currentVersion = packageJson.version || '0.0.0.0';
const versionParts = currentVersion.split('.');

// Ottieni la data corrente nel formato YYYY.MM.DD
const today = new Date();
const year = today.getFullYear();
const month = String(today.getMonth() + 1).padStart(2, '0'); // Mesi da 0 a 11
const day = String(today.getDate()).padStart(2, '0');
const todayDateString = `${year}.${month}.${day}`;

// Ottieni timestamp completo per build time
const hours = String(today.getHours()).padStart(2, '0');
const minutes = String(today.getMinutes()).padStart(2, '0');
const seconds = String(today.getSeconds()).padStart(2, '0');
const buildTimestamp = `${todayDateString} ${hours}:${minutes}:${seconds}`;

let newVersion;
let iteration = 1;

// Controlla se la versione attuale è valida e corrisponde alla data odierna
if (versionParts.length === 4 && `${versionParts[0]}.${versionParts[1]}.${versionParts[2]}` === todayDateString) {
    // Se la data è la stessa, incrementa l'iterazione
    iteration = parseInt(versionParts[3], 10) + 1;
    if (isNaN(iteration)) {
        console.warn(`Iterazione non valida in ${currentVersion}. Resetto a 1.`);
        iteration = 1;
    }
} else {
    // Se la data è diversa o la versione non è valida, inizia una nuova versione per oggi
    iteration = 1;
}

newVersion = `${todayDateString}.${iteration}`;

console.log(`Aggiornamento versione a: ${newVersion}`);

// Aggiorna package.json
packageJson.version = newVersion;
try {
    fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2) + '\n', 'utf8');
    console.log(`${packageJsonPath} aggiornato.`);
} catch (error) {
    console.error(`Errore nella scrittura di ${packageJsonPath}:`, error);
    process.exit(1);
}

// Genera src/environments/version.ts
const versionFileContent = `// Questo file è generato automaticamente dallo script update-version.js
// Non modificarlo manualmente.
export const versionInfo = {
  version: '${newVersion}',
  buildTime: '${buildTimestamp}'
};
`;

try {
    fs.writeFileSync(versionFilePath, versionFileContent, 'utf8');
    console.log(`${versionFilePath} generato.`);
} catch (error) {
    console.error(`Errore nella scrittura di ${versionFilePath}:`, error);
    process.exit(1);
}
