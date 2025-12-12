import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';
import fs from 'fs';
// Helper function to ensure directory exists (can be kept as it's generally useful)
function ensureDirSync(dirPath) {
    if (!fs.existsSync(dirPath)) {
        fs.mkdirSync(dirPath, { recursive: true });
    }
}
// Original logic for copying main CSS files for local testing
function originalCopyCssPlugin() {
    return {
        name: 'copy-main-css-for-local-test',
        closeBundle: function () {
            console.log('[vite-plugin-copy-main-css-local] Starting copy of main CSS for local test...');
            var mainCssSource = 'public/css/milkdown-all.css';
            if (fs.existsSync(mainCssSource)) {
                // For dist/css/ (perhaps used by other means or older configs, keep for now)
                ensureDirSync('dist/css');
                fs.copyFileSync(mainCssSource, 'dist/css/milkdown-all.css');
                console.log("[vite-plugin-copy-main-css-local] Copied ".concat(mainCssSource, " to dist/css/milkdown-all.css"));
                // For very-simple-test.html convenience (if served from root)
                fs.copyFileSync(mainCssSource, 'milkdown-all.css');
                console.log("[vite-plugin-copy-main-css-local] Copied ".concat(mainCssSource, " to project root/milkdown-all.css for very-simple-test.html"));
            }
            else {
                console.warn("[vite-plugin-copy-main-css-local] Main CSS file ".concat(mainCssSource, " not found."));
            }
            console.log('[vite-plugin-copy-main-css-local] Finished copying main CSS for local test.');
        }
    };
}
// New plugin to handle deployable CSS assets
function vitePluginDeployMilkdownCss() {
    var filesToCopy = [
        { src: 'node_modules/@milkdown/prose/lib/style/prosemirror.css', destName: 'milkdown-prose-prosemirror.css' },
        { src: 'node_modules/prosemirror-view/style/prosemirror.css', destName: 'prosemirror-view-prosemirror.css' },
        { src: 'node_modules/prosemirror-gapcursor/style/gapcursor.css', destName: 'prosemirror-gapcursor-gapcursor.css' },
        { src: 'node_modules/prosemirror-tables/style/tables.css', destName: 'prosemirror-tables-tables.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/prosemirror.css', destName: 'milkdown-crepe-common-prosemirror.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/block-edit.css', destName: 'milkdown-crepe-common-block-edit.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/code-mirror.css', destName: 'milkdown-crepe-common-code-mirror.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/cursor.css', destName: 'milkdown-crepe-common-cursor.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/image-block.css', destName: 'milkdown-crepe-common-image-block.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/latex.css', destName: 'milkdown-crepe-common-latex.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/link-tooltip.css', destName: 'milkdown-crepe-common-link-tooltip.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/list-item.css', destName: 'milkdown-crepe-common-list-item.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/placeholder.css', destName: 'milkdown-crepe-common-placeholder.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/table.css', destName: 'milkdown-crepe-common-table.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/common/toolbar.css', destName: 'milkdown-crepe-common-toolbar.css' },
        { src: 'node_modules/@milkdown/crepe/lib/theme/crepe/style.css', destName: 'milkdown-crepe-theme-crepe-style.css' },
    ];
    return {
        name: 'vite-plugin-deploy-milkdown-css',
        closeBundle: function () {
            console.log('[vite-plugin-deploy-css] Starting copy of Milkdown CSS assets for deployment...');
            var outputAssetCssDir = 'dist/assets/css';
            ensureDirSync(outputAssetCssDir);
            // Copy individual imported CSS files
            filesToCopy.forEach(function (file) {
                var sourcePath = path.resolve(process.cwd(), file.src);
                var destPath = path.resolve(process.cwd(), outputAssetCssDir, file.destName);
                if (fs.existsSync(sourcePath)) {
                    fs.copyFileSync(sourcePath, destPath);
                    console.log("[vite-plugin-deploy-css] Copied ".concat(file.src, " to ").concat(outputAssetCssDir, "/").concat(file.destName));
                }
                else {
                    console.warn("[vite-plugin-deploy-css] Source CSS file not found: ".concat(sourcePath));
                }
            });
            // Copy and rename milkdown-all-deploy.css to milkdown-all.css in dist/assets/css
            var deployCssSource = 'public/css/milkdown-all-deploy.css';
            var deployCssDest = path.resolve(process.cwd(), outputAssetCssDir, 'milkdown-all.css');
            if (fs.existsSync(deployCssSource)) {
                fs.copyFileSync(deployCssSource, deployCssDest);
                console.log("[vite-plugin-deploy-css] Copied ".concat(deployCssSource, " to ").concat(outputAssetCssDir, "/milkdown-all.css"));
            }
            else {
                console.warn("[vite-plugin-deploy-css] ".concat(deployCssSource, " not found. Please create and configure this file."));
            }
            console.log('[vite-plugin-deploy-css] Finished copying Milkdown CSS assets for deployment.');
        }
    };
}
// Plugin per servire node_modules durante lo sviluppo (per gli @import CSS)
// Il problema: gli @import CSS vengono risolti dal browser, non da Vite
// Quando il browser richiede /node_modules/..., Vite lo trasforma in JS per HMR
// Soluzione: servire i CSS raw PRIMA che Vite li trasformi
function serveNodeModulesPlugin() {
    return {
        name: 'serve-node-modules-css',
        enforce: 'pre',
        configureServer: function (server) {
            // Aggiungi middleware PRIMA di tutti gli altri (inclusi quelli di Vite)
            server.middlewares.use(function (req, res, next) {
                // Intercetta solo richieste a /node_modules/ per file CSS
                if (req.url && req.url.startsWith('/node_modules/') && req.url.endsWith('.css')) {
                    var filePath = path.join(process.cwd(), req.url);
                    if (fs.existsSync(filePath)) {
                        res.setHeader('Content-Type', 'text/css');
                        res.setHeader('Cache-Control', 'no-cache');
                        fs.createReadStream(filePath).pipe(res);
                        return; // Termina qui, non passare a next()
                    }
                }
                next();
            });
        }
    };
}
// https://vite.dev/config/
export default defineConfig({
    plugins: [
        react(),
        serveNodeModulesPlugin(), // Serve node_modules in dev mode
        originalCopyCssPlugin(),
        vitePluginDeployMilkdownCss() // Add the new plugin
    ],
    define: {
        // Vue feature flags richiesti da Milkdown/Crepe (che usa Vue internamente)
        __VUE_OPTIONS_API__: JSON.stringify(true),
        __VUE_PROD_DEVTOOLS__: JSON.stringify(false),
        __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: JSON.stringify(false),
        // Polyfill per process.env (richiesto da alcune librerie Node.js)
        'process.env.NODE_ENV': JSON.stringify('production'),
    },
    server: {
        fs: {
            // Permetti accesso a file fuori dalla root del progetto (node_modules)
            allow: ['..'],
        },
    },
    build: {
        lib: {
            entry: path.resolve(process.cwd(), 'src/integration.ts'),
            name: 'DocsPilot',
            fileName: function (format) { return "docspilot.".concat(format, ".js"); },
            formats: ['es', 'umd'],
        },
        rollupOptions: {
            external: ['react', 'react-dom', 'react/jsx-runtime'],
            output: {
                globals: {
                    react: 'React',
                    'react-dom': 'ReactDOM',
                    'react/jsx-runtime': 'jsxRuntime',
                },
                assetFileNames: function (assetInfo) {
                    // This was in the original, keep it.
                    // It handles CSS imported by JS/TS and processed by Vite.
                    if (assetInfo.name && assetInfo.name.endsWith('.css')) {
                        return 'css/[name][extname]';
                    }
                    return 'assets/[name][extname]';
                }
            }
        },
        emptyOutDir: false,
    },
});
