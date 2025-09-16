(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["default~md-explorer-md-explorer-module~projects-projects-module"],{

/***/ "4cNg":
/*!**************************************************************************************!*\
  !*** ./src/app/git/dialogs/commit-message-dialog/commit-message-dialog.component.ts ***!
  \**************************************************************************************/
/*! exports provided: CommitMessageDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CommitMessageDialogComponent", function() { return CommitMessageDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");











function CommitMessageDialogComponent_mat_spinner_15_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "mat-spinner", 11);
} }
function CommitMessageDialogComponent_div_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r1.aiError, " ");
} }
class CommitMessageDialogComponent {
    constructor(dialogRef, data, http) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.http = http;
        this.isGeneratingMessage = false;
        this.aiError = null;
        this.commitMessage = data.defaultMessage || 'Update from MdExplorer';
    }
    onCancel() {
        this.dialogRef.close(null);
    }
    onConfirm() {
        if (this.commitMessage && this.commitMessage.trim()) {
            this.dialogRef.close(this.commitMessage.trim());
        }
    }
    generateWithAi() {
        if (!this.data.projectPath) {
            this.aiError = 'Project path not available';
            return;
        }
        this.isGeneratingMessage = true;
        this.aiError = null;
        this.http.post('/api/GitAi/generate-commit-message', {
            projectPath: this.data.projectPath
        }).subscribe({
            next: (response) => {
                this.isGeneratingMessage = false;
                if (response.success && response.suggestedMessage) {
                    this.commitMessage = response.suggestedMessage;
                }
                else if (response.error) {
                    this.aiError = response.error;
                    if (response.suggestedMessage) {
                        // Use fallback message if provided
                        this.commitMessage = response.suggestedMessage;
                    }
                }
            },
            error: (err) => {
                this.isGeneratingMessage = false;
                this.aiError = 'Errore durante la generazione del messaggio';
                console.error('Error generating commit message:', err);
            }
        });
    }
}
CommitMessageDialogComponent.ɵfac = function CommitMessageDialogComponent_Factory(t) { return new (t || CommitMessageDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_2__["HttpClient"])); };
CommitMessageDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: CommitMessageDialogComponent, selectors: [["app-commit-message-dialog"]], decls: 22, vars: 7, consts: [["mat-dialog-title", ""], ["appearance", "outline", 2, "width", "100%", "min-width", "400px"], ["matInput", "", "rows", "4", "placeholder", "Describe your changes...", "cdkFocusInitial", "", 3, "ngModel", "disabled", "ngModelChange", "keydown.enter"], [2, "margin-top", "16px", "display", "flex", "align-items", "center", "gap", "8px"], ["mat-stroked-button", "", "color", "accent", 3, "disabled", "click"], [2, "margin-right", "4px"], ["diameter", "20", "strokeWidth", "2", 4, "ngIf"], ["style", "margin-top: 8px; color: #f44336; font-size: 12px;", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "strokeWidth", "2"], [2, "margin-top", "8px", "color", "#f44336", "font-size", "12px"], [2, "font-size", "16px", "vertical-align", "middle"]], template: function CommitMessageDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Commit Message");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-form-field", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "Enter commit message");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "textarea", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function CommitMessageDialogComponent_Template_textarea_ngModelChange_6_listener($event) { return ctx.commitMessage = $event; })("keydown.enter", function CommitMessageDialogComponent_Template_textarea_keydown_enter_6_listener($event) { return $event.ctrlKey && ctx.onConfirm(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "    ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Press Ctrl+Enter to commit");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_11_listener() { return ctx.generateWithAi(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "smart_toy");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](14);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](15, CommitMessageDialogComponent_mat_spinner_15_Template, 1, 0, "mat-spinner", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, CommitMessageDialogComponent_div_16_Template, 4, 1, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-dialog-actions", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "button", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_18_listener() { return ctx.onCancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](19, "Cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_20_listener() { return ctx.onConfirm(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](21, " Commit ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.commitMessage)("disabled", ctx.isGeneratingMessage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx.isGeneratingMessage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx.isGeneratingMessage ? "Generazione in corso..." : "Genera con AI", " ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isGeneratingMessage);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.aiError);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx.commitMessage || !ctx.commitMessage.trim() || ctx.isGeneratingMessage);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_4__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatHint"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_8__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_9__["MatSpinner"]], styles: [".mat-dialog-container {\n  padding: 24px;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-form-field[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n\ntextarea[_ngcontent-%COMP%] {\n  font-family: monospace;\n  font-size: 14px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  padding: 0;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcY29tbWl0LW1lc3NhZ2UtZGlhbG9nLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtBQUNGOztBQUVBO0VBQ0UsY0FBQTtBQUNGOztBQUVBO0VBQ0UsZUFBQTtBQUNGOztBQUVBO0VBQ0Usc0JBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxnQkFBQTtFQUNBLFVBQUE7QUFDRiIsImZpbGUiOiJjb21taXQtbWVzc2FnZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyI6Om5nLWRlZXAgLm1hdC1kaWFsb2ctY29udGFpbmVyIHtcbiAgcGFkZGluZzogMjRweDtcbn1cblxubWF0LWRpYWxvZy1jb250ZW50IHtcbiAgbWFyZ2luOiAyMHB4IDA7XG59XG5cbm1hdC1mb3JtLWZpZWxkIHtcbiAgZm9udC1zaXplOiAxNHB4O1xufVxuXG50ZXh0YXJlYSB7XG4gIGZvbnQtZmFtaWx5OiBtb25vc3BhY2U7XG4gIGZvbnQtc2l6ZTogMTRweDtcbn1cblxubWF0LWRpYWxvZy1hY3Rpb25zIHtcbiAgbWFyZ2luLXRvcDogMTZweDtcbiAgcGFkZGluZzogMDtcbn0iXX0= */"] });


/***/ }),

/***/ "5Zmz":
/*!***************************************************************!*\
  !*** ./src/app/projects/new-project/new-project.component.ts ***!
  \***************************************************************/
/*! exports provided: DynamicDatabase, NewProjectComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DynamicDatabase", function() { return DynamicDatabase; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NewProjectComponent", function() { return NewProjectComponent; });
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/cdk/tree */ "FvrZ");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../md-explorer/models/md-file */ "aS6m");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/tree */ "8yBR");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");













function NewProjectComponent_mat_tree_node_5_Template(rf, ctx) { if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](0, "mat-tree-node", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](1, "button", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](2, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](3, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](4, "a", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_5_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵrestoreView"](_r4); const node_r2 = ctx.$implicit; const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵnextContext"](); ctx_r3.getFolder(node_r2); return ctx_r3.activeNode = node_r2; });
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r2 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate"](node_r2.name);
} }
function NewProjectComponent_mat_tree_node_6_Template(rf, ctx) { if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](0, "mat-tree-node", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_6_Template_mat_tree_node_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵrestoreView"](_r7); const node_r5 = ctx.$implicit; const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵnextContext"](); return ctx_r6.activeNode = node_r5; });
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](1, "button", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](2, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](4, "a", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_6_Template_a_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵrestoreView"](_r7); const node_r5 = ctx.$implicit; const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵnextContext"](); return ctx_r8.getFolder(node_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
} if (rf & 2) {
    const node_r5 = ctx.$implicit;
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵattribute"]("aria-label", "Toggle " + node_r5.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate1"](" ", ctx_r1.treeControl.isExpanded(node_r5) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate"](node_r5.name);
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
        var md1 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]('12Folder', 'c:primoFolder', 0, true);
        var md2 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]('2Folder', 'c:primoFoldersottoFolder', 1, true);
        var md3 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md4 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
        var md5 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_3__["MdFile"]('5Folder', 'c:cuccu', 3, false);
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
DynamicDatabase.ɵfac = function DynamicDatabase_Factory(t) { return new (t || DynamicDatabase)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_5__["MdFileService"])); };
DynamicDatabase.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineInjectable"]({ token: DynamicDatabase, factory: DynamicDatabase.ɵfac, providedIn: 'root' });
class DynamicDataSource {
    constructor(_treeControl, _database, _mdFileService) {
        this._treeControl = _treeControl;
        this._database = _database;
        this._mdFileService = _mdFileService;
        this.dataChange = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this.data = _database.initialData();
        this._mdFileService.loadDocumentFolder('root', 0, "Folders").subscribe(_ => {
            this.data = _;
        });
        //this.dataChange = _mdFileService._mdDynFolderDocument;
        //_mdFileService.loadDynFolders('root', 1);
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
        return Object(rxjs__WEBPACK_IMPORTED_MODULE_1__["merge"])(collectionViewer.viewChange, this.dataChange).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_2__["map"])(() => this.data));
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
        this._mdFileService.loadDocumentFolder(node.path, node.level + 1, "Folders").subscribe(_ => {
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
class NewProjectComponent {
    constructor(database, mdFileService, router, projectService, dialogRef) {
        this.database = database;
        this.mdFileService = mdFileService;
        this.router = router;
        this.projectService = projectService;
        this.dialogRef = dialogRef;
        this.getLevel = (node) => node.level;
        this.isExpandable = (node) => node.expandable;
        this.hasChild = (_, _nodeData) => _nodeData.expandable;
        this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_0__["FlatTreeControl"](this.getLevel, this.isExpandable);
        this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService);
    }
    ngOnInit() {
        this.folder = { name: "<select project>", path: "" };
        // when the project change, then switch to navigation environment
        this.projectService.currentProjects$.subscribe(_ => {
            if (_ != null && _ != undefined) {
                var dateTime = new Date();
                this.router.navigate(['/main/navigation/document']); //main
                this.dialogRef.close();
            }
        });
    }
    getFolder(node) {
        this.folder.name = node.name;
        this.folder.path = node.path;
    }
    closeDialog() {
        this.projectService.setNewFolderProject(this.folder.path);
    }
}
NewProjectComponent.ɵfac = function NewProjectComponent_Factory(t) { return new (t || NewProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](DynamicDatabase), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_5__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_6__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_8__["MatDialogRef"])); };
NewProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineComponent"]({ type: NewProjectComponent, selectors: [["app-new-project"]], decls: 16, vars: 5, consts: [[1, "flex-container"], [1, "flex-items-overflow"], [3, "dataSource", "treeControl"], ["matTreeNodePadding", "", 4, "matTreeNodeDef"], ["matTreeNodePadding", "", 3, "click", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], [1, "flex-selected-folder"], [1, "flex-items"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["matTreeNodePadding", ""], ["mat-icon-button", ""], [1, "mat-icon-rtl-mirror"], [2, "cursor", "pointer", 3, "click"], ["matTreeNodePadding", "", 3, "click"], ["mat-icon-button", "", "matTreeNodeToggle", ""]], template: function NewProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](1, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](2, "Document's Folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](3, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](4, "mat-tree", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtemplate"](5, NewProjectComponent_mat_tree_node_5_Template, 6, 1, "mat-tree-node", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtemplate"](6, NewProjectComponent_mat_tree_node_6_Template, 6, 3, "mat-tree-node", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](7, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](8, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](9, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](11, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](12);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](13, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementStart"](14, "button", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵlistener"]("click", function NewProjectComponent_Template_button_click_14_listener() { return ctx.closeDialog(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtext"](15, "Open");
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.hasChild);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate"](ctx.folder.name);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵtextInterpolate"](ctx.folder.path);
    } }, directives: [_angular_material_tree__WEBPACK_IMPORTED_MODULE_9__["MatTree"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_9__["MatTreeNodeDef"], _angular_material_card__WEBPACK_IMPORTED_MODULE_10__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_10__["MatCardSubtitle"], _angular_material_button__WEBPACK_IMPORTED_MODULE_11__["MatButton"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_9__["MatTreeNode"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_9__["MatTreeNodePadding"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_12__["MatIcon"], _angular_material_tree__WEBPACK_IMPORTED_MODULE_9__["MatTreeNodeToggle"]], styles: [".flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.flex-items-overflow[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-x: scroll;\n  overflow-y: scroll;\n  background: tomato;\n  color: white;\n  text-align: center;\n  font-size: 3em;\n  flex: 1;\n}\n\n.flex-items[_ngcontent-%COMP%] {\n  width: 500px;\n  text-align: center;\n  font-size: 3em;\n}\n\n.flex-selected-folder[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFxuZXctcHJvamVjdC5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtBQUNGOztBQUVBO0VBQ0UsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0EsT0FBQTtBQUNGOztBQUVBO0VBQ0UsWUFBQTtFQUVBLGtCQUFBO0VBQ0EsY0FBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0FBQUYiLCJmaWxlIjoibmV3LXByb2plY3QuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuZmxleC1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxufVxyXG5cclxuLmZsZXgtaXRlbXMtb3ZlcmZsb3cge1xyXG4gIG1heC1oZWlnaHQ6IDQwMHB4O1xyXG4gIG92ZXJmbG93LXg6IHNjcm9sbDtcclxuICBvdmVyZmxvdy15OiBzY3JvbGw7XHJcbiAgYmFja2dyb3VuZDogdG9tYXRvO1xyXG4gIGNvbG9yOiB3aGl0ZTtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgZm9udC1zaXplOiAzZW07XHJcbiAgZmxleDogMTtcclxufVxyXG5cclxuLmZsZXgtaXRlbXMge1xyXG4gIHdpZHRoOjUwMHB4O1xyXG4gIFxyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBmb250LXNpemU6IDNlbTtcclxufVxyXG5cclxuLmZsZXgtc2VsZWN0ZWQtZm9sZGVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbn1cclxuIl19 */"] });


/***/ }),

/***/ "6Z0Z":
/*!*************************************************************************************!*\
  !*** ./node_modules/angular-animations/__ivy_ngcc__/fesm2015/angular-animations.js ***!
  \*************************************************************************************/
/*! exports provided: animateChildrenOnLeaveAnimation, bounceAnimation, bounceInAnimation, bounceInDownAnimation, bounceInDownOnEnterAnimation, bounceInLeftAnimation, bounceInLeftOnEnterAnimation, bounceInOnEnterAnimation, bounceInRightAnimation, bounceInRightOnEnterAnimation, bounceInUpAnimation, bounceInUpOnEnterAnimation, bounceOnEnterAnimation, bounceOutAnimation, bounceOutDownAnimation, bounceOutDownOnLeaveAnimation, bounceOutLeftAnimation, bounceOutLeftOnLeaveAnimation, bounceOutOnLeaveAnimation, bounceOutRightAnimation, bounceOutRightOnLeaveAnimation, bounceOutUpAnimation, bounceOutUpOnLeaveAnimation, collapseAnimation, collapseHorizontallyAnimation, collapseLeftOnLeaveAnimation, collapseOnLeaveAnimation, expandOnEnterAnimation, expandRightOnEnterAnimation, fadeInAnimation, fadeInDownAnimation, fadeInDownBigAnimation, fadeInDownBigOnEnterAnimation, fadeInDownOnEnterAnimation, fadeInExpandOnEnterAnimation, fadeInExpandRightOnEnterAnimation, fadeInLeftAnimation, fadeInLeftBigAnimation, fadeInLeftBigOnEnterAnimation, fadeInLeftOnEnterAnimation, fadeInOnEnterAnimation, fadeInRightAnimation, fadeInRightBigAnimation, fadeInRightBigOnEnterAnimation, fadeInRightOnEnterAnimation, fadeInUpAnimation, fadeInUpBigAnimation, fadeInUpBigOnEnterAnimation, fadeInUpOnEnterAnimation, fadeOutAnimation, fadeOutCollapseLeftOnLeaveAnimation, fadeOutCollapseOnLeaveAnimation, fadeOutDownAnimation, fadeOutDownBigAnimation, fadeOutDownBigOnLeaveAnimation, fadeOutDownOnLeaveAnimation, fadeOutLeftAnimation, fadeOutLeftBigAnimation, fadeOutLeftBigOnLeaveAnimation, fadeOutLeftOnLeaveAnimation, fadeOutOnLeaveAnimation, fadeOutRightAnimation, fadeOutRightBigAnimation, fadeOutRightBigOnLeaveAnimation, fadeOutRightOnLeaveAnimation, fadeOutUpAnimation, fadeOutUpBigAnimation, fadeOutUpBigOnLeaveAnimation, fadeOutUpOnLeaveAnimation, flashAnimation, flashOnEnterAnimation, flipAnimation, flipInXAnimation, flipInXOnEnterAnimation, flipInYAnimation, flipInYOnEnterAnimation, flipOnEnterAnimation, flipOutXAnimation, flipOutXOnLeaveAnimation, flipOutYAnimation, flipOutYOnLeaveAnimation, headShakeAnimation, headShakeOnEnterAnimation, heartBeatAnimation, heartBeatOnEnterAnimation, hingeAnimation, hingeOnLeaveAnimation, hueRotateAnimation, jackInTheBoxAnimation, jackInTheBoxOnEnterAnimation, jelloAnimation, jelloOnEnterAnimation, lightSpeedInAnimation, lightSpeedInOnEnterAnimation, lightSpeedOutAnimation, lightSpeedOutOnLeaveAnimation, pulseAnimation, pulseOnEnterAnimation, rollInAnimation, rollInOnEnterAnimation, rollOutAnimation, rollOutOnLeaveAnimation, rotateAnimation, rotateInAnimation, rotateInDownLeftAnimation, rotateInDownLeftOnEnterAnimation, rotateInDownRightAnimation, rotateInDownRightOnEnterAnimation, rotateInOnEnterAnimation, rotateInUpLeftAnimation, rotateInUpLeftOnEnterAnimation, rotateInUpRightAnimation, rotateInUpRightOnEnterAnimation, rotateOutAnimation, rotateOutDownLeftAnimation, rotateOutDownLeftOnLeaveAnimation, rotateOutDownRightAnimation, rotateOutDownRightOnLeaveAnimation, rotateOutOnLeaveAnimation, rotateOutUpLeftAnimation, rotateOutUpLeftOnLeaveAnimation, rotateOutUpRightAnimation, rotateOutUpRightOnLeaveAnimation, rubberBandAnimation, rubberBandOnEnterAnimation, shakeAnimation, shakeOnEnterAnimation, slideInDownAnimation, slideInDownOnEnterAnimation, slideInLeftAnimation, slideInLeftOnEnterAnimation, slideInRightAnimation, slideInRightOnEnterAnimation, slideInUpAnimation, slideInUpOnEnterAnimation, slideOutDownAnimation, slideOutDownOnLeaveAnimation, slideOutLeftAnimation, slideOutLeftOnLeaveAnimation, slideOutRightAnimation, slideOutRightOnLeaveAnimation, slideOutUpAnimation, slideOutUpOnLeaveAnimation, swingAnimation, swingOnEnterAnimation, tadaAnimation, tadaOnEnterAnimation, wobbleAnimation, wobbleOnEnterAnimation, zoomInAnimation, zoomInDownAnimation, zoomInDownOnEnterAnimation, zoomInLeftAnimation, zoomInLeftOnEnterAnimation, zoomInOnEnterAnimation, zoomInRightAnimation, zoomInRightOnEnterAnimation, zoomInUpAnimation, zoomInUpOnEnterAnimation, zoomOutAnimation, zoomOutDownAnimation, zoomOutDownOnLeaveAnimation, zoomOutLeftAnimation, zoomOutLeftOnLeaveAnimation, zoomOutOnLeaveAnimation, zoomOutRightAnimation, zoomOutRightOnLeaveAnimation, zoomOutUpAnimation, zoomOutUpOnLeaveAnimation */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "animateChildrenOnLeaveAnimation", function() { return animateChildrenOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceAnimation", function() { return bounceAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInAnimation", function() { return bounceInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInDownAnimation", function() { return bounceInDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInDownOnEnterAnimation", function() { return bounceInDownOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInLeftAnimation", function() { return bounceInLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInLeftOnEnterAnimation", function() { return bounceInLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInOnEnterAnimation", function() { return bounceInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInRightAnimation", function() { return bounceInRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInRightOnEnterAnimation", function() { return bounceInRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInUpAnimation", function() { return bounceInUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceInUpOnEnterAnimation", function() { return bounceInUpOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOnEnterAnimation", function() { return bounceOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutAnimation", function() { return bounceOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutDownAnimation", function() { return bounceOutDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutDownOnLeaveAnimation", function() { return bounceOutDownOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutLeftAnimation", function() { return bounceOutLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutLeftOnLeaveAnimation", function() { return bounceOutLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutOnLeaveAnimation", function() { return bounceOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutRightAnimation", function() { return bounceOutRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutRightOnLeaveAnimation", function() { return bounceOutRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutUpAnimation", function() { return bounceOutUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bounceOutUpOnLeaveAnimation", function() { return bounceOutUpOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "collapseAnimation", function() { return collapseAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "collapseHorizontallyAnimation", function() { return collapseHorizontallyAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "collapseLeftOnLeaveAnimation", function() { return collapseLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "collapseOnLeaveAnimation", function() { return collapseOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "expandOnEnterAnimation", function() { return expandOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "expandRightOnEnterAnimation", function() { return expandRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInAnimation", function() { return fadeInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInDownAnimation", function() { return fadeInDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInDownBigAnimation", function() { return fadeInDownBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInDownBigOnEnterAnimation", function() { return fadeInDownBigOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInDownOnEnterAnimation", function() { return fadeInDownOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInExpandOnEnterAnimation", function() { return fadeInExpandOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInExpandRightOnEnterAnimation", function() { return fadeInExpandRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInLeftAnimation", function() { return fadeInLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInLeftBigAnimation", function() { return fadeInLeftBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInLeftBigOnEnterAnimation", function() { return fadeInLeftBigOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInLeftOnEnterAnimation", function() { return fadeInLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInOnEnterAnimation", function() { return fadeInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInRightAnimation", function() { return fadeInRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInRightBigAnimation", function() { return fadeInRightBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInRightBigOnEnterAnimation", function() { return fadeInRightBigOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInRightOnEnterAnimation", function() { return fadeInRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInUpAnimation", function() { return fadeInUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInUpBigAnimation", function() { return fadeInUpBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInUpBigOnEnterAnimation", function() { return fadeInUpBigOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeInUpOnEnterAnimation", function() { return fadeInUpOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutAnimation", function() { return fadeOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutCollapseLeftOnLeaveAnimation", function() { return fadeOutCollapseLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutCollapseOnLeaveAnimation", function() { return fadeOutCollapseOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutDownAnimation", function() { return fadeOutDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutDownBigAnimation", function() { return fadeOutDownBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutDownBigOnLeaveAnimation", function() { return fadeOutDownBigOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutDownOnLeaveAnimation", function() { return fadeOutDownOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutLeftAnimation", function() { return fadeOutLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutLeftBigAnimation", function() { return fadeOutLeftBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutLeftBigOnLeaveAnimation", function() { return fadeOutLeftBigOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutLeftOnLeaveAnimation", function() { return fadeOutLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutOnLeaveAnimation", function() { return fadeOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutRightAnimation", function() { return fadeOutRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutRightBigAnimation", function() { return fadeOutRightBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutRightBigOnLeaveAnimation", function() { return fadeOutRightBigOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutRightOnLeaveAnimation", function() { return fadeOutRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutUpAnimation", function() { return fadeOutUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutUpBigAnimation", function() { return fadeOutUpBigAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutUpBigOnLeaveAnimation", function() { return fadeOutUpBigOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fadeOutUpOnLeaveAnimation", function() { return fadeOutUpOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flashAnimation", function() { return flashAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flashOnEnterAnimation", function() { return flashOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipAnimation", function() { return flipAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipInXAnimation", function() { return flipInXAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipInXOnEnterAnimation", function() { return flipInXOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipInYAnimation", function() { return flipInYAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipInYOnEnterAnimation", function() { return flipInYOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipOnEnterAnimation", function() { return flipOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipOutXAnimation", function() { return flipOutXAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipOutXOnLeaveAnimation", function() { return flipOutXOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipOutYAnimation", function() { return flipOutYAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flipOutYOnLeaveAnimation", function() { return flipOutYOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "headShakeAnimation", function() { return headShakeAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "headShakeOnEnterAnimation", function() { return headShakeOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "heartBeatAnimation", function() { return heartBeatAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "heartBeatOnEnterAnimation", function() { return heartBeatOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hingeAnimation", function() { return hingeAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hingeOnLeaveAnimation", function() { return hingeOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hueRotateAnimation", function() { return hueRotateAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "jackInTheBoxAnimation", function() { return jackInTheBoxAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "jackInTheBoxOnEnterAnimation", function() { return jackInTheBoxOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "jelloAnimation", function() { return jelloAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "jelloOnEnterAnimation", function() { return jelloOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "lightSpeedInAnimation", function() { return lightSpeedInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "lightSpeedInOnEnterAnimation", function() { return lightSpeedInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "lightSpeedOutAnimation", function() { return lightSpeedOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "lightSpeedOutOnLeaveAnimation", function() { return lightSpeedOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "pulseAnimation", function() { return pulseAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "pulseOnEnterAnimation", function() { return pulseOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rollInAnimation", function() { return rollInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rollInOnEnterAnimation", function() { return rollInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rollOutAnimation", function() { return rollOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rollOutOnLeaveAnimation", function() { return rollOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateAnimation", function() { return rotateAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInAnimation", function() { return rotateInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInDownLeftAnimation", function() { return rotateInDownLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInDownLeftOnEnterAnimation", function() { return rotateInDownLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInDownRightAnimation", function() { return rotateInDownRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInDownRightOnEnterAnimation", function() { return rotateInDownRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInOnEnterAnimation", function() { return rotateInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInUpLeftAnimation", function() { return rotateInUpLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInUpLeftOnEnterAnimation", function() { return rotateInUpLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInUpRightAnimation", function() { return rotateInUpRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateInUpRightOnEnterAnimation", function() { return rotateInUpRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutAnimation", function() { return rotateOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutDownLeftAnimation", function() { return rotateOutDownLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutDownLeftOnLeaveAnimation", function() { return rotateOutDownLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutDownRightAnimation", function() { return rotateOutDownRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutDownRightOnLeaveAnimation", function() { return rotateOutDownRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutOnLeaveAnimation", function() { return rotateOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutUpLeftAnimation", function() { return rotateOutUpLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutUpLeftOnLeaveAnimation", function() { return rotateOutUpLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutUpRightAnimation", function() { return rotateOutUpRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rotateOutUpRightOnLeaveAnimation", function() { return rotateOutUpRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rubberBandAnimation", function() { return rubberBandAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "rubberBandOnEnterAnimation", function() { return rubberBandOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "shakeAnimation", function() { return shakeAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "shakeOnEnterAnimation", function() { return shakeOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInDownAnimation", function() { return slideInDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInDownOnEnterAnimation", function() { return slideInDownOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInLeftAnimation", function() { return slideInLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInLeftOnEnterAnimation", function() { return slideInLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInRightAnimation", function() { return slideInRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInRightOnEnterAnimation", function() { return slideInRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInUpAnimation", function() { return slideInUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideInUpOnEnterAnimation", function() { return slideInUpOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutDownAnimation", function() { return slideOutDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutDownOnLeaveAnimation", function() { return slideOutDownOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutLeftAnimation", function() { return slideOutLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutLeftOnLeaveAnimation", function() { return slideOutLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutRightAnimation", function() { return slideOutRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutRightOnLeaveAnimation", function() { return slideOutRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutUpAnimation", function() { return slideOutUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slideOutUpOnLeaveAnimation", function() { return slideOutUpOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "swingAnimation", function() { return swingAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "swingOnEnterAnimation", function() { return swingOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "tadaAnimation", function() { return tadaAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "tadaOnEnterAnimation", function() { return tadaOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "wobbleAnimation", function() { return wobbleAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "wobbleOnEnterAnimation", function() { return wobbleOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInAnimation", function() { return zoomInAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInDownAnimation", function() { return zoomInDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInDownOnEnterAnimation", function() { return zoomInDownOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInLeftAnimation", function() { return zoomInLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInLeftOnEnterAnimation", function() { return zoomInLeftOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInOnEnterAnimation", function() { return zoomInOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInRightAnimation", function() { return zoomInRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInRightOnEnterAnimation", function() { return zoomInRightOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInUpAnimation", function() { return zoomInUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomInUpOnEnterAnimation", function() { return zoomInUpOnEnterAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutAnimation", function() { return zoomOutAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutDownAnimation", function() { return zoomOutDownAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutDownOnLeaveAnimation", function() { return zoomOutDownOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutLeftAnimation", function() { return zoomOutLeftAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutLeftOnLeaveAnimation", function() { return zoomOutLeftOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutOnLeaveAnimation", function() { return zoomOutOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutRightAnimation", function() { return zoomOutRightAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutRightOnLeaveAnimation", function() { return zoomOutRightOnLeaveAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutUpAnimation", function() { return zoomOutUpAnimation; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zoomOutUpOnLeaveAnimation", function() { return zoomOutUpOnLeaveAnimation; });
/* harmony import */ var _angular_animations__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/animations */ "R0Ic");


function useAnimationIncludingChildren(animation, options) {
    return [
        ...(options && options.animateChildren === 'before' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : []),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["useAnimation"])(animation),
            ...(!options || !options.animateChildren || options.animateChildren === 'together'
                ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })]
                : [])
        ]),
        ...(options && options.animateChildren === 'after' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : [])
    ];
}

const bounce = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -30px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -30px, 0)', easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)', offset: 0.43 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)', offset: 0.53 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -15px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.7 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -4px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0 = bounce;
const DEFAULT_DURATION = 1000;
function bounceAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounce', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom' }), ...useAnimationIncludingChildren(bounce(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION
            }
        })
    ]);
}
function bounceOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom' }),
            ...useAnimationIncludingChildren(bounce(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION
            }
        })
    ]);
}

const flash = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 0.25 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$1 = flash;
const DEFAULT_DURATION$1 = 1000;
function flashAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flash', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(flash(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1
            }
        })
    ]);
}
function flashOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flashOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(flash(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1
            }
        })
    ]);
}

const headShake = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'translateX(0)', easing: 'ease-in-out', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translateX(-6px) rotateY(-9deg)', easing: 'ease-in-out', offset: 0.065 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translateX(5px) rotateY(7deg)', easing: 'ease-in-out', offset: 0.185 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translateX(-3px) rotateY(-5deg)', easing: 'ease-in-out', offset: 0.315 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translateX(2px) rotateY(3deg)', easing: 'ease-in-out', offset: 0.435 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translateX(0)', easing: 'ease-in-out', offset: 0.5 })
    ]))
]);
const ɵ0$2 = headShake;
const DEFAULT_DURATION$2 = 1000;
function headShakeAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'headShake', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(headShake(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$2
            }
        })
    ]);
}
function headShakeOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'headShakeOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(headShake(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$2
            }
        })
    ]);
}

const heartBeat = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'scale(1)', easing: 'ease-in-out', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale({{scale}})', easing: 'ease-in-out', offset: 0.14 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale(1)', easing: 'ease-in-out', offset: 0.28 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale({{scale}})', easing: 'ease-in-out', offset: 0.42 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale(1)', easing: 'ease-in-out', offset: 0.7 })
    ]))
]);
const ɵ0$3 = heartBeat;
const DEFAULT_DURATION$3 = 1300;
const DEFAULT_SCALE = 1.3;
function heartBeatAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'heartBeat', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(heartBeat(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$3,
                scale: (options && options.scale) || DEFAULT_SCALE
            }
        })
    ]);
}
function heartBeatOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'heartBeatOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(heartBeat(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$3,
                scale: (options && options.scale) || DEFAULT_SCALE
            }
        })
    ]);
}

const jello = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0.111 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(-12.5deg) skewY(-12.5deg)', easing: 'ease', offset: 0.222 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(6.25deg) skewY(6.25deg)', easing: 'ease', offset: 0.333 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(-3.125deg) skewY(-3.125deg)', easing: 'ease', offset: 0.444 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(1.5625deg) skewY(1.5625deg)', easing: 'ease', offset: 0.555 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(-0.78125deg) skewY(-0.78125deg)', easing: 'ease', offset: 0.666 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(0.390625deg) skewY(0.390625deg)', easing: 'ease', offset: 0.777 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(-0.1953125deg) skewY(-0.1953125deg)', easing: 'ease', offset: 0.888 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'skewX(0deg) skewY(0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$4 = jello;
const DEFAULT_DURATION$4 = 1000;
function jelloAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'jello', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(jello(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$4
            }
        })
    ]);
}
function jelloOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'jelloOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(jello(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$4
            }
        })
    ]);
}

const pulse = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d({{scale}}, {{scale}}, {{scale}})', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$5 = pulse;
const DEFAULT_DURATION$5 = 1000;
function pulseAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'pulse', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(pulse(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$5,
                scale: (options && options.scale) || 1.05
            }
        })
    ]);
}
function pulseOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'pulseOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(pulse(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$5,
                scale: (options && options.scale) || 1.05
            }
        })
    ]);
}

const rubberBand = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.25, 0.75, 1)', easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.75, 1.25, 1)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.15, 0.85, 1)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.95, 1.05, 1)', easing: 'ease', offset: 0.65 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.05, 0.95, 1)', easing: 'ease', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$6 = rubberBand;
const DEFAULT_DURATION$6 = 1000;
function rubberBandAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rubberBand', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(rubberBand(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$6
            }
        })
    ]);
}
function rubberBandOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rubberBandOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(rubberBand(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$6
            }
        })
    ]);
}

const shake = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0.1 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0.7 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$7 = shake;
const DEFAULT_DURATION$7 = 1000;
function shakeAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'shake', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(shake(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$7,
                translate: (options && options.translate) || '10px'
            }
        })
    ]);
}
function shakeOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'shakeOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(shake(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$7,
                translate: (options && options.translate) || '10px'
            }
        })
    ]);
}

const swing = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'top center', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'rotate3d(0, 0, 1, 15deg)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'rotate3d(0, 0, 1, -10deg)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'rotate3d(0, 0, 1, 5deg)', easing: 'ease', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'rotate3d(0, 0, 1, -5deg)', easing: 'ease', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$8 = swing;
const DEFAULT_DURATION$8 = 1000;
function swingAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'swing', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(swing(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$8
            }
        })
    ]);
}
function swingOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'swingOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'top center' }), ...useAnimationIncludingChildren(swing(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$8
            }
        })
    ]);
}

const tada = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.1 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)', easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)', easing: 'ease', offset: 0.7 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)', easing: 'ease', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$9 = tada;
const DEFAULT_DURATION$9 = 1000;
function tadaAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'tada', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(tada(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$9
            }
        })
    ]);
}
function tadaOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'tadaOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(tada(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$9
            }
        })
    ]);
}

const wobble = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-25%, 0, 0) rotate3d(0, 0, 1, -5deg)', easing: 'ease', offset: 0.15 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(20%, 0, 0) rotate3d(0, 0, 1, 3deg)', easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-15%, 0, 0) rotate3d(0, 0, 1, -3deg)', easing: 'ease', offset: 0.45 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(10%, 0, 0) rotate3d(0, 0, 1, 2deg)', easing: 'ease', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-5%, 0, 0) rotate3d(0, 0, 1, -1deg)', easing: 'ease', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$a = wobble;
const DEFAULT_DURATION$a = 1000;
function wobbleAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'wobble', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(`0 ${(options && options.direction) || '<=>'} 1`, [...useAnimationIncludingChildren(wobble(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$a
            }
        })
    ]);
}
function wobbleOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'wobbleOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(wobble(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$a
            }
        })
    ]);
}

const bounceInDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -{{translate}}, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 25px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -10px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 5px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ]))
]));
const ɵ0$b = bounceInDown;
const DEFAULT_DURATION$b = 1000;
function bounceInDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$b,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}
function bounceInDownOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInDownOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$b,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}

const bounceInLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(25px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-10px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(5px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ]))
]));
const ɵ0$c = bounceInLeft;
const DEFAULT_DURATION$c = 1000;
function bounceInLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$c,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}
function bounceInLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$c,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}

const bounceInRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-25px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(10px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-5px, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ]))
]));
const ɵ0$d = bounceInRight;
const DEFAULT_DURATION$d = 1000;
function bounceInRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$d,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}
function bounceInRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$d,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}

const bounceInUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, {{translate}}, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -20px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 10px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.75 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -5px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.9 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -5px, 0)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ]))
]));
const ɵ0$e = bounceInUp;
const DEFAULT_DURATION$e = 1000;
function bounceInUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$e,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}
function bounceInUpOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInUpOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$e,
                translate: (options && options.translate) || '3000px'
            }
        })
    ]);
}

const bounceIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.3, 0.3, 0.3)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.9, 0.9, 0.9)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.03, 1.03, 1.03)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.97, 0.97, 0.97)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)', offset: 1 })
    ]))
]));
const ɵ0$f = bounceIn;
const DEFAULT_DURATION$f = 750;
function bounceInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$f
            }
        })
    ]);
}
function bounceInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(bounceIn(), options)]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$f
            }
        })
    ]);
}

const bounceOutDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 10px, 0)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -20px, 0)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -20px, 0)', easing: 'ease', offset: 0.45 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0.45 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 1 })
    ]))
]));
const ɵ0$g = bounceOutDown;
const DEFAULT_DURATION$g = 1000;
function bounceOutDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(bounceOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$g,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function bounceOutDownOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutDownOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(bounceOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$g,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const bounceOutLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(20px, 0, 0)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$h = bounceOutLeft;
const DEFAULT_DURATION$h = 1000;
function bounceOutLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(bounceOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$h,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function bounceOutLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(bounceOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$h,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const bounceOutRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(-20px, 0, 0)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$i = bounceOutRight;
const DEFAULT_DURATION$i = 1000;
function bounceOutRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(bounceOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$i,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function bounceOutRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(bounceOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$i,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const bounceOutUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -10px, 0)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 20px, 0)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 20px, 0)', easing: 'ease', offset: 0.45 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0.45 }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 1 })
        ]))
    ])
]));
const ɵ0$j = bounceOutUp;
const DEFAULT_DURATION$j = 1000;
function bounceOutUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(bounceOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$j,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function bounceOutUpOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutUpOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(bounceOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$j,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const bounceOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.9, 0.9, 0.9)', easing: 'ease', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1.1, 1.1, 1.1)', easing: 'ease', offset: 0.55 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.3, 0.3, 0.3)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0.55 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 1 })
    ]))
]));
const ɵ0$k = bounceOut;
const DEFAULT_DURATION$k = 750;
function bounceOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(bounceOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$k
            }
        })
    ]);
}
function bounceOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'bounceOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(bounceOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$k
            }
        })
    ]);
}

const fadeInDownBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$l = fadeInDownBig;
const DEFAULT_DURATION$l = 1000;
function fadeInDownBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInDownBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInDownBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$l,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeInDownBigOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInDownBigOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInDownBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$l,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeInDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$m = fadeInDown;
const DEFAULT_DURATION$m = 1000;
function fadeInDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$m,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeInDownOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInDownOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$m,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeInLeftBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$n = fadeInLeftBig;
const DEFAULT_DURATION$n = 1000;
function fadeInLeftBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInLeftBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInLeftBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$n,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeInLeftBigOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInLeftBigOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInLeftBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$n,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeInLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$o = fadeInLeft;
const DEFAULT_DURATION$o = 1000;
function fadeInLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$o,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeInLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$o,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeInRightBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$p = fadeInRightBig;
const DEFAULT_DURATION$p = 1000;
function fadeInRightBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInRightBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInRightBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$p,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeInRightBigOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInRightBigOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInRightBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$p,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeInRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$q = fadeInRight;
const DEFAULT_DURATION$q = 1000;
function fadeInRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$q,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeInRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$q,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeInUpBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$r = fadeInUpBig;
const DEFAULT_DURATION$r = 1000;
function fadeInUpBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInUpBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInUpBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$r,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeInUpBigOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInUpBigOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInUpBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$r,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeInUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$s = fadeInUp;
const DEFAULT_DURATION$s = 1000;
function fadeInUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$s,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeInUpOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInUpOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$s,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, easing: 'ease', offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 1 })]))
]);
const ɵ0$t = fadeIn;
const DEFAULT_DURATION$t = 1000;
function fadeInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$t
            }
        })
    ]);
}
function fadeInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$t
            }
        })
    ]);
}

const fadeOutDownBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$u = fadeOutDownBig;
const DEFAULT_DURATION$u = 1000;
function fadeOutDownBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutDownBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutDownBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$u,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeOutDownBigOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutDownBigOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutDownBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$u,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeOutDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$v = fadeOutDown;
const DEFAULT_DURATION$v = 1000;
function fadeOutDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$v,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeOutDownOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutDownOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$v,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeOutLeftBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$w = fadeOutLeftBig;
const DEFAULT_DURATION$w = 1000;
function fadeOutLeftBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutLeftBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutLeftBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$w,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeOutLeftBigOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutLeftBigOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutLeftBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$w,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeOutLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$x = fadeOutLeft;
const DEFAULT_DURATION$x = 1000;
function fadeOutLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$x,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeOutLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$x,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeOutRightBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$y = fadeOutRightBig;
const DEFAULT_DURATION$y = 1000;
function fadeOutRightBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutRightBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutRightBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$y,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeOutRightBigOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutRightBigOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutRightBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$y,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeOutRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$z = fadeOutRight;
const DEFAULT_DURATION$z = 1000;
function fadeOutRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$z,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeOutRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$z,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeOutUpBig = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$A = fadeOutUpBig;
const DEFAULT_DURATION$A = 1000;
function fadeOutUpBigAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutUpBig', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutUpBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$A,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}
function fadeOutUpBigOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutUpBigOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutUpBig(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$A,
                translate: (options && options.translate) || '2000px'
            }
        })
    ]);
}

const fadeOutUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$B = fadeOutUp;
const DEFAULT_DURATION$B = 1000;
function fadeOutUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$B,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function fadeOutUpOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutUpOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$B,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const fadeOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 1 })]))
]);
const ɵ0$C = fadeOut;
const DEFAULT_DURATION$C = 1000;
function fadeOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(fadeOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$C
            }
        })
    ]);
}
function fadeOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$C
            }
        })
    ]);
}

const flipInX = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            transform: 'perspective(400px) rotate3d(1, 0, 0, {{degrees}}deg)',
            opacity: 0,
            easing: 'ease-in',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(1, 0, 0, -20deg)', opacity: 0.5, easing: 'ease-in', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(1, 0, 0, 10deg)', opacity: 1, easing: 'ease-in', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(1, 0, 0, -5deg)', easing: 'ease', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$D = flipInX;
const DEFAULT_DURATION$D = 1000;
function flipInXAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipInX', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipInX(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$D,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}
function flipInXOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipInXOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipInX(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$D,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}

const flipInY = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            transform: 'perspective(400px) rotate3d(0, 1, 0, {{degrees}}deg)',
            opacity: 0,
            easing: 'ease-in',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(0, 1, 0, -20deg)', opacity: 0.5, easing: 'ease-in', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(0, 1, 0, 10deg)', opacity: 1, easing: 'ease-in', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(0, 1, 0, -5deg)', easing: 'ease', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$E = flipInY;
const DEFAULT_DURATION$E = 1000;
function flipInYAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipInY', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipInY(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$E,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}
function flipInYOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipInYOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipInY(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$E,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}

const flipOutX = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px)', opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(1, 0, 0, -20deg)', opacity: 1, easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(1, 0, 0, {{degrees}}deg)', opacity: 0, easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$F = flipOutX;
const DEFAULT_DURATION$F = 750;
function flipOutXAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipOutX', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipOutX(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$F,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}
function flipOutXOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipOutXOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipOutX(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$F,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}

const flipOutY = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px)', opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(0, 1, 0, -15deg)', opacity: 1, easing: 'ease', offset: 0.3 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'perspective(400px) rotate3d(0, 1, 0, {{degrees}}deg)', opacity: 0, easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$G = flipOutY;
const DEFAULT_DURATION$G = 750;
function flipOutYAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipOutY', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipOutY(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$G,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}
function flipOutYOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipOutYOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flipOutY(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$G,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}

const flip = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, -360deg)',
            easing: 'ease-out',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px) rotate3d(0, 1, 0, -190deg)',
            easing: 'ease-out',
            offset: 0.4
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px) rotate3d(0, 1, 0, -170deg)',
            easing: 'ease-out',
            offset: 0.5
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'perspective(400px) scale3d(0.95, 0.95, 0.95) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)',
            easing: 'ease-in',
            offset: 0.8
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)',
            easing: 'ease-in',
            offset: 1
        })
    ]))
]);
const ɵ0$H = flip;
const DEFAULT_DURATION$H = 1000;
function flipAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flip', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 <=> 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flip(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$H
            }
        })
    ]);
}
function flipOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'flipOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'backface-visibility': 'visible' }), ...useAnimationIncludingChildren(flip(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$H
            }
        })
    ]);
}

const lightSpeedIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'translate3d({{translate}}, 0, 0) skewX(-30deg)',
            easing: 'ease-out',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'skewX(20deg)', easing: 'ease-out', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'skewX(-5deg)', easing: 'ease-out', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0)', easing: 'ease-out', offset: 1 })
    ]))
]);
const ɵ0$I = lightSpeedIn;
const DEFAULT_DURATION$I = 1000;
function lightSpeedInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'lightSpeedIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(lightSpeedIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$I,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function lightSpeedInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'lightSpeedInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(lightSpeedIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$I,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const lightSpeedOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease-in', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d({{translate}}, 0, 0) skewX(30deg)', easing: 'ease-in', offset: 1 })
    ]))
]);
const ɵ0$J = lightSpeedOut;
const DEFAULT_DURATION$J = 1000;
function lightSpeedOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'lightSpeedOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(lightSpeedOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$J,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function lightSpeedOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'lightSpeedOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(lightSpeedOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$J,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const rotateInDownLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$K = rotateInDownLeft;
const DEFAULT_DURATION$K = 1000;
function rotateInDownLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInDownLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }),
            ...useAnimationIncludingChildren(rotateInDownLeft(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$K,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}
function rotateInDownLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInDownLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }),
            ...useAnimationIncludingChildren(rotateInDownLeft(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$K,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}

const rotateInDownRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$L = rotateInDownRight;
const DEFAULT_DURATION$L = 1000;
function rotateInDownRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInDownRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }),
            ...useAnimationIncludingChildren(rotateInDownRight(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$L,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}
function rotateInDownRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInDownRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }),
            ...useAnimationIncludingChildren(rotateInDownRight(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$L,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}

const rotateInUpLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$M = rotateInUpLeft;
const DEFAULT_DURATION$M = 1000;
function rotateInUpLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInUpLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }),
            ...useAnimationIncludingChildren(rotateInUpLeft(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$M,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}
function rotateInUpLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInUpLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }),
            ...useAnimationIncludingChildren(rotateInUpLeft(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$M,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}

const rotateInUpRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$N = rotateInUpRight;
const DEFAULT_DURATION$N = 1000;
function rotateInUpRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInUpRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }),
            ...useAnimationIncludingChildren(rotateInUpRight(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$N,
                degrees: (options && options.degrees) || -90
            }
        })
    ]);
}
function rotateInUpRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInUpRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }),
            ...useAnimationIncludingChildren(rotateInUpRight(), options)
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$N,
                degrees: (options && options.degrees) || -90
            }
        })
    ]);
}

const rotateIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, transform: 'rotate({{degrees}}deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'rotate(0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$O = rotateIn;
const DEFAULT_DURATION$O = 1000;
function rotateInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(rotateIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$O,
                degrees: (options && options.degrees) || -200
            }
        })
    ]);
}
function rotateInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(rotateIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$O,
                degrees: (options && options.degrees) || -200
            }
        })
    ]);
}

const rotateOutDownLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$P = rotateOutDownLeft;
const DEFAULT_DURATION$P = 1000;
function rotateOutDownLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutDownLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }), ...useAnimationIncludingChildren(rotateOutDownLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$P,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}
function rotateOutDownLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutDownLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }), ...useAnimationIncludingChildren(rotateOutDownLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$P,
                degrees: (options && options.degrees) || 45
            }
        })
    ]);
}

const rotateOutDownRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$Q = rotateOutDownRight;
const DEFAULT_DURATION$Q = 1000;
function rotateOutDownRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutDownRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }), ...useAnimationIncludingChildren(rotateOutDownRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Q,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}
function rotateOutDownRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutDownRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }), ...useAnimationIncludingChildren(rotateOutDownRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Q,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}

const rotateOutUpLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$R = rotateOutUpLeft;
const DEFAULT_DURATION$R = 1000;
function rotateOutUpLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutUpLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }), ...useAnimationIncludingChildren(rotateOutUpLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$R,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}
function rotateOutUpLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutUpLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left bottom' }), ...useAnimationIncludingChildren(rotateOutUpLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$R,
                degrees: (options && options.degrees) || -45
            }
        })
    ]);
}

const rotateOutUpRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$S = rotateOutUpRight;
const DEFAULT_DURATION$S = 1000;
function rotateOutUpRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutUpRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }), ...useAnimationIncludingChildren(rotateOutUpRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$S,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}
function rotateOutUpRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutUpRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right bottom' }), ...useAnimationIncludingChildren(rotateOutUpRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$S,
                degrees: (options && options.degrees) || 90
            }
        })
    ]);
}

const rotateOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'rotate({{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$T = rotateOut;
const DEFAULT_DURATION$T = 1000;
function rotateOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(rotateOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$T,
                degrees: (options && options.degrees) || 200
            }
        })
    ]);
}
function rotateOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotateOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center' }), ...useAnimationIncludingChildren(rotateOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$T,
                degrees: (options && options.degrees) || 200
            }
        })
    ]);
}

const slideInDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', transform: 'translate3d(0, -{{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$U = slideInDown;
const DEFAULT_DURATION$U = 1000;
function slideInDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$U,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideInDownOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInDownOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$U,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideInLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', transform: 'translate3d(-{{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$V = slideInLeft;
const DEFAULT_DURATION$V = 1000;
function slideInLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$V,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideInLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$V,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideInRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', transform: 'translate3d({{translate}}, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$W = slideInRight;
const DEFAULT_DURATION$W = 1000;
function slideInRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$W,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideInRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$W,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideInUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', transform: 'translate3d(0, {{translate}}, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$X = slideInUp;
const DEFAULT_DURATION$X = 1000;
function slideInUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$X,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideInUpOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideInUpOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(slideInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$X,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideOutDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, {{translate}}, 0)', visibility: 'hidden', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$Y = slideOutDown;
const DEFAULT_DURATION$Y = 1000;
function slideOutDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(slideOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Y,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideOutDownOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutDownOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(slideOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Y,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideOutLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(-{{translate}}, 0, 0)', visibility: 'hidden', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$Z = slideOutLeft;
const DEFAULT_DURATION$Z = 1000;
function slideOutLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(slideOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Z,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideOutLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(slideOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$Z,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideOutRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d({{translate}}, 0, 0)', visibility: 'hidden', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$_ = slideOutRight;
const DEFAULT_DURATION$_ = 1000;
function slideOutRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(slideOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$_,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideOutRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(slideOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$_,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const slideOutUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, 0, 0)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'translate3d(0, -{{translate}}, 0)', visibility: 'hidden', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$$ = slideOutUp;
const DEFAULT_DURATION$$ = 1000;
function slideOutUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(slideOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$$,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function slideOutUpOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'slideOutUpOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(slideOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$$,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

const zoomInDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, -1000px, 0)',
            easing: 'ease',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 0.6
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)', easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)', offset: 1 })
    ]))
]);
const ɵ0$10 = zoomInDown;
const DEFAULT_DURATION$10 = 1000;
function zoomInDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$10
            }
        })
    ]);
}
function zoomInDownOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInDownOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$10
            }
        })
    ]);
}

const zoomInLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(-3000px, 0, 0)',
            easing: 'ease',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(10px, 0, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 0.6
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)', easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)', offset: 1 })
    ]))
]);
const ɵ0$11 = zoomInLeft;
const DEFAULT_DURATION$11 = 1000;
function zoomInLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$11
            }
        })
    ]);
}
function zoomInLeftOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInLeftOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$11
            }
        })
    ]);
}

const zoomInRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(1000px, 0, 0)',
            easing: 'ease',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(-10px, 0, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 0.6
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)', easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)', offset: 1 })
    ]))
]);
const ɵ0$12 = zoomInRight;
const DEFAULT_DURATION$12 = 1000;
function zoomInRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$12
            }
        })
    ]);
}
function zoomInRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$12
            }
        })
    ]);
}

const zoomInUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, 1000px, 0)',
            easing: 'ease',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 0.6
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)', easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)', offset: 1 })
    ]))
]);
const ɵ0$13 = zoomInUp;
const DEFAULT_DURATION$13 = 1000;
function zoomInUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$13
            }
        })
    ]);
}
function zoomInUpOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInUpOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomInUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$13
            }
        })
    ]);
}

const zoomIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', transform: 'scale3d(0.3, 0.3, 0.3)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 1 })
    ]))
]));
const ɵ0$14 = zoomIn;
const DEFAULT_DURATION$14 = 1000;
function zoomInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$14
            }
        })
    ]);
}
function zoomInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(zoomIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$14
            }
        })
    ]);
}

const zoomOutDown = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            'transform-origin': 'center bottom',
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)',
            easing: 'ease',
            offset: 0.4
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            'transform-origin': 'center bottom',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, 2000px, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 1
        })
    ]))
]);
const ɵ0$15 = zoomOutDown;
const DEFAULT_DURATION$15 = 1000;
function zoomOutDownAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutDown', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(zoomOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$15
            }
        })
    ]);
}
function zoomOutDownOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutDownOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(zoomOutDown(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$15
            }
        })
    ]);
}

const zoomOutLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(0.475, 0.475, 0.475) translate3d(42px, 0, 0)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'scale3d(0.1, 0.1, 0.1) translate3d(-2000px, 0, 0)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center center', offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'left center', offset: 0.4 })]))
]));
const ɵ0$16 = zoomOutLeft;
const DEFAULT_DURATION$16 = 1000;
function zoomOutLeftAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutLeft', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(zoomOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$16
            }
        })
    ]);
}
function zoomOutLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(zoomOutLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$16
            }
        })
    ]);
}

const zoomOutRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(0.475, 0.475, 0.475) translate3d(-42px, 0, 0)', easing: 'ease', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'scale3d(0.1, 0.1, 0.1) translate3d(2000px, 0, 0)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center center', offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'right center', offset: 0.4 })]))
]));
const ɵ0$17 = zoomOutRight;
const DEFAULT_DURATION$17 = 1000;
function zoomOutRightAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutRight', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(zoomOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$17
            }
        })
    ]);
}
function zoomOutRightOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutRightOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(zoomOutRight(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$17
            }
        })
    ]);
}

const zoomOutUp = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            'transform-origin': 'center bottom',
            opacity: 1,
            transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)',
            easing: 'ease',
            offset: 0.4
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            'transform-origin': 'center bottom',
            opacity: 0,
            transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, -2000px, 0)',
            easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
            offset: 1
        })
    ]))
]);
const ɵ0$18 = zoomOutUp;
const DEFAULT_DURATION$18 = 1000;
function zoomOutUpAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutUp', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(zoomOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$18
            }
        })
    ]);
}
function zoomOutUpOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutUpOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(zoomOutUp(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$18
            }
        })
    ]);
}

const zoomOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'scale3d(0.3, 0.3, 0.3)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(1, 1, 1)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ transform: 'scale3d(0.3, 0.3, 0.3)', easing: 'ease', offset: 0.5 })
    ]))
]));
const ɵ0$19 = zoomOut;
const DEFAULT_DURATION$19 = 1000;
function zoomOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(zoomOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$19
            }
        })
    ]);
}
function zoomOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'zoomOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(zoomOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$19
            }
        })
    ]);
}

const hinge = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, 'transform-origin': 'top left', transform: 'translate3d(0, 0, 0)', easing: 'ease-in-out', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, 'transform-origin': 'top left', transform: 'rotate3d(0, 0, 1, 80deg)', easing: 'ease-in-out', offset: 0.2 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, 'transform-origin': 'top left', transform: 'rotate3d(0, 0, 1, 60deg)', easing: 'ease-in-out', offset: 0.4 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, 'transform-origin': 'top left', transform: 'rotate3d(0, 0, 1, 80deg)', easing: 'ease-in-out', offset: 0.6 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, 'transform-origin': 'top left', transform: 'rotate3d(0, 0, 1, 60deg)', easing: 'ease-in-out', offset: 0.8 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, 'transform-origin': 'top left', transform: 'translate3d(0, 700px, 0)', easing: 'ease-in-out', offset: 1 })
    ]))
]);
const ɵ0$1a = hinge;
const DEFAULT_DURATION$1a = 2000;
function hingeAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'hinge', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(hinge(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1a
            }
        })
    ]);
}
function hingeOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'hingeOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(hinge(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1a
            }
        })
    ]);
}

const jackInTheBox = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom', transform: 'scale(0.1) rotate(30deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom', transform: 'rotate(-10deg)', easing: 'ease', offset: 0.5 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom', transform: 'rotate(3deg)', easing: 'ease', offset: 0.7 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ 'transform-origin': 'center bottom', transform: 'scale(1)', easing: 'ease', offset: 1 })
    ])),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'visible', opacity: 0, offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, offset: 1 })]))
]));
const ɵ0$1b = jackInTheBox;
const DEFAULT_DURATION$1b = 1000;
function jackInTheBoxAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'jackInTheBox', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(jackInTheBox(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1b
            }
        })
    ]);
}
function jackInTheBoxOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'jackInTheBoxOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(jackInTheBox(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1b
            }
        })
    ]);
}

const rollIn = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            visibility: 'visible',
            opacity: 0,
            transform: 'translate3d({{translate}}, 0, 0) rotate3d(0, 0, 1, {{degrees}}deg)',
            easing: 'ease',
            offset: 0
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0) rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$1c = rollIn;
const DEFAULT_DURATION$1c = 1000;
function rollInAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rollIn', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(rollIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1c,
                degrees: (options && options.degrees) || -120,
                translate: (options && options.translate) || '-100%'
            }
        })
    ]);
}
function rollInOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rollInOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(rollIn(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1c,
                degrees: (options && options.degrees) || -120,
                translate: (options && options.translate) || '-100%'
            }
        })
    ]);
}

const rollOut = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 1, transform: 'translate3d(0, 0, 0) rotate3d(0, 0, 1, 0deg)', easing: 'ease', offset: 0 }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ opacity: 0, transform: 'translate3d({{translate}}, 0, 0) rotate3d(0, 0, 1, {{degrees}}deg)', easing: 'ease', offset: 1 })
    ]))
]);
const ɵ0$1d = rollOut;
const DEFAULT_DURATION$1d = 1000;
function rollOutAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rollOut', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...useAnimationIncludingChildren(rollOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1d,
                degrees: (options && options.degrees) || 120,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}
function rollOutOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rollOutOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(rollOut(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1d,
                degrees: (options && options.degrees) || 120,
                translate: (options && options.translate) || '100%'
            }
        })
    ]);
}

function animateChildrenOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'animateChildrenOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })])
    ]);
}

function animateIncludingChildren(easing, options) {
    return [
        ...(options && options.animateChildren === 'before' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : []),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + easing)]),
            ...(!options || !options.animateChildren || options.animateChildren === 'together'
                ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })]
                : [])
        ]),
        ...(options && options.animateChildren === 'after' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : [])
    ];
}

const DEFAULT_DURATION$1e = 200;
function collapseAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'collapse', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('1', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            height: '0',
            visibility: 'hidden',
            overflow: 'hidden'
        })),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('0', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"],
            visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"],
            overflow: 'hidden'
        })),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...animateIncludingChildren('ease-in', options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('1 => 0', [...animateIncludingChildren('ease-out', options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function collapseHorizontallyAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'collapseHorizontally', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('1', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            width: '0',
            visibility: 'hidden',
            overflow: 'hidden'
        })),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('0', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"],
            visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"],
            overflow: 'hidden'
        })),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [...animateIncludingChildren('ease-in', options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('1 => 0', [...animateIncludingChildren('ease-out', options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
const expand = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: '0', visibility: 'hidden', overflow: 'hidden', easing: 'ease-out', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-out', offset: 1 })
])));
const ɵ0$1e = expand;
const expandRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: '0', visibility: 'hidden', overflow: 'hidden', easing: 'ease-out', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-out', offset: 1 })
])));
const ɵ1 = expandRight;
const fadeInExpand = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: '0', opacity: 0, visibility: 'hidden', overflow: 'hidden', easing: 'ease-out', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-out', offset: 1 })
])));
const ɵ2 = fadeInExpand;
const fadeInExpandRight = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: '0', opacity: 0, visibility: 'hidden', overflow: 'hidden', easing: 'ease-out', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-out', offset: 1 })
])));
const ɵ3 = fadeInExpandRight;
const collapse = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-in', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: '0', visibility: 'hidden', overflow: 'hidden', easing: 'ease-in', offset: 1 })
])));
const ɵ4 = collapse;
const collapseLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-in', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: '0', visibility: 'hidden', overflow: 'hidden', easing: 'ease-in', offset: 1 })
])));
const ɵ5 = collapseLeft;
const fadeOutCollapse = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-in', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ height: '0', opacity: 0, visibility: 'hidden', overflow: 'hidden', easing: 'ease-in', offset: 1 })
])));
const ɵ6 = fadeOutCollapse;
const fadeOutCollapseLeft = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])(Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__["AUTO_STYLE"], overflow: 'hidden', easing: 'ease-in', offset: 0 }),
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ width: '0', opacity: 0, visibility: 'hidden', overflow: 'hidden', easing: 'ease-in', offset: 1 })
])));
const ɵ7 = fadeOutCollapseLeft;
function expandOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'expandOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(expand(), options)]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function expandRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'expandRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(expandRight(), options)]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function collapseOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'collapseOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(collapse(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function collapseLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'collapseLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(collapseLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function fadeInExpandOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInExpandOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInExpand(), options)]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function fadeInExpandRightOnEnterAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeInExpandRightOnEnter', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':enter', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ visibility: 'hidden' }), ...useAnimationIncludingChildren(fadeInExpandRight(), options)]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function fadeOutCollapseOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutCollapseOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutCollapse(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}
function fadeOutCollapseLeftOnLeaveAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'fadeOutCollapseLeftOnLeave', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])(':leave', [...useAnimationIncludingChildren(fadeOutCollapseLeft(), options)], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1e
            }
        })
    ]);
}

const hueRotate = () => Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animation"])([
    Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}ms {{delay}}ms', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["keyframes"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ filter: 'hue-rotate(0deg)', offset: 0 }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({ filter: 'hue-rotate(-360deg)', offset: 1 })]))
]);
const ɵ0$1f = hueRotate;
const DEFAULT_DURATION$1f = 3000;
function hueRotateAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'hueRotate', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 <=> 1', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["useAnimation"])(hueRotate())]), {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1f
            }
        })
    ]);
}

const DEFAULT_DURATION$1g = 200;
function rotateAnimation(options) {
    return Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["trigger"])((options && options.anchor) || 'rotate', [
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('0', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'rotate(0deg)'
        })),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["state"])('1', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["style"])({
            transform: 'rotate(' + '{{degrees}}' + 'deg)'
        }), {
            params: {
                degrees: (options && options.degrees) || 180
            }
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('0 => 1', [
            ...(options && options.animateChildren === 'before' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : []),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + 'ease')]),
                ...(!options || !options.animateChildren || options.animateChildren === 'together'
                    ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })]
                    : [])
            ]),
            ...(options && options.animateChildren === 'after' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : [])
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1g
            }
        }),
        Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["transition"])('1 => 0', [
            ...(options && options.animateChildren === 'before' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : []),
            Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([
                Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["group"])([Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true }), Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animate"])('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + 'ease')]),
                ...(!options || !options.animateChildren || options.animateChildren === 'together'
                    ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })]
                    : [])
            ]),
            ...(options && options.animateChildren === 'after' ? [Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["query"])('@*', Object(_angular_animations__WEBPACK_IMPORTED_MODULE_0__["animateChild"])(), { optional: true })] : [])
        ], {
            params: {
                delay: (options && options.delay) || 0,
                duration: (options && options.duration) || DEFAULT_DURATION$1g
            }
        })
    ]);
}

/**
 * Generated bundle index. Do not edit.
 */



//# sourceMappingURL=angular-animations.js.map

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
/* harmony import */ var _services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../../services/app-current-metadata.service */ "QI1B");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");










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
                this.vscodePath = settings.filter(_ => _.name === "EditorPath")[0].valueString || null;
                this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
                this.plantumlLocalPath = settings.filter(_ => _.name === "PlantumlLocalPath")[0].valueString || null;
                this.javaPath = settings.filter(_ => _.name === "JavaPath")[0].valueString || null;
                this.localGraphvizDotPath = settings.filter(_ => _.name === "LocalGraphvizDotPath")[0].valueString || null;
            }
        });
    }
    save() {
        this._settings.filter(_ => _.name === "EditorPath")[0].valueString = this.vscodePath;
        this._settings.filter(_ => _.name === "JiraServer")[0].valueString = this.jiraServer;
        this._settings.filter(_ => _.name === "PlantumlLocalPath")[0].valueString = this.plantumlLocalPath;
        this._settings.filter(_ => _.name === "JavaPath")[0].valueString = this.javaPath;
        this._settings.filter(_ => _.name === "LocalGraphvizDotPath")[0].valueString = this.localGraphvizDotPath;
        // Pass the updated settings to the service
        this.appCurrentFolder.saveSettings(this._settings).subscribe(data => {
            this._snackBar.open("settings saved", "", { duration: 1000 });
        });
        this.dialogRef.close(null);
    }
    dismiss() {
        this.dialogRef.close(null);
    }
}
SettingsComponent.ɵfac = function SettingsComponent_Factory(t) { return new (t || SettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_services_app_current_metadata_service__WEBPACK_IMPORTED_MODULE_1__["AppCurrentMetadataService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__["MatSnackBar"])); };
SettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SettingsComponent, selectors: [["app-settings"]], decls: 46, vars: 5, consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/IconReady.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matInput", "", "placeholder", "Visual studio code path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "plantuml local path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "Java folder", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "LocalGraphvizDot Path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "Jira path", 3, "ngModel", "ngModelChange"], ["mat-button", "", "color", "primary", 3, "click"]], template: function SettingsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](1, "img", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "MdExplorer Settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "mat-card", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "Visual studio code server path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "input", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_10_listener($event) { return ctx.vscodePath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12, "Set visual studio Path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](15, "Plantuml local path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "input", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_16_listener($event) { return ctx.plantumlLocalPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18, "Set path for plantuml server");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](19, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](20, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](21, "Java path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](22, "input", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_22_listener($event) { return ctx.javaPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](24, "Set java path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](25, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](26, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](27, "LocalGraphvizDot path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](28, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_28_listener($event) { return ctx.localGraphvizDotPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](29, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](30, "Set graphiz internal path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](31, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](32, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](33, "Jira path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](34, "input", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_34_listener($event) { return ctx.jiraServer = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](35, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](36, "Set http to jira");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](37, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](38, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_38_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](39, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](40, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](41, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](42, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_42_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](43, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](44, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](45, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.vscodePath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.plantumlLocalPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.javaPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.localGraphvizDotPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.jiraServer);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogContent"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCard"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_6__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_7__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__["MatIcon"]], styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.vertical-form-container[_ngcontent-%COMP%]    > *[_ngcontent-%COMP%] {\n  width: 100%;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxXQUFBO0FBQ0YiLCJmaWxlIjoic2V0dGluZ3MuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIudmVydGljYWwtZm9ybS1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxufVxyXG5cclxuLnZlcnRpY2FsLWZvcm0tY29udGFpbmVyID4gKiB7XHJcbiAgd2lkdGg6IDEwMCVcclxufVxyXG4iXX0= */"], data: { animation: [] } });


/***/ }),

/***/ "Trdj":
/*!***********************************!*\
  !*** ./src/app/git/git.module.ts ***!
  \***********************************/
/*! exports provided: GitModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitModule", function() { return GitModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./components/git-messages/git-messages.component */ "jwHG");
/* harmony import */ var _dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/commit-message-dialog/commit-message-dialog.component */ "4cNg");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/core */ "fXoL");






class GitModule {
}
GitModule.ɵfac = function GitModule_Factory(t) { return new (t || GitModule)(); };
GitModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵdefineNgModule"]({ type: GitModule });
GitModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵdefineInjector"]({ imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_3__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_4__["FormsModule"]
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵsetNgModuleScope"](GitModule, { declarations: [_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_1__["GitMessagesComponent"],
        _dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_2__["CommitMessageDialogComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_3__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_4__["FormsModule"]], exports: [_dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_2__["CommitMessageDialogComponent"]] }); })();


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


/***/ })

}]);
//# sourceMappingURL=default~md-explorer-md-explorer-module~projects-projects-module.js.map