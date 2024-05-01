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

module.exports = __webpack_require__(/*! C:\Users\Carlo\Documents\2-personale\sviluppo\MdExplorer\MdExplorer\client2\src\main.ts */"zUnb");


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
/* harmony import */ var _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../models/gitlab-setting */ "MVql");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ "tk/3");




class GITService {
    constructor(http) {
        this.http = http;
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
        setInterval(() => this.getCurrentBranch(), 10000);
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
        let setting = new _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_1__["GitlabSetting"]();
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
}
GITService.ɵfac = function GITService_Factory(t) { return new (t || GITService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__["HttpClient"])); };
GITService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: GITService, factory: GITService.ɵfac, providedIn: 'root' });


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
        // searching directories    
        const currentItem = data[0];
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
        const currentItem = data[i + 1];
        const currentFolder = parentFolder.childrens.find(folder => folder.fullPath == currentItem.fullPath);
        if (currentFolder) {
            this.recursiveSearchFolder(data, i + 1, currentFolder);
        }
        else {
            parentFolder.childrens.push(currentItem); // Directly use currentItem
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles); // Simplified notification
        }
    }
    loadAll(callback, objectThis) {
        const url = '../api/mdfiles/GetAllMdFiles?connectionId=' + this.mdServerMessages.connectionId;
        return this.http.get(url)
            .subscribe(data => {
            this.dataStore.mdFiles = data;
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
            if (callback != null) {
                callback(data, objectThis);
            }
        }, error => {
            console.log("failed to fetch mdfile list");
        });
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
        leaf.name = newFile.name;
        leaf.fullPath = newFile.fullPath;
        leaf.path = newFile.path;
        leaf.relativePath = newFile.relativePath;
        this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
        this._serverSelectedMdFile.next(returnFound);
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
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/tree */ "8yBR");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ "ofXK");


















function ShowFileSystemComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    const _r6 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r6); const item_r4 = ctx.item; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r5.createDirectoryOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "button", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_ng_template_3_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r6); const item_r4 = ctx.item; const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r7.openFolderOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "b", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "b", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](9, "File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_mat_tree_node_9_button_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "text_snippet");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_mat_tree_node_9_button_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "button", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} }
function ShowFileSystemComponent_mat_tree_node_9_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-tree-node", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("contextmenu", function ShowFileSystemComponent_mat_tree_node_9_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const node_r8 = ctx.$implicit; const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r11.onRightClick($event, node_r8); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, ShowFileSystemComponent_mat_tree_node_9_button_1_Template, 3, 0, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](2, ShowFileSystemComponent_mat_tree_node_9_button_2_Template, 3, 0, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "a", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_mat_tree_node_9_Template_a_click_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12); const node_r8 = ctx.$implicit; const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); ctx_r13.getFolder(node_r8); return ctx_r13.activeNode = node_r8; });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r8 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", node_r8.type === "file");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", node_r8.type === "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](node_r8.name);
} }
function ShowFileSystemComponent_mat_tree_node_10_Template(rf, ctx) { if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "mat-tree-node", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_mat_tree_node_10_Template_mat_tree_node_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r16); const node_r14 = ctx.$implicit; const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r15.activeNode = node_r14; })("contextmenu", function ShowFileSystemComponent_mat_tree_node_10_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r16); const node_r14 = ctx.$implicit; const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r17.onRightClick($event, node_r14); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "a", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_mat_tree_node_10_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r16); const node_r14 = ctx.$implicit; const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](); return ctx_r18.getFolder(node_r14); });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r14 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵattribute"]("aria-label", "Toggle " + node_r14.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r3.treeControl.isExpanded(node_r14) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](node_r14.name);
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
        this.menuTopLeftPosition = { x: 0, y: 0 };
        this.getLevel = (node) => node.level;
        this.isExpandable = (node) => node.expandable;
        this.hasChild = (_, _nodeData) => _nodeData.expandable;
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
    }
    getFolder(node) {
        this.folder.name = node.name;
        this.folder.path = node.path;
    }
    closeDialog() {
        this.dialogRef.close({ event: 'open', data: this.folder.path });
    }
}
ShowFileSystemComponent.ɵfac = function ShowFileSystemComponent_Factory(t) { return new (t || ShowFileSystemComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](DynamicDatabase), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogRef"])); };
ShowFileSystemComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({ type: ShowFileSystemComponent, selectors: [["app-show-file-system"]], viewQuery: function ShowFileSystemComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 20, vars: 11, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [1, "flex-container"], [1, "flex-items-overflow"], [3, "dataSource", "treeControl", "contextmenu"], ["matTreeNodePadding", "", 3, "contextmenu", 4, "matTreeNodeDef"], ["matTreeNodePadding", "", 3, "click", "contextmenu", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], [1, "flex-selected-folder"], [1, "flex-items"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["matTreeNodePadding", "", 3, "contextmenu"], ["mat-icon-button", "", "color", "primary", "style", "cursor:pointer", 4, "ngIf"], ["mat-icon-button", "", 4, "ngIf"], [2, "cursor", "pointer", 3, "click"], ["mat-icon-button", "", "color", "primary", 2, "cursor", "pointer"], [1, "mat-icon-rtl-mirror"], ["mat-icon-button", ""], ["matTreeNodePadding", "", 3, "click", "contextmenu"], ["mat-icon-button", "", "matTreeNodeToggle", ""]], template: function ShowFileSystemComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](3, ShowFileSystemComponent_ng_template_3_Template, 10, 0, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "mat-tree", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("contextmenu", function ShowFileSystemComponent_Template_mat_tree_contextmenu_8_listener($event) { return ctx.onRightClick($event, null); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, ShowFileSystemComponent_mat_tree_node_9_Template, 5, 3, "mat-tree-node", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, ShowFileSystemComponent_mat_tree_node_10_Template, 6, 3, "mat-tree-node", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "div", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](12, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](13, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](14);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](15, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](16);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](17, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](18, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function ShowFileSystemComponent_Template_button_click_18_listener() { return ctx.closeDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "Select directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.title);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.hasChild);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.folder.name);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.folder.path);
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuContent"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__["MatTreeNodeDef"], _angular_material_card__WEBPACK_IMPORTED_MODULE_11__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_11__["MatCardSubtitle"], _angular_material_button__WEBPACK_IMPORTED_MODULE_12__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_13__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__["MatIcon"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_2__["MatMenuItem"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__["MatTreeNodePadding"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgIf"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_10__["MatTreeNodeToggle"]], styles: [".flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.flex-items-overflow[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-x: scroll;\n  overflow-y: scroll;\n  background: tomato;\n  color: white;\n  text-align: center;\n  font-size: 3em;\n  flex: 1;\n}\n\n.flex-items[_ngcontent-%COMP%] {\n  width: 500px;\n  text-align: center;\n  font-size: 3em;\n}\n\n.flex-selected-folder[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2hvdy1maWxlLXN5c3RlbS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUVBO0VBQ0UsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0EsT0FBQTtBQUNGOztBQUVBO0VBQ0UsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0FBQ0YiLCJmaWxlIjoic2hvdy1maWxlLXN5c3RlbS5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5mbGV4LWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4uZmxleC1pdGVtcy1vdmVyZmxvdyB7XHJcbiAgbWF4LWhlaWdodDogNDAwcHg7XHJcbiAgb3ZlcmZsb3cteDogc2Nyb2xsO1xyXG4gIG92ZXJmbG93LXk6IHNjcm9sbDtcclxuICBiYWNrZ3JvdW5kOiB0b21hdG87XHJcbiAgY29sb3I6IHdoaXRlO1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBmb250LXNpemU6IDNlbTtcclxuICBmbGV4OiAxO1xyXG59XHJcblxyXG4uZmxleC1pdGVtcyB7XHJcbiAgd2lkdGg6IDUwMHB4O1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBmb250LXNpemU6IDNlbTtcclxufVxyXG5cclxuLmZsZXgtc2VsZWN0ZWQtZm9sZGVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbn1cclxuIl19 */"] });


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