(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["default~md-explorer-md-explorer-module~projects-projects-module"],{

/***/ "/DYt":
/*!***********************************************!*\
  !*** ./src/app/git/models/git-init.models.ts ***!
  \***********************************************/
/*! exports provided: GITIGNORE_TEMPLATES */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GITIGNORE_TEMPLATES", function() { return GITIGNORE_TEMPLATES; });
/**
 * Available .gitignore templates
 */
const GITIGNORE_TEMPLATES = [
    {
        value: 'mdexplorer',
        label: 'MdExplorer (Default)',
        description: 'Ignores .md/, database files, logs, and temporary files'
    },
    {
        value: 'node',
        label: 'Node.js',
        description: 'Ignores node_modules/, npm logs, build outputs, and .env files'
    },
    {
        value: 'python',
        label: 'Python',
        description: 'Ignores __pycache__/, virtual environments, and distribution files'
    },
    {
        value: 'csharp',
        label: '.NET / C#',
        description: 'Ignores bin/, obj/, Visual Studio files, and build outputs'
    },
    {
        value: 'none',
        label: 'None',
        description: 'Creates an empty .gitignore file'
    }
];


/***/ }),

/***/ "3YvM":
/*!******************************************************************************!*\
  !*** ./src/app/git/dialogs/git-branch-dialog/git-branch-dialog.component.ts ***!
  \******************************************************************************/
/*! exports provided: GitBranchDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitBranchDialogComponent", function() { return GitBranchDialogComponent; });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! tslib */ "mrSG");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../signalR/services/server-messages.service */ "+dpY");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_tabs__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/tabs */ "wZkO");
/* harmony import */ var _angular_material_list__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/list */ "MutI");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/core */ "FKr1");



















function GitBranchDialogComponent_mat_card_5_div_10_mat_chip_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx_r7.currentBranch.howManyFilesAreChanged, " file modificati ");
} }
function GitBranchDialogComponent_mat_card_5_div_10_mat_chip_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "upload");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx_r8.currentBranch.howManyCommitAreToPush, " commit da pushare ");
} }
function GitBranchDialogComponent_mat_card_5_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-chip-list");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](2, GitBranchDialogComponent_mat_card_5_div_10_mat_chip_2_Template, 4, 1, "mat-chip", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, GitBranchDialogComponent_mat_card_5_div_10_mat_chip_3_Template, 4, 1, "mat-chip", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r6.currentBranch.somethingIsChangedInTheBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r6.currentBranch.howManyCommitAreToPush > 0);
} }
function GitBranchDialogComponent_mat_card_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-card", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-icon", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "span", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7, "Branch corrente:");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](10, GitBranchDialogComponent_mat_card_5_div_10_Template, 4, 2, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r0.currentBranch.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r0.currentBranch.somethingIsChangedInTheBranch || ctx_r0.currentBranch.howManyCommitAreToPush > 0);
} }
function GitBranchDialogComponent_mat_form_field_6_button_6_Template(rf, ctx) { if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_mat_form_field_6_button_6_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r11); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2); ctx_r10.searchTerm = ""; return ctx_r10.filterBranches(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitBranchDialogComponent_mat_form_field_6_Template(rf, ctx) { if (rf & 1) {
    const _r13 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-form-field", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "Cerca branch");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "input", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitBranchDialogComponent_mat_form_field_6_Template_input_ngModelChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r13); const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r12.searchTerm = $event; })("input", function GitBranchDialogComponent_mat_form_field_6_Template_input_input_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r13); const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r14.filterBranches(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "mat-icon", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, GitBranchDialogComponent_mat_form_field_6_button_6_Template, 3, 0, "button", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.searchTerm);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r1.searchTerm);
} }
function GitBranchDialogComponent_div_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](1, "mat-spinner", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "Caricamento branch...");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitBranchDialogComponent_mat_card_8_Template(rf, ctx) { if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-card", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-icon", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_mat_card_8_Template_button_click_6_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r16); const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r15.loadBranches(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](8, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9, " Riprova ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r3.error);
} }
function GitBranchDialogComponent_div_9_mat_tab_4_ng_template_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-icon", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "span", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const group_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](group_r19.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](group_r19.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"]("(", group_r19.branches.length, ")");
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_span_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "CORRENTE");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_1_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", branch_r24.upstreamBranch, " ");
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_2_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "arrow_upward");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", branch_r24.ahead, " ahead ");
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "arrow_downward");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", branch_r24.behind, " behind ");
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](1, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_1_Template, 4, 1, "span", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](2, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_2_Template, 4, 1, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_span_3_Template, 4, 1, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", branch_r24.upstreamBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", branch_r24.ahead);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", branch_r24.behind);
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_button_8_Template(rf, ctx) { if (rf & 1) {
    const _r37 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_button_8_Template_button_click_0_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r37); const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3); ctx_r35.switchToBranch(branch_r24); return $event.stopPropagation(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "arrow_forward");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx_r27.isSwitching);
} }
function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_Template(rf, ctx) { if (rf & 1) {
    const _r39 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-list-item", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_Template_mat_list_item_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r39); const branch_r24 = ctx.$implicit; const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3); return ctx_r38.switchToBranch(branch_r24); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "div", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_span_6_Template, 2, 0, "span", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](7, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_div_7_Template, 4, 3, "div", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](8, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_button_8_Template, 3, 1, "button", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const branch_r24 = ctx.$implicit;
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("current", branch_r24.isCurrentBranch)("remote", branch_r24.isRemote)("local", !branch_r24.isRemote)("disabled", ctx_r21.isSwitching);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("color", branch_r24.isCurrentBranch ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", branch_r24.isCurrentBranch ? "check_circle" : branch_r24.isRemote ? "cloud" : "computer", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](branch_r24.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", branch_r24.isCurrentBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", branch_r24.upstreamBranch || branch_r24.ahead || branch_r24.behind);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !branch_r24.isCurrentBranch);
} }
function GitBranchDialogComponent_div_9_mat_tab_4_div_4_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "search_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "Nessun branch trovato in questa categoria");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitBranchDialogComponent_div_9_mat_tab_4_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](1, GitBranchDialogComponent_div_9_mat_tab_4_ng_template_1_Template, 6, 3, "ng-template", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-list", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, GitBranchDialogComponent_div_9_mat_tab_4_mat_list_item_3_Template, 9, 14, "mat-list-item", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, GitBranchDialogComponent_div_9_mat_tab_4_div_4_Template, 5, 0, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const group_r19 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", group_r19.branches);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", group_r19.branches.length === 0);
} }
function GitBranchDialogComponent_div_9_div_5_button_5_Template(rf, ctx) { if (rf & 1) {
    const _r42 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_9_div_5_button_5_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r42); const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](3); ctx_r41.searchTerm = ""; return ctx_r41.filterBranches(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, " Cancella ricerca ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitBranchDialogComponent_div_9_div_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "search_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "Nessun branch trovato");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, GitBranchDialogComponent_div_9_div_5_button_5_Template, 2, 0, "button", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r18.searchTerm);
} }
function GitBranchDialogComponent_div_9_Template(rf, ctx) { if (rf & 1) {
    const _r44 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "div", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-tab-group", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("selectedIndexChange", function GitBranchDialogComponent_div_9_Template_mat_tab_group_selectedIndexChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r44); const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r43.selectedTabIndex = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, GitBranchDialogComponent_div_9_mat_tab_4_Template, 5, 2, "mat-tab", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, GitBranchDialogComponent_div_9_div_5_Template, 6, 1, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx_r4.filteredBranches.length, " branch trovati ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("selectedIndex", ctx_r4.selectedTabIndex);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx_r4.branchGroups);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r4.branchGroups.length === 0);
} }
function GitBranchDialogComponent_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](1, "mat-spinner", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "Cambio branch in corso...");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
class GitBranchDialogComponent {
    constructor(dialogRef, data, gitService, snackBar, dialog, serverMessages) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitService = gitService;
        this.snackBar = snackBar;
        this.dialog = dialog;
        this.serverMessages = serverMessages;
        this.currentBranch = null;
        this.branches = [];
        this.filteredBranches = [];
        this.branchGroups = [];
        this.selectedTabIndex = 0;
        this.searchTerm = '';
        this.isLoading = true;
        this.isSwitching = false;
        this.error = null;
    }
    ngOnInit() {
        this.loadBranchInfo();
        this.loadBranches();
    }
    loadBranchInfo() {
        // Subscribe to current branch data
        this.gitService.currentBranch$.subscribe(branch => {
            if (branch && branch.name) {
                this.currentBranch = branch;
            }
        });
        // Trigger branch status update
        this.gitService.modernGetBranchStatus(this.data.projectPath).subscribe({
            next: (branch) => {
                this.currentBranch = branch;
                // Update the BehaviorSubject
                this.gitService.currentBranch$.next(branch);
            },
            error: (err) => {
                this.error = 'Errore nel caricamento delle informazioni del branch';
                console.error('Error loading branch info:', err);
            }
        });
    }
    loadBranches() {
        this.isLoading = true;
        this.error = null;
        this.gitService.getBranches(this.data.projectPath, true).subscribe({
            next: (branches) => {
                this.branches = branches;
                this.filteredBranches = this.sortBranches(branches);
                this.groupBranchesBySource(branches);
                this.isLoading = false;
            },
            error: (err) => {
                this.isLoading = false;
                this.error = 'Errore nel caricamento della lista branch';
                console.error('Error loading branches:', err);
            }
        });
    }
    groupBranchesBySource(branches) {
        const groups = new Map();
        // Group local branches
        const localBranches = branches.filter(b => !b.isRemote);
        if (localBranches.length > 0) {
            groups.set('local', this.sortBranches(localBranches));
        }
        // Group remote branches by remoteName
        const remoteBranches = branches.filter(b => b.isRemote);
        remoteBranches.forEach(branch => {
            const remoteName = branch.remoteName || 'origin';
            if (!groups.has(remoteName)) {
                groups.set(remoteName, []);
            }
            groups.get(remoteName).push(branch);
        });
        // Sort remote branches within each group
        groups.forEach((branchList, key) => {
            if (key !== 'local') {
                groups.set(key, this.sortBranches(branchList));
            }
        });
        // Build branchGroups array with local first, then remotes alphabetically
        this.branchGroups = [];
        if (groups.has('local')) {
            this.branchGroups.push({
                name: 'Local',
                icon: 'computer',
                branches: groups.get('local')
            });
        }
        // Add remote groups sorted alphabetically
        const remoteKeys = Array.from(groups.keys())
            .filter(k => k !== 'local')
            .sort();
        remoteKeys.forEach(remoteName => {
            this.branchGroups.push({
                name: remoteName,
                icon: 'cloud',
                branches: groups.get(remoteName)
            });
        });
    }
    sortBranches(branches) {
        return branches.sort((a, b) => {
            // Current branch first
            if (a.isCurrentBranch)
                return -1;
            if (b.isCurrentBranch)
                return 1;
            // Then local branches
            if (!a.isRemote && b.isRemote)
                return -1;
            if (a.isRemote && !b.isRemote)
                return 1;
            // Alphabetically
            return a.name.localeCompare(b.name);
        });
    }
    filterBranches() {
        const term = this.searchTerm.toLowerCase();
        if (!term) {
            this.filteredBranches = this.sortBranches(this.branches);
            this.groupBranchesBySource(this.branches);
        }
        else {
            const filtered = this.branches.filter(b => b.name.toLowerCase().includes(term));
            this.filteredBranches = this.sortBranches(filtered);
            this.groupBranchesBySource(filtered);
        }
    }
    switchToBranch(branch) {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            if (branch.isCurrentBranch) {
                this.snackBar.open('Sei già su questo branch', 'OK', { duration: 2000 });
                return;
            }
            if (this.isSwitching) {
                return; // Prevent multiple simultaneous switches
            }
            // Check for uncommitted changes
            this.gitService.getRepositoryStatus(this.data.projectPath).subscribe({
                next: (status) => {
                    var _a;
                    const hasChanges = status.hasChanges ||
                        ((_a = this.currentBranch) === null || _a === void 0 ? void 0 : _a.somethingIsChangedInTheBranch);
                    if (hasChanges) {
                        this.showUncommittedChangesDialog(branch);
                    }
                    else {
                        this.performCheckout(branch);
                    }
                },
                error: (err) => {
                    console.error('Error checking status:', err);
                    // Proceed anyway
                    this.performCheckout(branch);
                }
            });
        });
    }
    showUncommittedChangesDialog(branch) {
        var _a;
        const changesCount = ((_a = this.currentBranch) === null || _a === void 0 ? void 0 : _a.howManyFilesAreChanged) || 0;
        const message = `Hai ${changesCount} file${changesCount !== 1 ? 's' : ''} modificati non committati. Cambiando branch potresti perdere le modifiche.`;
        const confirmed = confirm(`${message}\n\nVuoi comunque cambiare branch?`);
        if (confirmed) {
            this.performCheckout(branch, true);
        }
    }
    performCheckout(branch, force = false) {
        this.isSwitching = true;
        this.error = null;
        // Extract branch name (remove remote prefix if present)
        let branchName = branch.name;
        if (branch.isRemote) {
            // For remote branches, remove the remote prefix (e.g., "origin/test" -> "test")
            // Handle both cases: when remoteName is set or when we need to detect it
            if (branch.remoteName) {
                branchName = branch.name.replace(`${branch.remoteName}/`, '');
            }
            else {
                // Fallback: remove everything before the last slash
                const slashIndex = branch.name.indexOf('/');
                if (slashIndex !== -1) {
                    branchName = branch.name.substring(slashIndex + 1);
                }
            }
        }
        this.gitService.checkoutBranch(this.data.projectPath, branchName, this.serverMessages.connectionId).subscribe({
            next: (result) => {
                this.isSwitching = false;
                if (result.success) {
                    const actualBranchName = result.branchName || branchName;
                    this.snackBar.open(`Switchato a branch: ${actualBranchName}`, 'OK', { duration: 3000, panelClass: ['success-snackbar'] });
                    // If backend returned branch name, use it directly to update current branch
                    if (result.branchName) {
                        this.gitService.currentBranch$.next({
                            id: '',
                            name: result.branchName,
                            somethingIsChangedInTheBranch: false,
                            howManyFilesAreChanged: 0,
                            howManyCommitAreToPush: 0,
                            fullPath: this.data.projectPath
                        });
                    }
                    // Refresh branch list
                    this.loadBranches();
                }
                else {
                    this.error = result.error || 'Errore durante il cambio branch';
                    this.snackBar.open(this.error, 'OK', { duration: 5000, panelClass: ['error-snackbar'] });
                }
            },
            error: (err) => {
                this.isSwitching = false;
                this.error = 'Errore di rete durante il cambio branch';
                console.error('Checkout error:', err);
                this.snackBar.open(this.error, 'OK', { duration: 5000, panelClass: ['error-snackbar'] });
            }
        });
    }
    onClose() {
        this.dialogRef.close();
    }
}
GitBranchDialogComponent.ɵfac = function GitBranchDialogComponent_Factory(t) { return new (t || GitBranchDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_3__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__["MdServerMessagesService"])); };
GitBranchDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: GitBranchDialogComponent, selectors: [["app-git-branch-dialog"]], decls: 16, vars: 8, consts: [["mat-dialog-title", ""], [1, "dialog-icon"], [1, "branch-dialog-content"], ["class", "current-branch-card", 4, "ngIf"], ["class", "search-field", "appearance", "outline", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "error-card", 4, "ngIf"], ["class", "branch-list-container", 4, "ngIf"], ["class", "switching-overlay", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], [1, "current-branch-card"], [1, "current-branch-header"], ["color", "primary"], [1, "branch-details"], [1, "current-label"], [1, "current-branch-name"], ["class", "branch-stats", 4, "ngIf"], [1, "branch-stats"], ["color", "warn", "selected", "", 4, "ngIf"], ["color", "accent", "selected", "", 4, "ngIf"], ["color", "warn", "selected", ""], ["color", "accent", "selected", ""], ["appearance", "outline", 1, "search-field"], ["matInput", "", "placeholder", "Nome del branch...", "autocomplete", "off", 3, "ngModel", "ngModelChange", "input"], ["matPrefix", ""], ["mat-icon-button", "", "matSuffix", "", "aria-label", "Clear", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matSuffix", "", "aria-label", "Clear", 3, "click"], [1, "loading-container"], ["diameter", "50"], [1, "error-card"], ["color", "warn"], ["mat-raised-button", "", "color", "primary", 3, "click"], [1, "branch-list-container"], [1, "branch-count"], [1, "branch-tabs", 3, "selectedIndex", "selectedIndexChange"], [4, "ngFor", "ngForOf"], ["class", "empty-state", 4, "ngIf"], ["mat-tab-label", ""], [1, "branch-list"], ["class", "branch-item", 3, "current", "remote", "local", "disabled", "click", 4, "ngFor", "ngForOf"], [1, "tab-icon"], [1, "tab-label"], [1, "tab-count"], [1, "branch-item", 3, "click"], ["matListIcon", "", 3, "color"], ["matLine", "", 1, "branch-name-line"], [1, "branch-name"], ["class", "branch-badge current-badge", 4, "ngIf"], ["matLine", "", "class", "branch-info-line", 4, "ngIf"], ["mat-icon-button", "", "class", "switch-button", 3, "disabled", "click", 4, "ngIf"], [1, "branch-badge", "current-badge"], ["matLine", "", 1, "branch-info-line"], ["class", "upstream-info", 4, "ngIf"], ["class", "sync-info", 4, "ngIf"], [1, "upstream-info"], [1, "small-icon"], [1, "sync-info"], ["mat-icon-button", "", 1, "switch-button", 3, "disabled", "click"], [1, "empty-state"], ["mat-button", "", 3, "click", 4, "ngIf"], ["mat-button", "", 3, "click"], [1, "switching-overlay"], ["diameter", "40"]], template: function GitBranchDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "account_tree");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "mat-dialog-content", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, GitBranchDialogComponent_mat_card_5_Template, 11, 2, "mat-card", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, GitBranchDialogComponent_mat_form_field_6_Template, 7, 2, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](7, GitBranchDialogComponent_div_7_Template, 4, 0, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](8, GitBranchDialogComponent_mat_card_8_Template, 10, 1, "mat-card", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](9, GitBranchDialogComponent_div_9_Template, 6, 4, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](10, GitBranchDialogComponent_div_10_Template, 4, 0, "div", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "mat-dialog-actions", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitBranchDialogComponent_Template_button_click_12_listener() { return ctx.onClose(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](13, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](14, "close");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](15, " Chiudi ");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" Branch Manager ", ctx.data.projectName ? "- " + ctx.data.projectName : "", "\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.currentBranch && !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.error && !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.error);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isSwitching);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.isSwitching);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogTitle"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__["MatIcon"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_1__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardContent"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_10__["MatChipList"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_10__["MatChip"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_12__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatPrefix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_11__["MatSuffix"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_14__["MatSpinner"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_15__["MatTabGroup"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgForOf"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_15__["MatTab"], _angular_material_tabs__WEBPACK_IMPORTED_MODULE_15__["MatTabLabel"], _angular_material_list__WEBPACK_IMPORTED_MODULE_16__["MatList"], _angular_material_list__WEBPACK_IMPORTED_MODULE_16__["MatListItem"], _angular_material_list__WEBPACK_IMPORTED_MODULE_16__["MatListIconCssMatStyler"], _angular_material_core__WEBPACK_IMPORTED_MODULE_17__["MatLine"]], styles: ["h2.mat-dialog-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin: 0;\n  padding: 16px 24px;\n  border-bottom: 1px solid #e0e0e0;\n}\nh2.mat-dialog-title[_ngcontent-%COMP%]   .dialog-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  height: 28px;\n  width: 28px;\n}\n.branch-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 !important;\n  min-width: 600px;\n  min-height: 400px;\n  max-height: 70vh;\n  display: flex;\n  flex-direction: column;\n  position: relative;\n}\n.current-branch-card[_ngcontent-%COMP%] {\n  margin: 16px 24px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n}\n.current-branch-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%] {\n  padding: 16px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin-bottom: 12px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 32px;\n  height: 32px;\n  width: 32px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%]   .current-label[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n  opacity: 0.9;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%]   .current-branch-name[_ngcontent-%COMP%] {\n  font-size: 1.3em;\n  font-weight: 600;\n  font-family: \"Courier New\", monospace;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip-list[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip[_ngcontent-%COMP%] {\n  background-color: rgba(255, 255, 255, 0.2) !important;\n  color: white !important;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  margin-right: 4px;\n}\n.search-field[_ngcontent-%COMP%] {\n  margin: 16px 24px 0;\n  width: calc(100% - 48px);\n}\n.search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n}\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 40px;\n  gap: 16px;\n}\n.loading-container[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: #666;\n  font-size: 1.1em;\n}\n.error-card[_ngcontent-%COMP%] {\n  margin: 16px 24px;\n  border: 2px solid #f44336;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  gap: 16px;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  height: 48px;\n  width: 48px;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: #f44336;\n  font-size: 1.1em;\n  margin: 0;\n}\n.branch-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  margin: 0 24px 16px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-count[_ngcontent-%COMP%] {\n  padding: 8px 16px;\n  background-color: #f5f5f5;\n  border-radius: 4px 4px 0 0;\n  font-size: 0.9em;\n  color: #666;\n  font-weight: 500;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%] {\n  border: 1px solid #e0e0e0;\n  border-top: none;\n  border-radius: 0 0 4px 4px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-header {\n  background-color: #fafafa;\n  border-bottom: 1px solid #e0e0e0;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-label {\n  min-width: 120px;\n  padding: 0 16px;\n  opacity: 0.8;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-label.mat-tab-label-active {\n  opacity: 1;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-body-wrapper {\n  max-height: calc(70vh - 300px);\n  overflow-y: auto;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-count[_ngcontent-%COMP%] {\n  margin-left: 4px;\n  font-size: 0.85em;\n  color: #666;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-list[_ngcontent-%COMP%] {\n  padding: 0;\n}\n.branch-item[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.2s ease;\n  border-bottom: 1px solid #f0f0f0;\n  padding: 12px 16px !important;\n  height: auto !important;\n  min-height: 60px;\n}\n.branch-item[_ngcontent-%COMP%]:hover:not(.disabled) {\n  background-color: #f5f5f5;\n}\n.branch-item.current[_ngcontent-%COMP%] {\n  background-color: #e3f2fd;\n  border-left: 4px solid #2196f3;\n}\n.branch-item.current[_ngcontent-%COMP%]:hover {\n  background-color: #bbdefb;\n}\n.branch-item.disabled[_ngcontent-%COMP%] {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.branch-item[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n.branch-item[_ngcontent-%COMP%]   .mat-list-item-content[_ngcontent-%COMP%] {\n  padding: 0 !important;\n  height: auto !important;\n  display: flex;\n  align-items: center;\n  gap: 12px;\n}\n.branch-item[_ngcontent-%COMP%]   mat-icon[matListIcon][_ngcontent-%COMP%] {\n  margin-right: 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 4px 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-name[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 1.05em;\n  font-weight: 500;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge[_ngcontent-%COMP%] {\n  font-size: 0.7em;\n  padding: 2px 6px;\n  border-radius: 3px;\n  font-weight: 600;\n  letter-spacing: 0.5px;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge.current-badge[_ngcontent-%COMP%] {\n  background-color: #2196f3;\n  color: white;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge.remote-badge[_ngcontent-%COMP%] {\n  background-color: #9e9e9e;\n  color: white;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 0.85em;\n  color: #666;\n  padding: 2px 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .upstream-info[_ngcontent-%COMP%], .branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .sync-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .small-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  height: 16px;\n  width: 16px;\n}\n.branch-item[_ngcontent-%COMP%]   .switch-button[_ngcontent-%COMP%] {\n  margin-left: auto;\n  color: #2196f3;\n}\n.branch-item[_ngcontent-%COMP%]   .switch-button[_ngcontent-%COMP%]:hover {\n  background-color: rgba(33, 150, 243, 0.1);\n}\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 40px;\n  text-align: center;\n  border: 1px solid #e0e0e0;\n  border-top: none;\n  border-radius: 0 0 4px 4px;\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  height: 64px;\n  width: 64px;\n  color: #bdbdbd;\n  margin-bottom: 16px;\n}\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1.1em;\n  color: #666;\n  margin-bottom: 16px;\n}\n.switching-overlay[_ngcontent-%COMP%] {\n  position: absolute;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background-color: rgba(255, 255, 255, 0.9);\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  z-index: 1000;\n}\n.switching-overlay[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1.1em;\n  color: #666;\n  font-weight: 500;\n}\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  border-top: 1px solid #e0e0e0;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n@media (max-width: 768px) {\n  .branch-dialog-content[_ngcontent-%COMP%] {\n    min-width: 90vw;\n    max-height: 80vh;\n  }\n\n  .current-branch-card[_ngcontent-%COMP%], .search-field[_ngcontent-%COMP%], .branch-list-container[_ngcontent-%COMP%] {\n    margin-left: 16px;\n    margin-right: 16px;\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LWJyYW5jaC1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7QUFBRjtBQUVFO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBQUo7QUFLQTtFQUNFLHFCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0Esa0JBQUE7QUFGRjtBQU1BO0VBQ0UsaUJBQUE7RUFDQSw2REFBQTtFQUNBLFlBQUE7QUFIRjtBQUtFO0VBQ0UsYUFBQTtBQUhKO0FBTUU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsbUJBQUE7QUFKSjtBQU1JO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBSk47QUFPSTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUFMTjtBQU9NO0VBQ0UsaUJBQUE7RUFDQSxZQUFBO0FBTFI7QUFRTTtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxxQ0FBQTtBQU5SO0FBWUk7RUFDRSxhQUFBO0VBQ0EsUUFBQTtBQVZOO0FBYUk7RUFDRSxxREFBQTtFQUNBLHVCQUFBO0FBWE47QUFhTTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLGlCQUFBO0FBWFI7QUFrQkE7RUFDRSxtQkFBQTtFQUNBLHdCQUFBO0FBZkY7QUFpQkU7RUFDRSxpQkFBQTtBQWZKO0FBb0JBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGtCQUFBO0VBQ0EsU0FBQTtBQWpCRjtBQW1CRTtFQUNFLFdBQUE7RUFDQSxnQkFBQTtBQWpCSjtBQXNCQTtFQUNFLGlCQUFBO0VBQ0EseUJBQUE7QUFuQkY7QUFxQkU7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFuQko7QUFxQkk7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7QUFuQk47QUFzQkk7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7RUFDQSxTQUFBO0FBcEJOO0FBMEJBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsbUJBQUE7QUF2QkY7QUF5QkU7RUFDRSxpQkFBQTtFQUNBLHlCQUFBO0VBQ0EsMEJBQUE7RUFDQSxnQkFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtBQXZCSjtBQTBCRTtFQUNFLHlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSwwQkFBQTtBQXhCSjtBQTJCTTtFQUNFLHlCQUFBO0VBQ0EsZ0NBQUE7QUF6QlI7QUE0Qk07RUFDRSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0FBMUJSO0FBNEJRO0VBQ0UsVUFBQTtBQTFCVjtBQThCTTtFQUNFLDhCQUFBO0VBQ0EsZ0JBQUE7QUE1QlI7QUFnQ0k7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQTlCTjtBQWlDSTtFQUNFLGdCQUFBO0FBL0JOO0FBa0NJO0VBQ0UsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLFdBQUE7QUFoQ047QUFvQ0U7RUFDRSxVQUFBO0FBbENKO0FBdUNBO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSw2QkFBQTtFQUNBLHVCQUFBO0VBQ0EsZ0JBQUE7QUFwQ0Y7QUFzQ0U7RUFDRSx5QkFBQTtBQXBDSjtBQXVDRTtFQUNFLHlCQUFBO0VBQ0EsOEJBQUE7QUFyQ0o7QUF1Q0k7RUFDRSx5QkFBQTtBQXJDTjtBQXlDRTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtBQXZDSjtBQTBDRTtFQUNFLG1CQUFBO0FBeENKO0FBMkNFO0VBQ0UscUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7QUF6Q0o7QUE0Q0U7RUFDRSxlQUFBO0FBMUNKO0FBNkNFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGNBQUE7QUEzQ0o7QUE2Q0k7RUFDRSxxQ0FBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7QUEzQ047QUE4Q0k7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHFCQUFBO0FBNUNOO0FBOENNO0VBQ0UseUJBQUE7RUFDQSxZQUFBO0FBNUNSO0FBK0NNO0VBQ0UseUJBQUE7RUFDQSxZQUFBO0FBN0NSO0FBa0RFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGlCQUFBO0VBQ0EsV0FBQTtFQUNBLGNBQUE7QUFoREo7QUFrREk7O0VBRUUsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQWhETjtBQW1ESTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQWpETjtBQXFERTtFQUNFLGlCQUFBO0VBQ0EsY0FBQTtBQW5ESjtBQXFESTtFQUNFLHlDQUFBO0FBbkROO0FBeURBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSx5QkFBQTtFQUNBLGdCQUFBO0VBQ0EsMEJBQUE7QUF0REY7QUF3REU7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxjQUFBO0VBQ0EsbUJBQUE7QUF0REo7QUF5REU7RUFDRSxnQkFBQTtFQUNBLFdBQUE7RUFDQSxtQkFBQTtBQXZESjtBQTREQTtFQUNFLGtCQUFBO0VBQ0EsTUFBQTtFQUNBLE9BQUE7RUFDQSxRQUFBO0VBQ0EsU0FBQTtFQUNBLDBDQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0FBekRGO0FBMkRFO0VBQ0UsZ0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUF6REo7QUE4REE7RUFDRSxrQkFBQTtFQUNBLDZCQUFBO0FBM0RGO0FBNkRFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQTNESjtBQWdFQTtFQUNFO0lBQ0UsZUFBQTtJQUNBLGdCQUFBO0VBN0RGOztFQWdFQTs7O0lBR0UsaUJBQUE7SUFDQSxrQkFBQTtFQTdERjtBQUNGIiwiZmlsZSI6ImdpdC1icmFuY2gtZGlhbG9nLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLy8gRGlhbG9nIFRpdGxlXHJcbmgyLm1hdC1kaWFsb2ctdGl0bGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDEycHg7XHJcbiAgbWFyZ2luOiAwO1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweDtcclxuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2UwZTBlMDtcclxuXHJcbiAgLmRpYWxvZy1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMjhweDtcclxuICAgIGhlaWdodDogMjhweDtcclxuICAgIHdpZHRoOiAyOHB4O1xyXG4gIH1cclxufVxyXG5cclxuLy8gRGlhbG9nIENvbnRlbnRcclxuLmJyYW5jaC1kaWFsb2ctY29udGVudCB7XHJcbiAgcGFkZGluZzogMCAhaW1wb3J0YW50O1xyXG4gIG1pbi13aWR0aDogNjAwcHg7XHJcbiAgbWluLWhlaWdodDogNDAwcHg7XHJcbiAgbWF4LWhlaWdodDogNzB2aDtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgcG9zaXRpb246IHJlbGF0aXZlO1xyXG59XHJcblxyXG4vLyBDdXJyZW50IEJyYW5jaCBTdW1tYXJ5IENhcmRcclxuLmN1cnJlbnQtYnJhbmNoLWNhcmQge1xyXG4gIG1hcmdpbjogMTZweCAyNHB4O1xyXG4gIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgY29sb3I6IHdoaXRlO1xyXG5cclxuICAubWF0LWNhcmQtY29udGVudCB7XHJcbiAgICBwYWRkaW5nOiAxNnB4O1xyXG4gIH1cclxuXHJcbiAgLmN1cnJlbnQtYnJhbmNoLWhlYWRlciB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogMTJweDtcclxuICAgIG1hcmdpbi1ib3R0b206IDEycHg7XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDMycHg7XHJcbiAgICAgIGhlaWdodDogMzJweDtcclxuICAgICAgd2lkdGg6IDMycHg7XHJcbiAgICB9XHJcblxyXG4gICAgLmJyYW5jaC1kZXRhaWxzIHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgICAgZ2FwOiA0cHg7XHJcblxyXG4gICAgICAuY3VycmVudC1sYWJlbCB7XHJcbiAgICAgICAgZm9udC1zaXplOiAwLjg1ZW07XHJcbiAgICAgICAgb3BhY2l0eTogMC45O1xyXG4gICAgICB9XHJcblxyXG4gICAgICAuY3VycmVudC1icmFuY2gtbmFtZSB7XHJcbiAgICAgICAgZm9udC1zaXplOiAxLjNlbTtcclxuICAgICAgICBmb250LXdlaWdodDogNjAwO1xyXG4gICAgICAgIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5icmFuY2gtc3RhdHMge1xyXG4gICAgbWF0LWNoaXAtbGlzdCB7XHJcbiAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgIGdhcDogOHB4O1xyXG4gICAgfVxyXG5cclxuICAgIG1hdC1jaGlwIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjIpICFpbXBvcnRhbnQ7XHJcbiAgICAgIGNvbG9yOiB3aGl0ZSAhaW1wb3J0YW50O1xyXG5cclxuICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgICAgbWFyZ2luLXJpZ2h0OiA0cHg7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi8vIFNlYXJjaCBGaWVsZFxyXG4uc2VhcmNoLWZpZWxkIHtcclxuICBtYXJnaW46IDE2cHggMjRweCAwO1xyXG4gIHdpZHRoOiBjYWxjKDEwMCUgLSA0OHB4KTtcclxuXHJcbiAgLm1hdC1mb3JtLWZpZWxkLXdyYXBwZXIge1xyXG4gICAgcGFkZGluZy1ib3R0b206IDA7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBMb2FkaW5nIENvbnRhaW5lclxyXG4ubG9hZGluZy1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDYwcHggNDBweDtcclxuICBnYXA6IDE2cHg7XHJcblxyXG4gIHAge1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgICBmb250LXNpemU6IDEuMWVtO1xyXG4gIH1cclxufVxyXG5cclxuLy8gRXJyb3IgQ2FyZFxyXG4uZXJyb3ItY2FyZCB7XHJcbiAgbWFyZ2luOiAxNnB4IDI0cHg7XHJcbiAgYm9yZGVyOiAycHggc29saWQgI2Y0NDMzNjtcclxuXHJcbiAgLm1hdC1jYXJkLWNvbnRlbnQge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgICBwYWRkaW5nOiA0MHB4O1xyXG4gICAgZ2FwOiAxNnB4O1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiA0OHB4O1xyXG4gICAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICAgIHdpZHRoOiA0OHB4O1xyXG4gICAgfVxyXG5cclxuICAgIHAge1xyXG4gICAgICBjb2xvcjogI2Y0NDMzNjtcclxuICAgICAgZm9udC1zaXplOiAxLjFlbTtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLy8gQnJhbmNoIExpc3QgQ29udGFpbmVyXHJcbi5icmFuY2gtbGlzdC1jb250YWluZXIge1xyXG4gIGZsZXg6IDE7XHJcbiAgb3ZlcmZsb3cteTogYXV0bztcclxuICBtYXJnaW46IDAgMjRweCAxNnB4O1xyXG5cclxuICAuYnJhbmNoLWNvdW50IHtcclxuICAgIHBhZGRpbmc6IDhweCAxNnB4O1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuICAgIGJvcmRlci1yYWRpdXM6IDRweCA0cHggMCAwO1xyXG4gICAgZm9udC1zaXplOiAwLjllbTtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICB9XHJcblxyXG4gIC5icmFuY2gtdGFicyB7XHJcbiAgICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG4gICAgYm9yZGVyLXRvcDogbm9uZTtcclxuICAgIGJvcmRlci1yYWRpdXM6IDAgMCA0cHggNHB4O1xyXG5cclxuICAgIDo6bmctZGVlcCB7XHJcbiAgICAgIC5tYXQtdGFiLWhlYWRlciB7XHJcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2ZhZmFmYTtcclxuICAgICAgICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2UwZTBlMDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLm1hdC10YWItbGFiZWwge1xyXG4gICAgICAgIG1pbi13aWR0aDogMTIwcHg7XHJcbiAgICAgICAgcGFkZGluZzogMCAxNnB4O1xyXG4gICAgICAgIG9wYWNpdHk6IDAuODtcclxuXHJcbiAgICAgICAgJi5tYXQtdGFiLWxhYmVsLWFjdGl2ZSB7XHJcbiAgICAgICAgICBvcGFjaXR5OiAxO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgLm1hdC10YWItYm9keS13cmFwcGVyIHtcclxuICAgICAgICBtYXgtaGVpZ2h0OiBjYWxjKDcwdmggLSAzMDBweCk7XHJcbiAgICAgICAgb3ZlcmZsb3cteTogYXV0bztcclxuICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIC50YWItaWNvbiB7XHJcbiAgICAgIG1hcmdpbi1yaWdodDogOHB4O1xyXG4gICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICB9XHJcblxyXG4gICAgLnRhYi1sYWJlbCB7XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICB9XHJcblxyXG4gICAgLnRhYi1jb3VudCB7XHJcbiAgICAgIG1hcmdpbi1sZWZ0OiA0cHg7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC44NWVtO1xyXG4gICAgICBjb2xvcjogIzY2NjtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5icmFuY2gtbGlzdCB7XHJcbiAgICBwYWRkaW5nOiAwO1xyXG4gIH1cclxufVxyXG5cclxuLy8gQnJhbmNoIExpc3QgSXRlbXNcclxuLmJyYW5jaC1pdGVtIHtcclxuICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgdHJhbnNpdGlvbjogYWxsIDAuMnMgZWFzZTtcclxuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgI2YwZjBmMDtcclxuICBwYWRkaW5nOiAxMnB4IDE2cHggIWltcG9ydGFudDtcclxuICBoZWlnaHQ6IGF1dG8gIWltcG9ydGFudDtcclxuICBtaW4taGVpZ2h0OiA2MHB4O1xyXG5cclxuICAmOmhvdmVyOm5vdCguZGlzYWJsZWQpIHtcclxuICAgIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcbiAgfVxyXG5cclxuICAmLmN1cnJlbnQge1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2UzZjJmZDtcclxuICAgIGJvcmRlci1sZWZ0OiA0cHggc29saWQgIzIxOTZmMztcclxuXHJcbiAgICAmOmhvdmVyIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2JiZGVmYjtcclxuICAgIH1cclxuICB9XHJcblxyXG4gICYuZGlzYWJsZWQge1xyXG4gICAgb3BhY2l0eTogMC41O1xyXG4gICAgY3Vyc29yOiBub3QtYWxsb3dlZDtcclxuICB9XHJcblxyXG4gICY6bGFzdC1jaGlsZCB7XHJcbiAgICBib3JkZXItYm90dG9tOiBub25lO1xyXG4gIH1cclxuXHJcbiAgLm1hdC1saXN0LWl0ZW0tY29udGVudCB7XHJcbiAgICBwYWRkaW5nOiAwICFpbXBvcnRhbnQ7XHJcbiAgICBoZWlnaHQ6IGF1dG8gIWltcG9ydGFudDtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiAxMnB4O1xyXG4gIH1cclxuXHJcbiAgbWF0LWljb25bbWF0TGlzdEljb25dIHtcclxuICAgIG1hcmdpbi1yaWdodDogMDtcclxuICB9XHJcblxyXG4gIC5icmFuY2gtbmFtZS1saW5lIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiA4cHg7XHJcbiAgICBwYWRkaW5nOiA0cHggMDtcclxuXHJcbiAgICAuYnJhbmNoLW5hbWUge1xyXG4gICAgICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xyXG4gICAgICBmb250LXNpemU6IDEuMDVlbTtcclxuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICAgIH1cclxuXHJcbiAgICAuYnJhbmNoLWJhZGdlIHtcclxuICAgICAgZm9udC1zaXplOiAwLjdlbTtcclxuICAgICAgcGFkZGluZzogMnB4IDZweDtcclxuICAgICAgYm9yZGVyLXJhZGl1czogM3B4O1xyXG4gICAgICBmb250LXdlaWdodDogNjAwO1xyXG4gICAgICBsZXR0ZXItc3BhY2luZzogMC41cHg7XHJcblxyXG4gICAgICAmLmN1cnJlbnQtYmFkZ2Uge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6ICMyMTk2ZjM7XHJcbiAgICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAmLnJlbW90ZS1iYWRnZSB7XHJcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogIzllOWU5ZTtcclxuICAgICAgICBjb2xvcjogd2hpdGU7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5icmFuY2gtaW5mby1saW5lIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiAxMnB4O1xyXG4gICAgZm9udC1zaXplOiAwLjg1ZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIHBhZGRpbmc6IDJweCAwO1xyXG5cclxuICAgIC51cHN0cmVhbS1pbmZvLFxyXG4gICAgLnN5bmMtaW5mbyB7XHJcbiAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICAgIGdhcDogNHB4O1xyXG4gICAgfVxyXG5cclxuICAgIC5zbWFsbC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAxNnB4O1xyXG4gICAgICBoZWlnaHQ6IDE2cHg7XHJcbiAgICAgIHdpZHRoOiAxNnB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnN3aXRjaC1idXR0b24ge1xyXG4gICAgbWFyZ2luLWxlZnQ6IGF1dG87XHJcbiAgICBjb2xvcjogIzIxOTZmMztcclxuXHJcbiAgICAmOmhvdmVyIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgzMywgMTUwLCAyNDMsIDAuMSk7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBFbXB0eSBTdGF0ZVxyXG4uZW1wdHktc3RhdGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDYwcHggNDBweDtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgYm9yZGVyOiAxcHggc29saWQgI2UwZTBlMDtcclxuICBib3JkZXItdG9wOiBub25lO1xyXG4gIGJvcmRlci1yYWRpdXM6IDAgMCA0cHggNHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDY0cHg7XHJcbiAgICBoZWlnaHQ6IDY0cHg7XHJcbiAgICB3aWR0aDogNjRweDtcclxuICAgIGNvbG9yOiAjYmRiZGJkO1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgZm9udC1zaXplOiAxLjFlbTtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICB9XHJcbn1cclxuXHJcbi8vIFN3aXRjaGluZyBPdmVybGF5XHJcbi5zd2l0Y2hpbmctb3ZlcmxheSB7XHJcbiAgcG9zaXRpb246IGFic29sdXRlO1xyXG4gIHRvcDogMDtcclxuICBsZWZ0OiAwO1xyXG4gIHJpZ2h0OiAwO1xyXG4gIGJvdHRvbTogMDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuOSk7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgZ2FwOiAxNnB4O1xyXG4gIHotaW5kZXg6IDEwMDA7XHJcblxyXG4gIHAge1xyXG4gICAgZm9udC1zaXplOiAxLjFlbTtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICB9XHJcbn1cclxuXHJcbi8vIERpYWxvZyBBY3Rpb25zXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgcGFkZGluZzogMTZweCAyNHB4O1xyXG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICB9XHJcbn1cclxuXHJcbi8vIFJlc3BvbnNpdmUgYWRqdXN0bWVudHNcclxuQG1lZGlhIChtYXgtd2lkdGg6IDc2OHB4KSB7XHJcbiAgLmJyYW5jaC1kaWFsb2ctY29udGVudCB7XHJcbiAgICBtaW4td2lkdGg6IDkwdnc7XHJcbiAgICBtYXgtaGVpZ2h0OiA4MHZoO1xyXG4gIH1cclxuXHJcbiAgLmN1cnJlbnQtYnJhbmNoLWNhcmQsXHJcbiAgLnNlYXJjaC1maWVsZCxcclxuICAuYnJhbmNoLWxpc3QtY29udGFpbmVyIHtcclxuICAgIG1hcmdpbi1sZWZ0OiAxNnB4O1xyXG4gICAgbWFyZ2luLXJpZ2h0OiAxNnB4O1xyXG4gIH1cclxufVxyXG4iXX0= */"] });


/***/ }),

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
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_4__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_5__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_3__["MatHint"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_8__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_9__["MatSpinner"]], styles: [".mat-dialog-container {\n  padding: 24px;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-form-field[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n\ntextarea[_ngcontent-%COMP%] {\n  font-family: monospace;\n  font-size: 14px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  padding: 0;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcY29tbWl0LW1lc3NhZ2UtZGlhbG9nLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtBQUNGOztBQUVBO0VBQ0UsY0FBQTtBQUNGOztBQUVBO0VBQ0UsZUFBQTtBQUNGOztBQUVBO0VBQ0Usc0JBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxnQkFBQTtFQUNBLFVBQUE7QUFDRiIsImZpbGUiOiJjb21taXQtbWVzc2FnZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyI6Om5nLWRlZXAgLm1hdC1kaWFsb2ctY29udGFpbmVyIHtcclxuICBwYWRkaW5nOiAyNHB4O1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIG1hcmdpbjogMjBweCAwO1xyXG59XHJcblxyXG5tYXQtZm9ybS1maWVsZCB7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG59XHJcblxyXG50ZXh0YXJlYSB7XHJcbiAgZm9udC1mYW1pbHk6IG1vbm9zcGFjZTtcclxuICBmb250LXNpemU6IDE0cHg7XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgbWFyZ2luLXRvcDogMTZweDtcclxuICBwYWRkaW5nOiAwO1xyXG59Il19 */"] });


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

/***/ "8cbT":
/*!******************************************************************************************!*\
  !*** ./src/app/git/dialogs/git-setup-remote-dialog/git-setup-remote-dialog.component.ts ***!
  \******************************************************************************************/
/*! exports provided: GitSetupRemoteDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitSetupRemoteDialogComponent", function() { return GitSetupRemoteDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../git-token-dialog/git-token-dialog.component */ "8zqs");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_radio__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/radio */ "QibW");
/* harmony import */ var _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/checkbox */ "bSwM");
/* harmony import */ var _angular_material_expansion__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/expansion */ "7EHt");
















function GitSetupRemoteDialogComponent_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](1, "mat-spinner", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "Caricamento configurazione...");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitSetupRemoteDialogComponent_div_4_div_6_Template(rf, ctx) { if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5, "Token GitHub non configurato!");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](6, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7, " Per creare automaticamente il repository, \u00E8 necessario configurare un Personal Access Token GitHub. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "a", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_div_4_div_6_Template_a_click_8_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r7); const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2); return ctx_r6.openTokenSettings(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9, "Configura token");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function GitSetupRemoteDialogComponent_div_4_div_42_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "URL Repository:");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r4.getGitHubUrl());
} }
function GitSetupRemoteDialogComponent_div_4_div_48_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r5.error);
} }
function GitSetupRemoteDialogComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5, " Configura il repository remoto GitHub per sincronizzare il tuo progetto. Il repository verr\u00E0 creato automaticamente su GitHub se non esiste gi\u00E0. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, GitSetupRemoteDialogComponent_div_4_div_6_Template, 10, 0, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "mat-form-field", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9, "Organizzazione GitHub / Username");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_input_ngModelChange_10_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r8.organization = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](12, "business");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](13, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](14, "Il nome della tua organizzazione GitHub o il tuo username");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](15, "mat-form-field", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](16, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](17, "Nome Repository");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](18, "input", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_input_ngModelChange_18_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r10.repositoryName = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](19, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](20, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](21, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](22, "Il nome del repository su GitHub (default: nome del progetto)");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](23, "mat-form-field", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](25, "Descrizione Repository (opzionale)");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](26, "textarea", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_textarea_ngModelChange_26_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r11.repositoryDescription = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](27, "      ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](28, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](29, "description");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](30, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](31, "mat-label", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](32, "Visibilit\u00E0 Repository:");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](33, "mat-radio-group", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_mat_radio_group_ngModelChange_33_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r12.isPrivate = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](34, "mat-radio-button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](35, "mat-icon", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](36, "lock");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](37, " Privato - Solo tu e i collaboratori ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](38, "mat-radio-button", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](39, "mat-icon", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](40, "public");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](41, " Pubblico - Visibile a tutti ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](42, GitSetupRemoteDialogComponent_div_4_div_42_Template, 5, 1, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](43, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](44, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_mat_checkbox_ngModelChange_44_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r13.saveOrganization = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](45, " Ricorda organizzazione per progetti futuri ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](46, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_4_Template_mat_checkbox_ngModelChange_46_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9); const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r14.pushAfterAdd = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](47, " Push automatico dei commit esistenti ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](48, GitSetupRemoteDialogComponent_div_4_div_48_Template, 5, 1, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](49, "mat-expansion-panel", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](50, "mat-expansion-panel-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](51, "mat-panel-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](52, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](53, "help_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](54, " Come creare un repository su GitHub ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](55, "ol");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](56, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](57, "Vai su ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](58, "a", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](59, "github.com/new");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](60, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](61, "Inserisci ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](62, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](63);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](64, " come nome");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](65, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](66, "Seleziona se vuoi che sia pubblico o privato");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](67, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](68, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](69, "NON");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](70, " inizializzare con README, .gitignore o licenza");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](71, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](72, "Clicca su \"Create repository\"");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](73, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](74, "Torna qui e clicca su \"Configura\"");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r1.hasToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.organization)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpropertyInterpolate1"]("placeholder", "es. ", ctx_r1.data.projectName, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.repositoryName)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.repositoryDescription)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.isPrivate)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", true);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", false);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r1.getGitHubUrl());
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.saveOrganization)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.pushAfterAdd)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r1.error);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r1.repositoryName || "nome-repository");
} }
function GitSetupRemoteDialogComponent_mat_spinner_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "mat-spinner", 32);
} }
class GitSetupRemoteDialogComponent {
    constructor(dialogRef, data, gitService, snackBar, dialog) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitService = gitService;
        this.snackBar = snackBar;
        this.dialog = dialog;
        this.organization = '';
        this.repositoryName = '';
        this.repositoryDescription = '';
        this.isPrivate = true;
        this.saveOrganization = true;
        this.pushAfterAdd = true;
        this.hasToken = false;
        this.isLoading = false;
        this.isSetting = false;
        this.error = null;
        // Initialize repository name with project name
        this.repositoryName = this.extractProjectName(data.projectName);
    }
    ngOnInit() {
        this.loadSavedOrganization();
        this.checkToken();
    }
    loadSavedOrganization() {
        this.isLoading = true;
        this.gitService.getGitHubOrganization().subscribe({
            next: (org) => {
                this.organization = org;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading organization:', err);
                this.isLoading = false;
            }
        });
    }
    checkToken() {
        this.gitService.getGitHubToken().subscribe({
            next: (result) => {
                this.hasToken = result.hasToken && result.tokenValid;
            },
            error: (err) => {
                console.error('Error checking token:', err);
                this.hasToken = false;
            }
        });
    }
    openTokenSettings() {
        const dialogRef = this.dialog.open(_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_1__["GitTokenDialogComponent"], {
            width: '650px',
            data: {}
        });
        dialogRef.afterClosed().subscribe(result => {
            if (result === true) {
                // Token was configured successfully, update the status
                this.checkToken();
                this.snackBar.open('Token configurato con successo!', 'OK', {
                    duration: 3000,
                    verticalPosition: 'top'
                });
            }
        });
    }
    extractProjectName(fullPath) {
        // Extract the folder name from the full path
        const parts = fullPath.replace(/\\/g, '/').split('/');
        return parts[parts.length - 1] || 'repository';
    }
    getGitHubUrl() {
        if (this.organization && this.repositoryName) {
            return `https://github.com/${this.organization}/${this.repositoryName}.git`;
        }
        return '';
    }
    onCancel() {
        this.dialogRef.close(false);
    }
    onSetup() {
        if (!this.organization || !this.repositoryName) {
            this.error = 'Organizzazione e nome repository sono richiesti';
            return;
        }
        this.isSetting = true;
        this.error = null;
        // Save organization if requested
        if (this.saveOrganization && this.organization) {
            this.gitService.saveGitHubOrganization(this.organization).subscribe();
        }
        // Setup the remote with additional parameters
        this.gitService.setupGitHubRemote(this.data.projectPath, this.organization, this.repositoryName, this.saveOrganization, this.pushAfterAdd, this.repositoryDescription, this.isPrivate).subscribe({
            next: (response) => {
                this.isSetting = false;
                if (response.success) {
                    this.snackBar.open(response.message || 'Remote configurato con successo', 'OK', { duration: 5000, verticalPosition: 'top' });
                    this.dialogRef.close(true);
                }
                else {
                    this.error = response.error || 'Errore durante la configurazione del remote';
                }
            },
            error: (err) => {
                this.isSetting = false;
                this.error = 'Errore durante la configurazione: ' + (err.message || 'Errore sconosciuto');
                console.error('Error setting up remote:', err);
            }
        });
    }
}
GitSetupRemoteDialogComponent.ɵfac = function GitSetupRemoteDialogComponent_Factory(t) { return new (t || GitSetupRemoteDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_3__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_4__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialog"])); };
GitSetupRemoteDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: GitSetupRemoteDialogComponent, selectors: [["app-git-setup-remote-dialog"]], decls: 11, vars: 6, consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], ["class", "setup-form", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], [1, "loading-container"], ["diameter", "30"], [1, "setup-form"], [1, "instructions"], ["class", "warning-message", 4, "ngIf"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "es. mia-azienda o mio-username", 3, "ngModel", "disabled", "ngModelChange"], ["matSuffix", ""], ["matInput", "", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "Una breve descrizione del progetto", "rows", "2", 3, "ngModel", "disabled", "ngModelChange"], [1, "visibility-option"], [2, "display", "block", "margin-bottom", "8px"], [3, "ngModel", "disabled", "ngModelChange"], [3, "value"], [2, "vertical-align", "middle"], [2, "margin-left", "20px", 3, "value"], ["class", "url-preview", 4, "ngIf"], [1, "options"], ["class", "error-message", 4, "ngIf"], [1, "github-instructions"], ["href", "https://github.com/new", "target", "_blank"], [1, "warning-message"], [2, "cursor", "pointer", "text-decoration", "underline", 3, "click"], [1, "url-preview"], [1, "error-message"], ["diameter", "20", 2, "display", "inline-block", "margin-right", "8px"]], template: function GitSetupRemoteDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "Setup GitHub Remote");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, GitSetupRemoteDialogComponent_div_3_Template, 4, 0, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, GitSetupRemoteDialogComponent_div_4_Template, 75, 19, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-dialog-actions", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "button", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_Template_button_click_6_listener() { return ctx.onCancel(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7, "Annulla");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_Template_button_click_8_listener() { return ctx.onSetup(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](9, GitSetupRemoteDialogComponent_mat_spinner_9_Template, 1, 0, "mat-spinner", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.isSetting);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.isSetting || !ctx.organization || !ctx.repositoryName);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isSetting);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx.isSetting ? "Configurazione..." : "Configura", " ");
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_5__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_7__["MatSpinner"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_10__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatSuffix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatHint"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_12__["MatRadioGroup"], _angular_material_radio__WEBPACK_IMPORTED_MODULE_12__["MatRadioButton"], _angular_material_checkbox__WEBPACK_IMPORTED_MODULE_13__["MatCheckbox"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_14__["MatExpansionPanel"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_14__["MatExpansionPanelHeader"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_14__["MatExpansionPanelTitle"]], styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  padding: 20px;\n  min-height: 200px;\n}\n\n.setup-form[_ngcontent-%COMP%] {\n  min-width: 500px;\n  padding: 16px 0;\n}\n\n.instructions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: #e3f2fd;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n\n.instructions[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  margin-top: 2px;\n}\n\n.instructions[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n\n.url-preview[_ngcontent-%COMP%] {\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n  margin-bottom: 20px;\n  font-family: monospace;\n}\n\n.url-preview[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 8px;\n  font-family: Roboto, sans-serif;\n  color: #666;\n  font-size: 0.9em;\n}\n\n.url-preview[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  color: #1976d2;\n  word-break: break-all;\n  font-size: 0.95em;\n}\n\n.warning-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: #fff3cd;\n  border: 1px solid #ffc107;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n\n.warning-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n  margin-top: 2px;\n}\n\n.warning-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.visibility-option[_ngcontent-%COMP%] {\n  margin: 20px 0;\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n}\n\n.visibility-option[_ngcontent-%COMP%]   mat-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 24px;\n}\n\n.visibility-option[_ngcontent-%COMP%]   mat-radio-button[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n}\n\n.options[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\n.options[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 12px;\n}\n\n.error-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px;\n  background-color: #ffebee;\n  border-radius: 4px;\n  margin-top: 16px;\n  color: #c62828;\n}\n\n.error-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #c62828;\n}\n\n.github-instructions[_ngcontent-%COMP%] {\n  margin-top: 24px;\n  box-shadow: none;\n  border: 1px solid #e0e0e0;\n}\n\n.github-instructions[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n\n.github-instructions[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  margin: 0;\n  padding-left: 24px;\n  line-height: 1.8;\n}\n\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   a[_ngcontent-%COMP%] {\n  color: #1976d2;\n  text-decoration: none;\n}\n\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 24px !important;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px !important;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   mat-spinner[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LXNldHVwLXJlbW90ZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsaUJBQUE7QUFDRjs7QUFFQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFDRjs7QUFDRTtFQUNFLGNBQUE7RUFDQSxlQUFBO0FBQ0o7O0FBRUU7RUFDRSxTQUFBO0VBQ0EsY0FBQTtFQUNBLGdCQUFBO0FBQUo7O0FBSUE7RUFDRSxXQUFBO0VBQ0EsbUJBQUE7QUFERjs7QUFJQTtFQUNFLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxzQkFBQTtBQURGOztBQUdFO0VBQ0UsY0FBQTtFQUNBLGtCQUFBO0VBQ0EsK0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFESjs7QUFJRTtFQUNFLGNBQUE7RUFDQSxxQkFBQTtFQUNBLGlCQUFBO0FBRko7O0FBTUE7RUFDRSxhQUFBO0VBQ0EsdUJBQUE7RUFDQSxTQUFBO0VBQ0EsYUFBQTtFQUNBLHlCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBSEY7O0FBS0U7RUFDRSxjQUFBO0VBQ0EsZUFBQTtBQUhKOztBQU1FO0VBQ0UsU0FBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQUpKOztBQVFBO0VBQ0UsY0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0FBTEY7O0FBT0U7RUFDRSxhQUFBO0VBQ0EsU0FBQTtBQUxKOztBQVFFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0FBTko7O0FBVUE7RUFDRSxjQUFBO0FBUEY7O0FBU0U7RUFDRSxjQUFBO0VBQ0EsbUJBQUE7QUFQSjs7QUFXQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtBQVJGOztBQVVFO0VBQ0UsY0FBQTtBQVJKOztBQVlBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLHlCQUFBO0FBVEY7O0FBV0U7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7QUFUSjs7QUFXSTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQVROOztBQWFFO0VBQ0UsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFYSjs7QUFhSTtFQUNFLGtCQUFBO0FBWE47O0FBY0k7RUFDRSxjQUFBO0VBQ0EscUJBQUE7QUFaTjs7QUFjTTtFQUNFLDBCQUFBO0FBWlI7O0FBa0JBO0VBQ0UsMEJBQUE7QUFmRjs7QUFrQkE7RUFDRSw2QkFBQTtBQWZGOztBQWlCRTtFQUNFLHNCQUFBO0FBZkoiLCJmaWxlIjoiZ2l0LXNldHVwLXJlbW90ZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIubG9hZGluZy1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBnYXA6IDE2cHg7XHJcbiAgcGFkZGluZzogMjBweDtcclxuICBtaW4taGVpZ2h0OiAyMDBweDtcclxufVxyXG5cclxuLnNldHVwLWZvcm0ge1xyXG4gIG1pbi13aWR0aDogNTAwcHg7XHJcbiAgcGFkZGluZzogMTZweCAwO1xyXG59XHJcblxyXG4uaW5zdHJ1Y3Rpb25zIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG4gIGdhcDogMTJweDtcclxuICBwYWRkaW5nOiAxNnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNlM2YyZmQ7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIG1hcmdpbi1ib3R0b206IDI0cHg7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gICAgbWFyZ2luLXRvcDogMnB4O1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBjb2xvcjogIzQyNDI0MjtcclxuICAgIGxpbmUtaGVpZ2h0OiAxLjU7XHJcbiAgfVxyXG59XHJcblxyXG4uZnVsbC13aWR0aCB7XHJcbiAgd2lkdGg6IDEwMCU7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxufVxyXG5cclxuLnVybC1wcmV2aWV3IHtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIG1hcmdpbi1ib3R0b206IDIwcHg7XHJcbiAgZm9udC1mYW1pbHk6IG1vbm9zcGFjZTtcclxuXHJcbiAgc3Ryb25nIHtcclxuICAgIGRpc3BsYXk6IGJsb2NrO1xyXG4gICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG4gICAgZm9udC1mYW1pbHk6IFJvYm90bywgc2Fucy1zZXJpZjtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC1zaXplOiAwLjllbTtcclxuICB9XHJcblxyXG4gIGNvZGUge1xyXG4gICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICB3b3JkLWJyZWFrOiBicmVhay1hbGw7XHJcbiAgICBmb250LXNpemU6IDAuOTVlbTtcclxuICB9XHJcbn1cclxuXHJcbi53YXJuaW5nLW1lc3NhZ2Uge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGZsZXgtc3RhcnQ7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDE2cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZmZjNjZDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZmZjMTA3O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogI2ZmOTgwMDtcclxuICAgIG1hcmdpbi10b3A6IDJweDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgY29sb3I6ICM0MjQyNDI7XHJcbiAgICBsaW5lLWhlaWdodDogMS41O1xyXG4gIH1cclxufVxyXG5cclxuLnZpc2liaWxpdHktb3B0aW9uIHtcclxuICBtYXJnaW46IDIwcHggMDtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG5cclxuICBtYXQtcmFkaW8tZ3JvdXAge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGdhcDogMjRweDtcclxuICB9XHJcblxyXG4gIG1hdC1yYWRpby1idXR0b24ge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgfVxyXG59XHJcblxyXG4ub3B0aW9ucyB7XHJcbiAgbWFyZ2luOiAyMHB4IDA7XHJcblxyXG4gIG1hdC1jaGVja2JveCB7XHJcbiAgICBkaXNwbGF5OiBibG9jaztcclxuICAgIG1hcmdpbi1ib3R0b206IDEycHg7XHJcbiAgfVxyXG59XHJcblxyXG4uZXJyb3ItbWVzc2FnZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIHBhZGRpbmc6IDEycHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZmZWJlZTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLXRvcDogMTZweDtcclxuICBjb2xvcjogI2M2MjgyODtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICNjNjI4Mjg7XHJcbiAgfVxyXG59XHJcblxyXG4uZ2l0aHViLWluc3RydWN0aW9ucyB7XHJcbiAgbWFyZ2luLXRvcDogMjRweDtcclxuICBib3gtc2hhZG93OiBub25lO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMGUwZTA7XHJcblxyXG4gIG1hdC1wYW5lbC10aXRsZSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogOHB4O1xyXG4gICAgZm9udC1zaXplOiAwLjk1ZW07XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBvbCB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBwYWRkaW5nLWxlZnQ6IDI0cHg7XHJcbiAgICBsaW5lLWhlaWdodDogMS44O1xyXG5cclxuICAgIGxpIHtcclxuICAgICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG4gICAgfVxyXG5cclxuICAgIGEge1xyXG4gICAgICBjb2xvcjogIzE5NzZkMjtcclxuICAgICAgdGV4dC1kZWNvcmF0aW9uOiBub25lO1xyXG5cclxuICAgICAgJjpob3ZlciB7XHJcbiAgICAgICAgdGV4dC1kZWNvcmF0aW9uOiB1bmRlcmxpbmU7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctY29udGVudCB7XHJcbiAgcGFkZGluZzogMCAyNHB4ICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgcGFkZGluZzogMTZweCAyNHB4ICFpbXBvcnRhbnQ7XHJcblxyXG4gIG1hdC1zcGlubmVyIHtcclxuICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgfVxyXG59Il19 */"] });


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













function GitTokenDialogComponent_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Controllo token esistente...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitTokenDialogComponent_div_4_div_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "Token esistente:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](7, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("valid", ctx_r3.tokenValid)("invalid", !ctx_r3.tokenValid);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r3.tokenValid ? "check_circle" : "error", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r3.existingMaskedToken, " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("Stato: ", ctx_r3.tokenValid ? "Valido" : "Non valido o scaduto", "");
} }
function GitTokenDialogComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
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
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, GitTokenDialogComponent_div_4_div_9_Template, 10, 7, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-form-field", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "GitHub Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitTokenDialogComponent_div_4_Template_input_ngModelChange_13_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r5); const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r4.token = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "button", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_div_4_Template_button_click_14_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r5); const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r6.hideToken = !ctx_r6.hideToken; });
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
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitTokenDialogComponent_div_4_Template_button_click_41_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r5); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r7.openGitHubTokenPage(); });
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
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "mat-spinner", 20);
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
        this.hasExistingToken = false;
        this.existingMaskedToken = '';
        this.tokenValid = false;
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
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error checking token:', err);
                this.isLoading = false;
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
GitTokenDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: GitTokenDialogComponent, selectors: [["app-git-token-dialog"]], decls: 11, vars: 6, consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], [4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], [1, "loading-container"], ["diameter", "30"], [1, "instructions"], [1, "small-text"], ["class", "existing-token", 4, "ngIf"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "ghp_xxxxxxxxxxxxxxxxxxxx", 3, "ngModel", "type", "disabled", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", 3, "disabled", "click"], [1, "instructions-panel"], ["mat-stroked-button", "", "color", "primary", 3, "click"], [1, "warning-message"], [1, "existing-token"], [1, "status"], ["diameter", "20", 2, "display", "inline-block", "margin-right", "8px"]], template: function GitTokenDialogComponent_Template(rf, ctx) { if (rf & 1) {
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
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_4__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_5__["MatButton"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_6__["MatSpinner"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_9__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_10__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatSuffix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_8__["MatHint"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanel"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanelHeader"], _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__["MatExpansionPanelTitle"]], styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  padding: 20px;\n  min-height: 200px;\n}\n\n.instructions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  padding: 16px;\n  background-color: #e3f2fd;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n\n.instructions[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  margin-top: 2px;\n}\n\n.instructions[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.instructions[_ngcontent-%COMP%]   p.small-text[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #666;\n  margin: 0;\n}\n\n.existing-token[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n  margin-bottom: 20px;\n}\n\n.existing-token[_ngcontent-%COMP%]   mat-icon.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.existing-token[_ngcontent-%COMP%]   mat-icon.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n\n.existing-token[_ngcontent-%COMP%]   .status[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #666;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n\n.instructions-panel[_ngcontent-%COMP%] {\n  margin: 20px 0;\n  box-shadow: none;\n  border: 1px solid #e0e0e0;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  margin: 16px 0;\n  padding-left: 24px;\n  line-height: 1.8;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n\n.instructions-panel[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-top: 12px;\n}\n\n.warning-message[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  padding: 12px;\n  background-color: #fff3cd;\n  border: 1px solid #ffc107;\n  border-radius: 4px;\n  margin-top: 16px;\n}\n\n.warning-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n}\n\n.warning-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  font-size: 0.9em;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px !important;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   mat-spinner[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LXRva2VuLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSxpQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBQ0Y7O0FBQ0U7RUFDRSxjQUFBO0VBQ0EsZUFBQTtBQUNKOztBQUVFO0VBQ0UsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFBSjs7QUFFSTtFQUNFLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLFNBQUE7QUFBTjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxTQUFBO0VBQ0EsYUFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtBQUZGOztBQUtJO0VBQ0UsY0FBQTtBQUhOOztBQU1JO0VBQ0UsY0FBQTtBQUpOOztBQVFFO0VBQ0UsZ0JBQUE7RUFDQSxXQUFBO0FBTko7O0FBVUE7RUFDRSxXQUFBO0VBQ0EsbUJBQUE7QUFQRjs7QUFVQTtFQUNFLGNBQUE7RUFDQSxnQkFBQTtFQUNBLHlCQUFBO0FBUEY7O0FBU0U7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7QUFQSjs7QUFTSTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQVBOOztBQVdFO0VBQ0UsY0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFUSjs7QUFXSTtFQUNFLGtCQUFBO0FBVE47O0FBYUU7RUFDRSxnQkFBQTtBQVhKOztBQWVBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFaRjs7QUFjRTtFQUNFLGNBQUE7QUFaSjs7QUFlRTtFQUNFLFNBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFiSjs7QUFpQkE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBZEY7O0FBaUJBO0VBQ0UsNkJBQUE7QUFkRjs7QUFnQkU7RUFDRSxzQkFBQTtBQWRKIiwiZmlsZSI6ImdpdC10b2tlbi1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIubG9hZGluZy1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBnYXA6IDE2cHg7XHJcbiAgcGFkZGluZzogMjBweDtcclxuICBtaW4taGVpZ2h0OiAyMDBweDtcclxufVxyXG5cclxuLmluc3RydWN0aW9ucyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTZweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZTNmMmZkO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogIzE5NzZkMjtcclxuICAgIG1hcmdpbi10b3A6IDJweDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwIDAgOHB4IDA7XHJcbiAgICBjb2xvcjogIzQyNDI0MjtcclxuICAgIGxpbmUtaGVpZ2h0OiAxLjU7XHJcblxyXG4gICAgJi5zbWFsbC10ZXh0IHtcclxuICAgICAgZm9udC1zaXplOiAwLjllbTtcclxuICAgICAgY29sb3I6ICM2NjY7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5leGlzdGluZy10b2tlbiB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICAmLnZhbGlkIHtcclxuICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICB9XHJcblxyXG4gICAgJi5pbnZhbGlkIHtcclxuICAgICAgY29sb3I6ICNmNDQzMzY7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuc3RhdHVzIHtcclxuICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICB9XHJcbn1cclxuXHJcbi5mdWxsLXdpZHRoIHtcclxuICB3aWR0aDogMTAwJTtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG59XHJcblxyXG4uaW5zdHJ1Y3Rpb25zLXBhbmVsIHtcclxuICBtYXJnaW46IDIwcHggMDtcclxuICBib3gtc2hhZG93OiBub25lO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMGUwZTA7XHJcblxyXG4gIG1hdC1wYW5lbC10aXRsZSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogOHB4O1xyXG4gICAgZm9udC1zaXplOiAwLjk1ZW07XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBvbCB7XHJcbiAgICBtYXJnaW46IDE2cHggMDtcclxuICAgIHBhZGRpbmctbGVmdDogMjRweDtcclxuICAgIGxpbmUtaGVpZ2h0OiAxLjg7XHJcblxyXG4gICAgbGkge1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiA4cHg7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBidXR0b24ge1xyXG4gICAgbWFyZ2luLXRvcDogMTJweDtcclxuICB9XHJcbn1cclxuXHJcbi53YXJuaW5nLW1lc3NhZ2Uge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDEycHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZmZjNjZDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZmZjMTA3O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tdG9wOiAxNnB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogI2ZmOTgwMDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgY29sb3I6ICM0MjQyNDI7XHJcbiAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gIH1cclxufVxyXG5cclxubWF0LWRpYWxvZy1jb250ZW50IHtcclxuICBtaW4td2lkdGg6IDUwMHB4O1xyXG4gIG1heC13aWR0aDogNjAwcHg7XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgcGFkZGluZzogMTZweCAyNHB4ICFpbXBvcnRhbnQ7XHJcblxyXG4gIG1hdC1zcGlubmVyIHtcclxuICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgfVxyXG59Il19 */"] });


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
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/button */ "bTqV");










class SettingsComponent {
    constructor(appCurrentFolder, dialogRef, _snackBar) {
        this.appCurrentFolder = appCurrentFolder;
        this.dialogRef = dialogRef;
        this._snackBar = _snackBar;
    }
    ngOnInit() {
        this.appCurrentFolder.loadSettings();
        this.appCurrentFolder.settings.subscribe((data) => {
            var _a;
            var settings = data.settings;
            if (settings != undefined) {
                this._settings = settings;
                this.vscodePath = settings.filter(_ => _.name === "EditorPath")[0].valueString || null;
                this.intellijPath = ((_a = settings.filter(_ => _.name === "IntelliJPath")[0]) === null || _a === void 0 ? void 0 : _a.valueString) || null;
                this.jiraServer = settings.filter(_ => _.name === "JiraServer")[0].valueString || null;
                this.plantumlLocalPath = settings.filter(_ => _.name === "PlantumlLocalPath")[0].valueString || null;
                this.javaPath = settings.filter(_ => _.name === "JavaPath")[0].valueString || null;
                this.localGraphvizDotPath = settings.filter(_ => _.name === "LocalGraphvizDotPath")[0].valueString || null;
            }
        });
    }
    save() {
        this._settings.filter(_ => _.name === "EditorPath")[0].valueString = this.vscodePath;
        this._settings.filter(_ => _.name === "IntelliJPath")[0].valueString = this.intellijPath;
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
SettingsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: SettingsComponent, selectors: [["app-settings"]], decls: 56, vars: 6, consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/IconReady.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matPrefix", "", 1, "ide-icon", "vscode-color"], ["matInput", "", "placeholder", "Visual studio code path", 3, "ngModel", "ngModelChange"], ["matPrefix", "", 1, "ide-icon", "intellij-color"], ["matInput", "", "placeholder", "IntelliJ IDEA path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "plantuml local path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "Java folder", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "LocalGraphvizDot Path", 3, "ngModel", "ngModelChange"], ["matInput", "", "placeholder", "Jira path", 3, "ngModel", "ngModelChange"], ["mat-button", "", "color", "primary", 3, "click"]], template: function SettingsComponent_Template(rf, ctx) { if (rf & 1) {
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
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "Visual Studio Code path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "mat-icon", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](11, "code");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "input", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_12_listener($event) { return ctx.vscodePath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](14, "Set Visual Studio Code installation path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](17, "IntelliJ IDEA path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](18, "mat-icon", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](19, "terminal");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](20, "input", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_20_listener($event) { return ctx.intellijPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](21, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](22, "Set IntelliJ IDEA installation path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](24, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](25, "Plantuml local path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](26, "input", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_26_listener($event) { return ctx.plantumlLocalPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](27, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](28, "Set path for plantuml server");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](29, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](30, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](31, "Java path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](32, "input", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_32_listener($event) { return ctx.javaPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](33, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](34, "Set java path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](35, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](36, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](37, "LocalGraphvizDot path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](38, "input", 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_38_listener($event) { return ctx.localGraphvizDotPath = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](39, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](40, "Set graphiz internal path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](41, "mat-form-field", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](42, "mat-label");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](43, "Jira path");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](44, "input", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function SettingsComponent_Template_input_ngModelChange_44_listener($event) { return ctx.jiraServer = $event; });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](45, "mat-hint");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](46, "Set http to jira");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](47, "mat-dialog-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](48, "button", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_48_listener() { return ctx.save(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](49, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](50, "save");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](51, "Save ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](52, "button", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function SettingsComponent_Template_button_click_52_listener() { return ctx.dismiss(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](53, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](54, "cancel");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](55, "Cancel ");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](12);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.vscodePath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](8);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.intellijPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.plantumlLocalPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.javaPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.localGraphvizDotPath);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.jiraServer);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogContent"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCard"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatLabel"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__["MatIcon"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatPrefix"], _angular_material_input__WEBPACK_IMPORTED_MODULE_7__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_8__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_5__["MatHint"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_2__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_9__["MatButton"]], styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.vertical-form-container[_ngcontent-%COMP%]    > *[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.ide-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n}\n\n.ide-icon.vscode-color[_ngcontent-%COMP%] {\n  color: #007ACC;\n}\n\n.ide-icon.intellij-color[_ngcontent-%COMP%] {\n  color: #FF6B00;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcLi5cXHNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxXQUFBO0FBQ0Y7O0FBR0E7RUFDRSxpQkFBQTtBQUFGOztBQUVFO0VBQ0UsY0FBQTtBQUFKOztBQUdFO0VBQ0UsY0FBQTtBQURKIiwiZmlsZSI6InNldHRpbmdzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLnZlcnRpY2FsLWZvcm0tY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcbi52ZXJ0aWNhbC1mb3JtLWNvbnRhaW5lciA+ICoge1xyXG4gIHdpZHRoOiAxMDAlXHJcbn1cclxuXHJcbi8vIElERSBpY29ucyBzdHlsaW5nXHJcbi5pZGUtaWNvbiB7XHJcbiAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcblxyXG4gICYudnNjb2RlLWNvbG9yIHtcclxuICAgIGNvbG9yOiAjMDA3QUNDOyAvLyBWUyBDb2RlIGJsdWVcclxuICB9XHJcblxyXG4gICYuaW50ZWxsaWotY29sb3Ige1xyXG4gICAgY29sb3I6ICNGRjZCMDA7IC8vIEludGVsbGlKIG9yYW5nZVxyXG4gIH1cclxufVxyXG4iXX0= */"], data: { animation: [] } });


/***/ }),

/***/ "K5hE":
/*!*********************************************************************************!*\
  !*** ./src/app/git/dialogs/git-init-wizard/git-init-wizard-dialog.component.ts ***!
  \*********************************************************************************/
/*! exports provided: GitInitWizardDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitInitWizardDialogComponent", function() { return GitInitWizardDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _models_git_init_models__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../models/git-init.models */ "/DYt");
/* harmony import */ var _git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../git-setup-remote-dialog/git-setup-remote-dialog.component */ "8cbT");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/gitservice.service */ "N73s");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_select__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/select */ "d3UM");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/core */ "FKr1");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
















function GitInitWizardDialogComponent_div_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "p", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Initializing Git repository...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitInitWizardDialogComponent_div_6_mat_option_21_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-option", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const template_r6 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", template_r6.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", template_r6.label, " ");
} }
function GitInitWizardDialogComponent_div_6_Template(rf, ctx) { if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "p", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, " Initialize a new Git repository in this project folder. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-form-field", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, "Repository Path");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "input", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_6_Template_input_ngModelChange_6_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8); const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r7.repositoryPath = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-form-field", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](10, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](11, "Initial Branch Name");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](12, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_6_Template_input_ngModelChange_12_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8); const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r9.initialBranch = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](14, "account_tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](15, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](16, "Default branch name (e.g., main, master, develop)");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](17, "mat-form-field", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](18, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](19, "Select .gitignore Template");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](20, "mat-select", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_6_Template_mat_select_ngModelChange_20_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8); const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r10.gitignoreTemplate = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](21, GitInitWizardDialogComponent_div_6_mat_option_21_Template, 2, 2, "mat-option", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](22, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](23, "description");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](24, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](27, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](28, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](29, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](30, "strong", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](31, "What will be created:");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](32, "ul", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](33, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](34, ".git/ folder (Git repository)");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](35, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](36);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](37, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](38, "Initial branch: ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](39, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.repositoryPath);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.initialBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.gitignoreTemplate);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r1.gitignoreTemplates);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r1.getSelectedTemplateDescription());
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](".gitignore file (", ctx_r1.gitignoreTemplate, " template)");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r1.initialBranch);
} }
function GitInitWizardDialogComponent_div_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "h3", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, "Git Repository Initialized!");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "p", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "div", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](10, "mat-icon", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](11, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](12, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13, "Next Step: Setup Remote Repository");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](14, "p", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](15, " Would you like to connect this repository to a remote Git service (GitHub, GitLab, etc.)? This allows you to backup your code and collaborate with others. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r2.initMessage);
} }
function GitInitWizardDialogComponent_div_9_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "button", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_9_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r12); const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r11.cancel(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, " Cancel ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "button", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_9_Template_button_click_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r12); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r13.initializeGit(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, "add_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6, " Initialize Git ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx_r3.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx_r3.isLoading);
} }
function GitInitWizardDialogComponent_div_10_Template(rf, ctx) { if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_10_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r15); const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r14.skipRemoteSetup(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "skip_next");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4, " Skip for Now ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "button", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_10_Template_button_click_5_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r15); const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r16.setupRemote(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "cloud_upload");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, " Setup Remote ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
class GitInitWizardDialogComponent {
    constructor(dialogRef, data, gitService, snackBar, dialog) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitService = gitService;
        this.snackBar = snackBar;
        this.dialog = dialog;
        // Wizard state
        this.currentStep = 0;
        this.isLoading = false;
        this.initialBranch = 'main';
        this.gitignoreTemplate = 'mdexplorer';
        this.gitignoreTemplates = _models_git_init_models__WEBPACK_IMPORTED_MODULE_1__["GITIGNORE_TEMPLATES"];
        // Results
        this.initSuccess = false;
        this.initMessage = '';
        this.repositoryPath = data.repositoryPath;
    }
    ngOnInit() {
        console.log('[GitInitWizard] Initialized for path:', this.repositoryPath);
    }
    /**
     * Initialize Git repository (Step 1)
     */
    initializeGit() {
        if (!this.repositoryPath) {
            this.snackBar.open('Repository path is required', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        this.isLoading = true;
        const request = {
            repositoryPath: this.repositoryPath,
            initialBranch: this.initialBranch,
            gitignoreTemplate: this.gitignoreTemplate
        };
        console.log('[GitInitWizard] Initializing Git repository:', request);
        this.gitService.initRepository(request).subscribe((response) => {
            this.isLoading = false;
            if (response.success) {
                this.initSuccess = true;
                this.initMessage = response.message;
                console.log('[GitInitWizard] ✅ Git repository initialized successfully');
                this.snackBar.open('Git repository initialized successfully!', 'OK', {
                    duration: 3000,
                    verticalPosition: 'top',
                    panelClass: ['success-snackbar']
                });
                // Move to step 2 (Remote Setup)
                this.currentStep = 1;
            }
            else {
                console.error('[GitInitWizard] ❌ Initialization failed:', response.message);
                this.snackBar.open(`Initialization failed: ${response.message}`, 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
            }
        }, (error) => {
            var _a;
            this.isLoading = false;
            console.error('[GitInitWizard] ❌ Error initializing repository:', error);
            const errorMessage = ((_a = error === null || error === void 0 ? void 0 : error.error) === null || _a === void 0 ? void 0 : _a.message) || (error === null || error === void 0 ? void 0 : error.message) || 'Unknown error';
            this.snackBar.open(`Error: ${errorMessage}`, 'OK', {
                duration: 5000,
                verticalPosition: 'top',
                panelClass: ['error-snackbar']
            });
        });
    }
    /**
     * Open remote setup dialog (Step 2)
     */
    setupRemote() {
        const remoteDialogRef = this.dialog.open(_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_2__["GitSetupRemoteDialogComponent"], {
            width: '600px',
            data: {
                repositoryPath: this.repositoryPath
            }
        });
        remoteDialogRef.afterClosed().subscribe(result => {
            if (result) {
                console.log('[GitInitWizard] Remote setup completed');
                // Close wizard after successful remote setup
                this.dialogRef.close(true);
            }
        });
    }
    /**
     * Skip remote setup and close wizard
     */
    skipRemoteSetup() {
        console.log('[GitInitWizard] Skipping remote setup');
        this.dialogRef.close(true);
    }
    /**
     * Cancel wizard
     */
    cancel() {
        this.dialogRef.close(false);
    }
    /**
     * Get selected template description
     */
    getSelectedTemplateDescription() {
        const template = this.gitignoreTemplates.find(t => t.value === this.gitignoreTemplate);
        return template ? template.description : '';
    }
}
GitInitWizardDialogComponent.ɵfac = function GitInitWizardDialogComponent_Factory(t) { return new (t || GitInitWizardDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_4__["GITService"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_5__["MatSnackBar"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialog"])); };
GitInitWizardDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: GitInitWizardDialogComponent, selectors: [["app-git-init-wizard-dialog"]], decls: 11, vars: 5, consts: [["mat-dialog-title", ""], [2, "vertical-align", "middle"], ["style", "text-align: center; padding: 40px;", 4, "ngIf"], [4, "ngIf"], ["align", "end"], ["style", "display: flex; gap: 8px; width: 100%; justify-content: flex-end;", 4, "ngIf"], [2, "text-align", "center", "padding", "40px"], ["diameter", "50", 2, "margin", "0 auto"], [2, "margin-top", "20px", "color", "#666"], [2, "margin-bottom", "20px", "color", "#666"], ["appearance", "outline", 2, "width", "100%", "margin-bottom", "16px"], ["matInput", "", "readonly", "", 3, "ngModel", "ngModelChange"], ["matSuffix", ""], ["matInput", "", "placeholder", "main", 3, "ngModel", "ngModelChange"], [3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], [2, "background", "#f5f5f5", "padding", "12px", "border-radius", "4px", "margin-bottom", "16px"], [2, "display", "flex", "align-items", "center", "gap", "8px", "margin-bottom", "8px"], [2, "color", "#666", "font-size", "20px"], [2, "color", "#333"], [2, "margin", "0", "padding-left", "24px", "color", "#666"], [3, "value"], [2, "text-align", "center", "padding", "20px"], [2, "color", "#4caf50", "font-size", "64px", "width", "64px", "height", "64px"], [2, "color", "#4caf50", "margin", "16px 0 8px 0"], [2, "color", "#666", "margin-bottom", "24px"], [2, "background", "#e3f2fd", "padding", "16px", "border-radius", "4px", "border-left", "4px solid #2196f3", "text-align", "left"], [2, "color", "#2196f3"], [2, "margin", "0", "color", "#666"], [2, "display", "flex", "gap", "8px", "width", "100%", "justify-content", "flex-end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"]], template: function GitInitWizardDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, " Initialize Git Repository\n");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, GitInitWizardDialogComponent_div_5_Template, 4, 0, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitInitWizardDialogComponent_div_6_Template, 41, 7, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, GitInitWizardDialogComponent_div_7_Template, 16, 1, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "mat-dialog-actions", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, GitInitWizardDialogComponent_div_9_Template, 7, 2, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitInitWizardDialogComponent_div_10_Template, 9, 0, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && ctx.currentStep === 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && ctx.currentStep === 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.currentStep === 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.currentStep === 1);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_6__["MatIcon"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__["MatSpinner"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_10__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_11__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatSuffix"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_9__["MatHint"], _angular_material_select__WEBPACK_IMPORTED_MODULE_12__["MatSelect"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgForOf"], _angular_material_core__WEBPACK_IMPORTED_MODULE_13__["MatOption"], _angular_material_button__WEBPACK_IMPORTED_MODULE_14__["MatButton"]], styles: ["mat-dialog-content[_ngcontent-%COMP%] {\n  min-height: 300px;\n  max-height: 70vh;\n  padding: 24px;\n}\n\nmat-form-field[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.success-snackbar[_ngcontent-%COMP%] {\n  background-color: #4caf50 !important;\n  color: white !important;\n}\n\n.error-snackbar[_ngcontent-%COMP%] {\n  background-color: #f44336 !important;\n  color: white !important;\n}\n\nmat-icon[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LWluaXQtd2l6YXJkLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxhQUFBO0FBQ0Y7O0FBRUE7RUFDRSxXQUFBO0FBQ0Y7O0FBRUE7RUFDRSxvQ0FBQTtFQUNBLHVCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxvQ0FBQTtFQUNBLHVCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxzQkFBQTtBQUNGIiwiZmlsZSI6ImdpdC1pbml0LXdpemFyZC1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyJtYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIG1pbi1oZWlnaHQ6IDMwMHB4O1xyXG4gIG1heC1oZWlnaHQ6IDcwdmg7XHJcbiAgcGFkZGluZzogMjRweDtcclxufVxyXG5cclxubWF0LWZvcm0tZmllbGQge1xyXG4gIHdpZHRoOiAxMDAlO1xyXG59XHJcblxyXG4uc3VjY2Vzcy1zbmFja2JhciB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogIzRjYWY1MCAhaW1wb3J0YW50O1xyXG4gIGNvbG9yOiB3aGl0ZSAhaW1wb3J0YW50O1xyXG59XHJcblxyXG4uZXJyb3Itc25hY2tiYXIge1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmNDQzMzYgIWltcG9ydGFudDtcclxuICBjb2xvcjogd2hpdGUgIWltcG9ydGFudDtcclxufVxyXG5cclxubWF0LWljb24ge1xyXG4gIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbn1cclxuIl19 */"] });


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
/* harmony import */ var _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/git-history-dialog/git-history-dialog.component */ "t2Ho");
/* harmony import */ var _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/git-branch-dialog/git-branch-dialog.component */ "3YvM");
/* harmony import */ var _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/git-setup-remote-dialog/git-setup-remote-dialog.component */ "8cbT");
/* harmony import */ var _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/git-token-dialog/git-token-dialog.component */ "8zqs");
/* harmony import */ var _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./dialogs/git-account-management-dialog/git-account-management-dialog.component */ "j/yN");
/* harmony import */ var _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./dialogs/git-init-wizard/git-init-wizard-dialog.component */ "K5hE");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/core */ "fXoL");












class GitModule {
}
GitModule.ɵfac = function GitModule_Factory(t) { return new (t || GitModule)(); };
GitModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdefineNgModule"]({ type: GitModule });
GitModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵdefineInjector"]({ imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_9__["MaterialModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_10__["FormsModule"]
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_11__["ɵɵsetNgModuleScope"](GitModule, { declarations: [_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_1__["GitMessagesComponent"],
        _dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_2__["CommitMessageDialogComponent"],
        _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_3__["GitHistoryDialogComponent"],
        _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_4__["GitBranchDialogComponent"],
        _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_5__["GitSetupRemoteDialogComponent"],
        _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__["GitTokenDialogComponent"],
        _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__["GitAccountManagementDialogComponent"],
        _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__["GitInitWizardDialogComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_9__["MaterialModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_10__["FormsModule"]], exports: [_dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_2__["CommitMessageDialogComponent"],
        _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_3__["GitHistoryDialogComponent"],
        _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_4__["GitBranchDialogComponent"],
        _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_5__["GitSetupRemoteDialogComponent"],
        _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__["GitTokenDialogComponent"],
        _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__["GitAccountManagementDialogComponent"],
        _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__["GitInitWizardDialogComponent"]] }); })();


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

/***/ "j/yN":
/*!******************************************************************************************************!*\
  !*** ./src/app/git/dialogs/git-account-management-dialog/git-account-management-dialog.component.ts ***!
  \******************************************************************************************************/
/*! exports provided: GitAccountManagementDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitAccountManagementDialogComponent", function() { return GitAccountManagementDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_git_account_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/git-account.service */ "pFRR");
/* harmony import */ var _angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/snack-bar */ "dNgK");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/divider */ "f0Cb");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_material_select__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/select */ "d3UM");
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/core */ "FKr1");
















function GitAccountManagementDialogComponent_div_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Caricamento...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitAccountManagementDialogComponent_div_12_div_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r6.currentAccount.username);
} }
function GitAccountManagementDialogComponent_div_12_div_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "email");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r7.currentAccount.email);
} }
function GitAccountManagementDialogComponent_div_12_span_17_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "GitHub Token configurato");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitAccountManagementDialogComponent_div_12_span_18_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "GitLab Token configurato");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitAccountManagementDialogComponent_div_12_div_21_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "note");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r10.currentAccount.notes);
} }
function GitAccountManagementDialogComponent_div_12_Template(rf, ctx) { if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "Account Attivo");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-card", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-card-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-icon", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, GitAccountManagementDialogComponent_div_12_div_12_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitAccountManagementDialogComponent_div_12_div_13_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16, "vpn_key");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](17, GitAccountManagementDialogComponent_div_12_span_17_Template, 2, 0, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](18, GitAccountManagementDialogComponent_div_12_span_18_Template, 2, 0, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](21, GitAccountManagementDialogComponent_div_12_div_21_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-card-actions");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](23, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_12_Template_button_click_23_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r12); const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r11.editAccount(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](25, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](26, " Modifica ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_12_Template_button_click_27_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r12); const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r13.deleteAccount(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](29, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](30, " Elimina ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r1.currentAccount.accountName, " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r1.currentAccount.accountType, " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.username);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.email);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.hasGitHubPAT);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.hasGitLabToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("success", ctx_r1.currentAccount.hasGitHubPAT || ctx_r1.currentAccount.hasGitLabToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r1.currentAccount.hasGitHubPAT || ctx_r1.currentAccount.hasGitLabToken ? "verified" : "error", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.notes);
} }
function GitAccountManagementDialogComponent_div_13_Template(rf, ctx) { if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "Nessun Account Configurato");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Configura un account Git per questo repository per abilitare push e pull.");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_13_Template_button_click_7_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r15); const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r14.showCreate(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, " Crea Nuovo Account ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} }
function GitAccountManagementDialogComponent_div_14_mat_option_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-option", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const type_r19 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("value", type_r19.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", type_r19.label, " ");
} }
function GitAccountManagementDialogComponent_div_14_mat_form_field_16_Template(rf, ctx) { if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "GitHub Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "input", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_mat_form_field_16_Template_input_ngModelChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r21); const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r20.gitHubPAT = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "button", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_mat_form_field_16_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r21); const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r22.hideToken = !ctx_r22.hideToken; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "a", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_mat_form_field_16_Template_a_click_8_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r21); const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r23.openTokenCreationPage(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Crea un nuovo token su GitHub");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r17.gitHubPAT)("type", ctx_r17.hideToken ? "password" : "text");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r17.hideToken ? "visibility" : "visibility_off");
} }
function GitAccountManagementDialogComponent_div_14_mat_form_field_17_Template(rf, ctx) { if (rf & 1) {
    const _r25 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "GitLab Personal Access Token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "input", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_mat_form_field_17_Template_input_ngModelChange_3_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r25); const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r24.gitLabToken = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "button", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_mat_form_field_17_Template_button_click_4_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r25); const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r26.hideToken = !ctx_r26.hideToken; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "a", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_mat_form_field_17_Template_a_click_8_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r25); const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2); return ctx_r27.openTokenCreationPage(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "Crea un nuovo token su GitLab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r18.gitLabToken)("type", ctx_r18.hideToken ? "password" : "text");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r18.hideToken ? "visibility" : "visibility_off");
} }
function GitAccountManagementDialogComponent_div_14_Template(rf, ctx) { if (rf & 1) {
    const _r29 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "Nome Account");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "input", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_Template_input_ngModelChange_6_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r29); const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r28.accountName = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, "badge");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "Tipo Account");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-select", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_Template_mat_select_ngModelChange_12_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r29); const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r30.accountType = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitAccountManagementDialogComponent_div_14_mat_option_13_Template, 2, 2, "mat-option", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15, "cloud");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, GitAccountManagementDialogComponent_div_14_mat_form_field_16_Template, 10, 3, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](17, GitAccountManagementDialogComponent_div_14_mat_form_field_17_Template, 10, 3, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](18, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20, "Informazioni Git (Opzionali)");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](23, "Git Username");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "input", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_Template_input_ngModelChange_24_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r29); const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r31.username = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](25, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](26, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](29, "Git Email");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "input", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_Template_input_ngModelChange_30_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r29); const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r32.email = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](31, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](32, "email");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](33, "mat-form-field", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](34, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](35, "Note");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](36, "textarea", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_14_Template_textarea_ngModelChange_36_listener($event) { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r29); const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r33.notes = $event; });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](37, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](38, "note");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r3.currentAccount ? "Modifica Account" : "Nuovo Account Git");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.accountName);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.accountType);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r3.accountTypes);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "GitHub");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "GitLab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.username);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.email);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.notes);
} }
function GitAccountManagementDialogComponent_ng_container_18_Template(rf, ctx) { if (rf & 1) {
    const _r35 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "button", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_ng_container_18_Template_button_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r35); const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r34.cancelCreate(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "Annulla");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "button", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_ng_container_18_Template_button_click_3_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r35); const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r36.currentAccount ? ctx_r36.updateAccount() : ctx_r36.saveAccount(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r4.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r4.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r4.currentAccount ? "save" : "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r4.currentAccount ? "Aggiorna" : "Salva", " ");
} }
function GitAccountManagementDialogComponent_button_19_Template(rf, ctx) { if (rf & 1) {
    const _r38 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_button_19_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r38); const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](); return ctx_r37.showCreate(); });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, " Crea Nuovo ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r5.isLoading);
} }
class GitAccountManagementDialogComponent {
    constructor(dialogRef, data, gitAccountService, snackBar) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitAccountService = gitAccountService;
        this.snackBar = snackBar;
        this.isLoading = false;
        this.currentAccount = null;
        this.showCreateForm = false;
        // Form fields
        this.accountName = '';
        this.accountType = 'GitHub';
        this.gitHubPAT = '';
        this.gitLabToken = '';
        this.username = '';
        this.email = '';
        this.notes = '';
        this.hideToken = true;
        this.accountTypes = [
            { value: 'GitHub', label: 'GitHub' },
            { value: 'GitLab', label: 'GitLab' },
            { value: 'Bitbucket', label: 'Bitbucket' },
            { value: 'Generic', label: 'Generic Git' }
        ];
    }
    ngOnInit() {
        this.loadCurrentAccount();
    }
    loadCurrentAccount() {
        this.isLoading = true;
        this.gitAccountService.getAccountForRepository(this.data.repositoryPath).subscribe({
            next: (account) => {
                this.currentAccount = account;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Error loading account:', err);
                this.isLoading = false;
                // No account is OK, user can create one
            }
        });
    }
    showCreate() {
        this.showCreateForm = true;
        // Pre-fill with defaults
        this.accountName = `${this.accountType} Account`;
    }
    cancelCreate() {
        this.showCreateForm = false;
        this.resetForm();
    }
    resetForm() {
        this.accountName = '';
        this.accountType = 'GitHub';
        this.gitHubPAT = '';
        this.gitLabToken = '';
        this.username = '';
        this.email = '';
        this.notes = '';
    }
    saveAccount() {
        // Validation
        if (!this.accountName.trim()) {
            this.snackBar.open('Nome account richiesto', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        if (this.accountType === 'GitHub' && !this.gitHubPAT.trim()) {
            this.snackBar.open('GitHub Personal Access Token richiesto', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        if (this.accountType === 'GitLab' && !this.gitLabToken.trim()) {
            this.snackBar.open('GitLab Token richiesto', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        const request = {
            repositoryPath: this.data.repositoryPath,
            accountName: this.accountName.trim(),
            accountType: this.accountType,
            gitHubPAT: this.accountType === 'GitHub' ? this.gitHubPAT.trim() : undefined,
            gitLabToken: this.accountType === 'GitLab' ? this.gitLabToken.trim() : undefined,
            username: this.username.trim() || undefined,
            email: this.email.trim() || undefined,
            notes: this.notes.trim() || undefined,
            isActive: true
        };
        this.isLoading = true;
        this.gitAccountService.createAccount(request).subscribe({
            next: (account) => {
                this.snackBar.open('Account Git creato con successo!', 'OK', {
                    duration: 3000,
                    verticalPosition: 'top',
                    panelClass: ['success-snackbar']
                });
                this.currentAccount = account;
                this.showCreateForm = false;
                this.resetForm();
                this.isLoading = false;
            },
            error: (err) => {
                var _a, _b;
                console.error('Error creating account:', err);
                const message = ((_a = err.error) === null || _a === void 0 ? void 0 : _a.error) || ((_b = err.error) === null || _b === void 0 ? void 0 : _b.title) || 'Errore nella creazione dell\'account';
                this.snackBar.open(message, 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
                this.isLoading = false;
            }
        });
    }
    deleteAccount() {
        if (!this.currentAccount || !this.currentAccount.id)
            return;
        const confirmed = confirm(`Vuoi davvero eliminare l'account "${this.currentAccount.accountName}"?`);
        if (!confirmed)
            return;
        this.isLoading = true;
        this.gitAccountService.deleteAccount(this.currentAccount.id).subscribe({
            next: (result) => {
                if (result.success) {
                    this.snackBar.open('Account eliminato con successo', 'OK', {
                        duration: 3000,
                        verticalPosition: 'top'
                    });
                    this.currentAccount = null;
                    this.isLoading = false;
                }
            },
            error: (err) => {
                console.error('Error deleting account:', err);
                this.snackBar.open('Errore nell\'eliminazione dell\'account', 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
                this.isLoading = false;
            }
        });
    }
    editAccount() {
        if (!this.currentAccount)
            return;
        // Fill form with current account data
        this.accountName = this.currentAccount.accountName;
        this.accountType = this.currentAccount.accountType;
        this.username = this.currentAccount.username || '';
        this.email = this.currentAccount.email || '';
        this.notes = this.currentAccount.notes || '';
        // Don't pre-fill tokens for security
        this.showCreateForm = true;
    }
    updateAccount() {
        if (!this.currentAccount || !this.currentAccount.id)
            return;
        // Validation similar to saveAccount
        if (!this.accountName.trim()) {
            this.snackBar.open('Nome account richiesto', 'OK', {
                duration: 3000,
                verticalPosition: 'top'
            });
            return;
        }
        const request = {
            repositoryPath: this.data.repositoryPath,
            accountName: this.accountName.trim(),
            accountType: this.accountType,
            username: this.username.trim() || undefined,
            email: this.email.trim() || undefined,
            notes: this.notes.trim() || undefined,
            isActive: true
        };
        // Only include token if user provided a new one
        if (this.accountType === 'GitHub' && this.gitHubPAT.trim()) {
            request.gitHubPAT = this.gitHubPAT.trim();
        }
        if (this.accountType === 'GitLab' && this.gitLabToken.trim()) {
            request.gitLabToken = this.gitLabToken.trim();
        }
        this.isLoading = true;
        this.gitAccountService.updateAccount(this.currentAccount.id, request).subscribe({
            next: (account) => {
                this.snackBar.open('Account aggiornato con successo!', 'OK', {
                    duration: 3000,
                    verticalPosition: 'top',
                    panelClass: ['success-snackbar']
                });
                this.currentAccount = account;
                this.showCreateForm = false;
                this.resetForm();
                this.isLoading = false;
            },
            error: (err) => {
                var _a;
                console.error('Error updating account:', err);
                const message = ((_a = err.error) === null || _a === void 0 ? void 0 : _a.error) || 'Errore nell\'aggiornamento dell\'account';
                this.snackBar.open(message, 'OK', {
                    duration: 5000,
                    verticalPosition: 'top',
                    panelClass: ['error-snackbar']
                });
                this.isLoading = false;
            }
        });
    }
    openTokenCreationPage() {
        if (this.accountType === 'GitHub') {
            window.open('https://github.com/settings/tokens/new?scopes=repo', '_blank');
        }
        else if (this.accountType === 'GitLab') {
            window.open('https://gitlab.com/-/profile/personal_access_tokens', '_blank');
        }
    }
    close() {
        this.dialogRef.close(this.currentAccount !== null);
    }
}
GitAccountManagementDialogComponent.ɵfac = function GitAccountManagementDialogComponent_Factory(t) { return new (t || GitAccountManagementDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_git_account_service__WEBPACK_IMPORTED_MODULE_2__["GitAccountService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_snack_bar__WEBPACK_IMPORTED_MODULE_3__["MatSnackBar"])); };
GitAccountManagementDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: GitAccountManagementDialogComponent, selectors: [["app-git-account-management-dialog"]], decls: 20, vars: 8, consts: [["mat-dialog-title", ""], [1, "dialog-content"], [1, "repository-info"], ["class", "loading-container", 4, "ngIf"], ["class", "current-account", 4, "ngIf"], ["class", "no-account", 4, "ngIf"], ["class", "account-form", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], [4, "ngIf"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click", 4, "ngIf"], [1, "loading-container"], ["diameter", "40"], [1, "current-account"], [1, "account-card"], [1, "status-icon", "success"], ["class", "account-detail", 4, "ngIf"], [1, "account-detail"], [1, "credential-status"], ["mat-button", "", "color", "primary", 3, "click"], ["mat-button", "", "color", "warn", 3, "click"], [1, "notes"], [1, "no-account"], [1, "large-icon"], ["mat-raised-button", "", "color", "primary", 3, "click"], [1, "account-form"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "placeholder", "es: My GitHub Work", "required", "", 3, "ngModel", "ngModelChange"], ["matSuffix", ""], ["required", "", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["appearance", "outline", "class", "full-width", 4, "ngIf"], ["matInput", "", "placeholder", "username", 3, "ngModel", "ngModelChange"], ["matInput", "", "type", "email", "placeholder", "user@example.com", 3, "ngModel", "ngModelChange"], ["matInput", "", "rows", "2", "placeholder", "Note opzionali su questo account", 3, "ngModel", "ngModelChange"], [3, "value"], ["matInput", "", "placeholder", "ghp_xxxxxxxxxxxxxxxxxxxx", "required", "", 3, "ngModel", "type", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", "type", "button", 3, "click"], [1, "link", 3, "click"], ["matInput", "", "placeholder", "glpat-xxxxxxxxxxxxxxxxxxxx", "required", "", 3, "ngModel", "type", "ngModelChange"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"]], template: function GitAccountManagementDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "Gestione Account Git");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-dialog-content", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "folder");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "span");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "Repository: ");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](10, "mat-divider");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, GitAccountManagementDialogComponent_div_11_Template, 4, 0, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, GitAccountManagementDialogComponent_div_12_Template, 31, 10, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitAccountManagementDialogComponent_div_13_Template, 11, 0, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, GitAccountManagementDialogComponent_div_14_Template, 39, 9, "div", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-dialog-actions", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "button", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_Template_button_click_16_listener() { return ctx.close(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "Chiudi");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](18, GitAccountManagementDialogComponent_ng_container_18_Template, 7, 4, "ng-container", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](19, GitAccountManagementDialogComponent_button_19_Template, 4, 1, "button", 10);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx.data.repositoryName);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.showCreateForm && ctx.currentAccount);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.showCreateForm && !ctx.currentAccount);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && ctx.showCreateForm);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.showCreateForm);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.showCreateForm && ctx.currentAccount);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_4__["MatIcon"], _angular_material_divider__WEBPACK_IMPORTED_MODULE_5__["MatDivider"], _angular_common__WEBPACK_IMPORTED_MODULE_6__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__["MatSpinner"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardContent"], _angular_material_card__WEBPACK_IMPORTED_MODULE_9__["MatCardActions"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatFormField"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatLabel"], _angular_material_input__WEBPACK_IMPORTED_MODULE_11__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["RequiredValidator"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgModel"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatSuffix"], _angular_material_select__WEBPACK_IMPORTED_MODULE_13__["MatSelect"], _angular_common__WEBPACK_IMPORTED_MODULE_6__["NgForOf"], _angular_material_core__WEBPACK_IMPORTED_MODULE_14__["MatOption"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatHint"]], styles: [".dialog-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n  min-height: 400px;\n}\n\n.repository-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 16px 0;\n  color: rgba(0, 0, 0, 0.6);\n}\n\n.repository-info[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.54);\n}\n\n.repository-info[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.87);\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px 0;\n  gap: 16px;\n}\n\n.current-account[_ngcontent-%COMP%] {\n  padding: 16px 0;\n}\n\n.current-account[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n  font-size: 18px;\n  font-weight: 500;\n}\n\n.account-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n\n.account-card[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 20px;\n}\n\n.account-card[_ngcontent-%COMP%]   .status-icon.success[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.account-detail[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 8px 0;\n  color: rgba(0, 0, 0, 0.87);\n}\n\n.account-detail[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.54);\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n\n.account-detail[_ngcontent-%COMP%]   .notes[_ngcontent-%COMP%] {\n  font-style: italic;\n  color: rgba(0, 0, 0, 0.6);\n}\n\n.account-detail[_ngcontent-%COMP%]   .credential-status[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n\n.account-detail[_ngcontent-%COMP%]   .credential-status.success[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.no-account[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px 0;\n  text-align: center;\n  gap: 16px;\n}\n\n.no-account[_ngcontent-%COMP%]   .large-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  width: 64px;\n  height: 64px;\n  color: rgba(0, 0, 0, 0.38);\n}\n\n.no-account[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 20px;\n  font-weight: 500;\n}\n\n.no-account[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: rgba(0, 0, 0, 0.6);\n  max-width: 400px;\n}\n\n.account-form[_ngcontent-%COMP%] {\n  padding: 16px 0;\n}\n\n.account-form[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n  font-size: 18px;\n  font-weight: 500;\n}\n\n.account-form[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 24px 0 16px;\n  font-size: 16px;\n  font-weight: 500;\n  color: rgba(0, 0, 0, 0.6);\n}\n\n.account-form[_ngcontent-%COMP%]   .full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.account-form[_ngcontent-%COMP%]   mat-divider[_ngcontent-%COMP%] {\n  margin: 24px 0;\n}\n\n.account-form[_ngcontent-%COMP%]   .link[_ngcontent-%COMP%] {\n  color: #1976d2;\n  cursor: pointer;\n  text-decoration: none;\n}\n\n.account-form[_ngcontent-%COMP%]   .link[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  gap: 8px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LWFjY291bnQtbWFuYWdlbWVudC1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0VBQ0EsaUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EseUJBQUE7QUFDRjs7QUFDRTtFQUNFLDBCQUFBO0FBQ0o7O0FBRUU7RUFDRSwwQkFBQTtBQUFKOztBQUlBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGVBQUE7RUFDQSxTQUFBO0FBREY7O0FBSUE7RUFDRSxlQUFBO0FBREY7O0FBR0U7RUFDRSxtQkFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQURKOztBQU1FO0VBQ0UsbUJBQUE7QUFISjs7QUFNRTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0FBSko7O0FBUUk7RUFDRSxjQUFBO0FBTk47O0FBV0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsY0FBQTtFQUNBLDBCQUFBO0FBUkY7O0FBVUU7RUFDRSwwQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQVJKOztBQVdFO0VBQ0Usa0JBQUE7RUFDQSx5QkFBQTtBQVRKOztBQVlFO0VBQ0UsaUJBQUE7QUFWSjs7QUFZSTtFQUNFLGNBQUE7QUFWTjs7QUFlQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxTQUFBO0FBWkY7O0FBY0U7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSwwQkFBQTtBQVpKOztBQWVFO0VBQ0UsU0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQWJKOztBQWdCRTtFQUNFLFNBQUE7RUFDQSx5QkFBQTtFQUNBLGdCQUFBO0FBZEo7O0FBa0JBO0VBQ0UsZUFBQTtBQWZGOztBQWlCRTtFQUNFLG1CQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBZko7O0FBa0JFO0VBQ0UsbUJBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSx5QkFBQTtBQWhCSjs7QUFtQkU7RUFDRSxXQUFBO0FBakJKOztBQW9CRTtFQUNFLGNBQUE7QUFsQko7O0FBcUJFO0VBQ0UsY0FBQTtFQUNBLGVBQUE7RUFDQSxxQkFBQTtBQW5CSjs7QUFxQkk7RUFDRSwwQkFBQTtBQW5CTjs7QUF3QkE7RUFDRSxrQkFBQTtFQUNBLFFBQUE7QUFyQkY7O0FBd0JJO0VBQ0UsaUJBQUE7QUF0Qk4iLCJmaWxlIjoiZ2l0LWFjY291bnQtbWFuYWdlbWVudC1kaWFsb2cuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuZGlhbG9nLWNvbnRlbnQge1xyXG4gIG1pbi13aWR0aDogNTAwcHg7XHJcbiAgbWF4LXdpZHRoOiA2MDBweDtcclxuICBtaW4taGVpZ2h0OiA0MDBweDtcclxufVxyXG5cclxuLnJlcG9zaXRvcnktaW5mbyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIHBhZGRpbmc6IDE2cHggMDtcclxuICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjU0KTtcclxuICB9XHJcblxyXG4gIHN0cm9uZyB7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjg3KTtcclxuICB9XHJcbn1cclxuXHJcbi5sb2FkaW5nLWNvbnRhaW5lciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNDBweCAwO1xyXG4gIGdhcDogMTZweDtcclxufVxyXG5cclxuLmN1cnJlbnQtYWNjb3VudCB7XHJcbiAgcGFkZGluZzogMTZweCAwO1xyXG5cclxuICBoMyB7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gICAgZm9udC1zaXplOiAxOHB4O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICB9XHJcbn1cclxuXHJcbi5hY2NvdW50LWNhcmQge1xyXG4gIG1hdC1jYXJkLWhlYWRlciB7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gIH1cclxuXHJcbiAgbWF0LWNhcmQtdGl0bGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICB9XHJcblxyXG4gIC5zdGF0dXMtaWNvbiB7XHJcbiAgICAmLnN1Y2Nlc3Mge1xyXG4gICAgICBjb2xvcjogIzRjYWY1MDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5hY2NvdW50LWRldGFpbCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogMTJweDtcclxuICBwYWRkaW5nOiA4cHggMDtcclxuICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjg3KTtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC41NCk7XHJcbiAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICB3aWR0aDogMjBweDtcclxuICAgIGhlaWdodDogMjBweDtcclxuICB9XHJcblxyXG4gIC5ub3RlcyB7XHJcbiAgICBmb250LXN0eWxlOiBpdGFsaWM7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gIH1cclxuXHJcbiAgLmNyZWRlbnRpYWwtc3RhdHVzIHtcclxuICAgIG1hcmdpbi1sZWZ0OiBhdXRvO1xyXG5cclxuICAgICYuc3VjY2VzcyB7XHJcbiAgICAgIGNvbG9yOiAjNGNhZjUwO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLm5vLWFjY291bnQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDQwcHggMDtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgZ2FwOiAxNnB4O1xyXG5cclxuICAubGFyZ2UtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDY0cHg7XHJcbiAgICB3aWR0aDogNjRweDtcclxuICAgIGhlaWdodDogNjRweDtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuMzgpO1xyXG4gIH1cclxuXHJcbiAgaDMge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC42KTtcclxuICAgIG1heC13aWR0aDogNDAwcHg7XHJcbiAgfVxyXG59XHJcblxyXG4uYWNjb3VudC1mb3JtIHtcclxuICBwYWRkaW5nOiAxNnB4IDA7XHJcblxyXG4gIGgzIHtcclxuICAgIG1hcmdpbi1ib3R0b206IDI0cHg7XHJcbiAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gIH1cclxuXHJcbiAgaDQge1xyXG4gICAgbWFyZ2luOiAyNHB4IDAgMTZweDtcclxuICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gIH1cclxuXHJcbiAgLmZ1bGwtd2lkdGgge1xyXG4gICAgd2lkdGg6IDEwMCU7XHJcbiAgfVxyXG5cclxuICBtYXQtZGl2aWRlciB7XHJcbiAgICBtYXJnaW46IDI0cHggMDtcclxuICB9XHJcblxyXG4gIC5saW5rIHtcclxuICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gICAgY3Vyc29yOiBwb2ludGVyO1xyXG4gICAgdGV4dC1kZWNvcmF0aW9uOiBub25lO1xyXG5cclxuICAgICY6aG92ZXIge1xyXG4gICAgICB0ZXh0LWRlY29yYXRpb246IHVuZGVybGluZTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgcGFkZGluZzogMTZweCAyNHB4O1xyXG4gIGdhcDogOHB4O1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBtYXJnaW4tcmlnaHQ6IDRweDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuIl19 */"] });


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

/***/ "pFRR":
/*!*****************************************************!*\
  !*** ./src/app/git/services/git-account.service.ts ***!
  \*****************************************************/
/*! exports provided: GitAccountService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitAccountService", function() { return GitAccountService; });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");




class GitAccountService {
    constructor(http) {
        this.http = http;
        this.API_BASE = '/api/gitaccount';
    }
    /**
     * Gets the Git account configuration for a specific repository
     */
    getAccountForRepository(repositoryPath) {
        const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('repositoryPath', repositoryPath);
        return this.http.get(`${this.API_BASE}/for-repository`, { params }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(account => {
            // Convert dates from strings to Date objects
            if (account && account.createdAt) {
                account.createdAt = new Date(account.createdAt);
            }
            if (account && account.updatedAt) {
                account.updatedAt = new Date(account.updatedAt);
            }
            return account;
        }));
    }
    /**
     * Gets all configured Git accounts
     */
    getAllAccounts() {
        return this.http.get(this.API_BASE).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(accounts => accounts.map(account => {
            // Convert dates from strings to Date objects
            if (account.createdAt) {
                account.createdAt = new Date(account.createdAt);
            }
            if (account.updatedAt) {
                account.updatedAt = new Date(account.updatedAt);
            }
            return account;
        })));
    }
    /**
     * Creates a new Git account configuration
     */
    createAccount(request) {
        return this.http.post(this.API_BASE, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(account => {
            if (account.createdAt) {
                account.createdAt = new Date(account.createdAt);
            }
            if (account.updatedAt) {
                account.updatedAt = new Date(account.updatedAt);
            }
            return account;
        }));
    }
    /**
     * Updates an existing Git account configuration
     */
    updateAccount(id, request) {
        return this.http.put(`${this.API_BASE}/${id}`, request).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(account => {
            if (account.createdAt) {
                account.createdAt = new Date(account.createdAt);
            }
            if (account.updatedAt) {
                account.updatedAt = new Date(account.updatedAt);
            }
            return account;
        }));
    }
    /**
     * Deletes a Git account configuration
     */
    deleteAccount(id) {
        return this.http.delete(`${this.API_BASE}/${id}`);
    }
    /**
     * Checks if a Git account exists for a specific repository
     */
    hasAccountForRepository(repositoryPath) {
        const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('repositoryPath', repositoryPath);
        return this.http.get(`${this.API_BASE}/exists`, { params }).pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(response => response.exists));
    }
}
GitAccountService.ɵfac = function GitAccountService_Factory(t) { return new (t || GitAccountService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"])); };
GitAccountService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: GitAccountService, factory: GitAccountService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "t2Ho":
/*!********************************************************************************!*\
  !*** ./src/app/git/dialogs/git-history-dialog/git-history-dialog.component.ts ***!
  \********************************************************************************/
/*! exports provided: GitHistoryDialogComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "GitHistoryDialogComponent", function() { return GitHistoryDialogComponent; });
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_material_table__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/table */ "+0xr");
/* harmony import */ var _angular_material_sort__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/sort */ "Dh3D");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/gitservice.service */ "N73s");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");












const _c0 = ["graphCanvas"];
function GitHistoryDialogComponent_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "Caricamento cronologia commit...");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_4_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r1.error);
} }
function GitHistoryDialogComponent_div_5_div_10_th_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Hash ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_td_4_mat_icon_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "merge_type");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_td_4_Template(rf, ctx) { if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "td", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_5_div_10_td_4_Template_span_click_1_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r19); const commit_r16 = ctx.$implicit; const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3); return ctx_r18.copyHash(commit_r16.hash); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_5_div_10_td_4_mat_icon_3_Template, 2, 0, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const commit_r16 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", commit_r16.shortHash || commit_r16.hash.substring(0, 7), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", commit_r16.isMerge);
} }
function GitHistoryDialogComponent_div_5_div_10_th_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Data ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_td_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "td", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const commit_r20 = ctx.$implicit;
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", ctx_r8.formatDate(commit_r20.date), " ");
} }
function GitHistoryDialogComponent_div_5_div_10_th_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Autore ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_td_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "td", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const commit_r21 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", commit_r21.author, " ");
} }
function GitHistoryDialogComponent_div_5_div_10_th_12_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "th", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Messaggio ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_td_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "td", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const commit_r22 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", commit_r22.message, " ");
} }
function GitHistoryDialogComponent_div_5_div_10_tr_14_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](0, "tr", 38);
} }
function GitHistoryDialogComponent_div_5_div_10_tr_15_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](0, "tr", 39);
} }
function GitHistoryDialogComponent_div_5_div_10_div_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Nessun commit trovato ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "table", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerStart"](2, 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_5_div_10_th_3_Template, 2, 0, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](4, GitHistoryDialogComponent_div_5_div_10_td_4_Template, 4, 2, "td", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerStart"](5, 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitHistoryDialogComponent_div_5_div_10_th_6_Template, 2, 0, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, GitHistoryDialogComponent_div_5_div_10_td_7_Template, 2, 1, "td", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerStart"](8, 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, GitHistoryDialogComponent_div_5_div_10_th_9_Template, 2, 0, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitHistoryDialogComponent_div_5_div_10_td_10_Template, 2, 1, "td", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerStart"](11, 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](12, GitHistoryDialogComponent_div_5_div_10_th_12_Template, 2, 0, "th", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](13, GitHistoryDialogComponent_div_5_div_10_td_13_Template, 2, 1, "td", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](14, GitHistoryDialogComponent_div_5_div_10_tr_14_Template, 1, 0, "tr", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](15, GitHistoryDialogComponent_div_5_div_10_tr_15_Template, 1, 0, "tr", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](16, GitHistoryDialogComponent_div_5_div_10_div_16_Template, 2, 0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("dataSource", ctx_r3.dataSource);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("matHeaderRowDef", ctx_r3.displayedColumns);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("matRowDefColumns", ctx_r3.displayedColumns);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r3.commits.length === 0);
} }
function GitHistoryDialogComponent_div_5_div_11_div_3_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, " Nessun commit trovato ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} }
function GitHistoryDialogComponent_div_5_div_11_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "canvas", 42, 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_5_div_11_div_3_Template, 2, 0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r4.commits.length === 0);
} }
function GitHistoryDialogComponent_div_5_Template(rf, ctx) { if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_5_Template_button_click_2_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r27); const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r26.switchView("table"); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4, "table_chart");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, " Tabella ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_5_Template_button_click_6_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r27); const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](); return ctx_r28.switchView("graph"); });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "account_tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9, " Grafico ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitHistoryDialogComponent_div_5_div_10_Template, 17, 4, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, GitHistoryDialogComponent_div_5_div_11_Template, 4, 1, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("color", ctx_r2.selectedView === "table" ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("color", ctx_r2.selectedView === "graph" ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r2.selectedView === "table");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r2.selectedView === "graph");
} }
class GitHistoryDialogComponent {
    constructor(dialogRef, data, gitService) {
        this.dialogRef = dialogRef;
        this.data = data;
        this.gitService = gitService;
        this.isLoading = true;
        this.error = null;
        this.commits = [];
        this.displayedColumns = ['hash', 'date', 'author', 'message'];
        this.selectedView = 'table';
        this.dataSource = new _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatTableDataSource"]([]);
    }
    ngOnInit() {
        this.loadCommitHistory();
    }
    ngAfterViewInit() {
        this.dataSource.sort = this.sort;
    }
    loadCommitHistory() {
        this.isLoading = true;
        this.error = null;
        this.gitService.getCommitHistory(this.data.projectPath, 100).subscribe({
            next: (commits) => {
                this.isLoading = false;
                this.commits = commits;
                this.dataSource.data = commits;
                // If graph view is selected and we have commits, render the graph
                if (this.selectedView === 'graph' && commits.length > 0) {
                    setTimeout(() => this.renderGraph(), 100);
                }
            },
            error: (err) => {
                this.isLoading = false;
                this.error = 'Errore nel caricamento della cronologia dei commit';
                console.error('Error loading commit history:', err);
            }
        });
    }
    switchView(view) {
        this.selectedView = view;
        if (view === 'graph' && this.commits.length > 0) {
            setTimeout(() => this.renderGraph(), 100);
        }
    }
    renderGraph() {
        // Basic graph rendering using canvas
        // This is a simplified implementation - you can enhance it with @gitgraph/js
        if (!this.graphCanvas || !this.graphCanvas.nativeElement) {
            return;
        }
        const canvas = this.graphCanvas.nativeElement;
        const ctx = canvas.getContext('2d');
        if (!ctx)
            return;
        // Set canvas size
        canvas.width = canvas.offsetWidth;
        canvas.height = this.commits.length * 60 + 40;
        // Clear canvas
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        // Draw commits
        const commitHeight = 60;
        const nodeRadius = 6;
        const leftMargin = 20;
        this.commits.forEach((commit, index) => {
            const y = index * commitHeight + 30;
            // Draw line to next commit
            if (index < this.commits.length - 1) {
                ctx.strokeStyle = '#999';
                ctx.lineWidth = 2;
                ctx.beginPath();
                ctx.moveTo(leftMargin, y);
                ctx.lineTo(leftMargin, y + commitHeight);
                ctx.stroke();
            }
            // Draw commit node
            ctx.fillStyle = commit.isMerge ? '#ff6b6b' : '#4caf50';
            ctx.beginPath();
            ctx.arc(leftMargin, y, nodeRadius, 0, 2 * Math.PI);
            ctx.fill();
            // Draw commit info
            ctx.fillStyle = '#333';
            ctx.font = '12px monospace';
            ctx.fillText(commit.shortHash || commit.hash.substring(0, 7), leftMargin + 20, y - 5);
            ctx.font = '11px sans-serif';
            ctx.fillStyle = '#666';
            const dateStr = new Date(commit.date).toLocaleDateString();
            ctx.fillText(`${dateStr} - ${commit.author}`, leftMargin + 20, y + 8);
            // Truncate message if too long
            const maxMessageWidth = canvas.width - leftMargin - 40;
            let message = commit.message;
            if (ctx.measureText(message).width > maxMessageWidth) {
                while (ctx.measureText(message + '...').width > maxMessageWidth && message.length > 0) {
                    message = message.substring(0, message.length - 1);
                }
                message += '...';
            }
            ctx.fillText(message, leftMargin + 20, y + 20);
        });
    }
    formatDate(date) {
        const d = typeof date === 'string' ? new Date(date) : date;
        return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
    }
    copyHash(hash) {
        navigator.clipboard.writeText(hash);
    }
    onClose() {
        this.dialogRef.close();
    }
}
GitHistoryDialogComponent.ɵfac = function GitHistoryDialogComponent_Factory(t) { return new (t || GitHistoryDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogRef"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MAT_DIALOG_DATA"]), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_4__["GITService"])); };
GitHistoryDialogComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({ type: GitHistoryDialogComponent, selectors: [["app-git-history-dialog"]], viewQuery: function GitHistoryDialogComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵviewQuery"](_c0, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵviewQuery"](_angular_material_sort__WEBPACK_IMPORTED_MODULE_2__["MatSort"], 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵloadQuery"]()) && (ctx.graphCanvas = _t.first);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵloadQuery"]()) && (ctx.sort = _t.first);
    } }, decls: 9, vars: 4, consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], ["class", "error-container", 4, "ngIf"], ["class", "history-content", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "loading-container"], [1, "error-container"], ["color", "warn"], [1, "history-content"], [1, "view-toggle"], ["mat-button", "", 3, "color", "click"], ["class", "table-container", 4, "ngIf"], ["class", "graph-container", 4, "ngIf"], [1, "table-container"], ["mat-table", "", "matSort", "", 1, "commit-table", 3, "dataSource"], ["matColumnDef", "hash"], ["mat-header-cell", "", "mat-sort-header", "", 4, "matHeaderCellDef"], ["mat-cell", "", "class", "hash-cell", 4, "matCellDef"], ["matColumnDef", "date"], ["mat-cell", "", "class", "date-cell", 4, "matCellDef"], ["matColumnDef", "author"], ["mat-cell", "", "class", "author-cell", 4, "matCellDef"], ["matColumnDef", "message"], ["mat-header-cell", "", 4, "matHeaderCellDef"], ["mat-cell", "", "class", "message-cell", 4, "matCellDef"], ["mat-header-row", "", 4, "matHeaderRowDef"], ["mat-row", "", 4, "matRowDef", "matRowDefColumns"], ["class", "no-commits", 4, "ngIf"], ["mat-header-cell", "", "mat-sort-header", ""], ["mat-cell", "", 1, "hash-cell"], ["title", "Click to copy", 1, "hash-text", 3, "click"], ["class", "merge-icon", "title", "Merge commit", 4, "ngIf"], ["title", "Merge commit", 1, "merge-icon"], ["mat-cell", "", 1, "date-cell"], ["mat-cell", "", 1, "author-cell"], ["mat-header-cell", ""], ["mat-cell", "", 1, "message-cell"], ["mat-header-row", ""], ["mat-row", ""], [1, "no-commits"], [1, "graph-container"], [1, "graph-canvas"], ["graphCanvas", ""]], template: function GitHistoryDialogComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-dialog-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_3_Template, 4, 0, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](4, GitHistoryDialogComponent_div_4_Template, 5, 1, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, GitHistoryDialogComponent_div_5_Template, 12, 4, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "mat-dialog-actions", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitHistoryDialogComponent_Template_button_click_7_listener() { return ctx.onClose(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "Chiudi");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("Git History ", ctx.data.projectName ? "- " + ctx.data.projectName : "", "");
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.error && !ctx.isLoading);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.error);
    } }, directives: [_angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogTitle"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogContent"], _angular_common__WEBPACK_IMPORTED_MODULE_5__["NgIf"], _angular_material_dialog__WEBPACK_IMPORTED_MODULE_0__["MatDialogActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_7__["MatSpinner"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatTable"], _angular_material_sort__WEBPACK_IMPORTED_MODULE_2__["MatSort"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatColumnDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatHeaderCellDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatCellDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatHeaderRowDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatRowDef"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatHeaderCell"], _angular_material_sort__WEBPACK_IMPORTED_MODULE_2__["MatSortHeader"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatCell"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatHeaderRow"], _angular_material_table__WEBPACK_IMPORTED_MODULE_1__["MatRow"]], styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n}\n\n.error-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  color: #f44336;\n}\n\n.error-container[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  height: 48px;\n  width: 48px;\n  margin-bottom: 16px;\n}\n\n.history-content[_ngcontent-%COMP%] {\n  min-width: 800px;\n  max-height: 600px;\n  overflow: auto;\n}\n\n.view-toggle[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  margin-bottom: 16px;\n  padding: 8px;\n  border-bottom: 1px solid #e0e0e0;\n}\n\n.table-container[_ngcontent-%COMP%] {\n  overflow: auto;\n  max-height: 500px;\n}\n\n.commit-table[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%] {\n  font-family: monospace;\n}\n\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .hash-text[_ngcontent-%COMP%] {\n  cursor: pointer;\n  padding: 2px 4px;\n  border-radius: 3px;\n}\n\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .hash-text[_ngcontent-%COMP%]:hover {\n  background-color: #f0f0f0;\n}\n\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .merge-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-left: 8px;\n  color: #ff6b6b;\n  vertical-align: middle;\n}\n\n.commit-table[_ngcontent-%COMP%]   .date-cell[_ngcontent-%COMP%] {\n  white-space: nowrap;\n  font-size: 0.9em;\n}\n\n.commit-table[_ngcontent-%COMP%]   .author-cell[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n\n.commit-table[_ngcontent-%COMP%]   .message-cell[_ngcontent-%COMP%] {\n  max-width: 400px;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n\n.commit-table[_ngcontent-%COMP%]   tr[_ngcontent-%COMP%]:hover {\n  background-color: #f5f5f5;\n}\n\n.graph-container[_ngcontent-%COMP%] {\n  overflow: auto;\n  max-height: 500px;\n  padding: 16px;\n  background-color: #fafafa;\n  border-radius: 4px;\n}\n\n.graph-canvas[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 400px;\n  background-color: white;\n  border: 1px solid #e0e0e0;\n  border-radius: 4px;\n}\n\n.no-commits[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 40px;\n  color: #666;\n  font-style: italic;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 24px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXC4uXFwuLlxcZ2l0LWhpc3RvcnktZGlhbG9nLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtBQUNGOztBQUNFO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsbUJBQUE7QUFDSjs7QUFHQTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBQUY7O0FBR0E7RUFDRSxhQUFBO0VBQ0EsUUFBQTtFQUNBLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLGdDQUFBO0FBQUY7O0FBR0E7RUFDRSxjQUFBO0VBQ0EsaUJBQUE7QUFBRjs7QUFHQTtFQUNFLFdBQUE7QUFBRjs7QUFFRTtFQUNFLHNCQUFBO0FBQUo7O0FBRUk7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtBQUFOOztBQUVNO0VBQ0UseUJBQUE7QUFBUjs7QUFJSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtFQUNBLHNCQUFBO0FBRk47O0FBTUU7RUFDRSxtQkFBQTtFQUNBLGdCQUFBO0FBSko7O0FBT0U7RUFDRSxnQkFBQTtBQUxKOztBQVFFO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsbUJBQUE7QUFOSjs7QUFTRTtFQUNFLHlCQUFBO0FBUEo7O0FBV0E7RUFDRSxjQUFBO0VBQ0EsaUJBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtBQVJGOztBQVdBO0VBQ0UsV0FBQTtFQUNBLGlCQUFBO0VBQ0EsdUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0FBUkY7O0FBV0E7RUFDRSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxXQUFBO0VBQ0Esa0JBQUE7QUFSRjs7QUFXQTtFQUNFLGVBQUE7QUFSRjs7QUFXQTtFQUNFLGtCQUFBO0FBUkYiLCJmaWxlIjoiZ2l0LWhpc3RvcnktZGlhbG9nLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4O1xyXG59XHJcblxyXG4uZXJyb3ItY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4O1xyXG4gIGNvbG9yOiAjZjQ0MzM2O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDQ4cHg7XHJcbiAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICB3aWR0aDogNDhweDtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG59XHJcblxyXG4uaGlzdG9yeS1jb250ZW50IHtcclxuICBtaW4td2lkdGg6IDgwMHB4O1xyXG4gIG1heC1oZWlnaHQ6IDYwMHB4O1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG59XHJcblxyXG4udmlldy10b2dnbGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICBwYWRkaW5nOiA4cHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcbn1cclxuXHJcbi50YWJsZS1jb250YWluZXIge1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG4gIG1heC1oZWlnaHQ6IDUwMHB4O1xyXG59XHJcblxyXG4uY29tbWl0LXRhYmxlIHtcclxuICB3aWR0aDogMTAwJTtcclxuXHJcbiAgLmhhc2gtY2VsbCB7XHJcbiAgICBmb250LWZhbWlseTogbW9ub3NwYWNlO1xyXG5cclxuICAgIC5oYXNoLXRleHQge1xyXG4gICAgICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgICAgIHBhZGRpbmc6IDJweCA0cHg7XHJcbiAgICAgIGJvcmRlci1yYWRpdXM6IDNweDtcclxuXHJcbiAgICAgICY6aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6ICNmMGYwZjA7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAubWVyZ2UtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgd2lkdGg6IDE2cHg7XHJcbiAgICAgIGhlaWdodDogMTZweDtcclxuICAgICAgbWFyZ2luLWxlZnQ6IDhweDtcclxuICAgICAgY29sb3I6ICNmZjZiNmI7XHJcbiAgICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuZGF0ZS1jZWxsIHtcclxuICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbiAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gIH1cclxuXHJcbiAgLmF1dGhvci1jZWxsIHtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG5cclxuICAubWVzc2FnZS1jZWxsIHtcclxuICAgIG1heC13aWR0aDogNDAwcHg7XHJcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIH1cclxuXHJcbiAgdHI6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuICB9XHJcbn1cclxuXHJcbi5ncmFwaC1jb250YWluZXIge1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG4gIG1heC1oZWlnaHQ6IDUwMHB4O1xyXG4gIHBhZGRpbmc6IDE2cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZhZmFmYTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbn1cclxuXHJcbi5ncmFwaC1jYW52YXMge1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIG1pbi1oZWlnaHQ6IDQwMHB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6IHdoaXRlO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMGUwZTA7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG59XHJcblxyXG4ubm8tY29tbWl0cyB7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDQwcHg7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgZm9udC1zdHlsZTogaXRhbGljO1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDAgMjRweDtcclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcbn0iXX0= */"] });


/***/ })

}]);
//# sourceMappingURL=default~md-explorer-md-explorer-module~projects-projects-module.js.map