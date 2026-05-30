"use strict";
(self["webpackChunkclient2"] = self["webpackChunkclient2"] || []).push([["default-src_app_git_git_module_ts-src_app_md-explorer_services_external-apps_service_ts-src_a-45d4e7"],{

/***/ 4386:
/*!**************************************************************************************!*\
  !*** ./src/app/git/dialogs/commit-message-dialog/commit-message-dialog.component.ts ***!
  \**************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "CommitMessageDialogComponent": () => (/* binding */ CommitMessageDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/forms */ 2508);












function CommitMessageDialogComponent_mat_spinner_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](0, "mat-spinner", 11);
  }
}
function CommitMessageDialogComponent_div_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 12)(1, "mat-icon", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", ctx_r1.aiError, " ");
  }
}
class CommitMessageDialogComponent {
  constructor(dialogRef, data, http, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.http = http;
    this.translate = translate;
    this.isGeneratingMessage = false;
    this.aiError = null;
    this.commitMessage = data.defaultMessage || this.translate.instant('GIT_COMMIT.DEFAULT_MSG');
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
      this.aiError = this.translate.instant('GIT_COMMIT.NO_PROJECT_PATH');
      return;
    }
    this.isGeneratingMessage = true;
    this.aiError = null;
    this.http.post('/api/GitAi/generate-commit-message', {
      projectPath: this.data.projectPath,
      language: this.translate.currentLang || this.translate.defaultLang || 'en'
    }).subscribe({
      next: response => {
        this.isGeneratingMessage = false;
        if (response.success && response.suggestedMessage) {
          this.commitMessage = response.suggestedMessage;
        } else if (response.error) {
          this.aiError = response.error;
          if (response.suggestedMessage) {
            // Use fallback message if provided
            this.commitMessage = response.suggestedMessage;
          }
        }
      },
      error: err => {
        this.isGeneratingMessage = false;
        this.aiError = this.translate.instant('GIT_COMMIT.GENERATION_ERROR');
        console.error('Error generating commit message:', err);
      }
    });
  }
  static {
    this.ɵfac = function CommitMessageDialogComponent_Factory(t) {
      return new (t || CommitMessageDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_2__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: CommitMessageDialogComponent,
      selectors: [["app-commit-message-dialog"]],
      decls: 29,
      vars: 26,
      consts: [["mat-dialog-title", ""], ["appearance", "outline", 2, "width", "100%", "min-width", "400px"], ["matInput", "", "rows", "4", "cdkFocusInitial", "", 3, "ngModel", "placeholder", "disabled", "ngModelChange", "keydown.enter"], [2, "margin-top", "16px", "display", "flex", "align-items", "center", "gap", "8px"], ["mat-stroked-button", "", "color", "accent", 3, "disabled", "click"], [2, "margin-right", "4px"], ["diameter", "20", "strokeWidth", "2", 4, "ngIf"], ["style", "margin-top: 8px; color: #f44336; font-size: 12px;", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "strokeWidth", "2"], [2, "margin-top", "8px", "color", "#f44336", "font-size", "12px"], [2, "font-size", "16px", "vertical-align", "middle"]],
      template: function CommitMessageDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "mat-dialog-content")(4, "mat-form-field", 1)(5, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](7, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "textarea", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function CommitMessageDialogComponent_Template_textarea_ngModelChange_8_listener($event) {
            return ctx.commitMessage = $event;
          })("keydown.enter", function CommitMessageDialogComponent_Template_textarea_keydown_enter_8_listener($event) {
            return $event.ctrlKey && ctx.onConfirm();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](9, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10, "    ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](13, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "div", 3)(15, "button", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_15_listener() {
            return ctx.generateWithAi();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "mat-icon", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](17, "smart_toy");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](19, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](20, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](21, CommitMessageDialogComponent_mat_spinner_21_Template, 1, 0, "mat-spinner", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](22, CommitMessageDialogComponent_div_22_Template, 4, 1, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "mat-dialog-actions", 8)(24, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_24_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](26, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](27, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function CommitMessageDialogComponent_Template_button_click_27_listener() {
            return ctx.onConfirm();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](28, " Commit ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 12, "GIT_COMMIT.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](7, 14, "GIT_COMMIT.LABEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.commitMessage)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](9, 16, "GIT_COMMIT.PLACEHOLDER"))("disabled", ctx.isGeneratingMessage);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](13, 18, "GIT_COMMIT.HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("disabled", ctx.isGeneratingMessage);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", ctx.isGeneratingMessage ? _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](19, 20, "GIT_COMMIT.GENERATING") : _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](20, 22, "GIT_COMMIT.GENERATE_AI"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.isGeneratingMessage);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.aiError);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](26, 24, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("disabled", !ctx.commitMessage || !ctx.commitMessage.trim() || ctx.isGeneratingMessage);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_4__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyLabel, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__.MatLegacyInput, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__.TranslatePipe],
      styles: [".mat-dialog-container {\n  padding: 24px;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-form-field[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n\ntextarea[_ngcontent-%COMP%] {\n  font-family: monospace;\n  font-size: 14px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  padding: 0;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvY29tbWl0LW1lc3NhZ2UtZGlhbG9nL2NvbW1pdC1tZXNzYWdlLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7QUFDRjs7QUFFQTtFQUNFLGNBQUE7QUFDRjs7QUFFQTtFQUNFLGVBQUE7QUFDRjs7QUFFQTtFQUNFLHNCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsZ0JBQUE7RUFDQSxVQUFBO0FBQ0YiLCJzb3VyY2VzQ29udGVudCI6WyI6Om5nLWRlZXAgLm1hdC1kaWFsb2ctY29udGFpbmVyIHtcclxuICBwYWRkaW5nOiAyNHB4O1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIG1hcmdpbjogMjBweCAwO1xyXG59XHJcblxyXG5tYXQtZm9ybS1maWVsZCB7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG59XHJcblxyXG50ZXh0YXJlYSB7XHJcbiAgZm9udC1mYW1pbHk6IG1vbm9zcGFjZTtcclxuICBmb250LXNpemU6IDE0cHg7XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgbWFyZ2luLXRvcDogMTZweDtcclxuICBwYWRkaW5nOiAwO1xyXG59Il0sInNvdXJjZVJvb3QiOiIifQ== */"]
    });
  }
}

/***/ }),

/***/ 3148:
/*!******************************************************************************************************!*\
  !*** ./src/app/git/dialogs/git-account-management-dialog/git-account-management-dialog.component.ts ***!
  \******************************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitAccountManagementDialogComponent": () => (/* binding */ GitAccountManagementDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_git_account_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../services/git-account.service */ 7360);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/divider */ 1528);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/forms */ 2508);


















function GitAccountManagementDialogComponent_div_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 1, "COMMON.LOADING"));
  }
}
function GitAccountManagementDialogComponent_div_14_div_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r6.currentAccount.username);
  }
}
function GitAccountManagementDialogComponent_div_14_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "email");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r7.currentAccount.email);
  }
}
function GitAccountManagementDialogComponent_div_14_span_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_ACCOUNT.GITHUB_TOKEN_CONFIGURED"));
  }
}
function GitAccountManagementDialogComponent_div_14_span_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_ACCOUNT.GITLAB_TOKEN_CONFIGURED"));
  }
}
function GitAccountManagementDialogComponent_div_14_span_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_ACCOUNT.BITBUCKET_APP_PASSWORD"));
  }
}
function GitAccountManagementDialogComponent_div_14_span_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_ACCOUNT.HTTPS_PASSWORD"));
  }
}
function GitAccountManagementDialogComponent_div_14_span_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_ACCOUNT.NO_CREDENTIALS"));
  }
}
function GitAccountManagementDialogComponent_div_14_div_25_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "account_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 2, "GIT_ACCOUNT.AUTH_USERNAME"), " ", ctx_r13.currentAccount.authUsername, "");
  }
}
function GitAccountManagementDialogComponent_div_14_div_26_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "security");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 2, "GIT_ACCOUNT.METHOD"), " ", ctx_r14.currentAccount.preferredAuthMethod === "username_password" ? _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](6, 4, "GIT_ACCOUNT.METHOD_USER_PASS") : _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](7, 6, "GIT_ACCOUNT.METHOD_TOKEN"), "");
  }
}
function GitAccountManagementDialogComponent_div_14_div_27_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "note");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r15.currentAccount.notes);
  }
}
function GitAccountManagementDialogComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 13)(1, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-card", 14)(5, "mat-card-header")(6, "mat-card-title")(7, "mat-icon", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitAccountManagementDialogComponent_div_14_div_13_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, GitAccountManagementDialogComponent_div_14_div_14_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "div", 17)(16, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "vpn_key");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](18, GitAccountManagementDialogComponent_div_14_span_18_Template, 3, 3, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](19, GitAccountManagementDialogComponent_div_14_span_19_Template, 3, 3, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](20, GitAccountManagementDialogComponent_div_14_span_20_Template, 3, 3, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](21, GitAccountManagementDialogComponent_div_14_span_21_Template, 3, 3, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](22, GitAccountManagementDialogComponent_div_14_span_22_Template, 3, 3, "span", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](23, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](25, GitAccountManagementDialogComponent_div_14_div_25_Template, 6, 4, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](26, GitAccountManagementDialogComponent_div_14_div_26_Template, 8, 8, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](27, GitAccountManagementDialogComponent_div_14_div_27_Template, 5, 1, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](28, "mat-card-actions")(29, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_Template_button_click_29_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r17);
      const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r16.editAccount());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](33, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](34, "button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_14_Template_button_click_34_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r17);
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r18.deleteAccount());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](35, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](36, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](38, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 18, "GIT_ACCOUNT.ACTIVE_ACCOUNT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
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
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.hasBitbucketAppPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.hasHttpsPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx_r1.currentAccount.hasGitHubPAT && !ctx_r1.currentAccount.hasGitLabToken && !ctx_r1.currentAccount.hasBitbucketAppPassword && !ctx_r1.currentAccount.hasHttpsPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("success", ctx_r1.currentAccount.hasGitHubPAT || ctx_r1.currentAccount.hasGitLabToken || ctx_r1.currentAccount.hasBitbucketAppPassword || ctx_r1.currentAccount.hasHttpsPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r1.currentAccount.hasGitHubPAT || ctx_r1.currentAccount.hasGitLabToken || ctx_r1.currentAccount.hasBitbucketAppPassword || ctx_r1.currentAccount.hasHttpsPassword ? "verified" : "error", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.authUsername);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.preferredAuthMethod);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r1.currentAccount.notes);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](33, 20, "GIT_ACCOUNT.EDIT_ACCOUNT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](38, 22, "COMMON.DELETE"), " ");
  }
}
function GitAccountManagementDialogComponent_div_15_Template(rf, ctx) {
  if (rf & 1) {
    const _r20 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 22)(1, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_15_Template_button_click_9_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r20);
      const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r19.showCreate());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 3, "GIT_ACCOUNT.NO_ACCOUNT_TITLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](8, 5, "GIT_ACCOUNT.NO_ACCOUNT_DESC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](13, 7, "GIT_ACCOUNT.CREATE_NEW"), " ");
  }
}
function GitAccountManagementDialogComponent_div_16_mat_option_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-option", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const type_r29 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("value", type_r29.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", type_r29.label, " ");
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_21_Template(rf, ctx) {
  if (rf & 1) {
    const _r31 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_21_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r31);
      const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r30.gitHubPAT = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_21_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r31);
      const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r32.hideToken = !ctx_r32.hideToken);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-hint")(10, "a", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_21_Template_a_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r31);
      const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r33.openTokenCreationPage());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 6, "GIT_ACCOUNT.GITHUB_PAT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r22.gitHubPAT)("type", ctx_r22.hideToken ? "password" : "text")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 8, "GIT_ACCOUNT.GITHUB_PAT_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r22.hideToken ? "visibility" : "visibility_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](12, 10, "GIT_ACCOUNT.CREATE_GITHUB_TOKEN"));
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_22_Template(rf, ctx) {
  if (rf & 1) {
    const _r35 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_22_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r35);
      const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r34.gitLabToken = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_22_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r35);
      const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r36.hideToken = !ctx_r36.hideToken);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-hint")(10, "a", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_22_Template_a_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r35);
      const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r37.openTokenCreationPage());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 6, "GIT_ACCOUNT.GITLAB_PAT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r23.gitLabToken)("type", ctx_r23.hideToken ? "password" : "text")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 8, "GIT_ACCOUNT.GITLAB_PAT_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r23.hideToken ? "visibility" : "visibility_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](12, 10, "GIT_ACCOUNT.CREATE_GITLAB_TOKEN"));
  }
}
function GitAccountManagementDialogComponent_div_16_div_23_Template(rf, ctx) {
  if (rf & 1) {
    const _r39 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 40)(1, "mat-label", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-radio-group", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_div_23_Template_mat_radio_group_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r39);
      const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r38.preferredAuthMethod = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-radio-button", 43)(6, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "mat-radio-button", 44)(11, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "vpn_key");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 4, "GIT_ACCOUNT.AUTH_METHOD"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r24.preferredAuthMethod);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](9, 6, "GIT_ACCOUNT.APP_PASSWORD"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](14, 8, "GIT_ACCOUNT.USER_PASSWORD"), " ");
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_24_Template(rf, ctx) {
  if (rf & 1) {
    const _r41 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_24_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r41);
      const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r40.bitbucketAppPassword = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_24_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r41);
      const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r42.hideToken = !ctx_r42.hideToken);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-hint")(10, "a", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_24_Template_a_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r41);
      const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r43.openTokenCreationPage());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 6, "GIT_ACCOUNT.BITBUCKET_APP_PASSWORD_LABEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r25.bitbucketAppPassword)("type", ctx_r25.hideToken ? "password" : "text")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 8, "GIT_ACCOUNT.BITBUCKET_APP_PASSWORD_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r25.hideToken ? "visibility" : "visibility_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](12, 10, "GIT_ACCOUNT.CREATE_BITBUCKET_APP"));
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_25_Template(rf, ctx) {
  if (rf & 1) {
    const _r45 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_25_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r45);
      const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r44.authUsername = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "account_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 4, "GIT_ACCOUNT.AUTH_USERNAME_LABEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r26.authUsername)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 6, "GIT_ACCOUNT.AUTH_USERNAME_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](10, 8, "GIT_ACCOUNT.AUTH_USERNAME_HINT"));
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_26_Template(rf, ctx) {
  if (rf & 1) {
    const _r47 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_26_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r47);
      const ctx_r46 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r46.httpsPassword = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "button", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_div_16_mat_form_field_26_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r47);
      const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r48.hideToken = !ctx_r48.hideToken);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 5, "GIT_ACCOUNT.PASSWORD"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r27.httpsPassword)("type", ctx_r27.hideToken ? "password" : "text")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 7, "GIT_ACCOUNT.PASSWORD_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r27.hideToken ? "visibility" : "visibility_off");
  }
}
function GitAccountManagementDialogComponent_div_16_mat_form_field_27_Template(rf, ctx) {
  if (rf & 1) {
    const _r50 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-form-field", 26)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "input", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_mat_form_field_27_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r50);
      const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r49.authUsername = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7, "account_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 4, "GIT_ACCOUNT.BITBUCKET_USERNAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r28.authUsername)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 6, "GIT_ACCOUNT.BITBUCKET_USERNAME_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](10, 8, "GIT_ACCOUNT.BITBUCKET_USERNAME_HINT"));
  }
}
function GitAccountManagementDialogComponent_div_16_Template(rf, ctx) {
  if (rf & 1) {
    const _r52 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 25)(1, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-form-field", 26)(6, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "input", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_Template_input_ngModelChange_9_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r52);
      const ctx_r51 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r51.accountName = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "badge");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "mat-form-field", 26)(14, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-select", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_Template_mat_select_ngModelChange_17_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r52);
      const ctx_r53 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r53.accountType = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](18, GitAccountManagementDialogComponent_div_16_mat_option_18_Template, 2, 2, "mat-option", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20, "cloud");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](21, GitAccountManagementDialogComponent_div_16_mat_form_field_21_Template, 13, 12, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](22, GitAccountManagementDialogComponent_div_16_mat_form_field_22_Template, 13, 12, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](23, GitAccountManagementDialogComponent_div_16_div_23_Template, 15, 10, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](24, GitAccountManagementDialogComponent_div_16_mat_form_field_24_Template, 13, 12, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](25, GitAccountManagementDialogComponent_div_16_mat_form_field_25_Template, 11, 10, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](26, GitAccountManagementDialogComponent_div_16_mat_form_field_26_Template, 9, 9, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](27, GitAccountManagementDialogComponent_div_16_mat_form_field_27_Template, 11, 10, "mat-form-field", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](28, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](29, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](30);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](31, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](32, "mat-form-field", 26)(33, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](34);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](35, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](36, "input", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_Template_input_ngModelChange_36_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r52);
      const ctx_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r54.username = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](37, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](38, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](39, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](40, "mat-form-field", 26)(41, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](42);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](43, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](44, "input", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_Template_input_ngModelChange_44_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r52);
      const ctx_r55 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r55.email = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](45, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](46, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](47, "email");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](48, "mat-form-field", 26)(49, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](50);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](51, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](52, "textarea", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function GitAccountManagementDialogComponent_div_16_Template_textarea_ngModelChange_52_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r52);
      const ctx_r56 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r56.notes = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](53, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](54, "mat-icon", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](55, "note");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r3.currentAccount ? _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 24, "GIT_ACCOUNT.EDIT_ACCOUNT") : _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 26, "GIT_ACCOUNT.NEW_ACCOUNT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](8, 28, "GIT_ACCOUNT.ACCOUNT_NAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.accountName)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](10, 30, "GIT_ACCOUNT.ACCOUNT_NAME_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](16, 32, "GIT_ACCOUNT.ACCOUNT_TYPE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.accountType);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r3.accountTypes);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "GitHub");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "GitLab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "Bitbucket");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "Bitbucket" && ctx_r3.preferredAuthMethod === "token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "Generic" || ctx_r3.accountType === "Bitbucket" && ctx_r3.preferredAuthMethod === "username_password");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "Generic" || ctx_r3.accountType === "Bitbucket" && ctx_r3.preferredAuthMethod === "username_password");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.accountType === "Bitbucket" && ctx_r3.preferredAuthMethod === "token");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](31, 34, "GIT_ACCOUNT.GIT_INFO_OPTIONAL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](35, 36, "GIT_ACCOUNT.GIT_USERNAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.username)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](37, 38, "GIT_ACCOUNT.GIT_USERNAME_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](43, 40, "GIT_ACCOUNT.GIT_EMAIL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.email)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](45, 42, "GIT_ACCOUNT.GIT_EMAIL_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](51, 44, "GIT_ACCOUNT.NOTES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r3.notes)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](53, 46, "GIT_ACCOUNT.NOTES_PLACEHOLDER"));
  }
}
function GitAccountManagementDialogComponent_ng_container_21_Template(rf, ctx) {
  if (rf & 1) {
    const _r58 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "button", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_ng_container_21_Template_button_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r58);
      const ctx_r57 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r57.cancelCreate());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "button", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_ng_container_21_Template_button_click_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r58);
      const ctx_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r59.currentAccount ? ctx_r59.updateAccount() : ctx_r59.saveAccount());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r4.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](3, 5, "COMMON.CANCEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r4.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r4.currentAccount ? "save" : "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r4.currentAccount ? _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](8, 7, "GIT_ACCOUNT.UPDATE") : _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](9, 9, "COMMON.SAVE"), " ");
  }
}
function GitAccountManagementDialogComponent_button_22_Template(rf, ctx) {
  if (rf & 1) {
    const _r61 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_button_22_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r61);
      const ctx_r60 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r60.showCreate());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r5.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 2, "GIT_ACCOUNT.CREATE_NEW"), " ");
  }
}
class GitAccountManagementDialogComponent {
  constructor(dialogRef, data, gitAccountService, snackBar, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitAccountService = gitAccountService;
    this.snackBar = snackBar;
    this.translate = translate;
    this.isLoading = false;
    this.currentAccount = null;
    this.showCreateForm = false;
    // Form fields
    this.accountName = '';
    this.accountType = 'GitHub';
    this.gitHubPAT = '';
    this.gitLabToken = '';
    this.bitbucketAppPassword = '';
    this.httpsPassword = '';
    this.authUsername = '';
    this.preferredAuthMethod = 'token';
    this.username = '';
    this.email = '';
    this.notes = '';
    this.hideToken = true;
    this.accountTypes = [{
      value: 'GitHub',
      label: 'GitHub'
    }, {
      value: 'GitLab',
      label: 'GitLab'
    }, {
      value: 'Bitbucket',
      label: 'Bitbucket'
    }, {
      value: 'Generic',
      label: 'Generic Git'
    }];
  }
  ngOnInit() {
    this.loadCurrentAccount();
  }
  loadCurrentAccount() {
    this.isLoading = true;
    this.gitAccountService.getAccountForRepository(this.data.repositoryPath).subscribe({
      next: account => {
        this.currentAccount = account;
        this.isLoading = false;
      },
      error: err => {
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
    this.bitbucketAppPassword = '';
    this.httpsPassword = '';
    this.authUsername = '';
    this.preferredAuthMethod = 'token';
    this.username = '';
    this.email = '';
    this.notes = '';
  }
  saveAccount() {
    // Validation
    if (!this.accountName.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.ACCOUNT_NAME_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }
    if (this.accountType === 'GitHub' && !this.gitHubPAT.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GITHUB_TOKEN_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }
    if (this.accountType === 'GitLab' && !this.gitLabToken.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GITLAB_TOKEN_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }
    if (this.accountType === 'Bitbucket') {
      if (this.preferredAuthMethod === 'token' && !this.bitbucketAppPassword.trim()) {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.BITBUCKET_PASSWORD_REQUIRED'), 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
        return;
      }
      if (this.preferredAuthMethod === 'username_password' && (!this.authUsername.trim() || !this.httpsPassword.trim())) {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.USER_PASS_REQUIRED'), 'OK', {
          duration: 3000,
          verticalPosition: 'top'
        });
        return;
      }
    }
    if (this.accountType === 'Generic' && (!this.authUsername.trim() || !this.httpsPassword.trim())) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.GENERIC_CREDS_REQUIRED'), 'OK', {
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
      bitbucketAppPassword: this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'token' ? this.bitbucketAppPassword.trim() : undefined,
      httpsPassword: this.accountType === 'Generic' || this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'username_password' ? this.httpsPassword.trim() : undefined,
      authUsername: this.accountType === 'Generic' || this.accountType === 'Bitbucket' ? this.authUsername.trim() : undefined,
      preferredAuthMethod: this.accountType === 'Bitbucket' || this.accountType === 'Generic' ? this.preferredAuthMethod : undefined,
      username: this.username.trim() || undefined,
      email: this.email.trim() || undefined,
      notes: this.notes.trim() || undefined,
      isActive: true
    };
    this.isLoading = true;
    this.gitAccountService.createAccount(request).subscribe({
      next: account => {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.CREATED_SUCCESS'), 'OK', {
          duration: 3000,
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.currentAccount = account;
        this.showCreateForm = false;
        this.resetForm();
        this.isLoading = false;
      },
      error: err => {
        console.error('Error creating account:', err);
        const message = err.error?.error || err.error?.title || this.translate.instant('GIT_ACCOUNT.CREATE_ERROR');
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
    if (!this.currentAccount || !this.currentAccount.id) return;
    const confirmed = confirm(`Vuoi davvero eliminare l'account "${this.currentAccount.accountName}"?`);
    if (!confirmed) return;
    this.isLoading = true;
    this.gitAccountService.deleteAccount(this.currentAccount.id).subscribe({
      next: result => {
        if (result.success) {
          this.snackBar.open(this.translate.instant('GIT_ACCOUNT.DELETED_SUCCESS'), 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
          this.currentAccount = null;
          this.isLoading = false;
        }
      },
      error: err => {
        console.error('Error deleting account:', err);
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.DELETE_ERROR'), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
      }
    });
  }
  editAccount() {
    if (!this.currentAccount) return;
    // Fill form with current account data
    this.accountName = this.currentAccount.accountName;
    this.accountType = this.currentAccount.accountType;
    this.authUsername = this.currentAccount.authUsername || '';
    // SSH is handled automatically, so default to 'token' if ssh is set
    const method = this.currentAccount.preferredAuthMethod;
    this.preferredAuthMethod = method === 'token' || method === 'username_password' ? method : 'token';
    this.username = this.currentAccount.username || '';
    this.email = this.currentAccount.email || '';
    this.notes = this.currentAccount.notes || '';
    // Don't pre-fill tokens/passwords for security
    this.showCreateForm = true;
  }
  updateAccount() {
    if (!this.currentAccount || !this.currentAccount.id) return;
    // Validation similar to saveAccount
    if (!this.accountName.trim()) {
      this.snackBar.open(this.translate.instant('GIT_ACCOUNT.ACCOUNT_NAME_REQUIRED'), 'OK', {
        duration: 3000,
        verticalPosition: 'top'
      });
      return;
    }
    const request = {
      repositoryPath: this.data.repositoryPath,
      accountName: this.accountName.trim(),
      accountType: this.accountType,
      authUsername: this.authUsername.trim() || undefined,
      preferredAuthMethod: this.accountType === 'Bitbucket' || this.accountType === 'Generic' ? this.preferredAuthMethod : undefined,
      username: this.username.trim() || undefined,
      email: this.email.trim() || undefined,
      notes: this.notes.trim() || undefined,
      isActive: true
    };
    // Only include credentials if user provided new ones
    if (this.accountType === 'GitHub' && this.gitHubPAT.trim()) {
      request.gitHubPAT = this.gitHubPAT.trim();
    }
    if (this.accountType === 'GitLab' && this.gitLabToken.trim()) {
      request.gitLabToken = this.gitLabToken.trim();
    }
    if (this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'token' && this.bitbucketAppPassword.trim()) {
      request.bitbucketAppPassword = this.bitbucketAppPassword.trim();
    }
    if ((this.accountType === 'Generic' || this.accountType === 'Bitbucket' && this.preferredAuthMethod === 'username_password') && this.httpsPassword.trim()) {
      request.httpsPassword = this.httpsPassword.trim();
    }
    this.isLoading = true;
    this.gitAccountService.updateAccount(this.currentAccount.id, request).subscribe({
      next: account => {
        this.snackBar.open(this.translate.instant('GIT_ACCOUNT.UPDATED_SUCCESS'), 'OK', {
          duration: 3000,
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.currentAccount = account;
        this.showCreateForm = false;
        this.resetForm();
        this.isLoading = false;
      },
      error: err => {
        console.error('Error updating account:', err);
        const message = err.error?.error || this.translate.instant('GIT_ACCOUNT.UPDATE_ERROR');
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
    } else if (this.accountType === 'GitLab') {
      window.open('https://gitlab.com/-/profile/personal_access_tokens', '_blank');
    } else if (this.accountType === 'Bitbucket') {
      window.open('https://bitbucket.org/account/settings/app-passwords/', '_blank');
    }
  }
  close() {
    this.dialogRef.close(this.currentAccount !== null);
  }
  static {
    this.ɵfac = function GitAccountManagementDialogComponent_Factory(t) {
      return new (t || GitAccountManagementDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_git_account_service__WEBPACK_IMPORTED_MODULE_0__.GitAccountService), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_3__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({
      type: GitAccountManagementDialogComponent,
      selectors: [["app-git-account-management-dialog"]],
      decls: 23,
      vars: 17,
      consts: [["mat-dialog-title", ""], [1, "dialog-content"], [1, "repository-info"], ["class", "loading-container", 4, "ngIf"], ["class", "current-account", 4, "ngIf"], ["class", "no-account", 4, "ngIf"], ["class", "account-form", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], [4, "ngIf"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click", 4, "ngIf"], [1, "loading-container"], ["diameter", "40"], [1, "current-account"], [1, "account-card"], [1, "status-icon", "success"], ["class", "account-detail", 4, "ngIf"], [1, "account-detail"], [1, "credential-status"], ["mat-button", "", "color", "primary", 3, "click"], ["mat-button", "", "color", "warn", 3, "click"], [1, "notes"], [1, "no-account"], [1, "large-icon"], ["mat-raised-button", "", "color", "primary", 3, "click"], [1, "account-form"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "required", "", 3, "ngModel", "placeholder", "ngModelChange"], ["matSuffix", ""], ["required", "", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["appearance", "outline", "class", "full-width", 4, "ngIf"], ["class", "auth-method-section", 4, "ngIf"], ["matInput", "", 3, "ngModel", "placeholder", "ngModelChange"], ["matInput", "", "type", "email", 3, "ngModel", "placeholder", "ngModelChange"], ["matInput", "", "rows", "2", 3, "ngModel", "placeholder", "ngModelChange"], [3, "value"], ["matInput", "", "required", "", 3, "ngModel", "type", "placeholder", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", "type", "button", 3, "click"], [1, "link", 3, "click"], [1, "auth-method-section"], [1, "field-label"], [1, "auth-radio-group", 3, "ngModel", "ngModelChange"], ["value", "token"], ["value", "username_password"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"]],
      template: function GitAccountManagementDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-dialog-content", 1)(4, "div", 2)(5, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "span");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](9, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "strong");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](12, "mat-divider");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitAccountManagementDialogComponent_div_13_Template, 5, 3, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, GitAccountManagementDialogComponent_div_14_Template, 39, 24, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](15, GitAccountManagementDialogComponent_div_15_Template, 14, 9, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, GitAccountManagementDialogComponent_div_16_Template, 56, 48, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](17, "mat-dialog-actions", 7)(18, "button", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitAccountManagementDialogComponent_Template_button_click_18_listener() {
            return ctx.close();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](19);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](20, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](21, GitAccountManagementDialogComponent_ng_container_21_Template, 10, 11, "ng-container", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](22, GitAccountManagementDialogComponent_button_22_Template, 5, 4, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 11, "GIT_ACCOUNT.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](9, 13, "GIT_ACCOUNT.REPOSITORY"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
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
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](20, 15, "COMMON.CLOSE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.showCreateForm);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.showCreateForm && ctx.currentAccount);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_5__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_5__.NgIf, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_6__.MatLegacyOption, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_7__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_7__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_7__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_7__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_8__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_9__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_9__.MatLegacyRadioButton, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_10__.MatLegacySelect, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCardHeader, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCardContent, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCardTitle, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCardSubtitle, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCardActions, _angular_material_divider__WEBPACK_IMPORTED_MODULE_12__.MatDivider, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_13__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_14__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_15__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__.TranslatePipe],
      styles: [".dialog-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n  min-height: 400px;\n}\n\n.repository-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 16px 0;\n  color: rgba(0, 0, 0, 0.6);\n}\n.repository-info[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.54);\n}\n.repository-info[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.87);\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px 0;\n  gap: 16px;\n}\n\n.current-account[_ngcontent-%COMP%] {\n  padding: 16px 0;\n}\n.current-account[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n  font-size: 18px;\n  font-weight: 500;\n}\n\n.account-card[_ngcontent-%COMP%]   mat-card-header[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n.account-card[_ngcontent-%COMP%]   mat-card-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 20px;\n}\n.account-card[_ngcontent-%COMP%]   .status-icon.success[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.account-detail[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 8px 0;\n  color: rgba(0, 0, 0, 0.87);\n}\n.account-detail[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.54);\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.account-detail[_ngcontent-%COMP%]   .notes[_ngcontent-%COMP%] {\n  font-style: italic;\n  color: rgba(0, 0, 0, 0.6);\n}\n.account-detail[_ngcontent-%COMP%]   .credential-status[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n.account-detail[_ngcontent-%COMP%]   .credential-status.success[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n\n.no-account[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px 0;\n  text-align: center;\n  gap: 16px;\n}\n.no-account[_ngcontent-%COMP%]   .large-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  width: 64px;\n  height: 64px;\n  color: rgba(0, 0, 0, 0.38);\n}\n.no-account[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 20px;\n  font-weight: 500;\n}\n.no-account[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: rgba(0, 0, 0, 0.6);\n  max-width: 400px;\n}\n\n.account-form[_ngcontent-%COMP%] {\n  padding: 16px 0;\n}\n.account-form[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n  font-size: 18px;\n  font-weight: 500;\n}\n.account-form[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 24px 0 16px;\n  font-size: 16px;\n  font-weight: 500;\n  color: rgba(0, 0, 0, 0.6);\n}\n.account-form[_ngcontent-%COMP%]   .full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n.account-form[_ngcontent-%COMP%]   mat-divider[_ngcontent-%COMP%] {\n  margin: 24px 0;\n}\n.account-form[_ngcontent-%COMP%]   .link[_ngcontent-%COMP%] {\n  color: #1976d2;\n  cursor: pointer;\n  text-decoration: none;\n}\n.account-form[_ngcontent-%COMP%]   .link[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  gap: 8px;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LWFjY291bnQtbWFuYWdlbWVudC1kaWFsb2cvZ2l0LWFjY291bnQtbWFuYWdlbWVudC1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0VBQ0EsaUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EseUJBQUE7QUFDRjtBQUNFO0VBQ0UsMEJBQUE7QUFDSjtBQUVFO0VBQ0UsMEJBQUE7QUFBSjs7QUFJQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxlQUFBO0VBQ0EsU0FBQTtBQURGOztBQUlBO0VBQ0UsZUFBQTtBQURGO0FBR0U7RUFDRSxtQkFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQURKOztBQU1FO0VBQ0UsbUJBQUE7QUFISjtBQU1FO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7QUFKSjtBQVFJO0VBQ0UsY0FBQTtBQU5OOztBQVdBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGNBQUE7RUFDQSwwQkFBQTtBQVJGO0FBVUU7RUFDRSwwQkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQVJKO0FBV0U7RUFDRSxrQkFBQTtFQUNBLHlCQUFBO0FBVEo7QUFZRTtFQUNFLGlCQUFBO0FBVko7QUFZSTtFQUNFLGNBQUE7QUFWTjs7QUFlQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxTQUFBO0FBWkY7QUFjRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLDBCQUFBO0FBWko7QUFlRTtFQUNFLFNBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFiSjtBQWdCRTtFQUNFLFNBQUE7RUFDQSx5QkFBQTtFQUNBLGdCQUFBO0FBZEo7O0FBa0JBO0VBQ0UsZUFBQTtBQWZGO0FBaUJFO0VBQ0UsbUJBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFmSjtBQWtCRTtFQUNFLG1CQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EseUJBQUE7QUFoQko7QUFtQkU7RUFDRSxXQUFBO0FBakJKO0FBb0JFO0VBQ0UsY0FBQTtBQWxCSjtBQXFCRTtFQUNFLGNBQUE7RUFDQSxlQUFBO0VBQ0EscUJBQUE7QUFuQko7QUFxQkk7RUFDRSwwQkFBQTtBQW5CTjs7QUF3QkE7RUFDRSxrQkFBQTtFQUNBLFFBQUE7QUFyQkY7QUF3Qkk7RUFDRSxpQkFBQTtBQXRCTiIsInNvdXJjZXNDb250ZW50IjpbIi5kaWFsb2ctY29udGVudCB7XHJcbiAgbWluLXdpZHRoOiA1MDBweDtcclxuICBtYXgtd2lkdGg6IDYwMHB4O1xyXG4gIG1pbi1oZWlnaHQ6IDQwMHB4O1xyXG59XHJcblxyXG4ucmVwb3NpdG9yeS1pbmZvIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgcGFkZGluZzogMTZweCAwO1xyXG4gIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNTQpO1xyXG4gIH1cclxuXHJcbiAgc3Ryb25nIHtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuODcpO1xyXG4gIH1cclxufVxyXG5cclxuLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4IDA7XHJcbiAgZ2FwOiAxNnB4O1xyXG59XHJcblxyXG4uY3VycmVudC1hY2NvdW50IHtcclxuICBwYWRkaW5nOiAxNnB4IDA7XHJcblxyXG4gIGgzIHtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gIH1cclxufVxyXG5cclxuLmFjY291bnQtY2FyZCB7XHJcbiAgbWF0LWNhcmQtaGVhZGVyIHtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG5cclxuICBtYXQtY2FyZC10aXRsZSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogOHB4O1xyXG4gICAgZm9udC1zaXplOiAyMHB4O1xyXG4gIH1cclxuXHJcbiAgLnN0YXR1cy1pY29uIHtcclxuICAgICYuc3VjY2VzcyB7XHJcbiAgICAgIGNvbG9yOiAjNGNhZjUwO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmFjY291bnQtZGV0YWlsIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDhweCAwO1xyXG4gIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuODcpO1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjU0KTtcclxuICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgaGVpZ2h0OiAyMHB4O1xyXG4gIH1cclxuXHJcbiAgLm5vdGVzIHtcclxuICAgIGZvbnQtc3R5bGU6IGl0YWxpYztcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcbiAgfVxyXG5cclxuICAuY3JlZGVudGlhbC1zdGF0dXMge1xyXG4gICAgbWFyZ2luLWxlZnQ6IGF1dG87XHJcblxyXG4gICAgJi5zdWNjZXNzIHtcclxuICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4ubm8tYWNjb3VudCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNDBweCAwO1xyXG4gIHRleHQtYWxpZ246IGNlbnRlcjtcclxuICBnYXA6IDE2cHg7XHJcblxyXG4gIC5sYXJnZS1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogNjRweDtcclxuICAgIHdpZHRoOiA2NHB4O1xyXG4gICAgaGVpZ2h0OiA2NHB4O1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC4zOCk7XHJcbiAgfVxyXG5cclxuICBoMyB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gICAgbWF4LXdpZHRoOiA0MDBweDtcclxuICB9XHJcbn1cclxuXHJcbi5hY2NvdW50LWZvcm0ge1xyXG4gIHBhZGRpbmc6IDE2cHggMDtcclxuXHJcbiAgaDMge1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxuICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG5cclxuICBoNCB7XHJcbiAgICBtYXJnaW46IDI0cHggMCAxNnB4O1xyXG4gICAgZm9udC1zaXplOiAxNnB4O1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcbiAgfVxyXG5cclxuICAuZnVsbC13aWR0aCB7XHJcbiAgICB3aWR0aDogMTAwJTtcclxuICB9XHJcblxyXG4gIG1hdC1kaXZpZGVyIHtcclxuICAgIG1hcmdpbjogMjRweCAwO1xyXG4gIH1cclxuXHJcbiAgLmxpbmsge1xyXG4gICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgICB0ZXh0LWRlY29yYXRpb246IG5vbmU7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIHRleHQtZGVjb3JhdGlvbjogdW5kZXJsaW5lO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcbiAgZ2FwOiA4cHg7XHJcblxyXG4gIGJ1dHRvbiB7XHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

/***/ }),

/***/ 6235:
/*!******************************************************************************!*\
  !*** ./src/app/git/dialogs/git-branch-dialog/git-branch-dialog.component.ts ***!
  \******************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitBranchDialogComponent": () => (/* binding */ GitBranchDialogComponent)
/* harmony export */ });
/* harmony import */ var C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/gitservice.service */ 7224);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/core */ 9121);
/* harmony import */ var _angular_material_legacy_list__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-list */ 744);
/* harmony import */ var _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-tabs */ 2821);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_legacy_chips__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-chips */ 9257);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/forms */ 2508);




















function GitBranchDialogComponent_mat_card_6_div_11_mat_chip_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-chip", 21)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", ctx_r7.currentBranch.howManyFilesAreChanged, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 2, "GIT_BRANCH.FILES_MODIFIED"), " ");
  }
}
function GitBranchDialogComponent_mat_card_6_div_11_mat_chip_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-chip", 22)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "upload");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", ctx_r8.currentBranch.howManyCommitAreToPush, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 2, "GIT_BRANCH.COMMITS_TO_PUSH"), " ");
  }
}
function GitBranchDialogComponent_mat_card_6_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 18)(1, "mat-chip-list");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](2, GitBranchDialogComponent_mat_card_6_div_11_mat_chip_2_Template, 5, 4, "mat-chip", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitBranchDialogComponent_mat_card_6_div_11_mat_chip_3_Template, 5, 4, "mat-chip", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r6.currentBranch.somethingIsChangedInTheBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r6.currentBranch.howManyCommitAreToPush > 0);
  }
}
function GitBranchDialogComponent_mat_card_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-card", 11)(1, "mat-card-content")(2, "div", 12)(3, "mat-icon", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "div", 14)(6, "span", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, GitBranchDialogComponent_mat_card_6_div_11_Template, 4, 2, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](8, 3, "GIT_BRANCH.CURRENT_BRANCH"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r0.currentBranch.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r0.currentBranch.somethingIsChangedInTheBranch || ctx_r0.currentBranch.howManyCommitAreToPush > 0);
  }
}
function GitBranchDialogComponent_mat_form_field_7_button_8_Template(rf, ctx) {
  if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "button", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_mat_form_field_7_button_8_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r11);
      const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      ctx_r10.searchTerm = "";
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r10.filterBranches());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
}
function GitBranchDialogComponent_mat_form_field_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r13 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-form-field", 23)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "input", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitBranchDialogComponent_mat_form_field_7_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r13);
      const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r12.searchTerm = $event);
    })("input", function GitBranchDialogComponent_mat_form_field_7_Template_input_input_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r13);
      const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r14.filterBranches());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "mat-icon", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, GitBranchDialogComponent_mat_form_field_7_button_8_Template, 3, 0, "button", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 4, "GIT_BRANCH.SEARCH_BRANCH"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.searchTerm)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 6, "GIT_BRANCH.SEARCH_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r1.searchTerm);
  }
}
function GitBranchDialogComponent_div_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 1, "GIT_BRANCH.LOADING_BRANCHES"));
  }
}
function GitBranchDialogComponent_mat_card_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-card", 30)(1, "mat-card-content")(2, "mat-icon", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_mat_card_9_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r16);
      const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r15.loadBranches());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r3.error);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](10, 2, "GIT_BRANCH.RETRY"), " ");
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_ng_template_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "span", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const group_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](group_r19.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](group_r19.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("(", group_r19.branches.length, ")");
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_span_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](2, 1, "GIT_BRANCH.CURRENT"));
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 55)(1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", branch_r24.upstreamBranch, " ");
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 57)(1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "arrow_upward");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", branch_r24.ahead, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 2, "GIT_BRANCH.AHEAD"), " ");
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 57)(1, "mat-icon", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "arrow_downward");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", branch_r24.behind, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 2, "GIT_BRANCH.BEHIND"), " ");
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](1, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_1_Template, 4, 1, "span", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](2, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_2_Template, 5, 4, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_span_3_Template, 5, 4, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", branch_r24.upstreamBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", branch_r24.ahead);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", branch_r24.behind);
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_button_8_Template(rf, ctx) {
  if (rf & 1) {
    const _r37 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "button", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_button_8_Template_button_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r37);
      const branch_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]().$implicit;
      const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
      ctx_r35.switchToBranch(branch_r24);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"]($event.stopPropagation());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "arrow_forward");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx_r27.isSwitching);
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_Template(rf, ctx) {
  if (rf & 1) {
    const _r39 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-list-item", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_Template_mat_list_item_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r39);
      const branch_r24 = restoredCtx.$implicit;
      const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r38.switchToBranch(branch_r24));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-icon", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "div", 46)(4, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_span_6_Template, 3, 3, "span", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_div_7_Template, 4, 3, "div", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_button_8_Template, 3, 1, "button", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const branch_r24 = ctx.$implicit;
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵclassProp"]("current", branch_r24.isCurrentBranch)("remote", branch_r24.isRemote)("local", !branch_r24.isRemote)("disabled", ctx_r21.isSwitching);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("color", branch_r24.isCurrentBranch ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", branch_r24.isCurrentBranch ? "check_circle" : branch_r24.isRemote ? "cloud" : "computer", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](branch_r24.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", branch_r24.isCurrentBranch);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", branch_r24.upstreamBranch || branch_r24.ahead || branch_r24.behind);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !branch_r24.isCurrentBranch);
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 59)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "search_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 1, "GIT_BRANCH.NO_BRANCHES_IN_CATEGORY"));
  }
}
function GitBranchDialogComponent_div_10_mat_tab_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](1, GitBranchDialogComponent_div_10_mat_tab_5_ng_template_1_Template, 6, 3, "ng-template", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-list", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, GitBranchDialogComponent_div_10_mat_tab_5_mat_list_item_3_Template, 9, 14, "mat-list-item", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](4, GitBranchDialogComponent_div_10_mat_tab_5_div_4_Template, 6, 3, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const group_r19 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", group_r19.branches);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", group_r19.branches.length === 0);
  }
}
function GitBranchDialogComponent_div_10_div_6_button_6_Template(rf, ctx) {
  if (rf & 1) {
    const _r42 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "button", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_div_10_div_6_button_6_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r42);
      const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
      ctx_r41.searchTerm = "";
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r41.filterBranches());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](2, 1, "GIT_BRANCH.CLEAR_SEARCH"), " ");
  }
}
function GitBranchDialogComponent_div_10_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 59)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "search_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitBranchDialogComponent_div_10_div_6_button_6_Template, 3, 3, "button", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 2, "GIT_BRANCH.NO_BRANCHES_FOUND"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r18.searchTerm);
  }
}
function GitBranchDialogComponent_div_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r44 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 33)(1, "div", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-tab-group", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("selectedIndexChange", function GitBranchDialogComponent_div_10_Template_mat_tab_group_selectedIndexChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r44);
      const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r43.selectedTabIndex = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, GitBranchDialogComponent_div_10_mat_tab_5_Template, 5, 2, "mat-tab", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitBranchDialogComponent_div_10_div_6_Template, 7, 4, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", ctx_r4.filteredBranches.length, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 5, "GIT_BRANCH.BRANCHES_FOUND"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("selectedIndex", ctx_r4.selectedTabIndex);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r4.branchGroups);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r4.branchGroups.length === 0);
  }
}
function GitBranchDialogComponent_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 1, "GIT_BRANCH.SWITCHING"));
  }
}
class GitBranchDialogComponent {
  constructor(dialogRef, data, gitService, snackBar, dialog, serverMessages, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitService = gitService;
    this.snackBar = snackBar;
    this.dialog = dialog;
    this.serverMessages = serverMessages;
    this.translate = translate;
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
      next: branch => {
        this.currentBranch = branch;
        // Update the BehaviorSubject
        this.gitService.currentBranch$.next(branch);
      },
      error: err => {
        this.error = this.translate.instant('GIT_BRANCH.LOAD_INFO_ERROR');
        console.error('Error loading branch info:', err);
      }
    });
  }
  loadBranches() {
    this.isLoading = true;
    this.error = null;
    this.gitService.getBranches(this.data.projectPath, true).subscribe({
      next: branches => {
        this.branches = branches;
        this.filteredBranches = this.sortBranches(branches);
        this.groupBranchesBySource(branches);
        this.isLoading = false;
      },
      error: err => {
        this.isLoading = false;
        this.error = this.translate.instant('GIT_BRANCH.LOAD_LIST_ERROR');
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
        name: this.translate.instant('GIT_BRANCH.LOCAL'),
        icon: 'computer',
        branches: groups.get('local')
      });
    }
    // Add remote groups sorted alphabetically
    const remoteKeys = Array.from(groups.keys()).filter(k => k !== 'local').sort();
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
      if (a.isCurrentBranch) return -1;
      if (b.isCurrentBranch) return 1;
      // Then local branches
      if (!a.isRemote && b.isRemote) return -1;
      if (a.isRemote && !b.isRemote) return 1;
      // Alphabetically
      return a.name.localeCompare(b.name);
    });
  }
  filterBranches() {
    const term = this.searchTerm.toLowerCase();
    if (!term) {
      this.filteredBranches = this.sortBranches(this.branches);
      this.groupBranchesBySource(this.branches);
    } else {
      const filtered = this.branches.filter(b => b.name.toLowerCase().includes(term));
      this.filteredBranches = this.sortBranches(filtered);
      this.groupBranchesBySource(filtered);
    }
  }
  switchToBranch(branch) {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      if (branch.isCurrentBranch) {
        _this.snackBar.open(_this.translate.instant('GIT_BRANCH.ALREADY_ON_BRANCH'), 'OK', {
          duration: 2000
        });
        return;
      }
      if (_this.isSwitching) {
        return; // Prevent multiple simultaneous switches
      }
      // Check for uncommitted changes
      _this.gitService.getRepositoryStatus(_this.data.projectPath).subscribe({
        next: status => {
          const hasChanges = status.hasChanges || _this.currentBranch?.somethingIsChangedInTheBranch;
          if (hasChanges) {
            _this.showUncommittedChangesDialog(branch);
          } else {
            _this.performCheckout(branch);
          }
        },
        error: err => {
          console.error('Error checking status:', err);
          // Proceed anyway
          _this.performCheckout(branch);
        }
      });
    })();
  }
  showUncommittedChangesDialog(branch) {
    const changesCount = this.currentBranch?.howManyFilesAreChanged || 0;
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
      } else {
        // Fallback: remove everything before the last slash
        const slashIndex = branch.name.indexOf('/');
        if (slashIndex !== -1) {
          branchName = branch.name.substring(slashIndex + 1);
        }
      }
    }
    this.gitService.checkoutBranch(this.data.projectPath, branchName, this.serverMessages.connectionId).subscribe({
      next: result => {
        this.isSwitching = false;
        if (result.success) {
          const actualBranchName = result.branchName || branchName;
          this.snackBar.open(this.translate.instant('GIT_BRANCH.SWITCHED_TO', {
            branch: actualBranchName
          }), 'OK', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
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
        } else {
          this.error = result.error || this.translate.instant('GIT_BRANCH.SWITCH_ERROR');
          this.snackBar.open(this.error, 'OK', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      },
      error: err => {
        this.isSwitching = false;
        this.error = this.translate.instant('GIT_BRANCH.NETWORK_ERROR');
        console.error('Checkout error:', err);
        this.snackBar.open(this.error, 'OK', {
          duration: 5000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }
  onClose() {
    this.dialogRef.close();
  }
  static {
    this.ɵfac = function GitBranchDialogComponent_Factory(t) {
      return new (t || GitBranchDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_2__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({
      type: GitBranchDialogComponent,
      selectors: [["app-git-branch-dialog"]],
      decls: 18,
      vars: 14,
      consts: [["mat-dialog-title", ""], [1, "dialog-icon"], [1, "branch-dialog-content"], ["class", "current-branch-card", 4, "ngIf"], ["class", "search-field", "appearance", "outline", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "error-card", 4, "ngIf"], ["class", "branch-list-container", 4, "ngIf"], ["class", "switching-overlay", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], [1, "current-branch-card"], [1, "current-branch-header"], ["color", "primary"], [1, "branch-details"], [1, "current-label"], [1, "current-branch-name"], ["class", "branch-stats", 4, "ngIf"], [1, "branch-stats"], ["color", "warn", "selected", "", 4, "ngIf"], ["color", "accent", "selected", "", 4, "ngIf"], ["color", "warn", "selected", ""], ["color", "accent", "selected", ""], ["appearance", "outline", 1, "search-field"], ["matInput", "", "autocomplete", "off", 3, "ngModel", "placeholder", "ngModelChange", "input"], ["matPrefix", ""], ["mat-icon-button", "", "matSuffix", "", "aria-label", "Clear", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matSuffix", "", "aria-label", "Clear", 3, "click"], [1, "loading-container"], ["diameter", "50"], [1, "error-card"], ["color", "warn"], ["mat-raised-button", "", "color", "primary", 3, "click"], [1, "branch-list-container"], [1, "branch-count"], [1, "branch-tabs", 3, "selectedIndex", "selectedIndexChange"], [4, "ngFor", "ngForOf"], ["class", "empty-state", 4, "ngIf"], ["mat-tab-label", ""], [1, "branch-list"], ["class", "branch-item", 3, "current", "remote", "local", "disabled", "click", 4, "ngFor", "ngForOf"], [1, "tab-icon"], [1, "tab-label"], [1, "tab-count"], [1, "branch-item", 3, "click"], ["matListIcon", "", 3, "color"], ["matLine", "", 1, "branch-name-line"], [1, "branch-name"], ["class", "branch-badge current-badge", 4, "ngIf"], ["matLine", "", "class", "branch-info-line", 4, "ngIf"], ["mat-icon-button", "", "class", "switch-button", 3, "disabled", "click", 4, "ngIf"], [1, "branch-badge", "current-badge"], ["matLine", "", 1, "branch-info-line"], ["class", "upstream-info", 4, "ngIf"], ["class", "sync-info", 4, "ngIf"], [1, "upstream-info"], [1, "small-icon"], [1, "sync-info"], ["mat-icon-button", "", 1, "switch-button", 3, "disabled", "click"], [1, "empty-state"], ["mat-button", "", 3, "click", 4, "ngIf"], ["mat-button", "", 3, "click"], [1, "switching-overlay"], ["diameter", "40"]],
      template: function GitBranchDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0)(1, "mat-icon", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "account_tree");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-dialog-content", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitBranchDialogComponent_mat_card_6_Template, 12, 5, "mat-card", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, GitBranchDialogComponent_mat_form_field_7_Template, 9, 8, "mat-form-field", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, GitBranchDialogComponent_div_8_Template, 5, 3, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, GitBranchDialogComponent_mat_card_9_Template, 11, 4, "mat-card", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitBranchDialogComponent_div_10_Template, 7, 7, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, GitBranchDialogComponent_div_11_Template, 5, 3, "div", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](12, "mat-dialog-actions", 9)(13, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitBranchDialogComponent_Template_button_click_13_listener() {
            return ctx.onClose();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](14, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](15, "close");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](16);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](17, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 10, "GIT_BRANCH.TITLE"), " ", ctx.data.projectName ? "- " + ctx.data.projectName : "", "\n");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.currentBranch && !ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.error && !ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.error);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isSwitching);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx.isSwitching);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](17, 12, "COMMON.CLOSE"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_7__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_7__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyPrefix, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_9__.MatLegacyInput, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__.MatLegacyCardContent, _angular_material_core__WEBPACK_IMPORTED_MODULE_11__.MatLine, _angular_material_legacy_list__WEBPACK_IMPORTED_MODULE_12__.MatLegacyList, _angular_material_legacy_list__WEBPACK_IMPORTED_MODULE_12__.MatLegacyListItem, _angular_material_legacy_list__WEBPACK_IMPORTED_MODULE_12__.MatLegacyListIconCssMatStyler, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_13__.MatLegacyTabGroup, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_13__.MatLegacyTabLabel, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_13__.MatLegacyTab, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_14__.MatLegacyButton, _angular_material_legacy_chips__WEBPACK_IMPORTED_MODULE_15__.MatLegacyChipList, _angular_material_legacy_chips__WEBPACK_IMPORTED_MODULE_15__.MatLegacyChip, _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_17__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_18__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_18__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_18__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslatePipe],
      styles: ["h2.mat-dialog-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin: 0;\n  padding: 16px 24px;\n  border-bottom: 1px solid #e0e0e0;\n}\nh2.mat-dialog-title[_ngcontent-%COMP%]   .dialog-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  height: 28px;\n  width: 28px;\n}\n\n.branch-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 !important;\n  min-width: 600px;\n  min-height: 400px;\n  max-height: 70vh;\n  display: flex;\n  flex-direction: column;\n  position: relative;\n}\n\n.current-branch-card[_ngcontent-%COMP%] {\n  margin: 16px 24px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n}\n.current-branch-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%] {\n  padding: 16px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  margin-bottom: 12px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 32px;\n  height: 32px;\n  width: 32px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%]   .current-label[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n  opacity: 0.9;\n}\n.current-branch-card[_ngcontent-%COMP%]   .current-branch-header[_ngcontent-%COMP%]   .branch-details[_ngcontent-%COMP%]   .current-branch-name[_ngcontent-%COMP%] {\n  font-size: 1.3em;\n  font-weight: 600;\n  font-family: \"Courier New\", monospace;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip-list[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip[_ngcontent-%COMP%] {\n  background-color: rgba(255, 255, 255, 0.2) !important;\n  color: white !important;\n}\n.current-branch-card[_ngcontent-%COMP%]   .branch-stats[_ngcontent-%COMP%]   mat-chip[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  margin-right: 4px;\n}\n\n.search-field[_ngcontent-%COMP%] {\n  margin: 16px 24px 0;\n  width: calc(100% - 48px);\n}\n.search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 40px;\n  gap: 16px;\n}\n.loading-container[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: #666;\n  font-size: 1.1em;\n}\n\n.error-card[_ngcontent-%COMP%] {\n  margin: 16px 24px;\n  border: 2px solid #f44336;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  gap: 16px;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  height: 48px;\n  width: 48px;\n}\n.error-card[_ngcontent-%COMP%]   .mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: #f44336;\n  font-size: 1.1em;\n  margin: 0;\n}\n\n.branch-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  margin: 0 24px 16px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-count[_ngcontent-%COMP%] {\n  padding: 8px 16px;\n  background-color: #f5f5f5;\n  border-radius: 4px 4px 0 0;\n  font-size: 0.9em;\n  color: #666;\n  font-weight: 500;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%] {\n  border: 1px solid #e0e0e0;\n  border-top: none;\n  border-radius: 0 0 4px 4px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-header {\n  background-color: #fafafa;\n  border-bottom: 1px solid #e0e0e0;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-label {\n  min-width: 120px;\n  padding: 0 16px;\n  opacity: 0.8;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-label.mat-tab-label-active {\n  opacity: 1;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]     .mat-tab-body-wrapper {\n  max-height: calc(70vh - 300px);\n  overflow-y: auto;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-tabs[_ngcontent-%COMP%]   .tab-count[_ngcontent-%COMP%] {\n  margin-left: 4px;\n  font-size: 0.85em;\n  color: #666;\n}\n.branch-list-container[_ngcontent-%COMP%]   .branch-list[_ngcontent-%COMP%] {\n  padding: 0;\n}\n\n.branch-item[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.2s ease;\n  border-bottom: 1px solid #f0f0f0;\n  padding: 12px 16px !important;\n  height: auto !important;\n  min-height: 60px;\n}\n.branch-item[_ngcontent-%COMP%]:hover:not(.disabled) {\n  background-color: #f5f5f5;\n}\n.branch-item.current[_ngcontent-%COMP%] {\n  background-color: #e3f2fd;\n  border-left: 4px solid #2196f3;\n}\n.branch-item.current[_ngcontent-%COMP%]:hover {\n  background-color: #bbdefb;\n}\n.branch-item.disabled[_ngcontent-%COMP%] {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.branch-item[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n.branch-item[_ngcontent-%COMP%]   .mat-list-item-content[_ngcontent-%COMP%] {\n  padding: 0 !important;\n  height: auto !important;\n  display: flex;\n  align-items: center;\n  gap: 12px;\n}\n.branch-item[_ngcontent-%COMP%]   mat-icon[matListIcon][_ngcontent-%COMP%] {\n  margin-right: 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 4px 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-name[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 1.05em;\n  font-weight: 500;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge[_ngcontent-%COMP%] {\n  font-size: 0.7em;\n  padding: 2px 6px;\n  border-radius: 3px;\n  font-weight: 600;\n  letter-spacing: 0.5px;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge.current-badge[_ngcontent-%COMP%] {\n  background-color: #2196f3;\n  color: white;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-name-line[_ngcontent-%COMP%]   .branch-badge.remote-badge[_ngcontent-%COMP%] {\n  background-color: #9e9e9e;\n  color: white;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 0.85em;\n  color: #666;\n  padding: 2px 0;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .upstream-info[_ngcontent-%COMP%], .branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .sync-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n}\n.branch-item[_ngcontent-%COMP%]   .branch-info-line[_ngcontent-%COMP%]   .small-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  height: 16px;\n  width: 16px;\n}\n.branch-item[_ngcontent-%COMP%]   .switch-button[_ngcontent-%COMP%] {\n  margin-left: auto;\n  color: #2196f3;\n}\n.branch-item[_ngcontent-%COMP%]   .switch-button[_ngcontent-%COMP%]:hover {\n  background-color: rgba(33, 150, 243, 0.1);\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 60px 40px;\n  text-align: center;\n  border: 1px solid #e0e0e0;\n  border-top: none;\n  border-radius: 0 0 4px 4px;\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  height: 64px;\n  width: 64px;\n  color: #bdbdbd;\n  margin-bottom: 16px;\n}\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1.1em;\n  color: #666;\n  margin-bottom: 16px;\n}\n\n.switching-overlay[_ngcontent-%COMP%] {\n  position: absolute;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background-color: rgba(255, 255, 255, 0.9);\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  z-index: 1000;\n}\n.switching-overlay[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1.1em;\n  color: #666;\n  font-weight: 500;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n  border-top: 1px solid #e0e0e0;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n@media (max-width: 768px) {\n  .branch-dialog-content[_ngcontent-%COMP%] {\n    min-width: 90vw;\n    max-height: 80vh;\n  }\n  .current-branch-card[_ngcontent-%COMP%], .search-field[_ngcontent-%COMP%], .branch-list-container[_ngcontent-%COMP%] {\n    margin-left: 16px;\n    margin-right: 16px;\n  }\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LWJyYW5jaC1kaWFsb2cvZ2l0LWJyYW5jaC1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7QUFBRjtBQUVFO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBQUo7O0FBS0E7RUFDRSxxQkFBQTtFQUNBLGdCQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGtCQUFBO0FBRkY7O0FBTUE7RUFDRSxpQkFBQTtFQUNBLDZEQUFBO0VBQ0EsWUFBQTtBQUhGO0FBS0U7RUFDRSxhQUFBO0FBSEo7QUFNRTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxtQkFBQTtBQUpKO0FBTUk7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7QUFKTjtBQU9JO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsUUFBQTtBQUxOO0FBT007RUFDRSxpQkFBQTtFQUNBLFlBQUE7QUFMUjtBQVFNO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLHFDQUFBO0FBTlI7QUFZSTtFQUNFLGFBQUE7RUFDQSxRQUFBO0FBVk47QUFhSTtFQUNFLHFEQUFBO0VBQ0EsdUJBQUE7QUFYTjtBQWFNO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsaUJBQUE7QUFYUjs7QUFrQkE7RUFDRSxtQkFBQTtFQUNBLHdCQUFBO0FBZkY7QUFpQkU7RUFDRSxpQkFBQTtBQWZKOztBQW9CQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxrQkFBQTtFQUNBLFNBQUE7QUFqQkY7QUFtQkU7RUFDRSxXQUFBO0VBQ0EsZ0JBQUE7QUFqQko7O0FBc0JBO0VBQ0UsaUJBQUE7RUFDQSx5QkFBQTtBQW5CRjtBQXFCRTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsU0FBQTtBQW5CSjtBQXFCSTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQW5CTjtBQXNCSTtFQUNFLGNBQUE7RUFDQSxnQkFBQTtFQUNBLFNBQUE7QUFwQk47O0FBMEJBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsbUJBQUE7QUF2QkY7QUF5QkU7RUFDRSxpQkFBQTtFQUNBLHlCQUFBO0VBQ0EsMEJBQUE7RUFDQSxnQkFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtBQXZCSjtBQTBCRTtFQUNFLHlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSwwQkFBQTtBQXhCSjtBQTJCTTtFQUNFLHlCQUFBO0VBQ0EsZ0NBQUE7QUF6QlI7QUE0Qk07RUFDRSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0FBMUJSO0FBNEJRO0VBQ0UsVUFBQTtBQTFCVjtBQThCTTtFQUNFLDhCQUFBO0VBQ0EsZ0JBQUE7QUE1QlI7QUFnQ0k7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQTlCTjtBQWlDSTtFQUNFLGdCQUFBO0FBL0JOO0FBa0NJO0VBQ0UsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLFdBQUE7QUFoQ047QUFvQ0U7RUFDRSxVQUFBO0FBbENKOztBQXVDQTtFQUNFLGVBQUE7RUFDQSx5QkFBQTtFQUNBLGdDQUFBO0VBQ0EsNkJBQUE7RUFDQSx1QkFBQTtFQUNBLGdCQUFBO0FBcENGO0FBc0NFO0VBQ0UseUJBQUE7QUFwQ0o7QUF1Q0U7RUFDRSx5QkFBQTtFQUNBLDhCQUFBO0FBckNKO0FBdUNJO0VBQ0UseUJBQUE7QUFyQ047QUF5Q0U7RUFDRSxZQUFBO0VBQ0EsbUJBQUE7QUF2Q0o7QUEwQ0U7RUFDRSxtQkFBQTtBQXhDSjtBQTJDRTtFQUNFLHFCQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0FBekNKO0FBNENFO0VBQ0UsZUFBQTtBQTFDSjtBQTZDRTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxjQUFBO0FBM0NKO0FBNkNJO0VBQ0UscUNBQUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0FBM0NOO0FBOENJO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxxQkFBQTtBQTVDTjtBQThDTTtFQUNFLHlCQUFBO0VBQ0EsWUFBQTtBQTVDUjtBQStDTTtFQUNFLHlCQUFBO0VBQ0EsWUFBQTtBQTdDUjtBQWtERTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxpQkFBQTtFQUNBLFdBQUE7RUFDQSxjQUFBO0FBaERKO0FBa0RJOztFQUVFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFoRE47QUFtREk7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7QUFqRE47QUFxREU7RUFDRSxpQkFBQTtFQUNBLGNBQUE7QUFuREo7QUFxREk7RUFDRSx5Q0FBQTtBQW5ETjs7QUF5REE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtFQUNBLHlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSwwQkFBQTtBQXRERjtBQXdERTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLGNBQUE7RUFDQSxtQkFBQTtBQXRESjtBQXlERTtFQUNFLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLG1CQUFBO0FBdkRKOztBQTREQTtFQUNFLGtCQUFBO0VBQ0EsTUFBQTtFQUNBLE9BQUE7RUFDQSxRQUFBO0VBQ0EsU0FBQTtFQUNBLDBDQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0FBekRGO0FBMkRFO0VBQ0UsZ0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUF6REo7O0FBOERBO0VBQ0Usa0JBQUE7RUFDQSw2QkFBQTtBQTNERjtBQTZERTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUEzREo7O0FBZ0VBO0VBQ0U7SUFDRSxlQUFBO0lBQ0EsZ0JBQUE7RUE3REY7RUFnRUE7OztJQUdFLGlCQUFBO0lBQ0Esa0JBQUE7RUE5REY7QUFDRiIsInNvdXJjZXNDb250ZW50IjpbIi8vIERpYWxvZyBUaXRsZVxyXG5oMi5tYXQtZGlhbG9nLXRpdGxlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIG1hcmdpbjogMDtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcblxyXG4gIC5kaWFsb2ctaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDI4cHg7XHJcbiAgICBoZWlnaHQ6IDI4cHg7XHJcbiAgICB3aWR0aDogMjhweDtcclxuICB9XHJcbn1cclxuXHJcbi8vIERpYWxvZyBDb250ZW50XHJcbi5icmFuY2gtZGlhbG9nLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDAgIWltcG9ydGFudDtcclxuICBtaW4td2lkdGg6IDYwMHB4O1xyXG4gIG1pbi1oZWlnaHQ6IDQwMHB4O1xyXG4gIG1heC1oZWlnaHQ6IDcwdmg7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIHBvc2l0aW9uOiByZWxhdGl2ZTtcclxufVxyXG5cclxuLy8gQ3VycmVudCBCcmFuY2ggU3VtbWFyeSBDYXJkXHJcbi5jdXJyZW50LWJyYW5jaC1jYXJkIHtcclxuICBtYXJnaW46IDE2cHggMjRweDtcclxuICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xyXG4gIGNvbG9yOiB3aGl0ZTtcclxuXHJcbiAgLm1hdC1jYXJkLWNvbnRlbnQge1xyXG4gICAgcGFkZGluZzogMTZweDtcclxuICB9XHJcblxyXG4gIC5jdXJyZW50LWJyYW5jaC1oZWFkZXIge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDEycHg7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxMnB4O1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAzMnB4O1xyXG4gICAgICBoZWlnaHQ6IDMycHg7XHJcbiAgICAgIHdpZHRoOiAzMnB4O1xyXG4gICAgfVxyXG5cclxuICAgIC5icmFuY2gtZGV0YWlscyB7XHJcbiAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgICAgIGdhcDogNHB4O1xyXG5cclxuICAgICAgLmN1cnJlbnQtbGFiZWwge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMC44NWVtO1xyXG4gICAgICAgIG9wYWNpdHk6IDAuOTtcclxuICAgICAgfVxyXG5cclxuICAgICAgLmN1cnJlbnQtYnJhbmNoLW5hbWUge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMS4zZW07XHJcbiAgICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcclxuICAgICAgICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuYnJhbmNoLXN0YXRzIHtcclxuICAgIG1hdC1jaGlwLWxpc3Qge1xyXG4gICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICBnYXA6IDhweDtcclxuICAgIH1cclxuXHJcbiAgICBtYXQtY2hpcCB7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC4yKSAhaW1wb3J0YW50O1xyXG4gICAgICBjb2xvcjogd2hpdGUgIWltcG9ydGFudDtcclxuXHJcbiAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTZWFyY2ggRmllbGRcclxuLnNlYXJjaC1maWVsZCB7XHJcbiAgbWFyZ2luOiAxNnB4IDI0cHggMDtcclxuICB3aWR0aDogY2FsYygxMDAlIC0gNDhweCk7XHJcblxyXG4gIC5tYXQtZm9ybS1maWVsZC13cmFwcGVyIHtcclxuICAgIHBhZGRpbmctYm90dG9tOiAwO1xyXG4gIH1cclxufVxyXG5cclxuLy8gTG9hZGluZyBDb250YWluZXJcclxuLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA2MHB4IDQwcHg7XHJcbiAgZ2FwOiAxNnB4O1xyXG5cclxuICBwIHtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC1zaXplOiAxLjFlbTtcclxuICB9XHJcbn1cclxuXHJcbi8vIEVycm9yIENhcmRcclxuLmVycm9yLWNhcmQge1xyXG4gIG1hcmdpbjogMTZweCAyNHB4O1xyXG4gIGJvcmRlcjogMnB4IHNvbGlkICNmNDQzMzY7XHJcblxyXG4gIC5tYXQtY2FyZC1jb250ZW50IHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gICAgcGFkZGluZzogNDBweDtcclxuICAgIGdhcDogMTZweDtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogNDhweDtcclxuICAgICAgaGVpZ2h0OiA0OHB4O1xyXG4gICAgICB3aWR0aDogNDhweDtcclxuICAgIH1cclxuXHJcbiAgICBwIHtcclxuICAgICAgY29sb3I6ICNmNDQzMzY7XHJcbiAgICAgIGZvbnQtc2l6ZTogMS4xZW07XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi8vIEJyYW5jaCBMaXN0IENvbnRhaW5lclxyXG4uYnJhbmNoLWxpc3QtY29udGFpbmVyIHtcclxuICBmbGV4OiAxO1xyXG4gIG92ZXJmbG93LXk6IGF1dG87XHJcbiAgbWFyZ2luOiAwIDI0cHggMTZweDtcclxuXHJcbiAgLmJyYW5jaC1jb3VudCB7XHJcbiAgICBwYWRkaW5nOiA4cHggMTZweDtcclxuICAgIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHggNHB4IDAgMDtcclxuICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG5cclxuICAuYnJhbmNoLXRhYnMge1xyXG4gICAgYm9yZGVyOiAxcHggc29saWQgI2UwZTBlMDtcclxuICAgIGJvcmRlci10b3A6IG5vbmU7XHJcbiAgICBib3JkZXItcmFkaXVzOiAwIDAgNHB4IDRweDtcclxuXHJcbiAgICA6Om5nLWRlZXAge1xyXG4gICAgICAubWF0LXRhYi1oZWFkZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6ICNmYWZhZmE7XHJcbiAgICAgICAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC5tYXQtdGFiLWxhYmVsIHtcclxuICAgICAgICBtaW4td2lkdGg6IDEyMHB4O1xyXG4gICAgICAgIHBhZGRpbmc6IDAgMTZweDtcclxuICAgICAgICBvcGFjaXR5OiAwLjg7XHJcblxyXG4gICAgICAgICYubWF0LXRhYi1sYWJlbC1hY3RpdmUge1xyXG4gICAgICAgICAgb3BhY2l0eTogMTtcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC5tYXQtdGFiLWJvZHktd3JhcHBlciB7XHJcbiAgICAgICAgbWF4LWhlaWdodDogY2FsYyg3MHZoIC0gMzAwcHgpO1xyXG4gICAgICAgIG92ZXJmbG93LXk6IGF1dG87XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAudGFiLWljb24ge1xyXG4gICAgICBtYXJnaW4tcmlnaHQ6IDhweDtcclxuICAgICAgZm9udC1zaXplOiAxOHB4O1xyXG4gICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgfVxyXG5cclxuICAgIC50YWItbGFiZWwge1xyXG4gICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgfVxyXG5cclxuICAgIC50YWItY291bnQge1xyXG4gICAgICBtYXJnaW4tbGVmdDogNHB4O1xyXG4gICAgICBmb250LXNpemU6IDAuODVlbTtcclxuICAgICAgY29sb3I6ICM2NjY7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuYnJhbmNoLWxpc3Qge1xyXG4gICAgcGFkZGluZzogMDtcclxuICB9XHJcbn1cclxuXHJcbi8vIEJyYW5jaCBMaXN0IEl0ZW1zXHJcbi5icmFuY2gtaXRlbSB7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjJzIGVhc2U7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNmMGYwZjA7XHJcbiAgcGFkZGluZzogMTJweCAxNnB4ICFpbXBvcnRhbnQ7XHJcbiAgaGVpZ2h0OiBhdXRvICFpbXBvcnRhbnQ7XHJcbiAgbWluLWhlaWdodDogNjBweDtcclxuXHJcbiAgJjpob3Zlcjpub3QoLmRpc2FibGVkKSB7XHJcbiAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIH1cclxuXHJcbiAgJi5jdXJyZW50IHtcclxuICAgIGJhY2tncm91bmQtY29sb3I6ICNlM2YyZmQ7XHJcbiAgICBib3JkZXItbGVmdDogNHB4IHNvbGlkICMyMTk2ZjM7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6ICNiYmRlZmI7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAmLmRpc2FibGVkIHtcclxuICAgIG9wYWNpdHk6IDAuNTtcclxuICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XHJcbiAgfVxyXG5cclxuICAmOmxhc3QtY2hpbGQge1xyXG4gICAgYm9yZGVyLWJvdHRvbTogbm9uZTtcclxuICB9XHJcblxyXG4gIC5tYXQtbGlzdC1pdGVtLWNvbnRlbnQge1xyXG4gICAgcGFkZGluZzogMCAhaW1wb3J0YW50O1xyXG4gICAgaGVpZ2h0OiBhdXRvICFpbXBvcnRhbnQ7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogMTJweDtcclxuICB9XHJcblxyXG4gIG1hdC1pY29uW21hdExpc3RJY29uXSB7XHJcbiAgICBtYXJnaW4tcmlnaHQ6IDA7XHJcbiAgfVxyXG5cclxuICAuYnJhbmNoLW5hbWUtbGluZSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogOHB4O1xyXG4gICAgcGFkZGluZzogNHB4IDA7XHJcblxyXG4gICAgLmJyYW5jaC1uYW1lIHtcclxuICAgICAgZm9udC1mYW1pbHk6ICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcclxuICAgICAgZm9udC1zaXplOiAxLjA1ZW07XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICB9XHJcblxyXG4gICAgLmJyYW5jaC1iYWRnZSB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMC43ZW07XHJcbiAgICAgIHBhZGRpbmc6IDJweCA2cHg7XHJcbiAgICAgIGJvcmRlci1yYWRpdXM6IDNweDtcclxuICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcclxuICAgICAgbGV0dGVyLXNwYWNpbmc6IDAuNXB4O1xyXG5cclxuICAgICAgJi5jdXJyZW50LWJhZGdlIHtcclxuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjMjE5NmYzO1xyXG4gICAgICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgICAgfVxyXG5cclxuICAgICAgJi5yZW1vdGUtYmFkZ2Uge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6ICM5ZTllOWU7XHJcbiAgICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuYnJhbmNoLWluZm8tbGluZSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogMTJweDtcclxuICAgIGZvbnQtc2l6ZTogMC44NWVtO1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgICBwYWRkaW5nOiAycHggMDtcclxuXHJcbiAgICAudXBzdHJlYW0taW5mbyxcclxuICAgIC5zeW5jLWluZm8ge1xyXG4gICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICBnYXA6IDRweDtcclxuICAgIH1cclxuXHJcbiAgICAuc21hbGwtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgaGVpZ2h0OiAxNnB4O1xyXG4gICAgICB3aWR0aDogMTZweDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5zd2l0Y2gtYnV0dG9uIHtcclxuICAgIG1hcmdpbi1sZWZ0OiBhdXRvO1xyXG4gICAgY29sb3I6ICMyMTk2ZjM7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMzMsIDE1MCwgMjQzLCAwLjEpO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLy8gRW1wdHkgU3RhdGVcclxuLmVtcHR5LXN0YXRlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA2MHB4IDQwcHg7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMGUwZTA7XHJcbiAgYm9yZGVyLXRvcDogbm9uZTtcclxuICBib3JkZXItcmFkaXVzOiAwIDAgNHB4IDRweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgZm9udC1zaXplOiA2NHB4O1xyXG4gICAgaGVpZ2h0OiA2NHB4O1xyXG4gICAgd2lkdGg6IDY0cHg7XHJcbiAgICBjb2xvcjogI2JkYmRiZDtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG5cclxuICBwIHtcclxuICAgIGZvbnQtc2l6ZTogMS4xZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTd2l0Y2hpbmcgT3ZlcmxheVxyXG4uc3dpdGNoaW5nLW92ZXJsYXkge1xyXG4gIHBvc2l0aW9uOiBhYnNvbHV0ZTtcclxuICB0b3A6IDA7XHJcbiAgbGVmdDogMDtcclxuICByaWdodDogMDtcclxuICBib3R0b206IDA7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNTUsIDI1NSwgMjU1LCAwLjkpO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIGdhcDogMTZweDtcclxuICB6LWluZGV4OiAxMDAwO1xyXG5cclxuICBwIHtcclxuICAgIGZvbnQtc2l6ZTogMS4xZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBEaWFsb2cgQWN0aW9uc1xyXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweDtcclxuICBib3JkZXItdG9wOiAxcHggc29saWQgI2UwZTBlMDtcclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiA4cHg7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBSZXNwb25zaXZlIGFkanVzdG1lbnRzXHJcbkBtZWRpYSAobWF4LXdpZHRoOiA3NjhweCkge1xyXG4gIC5icmFuY2gtZGlhbG9nLWNvbnRlbnQge1xyXG4gICAgbWluLXdpZHRoOiA5MHZ3O1xyXG4gICAgbWF4LWhlaWdodDogODB2aDtcclxuICB9XHJcblxyXG4gIC5jdXJyZW50LWJyYW5jaC1jYXJkLFxyXG4gIC5zZWFyY2gtZmllbGQsXHJcbiAgLmJyYW5jaC1saXN0LWNvbnRhaW5lciB7XHJcbiAgICBtYXJnaW4tbGVmdDogMTZweDtcclxuICAgIG1hcmdpbi1yaWdodDogMTZweDtcclxuICB9XHJcbn1cclxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
    });
  }
}

/***/ }),

/***/ 9497:
/*!********************************************************************************!*\
  !*** ./src/app/git/dialogs/git-history-dialog/git-history-dialog.component.ts ***!
  \********************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitHistoryDialogComponent": () => (/* binding */ GitHistoryDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-table */ 6538);
/* harmony import */ var _angular_material_sort__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/sort */ 2197);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../services/gitservice.service */ 7224);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);













const _c0 = ["graphCanvas"];
function GitHistoryDialogComponent_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 1, "GIT_HISTORY.LOADING"));
  }
}
function GitHistoryDialogComponent_div_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 7)(1, "mat-icon", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r1.error);
  }
}
function GitHistoryDialogComponent_div_6_div_12_th_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.HASH"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_td_4_mat_icon_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "merge_type");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](1, 1, "GIT_HISTORY.MERGE_COMMIT"));
  }
}
function GitHistoryDialogComponent_div_6_div_12_td_4_Template(rf, ctx) {
  if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 30)(1, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_6_div_12_td_4_Template_span_click_1_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r19);
      const commit_r16 = restoredCtx.$implicit;
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r18.copyHash(commit_r16.hash));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, GitHistoryDialogComponent_div_6_div_12_td_4_mat_icon_4_Template, 3, 3, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const commit_r16 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 3, "GIT_HISTORY.CLICK_TO_COPY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", commit_r16.shortHash || commit_r16.hash.substring(0, 7), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", commit_r16.isMerge);
  }
}
function GitHistoryDialogComponent_div_6_div_12_th_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.DATE"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_td_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const commit_r20 = ctx.$implicit;
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r8.formatDate(commit_r20.date), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_th_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.AUTHOR"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_td_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const commit_r21 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", commit_r21.author, " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_th_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "th", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.MESSAGE"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_td_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "td", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const commit_r22 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", commit_r22.message, " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_tr_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "tr", 38);
  }
}
function GitHistoryDialogComponent_div_6_div_12_tr_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](0, "tr", 39);
  }
}
function GitHistoryDialogComponent_div_6_div_12_div_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.NO_COMMITS"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 14)(1, "table", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](2, 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_6_div_12_th_3_Template, 3, 3, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, GitHistoryDialogComponent_div_6_div_12_td_4_Template, 5, 5, "td", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](5, 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, GitHistoryDialogComponent_div_6_div_12_th_6_Template, 3, 3, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](7, GitHistoryDialogComponent_div_6_div_12_td_7_Template, 2, 1, "td", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](8, 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](9, GitHistoryDialogComponent_div_6_div_12_th_9_Template, 3, 3, "th", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](10, GitHistoryDialogComponent_div_6_div_12_td_10_Template, 2, 1, "td", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerStart"](11, 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, GitHistoryDialogComponent_div_6_div_12_th_12_Template, 3, 3, "th", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitHistoryDialogComponent_div_6_div_12_td_13_Template, 2, 1, "td", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementContainerEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, GitHistoryDialogComponent_div_6_div_12_tr_14_Template, 1, 0, "tr", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](15, GitHistoryDialogComponent_div_6_div_12_tr_15_Template, 1, 0, "tr", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, GitHistoryDialogComponent_div_6_div_12_div_16_Template, 3, 3, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("dataSource", ctx_r3.dataSource);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("matHeaderRowDef", ctx_r3.displayedColumns);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("matRowDefColumns", ctx_r3.displayedColumns);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r3.commits.length === 0);
  }
}
function GitHistoryDialogComponent_div_6_div_13_div_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "GIT_HISTORY.NO_COMMITS"), " ");
  }
}
function GitHistoryDialogComponent_div_6_div_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "canvas", 42, 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, GitHistoryDialogComponent_div_6_div_13_div_3_Template, 3, 3, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r4.commits.length === 0);
  }
}
function GitHistoryDialogComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 9)(1, "div", 10)(2, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_6_Template_button_click_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r27);
      const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r26.switchView("table"));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "table_chart");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitHistoryDialogComponent_div_6_Template_button_click_7_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r27);
      const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r28.switchView("graph"));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "account_tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, GitHistoryDialogComponent_div_6_div_12_Template, 17, 4, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, GitHistoryDialogComponent_div_6_div_13_Template, 4, 1, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("color", ctx_r2.selectedView === "table" ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](6, 6, "GIT_HISTORY.TABLE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("color", ctx_r2.selectedView === "graph" ? "primary" : "");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](11, 8, "GIT_HISTORY.GRAPH"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.selectedView === "table");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.selectedView === "graph");
  }
}
class GitHistoryDialogComponent {
  constructor(dialogRef, data, gitService, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitService = gitService;
    this.translate = translate;
    this.isLoading = true;
    this.error = null;
    this.commits = [];
    this.displayedColumns = ['hash', 'date', 'author', 'message'];
    this.selectedView = 'table';
    this.dataSource = new _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyTableDataSource([]);
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
      next: commits => {
        this.isLoading = false;
        this.commits = commits;
        this.dataSource.data = commits;
        // If graph view is selected and we have commits, render the graph
        if (this.selectedView === 'graph' && commits.length > 0) {
          setTimeout(() => this.renderGraph(), 100);
        }
      },
      error: err => {
        this.isLoading = false;
        this.error = this.translate.instant('GIT_HISTORY.LOAD_ERROR');
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
    if (!ctx) return;
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
  static {
    this.ɵfac = function GitHistoryDialogComponent_Factory(t) {
      return new (t || GitHistoryDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_0__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({
      type: GitHistoryDialogComponent,
      selectors: [["app-git-history-dialog"]],
      viewQuery: function GitHistoryDialogComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵviewQuery"](_c0, 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵviewQuery"](_angular_material_sort__WEBPACK_IMPORTED_MODULE_5__.MatSort, 5);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵloadQuery"]()) && (ctx.graphCanvas = _t.first);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵloadQuery"]()) && (ctx.sort = _t.first);
        }
      },
      decls: 11,
      vars: 10,
      consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], ["class", "error-container", 4, "ngIf"], ["class", "history-content", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "loading-container"], [1, "error-container"], ["color", "warn"], [1, "history-content"], [1, "view-toggle"], ["mat-button", "", 3, "color", "click"], ["class", "table-container", 4, "ngIf"], ["class", "graph-container", 4, "ngIf"], [1, "table-container"], ["mat-table", "", "matSort", "", 1, "commit-table", 3, "dataSource"], ["matColumnDef", "hash"], ["mat-header-cell", "", "mat-sort-header", "", 4, "matHeaderCellDef"], ["mat-cell", "", "class", "hash-cell", 4, "matCellDef"], ["matColumnDef", "date"], ["mat-cell", "", "class", "date-cell", 4, "matCellDef"], ["matColumnDef", "author"], ["mat-cell", "", "class", "author-cell", 4, "matCellDef"], ["matColumnDef", "message"], ["mat-header-cell", "", 4, "matHeaderCellDef"], ["mat-cell", "", "class", "message-cell", 4, "matCellDef"], ["mat-header-row", "", 4, "matHeaderRowDef"], ["mat-row", "", 4, "matRowDef", "matRowDefColumns"], ["class", "no-commits", 4, "ngIf"], ["mat-header-cell", "", "mat-sort-header", ""], ["mat-cell", "", 1, "hash-cell"], [1, "hash-text", 3, "title", "click"], ["class", "merge-icon", 3, "title", 4, "ngIf"], [1, "merge-icon", 3, "title"], ["mat-cell", "", 1, "date-cell"], ["mat-cell", "", 1, "author-cell"], ["mat-header-cell", ""], ["mat-cell", "", 1, "message-cell"], ["mat-header-row", ""], ["mat-row", ""], [1, "no-commits"], [1, "graph-container"], [1, "graph-canvas"], ["graphCanvas", ""]],
      template: function GitHistoryDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-dialog-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, GitHistoryDialogComponent_div_4_Template, 5, 3, "div", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, GitHistoryDialogComponent_div_5_Template, 5, 1, "div", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, GitHistoryDialogComponent_div_6_Template, 14, 10, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "mat-dialog-actions", 4)(8, "button", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function GitHistoryDialogComponent_Template_button_click_8_listener() {
            return ctx.onClose();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](10, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 6, "GIT_HISTORY.TITLE"), " ", ctx.data.projectName ? "- " + ctx.data.projectName : "", "");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.error && !ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && !ctx.error);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](10, 8, "COMMON.CLOSE"));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.NgIf, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogActions, _angular_material_sort__WEBPACK_IMPORTED_MODULE_5__.MatSort, _angular_material_sort__WEBPACK_IMPORTED_MODULE_5__.MatSortHeader, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyTable, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyHeaderCellDef, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyHeaderRowDef, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyColumnDef, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyCellDef, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyRowDef, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyHeaderCell, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyCell, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyHeaderRow, _angular_material_legacy_table__WEBPACK_IMPORTED_MODULE_2__.MatLegacyRow, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_4__.TranslatePipe],
      styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n}\n\n.error-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  color: #f44336;\n}\n.error-container[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  height: 48px;\n  width: 48px;\n  margin-bottom: 16px;\n}\n\n.history-content[_ngcontent-%COMP%] {\n  min-width: 800px;\n  max-height: 600px;\n  overflow: auto;\n}\n\n.view-toggle[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  margin-bottom: 16px;\n  padding: 8px;\n  border-bottom: 1px solid #e0e0e0;\n}\n\n.table-container[_ngcontent-%COMP%] {\n  overflow: auto;\n  max-height: 500px;\n}\n\n.commit-table[_ngcontent-%COMP%] {\n  width: 100%;\n}\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%] {\n  font-family: monospace;\n}\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .hash-text[_ngcontent-%COMP%] {\n  cursor: pointer;\n  padding: 2px 4px;\n  border-radius: 3px;\n}\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .hash-text[_ngcontent-%COMP%]:hover {\n  background-color: #f0f0f0;\n}\n.commit-table[_ngcontent-%COMP%]   .hash-cell[_ngcontent-%COMP%]   .merge-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-left: 8px;\n  color: #ff6b6b;\n  vertical-align: middle;\n}\n.commit-table[_ngcontent-%COMP%]   .date-cell[_ngcontent-%COMP%] {\n  white-space: nowrap;\n  font-size: 0.9em;\n}\n.commit-table[_ngcontent-%COMP%]   .author-cell[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n.commit-table[_ngcontent-%COMP%]   .message-cell[_ngcontent-%COMP%] {\n  max-width: 400px;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n.commit-table[_ngcontent-%COMP%]   tr[_ngcontent-%COMP%]:hover {\n  background-color: #f5f5f5;\n}\n\n.graph-container[_ngcontent-%COMP%] {\n  overflow: auto;\n  max-height: 500px;\n  padding: 16px;\n  background-color: #fafafa;\n  border-radius: 4px;\n}\n\n.graph-canvas[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 400px;\n  background-color: white;\n  border: 1px solid #e0e0e0;\n  border-radius: 4px;\n}\n\n.no-commits[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 40px;\n  color: #666;\n  font-style: italic;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 24px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LWhpc3RvcnktZGlhbG9nL2dpdC1oaXN0b3J5LWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLGNBQUE7QUFDRjtBQUNFO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsbUJBQUE7QUFDSjs7QUFHQTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBQUY7O0FBR0E7RUFDRSxhQUFBO0VBQ0EsUUFBQTtFQUNBLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLGdDQUFBO0FBQUY7O0FBR0E7RUFDRSxjQUFBO0VBQ0EsaUJBQUE7QUFBRjs7QUFHQTtFQUNFLFdBQUE7QUFBRjtBQUVFO0VBQ0Usc0JBQUE7QUFBSjtBQUVJO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7QUFBTjtBQUVNO0VBQ0UseUJBQUE7QUFBUjtBQUlJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0Esc0JBQUE7QUFGTjtBQU1FO0VBQ0UsbUJBQUE7RUFDQSxnQkFBQTtBQUpKO0FBT0U7RUFDRSxnQkFBQTtBQUxKO0FBUUU7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxtQkFBQTtBQU5KO0FBU0U7RUFDRSx5QkFBQTtBQVBKOztBQVdBO0VBQ0UsY0FBQTtFQUNBLGlCQUFBO0VBQ0EsYUFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7QUFSRjs7QUFXQTtFQUNFLFdBQUE7RUFDQSxpQkFBQTtFQUNBLHVCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtBQVJGOztBQVdBO0VBQ0Usa0JBQUE7RUFDQSxhQUFBO0VBQ0EsV0FBQTtFQUNBLGtCQUFBO0FBUkY7O0FBV0E7RUFDRSxlQUFBO0FBUkY7O0FBV0E7RUFDRSxrQkFBQTtBQVJGIiwic291cmNlc0NvbnRlbnQiOlsiLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4O1xyXG59XHJcblxyXG4uZXJyb3ItY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICBwYWRkaW5nOiA0MHB4O1xyXG4gIGNvbG9yOiAjZjQ0MzM2O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmb250LXNpemU6IDQ4cHg7XHJcbiAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICB3aWR0aDogNDhweDtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG59XHJcblxyXG4uaGlzdG9yeS1jb250ZW50IHtcclxuICBtaW4td2lkdGg6IDgwMHB4O1xyXG4gIG1heC1oZWlnaHQ6IDYwMHB4O1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG59XHJcblxyXG4udmlldy10b2dnbGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICBwYWRkaW5nOiA4cHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XHJcbn1cclxuXHJcbi50YWJsZS1jb250YWluZXIge1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG4gIG1heC1oZWlnaHQ6IDUwMHB4O1xyXG59XHJcblxyXG4uY29tbWl0LXRhYmxlIHtcclxuICB3aWR0aDogMTAwJTtcclxuXHJcbiAgLmhhc2gtY2VsbCB7XHJcbiAgICBmb250LWZhbWlseTogbW9ub3NwYWNlO1xyXG5cclxuICAgIC5oYXNoLXRleHQge1xyXG4gICAgICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgICAgIHBhZGRpbmc6IDJweCA0cHg7XHJcbiAgICAgIGJvcmRlci1yYWRpdXM6IDNweDtcclxuXHJcbiAgICAgICY6aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6ICNmMGYwZjA7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAubWVyZ2UtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgd2lkdGg6IDE2cHg7XHJcbiAgICAgIGhlaWdodDogMTZweDtcclxuICAgICAgbWFyZ2luLWxlZnQ6IDhweDtcclxuICAgICAgY29sb3I6ICNmZjZiNmI7XHJcbiAgICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAuZGF0ZS1jZWxsIHtcclxuICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbiAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gIH1cclxuXHJcbiAgLmF1dGhvci1jZWxsIHtcclxuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgfVxyXG5cclxuICAubWVzc2FnZS1jZWxsIHtcclxuICAgIG1heC13aWR0aDogNDAwcHg7XHJcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gIH1cclxuXHJcbiAgdHI6aG92ZXIge1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2Y1ZjVmNTtcclxuICB9XHJcbn1cclxuXHJcbi5ncmFwaC1jb250YWluZXIge1xyXG4gIG92ZXJmbG93OiBhdXRvO1xyXG4gIG1heC1oZWlnaHQ6IDUwMHB4O1xyXG4gIHBhZGRpbmc6IDE2cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZhZmFmYTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbn1cclxuXHJcbi5ncmFwaC1jYW52YXMge1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIG1pbi1oZWlnaHQ6IDQwMHB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6IHdoaXRlO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNlMGUwZTA7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG59XHJcblxyXG4ubm8tY29tbWl0cyB7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDQwcHg7XHJcbiAgY29sb3I6ICM2NjY7XHJcbiAgZm9udC1zdHlsZTogaXRhbGljO1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDAgMjRweDtcclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHg7XHJcbn0iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

/***/ }),

/***/ 5855:
/*!*********************************************************************************!*\
  !*** ./src/app/git/dialogs/git-init-wizard/git-init-wizard-dialog.component.ts ***!
  \*********************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitInitWizardDialogComponent": () => (/* binding */ GitInitWizardDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _models_git_init_models__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../models/git-init.models */ 3601);
/* harmony import */ var _git_setup_remote_generic_dialog_git_setup_remote_generic_dialog_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component */ 8995);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/gitservice.service */ 7224);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/forms */ 2508);

















function GitInitWizardDialogComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-spinner", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "p", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 1, "GIT_INIT.INITIALIZING"));
  }
}
function GitInitWizardDialogComponent_div_7_mat_option_27_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-option", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const template_r6 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", template_r6.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", template_r6.label, " ");
  }
}
function GitInitWizardDialogComponent_div_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div")(1, "p", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "mat-form-field", 10)(5, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "input", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_7_Template_input_ngModelChange_8_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8);
      const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r7.repositoryPath = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "mat-form-field", 10)(12, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](15, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_7_Template_input_ngModelChange_15_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8);
      const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r9.initialBranch = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](17, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](18, "account_tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](19, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](21, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](22, "mat-form-field", 10)(23, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](25, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "mat-select", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitInitWizardDialogComponent_div_7_Template_mat_select_ngModelChange_26_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r8);
      const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r10.gitignoreTemplate = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](27, GitInitWizardDialogComponent_div_7_mat_option_27_Template, 2, 2, "mat-option", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](28, "mat-icon", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](29, "description");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](30, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](32, "div", 16)(33, "div", 17)(34, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](35, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](36, "strong", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](38, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](39, "ul", 20)(40, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](41);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](42, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](43, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](44);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](45, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](46, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](47);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](48, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](49, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](50);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()()();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 17, "GIT_INIT.DESCRIPTION"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](7, 19, "GIT_INIT.REPO_PATH"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.repositoryPath);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](14, 21, "GIT_INIT.BRANCH_NAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.initialBranch)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](16, 23, "GIT_INIT.BRANCH_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](21, 25, "GIT_INIT.BRANCH_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](25, 27, "GIT_INIT.GITIGNORE_TEMPLATE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r1.gitignoreTemplate);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r1.gitignoreTemplates);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r1.getSelectedTemplateDescription());
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](38, 29, "GIT_INIT.WILL_BE_CREATED"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](42, 31, "GIT_INIT.GIT_FOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](45, 33, "GIT_INIT.GITIGNORE_FILE"), " (", ctx_r1.gitignoreTemplate, " template)");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"]("", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](48, 35, "GIT_INIT.INITIAL_BRANCH"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r1.initialBranch);
  }
}
function GitInitWizardDialogComponent_div_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div")(1, "div", 22)(2, "mat-icon", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "h3", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "p", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "div", 26)(10, "div", 17)(11, "mat-icon", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](12, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](16, "p", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](6, 4, "GIT_INIT.SUCCESS_TITLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r2.initMessage);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](15, 6, "GIT_INIT.NEXT_STEP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](18, 8, "GIT_INIT.NEXT_STEP_DESC"), " ");
  }
}
function GitInitWizardDialogComponent_div_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 29)(1, "button", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_10_Template_button_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r12);
      const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r11.cancel());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "button", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_10_Template_button_click_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r12);
      const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r13.initializeGit());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6, "add_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx_r3.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 4, "COMMON.CANCEL"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx_r3.isLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](8, 6, "GIT_INIT.INITIALIZE_BTN"), " ");
  }
}
function GitInitWizardDialogComponent_div_11_Template(rf, ctx) {
  if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 29)(1, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_11_Template_button_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r15);
      const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r14.skipRemoteSetup());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "skip_next");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "button", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitInitWizardDialogComponent_div_11_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r15);
      const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r16.setupRemote());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "cloud_upload");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 2, "GIT_INIT.SKIP_FOR_NOW"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](10, 4, "GIT_INIT.SETUP_REMOTE"), " ");
  }
}
class GitInitWizardDialogComponent {
  constructor(dialogRef, data, gitService, snackBar, dialog, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitService = gitService;
    this.snackBar = snackBar;
    this.dialog = dialog;
    this.translate = translate;
    // Wizard state
    this.currentStep = 0;
    this.isLoading = false;
    this.initialBranch = 'main';
    this.gitignoreTemplate = 'mdexplorer';
    this.gitignoreTemplates = _models_git_init_models__WEBPACK_IMPORTED_MODULE_0__.GITIGNORE_TEMPLATES;
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
      this.snackBar.open(this.translate.instant('GIT_INIT.PATH_REQUIRED'), 'OK', {
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
    this.gitService.initRepository(request).subscribe(response => {
      this.isLoading = false;
      if (response.success) {
        this.initSuccess = true;
        this.initMessage = response.message;
        console.log('[GitInitWizard] ✅ Git repository initialized successfully');
        this.snackBar.open(this.translate.instant('GIT_INIT.INIT_SUCCESS'), 'OK', {
          duration: 3000,
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        // Move to step 2 (Remote Setup)
        this.currentStep = 1;
      } else {
        console.error('[GitInitWizard] ❌ Initialization failed:', response.message);
        this.snackBar.open(this.translate.instant('GIT_INIT.INIT_FAILED', {
          error: response.message
        }), 'OK', {
          duration: 5000,
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    }, error => {
      this.isLoading = false;
      console.error('[GitInitWizard] ❌ Error initializing repository:', error);
      const errorMessage = error?.error?.message || error?.message || 'Unknown error';
      this.snackBar.open(this.translate.instant('COMMON.ERROR') + ': ' + errorMessage, 'OK', {
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
    const projectName = this.repositoryPath.split(/[/\\]/).pop() || 'repository';
    const remoteDialogRef = this.dialog.open(_git_setup_remote_generic_dialog_git_setup_remote_generic_dialog_component__WEBPACK_IMPORTED_MODULE_1__.GitSetupRemoteGenericDialogComponent, {
      width: '650px',
      data: {
        projectPath: this.repositoryPath,
        projectName: projectName
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
  static {
    this.ɵfac = function GitInitWizardDialogComponent_Factory(t) {
      return new (t || GitInitWizardDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_2__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({
      type: GitInitWizardDialogComponent,
      selectors: [["app-git-init-wizard-dialog"]],
      decls: 12,
      vars: 8,
      consts: [["mat-dialog-title", ""], [2, "vertical-align", "middle"], ["style", "text-align: center; padding: 40px;", 4, "ngIf"], [4, "ngIf"], ["align", "end"], ["style", "display: flex; gap: 8px; width: 100%; justify-content: flex-end;", 4, "ngIf"], [2, "text-align", "center", "padding", "40px"], ["diameter", "50", 2, "margin", "0 auto"], [2, "margin-top", "20px", "color", "#666"], [2, "margin-bottom", "20px", "color", "#666"], ["appearance", "outline", 2, "width", "100%", "margin-bottom", "16px"], ["matInput", "", "readonly", "", 3, "ngModel", "ngModelChange"], ["matSuffix", ""], ["matInput", "", 3, "ngModel", "placeholder", "ngModelChange"], [3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], [2, "background", "#f5f5f5", "padding", "12px", "border-radius", "4px", "margin-bottom", "16px"], [2, "display", "flex", "align-items", "center", "gap", "8px", "margin-bottom", "8px"], [2, "color", "#666", "font-size", "20px"], [2, "color", "#333"], [2, "margin", "0", "padding-left", "24px", "color", "#666"], [3, "value"], [2, "text-align", "center", "padding", "20px"], [2, "color", "#4caf50", "font-size", "64px", "width", "64px", "height", "64px"], [2, "color", "#4caf50", "margin", "16px 0 8px 0"], [2, "color", "#666", "margin-bottom", "24px"], [2, "background", "#e3f2fd", "padding", "16px", "border-radius", "4px", "border-left", "4px solid #2196f3", "text-align", "left"], [2, "color", "#2196f3"], [2, "margin", "0", "color", "#666"], [2, "display", "flex", "gap", "8px", "width", "100%", "justify-content", "flex-end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"]],
      template: function GitInitWizardDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0)(1, "mat-icon", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "settings");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-dialog-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitInitWizardDialogComponent_div_6_Template, 5, 3, "div", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](7, GitInitWizardDialogComponent_div_7_Template, 51, 37, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, GitInitWizardDialogComponent_div_8_Template, 19, 10, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-dialog-actions", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitInitWizardDialogComponent_div_10_Template, 9, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, GitInitWizardDialogComponent_div_11_Template, 11, 6, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 6, "GIT_INIT.TITLE"), "\n");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && ctx.currentStep === 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isLoading && ctx.currentStep === 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.currentStep === 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.currentStep === 1);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_7__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_7__.NgIf, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_8__.MatLegacyOption, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_9__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_9__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_9__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_9__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_10__.MatLegacyInput, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_11__.MatLegacySelect, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_14__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslatePipe],
      styles: ["mat-dialog-content[_ngcontent-%COMP%] {\n  min-height: 300px;\n  max-height: 70vh;\n  padding: 24px;\n}\n\nmat-form-field[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.success-snackbar[_ngcontent-%COMP%] {\n  background-color: #4caf50 !important;\n  color: white !important;\n}\n\n.error-snackbar[_ngcontent-%COMP%] {\n  background-color: #f44336 !important;\n  color: white !important;\n}\n\nmat-icon[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LWluaXQtd2l6YXJkL2dpdC1pbml0LXdpemFyZC1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtBQUNGOztBQUVBO0VBQ0UsV0FBQTtBQUNGOztBQUVBO0VBQ0Usb0NBQUE7RUFDQSx1QkFBQTtBQUNGOztBQUVBO0VBQ0Usb0NBQUE7RUFDQSx1QkFBQTtBQUNGOztBQUVBO0VBQ0Usc0JBQUE7QUFDRiIsInNvdXJjZXNDb250ZW50IjpbIm1hdC1kaWFsb2ctY29udGVudCB7XHJcbiAgbWluLWhlaWdodDogMzAwcHg7XHJcbiAgbWF4LWhlaWdodDogNzB2aDtcclxuICBwYWRkaW5nOiAyNHB4O1xyXG59XHJcblxyXG5tYXQtZm9ybS1maWVsZCB7XHJcbiAgd2lkdGg6IDEwMCU7XHJcbn1cclxuXHJcbi5zdWNjZXNzLXNuYWNrYmFyIHtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjNGNhZjUwICFpbXBvcnRhbnQ7XHJcbiAgY29sb3I6IHdoaXRlICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcbi5lcnJvci1zbmFja2JhciB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2Y0NDMzNiAhaW1wb3J0YW50O1xyXG4gIGNvbG9yOiB3aGl0ZSAhaW1wb3J0YW50O1xyXG59XHJcblxyXG5tYXQtaWNvbiB7XHJcbiAgdmVydGljYWwtYWxpZ246IG1pZGRsZTtcclxufVxyXG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

/***/ }),

/***/ 3992:
/*!******************************************************************************************!*\
  !*** ./src/app/git/dialogs/git-setup-remote-dialog/git-setup-remote-dialog.component.ts ***!
  \******************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitSetupRemoteDialogComponent": () => (/* binding */ GitSetupRemoteDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../git-token-dialog/git-token-dialog.component */ 3701);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/gitservice.service */ 7224);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/expansion */ 7591);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/forms */ 2508);

















function GitSetupRemoteDialogComponent_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](1, "mat-spinner", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 1, "GIT_REMOTE.LOADING_CONFIG"));
  }
}
function GitSetupRemoteDialogComponent_div_5_div_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 26)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p")(4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](7, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "a", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_div_5_div_7_Template_a_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r7);
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r6.openTokenSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](6, 3, "GIT_REMOTE.NO_TOKEN_WARNING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](9, 5, "GIT_REMOTE.NO_TOKEN_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](12, 7, "GIT_REMOTE.CONFIGURE_TOKEN"));
  }
}
function GitSetupRemoteDialogComponent_div_5_div_53_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 28)(1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](3, 2, "GIT_REMOTE.REPO_URL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r4.getGitHubUrl());
  }
}
function GitSetupRemoteDialogComponent_div_5_div_61_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 29)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r5.error);
  }
}
const _c0 = function (a0) {
  return {
    projectName: a0
  };
};
const _c1 = function (a0) {
  return {
    repoName: a0
  };
};
function GitSetupRemoteDialogComponent_div_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 9)(1, "div", 10)(2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](7, GitSetupRemoteDialogComponent_div_5_div_7_Template, 13, 9, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "mat-form-field", 12)(9, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_input_ngModelChange_12_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r8.organization = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](14, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](15, "business");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](16, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](19, "mat-form-field", 12)(20, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](22, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](23, "input", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_input_ngModelChange_23_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r10.repositoryName = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](25, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](26, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](28, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](29, "mat-form-field", 12)(30, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](32, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](33, "textarea", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_textarea_ngModelChange_33_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r11.repositoryDescription = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](34, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](35, "      ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](36, "mat-icon", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](37, "description");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](38, "div", 16)(39, "mat-label", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](40);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](41, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](42, "mat-radio-group", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_mat_radio_group_ngModelChange_42_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r12.isPrivate = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](43, "mat-radio-button", 19)(44, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](45, "lock");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](46);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](47, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](48, "mat-radio-button", 21)(49, "mat-icon", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](50, "public");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](51);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](52, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](53, GitSetupRemoteDialogComponent_div_5_div_53_Template, 6, 4, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](54, "div", 23)(55, "mat-checkbox", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_mat_checkbox_ngModelChange_55_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r13.saveOrganization = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](57, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](58, "mat-checkbox", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteDialogComponent_div_5_Template_mat_checkbox_ngModelChange_58_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r9);
      const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r14.pushAfterAdd = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](59);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](60, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](61, GitSetupRemoteDialogComponent_div_5_div_61_Template, 5, 1, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](62, "mat-expansion-panel", 25)(63, "mat-expansion-panel-header")(64, "mat-panel-title")(65, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](66, "help_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](67);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](68, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](69, "ol")(70, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](71);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](72, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](73, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](74);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](75, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](76, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](77);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](78, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](79, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](80);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](81, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](82, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](83);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](84, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](85, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](86);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](87, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](6, 38, "GIT_REMOTE.GITHUB_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r1.hasToken);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](11, 40, "GIT_REMOTE.GITHUB_ORG"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.organization)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](13, 42, "GIT_REMOTE.GITHUB_ORG_PLACEHOLDER"))("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](18, 44, "GIT_REMOTE.GITHUB_ORG_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](22, 46, "GIT_REMOTE.REPO_NAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.repositoryName)("placeholder", ctx_r1.translate.instant("GIT_REMOTE.REPO_NAME_PLACEHOLDER", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](79, _c0, ctx_r1.data.projectName)))("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](28, 48, "GIT_REMOTE.REPO_NAME_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](32, 50, "GIT_REMOTE.REPO_DESC_OPTIONAL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.repositoryDescription)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](34, 52, "GIT_REMOTE.REPO_DESC_PLACEHOLDER"))("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](41, 54, "GIT_REMOTE.VISIBILITY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.isPrivate)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", true);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](47, 56, "GIT_REMOTE.PRIVATE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", false);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](52, 58, "GIT_REMOTE.PUBLIC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r1.getGitHubUrl());
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.saveOrganization)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](57, 60, "GIT_REMOTE.REMEMBER_ORG"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r1.pushAfterAdd)("disabled", ctx_r1.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](60, 62, "GIT_REMOTE.AUTO_PUSH"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r1.error);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](68, 64, "GIT_REMOTE.HOW_TO_CREATE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](72, 66, "GIT_REMOTE.HOW_TO_STEP1"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](75, 68, "GIT_REMOTE.HOW_TO_STEP2", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](81, _c1, ctx_r1.repositoryName || "nome-repository")));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](78, 71, "GIT_REMOTE.HOW_TO_STEP3"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](81, 73, "GIT_REMOTE.HOW_TO_STEP4"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](84, 75, "GIT_REMOTE.HOW_TO_STEP5"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](87, 77, "GIT_REMOTE.HOW_TO_STEP6"));
  }
}
function GitSetupRemoteDialogComponent_mat_spinner_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "mat-spinner", 30);
  }
}
class GitSetupRemoteDialogComponent {
  constructor(dialogRef, data, gitService, snackBar, dialog, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitService = gitService;
    this.snackBar = snackBar;
    this.dialog = dialog;
    this.translate = translate;
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
      next: org => {
        this.organization = org;
        this.isLoading = false;
      },
      error: err => {
        console.error('Error loading organization:', err);
        this.isLoading = false;
      }
    });
  }
  checkToken() {
    this.gitService.getGitHubToken().subscribe({
      next: result => {
        this.hasToken = result.hasToken && result.tokenValid;
      },
      error: err => {
        console.error('Error checking token:', err);
        this.hasToken = false;
      }
    });
  }
  openTokenSettings() {
    const dialogRef = this.dialog.open(_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_0__.GitTokenDialogComponent, {
      width: '650px',
      data: {}
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Token was configured successfully, update the status
        this.checkToken();
        this.snackBar.open(this.translate.instant('GIT_REMOTE.TOKEN_SUCCESS'), 'OK', {
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
      this.error = this.translate.instant('GIT_REMOTE.ORG_REPO_REQUIRED');
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
      next: response => {
        this.isSetting = false;
        if (response.success) {
          this.snackBar.open(response.message || this.translate.instant('GIT_REMOTE.REMOTE_SUCCESS'), 'OK', {
            duration: 5000,
            verticalPosition: 'top'
          });
          this.dialogRef.close(true);
        } else {
          this.error = response.error || this.translate.instant('GIT_REMOTE.REMOTE_ERROR');
        }
      },
      error: err => {
        this.isSetting = false;
        this.error = this.translate.instant('GIT_REMOTE.REMOTE_ERROR_DETAIL', {
          error: err.message || 'Errore sconosciuto'
        });
        console.error('Error setting up remote:', err);
      }
    });
  }
  static {
    this.ɵfac = function GitSetupRemoteDialogComponent_Factory(t) {
      return new (t || GitSetupRemoteDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_4__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_5__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({
      type: GitSetupRemoteDialogComponent,
      selectors: [["app-git-setup-remote-dialog"]],
      decls: 15,
      vars: 16,
      consts: [["mat-dialog-title", ""], ["class", "loading-container", 4, "ngIf"], ["class", "setup-form", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], [1, "loading-container"], ["diameter", "30"], [1, "setup-form"], [1, "instructions"], ["class", "warning-message", 4, "ngIf"], ["appearance", "outline", 1, "full-width"], ["matInput", "", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], ["matSuffix", ""], ["matInput", "", "rows", "2", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], [1, "visibility-option"], [2, "display", "block", "margin-bottom", "8px"], [3, "ngModel", "disabled", "ngModelChange"], [3, "value"], [2, "vertical-align", "middle"], [2, "margin-left", "20px", 3, "value"], ["class", "url-preview", 4, "ngIf"], [1, "options"], ["class", "error-message", 4, "ngIf"], [1, "github-instructions"], [1, "warning-message"], [2, "cursor", "pointer", "text-decoration", "underline", 3, "click"], [1, "url-preview"], [1, "error-message"], ["diameter", "20", 2, "display", "inline-block", "margin-right", "8px"]],
      template: function GitSetupRemoteDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-dialog-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, GitSetupRemoteDialogComponent_div_4_Template, 5, 3, "div", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, GitSetupRemoteDialogComponent_div_5_Template, 88, 83, "div", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "mat-dialog-actions", 3)(7, "button", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_Template_button_click_7_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](9, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "button", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function GitSetupRemoteDialogComponent_Template_button_click_10_listener() {
            return ctx.onSetup();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](11, GitSetupRemoteDialogComponent_mat_spinner_11_Template, 1, 0, "mat-spinner", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](13, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](14, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 8, "GIT_REMOTE.GITHUB_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](9, 10, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.isSetting || !ctx.organization || !ctx.repositoryName);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx.isSetting ? _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](13, 12, "GIT_REMOTE.CONFIGURING") : _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](14, 14, "GIT_REMOTE.CONFIGURE_BTN"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.NgIf, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_7__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_8__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_9__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_10__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_10__.MatLegacyRadioButton, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__.MatExpansionPanel, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__.MatExpansionPanelHeader, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_11__.MatExpansionPanelTitle, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_14__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_3__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_15__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_5__.TranslatePipe],
      styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 16px;\n  padding: 20px;\n  min-height: 200px;\n}\n\n.setup-form[_ngcontent-%COMP%] {\n  min-width: 500px;\n  padding: 16px 0;\n}\n\n.instructions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: #e3f2fd;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n.instructions[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  margin-top: 2px;\n}\n.instructions[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n\n.url-preview[_ngcontent-%COMP%] {\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n  margin-bottom: 20px;\n  font-family: monospace;\n}\n.url-preview[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 8px;\n  font-family: Roboto, sans-serif;\n  color: #666;\n  font-size: 0.9em;\n}\n.url-preview[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  color: #1976d2;\n  word-break: break-all;\n  font-size: 0.95em;\n}\n\n.warning-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: #fff3cd;\n  border: 1px solid #ffc107;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n.warning-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n  margin-top: 2px;\n}\n.warning-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.visibility-option[_ngcontent-%COMP%] {\n  margin: 20px 0;\n  padding: 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n}\n.visibility-option[_ngcontent-%COMP%]   mat-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 24px;\n}\n.visibility-option[_ngcontent-%COMP%]   mat-radio-button[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n}\n\n.options[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n.options[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 12px;\n}\n\n.error-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px;\n  background-color: #ffebee;\n  border-radius: 4px;\n  margin-top: 16px;\n  color: #c62828;\n}\n.error-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #c62828;\n}\n\n.github-instructions[_ngcontent-%COMP%] {\n  margin-top: 24px;\n  box-shadow: none;\n  border: 1px solid #e0e0e0;\n}\n.github-instructions[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n.github-instructions[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  margin: 0;\n  padding-left: 24px;\n  line-height: 1.8;\n}\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   a[_ngcontent-%COMP%] {\n  color: #1976d2;\n  text-decoration: none;\n}\n.github-instructions[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   a[_ngcontent-%COMP%]:hover {\n  text-decoration: underline;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 24px !important;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px !important;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   mat-spinner[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LXNldHVwLXJlbW90ZS1kaWFsb2cvZ2l0LXNldHVwLXJlbW90ZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsaUJBQUE7QUFDRjs7QUFFQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFDRjtBQUNFO0VBQ0UsY0FBQTtFQUNBLGVBQUE7QUFDSjtBQUVFO0VBQ0UsU0FBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQUFKOztBQUlBO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBREY7O0FBSUE7RUFDRSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0VBQ0Esc0JBQUE7QUFERjtBQUdFO0VBQ0UsY0FBQTtFQUNBLGtCQUFBO0VBQ0EsK0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFESjtBQUlFO0VBQ0UsY0FBQTtFQUNBLHFCQUFBO0VBQ0EsaUJBQUE7QUFGSjs7QUFNQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFIRjtBQUtFO0VBQ0UsY0FBQTtFQUNBLGVBQUE7QUFISjtBQU1FO0VBQ0UsU0FBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQUpKOztBQVFBO0VBQ0UsY0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0FBTEY7QUFPRTtFQUNFLGFBQUE7RUFDQSxTQUFBO0FBTEo7QUFRRTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtBQU5KOztBQVVBO0VBQ0UsY0FBQTtBQVBGO0FBU0U7RUFDRSxjQUFBO0VBQ0EsbUJBQUE7QUFQSjs7QUFXQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtBQVJGO0FBVUU7RUFDRSxjQUFBO0FBUko7O0FBWUE7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0VBQ0EseUJBQUE7QUFURjtBQVdFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGlCQUFBO0FBVEo7QUFXSTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQVROO0FBYUU7RUFDRSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtBQVhKO0FBYUk7RUFDRSxrQkFBQTtBQVhOO0FBY0k7RUFDRSxjQUFBO0VBQ0EscUJBQUE7QUFaTjtBQWNNO0VBQ0UsMEJBQUE7QUFaUjs7QUFrQkE7RUFDRSwwQkFBQTtBQWZGOztBQWtCQTtFQUNFLDZCQUFBO0FBZkY7QUFpQkU7RUFDRSxzQkFBQTtBQWZKIiwic291cmNlc0NvbnRlbnQiOlsiLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgZ2FwOiAxNnB4O1xyXG4gIHBhZGRpbmc6IDIwcHg7XHJcbiAgbWluLWhlaWdodDogMjAwcHg7XHJcbn1cclxuXHJcbi5zZXR1cC1mb3JtIHtcclxuICBtaW4td2lkdGg6IDUwMHB4O1xyXG4gIHBhZGRpbmc6IDE2cHggMDtcclxufVxyXG5cclxuLmluc3RydWN0aW9ucyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTZweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZTNmMmZkO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogIzE5NzZkMjtcclxuICAgIG1hcmdpbi10b3A6IDJweDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgY29sb3I6ICM0MjQyNDI7XHJcbiAgICBsaW5lLWhlaWdodDogMS41O1xyXG4gIH1cclxufVxyXG5cclxuLmZ1bGwtd2lkdGgge1xyXG4gIHdpZHRoOiAxMDAlO1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbn1cclxuXHJcbi51cmwtcHJldmlldyB7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xyXG4gIGZvbnQtZmFtaWx5OiBtb25vc3BhY2U7XHJcblxyXG4gIHN0cm9uZyB7XHJcbiAgICBkaXNwbGF5OiBibG9jaztcclxuICAgIG1hcmdpbi1ib3R0b206IDhweDtcclxuICAgIGZvbnQtZmFtaWx5OiBSb2JvdG8sIHNhbnMtc2VyaWY7XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgfVxyXG5cclxuICBjb2RlIHtcclxuICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gICAgd29yZC1icmVhazogYnJlYWstYWxsO1xyXG4gICAgZm9udC1zaXplOiAwLjk1ZW07XHJcbiAgfVxyXG59XHJcblxyXG4ud2FybmluZy1tZXNzYWdlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG4gIGdhcDogMTJweDtcclxuICBwYWRkaW5nOiAxNnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmZmYzY2Q7XHJcbiAgYm9yZGVyOiAxcHggc29saWQgI2ZmYzEwNztcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICNmZjk4MDA7XHJcbiAgICBtYXJnaW4tdG9wOiAycHg7XHJcbiAgfVxyXG5cclxuICBwIHtcclxuICAgIG1hcmdpbjogMDtcclxuICAgIGNvbG9yOiAjNDI0MjQyO1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuNTtcclxuICB9XHJcbn1cclxuXHJcbi52aXNpYmlsaXR5LW9wdGlvbiB7XHJcbiAgbWFyZ2luOiAyMHB4IDA7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuXHJcbiAgbWF0LXJhZGlvLWdyb3VwIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBnYXA6IDI0cHg7XHJcbiAgfVxyXG5cclxuICBtYXQtcmFkaW8tYnV0dG9uIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIH1cclxufVxyXG5cclxuLm9wdGlvbnMge1xyXG4gIG1hcmdpbjogMjBweCAwO1xyXG5cclxuICBtYXQtY2hlY2tib3gge1xyXG4gICAgZGlzcGxheTogYmxvY2s7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxMnB4O1xyXG4gIH1cclxufVxyXG5cclxuLmVycm9yLW1lc3NhZ2Uge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDhweDtcclxuICBwYWRkaW5nOiAxMnB4O1xyXG4gIGJhY2tncm91bmQtY29sb3I6ICNmZmViZWU7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIG1hcmdpbi10b3A6IDE2cHg7XHJcbiAgY29sb3I6ICNjNjI4Mjg7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiAjYzYyODI4O1xyXG4gIH1cclxufVxyXG5cclxuLmdpdGh1Yi1pbnN0cnVjdGlvbnMge1xyXG4gIG1hcmdpbi10b3A6IDI0cHg7XHJcbiAgYm94LXNoYWRvdzogbm9uZTtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICBtYXQtcGFuZWwtdGl0bGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIGZvbnQtc2l6ZTogMC45NWVtO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgb2wge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgcGFkZGluZy1sZWZ0OiAyNHB4O1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuODtcclxuXHJcbiAgICBsaSB7XHJcbiAgICAgIG1hcmdpbi1ib3R0b206IDhweDtcclxuICAgIH1cclxuXHJcbiAgICBhIHtcclxuICAgICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICAgIHRleHQtZGVjb3JhdGlvbjogbm9uZTtcclxuXHJcbiAgICAgICY6aG92ZXIge1xyXG4gICAgICAgIHRleHQtZGVjb3JhdGlvbjogdW5kZXJsaW5lO1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG5tYXQtZGlhbG9nLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDAgMjRweCAhaW1wb3J0YW50O1xyXG59XHJcblxyXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xyXG4gIHBhZGRpbmc6IDE2cHggMjRweCAhaW1wb3J0YW50O1xyXG5cclxuICBtYXQtc3Bpbm5lciB7XHJcbiAgICB2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO1xyXG4gIH1cclxufSJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

/***/ }),

/***/ 8995:
/*!**********************************************************************************************************!*\
  !*** ./src/app/git/dialogs/git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component.ts ***!
  \**********************************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitSetupRemoteGenericDialogComponent": () => (/* binding */ GitSetupRemoteGenericDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _models_remote_setup_models__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../models/remote-setup.models */ 3204);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/gitservice.service */ 7224);
/* harmony import */ var _services_git_credential_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/git-credential.service */ 7554);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/divider */ 1528);
/* harmony import */ var _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/expansion */ 7591);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/forms */ 2508);





















function GitSetupRemoteGenericDialogComponent_mat_icon_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
}
function GitSetupRemoteGenericDialogComponent_mat_spinner_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](0, "mat-spinner", 36);
  }
}
function GitSetupRemoteGenericDialogComponent_mat_icon_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
}
function GitSetupRemoteGenericDialogComponent_mat_icon_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
}
function GitSetupRemoteGenericDialogComponent_div_24_span_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "span", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](2, 1, "GIT_REMOTE.AUTO_CREATE_AVAILABLE"), " ");
  }
}
function GitSetupRemoteGenericDialogComponent_div_24_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 39)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, GitSetupRemoteGenericDialogComponent_div_24_span_6_Template, 3, 3, "span", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const provider_r14 = ctx.ngIf;
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵstyleProp"]("color", provider_r14.color);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](provider_r14.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate2"]("", provider_r14.name, " ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 6, "GIT_REMOTE.DETECTED"), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r4.urlInfo == null ? null : ctx_r4.urlInfo.supportsAutoCreate);
  }
}
function GitSetupRemoteGenericDialogComponent_div_31_mat_option_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-option", 43)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "account_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const credential_r17 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", credential_r17.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", credential_r17.username || credential_r17.accountName, " ");
  }
}
function GitSetupRemoteGenericDialogComponent_div_31_Template(rf, ctx) {
  if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 42)(1, "mat-form-field", 3)(2, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-select", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_div_31_Template_mat_select_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r19);
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r18.selectedCredentialId = $event);
    })("ngModelChange", function GitSetupRemoteGenericDialogComponent_div_31_Template_mat_select_ngModelChange_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r19);
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r20.onCredentialSelectionChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "mat-option", 43)(7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](11, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](12, GitSetupRemoteGenericDialogComponent_div_31_mat_option_12_Template, 4, 2, "mat-option", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 7, "GIT_REMOTE.GITHUB_ACCOUNT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r5.selectedCredentialId)("disabled", ctx_r5.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", null);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](10, 9, "GIT_REMOTE.ENTER_NEW_CREDS"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", ctx_r5.savedGitHubCredentials);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](15, 11, "GIT_REMOTE.SELECT_ACCOUNT_HINT"));
  }
}
function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r25 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-form-field", 3)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "input", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_9_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r25);
      const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r24.password = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "button", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_9_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r25);
      const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r26.showPassword = !ctx_r26.showPassword);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 6, "GIT_REMOTE.PASSWORD_TOKEN"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r21.password)("type", ctx_r21.showPassword ? "text" : "password")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 8, "GIT_REMOTE.PASSWORD_PLACEHOLDER"))("disabled", ctx_r21.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r21.showPassword ? "visibility_off" : "visibility");
  }
}
function GitSetupRemoteGenericDialogComponent_ng_container_32_div_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r28 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 49)(1, "mat-icon", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "span")(4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "a", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_ng_container_32_div_10_Template_a_click_7_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r28);
      const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r27.openTokenCreationUrl());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](6, 2, "GIT_REMOTE.GITHUB_PAT_NOTE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](9, 4, "GIT_REMOTE.CREATE_TOKEN_HERE"), " ");
  }
}
function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_mat_hint_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r31 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-hint")(1, "a", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_mat_hint_9_Template_a_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r31);
      const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r30.openTokenCreationUrl());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 1, "GIT_REMOTE.CREATE_NEW_TOKEN"), " ");
  }
}
function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_Template(rf, ctx) {
  if (rf & 1) {
    const _r33 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-form-field", 3)(1, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "input", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_Template_input_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r33);
      const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r32.token = $event);
    })("ngModelChange", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_Template_input_ngModelChange_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r33);
      const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r34.onTokenChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](6, "button", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r33);
      const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r35.showPassword = !ctx_r35.showPassword);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_mat_hint_9_Template, 4, 3, "mat-hint", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](3, 7, "GIT_REMOTE.PAT_LABEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r23.token)("type", ctx_r23.showPassword ? "text" : "password")("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 9, "GIT_REMOTE.PAT_PLACEHOLDER"))("disabled", ctx_r23.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r23.showPassword ? "visibility_off" : "visibility");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r23.urlInfo == null ? null : ctx_r23.urlInfo.tokenCreationUrl);
  }
}
function GitSetupRemoteGenericDialogComponent_ng_container_32_Template(rf, ctx) {
  if (rf & 1) {
    const _r37 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "mat-form-field", 3)(2, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "input", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_ng_container_32_Template_input_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r37);
      const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r36.username = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "mat-icon", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](8, "person");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](9, GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_9_Template, 9, 10, "mat-form-field", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](10, GitSetupRemoteGenericDialogComponent_ng_container_32_div_10_Template, 10, 6, "div", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](11, GitSetupRemoteGenericDialogComponent_ng_container_32_mat_form_field_11_Template, 10, 11, "mat-form-field", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 7, "GIT_REMOTE.USERNAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r6.username)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](6, 9, "GIT_REMOTE.USERNAME_PLACEHOLDER"))("disabled", ctx_r6.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r6.authMethod === "username_password");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", (ctx_r6.urlInfo == null ? null : ctx_r6.urlInfo.provider) === "github" && ctx_r6.authMethod === "username_password");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r6.authMethod === "pat");
  }
}
function GitSetupRemoteGenericDialogComponent_mat_spinner_34_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](0, "mat-spinner", 53);
  }
}
function GitSetupRemoteGenericDialogComponent_mat_icon_35_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "wifi_tethering");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
}
function GitSetupRemoteGenericDialogComponent_div_39_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 54)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 1, "GIT_REMOTE.CONNECTION_VERIFIED"));
  }
}
function GitSetupRemoteGenericDialogComponent_div_40_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 55)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](5, 1, "GIT_REMOTE.REPO_NOT_FOUND_WILL_CREATE"));
  }
}
function GitSetupRemoteGenericDialogComponent_div_80_div_8_Template(rf, ctx) {
  if (rf & 1) {
    const _r40 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 58)(1, "div", 59)(2, "mat-radio-group", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_div_80_div_8_Template_mat_radio_group_ngModelChange_2_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r40);
      const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r39.isPrivate = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-radio-button", 43)(4, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5, "lock");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "mat-radio-button", 43)(9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10, "public");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "mat-form-field", 3)(14, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](17, "input", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_div_80_div_8_Template_input_ngModelChange_17_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r40);
      const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r41.repoDescription = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r38.isPrivate)("disabled", ctx_r38.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", true);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](7, 10, "GIT_REMOTE.PRIVATE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("value", false);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](12, 12, "GIT_REMOTE.PUBLIC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](16, 14, "GIT_REMOTE.REPO_DESC_OPTIONAL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r38.repoDescription)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](18, 16, "GIT_REMOTE.REPO_DESC_PLACEHOLDER"))("disabled", ctx_r38.isSetting);
  }
}
const _c0 = function (a0) {
  return {
    provider: a0
  };
};
function GitSetupRemoteGenericDialogComponent_div_80_Template(rf, ctx) {
  if (rf & 1) {
    const _r43 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](1, "mat-divider");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_div_80_Template_mat_checkbox_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r43);
      const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r42.createRemoteRepo = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](8, GitSetupRemoteGenericDialogComponent_div_80_div_8_Template, 19, 18, "div", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    let tmp_0_0;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind2"](4, 5, "GIT_REMOTE.AUTO_CREATE_TITLE", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpureFunction1"](10, _c0, (tmp_0_0 = ctx_r11.getProviderInfo()) == null ? null : tmp_0_0.name)));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx_r11.createRemoteRepo)("disabled", ctx_r11.isSetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](7, 8, "GIT_REMOTE.AUTO_CREATE_CHECKBOX"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx_r11.createRemoteRepo);
  }
}
function GitSetupRemoteGenericDialogComponent_div_81_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 60)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx_r12.error);
  }
}
function GitSetupRemoteGenericDialogComponent_mat_spinner_119_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](0, "mat-spinner", 61);
  }
}
class GitSetupRemoteGenericDialogComponent {
  constructor(dialogRef, data, gitService, gitCredentialService, snackBar, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.gitService = gitService;
    this.gitCredentialService = gitCredentialService;
    this.snackBar = snackBar;
    this.translate = translate;
    // Main form fields
    this.remoteUrl = '';
    this.username = '';
    this.password = '';
    this.token = '';
    // Options
    this.saveCredentials = true;
    this.pushAfterAdd = true;
    this.remoteName = 'origin';
    // Advanced options
    this.authMethod = 'username_password';
    this.createRemoteRepo = false;
    this.repoDescription = '';
    this.isPrivate = true;
    // State
    this.isLoading = false;
    this.isValidating = false;
    this.isSetting = false;
    this.showAdvanced = false;
    this.showPassword = false;
    this.error = null;
    // URL parsing result
    this.urlInfo = null;
    this.validationResult = null;
    // Multi-account support (uses unique credentials)
    this.savedGitHubCredentials = [];
    this.selectedCredentialId = null;
    // Provider info for display
    this.providerInfo = _models_remote_setup_models__WEBPACK_IMPORTED_MODULE_0__.PROVIDER_INFO;
  }
  ngOnInit() {
    // Use pre-filled URL if available (from toolbar when reconfiguring credentials)
    if (this.data?.prefilledRemoteUrl) {
      this.remoteUrl = this.data.prefilledRemoteUrl;
      this.onUrlChange(); // Trigger provider detection and GitHub token check
    }
  }
  /**
   * Handle URL input change - parse and detect provider
   */
  onUrlChange() {
    this.error = null;
    this.validationResult = null;
    if (!this.remoteUrl || this.remoteUrl.trim().length < 10) {
      this.urlInfo = null;
      this.savedGitHubCredentials = [];
      return;
    }
    this.isValidating = true;
    this.gitService.parseRemoteUrl(this.remoteUrl.trim()).subscribe({
      next: result => {
        this.urlInfo = result;
        this.isValidating = false;
        if (!result.isValid) {
          this.error = result.error || this.translate.instant('GIT_REMOTE.URL_INVALID');
        }
        // If GitHub detected, load saved credentials
        if (result.provider === 'github') {
          this.loadSavedGitHubCredentials();
          // GitHub requires PAT, auto-select it
          this.authMethod = 'pat';
        } else {
          this.savedGitHubCredentials = [];
          this.selectedCredentialId = null;
        }
      },
      error: err => {
        console.error('Error parsing URL:', err);
        this.isValidating = false;
        this.urlInfo = null;
      }
    });
  }
  /**
   * Validate connection with credentials
   */
  validateConnection() {
    if (!this.remoteUrl || !this.urlInfo?.isValid) {
      return;
    }
    this.isValidating = true;
    this.error = null;
    this.validationResult = null;
    const effectivePassword = this.getEffectivePassword();
    this.gitService.validateRemoteAuth({
      remoteUrl: this.remoteUrl,
      username: this.username,
      password: effectivePassword,
      authMethod: this.authMethod
    }).subscribe({
      next: result => {
        this.validationResult = result;
        this.isValidating = false;
        if (result.isReachable && result.credentialsValid) {
          this.snackBar.open(this.translate.instant('GIT_REMOTE.VERIFY_SUCCESS'), 'OK', {
            duration: 3000,
            verticalPosition: 'top'
          });
        } else if (result.error) {
          this.error = result.error;
        }
      },
      error: err => {
        console.error('Error validating connection:', err);
        this.isValidating = false;
        this.error = this.translate.instant('GIT_REMOTE.VERIFY_ERROR');
      }
    });
  }
  /**
   * Open token creation URL for the detected provider
   */
  openTokenCreationUrl() {
    if (this.urlInfo?.tokenCreationUrl) {
      window.open(this.urlInfo.tokenCreationUrl, '_blank');
    }
  }
  /**
   * Get effective password based on auth method
   */
  getEffectivePassword() {
    if (this.authMethod === 'pat' && this.token) {
      return this.token;
    }
    return this.password;
  }
  /**
   * Auto-select PAT auth method when user types in token field
   */
  onTokenChange() {
    if (this.token && this.token.length > 0) {
      this.authMethod = 'pat';
    }
  }
  /**
   * Load saved GitHub credentials from credential service
   * Now returns unique credentials directly (no more deduplication needed)
   */
  loadSavedGitHubCredentials() {
    this.gitCredentialService.getCredentialsByType('GitHub').subscribe({
      next: credentials => {
        this.savedGitHubCredentials = credentials;
      },
      error: err => {
        console.error('Error loading saved GitHub credentials:', err);
        this.savedGitHubCredentials = [];
      }
    });
  }
  /**
   * Get display names of saved GitHub credentials
   */
  getSavedCredentialNames() {
    return this.savedGitHubCredentials.map(c => c.username || c.accountName).join(', ');
  }
  /**
   * Handle credential selection change
   */
  onCredentialSelectionChange() {
    if (!this.selectedCredentialId) {
      // When deselected (new credentials), clear manual input fields
      this.username = '';
      this.token = '';
      this.password = '';
    }
  }
  /**
   * Get provider display info
   */
  getProviderInfo() {
    if (!this.urlInfo?.provider) return null;
    return this.providerInfo[this.urlInfo.provider] || this.providerInfo['generic'];
  }
  /**
   * Check if form is valid for setup
   */
  isFormValid() {
    if (!this.remoteUrl || !this.urlInfo?.isValid) {
      return false;
    }
    // If using a saved credential
    if (this.selectedCredentialId) {
      return true;
    }
    // Must have credentials (username + password/token)
    const hasCredentials = this.username && (this.authMethod === 'pat' ? this.token : this.password);
    return !!hasCredentials;
  }
  /**
   * Toggle advanced options panel
   */
  toggleAdvanced() {
    this.showAdvanced = !this.showAdvanced;
  }
  /**
   * Cancel and close dialog
   */
  onCancel() {
    this.dialogRef.close(false);
  }
  /**
   * Setup the remote
   */
  onSetup() {
    if (!this.isFormValid()) {
      this.error = this.translate.instant('GIT_REMOTE.FILL_ALL_REQUIRED');
      return;
    }
    this.isSetting = true;
    this.error = null;
    const effectivePassword = this.getEffectivePassword();
    const useExistingCredential = !!this.selectedCredentialId;
    const request = {
      repositoryPath: this.data.projectPath,
      remoteUrl: this.remoteUrl,
      remoteName: this.remoteName || 'origin',
      authMethod: this.authMethod || 'username_password',
      username: useExistingCredential ? '' : this.username || '',
      password: useExistingCredential ? '' : this.authMethod === 'username_password' ? this.password || '' : '',
      token: useExistingCredential ? '' : this.authMethod === 'pat' ? this.token || '' : '',
      saveCredentials: this.saveCredentials === true,
      pushAfterAdd: this.pushAfterAdd === true,
      createRemoteRepo: this.createRemoteRepo === true && this.urlInfo?.supportsAutoCreate === true,
      repoDescription: this.repoDescription || '',
      isPrivate: this.isPrivate !== false,
      useSavedToken: false,
      copyFromCredentialId: this.selectedCredentialId || undefined
    };
    console.log('Setup remote request:', request);
    this.gitService.setupRemoteGeneric(request).subscribe({
      next: response => {
        this.isSetting = false;
        if (response.success) {
          this.snackBar.open(response.message || this.translate.instant('GIT_REMOTE.REMOTE_SUCCESS'), 'OK', {
            duration: 5000,
            verticalPosition: 'top'
          });
          this.dialogRef.close(true);
        } else {
          this.error = response.error || this.translate.instant('GIT_REMOTE.REMOTE_ERROR');
        }
      },
      error: err => {
        this.isSetting = false;
        this.error = this.translate.instant('GIT_REMOTE.REMOTE_ERROR_DETAIL', {
          error: err.message || 'Errore sconosciuto'
        });
        console.error('Error setting up remote:', err);
      }
    });
  }
  static {
    this.ɵfac = function GitSetupRemoteGenericDialogComponent_Factory(t) {
      return new (t || GitSetupRemoteGenericDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_gitservice_service__WEBPACK_IMPORTED_MODULE_1__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_services_git_credential_service__WEBPACK_IMPORTED_MODULE_2__.GitCredentialService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_5__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({
      type: GitSetupRemoteGenericDialogComponent,
      selectors: [["app-git-setup-remote-generic-dialog"]],
      decls: 123,
      vars: 113,
      consts: [["mat-dialog-title", ""], [1, "setup-form"], [1, "instructions"], ["appearance", "outline", 1, "full-width"], ["matInput", "", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], ["matSuffix", "", 4, "ngIf"], ["matSuffix", "", "diameter", "20", 4, "ngIf"], ["matSuffix", "", "style", "color: #4caf50;", 4, "ngIf"], ["matSuffix", "", "style", "color: #f44336;", 4, "ngIf"], ["class", "provider-badge", 4, "ngIf"], [1, "section-divider"], [1, "section-title"], ["class", "saved-credentials-section", 4, "ngIf"], [4, "ngIf"], ["mat-stroked-button", "", "color", "primary", 1, "validate-button", 3, "disabled", "click"], ["diameter", "18", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], ["class", "validation-result success", 4, "ngIf"], ["class", "validation-result warning", 4, "ngIf"], [1, "options"], [3, "ngModel", "disabled", "ngModelChange"], [1, "advanced-panel", 3, "expanded"], [3, "click"], [1, "advanced-content"], [1, "auth-method-section"], [1, "field-label"], ["value", "username_password"], ["value", "pat"], ["class", "auto-create-section", 4, "ngIf"], ["class", "error-message", 4, "ngIf"], [1, "help-panel"], [1, "help-content"], ["align", "end"], ["mat-button", "", 3, "disabled", "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "style", "display: inline-block; margin-right: 8px;", 4, "ngIf"], ["matSuffix", ""], ["matSuffix", "", "diameter", "20"], ["matSuffix", "", 2, "color", "#4caf50"], ["matSuffix", "", 2, "color", "#f44336"], [1, "provider-badge"], ["class", "auto-create-badge", 4, "ngIf"], [1, "auto-create-badge"], [1, "saved-credentials-section"], [3, "value"], [3, "value", 4, "ngFor", "ngForOf"], ["appearance", "outline", "class", "full-width", 4, "ngIf"], ["class", "github-pat-warning", 4, "ngIf"], ["matInput", "", 3, "ngModel", "type", "placeholder", "disabled", "ngModelChange"], ["mat-icon-button", "", "matSuffix", "", "type", "button", 3, "click"], [1, "github-pat-warning"], ["color", "warn"], [2, "cursor", "pointer", "color", "#1976d2", "text-decoration", "underline", 3, "click"], [2, "cursor", "pointer", "color", "#1976d2", 3, "click"], ["diameter", "18", 2, "display", "inline-block", "margin-right", "8px"], [1, "validation-result", "success"], [1, "validation-result", "warning"], [1, "auto-create-section"], ["class", "create-repo-options", 4, "ngIf"], [1, "create-repo-options"], [1, "visibility-option"], [1, "error-message"], ["diameter", "20", 2, "display", "inline-block", "margin-right", "8px"]],
      template: function GitSetupRemoteGenericDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-dialog-content")(4, "div", 1)(5, "div", 2)(6, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "info");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](9);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](10, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "mat-form-field", 3)(12, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](14, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](15, "input", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_input_ngModelChange_15_listener($event) {
            return ctx.remoteUrl = $event;
          })("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_input_ngModelChange_15_listener() {
            return ctx.onUrlChange();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](16, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](17, GitSetupRemoteGenericDialogComponent_mat_icon_17_Template, 2, 0, "mat-icon", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](18, GitSetupRemoteGenericDialogComponent_mat_spinner_18_Template, 1, 0, "mat-spinner", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](19, GitSetupRemoteGenericDialogComponent_mat_icon_19_Template, 2, 0, "mat-icon", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](20, GitSetupRemoteGenericDialogComponent_mat_icon_20_Template, 2, 0, "mat-icon", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](21, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](22);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](24, GitSetupRemoteGenericDialogComponent_div_24_Template, 7, 8, "div", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelement"](25, "mat-divider", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](26, "h3", 11)(27, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](28, "key");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](29);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](30, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](31, GitSetupRemoteGenericDialogComponent_div_31_Template, 16, 13, "div", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](32, GitSetupRemoteGenericDialogComponent_ng_container_32_Template, 12, 11, "ng-container", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](33, "button", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_Template_button_click_33_listener() {
            return ctx.validateConnection();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](34, GitSetupRemoteGenericDialogComponent_mat_spinner_34_Template, 1, 0, "mat-spinner", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](35, GitSetupRemoteGenericDialogComponent_mat_icon_35_Template, 2, 0, "mat-icon", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](36);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](37, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](38, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](39, GitSetupRemoteGenericDialogComponent_div_39_Template, 6, 3, "div", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](40, GitSetupRemoteGenericDialogComponent_div_40_Template, 6, 3, "div", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](41, "div", 18)(42, "mat-checkbox", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_mat_checkbox_ngModelChange_42_listener($event) {
            return ctx.saveCredentials = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](43);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](44, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](45, "mat-checkbox", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_mat_checkbox_ngModelChange_45_listener($event) {
            return ctx.pushAfterAdd = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](46);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](47, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](48, "mat-expansion-panel", 20)(49, "mat-expansion-panel-header", 21);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_Template_mat_expansion_panel_header_click_49_listener() {
            return ctx.toggleAdvanced();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](50, "mat-panel-title")(51, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](52, "settings");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](53);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](54, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](55, "div", 22)(56, "div", 23)(57, "mat-label", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](58);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](59, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](60, "mat-radio-group", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_mat_radio_group_ngModelChange_60_listener($event) {
            return ctx.authMethod = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](61, "mat-radio-button", 25)(62, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](63, "vpn_key");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](64);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](65, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](66, "mat-radio-button", 26)(67, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](68, "token");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](69);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](70, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](71, "mat-form-field", 3)(72, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](73);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](74, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](75, "input", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("ngModelChange", function GitSetupRemoteGenericDialogComponent_Template_input_ngModelChange_75_listener($event) {
            return ctx.remoteName = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](76, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](77, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](78);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](79, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](80, GitSetupRemoteGenericDialogComponent_div_80_Template, 9, 12, "div", 27);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](81, GitSetupRemoteGenericDialogComponent_div_81_Template, 5, 1, "div", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](82, "mat-expansion-panel", 29)(83, "mat-expansion-panel-header")(84, "mat-panel-title")(85, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](86, "help_outline");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](87);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](88, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](89, "div", 30)(90, "h4");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](91);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](92, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](93, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](94);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](95, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](96, "h4");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](97);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](98, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](99, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](100);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](101, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](102, "h4");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](103);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](104, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](105, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](106);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](107, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](108, "h4");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](109);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](110, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](111, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](112);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](113, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](114, "mat-dialog-actions", 31)(115, "button", 32);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_Template_button_click_115_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](116);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](117, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](118, "button", 33);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function GitSetupRemoteGenericDialogComponent_Template_button_click_118_listener() {
            return ctx.onSetup();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](119, GitSetupRemoteGenericDialogComponent_mat_spinner_119_Template, 1, 0, "mat-spinner", 34);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](120);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](121, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](122, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](2, 55, "GIT_REMOTE.GENERIC_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](10, 57, "GIT_REMOTE.GENERIC_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](14, 59, "GIT_REMOTE.REMOTE_URL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.remoteUrl)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](16, 61, "GIT_REMOTE.REMOTE_URL_PLACEHOLDER"))("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isValidating && !ctx.urlInfo);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isValidating);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.urlInfo == null ? null : ctx.urlInfo.isValid);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.urlInfo && !ctx.urlInfo.isValid);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](23, 63, "GIT_REMOTE.REMOTE_URL_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", (ctx.urlInfo == null ? null : ctx.urlInfo.isValid) && ctx.getProviderInfo());
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](30, 65, "GIT_REMOTE.AUTH_SECTION"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", (ctx.urlInfo == null ? null : ctx.urlInfo.provider) === "github" && ctx.savedGitHubCredentials.length > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.selectedCredentialId);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", !ctx.remoteUrl || !(ctx.urlInfo == null ? null : ctx.urlInfo.isValid) || ctx.isValidating || ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isValidating);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", !ctx.isValidating);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", ctx.isValidating ? _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](37, 67, "GIT_REMOTE.VERIFYING") : _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](38, 69, "GIT_REMOTE.VERIFY_CONNECTION"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", (ctx.validationResult == null ? null : ctx.validationResult.isReachable) && (ctx.validationResult == null ? null : ctx.validationResult.credentialsValid));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", (ctx.validationResult == null ? null : ctx.validationResult.isReachable) && !(ctx.validationResult == null ? null : ctx.validationResult.repositoryExists));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.saveCredentials)("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](44, 71, "GIT_REMOTE.SAVE_CREDENTIALS"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.pushAfterAdd)("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](47, 73, "GIT_REMOTE.AUTO_PUSH"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("expanded", ctx.showAdvanced);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](54, 75, "GIT_REMOTE.ADVANCED_OPTIONS"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](59, 77, "GIT_REMOTE.AUTH_METHOD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.authMethod)("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](65, 79, "GIT_REMOTE.USER_PASSWORD"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](70, 81, "GIT_REMOTE.PAT"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](74, 83, "GIT_REMOTE.REMOTE_NAME"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngModel", ctx.remoteName)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](76, 85, "GIT_REMOTE.REMOTE_NAME_PLACEHOLDER"))("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](79, 87, "GIT_REMOTE.REMOTE_NAME_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.urlInfo == null ? null : ctx.urlInfo.supportsAutoCreate);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.error);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](88, 89, "GIT_REMOTE.HELP_TITLE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](92, 91, "GIT_REMOTE.HELP_STEP1_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](95, 93, "GIT_REMOTE.HELP_STEP1_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](98, 95, "GIT_REMOTE.HELP_STEP2_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](101, 97, "GIT_REMOTE.HELP_STEP2_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](104, 99, "GIT_REMOTE.HELP_STEP3_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](107, 101, "GIT_REMOTE.HELP_STEP3_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](110, 103, "GIT_REMOTE.HELP_STEP4_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](113, 105, "GIT_REMOTE.HELP_STEP4_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](117, 107, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("disabled", ctx.isSetting || !ctx.isFormValid());
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngIf", ctx.isSetting);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", ctx.isSetting ? _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](121, 109, "GIT_REMOTE.CONFIGURING") : _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](122, 111, "GIT_REMOTE.CONFIGURE_BTN"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_7__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_7__.NgIf, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_8__.MatLegacyOption, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_9__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_10__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_10__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_10__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_10__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_11__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_12__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_12__.MatLegacyRadioButton, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_13__.MatLegacySelect, _angular_material_divider__WEBPACK_IMPORTED_MODULE_14__.MatDivider, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__.MatExpansionPanel, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__.MatExpansionPanelHeader, _angular_material_expansion__WEBPACK_IMPORTED_MODULE_15__.MatExpansionPanelTitle, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_16__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_18__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_6__.TranslatePipe],
      styles: [".setup-form[_ngcontent-%COMP%] {\n  min-width: 550px;\n  max-width: 600px;\n  padding: 16px 0;\n}\n\n.instructions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 12px;\n  padding: 16px;\n  background-color: #e3f2fd;\n  border-radius: 4px;\n  margin-bottom: 24px;\n}\n.instructions[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  margin-top: 2px;\n}\n.instructions[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #424242;\n  line-height: 1.5;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n\n.github-pat-warning[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 8px;\n  padding: 12px;\n  background-color: #fff3e0;\n  border-radius: 4px;\n  margin-bottom: 16px;\n  border-left: 4px solid #ff9800;\n}\n.github-pat-warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n  flex-shrink: 0;\n  margin-top: 2px;\n}\n.github-pat-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  color: #5d4037;\n  font-size: 0.9em;\n  line-height: 1.5;\n}\n.github-pat-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]   a[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n\n.provider-badge[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 8px 12px;\n  background-color: #f5f5f5;\n  border-radius: 4px;\n  margin-bottom: 16px;\n  font-size: 0.9em;\n}\n.provider-badge[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n.provider-badge[_ngcontent-%COMP%]   .auto-create-badge[_ngcontent-%COMP%] {\n  margin-left: auto;\n  padding: 2px 8px;\n  background-color: #e8f5e9;\n  color: #2e7d32;\n  border-radius: 12px;\n  font-size: 0.85em;\n}\n\n.section-divider[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\n.section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 1em;\n  font-weight: 500;\n  color: #424242;\n  margin: 16px 0;\n}\n.section-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n  color: #666;\n}\n\n.saved-token-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%] {\n  padding: 16px;\n  background-color: #f5f5f5;\n  border-radius: 8px;\n  border: 2px solid transparent;\n  transition: border-color 0.2s, background-color 0.2s;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card.selected[_ngcontent-%COMP%] {\n  border-color: #1976d2;\n  background-color: #e3f2fd;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-header[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-header[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  font-weight: 500;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px;\n  background-color: rgba(255, 255, 255, 0.7);\n  border-radius: 4px;\n  margin-top: 8px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]    > mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  height: 24px;\n  width: 24px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]    > mat-icon.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]    > mat-icon.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 1em;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .username[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  color: #666;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.valid[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-info[_ngcontent-%COMP%]   .token-details[_ngcontent-%COMP%]   .status.invalid[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-warning[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 8px;\n  padding: 8px 12px;\n  background-color: #fff3e0;\n  border-radius: 4px;\n  margin-top: 12px;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  flex-shrink: 0;\n}\n.saved-token-section[_ngcontent-%COMP%]   .saved-token-card[_ngcontent-%COMP%]   .saved-token-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  font-size: 0.85em;\n  color: #5d4037;\n  line-height: 1.4;\n}\n.saved-token-section[_ngcontent-%COMP%]   .or-divider[_ngcontent-%COMP%] {\n  text-align: center;\n  margin: 16px 0;\n  color: #666;\n  font-size: 0.9em;\n}\n.saved-token-section[_ngcontent-%COMP%]   .or-divider[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  background-color: white;\n  padding: 0 12px;\n}\n\n.saved-accounts-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n.saved-accounts-section[_ngcontent-%COMP%]   .account-repo-hint[_ngcontent-%COMP%] {\n  font-size: 0.8em;\n  color: #666;\n  margin-left: 4px;\n}\n.saved-accounts-section[_ngcontent-%COMP%]   mat-option[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n  vertical-align: middle;\n}\n\n.validate-button[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n.validate-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 8px;\n}\n\n.validation-result[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px;\n  border-radius: 4px;\n  margin-bottom: 16px;\n}\n.validation-result.success[_ngcontent-%COMP%] {\n  background-color: #e8f5e9;\n  color: #2e7d32;\n}\n.validation-result.success[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.validation-result.warning[_ngcontent-%COMP%] {\n  background-color: #fff3e0;\n  color: #e65100;\n}\n.validation-result.warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n}\n.validation-result.error[_ngcontent-%COMP%] {\n  background-color: #ffebee;\n  color: #c62828;\n}\n.validation-result.error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #f44336;\n}\n\n.options[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n.options[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 12px;\n}\n\n.advanced-panel[_ngcontent-%COMP%] {\n  margin: 16px 0;\n  box-shadow: none !important;\n  border: 1px solid #e0e0e0;\n}\n.advanced-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n.advanced-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n.advanced-content[_ngcontent-%COMP%] {\n  padding-top: 16px;\n}\n\n.auth-method-section[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n}\n.auth-method-section[_ngcontent-%COMP%]   .field-label[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 12px;\n  font-size: 0.9em;\n  color: #666;\n}\n.auth-method-section[_ngcontent-%COMP%]   mat-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n.auth-method-section[_ngcontent-%COMP%]   mat-radio-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  vertical-align: middle;\n}\n\n.auto-create-section[_ngcontent-%COMP%] {\n  margin-top: 16px;\n}\n.auto-create-section[_ngcontent-%COMP%]   mat-divider[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n.auto-create-section[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  font-size: 0.95em;\n  font-weight: 500;\n  color: #424242;\n  margin: 0 0 12px 0;\n}\n\n.create-repo-options[_ngcontent-%COMP%] {\n  margin-top: 12px;\n  padding-left: 28px;\n}\n\n.visibility-option[_ngcontent-%COMP%] {\n  margin: 12px 0 16px 0;\n}\n.visibility-option[_ngcontent-%COMP%]   mat-radio-group[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 24px;\n}\n.visibility-option[_ngcontent-%COMP%]   mat-radio-button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n  vertical-align: middle;\n}\n\n.error-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px;\n  background-color: #ffebee;\n  border-radius: 4px;\n  margin-top: 16px;\n  color: #c62828;\n}\n.error-message[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #c62828;\n}\n\n.help-panel[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  box-shadow: none !important;\n  border: 1px solid #e0e0e0;\n}\n.help-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-size: 0.95em;\n}\n.help-panel[_ngcontent-%COMP%]   mat-panel-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n.help-content[_ngcontent-%COMP%] {\n  padding-top: 8px;\n}\n.help-content[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  font-size: 0.95em;\n  font-weight: 500;\n  color: #424242;\n  margin: 16px 0 8px 0;\n}\n.help-content[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%]:first-child {\n  margin-top: 0;\n}\n.help-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  color: #666;\n  font-size: 0.9em;\n  line-height: 1.5;\n}\n\nmat-dialog-content[_ngcontent-%COMP%] {\n  padding: 0 24px !important;\n  max-height: 70vh;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px !important;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   mat-spinner[_ngcontent-%COMP%] {\n  vertical-align: middle;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvZ2l0L2RpYWxvZ3MvZ2l0LXNldHVwLXJlbW90ZS1nZW5lcmljLWRpYWxvZy9naXQtc2V0dXAtcmVtb3RlLWdlbmVyaWMtZGlhbG9nLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBQ0Y7QUFDRTtFQUNFLGNBQUE7RUFDQSxlQUFBO0FBQ0o7QUFFRTtFQUNFLFNBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFBSjs7QUFJQTtFQUNFLFdBQUE7RUFDQSxtQkFBQTtBQURGOztBQUlBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsUUFBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtBQURGO0FBR0U7RUFDRSxjQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7QUFESjtBQUlFO0VBQ0UsY0FBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7QUFGSjtBQUlJO0VBQ0UsZ0JBQUE7QUFGTjs7QUFPQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0FBSkY7QUFNRTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQUpKO0FBT0U7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EseUJBQUE7RUFDQSxjQUFBO0VBQ0EsbUJBQUE7RUFDQSxpQkFBQTtBQUxKOztBQVNBO0VBQ0UsY0FBQTtBQU5GOztBQVNBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxjQUFBO0FBTkY7QUFRRTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLFdBQUE7QUFOSjs7QUFVQTtFQUNFLG1CQUFBO0FBUEY7QUFTRTtFQUNFLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsNkJBQUE7RUFDQSxvREFBQTtBQVBKO0FBU0k7RUFDRSxxQkFBQTtFQUNBLHlCQUFBO0FBUE47QUFVSTtFQUNFLGtCQUFBO0FBUk47QUFVTTtFQUNFLGdCQUFBO0FBUlI7QUFZSTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsMENBQUE7RUFDQSxrQkFBQTtFQUNBLGVBQUE7QUFWTjtBQVlNO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBVlI7QUFZUTtFQUNFLGNBQUE7QUFWVjtBQWFRO0VBQ0UsY0FBQTtBQVhWO0FBZU07RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBYlI7QUFlUTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxjQUFBO0FBYlY7QUFlVTtFQUNFLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLFdBQUE7QUFiWjtBQWlCUTtFQUNFLGlCQUFBO0FBZlY7QUFpQlU7RUFDRSxjQUFBO0FBZlo7QUFrQlU7RUFDRSxjQUFBO0FBaEJaO0FBc0JJO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsUUFBQTtFQUNBLGlCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBcEJOO0FBc0JNO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsY0FBQTtBQXBCUjtBQXVCTTtFQUNFLGlCQUFBO0VBQ0EsY0FBQTtFQUNBLGdCQUFBO0FBckJSO0FBMEJFO0VBQ0Usa0JBQUE7RUFDQSxjQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0FBeEJKO0FBMEJJO0VBQ0UsdUJBQUE7RUFDQSxlQUFBO0FBeEJOOztBQTZCQTtFQUNFLG1CQUFBO0FBMUJGO0FBNEJFO0VBQ0UsZ0JBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUExQko7QUE2QkU7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLHNCQUFBO0FBM0JKOztBQStCQTtFQUNFLG1CQUFBO0FBNUJGO0FBOEJFO0VBQ0UsaUJBQUE7QUE1Qko7O0FBZ0NBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGFBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0FBN0JGO0FBK0JFO0VBQ0UseUJBQUE7RUFDQSxjQUFBO0FBN0JKO0FBK0JJO0VBQ0UsY0FBQTtBQTdCTjtBQWlDRTtFQUNFLHlCQUFBO0VBQ0EsY0FBQTtBQS9CSjtBQWlDSTtFQUNFLGNBQUE7QUEvQk47QUFtQ0U7RUFDRSx5QkFBQTtFQUNBLGNBQUE7QUFqQ0o7QUFtQ0k7RUFDRSxjQUFBO0FBakNOOztBQXNDQTtFQUNFLGNBQUE7QUFuQ0Y7QUFxQ0U7RUFDRSxjQUFBO0VBQ0EsbUJBQUE7QUFuQ0o7O0FBdUNBO0VBQ0UsY0FBQTtFQUNBLDJCQUFBO0VBQ0EseUJBQUE7QUFwQ0Y7QUFzQ0U7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7QUFwQ0o7QUFzQ0k7RUFDRSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7QUFwQ047O0FBeUNBO0VBQ0UsaUJBQUE7QUF0Q0Y7O0FBeUNBO0VBQ0UsbUJBQUE7QUF0Q0Y7QUF3Q0U7RUFDRSxjQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLFdBQUE7QUF0Q0o7QUF5Q0U7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBdkNKO0FBMkNJO0VBQ0UsaUJBQUE7RUFDQSxlQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxzQkFBQTtBQXpDTjs7QUE4Q0E7RUFDRSxnQkFBQTtBQTNDRjtBQTZDRTtFQUNFLG1CQUFBO0FBM0NKO0FBOENFO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxrQkFBQTtBQTVDSjs7QUFnREE7RUFDRSxnQkFBQTtFQUNBLGtCQUFBO0FBN0NGOztBQWdEQTtFQUNFLHFCQUFBO0FBN0NGO0FBK0NFO0VBQ0UsYUFBQTtFQUNBLFNBQUE7QUE3Q0o7QUFpREk7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLHNCQUFBO0FBL0NOOztBQW9EQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtBQWpERjtBQW1ERTtFQUNFLGNBQUE7QUFqREo7O0FBcURBO0VBQ0UsZ0JBQUE7RUFDQSwyQkFBQTtFQUNBLHlCQUFBO0FBbERGO0FBb0RFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGlCQUFBO0FBbERKO0FBb0RJO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBbEROOztBQXVEQTtFQUNFLGdCQUFBO0FBcERGO0FBc0RFO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxvQkFBQTtBQXBESjtBQXNESTtFQUNFLGFBQUE7QUFwRE47QUF3REU7RUFDRSxTQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7QUF0REo7O0FBMERBO0VBQ0UsMEJBQUE7RUFDQSxnQkFBQTtBQXZERjs7QUEwREE7RUFDRSw2QkFBQTtBQXZERjtBQXlERTtFQUNFLHNCQUFBO0FBdkRKIiwic291cmNlc0NvbnRlbnQiOlsiLnNldHVwLWZvcm0ge1xyXG4gIG1pbi13aWR0aDogNTUwcHg7XHJcbiAgbWF4LXdpZHRoOiA2MDBweDtcclxuICBwYWRkaW5nOiAxNnB4IDA7XHJcbn1cclxuXHJcbi5pbnN0cnVjdGlvbnMge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGZsZXgtc3RhcnQ7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDE2cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2UzZjJmZDtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICBtYXJnaW4tdG9wOiAycHg7XHJcbiAgfVxyXG5cclxuICBwIHtcclxuICAgIG1hcmdpbjogMDtcclxuICAgIGNvbG9yOiAjNDI0MjQyO1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuNTtcclxuICB9XHJcbn1cclxuXHJcbi5mdWxsLXdpZHRoIHtcclxuICB3aWR0aDogMTAwJTtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG59XHJcblxyXG4uZ2l0aHViLXBhdC13YXJuaW5nIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG4gIGdhcDogOHB4O1xyXG4gIHBhZGRpbmc6IDEycHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogI2ZmZjNlMDtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICBib3JkZXItbGVmdDogNHB4IHNvbGlkICNmZjk4MDA7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiAjZmY5ODAwO1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgICBtYXJnaW4tdG9wOiAycHg7XHJcbiAgfVxyXG5cclxuICBzcGFuIHtcclxuICAgIGNvbG9yOiAjNWQ0MDM3O1xyXG4gICAgZm9udC1zaXplOiAwLjllbTtcclxuICAgIGxpbmUtaGVpZ2h0OiAxLjU7XHJcblxyXG4gICAgYSB7XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4ucHJvdmlkZXItYmFkZ2Uge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDhweDtcclxuICBwYWRkaW5nOiA4cHggMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gIGZvbnQtc2l6ZTogMC45ZW07XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgIGhlaWdodDogMjBweDtcclxuICAgIHdpZHRoOiAyMHB4O1xyXG4gIH1cclxuXHJcbiAgLmF1dG8tY3JlYXRlLWJhZGdlIHtcclxuICAgIG1hcmdpbi1sZWZ0OiBhdXRvO1xyXG4gICAgcGFkZGluZzogMnB4IDhweDtcclxuICAgIGJhY2tncm91bmQtY29sb3I6ICNlOGY1ZTk7XHJcbiAgICBjb2xvcjogIzJlN2QzMjtcclxuICAgIGJvcmRlci1yYWRpdXM6IDEycHg7XHJcbiAgICBmb250LXNpemU6IDAuODVlbTtcclxuICB9XHJcbn1cclxuXHJcbi5zZWN0aW9uLWRpdmlkZXIge1xyXG4gIG1hcmdpbjogMjBweCAwO1xyXG59XHJcblxyXG4uc2VjdGlvbi10aXRsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIGZvbnQtc2l6ZTogMWVtO1xyXG4gIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgY29sb3I6ICM0MjQyNDI7XHJcbiAgbWFyZ2luOiAxNnB4IDA7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgIGhlaWdodDogMjBweDtcclxuICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgfVxyXG59XHJcblxyXG4uc2F2ZWQtdG9rZW4tc2VjdGlvbiB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuXHJcbiAgLnNhdmVkLXRva2VuLWNhcmQge1xyXG4gICAgcGFkZGluZzogMTZweDtcclxuICAgIGJhY2tncm91bmQtY29sb3I6ICNmNWY1ZjU7XHJcbiAgICBib3JkZXItcmFkaXVzOiA4cHg7XHJcbiAgICBib3JkZXI6IDJweCBzb2xpZCB0cmFuc3BhcmVudDtcclxuICAgIHRyYW5zaXRpb246IGJvcmRlci1jb2xvciAwLjJzLCBiYWNrZ3JvdW5kLWNvbG9yIDAuMnM7XHJcblxyXG4gICAgJi5zZWxlY3RlZCB7XHJcbiAgICAgIGJvcmRlci1jb2xvcjogIzE5NzZkMjtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogI2UzZjJmZDtcclxuICAgIH1cclxuXHJcbiAgICAuc2F2ZWQtdG9rZW4taGVhZGVyIHtcclxuICAgICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG5cclxuICAgICAgbWF0LWNoZWNrYm94IHtcclxuICAgICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICB9XHJcbiAgICB9XHJcblxyXG4gICAgLnNhdmVkLXRva2VuLWluZm8ge1xyXG4gICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICBnYXA6IDEycHg7XHJcbiAgICAgIHBhZGRpbmc6IDEycHg7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMjU1LCAyNTUsIDI1NSwgMC43KTtcclxuICAgICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gICAgICBtYXJnaW4tdG9wOiA4cHg7XHJcblxyXG4gICAgICA+IG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDI0cHg7XHJcbiAgICAgICAgaGVpZ2h0OiAyNHB4O1xyXG4gICAgICAgIHdpZHRoOiAyNHB4O1xyXG5cclxuICAgICAgICAmLnZhbGlkIHtcclxuICAgICAgICAgIGNvbG9yOiAjNGNhZjUwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgJi5pbnZhbGlkIHtcclxuICAgICAgICAgIGNvbG9yOiAjZjQ0MzM2O1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgLnRva2VuLWRldGFpbHMge1xyXG4gICAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgICAgICBnYXA6IDRweDtcclxuXHJcbiAgICAgICAgLnVzZXJuYW1lIHtcclxuICAgICAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICAgICAgZ2FwOiA0cHg7XHJcbiAgICAgICAgICBmb250LXNpemU6IDFlbTtcclxuXHJcbiAgICAgICAgICBtYXQtaWNvbiB7XHJcbiAgICAgICAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgICAgICB3aWR0aDogMThweDtcclxuICAgICAgICAgICAgY29sb3I6ICM2NjY7XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAuc3RhdHVzIHtcclxuICAgICAgICAgIGZvbnQtc2l6ZTogMC44NWVtO1xyXG5cclxuICAgICAgICAgICYudmFsaWQge1xyXG4gICAgICAgICAgICBjb2xvcjogIzRjYWY1MDtcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAmLmludmFsaWQge1xyXG4gICAgICAgICAgICBjb2xvcjogI2Y0NDMzNjtcclxuICAgICAgICAgIH1cclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAuc2F2ZWQtdG9rZW4td2FybmluZyB7XHJcbiAgICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICAgIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG4gICAgICBnYXA6IDhweDtcclxuICAgICAgcGFkZGluZzogOHB4IDEycHg7XHJcbiAgICAgIGJhY2tncm91bmQtY29sb3I6ICNmZmYzZTA7XHJcbiAgICAgIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICAgICAgbWFyZ2luLXRvcDogMTJweDtcclxuXHJcbiAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgICB9XHJcblxyXG4gICAgICBzcGFuIHtcclxuICAgICAgICBmb250LXNpemU6IDAuODVlbTtcclxuICAgICAgICBjb2xvcjogIzVkNDAzNztcclxuICAgICAgICBsaW5lLWhlaWdodDogMS40O1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAub3ItZGl2aWRlciB7XHJcbiAgICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgICBtYXJnaW46IDE2cHggMDtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgZm9udC1zaXplOiAwLjllbTtcclxuXHJcbiAgICBzcGFuIHtcclxuICAgICAgYmFja2dyb3VuZC1jb2xvcjogd2hpdGU7XHJcbiAgICAgIHBhZGRpbmc6IDAgMTJweDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5zYXZlZC1hY2NvdW50cy1zZWN0aW9uIHtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG5cclxuICAuYWNjb3VudC1yZXBvLWhpbnQge1xyXG4gICAgZm9udC1zaXplOiAwLjhlbTtcclxuICAgIGNvbG9yOiAjNjY2O1xyXG4gICAgbWFyZ2luLWxlZnQ6IDRweDtcclxuICB9XHJcblxyXG4gIG1hdC1vcHRpb24gbWF0LWljb24ge1xyXG4gICAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcbiAgICBmb250LXNpemU6IDIwcHg7XHJcbiAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICB3aWR0aDogMjBweDtcclxuICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgfVxyXG59XHJcblxyXG4udmFsaWRhdGUtYnV0dG9uIHtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBtYXJnaW4tcmlnaHQ6IDhweDtcclxuICB9XHJcbn1cclxuXHJcbi52YWxpZGF0aW9uLXJlc3VsdCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIHBhZGRpbmc6IDEycHg7XHJcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcblxyXG4gICYuc3VjY2VzcyB7XHJcbiAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZThmNWU5O1xyXG4gICAgY29sb3I6ICMyZTdkMzI7XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBjb2xvcjogIzRjYWY1MDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gICYud2FybmluZyB7XHJcbiAgICBiYWNrZ3JvdW5kLWNvbG9yOiAjZmZmM2UwO1xyXG4gICAgY29sb3I6ICNlNjUxMDA7XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBjb2xvcjogI2ZmOTgwMDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gICYuZXJyb3Ige1xyXG4gICAgYmFja2dyb3VuZC1jb2xvcjogI2ZmZWJlZTtcclxuICAgIGNvbG9yOiAjYzYyODI4O1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgY29sb3I6ICNmNDQzMzY7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4ub3B0aW9ucyB7XHJcbiAgbWFyZ2luOiAyMHB4IDA7XHJcblxyXG4gIG1hdC1jaGVja2JveCB7XHJcbiAgICBkaXNwbGF5OiBibG9jaztcclxuICAgIG1hcmdpbi1ib3R0b206IDEycHg7XHJcbiAgfVxyXG59XHJcblxyXG4uYWR2YW5jZWQtcGFuZWwge1xyXG4gIG1hcmdpbjogMTZweCAwO1xyXG4gIGJveC1zaGFkb3c6IG5vbmUgIWltcG9ydGFudDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICBtYXQtcGFuZWwtdGl0bGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIGZvbnQtc2l6ZTogMC45NWVtO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmFkdmFuY2VkLWNvbnRlbnQge1xyXG4gIHBhZGRpbmctdG9wOiAxNnB4O1xyXG59XHJcblxyXG4uYXV0aC1tZXRob2Qtc2VjdGlvbiB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjBweDtcclxuXHJcbiAgLmZpZWxkLWxhYmVsIHtcclxuICAgIGRpc3BsYXk6IGJsb2NrO1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMTJweDtcclxuICAgIGZvbnQtc2l6ZTogMC45ZW07XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICB9XHJcblxyXG4gIG1hdC1yYWRpby1ncm91cCB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGdhcDogOHB4O1xyXG4gIH1cclxuXHJcbiAgbWF0LXJhZGlvLWJ1dHRvbiB7XHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xyXG4gICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIHZlcnRpY2FsLWFsaWduOiBtaWRkbGU7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4uYXV0by1jcmVhdGUtc2VjdGlvbiB7XHJcbiAgbWFyZ2luLXRvcDogMTZweDtcclxuXHJcbiAgbWF0LWRpdmlkZXIge1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcclxuICB9XHJcblxyXG4gIGg0IHtcclxuICAgIGZvbnQtc2l6ZTogMC45NWVtO1xyXG4gICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICAgIGNvbG9yOiAjNDI0MjQyO1xyXG4gICAgbWFyZ2luOiAwIDAgMTJweCAwO1xyXG4gIH1cclxufVxyXG5cclxuLmNyZWF0ZS1yZXBvLW9wdGlvbnMge1xyXG4gIG1hcmdpbi10b3A6IDEycHg7XHJcbiAgcGFkZGluZy1sZWZ0OiAyOHB4O1xyXG59XHJcblxyXG4udmlzaWJpbGl0eS1vcHRpb24ge1xyXG4gIG1hcmdpbjogMTJweCAwIDE2cHggMDtcclxuXHJcbiAgbWF0LXJhZGlvLWdyb3VwIHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBnYXA6IDI0cHg7XHJcbiAgfVxyXG5cclxuICBtYXQtcmFkaW8tYnV0dG9uIHtcclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgbWFyZ2luLXJpZ2h0OiA0cHg7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICB3aWR0aDogMThweDtcclxuICAgICAgdmVydGljYWwtYWxpZ246IG1pZGRsZTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5lcnJvci1tZXNzYWdlIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kLWNvbG9yOiAjZmZlYmVlO1xyXG4gIGJvcmRlci1yYWRpdXM6IDRweDtcclxuICBtYXJnaW4tdG9wOiAxNnB4O1xyXG4gIGNvbG9yOiAjYzYyODI4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBjb2xvcjogI2M2MjgyODtcclxuICB9XHJcbn1cclxuXHJcbi5oZWxwLXBhbmVsIHtcclxuICBtYXJnaW4tdG9wOiAxNnB4O1xyXG4gIGJveC1zaGFkb3c6IG5vbmUgIWltcG9ydGFudDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICBtYXQtcGFuZWwtdGl0bGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDhweDtcclxuICAgIGZvbnQtc2l6ZTogMC45NWVtO1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmhlbHAtY29udGVudCB7XHJcbiAgcGFkZGluZy10b3A6IDhweDtcclxuXHJcbiAgaDQge1xyXG4gICAgZm9udC1zaXplOiAwLjk1ZW07XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgY29sb3I6ICM0MjQyNDI7XHJcbiAgICBtYXJnaW46IDE2cHggMCA4cHggMDtcclxuXHJcbiAgICAmOmZpcnN0LWNoaWxkIHtcclxuICAgICAgbWFyZ2luLXRvcDogMDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgICBmb250LXNpemU6IDAuOWVtO1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuNTtcclxuICB9XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctY29udGVudCB7XHJcbiAgcGFkZGluZzogMCAyNHB4ICFpbXBvcnRhbnQ7XHJcbiAgbWF4LWhlaWdodDogNzB2aDtcclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nOiAxNnB4IDI0cHggIWltcG9ydGFudDtcclxuXHJcbiAgbWF0LXNwaW5uZXIge1xyXG4gICAgdmVydGljYWwtYWxpZ246IG1pZGRsZTtcclxuICB9XHJcbn1cclxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
    });
  }
}

/***/ }),

/***/ 1312:
/*!***********************************!*\
  !*** ./src/app/git/git.module.ts ***!
  \***********************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitModule": () => (/* binding */ GitModule)
/* harmony export */ });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./components/git-messages/git-messages.component */ 2055);
/* harmony import */ var _dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./dialogs/commit-message-dialog/commit-message-dialog.component */ 4386);
/* harmony import */ var _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/git-history-dialog/git-history-dialog.component */ 9497);
/* harmony import */ var _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/git-branch-dialog/git-branch-dialog.component */ 6235);
/* harmony import */ var _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/git-setup-remote-dialog/git-setup-remote-dialog.component */ 3992);
/* harmony import */ var _dialogs_git_setup_remote_generic_dialog_git_setup_remote_generic_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/git-setup-remote-generic-dialog/git-setup-remote-generic-dialog.component */ 8995);
/* harmony import */ var _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/git-token-dialog/git-token-dialog.component */ 3701);
/* harmony import */ var _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./dialogs/git-account-management-dialog/git-account-management-dialog.component */ 3148);
/* harmony import */ var _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./dialogs/git-init-wizard/git-init-wizard-dialog.component */ 5855);
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../shared/material.module */ 4872);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/core */ 2560);














class GitModule {
  static {
    this.ɵfac = function GitModule_Factory(t) {
      return new (t || GitModule)();
    };
  }
  static {
    this.ɵmod = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineNgModule"]({
      type: GitModule
    });
  }
  static {
    this.ɵinj = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineInjector"]({
      imports: [_angular_common__WEBPACK_IMPORTED_MODULE_11__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_9__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_12__.FormsModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_13__.TranslateModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵsetNgModuleScope"](GitModule, {
    declarations: [_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_0__.GitMessagesComponent, _dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_1__.CommitMessageDialogComponent, _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_2__.GitHistoryDialogComponent, _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_3__.GitBranchDialogComponent, _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_4__.GitSetupRemoteDialogComponent, _dialogs_git_setup_remote_generic_dialog_git_setup_remote_generic_dialog_component__WEBPACK_IMPORTED_MODULE_5__.GitSetupRemoteGenericDialogComponent, _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__.GitTokenDialogComponent, _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__.GitAccountManagementDialogComponent, _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__.GitInitWizardDialogComponent],
    imports: [_angular_common__WEBPACK_IMPORTED_MODULE_11__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_9__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_12__.FormsModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_13__.TranslateModule],
    exports: [_dialogs_commit_message_dialog_commit_message_dialog_component__WEBPACK_IMPORTED_MODULE_1__.CommitMessageDialogComponent, _dialogs_git_history_dialog_git_history_dialog_component__WEBPACK_IMPORTED_MODULE_2__.GitHistoryDialogComponent, _dialogs_git_branch_dialog_git_branch_dialog_component__WEBPACK_IMPORTED_MODULE_3__.GitBranchDialogComponent, _dialogs_git_setup_remote_dialog_git_setup_remote_dialog_component__WEBPACK_IMPORTED_MODULE_4__.GitSetupRemoteDialogComponent, _dialogs_git_setup_remote_generic_dialog_git_setup_remote_generic_dialog_component__WEBPACK_IMPORTED_MODULE_5__.GitSetupRemoteGenericDialogComponent, _dialogs_git_token_dialog_git_token_dialog_component__WEBPACK_IMPORTED_MODULE_6__.GitTokenDialogComponent, _dialogs_git_account_management_dialog_git_account_management_dialog_component__WEBPACK_IMPORTED_MODULE_7__.GitAccountManagementDialogComponent, _dialogs_git_init_wizard_git_init_wizard_dialog_component__WEBPACK_IMPORTED_MODULE_8__.GitInitWizardDialogComponent]
  });
})();

/***/ }),

/***/ 3601:
/*!***********************************************!*\
  !*** ./src/app/git/models/git-init.models.ts ***!
  \***********************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GITIGNORE_TEMPLATES": () => (/* binding */ GITIGNORE_TEMPLATES)
/* harmony export */ });
/**
 * Available .gitignore templates
 */
const GITIGNORE_TEMPLATES = [{
  value: 'mdexplorer',
  label: 'MdExplorer (Default)',
  description: 'Ignores .md/, database files, logs, and temporary files'
}, {
  value: 'node',
  label: 'Node.js',
  description: 'Ignores node_modules/, npm logs, build outputs, and .env files'
}, {
  value: 'python',
  label: 'Python',
  description: 'Ignores __pycache__/, virtual environments, and distribution files'
}, {
  value: 'csharp',
  label: '.NET / C#',
  description: 'Ignores bin/, obj/, Visual Studio files, and build outputs'
}, {
  value: 'none',
  label: 'None',
  description: 'Creates an empty .gitignore file'
}];

/***/ }),

/***/ 3204:
/*!***************************************************!*\
  !*** ./src/app/git/models/remote-setup.models.ts ***!
  \***************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PROVIDER_INFO": () => (/* binding */ PROVIDER_INFO)
/* harmony export */ });
/**
 * Provider display configurations
 */
const PROVIDER_INFO = {
  github: {
    id: 'github',
    name: 'GitHub',
    icon: 'code',
    color: '#24292e'
  },
  gitlab: {
    id: 'gitlab',
    name: 'GitLab',
    icon: 'code',
    color: '#fc6d26'
  },
  bitbucket: {
    id: 'bitbucket',
    name: 'Bitbucket',
    icon: 'cloud',
    color: '#0052cc'
  },
  gitea: {
    id: 'gitea',
    name: 'Gitea',
    icon: 'pets',
    color: '#609926'
  },
  azure: {
    id: 'azure',
    name: 'Azure DevOps',
    icon: 'cloud_queue',
    color: '#0078d4'
  },
  generic: {
    id: 'generic',
    name: 'Git',
    icon: 'source',
    color: '#666666'
  }
};

/***/ }),

/***/ 7360:
/*!*****************************************************!*\
  !*** ./src/app/git/services/git-account.service.ts ***!
  \*****************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitAccountService": () => (/* binding */ GitAccountService)
/* harmony export */ });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ 635);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);




class GitAccountService {
  constructor(http) {
    this.http = http;
    this.API_BASE = '/api/gitaccount';
  }
  /**
   * Gets the Git account configuration for a specific repository
   */
  getAccountForRepository(repositoryPath) {
    const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__.HttpParams().set('repositoryPath', repositoryPath);
    return this.http.get(`${this.API_BASE}/for-repository`, {
      params
    }).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.map)(account => {
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
    return this.http.get(this.API_BASE).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.map)(accounts => accounts.map(account => {
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
    return this.http.post(this.API_BASE, request).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.map)(account => {
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
    return this.http.put(`${this.API_BASE}/${id}`, request).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.map)(account => {
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
    const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__.HttpParams().set('repositoryPath', repositoryPath);
    return this.http.get(`${this.API_BASE}/exists`, {
      params
    }).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.map)(response => response.exists));
  }
  static {
    this.ɵfac = function GitAccountService_Factory(t) {
      return new (t || GitAccountService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__.HttpClient));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({
      token: GitAccountService,
      factory: GitAccountService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 7554:
/*!********************************************************!*\
  !*** ./src/app/git/services/git-credential.service.ts ***!
  \********************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "GitCredentialService": () => (/* binding */ GitCredentialService)
/* harmony export */ });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs/operators */ 635);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);




class GitCredentialService {
  constructor(http) {
    this.http = http;
    this.API_BASE = '/api/gitaccount/credentials';
  }
  /**
   * Gets all Git credentials
   */
  getAllCredentials() {
    return this.http.get(this.API_BASE).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_0__.map)(credentials => credentials.map(credential => {
      // Convert dates from strings to Date objects
      if (credential.createdAt) {
        credential.createdAt = new Date(credential.createdAt);
      }
      if (credential.updatedAt) {
        credential.updatedAt = new Date(credential.updatedAt);
      }
      return credential;
    })));
  }
  /**
   * Gets a specific Git credential by ID
   */
  getCredential(id) {
    return this.http.get(`${this.API_BASE}/${id}`).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_0__.map)(credential => {
      if (credential.createdAt) {
        credential.createdAt = new Date(credential.createdAt);
      }
      if (credential.updatedAt) {
        credential.updatedAt = new Date(credential.updatedAt);
      }
      return credential;
    }));
  }
  /**
   * Gets credentials by account type (GitHub, GitLab, etc.)
   */
  getCredentialsByType(accountType) {
    const params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_1__.HttpParams().set('accountType', accountType);
    return this.http.get('/api/gitaccount/usernames-by-type', {
      params
    });
  }
  /**
   * Creates a new Git credential
   */
  createCredential(request) {
    return this.http.post(this.API_BASE, request).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_0__.map)(credential => {
      if (credential.createdAt) {
        credential.createdAt = new Date(credential.createdAt);
      }
      if (credential.updatedAt) {
        credential.updatedAt = new Date(credential.updatedAt);
      }
      return credential;
    }));
  }
  /**
   * Updates an existing Git credential
   */
  updateCredential(id, request) {
    return this.http.put(`${this.API_BASE}/${id}`, request).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_0__.map)(credential => {
      if (credential.createdAt) {
        credential.createdAt = new Date(credential.createdAt);
      }
      if (credential.updatedAt) {
        credential.updatedAt = new Date(credential.updatedAt);
      }
      return credential;
    }));
  }
  /**
   * Deletes a Git credential
   */
  deleteCredential(id) {
    return this.http.delete(`${this.API_BASE}/${id}`);
  }
  static {
    this.ɵfac = function GitCredentialService_Factory(t) {
      return new (t || GitCredentialService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_1__.HttpClient));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({
      token: GitCredentialService,
      factory: GitCredentialService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 9595:
/*!***************************************************************!*\
  !*** ./src/app/md-explorer/services/external-apps.service.ts ***!
  \***************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ExternalAppsService": () => (/* binding */ ExternalAppsService)
/* harmony export */ });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ 8635);



class ExternalAppsService {
  constructor(http, serverMessages) {
    this.http = http;
    this.serverMessages = serverMessages;
  }
  get connectionId() {
    return this.serverMessages.connectionId || '';
  }
  getApps() {
    return this.http.get(`/api/MdExternalApps?ConnectionId=${this.connectionId}`);
  }
  addApp(app, projectPath) {
    const params = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.post(`/api/MdExternalApps/Add`, app, {
      params
    });
  }
  deleteApp(id, projectPath) {
    const params = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.delete(`/api/MdExternalApps/${encodeURIComponent(id)}`, {
      params
    });
  }
  getConfig(projectPath) {
    const params = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.get(`/api/MdExternalApps/config`, {
      params
    });
  }
  saveTree(tree, projectPath) {
    const params = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.put(`/api/MdExternalApps/tree`, tree, {
      params
    });
  }
  saveConfig(config, projectPath) {
    const params = {};
    if (projectPath) params.projectPath = projectPath;
    return this.http.put(`/api/MdExternalApps/config`, config, {
      params
    });
  }
  static {
    this.ɵfac = function ExternalAppsService_Factory(t) {
      return new (t || ExternalAppsService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_2__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_0__.MdServerMessagesService));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({
      token: ExternalAppsService,
      factory: ExternalAppsService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 5389:
/*!***************************************************************!*\
  !*** ./src/app/projects/new-project/new-project.component.ts ***!
  \***************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "DynamicDatabase": () => (/* binding */ DynamicDatabase),
/* harmony export */   "NewProjectComponent": () => (/* binding */ NewProjectComponent)
/* harmony export */ });
/* harmony import */ var _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/cdk/tree */ 5183);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs */ 6317);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! rxjs */ 6646);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! rxjs/operators */ 635);
/* harmony import */ var _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../md-explorer/models/md-file */ 1115);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/tree */ 3453);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/icon */ 7822);













function NewProjectComponent_mat_tree_node_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-tree-node", 8)(1, "button", 9)(2, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "a", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_5_Template_a_click_4_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4);
      const node_r2 = restoredCtx.$implicit;
      const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      ctx_r3.getFolder(node_r2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r3.activeNode = node_r2);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const node_r2 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](node_r2.name);
  }
}
function NewProjectComponent_mat_tree_node_6_Template(rf, ctx) {
  if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-tree-node", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_6_Template_mat_tree_node_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r7);
      const node_r5 = restoredCtx.$implicit;
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r6.activeNode = node_r5);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](1, "button", 13)(2, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](4, "a", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function NewProjectComponent_mat_tree_node_6_Template_a_click_4_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r7);
      const node_r5 = restoredCtx.$implicit;
      const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r8.getFolder(node_r5));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const node_r5 = ctx.$implicit;
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵattribute"]("aria-label", "Toggle " + node_r5.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate1"](" ", ctx_r1.treeControl.isExpanded(node_r5) ? "folder_open" : "folder", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](node_r5.name);
  }
}
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
    var md1 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__.MdFile('12Folder', 'c:primoFolder', 0, true);
    var md2 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__.MdFile('2Folder', 'c:primoFoldersottoFolder', 1, true);
    var md3 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__.MdFile('3Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
    var md4 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__.MdFile('4Folder', 'c:primoFoldersottoFoldersottoFolder', 2, true);
    var md5 = new _md_explorer_models_md_file__WEBPACK_IMPORTED_MODULE_0__.MdFile('5Folder', 'c:cuccu', 3, false);
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
  static {
    this.ɵfac = function DynamicDatabase_Factory(t) {
      return new (t || DynamicDatabase)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__.MdFileService));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({
      token: DynamicDatabase,
      factory: DynamicDatabase.ɵfac,
      providedIn: 'root'
    });
  }
}
class DynamicDataSource {
  get data() {
    return this.dataChange.value;
  }
  set data(value) {
    this._treeControl.dataNodes = value;
    this.dataChange.next(value);
  }
  constructor(_treeControl, _database, _mdFileService) {
    this._treeControl = _treeControl;
    this._database = _database;
    this._mdFileService = _mdFileService;
    this.dataChange = new rxjs__WEBPACK_IMPORTED_MODULE_4__.BehaviorSubject([]);
    this.data = _database.initialData();
    this._mdFileService.loadDocumentFolder('root', 0, "Folders").subscribe(_ => {
      this.data = _;
    });
    //this.dataChange = _mdFileService._mdDynFolderDocument;
    //_mdFileService.loadDynFolders('root', 1);
  }
  connect(collectionViewer) {
    this._treeControl.expansionModel.changed.subscribe(change => {
      if (change.added || change.removed) {
        this.handleTreeControl(change);
      }
    });
    return (0,rxjs__WEBPACK_IMPORTED_MODULE_5__.merge)(collectionViewer.viewChange, this.dataChange).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_6__.map)(() => this.data));
  }
  disconnect(collectionViewer) {}
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
      if (!children || index < 0) {
        // If no children, or cannot find the node, no op
        return;
      }
      node.isLoading = true;
      setTimeout(() => {
        if (expand) {
          const nodes = children; // punto per fare chiamata remota
          this.data.splice(index + 1, 0, ...nodes);
        } else {
          let count = 0;
          for (let i = index + 1; i < this.data.length && this.data[i].level > node.level; i++, count++) {}
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
    this.getLevel = node => node.level;
    this.isExpandable = node => node.expandable;
    this.hasChild = (_, _nodeData) => _nodeData.expandable;
    this.treeControl = new _angular_cdk_tree__WEBPACK_IMPORTED_MODULE_7__.FlatTreeControl(this.getLevel, this.isExpandable);
    this.dataSource = new DynamicDataSource(this.treeControl, database, mdFileService);
  }
  ngOnInit() {
    this.folder = {
      name: "<select project>",
      path: ""
    };
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
  static {
    this.ɵfac = function NewProjectComponent_Factory(t) {
      return new (t || NewProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](DynamicDatabase), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_1__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_8__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogRef));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({
      type: NewProjectComponent,
      selectors: [["app-new-project"]],
      decls: 16,
      vars: 5,
      consts: [[1, "flex-container"], [1, "flex-items-overflow"], [3, "dataSource", "treeControl"], ["matTreeNodePadding", "", 4, "matTreeNodeDef"], ["matTreeNodePadding", "", 3, "click", 4, "matTreeNodeDef", "matTreeNodeDefWhen"], [1, "flex-selected-folder"], [1, "flex-items"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["matTreeNodePadding", ""], ["mat-icon-button", ""], [1, "mat-icon-rtl-mirror"], [2, "cursor", "pointer", 3, "click"], ["matTreeNodePadding", "", 3, "click"], ["mat-icon-button", "", "matTreeNodeToggle", ""]],
      template: function NewProjectComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "div", 0)(1, "h1");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2, "Document's Folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "div", 1)(4, "mat-tree", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](5, NewProjectComponent_mat_tree_node_5_Template, 6, 1, "mat-tree-node", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](6, NewProjectComponent_mat_tree_node_6_Template, 6, 3, "mat-tree-node", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](7, "div", 5)(8, "div", 6)(9, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "mat-card-subtitle");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](13, "div")(14, "button", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function NewProjectComponent_Template_button_click_14_listener() {
            return ctx.closeDialog();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](15, "Open");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("dataSource", ctx.dataSource)("treeControl", ctx.treeControl);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("matTreeNodeDefWhen", ctx.hasChild);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx.folder.name);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](ctx.folder.path);
        }
      },
      dependencies: [_angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__.MatLegacyCardTitle, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_10__.MatLegacyCardSubtitle, _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__.MatTreeNodeDef, _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__.MatTreeNodePadding, _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__.MatTreeNodeToggle, _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__.MatTree, _angular_material_tree__WEBPACK_IMPORTED_MODULE_11__.MatTreeNode, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_12__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_13__.MatIcon],
      styles: [".flex-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.flex-items-overflow[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-x: scroll;\n  overflow-y: scroll;\n  background: tomato;\n  color: white;\n  text-align: center;\n  font-size: 3em;\n  flex: 1;\n}\n\n.flex-items[_ngcontent-%COMP%] {\n  width: 500px;\n  text-align: center;\n  font-size: 3em;\n}\n\n.flex-selected-folder[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvbmV3LXByb2plY3QvbmV3LXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7QUFDRjs7QUFFQTtFQUNFLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7QUFDRjs7QUFFQTtFQUNFLFlBQUE7RUFFQSxrQkFBQTtFQUNBLGNBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtBQUFGIiwic291cmNlc0NvbnRlbnQiOlsiLmZsZXgtY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcbi5mbGV4LWl0ZW1zLW92ZXJmbG93IHtcclxuICBtYXgtaGVpZ2h0OiA0MDBweDtcclxuICBvdmVyZmxvdy14OiBzY3JvbGw7XHJcbiAgb3ZlcmZsb3cteTogc2Nyb2xsO1xyXG4gIGJhY2tncm91bmQ6IHRvbWF0bztcclxuICBjb2xvcjogd2hpdGU7XHJcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xyXG4gIGZvbnQtc2l6ZTogM2VtO1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5mbGV4LWl0ZW1zIHtcclxuICB3aWR0aDo1MDBweDtcclxuICBcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgZm9udC1zaXplOiAzZW07XHJcbn1cclxuXHJcbi5mbGV4LXNlbGVjdGVkLWZvbGRlciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogcm93O1xyXG59XHJcbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common/http */ 8987);


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
    return this.http.post(url, {
      enabled
    });
  }
  getStickyScrollSetting() {
    return this.http.get('../api/ProjectSettings/GetStickyScrollSetting');
  }
  setStickyScrollSetting(enabled) {
    return this.http.post('../api/ProjectSettings/SetStickyScrollSetting', {
      enabled
    });
  }
  getLinkIndexingSetting(projectPath) {
    const url = '../api/ProjectSettings/GetLinkIndexingSetting';
    return this.http.get(url, {
      params: {
        projectPath
      }
    });
  }
  setLinkIndexingSetting(enabled, projectPath) {
    const url = '../api/ProjectSettings/SetLinkIndexingSetting';
    return this.http.post(url, {
      enabled,
      projectPath
    });
  }
  getPlantUmlKeepOriginalColorsSetting(projectPath) {
    const url = '../api/ProjectSettings/GetPlantUmlKeepOriginalColorsSetting';
    return this.http.get(url, {
      params: {
        projectPath
      }
    });
  }
  setPlantUmlKeepOriginalColorsSetting(enabled, projectPath) {
    const url = '../api/ProjectSettings/SetPlantUmlKeepOriginalColorsSetting';
    return this.http.post(url, {
      enabled,
      projectPath
    });
  }
  getCopilotCliAutoSelectSetting(projectPath) {
    const url = '../api/ProjectSettings/GetCopilotCliAutoSelectSetting';
    return this.http.get(url, {
      params: {
        projectPath
      }
    });
  }
  setCopilotCliAutoSelectSetting(enabled, projectPath) {
    const url = '../api/ProjectSettings/SetCopilotCliAutoSelectSetting';
    return this.http.post(url, {
      enabled,
      projectPath
    });
  }
  // RAG Settings
  getRagStatus() {
    const url = '../api/Rag/status';
    return this.http.get(url);
  }
  enableRag() {
    const url = '../api/Rag/enable';
    return this.http.post(url, {});
  }
  disableRag() {
    const url = '../api/Rag/disable';
    return this.http.post(url, {});
  }
  reindexRag(projectPath) {
    const url = '../api/Rag/reindex';
    return this.http.post(url, {
      projectPath
    });
  }
  clearRagIndex(projectPath) {
    const url = '../api/Rag/clear';
    return this.http.post(url, {
      projectPath
    });
  }
  indexRagFile(filePath, projectPath, forceReindex = false) {
    const url = '../api/Rag/index-file';
    return this.http.post(url, {
      filePath,
      projectPath,
      forceReindex
    });
  }
  indexRagDirectory(directoryPath, projectPath, forceReindex = false) {
    const url = '../api/Rag/index-directory';
    return this.http.post(url, {
      directoryPath,
      projectPath,
      forceReindex
    });
  }
  // ============================================================
  //   Knowledge Graph (Neo4j) settings + sync
  // ============================================================
  getKgSettings(projectId) {
    return this.http.get(`../api/kg/settings/${encodeURIComponent(projectId)}`);
  }
  saveKgSettings(projectId, body) {
    return this.http.put(`../api/kg/settings/${encodeURIComponent(projectId)}`, body);
  }
  testKgConnection(body) {
    return this.http.post('../api/kg/test-connection', body);
  }
  syncKgProject(projectId) {
    return this.http.post('../api/kg/ingest/project', {
      projectId
    });
  }
  syncKgFolder(projectId, relativeFolderPath) {
    return this.http.post('../api/kg/ingest/folder', {
      projectId,
      relativeFolderPath
    });
  }
  syncKgFile(projectId, relativeKgPath) {
    return this.http.post('../api/kg/ingest/file', {
      projectId,
      relativeKgPath
    });
  }
  resetKg(projectId) {
    return this.http.post('../api/kg/reset', {
      projectId,
      confirm: true
    });
  }
  getKgState(projectId) {
    return this.http.get(`../api/kg/state/${encodeURIComponent(projectId)}`);
  }
  // ============================================================
  //   Apache Jena Fuseki settings (parallelo a KG/Neo4j)
  // ============================================================
  getFusekiSettings(projectId) {
    return this.http.get(`../api/fs/settings/${encodeURIComponent(projectId)}`);
  }
  saveFusekiSettings(projectId, body) {
    return this.http.put(`../api/fs/settings/${encodeURIComponent(projectId)}`, body);
  }
  testFusekiConnection(body) {
    return this.http.post('../api/fs/test-connection', body);
  }
  ensureFusekiDataset(projectId) {
    return this.http.post('../api/fs/ensure-dataset', {
      projectId
    });
  }
  // ============================================================
  //   Atlassian (Jira/Confluence) settings
  //   Shared config (base url, project keys, planning folder) -> .development.yml
  //   Personal token -> UserDB (encrypted). Both saved in one PUT.
  // ============================================================
  getAtlassianSettings(projectId) {
    return this.http.get(`../api/atlassian/settings/${encodeURIComponent(projectId)}`);
  }
  saveAtlassianSettings(projectId, body) {
    return this.http.put(`../api/atlassian/settings/${encodeURIComponent(projectId)}`, body);
  }
  testAtlassianConnection(body) {
    return this.http.post('../api/atlassian/test-connection', body);
  }
  static {
    this.ɵfac = function ProjectSettingsService_Factory(t) {
      return new (t || ProjectSettingsService)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_1__.HttpClient));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({
      token: ProjectSettingsService,
      factory: ProjectSettingsService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 9811:
/*!*****************************************!*\
  !*** ./src/app/services/p2p.service.ts ***!
  \*****************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "P2PService": () => (/* binding */ P2PService)
/* harmony export */ });
/* harmony import */ var C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var _microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @microsoft/signalr */ 3509);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs */ 6317);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common/http */ 8987);





class P2PService {
  constructor(http) {
    this.http = http;
    this.hubConnection = null;
    this.baseUrl = '/api/P2P';
    // Observables for state
    this._status$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.BehaviorSubject(null);
    this.status$ = this._status$.asObservable();
    this._transfers$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.BehaviorSubject([]);
    this.transfers$ = this._transfers$.asObservable();
    this._transferProgress$ = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subject();
    this.transferProgress$ = this._transferProgress$.asObservable();
    this._transferComplete$ = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subject();
    this.transferComplete$ = this._transferComplete$.asObservable();
    this._transferError$ = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subject();
    this.transferError$ = this._transferError$.asObservable();
    // New observables for peer activity feedback
    this._peerConnected$ = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subject();
    this.peerConnected$ = this._peerConnected$.asObservable();
    this._uploadActivity$ = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subject();
    this.uploadActivity$ = this._uploadActivity$.asObservable();
    // Aggregated state for UI widget
    this._activeUploads$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.BehaviorSubject(0);
    this.activeUploads$ = this._activeUploads$.asObservable();
    this._totalPeers$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.BehaviorSubject(0);
    this.totalPeers$ = this._totalPeers$.asObservable();
    this._isAvailable$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.BehaviorSubject(false);
    this.isAvailable$ = this._isAvailable$.asObservable();
    // Don't call checkAvailability() here — it fires before SignalR is ready
    // and causes 404 noise if the P2P Premium module is not loaded.
    // It's called explicitly by ProjectsComponent.ngOnInit() instead.
  }
  /**
   * Check if P2P service is available (Premium module loaded and Electron plugin running)
   */
  checkAvailability() {
    this.http.get(`${this.baseUrl}/status`).subscribe({
      next: status => {
        this._status$.next(status);
        this._isAvailable$.next(status?.enabled || false);
        if (status?.enabled) {
          this.initializeSignalR();
          this.refreshTransfers();
          // Auto-restore seeding for all projects with P2P files
          this.autoRestoreAll();
        }
      },
      error: () => {
        this._isAvailable$.next(false);
        this._status$.next(null);
      }
    });
  }
  /**
   * Auto-restore seeding for all projects that have P2P metadata
   * Called automatically when P2P service becomes available
   */
  autoRestoreAll() {
    // First get all projects from MdExplorer
    this.http.get('../api/MdProjects/GetProjects').subscribe({
      next: projects => {
        if (projects && projects.length > 0) {
          // Send to P2P plugin to restore seeding
          this.http.post(`${this.baseUrl}/auto-restore-all`, {
            projects
          }).subscribe({
            next: result => {
              if (result.restored > 0) {
                console.log(`[P2P] Auto-restored ${result.restored} files from ${result.withP2P} projects`);
                this.refreshTransfers();
              }
            },
            error: err => {
              console.error('[P2P] Auto-restore error:', err);
            }
          });
        }
      },
      error: err => {
        console.error('[P2P] Failed to get projects for auto-restore:', err);
      }
    });
  }
  initializeSignalR() {
    if (this.hubConnection) {
      return; // Already initialized
    }
    this.hubConnection = new _microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__.HubConnectionBuilder().withUrl('/signalr/p2p').configureLogging(_microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__.LogLevel.Information).withAutomaticReconnect().build();
    // Setup event handlers
    this.hubConnection.on('TransferProgress', transfer => {
      this._transferProgress$.next(transfer);
      this.updateTransferInList(transfer);
    });
    this.hubConnection.on('TransferComplete', transfer => {
      this._transferComplete$.next(transfer);
      this.updateTransferInList(transfer);
    });
    this.hubConnection.on('TransferError', data => {
      this._transferError$.next(data);
    });
    // Peer connected event - when a remote peer connects to download from us
    this.hubConnection.on('PeerConnected', data => {
      console.log('[P2PService] Peer connected:', data);
      this._peerConnected$.next(data);
      this._totalPeers$.next(data.numPeers);
    });
    // Upload activity event - periodic updates while uploading
    this.hubConnection.on('UploadActivity', data => {
      console.log('[P2PService] Upload activity:', data);
      this._uploadActivity$.next(data);
      this._totalPeers$.next(data.numPeers);
      // Count active uploads (those with speed > 0)
      if (data.uploadSpeed > 0) {
        this._activeUploads$.next(1); // Simplified: at least one active
      }
    });
    // Start connection
    this.startConnection();
  }
  startConnection() {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      if (!_this.hubConnection) return;
      try {
        yield _this.hubConnection.start();
        console.log('[P2PService] SignalR connection established');
        // Subscribe to all transfer updates
        yield _this.hubConnection.invoke('SubscribeToAllTransfers');
      } catch (err) {
        console.error('[P2PService] Error establishing SignalR connection:', err);
        setTimeout(() => _this.startConnection(), 5000);
      }
    })();
  }
  updateTransferInList(transfer) {
    const transfers = this._transfers$.value;
    const index = transfers.findIndex(t => t.infoHash === transfer.infoHash);
    if (index >= 0) {
      transfers[index] = transfer;
    } else {
      transfers.push(transfer);
    }
    this._transfers$.next([...transfers]);
  }
  // API Methods
  /**
   * Get P2P service status
   */
  getStatus() {
    return this.http.get(`${this.baseUrl}/status`);
  }
  /**
   * Get P2P health check
   */
  getHealth() {
    return this.http.get(`${this.baseUrl}/health`);
  }
  /**
   * Get tracker connectivity status
   * Returns information about whether the tracker is reachable and authenticated
   */
  getTrackerStatus() {
    return this.http.get(`${this.baseUrl}/tracker-status`);
  }
  /**
   * Get P2P statistics
   */
  getStats() {
    return this.http.get(`${this.baseUrl}/stats`);
  }
  /**
   * Get all active transfers
   */
  getTransfers() {
    return this.http.get(`${this.baseUrl}/transfers`);
  }
  /**
   * Refresh the transfers list
   */
  refreshTransfers() {
    this.getTransfers().subscribe({
      next: transfers => {
        this._transfers$.next(transfers);
      },
      error: err => {
        console.error('[P2PService] Error fetching transfers:', err);
      }
    });
  }
  /**
   * Get a specific transfer by info hash
   */
  getTransfer(infoHash) {
    return this.http.get(`${this.baseUrl}/transfers/${infoHash}`);
  }
  /**
   * Share a file via P2P
   * @param filePath Full path to the file to share
   * @param name Optional display name
   */
  shareFile(filePath, name) {
    return this.http.post(`${this.baseUrl}/share`, {
      filePath,
      name
    });
  }
  /**
   * Download from a magnet link
   * @param magnetUri Magnet URI to download
   * @param destPath Optional destination path
   */
  download(magnetUri, destPath) {
    return this.http.post(`${this.baseUrl}/download`, {
      magnetUri,
      destPath
    });
  }
  /**
   * Pause a transfer
   */
  pauseTransfer(infoHash) {
    return this.http.post(`${this.baseUrl}/transfers/${infoHash}/pause`, {});
  }
  /**
   * Resume a transfer
   */
  resumeTransfer(infoHash) {
    return this.http.post(`${this.baseUrl}/transfers/${infoHash}/resume`, {});
  }
  /**
   * Stop and remove a transfer
   * @param deleteFiles Whether to delete the downloaded files
   */
  stopTransfer(infoHash, deleteFiles = false) {
    return this.http.delete(`${this.baseUrl}/transfers/${infoHash}?deleteFiles=${deleteFiles}`);
  }
  /**
   * Parse a magnet URI to get info before downloading
   */
  parseMagnet(magnetUri) {
    return this.http.post(`${this.baseUrl}/parse-magnet`, {
      magnetUri
    });
  }
  /**
   * Copy a file to .p2pshare/files/, start seeding it, and append a P2P link to the markdown document.
   * This is the main method for the "Add file to share via P2P" feature.
   * @param sourcePath Full path to the source file to copy and share
   * @param documentPath Full path to the markdown document where the link will be appended
   */
  copyAndShareFile(sourcePath, documentPath) {
    return this.http.post(`${this.baseUrl}/copy-and-share`, {
      sourcePath,
      documentPath
    });
  }
  /**
   * Check if a file exists at a relative path within the project
   * @param path Relative path to check (e.g., ".p2pshare/files/video.mp4")
   * @param projectPath Full path to the project root
   */
  checkFile(path, projectPath) {
    return this.http.get(`${this.baseUrl}/check-file?path=${encodeURIComponent(path)}&projectPath=${encodeURIComponent(projectPath)}`);
  }
  /**
   * Get peer status for a specific torrent by infoHash.
   * Returns number of peers, download/upload speeds, and transfer status.
   * @param infoHash The torrent info hash
   */
  getPeerStatus(infoHash) {
    return this.http.get(`${this.baseUrl}/peer-status/${infoHash}`);
  }
  /**
   * Get P2P metadata for a project.
   * Returns the contents of .p2pshare/metadata.json if it exists.
   * @param projectPath Full path to the project root
   */
  getMetadata(projectPath) {
    return this.http.get(`${this.baseUrl}/metadata?projectPath=${encodeURIComponent(projectPath)}`);
  }
  /**
   * Get P2P info for a specific file by filename.
   * Returns magnetUri, infoHash, size from metadata.json.
   * @param filename The filename to look up
   * @param projectPath Full path to the project root
   */
  getFileInfo(filename, projectPath) {
    return this.http.get(`${this.baseUrl}/file-info/${encodeURIComponent(filename)}?projectPath=${encodeURIComponent(projectPath)}`);
  }
  /**
   * Get all projects that have P2P sharing enabled, along with their metadata.
   * Accepts a list of projects and returns only those that have .p2pshare/metadata.json.
   * Used by the P2P Manager to show files organized by project.
   * @param projects List of projects with id, name, and path
   */
  getProjectsWithP2P(projects) {
    return this.http.post(`${this.baseUrl}/projects-with-p2p`, {
      projects
    });
  }
  /**
   * Restore seeding for all files in a project's metadata.json.
   * Called when opening a project to resume P2P sharing.
   * @param projectPath Full path to the project root
   */
  restoreSeeding(projectPath) {
    return this.http.post(`${this.baseUrl}/restore-seeding`, {
      projectPath
    });
  }
  /**
   * Format bytes to human readable string
   */
  formatBytes(bytes, decimals = 2) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  }
  /**
   * Format speed to human readable string
   */
  formatSpeed(bytesPerSecond) {
    return this.formatBytes(bytesPerSecond) + '/s';
  }
  /**
   * Format ETA to human readable string
   */
  formatEta(seconds) {
    if (seconds <= 0 || !isFinite(seconds)) return '--';
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor(seconds % 3600 / 60);
    const secs = Math.floor(seconds % 60);
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    } else if (minutes > 0) {
      return `${minutes}m ${secs}s`;
    } else {
      return `${secs}s`;
    }
  }
  ngOnDestroy() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
  static {
    this.ɵfac = function P2PService_Factory(t) {
      return new (t || P2PService)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_5__.HttpClient));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineInjectable"]({
      token: P2PService,
      factory: P2PService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 9862:
/*!*************************************************************************************!*\
  !*** ./node_modules/angular-animations/__ivy_ngcc__/fesm2015/angular-animations.js ***!
  \*************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "animateChildrenOnLeaveAnimation": () => (/* binding */ animateChildrenOnLeaveAnimation),
/* harmony export */   "bounceAnimation": () => (/* binding */ bounceAnimation),
/* harmony export */   "bounceInAnimation": () => (/* binding */ bounceInAnimation),
/* harmony export */   "bounceInDownAnimation": () => (/* binding */ bounceInDownAnimation),
/* harmony export */   "bounceInDownOnEnterAnimation": () => (/* binding */ bounceInDownOnEnterAnimation),
/* harmony export */   "bounceInLeftAnimation": () => (/* binding */ bounceInLeftAnimation),
/* harmony export */   "bounceInLeftOnEnterAnimation": () => (/* binding */ bounceInLeftOnEnterAnimation),
/* harmony export */   "bounceInOnEnterAnimation": () => (/* binding */ bounceInOnEnterAnimation),
/* harmony export */   "bounceInRightAnimation": () => (/* binding */ bounceInRightAnimation),
/* harmony export */   "bounceInRightOnEnterAnimation": () => (/* binding */ bounceInRightOnEnterAnimation),
/* harmony export */   "bounceInUpAnimation": () => (/* binding */ bounceInUpAnimation),
/* harmony export */   "bounceInUpOnEnterAnimation": () => (/* binding */ bounceInUpOnEnterAnimation),
/* harmony export */   "bounceOnEnterAnimation": () => (/* binding */ bounceOnEnterAnimation),
/* harmony export */   "bounceOutAnimation": () => (/* binding */ bounceOutAnimation),
/* harmony export */   "bounceOutDownAnimation": () => (/* binding */ bounceOutDownAnimation),
/* harmony export */   "bounceOutDownOnLeaveAnimation": () => (/* binding */ bounceOutDownOnLeaveAnimation),
/* harmony export */   "bounceOutLeftAnimation": () => (/* binding */ bounceOutLeftAnimation),
/* harmony export */   "bounceOutLeftOnLeaveAnimation": () => (/* binding */ bounceOutLeftOnLeaveAnimation),
/* harmony export */   "bounceOutOnLeaveAnimation": () => (/* binding */ bounceOutOnLeaveAnimation),
/* harmony export */   "bounceOutRightAnimation": () => (/* binding */ bounceOutRightAnimation),
/* harmony export */   "bounceOutRightOnLeaveAnimation": () => (/* binding */ bounceOutRightOnLeaveAnimation),
/* harmony export */   "bounceOutUpAnimation": () => (/* binding */ bounceOutUpAnimation),
/* harmony export */   "bounceOutUpOnLeaveAnimation": () => (/* binding */ bounceOutUpOnLeaveAnimation),
/* harmony export */   "collapseAnimation": () => (/* binding */ collapseAnimation),
/* harmony export */   "collapseHorizontallyAnimation": () => (/* binding */ collapseHorizontallyAnimation),
/* harmony export */   "collapseLeftOnLeaveAnimation": () => (/* binding */ collapseLeftOnLeaveAnimation),
/* harmony export */   "collapseOnLeaveAnimation": () => (/* binding */ collapseOnLeaveAnimation),
/* harmony export */   "expandOnEnterAnimation": () => (/* binding */ expandOnEnterAnimation),
/* harmony export */   "expandRightOnEnterAnimation": () => (/* binding */ expandRightOnEnterAnimation),
/* harmony export */   "fadeInAnimation": () => (/* binding */ fadeInAnimation),
/* harmony export */   "fadeInDownAnimation": () => (/* binding */ fadeInDownAnimation),
/* harmony export */   "fadeInDownBigAnimation": () => (/* binding */ fadeInDownBigAnimation),
/* harmony export */   "fadeInDownBigOnEnterAnimation": () => (/* binding */ fadeInDownBigOnEnterAnimation),
/* harmony export */   "fadeInDownOnEnterAnimation": () => (/* binding */ fadeInDownOnEnterAnimation),
/* harmony export */   "fadeInExpandOnEnterAnimation": () => (/* binding */ fadeInExpandOnEnterAnimation),
/* harmony export */   "fadeInExpandRightOnEnterAnimation": () => (/* binding */ fadeInExpandRightOnEnterAnimation),
/* harmony export */   "fadeInLeftAnimation": () => (/* binding */ fadeInLeftAnimation),
/* harmony export */   "fadeInLeftBigAnimation": () => (/* binding */ fadeInLeftBigAnimation),
/* harmony export */   "fadeInLeftBigOnEnterAnimation": () => (/* binding */ fadeInLeftBigOnEnterAnimation),
/* harmony export */   "fadeInLeftOnEnterAnimation": () => (/* binding */ fadeInLeftOnEnterAnimation),
/* harmony export */   "fadeInOnEnterAnimation": () => (/* binding */ fadeInOnEnterAnimation),
/* harmony export */   "fadeInRightAnimation": () => (/* binding */ fadeInRightAnimation),
/* harmony export */   "fadeInRightBigAnimation": () => (/* binding */ fadeInRightBigAnimation),
/* harmony export */   "fadeInRightBigOnEnterAnimation": () => (/* binding */ fadeInRightBigOnEnterAnimation),
/* harmony export */   "fadeInRightOnEnterAnimation": () => (/* binding */ fadeInRightOnEnterAnimation),
/* harmony export */   "fadeInUpAnimation": () => (/* binding */ fadeInUpAnimation),
/* harmony export */   "fadeInUpBigAnimation": () => (/* binding */ fadeInUpBigAnimation),
/* harmony export */   "fadeInUpBigOnEnterAnimation": () => (/* binding */ fadeInUpBigOnEnterAnimation),
/* harmony export */   "fadeInUpOnEnterAnimation": () => (/* binding */ fadeInUpOnEnterAnimation),
/* harmony export */   "fadeOutAnimation": () => (/* binding */ fadeOutAnimation),
/* harmony export */   "fadeOutCollapseLeftOnLeaveAnimation": () => (/* binding */ fadeOutCollapseLeftOnLeaveAnimation),
/* harmony export */   "fadeOutCollapseOnLeaveAnimation": () => (/* binding */ fadeOutCollapseOnLeaveAnimation),
/* harmony export */   "fadeOutDownAnimation": () => (/* binding */ fadeOutDownAnimation),
/* harmony export */   "fadeOutDownBigAnimation": () => (/* binding */ fadeOutDownBigAnimation),
/* harmony export */   "fadeOutDownBigOnLeaveAnimation": () => (/* binding */ fadeOutDownBigOnLeaveAnimation),
/* harmony export */   "fadeOutDownOnLeaveAnimation": () => (/* binding */ fadeOutDownOnLeaveAnimation),
/* harmony export */   "fadeOutLeftAnimation": () => (/* binding */ fadeOutLeftAnimation),
/* harmony export */   "fadeOutLeftBigAnimation": () => (/* binding */ fadeOutLeftBigAnimation),
/* harmony export */   "fadeOutLeftBigOnLeaveAnimation": () => (/* binding */ fadeOutLeftBigOnLeaveAnimation),
/* harmony export */   "fadeOutLeftOnLeaveAnimation": () => (/* binding */ fadeOutLeftOnLeaveAnimation),
/* harmony export */   "fadeOutOnLeaveAnimation": () => (/* binding */ fadeOutOnLeaveAnimation),
/* harmony export */   "fadeOutRightAnimation": () => (/* binding */ fadeOutRightAnimation),
/* harmony export */   "fadeOutRightBigAnimation": () => (/* binding */ fadeOutRightBigAnimation),
/* harmony export */   "fadeOutRightBigOnLeaveAnimation": () => (/* binding */ fadeOutRightBigOnLeaveAnimation),
/* harmony export */   "fadeOutRightOnLeaveAnimation": () => (/* binding */ fadeOutRightOnLeaveAnimation),
/* harmony export */   "fadeOutUpAnimation": () => (/* binding */ fadeOutUpAnimation),
/* harmony export */   "fadeOutUpBigAnimation": () => (/* binding */ fadeOutUpBigAnimation),
/* harmony export */   "fadeOutUpBigOnLeaveAnimation": () => (/* binding */ fadeOutUpBigOnLeaveAnimation),
/* harmony export */   "fadeOutUpOnLeaveAnimation": () => (/* binding */ fadeOutUpOnLeaveAnimation),
/* harmony export */   "flashAnimation": () => (/* binding */ flashAnimation),
/* harmony export */   "flashOnEnterAnimation": () => (/* binding */ flashOnEnterAnimation),
/* harmony export */   "flipAnimation": () => (/* binding */ flipAnimation),
/* harmony export */   "flipInXAnimation": () => (/* binding */ flipInXAnimation),
/* harmony export */   "flipInXOnEnterAnimation": () => (/* binding */ flipInXOnEnterAnimation),
/* harmony export */   "flipInYAnimation": () => (/* binding */ flipInYAnimation),
/* harmony export */   "flipInYOnEnterAnimation": () => (/* binding */ flipInYOnEnterAnimation),
/* harmony export */   "flipOnEnterAnimation": () => (/* binding */ flipOnEnterAnimation),
/* harmony export */   "flipOutXAnimation": () => (/* binding */ flipOutXAnimation),
/* harmony export */   "flipOutXOnLeaveAnimation": () => (/* binding */ flipOutXOnLeaveAnimation),
/* harmony export */   "flipOutYAnimation": () => (/* binding */ flipOutYAnimation),
/* harmony export */   "flipOutYOnLeaveAnimation": () => (/* binding */ flipOutYOnLeaveAnimation),
/* harmony export */   "headShakeAnimation": () => (/* binding */ headShakeAnimation),
/* harmony export */   "headShakeOnEnterAnimation": () => (/* binding */ headShakeOnEnterAnimation),
/* harmony export */   "heartBeatAnimation": () => (/* binding */ heartBeatAnimation),
/* harmony export */   "heartBeatOnEnterAnimation": () => (/* binding */ heartBeatOnEnterAnimation),
/* harmony export */   "hingeAnimation": () => (/* binding */ hingeAnimation),
/* harmony export */   "hingeOnLeaveAnimation": () => (/* binding */ hingeOnLeaveAnimation),
/* harmony export */   "hueRotateAnimation": () => (/* binding */ hueRotateAnimation),
/* harmony export */   "jackInTheBoxAnimation": () => (/* binding */ jackInTheBoxAnimation),
/* harmony export */   "jackInTheBoxOnEnterAnimation": () => (/* binding */ jackInTheBoxOnEnterAnimation),
/* harmony export */   "jelloAnimation": () => (/* binding */ jelloAnimation),
/* harmony export */   "jelloOnEnterAnimation": () => (/* binding */ jelloOnEnterAnimation),
/* harmony export */   "lightSpeedInAnimation": () => (/* binding */ lightSpeedInAnimation),
/* harmony export */   "lightSpeedInOnEnterAnimation": () => (/* binding */ lightSpeedInOnEnterAnimation),
/* harmony export */   "lightSpeedOutAnimation": () => (/* binding */ lightSpeedOutAnimation),
/* harmony export */   "lightSpeedOutOnLeaveAnimation": () => (/* binding */ lightSpeedOutOnLeaveAnimation),
/* harmony export */   "pulseAnimation": () => (/* binding */ pulseAnimation),
/* harmony export */   "pulseOnEnterAnimation": () => (/* binding */ pulseOnEnterAnimation),
/* harmony export */   "rollInAnimation": () => (/* binding */ rollInAnimation),
/* harmony export */   "rollInOnEnterAnimation": () => (/* binding */ rollInOnEnterAnimation),
/* harmony export */   "rollOutAnimation": () => (/* binding */ rollOutAnimation),
/* harmony export */   "rollOutOnLeaveAnimation": () => (/* binding */ rollOutOnLeaveAnimation),
/* harmony export */   "rotateAnimation": () => (/* binding */ rotateAnimation),
/* harmony export */   "rotateInAnimation": () => (/* binding */ rotateInAnimation),
/* harmony export */   "rotateInDownLeftAnimation": () => (/* binding */ rotateInDownLeftAnimation),
/* harmony export */   "rotateInDownLeftOnEnterAnimation": () => (/* binding */ rotateInDownLeftOnEnterAnimation),
/* harmony export */   "rotateInDownRightAnimation": () => (/* binding */ rotateInDownRightAnimation),
/* harmony export */   "rotateInDownRightOnEnterAnimation": () => (/* binding */ rotateInDownRightOnEnterAnimation),
/* harmony export */   "rotateInOnEnterAnimation": () => (/* binding */ rotateInOnEnterAnimation),
/* harmony export */   "rotateInUpLeftAnimation": () => (/* binding */ rotateInUpLeftAnimation),
/* harmony export */   "rotateInUpLeftOnEnterAnimation": () => (/* binding */ rotateInUpLeftOnEnterAnimation),
/* harmony export */   "rotateInUpRightAnimation": () => (/* binding */ rotateInUpRightAnimation),
/* harmony export */   "rotateInUpRightOnEnterAnimation": () => (/* binding */ rotateInUpRightOnEnterAnimation),
/* harmony export */   "rotateOutAnimation": () => (/* binding */ rotateOutAnimation),
/* harmony export */   "rotateOutDownLeftAnimation": () => (/* binding */ rotateOutDownLeftAnimation),
/* harmony export */   "rotateOutDownLeftOnLeaveAnimation": () => (/* binding */ rotateOutDownLeftOnLeaveAnimation),
/* harmony export */   "rotateOutDownRightAnimation": () => (/* binding */ rotateOutDownRightAnimation),
/* harmony export */   "rotateOutDownRightOnLeaveAnimation": () => (/* binding */ rotateOutDownRightOnLeaveAnimation),
/* harmony export */   "rotateOutOnLeaveAnimation": () => (/* binding */ rotateOutOnLeaveAnimation),
/* harmony export */   "rotateOutUpLeftAnimation": () => (/* binding */ rotateOutUpLeftAnimation),
/* harmony export */   "rotateOutUpLeftOnLeaveAnimation": () => (/* binding */ rotateOutUpLeftOnLeaveAnimation),
/* harmony export */   "rotateOutUpRightAnimation": () => (/* binding */ rotateOutUpRightAnimation),
/* harmony export */   "rotateOutUpRightOnLeaveAnimation": () => (/* binding */ rotateOutUpRightOnLeaveAnimation),
/* harmony export */   "rubberBandAnimation": () => (/* binding */ rubberBandAnimation),
/* harmony export */   "rubberBandOnEnterAnimation": () => (/* binding */ rubberBandOnEnterAnimation),
/* harmony export */   "shakeAnimation": () => (/* binding */ shakeAnimation),
/* harmony export */   "shakeOnEnterAnimation": () => (/* binding */ shakeOnEnterAnimation),
/* harmony export */   "slideInDownAnimation": () => (/* binding */ slideInDownAnimation),
/* harmony export */   "slideInDownOnEnterAnimation": () => (/* binding */ slideInDownOnEnterAnimation),
/* harmony export */   "slideInLeftAnimation": () => (/* binding */ slideInLeftAnimation),
/* harmony export */   "slideInLeftOnEnterAnimation": () => (/* binding */ slideInLeftOnEnterAnimation),
/* harmony export */   "slideInRightAnimation": () => (/* binding */ slideInRightAnimation),
/* harmony export */   "slideInRightOnEnterAnimation": () => (/* binding */ slideInRightOnEnterAnimation),
/* harmony export */   "slideInUpAnimation": () => (/* binding */ slideInUpAnimation),
/* harmony export */   "slideInUpOnEnterAnimation": () => (/* binding */ slideInUpOnEnterAnimation),
/* harmony export */   "slideOutDownAnimation": () => (/* binding */ slideOutDownAnimation),
/* harmony export */   "slideOutDownOnLeaveAnimation": () => (/* binding */ slideOutDownOnLeaveAnimation),
/* harmony export */   "slideOutLeftAnimation": () => (/* binding */ slideOutLeftAnimation),
/* harmony export */   "slideOutLeftOnLeaveAnimation": () => (/* binding */ slideOutLeftOnLeaveAnimation),
/* harmony export */   "slideOutRightAnimation": () => (/* binding */ slideOutRightAnimation),
/* harmony export */   "slideOutRightOnLeaveAnimation": () => (/* binding */ slideOutRightOnLeaveAnimation),
/* harmony export */   "slideOutUpAnimation": () => (/* binding */ slideOutUpAnimation),
/* harmony export */   "slideOutUpOnLeaveAnimation": () => (/* binding */ slideOutUpOnLeaveAnimation),
/* harmony export */   "swingAnimation": () => (/* binding */ swingAnimation),
/* harmony export */   "swingOnEnterAnimation": () => (/* binding */ swingOnEnterAnimation),
/* harmony export */   "tadaAnimation": () => (/* binding */ tadaAnimation),
/* harmony export */   "tadaOnEnterAnimation": () => (/* binding */ tadaOnEnterAnimation),
/* harmony export */   "wobbleAnimation": () => (/* binding */ wobbleAnimation),
/* harmony export */   "wobbleOnEnterAnimation": () => (/* binding */ wobbleOnEnterAnimation),
/* harmony export */   "zoomInAnimation": () => (/* binding */ zoomInAnimation),
/* harmony export */   "zoomInDownAnimation": () => (/* binding */ zoomInDownAnimation),
/* harmony export */   "zoomInDownOnEnterAnimation": () => (/* binding */ zoomInDownOnEnterAnimation),
/* harmony export */   "zoomInLeftAnimation": () => (/* binding */ zoomInLeftAnimation),
/* harmony export */   "zoomInLeftOnEnterAnimation": () => (/* binding */ zoomInLeftOnEnterAnimation),
/* harmony export */   "zoomInOnEnterAnimation": () => (/* binding */ zoomInOnEnterAnimation),
/* harmony export */   "zoomInRightAnimation": () => (/* binding */ zoomInRightAnimation),
/* harmony export */   "zoomInRightOnEnterAnimation": () => (/* binding */ zoomInRightOnEnterAnimation),
/* harmony export */   "zoomInUpAnimation": () => (/* binding */ zoomInUpAnimation),
/* harmony export */   "zoomInUpOnEnterAnimation": () => (/* binding */ zoomInUpOnEnterAnimation),
/* harmony export */   "zoomOutAnimation": () => (/* binding */ zoomOutAnimation),
/* harmony export */   "zoomOutDownAnimation": () => (/* binding */ zoomOutDownAnimation),
/* harmony export */   "zoomOutDownOnLeaveAnimation": () => (/* binding */ zoomOutDownOnLeaveAnimation),
/* harmony export */   "zoomOutLeftAnimation": () => (/* binding */ zoomOutLeftAnimation),
/* harmony export */   "zoomOutLeftOnLeaveAnimation": () => (/* binding */ zoomOutLeftOnLeaveAnimation),
/* harmony export */   "zoomOutOnLeaveAnimation": () => (/* binding */ zoomOutOnLeaveAnimation),
/* harmony export */   "zoomOutRightAnimation": () => (/* binding */ zoomOutRightAnimation),
/* harmony export */   "zoomOutRightOnLeaveAnimation": () => (/* binding */ zoomOutRightOnLeaveAnimation),
/* harmony export */   "zoomOutUpAnimation": () => (/* binding */ zoomOutUpAnimation),
/* harmony export */   "zoomOutUpOnLeaveAnimation": () => (/* binding */ zoomOutUpOnLeaveAnimation)
/* harmony export */ });
/* harmony import */ var _angular_animations__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/animations */ 4851);

function useAnimationIncludingChildren(animation, options) {
  return [...(options && options.animateChildren === 'before' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : []), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.useAnimation)(animation), ...(!options || !options.animateChildren || options.animateChildren === 'together' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])]), ...(options && options.animateChildren === 'after' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])];
}
const bounce = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -30px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -30px, 0)',
  easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)',
  offset: 0.43
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)',
  offset: 0.53
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -15px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.7
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.755, 0.05, 0.855, 0.06)',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -4px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0 = bounce;
const DEFAULT_DURATION = 1000;
function bounceAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounce', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center bottom'
  }), ...useAnimationIncludingChildren(bounce(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION
    }
  })]);
}
function bounceOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center bottom'
  }), ...useAnimationIncludingChildren(bounce(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION
    }
  })]);
}
const flash = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 0.25
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$1 = flash;
const DEFAULT_DURATION$1 = 1000;
function flashAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flash', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(flash(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1
    }
  })]);
}
function flashOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flashOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(flash(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1
    }
  })]);
}
const headShake = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'translateX(0)',
  easing: 'ease-in-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translateX(-6px) rotateY(-9deg)',
  easing: 'ease-in-out',
  offset: 0.065
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translateX(5px) rotateY(7deg)',
  easing: 'ease-in-out',
  offset: 0.185
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translateX(-3px) rotateY(-5deg)',
  easing: 'ease-in-out',
  offset: 0.315
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translateX(2px) rotateY(3deg)',
  easing: 'ease-in-out',
  offset: 0.435
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translateX(0)',
  easing: 'ease-in-out',
  offset: 0.5
})]))]);
const ɵ0$2 = headShake;
const DEFAULT_DURATION$2 = 1000;
function headShakeAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'headShake', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(headShake(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$2
    }
  })]);
}
function headShakeOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'headShakeOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(headShake(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$2
    }
  })]);
}
const heartBeat = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'scale(1)',
  easing: 'ease-in-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale({{scale}})',
  easing: 'ease-in-out',
  offset: 0.14
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale(1)',
  easing: 'ease-in-out',
  offset: 0.28
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale({{scale}})',
  easing: 'ease-in-out',
  offset: 0.42
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale(1)',
  easing: 'ease-in-out',
  offset: 0.7
})]))]);
const ɵ0$3 = heartBeat;
const DEFAULT_DURATION$3 = 1300;
const DEFAULT_SCALE = 1.3;
function heartBeatAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'heartBeat', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(heartBeat(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$3,
      scale: options && options.scale || DEFAULT_SCALE
    }
  })]);
}
function heartBeatOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'heartBeatOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(heartBeat(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$3,
      scale: options && options.scale || DEFAULT_SCALE
    }
  })]);
}
const jello = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0.111
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(-12.5deg) skewY(-12.5deg)',
  easing: 'ease',
  offset: 0.222
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(6.25deg) skewY(6.25deg)',
  easing: 'ease',
  offset: 0.333
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(-3.125deg) skewY(-3.125deg)',
  easing: 'ease',
  offset: 0.444
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(1.5625deg) skewY(1.5625deg)',
  easing: 'ease',
  offset: 0.555
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(-0.78125deg) skewY(-0.78125deg)',
  easing: 'ease',
  offset: 0.666
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(0.390625deg) skewY(0.390625deg)',
  easing: 'ease',
  offset: 0.777
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(-0.1953125deg) skewY(-0.1953125deg)',
  easing: 'ease',
  offset: 0.888
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'skewX(0deg) skewY(0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$4 = jello;
const DEFAULT_DURATION$4 = 1000;
function jelloAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'jello', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(jello(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$4
    }
  })]);
}
function jelloOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'jelloOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(jello(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$4
    }
  })]);
}
const pulse = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d({{scale}}, {{scale}}, {{scale}})',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$5 = pulse;
const DEFAULT_DURATION$5 = 1000;
function pulseAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'pulse', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(pulse(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$5,
      scale: options && options.scale || 1.05
    }
  })]);
}
function pulseOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'pulseOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(pulse(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$5,
      scale: options && options.scale || 1.05
    }
  })]);
}
const rubberBand = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.25, 0.75, 1)',
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.75, 1.25, 1)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.15, 0.85, 1)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.95, 1.05, 1)',
  easing: 'ease',
  offset: 0.65
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.05, 0.95, 1)',
  easing: 'ease',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$6 = rubberBand;
const DEFAULT_DURATION$6 = 1000;
function rubberBandAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rubberBand', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(rubberBand(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$6
    }
  })]);
}
function rubberBandOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rubberBandOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(rubberBand(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$6
    }
  })]);
}
const shake = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.1
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.7
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$7 = shake;
const DEFAULT_DURATION$7 = 1000;
function shakeAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'shake', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(shake(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$7,
      translate: options && options.translate || '10px'
    }
  })]);
}
function shakeOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'shakeOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(shake(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$7,
      translate: options && options.translate || '10px'
    }
  })]);
}
const swing = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'top center',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'rotate3d(0, 0, 1, 15deg)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'rotate3d(0, 0, 1, -10deg)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'rotate3d(0, 0, 1, 5deg)',
  easing: 'ease',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'rotate3d(0, 0, 1, -5deg)',
  easing: 'ease',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$8 = swing;
const DEFAULT_DURATION$8 = 1000;
function swingAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'swing', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(swing(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$8
    }
  })]);
}
function swingOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'swingOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'top center'
  }), ...useAnimationIncludingChildren(swing(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$8
    }
  })]);
}
const tada = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.1
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.9, 0.9, 0.9) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)',
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)',
  easing: 'ease',
  offset: 0.7
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1) rotate3d(0, 0, 1, 3deg)',
  easing: 'ease',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$9 = tada;
const DEFAULT_DURATION$9 = 1000;
function tadaAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'tada', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(tada(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$9
    }
  })]);
}
function tadaOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'tadaOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(tada(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$9
    }
  })]);
}
const wobble = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-25%, 0, 0) rotate3d(0, 0, 1, -5deg)',
  easing: 'ease',
  offset: 0.15
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(20%, 0, 0) rotate3d(0, 0, 1, 3deg)',
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-15%, 0, 0) rotate3d(0, 0, 1, -3deg)',
  easing: 'ease',
  offset: 0.45
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(10%, 0, 0) rotate3d(0, 0, 1, 2deg)',
  easing: 'ease',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-5%, 0, 0) rotate3d(0, 0, 1, -1deg)',
  easing: 'ease',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$a = wobble;
const DEFAULT_DURATION$a = 1000;
function wobbleAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'wobble', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(`0 ${options && options.direction || '<=>'} 1`, [...useAnimationIncludingChildren(wobble(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$a
    }
  })]);
}
function wobbleOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'wobbleOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(wobble(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$a
    }
  })]);
}
const bounceInDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 25px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -10px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 5px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})]))]));
const ɵ0$b = bounceInDown;
const DEFAULT_DURATION$b = 1000;
function bounceInDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$b,
      translate: options && options.translate || '3000px'
    }
  })]);
}
function bounceInDownOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInDownOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$b,
      translate: options && options.translate || '3000px'
    }
  })]);
}
const bounceInLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(25px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-10px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(5px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})]))]));
const ɵ0$c = bounceInLeft;
const DEFAULT_DURATION$c = 1000;
function bounceInLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$c,
      translate: options && options.translate || '3000px'
    }
  })]);
}
function bounceInLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$c,
      translate: options && options.translate || '3000px'
    }
  })]);
}
const bounceInRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-25px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(10px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-5px, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})]))]));
const ɵ0$d = bounceInRight;
const DEFAULT_DURATION$d = 1000;
function bounceInRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$d,
      translate: options && options.translate || '3000px'
    }
  })]);
}
function bounceInRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$d,
      translate: options && options.translate || '3000px'
    }
  })]);
}
const bounceInUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -20px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 10px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.75
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -5px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.9
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -5px, 0)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})]))]));
const ɵ0$e = bounceInUp;
const DEFAULT_DURATION$e = 1000;
function bounceInUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$e,
      translate: options && options.translate || '3000px'
    }
  })]);
}
function bounceInUpOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInUpOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$e,
      translate: options && options.translate || '3000px'
    }
  })]);
}
const bounceIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.3, 0.3, 0.3)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.9, 0.9, 0.9)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.03, 1.03, 1.03)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.97, 0.97, 0.97)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'cubic-bezier(0.215, 0.61, 0.355, 1)',
  offset: 1
})]))]));
const ɵ0$f = bounceIn;
const DEFAULT_DURATION$f = 750;
function bounceInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$f
    }
  })]);
}
function bounceInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(bounceIn(), options)]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$f
    }
  })]);
}
const bounceOutDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 10px, 0)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -20px, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -20px, 0)',
  easing: 'ease',
  offset: 0.45
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0.45
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))]));
const ɵ0$g = bounceOutDown;
const DEFAULT_DURATION$g = 1000;
function bounceOutDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(bounceOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$g,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function bounceOutDownOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutDownOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(bounceOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$g,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const bounceOutLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(20px, 0, 0)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$h = bounceOutLeft;
const DEFAULT_DURATION$h = 1000;
function bounceOutLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(bounceOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$h,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function bounceOutLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(bounceOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$h,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const bounceOutRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(-20px, 0, 0)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$i = bounceOutRight;
const DEFAULT_DURATION$i = 1000;
function bounceOutRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(bounceOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$i,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function bounceOutRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(bounceOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$i,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const bounceOutUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -10px, 0)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 20px, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 20px, 0)',
  easing: 'ease',
  offset: 0.45
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0.45
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))])]));
const ɵ0$j = bounceOutUp;
const DEFAULT_DURATION$j = 1000;
function bounceOutUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(bounceOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$j,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function bounceOutUpOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutUpOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(bounceOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$j,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const bounceOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.9, 0.9, 0.9)',
  easing: 'ease',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1.1, 1.1, 1.1)',
  easing: 'ease',
  offset: 0.55
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.3, 0.3, 0.3)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0.55
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))]));
const ɵ0$k = bounceOut;
const DEFAULT_DURATION$k = 750;
function bounceOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(bounceOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$k
    }
  })]);
}
function bounceOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'bounceOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(bounceOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$k
    }
  })]);
}
const fadeInDownBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$l = fadeInDownBig;
const DEFAULT_DURATION$l = 1000;
function fadeInDownBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInDownBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInDownBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$l,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeInDownBigOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInDownBigOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInDownBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$l,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeInDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$m = fadeInDown;
const DEFAULT_DURATION$m = 1000;
function fadeInDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$m,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeInDownOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInDownOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$m,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeInLeftBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$n = fadeInLeftBig;
const DEFAULT_DURATION$n = 1000;
function fadeInLeftBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInLeftBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInLeftBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$n,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeInLeftBigOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInLeftBigOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInLeftBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$n,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeInLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$o = fadeInLeft;
const DEFAULT_DURATION$o = 1000;
function fadeInLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$o,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeInLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$o,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeInRightBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$p = fadeInRightBig;
const DEFAULT_DURATION$p = 1000;
function fadeInRightBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInRightBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInRightBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$p,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeInRightBigOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInRightBigOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInRightBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$p,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeInRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$q = fadeInRight;
const DEFAULT_DURATION$q = 1000;
function fadeInRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$q,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeInRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$q,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeInUpBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$r = fadeInUpBig;
const DEFAULT_DURATION$r = 1000;
function fadeInUpBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInUpBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInUpBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$r,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeInUpBigOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInUpBigOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInUpBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$r,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeInUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$s = fadeInUp;
const DEFAULT_DURATION$s = 1000;
function fadeInUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$s,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeInUpOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInUpOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$s,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$t = fadeIn;
const DEFAULT_DURATION$t = 1000;
function fadeInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$t
    }
  })]);
}
function fadeInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$t
    }
  })]);
}
const fadeOutDownBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$u = fadeOutDownBig;
const DEFAULT_DURATION$u = 1000;
function fadeOutDownBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutDownBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutDownBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$u,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeOutDownBigOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutDownBigOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutDownBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$u,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeOutDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$v = fadeOutDown;
const DEFAULT_DURATION$v = 1000;
function fadeOutDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$v,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeOutDownOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutDownOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$v,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeOutLeftBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$w = fadeOutLeftBig;
const DEFAULT_DURATION$w = 1000;
function fadeOutLeftBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutLeftBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutLeftBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$w,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeOutLeftBigOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutLeftBigOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutLeftBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$w,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeOutLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$x = fadeOutLeft;
const DEFAULT_DURATION$x = 1000;
function fadeOutLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$x,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeOutLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$x,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeOutRightBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$y = fadeOutRightBig;
const DEFAULT_DURATION$y = 1000;
function fadeOutRightBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutRightBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutRightBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$y,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeOutRightBigOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutRightBigOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutRightBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$y,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeOutRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$z = fadeOutRight;
const DEFAULT_DURATION$z = 1000;
function fadeOutRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$z,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeOutRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$z,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeOutUpBig = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$A = fadeOutUpBig;
const DEFAULT_DURATION$A = 1000;
function fadeOutUpBigAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutUpBig', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutUpBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$A,
      translate: options && options.translate || '2000px'
    }
  })]);
}
function fadeOutUpBigOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutUpBigOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutUpBig(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$A,
      translate: options && options.translate || '2000px'
    }
  })]);
}
const fadeOutUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$B = fadeOutUp;
const DEFAULT_DURATION$B = 1000;
function fadeOutUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$B,
      translate: options && options.translate || '100%'
    }
  })]);
}
function fadeOutUpOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutUpOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$B,
      translate: options && options.translate || '100%'
    }
  })]);
}
const fadeOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$C = fadeOut;
const DEFAULT_DURATION$C = 1000;
function fadeOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(fadeOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$C
    }
  })]);
}
function fadeOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$C
    }
  })]);
}
const flipInX = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'perspective(400px) rotate3d(1, 0, 0, {{degrees}}deg)',
  opacity: 0,
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(1, 0, 0, -20deg)',
  opacity: 0.5,
  easing: 'ease-in',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(1, 0, 0, 10deg)',
  opacity: 1,
  easing: 'ease-in',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(1, 0, 0, -5deg)',
  easing: 'ease',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$D = flipInX;
const DEFAULT_DURATION$D = 1000;
function flipInXAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipInX', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipInX(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$D,
      degrees: options && options.degrees || 90
    }
  })]);
}
function flipInXOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipInXOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipInX(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$D,
      degrees: options && options.degrees || 90
    }
  })]);
}
const flipInY = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'perspective(400px) rotate3d(0, 1, 0, {{degrees}}deg)',
  opacity: 0,
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(0, 1, 0, -20deg)',
  opacity: 0.5,
  easing: 'ease-in',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(0, 1, 0, 10deg)',
  opacity: 1,
  easing: 'ease-in',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(0, 1, 0, -5deg)',
  easing: 'ease',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$E = flipInY;
const DEFAULT_DURATION$E = 1000;
function flipInYAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipInY', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipInY(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$E,
      degrees: options && options.degrees || 90
    }
  })]);
}
function flipInYOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipInYOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipInY(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$E,
      degrees: options && options.degrees || 90
    }
  })]);
}
const flipOutX = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px)',
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(1, 0, 0, -20deg)',
  opacity: 1,
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(1, 0, 0, {{degrees}}deg)',
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$F = flipOutX;
const DEFAULT_DURATION$F = 750;
function flipOutXAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipOutX', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipOutX(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$F,
      degrees: options && options.degrees || 90
    }
  })]);
}
function flipOutXOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipOutXOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipOutX(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$F,
      degrees: options && options.degrees || 90
    }
  })]);
}
const flipOutY = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px)',
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(0, 1, 0, -15deg)',
  opacity: 1,
  easing: 'ease',
  offset: 0.3
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) rotate3d(0, 1, 0, {{degrees}}deg)',
  opacity: 0,
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$G = flipOutY;
const DEFAULT_DURATION$G = 750;
function flipOutYAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipOutY', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipOutY(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$G,
      degrees: options && options.degrees || 90
    }
  })]);
}
function flipOutYOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipOutYOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flipOutY(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$G,
      degrees: options && options.degrees || 90
    }
  })]);
}
const flip = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, -360deg)',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px) rotate3d(0, 1, 0, -190deg)',
  easing: 'ease-out',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 150px) rotate3d(0, 1, 0, -170deg)',
  easing: 'ease-out',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) scale3d(0.95, 0.95, 0.95) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)',
  easing: 'ease-in',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'perspective(400px) scale3d(1, 1, 1) translate3d(0, 0, 0) rotate3d(0, 1, 0, 0deg)',
  easing: 'ease-in',
  offset: 1
})]))]);
const ɵ0$H = flip;
const DEFAULT_DURATION$H = 1000;
function flipAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flip', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 <=> 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flip(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$H
    }
  })]);
}
function flipOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'flipOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'backface-visibility': 'visible'
  }), ...useAnimationIncludingChildren(flip(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$H
    }
  })]);
}
const lightSpeedIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0) skewX(-30deg)',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'skewX(20deg)',
  easing: 'ease-out',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'skewX(-5deg)',
  easing: 'ease-out',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease-out',
  offset: 1
})]))]);
const ɵ0$I = lightSpeedIn;
const DEFAULT_DURATION$I = 1000;
function lightSpeedInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'lightSpeedIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(lightSpeedIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$I,
      translate: options && options.translate || '100%'
    }
  })]);
}
function lightSpeedInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'lightSpeedInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(lightSpeedIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$I,
      translate: options && options.translate || '100%'
    }
  })]);
}
const lightSpeedOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0) skewX(30deg)',
  easing: 'ease-in',
  offset: 1
})]))]);
const ɵ0$J = lightSpeedOut;
const DEFAULT_DURATION$J = 1000;
function lightSpeedOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'lightSpeedOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(lightSpeedOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$J,
      translate: options && options.translate || '100%'
    }
  })]);
}
function lightSpeedOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'lightSpeedOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(lightSpeedOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$J,
      translate: options && options.translate || '100%'
    }
  })]);
}
const rotateInDownLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$K = rotateInDownLeft;
const DEFAULT_DURATION$K = 1000;
function rotateInDownLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInDownLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateInDownLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$K,
      degrees: options && options.degrees || -45
    }
  })]);
}
function rotateInDownLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInDownLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateInDownLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$K,
      degrees: options && options.degrees || -45
    }
  })]);
}
const rotateInDownRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$L = rotateInDownRight;
const DEFAULT_DURATION$L = 1000;
function rotateInDownRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInDownRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateInDownRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$L,
      degrees: options && options.degrees || 45
    }
  })]);
}
function rotateInDownRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInDownRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateInDownRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$L,
      degrees: options && options.degrees || 45
    }
  })]);
}
const rotateInUpLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$M = rotateInUpLeft;
const DEFAULT_DURATION$M = 1000;
function rotateInUpLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInUpLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateInUpLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$M,
      degrees: options && options.degrees || 45
    }
  })]);
}
function rotateInUpLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInUpLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateInUpLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$M,
      degrees: options && options.degrees || 45
    }
  })]);
}
const rotateInUpRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$N = rotateInUpRight;
const DEFAULT_DURATION$N = 1000;
function rotateInUpRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInUpRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateInUpRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$N,
      degrees: options && options.degrees || -90
    }
  })]);
}
function rotateInUpRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInUpRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateInUpRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$N,
      degrees: options && options.degrees || -90
    }
  })]);
}
const rotateIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'rotate({{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'rotate(0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$O = rotateIn;
const DEFAULT_DURATION$O = 1000;
function rotateInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(rotateIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$O,
      degrees: options && options.degrees || -200
    }
  })]);
}
function rotateInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(rotateIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$O,
      degrees: options && options.degrees || -200
    }
  })]);
}
const rotateOutDownLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$P = rotateOutDownLeft;
const DEFAULT_DURATION$P = 1000;
function rotateOutDownLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutDownLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateOutDownLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$P,
      degrees: options && options.degrees || 45
    }
  })]);
}
function rotateOutDownLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutDownLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateOutDownLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$P,
      degrees: options && options.degrees || 45
    }
  })]);
}
const rotateOutDownRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$Q = rotateOutDownRight;
const DEFAULT_DURATION$Q = 1000;
function rotateOutDownRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutDownRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateOutDownRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Q,
      degrees: options && options.degrees || -45
    }
  })]);
}
function rotateOutDownRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutDownRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateOutDownRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Q,
      degrees: options && options.degrees || -45
    }
  })]);
}
const rotateOutUpLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$R = rotateOutUpLeft;
const DEFAULT_DURATION$R = 1000;
function rotateOutUpLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutUpLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateOutUpLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$R,
      degrees: options && options.degrees || -45
    }
  })]);
}
function rotateOutUpLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutUpLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'left bottom'
  }), ...useAnimationIncludingChildren(rotateOutUpLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$R,
      degrees: options && options.degrees || -45
    }
  })]);
}
const rotateOutUpRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$S = rotateOutUpRight;
const DEFAULT_DURATION$S = 1000;
function rotateOutUpRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutUpRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateOutUpRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$S,
      degrees: options && options.degrees || 90
    }
  })]);
}
function rotateOutUpRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutUpRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'right bottom'
  }), ...useAnimationIncludingChildren(rotateOutUpRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$S,
      degrees: options && options.degrees || 90
    }
  })]);
}
const rotateOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'rotate({{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$T = rotateOut;
const DEFAULT_DURATION$T = 1000;
function rotateOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(rotateOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$T,
      degrees: options && options.degrees || 200
    }
  })]);
}
function rotateOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotateOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    'transform-origin': 'center'
  }), ...useAnimationIncludingChildren(rotateOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$T,
      degrees: options && options.degrees || 200
    }
  })]);
}
const slideInDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'translate3d(0, -{{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$U = slideInDown;
const DEFAULT_DURATION$U = 1000;
function slideInDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$U,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideInDownOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInDownOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$U,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideInLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'translate3d(-{{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$V = slideInLeft;
const DEFAULT_DURATION$V = 1000;
function slideInLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$V,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideInLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$V,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideInRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'translate3d({{translate}}, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$W = slideInRight;
const DEFAULT_DURATION$W = 1000;
function slideInRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$W,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideInRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$W,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideInUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'translate3d(0, {{translate}}, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$X = slideInUp;
const DEFAULT_DURATION$X = 1000;
function slideInUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$X,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideInUpOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideInUpOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(slideInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$X,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideOutDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, {{translate}}, 0)',
  visibility: 'hidden',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$Y = slideOutDown;
const DEFAULT_DURATION$Y = 1000;
function slideOutDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(slideOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Y,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideOutDownOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutDownOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(slideOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Y,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideOutLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(-{{translate}}, 0, 0)',
  visibility: 'hidden',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$Z = slideOutLeft;
const DEFAULT_DURATION$Z = 1000;
function slideOutLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(slideOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Z,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideOutLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(slideOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$Z,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideOutRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d({{translate}}, 0, 0)',
  visibility: 'hidden',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$_ = slideOutRight;
const DEFAULT_DURATION$_ = 1000;
function slideOutRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(slideOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$_,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideOutRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(slideOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$_,
      translate: options && options.translate || '100%'
    }
  })]);
}
const slideOutUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'translate3d(0, -{{translate}}, 0)',
  visibility: 'hidden',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$$ = slideOutUp;
const DEFAULT_DURATION$$ = 1000;
function slideOutUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(slideOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$$,
      translate: options && options.translate || '100%'
    }
  })]);
}
function slideOutUpOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'slideOutUpOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(slideOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$$,
      translate: options && options.translate || '100%'
    }
  })]);
}
const zoomInDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, -1000px, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)',
  offset: 1
})]))]);
const ɵ0$10 = zoomInDown;
const DEFAULT_DURATION$10 = 1000;
function zoomInDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$10
    }
  })]);
}
function zoomInDownOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInDownOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$10
    }
  })]);
}
const zoomInLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(-3000px, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(10px, 0, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)',
  offset: 1
})]))]);
const ɵ0$11 = zoomInLeft;
const DEFAULT_DURATION$11 = 1000;
function zoomInLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$11
    }
  })]);
}
function zoomInLeftOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInLeftOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$11
    }
  })]);
}
const zoomInRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(1000px, 0, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(-10px, 0, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)',
  offset: 1
})]))]);
const ɵ0$12 = zoomInRight;
const DEFAULT_DURATION$12 = 1000;
function zoomInRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$12
    }
  })]);
}
function zoomInRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$12
    }
  })]);
}
const zoomInUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, 1000px, 0)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(1, 1, 1) translate3d(0, 0, 0)',
  easing: 'cubic-bezier(0.175, 0.885, 0.32, 1)',
  offset: 1
})]))]);
const ɵ0$13 = zoomInUp;
const DEFAULT_DURATION$13 = 1000;
function zoomInUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$13
    }
  })]);
}
function zoomInUpOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInUpOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomInUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$13
    }
  })]);
}
const zoomIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  transform: 'scale3d(0.3, 0.3, 0.3)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 1
})]))]));
const ɵ0$14 = zoomIn;
const DEFAULT_DURATION$14 = 1000;
function zoomInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$14
    }
  })]);
}
function zoomInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(zoomIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$14
    }
  })]);
}
const zoomOutDown = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, -60px, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, 2000px, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 1
})]))]);
const ɵ0$15 = zoomOutDown;
const DEFAULT_DURATION$15 = 1000;
function zoomOutDownAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutDown', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(zoomOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$15
    }
  })]);
}
function zoomOutDownOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutDownOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(zoomOutDown(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$15
    }
  })]);
}
const zoomOutLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(42px, 0, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(-2000px, 0, 0)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center center',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'left center',
  offset: 0.4
})]))]));
const ɵ0$16 = zoomOutLeft;
const DEFAULT_DURATION$16 = 1000;
function zoomOutLeftAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutLeft', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(zoomOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$16
    }
  })]);
}
function zoomOutLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(zoomOutLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$16
    }
  })]);
}
const zoomOutRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(-42px, 0, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(2000px, 0, 0)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center center',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'right center',
  offset: 0.4
})]))]));
const ɵ0$17 = zoomOutRight;
const DEFAULT_DURATION$17 = 1000;
function zoomOutRightAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutRight', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(zoomOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$17
    }
  })]);
}
function zoomOutRightOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutRightOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(zoomOutRight(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$17
    }
  })]);
}
const zoomOutUp = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  opacity: 1,
  transform: 'scale3d(0.475, 0.475, 0.475) translate3d(0, 60px, 0)',
  easing: 'ease',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  opacity: 0,
  transform: 'scale3d(0.1, 0.1, 0.1) translate3d(0, -2000px, 0)',
  easing: 'cubic-bezier(0.55, 0.055, 0.675, 0.19)',
  offset: 1
})]))]);
const ɵ0$18 = zoomOutUp;
const DEFAULT_DURATION$18 = 1000;
function zoomOutUpAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutUp', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(zoomOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$18
    }
  })]);
}
function zoomOutUpOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutUpOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(zoomOutUp(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$18
    }
  })]);
}
const zoomOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'scale3d(0.3, 0.3, 0.3)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(1, 1, 1)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  transform: 'scale3d(0.3, 0.3, 0.3)',
  easing: 'ease',
  offset: 0.5
})]))]));
const ɵ0$19 = zoomOut;
const DEFAULT_DURATION$19 = 1000;
function zoomOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(zoomOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$19
    }
  })]);
}
function zoomOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'zoomOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(zoomOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$19
    }
  })]);
}
const hinge = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  'transform-origin': 'top left',
  transform: 'translate3d(0, 0, 0)',
  easing: 'ease-in-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  'transform-origin': 'top left',
  transform: 'rotate3d(0, 0, 1, 80deg)',
  easing: 'ease-in-out',
  offset: 0.2
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  'transform-origin': 'top left',
  transform: 'rotate3d(0, 0, 1, 60deg)',
  easing: 'ease-in-out',
  offset: 0.4
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  'transform-origin': 'top left',
  transform: 'rotate3d(0, 0, 1, 80deg)',
  easing: 'ease-in-out',
  offset: 0.6
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  'transform-origin': 'top left',
  transform: 'rotate3d(0, 0, 1, 60deg)',
  easing: 'ease-in-out',
  offset: 0.8
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  'transform-origin': 'top left',
  transform: 'translate3d(0, 700px, 0)',
  easing: 'ease-in-out',
  offset: 1
})]))]);
const ɵ0$1a = hinge;
const DEFAULT_DURATION$1a = 2000;
function hingeAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'hinge', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(hinge(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1a
    }
  })]);
}
function hingeOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'hingeOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(hinge(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1a
    }
  })]);
}
const jackInTheBox = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  transform: 'scale(0.1) rotate(30deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  transform: 'rotate(-10deg)',
  easing: 'ease',
  offset: 0.5
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  transform: 'rotate(3deg)',
  easing: 'ease',
  offset: 0.7
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  'transform-origin': 'center bottom',
  transform: 'scale(1)',
  easing: 'ease',
  offset: 1
})])), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  offset: 1
})]))]));
const ɵ0$1b = jackInTheBox;
const DEFAULT_DURATION$1b = 1000;
function jackInTheBoxAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'jackInTheBox', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(jackInTheBox(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1b
    }
  })]);
}
function jackInTheBoxOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'jackInTheBoxOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(jackInTheBox(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1b
    }
  })]);
}
const rollIn = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  visibility: 'visible',
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0) rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0) rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$1c = rollIn;
const DEFAULT_DURATION$1c = 1000;
function rollInAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rollIn', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(rollIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1c,
      degrees: options && options.degrees || -120,
      translate: options && options.translate || '-100%'
    }
  })]);
}
function rollInOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rollInOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(rollIn(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1c,
      degrees: options && options.degrees || -120,
      translate: options && options.translate || '-100%'
    }
  })]);
}
const rollOut = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 1,
  transform: 'translate3d(0, 0, 0) rotate3d(0, 0, 1, 0deg)',
  easing: 'ease',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  opacity: 0,
  transform: 'translate3d({{translate}}, 0, 0) rotate3d(0, 0, 1, {{degrees}}deg)',
  easing: 'ease',
  offset: 1
})]))]);
const ɵ0$1d = rollOut;
const DEFAULT_DURATION$1d = 1000;
function rollOutAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rollOut', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...useAnimationIncludingChildren(rollOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1d,
      degrees: options && options.degrees || 120,
      translate: options && options.translate || '100%'
    }
  })]);
}
function rollOutOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rollOutOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(rollOut(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1d,
      degrees: options && options.degrees || 120,
      translate: options && options.translate || '100%'
    }
  })]);
}
function animateChildrenOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'animateChildrenOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })])]);
}
function animateIncludingChildren(easing, options) {
  return [...(options && options.animateChildren === 'before' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : []), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + easing)]), ...(!options || !options.animateChildren || options.animateChildren === 'together' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])]), ...(options && options.animateChildren === 'after' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])];
}
const DEFAULT_DURATION$1e = 200;
function collapseAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'collapse', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('1', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    height: '0',
    visibility: 'hidden',
    overflow: 'hidden'
  })), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('0', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
    visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
    overflow: 'hidden'
  })), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...animateIncludingChildren('ease-in', options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('1 => 0', [...animateIncludingChildren('ease-out', options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function collapseHorizontallyAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'collapseHorizontally', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('1', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    width: '0',
    visibility: 'hidden',
    overflow: 'hidden'
  })), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('0', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
    visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
    overflow: 'hidden'
  })), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...animateIncludingChildren('ease-in', options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('1 => 0', [...animateIncludingChildren('ease-out', options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
const expand = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: '0',
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 1
})])));
const ɵ0$1e = expand;
const expandRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: '0',
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 1
})])));
const ɵ1 = expandRight;
const fadeInExpand = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: '0',
  opacity: 0,
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 1
})])));
const ɵ2 = fadeInExpand;
const fadeInExpandRight = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: '0',
  opacity: 0,
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-out',
  offset: 1
})])));
const ɵ3 = fadeInExpandRight;
const collapse = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: '0',
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 1
})])));
const ɵ4 = collapse;
const collapseLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: '0',
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 1
})])));
const ɵ5 = collapseLeft;
const fadeOutCollapse = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  height: '0',
  opacity: 0,
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 1
})])));
const ɵ6 = fadeOutCollapse;
const fadeOutCollapseLeft = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)((0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  opacity: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  visibility: _angular_animations__WEBPACK_IMPORTED_MODULE_0__.AUTO_STYLE,
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  width: '0',
  opacity: 0,
  visibility: 'hidden',
  overflow: 'hidden',
  easing: 'ease-in',
  offset: 1
})])));
const ɵ7 = fadeOutCollapseLeft;
function expandOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'expandOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(expand(), options)]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function expandRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'expandRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(expandRight(), options)]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function collapseOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'collapseOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(collapse(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function collapseLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'collapseLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(collapseLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function fadeInExpandOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInExpandOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInExpand(), options)]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function fadeInExpandRightOnEnterAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeInExpandRightOnEnter', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':enter', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    visibility: 'hidden'
  }), ...useAnimationIncludingChildren(fadeInExpandRight(), options)]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function fadeOutCollapseOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutCollapseOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutCollapse(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
function fadeOutCollapseLeftOnLeaveAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'fadeOutCollapseLeftOnLeave', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)(':leave', [...useAnimationIncludingChildren(fadeOutCollapseLeft(), options)], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1e
    }
  })]);
}
const hueRotate = () => (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animation)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}ms {{delay}}ms', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.keyframes)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  filter: 'hue-rotate(0deg)',
  offset: 0
}), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
  filter: 'hue-rotate(-360deg)',
  offset: 1
})]))]);
const ɵ0$1f = hueRotate;
const DEFAULT_DURATION$1f = 3000;
function hueRotateAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'hueRotate', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 <=> 1', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.useAnimation)(hueRotate())]), {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1f
    }
  })]);
}
const DEFAULT_DURATION$1g = 200;
function rotateAnimation(options) {
  return (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.trigger)(options && options.anchor || 'rotate', [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('0', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    transform: 'rotate(0deg)'
  })), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.state)('1', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.style)({
    transform: 'rotate(' + '{{degrees}}' + 'deg)'
  }), {
    params: {
      degrees: options && options.degrees || 180
    }
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('0 => 1', [...(options && options.animateChildren === 'before' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : []), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + 'ease')]), ...(!options || !options.animateChildren || options.animateChildren === 'together' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])]), ...(options && options.animateChildren === 'after' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1g
    }
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.transition)('1 => 0', [...(options && options.animateChildren === 'before' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : []), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.group)([(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  }), (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animate)('{{duration}}' + 'ms ' + '{{delay}}' + 'ms ' + 'ease')]), ...(!options || !options.animateChildren || options.animateChildren === 'together' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])]), ...(options && options.animateChildren === 'after' ? [(0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.query)('@*', (0,_angular_animations__WEBPACK_IMPORTED_MODULE_0__.animateChild)(), {
    optional: true
  })] : [])], {
    params: {
      delay: options && options.delay || 0,
      duration: options && options.duration || DEFAULT_DURATION$1g
    }
  })]);
}

/**
 * Generated bundle index. Do not edit.
 */



/***/ }),

/***/ 6079:
/*!**********************************************************!*\
  !*** ./node_modules/@angular/cdk/fesm2020/clipboard.mjs ***!
  \**********************************************************/
/***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "CDK_COPY_TO_CLIPBOARD_CONFIG": () => (/* binding */ CDK_COPY_TO_CLIPBOARD_CONFIG),
/* harmony export */   "CdkCopyToClipboard": () => (/* binding */ CdkCopyToClipboard),
/* harmony export */   "Clipboard": () => (/* binding */ Clipboard),
/* harmony export */   "ClipboardModule": () => (/* binding */ ClipboardModule),
/* harmony export */   "PendingCopy": () => (/* binding */ PendingCopy)
/* harmony export */ });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);




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
 * forces a re-layout. This re-layout can take too long if the string is large,
 * causing the execCommand('copy') to happen too long after the user clicked.
 * This results in the browser refusing to copy. This object lets the
 * re-layout happen in a separate tick from copying by providing a copy function
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
    // Making the textarea `readonly` prevents the screen from jumping on iOS Safari (see #25169).
    textarea.readOnly = true;
    this._document.body.appendChild(textarea);
  }
  /** Finishes copying the text. */
  copy() {
    const textarea = this._textarea;
    let successful = false;
    try {
      // Older browsers could throw if copy is not supported.
      if (textarea) {
        const currentFocus = this._document.activeElement;
        textarea.select();
        textarea.setSelectionRange(0, textarea.value.length);
        successful = this._document.execCommand('copy');
        if (currentFocus) {
          currentFocus.focus();
        }
      }
    } catch {
      // Discard error.
      // Initial setting of {@code successful} will represent failure here.
    }
    return successful;
  }
  /** Cleans up DOM changes used to perform the copy operation. */
  destroy() {
    const textarea = this._textarea;
    if (textarea) {
      textarea.remove();
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
Clipboard.ɵfac = function Clipboard_Factory(t) {
  return new (t || Clipboard)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵinject"](_angular_common__WEBPACK_IMPORTED_MODULE_1__.DOCUMENT));
};
Clipboard.ɵprov = /* @__PURE__ */_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjectable"]({
  token: Clipboard,
  factory: Clipboard.ɵfac,
  providedIn: 'root'
});
(function () {
  (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵsetClassMetadata"](Clipboard, [{
    type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Injectable,
    args: [{
      providedIn: 'root'
    }]
  }], function () {
    return [{
      type: undefined,
      decorators: [{
        type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Inject,
        args: [_angular_common__WEBPACK_IMPORTED_MODULE_1__.DOCUMENT]
      }]
    }];
  }, null);
})();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
/** Injection token that can be used to provide the default options to `CdkCopyToClipboard`. */
const CDK_COPY_TO_CLIPBOARD_CONFIG = new _angular_core__WEBPACK_IMPORTED_MODULE_0__.InjectionToken('CDK_COPY_TO_CLIPBOARD_CONFIG');
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
    this.copied = new _angular_core__WEBPACK_IMPORTED_MODULE_0__.EventEmitter();
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
        } else {
          this._currentTimeout = null;
          this._pending.delete(pending);
          pending.destroy();
          this.copied.emit(successful);
        }
      };
      attempt();
    } else {
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
CdkCopyToClipboard.ɵfac = function CdkCopyToClipboard_Factory(t) {
  return new (t || CdkCopyToClipboard)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](Clipboard), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_0__.NgZone), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](CDK_COPY_TO_CLIPBOARD_CONFIG, 8));
};
CdkCopyToClipboard.ɵdir = /* @__PURE__ */_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineDirective"]({
  type: CdkCopyToClipboard,
  selectors: [["", "cdkCopyToClipboard", ""]],
  hostBindings: function CdkCopyToClipboard_HostBindings(rf, ctx) {
    if (rf & 1) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function CdkCopyToClipboard_click_HostBindingHandler() {
        return ctx.copy();
      });
    }
  },
  inputs: {
    text: ["cdkCopyToClipboard", "text"],
    attempts: ["cdkCopyToClipboardAttempts", "attempts"]
  },
  outputs: {
    copied: "cdkCopyToClipboardCopied"
  }
});
(function () {
  (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵsetClassMetadata"](CdkCopyToClipboard, [{
    type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Directive,
    args: [{
      selector: '[cdkCopyToClipboard]',
      host: {
        '(click)': 'copy()'
      }
    }]
  }], function () {
    return [{
      type: Clipboard
    }, {
      type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.NgZone
    }, {
      type: undefined,
      decorators: [{
        type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Optional
      }, {
        type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Inject,
        args: [CDK_COPY_TO_CLIPBOARD_CONFIG]
      }]
    }];
  }, {
    text: [{
      type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Input,
      args: ['cdkCopyToClipboard']
    }],
    attempts: [{
      type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Input,
      args: ['cdkCopyToClipboardAttempts']
    }],
    copied: [{
      type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.Output,
      args: ['cdkCopyToClipboardCopied']
    }]
  });
})();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
class ClipboardModule {}
ClipboardModule.ɵfac = function ClipboardModule_Factory(t) {
  return new (t || ClipboardModule)();
};
ClipboardModule.ɵmod = /* @__PURE__ */_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineNgModule"]({
  type: ClipboardModule
});
ClipboardModule.ɵinj = /* @__PURE__ */_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineInjector"]({});
(function () {
  (typeof ngDevMode === "undefined" || ngDevMode) && _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵsetClassMetadata"](ClipboardModule, [{
    type: _angular_core__WEBPACK_IMPORTED_MODULE_0__.NgModule,
    args: [{
      declarations: [CdkCopyToClipboard],
      exports: [CdkCopyToClipboard]
    }]
  }], null, null);
})();

/**
 * @license
 * Copyright Google LLC All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */

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



/***/ })

}]);
//# sourceMappingURL=default-src_app_git_git_module_ts-src_app_md-explorer_services_external-apps_service_ts-src_a-45d4e7.js.map