"use strict";
(self["webpackChunkclient2"] = self["webpackChunkclient2"] || []).push([["src_app_projects_projects_module_ts"],{

/***/ 7179:
/*!***************************************************************************!*\
  !*** ./src/app/projects/dialogs/clone-project/clone-project.component.ts ***!
  \***************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "CloneProjectComponent": () => (/* binding */ CloneProjectComponent)
/* harmony export */ });
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo */ 1775);
/* harmony import */ var _git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../git/components/git-messages/git-messages.component */ 2055);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/dialog */ 5758);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ 7224);
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ 1394);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/router */ 2816);
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/card */ 1961);
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/form-field */ 4770);
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/input */ 3365);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ 587);
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/button */ 7317);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/icon */ 5590);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/common */ 6362);
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/checkbox */ 1534);



















function CloneProjectComponent_mat_hint_35_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1, "Enter your password");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
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
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_1__.WaitingDialogInfo();
        info.message = "Oh MY GOD... Cloning Repository!";
        this.waitingDialog.showMessageBox(info);
        this.gitService.clone(this.dataForCloning).subscribe(_ => {
            if (_.areCredentialsCorrect) {
                this.projectService.setNewFolderProject(this.dataForCloning.directoryPath);
            }
            else {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_2__.GitMessagesComponent, {
                    width: '300px',
                    data: { message: "Credentials are not correct" }
                });
            }
            this.waitingDialog.closeMessageBox();
            this.dialogRef.close(this.dataForCloning);
        });
    }
    openFileSystem() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_3__.ShowFileMetadata();
        data.start = null;
        data.title = "C:\ Folder";
        data.typeOfSelection = "Folders";
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__.ShowFileSystemComponent, {
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
CloneProjectComponent.ɵfac = function CloneProjectComponent_Factory(t) { return new (t || CloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__.MatDialog), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__.MatDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_6__.WaitingDialogService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_10__.Router)); };
CloneProjectComponent.ɵcmp = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({ type: CloneProjectComponent, selectors: [["app-clone-project"]], decls: 42, vars: 8, consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/gitlogo.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "gitlab url", "required", "", 3, "ngModel", "ngModelChange"], [1, "orizzontal-form-container"], ["appearance", "outline", 2, "width", "100%"], ["matInput", "", "placeholder", "local filesystem", "required", "", 3, "ngModel", "ngModelChange"], ["mat-button", "", "matSuffix", "", "color", "primary", 3, "click"], ["matInput", "", "placeholder", "username", "required", "", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "password", "required", "", 3, "ngModel", "type", "ngModelChange"], ["matSuffix", "", 3, "click"], [4, "ngIf"], [1, "store-credential"], [3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-stroked-button", "", "color", "primary", 3, "click"]], template: function CloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "h1", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "img", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "Clone Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "mat-dialog-content")(5, "mat-card", 2)(6, "div", 3)(7, "mat-form-field", 4)(8, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](9, "URL");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "input", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_10_listener($event) { return ctx.dataForCloning.urlPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](11, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](12, "Set GIT Path");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "div", 6)(14, "mat-form-field", 7)(15, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](16, "Directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_17_listener($event) { return ctx.dataForCloning.directoryPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](18, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](19, "Set project directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](20, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_20_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](21, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](22, "more_horiz");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](23, "mat-form-field", 4)(24, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](25, "UserName");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](26, "input", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_26_listener($event) { return ctx.dataForCloning.username = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](27, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](28, "Username");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](29, "mat-form-field", 4)(30, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](31, "Password");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](32, "input", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_32_listener($event) { return ctx.dataForCloning.password = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](33, "mat-icon", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_mat_icon_click_33_listener() { return ctx.hide = !ctx.hide; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](34);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](35, CloneProjectComponent_mat_hint_35_Template, 2, 0, "mat-hint", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](36, "section", 14)(37, "mat-checkbox", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_mat_checkbox_ngModelChange_37_listener($event) { return ctx.dataForCloning.storeCredentials = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](38, "Store credentials");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](39, "mat-dialog-actions", 16)(40, "button", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_40_listener() { return ctx.cloneDirectory(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](41, "Clone!");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.urlPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.directoryPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.username);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.password)("type", ctx.hide ? "password" : "text");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx.hide ? "visibility_off" : "visibility");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.dataForCloning.password);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.storeCredentials);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__.MatDialogTitle, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__.MatDialogContent, _angular_material_card__WEBPACK_IMPORTED_MODULE_11__.MatCard, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__.MatFormField, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__.MatLabel, _angular_material_input__WEBPACK_IMPORTED_MODULE_13__.MatInput, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgModel, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__.MatHint, _angular_material_button__WEBPACK_IMPORTED_MODULE_15__.MatButton, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_12__.MatSuffix, _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__.MatIcon, _angular_common__WEBPACK_IMPORTED_MODULE_17__.NgIf, _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_18__.MatCheckbox, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_9__.MatDialogActions], styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.orizzontal-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbImNsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtBQUNGIiwiZmlsZSI6ImNsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIudmVydGljYWwtZm9ybS1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxufVxyXG5cclxuLm9yaXp6b250YWwtZm9ybS1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IHJvdztcclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ 983:
/*!**************************************************************************************************!*\
  !*** ./src/app/projects/dialogs/project-create-config/project-create-config-dialog.component.ts ***!
  \**************************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectCreateConfigDialogComponent": () => (/* binding */ ProjectCreateConfigDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/dialog */ 5758);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/icon */ 5590);
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/divider */ 9975);
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/checkbox */ 1534);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/forms */ 587);
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ 7317);








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
ProjectCreateConfigDialogComponent.ɵfac = function ProjectCreateConfigDialogComponent_Factory(t) { return new (t || ProjectCreateConfigDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__.MatDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__.MAT_DIALOG_DATA)); };
ProjectCreateConfigDialogComponent.ɵcmp = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: ProjectCreateConfigDialogComponent, selectors: [["app-project-create-config-dialog"]], decls: 32, vars: 3, consts: [["mat-dialog-title", ""], [1, "config-container"], [1, "project-path"], [1, "path-text"], [1, "config-options"], [1, "option-item"], ["color", "primary", 3, "ngModel", "ngModelChange"], [1, "option-description"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"]], template: function ProjectCreateConfigDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, "Configure New Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "mat-dialog-content")(3, "div", 1)(4, "div", 2)(5, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6, "folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8, "Project Path:");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "span", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](11, "mat-divider");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "div", 4)(13, "div", 5)(14, "mat-checkbox", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_14_listener($event) { return ctx.config.addCopilotInstructions = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](16, "Add GitHub Copilot instructions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18, " Creates .github/copilot-instructions.md file with project-specific AI guidelines ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](19, "div", 5)(20, "mat-checkbox", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_20_listener($event) { return ctx.config.initializeGit = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](21, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](22, "Initialize Git repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](24, " Creates a new Git repository with .gitignore configured for MdExplorer ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](25, "mat-dialog-actions", 8)(26, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_26_listener() { return ctx.onCancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](27, "Cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](28, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_28_listener() { return ctx.onCreateProject(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](29, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](30, "add");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](31, " Create Project ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.config.projectPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.config.addCopilotInstructions);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.config.initializeGit);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__.MatDialogTitle, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__.MatDialogContent, _angular_material_icon__WEBPACK_IMPORTED_MODULE_2__.MatIcon, _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__.MatDivider, _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_4__.MatCheckbox, _angular_forms__WEBPACK_IMPORTED_MODULE_5__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_5__.NgModel, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__.MatDialogActions, _angular_material_button__WEBPACK_IMPORTED_MODULE_6__.MatButton], styles: [".config-container[_ngcontent-%COMP%] {\n  min-width: 400px;\n  padding: 20px 0;\n}\n\n.project-path[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 15px;\n  background-color: rgba(0, 0, 0, 0.04);\n  border-radius: 4px;\n  margin-bottom: 20px;\n}\n\n.project-path[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #666;\n}\n\n.project-path[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  font-family: monospace;\n  color: #444;\n  word-break: break-all;\n}\n\n.config-options[_ngcontent-%COMP%] {\n  padding: 20px 0;\n}\n\n.option-item[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n}\n\n.option-item[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n}\n\n.option-item[_ngcontent-%COMP%]   .option-description[_ngcontent-%COMP%] {\n  margin-top: 5px;\n  margin-left: 28px;\n  font-size: 12px;\n  color: #666;\n  line-height: 1.4;\n}\n\nmat-divider[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbInByb2plY3QtY3JlYXRlLWNvbmZpZy1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxnQkFBQTtFQUNBLGVBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EscUNBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBQ0Y7O0FBQ0U7RUFDRSxXQUFBO0FBQ0o7O0FBRUU7RUFDRSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxxQkFBQTtBQUFKOztBQUlBO0VBQ0UsZUFBQTtBQURGOztBQUlBO0VBQ0UsbUJBQUE7QUFERjs7QUFHRTtFQUNFLGNBQUE7QUFESjs7QUFJRTtFQUNFLGVBQUE7RUFDQSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFGSjs7QUFNQTtFQUNFLGNBQUE7QUFIRjs7QUFNQTtFQUNFLGtCQUFBO0FBSEY7O0FBTUk7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQUpOIiwiZmlsZSI6InByb2plY3QtY3JlYXRlLWNvbmZpZy1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuY29uZmlnLWNvbnRhaW5lciB7XHJcbiAgbWluLXdpZHRoOiA0MDBweDtcclxuICBwYWRkaW5nOiAyMHB4IDA7XHJcbn1cclxuXHJcbi5wcm9qZWN0LXBhdGgge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDEwcHg7XHJcbiAgcGFkZGluZzogMTVweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuMDQpO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICB9XHJcblxyXG4gIC5wYXRoLXRleHQge1xyXG4gICAgZm9udC1mYW1pbHk6IG1vbm9zcGFjZTtcclxuICAgIGNvbG9yOiAjNDQ0O1xyXG4gICAgd29yZC1icmVhazogYnJlYWstYWxsO1xyXG4gIH1cclxufVxyXG5cclxuLmNvbmZpZy1vcHRpb25zIHtcclxuICBwYWRkaW5nOiAyMHB4IDA7XHJcbn1cclxuXHJcbi5vcHRpb24taXRlbSB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjBweDtcclxuXHJcbiAgbWF0LWNoZWNrYm94IHtcclxuICAgIGRpc3BsYXk6IGJsb2NrO1xyXG4gIH1cclxuXHJcbiAgLm9wdGlvbi1kZXNjcmlwdGlvbiB7XHJcbiAgICBtYXJnaW4tdG9wOiA1cHg7XHJcbiAgICBtYXJnaW4tbGVmdDogMjhweDtcclxuICAgIGZvbnQtc2l6ZTogMTJweDtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuNDtcclxuICB9XHJcbn1cclxuXHJcbm1hdC1kaXZpZGVyIHtcclxuICBtYXJnaW46IDIwcHggMDtcclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcblxyXG4gIGJ1dHRvbiB7XHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xyXG4gICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICB9XHJcbiAgfVxyXG59Il19 */"] });


/***/ }),

/***/ 7111:
/*!***************************************************************!*\
  !*** ./src/app/projects/open-recent/open-recent.component.ts ***!
  \***************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "OpenRecentComponent": () => (/* binding */ OpenRecentComponent)
/* harmony export */ });
/* harmony import */ var angular_animations__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! angular-animations */ 9862);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../project-settings/project-settings.component */ 2482);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ 2816);
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/dialog */ 5758);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ 6362);
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/card */ 1961);
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ 7317);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ 5590);
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/tooltip */ 89);












const _c0 = function (a0) { return { delay: a0 }; };
const _c1 = function (a1) { return { value: "", params: a1 }; };
function OpenRecentComponent_mat_card_3_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-card")(1, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-card-actions")(6, "button", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_6_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4); const element_r1 = restoredCtx.$implicit; const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r3.openNewProject(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_8_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4); const element_r1 = restoredCtx.$implicit; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r5.deleteProject(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "button", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_11_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4); const element_r1 = restoredCtx.$implicit; const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r6.openProjectSettings(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
} if (rf & 2) {
    const element_r1 = ctx.$implicit;
    const i_r2 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("@bounceInLeftOnEnter", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpureFunction1"](5, _c1, _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpureFunction1"](3, _c0, i_r2 * 100)));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](element_r1.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](element_r1.path);
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
        const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_0__.ProjectSettingsComponent, {
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
OpenRecentComponent.ɵfac = function OpenRecentComponent_Factory(t) { return new (t || OpenRecentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__.ActivatedRoute), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__.MatDialog)); };
OpenRecentComponent.ɵcmp = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: OpenRecentComponent, selectors: [["app-open-recent"]], decls: 5, vars: 3, consts: [[1, "projects-list-container"], [4, "ngFor", "ngForOf"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-raised-button", "", "color", "warn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Project Settings", 3, "click"]], template: function OpenRecentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "Recent Projects");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, OpenRecentComponent_mat_card_3_Template, 14, 7, "mat-card", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 1, ctx.dataSource));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.NgForOf, _angular_material_card__WEBPACK_IMPORTED_MODULE_7__.MatCard, _angular_material_card__WEBPACK_IMPORTED_MODULE_7__.MatCardTitle, _angular_material_card__WEBPACK_IMPORTED_MODULE_7__.MatCardSubtitle, _angular_material_card__WEBPACK_IMPORTED_MODULE_7__.MatCardActions, _angular_material_button__WEBPACK_IMPORTED_MODULE_8__.MatButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__.MatIcon, _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_10__.MatTooltip], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.AsyncPipe], styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  overflow: hidden;\n}\n\nh1[_ngcontent-%COMP%] {\n  margin: 0 0 20px 0;\n  font-weight: 400;\n  font-size: 1.8rem;\n  flex-shrink: 0;\n}\n\n.projects-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 0 8px 0 0;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n  scrollbar-width: thin;\n  scrollbar-color: rgba(0, 0, 0, 0.2) transparent;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: transparent;\n  border-radius: 4px;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: rgba(0, 0, 0, 0.2);\n  border-radius: 4px;\n  -webkit-transition: background 0.3s ease;\n  transition: background 0.3s ease;\n}\n\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: rgba(0, 0, 0, 0.3);\n}\n\nmat-card[_ngcontent-%COMP%] {\n  transition: transform 0.2s ease, box-shadow 0.2s ease;\n  cursor: pointer;\n}\n\nmat-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIm9wZW4tcmVjZW50LmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsWUFBQTtFQUNBLGdCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBQ0Y7O0FBRUE7RUFDRSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUF1QkEscUJBQUE7RUFDQSwrQ0FBQTtBQXJCRjs7QUFBRTtFQUNFLFVBQUE7QUFFSjs7QUFDRTtFQUNFLHVCQUFBO0VBQ0Esa0JBQUE7QUFDSjs7QUFFRTtFQUNFLDhCQUFBO0VBQ0Esa0JBQUE7RUFDQSx3Q0FBQTtFQUFBLGdDQUFBO0FBQUo7O0FBRUk7RUFDRSw4QkFBQTtBQUFOOztBQVVBO0VBQ0UscURBQUE7RUFDQSxlQUFBO0FBUEY7O0FBU0U7RUFDRSwyQkFBQTtFQUNBLDBDQUFBO0FBUEoiLCJmaWxlIjoib3Blbi1yZWNlbnQuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG59XHJcblxyXG5oMSB7XHJcbiAgbWFyZ2luOiAwIDAgMjBweCAwO1xyXG4gIGZvbnQtd2VpZ2h0OiA0MDA7XHJcbiAgZm9udC1zaXplOiAxLjhyZW07XHJcbiAgZmxleC1zaHJpbms6IDA7XHJcbn1cclxuXHJcbi5wcm9qZWN0cy1saXN0LWNvbnRhaW5lciB7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIHBhZGRpbmc6IDAgOHB4IDAgMDtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgZ2FwOiAxNnB4O1xyXG5cclxuICAvLyBDdXN0b20gc2Nyb2xsYmFyIHN0eWxpbmdcclxuICAmOjotd2Via2l0LXNjcm9sbGJhciB7XHJcbiAgICB3aWR0aDogOHB4O1xyXG4gIH1cclxuXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdHJhY2sge1xyXG4gICAgYmFja2dyb3VuZDogdHJhbnNwYXJlbnQ7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgfVxyXG5cclxuICAmOjotd2Via2l0LXNjcm9sbGJhci10aHVtYiB7XHJcbiAgICBiYWNrZ3JvdW5kOiByZ2JhKDAsIDAsIDAsIDAuMik7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuM3MgZWFzZTtcclxuXHJcbiAgICAmOmhvdmVyIHtcclxuICAgICAgYmFja2dyb3VuZDogcmdiYSgwLCAwLCAwLCAwLjMpO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLy8gRmlyZWZveCBzY3JvbGxiYXJcclxuICBzY3JvbGxiYXItd2lkdGg6IHRoaW47XHJcbiAgc2Nyb2xsYmFyLWNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuMikgdHJhbnNwYXJlbnQ7XHJcbn1cclxuXHJcbi8vIFNtb290aCBjYXJkIGhvdmVyIGVmZmVjdFxyXG5tYXQtY2FyZCB7XHJcbiAgdHJhbnNpdGlvbjogdHJhbnNmb3JtIDAuMnMgZWFzZSwgYm94LXNoYWRvdyAwLjJzIGVhc2U7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcclxuICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgwLCAwLCAwLCAwLjE1KTtcclxuICB9XHJcbn1cclxuIl19 */"], data: { animation: [
            (0,angular_animations__WEBPACK_IMPORTED_MODULE_11__.bounceInLeftOnEnterAnimation)({ translate: '500px' }),
        ] } });


/***/ }),

/***/ 2482:
/*!*************************************************************************!*\
  !*** ./src/app/projects/project-settings/project-settings.component.ts ***!
  \*************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectSettingsComponent": () => (/* binding */ ProjectSettingsComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ 5758);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../services/project-settings.service */ 5450);
/* harmony import */ var _services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/compatibility-mode.service */ 7929);
/* harmony import */ var _services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../services/ide-configuration.service */ 9909);
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/tabs */ 2379);
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/card */ 1961);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ 6362);
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/checkbox */ 1534);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/forms */ 587);
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/progress-spinner */ 4742);
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/radio */ 8390);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/icon */ 5590);
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/button */ 7317);















function ProjectSettingsComponent_div_11_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 12)(1, "mat-checkbox", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_11_Template_mat_checkbox_ngModelChange_1_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r7.rule1Enabled = $event; })("change", function ProjectSettingsComponent_div_11_Template_mat_checkbox_change_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8); const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r9.onRule1Change(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Rule #1: Check H1 matches filename");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, " When enabled, verifies that the H1 heading in each Markdown file matches the filename and automatically adds YAML frontmatter to markdown files. This helps maintain consistency between file names and document titles. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](6, "br")(7, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9, "Note:");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10, " This feature should be disabled when using GitHub Compatible Mode, as GitHub doesn't recognize MdExplorer's custom YAML frontmatter format. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r0.rule1Enabled)("disabled", ctx_r0.saving);
} }
function ProjectSettingsComponent_div_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Loading settings...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
} }
function ProjectSettingsComponent_div_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Saving...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
} }
function ProjectSettingsComponent_div_19_Template(rf, ctx) { if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 12)(1, "mat-checkbox", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_19_Template_mat_checkbox_ngModelChange_1_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r11); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r10.githubModeEnabled = $event; })("change", function ProjectSettingsComponent_div_19_Template_mat_checkbox_change_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r11); const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r12.onGitHubModeChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "GitHub Compatible Mode");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, " When enabled, uses GitHub-compatible markdown rendering (no PlantUML, interactive emoji, etc.). Disable for full MdExplorer features when working in a team environment. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r3.githubModeEnabled)("disabled", ctx_r3.saving);
} }
function ProjectSettingsComponent_div_27_Template(rf, ctx) { if (rf & 1) {
    const _r14 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 12)(1, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, " Select which IDE should be used to open files from the toolbar: ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-radio-group", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_27_Template_mat_radio_group_ngModelChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r14); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r13.selectedIde = $event; })("change", function ProjectSettingsComponent_div_27_Template_mat_radio_group_change_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r14); const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r15.onIdeChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-radio-button", 20)(5, "span", 21)(6, "mat-icon", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, " Microsoft Visual Studio Code ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-radio-button", 23)(10, "span", 21)(11, "mat-icon", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](12, "terminal");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13, " IntelliJ IDEA ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r4.selectedIde)("disabled", ctx_r4.saving);
} }
function ProjectSettingsComponent_div_28_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Loading settings...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
} }
function ProjectSettingsComponent_div_29_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Saving...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
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
ProjectSettingsComponent.ɵfac = function ProjectSettingsComponent_Factory(t) { return new (t || ProjectSettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__.MatDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__.MAT_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_project_settings_service__WEBPACK_IMPORTED_MODULE_0__.ProjectSettingsService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_1__.CompatibilityModeService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_2__.IdeConfigurationService)); };
ProjectSettingsComponent.ɵcmp = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: ProjectSettingsComponent, selectors: [["app-project-settings"]], decls: 39, vars: 8, consts: [["mat-dialog-title", ""], [1, "settings-tabs"], ["label", "Markdown Settings"], [1, "tab-content"], [1, "settings-section"], ["class", "setting-item", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "saving-indicator", 4, "ngIf"], ["label", "IDE Settings"], [1, "settings-section", "info-card"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "setting-item"], [3, "ngModel", "disabled", "ngModelChange", "change"], [1, "setting-label"], [1, "setting-description"], [1, "loading-container"], ["diameter", "30"], [1, "saving-indicator"], ["diameter", "20"], ["value", "vscode", 1, "ide-radio"], [1, "ide-label"], [1, "ide-icon-inline", "vscode-color"], ["value", "intellij", 1, "ide-radio"], [1, "ide-icon-inline", "intellij-color"]], template: function ProjectSettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-dialog-content")(3, "mat-tab-group", 1)(4, "mat-tab", 2)(5, "div", 3)(6, "mat-card", 4)(7, "mat-card-header")(8, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9, "Validation Rules");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](10, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, ProjectSettingsComponent_div_11_Template, 11, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](12, ProjectSettingsComponent_div_12_Template, 4, 0, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](13, ProjectSettingsComponent_div_13_Template, 4, 0, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](14, "mat-card", 4)(15, "mat-card-header")(16, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](17, "GitHub Compatibility");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](18, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](19, ProjectSettingsComponent_div_19_Template, 6, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](20, "mat-tab", 8)(21, "div", 3)(22, "mat-card", 4)(23, "mat-card-header")(24, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](25, "IDE Configuration");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](27, ProjectSettingsComponent_div_27_Template, 14, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](28, ProjectSettingsComponent_div_28_Template, 4, 0, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](29, ProjectSettingsComponent_div_29_Template, 4, 0, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](30, "mat-card", 9)(31, "mat-card-content")(32, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](33, "info");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](34, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](35, " IDE settings are specific to this project and saved in the database. Changes take effect immediately when opening files from the toolbar. ");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](36, "mat-dialog-actions", 10)(37, "button", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function ProjectSettingsComponent_Template_button_click_37_listener() { return ctx.close(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](38, "Close");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("Project Settings - ", ctx.projectName, "");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.saving);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.saving);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__.MatDialogTitle, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__.MatDialogContent, _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__.MatTabGroup, _angular_material_tabs__WEBPACK_IMPORTED_MODULE_5__.MatTab, _angular_material_card__WEBPACK_IMPORTED_MODULE_6__.MatCard, _angular_material_card__WEBPACK_IMPORTED_MODULE_6__.MatCardHeader, _angular_material_card__WEBPACK_IMPORTED_MODULE_6__.MatCardTitle, _angular_material_card__WEBPACK_IMPORTED_MODULE_6__.MatCardContent, _angular_common__WEBPACK_IMPORTED_MODULE_7__.NgIf, _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_8__.MatCheckbox, _angular_forms__WEBPACK_IMPORTED_MODULE_9__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_9__.NgModel, _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_10__.MatProgressSpinner, _angular_material_radio__WEBPACK_IMPORTED_MODULE_11__.MatRadioGroup, _angular_material_radio__WEBPACK_IMPORTED_MODULE_11__.MatRadioButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__.MatIcon, _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__.MatDialogActions, _angular_material_button__WEBPACK_IMPORTED_MODULE_13__.MatButton], styles: [".settings-tabs[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 20px 16px;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n\n.setting-item[_ngcontent-%COMP%] {\n  margin: 16px 0;\n}\n\n.setting-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  margin-left: 8px;\n}\n\n.setting-description[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: rgba(0, 0, 0, 0.6);\n  font-size: 14px;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 20px;\n  gap: 12px;\n}\n\n.saving-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 8px;\n  margin-left: 32px;\n  color: rgba(0, 0, 0, 0.6);\n  font-size: 14px;\n}\n\n.info-card[_ngcontent-%COMP%] {\n  background-color: #f5f5f5;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: start;\n  gap: 12px;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n}\n\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 14px;\n  color: rgba(0, 0, 0, 0.7);\n}\n\n.ide-radio[_ngcontent-%COMP%] {\n  display: block;\n  margin: 12px 0;\n}\n\n.ide-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n.ide-icon-inline[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n\n.ide-icon-inline.vscode-color[_ngcontent-%COMP%] {\n  color: #007ACC;\n}\n\n.ide-icon-inline.intellij-color[_ngcontent-%COMP%] {\n  color: #FF6B00;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbInByb2plY3Qtc2V0dGluZ3MuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBQUY7O0FBR0E7RUFDRSxrQkFBQTtBQUFGOztBQUdBO0VBQ0UsbUJBQUE7QUFBRjs7QUFHQTtFQUNFLGNBQUE7QUFBRjs7QUFHQTtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7QUFBRjs7QUFHQTtFQUNFLGVBQUE7RUFDQSxpQkFBQTtFQUNBLHlCQUFBO0VBQ0EsZUFBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsU0FBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7RUFDQSxpQkFBQTtFQUNBLHlCQUFBO0VBQ0EsZUFBQTtBQUFGOztBQUdBO0VBQ0UseUJBQUE7QUFBRjs7QUFFRTtFQUNFLGFBQUE7RUFDQSxrQkFBQTtFQUNBLFNBQUE7QUFBSjs7QUFFSTtFQUNFLGNBQUE7QUFBTjs7QUFHSTtFQUNFLFNBQUE7RUFDQSxlQUFBO0VBQ0EseUJBQUE7QUFETjs7QUFNQTtFQUNFLGNBQUE7RUFDQSxjQUFBO0FBSEY7O0FBTUE7RUFDRSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFIRjs7QUFNQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQUhGOztBQUtFO0VBQ0UsY0FBQTtBQUhKOztBQU1FO0VBQ0UsY0FBQTtBQUpKIiwiZmlsZSI6InByb2plY3Qtc2V0dGluZ3MuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIvLyBUYWIgc3R5bGVzXHJcbi5zZXR0aW5ncy10YWJzIHtcclxuICBtaW4td2lkdGg6IDUwMHB4O1xyXG4gIG1heC13aWR0aDogNjAwcHg7XHJcbn1cclxuXHJcbi50YWItY29udGVudCB7XHJcbiAgcGFkZGluZzogMjBweCAxNnB4O1xyXG59XHJcblxyXG4uc2V0dGluZ3Mtc2VjdGlvbiB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxufVxyXG5cclxuLnNldHRpbmctaXRlbSB7XHJcbiAgbWFyZ2luOiAxNnB4IDA7XHJcbn1cclxuXHJcbi5zZXR0aW5nLWxhYmVsIHtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIG1hcmdpbi1sZWZ0OiA4cHg7XHJcbn1cclxuXHJcbi5zZXR0aW5nLWRlc2NyaXB0aW9uIHtcclxuICBtYXJnaW4tdG9wOiA4cHg7XHJcbiAgbWFyZ2luLWxlZnQ6IDMycHg7XHJcbiAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC42KTtcclxuICBmb250LXNpemU6IDE0cHg7XHJcbn1cclxuXHJcbi5sb2FkaW5nLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDIwcHg7XHJcbiAgZ2FwOiAxMnB4O1xyXG59XHJcblxyXG4uc2F2aW5nLWluZGljYXRvciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIG1hcmdpbi10b3A6IDhweDtcclxuICBtYXJnaW4tbGVmdDogMzJweDtcclxuICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxufVxyXG5cclxuLmluZm8tY2FyZCB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuXHJcbiAgbWF0LWNhcmQtY29udGVudCB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IHN0YXJ0O1xyXG4gICAgZ2FwOiAxMnB4O1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICB9XHJcblxyXG4gICAgcCB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAxNHB4O1xyXG4gICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjcpO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmlkZS1yYWRpbyB7XHJcbiAgZGlzcGxheTogYmxvY2s7XHJcbiAgbWFyZ2luOiAxMnB4IDA7XHJcbn1cclxuXHJcbi5pZGUtbGFiZWwge1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG59XHJcblxyXG4uaWRlLWljb24taW5saW5lIHtcclxuICBmb250LXNpemU6IDIwcHg7XHJcbiAgd2lkdGg6IDIwcHg7XHJcbiAgaGVpZ2h0OiAyMHB4O1xyXG5cclxuICAmLnZzY29kZS1jb2xvciB7XHJcbiAgICBjb2xvcjogIzAwN0FDQzsgLy8gVlMgQ29kZSBibHVlXHJcbiAgfVxyXG5cclxuICAmLmludGVsbGlqLWNvbG9yIHtcclxuICAgIGNvbG9yOiAjRkY2QjAwOyAvLyBJbnRlbGxpSiBvcmFuZ2VcclxuICB9XHJcbn1cclxuXHJcbi8vIElERSBwYXRocyBzdHlsaW5nIHJlbW92ZWQgLSBub3cgc2hvd24gaW4gUHJvamVjdHMgY29tcG9uZW50IG1haW4gcGFnZSJdfQ== */"] });


/***/ }),

/***/ 5609:
/*!************************************************!*\
  !*** ./src/app/projects/projects.component.ts ***!
  \************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectsComponent": () => (/* binding */ ProjectsComponent)
/* harmony export */ });
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! rxjs/operators */ 6942);
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../md-explorer/components/dialogs/settings/settings.component */ 6350);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _environments_version__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../environments/version */ 9279);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/dialog */ 5758);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/router */ 2816);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../shared/NgDialogAnimationService */ 5811);
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../git/services/gitservice.service */ 7224);
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/cdk/clipboard */ 1604);
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/snack-bar */ 2528);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/common/http */ 8784);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/common */ 6362);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/icon */ 5590);
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/form-field */ 4770);
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/input */ 3365);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/forms */ 587);
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/button */ 7317);
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/card */ 1961);
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/material/tooltip */ 89);







 // Importa la versione


















function ProjectsComponent_div_4_button_10_Template(rf, ctx) { if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_div_4_button_10_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r5); const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2); ctx_r4.searchQuery = ""; return ctx_r4.onSearchChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "clear");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
} }
function ProjectsComponent_div_4_mat_card_12_span_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "span", 48)(1, "mat-icon", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "schedule");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](3, " Last Opened ");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function ProjectsComponent_div_4_mat_card_12_p_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "p", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipe"](2, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} if (rf & 2) {
    const project_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate1"](" Last accessed: ", _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipeBind2"](2, 1, project_r6.lastUpdate, "short"), " ");
} }
function ProjectsComponent_div_4_mat_card_12_button_14_Template(rf, ctx) { if (rf & 1) {
    const _r13 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_button_14_Template_button_click_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r13); const project_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().$implicit; const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2); return ctx_r11.shareProject(project_r6, $event); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "share");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
} }
function ProjectsComponent_div_4_mat_card_12_Template(rf, ctx) { if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-card", 33)(1, "mat-card-content", 34)(2, "div", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_div_click_2_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r15); const project_r6 = restoredCtx.$implicit; const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2); return ctx_r14.openProject(project_r6.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](3, "div", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelement"](4, "img", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](5, "div", 38)(6, "h3", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](8, "p", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](10, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](11, ProjectsComponent_div_4_mat_card_12_span_11_Template, 4, 0, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](12, ProjectsComponent_div_4_mat_card_12_p_12_Template, 3, 4, "p", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](13, "div", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](14, ProjectsComponent_div_4_mat_card_12_button_14_Template, 3, 0, "button", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](15, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_button_click_15_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r15); const project_r6 = restoredCtx.$implicit; const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2); return ctx_r16.openProjectSettings(project_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](16, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](17, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](18, "button", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_12_Template_button_click_18_listener() { const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r15); const project_r6 = restoredCtx.$implicit; const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2); return ctx_r17.deleteProject(project_r6); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](19, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](20, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()()()();
} if (rf & 2) {
    const project_r6 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵclassProp"]("last-opened", ctx_r3.isLastOpened(project_r6));
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate"](project_r6.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate"](project_r6.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", ctx_r3.isLastOpened(project_r6));
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", project_r6.lastUpdate);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", ctx_r3.projectHasRemote(project_r6));
} }
function ProjectsComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "div", 24)(1, "div", 25)(2, "h2", 7)(3, "mat-icon", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](4, "history");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](5, " Recent Projects ");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](6, "mat-form-field", 26)(7, "mat-icon", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](8, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](9, "input", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("ngModelChange", function ProjectsComponent_div_4_Template_input_ngModelChange_9_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r19); const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r18.searchQuery = $event; })("input", function ProjectsComponent_div_4_Template_input_input_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r19); const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r20.onSearchChange(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](10, ProjectsComponent_div_4_button_10_Template, 3, 0, "button", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](11, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](12, ProjectsComponent_div_4_mat_card_12_Template, 21, 7, "mat-card", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipe"](13, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngModel", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipeBind1"](13, 3, ctx_r0.recentProjects));
} }
function ProjectsComponent_div_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "div", 52)(1, "mat-icon", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](4, "No Recent Projects");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](5, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](6, "Start by creating a new project or cloning a repository");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
} }
class ProjectsComponent {
    constructor(projectService, dialog, router, signalRService, dialogAn, gitService, clipboard, snackBar, http) {
        this.projectService = projectService;
        this.dialog = dialog;
        this.router = router;
        this.signalRService = signalRService;
        this.dialogAn = dialogAn;
        this.gitService = gitService;
        this.clipboard = clipboard;
        this.snackBar = snackBar;
        this.http = http;
        this.appVersion = _environments_version__WEBPACK_IMPORTED_MODULE_6__.versionInfo.version; // Rendi la versione disponibile nel template
        this.buildTime = _environments_version__WEBPACK_IMPORTED_MODULE_6__.versionInfo.buildTime; // Rendi il timestamp di build disponibile nel template
        this.searchQuery = '';
        this.lastOpenedProjectId = null;
        // Flag to prevent multiple clicks when opening a project
        this.isOpeningProject = false;
        // Cache for remote URL status per project path
        this.remoteUrlCache = new Map();
        this.dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }];
    }
    ngOnDestroy() {
        console.log('ProjectsComponent destroyed!');
    }
    ngOnInit() {
        // Load recent projects and sort by lastUpdate descending (most recent first)
        this.projectService.fetchProjects();
        this.recentProjects = this.projectService.mdProjects.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_12__.map)(projects => {
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
                this.router.navigate(['/main/navigation/document']);
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
        // Prevent multiple clicks while project is opening
        if (this.isOpeningProject) {
            console.log('[Projects] Ignoring click - project opening already in progress');
            return;
        }
        this.isOpeningProject = true;
        console.log('[Projects] Opening project:', path);
        this.projectService.setNewFolderProject(path);
        // Reset flag after a timeout (in case navigation doesn't happen)
        setTimeout(() => {
            this.isOpeningProject = false;
        }, 10000); // 10 second safety timeout
    }
    deleteProject(project) {
        this.projectService.deleteProject(project, () => {
            this.projectService.fetchProjects();
        }, this);
    }
    openProjectSettings(project) {
        const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_3__.ProjectSettingsComponent, {
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
        const dialogRef = this.dialog.open(_dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_1__.ModernCloneProjectComponent, {
            width: '600px',
            maxHeight: '600px',
            data: null
        });
    }
    openNewFolder() {
        let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_5__.ShowFileMetadata();
        data.start = null;
        data.title = "Select project folder";
        data.typeOfSelection = "Folders";
        data.buttonText = "Select folder"; // Testo personalizzato
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__.ShowFileSystemComponent, {
            width: '800px',
            height: '600px',
            panelClass: 'resizable-dialog-container',
            data: data
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result && result.data) {
                // Open configuration dialog after folder selection
                const configDialogRef = this.dialog.open(_dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_2__.ProjectCreateConfigDialogComponent, {
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
        const dialogRef = this.dialogAn.open(_md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_4__.SettingsComponent, {
            width: '600px',
            animation: {},
        });
    }
    /**
     * Check if project has a Git remote configured (cached)
     * Used to show/hide the Share button
     */
    projectHasRemote(project) {
        const cached = this.remoteUrlCache.get(project.path);
        if (cached !== undefined) {
            return cached.hasRemote;
        }
        // Mark as loading to prevent duplicate requests
        this.remoteUrlCache.set(project.path, { hasRemote: false, loading: true });
        // Fetch asynchronously and update cache
        this.gitService.getRemoteUrl(project.path).subscribe(result => {
            this.remoteUrlCache.set(project.path, {
                hasRemote: result.hasRemote,
                remoteUrl: result.remoteUrl,
                loading: false
            });
        });
        return false; // Return false initially, will update on next change detection
    }
    /**
     * Share project URL to clipboard
     * Generates: mdexplorer://configproject?repo=<url>&basePath=<parent-folder>
     */
    shareProject(project, event) {
        event.stopPropagation(); // Prevent opening the project
        const cached = this.remoteUrlCache.get(project.path);
        if (!(cached === null || cached === void 0 ? void 0 : cached.hasRemote) || !(cached === null || cached === void 0 ? void 0 : cached.remoteUrl)) {
            this.snackBar.open('No Git remote configured for this project', 'OK', { duration: 3000 });
            return;
        }
        // Extract base path (parent folder of project)
        // e.g., C:\Progetti\myrepo -> C:\Progetti
        const lastSeparator = project.path.lastIndexOf('\\');
        const basePath = lastSeparator > 0 ? project.path.substring(0, lastSeparator) : project.path;
        // Build mdexplorer:// URL
        const shareUrl = `mdexplorer://configproject?repo=${encodeURIComponent(cached.remoteUrl)}&basePath=${encodeURIComponent(basePath)}`;
        // Copy to clipboard
        this.clipboard.copy(shareUrl);
        this.snackBar.open('Share URL copied to clipboard!', 'OK', { duration: 3000 });
        console.log('[Projects] Share URL copied:', shareUrl);
    }
    /**
     * Open the application log file
     */
    openLog() {
        this.http.post('../api/Diagnostics/OpenLog', {}).subscribe({
            next: (result) => {
                console.log('[Projects] Log opened:', result.path);
            },
            error: (err) => {
                var _a;
                console.error('[Projects] Error opening log:', err);
                this.snackBar.open('Error opening log file: ' + (((_a = err.error) === null || _a === void 0 ? void 0 : _a.error) || err.message), 'OK', { duration: 5000 });
            }
        });
    }
}
ProjectsComponent.ɵfac = function ProjectsComponent_Factory(t) { return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_13__.MatDialog), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_14__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_8__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_9__.NgDialogAnimationService), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_10__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_15__.Clipboard), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_16__.MatSnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_17__.HttpClient)); };
ProjectsComponent.ɵcmp = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdefineComponent"]({ type: ProjectsComponent, selectors: [["app-projects"]], decls: 58, vars: 6, consts: [[1, "modern-container"], [1, "content-wrapper"], [1, "main-layout"], [1, "left-column"], ["class", "recent-projects-section", 4, "ngIf"], ["class", "no-projects-message", 4, "ngIf"], [1, "right-column"], [1, "section-title"], [1, "section-icon"], [1, "actions-grid"], ["data-test", "clone-button", 1, "action-card", "clone-card", 3, "click"], [1, "card-content"], [1, "icon-wrapper", "clone-icon"], [1, "card-text"], [1, "card-title"], [1, "card-description"], [1, "card-action"], [1, "arrow-icon"], ["data-test", "new-folder-button", 1, "action-card", "create-card", 3, "click"], [1, "icon-wrapper", "create-icon"], ["data-test", "settings-button", 1, "action-card", "settings-card", 3, "click"], [1, "icon-wrapper", "settings-icon"], [1, "diagnostics-section"], ["mat-stroked-button", "", "color", "primary", "matTooltip", "Open application log file for debugging", 3, "click"], [1, "recent-projects-section"], [1, "section-header"], ["appearance", "outline", 1, "search-field"], ["matPrefix", ""], ["matInput", "", "placeholder", "Search projects...", 3, "ngModel", "ngModelChange", "input"], ["mat-icon-button", "", "matSuffix", "", 3, "click", 4, "ngIf"], [1, "projects-grid"], ["class", "project-card", 3, "last-opened", 4, "ngFor", "ngForOf"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], [1, "project-card"], [1, "project-card-content"], [1, "project-info", 3, "click"], [1, "project-icon-wrapper"], ["src", "assets/icons/files-stack.svg", "alt", "Project", 1, "project-icon-svg"], [1, "project-details"], [1, "project-name"], [1, "project-path"], [1, "project-meta"], ["class", "last-opened-badge", 4, "ngIf"], ["class", "project-date", 4, "ngIf"], [1, "project-actions"], ["mat-icon-button", "", "matTooltip", "Share Project URL", "class", "action-btn share-btn", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "Project Settings", 1, "action-btn", "settings-btn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Delete Project", 1, "action-btn", "delete-btn", 3, "click"], [1, "last-opened-badge"], [1, "badge-icon"], [1, "project-date"], ["mat-icon-button", "", "matTooltip", "Share Project URL", 1, "action-btn", "share-btn", 3, "click"], [1, "no-projects-message"], [1, "empty-icon"]], template: function ProjectsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "div", 2)(3, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](4, ProjectsComponent_div_4_Template, 14, 5, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipe"](5, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](6, ProjectsComponent_div_6_Template, 7, 0, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipe"](7, "async");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](8, "div", 6)(9, "h2", 7)(10, "mat-icon", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](11, "flash_on");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](12, " Quick Actions ");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](13, "div", 9)(14, "mat-card", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_14_listener() { return ctx.prepareToClone(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](15, "mat-card-content", 11)(16, "div", 12)(17, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](18, "cloud_download");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](19, "div", 13)(20, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](21, "Clone Repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](22, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](23, "Get code from an online repository like GitHub or GitLab");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](24, "div", 16)(25, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](26, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](27, "mat-card", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_27_listener() { return ctx.openNewFolder(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](28, "mat-card-content", 11)(29, "div", 19)(30, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](31, "create_new_folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](32, "div", 13)(33, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](34, "Create New Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](35, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](36, "Choose a filesystem folder to get started with your documentation");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](37, "div", 16)(38, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](39, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](40, "mat-card", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_40_listener() { return ctx.openSettings(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](41, "mat-card-content", 11)(42, "div", 21)(43, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](44, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](45, "div", 13)(46, "h2", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](47, "Settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](48, "p", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](49, "Configure IDE paths, PlantUML, and other global preferences");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](50, "div", 16)(51, "mat-icon", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](52, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()()()();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](53, "div", 22)(54, "button", 23);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_54_listener() { return ctx.openLog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](55, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](56, "description");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](57, " Open Log ");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]()()()()()();
    } if (rf & 2) {
        let tmp_0_0;
        let tmp_1_0;
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", ((tmp_0_0 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipeBind1"](5, 2, ctx.recentProjects)) == null ? null : tmp_0_0.length) > 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", !((tmp_1_0 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵpipeBind1"](7, 4, ctx.recentProjects)) == null ? null : tmp_1_0.length));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_18__.NgIf, _angular_material_icon__WEBPACK_IMPORTED_MODULE_19__.MatIcon, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_20__.MatFormField, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_20__.MatPrefix, _angular_material_input__WEBPACK_IMPORTED_MODULE_21__.MatInput, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.NgModel, _angular_material_button__WEBPACK_IMPORTED_MODULE_23__.MatButton, _angular_material_form_field__WEBPACK_IMPORTED_MODULE_20__.MatSuffix, _angular_common__WEBPACK_IMPORTED_MODULE_18__.NgForOf, _angular_material_card__WEBPACK_IMPORTED_MODULE_24__.MatCard, _angular_material_card__WEBPACK_IMPORTED_MODULE_24__.MatCardContent, _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_25__.MatTooltip], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_18__.AsyncPipe, _angular_common__WEBPACK_IMPORTED_MODULE_18__.DatePipe], styles: [".modern-container[_ngcontent-%COMP%] {\n  min-height: 100vh;\n  background: #f5f7fa;\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n  overflow-x: hidden;\n  overflow-y: auto;\n}\n\n.content-wrapper[_ngcontent-%COMP%] {\n  flex: 1;\n  background: #f5f7fa;\n  padding: 40px;\n  max-width: 1600px;\n  margin: 0 auto;\n  width: 100%;\n  min-height: calc(100vh - 36px);\n  box-sizing: border-box;\n}\n\n.main-layout[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 400px;\n  grid-gap: 32px;\n  gap: 32px;\n  align-items: start;\n  height: calc(100vh - 116px);\n}\n\n.left-column[_ngcontent-%COMP%] {\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  max-height: 100%;\n}\n\n.right-column[_ngcontent-%COMP%] {\n  position: sticky;\n  top: 40px;\n  align-self: start;\n}\n\n.actions-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.action-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 16px;\n  border: none;\n  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);\n  overflow: hidden;\n  background: white;\n}\n\n.action-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);\n}\n\n.action-card[_ngcontent-%COMP%]:hover   .arrow-icon[_ngcontent-%COMP%] {\n  transform: translateX(4px);\n}\n\n.action-card[_ngcontent-%COMP%]:hover   .icon-wrapper[_ngcontent-%COMP%] {\n  transform: scale(1.1);\n}\n\n.action-card[_ngcontent-%COMP%]:active {\n  transform: translateY(-2px);\n}\n\n.action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 16px;\n  padding: 20px 16px;\n  position: relative;\n}\n\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 48px;\n  height: 48px;\n  border-radius: 12px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: transform 0.3s ease;\n}\n\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: white;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-title[_ngcontent-%COMP%] {\n  margin: 0 0 4px 0;\n  font-size: 1rem;\n  font-weight: 500;\n  color: #2d3748;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.75rem;\n  color: #718096;\n  line-height: 1.4;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%]   .arrow-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: transform 0.3s ease;\n}\n\n.action-card.clone-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n}\n\n.action-card.create-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);\n}\n\n.action-card.settings-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);\n}\n\n.section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #2d3748;\n  margin: 0 0 24px 0;\n}\n\n.section-title[_ngcontent-%COMP%]   .section-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n  color: #667eea;\n}\n\n.section-divider[_ngcontent-%COMP%] {\n  height: 1px;\n  background: linear-gradient(to right, transparent, #e2e8f0, transparent);\n  margin: 32px 0;\n}\n\n.recent-projects-section[_ngcontent-%COMP%] {\n  margin-bottom: 0;\n  display: flex;\n  flex-direction: column;\n  flex: 1;\n  min-height: 0;\n}\n\n.section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 24px;\n  gap: 24px;\n  flex-shrink: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  margin: 0;\n  flex-shrink: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n  width: 100%;\n  max-width: 400px;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n  margin-bottom: 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-infix[_ngcontent-%COMP%] {\n  padding: 0.5em 0;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-underline[_ngcontent-%COMP%] {\n  display: none;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #718096;\n}\n\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.no-projects-message[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 60px 20px;\n  color: #718096;\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   .empty-icon[_ngcontent-%COMP%] {\n  font-size: 80px;\n  width: 80px;\n  height: 80px;\n  color: #cbd5e0;\n  margin: 0 auto 20px;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #4a5568;\n  margin: 0 0 8px 0;\n}\n\n.no-projects-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n  flex: 1;\n  overflow-y: auto;\n  padding-right: 8px;\n  min-height: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: #e2e8f0;\n  border-radius: 4px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: #cbd5e0;\n  border-radius: 4px;\n}\n\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: #a0aec0;\n}\n\n.project-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 12px;\n  border: 1px solid #e2e8f0;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);\n  background: white;\n  position: relative;\n  overflow: visible;\n}\n\n.project-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);\n  border-color: #667eea;\n}\n\n.project-card.last-opened[_ngcontent-%COMP%] {\n  border: 2px solid #667eea;\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);\n  background: linear-gradient(135deg, rgba(102, 126, 234, 0.03) 0%, rgba(118, 75, 162, 0.03) 100%);\n}\n\n.project-card.last-opened[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(102, 126, 234, 0.3);\n  border-color: #764ba2;\n}\n\n.project-card.last-opened[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);\n  animation: pulse 2s ease-in-out infinite;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 16px;\n  gap: 12px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 1;\n  min-width: 0;\n  cursor: pointer;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 40px;\n  height: 40px;\n  border-radius: 10px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: white;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon-svg[_ngcontent-%COMP%] {\n  width: 20px;\n  height: 20px;\n  filter: brightness(0) invert(1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.1rem;\n  font-weight: 500;\n  color: #2d3748;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-path[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: #718096;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-meta[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin-top: 4px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n  padding: 4px 10px;\n  border-radius: 12px;\n  font-size: 0.65rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.5px;\n  white-space: nowrap;\n  box-shadow: 0 2px 6px rgba(102, 126, 234, 0.3);\n  animation: fadeInScale 0.5s ease-out;\n  flex-shrink: 0;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%]   .badge-icon[_ngcontent-%COMP%] {\n  font-size: 12px;\n  width: 12px;\n  height: 12px;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-date[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.7rem;\n  color: #a0aec0;\n  font-style: italic;\n  flex: 1;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 4px;\n  flex-shrink: 0;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%] {\n  transition: all 0.2s ease;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(76, 175, 80, 0.1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(102, 126, 234, 0.1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #667eea;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(245, 87, 108, 0.1);\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #f5576c;\n}\n\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: color 0.2s ease;\n}\n\n.sidebar-section[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n}\n\n@media (max-width: 1024px) {\n  .main-layout[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n    gap: 32px;\n  }\n\n  .right-column[_ngcontent-%COMP%] {\n    position: static;\n  }\n\n  .actions-grid[_ngcontent-%COMP%] {\n    display: grid;\n    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));\n    grid-gap: 16px;\n    gap: 16px;\n  }\n}\n\n@media (max-width: 768px) {\n  .header-section[_ngcontent-%COMP%] {\n    padding: 40px 20px 20px;\n  }\n  .header-section[_ngcontent-%COMP%]   .main-title[_ngcontent-%COMP%] {\n    font-size: 2rem;\n  }\n  .header-section[_ngcontent-%COMP%]   .subtitle[_ngcontent-%COMP%] {\n    font-size: 1rem;\n  }\n\n  .content-wrapper[_ngcontent-%COMP%] {\n    padding: 32px 20px;\n    border-radius: 24px 24px 0 0;\n  }\n\n  .actions-grid[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n  }\n\n  .action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n    padding: 24px 20px;\n  }\n\n  .section-title[_ngcontent-%COMP%] {\n    font-size: 1.25rem;\n  }\n\n  .section-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: flex-start;\n  }\n  .section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n    max-width: 100%;\n  }\n}\n\n@keyframes pulse {\n  0%, 100% {\n    transform: scale(1);\n  }\n  50% {\n    transform: scale(1.05);\n  }\n}\n\n@keyframes fadeInScale {\n  0% {\n    opacity: 0;\n    transform: scale(0.8);\n  }\n  100% {\n    opacity: 1;\n    transform: scale(1);\n  }\n}\n\n.diagnostics-section[_ngcontent-%COMP%] {\n  margin-top: 24px;\n  padding-top: 16px;\n  border-top: 1px solid #e2e8f0;\n}\n\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n}\n\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  margin-right: 6px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbInByb2plY3RzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUNBO0VBQ0UsaUJBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFVBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQUY7O0FBSUE7RUFDRSxPQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsV0FBQTtFQUNBLDhCQUFBO0VBQ0Esc0JBQUE7QUFERjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxnQ0FBQTtFQUNBLGNBQUE7RUFBQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSwyQkFBQTtBQUZGOztBQUtBO0VBQ0UsWUFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtBQUZGOztBQUtBO0VBQ0UsZ0JBQUE7RUFDQSxTQUFBO0VBQ0EsaUJBQUE7QUFGRjs7QUFNQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7QUFIRjs7QUFPQTtFQUNFLGVBQUE7RUFDQSxpREFBQTtFQUNBLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLHlDQUFBO0VBQ0EsZ0JBQUE7RUFDQSxpQkFBQTtBQUpGOztBQU1FO0VBQ0UsMkJBQUE7RUFDQSwyQ0FBQTtBQUpKOztBQU1JO0VBQ0UsMEJBQUE7QUFKTjs7QUFPSTtFQUNFLHFCQUFBO0FBTE47O0FBU0U7RUFDRSwyQkFBQTtBQVBKOztBQVVFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7QUFSSjs7QUFXRTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSwrQkFBQTtBQVRKOztBQVdJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsWUFBQTtBQVROOztBQWFFO0VBQ0UsT0FBQTtBQVhKOztBQWFJO0VBQ0UsaUJBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0FBWE47O0FBY0k7RUFDRSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFaTjs7QUFnQkU7RUFDRSxjQUFBO0FBZEo7O0FBZ0JJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLCtCQUFBO0FBZE47O0FBbUJFO0VBQ0UsNkRBQUE7QUFqQko7O0FBb0JFO0VBQ0UsNkRBQUE7QUFsQko7O0FBcUJFO0VBQ0UsNkRBQUE7QUFuQko7O0FBd0JBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0Esa0JBQUE7QUFyQkY7O0FBdUJFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtBQXJCSjs7QUEwQkE7RUFDRSxXQUFBO0VBQ0Esd0VBQUE7RUFDQSxjQUFBO0FBdkJGOztBQTJCQTtFQUNFLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsT0FBQTtFQUNBLGFBQUE7QUF4QkY7O0FBNEJBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsOEJBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxjQUFBO0FBekJGOztBQTJCRTtFQUNFLFNBQUE7RUFDQSxjQUFBO0FBekJKOztBQTRCRTtFQUNFLFdBQUE7RUFDQSxnQkFBQTtBQTFCSjs7QUE2Qkk7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0FBM0JOOztBQThCSTtFQUNFLGdCQUFBO0FBNUJOOztBQStCSTtFQUNFLGFBQUE7QUE3Qk47O0FBZ0NJO0VBQ0UsZUFBQTtBQTlCTjs7QUFpQ0k7RUFDRSxjQUFBO0FBL0JOOztBQWtDSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQWhDTjs7QUFzQ0E7RUFDRSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBbkNGOztBQXFDRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7RUFDQSxtQkFBQTtBQW5DSjs7QUFzQ0U7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtFQUNBLGlCQUFBO0FBcENKOztBQXVDRTtFQUNFLGVBQUE7RUFDQSxTQUFBO0FBckNKOztBQXlDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUFDQSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7QUF0Q0Y7O0FBeUNFO0VBQ0UsVUFBQTtBQXZDSjs7QUEwQ0U7RUFDRSxtQkFBQTtFQUNBLGtCQUFBO0FBeENKOztBQTJDRTtFQUNFLG1CQUFBO0VBQ0Esa0JBQUE7QUF6Q0o7O0FBMkNJO0VBQ0UsbUJBQUE7QUF6Q047O0FBOENBO0VBQ0UsZUFBQTtFQUNBLGlEQUFBO0VBQ0EsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLHlDQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGlCQUFBO0FBM0NGOztBQTZDRTtFQUNFLDJCQUFBO0VBQ0EseUNBQUE7RUFDQSxxQkFBQTtBQTNDSjs7QUErQ0U7RUFDRSx5QkFBQTtFQUNBLCtDQUFBO0VBQ0EsZ0dBQUE7QUE3Q0o7O0FBK0NJO0VBQ0UsMkJBQUE7RUFDQSxnREFBQTtFQUNBLHFCQUFBO0FBN0NOOztBQWdESTtFQUNFLDZEQUFBO0VBQ0EsK0NBQUE7RUFDQSx3Q0FBQTtBQTlDTjs7QUFrREU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLGFBQUE7RUFDQSxTQUFBO0FBaERKOztBQW1ERTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxPQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7QUFqREo7O0FBb0RFO0VBQ0UsY0FBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7RUFDQSw2REFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBbERKOztBQW9ESTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLFlBQUE7QUFsRE47O0FBcURJO0VBQ0UsV0FBQTtFQUNBLFlBQUE7RUFDQSwrQkFBQTtBQW5ETjs7QUF1REU7RUFDRSxPQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUFyREo7O0FBdURJO0VBQ0UsU0FBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0FBckROOztBQXdESTtFQUNFLFNBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7QUF0RE47O0FBeURJO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGVBQUE7QUF2RE47O0FBMERJO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSw2REFBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHlCQUFBO0VBQ0EscUJBQUE7RUFDQSxtQkFBQTtFQUNBLDhDQUFBO0VBQ0Esb0NBQUE7RUFDQSxjQUFBO0FBeEROOztBQTBETTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQXhEUjs7QUE0REk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0VBQ0Esa0JBQUE7RUFDQSxPQUFBO0FBMUROOztBQThERTtFQUNFLGFBQUE7RUFDQSxRQUFBO0VBQ0EsY0FBQTtBQTVESjs7QUE4REk7RUFDRSx5QkFBQTtBQTVETjs7QUE4RE07RUFDRSx3Q0FBQTtBQTVEUjs7QUE2RFE7RUFDRSxjQUFBO0FBM0RWOztBQStETTtFQUNFLDBDQUFBO0FBN0RSOztBQThEUTtFQUNFLGNBQUE7QUE1RFY7O0FBZ0VNO0VBQ0UseUNBQUE7QUE5RFI7O0FBK0RRO0VBQ0UsY0FBQTtBQTdEVjs7QUFpRU07RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsMkJBQUE7QUEvRFI7O0FBc0VBO0VBQ0UsbUJBQUE7QUFuRUY7O0FBdUVBO0VBQ0U7SUFDRSwwQkFBQTtJQUNBLFNBQUE7RUFwRUY7O0VBdUVBO0lBQ0UsZ0JBQUE7RUFwRUY7O0VBdUVBO0lBQ0UsYUFBQTtJQUNBLDJEQUFBO0lBQ0EsY0FBQTtJQUFBLFNBQUE7RUFwRUY7QUFDRjs7QUF1RUE7RUFDRTtJQUNFLHVCQUFBO0VBckVGO0VBdUVFO0lBQ0UsZUFBQTtFQXJFSjtFQXdFRTtJQUNFLGVBQUE7RUF0RUo7O0VBMEVBO0lBQ0Usa0JBQUE7SUFDQSw0QkFBQTtFQXZFRjs7RUEwRUE7SUFDRSwwQkFBQTtFQXZFRjs7RUEwRUE7SUFDRSxrQkFBQTtFQXZFRjs7RUEwRUE7SUFDRSxrQkFBQTtFQXZFRjs7RUEwRUE7SUFDRSxzQkFBQTtJQUNBLHVCQUFBO0VBdkVGO0VBeUVFO0lBQ0UsZUFBQTtFQXZFSjtBQUNGOztBQTRFQTtFQUNFO0lBQ0UsbUJBQUE7RUExRUY7RUE0RUE7SUFDRSxzQkFBQTtFQTFFRjtBQUNGOztBQTZFQTtFQUNFO0lBQ0UsVUFBQTtJQUNBLHFCQUFBO0VBM0VGO0VBNkVBO0lBQ0UsVUFBQTtJQUNBLG1CQUFBO0VBM0VGO0FBQ0Y7O0FBK0VBO0VBQ0UsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLDZCQUFBO0FBN0VGOztBQStFRTtFQUNFLGtCQUFBO0FBN0VKOztBQThFSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBNUVOIiwiZmlsZSI6InByb2plY3RzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLy8gTW9kZXJuIGNvbnRhaW5lciBsYXlvdXRcclxuLm1vZGVybi1jb250YWluZXIge1xyXG4gIG1pbi1oZWlnaHQ6IDEwMHZoO1xyXG4gIGJhY2tncm91bmQ6ICNmNWY3ZmE7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIHBhZGRpbmc6IDA7XHJcbiAgb3ZlcmZsb3cteDogaGlkZGVuO1xyXG4gIG92ZXJmbG93LXk6IGF1dG87XHJcbn1cclxuXHJcbi8vIENvbnRlbnQgd3JhcHBlclxyXG4uY29udGVudC13cmFwcGVyIHtcclxuICBmbGV4OiAxO1xyXG4gIGJhY2tncm91bmQ6ICNmNWY3ZmE7XHJcbiAgcGFkZGluZzogNDBweDtcclxuICBtYXgtd2lkdGg6IDE2MDBweDtcclxuICBtYXJnaW46IDAgYXV0bztcclxuICB3aWR0aDogMTAwJTtcclxuICBtaW4taGVpZ2h0OiBjYWxjKDEwMHZoIC0gMzZweCk7XHJcbiAgYm94LXNpemluZzogYm9yZGVyLWJveDtcclxufVxyXG5cclxuLy8gTWFpbiBsYXlvdXQgLSBUd28gY29sdW1uc1xyXG4ubWFpbi1sYXlvdXQge1xyXG4gIGRpc3BsYXk6IGdyaWQ7XHJcbiAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnIgNDAwcHg7XHJcbiAgZ2FwOiAzMnB4O1xyXG4gIGFsaWduLWl0ZW1zOiBzdGFydDtcclxuICBoZWlnaHQ6IGNhbGMoMTAwdmggLSAxMTZweCk7IC8vIDM2cHggdGl0bGUgYmFyICsgODBweCBwYWRkaW5nXHJcbn1cclxuXHJcbi5sZWZ0LWNvbHVtbiB7XHJcbiAgbWluLXdpZHRoOiAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgbWF4LWhlaWdodDogMTAwJTtcclxufVxyXG5cclxuLnJpZ2h0LWNvbHVtbiB7XHJcbiAgcG9zaXRpb246IHN0aWNreTtcclxuICB0b3A6IDQwcHg7XHJcbiAgYWxpZ24tc2VsZjogc3RhcnQ7XHJcbn1cclxuXHJcbi8vIEFjdGlvbiBjYXJkcyBncmlkIChpbiByaWdodCBjb2x1bW4gLSB2ZXJ0aWNhbCBzdGFjaylcclxuLmFjdGlvbnMtZ3JpZCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMTZweDtcclxufVxyXG5cclxuLy8gQWN0aW9uIGNhcmQgc3R5bGluZ1xyXG4uYWN0aW9uLWNhcmQge1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICB0cmFuc2l0aW9uOiBhbGwgMC4zcyBjdWJpYy1iZXppZXIoMC40LCAwLCAwLjIsIDEpO1xyXG4gIGJvcmRlci1yYWRpdXM6IDE2cHg7XHJcbiAgYm9yZGVyOiBub25lO1xyXG4gIGJveC1zaGFkb3c6IDAgMnB4IDhweCByZ2JhKDAsIDAsIDAsIDAuMDgpO1xyXG4gIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgYmFja2dyb3VuZDogd2hpdGU7XHJcblxyXG4gICY6aG92ZXIge1xyXG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKC00cHgpO1xyXG4gICAgYm94LXNoYWRvdzogMCAxMnB4IDI0cHggcmdiYSgwLCAwLCAwLCAwLjE1KTtcclxuXHJcbiAgICAuYXJyb3ctaWNvbiB7XHJcbiAgICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWCg0cHgpO1xyXG4gICAgfVxyXG5cclxuICAgIC5pY29uLXdyYXBwZXIge1xyXG4gICAgICB0cmFuc2Zvcm06IHNjYWxlKDEuMSk7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAmOmFjdGl2ZSB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XHJcbiAgfVxyXG5cclxuICAuY2FyZC1jb250ZW50IHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiAxNnB4O1xyXG4gICAgcGFkZGluZzogMjBweCAxNnB4O1xyXG4gICAgcG9zaXRpb246IHJlbGF0aXZlO1xyXG4gIH1cclxuXHJcbiAgLmljb24td3JhcHBlciB7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuICAgIHdpZHRoOiA0OHB4O1xyXG4gICAgaGVpZ2h0OiA0OHB4O1xyXG4gICAgYm9yZGVyLXJhZGl1czogMTJweDtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgICB0cmFuc2l0aW9uOiB0cmFuc2Zvcm0gMC4zcyBlYXNlO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyNHB4O1xyXG4gICAgICB3aWR0aDogMjRweDtcclxuICAgICAgaGVpZ2h0OiAyNHB4O1xyXG4gICAgICBjb2xvcjogd2hpdGU7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuY2FyZC10ZXh0IHtcclxuICAgIGZsZXg6IDE7XHJcblxyXG4gICAgLmNhcmQtdGl0bGUge1xyXG4gICAgICBtYXJnaW46IDAgMCA0cHggMDtcclxuICAgICAgZm9udC1zaXplOiAxcmVtO1xyXG4gICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICBjb2xvcjogIzJkMzc0ODtcclxuICAgIH1cclxuXHJcbiAgICAuY2FyZC1kZXNjcmlwdGlvbiB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAwLjc1cmVtO1xyXG4gICAgICBjb2xvcjogIzcxODA5NjtcclxuICAgICAgbGluZS1oZWlnaHQ6IDEuNDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5jYXJkLWFjdGlvbiB7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgICAuYXJyb3ctaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgY29sb3I6ICNhMGFlYzA7XHJcbiAgICAgIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjNzIGVhc2U7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAvLyBTcGVjaWZpYyBjYXJkIGNvbG9yc1xyXG4gICYuY2xvbmUtY2FyZCAuaWNvbi13cmFwcGVyIHtcclxuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgfVxyXG5cclxuICAmLmNyZWF0ZS1jYXJkIC5pY29uLXdyYXBwZXIge1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgI2YwOTNmYiAwJSwgI2Y1NTc2YyAxMDAlKTtcclxuICB9XHJcblxyXG4gICYuc2V0dGluZ3MtY2FyZCAuaWNvbi13cmFwcGVyIHtcclxuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM0ZmFjZmUgMCUsICMwMGYyZmUgMTAwJSk7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTZWN0aW9uIHRpdGxlXHJcbi5zZWN0aW9uLXRpdGxlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIGZvbnQtc2l6ZTogMS41cmVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgY29sb3I6ICMyZDM3NDg7XHJcbiAgbWFyZ2luOiAwIDAgMjRweCAwO1xyXG5cclxuICAuc2VjdGlvbi1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMjhweDtcclxuICAgIHdpZHRoOiAyOHB4O1xyXG4gICAgaGVpZ2h0OiAyOHB4O1xyXG4gICAgY29sb3I6ICM2NjdlZWE7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTZWN0aW9uIGRpdmlkZXJcclxuLnNlY3Rpb24tZGl2aWRlciB7XHJcbiAgaGVpZ2h0OiAxcHg7XHJcbiAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KHRvIHJpZ2h0LCB0cmFuc3BhcmVudCwgI2UyZThmMCwgdHJhbnNwYXJlbnQpO1xyXG4gIG1hcmdpbjogMzJweCAwO1xyXG59XHJcblxyXG4vLyBSZWNlbnQgUHJvamVjdHMgU2VjdGlvblxyXG4ucmVjZW50LXByb2plY3RzLXNlY3Rpb24ge1xyXG4gIG1hcmdpbi1ib3R0b206IDA7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGZsZXg6IDE7XHJcbiAgbWluLWhlaWdodDogMDtcclxufVxyXG5cclxuLy8gU2VjdGlvbiBoZWFkZXIgd2l0aCBzZWFyY2hcclxuLnNlY3Rpb24taGVhZGVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xyXG4gIG1hcmdpbi1ib3R0b206IDI0cHg7XHJcbiAgZ2FwOiAyNHB4O1xyXG4gIGZsZXgtc2hyaW5rOiAwO1xyXG5cclxuICAuc2VjdGlvbi10aXRsZSB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuICB9XHJcblxyXG4gIC5zZWFyY2gtZmllbGQge1xyXG4gICAgd2lkdGg6IDEwMCU7XHJcbiAgICBtYXgtd2lkdGg6IDQwMHB4O1xyXG5cclxuICAgIC8vIE92ZXJyaWRlIE1hdGVyaWFsIERlc2lnbiBkZWZhdWx0IG1hcmdpbnNcclxuICAgIC5tYXQtZm9ybS1maWVsZC13cmFwcGVyIHtcclxuICAgICAgcGFkZGluZy1ib3R0b206IDA7XHJcbiAgICAgIG1hcmdpbi1ib3R0b206IDA7XHJcbiAgICB9XHJcblxyXG4gICAgLm1hdC1mb3JtLWZpZWxkLWluZml4IHtcclxuICAgICAgcGFkZGluZzogMC41ZW0gMDtcclxuICAgIH1cclxuXHJcbiAgICAubWF0LWZvcm0tZmllbGQtdW5kZXJsaW5lIHtcclxuICAgICAgZGlzcGxheTogbm9uZTtcclxuICAgIH1cclxuXHJcbiAgICBpbnB1dCB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMTRweDtcclxuICAgIH1cclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGNvbG9yOiAjNzE4MDk2O1xyXG4gICAgfVxyXG5cclxuICAgIGJ1dHRvbiBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi8vIE5vIHByb2plY3RzIG1lc3NhZ2VcclxuLm5vLXByb2plY3RzLW1lc3NhZ2Uge1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBwYWRkaW5nOiA2MHB4IDIwcHg7XHJcbiAgY29sb3I6ICM3MTgwOTY7XHJcbiAgZmxleDogMTtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuXHJcbiAgLmVtcHR5LWljb24ge1xyXG4gICAgZm9udC1zaXplOiA4MHB4O1xyXG4gICAgd2lkdGg6IDgwcHg7XHJcbiAgICBoZWlnaHQ6IDgwcHg7XHJcbiAgICBjb2xvcjogI2NiZDVlMDtcclxuICAgIG1hcmdpbjogMCBhdXRvIDIwcHg7XHJcbiAgfVxyXG5cclxuICBoMyB7XHJcbiAgICBmb250LXNpemU6IDEuNXJlbTtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICBjb2xvcjogIzRhNTU2ODtcclxuICAgIG1hcmdpbjogMCAwIDhweCAwO1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBmb250LXNpemU6IDFyZW07XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgfVxyXG59XHJcblxyXG4ucHJvamVjdHMtZ3JpZCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMTJweDtcclxuICBmbGV4OiAxO1xyXG4gIG92ZXJmbG93LXk6IGF1dG87XHJcbiAgcGFkZGluZy1yaWdodDogOHB4O1xyXG4gIG1pbi1oZWlnaHQ6IDA7XHJcblxyXG4gIC8vIEN1c3RvbSBzY3JvbGxiYXJcclxuICAmOjotd2Via2l0LXNjcm9sbGJhciB7XHJcbiAgICB3aWR0aDogOHB4O1xyXG4gIH1cclxuXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdHJhY2sge1xyXG4gICAgYmFja2dyb3VuZDogI2UyZThmMDtcclxuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICB9XHJcblxyXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRodW1iIHtcclxuICAgIGJhY2tncm91bmQ6ICNjYmQ1ZTA7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6ICNhMGFlYzA7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4ucHJvamVjdC1jYXJkIHtcclxuICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgdHJhbnNpdGlvbjogYWxsIDAuM3MgY3ViaWMtYmV6aWVyKDAuNCwgMCwgMC4yLCAxKTtcclxuICBib3JkZXItcmFkaXVzOiAxMnB4O1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMmU4ZjA7XHJcbiAgYm94LXNoYWRvdzogMCAxcHggM3B4IHJnYmEoMCwgMCwgMCwgMC4wNSk7XHJcbiAgYmFja2dyb3VuZDogd2hpdGU7XHJcbiAgcG9zaXRpb246IHJlbGF0aXZlO1xyXG4gIG92ZXJmbG93OiB2aXNpYmxlO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcclxuICAgIGJveC1zaGFkb3c6IDAgOHB4IDE2cHggcmdiYSgwLCAwLCAwLCAwLjEpO1xyXG4gICAgYm9yZGVyLWNvbG9yOiAjNjY3ZWVhO1xyXG4gIH1cclxuXHJcbiAgLy8gU3BlY2lhbCBzdHlsaW5nIGZvciBsYXN0IG9wZW5lZCBwcm9qZWN0XHJcbiAgJi5sYXN0LW9wZW5lZCB7XHJcbiAgICBib3JkZXI6IDJweCBzb2xpZCAjNjY3ZWVhO1xyXG4gICAgYm94LXNoYWRvdzogMCA0cHggMTJweCByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMik7XHJcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMDMpIDAlLCByZ2JhKDExOCwgNzUsIDE2MiwgMC4wMykgMTAwJSk7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtNHB4KTtcclxuICAgICAgYm94LXNoYWRvdzogMCAxMnB4IDI0cHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjMpO1xyXG4gICAgICBib3JkZXItY29sb3I6ICM3NjRiYTI7XHJcbiAgICB9XHJcblxyXG4gICAgLnByb2plY3QtaWNvbi13cmFwcGVyIHtcclxuICAgICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcclxuICAgICAgYm94LXNoYWRvdzogMCA0cHggMTJweCByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuNCk7XHJcbiAgICAgIGFuaW1hdGlvbjogcHVsc2UgMnMgZWFzZS1pbi1vdXQgaW5maW5pdGU7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1jYXJkLWNvbnRlbnQge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XHJcbiAgICBwYWRkaW5nOiAxNnB4O1xyXG4gICAgZ2FwOiAxMnB4O1xyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtaW5mbyB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogMTJweDtcclxuICAgIGZsZXg6IDE7XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcbiAgICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1pY29uLXdyYXBwZXIge1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgICB3aWR0aDogNDBweDtcclxuICAgIGhlaWdodDogNDBweDtcclxuICAgIGJvcmRlci1yYWRpdXM6IDEwcHg7XHJcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuXHJcbiAgICAucHJvamVjdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICB3aWR0aDogMjBweDtcclxuICAgICAgaGVpZ2h0OiAyMHB4O1xyXG4gICAgICBjb2xvcjogd2hpdGU7XHJcbiAgICB9XHJcblxyXG4gICAgLnByb2plY3QtaWNvbi1zdmcge1xyXG4gICAgICB3aWR0aDogMjBweDtcclxuICAgICAgaGVpZ2h0OiAyMHB4O1xyXG4gICAgICBmaWx0ZXI6IGJyaWdodG5lc3MoMCkgaW52ZXJ0KDEpOyAvLyBNYWtlcyBTVkcgd2hpdGVcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWRldGFpbHMge1xyXG4gICAgZmxleDogMTtcclxuICAgIG1pbi13aWR0aDogMDtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gICAgZ2FwOiA0cHg7XHJcblxyXG4gICAgLnByb2plY3QtbmFtZSB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAxLjFyZW07XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICAgIGNvbG9yOiAjMmQzNzQ4O1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1wYXRoIHtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgICBmb250LXNpemU6IDAuOHJlbTtcclxuICAgICAgY29sb3I6ICM3MTgwOTY7XHJcbiAgICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbiAgICAgIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgICAgIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gICAgfVxyXG5cclxuICAgIC5wcm9qZWN0LW1ldGEge1xyXG4gICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICBnYXA6IDEycHg7XHJcbiAgICAgIG1hcmdpbi10b3A6IDRweDtcclxuICAgIH1cclxuXHJcbiAgICAubGFzdC1vcGVuZWQtYmFkZ2Uge1xyXG4gICAgICBkaXNwbGF5OiBpbmxpbmUtZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgZ2FwOiA0cHg7XHJcbiAgICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgICAgcGFkZGluZzogNHB4IDEwcHg7XHJcbiAgICAgIGJvcmRlci1yYWRpdXM6IDEycHg7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC42NXJlbTtcclxuICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcclxuICAgICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcclxuICAgICAgbGV0dGVyLXNwYWNpbmc6IDAuNXB4O1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBib3gtc2hhZG93OiAwIDJweCA2cHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjMpO1xyXG4gICAgICBhbmltYXRpb246IGZhZGVJblNjYWxlIDAuNXMgZWFzZS1vdXQ7XHJcbiAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG5cclxuICAgICAgLmJhZGdlLWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMTJweDtcclxuICAgICAgICB3aWR0aDogMTJweDtcclxuICAgICAgICBoZWlnaHQ6IDEycHg7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1kYXRlIHtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgICBmb250LXNpemU6IDAuN3JlbTtcclxuICAgICAgY29sb3I6ICNhMGFlYzA7XHJcbiAgICAgIGZvbnQtc3R5bGU6IGl0YWxpYztcclxuICAgICAgZmxleDogMTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWFjdGlvbnMge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGdhcDogNHB4O1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcblxyXG4gICAgLmFjdGlvbi1idG4ge1xyXG4gICAgICB0cmFuc2l0aW9uOiBhbGwgMC4ycyBlYXNlO1xyXG5cclxuICAgICAgJi5zaGFyZS1idG46aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoNzYsIDE3NSwgODAsIDAuMSk7XHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcblxyXG4gICAgICAmLnNldHRpbmdzLWJ0bjpob3ZlciB7XHJcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjEpO1xyXG4gICAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICAgIGNvbG9yOiAjNjY3ZWVhO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgJi5kZWxldGUtYnRuOmhvdmVyIHtcclxuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDI0NSwgODcsIDEwOCwgMC4xKTtcclxuICAgICAgICBtYXQtaWNvbiB7XHJcbiAgICAgICAgICBjb2xvcjogI2Y1NTc2YztcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgICAgaGVpZ2h0OiAyMHB4O1xyXG4gICAgICAgIGNvbG9yOiAjYTBhZWMwO1xyXG4gICAgICAgIHRyYW5zaXRpb246IGNvbG9yIDAuMnMgZWFzZTtcclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLy8gU2lkZWJhciBzZWN0aW9uIChoaWRkZW4gYnkgZGVmYXVsdClcclxuLnNpZGViYXItc2VjdGlvbiB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxufVxyXG5cclxuLy8gUmVzcG9uc2l2ZSBkZXNpZ25cclxuQG1lZGlhIChtYXgtd2lkdGg6IDEwMjRweCkge1xyXG4gIC5tYWluLWxheW91dCB7XHJcbiAgICBncmlkLXRlbXBsYXRlLWNvbHVtbnM6IDFmcjtcclxuICAgIGdhcDogMzJweDtcclxuICB9XHJcblxyXG4gIC5yaWdodC1jb2x1bW4ge1xyXG4gICAgcG9zaXRpb246IHN0YXRpYztcclxuICB9XHJcblxyXG4gIC5hY3Rpb25zLWdyaWQge1xyXG4gICAgZGlzcGxheTogZ3JpZDtcclxuICAgIGdyaWQtdGVtcGxhdGUtY29sdW1uczogcmVwZWF0KGF1dG8tZml0LCBtaW5tYXgoMjgwcHgsIDFmcikpO1xyXG4gICAgZ2FwOiAxNnB4O1xyXG4gIH1cclxufVxyXG5cclxuQG1lZGlhIChtYXgtd2lkdGg6IDc2OHB4KSB7XHJcbiAgLmhlYWRlci1zZWN0aW9uIHtcclxuICAgIHBhZGRpbmc6IDQwcHggMjBweCAyMHB4O1xyXG5cclxuICAgIC5tYWluLXRpdGxlIHtcclxuICAgICAgZm9udC1zaXplOiAycmVtO1xyXG4gICAgfVxyXG5cclxuICAgIC5zdWJ0aXRsZSB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMXJlbTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5jb250ZW50LXdyYXBwZXIge1xyXG4gICAgcGFkZGluZzogMzJweCAyMHB4O1xyXG4gICAgYm9yZGVyLXJhZGl1czogMjRweCAyNHB4IDAgMDtcclxuICB9XHJcblxyXG4gIC5hY3Rpb25zLWdyaWQge1xyXG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnI7XHJcbiAgfVxyXG5cclxuICAuYWN0aW9uLWNhcmQgLmNhcmQtY29udGVudCB7XHJcbiAgICBwYWRkaW5nOiAyNHB4IDIwcHg7XHJcbiAgfVxyXG5cclxuICAuc2VjdGlvbi10aXRsZSB7XHJcbiAgICBmb250LXNpemU6IDEuMjVyZW07XHJcbiAgfVxyXG5cclxuICAuc2VjdGlvbi1oZWFkZXIge1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG5cclxuICAgIC5zZWFyY2gtZmllbGQge1xyXG4gICAgICBtYXgtd2lkdGg6IDEwMCU7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBBbmltYXRpb25zXHJcbkBrZXlmcmFtZXMgcHVsc2Uge1xyXG4gIDAlLCAxMDAlIHtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMSk7XHJcbiAgfVxyXG4gIDUwJSB7XHJcbiAgICB0cmFuc2Zvcm06IHNjYWxlKDEuMDUpO1xyXG4gIH1cclxufVxyXG5cclxuQGtleWZyYW1lcyBmYWRlSW5TY2FsZSB7XHJcbiAgMCUge1xyXG4gICAgb3BhY2l0eTogMDtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMC44KTtcclxuICB9XHJcbiAgMTAwJSB7XHJcbiAgICBvcGFjaXR5OiAxO1xyXG4gICAgdHJhbnNmb3JtOiBzY2FsZSgxKTtcclxuICB9XHJcbn1cclxuXHJcbi8vIERpYWdub3N0aWNzIHNlY3Rpb25cclxuLmRpYWdub3N0aWNzLXNlY3Rpb24ge1xyXG4gIG1hcmdpbi10b3A6IDI0cHg7XHJcbiAgcGFkZGluZy10b3A6IDE2cHg7XHJcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICNlMmU4ZjA7XHJcblxyXG4gIGJ1dHRvbiB7XHJcbiAgICBmb250LXNpemU6IDAuODVyZW07XHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgICAgbWFyZ2luLXJpZ2h0OiA2cHg7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcbiJdfQ== */"] });


/***/ }),

/***/ 132:
/*!*********************************************!*\
  !*** ./src/app/projects/projects.module.ts ***!
  \*********************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectsModule": () => (/* binding */ ProjectsModule)
/* harmony export */ });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/common */ 6362);
/* harmony import */ var _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./open-recent/open-recent.component */ 7111);
/* harmony import */ var _projects_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./projects.component */ 5609);
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./new-project/new-project.component */ 5389);
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ 7179);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/router */ 2816);
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../shared/material.module */ 4872);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/forms */ 587);
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../git/git.module */ 1312);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./services/project-settings.service */ 5450);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/core */ 3184);















const routes = [
    {
        path: '', component: _projects_component__WEBPACK_IMPORTED_MODULE_1__.ProjectsComponent,
        children: [
            { path: '', redirectTo: 'openrecent', pathMatch: 'full' },
            { path: 'openrecent', component: _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__.OpenRecentComponent },
            { path: 'newproject', component: _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__.NewProjectComponent },
        ]
    }
];
class ProjectsModule {
    constructor() {
        console.log('constructor ProjectsModule');
    }
}
ProjectsModule.ɵfac = function ProjectsModule_Factory(t) { return new (t || ProjectsModule)(); };
ProjectsModule.ɵmod = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineNgModule"]({ type: ProjectsModule });
ProjectsModule.ɵinj = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineInjector"]({ providers: [
        _services_project_settings_service__WEBPACK_IMPORTED_MODULE_9__.ProjectSettingsService
    ], imports: [[
            _angular_router__WEBPACK_IMPORTED_MODULE_11__.RouterModule.forChild(routes),
            _angular_common__WEBPACK_IMPORTED_MODULE_12__.CommonModule,
            _shared_material_module__WEBPACK_IMPORTED_MODULE_7__.MaterialModule,
            _angular_forms__WEBPACK_IMPORTED_MODULE_13__.FormsModule,
            _git_git_module__WEBPACK_IMPORTED_MODULE_8__.GitModule,
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵsetNgModuleScope"](ProjectsModule, { declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__.OpenRecentComponent,
        _projects_component__WEBPACK_IMPORTED_MODULE_1__.ProjectsComponent,
        _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__.NewProjectComponent,
        _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__.CloneProjectComponent,
        _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__.ModernCloneProjectComponent,
        _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__.ProjectCreateConfigDialogComponent,
        _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_6__.ProjectSettingsComponent], imports: [_angular_router__WEBPACK_IMPORTED_MODULE_11__.RouterModule, _angular_common__WEBPACK_IMPORTED_MODULE_12__.CommonModule,
        _shared_material_module__WEBPACK_IMPORTED_MODULE_7__.MaterialModule,
        _angular_forms__WEBPACK_IMPORTED_MODULE_13__.FormsModule,
        _git_git_module__WEBPACK_IMPORTED_MODULE_8__.GitModule] }); })();


/***/ }),

/***/ 9909:
/*!****************************************************************!*\
  !*** ./src/app/projects/services/ide-configuration.service.ts ***!
  \****************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "IdeConfigurationService": () => (/* binding */ IdeConfigurationService)
/* harmony export */ });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ 4505);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ 8759);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ 8784);




class IdeConfigurationService {
    constructor(http) {
        this.http = http;
        this.apiUrl = '../api/ideConfiguration';
        this.currentConfigSubject = new rxjs__WEBPACK_IMPORTED_MODULE_0__.BehaviorSubject(null);
        this.currentConfig$ = this.currentConfigSubject.asObservable();
    }
    /**
     * Get IDE configuration for a project
     * @param projectPath Optional project path. If not provided, uses current project.
     */
    getIdeConfiguration(projectPath) {
        const params = projectPath ? { projectPath } : {};
        return this.http.get(`${this.apiUrl}/config`, { params }).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.tap)(config => this.currentConfigSubject.next(config)));
    }
    /**
     * Set IDE configuration for a project
     */
    setIdeConfiguration(request) {
        return this.http.post(`${this.apiUrl}/config`, request).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.tap)(() => {
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
IdeConfigurationService.ɵfac = function IdeConfigurationService_Factory(t) { return new (t || IdeConfigurationService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__.HttpClient)); };
IdeConfigurationService.ɵprov = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: IdeConfigurationService, factory: IdeConfigurationService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ 5450:
/*!***************************************************************!*\
  !*** ./src/app/projects/services/project-settings.service.ts ***!
  \***************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectSettingsService": () => (/* binding */ ProjectSettingsService)
/* harmony export */ });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common/http */ 8784);


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
ProjectSettingsService.ɵfac = function ProjectSettingsService_Factory(t) { return new (t || ProjectSettingsService)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_1__.HttpClient)); };
ProjectSettingsService.ɵprov = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: ProjectSettingsService, factory: ProjectSettingsService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ 5811:
/*!****************************************************!*\
  !*** ./src/app/shared/NgDialogAnimationService.ts ***!
  \****************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "NgDialogAnimationService": () => (/* binding */ NgDialogAnimationService)
/* harmony export */ });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ 2218);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 3184);
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ 5758);




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
            const _afterClosed = new rxjs__WEBPACK_IMPORTED_MODULE_0__.Subject();
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
NgDialogAnimationService.ɵfac = function NgDialogAnimationService_Factory(t) { return new (t || NgDialogAnimationService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__.MatDialog), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_1__.NgZone), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"]('INCOMING_OPTION', 8), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"]('OUTGOING_OPTION', 8)); };
NgDialogAnimationService.ɵprov = /*@__PURE__*/ _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: NgDialogAnimationService, factory: NgDialogAnimationService.ɵfac, providedIn: 'root' });


/***/ })

}]);
//# sourceMappingURL=src_app_projects_projects_module_ts.js.map