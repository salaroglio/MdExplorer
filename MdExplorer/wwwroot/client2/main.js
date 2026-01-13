(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["main"],{

/***/ "+Jvq":
/*!*****************************************************!*\
  !*** ./src/app/services/ai-notification.service.ts ***!
  \*****************************************************/
/*! exports provided: AiNotificationService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AiNotificationService", function() { return AiNotificationService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../md-explorer/services/md-file.service */ "xmhS");




class AiNotificationService {
    constructor(snackBar, serverMessages, mdFileService) {
        this.snackBar = snackBar;
        this.serverMessages = serverMessages;
        this.mdFileService = mdFileService;
        // Register listener for AI file operations
        this.registerAiFileOperationListener();
    }
    registerAiFileOperationListener() {
        this.serverMessages.addAiFileOperationListener((data, objectThis) => {
            objectThis.handleAiFileOperation(data);
        }, this);
    }
    handleAiFileOperation(data) {
        console.log('[AI Notification] Received event:', data);
        const icon = this.getOperationIcon(data.operationType);
        const action = data.success ? 'Open' : 'OK';
        const snackBarRef = this.snackBar.open(`${icon} ${data.message}`, action, {
            duration: data.success ? 5000 : 7000,
            horizontalPosition: 'right',
            verticalPosition: 'bottom',
            panelClass: data.success ? ['success-snackbar'] : ['error-snackbar']
        });
        // If successful and user clicks "Open", navigate to the file
        if (data.success) {
            snackBarRef.onAction().subscribe(() => {
                this.openFile(data.filePath);
            });
        }
    }
    getOperationIcon(operationType) {
        switch (operationType) {
            case 'create':
                return '📄';
            case 'read':
                return '📖';
            case 'update':
                return '✏️';
            default:
                return '🤖';
        }
    }
    openFile(filePath) {
        console.log('[AI Notification] Opening file:', filePath);
        // TODO: Implement file navigation
        // For now, just log the request. In the future, we can:
        // 1. Find the MdFile object by path
        // 2. Call mdFileService.setSelectedMdFileFromSideNav(mdFile)
        // 3. Navigate to the file in the editor
        // Temporary: show info that feature is coming
        this.snackBar.open(`File path: ${filePath}`, 'OK', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
        });
    }
}
AiNotificationService.ɵfac = function AiNotificationService_Factory(t) { return new (t || AiNotificationService)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_1__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_2__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__["MdFileService"])); };
AiNotificationService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: AiNotificationService, factory: AiNotificationService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "+dpY":
/*!*************************************************************!*\
  !*** ./src/app/signalR/services/server-messages.service.ts ***!
  \*************************************************************/
/*! exports provided: MdServerMessagesService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdServerMessagesService", function() { return MdServerMessagesService; });
/* harmony import */ var _microsoft_signalr__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @microsoft/signalr */ "6HpG");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../signalR/dialogs/parsing-project/parsing-project.provider */ "YG1a");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../signalR/dialogs/plantuml-working/plantuml-working.provider */ "CqLH");
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../signalR/dialogs/connection-lost/connection-lost.provider */ "jX2R");
/* harmony import */ var _dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../dialogs/opening-application/opening-application.provider */ "l94Z");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../git/services/gitservice.service */ "N73s");








class MdServerMessagesService {
    constructor(parsingProjectProvider, plantumlWorkingProvider, connectionLostProvider, openingApplicationProvider, gitService, injector) {
        this.parsingProjectProvider = parsingProjectProvider;
        this.plantumlWorkingProvider = plantumlWorkingProvider;
        this.connectionLostProvider = connectionLostProvider;
        this.openingApplicationProvider = openingApplicationProvider;
        this.gitService = gitService;
        this.injector = injector;
        // Observable for Git branch switch events
        this.gitBranchSwitched$ = new rxjs__WEBPACK_IMPORTED_MODULE_1__["Subject"]();
        this.connectionIsLost = false;
        this.consoleIsClosed = false;
        this.startConnection = () => {
            if (this.hubConnection == null) {
                this.hubConnection = new _microsoft_signalr__WEBPACK_IMPORTED_MODULE_0__["HubConnectionBuilder"]()
                    .withUrl('../signalr/monitormd')
                    .build();
                this.hubConnection.on('markdownfileisprocessed', (data) => {
                    this.processCallBack(data, 'markdownfileisprocessed');
                });
                this.hubConnection.on('yamlAutoGenerated', (data) => {
                    this.processCallBack(data, 'yamlAutoGenerated');
                });
                this.hubConnection.on('documentNavigated', (data) => {
                    this.processCallBack(data, 'documentNavigated');
                });
                this.hubConnection.on('parsingProjectStart', (data) => {
                    this.parsingProjectProvider.show(data);
                });
                this.hubConnection.on('openingApplication', (data) => {
                    this.openingApplicationProvider.show(data);
                });
                this.hubConnection.on('parsingProjectStop', (data) => {
                    this.parsingProjectProvider.hide(data);
                });
                this.hubConnection.on('plantumlWorkStart', (data) => {
                    this.plantumlWorkingProvider.show(data);
                });
                this.hubConnection.on('plantumlWorkStop', (data) => {
                    this.plantumlWorkingProvider.hide(data);
                });
                this.hubConnection.on('indexingFolder', (folder) => {
                    this.parsingProjectProvider.folder$.next(folder);
                });
                // Git branch switch event (client-specific, from ModernGitController)
                this.hubConnection.on('gitBranchSwitched', (data) => {
                    console.log('✅ SignalR event received: gitBranchSwitched', data);
                    this.gitBranchSwitched$.next(data);
                });
                this.hubConnection.on('consoleClosed', (data) => {
                    console.log('consoleClosed');
                    this.consoleIsClosed = true;
                    this.connectionLostProvider.showConsoleClosed();
                });
                this.hubConnection.onclose((data) => {
                    if (!this.consoleIsClosed) {
                        this.connectionLostProvider.show(this);
                        this.connectionIsLost = true;
                    }
                });
            }
            if (this.hubConnection.state == "Disconnected") {
                const wasReconnection = this.connectionIsLost; // Capture before reset
                this.hubConnection
                    .start()
                    .then(() => {
                    console.log('Connection started');
                    this.connectionIsLost = false;
                    this.getCurrentConnectionId(this, wasReconnection);
                })
                    .catch(err => {
                    console.log('Error while starting connection: ' + err);
                });
            }
        };
        this.startConnection();
        console.log('MonitorMDService constructor');
        this.linkEventCompArray = [];
    }
    addRefactoringFileEvent(callback, objectThis) {
        this.hubConnection.on('refactoringFileEvent', (data) => {
            callback(data, objectThis);
        });
    }
    addMarkdownFileListener(callback, objectThis) {
        this.hubConnection.on('markdownfileischanged', (data) => {
            // Modern Git service will automatically update via polling - no manual refresh needed
            callback(data, objectThis);
            console.log('markdownfileischanged');
        });
    }
    processCallBack(data, signalREvent) {
        this.linkEventCompArray.forEach(_ => {
            if (_.key == signalREvent) {
                _.callback(data, _.object);
            }
        });
    }
    addMdProcessedListener(callback, objectThis) {
        let check = this.linkEventCompArray.find(_ => _.key == 'markdownfileisprocessed' && _.object.constructor.name === objectThis.constructor.name);
        if (check == undefined) {
            this.linkEventCompArray.push({ key: 'markdownfileisprocessed', object: objectThis, callback: callback });
        }
    }
    addMdRule1Listener(callback, objectThis) {
        // giusto per evitare di effettuare l'instanziazione un centinaio di volte l'evento
        console.log('addMdRule1Listener');
        if (this.rule1IsRegistered == undefined) {
            this.rule1IsRegistered = objectThis;
            this.hubConnection.on('markdownbreakrule1', (data) => {
                callback(data, objectThis);
            });
        }
    }
    addPdfIsReadyListener(callback, objectThis) {
        this.hubConnection.on('pdfisready', (data) => {
            callback(data, objectThis);
        });
    }
    addYamlAutoGeneratedListener(callback, objectThis) {
        let check = this.linkEventCompArray.find(_ => _.key == 'yamlAutoGenerated' && _.object.constructor.name === objectThis.constructor.name);
        if (check == undefined) {
            this.linkEventCompArray.push({ key: 'yamlAutoGenerated', object: objectThis, callback: callback });
        }
    }
    addDocumentNavigatedListener(callback, objectThis) {
        let check = this.linkEventCompArray.find(_ => _.key == 'documentNavigated' && _.object.constructor.name === objectThis.constructor.name);
        if (check == undefined) {
            this.linkEventCompArray.push({ key: 'documentNavigated', object: objectThis, callback: callback });
        }
    }
    addRule1ForceUpdateListener(callback, objectThis) {
        console.log('addRule1ForceUpdateListener');
        if (this.rule1ForceUpdateRegistered == undefined) {
            this.rule1ForceUpdateRegistered = objectThis;
            // Non abbiamo bisogno di un evento SignalR reale, useremo questo per il pattern locale
        }
    }
    // Metodo per triggerare l'evento di force update localmente
    triggerRule1ForceUpdate(filePath) {
        var _a;
        if ((_a = this.rule1ForceUpdateRegistered) === null || _a === void 0 ? void 0 : _a.handleRule1ForceUpdate) {
            this.rule1ForceUpdateRegistered.handleRule1ForceUpdate(filePath);
        }
    }
    addFileIndexedListener(callback, objectThis) {
        this.hubConnection.on('fileIndexed', (data) => {
            callback(data, objectThis);
        });
    }
    addFolderIndexingStartListener(callback, objectThis) {
        this.hubConnection.on('folderIndexingStart', (data) => {
            callback(data, objectThis);
        });
    }
    addFolderIndexingCompleteListener(callback, objectThis) {
        this.hubConnection.on('folderIndexingComplete', (data) => {
            callback(data, objectThis);
        });
    }
    addParsingProjectStartListener(callback, objectThis) {
        this.hubConnection.on('parsingProjectStart', (data) => {
            callback(data, objectThis);
        });
    }
    addMarkdownFileCreatedListener(callback, objectThis) {
        this.hubConnection.on('markdownFileCreated', (data) => {
            console.log('📄 [SignalR] Evento markdownFileCreated ricevuto:');
            console.log('📄 [SignalR] Data ricevuta:', JSON.stringify(data, null, 2));
            console.log('📄 [SignalR] Nome file:', data.name);
            console.log('📄 [SignalR] Path completo:', data.fullPath);
            callback(data, objectThis);
        });
    }
    addMarkdownFileDeletedListener(callback, objectThis) {
        this.hubConnection.on('markdownFileDeleted', (data) => {
            console.log('🗑️ [SignalR] Evento markdownFileDeleted ricevuto:');
            console.log('🗑️ [SignalR] Data ricevuta:', JSON.stringify(data, null, 2));
            console.log('🗑️ [SignalR] Nome file:', data.name);
            console.log('🗑️ [SignalR] Path completo:', data.fullPath);
            callback(data, objectThis);
        });
    }
    addParsingProjectStopListener(callback, objectThis) {
        this.hubConnection.on('parsingProjectStop', (data) => {
            callback(data, objectThis);
        });
    }
    addConnectionIdListener(callback, objectThis) {
        this.hubConnection.on('getconnectionid', (data) => {
            callback(data, objectThis);
        });
    }
    getConnectionId(callback, objectThis) {
        this.hubConnection.invoke('GetConnectionId')
            .then(function (connectionId) {
            objectThis.connectionId = connectionId;
            callback(connectionId, objectThis);
        });
    }
    getCurrentConnectionId(objectThis, isReconnection = false) {
        this.hubConnection.invoke('GetConnectionId')
            .then(function (connectionId) {
            var _a;
            objectThis.connectionId = connectionId;
            // Notify Electron that connectionId is ready (for URL handler feature)
            if ((_a = window.electronAPI) === null || _a === void 0 ? void 0 : _a.notifyConnectionIdReady) {
                console.log('[SignalR] Notifying Electron of connectionId:', connectionId);
                window.electronAPI.notifyConnectionIdReady(connectionId);
            }
            // If this was a reconnection, re-register the current project with the new connectionId
            // This is necessary because when SignalR disconnects, the backend cleans up
            // FileSystemWatcher and DatabaseContext for the old connectionId
            if (isReconnection) {
                console.log('[SignalR] Reconnection detected, re-registering project...');
                objectThis.reregisterCurrentProject();
            }
        });
    }
    reregisterCurrentProject() {
        // Use dynamic import to avoid circular dependency issues
        Promise.resolve(/*! import() */).then(__webpack_require__.bind(null, /*! ../../md-explorer/services/projects.service */ "vUCT")).then(module => {
            const projectsService = this.injector.get(module.ProjectsService);
            projectsService.reregisterCurrentProject();
        }).catch(err => {
            console.error('[SignalR] Failed to re-register project:', err);
        });
    }
    // TOC Generation listeners
    addTocGenerationProgressListener(callback, objectThis) {
        this.hubConnection.on('TocGenerationProgress', (data) => {
            console.log('[SignalR] TOC Generation Progress:', data);
            callback(data, objectThis);
        });
    }
    addTocGenerationCompleteListener(callback, objectThis) {
        this.hubConnection.on('TocGenerationComplete', (data) => {
            console.log('[SignalR] TOC Generation Complete:', data);
            callback(data, objectThis);
        });
    }
    // AI File Operation listener
    addAiFileOperationListener(callback, objectThis) {
        this.hubConnection.on('aiFileOperation', (data) => {
            console.log('[SignalR] AI File Operation:', data);
            callback(data, objectThis);
        });
    }
    // URL Handler listeners
    addUrlHandlerOpenDocumentListener(callback, objectThis) {
        this.hubConnection.on('urlHandlerOpenDocument', (data) => {
            console.log('[SignalR] URL Handler Open Document:', data);
            callback(data, objectThis);
        });
    }
    addUrlHandlerOpenConfigProjectDialogListener(callback, objectThis) {
        // Listen for new configproject event
        this.hubConnection.on('urlHandlerOpenConfigProjectDialog', (data) => {
            console.log('[SignalR] URL Handler Open ConfigProject Dialog:', data);
            callback(data, objectThis);
        });
        // Also listen for legacy clone event for backward compatibility
        this.hubConnection.on('urlHandlerOpenCloneDialog', (data) => {
            console.log('[SignalR] URL Handler Open Clone Dialog (legacy):', data);
            callback(data, objectThis);
        });
    }
    addUrlHandlerErrorListener(callback, objectThis) {
        this.hubConnection.on('urlHandlerError', (data) => {
            console.log('[SignalR] URL Handler Error:', data);
            callback(data, objectThis);
        });
    }
}
MdServerMessagesService.ɵfac = function MdServerMessagesService_Factory(t) { return new (t || MdServerMessagesService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_3__["ParsingProjectProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_4__["PlantumlWorkingProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_5__["ConnectionLostProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_6__["OpeningApplicationProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_7__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["Injector"])); };
MdServerMessagesService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: MdServerMessagesService, factory: MdServerMessagesService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ 0:
/*!***************************!*\
  !*** multi ./src/main.ts ***!
  \***************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

module.exports = __webpack_require__(/*! C:\sviluppo\mdExplorer\MdExplorer\client2\src\main.ts */"zUnb");


/***/ }),

/***/ "25Jb":
/*!****************************************************!*\
  !*** ./src/app/models/compatibility-mode.model.ts ***!
  \****************************************************/
/*! exports provided: CompatibilityMode */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CompatibilityMode", function() { return CompatibilityMode; });
var CompatibilityMode;
(function (CompatibilityMode) {
    CompatibilityMode["MdExplorer"] = "mdexplorer";
    CompatibilityMode["GitHub"] = "github";
    CompatibilityMode["CommonMark"] = "commonmark";
})(CompatibilityMode || (CompatibilityMode = {}));


/***/ }),

/***/ "4rFV":
/*!********************************************************!*\
  !*** ./src/app/services/compatibility-mode.service.ts ***!
  \********************************************************/
/*! exports provided: CompatibilityModeService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CompatibilityModeService", function() { return CompatibilityModeService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../models/compatibility-mode.model */ "25Jb");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ "+dpY");







class CompatibilityModeService {
    constructor(http, projectsService, serverMessages) {
        this.http = http;
        this.projectsService = projectsService;
        this.serverMessages = serverMessages;
        this.apiUrl = '/api/compatibility';
        // Observable per il mode corrente
        this.currentModeSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](_models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer);
        this.currentMode$ = this.currentModeSubject.asObservable();
        this.configSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](null);
        this.config$ = this.configSubject.asObservable();
        // Load initial mode
        this.loadCurrentMode();
        // Reload when project changes
        this.projectsService.currentProjects$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["filter"])(project => project != null))
            .subscribe(() => {
            console.log('Project changed, reloading compatibility mode');
            this.loadCurrentMode();
        });
    }
    /**
     * Load current compatibility mode from server
     */
    loadCurrentMode() {
        // Reset to default before loading to avoid keeping old project's mode
        this.currentModeSubject.next(_models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer);
        // Skip if connectionId not yet available (will be called again when project changes)
        const connectionId = this.serverMessages.connectionId;
        if (!connectionId) {
            console.log('ConnectionId not yet available, skipping compatibility mode load');
            return;
        }
        this.http.get(`${this.apiUrl}/mode`)
            .subscribe({
            next: (config) => {
                var _a;
                console.log('Loaded compatibility config from backend:', config);
                this.configSubject.next(config);
                // Ensure mode is set, default to mdexplorer if missing
                const mode = ((_a = config === null || config === void 0 ? void 0 : config.mode) === null || _a === void 0 ? void 0 : _a.toLowerCase()) || 'mdexplorer';
                console.log('Setting compatibility mode to:', mode);
                const compatMode = mode === 'github' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub :
                    mode === 'commonmark' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark :
                        _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
                this.currentModeSubject.next(compatMode);
            },
            error: (error) => {
                console.error('Error loading compatibility mode:', error);
                // Default to mdexplorer on error
                this.currentModeSubject.next(_models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer);
            }
        });
    }
    /**
     * Get current compatibility mode
     * @param projectPath Optional project path (for project settings dialog)
     */
    getCurrentMode(projectPath) {
        const params = {};
        if (projectPath) {
            params.projectPath = projectPath;
        }
        return this.http.get(`${this.apiUrl}/mode`, { params });
    }
    /**
     * Set compatibility mode
     */
    setCompatibilityMode(request) {
        return this.http.post(`${this.apiUrl}/mode`, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["tap"])(() => {
            // Reload mode after setting
            this.loadCurrentMode();
        }));
    }
    /**
     * Validate document for GitHub compatibility
     */
    validateDocument(request) {
        return this.http.post(`${this.apiUrl}/validate`, request);
    }
    /**
     * Check if current mode is GitHub
     */
    isGitHubMode() {
        return this.currentModeSubject.value === _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub;
    }
    /**
     * Check if current mode is MdExplorer
     */
    isMdExplorerMode() {
        return this.currentModeSubject.value === _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
    }
    /**
     * Get current mode value synchronously
     */
    getCurrentModeValue() {
        return this.currentModeSubject.value;
    }
    /**
     * Update mode directly (used when opening a project)
     */
    updateMode(mode) {
        console.log('Updating compatibility mode directly to:', mode);
        this.currentModeSubject.next(mode);
    }
    /**
     * Get mode display name
     */
    getModeDisplayName(mode) {
        switch (mode) {
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub:
                return 'GitHub Compatible';
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark:
                return 'CommonMark';
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer:
            default:
                return 'MdExplorer';
        }
    }
    /**
     * Get mode icon
     */
    getModeIcon(mode) {
        switch (mode) {
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub:
                return 'public'; // Material icon for public/github
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark:
                return 'article'; // Material icon for document
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer:
            default:
                return 'rocket_launch'; // Material icon for advanced features
        }
    }
    /**
     * Get mode color
     */
    getModeColor(mode) {
        switch (mode) {
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub:
                return 'accent'; // Blue/green for GitHub
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark:
                return 'warn'; // Orange for standard
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer:
            default:
                return 'primary'; // Primary theme color
        }
    }
}
CompatibilityModeService.ɵfac = function CompatibilityModeService_Factory(t) { return new (t || CompatibilityModeService)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_5__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_6__["MdServerMessagesService"])); };
CompatibilityModeService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({ token: CompatibilityModeService, factory: CompatibilityModeService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "5dmV":
/*!*******************************************!*\
  !*** ./src/app/shared/material.module.ts ***!
  \*******************************************/
/*! exports provided: MaterialModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MaterialModule", function() { return MaterialModule; });
/* harmony import */ var _angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/autocomplete */ "/1cH");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_material_datepicker__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/datepicker */ "iadO");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/core */ "FKr1");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/radio */ "QibW");
/* harmony import */ var _angular_material_select__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/select */ "d3UM");
/* harmony import */ var _angular_material_slider__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/slider */ "5RNC");
/* harmony import */ var _angular_material_slide_toggle__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/slide-toggle */ "1jcm");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/sidenav */ "XhcP");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/divider */ "f0Cb");
/* harmony import */ var _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/expansion */ "7EHt");
/* harmony import */ var _angular_material_grid_list__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/grid-list */ "zkoq");
/* harmony import */ var _angular_material_list__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/list */ "MutI");
/* harmony import */ var _angular_material_stepper__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/stepper */ "xHqg");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/tree */ "8yBR");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_button_toggle__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/material/button-toggle */ "jaxi");
/* harmony import */ var _angular_material_badge__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/badge */ "TU8p");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/material/progress-bar */ "bv9b");
/* harmony import */ var _angular_material_bottom_sheet__WEBPACK_IMPORTED_MODULE_28__ = __webpack_require__(/*! @angular/material/bottom-sheet */ "2ChS");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_29__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_30__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_31__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_paginator__WEBPACK_IMPORTED_MODULE_32__ = __webpack_require__(/*! @angular/material/paginator */ "M9IT");
/* harmony import */ var _angular_material_sort__WEBPACK_IMPORTED_MODULE_33__ = __webpack_require__(/*! @angular/material/sort */ "Dh3D");
/* harmony import */ var _angular_material_table__WEBPACK_IMPORTED_MODULE_34__ = __webpack_require__(/*! @angular/material/table */ "+0xr");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_35__ = __webpack_require__(/*! @angular/core */ "fXoL");





































class MaterialModule {
}
MaterialModule.ɵfac = function MaterialModule_Factory(t) { return new (t || MaterialModule)(); };
MaterialModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_35__["ɵɵdefineNgModule"]({ type: MaterialModule });
MaterialModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_35__["ɵɵdefineInjector"]({ imports: [_angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_0__["MatAutocompleteModule"],
        _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_1__["MatCheckboxModule"],
        _angular_material_datepicker__WEBPACK_IMPORTED_MODULE_2__["MatDatepickerModule"],
        _angular_material_core__WEBPACK_IMPORTED_MODULE_3__["MatNativeDateModule"],
        _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormFieldModule"],
        _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInputModule"],
        _angular_material_radio__WEBPACK_IMPORTED_MODULE_6__["MatRadioModule"],
        _angular_material_select__WEBPACK_IMPORTED_MODULE_7__["MatSelectModule"],
        _angular_material_slider__WEBPACK_IMPORTED_MODULE_8__["MatSliderModule"],
        _angular_material_slide_toggle__WEBPACK_IMPORTED_MODULE_9__["MatSlideToggleModule"],
        _angular_material_menu__WEBPACK_IMPORTED_MODULE_10__["MatMenuModule"],
        _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__["MatSidenavModule"],
        _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_12__["MatToolbarModule"],
        _angular_material_card__WEBPACK_IMPORTED_MODULE_13__["MatCardModule"],
        _angular_material_divider__WEBPACK_IMPORTED_MODULE_14__["MatDividerModule"],
        _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__["MatExpansionModule"],
        _angular_material_grid_list__WEBPACK_IMPORTED_MODULE_16__["MatGridListModule"],
        _angular_material_list__WEBPACK_IMPORTED_MODULE_17__["MatListModule"],
        _angular_material_stepper__WEBPACK_IMPORTED_MODULE_18__["MatStepperModule"],
        _angular_material_tabs__WEBPACK_IMPORTED_MODULE_19__["MatTabsModule"],
        _angular_material_tree__WEBPACK_IMPORTED_MODULE_20__["MatTreeModule"],
        _angular_material_button__WEBPACK_IMPORTED_MODULE_21__["MatButtonModule"],
        _angular_material_button_toggle__WEBPACK_IMPORTED_MODULE_22__["MatButtonToggleModule"],
        _angular_material_badge__WEBPACK_IMPORTED_MODULE_23__["MatBadgeModule"],
        _angular_material_chips__WEBPACK_IMPORTED_MODULE_24__["MatChipsModule"],
        _angular_material_icon__WEBPACK_IMPORTED_MODULE_25__["MatIconModule"],
        _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_26__["MatProgressSpinnerModule"],
        _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_27__["MatProgressBarModule"],
        _angular_material_core__WEBPACK_IMPORTED_MODULE_3__["MatRippleModule"],
        _angular_material_bottom_sheet__WEBPACK_IMPORTED_MODULE_28__["MatBottomSheetModule"],
        _angular_material_dialog__WEBPACK_IMPORTED_MODULE_29__["MatDialogModule"],
        _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_30__["MatSnackBarModule"],
        _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_31__["MatTooltipModule"],
        _angular_material_paginator__WEBPACK_IMPORTED_MODULE_32__["MatPaginatorModule"],
        _angular_material_sort__WEBPACK_IMPORTED_MODULE_33__["MatSortModule"],
        _angular_material_table__WEBPACK_IMPORTED_MODULE_34__["MatTableModule"]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_35__["ɵɵsetNgModuleScope"](MaterialModule, { exports: [_angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_0__["MatAutocompleteModule"],
        _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_1__["MatCheckboxModule"],
        _angular_material_datepicker__WEBPACK_IMPORTED_MODULE_2__["MatDatepickerModule"],
        _angular_material_core__WEBPACK_IMPORTED_MODULE_3__["MatNativeDateModule"],
        _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormFieldModule"],
        _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInputModule"],
        _angular_material_radio__WEBPACK_IMPORTED_MODULE_6__["MatRadioModule"],
        _angular_material_select__WEBPACK_IMPORTED_MODULE_7__["MatSelectModule"],
        _angular_material_slider__WEBPACK_IMPORTED_MODULE_8__["MatSliderModule"],
        _angular_material_slide_toggle__WEBPACK_IMPORTED_MODULE_9__["MatSlideToggleModule"],
        _angular_material_menu__WEBPACK_IMPORTED_MODULE_10__["MatMenuModule"],
        _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__["MatSidenavModule"],
        _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_12__["MatToolbarModule"],
        _angular_material_card__WEBPACK_IMPORTED_MODULE_13__["MatCardModule"],
        _angular_material_divider__WEBPACK_IMPORTED_MODULE_14__["MatDividerModule"],
        _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__["MatExpansionModule"],
        _angular_material_grid_list__WEBPACK_IMPORTED_MODULE_16__["MatGridListModule"],
        _angular_material_list__WEBPACK_IMPORTED_MODULE_17__["MatListModule"],
        _angular_material_stepper__WEBPACK_IMPORTED_MODULE_18__["MatStepperModule"],
        _angular_material_tabs__WEBPACK_IMPORTED_MODULE_19__["MatTabsModule"],
        _angular_material_tree__WEBPACK_IMPORTED_MODULE_20__["MatTreeModule"],
        _angular_material_button__WEBPACK_IMPORTED_MODULE_21__["MatButtonModule"],
        _angular_material_button_toggle__WEBPACK_IMPORTED_MODULE_22__["MatButtonToggleModule"],
        _angular_material_badge__WEBPACK_IMPORTED_MODULE_23__["MatBadgeModule"],
        _angular_material_chips__WEBPACK_IMPORTED_MODULE_24__["MatChipsModule"],
        _angular_material_icon__WEBPACK_IMPORTED_MODULE_25__["MatIconModule"],
        _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_26__["MatProgressSpinnerModule"],
        _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_27__["MatProgressBarModule"],
        _angular_material_core__WEBPACK_IMPORTED_MODULE_3__["MatRippleModule"],
        _angular_material_bottom_sheet__WEBPACK_IMPORTED_MODULE_28__["MatBottomSheetModule"],
        _angular_material_dialog__WEBPACK_IMPORTED_MODULE_29__["MatDialogModule"],
        _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_30__["MatSnackBarModule"],
        _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_31__["MatTooltipModule"],
        _angular_material_paginator__WEBPACK_IMPORTED_MODULE_32__["MatPaginatorModule"],
        _angular_material_sort__WEBPACK_IMPORTED_MODULE_33__["MatSortModule"],
        _angular_material_table__WEBPACK_IMPORTED_MODULE_34__["MatTableModule"]] }); })();


/***/ }),

/***/ "89FR":
/*!*************************************************************!*\
  !*** ./src/app/components/title-bar/title-bar.component.ts ***!
  \*************************************************************/
/*! exports provided: TitleBarComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "TitleBarComponent", function() { return TitleBarComponent; });
/* harmony import */ var _environments_version__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../environments/version */ "octk");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_navigation_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/md-navigation.service */ "hFUY");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _md_explorer_services_layout_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../md-explorer/services/layout.service */ "FGkB");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _search_box_search_box_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../search-box/search-box.component */ "XPJ/");
/* harmony import */ var _md_explorer_components_compatibility_mode_badge_compatibility_mode_badge_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../../md-explorer/components/compatibility-mode-badge/compatibility-mode-badge.component */ "ZNV1");












function TitleBarComponent_div_7_Template(rf, ctx) { if (rf & 1) {
    const _r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function TitleBarComponent_div_7_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r6); const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r5.backward(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "arrow_back");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function TitleBarComponent_div_7_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r6); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r7.forward(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "arrow_forward");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r0.navService.navigationGhost.length <= 1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r0.navService.navigation.length == ctx_r0.navService.navigationGhost.length);
} }
function TitleBarComponent_app_search_box_8_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "app-search-box");
} }
function TitleBarComponent_app_compatibility_mode_badge_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "app-compatibility-mode-badge");
} }
function TitleBarComponent_div_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "span", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "Welcome to MdExplorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "Your markdown documentation workspace");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function TitleBarComponent_div_12_Template(rf, ctx) { if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function TitleBarComponent_div_12_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r9); const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r8.minimizeWindow(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "remove");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function TitleBarComponent_div_12_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r9); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r10.maximizeWindow(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "crop_square");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function TitleBarComponent_div_12_Template_button_click_7_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r9); const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r11.closeWindow(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
class TitleBarComponent {
    constructor(navService, mdFileService, router, layoutService, monitorMDService) {
        this.navService = navService;
        this.mdFileService = mdFileService;
        this.router = router;
        this.layoutService = layoutService;
        this.monitorMDService = monitorMDService;
        this.isElectron = false;
        this.version = _environments_version__WEBPACK_IMPORTED_MODULE_0__["versionInfo"].version;
        this.buildTime = _environments_version__WEBPACK_IMPORTED_MODULE_0__["versionInfo"].buildTime;
        this.isSidenavOpen = true;
        this.isProjectOpened = false;
        this.isProjectsPage = false;
        // Check if running in Electron
        this.isElectron = !!(window && window.electronAPI);
    }
    ngOnInit() {
        // Subscribe to sidenav state changes
        this.layoutService.sidenavOpen$.subscribe(isOpen => {
            this.isSidenavOpen = isOpen;
        });
        // Subscribe to route changes to detect if we're in a project
        this.router.events.subscribe(() => {
            this.isProjectOpened = this.router.url.startsWith('/main');
            this.isProjectsPage = this.router.url === '/projects' || this.router.url.startsWith('/projects/');
        });
        // Initialize on first load
        this.isProjectOpened = this.router.url.startsWith('/main');
        this.isProjectsPage = this.router.url === '/projects' || this.router.url.startsWith('/projects/');
        // Subscribe to document navigation events from iframe links
        this.monitorMDService.addDocumentNavigatedListener(this.onDocumentNavigated, this);
    }
    onDocumentNavigated(data, objectThis) {
        console.log('[TitleBar] Document navigated event received:', data);
        // Create MdFile object from the data
        const mdFile = {
            name: data.name,
            path: data.relativePath,
            relativePath: data.relativePath,
            fullPath: data.fullPath,
            fullDirectoryPath: data.fullDirectoryPath,
            level: 0,
            expandable: false,
            type: 'file',
            index: 0,
            isLoading: false,
            childrens: []
        };
        // Add to navigation history
        objectThis.navService.setNewNavigation(mdFile);
        // Update the selected file so other components (like React editor) know the current file
        objectThis.mdFileService.setSelectedMdFileFromSideNav(mdFile);
        console.log('[TitleBar] Added to navigation history:', mdFile);
        console.log('[TitleBar] Navigation stack:', objectThis.navService.navigation);
    }
    backward() {
        console.log('[TitleBar] backward() called');
        console.log('[TitleBar] navigation before:', this.navService.navigation);
        console.log('[TitleBar] navigationGhost before:', this.navService.navigationGhost);
        console.log('[TitleBar] currentIndex before:', this.navService.currentIndex);
        const navToMdFile = this.navService.back();
        console.log('[TitleBar] navToMdFile returned:', navToMdFile);
        console.log('[TitleBar] Navigating to route: /main/navigation/document');
        this.router.navigate(['/main/navigation/document']);
        console.log('[TitleBar] Calling setSelectedMdFileFromSideNav with:', navToMdFile);
        this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
        console.log('[TitleBar] navigation after:', this.navService.navigation);
        console.log('[TitleBar] navigationGhost after:', this.navService.navigationGhost);
        console.log('[TitleBar] currentIndex after:', this.navService.currentIndex);
    }
    forward() {
        console.log('[TitleBar] forward() called');
        console.log('[TitleBar] navigation before:', this.navService.navigation);
        console.log('[TitleBar] navigationGhost before:', this.navService.navigationGhost);
        console.log('[TitleBar] currentIndex before:', this.navService.currentIndex);
        const navToMdFile = this.navService.forward();
        console.log('[TitleBar] navToMdFile returned:', navToMdFile);
        console.log('[TitleBar] Navigating to route: /main/navigation/document');
        this.router.navigate(['/main/navigation/document']);
        console.log('[TitleBar] Calling setSelectedMdFileFromSideNav with:', navToMdFile);
        this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
        console.log('[TitleBar] navigation after:', this.navService.navigation);
        console.log('[TitleBar] navigationGhost after:', this.navService.navigationGhost);
        console.log('[TitleBar] currentIndex after:', this.navService.currentIndex);
    }
    minimizeWindow() {
        if (this.isElectron && window.electronAPI) {
            window.electronAPI.minimizeWindow();
        }
    }
    maximizeWindow() {
        if (this.isElectron && window.electronAPI) {
            window.electronAPI.maximizeWindow();
        }
    }
    closeWindow() {
        if (this.isElectron && window.electronAPI) {
            window.electronAPI.closeWindow();
        }
    }
}
TitleBarComponent.ɵfac = function TitleBarComponent_Factory(t) { return new (t || TitleBarComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_md_navigation_service__WEBPACK_IMPORTED_MODULE_2__["MdNavigationService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_layout_service__WEBPACK_IMPORTED_MODULE_5__["LayoutService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_6__["MdServerMessagesService"])); };
TitleBarComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: TitleBarComponent, selectors: [["app-title-bar"]], decls: 13, vars: 6, consts: [[1, "title-bar"], [1, "app-info"], ["src", "/assets/MdIcon.png", "alt", "MdExplorer", 1, "app-icon"], [1, "app-title"], [1, "app-version"], ["class", "navigation-controls", 4, "ngIf"], [4, "ngIf"], [1, "drag-region"], ["class", "welcome-message", 4, "ngIf"], ["class", "window-controls", 4, "ngIf"], [1, "navigation-controls"], ["mat-icon-button", "", 1, "nav-button", 3, "disabled", "click"], [1, "welcome-message"], [1, "welcome-text"], [1, "welcome-subtitle"], [1, "window-controls"], ["mat-icon-button", "", 1, "window-control", 3, "click"], ["mat-icon-button", "", 1, "window-control", "close-button", 3, "click"]], template: function TitleBarComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](2, "img", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "MdExplorer");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "span", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](7, TitleBarComponent_div_7_Template, 7, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](8, TitleBarComponent_app_search_box_8_Template, 1, 0, "app-search-box", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, TitleBarComponent_app_compatibility_mode_badge_9_Template, 1, 0, "app-compatibility-mode-badge", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, TitleBarComponent_div_11_Template, 5, 0, "div", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, TitleBarComponent_div_12_Template, 10, 0, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("v", ctx.version, "");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isSidenavOpen && ctx.isProjectOpened);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isProjectOpened);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isProjectOpened);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isProjectsPage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isElectron);
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__["MatIcon"], _search_box_search_box_component__WEBPACK_IMPORTED_MODULE_10__["SearchBoxComponent"], _md_explorer_components_compatibility_mode_badge_compatibility_mode_badge_component__WEBPACK_IMPORTED_MODULE_11__["CompatibilityModeBadgeComponent"]], styles: [".title-bar[_ngcontent-%COMP%] {\n  position: fixed;\n  top: 0;\n  left: 0;\n  right: 0;\n  height: 36px;\n  background-color: #1976d2;\n  border-bottom: 1px solid #1565c0;\n  display: flex;\n  align-items: center;\n  z-index: 1000;\n  -webkit-user-select: none;\n  user-select: none;\n}\n\n.app-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 0 12px;\n  -webkit-app-region: no-drag;\n}\n\n.app-icon[_ngcontent-%COMP%] {\n  width: 20px;\n  height: 20px;\n  margin-right: 8px;\n}\n\n.app-title[_ngcontent-%COMP%] {\n  font-size: 14px;\n  font-weight: 500;\n  color: white;\n}\n\n.app-version[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: rgba(255, 255, 255, 0.7);\n  margin-left: 8px;\n}\n\n.navigation-controls[_ngcontent-%COMP%] {\n  display: flex;\n  margin-left: 20px;\n  -webkit-app-region: no-drag;\n}\n\n.navigation-controls[_ngcontent-%COMP%]   .nav-button[_ngcontent-%COMP%] {\n  width: 30px;\n  height: 30px;\n  min-width: 30px;\n  padding: 0;\n  line-height: 30px;\n  color: white;\n}\n\n.navigation-controls[_ngcontent-%COMP%]   .nav-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  line-height: 18px;\n}\n\n.navigation-controls[_ngcontent-%COMP%]   .nav-button[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n}\n\n.drag-region[_ngcontent-%COMP%] {\n  flex: 1;\n  height: 100%;\n  -webkit-app-region: drag;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n}\n\n.welcome-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  color: white;\n}\n\n.welcome-message[_ngcontent-%COMP%]   .welcome-text[_ngcontent-%COMP%] {\n  font-size: 14px;\n  font-weight: 500;\n}\n\n.welcome-message[_ngcontent-%COMP%]   .welcome-subtitle[_ngcontent-%COMP%] {\n  font-size: 12px;\n  opacity: 0.8;\n  padding-left: 12px;\n  border-left: 1px solid rgba(255, 255, 255, 0.3);\n}\n\n.window-controls[_ngcontent-%COMP%] {\n  display: flex;\n  -webkit-app-region: no-drag;\n}\n\n.window-control[_ngcontent-%COMP%] {\n  width: 46px;\n  height: 36px;\n  border: none;\n  background: transparent;\n  color: white;\n  transition: background-color 0.2s;\n}\n\n.window-control[_ngcontent-%COMP%]:hover {\n  background-color: rgba(255, 255, 255, 0.1);\n}\n\n.window-control[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  line-height: 16px;\n}\n\n.close-button[_ngcontent-%COMP%]:hover {\n  background-color: #e81123;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFx0aXRsZS1iYXIuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxlQUFBO0VBQ0EsTUFBQTtFQUNBLE9BQUE7RUFDQSxRQUFBO0VBQ0EsWUFBQTtFQUNBLHlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxpQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsZUFBQTtFQUNBLDJCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxZQUFBO0FBQ0Y7O0FBRUE7RUFDRSxlQUFBO0VBQ0EsK0JBQUE7RUFDQSxnQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLGlCQUFBO0VBQ0EsMkJBQUE7QUFDRjs7QUFDRTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsZUFBQTtFQUNBLFVBQUE7RUFDQSxpQkFBQTtFQUNBLFlBQUE7QUFDSjs7QUFDSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBQ047O0FBRUk7RUFDRSxZQUFBO0FBQU47O0FBS0E7RUFDRSxPQUFBO0VBQ0EsWUFBQTtFQUNBLHdCQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7QUFGRjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxZQUFBO0FBRkY7O0FBSUU7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7QUFGSjs7QUFLRTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSwrQ0FBQTtBQUhKOztBQU9BO0VBQ0UsYUFBQTtFQUNBLDJCQUFBO0FBSkY7O0FBT0E7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLFlBQUE7RUFDQSx1QkFBQTtFQUNBLFlBQUE7RUFDQSxpQ0FBQTtBQUpGOztBQU1FO0VBQ0UsMENBQUE7QUFKSjs7QUFPRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBTEo7O0FBVUU7RUFDRSx5QkFBQTtFQUNBLFlBQUE7QUFQSiIsImZpbGUiOiJ0aXRsZS1iYXIuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIudGl0bGUtYmFyIHtcclxuICBwb3NpdGlvbjogZml4ZWQ7XHJcbiAgdG9wOiAwO1xyXG4gIGxlZnQ6IDA7XHJcbiAgcmlnaHQ6IDA7XHJcbiAgaGVpZ2h0OiAzNnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICMxOTc2ZDI7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICMxNTY1YzA7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIHotaW5kZXg6IDEwMDA7XHJcbiAgLXdlYmtpdC11c2VyLXNlbGVjdDogbm9uZTtcclxuICB1c2VyLXNlbGVjdDogbm9uZTtcclxufVxyXG5cclxuLmFwcC1pbmZvIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogMCAxMnB4O1xyXG4gIC13ZWJraXQtYXBwLXJlZ2lvbjogbm8tZHJhZztcclxufVxyXG5cclxuLmFwcC1pY29uIHtcclxuICB3aWR0aDogMjBweDtcclxuICBoZWlnaHQ6IDIwcHg7XHJcbiAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcbn1cclxuXHJcbi5hcHAtdGl0bGUge1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIGNvbG9yOiB3aGl0ZTtcclxufVxyXG5cclxuLmFwcC12ZXJzaW9uIHtcclxuICBmb250LXNpemU6IDEycHg7XHJcbiAgY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC43KTtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG59XHJcblxyXG4ubmF2aWdhdGlvbi1jb250cm9scyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBtYXJnaW4tbGVmdDogMjBweDtcclxuICAtd2Via2l0LWFwcC1yZWdpb246IG5vLWRyYWc7XHJcbiAgXHJcbiAgLm5hdi1idXR0b24ge1xyXG4gICAgd2lkdGg6IDMwcHg7XHJcbiAgICBoZWlnaHQ6IDMwcHg7XHJcbiAgICBtaW4td2lkdGg6IDMwcHg7XHJcbiAgICBwYWRkaW5nOiAwO1xyXG4gICAgbGluZS1oZWlnaHQ6IDMwcHg7XHJcbiAgICBjb2xvcjogd2hpdGU7XHJcbiAgICBcclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAxOHB4O1xyXG4gICAgICB3aWR0aDogMThweDtcclxuICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICBsaW5lLWhlaWdodDogMThweDtcclxuICAgIH1cclxuICAgIFxyXG4gICAgJjpkaXNhYmxlZCB7XHJcbiAgICAgIG9wYWNpdHk6IDAuNDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5kcmFnLXJlZ2lvbiB7XHJcbiAgZmxleDogMTtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgLXdlYmtpdC1hcHAtcmVnaW9uOiBkcmFnO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxufVxyXG5cclxuLndlbGNvbWUtbWVzc2FnZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTJweDtcclxuICBjb2xvcjogd2hpdGU7XHJcblxyXG4gIC53ZWxjb21lLXRleHQge1xyXG4gICAgZm9udC1zaXplOiAxNHB4O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICB9XHJcblxyXG4gIC53ZWxjb21lLXN1YnRpdGxlIHtcclxuICAgIGZvbnQtc2l6ZTogMTJweDtcclxuICAgIG9wYWNpdHk6IDAuODtcclxuICAgIHBhZGRpbmctbGVmdDogMTJweDtcclxuICAgIGJvcmRlci1sZWZ0OiAxcHggc29saWQgcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjMpO1xyXG4gIH1cclxufVxyXG5cclxuLndpbmRvdy1jb250cm9scyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICAtd2Via2l0LWFwcC1yZWdpb246IG5vLWRyYWc7XHJcbn1cclxuXHJcbi53aW5kb3ctY29udHJvbCB7XHJcbiAgd2lkdGg6IDQ2cHg7XHJcbiAgaGVpZ2h0OiAzNnB4O1xyXG4gIGJvcmRlcjogbm9uZTtcclxuICBiYWNrZ3JvdW5kOiB0cmFuc3BhcmVudDtcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgdHJhbnNpdGlvbjogYmFja2dyb3VuZC1jb2xvciAwLjJzO1xyXG4gIFxyXG4gICY6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjEpO1xyXG4gIH1cclxuICBcclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDE2cHg7XHJcbiAgICB3aWR0aDogMTZweDtcclxuICAgIGhlaWdodDogMTZweDtcclxuICAgIGxpbmUtaGVpZ2h0OiAxNnB4O1xyXG4gIH1cclxufVxyXG5cclxuLmNsb3NlLWJ1dHRvbiB7XHJcbiAgJjpob3ZlciB7XHJcbiAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZTgxMTIzO1xyXG4gICAgY29sb3I6IHdoaXRlO1xyXG4gIH1cclxufSJdfQ== */"] });


/***/ }),

/***/ "8zqs":
/*!****************************************************************************!*\
  !*** ./src/app/git/dialogs/git-token-dialog/git-token-dialog.component.ts ***!
  \****************************************************************************/
/*! exports provided: GitTokenDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitTokenDialogComponent", function() { return GitTokenDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/expansion */ "7EHt");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");














function GitTokenDialogComponent_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Controllo token esistente...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitTokenDialogComponent_div_4_div_9_div_4_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r4.tokenUsername);
} }
function GitTokenDialogComponent_div_4_div_9_Template(rf, ctx) { if (rf & 1) {
    const _r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, GitTokenDialogComponent_div_4_div_9_div_4_Template, 5, 1, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "div", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "span", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_div_4_div_9_Template_button_click_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r6); const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r5.deleteToken(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("valid", ctx_r3.tokenValid)("invalid", !ctx_r3.tokenValid);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r3.tokenValid ? "check_circle" : "error", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.tokenUsername);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" Token: ", ctx_r3.existingMaskedToken, " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("valid", ctx_r3.tokenValid)("invalid", !ctx_r3.tokenValid);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" (", ctx_r3.tokenValid ? "Valido" : "Non valido o scaduto", ") ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r3.isDeleting);
} }
function GitTokenDialogComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, " Per creare automaticamente repository su GitHub, \u00E8 necessario un Personal Access Token. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "p", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, " Il token verr\u00E0 salvato in modo sicuro nel database locale. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, GitTokenDialogComponent_div_4_div_9_Template, 12, 13, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-form-field", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "GitHub Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitTokenDialogComponent_div_4_Template_input_ngModelChange_13_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r8); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r7.token = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_div_4_Template_button_click_14_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r8); const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r9.hideToken = !ctx_r9.hideToken; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "Inserisci un token con permessi 'repo'");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "mat-expansion-panel", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "mat-expansion-panel-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "mat-panel-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](23, "help_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](24, " Come creare un Personal Access Token ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](25, "ol");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](26, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](27, "Vai su GitHub Settings \u2192 Developer settings \u2192 Personal access tokens");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](29, "Clicca su \"Generate new token (classic)\"");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, "Dai un nome descrittivo (es. \"MdExplorer\")");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](32, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](33, "Seleziona l'ambito ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](34, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](35, "repo");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](36, " (accesso completo ai repository privati)");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](37, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](38, "Clicca su \"Generate token\"");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](39, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](40, "Copia il token e incollalo qui sopra");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](41, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_div_4_Template_button_click_41_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r8); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r10.openGitHubTokenPage(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](42, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](43, "open_in_new");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](44, " Apri pagina GitHub Token ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](45, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](46, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](47, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](48, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](49, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](50, "Importante:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](51, " Non condividere mai il tuo token con altri. Trattalo come una password. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.hasExistingToken && !ctx_r1.token);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r1.token)("type", ctx_r1.hideToken ? "password" : "text")("disabled", ctx_r1.isSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r1.isSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r1.hideToken ? "visibility" : "visibility_off");
} }
function GitTokenDialogComponent_mat_spinner_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "mat-spinner", 25);
} }
class GitTokenDialogComponent {
    constructor(dialogRef, data, gitService, snackBar) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitService = gitService;
        this.snackBar = snackBar;
        this.token = '';
        this.hideToken = true;
        this.isLoading = false;
        this.isSaving = false;
        this.isDeleting = false;
        this.hasExistingToken = false;
        this.existingMaskedToken = '';
        this.tokenValid = false;
        this.tokenUsername = '';
    }
    ngOnInit() {
        this.checkExistingToken();
    }
    checkExistingToken() {
        this.isLoading = true;
        this.gitService.getGitHubToken().subscribe({
            next: (result) => {
                this.hasExistingToken = result.hasToken;
                this.existingMaskedToken = result.maskedToken || '';
                this.tokenValid = result.tokenValid;
                this.tokenUsername = result.username || '';
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error checking token:', err);
                this.isLoading = false;
            }
        });
    }
    deleteToken() {
        const message = this.tokenUsername
            ? `Vuoi eliminare il token GitHub dell'account "${this.tokenUsername}"?`
            : 'Vuoi davvero eliminare il token GitHub salvato?';
        const confirmed = confirm(message);
        if (!confirmed)
            return;
        this.isDeleting = true;
        this.gitService.deleteGitHubToken().subscribe({
            next: () => {
                this.snackBar.open('Token eliminato con successo', 'OK', {
                    duration: 3000,
                    verticalPosition: 'top'
                });
                this.hasExistingToken = false;
                this.existingMaskedToken = '';
                this.tokenValid = false;
                this.tokenUsername = '';
                this.isDeleting = false;
            },
            error: (err) => {
                console.error('Error deleting token:', err);
                this.snackBar.open('Errore nell\'eliminazione del token', 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
                this.isDeleting = false;
            }
        });
    }
    testToken() {
        if (!this.token) {
            this.snackBar.open('Inserisci un token da testare', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        this.isSaving = true;
        // First save the token
        this.gitService.setGitHubToken(this.token).subscribe({
            next: (result) => {
                if (result.tokenValid) {
                    this.snackBar.open('Token valido e configurato con successo!', 'OK', {
                        duration: 3000,
                        verticalPosition: 'top',
                        panelClass: ['success-snackbar']
                    });
                    this.dialogRef.close(true);
                }
                else {
                    this.snackBar.open('Token non valido. Verifica di aver copiato correttamente il token.', 'OK', {
                        duration: 5000,
                        verticalPosition: 'top',
                        panelClass: ['error-snackbar']
                    });
                }
                this.isSaving = false;
            },
            error: (err) => {
                console.error('Error setting token:', err);
                this.snackBar.open('Errore nel salvataggio del token', 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
                this.isSaving = false;
            }
        });
    }
    onCancel() {
        this.dialogRef.close(false);
    }
    openGitHubTokenPage() {
        window.open('https://github.com/settings/tokens/new?scopes=repo', '_blank');
    }
}
GitTokenDialogComponent.ɵfac = function GitTokenDialogComponent_Factory(t) { return new (t || GitTokenDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__["MatSnackBar"])); };
GitTokenDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: GitTokenDialogComponent, selectors: [["app-git-token-dialog"]], decls: 11, vars: 6, consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], [4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], [1, "loading-container"], ["diameter", "30"], [1, "instructions"], [1, "small-text"], ["class", "existing-token", 4, "ngIf"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "ghp_xxxxxxxxxxxxxxxxxxxx", 3, "ngModel", "type", "disabled", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", 3, "disabled", "click"], [1, "instructions-panel"], ["mat-stroked-button", "", "color", "primary", 3, "click"], [1, "warning-message"], [1, "existing-token"], [1, "token-info"], ["class", "username", 4, "ngIf"], [1, "token-details"], [1, "status"], ["mat-icon-button", "", "color", "warn", "matTooltip", "Elimina token", 3, "disabled", "click"], [1, "username"], ["diameter", "20", 2, "display", "inline-block", "margin-right", "8px"]], template: function GitTokenDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Configura GitHub Personal Access Token");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, GitTokenDialogComponent_div_3_Template, 4, 0, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, GitTokenDialogComponent_div_4_Template, 52, 6, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-dialog-actions", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_Template_button_click_6_listener() { return ctx.onCancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "Annulla");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_Template_button_click_8_listener() { return ctx.testToken(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, GitTokenDialogComponent_mat_spinner_9_Template, 1, 0, "mat-spinner", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx.isSaving);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx.token || ctx.isSaving);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isSaving);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx.isSaving ? "Validazione..." : "Salva e Testa Token", " ");
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_4__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_5__["MatButton"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_6__["MatSpinner"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_9__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatSuffix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatHint"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanel"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanelHeader"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanelTitle"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_12__["MatTooltip"]], styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  padding: 20px;\n  min-height: 200px;\n}\n\n.instructions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  padding: 16px;\n  background-color: #e3f2fd;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n\n.instructions[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  margin-top: 2px;\n}\n\n.instructions[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.instructions[_ngcontent-%COMP%]   p.small-text[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #666;\n  margin: 0;\n}\n\n.existing-token[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 8px;\n  margin-bottom: 20px;\n}\n\n.existing-token[_ngcontent-%COMP%]    > mat-icon.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.existing-token[_ngcontent-%COMP%]    > mat-icon.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 1.1em;\n  margin-bottom: 4px;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #666;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #666;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.existing-token[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n\n.instructions-panel[_ngcontent-%COMP%] {\n  margin: 20px 0;\n  box-shadow: none;\n  border: 1px solid #e0e0e0;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  margin: 16px 0;\n  padding-left: 24px;\n  line-height: 1.8;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-top: 12px;\n}\n\n.warning-message[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  padding: 12px;\n  background-color: #fff3cd;\n  border: 1px solid #ffc107;\n  border-radius: 4px;\n  margin-top: 16px;\n}\n\n.warning-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n}\n\n.warning-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  font-size: 0.9em;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px !important;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   mat-spinner[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LXRva2VuLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSxpQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBQ0Y7O0FBQ0U7RUFDRSxjQUFBO0VBQ0EsZUFBQTtBQUNKOztBQUVFO0VBQ0UsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFBSjs7QUFFSTtFQUNFLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLFNBQUE7QUFBTjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBRkY7O0FBS0k7RUFDRSxjQUFBO0FBSE47O0FBTUk7RUFDRSxjQUFBO0FBSk47O0FBUUU7RUFDRSxPQUFBO0FBTko7O0FBUUk7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtBQU5OOztBQVFNO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQU5SOztBQVVJO0VBQ0UsZ0JBQUE7RUFDQSxXQUFBO0FBUk47O0FBV1E7RUFDRSxjQUFBO0FBVFY7O0FBWVE7RUFDRSxjQUFBO0FBVlY7O0FBaUJBO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBZEY7O0FBaUJBO0VBQ0UsY0FBQTtFQUNBLGdCQUFBO0VBQ0EseUJBQUE7QUFkRjs7QUFnQkU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7QUFkSjs7QUFnQkk7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7QUFkTjs7QUFrQkU7RUFDRSxjQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtBQWhCSjs7QUFrQkk7RUFDRSxrQkFBQTtBQWhCTjs7QUFvQkU7RUFDRSxnQkFBQTtBQWxCSjs7QUFzQkE7RUFDRSxhQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtBQW5CRjs7QUFxQkU7RUFDRSxjQUFBO0FBbkJKOztBQXNCRTtFQUNFLFNBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFwQko7O0FBd0JBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtBQXJCRjs7QUF3QkE7RUFDRSw2QkFBQTtBQXJCRjs7QUF1QkU7RUFDRSxzQkFBQTtBQXJCSiIsImZpbGUiOiJnaXQtdG9rZW4tZGlhbG9nLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgZ2FwOiAxNnB4O1xyXG4gIHBhZGRpbmc6IDIwcHg7XHJcbiAgbWluLWhlaWdodDogMjAwcHg7XHJcbn1cclxuXHJcbi5pbnN0cnVjdGlvbnMge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDE2cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2UzZjJmZDtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICBtYXJnaW4tdG9wOiAycHg7XHJcbiAgfVxyXG5cclxuICBwIHtcclxuICAgIG1hcmdpbjogMCAwIDhweCAwO1xyXG4gICAgY29sb3I6ICM0MjQyNDI7XHJcbiAgICBsaW5lLWhlaWdodDogMS41O1xyXG5cclxuICAgICYuc21hbGwtdGV4dCB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgICBtYXJnaW46IDA7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4uZXhpc3RpbmctdG9rZW4ge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDhweDtcclxuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xyXG5cclxuICA+IG1hdC1pY29uIHtcclxuICAgICYudmFsaWQge1xyXG4gICAgICBjb2xvcjogIzRjYWY1MDtcclxuICAgIH1cclxuXHJcbiAgICAmLmludmFsaWQge1xyXG4gICAgICBjb2xvcjogI2Y0NDMzNjtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC50b2tlbi1pbmZvIHtcclxuICAgIGZsZXg6IDE7XHJcblxyXG4gICAgLnVzZXJuYW1lIHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgZ2FwOiA0cHg7XHJcbiAgICAgIGZvbnQtc2l6ZTogMS4xZW07XHJcbiAgICAgIG1hcmdpbi1ib3R0b206IDRweDtcclxuXHJcbiAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLnRva2VuLWRldGFpbHMge1xyXG4gICAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gICAgICBjb2xvcjogIzY2NjtcclxuXHJcbiAgICAgIC5zdGF0dXMge1xyXG4gICAgICAgICYudmFsaWQge1xyXG4gICAgICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAmLmludmFsaWQge1xyXG4gICAgICAgICAgY29sb3I6ICNmNDQzMzY7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4uZnVsbC13aWR0aCB7XHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxufVxyXG5cclxuLmluc3RydWN0aW9ucy1wYW5lbCB7XHJcbiAgbWFyZ2luOiAyMHB4IDA7XHJcbiAgYm94LXNoYWRvdzogbm9uZTtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICBtYXQtcGFuZWwtdGl0bGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIGZvbnQtc2l6ZTogMC45NWVtO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgb2wge1xyXG4gICAgbWFyZ2luOiAxNnB4IDA7XHJcbiAgICBwYWRkaW5nLWxlZnQ6IDI0cHg7XHJcbiAgICBsaW5lLWhlaWdodDogMS44O1xyXG5cclxuICAgIGxpIHtcclxuICAgICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIG1hcmdpbi10b3A6IDEycHg7XHJcbiAgfVxyXG59XHJcblxyXG4ud2FybmluZy1tZXNzYWdlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGdhcDogMTJweDtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmZmYzY2Q7XHJcbiAgYm9yZGVyOiAxcHggc29saWQgI2ZmYzEwNztcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLXRvcDogMTZweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICNmZjk4MDA7XHJcbiAgfVxyXG5cclxuICBwIHtcclxuICAgIG1hcmdpbjogMDtcclxuICAgIGNvbG9yOiAjNDI0MjQyO1xyXG4gICAgZm9udC1zaXplOiAwLjllbTtcclxuICB9XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctY29udGVudCB7XHJcbiAgbWluLXdpZHRoOiA1MDBweDtcclxuICBtYXgtd2lkdGg6IDYwMHB4O1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweCAhaW1wb3J0YW50O1xyXG5cclxuICBtYXQtc3Bpbm5lciB7XHJcbiAgICB2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO1xyXG4gIH1cclxufSJdfQ== */"] });


/***/ }),

/***/ "9LnC":
/*!******************************************************************************!*\
  !*** ./src/app/signalR/dialogs/connection-lost/connection-lost.component.ts ***!
  \******************************************************************************/
/*! exports provided: ConnectionLostComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ConnectionLostComponent", function() { return ConnectionLostComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");







function ConnectionLostComponent_div_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "img", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function ConnectionLostComponent_div_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "h1");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "Server down!");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, " Close the MdExplorer tab browser ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function ConnectionLostComponent_button_4_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ConnectionLostComponent_button_4_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r4); const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r3.refresh(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "re-link ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
class ConnectionLostComponent {
    constructor(data, gitservice, dialogRef) {
        this.data = data;
        this.gitservice = gitservice;
        this.dialogRef = dialogRef;
        //private _this: any;
        this._HideImg = true;
        this.consoleIsClosed = false;
        console.log('MAT_DIALOG_DATA = ' + data);
        if (data === 'serverIsDown') {
            this.consoleIsClosed = true;
        }
        dialogRef.disableClose = true;
    }
    ngOnInit() {
    }
    refresh() {
        //this.monitorMDService.startConnection();
        // Trigger a manual refresh by calling checkConnection on the modern Git service
        // Note: The observable subscription will automatically update the UI
        this.dialogRef.close();
    }
}
ConnectionLostComponent.ɵfac = function ConnectionLostComponent_Factory(t) { return new (t || ConnectionLostComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"])); };
ConnectionLostComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ConnectionLostComponent, selectors: [["app-connection-lost"]], decls: 5, vars: 3, consts: [[4, "ngIf"], ["mat-button", "", "color", "primary", 3, "click", 4, "ngIf"], ["src", "/assets/ConnectionLost.png", 2, "left", "0px", "right", "0px", "margin-left", "auto", "width", "500px", "margin-right", "auto"], ["mat-button", "", "color", "primary", 3, "click"]], template: function ConnectionLostComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, ConnectionLostComponent_div_1_Template, 2, 0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](2, ConnectionLostComponent_div_2_Template, 4, 0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, ConnectionLostComponent_button_4_Template, 4, 0, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.consoleIsClosed);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.consoleIsClosed);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.consoleIsClosed);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_3__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_4__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJjb25uZWN0aW9uLWxvc3QuY29tcG9uZW50LnNjc3MifQ== */"] });


/***/ }),

/***/ "AytR":
/*!*****************************************!*\
  !*** ./src/environments/environment.ts ***!
  \*****************************************/
/*! exports provided: environment */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "environment", function() { return environment; });
// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.
const environment = {
    production: false,
    firebase: {
        apiKey: "AIzaSyDN7WEySybw7YGeIxNPhca0r13m_Ynv7Cw",
        authDomain: "mdexplorer-chat.firebaseapp.com",
        databaseURL: "https://mdexplorer-chat-default-rtdb.europe-west1.firebasedatabase.app",
        projectId: "mdexplorer-chat",
        storageBucket: "mdexplorer-chat.firebasestorage.app",
        messagingSenderId: "651311255174",
        appId: "1:651311255174:web:4a458b51e35e13c9054b07",
        measurementId: "G-TFT73YSGQ3"
    }
};
/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.


/***/ }),

/***/ "CqLH":
/*!*******************************************************************************!*\
  !*** ./src/app/signalR/dialogs/plantuml-working/plantuml-working.provider.ts ***!
  \*******************************************************************************/
/*! exports provided: PlantumlWorkingProvider */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "PlantumlWorkingProvider", function() { return PlantumlWorkingProvider; });
/* harmony import */ var _plantuml_working_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./plantuml-working.component */ "ggj0");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");



class PlantumlWorkingProvider {
    constructor(dialog) {
        this.dialog = dialog;
    }
    show(data) {
        this._dialogRef = this.dialog.open(_plantuml_working_component__WEBPACK_IMPORTED_MODULE_0__["PlantumlWorkingComponent"], {
            data: data
        });
        return this;
    }
    hide(data) {
        this._dialogRef.close();
    }
}
PlantumlWorkingProvider.ɵfac = function PlantumlWorkingProvider_Factory(t) { return new (t || PlantumlWorkingProvider)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialog"])); };
PlantumlWorkingProvider.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: PlantumlWorkingProvider, factory: PlantumlWorkingProvider.ɵfac });


/***/ }),

/***/ "DEjE":
/*!***************************************************************************!*\
  !*** ./src/app/commons/components/show-file-system/show-file-metadata.ts ***!
  \***************************************************************************/
/*! exports provided: ShowFileMetadata */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ShowFileMetadata", function() { return ShowFileMetadata; });
class ShowFileMetadata {
    constructor() {
    }
}


/***/ }),

/***/ "FGkB":
/*!********************************************************!*\
  !*** ./src/app/md-explorer/services/layout.service.ts ***!
  \********************************************************/
/*! exports provided: LayoutService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "LayoutService", function() { return LayoutService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");


class LayoutService {
    constructor() {
        this.sidenavWidthSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](240);
        this.sidenavWidth$ = this.sidenavWidthSubject.asObservable();
        this.sidenavOpenSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](true);
        this.sidenavOpen$ = this.sidenavOpenSubject.asObservable();
    }
    setSidenavWidth(width) {
        this.sidenavWidthSubject.next(width);
    }
    getSidenavWidth() {
        return this.sidenavWidthSubject.value;
    }
    setSidenavOpen(isOpen) {
        this.sidenavOpenSubject.next(isOpen);
    }
    getSidenavOpen() {
        return this.sidenavOpenSubject.value;
    }
}
LayoutService.ɵfac = function LayoutService_Factory(t) { return new (t || LayoutService)(); };
LayoutService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: LayoutService, factory: LayoutService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "Ivbh":
/*!*************************************************!*\
  !*** ./src/app/services/url-handler.service.ts ***!
  \*************************************************/
/*! exports provided: UrlHandlerService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "UrlHandlerService", function() { return UrlHandlerService; });
/* harmony import */ var _projects_dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../projects/dialogs/modern-clone-project/modern-clone-project.component */ "zabz");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../md-explorer/services/md-file.service */ "xmhS");








/**
 * Service to handle mdexplorer:// URL commands received via SignalR
 */
class UrlHandlerService {
    constructor(router, dialog, snackBar, mdServerMessages, projectsService, mdFileService) {
        this.router = router;
        this.dialog = dialog;
        this.snackBar = snackBar;
        this.mdServerMessages = mdServerMessages;
        this.projectsService = projectsService;
        this.mdFileService = mdFileService;
        this.initialized = false;
        /**
         * Flag to indicate that a URL handler command is pending.
         * When true, the landing page should NOT be opened automatically.
         */
        this.skipLandingPage = false;
    }
    /**
     * Initialize the URL handler listeners. Should be called once on app startup.
     */
    initialize() {
        if (this.initialized) {
            return;
        }
        console.log('[UrlHandler] Initializing URL handler service');
        // Listen for open document commands
        this.mdServerMessages.addUrlHandlerOpenDocumentListener((data, _) => {
            this.handleOpenDocument(data);
        }, this);
        // Listen for configproject dialog commands (formerly clone)
        this.mdServerMessages.addUrlHandlerOpenConfigProjectDialogListener((data, _) => {
            this.handleOpenConfigProjectDialog(data);
        }, this);
        // Listen for error messages
        this.mdServerMessages.addUrlHandlerErrorListener((data, _) => {
            this.handleError(data);
        }, this);
        this.initialized = true;
        console.log('[UrlHandler] URL handler service initialized');
    }
    /**
     * Handle open document command
     */
    handleOpenDocument(data) {
        var _a;
        console.log('[UrlHandler] ========== handleOpenDocument called ==========');
        console.log('[UrlHandler] Data received:', JSON.stringify(data));
        // Set flag to skip landing page - we're opening a specific document
        this.skipLandingPage = true;
        console.log('[UrlHandler] skipLandingPage set to true');
        // Data structure from backend:
        // {
        //   projectId: string,
        //   projectName: string,
        //   projectPath: string,
        //   filePath: string (relative),
        //   fullPath: string (absolute),
        //   section: string (optional anchor)
        // }
        // Normalize paths for comparison (replace backslashes with forward slashes)
        const normalizedProjectPath = (_a = data.projectPath) === null || _a === void 0 ? void 0 : _a.replace(/\\/g, '/').toLowerCase();
        // Set up listener for indexing complete BEFORE setting the project
        // This ensures we catch the event even if indexing is fast
        let indexingCompleteReceived = false;
        const indexingCompleteHandler = (indexData, _) => {
            console.log('[UrlHandler] folderIndexingComplete received:', indexData);
            indexingCompleteReceived = true;
        };
        this.mdServerMessages.addFolderIndexingCompleteListener(indexingCompleteHandler, this);
        // First, set the project as current
        console.log('[UrlHandler] Setting project:', data.projectPath);
        this.projectsService.setNewFolderProject(data.projectPath);
        // Wait for project to be set, then navigate to the document
        console.log('[UrlHandler] Subscribing to currentProjects$...');
        const subscription = this.projectsService.currentProjects$.subscribe(project => {
            var _a;
            console.log('[UrlHandler] currentProjects$ emitted:', project ? project.path : 'null');
            if (project) {
                const normalizedCurrentPath = (_a = project.path) === null || _a === void 0 ? void 0 : _a.replace(/\\/g, '/').toLowerCase();
                console.log('[UrlHandler] Comparing paths:');
                console.log('[UrlHandler]   - Current project path (normalized):', normalizedCurrentPath);
                console.log('[UrlHandler]   - Expected project path (normalized):', normalizedProjectPath);
                console.log('[UrlHandler]   - Match:', normalizedCurrentPath === normalizedProjectPath);
                if (normalizedCurrentPath === normalizedProjectPath) {
                    console.log('[UrlHandler] Project matched! Navigating to document view...');
                    subscription.unsubscribe();
                    console.log('[UrlHandler] Unsubscribed from currentProjects$');
                    // Navigate to the document view
                    this.router.navigate(['/main/navigation/document']).then(() => {
                        console.log('[UrlHandler] Navigation complete, waiting for indexing to complete...');
                        // Wait for indexing to complete before selecting the file
                        this.waitForIndexingComplete(indexingCompleteReceived, () => {
                            console.log('[UrlHandler] Indexing complete, selecting file...');
                            this.selectFile(data.fullPath, data.filePath, data.section);
                        });
                    });
                }
            }
        });
        this.snackBar.open(`Opening ${data.filePath}...`, 'OK', {
            duration: 3000
        });
    }
    /**
     * Wait for indexing to complete, with timeout fallback
     */
    waitForIndexingComplete(alreadyComplete, callback) {
        if (alreadyComplete) {
            console.log('[UrlHandler] Indexing already complete');
            setTimeout(callback, 500); // Small delay to ensure UI is ready
            return;
        }
        console.log('[UrlHandler] Waiting for folderIndexingComplete event...');
        let completed = false;
        // Set up one-time listener for indexing complete
        const handler = (_data, _) => {
            if (!completed) {
                completed = true;
                console.log('[UrlHandler] folderIndexingComplete event received');
                setTimeout(callback, 500); // Small delay to ensure UI is ready
            }
        };
        this.mdServerMessages.addFolderIndexingCompleteListener(handler, this);
        // Timeout fallback after 10 seconds
        setTimeout(() => {
            if (!completed) {
                completed = true;
                console.log('[UrlHandler] Timeout waiting for indexing, proceeding anyway...');
                callback();
            }
        }, 10000);
    }
    /**
     * Select a file in the tree and optionally scroll to a section
     */
    selectFile(fullPath, relativePath, section) {
        console.log('[UrlHandler] Selecting file:', fullPath, 'relativePath:', relativePath, 'section:', section);
        // Create a minimal MdFile object to search for in the dataStore
        const searchFile = {
            fullPath: fullPath,
            path: fullPath,
            name: fullPath.split(/[/\\]/).pop() || '',
            relativePath: relativePath,
            level: 0,
            expandable: false,
            type: 'mdFile',
            childrens: [],
            index: 0,
            isLoading: false,
            fullDirectoryPath: fullPath.substring(0, Math.max(fullPath.lastIndexOf('/'), fullPath.lastIndexOf('\\')))
        };
        // Try to find the file in the dataStore to get the complete MdFile object
        const foundFile = this.mdFileService.getMdFileFromDataStore(searchFile);
        if (foundFile) {
            console.log('[UrlHandler] Found file in dataStore:', foundFile.fullPath);
            // Set the file as selected - this will trigger the document to load
            this.mdFileService.setSelectedMdFileFromSideNav(foundFile);
            this.mdFileService.setSelectedMdFileFromServer(foundFile);
        }
        else {
            console.log('[UrlHandler] File not found in dataStore, using search file with relativePath:', relativePath);
            // Fallback: use the minimal object with the relativePath from the URL
            this.mdFileService.setSelectedMdFileFromSideNav(searchFile);
            this.mdFileService.setSelectedMdFileFromServer(searchFile);
        }
        // Reset skipLandingPage flag after file selection
        this.skipLandingPage = false;
        console.log('[UrlHandler] skipLandingPage reset to false');
        // If there's a section anchor, scroll to it after the document loads
        if (section) {
            setTimeout(() => {
                this.scrollToSection(section);
            }, 1000); // Give time for the document to render
        }
    }
    /**
     * Scroll to a section anchor in the document
     */
    scrollToSection(section) {
        console.log('[UrlHandler] Scrolling to section:', section);
        // Try to find the element by id
        const element = document.getElementById(section);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
        else {
            console.log('[UrlHandler] Section not found:', section);
        }
    }
    /**
     * Handle configproject dialog command (formerly clone)
     * Opens the clone/configproject dialog with pre-filled data from the shared URL
     */
    handleOpenConfigProjectDialog(data) {
        console.log('[UrlHandler] Opening configproject dialog:', data);
        // Data structure from backend:
        // {
        //   repo: string (repository URL),
        //   branch: string (optional branch name),
        //   user: string (optional username),
        //   basePath: string (optional parent folder for clone destination)
        // }
        // Open the clone dialog with pre-filled data
        const dialogRef = this.dialog.open(_projects_dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_0__["ModernCloneProjectComponent"], {
            width: '600px',
            data: {
                prefilledUrl: data.repo,
                prefilledBranch: data.branch,
                prefilledUser: data.user,
                prefilledBasePath: data.basePath
            }
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                console.log('[UrlHandler] ConfigProject dialog closed with result:', result);
            }
        });
    }
    /**
     * Handle error messages from backend
     */
    handleError(data) {
        console.error('[UrlHandler] Error:', data);
        this.snackBar.open(data.error || 'An error occurred', 'OK', {
            duration: 5000,
            panelClass: ['error-snackbar']
        });
    }
}
UrlHandlerService.ɵfac = function UrlHandlerService_Factory(t) { return new (t || UrlHandlerService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_router__WEBPACK_IMPORTED_MODULE_2__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__["MdFileService"])); };
UrlHandlerService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: UrlHandlerService, factory: UrlHandlerService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "Jfj3":
/*!**************************************!*\
  !*** ./src/app/models/MdSettings.ts ***!
  \**************************************/
/*! exports provided: MdSetting */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdSetting", function() { return MdSetting; });
class MdSetting {
    constructor(init) {
        Object.assign(this, init);
    }
}


/***/ }),

/***/ "MVql":
/*!**********************************************!*\
  !*** ./src/app/git/models/gitlab-setting.ts ***!
  \**********************************************/
/*! exports provided: GitlabSetting */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitlabSetting", function() { return GitlabSetting; });
class GitlabSetting {
}


/***/ }),

/***/ "Mj1F":
/*!**************************************!*\
  !*** ./src/app/shared/animations.ts ***!
  \**************************************/
/*! exports provided: slideInAnimation */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInAnimation", function() { return slideInAnimation; });
/* harmony import */ var _angular_animations__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/animations */ "R0Ic");

const fromProjectsToMain = (fromState, toState) => {
    if (fromState === "void" || fromState === "") {
        return true;
    }
    let fromStart = fromState;
    let toArrive = toState;
    let test1 = fromStart.data;
    let test2 = toArrive.data;
    if (test1.value.animation === "projects" && test2.value.animation === "main") {
        return true;
    }
    return false;
};
const fromMainToProjects = (fromState, toState) => {
    if (fromState === "void" || fromState === "") {
        return true;
    }
    let fromStart = fromState;
    let toArrive = toState;
    let test1 = fromStart.data;
    let test2 = toArrive.data;
    if (test1.value.animation === "main" && test2.value.animation === "projects") {
        return true;
    }
    return false;
};
const slideInAnimation = Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])('routeAnimations', [
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(fromProjectsToMain, [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ position: 'relative' }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter, :leave', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%'
            })
        ], { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '-100%' })
        ], { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':leave', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':leave', [
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('300ms ease-out', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '100%' }))
            ], { optional: true }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter', [
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('300ms ease-out', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '0%' }))
            ], { optional: true }),
        ]),
    ]),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(fromMainToProjects, [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ position: 'relative' }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter, :leave', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%'
            })
        ], { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '100%' })
        ], { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':leave', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':leave', [
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('300ms ease-out', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '-100%' }))
            ], { optional: true }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])(':enter', [
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('300ms ease-out', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ left: '0%' }))
            ], { optional: true }),
        ]),
    ]),
]);


/***/ }),

/***/ "N+BC":
/*!**********************************************************************************!*\
  !*** ./src/app/commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo.ts ***!
  \**********************************************************************************/
/*! exports provided: WaitingDialogInfo */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "WaitingDialogInfo", function() { return WaitingDialogInfo; });
class WaitingDialogInfo {
}


/***/ }),

/***/ "N73s":
/*!****************************************************!*\
  !*** ./src/app/git/services/gitservice.service.ts ***!
  \****************************************************/
/*! exports provided: GITService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GITService", function() { return GITService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../models/gitlab-setting */ "MVql");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "tk/3");





class GITService {
    constructor(http) {
        this.http = http;
        this.gitPollingInterval = null;
        this.ACTIVE_POLLING_INTERVAL = 60000; // 60 secondi quando attivo
        this.INACTIVE_POLLING_INTERVAL = 300000; // 5 minuti quando inattivo
        this.currentProjectPath = null;
        this.currentBranch$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]({
            id: "", name: "",
            somethingIsChangedInTheBranch: true,
            howManyFilesAreChanged: 0,
            fullPath: "",
            howManyCommitAreToPush: 0,
        });
        this.commmitsToPull$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]({
            howManyFilesAreToPull: 0,
            howManyCommitAreToPush: 0,
            somethingIsToPull: false,
            somethingIsToPush: false,
            connectionIsActive: false,
            whatFilesWillBeChanged: []
        });
        this.initializeSmartPolling();
    }
    /**
     * Set the current project path for modern Git operations
     */
    setProjectPath(path) {
        this.currentProjectPath = path;
        // Trigger immediate poll with new path
        if (path) {
            this.performPoll();
        }
    }
    /**
     * Inizializza il polling intelligente che si adatta alla visibilità della finestra
     */
    initializeSmartPolling() {
        // Polling iniziale immediato
        this.performPoll();
        // Avvia polling con intervallo attivo
        this.startPolling(this.ACTIVE_POLLING_INTERVAL);
        // Listener per cambio visibilità finestra
        document.addEventListener('visibilitychange', () => {
            this.handleVisibilityChange();
        });
        // Listener per focus/blur finestra (backup per browser che non supportano visibilitychange)
        window.addEventListener('focus', () => {
            this.handleWindowFocus();
        });
        window.addEventListener('blur', () => {
            this.handleWindowBlur();
        });
    }
    /**
     * Perform polling based on current configuration
     * IMPORTANT: Checks remote status first to authenticate, then fetches Git data using cached credentials
     */
    performPoll() {
        if (this.currentProjectPath) {
            // Step 1: Check remote status first (authenticates and caches credentials)
            this.checkRemoteStatus(this.currentProjectPath).subscribe(remoteStatus => {
                // Only proceed with Git operations if authentication is successful
                if (remoteStatus.hasRemote && remoteStatus.canAuthenticate) {
                    // Step 2: Now fetch Git data (will use cached credentials, no additional auth)
                    this.modernGetBranchStatus(this.currentProjectPath).subscribe(branch => {
                        this.currentBranch$.next(branch);
                    }, error => {
                        console.error('Error in modern branch status:', error);
                        // Set default empty state on error
                        this.currentBranch$.next({
                            id: "", name: "unknown",
                            somethingIsChangedInTheBranch: false,
                            howManyFilesAreChanged: 0,
                            fullPath: this.currentProjectPath,
                            howManyCommitAreToPush: 0
                        });
                    });
                    this.modernGetDataToPull(this.currentProjectPath).subscribe(pullData => {
                        this.commmitsToPull$.next(pullData);
                    }, error => {
                        console.error('Error in modern data to pull:', error);
                    });
                }
                else if (!remoteStatus.hasRemote && remoteStatus.isGitRepository) {
                    // No remote configured - still get branch status but skip pull/push data
                    this.modernGetBranchStatus(this.currentProjectPath).subscribe(branch => {
                        this.currentBranch$.next(branch);
                    }, error => {
                        console.error('Error in modern branch status:', error);
                    });
                }
                else if (!remoteStatus.isGitRepository) {
                    // Not a Git repository - emit empty state to clear UI
                    console.log('📁 Not a Git repository - clearing Git state');
                    this.currentBranch$.next({
                        id: "",
                        name: "",
                        somethingIsChangedInTheBranch: false,
                        howManyFilesAreChanged: 0,
                        fullPath: this.currentProjectPath,
                        howManyCommitAreToPush: 0
                    });
                    this.commmitsToPull$.next({
                        somethingIsToPull: false,
                        somethingIsToPush: false,
                        howManyFilesAreToPull: 0,
                        howManyCommitAreToPush: 0,
                        connectionIsActive: false,
                        whatFilesWillBeChanged: []
                    });
                }
            }, error => {
                console.error('Error checking remote status in poll:', error);
                // On error, also clear Git state
                this.currentBranch$.next({
                    id: "",
                    name: "",
                    somethingIsChangedInTheBranch: false,
                    howManyFilesAreChanged: 0,
                    fullPath: this.currentProjectPath,
                    howManyCommitAreToPush: 0
                });
                this.commmitsToPull$.next({
                    somethingIsToPull: false,
                    somethingIsToPush: false,
                    howManyFilesAreToPull: 0,
                    howManyCommitAreToPush: 0,
                    connectionIsActive: false,
                    whatFilesWillBeChanged: []
                });
            });
        }
    }
    /**
     * Gestisce il cambio di visibilità della finestra
     */
    handleVisibilityChange() {
        if (document.visibilityState === 'visible') {
            this.handleWindowFocus();
        }
        else {
            this.handleWindowBlur();
        }
    }
    /**
     * Quando la finestra diventa attiva: polling più frequente
     */
    handleWindowFocus() {
        this.startPolling(this.ACTIVE_POLLING_INTERVAL);
        // Add short delay to avoid race condition with dialog updates (e.g., branch switch)
        setTimeout(() => {
            this.performPoll();
        }, 500);
    }
    /**
     * Quando la finestra diventa inattiva: polling meno frequente
     */
    handleWindowBlur() {
        this.startPolling(this.INACTIVE_POLLING_INTERVAL);
    }
    /**
     * Avvia polling con intervallo specificato
     */
    startPolling(interval) {
        // Ferma polling esistente
        if (this.gitPollingInterval) {
            clearInterval(this.gitPollingInterval);
        }
        // Avvia nuovo polling
        this.gitPollingInterval = setInterval(() => {
            this.performPoll();
        }, interval);
    }
    /**
     * Ferma completamente il polling (per cleanup)
     */
    stopPolling() {
        if (this.gitPollingInterval) {
            clearInterval(this.gitPollingInterval);
            this.gitPollingInterval = null;
        }
    }
    clone(request) {
        const url = '../api/gitfeatures/cloneRepository';
        return this.http.post(url, request);
    }
    getBranchList() {
        const url = '../api/gitservice/branches';
        return this.http.get(url);
    }
    checkoutSelectedBranch(selected) {
        const url = '../api/gitservice/branches/feat/checkoutBranch';
        return this.http.post(url, selected);
    }
    getTagList() {
        const url = '../api/gitservice/tags';
        return this.http.get(url);
    }
    storeGitlabSettings(user, password, gitlabLink) {
        const url = '../api/gitservice/gitlabsettings';
        let setting = new _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_2__["GitlabSetting"]();
        return this.http.post(url, setting);
    }
    getGitlabSettings() {
        const url = '../api/gitservice/gitlabsettings';
        return this.http.get(url);
    }
    // ===== MODERN GIT METHODS WITH NATIVE AUTHENTICATION =====
    /**
     * Pull using modern Git service with native authentication
     */
    modernPull(projectPath) {
        const request = { ProjectPath: projectPath };
        const url = '../api/ModernGitToolbar/pull';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => this.adaptModernResponseToLegacy(response)));
    }
    /**
     * Commit using modern Git service with native authentication
     */
    modernCommit(projectPath, commitMessage) {
        const request = {
            ProjectPath: projectPath,
            CommitMessage: commitMessage
        };
        const url = '../api/ModernGitToolbar/commit';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => this.adaptModernResponseToLegacy(response)));
    }
    /**
     * Commit and push using modern Git service with native authentication
     */
    modernCommitAndPush(projectPath, commitMessage) {
        const request = {
            ProjectPath: projectPath,
            CommitMessage: commitMessage
        };
        const url = '../api/ModernGitToolbar/commit-and-push';
        console.log('[DEBUG] Sending commit request:', JSON.stringify(request, null, 2));
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => this.adaptModernResponseToLegacy(response)));
    }
    /**
     * Push using modern Git service with native authentication
     */
    modernPush(projectPath) {
        const request = { ProjectPath: projectPath };
        const url = '../api/ModernGitToolbar/push-v2';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => this.adaptModernResponseToLegacy(response)));
    }
    /**
     * Clone repository using modern Git service with native authentication
     */
    modernClone(request) {
        const url = '../api/ModernGit/clone';
        // Convert to PascalCase for C# backend
        const requestBody = {
            Url: request.url,
            LocalPath: request.localPath,
            BranchName: request.branchName || null,
            UseSavedToken: request.useSavedToken !== false,
            Username: request.username || null,
            Password: request.password || null
        };
        console.log('[GITService.modernClone] Sending to backend:', requestBody);
        return this.http.post(url, requestBody).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('[modernClone] Full error:', error);
            // Try to extract validation errors if present
            let errorMessage = 'Clone failed';
            if (error.error) {
                if (typeof error.error === 'string') {
                    errorMessage = error.error;
                }
                else if (error.error.errors) {
                    // Validation errors from ModelState
                    const validationErrors = [];
                    for (const field in error.error.errors) {
                        validationErrors.push(`${field}: ${error.error.errors[field].join(', ')}`);
                    }
                    errorMessage = validationErrors.join('; ');
                }
                else if (error.error.message) {
                    errorMessage = error.error.message;
                }
                else if (error.error.error) {
                    errorMessage = error.error.error;
                }
            }
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ success: false, error: errorMessage });
        }));
    }
    /**
     * Get branch status using modern Git service
     */
    modernGetBranchStatus(projectPath) {
        const url = `../api/ModernGitToolbar/branch-status?projectPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => ({
            id: '',
            name: response.name,
            somethingIsChangedInTheBranch: response.somethingIsChangedInTheBranch,
            howManyFilesAreChanged: response.howManyFilesAreChanged,
            howManyCommitAreToPush: response.howManyCommitAreToPush,
            fullPath: response.fullPath
        })));
    }
    /**
     * Get data to pull/push using modern Git service
     */
    modernGetDataToPull(projectPath) {
        const url = `../api/ModernGitToolbar/get-data-to-pull?projectPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error in modernGetDataToPull:', error);
            // Return empty data on error
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                somethingIsToPull: false,
                somethingIsToPush: false,
                howManyFilesAreToPull: 0,
                howManyCommitAreToPush: 0,
                connectionIsActive: false,
                whatFilesWillBeChanged: []
            });
        }));
    }
    /**
     * Get commit history for a repository
     */
    getCommitHistory(projectPath, maxCommits) {
        const request = {
            repositoryPath: projectPath,
            maxCommits: maxCommits || 50
        };
        const url = '../api/ModernGit/history';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => {
            if (response.success && response.commits) {
                // Convert date strings to Date objects if needed
                return response.commits.map(commit => (Object.assign(Object.assign({}, commit), { date: typeof commit.date === 'string' ? new Date(commit.date) : commit.date, shortHash: commit.hash ? commit.hash.substring(0, 7) : '', isMerge: commit.parents && commit.parents.length > 1 })));
            }
            return [];
        }), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting commit history:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])([]);
        }));
    }
    /**
     * Check if repository has remote configured
     */
    checkRemoteStatus(projectPath) {
        const url = `../api/ModernGit/remote-status?repositoryPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error checking remote status:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                hasRemote: false,
                isGitRepository: false,
                errorMessage: error.message || 'Failed to check remote status',
                canAuthenticate: false
            });
        }));
    }
    /**
     * Get the remote URL for a repository (typically origin)
     * Used by Share Project feature to generate shareable URLs
     */
    getRemoteUrl(projectPath) {
        const url = `../api/ModernGit/remote-url?repositoryPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting remote URL:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                hasRemote: false,
                error: error.message || 'Failed to get remote URL'
            });
        }));
    }
    /**
     * Validate if a remote Git URL is reachable
     * Used before cloning to verify URL is accessible
     */
    validateRemoteUrl(gitUrl) {
        const url = `../api/ModernGit/validate-remote-url?url=${encodeURIComponent(gitUrl)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error validating remote URL:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                isReachable: false,
                error: error.message || 'Failed to validate remote URL'
            });
        }));
    }
    /**
     * Initialize a new Git repository
     */
    initRepository(request) {
        const url = '../api/ModernGit/init';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error initializing Git repository:', error);
            throw error;
        }));
    }
    /**
     * Remove a remote from the repository
     */
    removeRemote(projectPath, remoteName = 'origin') {
        const url = `../api/ModernGit/remove-remote?repositoryPath=${encodeURIComponent(projectPath)}&remoteName=${encodeURIComponent(remoteName)}`;
        return this.http.delete(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error removing remote:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to remove remote'
            });
        }));
    }
    /**
     * Setup GitHub remote for repository (legacy - GitHub specific)
     */
    setupGitHubRemote(projectPath, organization, repositoryName, saveOrganization = true, pushAfterAdd = true, repositoryDescription, isPrivate) {
        const request = {
            repositoryPath: projectPath,
            organization: organization,
            repositoryName: repositoryName,
            repositoryDescription: repositoryDescription,
            isPrivate: isPrivate !== undefined ? isPrivate : true,
            saveOrganization: saveOrganization,
            pushAfterAdd: pushAfterAdd
        };
        const url = '../api/ModernGit/setup-remote';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error setting up remote:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to setup remote'
            });
        }));
    }
    // #region Generic Remote Setup Methods
    /**
     * Parse a remote URL and detect provider
     */
    parseRemoteUrl(url) {
        const apiUrl = '../api/ModernGit/parse-remote-url';
        return this.http.post(apiUrl, { url: url }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error parsing remote URL:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                isValid: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to parse URL'
            });
        }));
    }
    /**
     * Validate remote with credentials
     */
    validateRemoteAuth(request) {
        const url = '../api/ModernGit/validate-remote-auth';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error validating remote auth:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                isReachable: false,
                credentialsValid: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to validate credentials'
            });
        }));
    }
    /**
     * Setup generic remote (supports any Git provider)
     */
    setupRemoteGeneric(request) {
        const url = '../api/ModernGit/setup-remote-generic';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error setting up generic remote:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to setup remote'
            });
        }));
    }
    // #endregion
    /**
     * Get saved GitHub organization
     */
    getGitHubOrganization() {
        const url = '../api/ModernGit/github-organization';
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response.organization || ''), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting GitHub organization:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])('');
        }));
    }
    /**
     * Sets the GitHub personal access token
     */
    setGitHubToken(token) {
        const url = '../api/ModernGit/github-token';
        return this.http.post(url, { token: token }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error setting GitHub token:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ success: false });
        }));
    }
    /**
     * Gets the GitHub token status (masked)
     */
    getGitHubToken() {
        const url = '../api/ModernGit/github-token';
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting GitHub token:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ hasToken: false, maskedToken: '', tokenValid: false });
        }));
    }
    /**
     * Tests the GitHub token validity
     */
    testGitHubToken() {
        const url = '../api/ModernGit/test-github-token';
        return this.http.post(url, {}).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error testing GitHub token:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ success: false, tokenValid: false });
        }));
    }
    /**
     * Deletes the stored GitHub token
     */
    deleteGitHubToken() {
        const url = '../api/ModernGit/github-token';
        return this.http.delete(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error deleting GitHub token:', error);
            throw error;
        }));
    }
    /**
     * Save GitHub organization for future use
     */
    saveGitHubOrganization(organization) {
        const url = '../api/ModernGit/github-organization';
        return this.http.post(url, { organization: organization }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response.success), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error saving GitHub organization:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])(false);
        }));
    }
    /**
     * Get list of all branches (local and remote)
     */
    getBranches(projectPath, includeRemote = true) {
        const url = `../api/ModernGit/branches?repositoryPath=${encodeURIComponent(projectPath)}&includeRemote=${includeRemote}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting branches:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])([]);
        }));
    }
    /**
     * Checkout/switch to a different branch
     * Sends connectionId to backend for client-specific SignalR notifications
     */
    checkoutBranch(projectPath, branchName, connectionId) {
        const url = '../api/ModernGit/checkout';
        const request = {
            repositoryPath: projectPath,
            branchName: branchName,
            connectionId: connectionId || null // Include SignalR connectionId if provided
        };
        if (connectionId) {
            console.log('🔄 Checkout branch request with connectionId:', connectionId);
        }
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error checking out branch:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                error: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || error.message || 'Failed to checkout branch'
            });
        }));
    }
    /**
     * Get repository status (for checking uncommitted changes)
     */
    getRepositoryStatus(projectPath) {
        const url = `../api/ModernGit/status?repositoryPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting repository status:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ hasChanges: false, files: [] });
        }));
    }
    /**
     * Get list of all changed files in the repository
     */
    getChangedFiles(projectPath) {
        const url = `../api/ModernGitToolbar/changed-files?projectPath=${encodeURIComponent(projectPath)}`;
        return this.http.get(url).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            console.error('Error getting changed files:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ files: [], totalCount: 0 });
        }));
    }
    /**
     * Discard changes to a file (restore from HEAD) or unstage a new file
     */
    discardFile(projectPath, filePath, isNew) {
        const url = '../api/ModernGitToolbar/discard-file';
        const request = {
            projectPath: projectPath,
            filePath: filePath,
            isNew: isNew
        };
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error discarding file:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                errorMessage: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.errorMessage) || error.message || 'Failed to discard file',
                filePath: filePath
            });
        }));
    }
    /**
     * Delete an untracked/new file from disk
     */
    deleteFile(projectPath, filePath) {
        const url = '../api/ModernGitToolbar/delete-file';
        const request = {
            projectPath: projectPath,
            filePath: filePath,
            isNew: true
        };
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(error => {
            var _a;
            console.error('Error deleting file:', error);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({
                success: false,
                errorMessage: ((_a = error.error) === null || _a === void 0 ? void 0 : _a.errorMessage) || error.message || 'Failed to delete file',
                filePath: filePath
            });
        }));
    }
    /**
     * Adapts modern Git response to legacy format for backward compatibility
     */
    adaptModernResponseToLegacy(response) {
        return {
            isConnectionMissing: false,
            isAuthenticationMissing: false,
            thereAreConflicts: response.thereAreConflicts,
            errorMessage: response.errorMessage,
            whatFilesWillBeChanged: response.changedFiles || []
        };
    }
    // ==================== Git Account Management ====================
    /**
     * Gets all unique usernames for a specific account type (GitHub, GitLab, etc.)
     * Used by the clone UI to show available accounts for a provider
     */
    getUsernamesByType(accountType) {
        return this.http.get(`../api/GitAccount/usernames-by-type?accountType=${encodeURIComponent(accountType)}`).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(err => {
            console.error('[GITService] Error getting usernames by type:', err);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])([]);
        }));
    }
    /**
     * Deletes a git account by ID
     */
    deleteGitAccount(id) {
        return this.http.delete(`../api/GitAccount/${id}`).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["catchError"])(err => {
            var _a;
            console.error('[GITService] Error deleting git account:', err);
            return Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["of"])({ success: false, message: ((_a = err.error) === null || _a === void 0 ? void 0 : _a.error) || 'Failed to delete account' });
        }));
    }
    /**
     * Cleanup quando il service viene distrutto
     */
    ngOnDestroy() {
        this.stopPolling();
        // Rimuovi event listeners per evitare memory leak
        document.removeEventListener('visibilitychange', this.handleVisibilityChange);
        window.removeEventListener('focus', this.handleWindowFocus);
        window.removeEventListener('blur', this.handleWindowBlur);
    }
}
GITService.ɵfac = function GITService_Factory(t) { return new (t || GITService)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClient"])); };
GITService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({ token: GITService, factory: GITService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "QI1B":
/*!**********************************************************!*\
  !*** ./src/app/services/app-current-metadata.service.ts ***!
  \**********************************************************/
/*! exports provided: AppCurrentMetadataService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AppCurrentMetadataService", function() { return AppCurrentMetadataService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _models_MdSettings__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../models/MdSettings */ "Jfj3");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ "tk/3");




class AppCurrentMetadataService {
    constructor(http) {
        this.http = http;
        this.dataStore = {
            folderName: 'test', settings: [new _models_MdSettings__WEBPACK_IMPORTED_MODULE_1__["MdSetting"]({ id: 'test', name: 'PlantumlServer' })]
        };
        this._folderName = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]('test');
        this._Settings = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]([]);
        this.showSidenav = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](true);
    }
    get folderName() {
        return this._folderName.asObservable();
    }
    get settings() {
        return this._Settings.asObservable();
    }
    loadFolderName() {
        const url = '../api/AppSettings/GetCurrentFolder';
        return this.http.get(url)
            .subscribe(data => {
            this.dataStore.folderName = data;
            this._folderName.next(Object.assign({}, this.dataStore).folderName);
        }, error => {
            console.log("failed to fetch working folder name");
        });
    }
    loadSettings() {
        const url = '../api/AppSettings/GetSettings';
        return this.http.get(url)
            .subscribe(data => {
            this.dataStore.settings = data.settings;
            this._Settings.next(Object.assign({}, this.dataStore).settings);
        }, error => {
        });
    }
    saveSettings(updatedSettings) {
        const url = '../api/AppSettings/SetSettings';
        // If updatedSettings are provided, update the dataStore first
        if (updatedSettings) {
            this.dataStore.settings = updatedSettings;
            this._Settings.next(Object.assign({}, this.dataStore).settings);
        }
        // Backend expects { settings: [...] } wrapper
        return this.http.post(url, { settings: this.dataStore.settings });
    }
    killServer() {
        const url = '../api/AppSettings/KillServer';
        return this.http.get(url).subscribe(data => {
        });
    }
}
AppCurrentMetadataService.ɵfac = function AppCurrentMetadataService_Factory(t) { return new (t || AppCurrentMetadataService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__["HttpClient"])); };
AppCurrentMetadataService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: AppCurrentMetadataService, factory: AppCurrentMetadataService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "S2pp":
/*!**********************************************************************************!*\
  !*** ./src/app/commons/waitingdialog/waiting-dialog/waiting-dialog.component.ts ***!
  \**********************************************************************************/
/*! exports provided: WaitingDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "WaitingDialogComponent", function() { return WaitingDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./models/WaitingDialogInfo */ "N+BC");




class WaitingDialogComponent {
    constructor(dialogRef, data) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.message = "<todo>";
        dialogRef.disableClose = true;
        this.message = this.data.message;
    }
    ngOnInit() {
    }
}
WaitingDialogComponent.ɵfac = function WaitingDialogComponent_Factory(t) { return new (t || WaitingDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"])); };
WaitingDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: WaitingDialogComponent, selectors: [["app-waiting-dialog"]], decls: 5, vars: 1, consts: [["src", "https://giphy.com/embed/l0NgQIwNvU9AUuaY0", "width", "120", "height", "120", "frameBorder", "0", "allowFullScreen", "", 1, "giphy-embed"]], template: function WaitingDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "iframe", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "h2");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.message);
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJ3YWl0aW5nLWRpYWxvZy5jb21wb25lbnQuc2NzcyJ9 */"] });


/***/ }),

/***/ "Sy1n":
/*!**********************************!*\
  !*** ./src/app/app.component.ts ***!
  \**********************************/
/*! exports provided: AppComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AppComponent", function() { return AppComponent; });
/* harmony import */ var _shared_animations__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./shared/animations */ "Mj1F");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/platform-browser */ "jhN1");
/* harmony import */ var _services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./services/app-current-metadata.service */ "QI1B");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_ai_notification_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./services/ai-notification.service */ "+Jvq");
/* harmony import */ var _services_url_handler_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./services/url-handler.service */ "Ivbh");
/* harmony import */ var _components_title_bar_title_bar_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./components/title-bar/title-bar.component */ "89FR");








class AppComponent {
    constructor(titleService, currentFolder, route, router, aiNotificationService, urlHandlerService) {
        this.titleService = titleService;
        this.currentFolder = currentFolder;
        this.route = route;
        this.router = router;
        this.aiNotificationService = aiNotificationService;
        this.urlHandlerService = urlHandlerService;
        this.title = 'client2';
        currentFolder.folderName.subscribe((data) => {
            this.titleService.setTitle(data.currentFolder);
        });
        currentFolder.loadFolderName();
        // Initialize URL handler service for mdexplorer:// protocol
        this.urlHandlerService.initialize();
    }
    unloadHandler(event) {
        // E' stato dato il comando di chiusura del tab o di chrome
        // spegni il serverino che si è acceso
        if (performance.navigation.type != performance.navigation.TYPE_RELOAD) {
            //this.currentFolder.killServer();
        }
        //
    }
}
AppComponent.ɵfac = function AppComponent_Factory(t) { return new (t || AppComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__["Title"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_3__["AppCurrentMetadataService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["ActivatedRoute"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_ai_notification_service__WEBPACK_IMPORTED_MODULE_5__["AiNotificationService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_url_handler_service__WEBPACK_IMPORTED_MODULE_6__["UrlHandlerService"])); };
AppComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: AppComponent, selectors: [["app-root"]], hostBindings: function AppComponent_HostBindings(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("unload", function AppComponent_unload_HostBindingHandler($event) { return ctx.unloadHandler($event); }, false, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresolveWindow"]);
    } }, decls: 4, vars: 1, consts: [[1, "container", "app-content"], ["o", "outlet"]], template: function AppComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "app-title-bar");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](2, "router-outlet", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵreference"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("@routeAnimations", _r0.isActivated ? _r0.activatedRoute : "");
    } }, directives: [_components_title_bar_title_bar_component__WEBPACK_IMPORTED_MODULE_7__["TitleBarComponent"], _angular_router__WEBPACK_IMPORTED_MODULE_4__["RouterOutlet"]], styles: [".flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-flow: row wrap;\n  flex-direction: row;\n  flex-wrap: wrap;\n}\n\n.flex-item[_ngcontent-%COMP%] {\n  background: tomato;\n}\n\n.app-content[_ngcontent-%COMP%] {\n  margin-top: 30px;\n  height: calc(100vh - 30px);\n  display: flex;\n  flex-direction: column;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcYXBwLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsbUJBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxrQkFBQTtBQUNGOztBQUVBO0VBQ0UsZ0JBQUE7RUFDQSwwQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtBQUNGIiwiZmlsZSI6ImFwcC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5mbGV4LWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWZsb3c6IHJvdyB3cmFwO1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbiAgZmxleC13cmFwOiB3cmFwOyAgXHJcbn1cclxuXHJcbi5mbGV4LWl0ZW0ge1xyXG4gIGJhY2tncm91bmQ6IHRvbWF0bztcclxufVxyXG5cclxuLmFwcC1jb250ZW50IHtcclxuICBtYXJnaW4tdG9wOiAzMHB4OyAvLyBTcGFjZSBmb3IgdGl0bGUgYmFyXHJcbiAgaGVpZ2h0OiBjYWxjKDEwMHZoIC0gMzBweCk7IC8vIEltcG9zdGEgbCdhbHRlenphIHBlciBvY2N1cGFyZSB0dXR0byBsbyBzcGF6aW8gZGlzcG9uaWJpbGVcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcbiJdfQ== */"], data: { animation: [
            _shared_animations__WEBPACK_IMPORTED_MODULE_0__["slideInAnimation"]
        ] } });


/***/ }),

/***/ "TUMs":
/*!*****************************************************************************!*\
  !*** ./src/app/commons/components/new-directory/new-directory.component.ts ***!
  \*****************************************************************************/
/*! exports provided: NewDirectoryComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NewDirectoryComponent", function() { return NewDirectoryComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");












function NewDirectoryComponent_mat_hint_23_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Press Enter to create");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function NewDirectoryComponent_mat_error_24_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-error");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r2.errorMessage);
} }
function NewDirectoryComponent_div_25_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "visibility");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "Will be created at:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r3.fullPathPreview);
} }
function NewDirectoryComponent_div_26_mat_chip_4_Template(rf, ctx) { if (rf & 1) {
    const _r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-chip", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_div_26_mat_chip_4_Template_mat_chip_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r10); const suggestion_r8 = ctx.$implicit; const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r9.applySuggestion(suggestion_r8); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const suggestion_r8 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", suggestion_r8, " ");
} }
function NewDirectoryComponent_div_26_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "Quick suggestions:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, NewDirectoryComponent_div_26_mat_chip_4_Template, 4, 1, "mat-chip", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r4.suggestions);
} }
function NewDirectoryComponent_mat_spinner_33_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "mat-spinner", 28);
} }
function NewDirectoryComponent_mat_icon_34_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
class NewDirectoryComponent {
    constructor(data, dialogRef, mdFileService) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
        this.directoryName = '';
        this.isCreating = false;
        this.errorMessage = '';
        // Suggerimenti nomi cartelle comuni
        this.suggestions = [
            'docs',
            'images',
            'assets',
            'templates',
            'backup',
            'archive'
        ];
    }
    ngOnInit() {
        // Auto-focus viene gestito tramite autoFocus: true nel dialog config
    }
    /**
     * Preview del path completo
     * Cross-platform: gestisce correttamente sia / che \
     */
    get fullPathPreview() {
        if (!this.directoryName) {
            return this.getParentPath();
        }
        const parentPath = this.getParentPath();
        // Determina il separatore corretto in base al path parent
        const separator = parentPath.includes('\\') ? '\\' : '/';
        return `${parentPath}${separator}${this.directoryName}`;
    }
    /**
     * Ottiene il path parent gestendo entrambi i tipi di data
     */
    getParentPath() {
        if (this.isNewDialogData(this.data)) {
            return this.data.parentPath;
        }
        else {
            return this.data.fullPath || this.data.path;
        }
    }
    /**
     * Ottiene il nome parent gestendo entrambi i tipi di data
     */
    getParentName() {
        if (this.isNewDialogData(this.data)) {
            return this.data.parentName;
        }
        else {
            return this.data.name;
        }
    }
    /**
     * Type guard per distinguere NewDirectoryDialogData da MdFile
     */
    isNewDialogData(data) {
        return data && 'parentPath' in data && 'parentName' in data;
    }
    /**
     * Validazione nome cartella
     * Cross-platform: caratteri non validi per Windows, Linux e Mac
     */
    isValidName() {
        if (!this.directoryName || this.directoryName.trim() === '') {
            return false;
        }
        // Caratteri non validi per file system (Windows, Linux, Mac)
        // Windows: < > : " / \ | ? *
        // Linux/Mac: / (e null byte)
        const invalidChars = /[<>:"|?*\\\/\x00]/;
        if (invalidChars.test(this.directoryName)) {
            return false;
        }
        // Nomi riservati Windows
        const windowsReserved = /^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..+)?$/i;
        if (windowsReserved.test(this.directoryName.trim())) {
            return false;
        }
        // Non può iniziare o finire con spazi o punti
        if (this.directoryName !== this.directoryName.trim()) {
            return false;
        }
        if (this.directoryName.endsWith('.')) {
            return false;
        }
        return true;
    }
    /**
     * Applica un suggerimento
     */
    applySuggestion(suggestion) {
        this.directoryName = suggestion;
        this.errorMessage = '';
    }
    /**
     * Salva la nuova cartella
     */
    save() {
        if (!this.isValidName()) {
            this.errorMessage = 'Invalid folder name. Avoid special characters and reserved names.';
            return;
        }
        this.isCreating = true;
        this.errorMessage = '';
        const parentPath = this.getParentPath();
        const level = this.isNewDialogData(this.data) ? 0 : this.data.level;
        this.mdFileService
            .CreateNewDirectoryEx(parentPath, this.directoryName, level)
            .subscribe({
            next: (result) => {
                this.isCreating = false;
                this.dialogRef.close(result);
            },
            error: (error) => {
                var _a;
                this.isCreating = false;
                const errorMsg = ((_a = error === null || error === void 0 ? void 0 : error.error) === null || _a === void 0 ? void 0 : _a.message) || (error === null || error === void 0 ? void 0 : error.message) || 'Failed to create folder';
                // Messaggi di errore specifici
                if (errorMsg.toLowerCase().includes('already exists')) {
                    this.errorMessage = 'A folder with this name already exists';
                }
                else if (errorMsg.toLowerCase().includes('permission')) {
                    this.errorMessage = 'Permission denied. Check folder permissions.';
                }
                else {
                    this.errorMessage = errorMsg;
                }
            }
        });
    }
    dismiss() {
        this.dialogRef.close();
    }
}
NewDirectoryComponent.ɵfac = function NewDirectoryComponent_Factory(t) { return new (t || NewDirectoryComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
NewDirectoryComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: NewDirectoryComponent, selectors: [["app-new-directory"]], decls: 36, vars: 13, consts: [["mat-dialog-title", "", 1, "dialog-header"], [1, "dialog-icon"], [1, "dialog-container"], [1, "context-info"], [1, "context-icon"], [1, "context-text"], [1, "context-path"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "Enter folder name", "autofocus", "", 3, "ngModel", "disabled", "ngModelChange", "keyup.enter"], ["nameInput", ""], ["matPrefix", ""], [4, "ngIf"], ["class", "path-preview", 4, "ngIf"], ["class", "suggestions", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "class", "button-spinner", 4, "ngIf"], [1, "path-preview"], [1, "preview-icon"], [1, "preview-content"], [1, "preview-label"], [1, "preview-path"], [1, "suggestions"], [1, "suggestions-label"], [1, "suggestions-chips"], ["class", "suggestion-chip", 3, "click", 4, "ngFor", "ngForOf"], [1, "suggestion-chip", 3, "click"], ["diameter", "20", 1, "button-spinner"]], template: function NewDirectoryComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "create_new_folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "Create New Folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-icon", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "info");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "Parent folder:");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-form-field", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "Folder Name");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "input", 8, 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewDirectoryComponent_Template_input_ngModelChange_19_listener($event) { return ctx.directoryName = $event; })("keyup.enter", function NewDirectoryComponent_Template_input_keyup_enter_19_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "mat-icon", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](22, "folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](23, NewDirectoryComponent_mat_hint_23_Template, 2, 0, "mat-hint", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](24, NewDirectoryComponent_mat_error_24_Template, 2, 1, "mat-error", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](25, NewDirectoryComponent_div_25_Template, 8, 1, "div", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](26, NewDirectoryComponent_div_26_Template, 5, 1, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "mat-dialog-actions", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "button", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_28_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](29, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](30, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, " Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](32, "button", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_32_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](33, NewDirectoryComponent_mat_spinner_33_Template, 1, 0, "mat-spinner", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](34, NewDirectoryComponent_mat_icon_34_Template, 2, 0, "mat-icon", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](35);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](13);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx.getParentName(), " ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.fullPathPreview);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.directoryName)("disabled", ctx.isCreating);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.errorMessage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.errorMessage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.directoryName && ctx.isValidName());
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.directoryName && ctx.suggestions.length > 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx.isCreating);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx.isValidName() || ctx.isCreating);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isCreating);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isCreating);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx.isCreating ? "Creating..." : "Create Folder", " ");
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__["MatIcon"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatPrefix"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatError"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgForOf"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__["MatChip"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_10__["MatSpinner"]], styles: [".dialog-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  color: #2196f3;\n}\n\n.dialog-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n}\n\n.dialog-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 20px;\n  padding: 8px 0;\n  min-width: 450px;\n}\n\n.context-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 12px;\n  background: #e3f2fd;\n  border-left: 4px solid #2196f3;\n  border-radius: 4px;\n}\n\n.context-icon[_ngcontent-%COMP%] {\n  color: #2196f3;\n  font-size: 20px;\n  flex-shrink: 0;\n}\n\n.context-text[_ngcontent-%COMP%] {\n  flex: 1;\n  font-size: 0.9rem;\n}\n\n.context-text[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: #1976d2;\n}\n\n.context-path[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 0.8rem;\n  color: #666;\n  margin-top: 4px;\n  word-break: break-all;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.path-preview[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 12px;\n  background: #f5f5f5;\n  border-radius: 4px;\n  border: 1px dashed #ccc;\n  animation: fadeIn 0.3s ease-in;\n}\n\n@keyframes fadeIn {\n  from {\n    opacity: 0;\n    transform: translateY(-5px);\n  }\n  to {\n    opacity: 1;\n    transform: translateY(0);\n  }\n}\n\n.preview-icon[_ngcontent-%COMP%] {\n  color: #4caf50;\n  font-size: 20px;\n  flex-shrink: 0;\n}\n\n.preview-content[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.preview-label[_ngcontent-%COMP%] {\n  font-size: 0.8rem;\n  color: #666;\n  margin-bottom: 4px;\n}\n\n.preview-path[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 0.85rem;\n  color: #333;\n  font-weight: 600;\n  word-break: break-all;\n}\n\n.suggestions[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n  animation: fadeIn 0.3s ease-in;\n}\n\n.suggestions-label[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n  color: #666;\n  font-weight: 500;\n}\n\n.suggestions-chips[_ngcontent-%COMP%] {\n  display: flex;\n  flex-wrap: wrap;\n  gap: 8px;\n}\n\n.suggestion-chip[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.2s ease;\n}\n\n.suggestion-chip[_ngcontent-%COMP%]:hover {\n  background: #e3f2fd;\n  transform: translateY(-2px);\n  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\n}\n\n.suggestion-chip[_ngcontent-%COMP%]:active {\n  transform: translateY(0);\n}\n\n.suggestion-chip[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n\n.button-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  margin-right: 8px;\n}\n\n.button-spinner[_ngcontent-%COMP%]     circle {\n  stroke: white;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  margin: 0;\n}\n\n@media (max-width: 768px) {\n  .dialog-container[_ngcontent-%COMP%] {\n    min-width: unset;\n  }\n\n  .suggestions-chips[_ngcontent-%COMP%] {\n    flex-direction: column;\n  }\n\n  .suggestion-chip[_ngcontent-%COMP%] {\n    width: 100%;\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbmV3LWRpcmVjdG9yeS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxjQUFBO0FBQ0Y7O0FBRUE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLGtCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7RUFDQSxpQkFBQTtBQUNGOztBQUNFO0VBQ0UsY0FBQTtBQUNKOztBQUdBO0VBQ0UscUNBQUE7RUFDQSxpQkFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EscUJBQUE7QUFBRjs7QUFHQTtFQUNFLFdBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLHVCQUFBO0VBQ0EsOEJBQUE7QUFBRjs7QUFHQTtFQUNFO0lBQ0UsVUFBQTtJQUNBLDJCQUFBO0VBQUY7RUFFQTtJQUNFLFVBQUE7SUFDQSx3QkFBQTtFQUFGO0FBQ0Y7O0FBR0E7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7QUFERjs7QUFJQTtFQUNFLE9BQUE7QUFERjs7QUFJQTtFQUNFLGlCQUFBO0VBQ0EsV0FBQTtFQUNBLGtCQUFBO0FBREY7O0FBSUE7RUFDRSxxQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EscUJBQUE7QUFERjs7QUFJQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7RUFDQSw4QkFBQTtBQURGOztBQUlBO0VBQ0Usa0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFERjs7QUFJQTtFQUNFLGFBQUE7RUFDQSxlQUFBO0VBQ0EsUUFBQTtBQURGOztBQUlBO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0FBREY7O0FBR0U7RUFDRSxtQkFBQTtFQUNBLDJCQUFBO0VBQ0Esd0NBQUE7QUFESjs7QUFJRTtFQUNFLHdCQUFBO0FBRko7O0FBS0U7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFISjs7QUFPQTtFQUNFLHFCQUFBO0VBQ0EsaUJBQUE7QUFKRjs7QUFNRTtFQUNFLGFBQUE7QUFKSjs7QUFTQTtFQUNFLGtCQUFBO0VBQ0EsU0FBQTtBQU5GOztBQVVBO0VBQ0U7SUFDRSxnQkFBQTtFQVBGOztFQVVBO0lBQ0Usc0JBQUE7RUFQRjs7RUFVQTtJQUNFLFdBQUE7RUFQRjtBQUNGIiwiZmlsZSI6Im5ldy1kaXJlY3RvcnkuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuZGlhbG9nLWhlYWRlciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTJweDtcclxuICBjb2xvcjogIzIxOTZmMztcclxufVxyXG5cclxuLmRpYWxvZy1pY29uIHtcclxuICBmb250LXNpemU6IDI4cHg7XHJcbiAgd2lkdGg6IDI4cHg7XHJcbiAgaGVpZ2h0OiAyOHB4O1xyXG59XHJcblxyXG4uZGlhbG9nLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMjBweDtcclxuICBwYWRkaW5nOiA4cHggMDtcclxuICBtaW4td2lkdGg6IDQ1MHB4O1xyXG59XHJcblxyXG4uY29udGV4dC1pbmZvIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG4gIGdhcDogMTJweDtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGJhY2tncm91bmQ6ICNlM2YyZmQ7XHJcbiAgYm9yZGVyLWxlZnQ6IDRweCBzb2xpZCAjMjE5NmYzO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxufVxyXG5cclxuLmNvbnRleHQtaWNvbiB7XHJcbiAgY29sb3I6ICMyMTk2ZjM7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG59XHJcblxyXG4uY29udGV4dC10ZXh0IHtcclxuICBmbGV4OiAxO1xyXG4gIGZvbnQtc2l6ZTogMC45cmVtO1xyXG5cclxuICBzdHJvbmcge1xyXG4gICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgfVxyXG59XHJcblxyXG4uY29udGV4dC1wYXRoIHtcclxuICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xyXG4gIGZvbnQtc2l6ZTogMC44cmVtO1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIG1hcmdpbi10b3A6IDRweDtcclxuICB3b3JkLWJyZWFrOiBicmVhay1hbGw7XHJcbn1cclxuXHJcbi5mdWxsLXdpZHRoIHtcclxuICB3aWR0aDogMTAwJTtcclxufVxyXG5cclxuLnBhdGgtcHJldmlldyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBib3JkZXI6IDFweCBkYXNoZWQgI2NjYztcclxuICBhbmltYXRpb246IGZhZGVJbiAwLjNzIGVhc2UtaW47XHJcbn1cclxuXHJcbkBrZXlmcmFtZXMgZmFkZUluIHtcclxuICBmcm9tIHtcclxuICAgIG9wYWNpdHk6IDA7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTVweCk7XHJcbiAgfVxyXG4gIHRvIHtcclxuICAgIG9wYWNpdHk6IDE7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoMCk7XHJcbiAgfVxyXG59XHJcblxyXG4ucHJldmlldy1pY29uIHtcclxuICBjb2xvcjogIzRjYWY1MDtcclxuICBmb250LXNpemU6IDIwcHg7XHJcbiAgZmxleC1zaHJpbms6IDA7XHJcbn1cclxuXHJcbi5wcmV2aWV3LWNvbnRlbnQge1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5wcmV2aWV3LWxhYmVsIHtcclxuICBmb250LXNpemU6IDAuOHJlbTtcclxuICBjb2xvcjogIzY2NjtcclxuICBtYXJnaW4tYm90dG9tOiA0cHg7XHJcbn1cclxuXHJcbi5wcmV2aWV3LXBhdGgge1xyXG4gIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XHJcbiAgZm9udC1zaXplOiAwLjg1cmVtO1xyXG4gIGNvbG9yOiAjMzMzO1xyXG4gIGZvbnQtd2VpZ2h0OiA2MDA7XHJcbiAgd29yZC1icmVhazogYnJlYWstYWxsO1xyXG59XHJcblxyXG4uc3VnZ2VzdGlvbnMge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBnYXA6IDhweDtcclxuICBhbmltYXRpb246IGZhZGVJbiAwLjNzIGVhc2UtaW47XHJcbn1cclxuXHJcbi5zdWdnZXN0aW9ucy1sYWJlbCB7XHJcbiAgZm9udC1zaXplOiAwLjg1cmVtO1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbn1cclxuXHJcbi5zdWdnZXN0aW9ucy1jaGlwcyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LXdyYXA6IHdyYXA7XHJcbiAgZ2FwOiA4cHg7XHJcbn1cclxuXHJcbi5zdWdnZXN0aW9uLWNoaXAge1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICB0cmFuc2l0aW9uOiBhbGwgMC4ycyBlYXNlO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIGJhY2tncm91bmQ6ICNlM2YyZmQ7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XHJcbiAgICBib3gtc2hhZG93OiAwIDJweCA0cHggcmdiYSgwLDAsMCwwLjEpO1xyXG4gIH1cclxuXHJcbiAgJjphY3RpdmUge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKDApO1xyXG4gIH1cclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgZm9udC1zaXplOiAxNnB4O1xyXG4gICAgd2lkdGg6IDE2cHg7XHJcbiAgICBoZWlnaHQ6IDE2cHg7XHJcbiAgfVxyXG59XHJcblxyXG4uYnV0dG9uLXNwaW5uZXIge1xyXG4gIGRpc3BsYXk6IGlubGluZS1ibG9jaztcclxuICBtYXJnaW4tcmlnaHQ6IDhweDtcclxuXHJcbiAgOjpuZy1kZWVwIGNpcmNsZSB7XHJcbiAgICBzdHJva2U6IHdoaXRlO1xyXG4gIH1cclxufVxyXG5cclxuLy8gQnV0dG9uIGFsaWdubWVudFxyXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweDtcclxuICBtYXJnaW46IDA7XHJcbn1cclxuXHJcbi8vIFJlc3BvbnNpdmVcclxuQG1lZGlhIChtYXgtd2lkdGg6IDc2OHB4KSB7XHJcbiAgLmRpYWxvZy1jb250YWluZXIge1xyXG4gICAgbWluLXdpZHRoOiB1bnNldDtcclxuICB9XHJcblxyXG4gIC5zdWdnZXN0aW9ucy1jaGlwcyB7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIH1cclxuXHJcbiAgLnN1Z2dlc3Rpb24tY2hpcCB7XHJcbiAgICB3aWR0aDogMTAwJTtcclxuICB9XHJcbn1cclxuIl19 */"] });


/***/ }),

/***/ "XPJ/":
/*!***************************************************************!*\
  !*** ./src/app/components/search-box/search-box.component.ts ***!
  \***************************************************************/
/*! exports provided: SearchBoxComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SearchBoxComponent", function() { return SearchBoxComponent; });
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_search_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/search.service */ "l3hs");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");














const _c0 = ["searchInput"];
function SearchBoxComponent_button_6_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "button", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function SearchBoxComponent_button_6_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4); const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r3.clearSearch(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function SearchBoxComponent_div_7_div_1_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Files ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("(", ctx_r8.searchResults.totalFiles, ")");
} }
function SearchBoxComponent_div_7_div_1_div_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Nessun file trovato ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function SearchBoxComponent_div_7_div_1_div_6_span_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const file_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](file_r14.fileType);
} }
function SearchBoxComponent_div_7_div_1_div_6_Template(rf, ctx) { if (rf & 1) {
    const _r18 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function SearchBoxComponent_div_7_div_1_div_6_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r18); const file_r14 = ctx.$implicit; const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3); return ctx_r17.selectFile(file_r14); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "description");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](4, "div", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "span", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, SearchBoxComponent_div_7_div_1_div_6_span_10_Template, 2, 1, "span", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const file_r14 = ctx.$implicit;
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("innerHTML", file_r14.highlightedText || file_r14.fileName, _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵsanitizeHtml"]);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("title", ctx_r10.getPathTooltip(file_r14.path))("matTooltip", ctx_r10.getRelativePath(file_r14.path))("matTooltipShowDelay", 500);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", ctx_r10.getRelativePath(file_r14.path), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](file_r14.matchedField);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", file_r14.fileType);
} }
function SearchBoxComponent_div_7_div_1_ng_template_8_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Links ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("(", ctx_r11.searchResults.totalLinks, ")");
} }
function SearchBoxComponent_div_7_div_1_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Nessun link trovato ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function SearchBoxComponent_div_7_div_1_div_11_div_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const link_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](link_r19.mdContext);
} }
function SearchBoxComponent_div_7_div_1_div_11_span_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const link_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("matTooltip", ctx_r21.getRelativePath(link_r19.markdownFilePath))("matTooltipShowDelay", 500);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" in ", link_r19.markdownFileName, " ");
} }
function SearchBoxComponent_div_7_div_1_div_11_Template(rf, ctx) { if (rf & 1) {
    const _r25 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function SearchBoxComponent_div_7_div_1_div_11_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r25); const link_r19 = ctx.$implicit; const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3); return ctx_r24.selectLink(link_r19); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](4, "div", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, SearchBoxComponent_div_7_div_1_div_11_div_5_Template, 2, 1, "div", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "span", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, SearchBoxComponent_div_7_div_1_div_11_span_9_Template, 2, 3, "span", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const link_r19 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("innerHTML", link_r19.highlightedText || link_r19.mdTitle || link_r19.path, _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵsanitizeHtml"]);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", link_r19.mdContext);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](link_r19.matchedField);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", link_r19.markdownFileName);
} }
function SearchBoxComponent_div_7_div_1_Template(rf, ctx) { if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-tab-group", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("selectedIndexChange", function SearchBoxComponent_div_7_div_1_Template_mat_tab_group_selectedIndexChange_1_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r27); const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2); return ctx_r26.onTabChange($event); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, SearchBoxComponent_div_7_div_1_ng_template_3_Template, 4, 1, "ng-template", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, SearchBoxComponent_div_7_div_1_div_5_Template, 2, 0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, SearchBoxComponent_div_7_div_1_div_6_Template, 11, 7, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, SearchBoxComponent_div_7_div_1_ng_template_8_Template, 4, 1, "ng-template", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, SearchBoxComponent_div_7_div_1_div_10_Template, 2, 0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, SearchBoxComponent_div_7_div_1_div_11_Template, 10, 4, "div", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("selectedIndex", ctx_r5.selectedTabIndex);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r5.searchResults.files.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r5.searchResults.files);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r5.searchResults.links.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r5.searchResults.links);
} }
function SearchBoxComponent_div_7_div_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Ricerca in corso...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function SearchBoxComponent_div_7_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "span", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("", ctx_r7.searchResults.searchDurationMs, "ms");
} }
function SearchBoxComponent_div_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](1, SearchBoxComponent_div_7_div_1_Template, 12, 5, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](2, SearchBoxComponent_div_7_div_2_Template, 4, 0, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, SearchBoxComponent_div_7_div_3_Template, 3, 1, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r2.searchResults);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r2.isSearching);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r2.searchResults && !ctx_r2.isSearching);
} }
class SearchBoxComponent {
    constructor(searchService, router, mdFileService, projectsService) {
        this.searchService = searchService;
        this.router = router;
        this.mdFileService = mdFileService;
        this.projectsService = projectsService;
        this.searchControl = new _angular_forms__WEBPACK_IMPORTED_MODULE_0__["FormControl"]('');
        this.searchResults = null;
        this.isSearching = false;
        this.showResults = false;
        this.selectedTab = 'files';
        this.selectedTabIndex = 0;
        this.currentProjectPath = '';
        this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_1__["Subject"]();
    }
    ngOnInit() {
        // Subscribe to current project changes
        this.projectsService.currentProjects$.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["takeUntil"])(this.destroy$)).subscribe(project => {
            if (project) {
                this.currentProjectPath = project.path;
            }
        });
        // Setup search with debounce
        this.searchControl.valueChanges.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["debounceTime"])(300), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["distinctUntilChanged"])(), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["switchMap"])(term => {
            if (!term || term.trim().length < 2) {
                this.searchResults = null;
                this.showResults = false;
                return [];
            }
            this.isSearching = true;
            this.showResults = true;
            return this.searchService.quickSearch(term);
        }), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["takeUntil"])(this.destroy$)).subscribe(results => {
            // Debug: Analizza i file ricevuti per trovare duplicati
            if (results && results.files) {
                // Raggruppa per path per trovare duplicati
                const filesByPath = {};
                results.files.forEach((file, index) => {
                    const normalizedPath = file.path.toLowerCase();
                    if (!filesByPath[normalizedPath]) {
                        filesByPath[normalizedPath] = [];
                    }
                    filesByPath[normalizedPath].push(file);
                });
                // Log dei duplicati trovati
                Object.keys(filesByPath).forEach(path => {
                    if (filesByPath[path].length > 1) {
                        console.warn('[SearchBox] ⚠️ DUPLICATE FILES FOUND!');
                        console.warn('[SearchBox] Path:', path);
                        console.warn('[SearchBox] Number of duplicates:', filesByPath[path].length);
                        console.warn('[SearchBox] Duplicate entries with IDs:', filesByPath[path].map(f => ({
                            id: f.id,
                            fileName: f.fileName,
                            path: f.path
                        })));
                    }
                });
            }
            this.searchResults = results;
            this.isSearching = false;
            // Auto-select tab with results
            if (results && results.files.length > 0 && results.links.length === 0) {
                this.selectedTab = 'files';
                this.selectedTabIndex = 0;
            }
            else if (results && results.links.length > 0 && results.files.length === 0) {
                this.selectedTab = 'links';
                this.selectedTabIndex = 1;
            }
        }, error => {
            this.isSearching = false;
            this.searchResults = null;
        });
    }
    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
    onDocumentClick(event) {
        const clickedElement = event.target;
        const searchBox = document.querySelector('.search-box-container');
        // Don't close if clicking on Material Design overlay, tab elements, or toolbar buttons
        if (clickedElement.closest('.cdk-overlay-container') ||
            clickedElement.closest('.mat-tab-label') ||
            clickedElement.closest('.mat-tab-group') ||
            clickedElement.closest('mat-toolbar') ||
            clickedElement.closest('.mat-toolbar')) {
            return;
        }
        if (searchBox && !searchBox.contains(clickedElement)) {
            this.showResults = false;
        }
    }
    onFocus() {
        if (this.searchControl.value && this.searchControl.value.trim().length >= 2) {
            this.showResults = true;
        }
    }
    onBlur(event) {
        // Delay to allow click on results
        setTimeout(() => {
            const searchContainer = document.querySelector('.search-box-container');
            const activeElement = document.activeElement;
            // Keep results open if focus is still within the search container
            if (searchContainer && searchContainer.contains(activeElement)) {
                return;
            }
            // Also keep open if clicking on Material tabs or tab content
            if (activeElement && (activeElement.closest('.mat-tab-label') ||
                activeElement.closest('.mat-tab-group') ||
                activeElement.closest('.search-results'))) {
                return;
            }
            this.showResults = false;
        }, 200);
    }
    selectFile(file) {
        // Get current project path to calculate relative path
        const currentProject = this.projectsService.currentProjects$.value;
        let fullPath = file.path;
        let relativePath = '';
        // Calculate relative path from project root
        if (currentProject && currentProject.path) {
            const projectPath = currentProject.path.replace(/\\/g, '/');
            const filePath = file.path.replace(/\\/g, '/');
            // If file path starts with project path, extract relative part
            if (filePath.startsWith(projectPath)) {
                relativePath = filePath.substring(projectPath.length);
                // Ensure relative path starts with backslash or forward slash
                if (!relativePath.startsWith('/') && !relativePath.startsWith('\\')) {
                    relativePath = '\\' + relativePath;
                }
            }
            else {
                // Fallback: use just the filename with backslash prefix
                relativePath = '\\' + file.fileName;
            }
        }
        else {
            // No project context, use filename as relative path
            relativePath = '\\' + file.fileName;
        }
        // Create MdFile object from search result
        const mdFile = {
            fullPath: fullPath,
            relativePath: relativePath,
            path: relativePath,
            name: file.fileName,
            type: file.fileType || 'mdFile'
        };
        // Navigate to document
        this.router.navigate(['/main/navigation/document']);
        this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
        // Clear search and hide results
        this.searchControl.setValue('');
        this.showResults = false;
    }
    selectLink(link) {
        // Navigate based on link type
        if (link.fullPath) {
            // Get current project path to calculate relative path
            const currentProject = this.projectsService.currentProjects$.value;
            let relativePath = '';
            // Calculate relative path from project root
            if (currentProject && currentProject.path) {
                const projectPath = currentProject.path.replace(/\\/g, '/');
                const filePath = link.fullPath.replace(/\\/g, '/');
                // If file path starts with project path, extract relative part
                if (filePath.startsWith(projectPath)) {
                    relativePath = filePath.substring(projectPath.length);
                    // Ensure relative path starts with backslash or forward slash
                    if (!relativePath.startsWith('/') && !relativePath.startsWith('\\')) {
                        relativePath = '\\' + relativePath;
                    }
                }
                else {
                    // Fallback: use just the filename with backslash prefix
                    const fileName = link.fullPath.split('\\').pop() || link.mdTitle || link.path;
                    relativePath = '\\' + fileName;
                }
            }
            else {
                // No project context, use filename as relative path
                const fileName = link.fullPath.split('\\').pop() || link.mdTitle || link.path;
                relativePath = '\\' + fileName;
            }
            const mdFile = {
                fullPath: link.fullPath,
                relativePath: relativePath,
                path: relativePath,
                name: link.mdTitle || link.path,
                type: 'mdFile'
            };
            this.router.navigate(['/main/navigation/document']);
            this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
        }
        // Clear search and hide results
        this.searchControl.setValue('');
        this.showResults = false;
    }
    clearSearch() {
        this.searchControl.setValue('');
        this.searchResults = null;
        this.showResults = false;
    }
    onTabChange(index) {
        console.log('Tab changed:', {
            newIndex: index,
            previousIndex: this.selectedTabIndex,
            newTab: index === 0 ? 'files' : 'links',
            showResults: this.showResults
        });
        this.selectedTabIndex = index;
        this.selectedTab = index === 0 ? 'files' : 'links';
    }
    // Keyboard navigation
    handleKeyboardEvent(event) {
        if (event.key === 'Escape') {
            this.showResults = false;
            this.searchInput.nativeElement.blur();
        }
        // Add arrow key navigation if needed
    }
    // Helper method to get relative path for display
    getRelativePath(fullPath) {
        if (!this.currentProjectPath || !fullPath) {
            return fullPath;
        }
        const projectPath = this.currentProjectPath.replace(/\\/g, '/').toLowerCase();
        const filePath = fullPath.replace(/\\/g, '/').toLowerCase();
        // If file path starts with project path, extract relative part
        if (filePath.startsWith(projectPath)) {
            let relativePath = fullPath.substring(this.currentProjectPath.length);
            // Remove leading slash/backslash if present
            if (relativePath.startsWith('\\') || relativePath.startsWith('/')) {
                relativePath = relativePath.substring(1);
            }
            return relativePath;
        }
        // Fallback: return the full path
        return fullPath;
    }
    // Get tooltip text for long paths
    getPathTooltip(fullPath) {
        const relativePath = this.getRelativePath(fullPath);
        // Show full path in tooltip if it's truncated
        return relativePath;
    }
}
SearchBoxComponent.ɵfac = function SearchBoxComponent_Factory(t) { return new (t || SearchBoxComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_search_service__WEBPACK_IMPORTED_MODULE_4__["SearchService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_5__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_6__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__["ProjectsService"])); };
SearchBoxComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: SearchBoxComponent, selectors: [["app-search-box"]], viewQuery: function SearchBoxComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵloadQuery"]()) && (ctx.searchInput = _t.first);
    } }, hostBindings: function SearchBoxComponent_HostBindings(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function SearchBoxComponent_click_HostBindingHandler($event) { return ctx.onDocumentClick($event); }, false, _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresolveDocument"])("keydown", function SearchBoxComponent_keydown_HostBindingHandler($event) { return ctx.handleKeyboardEvent($event); });
    } }, decls: 8, vars: 5, consts: [[1, "search-box-container"], [1, "search-input-wrapper"], [1, "search-icon"], ["type", "text", "placeholder", "Cerca documenti e link...", "autocomplete", "off", 1, "search-input", 3, "formControl", "focus", "blur"], ["searchInput", ""], ["class", "clear-button", "type", "button", 3, "click", 4, "ngIf"], ["class", "search-results", 4, "ngIf"], ["type", "button", 1, "clear-button", 3, "click"], [1, "search-results"], ["class", "search-header", 4, "ngIf"], ["class", "search-loading", 4, "ngIf"], ["class", "search-footer", 4, "ngIf"], [1, "search-header"], [3, "selectedIndex", "selectedIndexChange"], ["mat-tab-label", ""], [1, "results-list", "files-list"], ["class", "no-results", 4, "ngIf"], ["class", "result-item file-item", 3, "click", 4, "ngFor", "ngForOf"], [1, "results-list", "links-list"], ["class", "result-item link-item", 3, "click", 4, "ngFor", "ngForOf"], [1, "tab-label"], [1, "count"], [1, "no-results"], [1, "result-item", "file-item", 3, "click"], [1, "result-icon"], [1, "result-content"], [1, "result-title", 3, "innerHTML"], ["matTooltipPosition", "above", 1, "result-path", 3, "title", "matTooltip", "matTooltipShowDelay"], [1, "result-meta"], [1, "matched-field"], ["class", "file-type", 4, "ngIf"], [1, "file-type"], [1, "result-item", "link-item", 3, "click"], ["class", "result-context", 4, "ngIf"], ["class", "source-file", "matTooltipPosition", "above", 3, "matTooltip", "matTooltipShowDelay", 4, "ngIf"], [1, "result-context"], ["matTooltipPosition", "above", 1, "source-file", 3, "matTooltip", "matTooltipShowDelay"], [1, "search-loading"], ["diameter", "20"], [1, "search-footer"], [1, "search-time"]], template: function SearchBoxComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-icon", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "search");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "input", 3, 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("focus", function SearchBoxComponent_Template_input_focus_4_listener() { return ctx.onFocus(); })("blur", function SearchBoxComponent_Template_input_blur_4_listener($event) { return ctx.onBlur($event); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, SearchBoxComponent_button_6_Template, 3, 0, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, SearchBoxComponent_div_7_Template, 4, 3, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵclassProp"]("has-value", ctx.searchControl.value);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("formControl", ctx.searchControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.searchControl.value);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.showResults && (ctx.searchResults || ctx.isSearching));
    } }, directives: [_angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"], _angular_forms__WEBPACK_IMPORTED_MODULE_0__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_0__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_0__["FormControlDirective"], _angular_common__WEBPACK_IMPORTED_MODULE_9__["NgIf"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_10__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_10__["MatTab"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_10__["MatTabLabel"], _angular_common__WEBPACK_IMPORTED_MODULE_9__["NgForOf"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_11__["MatTooltip"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_12__["MatSpinner"]], styles: [".search-box-container[_ngcontent-%COMP%] {\n  position: relative;\n  flex: 1;\n  max-width: 600px;\n  margin: 0 20px;\n}\n\n.search-input-wrapper[_ngcontent-%COMP%] {\n  position: relative;\n  display: flex;\n  align-items: center;\n  background: rgba(255, 255, 255, 0.1);\n  border-radius: 4px;\n  transition: background-color 0.2s;\n}\n\n.search-input-wrapper[_ngcontent-%COMP%]:hover {\n  background: rgba(255, 255, 255, 0.15);\n}\n\n.search-input-wrapper[_ngcontent-%COMP%]:focus-within {\n  background: rgba(255, 255, 255, 0.2);\n  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.3);\n}\n\n.search-icon[_ngcontent-%COMP%] {\n  position: absolute;\n  left: 8px;\n  color: rgba(255, 255, 255, 0.5);\n  font-size: 20px;\n  transition: color 0.2s;\n}\n\n.search-icon.has-value[_ngcontent-%COMP%] {\n  color: rgba(255, 255, 255, 0.8);\n}\n\n.search-input[_ngcontent-%COMP%] {\n  flex: 1;\n  border: none;\n  background: transparent;\n  color: white;\n  padding: 6px 32px 6px 36px;\n  font-size: 14px;\n  outline: none;\n}\n\n.search-input[_ngcontent-%COMP%]::placeholder {\n  color: rgba(255, 255, 255, 0.5);\n}\n\n.clear-button[_ngcontent-%COMP%] {\n  position: absolute;\n  right: 4px;\n  background: none;\n  border: none;\n  color: rgba(255, 255, 255, 0.5);\n  cursor: pointer;\n  padding: 4px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n}\n\n.clear-button[_ngcontent-%COMP%]:hover {\n  color: rgba(255, 255, 255, 0.8);\n}\n\n.clear-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.search-results[_ngcontent-%COMP%] {\n  position: absolute;\n  top: calc(100% + 4px);\n  left: 0;\n  right: 0;\n  background: white;\n  border-radius: 4px;\n  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);\n  max-height: 500px;\n  overflow: hidden;\n  z-index: 1000;\n  display: flex;\n  flex-direction: column;\n}\n\n.search-header[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow: hidden;\n  display: flex;\n  flex-direction: column;\n}\n\n.search-header[_ngcontent-%COMP%]     .mat-tab-group {\n  height: 100%;\n}\n\n.search-header[_ngcontent-%COMP%]     .mat-tab-body-wrapper {\n  flex: 1;\n}\n\n.tab-label[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n}\n\n.tab-label[_ngcontent-%COMP%]   .count[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #666;\n}\n\n.results-list[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-y: auto;\n}\n\n.result-item[_ngcontent-%COMP%] {\n  display: flex;\n  padding: 12px;\n  cursor: pointer;\n  border-bottom: 1px solid #f0f0f0;\n  transition: background-color 0.2s;\n}\n\n.result-item[_ngcontent-%COMP%]:hover {\n  background-color: #f5f5f5;\n}\n\n.result-item[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n\n.result-icon[_ngcontent-%COMP%] {\n  margin-right: 12px;\n  color: #666;\n  font-size: 20px;\n}\n\n.result-content[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow: hidden;\n}\n\n.result-title[_ngcontent-%COMP%] {\n  font-weight: 500;\n  color: #333;\n  margin-bottom: 4px;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.result-title[_ngcontent-%COMP%]     mark {\n  background-color: #ffeb3b;\n  color: #333;\n  font-weight: 600;\n}\n\n.result-path[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #666;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  max-width: 100%;\n  cursor: default;\n}\n\n.result-path[_ngcontent-%COMP%]:hover {\n  color: #444;\n  -webkit-text-decoration: underline dotted;\n          text-decoration: underline dotted;\n}\n\n.result-context[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #666;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.result-meta[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  margin-top: 4px;\n  font-size: 11px;\n  color: #999;\n}\n\n.matched-field[_ngcontent-%COMP%] {\n  background: #e3f2fd;\n  color: #1976d2;\n  padding: 2px 6px;\n  border-radius: 2px;\n}\n\n.file-type[_ngcontent-%COMP%], .source-file[_ngcontent-%COMP%] {\n  color: #999;\n}\n\n.no-results[_ngcontent-%COMP%] {\n  padding: 24px;\n  text-align: center;\n  color: #666;\n  font-style: italic;\n}\n\n.search-loading[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 24px;\n  gap: 12px;\n  color: #666;\n}\n\n.search-footer[_ngcontent-%COMP%] {\n  padding: 8px 12px;\n  border-top: 1px solid #f0f0f0;\n  background: #fafafa;\n  text-align: right;\n  font-size: 11px;\n  color: #999;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFxzZWFyY2gtYm94LmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0Usa0JBQUE7RUFDQSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0FBQ0Y7O0FBRUE7RUFDRSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLG9DQUFBO0VBQ0Esa0JBQUE7RUFDQSxpQ0FBQTtBQUNGOztBQUNFO0VBQ0UscUNBQUE7QUFDSjs7QUFFRTtFQUNFLG9DQUFBO0VBQ0EsOENBQUE7QUFBSjs7QUFJQTtFQUNFLGtCQUFBO0VBQ0EsU0FBQTtFQUNBLCtCQUFBO0VBQ0EsZUFBQTtFQUNBLHNCQUFBO0FBREY7O0FBR0U7RUFDRSwrQkFBQTtBQURKOztBQUtBO0VBQ0UsT0FBQTtFQUNBLFlBQUE7RUFDQSx1QkFBQTtFQUNBLFlBQUE7RUFDQSwwQkFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0FBRkY7O0FBSUU7RUFDRSwrQkFBQTtBQUZKOztBQU1BO0VBQ0Usa0JBQUE7RUFDQSxVQUFBO0VBQ0EsZ0JBQUE7RUFDQSxZQUFBO0VBQ0EsK0JBQUE7RUFDQSxlQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBSEY7O0FBS0U7RUFDRSwrQkFBQTtBQUhKOztBQU1FO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBSko7O0FBUUE7RUFDRSxrQkFBQTtFQUNBLHFCQUFBO0VBQ0EsT0FBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsMENBQUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtBQUxGOztBQVFBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0FBTEY7O0FBT0U7RUFDRSxZQUFBO0FBTEo7O0FBUUU7RUFDRSxPQUFBO0FBTko7O0FBVUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0FBUEY7O0FBU0U7RUFDRSxlQUFBO0VBQ0EsV0FBQTtBQVBKOztBQVdBO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtBQVJGOztBQVdBO0VBQ0UsYUFBQTtFQUNBLGFBQUE7RUFDQSxlQUFBO0VBQ0EsZ0NBQUE7RUFDQSxpQ0FBQTtBQVJGOztBQVVFO0VBQ0UseUJBQUE7QUFSSjs7QUFXRTtFQUNFLG1CQUFBO0FBVEo7O0FBYUE7RUFDRSxrQkFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0FBVkY7O0FBYUE7RUFDRSxPQUFBO0VBQ0EsZ0JBQUE7QUFWRjs7QUFhQTtFQUNFLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLG1CQUFBO0FBVkY7O0FBWUU7RUFDRSx5QkFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtBQVZKOztBQWNBO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtBQVhGOztBQWNFO0VBQ0UsV0FBQTtFQUNBLHlDQUFBO1VBQUEsaUNBQUE7QUFaSjs7QUFnQkE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtBQWJGOztBQWdCQTtFQUNFLGFBQUE7RUFDQSxTQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsbUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtBQWJGOztBQWdCQTs7RUFFRSxXQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsYUFBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGtCQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsU0FBQTtFQUNBLFdBQUE7QUFiRjs7QUFnQkE7RUFDRSxpQkFBQTtFQUNBLDZCQUFBO0VBQ0EsbUJBQUE7RUFDQSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0FBYkYiLCJmaWxlIjoic2VhcmNoLWJveC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5zZWFyY2gtYm94LWNvbnRhaW5lciB7XHJcbiAgcG9zaXRpb246IHJlbGF0aXZlO1xyXG4gIGZsZXg6IDE7XHJcbiAgbWF4LXdpZHRoOiA2MDBweDtcclxuICBtYXJnaW46IDAgMjBweDtcclxufVxyXG5cclxuLnNlYXJjaC1pbnB1dC13cmFwcGVyIHtcclxuICBwb3NpdGlvbjogcmVsYXRpdmU7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGJhY2tncm91bmQ6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC4xKTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgdHJhbnNpdGlvbjogYmFja2dyb3VuZC1jb2xvciAwLjJzO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIGJhY2tncm91bmQ6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC4xNSk7XHJcbiAgfVxyXG5cclxuICAmOmZvY3VzLXdpdGhpbiB7XHJcbiAgICBiYWNrZ3JvdW5kOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuMik7XHJcbiAgICBib3gtc2hhZG93OiAwIDAgMCAxcHggcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjMpO1xyXG4gIH1cclxufVxyXG5cclxuLnNlYXJjaC1pY29uIHtcclxuICBwb3NpdGlvbjogYWJzb2x1dGU7XHJcbiAgbGVmdDogOHB4O1xyXG4gIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuNSk7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIHRyYW5zaXRpb246IGNvbG9yIDAuMnM7XHJcblxyXG4gICYuaGFzLXZhbHVlIHtcclxuICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOCk7XHJcbiAgfVxyXG59XHJcblxyXG4uc2VhcmNoLWlucHV0IHtcclxuICBmbGV4OiAxO1xyXG4gIGJvcmRlcjogbm9uZTtcclxuICBiYWNrZ3JvdW5kOiB0cmFuc3BhcmVudDtcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgcGFkZGluZzogNnB4IDMycHggNnB4IDM2cHg7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG4gIG91dGxpbmU6IG5vbmU7XHJcblxyXG4gICY6OnBsYWNlaG9sZGVyIHtcclxuICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuNSk7XHJcbiAgfVxyXG59XHJcblxyXG4uY2xlYXItYnV0dG9uIHtcclxuICBwb3NpdGlvbjogYWJzb2x1dGU7XHJcbiAgcmlnaHQ6IDRweDtcclxuICBiYWNrZ3JvdW5kOiBub25lO1xyXG4gIGJvcmRlcjogbm9uZTtcclxuICBjb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjUpO1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICBwYWRkaW5nOiA0cHg7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOCk7XHJcbiAgfVxyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICB3aWR0aDogMThweDtcclxuICAgIGhlaWdodDogMThweDtcclxuICB9XHJcbn1cclxuXHJcbi5zZWFyY2gtcmVzdWx0cyB7XHJcbiAgcG9zaXRpb246IGFic29sdXRlO1xyXG4gIHRvcDogY2FsYygxMDAlICsgNHB4KTtcclxuICBsZWZ0OiAwO1xyXG4gIHJpZ2h0OiAwO1xyXG4gIGJhY2tncm91bmQ6IHdoaXRlO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBib3gtc2hhZG93OiAwIDRweCAxMnB4IHJnYmEoMCwgMCwgMCwgMC4xNSk7XHJcbiAgbWF4LWhlaWdodDogNTAwcHg7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB6LWluZGV4OiAxMDAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxufVxyXG5cclxuLnNlYXJjaC1oZWFkZXIge1xyXG4gIGZsZXg6IDE7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcblxyXG4gIDo6bmctZGVlcCAubWF0LXRhYi1ncm91cCB7XHJcbiAgICBoZWlnaHQ6IDEwMCU7XHJcbiAgfVxyXG5cclxuICA6Om5nLWRlZXAgLm1hdC10YWItYm9keS13cmFwcGVyIHtcclxuICAgIGZsZXg6IDE7XHJcbiAgfVxyXG59XHJcblxyXG4udGFiLWxhYmVsIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA0cHg7XHJcblxyXG4gIC5jb3VudCB7XHJcbiAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICB9XHJcbn1cclxuXHJcbi5yZXN1bHRzLWxpc3Qge1xyXG4gIG1heC1oZWlnaHQ6IDQwMHB4O1xyXG4gIG92ZXJmbG93LXk6IGF1dG87XHJcbn1cclxuXHJcbi5yZXN1bHQtaXRlbSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2YwZjBmMDtcclxuICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kLWNvbG9yIDAuMnM7XHJcblxyXG4gICY6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuICB9XHJcblxyXG4gICY6bGFzdC1jaGlsZCB7XHJcbiAgICBib3JkZXItYm90dG9tOiBub25lO1xyXG4gIH1cclxufVxyXG5cclxuLnJlc3VsdC1pY29uIHtcclxuICBtYXJnaW4tcmlnaHQ6IDEycHg7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG59XHJcblxyXG4ucmVzdWx0LWNvbnRlbnQge1xyXG4gIGZsZXg6IDE7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxufVxyXG5cclxuLnJlc3VsdC10aXRsZSB7XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICBjb2xvcjogIzMzMztcclxuICBtYXJnaW4tYm90dG9tOiA0cHg7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG5cclxuICA6Om5nLWRlZXAgbWFyayB7XHJcbiAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZmZlYjNiO1xyXG4gICAgY29sb3I6ICMzMzM7XHJcbiAgICBmb250LXdlaWdodDogNjAwO1xyXG4gIH1cclxufVxyXG5cclxuLnJlc3VsdC1wYXRoIHtcclxuICBmb250LXNpemU6IDEycHg7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIG1heC13aWR0aDogMTAwJTtcclxuICBjdXJzb3I6IGRlZmF1bHQ7XHJcbiAgXHJcbiAgLy8gQWRkIHZpc3VhbCBmZWVkYmFjayBvbiBob3ZlclxyXG4gICY6aG92ZXIge1xyXG4gICAgY29sb3I6ICM0NDQ7XHJcbiAgICB0ZXh0LWRlY29yYXRpb246IHVuZGVybGluZSBkb3R0ZWQ7XHJcbiAgfVxyXG59XHJcblxyXG4ucmVzdWx0LWNvbnRleHQge1xyXG4gIGZvbnQtc2l6ZTogMTJweDtcclxuICBjb2xvcjogIzY2NjtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbn1cclxuXHJcbi5yZXN1bHQtbWV0YSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBnYXA6IDEycHg7XHJcbiAgbWFyZ2luLXRvcDogNHB4O1xyXG4gIGZvbnQtc2l6ZTogMTFweDtcclxuICBjb2xvcjogIzk5OTtcclxufVxyXG5cclxuLm1hdGNoZWQtZmllbGQge1xyXG4gIGJhY2tncm91bmQ6ICNlM2YyZmQ7XHJcbiAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgcGFkZGluZzogMnB4IDZweDtcclxuICBib3JkZXItcmFkaXVzOiAycHg7XHJcbn1cclxuXHJcbi5maWxlLXR5cGUsXHJcbi5zb3VyY2UtZmlsZSB7XHJcbiAgY29sb3I6ICM5OTk7XHJcbn1cclxuXHJcbi5uby1yZXN1bHRzIHtcclxuICBwYWRkaW5nOiAyNHB4O1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBjb2xvcjogIzY2NjtcclxuICBmb250LXN0eWxlOiBpdGFsaWM7XHJcbn1cclxuXHJcbi5zZWFyY2gtbG9hZGluZyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDI0cHg7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIGNvbG9yOiAjNjY2O1xyXG59XHJcblxyXG4uc2VhcmNoLWZvb3RlciB7XHJcbiAgcGFkZGluZzogOHB4IDEycHg7XHJcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICNmMGYwZjA7XHJcbiAgYmFja2dyb3VuZDogI2ZhZmFmYTtcclxuICB0ZXh0LWFsaWduOiByaWdodDtcclxuICBmb250LXNpemU6IDExcHg7XHJcbiAgY29sb3I6ICM5OTk7XHJcbn0iXX0= */"] });


/***/ }),

/***/ "YG1a":
/*!*****************************************************************************!*\
  !*** ./src/app/signalR/dialogs/parsing-project/parsing-project.provider.ts ***!
  \*****************************************************************************/
/*! exports provided: ParsingProjectProvider */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ParsingProjectProvider", function() { return ParsingProjectProvider; });
/* harmony import */ var _parsing_project_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./parsing-project.component */ "oPln");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");




class ParsingProjectProvider {
    constructor(dialog) {
        this.dialog = dialog;
        this.folder$ = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]("Processing");
    }
    show(data) {
        this._dialogRef = this.dialog.open(_parsing_project_component__WEBPACK_IMPORTED_MODULE_0__["ParsingProjectComponent"], {
            data: { data: data, folder$: this.folder$ }
        });
        return this;
    }
    hide(data) {
        this._dialogRef.close();
    }
}
ParsingProjectProvider.ɵfac = function ParsingProjectProvider_Factory(t) { return new (t || ParsingProjectProvider)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__["MatDialog"])); };
ParsingProjectProvider.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: ParsingProjectProvider, factory: ParsingProjectProvider.ɵfac });


/***/ }),

/***/ "ZAI4":
/*!*******************************!*\
  !*** ./src/app/app.module.ts ***!
  \*******************************/
/*! exports provided: AppModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AppModule", function() { return AppModule; });
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/platform-browser */ "jhN1");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_flex_layout__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/flex-layout */ "YUcS");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _interceptors_connection_id_interceptor__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./interceptors/connection-id.interceptor */ "n3Jk");
/* harmony import */ var _app_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./app.component */ "Sy1n");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./shared/material.module */ "5dmV");
/* harmony import */ var _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/platform-browser/animations */ "R1ws");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./signalR/dialogs/parsing-project/parsing-project.provider */ "YG1a");
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./signalR/dialogs/connection-lost/connection-lost.provider */ "jX2R");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./signalR/dialogs/plantuml-working/plantuml-working.component */ "ggj0");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./signalR/dialogs/plantuml-working/plantuml-working.provider */ "CqLH");
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./commons/waitingdialog/waiting-dialog/waiting-dialog.component */ "S2pp");
/* harmony import */ var _commons_components_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./commons/components/new-directory/new-directory.component */ "TUMs");
/* harmony import */ var _signalR_dialogs_opening_application_opening_application_component__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./signalR/dialogs/opening-application/opening-application.component */ "mLxA");
/* harmony import */ var _signalR_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./signalR/dialogs/opening-application/opening-application.provider */ "l94Z");
/* harmony import */ var _components_title_bar_title_bar_component__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ./components/title-bar/title-bar.component */ "89FR");
/* harmony import */ var _components_search_box_search_box_component__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./components/search-box/search-box.component */ "XPJ/");
/* harmony import */ var _md_explorer_components_compatibility_mode_badge_compatibility_mode_badge_component__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./md-explorer/components/compatibility-mode-badge/compatibility-mode-badge.component */ "ZNV1");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/core */ "fXoL");






//import { AppRoutingModule } from './app-routing.module';

















const routes = [
    {
        path: 'main',
        loadChildren: () => Promise.all(/*! import() | md-explorer-md-explorer-module */[__webpack_require__.e("default~md-explorer-md-explorer-module~projects-projects-module"), __webpack_require__.e("md-explorer-md-explorer-module")]).then(__webpack_require__.bind(null, /*! ./md-explorer/md-explorer.module */ "A4yT")).then(m => m.MdExplorerModule),
        data: { animation: 'main' }
    },
    {
        path: 'projects', loadChildren: () => Promise.all(/*! import() | projects-projects-module */[__webpack_require__.e("default~md-explorer-md-explorer-module~projects-projects-module"), __webpack_require__.e("projects-projects-module")]).then(__webpack_require__.bind(null, /*! ./projects/projects.module */ "Wm2z")).then(m => m.ProjectsModule),
        data: { animation: 'projects' }
    },
    { path: '**', redirectTo: 'projects', data: { animation: 'projects' } }
];
class AppModule {
    constructor() {
        console.log('AppModuleConstructor');
    }
}
AppModule.ɵfac = function AppModule_Factory(t) { return new (t || AppModule)(); };
AppModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_21__["ɵɵdefineNgModule"]({ type: AppModule, bootstrap: [_app_component__WEBPACK_IMPORTED_MODULE_6__["AppComponent"]] });
AppModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_21__["ɵɵdefineInjector"]({ providers: [
        _signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_9__["ParsingProjectProvider"],
        _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_10__["ConnectionLostProvider"],
        _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_12__["PlantumlWorkingProvider"],
        _signalR_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_17__["OpeningApplicationProvider"],
        {
            provide: _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HTTP_INTERCEPTORS"],
            useClass: _interceptors_connection_id_interceptor__WEBPACK_IMPORTED_MODULE_5__["ConnectionIdInterceptor"],
            multi: true
        }
    ], imports: [[
            _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["BrowserModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"].forRoot(routes),
            _angular_flex_layout__WEBPACK_IMPORTED_MODULE_2__["FlexLayoutModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_7__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_3__["FormsModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_3__["ReactiveFormsModule"],
            _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_8__["BrowserAnimationsModule"],
            _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClientModule"],
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_21__["ɵɵsetNgModuleScope"](AppModule, { declarations: [_app_component__WEBPACK_IMPORTED_MODULE_6__["AppComponent"],
        _signalR_dialogs_plantuml_working_plantuml_working_component__WEBPACK_IMPORTED_MODULE_11__["PlantumlWorkingComponent"],
        _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_13__["ShowFileSystemComponent"],
        _commons_waitingdialog_waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_14__["WaitingDialogComponent"],
        _commons_components_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_15__["NewDirectoryComponent"],
        _signalR_dialogs_opening_application_opening_application_component__WEBPACK_IMPORTED_MODULE_16__["OpeningApplicationComponent"],
        _components_title_bar_title_bar_component__WEBPACK_IMPORTED_MODULE_18__["TitleBarComponent"],
        _components_search_box_search_box_component__WEBPACK_IMPORTED_MODULE_19__["SearchBoxComponent"],
        _md_explorer_components_compatibility_mode_badge_compatibility_mode_badge_component__WEBPACK_IMPORTED_MODULE_20__["CompatibilityModeBadgeComponent"]], imports: [_angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["BrowserModule"], _angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"], //
        _angular_flex_layout__WEBPACK_IMPORTED_MODULE_2__["FlexLayoutModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_7__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_3__["FormsModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_3__["ReactiveFormsModule"],
        _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_8__["BrowserAnimationsModule"],
        _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClientModule"]] }); })();


/***/ }),

/***/ "ZNV1":
/*!*******************************************************************************************************!*\
  !*** ./src/app/md-explorer/components/compatibility-mode-badge/compatibility-mode-badge.component.ts ***!
  \*******************************************************************************************************/
/*! exports provided: CompatibilityModeBadgeComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CompatibilityModeBadgeComponent", function() { return CompatibilityModeBadgeComponent; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../models/compatibility-mode.model */ "25Jb");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../services/compatibility-mode.service */ "4rFV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");








class CompatibilityModeBadgeComponent {
    constructor(compatibilityService) {
        this.compatibilityService = compatibilityService;
        this.currentMode = _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
        this.modeDisplayName = 'MdExplorer';
        this.modeIcon = 'rocket_launch';
        this.modeColor = 'primary';
        this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["Subject"]();
    }
    ngOnInit() {
        // Subscribe to mode changes
        this.compatibilityService.currentMode$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(mode => {
            this.currentMode = mode;
            this.updateBadgeProperties();
        });
    }
    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
    updateBadgeProperties() {
        this.modeDisplayName = this.compatibilityService.getModeDisplayName(this.currentMode);
        this.modeIcon = this.compatibilityService.getModeIcon(this.currentMode);
        this.modeColor = this.compatibilityService.getModeColor(this.currentMode);
    }
    /**
     * Get tooltip text based on current mode
     */
    getTooltip() {
        switch (this.currentMode) {
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub:
                return 'GitHub Compatible Mode - Markdown is compatible with GitHub and other standard viewers';
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark:
                return 'CommonMark Mode - Strict CommonMark specification compliance';
            case _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer:
            default:
                return 'MdExplorer Mode - All advanced features enabled (PlantUML, interactive emoji, etc.)';
        }
    }
    /**
     * Check if mode is GitHub
     */
    isGitHubMode() {
        return this.currentMode === _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub;
    }
    /**
     * Check if mode is MdExplorer
     */
    isMdExplorerMode() {
        return this.currentMode === _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
    }
}
CompatibilityModeBadgeComponent.ɵfac = function CompatibilityModeBadgeComponent_Factory(t) { return new (t || CompatibilityModeBadgeComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_4__["CompatibilityModeService"])); };
CompatibilityModeBadgeComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: CompatibilityModeBadgeComponent, selectors: [["app-compatibility-mode-badge"]], decls: 6, vars: 8, consts: [[1, "compatibility-badge", 3, "matTooltip"], ["mat-button", "", "disabled", "", 3, "color"], [1, "mode-icon"], [1, "mode-text"]], template: function CompatibilityModeBadgeComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-icon", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "span", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵclassProp"]("github-mode", ctx.isGitHubMode())("mdexplorer-mode", ctx.isMdExplorerMode());
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("matTooltip", ctx.getTooltip());
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("color", ctx.modeColor);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx.modeIcon);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx.modeDisplayName);
    } }, directives: [_angular_material_tooltip__WEBPACK_IMPORTED_MODULE_5__["MatTooltip"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"]], styles: [".compatibility-badge[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  padding: 0 4px;\n}\n.compatibility-badge[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  font-size: 12px;\n  padding: 4px 8px;\n  cursor: default;\n  min-width: auto;\n  color: rgba(255, 255, 255, 0.9) !important;\n}\n.compatibility-badge[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:disabled {\n  color: rgba(255, 255, 255, 0.9) !important;\n}\n.compatibility-badge[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   .mode-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  margin-right: 4px;\n  color: rgba(255, 255, 255, 0.9);\n}\n.compatibility-badge[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   .mode-text[_ngcontent-%COMP%] {\n  font-weight: 500;\n  font-size: 12px;\n  color: rgba(255, 255, 255, 0.9);\n}\n.compatibility-badge.github-mode[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  animation: pulse-github 2s ease-in-out infinite;\n}\n@keyframes pulse-github {\n  0%, 100% {\n    opacity: 1;\n  }\n  50% {\n    opacity: 0.8;\n  }\n}\n@media (max-width: 768px) {\n  .compatibility-badge[_ngcontent-%COMP%]   .mode-text[_ngcontent-%COMP%] {\n    display: none;\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcY29tcGF0aWJpbGl0eS1tb2RlLWJhZGdlLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLGNBQUE7QUFDRjtBQUNFO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSwwQ0FBQTtBQUNKO0FBQ0k7RUFDRSwwQ0FBQTtBQUNOO0FBRUk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLCtCQUFBO0FBQU47QUFHSTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtFQUNBLCtCQUFBO0FBRE47QUFNSTtFQUNFLCtDQUFBO0FBSk47QUFlQTtFQUNFO0lBQ0UsVUFBQTtFQWJGO0VBZUE7SUFDRSxZQUFBO0VBYkY7QUFDRjtBQWlCQTtFQUVJO0lBQ0UsYUFBQTtFQWhCSjtBQUNGIiwiZmlsZSI6ImNvbXBhdGliaWxpdHktbW9kZS1iYWRnZS5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5jb21wYXRpYmlsaXR5LWJhZGdlIHtcclxuICBkaXNwbGF5OiBpbmxpbmUtZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDAgNHB4O1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgcGFkZGluZzogNHB4IDhweDtcclxuICAgIGN1cnNvcjogZGVmYXVsdDtcclxuICAgIG1pbi13aWR0aDogYXV0bztcclxuICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOSkgIWltcG9ydGFudDtcclxuXHJcbiAgICAmOmRpc2FibGVkIHtcclxuICAgICAgY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC45KSAhaW1wb3J0YW50O1xyXG4gICAgfVxyXG5cclxuICAgIC5tb2RlLWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xyXG4gICAgICBjb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjkpO1xyXG4gICAgfVxyXG5cclxuICAgIC5tb2RlLXRleHQge1xyXG4gICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOSk7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAmLmdpdGh1Yi1tb2RlIHtcclxuICAgIGJ1dHRvbiB7XHJcbiAgICAgIGFuaW1hdGlvbjogcHVsc2UtZ2l0aHViIDJzIGVhc2UtaW4tb3V0IGluZmluaXRlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgJi5tZGV4cGxvcmVyLW1vZGUge1xyXG4gICAgYnV0dG9uIHtcclxuICAgICAgLy8gRGVmYXVsdCBzdHlsaW5nXHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG5Aa2V5ZnJhbWVzIHB1bHNlLWdpdGh1YiB7XHJcbiAgMCUsIDEwMCUge1xyXG4gICAgb3BhY2l0eTogMTtcclxuICB9XHJcbiAgNTAlIHtcclxuICAgIG9wYWNpdHk6IDAuODtcclxuICB9XHJcbn1cclxuXHJcbi8vIFJlc3BvbnNpdmUgc3R5bGVzXHJcbkBtZWRpYSAobWF4LXdpZHRoOiA3NjhweCkge1xyXG4gIC5jb21wYXRpYmlsaXR5LWJhZGdlIHtcclxuICAgIC5tb2RlLXRleHQge1xyXG4gICAgICBkaXNwbGF5OiBub25lOyAvLyBIaWRlIHRleHQgb24gbW9iaWxlLCBzaG93IG9ubHkgaWNvblxyXG4gICAgfVxyXG4gIH1cclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "aS6m":
/*!***********************************************!*\
  !*** ./src/app/md-explorer/models/md-file.ts ***!
  \***********************************************/
/*! exports provided: MdFile */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdFile", function() { return MdFile; });
class MdFile {
    constructor(name, path, level, expandable) {
        this.name = name;
        this.path = path;
        this.level = level;
        this.expandable = expandable;
    }
}


/***/ }),

/***/ "eAi6":
/*!*****************************************************************!*\
  !*** ./src/app/commons/waitingdialog/waiting-dialog.service.ts ***!
  \*****************************************************************/
/*! exports provided: WaitingDialogService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "WaitingDialogService", function() { return WaitingDialogService; });
/* harmony import */ var _waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./waiting-dialog/waiting-dialog.component */ "S2pp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");



class WaitingDialogService {
    constructor(dialog) {
        this.dialog = dialog;
    }
    showMessageBox(info) {
        this.dialogRef = this.dialog.open(_waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_0__["WaitingDialogComponent"], {
            width: '300px',
            height: '300px',
            data: info
        });
    }
    closeMessageBox() {
        this.dialogRef.close();
    }
}
WaitingDialogService.ɵfac = function WaitingDialogService_Factory(t) { return new (t || WaitingDialogService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialog"])); };
WaitingDialogService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: WaitingDialogService, factory: WaitingDialogService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "ggj0":
/*!********************************************************************************!*\
  !*** ./src/app/signalR/dialogs/plantuml-working/plantuml-working.component.ts ***!
  \********************************************************************************/
/*! exports provided: PlantumlWorkingComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "PlantumlWorkingComponent", function() { return PlantumlWorkingComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class PlantumlWorkingComponent {
    constructor() { }
    ngOnInit() {
    }
}
PlantumlWorkingComponent.ɵfac = function PlantumlWorkingComponent_Factory(t) { return new (t || PlantumlWorkingComponent)(); };
PlantumlWorkingComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: PlantumlWorkingComponent, selectors: [["app-plantuml-working"]], decls: 4, vars: 0, consts: [["href", "https://plantuml.com", "target", "_blank"], ["src", "/assets/Plantuml_Logo.svg", 1, "rise-shake"]], template: function PlantumlWorkingComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "a", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2, " Thanks Plantuml.com!");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](3, "img", 1);
    } }, styles: ["img.rise-shake[_ngcontent-%COMP%] {\n  animation: jump-shaking 0.83s infinite;\n}\n\n@keyframes jump-shaking {\n  0% {\n    transform: translateX(0);\n  }\n  25% {\n    transform: translateY(-9px);\n  }\n  35% {\n    transform: translateY(-9px) rotate(17deg);\n  }\n  55% {\n    transform: translateY(-9px) rotate(-17deg);\n  }\n  65% {\n    transform: translateY(-9px) rotate(17deg);\n  }\n  75% {\n    transform: translateY(-9px) rotate(-17deg);\n  }\n  100% {\n    transform: translateY(0) rotate(0);\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxccGxhbnR1bWwtd29ya2luZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLHNDQUFBO0FBQ0Y7O0FBR0E7RUFDRTtJQUNFLHdCQUFBO0VBQUY7RUFHQTtJQUNFLDJCQUFBO0VBREY7RUFJQTtJQUNFLHlDQUFBO0VBRkY7RUFLQTtJQUNFLDBDQUFBO0VBSEY7RUFNQTtJQUNFLHlDQUFBO0VBSkY7RUFPQTtJQUNFLDBDQUFBO0VBTEY7RUFRQTtJQUNFLGtDQUFBO0VBTkY7QUFDRiIsImZpbGUiOiJwbGFudHVtbC13b3JraW5nLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiaW1nLnJpc2Utc2hha2Uge1xyXG4gIGFuaW1hdGlvbjoganVtcC1zaGFraW5nIDAuODNzIGluZmluaXRlO1xyXG59XHJcblxyXG5cclxuQGtleWZyYW1lcyBqdW1wLXNoYWtpbmcge1xyXG4gIDAlIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWCgwKVxyXG4gIH1cclxuXHJcbiAgMjUlIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtOXB4KVxyXG4gIH1cclxuXHJcbiAgMzUlIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtOXB4KSByb3RhdGUoMTdkZWcpXHJcbiAgfVxyXG5cclxuICA1NSUge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKC05cHgpIHJvdGF0ZSgtMTdkZWcpXHJcbiAgfVxyXG5cclxuICA2NSUge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKC05cHgpIHJvdGF0ZSgxN2RlZylcclxuICB9XHJcblxyXG4gIDc1JSB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTlweCkgcm90YXRlKC0xN2RlZylcclxuICB9XHJcblxyXG4gIDEwMCUge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKDApIHJvdGF0ZSgwKVxyXG4gIH1cclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "hFUY":
/*!***************************************************************!*\
  !*** ./src/app/md-explorer/services/md-navigation.service.ts ***!
  \***************************************************************/
/*! exports provided: MdNavigationService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdNavigationService", function() { return MdNavigationService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class MdNavigationService {
    constructor() {
        this.navigation = [];
        this.navigationGhost = [];
        this.currentIndex = 0;
    }
    back() {
        this.navigationGhost.pop(); // throw away the item into the copy    
        this.currentIndex = this.navigationGhost.length - 1;
        let currentMdFile = this.navigation[this.currentIndex];
        // Navigation back: index " + this.currentIndex
        return currentMdFile;
    }
    forward() {
        this.currentIndex = this.currentIndex + 1;
        let currentMdFile = this.navigation[this.currentIndex];
        this.navigationGhost.push(currentMdFile);
        // Navigation forward: index " + this.currentIndex
        return currentMdFile;
    }
    resetNavigation() {
        this.navigationGhost = [];
        this.navigation = [];
    }
    setNewNavigation(currentMdFile) {
        // Adding to navigation: " + currentMdFile.fullPath
        if (this.navigationGhost.length >= 1) {
            // Checking if current file matches last in navigation
        }
        if ((this.navigationGhost.length - 1) >= 0 //check its not at beginning of navigation
            && currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath) {
            // Same file as current, skipping
            return; //DO NOTHING
        }
        this.navigationGhost.push(currentMdFile);
        this.navigation = this.deepCopyArray(this.navigationGhost);
        this.currentIndex = this.navigationGhost.length - 1; // index i 0 based, length is 1 based
        // Navigation updated, length: " + this.navigation.length
    }
    deepCopyArray(array) {
        return JSON.parse(JSON.stringify(array));
    }
}
MdNavigationService.ɵfac = function MdNavigationService_Factory(t) { return new (t || MdNavigationService)(); };
MdNavigationService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: MdNavigationService, factory: MdNavigationService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "jX2R":
/*!*****************************************************************************!*\
  !*** ./src/app/signalR/dialogs/connection-lost/connection-lost.provider.ts ***!
  \*****************************************************************************/
/*! exports provided: ConnectionLostProvider */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ConnectionLostProvider", function() { return ConnectionLostProvider; });
/* harmony import */ var _connection_lost_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./connection-lost.component */ "9LnC");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");



class ConnectionLostProvider {
    constructor(dialog) {
        this.dialog = dialog;
    }
    show(hub) {
        this._dialogRef = this.dialog.open(_connection_lost_component__WEBPACK_IMPORTED_MODULE_0__["ConnectionLostComponent"], {
            data: null
        });
        this._dialogRef.afterClosed().subscribe(_ => {
            hub.startConnection();
        });
        return this;
    }
    showConsoleClosed() {
        console.log('showConsoleClosed');
        this._dialogRef = this.dialog.open(_connection_lost_component__WEBPACK_IMPORTED_MODULE_0__["ConnectionLostComponent"], {
            data: 'serverIsDown'
        });
    }
    hide(data) {
        this._dialogRef.close();
    }
}
ConnectionLostProvider.ɵfac = function ConnectionLostProvider_Factory(t) { return new (t || ConnectionLostProvider)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialog"])); };
ConnectionLostProvider.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: ConnectionLostProvider, factory: ConnectionLostProvider.ɵfac });


/***/ }),

/***/ "jwHG":
/*!***********************************************************************!*\
  !*** ./src/app/git/components/git-messages/git-messages.component.ts ***!
  \***********************************************************************/
/*! exports provided: GitMessagesComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitMessagesComponent", function() { return GitMessagesComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");


class GitMessagesComponent {
    constructor(data) {
        this.data = data;
    }
    ngOnInit() {
    }
}
GitMessagesComponent.ɵfac = function GitMessagesComponent_Factory(t) { return new (t || GitMessagesComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"])); };
GitMessagesComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: GitMessagesComponent, selectors: [["app-git-messages"]], decls: 4, vars: 2, template: function GitMessagesComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.data.message);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.data.description);
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJnaXQtbWVzc2FnZXMuY29tcG9uZW50LnNjc3MifQ== */"] });


/***/ }),

/***/ "l3hs":
/*!********************************************!*\
  !*** ./src/app/services/search.service.ts ***!
  \********************************************/
/*! exports provided: SearchService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SearchService", function() { return SearchService; });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");



class SearchService {
    constructor(http) {
        this.http = http;
        this.baseUrl = '../api/search';
        console.log('[SearchService] Service initialized');
    }
    quickSearch(term, maxResults = 20) {
        console.log(`[SearchService] Quick search for: ${term}`);
        const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]()
            .set('term', term)
            .set('maxResults', maxResults.toString());
        return this.http.get(`${this.baseUrl}/quick`, { params });
    }
    advancedSearch(request) {
        console.log(`[SearchService] Advanced search:`, request);
        return this.http.post(`${this.baseUrl}/advanced`, request);
    }
    searchFiles(term, maxResults = 50) {
        console.log(`[SearchService] Search files for: ${term}`);
        const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]()
            .set('term', term)
            .set('maxResults', maxResults.toString());
        return this.http.get(`${this.baseUrl}/files`, { params });
    }
    searchLinks(term, maxResults = 50) {
        console.log(`[SearchService] Search links for: ${term}`);
        const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]()
            .set('term', term)
            .set('maxResults', maxResults.toString());
        return this.http.get(`${this.baseUrl}/links`, { params });
    }
}
SearchService.ɵfac = function SearchService_Factory(t) { return new (t || SearchService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"])); };
SearchService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: SearchService, factory: SearchService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "l94Z":
/*!*************************************************************************************!*\
  !*** ./src/app/signalR/dialogs/opening-application/opening-application.provider.ts ***!
  \*************************************************************************************/
/*! exports provided: OpeningApplicationProvider */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OpeningApplicationProvider", function() { return OpeningApplicationProvider; });
/* harmony import */ var _opening_application_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./opening-application.component */ "mLxA");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");



class OpeningApplicationProvider {
    constructor(dialog) {
        this.dialog = dialog;
    }
    show(data) {
        this._dialogRef = this.dialog.open(_opening_application_component__WEBPACK_IMPORTED_MODULE_0__["OpeningApplicationComponent"], {
            data: data
        });
        return this;
    }
    hide(data) {
        this._dialogRef.close();
    }
}
OpeningApplicationProvider.ɵfac = function OpeningApplicationProvider_Factory(t) { return new (t || OpeningApplicationProvider)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialog"])); };
OpeningApplicationProvider.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: OpeningApplicationProvider, factory: OpeningApplicationProvider.ɵfac });


/***/ }),

/***/ "mLxA":
/*!**************************************************************************************!*\
  !*** ./src/app/signalR/dialogs/opening-application/opening-application.component.ts ***!
  \**************************************************************************************/
/*! exports provided: OpeningApplicationComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OpeningApplicationComponent", function() { return OpeningApplicationComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class OpeningApplicationComponent {
    constructor() { }
    ngOnInit() {
    }
}
OpeningApplicationComponent.ɵfac = function OpeningApplicationComponent_Factory(t) { return new (t || OpeningApplicationComponent)(); };
OpeningApplicationComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: OpeningApplicationComponent, selectors: [["app-opening-application"]], decls: 6, vars: 0, template: function OpeningApplicationComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, "MarkDown is opening an application");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "h3");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "Please check on you computer");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5, "Remember to close application, before commit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJvcGVuaW5nLWFwcGxpY2F0aW9uLmNvbXBvbmVudC5zY3NzIn0= */"] });


/***/ }),

/***/ "n3Jk":
/*!***********************************************************!*\
  !*** ./src/app/interceptors/connection-id.interceptor.ts ***!
  \***********************************************************/
/*! exports provided: ConnectionIdInterceptor */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ConnectionIdInterceptor", function() { return ConnectionIdInterceptor; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ "+dpY");


/**
 * HTTP Interceptor that automatically adds connectionId to all API requests.
 * This ensures that the backend can identify the client and use the correct
 * per-client DatabaseManager context.
 */
class ConnectionIdInterceptor {
    constructor(mdServerMessages) {
        this.mdServerMessages = mdServerMessages;
    }
    intercept(req, next) {
        // Only add connectionId to API requests (not external URLs)
        if (!req.url.startsWith('../api/') && !req.url.startsWith('/api/')) {
            return next.handle(req);
        }
        // Skip if connectionId is already in the URL
        if (req.url.includes('connectionId=') || req.url.includes('ConnectionId=')) {
            return next.handle(req);
        }
        const connectionId = this.mdServerMessages.connectionId;
        // Skip if connectionId is not yet available
        if (!connectionId) {
            console.error('[ConnectionIdInterceptor] ❌ connectionId is NULL for request:', req.url);
            console.error('[ConnectionIdInterceptor] mdServerMessages:', this.mdServerMessages);
            return next.handle(req);
        }
        // Add connectionId as query parameter
        const modifiedReq = req.clone({
            setParams: {
                ConnectionId: connectionId,
            },
        });
        console.log('[ConnectionIdInterceptor] ✅ Added connectionId to:', req.url);
        return next.handle(modifiedReq);
    }
}
ConnectionIdInterceptor.ɵfac = function ConnectionIdInterceptor_Factory(t) { return new (t || ConnectionIdInterceptor)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_1__["MdServerMessagesService"])); };
ConnectionIdInterceptor.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: ConnectionIdInterceptor, factory: ConnectionIdInterceptor.ɵfac });


/***/ }),

/***/ "oPln":
/*!******************************************************************************!*\
  !*** ./src/app/signalR/dialogs/parsing-project/parsing-project.component.ts ***!
  \******************************************************************************/
/*! exports provided: ParsingProjectComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ParsingProjectComponent", function() { return ParsingProjectComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");



class ParsingProjectComponent {
    constructor(data, dialogRef) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.folder$ = data.folder$;
        dialogRef.disableClose = true;
    }
    ngOnInit() {
    }
}
ParsingProjectComponent.ɵfac = function ParsingProjectComponent_Factory(t) { return new (t || ParsingProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"])); };
ParsingProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ParsingProjectComponent, selectors: [["app-parsing-project"]], decls: 3, vars: 1, template: function ParsingProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Indexing documents");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("\n", ctx.folder$.value, "\n");
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJwYXJzaW5nLXByb2plY3QuY29tcG9uZW50LnNjc3MifQ== */"] });


/***/ }),

/***/ "octk":
/*!*************************************!*\
  !*** ./src/environments/version.ts ***!
  \*************************************/
/*! exports provided: versionInfo */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "versionInfo", function() { return versionInfo; });
// Questo file è generato automaticamente dallo script update-version.js
// Non modificarlo manualmente.
const versionInfo = {
    version: '2026.01.12.3',
    buildTime: '2026.01.12 15:28:11'
};


/***/ }),

/***/ "vUCT":
/*!**********************************************************!*\
  !*** ./src/app/md-explorer/services/projects.service.ts ***!
  \**********************************************************/
/*! exports provided: ProjectsService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsService", function() { return ProjectsService; });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! tslib */ "mrSG");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../models/compatibility-mode.model */ "25Jb");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "tk/3");





class ProjectsService {
    constructor(http, injector) {
        this.http = http;
        this.injector = injector;
        this.currentProjects$ = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"](null);
        // Emette PRIMA che il progetto cambi (per mostrare skeleton loader)
        this.projectChangingSubject = new rxjs__WEBPACK_IMPORTED_MODULE_1__["Subject"]();
        this.projectChanging$ = this.projectChangingSubject.asObservable();
        // Track current project's chat room info for cleanup
        this.currentRoomId = null;
        this.currentOderId = null;
        this.dataStore = { mdProjects: [] };
        this._mdProjects = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
    }
    get mdProjects() {
        return this._mdProjects.asObservable();
    }
    fetchProjects() {
        const url = '../api/MdProjects/GetProjects';
        this.http.get(url)
            .subscribe(data => {
            this.dataStore.mdProjects = data;
            this._mdProjects.next(Object.assign({}, this.dataStore).mdProjects);
        }, error => {
            console.log(error);
        });
    }
    SetSideNavWidth(mdProject) {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            const url = '../api/MdProjects/SetSideNavWidth';
            yield this.http.post(url, mdProject).toPromise();
        });
    }
    setNewFolderProject(path) {
        this.projectChangingSubject.next(); // Notifica cambio progetto in corso
        // Close previous project if any
        this.notifyProjectClosed();
        this.http.post('../api/MdProjects/SetFolderProject', { path: path }).subscribe((response) => Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            this.currentProjects$.next(response);
            // Update window title for Electron taskbar preview
            this.updateWindowTitle(response.name);
            // Register project open for chat presence tracking
            this.notifyProjectOpened(path);
            // Update compatibility mode from response
            if (response.compatibilityMode) {
                const mode = response.compatibilityMode === 'github' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub :
                    response.compatibilityMode === 'commonmark' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark :
                        _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
                console.log('Setting compatibility mode from project open response:', mode);
                // Get CompatibilityModeService using dynamic import to avoid circular dependency
                const { CompatibilityModeService } = yield Promise.resolve(/*! import() */).then(__webpack_require__.bind(null, /*! ../../services/compatibility-mode.service */ "4rFV"));
                const compatibilityService = this.injector.get(CompatibilityModeService);
                compatibilityService.updateMode(mode);
            }
        }));
    }
    createProjectWithConfig(config) {
        this.projectChangingSubject.next(); // Notifica cambio progetto in corso
        // Close previous project if any
        this.notifyProjectClosed();
        const request = {
            path: config.projectPath,
            initializeGit: config.initializeGit,
            addCopilotInstructions: config.addCopilotInstructions
        };
        this.http.post('../api/MdProjects/SetFolderProject', request).subscribe((response) => Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            this.currentProjects$.next(response);
            // Update window title for Electron taskbar preview
            this.updateWindowTitle(response.name);
            // Register project open for chat presence tracking
            this.notifyProjectOpened(config.projectPath);
            // Update compatibility mode from response
            if (response.compatibilityMode) {
                const mode = response.compatibilityMode === 'github' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].GitHub :
                    response.compatibilityMode === 'commonmark' ? _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].CommonMark :
                        _models_compatibility_mode_model__WEBPACK_IMPORTED_MODULE_2__["CompatibilityMode"].MdExplorer;
                console.log('Setting compatibility mode from project create response:', mode);
                // Get CompatibilityModeService using dynamic import to avoid circular dependency
                const { CompatibilityModeService } = yield Promise.resolve(/*! import() */).then(__webpack_require__.bind(null, /*! ../../services/compatibility-mode.service */ "4rFV"));
                const compatibilityService = this.injector.get(CompatibilityModeService);
                compatibilityService.updateMode(mode);
            }
        }), error => {
            console.error('Error creating project with config:', error);
        });
    }
    //setNewFolderProject(path: string, callback: (data: any, objectThis: any) => any, objectThis: any) {
    //  const url = '../api/MdProjects/SetFolderProject';
    //  this.http.post<any>(url, { path: path }).subscribe(data => {
    //    callback(data, objectThis);
    //  });
    //}
    deleteProject(project, callback, objectThis) {
        const url = '../api/MdProjects/DeleteProject';
        this.http.post(url, project).subscribe(data => {
            callback(data, objectThis);
        });
    }
    /**
     * Closes the current project and deallocates backend resources (FileSystemWatcher, database contexts).
     * Should be called when navigating back to the projects list.
     */
    closeCurrentProject() {
        // Notify chat system that project is being closed
        this.notifyProjectClosed();
        // Reset window title
        this.updateWindowTitle(null);
        return this.http.post('../api/MdProjects/CloseProject', {});
    }
    /**
     * Re-registers the current project with the backend after a SignalR reconnection.
     * This is necessary because when the SignalR connection is lost, the backend
     * cleans up FileSystemWatcher and DatabaseContext for the old ConnectionId.
     * After reconnection with a new ConnectionId, we need to re-register.
     */
    reregisterCurrentProject() {
        const currentProject = this.currentProjects$.getValue();
        if (currentProject && currentProject.path) {
            console.log('[ProjectsService] Re-registering project after SignalR reconnection:', currentProject.path);
            this.http.post('../api/MdProjects/SetFolderProject', { path: currentProject.path }).subscribe(response => {
                console.log('[ProjectsService] Project re-registered successfully');
                // Update the project in case any settings changed
                this.currentProjects$.next(response);
                // Update window title
                this.updateWindowTitle(response.name);
            }, error => {
                console.error('[ProjectsService] Failed to re-register project:', error);
            });
        }
        else {
            console.log('[ProjectsService] No current project to re-register');
        }
    }
    /**
     * Notify the chat system that a project has been opened.
     * This registers the user in the project users count.
     */
    notifyProjectOpened(projectPath) {
        this.http.post('../api/GitChat/project-opened', {
            repositoryPath: projectPath
        }).subscribe(response => {
            if (response.success) {
                this.currentRoomId = response.roomId || null;
                this.currentOderId = response.oderId || null;
                console.log('[ProjectsService] Project opened registered, roomId:', response.roomId, 'oderId:', response.oderId, 'users:', response.projectUsersCount);
            }
            else {
                // Not a git repo or no remote - this is fine, just don't track
                console.log('[ProjectsService] Project opened but not tracking (no git remote):', response.error);
                this.currentRoomId = null;
                this.currentOderId = null;
            }
        }, error => {
            console.warn('[ProjectsService] Failed to register project open:', error);
            this.currentRoomId = null;
            this.currentOderId = null;
        });
    }
    /**
     * Notify the chat system that a project has been closed.
     * This unregisters the user from the project users count.
     */
    notifyProjectClosed() {
        if (this.currentRoomId && this.currentOderId) {
            this.http.post('../api/GitChat/project-closed', {
                roomId: this.currentRoomId,
                oderId: this.currentOderId
            }).subscribe(response => {
                console.log('[ProjectsService] Project closed registered');
            }, error => {
                console.warn('[ProjectsService] Failed to register project close:', error);
            });
            this.currentRoomId = null;
            this.currentOderId = null;
        }
    }
    /**
     * Updates the Electron window title to show the project name in taskbar preview.
     * Only works when running in Electron.
     */
    updateWindowTitle(projectName) {
        var _a;
        if ((_a = window.electronAPI) === null || _a === void 0 ? void 0 : _a.setWindowTitle) {
            const title = projectName ? `${projectName} - MdExplorer` : 'MdExplorer';
            window.electronAPI.setWindowTitle(title);
        }
    }
}
ProjectsService.ɵfac = function ProjectsService_Factory(t) { return new (t || ProjectsService)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["Injector"])); };
ProjectsService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({ token: ProjectsService, factory: ProjectsService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "xmhS":
/*!*********************************************************!*\
  !*** ./src/app/md-explorer/services/md-file.service.ts ***!
  \*********************************************************/
/*! exports provided: MdFileService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdFileService", function() { return MdFileService; });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ "+dpY");






class MdFileService {
    constructor(http, mdServerMessages, injector) {
        this.http = http;
        this.mdServerMessages = mdServerMessages;
        this.injector = injector;
        this._navigationArray = []; // deve morire
        var defaultSelectedMdFile = [];
        this.dataStore = {
            mdFiles: [],
            mdFoldersDocument: [],
            mdDynFolderDocument: [],
            serverSelectedMdFile: defaultSelectedMdFile
        };
        this._mdFiles = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._mdDynFolderDocument = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._serverSelectedMdFile = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._selectedMdFileFromToolbar = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._selectedMdFileFromSideNav = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"](null);
        this._selectedDirectoryFromNewDirectory = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"](null);
        this._whatDisplayForToolbar = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]('block');
        // Subscribe to Git branch switch events to refresh tree
        this.mdServerMessages.gitBranchSwitched$.subscribe((data) => {
            console.log('🌳 Git branch switched - refreshing tree. Files indexed:', data.fileCount);
            // Use loadAll() to properly update dataStore and notify subscribers
            this.loadAll(null, null);
        });
        // Subscribe to project changing events - clear data to show skeleton loader
        // Use setTimeout to avoid circular dependency issues
        setTimeout(() => {
            const { ProjectsService } = __webpack_require__(/*! ./projects.service */ "vUCT");
            const projectsService = this.injector.get(ProjectsService);
            projectsService.projectChanging$.subscribe(() => {
                console.log('🔄 Project changing - clearing tree data for skeleton');
                this.dataStore.mdFiles = [];
                this._mdFiles.next([]);
            });
        }, 0);
    }
    get whatDisplayForToolbar() {
        return this._whatDisplayForToolbar.asObservable();
    }
    setWhatDisplayForToolbar(value) {
        this._whatDisplayForToolbar.next(value);
    }
    get mdFiles() {
        return this._mdFiles.asObservable();
    }
    get mdDynFolderDocument() {
        return this._mdDynFolderDocument.asObservable();
    }
    get serverSelectedMdFile() {
        return this._serverSelectedMdFile.asObservable();
    }
    get selectedMdFileFromToolbar() {
        return this._selectedMdFileFromToolbar.asObservable();
    }
    get selectedMdFileFromSideNav() {
        return this._selectedMdFileFromSideNav.asObservable();
    }
    get selectedDirectoryFromNewDirectory() {
        return this._selectedDirectoryFromNewDirectory.asObservable();
    }
    // breadcrumb
    get navigationArray() {
        return this._navigationArray;
    }
    set navigationArray(mdFile) {
        this._navigationArray = mdFile;
    }
    moveMdFile(mdFile, pathDestination) {
        const url = '../api/mdfiles/MoveMdFile';
        return this.http.post(url, { mdFile: mdFile, destinationPath: pathDestination });
    }
    openInheritingTemplateWord(InheringTemplate) {
        const url = '../api/mdFiles/openinheritingtemplateWord';
        return this.http.post(url, { templateName: InheringTemplate });
    }
    opencustomwordtemplate(mdFile) {
        const url = '../api/mdFiles/opencustomwordtemplate';
        return this.http.post(url, mdFile);
    }
    setDocumentSettings(documentDescriptor, mdFile) {
        const url = '../api/mdFiles/setdocumentsettings';
        return this.http.post(url, { documentDescriptor, mdFile });
    }
    getDocumentSettings(mdFile) {
        const url = '../api/mdFiles/getdocumentsettings';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('fullPath', mdFile.fullPath);
        return this.http.get(url, { params });
    }
    // This function adds a new file,
    // looking for the right position in the
    // folder hierarchy.
    // It assumes that all structures are complete,
    // and the only thing to add is the file itself.
    addNewFile(data) {
        var _a, _b;
        // searching directories    
        const currentItem = data[0];
        // Assicuriamoci che le proprietà di indicizzazione siano preservate
        if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
            // Preserva le proprietà esistenti o imposta i default
            currentItem.isIndexed = (_a = currentItem.isIndexed) !== null && _a !== void 0 ? _a : true; // Default true per nuovi file
            currentItem.indexingStatus = (_b = currentItem.indexingStatus) !== null && _b !== void 0 ? _b : 'completed';
        }
        const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);
        if (currentFolder) {
            this.recursiveSearchFolder(data, 0, currentFolder);
        }
        else {
            // The file is in the root
            const dummyItem = this.dataStore.mdFiles.pop();
            this.dataStore.mdFiles.push(currentItem, dummyItem); // Simplified push operation
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles); // Simplified object cloning and notification
        }
    }
    // This function adds new directories
    // if one or more on the file path are missing.
    // At the end of the process, it will call the classic addNewFile method.
    addNewDirectoryExtended(folders) {
        let currentfolder = [];
        folders.forEach((folder, index) => {
            const dataFound = [];
            this.recursiveSearch(this.dataStore.mdFiles, folder, dataFound);
            currentfolder.push(folder);
            if (dataFound.length === 0) {
                this.addNewDirectory(currentfolder);
            }
        });
    }
    // This function adds a new directory.
    // Assuming that all directories/folders are already present,
    // and there is just one to add consequently to
    // what already exists in the store.
    addNewDirectory(data) {
        //alert(JSON.stringify(data, null, 2));
        // Initialize the current item and mark it as expandable
        const currentItem = data[0];
        currentItem.expandable = true;
        // Search for the directory in the current datastore
        const currentFolder = this.dataStore.mdFiles.find(item => item.fullPath == currentItem.fullPath);
        if (currentFolder) {
            // If found, perform a recursive search to insert the directory
            this.recursiveSearchFolder(data, 0, currentFolder);
        }
        else {
            // If the directory is in the root, handle the dummy item and reinsert
            const dummyItem = this.dataStore.mdFiles.pop(); // Remove the last item (dummy)
            this.dataStore.mdFiles.push(currentItem, dummyItem); // Add the current item and then the dummy back
            // Notify subscribers of the update
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
        }
    }
    recursiveSearchFolder(data, i, parentFolder) {
        var _a, _b;
        const currentItem = data[i + 1];
        if (!currentItem)
            return; // Guard clause
        // Assicuriamoci che le proprietà di indicizzazione siano preservate
        if (currentItem.type === 'mdFile' || currentItem.type === 'mdFileTimer') {
            currentItem.isIndexed = (_a = currentItem.isIndexed) !== null && _a !== void 0 ? _a : true;
            currentItem.indexingStatus = (_b = currentItem.indexingStatus) !== null && _b !== void 0 ? _b : 'completed';
        }
        const currentFolder = parentFolder.childrens.find(folder => folder.fullPath == currentItem.fullPath);
        if (currentFolder) {
            this.recursiveSearchFolder(data, i + 1, currentFolder);
        }
        else {
            parentFolder.childrens.push(currentItem); // Directly use currentItem
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles); // Simplified notification
        }
    }
    getShallowStructure() {
        return this.http.get('../api/mdfiles/GetShallowStructure');
    }
    loadAll(callback, objectThis) {
        return this.http.get('../api/mdfiles/GetShallowStructure')
            .subscribe(data => {
            // Assicuriamo che tutte le proprietà siano definite fin dall'inizio
            this.initializeIndexingProperties(data);
            // Compatta le cartelle annidate (VS Code-style)
            this.compactFolders(data);
            this.dataStore.mdFiles = data;
            this._mdFiles.next([...this.dataStore.mdFiles]);
            if (callback != null) {
                callback(data, objectThis);
            }
        }, error => {
            console.log("failed to fetch mdfile list");
        });
    }
    initializeIndexingProperties(nodes) {
        nodes.forEach(node => {
            var _a, _b;
            // Assicura che le proprietà esistano fin dall'inizio
            if (node.type === 'mdFile' || node.type === 'mdFileTimer') {
                node.isIndexed = (_a = node.isIndexed) !== null && _a !== void 0 ? _a : false;
                node.indexingStatus = (_b = node.indexingStatus) !== null && _b !== void 0 ? _b : 'idle';
            }
            if (node.childrens && node.childrens.length > 0) {
                this.initializeIndexingProperties(node.childrens);
            }
        });
    }
    /**
     * Compatta catene di cartelle con un solo figlio cartella (VS Code-style)
     * Es: src/main/java/com viene mostrato come "src / main / java / com" su una riga
     */
    compactFolders(nodes) {
        nodes.forEach(node => this.compactSingleNode(node));
    }
    compactSingleNode(node) {
        var _a, _b, _c;
        if (node.type !== 'folder' || !((_a = node.childrens) === null || _a === void 0 ? void 0 : _a.length)) {
            return;
        }
        // Raccogli i segmenti della catena
        const segments = [
            { name: node.name, fullPath: node.fullPath, level: node.level }
        ];
        let current = node;
        let lastCompactedLevel = node.level;
        // Segui la catena finché c'è esattamente 1 figlio che è una cartella
        while (((_b = current.childrens) === null || _b === void 0 ? void 0 : _b.length) === 1 && current.childrens[0].type === 'folder') {
            current = current.childrens[0];
            lastCompactedLevel++;
            segments.push({ name: current.name, fullPath: current.fullPath, level: lastCompactedLevel });
        }
        // Se abbiamo compresso almeno 2 livelli
        if (segments.length > 1) {
            node.isCompacted = true;
            node.compactedPath = segments.map(s => s.name).join(' / ');
            node.compactedSegments = segments;
            // I figli diventano quelli dell'ultimo nodo compresso
            node.childrens = current.childrens;
            // Il fullPath del nodo diventa quello dell'ultimo segmento per le operazioni di default
            // Ma manteniamo il path originale per la visualizzazione
        }
        // Processa ricorsivamente i figli (che ora sono i figli dell'ultimo nodo compresso se compattato)
        if ((_c = node.childrens) === null || _c === void 0 ? void 0 : _c.length) {
            node.childrens.forEach(child => this.compactSingleNode(child));
        }
    }
    updateFileIndexStatus(path, isIndexed) {
        // Ricostruisce completamente l'array invece di modificare gli oggetti esistenti
        const updateNodeInArray = (nodes) => {
            return nodes.map(node => {
                if (node.fullPath === path) {
                    // Crea un nuovo oggetto invece di modificare quello esistente
                    return Object.assign(Object.assign({}, node), { isIndexed: isIndexed, indexingStatus: isIndexed ? 'completed' : 'idle' });
                }
                if (node.childrens && node.childrens.length > 0) {
                    return Object.assign(Object.assign({}, node), { childrens: updateNodeInArray(node.childrens) });
                }
                return node;
            });
        };
        // Ricostruisce completamente l'array
        this.dataStore.mdFiles = updateNodeInArray(this.dataStore.mdFiles);
        // Emette il nuovo array
        this._mdFiles.next([...this.dataStore.mdFiles]);
    }
    // Forza aggiornamento stato indicizzazione per file rinominati Rule #1
    forceFileAsIndexed(filePath) {
        this.updateFileIndexStatus(filePath, true);
        setTimeout(() => {
            this.mdServerMessages.triggerRule1ForceUpdate(filePath);
        }, 100);
    }
    loadDynFolders(path, level) {
        const url = '../api/mdfiles/GetDynFoldersDocument';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('path', path).set('level', String(level));
        return this.http.get(url, { params })
            .subscribe(data => {
            if (this.dataStore.mdDynFolderDocument.length > 0) {
                //var test = this.dataStore.mdDynFolderDocument.find(_ => _.path == path);
                //test.children = data;
            }
            else {
                this.dataStore.mdDynFolderDocument = data;
            }
            this._mdDynFolderDocument.next(Object.assign({}, this.dataStore).mdDynFolderDocument);
        }, error => {
            console.log("failed to fetch mdfile list");
        });
    }
    loadDocumentFolder(path, level, typeOfSelection) {
        let url = '../api/mdfiles/GetDynFoldersDocument';
        if (typeOfSelection === "FoldersAndFiles") {
            url = '../api/mdfiles/GetDynFoldersAndFilesDocument';
        }
        console.log(url);
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('path', path).set('level', String(level));
        return this.http.get(url, { params });
    }
    loadPublishNodes(path, level) {
        const url = '../api/mdPublishNodes';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('path', path).set('level', String(level));
        return this.http.get(url, { params });
    }
    GetHtml(path) {
        const url = '../api/mdexplorer/' + path;
        return this.http.get(url, { responseType: 'text' }); //, currentFile      
    }
    getLandingPage() {
        const url = '../api/mdfiles/GetLandingPage';
        return this.http.get(url);
    }
    SetLandingPage(file) {
        const url = '../api/mdfiles/SetLandingPage';
        return this.http.post(url, file);
    }
    setDevelopmentTags(folder, projectRoot, tags) {
        const url = '../api/mdfiles/SetDevelopmentTags';
        return this.http.post(url, {
            folderPath: folder.fullPath,
            projectRoot: projectRoot,
            tags: tags
        });
    }
    openFolderOnFileExplorer(file) {
        console.log('[MdFileService] openFolderOnFileExplorer() called');
        console.log('[MdFileService] file:', file);
        console.log('[MdFileService] file.fullPath:', file.fullPath);
        const url = '../api/mdfiles/OpenFolderOnFileExplorer';
        console.log('[MdFileService] POST to:', url);
        return this.http.post(url, file).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["tap"])(response => {
            console.log('[MdFileService] Response received:', response);
        }), Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["catchError"])(error => {
            console.error('[MdFileService] Error in openFolderOnFileExplorer:', error);
            throw error;
        }));
    }
    deleteFile(file) {
        const url = '../api/mdfiles/DeleteFile';
        return this.http.post(url, file);
        //this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
    //Minimum information to set
    // 1. fullPath:ex: "C:\Users\Carlo\Documents\2-personale\sviluppo\MdExplorer\UnitTestMdExplorer\RockSolidEdition\using-chatGPT\eargaer.md"
    // 2. level: not important
    recursiveDeleteFileFromDataStore(fileToFind) {
        const dataFound = [];
        this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);
        if (dataFound.length === 1) {
            const dataIndex = this.dataStore.mdFiles.indexOf(dataFound[0]);
            if (dataIndex > -1) {
                this.dataStore.mdFiles.splice(dataIndex, 1);
            }
        }
        if (dataFound.length > 1) {
            //let cursor = this.dataStore.mdFiles;
            let currentFolder = this.dataStore.mdFiles;
            for (var i = dataFound.length - 1; i > 0; i--) {
                currentFolder = currentFolder[currentFolder.indexOf(dataFound[i])].childrens;
            }
            currentFolder.splice(currentFolder.indexOf(dataFound[0]), 1);
        }
        this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
    recursiveSearchForShowData(fileToFind) {
        let dataFound = [];
        this.recursiveSearch(this.dataStore.mdFiles, fileToFind, dataFound);
        return dataFound;
    }
    CreateNewDirectoryEx(path, directoryName, directoryLevel) {
        const url = '../api/mdfiles/CreateNewDirectoryEx';
        var newData = {
            directoryPath: path,
            directoryName: directoryName,
            directoryLevel: directoryLevel,
        };
        return this.http.post(url, newData);
    }
    CreateNewDirectory(path, directoryName, directoryLevel) {
        const url = '../api/mdfiles/CreateNewDirectory';
        var newData = {
            directoryPath: path,
            directoryName: directoryName,
            directoryLevel: directoryLevel,
        };
        return this.http.post(url, newData);
    }
    RenameDirectory(path, directoryName, directoryLevel) {
        const url = '../api/mdfiles/RenameDirectory';
        var newData = {
            directoryPath: path,
            directoryName: directoryName,
            directoryLevel: directoryLevel,
        };
        return this.http.post(url, newData);
    }
    pasteFromClipboard(node) {
        const url = '../api/mdfiles/pasteFromClipboard';
        console.log('[MdFileService] pasteFromClipboard called with:', node);
        console.log('[MdFileService] Making POST request to:', url);
        return this.http.post(url, node);
    }
    addExistingFileToMDEProject(node, path) {
        const url = '../api/mdfiles/addExistingFileToMDEProject';
        return this.http.post(url, { mdFile: node, fullPath: path });
    }
    getTextFromClipboard() {
        const url = '../api/mdfiles/getTextFromClipboard';
        return this.http.get(url);
    }
    cloneTimerDocument(node) {
        const url = '../api/mdfiles/CloneTimerMd';
        return this.http.post(url, node);
    }
    CreateNewMd(path, title, directoryLevel, documentTypeId, documentType) {
        const url = '../api/mdfiles/CreateNewMd';
        var newData = {
            directoryPath: path,
            title: title,
            directoryLevel: directoryLevel,
            documentTypeId: documentTypeId,
            documentType: documentType
        };
        return this.http.post(url, newData);
    }
    //fileFoundMd: boolean = false;
    /**
     * Funzione di sostituzione di un nodo, con un altro
     * @param oldFile
     * @param newFile
     */
    changeDataStoreMdFiles(oldFile, newFile) {
        var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, oldFile);
        var leaf = returnFound[0];
        if (!leaf) {
            console.error('❌ [Service] File non trovato nel datastore:', oldFile.name);
            return;
        }
        // Aggiorna le proprietà del file
        leaf.name = newFile.name;
        leaf.fullPath = newFile.fullPath;
        leaf.path = newFile.path;
        leaf.relativePath = newFile.relativePath;
        // Per file rinominati via Rule #1, forza come indicizzato
        leaf.isIndexed = true;
        leaf.indexingStatus = 'completed';
        // Forza nuova referenza per triggerare OnPush change detection
        this._mdFiles.next([...this.dataStore.mdFiles]);
        this._serverSelectedMdFile.next([...returnFound]);
        // Notifica il tree component per aggiornare il Set di tracking
        this.mdServerMessages.triggerRule1ForceUpdate(leaf.fullPath);
    }
    setSelectedMdFileFromSideNav(selectedFile) {
        var _a;
        console.log('[MdFileService] setSelectedMdFileFromSideNav called with:', selectedFile);
        console.log('[MdFileService] _selectedMdFileFromSideNav value before:', this._selectedMdFileFromSideNav.value);
        console.log('[MdFileService] _selectedMdFileFromSideNav has observers:', ((_a = this._selectedMdFileFromSideNav.observers) === null || _a === void 0 ? void 0 : _a.length) || 0);
        this._selectedMdFileFromSideNav.next(selectedFile);
        console.log('[MdFileService] _selectedMdFileFromSideNav value after:', this._selectedMdFileFromSideNav.value);
    }
    setSelectedDirectoryFromNewDirectory(selectedDirectory) {
        this._selectedDirectoryFromNewDirectory.next(selectedDirectory);
    }
    setSelectedMdFileFromToolbar(selectedFile) {
        let returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
        this._selectedMdFileFromToolbar.next(returnFound);
    }
    setSelectedMdFileFromServer(selectedFile) {
        var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
        this._serverSelectedMdFile.next(returnFound);
    }
    setSelectionMdFile(selectedFile) {
        this._serverSelectedMdFile.next(selectedFile);
    }
    getMdFileFromDataStore(selectedFile) {
        var returnFound = this.searchMdFileIntoDataStore(this.dataStore.mdFiles, selectedFile);
        return returnFound[0];
    }
    searchMdFileIntoDataStore(arrayMd, FileToFind) {
        //this.fileFoundMd = false;
        var arrayFound = [];
        this.recursiveSearch(arrayMd, FileToFind, arrayFound);
        return arrayFound;
    }
    recursiveSearch(arrayMd, fileToFind, arrayFound) {
        if (arrayMd.length === 0) {
            return false;
        }
        const thatFile = arrayMd.find(item => item.fullPath.toLowerCase() === fileToFind.fullPath.toLowerCase());
        if (!thatFile) {
            return arrayMd.some(item => {
                const found = this.recursiveSearch(item.childrens, fileToFind, arrayFound);
                if (found) {
                    arrayFound.push(item);
                }
                return found;
            });
        }
        else {
            arrayFound.push(thatFile);
            return true;
        }
    }
    // New methods for file explorer functionality
    getSpecialFolders() {
        const url = '../api/mdfiles/GetSpecialFolders';
        return this.http.get(url);
    }
    getDrives() {
        const url = '../api/mdfiles/GetDrives';
        return this.http.get(url);
    }
    getNetworkShares() {
        const url = '../api/mdfiles/GetNetworkShares';
        return this.http.get(url);
    }
}
MdFileService.ɵfac = function MdFileService_Factory(t) { return new (t || MdFileService)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["Injector"])); };
MdFileService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({ token: MdFileService, factory: MdFileService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "yrD1":
/*!***********************************************************************************!*\
  !*** ./src/app/commons/components/show-file-system/show-file-system.component.ts ***!
  \***********************************************************************************/
/*! exports provided: DynamicDatabase, ShowFileSystemComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DynamicDatabase", function() { return DynamicDatabase; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ShowFileSystemComponent", function() { return ShowFileSystemComponent; });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! tslib */ "mrSG");
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/cdk/tree */ "FvrZ");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../new-directory/new-directory.component */ "TUMs");
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../md-explorer/models/md-file */ "aS6m");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _show_file_metadata__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./show-file-metadata */ "DEjE");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/divider */ "f0Cb");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");

























const _c0 = ["filterInput"];
function ShowFileSystemComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](8, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "button", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r17); const item_r15 = ctx.item; const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r16.createDirectoryOnImproved(item_r15); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "mat-icon", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](12, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](13, "New folder here");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](14, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](16, "button", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_16_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r17); const item_r15 = ctx.item; const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r18.openFolderOn(item_r15); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](18, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](19, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](20, "Open in File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](21, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](22, "button", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_22_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r17); const item_r15 = ctx.item; const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r19.refreshFolder(item_r15); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](23, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](24, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](25, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](26, "Refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](27, "button", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_27_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r17); const item_r15 = ctx.item; const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r20.copyPathToClipboard(item_r15.fullPath || item_r15.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](28, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](29, "content_copy");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](30, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](31, "Copy path");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const item_r15 = ctx.item;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", item_r15.type === "folder" ? "folder" : "insert_drive_file", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](item_r15.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](item_r15.fullPath || item_r15.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"]("Create as child of \"", item_r15.name, "\"");
} }
function ShowFileSystemComponent_span_14_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r3.filteredItems.length, " ");
} }
function ShowFileSystemComponent_mat_icon_15_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-icon", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_button_16_Template(rf, ctx) { if (rf & 1) {
    const _r22 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_button_16_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r22); const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r21.clearFilter(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_mat_chip_list_17_Template(rf, ctx) { if (rf & 1) {
    const _r24 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-chip-list", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-chip", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("removed", function ShowFileSystemComponent_mat_chip_list_17_Template_mat_chip_removed_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r24); const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r23.clearFilter(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "mat-icon", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "filter_alt");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "mat-icon", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6, "cancel");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" Filter: \"", ctx_r6.searchFilter, "\" ");
} }
function ShowFileSystemComponent_div_24_Template(rf, ctx) { if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_24_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r27); const folder_r25 = ctx.$implicit; const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r26.navigateToFolder(folder_r25.path); })("keydown.enter", function ShowFileSystemComponent_div_24_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r27); const folder_r25 = ctx.$implicit; const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r28.navigateToFolder(folder_r25.path); })("keydown.space", function ShowFileSystemComponent_div_24_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r27); const folder_r25 = ctx.$implicit; const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r29.navigateToFolder(folder_r25.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const folder_r25 = ctx.$implicit;
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx_r7.currentPath === folder_r25.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("aria-label", "Navigate to " + folder_r25.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](folder_r25.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](folder_r25.name);
} }
function ShowFileSystemComponent_div_29_Template(rf, ctx) { if (rf & 1) {
    const _r32 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_29_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32); const drive_r30 = ctx.$implicit; const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r31.navigateToFolder(drive_r30.path); })("keydown.enter", function ShowFileSystemComponent_div_29_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32); const drive_r30 = ctx.$implicit; const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r33.navigateToFolder(drive_r30.path); })("keydown.space", function ShowFileSystemComponent_div_29_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32); const drive_r30 = ctx.$implicit; const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r34.navigateToFolder(drive_r30.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const drive_r30 = ctx.$implicit;
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx_r8.currentPath === drive_r30.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("aria-label", "Navigate to drive " + drive_r30.label);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](drive_r30.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate2"]("", drive_r30.label, " (", drive_r30.name, ")");
} }
function ShowFileSystemComponent_div_30_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r38 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_30_div_4_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r38); const share_r36 = ctx.$implicit; const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r37.navigateToFolder(share_r36.path); })("keydown.enter", function ShowFileSystemComponent_div_30_div_4_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r38); const share_r36 = ctx.$implicit; const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r39.navigateToFolder(share_r36.path); })("keydown.space", function ShowFileSystemComponent_div_30_div_4_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r38); const share_r36 = ctx.$implicit; const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r40.navigateToFolder(share_r36.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const share_r36 = ctx.$implicit;
    const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx_r35.currentPath === share_r36.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("aria-label", "Navigate to network share " + share_r36.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](share_r36.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](share_r36.name);
} }
function ShowFileSystemComponent_div_30_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "h3", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "Network");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](4, ShowFileSystemComponent_div_30_div_4_Template, 5, 5, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r9.networkShares)("ngForTrackBy", ctx_r9.trackByPath);
} }
function ShowFileSystemComponent_ng_container_37_mat_icon_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-icon", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const segment_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](segment_r41.icon);
} }
function ShowFileSystemComponent_ng_container_37_mat_icon_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-icon", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1, "chevron_right");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_ng_container_37_Template(rf, ctx) { if (rf & 1) {
    const _r48 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "button", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_container_37_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r48); const segment_r41 = ctx.$implicit; const ctx_r47 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r47.navigateToBreadcrumb(segment_r41); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, ShowFileSystemComponent_ng_container_37_mat_icon_2_Template, 2, 1, "mat-icon", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](5, ShowFileSystemComponent_ng_container_37_mat_icon_5_Template, 2, 0, "mat-icon", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
} if (rf & 2) {
    const segment_r41 = ctx.$implicit;
    const i_r42 = ctx.index;
    const last_r43 = ctx.last;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("current", last_r43);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", last_r43);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", segment_r41.icon && i_r42 === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](segment_r41.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !last_r43);
} }
function ShowFileSystemComponent_div_41_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "Loading...");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_div_42_div_1_Template(rf, ctx) { if (rf & 1) {
    const _r53 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_42_div_1_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r52 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r52.onItemClick(item_r51); })("dblclick", function ShowFileSystemComponent_div_42_div_1_Template_div_dblclick_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r54.onItemDoubleClick(item_r51); })("mouseenter", function ShowFileSystemComponent_div_42_div_1_Template_div_mouseenter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r55 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r55.hoveredNode = item_r51; })("mouseleave", function ShowFileSystemComponent_div_42_div_1_Template_div_mouseleave_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const ctx_r56 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r56.hoveredNode = null; })("keydown.enter", function ShowFileSystemComponent_div_42_div_1_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r57 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r57.onItemDoubleClick(item_r51); })("keydown.space", function ShowFileSystemComponent_div_42_div_1_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r58 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r58.onItemClick(item_r51); })("contextmenu", function ShowFileSystemComponent_div_42_div_1_Template_div_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53); const item_r51 = ctx.$implicit; const ctx_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r59.onRightClickImproved($event, item_r51); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const item_r51 = ctx.$implicit;
    const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("selected", ctx_r49.activeNode === item_r51)("context-menu-active", ctx_r49.contextMenuNode === item_r51)("hovered", ctx_r49.hoveredNode === item_r51)("not-selectable", !ctx_r49.isItemSelectable(item_r51));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("aria-label", ctx_r49.getItemAriaLabel(item_r51));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", item_r51.type === "folder" ? "folder" : item_r51.type === "file" ? "insert_drive_file" : "text_snippet", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](item_r51.name);
} }
function ShowFileSystemComponent_div_42_div_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r50 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r50.searchFilter ? "search_off" : "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r50.searchFilter ? "No files match your filter" : "This folder is empty");
} }
function ShowFileSystemComponent_div_42_Template(rf, ctx) { if (rf & 1) {
    const _r61 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("contextmenu", function ShowFileSystemComponent_div_42_Template_div_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r61); const ctx_r60 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r60.onRightClickImproved($event, null); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, ShowFileSystemComponent_div_42_div_1_Template, 5, 11, "div", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, ShowFileSystemComponent_div_42_div_2_Template, 5, 2, "div", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r12.filteredItems)("ngForTrackBy", ctx_r12.trackByItem);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r12.filteredItems.length === 0 && !ctx_r12.isLoading);
} }
function ShowFileSystemComponent_mat_card_title_45_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r13.folder.name || (ctx_r13.currentPath ? "Current folder" : "No folder selected"), " ");
} }
function ShowFileSystemComponent_mat_card_title_46_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r14.folder.name || "No file selected", " ");
} }
// IFileInfoNode è interfaccia
// MdFile è la classe -> DynamicFlatNode
/**
 * Database for dynamic data. When expanding a node in the tree, the data source will need to fetch
 * the descendants data from the database.
 */
class DynamicDatabase {
    constructor(mdFileService) {
        this.mdFileService = mdFileService;
        this.dataMap = new Map();
        this.mdFileService.loadDynFolders('root', 1);
        var md1 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]('12Folder', 'c:primoFolder', 0, true);
        var md2 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]('2Folder', 'c:primoFoldersottoFolder', 1, true);
        var md3 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md4 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md5 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]('5Folder', 'c:cuccu', 3, false);
        this.dataMap.set(md1, [md2]);
        this.dataMap.set(md2, [md3, md4]);
        //this.dataMap.set(md3, [md4]);
        this.dataMap.set(md4, [md5]);
        var test = this.dataMap.get(md1);
        this.rootLevelNodes = [md1];
    }
    /** Initial data from database */
    initialData() {
        return this.rootLevelNodes;
    }
    getChildren(node) {
        var test = this.dataMap.get(node);
        return test;
    }
    isExpandable(node) {
        return this.dataMap.has(node);
    }
}
DynamicDatabase.ɵfac = function DynamicDatabase_Factory(t) { return new (t || DynamicDatabase)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__["MdFileService"])); };
DynamicDatabase.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineInjectable"]({ token: DynamicDatabase, factory: DynamicDatabase.ɵfac, providedIn: 'root' });
class DynamicDataSource {
    constructor(_treeControl, _database, _mdFileService, baseStart, typeOfSelection) {
        this._treeControl = _treeControl;
        this._database = _database;
        this._mdFileService = _mdFileService;
        this.baseStart = baseStart;
        this.typeOfSelection = typeOfSelection;
        this.dataChange = new rxjs__WEBPACK_IMPORTED_MODULE_4__["BehaviorSubject"]([]);
        this.data = _database.initialData();
        console.log("constructor-> this.typeOfSelection " + this.typeOfSelection);
        this._mdFileService.loadDocumentFolder(baseStart, 0, this.typeOfSelection).subscribe(_ => {
            this.data = _;
        });
    }
    get data() { return this.dataChange.value; }
    set data(value) {
        this._treeControl.dataNodes = value;
        this.dataChange.next(value);
    }
    connect(collectionViewer) {
        this._treeControl.expansionModel.changed.subscribe(change => {
            if (change.added ||
                change.removed) {
                this.handleTreeControl(change);
            }
        });
        return Object(rxjs__WEBPACK_IMPORTED_MODULE_4__["merge"])(collectionViewer.viewChange, this.dataChange).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_5__["map"])(() => this.data));
    }
    disconnect(collectionViewer) { }
    /** Handle expand/collapse behaviors */
    handleTreeControl(change) {
        if (change.added) {
            change.added.forEach(node => this.toggleNode(node, true));
        }
        if (change.removed) {
            change.removed.slice().reverse().forEach(node => this.toggleNode(node, false));
        }
    }
    /**
     * Toggle the node, remove from display list
     */
    toggleNode(node, expand) {
        this._mdFileService.loadDocumentFolder(node.path, node.level + 1, this.typeOfSelection).subscribe(_ => {
            const children = _;
            const index = this.data.indexOf(node);
            if (!children || index < 0) { // If no children, or cannot find the node, no op
                return;
            }
            node.isLoading = true;
            setTimeout(() => {
                if (expand) {
                    const nodes = children; // punto per fare chiamata remota
                    this.data.splice(index + 1, 0, ...nodes);
                }
                else {
                    let count = 0;
                    for (let i = index + 1; i < this.data.length
                        && this.data[i].level > node.level; i++, count++) { }
                    this.data.splice(index + 1, count);
                }
                // notify the change
                this.dataChange.next(this.data);
                node.isLoading = false;
            });
        });
    }
    refreshNode(node) {
        this._mdFileService.loadDocumentFolder(node.path, node.level + 1, this.typeOfSelection).subscribe(children => {
            const index = this.data.indexOf(node);
            let count = 0;
            for (let i = index + 1; i < this.data.length
                && this.data[i].level > node.level; i++, count++) { }
            this.data.splice(index + 1, count); // toglie i nodi figlio
            const nodes = children;
            this.data.splice(index + 1, 0, ...nodes); // mette i nuovi nodi
            this.dataChange.next(this.data);
        });
    }
}
class ShowFileSystemComponent {
    constructor(baseStart, database, dialog, mdFileService, dialogRef, snackBar) {
        this.baseStart = baseStart;
        this.database = database;
        this.dialog = dialog;
        this.mdFileService = mdFileService;
        this.dialogRef = dialogRef;
        this.snackBar = snackBar;
        this.title = "Document's Folder";
        // Existing properties for context menu
        this.menuTopLeftPosition = { x: 0, y: 0 };
        // New properties for file explorer
        this.specialFolders = [];
        this.drives = [];
        this.networkShares = [];
        this.currentPath = '';
        this.displayPath = '';
        this.currentItems = [];
        this.filteredItems = [];
        this.searchFilter = '';
        this.navigationHistory = [];
        this.isLoading = false;
        // Performance optimization: caching
        this.folderCache = new Map();
        this.CACHE_DURATION = 30000; // 30 seconds
        // NEW: Breadcrumb navigation
        this.pathSegments = [];
        // NEW: Search filter tracking
        this.filterAppliedToPath = '';
        // NEW: Context menu and hover tracking
        this.hoveredNode = null;
        this.contextMenuNode = null;
        // Legacy properties (manteniamo per compatibilità)
        this.getLevel = (node) => node.level;
        this.isExpandable = (node) => node.expandable;
        this.hasChild = (_, _nodeData) => _nodeData.expandable;
        // Inizializza legacy tree control per compatibilità
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_1__["FlatTreeControl"](this.getLevel, this.isExpandable);
        let start = this.baseStart.start == null ? 'root' : this.baseStart.start;
        this.title = this.baseStart.title;
        this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService, start, baseStart.typeOfSelection);
    }
    createDirectoryOn(node) {
        if (node == null) {
            node = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        let dialogRef = this.dialog.open(_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_6__["NewDirectoryComponent"], {
            width: '300px',
            data: node,
        });
        dialogRef.afterClosed().subscribe(_ => {
            this.dataSource.refreshNode(node);
        });
    }
    onRightClick(event, item) {
        // preventDefault avoids to show the visualization of the right-click menu of the browser
        event.preventDefault();
        if (item == null) {
            item = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]("root", "root", 0, false);
            item.fullPath = "root";
        }
        // we record the mouse position in our object
        this.menuTopLeftPosition.x = event.clientX;
        this.menuTopLeftPosition.y = event.clientY;
        // we open the menu
        // we pass to the menu the information about our object
        this.matMenuTrigger.menuData = { item: item };
        // we open the menu
        this.matMenuTrigger.openMenu();
    }
    ngOnInit() {
        this.folder = { name: "<select project>", path: "" };
        this.filteredItems = [];
        this.loadInitialData();
    }
    loadInitialData() {
        this.isLoading = true;
        // Carica special folders, drives e network shares
        Object(rxjs__WEBPACK_IMPORTED_MODULE_4__["forkJoin"])({
            folders: this.mdFileService.getSpecialFolders(),
            drives: this.mdFileService.getDrives(),
            networkShares: this.mdFileService.getNetworkShares()
        }).subscribe({
            next: ({ folders, drives, networkShares }) => {
                var _a;
                this.specialFolders = folders;
                this.drives = drives;
                this.networkShares = networkShares;
                // Naviga alla cartella iniziale
                const initialPath = this.baseStart.start === 'root' ? 'project' : this.baseStart.start;
                const initialFolder = this.specialFolders.find(f => f.name.toLowerCase() === (initialPath === null || initialPath === void 0 ? void 0 : initialPath.toLowerCase()));
                if (initialFolder) {
                    this.navigateToFolder(initialFolder.path);
                }
                else {
                    this.navigateToFolder(((_a = this.specialFolders[0]) === null || _a === void 0 ? void 0 : _a.path) || '');
                }
            },
            error: (error) => {
                console.error('Error loading initial data:', error);
                this.isLoading = false;
            }
        });
    }
    navigateToFolder(path) {
        if (!path || path === this.currentPath)
            return;
        // NEW: Reset automatico del filtro quando si naviga
        if (this.searchFilter && this.filterAppliedToPath !== path) {
            this.searchFilter = '';
            this.filterAppliedToPath = '';
        }
        // Aggiungi il path corrente alla history
        if (this.currentPath) {
            this.navigationHistory.push(this.currentPath);
        }
        this.currentPath = path;
        this.displayPath = this.formatDisplayPath(path);
        this.buildBreadcrumb(path); // NEW: Costruisci breadcrumb
        this.loadFolderContent(path);
    }
    navigateUp() {
        if (this.navigationHistory.length > 0) {
            const previousPath = this.navigationHistory.pop();
            this.currentPath = previousPath;
            this.displayPath = this.formatDisplayPath(previousPath);
            this.buildBreadcrumb(previousPath); // FIX: Aggiorna breadcrumb
            this.loadFolderContent(previousPath);
        }
    }
    loadFolderContent(path) {
        // Check cache first
        const cached = this.folderCache.get(path);
        const now = Date.now();
        if (cached && (now - cached.timestamp) < this.CACHE_DURATION) {
            this.currentItems = cached.data;
            this.applyFilter(); // Apply current filter to cached data
            this.isLoading = false;
            return;
        }
        this.isLoading = true;
        this.currentItems = [];
        this.mdFileService.loadDocumentFolder(path, 0, this.baseStart.typeOfSelection)
            .subscribe({
            next: (items) => {
                const data = items || [];
                this.currentItems = data;
                this.applyFilter(); // Apply current filter to new data
                // Cache the result
                this.folderCache.set(path, { data, timestamp: now });
                // Clean old cache entries (keep cache size manageable)
                this.cleanOldCacheEntries();
                this.isLoading = false;
            },
            error: (error) => {
                console.error('Error loading folder content:', error);
                this.currentItems = [];
                this.filteredItems = [];
                this.isLoading = false;
            }
        });
    }
    cleanOldCacheEntries() {
        const now = Date.now();
        for (const [key, value] of this.folderCache.entries()) {
            if ((now - value.timestamp) > this.CACHE_DURATION) {
                this.folderCache.delete(key);
            }
        }
        // Limit cache size to prevent memory issues
        if (this.folderCache.size > 50) {
            const entries = Array.from(this.folderCache.entries());
            entries.sort((a, b) => a[1].timestamp - b[1].timestamp);
            // Keep only the 30 most recent entries
            for (let i = 0; i < entries.length - 30; i++) {
                this.folderCache.delete(entries[i][0]);
            }
        }
    }
    formatDisplayPath(path) {
        // Accorcia il path per la visualizzazione
        if (path.length > 50) {
            return '...' + path.substring(path.length - 47);
        }
        return path;
    }
    onItemClick(item) {
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles' && item.type === 'folder') {
            // Per selezione file, le cartelle servono solo per navigare
            // Non selezionare la cartella
            return;
        }
        this.activeNode = item;
        if (item.type === 'folder') {
            // Per le cartelle, seleziona ma non naviga (single click)
            this.getFolder(item);
        }
        else {
            // Per i file, seleziona direttamente
            this.getFolder(item);
        }
    }
    onItemDoubleClick(item) {
        if (item.type === 'folder') {
            // Double click su cartella: naviga
            this.navigateToFolder(item.fullPath || item.path);
        }
    }
    getFolder(node) {
        this.folder.name = node.name;
        this.folder.path = node.fullPath || node.path;
    }
    // Legacy method mantained for compatibility
    openFolderOn(item) {
        if (item && item.fullPath) {
            this.mdFileService.openFolderOnFileExplorer(item).subscribe({
                next: () => console.log('Folder opened in explorer'),
                error: (error) => console.error('Error opening folder:', error)
            });
        }
    }
    closeDialog() {
        // Determina quale path usare in base al tipo di selezione
        let selectedPath;
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
            // Per file: usa sempre folder.path (che viene aggiornato quando si seleziona un file)
            selectedPath = this.folder.path;
        }
        else {
            // Per cartelle: priorità a folder.path (cartella selezionata), 
            // altrimenti usa currentPath (cartella in cui stiamo navigando)
            selectedPath = this.folder.path || this.currentPath;
        }
        this.dialogRef.close({ event: 'open', data: selectedPath });
    }
    // TrackBy functions for performance optimization
    trackByPath(index, item) {
        return item.path;
    }
    trackByItem(index, item) {
        return item.fullPath || item.path || item.name;
    }
    // Filter functionality
    onFilterChange(event) {
        this.searchFilter = event.target.value;
        // NEW: Traccia il path dove è stato applicato il filtro
        if (this.searchFilter && this.searchFilter.trim() !== '') {
            this.filterAppliedToPath = this.currentPath;
        }
        else {
            this.filterAppliedToPath = '';
        }
        this.applyFilter();
    }
    applyFilter() {
        let filtered = [...this.currentItems];
        // Filtro per nome
        if (this.searchFilter && this.searchFilter.trim() !== '') {
            const filter = this.searchFilter.toLowerCase().trim();
            filtered = filtered.filter(item => item.name.toLowerCase().includes(filter));
        }
        // FILTRO PER ESTENSIONI RIMOSSO - mostra tutti i file
        // Commentato per permettere la visualizzazione di tutti i file disponibili
        // Se necessario in futuro, l'utente può aggiungere una configurazione
        this.filteredItems = filtered;
    }
    getFileExtension(filename) {
        const lastDot = filename.lastIndexOf('.');
        return lastDot > 0 ? filename.substring(lastDot) : '';
    }
    // Selection button text
    getSelectionButtonText() {
        // Prima controlla se c'è un testo personalizzato
        if (this.baseStart.buttonText) {
            return this.baseStart.buttonText;
        }
        // Altrimenti usa il default basato sul tipo
        return this.baseStart.typeOfSelection === 'FoldersAndFiles'
            ? 'Select file'
            : 'Select folder';
    }
    // Validation for selection
    canSelectItem() {
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
            // Solo file possono essere selezionati
            return this.activeNode && this.activeNode.type !== 'folder' && !!this.folder.path;
        }
        // Per selezione cartelle: può selezionare la cartella corrente o una cartella selezionata
        // Se c'è una cartella selezionata (activeNode), usa quella
        // Altrimenti usa la cartella corrente in cui si sta navigando
        if (this.activeNode && this.activeNode.type === 'folder') {
            return true;
        }
        // Se non c'è activeNode ma stiamo navigando in una cartella, possiamo selezionare la cartella corrente
        return !!(this.currentPath && this.currentPath.length > 0);
    }
    // Check if item is selectable
    isItemSelectable(item) {
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
            return item.type !== 'folder';
        }
        return item.type === 'folder';
    }
    // Accessibility helper
    getItemAriaLabel(item) {
        const type = item.type === 'folder' ? 'folder' : 'file';
        return `${type} ${item.name}. ${item.type === 'folder' ? 'Double click to open' : 'Click to select'}`;
    }
    // ============================================
    // NEW METHODS FOR UX IMPROVEMENTS
    // ============================================
    /**
     * Costruisce il breadcrumb path cliccabile
     * Cross-platform: gestisce sia / che \ come separatori
     */
    buildBreadcrumb(path) {
        this.pathSegments = [];
        if (!path)
            return;
        // Normalizza i separatori per la gestione cross-platform
        const normalizedPath = path.replace(/\\/g, '/');
        // Trova special folder come primo elemento
        const specialFolder = this.specialFolders.find(f => {
            const normalizedFolderPath = f.path.replace(/\\/g, '/');
            return normalizedPath.startsWith(normalizedFolderPath) || normalizedPath === normalizedFolderPath;
        });
        if (specialFolder) {
            this.pathSegments.push({
                name: specialFolder.name,
                fullPath: specialFolder.path,
                icon: specialFolder.icon
            });
            // Aggiungi sottocartelle relative
            const normalizedSpecialPath = specialFolder.path.replace(/\\/g, '/');
            let relativePath = normalizedPath.substring(normalizedSpecialPath.length);
            // Rimuovi il separatore iniziale se presente
            if (relativePath.startsWith('/')) {
                relativePath = relativePath.substring(1);
            }
            if (relativePath) {
                const parts = relativePath.split('/').filter(p => p);
                let currentPath = specialFolder.path;
                parts.forEach(part => {
                    // Usa il separatore del sistema operativo originale
                    const separator = specialFolder.path.includes('\\') ? '\\' : '/';
                    currentPath = `${currentPath}${separator}${part}`;
                    this.pathSegments.push({
                        name: part,
                        fullPath: currentPath
                    });
                });
            }
        }
        else {
            // Fallback per path normali (senza special folder)
            const parts = normalizedPath.split('/').filter(p => p);
            let currentPath = '';
            parts.forEach((part, index) => {
                if (index === 0) {
                    // Prima parte (es: C:, D:, /home, etc.)
                    currentPath = part;
                    // Ripristina \ se era nel path originale
                    if (path.includes('\\')) {
                        currentPath = part;
                    }
                }
                else {
                    // Usa il separatore appropriato
                    const separator = path.includes('\\') ? '\\' : '/';
                    currentPath = `${currentPath}${separator}${part}`;
                }
                this.pathSegments.push({
                    name: part,
                    fullPath: currentPath
                });
            });
        }
    }
    /**
     * Naviga a un segmento specifico del breadcrumb
     */
    navigateToBreadcrumb(segment) {
        this.navigateToFolder(segment.fullPath);
    }
    /**
     * Copia il path corrente negli appunti
     */
    copyPathToClipboard(path) {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            const pathToCopy = path || this.currentPath;
            try {
                if (navigator.clipboard && window.isSecureContext) {
                    yield navigator.clipboard.writeText(pathToCopy);
                    this.showSuccessNotification('Path copied to clipboard');
                }
                else {
                    // Fallback per ambienti non sicuri
                    this.fallbackCopyToClipboard(pathToCopy);
                    this.showSuccessNotification('Path copied to clipboard');
                }
            }
            catch (error) {
                console.error('Failed to copy path:', error);
                this.snackBar.open('Failed to copy path', 'Close', { duration: 3000 });
            }
        });
    }
    /**
     * Fallback per copia negli appunti (cross-browser compatibility)
     */
    fallbackCopyToClipboard(text) {
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        try {
            document.execCommand('copy');
        }
        catch (error) {
            console.error('Fallback copy failed:', error);
        }
        document.body.removeChild(textArea);
    }
    /**
     * Getter per verificare se il filtro è attivo
     */
    get isFilterActive() {
        return !!(this.searchFilter && this.searchFilter.trim() !== '');
    }
    /**
     * Pulisce il filtro di ricerca
     */
    clearFilter() {
        this.searchFilter = '';
        this.filterAppliedToPath = '';
        this.applyFilter();
    }
    /**
     * Aggiorna createDirectoryOn con contesto migliorato
     */
    createDirectoryOnImproved(node) {
        if (node == null) {
            node = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]("root", "root", 0, false);
            node.fullPath = this.currentPath || "root";
        }
        // Prepara i dati con contesto completo
        const dialogData = {
            parentNode: node,
            parentPath: node.fullPath || node.path,
            parentName: node.name,
            isRoot: node.name === "root",
            currentPath: this.currentPath
        };
        const dialogRef = this.dialog.open(_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_6__["NewDirectoryComponent"], {
            width: '500px',
            data: dialogData,
            disableClose: false,
            autoFocus: true
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                // Refresh node nel tree view legacy
                this.dataSource.refreshNode(node);
                // Refresh anche la vista corrente
                this.refreshCurrentFolder();
                // Mostra notifica di successo
                this.showSuccessNotification(`Folder "${result.name}" created successfully`);
            }
        });
    }
    /**
     * Cleanup quando il context menu si chiude
     */
    onMenuClosed() {
        setTimeout(() => {
            this.contextMenuNode = null;
        }, 200);
    }
    /**
     * Aggiorna l'onRightClick con tracking del context menu
     */
    onRightClickImproved(event, item) {
        event.preventDefault();
        // Normalizza item per root
        if (item == null) {
            item = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_7__["MdFile"]("root", "root", 0, false);
            item.fullPath = this.currentPath || "root";
        }
        this.contextMenuNode = item;
        // Posiziona menu
        this.menuTopLeftPosition.x = event.clientX;
        this.menuTopLeftPosition.y = event.clientY;
        this.matMenuTrigger.menuData = { item: item };
        this.matMenuTrigger.openMenu();
    }
    /**
     * Refresh della cartella corrente (invalida cache)
     */
    refreshCurrentFolder() {
        this.folderCache.delete(this.currentPath);
        this.loadFolderContent(this.currentPath);
    }
    /**
     * Refresh di una cartella specifica
     */
    refreshFolder(item) {
        const pathToRefresh = item.fullPath || item.path;
        this.folderCache.delete(pathToRefresh);
        if (pathToRefresh === this.currentPath) {
            this.loadFolderContent(pathToRefresh);
        }
    }
    /**
     * Mostra notifica di successo
     */
    showSuccessNotification(message) {
        this.snackBar.open(message, 'Close', {
            duration: 3000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            panelClass: ['success-snackbar']
        });
    }
    /**
     * Focus sull'input del filtro
     */
    focusFilter() {
        if (this.filterInput && this.filterInput.nativeElement) {
            this.filterInput.nativeElement.focus();
        }
    }
    /**
     * Keyboard shortcuts handler
     * Cross-platform: usa Ctrl su Windows/Linux, Cmd su Mac
     */
    handleKeyboardEvent(event) {
        // Ctrl/Cmd + F: Focus sul filtro
        if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
            event.preventDefault();
            this.focusFilter();
        }
        // Ctrl/Cmd + N: Nuova cartella nella cartella corrente
        if ((event.ctrlKey || event.metaKey) && event.key === 'n') {
            event.preventDefault();
            this.createDirectoryOnImproved(null);
        }
        // Escape: Clear filter
        if (event.key === 'Escape' && this.isFilterActive) {
            event.preventDefault();
            this.clearFilter();
        }
        // Alt + Up: Navigate up
        if (event.altKey && event.key === 'ArrowUp') {
            event.preventDefault();
            this.navigateUp();
        }
        // F5: Refresh
        if (event.key === 'F5') {
            event.preventDefault();
            this.refreshCurrentFolder();
        }
    }
}
ShowFileSystemComponent.ɵfac = function ShowFileSystemComponent_Factory(t) { return new (t || ShowFileSystemComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](DynamicDatabase), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_10__["MatSnackBar"])); };
ShowFileSystemComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({ type: ShowFileSystemComponent, selectors: [["app-show-file-system"]], viewQuery: function ShowFileSystemComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuTrigger"], 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵloadQuery"]()) && (ctx.filterInput = _t.first);
    } }, hostBindings: function ShowFileSystemComponent_HostBindings(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("keydown", function ShowFileSystemComponent_keydown_HostBindingHandler($event) { return ctx.handleKeyboardEvent($event); }, false, _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresolveDocument"]);
    } }, decls: 52, vars: 25, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor", "menuClosed"], [1, "context-menu"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [1, "file-explorer-container"], [1, "explorer-header"], [1, "explorer-title"], [1, "filter-section"], ["appearance", "outline", 1, "filter-input"], ["matInput", "", "placeholder", "Type to filter...", 3, "ngModel", "ngModelChange", "input"], ["filterInput", ""], ["matPrefix", "", "class", "filter-badge", 4, "ngIf"], ["matSuffix", "", 4, "ngIf"], ["mat-icon-button", "", "matSuffix", "", "matTooltip", "Clear filter", "class", "clear-filter-button", 3, "click", 4, "ngIf"], ["class", "filter-chip-list", 4, "ngIf"], [1, "explorer-content"], [1, "left-panel"], [1, "quick-access-section"], [1, "section-title"], ["role", "navigation", "aria-label", "Quick access folders", 1, "nav-items"], ["class", "nav-item", "tabindex", "0", "role", "button", 3, "active", "click", "keydown.enter", "keydown.space", 4, "ngFor", "ngForOf", "ngForTrackBy"], [1, "drives-section"], ["role", "navigation", "aria-label", "Available drives", 1, "nav-items"], ["class", "network-section", 4, "ngIf"], [1, "right-panel"], [1, "breadcrumb"], ["mat-icon-button", "", "matTooltip", "Go back", 1, "breadcrumb-back", 3, "disabled", "click"], [1, "breadcrumb-path"], [4, "ngFor", "ngForOf"], ["mat-icon-button", "", "matTooltip", "New folder (Ctrl+N)", "color", "primary", 1, "breadcrumb-action", 3, "click"], ["class", "loading-indicator", 4, "ngIf"], ["class", "content-list", "role", "grid", "aria-label", "Folder contents", 3, "contextmenu", 4, "ngIf"], [1, "bottom-panel"], [1, "selected-folder-info"], [4, "ngIf"], [1, "action-buttons"], ["mat-stroked-button", "", "color", "primary", 3, "disabled", "click"], [1, "context-menu-header"], [1, "context-menu-icon"], [1, "context-menu-info"], [1, "context-menu-name"], [1, "context-menu-path"], ["mat-menu-item", "", 3, "click"], ["color", "primary"], [1, "menu-hint"], ["matPrefix", "", 1, "filter-badge"], ["matSuffix", ""], ["mat-icon-button", "", "matSuffix", "", "matTooltip", "Clear filter", 1, "clear-filter-button", 3, "click"], [1, "filter-chip-list"], ["color", "primary", "selected", "", "removable", "", 3, "removed"], ["matChipAvatar", ""], ["matChipRemove", ""], ["tabindex", "0", "role", "button", 1, "nav-item", 3, "click", "keydown.enter", "keydown.space"], ["aria-hidden", "true", 1, "nav-icon"], [1, "nav-label"], [1, "network-section"], ["role", "navigation", "aria-label", "Network shares", 1, "nav-items"], ["mat-button", "", 1, "breadcrumb-segment", 3, "disabled", "click"], ["class", "breadcrumb-icon", 4, "ngIf"], ["class", "breadcrumb-separator", 4, "ngIf"], [1, "breadcrumb-icon"], [1, "breadcrumb-separator"], [1, "loading-indicator"], ["diameter", "30"], ["role", "grid", "aria-label", "Folder contents", 1, "content-list", 3, "contextmenu"], ["class", "content-item", "tabindex", "0", "role", "gridcell", 3, "selected", "context-menu-active", "hovered", "not-selectable", "click", "dblclick", "mouseenter", "mouseleave", "keydown.enter", "keydown.space", "contextmenu", 4, "ngFor", "ngForOf", "ngForTrackBy"], ["class", "empty-state", 4, "ngIf"], ["tabindex", "0", "role", "gridcell", 1, "content-item", 3, "click", "dblclick", "mouseenter", "mouseleave", "keydown.enter", "keydown.space", "contextmenu"], ["aria-hidden", "true", 1, "item-icon"], [1, "item-name"], [1, "empty-state"]], template: function ShowFileSystemComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("menuClosed", function ShowFileSystemComponent_Template_div_menuClosed_0_listener() { return ctx.onMenuClosed(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-menu", 1, 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](3, ShowFileSystemComponent_ng_template_3_Template, 32, 4, "ng-template", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "h1", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "mat-form-field", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11, "Filter files");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](12, "input", 9, 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ShowFileSystemComponent_Template_input_ngModelChange_12_listener($event) { return ctx.searchFilter = $event; })("input", function ShowFileSystemComponent_Template_input_input_12_listener($event) { return ctx.onFilterChange($event); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](14, ShowFileSystemComponent_span_14_Template, 2, 1, "span", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](15, ShowFileSystemComponent_mat_icon_15_Template, 2, 0, "mat-icon", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](16, ShowFileSystemComponent_button_16_Template, 3, 0, "button", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](17, ShowFileSystemComponent_mat_chip_list_17_Template, 7, 1, "mat-chip-list", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](18, "div", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](19, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](20, "div", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](21, "h3", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](22, "Quick Access");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](23, "div", 19);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](24, ShowFileSystemComponent_div_24_Template, 5, 5, "div", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](25, "div", 21);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](26, "h3", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](27, "This PC");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](28, "div", 22);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](29, ShowFileSystemComponent_div_29_Template, 5, 6, "div", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](30, ShowFileSystemComponent_div_30_Template, 5, 2, "div", 23);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](31, "div", 24);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](32, "div", 25);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](33, "button", 26);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_33_listener() { return ctx.navigateUp(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](34, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](35, "arrow_back");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](36, "div", 27);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](37, ShowFileSystemComponent_ng_container_37_Template, 6, 6, "ng-container", 28);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](38, "button", 29);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_38_listener() { return ctx.createDirectoryOnImproved(null); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](39, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](40, "create_new_folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](41, ShowFileSystemComponent_div_41_Template, 4, 0, "div", 30);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](42, ShowFileSystemComponent_div_42_Template, 3, 3, "div", 31);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](43, "div", 32);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](44, "div", 33);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](45, ShowFileSystemComponent_mat_card_title_45_Template, 2, 1, "mat-card-title", 34);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](46, ShowFileSystemComponent_mat_card_title_46_Template, 2, 1, "mat-card-title", 34);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](47, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](48);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](49, "div", 35);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](50, "button", 36);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_50_listener() { return ctx.closeDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](51);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx.title);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.searchFilter);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isFilterActive);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.isFilterActive);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isFilterActive);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isFilterActive);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx.specialFolders)("ngForTrackBy", ctx.trackByPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx.drives)("ngForTrackBy", ctx.trackByPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.networkShares && ctx.networkShares.length > 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx.navigationHistory.length === 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx.pathSegments);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.baseStart.typeOfSelection === "Folders");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.baseStart.typeOfSelection !== "Folders");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx.folder.path || ctx.currentPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", !ctx.canSelectItem());
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx.getSelectionButtonText(), " ");
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_13__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgModel"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgIf"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgForOf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_16__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_17__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__["MatIcon"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCardSubtitle"], _angular_material_divider__WEBPACK_IMPORTED_MODULE_20__["MatDivider"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuItem"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatPrefix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatSuffix"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_21__["MatChipList"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_21__["MatChip"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_21__["MatChipAvatar"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_21__["MatChipRemove"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_22__["MatSpinner"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCardTitle"]], styles: [".file-explorer-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  width: 100%;\n  background-color: #ffffff;\n  overflow: hidden;\n  position: relative;\n}\n\n.explorer-header[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #47b784, #2e7d32);\n  color: white;\n  padding: 16px 20px;\n  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  gap: 16px;\n}\n\n.explorer-title[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.2rem;\n  font-weight: 500;\n  flex: 1;\n}\n\n.filter-section[_ngcontent-%COMP%] {\n  min-width: 250px;\n}\n\n.filter-input[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-outline {\n  color: rgba(255, 255, 255, 0.3) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-outline-thick {\n  color: white !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-label {\n  color: rgba(255, 255, 255, 0.8) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-input-element {\n  color: white !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-input-element::placeholder {\n  color: rgba(255, 255, 255, 0.6) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-icon {\n  color: rgba(255, 255, 255, 0.8) !important;\n}\n\n.explorer-content[_ngcontent-%COMP%] {\n  display: flex;\n  flex: 1;\n  min-height: 0;\n}\n\n.left-panel[_ngcontent-%COMP%] {\n  width: 280px;\n  background: #f8f9fa;\n  display: flex;\n  flex-direction: column;\n  overflow-y: auto;\n}\n\n.section-title[_ngcontent-%COMP%] {\n  font-size: 0.9rem;\n  font-weight: 600;\n  color: #666;\n  margin: 16px 16px 8px 16px;\n  text-transform: uppercase;\n  letter-spacing: 0.5px;\n}\n\n.nav-items[_ngcontent-%COMP%] {\n  padding: 0 8px;\n}\n\n.nav-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 10px 12px;\n  margin: 2px 0;\n  cursor: pointer;\n  border-radius: 6px;\n  transition: all 0.2s ease;\n  -webkit-user-select: none;\n          user-select: none;\n}\n\n.nav-item[_ngcontent-%COMP%]:hover {\n  background: #e3f2fd;\n  transform: translateX(2px);\n}\n\n.nav-item[_ngcontent-%COMP%]:focus {\n  outline: 2px solid #2196f3;\n  outline-offset: 2px;\n  background: #e3f2fd;\n}\n\n.nav-item.active[_ngcontent-%COMP%] {\n  background: #2196f3;\n  color: white;\n  box-shadow: 0 2px 4px rgba(33, 150, 243, 0.3);\n}\n\n.nav-item.active[_ngcontent-%COMP%]   .nav-icon[_ngcontent-%COMP%] {\n  color: white;\n}\n\n.nav-item.active[_ngcontent-%COMP%]:focus {\n  outline-color: white;\n}\n\n.nav-icon[_ngcontent-%COMP%] {\n  margin-right: 12px;\n  font-size: 20px;\n  color: #666;\n  transition: color 0.2s ease;\n}\n\n.nav-label[_ngcontent-%COMP%] {\n  font-size: 0.95rem;\n  font-weight: 500;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.right-panel[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 0;\n}\n\n.breadcrumb[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  background: #fafafa;\n  border-bottom: 1px solid #e0e0e0;\n  min-height: 48px;\n}\n\n.breadcrumb[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-right: 12px;\n}\n\n.current-path[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 0.9rem;\n  color: #555;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  flex: 1;\n}\n\n.loading-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  color: #666;\n}\n\n.loading-indicator[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  margin-top: 12px;\n  font-size: 0.9rem;\n}\n\n.content-list[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 8px;\n}\n\n.content-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 10px 12px;\n  margin: 2px 0;\n  cursor: pointer;\n  border-radius: 6px;\n  transition: all 0.2s ease;\n  -webkit-user-select: none;\n          user-select: none;\n}\n\n.content-item[_ngcontent-%COMP%]:hover {\n  background: #f0f0f0;\n}\n\n.content-item[_ngcontent-%COMP%]:focus {\n  outline: 2px solid #2196f3;\n  outline-offset: 2px;\n  background: #f0f0f0;\n}\n\n.content-item.selected[_ngcontent-%COMP%] {\n  background: #e3f2fd;\n  border: 1px solid #2196f3;\n}\n\n.content-item.selected[_ngcontent-%COMP%]:focus {\n  outline-color: #1976d2;\n}\n\n.content-item[_ngcontent-%COMP%]:active {\n  transform: scale(0.98);\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%] {\n  opacity: 0.6;\n  cursor: default;\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%]:hover {\n  background: transparent;\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%]:active {\n  transform: none;\n}\n\n.item-icon[_ngcontent-%COMP%] {\n  margin-right: 12px;\n  font-size: 20px;\n  color: #666;\n}\n\n.content-item[_ngcontent-%COMP%]:hover   .item-icon[_ngcontent-%COMP%] {\n  color: #2196f3;\n}\n\n.item-name[_ngcontent-%COMP%] {\n  font-size: 0.95rem;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  flex: 1;\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 20px;\n  color: #999;\n  text-align: center;\n}\n\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  margin-bottom: 16px;\n  opacity: 0.5;\n}\n\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 0.9rem;\n  margin: 0;\n}\n\n.bottom-panel[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  padding: 16px 20px;\n  background: #f8f9fa;\n  border-top: 1px solid #e0e0e0;\n  min-height: 80px;\n}\n\n.selected-folder-info[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.selected-folder-info[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin-bottom: 4px;\n  color: #333;\n}\n\n.selected-folder-info[_ngcontent-%COMP%]   mat-card-subtitle[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n  color: #666;\n  font-family: \"Courier New\", monospace;\n}\n\n.action-buttons[_ngcontent-%COMP%] {\n  margin-left: 20px;\n}\n\n.action-buttons[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  min-width: 140px;\n  font-weight: 500;\n}\n\n.flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.flex-items-overflow[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-x: scroll;\n  overflow-y: scroll;\n  background: tomato;\n  color: white;\n  text-align: center;\n  font-size: 3em;\n  flex: 1;\n}\n\n.flex-items[_ngcontent-%COMP%] {\n  width: 500px;\n  text-align: center;\n  font-size: 3em;\n}\n\n.flex-selected-folder[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n\n  .resizable-dialog-container {\n  resize: both;\n  overflow: auto;\n  min-width: 400px;\n  min-height: 300px;\n  max-width: 90vw;\n  max-height: 90vh;\n}\n\n  .resizable-dialog-container .mat-dialog-container {\n  height: 100%;\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n}\n\n  .resizable-dialog-container .mat-dialog-content {\n  flex: 1;\n  max-height: unset;\n  padding: 0;\n  margin: 0;\n  overflow: hidden;\n}\n\n  .resizable-dialog-container::after {\n  content: \"\";\n  position: absolute;\n  bottom: 0;\n  right: 0;\n  width: 15px;\n  height: 15px;\n  cursor: nwse-resize;\n  background: linear-gradient(135deg, transparent 50%, #47b784 50%), linear-gradient(135deg, transparent 50%, #47b784 50%);\n  background-size: 5px 5px, 5px 5px;\n  background-position: 100% 0, 0 100%;\n  background-repeat: no-repeat;\n  opacity: 0.5;\n  transition: opacity 0.2s ease;\n}\n\n  .resizable-dialog-container:hover::after {\n  opacity: 0.8;\n}\n\n.breadcrumb[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 8px 12px;\n  background: #fafafa;\n  border-bottom: 1px solid #e0e0e0;\n  min-height: 48px;\n  gap: 8px;\n}\n\n.breadcrumb-back[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.breadcrumb-path[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  flex: 1;\n  overflow-x: auto;\n  overflow-y: hidden;\n  gap: 4px;\n  padding: 4px 0;\n}\n\n.breadcrumb-path[_ngcontent-%COMP%]::-webkit-scrollbar {\n  height: 4px;\n}\n\n.breadcrumb-path[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: #f1f1f1;\n}\n\n.breadcrumb-path[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: #888;\n  border-radius: 2px;\n}\n\n.breadcrumb-segment[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  padding: 4px 8px;\n  min-width: -moz-fit-content;\n  min-width: fit-content;\n  text-transform: none;\n  font-size: 0.85rem;\n  border-radius: 4px;\n  transition: all 0.2s ease;\n  max-width: 150px;\n}\n\n.breadcrumb-segment[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.breadcrumb-segment[_ngcontent-%COMP%]:not([disabled]):hover {\n  background: #e3f2fd;\n  color: #1976d2;\n}\n\n.breadcrumb-segment.current[_ngcontent-%COMP%] {\n  font-weight: 600;\n  color: #1976d2;\n}\n\n.breadcrumb-segment[disabled][_ngcontent-%COMP%] {\n  opacity: 1;\n  cursor: default;\n}\n\n.breadcrumb-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  flex-shrink: 0;\n}\n\n.breadcrumb-separator[_ngcontent-%COMP%] {\n  font-size: 14px;\n  color: #999;\n  flex-shrink: 0;\n  margin: 0 2px;\n}\n\n.breadcrumb-action[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  margin-left: auto;\n}\n\n.filter-section[_ngcontent-%COMP%] {\n  min-width: 250px;\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.filter-badge[_ngcontent-%COMP%] {\n  background: #4caf50;\n  color: white;\n  padding: 2px 8px;\n  border-radius: 12px;\n  font-size: 0.8rem;\n  font-weight: 600;\n  margin-right: 8px;\n}\n\n.filter-chip-list[_ngcontent-%COMP%] {\n  margin: 0;\n}\n\n.filter-chip-list[_ngcontent-%COMP%]     .mat-chip {\n  font-size: 0.85rem;\n  min-height: 28px;\n}\n\n.clear-filter-button[_ngcontent-%COMP%] {\n  width: 32px;\n  height: 32px;\n}\n\n.clear-filter-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n}\n\n.context-menu[_ngcontent-%COMP%] {\n  max-width: 350px;\n}\n\n.context-menu[_ngcontent-%COMP%]     .mat-menu-content {\n  padding: 0;\n}\n\n.context-menu-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 12px 16px;\n  background: #f5f5f5;\n  border-bottom: 1px solid #e0e0e0;\n}\n\n.context-menu-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: #2196f3;\n  flex-shrink: 0;\n}\n\n.context-menu-info[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n\n.context-menu-name[_ngcontent-%COMP%] {\n  font-weight: 600;\n  font-size: 0.95rem;\n  color: #333;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.context-menu-path[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  color: #666;\n  font-family: \"Courier New\", monospace;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  margin-top: 2px;\n}\n\n.menu-hint[_ngcontent-%COMP%] {\n  font-size: 0.75rem;\n  color: #999;\n  font-style: italic;\n  margin-left: auto;\n  padding-left: 12px;\n  display: block;\n  margin-top: 2px;\n}\n\n.content-item.context-menu-active[_ngcontent-%COMP%] {\n  background: #fff3cd;\n  border: 1px solid #ffc107;\n  box-shadow: 0 0 0 3px rgba(255, 193, 7, 0.2);\n}\n\n.content-item.hovered[_ngcontent-%COMP%]:not(.context-menu-active):not(.selected) {\n  background: #f5f5f5;\n}\n\n  .success-snackbar {\n  background: #4caf50 !important;\n  color: white !important;\n}\n\n@media (max-width: 768px) {\n  .file-explorer-container[_ngcontent-%COMP%] {\n    height: 500px;\n  }\n\n  .explorer-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    gap: 12px;\n    align-items: stretch;\n  }\n\n  .filter-section[_ngcontent-%COMP%] {\n    min-width: unset;\n  }\n\n  .left-panel[_ngcontent-%COMP%] {\n    width: 240px;\n  }\n\n  .nav-label[_ngcontent-%COMP%] {\n    font-size: 0.9rem;\n  }\n\n  .bottom-panel[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: stretch;\n  }\n  .bottom-panel[_ngcontent-%COMP%]   .action-buttons[_ngcontent-%COMP%] {\n    margin-left: 0;\n    margin-top: 12px;\n  }\n\n  .breadcrumb-segment[_ngcontent-%COMP%] {\n    font-size: 0.8rem;\n    padding: 3px 6px;\n    max-width: 100px;\n  }\n\n  .breadcrumb-icon[_ngcontent-%COMP%] {\n    font-size: 14px;\n    width: 14px;\n    height: 14px;\n  }\n\n  .breadcrumb-separator[_ngcontent-%COMP%] {\n    font-size: 12px;\n    margin: 0 1px;\n  }\n\n  .context-menu[_ngcontent-%COMP%] {\n    max-width: 300px;\n  }\n\n  .menu-hint[_ngcontent-%COMP%] {\n    display: none;\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2hvdy1maWxlLXN5c3RlbS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EseUJBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0FBQUY7O0FBR0E7RUFDRSxxREFBQTtFQUNBLFlBQUE7RUFDQSxrQkFBQTtFQUNBLHdDQUFBO0VBQ0EsYUFBQTtFQUNBLDhCQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0FBQUY7O0FBR0E7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLE9BQUE7QUFBRjs7QUFHQTtFQUNFLGdCQUFBO0FBQUY7O0FBR0E7RUFDRSxXQUFBO0FBQUY7O0FBR0k7RUFDRSwwQ0FBQTtBQUROOztBQUlJO0VBQ0UsdUJBQUE7QUFGTjs7QUFLSTtFQUNFLDBDQUFBO0FBSE47O0FBTUk7RUFDRSx1QkFBQTtBQUpOOztBQU1NO0VBQ0UsMENBQUE7QUFKUjs7QUFRSTtFQUNFLDBDQUFBO0FBTk47O0FBV0E7RUFDRSxhQUFBO0VBQ0EsT0FBQTtFQUNBLGFBQUE7QUFSRjs7QUFZQTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGdCQUFBO0FBVEY7O0FBWUE7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLDBCQUFBO0VBQ0EseUJBQUE7RUFDQSxxQkFBQTtBQVRGOztBQVlBO0VBQ0UsY0FBQTtBQVRGOztBQVlBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EsZUFBQTtFQUNBLGtCQUFBO0VBQ0EseUJBQUE7RUFDQSx5QkFBQTtVQUFBLGlCQUFBO0FBVEY7O0FBV0U7RUFDRSxtQkFBQTtFQUNBLDBCQUFBO0FBVEo7O0FBWUU7RUFDRSwwQkFBQTtFQUNBLG1CQUFBO0VBQ0EsbUJBQUE7QUFWSjs7QUFhRTtFQUNFLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLDZDQUFBO0FBWEo7O0FBYUk7RUFDRSxZQUFBO0FBWE47O0FBY0k7RUFDRSxvQkFBQTtBQVpOOztBQWlCQTtFQUNFLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLFdBQUE7RUFDQSwyQkFBQTtBQWRGOztBQWlCQTtFQUNFLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7QUFkRjs7QUFrQkE7RUFDRSxPQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsWUFBQTtBQWZGOztBQWtCQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQ0FBQTtFQUNBLGdCQUFBO0FBZkY7O0FBaUJFO0VBQ0Usa0JBQUE7QUFmSjs7QUFtQkE7RUFDRSxxQ0FBQTtFQUNBLGlCQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtFQUNBLE9BQUE7QUFoQkY7O0FBbUJBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7RUFDQSxXQUFBO0FBaEJGOztBQWtCRTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7QUFoQko7O0FBb0JBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsWUFBQTtBQWpCRjs7QUFvQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSx5QkFBQTtFQUNBLHlCQUFBO1VBQUEsaUJBQUE7QUFqQkY7O0FBbUJFO0VBQ0UsbUJBQUE7QUFqQko7O0FBb0JFO0VBQ0UsMEJBQUE7RUFDQSxtQkFBQTtFQUNBLG1CQUFBO0FBbEJKOztBQXFCRTtFQUNFLG1CQUFBO0VBQ0EseUJBQUE7QUFuQko7O0FBcUJJO0VBQ0Usc0JBQUE7QUFuQk47O0FBdUJFO0VBQ0Usc0JBQUE7QUFyQko7O0FBd0JFO0VBQ0UsWUFBQTtFQUNBLGVBQUE7QUF0Qko7O0FBd0JJO0VBQ0UsdUJBQUE7QUF0Qk47O0FBeUJJO0VBQ0UsZUFBQTtBQXZCTjs7QUE0QkE7RUFDRSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0FBekJGOztBQTJCRTtFQUNFLGNBQUE7QUF6Qko7O0FBNkJBO0VBQ0Usa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7RUFDQSxPQUFBO0FBMUJGOztBQTZCQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxrQkFBQTtFQUNBLFdBQUE7RUFDQSxrQkFBQTtBQTFCRjs7QUE0QkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7QUExQko7O0FBNkJFO0VBQ0UsaUJBQUE7RUFDQSxTQUFBO0FBM0JKOztBQWdDQTtFQUNFLGFBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLDZCQUFBO0VBQ0EsZ0JBQUE7QUE3QkY7O0FBZ0NBO0VBQ0UsT0FBQTtBQTdCRjs7QUErQkU7RUFDRSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxXQUFBO0FBN0JKOztBQWdDRTtFQUNFLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLHFDQUFBO0FBOUJKOztBQWtDQTtFQUNFLGlCQUFBO0FBL0JGOztBQWlDRTtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7QUEvQko7O0FBb0NBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0FBakNGOztBQW9DQTtFQUNFLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7QUFqQ0Y7O0FBb0NBO0VBQ0UsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtBQWpDRjs7QUFvQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7QUFqQ0Y7O0FBcUNBO0VBQ0UsWUFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBbENGOztBQW9DRTtFQUNFLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxVQUFBO0FBbENKOztBQXFDRTtFQUNFLE9BQUE7RUFDQSxpQkFBQTtFQUNBLFVBQUE7RUFDQSxTQUFBO0VBQ0EsZ0JBQUE7QUFuQ0o7O0FBeUNFO0VBQ0UsV0FBQTtFQUNBLGtCQUFBO0VBQ0EsU0FBQTtFQUNBLFFBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0Esd0hBQ0U7RUFFRixpQ0FBQTtFQUNBLG1DQUFBO0VBQ0EsNEJBQUE7RUFDQSxZQUFBO0VBQ0EsNkJBQUE7QUF4Q0o7O0FBMkNFO0VBQ0UsWUFBQTtBQXpDSjs7QUFrREE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0NBQUE7RUFDQSxnQkFBQTtFQUNBLFFBQUE7QUEvQ0Y7O0FBa0RBO0VBQ0UsY0FBQTtBQS9DRjs7QUFrREE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLFFBQUE7RUFDQSxjQUFBO0FBL0NGOztBQWtERTtFQUNFLFdBQUE7QUFoREo7O0FBbURFO0VBQ0UsbUJBQUE7QUFqREo7O0FBb0RFO0VBQ0UsZ0JBQUE7RUFDQSxrQkFBQTtBQWxESjs7QUFzREE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZ0JBQUE7RUFDQSwyQkFBQTtFQUFBLHNCQUFBO0VBQ0Esb0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EseUJBQUE7RUFDQSxnQkFBQTtBQW5ERjs7QUFxREU7RUFDRSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7QUFuREo7O0FBc0RFO0VBQ0UsbUJBQUE7RUFDQSxjQUFBO0FBcERKOztBQXVERTtFQUNFLGdCQUFBO0VBQ0EsY0FBQTtBQXJESjs7QUF3REU7RUFDRSxVQUFBO0VBQ0EsZUFBQTtBQXRESjs7QUEwREE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBdkRGOztBQTBEQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsY0FBQTtFQUNBLGFBQUE7QUF2REY7O0FBMERBO0VBQ0UsY0FBQTtFQUNBLGlCQUFBO0FBdkRGOztBQTJEQTtFQUNFLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsUUFBQTtBQXhERjs7QUEyREE7RUFDRSxtQkFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0FBeERGOztBQTJEQTtFQUNFLFNBQUE7QUF4REY7O0FBMERFO0VBQ0Usa0JBQUE7RUFDQSxnQkFBQTtBQXhESjs7QUE0REE7RUFDRSxXQUFBO0VBQ0EsWUFBQTtBQXpERjs7QUEyREU7RUFDRSxlQUFBO0FBekRKOztBQThEQTtFQUNFLGdCQUFBO0FBM0RGOztBQTZERTtFQUNFLFVBQUE7QUEzREo7O0FBK0RBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQ0FBQTtBQTVERjs7QUErREE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsY0FBQTtBQTVERjs7QUErREE7RUFDRSxPQUFBO0VBQ0EsWUFBQTtBQTVERjs7QUErREE7RUFDRSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtBQTVERjs7QUErREE7RUFDRSxrQkFBQTtFQUNBLFdBQUE7RUFDQSxxQ0FBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtFQUNBLGVBQUE7QUE1REY7O0FBK0RBO0VBQ0Usa0JBQUE7RUFDQSxXQUFBO0VBQ0Esa0JBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7QUE1REY7O0FBZ0VBO0VBQ0UsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLDRDQUFBO0FBN0RGOztBQWdFQTtFQUNFLG1CQUFBO0FBN0RGOztBQWlFQTtFQUNFLDhCQUFBO0VBQ0EsdUJBQUE7QUE5REY7O0FBa0VBO0VBQ0U7SUFDRSxhQUFBO0VBL0RGOztFQWtFQTtJQUNFLHNCQUFBO0lBQ0EsU0FBQTtJQUNBLG9CQUFBO0VBL0RGOztFQWtFQTtJQUNFLGdCQUFBO0VBL0RGOztFQWtFQTtJQUNFLFlBQUE7RUEvREY7O0VBa0VBO0lBQ0UsaUJBQUE7RUEvREY7O0VBa0VBO0lBQ0Usc0JBQUE7SUFDQSxvQkFBQTtFQS9ERjtFQWlFRTtJQUNFLGNBQUE7SUFDQSxnQkFBQTtFQS9ESjs7RUFvRUE7SUFDRSxpQkFBQTtJQUNBLGdCQUFBO0lBQ0EsZ0JBQUE7RUFqRUY7O0VBb0VBO0lBQ0UsZUFBQTtJQUNBLFdBQUE7SUFDQSxZQUFBO0VBakVGOztFQW9FQTtJQUNFLGVBQUE7SUFDQSxhQUFBO0VBakVGOztFQW9FQTtJQUNFLGdCQUFBO0VBakVGOztFQW9FQTtJQUNFLGFBQUE7RUFqRUY7QUFDRiIsImZpbGUiOiJzaG93LWZpbGUtc3lzdGVtLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLy8gTWFpbiBjb250YWluZXJcclxuLmZpbGUtZXhwbG9yZXItY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgaGVpZ2h0OiAxMDAlO1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmZmZmZmY7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICBwb3NpdGlvbjogcmVsYXRpdmU7XHJcbn1cclxuXHJcbi5leHBsb3Jlci1oZWFkZXIge1xyXG4gIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM0N2I3ODQsICMyZTdkMzIpO1xyXG4gIGNvbG9yOiB3aGl0ZTtcclxuICBwYWRkaW5nOiAxNnB4IDIwcHg7XHJcbiAgYm94LXNoYWRvdzogMCAycHggNHB4IHJnYmEoMCwwLDAsMC4xKTtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTZweDtcclxufVxyXG5cclxuLmV4cGxvcmVyLXRpdGxlIHtcclxuICBtYXJnaW46IDA7XHJcbiAgZm9udC1zaXplOiAxLjJyZW07XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICBmbGV4OiAxO1xyXG59XHJcblxyXG4uZmlsdGVyLXNlY3Rpb24ge1xyXG4gIG1pbi13aWR0aDogMjUwcHg7XHJcbn1cclxuXHJcbi5maWx0ZXItaW5wdXQge1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIFxyXG4gIDo6bmctZGVlcCB7XHJcbiAgICAubWF0LWZvcm0tZmllbGQtb3V0bGluZSB7XHJcbiAgICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuMykgIWltcG9ydGFudDtcclxuICAgIH1cclxuICAgIFxyXG4gICAgLm1hdC1mb3JtLWZpZWxkLW91dGxpbmUtdGhpY2sge1xyXG4gICAgICBjb2xvcjogd2hpdGUgIWltcG9ydGFudDtcclxuICAgIH1cclxuICAgIFxyXG4gICAgLm1hdC1mb3JtLWZpZWxkLWxhYmVsIHtcclxuICAgICAgY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC44KSAhaW1wb3J0YW50O1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICAubWF0LWlucHV0LWVsZW1lbnQge1xyXG4gICAgICBjb2xvcjogd2hpdGUgIWltcG9ydGFudDtcclxuICAgICAgXHJcbiAgICAgICY6OnBsYWNlaG9sZGVyIHtcclxuICAgICAgICBjb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjYpICFpbXBvcnRhbnQ7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICAgIFxyXG4gICAgLm1hdC1pY29uIHtcclxuICAgICAgY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC44KSAhaW1wb3J0YW50O1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmV4cGxvcmVyLWNvbnRlbnQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleDogMTtcclxuICBtaW4taGVpZ2h0OiAwOyAvLyBJbXBvcnRhbnQgZm9yIHByb3BlciBmbGV4IGJlaGF2aW9yXHJcbn1cclxuXHJcbi8vIExlZnQgcGFuZWwgLSBOYXZpZ2F0aW9uXHJcbi5sZWZ0LXBhbmVsIHtcclxuICB3aWR0aDogMjgwcHg7XHJcbiAgYmFja2dyb3VuZDogI2Y4ZjlmYTtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgb3ZlcmZsb3cteTogYXV0bztcclxufVxyXG5cclxuLnNlY3Rpb24tdGl0bGUge1xyXG4gIGZvbnQtc2l6ZTogMC45cmVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA2MDA7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgbWFyZ2luOiAxNnB4IDE2cHggOHB4IDE2cHg7XHJcbiAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcclxuICBsZXR0ZXItc3BhY2luZzogMC41cHg7XHJcbn1cclxuXHJcbi5uYXYtaXRlbXMge1xyXG4gIHBhZGRpbmc6IDAgOHB4O1xyXG59XHJcblxyXG4ubmF2LWl0ZW0ge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBwYWRkaW5nOiAxMHB4IDEycHg7XHJcbiAgbWFyZ2luOiAycHggMDtcclxuICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgYm9yZGVyLXJhZGl1czogNnB4O1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjJzIGVhc2U7XHJcbiAgdXNlci1zZWxlY3Q6IG5vbmU7XHJcblxyXG4gICY6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZDogI2UzZjJmZDtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWCgycHgpO1xyXG4gIH1cclxuXHJcbiAgJjpmb2N1cyB7XHJcbiAgICBvdXRsaW5lOiAycHggc29saWQgIzIxOTZmMztcclxuICAgIG91dGxpbmUtb2Zmc2V0OiAycHg7XHJcbiAgICBiYWNrZ3JvdW5kOiAjZTNmMmZkO1xyXG4gIH1cclxuXHJcbiAgJi5hY3RpdmUge1xyXG4gICAgYmFja2dyb3VuZDogIzIxOTZmMztcclxuICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgIGJveC1zaGFkb3c6IDAgMnB4IDRweCByZ2JhKDMzLCAxNTAsIDI0MywgMC4zKTtcclxuXHJcbiAgICAubmF2LWljb24ge1xyXG4gICAgICBjb2xvcjogd2hpdGU7XHJcbiAgICB9XHJcblxyXG4gICAgJjpmb2N1cyB7XHJcbiAgICAgIG91dGxpbmUtY29sb3I6IHdoaXRlO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLm5hdi1pY29uIHtcclxuICBtYXJnaW4tcmlnaHQ6IDEycHg7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIHRyYW5zaXRpb246IGNvbG9yIDAuMnMgZWFzZTtcclxufVxyXG5cclxuLm5hdi1sYWJlbCB7XHJcbiAgZm9udC1zaXplOiAwLjk1cmVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG59XHJcblxyXG4vLyBSaWdodCBwYW5lbCAtIENvbnRlbnRcclxuLnJpZ2h0LXBhbmVsIHtcclxuICBmbGV4OiAxO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBtaW4td2lkdGg6IDA7IC8vIEltcG9ydGFudCBmb3IgcHJvcGVyIGZsZXggYmVoYXZpb3JcclxufVxyXG5cclxuLmJyZWFkY3J1bWIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBwYWRkaW5nOiAxMnB4IDE2cHg7XHJcbiAgYmFja2dyb3VuZDogI2ZhZmFmYTtcclxuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2UwZTBlMDtcclxuICBtaW4taGVpZ2h0OiA0OHB4O1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgbWFyZ2luLXJpZ2h0OiAxMnB4O1xyXG4gIH1cclxufVxyXG5cclxuLmN1cnJlbnQtcGF0aCB7XHJcbiAgZm9udC1mYW1pbHk6ICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcclxuICBmb250LXNpemU6IDAuOXJlbTtcclxuICBjb2xvcjogIzU1NTtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbiAgZmxleDogMTtcclxufVxyXG5cclxuLmxvYWRpbmctaW5kaWNhdG9yIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4O1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIFxyXG4gIHNwYW4ge1xyXG4gICAgbWFyZ2luLXRvcDogMTJweDtcclxuICAgIGZvbnQtc2l6ZTogMC45cmVtO1xyXG4gIH1cclxufVxyXG5cclxuLmNvbnRlbnQtbGlzdCB7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIHBhZGRpbmc6IDhweDtcclxufVxyXG5cclxuLmNvbnRlbnQtaXRlbSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDEwcHggMTJweDtcclxuICBtYXJnaW46IDJweCAwO1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICBib3JkZXItcmFkaXVzOiA2cHg7XHJcbiAgdHJhbnNpdGlvbjogYWxsIDAuMnMgZWFzZTtcclxuICB1c2VyLXNlbGVjdDogbm9uZTtcclxuXHJcbiAgJjpob3ZlciB7XHJcbiAgICBiYWNrZ3JvdW5kOiAjZjBmMGYwO1xyXG4gIH1cclxuXHJcbiAgJjpmb2N1cyB7XHJcbiAgICBvdXRsaW5lOiAycHggc29saWQgIzIxOTZmMztcclxuICAgIG91dGxpbmUtb2Zmc2V0OiAycHg7XHJcbiAgICBiYWNrZ3JvdW5kOiAjZjBmMGYwO1xyXG4gIH1cclxuXHJcbiAgJi5zZWxlY3RlZCB7XHJcbiAgICBiYWNrZ3JvdW5kOiAjZTNmMmZkO1xyXG4gICAgYm9yZGVyOiAxcHggc29saWQgIzIxOTZmMztcclxuXHJcbiAgICAmOmZvY3VzIHtcclxuICAgICAgb3V0bGluZS1jb2xvcjogIzE5NzZkMjtcclxuICAgIH1cclxuICB9XHJcblxyXG4gICY6YWN0aXZlIHtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMC45OCk7XHJcbiAgfVxyXG5cclxuICAmLm5vdC1zZWxlY3RhYmxlIHtcclxuICAgIG9wYWNpdHk6IDAuNjtcclxuICAgIGN1cnNvcjogZGVmYXVsdDtcclxuICAgIFxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6IHRyYW5zcGFyZW50O1xyXG4gICAgfVxyXG5cclxuICAgICY6YWN0aXZlIHtcclxuICAgICAgdHJhbnNmb3JtOiBub25lO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLml0ZW0taWNvbiB7XHJcbiAgbWFyZ2luLXJpZ2h0OiAxMnB4O1xyXG4gIGZvbnQtc2l6ZTogMjBweDtcclxuICBjb2xvcjogIzY2NjtcclxuICBcclxuICAuY29udGVudC1pdGVtOmhvdmVyICYge1xyXG4gICAgY29sb3I6ICMyMTk2ZjM7XHJcbiAgfVxyXG59XHJcblxyXG4uaXRlbS1uYW1lIHtcclxuICBmb250LXNpemU6IDAuOTVyZW07XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5lbXB0eS1zdGF0ZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNjBweCAyMHB4O1xyXG4gIGNvbG9yOiAjOTk5O1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgZm9udC1zaXplOiA0OHB4O1xyXG4gICAgd2lkdGg6IDQ4cHg7XHJcbiAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gICAgb3BhY2l0eTogMC41O1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBmb250LXNpemU6IDAuOXJlbTtcclxuICAgIG1hcmdpbjogMDtcclxuICB9XHJcbn1cclxuXHJcbi8vIEJvdHRvbSBwYW5lbFxyXG4uYm90dG9tLXBhbmVsIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDE2cHggMjBweDtcclxuICBiYWNrZ3JvdW5kOiAjZjhmOWZhO1xyXG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG4gIG1pbi1oZWlnaHQ6IDgwcHg7XHJcbn1cclxuXHJcbi5zZWxlY3RlZC1mb2xkZXItaW5mbyB7XHJcbiAgZmxleDogMTtcclxuICBcclxuICBtYXQtY2FyZC10aXRsZSB7XHJcbiAgICBmb250LXNpemU6IDFyZW07XHJcbiAgICBtYXJnaW4tYm90dG9tOiA0cHg7XHJcbiAgICBjb2xvcjogIzMzMztcclxuICB9XHJcbiAgXHJcbiAgbWF0LWNhcmQtc3VidGl0bGUge1xyXG4gICAgZm9udC1zaXplOiAwLjg1cmVtO1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xyXG4gIH1cclxufVxyXG5cclxuLmFjdGlvbi1idXR0b25zIHtcclxuICBtYXJnaW4tbGVmdDogMjBweDtcclxuICBcclxuICBidXR0b24ge1xyXG4gICAgbWluLXdpZHRoOiAxNDBweDtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBMZWdhY3kgY29tcGF0aWJpbGl0eSBzdHlsZXNcclxuLmZsZXgtY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcbi5mbGV4LWl0ZW1zLW92ZXJmbG93IHtcclxuICBtYXgtaGVpZ2h0OiA0MDBweDtcclxuICBvdmVyZmxvdy14OiBzY3JvbGw7XHJcbiAgb3ZlcmZsb3cteTogc2Nyb2xsO1xyXG4gIGJhY2tncm91bmQ6IHRvbWF0bztcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIGZvbnQtc2l6ZTogM2VtO1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5mbGV4LWl0ZW1zIHtcclxuICB3aWR0aDogNTAwcHg7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIGZvbnQtc2l6ZTogM2VtO1xyXG59XHJcblxyXG4uZmxleC1zZWxlY3RlZC1mb2xkZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IHJvdztcclxufVxyXG5cclxuLy8gUmVzaXphYmxlIGRpYWxvZyB3cmFwcGVyXHJcbjo6bmctZGVlcCAucmVzaXphYmxlLWRpYWxvZy1jb250YWluZXIge1xyXG4gIHJlc2l6ZTogYm90aDtcclxuICBvdmVyZmxvdzogYXV0bztcclxuICBtaW4td2lkdGg6IDQwMHB4O1xyXG4gIG1pbi1oZWlnaHQ6IDMwMHB4O1xyXG4gIG1heC13aWR0aDogOTB2dztcclxuICBtYXgtaGVpZ2h0OiA5MHZoO1xyXG4gIFxyXG4gIC5tYXQtZGlhbG9nLWNvbnRhaW5lciB7XHJcbiAgICBoZWlnaHQ6IDEwMCU7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIHBhZGRpbmc6IDA7XHJcbiAgfVxyXG4gIFxyXG4gIC5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gICAgZmxleDogMTtcclxuICAgIG1heC1oZWlnaHQ6IHVuc2V0O1xyXG4gICAgcGFkZGluZzogMDtcclxuICAgIG1hcmdpbjogMDtcclxuICAgIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgfVxyXG59XHJcblxyXG4vLyBSZXNpemUgaGFuZGxlIHN0eWxpbmdcclxuOjpuZy1kZWVwIC5yZXNpemFibGUtZGlhbG9nLWNvbnRhaW5lciB7XHJcbiAgJjo6YWZ0ZXIge1xyXG4gICAgY29udGVudDogJyc7XHJcbiAgICBwb3NpdGlvbjogYWJzb2x1dGU7XHJcbiAgICBib3R0b206IDA7XHJcbiAgICByaWdodDogMDtcclxuICAgIHdpZHRoOiAxNXB4O1xyXG4gICAgaGVpZ2h0OiAxNXB4O1xyXG4gICAgY3Vyc29yOiBud3NlLXJlc2l6ZTtcclxuICAgIGJhY2tncm91bmQ6XHJcbiAgICAgIGxpbmVhci1ncmFkaWVudCgxMzVkZWcsIHRyYW5zcGFyZW50IDUwJSwgIzQ3Yjc4NCA1MCUpLFxyXG4gICAgICBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCB0cmFuc3BhcmVudCA1MCUsICM0N2I3ODQgNTAlKTtcclxuICAgIGJhY2tncm91bmQtc2l6ZTogNXB4IDVweCwgNXB4IDVweDtcclxuICAgIGJhY2tncm91bmQtcG9zaXRpb246IDEwMCUgMCwgMCAxMDAlO1xyXG4gICAgYmFja2dyb3VuZC1yZXBlYXQ6IG5vLXJlcGVhdDtcclxuICAgIG9wYWNpdHk6IDAuNTtcclxuICAgIHRyYW5zaXRpb246IG9wYWNpdHkgMC4ycyBlYXNlO1xyXG4gIH1cclxuXHJcbiAgJjpob3Zlcjo6YWZ0ZXIge1xyXG4gICAgb3BhY2l0eTogMC44O1xyXG4gIH1cclxufVxyXG5cclxuLy8gPT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT1cclxuLy8gTkVXIFNUWUxFUyBGT1IgVVggSU1QUk9WRU1FTlRTXHJcbi8vID09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09XHJcblxyXG4vLyBCcmVhZGNydW1iIG1pZ2xpb3JhdG9cclxuLmJyZWFkY3J1bWIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA4cHggMTJweDtcclxuICBiYWNrZ3JvdW5kOiAjZmFmYWZhO1xyXG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjZTBlMGUwO1xyXG4gIG1pbi1oZWlnaHQ6IDQ4cHg7XHJcbiAgZ2FwOiA4cHg7XHJcbn1cclxuXHJcbi5icmVhZGNydW1iLWJhY2sge1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG59XHJcblxyXG4uYnJlYWRjcnVtYi1wYXRoIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy14OiBhdXRvO1xyXG4gIG92ZXJmbG93LXk6IGhpZGRlbjtcclxuICBnYXA6IDRweDtcclxuICBwYWRkaW5nOiA0cHggMDtcclxuXHJcbiAgLy8gU2Nyb2xsYmFyIHNvdHRpbGVcclxuICAmOjotd2Via2l0LXNjcm9sbGJhciB7XHJcbiAgICBoZWlnaHQ6IDRweDtcclxuICB9XHJcblxyXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRyYWNrIHtcclxuICAgIGJhY2tncm91bmQ6ICNmMWYxZjE7XHJcbiAgfVxyXG5cclxuICAmOjotd2Via2l0LXNjcm9sbGJhci10aHVtYiB7XHJcbiAgICBiYWNrZ3JvdW5kOiAjODg4O1xyXG4gICAgYm9yZGVyLXJhZGl1czogMnB4O1xyXG4gIH1cclxufVxyXG5cclxuLmJyZWFkY3J1bWItc2VnbWVudCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogNHB4O1xyXG4gIHBhZGRpbmc6IDRweCA4cHg7XHJcbiAgbWluLXdpZHRoOiBmaXQtY29udGVudDtcclxuICB0ZXh0LXRyYW5zZm9ybTogbm9uZTtcclxuICBmb250LXNpemU6IDAuODVyZW07XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjJzIGVhc2U7XHJcbiAgbWF4LXdpZHRoOiAxNTBweDtcclxuXHJcbiAgc3BhbiB7XHJcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIH1cclxuXHJcbiAgJjpub3QoW2Rpc2FibGVkXSk6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZDogI2UzZjJmZDtcclxuICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gIH1cclxuXHJcbiAgJi5jdXJyZW50IHtcclxuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XHJcbiAgICBjb2xvcjogIzE5NzZkMjtcclxuICB9XHJcblxyXG4gICZbZGlzYWJsZWRdIHtcclxuICAgIG9wYWNpdHk6IDE7XHJcbiAgICBjdXJzb3I6IGRlZmF1bHQ7XHJcbiAgfVxyXG59XHJcblxyXG4uYnJlYWRjcnVtYi1pY29uIHtcclxuICBmb250LXNpemU6IDE2cHg7XHJcbiAgd2lkdGg6IDE2cHg7XHJcbiAgaGVpZ2h0OiAxNnB4O1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG59XHJcblxyXG4uYnJlYWRjcnVtYi1zZXBhcmF0b3Ige1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxuICBjb2xvcjogIzk5OTtcclxuICBmbGV4LXNocmluazogMDtcclxuICBtYXJnaW46IDAgMnB4O1xyXG59XHJcblxyXG4uYnJlYWRjcnVtYi1hY3Rpb24ge1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG4gIG1hcmdpbi1sZWZ0OiBhdXRvO1xyXG59XHJcblxyXG4vLyBGaWx0ZXIgc2VjdGlvbiBpbXByb3ZlbWVudHNcclxuLmZpbHRlci1zZWN0aW9uIHtcclxuICBtaW4td2lkdGg6IDI1MHB4O1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBnYXA6IDhweDtcclxufVxyXG5cclxuLmZpbHRlci1iYWRnZSB7XHJcbiAgYmFja2dyb3VuZDogIzRjYWY1MDtcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgcGFkZGluZzogMnB4IDhweDtcclxuICBib3JkZXItcmFkaXVzOiAxMnB4O1xyXG4gIGZvbnQtc2l6ZTogMC44cmVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA2MDA7XHJcbiAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcbn1cclxuXHJcbi5maWx0ZXItY2hpcC1saXN0IHtcclxuICBtYXJnaW46IDA7XHJcblxyXG4gIDo6bmctZGVlcCAubWF0LWNoaXAge1xyXG4gICAgZm9udC1zaXplOiAwLjg1cmVtO1xyXG4gICAgbWluLWhlaWdodDogMjhweDtcclxuICB9XHJcbn1cclxuXHJcbi5jbGVhci1maWx0ZXItYnV0dG9uIHtcclxuICB3aWR0aDogMzJweDtcclxuICBoZWlnaHQ6IDMycHg7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMThweDtcclxuICB9XHJcbn1cclxuXHJcbi8vIENvbnRleHQgbWVudSBzdHlsZXNcclxuLmNvbnRleHQtbWVudSB7XHJcbiAgbWF4LXdpZHRoOiAzNTBweDtcclxuXHJcbiAgOjpuZy1kZWVwIC5tYXQtbWVudS1jb250ZW50IHtcclxuICAgIHBhZGRpbmc6IDA7XHJcbiAgfVxyXG59XHJcblxyXG4uY29udGV4dC1tZW51LWhlYWRlciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTJweCAxNnB4O1xyXG4gIGJhY2tncm91bmQ6ICNmNWY1ZjU7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcbn1cclxuXHJcbi5jb250ZXh0LW1lbnUtaWNvbiB7XHJcbiAgZm9udC1zaXplOiAyNHB4O1xyXG4gIHdpZHRoOiAyNHB4O1xyXG4gIGhlaWdodDogMjRweDtcclxuICBjb2xvcjogIzIxOTZmMztcclxuICBmbGV4LXNocmluazogMDtcclxufVxyXG5cclxuLmNvbnRleHQtbWVudS1pbmZvIHtcclxuICBmbGV4OiAxO1xyXG4gIG1pbi13aWR0aDogMDtcclxufVxyXG5cclxuLmNvbnRleHQtbWVudS1uYW1lIHtcclxuICBmb250LXdlaWdodDogNjAwO1xyXG4gIGZvbnQtc2l6ZTogMC45NXJlbTtcclxuICBjb2xvcjogIzMzMztcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbn1cclxuXHJcbi5jb250ZXh0LW1lbnUtcGF0aCB7XHJcbiAgZm9udC1zaXplOiAwLjc1cmVtO1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIG1hcmdpbi10b3A6IDJweDtcclxufVxyXG5cclxuLm1lbnUtaGludCB7XHJcbiAgZm9udC1zaXplOiAwLjc1cmVtO1xyXG4gIGNvbG9yOiAjOTk5O1xyXG4gIGZvbnQtc3R5bGU6IGl0YWxpYztcclxuICBtYXJnaW4tbGVmdDogYXV0bztcclxuICBwYWRkaW5nLWxlZnQ6IDEycHg7XHJcbiAgZGlzcGxheTogYmxvY2s7XHJcbiAgbWFyZ2luLXRvcDogMnB4O1xyXG59XHJcblxyXG4vLyBFdmlkZW56aWEgaXRlbSBjb24gY29udGV4dCBtZW51IGF0dGl2b1xyXG4uY29udGVudC1pdGVtLmNvbnRleHQtbWVudS1hY3RpdmUge1xyXG4gIGJhY2tncm91bmQ6ICNmZmYzY2Q7XHJcbiAgYm9yZGVyOiAxcHggc29saWQgI2ZmYzEwNztcclxuICBib3gtc2hhZG93OiAwIDAgMCAzcHggcmdiYSgyNTUsIDE5MywgNywgMC4yKTtcclxufVxyXG5cclxuLmNvbnRlbnQtaXRlbS5ob3ZlcmVkOm5vdCguY29udGV4dC1tZW51LWFjdGl2ZSk6bm90KC5zZWxlY3RlZCkge1xyXG4gIGJhY2tncm91bmQ6ICNmNWY1ZjU7XHJcbn1cclxuXHJcbi8vIFN1Y2Nlc3Mgc25hY2tiYXJcclxuOjpuZy1kZWVwIC5zdWNjZXNzLXNuYWNrYmFyIHtcclxuICBiYWNrZ3JvdW5kOiAjNGNhZjUwICFpbXBvcnRhbnQ7XHJcbiAgY29sb3I6IHdoaXRlICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcbi8vIFJlc3BvbnNpdmUgZGVzaWduXHJcbkBtZWRpYSAobWF4LXdpZHRoOiA3NjhweCkge1xyXG4gIC5maWxlLWV4cGxvcmVyLWNvbnRhaW5lciB7XHJcbiAgICBoZWlnaHQ6IDUwMHB4O1xyXG4gIH1cclxuXHJcbiAgLmV4cGxvcmVyLWhlYWRlciB7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gICAgZ2FwOiAxMnB4O1xyXG4gICAgYWxpZ24taXRlbXM6IHN0cmV0Y2g7XHJcbiAgfVxyXG5cclxuICAuZmlsdGVyLXNlY3Rpb24ge1xyXG4gICAgbWluLXdpZHRoOiB1bnNldDtcclxuICB9XHJcblxyXG4gIC5sZWZ0LXBhbmVsIHtcclxuICAgIHdpZHRoOiAyNDBweDtcclxuICB9XHJcblxyXG4gIC5uYXYtbGFiZWwge1xyXG4gICAgZm9udC1zaXplOiAwLjlyZW07XHJcbiAgfVxyXG5cclxuICAuYm90dG9tLXBhbmVsIHtcclxuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICBhbGlnbi1pdGVtczogc3RyZXRjaDtcclxuXHJcbiAgICAuYWN0aW9uLWJ1dHRvbnMge1xyXG4gICAgICBtYXJnaW4tbGVmdDogMDtcclxuICAgICAgbWFyZ2luLXRvcDogMTJweDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC8vIEJyZWFkY3J1bWIgcmVzcG9uc2l2b1xyXG4gIC5icmVhZGNydW1iLXNlZ21lbnQge1xyXG4gICAgZm9udC1zaXplOiAwLjhyZW07XHJcbiAgICBwYWRkaW5nOiAzcHggNnB4O1xyXG4gICAgbWF4LXdpZHRoOiAxMDBweDtcclxuICB9XHJcblxyXG4gIC5icmVhZGNydW1iLWljb24ge1xyXG4gICAgZm9udC1zaXplOiAxNHB4O1xyXG4gICAgd2lkdGg6IDE0cHg7XHJcbiAgICBoZWlnaHQ6IDE0cHg7XHJcbiAgfVxyXG5cclxuICAuYnJlYWRjcnVtYi1zZXBhcmF0b3Ige1xyXG4gICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgbWFyZ2luOiAwIDFweDtcclxuICB9XHJcblxyXG4gIC5jb250ZXh0LW1lbnUge1xyXG4gICAgbWF4LXdpZHRoOiAzMDBweDtcclxuICB9XHJcblxyXG4gIC5tZW51LWhpbnQge1xyXG4gICAgZGlzcGxheTogbm9uZTsgLy8gTmFzY29uZGUgaGludCBzdSBtb2JpbGUgcGVyIHJpc3Bhcm1pYXJlIHNwYXppb1xyXG4gIH1cclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "zUnb":
/*!*********************!*\
  !*** ./src/main.ts ***!
  \*********************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/platform-browser */ "jhN1");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _app_app_module__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./app/app.module */ "ZAI4");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./environments/environment */ "AytR");




if (_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].production) {
    Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["enableProdMode"])();
}
_angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["platformBrowser"]().bootstrapModule(_app_app_module__WEBPACK_IMPORTED_MODULE_2__["AppModule"])
    .catch(err => console.error(err));


/***/ }),

/***/ "zabz":
/*!*****************************************************************************************!*\
  !*** ./src/app/projects/dialogs/modern-clone-project/modern-clone-project.component.ts ***!
  \*****************************************************************************************/
/*! exports provided: ModernCloneProjectComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ModernCloneProjectComponent", function() { return ModernCloneProjectComponent; });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! tslib */ "mrSG");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo */ "N+BC");
/* harmony import */ var _git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../git/components/git-messages/git-messages.component */ "jwHG");
/* harmony import */ var _git_dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../git/dialogs/git-token-dialog/git-token-dialog.component */ "8zqs");
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ "DEjE");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ "eAi6");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/material/autocomplete */ "/1cH");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/core */ "FKr1");


























function ModernCloneProjectComponent_mat_hint_9_span_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "(OAuth)");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_mat_hint_9_span_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "(Basic Auth)");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_mat_hint_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](2, ModernCloneProjectComponent_mat_hint_9_span_2_Template, 2, 0, "span", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](3, ModernCloneProjectComponent_mat_hint_9_span_3_Template, 2, 0, "span", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r0.getProviderDisplayName(), " repository detected ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r0.authType === "oauth");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r0.authType === "basic");
} }
function ModernCloneProjectComponent_div_10_div_8_div_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "open_in_browser");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](5, "Autenticazione automatica via browser");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("", ctx_r6.getProviderDisplayName(), " aprira una finestra del browser per l'autenticazione OAuth.");
} }
function ModernCloneProjectComponent_div_10_div_8_div_6_div_4_span_4_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r10.tokenUsername);
} }
function ModernCloneProjectComponent_div_10_div_8_div_6_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](4, ModernCloneProjectComponent_div_10_div_8_div_6_div_4_span_4_Template, 5, 1, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "span", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "span", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](9, "button", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_div_10_div_8_div_6_div_4_Template_button_click_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](4); return ctx_r11.deleteToken(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](11, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("valid", ctx_r8.tokenValid)("invalid", !ctx_r8.tokenValid);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r8.tokenValid ? "check_circle" : "error", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r8.tokenUsername);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r8.tokenStatus);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("valid", ctx_r8.tokenValid)("invalid", !ctx_r8.tokenValid);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" (", ctx_r8.tokenValid ? "Valido" : "Non valido o scaduto", ") ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx_r8.isDeleting);
} }
function ModernCloneProjectComponent_div_10_div_8_div_6_div_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "Il token salvato non \u00E8 valido. Considera di eliminarlo o inserire credenziali diverse.");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_8_div_6_Template(rf, ctx) { if (rf & 1) {
    const _r14 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-checkbox", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_div_10_div_8_div_6_Template_mat_checkbox_ngModelChange_2_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r14); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3); return ctx_r13.useSavedToken = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3, " Usa token GitHub salvato ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](4, ModernCloneProjectComponent_div_10_div_8_div_6_div_4_Template, 12, 13, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](5, ModernCloneProjectComponent_div_10_div_8_div_6_div_5_Template, 5, 0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r7.useSavedToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r7.useSavedToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r7.useSavedToken && !ctx_r7.tokenValid);
} }
function ModernCloneProjectComponent_div_10_div_8_Template(rf, ctx) { if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, ModernCloneProjectComponent_div_10_div_8_div_1_Template, 8, 1, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_div_10_div_8_Template_button_click_2_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r16); const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r15.toggleCredentialForm(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](6, ModernCloneProjectComponent_div_10_div_8_div_6_Template, 6, 3, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx_r4.showCredentialForm);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r4.showCredentialForm ? "close" : "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r4.showCredentialForm ? "Usa autenticazione automatica" : "Inserisci credenziali manualmente", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r4.isGitHubRepo && ctx_r4.hasGitHubToken && !ctx_r4.showCredentialForm);
} }
function ModernCloneProjectComponent_div_10_div_9_div_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "vpn_key");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("", ctx_r17.getProviderDisplayName(), " richiede autenticazione manuale");
} }
function ModernCloneProjectComponent_div_10_div_9_mat_spinner_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "mat-spinner", 49);
} }
function ModernCloneProjectComponent_div_10_div_9_mat_option_9_Template(rf, ctx) { if (rf & 1) {
    const _r29 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-option", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "span", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "button", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_div_10_div_9_mat_option_9_Template_button_click_4_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r29); const account_r27 = ctx.$implicit; const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3); return ctx_r28.deleteAccount(account_r27, $event); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const account_r27 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", account_r27.username);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](account_r27.username);
} }
function ModernCloneProjectComponent_div_10_div_9_mat_option_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-option", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "em");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("Nuovo account: ", ctx_r21.manualCredentials.username, "");
} }
function ModernCloneProjectComponent_div_10_div_9_mat_hint_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r22.availableAccounts.length, " account salvati disponibili ");
} }
function ModernCloneProjectComponent_div_10_div_9_mat_hint_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Per GitHub usa un Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_9_mat_hint_17_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Per GitLab usa un Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_9_mat_hint_18_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Per Bitbucket usa una App Password");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_9_mat_hint_19_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Per Azure DevOps usa un PAT");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_9_Template(rf, ctx) { if (rf & 1) {
    const _r31 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, ModernCloneProjectComponent_div_10_div_9_div_1_Template, 5, 1, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-form-field", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "Username");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "input", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_div_10_div_9_Template_input_ngModelChange_5_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r31); const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r30.manualCredentials.username = $event; })("ngModelChange", function ModernCloneProjectComponent_div_10_div_9_Template_input_ngModelChange_5_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r31); const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r32.filterAccounts($event); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](6, ModernCloneProjectComponent_div_10_div_9_mat_spinner_6_Template, 1, 0, "mat-spinner", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "mat-autocomplete", 42, 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("optionSelected", function ModernCloneProjectComponent_div_10_div_9_Template_mat_autocomplete_optionSelected_7_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r31); const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r33.selectAccount($event.option.value); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, ModernCloneProjectComponent_div_10_div_9_mat_option_9_Template, 7, 2, "mat-option", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, ModernCloneProjectComponent_div_10_div_9_mat_option_10_Template, 3, 1, "mat-option", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](11, ModernCloneProjectComponent_div_10_div_9_mat_hint_11_Template, 2, 1, "mat-hint", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](12, "mat-form-field", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](13, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](14, "Password / Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](15, "input", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_div_10_div_9_Template_input_ngModelChange_15_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r31); const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r34.manualCredentials.password = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](16, ModernCloneProjectComponent_div_10_div_9_mat_hint_16_Template, 2, 0, "mat-hint", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](17, ModernCloneProjectComponent_div_10_div_9_mat_hint_17_Template, 2, 0, "mat-hint", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](18, ModernCloneProjectComponent_div_10_div_9_mat_hint_18_Template, 2, 0, "mat-hint", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](19, ModernCloneProjectComponent_div_10_div_9_mat_hint_19_Template, 2, 0, "mat-hint", 5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵreference"](8);
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.authType === "basic");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.manualCredentials.username)("matAutocomplete", _r19);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.isLoadingAccounts);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("displayWith", ctx_r5.displayAccountFn);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r5.filteredAccounts);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.filteredAccounts.length === 0 && ctx_r5.manualCredentials.username && !ctx_r5.isLoadingAccounts);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.availableAccounts.length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.manualCredentials.password);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.isGitHubRepo);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.detectedProvider === "gitlab");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.detectedProvider === "bitbucket");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.detectedProvider === "azure");
} }
function ModernCloneProjectComponent_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-card", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-card-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](5, "security");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](8, ModernCloneProjectComponent_div_10_div_8_Template, 7, 4, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, ModernCloneProjectComponent_div_10_div_9_Template, 20, 13, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" Authentication - ", ctx_r1.getProviderDisplayName(), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r1.authType === "oauth");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r1.authType === "basic" || ctx_r1.showCredentialForm);
} }
// OAuth providers: GCM handles authentication via browser
const OAUTH_PROVIDERS = ['github', 'gitlab', 'azure', 'bitbucket'];
// Basic Auth providers: always require manual credentials
const BASIC_AUTH_PROVIDERS = ['scm-manager', 'gitea', 'generic'];
class ModernCloneProjectComponent {
    constructor(dialog, mdFileService, gitService, dialogRef, waitingDialog, projectService, router, data) {
        this.dialog = dialog;
        this.mdFileService = mdFileService;
        this.gitService = gitService;
        this.dialogRef = dialogRef;
        this.waitingDialog = waitingDialog;
        this.projectService = projectService;
        this.router = router;
        this.data = data;
        this.cloneRequest = {
            url: '',
            localPath: '',
            branchName: '',
            useToken: true,
            useSSH: false
        };
        this.hasGitHubToken = false;
        this.tokenStatus = '';
        this.tokenUsername = '';
        this.tokenValid = false;
        this.useSavedToken = true;
        this.isDeleting = false;
        this.isGitHubRepo = false;
        this.authMethod = 'automatic';
        // Provider-based authentication
        this.detectedProvider = 'generic';
        this.authType = 'basic';
        this.showCredentialForm = true; // Show form for basic auth providers
        // For manual authentication (non-GitHub repos)
        this.manualCredentials = {
            username: '',
            password: ''
        };
        // Account selector for multi-account support
        this.availableAccounts = [];
        this.filteredAccounts = [];
        this.isLoadingAccounts = false;
        // For Share Project feature: when basePath is provided, the path is auto-computed
        this.isPrefilledFromShare = false;
        // URL validation state
        this.isValidatingUrl = false;
        this.urlValidationResult = null;
    }
    ngOnInit() {
        var _a;
        // Check if we have prefilled data from URL handler (Share Project feature)
        if ((_a = this.data) === null || _a === void 0 ? void 0 : _a.prefilledUrl) {
            console.log('[ModernClone] Using prefilled URL from URL handler:', this.data.prefilledUrl);
            this.cloneRequest.url = this.data.prefilledUrl;
            if (this.data.prefilledBranch) {
                this.cloneRequest.branchName = this.data.prefilledBranch;
            }
            if (this.data.prefilledUser) {
                this.manualCredentials.username = this.data.prefilledUser;
            }
            // Handle basePath from Share Project feature
            if (this.data.prefilledBasePath) {
                console.log('[ModernClone] Using prefilled basePath:', this.data.prefilledBasePath);
                this.isPrefilledFromShare = true;
                // Auto-compute the full path: basePath + repoName
                const repoName = this.extractRepoName(this.data.prefilledUrl);
                if (repoName) {
                    this.cloneRequest.localPath = `${this.data.prefilledBasePath}\\${repoName}`;
                }
                else {
                    this.cloneRequest.localPath = this.data.prefilledBasePath;
                }
                console.log('[ModernClone] Auto-computed localPath:', this.cloneRequest.localPath);
            }
            this.checkIfGitHubRepo();
        }
        else {
            // Get URL from clipboard (default behavior)
            this.mdFileService.getTextFromClipboard().subscribe(clipboard => {
                if (clipboard === null || clipboard === void 0 ? void 0 : clipboard.url) {
                    this.cloneRequest.url = clipboard.url;
                    this.checkIfGitHubRepo();
                }
            });
        }
        // Check GitHub token status
        this.checkGitHubToken();
        // When the project changes, navigate to the main environment
        this.projectService.currentProjects$.subscribe(project => {
            if (project != null && project != undefined) {
                this.router.navigate(['/main/navigation/document']);
                this.dialogRef.close();
            }
        });
    }
    checkGitHubToken() {
        this.gitService.getGitHubToken().subscribe(response => {
            this.hasGitHubToken = response.hasToken;
            this.tokenUsername = response.username || '';
            this.tokenValid = response.tokenValid;
            if (response.hasToken) {
                if (this.tokenUsername) {
                    this.tokenStatus = `${response.maskedToken}`;
                }
                else {
                    this.tokenStatus = `Token: ${response.maskedToken}`;
                }
            }
            else {
                this.tokenStatus = 'No GitHub token configured';
            }
            // Default to using saved token only if valid
            this.useSavedToken = response.hasToken && response.tokenValid;
        });
    }
    deleteToken() {
        const message = this.tokenUsername
            ? `Vuoi eliminare il token GitHub dell'account "${this.tokenUsername}"?`
            : 'Vuoi davvero eliminare il token GitHub salvato?';
        const confirmed = confirm(message);
        if (!confirmed)
            return;
        this.isDeleting = true;
        this.gitService.deleteGitHubToken().subscribe({
            next: () => {
                this.showMessage('Token eliminato con successo');
                this.hasGitHubToken = false;
                this.tokenStatus = 'No GitHub token configured';
                this.tokenUsername = '';
                this.tokenValid = false;
                this.useSavedToken = false;
                this.isDeleting = false;
            },
            error: (err) => {
                console.error('Error deleting token:', err);
                this.showMessage('Errore nell\'eliminazione del token');
                this.isDeleting = false;
            }
        });
    }
    onUrlChange() {
        this.detectProviderFromUrl();
    }
    /**
     * Detects the Git provider from URL and sets authentication type accordingly.
     * OAuth providers (GitHub, GitLab, Azure, Bitbucket): GCM handles auth via browser
     * Basic Auth providers (SCM-Manager, Gitea, Generic): Manual credentials required
     */
    detectProviderFromUrl() {
        const url = this.cloneRequest.url.toLowerCase();
        // Detect provider from URL
        if (url.includes('github.com')) {
            this.detectedProvider = 'github';
        }
        else if (url.includes('gitlab.com') || url.includes('gitlab')) {
            this.detectedProvider = 'gitlab';
        }
        else if (url.includes('dev.azure.com') || url.includes('visualstudio.com')) {
            this.detectedProvider = 'azure';
        }
        else if (url.includes('bitbucket.org') || url.includes('bitbucket')) {
            this.detectedProvider = 'bitbucket';
        }
        else if (url.includes('scm-manager') || url.includes('/scm/')) {
            this.detectedProvider = 'scm-manager';
        }
        else if (url.includes('gitea') || url.includes(':3000/')) {
            this.detectedProvider = 'gitea';
        }
        else {
            this.detectedProvider = 'generic';
        }
        // Set legacy flag for backward compatibility
        this.isGitHubRepo = this.detectedProvider === 'github';
        // Determine auth type based on provider
        if (OAUTH_PROVIDERS.includes(this.detectedProvider)) {
            this.authType = 'oauth';
            this.showCredentialForm = false; // GCM handles OAuth via browser
            this.authMethod = 'automatic';
        }
        else {
            this.authType = 'basic';
            this.showCredentialForm = true; // Always show form for basic auth
            this.authMethod = 'manual';
        }
        console.log(`[ModernClone] Detected provider: ${this.detectedProvider}, authType: ${this.authType}, showForm: ${this.showCredentialForm}`);
        // Load available accounts for this provider
        this.loadAccountsForProvider();
    }
    // Keep old method name for backward compatibility
    checkIfGitHubRepo() {
        this.detectProviderFromUrl();
    }
    /**
     * Loads available accounts for the detected provider
     */
    loadAccountsForProvider() {
        if (!this.detectedProvider || this.detectedProvider === 'generic') {
            this.availableAccounts = [];
            this.filteredAccounts = [];
            return;
        }
        // Map provider to account type
        const accountTypeMap = {
            'github': 'GitHub',
            'gitlab': 'GitLab',
            'azure': 'Azure',
            'bitbucket': 'Bitbucket',
            'scm-manager': 'Generic',
            'gitea': 'Generic',
            'generic': 'Generic'
        };
        const accountType = accountTypeMap[this.detectedProvider];
        this.isLoadingAccounts = true;
        this.gitService.getUsernamesByType(accountType).subscribe({
            next: (accounts) => {
                this.availableAccounts = accounts;
                this.filteredAccounts = [...accounts];
                this.isLoadingAccounts = false;
                console.log(`[ModernClone] Loaded ${accounts.length} accounts for ${accountType}`);
            },
            error: (err) => {
                console.error('[ModernClone] Error loading accounts:', err);
                this.availableAccounts = [];
                this.filteredAccounts = [];
                this.isLoadingAccounts = false;
            }
        });
    }
    /**
     * Filters accounts based on user input
     */
    filterAccounts(value) {
        if (!value) {
            this.filteredAccounts = [...this.availableAccounts];
            return;
        }
        const filterValue = value.toLowerCase();
        this.filteredAccounts = this.availableAccounts.filter(account => account.username.toLowerCase().includes(filterValue));
    }
    /**
     * Selects an account from the dropdown
     */
    selectAccount(username) {
        this.manualCredentials.username = username;
    }
    /**
     * Display function for autocomplete
     */
    displayAccountFn(value) {
        return value || '';
    }
    /**
     * Deletes an account from the list
     */
    deleteAccount(account, event) {
        event.stopPropagation(); // Prevent dropdown from selecting the item
        const confirmed = confirm(`Vuoi eliminare l'account "${account.username}"?`);
        if (!confirmed)
            return;
        this.gitService.deleteGitAccount(account.id).subscribe({
            next: (response) => {
                if (response.success) {
                    // Remove from local lists
                    this.availableAccounts = this.availableAccounts.filter(a => a.id !== account.id);
                    this.filteredAccounts = this.filteredAccounts.filter(a => a.id !== account.id);
                    // Clear username if it was the deleted one
                    if (this.manualCredentials.username === account.username) {
                        this.manualCredentials.username = '';
                    }
                    this.showMessage('Account eliminato con successo');
                }
                else {
                    this.showMessage(response.message || 'Errore nell\'eliminazione dell\'account');
                }
            },
            error: (err) => {
                console.error('[ModernClone] Error deleting account:', err);
                this.showMessage('Errore nell\'eliminazione dell\'account');
            }
        });
    }
    openFileSystem() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_6__["ShowFileMetadata"]();
        data.start = null;
        data.title = "Select Clone Destination";
        data.typeOfSelection = "Folders";
        data.buttonText = "Select folder";
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_2__["ShowFileSystemComponent"], {
            width: '800px',
            height: '600px',
            panelClass: 'resizable-dialog-container',
            data: data
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result === null || result === void 0 ? void 0 : result.data) {
                // Add repository name to the path
                const repoName = this.extractRepoName(this.cloneRequest.url);
                if (repoName) {
                    this.cloneRequest.localPath = `${result.data}\\${repoName}`;
                }
                else {
                    this.cloneRequest.localPath = result.data;
                }
            }
        });
    }
    extractRepoName(url) {
        var _a, _b;
        if (!url)
            return '';
        // Handle various Git URL formats
        let repoName = url;
        // Remove .git extension if present
        repoName = repoName.replace(/\.git$/, '');
        // Extract from HTTPS URL: https://github.com/user/repo
        if (repoName.includes('github.com/')) {
            const parts = (_a = repoName.split('github.com/')[1]) === null || _a === void 0 ? void 0 : _a.split('/');
            if (parts && parts.length >= 2) {
                return parts[1];
            }
        }
        // Extract from SSH URL: git@github.com:user/repo
        if (repoName.includes('git@github.com:')) {
            const parts = (_b = repoName.split(':')[1]) === null || _b === void 0 ? void 0 : _b.split('/');
            if (parts && parts.length >= 2) {
                return parts[1];
            }
        }
        // Fallback: get last part of URL
        const parts = repoName.split('/');
        return parts[parts.length - 1] || 'repository';
    }
    performClone() {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            if (!this.cloneRequest.url || !this.cloneRequest.localPath) {
                this.showMessage('Please fill in all required fields');
                return;
            }
            const info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_3__["WaitingDialogInfo"]();
            this.isValidatingUrl = true;
            try {
                // Step 1: Validate URL reachability before cloning
                // Skip validation for Basic Auth providers (SCM-Manager, Gitea, Generic)
                // because they require credentials even for ls-remote
                const skipValidation = this.authType === 'basic';
                if (!skipValidation) {
                    info.message = "Validating repository URL...";
                    this.waitingDialog.showMessageBox(info);
                    // Validate URL first (only for OAuth providers)
                    const validationResult = yield this.gitService.validateRemoteUrl(this.cloneRequest.url).toPromise();
                    if (!(validationResult === null || validationResult === void 0 ? void 0 : validationResult.isReachable)) {
                        this.waitingDialog.closeMessageBox();
                        this.isValidatingUrl = false;
                        this.urlValidationResult = { isValid: false, error: (validationResult === null || validationResult === void 0 ? void 0 : validationResult.error) || 'Repository not reachable' };
                        if (validationResult === null || validationResult === void 0 ? void 0 : validationResult.isAuthenticationError) {
                            this.showMessage('Authentication required. Please check your credentials.');
                        }
                        else {
                            this.showMessage(`Repository URL not reachable: ${(validationResult === null || validationResult === void 0 ? void 0 : validationResult.error) || 'Unknown error'}`);
                        }
                        return;
                    }
                    this.urlValidationResult = { isValid: true };
                    console.log('[ModernClone] URL validation passed, proceeding with clone');
                }
                else {
                    console.log('[ModernClone] Skipping URL validation for Basic Auth provider, proceeding directly with clone');
                    this.waitingDialog.showMessageBox(info);
                }
                // Step 2: Proceed with clone
                const useAutomaticAuth = this.authType === 'oauth' && !this.showCredentialForm;
                if (useAutomaticAuth) {
                    info.message = `Cloning repository... (${this.getProviderDisplayName()} may open browser for authentication)`;
                }
                else {
                    info.message = "Cloning repository...";
                }
                // Determine if we need manual credentials
                const needsManualCredentials = this.showCredentialForm || this.authType === 'basic';
                // Validate manual credentials when required
                if (needsManualCredentials && (!this.manualCredentials.username || !this.manualCredentials.password)) {
                    this.waitingDialog.closeMessageBox();
                    this.isValidatingUrl = false;
                    this.showMessage('Inserisci username e password/token per l\'autenticazione');
                    return;
                }
                // Build clone request based on auth type
                const request = {
                    url: this.cloneRequest.url,
                    localPath: this.cloneRequest.localPath,
                    branchName: this.cloneRequest.branchName || null,
                    // For OAuth providers without manual override, let GCM handle authentication
                    useSavedToken: useAutomaticAuth || (this.isGitHubRepo && this.useSavedToken),
                    // Pass credentials if manual auth is required
                    username: needsManualCredentials ? this.manualCredentials.username : null,
                    password: needsManualCredentials ? this.manualCredentials.password : null
                };
                // Log the request for debugging
                console.log(`[ModernClone] Sending clone request (provider: ${this.detectedProvider}, authType: ${this.authType}, manual: ${needsManualCredentials}):`, request);
                // Call the modern Git service clone method
                this.gitService.modernClone(request).subscribe(response => {
                    this.waitingDialog.closeMessageBox();
                    this.isValidatingUrl = false;
                    if (response.success) {
                        // Set the new project folder
                        this.projectService.setNewFolderProject(this.cloneRequest.localPath);
                        this.showMessage('Repository cloned successfully!');
                        this.dialogRef.close(this.cloneRequest);
                    }
                    else {
                        this.showMessage(response.error || 'Clone failed');
                    }
                }, error => {
                    var _a;
                    this.waitingDialog.closeMessageBox();
                    this.isValidatingUrl = false;
                    this.showMessage(((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || 'Clone failed: ' + error.message);
                });
            }
            catch (error) {
                this.waitingDialog.closeMessageBox();
                this.isValidatingUrl = false;
                this.showMessage('Unexpected error: ' + ((error === null || error === void 0 ? void 0 : error.message) || 'Unknown error'));
            }
        });
    }
    showMessage(message) {
        const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_4__["GitMessagesComponent"], {
            width: '400px',
            data: { message: message }
        });
    }
    openTokenSettings() {
        const dialogRef = this.dialog.open(_git_dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_5__["GitTokenDialogComponent"], {
            width: '500px',
            disableClose: false
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result) {
                // Token was saved successfully, refresh token status
                this.checkGitHubToken();
            }
        });
    }
    cancel() {
        this.dialogRef.close();
    }
    /**
     * Gets a user-friendly display name for the detected provider
     */
    getProviderDisplayName() {
        const names = {
            'github': 'GitHub',
            'gitlab': 'GitLab',
            'azure': 'Azure DevOps',
            'bitbucket': 'Bitbucket',
            'scm-manager': 'SCM-Manager',
            'gitea': 'Gitea',
            'generic': 'Git Server'
        };
        return names[this.detectedProvider] || 'Git Server';
    }
    /**
     * Toggle credential form visibility for OAuth providers
     * (Allows manual override if user wants to enter credentials manually)
     */
    toggleCredentialForm() {
        this.showCredentialForm = !this.showCredentialForm;
        if (this.showCredentialForm) {
            this.authMethod = 'manual';
        }
        else {
            this.authMethod = 'automatic';
        }
    }
}
ModernCloneProjectComponent.ɵfac = function ModernCloneProjectComponent_Factory(t) { return new (t || ModernCloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_9__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_10__["WaitingDialogService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_11__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_12__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MAT_DIALOG_DATA"])); };
ModernCloneProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({ type: ModernCloneProjectComponent, selectors: [["app-modern-clone-project"]], decls: 31, vars: 6, consts: [[1, "modern-clone-dialog"], ["mat-dialog-title", ""], [1, "clone-form"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "https://github.com/user/repository.git", "required", "", 3, "ngModel", "ngModelChange"], [4, "ngIf"], ["class", "auth-section", 4, "ngIf"], ["matInput", "", "placeholder", "Select destination folder", "required", "", "readonly", "", 3, "ngModel", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], ["matInput", "", "placeholder", "main", 3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], [1, "auth-section"], [1, "auth-card"], ["class", "oauth-section", 4, "ngIf"], ["class", "basic-auth-section", 4, "ngIf"], [1, "oauth-section"], ["class", "oauth-info", 4, "ngIf"], ["mat-stroked-button", "", 1, "toggle-manual-btn", 3, "click"], ["class", "saved-token-option", 4, "ngIf"], [1, "oauth-info"], ["color", "primary"], [1, "oauth-text"], [1, "saved-token-option"], [1, "token-header"], [3, "ngModel", "ngModelChange"], ["class", "token-info", 4, "ngIf"], ["class", "token-warning", 4, "ngIf"], [1, "token-info"], [1, "token-details"], ["class", "username", 4, "ngIf"], [1, "token-mask"], [1, "status"], ["mat-icon-button", "", "color", "warn", "matTooltip", "Elimina token", 3, "disabled", "click"], [1, "username"], [1, "token-warning"], ["color", "warn"], [1, "basic-auth-section"], ["class", "basic-auth-info", 4, "ngIf"], ["matInput", "", "placeholder", "Seleziona o inserisci nuovo username", 3, "ngModel", "matAutocomplete", "ngModelChange"], ["matSuffix", "", "diameter", "20", 4, "ngIf"], [3, "displayWith", "optionSelected"], ["accountAutocomplete", "matAutocomplete"], [3, "value", 4, "ngFor", "ngForOf"], ["disabled", "", 4, "ngIf"], ["matInput", "", "type", "password", 3, "ngModel", "ngModelChange"], [1, "basic-auth-info"], ["color", "accent"], ["matSuffix", "", "diameter", "20"], [3, "value"], [1, "account-option"], [1, "account-username"], ["mat-icon-button", "", "matTooltip", "Elimina account", 1, "delete-account-btn", 3, "click"], ["disabled", ""]], template: function ModernCloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "h2", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "Clone Repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7, "Repository URL");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "input", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_8_listener($event) { return ctx.cloneRequest.url = $event; })("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_8_listener() { return ctx.onUrlChange(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, ModernCloneProjectComponent_mat_hint_9_Template, 4, 3, "mat-hint", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, ModernCloneProjectComponent_div_10_Template, 10, 3, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](12, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](13, "Clone to");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](14, "input", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_14_listener($event) { return ctx.cloneRequest.localPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](15, "button", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_15_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](16, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](17, "folder_open");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](18, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "The repository will be cloned to this location");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](20, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](21, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](22, "Branch (optional)");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](23, "input", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_23_listener($event) { return ctx.cloneRequest.branchName = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](24, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](25, "Leave empty to clone the default branch");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](26, "mat-dialog-actions", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](27, "button", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_27_listener() { return ctx.cancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](28, "Cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](29, "button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_29_listener() { return ctx.performClone(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](30, " Clone Repository ");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.cloneRequest.url);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.cloneRequest.url && ctx.detectedProvider !== "generic");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.cloneRequest.url);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.cloneRequest.localPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.cloneRequest.branchName);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", !ctx.cloneRequest.url || !ctx.cloneRequest.localPath);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_13__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_13__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_14__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_15__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_15__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_15__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_15__["NgModel"], _angular_common__WEBPACK_IMPORTED_MODULE_16__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_17__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_13__["MatSuffix"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_13__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogActions"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_19__["MatCardContent"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_20__["MatCheckbox"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_21__["MatTooltip"], _angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_22__["MatAutocompleteTrigger"], _angular_material_autocomplete__WEBPACK_IMPORTED_MODULE_22__["MatAutocomplete"], _angular_common__WEBPACK_IMPORTED_MODULE_16__["NgForOf"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_23__["MatSpinner"], _angular_material_core__WEBPACK_IMPORTED_MODULE_24__["MatOption"]], styles: [".modern-clone-dialog[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 700px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-content[_ngcontent-%COMP%] {\n  padding: 20px;\n  max-height: 70vh;\n  overflow-y: auto;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .clone-form[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%] {\n  margin: 8px 0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%] {\n  background-color: #f5f5f5;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%] {\n  padding-bottom: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 16px;\n  font-weight: 500;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  padding-top: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .oauth-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: rgba(33, 150, 243, 0.1);\n  border-radius: 8px;\n  margin-bottom: 16px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .oauth-info[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 32px;\n  width: 32px;\n  height: 32px;\n  flex-shrink: 0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .oauth-info[_ngcontent-%COMP%]   .oauth-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .oauth-info[_ngcontent-%COMP%]   .oauth-text[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 4px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .oauth-info[_ngcontent-%COMP%]   .oauth-text[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.9em;\n  color: #666;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .toggle-manual-btn[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .oauth-section[_ngcontent-%COMP%]   .toggle-manual-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .basic-auth-section[_ngcontent-%COMP%]   .basic-auth-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px;\n  background-color: rgba(156, 39, 176, 0.1);\n  border-radius: 8px;\n  margin-bottom: 16px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .basic-auth-section[_ngcontent-%COMP%]   .basic-auth-info[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .basic-auth-section[_ngcontent-%COMP%]   .basic-auth-info[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 8px;\n  border-radius: 4px;\n  background-color: rgba(33, 150, 243, 0.1);\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status.warn[_ngcontent-%COMP%] {\n  background-color: rgba(255, 152, 0, 0.1);\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-header[_ngcontent-%COMP%] {\n  margin-bottom: 12px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-header[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px;\n  background-color: rgba(33, 150, 243, 0.1);\n  border-radius: 8px;\n  margin-bottom: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]    > mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]    > mat-icon.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]    > mat-icon.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 1em;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  color: #666;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .token-mask[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n  color: #666;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-warning[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 8px;\n  padding: 8px 12px;\n  background-color: rgba(255, 152, 0, 0.1);\n  border-radius: 4px;\n  margin-bottom: 12px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  flex-shrink: 0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .saved-token-option[_ngcontent-%COMP%]   .token-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n  color: #5d4037;\n  line-height: 1.4;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .manual-credentials[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  padding-top: 16px;\n  border-top: 1px dashed #ccc;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .manual-credentials[_ngcontent-%COMP%]   .manual-hint[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #666;\n  margin-bottom: 12px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  border-top: 1px solid #e0e0e0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-left: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-form-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 1.25em;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-hint[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #666;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .account-option[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  width: 100%;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .account-option[_ngcontent-%COMP%]   .account-username[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .account-option[_ngcontent-%COMP%]   .delete-account-btn[_ngcontent-%COMP%] {\n  width: 32px;\n  height: 32px;\n  line-height: 32px;\n  margin-left: 8px;\n  opacity: 0.6;\n  transition: opacity 0.2s;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .account-option[_ngcontent-%COMP%]   .delete-account-btn[_ngcontent-%COMP%]:hover {\n  opacity: 1;\n  color: #f44336;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .account-option[_ngcontent-%COMP%]   .delete-account-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbW9kZXJuLWNsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0Y7QUFDRTtFQUNFLGFBQUE7RUFDQSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0o7QUFFRTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7QUFBSjtBQUdFO0VBQ0UsV0FBQTtBQURKO0FBSUU7RUFDRSxhQUFBO0FBRko7QUFJSTtFQUNFLHlCQUFBO0FBRk47QUFJTTtFQUNFLG1CQUFBO0FBRlI7QUFJUTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFGVjtBQUlVO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBRlo7QUFPTTtFQUNFLGdCQUFBO0FBTFI7QUFVTTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBUlI7QUFVUTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUFSVjtBQVdRO0VBQ0UsT0FBQTtBQVRWO0FBV1U7RUFDRSxjQUFBO0VBQ0Esa0JBQUE7QUFUWjtBQVlVO0VBQ0UsU0FBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtBQVZaO0FBZU07RUFDRSxXQUFBO0VBQ0EsbUJBQUE7QUFiUjtBQWVRO0VBQ0UsaUJBQUE7QUFiVjtBQW1CTTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBakJSO0FBbUJRO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBakJWO0FBb0JRO0VBQ0UsZ0JBQUE7QUFsQlY7QUF3QkU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EseUNBQUE7QUF0Qko7QUF3Qkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUF0Qk47QUF5Qkk7RUFDRSx3Q0FBQTtBQXZCTjtBQTBCSTtFQUNFLGlCQUFBO0FBeEJOO0FBNkJJO0VBQ0UsbUJBQUE7QUEzQk47QUE2Qk07RUFDRSxnQkFBQTtBQTNCUjtBQStCSTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0FBN0JOO0FBK0JNO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBN0JSO0FBK0JRO0VBQ0UsY0FBQTtBQTdCVjtBQWdDUTtFQUNFLGNBQUE7QUE5QlY7QUFrQ007RUFDRSxPQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsUUFBQTtBQWhDUjtBQWtDUTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxjQUFBO0FBaENWO0FBa0NVO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsV0FBQTtBQWhDWjtBQW9DUTtFQUNFLGlCQUFBO0VBQ0EsV0FBQTtBQWxDVjtBQXFDUTtFQUNFLGlCQUFBO0FBbkNWO0FBcUNVO0VBQ0UsY0FBQTtBQW5DWjtBQXNDVTtFQUNFLGNBQUE7QUFwQ1o7QUEwQ0k7RUFDRSxhQUFBO0VBQ0EsdUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7RUFDQSx3Q0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUF4Q047QUEwQ007RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxjQUFBO0FBeENSO0FBMkNNO0VBQ0UsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUF6Q1I7QUE4Q0U7RUFDRSxnQkFBQTtFQUNBLGlCQUFBO0VBQ0EsMkJBQUE7QUE1Q0o7QUE4Q0k7RUFDRSxnQkFBQTtFQUNBLFdBQUE7RUFDQSxtQkFBQTtBQTVDTjtBQWdERTtFQUNFLGtCQUFBO0VBQ0EsNkJBQUE7QUE5Q0o7QUFnREk7RUFDRSxnQkFBQTtBQTlDTjtBQW1ESTtFQUNFLHNCQUFBO0FBakROO0FBcURFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7QUFuREo7QUF1REU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLFdBQUE7QUFyREo7QUF1REk7RUFDRSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLG1CQUFBO0FBckROO0FBd0RJO0VBQ0UsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsWUFBQTtFQUNBLHdCQUFBO0FBdEROO0FBd0RNO0VBQ0UsVUFBQTtFQUNBLGNBQUE7QUF0RFI7QUF5RE07RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUF2RFIiLCJmaWxlIjoibW9kZXJuLWNsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIubW9kZXJuLWNsb25lLWRpYWxvZyB7XHJcbiAgbWluLXdpZHRoOiA1MDBweDtcclxuICBtYXgtd2lkdGg6IDcwMHB4O1xyXG5cclxuICBtYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gICAgcGFkZGluZzogMjBweDtcclxuICAgIG1heC1oZWlnaHQ6IDcwdmg7XHJcbiAgICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIH1cclxuXHJcbiAgLmNsb25lLWZvcm0ge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICBnYXA6IDE2cHg7XHJcbiAgfVxyXG5cclxuICAuZnVsbC13aWR0aCB7XHJcbiAgICB3aWR0aDogMTAwJTtcclxuICB9XHJcblxyXG4gIC5hdXRoLXNlY3Rpb24ge1xyXG4gICAgbWFyZ2luOiA4cHggMDtcclxuXHJcbiAgICAuYXV0aC1jYXJkIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuXHJcbiAgICAgIG1hdC1jYXJkLWhlYWRlciB7XHJcbiAgICAgICAgcGFkZGluZy1ib3R0b206IDhweDtcclxuXHJcbiAgICAgICAgbWF0LWNhcmQtdGl0bGUge1xyXG4gICAgICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICAgICAgICBnYXA6IDhweDtcclxuICAgICAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcblxyXG4gICAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgICAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgICAgICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcblxyXG4gICAgICBtYXQtY2FyZC1jb250ZW50IHtcclxuICAgICAgICBwYWRkaW5nLXRvcDogOHB4O1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLm9hdXRoLXNlY3Rpb24ge1xyXG4gICAgICAub2F1dGgtaW5mbyB7XHJcbiAgICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICAgICAgICBnYXA6IDEycHg7XHJcbiAgICAgICAgcGFkZGluZzogMTZweDtcclxuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDMzLCAxNTAsIDI0MywgMC4xKTtcclxuICAgICAgICBib3JkZXItcmFkaXVzOiA4cHg7XHJcbiAgICAgICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuXHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgZm9udC1zaXplOiAzMnB4O1xyXG4gICAgICAgICAgd2lkdGg6IDMycHg7XHJcbiAgICAgICAgICBoZWlnaHQ6IDMycHg7XHJcbiAgICAgICAgICBmbGV4LXNocmluazogMDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIC5vYXV0aC10ZXh0IHtcclxuICAgICAgICAgIGZsZXg6IDE7XHJcblxyXG4gICAgICAgICAgc3Ryb25nIHtcclxuICAgICAgICAgICAgZGlzcGxheTogYmxvY2s7XHJcbiAgICAgICAgICAgIG1hcmdpbi1ib3R0b206IDRweDtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICBwIHtcclxuICAgICAgICAgICAgbWFyZ2luOiAwO1xyXG4gICAgICAgICAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gICAgICAgICAgICBjb2xvcjogIzY2NjtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC50b2dnbGUtbWFudWFsLWJ0biB7XHJcbiAgICAgICAgd2lkdGg6IDEwMCU7XHJcbiAgICAgICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuXHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLmJhc2ljLWF1dGgtc2VjdGlvbiB7XHJcbiAgICAgIC5iYXNpYy1hdXRoLWluZm8ge1xyXG4gICAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgICBnYXA6IDhweDtcclxuICAgICAgICBwYWRkaW5nOiAxMnB4O1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMTU2LCAzOSwgMTc2LCAwLjEpO1xyXG4gICAgICAgIGJvcmRlci1yYWRpdXM6IDhweDtcclxuICAgICAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG5cclxuICAgICAgICBtYXQtaWNvbiB7XHJcbiAgICAgICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgICAgICB3aWR0aDogMjBweDtcclxuICAgICAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHNwYW4ge1xyXG4gICAgICAgICAgZm9udC1zaXplOiAwLjllbTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcblxyXG4gIC50b2tlbi1zdGF0dXMge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIHBhZGRpbmc6IDhweDtcclxuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMzMsIDE1MCwgMjQzLCAwLjEpO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICB3aWR0aDogMjBweDtcclxuICAgICAgaGVpZ2h0OiAyMHB4O1xyXG4gICAgfVxyXG5cclxuICAgICYud2FybiB7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMjU1LCAxNTIsIDAsIDAuMSk7XHJcbiAgICB9XHJcblxyXG4gICAgYnV0dG9uIHtcclxuICAgICAgbWFyZ2luLWxlZnQ6IGF1dG87XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuc2F2ZWQtdG9rZW4tb3B0aW9uIHtcclxuICAgIC50b2tlbi1oZWFkZXIge1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiAxMnB4O1xyXG5cclxuICAgICAgbWF0LWNoZWNrYm94IHtcclxuICAgICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLnRva2VuLWluZm8ge1xyXG4gICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICBnYXA6IDEycHg7XHJcbiAgICAgIHBhZGRpbmc6IDEycHg7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMzMsIDE1MCwgMjQzLCAwLjEpO1xyXG4gICAgICBib3JkZXItcmFkaXVzOiA4cHg7XHJcbiAgICAgIG1hcmdpbi1ib3R0b206IDhweDtcclxuXHJcbiAgICAgID4gbWF0LWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMjRweDtcclxuICAgICAgICB3aWR0aDogMjRweDtcclxuICAgICAgICBoZWlnaHQ6IDI0cHg7XHJcblxyXG4gICAgICAgICYudmFsaWQge1xyXG4gICAgICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAmLmludmFsaWQge1xyXG4gICAgICAgICAgY29sb3I6ICNmNDQzMzY7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcblxyXG4gICAgICAudG9rZW4tZGV0YWlscyB7XHJcbiAgICAgICAgZmxleDogMTtcclxuICAgICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICAgICAgZ2FwOiA0cHg7XHJcblxyXG4gICAgICAgIC51c2VybmFtZSB7XHJcbiAgICAgICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgICAgIGdhcDogNHB4O1xyXG4gICAgICAgICAgZm9udC1zaXplOiAxZW07XHJcblxyXG4gICAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgICAgICAgIGhlaWdodDogMThweDtcclxuICAgICAgICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgICAgICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLnRva2VuLW1hc2sge1xyXG4gICAgICAgICAgZm9udC1zaXplOiAwLjg1ZW07XHJcbiAgICAgICAgICBjb2xvcjogIzY2NjtcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIC5zdGF0dXMge1xyXG4gICAgICAgICAgZm9udC1zaXplOiAwLjg1ZW07XHJcblxyXG4gICAgICAgICAgJi52YWxpZCB7XHJcbiAgICAgICAgICAgIGNvbG9yOiAjNGNhZjUwO1xyXG4gICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICYuaW52YWxpZCB7XHJcbiAgICAgICAgICAgIGNvbG9yOiAjZjQ0MzM2O1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIC50b2tlbi13YXJuaW5nIHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGZsZXgtc3RhcnQ7XHJcbiAgICAgIGdhcDogOHB4O1xyXG4gICAgICBwYWRkaW5nOiA4cHggMTJweDtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNTUsIDE1MiwgMCwgMC4xKTtcclxuICAgICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiAxMnB4O1xyXG5cclxuICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgICAgZmxleC1zaHJpbms6IDA7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIHNwYW4ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMC44NWVtO1xyXG4gICAgICAgIGNvbG9yOiAjNWQ0MDM3O1xyXG4gICAgICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5tYW51YWwtY3JlZGVudGlhbHMge1xyXG4gICAgbWFyZ2luLXRvcDogMTZweDtcclxuICAgIHBhZGRpbmctdG9wOiAxNnB4O1xyXG4gICAgYm9yZGVyLXRvcDogMXB4IGRhc2hlZCAjY2NjO1xyXG5cclxuICAgIC5tYW51YWwtaGludCB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiAxMnB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgbWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICAgIHBhZGRpbmc6IDE2cHggMjRweDtcclxuICAgIGJvcmRlci10b3A6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICAgIGJ1dHRvbiB7XHJcbiAgICAgIG1hcmdpbi1sZWZ0OiA4cHg7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBtYXQtZm9ybS1maWVsZCB7XHJcbiAgICAubWF0LWZvcm0tZmllbGQtd3JhcHBlciB7XHJcbiAgICAgIHBhZGRpbmctYm90dG9tOiAxLjI1ZW07XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBtYXQtaGludCB7XHJcbiAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICB9XHJcblxyXG4gIC8vIEFjY291bnQgc2VsZWN0b3IgZHJvcGRvd24gc3R5bGVzXHJcbiAgLmFjY291bnQtb3B0aW9uIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xyXG4gICAgd2lkdGg6IDEwMCU7XHJcblxyXG4gICAgLmFjY291bnQtdXNlcm5hbWUge1xyXG4gICAgICBmbGV4OiAxO1xyXG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcclxuICAgIH1cclxuXHJcbiAgICAuZGVsZXRlLWFjY291bnQtYnRuIHtcclxuICAgICAgd2lkdGg6IDMycHg7XHJcbiAgICAgIGhlaWdodDogMzJweDtcclxuICAgICAgbGluZS1oZWlnaHQ6IDMycHg7XHJcbiAgICAgIG1hcmdpbi1sZWZ0OiA4cHg7XHJcbiAgICAgIG9wYWNpdHk6IDAuNjtcclxuICAgICAgdHJhbnNpdGlvbjogb3BhY2l0eSAwLjJzO1xyXG5cclxuICAgICAgJjpob3ZlciB7XHJcbiAgICAgICAgb3BhY2l0eTogMTtcclxuICAgICAgICBjb2xvcjogI2Y0NDMzNjtcclxuICAgICAgfVxyXG5cclxuICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgICB3aWR0aDogMThweDtcclxuICAgICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcbn0iXX0= */"] });


/***/ }),

/***/ "zn8P":
/*!******************************************************!*\
  !*** ./$$_lazy_route_resource lazy namespace object ***!
  \******************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

function webpackEmptyAsyncContext(req) {
	// Here Promise.resolve().then() is used instead of new Promise() to prevent
	// uncaught exception popping up in devtools
	return Promise.resolve().then(function() {
		var e = new Error("Cannot find module '" + req + "'");
		e.code = 'MODULE_NOT_FOUND';
		throw e;
	});
}
webpackEmptyAsyncContext.keys = function() { return []; };
webpackEmptyAsyncContext.resolve = webpackEmptyAsyncContext;
module.exports = webpackEmptyAsyncContext;
webpackEmptyAsyncContext.id = "zn8P";

/***/ })

},[[0,"runtime","vendor"]]]);
//# sourceMappingURL=main.js.map