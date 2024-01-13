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

/***/ "/6Vx":
/*!*************************************************************************************!*\
  !*** ./src/app/md-explorer/components/gitlab-settings/gitlab-settings.component.ts ***!
  \*************************************************************************************/
/*! exports provided: GitlabSettingsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitlabSettingsComponent", function() { return GitlabSettingsComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class GitlabSettingsComponent {
    constructor() { }
    ngOnInit() {
    }
}
GitlabSettingsComponent.ɵfac = function GitlabSettingsComponent_Factory(t) { return new (t || GitlabSettingsComponent)(); };
GitlabSettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: GitlabSettingsComponent, selectors: [["app-gitlab-settings"]], decls: 3, vars: 0, consts: [[1, "container-columns"], [1, "container-title"], ["src", "/assets/gitlabcicd.png", 2, "width", "300px"]], template: function GitlabSettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](2, "img", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } }, styles: [".container-title[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: center;\n}\n\n.container-columns[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  justify-content: center;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0bGFiLXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSx1QkFBQTtBQUNGIiwiZmlsZSI6ImdpdGxhYi1zZXR0aW5ncy5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5jb250YWluZXItdGl0bGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbn1cclxuXHJcbi5jb250YWluZXItY29sdW1ucyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG59XHJcbiJdfQ== */"] });


/***/ }),

/***/ "/q41":
/*!********************************************************!*\
  !*** ./src/app/md-explorer/services/Types/Bookmark.ts ***!
  \********************************************************/
/*! exports provided: Bookmark */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "Bookmark", function() { return Bookmark; });
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");

class Bookmark extends _models_md_file__WEBPACK_IMPORTED_MODULE_0__["MdFile"] {
    constructor(mdFile) {
        super(mdFile.name, mdFile.path, mdFile.level, mdFile.expandable);
        this.childrens = mdFile.childrens;
        this.fullDirectoryPath = mdFile.fullDirectoryPath;
        this.fullPath = mdFile.fullPath;
        this.index = mdFile.index;
        this.isLoading = mdFile.isLoading;
        this.relativePath = mdFile.relativePath;
        this.type = mdFile.type;
    }
}


/***/ }),

/***/ "5hcZ":
/*!*************************************************************************************!*\
  !*** ./src/app/md-explorer/components/publish-md-tree/publish-md-tree.component.ts ***!
  \*************************************************************************************/
/*! exports provided: PublishMdTreeComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "PublishMdTreeComponent", function() { return PublishMdTreeComponent; });
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/cdk/tree */ "FvrZ");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../dialogs/new-markdown/new-markdown.component */ "WxmV");
/* harmony import */ var _dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../dialogs/new-directory/new-directory.component */ "p8DV");
/* harmony import */ var _dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../dialogs/change-directory/change-directory.component */ "HotF");
/* harmony import */ var _dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../dialogs/delete-markdown/delete-markdown.component */ "BUpo");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _projects_new_project_new_project_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../../../projects/new-project/new-project.component */ "5Zmz");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _services_projects_service__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../../services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/cdk/clipboard */ "UXJo");
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/tree */ "8yBR");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");























function PublishMdTreeComponent_ng_template_3_button_1_Template(rf, ctx) { if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_1_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r15); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r13.renameDirectoryOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelement"](1, "img", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_2_Template(rf, ctx) { if (rf & 1) {
    const _r18 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_2_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r18); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r16.createDirectoryOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](2, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_3_Template(rf, ctx) { if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_3_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r21); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r19.createMdOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](2, "note_add");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_4_Template(rf, ctx) { if (rf & 1) {
    const _r24 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_4_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r24); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r22.deleteFile(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_5_Template(rf, ctx) { if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_5_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r27); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r25.getLinkFromNode(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_6_Template(rf, ctx) { if (rf & 1) {
    const _r30 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_6_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r30); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r28.openGitlabSettingsFromNode(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelement"](1, "img", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_button_7_Template(rf, ctx) { if (rf & 1) {
    const _r33 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "button", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_ng_template_3_button_7_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r33); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]().item; const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r31.openFolderOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "b", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](2, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](3, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](4, "b", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](5, "File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} }
function PublishMdTreeComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](1, PublishMdTreeComponent_ng_template_3_button_1_Template, 2, 0, "button", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](2, PublishMdTreeComponent_ng_template_3_button_2_Template, 3, 0, "button", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](3, PublishMdTreeComponent_ng_template_3_button_3_Template, 3, 0, "button", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](4, PublishMdTreeComponent_ng_template_3_button_4_Template, 3, 0, "button", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](5, PublishMdTreeComponent_ng_template_3_button_5_Template, 3, 0, "button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](6, PublishMdTreeComponent_ng_template_3_button_6_Template, 2, 0, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](7, PublishMdTreeComponent_ng_template_3_button_7_Template, 6, 0, "button", 12);
} if (rf & 2) {
    const item_r5 = ctx.item;
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "publishFolder");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "publishFolder" || item_r5.type === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "publishFolder" || item_r5.type === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngIf", item_r5.type === "publishFolder" || item_r5.type === "root");
} }
function PublishMdTreeComponent_mat_tree_node_5_Template(rf, ctx) { if (rf & 1) {
    const _r36 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "mat-tree-node", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("contextmenu", function PublishMdTreeComponent_mat_tree_node_5_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r36); const node_r34 = ctx.$implicit; const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r35.onRightClick($event, node_r34); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "button", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](3, "text_snippet");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](4, "a", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("click", function PublishMdTreeComponent_mat_tree_node_5_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r36); const node_r34 = ctx.$implicit; const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r37.getNode(node_r34); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r34 = ctx.$implicit;
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("ngClass", ctx_r2.activeNode === node_r34 ? "background-highlight" : "accent-link");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtextInterpolate"](node_r34.name);
} }
function PublishMdTreeComponent_mat_tree_node_6_Template(rf, ctx) { if (rf & 1) {
    const _r40 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "mat-tree-node", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("contextmenu", function PublishMdTreeComponent_mat_tree_node_6_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r40); const node_r38 = ctx.$implicit; const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r39.onRightClick($event, node_r38); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "button", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r38 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵattribute"]("aria-label", "Toggle " + node_r38.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtextInterpolate1"](" ", ctx_r3.treeControl.isExpanded(node_r38) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtextInterpolate1"](" ", node_r38.name, " ");
} }
function PublishMdTreeComponent_mat_tree_node_7_Template(rf, ctx) { if (rf & 1) {
    const _r43 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](0, "mat-tree-node", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵlistener"]("contextmenu", function PublishMdTreeComponent_mat_tree_node_7_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵrestoreView"](_r43); const node_r41 = ctx.$implicit; const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵnextContext"](); return ctx_r42.onRightClick($event, node_r41); });
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "button", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](3, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r41 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵattribute"]("aria-label", "Toggle " + node_r41.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtextInterpolate1"](" ", node_r41.name, " ");
} }
class DynamicDataSource {
    constructor(_treeControl, _database, _mdFileService) {
        this._treeControl = _treeControl;
        this._database = _database;
        this._mdFileService = _mdFileService;
        this.dataChange = new rxjs__WEBPACK_IMPORTED_MODULE_3__["BehaviorSubject"]([]);
        this.data = _database.initialData();
        this._mdFileService.loadPublishNodes('root', 0).subscribe(_ => {
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
        this._mdFileService.loadPublishNodes(node.path, node.level + 1).subscribe(_ => {
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
}
class PublishMdTreeComponent {
    constructor(router, database, mdFileService, projectService, dialog, snackBar, clipboard) {
        this.router = router;
        this.database = database;
        this.mdFileService = mdFileService;
        this.projectService = projectService;
        this.dialog = dialog;
        this.snackBar = snackBar;
        this.clipboard = clipboard;
        this.menuTopLeftPosition = { x: 0, y: 0 };
        this.isFolder = (_, node) => node.type == "publishFolder";
        this.isEmptyRoot = (_, node) => node.type == "root";
        this.getLevel = (node) => node.level;
        this.isExpandable = (node) => node.expandable;
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__["FlatTreeControl"](this.getLevel, this.isExpandable);
        this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService);
    }
    ngOnInit() {
    }
    getFolder(node) {
        this.folder.name = node.name;
        this.folder.path = node.path;
    }
    onRightClick(event, item) {
        // preventDefault avoids to show the visualization of the right-click menu of the browser
        event.preventDefault();
        if (item == null) {
            item = new _models_md_file__WEBPACK_IMPORTED_MODULE_2__["MdFile"]("root", "root", 0, false);
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
    createMdOn(node) {
        this.dialog.open(_dialogs_new_markdown_new_markdown_component__WEBPACK_IMPORTED_MODULE_5__["NewMarkdownComponent"], {
            width: '300px',
            //height:'400px',
            data: node,
        });
    }
    createDirectoryOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_2__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_new_directory_new_directory_component__WEBPACK_IMPORTED_MODULE_6__["NewDirectoryComponent"], {
            width: '300px',
            data: node,
        });
    }
    renameDirectoryOn(node) {
        if (node == null) {
            node = new _models_md_file__WEBPACK_IMPORTED_MODULE_2__["MdFile"]("root", "root", 0, false);
            node.fullPath = "root";
        }
        this.dialog.open(_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_7__["ChangeDirectoryComponent"], {
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
        this.dialog.open(_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_8__["DeleteMarkdownComponent"], {
            width: '300px',
            data: node,
        });
    }
    getNode(node) {
        this.router.navigate(['/main/navigation/document']);
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.activeNode = node;
    }
    openGitlabSettingsFromNode(node) {
        this.router.navigate(['/main/navigation/gitlabsettings']);
    }
}
PublishMdTreeComponent.ɵfac = function PublishMdTreeComponent_Factory(t) { return new (t || PublishMdTreeComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_10__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_projects_new_project_new_project_component__WEBPACK_IMPORTED_MODULE_11__["DynamicDatabase"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_12__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_services_projects_service__WEBPACK_IMPORTED_MODULE_13__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_14__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_15__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_16__["Clipboard"])); };
PublishMdTreeComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵdefineComponent"]({ type: PublishMdTreeComponent, selectors: [["app-publish-md-tree"]], viewQuery: function PublishMdTreeComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 8, vars: 9, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [3, "dataSource", "treeControl"], ["color", "accent", "matTreeNodePadding", "", "style", "cursor:pointer", 3, "contextmenu", 4, "matTreeNodeDef"], ["style", "cursor:pointer", "matTreeNodePadding", "", 3, "contextmenu", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new document", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "Gitlab settings", 3, "click", 4, "ngIf"], ["mat-menu-item", "", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click"], ["src", "/assets/rename.png", "alt", "Rename directory"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click"], ["mat-icon-button", "", "matTooltip", "create new document", "color", "primary", 3, "click"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "Gitlab settings", 3, "click"], ["src", "/assets/gitlab.png"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["color", "accent", "matTreeNodePadding", "", 2, "cursor", "pointer", 3, "contextmenu"], ["mat-icon-button", "", 2, "cursor", "pointer"], [3, "ngClass", "click"], ["matTreeNodePadding", "", 2, "cursor", "pointer", 3, "contextmenu"], ["mat-icon-button", "", "matTreeNodeToggle", "", "color", "accent"]], template: function PublishMdTreeComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](3, PublishMdTreeComponent_ng_template_3_Template, 8, 7, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementStart"](4, "mat-tree", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](5, PublishMdTreeComponent_mat_tree_node_5_Template, 6, 2, "mat-tree-node", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](6, PublishMdTreeComponent_mat_tree_node_6_Template, 5, 3, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵtemplate"](7, PublishMdTreeComponent_mat_tree_node_7_Template, 5, 2, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isFolder);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_9__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isEmptyRoot);
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuContent"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__["MatTreeNodeDef"], _angular_common__WEBPACK_IMPORTED_MODULE_18__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_19__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_20__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_21__["MatIcon"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuItem"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__["MatTreeNodePadding"], _angular_common__WEBPACK_IMPORTED_MODULE_18__["NgClass"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_17__["MatTreeNodeToggle"]], styles: [".mat-tree-node[_ngcontent-%COMP%] {\n  min-height: 40px !important;\n}\n\n.red-icon[_ngcontent-%COMP%] {\n  color: red;\n}\n\n.accent-link[_ngcontent-%COMP%] {\n  color: blue;\n  border-bottom-width: 1px;\n  border-bottom: dashed;\n  border-bottom-color: palevioletred;\n}\n\n.background-highlight[_ngcontent-%COMP%] {\n  background-color: purple;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxccHVibGlzaC1tZC10cmVlLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsMkJBQUE7QUFDRjs7QUFHQTtFQUNFLFVBQUE7QUFBRjs7QUFHQTtFQUNFLFdBQUE7RUFDQSx3QkFBQTtFQUNBLHFCQUFBO0VBQ0Esa0NBQUE7QUFBRjs7QUFHQTtFQUNFLHdCQUFBO0VBQ0EsWUFBQTtBQUFGIiwiZmlsZSI6InB1Ymxpc2gtbWQtdHJlZS5jb21wb25lbnQuc2NzcyIsInNvdXJjZXNDb250ZW50IjpbIi5tYXQtdHJlZS1ub2RlIHtcclxuICBtaW4taGVpZ2h0OiA0MHB4ICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcblxyXG4ucmVkLWljb24ge1xyXG4gIGNvbG9yOiByZWQ7XHJcbn1cclxuXHJcbi5hY2NlbnQtbGluayB7XHJcbiAgY29sb3I6IGJsdWU7XHJcbiAgYm9yZGVyLWJvdHRvbS13aWR0aDogMXB4O1xyXG4gIGJvcmRlci1ib3R0b206IGRhc2hlZDtcclxuICBib3JkZXItYm90dG9tLWNvbG9yOiBwYWxldmlvbGV0cmVkO1xyXG59XHJcblxyXG4uYmFja2dyb3VuZC1oaWdobGlnaHQge1xyXG4gIGJhY2tncm91bmQtY29sb3I6IHB1cnBsZTtcclxuICBjb2xvcjogd2hpdGU7XHJcbn1cclxuIl19 */"] });


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
/* harmony import */ var _signalR_dialogs_connection_lost_connection_lost_component__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ../signalR/dialogs/connection-lost/connection-lost.component */ "9LnC");
/* harmony import */ var _signalR_dialogs_parsing_project_parsing_project_component__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ../signalR/dialogs/parsing-project/parsing-project.component */ "oPln");
/* harmony import */ var _components_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./components/dialogs/change-directory/change-directory.component */ "HotF");
/* harmony import */ var _components_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./components/dialogs/delete-markdown/delete-markdown.component */ "BUpo");
/* harmony import */ var _components_publish_md_tree_publish_md_tree_component__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ./components/publish-md-tree/publish-md-tree.component */ "5hcZ");
/* harmony import */ var _components_gitlab_settings_gitlab_settings_component__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ./components/gitlab-settings/gitlab-settings.component */ "/6Vx");
/* harmony import */ var _components_document_settings_document_settings_component__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! ./components/document-settings/document-settings.component */ "oTTF");
/* harmony import */ var _components_dialogs_copy_from_clipboard_copy_from_clipboard_component__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! ./components/dialogs/copy-from-clipboard/copy-from-clipboard.component */ "WD3D");
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! ../git/git.module */ "Trdj");
/* harmony import */ var _components_dialogs_move_md_file_move_md_file_component__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! ./components/dialogs/move-md-file/move-md-file.component */ "N7Dz");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/core */ "fXoL");





























const routes = [
    { path: '', component: _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"] },
    {
        path: 'navigation', component: _components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"],
        children: [
            { path: 'document', component: _components_main_content_main_content_component__WEBPACK_IMPORTED_MODULE_3__["MainContentComponent"] },
            { path: 'gitlabsettings', component: _components_gitlab_settings_gitlab_settings_component__WEBPACK_IMPORTED_MODULE_22__["GitlabSettingsComponent"] },
            { path: 'documentsettings', component: _components_document_settings_document_settings_component__WEBPACK_IMPORTED_MODULE_23__["DocumentSettingsComponent"] },
        ]
    }
];
class MdExplorerModule {
    constructor() {
        console.log('constructor MdExplorerModule');
    }
}
MdExplorerModule.ɵfac = function MdExplorerModule_Factory(t) { return new (t || MdExplorerModule)(); };
MdExplorerModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_27__["ɵɵdefineNgModule"]({ type: MdExplorerModule });
MdExplorerModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_27__["ɵɵdefineInjector"]({ providers: [
        _services_md_file_service__WEBPACK_IMPORTED_MODULE_8__["MdFileService"],
    ], imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_4__["MaterialModule"],
            _angular_common_http__WEBPACK_IMPORTED_MODULE_9__["HttpClientModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_5__["FormsModule"],
            _git_git_module__WEBPACK_IMPORTED_MODULE_25__["GitModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_7__["RouterModule"].forChild(routes)
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_27__["ɵɵsetNgModuleScope"](MdExplorerModule, { declarations: [_components_sidenav_sidenav_component__WEBPACK_IMPORTED_MODULE_1__["SidenavComponent"],
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
        _signalR_dialogs_connection_lost_connection_lost_component__WEBPACK_IMPORTED_MODULE_17__["ConnectionLostComponent"],
        _signalR_dialogs_parsing_project_parsing_project_component__WEBPACK_IMPORTED_MODULE_18__["ParsingProjectComponent"],
        _components_dialogs_change_directory_change_directory_component__WEBPACK_IMPORTED_MODULE_19__["ChangeDirectoryComponent"],
        _components_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_20__["DeleteMarkdownComponent"],
        _components_publish_md_tree_publish_md_tree_component__WEBPACK_IMPORTED_MODULE_21__["PublishMdTreeComponent"],
        _components_gitlab_settings_gitlab_settings_component__WEBPACK_IMPORTED_MODULE_22__["GitlabSettingsComponent"],
        _components_document_settings_document_settings_component__WEBPACK_IMPORTED_MODULE_23__["DocumentSettingsComponent"],
        _components_dialogs_copy_from_clipboard_copy_from_clipboard_component__WEBPACK_IMPORTED_MODULE_24__["CopyFromClipboardComponent"],
        _components_dialogs_move_md_file_move_md_file_component__WEBPACK_IMPORTED_MODULE_26__["MoveMdFileComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_4__["MaterialModule"],
        _angular_common_http__WEBPACK_IMPORTED_MODULE_9__["HttpClientModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_5__["FormsModule"],
        _git_git_module__WEBPACK_IMPORTED_MODULE_25__["GitModule"], _angular_router__WEBPACK_IMPORTED_MODULE_7__["RouterModule"]] }); })();


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
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "WOW! Relinking analysis!");
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
/* harmony import */ var _refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../refactoring/rename-file/rename-file.component */ "FboE");
/* harmony import */ var _signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../signalR/dialogs/rules/rules.component */ "+59A");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/menu */ "STbY");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog/models/WaitingDialogInfo */ "N+BC");
/* harmony import */ var _git_components_git_auth_git_auth_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../git/components/git-auth/git-auth.component */ "gPg1");
/* harmony import */ var _git_models_pullInfo__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../git/models/pullInfo */ "euKz");
/* harmony import */ var _git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../git/components/git-messages/git-messages.component */ "jwHG");
/* harmony import */ var _services_Types_Bookmark__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../services/Types/Bookmark */ "/q41");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../../../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ../../../services/app-current-metadata.service */ "QI1B");
/* harmony import */ var _services_projects_service__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ../../services/projects.service */ "vUCT");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ "eAi6");
/* harmony import */ var _services_bookmarks_service__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ../../services/bookmarks.service */ "c/kW");
/* harmony import */ var _services_md_navigation_service__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ../../services/md-navigation.service */ "hFUY");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/common */ "ofXK");





























function ToolbarComponent_button_20_Template(rf, ctx) { if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_button_20_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r5); const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](); return ctx_r4.checkConnection(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "mat-icon", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
} }
function ToolbarComponent_span_21_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "img", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
} }
function ToolbarComponent_span_22_button_2_Template(rf, ctx) { if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_span_22_button_2_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r9); const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2); return ctx_r8.commit(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"]("", ctx_r6.howManyFilesAreToCommit, " to commit");
} }
function ToolbarComponent_span_22_button_3_Template(rf, ctx) { if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_span_22_button_3_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r11); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2); return ctx_r10.pull(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"]("", ctx_r7.howManyFilesAreToPull, " to pull");
} }
function ToolbarComponent_span_22_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ToolbarComponent_span_22_button_2_Template, 2, 1, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](3, ToolbarComponent_span_22_button_3_Template, 2, 1, "button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "button", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](6, "img", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    const _r3 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵreference"](24);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r2.somethingIsChangedInTheBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r2.somethingIsToPull);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matMenuTriggerFor", _r3)("ngClass", !ctx_r2.somethingIsChangedInTheBranch ? "branchGood" : "branchToCommit");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" [", ctx_r2.currentBranch, "] ");
} }
class ToolbarComponent {
    //@Output() toggleSidenav = new EventEmitter<void>();
    constructor(dialog, monitorMDService, http, _snackBar, mdFileService, gitservice, appSettings, projectService, router, waitingDialogService, bookmarksService, navService) {
        this.dialog = dialog;
        this.monitorMDService = monitorMDService;
        this.http = http;
        this._snackBar = _snackBar;
        this.mdFileService = mdFileService;
        this.gitservice = gitservice;
        this.appSettings = appSettings;
        this.projectService = projectService;
        this.router = router;
        this.waitingDialogService = waitingDialogService;
        this.bookmarksService = bookmarksService;
        this.navService = navService;
        this.connectionIsActive = true;
        this.isCheckingConnection = false;
        this.screenType = "fullscreen";
        this.TitleToShow = "MdExplorer";
        this.connectionIsActive = true;
    }
    ngOnInit() {
        this.monitorMDService.addMdProcessedListener(this.markdownFileIsProcessed, this);
        this.monitorMDService.addPdfIsReadyListener(this.showPdfIsready, this); //TODO: da spostare in SignalR
        this.monitorMDService.addMdRule1Listener(this.showRule1IsBroken, this); //TODO: da spostare in SignalR
        // get current branch name and if the branch has something to commit    
        this.gitservice.currentBranch$.subscribe(branch => {
            this.currentBranch = branch.name;
            this.somethingIsChangedInTheBranch = branch.somethingIsChangedInTheBranch;
            this.howManyFilesAreToCommit = branch.howManyFilesAreChanged;
            this.connectionIsActive = true;
        });
        this.gitservice.commmitsToPull$.subscribe(_ => {
            this.somethingIsToPull = _.somethingIsToPull;
            this.howManyFilesAreToPull = _.howManyFilesAreToPull;
            this.connectionIsActive = _.connectionIsActive;
            this.isCheckingConnection = false;
        });
        this.checkConnection();
        // manage resize fullscreen
        document.onfullscreenchange = (event) => {
            if (document.fullscreenElement) {
                this.screenType = "close_fullscreen";
            }
            else {
                this.screenType = "fullscreen";
            }
        };
        this.gitservice.getBranchList().subscribe(branches => {
            this.branches = branches;
        });
        // something is selected from treeview/sidenav
        this.mdFileService.selectedMdFileFromSideNav.subscribe(_ => {
            if (_ != null) {
                this.currentMdFile = _;
                this.mdFileService.navigationArray = [];
                this.absolutePath = _.fullPath;
                this.relativePath = _.relativePath;
            }
        });
        // something has changed on filesystem
        this.subscriptionserverSelectedMdFile = this.mdFileService.serverSelectedMdFile.subscribe(val => {
            var current = val[0];
            if (current != undefined) {
                let index = this.mdFileService.navigationArray.length;
                if (index > 0) {
                    //if (current.fullPath == this.mdFileService.navigationArray[index - 1].fullPath) {
                    if (current == this.mdFileService.navigationArray[index - 1]) {
                        //return;
                    }
                }
                this.navService.setNewNavigation(current);
                this.absolutePath = current.fullPath;
                this.relativePath = current.relativePath;
                this.currentMdFile = current;
            }
        });
    }
    ngOnDestroy() {
        console.log("ngOnDestroy toolbar");
        this.subscriptionserverSelectedMdFile.unsubscribe();
    }
    toggleSidenav() {
        let test = !this.appSettings.showSidenav.value;
        this.appSettings.showSidenav.next(test);
    }
    openRules(data) {
        const dialogRef = this.dialog.open(_signalR_dialogs_rules_rules_component__WEBPACK_IMPORTED_MODULE_1__["RulesComponent"], {
            width: '600px',
            data: data
        });
        dialogRef.afterClosed().subscribe(_ => {
            if (_.refactoringSourceActionId != undefined) {
                this.dialog.open(_refactoring_rename_file_rename_file_component__WEBPACK_IMPORTED_MODULE_0__["RenameFileComponent"], {
                    width: '600px',
                    data: _
                });
            }
        });
    }
    checkConnection() {
        this.isCheckingConnection = true;
        this.gitservice.getCurrentBranch();
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
        objectThis.currentMdFile = data;
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
    FullScreenToggle() {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        }
        else {
            document.exitFullscreen();
        }
    }
    pull() {
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_5__["WaitingDialogInfo"]();
        info.message = "Please wait... Pulling branch";
        this.waitingDialogService.showMessageBox(info);
        let pullInfo = new _git_models_pullInfo__WEBPACK_IMPORTED_MODULE_7__["PullInfo"]();
        this.gitservice.pull(pullInfo).subscribe(_ => {
            // manage failed connection
            if (_.isConnectionMissing) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Missing connection',
                        description: 'Please verify your vpn or network settings'
                    }
                });
            }
            if (_.isAuthenticationMissing) {
                const dialogRef = this.dialog.open(_git_components_git_auth_git_auth_component__WEBPACK_IMPORTED_MODULE_6__["GitAuthComponent"], {
                    width: '300px',
                });
            }
            if (_.thereAreConflicts) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Conflicts appear',
                        description: _.errorMessage
                    }
                });
            }
            if (!_.isAuthenticationMissing && !_.isConnectionMissing && !_.thereAreConflicts) {
                this.mdFileService.loadAll(null, null);
                this.mdFileService.getLandingPage().subscribe(node => {
                    if (node != null) {
                        this.checkConnection();
                        this.mdFileService.setSelectedMdFileFromSideNav(node);
                    }
                });
            }
            this.waitingDialogService.closeMessageBox();
            this.matMenuTrigger.closeMenu();
        });
    }
    commit() {
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_5__["WaitingDialogInfo"]();
        info.message = "Please wait... commit and pushing branch";
        this.waitingDialogService.showMessageBox(info);
        let pullInfo = new _git_models_pullInfo__WEBPACK_IMPORTED_MODULE_7__["PullInfo"]();
        this.gitservice.commit(pullInfo).subscribe(_ => {
            if (_.isConnectionMissing) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Missing connection',
                        description: 'Please verify your vpn or network settings'
                    }
                });
            }
            if (_.isAuthenticationMissing) {
                const dialogRef = this.dialog.open(_git_components_git_auth_git_auth_component__WEBPACK_IMPORTED_MODULE_6__["GitAuthComponent"], {
                    width: '300px',
                });
            }
            if (_.thereAreConflicts) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Conflicts appear',
                        description: _.errorMessage,
                    }
                });
            }
            this.checkConnection();
            this.waitingDialogService.closeMessageBox();
            this.matMenuTrigger.closeMenu();
        });
    }
    push() {
        let info = new _commons_waitingdialog_waiting_dialog_models_WaitingDialogInfo__WEBPACK_IMPORTED_MODULE_5__["WaitingDialogInfo"]();
        info.message = "Please wait... commit and pushing branch";
        this.waitingDialogService.showMessageBox(info);
        let pullInfo = new _git_models_pullInfo__WEBPACK_IMPORTED_MODULE_7__["PullInfo"]();
        this.gitservice.push(pullInfo).subscribe(_ => {
            if (_.isConnectionMissing) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Missing connection',
                        description: 'Please verify your vpn or network settings'
                    }
                });
            }
            if (_.isAuthenticationMissing) {
                const dialogRef = this.dialog.open(_git_components_git_auth_git_auth_component__WEBPACK_IMPORTED_MODULE_6__["GitAuthComponent"], {
                    width: '300px',
                });
            }
            if (_.thereAreConflicts) {
                const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_8__["GitMessagesComponent"], {
                    width: '300px',
                    data: {
                        message: 'Conflicts appear',
                        description: _.errorMessage,
                    }
                });
            }
            this.checkConnection();
            this.waitingDialogService.closeMessageBox();
            this.matMenuTrigger.closeMenu();
        });
    }
    openBranch(branch) {
        this.gitservice.checkoutSelectedBranch(branch).subscribe(_ => {
            this.currentBranch = _.name;
            var mdFile = new _models_md_file__WEBPACK_IMPORTED_MODULE_2__["MdFile"]("Welcome to MDExplorer", '/../welcome.html', 0, false);
            mdFile.relativePath = '/../../welcome.html';
            this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
            this.projectService.setNewFolderProject(_.fullPath);
        });
        this.matMenuTrigger.closeMenu();
    }
    bookmarkToggle() {
        let bookmark = new _services_Types_Bookmark__WEBPACK_IMPORTED_MODULE_9__["Bookmark"](this.currentMdFile);
        bookmark.projectId = this.projectService.currentProjects$.value.id;
        this.bookmarksService.toggleBookmark(bookmark);
    }
}
ToolbarComponent.ɵfac = function ToolbarComponent_Factory(t) { return new (t || ToolbarComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_11__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_13__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_14__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_15__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_16__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_17__["AppCurrentMetadataService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_projects_service__WEBPACK_IMPORTED_MODULE_18__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_19__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_20__["WaitingDialogService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_bookmarks_service__WEBPACK_IMPORTED_MODULE_21__["BookmarksService"]), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_md_navigation_service__WEBPACK_IMPORTED_MODULE_22__["MdNavigationService"])); };
ToolbarComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineComponent"]({ type: ToolbarComponent, selectors: [["app-toolbar"]], viewQuery: function ToolbarComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuTrigger"], 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵviewQuery"](_angular_material_tabs__WEBPACK_IMPORTED_MODULE_4__["MatTabGroup"], 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵloadQuery"]()) && (ctx.tabGroup = _t.first);
    } }, decls: 47, vars: 5, consts: [["color", "primary"], ["mat-icon-button", "", "matTooltip", "hide/show menu", 3, "click"], ["mat-icon-button", "", "matTooltip", "edit document", 3, "click"], ["mat-icon-button", "", "matTooltip", "export in word", 3, "click"], ["mat-icon-button", "", "matTooltip", "full screen", 3, "click"], ["mat-icon-button", "", "matTooltip", "Bookmarks", 3, "click"], [1, "flexExpand"], ["mat-icon-button", "", "matTooltip", "missing connection to remote", 3, "click", 4, "ngIf"], [4, "ngIf"], ["style", "display:flex; flex-direction:row", 4, "ngIf"], ["tagsAndBranches", "matMenu"], ["color", "warn", "mat-menu-item", "", 1, "container", 3, "click"], ["mat-icon-button", ""], ["color", "warn", 2, "text-align", "right"], ["mat-icon-button", "", "matTooltip", "missing connection to remote", 3, "click"], [2, "color", "yellow"], ["src", "/assets/connecting.gif"], [2, "display", "flex", "flex-direction", "row"], [2, "display", "flex", "flex-direction", "column"], ["class", "smallMessagesToCommit", 3, "click", 4, "ngIf"], ["class", "smallMessagesToPull", 3, "click", 4, "ngIf"], ["mat-button", "", 3, "matMenuTriggerFor", "ngClass"], ["src", "/assets/gitlogo.png"], [1, "smallMessagesToCommit", 3, "click"], [1, "smallMessagesToPull", 3, "click"]], template: function ToolbarComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-toolbar", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "mat-toolbar-row");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "button", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_2_listener() { return ctx.toggleSidenav(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4, "vertical_split");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "button", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_5_listener() { return ctx.OpenEditor(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_8_listener() { return ctx.Export(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10, "directions");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](11, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_11_listener() { return ctx.FullScreenToggle(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](13);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](14, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_button_click_14_listener() { return ctx.bookmarkToggle(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](15, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](16, "bookmark");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](17, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](18);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](19, "span", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](20, ToolbarComponent_button_20_Template, 3, 0, "button", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](21, ToolbarComponent_span_21_Template, 2, 0, "span", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](22, ToolbarComponent_span_22_Template, 7, 5, "span", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](23, "mat-menu", null, 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](25, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](26, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_div_click_26_listener() { return ctx.pull(); })("click", function ToolbarComponent_Template_div_click_26_listener($event) { return $event.stopPropagation(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](27, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](28, "Pull");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](29, "span", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](30, "button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](31, "mat-icon", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](32, "arrow_downward");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](33, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_div_click_33_listener() { return ctx.commit(); })("click", function ToolbarComponent_Template_div_click_33_listener($event) { return $event.stopPropagation(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](34, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](35, "Commit");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](36, "span", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](37, "button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](38, "mat-icon", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](39, "arrow_upward");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](40, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ToolbarComponent_Template_div_click_40_listener() { return ctx.push(); })("click", function ToolbarComponent_Template_div_click_40_listener($event) { return $event.stopPropagation(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](41, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](42, "Push");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](43, "span", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](44, "button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](45, "mat-icon", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](46, "arrow_upward");
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](13);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx.screenType);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx.currentMdFile == null ? null : ctx.currentMdFile.name);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.connectionIsActive);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.isCheckingConnection);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.currentBranch != null);
    } }, directives: [_angular_material_toolbar__WEBPACK_IMPORTED_MODULE_23__["MatToolbar"], _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_23__["MatToolbarRow"], _angular_material_button__WEBPACK_IMPORTED_MODULE_24__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_25__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_26__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_27__["NgIf"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuItem"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_3__["MatMenuTrigger"], _angular_common__WEBPACK_IMPORTED_MODULE_27__["NgClass"]], styles: [".branchGood[_ngcontent-%COMP%] {\n  color: greenyellow;\n}\n\n.branchToCommit[_ngcontent-%COMP%] {\n  color: white;\n}\n\n.sidenav-toggle[_ngcontent-%COMP%] {\n  display: none;\n  padding: 0;\n  margin: 8px;\n  min-width: after 56px;\n}\n\n@media (max-width: 720px) {\n  .sidenav-toggle[_ngcontent-%COMP%] {\n    display: flex;\n    align: center;\n    justify-content: center;\n  }\n}\n\n.sidenav-toggle[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 30px;\n  height: after 56px;\n  width: after 56px;\n  line-height: after 56px;\n  color: white;\n}\n\n.container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n\n.flexExpand[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n}\n\n#projectcss[_ngcontent-%COMP%]    .mat-tooltip {\n  \n  \n  color: white;\n  background-color: blue;\n}\n\n#toPull[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  background-color: white;\n  color: black;\n  top: 30px;\n}\n\n.smallMessagesToCommit[_ngcontent-%COMP%] {\n  cursor: pointer;\n  font-size: 12px;\n  height: 20px;\n  background-color: deeppink;\n  color: white;\n}\n\n.smallMessagesToPull[_ngcontent-%COMP%] {\n  cursor: pointer;\n  font-size: 12px;\n  height: 20px;\n  background-color: white;\n  color: black;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcdG9vbGJhci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFFQTtFQUNFLGtCQUFBO0FBREY7O0FBSUE7RUFDRSxZQUFBO0FBREY7O0FBSUE7RUFDRSxhQUFBO0VBQ0EsVUFBQTtFQUNBLFdBQUE7RUFDQSxxQkFkVTtBQWFaOztBQUdFO0VBTkY7SUFPSSxhQUFBO0lBQ0EsYUFBQTtJQUNBLHVCQUFBO0VBQUY7QUFDRjs7QUFFRTtFQUNFLGVBQUE7RUFDQSxrQkF4QlE7RUF5QlIsaUJBekJRO0VBMEJSLHVCQTFCUTtFQTJCUixZQUFBO0FBQUo7O0FBSUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7QUFERjs7QUFJQTtFQUNFLGNBQUE7QUFERjs7QUFNRTtFQUNFLGdDQUFBO0VBQ0EsU0FBQTtFQUNBLFlBQUE7RUFDQSxzQkFBQTtBQUhKOztBQU9BO0VBQ0UsdUJBQUE7RUFDQSxZQUFBO0VBQ0EsU0FBQTtBQUpGOztBQU9BO0VBQ0UsZUFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsMEJBQUE7RUFDQSxZQUFBO0FBSkY7O0FBT0E7RUFDRSxlQUFBO0VBQ0EsZUFBQTtFQUNBLFlBQUE7RUFDQSx1QkFBQTtFQUNBLFlBQUE7QUFKRiIsImZpbGUiOiJ0b29sYmFyLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiJGljb25XaWR0aDogYWZ0ZXIgNTZweDtcclxuXHJcbi5icmFuY2hHb29kIHtcclxuICBjb2xvcjogZ3JlZW55ZWxsb3c7XHJcbn1cclxuXHJcbi5icmFuY2hUb0NvbW1pdCB7XHJcbiAgY29sb3I6IHdoaXRlXHJcbn1cclxuXHJcbi5zaWRlbmF2LXRvZ2dsZSB7XHJcbiAgZGlzcGxheTogbm9uZTtcclxuICBwYWRkaW5nOiAwO1xyXG4gIG1hcmdpbjogOHB4O1xyXG4gIG1pbi13aWR0aDogJGljb25XaWR0aDtcclxuXHJcbiAgQG1lZGlhIChtYXgtd2lkdGg6NzIwcHgpIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbjogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgfVxyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDMwcHg7XHJcbiAgICBoZWlnaHQ6ICRpY29uV2lkdGg7XHJcbiAgICB3aWR0aDogJGljb25XaWR0aDtcclxuICAgIGxpbmUtaGVpZ2h0OiAkaWNvbldpZHRoO1xyXG4gICAgY29sb3I6IHdoaXRlO1xyXG4gIH1cclxufVxyXG5cclxuLmNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogcm93O1xyXG59XHJcblxyXG4uZmxleEV4cGFuZCB7XHJcbiAgZmxleDogMSAxIGF1dG87XHJcbn1cclxuXHJcblxyXG4jcHJvamVjdGNzcyB7XHJcbiAgOjpuZy1kZWVwLm1hdC10b29sdGlwIHtcclxuICAgIC8qIHlvdXIgb3duIGN1c3RvbSBzdHlsZXMgaGVyZSAqL1xyXG4gICAgLyogZS5nLiAqL1xyXG4gICAgY29sb3I6IHdoaXRlO1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogYmx1ZTtcclxuICB9XHJcbn1cclxuXHJcbiN0b1B1bGwgc3BhbiB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7XHJcbiAgY29sb3I6IGJsYWNrO1xyXG4gIHRvcDogMzBweDtcclxufVxyXG5cclxuLnNtYWxsTWVzc2FnZXNUb0NvbW1pdCB7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIGZvbnQtc2l6ZTogMTJweDtcclxuICBoZWlnaHQ6IDIwcHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogZGVlcHBpbms7XHJcbiAgY29sb3I6IHdoaXRlO1xyXG59XHJcblxyXG4uc21hbGxNZXNzYWdlc1RvUHVsbCB7XHJcbiAgY3Vyc29yOiBwb2ludGVyOyAgXHJcbiAgZm9udC1zaXplOiAxMnB4O1xyXG4gIGhlaWdodDogMjBweDsgIFxyXG4gIGJhY2tncm91bmQtY29sb3I6IHdoaXRlO1xyXG4gIGNvbG9yOiBibGFjaztcclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "N7Dz":
/*!***************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/move-md-file/move-md-file.component.ts ***!
  \***************************************************************************************/
/*! exports provided: MoveMdFileComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MoveMdFileComponent", function() { return MoveMdFileComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../../commons/components/show-file-system/show-file-system.component */ "yrD1");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");












class MoveMdFileComponent {
    constructor(dataMdFile, dialog, dialogRef, mdFileService, mdServerMessages) {
        this.dataMdFile = dataMdFile;
        this.dialog = dialog;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
        this.mdServerMessages = mdServerMessages;
    }
    ngOnInit() { }
    openFileSystem() {
        const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__["ShowFileSystemComponent"], {
            width: '600px',
            height: '600px',
            data: 'project'
        });
        dialogRef.afterClosed().subscribe(_ => {
            this.directoryDestination = _.data;
        });
    }
    move() {
        this.mdFileService.moveMdFile(this.dataMdFile, this.directoryDestination)
            .subscribe(_ => {
            this.mdFileService.loadAll(null, null);
            this.dialogRef.close();
        });
    }
    dismiss() {
        this.dialogRef.close();
    }
}
MoveMdFileComponent.ɵfac = function MoveMdFileComponent_Factory(t) { return new (t || MoveMdFileComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_3__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__["MdServerMessagesService"])); };
MoveMdFileComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: MoveMdFileComponent, selectors: [["app-move-md-file"]], decls: 22, vars: 1, consts: [[1, "orizzontal-form-container"], ["appearance", "outline", 2, "width", "100%"], ["matInput", "", "placeholder", "local filesystem", "required", "", 3, "ngModel", "ngModelChange"], ["mat-button", "", "matSuffix", "", "color", "primary", 3, "click"], ["mat-button", "", "color", "primary", 3, "click"]], template: function MoveMdFileComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "Move Markdown File");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](6, "Directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function MoveMdFileComponent_Template_input_ngModelChange_7_listener($event) { return ctx.directoryDestination = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9, "Set destination directory");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function MoveMdFileComponent_Template_button_click_10_listener() { return ctx.openFileSystem(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](12, "more_horiz");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](13, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](14, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function MoveMdFileComponent_Template_button_click_14_listener() { return ctx.move(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](15, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](16, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](17, "move! ");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](18, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function MoveMdFileComponent_Template_button_click_18_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](19, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](20, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](21, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.directoryDestination);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_7__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatHint"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatSuffix"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__["MatIcon"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJtb3ZlLW1kLWZpbGUuY29tcG9uZW50LnNjc3MifQ== */"] });


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

/***/ "WD3D":
/*!*****************************************************************************************************!*\
  !*** ./src/app/md-explorer/components/dialogs/copy-from-clipboard/copy-from-clipboard.component.ts ***!
  \*****************************************************************************************************/
/*! exports provided: CopyFromClipboardComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CopyFromClipboardComponent", function() { return CopyFromClipboardComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../models/md-file */ "aS6m");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");











class CopyFromClipboardComponent {
    constructor(data, dialogRef, mdFileService, snackBar) {
        this.data = data;
        this.dialogRef = dialogRef;
        this.mdFileService = mdFileService;
        this.snackBar = snackBar;
    }
    ngOnInit() {
    }
    dismiss() {
        this.dialogRef.close();
    }
    save() {
        let dataToSend = { fileName: this.imageName, fileInfoNode: this.data };
        this.mdFileService.pasteFromClipboard(dataToSend).subscribe(data => {
            this.snackBar.open("Copied", null, { duration: 5000 });
        });
        this.dialogRef.close();
    }
}
CopyFromClipboardComponent.ɵfac = function CopyFromClipboardComponent_Factory(t) { return new (t || CopyFromClipboardComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__["MatSnackBar"])); };
CopyFromClipboardComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: CopyFromClipboardComponent, selectors: [["app-copy-from-clipboard"]], decls: 19, vars: 1, consts: [[1, "simple-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "imageName", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-button", "", "color", "primary", 3, "click"]], template: function CopyFromClipboardComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Copy from clipboard");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Image name");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "input", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function CopyFromClipboardComponent_Template_input_ngModelChange_7_listener($event) { return ctx.imageName = $event; })("keyup.enter", function CopyFromClipboardComponent_Template_input_keyup_enter_7_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Name of Image");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CopyFromClipboardComponent_Template_button_click_11_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](14, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CopyFromClipboardComponent_Template_button_click_15_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.imageName);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_6__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__["MatIcon"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJjb3B5LWZyb20tY2xpcGJvYXJkLmNvbXBvbmVudC5zY3NzIn0= */"] });


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

/***/ "c/kW":
/*!***********************************************************!*\
  !*** ./src/app/md-explorer/services/bookmarks.service.ts ***!
  \***********************************************************/
/*! exports provided: BookmarksService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BookmarksService", function() { return BookmarksService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../models/md-file */ "aS6m");
/* harmony import */ var _Types_Bookmark__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./Types/Bookmark */ "/q41");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_file_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./md-file.service */ "xmhS");







class BookmarksService {
    constructor(http, mdFileService) {
        this.http = http;
        this.mdFileService = mdFileService;
        this.bookmarks$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]([]);
    }
    initBookmark(projectId) {
        // passing projectId
        let books = [];
        const url = '../api/mdFiles/GetBookmarks';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_3__["HttpParams"]().set('projectId', projectId);
        let bookmarks$$ = this.http.get(url, { params });
        let bookmarksWaitingAllMdFiles$ = Object(rxjs__WEBPACK_IMPORTED_MODULE_0__["combineLatest"])([this.mdFileService._mdFiles, bookmarks$$]);
        bookmarksWaitingAllMdFiles$.subscribe(([mdFiles, bookmarks]) => {
            let mdcheck = mdFiles;
            bookmarks.forEach(_ => {
                // devo ricostruirmi la lista di mdFile
                let mdFileToSearch = new _models_md_file__WEBPACK_IMPORTED_MODULE_1__["MdFile"](_.name, _.fullPath, null, null);
                let mdfile = this.mdFileService.getMdFileFromDataStore(mdFileToSearch);
                let bookmark = new _Types_Bookmark__WEBPACK_IMPORTED_MODULE_2__["Bookmark"](mdFileToSearch);
                bookmark.fullPath = _.fullPath;
                books.push(bookmark);
            });
            this.bookmarks$.next(books);
        });
    }
    toggleBookmark(bookmark) {
        const url = '../api/mdFiles/ToggleBookmark';
        let post$ = this.http.post(url, bookmark);
        post$.subscribe(_ => {
            //this.bookmarks$.next(_);
        });
        let currentBookmarks = this.bookmarks$.value;
        let currentBookmark = currentBookmarks.find(_ => _.fullPath === bookmark.fullPath);
        if (currentBookmark == null || currentBookmark == undefined) {
            // call api
            currentBookmarks.push(bookmark);
            this.bookmarks$.next(currentBookmarks);
        }
        else {
            // call api
            let currentBookmarkIndex = currentBookmarks.indexOf(currentBookmark);
            currentBookmarks.splice(currentBookmarkIndex, 1);
        }
    }
}
BookmarksService.ɵfac = function BookmarksService_Factory(t) { return new (t || BookmarksService)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__["HttpClient"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵinject"](_md_file_service__WEBPACK_IMPORTED_MODULE_5__["MdFileService"])); };
BookmarksService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineInjectable"]({ token: BookmarksService, factory: BookmarksService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "euKz":
/*!****************************************!*\
  !*** ./src/app/git/models/pullInfo.ts ***!
  \****************************************/
/*! exports provided: PullInfo */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "PullInfo", function() { return PullInfo; });
class PullInfo {
}


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
        console.log("back index: " + this.currentIndex);
        console.log("back/navigation: " + this.navigation.length);
        console.log("back/navigationGhost: " + this.navigationGhost.length);
        return currentMdFile;
    }
    forward() {
        this.currentIndex = this.currentIndex + 1;
        let currentMdFile = this.navigation[this.currentIndex];
        this.navigationGhost.push(currentMdFile);
        console.log("forward index: " + this.currentIndex);
        console.log("Forward/navigation: " + this.navigation.length);
        console.log("Forward/navigationGhost: " + this.navigationGhost.length);
        return currentMdFile;
    }
    resetNavigation() {
        this.navigationGhost = [];
        this.navigation = [];
    }
    setNewNavigation(currentMdFile) {
        console.log("this.navigationGhost.length: " + this.navigationGhost.length);
        console.log("currentMdFile.fullPath: " + currentMdFile.fullPath);
        if (this.navigationGhost.length >= 1) {
            console.log(" this.navigationGhost[this.navigationGhost.length - 1].fullPath" + this.navigationGhost[this.navigationGhost.length - 1].fullPath);
            console.log("check: " + (currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath));
        }
        if ((this.navigationGhost.length - 1) >= 0 //check its not at beginning of navigation
            && currentMdFile.fullPath == this.navigationGhost[this.navigationGhost.length - 1].fullPath) {
            console.log("RETURN;");
            return; //DO NOTHING
        }
        this.navigationGhost.push(currentMdFile);
        this.navigation = this.deepCopyArray(this.navigationGhost);
        this.currentIndex = this.navigationGhost.length - 1; // index i 0 based, length is 1 based
        console.log("setNewNavigation/navigation: " + this.navigation.length);
        console.log("setNewNavigation/navigationGhost: " + this.navigationGhost.length);
    }
    deepCopyArray(array) {
        return JSON.parse(JSON.stringify(array));
    }
}
MdNavigationService.ɵfac = function MdNavigationService_Factory(t) { return new (t || MdNavigationService)(); };
MdNavigationService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({ token: MdNavigationService, factory: MdNavigationService.ɵfac, providedIn: 'root' });


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
/* harmony import */ var _models_md_file__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../models/md-file */ "aS6m");
/* harmony import */ var _services_Types_Bookmark__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/Types/Bookmark */ "/q41");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_cdk_layout__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/cdk/layout */ "0MNC");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../services/app-current-metadata.service */ "QI1B");
/* harmony import */ var _services_bookmarks_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../services/bookmarks.service */ "c/kW");
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ "N73s");
/* harmony import */ var _services_projects_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../services/projects.service */ "vUCT");
/* harmony import */ var _services_md_navigation_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../../services/md-navigation.service */ "hFUY");
/* harmony import */ var _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/sidenav */ "XhcP");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ../md-tree/md-tree.component */ "s8RO");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _publish_md_tree_publish_md_tree_component__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ../publish-md-tree/publish-md-tree.component */ "5hcZ");




















const _c0 = ["sidenav"];
function SidenavComponent_ng_template_19_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "img", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, " Project docs ");
} }
function SidenavComponent_mat_tab_21_ng_template_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "img", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "CI/CD ");
} }
function SidenavComponent_mat_tab_21_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](1, SidenavComponent_mat_tab_21_ng_template_1_Template, 2, 0, "ng-template", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](2, "app-publish-md-tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function SidenavComponent_div_25_span_2_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function SidenavComponent_div_25_span_2_Template_span_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r12); const bookmark_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r10.openDocument(bookmark_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const bookmark_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](bookmark_r5.name);
} }
function SidenavComponent_div_25_span_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const indexOfElement_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().index;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](indexOfElement_r6);
} }
function SidenavComponent_div_25_mat_icon_4_Template(rf, ctx) { if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-icon", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function SidenavComponent_div_25_mat_icon_4_Template_mat_icon_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r17); const bookmark_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r15.toggleBookmark(bookmark_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "cancel");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function SidenavComponent_div_25_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("mouseenter", function SidenavComponent_div_25_Template_div_mouseenter_0_listener() { const bookmark_r5 = ctx.$implicit; return bookmark_r5.show = true; })("mouseleave", function SidenavComponent_div_25_Template_div_mouseleave_0_listener() { const bookmark_r5 = ctx.$implicit; return bookmark_r5.show = false; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](2, SidenavComponent_div_25_span_2_Template, 2, 1, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, SidenavComponent_div_25_span_3_Template, 2, 1, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, SidenavComponent_div_25_mat_icon_4_Template, 2, 0, "mat-icon", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](6, "arrow_forward_ios");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const bookmark_r5 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵattribute"]("alt", bookmark_r5.fullPath);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", bookmark_r5.show);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !bookmark_r5.show);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", bookmark_r5.show);
} }
const SMALL_WIDTH_BREAKPOINT = 720;
class SidenavComponent {
    constructor(breakpointObserver, mdFileService, router, currentFolder, bookmarksService, gitService, projectService, navService) {
        this.breakpointObserver = breakpointObserver;
        this.mdFileService = mdFileService;
        this.router = router;
        this.currentFolder = currentFolder;
        this.bookmarksService = bookmarksService;
        this.gitService = gitService;
        this.projectService = projectService;
        this.navService = navService;
        this.showText = false;
        this.sideNavWidth = "240px";
        this.hooked = false;
        this.classForBorderDiv = "border-div";
        this.currentBranch = null;
        document.addEventListener("mousemove", (event) => {
            if (this.hooked) {
                this.sideNavWidth = event.clientX + "px";
            }
        });
        document.addEventListener("mouseup", (event) => {
            console.log(this.hooked);
            if (this.hooked) {
                //this.stopResizeWidth();
                this.hooked = false;
                this.classForBorderDiv = "border-div";
                this.projectService.currentProjects$.value.sidenavWidth = event.clientX;
                this.projectService.SetSideNavWidth(this.projectService.currentProjects$.value);
            }
        });
        this.currentFolder.folderName.subscribe((data) => {
            this.titleProject = data.currentFolder;
        });
        this.currentFolder.loadFolderName();
        this.gitService.getCurrentBranch().subscribe(_ => {
            this.currentBranch = _.name;
        });
        this.currentFolder.showSidenav.subscribe(_ => {
            if (this.sidenav != undefined) {
                if (_) {
                    this.sidenav.open();
                }
                else {
                    this.sidenav.close();
                }
            }
        });
    }
    resizeWidth() {
        this.hooked = true;
        this.classForBorderDiv = "border-div-moving";
    }
    stopResizeWidth() {
    }
    openProject() {
        var mdFile = new _models_md_file__WEBPACK_IMPORTED_MODULE_0__["MdFile"]("Welcome to MDExplorer", '/../welcome.html', 0, false);
        mdFile.relativePath = '/../../welcome.html';
        this.mdFileService.setSelectedMdFileFromSideNav(mdFile);
        this.router.navigate(['/projects']);
        this.projectService.currentProjects$.next(null);
    }
    ngOnInit() {
        this.breakpointObserver.observe([`(max-width:${SMALL_WIDTH_BREAKPOINT}px)`])
            .subscribe((state) => {
            this.isScreenSmall = state.matches;
        });
        this.bookmarksService.bookmarks$.subscribe(_ => this.bookmarks = _);
        this.projectService.currentProjects$.subscribe(_ => {
            if (_ != null && _ != undefined) {
                this.bookmarksService.initBookmark(_.id);
                if (_.sidenavWidth != null && _.sidenavWidth != 0) {
                    this.sideNavWidth = _.sidenavWidth + "px";
                }
            }
        });
    }
    openDocument(bookmark) {
        let mdfile = this.mdFileService.getMdFileFromDataStore(bookmark);
        this.router.navigate(['/main/navigation/document']);
        this.mdFileService.setSelectedMdFileFromSideNav(mdfile);
    }
    toggleBookmark(mdFile) {
        let bookmark = new _services_Types_Bookmark__WEBPACK_IMPORTED_MODULE_1__["Bookmark"](mdFile);
        bookmark.projectId = this.projectService.currentProjects$.value.id;
        this.bookmarksService.toggleBookmark(bookmark);
    }
    forward() {
        let navToMdFile = this.navService.forward();
        this.router.navigate(['/main/navigation/document']);
        this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
    }
    backward() {
        let navToMdFile = this.navService.back();
        this.router.navigate(['/main/navigation/document']);
        this.mdFileService.setSelectedMdFileFromSideNav(navToMdFile);
    }
}
SidenavComponent.ɵfac = function SidenavComponent_Factory(t) { return new (t || SidenavComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_cdk_layout__WEBPACK_IMPORTED_MODULE_3__["BreakpointObserver"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_5__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_6__["AppCurrentMetadataService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_bookmarks_service__WEBPACK_IMPORTED_MODULE_7__["BookmarksService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_8__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_projects_service__WEBPACK_IMPORTED_MODULE_9__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_md_navigation_service__WEBPACK_IMPORTED_MODULE_10__["MdNavigationService"])); };
SidenavComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: SidenavComponent, selectors: [["app-sidenav"]], viewQuery: function SidenavComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵloadQuery"]()) && (ctx.sidenav = _t.first);
    } }, decls: 29, vars: 10, consts: [["autosize", "", 1, "app-sidenav-container"], [1, "mat-elevation-z10", 3, "opened", "mode"], ["sidenav", ""], ["color", "primary"], ["mat-icon-button", "", "id", "projectcss", "matTooltip", "projects list", 3, "click"], [1, "flexExpand"], ["mat-icon-button", "", 3, "disabled", "click"], ["headerPosition", "below", 2, "justify-content", "space-between", "height", "calc(100% - 64px)"], [2, "height", "100%"], ["mat-tab-label", ""], [4, "ngIf"], [1, "flex-container"], [3, "ngClass", "pointerdown", "mouseup"], [2, "position", "fixed", "display", "flex", "flex-direction", "column", "align-items", "flex-start", "background-color", "transparent", "top", "66px"], ["class", "mdbookmark", "style", "padding:2px;", 3, "mouseenter", "mouseleave", 4, "ngFor", "ngForOf"], [2, "width", "100%", "height", "100%"], [1, "app-sidenav-content", 2, "height", "100%"], ["src", "/assets/project.png", 2, "height", "24px"], ["src", "/assets/cicd.png", 2, "height", "24px"], [1, "mdbookmark", 2, "padding", "2px", 3, "mouseenter", "mouseleave"], [2, "border-color", "black"], [3, "click", 4, "ngIf"], ["style", "vertical-align:bottom;", 3, "click", 4, "ngIf"], [2, "vertical-align", "bottom"], [3, "click"], [2, "vertical-align", "bottom", 3, "click"]], template: function SidenavComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-sidenav-container", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-sidenav", 1, 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-toolbar", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function SidenavComponent_Template_button_click_5_listener() { return ctx.openProject(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7, "ballot");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](10, "span", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "button", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function SidenavComponent_Template_button_click_11_listener() { return ctx.backward(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](13, "arrow_back");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](14, "button", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function SidenavComponent_Template_button_click_14_listener() { return ctx.forward(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](15, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](16, "arrow_forward");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](17, "mat-tab-group", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](18, "mat-tab", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](19, SidenavComponent_ng_template_19_Template, 2, 0, "ng-template", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](20, "app-md-tree");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](21, SidenavComponent_mat_tab_21_Template, 3, 0, "mat-tab", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](22, "div", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](23, "div", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("pointerdown", function SidenavComponent_Template_div_pointerdown_23_listener() { return ctx.resizeWidth(); })("mouseup", function SidenavComponent_Template_div_mouseup_23_listener() { return ctx.stopResizeWidth(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](25, SidenavComponent_div_25_Template, 7, 4, "div", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](26, "div", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](27, "div", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](28, "router-outlet");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵstyleProp"]("width", ctx.sideNavWidth);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("opened", !ctx.isScreenSmall)("mode", !ctx.isScreenSmall ? "side" : "over");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.titleProject);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.navService.navigationGhost.length <= 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.navService.navigation.length == ctx.navService.navigationGhost.length);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.currentBranch != null);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngClass", ctx.classForBorderDiv);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.bookmarks);
    } }, directives: [_angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__["MatSidenavContainer"], _angular_material_sidenav__WEBPACK_IMPORTED_MODULE_11__["MatSidenav"], _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_12__["MatToolbar"], _angular_material_button__WEBPACK_IMPORTED_MODULE_13__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_14__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_15__["MatIcon"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_16__["MatTabGroup"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_16__["MatTab"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_16__["MatTabLabel"], _md_tree_md_tree_component__WEBPACK_IMPORTED_MODULE_17__["MdTreeComponent"], _angular_common__WEBPACK_IMPORTED_MODULE_18__["NgIf"], _angular_common__WEBPACK_IMPORTED_MODULE_18__["NgClass"], _angular_common__WEBPACK_IMPORTED_MODULE_18__["NgForOf"], _angular_router__WEBPACK_IMPORTED_MODULE_5__["RouterOutlet"], _publish_md_tree_publish_md_tree_component__WEBPACK_IMPORTED_MODULE_19__["PublishMdTreeComponent"]], styles: [".app-sidenav-container[_ngcontent-%COMP%] {\n  flex: 1;\n  position: fixed;\n  width: 100%;\n  min-width: 100%;\n  height: 100%;\n  min-height: 100%;\n  -webkit-user-select: none;\n          user-select: none;\n}\n\n.app-sidenav-content[_ngcontent-%COMP%] {\n  display: flex;\n  height: 100%;\n  width: auto;\n  flex-direction: column;\n}\n\n.app-sidenav[_ngcontent-%COMP%] {\n  width: 240px;\n}\n\n.flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  width: 100%;\n  height: 100%;\n  min-width: 100%;\n  min-height: 100%;\n}\n\n.border-div[_ngcontent-%COMP%] {\n  border-left: 10px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.border-div-moving[_ngcontent-%COMP%] {\n  border-left: 50px;\n  border-left-style: solid;\n  border-left-color: grey;\n  cursor: w-resize;\n}\n\n.showStartToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  display: contents;\n}\n\n.showToolbar[_ngcontent-%COMP%] {\n  height: 64px;\n  transition: height 300ms ease-out;\n}\n\n\n\n .mat-tab-label,  .mat-tab-label-active {\n  min-width: 0 !important;\n  padding: 3px !important;\n  margin: 3px !important;\n}\n\n#projectcss[_ngcontent-%COMP%]    .mat-tooltip {\n  \n  \n  color: white;\n  background-color: blue;\n}\n\ndiv.mdbookmark[_ngcontent-%COMP%] {\n  cursor: pointer;\n  white-space: nowrap;\n  text-align: right;\n  background: yellow;\n  transition: all 0.5s ease;\n}\n\ndiv.inner[_ngcontent-%COMP%] {\n  height: 50px;\n  width: 50px;\n  background: red;\n  opacity: 0;\n  visibility: hidden;\n  transition: all 0.5s ease;\n}\n\ndiv.outer[_ngcontent-%COMP%]:hover   .inner[_ngcontent-%COMP%] {\n  visibility: initial;\n  opacity: 1;\n}\n\n.flexExpand[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcc2lkZW5hdi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLE9BQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7RUFDQSx5QkFBQTtVQUFBLGlCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUVBO0VBQ0UsWUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGlCQUFBO0VBQ0Esd0JBQUE7RUFDQSx1QkFBQTtFQUNBLGdCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxpQkFBQTtFQUNBLHdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxnQkFBQTtBQUNGOztBQUVBO0VBQ0ksV0FBQTtFQUNBLGlCQUFBO0FBQ0o7O0FBR0E7RUFDRSxZQUFBO0VBQ0EsaUNBQUE7QUFBRjs7QUFHQTs7RUFBQTs7QUFPQTtFQUNFLHVCQUFBO0VBQ0EsdUJBQUE7RUFDQSxzQkFBQTtBQUpGOztBQVFFO0VBQ0UsZ0NBQUE7RUFDQSxTQUFBO0VBQ0EsWUFBQTtFQUNBLHNCQUFBO0FBTEo7O0FBVUE7RUFDRSxlQUFBO0VBQ0EsbUJBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EseUJBQUE7QUFQRjs7QUFhQTtFQUNFLFlBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLFVBQUE7RUFDQSxrQkFBQTtFQUNBLHlCQUFBO0FBVkY7O0FBYUE7RUFDRSxtQkFBQTtFQUNBLFVBQUE7QUFWRjs7QUFhQTtFQUNFLGNBQUE7QUFWRiIsImZpbGUiOiJzaWRlbmF2LmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmFwcC1zaWRlbmF2LWNvbnRhaW5lciB7XHJcbiAgZmxleDogMTtcclxuICBwb3NpdGlvbjogZml4ZWQ7XHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgbWluLXdpZHRoOiAxMDAlO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBtaW4taGVpZ2h0OiAxMDAlO1xyXG4gIHVzZXItc2VsZWN0OiBub25lO1xyXG59XHJcblxyXG4uYXBwLXNpZGVuYXYtY29udGVudCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgd2lkdGg6YXV0bztcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG59XHJcblxyXG4uYXBwLXNpZGVuYXYge1xyXG4gIHdpZHRoOiAyNDBweDtcclxufVxyXG5cclxuLmZsZXgtY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7ICBcclxuICB3aWR0aDogMTAwJTtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgbWluLXdpZHRoOiAxMDAlO1xyXG4gIG1pbi1oZWlnaHQ6IDEwMCU7XHJcbn1cclxuXHJcbi5ib3JkZXItZGl2IHtcclxuICBib3JkZXItbGVmdDogMTBweDtcclxuICBib3JkZXItbGVmdC1zdHlsZTogc29saWQ7XHJcbiAgYm9yZGVyLWxlZnQtY29sb3I6IGdyZXk7XHJcbiAgY3Vyc29yOnctcmVzaXplO1xyXG59XHJcblxyXG4uYm9yZGVyLWRpdi1tb3Zpbmcge1xyXG4gIGJvcmRlci1sZWZ0OiA1MHB4O1xyXG4gIGJvcmRlci1sZWZ0LXN0eWxlOiBzb2xpZDtcclxuICBib3JkZXItbGVmdC1jb2xvcjogZ3JleTtcclxuICBjdXJzb3I6IHctcmVzaXplO1xyXG59XHJcblxyXG4uc2hvd1N0YXJ0VG9vbGJhcntcclxuICAgIGhlaWdodDowcHg7XHJcbiAgICBkaXNwbGF5OmNvbnRlbnRzO1xyXG59XHJcblxyXG4vLyBOYXNjb25kZXJlIGUgdmlzdWFsaXp6YXJlIGxhIHRvb2xiYXJcclxuLnNob3dUb29sYmFyIHtcclxuICBoZWlnaHQ6IDY0cHg7XHJcbiAgdHJhbnNpdGlvbjogaGVpZ2h0IDMwMG1zIGVhc2Utb3V0O1xyXG59XHJcblxyXG4vKi5zaG93VG9vbGJhckJsb2Nre1xyXG4gICAgZGlzcGxheTpibG9jaztcclxufSovXHJcblxyXG5cclxuXHJcbi8vIHBlciBmYXJlIHBpY2NvbGkgaSB0YWJcclxuOjpuZy1kZWVwLm1hdC10YWItbGFiZWwsIDo6bmctZGVlcC5tYXQtdGFiLWxhYmVsLWFjdGl2ZSB7XHJcbiAgbWluLXdpZHRoOiAwICFpbXBvcnRhbnQ7XHJcbiAgcGFkZGluZzogM3B4ICFpbXBvcnRhbnQ7XHJcbiAgbWFyZ2luOiAzcHggIWltcG9ydGFudDtcclxufVxyXG5cclxuI3Byb2plY3Rjc3Mge1xyXG4gIDo6bmctZGVlcC5tYXQtdG9vbHRpcCB7XHJcbiAgICAvKiB5b3VyIG93biBjdXN0b20gc3R5bGVzIGhlcmUgKi9cclxuICAgIC8qIGUuZy4gKi9cclxuICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgIGJhY2tncm91bmQtY29sb3I6IGJsdWU7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBib29rbWFyayBtYW5hZ2VtZW50XHJcbmRpdi5tZGJvb2ttYXJrIHtcclxuICBjdXJzb3I6IHBvaW50ZXI7ICBcclxuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIHRleHQtYWxpZ246IHJpZ2h0OyAgIFxyXG4gIGJhY2tncm91bmQ6IHllbGxvdztcclxuICB0cmFuc2l0aW9uOiBhbGwgMC41cyBlYXNlO1xyXG59XHJcblxyXG5kaXYubWRib29rbWFyazpob3ZlciB7ICBcclxufVxyXG5cclxuZGl2LmlubmVyIHtcclxuICBoZWlnaHQ6IDUwcHg7XHJcbiAgd2lkdGg6IDUwcHg7XHJcbiAgYmFja2dyb3VuZDogcmVkO1xyXG4gIG9wYWNpdHk6IDA7IC8vb25seSBmb3IgYmV0dGVyIGVmZmVjdFxyXG4gIHZpc2liaWxpdHk6IGhpZGRlbjtcclxuICB0cmFuc2l0aW9uOiBhbGwgMC41cyBlYXNlO1xyXG59XHJcblxyXG5kaXYub3V0ZXI6aG92ZXIgLmlubmVyIHtcclxuICB2aXNpYmlsaXR5OiBpbml0aWFsO1xyXG4gIG9wYWNpdHk6IDE7XHJcbn1cclxuXHJcbi5mbGV4RXhwYW5kIHtcclxuICBmbGV4OiAxIDEgYXV0bztcclxufVxyXG4iXX0= */"] });


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
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../toolbar/toolbar.component */ "K3CX");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _pipes_safePipe__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../pipes/safePipe */ "8Cql");








const _c0 = ["myBelovedIframe"];
//import { ConnectionLostComponent } from '../../../signalR/dialogs/connection-lost/connection-lost.component';///dialogs/connection-lost/connection-lost.component
class MainContentComponent {
    constructor(service, sanitizer, monitorMDService, dialog, ref) {
        this.service = service;
        this.sanitizer = sanitizer;
        this.monitorMDService = monitorMDService;
        this.dialog = dialog;
        this.ref = ref;
        this.classForToolbar = "showToolbar";
        this.classForContent = "contentWithToolbar";
        this.htmlSource = '../welcome.html';
        //helloWorld: IWorkWithElement = (msg) => {
        //  alert('this is the callback');
        //};
        this._HideIFrame = false;
        console.log("MainContentComponent constructor");
        this.monitorMDService.addMarkdownFileListener(this.markdownFileIsChanged, this);
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
        this.service.whatDisplayForToolbar.subscribe(_ => {
            if ((_ == 'showToolbar' && this.classForToolbar != _) ||
                (_ == 'hideToolbar' && this.classForToolbar != _ + ' ' + 'hideToolbarNone')) { // check if something is truely changed
                this.classForToolbar = _;
                this.classForContent = 'contentWithToolbar';
                if (_ == 'hideToolbar' && _ != undefined) {
                    this.classForToolbar = _ + ' ' + 'hideToolbarNone';
                    this.classForContent = 'hundredPercentContent';
                    this.ref.detectChanges();
                }
                this.ref.detectChanges();
            }
        });
    }
    callMdExplorerController(node) {
        if (node != null && node.relativePath != undefined) {
            let dateTime = new Date().getTime() / 1000;
            this.htmlSource = '../api/mdexplorer' + node.relativePath + '?time=' + dateTime;
        }
    }
    markdownFileIsChanged(data, objectThis) {
        let dateTime = new Date();
        objectThis.service.navigationArray = [];
        objectThis.service.setSelectedMdFileFromServer(data);
        objectThis.htmlSource = '../api/mdexplorer/' + data.path + '?time=' + dateTime;
    }
}
MainContentComponent.ɵfac = function MainContentComponent_Factory(t) { return new (t || MainContentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_2__["DomSanitizer"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_3__["MdServerMessagesService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_4__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectorRef"])); };
MainContentComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: MainContentComponent, selectors: [["app-main-content"]], viewQuery: function MainContentComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵviewQuery"](_c0, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵloadQuery"]()) && (ctx.el = _t.first);
    } }, decls: 4, vars: 6, consts: [[3, "ngClass"], ["id", "mdIframe", "name", "mdIframe", 3, "hidden", "ngClass", "src"], ["myBelovedIframe", ""]], template: function MainContentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](0, "app-toolbar", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](1, "iframe", 1, 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](3, "safe");
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngClass", ctx.classForToolbar);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("hidden", ctx._HideIFrame)("ngClass", ctx.classForContent)("src", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](3, 4, ctx.htmlSource), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵsanitizeResourceUrl"]);
    } }, directives: [_toolbar_toolbar_component__WEBPACK_IMPORTED_MODULE_5__["ToolbarComponent"], _angular_common__WEBPACK_IMPORTED_MODULE_6__["NgClass"]], pipes: [_pipes_safePipe__WEBPACK_IMPORTED_MODULE_7__["SafePipe"]], styles: ["[_nghost-%COMP%] {\n  overflow-y: hidden;\n  overflow-x: hidden;\n  height: 100%;\n  border: 1px solid black;\n}\n\n.hideToolbar[_ngcontent-%COMP%] {\n  height: 0px;\n  transition: height 300ms ease-out;\n}\n\n.hideToolbarNone[_ngcontent-%COMP%] {\n  display: none;\n}\n\n.hundredPercentContent[_ngcontent-%COMP%] {\n  height: 100%;\n  width: 100%;\n}\n\n.contentWithToolbar[_ngcontent-%COMP%] {\n  height: calc(100% - 72px);\n  width: 100%;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbWFpbi1jb250ZW50LmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0Usa0JBQUE7RUFDQSxrQkFBQTtFQUNBLFlBQUE7RUFDQSx1QkFBQTtBQUNGOztBQUdBO0VBQ0UsV0FBQTtFQUNBLGlDQUFBO0FBQUY7O0FBR0E7RUFDRSxhQUFBO0FBQUY7O0FBR0E7RUFDRSxZQUFBO0VBQ0EsV0FBQTtBQUFGOztBQUdBO0VBQ0UseUJBQUE7RUFDQSxXQUFBO0FBQUYiLCJmaWxlIjoibWFpbi1jb250ZW50LmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xyXG4gIG92ZXJmbG93LXk6IGhpZGRlbjtcclxuICBvdmVyZmxvdy14OiBoaWRkZW47XHJcbiAgaGVpZ2h0OiAxMDAlO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkIGJsYWNrO1xyXG59XHJcblxyXG5cclxuLmhpZGVUb29sYmFyIHtcclxuICBoZWlnaHQ6IDBweDtcclxuICB0cmFuc2l0aW9uOiBoZWlnaHQgMzAwbXMgZWFzZS1vdXQ7XHJcbn1cclxuXHJcbi5oaWRlVG9vbGJhck5vbmUge1xyXG4gIGRpc3BsYXk6IG5vbmU7XHJcbn1cclxuXHJcbi5odW5kcmVkUGVyY2VudENvbnRlbnQge1xyXG4gIGhlaWdodDogMTAwJTtcclxuICB3aWR0aDogMTAwJTtcclxufVxyXG5cclxuLmNvbnRlbnRXaXRoVG9vbGJhciB7XHJcbiAgaGVpZ2h0OiBjYWxjKDEwMCUgLSA3MnB4KTtcclxuICB3aWR0aDogMTAwJTtcclxufVxyXG4iXX0= */"] });


/***/ }),

/***/ "oTTF":
/*!*****************************************************************************************!*\
  !*** ./src/app/md-explorer/components/document-settings/document-settings.component.ts ***!
  \*****************************************************************************************/
/*! exports provided: DocumentSettingsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DocumentSettingsComponent", function() { return DocumentSettingsComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/radio */ "QibW");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_select__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/select */ "d3UM");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/core */ "FKr1");














function DocumentSettingsComponent_mat_option_33_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const template_r2 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("value", template_r2.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", template_r2.name, " ");
} }
function DocumentSettingsComponent_mat_option_43_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const template_r3 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("value", template_r3.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", template_r3.name, " ");
} }
class DocumentSettingsComponent {
    //////////////////////////////////
    constructor(mdFileService, _snackBar) {
        this.mdFileService = mdFileService;
        this._snackBar = _snackBar;
        this.standardTemplates = [{ name: "project", fullPath: "" }, { name: "minute", fullPath: "" }];
        this.documentDescriptor = {
            author: 'Carlo',
            date: '2022',
            documentType: 'document',
            email: '@carlo',
            title: 'title',
            wordSection: {
                documentHeader: 'None',
                writeToc: true,
                templateSection: {
                    customTemplate: "",
                    inheritFromTemplate: "minute",
                    templateType: "inherit"
                }
            }
        };
    }
    ;
    /////////////////////////////////
    ngOnInit() {
        this.mdFileService.selectedMdFileFromSideNav.subscribe(currentMdFile => {
            this.selectedMdFile = currentMdFile;
            //
            this.mdFileService.getDocumentSettings(currentMdFile).subscribe(descriptor => {
                this.documentDescriptor = descriptor;
            });
        });
    }
    openSelectedInheritingTemplateWord() {
        this.mdFileService.openInheritingTemplateWord(this.documentDescriptor.wordSection.templateSection.inheritFromTemplate)
            .subscribe(_ => {
            this._snackBar.open("Opening Word Custom template!");
        });
    }
    saveSettings() {
        this.mdFileService.setDocumentSettings(this.documentDescriptor, this.selectedMdFile)
            .subscribe(_ => {
            this._snackBar.open("Saved!");
        });
    }
    openCustomWord() {
        this.mdFileService.opencustomwordtemplate(this.selectedMdFile)
            .subscribe(_ => {
            this._snackBar.open("Opening Word Custom template!");
        });
    }
}
DocumentSettingsComponent.ɵfac = function DocumentSettingsComponent_Factory(t) { return new (t || DocumentSettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_2__["MatSnackBar"])); };
DocumentSettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: DocumentSettingsComponent, selectors: [["app-document-settings"]], decls: 77, vars: 11, consts: [[1, "container-columns"], [1, "container-title"], [1, "container-row"], ["name", "documentType", 1, "container-columns", 3, "disabled", "ngModel", "ngModelChange"], ["value", "document"], ["value", "slide"], ["name", "wordTemplates", "aria-label", "Select a template", 1, "container-columns", 3, "ngModel", "ngModelChange"], ["name", "wordTemplates", "value", "inherits"], ["appearance", "fill"], ["name", "projectTemplate", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["mat-button", "", "color", "warn", 3, "click"], ["value", "custom"], ["name", "customTemplate", 3, "ngModel", "ngModelChange"], ["mat-button", "", 3, "click"], ["color", "warn"], ["name", "wordHeaderPage", "aria-label", "Select a header page", 1, "container-columns", 3, "ngModel", "ngModelChange"], ["value", "None"], ["value", "Project"], ["value", "Custom"], ["name", "writeToc", 1, "example-margin", 3, "ngModel", "checked", "ngModelChange"], ["mat-dialog-actions", "", "align", "end"], ["mat-stroked-button", "", "mat-button", "", "color", "primary", 3, "click"], [3, "value"]], template: function DocumentSettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](4, "Document's Settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "h3");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "form");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](14, "Document's type");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "mat-radio-group", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_radio_group_ngModelChange_16_listener($event) { return ctx.documentDescriptor.documentType = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "mat-radio-button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18, "document");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](19, "mat-radio-button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](20, "slide");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](21, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](22, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](24, "Microsoft Word Templates");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](25, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](26, "mat-radio-group", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_radio_group_ngModelChange_26_listener($event) { return ctx.documentDescriptor.wordSection.templateSection.templateType = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](27, "mat-radio-button", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](28, " Inherit standard template ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](29, "mat-form-field", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](30, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](31, "Select a template");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](32, "mat-select", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_select_ngModelChange_32_listener($event) { return ctx.documentDescriptor.wordSection.templateSection.inheritFromTemplate = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](33, DocumentSettingsComponent_mat_option_33_Template, 2, 2, "mat-option", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](34, "button", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function DocumentSettingsComponent_Template_button_click_34_listener() { return ctx.openSelectedInheritingTemplateWord(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](35, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](36, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](37, "mat-radio-button", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](38, " Custom Template ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](39, "mat-form-field", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](40, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](41, "Copy as reference from");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](42, "mat-select", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_select_ngModelChange_42_listener($event) { return ctx.documentDescriptor.wordSection.templateSection.customTemplate = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](43, DocumentSettingsComponent_mat_option_43_Template, 2, 2, "mat-option", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](44, "button", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function DocumentSettingsComponent_Template_button_click_44_listener() { return ctx.openCustomWord(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](45, "mat-icon", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](46, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](47, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](48, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](49, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](50, "Microsoft Word Document's header page");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](51, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](52, "mat-radio-group", 16);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_radio_group_ngModelChange_52_listener($event) { return ctx.documentDescriptor.wordSection.documentHeader = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](53, "mat-radio-button", 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](54, "None");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](55, "mat-radio-button", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](56, "Append project's document header page");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](57, "mat-icon", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](58, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](59, "mat-radio-button", 19);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](60, " Append custom document header page ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](61, "button", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function DocumentSettingsComponent_Template_button_click_61_listener() { return ctx.openCustomWord(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](62, "mat-icon", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](63, "edit");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](64, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](65, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](66, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](67, "Documents goodies");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](68, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](69, "section", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](70, "mat-checkbox", 20);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function DocumentSettingsComponent_Template_mat_checkbox_ngModelChange_70_listener($event) { return ctx.documentDescriptor.wordSection.writeToc = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](71, "Create TOC in Microsoft Word");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](72, "div", 21);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](73, "button", 22);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function DocumentSettingsComponent_Template_button_click_73_listener() { return ctx.saveSettings(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](74, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](75, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](76, "Save");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.selectedMdFile.name);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("disabled", true)("ngModel", ctx.documentDescriptor.documentType);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.documentDescriptor.wordSection.templateSection.templateType);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.documentDescriptor.wordSection.templateSection.inheritFromTemplate);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx.standardTemplates);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.documentDescriptor.wordSection.templateSection.customTemplate);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx.standardTemplates);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.documentDescriptor.wordSection.documentHeader);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](18);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.documentDescriptor.wordSection.writeToc)("checked", ctx.documentDescriptor.wordSection.writeToc);
    } }, directives: [_angular_forms__WEBPACK_IMPORTED_MODULE_3__["ɵangular_packages_forms_forms_ba"], _angular_forms__WEBPACK_IMPORTED_MODULE_3__["NgControlStatusGroup"], _angular_forms__WEBPACK_IMPORTED_MODULE_3__["NgForm"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardContent"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_5__["MatRadioGroup"], _angular_forms__WEBPACK_IMPORTED_MODULE_3__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_3__["NgModel"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_5__["MatRadioButton"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_6__["MatLabel"], _angular_material_select__WEBPACK_IMPORTED_MODULE_7__["MatSelect"], _angular_common__WEBPACK_IMPORTED_MODULE_8__["NgForOf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__["MatIcon"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_11__["MatCheckbox"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_12__["MatDialogActions"], _angular_material_core__WEBPACK_IMPORTED_MODULE_13__["MatOption"]], styles: [".container-title[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: center;\n}\n\n.container-row[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  justify-content: center;\n}\n\n.container-columns[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  justify-content: center;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZG9jdW1lbnQtc2V0dGluZ3MuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0EsdUJBQUE7QUFDRjs7QUFFQTtFQUNJLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBQ0o7O0FBRUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSx1QkFBQTtBQUNGIiwiZmlsZSI6ImRvY3VtZW50LXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmNvbnRhaW5lci10aXRsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxufVxyXG5cclxuLmNvbnRhaW5lci1yb3d7XHJcbiAgICBkaXNwbGF5OmZsZXg7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjpyb3c7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6Y2VudGVyO1xyXG59XHJcblxyXG4uY29udGFpbmVyLWNvbHVtbnMge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxufVxyXG4iXX0= */"] });


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
/* harmony import */ var angular_animations__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! angular-animations */ "6Z0Z");
/* harmony import */ var _dialogs_copy_from_clipboard_copy_from_clipboard_component__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../dialogs/copy-from-clipboard/copy-from-clipboard.component */ "WD3D");
/* harmony import */ var _dialogs_move_md_file_move_md_file_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../dialogs/move-md-file/move-md-file.component */ "N7Dz");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _services_md_file_service__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../../services/md-file.service */ "xmhS");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/cdk/clipboard */ "UXJo");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");























function MdTreeComponent_ng_template_3_button_1_Template(rf, ctx) { if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_1_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r19); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r17.renameDirectoryOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelement"](1, "img", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_2_Template(rf, ctx) { if (rf & 1) {
    const _r22 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_2_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r22); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r20.createDirectoryOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_3_Template(rf, ctx) { if (rf & 1) {
    const _r25 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_3_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r25); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r23.createMdOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "note_add");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_4_Template(rf, ctx) { if (rf & 1) {
    const _r28 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_4_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r28); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r26.deleteFile(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_5_Template(rf, ctx) { if (rf & 1) {
    const _r31 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_5_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r31); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r29.pasteFromClipboard(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "archive");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_6_Template(rf, ctx) { if (rf & 1) {
    const _r34 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_6_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r34); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r32.getLinkFromNode(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_7_Template(rf, ctx) { if (rf & 1) {
    const _r37 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_7_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r37); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r35.openDocumentSettings(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_8_Template(rf, ctx) { if (rf & 1) {
    const _r40 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_8_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r40); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r38.moveDocument(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "open_with");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_9_Template(rf, ctx) { if (rf & 1) {
    const _r43 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_9_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r43); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r41.openFolderOn(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "b", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](3, " directory on ");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](4, "b", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](5, "File Explorer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_10_Template(rf, ctx) { if (rf & 1) {
    const _r46 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_10_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r46); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r44.setMdAsLandingPage(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "home");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](3, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](4, "Set as project's landing page");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_button_11_Template(rf, ctx) { if (rf & 1) {
    const _r49 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_ng_template_3_button_11_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r49); const item_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().item; const ctx_r47 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r47.cloneTimerDocument(item_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](2, "file_copy");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](3, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](4, "Clone timed document");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
function MdTreeComponent_ng_template_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](1, MdTreeComponent_ng_template_3_button_1_Template, 2, 0, "button", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](2, MdTreeComponent_ng_template_3_button_2_Template, 3, 0, "button", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](3, MdTreeComponent_ng_template_3_button_3_Template, 3, 0, "button", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](4, MdTreeComponent_ng_template_3_button_4_Template, 3, 0, "button", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](5, MdTreeComponent_ng_template_3_button_5_Template, 3, 0, "button", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](6, MdTreeComponent_ng_template_3_button_6_Template, 3, 0, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](7, MdTreeComponent_ng_template_3_button_7_Template, 3, 0, "button", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](8, MdTreeComponent_ng_template_3_button_8_Template, 3, 0, "button", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](9, MdTreeComponent_ng_template_3_button_9_Template, 6, 0, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](10, MdTreeComponent_ng_template_3_button_10_Template, 5, 0, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](11, MdTreeComponent_ng_template_3_button_11_Template, 5, 0, "button", 14);
} if (rf & 2) {
    const item_r5 = ctx.item;
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "folder" || item_r5.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "folder" || item_r5.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "folder" || item_r5.name === "root");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFile" || item_r5.type === "mdFileTimer");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", item_r5.type === "mdFileTimer");
} }
function MdTreeComponent_mat_tree_node_5_Template(rf, ctx) { if (rf & 1) {
    const _r52 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-tree-node", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_5_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r52); const node_r50 = ctx.$implicit; const ctx_r51 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r51.onRightClick($event, node_r50); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "button", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](2, "mat-icon", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](3, "text_snippet");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](4, "a", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("click", function MdTreeComponent_mat_tree_node_5_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r52); const node_r50 = ctx.$implicit; const ctx_r53 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r53.getNode(node_r50); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r50 = ctx.$implicit;
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngClass", ctx_r2.activeNode === node_r50 ? "background-highlight" : "blue-link");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate"](node_r50.name);
} }
function MdTreeComponent_mat_tree_node_6_mat_icon_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-icon", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().$implicit;
    const ctx_r55 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate1"](" ", ctx_r55.treeControl.isExpanded(node_r54) ? "folder_open" : "folder", " ");
} }
function MdTreeComponent_mat_tree_node_6_mat_icon_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-icon", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]().$implicit;
    const ctx_r56 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate1"](" ", ctx_r56.treeControl.isExpanded(node_r54) ? "folder_open" : "folder", " ");
} }
function MdTreeComponent_mat_tree_node_6_Template(rf, ctx) { if (rf & 1) {
    const _r60 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-tree-node", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_6_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r60); const node_r54 = ctx.$implicit; const ctx_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r59.onRightClick($event, node_r54); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "button", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](2, MdTreeComponent_mat_tree_node_6_mat_icon_2_Template, 2, 1, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](3, MdTreeComponent_mat_tree_node_6_mat_icon_3_Template, 2, 1, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r54 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("@fadeInOnEnter", undefined);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵattribute"]("aria-label", "Toggle " + node_r54.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", node_r54.name != "mdPublish");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("ngIf", node_r54.name == "mdPublish");
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtextInterpolate1"](" ", node_r54.name, " ");
} }
function MdTreeComponent_mat_tree_node_7_Template(rf, ctx) { if (rf & 1) {
    const _r63 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](0, "mat-tree-node", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("contextmenu", function MdTreeComponent_mat_tree_node_7_Template_mat_tree_node_contextmenu_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵrestoreView"](_r63); const node_r61 = ctx.$implicit; const ctx_r62 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵnextContext"](); return ctx_r62.onRightClick($event, node_r61); });
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelement"](1, "button", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
} }
const TREE_DATA = [];
class MdTreeComponent {
    ///////////////////////////////
    constructor(router, mdFileService, dialog, snackBar, clipboard) {
        this.router = router;
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
                index: node.index
            };
        };
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__["FlatTreeControl"](node => node.level, node => node.expandable);
        this.treeFlattener = new _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeFlattener"](this._transformer, node => node.level, node => node.expandable, node => node.childrens);
        this.dataSource = new _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeFlatDataSource"](this.treeControl, this.treeFlattener);
        this.hasChild = (_, node) => node.expandable;
        this.isFolder = (_, node) => node.type == "folder";
        this.isMdPublish = (_, node) => node.type == "folder" && node.name == "mdPublish";
        this.isEmptyRoot = (_, node) => node.type == "emptyroot";
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
    //="{ value: '', params: { delay: node.index * 100 } }"
    ngOnInit() {
        this.mdFiles = this.mdFileService.mdFiles;
        this.mdFileService.mdFiles.subscribe(data => {
            data.map(_ => {
                _.index = 0;
                if (_.level === 0) {
                    _.index = Math.floor(Math.random() * 5);
                }
            });
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
        this.router.navigate(['/main/navigation/document']);
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
    moveDocument(node) {
        this.dialog.open(_dialogs_move_md_file_move_md_file_component__WEBPACK_IMPORTED_MODULE_10__["MoveMdFileComponent"], {
            width: '300px',
            data: node,
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
        let finalPath = node.relativePath.replace(/\\/g, "/");
        this.clipboard.copy(finalPath);
    }
    deleteFile(node) {
        this.dialog.open(_dialogs_delete_markdown_delete_markdown_component__WEBPACK_IMPORTED_MODULE_7__["DeleteMarkdownComponent"], {
            width: '300px',
            data: node,
        });
    }
    cloneTimerDocument(node) {
        this.mdFileService.cloneTimerDocument(node).subscribe(data => {
            this.mdFileService.addNewFile(data);
            this.mdFileService.setSelectedMdFileFromSideNav(data[data.length - 1]);
        });
    }
    openDocumentSettings(node) {
        this.mdFileService.setSelectedMdFileFromSideNav(node);
        this.router.navigate(['/main/navigation/documentsettings']);
    }
    pasteFromClipboard(node) {
        this.dialog.open(_dialogs_copy_from_clipboard_copy_from_clipboard_component__WEBPACK_IMPORTED_MODULE_9__["CopyFromClipboardComponent"], {
            width: '300px',
            data: node,
        });
    }
}
MdTreeComponent.ɵfac = function MdTreeComponent_Factory(t) { return new (t || MdTreeComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_12__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_services_md_file_service__WEBPACK_IMPORTED_MODULE_13__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_14__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_15__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_16__["Clipboard"])); };
MdTreeComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdefineComponent"]({ type: MdTreeComponent, selectors: [["app-md-tree"]], viewQuery: function MdTreeComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵviewQuery"](_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], 3);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵloadQuery"]()) && (ctx.matMenuTrigger = _t.first);
    } }, decls: 8, vars: 9, consts: [[2, "visibility", "hidden", "position", "fixed", 3, "matMenuTriggerFor"], ["rightMenu", "matMenu"], ["matMenuContent", ""], [3, "dataSource", "treeControl", "contextmenu"], ["matTreeNodePadding", "", "style", "cursor:pointer", 3, "contextmenu", 4, "matTreeNodeDef"], ["style", "cursor:pointer", "matTreeNodePadding", "", 3, "contextmenu", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "create new document", "mat-icon-button", "", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", "matTooltip", "Paste from Clipboard", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "document settings", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "primary", "matTooltip", "move", 3, "click", 4, "ngIf"], ["mat-menu-item", "", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "color", "accent", "matTooltip", "rename folder", 3, "click"], ["src", "/assets/rename.png", "alt", "Rename directory"], ["mat-icon-button", "", "matTooltip", "create new folder", "color", "primary", 3, "click"], ["mat-icon-button", "", "matTooltip", "create new document", "mat-icon-button", "", "color", "primary", 3, "click"], ["mat-icon-button", "", "color", "warn", "matTooltip", "delete document", 3, "click"], ["mat-icon-button", "", "color", "warn", "matTooltip", "Paste from Clipboard", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "copy link to clipboard", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "document settings", 3, "click"], ["mat-icon-button", "", "color", "primary", "matTooltip", "move", 3, "click"], ["mat-menu-item", "", 3, "click"], [2, "color", "violet"], [2, "color", "blue"], ["matTreeNodePadding", "", 2, "cursor", "pointer", 3, "contextmenu"], ["mat-icon-button", "", "color", "primary", 2, "cursor", "pointer"], [1, "mat-icon-rtl-mirror"], [3, "ngClass", "click"], ["mat-icon-button", "", "matTreeNodeToggle", ""], ["class", "gold-icon mat-icon-rtl-mirror", 4, "ngIf"], ["color", "accent", "class", "gold-icon mat-icon-rtl-mirror", 4, "ngIf"], [1, "gold-icon", "mat-icon-rtl-mirror"], ["color", "accent", 1, "gold-icon", "mat-icon-rtl-mirror"]], template: function MdTreeComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelement"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](1, "mat-menu", null, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](3, MdTreeComponent_ng_template_3_Template, 12, 11, "ng-template", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementStart"](4, "mat-tree", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵlistener"]("contextmenu", function MdTreeComponent_Template_mat_tree_contextmenu_4_listener($event) { return ctx.onRightClick($event, null); });
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](5, MdTreeComponent_mat_tree_node_5_Template, 6, 2, "mat-tree-node", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](6, MdTreeComponent_mat_tree_node_6_Template, 5, 5, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵtemplate"](7, MdTreeComponent_mat_tree_node_7_Template, 2, 0, "mat-tree-node", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵelementEnd"]();
    } if (rf & 2) {
        const _r0 = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵreference"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵstyleProp"]("left", ctx.menuTopLeftPosition.x, "px")("top", ctx.menuTopLeftPosition.y, "px");
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("matMenuTriggerFor", _r0);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isFolder);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.isEmptyRoot);
    } }, directives: [_angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuTrigger"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenu"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuContent"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeDef"], _angular_common__WEBPACK_IMPORTED_MODULE_17__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_18__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_19__["MatTooltip"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_20__["MatIcon"], _angular_material_menu__WEBPACK_IMPORTED_MODULE_1__["MatMenuItem"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodePadding"], _angular_common__WEBPACK_IMPORTED_MODULE_17__["NgClass"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_2__["MatTreeNodeToggle"]], styles: [".mat-tree-node[_ngcontent-%COMP%] {\n  min-height: 40px !important;\n}\n\n.blue-link[_ngcontent-%COMP%] {\n  color: blue;\n  border-bottom-width: 1px;\n  border-bottom: dashed;\n  border-bottom-color: grey;\n}\n\n.background-highlight[_ngcontent-%COMP%] {\n  background-color: blue;\n  color: white;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcbWQtdHJlZS5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLDJCQUFBO0FBQ0Y7O0FBQ0E7RUFDRSxXQUFBO0VBQ0Esd0JBQUE7RUFDQSxxQkFBQTtFQUNBLHlCQUFBO0FBRUY7O0FBQ0E7RUFDRSxzQkFBQTtFQUNBLFlBQUE7QUFFRiIsImZpbGUiOiJtZC10cmVlLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLm1hdC10cmVlLW5vZGUge1xyXG4gIG1pbi1oZWlnaHQ6IDQwcHggIWltcG9ydGFudDtcclxufVxyXG4uYmx1ZS1saW5rIHtcclxuICBjb2xvcjogYmx1ZTtcclxuICBib3JkZXItYm90dG9tLXdpZHRoOiAxcHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogZGFzaGVkO1xyXG4gIGJvcmRlci1ib3R0b20tY29sb3I6IGdyZXk7XHJcbn1cclxuXHJcbi5iYWNrZ3JvdW5kLWhpZ2hsaWdodCB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogYmx1ZTtcclxuICBjb2xvcjogd2hpdGU7XHJcbn1cclxuIl19 */"], data: { animation: [
            Object(angular_animations__WEBPACK_IMPORTED_MODULE_8__["fadeInOnEnterAnimation"])()
        ] } });


/***/ })

}]);
//# sourceMappingURL=md-explorer-md-explorer-module.js.map