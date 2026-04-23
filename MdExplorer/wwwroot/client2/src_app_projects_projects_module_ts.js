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
      styles: [".loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 24px;\n  gap: 12px;\n}\n\n.error-message[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 16px;\n  color: #f44336;\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 24px;\n  color: rgba(0, 0, 0, 0.4);\n  gap: 8px;\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n}\n\n.catalog-list[_ngcontent-%COMP%] {\n  max-height: 400px;\n  overflow-y: auto;\n}\n\n.catalog-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  padding: 12px 8px;\n  border-bottom: 1px solid rgba(0, 0, 0, 0.08);\n}\n.catalog-item[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n.catalog-item.already-added[_ngcontent-%COMP%] {\n  opacity: 0.5;\n}\n\n.app-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 1;\n  min-width: 0;\n}\n\n.app-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n  flex-shrink: 0;\n}\n\n.app-details[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  min-width: 0;\n}\n\n.app-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  font-size: 14px;\n}\n\n.app-desc[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: rgba(0, 0, 0, 0.54);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n.app-version[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: rgba(0, 0, 0, 0.38);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9jYXRhbG9nLXBpY2tlci9jYXRhbG9nLXBpY2tlci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxRQUFBO0FBQ0Y7QUFDRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQUNKOztBQUdBO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsOEJBQUE7RUFDQSxpQkFBQTtFQUNBLDRDQUFBO0FBQUY7QUFFRTtFQUNFLG1CQUFBO0FBQUo7QUFHRTtFQUNFLFlBQUE7QUFESjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxPQUFBO0VBQ0EsWUFBQTtBQUZGOztBQUtBO0VBQ0UsY0FBQTtFQUNBLGNBQUE7QUFGRjs7QUFLQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFlBQUE7QUFGRjs7QUFLQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUZGOztBQUtBO0VBQ0UsZUFBQTtFQUNBLDBCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0FBRkY7O0FBS0E7RUFDRSxlQUFBO0VBQ0EsMEJBQUE7QUFGRiIsInNvdXJjZXNDb250ZW50IjpbIi5sb2FkaW5nLWNvbnRhaW5lciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICBwYWRkaW5nOiAyNHB4O1xuICBnYXA6IDEycHg7XG59XG5cbi5lcnJvci1tZXNzYWdlIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG4gIHBhZGRpbmc6IDE2cHg7XG4gIGNvbG9yOiAjZjQ0MzM2O1xufVxuXG4uZW1wdHktc3RhdGUge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBwYWRkaW5nOiAyNHB4O1xuICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjQpO1xuICBnYXA6IDhweDtcblxuICBtYXQtaWNvbiB7XG4gICAgZm9udC1zaXplOiA0OHB4O1xuICAgIHdpZHRoOiA0OHB4O1xuICAgIGhlaWdodDogNDhweDtcbiAgfVxufVxuXG4uY2F0YWxvZy1saXN0IHtcbiAgbWF4LWhlaWdodDogNDAwcHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG59XG5cbi5jYXRhbG9nLWl0ZW0ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XG4gIHBhZGRpbmc6IDEycHggOHB4O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgcmdiYSgwLCAwLCAwLCAwLjA4KTtcblxuICAmOmxhc3QtY2hpbGQge1xuICAgIGJvcmRlci1ib3R0b206IG5vbmU7XG4gIH1cblxuICAmLmFscmVhZHktYWRkZWQge1xuICAgIG9wYWNpdHk6IDAuNTtcbiAgfVxufVxuXG4uYXBwLWluZm8ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDEycHg7XG4gIGZsZXg6IDE7XG4gIG1pbi13aWR0aDogMDtcbn1cblxuLmFwcC1pY29uIHtcbiAgY29sb3I6ICMxOTc2ZDI7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uYXBwLWRldGFpbHMge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBtaW4td2lkdGg6IDA7XG59XG5cbi5hcHAtbmFtZSB7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG4gIGZvbnQtc2l6ZTogMTRweDtcbn1cblxuLmFwcC1kZXNjIHtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjU0KTtcbiAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcbiAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XG59XG5cbi5hcHAtdmVyc2lvbiB7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC4zOCk7XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"]
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
      styles: [".vertical-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n}\n\n.orizzontal-form-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9jbG9uZS1wcm9qZWN0L2Nsb25lLXByb2plY3QuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtBQUNGIiwic291cmNlc0NvbnRlbnQiOlsiLnZlcnRpY2FsLWZvcm0tY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbn1cclxuXHJcbi5vcml6em9udGFsLWZvcm0tY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbn1cclxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
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
      this.snackBar.open('Magnet link non disponibile', 'OK', {
        duration: 3000
      });
      return;
    }
    this.snackBar.open('Avvio download...', '', {
      duration: 2000
    });
    this.p2pService.download(fileInfo.magnetUri).subscribe({
      next: result => {
        if (result.success) {
          this.snackBar.open(`Download avviato: ${result.name}`, 'OK', {
            duration: 3000
          });
          this.selectedTab = 0; // Switch to Transfers tab
          this.refreshTransfers();
        } else {
          this.snackBar.open('Errore: ' + result.error, 'OK', {
            duration: 3000
          });
        }
      },
      error: err => {
        this.snackBar.open('Errore download: ' + err.message, 'OK', {
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
      styles: ["[_nghost-%COMP%] {\n  display: block;\n}\n\nh1[mat-dialog-title][_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\nh1[mat-dialog-title][_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #1976d2;\n}\n\n.p2p-content[_ngcontent-%COMP%] {\n  min-width: 500px;\n  min-height: 400px;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 40px;\n  gap: 16px;\n}\n.loading-container[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.6);\n}\n\n.not-available[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 32px;\n  text-align: center;\n}\n.not-available[_ngcontent-%COMP%]   .warning-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  color: #f57c00;\n  margin-bottom: 16px;\n}\n.not-available[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n}\n.not-available[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.6);\n  margin-bottom: 16px;\n}\n.not-available[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%] {\n  text-align: left;\n  margin-bottom: 24px;\n}\n.not-available[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.7);\n  margin-bottom: 4px;\n}\n\n.stats-bar[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 24px;\n  padding: 12px 16px;\n  background: #f5f5f5;\n  border-radius: 8px;\n  margin-bottom: 16px;\n  flex-wrap: wrap;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  font-size: 13px;\n  color: rgba(0, 0, 0, 0.7);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #1976d2;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status[_ngcontent-%COMP%] {\n  cursor: pointer;\n  padding: 4px 8px;\n  margin: -4px;\n  border-radius: 4px;\n  transition: background-color 0.2s;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status[_ngcontent-%COMP%]:hover {\n  background: rgba(0, 0, 0, 0.05);\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status.tracker-connected[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #28a745;\n}\n.stats-bar[_ngcontent-%COMP%]   .stat-item.tracker-status.tracker-error[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #dc3545;\n}\n\n.tracker-warning[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px 16px;\n  background: #fff3cd;\n  border: 1px solid #ffc107;\n  border-radius: 8px;\n  margin-bottom: 16px;\n  font-size: 13px;\n}\n.tracker-warning[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.tracker-warning[_ngcontent-%COMP%]   span[_ngcontent-%COMP%] {\n  flex: 1;\n  color: #856404;\n}\n.tracker-warning[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 16px 0;\n  min-height: 280px;\n}\n\n.transfers-header[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  margin-bottom: 8px;\n}\n\n.empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  padding: 48px;\n  color: rgba(0, 0, 0, 0.5);\n}\n.empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  width: 64px;\n  height: 64px;\n  margin-bottom: 16px;\n  opacity: 0.5;\n}\n.empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 16px;\n}\n.empty-state[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  margin-top: 8px;\n}\n\n.transfer-list[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n\n.transfer-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 12px;\n  background: #fafafa;\n  border-radius: 8px;\n  border: 1px solid #e0e0e0;\n}\n.transfer-item[_ngcontent-%COMP%]:hover {\n  background: #f5f5f5;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-icon[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-icon[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  margin-bottom: 4px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-details[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: rgba(0, 0, 0, 0.6);\n  margin-bottom: 8px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   .transfer-details[_ngcontent-%COMP%]   span[_ngcontent-%COMP%]:not(:last-child)::after {\n  content: \"\";\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-info[_ngcontent-%COMP%]   mat-progress-bar[_ngcontent-%COMP%] {\n  height: 6px;\n  border-radius: 3px;\n}\n.transfer-item[_ngcontent-%COMP%]   .transfer-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 4px;\n  flex-shrink: 0;\n}\n\n.download-tab[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 8px 0;\n}\n.download-tab[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.6);\n  font-size: 13px;\n  margin-bottom: 24px;\n}\n.download-tab[_ngcontent-%COMP%]   .project-selector[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n.download-tab[_ngcontent-%COMP%]   .magnet-input[_ngcontent-%COMP%] {\n  width: 100%;\n  margin-bottom: 16px;\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 16px;\n  padding: 12px;\n  background: #e8f5e9;\n  border-radius: 4px;\n  font-size: 13px;\n  color: #2e7d32;\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n.download-tab[_ngcontent-%COMP%]   .download-path-preview[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  background: rgba(0, 0, 0, 0.05);\n  padding: 2px 6px;\n  border-radius: 3px;\n  font-family: monospace;\n  font-size: 12px;\n}\n\n.info-tab[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0 0 16px 0;\n}\n.info-tab[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  margin: 24px 0 8px 0;\n}\n.info-tab[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.7);\n  line-height: 1.6;\n}\n.info-tab[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%], .info-tab[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.7);\n  padding-left: 24px;\n}\n.info-tab[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%]   li[_ngcontent-%COMP%], .info-tab[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin-bottom: 8px;\n}\n.info-tab[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: rgba(0, 0, 0, 0.5);\n}\n\n.projects-tab[_ngcontent-%COMP%]   .projects-header[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  margin-bottom: 16px;\n}\n.projects-tab[_ngcontent-%COMP%]   .projects-header[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  margin: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .loading-state[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  gap: 12px;\n  padding: 32px;\n  color: rgba(0, 0, 0, 0.6);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-list[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%] {\n  border: 1px solid #e0e0e0;\n  border-radius: 8px;\n  overflow: hidden;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 12px 16px;\n  background: #f5f5f5;\n  cursor: pointer;\n  transition: background-color 0.2s;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]:hover {\n  background: #eeeeee;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  font-weight: 500;\n  flex: 1;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .file-count[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.5);\n  font-size: 13px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-header[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  margin-left: auto;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%] {\n  padding: 8px;\n  background: #fafafa;\n  border-top: 1px solid #e0e0e0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  padding: 8px 12px;\n  border-radius: 4px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]:hover {\n  background: #f0f0f0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.5);\n  flex-shrink: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%]   .file-name[_ngcontent-%COMP%] {\n  display: block;\n  font-size: 14px;\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-info[_ngcontent-%COMP%]   .file-size[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: rgba(0, 0, 0, 0.5);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  margin-right: 8px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 4px;\n  font-size: 12px;\n  color: rgba(0, 0, 0, 0.6);\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-local[_ngcontent-%COMP%] {\n  color: #28a745;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-seeding[_ngcontent-%COMP%] {\n  color: #007bff;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-downloading[_ngcontent-%COMP%] {\n  color: #17a2b8;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-completed[_ngcontent-%COMP%] {\n  color: #28a745;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-available[_ngcontent-%COMP%] {\n  color: #fd7e14;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-unavailable[_ngcontent-%COMP%] {\n  color: #6c757d;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   mat-icon.status-unknown[_ngcontent-%COMP%] {\n  color: #6c757d;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-status[_ngcontent-%COMP%]   .peer-count[_ngcontent-%COMP%]   .status-text[_ngcontent-%COMP%] {\n  white-space: nowrap;\n}\n.projects-tab[_ngcontent-%COMP%]   .project-card[_ngcontent-%COMP%]   .project-files[_ngcontent-%COMP%]   .file-item[_ngcontent-%COMP%]   .file-actions[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n\n.badge[_ngcontent-%COMP%] {\n  background: #1976d2;\n  color: white;\n  font-size: 11px;\n  padding: 2px 6px;\n  border-radius: 10px;\n  margin-left: 8px;\n}\n\n  .mat-tab-label mat-icon {\n  margin-right: 8px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wMnAtbWFuYWdlci9wMnAtbWFuYWdlci5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGNBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFDRjtBQUNFO0VBQ0UsY0FBQTtBQUNKOztBQUdBO0VBQ0UsZ0JBQUE7RUFDQSxpQkFBQTtBQUFGOztBQUdBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7RUFDQSxTQUFBO0FBQUY7QUFFRTtFQUNFLHlCQUFBO0FBQUo7O0FBSUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxrQkFBQTtBQURGO0FBR0U7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsbUJBQUE7QUFESjtBQUlFO0VBQ0UsaUJBQUE7QUFGSjtBQUtFO0VBQ0UseUJBQUE7RUFDQSxtQkFBQTtBQUhKO0FBTUU7RUFDRSxnQkFBQTtFQUNBLG1CQUFBO0FBSko7QUFNSTtFQUNFLHlCQUFBO0VBQ0Esa0JBQUE7QUFKTjs7QUFTQTtFQUNFLGFBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxlQUFBO0FBTkY7QUFRRTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EseUJBQUE7QUFOSjtBQVFJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtBQU5OO0FBU0k7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxpQ0FBQTtBQVBOO0FBU007RUFDRSwrQkFBQTtBQVBSO0FBVU07RUFDRSxjQUFBO0FBUlI7QUFXTTtFQUNFLGNBQUE7QUFUUjs7QUFlQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZUFBQTtBQVpGO0FBY0U7RUFDRSxjQUFBO0FBWko7QUFlRTtFQUNFLE9BQUE7RUFDQSxjQUFBO0FBYko7QUFnQkU7RUFDRSxjQUFBO0FBZEo7O0FBa0JBO0VBQ0UsZUFBQTtFQUNBLGlCQUFBO0FBZkY7O0FBa0JBO0VBQ0UsYUFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7QUFmRjs7QUFrQkE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLHlCQUFBO0FBZkY7QUFpQkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7QUFmSjtBQWtCRTtFQUNFLFNBQUE7RUFDQSxlQUFBO0FBaEJKO0FBbUJFO0VBQ0UsZUFBQTtFQUNBLGVBQUE7QUFqQko7O0FBcUJBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQWxCRjs7QUFxQkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSx5QkFBQTtBQWxCRjtBQW9CRTtFQUNFLG1CQUFBO0FBbEJKO0FBcUJFO0VBQ0UsY0FBQTtBQW5CSjtBQXFCSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQW5CTjtBQXVCRTtFQUNFLE9BQUE7RUFDQSxZQUFBO0FBckJKO0FBdUJJO0VBQ0UsZ0JBQUE7RUFDQSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0EsdUJBQUE7RUFDQSxrQkFBQTtBQXJCTjtBQXdCSTtFQUNFLGVBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0FBdEJOO0FBeUJRO0VBQ0UsV0FBQTtBQXZCVjtBQTRCSTtFQUNFLFdBQUE7RUFDQSxrQkFBQTtBQTFCTjtBQThCRTtFQUNFLGFBQUE7RUFDQSxRQUFBO0VBQ0EsY0FBQTtBQTVCSjs7QUFpQ0U7RUFDRSxpQkFBQTtBQTlCSjtBQWlDRTtFQUNFLHlCQUFBO0VBQ0EsZUFBQTtFQUNBLG1CQUFBO0FBL0JKO0FBa0NFO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBaENKO0FBbUNFO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBakNKO0FBb0NFO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EsY0FBQTtBQWxDSjtBQW9DSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQWxDTjtBQXFDSTtFQUNFLCtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLHNCQUFBO0VBQ0EsZUFBQTtBQW5DTjs7QUF5Q0U7RUFDRSxrQkFBQTtBQXRDSjtBQXlDRTtFQUNFLG9CQUFBO0FBdkNKO0FBMENFO0VBQ0UseUJBQUE7RUFDQSxnQkFBQTtBQXhDSjtBQTJDRTtFQUNFLHlCQUFBO0VBQ0Esa0JBQUE7QUF6Q0o7QUEyQ0k7RUFDRSxrQkFBQTtBQXpDTjtBQTZDRTtFQUNFLGVBQUE7RUFDQSx5QkFBQTtBQTNDSjs7QUFnREU7RUFDRSxhQUFBO0VBQ0EsOEJBQUE7RUFDQSxtQkFBQTtFQUNBLG1CQUFBO0FBN0NKO0FBK0NJO0VBQ0UsU0FBQTtBQTdDTjtBQWlERTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSx5QkFBQTtBQS9DSjtBQWtERTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7QUFoREo7QUFtREU7RUFDRSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFqREo7QUFtREk7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGVBQUE7RUFDQSxpQ0FBQTtBQWpETjtBQW1ETTtFQUNFLG1CQUFBO0FBakRSO0FBb0RNO0VBQ0UsZ0JBQUE7RUFDQSxPQUFBO0FBbERSO0FBcURNO0VBQ0UseUJBQUE7RUFDQSxlQUFBO0FBbkRSO0FBc0RNO0VBQ0UsaUJBQUE7QUFwRFI7QUF3REk7RUFDRSxZQUFBO0VBQ0EsbUJBQUE7RUFDQSw2QkFBQTtBQXRETjtBQXdETTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFNBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0FBdERSO0FBd0RRO0VBQ0UsbUJBQUE7QUF0RFY7QUF5RFE7RUFDRSx5QkFBQTtFQUNBLGNBQUE7QUF2RFY7QUEwRFE7RUFDRSxPQUFBO0VBQ0EsWUFBQTtBQXhEVjtBQTBEVTtFQUNFLGNBQUE7RUFDQSxlQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0FBeERaO0FBMkRVO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0FBekRaO0FBNkRRO0VBQ0UsY0FBQTtFQUNBLGlCQUFBO0FBM0RWO0FBNkRVO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7RUFDQSx5QkFBQTtBQTNEWjtBQTZEWTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQTNEZDtBQTZEYztFQUNFLGNBQUE7QUEzRGhCO0FBNkRjO0VBQ0UsY0FBQTtBQTNEaEI7QUE2RGM7RUFDRSxjQUFBO0FBM0RoQjtBQTZEYztFQUNFLGNBQUE7QUEzRGhCO0FBNkRjO0VBQ0UsY0FBQTtBQTNEaEI7QUE2RGM7RUFDRSxjQUFBO0FBM0RoQjtBQTZEYztFQUNFLGNBQUE7QUEzRGhCO0FBK0RZO0VBQ0UsbUJBQUE7QUE3RGQ7QUFrRVE7RUFDRSxjQUFBO0FBaEVWOztBQXVFQTtFQUNFLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7QUFwRUY7O0FBd0VFO0VBQ0UsaUJBQUE7QUFyRUoiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XHJcbiAgZGlzcGxheTogYmxvY2s7XHJcbn1cclxuXHJcbmgxW21hdC1kaWFsb2ctdGl0bGVdIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA4cHg7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGNvbG9yOiAjMTk3NmQyO1xyXG4gIH1cclxufVxyXG5cclxuLnAycC1jb250ZW50IHtcclxuICBtaW4td2lkdGg6IDUwMHB4O1xyXG4gIG1pbi1oZWlnaHQ6IDQwMHB4O1xyXG59XHJcblxyXG4ubG9hZGluZy1jb250YWluZXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDQwcHg7XHJcbiAgZ2FwOiAxNnB4O1xyXG5cclxuICBwIHtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcbiAgfVxyXG59XHJcblxyXG4ubm90LWF2YWlsYWJsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogMzJweDtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcblxyXG4gIC53YXJuaW5nLWljb24ge1xyXG4gICAgZm9udC1zaXplOiA0OHB4O1xyXG4gICAgd2lkdGg6IDQ4cHg7XHJcbiAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICBjb2xvcjogI2Y1N2MwMDtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG5cclxuICBoMyB7XHJcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC42KTtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG5cclxuICB1bCB7XHJcbiAgICB0ZXh0LWFsaWduOiBsZWZ0O1xyXG4gICAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxuXHJcbiAgICBsaSB7XHJcbiAgICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNyk7XHJcbiAgICAgIG1hcmdpbi1ib3R0b206IDRweDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5zdGF0cy1iYXIge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiAyNHB4O1xyXG4gIHBhZGRpbmc6IDEycHggMTZweDtcclxuICBiYWNrZ3JvdW5kOiAjZjVmNWY1O1xyXG4gIGJvcmRlci1yYWRpdXM6IDhweDtcclxuICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gIGZsZXgtd3JhcDogd3JhcDtcclxuXHJcbiAgLnN0YXQtaXRlbSB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIGdhcDogNnB4O1xyXG4gICAgZm9udC1zaXplOiAxM3B4O1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC43KTtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgICAgY29sb3I6ICMxOTc2ZDI7XHJcbiAgICB9XHJcblxyXG4gICAgJi50cmFja2VyLXN0YXR1cyB7XHJcbiAgICAgIGN1cnNvcjogcG9pbnRlcjtcclxuICAgICAgcGFkZGluZzogNHB4IDhweDtcclxuICAgICAgbWFyZ2luOiAtNHB4O1xyXG4gICAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgICAgIHRyYW5zaXRpb246IGJhY2tncm91bmQtY29sb3IgMC4ycztcclxuXHJcbiAgICAgICY6aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQ6IHJnYmEoMCwgMCwgMCwgMC4wNSk7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgICYudHJhY2tlci1jb25uZWN0ZWQgbWF0LWljb24ge1xyXG4gICAgICAgIGNvbG9yOiAjMjhhNzQ1O1xyXG4gICAgICB9XHJcblxyXG4gICAgICAmLnRyYWNrZXItZXJyb3IgbWF0LWljb24ge1xyXG4gICAgICAgIGNvbG9yOiAjZGMzNTQ1O1xyXG4gICAgICB9XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4udHJhY2tlci13YXJuaW5nIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIHBhZGRpbmc6IDEycHggMTZweDtcclxuICBiYWNrZ3JvdW5kOiAjZmZmM2NkO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkICNmZmMxMDc7XHJcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgZm9udC1zaXplOiAxM3B4O1xyXG5cclxuICBtYXQtaWNvbiB7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuICB9XHJcblxyXG4gIHNwYW4ge1xyXG4gICAgZmxleDogMTtcclxuICAgIGNvbG9yOiAjODU2NDA0O1xyXG4gIH1cclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gIH1cclxufVxyXG5cclxuLnRhYi1jb250ZW50IHtcclxuICBwYWRkaW5nOiAxNnB4IDA7XHJcbiAgbWluLWhlaWdodDogMjgwcHg7XHJcbn1cclxuXHJcbi50cmFuc2ZlcnMtaGVhZGVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGp1c3RpZnktY29udGVudDogZmxleC1lbmQ7XHJcbiAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG59XHJcblxyXG4uZW1wdHktc3RhdGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDQ4cHg7XHJcbiAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC41KTtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgZm9udC1zaXplOiA2NHB4O1xyXG4gICAgd2lkdGg6IDY0cHg7XHJcbiAgICBoZWlnaHQ6IDY0cHg7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gICAgb3BhY2l0eTogMC41O1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgICBmb250LXNpemU6IDE2cHg7XHJcbiAgfVxyXG5cclxuICAuaGludCB7XHJcbiAgICBmb250LXNpemU6IDEzcHg7XHJcbiAgICBtYXJnaW4tdG9wOiA4cHg7XHJcbiAgfVxyXG59XHJcblxyXG4udHJhbnNmZXItbGlzdCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMTJweDtcclxufVxyXG5cclxuLnRyYW5zZmVyLWl0ZW0ge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDEycHg7XHJcbiAgcGFkZGluZzogMTJweDtcclxuICBiYWNrZ3JvdW5kOiAjZmFmYWZhO1xyXG4gIGJvcmRlci1yYWRpdXM6IDhweDtcclxuICBib3JkZXI6IDFweCBzb2xpZCAjZTBlMGUwO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIGJhY2tncm91bmQ6ICNmNWY1ZjU7XHJcbiAgfVxyXG5cclxuICAudHJhbnNmZXItaWNvbiB7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjhweDtcclxuICAgICAgd2lkdGg6IDI4cHg7XHJcbiAgICAgIGhlaWdodDogMjhweDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC50cmFuc2Zlci1pbmZvIHtcclxuICAgIGZsZXg6IDE7XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcblxyXG4gICAgLnRyYW5zZmVyLW5hbWUge1xyXG4gICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgICAgbWFyZ2luLWJvdHRvbTogNHB4O1xyXG4gICAgfVxyXG5cclxuICAgIC50cmFuc2Zlci1kZXRhaWxzIHtcclxuICAgICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiA4cHg7XHJcblxyXG4gICAgICBzcGFuIHtcclxuICAgICAgICAmOm5vdCg6bGFzdC1jaGlsZCk6OmFmdGVyIHtcclxuICAgICAgICAgIGNvbnRlbnQ6ICcnO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIG1hdC1wcm9ncmVzcy1iYXIge1xyXG4gICAgICBoZWlnaHQ6IDZweDtcclxuICAgICAgYm9yZGVyLXJhZGl1czogM3B4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnRyYW5zZmVyLWFjdGlvbnMge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGdhcDogNHB4O1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgfVxyXG59XHJcblxyXG4uZG93bmxvYWQtdGFiIHtcclxuICBoMyB7XHJcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcclxuICB9XHJcblxyXG4gIC5oaW50IHtcclxuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNik7XHJcbiAgICBmb250LXNpemU6IDEzcHg7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG4gIH1cclxuXHJcbiAgLnByb2plY3Qtc2VsZWN0b3Ige1xyXG4gICAgd2lkdGg6IDEwMCU7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG4gIH1cclxuXHJcbiAgLm1hZ25ldC1pbnB1dCB7XHJcbiAgICB3aWR0aDogMTAwJTtcclxuICAgIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgfVxyXG5cclxuICAuZG93bmxvYWQtcGF0aC1wcmV2aWV3IHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgZ2FwOiA4cHg7XHJcbiAgICBtYXJnaW4tdG9wOiAxNnB4O1xyXG4gICAgcGFkZGluZzogMTJweDtcclxuICAgIGJhY2tncm91bmQ6ICNlOGY1ZTk7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgICBmb250LXNpemU6IDEzcHg7XHJcbiAgICBjb2xvcjogIzJlN2QzMjtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgIH1cclxuXHJcbiAgICBjb2RlIHtcclxuICAgICAgYmFja2dyb3VuZDogcmdiYSgwLCAwLCAwLCAwLjA1KTtcclxuICAgICAgcGFkZGluZzogMnB4IDZweDtcclxuICAgICAgYm9yZGVyLXJhZGl1czogM3B4O1xyXG4gICAgICBmb250LWZhbWlseTogbW9ub3NwYWNlO1xyXG4gICAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4uaW5mby10YWIge1xyXG4gIGgzIHtcclxuICAgIG1hcmdpbjogMCAwIDE2cHggMDtcclxuICB9XHJcblxyXG4gIGg0IHtcclxuICAgIG1hcmdpbjogMjRweCAwIDhweCAwO1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjcpO1xyXG4gICAgbGluZS1oZWlnaHQ6IDEuNjtcclxuICB9XHJcblxyXG4gIHVsLCBvbCB7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjcpO1xyXG4gICAgcGFkZGluZy1sZWZ0OiAyNHB4O1xyXG5cclxuICAgIGxpIHtcclxuICAgICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLmhpbnQge1xyXG4gICAgZm9udC1zaXplOiAxM3B4O1xyXG4gICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC41KTtcclxuICB9XHJcbn1cclxuXHJcbi5wcm9qZWN0cy10YWIge1xyXG4gIC5wcm9qZWN0cy1oZWFkZXIge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBtYXJnaW4tYm90dG9tOiAxNnB4O1xyXG5cclxuICAgIGgzIHtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLmxvYWRpbmctc3RhdGUge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICAgIGdhcDogMTJweDtcclxuICAgIHBhZGRpbmc6IDMycHg7XHJcbiAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtbGlzdCB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGdhcDogMTJweDtcclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWNhcmQge1xyXG4gICAgYm9yZGVyOiAxcHggc29saWQgI2UwZTBlMDtcclxuICAgIGJvcmRlci1yYWRpdXM6IDhweDtcclxuICAgIG92ZXJmbG93OiBoaWRkZW47XHJcblxyXG4gICAgLnByb2plY3QtaGVhZGVyIHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgZ2FwOiA4cHg7XHJcbiAgICAgIHBhZGRpbmc6IDEycHggMTZweDtcclxuICAgICAgYmFja2dyb3VuZDogI2Y1ZjVmNTtcclxuICAgICAgY3Vyc29yOiBwb2ludGVyO1xyXG4gICAgICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kLWNvbG9yIDAuMnM7XHJcblxyXG4gICAgICAmOmhvdmVyIHtcclxuICAgICAgICBiYWNrZ3JvdW5kOiAjZWVlZWVlO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAucHJvamVjdC1uYW1lIHtcclxuICAgICAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgICAgIGZsZXg6IDE7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIC5maWxlLWNvdW50IHtcclxuICAgICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjUpO1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMTNweDtcclxuICAgICAgfVxyXG5cclxuICAgICAgLnByb2plY3QtYWN0aW9ucyB7XHJcbiAgICAgICAgbWFyZ2luLWxlZnQ6IGF1dG87XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1maWxlcyB7XHJcbiAgICAgIHBhZGRpbmc6IDhweDtcclxuICAgICAgYmFja2dyb3VuZDogI2ZhZmFmYTtcclxuICAgICAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICNlMGUwZTA7XHJcblxyXG4gICAgICAuZmlsZS1pdGVtIHtcclxuICAgICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICAgICAgZ2FwOiAxMnB4O1xyXG4gICAgICAgIHBhZGRpbmc6IDhweCAxMnB4O1xyXG4gICAgICAgIGJvcmRlci1yYWRpdXM6IDRweDtcclxuXHJcbiAgICAgICAgJjpob3ZlciB7XHJcbiAgICAgICAgICBiYWNrZ3JvdW5kOiAjZjBmMGYwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC41KTtcclxuICAgICAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLmZpbGUtaW5mbyB7XHJcbiAgICAgICAgICBmbGV4OiAxO1xyXG4gICAgICAgICAgbWluLXdpZHRoOiAwO1xyXG5cclxuICAgICAgICAgIC5maWxlLW5hbWUge1xyXG4gICAgICAgICAgICBkaXNwbGF5OiBibG9jaztcclxuICAgICAgICAgICAgZm9udC1zaXplOiAxNHB4O1xyXG4gICAgICAgICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICAgICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICAgICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgICAgICAgIH1cclxuXHJcbiAgICAgICAgICAuZmlsZS1zaXplIHtcclxuICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgICAgICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjUpO1xyXG4gICAgICAgICAgfVxyXG4gICAgICAgIH1cclxuXHJcbiAgICAgICAgLmZpbGUtc3RhdHVzIHtcclxuICAgICAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgICAgICAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcblxyXG4gICAgICAgICAgLnBlZXItY291bnQge1xyXG4gICAgICAgICAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgICAgICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICAgICAgICBnYXA6IDRweDtcclxuICAgICAgICAgICAgZm9udC1zaXplOiAxMnB4O1xyXG4gICAgICAgICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xyXG5cclxuICAgICAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgICAgICAgICB3aWR0aDogMTZweDtcclxuICAgICAgICAgICAgICBoZWlnaHQ6IDE2cHg7XHJcblxyXG4gICAgICAgICAgICAgICYuc3RhdHVzLWxvY2FsIHtcclxuICAgICAgICAgICAgICAgIGNvbG9yOiAjMjhhNzQ1O1xyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAmLnN0YXR1cy1zZWVkaW5nIHtcclxuICAgICAgICAgICAgICAgIGNvbG9yOiAjMDA3YmZmO1xyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAmLnN0YXR1cy1kb3dubG9hZGluZyB7XHJcbiAgICAgICAgICAgICAgICBjb2xvcjogIzE3YTJiODtcclxuICAgICAgICAgICAgICB9XHJcbiAgICAgICAgICAgICAgJi5zdGF0dXMtY29tcGxldGVkIHtcclxuICAgICAgICAgICAgICAgIGNvbG9yOiAjMjhhNzQ1O1xyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAmLnN0YXR1cy1hdmFpbGFibGUge1xyXG4gICAgICAgICAgICAgICAgY29sb3I6ICNmZDdlMTQ7XHJcbiAgICAgICAgICAgICAgfVxyXG4gICAgICAgICAgICAgICYuc3RhdHVzLXVuYXZhaWxhYmxlIHtcclxuICAgICAgICAgICAgICAgIGNvbG9yOiAjNmM3NTdkO1xyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAmLnN0YXR1cy11bmtub3duIHtcclxuICAgICAgICAgICAgICAgIGNvbG9yOiAjNmM3NTdkO1xyXG4gICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgfVxyXG5cclxuICAgICAgICAgICAgLnN0YXR1cy10ZXh0IHtcclxuICAgICAgICAgICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG5cclxuICAgICAgICAuZmlsZS1hY3Rpb25zIHtcclxuICAgICAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLmJhZGdlIHtcclxuICBiYWNrZ3JvdW5kOiAjMTk3NmQyO1xyXG4gIGNvbG9yOiB3aGl0ZTtcclxuICBmb250LXNpemU6IDExcHg7XHJcbiAgcGFkZGluZzogMnB4IDZweDtcclxuICBib3JkZXItcmFkaXVzOiAxMHB4O1xyXG4gIG1hcmdpbi1sZWZ0OiA4cHg7XHJcbn1cclxuXHJcbjo6bmctZGVlcCAubWF0LXRhYi1sYWJlbCB7XHJcbiAgbWF0LWljb24ge1xyXG4gICAgbWFyZ2luLXJpZ2h0OiA4cHg7XHJcbiAgfVxyXG59XHJcbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
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
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/divider */ 1528);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @ngx-translate/core */ 8699);









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
  ngOnInit() {}
  onCreateProject() {
    this.dialogRef.close(this.config);
  }
  onCancel() {
    this.dialogRef.close(null);
  }
  static {
    this.ɵfac = function ProjectCreateConfigDialogComponent_Factory(t) {
      return new (t || ProjectCreateConfigDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MAT_LEGACY_DIALOG_DATA));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: ProjectCreateConfigDialogComponent,
      selectors: [["app-project-create-config-dialog"]],
      decls: 40,
      vars: 27,
      consts: [["mat-dialog-title", ""], [1, "config-container"], [1, "project-path"], [1, "path-text"], [1, "config-options"], [1, "option-item"], ["color", "primary", 3, "ngModel", "ngModelChange"], [1, "option-description"], ["align", "end"], ["mat-button", "", 3, "click"], ["mat-raised-button", "", "color", "primary", 3, "click"]],
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
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](14, "div", 4)(15, "div", 5)(16, "mat-checkbox", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_16_listener($event) {
            return ctx.config.addCopilotInstructions = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](17, "strong");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](19, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](20, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](21);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](22, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](23, "div", 5)(24, "mat-checkbox", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngModelChange", function ProjectCreateConfigDialogComponent_Template_mat_checkbox_ngModelChange_24_listener($event) {
            return ctx.config.initializeGit = $event;
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](25, "strong");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](26);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](27, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](28, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](29);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](30, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](31, "mat-dialog-actions", 8)(32, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_32_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](33);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](34, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](35, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectCreateConfigDialogComponent_Template_button_click_35_listener() {
            return ctx.onCreateProject();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](36, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](37, "add");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](38);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](39, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 11, "PROJECT_CONFIG.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](10, 13, "PROJECT_CONFIG.PROJECT_PATH"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.config.projectPath);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.config.addCopilotInstructions);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](19, 15, "PROJECT_CONFIG.ADD_COPILOT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](22, 17, "PROJECT_CONFIG.ADD_COPILOT_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx.config.initializeGit);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](27, 19, "PROJECT_CONFIG.INIT_GIT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](30, 21, "PROJECT_CONFIG.INIT_GIT_DESC"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](34, 23, "COMMON.CANCEL"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](39, 25, "PROJECT_CONFIG.CREATE_PROJECT"), " ");
        }
      },
      dependencies: [_angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_2__.MatLegacyCheckbox, _angular_material_divider__WEBPACK_IMPORTED_MODULE_3__.MatDivider, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_4__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__.MatIcon, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_1__.MatLegacyDialogActions, _angular_forms__WEBPACK_IMPORTED_MODULE_6__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_6__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_7__.TranslatePipe],
      styles: [".config-container[_ngcontent-%COMP%] {\n  min-width: 400px;\n  padding: 20px 0;\n}\n\n.project-path[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 15px;\n  background-color: rgba(0, 0, 0, 0.04);\n  border-radius: 4px;\n  margin-bottom: 20px;\n}\n.project-path[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #666;\n}\n.project-path[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  font-family: monospace;\n  color: #444;\n  word-break: break-all;\n}\n\n.config-options[_ngcontent-%COMP%] {\n  padding: 20px 0;\n}\n\n.option-item[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n}\n.option-item[_ngcontent-%COMP%]   mat-checkbox[_ngcontent-%COMP%] {\n  display: block;\n}\n.option-item[_ngcontent-%COMP%]   .option-description[_ngcontent-%COMP%] {\n  margin-top: 5px;\n  margin-left: 28px;\n  font-size: 12px;\n  color: #666;\n  line-height: 1.4;\n}\n\nmat-divider[_ngcontent-%COMP%] {\n  margin: 20px 0;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding: 16px 24px;\n}\nmat-dialog-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  margin-right: 4px;\n  font-size: 20px;\n  height: 20px;\n  width: 20px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wcm9qZWN0LWNyZWF0ZS1jb25maWcvcHJvamVjdC1jcmVhdGUtY29uZmlnLWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGdCQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGFBQUE7RUFDQSxxQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7QUFDRjtBQUNFO0VBQ0UsV0FBQTtBQUNKO0FBRUU7RUFDRSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxxQkFBQTtBQUFKOztBQUlBO0VBQ0UsZUFBQTtBQURGOztBQUlBO0VBQ0UsbUJBQUE7QUFERjtBQUdFO0VBQ0UsY0FBQTtBQURKO0FBSUU7RUFDRSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGdCQUFBO0FBRko7O0FBTUE7RUFDRSxjQUFBO0FBSEY7O0FBTUE7RUFDRSxrQkFBQTtBQUhGO0FBTUk7RUFDRSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtBQUpOIiwic291cmNlc0NvbnRlbnQiOlsiLmNvbmZpZy1jb250YWluZXIge1xyXG4gIG1pbi13aWR0aDogNDAwcHg7XHJcbiAgcGFkZGluZzogMjBweCAwO1xyXG59XHJcblxyXG4ucHJvamVjdC1wYXRoIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiAxMHB4O1xyXG4gIHBhZGRpbmc6IDE1cHg7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgwLCAwLCAwLCAwLjA0KTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjBweDtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgY29sb3I6ICM2NjY7XHJcbiAgfVxyXG5cclxuICAucGF0aC10ZXh0IHtcclxuICAgIGZvbnQtZmFtaWx5OiBtb25vc3BhY2U7XHJcbiAgICBjb2xvcjogIzQ0NDtcclxuICAgIHdvcmQtYnJlYWs6IGJyZWFrLWFsbDtcclxuICB9XHJcbn1cclxuXHJcbi5jb25maWctb3B0aW9ucyB7XHJcbiAgcGFkZGluZzogMjBweCAwO1xyXG59XHJcblxyXG4ub3B0aW9uLWl0ZW0ge1xyXG4gIG1hcmdpbi1ib3R0b206IDIwcHg7XHJcblxyXG4gIG1hdC1jaGVja2JveCB7XHJcbiAgICBkaXNwbGF5OiBibG9jaztcclxuICB9XHJcblxyXG4gIC5vcHRpb24tZGVzY3JpcHRpb24ge1xyXG4gICAgbWFyZ2luLXRvcDogNXB4O1xyXG4gICAgbWFyZ2luLWxlZnQ6IDI4cHg7XHJcbiAgICBmb250LXNpemU6IDEycHg7XHJcbiAgICBjb2xvcjogIzY2NjtcclxuICAgIGxpbmUtaGVpZ2h0OiAxLjQ7XHJcbiAgfVxyXG59XHJcblxyXG5tYXQtZGl2aWRlciB7XHJcbiAgbWFyZ2luOiAyMHB4IDA7XHJcbn1cclxuXHJcbm1hdC1kaWFsb2ctYWN0aW9ucyB7XHJcbiAgcGFkZGluZzogMTZweCAyNHB4O1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBtYXJnaW4tcmlnaHQ6IDRweDtcclxuICAgICAgZm9udC1zaXplOiAyMHB4O1xyXG4gICAgICBoZWlnaHQ6IDIwcHg7XHJcbiAgICAgIHdpZHRoOiAyMHB4O1xyXG4gICAgfVxyXG4gIH1cclxufSJdLCJzb3VyY2VSb290IjoiIn0= */"]
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
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @ngx-translate/core */ 8699);












function ProjectEditDialogComponent_mat_error_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-error");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 1, "PROJECTS.NAME_REQUIRED"), " ");
  }
}
class ProjectEditDialogComponent {
  constructor(fb, dialogRef, data) {
    this.fb = fb;
    this.dialogRef = dialogRef;
    this.data = data;
    this.descriptionMaxLength = 200;
    this.nameMaxLength = 255;
    this.form = this.fb.group({
      name: [data.name ?? '', [_angular_forms__WEBPACK_IMPORTED_MODULE_1__.Validators.required, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.Validators.maxLength(this.nameMaxLength)]],
      description: [data.description ?? '', [_angular_forms__WEBPACK_IMPORTED_MODULE_1__.Validators.maxLength(this.descriptionMaxLength)]]
    });
  }
  get descriptionLength() {
    return (this.form.value.description ?? '').length;
  }
  onCancel() {
    this.dialogRef.close();
  }
  onSave() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value;
    this.dialogRef.close({
      id: this.data.id,
      name: (value.name ?? '').trim(),
      description: (value.description ?? '').trim()
    });
  }
  static {
    this.ɵfac = function ProjectEditDialogComponent_Factory(t) {
      return new (t || ProjectEditDialogComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_forms__WEBPACK_IMPORTED_MODULE_1__.FormBuilder), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MAT_LEGACY_DIALOG_DATA));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: ProjectEditDialogComponent,
      selectors: [["app-project-edit-dialog"]],
      decls: 33,
      vars: 27,
      consts: [["mat-dialog-title", ""], [1, "title-icon"], [1, "project-edit-form", 3, "formGroup", "ngSubmit"], [1, "path-readonly", 3, "matTooltip"], [1, "path-text"], ["appearance", "outline", 1, "full-width"], ["matInput", "", "formControlName", "name", "required", "", 3, "maxlength"], [4, "ngIf"], ["matInput", "", "formControlName", "description", "rows", "4", 3, "maxlength", "placeholder"], ["align", "end"], ["mat-button", "", "type", "button", 3, "click"], ["mat-flat-button", "", "color", "primary", "type", "submit", 3, "disabled"]],
      template: function ProjectEditDialogComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h2", 0)(1, "mat-icon", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2, "edit");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](4, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "form", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("ngSubmit", function ProjectEditDialogComponent_Template_form_ngSubmit_5_listener() {
            return ctx.onSave();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "mat-dialog-content")(7, "p", 3)(8, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "mat-form-field", 5)(13, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](14);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](15, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](16, "input", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](17, ProjectEditDialogComponent_mat_error_17_Template, 3, 3, "mat-error", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](18, "mat-form-field", 5)(19, "mat-label");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](22, "textarea", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](24, "mat-hint", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](26, "mat-dialog-actions", 9)(27, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function ProjectEditDialogComponent_Template_button_click_27_listener() {
            return ctx.onCancel();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](28);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](29, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](30, "button", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](31);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](32, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](4, 15, "PROJECTS.EDIT_PROJECT_TITLE"), "\n");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("formGroup", ctx.form);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("matTooltip", ctx.data.path);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.data.path);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](15, 17, "PROJECTS.NAME"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("maxlength", ctx.nameMaxLength);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx.form.get("name").hasError("required"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](21, 19, "PROJECTS.DESCRIPTION"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("maxlength", ctx.descriptionMaxLength)("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](23, 21, "PROJECTS.DESCRIPTION_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate2"]("", ctx.descriptionLength, "/", ctx.descriptionMaxLength, "");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](29, 23, "PROJECTS.CANCEL"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("disabled", ctx.form.invalid);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](32, 25, "PROJECTS.SAVE"), " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_3__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_4__.MatLegacyError, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_4__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_4__.MatLegacyHint, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_4__.MatLegacyLabel, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_5__.MatLegacyInput, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_6__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__.MatIcon, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_2__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_8__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_1__["ɵNgNoValidate"], _angular_forms__WEBPACK_IMPORTED_MODULE_1__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.NgControlStatusGroup, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.RequiredValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.MaxLengthValidator, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.FormGroupDirective, _angular_forms__WEBPACK_IMPORTED_MODULE_1__.FormControlName, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_9__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n  min-width: 480px;\n  max-width: 560px;\n}\n\nh2[mat-dialog-title][_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  color: var(--mde-text-primary);\n  margin: 0;\n}\nh2[mat-dialog-title][_ngcontent-%COMP%]   .title-icon[_ngcontent-%COMP%] {\n  font-size: 22px;\n  width: 22px;\n  height: 22px;\n  color: #667eea;\n}\n\n.project-edit-form[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.path-readonly[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 8px 12px;\n  background: var(--mde-bg-secondary);\n  border: 1px solid var(--mde-border-color);\n  border-radius: 6px;\n  font-size: 0.8rem;\n  color: var(--mde-text-secondary);\n  margin: 0 0 16px 0;\n  overflow: hidden;\n}\n.path-readonly[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  color: #a0aec0;\n  flex-shrink: 0;\n}\n.path-readonly[_ngcontent-%COMP%]   .path-text[_ngcontent-%COMP%] {\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  flex: 1;\n  min-width: 0;\n}\n\n.full-width[_ngcontent-%COMP%] {\n  width: 100%;\n}\n\ntextarea[_ngcontent-%COMP%] {\n  resize: vertical;\n  min-height: 80px;\n}\n\nmat-dialog-actions[_ngcontent-%COMP%] {\n  padding-top: 8px;\n  gap: 8px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvZGlhbG9ncy9wcm9qZWN0LWVkaXQvcHJvamVjdC1lZGl0LWRpYWxvZy5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGNBQUE7RUFDQSxnQkFBQTtFQUNBLGdCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsOEJBQUE7RUFDQSxTQUFBO0FBQ0Y7QUFDRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUFDSjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLG1DQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQUY7QUFFRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7RUFDQSxjQUFBO0FBQUo7QUFHRTtFQUNFLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLE9BQUE7RUFDQSxZQUFBO0FBREo7O0FBS0E7RUFDRSxXQUFBO0FBRkY7O0FBS0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBRkY7O0FBS0E7RUFDRSxnQkFBQTtFQUNBLFFBQUE7QUFGRiIsInNvdXJjZXNDb250ZW50IjpbIjpob3N0IHtcclxuICBkaXNwbGF5OiBibG9jaztcclxuICBtaW4td2lkdGg6IDQ4MHB4O1xyXG4gIG1heC13aWR0aDogNTYwcHg7XHJcbn1cclxuXHJcbmgyW21hdC1kaWFsb2ctdGl0bGVdIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xyXG4gIG1hcmdpbjogMDtcclxuXHJcbiAgLnRpdGxlLWljb24ge1xyXG4gICAgZm9udC1zaXplOiAyMnB4O1xyXG4gICAgd2lkdGg6IDIycHg7XHJcbiAgICBoZWlnaHQ6IDIycHg7XHJcbiAgICBjb2xvcjogIzY2N2VlYTtcclxuICB9XHJcbn1cclxuXHJcbi5wcm9qZWN0LWVkaXQtZm9ybSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogOHB4O1xyXG59XHJcblxyXG4ucGF0aC1yZWFkb25seSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGdhcDogOHB4O1xyXG4gIHBhZGRpbmc6IDhweCAxMnB4O1xyXG4gIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1zZWNvbmRhcnkpO1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xyXG4gIGJvcmRlci1yYWRpdXM6IDZweDtcclxuICBmb250LXNpemU6IDAuOHJlbTtcclxuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcclxuICBtYXJnaW46IDAgMCAxNnB4IDA7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuXHJcbiAgbWF0LWljb24ge1xyXG4gICAgZm9udC1zaXplOiAxOHB4O1xyXG4gICAgd2lkdGg6IDE4cHg7XHJcbiAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICBjb2xvcjogI2EwYWVjMDtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gIH1cclxuXHJcbiAgLnBhdGgtdGV4dCB7XHJcbiAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICAgIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gICAgZmxleDogMTtcclxuICAgIG1pbi13aWR0aDogMDtcclxuICB9XHJcbn1cclxuXHJcbi5mdWxsLXdpZHRoIHtcclxuICB3aWR0aDogMTAwJTtcclxufVxyXG5cclxudGV4dGFyZWEge1xyXG4gIHJlc2l6ZTogdmVydGljYWw7XHJcbiAgbWluLWhlaWdodDogODBweDtcclxufVxyXG5cclxubWF0LWRpYWxvZy1hY3Rpb25zIHtcclxuICBwYWRkaW5nLXRvcDogOHB4O1xyXG4gIGdhcDogOHB4O1xyXG59XHJcbiJdLCJzb3VyY2VSb290IjoiIn0= */"]
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
      styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  overflow: hidden;\n}\n\nh1[_ngcontent-%COMP%] {\n  margin: 0 0 20px 0;\n  font-weight: 400;\n  font-size: 1.8rem;\n  flex-shrink: 0;\n}\n\n.projects-list-container[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 0 8px 0 0;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n  scrollbar-width: thin;\n  scrollbar-color: rgba(0, 0, 0, 0.2) transparent;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: transparent;\n  border-radius: 4px;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: rgba(0, 0, 0, 0.2);\n  border-radius: 4px;\n  -webkit-transition: background 0.3s ease;\n  transition: background 0.3s ease;\n}\n.projects-list-container[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: rgba(0, 0, 0, 0.3);\n}\n\nmat-card[_ngcontent-%COMP%] {\n  transition: transform 0.2s ease, box-shadow 0.2s ease;\n  cursor: pointer;\n}\nmat-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvb3Blbi1yZWNlbnQvb3Blbi1yZWNlbnQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtFQXVCQSxxQkFBQTtFQUNBLCtDQUFBO0FBckJGO0FBQUU7RUFDRSxVQUFBO0FBRUo7QUFDRTtFQUNFLHVCQUFBO0VBQ0Esa0JBQUE7QUFDSjtBQUVFO0VBQ0UsOEJBQUE7RUFDQSxrQkFBQTtFQUNBLHdDQUFBO0VBQUEsZ0NBQUE7QUFBSjtBQUVJO0VBQ0UsOEJBQUE7QUFBTjs7QUFVQTtFQUNFLHFEQUFBO0VBQ0EsZUFBQTtBQVBGO0FBU0U7RUFDRSwyQkFBQTtFQUNBLDBDQUFBO0FBUEoiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGhlaWdodDogMTAwJTtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG59XHJcblxyXG5oMSB7XHJcbiAgbWFyZ2luOiAwIDAgMjBweCAwO1xyXG4gIGZvbnQtd2VpZ2h0OiA0MDA7XHJcbiAgZm9udC1zaXplOiAxLjhyZW07XHJcbiAgZmxleC1zaHJpbms6IDA7XHJcbn1cclxuXHJcbi5wcm9qZWN0cy1saXN0LWNvbnRhaW5lciB7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIHBhZGRpbmc6IDAgOHB4IDAgMDtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgZ2FwOiAxNnB4O1xyXG5cclxuICAvLyBDdXN0b20gc2Nyb2xsYmFyIHN0eWxpbmdcclxuICAmOjotd2Via2l0LXNjcm9sbGJhciB7XHJcbiAgICB3aWR0aDogOHB4O1xyXG4gIH1cclxuXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdHJhY2sge1xyXG4gICAgYmFja2dyb3VuZDogdHJhbnNwYXJlbnQ7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgfVxyXG5cclxuICAmOjotd2Via2l0LXNjcm9sbGJhci10aHVtYiB7XHJcbiAgICBiYWNrZ3JvdW5kOiByZ2JhKDAsIDAsIDAsIDAuMik7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuM3MgZWFzZTtcclxuXHJcbiAgICAmOmhvdmVyIHtcclxuICAgICAgYmFja2dyb3VuZDogcmdiYSgwLCAwLCAwLCAwLjMpO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLy8gRmlyZWZveCBzY3JvbGxiYXJcclxuICBzY3JvbGxiYXItd2lkdGg6IHRoaW47XHJcbiAgc2Nyb2xsYmFyLWNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuMikgdHJhbnNwYXJlbnQ7XHJcbn1cclxuXHJcbi8vIFNtb290aCBjYXJkIGhvdmVyIGVmZmVjdFxyXG5tYXQtY2FyZCB7XHJcbiAgdHJhbnNpdGlvbjogdHJhbnNmb3JtIDAuMnMgZWFzZSwgYm94LXNoYWRvdyAwLjJzIGVhc2U7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG5cclxuICAmOmhvdmVyIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcclxuICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgwLCAwLCAwLCAwLjE1KTtcclxuICB9XHJcbn1cclxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"],
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
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../dialogs/catalog-picker/catalog-picker.component */ 5922);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../services/project-settings.service */ 5450);
/* harmony import */ var _services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/compatibility-mode.service */ 7929);
/* harmony import */ var _services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../services/ide-configuration.service */ 9909);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _md_explorer_services_external_apps_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../md-explorer/services/external-apps.service */ 9595);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-checkbox */ 8469);
/* harmony import */ var _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-radio */ 3493);
/* harmony import */ var _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/legacy-menu */ 1051);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-tabs */ 2821);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-progress-spinner */ 7578);
/* harmony import */ var _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/material/legacy-progress-bar */ 5042);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/forms */ 2508);
























function ProjectSettingsComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    const _r20 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_14_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r20);
      const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r19.rule1Enabled = $event);
    })("change", function ProjectSettingsComponent_div_14_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r20);
      const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r21.onRule1Change());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r0.rule1Enabled)("disabled", ctx_r0.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.RULE1_LABEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.RULE1_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](12, 9, "PROJECT_SETTINGS.RULE1_NOTE"));
  }
}
function ProjectSettingsComponent_div_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_23_Template(rf, ctx) {
  if (rf & 1) {
    const _r23 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_23_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r23);
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r22.githubModeEnabled = $event);
    })("change", function ProjectSettingsComponent_div_23_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r23);
      const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r24.onGitHubModeChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r3.githubModeEnabled)("disabled", ctx_r3.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.GITHUB_MODE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.GITHUB_MODE_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_30_Template(rf, ctx) {
  if (rf & 1) {
    const _r26 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_30_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r26);
      const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r25.linkIndexingEnabled = $event);
    })("change", function ProjectSettingsComponent_div_30_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r26);
      const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r27.onLinkIndexingChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r4.linkIndexingEnabled)("disabled", ctx_r4.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.ENABLE_LINK_INDEXING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.LINK_INDEXING_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](12, 9, "PROJECT_SETTINGS.LINK_INDEXING_NOTE"));
  }
}
function ProjectSettingsComponent_div_37_Template(rf, ctx) {
  if (rf & 1) {
    const _r29 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_37_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r29);
      const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r28.stickyScrollEnabled = $event);
    })("change", function ProjectSettingsComponent_div_37_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r29);
      const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r30.onStickyScrollChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r5.stickyScrollEnabled)("disabled", ctx_r5.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.STICKY_SCROLL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.STICKY_SCROLL_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_38_Template(rf, ctx) {
  if (rf & 1) {
    const _r32 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_38_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32);
      const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r31.plantUmlKeepOriginalColorsEnabled = $event);
    })("change", function ProjectSettingsComponent_div_38_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r32);
      const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r33.onPlantUmlKeepOriginalColorsChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r6.plantUmlKeepOriginalColorsEnabled)("disabled", ctx_r6.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.PLANTUML_KEEP_ORIGINAL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.PLANTUML_KEEP_ORIGINAL_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_48_Template(rf, ctx) {
  if (rf & 1) {
    const _r35 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "mat-radio-group", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_48_Template_mat_radio_group_ngModelChange_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r35);
      const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r34.selectedIde = $event);
    })("change", function ProjectSettingsComponent_div_48_Template_mat_radio_group_change_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r35);
      const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r36.onIdeChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "mat-radio-button", 22)(6, "span", 23)(7, "mat-icon", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8, "code");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](11, "mat-radio-button", 25)(12, "span", 23)(13, "mat-icon", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](14, "terminal");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](16, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 5, "PROJECT_SETTINGS.IDE_SELECT_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r7.selectedIde)("disabled", ctx_r7.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](10, 7, "PROJECT_SETTINGS.VSCODE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](16, 9, "PROJECT_SETTINGS.INTELLIJ"), " ");
  }
}
function ProjectSettingsComponent_div_49_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_50_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_div_67_Template(rf, ctx) {
  if (rf & 1) {
    const _r38 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_67_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r38);
      const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r37.copilotCliAutoSelectEnabled = $event);
    })("change", function ProjectSettingsComponent_div_67_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r38);
      const ctx_r39 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r39.onCopilotCliAutoSelectChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r10.copilotCliAutoSelectEnabled)("disabled", ctx_r10.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 4, "PROJECT_SETTINGS.COPILOT_CLI_AUTO_SELECT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 6, "PROJECT_SETTINGS.COPILOT_CLI_AUTO_SELECT_DESC"), " ");
  }
}
function ProjectSettingsComponent_div_74_Template(rf, ctx) {
  if (rf & 1) {
    const _r41 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "mat-checkbox", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_74_Template_mat_checkbox_ngModelChange_1_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r41);
      const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r40.ragEnabled = $event);
    })("change", function ProjectSettingsComponent_div_74_Template_mat_checkbox_change_1_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r41);
      const ctx_r42 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r42.onRagChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "p", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](8, "br")(9, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r11.ragEnabled)("disabled", ctx_r11.saving || !ctx_r11.ragModelInstalled);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 5, "PROJECT_SETTINGS.ENABLE_RAG"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 7, "PROJECT_SETTINGS.RAG_DESC"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](11, 9, "PROJECT_SETTINGS.RAG_INCREMENTAL"), " ");
  }
}
function ProjectSettingsComponent_div_75_div_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 28)(1, "span", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.INDEXED_CHUNKS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r43.ragChunksCount);
  }
}
function ProjectSettingsComponent_div_75_div_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 28)(1, "span", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.WITH_EMBEDDINGS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r44.ragEmbeddedCount);
  }
}
function ProjectSettingsComponent_div_75_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 27)(1, "div", 28)(2, "span", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](9, ProjectSettingsComponent_div_75_div_9_Template, 6, 4, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](10, ProjectSettingsComponent_div_75_div_10_Template, 6, 4, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 8, "PROJECT_SETTINGS.EMBEDDING_MODEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("status-ok", ctx_r12.ragModelInstalled)("status-missing", !ctx_r12.ragModelInstalled);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r12.ragModelInstalled ? _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 10, "PROJECT_SETTINGS.INSTALLED") : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](8, 12, "PROJECT_SETTINGS.NOT_INSTALLED"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r12.ragEnabled);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r12.ragEnabled);
  }
}
function ProjectSettingsComponent_div_76_mat_icon_3_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1, "refresh");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
}
function ProjectSettingsComponent_div_76_mat_spinner_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "mat-spinner", 39);
  }
}
function ProjectSettingsComponent_div_76_p_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "p", 17)(1, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](4, "br");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 2, "PROJECT_SETTINGS.FORCE_REINDEX_DESC"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 4, "PROJECT_SETTINGS.CLEAR_INDEX_DESC"));
  }
}
const _c0 = function (a0, a1, a2) {
  return {
    processed: a0,
    total: a1,
    percent: a2
  };
};
function ProjectSettingsComponent_div_76_div_14_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "number");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r50 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind2"](2, 1, "PROJECT_SETTINGS.PROCESSING_FILES", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpureFunction3"](7, _c0, ctx_r50.ragProcessed, ctx_r50.ragTotal, _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind2"](3, 4, ctx_r50.ragProgress, "1.0-0"))), " ");
  }
}
function ProjectSettingsComponent_div_76_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-progress-bar", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, ProjectSettingsComponent_div_76_div_14_span_2_Template, 4, 11, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("value", ctx_r48.ragProgress);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r48.ragTotal > 0);
  }
}
function ProjectSettingsComponent_div_76_p_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "p", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r49.ragMessage);
  }
}
function ProjectSettingsComponent_div_76_Template(rf, ctx) {
  if (rf & 1) {
    const _r52 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 14)(1, "div", 31)(2, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_76_Template_button_click_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r52);
      const ctx_r51 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r51.onRagReindex());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](3, ProjectSettingsComponent_div_76_mat_icon_3_Template, 2, 0, "mat-icon", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](4, ProjectSettingsComponent_div_76_mat_spinner_4_Template, 1, 0, "mat-spinner", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "button", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_76_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r52);
      const ctx_r53 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r53.onRagClear());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](10, "delete_sweep");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](12, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](13, ProjectSettingsComponent_div_76_p_13_Template, 8, 6, "p", 36);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](14, ProjectSettingsComponent_div_76_div_14_Template, 3, 2, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](15, ProjectSettingsComponent_div_76_p_15_Template, 2, 1, "p", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx_r13.ragReindexing || ctx_r13.saving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx_r13.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r13.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r13.ragReindexing ? _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 9, "PROJECT_SETTINGS.INDEXING") : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 11, "PROJECT_SETTINGS.FORCE_REINDEX"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx_r13.ragReindexing || ctx_r13.saving || ctx_r13.ragChunksCount === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](12, 13, "PROJECT_SETTINGS.CLEAR_INDEX"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx_r13.ragMessage && !ctx_r13.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r13.ragReindexing);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r13.ragMessage);
  }
}
function ProjectSettingsComponent_div_77_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_SETTINGS"));
  }
}
function ProjectSettingsComponent_div_78_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "COMMON.SAVING"));
  }
}
function ProjectSettingsComponent_mat_card_79_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "mat-card", 8)(1, "mat-card-content")(2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 1, "PROJECT_SETTINGS.NO_EMBEDDING_MODEL"), " ");
  }
}
function ProjectSettingsComponent_div_82_mat_spinner_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "mat-spinner", 53);
  }
}
function ProjectSettingsComponent_div_82_span_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r55 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r55.appsSavedMessage);
  }
}
function ProjectSettingsComponent_div_82_div_20_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 55)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "apps");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "p", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](5, 2, "PROJECT_SETTINGS.NO_APPS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](8, 4, "PROJECT_SETTINGS.NO_APPS_HINT"));
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_span_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](node_r59.name);
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template(rf, ctx) {
  if (rf & 1) {
    const _r68 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "input", 71);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template_input_ngModelChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r68);
      const ctx_r67 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r67.editingCategoryName = $event);
    })("keydown.enter", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template_input_keydown_enter_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r68);
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
      const ctx_r69 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r69.confirmRenameCategory(node_r59));
    })("keydown.escape", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template_input_keydown_escape_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r68);
      const ctx_r71 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r71.cancelRenameCategory());
    })("blur", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template_input_blur_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r68);
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
      const ctx_r72 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r72.confirmRenameCategory(node_r59));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r63 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r63.editingCategoryName);
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_button_7_Template(rf, ctx) {
  if (rf & 1) {
    const _r76 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_button_7_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r76);
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
      const ctx_r74 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r74.startRenameCategory(node_r59));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](1, 1, "COMMON.RENAME"));
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_div_12_Template(rf, ctx) {
  if (rf & 1) {
    const _r80 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 73)(1, "mat-icon", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span", 66)(6, "button", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_div_12_Template_button_click_6_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r80);
      const child_r77 = restoredCtx.$implicit;
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
      const ctx_r78 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r78.moveAppToRoot(node_r59, child_r77));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](9, "arrow_upward");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "button", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_div_12_Template_button_click_10_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r80);
      const child_r77 = restoredCtx.$implicit;
      const ctx_r81 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](5);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r81.removeApp(child_r77.appId));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](13, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const child_r77 = ctx.$implicit;
    const ctx_r65 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r65.getAppIcon(child_r77.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r65.getAppName(child_r77.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 4, "PROJECT_SETTINGS.MOVE_TO_ROOT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](11, 6, "PROJECT_SETTINGS.REMOVE_FROM_PROJECT"));
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r84 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 61)(1, "div", 62)(2, "mat-icon", 63);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](4, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_span_4_Template, 2, 1, "span", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](5, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_input_5_Template, 1, 1, "input", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "span", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](7, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_button_7_Template, 4, 3, "button", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "button", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_Template_button_click_8_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r84);
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
      const ctx_r82 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r82.deleteCategory(node_r59));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](12, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_div_12_Template, 14, 8, "div", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    const ctx_r60 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r60.editingCategoryId !== node_r59.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r60.editingCategoryId === node_r59.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r60.editingCategoryId !== node_r59.id);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](9, 5, "PROJECT_SETTINGS.DELETE_CATEGORY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", node_r59.children);
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_button_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3, "arrow_downward");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    const _r87 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵreference"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matMenuTriggerFor", _r87)("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](1, 2, "PROJECT_SETTINGS.MOVE_TO_CATEGORY"));
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_button_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r92 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "button", 81);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_button_9_Template_button_click_0_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r92);
      const cat_r89 = restoredCtx.$implicit;
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
      const ctx_r90 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r90.moveAppToCategory(node_r59, cat_r89));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2, "folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const cat_r89 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", cat_r89.name, " ");
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r95 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 76)(1, "mat-icon", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](6, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_button_6_Template, 4, 4, "button", 77);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "mat-menu", null, 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](9, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_button_9_Template, 4, 1, "button", 79);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "button", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_Template_button_click_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r95);
      const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
      const ctx_r93 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r93.removeApp(node_r59.appId));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](13, "close");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const node_r59 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    const ctx_r61 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r61.getAppIcon(node_r59.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r61.getAppName(node_r59.appId));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r61.getCategories().length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r61.getCategories());
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](11, 5, "PROJECT_SETTINGS.REMOVE_FROM_PROJECT"));
  }
}
function ProjectSettingsComponent_div_82_div_21_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_1_Template, 13, 7, "div", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, ProjectSettingsComponent_div_82_div_21_ng_container_1_div_2_Template, 14, 7, "div", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const node_r59 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", node_r59.type === "category");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", node_r59.type === "app");
  }
}
function ProjectSettingsComponent_div_82_div_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, ProjectSettingsComponent_div_82_div_21_ng_container_1_Template, 3, 2, "ng-container", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r57 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r57.appsTree);
  }
}
function ProjectSettingsComponent_div_82_Template(rf, ctx) {
  if (rf & 1) {
    const _r98 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 3)(1, "div", 45)(2, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_Template_button_click_2_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r98);
      const ctx_r97 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r97.openCatalogPicker());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "add");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "button", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_Template_button_click_7_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r98);
      const ctx_r99 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r99.addCategory());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](8, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](9, "create_new_folder");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](12, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "button", 48);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_div_82_Template_button_click_13_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r98);
      const ctx_r100 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r100.saveAppsTree());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](14, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15, "save");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](17, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](18, ProjectSettingsComponent_div_82_mat_spinner_18_Template, 1, 0, "mat-spinner", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](19, ProjectSettingsComponent_div_82_span_19_Template, 2, 1, "span", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](20, ProjectSettingsComponent_div_82_div_20_Template, 9, 6, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](21, ProjectSettingsComponent_div_82_div_21_Template, 2, 1, "div", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx_r17.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 10, "PROJECT_SETTINGS.ADD_FROM_CATALOG"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx_r17.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](11, 12, "PROJECT_SETTINGS.NEW_CATEGORY"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx_r17.appsSaving || ctx_r17.appsTree.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](17, 14, "COMMON.SAVE"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r17.appsSaving);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r17.appsSavedMessage);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r17.appsTree.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r17.appsTree.length > 0);
  }
}
function ProjectSettingsComponent_div_83_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "mat-spinner", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 1, "PROJECT_SETTINGS.LOADING_APPS"));
  }
}
const _c1 = function (a0) {
  return {
    name: a0
  };
};
class ProjectSettingsComponent {
  constructor(dialogRef, data, projectSettingsService, compatibilityService, ideConfigService, serverMessages, projectsService, externalAppsService, mdFileService, dialog, translate) {
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
    this.rule1Enabled = false;
    this.linkIndexingEnabled = true;
    this.plantUmlKeepOriginalColorsEnabled = false;
    this.copilotCliAutoSelectEnabled = true;
    this.githubModeEnabled = false;
    this.stickyScrollEnabled = true;
    this.selectedIde = 'vscode';
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
    this.projectId = data.projectId;
    this.projectName = data.projectName;
    this.projectPath = data.projectPath;
  }
  ngOnInit() {
    this.loadSettings();
    this.loadAppsConfig();
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
  }
  ngOnDestroy() {
    this.ragProgressSub?.unsubscribe();
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
    let copilotCliAutoSelectLoaded = false;
    const checkIfDone = () => {
      if (rule1Loaded && linkIndexingLoaded && compatibilityLoaded && ideConfigLoaded && ragLoaded && stickyScrollLoaded && plantUmlKeepOriginalColorsLoaded && copilotCliAutoSelectLoaded) {
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
    // Load Copilot CLI Auto-Select setting
    this.projectSettingsService.getCopilotCliAutoSelectSetting(this.projectPath).subscribe({
      next: response => {
        this.copilotCliAutoSelectEnabled = response.enabled;
        copilotCliAutoSelectLoaded = true;
        checkIfDone();
      },
      error: error => {
        console.error('Error loading Copilot CLI Auto-Select setting:', error);
        copilotCliAutoSelectLoaded = true;
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
        this.selectedIde = response.selectedIde || 'vscode';
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
  onCopilotCliAutoSelectChange() {
    this.saving = true;
    this.projectSettingsService.setCopilotCliAutoSelectSetting(this.copilotCliAutoSelectEnabled, this.projectPath).subscribe({
      next: () => {
        this.saving = false;
      },
      error: error => {
        console.error('Error saving Copilot CLI Auto-Select setting:', error);
        this.saving = false;
        this.copilotCliAutoSelectEnabled = !this.copilotCliAutoSelectEnabled;
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
        this.saving = false;
      },
      error: error => {
        console.error('Error saving IDE configuration:', error);
        this.saving = false;
        // Revert the change on error
        this.selectedIde = this.selectedIde === 'vscode' ? 'intellij' : 'vscode';
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
    const ref = this.dialog.open(_dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_0__.CatalogPickerDialogComponent, {
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
  static {
    this.ɵfac = function ProjectSettingsComponent_Factory(t) {
      return new (t || ProjectSettingsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogRef), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MAT_LEGACY_DIALOG_DATA), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_project_settings_service__WEBPACK_IMPORTED_MODULE_1__.ProjectSettingsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_compatibility_mode_service__WEBPACK_IMPORTED_MODULE_2__.CompatibilityModeService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_ide_configuration_service__WEBPACK_IMPORTED_MODULE_3__.IdeConfigurationService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_5__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_external_apps_service__WEBPACK_IMPORTED_MODULE_6__.ExternalAppsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_7__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_10__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({
      type: ProjectSettingsComponent,
      selectors: [["app-project-settings"]],
      decls: 88,
      vars: 64,
      consts: [["mat-dialog-title", ""], [1, "settings-tabs"], [3, "label"], [1, "tab-content"], [1, "settings-section"], ["class", "setting-item", 4, "ngIf"], ["class", "loading-container", 4, "ngIf"], ["class", "saving-indicator", 4, "ngIf"], [1, "settings-section", "info-card"], ["class", "rag-status", 4, "ngIf"], ["class", "settings-section info-card", 4, "ngIf"], ["class", "tab-content", 4, "ngIf"], ["align", "end"], ["mat-button", "", 3, "click"], [1, "setting-item"], [3, "ngModel", "disabled", "ngModelChange", "change"], [1, "setting-label"], [1, "setting-description"], [1, "loading-container"], ["diameter", "30"], [1, "saving-indicator"], ["diameter", "20"], ["value", "vscode", 1, "ide-radio"], [1, "ide-label"], [1, "ide-icon-inline", "vscode-color"], ["value", "intellij", 1, "ide-radio"], [1, "ide-icon-inline", "intellij-color"], [1, "rag-status"], [1, "status-row"], [1, "status-label"], ["class", "status-row", 4, "ngIf"], [1, "rag-actions"], ["mat-stroked-button", "", 3, "disabled", "click"], [4, "ngIf"], ["diameter", "18", "class", "inline-spinner", 4, "ngIf"], ["mat-stroked-button", "", "color", "warn", 3, "disabled", "click"], ["class", "setting-description", 4, "ngIf"], ["class", "rag-progress", 4, "ngIf"], ["class", "rag-message", 4, "ngIf"], ["diameter", "18", 1, "inline-spinner"], [1, "rag-progress"], ["mode", "determinate", 3, "value"], ["class", "rag-progress-text", 4, "ngIf"], [1, "rag-progress-text"], [1, "rag-message"], [1, "apps-toolbar"], ["mat-stroked-button", "", "color", "primary", 3, "disabled", "click"], [1, "toolbar-spacer"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["diameter", "20", "class", "inline-spinner", 4, "ngIf"], ["class", "saved-message", 4, "ngIf"], ["class", "apps-empty-state", 4, "ngIf"], ["class", "apps-tree-editor", 4, "ngIf"], ["diameter", "20", 1, "inline-spinner"], [1, "saved-message"], [1, "apps-empty-state"], [1, "apps-empty-hint"], [1, "apps-tree-editor"], [4, "ngFor", "ngForOf"], ["class", "tree-category", 4, "ngIf"], ["class", "tree-row app-row", 4, "ngIf"], [1, "tree-category"], [1, "tree-row", "category-row"], [1, "gold-icon"], ["class", "category-name", 4, "ngIf"], ["class", "category-edit-input", "autofocus", "", 3, "ngModel", "ngModelChange", "keydown.enter", "keydown.escape", "blur", 4, "ngIf"], [1, "tree-actions"], ["mat-icon-button", "", 3, "matTooltip", "click", 4, "ngIf"], ["mat-icon-button", "", "color", "warn", 3, "matTooltip", "click"], ["class", "tree-row app-row indented", 4, "ngFor", "ngForOf"], [1, "category-name"], ["autofocus", "", 1, "category-edit-input", 3, "ngModel", "ngModelChange", "keydown.enter", "keydown.escape", "blur"], ["mat-icon-button", "", 3, "matTooltip", "click"], [1, "tree-row", "app-row", "indented"], [1, "app-row-icon"], [1, "app-row-name"], [1, "tree-row", "app-row"], ["mat-icon-button", "", 3, "matMenuTriggerFor", "matTooltip", 4, "ngIf"], ["moveToCatMenu", "matMenu"], ["mat-menu-item", "", 3, "click", 4, "ngFor", "ngForOf"], ["mat-icon-button", "", 3, "matMenuTriggerFor", "matTooltip"], ["mat-menu-item", "", 3, "click"]],
      template: function ProjectSettingsComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "h2", 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "mat-dialog-content")(4, "mat-tab-group", 1)(5, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "div", 3)(8, "mat-card", 4)(9, "mat-card-header")(10, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](12, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](14, ProjectSettingsComponent_div_14_Template, 13, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](15, ProjectSettingsComponent_div_15_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](16, ProjectSettingsComponent_div_16_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "mat-card", 4)(18, "mat-card-header")(19, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](22, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](23, ProjectSettingsComponent_div_23_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](24, "mat-card", 4)(25, "mat-card-header")(26, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](27);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](28, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](29, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](30, ProjectSettingsComponent_div_30_Template, 13, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](31, "mat-card", 4)(32, "mat-card-header")(33, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](34);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](35, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](36, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](37, ProjectSettingsComponent_div_37_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](38, ProjectSettingsComponent_div_38_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](39, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](40, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](41, "div", 3)(42, "mat-card", 4)(43, "mat-card-header")(44, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](45);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](46, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](47, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](48, ProjectSettingsComponent_div_48_Template, 17, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](49, ProjectSettingsComponent_div_49_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](50, ProjectSettingsComponent_div_50_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](51, "mat-card", 8)(52, "mat-card-content")(53, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](54, "info");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](55, "p");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](56);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](57, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](58, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](59, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](60, "div", 3)(61, "mat-card", 4)(62, "mat-card-header")(63, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](64);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](65, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](66, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](67, ProjectSettingsComponent_div_67_Template, 8, 8, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](68, "mat-card", 4)(69, "mat-card-header")(70, "mat-card-title");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](71);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](72, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](73, "mat-card-content");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](74, ProjectSettingsComponent_div_74_Template, 12, 11, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](75, ProjectSettingsComponent_div_75_Template, 11, 14, "div", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](76, ProjectSettingsComponent_div_76_Template, 16, 15, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](77, ProjectSettingsComponent_div_77_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](78, ProjectSettingsComponent_div_78_Template, 5, 3, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](79, ProjectSettingsComponent_mat_card_79_Template, 7, 3, "mat-card", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](80, "mat-tab", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](81, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](82, ProjectSettingsComponent_div_82_Template, 22, 16, "div", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](83, ProjectSettingsComponent_div_83_Template, 5, 3, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](84, "mat-dialog-actions", 12)(85, "button", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function ProjectSettingsComponent_Template_button_click_85_listener() {
            return ctx.close();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](86);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](87, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind2"](2, 33, "PROJECT_SETTINGS.TITLE", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpureFunction1"](62, _c1, ctx.projectName)));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 36, "PROJECT_SETTINGS.MARKDOWN_SETTINGS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](12, 38, "PROJECT_SETTINGS.VALIDATION_RULES"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](21, 40, "PROJECT_SETTINGS.GITHUB_COMPAT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](28, 42, "PROJECT_SETTINGS.LINK_INDEXING"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](35, 44, "PROJECT_SETTINGS.VIEW_OPTIONS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](40, 46, "PROJECT_SETTINGS.IDE_SETTINGS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](46, 48, "PROJECT_SETTINGS.IDE_CONFIGURATION"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](57, 50, "PROJECT_SETTINGS.IDE_INFO"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](59, 52, "PROJECT_SETTINGS.AI_RAG"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](65, 54, "PROJECT_SETTINGS.COPILOT_CLI_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](72, 56, "PROJECT_SETTINGS.RAG_TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.loading && ctx.ragEnabled);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.loading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.saving);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.ragModelInstalled);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("label", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](81, 58, "PROJECT_SETTINGS.APPS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.appsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.appsLoading);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](87, 60, "COMMON.CLOSE"));
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_11__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_11__.NgIf, _angular_material_legacy_checkbox__WEBPACK_IMPORTED_MODULE_12__.MatLegacyCheckbox, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_13__.MatLegacyRadioGroup, _angular_material_legacy_radio__WEBPACK_IMPORTED_MODULE_13__.MatLegacyRadioButton, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_14__.MatLegacyMenu, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_14__.MatLegacyMenuItem, _angular_material_legacy_menu__WEBPACK_IMPORTED_MODULE_14__.MatLegacyMenuTrigger, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_15__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_15__.MatLegacyCardHeader, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_15__.MatLegacyCardContent, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_15__.MatLegacyCardTitle, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_16__.MatLegacyTabGroup, _angular_material_legacy_tabs__WEBPACK_IMPORTED_MODULE_16__.MatLegacyTab, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_17__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_18__.MatIcon, _angular_material_legacy_progress_spinner__WEBPACK_IMPORTED_MODULE_19__.MatLegacyProgressSpinner, _angular_material_legacy_progress_bar__WEBPACK_IMPORTED_MODULE_20__.MatLegacyProgressBar, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogTitle, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogContent, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_9__.MatLegacyDialogActions, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_21__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_22__.NgModel, _angular_common__WEBPACK_IMPORTED_MODULE_11__.DecimalPipe, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_10__.TranslatePipe],
      styles: [".settings-tabs[_ngcontent-%COMP%] {\n  min-width: 500px;\n  max-width: 600px;\n}\n\n.tab-content[_ngcontent-%COMP%] {\n  padding: 20px 16px;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  margin-bottom: 16px;\n}\n\n.setting-item[_ngcontent-%COMP%] {\n  margin: 16px 0;\n}\n\n.setting-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  margin-left: 8px;\n}\n\n.setting-description[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-text-secondary);\n  font-size: 14px;\n}\n\n.loading-container[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  padding: 20px;\n  gap: 12px;\n}\n\n.saving-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-text-secondary);\n  font-size: 14px;\n}\n\n.info-card[_ngcontent-%COMP%] {\n  background-color: var(--mde-bg-tertiary);\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: start;\n  gap: 12px;\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: var(--mde-accent-primary);\n}\n.info-card[_ngcontent-%COMP%]   mat-card-content[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 14px;\n  color: var(--mde-text-secondary);\n}\n\n.ide-radio[_ngcontent-%COMP%] {\n  display: block;\n  margin: 12px 0;\n}\n\n.ide-label[_ngcontent-%COMP%] {\n  font-weight: 500;\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n.ide-icon-inline[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n}\n.ide-icon-inline.vscode-color[_ngcontent-%COMP%] {\n  color: #007ACC;\n}\n.ide-icon-inline.intellij-color[_ngcontent-%COMP%] {\n  color: #FF6B00;\n}\n\n.rag-status[_ngcontent-%COMP%] {\n  margin: 16px 0 16px 32px;\n  font-size: 14px;\n}\n\n.status-row[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  margin: 4px 0;\n}\n\n.status-label[_ngcontent-%COMP%] {\n  color: var(--mde-text-secondary);\n}\n\n.status-ok[_ngcontent-%COMP%] {\n  color: #4caf50;\n  font-weight: 500;\n}\n\n.status-missing[_ngcontent-%COMP%] {\n  color: #f44336;\n  font-weight: 500;\n}\n\n.rag-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  align-items: center;\n}\n\n.rag-message[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  margin-left: 32px;\n  color: var(--mde-accent-primary);\n  font-size: 14px;\n  font-weight: 500;\n}\n\n.inline-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  margin-right: 4px;\n}\n\n.rag-progress[_ngcontent-%COMP%] {\n  margin: 12px 0 0 32px;\n}\n.rag-progress[_ngcontent-%COMP%]   mat-progress-bar[_ngcontent-%COMP%] {\n  margin-bottom: 6px;\n}\n\n.rag-progress-text[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: var(--mde-text-secondary);\n}\n\n.apps-toolbar[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 12px;\n  align-items: center;\n  margin-bottom: 16px;\n  flex-wrap: wrap;\n}\n\n.toolbar-spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.saved-message[_ngcontent-%COMP%] {\n  font-size: 13px;\n  color: #4caf50;\n  font-weight: 500;\n}\n\n.apps-empty-state[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  padding: 32px 16px;\n  color: var(--mde-text-hint);\n}\n.apps-empty-state[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  margin-bottom: 8px;\n}\n.apps-empty-state[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n}\n\n.apps-empty-hint[_ngcontent-%COMP%] {\n  font-size: 13px;\n  margin-top: 4px !important;\n}\n\n.apps-tree-editor[_ngcontent-%COMP%] {\n  border: 1px solid var(--mde-border-color);\n  border-radius: 4px;\n  overflow: hidden;\n}\n\n.tree-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 6px 12px;\n  border-bottom: 1px solid var(--mde-border-color);\n  min-height: 40px;\n}\n.tree-row[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n\n.category-row[_ngcontent-%COMP%] {\n  background: var(--mde-bg-tertiary);\n  font-weight: 500;\n}\n\n.category-name[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n}\n\n.category-edit-input[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n  border: 1px solid var(--mde-accent-primary);\n  border-radius: 3px;\n  padding: 4px 8px;\n  font-size: 14px;\n  outline: none;\n  background: var(--mde-bg-primary);\n  color: var(--mde-text-primary);\n}\n\n.app-row.indented[_ngcontent-%COMP%] {\n  padding-left: 40px;\n}\n\n.app-row-icon[_ngcontent-%COMP%] {\n  color: var(--mde-accent-primary);\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  flex-shrink: 0;\n}\n\n.app-row-name[_ngcontent-%COMP%] {\n  flex: 1;\n  margin-left: 8px;\n  font-size: 14px;\n}\n\n.tree-actions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  margin-left: auto;\n}\n.tree-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  width: 32px;\n  height: 32px;\n  line-height: 32px;\n}\n.tree-actions[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvcHJvamVjdC1zZXR0aW5ncy9wcm9qZWN0LXNldHRpbmdzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUNBO0VBQ0UsZ0JBQUE7RUFDQSxnQkFBQTtBQUFGOztBQUdBO0VBQ0Usa0JBQUE7QUFBRjs7QUFHQTtFQUNFLG1CQUFBO0FBQUY7O0FBR0E7RUFDRSxjQUFBO0FBQUY7O0FBR0E7RUFDRSxnQkFBQTtFQUNBLGdCQUFBO0FBQUY7O0FBR0E7RUFDRSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQ0FBQTtFQUNBLGVBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFBRjs7QUFHQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQ0FBQTtFQUNBLGVBQUE7QUFBRjs7QUFHQTtFQUNFLHdDQUFBO0FBQUY7QUFFRTtFQUNFLGFBQUE7RUFDQSxrQkFBQTtFQUNBLFNBQUE7QUFBSjtBQUVJO0VBQ0UsZ0NBQUE7QUFBTjtBQUdJO0VBQ0UsU0FBQTtFQUNBLGVBQUE7RUFDQSxnQ0FBQTtBQUROOztBQU1BO0VBQ0UsY0FBQTtFQUNBLGNBQUE7QUFIRjs7QUFNQTtFQUNFLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQUhGOztBQU1BO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBSEY7QUFLRTtFQUNFLGNBQUE7QUFISjtBQU1FO0VBQ0UsY0FBQTtBQUpKOztBQVdBO0VBQ0Usd0JBQUE7RUFDQSxlQUFBO0FBUkY7O0FBV0E7RUFDRSxhQUFBO0VBQ0EsUUFBQTtFQUNBLGFBQUE7QUFSRjs7QUFXQTtFQUNFLGdDQUFBO0FBUkY7O0FBV0E7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7QUFSRjs7QUFXQTtFQUNFLGNBQUE7RUFDQSxnQkFBQTtBQVJGOztBQVdBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxtQkFBQTtBQVJGOztBQVdBO0VBQ0UsZUFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFSRjs7QUFXQTtFQUNFLHFCQUFBO0VBQ0EsaUJBQUE7QUFSRjs7QUFXQTtFQUNFLHFCQUFBO0FBUkY7QUFVRTtFQUNFLGtCQUFBO0FBUko7O0FBWUE7RUFDRSxlQUFBO0VBQ0EsZ0NBQUE7QUFURjs7QUFjQTtFQUNFLGFBQUE7RUFDQSxTQUFBO0VBQ0EsbUJBQUE7RUFDQSxtQkFBQTtFQUNBLGVBQUE7QUFYRjs7QUFjQTtFQUNFLE9BQUE7QUFYRjs7QUFjQTtFQUNFLGVBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFYRjs7QUFjQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSwyQkFBQTtBQVhGO0FBYUU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxrQkFBQTtBQVhKO0FBY0U7RUFDRSxTQUFBO0FBWko7O0FBZ0JBO0VBQ0UsZUFBQTtFQUNBLDBCQUFBO0FBYkY7O0FBZ0JBO0VBQ0UseUNBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsaUJBQUE7RUFDQSxnREFBQTtFQUNBLGdCQUFBO0FBYkY7QUFlRTtFQUNFLG1CQUFBO0FBYko7O0FBaUJBO0VBQ0Usa0NBQUE7RUFDQSxnQkFBQTtBQWRGOztBQWlCQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtBQWRGOztBQWlCQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLDJDQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0VBQ0EsaUNBQUE7RUFDQSw4QkFBQTtBQWRGOztBQWtCRTtFQUNFLGtCQUFBO0FBZko7O0FBbUJBO0VBQ0UsZ0NBQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBaEJGOztBQW1CQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7QUFoQkY7O0FBbUJBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsaUJBQUE7QUFoQkY7QUFrQkU7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0FBaEJKO0FBa0JJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0FBaEJOIiwic291cmNlc0NvbnRlbnQiOlsiLy8gVGFiIHN0eWxlc1xyXG4uc2V0dGluZ3MtdGFicyB7XHJcbiAgbWluLXdpZHRoOiA1MDBweDtcclxuICBtYXgtd2lkdGg6IDYwMHB4O1xyXG59XHJcblxyXG4udGFiLWNvbnRlbnQge1xyXG4gIHBhZGRpbmc6IDIwcHggMTZweDtcclxufVxyXG5cclxuLnNldHRpbmdzLXNlY3Rpb24ge1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbn1cclxuXHJcbi5zZXR0aW5nLWl0ZW0ge1xyXG4gIG1hcmdpbjogMTZweCAwO1xyXG59XHJcblxyXG4uc2V0dGluZy1sYWJlbCB7XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG59XHJcblxyXG4uc2V0dGluZy1kZXNjcmlwdGlvbiB7XHJcbiAgbWFyZ2luLXRvcDogOHB4O1xyXG4gIG1hcmdpbi1sZWZ0OiAzMnB4O1xyXG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxufVxyXG5cclxuLmxvYWRpbmctY29udGFpbmVyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbiAgcGFkZGluZzogMjBweDtcclxuICBnYXA6IDEycHg7XHJcbn1cclxuXHJcbi5zYXZpbmctaW5kaWNhdG9yIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgZ2FwOiA4cHg7XHJcbiAgbWFyZ2luLXRvcDogOHB4O1xyXG4gIG1hcmdpbi1sZWZ0OiAzMnB4O1xyXG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxufVxyXG5cclxuLmluZm8tY2FyZCB7XHJcbiAgYmFja2dyb3VuZC1jb2xvcjogdmFyKC0tbWRlLWJnLXRlcnRpYXJ5KTtcclxuXHJcbiAgbWF0LWNhcmQtY29udGVudCB7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IHN0YXJ0O1xyXG4gICAgZ2FwOiAxMnB4O1xyXG5cclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgY29sb3I6IHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XHJcbiAgICB9XHJcblxyXG4gICAgcCB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAxNHB4O1xyXG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcclxuICAgIH1cclxuICB9XHJcbn1cclxuXHJcbi5pZGUtcmFkaW8ge1xyXG4gIGRpc3BsYXk6IGJsb2NrO1xyXG4gIG1hcmdpbjogMTJweCAwO1xyXG59XHJcblxyXG4uaWRlLWxhYmVsIHtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDhweDtcclxufVxyXG5cclxuLmlkZS1pY29uLWlubGluZSB7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIHdpZHRoOiAyMHB4O1xyXG4gIGhlaWdodDogMjBweDtcclxuXHJcbiAgJi52c2NvZGUtY29sb3Ige1xyXG4gICAgY29sb3I6ICMwMDdBQ0M7IC8vIFZTIENvZGUgYmx1ZVxyXG4gIH1cclxuXHJcbiAgJi5pbnRlbGxpai1jb2xvciB7XHJcbiAgICBjb2xvcjogI0ZGNkIwMDsgLy8gSW50ZWxsaUogb3JhbmdlXHJcbiAgfVxyXG59XHJcblxyXG4vLyBJREUgcGF0aHMgc3R5bGluZyByZW1vdmVkIC0gbm93IHNob3duIGluIFByb2plY3RzIGNvbXBvbmVudCBtYWluIHBhZ2VcclxuXHJcbi8vIFJBRyBzdGF0dXMgc3R5bGVzXHJcbi5yYWctc3RhdHVzIHtcclxuICBtYXJnaW46IDE2cHggMCAxNnB4IDMycHg7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG59XHJcblxyXG4uc3RhdHVzLXJvdyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBnYXA6IDhweDtcclxuICBtYXJnaW46IDRweCAwO1xyXG59XHJcblxyXG4uc3RhdHVzLWxhYmVsIHtcclxuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcclxufVxyXG5cclxuLnN0YXR1cy1vayB7XHJcbiAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxufVxyXG5cclxuLnN0YXR1cy1taXNzaW5nIHtcclxuICBjb2xvcjogI2Y0NDMzNjtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG59XHJcblxyXG4ucmFnLWFjdGlvbnMge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbn1cclxuXHJcbi5yYWctbWVzc2FnZSB7XHJcbiAgbWFyZ2luLXRvcDogOHB4O1xyXG4gIG1hcmdpbi1sZWZ0OiAzMnB4O1xyXG4gIGNvbG9yOiB2YXIoLS1tZGUtYWNjZW50LXByaW1hcnkpO1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG59XHJcblxyXG4uaW5saW5lLXNwaW5uZXIge1xyXG4gIGRpc3BsYXk6IGlubGluZS1ibG9jaztcclxuICBtYXJnaW4tcmlnaHQ6IDRweDtcclxufVxyXG5cclxuLnJhZy1wcm9ncmVzcyB7XHJcbiAgbWFyZ2luOiAxMnB4IDAgMCAzMnB4O1xyXG5cclxuICBtYXQtcHJvZ3Jlc3MtYmFyIHtcclxuICAgIG1hcmdpbi1ib3R0b206IDZweDtcclxuICB9XHJcbn1cclxuXHJcbi5yYWctcHJvZ3Jlc3MtdGV4dCB7XHJcbiAgZm9udC1zaXplOiAxM3B4O1xyXG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1zZWNvbmRhcnkpO1xyXG59XHJcblxyXG4vLyDDosKUwoDDosKUwoAgQXBwcyB0YWIgw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAw6LClMKAXHJcblxyXG4uYXBwcy10b29sYmFyIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGdhcDogMTJweDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIG1hcmdpbi1ib3R0b206IDE2cHg7XHJcbiAgZmxleC13cmFwOiB3cmFwO1xyXG59XHJcblxyXG4udG9vbGJhci1zcGFjZXIge1xyXG4gIGZsZXg6IDE7XHJcbn1cclxuXHJcbi5zYXZlZC1tZXNzYWdlIHtcclxuICBmb250LXNpemU6IDEzcHg7XHJcbiAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxufVxyXG5cclxuLmFwcHMtZW1wdHktc3RhdGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIHBhZGRpbmc6IDMycHggMTZweDtcclxuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtaGludCk7XHJcblxyXG4gIG1hdC1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogNDhweDtcclxuICAgIHdpZHRoOiA0OHB4O1xyXG4gICAgaGVpZ2h0OiA0OHB4O1xyXG4gICAgbWFyZ2luLWJvdHRvbTogOHB4O1xyXG4gIH1cclxuXHJcbiAgcCB7XHJcbiAgICBtYXJnaW46IDA7XHJcbiAgfVxyXG59XHJcblxyXG4uYXBwcy1lbXB0eS1oaW50IHtcclxuICBmb250LXNpemU6IDEzcHg7XHJcbiAgbWFyZ2luLXRvcDogNHB4ICFpbXBvcnRhbnQ7XHJcbn1cclxuXHJcbi5hcHBzLXRyZWUtZWRpdG9yIHtcclxuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcclxuICBib3JkZXItcmFkaXVzOiA0cHg7XHJcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcclxufVxyXG5cclxuLnRyZWUtcm93IHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNnB4IDEycHg7XHJcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xyXG4gIG1pbi1oZWlnaHQ6IDQwcHg7XHJcblxyXG4gICY6bGFzdC1jaGlsZCB7XHJcbiAgICBib3JkZXItYm90dG9tOiBub25lO1xyXG4gIH1cclxufVxyXG5cclxuLmNhdGVnb3J5LXJvdyB7XHJcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXRlcnRpYXJ5KTtcclxuICBmb250LXdlaWdodDogNTAwO1xyXG59XHJcblxyXG4uY2F0ZWdvcnktbmFtZSB7XHJcbiAgZmxleDogMTtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG59XHJcblxyXG4uY2F0ZWdvcnktZWRpdC1pbnB1dCB7XHJcbiAgZmxleDogMTtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG4gIGJvcmRlcjogMXB4IHNvbGlkIHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XHJcbiAgYm9yZGVyLXJhZGl1czogM3B4O1xyXG4gIHBhZGRpbmc6IDRweCA4cHg7XHJcbiAgZm9udC1zaXplOiAxNHB4O1xyXG4gIG91dGxpbmU6IG5vbmU7XHJcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXByaW1hcnkpO1xyXG4gIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcclxufVxyXG5cclxuLmFwcC1yb3cge1xyXG4gICYuaW5kZW50ZWQge1xyXG4gICAgcGFkZGluZy1sZWZ0OiA0MHB4O1xyXG4gIH1cclxufVxyXG5cclxuLmFwcC1yb3ctaWNvbiB7XHJcbiAgY29sb3I6IHZhcigtLW1kZS1hY2NlbnQtcHJpbWFyeSk7XHJcbiAgZm9udC1zaXplOiAyMHB4O1xyXG4gIHdpZHRoOiAyMHB4O1xyXG4gIGhlaWdodDogMjBweDtcclxuICBmbGV4LXNocmluazogMDtcclxufVxyXG5cclxuLmFwcC1yb3ctbmFtZSB7XHJcbiAgZmxleDogMTtcclxuICBtYXJnaW4tbGVmdDogOHB4O1xyXG4gIGZvbnQtc2l6ZTogMTRweDtcclxufVxyXG5cclxuLnRyZWUtYWN0aW9ucyB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIG1hcmdpbi1sZWZ0OiBhdXRvO1xyXG5cclxuICBidXR0b24ge1xyXG4gICAgd2lkdGg6IDMycHg7XHJcbiAgICBoZWlnaHQ6IDMycHg7XHJcbiAgICBsaW5lLWhlaWdodDogMzJweDtcclxuXHJcbiAgICBtYXQtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMThweDtcclxuICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgIGhlaWdodDogMThweDtcclxuICAgIH1cclxuICB9XHJcbn0iXSwic291cmNlUm9vdCI6IiJ9 */"]
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
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! rxjs/operators */ 635);
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/project-edit/project-edit-dialog.component */ 9116);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/p2p-manager/p2p-manager.component */ 1902);
/* harmony import */ var _components_unified_settings_dialog_unified_settings_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../components/unified-settings-dialog/unified-settings-dialog.component */ 8833);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _environments_version__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../environments/version */ 9279);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../shared/NgDialogAnimationService */ 5811);
/* harmony import */ var _git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../git/services/gitservice.service */ 7224);
/* harmony import */ var _angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! @angular/cdk/clipboard */ 6079);
/* harmony import */ var _angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! @angular/material/legacy-snack-bar */ 7402);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _services_p2p_service__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../services/p2p.service */ 9811);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! @angular/material/legacy-form-field */ 1204);
/* harmony import */ var _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! @angular/material/legacy-input */ 2044);
/* harmony import */ var _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! @angular/material/legacy-card */ 7315);
/* harmony import */ var _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! @angular/material/legacy-button */ 9159);
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! @angular/material/icon */ 7822);
/* harmony import */ var _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_28__ = __webpack_require__(/*! @angular/material/legacy-tooltip */ 3370);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_29__ = __webpack_require__(/*! @angular/forms */ 2508);









 // Importa la versione




















function ProjectsComponent_div_4_button_12_Template(rf, ctx) {
  if (rf & 1) {
    const _r7 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "button", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_button_12_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r7);
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      ctx_r6.searchQuery = "";
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r6.onSearchChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](2, "clear");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
  }
}
function ProjectsComponent_div_4_mat_card_14_span_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "span", 53)(1, "mat-icon", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](2, "schedule");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](4, 1, "PROJECTS.LAST_OPENED"), " ");
  }
}
function ProjectsComponent_div_4_mat_card_14_p_15_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "p", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const project_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]().$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](project_r8.description);
  }
}
function ProjectsComponent_div_4_mat_card_14_span_16_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "span", 56)(1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](2, "edit_note");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](4, 1, "PROJECTS.ADD_DESCRIPTION"), " ");
  }
}
function ProjectsComponent_div_4_mat_card_14_button_26_Template(rf, ctx) {
  if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "button", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_button_26_Template_button_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r16);
      const project_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]().$implicit;
      const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r14.shareProject(project_r8, $event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](3, "share");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](1, 1, "PROJECTS.SHARE_URL"));
  }
}
function ProjectsComponent_div_4_mat_card_14_Template(rf, ctx) {
  if (rf & 1) {
    const _r18 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "mat-card", 35);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](2, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](3, "mat-card-content", 36)(4, "div", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_Template_div_click_4_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r18);
      const project_r8 = restoredCtx.$implicit;
      const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r17.openProject(project_r8.path));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](5, "div", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelement"](6, "img", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](7, "div", 40)(8, "div", 41)(9, "h3", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](11, ProjectsComponent_div_4_mat_card_14_span_11_Template, 5, 3, "span", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](12, "p", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](14, "div", 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_Template_div_click_14_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r18);
      const project_r8 = restoredCtx.$implicit;
      const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r19.openProject(project_r8.path));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](15, ProjectsComponent_div_4_mat_card_14_p_15_Template, 2, 1, "p", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](16, ProjectsComponent_div_4_mat_card_14_span_16_Template, 5, 3, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](17, "div", 48)(18, "button", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_Template_button_click_18_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r18);
      const project_r8 = restoredCtx.$implicit;
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r20.openProjectEdit(project_r8));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](19, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](20, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](21, "edit");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](22, "button", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_Template_button_click_22_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r18);
      const project_r8 = restoredCtx.$implicit;
      const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r21.openProjectSettings(project_r8));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](23, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](24, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](25, "settings");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](26, ProjectsComponent_div_4_mat_card_14_button_26_Template, 4, 3, "button", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](27, "button", 52);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_div_4_mat_card_14_Template_button_click_27_listener() {
      const restoredCtx = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r18);
      const project_r8 = restoredCtx.$implicit;
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r22.deleteProject(project_r8));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](28, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](29, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](30, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()()();
  }
  if (rf & 2) {
    const project_r8 = ctx.$implicit;
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵclassProp"]("last-opened", ctx_r5.isLastOpened(project_r8));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", project_r8.lastUpdate ? _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](1, 14, "PROJECTS.LAST_ACCESSED") + " " + _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind2"](2, 16, project_r8.lastUpdate, "short") : null);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](project_r8.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ctx_r5.isLastOpened(project_r8));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](project_r8.path);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵclassProp"]("empty", !project_r8.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", project_r8.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", !project_r8.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](19, 19, "PROJECTS.EDIT_PROJECT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](23, 21, "PROJECTS.PROJECT_SETTINGS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ctx_r5.projectHasRemote(project_r8));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](28, 23, "PROJECTS.DELETE_PROJECT"));
  }
}
function ProjectsComponent_div_4_Template(rf, ctx) {
  if (rf & 1) {
    const _r24 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "div", 26)(1, "div", 27)(2, "h2", 7)(3, "mat-icon", 8);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](4, "history");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](7, "mat-form-field", 28)(8, "mat-icon", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](9, "search");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](10, "input", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("ngModelChange", function ProjectsComponent_div_4_Template_input_ngModelChange_10_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r24);
      const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r23.searchQuery = $event);
    })("input", function ProjectsComponent_div_4_Template_input_input_10_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r24);
      const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r25.onSearchChange());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](12, ProjectsComponent_div_4_button_12_Template, 3, 0, "button", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](13, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](14, ProjectsComponent_div_4_mat_card_14_Template, 31, 25, "mat-card", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](15, "async");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](6, 5, "PROJECTS.RECENT_PROJECTS"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](11, 7, "PROJECTS.SEARCH_PROJECTS"))("ngModel", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ctx_r0.searchQuery);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](15, 9, ctx_r0.recentProjects));
  }
}
function ProjectsComponent_div_6_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "div", 58)(1, "mat-icon", 59);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](2, "folder_open");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](3, "h3");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](6, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](5, 2, "PROJECTS.NO_RECENT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](8, 4, "PROJECTS.NO_RECENT_HINT"));
  }
}
function ProjectsComponent_mat_card_60_Template(rf, ctx) {
  if (rf & 1) {
    const _r27 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "mat-card", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_mat_card_60_Template_mat_card_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r27);
      const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r26.openP2PManager());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](1, "mat-card-content", 11)(2, "div", 61)(3, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](4, "share");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](5, "div", 13)(6, "h2", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](8, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](9, "p", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](11, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](12, "div", 16)(13, "mat-icon", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](14, "arrow_forward");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](8, 2, "PROJECTS.P2P_SHARING"));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](11, 4, "PROJECTS.P2P_SHARING_DESC"));
  }
}
function ProjectsComponent_button_68_Template(rf, ctx) {
  if (rf & 1) {
    const _r29 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "button", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_button_68_Template_button_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵrestoreView"](_r29);
      const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵresetView"](ctx_r28.openElectronLog());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](1, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](3, "monitor");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](1, 2, "PROJECTS.ELECTRON_LOG_TOOLTIP"));
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](5, 4, "PROJECTS.ELECTRON_LOG"), " ");
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
    this.appVersion = _environments_version__WEBPACK_IMPORTED_MODULE_8__.versionInfo.version; // Rendi la versione disponibile nel template
    this.buildTime = _environments_version__WEBPACK_IMPORTED_MODULE_8__.versionInfo.buildTime; // Rendi il timestamp di build disponibile nel template
    this.searchQuery = '';
    this.lastOpenedProjectId = null;
    this.isP2PAvailable = false;
    // Flag to prevent multiple clicks when opening a project
    this.isOpeningProject = false;
    // Cache for remote URL status per project path
    this.remoteUrlCache = new Map();
    this.dataSource1 = [{
      name: 'Nome progetto',
      path: 'c:\folder\folder\folder'
    }];
  }
  ngOnDestroy() {
    console.log('ProjectsComponent destroyed!');
  }
  ngOnInit() {
    // Check P2P availability
    this.p2pService.isAvailable$.subscribe(available => {
      this.isP2PAvailable = available;
    });
    this.p2pService.checkAvailability();
    // Load recent projects and sort by lastUpdate descending (most recent first)
    this.projectService.fetchProjects();
    this.recentProjects = this.projectService.mdProjects.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_15__.map)(projects => {
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
    const dialogRef = this.dialog.open(_project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_4__.ProjectSettingsComponent, {
      width: '600px',
      data: {
        projectId: project.id,
        projectName: project.name,
        projectPath: project.path
      }
    });
  }
  openProjectEdit(project) {
    const dialogRef = this.dialog.open(_dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_3__.ProjectEditDialogComponent, {
      width: '520px',
      data: {
        id: project.id,
        name: project.name,
        description: project.description,
        path: project.path
      }
    });
    dialogRef.afterClosed().subscribe(result => {
      if (!result) {
        return;
      }
      this.projectService.updateProject(result).subscribe({
        next: () => {
          this.projectService.fetchProjects();
          this.snackBar.open(this.translate.instant('PROJECTS.PROJECT_UPDATED'), 'OK', {
            duration: 2500
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
    let data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_7__.ShowFileMetadata();
    data.start = null;
    data.title = this.translate.instant('PROJECTS.SELECT_FOLDER');
    data.typeOfSelection = "Folders";
    data.buttonText = this.translate.instant('PROJECTS.SELECT_FOLDER_BTN');
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
          data: {
            projectPath: result.data
          }
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
    this.dialog.open(_components_unified_settings_dialog_unified_settings_dialog_component__WEBPACK_IMPORTED_MODULE_6__.UnifiedSettingsDialogComponent, {
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
    const dialogRef = this.dialog.open(_dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_5__.P2PManagerComponent, {
      width: '700px',
      maxHeight: '80vh',
      data: {}
    });
  }
  static {
    this.ɵfac = function ProjectsComponent_Factory(t) {
      return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_9__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_16__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_17__.Router), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_10__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_shared_NgDialogAnimationService__WEBPACK_IMPORTED_MODULE_11__.NgDialogAnimationService), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_git_services_gitservice_service__WEBPACK_IMPORTED_MODULE_12__.GITService), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_angular_cdk_clipboard__WEBPACK_IMPORTED_MODULE_18__.Clipboard), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_angular_material_legacy_snack_bar__WEBPACK_IMPORTED_MODULE_19__.MatLegacySnackBar), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_20__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_services_p2p_service__WEBPACK_IMPORTED_MODULE_13__.P2PService), _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_21__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵdefineComponent"]({
      type: ProjectsComponent,
      selectors: [["app-projects"]],
      decls: 69,
      vars: 35,
      consts: [[1, "modern-container"], [1, "content-wrapper"], [1, "main-layout"], [1, "left-column"], ["class", "recent-projects-section", 4, "ngIf"], ["class", "no-projects-message", 4, "ngIf"], [1, "right-column"], [1, "section-title"], [1, "section-icon"], [1, "actions-grid"], ["data-test", "clone-button", 1, "action-card", "clone-card", 3, "click"], [1, "card-content"], [1, "icon-wrapper", "clone-icon"], [1, "card-text"], [1, "card-title"], [1, "card-description"], [1, "card-action"], [1, "arrow-icon"], ["data-test", "new-folder-button", 1, "action-card", "create-card", 3, "click"], [1, "icon-wrapper", "create-icon"], ["data-test", "settings-button", 1, "action-card", "settings-card", 3, "click"], [1, "icon-wrapper", "settings-icon"], ["class", "action-card p2p-card", "data-test", "p2p-button", 3, "click", 4, "ngIf"], [1, "diagnostics-section"], ["mat-stroked-button", "", "color", "primary", 3, "matTooltip", "click"], ["mat-stroked-button", "", "color", "primary", 3, "matTooltip", "click", 4, "ngIf"], [1, "recent-projects-section"], [1, "section-header"], ["appearance", "outline", 1, "search-field"], ["matPrefix", ""], ["matInput", "", 3, "placeholder", "ngModel", "ngModelChange", "input"], ["mat-icon-button", "", "matSuffix", "", 3, "click", 4, "ngIf"], [1, "projects-grid"], ["class", "project-card", "matTooltipPosition", "below", 3, "last-opened", "matTooltip", 4, "ngFor", "ngForOf"], ["mat-icon-button", "", "matSuffix", "", 3, "click"], ["matTooltipPosition", "below", 1, "project-card", 3, "matTooltip"], [1, "project-card-content"], [1, "project-info", 3, "click"], [1, "project-icon-wrapper"], ["src", "assets/icons/files-stack.svg", "alt", "Project", 1, "project-icon-svg"], [1, "project-details"], [1, "project-name-row"], [1, "project-name"], ["class", "last-opened-badge", 4, "ngIf"], [1, "project-path"], [1, "project-description-box", 3, "click"], ["class", "project-description", 4, "ngIf"], ["class", "empty-placeholder", 4, "ngIf"], [1, "project-actions"], ["mat-icon-button", "", 1, "action-btn", "edit-btn", 3, "matTooltip", "click"], ["mat-icon-button", "", 1, "action-btn", "settings-btn", 3, "matTooltip", "click"], ["mat-icon-button", "", "class", "action-btn share-btn", 3, "matTooltip", "click", 4, "ngIf"], ["mat-icon-button", "", 1, "action-btn", "delete-btn", 3, "matTooltip", "click"], [1, "last-opened-badge"], [1, "badge-icon"], [1, "project-description"], [1, "empty-placeholder"], ["mat-icon-button", "", 1, "action-btn", "share-btn", 3, "matTooltip", "click"], [1, "no-projects-message"], [1, "empty-icon"], ["data-test", "p2p-button", 1, "action-card", "p2p-card", 3, "click"], [1, "icon-wrapper", "p2p-icon"]],
      template: function ProjectsComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "div", 2)(3, "div", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](4, ProjectsComponent_div_4_Template, 16, 11, "div", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](5, "async");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](6, ProjectsComponent_div_6_Template, 9, 6, "div", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](7, "async");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](8, "div", 6)(9, "h2", 7)(10, "mat-icon", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](11, "flash_on");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](13, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](14, "div", 9)(15, "mat-card", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_15_listener() {
            return ctx.prepareToClone();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](16, "mat-card-content", 11)(17, "div", 12)(18, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](19, "cloud_download");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](20, "div", 13)(21, "h2", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](22);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](23, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](24, "p", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](25);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](26, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](27, "div", 16)(28, "mat-icon", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](29, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](30, "mat-card", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_30_listener() {
            return ctx.openNewFolder();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](31, "mat-card-content", 11)(32, "div", 19)(33, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](34, "create_new_folder");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](35, "div", 13)(36, "h2", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](37);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](38, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](39, "p", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](40);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](41, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](42, "div", 16)(43, "mat-icon", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](44, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](45, "mat-card", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_Template_mat_card_click_45_listener() {
            return ctx.openSettings();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](46, "mat-card-content", 11)(47, "div", 21)(48, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](49, "settings");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](50, "div", 13)(51, "h2", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](52);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](53, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](54, "p", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](55);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](56, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](57, "div", 16)(58, "mat-icon", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](59, "arrow_forward");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](60, ProjectsComponent_mat_card_60_Template, 15, 6, "mat-card", 22);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](61, "div", 23)(62, "button", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_62_listener() {
            return ctx.openLog();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](63, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementStart"](64, "mat-icon");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](65, "description");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtext"](66);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipe"](67, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtemplate"](68, ProjectsComponent_button_68_Template, 6, 6, "button", 25);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵelementEnd"]()()()()();
        }
        if (rf & 2) {
          let tmp_0_0;
          let tmp_1_0;
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ((tmp_0_0 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](5, 13, ctx.recentProjects)) == null ? null : tmp_0_0.length) > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", !((tmp_1_0 = _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](7, 15, ctx.recentProjects)) == null ? null : tmp_1_0.length));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](13, 17, "PROJECTS.QUICK_ACTIONS"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](10);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](23, 19, "PROJECTS.CLONE_REPO"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](26, 21, "PROJECTS.CLONE_REPO_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](38, 23, "PROJECTS.CREATE_NEW"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](41, 25, "PROJECTS.CREATE_NEW_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](12);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](53, 27, "SETTINGS.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](56, 29, "PROJECTS.SETTINGS_CARD_DESC"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ctx.isP2PAvailable);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("matTooltip", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](63, 31, "PROJECTS.NET_LOG_TOOLTIP"));
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵpipeBind1"](67, 33, "PROJECTS.NET_LOG"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_14__["ɵɵproperty"]("ngIf", ctx.isElectron());
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_22__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_22__.NgIf, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_23__.MatLegacyFormField, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_23__.MatLegacyPrefix, _angular_material_legacy_form_field__WEBPACK_IMPORTED_MODULE_23__.MatLegacySuffix, _angular_material_legacy_input__WEBPACK_IMPORTED_MODULE_24__.MatLegacyInput, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_25__.MatLegacyCard, _angular_material_legacy_card__WEBPACK_IMPORTED_MODULE_25__.MatLegacyCardContent, _angular_material_legacy_button__WEBPACK_IMPORTED_MODULE_26__.MatLegacyButton, _angular_material_icon__WEBPACK_IMPORTED_MODULE_27__.MatIcon, _angular_material_legacy_tooltip__WEBPACK_IMPORTED_MODULE_28__.MatLegacyTooltip, _angular_forms__WEBPACK_IMPORTED_MODULE_29__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_29__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_29__.NgModel, _angular_common__WEBPACK_IMPORTED_MODULE_22__.AsyncPipe, _angular_common__WEBPACK_IMPORTED_MODULE_22__.DatePipe, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_21__.TranslatePipe],
      styles: [".modern-container[_ngcontent-%COMP%] {\n  min-height: 100vh;\n  background: var(--mde-bg-secondary);\n  display: flex;\n  flex-direction: column;\n  padding: 0;\n  overflow-x: hidden;\n  overflow-y: auto;\n}\n\n.content-wrapper[_ngcontent-%COMP%] {\n  flex: 1;\n  background: var(--mde-bg-secondary);\n  padding: 40px;\n  max-width: 1600px;\n  margin: 0 auto;\n  width: 100%;\n  min-height: calc(100vh - 36px);\n  box-sizing: border-box;\n}\n\n.main-layout[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: 1fr 400px;\n  gap: 32px;\n  align-items: start;\n  height: calc(100vh - 116px);\n}\n\n.left-column[_ngcontent-%COMP%] {\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  height: 100%;\n  max-height: 100%;\n}\n\n.right-column[_ngcontent-%COMP%] {\n  position: sticky;\n  top: 40px;\n  align-self: start;\n}\n\n.actions-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.action-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 16px;\n  border: none;\n  box-shadow: 0 2px 8px var(--mde-shadow-color);\n  overflow: hidden;\n  background: var(--mde-bg-primary);\n}\n.action-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);\n}\n.action-card[_ngcontent-%COMP%]:hover   .arrow-icon[_ngcontent-%COMP%] {\n  transform: translateX(4px);\n}\n.action-card[_ngcontent-%COMP%]:hover   .icon-wrapper[_ngcontent-%COMP%] {\n  transform: scale(1.1);\n}\n.action-card[_ngcontent-%COMP%]:active {\n  transform: translateY(-2px);\n}\n.action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 16px;\n  padding: 20px 16px;\n  position: relative;\n}\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 48px;\n  height: 48px;\n  border-radius: 12px;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: transform 0.3s ease;\n}\n.action-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 24px;\n  width: 24px;\n  height: 24px;\n  color: white;\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-title[_ngcontent-%COMP%] {\n  margin: 0 0 4px 0;\n  font-size: 1rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n}\n.action-card[_ngcontent-%COMP%]   .card-text[_ngcontent-%COMP%]   .card-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.75rem;\n  color: var(--mde-text-secondary);\n  line-height: 1.4;\n}\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n}\n.action-card[_ngcontent-%COMP%]   .card-action[_ngcontent-%COMP%]   .arrow-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: #a0aec0;\n  transition: transform 0.3s ease;\n}\n.action-card.clone-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n}\n.action-card.create-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);\n}\n.action-card.settings-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);\n}\n.action-card.p2p-card[_ngcontent-%COMP%]   .icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);\n}\n\n.section-title[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n  margin: 0 0 24px 0;\n}\n.section-title[_ngcontent-%COMP%]   .section-icon[_ngcontent-%COMP%] {\n  font-size: 28px;\n  width: 28px;\n  height: 28px;\n  color: #667eea;\n}\n\n.section-divider[_ngcontent-%COMP%] {\n  height: 1px;\n  background: linear-gradient(to right, transparent, var(--mde-border-color), transparent);\n  margin: 32px 0;\n}\n\n.recent-projects-section[_ngcontent-%COMP%] {\n  margin-bottom: 0;\n  display: flex;\n  flex-direction: column;\n  flex: 1;\n  min-height: 0;\n}\n\n.section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n  margin-bottom: 24px;\n  gap: 24px;\n  flex-shrink: 0;\n}\n.section-header[_ngcontent-%COMP%]   .section-title[_ngcontent-%COMP%] {\n  margin: 0;\n  flex-shrink: 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n  width: 100%;\n  max-width: 400px;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-wrapper[_ngcontent-%COMP%] {\n  padding-bottom: 0;\n  margin-bottom: 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-infix[_ngcontent-%COMP%] {\n  padding: 0.5em 0;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   .mat-form-field-underline[_ngcontent-%COMP%] {\n  display: none;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: #718096;\n}\n.section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n}\n\n.no-projects-message[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 60px 20px;\n  color: #718096;\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n}\n.no-projects-message[_ngcontent-%COMP%]   .empty-icon[_ngcontent-%COMP%] {\n  font-size: 80px;\n  width: 80px;\n  height: 80px;\n  color: #cbd5e0;\n  margin: 0 auto 20px;\n}\n.no-projects-message[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 1.5rem;\n  font-weight: 500;\n  color: #4a5568;\n  margin: 0 0 8px 0;\n}\n.no-projects-message[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  font-size: 1rem;\n  margin: 0;\n}\n\n.projects-grid[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n  flex: 1;\n  overflow-y: auto;\n  padding-right: 8px;\n  min-height: 0;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar {\n  width: 8px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-track {\n  background: var(--mde-scrollbar-track);\n  border-radius: 4px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb {\n  background: var(--mde-scrollbar-thumb);\n  border-radius: 4px;\n}\n.projects-grid[_ngcontent-%COMP%]::-webkit-scrollbar-thumb:hover {\n  background: var(--mde-scrollbar-hover);\n}\n\n.project-card[_ngcontent-%COMP%] {\n  cursor: pointer;\n  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);\n  border-radius: 12px;\n  border: 1px solid var(--mde-border-color);\n  box-shadow: 0 1px 3px var(--mde-shadow-color);\n  background: var(--mde-bg-primary);\n  position: relative;\n  overflow: visible;\n}\n.project-card[_ngcontent-%COMP%]:hover {\n  transform: translateY(-2px);\n  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);\n  border-color: #667eea;\n}\n.project-card.last-opened[_ngcontent-%COMP%] {\n  border: 2px solid #667eea;\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);\n  background: linear-gradient(135deg, rgba(102, 126, 234, 0.03) 0%, rgba(118, 75, 162, 0.03) 100%);\n}\n.project-card.last-opened[_ngcontent-%COMP%]:hover {\n  transform: translateY(-4px);\n  box-shadow: 0 12px 24px rgba(102, 126, 234, 0.3);\n  border-color: #764ba2;\n}\n.project-card.last-opened[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);\n  animation: _ngcontent-%COMP%_pulse 2s ease-in-out infinite;\n}\n.project-card[_ngcontent-%COMP%]   .project-card-content[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: stretch;\n  padding: 12px 16px;\n  gap: 14px;\n}\n.project-card[_ngcontent-%COMP%]   .project-info[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 12px;\n  flex: 0 0 auto;\n  width: clamp(220px, 34%, 340px);\n  min-width: 0;\n  cursor: pointer;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n  min-width: 0;\n  display: flex;\n  align-items: center;\n  padding: 8px 12px;\n  border: 1px solid var(--mde-border-color);\n  border-radius: 8px;\n  background: var(--mde-bg-secondary);\n  cursor: pointer;\n  transition: border-color 0.2s ease, background 0.2s ease;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%]:hover {\n  border-color: rgba(102, 126, 234, 0.5);\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box[_ngcontent-%COMP%]   .project-description[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: var(--mde-text-primary);\n  line-height: 1.4;\n  display: -webkit-box;\n  -webkit-line-clamp: 3;\n  -webkit-box-orient: vertical;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  word-break: break-word;\n  width: 100%;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%] {\n  border-style: dashed;\n  background: transparent;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]:hover {\n  background: rgba(102, 126, 234, 0.04);\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]   .empty-placeholder[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n  color: var(--mde-text-secondary);\n  opacity: 0.6;\n  font-size: 0.78rem;\n  font-style: italic;\n}\n.project-card[_ngcontent-%COMP%]   .project-description-box.empty[_ngcontent-%COMP%]   .empty-placeholder[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 16px;\n  width: 16px;\n  height: 16px;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 40px;\n  height: 40px;\n  border-radius: 10px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon[_ngcontent-%COMP%] {\n  font-size: 20px;\n  width: 20px;\n  height: 20px;\n  color: white;\n}\n.project-card[_ngcontent-%COMP%]   .project-icon-wrapper[_ngcontent-%COMP%]   .project-icon-svg[_ngcontent-%COMP%] {\n  width: 20px;\n  height: 20px;\n  filter: brightness(0) invert(1);\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n  display: flex;\n  flex-direction: column;\n  gap: 2px;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  min-width: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-name[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 1.1rem;\n  font-weight: 500;\n  color: var(--mde-text-primary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n  min-width: 0;\n  flex-shrink: 1;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .project-path[_ngcontent-%COMP%] {\n  margin: 0;\n  font-size: 0.8rem;\n  color: var(--mde-text-secondary);\n  white-space: nowrap;\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);\n  color: white;\n  padding: 3px 8px;\n  border-radius: 10px;\n  font-size: 0.6rem;\n  font-weight: 600;\n  text-transform: uppercase;\n  letter-spacing: 0.4px;\n  white-space: nowrap;\n  box-shadow: 0 2px 6px rgba(102, 126, 234, 0.3);\n  animation: _ngcontent-%COMP%_fadeInScale 0.5s ease-out;\n  flex-shrink: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-details[_ngcontent-%COMP%]   .last-opened-badge[_ngcontent-%COMP%]   .badge-icon[_ngcontent-%COMP%] {\n  font-size: 11px;\n  width: 11px;\n  height: 11px;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: repeat(2, 32px);\n  grid-auto-rows: 32px;\n  gap: 2px;\n  flex-shrink: 0;\n  align-self: center;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%] {\n  transition: all 0.2s ease;\n  width: 32px;\n  height: 32px;\n  line-height: 32px;\n  padding: 0;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]     .mat-mdc-button-touch-target {\n  width: 32px !important;\n  height: 32px !important;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.edit-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(255, 152, 0, 0.12);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.edit-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(76, 175, 80, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.share-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #4caf50;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(102, 126, 234, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.settings-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #667eea;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover {\n  background-color: rgba(245, 87, 108, 0.1);\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn.delete-btn[_ngcontent-%COMP%]:hover   mat-icon[_ngcontent-%COMP%] {\n  color: #f5576c;\n}\n.project-card[_ngcontent-%COMP%]   .project-actions[_ngcontent-%COMP%]   .action-btn[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  line-height: 18px;\n  color: #a0aec0;\n  transition: color 0.2s ease;\n}\n\n.sidebar-section[_ngcontent-%COMP%] {\n  margin-bottom: 24px;\n}\n\n@media (max-width: 1024px) {\n  .main-layout[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n    gap: 32px;\n  }\n  .right-column[_ngcontent-%COMP%] {\n    position: static;\n  }\n  .actions-grid[_ngcontent-%COMP%] {\n    display: grid;\n    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));\n    gap: 16px;\n  }\n}\n@media (max-width: 768px) {\n  .header-section[_ngcontent-%COMP%] {\n    padding: 40px 20px 20px;\n  }\n  .header-section[_ngcontent-%COMP%]   .main-title[_ngcontent-%COMP%] {\n    font-size: 2rem;\n  }\n  .header-section[_ngcontent-%COMP%]   .subtitle[_ngcontent-%COMP%] {\n    font-size: 1rem;\n  }\n  .content-wrapper[_ngcontent-%COMP%] {\n    padding: 32px 20px;\n    border-radius: 24px 24px 0 0;\n  }\n  .actions-grid[_ngcontent-%COMP%] {\n    grid-template-columns: 1fr;\n  }\n  .action-card[_ngcontent-%COMP%]   .card-content[_ngcontent-%COMP%] {\n    padding: 24px 20px;\n  }\n  .section-title[_ngcontent-%COMP%] {\n    font-size: 1.25rem;\n  }\n  .section-header[_ngcontent-%COMP%] {\n    flex-direction: column;\n    align-items: flex-start;\n  }\n  .section-header[_ngcontent-%COMP%]   .search-field[_ngcontent-%COMP%] {\n    max-width: 100%;\n  }\n}\n@keyframes _ngcontent-%COMP%_pulse {\n  0%, 100% {\n    transform: scale(1);\n  }\n  50% {\n    transform: scale(1.05);\n  }\n}\n@keyframes _ngcontent-%COMP%_fadeInScale {\n  0% {\n    opacity: 0;\n    transform: scale(0.8);\n  }\n  100% {\n    opacity: 1;\n    transform: scale(1);\n  }\n}\n.diagnostics-section[_ngcontent-%COMP%] {\n  margin-top: 24px;\n  padding-top: 16px;\n  border-top: 1px solid var(--mde-border-color);\n  display: flex;\n  gap: 12px;\n  flex-wrap: wrap;\n}\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  font-size: 0.85rem;\n}\n.diagnostics-section[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 18px;\n  width: 18px;\n  height: 18px;\n  margin-right: 6px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvamVjdHMvcHJvamVjdHMuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQ0E7RUFDRSxpQkFBQTtFQUNBLG1DQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsVUFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7QUFBRjs7QUFJQTtFQUNFLE9BQUE7RUFDQSxtQ0FBQTtFQUNBLGFBQUE7RUFDQSxpQkFBQTtFQUNBLGNBQUE7RUFDQSxXQUFBO0VBQ0EsOEJBQUE7RUFDQSxzQkFBQTtBQURGOztBQUtBO0VBQ0UsYUFBQTtFQUNBLGdDQUFBO0VBQ0EsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsMkJBQUE7QUFGRjs7QUFLQTtFQUNFLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7QUFGRjs7QUFLQTtFQUNFLGdCQUFBO0VBQ0EsU0FBQTtFQUNBLGlCQUFBO0FBRkY7O0FBTUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0FBSEY7O0FBT0E7RUFDRSxlQUFBO0VBQ0EsaURBQUE7RUFDQSxtQkFBQTtFQUNBLFlBQUE7RUFDQSw2Q0FBQTtFQUNBLGdCQUFBO0VBQ0EsaUNBQUE7QUFKRjtBQU1FO0VBQ0UsMkJBQUE7RUFDQSwyQ0FBQTtBQUpKO0FBTUk7RUFDRSwwQkFBQTtBQUpOO0FBT0k7RUFDRSxxQkFBQTtBQUxOO0FBU0U7RUFDRSwyQkFBQTtBQVBKO0FBVUU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtBQVJKO0FBV0U7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsK0JBQUE7QUFUSjtBQVdJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsWUFBQTtBQVROO0FBYUU7RUFDRSxPQUFBO0FBWEo7QUFhSTtFQUNFLGlCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsOEJBQUE7QUFYTjtBQWNJO0VBQ0UsU0FBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxnQkFBQTtBQVpOO0FBZ0JFO0VBQ0UsY0FBQTtBQWRKO0FBZ0JJO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLCtCQUFBO0FBZE47QUFtQkU7RUFDRSw2REFBQTtBQWpCSjtBQW9CRTtFQUNFLDZEQUFBO0FBbEJKO0FBcUJFO0VBQ0UsNkRBQUE7QUFuQko7QUFzQkU7RUFDRSw2REFBQTtBQXBCSjs7QUF5QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLDhCQUFBO0VBQ0Esa0JBQUE7QUF0QkY7QUF3QkU7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0FBdEJKOztBQTJCQTtFQUNFLFdBQUE7RUFDQSx3RkFBQTtFQUNBLGNBQUE7QUF4QkY7O0FBNEJBO0VBQ0UsZ0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxPQUFBO0VBQ0EsYUFBQTtBQXpCRjs7QUE2QkE7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGNBQUE7QUExQkY7QUE0QkU7RUFDRSxTQUFBO0VBQ0EsY0FBQTtBQTFCSjtBQTZCRTtFQUNFLFdBQUE7RUFDQSxnQkFBQTtBQTNCSjtBQThCSTtFQUNFLGlCQUFBO0VBQ0EsZ0JBQUE7QUE1Qk47QUErQkk7RUFDRSxnQkFBQTtBQTdCTjtBQWdDSTtFQUNFLGFBQUE7QUE5Qk47QUFpQ0k7RUFDRSxlQUFBO0FBL0JOO0FBa0NJO0VBQ0UsY0FBQTtBQWhDTjtBQW1DSTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQWpDTjs7QUF1Q0E7RUFDRSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLE9BQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0FBcENGO0FBc0NFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLG1CQUFBO0FBcENKO0FBdUNFO0VBQ0UsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxpQkFBQTtBQXJDSjtBQXdDRTtFQUNFLGVBQUE7RUFDQSxTQUFBO0FBdENKOztBQTBDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFNBQUE7RUFDQSxPQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7QUF2Q0Y7QUEwQ0U7RUFDRSxVQUFBO0FBeENKO0FBMkNFO0VBQ0Usc0NBQUE7RUFDQSxrQkFBQTtBQXpDSjtBQTRDRTtFQUNFLHNDQUFBO0VBQ0Esa0JBQUE7QUExQ0o7QUE0Q0k7RUFDRSxzQ0FBQTtBQTFDTjs7QUErQ0E7RUFDRSxlQUFBO0VBQ0EsaURBQUE7RUFDQSxtQkFBQTtFQUNBLHlDQUFBO0VBQ0EsNkNBQUE7RUFDQSxpQ0FBQTtFQUNBLGtCQUFBO0VBQ0EsaUJBQUE7QUE1Q0Y7QUE4Q0U7RUFDRSwyQkFBQTtFQUNBLHlDQUFBO0VBQ0EscUJBQUE7QUE1Q0o7QUFnREU7RUFDRSx5QkFBQTtFQUNBLCtDQUFBO0VBQ0EsZ0dBQUE7QUE5Q0o7QUFnREk7RUFDRSwyQkFBQTtFQUNBLGdEQUFBO0VBQ0EscUJBQUE7QUE5Q047QUFpREk7RUFDRSw2REFBQTtFQUNBLCtDQUFBO0VBQ0Esd0NBQUE7QUEvQ047QUFtREU7RUFDRSxhQUFBO0VBQ0Esb0JBQUE7RUFDQSxrQkFBQTtFQUNBLFNBQUE7QUFqREo7QUFvREU7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0EsY0FBQTtFQUNBLCtCQUFBO0VBQ0EsWUFBQTtFQUNBLGVBQUE7QUFsREo7QUFxREU7RUFDRSxjQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGlCQUFBO0VBQ0EseUNBQUE7RUFDQSxrQkFBQTtFQUNBLG1DQUFBO0VBQ0EsZUFBQTtFQUNBLHdEQUFBO0FBbkRKO0FBcURJO0VBQ0Usc0NBQUE7QUFuRE47QUFzREk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSw4QkFBQTtFQUNBLGdCQUFBO0VBQ0Esb0JBQUE7RUFDQSxxQkFBQTtFQUNBLDRCQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtFQUNBLHNCQUFBO0VBQ0EsV0FBQTtBQXBETjtBQXVESTtFQUNFLG9CQUFBO0VBQ0EsdUJBQUE7QUFyRE47QUF1RE07RUFDRSxxQ0FBQTtBQXJEUjtBQXdETTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsZ0NBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtBQXREUjtBQXdEUTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQXREVjtBQTRERTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0EsNkRBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtBQTFESjtBQTRESTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLFlBQUE7QUExRE47QUE2REk7RUFDRSxXQUFBO0VBQ0EsWUFBQTtFQUNBLCtCQUFBO0FBM0ROO0FBK0RFO0VBQ0UsT0FBQTtFQUNBLFlBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBN0RKO0FBK0RJO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLFlBQUE7QUE3RE47QUFnRUk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUNBLDhCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQkFBQTtFQUNBLHVCQUFBO0VBQ0EsWUFBQTtFQUNBLGNBQUE7QUE5RE47QUFpRUk7RUFDRSxTQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQ0FBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSx1QkFBQTtBQS9ETjtBQWtFSTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsNkRBQUE7RUFDQSxZQUFBO0VBQ0EsZ0JBQUE7RUFDQSxtQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSx5QkFBQTtFQUNBLHFCQUFBO0VBQ0EsbUJBQUE7RUFDQSw4Q0FBQTtFQUNBLG9DQUFBO0VBQ0EsY0FBQTtBQWhFTjtBQWtFTTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtBQWhFUjtBQXFFRTtFQUNFLGFBQUE7RUFDQSxzQ0FBQTtFQUNBLG9CQUFBO0VBQ0EsUUFBQTtFQUNBLGNBQUE7RUFDQSxrQkFBQTtBQW5FSjtBQXFFSTtFQUNFLHlCQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLFVBQUE7QUFuRU47QUFzRU07RUFDRSxzQkFBQTtFQUNBLHVCQUFBO0FBcEVSO0FBdUVNO0VBQ0UseUNBQUE7QUFyRVI7QUFzRVE7RUFDRSxjQUFBO0FBcEVWO0FBd0VNO0VBQ0Usd0NBQUE7QUF0RVI7QUF1RVE7RUFDRSxjQUFBO0FBckVWO0FBeUVNO0VBQ0UsMENBQUE7QUF2RVI7QUF3RVE7RUFDRSxjQUFBO0FBdEVWO0FBMEVNO0VBQ0UseUNBQUE7QUF4RVI7QUF5RVE7RUFDRSxjQUFBO0FBdkVWO0FBMkVNO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxZQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0VBQ0EsMkJBQUE7QUF6RVI7O0FBZ0ZBO0VBQ0UsbUJBQUE7QUE3RUY7O0FBaUZBO0VBQ0U7SUFDRSwwQkFBQTtJQUNBLFNBQUE7RUE5RUY7RUFpRkE7SUFDRSxnQkFBQTtFQS9FRjtFQWtGQTtJQUNFLGFBQUE7SUFDQSwyREFBQTtJQUNBLFNBQUE7RUFoRkY7QUFDRjtBQW1GQTtFQUNFO0lBQ0UsdUJBQUE7RUFqRkY7RUFtRkU7SUFDRSxlQUFBO0VBakZKO0VBb0ZFO0lBQ0UsZUFBQTtFQWxGSjtFQXNGQTtJQUNFLGtCQUFBO0lBQ0EsNEJBQUE7RUFwRkY7RUF1RkE7SUFDRSwwQkFBQTtFQXJGRjtFQXdGQTtJQUNFLGtCQUFBO0VBdEZGO0VBeUZBO0lBQ0Usa0JBQUE7RUF2RkY7RUEwRkE7SUFDRSxzQkFBQTtJQUNBLHVCQUFBO0VBeEZGO0VBMEZFO0lBQ0UsZUFBQTtFQXhGSjtBQUNGO0FBNkZBO0VBQ0U7SUFDRSxtQkFBQTtFQTNGRjtFQTZGQTtJQUNFLHNCQUFBO0VBM0ZGO0FBQ0Y7QUE4RkE7RUFDRTtJQUNFLFVBQUE7SUFDQSxxQkFBQTtFQTVGRjtFQThGQTtJQUNFLFVBQUE7SUFDQSxtQkFBQTtFQTVGRjtBQUNGO0FBZ0dBO0VBQ0UsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLDZDQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7RUFDQSxlQUFBO0FBOUZGO0FBZ0dFO0VBQ0Usa0JBQUE7QUE5Rko7QUErRkk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtBQTdGTiIsInNvdXJjZXNDb250ZW50IjpbIi8vIE1vZGVybiBjb250YWluZXIgbGF5b3V0XHJcbi5tb2Rlcm4tY29udGFpbmVyIHtcclxuICBtaW4taGVpZ2h0OiAxMDB2aDtcclxuICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XHJcbiAgcGFkZGluZzogMDtcclxuICBvdmVyZmxvdy14OiBoaWRkZW47XHJcbiAgb3ZlcmZsb3cteTogYXV0bztcclxufVxyXG5cclxuLy8gQ29udGVudCB3cmFwcGVyXHJcbi5jb250ZW50LXdyYXBwZXIge1xyXG4gIGZsZXg6IDE7XHJcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXNlY29uZGFyeSk7XHJcbiAgcGFkZGluZzogNDBweDtcclxuICBtYXgtd2lkdGg6IDE2MDBweDtcclxuICBtYXJnaW46IDAgYXV0bztcclxuICB3aWR0aDogMTAwJTtcclxuICBtaW4taGVpZ2h0OiBjYWxjKDEwMHZoIC0gMzZweCk7XHJcbiAgYm94LXNpemluZzogYm9yZGVyLWJveDtcclxufVxyXG5cclxuLy8gTWFpbiBsYXlvdXQgLSBUd28gY29sdW1uc1xyXG4ubWFpbi1sYXlvdXQge1xyXG4gIGRpc3BsYXk6IGdyaWQ7XHJcbiAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnIgNDAwcHg7XHJcbiAgZ2FwOiAzMnB4O1xyXG4gIGFsaWduLWl0ZW1zOiBzdGFydDtcclxuICBoZWlnaHQ6IGNhbGMoMTAwdmggLSAxMTZweCk7IC8vIDM2cHggdGl0bGUgYmFyICsgODBweCBwYWRkaW5nXHJcbn1cclxuXHJcbi5sZWZ0LWNvbHVtbiB7XHJcbiAgbWluLXdpZHRoOiAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBoZWlnaHQ6IDEwMCU7XHJcbiAgbWF4LWhlaWdodDogMTAwJTtcclxufVxyXG5cclxuLnJpZ2h0LWNvbHVtbiB7XHJcbiAgcG9zaXRpb246IHN0aWNreTtcclxuICB0b3A6IDQwcHg7XHJcbiAgYWxpZ24tc2VsZjogc3RhcnQ7XHJcbn1cclxuXHJcbi8vIEFjdGlvbiBjYXJkcyBncmlkIChpbiByaWdodCBjb2x1bW4gLSB2ZXJ0aWNhbCBzdGFjaylcclxuLmFjdGlvbnMtZ3JpZCB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGdhcDogMTZweDtcclxufVxyXG5cclxuLy8gQWN0aW9uIGNhcmQgc3R5bGluZ1xyXG4uYWN0aW9uLWNhcmQge1xyXG4gIGN1cnNvcjogcG9pbnRlcjtcclxuICB0cmFuc2l0aW9uOiBhbGwgMC4zcyBjdWJpYy1iZXppZXIoMC40LCAwLCAwLjIsIDEpO1xyXG4gIGJvcmRlci1yYWRpdXM6IDE2cHg7XHJcbiAgYm9yZGVyOiBub25lO1xyXG4gIGJveC1zaGFkb3c6IDAgMnB4IDhweCB2YXIoLS1tZGUtc2hhZG93LWNvbG9yKTtcclxuICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gIGJhY2tncm91bmQ6IHZhcigtLW1kZS1iZy1wcmltYXJ5KTtcclxuXHJcbiAgJjpob3ZlciB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTRweCk7XHJcbiAgICBib3gtc2hhZG93OiAwIDEycHggMjRweCByZ2JhKDAsIDAsIDAsIDAuMTUpO1xyXG5cclxuICAgIC5hcnJvdy1pY29uIHtcclxuICAgICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVYKDRweCk7XHJcbiAgICB9XHJcblxyXG4gICAgLmljb24td3JhcHBlciB7XHJcbiAgICAgIHRyYW5zZm9ybTogc2NhbGUoMS4xKTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gICY6YWN0aXZlIHtcclxuICAgIHRyYW5zZm9ybTogdHJhbnNsYXRlWSgtMnB4KTtcclxuICB9XHJcblxyXG4gIC5jYXJkLWNvbnRlbnQge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDE2cHg7XHJcbiAgICBwYWRkaW5nOiAyMHB4IDE2cHg7XHJcbiAgICBwb3NpdGlvbjogcmVsYXRpdmU7XHJcbiAgfVxyXG5cclxuICAuaWNvbi13cmFwcGVyIHtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgd2lkdGg6IDQ4cHg7XHJcbiAgICBoZWlnaHQ6IDQ4cHg7XHJcbiAgICBib3JkZXItcmFkaXVzOiAxMnB4O1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxuICAgIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjNzIGVhc2U7XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDI0cHg7XHJcbiAgICAgIHdpZHRoOiAyNHB4O1xyXG4gICAgICBoZWlnaHQ6IDI0cHg7XHJcbiAgICAgIGNvbG9yOiB3aGl0ZTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5jYXJkLXRleHQge1xyXG4gICAgZmxleDogMTtcclxuXHJcbiAgICAuY2FyZC10aXRsZSB7XHJcbiAgICAgIG1hcmdpbjogMCAwIDRweCAwO1xyXG4gICAgICBmb250LXNpemU6IDFyZW07XHJcbiAgICAgIGZvbnQtd2VpZ2h0OiA1MDA7XHJcbiAgICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcclxuICAgIH1cclxuXHJcbiAgICAuY2FyZC1kZXNjcmlwdGlvbiB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAwLjc1cmVtO1xyXG4gICAgICBjb2xvcjogdmFyKC0tbWRlLXRleHQtc2Vjb25kYXJ5KTtcclxuICAgICAgbGluZS1oZWlnaHQ6IDEuNDtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5jYXJkLWFjdGlvbiB7XHJcbiAgICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgICAuYXJyb3ctaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgY29sb3I6ICNhMGFlYzA7XHJcbiAgICAgIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjNzIGVhc2U7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAvLyBTcGVjaWZpYyBjYXJkIGNvbG9yc1xyXG4gICYuY2xvbmUtY2FyZCAuaWNvbi13cmFwcGVyIHtcclxuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgfVxyXG5cclxuICAmLmNyZWF0ZS1jYXJkIC5pY29uLXdyYXBwZXIge1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgI2YwOTNmYiAwJSwgI2Y1NTc2YyAxMDAlKTtcclxuICB9XHJcblxyXG4gICYuc2V0dGluZ3MtY2FyZCAuaWNvbi13cmFwcGVyIHtcclxuICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM0ZmFjZmUgMCUsICMwMGYyZmUgMTAwJSk7XHJcbiAgfVxyXG5cclxuICAmLnAycC1jYXJkIC5pY29uLXdyYXBwZXIge1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzExOTk4ZSAwJSwgIzM4ZWY3ZCAxMDAlKTtcclxuICB9XHJcbn1cclxuXHJcbi8vIFNlY3Rpb24gdGl0bGVcclxuLnNlY3Rpb24tdGl0bGUge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICBnYXA6IDEycHg7XHJcbiAgZm9udC1zaXplOiAxLjVyZW07XHJcbiAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICBjb2xvcjogdmFyKC0tbWRlLXRleHQtcHJpbWFyeSk7XHJcbiAgbWFyZ2luOiAwIDAgMjRweCAwO1xyXG5cclxuICAuc2VjdGlvbi1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogMjhweDtcclxuICAgIHdpZHRoOiAyOHB4O1xyXG4gICAgaGVpZ2h0OiAyOHB4O1xyXG4gICAgY29sb3I6ICM2NjdlZWE7XHJcbiAgfVxyXG59XHJcblxyXG4vLyBTZWN0aW9uIGRpdmlkZXJcclxuLnNlY3Rpb24tZGl2aWRlciB7XHJcbiAgaGVpZ2h0OiAxcHg7XHJcbiAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KHRvIHJpZ2h0LCB0cmFuc3BhcmVudCwgdmFyKC0tbWRlLWJvcmRlci1jb2xvciksIHRyYW5zcGFyZW50KTtcclxuICBtYXJnaW46IDMycHggMDtcclxufVxyXG5cclxuLy8gUmVjZW50IFByb2plY3RzIFNlY3Rpb25cclxuLnJlY2VudC1wcm9qZWN0cy1zZWN0aW9uIHtcclxuICBtYXJnaW4tYm90dG9tOiAwO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBmbGV4OiAxO1xyXG4gIG1pbi1oZWlnaHQ6IDA7XHJcbn1cclxuXHJcbi8vIFNlY3Rpb24gaGVhZGVyIHdpdGggc2VhcmNoXHJcbi5zZWN0aW9uLWhlYWRlciB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gIGp1c3RpZnktY29udGVudDogc3BhY2UtYmV0d2VlbjtcclxuICBtYXJnaW4tYm90dG9tOiAyNHB4O1xyXG4gIGdhcDogMjRweDtcclxuICBmbGV4LXNocmluazogMDtcclxuXHJcbiAgLnNlY3Rpb24tdGl0bGUge1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gICAgZmxleC1zaHJpbms6IDA7XHJcbiAgfVxyXG5cclxuICAuc2VhcmNoLWZpZWxkIHtcclxuICAgIHdpZHRoOiAxMDAlO1xyXG4gICAgbWF4LXdpZHRoOiA0MDBweDtcclxuXHJcbiAgICAvLyBPdmVycmlkZSBNYXRlcmlhbCBEZXNpZ24gZGVmYXVsdCBtYXJnaW5zXHJcbiAgICAubWF0LWZvcm0tZmllbGQtd3JhcHBlciB7XHJcbiAgICAgIHBhZGRpbmctYm90dG9tOiAwO1xyXG4gICAgICBtYXJnaW4tYm90dG9tOiAwO1xyXG4gICAgfVxyXG5cclxuICAgIC5tYXQtZm9ybS1maWVsZC1pbmZpeCB7XHJcbiAgICAgIHBhZGRpbmc6IDAuNWVtIDA7XHJcbiAgICB9XHJcblxyXG4gICAgLm1hdC1mb3JtLWZpZWxkLXVuZGVybGluZSB7XHJcbiAgICAgIGRpc3BsYXk6IG5vbmU7XHJcbiAgICB9XHJcblxyXG4gICAgaW5wdXQge1xyXG4gICAgICBmb250LXNpemU6IDE0cHg7XHJcbiAgICB9XHJcblxyXG4gICAgbWF0LWljb24ge1xyXG4gICAgICBjb2xvcjogIzcxODA5NjtcclxuICAgIH1cclxuXHJcbiAgICBidXR0b24gbWF0LWljb24ge1xyXG4gICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgIHdpZHRoOiAxOHB4O1xyXG4gICAgICBoZWlnaHQ6IDE4cHg7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBObyBwcm9qZWN0cyBtZXNzYWdlXHJcbi5uby1wcm9qZWN0cy1tZXNzYWdlIHtcclxuICB0ZXh0LWFsaWduOiBjZW50ZXI7XHJcbiAgcGFkZGluZzogNjBweCAyMHB4O1xyXG4gIGNvbG9yOiAjNzE4MDk2O1xyXG4gIGZsZXg6IDE7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xyXG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcblxyXG4gIC5lbXB0eS1pY29uIHtcclxuICAgIGZvbnQtc2l6ZTogODBweDtcclxuICAgIHdpZHRoOiA4MHB4O1xyXG4gICAgaGVpZ2h0OiA4MHB4O1xyXG4gICAgY29sb3I6ICNjYmQ1ZTA7XHJcbiAgICBtYXJnaW46IDAgYXV0byAyMHB4O1xyXG4gIH1cclxuXHJcbiAgaDMge1xyXG4gICAgZm9udC1zaXplOiAxLjVyZW07XHJcbiAgICBmb250LXdlaWdodDogNTAwO1xyXG4gICAgY29sb3I6ICM0YTU1Njg7XHJcbiAgICBtYXJnaW46IDAgMCA4cHggMDtcclxuICB9XHJcblxyXG4gIHAge1xyXG4gICAgZm9udC1zaXplOiAxcmVtO1xyXG4gICAgbWFyZ2luOiAwO1xyXG4gIH1cclxufVxyXG5cclxuLnByb2plY3RzLWdyaWQge1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICBnYXA6IDEycHg7XHJcbiAgZmxleDogMTtcclxuICBvdmVyZmxvdy15OiBhdXRvO1xyXG4gIHBhZGRpbmctcmlnaHQ6IDhweDtcclxuICBtaW4taGVpZ2h0OiAwO1xyXG5cclxuICAvLyBDdXN0b20gc2Nyb2xsYmFyXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXIge1xyXG4gICAgd2lkdGg6IDhweDtcclxuICB9XHJcblxyXG4gICY6Oi13ZWJraXQtc2Nyb2xsYmFyLXRyYWNrIHtcclxuICAgIGJhY2tncm91bmQ6IHZhcigtLW1kZS1zY3JvbGxiYXItdHJhY2spO1xyXG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xyXG4gIH1cclxuXHJcbiAgJjo6LXdlYmtpdC1zY3JvbGxiYXItdGh1bWIge1xyXG4gICAgYmFja2dyb3VuZDogdmFyKC0tbWRlLXNjcm9sbGJhci10aHVtYik7XHJcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XHJcblxyXG4gICAgJjpob3ZlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6IHZhcigtLW1kZS1zY3JvbGxiYXItaG92ZXIpO1xyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLnByb2plY3QtY2FyZCB7XHJcbiAgY3Vyc29yOiBwb2ludGVyO1xyXG4gIHRyYW5zaXRpb246IGFsbCAwLjNzIGN1YmljLWJlemllcigwLjQsIDAsIDAuMiwgMSk7XHJcbiAgYm9yZGVyLXJhZGl1czogMTJweDtcclxuICBib3JkZXI6IDFweCBzb2xpZCB2YXIoLS1tZGUtYm9yZGVyLWNvbG9yKTtcclxuICBib3gtc2hhZG93OiAwIDFweCAzcHggdmFyKC0tbWRlLXNoYWRvdy1jb2xvcik7XHJcbiAgYmFja2dyb3VuZDogdmFyKC0tbWRlLWJnLXByaW1hcnkpO1xyXG4gIHBvc2l0aW9uOiByZWxhdGl2ZTtcclxuICBvdmVyZmxvdzogdmlzaWJsZTtcclxuXHJcbiAgJjpob3ZlciB7XHJcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTJweCk7XHJcbiAgICBib3gtc2hhZG93OiAwIDhweCAxNnB4IHJnYmEoMCwgMCwgMCwgMC4xKTtcclxuICAgIGJvcmRlci1jb2xvcjogIzY2N2VlYTtcclxuICB9XHJcblxyXG4gIC8vIFNwZWNpYWwgc3R5bGluZyBmb3IgbGFzdCBvcGVuZWQgcHJvamVjdFxyXG4gICYubGFzdC1vcGVuZWQge1xyXG4gICAgYm9yZGVyOiAycHggc29saWQgIzY2N2VlYTtcclxuICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjIpO1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjAzKSAwJSwgcmdiYSgxMTgsIDc1LCAxNjIsIDAuMDMpIDEwMCUpO1xyXG5cclxuICAgICY6aG92ZXIge1xyXG4gICAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoLTRweCk7XHJcbiAgICAgIGJveC1zaGFkb3c6IDAgMTJweCAyNHB4IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC4zKTtcclxuICAgICAgYm9yZGVyLWNvbG9yOiAjNzY0YmEyO1xyXG4gICAgfVxyXG5cclxuICAgIC5wcm9qZWN0LWljb24td3JhcHBlciB7XHJcbiAgICAgIGJhY2tncm91bmQ6IGxpbmVhci1ncmFkaWVudCgxMzVkZWcsICM2NjdlZWEgMCUsICM3NjRiYTIgMTAwJSk7XHJcbiAgICAgIGJveC1zaGFkb3c6IDAgNHB4IDEycHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjQpO1xyXG4gICAgICBhbmltYXRpb246IHB1bHNlIDJzIGVhc2UtaW4tb3V0IGluZmluaXRlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtY2FyZC1jb250ZW50IHtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogc3RyZXRjaDtcclxuICAgIHBhZGRpbmc6IDEycHggMTZweDtcclxuICAgIGdhcDogMTRweDtcclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWluZm8ge1xyXG4gICAgZGlzcGxheTogZmxleDtcclxuICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICBnYXA6IDEycHg7XHJcbiAgICBmbGV4OiAwIDAgYXV0bztcclxuICAgIHdpZHRoOiBjbGFtcCgyMjBweCwgMzQlLCAzNDBweCk7XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcbiAgICBjdXJzb3I6IHBvaW50ZXI7XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1kZXNjcmlwdGlvbi1ib3gge1xyXG4gICAgZmxleDogMSAxIGF1dG87XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgIHBhZGRpbmc6IDhweCAxMnB4O1xyXG4gICAgYm9yZGVyOiAxcHggc29saWQgdmFyKC0tbWRlLWJvcmRlci1jb2xvcik7XHJcbiAgICBib3JkZXItcmFkaXVzOiA4cHg7XHJcbiAgICBiYWNrZ3JvdW5kOiB2YXIoLS1tZGUtYmctc2Vjb25kYXJ5KTtcclxuICAgIGN1cnNvcjogcG9pbnRlcjtcclxuICAgIHRyYW5zaXRpb246IGJvcmRlci1jb2xvciAwLjJzIGVhc2UsIGJhY2tncm91bmQgMC4ycyBlYXNlO1xyXG5cclxuICAgICY6aG92ZXIge1xyXG4gICAgICBib3JkZXItY29sb3I6IHJnYmEoMTAyLCAxMjYsIDIzNCwgMC41KTtcclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1kZXNjcmlwdGlvbiB7XHJcbiAgICAgIG1hcmdpbjogMDtcclxuICAgICAgZm9udC1zaXplOiAwLjhyZW07XHJcbiAgICAgIGNvbG9yOiB2YXIoLS1tZGUtdGV4dC1wcmltYXJ5KTtcclxuICAgICAgbGluZS1oZWlnaHQ6IDEuNDtcclxuICAgICAgZGlzcGxheTogLXdlYmtpdC1ib3g7XHJcbiAgICAgIC13ZWJraXQtbGluZS1jbGFtcDogMztcclxuICAgICAgLXdlYmtpdC1ib3gtb3JpZW50OiB2ZXJ0aWNhbDtcclxuICAgICAgb3ZlcmZsb3c6IGhpZGRlbjtcclxuICAgICAgdGV4dC1vdmVyZmxvdzogZWxsaXBzaXM7XHJcbiAgICAgIHdvcmQtYnJlYWs6IGJyZWFrLXdvcmQ7XHJcbiAgICAgIHdpZHRoOiAxMDAlO1xyXG4gICAgfVxyXG5cclxuICAgICYuZW1wdHkge1xyXG4gICAgICBib3JkZXItc3R5bGU6IGRhc2hlZDtcclxuICAgICAgYmFja2dyb3VuZDogdHJhbnNwYXJlbnQ7XHJcblxyXG4gICAgICAmOmhvdmVyIHtcclxuICAgICAgICBiYWNrZ3JvdW5kOiByZ2JhKDEwMiwgMTI2LCAyMzQsIDAuMDQpO1xyXG4gICAgICB9XHJcblxyXG4gICAgICAuZW1wdHktcGxhY2Vob2xkZXIge1xyXG4gICAgICAgIGRpc3BsYXk6IGlubGluZS1mbGV4O1xyXG4gICAgICAgIGFsaWduLWl0ZW1zOiBjZW50ZXI7XHJcbiAgICAgICAgZ2FwOiA2cHg7XHJcbiAgICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XHJcbiAgICAgICAgb3BhY2l0eTogMC42O1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMC43OHJlbTtcclxuICAgICAgICBmb250LXN0eWxlOiBpdGFsaWM7XHJcblxyXG4gICAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICAgIGZvbnQtc2l6ZTogMTZweDtcclxuICAgICAgICAgIHdpZHRoOiAxNnB4O1xyXG4gICAgICAgICAgaGVpZ2h0OiAxNnB4O1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgLnByb2plY3QtaWNvbi13cmFwcGVyIHtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgd2lkdGg6IDQwcHg7XHJcbiAgICBoZWlnaHQ6IDQwcHg7XHJcbiAgICBib3JkZXItcmFkaXVzOiAxMHB4O1xyXG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcclxuICAgIGRpc3BsYXk6IGZsZXg7XHJcbiAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcblxyXG4gICAgLnByb2plY3QtaWNvbiB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMjBweDtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgfVxyXG5cclxuICAgIC5wcm9qZWN0LWljb24tc3ZnIHtcclxuICAgICAgd2lkdGg6IDIwcHg7XHJcbiAgICAgIGhlaWdodDogMjBweDtcclxuICAgICAgZmlsdGVyOiBicmlnaHRuZXNzKDApIGludmVydCgxKTsgLy8gTWFrZXMgU1ZHIHdoaXRlXHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICAucHJvamVjdC1kZXRhaWxzIHtcclxuICAgIGZsZXg6IDE7XHJcbiAgICBtaW4td2lkdGg6IDA7XHJcbiAgICBkaXNwbGF5OiBmbGV4O1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGdhcDogMnB4O1xyXG5cclxuICAgIC5wcm9qZWN0LW5hbWUtcm93IHtcclxuICAgICAgZGlzcGxheTogZmxleDtcclxuICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcclxuICAgICAgZ2FwOiA4cHg7XHJcbiAgICAgIG1pbi13aWR0aDogMDtcclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1uYW1lIHtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgICBmb250LXNpemU6IDEuMXJlbTtcclxuICAgICAgZm9udC13ZWlnaHQ6IDUwMDtcclxuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXByaW1hcnkpO1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBvdmVyZmxvdzogaGlkZGVuO1xyXG4gICAgICB0ZXh0LW92ZXJmbG93OiBlbGxpcHNpcztcclxuICAgICAgbWluLXdpZHRoOiAwO1xyXG4gICAgICBmbGV4LXNocmluazogMTtcclxuICAgIH1cclxuXHJcbiAgICAucHJvamVjdC1wYXRoIHtcclxuICAgICAgbWFyZ2luOiAwO1xyXG4gICAgICBmb250LXNpemU6IDAuOHJlbTtcclxuICAgICAgY29sb3I6IHZhcigtLW1kZS10ZXh0LXNlY29uZGFyeSk7XHJcbiAgICAgIHdoaXRlLXNwYWNlOiBub3dyYXA7XHJcbiAgICAgIG92ZXJmbG93OiBoaWRkZW47XHJcbiAgICAgIHRleHQtb3ZlcmZsb3c6IGVsbGlwc2lzO1xyXG4gICAgfVxyXG5cclxuICAgIC5sYXN0LW9wZW5lZC1iYWRnZSB7XHJcbiAgICAgIGRpc3BsYXk6IGlubGluZS1mbGV4O1xyXG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xyXG4gICAgICBnYXA6IDRweDtcclxuICAgICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KDEzNWRlZywgIzY2N2VlYSAwJSwgIzc2NGJhMiAxMDAlKTtcclxuICAgICAgY29sb3I6IHdoaXRlO1xyXG4gICAgICBwYWRkaW5nOiAzcHggOHB4O1xyXG4gICAgICBib3JkZXItcmFkaXVzOiAxMHB4O1xyXG4gICAgICBmb250LXNpemU6IDAuNnJlbTtcclxuICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcclxuICAgICAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcclxuICAgICAgbGV0dGVyLXNwYWNpbmc6IDAuNHB4O1xyXG4gICAgICB3aGl0ZS1zcGFjZTogbm93cmFwO1xyXG4gICAgICBib3gtc2hhZG93OiAwIDJweCA2cHggcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjMpO1xyXG4gICAgICBhbmltYXRpb246IGZhZGVJblNjYWxlIDAuNXMgZWFzZS1vdXQ7XHJcbiAgICAgIGZsZXgtc2hyaW5rOiAwO1xyXG5cclxuICAgICAgLmJhZGdlLWljb24ge1xyXG4gICAgICAgIGZvbnQtc2l6ZTogMTFweDtcclxuICAgICAgICB3aWR0aDogMTFweDtcclxuICAgICAgICBoZWlnaHQ6IDExcHg7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5wcm9qZWN0LWFjdGlvbnMge1xyXG4gICAgZGlzcGxheTogZ3JpZDtcclxuICAgIGdyaWQtdGVtcGxhdGUtY29sdW1uczogcmVwZWF0KDIsIDMycHgpO1xyXG4gICAgZ3JpZC1hdXRvLXJvd3M6IDMycHg7XHJcbiAgICBnYXA6IDJweDtcclxuICAgIGZsZXgtc2hyaW5rOiAwO1xyXG4gICAgYWxpZ24tc2VsZjogY2VudGVyO1xyXG5cclxuICAgIC5hY3Rpb24tYnRuIHtcclxuICAgICAgdHJhbnNpdGlvbjogYWxsIDAuMnMgZWFzZTtcclxuICAgICAgd2lkdGg6IDMycHg7XHJcbiAgICAgIGhlaWdodDogMzJweDtcclxuICAgICAgbGluZS1oZWlnaHQ6IDMycHg7XHJcbiAgICAgIHBhZGRpbmc6IDA7XHJcblxyXG4gICAgICAvLyBTaHJpbmsgdGhlIE1hdGVyaWFsIHJpcHBsZSBjb250YWluZXIgc28gaG92ZXIvZm9jdXMgc3RheXMgaW5zaWRlIHRoZSAzMngzMiBib3hcclxuICAgICAgOjpuZy1kZWVwIC5tYXQtbWRjLWJ1dHRvbi10b3VjaC10YXJnZXQge1xyXG4gICAgICAgIHdpZHRoOiAzMnB4ICFpbXBvcnRhbnQ7XHJcbiAgICAgICAgaGVpZ2h0OiAzMnB4ICFpbXBvcnRhbnQ7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgICYuZWRpdC1idG46aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoMjU1LCAxNTIsIDAsIDAuMTIpO1xyXG4gICAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICAgIGNvbG9yOiAjZmY5ODAwO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgJi5zaGFyZS1idG46aG92ZXIge1xyXG4gICAgICAgIGJhY2tncm91bmQtY29sb3I6IHJnYmEoNzYsIDE3NSwgODAsIDAuMSk7XHJcbiAgICAgICAgbWF0LWljb24ge1xyXG4gICAgICAgICAgY29sb3I6ICM0Y2FmNTA7XHJcbiAgICAgICAgfVxyXG4gICAgICB9XHJcblxyXG4gICAgICAmLnNldHRpbmdzLWJ0bjpob3ZlciB7XHJcbiAgICAgICAgYmFja2dyb3VuZC1jb2xvcjogcmdiYSgxMDIsIDEyNiwgMjM0LCAwLjEpO1xyXG4gICAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICAgIGNvbG9yOiAjNjY3ZWVhO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG5cclxuICAgICAgJi5kZWxldGUtYnRuOmhvdmVyIHtcclxuICAgICAgICBiYWNrZ3JvdW5kLWNvbG9yOiByZ2JhKDI0NSwgODcsIDEwOCwgMC4xKTtcclxuICAgICAgICBtYXQtaWNvbiB7XHJcbiAgICAgICAgICBjb2xvcjogI2Y1NTc2YztcclxuICAgICAgICB9XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIG1hdC1pY29uIHtcclxuICAgICAgICBmb250LXNpemU6IDE4cHg7XHJcbiAgICAgICAgd2lkdGg6IDE4cHg7XHJcbiAgICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgIGxpbmUtaGVpZ2h0OiAxOHB4O1xyXG4gICAgICAgIGNvbG9yOiAjYTBhZWMwO1xyXG4gICAgICAgIHRyYW5zaXRpb246IGNvbG9yIDAuMnMgZWFzZTtcclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxufVxyXG5cclxuLy8gU2lkZWJhciBzZWN0aW9uIChoaWRkZW4gYnkgZGVmYXVsdClcclxuLnNpZGViYXItc2VjdGlvbiB7XHJcbiAgbWFyZ2luLWJvdHRvbTogMjRweDtcclxufVxyXG5cclxuLy8gUmVzcG9uc2l2ZSBkZXNpZ25cclxuQG1lZGlhIChtYXgtd2lkdGg6IDEwMjRweCkge1xyXG4gIC5tYWluLWxheW91dCB7XHJcbiAgICBncmlkLXRlbXBsYXRlLWNvbHVtbnM6IDFmcjtcclxuICAgIGdhcDogMzJweDtcclxuICB9XHJcblxyXG4gIC5yaWdodC1jb2x1bW4ge1xyXG4gICAgcG9zaXRpb246IHN0YXRpYztcclxuICB9XHJcblxyXG4gIC5hY3Rpb25zLWdyaWQge1xyXG4gICAgZGlzcGxheTogZ3JpZDtcclxuICAgIGdyaWQtdGVtcGxhdGUtY29sdW1uczogcmVwZWF0KGF1dG8tZml0LCBtaW5tYXgoMjgwcHgsIDFmcikpO1xyXG4gICAgZ2FwOiAxNnB4O1xyXG4gIH1cclxufVxyXG5cclxuQG1lZGlhIChtYXgtd2lkdGg6IDc2OHB4KSB7XHJcbiAgLmhlYWRlci1zZWN0aW9uIHtcclxuICAgIHBhZGRpbmc6IDQwcHggMjBweCAyMHB4O1xyXG5cclxuICAgIC5tYWluLXRpdGxlIHtcclxuICAgICAgZm9udC1zaXplOiAycmVtO1xyXG4gICAgfVxyXG5cclxuICAgIC5zdWJ0aXRsZSB7XHJcbiAgICAgIGZvbnQtc2l6ZTogMXJlbTtcclxuICAgIH1cclxuICB9XHJcblxyXG4gIC5jb250ZW50LXdyYXBwZXIge1xyXG4gICAgcGFkZGluZzogMzJweCAyMHB4O1xyXG4gICAgYm9yZGVyLXJhZGl1czogMjRweCAyNHB4IDAgMDtcclxuICB9XHJcblxyXG4gIC5hY3Rpb25zLWdyaWQge1xyXG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiAxZnI7XHJcbiAgfVxyXG5cclxuICAuYWN0aW9uLWNhcmQgLmNhcmQtY29udGVudCB7XHJcbiAgICBwYWRkaW5nOiAyNHB4IDIwcHg7XHJcbiAgfVxyXG5cclxuICAuc2VjdGlvbi10aXRsZSB7XHJcbiAgICBmb250LXNpemU6IDEuMjVyZW07XHJcbiAgfVxyXG5cclxuICAuc2VjdGlvbi1oZWFkZXIge1xyXG4gICAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcclxuICAgIGFsaWduLWl0ZW1zOiBmbGV4LXN0YXJ0O1xyXG5cclxuICAgIC5zZWFyY2gtZmllbGQge1xyXG4gICAgICBtYXgtd2lkdGg6IDEwMCU7XHJcbiAgICB9XHJcbiAgfVxyXG59XHJcblxyXG4vLyBBbmltYXRpb25zXHJcbkBrZXlmcmFtZXMgcHVsc2Uge1xyXG4gIDAlLCAxMDAlIHtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMSk7XHJcbiAgfVxyXG4gIDUwJSB7XHJcbiAgICB0cmFuc2Zvcm06IHNjYWxlKDEuMDUpO1xyXG4gIH1cclxufVxyXG5cclxuQGtleWZyYW1lcyBmYWRlSW5TY2FsZSB7XHJcbiAgMCUge1xyXG4gICAgb3BhY2l0eTogMDtcclxuICAgIHRyYW5zZm9ybTogc2NhbGUoMC44KTtcclxuICB9XHJcbiAgMTAwJSB7XHJcbiAgICBvcGFjaXR5OiAxO1xyXG4gICAgdHJhbnNmb3JtOiBzY2FsZSgxKTtcclxuICB9XHJcbn1cclxuXHJcbi8vIERpYWdub3N0aWNzIHNlY3Rpb25cclxuLmRpYWdub3N0aWNzLXNlY3Rpb24ge1xyXG4gIG1hcmdpbi10b3A6IDI0cHg7XHJcbiAgcGFkZGluZy10b3A6IDE2cHg7XHJcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkIHZhcigtLW1kZS1ib3JkZXItY29sb3IpO1xyXG4gIGRpc3BsYXk6IGZsZXg7XHJcbiAgZ2FwOiAxMnB4O1xyXG4gIGZsZXgtd3JhcDogd3JhcDtcclxuXHJcbiAgYnV0dG9uIHtcclxuICAgIGZvbnQtc2l6ZTogMC44NXJlbTtcclxuICAgIG1hdC1pY29uIHtcclxuICAgICAgZm9udC1zaXplOiAxOHB4O1xyXG4gICAgICB3aWR0aDogMThweDtcclxuICAgICAgaGVpZ2h0OiAxOHB4O1xyXG4gICAgICBtYXJnaW4tcmlnaHQ6IDZweDtcclxuICAgIH1cclxuICB9XHJcbn1cclxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"]
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
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./open-recent/open-recent.component */ 7111);
/* harmony import */ var _projects_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./projects.component */ 5609);
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./new-project/new-project.component */ 5389);
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ 7179);
/* harmony import */ var _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/modern-clone-project/modern-clone-project.component */ 443);
/* harmony import */ var _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./dialogs/project-create-config/project-create-config-dialog.component */ 983);
/* harmony import */ var _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./dialogs/project-edit/project-edit-dialog.component */ 9116);
/* harmony import */ var _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./project-settings/project-settings.component */ 2482);
/* harmony import */ var _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./dialogs/p2p-manager/p2p-manager.component */ 1902);
/* harmony import */ var _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./dialogs/catalog-picker/catalog-picker.component */ 5922);
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../shared/material.module */ 4872);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _git_git_module__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../git/git.module */ 1312);
/* harmony import */ var _services_project_settings_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./services/project-settings.service */ 5450);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/core */ 2560);



















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
    this.ɵmod = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵdefineNgModule"]({
      type: ProjectsModule
    });
  }
  static {
    this.ɵinj = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵdefineInjector"]({
      providers: [_services_project_settings_service__WEBPACK_IMPORTED_MODULE_12__.ProjectSettingsService],
      imports: [_angular_router__WEBPACK_IMPORTED_MODULE_14__.RouterModule.forChild(routes), _angular_common__WEBPACK_IMPORTED_MODULE_15__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_10__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.FormsModule, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.ReactiveFormsModule, _git_git_module__WEBPACK_IMPORTED_MODULE_11__.GitModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_17__.TranslateModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_13__["ɵɵsetNgModuleScope"](ProjectsModule, {
    declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_0__.OpenRecentComponent, _projects_component__WEBPACK_IMPORTED_MODULE_1__.ProjectsComponent, _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_2__.NewProjectComponent, _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_3__.CloneProjectComponent, _dialogs_modern_clone_project_modern_clone_project_component__WEBPACK_IMPORTED_MODULE_4__.ModernCloneProjectComponent, _dialogs_project_create_config_project_create_config_dialog_component__WEBPACK_IMPORTED_MODULE_5__.ProjectCreateConfigDialogComponent, _dialogs_project_edit_project_edit_dialog_component__WEBPACK_IMPORTED_MODULE_6__.ProjectEditDialogComponent, _project_settings_project_settings_component__WEBPACK_IMPORTED_MODULE_7__.ProjectSettingsComponent, _dialogs_p2p_manager_p2p_manager_component__WEBPACK_IMPORTED_MODULE_8__.P2PManagerComponent, _dialogs_catalog_picker_catalog_picker_component__WEBPACK_IMPORTED_MODULE_9__.CatalogPickerDialogComponent],
    imports: [_angular_router__WEBPACK_IMPORTED_MODULE_14__.RouterModule, _angular_common__WEBPACK_IMPORTED_MODULE_15__.CommonModule, _shared_material_module__WEBPACK_IMPORTED_MODULE_10__.MaterialModule, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.FormsModule, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.ReactiveFormsModule, _git_git_module__WEBPACK_IMPORTED_MODULE_11__.GitModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_17__.TranslateModule]
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
//# sourceMappingURL=src_app_projects_projects_module_ts.js.map