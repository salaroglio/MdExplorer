/**
 * MdExplorer Christmas Edition 2024
 * Animazioni natalizie temporanee
 *
 * Per disattivare: commentare il caricamento in common.js e common.css
 */

(function() {
    'use strict';

    // Evita caricamenti multipli
    if (window.xmasLoaded) {
        console.log('[XMAS] Already loaded, skipping...');
        return;
    }
    window.xmasLoaded = true;

    console.log('[XMAS] Loading Christmas animations...');

    // ==========================================================================
    // CONFIGURAZIONE
    // ==========================================================================
    const CONFIG = {
        // Slitta volante
        flyingEnabled: true,
        flyInterval: 30000,        // Ogni 30 secondi
        flyDuration: 8000,         // Durata animazione 8 secondi
        sleighSize: 150,           // Larghezza slitta volante

        // Progress bar
        progressEnabled: true,
        progressSleighSize: 70,    // Larghezza slitta progress

        // Neve (opzionale)
        snowEnabled: false,        // Disabilitata per default
        snowflakeCount: 30,

        // Debug
        debug: false
    };

    // ==========================================================================
    // SLITTA VOLANTE
    // ==========================================================================
    let flyingSleigh = null;
    let flyInterval = null;

    function createFlyingSleigh() {
        if (!CONFIG.flyingEnabled) return;

        flyingSleigh = document.createElement('div');
        flyingSleigh.className = 'xmas-sleigh';
        flyingSleigh.innerHTML = `<img src="/xmas/santa-sleigh.svg" width="${CONFIG.sleighSize}" alt="Santa">`;
        document.body.appendChild(flyingSleigh);

        if (CONFIG.debug) console.log('[XMAS] Flying sleigh created');
    }

    function triggerFlight() {
        if (!flyingSleigh) return;

        // Rimuovi classe per resettare animazione
        flyingSleigh.classList.remove('flying');

        // Forza reflow
        void flyingSleigh.offsetWidth;

        // Avvia animazione
        flyingSleigh.classList.add('flying');

        if (CONFIG.debug) console.log('[XMAS] Sleigh flight triggered!');

        // Rimuovi classe dopo animazione
        setTimeout(() => {
            flyingSleigh.classList.remove('flying');
        }, CONFIG.flyDuration);
    }

    function startFlyingSchedule() {
        if (!CONFIG.flyingEnabled) return;

        // Prima animazione dopo 3 secondi
        setTimeout(() => {
            triggerFlight();

            // Poi ogni X secondi
            flyInterval = setInterval(triggerFlight, CONFIG.flyInterval);
        }, 3000);

        if (CONFIG.debug) console.log('[XMAS] Flying schedule started');
    }

    // ==========================================================================
    // PROGRESS BAR CON SLITTA
    // ==========================================================================
    let progressContainer = null;
    let progressSleigh = null;
    let progressFill = null;
    let progressPercent = null;

    function createProgressBar() {
        if (!CONFIG.progressEnabled) return;

        progressContainer = document.createElement('div');
        progressContainer.className = 'xmas-progress-container';
        progressContainer.innerHTML = `
            <span class="xmas-star">&#10022;</span>
            <span class="xmas-star">&#10022;</span>
            <span class="xmas-star">&#10022;</span>
            <span class="xmas-star">&#10022;</span>
            <span class="xmas-star">&#10022;</span>
            <div class="xmas-progress-ground"></div>
            <div class="xmas-progress-track">
                <div class="xmas-progress-fill"></div>
            </div>
            <div class="xmas-progress-sleigh">
                <img src="/xmas/santa-sleigh.svg" width="${CONFIG.progressSleighSize}" alt="Progress">
            </div>
            <div class="xmas-progress-percent">0%</div>
        `;
        document.body.appendChild(progressContainer);

        progressSleigh = progressContainer.querySelector('.xmas-progress-sleigh');
        progressFill = progressContainer.querySelector('.xmas-progress-fill');
        progressPercent = progressContainer.querySelector('.xmas-progress-percent');

        // Aggiorna posizione al scroll
        window.addEventListener('scroll', updateProgress, { passive: true });

        // Aggiorna anche su resize
        window.addEventListener('resize', updateProgress, { passive: true });

        // Update iniziale
        updateProgress();

        if (CONFIG.debug) console.log('[XMAS] Progress bar created');
    }

    function updateProgress() {
        if (!progressSleigh || !progressFill || !progressPercent) return;

        // Calcola progresso scroll
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const docHeight = Math.max(
            document.body.scrollHeight,
            document.documentElement.scrollHeight
        ) - window.innerHeight;

        let progress = 0;
        if (docHeight > 0) {
            progress = Math.min(scrollTop / docHeight, 1);
        }

        const percent = Math.round(progress * 100);

        // Calcola posizione slitta (con margini)
        const containerWidth = progressContainer.offsetWidth;
        const margin = 30;
        const maxLeft = containerWidth - CONFIG.progressSleighSize - margin * 2;
        const sleighLeft = margin + (progress * maxLeft);

        // Aggiorna UI
        progressSleigh.style.left = sleighLeft + 'px';
        progressFill.style.width = (progress * 100) + '%';
        progressPercent.textContent = percent + '%';
    }

    // ==========================================================================
    // NEVE (Opzionale)
    // ==========================================================================
    function createSnowflakes() {
        if (!CONFIG.snowEnabled) return;

        const snowflakes = ['&#10052;', '&#10053;', '&#10054;', '&#42;'];
        const sizes = ['small', 'medium', 'large'];

        for (let i = 0; i < CONFIG.snowflakeCount; i++) {
            const flake = document.createElement('div');
            flake.className = 'xmas-snowflake ' + sizes[Math.floor(Math.random() * sizes.length)];
            flake.innerHTML = snowflakes[Math.floor(Math.random() * snowflakes.length)];
            flake.style.left = Math.random() * 100 + 'vw';
            flake.style.animationDuration = (Math.random() * 5 + 5) + 's';
            flake.style.animationDelay = (Math.random() * 10) + 's';
            document.body.appendChild(flake);
        }

        if (CONFIG.debug) console.log('[XMAS] Snowflakes created');
    }

    // ==========================================================================
    // EASTER EGG: ABBATTI LA SLITTA!
    // ==========================================================================
    let sleighHit = false;
    const HIT_RADIUS = 100; // Pixel di tolleranza per il click

    function setupSleighHitDetection() {
        document.addEventListener('click', function(e) {
            if (!flyingSleigh || sleighHit) return;
            if (!flyingSleigh.classList.contains('flying')) return;

            // Ottieni posizione della slitta
            const sleighRect = flyingSleigh.getBoundingClientRect();
            const sleighCenterX = sleighRect.left + sleighRect.width / 2;
            const sleighCenterY = sleighRect.top + sleighRect.height / 2;

            // Calcola distanza dal click
            const distance = Math.sqrt(
                Math.pow(e.clientX - sleighCenterX, 2) +
                Math.pow(e.clientY - sleighCenterY, 2)
            );

            if (distance < HIT_RADIUS + sleighRect.width / 2) {
                // COLPITO!
                shootDownSleigh(e.clientX, e.clientY, sleighCenterX, sleighCenterY);
            }
        });
    }

    function shootDownSleigh(clickX, clickY, sleighX, sleighY) {
        sleighHit = true;
        console.log('[XMAS] 💥 SLEIGH HIT! Santa is going down!');

        // Crea esplosioni multiple
        createExplosion(sleighX, sleighY);
        setTimeout(() => createExplosion(sleighX + 30, sleighY - 20), 100);
        setTimeout(() => createExplosion(sleighX - 20, sleighY + 10), 200);

        // Mostra il Grinch che ride
        setTimeout(() => showGrinch(), 500);

        // Aggiungi classe hit alla slitta
        flyingSleigh.classList.add('hit');
        flyingSleigh.classList.remove('flying');

        // Crea scia di fiamme
        let flameInterval = setInterval(() => {
            if (!flyingSleigh) {
                clearInterval(flameInterval);
                return;
            }
            const rect = flyingSleigh.getBoundingClientRect();
            if (rect.top > window.innerHeight) {
                clearInterval(flameInterval);
                return;
            }
            createFlame(rect.left + rect.width / 2, rect.top + rect.height / 2);
            createDebris(rect.left + Math.random() * rect.width, rect.top);
        }, 100);

        // Reset dopo 3 secondi
        setTimeout(() => {
            sleighHit = false;
            flyingSleigh.classList.remove('hit');
            flyingSleigh.style.opacity = '0';

            // Pulisci elementi residui
            document.querySelectorAll('.xmas-explosion, .xmas-flame, .xmas-debris, .xmas-grinch, .xmas-grinch-laugh')
                .forEach(el => el.remove());
        }, 3000);
    }

    function createExplosion(x, y) {
        const explosions = ['💥', '🔥', '💣', '✨', '⭐'];
        for (let i = 0; i < 5; i++) {
            const explosion = document.createElement('div');
            explosion.className = 'xmas-explosion';
            explosion.innerHTML = explosions[Math.floor(Math.random() * explosions.length)];
            explosion.style.left = (x + (Math.random() - 0.5) * 60) + 'px';
            explosion.style.top = (y + (Math.random() - 0.5) * 60) + 'px';
            explosion.style.animationDelay = (Math.random() * 0.2) + 's';
            document.body.appendChild(explosion);

            setTimeout(() => explosion.remove(), 1000);
        }
    }

    function createFlame(x, y) {
        const flames = ['🔥', '🔥', '💨', '☁️'];
        const flame = document.createElement('div');
        flame.className = 'xmas-flame';
        flame.innerHTML = flames[Math.floor(Math.random() * flames.length)];
        flame.style.left = (x + (Math.random() - 0.5) * 40) + 'px';
        flame.style.top = y + 'px';
        document.body.appendChild(flame);

        setTimeout(() => flame.remove(), 1000);
    }

    function createDebris(x, y) {
        const debris = ['🎁', '⭐', '🦌', '🔔', '❄️', '🎄'];
        const deb = document.createElement('div');
        deb.className = 'xmas-debris';
        deb.innerHTML = debris[Math.floor(Math.random() * debris.length)];
        deb.style.left = x + 'px';
        deb.style.top = y + 'px';
        deb.style.animationDuration = (1 + Math.random()) + 's';
        document.body.appendChild(deb);

        setTimeout(() => deb.remove(), 2000);
    }

    function showGrinch() {
        // Grinch emoji che ride
        const grinch = document.createElement('div');
        grinch.className = 'xmas-grinch';
        grinch.innerHTML = '😈';
        document.body.appendChild(grinch);

        // Testo risata
        const laugh = document.createElement('div');
        laugh.className = 'xmas-grinch-laugh';
        laugh.textContent = 'Muahaha!';
        document.body.appendChild(laugh);

        // Rimuovi dopo animazione
        setTimeout(() => {
            grinch.remove();
            laugh.remove();
        }, 3500);
    }

    // ==========================================================================
    // INIZIALIZZAZIONE
    // ==========================================================================
    function init() {
        console.log('[XMAS] Initializing Christmas animations...');

        createFlyingSleigh();
        createProgressBar();
        createSnowflakes();
        startFlyingSchedule();
        setupSleighHitDetection();

        console.log('[XMAS] Christmas animations ready!');
        console.log('[XMAS] 🎯 Easter egg: Click on the flying sleigh to shoot it down!');
    }

    // Avvia quando DOM è pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        // DOM già caricato
        init();
    }

    // ==========================================================================
    // API PUBBLICA (per debug/configurazione)
    // ==========================================================================
    window.xmasAnimations = {
        triggerFlight: triggerFlight,
        updateProgress: updateProgress,
        config: CONFIG,

        // Abilita/disabilita neve
        toggleSnow: function(enabled) {
            CONFIG.snowEnabled = enabled;
            if (enabled) {
                createSnowflakes();
            } else {
                document.querySelectorAll('.xmas-snowflake').forEach(el => el.remove());
            }
        },

        // Rimuovi tutte le animazioni
        destroy: function() {
            if (flyInterval) clearInterval(flyInterval);
            if (flyingSleigh) flyingSleigh.remove();
            if (progressContainer) progressContainer.remove();
            document.querySelectorAll('.xmas-snowflake').forEach(el => el.remove());
            window.xmasLoaded = false;
            console.log('[XMAS] Christmas animations destroyed');
        }
    };

})();
