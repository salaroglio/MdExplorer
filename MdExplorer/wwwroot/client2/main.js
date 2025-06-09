(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["main"],{

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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../signalR/dialogs/parsing-project/parsing-project.provider */ "YG1a");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../signalR/dialogs/plantuml-working/plantuml-working.provider */ "CqLH");
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../signalR/dialogs/connection-lost/connection-lost.provider */ "jX2R");
/* harmony import */ var _dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../dialogs/opening-application/opening-application.provider */ "l94Z");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../git/services/gitservice.service */ "N73s");







class MdServerMessagesService {
    constructor(parsingProjectProvider, plantumlWorkingProvider, connectionLostProvider, openingApplicationProvider, gitService) {
        this.parsingProjectProvider = parsingProjectProvider;
        this.plantumlWorkingProvider = plantumlWorkingProvider;
        this.connectionLostProvider = connectionLostProvider;
        this.openingApplicationProvider = openingApplicationProvider;
        this.gitService = gitService;
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
                this.hubConnection
                    .start()
                    .then(() => {
                    console.log('Connection started');
                    this.connectionIsLost = false;
                    this.getCurrentConnectionId(this);
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
            this.gitService.getCurrentBranch();
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
    getCurrentConnectionId(objectThis) {
        this.hubConnection.invoke('GetConnectionId')
            .then(function (connectionId) {
            objectThis.connectionId = connectionId;
        });
    }
}
MdServerMessagesService.ɵfac = function MdServerMessagesService_Factory(t) { return new (t || MdServerMessagesService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_2__["ParsingProjectProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_3__["PlantumlWorkingProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_4__["ConnectionLostProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_5__["OpeningApplicationProvider"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_6__["GITService"])); };
MdServerMessagesService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: MdServerMessagesService, factory: MdServerMessagesService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ 0:
/*!***************************!*\
  !*** multi ./src/main.ts ***!
  \***************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

module.exports = __webpack_require__(/*! C:\sviluppo\mdExplorer\MdExplorer\client2\src\main.ts */"zUnb");


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
        this.gitservice.getCurrentBranch();
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
    production: false
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
        this.useModernGit = true; // Match toolbar component setting
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
        console.log('Git service project path set to:', path);
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
        console.log('🔄 Smart Git polling initialized');
    }
    /**
     * Perform polling based on current configuration
     */
    performPoll() {
        if (this.useModernGit && this.currentProjectPath) {
            // Use modern endpoints with SSH support
            console.log('🔄 Performing modern Git poll for:', this.currentProjectPath);
            // Get branch status
            this.modernGetBranchStatus(this.currentProjectPath).subscribe(branch => {
                this.currentBranch$.next(branch);
            }, error => {
                console.error('Error in modern branch status:', error);
                // Fallback to legacy
                this.getCurrentBranch();
            });
            // Get pull/push data
            this.modernGetDataToPull(this.currentProjectPath).subscribe(pullData => {
                this.commmitsToPull$.next(pullData);
            }, error => {
                console.error('Error in modern data to pull:', error);
            });
        }
        else {
            // Use legacy endpoints
            this.getCurrentBranch();
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
        console.log('🟢 Window focused - activating frequent Git polling (60s)');
        this.startPolling(this.ACTIVE_POLLING_INTERVAL);
        // Polling immediato quando torna in focus
        this.performPoll();
    }
    /**
     * Quando la finestra diventa inattiva: polling meno frequente
     */
    handleWindowBlur() {
        console.log('🟡 Window blurred - reducing Git polling frequency (5min)');
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
            console.log('🔴 Git polling stopped');
        }
    }
    clone(request) {
        const url = '../api/gitfeatures/cloneRepository';
        return this.http.post(url, request);
    }
    getCurrentBranch() {
        const url = '../api/gitservice/branches/feat/getcurrentbranch';
        let data$ = this.http.get(url);
        data$.subscribe(_ => {
            this.currentBranch$.next(_);
        });
        const url2 = '../api/gitservice/branches/feat/getdatatopull';
        let data2$ = this.http.get(url2);
        data2$.subscribe(_ => {
            this.commmitsToPull$.next(_);
        });
        return data$;
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
    pull(request) {
        const url = '../api/gitfeatures/pull';
        return this.http.post(url, request);
        //return this.http.get<any>(url);
    }
    commitAndPush(request) {
        const url = '../api/gitfeatures/commitandpush';
        return this.http.post(url, request);
    }
    commit(request) {
        const url = '../api/gitfeatures/commit';
        return this.http.post(url, request);
    }
    push(request) {
        const url = '../api/gitfeatures/push';
        return this.http.post(url, request);
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
        const url = '../api/ModernGitToolbar/push';
        return this.http.post(url, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => this.adaptModernResponseToLegacy(response)));
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
    /**
     * Cleanup quando il service viene distrutto
     */
    ngOnDestroy() {
        this.stopPolling();
        // Rimuovi event listeners per evitare memory leak
        document.removeEventListener('visibilitychange', this.handleVisibilityChange);
        window.removeEventListener('focus', this.handleWindowFocus);
        window.removeEventListener('blur', this.handleWindowBlur);
        console.log('🧹 Git service cleanup completed');
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
/* harmony import */ var _Models_MdSettings__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../Models/MdSettings */ "WJkQ");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ "tk/3");




class AppCurrentMetadataService {
    constructor(http) {
        this.http = http;
        this.dataStore = {
            folderName: 'test', settings: [new _Models_MdSettings__WEBPACK_IMPORTED_MODULE_1__["MdSetting"]({ id: 'test', name: 'PlantumlServer' })]
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
            this.dataStore.settings = data;
            this._Settings.next(Object.assign({}, this.dataStore).settings);
        }, error => {
        });
    }
    saveSettings() {
        const url = '../api/AppSettings/SetSettings';
        var test = { settings: this.dataStore.settings };
        return this.http.post(url, this.dataStore.settings);
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





class AppComponent {
    constructor(titleService, currentFolder, route, router) {
        this.titleService = titleService;
        this.currentFolder = currentFolder;
        this.route = route;
        this.router = router;
        this.title = 'client2';
        currentFolder.folderName.subscribe((data) => {
            this.titleService.setTitle(data.currentFolder);
        });
        currentFolder.loadFolderName();
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
AppComponent.ɵfac = function AppComponent_Factory(t) { return new (t || AppComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__["Title"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_3__["AppCurrentMetadataService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["ActivatedRoute"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["Router"])); };
AppComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: AppComponent, selectors: [["app-root"]], hostBindings: function AppComponent_HostBindings(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("unload", function AppComponent_unload_HostBindingHandler($event) { return ctx.unloadHandler($event); }, false, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresolveWindow"]);
    } }, decls: 3, vars: 1, consts: [[1, "container"], ["o", "outlet"]], template: function AppComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "router-outlet", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("@routeAnimations", _r0.isActivated ? _r0.activatedRoute : "");
    } }, directives: [_angular_router__WEBPACK_IMPORTED_MODULE_4__["RouterOutlet"]], styles: [".flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-flow: row wrap;\n  flex-direction: row;\n  flex-wrap: wrap;\n}\n\n.flex-item[_ngcontent-%COMP%] {\n  background: tomato;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcYXBwLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsbUJBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxrQkFBQTtBQUNGIiwiZmlsZSI6ImFwcC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5mbGV4LWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWZsb3c6IHJvdyB3cmFwO1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbiAgZmxleC13cmFwOiB3cmFwOyAgXHJcbn1cclxuXHJcbi5mbGV4LWl0ZW0ge1xyXG4gIGJhY2tncm91bmQ6IHRvbWF0bztcclxufVxyXG5cclxuIl19 */"], data: { animation: [
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
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../md-explorer/models/md-file */ "aS6m");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");










class NewDirectoryComponent {
    constructor(data, dialogRef, mdFileService) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
    }
    ngOnInit() {
    }
    save() {
        this.mdFileService.CreateNewDirectoryEx(this.data.fullPath, this.directoryName, this.data.level)
            .subscribe(data => {
            this.dialogRef.close(data);
        });
    }
    dismiss() {
        this.dialogRef.close();
    }
}
NewDirectoryComponent.ɵfac = function NewDirectoryComponent_Factory(t) { return new (t || NewDirectoryComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
NewDirectoryComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: NewDirectoryComponent, selectors: [["app-new-directory"]], decls: 19, vars: 1, consts: [["mat-dialog-title", ""], [1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "directoryName", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-button", "", "color", "primary", 3, "click"]], template: function NewDirectoryComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Create new folder\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-form-field", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Directory Name");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "input", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewDirectoryComponent_Template_input_ngModelChange_7_listener($event) { return ctx.directoryName = $event; })("keyup.enter", function NewDirectoryComponent_Template_input_keyup_enter_7_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_11_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](14, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_15_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.directoryName);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJuZXctZGlyZWN0b3J5LmNvbXBvbmVudC5zY3NzIn0= */"] });


/***/ }),

/***/ "WJkQ":
/*!**************************************!*\
  !*** ./src/app/Models/MdSettings.ts ***!
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
/* harmony import */ var _app_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./app.component */ "Sy1n");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./shared/material.module */ "5dmV");
/* harmony import */ var _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/platform-browser/animations */ "R1ws");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./signalR/dialogs/parsing-project/parsing-project.provider */ "YG1a");
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./signalR/dialogs/connection-lost/connection-lost.provider */ "jX2R");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./signalR/dialogs/plantuml-working/plantuml-working.component */ "ggj0");
/* harmony import */ var _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./signalR/dialogs/plantuml-working/plantuml-working.provider */ "CqLH");
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./commons/waitingdialog/waiting-dialog/waiting-dialog.component */ "S2pp");
/* harmony import */ var _commons_components_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./commons/components/new-directory/new-directory.component */ "TUMs");
/* harmony import */ var _signalR_dialogs_opening_application_opening_application_component__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./signalR/dialogs/opening-application/opening-application.component */ "mLxA");
/* harmony import */ var _signalR_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./signalR/dialogs/opening-application/opening-application.provider */ "l94Z");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/core */ "fXoL");





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
AppModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdefineNgModule"]({ type: AppModule, bootstrap: [_app_component__WEBPACK_IMPORTED_MODULE_5__["AppComponent"]] });
AppModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdefineInjector"]({ providers: [_signalR_dialogs_parsing_project_parsing_project_provider__WEBPACK_IMPORTED_MODULE_8__["ParsingProjectProvider"],
        _signalR_dialogs_connection_lost_connection_lost_provider__WEBPACK_IMPORTED_MODULE_9__["ConnectionLostProvider"],
        _signalR_dialogs_plantuml_working_plantuml_working_provider__WEBPACK_IMPORTED_MODULE_11__["PlantumlWorkingProvider"],
        _signalR_dialogs_opening_application_opening_application_provider__WEBPACK_IMPORTED_MODULE_16__["OpeningApplicationProvider"]], imports: [[
            _angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["BrowserModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"].forRoot(routes),
            _angular_flex_layout__WEBPACK_IMPORTED_MODULE_2__["FlexLayoutModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_3__["FormsModule"],
            _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_7__["BrowserAnimationsModule"],
            _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClientModule"],
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵsetNgModuleScope"](AppModule, { declarations: [_app_component__WEBPACK_IMPORTED_MODULE_5__["AppComponent"],
        _signalR_dialogs_plantuml_working_plantuml_working_component__WEBPACK_IMPORTED_MODULE_10__["PlantumlWorkingComponent"],
        _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_12__["ShowFileSystemComponent"],
        _commons_waitingdialog_waiting_dialog_waiting_dialog_component__WEBPACK_IMPORTED_MODULE_13__["WaitingDialogComponent"],
        _commons_components_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_14__["NewDirectoryComponent"],
        _signalR_dialogs_opening_application_opening_application_component__WEBPACK_IMPORTED_MODULE_15__["OpeningApplicationComponent"]], imports: [_angular_platform_browser__WEBPACK_IMPORTED_MODULE_0__["BrowserModule"], _angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"], //
        _angular_flex_layout__WEBPACK_IMPORTED_MODULE_2__["FlexLayoutModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_3__["FormsModule"],
        _angular_platform_browser_animations__WEBPACK_IMPORTED_MODULE_7__["BrowserAnimationsModule"],
        _angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClientModule"]] }); })();


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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ "+dpY");





class MdFileService {
    constructor(http, mdServerMessages) {
        this.http = http;
        this.mdServerMessages = mdServerMessages;
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
        const url = '../api/mdFiles/moveMdFile';
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
        const url = '../api/mdfiles/GetShallowStructure?connectionId=' + this.mdServerMessages.connectionId;
        return this.http.get(url);
    }
    loadAll(callback, objectThis) {
        const url = '../api/mdfiles/GetShallowStructure?connectionId=' + this.mdServerMessages.connectionId;
        return this.http.get(url)
            .subscribe(data => {
            // Assicuriamo che tutte le proprietà siano definite fin dall'inizio
            this.initializeIndexingProperties(data);
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
    openFolderOnFileExplorer(file) {
        const url = '../api/mdfiles/OpenFolderOnFileExplorer';
        return this.http.post(url, file);
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
        this._selectedMdFileFromSideNav.next(selectedFile);
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
}
MdFileService.ɵfac = function MdFileService_Factory(t) { return new (t || MdFileService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__["MdServerMessagesService"])); };
MdFileService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: MdFileService, factory: MdFileService.ɵfac, providedIn: 'root' });


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
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/cdk/tree */ "FvrZ");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../new-directory/new-directory.component */ "TUMs");
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../md-explorer/models/md-file */ "aS6m");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _show_file_metadata__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./show-file-metadata */ "DEjE");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");





















function ShowFileSystemComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "button", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r8); const item_r6 = ctx.item; const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r7.createDirectoryOn(item_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "button", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r8); const item_r6 = ctx.item; const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r9.openFolderOn(item_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "b", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](9, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](10, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "b", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](12, "File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_div_21_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_21_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const folder_r10 = ctx.$implicit; const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r11.navigateToFolder(folder_r10.path); })("keydown.enter", function ShowFileSystemComponent_div_21_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const folder_r10 = ctx.$implicit; const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r13.navigateToFolder(folder_r10.path); })("keydown.space", function ShowFileSystemComponent_div_21_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const folder_r10 = ctx.$implicit; const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r14.navigateToFolder(folder_r10.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const folder_r10 = ctx.$implicit;
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx_r2.currentPath === folder_r10.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵattribute"]("aria-label", "Navigate to " + folder_r10.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](folder_r10.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](folder_r10.name);
} }
function ShowFileSystemComponent_div_26_Template(rf, ctx) { if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_26_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r17); const drive_r15 = ctx.$implicit; const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r16.navigateToFolder(drive_r15.path); })("keydown.enter", function ShowFileSystemComponent_div_26_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r17); const drive_r15 = ctx.$implicit; const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r18.navigateToFolder(drive_r15.path); })("keydown.space", function ShowFileSystemComponent_div_26_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r17); const drive_r15 = ctx.$implicit; const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r19.navigateToFolder(drive_r15.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const drive_r15 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx_r3.currentPath === drive_r15.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵattribute"]("aria-label", "Navigate to drive " + drive_r15.label);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](drive_r15.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate2"]("", drive_r15.label, " (", drive_r15.name, ")");
} }
function ShowFileSystemComponent_div_34_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](1, "mat-spinner", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3, "Loading...");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_div_35_div_1_Template(rf, ctx) { if (rf & 1) {
    const _r24 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_div_35_div_1_Template_div_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r24); const item_r22 = ctx.$implicit; const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r23.onItemClick(item_r22); })("dblclick", function ShowFileSystemComponent_div_35_div_1_Template_div_dblclick_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r24); const item_r22 = ctx.$implicit; const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r25.onItemDoubleClick(item_r22); })("keydown.enter", function ShowFileSystemComponent_div_35_div_1_Template_div_keydown_enter_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r24); const item_r22 = ctx.$implicit; const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r26.onItemDoubleClick(item_r22); })("keydown.space", function ShowFileSystemComponent_div_35_div_1_Template_div_keydown_space_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r24); const item_r22 = ctx.$implicit; const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r27.onItemClick(item_r22); })("contextmenu", function ShowFileSystemComponent_div_35_div_1_Template_div_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r24); const item_r22 = ctx.$implicit; const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2); return ctx_r28.onRightClick($event, item_r22); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const item_r22 = ctx.$implicit;
    const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("selected", ctx_r20.activeNode === item_r22)("not-selectable", !ctx_r20.isItemSelectable(item_r22));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵattribute"]("aria-label", ctx_r20.getItemAriaLabel(item_r22));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", item_r22.type === "folder" ? "folder" : item_r22.type === "file" ? "insert_drive_file" : "text_snippet", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](item_r22.name);
} }
function ShowFileSystemComponent_div_35_div_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r21.searchFilter ? "search_off" : "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r21.searchFilter ? "No files match your filter" : "This folder is empty");
} }
function ShowFileSystemComponent_div_35_Template(rf, ctx) { if (rf & 1) {
    const _r30 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("contextmenu", function ShowFileSystemComponent_div_35_Template_div_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r30); const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r29.onRightClick($event, null); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, ShowFileSystemComponent_div_35_div_1_Template, 5, 7, "div", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](2, ShowFileSystemComponent_div_35_div_2_Template, 5, 2, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r5.filteredItems)("ngForTrackBy", ctx_r5.trackByItem);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r5.filteredItems.length === 0 && !ctx_r5.isLoading);
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
        var md1 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]('12Folder', 'c:primoFolder', 0, true);
        var md2 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]('2Folder', 'c:primoFoldersottoFolder', 1, true);
        var md3 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md4 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md5 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]('5Folder', 'c:cuccu', 3, false);
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
DynamicDatabase.ɵfac = function DynamicDatabase_Factory(t) { return new (t || DynamicDatabase)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"])); };
DynamicDatabase.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineInjectable"]({ token: DynamicDatabase, factory: DynamicDatabase.ɵfac, providedIn: 'root' });
class DynamicDataSource {
    constructor(_treeControl, _database, _mdFileService, baseStart, typeOfSelection) {
        this._treeControl = _treeControl;
        this._database = _database;
        this._mdFileService = _mdFileService;
        this.baseStart = baseStart;
        this.typeOfSelection = typeOfSelection;
        this.dataChange = new rxjs__WEBPACK_IMPORTED_MODULE_3__["BehaviorSubject"]([]);
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
        return Object(rxjs__WEBPACK_IMPORTED_MODULE_3__["merge"])(collectionViewer.viewChange, this.dataChange).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["map"])(() => this.data));
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
    constructor(baseStart, database, dialog, mdFileService, dialogRef) {
        this.baseStart = baseStart;
        this.database = database;
        this.dialog = dialog;
        this.mdFileService = mdFileService;
        this.dialogRef = dialogRef;
        this.title = "Document's Folder";
        // Existing properties for context menu
        this.menuTopLeftPosition = { x: 0, y: 0 };
        // New properties for file explorer
        this.specialFolders = [];
        this.drives = [];
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
        // Legacy properties (manteniamo per compatibilità)
        this.getLevel = (node) => node.level;
        this.isExpandable = (node) => node.expandable;
        this.hasChild = (_, _nodeData) => _nodeData.expandable;
        // Inizializza legacy tree control per compatibilità
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__["FlatTreeControl"](this.getLevel, this.isExpandable);
        let start = this.baseStart.start == null ? 'root' : this.baseStart.start;
        this.title = this.baseStart.title;
        this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService, start, baseStart.typeOfSelection);
    }
    createDirectoryOn(node) {
        if (node == null) {
            node = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        let dialogRef = this.dialog.open(_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_5__["NewDirectoryComponent"], {
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
            item = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_6__["MdFile"]("root", "root", 0, false);
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
        // Carica special folders e drives
        Object(rxjs__WEBPACK_IMPORTED_MODULE_3__["forkJoin"])({
            folders: this.mdFileService.getSpecialFolders(),
            drives: this.mdFileService.getDrives()
        }).subscribe({
            next: ({ folders, drives }) => {
                var _a;
                this.specialFolders = folders;
                this.drives = drives;
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
        // Aggiungi il path corrente alla history
        if (this.currentPath) {
            this.navigationHistory.push(this.currentPath);
        }
        this.currentPath = path;
        this.displayPath = this.formatDisplayPath(path);
        this.loadFolderContent(path);
    }
    navigateUp() {
        if (this.navigationHistory.length > 0) {
            const previousPath = this.navigationHistory.pop();
            this.currentPath = previousPath;
            this.displayPath = this.formatDisplayPath(previousPath);
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
        this.dialogRef.close({ event: 'open', data: this.folder.path });
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
        this.applyFilter();
    }
    applyFilter() {
        let filtered = [...this.currentItems];
        // Filtro per nome
        if (this.searchFilter && this.searchFilter.trim() !== '') {
            const filter = this.searchFilter.toLowerCase().trim();
            filtered = filtered.filter(item => item.name.toLowerCase().includes(filter));
        }
        // Filtro per estensioni (solo se specificato e per selezione file)
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles'
            && this.baseStart.fileExtensions
            && this.baseStart.fileExtensions.length > 0) {
            filtered = filtered.filter(item => {
                if (item.type === 'folder')
                    return true; // Sempre mostra cartelle per navigazione
                const extension = this.getFileExtension(item.name);
                return this.baseStart.fileExtensions.includes(extension);
            });
        }
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
        if (!this.folder.path)
            return false;
        if (this.baseStart.typeOfSelection === 'FoldersAndFiles') {
            // Solo file possono essere selezionati
            return this.activeNode && this.activeNode.type !== 'folder';
        }
        // Solo cartelle possono essere selezionate
        return true;
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
}
ShowFileSystemComponent.ɵfac = function ShowFileSystemComponent_Factory(t) { return new (t || ShowFileSystemComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](DynamicDatabase), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogRef"])); };
ShowFileSystemComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({ type: ShowFileSystemComponent, selectors: [["app-show-file-system"]], viewQuery: function ShowFileSystemComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 45, vars: 19, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [1, "file-explorer-container"], [1, "explorer-header"], [1, "explorer-title"], [1, "filter-section"], ["appearance", "outline", 1, "filter-input"], ["matInput", "", "placeholder", "Type to filter...", 3, "ngModel", "ngModelChange", "input"], ["matSuffix", ""], [1, "explorer-content"], [1, "left-panel"], [1, "quick-access-section"], [1, "section-title"], ["role", "navigation", "aria-label", "Quick access folders", 1, "nav-items"], ["class", "nav-item", "tabindex", "0", "role", "button", 3, "active", "click", "keydown.enter", "keydown.space", 4, "ngFor", "ngForOf", "ngForTrackBy"], [1, "drives-section"], ["role", "navigation", "aria-label", "Available drives", 1, "nav-items"], [1, "right-panel"], [1, "breadcrumb"], ["mat-icon-button", "", "matTooltip", "Go back", 3, "disabled", "click"], [1, "current-path"], ["class", "loading-indicator", 4, "ngIf"], ["class", "content-list", "role", "grid", "aria-label", "Folder contents", 3, "contextmenu", 4, "ngIf"], [1, "bottom-panel"], [1, "selected-folder-info"], [1, "action-buttons"], ["mat-stroked-button", "", "color", "primary", 3, "disabled", "click"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["tabindex", "0", "role", "button", 1, "nav-item", 3, "click", "keydown.enter", "keydown.space"], ["aria-hidden", "true", 1, "nav-icon"], [1, "nav-label"], [1, "loading-indicator"], ["diameter", "30"], ["role", "grid", "aria-label", "Folder contents", 1, "content-list", 3, "contextmenu"], ["class", "content-item", "tabindex", "0", "role", "gridcell", 3, "selected", "not-selectable", "click", "dblclick", "keydown.enter", "keydown.space", "contextmenu", 4, "ngFor", "ngForOf", "ngForTrackBy"], ["class", "empty-state", 4, "ngIf"], ["tabindex", "0", "role", "gridcell", 1, "content-item", 3, "click", "dblclick", "keydown.enter", "keydown.space", "contextmenu"], ["aria-hidden", "true", 1, "item-icon"], [1, "item-name"], [1, "empty-state"]], template: function ShowFileSystemComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](3, ShowFileSystemComponent_ng_template_3_Template, 13, 0, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "h1", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](9, "mat-form-field", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](10, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](11, "Filter files");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](12, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function ShowFileSystemComponent_Template_input_ngModelChange_12_listener($event) { return ctx.searchFilter = $event; })("input", function ShowFileSystemComponent_Template_input_input_12_listener($event) { return ctx.onFilterChange($event); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](13, "mat-icon", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](14, "search");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](15, "div", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](16, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](17, "div", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](18, "h3", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "Quick Access");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](20, "div", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](21, ShowFileSystemComponent_div_21_Template, 5, 5, "div", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](22, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](23, "h3", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](24, "This PC");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](25, "div", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](26, ShowFileSystemComponent_div_26_Template, 5, 6, "div", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](27, "div", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](28, "div", 19);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](29, "button", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_29_listener() { return ctx.navigateUp(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](30, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](31, "arrow_back");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](32, "span", 21);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](33);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](34, ShowFileSystemComponent_div_34_Template, 4, 0, "div", 22);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](35, ShowFileSystemComponent_div_35_Template, 3, 3, "div", 23);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](36, "div", 24);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](37, "div", 25);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](38, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](39);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](40, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](41);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](42, "div", 26);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](43, "button", 27);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_43_listener() { return ctx.closeDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](44);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.title);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.searchFilter);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.specialFolders)("ngForTrackBy", ctx.trackByPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.drives)("ngForTrackBy", ctx.trackByPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.navigationHistory.length === 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.displayPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.folder.name || "No folder selected");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.folder.path);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", !ctx.canSelectItem());
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.getSelectionButtonText(), " ");
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_11__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgModel"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatSuffix"], _angular_common__WEBPACK_IMPORTED_MODULE_14__["NgForOf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_15__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_16__["MatTooltip"], _angular_common__WEBPACK_IMPORTED_MODULE_14__["NgIf"], _angular_material_card__WEBPACK_IMPORTED_MODULE_17__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_17__["MatCardSubtitle"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuItem"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_18__["MatSpinner"]], styles: [".file-explorer-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  width: 100%;\n  background-color: #ffffff;\n  overflow: hidden;\n  position: relative;\n}\n\n.explorer-header[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #47b784, #2e7d32);\n  color: white;\n  padding: 16px 20px;\n  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  gap: 16px;\n}\n\n.explorer-title[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.2rem;\n  font-weight: 500;\n  flex: 1;\n}\n\n.filter-section[_ngcontent-%COMP%] {\n  min-width: 250px;\n}\n\n.filter-input[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-outline {\n  color: rgba(255, 255, 255, 0.3) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-outline-thick {\n  color: white !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-form-field-label {\n  color: rgba(255, 255, 255, 0.8) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-input-element {\n  color: white !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-input-element::placeholder {\n  color: rgba(255, 255, 255, 0.6) !important;\n}\n\n.filter-input[_ngcontent-%COMP%]     .mat-icon {\n  color: rgba(255, 255, 255, 0.8) !important;\n}\n\n.explorer-content[_ngcontent-%COMP%] {\n  display: flex;\n  flex: 1;\n  min-height: 0;\n}\n\n.left-panel[_ngcontent-%COMP%] {\n  width: 280px;\n  background: #f8f9fa;\n  display: flex;\n  flex-direction: column;\n  overflow-y: auto;\n}\n\n.section-title[_ngcontent-%COMP%] {\n  font-size: 0.9rem;\n  font-weight: 600;\n  color: #666;\n  margin: 16px 16px 8px 16px;\n  text-transform: uppercase;\n  letter-spacing: 0.5px;\n}\n\n.nav-items[_ngcontent-%COMP%] {\n  padding: 0 8px;\n}\n\n.nav-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 10px 12px;\n  margin: 2px 0;\n  cursor: pointer;\n  border-radius: 6px;\n  transition: all 0.2s ease;\n  -webkit-user-select: none;\n          user-select: none;\n}\n\n.nav-item[_ngcontent-%COMP%]:hover {\n  background: #e3f2fd;\n  transform: translateX(2px);\n}\n\n.nav-item[_ngcontent-%COMP%]:focus {\n  outline: 2px solid #2196f3;\n  outline-offset: 2px;\n  background: #e3f2fd;\n}\n\n.nav-item.active[_ngcontent-%COMP%] {\n  background: #2196f3;\n  color: white;\n  box-shadow: 0 2px 4px rgba(33, 150, 243, 0.3);\n}\n\n.nav-item.active[_ngcontent-%COMP%]   .nav-icon[_ngcontent-%COMP%] {\n  color: white;\n}\n\n.nav-item.active[_ngcontent-%COMP%]:focus {\n  outline-color: white;\n}\n\n.nav-icon[_ngcontent-%COMP%] {\n  margin-right: 12px;\n  font-size: 20px;\n  color: #666;\n  transition: color 0.2s ease;\n}\n\n.nav-label[_ngcontent-%COMP%] {\n  font-size: 0.95rem;\n  font-weight: 500;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.right-panel[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 0;\n}\n\n.breadcrumb[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  background: #fafafa;\n  border-bottom: 1px solid #e0e0e0;\n  min-height: 48px;\n}\n\n.breadcrumb[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-right: 12px;\n}\n\n.current-path[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 0.9rem;\n  color: #555;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  flex: 1;\n}\n\n.loading-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  color: #666;\n}\n\n.loading-indicator[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  margin-top: 12px;\n  font-size: 0.9rem;\n}\n\n.content-list[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 8px;\n}\n\n.content-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 10px 12px;\n  margin: 2px 0;\n  cursor: pointer;\n  border-radius: 6px;\n  transition: all 0.2s ease;\n  -webkit-user-select: none;\n          user-select: none;\n}\n\n.content-item[_ngcontent-%COMP%]:hover {\n  background: #f0f0f0;\n}\n\n.content-item[_ngcontent-%COMP%]:focus {\n  outline: 2px solid #2196f3;\n  outline-offset: 2px;\n  background: #f0f0f0;\n}\n\n.content-item.selected[_ngcontent-%COMP%] {\n  background: #e3f2fd;\n  border: 1px solid #2196f3;\n}\n\n.content-item.selected[_ngcontent-%COMP%]:focus {\n  outline-color: #1976d2;\n}\n\n.content-item[_ngcontent-%COMP%]:active {\n  transform: scale(0.98);\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%] {\n  opacity: 0.6;\n  cursor: default;\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%]:hover {\n  background: transparent;\n}\n\n.content-item.not-selectable[_ngcontent-%COMP%]:active {\n  transform: none;\n}\n\n.item-icon[_ngcontent-%COMP%] {\n  margin-right: 12px;\n  font-size: 20px;\n  color: #666;\n}\n\n.content-item[_ngcontent-%COMP%]:hover   .item-icon[_ngcontent-%COMP%] {\n  color: #2196f3;\n}\n\n.item-name[_ngcontent-%COMP%] {\n  font-size: 0.95rem;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  flex: 1;\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 20px;\n  color: #999;\n  text-align: center;\n}\n\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  margin-bottom: 16px;\n  opacity: 0.5;\n}\n\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 0.9rem;\n  margin: 0;\n}\n\n.bottom-panel[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  padding: 16px 20px;\n  background: #f8f9fa;\n  border-top: 1px solid #e0e0e0;\n  min-height: 80px;\n}\n\n.selected-folder-info[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.selected-folder-info[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin-bottom: 4px;\n  color: #333;\n}\n\n.selected-folder-info[_ngcontent-%COMP%]   mat-card-subtitle[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n  color: #666;\n  font-family: \"Courier New\", monospace;\n}\n\n.action-buttons[_ngcontent-%COMP%] {\n  margin-left: 20px;\n}\n\n.action-buttons[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  min-width: 140px;\n  font-weight: 500;\n}\n\n.flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.flex-items-overflow[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-x: scroll;\n  overflow-y: scroll;\n  background: tomato;\n  color: white;\n  text-align: center;\n  font-size: 3em;\n  flex: 1;\n}\n\n.flex-items[_ngcontent-%COMP%] {\n  width: 500px;\n  text-align: center;\n  font-size: 3em;\n}\n\n.flex-selected-folder[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n\n  .resizable-dialog-container {\n  resize: both;\n  overflow: auto;\n  min-width: 400px;\n  min-height: 300px;\n  max-width: 90vw;\n  max-height: 90vh;\n}\n\n  .resizable-dialog-container .mat-dialog-container {\n  height: 100%;\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n}\n\n  .resizable-dialog-container .mat-dialog-content {\n  flex: 1;\n  max-height: unset;\n  padding: 0;\n  margin: 0;\n  overflow: hidden;\n}\n\n  .resizable-dialog-container::after {\n  content: \"\";\n  position: absolute;\n  bottom: 0;\n  right: 0;\n  width: 15px;\n  height: 15px;\n  cursor: nwse-resize;\n  background: linear-gradient(135deg, transparent 50%, #47b784 50%), linear-gradient(135deg, transparent 50%, #47b784 50%);\n  background-size: 5px 5px, 5px 5px;\n  background-position: 100% 0, 0 100%;\n  background-repeat: no-repeat;\n  opacity: 0.5;\n  transition: opacity 0.2s ease;\n}\n\n  .resizable-dialog-container:hover::after {\n  opacity: 0.8;\n}\n\n@media (max-width: 768px) {\n  .file-explorer-container[_ngcontent-%COMP%] {\n    height: 500px;\n  }\n\n  .explorer-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    gap: 12px;\n    align-items: stretch;\n  }\n\n  .filter-section[_ngcontent-%COMP%] {\n    min-width: unset;\n  }\n\n  .left-panel[_ngcontent-%COMP%] {\n    width: 240px;\n  }\n\n  .nav-label[_ngcontent-%COMP%] {\n    font-size: 0.9rem;\n  }\n\n  .bottom-panel[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: stretch;\n  }\n  .bottom-panel[_ngcontent-%COMP%]   .action-buttons[_ngcontent-%COMP%] {\n    margin-left: 0;\n    margin-top: 12px;\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2hvdy1maWxlLXN5c3RlbS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EseUJBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0FBQUY7O0FBR0E7RUFDRSxxREFBQTtFQUNBLFlBQUE7RUFDQSxrQkFBQTtFQUNBLHdDQUFBO0VBQ0EsYUFBQTtFQUNBLDhCQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0FBQUY7O0FBR0E7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLE9BQUE7QUFBRjs7QUFHQTtFQUNFLGdCQUFBO0FBQUY7O0FBR0E7RUFDRSxXQUFBO0FBQUY7O0FBR0k7RUFDRSwwQ0FBQTtBQUROOztBQUlJO0VBQ0UsdUJBQUE7QUFGTjs7QUFLSTtFQUNFLDBDQUFBO0FBSE47O0FBTUk7RUFDRSx1QkFBQTtBQUpOOztBQU1NO0VBQ0UsMENBQUE7QUFKUjs7QUFRSTtFQUNFLDBDQUFBO0FBTk47O0FBV0E7RUFDRSxhQUFBO0VBQ0EsT0FBQTtFQUNBLGFBQUE7QUFSRjs7QUFZQTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGdCQUFBO0FBVEY7O0FBWUE7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLDBCQUFBO0VBQ0EseUJBQUE7RUFDQSxxQkFBQTtBQVRGOztBQVlBO0VBQ0UsY0FBQTtBQVRGOztBQVlBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EsZUFBQTtFQUNBLGtCQUFBO0VBQ0EseUJBQUE7RUFDQSx5QkFBQTtVQUFBLGlCQUFBO0FBVEY7O0FBV0U7RUFDRSxtQkFBQTtFQUNBLDBCQUFBO0FBVEo7O0FBWUU7RUFDRSwwQkFBQTtFQUNBLG1CQUFBO0VBQ0EsbUJBQUE7QUFWSjs7QUFhRTtFQUNFLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLDZDQUFBO0FBWEo7O0FBYUk7RUFDRSxZQUFBO0FBWE47O0FBY0k7RUFDRSxvQkFBQTtBQVpOOztBQWlCQTtFQUNFLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLFdBQUE7RUFDQSwyQkFBQTtBQWRGOztBQWlCQTtFQUNFLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7QUFkRjs7QUFrQkE7RUFDRSxPQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsWUFBQTtBQWZGOztBQWtCQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQ0FBQTtFQUNBLGdCQUFBO0FBZkY7O0FBaUJFO0VBQ0Usa0JBQUE7QUFmSjs7QUFtQkE7RUFDRSxxQ0FBQTtFQUNBLGlCQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtFQUNBLE9BQUE7QUFoQkY7O0FBbUJBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7RUFDQSxXQUFBO0FBaEJGOztBQWtCRTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7QUFoQko7O0FBb0JBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsWUFBQTtBQWpCRjs7QUFvQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSx5QkFBQTtFQUNBLHlCQUFBO1VBQUEsaUJBQUE7QUFqQkY7O0FBbUJFO0VBQ0UsbUJBQUE7QUFqQko7O0FBb0JFO0VBQ0UsMEJBQUE7RUFDQSxtQkFBQTtFQUNBLG1CQUFBO0FBbEJKOztBQXFCRTtFQUNFLG1CQUFBO0VBQ0EseUJBQUE7QUFuQko7O0FBcUJJO0VBQ0Usc0JBQUE7QUFuQk47O0FBdUJFO0VBQ0Usc0JBQUE7QUFyQko7O0FBd0JFO0VBQ0UsWUFBQTtFQUNBLGVBQUE7QUF0Qko7O0FBd0JJO0VBQ0UsdUJBQUE7QUF0Qk47O0FBeUJJO0VBQ0UsZUFBQTtBQXZCTjs7QUE0QkE7RUFDRSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0FBekJGOztBQTJCRTtFQUNFLGNBQUE7QUF6Qko7O0FBNkJBO0VBQ0Usa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7RUFDQSxPQUFBO0FBMUJGOztBQTZCQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxrQkFBQTtFQUNBLFdBQUE7RUFDQSxrQkFBQTtBQTFCRjs7QUE0QkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7QUExQko7O0FBNkJFO0VBQ0UsaUJBQUE7RUFDQSxTQUFBO0FBM0JKOztBQWdDQTtFQUNFLGFBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLDZCQUFBO0VBQ0EsZ0JBQUE7QUE3QkY7O0FBZ0NBO0VBQ0UsT0FBQTtBQTdCRjs7QUErQkU7RUFDRSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxXQUFBO0FBN0JKOztBQWdDRTtFQUNFLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLHFDQUFBO0FBOUJKOztBQWtDQTtFQUNFLGlCQUFBO0FBL0JGOztBQWlDRTtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7QUEvQko7O0FBb0NBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0FBakNGOztBQW9DQTtFQUNFLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7QUFqQ0Y7O0FBb0NBO0VBQ0UsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtBQWpDRjs7QUFvQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7QUFqQ0Y7O0FBcUNBO0VBQ0UsWUFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBbENGOztBQW9DRTtFQUNFLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxVQUFBO0FBbENKOztBQXFDRTtFQUNFLE9BQUE7RUFDQSxpQkFBQTtFQUNBLFVBQUE7RUFDQSxTQUFBO0VBQ0EsZ0JBQUE7QUFuQ0o7O0FBeUNFO0VBQ0UsV0FBQTtFQUNBLGtCQUFBO0VBQ0EsU0FBQTtFQUNBLFFBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0Esd0hBQ0U7RUFFRixpQ0FBQTtFQUNBLG1DQUFBO0VBQ0EsNEJBQUE7RUFDQSxZQUFBO0VBQ0EsNkJBQUE7QUF4Q0o7O0FBMkNFO0VBQ0UsWUFBQTtBQXpDSjs7QUE4Q0E7RUFDRTtJQUNFLGFBQUE7RUEzQ0Y7O0VBOENBO0lBQ0Usc0JBQUE7SUFDQSxTQUFBO0lBQ0Esb0JBQUE7RUEzQ0Y7O0VBOENBO0lBQ0UsZ0JBQUE7RUEzQ0Y7O0VBOENBO0lBQ0UsWUFBQTtFQTNDRjs7RUE4Q0E7SUFDRSxpQkFBQTtFQTNDRjs7RUE4Q0E7SUFDRSxzQkFBQTtJQUNBLG9CQUFBO0VBM0NGO0VBNkNFO0lBQ0UsY0FBQTtJQUNBLGdCQUFBO0VBM0NKO0FBQ0YiLCJmaWxlIjoic2hvdy1maWxlLXN5c3RlbS5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi8vIE1haW4gY29udGFpbmVyXHJcbi5maWxlLWV4cGxvcmVyLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICB3aWR0aDogMTAwJTtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZmZmZmZmO1xyXG4gIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgcG9zaXRpb246IHJlbGF0aXZlO1xyXG59XHJcblxyXG4uZXhwbG9yZXItaGVhZGVyIHtcclxuICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNDdiNzg0LCAjMmU3ZDMyKTtcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgcGFkZGluZzogMTZweCAyMHB4O1xyXG4gIGJveC1zaGFkb3c6IDAgMnB4IDRweCByZ2JhKDAsMCwwLDAuMSk7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDE2cHg7XHJcbn1cclxuXHJcbi5leHBsb3Jlci10aXRsZSB7XHJcbiAgbWFyZ2luOiAwO1xyXG4gIGZvbnQtc2l6ZTogMS4ycmVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgZmxleDogMTtcclxufVxyXG5cclxuLmZpbHRlci1zZWN0aW9uIHtcclxuICBtaW4td2lkdGg6IDI1MHB4O1xyXG59XHJcblxyXG4uZmlsdGVyLWlucHV0IHtcclxuICB3aWR0aDogMTAwJTtcclxuICBcclxuICA6Om5nLWRlZXAge1xyXG4gICAgLm1hdC1mb3JtLWZpZWxkLW91dGxpbmUge1xyXG4gICAgICBjb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjMpICFpbXBvcnRhbnQ7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIC5tYXQtZm9ybS1maWVsZC1vdXRsaW5lLXRoaWNrIHtcclxuICAgICAgY29sb3I6IHdoaXRlICFpbXBvcnRhbnQ7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIC5tYXQtZm9ybS1maWVsZC1sYWJlbCB7XHJcbiAgICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOCkgIWltcG9ydGFudDtcclxuICAgIH1cclxuICAgIFxyXG4gICAgLm1hdC1pbnB1dC1lbGVtZW50IHtcclxuICAgICAgY29sb3I6IHdoaXRlICFpbXBvcnRhbnQ7XHJcbiAgICAgIFxyXG4gICAgICAmOjpwbGFjZWhvbGRlciB7XHJcbiAgICAgICAgY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC42KSAhaW1wb3J0YW50O1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgICBcclxuICAgIC5tYXQtaWNvbiB7XHJcbiAgICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOCkgIWltcG9ydGFudDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5leHBsb3Jlci1jb250ZW50IHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXg6IDE7XHJcbiAgbWluLWhlaWdodDogMDsgLy8gSW1wb3J0YW50IGZvciBwcm9wZXIgZmxleCBiZWhhdmlvclxyXG59XHJcblxyXG4vLyBMZWZ0IHBhbmVsIC0gTmF2aWdhdGlvblxyXG4ubGVmdC1wYW5lbCB7XHJcbiAgd2lkdGg6IDI4MHB4O1xyXG4gIGJhY2tncm91bmQ6ICNmOGY5ZmE7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIG92ZXJmbG93LXk6IGF1dG87XHJcbn1cclxuXHJcbi5zZWN0aW9uLXRpdGxlIHtcclxuICBmb250LXNpemU6IDAuOXJlbTtcclxuICBmb250LXdlaWdodDogNjAwO1xyXG4gIGNvbG9yOiAjNjY2O1xyXG4gIG1hcmdpbjogMTZweCAxNnB4IDhweCAxNnB4O1xyXG4gIHRleHQtdHJhbnNmb3JtOiB1cHBlcmNhc2U7XHJcbiAgbGV0dGVyLXNwYWNpbmc6IDAuNXB4O1xyXG59XHJcblxyXG4ubmF2LWl0ZW1zIHtcclxuICBwYWRkaW5nOiAwIDhweDtcclxufVxyXG5cclxuLm5hdi1pdGVtIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogMTBweCAxMnB4O1xyXG4gIG1hcmdpbjogMnB4IDA7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIGJvcmRlci1yYWRpdXM6IDZweDtcclxuICB0cmFuc2l0aW9uOiBhbGwgMC4ycyBlYXNlO1xyXG4gIHVzZXItc2VsZWN0OiBub25lO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIGJhY2tncm91bmQ6ICNlM2YyZmQ7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVgoMnB4KTtcclxuICB9XHJcblxyXG4gICY6Zm9jdXMge1xyXG4gICAgb3V0bGluZTogMnB4IHNvbGlkICMyMTk2ZjM7XHJcbiAgICBvdXRsaW5lLW9mZnNldDogMnB4O1xyXG4gICAgYmFja2dyb3VuZDogI2UzZjJmZDtcclxuICB9XHJcblxyXG4gICYuYWN0aXZlIHtcclxuICAgIGJhY2tncm91bmQ6ICMyMTk2ZjM7XHJcbiAgICBjb2xvcjogd2hpdGU7XHJcbiAgICBib3gtc2hhZG93OiAwIDJweCA0cHggcmdiYSgzMywgMTUwLCAyNDMsIDAuMyk7XHJcblxyXG4gICAgLm5hdi1pY29uIHtcclxuICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgfVxyXG5cclxuICAgICY6Zm9jdXMge1xyXG4gICAgICBvdXRsaW5lLWNvbG9yOiB3aGl0ZTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5uYXYtaWNvbiB7XHJcbiAgbWFyZ2luLXJpZ2h0OiAxMnB4O1xyXG4gIGZvbnQtc2l6ZTogMjBweDtcclxuICBjb2xvcjogIzY2NjtcclxuICB0cmFuc2l0aW9uOiBjb2xvciAwLjJzIGVhc2U7XHJcbn1cclxuXHJcbi5uYXYtbGFiZWwge1xyXG4gIGZvbnQtc2l6ZTogMC45NXJlbTtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcclxufVxyXG5cclxuLy8gUmlnaHQgcGFuZWwgLSBDb250ZW50XHJcbi5yaWdodC1wYW5lbCB7XHJcbiAgZmxleDogMTtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgbWluLXdpZHRoOiAwOyAvLyBJbXBvcnRhbnQgZm9yIHByb3BlciBmbGV4IGJlaGF2aW9yXHJcbn1cclxuXHJcbi5icmVhZGNydW1iIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogMTJweCAxNnB4O1xyXG4gIGJhY2tncm91bmQ6ICNmYWZhZmE7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcbiAgbWluLWhlaWdodDogNDhweDtcclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIG1hcmdpbi1yaWdodDogMTJweDtcclxuICB9XHJcbn1cclxuXHJcbi5jdXJyZW50LXBhdGgge1xyXG4gIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XHJcbiAgZm9udC1zaXplOiAwLjlyZW07XHJcbiAgY29sb3I6ICM1NTU7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5sb2FkaW5nLWluZGljYXRvciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNDBweDtcclxuICBjb2xvcjogIzY2NjtcclxuICBcclxuICBzcGFuIHtcclxuICAgIG1hcmdpbi10b3A6IDEycHg7XHJcbiAgICBmb250LXNpemU6IDAuOXJlbTtcclxuICB9XHJcbn1cclxuXHJcbi5jb250ZW50LWxpc3Qge1xyXG4gIGZsZXg6IDE7XHJcbiAgb3ZlcmZsb3cteTogYXV0bztcclxuICBwYWRkaW5nOiA4cHg7XHJcbn1cclxuXHJcbi5jb250ZW50LWl0ZW0ge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBwYWRkaW5nOiAxMHB4IDEycHg7XHJcbiAgbWFyZ2luOiAycHggMDtcclxuICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgYm9yZGVyLXJhZGl1czogNnB4O1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjJzIGVhc2U7XHJcbiAgdXNlci1zZWxlY3Q6IG5vbmU7XHJcblxyXG4gICY6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZDogI2YwZjBmMDtcclxuICB9XHJcblxyXG4gICY6Zm9jdXMge1xyXG4gICAgb3V0bGluZTogMnB4IHNvbGlkICMyMTk2ZjM7XHJcbiAgICBvdXRsaW5lLW9mZnNldDogMnB4O1xyXG4gICAgYmFja2dyb3VuZDogI2YwZjBmMDtcclxuICB9XHJcblxyXG4gICYuc2VsZWN0ZWQge1xyXG4gICAgYmFja2dyb3VuZDogI2UzZjJmZDtcclxuICAgIGJvcmRlcjogMXB4IHNvbGlkICMyMTk2ZjM7XHJcblxyXG4gICAgJjpmb2N1cyB7XHJcbiAgICAgIG91dGxpbmUtY29sb3I6ICMxOTc2ZDI7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAmOmFjdGl2ZSB7XHJcbiAgICB0cmFuc2Zvcm06IHNjYWxlKDAuOTgpO1xyXG4gIH1cclxuXHJcbiAgJi5ub3Qtc2VsZWN0YWJsZSB7XHJcbiAgICBvcGFjaXR5OiAwLjY7XHJcbiAgICBjdXJzb3I6IGRlZmF1bHQ7XHJcbiAgICBcclxuICAgICY6aG92ZXIge1xyXG4gICAgICBiYWNrZ3JvdW5kOiB0cmFuc3BhcmVudDtcclxuICAgIH1cclxuXHJcbiAgICAmOmFjdGl2ZSB7XHJcbiAgICAgIHRyYW5zZm9ybTogbm9uZTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5pdGVtLWljb24ge1xyXG4gIG1hcmdpbi1yaWdodDogMTJweDtcclxuICBmb250LXNpemU6IDIwcHg7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgXHJcbiAgLmNvbnRlbnQtaXRlbTpob3ZlciAmIHtcclxuICAgIGNvbG9yOiAjMjE5NmYzO1xyXG4gIH1cclxufVxyXG5cclxuLml0ZW0tbmFtZSB7XHJcbiAgZm9udC1zaXplOiAwLjk1cmVtO1xyXG4gIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcclxuICBmbGV4OiAxO1xyXG59XHJcblxyXG4uZW1wdHktc3RhdGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDYwcHggMjBweDtcclxuICBjb2xvcjogIzk5OTtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogNDhweDtcclxuICAgIHdpZHRoOiA0OHB4O1xyXG4gICAgaGVpZ2h0OiA0OHB4O1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICAgIG9wYWNpdHk6IDAuNTtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgZm9udC1zaXplOiAwLjlyZW07XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBCb3R0b20gcGFuZWxcclxuLmJvdHRvbS1wYW5lbCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBwYWRkaW5nOiAxNnB4IDIwcHg7XHJcbiAgYmFja2dyb3VuZDogI2Y4ZjlmYTtcclxuICBib3JkZXItdG9wOiAxcHggc29saWQgI2UwZTBlMDtcclxuICBtaW4taGVpZ2h0OiA4MHB4O1xyXG59XHJcblxyXG4uc2VsZWN0ZWQtZm9sZGVyLWluZm8ge1xyXG4gIGZsZXg6IDE7XHJcbiAgXHJcbiAgbWF0LWNhcmQtdGl0bGUge1xyXG4gICAgZm9udC1zaXplOiAxcmVtO1xyXG4gICAgbWFyZ2luLWJvdHRvbTogNHB4O1xyXG4gICAgY29sb3I6ICMzMzM7XHJcbiAgfVxyXG4gIFxyXG4gIG1hdC1jYXJkLXN1YnRpdGxlIHtcclxuICAgIGZvbnQtc2l6ZTogMC44NXJlbTtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC1mYW1pbHk6ICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcclxuICB9XHJcbn1cclxuXHJcbi5hY3Rpb24tYnV0dG9ucyB7XHJcbiAgbWFyZ2luLWxlZnQ6IDIwcHg7XHJcbiAgXHJcbiAgYnV0dG9uIHtcclxuICAgIG1pbi13aWR0aDogMTQwcHg7XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gIH1cclxufVxyXG5cclxuLy8gTGVnYWN5IGNvbXBhdGliaWxpdHkgc3R5bGVzXHJcbi5mbGV4LWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4uZmxleC1pdGVtcy1vdmVyZmxvdyB7XHJcbiAgbWF4LWhlaWdodDogNDAwcHg7XHJcbiAgb3ZlcmZsb3cteDogc2Nyb2xsO1xyXG4gIG92ZXJmbG93LXk6IHNjcm9sbDtcclxuICBiYWNrZ3JvdW5kOiB0b21hdG87XHJcbiAgY29sb3I6IHdoaXRlO1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBmb250LXNpemU6IDNlbTtcclxuICBmbGV4OiAxO1xyXG59XHJcblxyXG4uZmxleC1pdGVtcyB7XHJcbiAgd2lkdGg6IDUwMHB4O1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBmb250LXNpemU6IDNlbTtcclxufVxyXG5cclxuLmZsZXgtc2VsZWN0ZWQtZm9sZGVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbn1cclxuXHJcbi8vIFJlc2l6YWJsZSBkaWFsb2cgd3JhcHBlclxyXG46Om5nLWRlZXAgLnJlc2l6YWJsZS1kaWFsb2ctY29udGFpbmVyIHtcclxuICByZXNpemU6IGJvdGg7XHJcbiAgb3ZlcmZsb3c6IGF1dG87XHJcbiAgbWluLXdpZHRoOiA0MDBweDtcclxuICBtaW4taGVpZ2h0OiAzMDBweDtcclxuICBtYXgtd2lkdGg6IDkwdnc7XHJcbiAgbWF4LWhlaWdodDogOTB2aDtcclxuICBcclxuICAubWF0LWRpYWxvZy1jb250YWluZXIge1xyXG4gICAgaGVpZ2h0OiAxMDAlO1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICBwYWRkaW5nOiAwO1xyXG4gIH1cclxuICBcclxuICAubWF0LWRpYWxvZy1jb250ZW50IHtcclxuICAgIGZsZXg6IDE7XHJcbiAgICBtYXgtaGVpZ2h0OiB1bnNldDtcclxuICAgIHBhZGRpbmc6IDA7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIH1cclxufVxyXG5cclxuLy8gUmVzaXplIGhhbmRsZSBzdHlsaW5nXHJcbjo6bmctZGVlcCAucmVzaXphYmxlLWRpYWxvZy1jb250YWluZXIge1xyXG4gICY6OmFmdGVyIHtcclxuICAgIGNvbnRlbnQ6ICcnO1xyXG4gICAgcG9zaXRpb246IGFic29sdXRlO1xyXG4gICAgYm90dG9tOiAwO1xyXG4gICAgcmlnaHQ6IDA7XHJcbiAgICB3aWR0aDogMTVweDtcclxuICAgIGhlaWdodDogMTVweDtcclxuICAgIGN1cnNvcjogbndzZS1yZXNpemU7XHJcbiAgICBiYWNrZ3JvdW5kOiBcclxuICAgICAgbGluZWFyLWdyYWRpZW50KDEzNWRlZywgdHJhbnNwYXJlbnQgNTAlLCAjNDdiNzg0IDUwJSksXHJcbiAgICAgIGxpbmVhci1ncmFkaWVudCgxMzVkZWcsIHRyYW5zcGFyZW50IDUwJSwgIzQ3Yjc4NCA1MCUpO1xyXG4gICAgYmFja2dyb3VuZC1zaXplOiA1cHggNXB4LCA1cHggNXB4O1xyXG4gICAgYmFja2dyb3VuZC1wb3NpdGlvbjogMTAwJSAwLCAwIDEwMCU7XHJcbiAgICBiYWNrZ3JvdW5kLXJlcGVhdDogbm8tcmVwZWF0O1xyXG4gICAgb3BhY2l0eTogMC41O1xyXG4gICAgdHJhbnNpdGlvbjogb3BhY2l0eSAwLjJzIGVhc2U7XHJcbiAgfVxyXG4gIFxyXG4gICY6aG92ZXI6OmFmdGVyIHtcclxuICAgIG9wYWNpdHk6IDAuODtcclxuICB9XHJcbn1cclxuXHJcbi8vIFJlc3BvbnNpdmUgZGVzaWduXHJcbkBtZWRpYSAobWF4LXdpZHRoOiA3NjhweCkge1xyXG4gIC5maWxlLWV4cGxvcmVyLWNvbnRhaW5lciB7XHJcbiAgICBoZWlnaHQ6IDUwMHB4O1xyXG4gIH1cclxuICBcclxuICAuZXhwbG9yZXItaGVhZGVyIHtcclxuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICBnYXA6IDEycHg7XHJcbiAgICBhbGlnbi1pdGVtczogc3RyZXRjaDtcclxuICB9XHJcbiAgXHJcbiAgLmZpbHRlci1zZWN0aW9uIHtcclxuICAgIG1pbi13aWR0aDogdW5zZXQ7XHJcbiAgfVxyXG4gIFxyXG4gIC5sZWZ0LXBhbmVsIHtcclxuICAgIHdpZHRoOiAyNDBweDtcclxuICB9XHJcbiAgXHJcbiAgLm5hdi1sYWJlbCB7XHJcbiAgICBmb250LXNpemU6IDAuOXJlbTtcclxuICB9XHJcbiAgXHJcbiAgLmJvdHRvbS1wYW5lbCB7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gICAgYWxpZ24taXRlbXM6IHN0cmV0Y2g7XHJcbiAgICBcclxuICAgIC5hY3Rpb24tYnV0dG9ucyB7XHJcbiAgICAgIG1hcmdpbi1sZWZ0OiAwO1xyXG4gICAgICBtYXJnaW4tdG9wOiAxMnB4O1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG4iXX0= */"] });


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