(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["md-explorer-md-explorer-module"],{

/***/ "4dB/":
/*!*****************************************************************************!*\
  !*** ./src/app/md-explorer/components/git-changes/git-changes.component.ts ***!
  \*****************************************************************************/
/*! exports provided: GitChangesComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitChangesComponent", function() { return GitChangesComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _select_branch_select_branch_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./select-branch/select-branch.component */ "qzEU");
/* harmony import */ var _select_branch_option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./select-branch/option-branch/option-branch.component */ "QGRl");



const _c0 = ["gitChanges"];
class GitChangesComponent {
    constructor(elRef) {
        this.selectPosition = { x: 0, y: 0, visibility: 'hidden' };
        this.matIconSpecial = 'mat-icon-special';
        this.SelectChildrensAreVisible = true;
        this.test1 = 'test1';
        this.selected = 'volvo';
    }
    ngOnInit() {
    }
    setClassOver() {
        this.matIconSpecial = 'mat-icon-special-hover';
    }
    setClassSimple() {
        this.matIconSpecial = 'mat-icon-special';
    }
    setSelectPosition() {
        let test = this.elGitChanges;
        if (this.SelectChildrensAreVisible) {
            this.selectPosition.visibility = 'visible';
            this.SelectChildrensAreVisible = false;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
        else {
            this.selectPosition.visibility = 'hidden';
            this.SelectChildrensAreVisible = true;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
    }
}
GitChangesComponent.ɵfac = function GitChangesComponent_Factory(t) { return new (t || GitChangesComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"])); };
GitChangesComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: GitChangesComponent, selectors: [["app-git-changes"]], viewQuery: function GitChangesComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵloadQuery"]()) && (ctx.elGitChanges = _t.first);
    } }, decls: 2, vars: 0, consts: [["branchName", "branch 1"]], template: function GitChangesComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "app-select-branch");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](1, "app-option-branch", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } }, directives: [_select_branch_select_branch_component__WEBPACK_IMPORTED_MODULE_1__["SelectBranchComponent"], _select_branch_option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_2__["OptionBranchComponent"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LWNoYW5nZXMuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBRUE7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Q0FBQSIsImZpbGUiOiJnaXQtY2hhbmdlcy5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIlxyXG5cclxuLyoubWF0LWljb24tc3BlY2lhbCB7XHJcbiAgcG9zaXRpb246IGFic29sdXRlO1xyXG4gIHBhZGRpbmctdG9wOiAyNHB4O1xyXG4gIHBhZGRpbmctYm90dG9tOiAzcHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogcmdiKDAsIDAsIDAsLjA0KTtcclxuICBib3JkZXItYm90dG9tOiBzb2xpZDtcclxuICBib3JkZXItd2lkdGg6MXB4O1xyXG4gIGJvcmRlci1jb2xvcjogcmdiKDAsIDAsIDApO1xyXG59XHJcblxyXG4ubWF0LWljb24tc3BlY2lhbC1ob3ZlciB7XHJcbiAgcG9zaXRpb246IGFic29sdXRlO1xyXG4gIHBhZGRpbmctdG9wOiAyNHB4O1xyXG4gIHBhZGRpbmctYm90dG9tOiAycHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogcmdiKDAsIDAsIDAsLjA0KTtcclxuICBib3JkZXItYm90dG9tOiBzb2xpZDtcclxuICBib3JkZXItd2lkdGg6IDJweDtcclxuICBib3JkZXItY29sb3I6IHJnYigwLCAwLCAwKTtcclxufVxyXG4qL1xyXG4iXX0= */"] });


/***/ }),

/***/ "8Cql":
/*!***********************************************!*\
  !*** ./src/app/md-explorer/pipes/safePipe.ts ***!
  \***********************************************/
/*! exports provided: SafePipe */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SafePipe", function() { return SafePipe; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/platform-browser */ "jhN1");


class SafePipe {
    constructor(sanitizer) {
        this.sanitizer = sanitizer;
    }
    transform(url) {
        return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    }
}
SafePipe.ɵfac = function SafePipe_Factory(t) { return new (t || SafePipe)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_1__["DomSanitizer"])); };
SafePipe.ɵpipe = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefinePipe"]({ name: "safe", type: SafePipe, pure: true });


/***/ }),

/***/ "A4yT":
/*!***************************************************!*\
  !*** ./src/app/md-explorer/md-explorer.module.ts ***!
  \***************************************************/
/*! exports provided: MdExplorerModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdExplorerModule", function() { return MdExplorerModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./components/sidenav/sidenav.component */ "hS8n");
/* harmony import */ var _components_toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./components/toolbar/toolbar.component */ "K3CX");
/* harmony import */ var _components_main_content_main_content_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./components/main-content/main-content.component */ "n8bz");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _md_explorer_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./md-explorer.component */ "fX/Y");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./services/md-file.service */ "xmhS");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _pipes_safePipe__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./pipes/safePipe */ "8Cql");
/* harmony import */ var _components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./components/dialogs/settings/settings.component */ "ATen");
/* harmony import */ var _components_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./components/refactoring/rename-file/rename-file.component */ "FboE");
/* harmony import */ var _components_rules_rules_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./components/rules/rules.component */ "hsi8");
/* harmony import */ var _components_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./components/dialogs/new-markdown/new-markdown.component */ "WxmV");
/* harmony import */ var _components_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./components/dialogs/new-directory/new-directory.component */ "p8DV");
/* harmony import */ var _components_md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./components/md-tree/md-tree.component */ "s8RO");
/* harmony import */ var _components_git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./components/git-changes/git-changes.component */ "4dB/");
/* harmony import */ var _components_git_changes_select_extended_select_extended_component__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ./components/git-changes/select-extended/select-extended.component */ "r8p0");
/* harmony import */ var _components_git_changes_select_extended_option_extended_option_extended_component__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./components/git-changes/select-extended/option-extended/option-extended.component */ "OpRs");
/* harmony import */ var _components_git_changes_select_branch_select_branch_component__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./components/git-changes/select-branch/select-branch.component */ "qzEU");
/* harmony import */ var _components_git_changes_select_branch_option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ./components/git-changes/select-branch/option-branch/option-branch.component */ "QGRl");
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_component__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ../signalR/dialogs/connection-lost/connection-lost.component */ "9LnC");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_component__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! ../signalR/dialogs/parsing-project/parsing-project.component */ "oPln");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/core */ "fXoL");


























const routes = [
    { path: '', component: _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"] },
    {
        path: 'navigation', component: _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"],
        children: [
            { path: ':path', component: _components_main_content_main_content_component__WEBPACK_IMPORTED_MODULE_3__["MainContentComponent"] },
        ]
    }
];
class MdExplorerModule {
    constructor() {
        console.log('constructor MdExplorerModule');
    }
}
MdExplorerModule.ɵfac = function MdExplorerModule_Factory(t) { return new (t || MdExplorerModule)(); };
MdExplorerModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_24__["ɵɵdefineNgModule"]({ type: MdExplorerModule });
MdExplorerModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_24__["ɵɵdefineInjector"]({ providers: [
        _services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"],
    ], imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_4__["MaterialModule"],
            _angular_common_http__WEBPACK_IMPORTED_MODULE_9__["HttpClientModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_5__["FormsModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_7__["RouterModule"].forChild(routes)
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_24__["ɵɵsetNgModuleScope"](MdExplorerModule, { declarations: [_components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"],
        _components_toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_2__["ToolbarComponent"],
        _pipes_safePipe__WEBPACK_IMPORTED_MODULE_10__["SafePipe"],
        _components_main_content_main_content_component__WEBPACK_IMPORTED_MODULE_3__["MainContentComponent"],
        _md_explorer_component__WEBPACK_IMPORTED_MODULE_6__["MdExplorerComponent"],
        _components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_11__["SettingsComponent"],
        _components_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_12__["RenameFileComponent"],
        _components_rules_rules_component__WEBPACK_IMPORTED_MODULE_13__["RulesComponent"],
        _components_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_14__["NewMarkdownComponent"],
        _components_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_15__["NewDirectoryComponent"],
        _components_md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_16__["MdTreeComponent"],
        _components_git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_17__["GitChangesComponent"],
        _components_git_changes_select_extended_select_extended_component__WEBPACK_IMPORTED_MODULE_18__["SelectExtendedComponent"],
        _components_git_changes_select_extended_option_extended_option_extended_component__WEBPACK_IMPORTED_MODULE_19__["OptionExtendedComponent"],
        _components_git_changes_select_branch_select_branch_component__WEBPACK_IMPORTED_MODULE_20__["SelectBranchComponent"],
        _components_git_changes_select_branch_option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_21__["OptionBranchComponent"],
        _signalR_dialogs_connection_lost_connection_lost_component__WEBPACK_IMPORTED_MODULE_22__["ConnectionLostComponent"],
        _signalR_dialogs_parsing_project_parsing_project_component__WEBPACK_IMPORTED_MODULE_23__["ParsingProjectComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_4__["MaterialModule"],
        _angular_common_http__WEBPACK_IMPORTED_MODULE_9__["HttpClientModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_5__["FormsModule"], _angular_router__WEBPACK_IMPORTED_MODULE_7__["RouterModule"]] }); })();


/***/ }),

/***/ "ATen":
/*!*******************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/settings/settings.component.ts ***!
  \*******************************************************************************/
/*! exports provided: SettingsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SettingsComponent", function() { return SettingsComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_app_current_folder_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../../services/app-current-folder.service */ "TCXh");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");









class SettingsComponent {
    constructor(appCurrentFolder, dialogRef, _snackBar) {
        this.appCurrentFolder = appCurrentFolder;
        this.dialogRef = dialogRef;
        this._snackBar = _snackBar;
    }
    ngOnInit() {
        this.appCurrentFolder.loadSettings();
        this.appCurrentFolder.settings.subscribe((data) => {
            var settings = data.settings;
            if (settings != undefined) {
                this._settings = settings;
                this.plantumlServer = settings.filter(_ => _.name === "PlantumlServer")[0].valueString || null;
                this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
            }
        });
    }
    save() {
        this._settings.filter(_ => _.name === "PlantumlServer")[0].valueString = this.plantumlServer;
        this._settings.filter(_ => _.name === "JiraServer")[0].valueString = this.jiraServer;
        this.appCurrentFolder.saveSettings().subscribe(data => {
            this._snackBar.open("settings saved", "", { duration: 1000 });
        });
        this.dialogRef.close(null);
    }
    dismiss() {
        this.dialogRef.close(null);
    }
}
SettingsComponent.ɵfac = function SettingsComponent_Factory(t) { return new (t || SettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_app_current_folder_service__WEBPACK_IMPORTED_MODULE_1__["AppCurrentFolderService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__["MatSnackBar"])); };
SettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SettingsComponent, selectors: [["app-settings"]], decls: 25, vars: 2, consts: [[1, "example-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "PlantumlServerPath", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "JiraServerPath", 3, "ngModel", "ngModelChange"], ["mat-button", "", "color", "primary", 3, "click"]], template: function SettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, "M\u2193Configurations");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6, "Plantuml server path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_7_listener($event) { return ctx.plantumlServer = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12, "Jira server path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "input", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_13_listener($event) { return ctx.jiraServer = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](15, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_17_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](18, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](19, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](20, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](21, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_21_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](22, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](23, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](24, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.plantumlServer);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.jiraServer);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: [".example-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.example-container[_ngcontent-%COMP%]    > *[_ngcontent-%COMP%] {\n  width: 100%;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0ksYUFBQTtFQUNBLHNCQUFBO0FBQ0o7O0FBRUE7RUFDSSxXQUFBO0FBQ0oiLCJmaWxlIjoic2V0dGluZ3MuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuZXhhbXBsZS1jb250YWluZXJ7XHJcbiAgICBkaXNwbGF5OmZsZXg7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjpjb2x1bW47XHJcbn1cclxuXHJcbi5leGFtcGxlLWNvbnRhaW5lciA+ICp7XHJcbiAgICB3aWR0aDoxMDAlXHJcbn1cclxuIl19 */"] });


/***/ }),

/***/ "FboE":
/*!*****************************************************************************************!*\
  !*** ./src/app/md-explorer/components/refactoring/rename-file/rename-file.component.ts ***!
  \*****************************************************************************************/
/*! exports provided: RenameFileComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "RenameFileComponent", function() { return RenameFileComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-refactoring.service */ "n7Vp");
/* harmony import */ var _angular_material_table__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/table */ "+0xr");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");







function RenameFileComponent_th_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Actions ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function RenameFileComponent_td_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r10 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", element_r10.suggestedAction, " ");
} }
function RenameFileComponent_th_8_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " File Name ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function RenameFileComponent_td_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r11 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", element_r11.fileName, " ");
} }
function RenameFileComponent_th_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Old Link ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function RenameFileComponent_td_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r12 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", element_r12.oldLinkStored, " ");
} }
function RenameFileComponent_th_14_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " New Link ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function RenameFileComponent_td_15_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r13 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", element_r13.newLinkToReplace, " ");
} }
function RenameFileComponent_tr_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "tr", 12);
} }
function RenameFileComponent_tr_17_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "tr", 13);
} }
class RenameFileComponent {
    constructor(mdRefactoringService, dialogRef, data) {
        this.mdRefactoringService = mdRefactoringService;
        this.dialogRef = dialogRef;
        this.data = data;
        this.displayedColumns = ['suggestedAction', 'fileName', 'oldLinkStored', 'newLinkToReplace'];
    }
    ngOnInit() {
        this.mdRefactoringService.getRefactoringSourceActionList(this.data.refactoringSourceActionId).subscribe(data => {
            debugger;
            this.actionList = data;
        }, error => {
            console.log("failed to fetch GetRefactoringFileEvent List");
        });
        ;
    }
    doRefactoring() {
        this.dialogRef.close(null);
    }
}
RenameFileComponent.ɵfac = function RenameFileComponent_Factory(t) { return new (t || RenameFileComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_2__["MdRefactoringService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"])); };
RenameFileComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: RenameFileComponent, selectors: [["app-rename-file"]], decls: 23, vars: 3, consts: [["mat-table", "", 3, "dataSource"], ["matColumnDef", "suggestedAction"], ["mat-header-cell", "", 4, "matHeaderCellDef"], ["mat-cell", "", 4, "matCellDef"], ["matColumnDef", "fileName"], ["matColumnDef", "oldLinkStored"], ["matColumnDef", "newLinkToReplace"], ["mat-header-row", "", 4, "matHeaderRowDef"], ["mat-row", "", 4, "matRowDef", "matRowDefColumns"], ["mat-button", "", "color", "primary", 3, "click"], ["mat-header-cell", ""], ["mat-cell", ""], ["mat-header-row", ""], ["mat-row", ""]], template: function RenameFileComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "WOW! Refactoring Management and analysis");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "table", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](4, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, RenameFileComponent_th_5_Template, 2, 0, "th", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, RenameFileComponent_td_6_Template, 2, 1, "td", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](7, 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](8, RenameFileComponent_th_8_Template, 2, 0, "th", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, RenameFileComponent_td_9_Template, 2, 1, "td", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](10, 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, RenameFileComponent_th_11_Template, 2, 0, "th", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, RenameFileComponent_td_12_Template, 2, 1, "td", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](13, 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, RenameFileComponent_th_14_Template, 2, 0, "th", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](15, RenameFileComponent_td_15_Template, 2, 1, "td", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, RenameFileComponent_tr_16_Template, 1, 0, "tr", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](17, RenameFileComponent_tr_17_Template, 1, 0, "tr", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function RenameFileComponent_Template_button_click_19_listener() { return ctx.doRefactoring(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](21, "ok");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](22, "Ok ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("dataSource", ctx.actionList);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](13);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("matHeaderRowDef", ctx.displayedColumns);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("matRowDefColumns", ctx.displayedColumns);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatTable"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatColumnDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatHeaderCellDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatCellDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatHeaderRowDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatRowDef"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_4__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__["MatIcon"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatHeaderCell"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatCell"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatHeaderRow"], _angular_material_table__WEBPACK_IMPORTED_MODULE_3__["MatRow"]], styles: [".mat-column-suggestedAction[_ngcontent-%COMP%] {\n  width: 32px;\n  border-right: 1px solid currentColor;\n  padding-right: 24px;\n  text-align: center;\n}\n\n.mat-column-fileName[_ngcontent-%COMP%] {\n  width: 32px;\n  border-right: 1px solid currentColor;\n  padding-right: 24px;\n  text-align: center;\n}\n\n.mat-column-oldLinkStored[_ngcontent-%COMP%] {\n  width: 50px;\n  border-right: 1px solid currentColor;\n  padding-right: 24px;\n  text-align: center;\n}\n\n.mat-column-newLinkToReplace[_ngcontent-%COMP%] {\n  width: 50px;\n  border-right: 1px solid currentColor;\n  padding-right: 24px;\n  text-align: center;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHJlbmFtZS1maWxlLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsV0FBQTtFQUNBLG9DQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtBQUNGOztBQUVBO0VBQ0UsV0FBQTtFQUNBLG9DQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtBQUNGOztBQUNBO0VBQ0UsV0FBQTtFQUNBLG9DQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtBQUVGOztBQUFBO0VBQ0UsV0FBQTtFQUNBLG9DQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtBQUdGIiwiZmlsZSI6InJlbmFtZS1maWxlLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm1hdC1jb2x1bW4tc3VnZ2VzdGVkQWN0aW9uIHtcclxuICB3aWR0aDogMzJweDtcclxuICBib3JkZXItcmlnaHQ6IDFweCBzb2xpZCBjdXJyZW50Q29sb3I7XHJcbiAgcGFkZGluZy1yaWdodDogMjRweDtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbn1cclxuXHJcbi5tYXQtY29sdW1uLWZpbGVOYW1lIHtcclxuICB3aWR0aDogMzJweDtcclxuICBib3JkZXItcmlnaHQ6IDFweCBzb2xpZCBjdXJyZW50Q29sb3I7XHJcbiAgcGFkZGluZy1yaWdodDogMjRweDtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbn1cclxuLm1hdC1jb2x1bW4tb2xkTGlua1N0b3JlZCB7XHJcbiAgd2lkdGg6IDUwcHg7XHJcbiAgYm9yZGVyLXJpZ2h0OiAxcHggc29saWQgY3VycmVudENvbG9yO1xyXG4gIHBhZGRpbmctcmlnaHQ6IDI0cHg7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG59XHJcbi5tYXQtY29sdW1uLW5ld0xpbmtUb1JlcGxhY2Uge1xyXG4gIHdpZHRoOiA1MHB4O1xyXG4gIGJvcmRlci1yaWdodDogMXB4IHNvbGlkIGN1cnJlbnRDb2xvcjtcclxuICBwYWRkaW5nLXJpZ2h0OiAyNHB4O1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxufVxyXG5cclxuIl19 */"] });


/***/ }),

/***/ "K3CX":
/*!*********************************************************************!*\
  !*** ./src/app/md-explorer/components/toolbar/toolbar.component.ts ***!
  \*********************************************************************/
/*! exports provided: ToolbarComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ToolbarComponent", function() { return ToolbarComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../dialogs/settings/settings.component */ "ATen");
/* harmony import */ var _refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../refactoring/rename-file/rename-file.component */ "FboE");
/* harmony import */ var _rules_rules_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../rules/rules.component */ "hsi8");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _services_monitor_md_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../services/monitor-md.service */ "AaxG");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ "ofXK");















function ToolbarComponent_span_16_span_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, ">>");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} }
const _c0 = function () { return { "cursor": "pointer", "color": "orange" }; };
const _c1 = function () { return {}; };
function ToolbarComponent_span_16_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, ToolbarComponent_span_16_span_1_Template, 2, 0, "span", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "a", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_span_16_Template_a_click_2_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r4); const doc_r1 = ctx.$implicit; const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](); return ctx_r3.mdFileService.navigationArray.indexOf(doc_r1) < ctx_r3.mdFileService.navigationArray.length - 1 ? ctx_r3.backToDocument(doc_r1) : ctx_r3.return; });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const doc_r1 = ctx.$implicit;
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx_r0.mdFileService.navigationArray.indexOf(doc_r1) > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngStyle", ctx_r0.mdFileService.navigationArray.indexOf(doc_r1) < ctx_r0.mdFileService.navigationArray.length - 1 ? _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction0"](3, _c0) : _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction0"](4, _c1));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](doc_r1.name);
} }
class ToolbarComponent {
    constructor(dialog, monitorMDService, http, _snackBar, router, mdFileService) {
        this.dialog = dialog;
        this.monitorMDService = monitorMDService;
        this.http = http;
        this._snackBar = _snackBar;
        this.router = router;
        this.mdFileService = mdFileService;
        this.toggleSidenav = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.TitleToShow = "MdExplorer";
    }
    openRefactoring() {
        const dialogRef = this.dialog.open(_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_2__["RenameFileComponent"], {
            width: '600px',
        });
    }
    openDialog() {
        const dialogRef = this.dialog.open(_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_1__["SettingsComponent"], {
            width: '300px',
        });
        dialogRef.afterClosed().subscribe(result => {
            console.log('The dialog was closed');
        });
    }
    openRules(data) {
        const dialogRef = this.dialog.open(_rules_rules_component__WEBPACK_IMPORTED_MODULE_3__["RulesComponent"], {
            width: '600px',
            data: data
        });
        dialogRef.afterClosed().subscribe(_ => {
            if (_.refactoringSourceActionId != undefined) {
                this.dialog.open(_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_2__["RenameFileComponent"], {
                    width: '600px',
                    data: _
                });
            }
        });
    }
    ngOnInit() {
        this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
        this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this);
        this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this);
        this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
            if (_ != null) {
                this.mdFileService.navigationArray = [];
                this.absolutePath = _.fullPath;
                this.relativePath = _.relativePath;
            }
        });
        this.mdFileService.serverSelectedMdFile.subscribe(val => {
            var current = val[0];
            if (current != undefined) {
                if (this.mdFileService.navigationArray.length > 0) {
                    if (current.fullPath == this.mdFileService.navigationArray[0].fullPath) {
                        return;
                    }
                }
                this.absolutePath = current.fullPath;
                this.relativePath = current.relativePath;
            }
        });
    }
    showRule1IsBroken(data, objectThis) {
        objectThis.openRules(data);
    }
    sendExportRequest(data, objectThis) {
        const url = '../api/mdexport/' + objectThis.relativePath + '?connectionId=' + data;
        return objectThis.http.get(url)
            .subscribe(data => { console.log(data); });
    }
    showPdfIsready(data, objectThis) {
        let snackRef = objectThis._snackBar.open("seconds: " + data.executionTimeInSeconds, "Open folder", { duration: 5000, verticalPosition: 'top' });
        snackRef.onAction().subscribe(() => {
            const url = '../api/AppSettings/OpenChromePdf?path=' + data.path;
            return objectThis.http.get(url)
                .subscribe(data => { console.log(data); });
        });
    }
    markdownFileIsProcessed(data, objectThis) {
        objectThis.mdFileService.navigationArray.push(data);
        objectThis.mdFileService.setSelectedMdFileFromServer(data);
    }
    OpenEditor() {
        const url = '../api/AppSettings/OpenFile?path=' + this.absolutePath;
        return this.http.get(url)
            .subscribe(data => { console.log(data); });
    }
    Export() {
        this._snackBar.open("Export request queued!", null, { duration: 2000, verticalPosition: 'top' });
        this.monitorMDService.getConnectionId(this.sendExportRequest, this);
    }
    backToDocument(doc) {
        var thatFile = this.mdFileService.navigationArray.find(_ => _.fullPath == doc.fullPath);
        if (thatFile != null && thatFile != undefined) {
            let i = this.mdFileService.navigationArray.length;
            let toDelete = this.mdFileService.navigationArray[i - 1];
            do {
                this.mdFileService.navigationArray.pop();
                i = i - 1;
                toDelete = this.mdFileService.navigationArray[i - 1];
            } while (toDelete != undefined && toDelete.fullPath == thatFile.fullPath);
        }
        this.mdFileService.setSelectedMdFileFromToolbar(doc);
    }
}
ToolbarComponent.ɵfac = function ToolbarComponent_Factory(t) { return new (t || ToolbarComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_monitor_md_service__WEBPACK_IMPORTED_MODULE_5__["MonitorMDService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_6__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_7__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_8__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__["MdFileService"])); };
ToolbarComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: ToolbarComponent, selectors: [["app-toolbar"]], outputs: { toggleSidenav: "toggleSidenav" }, decls: 17, vars: 1, consts: [["color", "primary"], ["mat-icon-button", "", 3, "click"], [4, "ngFor", "ngForOf"], [4, "ngIf"], [3, "ngStyle", "click"]], template: function ToolbarComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-toolbar", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_1_listener() { return ctx.toggleSidenav.emit(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "vertical_split");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_4_listener() { return ctx.OpenEditor(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_7_listener() { return ctx.Export(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "directions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_10_listener() { return ctx.openDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_13_listener() { return ctx.openRefactoring(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](15, "rule");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](16, ToolbarComponent_span_16_Template, 4, 5, "span", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](16);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx.mdFileService.navigationArray);
    } }, directives: [_angular_material_toolbar__WEBPACK_IMPORTED_MODULE_10__["MatToolbar"], _angular_material_button__WEBPACK_IMPORTED_MODULE_11__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgForOf"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgIf"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgStyle"]], styles: [".sidenav-toggle[_ngcontent-%COMP%] {\n  display: none;\n  padding: 0;\n  margin: 8px;\n  min-width: after 56px;\n}\n@media (max-width: 720px) {\n  .sidenav-toggle[_ngcontent-%COMP%] {\n    display: flex;\n    align: center;\n    justify-content: center;\n  }\n}\n.sidenav-toggle[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 30px;\n  height: after 56px;\n  width: after 56px;\n  line-height: after 56px;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcdG9vbGJhci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFHQTtFQUNFLGFBQUE7RUFDQSxVQUFBO0VBQ0EsV0FBQTtFQUNBLHFCQVBVO0FBS1o7QUFJRTtFQU5GO0lBT0ksYUFBQTtJQUNBLGFBQUE7SUFDQSx1QkFBQTtFQURGO0FBQ0Y7QUFHRTtFQUNFLGVBQUE7RUFDQSxrQkFqQlE7RUFrQlIsaUJBbEJRO0VBbUJSLHVCQW5CUTtFQW9CUixZQUFBO0FBREoiLCJmaWxlIjoidG9vbGJhci5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIiRpY29uV2lkdGg6IGFmdGVyIDU2cHg7XHJcblxyXG5cclxuLnNpZGVuYXYtdG9nZ2xlIHtcclxuICBkaXNwbGF5OiBub25lO1xyXG4gIHBhZGRpbmc6IDA7XHJcbiAgbWFyZ2luOiA4cHg7XHJcbiAgbWluLXdpZHRoOiAkaWNvbldpZHRoO1xyXG5cclxuICBAbWVkaWEgKG1heC13aWR0aDo3MjBweCkge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICB9XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMzBweDtcclxuICAgIGhlaWdodDogJGljb25XaWR0aDtcclxuICAgIHdpZHRoOiAkaWNvbldpZHRoO1xyXG4gICAgbGluZS1oZWlnaHQ6ICRpY29uV2lkdGg7XHJcbiAgICBjb2xvcjogd2hpdGU7XHJcbiAgfVxyXG59XHJcbiJdfQ== */"] });


/***/ }),

/***/ "OpRs":
/*!*****************************************************************************************************************!*\
  !*** ./src/app/md-explorer/components/git-changes/select-extended/option-extended/option-extended.component.ts ***!
  \*****************************************************************************************************************/
/*! exports provided: OptionExtendedComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OptionExtendedComponent", function() { return OptionExtendedComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class OptionExtendedComponent {
    constructor() { }
    ngOnInit() {
    }
}
OptionExtendedComponent.ɵfac = function OptionExtendedComponent_Factory(t) { return new (t || OptionExtendedComponent)(); };
OptionExtendedComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: OptionExtendedComponent, selectors: [["app-option-extended"]], inputs: { title: "title" }, decls: 2, vars: 1, template: function OptionExtendedComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.title);
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJvcHRpb24tZXh0ZW5kZWQuY29tcG9uZW50LnNjc3MifQ== */"] });


/***/ }),

/***/ "QGRl":
/*!***********************************************************************************************************!*\
  !*** ./src/app/md-explorer/components/git-changes/select-branch/option-branch/option-branch.component.ts ***!
  \***********************************************************************************************************/
/*! exports provided: OptionBranchComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OptionBranchComponent", function() { return OptionBranchComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class OptionBranchComponent {
    constructor() {
        this.branchName = 'nothing';
        this.searchText = 'not searched';
    }
    ngOnInit() {
    }
}
OptionBranchComponent.ɵfac = function OptionBranchComponent_Factory(t) { return new (t || OptionBranchComponent)(); };
OptionBranchComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: OptionBranchComponent, selectors: [["app-option-branch"]], inputs: { branchName: "branchName", searchText: "searchText" }, decls: 2, vars: 1, template: function OptionBranchComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h3");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.branchName);
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJvcHRpb24tYnJhbmNoLmNvbXBvbmVudC5zY3NzIn0= */"] });


/***/ }),

/***/ "WxmV":
/*!***************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/new-markdown/new-markdown.component.ts ***!
  \***************************************************************************************/
/*! exports provided: NewMarkdownComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NewMarkdownComponent", function() { return NewMarkdownComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");










class NewMarkdownComponent {
    constructor(data, dialogRef, mdFileService) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
    }
    ngOnInit() {
    }
    dismiss() {
        this.dialogRef.close();
    }
    save() {
        this.mdFileService.CreateNewMd(this.data.fullPath, this.markdownTitle, this.data.level)
            .subscribe(data => {
            this.mdFileService.addNewFile(data);
            this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
        });
        this.dialogRef.close();
    }
}
NewMarkdownComponent.ɵfac = function NewMarkdownComponent_Factory(t) { return new (t || NewMarkdownComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
NewMarkdownComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: NewMarkdownComponent, selectors: [["app-new-markdown"]], decls: 19, vars: 1, consts: [[1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "markdownTitle", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-button", "", "color", "primary", 3, "click"]], template: function NewMarkdownComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "new-markdown works!");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Document\u00EC Title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewMarkdownComponent_Template_input_ngModelChange_7_listener($event) { return ctx.markdownTitle = $event; })("keyup.enter", function NewMarkdownComponent_Template_input_keyup_enter_7_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewMarkdownComponent_Template_button_click_11_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](14, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewMarkdownComponent_Template_button_click_15_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.markdownTitle);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: [".simple-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXG5ldy1tYXJrZG93bi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGIiwiZmlsZSI6Im5ldy1tYXJrZG93bi5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5zaW1wbGUtY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuIl19 */"] });


/***/ }),

/***/ "fX/Y":
/*!******************************************************!*\
  !*** ./src/app/md-explorer/md-explorer.component.ts ***!
  \******************************************************/
/*! exports provided: MdExplorerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdExplorerComponent", function() { return MdExplorerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./components/sidenav/sidenav.component */ "hS8n");


class MdExplorerComponent {
    constructor() { }
    ngOnInit() {
    }
}
MdExplorerComponent.ɵfac = function MdExplorerComponent_Factory(t) { return new (t || MdExplorerComponent)(); };
MdExplorerComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: MdExplorerComponent, selectors: [["app-md-explorer"]], decls: 1, vars: 0, template: function MdExplorerComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](0, "app-sidenav");
    } }, directives: [_components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"]], encapsulation: 2 });


/***/ }),

/***/ "hS8n":
/*!*********************************************************************!*\
  !*** ./src/app/md-explorer/components/sidenav/sidenav.component.ts ***!
  \*********************************************************************/
/*! exports provided: SidenavComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SidenavComponent", function() { return SidenavComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_cdk_layout__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/cdk/layout */ "0MNC");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_app_current_folder_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../services/app-current-folder.service */ "TCXh");
/* harmony import */ var _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/sidenav */ "XhcP");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../md-tree/md-tree.component */ "s8RO");
/* harmony import */ var _git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../git-changes/git-changes.component */ "4dB/");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../toolbar/toolbar.component */ "K3CX");














const SMALL_WIDTH_BREAKPOINT = 720;
class SidenavComponent {
    constructor(breakpointObserver, mdFileService, router, currentFolder, ref) {
        this.breakpointObserver = breakpointObserver;
        this.mdFileService = mdFileService;
        this.router = router;
        this.currentFolder = currentFolder;
        this.ref = ref;
        this.sideNavWidth = "240px";
        this.hooked = false;
        this.classForBorderDiv = "border-div";
        this.whatClass = "showToolbar";
        document.addEventListener("mousemove", (event) => {
            if (this.hooked) {
                this.sideNavWidth = event.clientX + "px";
            }
        });
        document.addEventListener("mouseup", (event) => {
            if (this.hooked) {
                this.stopResizeWidth();
            }
        });
        this.currentFolder.folderName.subscribe((data) => {
            this.titleProject = data.currentFolder;
        });
        this.currentFolder.loadFolderName();
    }
    resizeWidth() {
        this.hooked = true;
        this.classForBorderDiv = "border-div-moving";
    }
    stopResizeWidth() {
        this.hooked = false;
        this.classForBorderDiv = "border-div";
    }
    openProject() {
        this.router.navigate(['/projects']);
    }
    ngOnInit() {
        this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
            .subscribe((state) => {
            this.isScreenSmall = state.matches;
        });
        this.mdFileService.whatDisplayForToolbar.subscribe(_ => {
            if ((_ == 'showToolbar' && this.whatClass != _) ||
                (_ == 'hideToolbar' && this.whatClass != _ + ' ' + 'hideToolbarNone')
            //|| (_ == 'hideToolbar' + ' ' + 'hideToolbarNone' && this.whatClass != _ + ' ' + 'hideToolbarNone')
            ) { // check if something is truely changed
                this.whatClass = _;
                this.sleep(300).then(m => {
                    if (_ == 'hideToolbar' && _ != undefined) {
                        this.whatClass = _ + ' ' + 'hideToolbarNone';
                        this.ref.detectChanges();
                    }
                });
                this.ref.detectChanges();
            }
        });
    }
    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
}
SidenavComponent.ɵfac = function SidenavComponent_Factory(t) { return new (t || SidenavComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_cdk_layout__WEBPACK_IMPORTED_MODULE_1__["BreakpointObserver"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_3__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_app_current_folder_service__WEBPACK_IMPORTED_MODULE_4__["AppCurrentFolderService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectorRef"])); };
SidenavComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SidenavComponent, selectors: [["app-sidenav"]], decls: 20, vars: 7, consts: [["autosize", "", 1, "app-sidenav-container"], [1, "mat-elevation-z10", 3, "opened", "mode"], ["sidenav", ""], ["color", "primary"], ["mat-icon-button", "", 3, "click"], ["headerPosition", "below", 2, "justify-content", "space-between", "height", "calc(100% - 64px)"], ["label", "Docs Explorer"], ["label", "Git Changes"], [1, "flex-container"], [3, "ngClass", "pointerdown", "mouseup"], [2, "width", "100%", "height", "100%"], [1, "app-sidenav-content", 2, "height", "100%"], [3, "ngClass", "toggleSidenav"]], template: function SidenavComponent_Template(rf, ctx) { if (rf & 1) {
        const _r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-sidenav-container", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "mat-sidenav", 1, 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "mat-toolbar", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SidenavComponent_Template_button_click_5_listener() { return ctx.openProject(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7, "ballot");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "mat-tab-group", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "mat-tab", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](11, "app-md-tree");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "mat-tab", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](13, "app-git-changes");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "div", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("pointerdown", function SidenavComponent_Template_div_pointerdown_15_listener() { return ctx.resizeWidth(); })("mouseup", function SidenavComponent_Template_div_mouseup_15_listener() { return ctx.stopResizeWidth(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "div", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](18, "app-toolbar", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("toggleSidenav", function SidenavComponent_Template_app_toolbar_toggleSidenav_18_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r1); const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵreference"](2); return _r0.toggle(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](19, "router-outlet");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵstyleProp"]("width", ctx.sideNavWidth);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("opened", !ctx.isScreenSmall)("mode", !ctx.isScreenSmall ? "side" : "over");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", ctx.titleProject, " ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngClass", ctx.classForBorderDiv);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngClass", ctx.whatClass);
    } }, directives: [_angular_material_sidenav__WEBPACK_IMPORTED_MODULE_5__["MatSidenavContainer"], _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_5__["MatSidenav"], _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_6__["MatToolbar"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_9__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_9__["MatTab"], _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_10__["MdTreeComponent"], _git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_11__["GitChangesComponent"], _angular_common__WEBPACK_IMPORTED_MODULE_12__["NgClass"], _toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_13__["ToolbarComponent"], _angular_router__WEBPACK_IMPORTED_MODULE_3__["RouterOutlet"]], styles: [".app-sidenav-container[_ngcontent-%COMP%] {\n  flex: 1;\n  position: fixed;\n  width: 100%;\n  min-width: 100%;\n  height: 100%;\n  min-height: 100%;\n}\n\n.app-sidenav-content[_ngcontent-%COMP%] {\n  display: flex;\n  height: 100%;\n  width: auto;\n  flex-direction: column;\n}\n\n.app-sidenav[_ngcontent-%COMP%] {\n  width: 240px;\n}\n\na[_ngcontent-%COMP%]:active {\n  color: red;\n}\n\n.flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  width: 100%;\n  height: 100%;\n  min-width: 100%;\n  min-height: 100%;\n}\n\n.border-div[_ngcontent-%COMP%] {\n  border-left: 10px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.border-div-moving[_ngcontent-%COMP%] {\n  border-left: 50px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.showStartToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  display: contents;\n}\n\n.showToolbar[_ngcontent-%COMP%] {\n  height: 64px;\n  transition: height 300ms ease-out;\n}\n\n\n\n.hideToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  transition: height 300ms ease-out;\n}\n\n.hideToolbarNone[_ngcontent-%COMP%] {\n  display: none;\n}\n\n .mat-tab-label,  .mat-tab-label-active {\n  min-width: 0 !important;\n  padding: 3px !important;\n  margin: 3px !important;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2lkZW5hdi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFJQTtFQUNFLE9BQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFIRjs7QUFNQTtFQUNFLGFBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLHNCQUFBO0FBSEY7O0FBTUE7RUFDRSxZQUFBO0FBSEY7O0FBTUE7RUFDRSxVQUFBO0FBSEY7O0FBTUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQUhGOztBQU1BO0VBQ0UsaUJBQUE7RUFDQSx3QkFBQTtFQUNBLHVCQUFBO0VBQ0EsZ0JBQUE7QUFIRjs7QUFNQTtFQUNFLGlCQUFBO0VBQ0Esd0JBQUE7RUFDQSx1QkFBQTtFQUNBLGdCQUFBO0FBSEY7O0FBTUE7RUFDSSxXQUFBO0VBQ0EsaUJBQUE7QUFISjs7QUFPQTtFQUNFLFlBQUE7RUFDQSxpQ0FBQTtBQUpGOztBQU9BOztFQUFBOztBQUlBO0VBQ0UsV0FBQTtFQUNBLGlDQUFBO0FBTEY7O0FBUUE7RUFDSSxhQUFBO0FBTEo7O0FBVUE7RUFDRSx1QkFBQTtFQUNBLHVCQUFBO0VBQ0Esc0JBQUE7QUFQRiIsImZpbGUiOiJzaWRlbmF2LmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiXHJcblxyXG5cclxuXHJcbi5hcHAtc2lkZW5hdi1jb250YWluZXIge1xyXG4gIGZsZXg6IDE7XHJcbiAgcG9zaXRpb246IGZpeGVkO1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIG1pbi13aWR0aDogMTAwJTtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgbWluLWhlaWdodDogMTAwJTtcclxufVxyXG5cclxuLmFwcC1zaWRlbmF2LWNvbnRlbnQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgaGVpZ2h0OiAxMDAlO1xyXG4gIHdpZHRoOmF1dG87XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxufVxyXG5cclxuLmFwcC1zaWRlbmF2IHtcclxuICB3aWR0aDogMjQwcHg7XHJcbn1cclxuXHJcbmE6YWN0aXZlIHtcclxuICBjb2xvcjogcmVkO1xyXG59XHJcblxyXG4uZmxleC1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IHJvdzsgIFxyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBtaW4td2lkdGg6IDEwMCU7XHJcbiAgbWluLWhlaWdodDogMTAwJTtcclxufVxyXG5cclxuLmJvcmRlci1kaXYge1xyXG4gIGJvcmRlci1sZWZ0OiAxMHB4O1xyXG4gIGJvcmRlci1sZWZ0LXN0eWxlOiBzb2xpZDtcclxuICBib3JkZXItbGVmdC1jb2xvcjogZ3JleTtcclxuICBjdXJzb3I6dy1yZXNpemU7XHJcbn1cclxuXHJcbi5ib3JkZXItZGl2LW1vdmluZyB7XHJcbiAgYm9yZGVyLWxlZnQ6IDUwcHg7XHJcbiAgYm9yZGVyLWxlZnQtc3R5bGU6IHNvbGlkO1xyXG4gIGJvcmRlci1sZWZ0LWNvbG9yOiBncmV5O1xyXG4gIGN1cnNvcjogdy1yZXNpemU7XHJcbn1cclxuXHJcbi5zaG93U3RhcnRUb29sYmFye1xyXG4gICAgaGVpZ2h0OjBweDtcclxuICAgIGRpc3BsYXk6Y29udGVudHM7XHJcbn1cclxuXHJcbi8vIE5hc2NvbmRlcmUgZSB2aXN1YWxpenphcmUgbGEgdG9vbGJhclxyXG4uc2hvd1Rvb2xiYXIge1xyXG4gIGhlaWdodDogNjRweDtcclxuICB0cmFuc2l0aW9uOiBoZWlnaHQgMzAwbXMgZWFzZS1vdXQ7XHJcbn1cclxuXHJcbi8qLnNob3dUb29sYmFyQmxvY2t7XHJcbiAgICBkaXNwbGF5OmJsb2NrO1xyXG59Ki9cclxuXHJcbi5oaWRlVG9vbGJhciB7XHJcbiAgaGVpZ2h0OjBweDtcclxuICB0cmFuc2l0aW9uOiAgaGVpZ2h0IDMwMG1zIGVhc2Utb3V0OyAgXHJcbn1cclxuXHJcbi5oaWRlVG9vbGJhck5vbmV7XHJcbiAgICBkaXNwbGF5Om5vbmU7XHJcbn1cclxuXHJcblxyXG4vLyBwZXIgZmFyZSBwaWNjb2xpIGkgdGFiXHJcbjo6bmctZGVlcC5tYXQtdGFiLWxhYmVsLCA6Om5nLWRlZXAubWF0LXRhYi1sYWJlbC1hY3RpdmUge1xyXG4gIG1pbi13aWR0aDogMCAhaW1wb3J0YW50O1xyXG4gIHBhZGRpbmc6IDNweCAhaW1wb3J0YW50O1xyXG4gIG1hcmdpbjogM3B4ICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcbiJdfQ== */"] });


/***/ }),

/***/ "hsi8":
/*!*****************************************************************!*\
  !*** ./src/app/md-explorer/components/rules/rules.component.ts ***!
  \*****************************************************************/
/*! exports provided: RulesComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "RulesComponent", function() { return RulesComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/md-refactoring.service */ "n7Vp");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");








class RulesComponent {
    constructor(dialogRef, data, refactoringService, mdFileService) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.refactoringService = refactoringService;
        this.mdFileService = mdFileService;
    }
    ngOnInit() {
        this.message = this.data.message;
        this.fromFileName = this.data.fromFileName;
        this.toFileName = this.data.toFileName;
    }
    changeName() {
        this.refactoringService.renameFileName(this.data)
            .subscribe(data => {
            var oldPath = data.relativePath + '\\' + data.oldName;
            var newPath = data.relativePath + '\\' + data.newName;
            var oldFile = new _models_md_file__WEBPACK_IMPORTED_MODULE_1__["MdFile"](data.oldName, oldPath, data.oldLevel, false);
            oldFile.relativePath = oldPath;
            oldFile.fullPath = data.oldPath;
            var newFile = new _models_md_file__WEBPACK_IMPORTED_MODULE_1__["MdFile"](data.newName, newPath, data.newLevel, false);
            newFile.fullPath = data.newPath;
            newFile.relativePath = newPath;
            this.mdFileService.changeDataStoreMdFiles(oldFile, newFile);
            debugger;
            this.dialogRef.close(data);
        });
    }
    closeDialog(data) {
        this.dialogRef.close(null);
    }
    dismiss() {
        this.dialogRef.close(null);
    }
}
RulesComponent.ɵfac = function RulesComponent_Factory(t) { return new (t || RulesComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_3__["MdRefactoringService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__["MdFileService"])); };
RulesComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: RulesComponent, selectors: [["app-rules"]], decls: 16, vars: 3, consts: [["mat-button", "", "color", "primary", 3, "click"]], template: function RulesComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "rule # 1 broken!");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "button", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function RulesComponent_Template_button_click_8_listener() { return ctx.changeName(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](9, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10, "swap_horiz");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](11, "change file name\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "button", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function RulesComponent_Template_button_click_12_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](13, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](14, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](15, "Cancel\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.message);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.fromFileName);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.toFileName);
    } }, directives: [_angular_material_button__WEBPACK_IMPORTED_MODULE_5__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJydWxlcy5jb21wb25lbnQuc2NzcyJ9 */"] });


/***/ }),

/***/ "n7Vp":
/*!****************************************************************!*\
  !*** ./src/app/md-explorer/services/md-refactoring.service.ts ***!
  \****************************************************************/
/*! exports provided: MdRefactoringService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdRefactoringService", function() { return MdRefactoringService; });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");



class MdRefactoringService {
    constructor(http) {
        this.http = http;
    }
    getFileEventList() {
        const url = '../api/refactoringfiles/GetRefactoringFileEventList';
        return this.http.get(url);
    }
    getRefactoringSourceActionList(SourceActionId) {
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('RefactoringSourceActionId', SourceActionId);
        const url = '../api/refactoringfiles/GetRefactoringSourceActionList';
        return this.http.get(url, { params });
    }
    renameFileName(data) {
        const url = '../api/refactoringfiles/RenameFileName';
        var newData = {
            message: data.message,
            fromFileName: data.fromFileName,
            toFileName: data.toFileName,
            fullPath: data.fullPath,
            relativePath: data.relativePath,
        };
        return this.http.post(url, newData);
    }
}
MdRefactoringService.ɵfac = function MdRefactoringService_Factory(t) { return new (t || MdRefactoringService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"])); };
MdRefactoringService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: MdRefactoringService, factory: MdRefactoringService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "n8bz":
/*!*******************************************************************************!*\
  !*** ./src/app/md-explorer/components/main-content/main-content.component.ts ***!
  \*******************************************************************************/
/*! exports provided: MainContentComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MainContentComponent", function() { return MainContentComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/platform-browser */ "jhN1");
/* harmony import */ var _services_monitor_md_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/monitor-md.service */ "AaxG");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _pipes_safePipe__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../pipes/safePipe */ "8Cql");






const _c0 = ["myBelovedIframe"];
//import { ConnectionLostComponent } from '../../../signalR/dialogs/connection-lost/connection-lost.component';///dialogs/connection-lost/connection-lost.component
class MainContentComponent {
    constructor(service, sanitizer, monitorMDService, dialog) {
        this.service = service;
        this.sanitizer = sanitizer;
        this.monitorMDService = monitorMDService;
        this.dialog = dialog;
        this.htmlSource = '../welcome.html';
        this.helloWorld = (msg) => {
            alert('this is the callback');
        };
        this._HideIFrame = false;
        console.log("MainContentComponent constructor");
        this.monitorMDService.addMarkdownFileListener(this.markdownFileIsChanged, this);
        //this.monitorMDService.addOnCloseEvent(this.ShowConnectionLost, this);
    }
    ngAfterViewInit() {
        this.el.nativeElement.onload = _ => {
            try {
                _.target.contentWindow.document.myReferenceObject = this;
                _.target.contentWindow.document.addEventListener('wheel', this.toolbarHandler);
            }
            catch (e) { // for some reason the wheel event "injection" failed, so in ordet to Not hide tolbar i set block
                this.service.setWhatDisplayForToolbar('showToolbar');
            }
        };
    }
    toolbarHandler(event) {
        event.currentTarget.myReferenceObject.lastCall(event);
    }
    lastCall(event) {
        if (event.deltaY < 0) {
            // visualizzare
            this.service.setWhatDisplayForToolbar('showToolbar');
        }
        else {
            // nascondere
            this.service.setWhatDisplayForToolbar('hideToolbar');
        }
    }
    ngOnInit() {
        this.service.selectedMdFileFromSideNav.subscribe(_ => {
            this.callMdExplorerController(_);
        });
        this.service.selectedMdFileFromToolbar.subscribe(_ => {
            let current = _[0];
            if (current != undefined) {
                this.callMdExplorerController(current);
            }
        });
        this.service.selectedDirectoryFromNewDirectory.subscribe(_ => {
        });
    }
    callMdExplorerController(node) {
        if (node != null && node.relativePath != undefined) {
            let dateTime = new Date().getTime() / 1000;
            //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
            this.htmlSource = '../api/mdexplorer' + node.relativePath + '?time=' + dateTime;
        }
    }
    markdownFileIsChanged(data, objectThis) {
        let dateTime = new Date();
        objectThis.service.navigationArray = [];
        objectThis.service.setSelectedMdFileFromServer(data);
        //this.el.nativeElement.contentWindow(null, 'http://127.0.0.1');
        objectThis.htmlSource = '../api/mdexplorer/' + data.path + '?time=' + dateTime;
    }
}
MainContentComponent.ɵfac = function MainContentComponent_Factory(t) { return new (t || MainContentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__["DomSanitizer"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_monitor_md_service__WEBPACK_IMPORTED_MODULE_3__["MonitorMDService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"])); };
MainContentComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: MainContentComponent, selectors: [["app-main-content"]], viewQuery: function MainContentComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵloadQuery"]()) && (ctx.el = _t.first);
    } }, decls: 3, vars: 4, consts: [["id", "mdIframe", "name", "mdIframe", 2, "width", "100%", "height", "100%", 3, "hidden", "src"], ["myBelovedIframe", ""]], template: function MainContentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](0, "iframe", 0, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](2, "safe");
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("hidden", ctx._HideIFrame)("src", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 2, ctx.htmlSource), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵsanitizeResourceUrl"]);
    } }, pipes: [_pipes_safePipe__WEBPACK_IMPORTED_MODULE_5__["SafePipe"]], styles: ["[_nghost-%COMP%] {\n  overflow-y: hidden;\n  overflow-x: hidden;\n  height: 100%;\n  border: 1px solid black;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbWFpbi1jb250ZW50LmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0Usa0JBQUE7RUFDQSxrQkFBQTtFQUNBLFlBQUE7RUFDQSx1QkFBQTtBQUNGIiwiZmlsZSI6Im1haW4tY29udGVudC5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIjpob3N0IHtcclxuICBvdmVyZmxvdy15OiBoaWRkZW47XHJcbiAgb3ZlcmZsb3cteDogaGlkZGVuO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBib3JkZXI6IDFweCBzb2xpZCBibGFjaztcclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "p8DV":
/*!*****************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/new-directory/new-directory.component.ts ***!
  \*****************************************************************************************/
/*! exports provided: NewDirectoryComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NewDirectoryComponent", function() { return NewDirectoryComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
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
    dismiss() {
        this.dialogRef.close();
    }
    save() {
        this.mdFileService.CreateNewDirectory(this.data.fullPath, this.directoryName, this.data.level)
            .subscribe(data => {
            debugger;
            this.mdFileService.addNewDirectory(data);
            this.mdFileService.setSelectedDirectoryFromNewDirectory(data[data.length - 1]);
        });
        this.dialogRef.close();
    }
}
NewDirectoryComponent.ɵfac = function NewDirectoryComponent_Factory(t) { return new (t || NewDirectoryComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
NewDirectoryComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: NewDirectoryComponent, selectors: [["app-new-directory"]], decls: 17, vars: 1, consts: [[1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "directoryName", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-button", "", "color", "primary", 3, "click"]], template: function NewDirectoryComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "Directory Name");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewDirectoryComponent_Template_input_ngModelChange_5_listener($event) { return ctx.directoryName = $event; })("keyup.enter", function NewDirectoryComponent_Template_input_keyup_enter_5_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_9_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewDirectoryComponent_Template_button_click_13_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.directoryName);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJuZXctZGlyZWN0b3J5LmNvbXBvbmVudC5zY3NzIn0= */"] });


/***/ }),

/***/ "qzEU":
/*!*********************************************************************************************!*\
  !*** ./src/app/md-explorer/components/git-changes/select-branch/select-branch.component.ts ***!
  \*********************************************************************************************/
/*! exports provided: SelectBranchComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SelectBranchComponent", function() { return SelectBranchComponent; });
/* harmony import */ var _option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./option-branch/option-branch.component */ "QGRl");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");




const _c0 = ["gitChanges"];
const _c1 = ["*"];
class SelectBranchComponent {
    constructor() {
        this.SelectChildrensAreVisible = true;
        this.selectPosition = { x: 0, y: 0, visibility: 'hidden' };
        this.matIconSpecial = 'mat-icon-special';
    }
    doSearch(searched) {
        this.optionList.forEach(_ => {
            _.searchText = searched;
        });
    }
    ngOnInit() {
    }
    setSelectPosition() {
        let test = this.elGitChanges;
        if (this.SelectChildrensAreVisible) {
            this.selectPosition.visibility = 'visible';
            this.SelectChildrensAreVisible = false;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
        else {
            this.selectPosition.visibility = 'hidden';
            this.SelectChildrensAreVisible = true;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
    }
}
SelectBranchComponent.ɵfac = function SelectBranchComponent_Factory(t) { return new (t || SelectBranchComponent)(); };
SelectBranchComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: SelectBranchComponent, selectors: [["app-select-branch"]], contentQueries: function SelectBranchComponent_ContentQueries(rf, ctx, dirIndex) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵcontentQuery"](dirIndex, _option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_0__["OptionBranchComponent"], 0);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵloadQuery"]()) && (ctx.optionList = _t);
    } }, viewQuery: function SelectBranchComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵloadQuery"]()) && (ctx.elGitChanges = _t.first);
    } }, ngContentSelectors: _c1, decls: 13, vars: 6, consts: [[2, "width", "100%", 3, "click"], ["gitChanges", ""], [2, "white-space", "nowrap", "width", "100%"], ["value", "selected item", "readonly", "", 2, "width", "calc(100% - 32px)", "cursor", "pointer", "border", "none"], [2, "display", "inline", "position", "relative", "top", "5px", "cursor", "pointer"], [2, "visibility", "hidden", "width", "calc(100% - 10px)", "border", "solid", "border-width", "1px"], ["placeholder", "Filter branches", 2, "width", "calc(100% - 10px)"], ["searchedBranch", ""], ["value", "search", 3, "click"]], template: function SelectBranchComponent_Template(rf, ctx) { if (rf & 1) {
        const _r2 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵprojectionDef"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function SelectBranchComponent_Template_div_click_0_listener() { return ctx.setSelectPosition(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Current branch");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](5, "input", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-icon", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "keyboard_arrow_down");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](9, "input", 6, 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function SelectBranchComponent_Template_button_click_11_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r2); const _r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵreference"](10); return ctx.doSearch(_r1.value); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵprojection"](12);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵstyleProp"]("visibility", ctx.selectPosition.visibility)("left", ctx.selectPosition.x, "px")("top", ctx.selectPosition.y, "px");
    } }, directives: [_angular_material_form_field__WEBPACK_IMPORTED_MODULE_2__["MatLabel"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__["MatIcon"]], styles: [".ng-tns-c121-2[_ngcontent-%COMP%] {\n  padding-right: 1em;\n}\n\n.mat-form-field.specialForm[_ngcontent-%COMP%] {\n  padding-right: 0px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHNlbGVjdC1icmFuY2guY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxrQkFBQTtBQUNGOztBQUVBO0VBQ0Usa0JBQUE7QUFDRiIsImZpbGUiOiJzZWxlY3QtYnJhbmNoLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm5nLXRucy1jMTIxLTIge1xyXG4gIHBhZGRpbmctcmlnaHQ6IDFlbTtcclxufVxyXG5cclxuLm1hdC1mb3JtLWZpZWxkLnNwZWNpYWxGb3JtIHtcclxuICBwYWRkaW5nLXJpZ2h0OiAwcHg7XHJcbn1cclxuIl19 */"] });


/***/ }),

/***/ "r8p0":
/*!*************************************************************************************************!*\
  !*** ./src/app/md-explorer/components/git-changes/select-extended/select-extended.component.ts ***!
  \*************************************************************************************************/
/*! exports provided: SelectExtendedComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SelectExtendedComponent", function() { return SelectExtendedComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");




const _c0 = ["gitChanges"];
function SelectExtendedComponent_ng_template_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵprojection"](0, 0, ["#myTemplate", ""]);
} }
const _c1 = ["*"];
class SelectExtendedComponent {
    constructor(elRef) {
        //@ViewChild(TemplateRef) templateObject: TemplateRef<ContentRef>;
        this.selectPosition = { x: 0, y: 0, visibility: 'hidden' };
        this.matIconSpecial = 'mat-icon-special';
        this.SelectChildrensAreVisible = true;
        this.selected = 'volvo';
    }
    ngOnInit() {
        //this.templateObject.createEmbeddedView();
    }
    setClassOver() {
        this.matIconSpecial = 'mat-icon-special-hover';
    }
    setClassSimple() {
        this.matIconSpecial = 'mat-icon-special';
    }
    setSelectPosition() {
        debugger;
        let test = this.elGitChanges;
        if (this.SelectChildrensAreVisible) {
            this.selectPosition.visibility = 'visible';
            this.SelectChildrensAreVisible = false;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
        else {
            this.selectPosition.visibility = 'hidden';
            this.SelectChildrensAreVisible = true;
            this.selectPosition.x = this.elGitChanges._elementRef.nativeElement.offsetLeft;
            this.selectPosition.y = this.elGitChanges._elementRef.nativeElement.offsetTop +
                this.elGitChanges._elementRef.nativeElement.offsetHeight + 44;
        }
    }
}
SelectExtendedComponent.ɵfac = function SelectExtendedComponent_Factory(t) { return new (t || SelectExtendedComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"])); };
SelectExtendedComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SelectExtendedComponent, selectors: [["app-select-extended"]], viewQuery: function SelectExtendedComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵloadQuery"]()) && (ctx.elGitChanges = _t.first);
    } }, ngContentSelectors: _c1, decls: 11, vars: 6, consts: [["appearance", "fill", 3, "click"], ["gitChanges", ""], [2, "white-space", "nowrap"], ["matInput", "", "value", "selected item", "readonly", ""], [2, "display", "inline"], [2, "visibility", "hidden", "position", "fixed", "border", "solid", "border-width", "1px", "min-width", "200px"], ["matInput", "", "placeholder", "Filter branches"], ["app-select-extended", ""]], template: function SelectExtendedComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵprojectionDef"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-form-field", 0, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SelectExtendedComponent_Template_mat_form_field_click_0_listener() { return ctx.setSelectPosition(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "Special dropdown list");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](5, "input", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "mat-icon", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7, "keyboard_arrow_down");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](9, "input", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](10, SelectExtendedComponent_ng_template_10_Template, 1, 0, "ng-template", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵstyleProp"]("visibility", ctx.selectPosition.visibility)("left", ctx.selectPosition.x, "px")("top", ctx.selectPosition.y, "px");
    } }, directives: [_angular_material_form_field__WEBPACK_IMPORTED_MODULE_1__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_1__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_2__["MatInput"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_3__["MatIcon"]], styles: [".ng-tns-c121-2[_ngcontent-%COMP%] {\n  padding-right: 1em;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHNlbGVjdC1leHRlbmRlZC5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGtCQUFBO0FBQ0YiLCJmaWxlIjoic2VsZWN0LWV4dGVuZGVkLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm5nLXRucy1jMTIxLTIge1xyXG4gIHBhZGRpbmctcmlnaHQ6IDFlbTtcclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "s8RO":
/*!*********************************************************************!*\
  !*** ./src/app/md-explorer/components/md-tree/md-tree.component.ts ***!
  \*********************************************************************/
/*! exports provided: MdTreeComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdTreeComponent", function() { return MdTreeComponent; });
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/cdk/tree */ "FvrZ");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/tree */ "8yBR");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");
/* harmony import */ var _dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../dialogs/new-directory/new-directory.component */ "p8DV");
/* harmony import */ var _dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../dialogs/new-markdown/new-markdown.component */ "WxmV");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/common */ "ofXK");














function MdTreeComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    const _r6 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "button", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r6); const item_r4 = ctx.item; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r5.createDirectoryOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "b", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](2, "New");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](3, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](4, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](6, "b", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](7, " dir");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](8, "button", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_Template_button_click_8_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r6); const item_r4 = ctx.item; const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r7.createMdOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](9, "b", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](10, "New");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](11, " Markdown on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](12, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](14, "b", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](15, " dir");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const item_r4 = ctx.item;
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate"](item_r4 == null ? "root" : item_r4.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate"](item_r4 == null ? "root" : item_r4.name);
} }
function MdTreeComponent_mat_tree_node_5_Template(rf, ctx) { if (rf & 1) {
    const _r10 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "mat-tree-node", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](2, "mat-icon", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](4, "a", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("click", function MdTreeComponent_mat_tree_node_5_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r10); const node_r8 = ctx.$implicit; const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r9.getNode(node_r8); });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r8 = ctx.$implicit;
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate1"](" ", "text_snippet", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("ngClass", ctx_r2.activeNode === node_r8 ? "background-highlight" : "blue-link");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate"](node_r8.name);
} }
function MdTreeComponent_mat_tree_node_6_Template(rf, ctx) { if (rf & 1) {
    const _r13 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](0, "mat-tree-node", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "button", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](2, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_6_Template_mat_icon_contextmenu_2_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵrestoreView"](_r13); const node_r11 = ctx.$implicit; const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"](); return ctx_r12.onRightClick($event, node_r11); });
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r11 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵattribute"]("aria-label", "Toggle " + node_r11.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate1"](" ", ctx_r3.treeControl.isExpanded(node_r11) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtextInterpolate1"](" ", node_r11.name, " ");
} }
const TREE_DATA = [];
class MdTreeComponent {
    constructor(mdFileService, dialog) {
        this.mdFileService = mdFileService;
        this.dialog = dialog;
        this.hooked = false;
        this.menuTopLeftPosition = { x: 0, y: 0 };
        this._transformer = (node, level) => {
            return {
                expandable: (!!node.childrens && node.childrens.length > 0) || node.type == "folder",
                name: node.name,
                level: level,
                path: node.path,
                relativePath: node.path,
                fullPath: node.fullPath,
                type: node.type,
            };
        };
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__["FlatTreeControl"](node => node.level, node => node.expandable);
        this.treeFlattener = new _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeFlattener"](this._transformer, node => node.level, node => node.expandable, node => node.childrens);
        this.dataSource = new _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeFlatDataSource"](this.treeControl, this.treeFlattener);
        this.hasChild = (_, node) => node.expandable;
        this.isFolder = (_, node) => node.type == "folder";
        this.dataSource.data = TREE_DATA;
        this.mdFileService.serverSelectedMdFile.subscribe(_ => {
            const myClonedArray = [];
            _.forEach(val => myClonedArray.push(Object.assign({}, val)));
            while (myClonedArray.length > 1) {
                var toExpand = myClonedArray.pop();
                var test = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
                this.treeControl.expand(test);
            }
            if (myClonedArray.length > 0) {
                var toExpand = myClonedArray.pop();
                this.activeNode = this.treeControl.dataNodes.find(_ => _.path == toExpand.path);
            }
        });
    }
    ngOnInit() {
        this.mdFiles = this.mdFileService.mdFiles;
        this.mdFileService.mdFiles.subscribe(data => {
            this.dataSource.data = data;
        });
        this.mdFileService.loadAll(this.deferredOpenProject, this);
    }
    deferredOpenProject(data, objectThis) {
    }
    onRightClick(event, item) {
        // preventDefault avoids to show the visualization of the right-click menu of the browser
        event.preventDefault();
        // we record the mouse position in our object
        this.menuTopLeftPosition.x = event.clientX;
        this.menuTopLeftPosition.y = event.clientY;
        // we open the menu
        // we pass to the menu the information about our object
        this.matMenuTrigger.menuData = { item: item };
        // we open the menu
        this.matMenuTrigger.openMenu();
    }
    getNode(node) {
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.activeNode = node;
    }
    // Manu management
    createMdOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_5__["NewMarkdownComponent"], {
            width: '300px',
            data: node,
        });
    }
    createDirectoryOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_4__["NewDirectoryComponent"], {
            width: '300px',
            data: node,
        });
    }
}
MdTreeComponent.ɵfac = function MdTreeComponent_Factory(t) { return new (t || MdTreeComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_8__["MatDialog"])); };
MdTreeComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵdefineComponent"]({ type: MdTreeComponent, selectors: [["app-md-tree"]], viewQuery: function MdTreeComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 7, vars: 8, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [3, "dataSource", "treeControl", "contextmenu"], ["matTreeNodePadding", "", 4, "matTreeNodeDef"], ["matTreeNodePadding", "", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["matTreeNodePadding", ""], ["mat-icon-button", "", "disabled", ""], [1, "mat-icon-rtl-mirror"], [2, "cursor", "pointer", 3, "ngClass", "click"], ["mat-icon-button", "", "matTreeNodeToggle", ""], [1, "gold-icon", "mat-icon-rtl-mirror", 3, "contextmenu"]], template: function MdTreeComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](3, MdTreeComponent_ng_template_3_Template, 16, 2, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementStart"](4, "mat-tree", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵlistener"]("contextmenu", function MdTreeComponent_Template_mat_tree_contextmenu_4_listener($event) { return ctx.onRightClick($event, null); });
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](5, MdTreeComponent_mat_tree_node_5_Template, 6, 3, "mat-tree-node", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵtemplate"](6, MdTreeComponent_mat_tree_node_6_Template, 5, 3, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_6__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isFolder);
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuContent"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeDef"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuItem"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodePadding"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_11__["NgClass"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeToggle"]], styles: [".mat-tree-node[_ngcontent-%COMP%] {\n  min-height: 40px !important;\n}\n\n.blue-link[_ngcontent-%COMP%] {\n  color: blue;\n  border-bottom-width: 1px;\n  border-bottom: dashed;\n  border-bottom-color: grey;\n}\n\n.background-highlight[_ngcontent-%COMP%] {\n  background-color: blue;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbWQtdHJlZS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLDJCQUFBO0FBQ0Y7O0FBQ0E7RUFDRSxXQUFBO0VBQ0Esd0JBQUE7RUFDQSxxQkFBQTtFQUNBLHlCQUFBO0FBRUY7O0FBQ0E7RUFDRSxzQkFBQTtFQUNBLFlBQUE7QUFFRiIsImZpbGUiOiJtZC10cmVlLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm1hdC10cmVlLW5vZGUge1xyXG4gIG1pbi1oZWlnaHQ6IDQwcHggIWltcG9ydGFudDtcclxufVxyXG4uYmx1ZS1saW5rIHtcclxuICBjb2xvcjogYmx1ZTtcclxuICBib3JkZXItYm90dG9tLXdpZHRoOiAxcHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogZGFzaGVkO1xyXG4gIGJvcmRlci1ib3R0b20tY29sb3I6IGdyZXk7XHJcbn1cclxuXHJcbi5iYWNrZ3JvdW5kLWhpZ2hsaWdodCB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogYmx1ZTtcclxuICBjb2xvcjogd2hpdGU7XHJcbn1cclxuIl19 */"] });


/***/ })

}]);
//# sourceMappingURL=md-explorer-md-explorer-module.js.map