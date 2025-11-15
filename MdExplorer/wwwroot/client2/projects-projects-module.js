(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["projects-projects-module"],{

/***/ "02R5":
/*!****************************************************************!*\
  !*** ./src/app/projects/services/ide-configuration.service.ts ***!
  \****************************************************************/
/*! exports provided: IdeConfigurationService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "IdeConfigurationService", function() { return IdeConfigurationService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ "tk/3");




class IdeConfigurationService {
    constructor(http) {
        this.http = http;
        this.apiUrl = '../api/ideConfiguration';
        this.currentConfigSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"](null);
        this.currentConfig$ = this.currentConfigSubject.asObservable();
    }
    /**
     * Get IDE configuration for a project
     * @param projectPath Optional project path. If not provided, uses current project.
     */
    getIdeConfiguration(projectPath) {
        const params = projectPath ? { projectPath } : {};
        return this.http.get(`${this.apiUrl}/config`, { params }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["tap"])(config => this.currentConfigSubject.next(config)));
    }
    /**
     * Set IDE configuration for a project
     */
    setIdeConfiguration(request) {
        return this.http.post(`${this.apiUrl}/config`, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["tap"])(() => {
            // Reload configuration after setting
            this.getIdeConfiguration(request.projectPath).subscribe();
        }));
    }
    /**
     * Get current configuration from subject (synchronous)
     */
    getCurrentConfig() {
        return this.currentConfigSubject.value;
    }
}
IdeConfigurationService.ɵfac = function IdeConfigurationService_Factory(t) { return new (t || IdeConfigurationService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__["HttpClient"])); };
IdeConfigurationService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: IdeConfigurationService, factory: IdeConfigurationService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "Bw61":
/*!***************************************************************************!*\
  !*** ./src/app/projects/dialogs/clone-project/clone-project.component.ts ***!
  \***************************************************************************/
/*! exports provided: CloneProjectComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CloneProjectComponent", function() { return CloneProjectComponent; });
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo */ "N+BC");
/* harmony import */ var _git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../git/components/git-messages/git-messages.component */ "jwHG");
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ "DEjE");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ "eAi6");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");



















function CloneProjectComponent_mat_hint_35_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](1, "Enter your password");
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
} }
class CloneProjectComponent {
    constructor(dialog, mdFileService, gitService, dialogRef, waitingDialog, projectService, router) {
        this.dialog = dialog;
        this.mdFileService = mdFileService;
        this.gitService = gitService;
        this.dialogRef = dialogRef;
        this.waitingDialog = waitingDialog;
        this.projectService = projectService;
        this.router = router;
        this.hide = true;
        this.dataForCloning = {
            urlPath: null,
            directoryPath: null,
            username: null,
            password: null,
            email: null,
            storeCredentials: true
        };
    }
    ngOnInit() {
        this.mdFileService.getTextFromClipboard().subscribe(_ => {
            this.dataForCloning.urlPath = _.url;
        });
        // when the project change, then switch to navigation environment
        this.projectService.currentProjects$.subscribe(_ => {
            if (_ != null && _ != undefined) {
                this.router.navigate(['/main/navigation/document']); //main
                this.dialogRef.close();
            }
        });
    }
    cloneDirectory() {
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_1__["WaitingDialogInfo"]();
        info.message = "Oh MY GOD... Cloning Repository!";
        this.waitingDialog.showMessageBox(info);
        this.gitService.clone(this.dataForCloning).subscribe(_ => {
            if (_.areCredentialsCorrect) {
                this.projectService.setNewFolderProject(this.dataForCloning.directoryPath);
            }
            else {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_2__["GitMessagesComponent"], {
                    width: '300px',
                    data: { message: "Credentials are not correct" }
                });
            }
            this.waitingDialog.closeMessageBox();
            this.dialogRef.close(this.dataForCloning);
        });
    }
    openFileSystem() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_3__["ShowFileMetadata"]();
        data.start = null;
        data.title = "C:\ Folder";
        data.typeOfSelection = "Folders";
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__["ShowFileSystemComponent"], {
            width: '800px',
            height: '600px',
            panelClass: 'resizable-dialog-container',
            data: data
        });
        dialogRef.afterClosed().subscribe(_ => {
            this.dataForCloning.directoryPath = _.data;
        });
    }
}
CloneProjectComponent.ɵfac = function CloneProjectComponent_Factory(t) { return new (t || CloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_6__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_7__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_8__["WaitingDialogService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_10__["Router"])); };
CloneProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineComponent"]({ type: CloneProjectComponent, selectors: [["app-clone-project"]], decls: 42, vars: 8, consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/gitlogo.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "gitlab url", "required", "", 3, "ngModel", "ngModelChange"], [1, "orizzontal-form-container"], ["appearance", "outline", 2, "width", "100%"], ["matInput", "", "placeholder", "local filesystem", "required", "", 3, "ngModel", "ngModelChange"], ["mat-button", "", "matSuffix", "", "color", "primary", 3, "click"], ["matInput", "", "placeholder", "username", "required", "", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "password", "required", "", 3, "ngModel", "type", "ngModelChange"], ["matSuffix", "", 3, "click"], [4, "ngIf"], [1, "store-credential"], [3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-stroked-button", "", "color", "primary", 3, "click"]], template: function CloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](0, "h1", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelement"](1, "img", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](2, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](3, "Clone Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](4, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](5, "mat-card", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](6, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](7, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](8, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](9, "URL");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](10, "input", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_10_listener($event) { return ctx.dataForCloning.urlPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](11, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](12, "Set GIT Path");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](13, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](14, "mat-form-field", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](15, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](16, "Directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](17, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_17_listener($event) { return ctx.dataForCloning.directoryPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](18, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](19, "Set project directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](20, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_20_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](21, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](22, "more_horiz");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](23, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](24, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](25, "UserName");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](26, "input", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_26_listener($event) { return ctx.dataForCloning.username = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](27, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](28, "Username");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](29, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](30, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](31, "Password");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](32, "input", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_32_listener($event) { return ctx.dataForCloning.password = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](33, "mat-icon", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function CloneProjectComponent_Template_mat_icon_click_33_listener() { return ctx.hide = !ctx.hide; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](34);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtemplate"](35, CloneProjectComponent_mat_hint_35_Template, 2, 0, "mat-hint", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](36, "section", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](37, "mat-checkbox", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_mat_checkbox_ngModelChange_37_listener($event) { return ctx.dataForCloning.storeCredentials = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](38, "Store credentials");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](39, "mat-dialog-actions", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](40, "button", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_40_listener() { return ctx.cloneDirectory(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](41, "Clone!");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngModel", ctx.dataForCloning.urlPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngModel", ctx.dataForCloning.directoryPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngModel", ctx.dataForCloning.username);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngModel", ctx.dataForCloning.password)("type", ctx.hide ? "password" : "text");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate"](ctx.hide ? "visibility_off" : "visibility");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngIf", !ctx.dataForCloning.password);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("ngModel", ctx.dataForCloning.storeCredentials);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialogContent"], _angular_material_card__WEBPACK_IMPORTED_MODULE_11__["MatCard"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_13__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatHint"], _angular_material_button__WEBPACK_IMPORTED_MODULE_15__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatSuffix"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_17__["NgIf"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_18__["MatCheckbox"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialogActions"]], styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.orizzontal-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcY2xvbmUtcHJvamVjdC5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0FBQ0YiLCJmaWxlIjoiY2xvbmUtcHJvamVjdC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi52ZXJ0aWNhbC1mb3JtLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4ub3JpenpvbnRhbC1mb3JtLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogcm93O1xyXG59XHJcbiJdfQ== */"] });


/***/ }),

/***/ "GXPS":
/*!**************************************************************************************************!*\
  !*** ./src/app/projects/dialogs/project-create-config/project-create-config-dialog.component.ts ***!
  \**************************************************************************************************/
/*! exports provided: ProjectCreateConfigDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectCreateConfigDialogComponent", function() { return ProjectCreateConfigDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/divider */ "f0Cb");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");








class ProjectCreateConfigDialogComponent {
    constructor(dialogRef, data) {
        this.dialogRef = dialogRef;
        this.data = data;
        // Initialize with default values
        this.config = {
            projectPath: data.projectPath,
            initializeGit: false,
            addCopilotInstructions: true // Copilot instructions enabled by default
        };
    }
    ngOnInit() {
    }
    onCreateProject() {
        this.dialogRef.close(this.config);
    }
    onCancel() {
        this.dialogRef.close(null);
    }
}
ProjectCreateConfigDialogComponent.ɵfac = function ProjectCreateConfigDialogComponent_Factory(t) { return new (t || ProjectCreateConfigDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"])); };
ProjectCreateConfigDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ProjectCreateConfigDialogComponent, selectors: [["app-project-create-config-dialog"]], decls: 32, vars: 3, consts: [["mat-dialog-title", ""], [1, "config-container"], [1, "project-path"], [1, "path-text"], [1, "config-options"], [1, "option-item"], ["color", "primary", 3, "ngModel", "ngModelChange"], [1, "option-description"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"]], template: function ProjectCreateConfigDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Configure New Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, "Project Path:");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "span", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](11, "mat-divider");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-checkbox", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_14_listener($event) { return ctx.config.addCopilotInstructions = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16, "Add GitHub Copilot instructions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, " Creates .github/copilot-instructions.md file with project-specific AI guidelines ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "mat-checkbox", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_20_listener($event) { return ctx.config.initializeGit = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](22, "Initialize Git repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](23, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](24, " Creates a new Git repository with .gitignore configured for MdExplorer ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](25, "mat-dialog-actions", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](26, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_26_listener() { return ctx.onCancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](27, "Cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_28_listener() { return ctx.onCreateProject(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](29, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](30, "add");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, " Create Project ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.config.projectPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.config.addCopilotInstructions);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.config.initializeGit);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_2__["MatIcon"], _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__["MatDivider"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_4__["MatCheckbox"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgModel"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"]], styles: [".config-container[_ngcontent-%COMP%] {\n  min-width: 400px;\n  padding: 20px 0;\n}\n\n.project-path[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 15px;\n  background-color: rgba(0, 0, 0, 0.04);\n  border-radius: 4px;\n  margin-bottom: 20px;\n}\n\n.project-path[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #666;\n}\n\n.project-path[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  font-family: monospace;\n  color: #444;\n  word-break: break-all;\n}\n\n.config-options[_ngcontent-%COMP%] {\n  padding: 20px 0;\n}\n\n.option-item[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n}\n\n.option-item[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n}\n\n.option-item[_ngcontent-%COMP%]   .option-description[_ngcontent-%COMP%] {\n  margin-top: 5px;\n  margin-left: 28px;\n  font-size: 12px;\n  color: #666;\n  line-height: 1.4;\n}\n\nmat-divider[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxccHJvamVjdC1jcmVhdGUtY29uZmlnLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSxxQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFDRjs7QUFDRTtFQUNFLFdBQUE7QUFDSjs7QUFFRTtFQUNFLHNCQUFBO0VBQ0EsV0FBQTtFQUNBLHFCQUFBO0FBQUo7O0FBSUE7RUFDRSxlQUFBO0FBREY7O0FBSUE7RUFDRSxtQkFBQTtBQURGOztBQUdFO0VBQ0UsY0FBQTtBQURKOztBQUlFO0VBQ0UsZUFBQTtFQUNBLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtBQUZKOztBQU1BO0VBQ0UsY0FBQTtBQUhGOztBQU1BO0VBQ0Usa0JBQUE7QUFIRjs7QUFNSTtFQUNFLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBSk4iLCJmaWxlIjoicHJvamVjdC1jcmVhdGUtY29uZmlnLWRpYWxvZy5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5jb25maWctY29udGFpbmVyIHtcclxuICBtaW4td2lkdGg6IDQwMHB4O1xyXG4gIHBhZGRpbmc6IDIwcHggMDtcclxufVxyXG5cclxuLnByb2plY3QtcGF0aCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTBweDtcclxuICBwYWRkaW5nOiAxNXB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMCwgMCwgMCwgMC4wNCk7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIG1hcmdpbi1ib3R0b206IDIwcHg7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gIH1cclxuXHJcbiAgLnBhdGgtdGV4dCB7XHJcbiAgICBmb250LWZhbWlseTogbW9ub3NwYWNlO1xyXG4gICAgY29sb3I6ICM0NDQ7XHJcbiAgICB3b3JkLWJyZWFrOiBicmVhay1hbGw7XHJcbiAgfVxyXG59XHJcblxyXG4uY29uZmlnLW9wdGlvbnMge1xyXG4gIHBhZGRpbmc6IDIwcHggMDtcclxufVxyXG5cclxuLm9wdGlvbi1pdGVtIHtcclxuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xyXG5cclxuICBtYXQtY2hlY2tib3gge1xyXG4gICAgZGlzcGxheTogYmxvY2s7XHJcbiAgfVxyXG5cclxuICAub3B0aW9uLWRlc2NyaXB0aW9uIHtcclxuICAgIG1hcmdpbi10b3A6IDVweDtcclxuICAgIG1hcmdpbi1sZWZ0OiAyOHB4O1xyXG4gICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgICBsaW5lLWhlaWdodDogMS40O1xyXG4gIH1cclxufVxyXG5cclxubWF0LWRpdmlkZXIge1xyXG4gIG1hcmdpbjogMjBweCAwO1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweDtcclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgbWFyZ2luLXJpZ2h0OiA0cHg7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgaGVpZ2h0OiAyMHB4O1xyXG4gICAgICB3aWR0aDogMjBweDtcclxuICAgIH1cclxuICB9XHJcbn0iXX0= */"] });


/***/ }),

/***/ "O0mF":
/*!****************************************************!*\
  !*** ./src/app/shared/NgDialogAnimationService.ts ***!
  \****************************************************/
/*! exports provided: NgDialogAnimationService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NgDialogAnimationService", function() { return NgDialogAnimationService; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");




const diractionMap = { left: 'left', right: 'left', top: 'top', bottom: 'top' };
const multyMap = { left: 1, right: -1, top: 1, bottom: -1 };
class NgDialogAnimationService {
    constructor(dialog, ngZone, incomingOptions, outgoingOptions) {
        this.dialog = dialog;
        this.ngZone = ngZone;
        this.incomingOptions = incomingOptions;
        this.outgoingOptions = outgoingOptions;
        this.incomingOptions = {
            keyframes: [
                { transform: "rotate(360deg)" },
                { transform: "rotate(0)" }
            ],
            keyframeAnimationOptions: { easing: "ease-in-out", duration: 500 }
        };
        this.outgoingOptions = {
            keyframes: [
                { transform: "rotate(0)" },
                { transform: "rotate(360deg)" }
            ],
            keyframeAnimationOptions: { easing: "ease-in-out", duration: 500 }
        };
    }
    open(componentOrTemplateRef, config) {
        const dir = config.direction || (document.querySelectorAll('[dir="rtl"]').length ? 'rtl' : 'ltr');
        config.direction = config.direction || dir;
        if (config.animation) {
            if (config.animation.to === 'aside') {
                config.animation.to = dir === 'rtl' ? 'left' : 'right';
            }
        }
        if (config.position && config.position.rowEnd) {
            if (dir === 'rtl') {
                config.position.right = config.position.rowEnd;
            }
            else {
                config.position.left = config.position.rowEnd;
            }
        }
        if (config.position && config.position.rowStart) {
            if (dir === 'rtl') {
                config.position.left = config.position.rowStart;
            }
            else {
                config.position.right = config.position.rowStart;
            }
        }
        const ref = this.dialog.open(componentOrTemplateRef, config);
        const container = document.getElementsByTagName('mat-dialog-container')[0];
        if (config.title) {
            const el = document.createElement('span');
            el.textContent = config.title;
            el.className = 'dialogTitle';
            const elClose = document.createElement('span');
            elClose.textContent = 'X';
            elClose.className = 'dialogClose';
            elClose.addEventListener('click', () => {
                ref.close();
            });
            const titleContainer = document.createElement('div');
            titleContainer.className = 'titleContainer';
            titleContainer.append(el);
            titleContainer.append(elClose);
            container.prepend(titleContainer);
        }
        if (config.animation) {
            const incomingOptions = config.animation.incomingOptions ||
                this.incomingOptions || { keyframeAnimationOptions: { duration: 600, easing: 'ease-in' } };
            const outgoingOptions = config.animation.outgoingOptions ||
                this.outgoingOptions || { keyframeAnimationOptions: { duration: 600, easing: 'ease-out' } };
            const wrapper = document.getElementsByClassName('cdk-global-overlay-wrapper')[0];
            const animate = (keyframes, options) => {
                return wrapper.animate(keyframes, options);
            };
            const _afterClosed = new rxjs__WEBPACK_IMPORTED_MODULE_1__["Subject"]();
            ref.afterClosed = () => {
                return _afterClosed.asObservable();
            };
            const closeFunction = ref.close;
            let incomeKeyFrames = incomingOptions.keyframes;
            let outgoingKeyFrames = outgoingOptions.keyframes;
            if (config.animation.to) {
                const to = diractionMap[config.animation.to];
                const keyFrame100 = {};
                const keyFrame0 = {};
                keyFrame0[to] = 0;
                keyFrame100[to] =
                    to === 'top' || to === 'bottom'
                        ? container.clientHeight * multyMap[config.animation.to] + 'px'
                        : container.clientWidth * multyMap[config.animation.to] + 'px';
                incomeKeyFrames = incomeKeyFrames || [keyFrame100, keyFrame0];
                outgoingKeyFrames = outgoingKeyFrames || [keyFrame0, keyFrame100];
            }
            animate(incomeKeyFrames, incomingOptions.keyframeAnimationOptions);
            const closeHandler = (dialogResult) => {
                _afterClosed.next(dialogResult);
                const animation = animate(outgoingKeyFrames, outgoingOptions.keyframeAnimationOptions);
                animation.onfinish = () => {
                    wrapper.style.display = 'none';
                    this.ngZone.run(() => ref.close(dialogResult));
                };
                ref.close = closeFunction;
            };
            ref.close = (dialogResult) => closeHandler(dialogResult);
        }
        return ref;
    }
}
NgDialogAnimationService.ɵfac = function NgDialogAnimationService_Factory(t) { return new (t || NgDialogAnimationService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["NgZone"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"]('INCOMING_OPTION', 8), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"]('OUTGOING_OPTION', 8)); };
NgDialogAnimationService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: NgDialogAnimationService, factory: NgDialogAnimationService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "PK6v":
/*!***************************************************************!*\
  !*** ./src/app/projects/open-recent/open-recent.component.ts ***!
  \***************************************************************/
/*! exports provided: OpenRecentComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OpenRecentComponent", function() { return OpenRecentComponent; });
/* harmony import */ var angular_animations__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! angular-animations */ "6Z0Z");
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../project-settings/project-settings.component */ "mpGJ");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");












const _c0 = function (a0) { return { delay: a0 }; };
const _c1 = function (a1) { return { value: "", params: a1 }; };
function OpenRecentComponent_mat_card_3_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-card");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-card-actions");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "button", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_6_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r3.openNewProject(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_8_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r5.deleteProject(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "button", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_11_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r6.openProjectSettings(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](13, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r1 = ctx.$implicit;
    const i_r2 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("@bounceInLeftOnEnter", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](5, _c1, _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](3, _c0, i_r2 * 100)));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](element_r1.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](element_r1.path);
} }
class OpenRecentComponent {
    constructor(projectService, mdFileService, router, route, dialog) {
        this.projectService = projectService;
        this.mdFileService = mdFileService;
        this.router = router;
        this.route = route;
        this.dialog = dialog;
    }
    ngOnInit() {
        this.projectService.fetchProjects();
        this.dataSource = this.projectService.mdProjects;
        // when the project is loaded, then switch to navigation environment
        this.projectService.currentProjects$.subscribe(_ => {
            if (_ != null && _ != undefined) {
                this.router.navigate(['/main/navigation/document']);
            }
        });
    }
    openNewProject(path) {
        this.projectService.setNewFolderProject(path);
    }
    getProjectList(data, objectThis) {
        objectThis.projectService.fetchProjects();
    }
    ;
    deleteProject(project) {
        this.projectService.deleteProject(project, this.getProjectList, this);
    }
    openProjectSettings(project) {
        const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_1__["ProjectSettingsComponent"], {
            width: '600px',
            data: {
                projectId: project.id,
                projectName: project.name,
                projectPath: project.path
            }
        });
        dialogRef.afterClosed().subscribe(result => {
            console.log('Settings dialog closed');
        });
    }
}
OpenRecentComponent.ɵfac = function OpenRecentComponent_Factory(t) { return new (t || OpenRecentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_3__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_5__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_5__["ActivatedRoute"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_6__["MatDialog"])); };
OpenRecentComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: OpenRecentComponent, selectors: [["app-open-recent"]], decls: 5, vars: 3, consts: [[1, "projects-list-container"], [4, "ngFor", "ngForOf"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-raised-button", "", "color", "warn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Project Settings", 3, "click"]], template: function OpenRecentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "Recent Projects");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, OpenRecentComponent_mat_card_3_Template, 14, 7, "mat-card", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 1, ctx.dataSource));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_7__["NgForOf"], _angular_material_card__WEBPACK_IMPORTED_MODULE_8__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_8__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_8__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_8__["MatCardActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__["MatIcon"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_11__["MatTooltip"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_7__["AsyncPipe"]], styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  overflow: hidden;\n}\n\nh1[_ngcontent-%COMP%] {\n  margin: 0 0 20px 0;\n  font-weight: 400;\n  font-size: 1.8rem;\n  flex-shrink: 0;\n}\n\n.projects-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 0 8px 0 0;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n  scrollbar-width: thin;\n  scrollbar-color: rgba(0, 0, 0, 0.2) transparent;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: transparent;\n  border-radius: 4px;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: rgba(0, 0, 0, 0.2);\n  border-radius: 4px;\n  -webkit-transition: background 0.3s ease;\n  transition: background 0.3s ease;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: rgba(0, 0, 0, 0.3);\n}\n\nmat-card[_ngcontent-%COMP%] {\n  transition: transform 0.2s ease, box-shadow 0.2s ease;\n  cursor: pointer;\n}\n\nmat-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFxvcGVuLXJlY2VudC5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtBQUNGOztBQUVBO0VBQ0Usa0JBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0VBQ0EsY0FBQTtBQUNGOztBQUVBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0VBdUJBLHFCQUFBO0VBQ0EsK0NBQUE7QUFyQkY7O0FBQUU7RUFDRSxVQUFBO0FBRUo7O0FBQ0U7RUFDRSx1QkFBQTtFQUNBLGtCQUFBO0FBQ0o7O0FBRUU7RUFDRSw4QkFBQTtFQUNBLGtCQUFBO0VBQ0Esd0NBQUE7RUFBQSxnQ0FBQTtBQUFKOztBQUVJO0VBQ0UsOEJBQUE7QUFBTjs7QUFVQTtFQUNFLHFEQUFBO0VBQ0EsZUFBQTtBQVBGOztBQVNFO0VBQ0UsMkJBQUE7RUFDQSwwQ0FBQTtBQVBKIiwiZmlsZSI6Im9wZW4tcmVjZW50LmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxufVxyXG5cclxuaDEge1xyXG4gIG1hcmdpbjogMCAwIDIwcHggMDtcclxuICBmb250LXdlaWdodDogNDAwO1xyXG4gIGZvbnQtc2l6ZTogMS44cmVtO1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG59XHJcblxyXG4ucHJvamVjdHMtbGlzdC1jb250YWluZXIge1xyXG4gIGZsZXg6IDE7XHJcbiAgb3ZlcmZsb3cteTogYXV0bztcclxuICBwYWRkaW5nOiAwIDhweCAwIDA7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMTZweDtcclxuXHJcbiAgLy8gQ3VzdG9tIHNjcm9sbGJhciBzdHlsaW5nXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXIge1xyXG4gICAgd2lkdGg6IDhweDtcclxuICB9XHJcblxyXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRyYWNrIHtcclxuICAgIGJhY2tncm91bmQ6IHRyYW5zcGFyZW50O1xyXG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIH1cclxuXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdGh1bWIge1xyXG4gICAgYmFja2dyb3VuZDogcmdiYSgwLCAwLCAwLCAwLjIpO1xyXG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gICAgdHJhbnNpdGlvbjogYmFja2dyb3VuZCAwLjNzIGVhc2U7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6IHJnYmEoMCwgMCwgMCwgMC4zKTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC8vIEZpcmVmb3ggc2Nyb2xsYmFyXHJcbiAgc2Nyb2xsYmFyLXdpZHRoOiB0aGluO1xyXG4gIHNjcm9sbGJhci1jb2xvcjogcmdiYSgwLCAwLCAwLCAwLjIpIHRyYW5zcGFyZW50O1xyXG59XHJcblxyXG4vLyBTbW9vdGggY2FyZCBob3ZlciBlZmZlY3RcclxubWF0LWNhcmQge1xyXG4gIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjJzIGVhc2UsIGJveC1zaGFkb3cgMC4ycyBlYXNlO1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuXHJcbiAgJjpob3ZlciB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XHJcbiAgICBib3gtc2hhZG93OiAwIDRweCAxMnB4IHJnYmEoMCwgMCwgMCwgMC4xNSk7XHJcbiAgfVxyXG59XHJcbiJdfQ== */"], data: { animation: [
            Object(angular_animations__WEBPACK_IMPORTED_MODULE_0__["bounceInLeftOnEnterAnimation"])({ translate: '500px' }),
        ] } });


/***/ }),

/***/ "Wm2z":
/*!*********************************************!*\
  !*** ./src/app/projects/projects.module.ts ***!
  \*********************************************/
/*! exports provided: ProjectsModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsModule", function() { return ProjectsModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./open-recent/open-recent.component */ "PK6v");
/* harmony import */ var _projects_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./projects.component */ "zUkc");
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./new-project/new-project.component */ "5Zmz");
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ "Bw61");
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ "zabz");
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ "GXPS");
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./project-settings/project-settings.component */ "mpGJ");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../git/git.module */ "Trdj");
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./services/project-settings.service */ "tPmj");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/core */ "fXoL");















const routes = [
    {
        path: '', component: _projects_component__WEBPACK_IMPORTED_MODULE_2__["ProjectsComponent"],
        children: [
            { path: '', redirectTo: 'openrecent', pathMatch: 'full' },
            { path: 'openrecent', component: _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__["OpenRecentComponent"] },
            { path: 'newproject', component: _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__["NewProjectComponent"] },
        ]
    }
];
class ProjectsModule {
    constructor() {
        console.log('constructor ProjectsModule');
    }
}
ProjectsModule.ɵfac = function ProjectsModule_Factory(t) { return new (t || ProjectsModule)(); };
ProjectsModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵdefineNgModule"]({ type: ProjectsModule });
ProjectsModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵdefineInjector"]({ providers: [
        _services_project_settings_service__WEBPACK_IMPORTED_MODULE_12__["ProjectSettingsService"]
    ], imports: [[
            _angular_router__WEBPACK_IMPORTED_MODULE_8__["RouterModule"].forChild(routes),
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_9__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_10__["FormsModule"],
            _git_git_module__WEBPACK_IMPORTED_MODULE_11__["GitModule"],
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵsetNgModuleScope"](ProjectsModule, { declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__["OpenRecentComponent"],
        _projects_component__WEBPACK_IMPORTED_MODULE_2__["ProjectsComponent"],
        _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__["NewProjectComponent"],
        _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_4__["CloneProjectComponent"],
        _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_5__["ModernCloneProjectComponent"],
        _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_6__["ProjectCreateConfigDialogComponent"],
        _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_7__["ProjectSettingsComponent"]], imports: [_angular_router__WEBPACK_IMPORTED_MODULE_8__["RouterModule"], _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_9__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_10__["FormsModule"],
        _git_git_module__WEBPACK_IMPORTED_MODULE_11__["GitModule"]] }); })();


/***/ }),

/***/ "mpGJ":
/*!*************************************************************************!*\
  !*** ./src/app/projects/project-settings/project-settings.component.ts ***!
  \*************************************************************************/
/*! exports provided: ProjectSettingsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectSettingsComponent", function() { return ProjectSettingsComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../services/project-settings.service */ "tPmj");
/* harmony import */ var _services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/compatibility-mode.service */ "4rFV");
/* harmony import */ var _services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../services/ide-configuration.service */ "02R5");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/radio */ "QibW");















function ProjectSettingsComponent_div_11_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-checkbox", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_11_Template_mat_checkbox_ngModelChange_1_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r8); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r7.rule1Enabled = $event; })("change", function ProjectSettingsComponent_div_11_Template_mat_checkbox_change_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r8); const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r9.onRule1Change(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Rule #1: Check H1 matches filename");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, " When enabled, verifies that the H1 heading in each Markdown file matches the filename and automatically adds YAML frontmatter to markdown files. This helps maintain consistency between file names and document titles. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](6, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](7, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Note:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, " This feature should be disabled when using GitHub Compatible Mode, as GitHub doesn't recognize MdExplorer's custom YAML frontmatter format. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r0.rule1Enabled)("disabled", ctx_r0.saving);
} }
function ProjectSettingsComponent_div_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Loading settings...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function ProjectSettingsComponent_div_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Saving...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function ProjectSettingsComponent_div_19_Template(rf, ctx) { if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-checkbox", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_19_Template_mat_checkbox_ngModelChange_1_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r11); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r10.githubModeEnabled = $event; })("change", function ProjectSettingsComponent_div_19_Template_mat_checkbox_change_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r11); const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r12.onGitHubModeChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "GitHub Compatible Mode");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, " When enabled, uses GitHub-compatible markdown rendering (no PlantUML, interactive emoji, etc.). Disable for full MdExplorer features when working in a team environment. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.githubModeEnabled)("disabled", ctx_r3.saving);
} }
function ProjectSettingsComponent_div_27_Template(rf, ctx) { if (rf & 1) {
    const _r14 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, " Select which IDE should be used to open files from the toolbar: ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-radio-group", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_27_Template_mat_radio_group_ngModelChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r14); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r13.selectedIde = $event; })("change", function ProjectSettingsComponent_div_27_Template_mat_radio_group_change_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r14); const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r15.onIdeChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-radio-button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-icon", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, " Microsoft Visual Studio Code ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-radio-button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "mat-icon", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "terminal");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, " IntelliJ IDEA ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r4.selectedIde)("disabled", ctx_r4.saving);
} }
function ProjectSettingsComponent_div_28_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Loading settings...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function ProjectSettingsComponent_div_29_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Saving...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
class ProjectSettingsComponent {
    constructor(dialogRef, data, projectSettingsService, compatibilityService, ideConfigService) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.projectSettingsService = projectSettingsService;
        this.compatibilityService = compatibilityService;
        this.ideConfigService = ideConfigService;
        this.rule1Enabled = false;
        this.githubModeEnabled = false;
        this.selectedIde = 'vscode';
        this.vscodePath = '';
        this.intellijPath = '';
        this.loading = false;
        this.saving = false;
        this.projectId = data.projectId;
        this.projectName = data.projectName;
        this.projectPath = data.projectPath;
    }
    ngOnInit() {
        this.loadSettings();
    }
    loadSettings() {
        this.loading = true;
        let rule1Loaded = false;
        let compatibilityLoaded = false;
        let ideConfigLoaded = false;
        const checkIfDone = () => {
            if (rule1Loaded && compatibilityLoaded && ideConfigLoaded) {
                this.loading = false;
            }
        };
        // Load Rule 1 setting
        this.projectSettingsService.getRule1Setting().subscribe({
            next: (response) => {
                this.rule1Enabled = response.enabled;
                rule1Loaded = true;
                checkIfDone();
            },
            error: (error) => {
                console.error('Error loading Rule 1 setting:', error);
                rule1Loaded = true;
                checkIfDone();
            }
        });
        // Load compatibility mode for this specific project
        this.compatibilityService.getCurrentMode(this.projectPath).subscribe({
            next: (response) => {
                console.log('Compatibility mode loaded for project:', this.projectPath, response);
                this.githubModeEnabled = response.mode === 'github';
                compatibilityLoaded = true;
                checkIfDone();
            },
            error: (error) => {
                console.error('Error loading compatibility mode:', error);
                compatibilityLoaded = true;
                checkIfDone();
            }
        });
        // Load IDE configuration for this specific project
        this.ideConfigService.getIdeConfiguration(this.projectPath).subscribe({
            next: (response) => {
                console.log('IDE configuration loaded for project:', this.projectPath, response);
                this.selectedIde = response.selectedIde || 'vscode';
                this.vscodePath = response.vscodePath || '';
                this.intellijPath = response.intellijPath || '';
                ideConfigLoaded = true;
                checkIfDone();
            },
            error: (error) => {
                console.error('Error loading IDE configuration:', error);
                ideConfigLoaded = true;
                checkIfDone();
            }
        });
    }
    onRule1Change() {
        this.saving = true;
        this.projectSettingsService.setRule1Setting(this.rule1Enabled).subscribe({
            next: () => {
                console.log('Rule 1 setting saved successfully');
                this.saving = false;
            },
            error: (error) => {
                console.error('Error saving Rule 1 setting:', error);
                this.saving = false;
                // Revert the change on error
                this.rule1Enabled = !this.rule1Enabled;
            }
        });
    }
    onGitHubModeChange() {
        console.log('onGitHubModeChange called, githubModeEnabled:', this.githubModeEnabled);
        this.saving = true;
        const mode = this.githubModeEnabled ? 'github' : 'mdexplorer';
        console.log('Setting compatibility mode to:', mode, 'for project:', this.projectPath);
        this.compatibilityService.setCompatibilityMode({
            mode,
            githubOptions: {
                embedImages: false,
                stripInteractive: true,
                preserveEmoji: true
            },
            projectPath: this.projectPath
        }).subscribe({
            next: (response) => {
                console.log('Compatibility mode saved successfully:', mode, response);
                this.saving = false;
            },
            error: (error) => {
                console.error('Error saving compatibility mode:', error);
                this.saving = false;
                // Revert the change on error
                this.githubModeEnabled = !this.githubModeEnabled;
            }
        });
    }
    onIdeChange() {
        console.log('onIdeChange called, selectedIde:', this.selectedIde);
        this.saving = true;
        this.ideConfigService.setIdeConfiguration({
            selectedIde: this.selectedIde,
            projectPath: this.projectPath
        }).subscribe({
            next: (response) => {
                console.log('IDE configuration saved successfully:', this.selectedIde, response);
                this.saving = false;
            },
            error: (error) => {
                console.error('Error saving IDE configuration:', error);
                this.saving = false;
                // Revert the change on error
                this.selectedIde = this.selectedIde === 'vscode' ? 'intellij' : 'vscode';
            }
        });
    }
    close() {
        this.dialogRef.close();
    }
}
ProjectSettingsComponent.ɵfac = function ProjectSettingsComponent_Factory(t) { return new (t || ProjectSettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_project_settings_service__WEBPACK_IMPORTED_MODULE_2__["ProjectSettingsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_3__["CompatibilityModeService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_4__["IdeConfigurationService"])); };
ProjectSettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ProjectSettingsComponent, selectors: [["app-project-settings"]], decls: 39, vars: 8, consts: [["mat-dialog-title", ""], [1, "settings-tabs"], ["label", "Markdown Settings"], [1, "tab-content"], [1, "settings-section"], ["class", "setting-item", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "saving-indicator", 4, "ngIf"], ["label", "IDE Settings"], [1, "settings-section", "info-card"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "setting-item"], [3, "ngModel", "disabled", "ngModelChange", "change"], [1, "setting-label"], [1, "setting-description"], [1, "loading-container"], ["diameter", "30"], [1, "saving-indicator"], ["diameter", "20"], ["value", "vscode", 1, "ide-radio"], [1, "ide-label"], [1, "ide-icon-inline", "vscode-color"], ["value", "intellij", 1, "ide-radio"], [1, "ide-icon-inline", "intellij-color"]], template: function ProjectSettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-tab-group", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-tab", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-card", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Validation Rules");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, ProjectSettingsComponent_div_11_Template, 11, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, ProjectSettingsComponent_div_12_Template, 4, 0, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, ProjectSettingsComponent_div_13_Template, 4, 0, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-card", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "GitHub Compatibility");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](19, ProjectSettingsComponent_div_19_Template, 6, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "mat-tab", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-card", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](23, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](25, "IDE Configuration");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](26, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](27, ProjectSettingsComponent_div_27_Template, 14, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](28, ProjectSettingsComponent_div_28_Template, 4, 0, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](29, ProjectSettingsComponent_div_29_Template, 4, 0, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "mat-card", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](31, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](32, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](33, "info");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](34, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](35, " IDE settings are specific to this project and saved in the database. Changes take effect immediately when opening files from the toolbar. ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](36, "mat-dialog-actions", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](37, "button", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ProjectSettingsComponent_Template_button_click_37_listener() { return ctx.close(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](38, "Close");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("Project Settings - ", ctx.projectName, "");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.saving);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.saving);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__["MatTab"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardContent"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_10__["MatCheckbox"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgModel"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_12__["MatSpinner"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_13__["MatRadioGroup"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_13__["MatRadioButton"]], styles: [".settings-tabs[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 20px 16px;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n\n.setting-item[_ngcontent-%COMP%] {\n  margin: 16px 0;\n}\n\n.setting-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  margin-left: 8px;\n}\n\n.setting-description[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: rgba(0, 0, 0, 0.6);\n  font-size: 14px;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 20px;\n  gap: 12px;\n}\n\n.saving-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 8px;\n  margin-left: 32px;\n  color: rgba(0, 0, 0, 0.6);\n  font-size: 14px;\n}\n\n.info-card[_ngcontent-%COMP%] {\n  background-color: #f5f5f5;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: start;\n  gap: 12px;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 14px;\n  color: rgba(0, 0, 0, 0.7);\n}\n\n.ide-radio[_ngcontent-%COMP%] {\n  display: block;\n  margin: 12px 0;\n}\n\n.ide-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n.ide-icon-inline[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n\n.ide-icon-inline.vscode-color[_ngcontent-%COMP%] {\n  color: #007ACC;\n}\n\n.ide-icon-inline.intellij-color[_ngcontent-%COMP%] {\n  color: #FF6B00;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFxwcm9qZWN0LXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUNBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtBQUFGOztBQUdBO0VBQ0Usa0JBQUE7QUFBRjs7QUFHQTtFQUNFLG1CQUFBO0FBQUY7O0FBR0E7RUFDRSxjQUFBO0FBQUY7O0FBR0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBQUY7O0FBR0E7RUFDRSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSx5QkFBQTtFQUNBLGVBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSx5QkFBQTtFQUNBLGVBQUE7QUFBRjs7QUFHQTtFQUNFLHlCQUFBO0FBQUY7O0FBRUU7RUFDRSxhQUFBO0VBQ0Esa0JBQUE7RUFDQSxTQUFBO0FBQUo7O0FBRUk7RUFDRSxjQUFBO0FBQU47O0FBR0k7RUFDRSxTQUFBO0VBQ0EsZUFBQTtFQUNBLHlCQUFBO0FBRE47O0FBTUE7RUFDRSxjQUFBO0VBQ0EsY0FBQTtBQUhGOztBQU1BO0VBQ0UsZ0JBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0FBSEY7O0FBTUE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFIRjs7QUFLRTtFQUNFLGNBQUE7QUFISjs7QUFNRTtFQUNFLGNBQUE7QUFKSiIsImZpbGUiOiJwcm9qZWN0LXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLy8gVGFiIHN0eWxlc1xyXG4uc2V0dGluZ3MtdGFicyB7XHJcbiAgbWluLXdpZHRoOiA1MDBweDtcclxuICBtYXgtd2lkdGg6IDYwMHB4O1xyXG59XHJcblxyXG4udGFiLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDIwcHggMTZweDtcclxufVxyXG5cclxuLnNldHRpbmdzLXNlY3Rpb24ge1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbn1cclxuXHJcbi5zZXR0aW5nLWl0ZW0ge1xyXG4gIG1hcmdpbjogMTZweCAwO1xyXG59XHJcblxyXG4uc2V0dGluZy1sYWJlbCB7XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG59XHJcblxyXG4uc2V0dGluZy1kZXNjcmlwdGlvbiB7XHJcbiAgbWFyZ2luLXRvcDogOHB4O1xyXG4gIG1hcmdpbi1sZWZ0OiAzMnB4O1xyXG4gIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG59XHJcblxyXG4ubG9hZGluZy1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiAyMHB4O1xyXG4gIGdhcDogMTJweDtcclxufVxyXG5cclxuLnNhdmluZy1pbmRpY2F0b3Ige1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDhweDtcclxuICBtYXJnaW4tdG9wOiA4cHg7XHJcbiAgbWFyZ2luLWxlZnQ6IDMycHg7XHJcbiAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC42KTtcclxuICBmb250LXNpemU6IDE0cHg7XHJcbn1cclxuXHJcbi5pbmZvLWNhcmQge1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcblxyXG4gIG1hdC1jYXJkLWNvbnRlbnQge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBzdGFydDtcclxuICAgIGdhcDogMTJweDtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gICAgfVxyXG5cclxuICAgIHAge1xyXG4gICAgICBtYXJnaW46IDA7XHJcbiAgICAgIGZvbnQtc2l6ZTogMTRweDtcclxuICAgICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC43KTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5pZGUtcmFkaW8ge1xyXG4gIGRpc3BsYXk6IGJsb2NrO1xyXG4gIG1hcmdpbjogMTJweCAwO1xyXG59XHJcblxyXG4uaWRlLWxhYmVsIHtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDhweDtcclxufVxyXG5cclxuLmlkZS1pY29uLWlubGluZSB7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIHdpZHRoOiAyMHB4O1xyXG4gIGhlaWdodDogMjBweDtcclxuXHJcbiAgJi52c2NvZGUtY29sb3Ige1xyXG4gICAgY29sb3I6ICMwMDdBQ0M7IC8vIFZTIENvZGUgYmx1ZVxyXG4gIH1cclxuXHJcbiAgJi5pbnRlbGxpai1jb2xvciB7XHJcbiAgICBjb2xvcjogI0ZGNkIwMDsgLy8gSW50ZWxsaUogb3JhbmdlXHJcbiAgfVxyXG59XHJcblxyXG4vLyBJREUgcGF0aHMgc3R5bGluZyByZW1vdmVkIC0gbm93IHNob3duIGluIFByb2plY3RzIGNvbXBvbmVudCBtYWluIHBhZ2UiXX0= */"] });


/***/ }),

/***/ "tPmj":
/*!***************************************************************!*\
  !*** ./src/app/projects/services/project-settings.service.ts ***!
  \***************************************************************/
/*! exports provided: ProjectSettingsService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectSettingsService", function() { return ProjectSettingsService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common/http */ "tk/3");


class ProjectSettingsService {
    constructor(http) {
        this.http = http;
    }
    getProjectSettings() {
        const url = '../api/ProjectSettings/GetProjectSettings';
        return this.http.get(url);
    }
    saveProjectSetting(setting) {
        const url = '../api/ProjectSettings/SaveProjectSetting';
        return this.http.post(url, setting);
    }
    getRule1Setting() {
        const url = '../api/ProjectSettings/GetRule1Setting';
        return this.http.get(url);
    }
    setRule1Setting(enabled) {
        const url = '../api/ProjectSettings/SetRule1Setting';
        return this.http.post(url, { enabled });
    }
}
ProjectSettingsService.ɵfac = function ProjectSettingsService_Factory(t) { return new (t || ProjectSettingsService)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_1__["HttpClient"])); };
ProjectSettingsService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: ProjectSettingsService, factory: ProjectSettingsService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "zUkc":
/*!************************************************!*\
  !*** ./src/app/projects/projects.component.ts ***!
  \************************************************/
/*! exports provided: ProjectsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsComponent", function() { return ProjectsComponent; });
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ "zabz");
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ "GXPS");
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./project-settings/project-settings.component */ "mpGJ");
/* harmony import */ var _md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../md-explorer/components/dialogs/settings/settings.component */ "ATen");
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-metadata */ "DEjE");
/* harmony import */ var _environments_version__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../environments/version */ "octk");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../shared/NgDialogAnimationService */ "O0mF");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");







 // Importa la versione














function ProjectsComponent_div_4_button_10_Template(rf, ctx) { if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_div_4_button_10_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r5); const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); ctx_r4.searchQuery = ""; return ctx_r4.onSearchChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "clear");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ProjectsComponent_div_4_mat_card_12_span_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "schedule");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, " Last Opened ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function ProjectsComponent_div_4_mat_card_12_p_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "p", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const project_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" Last accessed: ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind2"](2, 1, project_r6.lastUpdate, "short"), " ");
} }
function ProjectsComponent_div_4_mat_card_12_Template(rf, ctx) { if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-card", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-card-content", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "div", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_div_click_2_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r11); const project_r6 = ctx.$implicit; const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r10.openProject(project_r6.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "div", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](4, "img", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "div", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "h3", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "p", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](11, ProjectsComponent_div_4_mat_card_12_span_11_Template, 4, 0, "span", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](12, ProjectsComponent_div_4_mat_card_12_p_12_Template, 3, 4, "p", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "div", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](14, "button", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_button_click_14_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r11); const project_r6 = ctx.$implicit; const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r12.openProjectSettings(project_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](15, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](16, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "button", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_button_click_17_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r11); const project_r6 = ctx.$implicit; const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2); return ctx_r13.deleteProject(project_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](18, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](19, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const project_r6 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("last-opened", ctx_r3.isLastOpened(project_r6));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](project_r6.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](project_r6.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r3.isLastOpened(project_r6));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", project_r6.lastUpdate);
} }
function ProjectsComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "h2", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "mat-icon", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "history");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5, " Recent Projects ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "mat-form-field", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "mat-icon", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "input", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectsComponent_div_4_Template_input_ngModelChange_9_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r15); const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r14.searchQuery = $event; })("input", function ProjectsComponent_div_4_Template_input_input_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r15); const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r16.onSearchChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](10, ProjectsComponent_div_4_button_10_Template, 3, 0, "button", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](11, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](12, ProjectsComponent_div_4_mat_card_12_Template, 20, 6, "mat-card", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](13, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](13, 3, ctx_r0.recentProjects));
} }
function ProjectsComponent_div_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "No Recent Projects");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6, "Start by creating a new project or cloning a repository");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
class ProjectsComponent {
    constructor(projectService, dialog, router, signalRService, dialogAn) {
        this.projectService = projectService;
        this.dialog = dialog;
        this.router = router;
        this.signalRService = signalRService;
        this.dialogAn = dialogAn;
        this.appVersion = _environments_version__WEBPACK_IMPORTED_MODULE_7__["versionInfo"].version; // Rendi la versione disponibile nel template
        this.buildTime = _environments_version__WEBPACK_IMPORTED_MODULE_7__["versionInfo"].buildTime; // Rendi il timestamp di build disponibile nel template
        this.searchQuery = '';
        this.lastOpenedProjectId = null;
        this.dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }];
    }
    ngOnDestroy() {
        console.log('ProjectsComponent destroyed!');
    }
    ngOnInit() {
        // Load recent projects and sort by lastUpdate descending (most recent first)
        this.projectService.fetchProjects();
        this.recentProjects = this.projectService.mdProjects.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_0__["map"])(projects => {
            if (!projects || projects.length === 0)
                return [];
            const sorted = projects.sort((a, b) => {
                const dateA = a.lastUpdate ? new Date(a.lastUpdate).getTime() : 0;
                const dateB = b.lastUpdate ? new Date(b.lastUpdate).getTime() : 0;
                return dateB - dateA; // Descending order (most recent first)
            });
            // Identify the last opened project (first in sorted list)
            if (sorted.length > 0 && !this.lastOpenedProjectId) {
                this.lastOpenedProjectId = sorted[0].id;
            }
            // Apply search filter
            if (this.searchQuery && this.searchQuery.trim() !== '') {
                const query = this.searchQuery.toLowerCase();
                return sorted.filter(p => p.name.toLowerCase().includes(query) ||
                    p.path.toLowerCase().includes(query));
            }
            return sorted;
        }));
        this.projectService.currentProjects$.subscribe(_ => {
            if (_ != null && _ != undefined) {
                this.router.navigate(['/main/navigation/document']); //main
            }
        });
    }
    onSearchChange() {
        // Trigger the observable to re-filter
        this.projectService.fetchProjects();
    }
    isLastOpened(project) {
        return project.id === this.lastOpenedProjectId;
    }
    openProject(path) {
        this.projectService.setNewFolderProject(path);
    }
    deleteProject(project) {
        this.projectService.deleteProject(project, () => {
            this.projectService.fetchProjects();
        }, this);
    }
    openProjectSettings(project) {
        const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_4__["ProjectSettingsComponent"], {
            width: '600px',
            data: {
                projectId: project.id,
                projectName: project.name,
                projectPath: project.path
            }
        });
    }
    openRecent() {
        this.router.navigate(['/projects/openrecent']);
    }
    prepareToClone() {
        const dialogRef = this.dialog.open(_dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_2__["ModernCloneProjectComponent"], {
            width: '600px',
            maxHeight: '600px',
            data: null
        });
    }
    openNewFolder() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_6__["ShowFileMetadata"]();
        data.start = null;
        data.title = "Select project folder";
        data.typeOfSelection = "Folders";
        data.buttonText = "Select folder"; // Testo personalizzato
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__["ShowFileSystemComponent"], {
            width: '800px',
            height: '600px',
            panelClass: 'resizable-dialog-container',
            data: data
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result && result.data) {
                // Open configuration dialog after folder selection
                const configDialogRef = this.dialog.open(_dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_3__["ProjectCreateConfigDialogComponent"], {
                    width: '500px',
                    disableClose: true,
                    data: { projectPath: result.data }
                });
                configDialogRef.afterClosed().subscribe(config => {
                    if (config) {
                        // Create project with configuration options
                        this.projectService.createProjectWithConfig(config);
                    }
                });
            }
        });
    }
    openSettings() {
        const dialogRef = this.dialogAn.open(_md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_5__["SettingsComponent"], {
            width: '600px',
            animation: {},
        });
    }
}
ProjectsComponent.ɵfac = function ProjectsComponent_Factory(t) { return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_10__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_11__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_13__["NgDialogAnimationService"])); };
ProjectsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({ type: ProjectsComponent, selectors: [["app-projects"]], decls: 53, vars: 6, consts: [[1, "modern-container"], [1, "content-wrapper"], [1, "main-layout"], [1, "left-column"], ["class", "recent-projects-section", 4, "ngIf"], ["class", "no-projects-message", 4, "ngIf"], [1, "right-column"], [1, "section-title"], [1, "section-icon"], [1, "actions-grid"], ["data-test", "clone-button", 1, "action-card", "clone-card", 3, "click"], [1, "card-content"], [1, "icon-wrapper", "clone-icon"], [1, "card-text"], [1, "card-title"], [1, "card-description"], [1, "card-action"], [1, "arrow-icon"], ["data-test", "new-folder-button", 1, "action-card", "create-card", 3, "click"], [1, "icon-wrapper", "create-icon"], ["data-test", "settings-button", 1, "action-card", "settings-card", 3, "click"], [1, "icon-wrapper", "settings-icon"], [1, "recent-projects-section"], [1, "section-header"], ["appearance", "outline", 1, "search-field"], ["matPrefix", ""], ["matInput", "", "placeholder", "Search projects...", 3, "ngModel", "ngModelChange", "input"], ["mat-icon-button", "", "matSuffix", "", 3, "click", 4, "ngIf"], [1, "projects-grid"], ["class", "project-card", 3, "last-opened", 4, "ngFor", "ngForOf"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], [1, "project-card"], [1, "project-card-content"], [1, "project-info", 3, "click"], [1, "project-icon-wrapper"], ["src", "/assets/icons/files-stack.svg", "alt", "Project", 1, "project-icon-svg"], [1, "project-details"], [1, "project-name"], [1, "project-path"], [1, "project-meta"], ["class", "last-opened-badge", 4, "ngIf"], ["class", "project-date", 4, "ngIf"], [1, "project-actions"], ["mat-icon-button", "", "matTooltip", "Project Settings", 1, "action-btn", "settings-btn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Delete Project", 1, "action-btn", "delete-btn", 3, "click"], [1, "last-opened-badge"], [1, "badge-icon"], [1, "project-date"], [1, "no-projects-message"], [1, "empty-icon"]], template: function ProjectsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](4, ProjectsComponent_div_4_Template, 14, 5, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](5, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](6, ProjectsComponent_div_6_Template, 7, 0, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "h2", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "mat-icon", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11, "flash_on");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](12, " Quick Actions ");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](14, "mat-card", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_14_listener() { return ctx.prepareToClone(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](15, "mat-card-content", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](16, "div", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](18, "cloud_download");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](19, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](20, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](21, "Clone Repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](22, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](23, "Get code from an online repository like GitHub or GitLab");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](24, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](25, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](26, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](27, "mat-card", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_27_listener() { return ctx.openNewFolder(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](28, "mat-card-content", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](29, "div", 19);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](30, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](31, "create_new_folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](32, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](33, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](34, "Create New Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](35, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](36, "Choose a filesystem folder to get started with your documentation");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](37, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](38, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](39, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](40, "mat-card", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_40_listener() { return ctx.openSettings(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](41, "mat-card-content", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](42, "div", 21);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](43, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](44, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](45, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](46, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](47, "Settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](48, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](49, "Configure IDE paths, PlantUML, and other global preferences");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](50, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](51, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](52, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    } if (rf & 2) {
        let tmp_0_0 = null;
        let tmp_1_0 = null;
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ((tmp_0_0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](5, 2, ctx.recentProjects)) == null ? null : tmp_0_0.length) > 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !((tmp_1_0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 4, ctx.recentProjects)) == null ? null : tmp_1_0.length));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_14__["NgIf"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__["MatIcon"], _angular_material_card__WEBPACK_IMPORTED_MODULE_16__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_16__["MatCardContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_17__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_17__["MatPrefix"], _angular_material_input__WEBPACK_IMPORTED_MODULE_18__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_19__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_19__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_19__["NgModel"], _angular_common__WEBPACK_IMPORTED_MODULE_14__["NgForOf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_20__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_17__["MatSuffix"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_21__["MatTooltip"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_14__["AsyncPipe"], _angular_common__WEBPACK_IMPORTED_MODULE_14__["DatePipe"]], styles: [".modern-container[_ngcontent-%COMP%] {\n  min-height: 100vh;\n  background: #f5f7fa;\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n  overflow-x: hidden;\n  overflow-y: auto;\n}\n\n.content-wrapper[_ngcontent-%COMP%] {\n  flex: 1;\n  background: #f5f7fa;\n  padding: 40px;\n  max-width: 1600px;\n  margin: 0 auto;\n  width: 100%;\n  min-height: calc(100vh - 36px);\n  box-sizing: border-box;\n}\n\n.main-layout[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 400px;\n  gap: 32px;\n  align-items: start;\n  height: calc(100vh - 116px);\n}\n\n.left-column[_ngcontent-%COMP%] {\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  max-height: 100%;\n}\n\n.right-column[_ngcontent-%COMP%] {\n  position: sticky;\n  top: 40px;\n  align-self: start;\n}\n\n.actions-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.action-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 16px;\n  border: none;\n  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);\n  overflow: hidden;\n  background: white;\n}\n\n.action-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);\n}\n\n.action-card[_ngcontent-%COMP%]:hover   .arrow-icon[_ngcontent-%COMP%] {\n  transform: translateX(4px);\n}\n\n.action-card[_ngcontent-%COMP%]:hover   .icon-wrapper[_ngcontent-%COMP%] {\n  transform: scale(1.1);\n}\n\n.action-card[_ngcontent-%COMP%]:active {\n  transform: translateY(-2px);\n}\n\n.action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 16px;\n  padding: 20px 16px;\n  position: relative;\n}\n\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 48px;\n  height: 48px;\n  border-radius: 12px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: transform 0.3s ease;\n}\n\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: white;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-title[_ngcontent-%COMP%] {\n  margin: 0 0 4px 0;\n  font-size: 1rem;\n  font-weight: 500;\n  color: #2d3748;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.75rem;\n  color: #718096;\n  line-height: 1.4;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%]   .arrow-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: transform 0.3s ease;\n}\n\n.action-card.clone-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n}\n\n.action-card.create-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);\n}\n\n.action-card.settings-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);\n}\n\n.section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #2d3748;\n  margin: 0 0 24px 0;\n}\n\n.section-title[_ngcontent-%COMP%]   .section-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n  color: #667eea;\n}\n\n.section-divider[_ngcontent-%COMP%] {\n  height: 1px;\n  background: linear-gradient(to right, transparent, #e2e8f0, transparent);\n  margin: 32px 0;\n}\n\n.recent-projects-section[_ngcontent-%COMP%] {\n  margin-bottom: 0;\n  display: flex;\n  flex-direction: column;\n  flex: 1;\n  min-height: 0;\n}\n\n.section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 24px;\n  gap: 24px;\n  flex-shrink: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  margin: 0;\n  flex-shrink: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n  width: 100%;\n  max-width: 400px;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n  margin-bottom: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-infix[_ngcontent-%COMP%] {\n  padding: 0.5em 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-underline[_ngcontent-%COMP%] {\n  display: none;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #718096;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.no-projects-message[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 60px 20px;\n  color: #718096;\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   .empty-icon[_ngcontent-%COMP%] {\n  font-size: 80px;\n  width: 80px;\n  height: 80px;\n  color: #cbd5e0;\n  margin: 0 auto 20px;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #4a5568;\n  margin: 0 0 8px 0;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n  flex: 1;\n  overflow-y: auto;\n  padding-right: 8px;\n  min-height: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: #e2e8f0;\n  border-radius: 4px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: #cbd5e0;\n  border-radius: 4px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: #a0aec0;\n}\n\n.project-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 12px;\n  border: 1px solid #e2e8f0;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);\n  background: white;\n  position: relative;\n  overflow: visible;\n}\n\n.project-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);\n  border-color: #667eea;\n}\n\n.project-card.last-opened[_ngcontent-%COMP%] {\n  border: 2px solid #667eea;\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);\n  background: linear-gradient(135deg, rgba(102, 126, 234, 0.03) 0%, rgba(118, 75, 162, 0.03) 100%);\n}\n\n.project-card.last-opened[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(102, 126, 234, 0.3);\n  border-color: #764ba2;\n}\n\n.project-card.last-opened[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);\n  animation: pulse 2s ease-in-out infinite;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 16px;\n  gap: 12px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 1;\n  min-width: 0;\n  cursor: pointer;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 40px;\n  height: 40px;\n  border-radius: 10px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: white;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon-svg[_ngcontent-%COMP%] {\n  width: 20px;\n  height: 20px;\n  filter: brightness(0) invert(1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.1rem;\n  font-weight: 500;\n  color: #2d3748;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-path[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: #718096;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-meta[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin-top: 4px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n  padding: 4px 10px;\n  border-radius: 12px;\n  font-size: 0.65rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.5px;\n  white-space: nowrap;\n  box-shadow: 0 2px 6px rgba(102, 126, 234, 0.3);\n  animation: fadeInScale 0.5s ease-out;\n  flex-shrink: 0;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%]   .badge-icon[_ngcontent-%COMP%] {\n  font-size: 12px;\n  width: 12px;\n  height: 12px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-date[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.7rem;\n  color: #a0aec0;\n  font-style: italic;\n  flex: 1;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 4px;\n  flex-shrink: 0;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%] {\n  transition: all 0.2s ease;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(102, 126, 234, 0.1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #667eea;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(245, 87, 108, 0.1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #f5576c;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: color 0.2s ease;\n}\n\n.sidebar-section[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n}\n\n@media (max-width: 1024px) {\n  .main-layout[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n    gap: 32px;\n  }\n\n  .right-column[_ngcontent-%COMP%] {\n    position: static;\n  }\n\n  .actions-grid[_ngcontent-%COMP%] {\n    display: grid;\n    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));\n    gap: 16px;\n  }\n}\n\n@media (max-width: 768px) {\n  .header-section[_ngcontent-%COMP%] {\n    padding: 40px 20px 20px;\n  }\n  .header-section[_ngcontent-%COMP%]   .main-title[_ngcontent-%COMP%] {\n    font-size: 2rem;\n  }\n  .header-section[_ngcontent-%COMP%]   .subtitle[_ngcontent-%COMP%] {\n    font-size: 1rem;\n  }\n\n  .content-wrapper[_ngcontent-%COMP%] {\n    padding: 32px 20px;\n    border-radius: 24px 24px 0 0;\n  }\n\n  .actions-grid[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n  }\n\n  .action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n    padding: 24px 20px;\n  }\n\n  .section-title[_ngcontent-%COMP%] {\n    font-size: 1.25rem;\n  }\n\n  .section-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: flex-start;\n  }\n  .section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n    max-width: 100%;\n  }\n}\n\n@keyframes pulse {\n  0%, 100% {\n    transform: scale(1);\n  }\n  50% {\n    transform: scale(1.05);\n  }\n}\n\n@keyframes fadeInScale {\n  0% {\n    opacity: 0;\n    transform: scale(0.8);\n  }\n  100% {\n    opacity: 1;\n    transform: scale(1);\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXHByb2plY3RzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUNBO0VBQ0UsaUJBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFVBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQUY7O0FBSUE7RUFDRSxPQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsV0FBQTtFQUNBLDhCQUFBO0VBQ0Esc0JBQUE7QUFERjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxnQ0FBQTtFQUNBLFNBQUE7RUFDQSxrQkFBQTtFQUNBLDJCQUFBO0FBRkY7O0FBS0E7RUFDRSxZQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsWUFBQTtFQUNBLGdCQUFBO0FBRkY7O0FBS0E7RUFDRSxnQkFBQTtFQUNBLFNBQUE7RUFDQSxpQkFBQTtBQUZGOztBQU1BO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQUhGOztBQU9BO0VBQ0UsZUFBQTtFQUNBLGlEQUFBO0VBQ0EsbUJBQUE7RUFDQSxZQUFBO0VBQ0EseUNBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0FBSkY7O0FBTUU7RUFDRSwyQkFBQTtFQUNBLDJDQUFBO0FBSko7O0FBTUk7RUFDRSwwQkFBQTtBQUpOOztBQU9JO0VBQ0UscUJBQUE7QUFMTjs7QUFTRTtFQUNFLDJCQUFBO0FBUEo7O0FBVUU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtBQVJKOztBQVdFO0VBQ0UsY0FBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLCtCQUFBO0FBVEo7O0FBV0k7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxZQUFBO0FBVE47O0FBYUU7RUFDRSxPQUFBO0FBWEo7O0FBYUk7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7QUFYTjs7QUFjSTtFQUNFLFNBQUE7RUFDQSxrQkFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQVpOOztBQWdCRTtFQUNFLGNBQUE7QUFkSjs7QUFnQkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsK0JBQUE7QUFkTjs7QUFtQkU7RUFDRSw2REFBQTtBQWpCSjs7QUFvQkU7RUFDRSw2REFBQTtBQWxCSjs7QUFxQkU7RUFDRSw2REFBQTtBQW5CSjs7QUF3QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxrQkFBQTtBQXJCRjs7QUF1QkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBckJKOztBQTBCQTtFQUNFLFdBQUE7RUFDQSx3RUFBQTtFQUNBLGNBQUE7QUF2QkY7O0FBMkJBO0VBQ0UsZ0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxPQUFBO0VBQ0EsYUFBQTtBQXhCRjs7QUE0QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGNBQUE7QUF6QkY7O0FBMkJFO0VBQ0UsU0FBQTtFQUNBLGNBQUE7QUF6Qko7O0FBNEJFO0VBQ0UsV0FBQTtFQUNBLGdCQUFBO0FBMUJKOztBQTZCSTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7QUEzQk47O0FBOEJJO0VBQ0UsZ0JBQUE7QUE1Qk47O0FBK0JJO0VBQ0UsYUFBQTtBQTdCTjs7QUFnQ0k7RUFDRSxlQUFBO0FBOUJOOztBQWlDSTtFQUNFLGNBQUE7QUEvQk47O0FBa0NJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBaENOOztBQXNDQTtFQUNFLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0EsT0FBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7QUFuQ0Y7O0FBcUNFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLG1CQUFBO0FBbkNKOztBQXNDRTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0EsaUJBQUE7QUFwQ0o7O0FBdUNFO0VBQ0UsZUFBQTtFQUNBLFNBQUE7QUFyQ0o7O0FBeUNBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtFQUNBLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsYUFBQTtBQXRDRjs7QUF5Q0U7RUFDRSxVQUFBO0FBdkNKOztBQTBDRTtFQUNFLG1CQUFBO0VBQ0Esa0JBQUE7QUF4Q0o7O0FBMkNFO0VBQ0UsbUJBQUE7RUFDQSxrQkFBQTtBQXpDSjs7QUEyQ0k7RUFDRSxtQkFBQTtBQXpDTjs7QUE4Q0E7RUFDRSxlQUFBO0VBQ0EsaURBQUE7RUFDQSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0EseUNBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsaUJBQUE7QUEzQ0Y7O0FBNkNFO0VBQ0UsMkJBQUE7RUFDQSx5Q0FBQTtFQUNBLHFCQUFBO0FBM0NKOztBQStDRTtFQUNFLHlCQUFBO0VBQ0EsK0NBQUE7RUFDQSxnR0FBQTtBQTdDSjs7QUErQ0k7RUFDRSwyQkFBQTtFQUNBLGdEQUFBO0VBQ0EscUJBQUE7QUE3Q047O0FBZ0RJO0VBQ0UsNkRBQUE7RUFDQSwrQ0FBQTtFQUNBLHdDQUFBO0FBOUNOOztBQWtERTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLDhCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFoREo7O0FBbURFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLE9BQUE7RUFDQSxZQUFBO0VBQ0EsZUFBQTtBQWpESjs7QUFvREU7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLDZEQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7QUFsREo7O0FBb0RJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsWUFBQTtBQWxETjs7QUFxREk7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLCtCQUFBO0FBbkROOztBQXVERTtFQUNFLE9BQUE7RUFDQSxZQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsUUFBQTtBQXJESjs7QUF1REk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7QUFyRE47O0FBd0RJO0VBQ0UsU0FBQTtFQUNBLGlCQUFBO0VBQ0EsY0FBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtBQXRETjs7QUF5REk7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsZUFBQTtBQXZETjs7QUEwREk7RUFDRSxvQkFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLDZEQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EseUJBQUE7RUFDQSxxQkFBQTtFQUNBLG1CQUFBO0VBQ0EsOENBQUE7RUFDQSxvQ0FBQTtFQUNBLGNBQUE7QUF4RE47O0FBMERNO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBeERSOztBQTRESTtFQUNFLFNBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7RUFDQSxrQkFBQTtFQUNBLE9BQUE7QUExRE47O0FBOERFO0VBQ0UsYUFBQTtFQUNBLFFBQUE7RUFDQSxjQUFBO0FBNURKOztBQThESTtFQUNFLHlCQUFBO0FBNUROOztBQThETTtFQUNFLDBDQUFBO0FBNURSOztBQTZEUTtFQUNFLGNBQUE7QUEzRFY7O0FBK0RNO0VBQ0UseUNBQUE7QUE3RFI7O0FBOERRO0VBQ0UsY0FBQTtBQTVEVjs7QUFnRU07RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsMkJBQUE7QUE5RFI7O0FBcUVBO0VBQ0UsbUJBQUE7QUFsRUY7O0FBc0VBO0VBQ0U7SUFDRSwwQkFBQTtJQUNBLFNBQUE7RUFuRUY7O0VBc0VBO0lBQ0UsZ0JBQUE7RUFuRUY7O0VBc0VBO0lBQ0UsYUFBQTtJQUNBLDJEQUFBO0lBQ0EsU0FBQTtFQW5FRjtBQUNGOztBQXNFQTtFQUNFO0lBQ0UsdUJBQUE7RUFwRUY7RUFzRUU7SUFDRSxlQUFBO0VBcEVKO0VBdUVFO0lBQ0UsZUFBQTtFQXJFSjs7RUF5RUE7SUFDRSxrQkFBQTtJQUNBLDRCQUFBO0VBdEVGOztFQXlFQTtJQUNFLDBCQUFBO0VBdEVGOztFQXlFQTtJQUNFLGtCQUFBO0VBdEVGOztFQXlFQTtJQUNFLGtCQUFBO0VBdEVGOztFQXlFQTtJQUNFLHNCQUFBO0lBQ0EsdUJBQUE7RUF0RUY7RUF3RUU7SUFDRSxlQUFBO0VBdEVKO0FBQ0Y7O0FBMkVBO0VBQ0U7SUFDRSxtQkFBQTtFQXpFRjtFQTJFQTtJQUNFLHNCQUFBO0VBekVGO0FBQ0Y7O0FBNEVBO0VBQ0U7SUFDRSxVQUFBO0lBQ0EscUJBQUE7RUExRUY7RUE0RUE7SUFDRSxVQUFBO0lBQ0EsbUJBQUE7RUExRUY7QUFDRiIsImZpbGUiOiJwcm9qZWN0cy5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi8vIE1vZGVybiBjb250YWluZXIgbGF5b3V0XHJcbi5tb2Rlcm4tY29udGFpbmVyIHtcclxuICBtaW4taGVpZ2h0OiAxMDB2aDtcclxuICBiYWNrZ3JvdW5kOiAjZjVmN2ZhO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBwYWRkaW5nOiAwO1xyXG4gIG92ZXJmbG93LXg6IGhpZGRlbjtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG59XHJcblxyXG4vLyBDb250ZW50IHdyYXBwZXJcclxuLmNvbnRlbnQtd3JhcHBlciB7XHJcbiAgZmxleDogMTtcclxuICBiYWNrZ3JvdW5kOiAjZjVmN2ZhO1xyXG4gIHBhZGRpbmc6IDQwcHg7XHJcbiAgbWF4LXdpZHRoOiAxNjAwcHg7XHJcbiAgbWFyZ2luOiAwIGF1dG87XHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgbWluLWhlaWdodDogY2FsYygxMDB2aCAtIDM2cHgpO1xyXG4gIGJveC1zaXppbmc6IGJvcmRlci1ib3g7XHJcbn1cclxuXHJcbi8vIE1haW4gbGF5b3V0IC0gVHdvIGNvbHVtbnNcclxuLm1haW4tbGF5b3V0IHtcclxuICBkaXNwbGF5OiBncmlkO1xyXG4gIGdyaWQtdGVtcGxhdGUtY29sdW1uczogMWZyIDQwMHB4O1xyXG4gIGdhcDogMzJweDtcclxuICBhbGlnbi1pdGVtczogc3RhcnQ7XHJcbiAgaGVpZ2h0OiBjYWxjKDEwMHZoIC0gMTE2cHgpOyAvLyAzNnB4IHRpdGxlIGJhciArIDgwcHggcGFkZGluZ1xyXG59XHJcblxyXG4ubGVmdC1jb2x1bW4ge1xyXG4gIG1pbi13aWR0aDogMDtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgaGVpZ2h0OiAxMDAlO1xyXG4gIG1heC1oZWlnaHQ6IDEwMCU7XHJcbn1cclxuXHJcbi5yaWdodC1jb2x1bW4ge1xyXG4gIHBvc2l0aW9uOiBzdGlja3k7XHJcbiAgdG9wOiA0MHB4O1xyXG4gIGFsaWduLXNlbGY6IHN0YXJ0O1xyXG59XHJcblxyXG4vLyBBY3Rpb24gY2FyZHMgZ3JpZCAoaW4gcmlnaHQgY29sdW1uIC0gdmVydGljYWwgc3RhY2spXHJcbi5hY3Rpb25zLWdyaWQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBnYXA6IDE2cHg7XHJcbn1cclxuXHJcbi8vIEFjdGlvbiBjYXJkIHN0eWxpbmdcclxuLmFjdGlvbi1jYXJkIHtcclxuICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgdHJhbnNpdGlvbjogYWxsIDAuM3MgY3ViaWMtYmV6aWVyKDAuNCwgMCwgMC4yLCAxKTtcclxuICBib3JkZXItcmFkaXVzOiAxNnB4O1xyXG4gIGJvcmRlcjogbm9uZTtcclxuICBib3gtc2hhZG93OiAwIDJweCA4cHggcmdiYSgwLCAwLCAwLCAwLjA4KTtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIGJhY2tncm91bmQ6IHdoaXRlO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtNHB4KTtcclxuICAgIGJveC1zaGFkb3c6IDAgMTJweCAyNHB4IHJnYmEoMCwgMCwgMCwgMC4xNSk7XHJcblxyXG4gICAgLmFycm93LWljb24ge1xyXG4gICAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVgoNHB4KTtcclxuICAgIH1cclxuXHJcbiAgICAuaWNvbi13cmFwcGVyIHtcclxuICAgICAgdHJhbnNmb3JtOiBzY2FsZSgxLjEpO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgJjphY3RpdmUge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKC0ycHgpO1xyXG4gIH1cclxuXHJcbiAgLmNhcmQtY29udGVudCB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogMTZweDtcclxuICAgIHBhZGRpbmc6IDIwcHggMTZweDtcclxuICAgIHBvc2l0aW9uOiByZWxhdGl2ZTtcclxuICB9XHJcblxyXG4gIC5pY29uLXdyYXBwZXIge1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgICB3aWR0aDogNDhweDtcclxuICAgIGhlaWdodDogNDhweDtcclxuICAgIGJvcmRlci1yYWRpdXM6IDEycHg7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gICAgdHJhbnNpdGlvbjogdHJhbnNmb3JtIDAuM3MgZWFzZTtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjRweDtcclxuICAgICAgd2lkdGg6IDI0cHg7XHJcbiAgICAgIGhlaWdodDogMjRweDtcclxuICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLmNhcmQtdGV4dCB7XHJcbiAgICBmbGV4OiAxO1xyXG5cclxuICAgIC5jYXJkLXRpdGxlIHtcclxuICAgICAgbWFyZ2luOiAwIDAgNHB4IDA7XHJcbiAgICAgIGZvbnQtc2l6ZTogMXJlbTtcclxuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICAgICAgY29sb3I6ICMyZDM3NDg7XHJcbiAgICB9XHJcblxyXG4gICAgLmNhcmQtZGVzY3JpcHRpb24ge1xyXG4gICAgICBtYXJnaW46IDA7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC43NXJlbTtcclxuICAgICAgY29sb3I6ICM3MTgwOTY7XHJcbiAgICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuY2FyZC1hY3Rpb24ge1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcblxyXG4gICAgLmFycm93LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIGNvbG9yOiAjYTBhZWMwO1xyXG4gICAgICB0cmFuc2l0aW9uOiB0cmFuc2Zvcm0gMC4zcyBlYXNlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLy8gU3BlY2lmaWMgY2FyZCBjb2xvcnNcclxuICAmLmNsb25lLWNhcmQgLmljb24td3JhcHBlciB7XHJcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xyXG4gIH1cclxuXHJcbiAgJi5jcmVhdGUtY2FyZCAuaWNvbi13cmFwcGVyIHtcclxuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICNmMDkzZmIgMCUsICNmNTU3NmMgMTAwJSk7XHJcbiAgfVxyXG5cclxuICAmLnNldHRpbmdzLWNhcmQgLmljb24td3JhcHBlciB7XHJcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNGZhY2ZlIDAlLCAjMDBmMmZlIDEwMCUpO1xyXG4gIH1cclxufVxyXG5cclxuLy8gU2VjdGlvbiB0aXRsZVxyXG4uc2VjdGlvbi10aXRsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTJweDtcclxuICBmb250LXNpemU6IDEuNXJlbTtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIGNvbG9yOiAjMmQzNzQ4O1xyXG4gIG1hcmdpbjogMCAwIDI0cHggMDtcclxuXHJcbiAgLnNlY3Rpb24taWNvbiB7XHJcbiAgICBmb250LXNpemU6IDI4cHg7XHJcbiAgICB3aWR0aDogMjhweDtcclxuICAgIGhlaWdodDogMjhweDtcclxuICAgIGNvbG9yOiAjNjY3ZWVhO1xyXG4gIH1cclxufVxyXG5cclxuLy8gU2VjdGlvbiBkaXZpZGVyXHJcbi5zZWN0aW9uLWRpdmlkZXIge1xyXG4gIGhlaWdodDogMXB4O1xyXG4gIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCh0byByaWdodCwgdHJhbnNwYXJlbnQsICNlMmU4ZjAsIHRyYW5zcGFyZW50KTtcclxuICBtYXJnaW46IDMycHggMDtcclxufVxyXG5cclxuLy8gUmVjZW50IFByb2plY3RzIFNlY3Rpb25cclxuLnJlY2VudC1wcm9qZWN0cy1zZWN0aW9uIHtcclxuICBtYXJnaW4tYm90dG9tOiAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBmbGV4OiAxO1xyXG4gIG1pbi1oZWlnaHQ6IDA7XHJcbn1cclxuXHJcbi8vIFNlY3Rpb24gaGVhZGVyIHdpdGggc2VhcmNoXHJcbi5zZWN0aW9uLWhlYWRlciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG4gIGdhcDogMjRweDtcclxuICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgLnNlY3Rpb24tdGl0bGUge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgfVxyXG5cclxuICAuc2VhcmNoLWZpZWxkIHtcclxuICAgIHdpZHRoOiAxMDAlO1xyXG4gICAgbWF4LXdpZHRoOiA0MDBweDtcclxuXHJcbiAgICAvLyBPdmVycmlkZSBNYXRlcmlhbCBEZXNpZ24gZGVmYXVsdCBtYXJnaW5zXHJcbiAgICAubWF0LWZvcm0tZmllbGQtd3JhcHBlciB7XHJcbiAgICAgIHBhZGRpbmctYm90dG9tOiAwO1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiAwO1xyXG4gICAgfVxyXG5cclxuICAgIC5tYXQtZm9ybS1maWVsZC1pbmZpeCB7XHJcbiAgICAgIHBhZGRpbmc6IDAuNWVtIDA7XHJcbiAgICB9XHJcblxyXG4gICAgLm1hdC1mb3JtLWZpZWxkLXVuZGVybGluZSB7XHJcbiAgICAgIGRpc3BsYXk6IG5vbmU7XHJcbiAgICB9XHJcblxyXG4gICAgaW5wdXQge1xyXG4gICAgICBmb250LXNpemU6IDE0cHg7XHJcbiAgICB9XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBjb2xvcjogIzcxODA5NjtcclxuICAgIH1cclxuXHJcbiAgICBidXR0b24gbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBObyBwcm9qZWN0cyBtZXNzYWdlXHJcbi5uby1wcm9qZWN0cy1tZXNzYWdlIHtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNjBweCAyMHB4O1xyXG4gIGNvbG9yOiAjNzE4MDk2O1xyXG4gIGZsZXg6IDE7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcblxyXG4gIC5lbXB0eS1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogODBweDtcclxuICAgIHdpZHRoOiA4MHB4O1xyXG4gICAgaGVpZ2h0OiA4MHB4O1xyXG4gICAgY29sb3I6ICNjYmQ1ZTA7XHJcbiAgICBtYXJnaW46IDAgYXV0byAyMHB4O1xyXG4gIH1cclxuXHJcbiAgaDMge1xyXG4gICAgZm9udC1zaXplOiAxLjVyZW07XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgY29sb3I6ICM0YTU1Njg7XHJcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgZm9udC1zaXplOiAxcmVtO1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gIH1cclxufVxyXG5cclxuLnByb2plY3RzLWdyaWQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBnYXA6IDEycHg7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIHBhZGRpbmctcmlnaHQ6IDhweDtcclxuICBtaW4taGVpZ2h0OiAwO1xyXG5cclxuICAvLyBDdXN0b20gc2Nyb2xsYmFyXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXIge1xyXG4gICAgd2lkdGg6IDhweDtcclxuICB9XHJcblxyXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRyYWNrIHtcclxuICAgIGJhY2tncm91bmQ6ICNlMmU4ZjA7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgfVxyXG5cclxuICAmOjotd2Via2l0LXNjcm9sbGJhci10aHVtYiB7XHJcbiAgICBiYWNrZ3JvdW5kOiAjY2JkNWUwO1xyXG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG5cclxuICAgICY6aG92ZXIge1xyXG4gICAgICBiYWNrZ3JvdW5kOiAjYTBhZWMwO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLnByb2plY3QtY2FyZCB7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjNzIGN1YmljLWJlemllcigwLjQsIDAsIDAuMiwgMSk7XHJcbiAgYm9yZGVyLXJhZGl1czogMTJweDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTJlOGYwO1xyXG4gIGJveC1zaGFkb3c6IDAgMXB4IDNweCByZ2JhKDAsIDAsIDAsIDAuMDUpO1xyXG4gIGJhY2tncm91bmQ6IHdoaXRlO1xyXG4gIHBvc2l0aW9uOiByZWxhdGl2ZTtcclxuICBvdmVyZmxvdzogdmlzaWJsZTtcclxuXHJcbiAgJjpob3ZlciB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XHJcbiAgICBib3gtc2hhZG93OiAwIDhweCAxNnB4IHJnYmEoMCwgMCwgMCwgMC4xKTtcclxuICAgIGJvcmRlci1jb2xvcjogIzY2N2VlYTtcclxuICB9XHJcblxyXG4gIC8vIFNwZWNpYWwgc3R5bGluZyBmb3IgbGFzdCBvcGVuZWQgcHJvamVjdFxyXG4gICYubGFzdC1vcGVuZWQge1xyXG4gICAgYm9yZGVyOiAycHggc29saWQgIzY2N2VlYTtcclxuICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjIpO1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjAzKSAwJSwgcmdiYSgxMTgsIDc1LCAxNjIsIDAuMDMpIDEwMCUpO1xyXG5cclxuICAgICY6aG92ZXIge1xyXG4gICAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTRweCk7XHJcbiAgICAgIGJveC1zaGFkb3c6IDAgMTJweCAyNHB4IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4zKTtcclxuICAgICAgYm9yZGVyLWNvbG9yOiAjNzY0YmEyO1xyXG4gICAgfVxyXG5cclxuICAgIC5wcm9qZWN0LWljb24td3JhcHBlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjQpO1xyXG4gICAgICBhbmltYXRpb246IHB1bHNlIDJzIGVhc2UtaW4tb3V0IGluZmluaXRlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtY2FyZC1jb250ZW50IHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xyXG4gICAgcGFkZGluZzogMTZweDtcclxuICAgIGdhcDogMTJweDtcclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWluZm8ge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDEycHg7XHJcbiAgICBmbGV4OiAxO1xyXG4gICAgbWluLXdpZHRoOiAwO1xyXG4gICAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtaWNvbi13cmFwcGVyIHtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgd2lkdGg6IDQwcHg7XHJcbiAgICBoZWlnaHQ6IDQwcHg7XHJcbiAgICBib3JkZXItcmFkaXVzOiAxMHB4O1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcblxyXG4gICAgLnByb2plY3QtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgfVxyXG5cclxuICAgIC5wcm9qZWN0LWljb24tc3ZnIHtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgZmlsdGVyOiBicmlnaHRuZXNzKDApIGludmVydCgxKTsgLy8gTWFrZXMgU1ZHIHdoaXRlXHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1kZXRhaWxzIHtcclxuICAgIGZsZXg6IDE7XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGdhcDogNHB4O1xyXG5cclxuICAgIC5wcm9qZWN0LW5hbWUge1xyXG4gICAgICBtYXJnaW46IDA7XHJcbiAgICAgIGZvbnQtc2l6ZTogMS4xcmVtO1xyXG4gICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICBjb2xvcjogIzJkMzc0ODtcclxuICAgICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcclxuICAgICAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICAgICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgICB9XHJcblxyXG4gICAgLnByb2plY3QtcGF0aCB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAwLjhyZW07XHJcbiAgICAgIGNvbG9yOiAjNzE4MDk2O1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1tZXRhIHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgZ2FwOiAxMnB4O1xyXG4gICAgICBtYXJnaW4tdG9wOiA0cHg7XHJcbiAgICB9XHJcblxyXG4gICAgLmxhc3Qtb3BlbmVkLWJhZGdlIHtcclxuICAgICAgZGlzcGxheTogaW5saW5lLWZsZXg7XHJcbiAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICAgIGdhcDogNHB4O1xyXG4gICAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xyXG4gICAgICBjb2xvcjogd2hpdGU7XHJcbiAgICAgIHBhZGRpbmc6IDRweCAxMHB4O1xyXG4gICAgICBib3JkZXItcmFkaXVzOiAxMnB4O1xyXG4gICAgICBmb250LXNpemU6IDAuNjVyZW07XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA2MDA7XHJcbiAgICAgIHRleHQtdHJhbnNmb3JtOiB1cHBlcmNhc2U7XHJcbiAgICAgIGxldHRlci1zcGFjaW5nOiAwLjVweDtcclxuICAgICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcclxuICAgICAgYm94LXNoYWRvdzogMCAycHggNnB4IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4zKTtcclxuICAgICAgYW5pbWF0aW9uOiBmYWRlSW5TY2FsZSAwLjVzIGVhc2Utb3V0O1xyXG4gICAgICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgICAgIC5iYWRnZS1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICAgICAgd2lkdGg6IDEycHg7XHJcbiAgICAgICAgaGVpZ2h0OiAxMnB4O1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLnByb2plY3QtZGF0ZSB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAwLjdyZW07XHJcbiAgICAgIGNvbG9yOiAjYTBhZWMwO1xyXG4gICAgICBmb250LXN0eWxlOiBpdGFsaWM7XHJcbiAgICAgIGZsZXg6IDE7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1hY3Rpb25zIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBnYXA6IDRweDtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG5cclxuICAgIC5hY3Rpb24tYnRuIHtcclxuICAgICAgdHJhbnNpdGlvbjogYWxsIDAuMnMgZWFzZTtcclxuXHJcbiAgICAgICYuc2V0dGluZ3MtYnRuOmhvdmVyIHtcclxuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMSk7XHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgY29sb3I6ICM2NjdlZWE7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcblxyXG4gICAgICAmLmRlbGV0ZS1idG46aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMjQ1LCA4NywgMTA4LCAwLjEpO1xyXG4gICAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICAgIGNvbG9yOiAjZjU1NzZjO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgICB3aWR0aDogMjBweDtcclxuICAgICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgICAgY29sb3I6ICNhMGFlYzA7XHJcbiAgICAgICAgdHJhbnNpdGlvbjogY29sb3IgMC4ycyBlYXNlO1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTaWRlYmFyIHNlY3Rpb24gKGhpZGRlbiBieSBkZWZhdWx0KVxyXG4uc2lkZWJhci1zZWN0aW9uIHtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG59XHJcblxyXG4vLyBSZXNwb25zaXZlIGRlc2lnblxyXG5AbWVkaWEgKG1heC13aWR0aDogMTAyNHB4KSB7XHJcbiAgLm1haW4tbGF5b3V0IHtcclxuICAgIGdyaWQtdGVtcGxhdGUtY29sdW1uczogMWZyO1xyXG4gICAgZ2FwOiAzMnB4O1xyXG4gIH1cclxuXHJcbiAgLnJpZ2h0LWNvbHVtbiB7XHJcbiAgICBwb3NpdGlvbjogc3RhdGljO1xyXG4gIH1cclxuXHJcbiAgLmFjdGlvbnMtZ3JpZCB7XHJcbiAgICBkaXNwbGF5OiBncmlkO1xyXG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiByZXBlYXQoYXV0by1maXQsIG1pbm1heCgyODBweCwgMWZyKSk7XHJcbiAgICBnYXA6IDE2cHg7XHJcbiAgfVxyXG59XHJcblxyXG5AbWVkaWEgKG1heC13aWR0aDogNzY4cHgpIHtcclxuICAuaGVhZGVyLXNlY3Rpb24ge1xyXG4gICAgcGFkZGluZzogNDBweCAyMHB4IDIwcHg7XHJcblxyXG4gICAgLm1haW4tdGl0bGUge1xyXG4gICAgICBmb250LXNpemU6IDJyZW07XHJcbiAgICB9XHJcblxyXG4gICAgLnN1YnRpdGxlIHtcclxuICAgICAgZm9udC1zaXplOiAxcmVtO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLmNvbnRlbnQtd3JhcHBlciB7XHJcbiAgICBwYWRkaW5nOiAzMnB4IDIwcHg7XHJcbiAgICBib3JkZXItcmFkaXVzOiAyNHB4IDI0cHggMCAwO1xyXG4gIH1cclxuXHJcbiAgLmFjdGlvbnMtZ3JpZCB7XHJcbiAgICBncmlkLXRlbXBsYXRlLWNvbHVtbnM6IDFmcjtcclxuICB9XHJcblxyXG4gIC5hY3Rpb24tY2FyZCAuY2FyZC1jb250ZW50IHtcclxuICAgIHBhZGRpbmc6IDI0cHggMjBweDtcclxuICB9XHJcblxyXG4gIC5zZWN0aW9uLXRpdGxlIHtcclxuICAgIGZvbnQtc2l6ZTogMS4yNXJlbTtcclxuICB9XHJcblxyXG4gIC5zZWN0aW9uLWhlYWRlciB7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gICAgYWxpZ24taXRlbXM6IGZsZXgtc3RhcnQ7XHJcblxyXG4gICAgLnNlYXJjaC1maWVsZCB7XHJcbiAgICAgIG1heC13aWR0aDogMTAwJTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi8vIEFuaW1hdGlvbnNcclxuQGtleWZyYW1lcyBwdWxzZSB7XHJcbiAgMCUsIDEwMCUge1xyXG4gICAgdHJhbnNmb3JtOiBzY2FsZSgxKTtcclxuICB9XHJcbiAgNTAlIHtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMS4wNSk7XHJcbiAgfVxyXG59XHJcblxyXG5Aa2V5ZnJhbWVzIGZhZGVJblNjYWxlIHtcclxuICAwJSB7XHJcbiAgICBvcGFjaXR5OiAwO1xyXG4gICAgdHJhbnNmb3JtOiBzY2FsZSgwLjgpO1xyXG4gIH1cclxuICAxMDAlIHtcclxuICAgIG9wYWNpdHk6IDE7XHJcbiAgICB0cmFuc2Zvcm06IHNjYWxlKDEpO1xyXG4gIH1cclxufVxyXG4iXX0= */"] });


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
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ "DEjE");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ "eAi6");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");




















function ModernCloneProjectComponent_mat_hint_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](1, "GitHub repository detected");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} }
function ModernCloneProjectComponent_div_10_div_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](2, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate"](ctx_r3.tokenStatus);
} }
function ModernCloneProjectComponent_div_10_div_7_Template(rf, ctx) { if (rf & 1) {
    const _r6 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "div", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](5, "button", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function ModernCloneProjectComponent_div_10_div_7_Template_button_click_5_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r6); const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](2); return ctx_r5.openTokenSettings(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](6, " Configure Token ");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate"](ctx_r4.tokenStatus);
} }
function ModernCloneProjectComponent_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "mat-card", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](2, "mat-card-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](3, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](4, "Authentication");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](5, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](6, ModernCloneProjectComponent_div_10_div_6_Template, 5, 1, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](7, ModernCloneProjectComponent_div_10_div_7_Template, 7, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngIf", ctx_r1.hasGitHubToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngIf", !ctx_r1.hasGitHubToken);
} }
function ModernCloneProjectComponent_div_11_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "mat-card", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](2, "mat-card-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](3, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](4, "Authentication");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](5, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](6, "mat-form-field", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](7, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](8, "Username");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](9, "input", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_div_11_Template_input_ngModelChange_9_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r8); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r7.manualCredentials.username = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](10, "mat-form-field", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](11, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](12, "Password / Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](13, "input", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_div_11_Template_input_ngModelChange_13_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r8); const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r9.manualCredentials.password = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngModel", ctx_r2.manualCredentials.username);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngModel", ctx_r2.manualCredentials.password);
} }
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
        this.isGitHubRepo = false;
        this.authMethod = 'automatic';
        // For manual authentication (non-GitHub repos)
        this.manualCredentials = {
            username: '',
            password: ''
        };
    }
    ngOnInit() {
        // Get URL from clipboard
        this.mdFileService.getTextFromClipboard().subscribe(clipboard => {
            if (clipboard === null || clipboard === void 0 ? void 0 : clipboard.url) {
                this.cloneRequest.url = clipboard.url;
                this.checkIfGitHubRepo();
            }
        });
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
            if (response.hasToken) {
                this.tokenStatus = `Token configured: ${response.maskedToken}`;
            }
            else {
                this.tokenStatus = 'No GitHub token configured';
            }
        });
    }
    onUrlChange() {
        this.checkIfGitHubRepo();
    }
    checkIfGitHubRepo() {
        const url = this.cloneRequest.url.toLowerCase();
        this.isGitHubRepo = url.includes('github.com');
        // Auto-select authentication method based on URL
        if (this.isGitHubRepo && this.hasGitHubToken) {
            this.authMethod = 'automatic';
        }
        else if (!this.isGitHubRepo) {
            this.authMethod = 'manual';
        }
    }
    openFileSystem() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_5__["ShowFileMetadata"]();
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
            info.message = "Cloning repository...";
            this.waitingDialog.showMessageBox(info);
            try {
                // Use modern clone endpoint
                const request = {
                    url: this.cloneRequest.url,
                    localPath: this.cloneRequest.localPath,
                    branchName: this.cloneRequest.branchName || null
                };
                // Log the request for debugging
                console.log('[ModernClone] Sending clone request:', request);
                // Call the modern Git service clone method
                this.gitService.modernClone(request).subscribe(response => {
                    this.waitingDialog.closeMessageBox();
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
                    this.showMessage(((_a = error.error) === null || _a === void 0 ? void 0 : _a.error) || 'Clone failed: ' + error.message);
                });
            }
            catch (error) {
                this.waitingDialog.closeMessageBox();
                this.showMessage('Unexpected error during clone');
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
        // TODO: Open settings dialog to configure GitHub token
        this.showMessage('Please configure your GitHub Personal Access Token in Settings');
    }
    cancel() {
        this.dialogRef.close();
    }
}
ModernCloneProjectComponent.ɵfac = function ModernCloneProjectComponent_Factory(t) { return new (t || ModernCloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_8__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_9__["WaitingDialogService"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_10__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_11__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MAT_DIALOG_DATA"])); };
ModernCloneProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdefineComponent"]({ type: ModernCloneProjectComponent, selectors: [["app-modern-clone-project"]], decls: 32, vars: 7, consts: [[1, "modern-clone-dialog"], ["mat-dialog-title", ""], [1, "clone-form"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "https://github.com/user/repository.git", "required", "", 3, "ngModel", "ngModelChange"], [4, "ngIf"], ["class", "auth-section", 4, "ngIf"], ["matInput", "", "placeholder", "Select destination folder", "required", "", "readonly", "", 3, "ngModel", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], ["matInput", "", "placeholder", "main", 3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], [1, "auth-section"], [1, "auth-card"], ["class", "token-status", 4, "ngIf"], ["class", "token-status warn", 4, "ngIf"], [1, "token-status"], ["color", "primary"], [1, "token-status", "warn"], ["color", "warn"], ["mat-button", "", "color", "primary", 3, "click"], ["matInput", "", 3, "ngModel", "ngModelChange"], ["matInput", "", "type", "password", 3, "ngModel", "ngModelChange"]], template: function ModernCloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "h2", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](2, "Clone Repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](3, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](4, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](5, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](6, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](7, "Repository URL");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](8, "input", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_8_listener($event) { return ctx.cloneRequest.url = $event; })("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_8_listener() { return ctx.onUrlChange(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](9, ModernCloneProjectComponent_mat_hint_9_Template, 2, 0, "mat-hint", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](10, ModernCloneProjectComponent_div_10_Template, 8, 2, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](11, ModernCloneProjectComponent_div_11_Template, 14, 2, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](12, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](13, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](14, "Clone to");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](15, "input", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_15_listener($event) { return ctx.cloneRequest.localPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](16, "button", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_16_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](17, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](18, "folder_open");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](19, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](20, "The repository will be cloned to this location");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](21, "mat-form-field", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](22, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](23, "Branch (optional)");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](24, "input", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("ngModelChange", function ModernCloneProjectComponent_Template_input_ngModelChange_24_listener($event) { return ctx.cloneRequest.branchName = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](25, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](26, "Leave empty to clone the default branch");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](27, "mat-dialog-actions", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](28, "button", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_28_listener() { return ctx.cancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](29, "Cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](30, "button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function ModernCloneProjectComponent_Template_button_click_30_listener() { return ctx.performClone(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](31, " Clone Repository ");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngModel", ctx.cloneRequest.url);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngIf", ctx.isGitHubRepo);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngIf", ctx.isGitHubRepo);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngIf", !ctx.isGitHubRepo && ctx.authMethod === "manual");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngModel", ctx.cloneRequest.localPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngModel", ctx.cloneRequest.branchName);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("disabled", !ctx.cloneRequest.url || !ctx.cloneRequest.localPath);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_13__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__["NgModel"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_16__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatSuffix"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogActions"], _angular_material_card__WEBPACK_IMPORTED_MODULE_18__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_18__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_18__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_18__["MatCardContent"]], styles: [".modern-clone-dialog[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 700px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-content[_ngcontent-%COMP%] {\n  padding: 20px;\n  max-height: 70vh;\n  overflow-y: auto;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .clone-form[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%] {\n  margin: 8px 0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%] {\n  background-color: #f5f5f5;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%] {\n  padding-bottom: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  font-size: 16px;\n  font-weight: 500;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .auth-section[_ngcontent-%COMP%]   .auth-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  padding-top: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 8px;\n  border-radius: 4px;\n  background-color: rgba(33, 150, 243, 0.1);\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status.warn[_ngcontent-%COMP%] {\n  background-color: rgba(255, 152, 0, 0.1);\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   .token-status[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  border-top: 1px solid #e0e0e0;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-left: 8px;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-form-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 1.25em;\n}\n.modern-clone-dialog[_ngcontent-%COMP%]   mat-hint[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #666;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbW9kZXJuLWNsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0Y7QUFDRTtFQUNFLGFBQUE7RUFDQSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0o7QUFFRTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7QUFBSjtBQUdFO0VBQ0UsV0FBQTtBQURKO0FBSUU7RUFDRSxhQUFBO0FBRko7QUFJSTtFQUNFLHlCQUFBO0FBRk47QUFJTTtFQUNFLG1CQUFBO0FBRlI7QUFJUTtFQUNFLGVBQUE7RUFDQSxnQkFBQTtBQUZWO0FBTU07RUFDRSxnQkFBQTtBQUpSO0FBU0U7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EseUNBQUE7QUFQSjtBQVNJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBUE47QUFVSTtFQUNFLHdDQUFBO0FBUk47QUFXSTtFQUNFLGlCQUFBO0FBVE47QUFhRTtFQUNFLGtCQUFBO0VBQ0EsNkJBQUE7QUFYSjtBQWFJO0VBQ0UsZ0JBQUE7QUFYTjtBQWdCSTtFQUNFLHNCQUFBO0FBZE47QUFrQkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtBQWhCSiIsImZpbGUiOiJtb2Rlcm4tY2xvbmUtcHJvamVjdC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5tb2Rlcm4tY2xvbmUtZGlhbG9nIHtcclxuICBtaW4td2lkdGg6IDUwMHB4O1xyXG4gIG1heC13aWR0aDogNzAwcHg7XHJcblxyXG4gIG1hdC1kaWFsb2ctY29udGVudCB7XHJcbiAgICBwYWRkaW5nOiAyMHB4O1xyXG4gICAgbWF4LWhlaWdodDogNzB2aDtcclxuICAgIG92ZXJmbG93LXk6IGF1dG87XHJcbiAgfVxyXG5cclxuICAuY2xvbmUtZm9ybSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGdhcDogMTZweDtcclxuICB9XHJcblxyXG4gIC5mdWxsLXdpZHRoIHtcclxuICAgIHdpZHRoOiAxMDAlO1xyXG4gIH1cclxuXHJcbiAgLmF1dGgtc2VjdGlvbiB7XHJcbiAgICBtYXJnaW46IDhweCAwO1xyXG5cclxuICAgIC5hdXRoLWNhcmQge1xyXG4gICAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG5cclxuICAgICAgbWF0LWNhcmQtaGVhZGVyIHtcclxuICAgICAgICBwYWRkaW5nLWJvdHRvbTogOHB4O1xyXG5cclxuICAgICAgICBtYXQtY2FyZC10aXRsZSB7XHJcbiAgICAgICAgICBmb250LXNpemU6IDE2cHg7XHJcbiAgICAgICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgbWF0LWNhcmQtY29udGVudCB7XHJcbiAgICAgICAgcGFkZGluZy10b3A6IDhweDtcclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnRva2VuLXN0YXR1cyB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogOHB4O1xyXG4gICAgcGFkZGluZzogOHB4O1xyXG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgzMywgMTUwLCAyNDMsIDAuMSk7XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICB9XHJcblxyXG4gICAgJi53YXJuIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNTUsIDE1MiwgMCwgMC4xKTtcclxuICAgIH1cclxuXHJcbiAgICBidXR0b24ge1xyXG4gICAgICBtYXJnaW4tbGVmdDogYXV0bztcclxuICAgIH1cclxuICB9XHJcblxyXG4gIG1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcbiAgICBib3JkZXItdG9wOiAxcHggc29saWQgI2UwZTBlMDtcclxuXHJcbiAgICBidXR0b24ge1xyXG4gICAgICBtYXJnaW4tbGVmdDogOHB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgbWF0LWZvcm0tZmllbGQge1xyXG4gICAgLm1hdC1mb3JtLWZpZWxkLXdyYXBwZXIge1xyXG4gICAgICBwYWRkaW5nLWJvdHRvbTogMS4yNWVtO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgbWF0LWhpbnQge1xyXG4gICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgfVxyXG59Il19 */"] });


/***/ })

}]);
//# sourceMappingURL=projects-projects-module.js.map