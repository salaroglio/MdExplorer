"use strict";
(self["webpackChunkclient2"] = self["webpackChunkclient2"] || []).push([["src_app_projects_projects_module_ts"],{

/***/ 5922:
/*!*****************************************************************************!*\
  !*** ./src/app/projects/dialogs/catalog-picker/catalog-picker.component.ts ***!
  \*****************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "CatalogPickerDialogComponent": () => (/* binding */ CatalogPickerDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_app_store_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../md-explorer/services/app-store.service */ 451);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/forms */ 2508);












function CatalogPickerDialogComponent_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 1, "CATALOG.LOADING_CATALOG"));
  }
}
function CatalogPickerDialogComponent_div_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r1.errorMessage);
  }
}
function CatalogPickerDialogComponent_div_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 13)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "inbox");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 1, "CATALOG.NO_APPS_FOUND"));
  }
}
function CatalogPickerDialogComponent_div_14_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 16)(1, "div", 17)(2, "mat-icon", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 19)(5, "span", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "span", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "span", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CatalogPickerDialogComponent_div_14_div_1_Template_button_click_11_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r7);
      const app_r5 = restoredCtx.$implicit;
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r6.selectApp(app_r5));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const app_r5 = ctx.$implicit;
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("already-added", ctx_r4.isAlreadyAdded(app_r5));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](app_r5.icon || "apps");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](app_r5.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](app_r5.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("v", app_r5.version, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx_r4.isAlreadyAdded(app_r5));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", ctx_r4.isAlreadyAdded(app_r5) ? _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](13, 8, "CATALOG.ALREADY_ADDED") : _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](14, 10, "CATALOG.ADD_TO_PROJECT"), " ");
  }
}
function CatalogPickerDialogComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, CatalogPickerDialogComponent_div_14_div_1_Template, 15, 12, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r3.filteredApps);
  }
}
class CatalogPickerDialogComponent {
  constructor(dialogRef, data, appStoreService, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.appStoreService = appStoreService;
    this.translate = translate;
    this.catalogApps = [];
    this.filteredApps = [];
    this.loading = true;
    this.errorMessage = '';
    this.searchText = '';
    this.existingAppIds = new Set(data.existingAppIds || []);
  }
  ngOnInit() {
    this.appStoreService.getCatalog().subscribe({
      next: catalog => {
        this.catalogApps = catalog.apps || [];
        this.filteredApps = [...this.catalogApps];
        this.loading = false;
      },
      error: err => {
        console.error('Error loading catalog:', err);
        this.errorMessage = this.translate.instant('CATALOG.FAILED_LOAD');
        this.loading = false;
      }
    });
  }
  onSearch() {
    const q = this.searchText.toLowerCase().trim();
    if (!q) {
      this.filteredApps = [...this.catalogApps];
    } else {
      this.filteredApps = this.catalogApps.filter(a => (a.name || '').toLowerCase().includes(q) || (a.description || '').toLowerCase().includes(q));
    }
  }
  isAlreadyAdded(app) {
    return this.existingAppIds.has(app.id);
  }
  selectApp(app) {
    this.dialogRef.close(app);
  }
  close() {
    this.dialogRef.close(null);
  }
  static {
    this.ɵfac = function CatalogPickerDialogComponent_Factory(t) {
      return new (t || CatalogPickerDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_app_store_service__WEBPACK_IMPORTED_MODULE_0__.AppStoreService), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({
      type: CatalogPickerDialogComponent,
      selectors: [["app-catalog-picker"]],
      decls: 19,
      vars: 14,
      consts: [["mat-dialog-title", ""], ["appearance", "outline", 2, "width", "100%", "margin-bottom", "8px"], ["matInput", "", "autocomplete", "off", 3, "ngModel", "ngModelChange"], ["matSuffix", ""], ["class", "loading-container", 4, "ngIf"], ["class", "error-message", 4, "ngIf"], ["class", "empty-state", 4, "ngIf"], ["class", "catalog-list", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "loading-container"], ["diameter", "30"], [1, "error-message"], [1, "empty-state"], [1, "catalog-list"], ["class", "catalog-item", 3, "already-added", 4, "ngFor", "ngForOf"], [1, "catalog-item"], [1, "app-info"], [1, "app-icon"], [1, "app-details"], [1, "app-name"], [1, "app-desc"], [1, "app-version"], ["mat-stroked-button", "", "color", "primary", 3, "disabled", "click"]],
      template: function CatalogPickerDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-dialog-content")(4, "mat-form-field", 1)(5, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](7, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "input", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function CatalogPickerDialogComponent_Template_input_ngModelChange_8_listener($event) {
            return ctx.searchText = $event;
          })("ngModelChange", function CatalogPickerDialogComponent_Template_input_ngModelChange_8_listener() {
            return ctx.onSearch();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-icon", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "search");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, CatalogPickerDialogComponent_div_11_Template, 5, 3, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, CatalogPickerDialogComponent_div_12_Template, 5, 1, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](13, CatalogPickerDialogComponent_div_13_Template, 6, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, CatalogPickerDialogComponent_div_14_Template, 2, 1, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-dialog-actions", 8)(16, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function CatalogPickerDialogComponent_Template_button_click_16_listener() {
            return ctx.close();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](18, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 8, "CATALOG.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](7, 10, "CATALOG.SEARCH_APPS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx.searchText);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.errorMessage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.loading && !ctx.errorMessage && ctx.filteredApps.length === 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.loading && !ctx.errorMessage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](18, 12, "COMMON.CANCEL"));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_4__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_4__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__.MatLegacyInput, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_7__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_9__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_10__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__.TranslatePipe],
      styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 24px;\n  gap: 12px;\n}\n\n.error-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 16px;\n  color: var(--mde-error-fg);\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 24px;\n  color: var(--mde-text-hint);\n  gap: 8px;\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n}\n\n.catalog-list[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-y: auto;\n}\n\n.catalog-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 12px 8px;\n  border-bottom: 1px solid var(--mde-border-color);\n}\n.catalog-item[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n.catalog-item.already-added[_ngcontent-%COMP%] {\n  opacity: 0.5;\n}\n\n.app-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 1;\n  min-width: 0;\n}\n\n.app-icon[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-blue);\n  flex-shrink: 0;\n}\n\n.app-details[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  min-width: 0;\n}\n\n.app-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  font-size: 14px;\n}\n\n.app-desc[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: var(--mde-text-secondary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.app-version[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: var(--mde-text-hint);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9jYXRhbG9nLXBpY2tlci9jYXRhbG9nLXBpY2tlci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EsMEJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsYUFBQTtFQUNBLDJCQUFBO0VBQ0EsUUFBQTtBQUNGO0FBQ0U7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFDSjs7QUFHQTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLDhCQUFBO0VBQ0EsaUJBQUE7RUFDQSxnREFBQTtBQUFGO0FBRUU7RUFDRSxtQkFBQTtBQUFKO0FBR0U7RUFDRSxZQUFBO0FBREo7O0FBS0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsT0FBQTtFQUNBLFlBQUE7QUFGRjs7QUFLQTtFQUNFLGtDQUFBO0VBQ0EsY0FBQTtBQUZGOztBQUtBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsWUFBQTtBQUZGOztBQUtBO0VBQ0UsZ0JBQUE7RUFDQSxlQUFBO0FBRkY7O0FBS0E7RUFDRSxlQUFBO0VBQ0EsZ0NBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7QUFGRjs7QUFLQTtFQUNFLGVBQUE7RUFDQSwyQkFBQTtBQUZGIiwic291cmNlc0NvbnRlbnQiOlsiLmxvYWRpbmctY29udGFpbmVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHBhZGRpbmc6IDI0cHg7XG4gIGdhcDogMTJweDtcbn1cblxuLmVycm9yLW1lc3NhZ2Uge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTZweDtcbiAgY29sb3I6IHZhcigtLW1kZS1lcnJvci1mZyk7XG59XG5cbi5lbXB0eS1zdGF0ZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDI0cHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1oaW50KTtcbiAgZ2FwOiA4cHg7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogNDhweDtcbiAgICB3aWR0aDogNDhweDtcbiAgICBoZWlnaHQ6IDQ4cHg7XG4gIH1cbn1cblxuLmNhdGFsb2ctbGlzdCB7XG4gIG1heC1oZWlnaHQ6IDQwMHB4O1xuICBvdmVyZmxvdy15OiBhdXRvO1xufVxuXG4uY2F0YWxvZy1pdGVtIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xuICBwYWRkaW5nOiAxMnB4IDhweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuXG4gICY6bGFzdC1jaGlsZCB7XG4gICAgYm9yZGVyLWJvdHRvbTogbm9uZTtcbiAgfVxuXG4gICYuYWxyZWFkeS1hZGRlZCB7XG4gICAgb3BhY2l0eTogMC41O1xuICB9XG59XG5cbi5hcHAtaW5mbyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogMTJweDtcbiAgZmxleDogMTtcbiAgbWluLXdpZHRoOiAwO1xufVxuXG4uYXBwLWljb24ge1xuICBjb2xvcjogdmFyKC0tbWRlLWljb24tYWNjZW50LWJsdWUpO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmFwcC1kZXRhaWxzIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgbWluLXdpZHRoOiAwO1xufVxuXG4uYXBwLW5hbWUge1xuICBmb250LXdlaWdodDogNTAwO1xuICBmb250LXNpemU6IDE0cHg7XG59XG5cbi5hcHAtZGVzYyB7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xufVxuXG4uYXBwLXZlcnNpb24ge1xuICBmb250LXNpemU6IDExcHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1oaW50KTtcbn1cbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

/***/ }),

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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../git/services/gitservice.service */ 7224);
/* harmony import */ var _commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../commons/waitingdialog/waiting-dialog.service */ 1394);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/forms */ 2508);




















function CloneProjectComponent_mat_hint_47_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "CLONE.PASSWORD_HINT"));
  }
}
class CloneProjectComponent {
  constructor(dialog, mdFileService, gitService, dialogRef, waitingDialog, projectService, router, translate) {
    this.dialog = dialog;
    this.mdFileService = mdFileService;
    this.gitService = gitService;
    this.dialogRef = dialogRef;
    this.waitingDialog = waitingDialog;
    this.projectService = projectService;
    this.router = router;
    this.translate = translate;
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
    info.message = this.translate.instant('CLONE.CLONING_MSG');
    this.waitingDialog.showMessageBox(info);
    this.gitService.clone(this.dataForCloning).subscribe(_ => {
      if (_.areCredentialsCorrect) {
        this.projectService.setNewFolderProject(this.dataForCloning.directoryPath);
      } else {
        const dialogRef = this.dialog.open(_git_components_git_messages_git_messages_component__WEBPACK_IMPORTED_MODULE_2__.GitMessagesComponent, {
          width: '300px',
          data: {
            message: this.translate.instant('CLONE.CREDENTIALS_WRONG')
          }
        });
      }
      this.waitingDialog.closeMessageBox();
      this.dialogRef.close(this.dataForCloning);
    });
  }
  openFileSystem() {
    let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_3__.ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('CLONE.FOLDER_TITLE');
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
  static {
    this.ɵfac = function CloneProjectComponent_Factory(t) {
      return new (t || CloneProjectComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_4__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_5__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_commons_waitingdialog_waiting_dialog_service__WEBPACK_IMPORTED_MODULE_6__.WaitingDialogService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_10__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_11__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({
      type: CloneProjectComponent,
      selectors: [["app-clone-project"]],
      decls: 56,
      vars: 50,
      consts: [["mat-dialog-title", "", 2, "display", "inline"], ["src", "/assets/gitlogo.png", 2, "display", "inline", "vertical-align", "middle"], [2, "margin-top", "10px", "margin-bottom", "10px"], [1, "vertical-form-container"], ["appearance", "outline"], ["matInput", "", "required", "", 3, "ngModel", "placeholder", "ngModelChange"], [1, "orizzontal-form-container"], ["appearance", "outline", 2, "width", "100%"], ["mat-button", "", "matSuffix", "", "color", "primary", 3, "click"], ["matInput", "", "required", "", 3, "ngModel", "placeholder", "type", "ngModelChange"], ["matSuffix", "", 3, "click"], [4, "ngIf"], [1, "store-credential"], [3, "ngModel", "ngModelChange"], ["align", "end"], ["mat-stroked-button", "", "color", "primary", 3, "click"]],
      template: function CloneProjectComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "h1", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "img", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "mat-dialog-content")(6, "mat-card", 2)(7, "div", 3)(8, "mat-form-field", 4)(9, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](10);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](11, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](12, "input", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_12_listener($event) {
            return ctx.dataForCloning.urlPath = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](13, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](14, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](16, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "div", 6)(18, "mat-form-field", 7)(19, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](22, "input", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_22_listener($event) {
            return ctx.dataForCloning.directoryPath = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](24, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](26, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](27, "button", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_27_listener() {
            return ctx.openFileSystem();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](28, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](29, "more_horiz");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](30, "mat-form-field", 4)(31, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](32);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](33, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](34, "input", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_34_listener($event) {
            return ctx.dataForCloning.username = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](35, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](36, "mat-hint");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](37);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](38, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](39, "mat-form-field", 4)(40, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](41);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](42, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](43, "input", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_input_ngModelChange_43_listener($event) {
            return ctx.dataForCloning.password = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](44, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](45, "mat-icon", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_mat_icon_click_45_listener() {
            return ctx.hide = !ctx.hide;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](46);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](47, CloneProjectComponent_mat_hint_47_Template, 3, 3, "mat-hint", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](48, "section", 12)(49, "mat-checkbox", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function CloneProjectComponent_Template_mat_checkbox_ngModelChange_49_listener($event) {
            return ctx.dataForCloning.storeCredentials = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](50);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](51, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](52, "mat-dialog-actions", 14)(53, "button", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function CloneProjectComponent_Template_button_click_53_listener() {
            return ctx.cloneDirectory();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](54);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](55, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 22, "CLONE.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](11, 24, "CLONE.URL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.urlPath)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](13, 26, "CLONE.URL_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](16, 28, "CLONE.URL_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](21, 30, "CLONE.DIRECTORY"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.directoryPath)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](23, 32, "CLONE.DIR_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](26, 34, "CLONE.DIR_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](33, 36, "CLONE.USERNAME"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.username)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](35, 38, "CLONE.USERNAME_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](38, 40, "CLONE.USERNAME_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](42, 42, "CLONE.PASSWORD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.password)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](44, 44, "CLONE.PASSWORD_PLACEHOLDER"))("type", ctx.hide ? "password" : "text");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx.hide ? "visibility_off" : "visibility");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.dataForCloning.password);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx.dataForCloning.storeCredentials);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](51, 46, "CLONE.STORE_CREDENTIALS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](55, 48, "CLONE.CLONE_BTN"));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_12__.NgIf, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_13__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_14__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_14__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_14__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_14__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_15__.MatLegacyInput, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_16__.MatLegacyCard, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_17__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__.MatIcon, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_11__.TranslatePipe],
      styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.orizzontal-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9jbG9uZS1wcm9qZWN0L2Nsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtBQUNGIiwic291cmNlc0NvbnRlbnQiOlsiLnZlcnRpY2FsLWZvcm0tY29udGFpbmVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbn1cblxuLm9yaXp6b250YWwtZm9ybS1jb250YWluZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogcm93O1xufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
    });
  }
}

/***/ }),

/***/ 1902:
/*!***********************************************************************!*\
  !*** ./src/app/projects/dialogs/p2p-manager/p2p-manager.component.ts ***!
  \***********************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "P2PManagerComponent": () => (/* binding */ P2PManagerComponent)
/* harmony export */ });
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs/operators */ 2673);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _services_p2p_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../../services/p2p.service */ 9811);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/cdk/clipboard */ 6079);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-tabs */ 2821);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/legacy-progress-bar */ 5042);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/forms */ 2508);





















function P2PManagerComponent_div_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Checking P2P service...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 9)(1, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "P2P Service Not Available");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "The P2P sharing service is not running. Make sure:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "ul")(8, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9, "You are running MdExplorer from Electron");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11, "The P2P addon is installed (check tray menu)");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "P2P Sharing is enabled in the tray menu");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "button", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_6_Template_button_click_14_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r4);
      const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r3.p2pService.checkAvailability());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, " Retry ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 38)(1, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_1_Template_div_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r19);
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r18.checkTrackerStatus());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-icon", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "div", 41)(7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, "download");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "div", 41)(12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "upload");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "div", 41)(17, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "people");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "div", 41)(22, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](23, "swap_horiz");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](25);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("tracker-connected", (ctx_r5.trackerStatus == null ? null : ctx_r5.trackerStatus.overall == null ? null : ctx_r5.trackerStatus.overall.status) === "connected")("tracker-error", (ctx_r5.trackerStatus == null ? null : ctx_r5.trackerStatus.overall == null ? null : ctx_r5.trackerStatus.overall.status) === "unauthorized" || (ctx_r5.trackerStatus == null ? null : ctx_r5.trackerStatus.overall == null ? null : ctx_r5.trackerStatus.overall.status) === "unreachable");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("color", ctx_r5.getTrackerStatusColor());
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r5.getTrackerStatusIcon());
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("Tracker: ", ctx_r5.getTrackerStatusText(), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r5.formatSpeed(ctx_r5.status.stats.downloadSpeed));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r5.formatSpeed(ctx_r5.status.stats.uploadSpeed));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("", ctx_r5.status.stats.peersConnected, " peers");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("", ctx_r5.status.stats.activeTransfers, " active");
  }
}
function P2PManagerComponent_div_7_div_2_span_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Il tracker ha rifiutato l'autenticazione. Verifica il token P2P nelle impostazioni. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
}
function P2PManagerComponent_div_7_div_2_span_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, " Il tracker non \u00E8 raggiungibile. Controlla la connessione internet. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
}
function P2PManagerComponent_div_7_div_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r23 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 42)(1, "mat-icon", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, P2PManagerComponent_div_7_div_2_span_3_Template, 2, 0, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, P2PManagerComponent_div_7_div_2_span_4_Template, 2, 0, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "button", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_2_Template_button_click_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r23);
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r22.checkTrackerStatus());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, " Riprova ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r6.trackerStatus.overall.status === "unauthorized");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r6.trackerStatus.overall.status === "unreachable");
  }
}
function P2PManagerComponent_div_7_ng_template_5_span_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r24.activeTransfers.length);
  }
}
function P2PManagerComponent_div_7_ng_template_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "sync");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, " Transfers ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, P2PManagerComponent_div_7_ng_template_5_span_3_Template, 2, 1, "span", 46);
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r7.activeTransfers.length > 0);
  }
}
function P2PManagerComponent_div_7_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 48)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "cloud_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "No active transfers");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "span", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Share a file or paste a magnet link to start");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_12_div_1_span_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
    const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" - ", ctx_r27.formatSpeed(transfer_r26.downloadSpeed), "");
  }
}
function P2PManagerComponent_div_7_div_12_div_1_span_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
    const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" - ", ctx_r28.formatSpeed(transfer_r26.uploadSpeed), " up");
  }
}
function P2PManagerComponent_div_7_div_12_div_1_span_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
    const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" - ETA: ", ctx_r29.formatEta(transfer_r26.eta), "");
  }
}
function P2PManagerComponent_div_7_div_12_div_1_button_15_Template(rf, ctx) {
  if (rf & 1) {
    const _r38 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_12_div_1_button_15_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r38);
      const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
      const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r36.copyMagnetLink(transfer_r26));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_12_div_1_button_16_Template(rf, ctx) {
  if (rf & 1) {
    const _r41 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_12_div_1_button_16_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r41);
      const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
      const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r39.pauseTransfer(transfer_r26));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "pause");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_12_div_1_button_17_Template(rf, ctx) {
  if (rf & 1) {
    const _r44 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_12_div_1_button_17_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r44);
      const transfer_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
      const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r42.resumeTransfer(transfer_r26));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "play_arrow");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_12_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r46 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 51)(1, "div", 52)(2, "mat-icon", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 53)(5, "div", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "div", 55)(8, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](10, P2PManagerComponent_div_7_div_12_div_1_span_10_Template, 2, 1, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, P2PManagerComponent_div_7_div_12_div_1_span_11_Template, 2, 1, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, P2PManagerComponent_div_7_div_12_div_1_span_12_Template, 2, 1, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](13, "mat-progress-bar", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "div", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](15, P2PManagerComponent_div_7_div_12_div_1_button_15_Template, 3, 0, "button", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](16, P2PManagerComponent_div_7_div_12_div_1_button_16_Template, 3, 0, "button", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](17, P2PManagerComponent_div_7_div_12_div_1_button_17_Template, 3, 0, "button", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "button", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_12_div_1_Template_button_click_18_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r46);
      const transfer_r26 = restoredCtx.$implicit;
      const ctx_r45 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r45.stopTransfer(transfer_r26, false));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20, "stop");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const transfer_r26 = ctx.$implicit;
    const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("color", ctx_r25.getStatusColor(transfer_r26));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r25.getStatusIcon(transfer_r26));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](transfer_r26.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate2"]("", ctx_r25.formatBytes(transfer_r26.downloaded), " / ", ctx_r25.formatBytes(transfer_r26.size), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.isDownloading);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.isSeeding);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.isDownloading && transfer_r26.eta > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("mode", transfer_r26.status === "paused" ? "buffer" : "determinate")("value", transfer_r26.progress);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.magnetUri);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.status !== "paused" && (transfer_r26.isDownloading || transfer_r26.isSeeding));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", transfer_r26.status === "paused");
  }
}
function P2PManagerComponent_div_7_div_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, P2PManagerComponent_div_7_div_12_div_1_Template, 21, 13, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r9.transfers);
  }
}
function P2PManagerComponent_div_7_ng_template_14_span_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r47 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r47.projects.length);
  }
}
function P2PManagerComponent_div_7_ng_template_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "folder_special");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, " Projects ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](3, P2PManagerComponent_div_7_ng_template_14_span_3_Template, 2, 1, "span", 46);
  }
  if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r10.projects.length > 0);
  }
}
function P2PManagerComponent_div_7_div_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](1, "mat-spinner", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "Loading projects...");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_23_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 48)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "folder_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4, "No P2P-enabled projects found");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](5, "span", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](6, "Share a file via P2P to enable a project");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_span_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span", 87);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const file_r52 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
    const ctx_r53 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r53.formatBytes(file_r52.info.size));
  }
}
function P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_span_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span", 88)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 89);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const status_r57 = ctx.ngIf;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassMap"]("status-" + status_r57.statusClass);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](status_r57.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](status_r57.text);
  }
}
function P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_button_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r60 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 90);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_button_10_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r60);
      const file_r52 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
      const ctx_r58 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r58.downloadFile(file_r52.info));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "cloud_download");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
}
function P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 79)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "insert_drive_file");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "div", 80)(4, "span", 81);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_span_6_Template, 2, 1, "span", 82);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "div", 83);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](8, P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_span_8_Template, 5, 4, "span", 84);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "div", 85);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](10, P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_button_10_Template, 3, 0, "button", 86);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const file_r52 = ctx.$implicit;
    const ctx_r51 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](file_r52.filename);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", file_r52.info == null ? null : file_r52.info.size);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r51.getFileStatus(file_r52.info == null ? null : file_r52.info.infoHash));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", (file_r52.info == null ? null : file_r52.info.magnetUri) && ctx_r51.canDownloadFile(file_r52.info == null ? null : file_r52.info.infoHash));
  }
}
function P2PManagerComponent_div_7_div_24_div_1_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 77);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, P2PManagerComponent_div_7_div_24_div_1_div_14_div_1_Template, 11, 4, "div", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]().$implicit;
    const ctx_r50 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r50.getProjectFiles(project_r49));
  }
}
function P2PManagerComponent_div_7_div_24_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r63 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 69)(1, "div", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_24_div_1_Template_div_click_1_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r63);
      const project_r49 = restoredCtx.$implicit;
      const ctx_r62 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r62.toggleProject(project_r49.id));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-icon", 71);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "span", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "span", 73);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "div", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_24_div_1_Template_div_click_10_listener($event) {
      return $event.stopPropagation();
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "button", 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_div_24_div_1_Template_button_click_11_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r63);
      const project_r49 = restoredCtx.$implicit;
      const ctx_r65 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r65.restoreSeeding(project_r49));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](13, "play_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, P2PManagerComponent_div_7_div_24_div_1_div_14_Template, 2, 1, "div", 76);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r49 = ctx.$implicit;
    const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](ctx_r48.isProjectExpanded(project_r49.id) ? "expand_more" : "chevron_right");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](project_r49.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("(", ctx_r48.getProjectFiles(project_r49).length, " files)");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r48.isProjectExpanded(project_r49.id));
  }
}
function P2PManagerComponent_div_7_div_24_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, P2PManagerComponent_div_7_div_24_div_1_Template, 15, 4, "div", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r13.projects);
  }
}
function P2PManagerComponent_div_7_ng_template_26_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "cloud_download");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, " Download ");
  }
}
function P2PManagerComponent_div_7_mat_option_36_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-option", 91)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r66 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("value", project_r66);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", project_r66.name, " ");
  }
}
function P2PManagerComponent_div_7_p_51_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "p", 92)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, " Download in: ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"]("", ctx_r16.selectedProjectForDownload.path, "/.p2pshare/received/");
  }
}
function P2PManagerComponent_div_7_ng_template_53_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, " Info ");
  }
}
function P2PManagerComponent_div_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r68 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](1, P2PManagerComponent_div_7_div_1_Template, 26, 11, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](2, P2PManagerComponent_div_7_div_2_Template, 7, 2, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "mat-tab-group", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("selectedIndexChange", function P2PManagerComponent_div_7_Template_mat_tab_group_selectedIndexChange_3_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r67 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r67.selectedTab = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, P2PManagerComponent_div_7_ng_template_5_Template, 4, 1, "ng-template", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "div", 17)(7, "div", 18)(8, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r69 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r69.refreshTransfers());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](11, P2PManagerComponent_div_7_div_11_Template, 7, 0, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](12, P2PManagerComponent_div_7_div_12_Template, 2, 1, "div", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](14, P2PManagerComponent_div_7_ng_template_14_Template, 4, 1, "ng-template", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](15, "div", 22)(16, "div", 23)(17, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](18, "P2P-Enabled Projects");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_Template_button_click_19_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r70 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r70.loadProjects());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](21, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](22, P2PManagerComponent_div_7_div_22_Template, 4, 0, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](23, P2PManagerComponent_div_7_div_23_Template, 7, 0, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](24, P2PManagerComponent_div_7_div_24_Template, 2, 1, "div", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](25, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](26, P2PManagerComponent_div_7_ng_template_26_Template, 3, 0, "ng-template", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "div", 27)(28, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](29, "Download from Magnet Link");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "p", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, "Paste a magnet link to start downloading a file shared by someone else.");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](32, "mat-form-field", 29)(33, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](34, "Destination Project");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](35, "mat-select", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function P2PManagerComponent_div_7_Template_mat_select_ngModelChange_35_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r71 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r71.selectedProjectForDownload = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](36, P2PManagerComponent_div_7_mat_option_36_Template, 4, 2, "mat-option", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](37, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](38, "folder_special");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](39, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](40, "Il file verr\u00E0 salvato in .p2pshare/received/ del progetto");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](41, "mat-form-field", 33)(42, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](43, "Magnet Link");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](44, "input", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("ngModelChange", function P2PManagerComponent_div_7_Template_input_ngModelChange_44_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r72 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r72.magnetInput = $event);
    })("keyup.enter", function P2PManagerComponent_div_7_Template_input_keyup_enter_44_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r73 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r73.startDownload());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](45, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](46, "link");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](47, "button", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_div_7_Template_button_click_47_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r68);
      const ctx_r74 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r74.startDownload());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](48, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](49, "cloud_download");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](50, " Start Download ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](51, P2PManagerComponent_div_7_p_51_Template, 6, 1, "p", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](52, "mat-tab");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](53, P2PManagerComponent_div_7_ng_template_53_Template, 3, 0, "ng-template", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](54, "div", 37)(55, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](56, "About P2P Sharing");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](57, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](58, " P2P (peer-to-peer) sharing allows you to transfer large files directly between MdExplorer users without uploading to a server. This is ideal for: ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](59, "ul")(60, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](61, "Large video files that shouldn't be in Git");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](62, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](63, "Media assets and resources");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](64, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](65, "Quick file transfers between team members");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](66, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](67, "How to share a file:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](68, "ol")(69, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](70, "Right-click on a file in the file tree");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](71, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](72, "Select \"Share via P2P\"");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](73, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](74, "Copy the generated magnet link");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](75, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](76, "Send the link to your recipient");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](77, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](78, "Privacy:");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](79, "p", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](80, " Files are transferred directly between peers. Your IP address is visible to connected peers. No files are stored on any central server. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()()()();
  }
  if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.status == null ? null : ctx_r2.status.stats);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.trackerStatus && ctx_r2.trackerStatus.overall.status !== "connected");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("selectedIndex", ctx_r2.selectedTab);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.transfers.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.transfers.length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.isLoadingProjects);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx_r2.isLoadingProjects && ctx_r2.projects.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx_r2.isLoadingProjects && ctx_r2.projects.length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r2.selectedProjectForDownload);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngForOf", ctx_r2.allProjects);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngModel", ctx_r2.magnetInput);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx_r2.magnetInput.trim() || !ctx_r2.selectedProjectForDownload);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx_r2.selectedProjectForDownload);
  }
}
class P2PManagerComponent {
  constructor(dialogRef, data, http, p2pService, snackBar, clipboard) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.http = http;
    this.p2pService = p2pService;
    this.snackBar = snackBar;
    this.clipboard = clipboard;
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.Subject();
    this.status = null;
    this.transfers = [];
    this.projects = [];
    this.allProjects = []; // All projects for download selector
    this.isLoading = true;
    this.isLoadingProjects = false;
    this.magnetInput = '';
    this.selectedTab = 1; // Default to Projects tab
    this.expandedProjects = new Set();
    this.fileStatuses = new Map();
    this.trackerStatus = null;
    this.isCheckingTracker = false;
    this.selectedProjectForDownload = null;
  }
  ngOnInit() {
    // Subscribe to status updates
    this.p2pService.status$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$)).subscribe(status => {
      this.status = status;
      this.isLoading = false;
    });
    // Subscribe to transfers
    this.p2pService.transfers$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$)).subscribe(transfers => {
      this.transfers = transfers;
    });
    // Subscribe to transfer events
    this.p2pService.transferComplete$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$)).subscribe(transfer => {
      this.snackBar.open(`Download complete: ${transfer.name}`, 'OK', {
        duration: 3000
      });
    });
    this.p2pService.transferError$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$)).subscribe(({
      error
    }) => {
      this.snackBar.open(`Transfer error: ${error}`, 'OK', {
        duration: 5000
      });
    });
    // Check availability and load data
    this.p2pService.checkAvailability();
    // Check tracker status
    this.checkTrackerStatus();
    // Load projects with P2P
    this.loadProjects();
  }
  checkTrackerStatus() {
    this.isCheckingTracker = true;
    this.p2pService.getTrackerStatus().pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$)).subscribe({
      next: status => {
        this.trackerStatus = status;
        this.isCheckingTracker = false;
      },
      error: err => {
        console.error('[P2PManager] Error checking tracker status:', err);
        this.trackerStatus = null;
        this.isCheckingTracker = false;
      }
    });
  }
  getTrackerStatusIcon() {
    if (!this.trackerStatus) return 'help_outline';
    switch (this.trackerStatus.overall.status) {
      case 'connected':
        return 'cloud_done';
      case 'unauthorized':
        return 'lock';
      case 'unreachable':
        return 'cloud_off';
      default:
        return 'help_outline';
    }
  }
  getTrackerStatusText() {
    if (this.isCheckingTracker) return 'Verifica...';
    if (!this.trackerStatus) return 'Stato sconosciuto';
    switch (this.trackerStatus.overall.status) {
      case 'connected':
        const latency = this.trackerStatus.trackers[0]?.latency;
        return latency ? `Connesso (${latency}ms)` : 'Connesso';
      case 'unauthorized':
        return 'Non autorizzato';
      case 'unreachable':
        return 'Non raggiungibile';
      default:
        return 'Stato sconosciuto';
    }
  }
  getTrackerStatusColor() {
    if (!this.trackerStatus) return '';
    switch (this.trackerStatus.overall.status) {
      case 'connected':
        return 'primary';
      case 'unauthorized':
        return 'warn';
      case 'unreachable':
        return 'warn';
      default:
        return '';
    }
  }
  loadProjects() {
    this.isLoadingProjects = true;
    // First fetch all projects, then filter for those with P2P metadata
    this.http.get('../api/MdProjects/GetProjects').pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(this.destroy$), (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_4__.switchMap)(allProjects => {
      // Store all projects for download selector
      this.allProjects = allProjects;
      // Auto-select first project if none selected
      if (!this.selectedProjectForDownload && allProjects.length > 0) {
        this.selectedProjectForDownload = allProjects[0];
      }
      // Pass all projects to the P2P service to check which have metadata.json
      const projectsToCheck = allProjects.map(p => ({
        id: p.id,
        name: p.name,
        path: p.path
      }));
      return this.p2pService.getProjectsWithP2P(projectsToCheck);
    })).subscribe({
      next: projects => {
        this.projects = projects;
        this.isLoadingProjects = false;
      },
      error: err => {
        console.error('[P2PManager] Error loading projects:', err);
        this.isLoadingProjects = false;
      }
    });
  }
  toggleProject(projectId) {
    if (this.expandedProjects.has(projectId)) {
      this.expandedProjects.delete(projectId);
    } else {
      this.expandedProjects.add(projectId);
      // Load file statuses for this project
      const project = this.projects.find(p => p.id === projectId);
      if (project) {
        this.loadFileStatuses(project);
      }
    }
  }
  loadFileStatuses(project) {
    const files = this.getProjectFiles(project);
    files.forEach(file => {
      if (file.info?.infoHash) {
        this.p2pService.getPeerStatus(file.info.infoHash).subscribe({
          next: status => {
            this.fileStatuses.set(file.info.infoHash, this.mapPeerStatusToFileStatus(status));
          },
          error: () => {
            // If error, set as unknown
            this.fileStatuses.set(file.info.infoHash, {
              icon: 'help_outline',
              text: 'Stato sconosciuto',
              statusClass: 'unknown',
              canDownload: false,
              numPeers: 0
            });
          }
        });
      }
    });
  }
  mapPeerStatusToFileStatus(status) {
    if (!status.found) {
      // Torrent not active - file might still be downloadable if peers exist elsewhere
      return {
        icon: 'cloud_off',
        text: 'Non attivo',
        statusClass: 'unavailable',
        canDownload: true,
        numPeers: 0
      };
    }
    switch (status.status) {
      case 'seeding':
        return {
          icon: 'folder',
          text: status.numPeers > 0 ? `File locale · ${status.numPeers} peer connessi` : 'File locale · In condivisione',
          statusClass: 'local',
          canDownload: false,
          numPeers: status.numPeers
        };
      case 'seeding_no_peers':
        return {
          icon: 'folder',
          text: 'File locale · In condivisione',
          statusClass: 'local',
          canDownload: false,
          numPeers: 0
        };
      case 'downloading':
        return {
          icon: 'downloading',
          text: `Download ${Math.round((status.progress || 0) * 100)}%`,
          statusClass: 'downloading',
          canDownload: false,
          numPeers: status.numPeers
        };
      case 'downloading_no_peers':
        return {
          icon: 'downloading',
          text: 'Download · Cercando peer...',
          statusClass: 'downloading',
          canDownload: false,
          numPeers: 0
        };
      case 'completed':
        return {
          icon: 'folder',
          text: 'File locale · Download completato',
          statusClass: 'local',
          canDownload: false,
          numPeers: status.numPeers
        };
      default:
        return {
          icon: 'cloud_queue',
          text: status.numPeers > 0 ? `${status.numPeers} peer disponibili` : 'Disponibile per download',
          statusClass: 'available',
          canDownload: true,
          numPeers: status.numPeers
        };
    }
  }
  getFileStatus(infoHash) {
    if (!infoHash) return null;
    return this.fileStatuses.get(infoHash) || null;
  }
  canDownloadFile(infoHash) {
    if (!infoHash) return false;
    const status = this.fileStatuses.get(infoHash);
    return status?.canDownload || false;
  }
  downloadFile(fileInfo) {
    if (!fileInfo?.magnetUri) {
      this.snackBar.open('Magnet link not available', 'OK', {
        duration: 3000
      });
      return;
    }
    this.snackBar.open('Starting download...', '', {
      duration: 2000
    });
    this.p2pService.download(fileInfo.magnetUri).subscribe({
      next: result => {
        if (result.success) {
          this.snackBar.open(`Download started: ${result.name}`, 'OK', {
            duration: 3000
          });
          this.selectedTab = 0; // Switch to Transfers tab
          this.refreshTransfers();
        } else {
          this.snackBar.open('Error: ' + result.error, 'OK', {
            duration: 3000
          });
        }
      },
      error: err => {
        this.snackBar.open('Download error: ' + err.message, 'OK', {
          duration: 3000
        });
      }
    });
  }
  isProjectExpanded(projectId) {
    return this.expandedProjects.has(projectId);
  }
  getProjectFiles(project) {
    if (!project.files || typeof project.files !== 'object') {
      return [];
    }
    return Object.entries(project.files).map(([filename, info]) => ({
      filename,
      info
    }));
  }
  restoreSeeding(project) {
    this.snackBar.open(`Restoring seeding for ${project.name}...`, '', {
      duration: 0
    });
    this.p2pService.restoreSeeding(project.path).subscribe({
      next: result => {
        this.snackBar.open(result.message, 'OK', {
          duration: 5000
        });
        this.refreshTransfers();
      },
      error: err => {
        this.snackBar.open('Error: ' + err.message, 'OK', {
          duration: 5000
        });
      }
    });
  }
  copyFileMagnet(info) {
    if (info?.magnetUri) {
      this.clipboard.copy(info.magnetUri);
      this.snackBar.open('Magnet link copied to clipboard', 'OK', {
        duration: 2000
      });
    }
  }
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
  get activeTransfers() {
    return this.transfers.filter(t => t.isDownloading);
  }
  get seedingTransfers() {
    return this.transfers.filter(t => t.isSeeding);
  }
  get completedTransfers() {
    return this.transfers.filter(t => t.progress >= 100 && !t.isSeeding);
  }
  refreshTransfers() {
    this.p2pService.refreshTransfers();
  }
  copyMagnetLink(transfer) {
    if (transfer.magnetUri) {
      this.clipboard.copy(transfer.magnetUri);
      this.snackBar.open('Magnet link copied to clipboard', 'OK', {
        duration: 2000
      });
    }
  }
  pauseTransfer(transfer) {
    this.p2pService.pauseTransfer(transfer.infoHash).subscribe({
      next: () => {
        this.snackBar.open('Transfer paused', 'OK', {
          duration: 2000
        });
        this.refreshTransfers();
      },
      error: err => {
        this.snackBar.open('Error pausing transfer: ' + err.message, 'OK', {
          duration: 3000
        });
      }
    });
  }
  resumeTransfer(transfer) {
    this.p2pService.resumeTransfer(transfer.infoHash).subscribe({
      next: () => {
        this.snackBar.open('Transfer resumed', 'OK', {
          duration: 2000
        });
        this.refreshTransfers();
      },
      error: err => {
        this.snackBar.open('Error resuming transfer: ' + err.message, 'OK', {
          duration: 3000
        });
      }
    });
  }
  stopTransfer(transfer, deleteFiles = false) {
    const action = deleteFiles ? 'remove and delete files' : 'stop';
    if (confirm(`Are you sure you want to ${action} "${transfer.name}"?`)) {
      this.p2pService.stopTransfer(transfer.infoHash, deleteFiles).subscribe({
        next: () => {
          this.snackBar.open('Transfer stopped', 'OK', {
            duration: 2000
          });
          this.refreshTransfers();
        },
        error: err => {
          this.snackBar.open('Error stopping transfer: ' + err.message, 'OK', {
            duration: 3000
          });
        }
      });
    }
  }
  startDownload() {
    if (!this.magnetInput.trim()) {
      this.snackBar.open('Please enter a magnet link', 'OK', {
        duration: 2000
      });
      return;
    }
    if (!this.selectedProjectForDownload) {
      this.snackBar.open('Please select a destination project', 'OK', {
        duration: 2000
      });
      return;
    }
    // Build destination path: projectPath/.p2pshare/received
    const destPath = `${this.selectedProjectForDownload.path}/.p2pshare/received`;
    this.p2pService.download(this.magnetInput.trim(), destPath).subscribe({
      next: result => {
        if (result.success) {
          this.snackBar.open(`Download started: ${result.name}`, 'OK', {
            duration: 3000
          });
          this.magnetInput = '';
          this.selectedTab = 0; // Switch to Transfers tab
          this.refreshTransfers();
        } else {
          this.snackBar.open('Error starting download: ' + result.error, 'OK', {
            duration: 3000
          });
        }
      },
      error: err => {
        this.snackBar.open('Error starting download: ' + err.message, 'OK', {
          duration: 3000
        });
      }
    });
  }
  formatBytes(bytes) {
    return this.p2pService.formatBytes(bytes);
  }
  formatSpeed(bytesPerSecond) {
    return this.p2pService.formatSpeed(bytesPerSecond);
  }
  formatEta(seconds) {
    return this.p2pService.formatEta(seconds);
  }
  getStatusIcon(transfer) {
    if (transfer.status === 'paused') return 'pause';
    if (transfer.isSeeding) return 'cloud_upload';
    if (transfer.isDownloading) return 'cloud_download';
    if (transfer.progress >= 100) return 'check_circle';
    return 'hourglass_empty';
  }
  getStatusColor(transfer) {
    if (transfer.status === 'paused') return 'warn';
    if (transfer.isSeeding) return 'accent';
    if (transfer.isDownloading) return 'primary';
    if (transfer.progress >= 100) return 'primary';
    return '';
  }
  close() {
    this.dialogRef.close();
  }
  static {
    this.ɵfac = function P2PManagerComponent_Factory(t) {
      return new (t || P2PManagerComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_6__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_services_p2p_service__WEBPACK_IMPORTED_MODULE_0__.P2PService), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_7__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_8__.Clipboard));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({
      type: P2PManagerComponent,
      selectors: [["app-p2p-manager"]],
      decls: 11,
      vars: 3,
      consts: [["mat-dialog-title", ""], ["mat-dialog-content", "", 1, "p2p-content"], ["class", "loading-container", 4, "ngIf"], ["class", "not-available", 4, "ngIf"], ["class", "p2p-available", 4, "ngIf"], ["mat-dialog-actions", "", "align", "end"], ["mat-button", "", 3, "click"], [1, "loading-container"], ["diameter", "40"], [1, "not-available"], [1, "warning-icon"], ["mat-stroked-button", "", "color", "primary", 3, "click"], [1, "p2p-available"], ["class", "stats-bar", 4, "ngIf"], ["class", "tracker-warning", 4, "ngIf"], [3, "selectedIndex", "selectedIndexChange"], ["mat-tab-label", ""], [1, "tab-content"], [1, "transfers-header"], ["mat-icon-button", "", "matTooltip", "Refresh", 3, "click"], ["class", "empty-state", 4, "ngIf"], ["class", "transfer-list", 4, "ngIf"], [1, "tab-content", "projects-tab"], [1, "projects-header"], ["mat-icon-button", "", "matTooltip", "Refresh Projects", 3, "click"], ["class", "loading-state", 4, "ngIf"], ["class", "project-list", 4, "ngIf"], [1, "tab-content", "download-tab"], [1, "hint"], ["appearance", "outline", 1, "project-selector"], ["required", "", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["matPrefix", ""], ["appearance", "outline", 1, "magnet-input"], ["matInput", "", "placeholder", "magnet:?xt=urn:btih:...", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["class", "download-path-preview", 4, "ngIf"], [1, "tab-content", "info-tab"], [1, "stats-bar"], ["matTooltip", "Click per aggiornare lo stato del tracker", 1, "stat-item", "tracker-status", 3, "click"], [3, "color"], [1, "stat-item"], [1, "tracker-warning"], ["color", "warn"], [4, "ngIf"], ["mat-button", "", "color", "primary", 3, "click"], ["class", "badge", 4, "ngIf"], [1, "badge"], [1, "empty-state"], [1, "transfer-list"], ["class", "transfer-item", 4, "ngFor", "ngForOf"], [1, "transfer-item"], [1, "transfer-icon"], [1, "transfer-info"], [1, "transfer-name"], [1, "transfer-details"], [3, "mode", "value"], [1, "transfer-actions"], ["mat-icon-button", "", "matTooltip", "Copy Magnet Link", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "Pause", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "Resume", 3, "click", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "Stop", "color", "warn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Copy Magnet Link", 3, "click"], ["mat-icon-button", "", "matTooltip", "Pause", 3, "click"], ["mat-icon-button", "", "matTooltip", "Resume", 3, "click"], [1, "loading-state"], ["diameter", "30"], [1, "project-list"], ["class", "project-card", 4, "ngFor", "ngForOf"], [1, "project-card"], [1, "project-header", 3, "click"], ["color", "primary"], [1, "project-name"], [1, "file-count"], [1, "project-actions", 3, "click"], ["mat-icon-button", "", "matTooltip", "Restore seeding for all files", "color", "primary", 3, "click"], ["class", "project-files", 4, "ngIf"], [1, "project-files"], ["class", "file-item", 4, "ngFor", "ngForOf"], [1, "file-item"], [1, "file-info"], [1, "file-name"], ["class", "file-size", 4, "ngIf"], [1, "file-status"], ["class", "peer-count", 4, "ngIf"], [1, "file-actions"], ["mat-icon-button", "", "matTooltip", "Download file", "color", "primary", 3, "click", 4, "ngIf"], [1, "file-size"], [1, "peer-count"], [1, "status-text"], ["mat-icon-button", "", "matTooltip", "Download file", "color", "primary", 3, "click"], [3, "value"], [1, "download-path-preview"]],
      template: function P2PManagerComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "h1", 0)(1, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "share");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, " P2P File Sharing\n");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "div", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, P2PManagerComponent_div_5_Template, 4, 0, "div", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, P2PManagerComponent_div_6_Template, 18, 0, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](7, P2PManagerComponent_div_7_Template, 81, 13, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "div", 5)(9, "button", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function P2PManagerComponent_Template_button_click_9_listener() {
            return ctx.close();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "Close");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.isLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && !(ctx.status == null ? null : ctx.status.enabled));
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.isLoading && (ctx.status == null ? null : ctx.status.enabled));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_9__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_9__.NgIf, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_10__.MatLegacyOption, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_11__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_11__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_11__.MatLegacyLabel, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_11__.MatLegacyPrefix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_12__.MatLegacyInput, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_13__.MatLegacySelect, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_14__.MatLegacyTabGroup, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_14__.MatLegacyTabLabel, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_14__.MatLegacyTab, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_15__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_16__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_17__.MatLegacyProgressSpinner, _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_18__.MatLegacyProgressBar, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_19__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.NgModel],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n}\n\nh1[mat-dialog-title][_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\nh1[mat-dialog-title][_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-blue);\n}\n\n.p2p-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  min-height: 400px;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  gap: 16px;\n}\n.loading-container[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n}\n\n.not-available[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 32px;\n  text-align: center;\n}\n.not-available[_ngcontent-%COMP%]   .warning-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  color: var(--mde-icon-accent-orange);\n  margin-bottom: 16px;\n}\n.not-available[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n}\n.not-available[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  margin-bottom: 16px;\n}\n.not-available[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%] {\n  text-align: left;\n  margin-bottom: 24px;\n}\n.not-available[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  margin-bottom: 4px;\n}\n\n.stats-bar[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 24px;\n  padding: 12px 16px;\n  background: var(--mde-bg-tertiary);\n  border-radius: 8px;\n  margin-bottom: 16px;\n  flex-wrap: wrap;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  font-size: 13px;\n  color: var(--mde-text-secondary);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: var(--mde-icon-accent-blue);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status[_ngcontent-%COMP%] {\n  cursor: pointer;\n  padding: 4px 8px;\n  margin: -4px;\n  border-radius: 4px;\n  transition: background-color 0.2s;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status[_ngcontent-%COMP%]:hover {\n  background: var(--mde-hover-overlay);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status.tracker-connected[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-green);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status.tracker-error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-error-fg);\n}\n\n.tracker-warning[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px 16px;\n  background: var(--mde-warning-bg);\n  border: 1px solid var(--mde-warning-fg);\n  border-radius: 8px;\n  margin-bottom: 16px;\n  font-size: 13px;\n}\n.tracker-warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.tracker-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  flex: 1;\n  color: var(--mde-warning-fg);\n}\n.tracker-warning[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 16px 0;\n  min-height: 280px;\n}\n\n.transfers-header[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  margin-bottom: 8px;\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 48px;\n  color: var(--mde-text-hint);\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  width: 64px;\n  height: 64px;\n  margin-bottom: 16px;\n  opacity: 0.5;\n}\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 16px;\n}\n.empty-state[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  margin-top: 8px;\n}\n\n.transfer-list[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n\n.transfer-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px;\n  background: var(--mde-bg-secondary);\n  border-radius: 8px;\n  border: 1px solid var(--mde-border-color);\n}\n.transfer-item[_ngcontent-%COMP%]:hover {\n  background: var(--mde-bg-hover);\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-icon[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-icon[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  margin-bottom: 4px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-details[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: var(--mde-text-secondary);\n  margin-bottom: 8px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-details[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]:not(:last-child)::after {\n  content: \"\";\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   mat-progress-bar[_ngcontent-%COMP%] {\n  height: 6px;\n  border-radius: 3px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 4px;\n  flex-shrink: 0;\n}\n\n.download-tab[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n}\n.download-tab[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  font-size: 13px;\n  margin-bottom: 24px;\n}\n.download-tab[_ngcontent-%COMP%]   .project-selector[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n.download-tab[_ngcontent-%COMP%]   .magnet-input[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 16px;\n  padding: 12px;\n  background: var(--mde-success-bg);\n  border-radius: 4px;\n  font-size: 13px;\n  color: var(--mde-success-fg);\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  background: var(--mde-hover-overlay);\n  padding: 2px 6px;\n  border-radius: 3px;\n  font-family: monospace;\n  font-size: 12px;\n}\n\n.info-tab[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 16px 0;\n}\n.info-tab[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 24px 0 8px 0;\n}\n.info-tab[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  line-height: 1.6;\n}\n.info-tab[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%], .info-tab[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  padding-left: 24px;\n}\n.info-tab[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%]   li[_ngcontent-%COMP%], .info-tab[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n.info-tab[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: var(--mde-text-hint);\n}\n\n.projects-tab[_ngcontent-%COMP%]   .projects-header[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  margin-bottom: 16px;\n}\n.projects-tab[_ngcontent-%COMP%]   .projects-header[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .loading-state[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 12px;\n  padding: 32px;\n  color: var(--mde-text-secondary);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-list[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%] {\n  border: 1px solid var(--mde-border-color);\n  border-radius: 8px;\n  overflow: hidden;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px 16px;\n  background: var(--mde-bg-tertiary);\n  cursor: pointer;\n  transition: background-color 0.2s;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]:hover {\n  background: var(--mde-bg-hover);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  flex: 1;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .file-count[_ngcontent-%COMP%] {\n  color: var(--mde-text-hint);\n  font-size: 13px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%] {\n  padding: 8px;\n  background: var(--mde-bg-secondary);\n  border-top: 1px solid var(--mde-border-color);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 8px 12px;\n  border-radius: 4px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]:hover {\n  background: var(--mde-bg-hover);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-icon-color);\n  flex-shrink: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%]   .file-name[_ngcontent-%COMP%] {\n  display: block;\n  font-size: 14px;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%]   .file-size[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: var(--mde-text-hint);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  margin-right: 8px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 12px;\n  color: var(--mde-text-secondary);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-local[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-green);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-seeding[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-blue);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-downloading[_ngcontent-%COMP%] {\n  color: #17a2b8;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-completed[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-green);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-available[_ngcontent-%COMP%] {\n  color: var(--mde-icon-accent-orange);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-unavailable[_ngcontent-%COMP%] {\n  color: var(--mde-icon-color);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-unknown[_ngcontent-%COMP%] {\n  color: var(--mde-icon-color);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   .status-text[_ngcontent-%COMP%] {\n  white-space: nowrap;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-actions[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.badge[_ngcontent-%COMP%] {\n  background: var(--mde-accent-primary);\n  color: var(--mde-text-on-primary);\n  font-size: 11px;\n  padding: 2px 6px;\n  border-radius: 10px;\n  margin-left: 8px;\n}\n\n  .mat-tab-label mat-icon {\n  margin-right: 8px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wMnAtbWFuYWdlci9wMnAtbWFuYWdlci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGNBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFDRjtBQUNFO0VBQ0Usa0NBQUE7QUFDSjs7QUFHQTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0VBQ0EsU0FBQTtBQUFGO0FBRUU7RUFDRSxnQ0FBQTtBQUFKOztBQUlBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0Esa0JBQUE7QUFERjtBQUdFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0Esb0NBQUE7RUFDQSxtQkFBQTtBQURKO0FBSUU7RUFDRSxpQkFBQTtBQUZKO0FBS0U7RUFDRSxnQ0FBQTtFQUNBLG1CQUFBO0FBSEo7QUFNRTtFQUNFLGdCQUFBO0VBQ0EsbUJBQUE7QUFKSjtBQU1JO0VBQ0UsZ0NBQUE7RUFDQSxrQkFBQTtBQUpOOztBQVNBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxrQkFBQTtFQUNBLGtDQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGVBQUE7QUFORjtBQVFFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7RUFDQSxnQ0FBQTtBQU5KO0FBUUk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxrQ0FBQTtBQU5OO0FBU0k7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxpQ0FBQTtBQVBOO0FBU007RUFDRSxvQ0FBQTtBQVBSO0FBVU07RUFDRSxtQ0FBQTtBQVJSO0FBV007RUFDRSwwQkFBQTtBQVRSOztBQWVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsaUNBQUE7RUFDQSx1Q0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxlQUFBO0FBWkY7QUFjRTtFQUNFLGNBQUE7QUFaSjtBQWVFO0VBQ0UsT0FBQTtFQUNBLDRCQUFBO0FBYko7QUFnQkU7RUFDRSxjQUFBO0FBZEo7O0FBa0JBO0VBQ0UsZUFBQTtFQUNBLGlCQUFBO0FBZkY7O0FBa0JBO0VBQ0UsYUFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7QUFmRjs7QUFrQkE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLDJCQUFBO0FBZkY7QUFpQkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7QUFmSjtBQWtCRTtFQUNFLFNBQUE7RUFDQSxlQUFBO0FBaEJKO0FBbUJFO0VBQ0UsZUFBQTtFQUNBLGVBQUE7QUFqQko7O0FBcUJBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQWxCRjs7QUFxQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsYUFBQTtFQUNBLG1DQUFBO0VBQ0Esa0JBQUE7RUFDQSx5Q0FBQTtBQWxCRjtBQW9CRTtFQUNFLCtCQUFBO0FBbEJKO0FBcUJFO0VBQ0UsY0FBQTtBQW5CSjtBQXFCSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQW5CTjtBQXVCRTtFQUNFLE9BQUE7RUFDQSxZQUFBO0FBckJKO0FBdUJJO0VBQ0UsZ0JBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxrQkFBQTtBQXJCTjtBQXdCSTtFQUNFLGVBQUE7RUFDQSxnQ0FBQTtFQUNBLGtCQUFBO0FBdEJOO0FBeUJRO0VBQ0UsV0FBQTtBQXZCVjtBQTRCSTtFQUNFLFdBQUE7RUFDQSxrQkFBQTtBQTFCTjtBQThCRTtFQUNFLGFBQUE7RUFDQSxRQUFBO0VBQ0EsY0FBQTtBQTVCSjs7QUFpQ0U7RUFDRSxpQkFBQTtBQTlCSjtBQWlDRTtFQUNFLGdDQUFBO0VBQ0EsZUFBQTtFQUNBLG1CQUFBO0FBL0JKO0FBa0NFO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBaENKO0FBbUNFO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBakNKO0FBb0NFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLGlDQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EsNEJBQUE7QUFsQ0o7QUFvQ0k7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFsQ047QUFxQ0k7RUFDRSxvQ0FBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxzQkFBQTtFQUNBLGVBQUE7QUFuQ047O0FBeUNFO0VBQ0Usa0JBQUE7QUF0Q0o7QUF5Q0U7RUFDRSxvQkFBQTtBQXZDSjtBQTBDRTtFQUNFLGdDQUFBO0VBQ0EsZ0JBQUE7QUF4Q0o7QUEyQ0U7RUFDRSxnQ0FBQTtFQUNBLGtCQUFBO0FBekNKO0FBMkNJO0VBQ0Usa0JBQUE7QUF6Q047QUE2Q0U7RUFDRSxlQUFBO0VBQ0EsMkJBQUE7QUEzQ0o7O0FBZ0RFO0VBQ0UsYUFBQTtFQUNBLDhCQUFBO0VBQ0EsbUJBQUE7RUFDQSxtQkFBQTtBQTdDSjtBQStDSTtFQUNFLFNBQUE7QUE3Q047QUFpREU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EsZ0NBQUE7QUEvQ0o7QUFrREU7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0FBaERKO0FBbURFO0VBQ0UseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBakRKO0FBbURJO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGtCQUFBO0VBQ0Esa0NBQUE7RUFDQSxlQUFBO0VBQ0EsaUNBQUE7QUFqRE47QUFtRE07RUFDRSwrQkFBQTtBQWpEUjtBQW9ETTtFQUNFLGdCQUFBO0VBQ0EsT0FBQTtBQWxEUjtBQXFETTtFQUNFLDJCQUFBO0VBQ0EsZUFBQTtBQW5EUjtBQXNETTtFQUNFLGlCQUFBO0FBcERSO0FBd0RJO0VBQ0UsWUFBQTtFQUNBLG1DQUFBO0VBQ0EsNkNBQUE7QUF0RE47QUF3RE07RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtBQXREUjtBQXdEUTtFQUNFLCtCQUFBO0FBdERWO0FBeURRO0VBQ0UsNEJBQUE7RUFDQSxjQUFBO0FBdkRWO0FBMERRO0VBQ0UsT0FBQTtFQUNBLFlBQUE7QUF4RFY7QUEwRFU7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtBQXhEWjtBQTJEVTtFQUNFLGVBQUE7RUFDQSwyQkFBQTtBQXpEWjtBQTZEUTtFQUNFLGNBQUE7RUFDQSxpQkFBQTtBQTNEVjtBQTZEVTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsZ0NBQUE7QUEzRFo7QUE2RFk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUEzRGQ7QUE2RGM7RUFDRSxtQ0FBQTtBQTNEaEI7QUE2RGM7RUFDRSxrQ0FBQTtBQTNEaEI7QUE2RGM7RUFDRSxjQUFBO0FBM0RoQjtBQTZEYztFQUNFLG1DQUFBO0FBM0RoQjtBQTZEYztFQUNFLG9DQUFBO0FBM0RoQjtBQTZEYztFQUNFLDRCQUFBO0FBM0RoQjtBQTZEYztFQUNFLDRCQUFBO0FBM0RoQjtBQStEWTtFQUNFLG1CQUFBO0FBN0RkO0FBa0VRO0VBQ0UsY0FBQTtBQWhFVjs7QUF1RUE7RUFDRSxxQ0FBQTtFQUNBLGlDQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtBQXBFRjs7QUF3RUU7RUFDRSxpQkFBQTtBQXJFSiIsInNvdXJjZXNDb250ZW50IjpbIjpob3N0IHtcbiAgZGlzcGxheTogYmxvY2s7XG59XG5cbmgxW21hdC1kaWFsb2ctdGl0bGVdIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG5cbiAgbWF0LWljb24ge1xuICAgIGNvbG9yOiB2YXIoLS1tZGUtaWNvbi1hY2NlbnQtYmx1ZSk7XG4gIH1cbn1cblxuLnAycC1jb250ZW50IHtcbiAgbWluLXdpZHRoOiA1MDBweDtcbiAgbWluLWhlaWdodDogNDAwcHg7XG59XG5cbi5sb2FkaW5nLWNvbnRhaW5lciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICBwYWRkaW5nOiA0MHB4O1xuICBnYXA6IDE2cHg7XG5cbiAgcCB7XG4gICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gIH1cbn1cblxuLm5vdC1hdmFpbGFibGUge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBwYWRkaW5nOiAzMnB4O1xuICB0ZXh0LWFsaWduOiBjZW50ZXI7XG5cbiAgLndhcm5pbmctaWNvbiB7XG4gICAgZm9udC1zaXplOiA0OHB4O1xuICAgIHdpZHRoOiA0OHB4O1xuICAgIGhlaWdodDogNDhweDtcbiAgICBjb2xvcjogdmFyKC0tbWRlLWljb24tYWNjZW50LW9yYW5nZSk7XG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcbiAgfVxuXG4gIGgzIHtcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcbiAgfVxuXG4gIHAge1xuICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XG4gIH1cblxuICB1bCB7XG4gICAgdGV4dC1hbGlnbjogbGVmdDtcbiAgICBtYXJnaW4tYm90dG9tOiAyNHB4O1xuXG4gICAgbGkge1xuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gICAgICBtYXJnaW4tYm90dG9tOiA0cHg7XG4gICAgfVxuICB9XG59XG5cbi5zdGF0cy1iYXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBnYXA6IDI0cHg7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXRlcnRpYXJ5KTtcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xuICBmbGV4LXdyYXA6IHdyYXA7XG5cbiAgLnN0YXQtaXRlbSB7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgIGdhcDogNnB4O1xuICAgIGZvbnQtc2l6ZTogMTNweDtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgIHdpZHRoOiAxOHB4O1xuICAgICAgaGVpZ2h0OiAxOHB4O1xuICAgICAgY29sb3I6IHZhcigtLW1kZS1pY29uLWFjY2VudC1ibHVlKTtcbiAgICB9XG5cbiAgICAmLnRyYWNrZXItc3RhdHVzIHtcbiAgICAgIGN1cnNvcjogcG9pbnRlcjtcbiAgICAgIHBhZGRpbmc6IDRweCA4cHg7XG4gICAgICBtYXJnaW46IC00cHg7XG4gICAgICBib3JkZXItcmFkaXVzOiA0cHg7XG4gICAgICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kLWNvbG9yIDAuMnM7XG5cbiAgICAgICY6aG92ZXIge1xuICAgICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtaG92ZXItb3ZlcmxheSk7XG4gICAgICB9XG5cbiAgICAgICYudHJhY2tlci1jb25uZWN0ZWQgbWF0LWljb24ge1xuICAgICAgICBjb2xvcjogdmFyKC0tbWRlLWljb24tYWNjZW50LWdyZWVuKTtcbiAgICAgIH1cblxuICAgICAgJi50cmFja2VyLWVycm9yIG1hdC1pY29uIHtcbiAgICAgICAgY29sb3I6IHZhcigtLW1kZS1lcnJvci1mZyk7XG4gICAgICB9XG4gICAgfVxuICB9XG59XG5cbi50cmFja2VyLXdhcm5pbmcge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDEycHg7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLXdhcm5pbmctYmcpO1xuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtd2FybmluZy1mZyk7XG4gIGJvcmRlci1yYWRpdXM6IDhweDtcbiAgbWFyZ2luLWJvdHRvbTogMTZweDtcbiAgZm9udC1zaXplOiAxM3B4O1xuXG4gIG1hdC1pY29uIHtcbiAgICBmbGV4LXNocmluazogMDtcbiAgfVxuXG4gIHNwYW4ge1xuICAgIGZsZXg6IDE7XG4gICAgY29sb3I6IHZhcigtLW1kZS13YXJuaW5nLWZnKTtcbiAgfVxuXG4gIGJ1dHRvbiB7XG4gICAgZmxleC1zaHJpbms6IDA7XG4gIH1cbn1cblxuLnRhYi1jb250ZW50IHtcbiAgcGFkZGluZzogMTZweCAwO1xuICBtaW4taGVpZ2h0OiAyODBweDtcbn1cblxuLnRyYW5zZmVycy1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBqdXN0aWZ5LWNvbnRlbnQ6IGZsZXgtZW5kO1xuICBtYXJnaW4tYm90dG9tOiA4cHg7XG59XG5cbi5lbXB0eS1zdGF0ZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICBwYWRkaW5nOiA0OHB4O1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtaGludCk7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogNjRweDtcbiAgICB3aWR0aDogNjRweDtcbiAgICBoZWlnaHQ6IDY0cHg7XG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcbiAgICBvcGFjaXR5OiAwLjU7XG4gIH1cblxuICBwIHtcbiAgICBtYXJnaW46IDA7XG4gICAgZm9udC1zaXplOiAxNnB4O1xuICB9XG5cbiAgLmhpbnQge1xuICAgIGZvbnQtc2l6ZTogMTNweDtcbiAgICBtYXJnaW4tdG9wOiA4cHg7XG4gIH1cbn1cblxuLnRyYW5zZmVyLWxpc3Qge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDEycHg7XG59XG5cbi50cmFuc2Zlci1pdGVtIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiAxMnB4O1xuICBwYWRkaW5nOiAxMnB4O1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcblxuICAmOmhvdmVyIHtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctaG92ZXIpO1xuICB9XG5cbiAgLnRyYW5zZmVyLWljb24ge1xuICAgIGZsZXgtc2hyaW5rOiAwO1xuXG4gICAgbWF0LWljb24ge1xuICAgICAgZm9udC1zaXplOiAyOHB4O1xuICAgICAgd2lkdGg6IDI4cHg7XG4gICAgICBoZWlnaHQ6IDI4cHg7XG4gICAgfVxuICB9XG5cbiAgLnRyYW5zZmVyLWluZm8ge1xuICAgIGZsZXg6IDE7XG4gICAgbWluLXdpZHRoOiAwO1xuXG4gICAgLnRyYW5zZmVyLW5hbWUge1xuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcbiAgICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xuICAgICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XG4gICAgICBtYXJnaW4tYm90dG9tOiA0cHg7XG4gICAgfVxuXG4gICAgLnRyYW5zZmVyLWRldGFpbHMge1xuICAgICAgZm9udC1zaXplOiAxMnB4O1xuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gICAgICBtYXJnaW4tYm90dG9tOiA4cHg7XG5cbiAgICAgIHNwYW4ge1xuICAgICAgICAmOm5vdCg6bGFzdC1jaGlsZCk6OmFmdGVyIHtcbiAgICAgICAgICBjb250ZW50OiAnJztcbiAgICAgICAgfVxuICAgICAgfVxuICAgIH1cblxuICAgIG1hdC1wcm9ncmVzcy1iYXIge1xuICAgICAgaGVpZ2h0OiA2cHg7XG4gICAgICBib3JkZXItcmFkaXVzOiAzcHg7XG4gICAgfVxuICB9XG5cbiAgLnRyYW5zZmVyLWFjdGlvbnMge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgZ2FwOiA0cHg7XG4gICAgZmxleC1zaHJpbms6IDA7XG4gIH1cbn1cblxuLmRvd25sb2FkLXRhYiB7XG4gIGgzIHtcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcbiAgfVxuXG4gIC5oaW50IHtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICBmb250LXNpemU6IDEzcHg7XG4gICAgbWFyZ2luLWJvdHRvbTogMjRweDtcbiAgfVxuXG4gIC5wcm9qZWN0LXNlbGVjdG9yIHtcbiAgICB3aWR0aDogMTAwJTtcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xuICB9XG5cbiAgLm1hZ25ldC1pbnB1dCB7XG4gICAgd2lkdGg6IDEwMCU7XG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcbiAgfVxuXG4gIC5kb3dubG9hZC1wYXRoLXByZXZpZXcge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBnYXA6IDhweDtcbiAgICBtYXJnaW4tdG9wOiAxNnB4O1xuICAgIHBhZGRpbmc6IDEycHg7XG4gICAgYmFja2dyb3VuZDogdmFyKC0tbWRlLXN1Y2Nlc3MtYmcpO1xuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgICBmb250LXNpemU6IDEzcHg7XG4gICAgY29sb3I6IHZhcigtLW1kZS1zdWNjZXNzLWZnKTtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgIHdpZHRoOiAxOHB4O1xuICAgICAgaGVpZ2h0OiAxOHB4O1xuICAgIH1cblxuICAgIGNvZGUge1xuICAgICAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWhvdmVyLW92ZXJsYXkpO1xuICAgICAgcGFkZGluZzogMnB4IDZweDtcbiAgICAgIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgICAgIGZvbnQtZmFtaWx5OiBtb25vc3BhY2U7XG4gICAgICBmb250LXNpemU6IDEycHg7XG4gICAgfVxuICB9XG59XG5cbi5pbmZvLXRhYiB7XG4gIGgzIHtcbiAgICBtYXJnaW46IDAgMCAxNnB4IDA7XG4gIH1cblxuICBoNCB7XG4gICAgbWFyZ2luOiAyNHB4IDAgOHB4IDA7XG4gIH1cblxuICBwIHtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICBsaW5lLWhlaWdodDogMS42O1xuICB9XG5cbiAgdWwsIG9sIHtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICBwYWRkaW5nLWxlZnQ6IDI0cHg7XG5cbiAgICBsaSB7XG4gICAgICBtYXJnaW4tYm90dG9tOiA4cHg7XG4gICAgfVxuICB9XG5cbiAgLmhpbnQge1xuICAgIGZvbnQtc2l6ZTogMTNweDtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtaGludCk7XG4gIH1cbn1cblxuLnByb2plY3RzLXRhYiB7XG4gIC5wcm9qZWN0cy1oZWFkZXIge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gICAgbWFyZ2luLWJvdHRvbTogMTZweDtcblxuICAgIGgzIHtcbiAgICAgIG1hcmdpbjogMDtcbiAgICB9XG4gIH1cblxuICAubG9hZGluZy1zdGF0ZSB7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICAgIGdhcDogMTJweDtcbiAgICBwYWRkaW5nOiAzMnB4O1xuICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICB9XG5cbiAgLnByb2plY3QtbGlzdCB7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICAgIGdhcDogMTJweDtcbiAgfVxuXG4gIC5wcm9qZWN0LWNhcmQge1xuICAgIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICAgIGJvcmRlci1yYWRpdXM6IDhweDtcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xuXG4gICAgLnByb2plY3QtaGVhZGVyIHtcbiAgICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgICAgZ2FwOiA4cHg7XG4gICAgICBwYWRkaW5nOiAxMnB4IDE2cHg7XG4gICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctdGVydGlhcnkpO1xuICAgICAgY3Vyc29yOiBwb2ludGVyO1xuICAgICAgdHJhbnNpdGlvbjogYmFja2dyb3VuZC1jb2xvciAwLjJzO1xuXG4gICAgICAmOmhvdmVyIHtcbiAgICAgICAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLWhvdmVyKTtcbiAgICAgIH1cblxuICAgICAgLnByb2plY3QtbmFtZSB7XG4gICAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XG4gICAgICAgIGZsZXg6IDE7XG4gICAgICB9XG5cbiAgICAgIC5maWxlLWNvdW50IHtcbiAgICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LWhpbnQpO1xuICAgICAgICBmb250LXNpemU6IDEzcHg7XG4gICAgICB9XG5cbiAgICAgIC5wcm9qZWN0LWFjdGlvbnMge1xuICAgICAgICBtYXJnaW4tbGVmdDogYXV0bztcbiAgICAgIH1cbiAgICB9XG5cbiAgICAucHJvamVjdC1maWxlcyB7XG4gICAgICBwYWRkaW5nOiA4cHg7XG4gICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgICAgIGJvcmRlci10b3A6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcblxuICAgICAgLmZpbGUtaXRlbSB7XG4gICAgICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gICAgICAgIGdhcDogMTJweDtcbiAgICAgICAgcGFkZGluZzogOHB4IDEycHg7XG4gICAgICAgIGJvcmRlci1yYWRpdXM6IDRweDtcblxuICAgICAgICAmOmhvdmVyIHtcbiAgICAgICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctaG92ZXIpO1xuICAgICAgICB9XG5cbiAgICAgICAgbWF0LWljb24ge1xuICAgICAgICAgIGNvbG9yOiB2YXIoLS1tZGUtaWNvbi1jb2xvcik7XG4gICAgICAgICAgZmxleC1zaHJpbms6IDA7XG4gICAgICAgIH1cblxuICAgICAgICAuZmlsZS1pbmZvIHtcbiAgICAgICAgICBmbGV4OiAxO1xuICAgICAgICAgIG1pbi13aWR0aDogMDtcblxuICAgICAgICAgIC5maWxlLW5hbWUge1xuICAgICAgICAgICAgZGlzcGxheTogYmxvY2s7XG4gICAgICAgICAgICBmb250LXNpemU6IDE0cHg7XG4gICAgICAgICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICAgICAgICAgICAgb3ZlcmZsb3c6IGhpZGRlbjtcbiAgICAgICAgICAgIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xuICAgICAgICAgIH1cblxuICAgICAgICAgIC5maWxlLXNpemUge1xuICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4O1xuICAgICAgICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LWhpbnQpO1xuICAgICAgICAgIH1cbiAgICAgICAgfVxuXG4gICAgICAgIC5maWxlLXN0YXR1cyB7XG4gICAgICAgICAgZmxleC1zaHJpbms6IDA7XG4gICAgICAgICAgbWFyZ2luLXJpZ2h0OiA4cHg7XG5cbiAgICAgICAgICAucGVlci1jb3VudCB7XG4gICAgICAgICAgICBkaXNwbGF5OiBmbGV4O1xuICAgICAgICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgICAgICAgIGdhcDogNHB4O1xuICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4O1xuICAgICAgICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG5cbiAgICAgICAgICAgIG1hdC1pY29uIHtcbiAgICAgICAgICAgICAgZm9udC1zaXplOiAxNnB4O1xuICAgICAgICAgICAgICB3aWR0aDogMTZweDtcbiAgICAgICAgICAgICAgaGVpZ2h0OiAxNnB4O1xuXG4gICAgICAgICAgICAgICYuc3RhdHVzLWxvY2FsIHtcbiAgICAgICAgICAgICAgICBjb2xvcjogdmFyKC0tbWRlLWljb24tYWNjZW50LWdyZWVuKTtcbiAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAmLnN0YXR1cy1zZWVkaW5nIHtcbiAgICAgICAgICAgICAgICBjb2xvcjogdmFyKC0tbWRlLWljb24tYWNjZW50LWJsdWUpO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICYuc3RhdHVzLWRvd25sb2FkaW5nIHtcbiAgICAgICAgICAgICAgICBjb2xvcjogIzE3YTJiODtcbiAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgICAmLnN0YXR1cy1jb21wbGV0ZWQge1xuICAgICAgICAgICAgICAgIGNvbG9yOiB2YXIoLS1tZGUtaWNvbi1hY2NlbnQtZ3JlZW4pO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICYuc3RhdHVzLWF2YWlsYWJsZSB7XG4gICAgICAgICAgICAgICAgY29sb3I6IHZhcigtLW1kZS1pY29uLWFjY2VudC1vcmFuZ2UpO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICYuc3RhdHVzLXVuYXZhaWxhYmxlIHtcbiAgICAgICAgICAgICAgICBjb2xvcjogdmFyKC0tbWRlLWljb24tY29sb3IpO1xuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICAgICYuc3RhdHVzLXVua25vd24ge1xuICAgICAgICAgICAgICAgIGNvbG9yOiB2YXIoLS1tZGUtaWNvbi1jb2xvcik7XG4gICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH1cblxuICAgICAgICAgICAgLnN0YXR1cy10ZXh0IHtcbiAgICAgICAgICAgICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICB9XG4gICAgICAgIH1cblxuICAgICAgICAuZmlsZS1hY3Rpb25zIHtcbiAgICAgICAgICBmbGV4LXNocmluazogMDtcbiAgICAgICAgfVxuICAgICAgfVxuICAgIH1cbiAgfVxufVxuXG4uYmFkZ2Uge1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYWNjZW50LXByaW1hcnkpO1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtb24tcHJpbWFyeSk7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogMnB4IDZweDtcbiAgYm9yZGVyLXJhZGl1czogMTBweDtcbiAgbWFyZ2luLWxlZnQ6IDhweDtcbn1cblxuOjpuZy1kZWVwIC5tYXQtdGFiLWxhYmVsIHtcbiAgbWF0LWljb24ge1xuICAgIG1hcmdpbi1yaWdodDogOHB4O1xuICB9XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

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
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/divider */ 1528);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/forms */ 2508);













function ProjectCreateConfigDialogComponent_div_14_p_28_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "p", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx_r2.templateError);
  }
}
const _c0 = function (a0) {
  return {
    titles: a0
  };
};
const _c1 = function (a0) {
  return {
    path: a0
  };
};
function ProjectCreateConfigDialogComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 9)(1, "div", 10)(2, "p", 11)(3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](4, "dashboard_customize");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "mat-checkbox", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_div_14_Template_mat_checkbox_ngModelChange_7_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r4);
      const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r3.templateExample = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "label", 13)(11, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "mat-form-field", 14)(15, "input", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_div_14_Template_input_ngModelChange_15_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r4);
      const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r5.folderName = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](19, "label", 17)(20, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](21);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](22, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "mat-form-field", 14)(24, "input", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_div_14_Template_input_ngModelChange_24_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r4);
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r6.originUrl = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](25, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](26);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](27, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](28, ProjectCreateConfigDialogComponent_div_14_p_28_Template, 2, 1, "p", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind2"](6, 10, "TEMPLATE_SHOWCASE.CHOSEN", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction1"](24, _c0, ctx_r0.chosenTitles)), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.templateExample);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](9, 13, "PROJECT_CONFIG.TEMPLATE_EXAMPLE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](13, 15, "PROJECT_CONFIG.TEMPLATE_FOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.folderName);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind2"](18, 17, "PROJECT_CONFIG.TEMPLATE_FOLDER_DESC", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpureFunction1"](26, _c1, ctx_r0.config.projectPath)));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](22, 20, "PROJECT_CONFIG.TEMPLATE_ORIGIN"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.originUrl);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](27, 22, "PROJECT_CONFIG.TEMPLATE_ORIGIN_DESC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx_r0.templateError);
  }
}
function ProjectCreateConfigDialogComponent_div_15_mat_radio_button_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-radio-button", 28)(1, "span", 29)(2, "mat-icon", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const option_r9 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("value", option_r9.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](option_r9.icon);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](6, 4, option_r9.labelKey));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](9, 6, option_r9.descKey), " ");
  }
}
function ProjectCreateConfigDialogComponent_div_15_div_11_Template(rf, ctx) {
  if (rf & 1) {
    const _r11 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 22)(1, "mat-checkbox", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_div_15_div_11_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r11);
      const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r10.config.initializeGit = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](2, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "div", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r8.config.initializeGit);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](4, 3, "PROJECT_CONFIG.INIT_GIT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](7, 5, "PROJECT_CONFIG.INIT_GIT_DESC"), " ");
  }
}
function ProjectCreateConfigDialogComponent_div_15_Template(rf, ctx) {
  if (rf & 1) {
    const _r13 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 21)(1, "div", 22)(2, "label", 23)(3, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "div", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "mat-radio-group", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_div_15_Template_mat_radio_group_ngModelChange_9_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r13);
      const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r12.config.harness = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](10, ProjectCreateConfigDialogComponent_div_15_mat_radio_button_10_Template, 10, 8, "mat-radio-button", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](11, ProjectCreateConfigDialogComponent_div_15_div_11_Template, 8, 7, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](5, 5, "PROJECT_CONFIG.HARNESS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](8, 7, "PROJECT_CONFIG.HARNESS_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r1.config.harness);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx_r1.harnessOptions);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx_r1.fromTemplate);
  }
}
class ProjectCreateConfigDialogComponent {
  constructor(dialogRef, data, translate) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.translate = translate;
    this.templateExample = false;
    this.folderName = '';
    this.originUrl = '';
    this.templateError = '';
    /**
     * Le tre scelte, con l'icona che le accompagna nella lista. Sono mutuamente esclusive:
     * il progetto ne dichiara UNA, e MdExplorer installa i propri file solo dove quella dice.
     */
    this.harnessOptions = [{
      value: 'copilot',
      icon: 'smart_toy',
      labelKey: 'PROJECT_CONFIG.HARNESS_COPILOT',
      descKey: 'PROJECT_CONFIG.HARNESS_COPILOT_DESC'
    }, {
      value: 'opencode',
      icon: 'terminal',
      labelKey: 'PROJECT_CONFIG.HARNESS_OPENCODE',
      descKey: 'PROJECT_CONFIG.HARNESS_OPENCODE_DESC'
    }, {
      value: 'claude',
      icon: 'psychology',
      labelKey: 'PROJECT_CONFIG.HARNESS_CLAUDE',
      descKey: 'PROJECT_CONFIG.HARNESS_CLAUDE_DESC'
    }, {
      value: 'none',
      icon: 'block',
      labelKey: 'PROJECT_CONFIG.HARNESS_NONE',
      descKey: 'PROJECT_CONFIG.HARNESS_NONE_DESC'
    }];
    this.fromTemplate = !!(data.choice?.baseUrl || data.choice?.methodUrls.length);
    this.chosenTitles = (data.choice?.titles || []).join(', ');
    // Initialize with default values
    this.config = {
      projectPath: data.projectPath,
      initializeGit: false,
      harness: 'copilot' // Copilot resta il default storico
    };
  }
  onCreateProject() {
    this.templateError = '';
    if (!this.fromTemplate) {
      this.dialogRef.close(this.config);
      return;
    }
    const choice = this.data.choice;
    const folderName = this.folderName.trim();
    if (!folderName || /[\\/]/.test(folderName)) {
      this.templateError = this.translate.instant('PROJECT_CONFIG.TEMPLATE_FOLDER_NEEDED');
      return;
    }
    this.dialogRef.close({
      ...this.config,
      template: {
        baseUrl: choice.baseUrl,
        methodUrls: choice.methodUrls,
        theoryUrls: choice.theoryUrls,
        asksEngine: choice.asksEngine,
        agentCity: choice.agentCity,
        agentCityGuide: choice.agentCityGuide,
        language: choice.language,
        example: this.templateExample,
        folderName,
        originUrl: this.originUrl.trim()
      }
    });
  }
  onCancel() {
    this.dialogRef.close(null);
  }
  static {
    this.ɵfac = function ProjectCreateConfigDialogComponent_Factory(t) {
      return new (t || ProjectCreateConfigDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_2__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: ProjectCreateConfigDialogComponent,
      selectors: [["app-project-create-config-dialog"]],
      decls: 25,
      vars: 15,
      consts: [["mat-dialog-title", ""], [1, "config-container"], [1, "project-path"], [1, "path-text"], ["class", "option-item template-choice", 4, "ngIf"], ["class", "config-options", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"], [1, "option-item", "template-choice"], [1, "template-fields"], [1, "chosen"], ["color", "primary", 3, "ngModel", "ngModelChange"], ["for", "template-folder", 1, "group-label"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "id", "template-folder", "placeholder", "gara-2027", 3, "ngModel", "ngModelChange"], [1, "option-description"], ["for", "template-origin", 1, "group-label"], ["matInput", "", "id", "template-origin", "placeholder", "https://github.com/\u2026/gara-2027.git", 3, "ngModel", "ngModelChange"], ["class", "option-description error-text", 4, "ngIf"], [1, "option-description", "error-text"], [1, "config-options"], [1, "option-item"], ["id", "harness-label", 1, "group-label"], [1, "option-description", "group-description"], ["aria-labelledby", "harness-label", 1, "harness-group", 3, "ngModel", "ngModelChange"], ["class", "harness-radio", 3, "value", 4, "ngFor", "ngForOf"], ["class", "option-item", 4, "ngIf"], [1, "harness-radio", 3, "value"], [1, "harness-label"], [1, "harness-icon"]],
      template: function ProjectCreateConfigDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "mat-dialog-content")(4, "div", 1)(5, "div", 2)(6, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7, "folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "strong");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](10, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](13, "mat-divider");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](14, ProjectCreateConfigDialogComponent_div_14_Template, 29, 28, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](15, ProjectCreateConfigDialogComponent_div_15_Template, 12, 9, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "mat-dialog-actions", 6)(17, "button", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_17_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](19, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](20, "button", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_20_listener() {
            return ctx.onCreateProject();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](21, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](22, "add");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](23);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](24, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 7, "PROJECT_CONFIG.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](10, 9, "PROJECT_CONFIG.PROJECT_PATH"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.config.projectPath);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.fromTemplate);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx.fromTemplate || ctx.data.choice.asksEngine);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](19, 11, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](24, 13, "PROJECT_CONFIG.CREATE_PROJECT"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_3__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_3__.NgIf, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_4__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_5__.MatLegacyFormField, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_6__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_7__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_7__.MatLegacyRadioButton, _angular_material_divider__WEBPACK_IMPORTED_MODULE_8__.MatDivider, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_9__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_10__.MatIcon, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_11__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_11__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_11__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_2__.TranslatePipe],
      styles: [".config-container[_ngcontent-%COMP%] {\n  min-width: 400px;\n  padding: 20px 0;\n}\n\n.project-path[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 15px;\n  background-color: rgba(0, 0, 0, 0.04);\n  border-radius: 4px;\n  margin-bottom: 20px;\n}\n.project-path[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #666;\n}\n.project-path[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  font-family: monospace;\n  color: #444;\n  word-break: break-all;\n}\n\n.config-options[_ngcontent-%COMP%] {\n  padding: 20px 0;\n}\n\n.option-item[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n}\n.option-item[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n}\n.option-item[_ngcontent-%COMP%]   .option-description[_ngcontent-%COMP%] {\n  margin-top: 5px;\n  margin-left: 28px;\n  font-size: 12px;\n  color: #666;\n  line-height: 1.4;\n}\n\n.group-label[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 4px;\n}\n\n.group-description[_ngcontent-%COMP%] {\n  margin-left: 0;\n  margin-bottom: 12px;\n}\n\n.harness-group[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 14px;\n}\n\n.harness-radio[_ngcontent-%COMP%]   .harness-label[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n}\n.harness-radio[_ngcontent-%COMP%]   .harness-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  height: 18px;\n  width: 18px;\n}\n\nmat-divider[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n\n\n.template-choice[_ngcontent-%COMP%] {\n  margin: 12px 0;\n}\n\n.chosen[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  font-weight: 500;\n  margin: 0 0 8px;\n}\n\n.template-fields[_ngcontent-%COMP%] {\n  margin: 8px 0 0 0;\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.error-text[_ngcontent-%COMP%] {\n  color: var(--mde-error, #c62828);\n}\n\n\n.template-choice[_ngcontent-%COMP%]     .mat-checkbox-layout, .template-choice[_ngcontent-%COMP%]     .mat-checkbox-label, .template-choice[_ngcontent-%COMP%]     .mat-radio-label, .template-choice[_ngcontent-%COMP%]     .mat-radio-label-content, .config-options[_ngcontent-%COMP%]     .mat-checkbox-layout, .config-options[_ngcontent-%COMP%]     .mat-checkbox-label, .config-options[_ngcontent-%COMP%]     .mat-radio-label, .config-options[_ngcontent-%COMP%]     .mat-radio-label-content {\n  white-space: normal;\n}\n\n\n.template-choice[_ngcontent-%COMP%] {\n  overflow-x: hidden;\n}\n\n.template-fields[_ngcontent-%COMP%]   .option-description[_ngcontent-%COMP%] {\n  overflow-wrap: anywhere;\n  margin-left: 0;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wcm9qZWN0LWNyZWF0ZS1jb25maWcvcHJvamVjdC1jcmVhdGUtY29uZmlnLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSxxQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFDRjtBQUNFO0VBQ0UsV0FBQTtBQUNKO0FBRUU7RUFDRSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxxQkFBQTtBQUFKOztBQUlBO0VBQ0UsZUFBQTtBQURGOztBQUlBO0VBQ0UsbUJBQUE7QUFERjtBQUdFO0VBQ0UsY0FBQTtBQURKO0FBSUU7RUFDRSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0FBRko7O0FBTUE7RUFDRSxjQUFBO0VBQ0Esa0JBQUE7QUFIRjs7QUFNQTtFQUNFLGNBQUE7RUFDQSxtQkFBQTtBQUhGOztBQU1BO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQUhGOztBQU9FO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFKSjtBQU9FO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0FBTEo7O0FBU0E7RUFDRSxjQUFBO0FBTkY7O0FBU0E7RUFDRSxrQkFBQTtBQU5GO0FBU0k7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQVBOOztBQVdBLDZFQUFBO0FBQ0E7RUFDRSxjQUFBO0FBUkY7O0FBV0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZ0JBQUE7RUFDQSxlQUFBO0FBUkY7O0FBV0E7RUFDRSxpQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUFSRjs7QUFXQTtFQUNFLFdBQUE7QUFSRjs7QUFXQTtFQUNFLGdDQUFBO0FBUkY7O0FBV0EsK0dBQUE7QUFHRTs7Ozs7Ozs7RUFJRSxtQkFBQTtBQU5KOztBQVVBLG1HQUFBO0FBQ0E7RUFDRSxrQkFBQTtBQVBGOztBQVVBO0VBQ0UsdUJBQUE7RUFDQSxjQUFBO0FBUEYiLCJzb3VyY2VzQ29udGVudCI6WyIuY29uZmlnLWNvbnRhaW5lciB7XG4gIG1pbi13aWR0aDogNDAwcHg7XG4gIHBhZGRpbmc6IDIwcHggMDtcbn1cblxuLnByb2plY3QtcGF0aCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogMTBweDtcbiAgcGFkZGluZzogMTVweDtcbiAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgwLCAwLCAwLCAwLjA0KTtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBtYXJnaW4tYm90dG9tOiAyMHB4O1xuXG4gIG1hdC1pY29uIHtcbiAgICBjb2xvcjogIzY2NjtcbiAgfVxuXG4gIC5wYXRoLXRleHQge1xuICAgIGZvbnQtZmFtaWx5OiBtb25vc3BhY2U7XG4gICAgY29sb3I6ICM0NDQ7XG4gICAgd29yZC1icmVhazogYnJlYWstYWxsO1xuICB9XG59XG5cbi5jb25maWctb3B0aW9ucyB7XG4gIHBhZGRpbmc6IDIwcHggMDtcbn1cblxuLm9wdGlvbi1pdGVtIHtcbiAgbWFyZ2luLWJvdHRvbTogMjBweDtcblxuICBtYXQtY2hlY2tib3gge1xuICAgIGRpc3BsYXk6IGJsb2NrO1xuICB9XG5cbiAgLm9wdGlvbi1kZXNjcmlwdGlvbiB7XG4gICAgbWFyZ2luLXRvcDogNXB4O1xuICAgIG1hcmdpbi1sZWZ0OiAyOHB4O1xuICAgIGZvbnQtc2l6ZTogMTJweDtcbiAgICBjb2xvcjogIzY2NjtcbiAgICBsaW5lLWhlaWdodDogMS40O1xuICB9XG59XG5cbi5ncm91cC1sYWJlbCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBtYXJnaW4tYm90dG9tOiA0cHg7XG59XG5cbi5ncm91cC1kZXNjcmlwdGlvbiB7XG4gIG1hcmdpbi1sZWZ0OiAwO1xuICBtYXJnaW4tYm90dG9tOiAxMnB4O1xufVxuXG4uaGFybmVzcy1ncm91cCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogMTRweDtcbn1cblxuLmhhcm5lc3MtcmFkaW8ge1xuICAuaGFybmVzcy1sYWJlbCB7XG4gICAgZGlzcGxheTogaW5saW5lLWZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBnYXA6IDZweDtcbiAgfVxuXG4gIC5oYXJuZXNzLWljb24ge1xuICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICBoZWlnaHQ6IDE4cHg7XG4gICAgd2lkdGg6IDE4cHg7XG4gIH1cbn1cblxubWF0LWRpdmlkZXIge1xuICBtYXJnaW46IDIwcHggMDtcbn1cblxubWF0LWRpYWxvZy1hY3Rpb25zIHtcbiAgcGFkZGluZzogMTZweCAyNHB4O1xuXG4gIGJ1dHRvbiB7XG4gICAgbWF0LWljb24ge1xuICAgICAgbWFyZ2luLXJpZ2h0OiA0cHg7XG4gICAgICBmb250LXNpemU6IDIwcHg7XG4gICAgICBoZWlnaHQ6IDIwcHg7XG4gICAgICB3aWR0aDogMjBweDtcbiAgICB9XG4gIH1cbn1cbi8qIERhIHRlbXBsYXRlOiBsYSBzY2VsdGEgZmF0dGEgbmVsbGEgdmV0cmluYSwgcG9pIGkgY2FtcGkgZGVsbGEgY3JlYXppb25lLiAqL1xuLnRlbXBsYXRlLWNob2ljZSB7XG4gIG1hcmdpbjogMTJweCAwO1xufVxuXG4uY2hvc2VuIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG4gIG1hcmdpbjogMCAwIDhweDtcbn1cblxuLnRlbXBsYXRlLWZpZWxkcyB7XG4gIG1hcmdpbjogOHB4IDAgMCAwO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDRweDtcbn1cblxuLmZ1bGwtd2lkdGgge1xuICB3aWR0aDogMTAwJTtcbn1cblxuLmVycm9yLXRleHQge1xuICBjb2xvcjogdmFyKC0tbWRlLWVycm9yLCAjYzYyODI4KTtcbn1cblxuLyogTGUgZGVzY3JpemlvbmkgZGVsbGUgc2NlbHRlIHZhbm5vIGEgY2FwbzogbGEgZmluZXN0cmEgbm9uIGRldmUgc2NvcnJlcmUgZGkgbGF0byAoYW5jaGUgcXVlbGxlIGRlaSBtb3RvcmkpLiAqL1xuLnRlbXBsYXRlLWNob2ljZSA6Om5nLWRlZXAsXG4uY29uZmlnLW9wdGlvbnMgOjpuZy1kZWVwIHtcbiAgLm1hdC1jaGVja2JveC1sYXlvdXQsXG4gIC5tYXQtY2hlY2tib3gtbGFiZWwsXG4gIC5tYXQtcmFkaW8tbGFiZWwsXG4gIC5tYXQtcmFkaW8tbGFiZWwtY29udGVudCB7XG4gICAgd2hpdGUtc3BhY2U6IG5vcm1hbDtcbiAgfVxufVxuXG4vKiBVbiBwZXp6byBpbnZpc2liaWxlIGRlaSBjYW1waSBNYXRlcmlhbCBzYm9yZGF2YSBkaSA0MHB4IGUgZmFjZXZhIHNjb3JyZXJlIGxhIGZpbmVzdHJhIGRpIGxhdG8uICovXG4udGVtcGxhdGUtY2hvaWNlIHtcbiAgb3ZlcmZsb3cteDogaGlkZGVuO1xufVxuXG4udGVtcGxhdGUtZmllbGRzIC5vcHRpb24tZGVzY3JpcHRpb24ge1xuICBvdmVyZmxvdy13cmFwOiBhbnl3aGVyZTtcbiAgbWFyZ2luLWxlZnQ6IDA7XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

/***/ }),

/***/ 1602:
/*!************************************************************************************!*\
  !*** ./src/app/projects/dialogs/project-edit/icon-editor/icon-editor.component.ts ***!
  \************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "IconEditorComponent": () => (/* binding */ IconEditorComponent)
/* harmony export */ });
/* harmony import */ var _home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @ngx-translate/core */ 8699);







const _c0 = ["canvas"];
function IconEditorComponent_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 12)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "content_paste");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "p", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 2, "PROJECTS.ICON_EDITOR_PASTE_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](8, 4, "PROJECTS.ICON_EDITOR_PASTE_SUB"));
  }
}
function IconEditorComponent_div_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 15)(1, "span", 16)(2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3, "open_with");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](6, "span", 16)(7, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](8, "zoom_in");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](5, 2, "PROJECTS.ICON_EDITOR_HINT_DRAG"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](10, 4, "PROJECTS.ICON_EDITOR_HINT_ZOOM"), " ");
  }
}
function IconEditorComponent_div_6_span_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "PROJECTS.ICON_EDITOR_ERROR_NO_IMAGE"));
  }
}
function IconEditorComponent_div_6_span_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "PROJECTS.ICON_EDITOR_ERROR_CLIPBOARD_DENIED"));
  }
}
function IconEditorComponent_div_6_span_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "PROJECTS.ICON_EDITOR_ERROR_CLIPBOARD_UNAVAILABLE"));
  }
}
function IconEditorComponent_div_6_span_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](2, 1, "PROJECTS.ICON_EDITOR_ERROR_LOAD"));
  }
}
function IconEditorComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 17)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "error_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "span", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, IconEditorComponent_div_6_span_4_Template, 3, 3, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, IconEditorComponent_div_6_span_5_Template, 3, 3, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, IconEditorComponent_div_6_span_6_Template, 3, 3, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](7, IconEditorComponent_div_6_span_7_Template, 3, 3, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngSwitch", ctx_r3.pasteError);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngSwitchCase", "NO_IMAGE");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngSwitchCase", "CLIPBOARD_DENIED");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngSwitchCase", "CLIPBOARD_UNAVAILABLE");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngSwitchCase", "LOAD_ERROR");
  }
}
function IconEditorComponent_button_19_Template(rf, ctx) {
  if (rf & 1) {
    const _r10 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "button", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function IconEditorComponent_button_19_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵrestoreView"](_r10);
      const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresetView"](ctx_r9.removeAndEmit());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](4, 1, "PROJECTS.ICON_EDITOR_REMOVE"), " ");
  }
}
/**
 * Inline canvas editor for the project icon.
 *
 * UX:
 * - Big canvas with a dashed squircle in the middle (the crop frame).
 * - User pastes a logo with Ctrl+V (or "Incolla" button as fallback).
 * - Drag the image with the mouse to reposition; Ctrl + wheel to zoom.
 * - "Applica" produces a 256×256 PNG (squircle-clipped) and emits it as base64.
 * - "Rimuovi" emits null so the dialog can clear the custom icon.
 *
 * The component is dumb about persistence — the parent dialog decides when to
 * actually call the backend (so the icon change participates in the dialog's
 * Save / Cancel semantics together with name, description and participants).
 */
class IconEditorComponent {
  constructor() {
    /** Current icon URL to preload into the canvas (for "edit existing icon"). */
    this.initialIconUrl = null;
    /** Whether a custom icon currently exists on the backend (drives "Rimuovi" visibility). */
    this.hasCustomIcon = false;
    /**
     * Emitted when the user clicks Applica/Rimuovi.
     * - { pngBase64: string }  → pending "set" with this PNG (data URL form)
     * - { pngBase64: null }    → pending "remove"
     */
    this.iconChanged = new _angular_core__WEBPACK_IMPORTED_MODULE_1__.EventEmitter();
    // Canvas + crop geometry. Kept as constants so the math in applyAndEmit() stays simple.
    this.canvasSize = 320;
    this.cropSize = 200; // dashed squircle side
    this.outputSize = 256; // exported PNG resolution
    this.cornerRatio = 0.25; // matches .project-icon-wrapper border-radius / size = 10/40
    // Image + transform state
    this.img = null;
    this.offsetX = 0;
    this.offsetY = 0;
    this.scale = 1;
    this.minScale = 0.05;
    this.maxScale = 8;
    // Drag state
    this.dragging = false;
    this.dragStartX = 0;
    this.dragStartY = 0;
    this.dragStartOffsetX = 0;
    this.dragStartOffsetY = 0;
    // UI flags
    this.hasImage = false;
    this.pasteError = null;
    this.pasting = false;
  }
  ngAfterViewInit() {
    this.draw();
    if (this.initialIconUrl) {
      this.loadFromUrl(this.initialIconUrl);
    }
  }
  ngOnChanges(changes) {
    if (changes.initialIconUrl && !changes.initialIconUrl.isFirstChange()) {
      const url = changes.initialIconUrl.currentValue;
      if (url) {
        this.loadFromUrl(url);
      } else {
        this.clear();
      }
    }
  }
  ngOnDestroy() {
    this.img = null;
  }
  // ─── Paste handling ────────────────────────────────────────────────
  /**
   * Document-level paste listener: when the dialog is open and the user hits
   * Ctrl+V, we accept the first image item. Scoped to "this component is
   * mounted" — the dialog is the only consumer so cross-talk is unlikely.
   */
  onPaste(event) {
    const items = event.clipboardData?.items;
    if (!items) return;
    for (let i = 0; i < items.length; i++) {
      const item = items[i];
      if (item.kind === 'file' && item.type.startsWith('image/')) {
        const blob = item.getAsFile();
        if (blob) {
          event.preventDefault();
          this.loadFromBlob(blob);
          return;
        }
      }
    }
  }
  /**
   * Programmatic clipboard read for the "Incolla" fallback button.
   * Uses the async Clipboard API; in Electron this usually works without
   * explicit permission because the page is treated as same-origin.
   */
  pasteFromClipboard() {
    var _this = this;
    return (0,_home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      _this.pasteError = null;
      _this.pasting = true;
      try {
        const items = yield navigator.clipboard.read?.();
        if (!items) {
          _this.pasteError = 'CLIPBOARD_UNAVAILABLE';
          return;
        }
        for (const item of items) {
          const imgType = item.types.find(t => t.startsWith('image/'));
          if (imgType) {
            const blob = yield item.getType(imgType);
            _this.loadFromBlob(blob);
            return;
          }
        }
        _this.pasteError = 'NO_IMAGE';
      } catch {
        _this.pasteError = 'CLIPBOARD_DENIED';
      } finally {
        _this.pasting = false;
      }
    })();
  }
  loadFromBlob(blob) {
    const url = URL.createObjectURL(blob);
    this.loadFromUrl(url, () => URL.revokeObjectURL(url));
  }
  loadFromUrl(url, onLoad) {
    const image = new Image();
    image.crossOrigin = 'anonymous';
    image.onload = () => {
      this.img = image;
      this.hasImage = true;
      this.pasteError = null;
      this.fitImageToCrop();
      this.draw();
      onLoad?.();
    };
    image.onerror = () => {
      this.pasteError = 'LOAD_ERROR';
      onLoad?.();
    };
    image.src = url;
  }
  /** Initial scale so the longest side fills the crop frame. */
  fitImageToCrop() {
    if (!this.img) return;
    const longest = Math.max(this.img.width, this.img.height);
    this.scale = longest > 0 ? this.cropSize / longest : 1;
    this.offsetX = 0;
    this.offsetY = 0;
  }
  // ─── Mouse interactions ────────────────────────────────────────────
  onMouseDown(event) {
    if (!this.img) return;
    this.dragging = true;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.dragStartOffsetX = this.offsetX;
    this.dragStartOffsetY = this.offsetY;
    event.preventDefault();
  }
  onMouseMove(event) {
    if (!this.dragging) return;
    this.offsetX = this.dragStartOffsetX + (event.clientX - this.dragStartX);
    this.offsetY = this.dragStartOffsetY + (event.clientY - this.dragStartY);
    this.draw();
  }
  onMouseUp() {
    this.dragging = false;
  }
  /** Ctrl + wheel to zoom; zooms toward the canvas center to keep things predictable. */
  onWheel(event) {
    // Block the Electron app-wide zoom regardless of whether we have an image:
    // ElectronMdExplorer/preload.js listens on `window` for ctrl+wheel and
    // forwards a `zoom-mouse-wheel` IPC that calls webContents.setZoomFactor.
    // Without stopPropagation the same gesture would zoom both the local crop
    // AND the entire app shell. preventDefault alone isn't enough — it stops
    // the browser default action but not other listeners on ancestor nodes.
    if (event.ctrlKey) {
      event.preventDefault();
      event.stopPropagation();
    }
    if (!this.img) return;
    if (!event.ctrlKey) return; // plain wheel keeps page scroll behavior
    const factor = event.deltaY < 0 ? 1.1 : 1 / 1.1;
    const next = Math.min(this.maxScale, Math.max(this.minScale, this.scale * factor));
    this.scale = next;
    this.draw();
  }
  // ─── Rendering ─────────────────────────────────────────────────────
  draw() {
    const canvas = this.canvasRef.nativeElement;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    // Background (checkerboard would be overkill here; a soft neutral works).
    ctx.fillStyle = '#0f1419';
    ctx.fillRect(0, 0, this.canvasSize, this.canvasSize);
    if (this.img) {
      const w = this.img.width * this.scale;
      const h = this.img.height * this.scale;
      const x = this.canvasSize / 2 + this.offsetX - w / 2;
      const y = this.canvasSize / 2 + this.offsetY - h / 2;
      ctx.drawImage(this.img, x, y, w, h);
    }
    // Dim everything outside the crop frame so the user sees what will be cut.
    const cropX = (this.canvasSize - this.cropSize) / 2;
    const cropY = (this.canvasSize - this.cropSize) / 2;
    const radius = this.cropSize * this.cornerRatio;
    ctx.save();
    ctx.fillStyle = 'rgba(0, 0, 0, 0.55)';
    ctx.beginPath();
    ctx.rect(0, 0, this.canvasSize, this.canvasSize);
    this.roundedRectPath(ctx, cropX, cropY, this.cropSize, this.cropSize, radius, true);
    ctx.fill('evenodd');
    ctx.restore();
    // Dashed squircle outline for the crop frame.
    ctx.save();
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 2;
    ctx.setLineDash([8, 6]);
    this.roundedRectPath(ctx, cropX, cropY, this.cropSize, this.cropSize, radius);
    ctx.stroke();
    ctx.restore();
  }
  /**
   * Standard rounded-rect path. Optional `counterclockwise` flag is used when
   * we need the inner cutout for the dim overlay (even-odd fill rule).
   */
  roundedRectPath(ctx, x, y, w, h, r, counterclockwise = false) {
    ctx.moveTo(x + r, y);
    if (counterclockwise) {
      ctx.lineTo(x + r, y);
      ctx.arcTo(x, y, x, y + r, r);
      ctx.lineTo(x, y + h - r);
      ctx.arcTo(x, y + h, x + r, y + h, r);
      ctx.lineTo(x + w - r, y + h);
      ctx.arcTo(x + w, y + h, x + w, y + h - r, r);
      ctx.lineTo(x + w, y + r);
      ctx.arcTo(x + w, y, x + w - r, y, r);
      ctx.closePath();
    } else {
      ctx.lineTo(x + w - r, y);
      ctx.arcTo(x + w, y, x + w, y + r, r);
      ctx.lineTo(x + w, y + h - r);
      ctx.arcTo(x + w, y + h, x + w - r, y + h, r);
      ctx.lineTo(x + r, y + h);
      ctx.arcTo(x, y + h, x, y + h - r, r);
      ctx.lineTo(x, y + r);
      ctx.arcTo(x, y, x + r, y, r);
      ctx.closePath();
    }
  }
  // ─── Output ────────────────────────────────────────────────────────
  applyAndEmit() {
    if (!this.img) return;
    const out = document.createElement('canvas');
    out.width = this.outputSize;
    out.height = this.outputSize;
    const ctx = out.getContext('2d');
    if (!ctx) return;
    // Map from "canvas crop frame coords" to "output coords":
    //   k = outputSize / cropSize
    // Image position relative to crop frame top-left:
    //   relX = cropSize/2 + offsetX - img.width  * scale / 2
    //   relY = cropSize/2 + offsetY - img.height * scale / 2
    const k = this.outputSize / this.cropSize;
    const relX = this.cropSize / 2 + this.offsetX - this.img.width * this.scale / 2;
    const relY = this.cropSize / 2 + this.offsetY - this.img.height * this.scale / 2;
    // Squircle clip — matches the card icon shape so the saved PNG looks the
    // same in both the editor preview and the project list card.
    ctx.save();
    ctx.beginPath();
    this.roundedRectPath(ctx, 0, 0, this.outputSize, this.outputSize, this.outputSize * this.cornerRatio);
    ctx.clip();
    ctx.drawImage(this.img, relX * k, relY * k, this.img.width * this.scale * k, this.img.height * this.scale * k);
    ctx.restore();
    const dataUrl = out.toDataURL('image/png');
    this.iconChanged.emit({
      pngBase64: dataUrl
    });
  }
  removeAndEmit() {
    this.clear();
    this.iconChanged.emit({
      pngBase64: null
    });
  }
  resetView() {
    if (!this.img) return;
    this.fitImageToCrop();
    this.draw();
  }
  clear() {
    this.img = null;
    this.hasImage = false;
    this.offsetX = 0;
    this.offsetY = 0;
    this.scale = 1;
    this.draw();
  }
  static {
    this.ɵfac = function IconEditorComponent_Factory(t) {
      return new (t || IconEditorComponent)();
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({
      type: IconEditorComponent,
      selectors: [["app-icon-editor"]],
      viewQuery: function IconEditorComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵviewQuery"](_c0, 7);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵloadQuery"]()) && (ctx.canvasRef = _t.first);
        }
      },
      hostBindings: function IconEditorComponent_HostBindings(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("paste", function IconEditorComponent_paste_HostBindingHandler($event) {
            return ctx.onPaste($event);
          }, false, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresolveDocument"])("mousemove", function IconEditorComponent_mousemove_HostBindingHandler($event) {
            return ctx.onMouseMove($event);
          }, false, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresolveDocument"])("mouseup", function IconEditorComponent_mouseup_HostBindingHandler() {
            return ctx.onMouseUp();
          }, false, _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵresolveDocument"]);
        }
      },
      inputs: {
        initialIconUrl: "initialIconUrl",
        hasCustomIcon: "hasCustomIcon"
      },
      outputs: {
        iconChanged: "iconChanged"
      },
      features: [_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵNgOnChangesFeature"]],
      decls: 25,
      vars: 20,
      consts: [[1, "icon-editor"], [1, "canvas-wrapper", 3, "mousedown", "wheel"], [1, "editor-canvas", 3, "width", "height"], ["canvas", ""], ["class", "placeholder-overlay", 4, "ngIf"], ["class", "hints", 4, "ngIf"], ["class", "error-line", 4, "ngIf"], [1, "actions"], ["mat-stroked-button", "", "type", "button", 3, "disabled", "click"], [1, "spacer"], ["mat-stroked-button", "", "type", "button", "color", "warn", 3, "click", 4, "ngIf"], ["mat-flat-button", "", "color", "primary", "type", "button", 3, "disabled", "click"], [1, "placeholder-overlay"], [1, "placeholder-title"], [1, "placeholder-sub"], [1, "hints"], [1, "hint-item"], [1, "error-line"], [3, "ngSwitch"], [4, "ngSwitchCase"], ["mat-stroked-button", "", "type", "button", "color", "warn", 3, "click"]],
      template: function IconEditorComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0)(1, "div", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("mousedown", function IconEditorComponent_Template_div_mousedown_1_listener($event) {
            return ctx.onMouseDown($event);
          })("wheel", function IconEditorComponent_Template_div_wheel_1_listener($event) {
            return ctx.onWheel($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](2, "canvas", 2, 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](4, IconEditorComponent_div_4_Template, 9, 6, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](5, IconEditorComponent_div_5_Template, 11, 6, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](6, IconEditorComponent_div_6_Template, 8, 5, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "div", 7)(8, "button", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function IconEditorComponent_Template_button_click_8_listener() {
            return ctx.pasteFromClipboard();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](9, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](10, "content_paste");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](12, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "button", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function IconEditorComponent_Template_button_click_13_listener() {
            return ctx.resetView();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15, "center_focus_strong");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](16);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](17, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](18, "span", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtemplate"](19, IconEditorComponent_button_19_Template, 5, 3, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](20, "button", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function IconEditorComponent_Template_button_click_20_listener() {
            return ctx.applyAndEmit();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](22, "check");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](23);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipe"](24, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵclassProp"]("empty", !ctx.hasImage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("width", ctx.canvasSize)("height", ctx.canvasSize);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", !ctx.hasImage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.hasImage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.pasteError);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", ctx.pasting);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](12, 14, "PROJECTS.ICON_EDITOR_PASTE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx.hasImage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](17, 16, "PROJECTS.ICON_EDITOR_RESET"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("ngIf", ctx.hasCustomIcon);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵproperty"]("disabled", !ctx.hasImage);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵpipeBind1"](24, 18, "PROJECTS.ICON_EDITOR_APPLY"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_2__.NgIf, _angular_common__WEBPACK_IMPORTED_MODULE_2__.NgSwitch, _angular_common__WEBPACK_IMPORTED_MODULE_2__.NgSwitchCase, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_3__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_4__.MatIcon, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_5__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n}\n\n.icon-editor[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 10px;\n}\n\n.canvas-wrapper[_ngcontent-%COMP%] {\n  position: relative;\n  width: 320px;\n  height: 320px;\n  margin: 0 auto;\n  border-radius: 12px;\n  overflow: hidden;\n  cursor: grab;\n  background: #0f1419;\n  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.25);\n  -webkit-user-select: none;\n          user-select: none;\n}\n.canvas-wrapper.empty[_ngcontent-%COMP%] {\n  cursor: default;\n}\n.canvas-wrapper[_ngcontent-%COMP%]:active {\n  cursor: grabbing;\n}\n.canvas-wrapper[_ngcontent-%COMP%]   .editor-canvas[_ngcontent-%COMP%] {\n  display: block;\n  width: 100%;\n  height: 100%;\n  pointer-events: none;\n}\n.canvas-wrapper[_ngcontent-%COMP%]   .placeholder-overlay[_ngcontent-%COMP%] {\n  position: absolute;\n  inset: 0;\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  text-align: center;\n  color: #cbd5e0;\n  pointer-events: none;\n  padding: 0 24px;\n}\n.canvas-wrapper[_ngcontent-%COMP%]   .placeholder-overlay[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  opacity: 0.7;\n  margin-bottom: 12px;\n}\n.canvas-wrapper[_ngcontent-%COMP%]   .placeholder-overlay[_ngcontent-%COMP%]   .placeholder-title[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.95rem;\n  font-weight: 500;\n}\n.canvas-wrapper[_ngcontent-%COMP%]   .placeholder-overlay[_ngcontent-%COMP%]   .placeholder-sub[_ngcontent-%COMP%] {\n  margin: 6px 0 0;\n  font-size: 0.78rem;\n  opacity: 0.7;\n  line-height: 1.4;\n}\n\n.hints[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: center;\n  gap: 16px;\n  flex-wrap: wrap;\n}\n.hints[_ngcontent-%COMP%]   .hint-item[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 0.72rem;\n  color: var(--mde-text-secondary);\n}\n.hints[_ngcontent-%COMP%]   .hint-item[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 14px;\n  width: 14px;\n  height: 14px;\n}\n\n.error-line[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  font-size: 0.78rem;\n  color: #e53e3e;\n}\n.error-line[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n\n.actions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  flex-wrap: wrap;\n}\n.actions[_ngcontent-%COMP%]   .spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  height: 34px;\n  line-height: 34px;\n  font-size: 0.82rem;\n  padding: 0 12px;\n}\n.actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-right: 4px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wcm9qZWN0LWVkaXQvaWNvbi1lZGl0b3IvaWNvbi1lZGl0b3IuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxjQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0FBQ0Y7O0FBRUE7RUFDRSxrQkFBQTtFQUNBLFlBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7RUFDQSwwQ0FBQTtFQUNBLHlCQUFBO1VBQUEsaUJBQUE7QUFDRjtBQUNFO0VBQ0UsZUFBQTtBQUNKO0FBRUU7RUFDRSxnQkFBQTtBQUFKO0FBR0U7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxvQkFBQTtBQURKO0FBSUU7RUFDRSxrQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0Esb0JBQUE7RUFDQSxlQUFBO0FBRko7QUFJSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtBQUZOO0FBS0k7RUFDRSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtBQUhOO0FBTUk7RUFDRSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFKTjs7QUFTQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFNBQUE7RUFDQSxlQUFBO0FBTkY7QUFRRTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQ0FBQTtBQU5KO0FBUUk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFOTjs7QUFXQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxrQkFBQTtFQUNBLGNBQUE7QUFSRjtBQVVFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBUko7O0FBWUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZUFBQTtBQVRGO0FBV0U7RUFDRSxPQUFBO0FBVEo7QUFZRTtFQUNFLFlBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtBQVZKO0FBWUk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtBQVZOIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xuICBkaXNwbGF5OiBibG9jaztcbn1cblxuLmljb24tZWRpdG9yIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgZ2FwOiAxMHB4O1xufVxuXG4uY2FudmFzLXdyYXBwZXIge1xuICBwb3NpdGlvbjogcmVsYXRpdmU7XG4gIHdpZHRoOiAzMjBweDtcbiAgaGVpZ2h0OiAzMjBweDtcbiAgbWFyZ2luOiAwIGF1dG87XG4gIGJvcmRlci1yYWRpdXM6IDEycHg7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIGN1cnNvcjogZ3JhYjtcbiAgYmFja2dyb3VuZDogIzBmMTQxOTtcbiAgYm94LXNoYWRvdzogMCAycHggMTJweCByZ2JhKDAsIDAsIDAsIDAuMjUpO1xuICB1c2VyLXNlbGVjdDogbm9uZTtcblxuICAmLmVtcHR5IHtcbiAgICBjdXJzb3I6IGRlZmF1bHQ7XG4gIH1cblxuICAmOmFjdGl2ZSB7XG4gICAgY3Vyc29yOiBncmFiYmluZztcbiAgfVxuXG4gIC5lZGl0b3ItY2FudmFzIHtcbiAgICBkaXNwbGF5OiBibG9jaztcbiAgICB3aWR0aDogMTAwJTtcbiAgICBoZWlnaHQ6IDEwMCU7XG4gICAgcG9pbnRlci1ldmVudHM6IG5vbmU7IC8vIG1vdXNlIGV2ZW50cyBoYW5kbGVkIG9uIHRoZSB3cmFwcGVyIGluc3RlYWRcbiAgfVxuXG4gIC5wbGFjZWhvbGRlci1vdmVybGF5IHtcbiAgICBwb3NpdGlvbjogYWJzb2x1dGU7XG4gICAgaW5zZXQ6IDA7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gICAgdGV4dC1hbGlnbjogY2VudGVyO1xuICAgIGNvbG9yOiAjY2JkNWUwO1xuICAgIHBvaW50ZXItZXZlbnRzOiBub25lO1xuICAgIHBhZGRpbmc6IDAgMjRweDtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogNDhweDtcbiAgICAgIHdpZHRoOiA0OHB4O1xuICAgICAgaGVpZ2h0OiA0OHB4O1xuICAgICAgb3BhY2l0eTogMC43O1xuICAgICAgbWFyZ2luLWJvdHRvbTogMTJweDtcbiAgICB9XG5cbiAgICAucGxhY2Vob2xkZXItdGl0bGUge1xuICAgICAgbWFyZ2luOiAwO1xuICAgICAgZm9udC1zaXplOiAwLjk1cmVtO1xuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcbiAgICB9XG5cbiAgICAucGxhY2Vob2xkZXItc3ViIHtcbiAgICAgIG1hcmdpbjogNnB4IDAgMDtcbiAgICAgIGZvbnQtc2l6ZTogMC43OHJlbTtcbiAgICAgIG9wYWNpdHk6IDAuNztcbiAgICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XG4gICAgfVxuICB9XG59XG5cbi5oaW50cyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICBnYXA6IDE2cHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcblxuICAuaGludC1pdGVtIHtcbiAgICBkaXNwbGF5OiBpbmxpbmUtZmxleDtcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgIGdhcDogNHB4O1xuICAgIGZvbnQtc2l6ZTogMC43MnJlbTtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMTRweDtcbiAgICAgIHdpZHRoOiAxNHB4O1xuICAgICAgaGVpZ2h0OiAxNHB4O1xuICAgIH1cbiAgfVxufVxuXG4uZXJyb3ItbGluZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogNnB4O1xuICBmb250LXNpemU6IDAuNzhyZW07XG4gIGNvbG9yOiAjZTUzZTNlO1xuXG4gIG1hdC1pY29uIHtcbiAgICBmb250LXNpemU6IDE2cHg7XG4gICAgd2lkdGg6IDE2cHg7XG4gICAgaGVpZ2h0OiAxNnB4O1xuICB9XG59XG5cbi5hY3Rpb25zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcblxuICAuc3BhY2VyIHtcbiAgICBmbGV4OiAxO1xuICB9XG5cbiAgYnV0dG9uIHtcbiAgICBoZWlnaHQ6IDM0cHg7XG4gICAgbGluZS1oZWlnaHQ6IDM0cHg7XG4gICAgZm9udC1zaXplOiAwLjgycmVtO1xuICAgIHBhZGRpbmc6IDAgMTJweDtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMTZweDtcbiAgICAgIHdpZHRoOiAxNnB4O1xuICAgICAgaGVpZ2h0OiAxNnB4O1xuICAgICAgbWFyZ2luLXJpZ2h0OiA0cHg7XG4gICAgfVxuICB9XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
    });
  }
}

/***/ }),

/***/ 9116:
/*!********************************************************************************!*\
  !*** ./src/app/projects/dialogs/project-edit/project-edit-dialog.component.ts ***!
  \********************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "ProjectEditDialogComponent": () => (/* binding */ ProjectEditDialogComponent)
/* harmony export */ });
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _icon_editor_icon_editor_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./icon-editor/icon-editor.component */ 1602);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @ngx-translate/core */ 8699);















function ProjectEditDialogComponent_mat_error_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-error");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECTS.NAME_REQUIRED"), " ");
  }
}
function ProjectEditDialogComponent_img_29_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "img", 31);
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("src", ctx_r1.iconPreviewUrl, _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵsanitizeUrl"]);
  }
}
function ProjectEditDialogComponent_mat_icon_30_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-icon", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "image");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
}
function ProjectEditDialogComponent_div_45_Template(rf, ctx) {
  if (rf & 1) {
    const _r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 33)(1, "app-icon-editor", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("iconChanged", function ProjectEditDialogComponent_div_45_Template_app_icon_editor_iconChanged_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r10);
      const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r9.onIconChanged($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("initialIconUrl", ctx_r3.iconPreviewUrl)("hasCustomIcon", ctx_r3.effectiveHasCustomIcon);
  }
}
function ProjectEditDialogComponent_div_46_span_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECTS.PROJECT_ICON_PENDING_SET"));
  }
}
function ProjectEditDialogComponent_div_46_span_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECTS.PROJECT_ICON_PENDING_REMOVE"));
  }
}
function ProjectEditDialogComponent_div_46_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 35)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, ProjectEditDialogComponent_div_46_span_3_Template, 3, 3, "span", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, ProjectEditDialogComponent_div_46_span_4_Template, 3, 3, "span", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r4.pendingIconAction === "set");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r4.pendingIconAction === "remove");
  }
}
function ProjectEditDialogComponent_div_62_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 36)(1, "mat-icon", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "sync");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 1, "PROJECTS.PARTICIPANTS_LOADING"), " ");
  }
}
function ProjectEditDialogComponent_div_63_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 38)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "error_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 1, "PROJECTS.PARTICIPANTS_LOAD_ERROR"), " ");
  }
}
function ProjectEditDialogComponent_div_64_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 39)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "people_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 1, "PROJECTS.PARTICIPANTS_EMPTY"), " ");
  }
}
function ProjectEditDialogComponent_div_65_div_1_mat_icon_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
}
function ProjectEditDialogComponent_div_65_div_1_mat_icon_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "person_add");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
}
function ProjectEditDialogComponent_div_65_div_1_button_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r23 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ProjectEditDialogComponent_div_65_div_1_button_9_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r23);
      const p_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
      const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r21.removeManual(p_r14));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](1, 1, "PROJECTS.PARTICIPANT_REMOVE"));
  }
}
function ProjectEditDialogComponent_div_65_div_1_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 54)(1, "span", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "span", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const p_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](3, 3, "PROJECTS.PARTICIPANT_GIT_EMAIL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("matTooltip", p_r14.gitEmail);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](p_r14.gitEmail);
  }
}
function ProjectEditDialogComponent_div_65_div_1_div_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECTS.PARTICIPANT_MANUAL_EMAIL_REQUIRED"), " ");
  }
}
function ProjectEditDialogComponent_div_65_div_1_div_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECTS.PARTICIPANT_CHAT_EMAIL_INVALID"), " ");
  }
}
const _c0 = function () {
  return {
    standalone: true
  };
};
function ProjectEditDialogComponent_div_65_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r26 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 42)(1, "div", 43)(2, "span", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](3, ProjectEditDialogComponent_div_65_div_1_mat_icon_3_Template, 2, 0, "mat-icon", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](4, ProjectEditDialogComponent_div_65_div_1_mat_icon_4_Template, 2, 0, "mat-icon", 7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "input", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function ProjectEditDialogComponent_div_65_div_1_Template_input_ngModelChange_7_listener($event) {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r26);
      const p_r14 = restoredCtx.$implicit;
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](p_r14.displayName = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](9, ProjectEditDialogComponent_div_65_div_1_button_9_Template, 4, 3, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](11, ProjectEditDialogComponent_div_65_div_1_div_11_Template, 6, 5, "div", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "div", 49)(13, "span", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](16, "input", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function ProjectEditDialogComponent_div_65_div_1_Template_input_ngModelChange_16_listener($event) {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r26);
      const p_r14 = restoredCtx.$implicit;
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](p_r14.chatEmail = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](17, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](18, ProjectEditDialogComponent_div_65_div_1_div_18_Template, 3, 3, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](19, ProjectEditDialogComponent_div_65_div_1_div_19_Template, 3, 3, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const p_r14 = ctx.$implicit;
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("manual", p_r14.manual)("invalid", ctx_r13.isManualInvalid(p_r14) || ctx_r13.isChatEmailInvalid(p_r14));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("manual-chip", p_r14.manual);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !p_r14.manual);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", p_r14.manual);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](6, 20, p_r14.manual ? "PROJECTS.PARTICIPANT_MANUAL" : "PROJECTS.PARTICIPANT_GIT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", p_r14.displayName)("ngModelOptions", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction0"](28, _c0))("placeholder", p_r14.gitName || _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](8, 22, "PROJECTS.PARTICIPANT_DISPLAY_NAME_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", p_r14.manual);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !p_r14.manual);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](15, 24, "PROJECTS.PARTICIPANT_CHAT_EMAIL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", p_r14.chatEmail)("ngModelOptions", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction0"](29, _c0))("placeholder", p_r14.gitEmail || _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](17, 26, "PROJECTS.PARTICIPANT_CHAT_EMAIL_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r13.isManualInvalid(p_r14));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r13.isManualInvalid(p_r14) && ctx_r13.isChatEmailInvalid(p_r14));
  }
}
function ProjectEditDialogComponent_div_65_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](1, ProjectEditDialogComponent_div_65_div_1_Template, 20, 30, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx_r8.participants)("ngForTrackBy", ctx_r8.trackByEmail);
  }
}
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
class ProjectEditDialogComponent {
  // Tracks whether the project currently has a custom icon, accounting for
  // pending changes (so "Rimuovi" inside the editor only renders when sensible).
  get effectiveHasCustomIcon() {
    if (this.pendingIconAction === 'remove') return false;
    if (this.pendingIconAction === 'set') return true;
    return !!this.data.hasCustomIcon;
  }
  constructor(fb, dialogRef, data, projectsService) {
    this.fb = fb;
    this.dialogRef = dialogRef;
    this.data = data;
    this.projectsService = projectsService;
    this.descriptionMaxLength = 200;
    this.nameMaxLength = 255;
    this.participants = [];
    this.loadingParticipants = false;
    this.participantsError = null;
    // Icon editor state — collapsed by default; the editor is non-trivial in
    // height so we only mount it when the user actually wants to customize.
    this.iconEditorOpen = false;
    // Pending icon change tracked locally so it participates in the dialog's
    // Save/Cancel flow alongside name/description/participants.
    this.pendingIconAction = 'none';
    this.pendingIconPngBase64 = null;
    // data URL used for the in-dialog preview (latest applied or current backend icon).
    this.iconPreviewUrl = null;
    this.trackByEmail = (_, p) => p.gitEmail || p.chatEmail || _;
    this.form = this.fb.group({
      name: [data.name ?? '', [_angular_forms__WEBPACK_IMPORTED_MODULE_3__.Validators.required, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.Validators.maxLength(this.nameMaxLength)]],
      description: [data.description ?? '', [_angular_forms__WEBPACK_IMPORTED_MODULE_3__.Validators.maxLength(this.descriptionMaxLength)]]
    });
    if (this.data.hasCustomIcon) {
      this.iconPreviewUrl = this.projectsService.getProjectIconUrl(this.data.id, this.data.iconUpdatedAt);
    }
  }
  ngOnInit() {
    this.loadParticipants();
  }
  get descriptionLength() {
    return (this.form.value.description ?? '').length;
  }
  loadParticipants() {
    this.loadingParticipants = true;
    this.participantsError = null;
    this.projectsService.getParticipants(this.data.path).subscribe({
      next: list => {
        // Deep clone so we can mutate without affecting other cached lists.
        this.participants = (list || []).map(p => ({
          ...p
        }));
        this.loadingParticipants = false;
      },
      error: err => {
        console.error('[ProjectEditDialog] Failed to load participants:', err);
        this.participantsError = 'load';
        this.loadingParticipants = false;
      }
    });
  }
  refreshFromGit() {
    // Merge fresh git authors with whatever the user has currently edited
    // (so in-flight edits aren't lost when the user clicks "Refresh").
    this.projectsService.getGitAuthors(this.data.path).subscribe({
      next: authors => {
        const known = new Map(this.participants.map(p => [p.gitEmail, p]));
        (authors || []).forEach(a => {
          const key = (a.email || '').trim().toLowerCase();
          if (!key) return;
          const existing = known.get(key);
          if (existing) {
            existing.gitName = a.name ?? existing.gitName;
            existing.manual = false;
          } else {
            this.participants.push({
              gitEmail: key,
              gitName: a.name,
              displayName: a.name,
              chatEmail: key,
              manual: false
            });
          }
        });
      },
      error: err => console.error('[ProjectEditDialog] Refresh from git failed:', err)
    });
  }
  addManual() {
    this.participants.push({
      gitEmail: '',
      gitName: '',
      displayName: '',
      chatEmail: '',
      manual: true
    });
  }
  removeManual(p) {
    this.participants = this.participants.filter(x => x !== p);
  }
  isChatEmailInvalid(p) {
    const v = (p.chatEmail || '').trim();
    if (!v) return false; // empty is allowed (button just won't render)
    return !EMAIL_REGEX.test(v);
  }
  isManualInvalid(p) {
    if (!p.manual) return false;
    const v = (p.chatEmail || '').trim();
    // Manual entries need a valid chat email — otherwise there is nothing to store.
    return !v || !EMAIL_REGEX.test(v);
  }
  get hasInvalidParticipants() {
    return this.participants.some(p => this.isChatEmailInvalid(p) || this.isManualInvalid(p));
  }
  toggleIconEditor() {
    this.iconEditorOpen = !this.iconEditorOpen;
  }
  onIconChanged(event) {
    if (event.pngBase64 === null) {
      this.pendingIconAction = 'remove';
      this.pendingIconPngBase64 = null;
      this.iconPreviewUrl = null;
    } else {
      this.pendingIconAction = 'set';
      this.pendingIconPngBase64 = event.pngBase64;
      this.iconPreviewUrl = event.pngBase64; // data URL is renderable as-is
    }
    console.debug('[ProjectEditDialog] icon changed, action=', this.pendingIconAction, 'pngLen=', event.pngBase64?.length ?? 0);
    // Auto-collapse so the user immediately sees the new preview chip;
    // they can re-open if they want another iteration.
    this.iconEditorOpen = false;
  }
  onCancel() {
    this.dialogRef.close();
  }
  onSave() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (this.hasInvalidParticipants) {
      return;
    }
    // Safety net: if the editor is still open with an image the user didn't
    // explicitly Apply, auto-apply now so the icon is not silently dropped.
    if (this.iconEditorOpen && this.iconEditor && this.iconEditor.hasImage && this.pendingIconAction === 'none') {
      this.iconEditor.applyAndEmit();
    }
    console.debug('[ProjectEditDialog] save, iconAction=', this.pendingIconAction, 'pngLen=', this.pendingIconPngBase64?.length ?? 0);
    const value = this.form.value;
    const cleaned = this.participants.map(p => ({
      gitEmail: (p.manual ? p.chatEmail || '' : p.gitEmail).trim().toLowerCase(),
      gitName: (p.gitName || '').trim(),
      displayName: (p.displayName || '').trim(),
      chatEmail: (p.chatEmail || p.gitEmail || '').trim(),
      manual: p.manual
    })).filter(p => !!p.gitEmail);
    this.dialogRef.close({
      id: this.data.id,
      name: (value.name ?? '').trim(),
      description: (value.description ?? '').trim(),
      path: this.data.path,
      participants: cleaned,
      iconAction: this.pendingIconAction,
      iconPngBase64: this.pendingIconPngBase64 ?? undefined
    });
  }
  static {
    this.ɵfac = function ProjectEditDialogComponent_Factory(t) {
      return new (t || ProjectEditDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_forms__WEBPACK_IMPORTED_MODULE_3__.FormBuilder), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__.ProjectsService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({
      type: ProjectEditDialogComponent,
      selectors: [["app-project-edit-dialog"]],
      viewQuery: function ProjectEditDialogComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵviewQuery"](_icon_editor_icon_editor_component__WEBPACK_IMPORTED_MODULE_0__.IconEditorComponent, 5);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵloadQuery"]()) && (ctx.iconEditor = _t.first);
        }
      },
      decls: 78,
      vars: 57,
      consts: [["mat-dialog-title", ""], [1, "title-icon"], [1, "project-edit-form", 3, "formGroup", "ngSubmit"], [1, "path-readonly", 3, "matTooltip"], [1, "path-text"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "formControlName", "name", "required", "", 3, "maxlength"], [4, "ngIf"], ["matInput", "", "formControlName", "description", "rows", "4", 3, "maxlength", "placeholder"], ["align", "end"], [1, "icon-section"], [1, "icon-section-header"], [1, "icon-preview-wrapper"], ["class", "icon-preview", "alt", "icon preview", 3, "src", 4, "ngIf"], ["class", "icon-preview-fallback", 4, "ngIf"], [1, "icon-section-text"], [1, "section-title"], [1, "section-hint"], ["mat-stroked-button", "", "type", "button", 1, "customize-btn", 3, "click"], ["class", "icon-editor-host", 4, "ngIf"], ["class", "pending-banner", 4, "ngIf"], [1, "participants-section"], [1, "participants-header"], ["mat-stroked-button", "", "type", "button", 1, "refresh-btn", 3, "click"], ["class", "participants-loading", 4, "ngIf"], ["class", "participants-error", 4, "ngIf"], ["class", "participants-empty", 4, "ngIf"], ["class", "participants-list", 4, "ngIf"], ["mat-stroked-button", "", "type", "button", 1, "add-manual-btn", 3, "click"], ["mat-button", "", "type", "button", 3, "click"], ["mat-flat-button", "", "color", "primary", "type", "submit", 3, "disabled"], ["alt", "icon preview", 1, "icon-preview", 3, "src"], [1, "icon-preview-fallback"], [1, "icon-editor-host"], [3, "initialIconUrl", "hasCustomIcon", "iconChanged"], [1, "pending-banner"], [1, "participants-loading"], [1, "spinner"], [1, "participants-error"], [1, "participants-empty"], [1, "participants-list"], ["class", "participant-row", 3, "manual", "invalid", 4, "ngFor", "ngForOf", "ngForTrackBy"], [1, "participant-row"], [1, "participant-main"], [1, "source-chip"], ["type", "text", 1, "inline-input", "display-name", 3, "ngModel", "ngModelOptions", "placeholder", "ngModelChange"], ["mat-icon-button", "", "type", "button", "class", "remove-btn", 3, "matTooltip", "click", 4, "ngIf"], [1, "participant-emails"], ["class", "git-email", 4, "ngIf"], [1, "chat-email"], [1, "email-label"], ["type", "email", 1, "inline-input", "chat-input", 3, "ngModel", "ngModelOptions", "placeholder", "ngModelChange"], ["class", "row-error", 4, "ngIf"], ["mat-icon-button", "", "type", "button", 1, "remove-btn", 3, "matTooltip", "click"], [1, "git-email"], [1, "email-value", "readonly", 3, "matTooltip"], [1, "row-error"]],
      template: function ProjectEditDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "h2", 0)(1, "mat-icon", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "edit");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](4, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "form", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngSubmit", function ProjectEditDialogComponent_Template_form_ngSubmit_5_listener() {
            return ctx.onSave();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "mat-dialog-content")(7, "p", 3)(8, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9, "folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "mat-form-field", 5)(13, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](14);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](15, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](16, "input", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](17, ProjectEditDialogComponent_mat_error_17_Template, 3, 3, "mat-error", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](18, "mat-form-field", 5)(19, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](22, "textarea", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "mat-hint", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](26, "div", 10)(27, "div", 11)(28, "div", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](29, ProjectEditDialogComponent_img_29_Template, 1, 1, "img", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](30, ProjectEditDialogComponent_mat_icon_30_Template, 2, 0, "mat-icon", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](31, "div", 15)(32, "h3", 16)(33, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](34, "image");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](35);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](36, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](37, "p", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](38);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](39, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](40, "button", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ProjectEditDialogComponent_Template_button_click_40_listener() {
            return ctx.toggleIconEditor();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](41, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](42);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](43);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](44, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](45, ProjectEditDialogComponent_div_45_Template, 2, 2, "div", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](46, ProjectEditDialogComponent_div_46_Template, 5, 2, "div", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](47, "div", 21)(48, "div", 22)(49, "h3", 16)(50, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](51, "group");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](52);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](53, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](54, "button", 23);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ProjectEditDialogComponent_Template_button_click_54_listener() {
            return ctx.refreshFromGit();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](55, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](56, "refresh");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](57);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](58, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](59, "p", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](60);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](61, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](62, ProjectEditDialogComponent_div_62_Template, 5, 3, "div", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](63, ProjectEditDialogComponent_div_63_Template, 5, 3, "div", 25);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](64, ProjectEditDialogComponent_div_64_Template, 5, 3, "div", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](65, ProjectEditDialogComponent_div_65_Template, 2, 2, "div", 27);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](66, "button", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ProjectEditDialogComponent_Template_button_click_66_listener() {
            return ctx.addManual();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](67, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](68, "person_add");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](69);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](70, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](71, "mat-dialog-actions", 9)(72, "button", 29);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ProjectEditDialogComponent_Template_button_click_72_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](73);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](74, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](75, "button", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](76);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](77, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](4, 31, "PROJECTS.EDIT_PROJECT_TITLE"), "\n");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("formGroup", ctx.form);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("matTooltip", ctx.data.path);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.data.path);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](15, 33, "PROJECTS.NAME"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("maxlength", ctx.nameMaxLength);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.form.get("name").hasError("required"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](21, 35, "PROJECTS.DESCRIPTION"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("maxlength", ctx.descriptionMaxLength)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](23, 37, "PROJECTS.DESCRIPTION_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate2"]("", ctx.descriptionLength, "/", ctx.descriptionMaxLength, "");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.iconPreviewUrl);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.iconPreviewUrl);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](36, 39, "PROJECTS.PROJECT_ICON"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](39, 41, "PROJECTS.PROJECT_ICON_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.iconEditorOpen ? "expand_less" : "edit");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](44, 43, ctx.iconEditorOpen ? "PROJECTS.PROJECT_ICON_CLOSE" : "PROJECTS.PROJECT_ICON_CUSTOMIZE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.iconEditorOpen);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.pendingIconAction !== "none");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](53, 45, "PROJECTS.PARTICIPANTS"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](58, 47, "PROJECTS.REFRESH_FROM_GIT"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](61, 49, "PROJECTS.PARTICIPANTS_HINT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.loadingParticipants);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.participantsError === "load");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.loadingParticipants && ctx.participants.length === 0 && !ctx.participantsError);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.loadingParticipants && ctx.participants.length > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](70, 51, "PROJECTS.ADD_PARTICIPANT"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](74, 53, "PROJECTS.CANCEL"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.form.invalid || ctx.hasInvalidParticipants);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](77, 55, "PROJECTS.SAVE"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_5__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_5__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_6__.MatLegacyError, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_6__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_6__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_6__.MatLegacyLabel, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_7__.MatLegacyInput, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_8__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__.MatIcon, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_4__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_10__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_3__["ɵNgNoValidate"], _angular_forms__WEBPACK_IMPORTED_MODULE_3__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.NgControlStatusGroup, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.MaxLengthValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.NgModel, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.FormGroupDirective, _angular_forms__WEBPACK_IMPORTED_MODULE_3__.FormControlName, _icon_editor_icon_editor_component__WEBPACK_IMPORTED_MODULE_0__.IconEditorComponent, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_11__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n  min-width: 520px;\n  max-width: 640px;\n}\n\nh2[mat-dialog-title][_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  color: var(--mde-text-primary);\n  margin: 0;\n}\nh2[mat-dialog-title][_ngcontent-%COMP%]   .title-icon[_ngcontent-%COMP%] {\n  font-size: 22px;\n  width: 22px;\n  height: 22px;\n  color: #667eea;\n}\n\n.project-edit-form[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.path-readonly[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 8px 12px;\n  background: var(--mde-bg-secondary);\n  border: 1px solid var(--mde-border-color);\n  border-radius: 6px;\n  font-size: 0.8rem;\n  color: var(--mde-text-secondary);\n  margin: 0 0 16px 0;\n  overflow: hidden;\n}\n.path-readonly[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #a0aec0;\n  flex-shrink: 0;\n}\n.path-readonly[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  flex: 1;\n  min-width: 0;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\ntextarea[_ngcontent-%COMP%] {\n  resize: vertical;\n  min-height: 80px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding-top: 8px;\n  gap: 8px;\n}\n\n.icon-section[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n  padding: 12px;\n  border: 1px solid var(--mde-border-color);\n  border-radius: 10px;\n  background: var(--mde-bg-secondary);\n  margin-top: 4px;\n}\n\n.icon-section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-preview-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 48px;\n  height: 48px;\n  border-radius: 12px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  overflow: hidden;\n  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-preview-wrapper[_ngcontent-%COMP%]   .icon-preview[_ngcontent-%COMP%] {\n  width: 100%;\n  height: 100%;\n  object-fit: cover;\n  display: block;\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-preview-wrapper[_ngcontent-%COMP%]   .icon-preview-fallback[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: rgba(255, 255, 255, 0.85);\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-section-text[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-section-text[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin: 0 0 4px 0;\n  font-size: 0.95rem;\n  font-weight: 600;\n  color: var(--mde-text-primary);\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-section-text[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #667eea;\n}\n.icon-section-header[_ngcontent-%COMP%]   .icon-section-text[_ngcontent-%COMP%]   .section-hint[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.78rem;\n  color: var(--mde-text-secondary);\n  line-height: 1.4;\n}\n.icon-section-header[_ngcontent-%COMP%]   .customize-btn[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  font-size: 0.8rem;\n  height: 32px;\n  line-height: 32px;\n  padding: 0 12px;\n}\n.icon-section-header[_ngcontent-%COMP%]   .customize-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-right: 4px;\n}\n\n.icon-editor-host[_ngcontent-%COMP%] {\n  padding: 6px 0;\n}\n\n.pending-banner[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  padding: 6px 10px;\n  font-size: 0.75rem;\n  color: #4a5568;\n  background: rgba(102, 126, 234, 0.1);\n  border-left: 3px solid #667eea;\n  border-radius: 4px;\n}\n.pending-banner[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  color: #667eea;\n}\n\n.participants-section[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 10px;\n  padding: 12px;\n  border: 1px solid var(--mde-border-color);\n  border-radius: 10px;\n  background: var(--mde-bg-secondary);\n  margin-top: 4px;\n}\n\n.participants-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  gap: 12px;\n}\n.participants-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin: 0;\n  font-size: 0.95rem;\n  font-weight: 600;\n  color: var(--mde-text-primary);\n}\n.participants-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #667eea;\n}\n.participants-header[_ngcontent-%COMP%]   .refresh-btn[_ngcontent-%COMP%] {\n  font-size: 0.8rem;\n  height: 30px;\n  line-height: 30px;\n  padding: 0 10px;\n}\n.participants-header[_ngcontent-%COMP%]   .refresh-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-right: 4px;\n}\n\n.section-hint[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.78rem;\n  color: var(--mde-text-secondary);\n  line-height: 1.4;\n}\n\n.participants-loading[_ngcontent-%COMP%], .participants-error[_ngcontent-%COMP%], .participants-empty[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 10px 4px;\n  font-size: 0.82rem;\n  color: var(--mde-text-secondary);\n}\n.participants-loading[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%], .participants-error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%], .participants-empty[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n.participants-loading.participants-error[_ngcontent-%COMP%], .participants-error.participants-error[_ngcontent-%COMP%], .participants-empty.participants-error[_ngcontent-%COMP%] {\n  color: #e53e3e;\n}\n.participants-loading[_ngcontent-%COMP%]   .spinner[_ngcontent-%COMP%], .participants-error[_ngcontent-%COMP%]   .spinner[_ngcontent-%COMP%], .participants-empty[_ngcontent-%COMP%]   .spinner[_ngcontent-%COMP%] {\n  animation: _ngcontent-%COMP%_pe-spin 1s linear infinite;\n}\n\n@keyframes _ngcontent-%COMP%_pe-spin {\n  from {\n    transform: rotate(0);\n  }\n  to {\n    transform: rotate(360deg);\n  }\n}\n.participants-list[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n  max-height: 280px;\n  overflow-y: auto;\n  padding-right: 4px;\n}\n.participants-list[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 6px;\n}\n.participants-list[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: var(--mde-scrollbar-thumb);\n  border-radius: 3px;\n}\n\n.participant-row[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 6px;\n  padding: 10px 10px 8px 10px;\n  background: var(--mde-bg-primary);\n  border: 1px solid var(--mde-border-color);\n  border-radius: 8px;\n  transition: border-color 0.15s ease;\n}\n.participant-row[_ngcontent-%COMP%]:hover {\n  border-color: rgba(102, 126, 234, 0.4);\n}\n.participant-row.manual[_ngcontent-%COMP%] {\n  border-left: 3px solid #f093fb;\n}\n.participant-row.invalid[_ngcontent-%COMP%] {\n  border-color: #e53e3e;\n}\n\n.participant-main[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n.participant-main[_ngcontent-%COMP%]   .source-chip[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 0.65rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.4px;\n  padding: 2px 8px;\n  border-radius: 10px;\n  background: rgba(102, 126, 234, 0.15);\n  color: #667eea;\n}\n.participant-main[_ngcontent-%COMP%]   .source-chip.manual-chip[_ngcontent-%COMP%] {\n  background: rgba(240, 147, 251, 0.18);\n  color: #d03eb8;\n}\n.participant-main[_ngcontent-%COMP%]   .source-chip[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 13px;\n  width: 13px;\n  height: 13px;\n}\n.participant-main[_ngcontent-%COMP%]   .display-name[_ngcontent-%COMP%] {\n  flex: 1;\n  font-size: 0.9rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n}\n.participant-main[_ngcontent-%COMP%]   .remove-btn[_ngcontent-%COMP%] {\n  width: 28px;\n  height: 28px;\n  line-height: 28px;\n}\n.participant-main[_ngcontent-%COMP%]   .remove-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  color: #a0aec0;\n}\n.participant-main[_ngcontent-%COMP%]   .remove-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #e53e3e;\n}\n\n.participant-emails[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 1fr;\n  gap: 8px;\n}\n@media (max-width: 560px) {\n  .participant-emails[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n  }\n}\n.participant-emails[_ngcontent-%COMP%]   .git-email[_ngcontent-%COMP%], .participant-emails[_ngcontent-%COMP%]   .chat-email[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 2px;\n  min-width: 0;\n}\n.participant-emails[_ngcontent-%COMP%]   .email-label[_ngcontent-%COMP%] {\n  font-size: 0.65rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.4px;\n  color: var(--mde-text-secondary);\n}\n.participant-emails[_ngcontent-%COMP%]   .email-value[_ngcontent-%COMP%] {\n  font-size: 0.78rem;\n  color: var(--mde-text-primary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n.participant-emails[_ngcontent-%COMP%]   .email-value.readonly[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n  font-style: italic;\n}\n\n.inline-input[_ngcontent-%COMP%] {\n  width: 100%;\n  background: transparent;\n  border: 1px solid transparent;\n  border-bottom: 1px solid var(--mde-border-color);\n  padding: 2px 4px;\n  font-family: inherit;\n  font-size: 0.82rem;\n  color: var(--mde-text-primary);\n  outline: none;\n  transition: border-color 0.15s ease, background 0.15s ease;\n}\n.inline-input[_ngcontent-%COMP%]:focus {\n  background: rgba(102, 126, 234, 0.05);\n  border-bottom-color: #667eea;\n}\n.inline-input[_ngcontent-%COMP%]::placeholder {\n  color: var(--mde-text-secondary);\n  opacity: 0.7;\n}\n.inline-input.chat-input[_ngcontent-%COMP%] {\n  font-size: 0.78rem;\n}\n\n.row-error[_ngcontent-%COMP%] {\n  font-size: 0.72rem;\n  color: #e53e3e;\n  padding-left: 4px;\n}\n\n.add-manual-btn[_ngcontent-%COMP%] {\n  align-self: flex-start;\n  font-size: 0.8rem;\n  height: 32px;\n  line-height: 32px;\n  padding: 0 12px;\n}\n.add-manual-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n  margin-right: 4px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wcm9qZWN0LWVkaXQvcHJvamVjdC1lZGl0LWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGNBQUE7RUFDQSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsOEJBQUE7RUFDQSxTQUFBO0FBQ0Y7QUFDRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUFDSjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLG1DQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQUY7QUFFRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7RUFDQSxjQUFBO0FBQUo7QUFHRTtFQUNFLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLE9BQUE7RUFDQSxZQUFBO0FBREo7O0FBS0E7RUFDRSxXQUFBO0FBRkY7O0FBS0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBRkY7O0FBS0E7RUFDRSxnQkFBQTtFQUNBLFFBQUE7QUFGRjs7QUFRQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUFDQSxhQUFBO0VBQ0EseUNBQUE7RUFDQSxtQkFBQTtFQUNBLG1DQUFBO0VBQ0EsZUFBQTtBQUxGOztBQVFBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtBQUxGO0FBT0U7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLDZEQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxnQkFBQTtFQUNBLHlDQUFBO0FBTEo7QUFPSTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBTE47QUFRSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGdDQUFBO0FBTk47QUFVRTtFQUNFLE9BQUE7RUFDQSxZQUFBO0FBUko7QUFVSTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSw4QkFBQTtBQVJOO0FBVU07RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBUlI7QUFZSTtFQUNFLFNBQUE7RUFDQSxrQkFBQTtFQUNBLGdDQUFBO0VBQ0EsZ0JBQUE7QUFWTjtBQWNFO0VBQ0UsY0FBQTtFQUNBLGlCQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0VBQ0EsZUFBQTtBQVpKO0FBY0k7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtBQVpOOztBQWlCQTtFQUNFLGNBQUE7QUFkRjs7QUFpQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGNBQUE7RUFDQSxvQ0FBQTtFQUNBLDhCQUFBO0VBQ0Esa0JBQUE7QUFkRjtBQWdCRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUFkSjs7QUFxQkE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0VBQ0EsYUFBQTtFQUNBLHlDQUFBO0VBQ0EsbUJBQUE7RUFDQSxtQ0FBQTtFQUNBLGVBQUE7QUFsQkY7O0FBcUJBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsOEJBQUE7RUFDQSxTQUFBO0FBbEJGO0FBb0JFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLFNBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsOEJBQUE7QUFsQko7QUFvQkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBbEJOO0FBc0JFO0VBQ0UsaUJBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7RUFDQSxlQUFBO0FBcEJKO0FBc0JJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7QUFwQk47O0FBeUJBO0VBQ0UsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxnQkFBQTtBQXRCRjs7QUF5QkE7OztFQUdFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7QUF0QkY7QUF3QkU7OztFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQXBCSjtBQXVCRTs7O0VBQ0UsY0FBQTtBQW5CSjtBQXNCRTs7O0VBQ0UscUNBQUE7QUFsQko7O0FBc0JBO0VBQ0U7SUFBTyxvQkFBQTtFQWxCUDtFQW1CQTtJQUFLLHlCQUFBO0VBaEJMO0FBQ0Y7QUFrQkE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0FBaEJGO0FBa0JFO0VBQ0UsVUFBQTtBQWhCSjtBQWtCRTtFQUNFLHNDQUFBO0VBQ0Esa0JBQUE7QUFoQko7O0FBb0JBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsUUFBQTtFQUNBLDJCQUFBO0VBQ0EsaUNBQUE7RUFDQSx5Q0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUNBQUE7QUFqQkY7QUFtQkU7RUFDRSxzQ0FBQTtBQWpCSjtBQW9CRTtFQUNFLDhCQUFBO0FBbEJKO0FBcUJFO0VBQ0UscUJBQUE7QUFuQko7O0FBdUJBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQXBCRjtBQXNCRTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHlCQUFBO0VBQ0EscUJBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EscUNBQUE7RUFDQSxjQUFBO0FBcEJKO0FBc0JJO0VBQ0UscUNBQUE7RUFDQSxjQUFBO0FBcEJOO0FBdUJJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBckJOO0FBeUJFO0VBQ0UsT0FBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSw4QkFBQTtBQXZCSjtBQTBCRTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7QUF4Qko7QUEwQkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBeEJOO0FBMkJJO0VBQ0UsY0FBQTtBQXpCTjs7QUE4QkE7RUFDRSxhQUFBO0VBQ0EsOEJBQUE7RUFDQSxRQUFBO0FBM0JGO0FBNkJFO0VBTEY7SUFNSSwwQkFBQTtFQTFCRjtBQUNGO0FBNEJFOztFQUVFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7RUFDQSxZQUFBO0FBMUJKO0FBNkJFO0VBQ0Usa0JBQUE7RUFDQSxnQkFBQTtFQUNBLHlCQUFBO0VBQ0EscUJBQUE7RUFDQSxnQ0FBQTtBQTNCSjtBQThCRTtFQUNFLGtCQUFBO0VBQ0EsOEJBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7QUE1Qko7QUE4Qkk7RUFDRSxnQ0FBQTtFQUNBLGtCQUFBO0FBNUJOOztBQWlDQTtFQUNFLFdBQUE7RUFDQSx1QkFBQTtFQUNBLDZCQUFBO0VBQ0EsZ0RBQUE7RUFDQSxnQkFBQTtFQUNBLG9CQUFBO0VBQ0Esa0JBQUE7RUFDQSw4QkFBQTtFQUNBLGFBQUE7RUFDQSwwREFBQTtBQTlCRjtBQWdDRTtFQUNFLHFDQUFBO0VBQ0EsNEJBQUE7QUE5Qko7QUFpQ0U7RUFDRSxnQ0FBQTtFQUNBLFlBQUE7QUEvQko7QUFrQ0U7RUFDRSxrQkFBQTtBQWhDSjs7QUFvQ0E7RUFDRSxrQkFBQTtFQUNBLGNBQUE7RUFDQSxpQkFBQTtBQWpDRjs7QUFvQ0E7RUFDRSxzQkFBQTtFQUNBLGlCQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0VBQ0EsZUFBQTtBQWpDRjtBQW1DRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBakNKIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xuICBkaXNwbGF5OiBibG9jaztcbiAgbWluLXdpZHRoOiA1MjBweDtcbiAgbWF4LXdpZHRoOiA2NDBweDtcbn1cblxuaDJbbWF0LWRpYWxvZy10aXRsZV0ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICBtYXJnaW46IDA7XG5cbiAgLnRpdGxlLWljb24ge1xuICAgIGZvbnQtc2l6ZTogMjJweDtcbiAgICB3aWR0aDogMjJweDtcbiAgICBoZWlnaHQ6IDIycHg7XG4gICAgY29sb3I6ICM2NjdlZWE7XG4gIH1cbn1cblxuLnByb2plY3QtZWRpdC1mb3JtIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgZ2FwOiA4cHg7XG59XG5cbi5wYXRoLXJlYWRvbmx5IHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG4gIHBhZGRpbmc6IDhweCAxMnB4O1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgYm9yZGVyOiAxcHggc29saWQgdmFyKC0tbWRlLWJvcmRlci1jb2xvcik7XG4gIGJvcmRlci1yYWRpdXM6IDZweDtcbiAgZm9udC1zaXplOiAwLjhyZW07XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICBtYXJnaW46IDAgMCAxNnB4IDA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICB3aWR0aDogMThweDtcbiAgICBoZWlnaHQ6IDE4cHg7XG4gICAgY29sb3I6ICNhMGFlYzA7XG4gICAgZmxleC1zaHJpbms6IDA7XG4gIH1cblxuICAucGF0aC10ZXh0IHtcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICAgIG92ZXJmbG93OiBoaWRkZW47XG4gICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XG4gICAgZmxleDogMTtcbiAgICBtaW4td2lkdGg6IDA7XG4gIH1cbn1cblxuLmZ1bGwtd2lkdGgge1xuICB3aWR0aDogMTAwJTtcbn1cblxudGV4dGFyZWEge1xuICByZXNpemU6IHZlcnRpY2FsO1xuICBtaW4taGVpZ2h0OiA4MHB4O1xufVxuXG5tYXQtZGlhbG9nLWFjdGlvbnMge1xuICBwYWRkaW5nLXRvcDogOHB4O1xuICBnYXA6IDhweDtcbn1cblxuLy8gw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG4vLyBQcm9qZWN0IGljb24gc2VjdGlvblxuLy8gw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG4uaWNvbi1zZWN0aW9uIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgZ2FwOiAxMnB4O1xuICBwYWRkaW5nOiAxMnB4O1xuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcbiAgYm9yZGVyLXJhZGl1czogMTBweDtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXNlY29uZGFyeSk7XG4gIG1hcmdpbi10b3A6IDRweDtcbn1cblxuLmljb24tc2VjdGlvbi1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDEycHg7XG5cbiAgLmljb24tcHJldmlldy13cmFwcGVyIHtcbiAgICBmbGV4LXNocmluazogMDtcbiAgICB3aWR0aDogNDhweDtcbiAgICBoZWlnaHQ6IDQ4cHg7XG4gICAgYm9yZGVyLXJhZGl1czogMTJweDtcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xuICAgIGJveC1zaGFkb3c6IDAgMnB4IDZweCByZ2JhKDAsIDAsIDAsIDAuMTUpO1xuXG4gICAgLmljb24tcHJldmlldyB7XG4gICAgICB3aWR0aDogMTAwJTtcbiAgICAgIGhlaWdodDogMTAwJTtcbiAgICAgIG9iamVjdC1maXQ6IGNvdmVyO1xuICAgICAgZGlzcGxheTogYmxvY2s7XG4gICAgfVxuXG4gICAgLmljb24tcHJldmlldy1mYWxsYmFjayB7XG4gICAgICBmb250LXNpemU6IDI0cHg7XG4gICAgICB3aWR0aDogMjRweDtcbiAgICAgIGhlaWdodDogMjRweDtcbiAgICAgIGNvbG9yOiByZ2JhKDI1NSwgMjU1LCAyNTUsIDAuODUpO1xuICAgIH1cbiAgfVxuXG4gIC5pY29uLXNlY3Rpb24tdGV4dCB7XG4gICAgZmxleDogMTtcbiAgICBtaW4td2lkdGg6IDA7XG5cbiAgICAuc2VjdGlvbi10aXRsZSB7XG4gICAgICBkaXNwbGF5OiBmbGV4O1xuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgIGdhcDogOHB4O1xuICAgICAgbWFyZ2luOiAwIDAgNHB4IDA7XG4gICAgICBmb250LXNpemU6IDAuOTVyZW07XG4gICAgICBmb250LXdlaWdodDogNjAwO1xuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuXG4gICAgICBtYXQtaWNvbiB7XG4gICAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgICAgd2lkdGg6IDE4cHg7XG4gICAgICAgIGhlaWdodDogMThweDtcbiAgICAgICAgY29sb3I6ICM2NjdlZWE7XG4gICAgICB9XG4gICAgfVxuXG4gICAgLnNlY3Rpb24taGludCB7XG4gICAgICBtYXJnaW46IDA7XG4gICAgICBmb250LXNpemU6IDAuNzhyZW07XG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XG4gICAgfVxuICB9XG5cbiAgLmN1c3RvbWl6ZS1idG4ge1xuICAgIGZsZXgtc2hyaW5rOiAwO1xuICAgIGZvbnQtc2l6ZTogMC44cmVtO1xuICAgIGhlaWdodDogMzJweDtcbiAgICBsaW5lLWhlaWdodDogMzJweDtcbiAgICBwYWRkaW5nOiAwIDEycHg7XG5cbiAgICBtYXQtaWNvbiB7XG4gICAgICBmb250LXNpemU6IDE2cHg7XG4gICAgICB3aWR0aDogMTZweDtcbiAgICAgIGhlaWdodDogMTZweDtcbiAgICAgIG1hcmdpbi1yaWdodDogNHB4O1xuICAgIH1cbiAgfVxufVxuXG4uaWNvbi1lZGl0b3ItaG9zdCB7XG4gIHBhZGRpbmc6IDZweCAwO1xufVxuXG4ucGVuZGluZy1iYW5uZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDZweDtcbiAgcGFkZGluZzogNnB4IDEwcHg7XG4gIGZvbnQtc2l6ZTogMC43NXJlbTtcbiAgY29sb3I6ICM0YTU1Njg7XG4gIGJhY2tncm91bmQ6IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4xKTtcbiAgYm9yZGVyLWxlZnQ6IDNweCBzb2xpZCAjNjY3ZWVhO1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogMTZweDtcbiAgICB3aWR0aDogMTZweDtcbiAgICBoZWlnaHQ6IDE2cHg7XG4gICAgY29sb3I6ICM2NjdlZWE7XG4gIH1cbn1cblxuLy8gw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG4vLyBQYXJ0aWNpcGFudHMgc2VjdGlvblxuLy8gw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG4ucGFydGljaXBhbnRzLXNlY3Rpb24ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDEwcHg7XG4gIHBhZGRpbmc6IDEycHg7XG4gIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICBib3JkZXItcmFkaXVzOiAxMHB4O1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgbWFyZ2luLXRvcDogNHB4O1xufVxuXG4ucGFydGljaXBhbnRzLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcbiAgZ2FwOiAxMnB4O1xuXG4gIC5zZWN0aW9uLXRpdGxlIHtcbiAgICBkaXNwbGF5OiBmbGV4O1xuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gICAgZ2FwOiA4cHg7XG4gICAgbWFyZ2luOiAwO1xuICAgIGZvbnQtc2l6ZTogMC45NXJlbTtcbiAgICBmb250LXdlaWdodDogNjAwO1xuICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgIHdpZHRoOiAxOHB4O1xuICAgICAgaGVpZ2h0OiAxOHB4O1xuICAgICAgY29sb3I6ICM2NjdlZWE7XG4gICAgfVxuICB9XG5cbiAgLnJlZnJlc2gtYnRuIHtcbiAgICBmb250LXNpemU6IDAuOHJlbTtcbiAgICBoZWlnaHQ6IDMwcHg7XG4gICAgbGluZS1oZWlnaHQ6IDMwcHg7XG4gICAgcGFkZGluZzogMCAxMHB4O1xuXG4gICAgbWF0LWljb24ge1xuICAgICAgZm9udC1zaXplOiAxNnB4O1xuICAgICAgd2lkdGg6IDE2cHg7XG4gICAgICBoZWlnaHQ6IDE2cHg7XG4gICAgICBtYXJnaW4tcmlnaHQ6IDRweDtcbiAgICB9XG4gIH1cbn1cblxuLnNlY3Rpb24taGludCB7XG4gIG1hcmdpbjogMDtcbiAgZm9udC1zaXplOiAwLjc4cmVtO1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgbGluZS1oZWlnaHQ6IDEuNDtcbn1cblxuLnBhcnRpY2lwYW50cy1sb2FkaW5nLFxuLnBhcnRpY2lwYW50cy1lcnJvcixcbi5wYXJ0aWNpcGFudHMtZW1wdHkge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTBweCA0cHg7XG4gIGZvbnQtc2l6ZTogMC44MnJlbTtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICB3aWR0aDogMThweDtcbiAgICBoZWlnaHQ6IDE4cHg7XG4gIH1cblxuICAmLnBhcnRpY2lwYW50cy1lcnJvciB7XG4gICAgY29sb3I6ICNlNTNlM2U7XG4gIH1cblxuICAuc3Bpbm5lciB7XG4gICAgYW5pbWF0aW9uOiBwZS1zcGluIDFzIGxpbmVhciBpbmZpbml0ZTtcbiAgfVxufVxuXG5Aa2V5ZnJhbWVzIHBlLXNwaW4ge1xuICBmcm9tIHsgdHJhbnNmb3JtOiByb3RhdGUoMCk7IH1cbiAgdG8geyB0cmFuc2Zvcm06IHJvdGF0ZSgzNjBkZWcpOyB9XG59XG5cbi5wYXJ0aWNpcGFudHMtbGlzdCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogOHB4O1xuICBtYXgtaGVpZ2h0OiAyODBweDtcbiAgb3ZlcmZsb3cteTogYXV0bztcbiAgcGFkZGluZy1yaWdodDogNHB4O1xuXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyIHtcbiAgICB3aWR0aDogNnB4O1xuICB9XG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRodW1iIHtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtc2Nyb2xsYmFyLXRodW1iKTtcbiAgICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIH1cbn1cblxuLnBhcnRpY2lwYW50LXJvdyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogNnB4O1xuICBwYWRkaW5nOiAxMHB4IDEwcHggOHB4IDEwcHg7XG4gIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1wcmltYXJ5KTtcbiAgYm9yZGVyOiAxcHggc29saWQgdmFyKC0tbWRlLWJvcmRlci1jb2xvcik7XG4gIGJvcmRlci1yYWRpdXM6IDhweDtcbiAgdHJhbnNpdGlvbjogYm9yZGVyLWNvbG9yIDAuMTVzIGVhc2U7XG5cbiAgJjpob3ZlciB7XG4gICAgYm9yZGVyLWNvbG9yOiByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuNCk7XG4gIH1cblxuICAmLm1hbnVhbCB7XG4gICAgYm9yZGVyLWxlZnQ6IDNweCBzb2xpZCAjZjA5M2ZiO1xuICB9XG5cbiAgJi5pbnZhbGlkIHtcbiAgICBib3JkZXItY29sb3I6ICNlNTNlM2U7XG4gIH1cbn1cblxuLnBhcnRpY2lwYW50LW1haW4ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcblxuICAuc291cmNlLWNoaXAge1xuICAgIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gICAgZ2FwOiA0cHg7XG4gICAgZm9udC1zaXplOiAwLjY1cmVtO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgICBsZXR0ZXItc3BhY2luZzogMC40cHg7XG4gICAgcGFkZGluZzogMnB4IDhweDtcbiAgICBib3JkZXItcmFkaXVzOiAxMHB4O1xuICAgIGJhY2tncm91bmQ6IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4xNSk7XG4gICAgY29sb3I6ICM2NjdlZWE7XG5cbiAgICAmLm1hbnVhbC1jaGlwIHtcbiAgICAgIGJhY2tncm91bmQ6IHJnYmEoMjQwLCAxNDcsIDI1MSwgMC4xOCk7XG4gICAgICBjb2xvcjogI2QwM2ViODtcbiAgICB9XG5cbiAgICBtYXQtaWNvbiB7XG4gICAgICBmb250LXNpemU6IDEzcHg7XG4gICAgICB3aWR0aDogMTNweDtcbiAgICAgIGhlaWdodDogMTNweDtcbiAgICB9XG4gIH1cblxuICAuZGlzcGxheS1uYW1lIHtcbiAgICBmbGV4OiAxO1xuICAgIGZvbnQtc2l6ZTogMC45cmVtO1xuICAgIGZvbnQtd2VpZ2h0OiA1MDA7XG4gICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICB9XG5cbiAgLnJlbW92ZS1idG4ge1xuICAgIHdpZHRoOiAyOHB4O1xuICAgIGhlaWdodDogMjhweDtcbiAgICBsaW5lLWhlaWdodDogMjhweDtcblxuICAgIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMTZweDtcbiAgICAgIHdpZHRoOiAxNnB4O1xuICAgICAgaGVpZ2h0OiAxNnB4O1xuICAgICAgY29sb3I6ICNhMGFlYzA7XG4gICAgfVxuXG4gICAgJjpob3ZlciBtYXQtaWNvbiB7XG4gICAgICBjb2xvcjogI2U1M2UzZTtcbiAgICB9XG4gIH1cbn1cblxuLnBhcnRpY2lwYW50LWVtYWlscyB7XG4gIGRpc3BsYXk6IGdyaWQ7XG4gIGdyaWQtdGVtcGxhdGUtY29sdW1uczogMWZyIDFmcjtcbiAgZ2FwOiA4cHg7XG5cbiAgQG1lZGlhIChtYXgtd2lkdGg6IDU2MHB4KSB7XG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnI7XG4gIH1cblxuICAuZ2l0LWVtYWlsLFxuICAuY2hhdC1lbWFpbCB7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICAgIGdhcDogMnB4O1xuICAgIG1pbi13aWR0aDogMDtcbiAgfVxuXG4gIC5lbWFpbC1sYWJlbCB7XG4gICAgZm9udC1zaXplOiAwLjY1cmVtO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgICBsZXR0ZXItc3BhY2luZzogMC40cHg7XG4gICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gIH1cblxuICAuZW1haWwtdmFsdWUge1xuICAgIGZvbnQtc2l6ZTogMC43OHJlbTtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtcHJpbWFyeSk7XG4gICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xuICAgIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xuXG4gICAgJi5yZWFkb25seSB7XG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICAgIGZvbnQtc3R5bGU6IGl0YWxpYztcbiAgICB9XG4gIH1cbn1cblxuLmlubGluZS1pbnB1dCB7XG4gIHdpZHRoOiAxMDAlO1xuICBiYWNrZ3JvdW5kOiB0cmFuc3BhcmVudDtcbiAgYm9yZGVyOiAxcHggc29saWQgdHJhbnNwYXJlbnQ7XG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcbiAgcGFkZGluZzogMnB4IDRweDtcbiAgZm9udC1mYW1pbHk6IGluaGVyaXQ7XG4gIGZvbnQtc2l6ZTogMC44MnJlbTtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICBvdXRsaW5lOiBub25lO1xuICB0cmFuc2l0aW9uOiBib3JkZXItY29sb3IgMC4xNXMgZWFzZSwgYmFja2dyb3VuZCAwLjE1cyBlYXNlO1xuXG4gICY6Zm9jdXMge1xuICAgIGJhY2tncm91bmQ6IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4wNSk7XG4gICAgYm9yZGVyLWJvdHRvbS1jb2xvcjogIzY2N2VlYTtcbiAgfVxuXG4gICY6OnBsYWNlaG9sZGVyIHtcbiAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICBvcGFjaXR5OiAwLjc7XG4gIH1cblxuICAmLmNoYXQtaW5wdXQge1xuICAgIGZvbnQtc2l6ZTogMC43OHJlbTtcbiAgfVxufVxuXG4ucm93LWVycm9yIHtcbiAgZm9udC1zaXplOiAwLjcycmVtO1xuICBjb2xvcjogI2U1M2UzZTtcbiAgcGFkZGluZy1sZWZ0OiA0cHg7XG59XG5cbi5hZGQtbWFudWFsLWJ0biB7XG4gIGFsaWduLXNlbGY6IGZsZXgtc3RhcnQ7XG4gIGZvbnQtc2l6ZTogMC44cmVtO1xuICBoZWlnaHQ6IDMycHg7XG4gIGxpbmUtaGVpZ2h0OiAzMnB4O1xuICBwYWRkaW5nOiAwIDEycHg7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogMTZweDtcbiAgICB3aWR0aDogMTZweDtcbiAgICBoZWlnaHQ6IDE2cHg7XG4gICAgbWFyZ2luLXJpZ2h0OiA0cHg7XG4gIH1cbn1cbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

/***/ }),

/***/ 2555:
/*!******************************************************************************************!*\
  !*** ./src/app/projects/dialogs/template-showcase/template-showcase-dialog.component.ts ***!
  \******************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "TemplateShowcaseDialogComponent": () => (/* binding */ TemplateShowcaseDialogComponent)
/* harmony export */ });
/* harmony import */ var _home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs */ 6078);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs */ 4363);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/platform-browser */ 4497);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _services_theme_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../services/theme.service */ 8140);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/forms */ 2508);




















const _c0 = ["otherErrorText"];
const _c1 = function (a0) {
  return {
    languages: a0
  };
};
function TemplateShowcaseDialogComponent_ng_container_13_div_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const f_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().ngIf;
    const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](2, 1, "PROJECT_CONFIG.TEMPLATE_ONLY_IN", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](4, _c1, ctx_r15.languageNames(f_r14))));
  }
}
function TemplateShowcaseDialogComponent_ng_container_13_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const f_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().ngIf;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](f_r14.url);
  }
}
function TemplateShowcaseDialogComponent_ng_container_13_ng_container_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementContainer"](0);
  }
}
const _c2 = function (a0) {
  return {
    $implicit: a0
  };
};
function TemplateShowcaseDialogComponent_ng_container_13_Template(rf, ctx) {
  if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "label", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_ng_container_13_Template_div_click_4_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r21);
      const f_r14 = restoredCtx.ngIf;
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r20.show(f_r14));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-checkbox", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_ng_container_13_Template_mat_checkbox_click_5_listener($event) {
      return $event.stopPropagation();
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "div", 16)(8, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](10, TemplateShowcaseDialogComponent_ng_container_13_div_10_Template, 3, 6, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](11, TemplateShowcaseDialogComponent_ng_container_13_div_11_Template, 2, 1, "div", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](12, TemplateShowcaseDialogComponent_ng_container_13_ng_container_12_Template, 1, 0, "ng-container", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const f_r14 = ctx.ngIf;
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵreference"](47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](3, 12, "TEMPLATE_SHOWCASE.COMMON_LAYER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("viewed", ctx_r0.viewed === f_r14)("mute", !ctx_r0.speaks(f_r14));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r0.isIn(f_r14))("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](6, 14, "TEMPLATE_SHOWCASE.COMMON_LAYER_ALWAYS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](f_r14.title);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r0.speaks(f_r14));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", f_r14.byAddress);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngTemplateOutlet", _r5)("ngTemplateOutletContext", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](16, _c2, f_r14));
  }
}
function TemplateShowcaseDialogComponent_p_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "p", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r1.catalogError);
  }
}
function TemplateShowcaseDialogComponent_p_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "p", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "PROJECT_CONFIG.TEMPLATE_NO_METHODS"));
  }
}
function TemplateShowcaseDialogComponent_div_20_div_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](2, 1, "PROJECT_CONFIG.TEMPLATE_ONLY_IN", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](4, _c1, ctx_r24.languageNames(m_r23))));
  }
}
function TemplateShowcaseDialogComponent_div_20_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](m_r23.url);
  }
}
function TemplateShowcaseDialogComponent_div_20_ng_container_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementContainer"](0);
  }
}
function TemplateShowcaseDialogComponent_div_20_Template(rf, ctx) {
  if (rf & 1) {
    const _r30 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_div_20_Template_div_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r30);
      const m_r23 = restoredCtx.$implicit;
      const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r29.show(m_r23));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_div_20_Template_mat_checkbox_ngModelChange_1_listener($event) {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r30);
      const m_r23 = restoredCtx.$implicit;
      const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r31.chosen[m_r23.url] = $event);
    })("click", function TemplateShowcaseDialogComponent_div_20_Template_mat_checkbox_click_1_listener($event) {
      return $event.stopPropagation();
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "div", 16)(3, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, TemplateShowcaseDialogComponent_div_20_div_5_Template, 3, 6, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, TemplateShowcaseDialogComponent_div_20_div_6_Template, 2, 1, "div", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](7, TemplateShowcaseDialogComponent_div_20_ng_container_7_Template, 1, 0, "ng-container", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const m_r23 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵreference"](47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("viewed", ctx_r3.viewed === m_r23)("mute", !ctx_r3.speaks(m_r23));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r3.chosen[m_r23.url])("disabled", !ctx_r3.speaks(m_r23) || !ctx_r3.composing);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](m_r23.title);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r3.speaks(m_r23));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", m_r23.byAddress);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngTemplateOutlet", _r5)("ngTemplateOutletContext", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](11, _c2, m_r23));
  }
}
function TemplateShowcaseDialogComponent_div_38_div_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const b_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](2, 1, "PROJECT_CONFIG.TEMPLATE_ONLY_IN", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpureFunction1"](4, _c1, ctx_r34.languageNames(b_r33))));
  }
}
function TemplateShowcaseDialogComponent_div_38_div_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const b_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](b_r33.url);
  }
}
function TemplateShowcaseDialogComponent_div_38_Template(rf, ctx) {
  if (rf & 1) {
    const _r39 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_div_38_Template_div_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r39);
      const b_r33 = restoredCtx.$implicit;
      const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r38.show(b_r33));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-radio-button", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_div_38_Template_mat_radio_button_click_1_listener($event) {
      return $event.stopPropagation();
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "div", 16)(3, "div", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](8, TemplateShowcaseDialogComponent_div_38_div_8_Template, 3, 6, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](9, TemplateShowcaseDialogComponent_div_38_div_9_Template, 2, 1, "div", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const b_r33 = ctx.$implicit;
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("viewed", ctx_r4.viewed === b_r33)("mute", !ctx_r4.speaks(b_r33));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", b_r33.url)("disabled", !ctx_r4.speaks(b_r33));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](b_r33.title);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](7, 10, "TEMPLATE_SHOWCASE.DEMO_ALONE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r4.speaks(b_r33));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", b_r33.byAddress);
  }
}
function TemplateShowcaseDialogComponent_ng_template_46_mat_checkbox_0_Template(rf, ctx) {
  if (rf & 1) {
    const _r44 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_ng_template_46_mat_checkbox_0_Template_mat_checkbox_ngModelChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r44);
      const item_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
      const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵresetView"](ctx_r43.theory[item_r41.url] = $event);
    })("click", function TemplateShowcaseDialogComponent_ng_template_46_mat_checkbox_0_Template_mat_checkbox_click_0_listener($event) {
      return $event.stopPropagation();
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const item_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx_r42.theory[item_r41.url])("disabled", !ctx_r42.isIn(item_r41))("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](1, 4, "TEMPLATE_SHOWCASE.THEORY_TIP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](3, 6, "TEMPLATE_SHOWCASE.THEORY"), " ");
  }
}
function TemplateShowcaseDialogComponent_ng_template_46_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](0, TemplateShowcaseDialogComponent_ng_template_46_mat_checkbox_0_Template, 4, 8, "mat-checkbox", 49);
  }
  if (rf & 2) {
    const item_r41 = ctx.$implicit;
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r6.hasTheory(item_r41));
  }
}
function TemplateShowcaseDialogComponent_p_58_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "p", 50, 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r7.otherError);
  }
}
function TemplateShowcaseDialogComponent_mat_option_65_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-option", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const l_r49 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", l_r49.code);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](l_r49.name);
  }
}
function TemplateShowcaseDialogComponent_span_70_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r9.deckOnlyIn);
  }
}
function TemplateShowcaseDialogComponent_mat_spinner_72_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "mat-spinner", 54);
  }
}
function TemplateShowcaseDialogComponent_div_73_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 55)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "cloud_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "p", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](5, 2, "TEMPLATE_SHOWCASE.UNREACHABLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r11.deckError);
  }
}
function TemplateShowcaseDialogComponent_iframe_74_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "iframe", 56);
  }
  if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("src", ctx_r12.deckUrl, _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵsanitizeResourceUrl"]);
  }
}
function TemplateShowcaseDialogComponent_span_79_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](2, 1, "TEMPLATE_SHOWCASE.CHOOSE_A_METHOD"));
  }
}
/**
 * La vetrina di «Crea nuovo progetto» (sprint 2026-10-19-Vetrina-Dei-Template.md, VD1; 2026-10-19-Metodi-Componibili.md,
 * M4): a sinistra tre modi di far nascere il progetto — comporlo (lo strato comune, sempre, più i metodi spuntati, ognuno
 * con la sua teoria se la si chiede), la demo che ha tutto incluso, o la cartella così com'è — e la lingua in fondo; a
 * destra le slide del template che si guarda. Guardare (clic sulla riga) non è scegliere (pallino o casella). La cartella
 * e la configurazione vengono dopo, nella finestra che apre chi la chiama.
 */
class TemplateShowcaseDialogComponent {
  static {
    this.Compose = 'compose';
  }
  constructor(dialogRef, http, sanitizer, translate, themeService) {
    this.dialogRef = dialogRef;
    this.http = http;
    this.sanitizer = sanitizer;
    this.translate = translate;
    this.languages = [{
      code: 'it',
      name: 'italiano'
    }, {
      code: 'en',
      name: 'English'
    }];
    this.language = 'it';
    /** La città scelta, e con lei la sua documentazione (CF1, CF3). */
    this.agentCity = false;
    this.agentCityGuide = false;
    /** Lo strato comune (MC2): null finché l'elenco non ne ha uno; allora si compone con i soli metodi. */
    this.foundation = null;
    /** I template di progetto, cioè la demo: hanno tutto incluso e stanno da soli (MC6). */
    this.bases = [];
    this.methods = [];
    this.catalogLoaded = false;
    this.catalogError = '';
    /** Come nasce il progetto: {@link Compose}, l'indirizzo di una demo, o '' = la cartella così com'è. */
    this.mode = TemplateShowcaseDialogComponent.Compose;
    this.chosen = {};
    /** La teoria chiesta, per indirizzo (MC5). */
    this.theory = {};
    this.deckUrl = null;
    this.deckLoading = false;
    this.deckError = '';
    /** «Solo in: italiano» sopra il deck, quando le slide sono in un'altra lingua. */
    this.deckOnlyIn = '';
    this.otherUrl = '';
    this.otherError = '';
    this.otherLoading = false;
    this.theme = 'light';
    /** Quello che il servizio ha detto di un template in una lingua: non si richiede a ogni clic. */
    this.described = new Map();
    this.subscriptions = new rxjs__WEBPACK_IMPORTED_MODULE_3__.Subscription();
    this.subscriptions.add(themeService.currentTheme$.subscribe(t => this.theme = t));
    this.empty = {
      url: null,
      id: null,
      kind: 'empty',
      languages: ['it', 'en'],
      title: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY'),
      description: this.translate.instant('TEMPLATE_SHOWCASE.EMPTY_DESC')
    };
    this.city = {
      url: null,
      id: null,
      kind: 'city',
      languages: ['it', 'en'],
      title: this.translate.instant('TEMPLATE_SHOWCASE.AGENT_CITY'),
      description: this.translate.instant('TEMPLATE_SHOWCASE.AGENT_CITY_DESC')
    };
    this.viewed = this.empty;
  }
  ngOnInit() {
    this.language = this.translate.currentLang === 'en' ? 'en' : 'it';
    this.http.get('../api/MdProjects/TemplateCatalog').subscribe({
      next: res => {
        const items = (res?.templates || []).map(t => ({
          url: t.url,
          id: t.id,
          kind: t.kind,
          languages: t.languages || [],
          title: this.text(t.title),
          description: this.text(t.description)
        }));
        this.foundation = items.find(i => i.kind === 'foundation') || null;
        this.bases = items.filter(i => i.kind === 'project');
        this.methods = items.filter(i => i.kind === 'method');
        this.catalogLoaded = true;
        // Senza lo strato comune si parte dalla cartella così com'è, come prima dei metodi componibili.
        if (this.foundation) this.show(this.foundation);else this.mode = '';
        this.describeComposable();
      },
      error: err => {
        this.catalogLoaded = true;
        this.catalogError = err?.error?.error || err?.message || '';
      }
    });
    this.show(this.empty);
  }
  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }
  get composing() {
    return this.mode === TemplateShowcaseDialogComponent.Compose;
  }
  /** Le righe che si compongono: lo strato comune (se c'è) e i metodi. */
  get composable() {
    return [...(this.foundation ? [this.foundation] : []), ...this.methods];
  }
  /** La riga entra nel progetto: lo strato comune sempre, un metodo se spuntato; tutto solo componendo. */
  isIn(item) {
    return this.composing && this.speaks(item) && (item === this.foundation || !!this.chosen[item.url]);
  }
  /** Il template ha la teoria nella lingua scelta: lo si sa dopo averlo letto (le righe si leggono in sottofondo). */
  hasTheory(item) {
    return !!this.described.get(`${this.language}|${item.url}`)?.theory;
  }
  /** Si legge in sottofondo ogni riga componibile, per sapere quali hanno la teoria; un errore lo mostra la riga guardata. */
  describeComposable() {
    for (const item of this.composable) this.describe(item.url).catch(() => {});
  }
  /** Il template c'è nella lingua scelta: un metodo che non la parla non si spunta (VD5). */
  speaks(item) {
    return item.languages.includes(this.language);
  }
  /** I nomi delle lingue del template nella lingua dell'interfaccia, come li scrive la slide di circostanza. */
  languageNames(item) {
    const it = this.translate.currentLang === 'it';
    const names = it ? {
      it: 'italiano',
      en: 'inglese'
    } : {
      it: 'Italian',
      en: 'English'
    };
    return item.languages.map(l => names[l] || l).join(', ');
  }
  /** Guardare: le slide della riga a destra. */
  show(item) {
    var _this = this;
    return (0,_home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      _this.viewed = item;
      _this.deckError = '';
      _this.deckOnlyIn = '';
      if (item.kind === 'empty' || item.kind === 'city') {
        _this.deckUrl = _this.trusted(`/api/templateshowcase/${item.kind === 'city' ? 'agent-city' : 'empty'}?language=${_this.language}`);
        return;
      }
      _this.deckUrl = null;
      _this.deckLoading = true;
      try {
        const entry = yield _this.describe(item.url);
        if (_this.viewed !== item) return; // nel frattempo si guarda un'altra riga
        if (!entry.speaks) _this.deckOnlyIn = _this.translate.instant('PROJECT_CONFIG.TEMPLATE_ONLY_IN', {
          languages: _this.languageNames(item)
        });
        _this.deckUrl = _this.trusted(entry.page);
      } catch (err) {
        if (_this.viewed === item) _this.deckError = err?.error?.error || err?.message || String(err);
      } finally {
        if (_this.viewed === item) _this.deckLoading = false;
      }
    })();
  }
  /** La lingua cambia: le slide si rifanno, un metodo che non la parla perde la spunta, la teoria si rilegge. */
  onLanguageChange() {
    for (const m of this.methods) if (this.chosen[m.url] && !this.speaks(m)) this.chosen[m.url] = false;
    const base = this.bases.find(b => b.url === this.mode);
    if (base && !this.speaks(base)) this.mode = TemplateShowcaseDialogComponent.Compose;
    this.theory = {};
    this.describeComposable();
    this.show(this.viewed);
  }
  /** Un template fuori elenco, per indirizzo: entra nella colonna come gli altri, già scelto. */
  addOther() {
    var _this2 = this;
    return (0,_home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      const url = _this2.otherUrl.trim();
      _this2.otherError = '';
      if (!url) return;
      if ([..._this2.composable, ..._this2.bases].some(i => i.url === url)) {
        _this2.otherError = _this2.translate.instant('TEMPLATE_SHOWCASE.ALREADY_LISTED');
        return;
      }
      _this2.otherLoading = true;
      try {
        const entry = yield _this2.describe(url);
        // Lo stesso template da un altro indirizzo: due righe uguali, e la creazione lo riceverebbe due volte.
        const twin = [..._this2.composable, ..._this2.bases].find(i => i.id === entry.id);
        if (twin) {
          _this2.otherError = _this2.translate.instant('TEMPLATE_SHOWCASE.SAME_TEMPLATE', {
            title: twin.title,
            url: twin.url
          });
          setTimeout(() => _this2.otherErrorText?.nativeElement.scrollIntoView({
            block: 'nearest'
          }));
          return;
        }
        const item = {
          url,
          id: entry.id,
          kind: entry.kind,
          title: entry.title,
          description: entry.description,
          languages: entry.languages,
          byAddress: true
        };
        if (item.kind === 'foundation') {
          // Lo strato comune è uno (MC2): per indirizzo entra solo se l'elenco non ne ha.
          if (_this2.foundation) {
            _this2.otherError = _this2.translate.instant('TEMPLATE_SHOWCASE.ONE_COMMON_LAYER', {
              title: _this2.foundation.title
            });
            setTimeout(() => _this2.otherErrorText?.nativeElement.scrollIntoView({
              block: 'nearest'
            }));
            return;
          }
          _this2.foundation = item;
          _this2.mode = TemplateShowcaseDialogComponent.Compose;
        } else if (item.kind === 'project') {
          _this2.bases = [..._this2.bases, item];
          if (_this2.speaks(item)) _this2.mode = url;
        } else {
          _this2.methods = [..._this2.methods, item];
          _this2.chosen[url] = _this2.speaks(item);
          if (_this2.speaks(item)) _this2.mode = TemplateShowcaseDialogComponent.Compose;
        }
        _this2.otherUrl = '';
        _this2.show(item);
      } catch (err) {
        _this2.otherError = err?.error?.error || err?.message || String(err);
        setTimeout(() => _this2.otherErrorText?.nativeElement.scrollIntoView({
          block: 'nearest'
        }));
      } finally {
        _this2.otherLoading = false;
      }
    })();
  }
  /** Componendo senza strato comune (l'elenco non ne ha) serve almeno un metodo. */
  get canGoOn() {
    return !this.composing || this.composable.some(i => this.isIn(i));
  }
  onNext() {
    if (!this.canGoOn) return;
    if (!this.composing) {
      const demo = this.bases.find(b => b.url === this.mode) || null;
      this.dialogRef.close({
        baseUrl: demo?.url || null,
        methodUrls: [],
        theoryUrls: [],
        language: this.language,
        titles: demo ? [demo.title] : [],
        asksEngine: !demo,
        agentCity: false,
        agentCityGuide: false
      });
      return;
    }
    const included = this.composable.filter(i => this.isIn(i));
    const first = included[0];
    this.dialogRef.close({
      // Il primo template è lo strato comune; senza, il primo metodo (finché l'elenco non ha lo strato comune).
      baseUrl: first.url,
      methodUrls: included.slice(1).map(i => i.url),
      theoryUrls: included.filter(i => this.theory[i.url] && this.hasTheory(i)).map(i => i.url),
      language: this.language,
      titles: [...included.map(i => i.title), ...(this.agentCity ? [this.city.title] : [])],
      // Né lo strato comune né un metodo portano il motore: lo sceglie chi crea.
      asksEngine: true,
      agentCity: this.agentCity,
      agentCityGuide: this.agentCity && this.agentCityGuide
    });
  }
  onCancel() {
    this.dialogRef.close(null);
  }
  describe(url) {
    var _this3 = this;
    return (0,_home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      const key = `${_this3.language}|${url}`;
      const known = _this3.described.get(key);
      if (known) return known;
      const entry = yield (0,rxjs__WEBPACK_IMPORTED_MODULE_4__.firstValueFrom)(_this3.http.get('../api/templateshowcase/describe', {
        params: {
          url,
          language: _this3.language
        }
      }));
      _this3.described.set(key, entry);
      return entry;
    })();
  }
  trusted(page) {
    return this.sanitizer.bypassSecurityTrustResourceUrl(`${page}${page.includes('?') ? '&' : '?'}theme=${this.theme}`);
  }
  /** Il testo di una voce dell'elenco nella lingua dell'interfaccia, o nella prima che ha. */
  text(texts) {
    return texts?.[this.translate.currentLang] || texts?.[Object.keys(texts || {})[0]] || '';
  }
  static {
    this.ɵfac = function TemplateShowcaseDialogComponent_Factory(t) {
      return new (t || TemplateShowcaseDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_6__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_7__.DomSanitizer), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_8__.TranslateService), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_theme_service__WEBPACK_IMPORTED_MODULE_1__.ThemeService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({
      type: TemplateShowcaseDialogComponent,
      selectors: [["app-template-showcase-dialog"]],
      viewQuery: function TemplateShowcaseDialogComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵviewQuery"](_c0, 5);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵloadQuery"]()) && (ctx.otherErrorText = _t.first);
        }
      },
      decls: 85,
      vars: 68,
      consts: [[1, "showcase"], ["mat-dialog-title", "", 1, "showcase-title"], [1, "showcase-body"], [1, "column"], [1, "column-scroll"], ["aria-label", "mode", 1, "rows", 3, "ngModel", "ngModelChange"], [1, "mode"], ["value", "compose"], [1, "compose"], [4, "ngIf"], [1, "group-label"], ["class", "row-desc", 4, "ngIf"], [1, "rows"], ["class", "row", 3, "viewed", "mute", "click", 4, "ngFor", "ngForOf"], [1, "row", 3, "click"], ["color", "primary", 3, "ngModel", "disabled", "ngModelChange", "click"], [1, "row-text"], [1, "row-title"], [1, "row-desc"], ["color", "primary", 1, "theory", 3, "ngModel", "disabled", "matTooltip", "ngModelChange", "click"], ["value", "", 3, "click"], ["theoryBox", ""], ["for", "showcase-other", 1, "group-label"], [1, "other"], ["appearance", "outline", 1, "other-field"], ["matInput", "", "id", "showcase-other", "placeholder", "https://github.com/\u2026/mdexplorer-template-\u2026.git", 3, "ngModel", "ngModelChange", "keyup.enter"], ["mat-icon-button", "", "color", "primary", 3, "disabled", "matTooltip", "click"], ["class", "row-desc error-text", 4, "ngIf"], [1, "language"], ["id", "showcase-language-label", 1, "group-label"], ["appearance", "outline", 1, "language-field"], ["aria-labelledby", "showcase-language-label", 3, "ngModel", "ngModelChange", "selectionChange"], [3, "value", 4, "ngFor", "ngForOf"], [1, "deck"], [1, "deck-head"], [1, "deck-title"], ["class", "deck-only-in", 4, "ngIf"], [1, "deck-frame"], ["diameter", "40", 4, "ngIf"], ["class", "deck-error", 4, "ngIf"], ["title", "slides", 3, "src", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["color", "primary", "disabled", "", 3, "ngModel", "matTooltip", "click"], ["class", "row-desc row-url", 4, "ngIf"], [4, "ngTemplateOutlet", "ngTemplateOutletContext"], [1, "row-desc", "row-url"], [3, "value", "disabled", "click"], ["class", "theory", "color", "primary", 3, "ngModel", "disabled", "matTooltip", "ngModelChange", "click", 4, "ngIf"], [1, "row-desc", "error-text"], ["otherErrorText", ""], [3, "value"], [1, "deck-only-in"], ["diameter", "40"], [1, "deck-error"], ["title", "slides", 3, "src"]],
      template: function TemplateShowcaseDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 0)(1, "h2", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](3, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "div", 2)(5, "div", 3)(6, "div", 4)(7, "mat-radio-group", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_Template_mat_radio_group_ngModelChange_7_listener($event) {
            return ctx.mode = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "div", 6)(9, "mat-radio-button", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](11, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "div", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](13, TemplateShowcaseDialogComponent_ng_container_13_Template, 13, 18, "ng-container", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](14, "label", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](15);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](16, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](17, TemplateShowcaseDialogComponent_p_17_Template, 2, 1, "p", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](18, TemplateShowcaseDialogComponent_p_18_Template, 3, 3, "p", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](19, "div", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](20, TemplateShowcaseDialogComponent_div_20_Template, 8, 13, "div", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](21, "label", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](22);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "div", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_div_click_24_listener() {
            return ctx.show(ctx.city);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](25, "mat-checkbox", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_Template_mat_checkbox_ngModelChange_25_listener($event) {
            return ctx.agentCity = $event;
          })("click", function TemplateShowcaseDialogComponent_Template_mat_checkbox_click_25_listener($event) {
            return $event.stopPropagation();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](26, "div", 16)(27, "div", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](28);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](29, "div", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](30);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](31, "mat-checkbox", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_Template_mat_checkbox_ngModelChange_31_listener($event) {
            return ctx.agentCityGuide = $event;
          })("click", function TemplateShowcaseDialogComponent_Template_mat_checkbox_click_31_listener($event) {
            return $event.stopPropagation();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](32, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](33);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](34, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](35, "label", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](36);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](37, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](38, TemplateShowcaseDialogComponent_div_38_Template, 10, 12, "div", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](39, "div", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_div_click_39_listener() {
            return ctx.show(ctx.empty);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](40, "mat-radio-button", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_mat_radio_button_click_40_listener($event) {
            return $event.stopPropagation();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](41, "div", 16)(42, "div", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](43);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](44, "div", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](45);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](46, TemplateShowcaseDialogComponent_ng_template_46_Template, 1, 1, "ng-template", null, 21, _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplateRefExtractor"]);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](48, "label", 22);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](49);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](50, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](51, "div", 23)(52, "mat-form-field", 24)(53, "input", 25);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_Template_input_ngModelChange_53_listener($event) {
            return ctx.otherUrl = $event;
          })("keyup.enter", function TemplateShowcaseDialogComponent_Template_input_keyup_enter_53_listener() {
            return ctx.addOther();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](54, "button", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_button_click_54_listener() {
            return ctx.addOther();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](55, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](56, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](57, "add");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](58, TemplateShowcaseDialogComponent_p_58_Template, 3, 1, "p", 27);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](59, "div", 28)(60, "label", 29);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](61);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](62, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](63, "mat-form-field", 30)(64, "mat-select", 31);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function TemplateShowcaseDialogComponent_Template_mat_select_ngModelChange_64_listener($event) {
            return ctx.language = $event;
          })("selectionChange", function TemplateShowcaseDialogComponent_Template_mat_select_selectionChange_64_listener() {
            return ctx.onLanguageChange();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](65, TemplateShowcaseDialogComponent_mat_option_65_Template, 2, 2, "mat-option", 32);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](66, "div", 33)(67, "div", 34)(68, "span", 35);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](69);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](70, TemplateShowcaseDialogComponent_span_70_Template, 2, 1, "span", 36);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](71, "div", 37);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](72, TemplateShowcaseDialogComponent_mat_spinner_72_Template, 1, 0, "mat-spinner", 38);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](73, TemplateShowcaseDialogComponent_div_73_Template, 8, 4, "div", 39);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](74, TemplateShowcaseDialogComponent_iframe_74_Template, 1, 1, "iframe", 40);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](75, "mat-dialog-actions", 41)(76, "button", 42);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_button_click_76_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](77);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](78, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](79, TemplateShowcaseDialogComponent_span_79_Template, 3, 3, "span", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](80, "button", 43);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function TemplateShowcaseDialogComponent_Template_button_click_80_listener() {
            return ctx.onNext();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](81);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](82, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](83, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](84, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]()()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](3, 44, "TEMPLATE_SHOWCASE.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.mode);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](11, 46, "TEMPLATE_SHOWCASE.COMPOSE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("off", !ctx.composing);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.foundation);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](16, 48, "TEMPLATE_SHOWCASE.METHODS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.catalogError);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.catalogLoaded && !ctx.catalogError && !ctx.methods.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.methods);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](23, 50, "TEMPLATE_SHOWCASE.FUNCTIONS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("viewed", ctx.viewed === ctx.city);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.agentCity)("disabled", !ctx.composing);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.city.title);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.city.description);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.agentCityGuide)("disabled", !ctx.composing || !ctx.agentCity)("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](32, 52, "TEMPLATE_SHOWCASE.AGENT_CITY_GUIDE_TIP"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](34, 54, "TEMPLATE_SHOWCASE.AGENT_CITY_GUIDE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](37, 56, "TEMPLATE_SHOWCASE.OR"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.bases);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵclassProp"]("viewed", ctx.viewed === ctx.empty);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.empty.title);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.empty.description);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](50, 58, "TEMPLATE_SHOWCASE.OTHER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.otherUrl);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.otherLoading || !ctx.otherUrl.trim())("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](55, 60, "TEMPLATE_SHOWCASE.OTHER_ADD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.otherError);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](62, 62, "TEMPLATE_SHOWCASE.LANGUAGE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.language);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.languages);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx.viewed.title);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.deckOnlyIn);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.deckLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.deckError);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.deckUrl && !ctx.deckLoading && !ctx.deckError);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](78, 64, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.canGoOn);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", !ctx.canGoOn);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](82, 66, "TEMPLATE_SHOWCASE.NEXT"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_9__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_9__.NgIf, _angular_common__WEBPACK_IMPORTED_MODULE_9__.NgTemplateOutlet, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_10__.MatLegacyOption, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_11__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_12__.MatLegacyFormField, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_13__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_14__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_14__.MatLegacyRadioButton, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_15__.MatLegacySelect, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_16__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_17__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_18__.MatLegacyProgressSpinner, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_19__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_20__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_8__.TranslatePipe],
      styles: [".showcase[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n}\n\n.showcase-title[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n\n.showcase-body[_ngcontent-%COMP%] {\n  flex: 1;\n  min-height: 0;\n  display: flex;\n  gap: 16px;\n}\n\n.column[_ngcontent-%COMP%] {\n  width: 300px;\n  flex: 0 0 300px;\n  display: flex;\n  flex-direction: column;\n  min-height: 0;\n  border-right: 1px solid var(--mde-border-color);\n  padding-right: 12px;\n}\n\n.column-scroll[_ngcontent-%COMP%] {\n  flex: 1;\n  min-height: 0;\n  overflow-y: auto;\n}\n\n.group-label[_ngcontent-%COMP%] {\n  display: block;\n  margin: 12px 0 4px;\n  font-weight: 600;\n  font-size: 12px;\n  text-transform: uppercase;\n  letter-spacing: 0.04em;\n  color: var(--mde-text-secondary);\n}\n\n.rows[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 8px;\n  padding: 8px;\n  border-radius: 6px;\n  cursor: pointer;\n}\n.row[_ngcontent-%COMP%]:hover {\n  background: var(--mde-bg-hover);\n}\n.row.viewed[_ngcontent-%COMP%] {\n  background: var(--mde-bg-selected);\n}\n.row.mute[_ngcontent-%COMP%]   .row-title[_ngcontent-%COMP%] {\n  color: var(--mde-disabled-text);\n}\n\n.row-text[_ngcontent-%COMP%] {\n  min-width: 0;\n}\n\n.mode[_ngcontent-%COMP%] {\n  padding: 8px 8px 0;\n  font-weight: 600;\n}\n\n.compose[_ngcontent-%COMP%] {\n  padding-left: 16px;\n}\n.compose.off[_ngcontent-%COMP%] {\n  opacity: 0.55;\n}\n\n.theory[_ngcontent-%COMP%] {\n  display: block;\n  margin-top: 2px;\n  font-size: 12px;\n}\n\n.row-title[_ngcontent-%COMP%] {\n  font-weight: 500;\n  color: var(--mde-text-primary);\n}\n\n.row-desc[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: var(--mde-text-secondary);\n  margin: 2px 0 0;\n}\n\n.row-url[_ngcontent-%COMP%] {\n  overflow-wrap: anywhere;\n  font-family: monospace;\n}\n\n.error-text[_ngcontent-%COMP%] {\n  color: var(--mde-error-fg);\n}\n\n.other[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 4px;\n}\n.other[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  margin-top: 12px;\n}\n\n.other-field[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.language[_ngcontent-%COMP%] {\n  border-top: 1px solid var(--mde-border-color);\n  padding-top: 4px;\n}\n\n.language-field[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\n.deck[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n}\n\n.deck-head[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: baseline;\n  gap: 12px;\n  padding: 4px 0 8px;\n}\n\n.deck-title[_ngcontent-%COMP%] {\n  font-weight: 600;\n  color: var(--mde-text-primary);\n}\n\n.deck-only-in[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: var(--mde-warning-fg);\n}\n\n.deck-frame[_ngcontent-%COMP%] {\n  flex: 1;\n  min-height: 0;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  border: 1px solid var(--mde-border-color);\n  border-radius: 6px;\n  overflow: hidden;\n  background: var(--mde-bg-secondary);\n}\n.deck-frame[_ngcontent-%COMP%]   iframe[_ngcontent-%COMP%] {\n  width: 100%;\n  height: 100%;\n  border: none;\n}\n\n.deck-error[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 24px;\n  color: var(--mde-text-secondary);\n}\n.deck-error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 40px;\n  width: 40px;\n  height: 40px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy90ZW1wbGF0ZS1zaG93Y2FzZS90ZW1wbGF0ZS1zaG93Y2FzZS1kaWFsb2cuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxZQUFBO0FBQUY7O0FBR0E7RUFDRSxrQkFBQTtBQUFGOztBQUdBO0VBQ0UsT0FBQTtFQUNBLGFBQUE7RUFDQSxhQUFBO0VBQ0EsU0FBQTtBQUFGOztBQUdBO0VBQ0UsWUFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxhQUFBO0VBQ0EsK0NBQUE7RUFDQSxtQkFBQTtBQUFGOztBQUdBO0VBQ0UsT0FBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtBQUFGOztBQUdBO0VBQ0UsY0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxlQUFBO0VBQ0EseUJBQUE7RUFDQSxzQkFBQTtFQUNBLGdDQUFBO0FBQUY7O0FBR0E7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtFQUNBLFFBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0FBQUY7QUFFRTtFQUNFLCtCQUFBO0FBQUo7QUFHRTtFQUNFLGtDQUFBO0FBREo7QUFJRTtFQUNFLCtCQUFBO0FBRko7O0FBTUE7RUFDRSxZQUFBO0FBSEY7O0FBTUE7RUFDRSxrQkFBQTtFQUNBLGdCQUFBO0FBSEY7O0FBT0E7RUFDRSxrQkFBQTtBQUpGO0FBTUU7RUFDRSxhQUFBO0FBSko7O0FBUUE7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7QUFMRjs7QUFRQTtFQUNFLGdCQUFBO0VBQ0EsOEJBQUE7QUFMRjs7QUFRQTtFQUNFLGVBQUE7RUFDQSxnQ0FBQTtFQUNBLGVBQUE7QUFMRjs7QUFRQTtFQUNFLHVCQUFBO0VBQ0Esc0JBQUE7QUFMRjs7QUFRQTtFQUNFLDBCQUFBO0FBTEY7O0FBUUE7RUFDRSxhQUFBO0VBQ0EsdUJBQUE7RUFDQSxRQUFBO0FBTEY7QUFPRTtFQUNFLGdCQUFBO0FBTEo7O0FBU0E7RUFDRSxPQUFBO0FBTkY7O0FBU0E7RUFDRSw2Q0FBQTtFQUNBLGdCQUFBO0FBTkY7O0FBU0E7RUFDRSxXQUFBO0FBTkY7O0FBU0E7RUFDRSxPQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtBQU5GOztBQVNBO0VBQ0UsYUFBQTtFQUNBLHFCQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0FBTkY7O0FBU0E7RUFDRSxnQkFBQTtFQUNBLDhCQUFBO0FBTkY7O0FBU0E7RUFDRSxlQUFBO0VBQ0EsNEJBQUE7QUFORjs7QUFTQTtFQUNFLE9BQUE7RUFDQSxhQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSx5Q0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxtQ0FBQTtBQU5GO0FBUUU7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLFlBQUE7QUFOSjs7QUFVQTtFQUNFLGtCQUFBO0VBQ0EsYUFBQTtFQUNBLGdDQUFBO0FBUEY7QUFTRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQVBKIiwic291cmNlc0NvbnRlbnQiOlsiLy8gTGEgdmV0cmluYSAoVkQ5KTogbGEgZmluZXN0cmEgb2NjdXBhIH45MCUgZGVsbG8gc2NoZXJtbywgbGEgY29sb25uYSBkZWkgdGVtcGxhdGUgfjMwMHB4LCBpbCByZXN0byBhbGxlIHNsaWRlLlxuLnNob3djYXNlIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgaGVpZ2h0OiAxMDAlO1xufVxuXG4uc2hvd2Nhc2UtdGl0bGUge1xuICBtYXJnaW4tYm90dG9tOiA4cHg7XG59XG5cbi5zaG93Y2FzZS1ib2R5IHtcbiAgZmxleDogMTtcbiAgbWluLWhlaWdodDogMDtcbiAgZGlzcGxheTogZmxleDtcbiAgZ2FwOiAxNnB4O1xufVxuXG4uY29sdW1uIHtcbiAgd2lkdGg6IDMwMHB4O1xuICBmbGV4OiAwIDAgMzAwcHg7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIG1pbi1oZWlnaHQ6IDA7XG4gIGJvcmRlci1yaWdodDogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICBwYWRkaW5nLXJpZ2h0OiAxMnB4O1xufVxuXG4uY29sdW1uLXNjcm9sbCB7XG4gIGZsZXg6IDE7XG4gIG1pbi1oZWlnaHQ6IDA7XG4gIG92ZXJmbG93LXk6IGF1dG87XG59XG5cbi5ncm91cC1sYWJlbCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBtYXJnaW46IDEycHggMCA0cHg7XG4gIGZvbnQtd2VpZ2h0OiA2MDA7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgbGV0dGVyLXNwYWNpbmc6IDAuMDRlbTtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG59XG5cbi5yb3dzIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbn1cblxuLnJvdyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogOHB4O1xuICBib3JkZXItcmFkaXVzOiA2cHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcblxuICAmOmhvdmVyIHtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctaG92ZXIpO1xuICB9XG5cbiAgJi52aWV3ZWQge1xuICAgIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1zZWxlY3RlZCk7XG4gIH1cblxuICAmLm11dGUgLnJvdy10aXRsZSB7XG4gICAgY29sb3I6IHZhcigtLW1kZS1kaXNhYmxlZC10ZXh0KTtcbiAgfVxufVxuXG4ucm93LXRleHQge1xuICBtaW4td2lkdGg6IDA7XG59XG5cbi5tb2RlIHtcbiAgcGFkZGluZzogOHB4IDhweCAwO1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4vLyBMZSByaWdoZSBkYSBjb21wb3JyZSwgcmllbnRyYXRlIHNvdHRvIGlsIGxvcm8gcGFsbGlubzsgc3BlbnRlIHF1YW5kbyBzaSBzY2VnbGllIGxhIGRlbW8gbyBsYSBjYXJ0ZWxsYSBjb3PDg8KsIGNvbSfDg8KoLlxuLmNvbXBvc2Uge1xuICBwYWRkaW5nLWxlZnQ6IDE2cHg7XG5cbiAgJi5vZmYge1xuICAgIG9wYWNpdHk6IDAuNTU7XG4gIH1cbn1cblxuLnRoZW9yeSB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBtYXJnaW4tdG9wOiAycHg7XG4gIGZvbnQtc2l6ZTogMTJweDtcbn1cblxuLnJvdy10aXRsZSB7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcbn1cblxuLnJvdy1kZXNjIHtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgbWFyZ2luOiAycHggMCAwO1xufVxuXG4ucm93LXVybCB7XG4gIG92ZXJmbG93LXdyYXA6IGFueXdoZXJlO1xuICBmb250LWZhbWlseTogbW9ub3NwYWNlO1xufVxuXG4uZXJyb3ItdGV4dCB7XG4gIGNvbG9yOiB2YXIoLS1tZGUtZXJyb3ItZmcpO1xufVxuXG4ub3RoZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcbiAgZ2FwOiA0cHg7XG5cbiAgYnV0dG9uIHtcbiAgICBtYXJnaW4tdG9wOiAxMnB4O1xuICB9XG59XG5cbi5vdGhlci1maWVsZCB7XG4gIGZsZXg6IDE7XG59XG5cbi5sYW5ndWFnZSB7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcbiAgcGFkZGluZy10b3A6IDRweDtcbn1cblxuLmxhbmd1YWdlLWZpZWxkIHtcbiAgd2lkdGg6IDEwMCU7XG59XG5cbi5kZWNrIHtcbiAgZmxleDogMTtcbiAgbWluLXdpZHRoOiAwO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xufVxuXG4uZGVjay1oZWFkIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGJhc2VsaW5lO1xuICBnYXA6IDEycHg7XG4gIHBhZGRpbmc6IDRweCAwIDhweDtcbn1cblxuLmRlY2stdGl0bGUge1xuICBmb250LXdlaWdodDogNjAwO1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtcHJpbWFyeSk7XG59XG5cbi5kZWNrLW9ubHktaW4ge1xuICBmb250LXNpemU6IDEycHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtd2FybmluZy1mZyk7XG59XG5cbi5kZWNrLWZyYW1lIHtcbiAgZmxleDogMTtcbiAgbWluLWhlaWdodDogMDtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICBib3JkZXItcmFkaXVzOiA2cHg7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1zZWNvbmRhcnkpO1xuXG4gIGlmcmFtZSB7XG4gICAgd2lkdGg6IDEwMCU7XG4gICAgaGVpZ2h0OiAxMDAlO1xuICAgIGJvcmRlcjogbm9uZTtcbiAgfVxufVxuXG4uZGVjay1lcnJvciB7XG4gIHRleHQtYWxpZ246IGNlbnRlcjtcbiAgcGFkZGluZzogMjRweDtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG5cbiAgbWF0LWljb24ge1xuICAgIGZvbnQtc2l6ZTogNDBweDtcbiAgICB3aWR0aDogNDBweDtcbiAgICBoZWlnaHQ6IDQwcHg7XG4gIH1cbn1cbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);












const _c0 = function (a0) {
  return {
    delay: a0
  };
};
const _c1 = function (a1) {
  return {
    value: "",
    params: a1
  };
};
function OpenRecentComponent_mat_card_3_Template(rf, ctx) {
  if (rf & 1) {
    const _r4 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "mat-card")(1, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](3, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](5, "mat-card-actions")(6, "button", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_6_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4);
      const element_r1 = restoredCtx.$implicit;
      const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r3.openNewProject(element_r1.path));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](7, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](8, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_8_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4);
      const element_r1 = restoredCtx.$implicit;
      const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r5.deleteProject(element_r1));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](10, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](11, "button", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_3_Template_button_click_11_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵrestoreView"](_r4);
      const element_r1 = restoredCtx.$implicit;
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵresetView"](ctx_r6.openProjectSettings(element_r1));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](13, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const element_r1 = ctx.$implicit;
    const i_r2 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("@bounceInLeftOnEnter", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpureFunction1"](5, _c1, _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpureFunction1"](3, _c0, i_r2 * 100)));
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](element_r1.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtextInterpolate"](element_r1.path);
  }
}
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
  static {
    this.ɵfac = function OpenRecentComponent_Factory(t) {
      return new (t || OpenRecentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__.ActivatedRoute), _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_5__.MatLegacyDialog));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineComponent"]({
      type: OpenRecentComponent,
      selectors: [["app-open-recent"]],
      decls: 5,
      vars: 3,
      consts: [[1, "projects-list-container"], [4, "ngFor", "ngForOf"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-raised-button", "", "color", "warn", 3, "click"], ["mat-icon-button", "", "matTooltip", "Project Settings", 3, "click"]],
      template: function OpenRecentComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](0, "h1");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtext"](1, "Recent Projects");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementStart"](2, "div", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵtemplate"](3, OpenRecentComponent_mat_card_3_Template, 14, 7, "mat-card", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipe"](4, "async");
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵpipeBind1"](4, 1, ctx.dataSource));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.NgForOf, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_7__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_7__.MatLegacyCardTitle, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_7__.MatLegacyCardSubtitle, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_7__.MatLegacyCardActions, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_8__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_9__.MatIcon, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_10__.MatLegacyTooltip, _angular_common__WEBPACK_IMPORTED_MODULE_6__.AsyncPipe],
      styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  overflow: hidden;\n}\n\nh1[_ngcontent-%COMP%] {\n  margin: 0 0 20px 0;\n  font-weight: 400;\n  font-size: 1.8rem;\n  flex-shrink: 0;\n}\n\n.projects-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 0 8px 0 0;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n  scrollbar-width: thin;\n  scrollbar-color: var(--mde-border-strong) transparent;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: transparent;\n  border-radius: 4px;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: var(--mde-border-strong);\n  border-radius: 4px;\n  -webkit-transition: background 0.3s ease;\n  transition: background 0.3s ease;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: var(--mde-text-hint);\n}\n\nmat-card[_ngcontent-%COMP%] {\n  transition: transform 0.2s ease, box-shadow 0.2s ease;\n  cursor: pointer;\n}\nmat-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 4px 12px var(--mde-shadow-color);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvb3Blbi1yZWNlbnQvb3Blbi1yZWNlbnQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtFQXVCQSxxQkFBQTtFQUNBLHFEQUFBO0FBckJGO0FBQUU7RUFDRSxVQUFBO0FBRUo7QUFDRTtFQUNFLHVCQUFBO0VBQ0Esa0JBQUE7QUFDSjtBQUVFO0VBQ0Usb0NBQUE7RUFDQSxrQkFBQTtFQUNBLHdDQUFBO0VBQUEsZ0NBQUE7QUFBSjtBQUVJO0VBQ0UsZ0NBQUE7QUFBTjs7QUFVQTtFQUNFLHFEQUFBO0VBQ0EsZUFBQTtBQVBGO0FBU0U7RUFDRSwyQkFBQTtFQUNBLDhDQUFBO0FBUEoiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGhlaWdodDogMTAwJTtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcbn1cblxuaDEge1xuICBtYXJnaW46IDAgMCAyMHB4IDA7XG4gIGZvbnQtd2VpZ2h0OiA0MDA7XG4gIGZvbnQtc2l6ZTogMS44cmVtO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLnByb2plY3RzLWxpc3QtY29udGFpbmVyIHtcbiAgZmxleDogMTtcbiAgb3ZlcmZsb3cteTogYXV0bztcbiAgcGFkZGluZzogMCA4cHggMCAwO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDE2cHg7XG5cbiAgLy8gQ3VzdG9tIHNjcm9sbGJhciBzdHlsaW5nXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyIHtcbiAgICB3aWR0aDogOHB4O1xuICB9XG5cbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdHJhY2sge1xuICAgIGJhY2tncm91bmQ6IHRyYW5zcGFyZW50O1xuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgfVxuXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRodW1iIHtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYm9yZGVyLXN0cm9uZyk7XG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xuICAgIHRyYW5zaXRpb246IGJhY2tncm91bmQgMC4zcyBlYXNlO1xuXG4gICAgJjpob3ZlciB7XG4gICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtdGV4dC1oaW50KTtcbiAgICB9XG4gIH1cblxuICAvLyBGaXJlZm94IHNjcm9sbGJhclxuICBzY3JvbGxiYXItd2lkdGg6IHRoaW47XG4gIHNjcm9sbGJhci1jb2xvcjogdmFyKC0tbWRlLWJvcmRlci1zdHJvbmcpIHRyYW5zcGFyZW50O1xufVxuXG4vLyBTbW9vdGggY2FyZCBob3ZlciBlZmZlY3Rcbm1hdC1jYXJkIHtcbiAgdHJhbnNpdGlvbjogdHJhbnNmb3JtIDAuMnMgZWFzZSwgYm94LXNoYWRvdyAwLjJzIGVhc2U7XG4gIGN1cnNvcjogcG9pbnRlcjtcblxuICAmOmhvdmVyIHtcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XG4gICAgYm94LXNoYWRvdzogMCA0cHggMTJweCB2YXIoLS1tZGUtc2hhZG93LWNvbG9yKTtcbiAgfVxufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"],
      data: {
        animation: [(0,angular_animations__WEBPACK_IMPORTED_MODULE_11__.bounceInLeftOnEnterAnimation)({
          translate: '500px'
        })]
      }
    });
  }
}

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
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _commons_components_confirm_dialog_confirm_dialog_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../commons/components/confirm-dialog/confirm-dialog.component */ 8950);
/* harmony import */ var _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../dialogs/catalog-picker/catalog-picker.component */ 5922);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../services/project-settings.service */ 5450);
/* harmony import */ var _services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/compatibility-mode.service */ 7929);
/* harmony import */ var _services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../services/ide-configuration.service */ 9909);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _md_explorer_services_external_apps_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../md-explorer/services/external-apps.service */ 9595);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _md_explorer_services_agent_city_state_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../md-explorer/services/agent-city-state.service */ 278);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-core */ 7090);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-select */ 6002);
/* harmony import */ var _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/legacy-menu */ 1051);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/material/legacy-tabs */ 2821);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/material/legacy-progress-bar */ 5042);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_28__ = __webpack_require__(/*! @angular/forms */ 2508);






























function ProjectSettingsComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    const _r56 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_14_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r56);
      const ctx_r55 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r55.rule1Enabled = $event);
    })("change", function ProjectSettingsComponent_div_14_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r56);
      const ctx_r57 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r57.onRule1Change());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r0.rule1Enabled)("disabled", ctx_r0.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.RULE1_LABEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.RULE1_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 9, "PROJECT_SETTINGS.RULE1_NOTE"));
  }
}
function ProjectSettingsComponent_div_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_23_Template(rf, ctx) {
  if (rf & 1) {
    const _r59 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_23_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r59);
      const ctx_r58 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r58.githubModeEnabled = $event);
    })("change", function ProjectSettingsComponent_div_23_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r59);
      const ctx_r60 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r60.onGitHubModeChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r3.githubModeEnabled)("disabled", ctx_r3.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.GITHUB_MODE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.GITHUB_MODE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_30_Template(rf, ctx) {
  if (rf & 1) {
    const _r62 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_30_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r62);
      const ctx_r61 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r61.linkIndexingEnabled = $event);
    })("change", function ProjectSettingsComponent_div_30_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r62);
      const ctx_r63 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r63.onLinkIndexingChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r4.linkIndexingEnabled)("disabled", ctx_r4.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.ENABLE_LINK_INDEXING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.LINK_INDEXING_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 9, "PROJECT_SETTINGS.LINK_INDEXING_NOTE"));
  }
}
function ProjectSettingsComponent_div_31_mat_icon_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_31_mat_spinner_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_div_31_Template(rf, ctx) {
  if (rf & 1) {
    const _r67 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_31_Template_button_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r67);
      const ctx_r66 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r66.onProjectReindex());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_31_mat_icon_2_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](3, ProjectSettingsComponent_div_31_mat_spinner_3_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r5.projectReindexing || ctx_r5.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r5.projectReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r5.projectReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r5.projectReindexing ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.REINDEXING_PROJECT") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 7, "PROJECT_SETTINGS.REINDEX_PROJECT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 9, "PROJECT_SETTINGS.REINDEX_PROJECT_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_38_Template(rf, ctx) {
  if (rf & 1) {
    const _r69 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_38_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r69);
      const ctx_r68 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r68.indexAllTextFilesEnabled = $event);
    })("change", function ProjectSettingsComponent_div_38_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r69);
      const ctx_r70 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r70.onTextIndexingChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r6.indexAllTextFilesEnabled)("disabled", ctx_r6.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.ENABLE_TEXT_INDEXING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.TEXT_INDEXING_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_39_Template(rf, ctx) {
  if (rf & 1) {
    const _r72 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-form-field", 57)(2, "mat-label");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "textarea", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_39_Template_textarea_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r72);
      const ctx_r71 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r71.textFileExtensions = $event);
    })("blur", function ProjectSettingsComponent_div_39_Template_textarea_blur_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r72);
      const ctx_r73 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r73.onTextIndexingChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.TEXT_EXTENSIONS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r7.textFileExtensions)("placeholder", ctx_r7.textFileExtensionsDefault)("disabled", ctx_r7.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](8, 7, "PROJECT_SETTINGS.TEXT_EXTENSIONS_HINT"));
  }
}
function ProjectSettingsComponent_div_40_mat_icon_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_40_mat_spinner_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_div_40_Template(rf, ctx) {
  if (rf & 1) {
    const _r77 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_40_Template_button_click_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r77);
      const ctx_r76 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r76.onReindexTextFiles());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_40_mat_icon_2_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](3, ProjectSettingsComponent_div_40_mat_spinner_3_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r8.reindexingText || ctx_r8.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r8.reindexingText);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r8.reindexingText);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r8.reindexingText ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.REINDEXING_TEXT") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 7, "PROJECT_SETTINGS.REINDEX_TEXT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 9, "PROJECT_SETTINGS.REINDEX_TEXT_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_47_Template(rf, ctx) {
  if (rf & 1) {
    const _r79 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_47_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r79);
      const ctx_r78 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r78.stickyScrollEnabled = $event);
    })("change", function ProjectSettingsComponent_div_47_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r79);
      const ctx_r80 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r80.onStickyScrollChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r9.stickyScrollEnabled)("disabled", ctx_r9.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.STICKY_SCROLL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.STICKY_SCROLL_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_48_Template(rf, ctx) {
  if (rf & 1) {
    const _r82 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_48_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r82);
      const ctx_r81 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r81.plantUmlKeepOriginalColorsEnabled = $event);
    })("change", function ProjectSettingsComponent_div_48_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r82);
      const ctx_r83 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r83.onPlantUmlKeepOriginalColorsChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r10.plantUmlKeepOriginalColorsEnabled)("disabled", ctx_r10.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.PLANTUML_KEEP_ORIGINAL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.PLANTUML_KEEP_ORIGINAL_DESC"), " ");
  }
}
function ProjectSettingsComponent_mat_spinner_60_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_button_64_Template(rf, ctx) {
  if (rf & 1) {
    const _r85 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_button_64_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r85);
      const ctx_r84 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r84.enableAgentCity(true));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "menu_book");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r12.agentCityBusy)("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](1, 3, "PROJECT_SETTINGS.AGENT_CITY_GUIDE_ADD_TIP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.AGENT_CITY_GUIDE_ADD"), " ");
  }
}
function ProjectSettingsComponent_ul_65_li_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const n_r87 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](n_r87);
  }
}
function ProjectSettingsComponent_ul_65_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "ul", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_ul_65_li_1_Template, 2, 1, "li", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r13.agentCityNotes);
  }
}
function ProjectSettingsComponent_div_66_div_12_Template(rf, ctx) {
  if (rf & 1) {
    const _r94 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 65)(1, "mat-form-field", 66)(2, "input", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_66_div_12_Template_input_ngModelChange_2_listener($event) {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r94);
      const i_r92 = restoredCtx.index;
      const ctx_r93 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r93.agentCityWorkflowDocs[i_r92] = $event);
    })("blur", function ProjectSettingsComponent_div_66_div_12_Template_input_blur_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r94);
      const ctx_r95 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r95.onAgentCityChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "button", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_66_div_12_Template_button_click_3_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r94);
      const i_r92 = restoredCtx.index;
      const ctx_r96 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r96.removeWorkflowDoc(i_r92));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const doc_r91 = ctx.$implicit;
    const ctx_r88 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", doc_r91)("disabled", ctx_r88.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r88.saving)("title", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 7, "PROJECT_SETTINGS.AGENT_CITY_WORKFLOW_REMOVE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵattribute"]("aria-label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.AGENT_CITY_WORKFLOW_REMOVE"));
  }
}
function ProjectSettingsComponent_div_66_p_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r89 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r89.agentCityError);
  }
}
function ProjectSettingsComponent_div_66_p_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" \uD83D\uDD11 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.AGENT_CITY_ROOM_SECRET_SET"), " ");
  }
}
function ProjectSettingsComponent_div_66_Template(rf, ctx) {
  if (rf & 1) {
    const _r98 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "label", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "mat-form-field", 16)(5, "input", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_66_Template_input_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r98);
      const ctx_r97 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r97.agentCityOwnershipDoc = $event);
    })("blur", function ProjectSettingsComponent_div_66_Template_input_blur_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r98);
      const ctx_r99 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r99.onAgentCityChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "label", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](12, ProjectSettingsComponent_div_66_div_12_Template, 8, 9, "div", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](13, "button", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_66_Template_button_click_13_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r98);
      const ctx_r100 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r100.addWorkflowDoc());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](14, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](15, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](17, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](18, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](20, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](21, ProjectSettingsComponent_div_66_p_21_Template, 2, 1, "p", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](22, ProjectSettingsComponent_div_66_p_22_Template, 3, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 12, "PROJECT_SETTINGS.AGENT_CITY_OWNERSHIP_DOC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r14.agentCityOwnershipDoc)("disabled", ctx_r14.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](8, 14, "PROJECT_SETTINGS.AGENT_CITY_OWNERSHIP_DOC_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 16, "PROJECT_SETTINGS.AGENT_CITY_WORKFLOW_DOCS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r14.agentCityWorkflowDocs)("ngForTrackBy", ctx_r14.trackByIndex);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r14.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](17, 18, "PROJECT_SETTINGS.AGENT_CITY_WORKFLOW_ADD"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](20, 20, "PROJECT_SETTINGS.AGENT_CITY_WORKFLOW_DOCS_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r14.agentCityError);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r14.agentCityHasRoomSecret);
  }
}
function ProjectSettingsComponent_div_67_p_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" \u2139\uFE0F ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.AGENT_NEEDS_GIT"), " ");
  }
}
const _c0 = function (a0, a1) {
  return {
    def: a0,
    max: a1
  };
};
function ProjectSettingsComponent_div_67_div_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r104 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 71)(1, "label", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "mat-form-field", 73)(5, "input", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_67_div_9_Template_input_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r104);
      const ctx_r103 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r103.agentWorktreeSlots = $event);
    })("change", function ProjectSettingsComponent_div_67_div_9_Template_input_change_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r104);
      const ctx_r105 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r105.onAgentWorktreeSlotsChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r102 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 5, "PROJECT_SETTINGS.AGENT_WORKTREE_SLOTS"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r102.agentWorktreeSlots)("disabled", ctx_r102.saving)("max", ctx_r102.agentWorktreeMaxSlots);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](8, 7, "PROJECT_SETTINGS.AGENT_WORKTREE_SLOTS_DESC", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction2"](10, _c0, ctx_r102.agentWorktreeDefaultSlots, ctx_r102.agentWorktreeMaxSlots)), " ");
  }
}
function ProjectSettingsComponent_div_67_Template(rf, ctx) {
  if (rf & 1) {
    const _r107 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_67_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r107);
      const ctx_r106 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r106.agentUseWorktrees = $event);
    })("change", function ProjectSettingsComponent_div_67_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r107);
      const ctx_r108 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r108.onAgentWorktreesChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](8, ProjectSettingsComponent_div_67_p_8_Template, 3, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](9, ProjectSettingsComponent_div_67_div_9_Template, 9, 13, "div", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r15.agentUseWorktrees)("disabled", ctx_r15.saving || !ctx_r15.projectIsGit);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 6, "PROJECT_SETTINGS.AGENT_WORKTREES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 8, "PROJECT_SETTINGS.AGENT_WORKTREES_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r15.projectIsGit);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r15.agentUseWorktrees && ctx_r15.projectIsGit);
  }
}
function ProjectSettingsComponent_div_68_button_29_Template(rf, ctx) {
  if (rf & 1) {
    const _r115 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_68_button_29_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r115);
      const ctx_r114 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r114.clearRelayApiKey());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r109 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r109.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 2, "PROJECT_SETTINGS.RELAY_CLEAR_KEY"), " ");
  }
}
function ProjectSettingsComponent_div_68_p_30_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.RELAY_TESTING"), " ");
  }
}
function ProjectSettingsComponent_div_68_p_31_span_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "\u2705");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_68_p_31_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "\u26A0\uFE0F");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_68_p_31_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_div_68_p_31_span_1_Template, 2, 0, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_68_p_31_span_2_Template, 2, 0, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r111 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r111.relayTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r111.relayTestSuccess === false);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r111.relayTestMessage, " ");
  }
}
function ProjectSettingsComponent_div_68_p_32_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.RELAY_KEY_FROM_GLOBAL"), " ");
  }
}
function ProjectSettingsComponent_div_68_p_33_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.RELAY_KEY_MISSING"), " ");
  }
}
function ProjectSettingsComponent_div_68_Template(rf, ctx) {
  if (rf & 1) {
    const _r119 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "label", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "mat-form-field", 16)(5, "input", 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_68_Template_input_ngModelChange_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r119);
      const ctx_r118 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r118.relayUrl = $event);
    })("blur", function ProjectSettingsComponent_div_68_Template_input_blur_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r119);
      const ctx_r120 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r120.onRelaySettingsChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "label", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-form-field", 16)(13, "input", 76);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_68_Template_input_ngModelChange_13_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r119);
      const ctx_r121 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r121.relayApiKey = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](15, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](17, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](18, "div", 77)(19, "button", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_68_Template_button_click_19_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r119);
      const ctx_r122 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r122.onRelaySettingsChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](20, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](21, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](22);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](23, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](24, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_68_Template_button_click_24_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r119);
      const ctx_r123 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r123.testRelay());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](25, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](26, "wifi_tethering");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](27);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](28, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](29, ProjectSettingsComponent_div_68_button_29_Template, 5, 4, "button", 79);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](30, ProjectSettingsComponent_div_68_p_30_Template, 3, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](31, ProjectSettingsComponent_div_68_p_31_Template, 4, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](32, ProjectSettingsComponent_div_68_p_32_Template, 3, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](33, ProjectSettingsComponent_div_68_p_33_Template, 3, 3, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 18, "PROJECT_SETTINGS.RELAY_URL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r16.relayUrl)("disabled", ctx_r16.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](8, 20, "PROJECT_SETTINGS.RELAY_URL_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 22, "PROJECT_SETTINGS.RELAY_API_KEY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r16.relayApiKey)("disabled", ctx_r16.saving)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](14, 24, ctx_r16.relayHasApiKey ? "PROJECT_SETTINGS.RELAY_API_KEY_SET" : "PROJECT_SETTINGS.RELAY_API_KEY_EMPTY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](17, 26, "PROJECT_SETTINGS.RELAY_API_KEY_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r16.saving || !(ctx_r16.relayApiKey == null ? null : ctx_r16.relayApiKey.trim()));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](23, 28, "PROJECT_SETTINGS.RELAY_SAVE_KEY"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r16.saving || ctx_r16.relayTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](28, 30, "PROJECT_SETTINGS.RELAY_TEST"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r16.relayHasApiKey && ctx_r16.relayApiKeySource === "Project");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r16.relayTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r16.relayTestMessage);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r16.relayApiKeySource === "Global");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r16.relayApiKeySource === "None");
  }
}
function ProjectSettingsComponent_p_76_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.TEMPLATES_NONE"));
  }
}
function ProjectSettingsComponent_ul_77_li_1_span_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" \u00B7 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.TEMPLATE_EXAMPLE"), "");
  }
}
function ProjectSettingsComponent_ul_77_li_1_span_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" \u00B7 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.TEMPLATE_THEORY"), "");
  }
}
const _c1 = function (a0) {
  return {
    date: a0
  };
};
function ProjectSettingsComponent_ul_77_li_1_span_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const t_r125 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" \u00B7 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](2, 1, "PROJECT_SETTINGS.TEMPLATE_DETACHED", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction1"](4, _c1, t_r125.detached)), "");
  }
}
function ProjectSettingsComponent_ul_77_li_1_button_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r134 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 85);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_ul_77_li_1_button_10_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r134);
      const t_r125 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
      const ctx_r132 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r132.addTheory(t_r125));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "school");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r129 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r129.addingMethod)("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](1, 3, "PROJECT_SETTINGS.TEMPLATE_THEORY_ADD_TIP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.TEMPLATE_THEORY_ADD"), " ");
  }
}
function ProjectSettingsComponent_ul_77_li_1_button_11_Template(rf, ctx) {
  if (rf & 1) {
    const _r137 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 86);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_ul_77_li_1_button_11_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r137);
      const t_r125 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
      const ctx_r135 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r135.detachTemplate(t_r125));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "link_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r130 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r130.addingMethod);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 2, "PROJECT_SETTINGS.TEMPLATE_DETACH"), " ");
  }
}
function ProjectSettingsComponent_ul_77_li_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "li")(1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](5, ProjectSettingsComponent_ul_77_li_1_span_5_Template, 3, 3, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](6, ProjectSettingsComponent_ul_77_li_1_span_6_Template, 3, 3, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](7, ProjectSettingsComponent_ul_77_li_1_span_7_Template, 3, 6, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "span", 82);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](10, ProjectSettingsComponent_ul_77_li_1_button_10_Template, 6, 7, "button", 83);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](11, ProjectSettingsComponent_ul_77_li_1_button_11_Template, 5, 4, "button", 84);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const t_r125 = ctx.$implicit;
    const ctx_r124 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](t_r125.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate3"](" ", t_r125.version, " \u00B7 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 10, ctx_r124.templateKindKey(t_r125)), " \u00B7 ", t_r125.language, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", t_r125.example);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", t_r125.theory);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", t_r125.detached);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](t_r125.url);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", t_r125.theoryAvailable && !t_r125.theory && !t_r125.detached);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !t_r125.detached);
  }
}
function ProjectSettingsComponent_ul_77_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "ul", 81);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_ul_77_li_1_Template, 12, 12, "li", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r18.projectTemplates);
  }
}
function ProjectSettingsComponent_mat_spinner_101_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_p_105_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r20.methodError);
  }
}
function ProjectSettingsComponent_ul_106_li_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const n_r139 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](n_r139);
  }
}
function ProjectSettingsComponent_ul_106_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "ul", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_ul_106_li_1_Template, 2, 1, "li", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r21.methodNotes);
  }
}
const _c2 = function (a0) {
  return {
    engine: a0
  };
};
function ProjectSettingsComponent_div_116_span_23_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const ctx_r141 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](2, 1, "PROJECT_SETTINGS.AGENT_CLI_HINT", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction1"](6, _c2, _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 4, ctx_r141.engineLabelKey(ctx_r141.markAgentEngine)))), " ");
  }
}
function ProjectSettingsComponent_div_116_span_23_ng_container_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.AGENT_CLI_NONE"), " ");
  }
}
function ProjectSettingsComponent_div_116_span_23_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_div_116_span_23_ng_container_1_Template, 4, 8, "ng-container", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_116_span_23_ng_container_2_Template, 3, 3, "ng-container", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r140 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r140.markAgentEngine !== "none");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r140.markAgentEngine === "none");
  }
}
function ProjectSettingsComponent_div_116_Template(rf, ctx) {
  if (rf & 1) {
    const _r144 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "mat-radio-group", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_116_Template_mat_radio_group_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r144);
      const ctx_r143 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r143.selectedIde = $event);
    })("change", function ProjectSettingsComponent_div_116_Template_mat_radio_group_change_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r144);
      const ctx_r145 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r145.onIdeChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-radio-button", 87)(6, "span", 28)(7, "mat-icon", 88);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](11, "mat-radio-button", 89)(12, "span", 28)(13, "mat-icon", 90);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](14, "terminal");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](17, "mat-radio-button", 91)(18, "span", 28)(19, "mat-icon", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](20, "smart_toy");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](21);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](22, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](23, ProjectSettingsComponent_div_116_span_23_Template, 3, 2, "span", 92);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 8, "PROJECT_SETTINGS.IDE_SELECT_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r22.selectedIde)("disabled", ctx_r22.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](10, 10, "PROJECT_SETTINGS.VSCODE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](16, 12, "PROJECT_SETTINGS.INTELLIJ"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", !ctx_r22.engineSettingsLoaded || ctx_r22.markAgentEngine === "none");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](22, 14, "PROJECT_SETTINGS.AGENT_CLI"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r22.engineSettingsLoaded);
  }
}
function ProjectSettingsComponent_div_117_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_118_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_139_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 12)(1, "mat-icon", 93);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.HARNESS_NOT_DECLARED"), " ");
  }
}
const _c3 = function (a0, a1) {
  return {
    engine: a0,
    harness: a1
  };
};
function ProjectSettingsComponent_div_140_Template(rf, ctx) {
  if (rf & 1) {
    const _r147 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 94)(1, "mat-icon", 95);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span", 96);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "button", 97);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_140_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r147);
      const ctx_r146 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r146.relinkEngine());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](5, 3, "PROJECT_SETTINGS.ENGINE_DECOUPLED", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction2"](12, _c3, _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 6, ctx_r26.engineLabelKey(ctx_r26.markAgentEngine)), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 8, ctx_r26.harnessLabelKey(ctx_r26.harness)))), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r26.savingEngine);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](10, 10, "PROJECT_SETTINGS.ENGINE_RELINK"), " ");
  }
}
function ProjectSettingsComponent_ng_container_181_Template(rf, ctx) {
  if (rf & 1) {
    const _r149 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "label", 98);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "div", 99)(5, "mat-form-field", 100)(6, "input", 101);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_ng_container_181_Template_input_ngModelChange_6_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r149);
      const ctx_r148 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r148.harnessModel = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "button", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_ng_container_181_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r149);
      const ctx_r150 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r150.saveHarnessModel());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](13, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 7, "PROJECT_SETTINGS.HARNESS_MODEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r27.harnessModel)("disabled", ctx_r27.savingHarnessModel || ctx_r27.savingHarness)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 9, "PROJECT_SETTINGS.HARNESS_MODEL_EMPTY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", !ctx_r27.harnessModelChanged || ctx_r27.savingHarnessModel || ctx_r27.savingHarness);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 11, "PROJECT_SETTINGS.HARNESS_MODEL_SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](15, 13, "PROJECT_SETTINGS.HARNESS_MODEL_DESC"));
  }
}
function ProjectSettingsComponent_div_217_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_228_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 12)(1, "mat-icon", 93);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.MCP_GROUPS_PROPOSED"), " ");
  }
}
function ProjectSettingsComponent_div_229_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 94)(1, "mat-icon", 95);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span", 96);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r30.mcpGroupsError);
  }
}
const _c4 = function (a0, a1) {
  return {
    tools: a0,
    tokens: a1
  };
};
function ProjectSettingsComponent_div_230_Template(rf, ctx) {
  if (rf & 1) {
    const _r153 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 102)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_230_Template_mat_checkbox_ngModelChange_1_listener($event) {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r153);
      const group_r151 = restoredCtx.$implicit;
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](group_r151.enabled = $event);
    })("change", function ProjectSettingsComponent_div_230_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r153);
      const ctx_r154 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r154.onMcpGroupChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "uppercase");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "span", 103);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "p", 104);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const group_r151 = ctx.$implicit;
    const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", group_r151.enabled)("disabled", group_r151.mandatory || ctx_r31.savingMcpGroups || !ctx_r31.mcpGroupsLoaded);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.MCP_GROUP_" + _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 7, group_r151.id)));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](8, 9, "PROJECT_SETTINGS.MCP_GROUP_WEIGHT", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction2"](12, _c4, group_r151.toolCount, group_r151.approxTokens)), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](group_r151.summary);
  }
}
const _c5 = function (a0, a1) {
  return {
    selected: a0,
    all: a1
  };
};
function ProjectSettingsComponent_p_231_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 105);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](3, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](2, 2, "PROJECT_SETTINGS.MCP_GROUPS_TOTAL", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction2"](7, _c5, ctx_r32.mcpTokensSelected, ctx_r32.mcpTokensAll)), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.MCP_GROUPS_NEXT_SESSION"), " ");
  }
}
function ProjectSettingsComponent_div_232_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_239_Template(rf, ctx) {
  if (rf & 1) {
    const _r156 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_239_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r156);
      const ctx_r155 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r155.ragEnabled = $event);
    })("change", function ProjectSettingsComponent_div_239_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r156);
      const ctx_r157 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r157.onRagChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r34.ragEnabled)("disabled", ctx_r34.saving || !ctx_r34.ragModelInstalled);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.ENABLE_RAG"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.RAG_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 9, "PROJECT_SETTINGS.RAG_INCREMENTAL"), " ");
  }
}
function ProjectSettingsComponent_div_240_div_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 107)(1, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r158 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.INDEXED_CHUNKS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r158.ragChunksCount);
  }
}
function ProjectSettingsComponent_div_240_div_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 107)(1, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r159 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.WITH_EMBEDDINGS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r159.ragEmbeddedCount);
  }
}
function ProjectSettingsComponent_div_240_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 106)(1, "div", 107)(2, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](9, ProjectSettingsComponent_div_240_div_9_Template, 6, 4, "div", 109);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](10, ProjectSettingsComponent_div_240_div_10_Template, 6, 4, "div", 109);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 8, "PROJECT_SETTINGS.EMBEDDING_MODEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵclassProp"]("status-ok", ctx_r35.ragModelInstalled)("status-missing", !ctx_r35.ragModelInstalled);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r35.ragModelInstalled ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 10, "PROJECT_SETTINGS.INSTALLED") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](8, 12, "PROJECT_SETTINGS.NOT_INSTALLED"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r35.ragEnabled);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r35.ragEnabled);
  }
}
function ProjectSettingsComponent_div_241_mat_icon_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_241_mat_spinner_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_div_241_p_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 12)(1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](4, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.FORCE_REINDEX_DESC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 4, "PROJECT_SETTINGS.CLEAR_INDEX_DESC"));
  }
}
const _c6 = function (a0, a1, a2) {
  return {
    processed: a0,
    total: a1,
    percent: a2
  };
};
function ProjectSettingsComponent_div_241_div_14_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 116);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "number");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r165 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](2, 1, "PROJECT_SETTINGS.PROCESSING_FILES", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction3"](7, _c6, ctx_r165.ragProcessed, ctx_r165.ragTotal, _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](3, 4, ctx_r165.ragProgress, "1.0-0"))), " ");
  }
}
function ProjectSettingsComponent_div_241_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 113);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-progress-bar", 114);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_241_div_14_span_2_Template, 4, 11, "span", 115);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r163 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("value", ctx_r163.ragProgress);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r163.ragTotal > 0);
  }
}
function ProjectSettingsComponent_div_241_p_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 117);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r164 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r164.ragMessage);
  }
}
function ProjectSettingsComponent_div_241_Template(rf, ctx) {
  if (rf & 1) {
    const _r167 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "div", 110)(2, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_241_Template_button_click_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r167);
      const ctx_r166 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r166.onRagReindex());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](3, ProjectSettingsComponent_div_241_mat_icon_3_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](4, ProjectSettingsComponent_div_241_mat_spinner_4_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "button", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_241_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r167);
      const ctx_r168 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r168.onRagClear());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10, "delete_sweep");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](13, ProjectSettingsComponent_div_241_p_13_Template, 8, 6, "p", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](14, ProjectSettingsComponent_div_241_div_14_Template, 3, 2, "div", 111);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](15, ProjectSettingsComponent_div_241_p_15_Template, 2, 1, "p", 112);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r36.ragReindexing || ctx_r36.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r36.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r36.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r36.ragReindexing ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 9, "PROJECT_SETTINGS.INDEXING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 11, "PROJECT_SETTINGS.FORCE_REINDEX"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r36.ragReindexing || ctx_r36.saving || ctx_r36.ragChunksCount === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 13, "PROJECT_SETTINGS.CLEAR_INDEX"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r36.ragMessage && !ctx_r36.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r36.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r36.ragMessage);
  }
}
function ProjectSettingsComponent_div_242_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_243_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_mat_card_244_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 25)(1, "mat-card-content")(2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 1, "PROJECT_SETTINGS.NO_EMBEDDING_MODEL"), " ");
  }
}
function ProjectSettingsComponent_div_247_mat_spinner_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_div_247_span_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 124);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r170 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r170.appsSavedMessage);
  }
}
function ProjectSettingsComponent_div_247_div_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 125)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "apps");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "p", 126);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 2, "PROJECT_SETTINGS.NO_APPS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](8, 4, "PROJECT_SETTINGS.NO_APPS_HINT"));
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_span_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 139);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](node_r174.name);
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r183 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "input", 140);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template_input_ngModelChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r183);
      const ctx_r182 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r182.editingCategoryName = $event);
    })("keydown.enter", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template_input_keydown_enter_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r183);
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
      const ctx_r184 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r184.confirmRenameCategory(node_r174));
    })("keydown.escape", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template_input_keydown_escape_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r183);
      const ctx_r186 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r186.cancelRenameCategory());
    })("blur", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template_input_blur_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r183);
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
      const ctx_r187 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r187.confirmRenameCategory(node_r174));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r178 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r178.editingCategoryName);
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_button_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r191 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 141);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_button_7_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r191);
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
      const ctx_r189 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r189.startRenameCategory(node_r174));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](1, 1, "COMMON.RENAME"));
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_div_12_Template(rf, ctx) {
  if (rf & 1) {
    const _r195 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 142)(1, "mat-icon", 143);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span", 144);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "span", 135)(6, "button", 141);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_div_12_Template_button_click_6_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r195);
      const child_r192 = restoredCtx.$implicit;
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
      const ctx_r193 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r193.moveAppToRoot(node_r174, child_r192));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9, "arrow_upward");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "button", 137);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_div_12_Template_button_click_10_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r195);
      const child_r192 = restoredCtx.$implicit;
      const ctx_r196 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r196.removeApp(child_r192.appId));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](13, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const child_r192 = ctx.$implicit;
    const ctx_r180 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r180.getAppIcon(child_r192.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r180.getAppName(child_r192.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 4, "PROJECT_SETTINGS.MOVE_TO_ROOT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 6, "PROJECT_SETTINGS.REMOVE_FROM_PROJECT"));
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r199 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 130)(1, "div", 131)(2, "mat-icon", 132);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](4, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_span_4_Template, 2, 1, "span", 133);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](5, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_input_5_Template, 1, 1, "input", 134);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](6, "span", 135);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](7, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_button_7_Template, 4, 3, "button", 136);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "button", 137);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r199);
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
      const ctx_r197 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r197.deleteCategory(node_r174));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](12, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_div_12_Template, 14, 8, "div", 138);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
    const ctx_r175 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r175.editingCategoryId !== node_r174.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r175.editingCategoryId === node_r174.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r175.editingCategoryId !== node_r174.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 5, "PROJECT_SETTINGS.DELETE_CATEGORY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", node_r174.children);
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_button_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 149);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3, "arrow_downward");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    const _r202 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵreference"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matMenuTriggerFor", _r202)("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](1, 2, "PROJECT_SETTINGS.MOVE_TO_CATEGORY"));
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_button_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r207 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "button", 150);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_button_9_Template_button_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r207);
      const cat_r204 = restoredCtx.$implicit;
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2).$implicit;
      const ctx_r205 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r205.moveAppToCategory(node_r174, cat_r204));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const cat_r204 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", cat_r204.name, " ");
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r210 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 145)(1, "mat-icon", 143);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span", 144);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "span", 135);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](6, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_button_6_Template, 4, 4, "button", 146);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "mat-menu", null, 147);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](9, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_button_9_Template, 4, 1, "button", 148);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "button", 137);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_Template_button_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r210);
      const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
      const ctx_r208 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r208.removeApp(node_r174.appId));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](13, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const node_r174 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
    const ctx_r176 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r176.getAppIcon(node_r174.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r176.getAppName(node_r174.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r176.getCategories().length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r176.getCategories());
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 5, "PROJECT_SETTINGS.REMOVE_FROM_PROJECT"));
  }
}
function ProjectSettingsComponent_div_247_div_21_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_1_Template, 13, 7, "div", 128);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](2, ProjectSettingsComponent_div_247_div_21_ng_container_1_div_2_Template, 14, 7, "div", 129);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const node_r174 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", node_r174.type === "category");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", node_r174.type === "app");
  }
}
function ProjectSettingsComponent_div_247_div_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 127);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_div_247_div_21_ng_container_1_Template, 3, 2, "ng-container", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r172 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r172.appsTree);
  }
}
function ProjectSettingsComponent_div_247_Template(rf, ctx) {
  if (rf & 1) {
    const _r213 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 3)(1, "div", 118)(2, "button", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_Template_button_click_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r213);
      const ctx_r212 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r212.openCatalogPicker());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_Template_button_click_7_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r213);
      const ctx_r214 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r214.addCategory());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](12, "span", 119);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](13, "button", 120);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_247_Template_button_click_13_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r213);
      const ctx_r215 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r215.saveAppsTree());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](14, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](15, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](17, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](18, ProjectSettingsComponent_div_247_mat_spinner_18_Template, 1, 0, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](19, ProjectSettingsComponent_div_247_span_19_Template, 2, 1, "span", 121);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](20, ProjectSettingsComponent_div_247_div_20_Template, 9, 6, "div", 122);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](21, ProjectSettingsComponent_div_247_div_21_Template, 2, 1, "div", 123);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r40.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 10, "PROJECT_SETTINGS.ADD_FROM_CATALOG"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r40.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](11, 12, "PROJECT_SETTINGS.NEW_CATEGORY"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r40.appsSaving || ctx_r40.appsTree.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](17, 14, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r40.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r40.appsSavedMessage);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r40.appsTree.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r40.appsTree.length > 0);
  }
}
function ProjectSettingsComponent_div_248_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_APPS"));
  }
}
function ProjectSettingsComponent_div_258_Template(rf, ctx) {
  if (rf & 1) {
    const _r217 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_258_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r217);
      const ctx_r216 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r216.kgEnabled = $event);
    })("change", function ProjectSettingsComponent_div_258_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r217);
      const ctx_r218 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r218.onKgEnabledChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r42.kgEnabled)("disabled", ctx_r42.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.KG_ENABLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.KG_ENABLE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_259_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_mat_card_260_mat_hint_31_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.KG_PASSWORD_HINT_KEEP"));
  }
}
function ProjectSettingsComponent_mat_card_260_mat_icon_34_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "cable");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_260_mat_spinner_35_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_260_mat_spinner_44_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_mat_card_260_span_45_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 161);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r223 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r223.kgTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_260_span_46_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 162);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r224 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r224.kgTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_260_Template(rf, ctx) {
  if (rf & 1) {
    const _r226 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 4)(1, "mat-card-header")(2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-card-content")(6, "div", 151)(7, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "mat-form-field", 153)(11, "input", 154);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_input_ngModelChange_11_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r225 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r225.kgUri = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "div", 151)(13, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](16, "mat-form-field", 153)(17, "input", 155);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_input_ngModelChange_17_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r227 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r227.kgDatabase = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](18, "div", 151)(19, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](21, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](22, "mat-form-field", 153)(23, "input", 155);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_input_ngModelChange_23_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r228 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r228.kgUsername = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](24, "div", 151)(25, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](26);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](27, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](28, "mat-form-field", 153)(29, "input", 156);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_input_ngModelChange_29_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r229 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r229.kgPassword = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](30, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](31, ProjectSettingsComponent_mat_card_260_mat_hint_31_Template, 3, 3, "mat-hint", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](32, "div", 157)(33, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_260_Template_button_click_33_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r230 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r230.onTestKgConnection());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](34, ProjectSettingsComponent_mat_card_260_mat_icon_34_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](35, ProjectSettingsComponent_mat_card_260_mat_spinner_35_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](37, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](38, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](39, "button", 120);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_260_Template_button_click_39_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r231 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r231.onSaveKgSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](40, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](41, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](42);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](43, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](44, ProjectSettingsComponent_mat_card_260_mat_spinner_44_Template, 1, 0, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](45, ProjectSettingsComponent_mat_card_260_span_45_Template, 2, 1, "span", 158);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](46, ProjectSettingsComponent_mat_card_260_span_46_Template, 2, 1, "span", 159);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](47, "div", 160)(48, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_mat_checkbox_ngModelChange_48_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r232 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r232.kgSyncOnTocGeneration = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](49, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](51, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](52, "div", 160)(53, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_260_Template_mat_checkbox_ngModelChange_53_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r226);
      const ctx_r233 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r233.kgSyncOnKgFileSave = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](54, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](56, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()()();
  }
  if (rf & 2) {
    const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 30, "PROJECT_SETTINGS.KG_CONNECTION"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 32, "PROJECT_SETTINGS.KG_URI"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgUri)("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](15, 34, "PROJECT_SETTINGS.KG_DATABASE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgDatabase)("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](21, 36, "PROJECT_SETTINGS.KG_USERNAME"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgUsername)("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](27, 38, "PROJECT_SETTINGS.KG_PASSWORD"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgPassword)("placeholder", ctx_r44.kgHasPassword ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](30, 40, "PROJECT_SETTINGS.KG_PASSWORD_KEEP") : "")("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r44.kgHasPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r44.kgSaving || ctx_r44.kgTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r44.kgTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r44.kgTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r44.kgTesting ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](37, 42, "PROJECT_SETTINGS.KG_TESTING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](38, 44, "PROJECT_SETTINGS.KG_TEST"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r44.kgSaving || ctx_r44.kgTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](43, 46, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r44.kgTestMessage && ctx_r44.kgTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r44.kgTestMessage && !ctx_r44.kgTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgSyncOnTocGeneration)("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](51, 48, "PROJECT_SETTINGS.KG_SYNC_ON_TOC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r44.kgSyncOnKgFileSave)("disabled", ctx_r44.kgSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](56, 50, "PROJECT_SETTINGS.KG_SYNC_ON_SAVE"));
  }
}
function ProjectSettingsComponent_mat_card_261_div_6_div_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 107)(1, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r241 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.KG_LAST_SYNC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](6, 4, ctx_r241.kgStateTotals.lastSyncAt, "short"));
  }
}
function ProjectSettingsComponent_mat_card_261_div_6_div_20_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 107)(1, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ns_r244 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ns_r244.graphNamespace);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate4"]("", ns_r244.files, " ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](5, 5, "PROJECT_SETTINGS.KG_STATS_FILES"), " \u00B7 ", ns_r244.concepts, " / ", ns_r244.relationships, "");
  }
}
function ProjectSettingsComponent_mat_card_261_div_6_div_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 169)(1, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](4, ProjectSettingsComponent_mat_card_261_div_6_div_20_div_4_Template, 6, 7, "div", 170);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r242 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.KG_NAMESPACES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r242.kgPerNamespace);
  }
}
function ProjectSettingsComponent_mat_card_261_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 167)(1, "div", 107)(2, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "div", 107)(8, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](11, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](13, "div", 107)(14, "span", 108);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](17, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](18);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](19, ProjectSettingsComponent_mat_card_261_div_6_div_19_Template, 7, 7, "div", 109);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](20, ProjectSettingsComponent_mat_card_261_div_6_div_20_Template, 5, 4, "div", 168);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r234 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 8, "PROJECT_SETTINGS.KG_STATS_FILES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"]((ctx_r234.kgStateTotals == null ? null : ctx_r234.kgStateTotals.files) || 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](10, 10, "PROJECT_SETTINGS.KG_STATS_CONCEPTS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"]((ctx_r234.kgStateTotals == null ? null : ctx_r234.kgStateTotals.concepts) || 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](16, 12, "PROJECT_SETTINGS.KG_STATS_RELATIONSHIPS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"]((ctx_r234.kgStateTotals == null ? null : ctx_r234.kgStateTotals.relationships) || 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r234.kgStateTotals == null ? null : ctx_r234.kgStateTotals.lastSyncAt);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r234.kgPerNamespace == null ? null : ctx_r234.kgPerNamespace.length);
  }
}
function ProjectSettingsComponent_mat_card_261_mat_icon_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "sync");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_261_mat_spinner_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_261_mat_icon_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "delete_sweep");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_261_mat_spinner_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_261_p_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 171);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r239 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r239.kgSyncMessage);
  }
}
function ProjectSettingsComponent_mat_card_261_div_21_div_4_ul_5_li_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "li");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const d_r249 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](d_r249);
  }
}
function ProjectSettingsComponent_mat_card_261_div_21_div_4_ul_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "ul");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](1, ProjectSettingsComponent_mat_card_261_div_21_div_4_ul_5_li_1_Template, 2, 1, "li", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const err_r246 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", err_r246.errorDetails);
  }
}
function ProjectSettingsComponent_mat_card_261_div_21_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 174)(1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](5, ProjectSettingsComponent_mat_card_261_div_21_div_4_ul_5_Template, 2, 1, "ul", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const err_r246 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](err_r246.sourceDocPath);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](err_r246.error);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", err_r246.errorDetails == null ? null : err_r246.errorDetails.length);
  }
}
function ProjectSettingsComponent_mat_card_261_div_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 172)(1, "h4");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](4, ProjectSettingsComponent_mat_card_261_div_21_div_4_Template, 6, 3, "div", 173);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r240 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.KG_FILES_WITH_ERRORS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx_r240.kgSyncErrors);
  }
}
function ProjectSettingsComponent_mat_card_261_Template(rf, ctx) {
  if (rf & 1) {
    const _r252 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 4)(1, "mat-card-header")(2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](6, ProjectSettingsComponent_mat_card_261_div_6_Template, 21, 14, "div", 163);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "div", 164)(8, "button", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_261_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r252);
      const ctx_r251 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r251.onSyncKgProject());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](9, ProjectSettingsComponent_mat_card_261_mat_icon_9_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](10, ProjectSettingsComponent_mat_card_261_mat_spinner_10_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](14, "button", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_261_Template_button_click_14_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r252);
      const ctx_r253 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r253.onResetKg());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](15, ProjectSettingsComponent_mat_card_261_mat_icon_15_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](16, ProjectSettingsComponent_mat_card_261_mat_spinner_16_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](19, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](20, ProjectSettingsComponent_mat_card_261_p_20_Template, 2, 1, "p", 165);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](21, ProjectSettingsComponent_mat_card_261_div_21_Template, 5, 4, "div", 166);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r45 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 12, "PROJECT_SETTINGS.KG_SYNC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r45.kgStateLoaded);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r45.kgSyncing || ctx_r45.kgResetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r45.kgSyncing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r45.kgSyncing);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r45.kgSyncing ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 14, "PROJECT_SETTINGS.KG_SYNCING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](13, 16, "PROJECT_SETTINGS.KG_SYNC_ALL"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r45.kgSyncing || ctx_r45.kgResetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r45.kgResetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r45.kgResetting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r45.kgResetting ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](18, 18, "PROJECT_SETTINGS.KG_RESETTING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](19, 20, "PROJECT_SETTINGS.KG_RESET"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r45.kgSyncMessage);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r45.kgSyncErrors == null ? null : ctx_r45.kgSyncErrors.length);
  }
}
function ProjectSettingsComponent_div_271_Template(rf, ctx) {
  if (rf & 1) {
    const _r255 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_271_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r255);
      const ctx_r254 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r254.fsEnabled = $event);
    })("change", function ProjectSettingsComponent_div_271_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r255);
      const ctx_r256 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r256.onFusekiEnabledChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r46 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r46.fsEnabled)("disabled", ctx_r46.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.FUSEKI_ENABLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.FUSEKI_ENABLE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_272_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_mat_card_273_span_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](5, ") ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r257 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" (", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 2, "PROJECT_SETTINGS.FUSEKI_DATASET_DEFAULT"), ": ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r257.fsDefaultDataset);
  }
}
function ProjectSettingsComponent_mat_card_273_mat_hint_38_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 1, "PROJECT_SETTINGS.KG_PASSWORD_HINT_KEEP"));
  }
}
function ProjectSettingsComponent_mat_card_273_mat_icon_41_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "cable");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_273_mat_spinner_42_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_273_mat_spinner_56_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_mat_card_273_span_57_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 161);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r262 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r262.fsTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_273_span_58_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 162);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r263 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r263.fsTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_273_Template(rf, ctx) {
  if (rf & 1) {
    const _r265 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 4)(1, "mat-card-header")(2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-card-content")(6, "div", 151)(7, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "mat-form-field", 153)(11, "input", 175);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_input_ngModelChange_11_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r264 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r264.fsUri = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "div", 151)(13, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](16, ProjectSettingsComponent_mat_card_273_span_16_Template, 6, 4, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](17, "mat-form-field", 153)(18, "input", 176);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_input_ngModelChange_18_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r266 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r266.fsDataset = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](19, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](21, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](22, "div", 151)(23, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](24);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](25, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](26, "span", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](27);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](28, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](29, "mat-form-field", 153)(30, "input", 177);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_input_ngModelChange_30_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r267 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r267.fsUsername = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](31, "div", 151)(32, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](33);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](34, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](35, "mat-form-field", 153)(36, "input", 156);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_input_ngModelChange_36_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r268 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r268.fsPassword = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](37, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](38, ProjectSettingsComponent_mat_card_273_mat_hint_38_Template, 3, 3, "mat-hint", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](39, "div", 157)(40, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_273_Template_button_click_40_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r269 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r269.onTestFusekiConnection(false));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](41, ProjectSettingsComponent_mat_card_273_mat_icon_41_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](42, ProjectSettingsComponent_mat_card_273_mat_spinner_42_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](43);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](44, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](45, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](46, "button", 178);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_273_Template_button_click_46_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r270 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r270.onCreateFusekiDataset());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](47, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](48, "add_box");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](49);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](50, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](51, "button", 120);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_273_Template_button_click_51_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r271 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r271.onSaveFusekiSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](52, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](53, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](55, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](56, ProjectSettingsComponent_mat_card_273_mat_spinner_56_Template, 1, 0, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](57, ProjectSettingsComponent_mat_card_273_span_57_Template, 2, 1, "span", 158);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](58, ProjectSettingsComponent_mat_card_273_span_58_Template, 2, 1, "span", 159);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](59, "div", 160)(60, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_mat_checkbox_ngModelChange_60_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r272 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r272.fsSyncOnTocGeneration = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](61, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](62);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](63, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](64, "div", 160)(65, "mat-checkbox", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_273_Template_mat_checkbox_ngModelChange_65_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r265);
      const ctx_r273 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r273.fsSyncOnKgFileSave = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](66, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](67);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](68, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()()();
  }
  if (rf & 2) {
    const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 36, "PROJECT_SETTINGS.FUSEKI_CONNECTION"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 38, "PROJECT_SETTINGS.FUSEKI_URI"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsUri)("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](15, 40, "PROJECT_SETTINGS.FUSEKI_DATASET"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsDefaultDataset && ctx_r48.fsDefaultDataset !== ctx_r48.fsDataset);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsDataset)("placeholder", ctx_r48.fsDefaultDataset)("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](21, 42, "PROJECT_SETTINGS.FUSEKI_DATASET_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](25, 44, "PROJECT_SETTINGS.FUSEKI_USERNAME"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"]("(", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](28, 46, "PROJECT_SETTINGS.FUSEKI_AUTH_OPTIONAL"), ")");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsUsername)("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](34, 48, "PROJECT_SETTINGS.FUSEKI_PASSWORD"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsPassword)("placeholder", ctx_r48.fsHasPassword ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](37, 50, "PROJECT_SETTINGS.KG_PASSWORD_KEEP") : "")("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsHasPassword);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r48.fsSaving || ctx_r48.fsTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r48.fsTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r48.fsTesting ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](44, 52, "PROJECT_SETTINGS.FUSEKI_TESTING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](45, 54, "PROJECT_SETTINGS.FUSEKI_TEST"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r48.fsSaving || ctx_r48.fsTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](50, 56, "PROJECT_SETTINGS.FUSEKI_CREATE_DATASET"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r48.fsSaving || ctx_r48.fsTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](55, 58, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsTestMessage && ctx_r48.fsTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r48.fsTestMessage && !ctx_r48.fsTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsSyncOnTocGeneration)("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](63, 60, "PROJECT_SETTINGS.KG_SYNC_ON_TOC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r48.fsSyncOnKgFileSave)("disabled", ctx_r48.fsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](68, 62, "PROJECT_SETTINGS.KG_SYNC_ON_SAVE"));
  }
}
function ProjectSettingsComponent_div_283_Template(rf, ctx) {
  if (rf & 1) {
    const _r275 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_283_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r275);
      const ctx_r274 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r274.atlEnabled = $event);
    })("change", function ProjectSettingsComponent_div_283_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r275);
      const ctx_r276 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r276.onAtlassianEnabledChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r49.atlEnabled)("disabled", ctx_r49.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.ATLASSIAN_ENABLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.ATLASSIAN_ENABLE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_284_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_mat_card_285_span_42_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r277 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate2"](" \u2014 ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](2, 2, "PROJECT_SETTINGS.CONFLUENCE_BASE_URL_EFFECTIVE"), ": ", ctx_r277.atlConfluenceBaseUrlEffective, " ");
  }
}
function ProjectSettingsComponent_mat_card_285_mat_icon_64_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "cable");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_285_mat_spinner_65_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_285_mat_spinner_74_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_mat_card_285_span_75_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 161);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r281 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r281.atlTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_285_span_76_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 162);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r282 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r282.atlTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_285_Template(rf, ctx) {
  if (rf & 1) {
    const _r284 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 4)(1, "mat-card-header")(2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-card-content")(6, "div", 151)(7, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "mat-form-field", 153)(11, "input", 179);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_11_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r283 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r283.atlBaseUrl = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](15, "div", 151)(16, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](19, "mat-form-field", 153)(20, "input", 180);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_20_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r285 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r285.atlProjectKeys = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](21, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](22);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](23, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](24, "div", 151)(25, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](26);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](27, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](28, "mat-form-field", 153)(29, "input", 181);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_29_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r286 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r286.atlConfluenceSpaceKeys = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](30, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](32, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](33, "div", 151)(34, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](35);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](36, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](37, "mat-form-field", 153)(38, "input", 176);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_38_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r287 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r287.atlConfluenceBaseUrl = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](39, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](40);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](41, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](42, ProjectSettingsComponent_mat_card_285_span_42_Template, 3, 4, "span", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](43, "div", 151)(44, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](45);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](46, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](47, "mat-form-field", 153)(48, "input", 182);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_48_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r288 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r288.atlEmail = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](49, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](51, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](52, "div", 151)(53, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](55, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](56, "mat-form-field", 153)(57, "input", 156);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_285_Template_input_ngModelChange_57_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r289 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r289.atlToken = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](58, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](59, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](60);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](61, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](62, "div", 157)(63, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_285_Template_button_click_63_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r290 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r290.onTestAtlassianConnection());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](64, ProjectSettingsComponent_mat_card_285_mat_icon_64_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](65, ProjectSettingsComponent_mat_card_285_mat_spinner_65_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](66);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](67, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](68, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](69, "button", 120);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_285_Template_button_click_69_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r284);
      const ctx_r291 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r291.onSaveAtlassianSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](70, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](71, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](72);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](73, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](74, ProjectSettingsComponent_mat_card_285_mat_spinner_74_Template, 1, 0, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](75, ProjectSettingsComponent_mat_card_285_span_75_Template, 2, 1, "span", 158);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](76, ProjectSettingsComponent_mat_card_285_span_76_Template, 2, 1, "span", 159);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r51 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 37, "PROJECT_SETTINGS.ATLASSIAN_CONNECTION"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 39, "PROJECT_SETTINGS.ATLASSIAN_BASE_URL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlBaseUrl)("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](14, 41, "PROJECT_SETTINGS.ATLASSIAN_SHARED_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](18, 43, "PROJECT_SETTINGS.ATLASSIAN_PROJECT_KEYS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlProjectKeys)("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](23, 45, "PROJECT_SETTINGS.ATLASSIAN_PROJECT_KEYS_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](27, 47, "PROJECT_SETTINGS.CONFLUENCE_SPACE_KEYS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlConfluenceSpaceKeys)("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](32, 49, "PROJECT_SETTINGS.CONFLUENCE_SPACE_KEYS_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](36, 51, "PROJECT_SETTINGS.CONFLUENCE_BASE_URL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlConfluenceBaseUrl)("placeholder", ctx_r51.atlConfluenceBaseUrlEffective || "https://acme.atlassian.net/wiki")("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](41, 53, "PROJECT_SETTINGS.CONFLUENCE_BASE_URL_HINT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r51.atlConfluenceBaseUrlEffective);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](46, 55, "PROJECT_SETTINGS.ATLASSIAN_EMAIL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlEmail)("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](51, 57, "PROJECT_SETTINGS.ATLASSIAN_PERSONAL_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](55, 59, "PROJECT_SETTINGS.ATLASSIAN_TOKEN"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r51.atlToken)("placeholder", ctx_r51.atlHasToken ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](58, 61, "PROJECT_SETTINGS.ATLASSIAN_TOKEN_KEEP") : "")("disabled", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](61, 63, "PROJECT_SETTINGS.ATLASSIAN_TOKEN_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r51.atlSaving || ctx_r51.atlTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r51.atlTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r51.atlTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r51.atlTesting ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](67, 65, "PROJECT_SETTINGS.ATLASSIAN_TESTING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](68, 67, "PROJECT_SETTINGS.ATLASSIAN_TEST"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r51.atlSaving || ctx_r51.atlTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](73, 69, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r51.atlSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r51.atlTestMessage && ctx_r51.atlTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r51.atlTestMessage && !ctx_r51.atlTestSuccess);
  }
}
function ProjectSettingsComponent_div_292_Template(rf, ctx) {
  if (rf & 1) {
    const _r293 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 8)(1, "mat-checkbox", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_292_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r293);
      const ctx_r292 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r292.rmEnabled = $event);
    })("change", function ProjectSettingsComponent_div_292_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r293);
      const ctx_r294 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r294.onRedmineEnabledChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "p", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r52 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r52.rmEnabled)("disabled", ctx_r52.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.REDMINE_ENABLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.REDMINE_ENABLE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_293_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](1, "mat-spinner", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_mat_card_294_mat_icon_52_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1, "cable");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_mat_card_294_mat_spinner_53_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 56);
  }
}
function ProjectSettingsComponent_mat_card_294_mat_spinner_62_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelement"](0, "mat-spinner", 59);
  }
}
function ProjectSettingsComponent_mat_card_294_span_63_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 161);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r298 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r298.rmTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_294_span_64_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "span", 162);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r299 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r299.rmTestMessage);
  }
}
function ProjectSettingsComponent_mat_card_294_p_65_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "p", 162);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r300 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](ctx_r300.rmSaveError);
  }
}
function ProjectSettingsComponent_mat_card_294_Template(rf, ctx) {
  if (rf & 1) {
    const _r302 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "mat-card", 4)(1, "mat-card-header")(2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](5, "mat-card-content")(6, "div", 151)(7, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](10, "mat-form-field", 153)(11, "input", 183);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_294_Template_input_ngModelChange_11_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r301 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r301.rmBaseUrl = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](12, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](15, "div", 151)(16, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](19, "mat-form-field", 153)(20, "input", 184);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_294_Template_input_ngModelChange_20_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r303 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r303.rmProjects = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](21, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](22);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](23, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](24, "div", 151)(25, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](26);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](27, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](28, "mat-form-field", 153)(29, "mat-select", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_294_Template_mat_select_ngModelChange_29_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r304 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r304.rmTextFormatting = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](30, "mat-option", 185);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](32, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](33, "mat-option", 186);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](34, "Markdown");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](35, "mat-option", 187);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](36, "Textile");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](37, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](38);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](39, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](40, "div", 151)(41, "label", 152);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](42);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](43, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](44, "mat-form-field", 153)(45, "input", 156);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_mat_card_294_Template_input_ngModelChange_45_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r305 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r305.rmApiKey = $event);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](46, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](47, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](48);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](49, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](50, "div", 157)(51, "button", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_294_Template_button_click_51_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r306 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r306.onTestRedmineConnection());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](52, ProjectSettingsComponent_mat_card_294_mat_icon_52_Template, 2, 0, "mat-icon", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](53, ProjectSettingsComponent_mat_card_294_mat_spinner_53_Template, 1, 0, "mat-spinner", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](54);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](55, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](56, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](57, "button", 120);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_mat_card_294_Template_button_click_57_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵrestoreView"](_r302);
      const ctx_r307 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵresetView"](ctx_r307.onSaveRedmineSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](58, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](59, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](60);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](61, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](62, ProjectSettingsComponent_mat_card_294_mat_spinner_62_Template, 1, 0, "mat-spinner", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](63, ProjectSettingsComponent_mat_card_294_span_63_Template, 2, 1, "span", 158);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](64, ProjectSettingsComponent_mat_card_294_span_64_Template, 2, 1, "span", 159);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](65, ProjectSettingsComponent_mat_card_294_p_65_Template, 2, 1, "p", 159);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](4, 29, "PROJECT_SETTINGS.REDMINE_CONNECTION"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](9, 31, "PROJECT_SETTINGS.REDMINE_BASE_URL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r54.rmBaseUrl)("disabled", ctx_r54.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](14, 33, "PROJECT_SETTINGS.REDMINE_BASE_URL_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](18, 35, "PROJECT_SETTINGS.REDMINE_PROJECTS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r54.rmProjects)("disabled", ctx_r54.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](23, 37, "PROJECT_SETTINGS.REDMINE_PROJECTS_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](27, 39, "PROJECT_SETTINGS.REDMINE_TEXT_FORMATTING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r54.rmTextFormatting)("disabled", ctx_r54.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](32, 41, "PROJECT_SETTINGS.REDMINE_TEXT_FORMATTING_NONE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](39, 43, "PROJECT_SETTINGS.REDMINE_TEXT_FORMATTING_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](43, 45, "PROJECT_SETTINGS.REDMINE_API_KEY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx_r54.rmApiKey)("placeholder", ctx_r54.rmHasKey ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](46, 47, "PROJECT_SETTINGS.REDMINE_API_KEY_KEEP") : "")("disabled", ctx_r54.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](49, 49, "PROJECT_SETTINGS.REDMINE_API_KEY_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r54.rmSaving || ctx_r54.rmTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx_r54.rmTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r54.rmTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", ctx_r54.rmTesting ? _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](55, 51, "PROJECT_SETTINGS.REDMINE_TESTING") : _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](56, 53, "PROJECT_SETTINGS.REDMINE_TEST"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx_r54.rmSaving || ctx_r54.rmTesting);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](61, 55, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r54.rmSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r54.rmTestMessage && ctx_r54.rmTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r54.rmTestMessage && !ctx_r54.rmTestSuccess);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx_r54.rmSaveError);
  }
}
const _c7 = function (a0) {
  return {
    name: a0
  };
};
class ProjectSettingsComponent {
  constructor(dialogRef, data, projectSettingsService, compatibilityService, ideConfigService, serverMessages, projectsService, externalAppsService, mdFileService, dialog, translate, agentCityState) {
    this.dialogRef = dialogRef;
    this.data = data;
    this.projectSettingsService = projectSettingsService;
    this.compatibilityService = compatibilityService;
    this.ideConfigService = ideConfigService;
    this.serverMessages = serverMessages;
    this.projectsService = projectsService;
    this.externalAppsService = externalAppsService;
    this.mdFileService = mdFileService;
    this.dialog = dialog;
    this.translate = translate;
    this.agentCityState = agentCityState;
    this.rule1Enabled = false;
    this.linkIndexingEnabled = true;
    this.plantUmlKeepOriginalColorsEnabled = false;
    // Motore di MarkAgent. `markAgentEngine` è quello IN VIGORE, `engineChoice` è quello che si
    // vede nel pannello avanzate: 'linked' = segue l'ambiente (la colonna a NULL nel DB).
    this.markAgentEngine = 'none';
    this.engineChoice = 'linked';
    this.savingEngine = false;
    /** Finché la risposta non arriva, un avviso di scollegamento sarebbe inventato. */
    this.engineSettingsLoaded = false;
    // Gruppi di funzionalità MCP: quali strumenti MdExplorer mette nel contesto della chat.
    // L'elenco e i pesi arrivano dal server MCP, qui non se ne tiene una copia.
    this.mcpGroups = [];
    /** false = nessuno ha ancora scelto, le caselle sono una proposta letta dalle integrazioni. */
    this.mcpGroupsChosen = false;
    this.mcpGroupsLoaded = false;
    this.savingMcpGroups = false;
    /** Il server MCP non c'è o non risponde: si dice, invece di mostrare zero gruppi. */
    this.mcpGroupsError = null;
    // Agent City / Federation (§12.4) — activation lives in .development.yml (shared via git).
    this.agentCityEnabled = false;
    this.agentCityOwnershipDoc = '';
    /** Il documento del workflow degli agenti (mde_type: workflow): quando c'è, MdExplorer lo applica. */
    /** I template da cui viene il progetto (righe «templates:» di .development.yml). */
    this.projectTemplates = [];
    this.newMethodUrl = '';
    this.newMethodLanguage = 'it';
    this.newMethodExample = false;
    this.addingMethod = false;
    /** Perché il metodo non si è aggiunto, detto dal servizio. */
    this.methodError = '';
    /** Che cosa MDE ha scritto per il metodo appena aggiunto, e cosa resta da fare. */
    this.methodNotes = [];
    /** I documenti dei workflow (PW: un progetto ne può avere più d'uno). Una riga vuota è una riga appena aggiunta. */
    this.agentCityWorkflowDocs = [];
    /** Perché l'ultimo salvataggio della città non è andato, detto dal servizio (es. lo stesso workflow due volte). */
    this.agentCityError = '';
    /** Com'era la città all'ultima lettura: spuntarla da spenta la accende con tutto ciò che porta (C6). */
    this.agentCityWasEnabled = false;
    /** La città è accesa ma la sua documentazione non c'è: si offre di scaricarla. */
    this.agentCityGuideMissing = false;
    this.agentCityBusy = false;
    this.agentCityNotes = [];
    this.agentCityHasRoomSecret = false;
    this.agentUseWorktrees = false;
    /** Posti di lavoro del pool: quanti agenti possono lavorare insieme su questa macchina. */
    this.agentWorktreeSlots = 2;
    this.agentWorktreeDefaultSlots = 2;
    this.agentWorktreeMaxSlots = 8;
    /** Senza git non esistono worktree né merge: le due opzioni restano spente e non toccabili. */
    this.projectIsGit = false;
    // Relay della federazione (per progetto). La chiave non arriva mai dal server:
    // relayApiKey è solo il campo di INSERIMENTO, vuoto se non si sta cambiando nulla.
    this.relayUrl = '';
    this.relayUrlSource = 'None';
    this.relayHasApiKey = false;
    this.relayApiKeySource = 'None';
    this.relayApiKey = '';
    this.relayTesting = false;
    this.relayTestMessage = '';
    this.relayTestSuccess = null;
    this.indexAllTextFilesEnabled = false;
    this.textFileExtensions = '';
    this.textFileExtensionsDefault = '';
    this.reindexingText = false;
    this.githubModeEnabled = false;
    this.stickyScrollEnabled = true;
    this.selectedIde = 'vscode';
    this.lastSavedIde = 'vscode';
    // Harness agentico (.development.yml). `harnessDeclared` a false = progetto creato prima che
    // la scelta esistesse: quello mostrato è dedotto dalle cartelle sul disco, non ancora scritto.
    this.harness = 'none';
    this.harnessDeclared = true;
    this.lastSavedHarness = 'none';
    /** Il modello predefinito del progetto (harness.model): quello salvato e quello nel campo. */
    this.harnessModel = '';
    this.lastSavedHarnessModel = '';
    this.savingHarnessModel = false;
    this.savingHarness = false;
    this.vscodePath = '';
    this.intellijPath = '';
    this.loading = false;
    this.saving = false;
    // Apps tab
    this.appsTree = [];
    this.appDefinitions = [];
    this.appsLoading = true;
    this.appsSaving = false;
    this.editingCategoryId = null;
    this.editingCategoryName = '';
    this.appsSavedMessage = '';
    // Knowledge Graph (Neo4j) settings
    this.kgLoading = false;
    this.kgSaving = false;
    this.kgEnabled = false;
    this.kgUri = 'bolt://localhost:7687';
    this.kgDatabase = 'neo4j';
    this.kgUsername = 'neo4j';
    this.kgPassword = '';
    this.kgHasPassword = false;
    this.kgSyncOnTocGeneration = true;
    this.kgSyncOnKgFileSave = true;
    this.kgTesting = false;
    this.kgTestMessage = '';
    this.kgTestSuccess = false;
    this.kgSyncing = false;
    this.kgResetting = false;
    this.kgSyncMessage = '';
    this.kgSyncErrors = [];
    this.kgStateLoaded = false;
    this.kgStateTotals = null;
    this.kgPerNamespace = [];
    // Apache Jena Fuseki settings (specchio del blocco Neo4j)
    this.fsLoading = false;
    this.fsSaving = false;
    this.fsEnabled = false;
    this.fsUri = 'http://localhost:3030';
    this.fsDataset = ''; // default dal nome progetto sanitizzato, popolato dal GET
    this.fsDefaultDataset = ''; // suggerimento del server
    this.fsUsername = '';
    this.fsPassword = '';
    this.fsHasPassword = false;
    this.fsSyncOnTocGeneration = true;
    this.fsSyncOnKgFileSave = true;
    this.fsTesting = false;
    this.fsTestMessage = '';
    this.fsTestSuccess = false;
    // Atlassian (Jira/Confluence) settings
    // Shared (jiraBaseUrl/projectKeys/confluence) -> .development.yml.
    // Token -> UserDB encrypted. Email lives in UserDB (personal).
    this.atlLoading = false;
    this.atlSaving = false;
    this.atlEnabled = false;
    this.atlBaseUrl = '';
    this.atlProjectKeys = ''; // comma-separated in the UI, split on save
    // Confluence shares the Atlassian site & token. Base URL is derived as
    // {jiraBaseUrl}/wiki; the override is only for the rare different-site case.
    this.atlConfluenceBaseUrl = ''; // optional override (empty = derived)
    this.atlConfluenceBaseUrlEffective = ''; // read-only, what will actually be used
    this.atlConfluenceSpaceKeys = ''; // comma-separated in the UI
    this.atlEmail = '';
    this.atlToken = '';
    this.atlHasToken = false;
    this.atlTesting = false;
    this.atlTestMessage = '';
    this.atlTestSuccess = false;
    // Redmine: indirizzo, progetti e formato del testo -> .development.yml; chiave API -> UserDB cifrata.
    this.rmLoading = false;
    this.rmSaving = false;
    this.rmEnabled = false;
    this.rmBaseUrl = '';
    this.rmProjects = ''; // separati da virgola nella UI
    this.rmTextFormatting = ''; // '' | 'markdown' | 'textile'
    this.rmApiKey = '';
    this.rmHasKey = false;
    this.rmTesting = false;
    this.rmTestMessage = '';
    this.rmTestSuccess = false;
    this.rmSaveError = '';
    // RAG settings
    this.ragEnabled = false;
    this.ragModelInstalled = false;
    this.ragModelLoaded = false;
    this.ragChunksCount = 0;
    this.ragEmbeddedCount = 0;
    this.ragReindexing = false;
    this.ragMessage = '';
    this.ragProgress = 0;
    this.ragTotal = 0;
    this.ragProcessed = 0;
    // Full project reindex (incremental indexing escape hatch)
    this.projectReindexing = false;
    this.projectId = data.projectId;
    this.projectName = data.projectName;
    this.projectPath = data.projectPath;
  }
  ngOnInit() {
    this.loadSettings();
    this.loadAppsConfig();
    this.loadKgSettings();
    this.loadFusekiSettings();
    this.loadAtlassianSettings();
    this.loadRedmineSettings();
    this.loadAgentCity();
    this.loadTemplates();
    this.loadHarness();
    this.loadMcpToolGroups();
    this.ragProgressSub = this.serverMessages.ragIndexingProgress$.subscribe(data => {
      this.ragProcessed = data.processed;
      this.ragTotal = data.total;
      this.ragProgress = data.total > 0 ? data.processed / data.total * 100 : 0;
      this.ragMessage = data.message;
      if (data.status === 'completed' || data.status === 'error') {
        this.ragReindexing = false;
        this.refreshRagStatus();
      }
    });
    // Full project reindex: progress shows in the global indexing snackbar
    // (parsingProjectStart/knowledgeProgress); here we only track the button state.
    this.projectReindexSub = this.serverMessages.parsingProjectStop$.subscribe(() => {
      this.projectReindexing = false;
    });
  }
  ngOnDestroy() {
    this.ragProgressSub?.unsubscribe();
    this.projectReindexSub?.unsubscribe();
  }
  onProjectReindex() {
    this.projectReindexing = true;
    this.projectSettingsService.reindexProject(this.serverMessages.connectionId ?? '').subscribe({
      next: () => {
        console.log('[ProjectSettings] Full project reindex started');
      },
      error: error => {
        console.error('[ProjectSettings] Error starting project reindex:', error);
        this.projectReindexing = false;
      }
    });
  }
  loadSettings() {
    this.loading = true;
    let rule1Loaded = false;
    let linkIndexingLoaded = false;
    let compatibilityLoaded = false;
    let ideConfigLoaded = false;
    let ragLoaded = false;
    let stickyScrollLoaded = false;
    let plantUmlKeepOriginalColorsLoaded = false;
    let markAgentEngineLoaded = false;
    let textIndexingLoaded = false;
    const checkIfDone = () => {
      if (rule1Loaded && linkIndexingLoaded && compatibilityLoaded && ideConfigLoaded && ragLoaded && stickyScrollLoaded && plantUmlKeepOriginalColorsLoaded && markAgentEngineLoaded && textIndexingLoaded) {
        this.loading = false;
      }
    };
    // Load Rule 1 setting
    this.projectSettingsService.getRule1Setting().subscribe({
      next: response => {
        this.rule1Enabled = response.enabled;
        rule1Loaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading Rule 1 setting:', error);
        rule1Loaded = true;
        checkIfDone();
      }
    });
    // Load Link Indexing setting
    this.projectSettingsService.getLinkIndexingSetting(this.projectPath).subscribe({
      next: response => {
        this.linkIndexingEnabled = response.enabled;
        linkIndexingLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading Link Indexing setting:', error);
        linkIndexingLoaded = true;
        checkIfDone();
      }
    });
    // Load PlantUML Keep Original Colors setting
    this.projectSettingsService.getPlantUmlKeepOriginalColorsSetting(this.projectPath).subscribe({
      next: response => {
        this.plantUmlKeepOriginalColorsEnabled = response.enabled;
        plantUmlKeepOriginalColorsLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading PlantUML Keep Original Colors setting:', error);
        plantUmlKeepOriginalColorsLoaded = true;
        checkIfDone();
      }
    });
    // Motore di MarkAgent: una chiamata sola, che dice anche se segue l'ambiente.
    this.projectSettingsService.getMarkAgentEngine(this.projectPath).subscribe({
      next: response => {
        this.applyEngineResponse(response.engine, response.linked);
        this.engineSettingsLoaded = true;
        markAgentEngineLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading the MarkAgent engine:', error);
        // engineSettingsLoaded resta false: senza risposta non si disegna né un motore né un
        // avviso di scollegamento, che sarebbero inventati.
        markAgentEngineLoaded = true;
        checkIfDone();
      }
    });
    // Load Text Indexing (non-markdown text files) setting
    this.projectSettingsService.getTextIndexingSetting(this.projectPath).subscribe({
      next: response => {
        this.indexAllTextFilesEnabled = response.enabled;
        this.textFileExtensions = response.extensions || '';
        this.textFileExtensionsDefault = response.defaultExtensions || '';
        textIndexingLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading Text Indexing setting:', error);
        textIndexingLoaded = true;
        checkIfDone();
      }
    });
    // Load compatibility mode for this specific project
    this.compatibilityService.getCurrentMode(this.projectPath).subscribe({
      next: response => {
        console.log('Compatibility mode loaded for project:', this.projectPath, response);
        this.githubModeEnabled = response.mode === 'github';
        compatibilityLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading compatibility mode:', error);
        compatibilityLoaded = true;
        checkIfDone();
      }
    });
    // Load IDE configuration for this specific project
    this.ideConfigService.getIdeConfiguration(this.projectPath).subscribe({
      next: response => {
        console.log('IDE configuration loaded for project:', this.projectPath, response);
        // 'copilot' è il valore storico della voce, quando era un CLI fisso: la migrazione
        // M2026_09_21_003 lo converte, ma un DB non ancora migrato lo manderebbe ancora, e il
        // radio resterebbe senza nessuna voce selezionata.
        const ide = response.selectedIde || 'vscode';
        this.selectedIde = ide === 'copilot' ? 'agent-cli' : ide;
        this.lastSavedIde = this.selectedIde;
        this.vscodePath = response.vscodePath || '';
        this.intellijPath = response.intellijPath || '';
        ideConfigLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading IDE configuration:', error);
        ideConfigLoaded = true;
        checkIfDone();
      }
    });
    // Load Sticky Scroll setting
    this.projectSettingsService.getStickyScrollSetting().subscribe({
      next: response => {
        this.stickyScrollEnabled = response.enabled;
        stickyScrollLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading Sticky Scroll setting:', error);
        stickyScrollLoaded = true;
        checkIfDone();
      }
    });
    // Load RAG status
    this.projectSettingsService.getRagStatus().subscribe({
      next: response => {
        this.ragEnabled = response.enabled;
        this.ragModelInstalled = response.modelInstalled;
        this.ragModelLoaded = response.modelLoaded;
        this.ragChunksCount = response.chunksCount || 0;
        this.ragEmbeddedCount = response.embeddedCount || 0;
        // Recover indexing state if reindex is in progress
        if (response.isIndexing && response.indexingProgress) {
          this.ragReindexing = true;
          this.ragProcessed = response.indexingProgress.processed || 0;
          this.ragTotal = response.indexingProgress.total || 0;
          this.ragProgress = this.ragTotal > 0 ? this.ragProcessed / this.ragTotal * 100 : 0;
          this.ragMessage = response.indexingProgress.message || this.translate.instant('PROJECT_SETTINGS.INDEXING');
        }
        ragLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading RAG status:', error);
        ragLoaded = true;
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
      error: error => {
        console.error('Error saving Rule 1 setting:', error);
        this.saving = false;
        // Revert the change on error
        this.rule1Enabled = !this.rule1Enabled;
      }
    });
  }
  onStickyScrollChange() {
    this.saving = true;
    this.projectSettingsService.setStickyScrollSetting(this.stickyScrollEnabled).subscribe({
      next: () => {
        console.log('Sticky Scroll setting saved successfully');
        this.saving = false;
      },
      error: error => {
        console.error('Error saving Sticky Scroll setting:', error);
        this.saving = false;
        this.stickyScrollEnabled = !this.stickyScrollEnabled;
      }
    });
  }
  onLinkIndexingChange() {
    this.saving = true;
    this.projectSettingsService.setLinkIndexingSetting(this.linkIndexingEnabled, this.projectPath).subscribe({
      next: () => {
        console.log('Link Indexing setting saved successfully');
        this.saving = false;
      },
      error: error => {
        console.error('Error saving Link Indexing setting:', error);
        this.saving = false;
        this.linkIndexingEnabled = !this.linkIndexingEnabled;
      }
    });
  }
  onPlantUmlKeepOriginalColorsChange() {
    this.saving = true;
    this.projectSettingsService.setPlantUmlKeepOriginalColorsSetting(this.plantUmlKeepOriginalColorsEnabled, this.projectPath).subscribe({
      next: () => {
        document.body.classList.toggle('plantuml-keep-original', this.plantUmlKeepOriginalColorsEnabled);
        this.saving = false;
      },
      error: error => {
        console.error('Error saving PlantUML Keep Original Colors setting:', error);
        this.saving = false;
        this.plantUmlKeepOriginalColorsEnabled = !this.plantUmlKeepOriginalColorsEnabled;
      }
    });
  }
  /** Allinea i campi della UI alla risposta del server. */
  applyEngineResponse(engine, linked) {
    this.markAgentEngine = engine;
    this.engineChoice = linked ? 'linked' : engine;
  }
  /**
   * Il motore in vigore è DAVVERO un altro rispetto all'ambiente del repository.
   * <br>Non basta che sia stato fissato a mano: fissare «Copilot CLI» su un progetto il cui
   * ambiente è già Copilot non è un disaccoppiamento, e dirlo sarebbe una bugia (la scelta resta
   * tua, ma oggi coincide col predefinito). Gli identificativi dell'ambiente e
   * del motore coincidono — 'copilot', 'opencode', 'claude', 'none' — perché sono la stessa
   * scelta vista da due lati, quindi il confronto è diretto.
   */
  get engineDecoupled() {
    return this.engineSettingsLoaded && this.engineChoice !== 'linked' && this.markAgentEngine !== this.harness;
  }
  /** Quanti token di strumenti entrano nel contesto con le caselle spuntate adesso. */
  get mcpTokensSelected() {
    return this.mcpGroups.filter(g => g.enabled).reduce((sum, g) => sum + g.approxTokens, 0);
  }
  /** Quanti ne entrerebbero con tutti i gruppi accesi: il termine di paragone. */
  get mcpTokensAll() {
    return this.mcpGroups.reduce((sum, g) => sum + g.approxTokens, 0);
  }
  loadMcpToolGroups() {
    if (!this.projectPath) return;
    this.projectSettingsService.getMcpToolGroups(this.projectPath).subscribe({
      next: res => {
        this.mcpGroups = res.groups;
        this.mcpGroupsChosen = res.chosen;
        this.mcpGroupsError = null;
        this.mcpGroupsLoaded = true;
      },
      error: err => {
        console.error('Error loading the MCP tool groups:', err);
        this.mcpGroups = [];
        this.mcpGroupsError = err?.error?.error ?? this.translate.instant('PROJECT_SETTINGS.MCP_GROUPS_UNAVAILABLE');
        this.mcpGroupsLoaded = true;
      }
    });
  }
  onMcpGroupChange() {
    this.savingMcpGroups = true;
    const enabled = this.mcpGroups.filter(g => g.enabled).map(g => g.id);
    this.projectSettingsService.setMcpToolGroups(enabled, this.projectPath).subscribe({
      next: () => {
        this.mcpGroupsChosen = true;
        this.savingMcpGroups = false;
      },
      error: err => {
        console.error('Error saving the MCP tool groups:', err);
        this.savingMcpGroups = false;
        this.loadMcpToolGroups(); // riallinea allo stato persistito
      }
    });
  }
  onEngineChange() {
    const wanted = this.engineChoice === 'linked' ? null : this.engineChoice;
    this.savingEngine = true;
    this.projectSettingsService.setMarkAgentEngine(wanted, this.projectPath).subscribe({
      next: res => {
        this.applyEngineResponse(res.engine, res.linked);
        this.savingEngine = false;
      },
      error: err => {
        console.error('Error saving the MarkAgent engine:', err);
        this.savingEngine = false;
        this.loadMarkAgentEngine(); // riallinea allo stato persistito
      }
    });
  }
  get harnessModelChanged() {
    return (this.harnessModel || '').trim() !== (this.lastSavedHarnessModel || '').trim();
  }
  /** Scrive il modello predefinito in .development.yml: vale per il team, come il motore. */
  saveHarnessModel() {
    this.savingHarnessModel = true;
    this.projectSettingsService.setHarnessModel(this.harness, (this.harnessModel || '').trim(), this.projectPath).subscribe({
      next: res => {
        this.harnessModel = this.lastSavedHarnessModel = res?.model || '';
        this.savingHarnessModel = false;
      },
      error: err => {
        console.error('Error saving the project default model:', err);
        this.savingHarnessModel = false;
        this.loadHarness();
      }
    });
  }
  /** "Usa il predefinito": la scelta personale si toglie e vale il predefinito del progetto. */
  relinkEngine() {
    this.engineChoice = 'linked';
    this.onEngineChange();
  }
  loadMarkAgentEngine() {
    if (!this.projectPath) return;
    this.projectSettingsService.getMarkAgentEngine(this.projectPath).subscribe({
      next: res => {
        this.applyEngineResponse(res.engine, res.linked);
        this.engineSettingsLoaded = true;
      },
      error: err => console.error('Error loading the MarkAgent engine:', err)
    });
  }
  loadAgentCity() {
    if (!this.projectPath) return;
    this.projectSettingsService.getAgentCity(this.projectPath).subscribe({
      next: res => {
        this.agentCityEnabled = !!res?.enabled;
        this.agentCityWasEnabled = this.agentCityEnabled;
        this.agentCityGuideMissing = !!res?.guideMissing;
        this.agentCityOwnershipDoc = res?.ownershipDoc || '';
        this.agentCityWorkflowDocs = [...(res?.workflowDocs || [])];
        this.agentCityHasRoomSecret = !!res?.hasRoomSecret;
        this.projectIsGit = !!res?.isGitRepository;
      },
      error: err => console.error('Error loading Agent City settings:', err)
    });
    this.loadRelaySettings();
    this.loadAgentWorktrees();
  }
  /** Preferenza locale di isolamento: UserDB, non .development.yml. */
  loadAgentWorktrees() {
    if (!this.projectPath) return;
    this.projectSettingsService.getAgentWorktreesSetting(this.projectPath).subscribe({
      next: res => {
        this.agentUseWorktrees = !!res?.enabled;
        if (res?.slots) this.agentWorktreeSlots = res.slots;
        if (res?.defaultSlots) this.agentWorktreeDefaultSlots = res.defaultSlots;
        if (res?.maxSlots) this.agentWorktreeMaxSlots = res.maxSlots;
      },
      error: err => console.error('Error loading agent worktrees setting:', err)
    });
  }
  onAgentWorktreesChange() {
    this.saveAgentWorktrees();
  }
  /**
   * Quanti posti di lavoro. È anche il tetto degli agenti che girano insieme: sono la stessa
   * domanda, e per questo qui c'è un numero solo.
   */
  onAgentWorktreeSlotsChange() {
    const n = Number(this.agentWorktreeSlots);
    if (!Number.isInteger(n) || n < 1 || n > this.agentWorktreeMaxSlots) {
      // Un numero fuori scala non si manda al server: si rimette quello vero.
      this.loadAgentWorktrees();
      return;
    }
    this.saveAgentWorktrees();
  }
  /** Il nome del CLI, senza le cartelle: quelle appartengono all'ambiente, non al motore. */
  engineLabelKey(engine) {
    switch (engine) {
      case 'copilot':
        return 'PROJECT_SETTINGS.ENGINE_COPILOT';
      case 'opencode':
        return 'PROJECT_SETTINGS.ENGINE_OPENCODE';
      case 'claude':
        return 'PROJECT_SETTINGS.ENGINE_CLAUDE';
      default:
        return 'PROJECT_SETTINGS.ENGINE_NONE';
    }
  }
  harnessLabelKey(target) {
    switch (target) {
      case 'copilot':
        return 'PROJECT_SETTINGS.HARNESS_COPILOT';
      case 'opencode':
        return 'PROJECT_SETTINGS.HARNESS_OPENCODE';
      case 'claude':
        return 'PROJECT_SETTINGS.HARNESS_CLAUDE';
      default:
        return 'PROJECT_SETTINGS.HARNESS_NONE';
    }
  }
  loadHarness() {
    if (!this.projectPath) return;
    this.projectSettingsService.getHarness(this.projectPath).subscribe({
      next: res => {
        this.harness = res?.target || 'none';
        this.harnessDeclared = !!res?.declared;
        this.lastSavedHarness = this.harness;
        this.harnessModel = this.lastSavedHarnessModel = res?.model || '';
      },
      error: err => {
        console.error('Error loading harness setting:', err);
      }
    });
  }
  /**
   * Cambiare harness installa subito i file dove il nuovo harness li vuole, ma NON rimuove
   * quelli del precedente: sono file che stanno in un repository, magari personalizzati o già
   * committati, e cancellarli per un cambio di impostazione sarebbe un danno che nessuno ha
   * chiesto. La UI lo dice, così la cartella rimasta non sembra un errore.
   */
  onHarnessChange() {
    if (this.harness === this.lastSavedHarness) return;
    // .development.yml è committato: cambiare ambiente non riguarda solo questa macchina.
    // Si chiede conferma PRIMA di scrivere, e un "no" rimette la scelta com'era.
    const wanted = this.harness;
    const data = {
      title: this.translate.instant('PROJECT_SETTINGS.HARNESS_SWITCH_CONFIRM_TITLE'),
      message: this.translate.instant('PROJECT_SETTINGS.HARNESS_SWITCH_CONFIRM_MESSAGE', {
        harness: this.translate.instant(this.harnessLabelKey(wanted))
      }),
      confirmText: this.translate.instant('COMMON.CONFIRM')
    };
    this.dialog.open(_commons_components_confirm_dialog_confirm_dialog_component__WEBPACK_IMPORTED_MODULE_0__.ConfirmDialogComponent, {
      width: '480px',
      data
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        this.harness = this.lastSavedHarness;
        return;
      }
      this.saveHarness(wanted);
    });
  }
  saveHarness(wanted) {
    this.harness = wanted;
    this.savingHarness = true;
    this.projectSettingsService.setHarness(this.harness, this.projectPath).subscribe({
      next: res => {
        this.harness = res?.target || this.harness;
        this.harnessDeclared = true;
        this.lastSavedHarness = this.harness;
        // Cambiato il motore, il modello del progetto (che era del motore di prima) non c'è più.
        this.harnessModel = this.lastSavedHarnessModel = res?.model || '';
        this.savingHarness = false;
        // Se il motore segue l'ambiente, ora è un altro: si rilegge invece di indovinarlo.
        this.loadMarkAgentEngine();
      },
      error: err => {
        console.error('Error saving harness setting:', err);
        this.savingHarness = false;
        this.loadHarness(); // riallinea allo stato persistito
      }
    });
  }
  saveAgentWorktrees() {
    this.saving = true;
    this.projectSettingsService.setAgentWorktreesSetting(this.agentUseWorktrees, this.projectPath, this.agentWorktreeSlots).subscribe({
      next: () => {
        this.saving = false;
      },
      error: err => {
        console.error('Error saving agent worktrees setting:', err);
        this.saving = false;
        this.loadAgentWorktrees(); // riallinea allo stato persistito
      }
    });
  }
  loadRelaySettings() {
    if (!this.projectPath) return;
    this.projectSettingsService.getRelaySettings(this.projectPath).subscribe({
      next: res => {
        this.relayUrl = res?.relayUrl || '';
        this.relayUrlSource = res?.relayUrlSource || 'None';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestSuccess = res?.lastTestSuccess ?? null;
      },
      error: err => console.error('Error loading relay settings:', err)
    });
  }
  onRelaySettingsChange() {
    if (!this.projectPath) return;
    this.saving = true;
    this.projectSettingsService.setRelaySettings(this.projectPath, {
      relayUrl: this.relayUrl?.trim() || undefined,
      // Campo vuoto = "non sto cambiando la chiave": il server la lascia com'è.
      apiKey: this.relayApiKey?.trim() || undefined
    }).subscribe({
      next: res => {
        this.saving = false;
        this.relayApiKey = ''; // mai tenere il segreto nel campo dopo il salvataggio
        this.relayUrl = res?.relayUrl || '';
        this.relayUrlSource = res?.relayUrlSource || 'None';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestMessage = '';
        this.relayTestSuccess = null;
      },
      error: err => {
        console.error('Error saving relay settings:', err);
        this.saving = false;
        this.relayTestSuccess = false;
        this.relayTestMessage = err?.error?.message || 'Salvataggio fallito.';
        this.loadRelaySettings(); // riallinea allo stato persistito
      }
    });
  }
  clearRelayApiKey() {
    if (!this.projectPath) return;
    this.saving = true;
    this.projectSettingsService.setRelaySettings(this.projectPath, {
      relayUrl: this.relayUrl?.trim() || undefined,
      clearApiKey: true
    }).subscribe({
      next: res => {
        this.saving = false;
        this.relayApiKey = '';
        this.relayHasApiKey = !!res?.hasApiKey;
        this.relayApiKeySource = res?.apiKeySource || 'None';
        this.relayTestMessage = '';
        this.relayTestSuccess = null;
      },
      error: err => {
        console.error('Error clearing relay api key:', err);
        this.saving = false;
        this.loadRelaySettings();
      }
    });
  }
  testRelay() {
    if (!this.projectPath) return;
    this.relayTesting = true;
    this.relayTestMessage = '';
    this.projectSettingsService.testRelaySettings(this.projectPath).subscribe({
      next: res => {
        this.relayTesting = false;
        this.relayTestSuccess = !!res?.success;
        this.relayTestMessage = res?.message || '';
      },
      error: err => {
        this.relayTesting = false;
        this.relayTestSuccess = false;
        this.relayTestMessage = err?.error?.message || 'Verifica fallita.';
      }
    });
  }
  loadTemplates() {
    if (!this.projectPath) return;
    this.projectSettingsService.getTemplates(this.projectPath).subscribe({
      next: res => {
        this.projectTemplates = res?.templates || [];
        // La lingua del metodo segue quella del progetto: la sua base, o il primo template.
        const base = this.projectTemplates.find(t => t.kind === 'project') || this.projectTemplates[0];
        this.newMethodLanguage = base?.language || (this.translate.currentLang === 'en' ? 'en' : 'it');
      },
      error: err => {
        this.projectTemplates = [];
        this.methodError = err?.error?.error || err?.message || '';
      }
    });
  }
  addMethod() {
    const url = (this.newMethodUrl || '').trim();
    if (!url) return;
    this.addingMethod = true;
    this.methodError = '';
    this.methodNotes = [];
    this.projectSettingsService.addTemplate(this.projectPath, url, this.newMethodLanguage, this.newMethodExample).subscribe({
      next: res => {
        this.addingMethod = false;
        this.newMethodUrl = '';
        this.methodNotes = [this.translate.instant('PROJECT_SETTINGS.TEMPLATE_ADDED', {
          title: res?.title,
          version: res?.version
        }), ...(res?.notes || [])];
        this.loadTemplates();
        // Il metodo può aver portato workflow e il documento delle responsabilità.
        this.loadAgentCity();
      },
      error: err => {
        this.addingMethod = false;
        this.methodError = err?.error?.error || err?.message || '';
      }
    });
  }
  /** Il nome del tipo di una riga: strato comune, metodo o progetto completo (la demo). */
  templateKindKey(t) {
    if (t.kind === 'project') return 'PROJECT_SETTINGS.TEMPLATE_KIND_PROJECT';
    if (t.kind === 'foundation') return 'PROJECT_SETTINGS.TEMPLATE_KIND_FOUNDATION';
    return 'PROJECT_SETTINGS.TEMPLATE_KIND_METHOD';
  }
  /** «Scarica la teoria»: la copia dalla zona del template, con un commit (M3 dei metodi componibili). */
  addTheory(t) {
    this.addingMethod = true;
    this.methodError = '';
    this.methodNotes = [];
    this.projectSettingsService.addTheory(this.projectPath, t.id).subscribe({
      next: res => {
        this.addingMethod = false;
        this.methodNotes = [this.translate.instant('PROJECT_SETTINGS.TEMPLATE_THEORY_ADDED', {
          title: res?.title,
          version: res?.version
        }), ...(res?.notes || [])];
        this.loadTemplates();
      },
      error: err => {
        this.addingMethod = false;
        this.methodError = err?.error?.error || err?.message || '';
      }
    });
  }
  /** Stacca il progetto da un template, per sempre: si chiede prima, con quello che vuol dire. */
  detachTemplate(t) {
    const ref = this.dialog.open(_commons_components_confirm_dialog_confirm_dialog_component__WEBPACK_IMPORTED_MODULE_0__.ConfirmDialogComponent, {
      width: '480px',
      data: {
        title: this.translate.instant('PROJECT_SETTINGS.TEMPLATE_DETACH_TITLE', {
          id: t.id
        }),
        message: this.translate.instant('PROJECT_SETTINGS.TEMPLATE_DETACH_CONFIRM', {
          id: t.id
        }),
        confirmText: this.translate.instant('PROJECT_SETTINGS.TEMPLATE_DETACH'),
        cancelText: this.translate.instant('COMMON.CANCEL')
      }
    });
    ref.afterClosed().subscribe(ok => {
      if (!ok) return;
      this.addingMethod = true;
      this.methodError = '';
      this.methodNotes = [];
      this.projectSettingsService.detachTemplate(this.projectPath, t.id).subscribe({
        next: res => {
          this.addingMethod = false;
          this.methodNotes = [res.message];
          this.loadTemplates();
        },
        error: err => {
          this.addingMethod = false;
          this.methodError = err?.error?.error || err?.message || '';
        }
      });
    });
  }
  addWorkflowDoc() {
    this.agentCityWorkflowDocs = [...this.agentCityWorkflowDocs, ''];
  }
  removeWorkflowDoc(index) {
    const wasWritten = !!(this.agentCityWorkflowDocs[index] || '').trim();
    this.agentCityWorkflowDocs = this.agentCityWorkflowDocs.filter((_, i) => i !== index);
    if (wasWritten) this.onAgentCityChange();
  }
  trackByIndex(index) {
    return index;
  }
  /**
   * «Accendi la città» (C6): la parte degli agenti dei template del progetto, i file della città e le sue skill, con un
   * commit; con <paramref name="guide"/> e la città già accesa, solo la sua documentazione.
   */
  enableAgentCity(guide) {
    this.agentCityBusy = true;
    this.agentCityError = '';
    this.agentCityNotes = [];
    this.projectSettingsService.enableAgentCity(this.projectPath, guide, this.translate.currentLang === 'en' ? 'en' : 'it').subscribe({
      next: res => {
        this.agentCityBusy = false;
        this.agentCityNotes = res?.notes || [];
        this.agentCityState.set(this.projectPath, true);
        this.loadAgentCity();
        this.loadTemplates();
      },
      error: err => {
        this.agentCityBusy = false;
        this.agentCityError = err?.error?.error || err?.message || '';
        this.loadAgentCity();
      }
    });
  }
  onAgentCityChange() {
    // Da spenta ad accesa: la città arriva con tutto ciò che le serve, come alla creazione del progetto.
    if (this.agentCityEnabled && !this.agentCityWasEnabled) {
      this.enableAgentCity(false);
      return;
    }
    this.saving = true;
    this.projectSettingsService.setAgentCity(this.projectPath, {
      enabled: this.agentCityEnabled,
      ownershipDoc: this.agentCityOwnershipDoc?.trim() || undefined,
      // Elenco vuoto vuol dire «nessun workflow»: si manda sempre, perché il servizio distingue «non toccare» (assente)
      // da «nessuno». Le righe vuote (appena aggiunte, non ancora scritte) non sono workflow.
      workflowDocs: this.agentCityWorkflowDocs.map(d => (d || '').trim()).filter(d => d.length > 0)
      // Solo ciò che questa schermata decide. Il worktree è una preferenza di macchina con un endpoint suo:
      // mandarlo, anche a `false`, scriveva nel `.development.yml` una scelta che nessuno aveva fatto.
    }).subscribe({
      next: res => {
        this.saving = false;
        this.agentCityHasRoomSecret = !!res?.hasRoomSecret;
        this.agentCityWasEnabled = !!res?.enabled;
        this.agentCityOwnershipDoc = res?.ownershipDoc || '';
        this.agentCityError = '';
        // Una riga appena aggiunta e ancora vuota resta lì da scrivere: il salvataggio parte anche dal blur di un'altra.
        const pending = this.agentCityWorkflowDocs.filter(d => !(d || '').trim()).length;
        this.agentCityWorkflowDocs = [...(res?.workflowDocs || []), ...Array(pending).fill('')];
        // La toolbar mostra i comandi della città in base a questo flag: allineala
        // subito, altrimenti resterebbe ferma fino alla riapertura del progetto.
        // Il service ignora il salvataggio se riguarda un progetto non aperto.
        this.agentCityState.set(this.projectPath, !!res?.enabled);
      },
      error: err => {
        console.error('Error saving Agent City settings:', err);
        this.saving = false;
        this.agentCityError = err?.error?.message || err?.message || '';
        // Un rifiuto del servizio (400: per esempio lo stesso workflow due volte): le righe restano come le ha scritte la
        // persona, da correggere; il file non è cambiato.
        if (err?.status === 400) return;
        // Il salvataggio è fallito: riallinea la UI allo stato PERSISTITO rileggendolo
        // dal server. Niente inversione cieca del checkbox: questo handler è agganciato
        // anche al blur del campo ownership-doc, e lì il toggle non è stato toccato.
        this.loadAgentCity();
      }
    });
  }
  onTextIndexingChange() {
    this.saving = true;
    this.projectSettingsService.setTextIndexingSetting(this.indexAllTextFilesEnabled, this.textFileExtensions, this.projectPath).subscribe({
      next: () => {
        this.saving = false;
      },
      error: error => {
        console.error('Error saving Text Indexing setting:', error);
        this.saving = false;
        this.indexAllTextFilesEnabled = !this.indexAllTextFilesEnabled;
      }
    });
  }
  onReindexTextFiles() {
    this.reindexingText = true;
    this.projectSettingsService.reindexTextFiles(this.serverMessages.connectionId ?? '').subscribe({
      next: () => {
        // Fire-and-forget on the backend; the index rebuilds in the background.
        setTimeout(() => {
          this.reindexingText = false;
        }, 1500);
      },
      error: error => {
        console.error('Error triggering text reindex:', error);
        this.reindexingText = false;
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
      next: response => {
        console.log('Compatibility mode saved successfully:', mode, response);
        this.saving = false;
      },
      error: error => {
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
      next: response => {
        console.log('IDE configuration saved successfully:', this.selectedIde, response);
        this.lastSavedIde = this.selectedIde;
        this.saving = false;
      },
      error: error => {
        console.error('Error saving IDE configuration:', error);
        this.saving = false;
        // Revert to the last successfully-saved value (works for any number of options)
        this.selectedIde = this.lastSavedIde;
      }
    });
  }
  onRagChange() {
    this.saving = true;
    const action$ = this.ragEnabled ? this.projectSettingsService.enableRag() : this.projectSettingsService.disableRag();
    action$.subscribe({
      next: () => {
        console.log('RAG setting saved:', this.ragEnabled);
        this.saving = false;
        this.projectsService.ragEnabled$.next(this.ragEnabled);
        this.refreshRagStatus();
      },
      error: error => {
        console.error('Error saving RAG setting:', error);
        this.saving = false;
        this.ragEnabled = !this.ragEnabled;
      }
    });
  }
  onRagReindex() {
    this.ragReindexing = true;
    this.ragProgress = 0;
    this.ragProcessed = 0;
    this.ragTotal = 0;
    this.ragMessage = this.translate.instant('PROJECT_SETTINGS.STARTING_INDEXING');
    this.projectSettingsService.reindexRag(this.projectPath).subscribe({
      next: response => {
        console.log('RAG reindex started:', response);
        // Progress updates will arrive via SignalR
      },
      error: error => {
        console.error('Error triggering RAG reindex:', error);
        this.ragReindexing = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_STARTING_INDEXING');
      }
    });
  }
  onRagClear() {
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.DELETE_RAG_CONFIRM'))) {
      return;
    }
    this.saving = true;
    this.projectSettingsService.clearRagIndex(this.projectPath).subscribe({
      next: response => {
        console.log('RAG index cleared:', response);
        this.saving = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.CLEARED_CHUNKS', {
          count: response.chunksDeleted
        });
        this.refreshRagStatus();
      },
      error: error => {
        console.error('Error clearing RAG index:', error);
        this.saving = false;
        this.ragMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_CLEARING_INDEX');
      }
    });
  }
  refreshRagStatus() {
    this.projectSettingsService.getRagStatus().subscribe({
      next: response => {
        this.ragEnabled = response.enabled;
        this.ragModelInstalled = response.modelInstalled;
        this.ragModelLoaded = response.modelLoaded;
        this.ragChunksCount = response.chunksCount || 0;
        this.ragEmbeddedCount = response.embeddedCount || 0;
      }
    });
  }
  // ── Apps tab ──────────────────────────────
  loadAppsConfig() {
    this.appsLoading = true;
    console.log('[ProjectSettings] loadAppsConfig — projectPath:', this.projectPath);
    this.externalAppsService.getConfig(this.projectPath).subscribe({
      next: config => {
        console.log('[ProjectSettings] apps config loaded:', config);
        this.appDefinitions = config.apps || [];
        this.appsTree = config.tree || [];
        this.appsLoading = false;
        console.log('[ProjectSettings] appsTree:', this.appsTree.length, 'appDefinitions:', this.appDefinitions.length);
      },
      error: err => {
        console.error('[ProjectSettings] Error loading apps config:', err);
        this.appDefinitions = [];
        this.appsTree = [];
        this.appsLoading = false;
      }
    });
  }
  getAppName(appId) {
    const app = this.appDefinitions.find(a => a.id === appId);
    return app?.name || appId;
  }
  getAppIcon(appId) {
    const app = this.appDefinitions.find(a => a.id === appId);
    return app?.icon || 'apps';
  }
  getCategories() {
    return this.appsTree.filter(n => n.type === 'category');
  }
  openCatalogPicker() {
    const existingIds = this.appDefinitions.map(a => a.id);
    const ref = this.dialog.open(_dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_1__.CatalogPickerDialogComponent, {
      width: '560px',
      data: {
        existingAppIds: existingIds
      }
    });
    ref.afterClosed().subscribe(selected => {
      if (!selected) return;
      // Convert StoreCatalogApp to MdeAppDefinition
      const newApp = {
        id: selected.id,
        name: selected.name,
        description: selected.description || '',
        icon: selected.icon || 'apps',
        executable: '',
        args: selected.defaultArgs || [],
        singleton: true
      };
      this.appDefinitions.push(newApp);
      this.appsTree.push({
        type: 'app',
        appId: selected.id
      });
      // Save full config (apps + tree) in one call
      this.saveAppsTree();
    });
  }
  addCategory() {
    const name = prompt(this.translate.instant('PROJECT_SETTINGS.CATEGORY_NAME_PROMPT'));
    if (!name || !name.trim()) return;
    const cat = {
      type: 'category',
      id: 'cat-' + Date.now(),
      name: name.trim(),
      icon: 'folder',
      children: []
    };
    this.appsTree.push(cat);
    this.saveAppsTree();
  }
  deleteCategory(cat) {
    // Move children to root level
    if (cat.children && cat.children.length > 0) {
      for (const child of cat.children) {
        this.appsTree.push(child);
      }
    }
    this.appsTree = this.appsTree.filter(n => n !== cat);
    this.saveAppsTree();
  }
  startRenameCategory(cat) {
    this.editingCategoryId = cat.id;
    this.editingCategoryName = cat.name || '';
  }
  confirmRenameCategory(cat) {
    if (this.editingCategoryName.trim()) {
      cat.name = this.editingCategoryName.trim();
      this.saveAppsTree();
    }
    this.editingCategoryId = null;
  }
  cancelRenameCategory() {
    this.editingCategoryId = null;
  }
  moveAppToCategory(appNode, targetCat) {
    // Remove from root
    this.appsTree = this.appsTree.filter(n => n !== appNode);
    // Remove from any other category
    for (const cat of this.getCategories()) {
      if (cat.children) {
        cat.children = cat.children.filter(c => c.appId !== appNode.appId);
      }
    }
    // Add to target
    if (!targetCat.children) targetCat.children = [];
    targetCat.children.push({
      type: 'app',
      appId: appNode.appId
    });
    this.saveAppsTree();
  }
  moveAppToRoot(cat, childNode) {
    if (cat.children) {
      cat.children = cat.children.filter(c => c !== childNode);
    }
    this.appsTree.push({
      type: 'app',
      appId: childNode.appId
    });
    this.saveAppsTree();
  }
  removeApp(appId) {
    const appName = this.getAppName(appId);
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.REMOVE_APP_CONFIRM', {
      name: appName
    }))) return;
    // Remove from tree (root + categories)
    this.appsTree = this.appsTree.filter(n => !(n.type === 'app' && n.appId === appId));
    for (const cat of this.getCategories()) {
      if (cat.children) {
        cat.children = cat.children.filter(c => c.appId !== appId);
      }
    }
    // Remove from definitions
    this.appDefinitions = this.appDefinitions.filter(a => a.id !== appId);
    // Save full config (apps + tree) in one call
    this.saveAppsTree();
  }
  saveAppsTree() {
    this.appsSaving = true;
    this.appsSavedMessage = '';
    const config = {
      version: '2',
      apps: this.appDefinitions,
      tree: this.appsTree
    };
    this.externalAppsService.saveConfig(config, this.projectPath).subscribe({
      next: () => {
        this.appsSaving = false;
        this.appsSavedMessage = this.translate.instant('PROJECT_SETTINGS.SAVED');
        this.refreshTree();
        setTimeout(() => this.appsSavedMessage = '', 3000);
      },
      error: err => {
        console.error('Error saving config:', err);
        this.appsSaving = false;
        this.appsSavedMessage = this.translate.instant('PROJECT_SETTINGS.ERROR_SAVING');
      }
    });
  }
  refreshTree() {
    // Refresh md-tree solo se siamo dentro un progetto aperto (connectionId registrato)
    if (this.serverMessages.connectionId) {
      this.mdFileService.loadAll(null, null);
    }
  }
  close() {
    this.dialogRef.close();
  }
  // ============================================================
  //   Knowledge Graph (Neo4j) — tab logic
  // ============================================================
  loadKgSettings() {
    if (!this.projectId) return;
    this.kgLoading = true;
    this.projectSettingsService.getKgSettings(this.projectId).subscribe({
      next: r => {
        this.kgEnabled = !!r.enabled;
        this.kgUri = r.uri || 'bolt://localhost:7687';
        this.kgDatabase = r.database || 'neo4j';
        this.kgUsername = r.username || 'neo4j';
        this.kgHasPassword = !!r.hasPassword;
        this.kgPassword = '';
        this.kgSyncOnTocGeneration = r.syncOnTocGeneration !== false;
        this.kgSyncOnKgFileSave = r.syncOnKgFileSave !== false;
        if (r.lastTestSuccess === true) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_OK_PREVIOUS');
          this.kgTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL_PREVIOUS');
          this.kgTestSuccess = false;
        }
        this.kgLoading = false;
        if (this.kgEnabled && this.kgHasPassword) {
          this.loadKgState();
        }
      },
      error: err => {
        console.error('Error loading KG settings:', err);
        this.kgLoading = false;
      }
    });
  }
  onKgEnabledChange() {
    // No immediate save — user must click Save to apply (so password and other fields go together).
    // If user just turned it off, persist immediately (no other state to gather).
    if (!this.kgEnabled) {
      this.onSaveKgSettings();
    }
  }
  onTestKgConnection() {
    this.kgTesting = true;
    this.kgTestMessage = '';
    this.projectSettingsService.testKgConnection({
      projectId: this.projectId,
      uri: this.kgUri,
      database: this.kgDatabase,
      username: this.kgUsername,
      password: this.kgPassword || (this.kgHasPassword ? '********' : '')
    }).subscribe({
      next: r => {
        this.kgTesting = false;
        this.kgTestSuccess = !!r.success;
        if (r.success) {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_OK', {
            ms: r.latencyMs
          });
        } else {
          this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL', {
            error: r.error || 'unknown'
          });
        }
      },
      error: err => {
        this.kgTesting = false;
        this.kgTestSuccess = false;
        this.kgTestMessage = this.translate.instant('PROJECT_SETTINGS.KG_TEST_FAIL', {
          error: err?.message || 'http error'
        });
      }
    });
  }
  onSaveKgSettings() {
    this.kgSaving = true;
    this.projectSettingsService.saveKgSettings(this.projectId, {
      enabled: this.kgEnabled,
      uri: this.kgUri,
      database: this.kgDatabase,
      username: this.kgUsername,
      password: this.kgPassword || '',
      syncOnTocGeneration: this.kgSyncOnTocGeneration,
      syncOnKgFileSave: this.kgSyncOnKgFileSave
    }).subscribe({
      next: () => {
        this.kgSaving = false;
        if (this.kgPassword) {
          this.kgHasPassword = true;
          this.kgPassword = '';
        }
        if (this.kgEnabled && this.kgHasPassword) {
          this.loadKgState();
        }
      },
      error: err => {
        this.kgSaving = false;
        console.error('Error saving KG settings:', err);
      }
    });
  }
  loadKgState() {
    this.projectSettingsService.getKgState(this.projectId).subscribe({
      next: r => {
        this.kgStateTotals = r.totals;
        this.kgPerNamespace = r.perNamespace || [];
        this.kgStateLoaded = true;
      },
      error: err => {
        console.error('Error loading KG state:', err);
        this.kgStateLoaded = false;
      }
    });
  }
  onSyncKgProject() {
    this.kgSyncing = true;
    this.kgSyncMessage = '';
    this.kgSyncErrors = [];
    this.projectSettingsService.syncKgProject(this.projectId).subscribe({
      next: r => {
        this.kgSyncing = false;
        const results = r.results || [];
        const ok = results.filter(x => !x.error && !x.skipped).length;
        const skipped = results.filter(x => x.skipped).length;
        const failed = results.filter(x => x.error).length;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_SYNC_DONE', {
          ok,
          skipped,
          failed,
          total: results.length
        });
        this.kgSyncErrors = results.filter(x => x.error);
        this.loadKgState();
      },
      error: err => {
        this.kgSyncing = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_SYNC_FAILED', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }
  onResetKg() {
    if (!confirm(this.translate.instant('PROJECT_SETTINGS.KG_RESET_CONFIRM'))) return;
    this.kgResetting = true;
    this.kgSyncMessage = '';
    this.projectSettingsService.resetKg(this.projectId).subscribe({
      next: () => {
        this.kgResetting = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_RESET_DONE');
        this.kgSyncErrors = [];
        this.loadKgState();
      },
      error: err => {
        this.kgResetting = false;
        this.kgSyncMessage = this.translate.instant('PROJECT_SETTINGS.KG_RESET_FAILED', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }
  // ============================================================
  //   Apache Jena Fuseki — tab logic (specchio del blocco KG/Neo4j)
  // ============================================================
  loadFusekiSettings() {
    if (!this.projectId) return;
    this.fsLoading = true;
    this.projectSettingsService.getFusekiSettings(this.projectId).subscribe({
      next: r => {
        this.fsEnabled = !!r.enabled;
        this.fsUri = r.uri || 'http://localhost:3030';
        this.fsDataset = r.dataset || '';
        this.fsDefaultDataset = r.defaultDataset || '';
        this.fsUsername = r.username || '';
        this.fsHasPassword = !!r.hasPassword;
        this.fsPassword = '';
        this.fsSyncOnTocGeneration = r.syncOnTocGeneration !== false;
        this.fsSyncOnKgFileSave = r.syncOnKgFileSave !== false;
        if (r.lastTestSuccess === true) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK_PREVIOUS');
          this.fsTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL_PREVIOUS');
          this.fsTestSuccess = false;
        }
        this.fsLoading = false;
      },
      error: err => {
        console.error('Error loading Fuseki settings:', err);
        this.fsLoading = false;
      }
    });
  }
  onFusekiEnabledChange() {
    if (!this.fsEnabled) {
      this.onSaveFusekiSettings();
    }
  }
  onTestFusekiConnection(autoCreate = false) {
    this.fsTesting = true;
    this.fsTestMessage = '';
    this.projectSettingsService.testFusekiConnection({
      projectId: this.projectId,
      uri: this.fsUri,
      dataset: this.fsDataset,
      username: this.fsUsername,
      password: this.fsPassword || (this.fsHasPassword ? '********' : ''),
      autoCreateDataset: autoCreate
    }).subscribe({
      next: r => {
        this.fsTesting = false;
        this.fsTestSuccess = !!r.success;
        if (r.success) {
          if (r.datasetCreated) {
            this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK_DATASET_CREATED', {
              dataset: r.dataset,
              ms: r.latencyMs
            });
          } else {
            this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_OK', {
              dataset: r.dataset,
              ms: r.latencyMs
            });
          }
        } else if (r.serverReachable && !r.datasetExists) {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_DATASET_MISSING', {
            dataset: r.dataset
          });
        } else {
          this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL', {
            error: r.error || 'unknown'
          });
        }
      },
      error: err => {
        this.fsTesting = false;
        this.fsTestSuccess = false;
        this.fsTestMessage = this.translate.instant('PROJECT_SETTINGS.FUSEKI_TEST_FAIL', {
          error: err?.message || 'http error'
        });
      }
    });
  }
  onCreateFusekiDataset() {
    // Test + auto-create in un colpo solo
    this.onTestFusekiConnection(true);
  }
  onSaveFusekiSettings() {
    this.fsSaving = true;
    this.projectSettingsService.saveFusekiSettings(this.projectId, {
      enabled: this.fsEnabled,
      uri: this.fsUri,
      dataset: this.fsDataset,
      username: this.fsUsername,
      password: this.fsPassword || '',
      syncOnTocGeneration: this.fsSyncOnTocGeneration,
      syncOnKgFileSave: this.fsSyncOnKgFileSave
    }).subscribe({
      next: r => {
        this.fsSaving = false;
        if (r.dataset) {
          this.fsDataset = r.dataset; // server può aver sanitizzato
        }
        if (this.fsPassword) {
          this.fsHasPassword = true;
          this.fsPassword = '';
        }
      },
      error: err => {
        this.fsSaving = false;
        console.error('Error saving Fuseki settings:', err);
      }
    });
  }
  // ============================================================
  //   Atlassian (Jira/Confluence) — tab logic
  // ============================================================
  loadAtlassianSettings() {
    if (!this.projectId) return;
    this.atlLoading = true;
    this.projectSettingsService.getAtlassianSettings(this.projectId).subscribe({
      next: r => {
        this.atlEnabled = !!r.enabled;
        this.atlBaseUrl = r.jiraBaseUrl || '';
        this.atlProjectKeys = (r.jiraProjectKeys || []).join(', ');
        this.atlConfluenceBaseUrl = r.confluenceBaseUrl || '';
        this.atlConfluenceBaseUrlEffective = r.confluenceBaseUrlEffective || '';
        this.atlConfluenceSpaceKeys = (r.confluenceSpaceKeys || []).join(', ');
        this.atlEmail = r.email || '';
        this.atlHasToken = !!r.hasToken;
        this.atlToken = '';
        if (r.lastTestSuccess === true) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_OK_PREVIOUS');
          this.atlTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL_PREVIOUS');
          this.atlTestSuccess = false;
        }
        this.atlLoading = false;
      },
      error: err => {
        console.error('Error loading Atlassian settings:', err);
        this.atlLoading = false;
      }
    });
  }
  onAtlassianEnabledChange() {
    // Persist immediately when turning off; otherwise wait for explicit Save
    // so the token and shared config travel together.
    if (!this.atlEnabled) {
      this.onSaveAtlassianSettings();
    }
  }
  parseProjectKeys() {
    return (this.atlProjectKeys || '').split(',').map(k => k.trim()).filter(k => k.length > 0);
  }
  parseConfluenceSpaceKeys() {
    return (this.atlConfluenceSpaceKeys || '').split(',').map(k => k.trim()).filter(k => k.length > 0);
  }
  onTestAtlassianConnection() {
    this.atlTesting = true;
    this.atlTestMessage = '';
    this.projectSettingsService.testAtlassianConnection({
      projectId: this.projectId,
      jiraBaseUrl: this.atlBaseUrl,
      email: this.atlEmail,
      apiToken: this.atlToken || (this.atlHasToken ? '********' : '')
    }).subscribe({
      next: r => {
        this.atlTesting = false;
        this.atlTestSuccess = !!r.success;
        if (r.success) {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_OK', {
            name: r.displayName || '',
            ms: r.latencyMs
          });
        } else {
          this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL', {
            error: r.error || 'unknown'
          });
        }
      },
      error: err => {
        this.atlTesting = false;
        this.atlTestSuccess = false;
        this.atlTestMessage = this.translate.instant('PROJECT_SETTINGS.ATLASSIAN_TEST_FAIL', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }
  onSaveAtlassianSettings() {
    this.atlSaving = true;
    this.projectSettingsService.saveAtlassianSettings(this.projectId, {
      enabled: this.atlEnabled,
      jiraBaseUrl: this.atlBaseUrl,
      jiraProjectKeys: this.parseProjectKeys(),
      confluenceBaseUrl: this.atlConfluenceBaseUrl,
      confluenceSpaceKeys: this.parseConfluenceSpaceKeys(),
      email: this.atlEmail,
      apiToken: this.atlToken || ''
    }).subscribe({
      next: () => {
        this.atlSaving = false;
        if (this.atlToken) {
          this.atlHasToken = true;
          this.atlToken = '';
        }
        // Refresh the derived/effective Confluence base URL after save.
        this.loadAtlassianSettings();
      },
      error: err => {
        this.atlSaving = false;
        console.error('Error saving Atlassian settings:', err);
      }
    });
  }
  // ============================================================
  //   Redmine — tab logic
  // ============================================================
  loadRedmineSettings() {
    if (!this.projectId) return;
    this.rmLoading = true;
    this.projectSettingsService.getRedmineSettings(this.projectId).subscribe({
      next: r => {
        this.rmEnabled = !!r.enabled;
        this.rmBaseUrl = r.baseUrl || '';
        this.rmProjects = (r.projects || []).join(', ');
        this.rmTextFormatting = r.textFormatting || '';
        this.rmHasKey = !!r.hasKey;
        this.rmApiKey = '';
        if (r.lastTestSuccess === true) {
          this.rmTestMessage = this.translate.instant('PROJECT_SETTINGS.REDMINE_TEST_OK_PREVIOUS');
          this.rmTestSuccess = true;
        } else if (r.lastTestSuccess === false) {
          this.rmTestMessage = this.translate.instant('PROJECT_SETTINGS.REDMINE_TEST_FAIL_PREVIOUS');
          this.rmTestSuccess = false;
        }
        this.rmLoading = false;
      },
      error: err => {
        this.rmLoading = false;
        this.rmSaveError = err?.error?.error || err?.message || 'http error';
      }
    });
  }
  onRedmineEnabledChange() {
    // Spegnere si salva subito; accendere aspetta «Salva», così chiave e configurazione viaggiano insieme.
    if (!this.rmEnabled) {
      this.onSaveRedmineSettings();
    }
  }
  onTestRedmineConnection() {
    this.rmTesting = true;
    this.rmTestMessage = '';
    this.projectSettingsService.testRedmineConnection({
      projectId: this.projectId,
      baseUrl: this.rmBaseUrl,
      apiKey: this.rmApiKey || (this.rmHasKey ? '********' : '')
    }).subscribe({
      next: r => {
        this.rmTesting = false;
        this.rmTestSuccess = !!r.success;
        this.rmTestMessage = r.success ? this.translate.instant('PROJECT_SETTINGS.REDMINE_TEST_OK', {
          name: r.user?.name || r.user?.login || '',
          ms: r.latencyMs
        }) : this.translate.instant('PROJECT_SETTINGS.REDMINE_TEST_FAIL', {
          error: r.error || 'unknown'
        });
      },
      error: err => {
        this.rmTesting = false;
        this.rmTestSuccess = false;
        this.rmTestMessage = this.translate.instant('PROJECT_SETTINGS.REDMINE_TEST_FAIL', {
          error: err?.error?.error || err?.message || 'http error'
        });
      }
    });
  }
  onSaveRedmineSettings() {
    this.rmSaving = true;
    this.rmSaveError = '';
    this.projectSettingsService.saveRedmineSettings(this.projectId, {
      enabled: this.rmEnabled,
      baseUrl: this.rmBaseUrl,
      projects: (this.rmProjects || '').split(',').map(p => p.trim()).filter(p => p.length > 0),
      textFormatting: this.rmTextFormatting,
      apiKey: this.rmApiKey || ''
    }).subscribe({
      next: () => {
        this.rmSaving = false;
        if (this.rmApiKey) {
          this.rmHasKey = true;
          this.rmApiKey = '';
        }
      },
      error: err => {
        this.rmSaving = false;
        this.rmSaveError = err?.error?.error || err?.message || 'http error';
      }
    });
  }
  static {
    this.ɵfac = function ProjectSettingsComponent_Factory(t) {
      return new (t || ProjectSettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_project_settings_service__WEBPACK_IMPORTED_MODULE_2__.ProjectSettingsService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_3__.CompatibilityModeService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_4__.IdeConfigurationService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_md_explorer_services_external_apps_service__WEBPACK_IMPORTED_MODULE_7__.ExternalAppsService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_8__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__.TranslateService), _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdirectiveInject"](_md_explorer_services_agent_city_state_service__WEBPACK_IMPORTED_MODULE_9__.AgentCityStateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵdefineComponent"]({
      type: ProjectSettingsComponent,
      selectors: [["app-project-settings"]],
      decls: 299,
      vars: 229,
      consts: [["mat-dialog-title", ""], [1, "settings-tabs"], [3, "label"], [1, "tab-content"], [1, "settings-section"], ["class", "setting-item", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "saving-indicator", 4, "ngIf"], [1, "setting-item"], [3, "ngModel", "disabled", "ngModelChange", "change"], [1, "setting-label"], ["diameter", "20", "class", "inline-spinner", 4, "ngIf"], [1, "setting-description"], ["mat-stroked-button", "", "color", "primary", "type", "button", 3, "disabled", "matTooltip", "click", 4, "ngIf"], ["class", "setting-description", 4, "ngIf"], ["class", "template-rows", 4, "ngIf"], ["appearance", "outline", 2, "width", "100%"], ["matInput", "", "placeholder", "https://github.com/\u2026/mdexplorer-template-ontologie.git", 3, "ngModel", "disabled", "ngModelChange"], ["appearance", "outline", 1, "template-language"], [3, "ngModel", "disabled", "ngModelChange"], ["value", "it"], ["value", "en"], [1, "template-add"], ["mat-stroked-button", "", "color", "primary", "type", "button", 3, "disabled", "click"], ["class", "setting-description setting-error", 4, "ngIf"], [1, "settings-section", "info-card"], ["class", "harness-mismatch", 4, "ngIf"], ["value", "copilot", 1, "ide-radio"], [1, "ide-label"], [1, "ide-icon-inline", "copilot-color"], [1, "harness-reason"], ["value", "opencode", 1, "ide-radio"], [1, "ide-icon-inline"], ["value", "claude", 1, "ide-radio"], [1, "ide-icon-inline", "claude-color"], ["value", "none", 1, "ide-radio"], [4, "ngIf"], [1, "setting-subtitle"], [1, "advanced-engine-body"], ["for", "markAgentEngineGroup", 1, "setting-label"], ["id", "markAgentEngineGroup", 3, "ngModel", "disabled", "ngModelChange", "change"], ["value", "linked", 1, "ide-radio"], ["class", "mcp-group-row", 4, "ngFor", "ngForOf"], ["class", "setting-description mcp-groups-total", 4, "ngIf"], ["class", "rag-status", 4, "ngIf"], ["class", "settings-section info-card", 4, "ngIf"], ["class", "tab-content", 4, "ngIf"], ["class", "settings-section", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "loading-container"], ["diameter", "30"], [1, "saving-indicator"], ["diameter", "20"], ["mat-stroked-button", "", 3, "disabled", "click"], ["diameter", "18", "class", "inline-spinner", 4, "ngIf"], ["diameter", "18", 1, "inline-spinner"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "rows", "2", 3, "ngModel", "placeholder", "disabled", "ngModelChange", "blur"], ["diameter", "20", 1, "inline-spinner"], ["mat-stroked-button", "", "color", "primary", "type", "button", 3, "disabled", "matTooltip", "click"], [4, "ngFor", "ngForOf"], ["matInput", "", "placeholder", "docs/ownership.md", 3, "ngModel", "disabled", "ngModelChange", "blur"], ["class", "workflow-doc-row", 4, "ngFor", "ngForOf", "ngForTrackBy"], ["mat-stroked-button", "", "type", "button", 3, "disabled", "click"], [1, "workflow-doc-row"], ["appearance", "outline", 1, "workflow-doc-field"], ["matInput", "", "placeholder", "docs/workflow.md", 3, "ngModel", "disabled", "ngModelChange", "blur"], ["mat-icon-button", "", "type", "button", 3, "disabled", "title", "click"], [1, "setting-description", "setting-error"], ["class", "setting-inline", 4, "ngIf"], [1, "setting-inline"], ["for", "agentWorktreeSlots", 1, "setting-label"], ["appearance", "outline", 1, "slots-field"], ["matInput", "", "id", "agentWorktreeSlots", "type", "number", "min", "1", 3, "ngModel", "disabled", "max", "ngModelChange", "change"], ["matInput", "", "placeholder", "wss://errantia.net/mdchat", 3, "ngModel", "disabled", "ngModelChange", "blur"], ["matInput", "", "type", "password", "autocomplete", "off", 3, "ngModel", "disabled", "placeholder", "ngModelChange"], [1, "setting-actions", 2, "display", "flex", "gap", "8px", "flex-wrap", "wrap"], ["mat-stroked-button", "", "color", "primary", 3, "disabled", "click"], ["mat-stroked-button", "", "color", "warn", 3, "disabled", "click", 4, "ngIf"], ["mat-stroked-button", "", "color", "warn", 3, "disabled", "click"], [1, "template-rows"], [1, "template-url"], ["mat-button", "", "color", "primary", "type", "button", 3, "disabled", "matTooltip", "click", 4, "ngIf"], ["mat-button", "", "color", "warn", "type", "button", 3, "disabled", "click", 4, "ngIf"], ["mat-button", "", "color", "primary", "type", "button", 3, "disabled", "matTooltip", "click"], ["mat-button", "", "color", "warn", "type", "button", 3, "disabled", "click"], ["value", "vscode", 1, "ide-radio"], [1, "ide-icon-inline", "vscode-color"], ["value", "intellij", 1, "ide-radio"], [1, "ide-icon-inline", "intellij-color"], ["value", "agent-cli", 1, "ide-radio", 3, "disabled"], ["class", "harness-reason", 4, "ngIf"], [1, "inline-warn-icon"], [1, "harness-mismatch"], [1, "harness-mismatch-icon"], [1, "harness-mismatch-text"], ["mat-stroked-button", "", "color", "primary", 1, "harness-mismatch-action", 3, "disabled", "click"], ["for", "harnessModelInput", 1, "setting-label"], [1, "harness-model-row"], ["appearance", "outline", 1, "harness-model-field"], ["matInput", "", "id", "harnessModelInput", "autocomplete", "off", 3, "ngModel", "disabled", "placeholder", "ngModelChange"], [1, "mcp-group-row"], [1, "mcp-group-weight"], [1, "setting-description", "mcp-group-summary"], [1, "setting-description", "mcp-groups-total"], [1, "rag-status"], [1, "status-row"], [1, "status-label"], ["class", "status-row", 4, "ngIf"], [1, "rag-actions"], ["class", "rag-progress", 4, "ngIf"], ["class", "rag-message", 4, "ngIf"], [1, "rag-progress"], ["mode", "determinate", 3, "value"], ["class", "rag-progress-text", 4, "ngIf"], [1, "rag-progress-text"], [1, "rag-message"], [1, "apps-toolbar"], [1, "toolbar-spacer"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["class", "saved-message", 4, "ngIf"], ["class", "apps-empty-state", 4, "ngIf"], ["class", "apps-tree-editor", 4, "ngIf"], [1, "saved-message"], [1, "apps-empty-state"], [1, "apps-empty-hint"], [1, "apps-tree-editor"], ["class", "tree-category", 4, "ngIf"], ["class", "tree-row app-row", 4, "ngIf"], [1, "tree-category"], [1, "tree-row", "category-row"], [1, "gold-icon"], ["class", "category-name", 4, "ngIf"], ["class", "category-edit-input", "autofocus", "", 3, "ngModel", "ngModelChange", "keydown.enter", "keydown.escape", "blur", 4, "ngIf"], [1, "tree-actions"], ["mat-icon-button", "", 3, "matTooltip", "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", 3, "matTooltip", "click"], ["class", "tree-row app-row indented", 4, "ngFor", "ngForOf"], [1, "category-name"], ["autofocus", "", 1, "category-edit-input", 3, "ngModel", "ngModelChange", "keydown.enter", "keydown.escape", "blur"], ["mat-icon-button", "", 3, "matTooltip", "click"], [1, "tree-row", "app-row", "indented"], [1, "app-row-icon"], [1, "app-row-name"], [1, "tree-row", "app-row"], ["mat-icon-button", "", 3, "matMenuTriggerFor", "matTooltip", 4, "ngIf"], ["moveToCatMenu", "matMenu"], ["mat-menu-item", "", 3, "click", 4, "ngFor", "ngForOf"], ["mat-icon-button", "", 3, "matMenuTriggerFor", "matTooltip"], ["mat-menu-item", "", 3, "click"], [1, "kg-field-group"], [1, "kg-field-label"], ["appearance", "outline", 1, "kg-field"], ["matInput", "", "placeholder", "bolt://localhost:7687", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "neo4j", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "type", "password", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], [1, "kg-row", "kg-actions"], ["class", "kg-status-ok", 4, "ngIf"], ["class", "kg-status-fail", 4, "ngIf"], [1, "kg-row"], [1, "kg-status-ok"], [1, "kg-status-fail"], ["class", "kg-state", 4, "ngIf"], [1, "kg-actions"], ["class", "kg-message", 4, "ngIf"], ["class", "kg-errors", 4, "ngIf"], [1, "kg-state"], ["class", "kg-namespaces", 4, "ngIf"], [1, "kg-namespaces"], ["class", "status-row", 4, "ngFor", "ngForOf"], [1, "kg-message"], [1, "kg-errors"], ["class", "kg-error-row", 4, "ngFor", "ngForOf"], [1, "kg-error-row"], ["matInput", "", "placeholder", "http://localhost:3030", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", 3, "ngModel", "placeholder", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "", 3, "ngModel", "disabled", "ngModelChange"], ["mat-stroked-button", "", "color", "accent", 3, "disabled", "click"], ["matInput", "", "placeholder", "https://acme.atlassian.net", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "BCO, OFELIA", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "DEV, ARCH", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "you@company.com", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "https://redmine.example.com", 3, "ngModel", "disabled", "ngModelChange"], ["matInput", "", "placeholder", "sviluppo, assistenza", 3, "ngModel", "disabled", "ngModelChange"], ["value", ""], ["value", "markdown"], ["value", "textile"]],
      template: function ProjectSettingsComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](3, "mat-dialog-content")(4, "mat-tab-group", 1)(5, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](6, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](7, "div", 3)(8, "mat-card", 4)(9, "mat-card-header")(10, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](12, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](13, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](14, ProjectSettingsComponent_div_14_Template, 13, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](15, ProjectSettingsComponent_div_15_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](16, ProjectSettingsComponent_div_16_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](17, "mat-card", 4)(18, "mat-card-header")(19, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](22, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](23, ProjectSettingsComponent_div_23_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](24, "mat-card", 4)(25, "mat-card-header")(26, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](27);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](28, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](29, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](30, ProjectSettingsComponent_div_30_Template, 13, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](31, ProjectSettingsComponent_div_31_Template, 10, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](32, "mat-card", 4)(33, "mat-card-header")(34, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](35);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](36, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](37, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](38, ProjectSettingsComponent_div_38_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](39, ProjectSettingsComponent_div_39_Template, 9, 9, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](40, ProjectSettingsComponent_div_40_Template, 10, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](41, "mat-card", 4)(42, "mat-card-header")(43, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](44);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](45, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](46, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](47, ProjectSettingsComponent_div_47_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](48, ProjectSettingsComponent_div_48_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](49, "mat-card", 4)(50, "mat-card-header")(51, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](52);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](53, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](54, "mat-card-content")(55, "div", 8)(56, "mat-checkbox", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_mat_checkbox_ngModelChange_56_listener($event) {
            return ctx.agentCityEnabled = $event;
          })("change", function ProjectSettingsComponent_Template_mat_checkbox_change_56_listener() {
            return ctx.onAgentCityChange();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](57, "span", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](58);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](59, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](60, ProjectSettingsComponent_mat_spinner_60_Template, 1, 0, "mat-spinner", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](61, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](62);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](63, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](64, ProjectSettingsComponent_button_64_Template, 6, 7, "button", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](65, ProjectSettingsComponent_ul_65_Template, 2, 1, "ul", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](66, ProjectSettingsComponent_div_66_Template, 23, 22, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](67, ProjectSettingsComponent_div_67_Template, 10, 10, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](68, ProjectSettingsComponent_div_68_Template, 34, 32, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](69, "mat-card", 4)(70, "mat-card-header")(71, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](72);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](73, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](74, "mat-card-content")(75, "div", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](76, ProjectSettingsComponent_p_76_Template, 3, 3, "p", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](77, ProjectSettingsComponent_ul_77_Template, 2, 1, "ul", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](78, "label", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](79);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](80, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](81, "mat-form-field", 16)(82, "input", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_input_ngModelChange_82_listener($event) {
            return ctx.newMethodUrl = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](83, "label", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](84);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](85, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](86, "mat-form-field", 18)(87, "mat-select", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_mat_select_ngModelChange_87_listener($event) {
            return ctx.newMethodLanguage = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](88, "mat-option", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](89, "italiano");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](90, "mat-option", 21);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](91, "English");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](92, "mat-checkbox", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_mat_checkbox_ngModelChange_92_listener($event) {
            return ctx.newMethodExample = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](93);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](94, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](95, "div", 22)(96, "button", 23);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_Template_button_click_96_listener() {
            return ctx.addMethod();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](97, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](98, "library_add");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](99);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](100, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](101, ProjectSettingsComponent_mat_spinner_101_Template, 1, 0, "mat-spinner", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](102, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](103);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](104, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](105, ProjectSettingsComponent_p_105_Template, 2, 1, "p", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](106, ProjectSettingsComponent_ul_106_Template, 2, 1, "ul", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](107, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](108, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](109, "div", 3)(110, "mat-card", 4)(111, "mat-card-header")(112, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](113);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](114, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](115, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](116, ProjectSettingsComponent_div_116_Template, 24, 16, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](117, ProjectSettingsComponent_div_117_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](118, ProjectSettingsComponent_div_118_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](119, "mat-card", 25)(120, "mat-card-content")(121, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](122, "info");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](123, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](124);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](125, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](126, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](127, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](128, "div", 3)(129, "mat-card", 4)(130, "mat-card-header")(131, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](132);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](133, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](134, "mat-card-content")(135, "div", 8)(136, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](137);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](138, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](139, ProjectSettingsComponent_div_139_Template, 5, 3, "div", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](140, ProjectSettingsComponent_div_140_Template, 11, 15, "div", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](141, "mat-radio-group", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_mat_radio_group_ngModelChange_141_listener($event) {
            return ctx.harness = $event;
          })("change", function ProjectSettingsComponent_Template_mat_radio_group_change_141_listener() {
            return ctx.onHarnessChange();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](142, "mat-radio-button", 27)(143, "span", 28)(144, "mat-icon", 29);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](145, "smart_toy");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](146);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](147, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](148, "span", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](149);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](150, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](151, "mat-radio-button", 31)(152, "span", 28)(153, "mat-icon", 32);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](154, "terminal");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](155);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](156, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](157, "span", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](158);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](159, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](160, "mat-radio-button", 33)(161, "span", 28)(162, "mat-icon", 34);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](163, "psychology");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](164);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](165, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](166, "span", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](167);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](168, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](169, "mat-radio-button", 35)(170, "span", 28)(171, "mat-icon", 32);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](172, "block");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](173);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](174, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](175, "span", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](176);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](177, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](178, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](179);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](180, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](181, ProjectSettingsComponent_ng_container_181_Template, 16, 15, "ng-container", 36);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](182, "div", 8)(183, "h4", 37);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](184);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](185, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](186, "div", 38)(187, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](188);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](189, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](190, "label", 39);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](191);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](192, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](193, "mat-radio-group", 40);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_Template_mat_radio_group_ngModelChange_193_listener($event) {
            return ctx.engineChoice = $event;
          })("change", function ProjectSettingsComponent_Template_mat_radio_group_change_193_listener() {
            return ctx.onEngineChange();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](194, "mat-radio-button", 41)(195, "span", 28)(196, "mat-icon", 32);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](197, "link");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](198);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](199, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](200, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](201, "mat-radio-button", 27)(202, "span", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](203);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](204, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](205, "mat-radio-button", 31)(206, "span", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](207);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](208, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](209, "mat-radio-button", 33)(210, "span", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](211);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](212, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](213, "mat-radio-button", 35)(214, "span", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](215);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](216, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](217, ProjectSettingsComponent_div_217_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](218, "mat-card", 4)(219, "mat-card-header")(220, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](221);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](222, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](223, "mat-card-content")(224, "div", 8)(225, "p", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](226);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](227, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](228, ProjectSettingsComponent_div_228_Template, 5, 3, "div", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](229, ProjectSettingsComponent_div_229_Template, 5, 1, "div", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](230, ProjectSettingsComponent_div_230_Template, 11, 15, "div", 42);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](231, ProjectSettingsComponent_p_231_Template, 6, 10, "p", 43);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](232, ProjectSettingsComponent_div_232_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](233, "mat-card", 4)(234, "mat-card-header")(235, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](236);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](237, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](238, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](239, ProjectSettingsComponent_div_239_Template, 12, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](240, ProjectSettingsComponent_div_240_Template, 11, 14, "div", 44);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](241, ProjectSettingsComponent_div_241_Template, 16, 15, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](242, ProjectSettingsComponent_div_242_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](243, ProjectSettingsComponent_div_243_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](244, ProjectSettingsComponent_mat_card_244_Template, 7, 3, "mat-card", 45);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](245, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](246, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](247, ProjectSettingsComponent_div_247_Template, 22, 16, "div", 46);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](248, ProjectSettingsComponent_div_248_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](249, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](250, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](251, "div", 3)(252, "mat-card", 4)(253, "mat-card-header")(254, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](255);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](256, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](257, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](258, ProjectSettingsComponent_div_258_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](259, ProjectSettingsComponent_div_259_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](260, ProjectSettingsComponent_mat_card_260_Template, 57, 52, "mat-card", 47);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](261, ProjectSettingsComponent_mat_card_261_Template, 22, 22, "mat-card", 47);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](262, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](263, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](264, "div", 3)(265, "mat-card", 4)(266, "mat-card-header")(267, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](268);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](269, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](270, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](271, ProjectSettingsComponent_div_271_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](272, ProjectSettingsComponent_div_272_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](273, ProjectSettingsComponent_mat_card_273_Template, 69, 64, "mat-card", 47);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](274, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](275, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](276, "div", 3)(277, "mat-card", 4)(278, "mat-card-header")(279, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](280);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](281, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](282, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](283, ProjectSettingsComponent_div_283_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](284, ProjectSettingsComponent_div_284_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](285, ProjectSettingsComponent_mat_card_285_Template, 77, 71, "mat-card", 47);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](286, "mat-card", 4)(287, "mat-card-header")(288, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](289);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](290, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](291, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](292, ProjectSettingsComponent_div_292_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](293, ProjectSettingsComponent_div_293_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtemplate"](294, ProjectSettingsComponent_mat_card_294_Template, 66, 57, "mat-card", 47);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementStart"](295, "mat-dialog-actions", 48)(296, "button", 49);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵlistener"]("click", function ProjectSettingsComponent_Template_button_click_296_listener() {
            return ctx.close();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtext"](297);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipe"](298, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](2, 119, "PROJECT_SETTINGS.TITLE", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction1"](225, _c7, ctx.projectName)));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](6, 122, "PROJECT_SETTINGS.MARKDOWN_SETTINGS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](12, 124, "PROJECT_SETTINGS.VALIDATION_RULES"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](21, 126, "PROJECT_SETTINGS.GITHUB_COMPAT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](28, 128, "PROJECT_SETTINGS.LINK_INDEXING"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](36, 130, "PROJECT_SETTINGS.TEXT_INDEXING"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading && ctx.indexAllTextFilesEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading && ctx.indexAllTextFilesEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](45, 132, "PROJECT_SETTINGS.VIEW_OPTIONS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](53, 134, "PROJECT_SETTINGS.AGENT_CITY_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.agentCityEnabled)("disabled", ctx.saving || ctx.agentCityBusy);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](59, 136, "PROJECT_SETTINGS.AGENT_CITY_ENABLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityBusy);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](63, 138, "PROJECT_SETTINGS.AGENT_CITY_ENABLE_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityEnabled && ctx.agentCityGuideMissing);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityNotes.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.agentCityEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](73, 140, "PROJECT_SETTINGS.TEMPLATES_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.projectTemplates.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.projectTemplates.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](80, 142, "PROJECT_SETTINGS.TEMPLATE_ADD_URL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.newMethodUrl)("disabled", ctx.addingMethod);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](85, 144, "PROJECT_SETTINGS.TEMPLATE_ADD_LANGUAGE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.newMethodLanguage)("disabled", ctx.addingMethod);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.newMethodExample)("disabled", ctx.addingMethod);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](94, 146, "PROJECT_SETTINGS.TEMPLATE_ADD_EXAMPLE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("disabled", ctx.addingMethod || !(ctx.newMethodUrl == null ? null : ctx.newMethodUrl.trim()));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](100, 148, "PROJECT_SETTINGS.TEMPLATE_ADD"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.addingMethod);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](104, 150, "PROJECT_SETTINGS.TEMPLATE_ADD_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.methodError);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.methodNotes.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](108, 152, "PROJECT_SETTINGS.IDE_SETTINGS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](114, 154, "PROJECT_SETTINGS.IDE_CONFIGURATION"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](125, 156, "PROJECT_SETTINGS.IDE_INFO"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](127, 158, "PROJECT_SETTINGS.AI_RAG"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](133, 160, "PROJECT_SETTINGS.AI_ENV_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](138, 162, "PROJECT_SETTINGS.AI_ENV_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.harnessDeclared);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.engineDecoupled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.harness)("disabled", ctx.savingHarness);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](147, 164, "PROJECT_SETTINGS.HARNESS_COPILOT"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](150, 166, "PROJECT_SETTINGS.AI_ENV_HINT_COPILOT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](156, 168, "PROJECT_SETTINGS.HARNESS_OPENCODE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](159, 170, "PROJECT_SETTINGS.AI_ENV_HINT_OPENCODE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](165, 172, "PROJECT_SETTINGS.HARNESS_CLAUDE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](168, 174, "PROJECT_SETTINGS.AI_ENV_HINT_CLAUDE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](174, 176, "PROJECT_SETTINGS.HARNESS_NONE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](177, 178, "PROJECT_SETTINGS.AI_ENV_HINT_NONE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](180, 180, "PROJECT_SETTINGS.HARNESS_SWITCH_NOTE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.harness !== "none");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](185, 182, "PROJECT_SETTINGS.ENGINE_ADVANCED"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](189, 184, "PROJECT_SETTINGS.ENGINE_ADVANCED_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](192, 186, "PROJECT_SETTINGS.ENGINE_LABEL"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngModel", ctx.engineChoice)("disabled", ctx.savingEngine || !ctx.engineSettingsLoaded);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind2"](199, 188, "PROJECT_SETTINGS.ENGINE_FOLLOWS", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpureFunction1"](227, _c2, _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](200, 191, ctx.engineLabelKey(ctx.harness)))), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](204, 193, "PROJECT_SETTINGS.ENGINE_COPILOT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](208, 195, "PROJECT_SETTINGS.ENGINE_OPENCODE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](212, 197, "PROJECT_SETTINGS.ENGINE_CLAUDE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](216, 199, "PROJECT_SETTINGS.ENGINE_NONE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.savingHarness || ctx.savingEngine);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](222, 201, "PROJECT_SETTINGS.MCP_GROUPS_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](227, 203, "PROJECT_SETTINGS.MCP_GROUPS_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.mcpGroupsLoaded && !ctx.mcpGroupsError && !ctx.mcpGroupsChosen);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.mcpGroupsError);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngForOf", ctx.mcpGroups);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.mcpGroupsLoaded && !ctx.mcpGroupsError);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.savingMcpGroups);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](237, 205, "PROJECT_SETTINGS.RAG_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.loading && ctx.ragEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.ragModelInstalled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](246, 207, "PROJECT_SETTINGS.APPS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.appsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.appsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](250, 209, "PROJECT_SETTINGS.KG_TAB"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](256, 211, "PROJECT_SETTINGS.KG_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.kgLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.kgLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.kgLoading && ctx.kgEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.kgLoading && ctx.kgEnabled && ctx.kgHasPassword);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](263, 213, "PROJECT_SETTINGS.FUSEKI_TAB"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](269, 215, "PROJECT_SETTINGS.FUSEKI_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.fsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.fsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.fsLoading && ctx.fsEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](275, 217, "PROJECT_SETTINGS.ATLASSIAN_TAB"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](281, 219, "PROJECT_SETTINGS.ATLASSIAN_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.atlLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.atlLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.atlLoading && ctx.atlEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](290, 221, "PROJECT_SETTINGS.REDMINE_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.rmLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", ctx.rmLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵproperty"]("ngIf", !ctx.rmLoading && ctx.rmEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_10__["ɵɵpipeBind1"](298, 223, "COMMON.CLOSE"));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_13__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_13__.NgIf, _angular_material_legacy_core__WEBPACK_IMPORTED_MODULE_14__.MatLegacyOption, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_15__.MatLegacyCheckbox, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_16__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_16__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_16__.MatLegacyLabel, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_17__.MatLegacyInput, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_18__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_18__.MatLegacyRadioButton, _angular_material_legacy_select__WEBPACK_IMPORTED_MODULE_19__.MatLegacySelect, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_20__.MatLegacyMenu, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_20__.MatLegacyMenuItem, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_20__.MatLegacyMenuTrigger, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_21__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_21__.MatLegacyCardHeader, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_21__.MatLegacyCardContent, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_21__.MatLegacyCardTitle, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_22__.MatLegacyTabGroup, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_22__.MatLegacyTab, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_23__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_24__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_25__.MatLegacyProgressSpinner, _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_26__.MatLegacyProgressBar, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_11__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_27__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.NumberValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.MinValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.MaxValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_28__.NgModel, _angular_common__WEBPACK_IMPORTED_MODULE_13__.UpperCasePipe, _angular_common__WEBPACK_IMPORTED_MODULE_13__.DecimalPipe, _angular_common__WEBPACK_IMPORTED_MODULE_13__.DatePipe, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__.TranslatePipe],
      styles: ["@charset \"UTF-8\";\n.settings-tabs[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 20px 16px;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n\n.setting-item[_ngcontent-%COMP%] {\n  margin: 16px 0;\n}\n\n.setting-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  margin-left: 8px;\n}\n\n.setting-description[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-text-secondary);\n  font-size: 14px;\n}\n\n.setting-error[_ngcontent-%COMP%] {\n  color: var(--mde-error, #c62828);\n}\n\n\n.template-rows[_ngcontent-%COMP%] {\n  margin: 0 0 12px 32px;\n  padding: 0;\n}\n.template-rows[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 4px;\n}\n\n.template-url[_ngcontent-%COMP%] {\n  display: block;\n  color: var(--mde-text-secondary);\n  font-size: 12px;\n  overflow-wrap: anywhere;\n}\n\n.template-language[_ngcontent-%COMP%] {\n  display: block;\n  width: 200px;\n}\n\n.template-add[_ngcontent-%COMP%] {\n  margin-top: 8px;\n}\n\n\n.workflow-doc-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-start;\n  gap: 4px;\n}\n\n.workflow-doc-field[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n\n\n.inline-warn-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  height: 16px;\n  width: 16px;\n  vertical-align: text-bottom;\n  margin-right: 4px;\n  color: var(--mde-text-secondary);\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 20px;\n  gap: 12px;\n}\n\n.saving-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-text-secondary);\n  font-size: 14px;\n}\n\n.info-card[_ngcontent-%COMP%] {\n  background-color: var(--mde-bg-tertiary);\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: start;\n  gap: 12px;\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-accent-primary);\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 14px;\n  color: var(--mde-text-secondary);\n}\n\n.ide-radio[_ngcontent-%COMP%] {\n  display: block;\n  margin: 12px 0;\n}\n\n.ide-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n.ide-icon-inline[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.ide-icon-inline.vscode-color[_ngcontent-%COMP%] {\n  color: #007ACC;\n}\n.ide-icon-inline.intellij-color[_ngcontent-%COMP%] {\n  color: #FF6B00;\n}\n.ide-icon-inline.copilot-color[_ngcontent-%COMP%] {\n  color: #6E40C9;\n}\n.ide-icon-inline.claude-color[_ngcontent-%COMP%] {\n  color: #D97757;\n}\n\n\n.harness-reason[_ngcontent-%COMP%] {\n  display: block;\n  margin-left: 28px;\n  font-size: 12px;\n  color: var(--mde-text-secondary);\n  white-space: normal;\n}\n\n\n.harness-mismatch[_ngcontent-%COMP%] {\n  display: flex;\n  flex-wrap: wrap;\n  align-items: center;\n  gap: 8px 12px;\n  margin: 12px 0;\n  padding: 10px 12px;\n  border-radius: 6px;\n  background-color: var(--mde-warning-bg);\n  color: var(--mde-warning-fg);\n  font-size: 14px;\n}\n.harness-mismatch[_ngcontent-%COMP%]   .harness-mismatch-icon[_ngcontent-%COMP%] {\n  flex: 0 0 auto;\n}\n.harness-mismatch[_ngcontent-%COMP%]   .harness-mismatch-text[_ngcontent-%COMP%] {\n  flex: 1 1 240px;\n}\n.harness-mismatch[_ngcontent-%COMP%]   .harness-mismatch-action[_ngcontent-%COMP%] {\n  flex: 0 0 auto;\n}\n\n\n.advanced-engine-body[_ngcontent-%COMP%] {\n  margin-left: 8px;\n  padding-left: 12px;\n  border-left: 2px solid var(--mde-border-color);\n}\n\n.rag-status[_ngcontent-%COMP%] {\n  margin: 16px 0 16px 32px;\n  font-size: 14px;\n}\n\n.status-row[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  margin: 4px 0;\n}\n\n.status-label[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n}\n\n.status-ok[_ngcontent-%COMP%] {\n  color: #4caf50;\n  font-weight: 500;\n}\n\n.status-missing[_ngcontent-%COMP%] {\n  color: #f44336;\n  font-weight: 500;\n}\n\n.rag-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  align-items: center;\n}\n\n.rag-message[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-accent-primary);\n  font-size: 14px;\n  font-weight: 500;\n}\n\n.inline-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  margin-right: 4px;\n}\n\n.rag-progress[_ngcontent-%COMP%] {\n  margin: 12px 0 0 32px;\n}\n.rag-progress[_ngcontent-%COMP%]   mat-progress-bar[_ngcontent-%COMP%] {\n  margin-bottom: 6px;\n}\n\n.rag-progress-text[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: var(--mde-text-secondary);\n}\n\n.apps-toolbar[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  align-items: center;\n  margin-bottom: 16px;\n  flex-wrap: wrap;\n}\n\n.toolbar-spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.saved-message[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: #4caf50;\n  font-weight: 500;\n}\n\n.apps-empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 32px 16px;\n  color: var(--mde-text-hint);\n}\n.apps-empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  margin-bottom: 8px;\n}\n.apps-empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n}\n\n.apps-empty-hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  margin-top: 4px !important;\n}\n\n.apps-tree-editor[_ngcontent-%COMP%] {\n  border: 1px solid var(--mde-border-color);\n  border-radius: 4px;\n  overflow: hidden;\n}\n\n.tree-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 6px 12px;\n  border-bottom: 1px solid var(--mde-border-color);\n  min-height: 40px;\n}\n.tree-row[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n\n.category-row[_ngcontent-%COMP%] {\n  background: var(--mde-bg-tertiary);\n  font-weight: 500;\n}\n\n.category-name[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n}\n\n.category-edit-input[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n  border: 1px solid var(--mde-accent-primary);\n  border-radius: 3px;\n  padding: 4px 8px;\n  font-size: 14px;\n  outline: none;\n  background: var(--mde-bg-primary);\n  color: var(--mde-text-primary);\n}\n\n.app-row.indented[_ngcontent-%COMP%] {\n  padding-left: 40px;\n}\n\n.app-row-icon[_ngcontent-%COMP%] {\n  color: var(--mde-accent-primary);\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  flex-shrink: 0;\n}\n\n.app-row-name[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n  font-size: 14px;\n}\n\n.tree-actions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  margin-left: auto;\n}\n.tree-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  width: 32px;\n  height: 32px;\n  line-height: 32px;\n}\n.tree-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.kg-field-group[_ngcontent-%COMP%] {\n  margin: 14px 0;\n}\n\n.kg-field-label[_ngcontent-%COMP%] {\n  display: block;\n  font-size: 13px;\n  font-weight: 600;\n  margin-bottom: 6px;\n  color: var(--mde-text-primary);\n  letter-spacing: 0.2px;\n}\n\n.kg-field[_ngcontent-%COMP%] {\n  display: block;\n  width: 100%;\n  margin: 0;\n}\n\n.kg-row[_ngcontent-%COMP%] {\n  margin: 12px 0;\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  flex-wrap: wrap;\n}\n.kg-row.kg-actions[_ngcontent-%COMP%] {\n  margin-top: 16px;\n}\n\n.kg-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  align-items: center;\n  flex-wrap: wrap;\n  margin: 16px 0;\n}\n\n.kg-status-ok[_ngcontent-%COMP%] {\n  color: #4caf50;\n  font-weight: 500;\n  font-size: 13px;\n}\n\n.kg-status-fail[_ngcontent-%COMP%] {\n  color: #f44336;\n  font-weight: 500;\n  font-size: 13px;\n}\n\n.kg-state[_ngcontent-%COMP%] {\n  margin: 8px 0 16px 0;\n  font-size: 14px;\n}\n\n.kg-namespaces[_ngcontent-%COMP%] {\n  margin-top: 12px;\n}\n.kg-namespaces[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 8px 0 6px;\n  font-size: 13px;\n  color: var(--mde-text-secondary);\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.4px;\n}\n\n.kg-message[_ngcontent-%COMP%] {\n  margin: 12px 0;\n  color: var(--mde-accent-primary);\n  font-size: 14px;\n  font-weight: 500;\n}\n\n.kg-errors[_ngcontent-%COMP%] {\n  margin-top: 16px;\n}\n.kg-errors[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 8px 0;\n  color: #f44336;\n  font-size: 14px;\n}\n\n.kg-error-row[_ngcontent-%COMP%] {\n  background: var(--mde-bg-tertiary);\n  padding: 10px 12px;\n  border-radius: 4px;\n  margin: 8px 0;\n  font-size: 13px;\n  border-left: 3px solid #f44336;\n}\n.kg-error-row[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  display: block;\n  margin-bottom: 4px;\n  color: var(--mde-text-primary);\n}\n.kg-error-row[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%] {\n  margin: 6px 0 0;\n  padding-left: 20px;\n  color: var(--mde-text-secondary);\n}\n\n\n.setting-inline[_ngcontent-%COMP%] {\n  margin-top: 10px;\n}\n.setting-inline[_ngcontent-%COMP%]   .slots-field[_ngcontent-%COMP%] {\n  width: 90px;\n  margin-left: 8px;\n}\n\n\n.mcp-group-row[_ngcontent-%COMP%] {\n  margin-bottom: 12px;\n}\n.mcp-group-row[_ngcontent-%COMP%]   .mcp-group-weight[_ngcontent-%COMP%] {\n  margin-left: 8px;\n  font-size: 12px;\n  opacity: 0.7;\n  white-space: nowrap;\n}\n.mcp-group-row[_ngcontent-%COMP%]   .mcp-group-summary[_ngcontent-%COMP%] {\n  margin: 2px 0 0 32px;\n}\n\n.mcp-groups-total[_ngcontent-%COMP%] {\n  margin-top: 16px;\n  font-weight: 500;\n}\n\n.harness-model-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  flex-wrap: wrap;\n}\n.harness-model-row[_ngcontent-%COMP%]   .harness-model-field[_ngcontent-%COMP%] {\n  flex: 1 1 220px;\n}\n\n.setting-subtitle[_ngcontent-%COMP%] {\n  margin: 16px 0 4px;\n  font-size: 14px;\n  font-weight: 500;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvcHJvamVjdC1zZXR0aW5ncy9wcm9qZWN0LXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBLGdCQUFnQjtBQUNoQjtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGtCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxtQkFBQTtBQUNGOztBQUVBO0VBQ0UsY0FBQTtBQUNGOztBQUVBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtBQUNGOztBQUVBO0VBQ0UsZUFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxnQ0FBQTtBQUNGOztBQUVBLDhDQUFBO0FBQ0E7RUFDRSxxQkFBQTtFQUNBLFVBQUE7QUFDRjtBQUFFO0VBQUssa0JBQUE7QUFHUDs7QUFBQTtFQUNFLGNBQUE7RUFDQSxnQ0FBQTtFQUNBLGVBQUE7RUFDQSx1QkFBQTtBQUdGOztBQUFBO0VBQ0UsY0FBQTtFQUNBLFlBQUE7QUFHRjs7QUFBQTtFQUNFLGVBQUE7QUFHRjs7QUFBQSxxRUFBQTtBQUNBO0VBQ0UsYUFBQTtFQUNBLHVCQUFBO0VBQ0EsUUFBQTtBQUdGOztBQUFBO0VBQ0UsT0FBQTtFQUNBLFlBQUE7QUFHRjs7QUFBQSw4RUFBQTtBQUNBO0VBQ0UsZUFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsMkJBQUE7RUFDQSxpQkFBQTtFQUNBLGdDQUFBO0FBR0Y7O0FBQUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7RUFDQSxTQUFBO0FBR0Y7O0FBQUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZUFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxlQUFBO0FBR0Y7O0FBQUE7RUFDRSx3Q0FBQTtBQUdGO0FBREU7RUFDRSxhQUFBO0VBQ0Esa0JBQUE7RUFDQSxTQUFBO0FBR0o7QUFESTtFQUNFLGdDQUFBO0FBR047QUFBSTtFQUNFLFNBQUE7RUFDQSxlQUFBO0VBQ0EsZ0NBQUE7QUFFTjs7QUFHQTtFQUNFLGNBQUE7RUFDQSxjQUFBO0FBQUY7O0FBR0E7RUFDRSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFBRjs7QUFHQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQUFGO0FBRUU7RUFDRSxjQUFBO0FBQUo7QUFHRTtFQUNFLGNBQUE7QUFESjtBQUlFO0VBQ0UsY0FBQTtBQUZKO0FBS0U7RUFDRSxjQUFBO0FBSEo7O0FBT0Esa0ZBQUE7QUFDQTtFQUNFLGNBQUE7RUFDQSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxnQ0FBQTtFQUNBLG1CQUFBO0FBSkY7O0FBT0EsdUVBQUE7QUFDQTtFQUNFLGFBQUE7RUFDQSxlQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSx1Q0FBQTtFQUNBLDRCQUFBO0VBQ0EsZUFBQTtBQUpGO0FBTUU7RUFDRSxjQUFBO0FBSko7QUFPRTtFQUNFLGVBQUE7QUFMSjtBQVFFO0VBQ0UsY0FBQTtBQU5KOztBQVVBLHdGQUFBO0FBQ0E7RUFDRSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsOENBQUE7QUFQRjs7QUFhQTtFQUNFLHdCQUFBO0VBQ0EsZUFBQTtBQVZGOztBQWFBO0VBQ0UsYUFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0FBVkY7O0FBYUE7RUFDRSxnQ0FBQTtBQVZGOztBQWFBO0VBQ0UsY0FBQTtFQUNBLGdCQUFBO0FBVkY7O0FBYUE7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7QUFWRjs7QUFhQTtFQUNFLGFBQUE7RUFDQSxTQUFBO0VBQ0EsbUJBQUE7QUFWRjs7QUFhQTtFQUNFLGVBQUE7RUFDQSxpQkFBQTtFQUNBLGdDQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBVkY7O0FBYUE7RUFDRSxxQkFBQTtFQUNBLGlCQUFBO0FBVkY7O0FBYUE7RUFDRSxxQkFBQTtBQVZGO0FBWUU7RUFDRSxrQkFBQTtBQVZKOztBQWNBO0VBQ0UsZUFBQTtFQUNBLGdDQUFBO0FBWEY7O0FBZ0JBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxtQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZUFBQTtBQWJGOztBQWdCQTtFQUNFLE9BQUE7QUFiRjs7QUFnQkE7RUFDRSxlQUFBO0VBQ0EsY0FBQTtFQUNBLGdCQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLDJCQUFBO0FBYkY7QUFlRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0FBYko7QUFnQkU7RUFDRSxTQUFBO0FBZEo7O0FBa0JBO0VBQ0UsZUFBQTtFQUNBLDBCQUFBO0FBZkY7O0FBa0JBO0VBQ0UseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBZkY7O0FBa0JBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsaUJBQUE7RUFDQSxnREFBQTtFQUNBLGdCQUFBO0FBZkY7QUFpQkU7RUFDRSxtQkFBQTtBQWZKOztBQW1CQTtFQUNFLGtDQUFBO0VBQ0EsZ0JBQUE7QUFoQkY7O0FBbUJBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0FBaEJGOztBQW1CQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLDJDQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0VBQ0EsaUNBQUE7RUFDQSw4QkFBQTtBQWhCRjs7QUFvQkU7RUFDRSxrQkFBQTtBQWpCSjs7QUFxQkE7RUFDRSxnQ0FBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUFsQkY7O0FBcUJBO0VBQ0UsT0FBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtBQWxCRjs7QUFxQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxpQkFBQTtBQWxCRjtBQW9CRTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7QUFsQko7QUFvQkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7QUFsQk47O0FBeUJBO0VBQ0UsY0FBQTtBQXRCRjs7QUF5QkE7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSw4QkFBQTtFQUNBLHFCQUFBO0FBdEJGOztBQXlCQTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsU0FBQTtBQXRCRjs7QUF5QkE7RUFDRSxjQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7QUF0QkY7QUF3QkU7RUFDRSxnQkFBQTtBQXRCSjs7QUEwQkE7RUFDRSxhQUFBO0VBQ0EsU0FBQTtFQUNBLG1CQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7QUF2QkY7O0FBMEJBO0VBQ0UsY0FBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtBQXZCRjs7QUEwQkE7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7RUFDQSxlQUFBO0FBdkJGOztBQTBCQTtFQUNFLG9CQUFBO0VBQ0EsZUFBQTtBQXZCRjs7QUEwQkE7RUFDRSxnQkFBQTtBQXZCRjtBQXlCRTtFQUNFLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLGdDQUFBO0VBQ0EsZ0JBQUE7RUFDQSx5QkFBQTtFQUNBLHFCQUFBO0FBdkJKOztBQTJCQTtFQUNFLGNBQUE7RUFDQSxnQ0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQXhCRjs7QUEyQkE7RUFDRSxnQkFBQTtBQXhCRjtBQTBCRTtFQUNFLGFBQUE7RUFDQSxjQUFBO0VBQ0EsZUFBQTtBQXhCSjs7QUE0QkE7RUFDRSxrQ0FBQTtFQUNBLGtCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EsZUFBQTtFQUNBLDhCQUFBO0FBekJGO0FBMkJFO0VBQ0UsY0FBQTtFQUNBLGtCQUFBO0VBQ0EsOEJBQUE7QUF6Qko7QUE0QkU7RUFDRSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQ0FBQTtBQTFCSjs7QUE2QkE7d0JBQUE7QUFFQTtFQUNFLGdCQUFBO0FBMUJGO0FBNEJFO0VBQ0UsV0FBQTtFQUNBLGdCQUFBO0FBMUJKOztBQThCQTtvRkFBQTtBQUVBO0VBQ0UsbUJBQUE7QUEzQkY7QUE2QkU7RUFDRSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7QUEzQko7QUE4QkU7RUFDRSxvQkFBQTtBQTVCSjs7QUFnQ0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBN0JGOztBQWdDQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0FBN0JGO0FBK0JFO0VBQ0UsZUFBQTtBQTdCSjs7QUFpQ0E7RUFDRSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQTlCRiIsInNvdXJjZXNDb250ZW50IjpbIi8vIFRhYiBzdHlsZXNcbi5zZXR0aW5ncy10YWJzIHtcbiAgbWluLXdpZHRoOiA1MDBweDtcbiAgbWF4LXdpZHRoOiA2MDBweDtcbn1cblxuLnRhYi1jb250ZW50IHtcbiAgcGFkZGluZzogMjBweCAxNnB4O1xufVxuXG4uc2V0dGluZ3Mtc2VjdGlvbiB7XG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XG59XG5cbi5zZXR0aW5nLWl0ZW0ge1xuICBtYXJnaW46IDE2cHggMDtcbn1cblxuLnNldHRpbmctbGFiZWwge1xuICBmb250LXdlaWdodDogNTAwO1xuICBtYXJnaW4tbGVmdDogOHB4O1xufVxuXG4uc2V0dGluZy1kZXNjcmlwdGlvbiB7XG4gIG1hcmdpbi10b3A6IDhweDtcbiAgbWFyZ2luLWxlZnQ6IDMycHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICBmb250LXNpemU6IDE0cHg7XG59XG5cbi5zZXR0aW5nLWVycm9yIHtcbiAgY29sb3I6IHZhcigtLW1kZS1lcnJvciwgI2M2MjgyOCk7XG59XG5cbi8qIEkgdGVtcGxhdGUgZGEgY3VpIHZpZW5lIGlsIHByb2dldHRvIChQMikuICovXG4udGVtcGxhdGUtcm93cyB7XG4gIG1hcmdpbjogMCAwIDEycHggMzJweDtcbiAgcGFkZGluZzogMDtcbiAgbGkgeyBtYXJnaW4tYm90dG9tOiA0cHg7IH1cbn1cblxuLnRlbXBsYXRlLXVybCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBvdmVyZmxvdy13cmFwOiBhbnl3aGVyZTtcbn1cblxuLnRlbXBsYXRlLWxhbmd1YWdlIHtcbiAgZGlzcGxheTogYmxvY2s7XG4gIHdpZHRoOiAyMDBweDtcbn1cblxuLnRlbXBsYXRlLWFkZCB7XG4gIG1hcmdpbi10b3A6IDhweDtcbn1cblxuLyogVW4gZG9jdW1lbnRvIGRlbCB3b3JrZmxvdyBwZXIgcmlnYSwgY29uIGlsIGNlc3Rpbm8gYWNjYW50byAoUFcpLiAqL1xuLndvcmtmbG93LWRvYy1yb3cge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcbiAgZ2FwOiA0cHg7XG59XG5cbi53b3JrZmxvdy1kb2MtZmllbGQge1xuICBmbGV4OiAxO1xuICBtaW4td2lkdGg6IDA7XG59XG5cbi8qIEljb25hIGEgZmlsbyBkaSB0ZXN0byBkZW50cm8gdW5hIC5zZXR0aW5nLWRlc2NyaXB0aW9uIChub3RhIGluZm9ybWF0aXZhKS4gKi9cbi5pbmxpbmUtd2Fybi1pY29uIHtcbiAgZm9udC1zaXplOiAxNnB4O1xuICBoZWlnaHQ6IDE2cHg7XG4gIHdpZHRoOiAxNnB4O1xuICB2ZXJ0aWNhbC1hbGlnbjogdGV4dC1ib3R0b207XG4gIG1hcmdpbi1yaWdodDogNHB4O1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbn1cblxuLmxvYWRpbmctY29udGFpbmVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHBhZGRpbmc6IDIwcHg7XG4gIGdhcDogMTJweDtcbn1cblxuLnNhdmluZy1pbmRpY2F0b3Ige1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgbWFyZ2luLXRvcDogOHB4O1xuICBtYXJnaW4tbGVmdDogMzJweDtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gIGZvbnQtc2l6ZTogMTRweDtcbn1cblxuLmluZm8tY2FyZCB7XG4gIGJhY2tncm91bmQtY29sb3I6IHZhcigtLW1kZS1iZy10ZXJ0aWFyeSk7XG5cbiAgbWF0LWNhcmQtY29udGVudCB7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBhbGlnbi1pdGVtczogc3RhcnQ7XG4gICAgZ2FwOiAxMnB4O1xuXG4gICAgbWF0LWljb24ge1xuICAgICAgY29sb3I6IHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XG4gICAgfVxuXG4gICAgcCB7XG4gICAgICBtYXJnaW46IDA7XG4gICAgICBmb250LXNpemU6IDE0cHg7XG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICB9XG4gIH1cbn1cblxuLmlkZS1yYWRpbyB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBtYXJnaW46IDEycHggMDtcbn1cblxuLmlkZS1sYWJlbCB7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogOHB4O1xufVxuXG4uaWRlLWljb24taW5saW5lIHtcbiAgZm9udC1zaXplOiAyMHB4O1xuICB3aWR0aDogMjBweDtcbiAgaGVpZ2h0OiAyMHB4O1xuXG4gICYudnNjb2RlLWNvbG9yIHtcbiAgICBjb2xvcjogIzAwN0FDQzsgLy8gVlMgQ29kZSBibHVlXG4gIH1cblxuICAmLmludGVsbGlqLWNvbG9yIHtcbiAgICBjb2xvcjogI0ZGNkIwMDsgLy8gSW50ZWxsaUogb3JhbmdlXG4gIH1cblxuICAmLmNvcGlsb3QtY29sb3Ige1xuICAgIGNvbG9yOiAjNkU0MEM5OyAvLyBHaXRIdWIgQ29waWxvdCBwdXJwbGVcbiAgfVxuXG4gICYuY2xhdWRlLWNvbG9yIHtcbiAgICBjb2xvcjogI0Q5Nzc1NzsgLy8gQ2xhdWRlIG9yYW5nZSwgYXMgdGhlIC5jbGF1ZGUgZm9sZGVyIGluIG1kLXRyZWVcbiAgfVxufVxuXG4vKiBDaGUgY29zYSBjb21wb3J0YSB1bmEgdm9jZTogc290dG8gbCdldGljaGV0dGEsIG5vbiBpbiB1biB0b29sdGlwIGRhIHNjb3ByaXJlLiAqL1xuLmhhcm5lc3MtcmVhc29uIHtcbiAgZGlzcGxheTogYmxvY2s7XG4gIG1hcmdpbi1sZWZ0OiAyOHB4O1xuICBmb250LXNpemU6IDEycHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICB3aGl0ZS1zcGFjZTogbm9ybWFsO1xufVxuXG4vKiBJbCBtb3RvcmUgZScgc3RhdG8gc2NvbGxlZ2F0byBhIG1hbm8gZGFsbCdhbWJpZW50ZSBkZWwgcmVwb3NpdG9yeS4gKi9cbi5oYXJuZXNzLW1pc21hdGNoIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC13cmFwOiB3cmFwO1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweCAxMnB4O1xuICBtYXJnaW46IDEycHggMDtcbiAgcGFkZGluZzogMTBweCAxMnB4O1xuICBib3JkZXItcmFkaXVzOiA2cHg7XG4gIGJhY2tncm91bmQtY29sb3I6IHZhcigtLW1kZS13YXJuaW5nLWJnKTtcbiAgY29sb3I6IHZhcigtLW1kZS13YXJuaW5nLWZnKTtcbiAgZm9udC1zaXplOiAxNHB4O1xuXG4gIC5oYXJuZXNzLW1pc21hdGNoLWljb24ge1xuICAgIGZsZXg6IDAgMCBhdXRvO1xuICB9XG5cbiAgLmhhcm5lc3MtbWlzbWF0Y2gtdGV4dCB7XG4gICAgZmxleDogMSAxIDI0MHB4O1xuICB9XG5cbiAgLmhhcm5lc3MtbWlzbWF0Y2gtYWN0aW9uIHtcbiAgICBmbGV4OiAwIDAgYXV0bztcbiAgfVxufVxuXG4vKiBBdmFuemF0ZTogY2hpdXNlIGRpIGRlZmF1bHQsIHBlcmNoZScgc2NvbGxlZ2FyZSBpbCBtb3RvcmUgbm9uIGUnIGxhIHN0cmFkYSBub3JtYWxlLiAqL1xuLmFkdmFuY2VkLWVuZ2luZS1ib2R5IHtcbiAgbWFyZ2luLWxlZnQ6IDhweDtcbiAgcGFkZGluZy1sZWZ0OiAxMnB4O1xuICBib3JkZXItbGVmdDogMnB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xufVxuXG4vLyBJREUgcGF0aHMgc3R5bGluZyByZW1vdmVkIC0gbm93IHNob3duIGluIFByb2plY3RzIGNvbXBvbmVudCBtYWluIHBhZ2VcblxuLy8gUkFHIHN0YXR1cyBzdHlsZXNcbi5yYWctc3RhdHVzIHtcbiAgbWFyZ2luOiAxNnB4IDAgMTZweCAzMnB4O1xuICBmb250LXNpemU6IDE0cHg7XG59XG5cbi5zdGF0dXMtcm93IHtcbiAgZGlzcGxheTogZmxleDtcbiAgZ2FwOiA4cHg7XG4gIG1hcmdpbjogNHB4IDA7XG59XG5cbi5zdGF0dXMtbGFiZWwge1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbn1cblxuLnN0YXR1cy1vayB7XG4gIGNvbG9yOiAjNGNhZjUwO1xuICBmb250LXdlaWdodDogNTAwO1xufVxuXG4uc3RhdHVzLW1pc3Npbmcge1xuICBjb2xvcjogI2Y0NDMzNjtcbiAgZm9udC13ZWlnaHQ6IDUwMDtcbn1cblxuLnJhZy1hY3Rpb25zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZ2FwOiAxMnB4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xufVxuXG4ucmFnLW1lc3NhZ2Uge1xuICBtYXJnaW4tdG9wOiA4cHg7XG4gIG1hcmdpbi1sZWZ0OiAzMnB4O1xuICBjb2xvcjogdmFyKC0tbWRlLWFjY2VudC1wcmltYXJ5KTtcbiAgZm9udC1zaXplOiAxNHB4O1xuICBmb250LXdlaWdodDogNTAwO1xufVxuXG4uaW5saW5lLXNwaW5uZXIge1xuICBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7XG4gIG1hcmdpbi1yaWdodDogNHB4O1xufVxuXG4ucmFnLXByb2dyZXNzIHtcbiAgbWFyZ2luOiAxMnB4IDAgMCAzMnB4O1xuXG4gIG1hdC1wcm9ncmVzcy1iYXIge1xuICAgIG1hcmdpbi1ib3R0b206IDZweDtcbiAgfVxufVxuXG4ucmFnLXByb2dyZXNzLXRleHQge1xuICBmb250LXNpemU6IDEzcHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xufVxuXG4vLyDDosKUwoDDosKUwoAgQXBwcyB0YWIgw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG5cbi5hcHBzLXRvb2xiYXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBnYXA6IDEycHg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcbn1cblxuLnRvb2xiYXItc3BhY2VyIHtcbiAgZmxleDogMTtcbn1cblxuLnNhdmVkLW1lc3NhZ2Uge1xuICBmb250LXNpemU6IDEzcHg7XG4gIGNvbG9yOiAjNGNhZjUwO1xuICBmb250LXdlaWdodDogNTAwO1xufVxuXG4uYXBwcy1lbXB0eS1zdGF0ZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDMycHggMTZweDtcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LWhpbnQpO1xuXG4gIG1hdC1pY29uIHtcbiAgICBmb250LXNpemU6IDQ4cHg7XG4gICAgd2lkdGg6IDQ4cHg7XG4gICAgaGVpZ2h0OiA0OHB4O1xuICAgIG1hcmdpbi1ib3R0b206IDhweDtcbiAgfVxuXG4gIHAge1xuICAgIG1hcmdpbjogMDtcbiAgfVxufVxuXG4uYXBwcy1lbXB0eS1oaW50IHtcbiAgZm9udC1zaXplOiAxM3B4O1xuICBtYXJnaW4tdG9wOiA0cHggIWltcG9ydGFudDtcbn1cblxuLmFwcHMtdHJlZS1lZGl0b3Ige1xuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBvdmVyZmxvdzogaGlkZGVuO1xufVxuXG4udHJlZS1yb3cge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBwYWRkaW5nOiA2cHggMTJweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICBtaW4taGVpZ2h0OiA0MHB4O1xuXG4gICY6bGFzdC1jaGlsZCB7XG4gICAgYm9yZGVyLWJvdHRvbTogbm9uZTtcbiAgfVxufVxuXG4uY2F0ZWdvcnktcm93IHtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXRlcnRpYXJ5KTtcbiAgZm9udC13ZWlnaHQ6IDUwMDtcbn1cblxuLmNhdGVnb3J5LW5hbWUge1xuICBmbGV4OiAxO1xuICBtYXJnaW4tbGVmdDogOHB4O1xufVxuXG4uY2F0ZWdvcnktZWRpdC1pbnB1dCB7XG4gIGZsZXg6IDE7XG4gIG1hcmdpbi1sZWZ0OiA4cHg7XG4gIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgcGFkZGluZzogNHB4IDhweDtcbiAgZm9udC1zaXplOiAxNHB4O1xuICBvdXRsaW5lOiBub25lO1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctcHJpbWFyeSk7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcbn1cblxuLmFwcC1yb3cge1xuICAmLmluZGVudGVkIHtcbiAgICBwYWRkaW5nLWxlZnQ6IDQwcHg7XG4gIH1cbn1cblxuLmFwcC1yb3ctaWNvbiB7XG4gIGNvbG9yOiB2YXIoLS1tZGUtYWNjZW50LXByaW1hcnkpO1xuICBmb250LXNpemU6IDIwcHg7XG4gIHdpZHRoOiAyMHB4O1xuICBoZWlnaHQ6IDIwcHg7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uYXBwLXJvdy1uYW1lIHtcbiAgZmxleDogMTtcbiAgbWFyZ2luLWxlZnQ6IDhweDtcbiAgZm9udC1zaXplOiAxNHB4O1xufVxuXG4udHJlZS1hY3Rpb25zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgbWFyZ2luLWxlZnQ6IGF1dG87XG5cbiAgYnV0dG9uIHtcbiAgICB3aWR0aDogMzJweDtcbiAgICBoZWlnaHQ6IDMycHg7XG4gICAgbGluZS1oZWlnaHQ6IDMycHg7XG5cbiAgICBtYXQtaWNvbiB7XG4gICAgICBmb250LXNpemU6IDE4cHg7XG4gICAgICB3aWR0aDogMThweDtcbiAgICAgIGhlaWdodDogMThweDtcbiAgICB9XG4gIH1cbn1cblxuLy8gw6LClMKAw6LClMKAIEtub3dsZWRnZSBHcmFwaCB0YWIgw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXG5cbi5rZy1maWVsZC1ncm91cCB7XG4gIG1hcmdpbjogMTRweCAwO1xufVxuXG4ua2ctZmllbGQtbGFiZWwge1xuICBkaXNwbGF5OiBibG9jaztcbiAgZm9udC1zaXplOiAxM3B4O1xuICBmb250LXdlaWdodDogNjAwO1xuICBtYXJnaW4tYm90dG9tOiA2cHg7XG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcbiAgbGV0dGVyLXNwYWNpbmc6IDAuMnB4O1xufVxuXG4ua2ctZmllbGQge1xuICBkaXNwbGF5OiBibG9jaztcbiAgd2lkdGg6IDEwMCU7XG4gIG1hcmdpbjogMDtcbn1cblxuLmtnLXJvdyB7XG4gIG1hcmdpbjogMTJweCAwO1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgZmxleC13cmFwOiB3cmFwO1xuXG4gICYua2ctYWN0aW9ucyB7XG4gICAgbWFyZ2luLXRvcDogMTZweDtcbiAgfVxufVxuXG4ua2ctYWN0aW9ucyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGdhcDogMTJweDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZmxleC13cmFwOiB3cmFwO1xuICBtYXJnaW46IDE2cHggMDtcbn1cblxuLmtnLXN0YXR1cy1vayB7XG4gIGNvbG9yOiAjNGNhZjUwO1xuICBmb250LXdlaWdodDogNTAwO1xuICBmb250LXNpemU6IDEzcHg7XG59XG5cbi5rZy1zdGF0dXMtZmFpbCB7XG4gIGNvbG9yOiAjZjQ0MzM2O1xuICBmb250LXdlaWdodDogNTAwO1xuICBmb250LXNpemU6IDEzcHg7XG59XG5cbi5rZy1zdGF0ZSB7XG4gIG1hcmdpbjogOHB4IDAgMTZweCAwO1xuICBmb250LXNpemU6IDE0cHg7XG59XG5cbi5rZy1uYW1lc3BhY2VzIHtcbiAgbWFyZ2luLXRvcDogMTJweDtcblxuICBoNCB7XG4gICAgbWFyZ2luOiA4cHggMCA2cHg7XG4gICAgZm9udC1zaXplOiAxM3B4O1xuICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgICBsZXR0ZXItc3BhY2luZzogMC40cHg7XG4gIH1cbn1cblxuLmtnLW1lc3NhZ2Uge1xuICBtYXJnaW46IDEycHggMDtcbiAgY29sb3I6IHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgZm9udC13ZWlnaHQ6IDUwMDtcbn1cblxuLmtnLWVycm9ycyB7XG4gIG1hcmdpbi10b3A6IDE2cHg7XG5cbiAgaDQge1xuICAgIG1hcmdpbjogOHB4IDA7XG4gICAgY29sb3I6ICNmNDQzMzY7XG4gICAgZm9udC1zaXplOiAxNHB4O1xuICB9XG59XG5cbi5rZy1lcnJvci1yb3cge1xuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctdGVydGlhcnkpO1xuICBwYWRkaW5nOiAxMHB4IDEycHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgbWFyZ2luOiA4cHggMDtcbiAgZm9udC1zaXplOiAxM3B4O1xuICBib3JkZXItbGVmdDogM3B4IHNvbGlkICNmNDQzMzY7XG5cbiAgc3Ryb25nIHtcbiAgICBkaXNwbGF5OiBibG9jaztcbiAgICBtYXJnaW4tYm90dG9tOiA0cHg7XG4gICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICB9XG5cbiAgdWwge1xuICAgIG1hcmdpbjogNnB4IDAgMDtcbiAgICBwYWRkaW5nLWxlZnQ6IDIwcHg7XG4gICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XG4gIH1cbn1cbi8qIElsIG51bWVybyBkZWkgcG9zdGk6IHBpY2NvbG8sIGFjY2FudG8gYWxsYSBzdWEgZXRpY2hldHRhLCBub24gdW4gY2FtcG8gYSB0dXR0YSBsYXJnaGV6emFcbiAgIHBlciB1bmEgY2lmcmEgc29sYS4gKi9cbi5zZXR0aW5nLWlubGluZSB7XG4gIG1hcmdpbi10b3A6IDEwcHg7XG5cbiAgLnNsb3RzLWZpZWxkIHtcbiAgICB3aWR0aDogOTBweDtcbiAgICBtYXJnaW4tbGVmdDogOHB4O1xuICB9XG59XG5cbi8qIEdydXBwaSBkaSBmdW56aW9uYWxpdMODwqAgTUNQOiBjYXNlbGxhLCBwZXNvIGUgdW5hIHJpZ2EgZGkgc3BpZWdhemlvbmUuIElsIHBlc28gc3RhIHN1bGxhXG4gICBzdGVzc2EgcmlnYSBkZWxsYSBjYXNlbGxhIHBlcmNow4PCqSDDg8KoIGwnaW5mb3JtYXppb25lIGNoZSBmYSBwcmVuZGVyZSBsYSBkZWNpc2lvbmUuICovXG4ubWNwLWdyb3VwLXJvdyB7XG4gIG1hcmdpbi1ib3R0b206IDEycHg7XG5cbiAgLm1jcC1ncm91cC13ZWlnaHQge1xuICAgIG1hcmdpbi1sZWZ0OiA4cHg7XG4gICAgZm9udC1zaXplOiAxMnB4O1xuICAgIG9wYWNpdHk6IDAuNztcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICB9XG5cbiAgLm1jcC1ncm91cC1zdW1tYXJ5IHtcbiAgICBtYXJnaW46IDJweCAwIDAgMzJweDtcbiAgfVxufVxuXG4ubWNwLWdyb3Vwcy10b3RhbCB7XG4gIG1hcmdpbi10b3A6IDE2cHg7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG59XG5cbi5oYXJuZXNzLW1vZGVsLXJvdyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogOHB4O1xuICBmbGV4LXdyYXA6IHdyYXA7XG5cbiAgLmhhcm5lc3MtbW9kZWwtZmllbGQge1xuICAgIGZsZXg6IDEgMSAyMjBweDtcbiAgfVxufVxuXG4uc2V0dGluZy1zdWJ0aXRsZSB7XG4gIG1hcmdpbjogMTZweCAwIDRweDtcbiAgZm9udC1zaXplOiAxNHB4O1xuICBmb250LXdlaWdodDogNTAwO1xufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
    });
  }
}

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
/* harmony import */ var _home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! rxjs/operators */ 635);
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/project-edit/project-edit-dialog.component */ 9116);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/p2p-manager/p2p-manager.component */ 1902);
/* harmony import */ var _components_unified_settings_dialog_unified_settings_dialog_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../components/unified-settings-dialog/unified-settings-dialog.component */ 8833);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _environments_version__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../../environments/version */ 9279);
/* harmony import */ var _dialogs_template_showcase_template_showcase_dialog_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./dialogs/template-showcase/template-showcase-dialog.component */ 2555);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../shared/NgDialogAnimationService */ 5811);
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ../git/services/gitservice.service */ 7224);
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/cdk/clipboard */ 6079);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _services_p2p_service__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ../services/p2p.service */ 9811);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_28__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_29__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_30__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_31__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_32__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _components_participant_gems_participant_gems_component__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ../components/participant-gems/participant-gems.component */ 587);










 // Importa la versione






















function ProjectsComponent_div_4_div_1_button_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r10 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "button", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_4_div_1_button_5_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r10);
      const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r9.openSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](2, 1, "PROJECTS.NOTICE_OPEN_SETTINGS"), " ");
  }
}
function ProjectsComponent_div_4_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 30)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](3, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](5, ProjectsComponent_div_4_div_1_button_5_Template, 3, 3, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](6, "button", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_4_div_1_Template_button_click_6_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r12);
      const n_r7 = restoredCtx.$implicit;
      const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r11.dismissNotice(n_r7));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](9, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const n_r7 = ctx.$implicit;
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵclassProp"]("level-warning", n_r7.level === "warning")("level-error", n_r7.level === "error");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](n_r7.level === "error" ? "error" : n_r7.level === "warning" ? "warning" : "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](ctx_r6.noticeText(n_r7));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", (n_r7.kind || "").startsWith("java"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](7, 8, "PROJECTS.NOTICE_DISMISS"));
  }
}
function ProjectsComponent_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](1, ProjectsComponent_div_4_div_1_Template, 10, 10, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngForOf", ctx_r0.startupNotices);
  }
}
function ProjectsComponent_div_5_button_12_Template(rf, ctx) {
  if (rf & 1) {
    const _r17 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "button", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_button_12_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r17);
      const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](2);
      ctx_r16.searchQuery = "";
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r16.onSearchChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2, "clear");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
  }
}
function ProjectsComponent_div_5_div_13_Template(rf, ctx) {
  if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 44)(1, "mat-icon", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2, "search_off");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](6, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](9, "button", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_13_Template_button_click_9_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r19);
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](2);
      ctx_r18.searchQuery = "";
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r18.onSearchChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](11, "clear");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](5, 3, "PROJECTS.NO_MATCH_TITLE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](8, 5, "PROJECTS.NO_MATCH_HINT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](13, 7, "PROJECTS.CLEAR_SEARCH"), " ");
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_img_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelement"](0, "img", 68);
  }
  if (rf & 2) {
    const project_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]().$implicit;
    const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("src", ctx_r22.getIconUrl(project_r21), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵsanitizeUrl"]);
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_img_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelement"](0, "img", 69);
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_span_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "span", 70)(1, "mat-icon", 71);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2, "schedule");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](4, 1, "PROJECTS.LAST_OPENED"), " ");
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_p_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "p", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](project_r21.description);
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_span_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "span", 73)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2, "edit_note");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](4, 1, "PROJECTS.ADD_DESCRIPTION"), " ");
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_button_27_Template(rf, ctx) {
  if (rf & 1) {
    const _r33 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "button", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_button_27_Template_button_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r33);
      const project_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]().$implicit;
      const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r31.shareProject(project_r21, $event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](3, "share");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](1, 1, "PROJECTS.SHARE_URL"));
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_app_participant_gems_32_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelement"](0, "app-participant-gems", 75);
  }
  if (rf & 2) {
    const project_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]().$implicit;
    const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("participants", ctx_r28.getParticipantsFor(project_r21.path))("currentUserEmail", ctx_r28.currentUserEmail)("maxVisible", 4);
  }
}
function ProjectsComponent_div_5_div_15_mat_card_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r36 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "mat-card", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](2, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](3, "mat-card-content", 49)(4, "div", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_Template_div_click_4_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r36);
      const project_r21 = restoredCtx.$implicit;
      const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r35.openProject(project_r21.path));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](5, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](6, ProjectsComponent_div_5_div_15_mat_card_1_img_6_Template, 1, 1, "img", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](7, ProjectsComponent_div_5_div_15_mat_card_1_img_7_Template, 1, 0, "img", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](8, "div", 54)(9, "div", 55)(10, "h3", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](12, ProjectsComponent_div_5_div_15_mat_card_1_span_12_Template, 5, 3, "span", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](13, "p", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](14);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](15, "div", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_Template_div_click_15_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r36);
      const project_r21 = restoredCtx.$implicit;
      const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r37.openProject(project_r21.path));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](16, ProjectsComponent_div_5_div_15_mat_card_1_p_16_Template, 2, 1, "p", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](17, ProjectsComponent_div_5_div_15_mat_card_1_span_17_Template, 5, 3, "span", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](18, "div", 62)(19, "button", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_Template_button_click_19_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r36);
      const project_r21 = restoredCtx.$implicit;
      const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r38.openProjectEdit(project_r21));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](20, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](21, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](22, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](23, "button", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_Template_button_click_23_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r36);
      const project_r21 = restoredCtx.$implicit;
      const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r39.openProjectSettings(project_r21));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](24, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](25, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](26, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](27, ProjectsComponent_div_5_div_15_mat_card_1_button_27_Template, 4, 3, "button", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](28, "button", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_5_div_15_mat_card_1_Template_button_click_28_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r36);
      const project_r21 = restoredCtx.$implicit;
      const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r40.deleteProject(project_r21));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](29, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](30, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](31, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](32, ProjectsComponent_div_5_div_15_mat_card_1_app_participant_gems_32_Template, 1, 3, "app-participant-gems", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r21 = ctx.$implicit;
    const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](3);
    let tmp_1_0;
    let tmp_16_0;
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵclassProp"]("last-opened", ctx_r20.isLastOpened(project_r21))("has-gems", (((tmp_1_0 = ctx_r20.getParticipantsFor(project_r21.path)) == null ? null : tmp_1_0.length) || 0) > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", project_r21.lastUpdate ? _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](1, 21, "PROJECTS.LAST_ACCESSED") + " " + _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind2"](2, 23, project_r21.lastUpdate, "short") : null);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵclassProp"]("has-custom", project_r21.hasCustomIcon);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", project_r21.hasCustomIcon);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", !project_r21.hasCustomIcon);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](project_r21.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx_r20.isLastOpened(project_r21));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](project_r21.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵclassProp"]("empty", !project_r21.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", project_r21.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", !project_r21.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](20, 26, "PROJECTS.EDIT_PROJECT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](24, 28, "PROJECTS.PROJECT_SETTINGS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx_r20.projectHasRemote(project_r21));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](29, 30, "PROJECTS.DELETE_PROJECT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", (((tmp_16_0 = ctx_r20.getParticipantsFor(project_r21.path)) == null ? null : tmp_16_0.length) || 0) > 0);
  }
}
function ProjectsComponent_div_5_div_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](1, ProjectsComponent_div_5_div_15_mat_card_1_Template, 33, 32, "mat-card", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](2, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](2, 1, ctx_r15.recentProjects));
  }
}
function ProjectsComponent_div_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r42 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 35)(1, "div", 36)(2, "h2", 9)(3, "mat-icon", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](4, "history");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](7, "mat-form-field", 37)(8, "mat-icon", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](9, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](10, "input", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("ngModelChange", function ProjectsComponent_div_5_Template_input_ngModelChange_10_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r42);
      const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r41.searchQuery = $event);
    })("input", function ProjectsComponent_div_5_Template_input_input_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r42);
      const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r43.onSearchChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](12, ProjectsComponent_div_5_button_12_Template, 3, 0, "button", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](13, ProjectsComponent_div_5_div_13_Template, 14, 9, "div", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](14, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](15, ProjectsComponent_div_5_div_15_Template, 3, 3, "div", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](16, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
    let tmp_4_0;
    let tmp_5_0;
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](6, 6, "PROJECTS.RECENT_PROJECTS"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](11, 8, "PROJECTS.SEARCH_PROJECTS"))("ngModel", ctx_r1.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx_r1.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx_r1.searchQuery && ((tmp_4_0 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](14, 10, ctx_r1.recentProjects)) == null ? null : tmp_4_0.length) === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ((tmp_5_0 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](16, 12, ctx_r1.recentProjects)) == null ? null : tmp_5_0.length) > 0);
  }
}
function ProjectsComponent_div_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 76)(1, "mat-icon", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](2, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](6, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](5, 2, "PROJECTS.NO_RECENT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](8, 4, "PROJECTS.NO_RECENT_HINT"));
  }
}
function ProjectsComponent_div_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r45 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 77);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_div_9_Template_div_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r45);
      const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r44.actionsExpanded = false);
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
}
function ProjectsComponent_mat_card_65_Template(rf, ctx) {
  if (rf & 1) {
    const _r47 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "mat-card", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_mat_card_65_Template_mat_card_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r47);
      const ctx_r46 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r46.act("openP2PManager"));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](2, "mat-card-content", 13)(3, "div", 79)(4, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](5, "share");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](6, "div", 15)(7, "h2", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](10, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](13, "div", 18)(14, "mat-icon", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](15, "arrow_forward");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](1, 4, "PROJECTS.P2P_SHARING"))("matTooltipDisabled", !ctx_r4.isCompact() || ctx_r4.actionsExpanded);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](9, 6, "PROJECTS.P2P_SHARING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](12, 8, "PROJECTS.P2P_SHARING_DESC"));
  }
}
function ProjectsComponent_button_73_Template(rf, ctx) {
  if (rf & 1) {
    const _r49 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "button", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_button_73_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵrestoreView"](_r49);
      const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵresetView"](ctx_r48.openElectronLog());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](3, "monitor");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](1, 2, "PROJECTS.ELECTRON_LOG_TOOLTIP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](5, 4, "PROJECTS.ELECTRON_LOG"), " ");
  }
}
class ProjectsComponent {
  constructor(projectService, dialog, router, signalRService, dialogAn, gitService, clipboard, snackBar, http, p2pService, translate) {
    this.projectService = projectService;
    this.dialog = dialog;
    this.router = router;
    this.signalRService = signalRService;
    this.dialogAn = dialogAn;
    this.gitService = gitService;
    this.clipboard = clipboard;
    this.snackBar = snackBar;
    this.http = http;
    this.p2pService = p2pService;
    this.translate = translate;
    this.appVersion = _environments_version__WEBPACK_IMPORTED_MODULE_9__.versionInfo.version; // Rendi la versione disponibile nel template
    this.buildTime = _environments_version__WEBPACK_IMPORTED_MODULE_9__.versionInfo.buildTime; // Rendi il timestamp di build disponibile nel template
    this.searchQuery = '';
    this.lastOpenedProjectId = null;
    this.isP2PAvailable = false;
    // Current git user email (lowercase) — used to hide "me" from the gem strip
    this.currentUserEmail = null;
    // Flag to prevent multiple clicks when opening a project
    this.isOpeningProject = false;
    // Cache for remote URL status per project path
    this.remoteUrlCache = new Map();
    // Participants are fetched asynchronously per-project after the grid renders,
    // so a slow repo does not block the others. Key = project.path.
    this.participantsByPath = new Map();
    this.dataSource1 = [{
      name: 'Nome progetto',
      path: 'c:\folder\folder\folder'
    }];
    /** Avvisi di avvio del servizio (Java e simili): api/Diagnostics/StartupNotices, chiusi uno per uno. */
    this.startupNotices = [];
    /** Le card delle azioni aperte sopra la lista, quando la finestra è stretta e sono una colonnina di icone. */
    this.actionsExpanded = false;
    /** La stessa larghezza della regola CSS che riduce le azioni a una colonnina (projects.component.scss). */
    this.compactQuery = window.matchMedia('(max-width: 1024px)');
  }
  ngOnDestroy() {
    console.log('ProjectsComponent destroyed!');
  }
  loadStartupNotices() {
    this.http.get('../api/Diagnostics/StartupNotices').subscribe({
      next: n => this.startupNotices = n || [],
      error: () => this.startupNotices = []
    });
  }
  dismissNotice(n) {
    this.http.post(`../api/Diagnostics/StartupNotices/${n.id}/dismiss`, {}).subscribe({
      next: () => this.startupNotices = this.startupNotices.filter(x => x.id !== n.id),
      error: () => this.startupNotices = this.startupNotices.filter(x => x.id !== n.id)
    });
  }
  noticeText(n) {
    const params = {
      ...(n.params || {})
    };
    if (params.problemKind) {
      params.problem = this.translate.instant('PROJECTS.NOTICE_PROBLEM_' + String(params.problemKind).toUpperCase(), params);
    }
    return this.translate.instant('PROJECTS.NOTICE_' + String(n.kind || '').toUpperCase().replace(/-/g, '_'), params);
  }
  ngOnInit() {
    this.loadStartupNotices();
    // Check P2P availability
    this.p2pService.isAvailable$.subscribe(available => {
      this.isP2PAvailable = available;
    });
    this.p2pService.checkAvailability();
    // Fetch the current git user's email so the team gems can hide "me".
    // Fallback: no path → backend returns the global git config.
    this.projectService.getCurrentGitUser(null).subscribe({
      next: user => {
        this.currentUserEmail = user?.email ? user.email.toLowerCase() : null;
      },
      error: () => {
        this.currentUserEmail = null;
      }
    });
    // Load recent projects and sort by lastUpdate descending (most recent first)
    this.projectService.fetchProjects();
    // Pre-filter signal: are there ANY projects registered? Drives the
    // visibility of the "Recent Projects" section header + search field,
    // independently of the search filter result.
    this.hasAnyProject$ = this.projectService.mdProjects.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_18__.map)(projects => !!projects && projects.length > 0));
    this.recentProjects = this.projectService.mdProjects.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_18__.map)(projects => {
      if (!projects || projects.length === 0) return [];
      const sorted = projects.sort((a, b) => {
        const dateA = a.lastUpdate ? new Date(a.lastUpdate).getTime() : 0;
        const dateB = b.lastUpdate ? new Date(b.lastUpdate).getTime() : 0;
        return dateB - dateA; // Descending order (most recent first)
      });
      // Identify the last opened project (first in sorted list)
      if (sorted.length > 0 && !this.lastOpenedProjectId) {
        this.lastOpenedProjectId = sorted[0].id;
      }
      // Kick off parallel participant fetches — cards render immediately,
      // each card's gem strip fills in as its own response arrives.
      this.loadParticipantsFor(sorted);
      // Apply search filter
      if (this.searchQuery && this.searchQuery.trim() !== '') {
        const query = this.searchQuery.toLowerCase();
        return sorted.filter(p => p.name.toLowerCase().includes(query) || p.path.toLowerCase().includes(query));
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
  getIconUrl(project) {
    return this.projectService.getProjectIconUrl(project.id, project.iconUpdatedAt);
  }
  getParticipantsFor(path) {
    return this.participantsByPath.get(path) || [];
  }
  loadParticipantsFor(projects) {
    for (const p of projects) {
      if (!p?.path) continue;
      if (this.participantsByPath.has(p.path)) continue; // already loaded or in-flight
      this.participantsByPath.set(p.path, []); // placeholder prevents duplicate requests
      this.projectService.getParticipants(p.path).subscribe({
        next: list => this.participantsByPath.set(p.path, list || []),
        error: () => this.participantsByPath.set(p.path, [])
      });
    }
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
    const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_5__.ProjectSettingsComponent, {
      width: '600px',
      data: {
        projectId: project.id,
        projectName: project.name,
        projectPath: project.path
      }
    });
  }
  openProjectEdit(project) {
    const dialogRef = this.dialog.open(_dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_4__.ProjectEditDialogComponent, {
      width: '620px',
      maxHeight: '90vh',
      data: {
        id: project.id,
        name: project.name,
        description: project.description,
        path: project.path,
        hasCustomIcon: !!project.hasCustomIcon,
        iconUpdatedAt: project.iconUpdatedAt
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }
      // Sequential saves: first name/description (UserDB + .development.yml),
      // then participants (.development.yml), then the optional icon change.
      // Icon is last because it's the heaviest payload and the only one that
      // can be skipped (iconAction === 'none').
      this.projectService.updateProject({
        id: result.id,
        name: result.name,
        description: result.description
      }).subscribe({
        next: () => {
          this.projectService.saveParticipants(result.path, result.participants || []).subscribe({
            next: () => {
              this.applyIconChange(result, () => {
                this.projectService.fetchProjects();
                this.snackBar.open(this.translate.instant('PROJECTS.PROJECT_UPDATED'), 'OK', {
                  duration: 2500
                });
              });
            },
            error: err => {
              console.error('[Projects] Error saving participants:', err);
              this.projectService.fetchProjects();
              this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_PARTICIPANTS'), 'OK', {
                duration: 4000
              });
            }
          });
        },
        error: err => {
          console.error('[Projects] Error updating project:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_UPDATING_PROJECT'), 'OK', {
            duration: 4000
          });
        }
      });
    });
  }
  /**
   * Persists a pending icon change (set / remove). No-op when iconAction is 'none'.
   * Errors are surfaced via the snackbar but do not block the rest of the save flow,
   * because at this point name/description/participants are already persisted.
   */
  applyIconChange(result, done) {
    console.debug('[Projects] applyIconChange', {
      iconAction: result.iconAction,
      pngLen: result.iconPngBase64?.length ?? 0,
      projectId: result.id
    });
    if (result.iconAction === 'set' && result.iconPngBase64) {
      this.projectService.setProjectIcon(result.id, result.iconPngBase64).subscribe({
        next: () => done(),
        error: err => {
          console.error('[Projects] Error saving project icon:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_ICON'), 'OK', {
            duration: 4000
          });
          done();
        }
      });
    } else if (result.iconAction === 'remove') {
      this.projectService.removeProjectIcon(result.id).subscribe({
        next: () => done(),
        error: err => {
          console.error('[Projects] Error removing project icon:', err);
          this.snackBar.open(this.translate.instant('PROJECTS.ERROR_SAVING_ICON'), 'OK', {
            duration: 4000
          });
          done();
        }
      });
    } else {
      done();
    }
  }
  openRecent() {
    this.router.navigate(['/projects/openrecent']);
  }
  prepareToClone() {
    const dialogRef = this.dialog.open(_dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_2__.ModernCloneProjectComponent, {
      width: '600px',
      maxHeight: '600px',
      data: null
    });
  }
  isCompact() {
    return this.compactQuery.matches;
  }
  /**
   * Un clic su una card delle azioni. Con la colonnina chiusa apre le card, così si legge che cosa fa ognuna; con le
   * card aperte (o con la finestra larga) esegue l'azione.
   */
  act(action) {
    if (this.isCompact() && !this.actionsExpanded) {
      this.actionsExpanded = true;
      return;
    }
    this.actionsExpanded = false;
    this[action]();
  }
  /**
   * «Crea nuovo progetto»: prima la vetrina dei template (sprint 2026-10-19-Vetrina-Dei-Template.md, VD1), poi la
   * cartella, poi la configurazione di ciò che resta.
   */
  openNewFolder() {
    this.dialog.open(_dialogs_template_showcase_template_showcase_dialog_component__WEBPACK_IMPORTED_MODULE_10__.TemplateShowcaseDialogComponent, {
      width: '90vw',
      height: '90vh',
      maxWidth: '90vw',
      disableClose: true
    }).afterClosed().subscribe(choice => {
      if (choice) this.chooseFolder(choice);
    });
  }
  chooseFolder(choice) {
    let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_8__.ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('PROJECTS.SELECT_FOLDER');
    data.typeOfSelection = "Folders";
    data.buttonText = this.translate.instant('PROJECTS.SELECT_FOLDER_BTN');
    const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__.ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.data) {
        // Open configuration dialog after folder selection
        const configDialogRef = this.dialog.open(_dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_3__.ProjectCreateConfigDialogComponent, {
          width: '500px',
          disableClose: true,
          data: {
            projectPath: result.data,
            choice
          }
        });
        configDialogRef.afterClosed().subscribe(config => {
          if (!config) return;
          if (config.template) {
            this.createFromTemplates(config);
            return;
          }
          // Create project with configuration options
          this.projectService.createProjectWithConfig(config);
        });
      }
    });
  }
  /**
   * Il progetto da template (P4): nella cartella nuova dentro quella scelta, dalla base (o dal primo metodo) più gli altri
   * metodi; poi si apre. Il servizio dice perché, se non si può, e in quel caso non lascia niente.
   */
  createFromTemplates(config) {
    var _this = this;
    return (0,_home_carlo_Documents_sviluppo_MdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      const t = config.template;
      const separator = config.projectPath.includes('\\') && !config.projectPath.includes('/') ? '\\' : '/';
      const path = config.projectPath.replace(/[\\/]+$/, '') + separator + t.folderName;
      const urls = [...(t.baseUrl ? [t.baseUrl] : []), ...t.methodUrls];
      const waiting = _this.snackBar.open(_this.translate.instant('PROJECT_CONFIG.TEMPLATE_CREATING', {
        path
      }), undefined, {
        verticalPosition: 'top'
      });
      try {
        const created = yield _this.projectService.createFromTemplate({
          path,
          templateUrl: urls[0],
          methods: urls.slice(1),
          theory: t.theoryUrls,
          language: t.language,
          example: t.example,
          agentCity: t.agentCity,
          agentCityGuide: t.agentCityGuide,
          originUrl: t.originUrl || undefined,
          // Il motore lo sceglie chi crea; la demo porta il suo.
          harness: t.asksEngine ? config.harness : undefined
        });
        waiting.dismiss();
        _this.projectService.setNewFolderProject(created.path);
        // Le note (ruoli aperti, workflow fermi…) restano finché non si chiudono: dicono che cosa resta da fare.
        if (created.notes?.length) _this.snackBar.open(created.notes.join(' '), 'OK', {
          verticalPosition: 'top'
        });
      } catch (err) {
        waiting.dismiss();
        _this.snackBar.open(_this.translate.instant('PROJECT_CONFIG.TEMPLATE_FAILED', {
          error: err?.message || String(err)
        }), 'OK', {
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    })();
  }
  openSettings() {
    this.dialog.open(_components_unified_settings_dialog_unified_settings_dialog_component__WEBPACK_IMPORTED_MODULE_7__.UnifiedSettingsDialogComponent, {
      width: '900px',
      maxHeight: '80vh',
      data: {
        initialTab: 'application'
      }
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
    this.remoteUrlCache.set(project.path, {
      hasRemote: false,
      loading: true
    });
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
    if (!cached?.hasRemote || !cached?.remoteUrl) {
      this.snackBar.open(this.translate.instant('PROJECTS.NO_GIT_REMOTE'), 'OK', {
        duration: 3000
      });
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
    this.snackBar.open(this.translate.instant('PROJECTS.SHARE_URL_COPIED'), 'OK', {
      duration: 3000
    });
    console.log('[Projects] Share URL copied:', shareUrl);
  }
  /**
   * Open the application log file (.NET backend)
   */
  openLog() {
    this.http.post('../api/Diagnostics/OpenLog', {}).subscribe({
      next: result => {
        console.log('[Projects] Log opened:', result.path);
      },
      error: err => {
        console.error('[Projects] Error opening log:', err);
        this.snackBar.open(this.translate.instant('PROJECTS.ERROR_OPENING_LOG', {
          error: err.error?.error || err.message
        }), 'OK', {
          duration: 5000
        });
      }
    });
  }
  /**
   * Open the Electron log file (only available in Electron environment)
   */
  openElectronLog() {
    const electronAPI = window.electronAPI;
    if (electronAPI?.openElectronLog) {
      electronAPI.openElectronLog().then(result => {
        if (result.success) {
          console.log('[Projects] Electron log opened:', result.path);
        } else {
          console.error('[Projects] Electron log not found:', result.error);
          this.snackBar.open(this.translate.instant('PROJECTS.ELECTRON_LOG_NOT_FOUND', {
            path: result.path
          }), 'OK', {
            duration: 5000
          });
        }
      }).catch(err => {
        console.error('[Projects] Error opening Electron log:', err);
        this.snackBar.open(this.translate.instant('PROJECTS.ERROR_OPENING_ELECTRON_LOG'), 'OK', {
          duration: 5000
        });
      });
    } else {
      this.snackBar.open(this.translate.instant('PROJECTS.ELECTRON_LOG_DESKTOP_ONLY'), 'OK', {
        duration: 3000
      });
    }
  }
  /**
   * Check if running in Electron environment
   */
  isElectron() {
    return !!window.electronAPI;
  }
  /**
   * Open the P2P Manager dialog
   */
  openP2PManager() {
    const dialogRef = this.dialog.open(_dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_6__.P2PManagerComponent, {
      width: '700px',
      maxHeight: '80vh',
      data: {}
    });
  }
  static {
    this.ɵfac = function ProjectsComponent_Factory(t) {
      return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_11__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_19__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_20__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_12__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_13__.NgDialogAnimationService), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_14__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_21__.Clipboard), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_22__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_23__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_services_p2p_service__WEBPACK_IMPORTED_MODULE_15__.P2PService), _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_24__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵdefineComponent"]({
      type: ProjectsComponent,
      selectors: [["app-projects"]],
      decls: 74,
      vars: 51,
      consts: [[1, "modern-container"], [1, "content-wrapper"], [1, "main-layout"], [1, "left-column"], ["class", "startup-notices", 4, "ngIf"], ["class", "recent-projects-section", 4, "ngIf"], ["class", "no-projects-message", 4, "ngIf"], ["class", "actions-backdrop", 3, "click", 4, "ngIf"], [1, "right-column"], [1, "section-title"], [1, "section-icon"], [1, "actions-grid"], ["data-test", "clone-button", "matTooltipPosition", "left", 1, "action-card", "clone-card", 3, "matTooltip", "matTooltipDisabled", "click"], [1, "card-content"], [1, "icon-wrapper", "clone-icon"], [1, "card-text"], [1, "card-title"], [1, "card-description"], [1, "card-action"], [1, "arrow-icon"], ["data-test", "new-folder-button", "matTooltipPosition", "left", 1, "action-card", "create-card", 3, "matTooltip", "matTooltipDisabled", "click"], [1, "icon-wrapper", "create-icon"], ["data-test", "settings-button", "matTooltipPosition", "left", 1, "action-card", "settings-card", 3, "matTooltip", "matTooltipDisabled", "click"], [1, "icon-wrapper", "settings-icon"], ["class", "action-card p2p-card", "data-test", "p2p-button", "matTooltipPosition", "left", 3, "matTooltip", "matTooltipDisabled", "click", 4, "ngIf"], [1, "diagnostics-section"], ["mat-stroked-button", "", "color", "primary", 3, "matTooltip", "click"], ["mat-stroked-button", "", "color", "primary", 3, "matTooltip", "click", 4, "ngIf"], [1, "startup-notices"], ["class", "startup-notice", 3, "level-warning", "level-error", 4, "ngFor", "ngForOf"], [1, "startup-notice"], [1, "notice-text"], ["mat-stroked-button", "", "color", "primary", 3, "click", 4, "ngIf"], ["mat-icon-button", "", 3, "matTooltip", "click"], ["mat-stroked-button", "", "color", "primary", 3, "click"], [1, "recent-projects-section"], [1, "section-header"], ["appearance", "outline", 1, "search-field"], ["matPrefix", ""], ["matInput", "", 3, "placeholder", "ngModel", "ngModelChange", "input"], ["mat-icon-button", "", "matSuffix", "", 3, "click", 4, "ngIf"], ["class", "no-match-message", 4, "ngIf"], ["class", "projects-grid", 4, "ngIf"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], [1, "no-match-message"], [1, "empty-icon"], [1, "projects-grid"], ["class", "project-card", "matTooltipPosition", "below", 3, "last-opened", "has-gems", "matTooltip", 4, "ngFor", "ngForOf"], ["matTooltipPosition", "below", 1, "project-card", 3, "matTooltip"], [1, "project-card-content"], [1, "project-info", 3, "click"], [1, "project-icon-wrapper"], ["class", "project-icon-custom", "alt", "Project icon", 3, "src", 4, "ngIf"], ["src", "assets/icons/files-stack.svg", "class", "project-icon-svg", "alt", "Project", 4, "ngIf"], [1, "project-details"], [1, "project-name-row"], [1, "project-name"], ["class", "last-opened-badge", 4, "ngIf"], [1, "project-path"], [1, "project-description-box", 3, "click"], ["class", "project-description", 4, "ngIf"], ["class", "empty-placeholder", 4, "ngIf"], [1, "project-actions"], ["mat-icon-button", "", 1, "action-btn", "edit-btn", 3, "matTooltip", "click"], ["mat-icon-button", "", 1, "action-btn", "settings-btn", 3, "matTooltip", "click"], ["mat-icon-button", "", "class", "action-btn share-btn", 3, "matTooltip", "click", 4, "ngIf"], ["mat-icon-button", "", 1, "action-btn", "delete-btn", 3, "matTooltip", "click"], ["class", "card-gems", 3, "participants", "currentUserEmail", "maxVisible", 4, "ngIf"], ["alt", "Project icon", 1, "project-icon-custom", 3, "src"], ["src", "assets/icons/files-stack.svg", "alt", "Project", 1, "project-icon-svg"], [1, "last-opened-badge"], [1, "badge-icon"], [1, "project-description"], [1, "empty-placeholder"], ["mat-icon-button", "", 1, "action-btn", "share-btn", 3, "matTooltip", "click"], [1, "card-gems", 3, "participants", "currentUserEmail", "maxVisible"], [1, "no-projects-message"], [1, "actions-backdrop", 3, "click"], ["data-test", "p2p-button", "matTooltipPosition", "left", 1, "action-card", "p2p-card", 3, "matTooltip", "matTooltipDisabled", "click"], [1, "icon-wrapper", "p2p-icon"]],
      template: function ProjectsComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "div", 2)(3, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](4, ProjectsComponent_div_4_Template, 2, 1, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](5, ProjectsComponent_div_5_Template, 17, 14, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](6, "async");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](7, ProjectsComponent_div_7_Template, 9, 6, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](8, "async");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](9, ProjectsComponent_div_9_Template, 1, 0, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](10, "div", 8)(11, "h2", 9)(12, "mat-icon", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](13, "flash_on");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](14);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](15, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](16, "div", 11)(17, "mat-card", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_17_listener() {
            return ctx.act("prepareToClone");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](18, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](19, "mat-card-content", 13)(20, "div", 14)(21, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](22, "cloud_download");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](23, "div", 15)(24, "h2", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](26, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](27, "p", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](28);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](29, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](30, "div", 18)(31, "mat-icon", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](32, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](33, "mat-card", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_33_listener() {
            return ctx.act("openNewFolder");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](34, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](35, "mat-card-content", 13)(36, "div", 21)(37, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](38, "create_new_folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](39, "div", 15)(40, "h2", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](41);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](42, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](43, "p", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](44);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](45, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](46, "div", 18)(47, "mat-icon", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](48, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](49, "mat-card", 22);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_49_listener() {
            return ctx.act("openSettings");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](50, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](51, "mat-card-content", 13)(52, "div", 23)(53, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](54, "settings");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](55, "div", 15)(56, "h2", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](57);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](58, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](59, "p", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](60);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](61, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](62, "div", 18)(63, "mat-icon", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](64, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](65, ProjectsComponent_mat_card_65_Template, 16, 10, "mat-card", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](66, "div", 25)(67, "button", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_67_listener() {
            return ctx.openLog();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](68, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementStart"](69, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](70, "description");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtext"](71);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipe"](72, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtemplate"](73, ProjectsComponent_button_73_Template, 6, 6, "button", 27);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵelementEnd"]()()()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx.startupNotices.length > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](6, 23, ctx.hasAnyProject$));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", !_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](8, 25, ctx.hasAnyProject$));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx.actionsExpanded);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵclassProp"]("expanded", ctx.actionsExpanded);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](15, 27, "PROJECTS.QUICK_ACTIONS"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](18, 29, "PROJECTS.CLONE_REPO"))("matTooltipDisabled", !ctx.isCompact() || ctx.actionsExpanded);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](26, 31, "PROJECTS.CLONE_REPO"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](29, 33, "PROJECTS.CLONE_REPO_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](34, 35, "PROJECTS.CREATE_NEW"))("matTooltipDisabled", !ctx.isCompact() || ctx.actionsExpanded);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](42, 37, "PROJECTS.CREATE_NEW"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](45, 39, "PROJECTS.CREATE_NEW_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](50, 41, "SETTINGS.TITLE"))("matTooltipDisabled", !ctx.isCompact() || ctx.actionsExpanded);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](58, 43, "SETTINGS.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](61, 45, "PROJECTS.SETTINGS_CARD_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx.isP2PAvailable);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](68, 47, "PROJECTS.NET_LOG_TOOLTIP"));
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵpipeBind1"](72, 49, "PROJECTS.NET_LOG"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_17__["ɵɵproperty"]("ngIf", ctx.isElectron());
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_25__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_25__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_26__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_26__.MatLegacyPrefix, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_26__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_27__.MatLegacyInput, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_28__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_28__.MatLegacyCardContent, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_29__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_30__.MatIcon, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_31__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_32__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_32__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_32__.NgModel, _components_participant_gems_participant_gems_component__WEBPACK_IMPORTED_MODULE_16__.ParticipantGemsComponent, _angular_common__WEBPACK_IMPORTED_MODULE_25__.AsyncPipe, _angular_common__WEBPACK_IMPORTED_MODULE_25__.DatePipe, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_24__.TranslatePipe],
      styles: [".modern-container[_ngcontent-%COMP%] {\n  min-height: 100vh;\n  background: var(--mde-bg-secondary);\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n  overflow-x: hidden;\n  overflow-y: auto;\n}\n\n.content-wrapper[_ngcontent-%COMP%] {\n  flex: 1;\n  background: var(--mde-bg-secondary);\n  padding: 40px;\n  max-width: 1600px;\n  margin: 0 auto;\n  width: 100%;\n  min-height: calc(100vh - 36px);\n  box-sizing: border-box;\n}\n\n.main-layout[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 400px;\n  gap: 32px;\n  align-items: start;\n  height: calc(100vh - 116px);\n}\n\n.left-column[_ngcontent-%COMP%] {\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  max-height: 100%;\n}\n\n.right-column[_ngcontent-%COMP%] {\n  position: sticky;\n  top: 40px;\n  align-self: start;\n}\n\n.actions-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.action-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 16px;\n  border: none;\n  box-shadow: 0 2px 8px var(--mde-shadow-color);\n  overflow: hidden;\n  background: var(--mde-bg-primary);\n}\n.action-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);\n}\n.action-card[_ngcontent-%COMP%]:hover   .arrow-icon[_ngcontent-%COMP%] {\n  transform: translateX(4px);\n}\n.action-card[_ngcontent-%COMP%]:hover   .icon-wrapper[_ngcontent-%COMP%] {\n  transform: scale(1.1);\n}\n.action-card[_ngcontent-%COMP%]:active {\n  transform: translateY(-2px);\n}\n.action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 16px;\n  padding: 20px 16px;\n  position: relative;\n}\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 48px;\n  height: 48px;\n  border-radius: 12px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: transform 0.3s ease;\n}\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: white;\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-title[_ngcontent-%COMP%] {\n  margin: 0 0 4px 0;\n  font-size: 1rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.75rem;\n  color: var(--mde-text-secondary);\n  line-height: 1.4;\n}\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%]   .arrow-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: transform 0.3s ease;\n}\n.action-card.clone-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n}\n.action-card.create-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);\n}\n.action-card.settings-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);\n}\n.action-card.p2p-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);\n}\n\n.section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n  margin: 0 0 24px 0;\n}\n.section-title[_ngcontent-%COMP%]   .section-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n  color: #667eea;\n}\n\n.section-divider[_ngcontent-%COMP%] {\n  height: 1px;\n  background: linear-gradient(to right, transparent, var(--mde-border-color), transparent);\n  margin: 32px 0;\n}\n\n.recent-projects-section[_ngcontent-%COMP%] {\n  margin-bottom: 0;\n  display: flex;\n  flex-direction: column;\n  flex: 1;\n  min-height: 0;\n}\n\n.section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 24px;\n  gap: 24px;\n  flex-shrink: 0;\n}\n.section-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  margin: 0;\n  flex-shrink: 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n  width: 100%;\n  max-width: 400px;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n  margin-bottom: 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-infix[_ngcontent-%COMP%] {\n  padding: 0.5em 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-underline[_ngcontent-%COMP%] {\n  display: none;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #718096;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.no-projects-message[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 60px 20px;\n  color: #718096;\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n}\n.no-projects-message[_ngcontent-%COMP%]   .empty-icon[_ngcontent-%COMP%] {\n  font-size: 80px;\n  width: 80px;\n  height: 80px;\n  color: #cbd5e0;\n  margin: 0 auto 20px;\n}\n.no-projects-message[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #4a5568;\n  margin: 0 0 8px 0;\n}\n.no-projects-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n  flex: 1;\n  overflow-y: auto;\n  padding: 0 8px 16px 0;\n  min-height: 0;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: var(--mde-scrollbar-track);\n  border-radius: 4px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: var(--mde-scrollbar-thumb);\n  border-radius: 4px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: var(--mde-scrollbar-hover);\n}\n\n.project-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 12px;\n  border: 1px solid var(--mde-border-color);\n  box-shadow: 0 1px 3px var(--mde-shadow-color);\n  background: var(--mde-bg-primary);\n  position: relative;\n  overflow: visible;\n}\n.project-card[_ngcontent-%COMP%]   .card-gems[_ngcontent-%COMP%] {\n  position: absolute;\n  bottom: -11px;\n  left: 20px;\n  z-index: 3;\n  pointer-events: auto;\n}\n.project-card.has-gems[_ngcontent-%COMP%] {\n  margin-bottom: 4px;\n}\n.project-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);\n  border-color: #667eea;\n}\n.project-card.last-opened[_ngcontent-%COMP%] {\n  border: 2px solid #667eea;\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);\n  background: linear-gradient(135deg, rgba(102, 126, 234, 0.03) 0%, rgba(118, 75, 162, 0.03) 100%);\n}\n.project-card.last-opened[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(102, 126, 234, 0.3);\n  border-color: #764ba2;\n}\n.project-card.last-opened[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);\n  animation: _ngcontent-%COMP%_pulse 2s ease-in-out infinite;\n}\n.project-card[_ngcontent-%COMP%]   .project-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: stretch;\n  padding: 12px 16px;\n  gap: 14px;\n}\n.project-card[_ngcontent-%COMP%]   .project-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 0 0 auto;\n  width: clamp(220px, 34%, 340px);\n  min-width: 0;\n  cursor: pointer;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n  min-width: 0;\n  display: flex;\n  align-items: center;\n  padding: 8px 12px;\n  border: 1px solid var(--mde-border-color);\n  border-radius: 8px;\n  background: var(--mde-bg-secondary);\n  cursor: pointer;\n  transition: border-color 0.2s ease, background 0.2s ease;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%]:hover {\n  border-color: rgba(102, 126, 234, 0.5);\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%]   .project-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: var(--mde-text-primary);\n  line-height: 1.4;\n  display: -webkit-box;\n  -webkit-line-clamp: 3;\n  -webkit-box-orient: vertical;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  word-break: break-word;\n  width: 100%;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%] {\n  border-style: dashed;\n  background: transparent;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]:hover {\n  background: rgba(102, 126, 234, 0.04);\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]   .empty-placeholder[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n  color: var(--mde-text-secondary);\n  opacity: 0.6;\n  font-size: 0.78rem;\n  font-style: italic;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]   .empty-placeholder[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 40px;\n  height: 40px;\n  border-radius: 10px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  overflow: hidden;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: white;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon-svg[_ngcontent-%COMP%] {\n  width: 20px;\n  height: 20px;\n  filter: brightness(0) invert(1);\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper.has-custom[_ngcontent-%COMP%] {\n  background: var(--mde-bg-secondary);\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper.has-custom[_ngcontent-%COMP%]   .project-icon-custom[_ngcontent-%COMP%] {\n  width: 100%;\n  height: 100%;\n  object-fit: cover;\n  display: block;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  gap: 2px;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  min-width: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.1rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  min-width: 0;\n  flex-shrink: 1;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-path[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: var(--mde-text-secondary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n  padding: 3px 8px;\n  border-radius: 10px;\n  font-size: 0.6rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.4px;\n  white-space: nowrap;\n  box-shadow: 0 2px 6px rgba(102, 126, 234, 0.3);\n  animation: _ngcontent-%COMP%_fadeInScale 0.5s ease-out;\n  flex-shrink: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%]   .badge-icon[_ngcontent-%COMP%] {\n  font-size: 11px;\n  width: 11px;\n  height: 11px;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: repeat(2, 32px);\n  grid-auto-rows: 32px;\n  gap: 2px;\n  flex-shrink: 0;\n  align-self: center;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%] {\n  transition: all 0.2s ease;\n  width: 32px;\n  height: 32px;\n  line-height: 32px;\n  padding: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]     .mat-mdc-button-touch-target {\n  width: 32px !important;\n  height: 32px !important;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.edit-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(255, 152, 0, 0.12);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.edit-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(76, 175, 80, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(102, 126, 234, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #667eea;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(245, 87, 108, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #f5576c;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  line-height: 18px;\n  color: #a0aec0;\n  transition: color 0.2s ease;\n}\n\n.sidebar-section[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n}\n\n.actions-backdrop[_ngcontent-%COMP%] {\n  display: none;\n}\n\n@media (max-width: 1024px) {\n  .main-layout[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr 72px;\n    gap: 16px;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .section-title[_ngcontent-%COMP%], .right-column[_ngcontent-%COMP%]:not(.expanded)   .diagnostics-section[_ngcontent-%COMP%] {\n    display: none;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .actions-grid[_ngcontent-%COMP%] {\n    gap: 12px;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .action-card[_ngcontent-%COMP%] {\n    border-radius: 14px;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .action-card[_ngcontent-%COMP%]:hover {\n    transform: none;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n    justify-content: center;\n    padding: 10px;\n  }\n  .right-column[_ngcontent-%COMP%]:not(.expanded)   .action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%], .right-column[_ngcontent-%COMP%]:not(.expanded)   .action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%] {\n    display: none;\n  }\n  .right-column.expanded[_ngcontent-%COMP%] {\n    position: fixed;\n    top: 52px;\n    right: 16px;\n    width: 400px;\n    max-width: calc(100vw - 32px);\n    max-height: calc(100vh - 72px);\n    overflow-y: auto;\n    box-sizing: border-box;\n    z-index: 1001;\n    padding: 20px;\n    border-radius: 16px;\n    background: var(--mde-bg-secondary);\n    box-shadow: 0 12px 32px var(--mde-shadow-color);\n  }\n  .actions-backdrop[_ngcontent-%COMP%] {\n    display: block;\n    position: fixed;\n    inset: 0;\n    z-index: 1000;\n    background: rgba(0, 0, 0, 0.25);\n  }\n}\n@media (max-width: 768px) {\n  .header-section[_ngcontent-%COMP%] {\n    padding: 40px 20px 20px;\n  }\n  .header-section[_ngcontent-%COMP%]   .main-title[_ngcontent-%COMP%] {\n    font-size: 2rem;\n  }\n  .header-section[_ngcontent-%COMP%]   .subtitle[_ngcontent-%COMP%] {\n    font-size: 1rem;\n  }\n  .content-wrapper[_ngcontent-%COMP%] {\n    padding: 32px 20px;\n    border-radius: 24px 24px 0 0;\n  }\n  .right-column.expanded[_ngcontent-%COMP%]   .action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n    padding: 24px 20px;\n  }\n  .section-title[_ngcontent-%COMP%] {\n    font-size: 1.25rem;\n  }\n  .section-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: flex-start;\n  }\n  .section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n    max-width: 100%;\n  }\n}\n@keyframes _ngcontent-%COMP%_pulse {\n  0%, 100% {\n    transform: scale(1);\n  }\n  50% {\n    transform: scale(1.05);\n  }\n}\n@keyframes _ngcontent-%COMP%_fadeInScale {\n  0% {\n    opacity: 0;\n    transform: scale(0.8);\n  }\n  100% {\n    opacity: 1;\n    transform: scale(1);\n  }\n}\n.diagnostics-section[_ngcontent-%COMP%] {\n  margin-top: 24px;\n  padding-top: 16px;\n  border-top: 1px solid var(--mde-border-color);\n  display: flex;\n  gap: 12px;\n  flex-wrap: wrap;\n}\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n}\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  margin-right: 6px;\n}\n\n.startup-notices[_ngcontent-%COMP%] {\n  margin: 8px 0 16px;\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.startup-notice[_ngcontent-%COMP%] {\n  display: flex;\n  flex-wrap: wrap;\n  align-items: center;\n  gap: 10px;\n  padding: 10px 14px;\n  border-radius: 10px;\n  background: #e8f0fe;\n  color: #1a3d7c;\n  border-left: 6px solid #1a73e8;\n}\n.startup-notice[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  flex: 0 0 auto;\n}\n.startup-notice[_ngcontent-%COMP%]   .notice-text[_ngcontent-%COMP%] {\n  flex: 1 1 320px;\n  min-width: 240px;\n}\n.startup-notice[_ngcontent-%COMP%]   .notice-text[_ngcontent-%COMP%] {\n  line-height: 1.35;\n}\n.startup-notice.level-warning[_ngcontent-%COMP%] {\n  background: #fff3e0;\n  color: #7a4a00;\n  border-left-color: #f29900;\n}\n.startup-notice.level-error[_ngcontent-%COMP%] {\n  background: #fdecea;\n  color: #8a1c13;\n  border-left-color: #d93025;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvcHJvamVjdHMuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxpQkFBQTtFQUNBLG1DQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsVUFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFBRjs7QUFJQTtFQUNFLE9BQUE7RUFDQSxtQ0FBQTtFQUNBLGFBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7RUFDQSxXQUFBO0VBQ0EsOEJBQUE7RUFDQSxzQkFBQTtBQURGOztBQUtBO0VBQ0UsYUFBQTtFQUNBLGdDQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsMkJBQUE7QUFGRjs7QUFLQTtFQUNFLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFGRjs7QUFLQTtFQUNFLGdCQUFBO0VBQ0EsU0FBQTtFQUNBLGlCQUFBO0FBRkY7O0FBTUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0FBSEY7O0FBT0E7RUFDRSxlQUFBO0VBQ0EsaURBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7RUFDQSw2Q0FBQTtFQUNBLGdCQUFBO0VBQ0EsaUNBQUE7QUFKRjtBQU1FO0VBQ0UsMkJBQUE7RUFDQSwyQ0FBQTtBQUpKO0FBTUk7RUFDRSwwQkFBQTtBQUpOO0FBT0k7RUFDRSxxQkFBQTtBQUxOO0FBU0U7RUFDRSwyQkFBQTtBQVBKO0FBVUU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtBQVJKO0FBV0U7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsK0JBQUE7QUFUSjtBQVdJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsWUFBQTtBQVROO0FBYUU7RUFDRSxPQUFBO0FBWEo7QUFhSTtFQUNFLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsOEJBQUE7QUFYTjtBQWNJO0VBQ0UsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxnQkFBQTtBQVpOO0FBZ0JFO0VBQ0UsY0FBQTtBQWRKO0FBZ0JJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLCtCQUFBO0FBZE47QUFtQkU7RUFDRSw2REFBQTtBQWpCSjtBQW9CRTtFQUNFLDZEQUFBO0FBbEJKO0FBcUJFO0VBQ0UsNkRBQUE7QUFuQko7QUFzQkU7RUFDRSw2REFBQTtBQXBCSjs7QUF5QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLDhCQUFBO0VBQ0Esa0JBQUE7QUF0QkY7QUF3QkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBdEJKOztBQTJCQTtFQUNFLFdBQUE7RUFDQSx3RkFBQTtFQUNBLGNBQUE7QUF4QkY7O0FBNEJBO0VBQ0UsZ0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxPQUFBO0VBQ0EsYUFBQTtBQXpCRjs7QUE2QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGNBQUE7QUExQkY7QUE0QkU7RUFDRSxTQUFBO0VBQ0EsY0FBQTtBQTFCSjtBQTZCRTtFQUNFLFdBQUE7RUFDQSxnQkFBQTtBQTNCSjtBQThCSTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7QUE1Qk47QUErQkk7RUFDRSxnQkFBQTtBQTdCTjtBQWdDSTtFQUNFLGFBQUE7QUE5Qk47QUFpQ0k7RUFDRSxlQUFBO0FBL0JOO0FBa0NJO0VBQ0UsY0FBQTtBQWhDTjtBQW1DSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQWpDTjs7QUF1Q0E7RUFDRSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBcENGO0FBc0NFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLG1CQUFBO0FBcENKO0FBdUNFO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxpQkFBQTtBQXJDSjtBQXdDRTtFQUNFLGVBQUE7RUFDQSxTQUFBO0FBdENKOztBQTBDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUFDQSxPQUFBO0VBQ0EsZ0JBQUE7RUFHQSxxQkFBQTtFQUNBLGFBQUE7QUF6Q0Y7QUE0Q0U7RUFDRSxVQUFBO0FBMUNKO0FBNkNFO0VBQ0Usc0NBQUE7RUFDQSxrQkFBQTtBQTNDSjtBQThDRTtFQUNFLHNDQUFBO0VBQ0Esa0JBQUE7QUE1Q0o7QUE4Q0k7RUFDRSxzQ0FBQTtBQTVDTjs7QUFpREE7RUFDRSxlQUFBO0VBQ0EsaURBQUE7RUFDQSxtQkFBQTtFQUNBLHlDQUFBO0VBQ0EsNkNBQUE7RUFDQSxpQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsaUJBQUE7QUE5Q0Y7QUFtREU7RUFDRSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxVQUFBO0VBQ0EsVUFBQTtFQUNBLG9CQUFBO0FBakRKO0FBb0RFO0VBRUUsa0JBQUE7QUFuREo7QUFzREU7RUFDRSwyQkFBQTtFQUNBLHlDQUFBO0VBQ0EscUJBQUE7QUFwREo7QUF3REU7RUFDRSx5QkFBQTtFQUNBLCtDQUFBO0VBQ0EsZ0dBQUE7QUF0REo7QUF3REk7RUFDRSwyQkFBQTtFQUNBLGdEQUFBO0VBQ0EscUJBQUE7QUF0RE47QUF5REk7RUFDRSw2REFBQTtFQUNBLCtDQUFBO0VBQ0Esd0NBQUE7QUF2RE47QUEyREU7RUFDRSxhQUFBO0VBQ0Esb0JBQUE7RUFDQSxrQkFBQTtFQUNBLFNBQUE7QUF6REo7QUE0REU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsY0FBQTtFQUNBLCtCQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7QUExREo7QUE2REU7RUFDRSxjQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGlCQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLG1DQUFBO0VBQ0EsZUFBQTtFQUNBLHdEQUFBO0FBM0RKO0FBNkRJO0VBQ0Usc0NBQUE7QUEzRE47QUE4REk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSw4QkFBQTtFQUNBLGdCQUFBO0VBQ0Esb0JBQUE7RUFDQSxxQkFBQTtFQUNBLDRCQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLHNCQUFBO0VBQ0EsV0FBQTtBQTVETjtBQStESTtFQUNFLG9CQUFBO0VBQ0EsdUJBQUE7QUE3RE47QUErRE07RUFDRSxxQ0FBQTtBQTdEUjtBQWdFTTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZ0NBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtBQTlEUjtBQWdFUTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQTlEVjtBQW9FRTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0EsNkRBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGdCQUFBO0FBbEVKO0FBb0VJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsWUFBQTtBQWxFTjtBQXFFSTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsK0JBQUE7QUFuRU47QUF3RUk7RUFDRSxtQ0FBQTtBQXRFTjtBQXdFTTtFQUNFLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBdEVSO0FBMkVFO0VBQ0UsT0FBQTtFQUNBLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBekVKO0FBMkVJO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLFlBQUE7QUF6RU47QUE0RUk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLDhCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUExRU47QUE2RUk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQ0FBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtBQTNFTjtBQThFSTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsNkRBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7RUFDQSxtQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSx5QkFBQTtFQUNBLHFCQUFBO0VBQ0EsbUJBQUE7RUFDQSw4Q0FBQTtFQUNBLG9DQUFBO0VBQ0EsY0FBQTtBQTVFTjtBQThFTTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQTVFUjtBQWlGRTtFQUNFLGFBQUE7RUFDQSxzQ0FBQTtFQUNBLG9CQUFBO0VBQ0EsUUFBQTtFQUNBLGNBQUE7RUFDQSxrQkFBQTtBQS9FSjtBQWlGSTtFQUNFLHlCQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLFVBQUE7QUEvRU47QUFrRk07RUFDRSxzQkFBQTtFQUNBLHVCQUFBO0FBaEZSO0FBbUZNO0VBQ0UseUNBQUE7QUFqRlI7QUFrRlE7RUFDRSxjQUFBO0FBaEZWO0FBb0ZNO0VBQ0Usd0NBQUE7QUFsRlI7QUFtRlE7RUFDRSxjQUFBO0FBakZWO0FBcUZNO0VBQ0UsMENBQUE7QUFuRlI7QUFvRlE7RUFDRSxjQUFBO0FBbEZWO0FBc0ZNO0VBQ0UseUNBQUE7QUFwRlI7QUFxRlE7RUFDRSxjQUFBO0FBbkZWO0FBdUZNO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsMkJBQUE7QUFyRlI7O0FBNEZBO0VBQ0UsbUJBQUE7QUF6RkY7O0FBOEZBO0VBQ0UsYUFBQTtBQTNGRjs7QUFnR0E7RUFDRTtJQUNFLCtCQUFBO0lBQ0EsU0FBQTtFQTdGRjtFQWlHRTs7SUFFRSxhQUFBO0VBL0ZKO0VBa0dFO0lBQ0UsU0FBQTtFQWhHSjtFQW1HRTtJQUNFLG1CQUFBO0VBakdKO0VBbUdJO0lBQ0UsZUFBQTtFQWpHTjtFQW9HSTtJQUNFLHVCQUFBO0lBQ0EsYUFBQTtFQWxHTjtFQXFHSTs7SUFFRSxhQUFBO0VBbkdOO0VBd0dBO0lBQ0UsZUFBQTtJQUNBLFNBQUE7SUFDQSxXQUFBO0lBQ0EsWUFBQTtJQUNBLDZCQUFBO0lBQ0EsOEJBQUE7SUFDQSxnQkFBQTtJQUNBLHNCQUFBO0lBQ0EsYUFBQTtJQUNBLGFBQUE7SUFDQSxtQkFBQTtJQUNBLG1DQUFBO0lBQ0EsK0NBQUE7RUF0R0Y7RUF5R0E7SUFDRSxjQUFBO0lBQ0EsZUFBQTtJQUNBLFFBQUE7SUFDQSxhQUFBO0lBQ0EsK0JBQUE7RUF2R0Y7QUFDRjtBQTBHQTtFQUNFO0lBQ0UsdUJBQUE7RUF4R0Y7RUEwR0U7SUFDRSxlQUFBO0VBeEdKO0VBMkdFO0lBQ0UsZUFBQTtFQXpHSjtFQTZHQTtJQUNFLGtCQUFBO0lBQ0EsNEJBQUE7RUEzR0Y7RUE4R0E7SUFDRSxrQkFBQTtFQTVHRjtFQStHQTtJQUNFLGtCQUFBO0VBN0dGO0VBZ0hBO0lBQ0Usc0JBQUE7SUFDQSx1QkFBQTtFQTlHRjtFQWdIRTtJQUNFLGVBQUE7RUE5R0o7QUFDRjtBQW1IQTtFQUNFO0lBQ0UsbUJBQUE7RUFqSEY7RUFtSEE7SUFDRSxzQkFBQTtFQWpIRjtBQUNGO0FBb0hBO0VBQ0U7SUFDRSxVQUFBO0lBQ0EscUJBQUE7RUFsSEY7RUFvSEE7SUFDRSxVQUFBO0lBQ0EsbUJBQUE7RUFsSEY7QUFDRjtBQXNIQTtFQUNFLGdCQUFBO0VBQ0EsaUJBQUE7RUFDQSw2Q0FBQTtFQUNBLGFBQUE7RUFDQSxTQUFBO0VBQ0EsZUFBQTtBQXBIRjtBQXNIRTtFQUNFLGtCQUFBO0FBcEhKO0FBcUhJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7QUFuSE47O0FBeUhBO0VBQW1CLGtCQUFBO0VBQW9CLGFBQUE7RUFBZSxzQkFBQTtFQUF3QixRQUFBO0FBbEg5RTs7QUFtSEE7RUFDRSxhQUFBO0VBQWUsZUFBQTtFQUFpQixtQkFBQTtFQUFxQixTQUFBO0VBQVcsa0JBQUE7RUFBb0IsbUJBQUE7RUFHcEYsbUJBQUE7RUFBcUIsY0FBQTtFQUFnQiw4QkFBQTtBQTNHdkM7QUF5R0U7RUFBVyxjQUFBO0FBdEdiO0FBdUdFO0VBQWUsZUFBQTtFQUFpQixnQkFBQTtBQW5HbEM7QUFxR0U7RUFBZSxpQkFBQTtBQWxHakI7QUFtR0U7RUFBa0IsbUJBQUE7RUFBcUIsY0FBQTtFQUFnQiwwQkFBQTtBQTlGekQ7QUErRkU7RUFBZ0IsbUJBQUE7RUFBcUIsY0FBQTtFQUFnQiwwQkFBQTtBQTFGdkQiLCJzb3VyY2VzQ29udGVudCI6WyIvLyBNb2Rlcm4gY29udGFpbmVyIGxheW91dFxuLm1vZGVybi1jb250YWluZXIge1xuICBtaW4taGVpZ2h0OiAxMDB2aDtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXNlY29uZGFyeSk7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIHBhZGRpbmc6IDA7XG4gIG92ZXJmbG93LXg6IGhpZGRlbjtcbiAgb3ZlcmZsb3cteTogYXV0bztcbn1cblxuLy8gQ29udGVudCB3cmFwcGVyXG4uY29udGVudC13cmFwcGVyIHtcbiAgZmxleDogMTtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXNlY29uZGFyeSk7XG4gIHBhZGRpbmc6IDQwcHg7XG4gIG1heC13aWR0aDogMTYwMHB4O1xuICBtYXJnaW46IDAgYXV0bztcbiAgd2lkdGg6IDEwMCU7XG4gIG1pbi1oZWlnaHQ6IGNhbGMoMTAwdmggLSAzNnB4KTtcbiAgYm94LXNpemluZzogYm9yZGVyLWJveDtcbn1cblxuLy8gTWFpbiBsYXlvdXQgLSBUd28gY29sdW1uc1xuLm1haW4tbGF5b3V0IHtcbiAgZGlzcGxheTogZ3JpZDtcbiAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnIgNDAwcHg7XG4gIGdhcDogMzJweDtcbiAgYWxpZ24taXRlbXM6IHN0YXJ0O1xuICBoZWlnaHQ6IGNhbGMoMTAwdmggLSAxMTZweCk7IC8vIDM2cHggdGl0bGUgYmFyICsgODBweCBwYWRkaW5nXG59XG5cbi5sZWZ0LWNvbHVtbiB7XG4gIG1pbi13aWR0aDogMDtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgaGVpZ2h0OiAxMDAlO1xuICBtYXgtaGVpZ2h0OiAxMDAlO1xufVxuXG4ucmlnaHQtY29sdW1uIHtcbiAgcG9zaXRpb246IHN0aWNreTtcbiAgdG9wOiA0MHB4O1xuICBhbGlnbi1zZWxmOiBzdGFydDtcbn1cblxuLy8gQWN0aW9uIGNhcmRzIGdyaWQgKGluIHJpZ2h0IGNvbHVtbiAtIHZlcnRpY2FsIHN0YWNrKVxuLmFjdGlvbnMtZ3JpZCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogMTZweDtcbn1cblxuLy8gQWN0aW9uIGNhcmQgc3R5bGluZ1xuLmFjdGlvbi1jYXJkIHtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4zcyBjdWJpYy1iZXppZXIoMC40LCAwLCAwLjIsIDEpO1xuICBib3JkZXItcmFkaXVzOiAxNnB4O1xuICBib3JkZXI6IG5vbmU7XG4gIGJveC1zaGFkb3c6IDAgMnB4IDhweCB2YXIoLS1tZGUtc2hhZG93LWNvbG9yKTtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXByaW1hcnkpO1xuXG4gICY6aG92ZXIge1xuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtNHB4KTtcbiAgICBib3gtc2hhZG93OiAwIDEycHggMjRweCByZ2JhKDAsIDAsIDAsIDAuMTUpO1xuXG4gICAgLmFycm93LWljb24ge1xuICAgICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVYKDRweCk7XG4gICAgfVxuXG4gICAgLmljb24td3JhcHBlciB7XG4gICAgICB0cmFuc2Zvcm06IHNjYWxlKDEuMSk7XG4gICAgfVxuICB9XG5cbiAgJjphY3RpdmUge1xuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcbiAgfVxuXG4gIC5jYXJkLWNvbnRlbnQge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBnYXA6IDE2cHg7XG4gICAgcGFkZGluZzogMjBweCAxNnB4O1xuICAgIHBvc2l0aW9uOiByZWxhdGl2ZTtcbiAgfVxuXG4gIC5pY29uLXdyYXBwZXIge1xuICAgIGZsZXgtc2hyaW5rOiAwO1xuICAgIHdpZHRoOiA0OHB4O1xuICAgIGhlaWdodDogNDhweDtcbiAgICBib3JkZXItcmFkaXVzOiAxMnB4O1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcbiAgICB0cmFuc2l0aW9uOiB0cmFuc2Zvcm0gMC4zcyBlYXNlO1xuXG4gICAgbWF0LWljb24ge1xuICAgICAgZm9udC1zaXplOiAyNHB4O1xuICAgICAgd2lkdGg6IDI0cHg7XG4gICAgICBoZWlnaHQ6IDI0cHg7XG4gICAgICBjb2xvcjogd2hpdGU7XG4gICAgfVxuICB9XG5cbiAgLmNhcmQtdGV4dCB7XG4gICAgZmxleDogMTtcblxuICAgIC5jYXJkLXRpdGxlIHtcbiAgICAgIG1hcmdpbjogMCAwIDRweCAwO1xuICAgICAgZm9udC1zaXplOiAxcmVtO1xuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcbiAgICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcbiAgICB9XG5cbiAgICAuY2FyZC1kZXNjcmlwdGlvbiB7XG4gICAgICBtYXJnaW46IDA7XG4gICAgICBmb250LXNpemU6IDAuNzVyZW07XG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XG4gICAgfVxuICB9XG5cbiAgLmNhcmQtYWN0aW9uIHtcbiAgICBmbGV4LXNocmluazogMDtcblxuICAgIC5hcnJvdy1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcbiAgICAgIHdpZHRoOiAyMHB4O1xuICAgICAgaGVpZ2h0OiAyMHB4O1xuICAgICAgY29sb3I6ICNhMGFlYzA7XG4gICAgICB0cmFuc2l0aW9uOiB0cmFuc2Zvcm0gMC4zcyBlYXNlO1xuICAgIH1cbiAgfVxuXG4gIC8vIFNwZWNpZmljIGNhcmQgY29sb3JzXG4gICYuY2xvbmUtY2FyZCAuaWNvbi13cmFwcGVyIHtcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xuICB9XG5cbiAgJi5jcmVhdGUtY2FyZCAuaWNvbi13cmFwcGVyIHtcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjZjA5M2ZiIDAlLCAjZjU1NzZjIDEwMCUpO1xuICB9XG5cbiAgJi5zZXR0aW5ncy1jYXJkIC5pY29uLXdyYXBwZXIge1xuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM0ZmFjZmUgMCUsICMwMGYyZmUgMTAwJSk7XG4gIH1cblxuICAmLnAycC1jYXJkIC5pY29uLXdyYXBwZXIge1xuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICMxMTk5OGUgMCUsICMzOGVmN2QgMTAwJSk7XG4gIH1cbn1cblxuLy8gU2VjdGlvbiB0aXRsZVxuLnNlY3Rpb24tdGl0bGUge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDEycHg7XG4gIGZvbnQtc2l6ZTogMS41cmVtO1xuICBmb250LXdlaWdodDogNTAwO1xuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtcHJpbWFyeSk7XG4gIG1hcmdpbjogMCAwIDI0cHggMDtcblxuICAuc2VjdGlvbi1pY29uIHtcbiAgICBmb250LXNpemU6IDI4cHg7XG4gICAgd2lkdGg6IDI4cHg7XG4gICAgaGVpZ2h0OiAyOHB4O1xuICAgIGNvbG9yOiAjNjY3ZWVhO1xuICB9XG59XG5cbi8vIFNlY3Rpb24gZGl2aWRlclxuLnNlY3Rpb24tZGl2aWRlciB7XG4gIGhlaWdodDogMXB4O1xuICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQodG8gcmlnaHQsIHRyYW5zcGFyZW50LCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKSwgdHJhbnNwYXJlbnQpO1xuICBtYXJnaW46IDMycHggMDtcbn1cblxuLy8gUmVjZW50IFByb2plY3RzIFNlY3Rpb25cbi5yZWNlbnQtcHJvamVjdHMtc2VjdGlvbiB7XG4gIG1hcmdpbi1ib3R0b206IDA7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGZsZXg6IDE7XG4gIG1pbi1oZWlnaHQ6IDA7XG59XG5cbi8vIFNlY3Rpb24gaGVhZGVyIHdpdGggc2VhcmNoXG4uc2VjdGlvbi1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XG4gIG1hcmdpbi1ib3R0b206IDI0cHg7XG4gIGdhcDogMjRweDtcbiAgZmxleC1zaHJpbms6IDA7XG5cbiAgLnNlY3Rpb24tdGl0bGUge1xuICAgIG1hcmdpbjogMDtcbiAgICBmbGV4LXNocmluazogMDtcbiAgfVxuXG4gIC5zZWFyY2gtZmllbGQge1xuICAgIHdpZHRoOiAxMDAlO1xuICAgIG1heC13aWR0aDogNDAwcHg7XG5cbiAgICAvLyBPdmVycmlkZSBNYXRlcmlhbCBEZXNpZ24gZGVmYXVsdCBtYXJnaW5zXG4gICAgLm1hdC1mb3JtLWZpZWxkLXdyYXBwZXIge1xuICAgICAgcGFkZGluZy1ib3R0b206IDA7XG4gICAgICBtYXJnaW4tYm90dG9tOiAwO1xuICAgIH1cblxuICAgIC5tYXQtZm9ybS1maWVsZC1pbmZpeCB7XG4gICAgICBwYWRkaW5nOiAwLjVlbSAwO1xuICAgIH1cblxuICAgIC5tYXQtZm9ybS1maWVsZC11bmRlcmxpbmUge1xuICAgICAgZGlzcGxheTogbm9uZTtcbiAgICB9XG5cbiAgICBpbnB1dCB7XG4gICAgICBmb250LXNpemU6IDE0cHg7XG4gICAgfVxuXG4gICAgbWF0LWljb24ge1xuICAgICAgY29sb3I6ICM3MTgwOTY7XG4gICAgfVxuXG4gICAgYnV0dG9uIG1hdC1pY29uIHtcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgIHdpZHRoOiAxOHB4O1xuICAgICAgaGVpZ2h0OiAxOHB4O1xuICAgIH1cbiAgfVxufVxuXG4vLyBObyBwcm9qZWN0cyBtZXNzYWdlXG4ubm8tcHJvamVjdHMtbWVzc2FnZSB7XG4gIHRleHQtYWxpZ246IGNlbnRlcjtcbiAgcGFkZGluZzogNjBweCAyMHB4O1xuICBjb2xvcjogIzcxODA5NjtcbiAgZmxleDogMTtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG5cbiAgLmVtcHR5LWljb24ge1xuICAgIGZvbnQtc2l6ZTogODBweDtcbiAgICB3aWR0aDogODBweDtcbiAgICBoZWlnaHQ6IDgwcHg7XG4gICAgY29sb3I6ICNjYmQ1ZTA7XG4gICAgbWFyZ2luOiAwIGF1dG8gMjBweDtcbiAgfVxuXG4gIGgzIHtcbiAgICBmb250LXNpemU6IDEuNXJlbTtcbiAgICBmb250LXdlaWdodDogNTAwO1xuICAgIGNvbG9yOiAjNGE1NTY4O1xuICAgIG1hcmdpbjogMCAwIDhweCAwO1xuICB9XG5cbiAgcCB7XG4gICAgZm9udC1zaXplOiAxcmVtO1xuICAgIG1hcmdpbjogMDtcbiAgfVxufVxuXG4ucHJvamVjdHMtZ3JpZCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogMTZweDtcbiAgZmxleDogMTtcbiAgb3ZlcmZsb3cteTogYXV0bztcbiAgLy8gRXh0cmEgcmlnaHQvYm90dG9tIHBhZGRpbmcgc28gdGVhbSBnZW1zIHNpdHRpbmcgb24gdGhlIGJvdHRvbSBib3JkZXIgb2ZcbiAgLy8gZWFjaCBjYXJkIGRvbid0IGdldCBjbGlwcGVkIGJ5IHRoZSBzY3JvbGwgY29udGFpbmVyLlxuICBwYWRkaW5nOiAwIDhweCAxNnB4IDA7XG4gIG1pbi1oZWlnaHQ6IDA7XG5cbiAgLy8gQ3VzdG9tIHNjcm9sbGJhclxuICAmOjotd2Via2l0LXNjcm9sbGJhciB7XG4gICAgd2lkdGg6IDhweDtcbiAgfVxuXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRyYWNrIHtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtc2Nyb2xsYmFyLXRyYWNrKTtcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIH1cblxuICAmOjotd2Via2l0LXNjcm9sbGJhci10aHVtYiB7XG4gICAgYmFja2dyb3VuZDogdmFyKC0tbWRlLXNjcm9sbGJhci10aHVtYik7XG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xuXG4gICAgJjpob3ZlciB7XG4gICAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtc2Nyb2xsYmFyLWhvdmVyKTtcbiAgICB9XG4gIH1cbn1cblxuLnByb2plY3QtY2FyZCB7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgdHJhbnNpdGlvbjogYWxsIDAuM3MgY3ViaWMtYmV6aWVyKDAuNCwgMCwgMC4yLCAxKTtcbiAgYm9yZGVyLXJhZGl1czogMTJweDtcbiAgYm9yZGVyOiAxcHggc29saWQgdmFyKC0tbWRlLWJvcmRlci1jb2xvcik7XG4gIGJveC1zaGFkb3c6IDAgMXB4IDNweCB2YXIoLS1tZGUtc2hhZG93LWNvbG9yKTtcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXByaW1hcnkpO1xuICBwb3NpdGlvbjogcmVsYXRpdmU7XG4gIG92ZXJmbG93OiB2aXNpYmxlO1xuXG4gIC8vIFRlYW0gZ2VtcyBhcmUgYWJzb2x1dGUtcG9zaXRpb25lZCBzbyB0aGV5IHNpdCBoYWxmLWVtYmVkZGVkIG9uIHRoZSBib3R0b21cbiAgLy8gYm9yZGVyIG9mIHRoZSBjYXJkIHdpdGhvdXQgY2hhbmdpbmcgaXRzIGhlaWdodC4gb3ZlcmZsb3c6IHZpc2libGUgYWJvdmVcbiAgLy8gYWxyZWFkeSBsZXRzIHRoZW0gcmVuZGVyIG91dHNpZGUgdGhlIGNhcmQgYm91bmRzLlxuICAuY2FyZC1nZW1zIHtcbiAgICBwb3NpdGlvbjogYWJzb2x1dGU7XG4gICAgYm90dG9tOiAtMTFweDtcbiAgICBsZWZ0OiAyMHB4O1xuICAgIHotaW5kZXg6IDM7XG4gICAgcG9pbnRlci1ldmVudHM6IGF1dG87XG4gIH1cblxuICAmLmhhcy1nZW1zIHtcbiAgICAvLyBTbWFsbCBleHRyYSBtYXJnaW4gb24gdGhlIGdyaWQgcm93IHNvIGdlbXMgZnJvbSBhZGphY2VudCBjYXJkcyBuZXZlciBvdmVybGFwLlxuICAgIG1hcmdpbi1ib3R0b206IDRweDtcbiAgfVxuXG4gICY6aG92ZXIge1xuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcbiAgICBib3gtc2hhZG93OiAwIDhweCAxNnB4IHJnYmEoMCwgMCwgMCwgMC4xKTtcbiAgICBib3JkZXItY29sb3I6ICM2NjdlZWE7XG4gIH1cblxuICAvLyBTcGVjaWFsIHN0eWxpbmcgZm9yIGxhc3Qgb3BlbmVkIHByb2plY3RcbiAgJi5sYXN0LW9wZW5lZCB7XG4gICAgYm9yZGVyOiAycHggc29saWQgIzY2N2VlYTtcbiAgICBib3gtc2hhZG93OiAwIDRweCAxMnB4IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4yKTtcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMDMpIDAlLCByZ2JhKDExOCwgNzUsIDE2MiwgMC4wMykgMTAwJSk7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtNHB4KTtcbiAgICAgIGJveC1zaGFkb3c6IDAgMTJweCAyNHB4IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4zKTtcbiAgICAgIGJvcmRlci1jb2xvcjogIzc2NGJhMjtcbiAgICB9XG5cbiAgICAucHJvamVjdC1pY29uLXdyYXBwZXIge1xuICAgICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcbiAgICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjQpO1xuICAgICAgYW5pbWF0aW9uOiBwdWxzZSAycyBlYXNlLWluLW91dCBpbmZpbml0ZTtcbiAgICB9XG4gIH1cblxuICAucHJvamVjdC1jYXJkLWNvbnRlbnQge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IHN0cmV0Y2g7XG4gICAgcGFkZGluZzogMTJweCAxNnB4O1xuICAgIGdhcDogMTRweDtcbiAgfVxuXG4gIC5wcm9qZWN0LWluZm8ge1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBnYXA6IDEycHg7XG4gICAgZmxleDogMCAwIGF1dG87XG4gICAgd2lkdGg6IGNsYW1wKDIyMHB4LCAzNCUsIDM0MHB4KTtcbiAgICBtaW4td2lkdGg6IDA7XG4gICAgY3Vyc29yOiBwb2ludGVyO1xuICB9XG5cbiAgLnByb2plY3QtZGVzY3JpcHRpb24tYm94IHtcbiAgICBmbGV4OiAxIDEgYXV0bztcbiAgICBtaW4td2lkdGg6IDA7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgIHBhZGRpbmc6IDhweCAxMnB4O1xuICAgIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICAgIGJvcmRlci1yYWRpdXM6IDhweDtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgICBjdXJzb3I6IHBvaW50ZXI7XG4gICAgdHJhbnNpdGlvbjogYm9yZGVyLWNvbG9yIDAuMnMgZWFzZSwgYmFja2dyb3VuZCAwLjJzIGVhc2U7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIGJvcmRlci1jb2xvcjogcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjUpO1xuICAgIH1cblxuICAgIC5wcm9qZWN0LWRlc2NyaXB0aW9uIHtcbiAgICAgIG1hcmdpbjogMDtcbiAgICAgIGZvbnQtc2l6ZTogMC44cmVtO1xuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICAgICAgbGluZS1oZWlnaHQ6IDEuNDtcbiAgICAgIGRpc3BsYXk6IC13ZWJraXQtYm94O1xuICAgICAgLXdlYmtpdC1saW5lLWNsYW1wOiAzO1xuICAgICAgLXdlYmtpdC1ib3gtb3JpZW50OiB2ZXJ0aWNhbDtcbiAgICAgIG92ZXJmbG93OiBoaWRkZW47XG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcbiAgICAgIHdvcmQtYnJlYWs6IGJyZWFrLXdvcmQ7XG4gICAgICB3aWR0aDogMTAwJTtcbiAgICB9XG5cbiAgICAmLmVtcHR5IHtcbiAgICAgIGJvcmRlci1zdHlsZTogZGFzaGVkO1xuICAgICAgYmFja2dyb3VuZDogdHJhbnNwYXJlbnQ7XG5cbiAgICAgICY6aG92ZXIge1xuICAgICAgICBiYWNrZ3JvdW5kOiByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMDQpO1xuICAgICAgfVxuXG4gICAgICAuZW1wdHktcGxhY2Vob2xkZXIge1xuICAgICAgICBkaXNwbGF5OiBpbmxpbmUtZmxleDtcbiAgICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgICAgZ2FwOiA2cHg7XG4gICAgICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xuICAgICAgICBvcGFjaXR5OiAwLjY7XG4gICAgICAgIGZvbnQtc2l6ZTogMC43OHJlbTtcbiAgICAgICAgZm9udC1zdHlsZTogaXRhbGljO1xuXG4gICAgICAgIG1hdC1pY29uIHtcbiAgICAgICAgICBmb250LXNpemU6IDE2cHg7XG4gICAgICAgICAgd2lkdGg6IDE2cHg7XG4gICAgICAgICAgaGVpZ2h0OiAxNnB4O1xuICAgICAgICB9XG4gICAgICB9XG4gICAgfVxuICB9XG5cbiAgLnByb2plY3QtaWNvbi13cmFwcGVyIHtcbiAgICBmbGV4LXNocmluazogMDtcbiAgICB3aWR0aDogNDBweDtcbiAgICBoZWlnaHQ6IDQwcHg7XG4gICAgYm9yZGVyLXJhZGl1czogMTBweDtcbiAgICBiYWNrZ3JvdW5kOiBsaW5lYXItZ3JhZGllbnQoMTM1ZGVnLCAjNjY3ZWVhIDAlLCAjNzY0YmEyIDEwMCUpO1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcbiAgICBvdmVyZmxvdzogaGlkZGVuO1xuXG4gICAgLnByb2plY3QtaWNvbiB7XG4gICAgICBmb250LXNpemU6IDIwcHg7XG4gICAgICB3aWR0aDogMjBweDtcbiAgICAgIGhlaWdodDogMjBweDtcbiAgICAgIGNvbG9yOiB3aGl0ZTtcbiAgICB9XG5cbiAgICAucHJvamVjdC1pY29uLXN2ZyB7XG4gICAgICB3aWR0aDogMjBweDtcbiAgICAgIGhlaWdodDogMjBweDtcbiAgICAgIGZpbHRlcjogYnJpZ2h0bmVzcygwKSBpbnZlcnQoMSk7IC8vIE1ha2VzIFNWRyB3aGl0ZVxuICAgIH1cblxuICAgIC8vIFdoZW4gYSBjdXN0b20gaWNvbiBpcyBzZXQgdGhlIHVzZXItcHJvdmlkZWQgUE5HIGZpbGxzIHRoZSBzcXVpcmNsZS5cbiAgICAvLyBObyBncmFkaWVudCBiYWNrZ3JvdW5kLCBubyB3aGl0ZSBmaWx0ZXIgw6LCgMKUIHNob3cgdGhlIGxvZ28gdmVyYmF0aW0uXG4gICAgJi5oYXMtY3VzdG9tIHtcbiAgICAgIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1zZWNvbmRhcnkpO1xuXG4gICAgICAucHJvamVjdC1pY29uLWN1c3RvbSB7XG4gICAgICAgIHdpZHRoOiAxMDAlO1xuICAgICAgICBoZWlnaHQ6IDEwMCU7XG4gICAgICAgIG9iamVjdC1maXQ6IGNvdmVyO1xuICAgICAgICBkaXNwbGF5OiBibG9jaztcbiAgICAgIH1cbiAgICB9XG4gIH1cblxuICAucHJvamVjdC1kZXRhaWxzIHtcbiAgICBmbGV4OiAxO1xuICAgIG1pbi13aWR0aDogMDtcbiAgICBkaXNwbGF5OiBmbGV4O1xuICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gICAgZ2FwOiAycHg7XG5cbiAgICAucHJvamVjdC1uYW1lLXJvdyB7XG4gICAgICBkaXNwbGF5OiBmbGV4O1xuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgIGdhcDogOHB4O1xuICAgICAgbWluLXdpZHRoOiAwO1xuICAgIH1cblxuICAgIC5wcm9qZWN0LW5hbWUge1xuICAgICAgbWFyZ2luOiAwO1xuICAgICAgZm9udC1zaXplOiAxLjFyZW07XG4gICAgICBmb250LXdlaWdodDogNTAwO1xuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xuICAgICAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgICAgIG92ZXJmbG93OiBoaWRkZW47XG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcbiAgICAgIG1pbi13aWR0aDogMDtcbiAgICAgIGZsZXgtc2hyaW5rOiAxO1xuICAgIH1cblxuICAgIC5wcm9qZWN0LXBhdGgge1xuICAgICAgbWFyZ2luOiAwO1xuICAgICAgZm9udC1zaXplOiAwLjhyZW07XG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcbiAgICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xuICAgICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XG4gICAgfVxuXG4gICAgLmxhc3Qtb3BlbmVkLWJhZGdlIHtcbiAgICAgIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgIGdhcDogNHB4O1xuICAgICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcbiAgICAgIGNvbG9yOiB3aGl0ZTtcbiAgICAgIHBhZGRpbmc6IDNweCA4cHg7XG4gICAgICBib3JkZXItcmFkaXVzOiAxMHB4O1xuICAgICAgZm9udC1zaXplOiAwLjZyZW07XG4gICAgICBmb250LXdlaWdodDogNjAwO1xuICAgICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgICAgIGxldHRlci1zcGFjaW5nOiAwLjRweDtcbiAgICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XG4gICAgICBib3gtc2hhZG93OiAwIDJweCA2cHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjMpO1xuICAgICAgYW5pbWF0aW9uOiBmYWRlSW5TY2FsZSAwLjVzIGVhc2Utb3V0O1xuICAgICAgZmxleC1zaHJpbms6IDA7XG5cbiAgICAgIC5iYWRnZS1pY29uIHtcbiAgICAgICAgZm9udC1zaXplOiAxMXB4O1xuICAgICAgICB3aWR0aDogMTFweDtcbiAgICAgICAgaGVpZ2h0OiAxMXB4O1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIC5wcm9qZWN0LWFjdGlvbnMge1xuICAgIGRpc3BsYXk6IGdyaWQ7XG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiByZXBlYXQoMiwgMzJweCk7XG4gICAgZ3JpZC1hdXRvLXJvd3M6IDMycHg7XG4gICAgZ2FwOiAycHg7XG4gICAgZmxleC1zaHJpbms6IDA7XG4gICAgYWxpZ24tc2VsZjogY2VudGVyO1xuXG4gICAgLmFjdGlvbi1idG4ge1xuICAgICAgdHJhbnNpdGlvbjogYWxsIDAuMnMgZWFzZTtcbiAgICAgIHdpZHRoOiAzMnB4O1xuICAgICAgaGVpZ2h0OiAzMnB4O1xuICAgICAgbGluZS1oZWlnaHQ6IDMycHg7XG4gICAgICBwYWRkaW5nOiAwO1xuXG4gICAgICAvLyBTaHJpbmsgdGhlIE1hdGVyaWFsIHJpcHBsZSBjb250YWluZXIgc28gaG92ZXIvZm9jdXMgc3RheXMgaW5zaWRlIHRoZSAzMngzMiBib3hcbiAgICAgIDo6bmctZGVlcCAubWF0LW1kYy1idXR0b24tdG91Y2gtdGFyZ2V0IHtcbiAgICAgICAgd2lkdGg6IDMycHggIWltcG9ydGFudDtcbiAgICAgICAgaGVpZ2h0OiAzMnB4ICFpbXBvcnRhbnQ7XG4gICAgICB9XG5cbiAgICAgICYuZWRpdC1idG46aG92ZXIge1xuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDI1NSwgMTUyLCAwLCAwLjEyKTtcbiAgICAgICAgbWF0LWljb24ge1xuICAgICAgICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgICYuc2hhcmUtYnRuOmhvdmVyIHtcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSg3NiwgMTc1LCA4MCwgMC4xKTtcbiAgICAgICAgbWF0LWljb24ge1xuICAgICAgICAgIGNvbG9yOiAjNGNhZjUwO1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgICYuc2V0dGluZ3MtYnRuOmhvdmVyIHtcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjEpO1xuICAgICAgICBtYXQtaWNvbiB7XG4gICAgICAgICAgY29sb3I6ICM2NjdlZWE7XG4gICAgICAgIH1cbiAgICAgIH1cblxuICAgICAgJi5kZWxldGUtYnRuOmhvdmVyIHtcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgyNDUsIDg3LCAxMDgsIDAuMSk7XG4gICAgICAgIG1hdC1pY29uIHtcbiAgICAgICAgICBjb2xvcjogI2Y1NTc2YztcbiAgICAgICAgfVxuICAgICAgfVxuXG4gICAgICBtYXQtaWNvbiB7XG4gICAgICAgIGZvbnQtc2l6ZTogMThweDtcbiAgICAgICAgd2lkdGg6IDE4cHg7XG4gICAgICAgIGhlaWdodDogMThweDtcbiAgICAgICAgbGluZS1oZWlnaHQ6IDE4cHg7XG4gICAgICAgIGNvbG9yOiAjYTBhZWMwO1xuICAgICAgICB0cmFuc2l0aW9uOiBjb2xvciAwLjJzIGVhc2U7XG4gICAgICB9XG4gICAgfVxuICB9XG59XG5cbi8vIFNpZGViYXIgc2VjdGlvbiAoaGlkZGVuIGJ5IGRlZmF1bHQpXG4uc2lkZWJhci1zZWN0aW9uIHtcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcbn1cblxuLy8gUmVzcG9uc2l2ZSBkZXNpZ25cbi8vIExlIGNhcmQgZGVsbGUgYXppb25pIGFwZXJ0ZSBzb3ByYSBsYSBsaXN0YSAoZmluZXN0cmEgc3RyZXR0YSk6IHVuIGNsaWMgc3VsbG8gc2ZvbmRvIGxlIHJpY2hpdWRlLlxuLmFjdGlvbnMtYmFja2Ryb3Age1xuICBkaXNwbGF5OiBub25lO1xufVxuXG4vLyBGaW5lc3RyYSBzdHJldHRhOiBsZSBhemlvbmkgbm9uIHNjZW5kb25vIHNvdHRvIGxhIGxpc3RhLiBSZXN0YW5vIGEgZGVzdHJhIGNvbWUgdW5hIGNvbG9ubmluYSBkaSBpY29uZTsgdW4gY2xpYyBsYVxuLy8gYXByZSBuZWxsZSBjYXJkIGNvbXBsZXRlLCBzb3ByYSBsYSBsaXN0YSAoUHJvamVjdHNDb21wb25lbnQuYWN0KS4gTGEgbGFyZ2hlenphIMODwqggbGEgc3Rlc3NhIGRpIGNvbXBhY3RRdWVyeS5cbkBtZWRpYSAobWF4LXdpZHRoOiAxMDI0cHgpIHtcbiAgLm1haW4tbGF5b3V0IHtcbiAgICBncmlkLXRlbXBsYXRlLWNvbHVtbnM6IDFmciA3MnB4O1xuICAgIGdhcDogMTZweDtcbiAgfVxuXG4gIC5yaWdodC1jb2x1bW46bm90KC5leHBhbmRlZCkge1xuICAgIC5zZWN0aW9uLXRpdGxlLFxuICAgIC5kaWFnbm9zdGljcy1zZWN0aW9uIHtcbiAgICAgIGRpc3BsYXk6IG5vbmU7XG4gICAgfVxuXG4gICAgLmFjdGlvbnMtZ3JpZCB7XG4gICAgICBnYXA6IDEycHg7XG4gICAgfVxuXG4gICAgLmFjdGlvbi1jYXJkIHtcbiAgICAgIGJvcmRlci1yYWRpdXM6IDE0cHg7XG5cbiAgICAgICY6aG92ZXIge1xuICAgICAgICB0cmFuc2Zvcm06IG5vbmU7XG4gICAgICB9XG5cbiAgICAgIC5jYXJkLWNvbnRlbnQge1xuICAgICAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcbiAgICAgICAgcGFkZGluZzogMTBweDtcbiAgICAgIH1cblxuICAgICAgLmNhcmQtdGV4dCxcbiAgICAgIC5jYXJkLWFjdGlvbiB7XG4gICAgICAgIGRpc3BsYXk6IG5vbmU7XG4gICAgICB9XG4gICAgfVxuICB9XG5cbiAgLnJpZ2h0LWNvbHVtbi5leHBhbmRlZCB7XG4gICAgcG9zaXRpb246IGZpeGVkO1xuICAgIHRvcDogNTJweDtcbiAgICByaWdodDogMTZweDtcbiAgICB3aWR0aDogNDAwcHg7XG4gICAgbWF4LXdpZHRoOiBjYWxjKDEwMHZ3IC0gMzJweCk7XG4gICAgbWF4LWhlaWdodDogY2FsYygxMDB2aCAtIDcycHgpO1xuICAgIG92ZXJmbG93LXk6IGF1dG87XG4gICAgYm94LXNpemluZzogYm9yZGVyLWJveDtcbiAgICB6LWluZGV4OiAxMDAxO1xuICAgIHBhZGRpbmc6IDIwcHg7XG4gICAgYm9yZGVyLXJhZGl1czogMTZweDtcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcbiAgICBib3gtc2hhZG93OiAwIDEycHggMzJweCB2YXIoLS1tZGUtc2hhZG93LWNvbG9yKTtcbiAgfVxuXG4gIC5hY3Rpb25zLWJhY2tkcm9wIHtcbiAgICBkaXNwbGF5OiBibG9jaztcbiAgICBwb3NpdGlvbjogZml4ZWQ7XG4gICAgaW5zZXQ6IDA7XG4gICAgei1pbmRleDogMTAwMDtcbiAgICBiYWNrZ3JvdW5kOiByZ2JhKDAsIDAsIDAsIDAuMjUpO1xuICB9XG59XG5cbkBtZWRpYSAobWF4LXdpZHRoOiA3NjhweCkge1xuICAuaGVhZGVyLXNlY3Rpb24ge1xuICAgIHBhZGRpbmc6IDQwcHggMjBweCAyMHB4O1xuXG4gICAgLm1haW4tdGl0bGUge1xuICAgICAgZm9udC1zaXplOiAycmVtO1xuICAgIH1cblxuICAgIC5zdWJ0aXRsZSB7XG4gICAgICBmb250LXNpemU6IDFyZW07XG4gICAgfVxuICB9XG5cbiAgLmNvbnRlbnQtd3JhcHBlciB7XG4gICAgcGFkZGluZzogMzJweCAyMHB4O1xuICAgIGJvcmRlci1yYWRpdXM6IDI0cHggMjRweCAwIDA7XG4gIH1cblxuICAucmlnaHQtY29sdW1uLmV4cGFuZGVkIC5hY3Rpb24tY2FyZCAuY2FyZC1jb250ZW50IHtcbiAgICBwYWRkaW5nOiAyNHB4IDIwcHg7XG4gIH1cblxuICAuc2VjdGlvbi10aXRsZSB7XG4gICAgZm9udC1zaXplOiAxLjI1cmVtO1xuICB9XG5cbiAgLnNlY3Rpb24taGVhZGVyIHtcbiAgICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICAgIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xuXG4gICAgLnNlYXJjaC1maWVsZCB7XG4gICAgICBtYXgtd2lkdGg6IDEwMCU7XG4gICAgfVxuICB9XG59XG5cbi8vIEFuaW1hdGlvbnNcbkBrZXlmcmFtZXMgcHVsc2Uge1xuICAwJSwgMTAwJSB7XG4gICAgdHJhbnNmb3JtOiBzY2FsZSgxKTtcbiAgfVxuICA1MCUge1xuICAgIHRyYW5zZm9ybTogc2NhbGUoMS4wNSk7XG4gIH1cbn1cblxuQGtleWZyYW1lcyBmYWRlSW5TY2FsZSB7XG4gIDAlIHtcbiAgICBvcGFjaXR5OiAwO1xuICAgIHRyYW5zZm9ybTogc2NhbGUoMC44KTtcbiAgfVxuICAxMDAlIHtcbiAgICBvcGFjaXR5OiAxO1xuICAgIHRyYW5zZm9ybTogc2NhbGUoMSk7XG4gIH1cbn1cblxuLy8gRGlhZ25vc3RpY3Mgc2VjdGlvblxuLmRpYWdub3N0aWNzLXNlY3Rpb24ge1xuICBtYXJnaW4tdG9wOiAyNHB4O1xuICBwYWRkaW5nLXRvcDogMTZweDtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xuICBkaXNwbGF5OiBmbGV4O1xuICBnYXA6IDEycHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcblxuICBidXR0b24ge1xuICAgIGZvbnQtc2l6ZTogMC44NXJlbTtcbiAgICBtYXQtaWNvbiB7XG4gICAgICBmb250LXNpemU6IDE4cHg7XG4gICAgICB3aWR0aDogMThweDtcbiAgICAgIGhlaWdodDogMThweDtcbiAgICAgIG1hcmdpbi1yaWdodDogNnB4O1xuICAgIH1cbiAgfVxufVxuXG4vLyBBdnZpc2kgZGkgYXZ2aW8gKEphdmEgZSBzaW1pbGkpXG4uc3RhcnR1cC1ub3RpY2VzIHsgbWFyZ2luOiA4cHggMCAxNnB4OyBkaXNwbGF5OiBmbGV4OyBmbGV4LWRpcmVjdGlvbjogY29sdW1uOyBnYXA6IDhweDsgfVxuLnN0YXJ0dXAtbm90aWNlIHtcbiAgZGlzcGxheTogZmxleDsgZmxleC13cmFwOiB3cmFwOyBhbGlnbi1pdGVtczogY2VudGVyOyBnYXA6IDEwcHg7IHBhZGRpbmc6IDEwcHggMTRweDsgYm9yZGVyLXJhZGl1czogMTBweDtcbiAgbWF0LWljb24geyBmbGV4OiAwIDAgYXV0bzsgfVxuICAubm90aWNlLXRleHQgeyBmbGV4OiAxIDEgMzIwcHg7IG1pbi13aWR0aDogMjQwcHg7IH1cbiAgYmFja2dyb3VuZDogI2U4ZjBmZTsgY29sb3I6ICMxYTNkN2M7IGJvcmRlci1sZWZ0OiA2cHggc29saWQgIzFhNzNlODtcbiAgLm5vdGljZS10ZXh0IHsgbGluZS1oZWlnaHQ6IDEuMzU7IH1cbiAgJi5sZXZlbC13YXJuaW5nIHsgYmFja2dyb3VuZDogI2ZmZjNlMDsgY29sb3I6ICM3YTRhMDA7IGJvcmRlci1sZWZ0LWNvbG9yOiAjZjI5OTAwOyB9XG4gICYubGV2ZWwtZXJyb3IgeyBiYWNrZ3JvdW5kOiAjZmRlY2VhOyBjb2xvcjogIzhhMWMxMzsgYm9yZGVyLWxlZnQtY29sb3I6ICNkOTMwMjU7IH1cbn1cbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
    });
  }
}

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
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./open-recent/open-recent.component */ 7111);
/* harmony import */ var _projects_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./projects.component */ 5609);
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./new-project/new-project.component */ 5389);
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ 7179);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _dialogs_template_showcase_template_showcase_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/template-showcase/template-showcase-dialog.component */ 2555);
/* harmony import */ var _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./dialogs/project-edit/project-edit-dialog.component */ 9116);
/* harmony import */ var _dialogs_project_edit_icon_editor_icon_editor_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./dialogs/project-edit/icon-editor/icon-editor.component */ 1602);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./dialogs/p2p-manager/p2p-manager.component */ 1902);
/* harmony import */ var _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./dialogs/catalog-picker/catalog-picker.component */ 5922);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../shared/material.module */ 4872);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../git/git.module */ 1312);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./services/project-settings.service */ 5450);
/* harmony import */ var _components_participant_gems_participant_gems_module__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ../components/participant-gems/participant-gems.module */ 2873);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/core */ 2560);






















const routes = [{
  path: '',
  component: _projects_component__WEBPACK_IMPORTED_MODULE_1__.ProjectsComponent,
  children: [{
    path: '',
    redirectTo: 'openrecent',
    pathMatch: 'full'
  }, {
    path: 'openrecent',
    component: _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__.OpenRecentComponent
  }, {
    path: 'newproject',
    component: _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__.NewProjectComponent
  }]
}];
class ProjectsModule {
  constructor() {
    console.log('constructor ProjectsModule');
  }
  static {
    this.ɵfac = function ProjectsModule_Factory(t) {
      return new (t || ProjectsModule)();
    };
  }
  static {
    this.ɵmod = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_16__["ɵɵdefineNgModule"]({
      type: ProjectsModule
    });
  }
  static {
    this.ɵinj = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_16__["ɵɵdefineInjector"]({
      providers: [_services_project_settings_service__WEBPACK_IMPORTED_MODULE_14__.ProjectSettingsService],
      imports: [_angular_router__WEBPACK_IMPORTED_MODULE_17__.RouterModule.forChild(routes), _angular_common__WEBPACK_IMPORTED_MODULE_18__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_12__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.FormsModule, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.ReactiveFormsModule, _git_git_module__WEBPACK_IMPORTED_MODULE_13__.GitModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_20__.TranslateModule, _components_participant_gems_participant_gems_module__WEBPACK_IMPORTED_MODULE_15__.ParticipantGemsModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_16__["ɵɵsetNgModuleScope"](ProjectsModule, {
    declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__.OpenRecentComponent, _projects_component__WEBPACK_IMPORTED_MODULE_1__.ProjectsComponent, _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__.NewProjectComponent, _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__.CloneProjectComponent, _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__.ModernCloneProjectComponent, _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__.ProjectCreateConfigDialogComponent, _dialogs_template_showcase_template_showcase_dialog_component__WEBPACK_IMPORTED_MODULE_6__.TemplateShowcaseDialogComponent, _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_7__.ProjectEditDialogComponent, _dialogs_project_edit_icon_editor_icon_editor_component__WEBPACK_IMPORTED_MODULE_8__.IconEditorComponent, _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_9__.ProjectSettingsComponent, _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_10__.P2PManagerComponent, _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_11__.CatalogPickerDialogComponent],
    imports: [_angular_router__WEBPACK_IMPORTED_MODULE_17__.RouterModule, _angular_common__WEBPACK_IMPORTED_MODULE_18__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_12__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.FormsModule, _angular_forms__WEBPACK_IMPORTED_MODULE_19__.ReactiveFormsModule, _git_git_module__WEBPACK_IMPORTED_MODULE_13__.GitModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_20__.TranslateModule, _components_participant_gems_participant_gems_module__WEBPACK_IMPORTED_MODULE_15__.ParticipantGemsModule]
  });
})();

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
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ 6317);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ 9337);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common/http */ 8987);




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
    const params = projectPath ? {
      projectPath
    } : {};
    return this.http.get(`${this.apiUrl}/config`, {
      params
    }).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_1__.tap)(config => this.currentConfigSubject.next(config)));
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
  static {
    this.ɵfac = function IdeConfigurationService_Factory(t) {
      return new (t || IdeConfigurationService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_3__.HttpClient));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({
      token: IdeConfigurationService,
      factory: IdeConfigurationService.ɵfac,
      providedIn: 'root'
    });
  }
}

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
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);




const diractionMap = {
  left: 'left',
  right: 'left',
  top: 'top',
  bottom: 'top'
};
const multyMap = {
  left: 1,
  right: -1,
  top: 1,
  bottom: -1
};
class NgDialogAnimationService {
  constructor(dialog, ngZone, incomingOptions, outgoingOptions) {
    this.dialog = dialog;
    this.ngZone = ngZone;
    this.incomingOptions = incomingOptions;
    this.outgoingOptions = outgoingOptions;
    this.incomingOptions = {
      keyframes: [{
        transform: "rotate(360deg)"
      }, {
        transform: "rotate(0)"
      }],
      keyframeAnimationOptions: {
        easing: "ease-in-out",
        duration: 500
      }
    };
    this.outgoingOptions = {
      keyframes: [{
        transform: "rotate(0)"
      }, {
        transform: "rotate(360deg)"
      }],
      keyframeAnimationOptions: {
        easing: "ease-in-out",
        duration: 500
      }
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
      } else {
        config.position.left = config.position.rowEnd;
      }
    }
    if (config.position && config.position.rowStart) {
      if (dir === 'rtl') {
        config.position.left = config.position.rowStart;
      } else {
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
      const incomingOptions = config.animation.incomingOptions || this.incomingOptions || {
        keyframeAnimationOptions: {
          duration: 600,
          easing: 'ease-in'
        }
      };
      const outgoingOptions = config.animation.outgoingOptions || this.outgoingOptions || {
        keyframeAnimationOptions: {
          duration: 600,
          easing: 'ease-out'
        }
      };
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
        keyFrame100[to] = to === 'top' || to === 'bottom' ? container.clientHeight * multyMap[config.animation.to] + 'px' : container.clientWidth * multyMap[config.animation.to] + 'px';
        incomeKeyFrames = incomeKeyFrames || [keyFrame100, keyFrame0];
        outgoingKeyFrames = outgoingKeyFrames || [keyFrame0, keyFrame100];
      }
      animate(incomeKeyFrames, incomingOptions.keyframeAnimationOptions);
      const closeHandler = dialogResult => {
        _afterClosed.next(dialogResult);
        const animation = animate(outgoingKeyFrames, outgoingOptions.keyframeAnimationOptions);
        animation.onfinish = () => {
          wrapper.style.display = 'none';
          this.ngZone.run(() => ref.close(dialogResult));
        };
        ref.close = closeFunction;
      };
      ref.close = dialogResult => closeHandler(dialogResult);
    }
    return ref;
  }
  static {
    this.ɵfac = function NgDialogAnimationService_Factory(t) {
      return new (t || NgDialogAnimationService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_core__WEBPACK_IMPORTED_MODULE_1__.NgZone), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"]('INCOMING_OPTION', 8), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"]('OUTGOING_OPTION', 8));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({
      token: NgDialogAnimationService,
      factory: NgDialogAnimationService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ })

}]);
//# sourceMappingURL=src_app_projects_projects_module_ts.d7c41a550b38fb60.js.map