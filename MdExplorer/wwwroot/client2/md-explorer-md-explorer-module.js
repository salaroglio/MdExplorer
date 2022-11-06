(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["md-explorer-md-explorer-module"],{

/***/ "+59A":
/*!**********************************************************!*\
  !*** ./src/app/signalR/dialogs/rules/rules.component.ts ***!
  \**********************************************************/
/*! exports provided: RulesComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "RulesComponent", function() { return RulesComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../md-explorer/models/md-file */ "aS6m");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../md-explorer/services/md-refactoring.service */ "n7Vp");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ "xmhS");
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
            var oldFile = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_1__["MdFile"](data.oldName, oldPath, data.oldLevel, false);
            oldFile.relativePath = oldPath;
            oldFile.fullPath = data.oldPath;
            var newFile = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_1__["MdFile"](data.newName, newPath, data.newLevel, false);
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
RulesComponent.ɵfac = function RulesComponent_Factory(t) { return new (t || RulesComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_md_explorer_services_md_refactoring_service__WEBPACK_IMPORTED_MODULE_3__["MdRefactoringService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__["MdFileService"])); };
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
/* harmony import */ var _signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../signalR/dialogs/rules/rules.component */ "+59A");
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
/* harmony import */ var _components_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! ./components/dialogs/change-directory/change-directory.component */ "HotF");
/* harmony import */ var _components_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! ./components/dialogs/delete-markdown/delete-markdown.component */ "BUpo");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/core */ "fXoL");




























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
MdExplorerModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_26__["ɵɵdefineNgModule"]({ type: MdExplorerModule });
MdExplorerModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_26__["ɵɵdefineInjector"]({ providers: [
        _services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"],
    ], imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_4__["MaterialModule"],
            _angular_common_http__WEBPACK_IMPORTED_MODULE_9__["HttpClientModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_5__["FormsModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_7__["RouterModule"].forChild(routes)
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_26__["ɵɵsetNgModuleScope"](MdExplorerModule, { declarations: [_components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"],
        _components_toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_2__["ToolbarComponent"],
        _pipes_safePipe__WEBPACK_IMPORTED_MODULE_10__["SafePipe"],
        _components_main_content_main_content_component__WEBPACK_IMPORTED_MODULE_3__["MainContentComponent"],
        _md_explorer_component__WEBPACK_IMPORTED_MODULE_6__["MdExplorerComponent"],
        _components_dialogs_settings_settings_component__WEBPACK_IMPORTED_MODULE_11__["SettingsComponent"],
        _components_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_12__["RenameFileComponent"],
        _signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_13__["RulesComponent"],
        _components_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_14__["NewMarkdownComponent"],
        _components_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_15__["NewDirectoryComponent"],
        _components_md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_16__["MdTreeComponent"],
        _components_git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_17__["GitChangesComponent"],
        _components_git_changes_select_extended_select_extended_component__WEBPACK_IMPORTED_MODULE_18__["SelectExtendedComponent"],
        _components_git_changes_select_extended_option_extended_option_extended_component__WEBPACK_IMPORTED_MODULE_19__["OptionExtendedComponent"],
        _components_git_changes_select_branch_select_branch_component__WEBPACK_IMPORTED_MODULE_20__["SelectBranchComponent"],
        _components_git_changes_select_branch_option_branch_option_branch_component__WEBPACK_IMPORTED_MODULE_21__["OptionBranchComponent"],
        _signalR_dialogs_connection_lost_connection_lost_component__WEBPACK_IMPORTED_MODULE_22__["ConnectionLostComponent"],
        _signalR_dialogs_parsing_project_parsing_project_component__WEBPACK_IMPORTED_MODULE_23__["ParsingProjectComponent"],
        _components_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_24__["ChangeDirectoryComponent"],
        _components_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_25__["DeleteMarkdownComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
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
                this.plantumlServer = settings.filter(_ => _.name === "EditorPath")[0].valueString || null;
                this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
            }
        });
    }
    save() {
        this._settings.filter(_ => _.name === "EditorPath")[0].valueString = this.plantumlServer;
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
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6, "Visual studio code server path");
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

/***/ "BUpo":
/*!*********************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/delete-markdown/delete-markdown.component.ts ***!
  \*********************************************************************************************/
/*! exports provided: DeleteMarkdownComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DeleteMarkdownComponent", function() { return DeleteMarkdownComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/button */ "bTqV");






class DeleteMarkdownComponent {
    constructor(dialogRef, mdFileService, data) {
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
        this.data = data;
    }
    ngOnInit() {
    }
    dismiss() {
        this.dialogRef.close();
    }
    delete() {
        this.mdFileService.deleteFile(this.data);
        this.dialogRef.close();
    }
}
DeleteMarkdownComponent.ɵfac = function DeleteMarkdownComponent_Factory(t) { return new (t || DeleteMarkdownComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"])); };
DeleteMarkdownComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: DeleteMarkdownComponent, selectors: [["app-delete-markdown"]], decls: 9, vars: 0, consts: [["mat-dialog-title", ""], ["mat-button", "", "color", "warn", 3, "click"], ["mat-button", "", "color", "primary", 3, "click"]], template: function DeleteMarkdownComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Delete markdown\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, " There are not depencies\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function DeleteMarkdownComponent_Template_button_click_5_listener() { return ctx.delete(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, " Yes ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "button", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function DeleteMarkdownComponent_Template_button_click_7_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, " Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_4__["MatButton"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJkZWxldGUtbWFya2Rvd24uY29tcG9uZW50LnNjc3MifQ== */"] });


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

/***/ "HotF":
/*!***********************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/change-directory/change-directory.component.ts ***!
  \***********************************************************************************************/
/*! exports provided: ChangeDirectoryComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ChangeDirectoryComponent", function() { return ChangeDirectoryComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");










class ChangeDirectoryComponent {
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
        this.mdFileService.RenameDirectory(this.data.fullPath, this.directoryName, this.data.level)
            .subscribe(data => {
            debugger;
            this.mdFileService.addNewDirectory(data);
            this.mdFileService.setSelectedDirectoryFromNewDirectory(data[data.length - 1]);
        });
        this.dialogRef.close();
    }
}
ChangeDirectoryComponent.ɵfac = function ChangeDirectoryComponent_Factory(t) { return new (t || ChangeDirectoryComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
ChangeDirectoryComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ChangeDirectoryComponent, selectors: [["app-change-directory"]], decls: 17, vars: 1, consts: [[1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "directoryName", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-button", "", "color", "primary", 3, "click"]], template: function ChangeDirectoryComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "Directory Name");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function ChangeDirectoryComponent_Template_input_ngModelChange_5_listener($event) { return ctx.directoryName = $event; })("keyup.enter", function ChangeDirectoryComponent_Template_input_keyup_enter_5_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "Hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ChangeDirectoryComponent_Template_button_click_9_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ChangeDirectoryComponent_Template_button_click_13_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.directoryName);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJjaGFuZ2UtZGlyZWN0b3J5LmNvbXBvbmVudC5zY3NzIn0= */"] });


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
/* harmony import */ var _signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../signalR/dialogs/rules/rules.component */ "+59A");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../signalr/services/server-messages.service */ "jQFx");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ "ofXK");

















function ToolbarComponent_span_11_span_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, ">>");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} }
const _c0 = function () { return { "cursor": "pointer", "color": "orange" }; };
const _c1 = function () { return {}; };
function ToolbarComponent_span_11_Template(rf, ctx) { if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, ToolbarComponent_span_11_span_1_Template, 2, 0, "span", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "a", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_span_11_Template_a_click_2_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5); const doc_r2 = ctx.$implicit; const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](); return ctx_r4.mdFileService.navigationArray.indexOf(doc_r2) < ctx_r4.mdFileService.navigationArray.length - 1 ? ctx_r4.backToDocument(doc_r2) : ctx_r4.return; });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const doc_r2 = ctx.$implicit;
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx_r0.mdFileService.navigationArray.indexOf(doc_r2) > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngStyle", ctx_r0.mdFileService.navigationArray.indexOf(doc_r2) < ctx_r0.mdFileService.navigationArray.length - 1 ? _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction0"](3, _c0) : _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction0"](4, _c1));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](doc_r2.name);
} }
function ToolbarComponent_span_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](1, "img", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"]("[", ctx_r1.currentBranch, "]");
} }
class ToolbarComponent {
    constructor(dialog, monitorMDService, http, _snackBar, router, mdFileService, gitservice) {
        this.dialog = dialog;
        this.monitorMDService = monitorMDService;
        this.http = http;
        this._snackBar = _snackBar;
        this.router = router;
        this.mdFileService = mdFileService;
        this.gitservice = gitservice;
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
            width: '600px',
        });
        dialogRef.afterClosed().subscribe(result => {
            console.log('The dialog was closed');
        });
    }
    openRules(data) {
        const dialogRef = this.dialog.open(_signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_3__["RulesComponent"], {
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
        this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
        this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this); //TODO: da spostare in SignalR
        this.gitservice.getCurrentBranch().subscribe(branch => {
            this.currentBranch = branch.name;
        });
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
ToolbarComponent.ɵfac = function ToolbarComponent_Factory(t) { return new (t || ToolbarComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__["ServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_6__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_7__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_8__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_10__["GITService"])); };
ToolbarComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: ToolbarComponent, selectors: [["app-toolbar"]], outputs: { toggleSidenav: "toggleSidenav" }, decls: 17, vars: 2, consts: [["color", "primary"], ["mat-icon-button", "", "matTooltip", "hide/show menu", 3, "click"], ["mat-icon-button", "", "matTooltip", "edit document", 3, "click"], ["mat-icon-button", "", "matTooltip", "export in word", 3, "click"], [4, "ngFor", "ngForOf"], [1, "flexExpand"], [4, "ngIf"], ["mat-icon-button", "", "matTooltip", "global settings", 3, "click"], [3, "ngStyle", "click"], ["src", "/assets/gitlogo.png"]], template: function ToolbarComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-toolbar", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "mat-toolbar-row");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_2_listener() { return ctx.toggleSidenav.emit(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](4, "vertical_split");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "button", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_5_listener() { return ctx.OpenEditor(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7, "edit_note");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_8_listener() { return ctx.Export(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10, "directions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](11, ToolbarComponent_span_11_Template, 4, 5, "span", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](12, "span", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](13, ToolbarComponent_span_13_Template, 4, 1, "span", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "button", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_14_listener() { return ctx.openDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](16, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](11);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx.mdFileService.navigationArray);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.currentBranch != null);
    } }, directives: [_angular_material_toolbar__WEBPACK_IMPORTED_MODULE_11__["MatToolbar"], _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_11__["MatToolbarRow"], _angular_material_button__WEBPACK_IMPORTED_MODULE_12__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_13__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgForOf"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgIf"], _angular_common__WEBPACK_IMPORTED_MODULE_15__["NgStyle"]], styles: [".sidenav-toggle[_ngcontent-%COMP%] {\n  display: none;\n  padding: 0;\n  margin: 8px;\n  min-width: after 56px;\n}\n@media (max-width: 720px) {\n  .sidenav-toggle[_ngcontent-%COMP%] {\n    display: flex;\n    align: center;\n    justify-content: center;\n  }\n}\n.sidenav-toggle[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 30px;\n  height: after 56px;\n  width: after 56px;\n  line-height: after 56px;\n  color: white;\n}\n.flexExpand[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n}\n#projectcss[_ngcontent-%COMP%]    .mat-tooltip {\n  \n  \n  color: white;\n  background-color: blue;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcdG9vbGJhci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFHQTtFQUNFLGFBQUE7RUFDQSxVQUFBO0VBQ0EsV0FBQTtFQUNBLHFCQVBVO0FBS1o7QUFJRTtFQU5GO0lBT0ksYUFBQTtJQUNBLGFBQUE7SUFDQSx1QkFBQTtFQURGO0FBQ0Y7QUFHRTtFQUNFLGVBQUE7RUFDQSxrQkFqQlE7RUFrQlIsaUJBbEJRO0VBbUJSLHVCQW5CUTtFQW9CUixZQUFBO0FBREo7QUFNQTtFQUNFLGNBQUE7QUFIRjtBQVFFO0VBQ0UsZ0NBQUE7RUFDQSxTQUFBO0VBQ0EsWUFBQTtFQUNBLHNCQUFBO0FBTEoiLCJmaWxlIjoidG9vbGJhci5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIiRpY29uV2lkdGg6IGFmdGVyIDU2cHg7XHJcblxyXG5cclxuLnNpZGVuYXYtdG9nZ2xlIHtcclxuICBkaXNwbGF5OiBub25lO1xyXG4gIHBhZGRpbmc6IDA7XHJcbiAgbWFyZ2luOiA4cHg7XHJcbiAgbWluLXdpZHRoOiAkaWNvbldpZHRoO1xyXG5cclxuICBAbWVkaWEgKG1heC13aWR0aDo3MjBweCkge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICB9XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMzBweDtcclxuICAgIGhlaWdodDogJGljb25XaWR0aDtcclxuICAgIHdpZHRoOiAkaWNvbldpZHRoO1xyXG4gICAgbGluZS1oZWlnaHQ6ICRpY29uV2lkdGg7XHJcbiAgICBjb2xvcjogd2hpdGU7XHJcbiAgfVxyXG59XHJcblxyXG5cclxuLmZsZXhFeHBhbmQge1xyXG4gIGZsZXg6IDEgMSBhdXRvO1xyXG59XHJcblxyXG5cclxuI3Byb2plY3Rjc3Mge1xyXG4gIDo6bmctZGVlcC5tYXQtdG9vbHRpcCB7XHJcbiAgICAvKiB5b3VyIG93biBjdXN0b20gc3R5bGVzIGhlcmUgKi9cclxuICAgIC8qIGUuZy4gKi9cclxuICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgIGJhY2tncm91bmQtY29sb3I6IGJsdWU7XHJcbiAgfVxyXG59XHJcbiJdfQ== */"] });


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

/***/ "N73s":
/*!****************************************************!*\
  !*** ./src/app/git/services/gitservice.service.ts ***!
  \****************************************************/
/*! exports provided: GITService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GITService", function() { return GITService; });
/* harmony import */ var _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../models/gitlab-setting */ "MVql");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common/http */ "tk/3");



class GITService {
    constructor(http) {
        this.http = http;
    }
    getCurrentBranch() {
        const url = '../api/gitservice/branches/feat/getcurrentbranch';
        return this.http.get(url);
    }
    storeGitlabSettings(user, password, gitlabLink) {
        const url = '../api/gitservice/gitlabsettings';
        let setting = new _models_gitlab_setting__WEBPACK_IMPORTED_MODULE_0__["GitlabSetting"]();
        return this.http.post(url, setting);
    }
    getGitlabSettings() {
        const url = '../api/gitservice/gitlabsettings';
        return this.http.get(url);
    }
}
GITService.ɵfac = function GITService_Factory(t) { return new (t || GITService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_2__["HttpClient"])); };
GITService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: GITService, factory: GITService.ɵfac, providedIn: 'root' });


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

/***/ "UXJo":
/*!**********************************************************************!*\
  !*** ./node_modules/@angular/cdk/__ivy_ngcc__/fesm2015/clipboard.js ***!
  \**********************************************************************/
/*! exports provided: CDK_COPY_TO_CLIPBOARD_CONFIG, CKD_COPY_TO_CLIPBOARD_CONFIG, CdkCopyToClipboard, Clipboard, ClipboardModule, PendingCopy */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CDK_COPY_TO_CLIPBOARD_CONFIG", function() { return CDK_COPY_TO_CLIPBOARD_CONFIG; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CKD_COPY_TO_CLIPBOARD_CONFIG", function() { return CKD_COPY_TO_CLIPBOARD_CONFIG; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CdkCopyToClipboard", function() { return CdkCopyToClipboard; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "Clipboard", function() { return Clipboard; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ClipboardModule", function() { return ClipboardModule; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "PendingCopy", function() { return PendingCopy; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");



/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
/**
 * A pending copy-to-clipboard operation.
 *
 * The implementation of copying text to the clipboard modifies the DOM and
 * forces a relayout. This relayout can take too long if the string is large,
 * causing the execCommand('copy') to happen too long after the user clicked.
 * This results in the browser refusing to copy. This object lets the
 * relayout happen in a separate tick from copying by providing a copy function
 * that can be called later.
 *
 * Destroy must be called when no longer in use, regardless of whether `copy` is
 * called.
 */

class PendingCopy {
    constructor(text, _document) {
        this._document = _document;
        const textarea = this._textarea = this._document.createElement('textarea');
        const styles = textarea.style;
        // Hide the element for display and accessibility. Set a fixed position so the page layout
        // isn't affected. We use `fixed` with `top: 0`, because focus is moved into the textarea
        // for a split second and if it's off-screen, some browsers will attempt to scroll it into view.
        styles.position = 'fixed';
        styles.top = styles.opacity = '0';
        styles.left = '-999em';
        textarea.setAttribute('aria-hidden', 'true');
        textarea.value = text;
        this._document.body.appendChild(textarea);
    }
    /** Finishes copying the text. */
    copy() {
        const textarea = this._textarea;
        let successful = false;
        try { // Older browsers could throw if copy is not supported.
            if (textarea) {
                const currentFocus = this._document.activeElement;
                textarea.select();
                textarea.setSelectionRange(0, textarea.value.length);
                successful = this._document.execCommand('copy');
                if (currentFocus) {
                    currentFocus.focus();
                }
            }
        }
        catch (_a) {
            // Discard error.
            // Initial setting of {@code successful} will represent failure here.
        }
        return successful;
    }
    /** Cleans up DOM changes used to perform the copy operation. */
    destroy() {
        const textarea = this._textarea;
        if (textarea) {
            if (textarea.parentNode) {
                textarea.parentNode.removeChild(textarea);
            }
            this._textarea = undefined;
        }
    }
}

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
/**
 * A service for copying text to the clipboard.
 */
class Clipboard {
    constructor(document) {
        this._document = document;
    }
    /**
     * Copies the provided text into the user's clipboard.
     *
     * @param text The string to copy.
     * @returns Whether the operation was successful.
     */
    copy(text) {
        const pendingCopy = this.beginCopy(text);
        const successful = pendingCopy.copy();
        pendingCopy.destroy();
        return successful;
    }
    /**
     * Prepares a string to be copied later. This is useful for large strings
     * which take too long to successfully render and be copied in the same tick.
     *
     * The caller must call `destroy` on the returned `PendingCopy`.
     *
     * @param text The string to copy.
     * @returns the pending copy operation.
     */
    beginCopy(text) {
        return new PendingCopy(text, this._document);
    }
}
Clipboard.ɵfac = function Clipboard_Factory(t) { return new (t || Clipboard)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common__WEBPACK_IMPORTED_MODULE_0__["DOCUMENT"])); };
Clipboard.ɵprov = Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"])({ factory: function Clipboard_Factory() { return new Clipboard(Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"])(_angular_common__WEBPACK_IMPORTED_MODULE_0__["DOCUMENT"])); }, token: Clipboard, providedIn: "root" });
Clipboard.ctorParameters = () => [
    { type: undefined, decorators: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Inject"], args: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["DOCUMENT"],] }] }
];
(function () { (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵsetClassMetadata"](Clipboard, [{
        type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Injectable"],
        args: [{ providedIn: 'root' }]
    }], function () { return [{ type: undefined, decorators: [{
                type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Inject"],
                args: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["DOCUMENT"]]
            }] }]; }, null); })();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
/** Injection token that can be used to provide the default options to `CdkCopyToClipboard`. */
const CDK_COPY_TO_CLIPBOARD_CONFIG = new _angular_core__WEBPACK_IMPORTED_MODULE_1__["InjectionToken"]('CDK_COPY_TO_CLIPBOARD_CONFIG');
/**
 * @deprecated Use `CDK_COPY_TO_CLIPBOARD_CONFIG` instead.
 * @breaking-change 13.0.0
 */
const CKD_COPY_TO_CLIPBOARD_CONFIG = CDK_COPY_TO_CLIPBOARD_CONFIG;
/**
 * Provides behavior for a button that when clicked copies content into user's
 * clipboard.
 */
class CdkCopyToClipboard {
    constructor(_clipboard, _ngZone, config) {
        this._clipboard = _clipboard;
        this._ngZone = _ngZone;
        /** Content to be copied. */
        this.text = '';
        /**
         * How many times to attempt to copy the text. This may be necessary for longer text, because
         * the browser needs time to fill an intermediate textarea element and copy the content.
         */
        this.attempts = 1;
        /**
         * Emits when some text is copied to the clipboard. The
         * emitted value indicates whether copying was successful.
         */
        this.copied = new _angular_core__WEBPACK_IMPORTED_MODULE_1__["EventEmitter"]();
        /** Copies that are currently being attempted. */
        this._pending = new Set();
        if (config && config.attempts != null) {
            this.attempts = config.attempts;
        }
    }
    /** Copies the current text to the clipboard. */
    copy(attempts = this.attempts) {
        if (attempts > 1) {
            let remainingAttempts = attempts;
            const pending = this._clipboard.beginCopy(this.text);
            this._pending.add(pending);
            const attempt = () => {
                const successful = pending.copy();
                if (!successful && --remainingAttempts && !this._destroyed) {
                    // We use 1 for the timeout since it's more predictable when flushing in unit tests.
                    this._currentTimeout = this._ngZone.runOutsideAngular(() => setTimeout(attempt, 1));
                }
                else {
                    this._currentTimeout = null;
                    this._pending.delete(pending);
                    pending.destroy();
                    this.copied.emit(successful);
                }
            };
            attempt();
        }
        else {
            this.copied.emit(this._clipboard.copy(this.text));
        }
    }
    ngOnDestroy() {
        if (this._currentTimeout) {
            clearTimeout(this._currentTimeout);
        }
        this._pending.forEach(copy => copy.destroy());
        this._pending.clear();
        this._destroyed = true;
    }
}
CdkCopyToClipboard.ɵfac = function CdkCopyToClipboard_Factory(t) { return new (t || CdkCopyToClipboard)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](Clipboard), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["NgZone"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](CKD_COPY_TO_CLIPBOARD_CONFIG, 8)); };
CdkCopyToClipboard.ɵdir = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineDirective"]({ type: CdkCopyToClipboard, selectors: [["", "cdkCopyToClipboard", ""]], hostBindings: function CdkCopyToClipboard_HostBindings(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CdkCopyToClipboard_click_HostBindingHandler() { return ctx.copy(); });
    } }, inputs: { text: ["cdkCopyToClipboard", "text"], attempts: ["cdkCopyToClipboardAttempts", "attempts"] }, outputs: { copied: "cdkCopyToClipboardCopied" } });
CdkCopyToClipboard.ctorParameters = () => [
    { type: Clipboard },
    { type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["NgZone"] },
    { type: undefined, decorators: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Optional"] }, { type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Inject"], args: [CKD_COPY_TO_CLIPBOARD_CONFIG,] }] }
];
CdkCopyToClipboard.propDecorators = {
    text: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Input"], args: ['cdkCopyToClipboard',] }],
    attempts: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Input"], args: ['cdkCopyToClipboardAttempts',] }],
    copied: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Output"], args: ['cdkCopyToClipboardCopied',] }]
};
(function () { (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵsetClassMetadata"](CdkCopyToClipboard, [{
        type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Directive"],
        args: [{
                selector: '[cdkCopyToClipboard]',
                host: {
                    '(click)': 'copy()'
                }
            }]
    }], function () { return [{ type: Clipboard }, { type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["NgZone"] }, { type: undefined, decorators: [{
                type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Optional"]
            }, {
                type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Inject"],
                args: [CKD_COPY_TO_CLIPBOARD_CONFIG]
            }] }]; }, { text: [{
            type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Input"],
            args: ['cdkCopyToClipboard']
        }], attempts: [{
            type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Input"],
            args: ['cdkCopyToClipboardAttempts']
        }], copied: [{
            type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["Output"],
            args: ['cdkCopyToClipboardCopied']
        }] }); })();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
class ClipboardModule {
}
ClipboardModule.ɵfac = function ClipboardModule_Factory(t) { return new (t || ClipboardModule)(); };
ClipboardModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineNgModule"]({ type: ClipboardModule });
ClipboardModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjector"]({});
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵsetNgModuleScope"](ClipboardModule, { declarations: [CdkCopyToClipboard], exports: [CdkCopyToClipboard] }); })();
(function () { (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵsetClassMetadata"](ClipboardModule, [{
        type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["NgModule"],
        args: [{
                declarations: [CdkCopyToClipboard],
                exports: [CdkCopyToClipboard]
            }]
    }], null, null); })();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */

/**
 * Generated bundle index. Do not edit.
 */



//# sourceMappingURL=clipboard.js.map

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
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/radio */ "QibW");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");













function NewMarkdownComponent_mat_radio_button_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-radio-button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const plantumlTemplate_r2 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("value", plantumlTemplate_r2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", plantumlTemplate_r2.name, " ");
} }
function NewMarkdownComponent_mat_radio_button_19_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-radio-button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const slideTemplate_r3 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("value", slideTemplate_r3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", slideTemplate_r3.name, " ");
} }
class NewMarkdownComponent {
    constructor(data, dialogRef, mdFileService) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
        this.plantumlTemplates = [{ id: 0, name: 'Text document', documentType: 'document' },
            { id: 1, name: 'Sequence Diagram', documentType: 'document' },
            { id: 2, name: 'State Diagram', documentType: 'document' },
            { id: 3, name: 'Workflow', documentType: 'document' },
            { id: 4, name: 'Gantt current week', documentType: 'document' }];
        this.slideTemplates = [{
                id: 5, name: 'Flicker document', documentType: 'slides'
            }, {
                id: 6, name: 'Slide video', documentType: 'slides'
            }, {
                id: 7, name: 'Slide power point', documentType: 'slides'
            }];
        this.selectedTemplate = this.plantumlTemplates[0];
    }
    ngOnInit() {
    }
    dismiss() {
        this.dialogRef.close();
    }
    save() {
        this.mdFileService.CreateNewMd(this.data.fullPath, this.markdownTitle, this.data.level, this.selectedTemplate.id, this.selectedTemplate.documentType)
            .subscribe(data => {
            this.mdFileService.addNewFile(data);
            this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
        });
        this.dialogRef.close();
    }
}
NewMarkdownComponent.ɵfac = function NewMarkdownComponent_Factory(t) { return new (t || NewMarkdownComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"])); };
NewMarkdownComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: NewMarkdownComponent, selectors: [["app-new-markdown"]], decls: 31, vars: 6, consts: [[1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "Title", 3, "ngModel", "ngModelChange", "keyup.enter"], ["headerPosition", "below", 2, "justify-content", "space-between", "height", "calc(100%)"], ["label", "Documents"], ["aria-labelledby", "plantuml-radio-group-label", 1, "plantuml-radio-group", 3, "ngModel", "ngModelChange"], ["class", "plantuml-list-radio-button", 3, "value", 4, "ngFor", "ngForOf"], ["label", "Presentation"], ["aria-labelledby", "slide-radio-group-label", 1, "slide-radio-group", 3, "ngModel", "ngModelChange"], ["mat-button", "", "color", "primary", 3, "click"], [1, "plantuml-list-radio-button", 3, "value"]], template: function NewMarkdownComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "h3");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "New markdown");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "Document's title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewMarkdownComponent_Template_input_ngModelChange_8_listener($event) { return ctx.markdownTitle = $event; })("keyup.enter", function NewMarkdownComponent_Template_input_keyup_enter_8_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "Set the title's document");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "Select document template");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "mat-tab-group", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-tab", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-radio-group", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewMarkdownComponent_Template_mat_radio_group_ngModelChange_15_listener($event) { return ctx.selectedTemplate = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, NewMarkdownComponent_mat_radio_button_16_Template, 2, 2, "mat-radio-button", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-tab", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "mat-radio-group", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function NewMarkdownComponent_Template_mat_radio_group_ngModelChange_18_listener($event) { return ctx.selectedTemplate = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](19, NewMarkdownComponent_mat_radio_button_19_Template, 2, 2, "mat-radio-button", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](21);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](23, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewMarkdownComponent_Template_button_click_23_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](25, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](26, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function NewMarkdownComponent_Template_button_click_27_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](29, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](30, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.markdownTitle);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.selectedTemplate);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx.plantumlTemplates);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.selectedTemplate);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx.slideTemplates);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("Template: ", ctx.selectedTemplate.name, "");
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_5__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_6__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_4__["MatHint"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_7__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_7__["MatTab"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_8__["MatRadioGroup"], _angular_common__WEBPACK_IMPORTED_MODULE_9__["NgForOf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_10__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_11__["MatIcon"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_8__["MatRadioButton"]], styles: [".simple-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.plantuml-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  margin: 15px 0;\n  align-items: flex-start;\n  height: 200px;\n}\n\n.plantuml-list-radio-button[_ngcontent-%COMP%] {\n  margin: 5px;\n}\n\n.slide-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  margin: 15px 0;\n  align-items: flex-start;\n  height: 200px;\n}\n\n.slide-list-radio-button[_ngcontent-%COMP%] {\n  margin: 5px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXG5ldy1tYXJrZG93bi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsY0FBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtBQUFGOztBQUdBO0VBQ0UsV0FBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsY0FBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtBQUFGOztBQUdBO0VBQ0UsV0FBQTtBQUFGIiwiZmlsZSI6Im5ldy1tYXJrZG93bi5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5zaW1wbGUtY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcblxyXG4ucGxhbnR1bWwtcmFkaW8tZ3JvdXAge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBtYXJnaW46IDE1cHggMDtcclxuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICBoZWlnaHQ6IDIwMHB4O1xyXG59XHJcblxyXG4ucGxhbnR1bWwtbGlzdC1yYWRpby1idXR0b24ge1xyXG4gIG1hcmdpbjogNXB4O1xyXG59XHJcblxyXG4uc2xpZGUtcmFkaW8tZ3JvdXAge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBtYXJnaW46IDE1cHggMDtcclxuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICBoZWlnaHQ6MjAwcHg7XHJcbn1cclxuXHJcbi5zbGlkZS1saXN0LXJhZGlvLWJ1dHRvbiB7XHJcbiAgbWFyZ2luOiA1cHg7XHJcbn1cclxuIl19 */"] });


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
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/sidenav */ "XhcP");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../md-tree/md-tree.component */ "s8RO");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ../toolbar/toolbar.component */ "K3CX");
/* harmony import */ var _git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ../git-changes/git-changes.component */ "4dB/");
















function SidenavComponent_mat_tab_12_ng_template_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](0, "img", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2, "Git CI/CD");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} }
function SidenavComponent_mat_tab_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, SidenavComponent_mat_tab_12_ng_template_1_Template, 3, 0, "ng-template", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](2, "app-git-changes");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} }
const SMALL_WIDTH_BREAKPOINT = 720;
class SidenavComponent {
    constructor(breakpointObserver, mdFileService, router, currentFolder, ref, gitService) {
        this.breakpointObserver = breakpointObserver;
        this.mdFileService = mdFileService;
        this.router = router;
        this.currentFolder = currentFolder;
        this.ref = ref;
        this.gitService = gitService;
        this.sideNavWidth = "240px";
        this.hooked = false;
        this.classForBorderDiv = "border-div";
        this.currentBranch = null;
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
        this.gitService.getCurrentBranch().subscribe(_ => {
            this.currentBranch = _.name;
        });
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
SidenavComponent.ɵfac = function SidenavComponent_Factory(t) { return new (t || SidenavComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_cdk_layout__WEBPACK_IMPORTED_MODULE_1__["BreakpointObserver"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_3__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_app_current_folder_service__WEBPACK_IMPORTED_MODULE_4__["AppCurrentFolderService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectorRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__["GITService"])); };
SidenavComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SidenavComponent, selectors: [["app-sidenav"]], decls: 19, vars: 8, consts: [["autosize", "", 1, "app-sidenav-container"], [1, "mat-elevation-z10", 3, "opened", "mode"], ["sidenav", ""], ["color", "primary"], ["mat-icon-button", "", "id", "projectcss", "matTooltip", "projects list", 3, "click"], ["headerPosition", "below", 2, "justify-content", "space-between", "height", "calc(100% - 64px)"], ["label", "Project docs"], [4, "ngIf"], [1, "flex-container"], [3, "ngClass", "pointerdown", "mouseup"], [2, "width", "100%", "height", "100%"], [1, "app-sidenav-content", 2, "height", "100%"], [3, "ngClass", "toggleSidenav"], ["mat-tab-label", ""], ["src", "/assets/gitlogo.png"]], template: function SidenavComponent_Template(rf, ctx) { if (rf & 1) {
        const _r3 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
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
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](12, SidenavComponent_mat_tab_12_Template, 3, 0, "mat-tab", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "div", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("pointerdown", function SidenavComponent_Template_div_pointerdown_14_listener() { return ctx.resizeWidth(); })("mouseup", function SidenavComponent_Template_div_mouseup_14_listener() { return ctx.stopResizeWidth(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "div", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "app-toolbar", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("toggleSidenav", function SidenavComponent_Template_app_toolbar_toggleSidenav_17_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r3); const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵreference"](2); return _r0.toggle(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](18, "router-outlet");
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
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.currentBranch != null);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngClass", ctx.classForBorderDiv);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngClass", ctx.whatClass);
    } }, directives: [_angular_material_sidenav__WEBPACK_IMPORTED_MODULE_6__["MatSidenavContainer"], _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_6__["MatSidenav"], _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_7__["MatToolbar"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_9__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__["MatIcon"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_11__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_11__["MatTab"], _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_12__["MdTreeComponent"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgIf"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgClass"], _toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_14__["ToolbarComponent"], _angular_router__WEBPACK_IMPORTED_MODULE_3__["RouterOutlet"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_11__["MatTabLabel"], _git_changes_git_changes_component__WEBPACK_IMPORTED_MODULE_15__["GitChangesComponent"]], styles: [".app-sidenav-container[_ngcontent-%COMP%] {\n  flex: 1;\n  position: fixed;\n  width: 100%;\n  min-width: 100%;\n  height: 100%;\n  min-height: 100%;\n}\n\n.app-sidenav-content[_ngcontent-%COMP%] {\n  display: flex;\n  height: 100%;\n  width: auto;\n  flex-direction: column;\n}\n\n.app-sidenav[_ngcontent-%COMP%] {\n  width: 240px;\n}\n\na[_ngcontent-%COMP%]:active {\n  color: red;\n}\n\n.flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  width: 100%;\n  height: 100%;\n  min-width: 100%;\n  min-height: 100%;\n}\n\n.border-div[_ngcontent-%COMP%] {\n  border-left: 10px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.border-div-moving[_ngcontent-%COMP%] {\n  border-left: 50px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.showStartToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  display: contents;\n}\n\n.showToolbar[_ngcontent-%COMP%] {\n  height: 64px;\n  transition: height 300ms ease-out;\n}\n\n\n\n.hideToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  transition: height 300ms ease-out;\n}\n\n.hideToolbarNone[_ngcontent-%COMP%] {\n  display: none;\n}\n\n .mat-tab-label,  .mat-tab-label-active {\n  min-width: 0 !important;\n  padding: 3px !important;\n  margin: 3px !important;\n}\n\n#projectcss[_ngcontent-%COMP%]    .mat-tooltip {\n  \n  \n  color: white;\n  background-color: blue;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2lkZW5hdi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFJQTtFQUNFLE9BQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFIRjs7QUFNQTtFQUNFLGFBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLHNCQUFBO0FBSEY7O0FBTUE7RUFDRSxZQUFBO0FBSEY7O0FBTUE7RUFDRSxVQUFBO0FBSEY7O0FBTUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQUhGOztBQU1BO0VBQ0UsaUJBQUE7RUFDQSx3QkFBQTtFQUNBLHVCQUFBO0VBQ0EsZ0JBQUE7QUFIRjs7QUFNQTtFQUNFLGlCQUFBO0VBQ0Esd0JBQUE7RUFDQSx1QkFBQTtFQUNBLGdCQUFBO0FBSEY7O0FBTUE7RUFDSSxXQUFBO0VBQ0EsaUJBQUE7QUFISjs7QUFPQTtFQUNFLFlBQUE7RUFDQSxpQ0FBQTtBQUpGOztBQU9BOztFQUFBOztBQUlBO0VBQ0UsV0FBQTtFQUNBLGlDQUFBO0FBTEY7O0FBUUE7RUFDSSxhQUFBO0FBTEo7O0FBVUE7RUFDRSx1QkFBQTtFQUNBLHVCQUFBO0VBQ0Esc0JBQUE7QUFQRjs7QUFXRTtFQUNFLGdDQUFBO0VBQ0EsU0FBQTtFQUNBLFlBQUE7RUFDQSxzQkFBQTtBQVJKIiwiZmlsZSI6InNpZGVuYXYuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyJcclxuXHJcblxyXG5cclxuLmFwcC1zaWRlbmF2LWNvbnRhaW5lciB7XHJcbiAgZmxleDogMTtcclxuICBwb3NpdGlvbjogZml4ZWQ7XHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgbWluLXdpZHRoOiAxMDAlO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBtaW4taGVpZ2h0OiAxMDAlO1xyXG59XHJcblxyXG4uYXBwLXNpZGVuYXYtY29udGVudCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgd2lkdGg6YXV0bztcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4uYXBwLXNpZGVuYXYge1xyXG4gIHdpZHRoOiAyNDBweDtcclxufVxyXG5cclxuYTphY3RpdmUge1xyXG4gIGNvbG9yOiByZWQ7XHJcbn1cclxuXHJcbi5mbGV4LWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogcm93OyAgXHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgaGVpZ2h0OiAxMDAlO1xyXG4gIG1pbi13aWR0aDogMTAwJTtcclxuICBtaW4taGVpZ2h0OiAxMDAlO1xyXG59XHJcblxyXG4uYm9yZGVyLWRpdiB7XHJcbiAgYm9yZGVyLWxlZnQ6IDEwcHg7XHJcbiAgYm9yZGVyLWxlZnQtc3R5bGU6IHNvbGlkO1xyXG4gIGJvcmRlci1sZWZ0LWNvbG9yOiBncmV5O1xyXG4gIGN1cnNvcjp3LXJlc2l6ZTtcclxufVxyXG5cclxuLmJvcmRlci1kaXYtbW92aW5nIHtcclxuICBib3JkZXItbGVmdDogNTBweDtcclxuICBib3JkZXItbGVmdC1zdHlsZTogc29saWQ7XHJcbiAgYm9yZGVyLWxlZnQtY29sb3I6IGdyZXk7XHJcbiAgY3Vyc29yOiB3LXJlc2l6ZTtcclxufVxyXG5cclxuLnNob3dTdGFydFRvb2xiYXJ7XHJcbiAgICBoZWlnaHQ6MHB4O1xyXG4gICAgZGlzcGxheTpjb250ZW50cztcclxufVxyXG5cclxuLy8gTmFzY29uZGVyZSBlIHZpc3VhbGl6emFyZSBsYSB0b29sYmFyXHJcbi5zaG93VG9vbGJhciB7XHJcbiAgaGVpZ2h0OiA2NHB4O1xyXG4gIHRyYW5zaXRpb246IGhlaWdodCAzMDBtcyBlYXNlLW91dDtcclxufVxyXG5cclxuLyouc2hvd1Rvb2xiYXJCbG9ja3tcclxuICAgIGRpc3BsYXk6YmxvY2s7XHJcbn0qL1xyXG5cclxuLmhpZGVUb29sYmFyIHtcclxuICBoZWlnaHQ6MHB4O1xyXG4gIHRyYW5zaXRpb246ICBoZWlnaHQgMzAwbXMgZWFzZS1vdXQ7ICBcclxufVxyXG5cclxuLmhpZGVUb29sYmFyTm9uZXtcclxuICAgIGRpc3BsYXk6bm9uZTtcclxufVxyXG5cclxuXHJcbi8vIHBlciBmYXJlIHBpY2NvbGkgaSB0YWJcclxuOjpuZy1kZWVwLm1hdC10YWItbGFiZWwsIDo6bmctZGVlcC5tYXQtdGFiLWxhYmVsLWFjdGl2ZSB7XHJcbiAgbWluLXdpZHRoOiAwICFpbXBvcnRhbnQ7XHJcbiAgcGFkZGluZzogM3B4ICFpbXBvcnRhbnQ7XHJcbiAgbWFyZ2luOiAzcHggIWltcG9ydGFudDtcclxufVxyXG5cclxuI3Byb2plY3Rjc3Mge1xyXG4gIDo6bmctZGVlcC5tYXQtdG9vbHRpcCB7XHJcbiAgICAvKiB5b3VyIG93biBjdXN0b20gc3R5bGVzIGhlcmUgKi9cclxuICAgIC8qIGUuZy4gKi9cclxuICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgIGJhY2tncm91bmQtY29sb3I6IGJsdWU7XHJcbiAgfVxyXG59XHJcbiJdfQ== */"] });


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
/* harmony import */ var _signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../signalr/services/server-messages.service */ "jQFx");
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
        //helloWorld: IWorkWithElement = (msg) => {
        //  alert('this is the callback');
        //};
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
MainContentComponent.ɵfac = function MainContentComponent_Factory(t) { return new (t || MainContentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__["DomSanitizer"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__["ServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"])); };
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
/* harmony import */ var _dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../dialogs/change-directory/change-directory.component */ "HotF");
/* harmony import */ var _dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../dialogs/new-directory/new-directory.component */ "p8DV");
/* harmony import */ var _dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../dialogs/new-markdown/new-markdown.component */ "WxmV");
/* harmony import */ var _dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../dialogs/delete-markdown/delete-markdown.component */ "BUpo");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/cdk/clipboard */ "UXJo");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");



















function MdTreeComponent_ng_template_3_button_1_Template(rf, ctx) { if (rf & 1) {
    const _r14 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_1_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r14); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r12.renameDirectoryOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "img", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_2_Template(rf, ctx) { if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_2_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r17); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r15.createDirectoryOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_3_Template(rf, ctx) { if (rf & 1) {
    const _r20 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_3_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r20); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r18.createMdOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "note_add");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_4_Template(rf, ctx) { if (rf & 1) {
    const _r23 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_4_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r23); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r21.deleteFile(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_5_Template(rf, ctx) { if (rf & 1) {
    const _r26 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_5_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r26); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r24.getLinkFromNode(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_6_Template(rf, ctx) { if (rf & 1) {
    const _r29 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_6_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r29); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r27.openFolderOn(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "b", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "b", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5, "File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_7_Template(rf, ctx) { if (rf & 1) {
    const _r32 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_7_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32); const item_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().item; const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r30.setMdAsLandingPage(item_r4); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "home");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "Set as project's landing page");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, MdTreeComponent_ng_template_3_button_1_Template, 2, 0, "button", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, MdTreeComponent_ng_template_3_button_2_Template, 3, 0, "button", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](3, MdTreeComponent_ng_template_3_button_3_Template, 3, 0, "button", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](4, MdTreeComponent_ng_template_3_button_4_Template, 3, 0, "button", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](5, MdTreeComponent_ng_template_3_button_5_Template, 3, 0, "button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](6, MdTreeComponent_ng_template_3_button_6_Template, 6, 0, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](7, MdTreeComponent_ng_template_3_button_7_Template, 5, 0, "button", 11);
} if (rf & 2) {
    const item_r4 = ctx.item;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "folder" || item_r4.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "folder" || item_r4.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "mdFile");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "mdFile");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "folder" || item_r4.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", item_r4.type === "mdFile");
} }
function MdTreeComponent_mat_tree_node_5_Template(rf, ctx) { if (rf & 1) {
    const _r35 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-tree-node", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_5_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r35); const node_r33 = ctx.$implicit; const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r34.onRightClick($event, node_r33); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "button", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "text_snippet");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "a", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function MdTreeComponent_mat_tree_node_5_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r35); const node_r33 = ctx.$implicit; const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r36.getNode(node_r33); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r33 = ctx.$implicit;
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngClass", ctx_r2.activeNode === node_r33 ? "background-highlight" : "blue-link");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](node_r33.name);
} }
function MdTreeComponent_mat_tree_node_6_Template(rf, ctx) { if (rf & 1) {
    const _r39 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-tree-node", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_6_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r39); const node_r37 = ctx.$implicit; const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](); return ctx_r38.onRightClick($event, node_r37); });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "button", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "mat-icon", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r37 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("aria-label", "Toggle " + node_r37.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r3.treeControl.isExpanded(node_r37) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", node_r37.name, " ");
} }
const TREE_DATA = [];
class MdTreeComponent {
    constructor(mdFileService, dialog, snackBar, clipboard) {
        this.mdFileService = mdFileService;
        this.dialog = dialog;
        this.snackBar = snackBar;
        this.clipboard = clipboard;
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
        objectThis.mdFileService.getLandingPage().subscribe(node => {
            if (node != null) {
                objectThis.mdFileService.setSelectedMdFileFromSideNav(node);
                objectThis.activeNode = node;
            }
        });
    }
    onRightClick(event, item) {
        // preventDefault avoids to show the visualization of the right-click menu of the browser
        event.preventDefault();
        if (item == null) {
            item = new _models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]("root", "root", 0, false);
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
    getNode(node) {
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.activeNode = node;
    }
    // Manu management
    createMdOn(node) {
        this.dialog.open(_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_6__["NewMarkdownComponent"], {
            width: '300px',
            //height:'400px',
            data: node,
        });
    }
    setMdAsLandingPage(node) {
        this.mdFileService.SetLandingPage(node).subscribe(_ => {
            this.snackBar.open(node.name, "is project landing page", { duration: 5000 });
        });
    }
    createDirectoryOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_5__["NewDirectoryComponent"], {
            width: '300px',
            data: node,
        });
    }
    renameDirectoryOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_4__["ChangeDirectoryComponent"], {
            width: '300px',
            data: node,
        });
    }
    openFolderOn(node) {
        this.mdFileService.openFolderOnFileExplorer(node).subscribe(_ => {
            this.snackBar.open("file explorer open", "", { duration: 500 });
        });
    }
    getLinkFromNode(node) {
        this.clipboard.copy(node.relativePath);
    }
    deleteFile(node) {
        this.dialog.open(_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_7__["DeleteMarkdownComponent"], {
            width: '300px',
            data: node,
        });
    }
}
MdTreeComponent.ɵfac = function MdTreeComponent_Factory(t) { return new (t || MdTreeComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_9__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_10__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_11__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_12__["Clipboard"])); };
MdTreeComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({ type: MdTreeComponent, selectors: [["app-md-tree"]], viewQuery: function MdTreeComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 7, vars: 8, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [3, "dataSource", "treeControl", "contextmenu"], ["matTreeNodePadding", "", "style", "cursor:pointer", 3, "contextmenu", 4, "matTreeNodeDef"], ["style", "cursor:pointer", "matTreeNodePadding", "", 3, "contextmenu", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new document", "mat-icon-button", "", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click", 4, "ngIf"], ["mat-menu-item", "", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click"], ["src", "/assets/rename.png", "alt", "Rename directory"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click"], ["mat-icon-button", "", "matTooltip", "create new document", "mat-icon-button", "", "color", "primary", 3, "click"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["matTreeNodePadding", "", 2, "cursor", "pointer", 3, "contextmenu"], ["mat-icon-button", "", "color", "primary", 2, "cursor", "pointer"], [1, "mat-icon-rtl-mirror"], [3, "ngClass", "click"], ["mat-icon-button", "", "matTreeNodeToggle", ""], [1, "gold-icon", "mat-icon-rtl-mirror"]], template: function MdTreeComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](3, MdTreeComponent_ng_template_3_Template, 8, 7, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "mat-tree", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("contextmenu", function MdTreeComponent_Template_mat_tree_contextmenu_4_listener($event) { return ctx.onRightClick($event, null); });
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](5, MdTreeComponent_mat_tree_node_5_Template, 6, 2, "mat-tree-node", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](6, MdTreeComponent_mat_tree_node_6_Template, 5, 3, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isFolder);
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuContent"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeDef"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_14__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_15__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__["MatIcon"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuItem"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodePadding"], _angular_common__WEBPACK_IMPORTED_MODULE_13__["NgClass"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeToggle"]], styles: [".mat-tree-node[_ngcontent-%COMP%] {\n  min-height: 40px !important;\n}\n\n.blue-link[_ngcontent-%COMP%] {\n  color: blue;\n  border-bottom-width: 1px;\n  border-bottom: dashed;\n  border-bottom-color: grey;\n}\n\n.background-highlight[_ngcontent-%COMP%] {\n  background-color: blue;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbWQtdHJlZS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLDJCQUFBO0FBQ0Y7O0FBQ0E7RUFDRSxXQUFBO0VBQ0Esd0JBQUE7RUFDQSxxQkFBQTtFQUNBLHlCQUFBO0FBRUY7O0FBQ0E7RUFDRSxzQkFBQTtFQUNBLFlBQUE7QUFFRiIsImZpbGUiOiJtZC10cmVlLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm1hdC10cmVlLW5vZGUge1xyXG4gIG1pbi1oZWlnaHQ6IDQwcHggIWltcG9ydGFudDtcclxufVxyXG4uYmx1ZS1saW5rIHtcclxuICBjb2xvcjogYmx1ZTtcclxuICBib3JkZXItYm90dG9tLXdpZHRoOiAxcHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogZGFzaGVkO1xyXG4gIGJvcmRlci1ib3R0b20tY29sb3I6IGdyZXk7XHJcbn1cclxuXHJcbi5iYWNrZ3JvdW5kLWhpZ2hsaWdodCB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogYmx1ZTtcclxuICBjb2xvcjogd2hpdGU7XHJcbn1cclxuIl19 */"] });


/***/ })

}]);
//# sourceMappingURL=md-explorer-md-explorer-module.js.map