(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["projects-projects-module"],{

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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ "eAi6");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");


















function CloneProjectComponent_mat_hint_35_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "Enter your password");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
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
    }
    cloneDirectory() {
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_1__["WaitingDialogInfo"]();
        info.message = "Oh MY GOD... Cloning Repository!";
        this.waitingDialog.showMessageBox(info);
        this.gitService.clone(this.dataForCloning).subscribe(_ => {
            if (_.areCredentialsCorrect) {
                this.projectService.setNewFolderProject(this.dataForCloning.directoryPath).subscribe(_ => {
                    this.router.navigate(['/main/navigation/document']); //main
                    this.dialogRef.close();
                });
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
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__["ShowFileSystemComponent"], {
            width: '600px',
            height: '600px',
            data: null
        });
        dialogRef.afterClosed().subscribe(_ => {
            this.dataForCloning.directoryPath = _.data;
        });
    }
}
CloneProjectComponent.ɵfac = function CloneProjectComponent_Factory(t) { return new (t || CloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_5__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_6__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_7__["WaitingDialogService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_8__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_9__["Router"])); };
CloneProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: CloneProjectComponent, selectors: [["app-clone-project"]], decls: 42, vars: 8, consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/gitlogo.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "gitlab url", "required", "", 3, "ngModel", "ngModelChange"], [1, "orizzontal-form-container"], ["appearance", "outline", 2, "width", "100%"], ["matInput", "", "placeholder", "local filesystem", "required", "", 3, "ngModel", "ngModelChange"], ["mat-button", "", "matSuffix", "", "color", "primary", 3, "click"], ["matInput", "", "placeholder", "username", "required", "", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "password", "required", "", 3, "ngModel", "type", "ngModelChange"], ["matSuffix", "", 3, "click"], [4, "ngIf"], [1, "store-credential"], [3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-stroked-button", "", "color", "primary", 3, "click"]], template: function CloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h1", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "img", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Clone Project");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-card", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9, "URL");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](10, "input", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_10_listener($event) { return ctx.dataForCloning.urlPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](12, "Set GIT Path");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](14, "mat-form-field", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](15, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](16, "Directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](17, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_17_listener($event) { return ctx.dataForCloning.directoryPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](18, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](19, "Set project directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](20, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_20_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](21, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](22, "more_horiz");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](23, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](24, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](25, "UserName");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "input", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_26_listener($event) { return ctx.dataForCloning.username = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](27, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](28, "Username");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](29, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](30, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](31, "Password");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](32, "input", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_32_listener($event) { return ctx.dataForCloning.password = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](33, "mat-icon", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function CloneProjectComponent_Template_mat_icon_click_33_listener() { return ctx.hide = !ctx.hide; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](34);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](35, CloneProjectComponent_mat_hint_35_Template, 2, 0, "mat-hint", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](36, "section", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](37, "mat-checkbox", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_mat_checkbox_ngModelChange_37_listener($event) { return ctx.dataForCloning.storeCredentials = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](38, "Store credentials");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](39, "mat-dialog-actions", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](40, "button", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_40_listener() { return ctx.cloneDirectory(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](41, "Clone!");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.dataForCloning.urlPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.dataForCloning.directoryPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.dataForCloning.username);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.dataForCloning.password)("type", ctx.hide ? "password" : "text");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx.hide ? "visibility_off" : "visibility");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.dataForCloning.password);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.dataForCloning.storeCredentials);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialogContent"], _angular_material_card__WEBPACK_IMPORTED_MODULE_10__["MatCard"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_12__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatHint"], _angular_material_button__WEBPACK_IMPORTED_MODULE_14__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatSuffix"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_16__["NgIf"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_17__["MatCheckbox"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialogActions"]], styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.orizzontal-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcY2xvbmUtcHJvamVjdC5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0FBQ0YiLCJmaWxlIjoiY2xvbmUtcHJvamVjdC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi52ZXJ0aWNhbC1mb3JtLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4ub3JpenpvbnRhbC1mb3JtLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogcm93O1xyXG59XHJcbiJdfQ== */"] });


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
        debugger;
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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");









const _c0 = function (a0) { return { delay: a0 }; };
const _c1 = function (a1) { return { value: "", params: a1 }; };
function OpenRecentComponent_mat_card_2_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-card");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-card-actions");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_6_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r3.quickOpenNotes(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](7, "img", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, " Quick notes ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r5.openNewProject(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_11_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r4); const element_r1 = ctx.$implicit; const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r6.deleteProject(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r1 = ctx.$implicit;
    const i_r2 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("@bounceInLeftOnEnter", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpureFunction1"](5, _c1, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpureFunction1"](3, _c0, i_r2 * 100)));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](element_r1.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](element_r1.path);
} }
class OpenRecentComponent {
    constructor(projectService, mdFileService, router, route) {
        this.projectService = projectService;
        this.mdFileService = mdFileService;
        this.router = router;
        this.route = route;
    }
    ngOnInit() {
        this.projectService.fetchProjects();
        this.dataSource = this.projectService.mdProjects;
    }
    quickOpenNotes(path) {
        this.projectService.setNewFolderProjectQuickNotes(path).subscribe(_ => {
            this.router.navigate(['/main/navigation/document']);
        });
    }
    openNewProject(path) {
        this.projectService.setNewFolderProject(path).subscribe(_ => {
            this.router.navigate(['/main/navigation/document']);
        });
    }
    getProjectList(data, objectThis) {
        objectThis.projectService.fetchProjects();
    }
    ;
    deleteProject(project) {
        this.projectService.deleteProject(project, this.getProjectList, this);
    }
}
OpenRecentComponent.ɵfac = function OpenRecentComponent_Factory(t) { return new (t || OpenRecentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["ActivatedRoute"])); };
OpenRecentComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: OpenRecentComponent, selectors: [["app-open-recent"]], decls: 4, vars: 3, consts: [[4, "ngFor", "ngForOf"], ["mat-stroked-button", "", "color", "accent", 3, "click"], ["src", "/assets/run.png"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-raised-button", "", "color", "warn", 3, "click"]], template: function OpenRecentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Recent Projects");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](2, OpenRecentComponent_mat_card_2_Template, 14, 7, "mat-card", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "async");
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 1, ctx.dataSource));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_5__["NgForOf"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_5__["AsyncPipe"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJvcGVuLXJlY2VudC5jb21wb25lbnQuc2NzcyJ9 */"], data: { animation: [
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
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../git/git.module */ "Trdj");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/core */ "fXoL");











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
ProjectsModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdefineNgModule"]({ type: ProjectsModule });
ProjectsModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdefineInjector"]({ imports: [[
            _angular_router__WEBPACK_IMPORTED_MODULE_5__["RouterModule"].forChild(routes),
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_7__["FormsModule"],
            _git_git_module__WEBPACK_IMPORTED_MODULE_8__["GitModule"],
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵsetNgModuleScope"](ProjectsModule, { declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__["OpenRecentComponent"],
        _projects_component__WEBPACK_IMPORTED_MODULE_2__["ProjectsComponent"],
        _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__["NewProjectComponent"],
        _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_4__["CloneProjectComponent"]], imports: [_angular_router__WEBPACK_IMPORTED_MODULE_5__["RouterModule"], _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_7__["FormsModule"],
        _git_git_module__WEBPACK_IMPORTED_MODULE_8__["GitModule"]] }); })();


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
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ "Bw61");
/* harmony import */ var _md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../md-explorer/components/dialogs/settings/settings.component */ "ATen");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../signalr/services/server-messages.service */ "jQFx");
/* harmony import */ var _shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../shared/NgDialogAnimationService */ "O0mF");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/button */ "bTqV");











class ProjectsComponent {
    constructor(projectService, dialog, router, signalRService, dialogAn) {
        this.projectService = projectService;
        this.dialog = dialog;
        this.router = router;
        this.signalRService = signalRService;
        this.dialogAn = dialogAn;
        this.dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }];
    }
    ngOnDestroy() {
        console.log('ProjectsComponent destroyed!');
    }
    ngOnInit() {
    }
    openRecent() {
        this.router.navigate(['/projects/openrecent']);
    }
    prepareToClone() {
        const dialogRef = this.dialog.open(_dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_1__["CloneProjectComponent"], {
            width: '600px',
            maxHeight: '600px',
            data: null
        });
    }
    openNewFolder() {
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__["ShowFileSystemComponent"], {
            width: '600px',
            maxHeight: '600px',
            data: null
        });
        dialogRef.afterClosed().subscribe(folderPath => {
            this.projectService.setNewFolderProject(folderPath.data).subscribe(_ => {
                this.router.navigate(['/main/navigation/document']); //main        
            });
        });
    }
    openSettings() {
        const dialogRef = this.dialogAn.open(_md_explorer_components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_2__["SettingsComponent"], {
            width: '600px',
            animation: {},
        });
    }
}
ProjectsComponent.ɵfac = function ProjectsComponent_Factory(t) { return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_4__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_5__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_6__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_7__["ServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_8__["NgDialogAnimationService"])); };
ProjectsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: ProjectsComponent, selectors: [["app-projects"]], decls: 37, vars: 0, consts: [[1, "container-flex"], [1, "container-title"], [1, "item"], [1, "container-columns"], [1, "container-first-column"], ["mat-stroked-button", "", "color", "primary", 3, "click"]], template: function ProjectsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, "Markdown Explorer 2023-05-22.001");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](6, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](9, "router-outlet");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](10, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](12, "Get started");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](14, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](15, "Clone a repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](16, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](17, "Get code from an online repository like GitLab");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](18, "mat-card-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](19, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_19_listener() { return ctx.prepareToClone(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](20, "Clone");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](21, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](22, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](23, "Create new project");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](24, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](25, "Choose filesystem folder to get started");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "mat-card-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](27, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_27_listener() { return ctx.openNewFolder(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](28, "Create");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](29, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](30, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](31, "General settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](32, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](33, "Change main settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](34, "mat-card-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](35, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_35_listener() { return ctx.openSettings(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](36, "Settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } }, directives: [_angular_router__WEBPACK_IMPORTED_MODULE_6__["RouterOutlet"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_10__["MatButton"]], styles: [".container-flex[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-content: center;\n}\n\n.container-title[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: center;\n}\n\n.container-columns[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  justify-content: center;\n}\n\n.container-first-column[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  row-gap: 10px;\n  column-gap: 20px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXHByb2plY3RzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EscUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtBQUNGIiwiZmlsZSI6InByb2plY3RzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmNvbnRhaW5lci1mbGV4IHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47ICBcclxuICBhbGlnbi1jb250ZW50OmNlbnRlcjtcclxufVxyXG5cclxuLmNvbnRhaW5lci10aXRsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxufVxyXG5cclxuLmNvbnRhaW5lci1jb2x1bW5zIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbn1cclxuXHJcbi5jb250YWluZXItZmlyc3QtY29sdW1uIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47ICAgIFxyXG4gIHJvdy1nYXA6IDEwcHg7XHJcbiAgY29sdW1uLWdhcDogMjBweDtcclxufVxyXG4iXX0= */"] });


/***/ })

}]);
//# sourceMappingURL=projects-projects-module.js.map