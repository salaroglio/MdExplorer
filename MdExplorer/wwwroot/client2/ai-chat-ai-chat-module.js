(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["ai-chat-ai-chat-module"],{

/***/ "H3KX":
/*!****************************************************!*\
  !*** ./src/app/ai-chat/model-manager.component.ts ***!
  \****************************************************/
/*! exports provided: ModelManagerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ModelManagerComponent", function() { return ModelManagerComponent; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../services/ai-chat.service */ "oRpr");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/progress-bar */ "bv9b");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");










function ModelManagerComponent_span_6_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" Current Model: ", ctx_r0.currentModel, " ");
} }
function ModelManagerComponent_span_7_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, " No model loaded ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_progress_bar_14_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "mat-progress-bar", 8);
} }
function ModelManagerComponent_mat_card_16_mat_chip_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Installed ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_card_16_mat_chip_14_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "memory");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Loaded ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_card_16_div_15_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](6, "number");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](7, "mat-progress-bar", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "div", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const model_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit;
    const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r8.downloadProgress[model_r5.id].status);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"]("", _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](6, 5, ctx_r8.downloadProgress[model_r5.id].percentComplete, "1.1-1"), "%");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("value", ctx_r8.downloadProgress[model_r5.id].percentComplete);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate2"](" ", ctx_r8.formatBytes(ctx_r8.downloadProgress[model_r5.id].bytesDownloaded), " / ", ctx_r8.formatBytes(ctx_r8.downloadProgress[model_r5.id].totalBytes), " ");
} }
function ModelManagerComponent_mat_card_16_button_17_Template(rf, ctx) { if (rf & 1) {
    const _r16 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ModelManagerComponent_mat_card_16_button_17_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r16); const model_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r14.downloadModel(model_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "download");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Download ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_card_16_button_18_Template(rf, ctx) { if (rf & 1) {
    const _r19 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ModelManagerComponent_mat_card_16_button_18_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r19); const model_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r17.loadModel(model_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "play_arrow");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Load ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx_r10.loading);
} }
function ModelManagerComponent_mat_card_16_button_19_Template(rf, ctx) { if (rf & 1) {
    const _r22 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ModelManagerComponent_mat_card_16_button_19_Template_button_click_0_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵrestoreView"](_r22); const model_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]().$implicit; const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"](); return ctx_r20.deleteModel(model_r5); });
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Delete ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_card_16_button_20_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "button", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "check");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " Currently Loaded ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function ModelManagerComponent_mat_card_16_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-card", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-card-header");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "mat-card-content");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "p", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](9, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "span", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](11);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "mat-chip-list");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](13, ModelManagerComponent_mat_card_16_mat_chip_13_Template, 4, 0, "mat-chip", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](14, ModelManagerComponent_mat_card_16_mat_chip_14_Template, 4, 0, "mat-chip", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](15, ModelManagerComponent_mat_card_16_div_15_Template, 10, 8, "div", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](16, "mat-card-actions");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](17, ModelManagerComponent_mat_card_16_button_17_Template, 4, 0, "button", 16);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](18, ModelManagerComponent_mat_card_16_button_18_Template, 4, 1, "button", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](19, ModelManagerComponent_mat_card_16_button_19_Template, 4, 0, "button", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](20, ModelManagerComponent_mat_card_16_button_20_Template, 4, 0, "button", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const model_r5 = ctx.$implicit;
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](model_r5.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate2"](" ", model_r5.parameters, " parameters | ", model_r5.contextLength, " context ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](model_r5.description);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"]("Size: ", ctx_r3.formatBytes(model_r5.fileSize), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", model_r5.isInstalled);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r3.isCurrentModel(model_r5));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r3.isDownloading(model_r5.id));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !model_r5.isInstalled && !ctx_r3.isDownloading(model_r5.id));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", model_r5.isInstalled && !ctx_r3.isCurrentModel(model_r5));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", model_r5.isInstalled && !ctx_r3.isCurrentModel(model_r5));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx_r3.isCurrentModel(model_r5));
} }
function ModelManagerComponent_div_17_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "info");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "No models available");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
class ModelManagerComponent {
    constructor(aiService) {
        this.aiService = aiService;
        this.availableModels = [];
        this.downloadProgress = {};
        this.currentModel = null;
        this.isModelLoaded = false;
        this.loading = false;
        this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["Subject"]();
    }
    ngOnInit() {
        this.loadModels();
        // Subscribe to download progress
        this.aiService.downloadProgress$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(progress => {
            this.downloadProgress[progress.modelId] = progress;
        });
        // Subscribe to current model
        this.aiService.currentModel$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(model => {
            this.currentModel = model;
        });
        // Subscribe to model loaded status
        this.aiService.isModelLoaded$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(loaded => {
            this.isModelLoaded = loaded;
        });
    }
    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
    loadModels() {
        this.loading = true;
        this.aiService.getAvailableModels().subscribe({
            next: (models) => {
                this.availableModels = models;
                this.loading = false;
            },
            error: (err) => {
                console.error('Error loading models:', err);
                this.loading = false;
            }
        });
    }
    downloadModel(model) {
        if (this.isDownloading(model.id))
            return;
        this.downloadProgress[model.id] = {
            modelId: model.id,
            bytesDownloaded: 0,
            totalBytes: model.fileSize,
            percentComplete: 0,
            status: 'Starting'
        };
        this.aiService.downloadModel(model.id).subscribe({
            next: () => {
                console.log(`Download started for ${model.name}`);
            },
            error: (err) => {
                console.error(`Error downloading ${model.name}:`, err);
                delete this.downloadProgress[model.id];
            }
        });
    }
    deleteModel(model) {
        if (!confirm(`Delete model ${model.name}?`))
            return;
        this.aiService.deleteModel(model.id).subscribe({
            next: () => {
                this.loadModels();
            },
            error: (err) => {
                console.error(`Error deleting ${model.name}:`, err);
            }
        });
    }
    loadModel(model) {
        console.log('[ModelManager] loadModel() called with:', model);
        if (!model.isInstalled) {
            console.log('[ModelManager] Model not installed, returning');
            return;
        }
        console.log('[ModelManager] Starting to load model:', model.id);
        this.loading = true;
        this.aiService.loadModel(model.id).subscribe({
            next: (response) => {
                console.log(`[ModelManager] Model ${model.name} loaded successfully, response:`, response);
                this.loading = false;
                // Refresh models list to update status
                this.loadModels();
            },
            error: (err) => {
                console.error(`[ModelManager] Error loading ${model.name}:`, err);
                console.error('[ModelManager] Error details:', err.error || err);
                this.loading = false;
            }
        });
    }
    isDownloading(modelId) {
        const progress = this.downloadProgress[modelId];
        return progress && progress.status !== 'Error' && progress.status !== 'Cancelled' && progress.percentComplete < 100;
    }
    formatBytes(bytes) {
        if (bytes === 0)
            return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }
    isCurrentModel(model) {
        var _a;
        return this.currentModel === ((_a = model.fileName) === null || _a === void 0 ? void 0 : _a.replace('.gguf', ''));
    }
}
ModelManagerComponent.ɵfac = function ModelManagerComponent_Factory(t) { return new (t || ModelManagerComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__["AiChatService"])); };
ModelManagerComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: ModelManagerComponent, selectors: [["app-model-manager"]], decls: 18, vars: 6, consts: [[1, "model-manager"], [4, "ngIf"], [1, "actions-bar"], ["mat-raised-button", "", "color", "primary", 3, "disabled", "click"], ["mode", "indeterminate", 4, "ngIf"], [1, "models-grid"], ["class", "model-card", 4, "ngFor", "ngForOf"], ["class", "no-models", 4, "ngIf"], ["mode", "indeterminate"], [1, "model-card"], [1, "model-description"], [1, "model-info"], [1, "file-size"], ["color", "primary", "selected", "", 4, "ngIf"], ["color", "accent", "selected", "", 4, "ngIf"], ["class", "download-progress", 4, "ngIf"], ["mat-button", "", "color", "primary", 3, "click", 4, "ngIf"], ["mat-button", "", "color", "accent", 3, "disabled", "click", 4, "ngIf"], ["mat-button", "", "color", "warn", 3, "click", 4, "ngIf"], ["mat-button", "", "disabled", "", 4, "ngIf"], ["color", "primary", "selected", ""], ["color", "accent", "selected", ""], [1, "download-progress"], [1, "progress-info"], ["mode", "determinate", 3, "value"], [1, "progress-bytes"], ["mat-button", "", "color", "primary", 3, "click"], ["mat-button", "", "color", "accent", 3, "disabled", "click"], ["mat-button", "", "color", "warn", 3, "click"], ["mat-button", "", "disabled", ""], [1, "no-models"]], template: function ModelManagerComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-card-header");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "AI Model Manager");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](6, ModelManagerComponent_span_6_Template, 2, 1, "span", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](7, ModelManagerComponent_span_7_Template, 2, 0, "span", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "mat-card-content");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](9, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](10, "button", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function ModelManagerComponent_Template_button_click_10_listener() { return ctx.loadModels(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](12, "refresh");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](13, " Refresh ");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](14, ModelManagerComponent_mat_progress_bar_14_Template, 1, 0, "mat-progress-bar", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](15, "div", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](16, ModelManagerComponent_mat_card_16_Template, 21, 12, "mat-card", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](17, ModelManagerComponent_div_17_Template, 5, 0, "div", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isModelLoaded && ctx.currentModel);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isModelLoaded);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.loading);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.availableModels);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.availableModels.length === 0 && !ctx.loading);
    } }, directives: [_angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardHeader"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardSubtitle"], _angular_common__WEBPACK_IMPORTED_MODULE_5__["NgIf"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardContent"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"], _angular_common__WEBPACK_IMPORTED_MODULE_5__["NgForOf"], _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_8__["MatProgressBar"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__["MatChipList"], _angular_material_card__WEBPACK_IMPORTED_MODULE_4__["MatCardActions"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__["MatChip"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_5__["DecimalPipe"]], styles: [".model-manager[_ngcontent-%COMP%] {\n  padding: 20px;\n  max-width: 1200px;\n  margin: 0 auto;\n}\n.model-manager[_ngcontent-%COMP%]   .actions-bar[_ngcontent-%COMP%] {\n  margin-bottom: 20px;\n  display: flex;\n  gap: 10px;\n}\n.model-manager[_ngcontent-%COMP%]   .models-grid[_ngcontent-%COMP%] {\n  display: grid;\n  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));\n  gap: 20px;\n  margin-top: 20px;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .model-description[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.6);\n  margin: 10px 0;\n  min-height: 40px;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .model-info[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  align-items: center;\n  margin: 15px 0;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .model-info[_ngcontent-%COMP%]   .file-size[_ngcontent-%COMP%] {\n  color: rgba(0, 0, 0, 0.54);\n  font-size: 0.9em;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .download-progress[_ngcontent-%COMP%] {\n  margin: 15px 0;\n  padding: 10px;\n  background: #f5f5f5;\n  border-radius: 4px;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .download-progress[_ngcontent-%COMP%]   .progress-info[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  margin-bottom: 8px;\n  font-size: 0.9em;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   .download-progress[_ngcontent-%COMP%]   .progress-bytes[_ngcontent-%COMP%] {\n  margin-top: 8px;\n  font-size: 0.85em;\n  color: rgba(0, 0, 0, 0.54);\n  text-align: center;\n}\n.model-manager[_ngcontent-%COMP%]   .model-card[_ngcontent-%COMP%]   mat-card-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  padding: 8px 16px;\n}\n.model-manager[_ngcontent-%COMP%]   .no-models[_ngcontent-%COMP%] {\n  text-align: center;\n  padding: 40px;\n  color: rgba(0, 0, 0, 0.54);\n}\n.model-manager[_ngcontent-%COMP%]   .no-models[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 48px;\n  width: 48px;\n  height: 48px;\n  margin-bottom: 10px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uLy4uLy4uL21vZGVsLW1hbmFnZXIuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0EsaUJBQUE7RUFDQSxjQUFBO0FBQ0Y7QUFDRTtFQUNFLG1CQUFBO0VBQ0EsYUFBQTtFQUNBLFNBQUE7QUFDSjtBQUVFO0VBQ0UsYUFBQTtFQUNBLDREQUFBO0VBQ0EsU0FBQTtFQUNBLGdCQUFBO0FBQUo7QUFJSTtFQUNFLHlCQUFBO0VBQ0EsY0FBQTtFQUNBLGdCQUFBO0FBRk47QUFLSTtFQUNFLGFBQUE7RUFDQSw4QkFBQTtFQUNBLG1CQUFBO0VBQ0EsY0FBQTtBQUhOO0FBS007RUFDRSwwQkFBQTtFQUNBLGdCQUFBO0FBSFI7QUFPSTtFQUNFLGNBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtBQUxOO0FBT007RUFDRSxhQUFBO0VBQ0EsOEJBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBTFI7QUFRTTtFQUNFLGVBQUE7RUFDQSxpQkFBQTtFQUNBLDBCQUFBO0VBQ0Esa0JBQUE7QUFOUjtBQVVJO0VBQ0UsYUFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtBQVJOO0FBWUU7RUFDRSxrQkFBQTtFQUNBLGFBQUE7RUFDQSwwQkFBQTtBQVZKO0FBWUk7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtBQVZOIiwiZmlsZSI6Im1vZGVsLW1hbmFnZXIuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIubW9kZWwtbWFuYWdlciB7XG4gIHBhZGRpbmc6IDIwcHg7XG4gIG1heC13aWR0aDogMTIwMHB4O1xuICBtYXJnaW46IDAgYXV0bztcblxuICAuYWN0aW9ucy1iYXIge1xuICAgIG1hcmdpbi1ib3R0b206IDIwcHg7XG4gICAgZGlzcGxheTogZmxleDtcbiAgICBnYXA6IDEwcHg7XG4gIH1cblxuICAubW9kZWxzLWdyaWQge1xuICAgIGRpc3BsYXk6IGdyaWQ7XG4gICAgZ3JpZC10ZW1wbGF0ZS1jb2x1bW5zOiByZXBlYXQoYXV0by1maWxsLCBtaW5tYXgoMzUwcHgsIDFmcikpO1xuICAgIGdhcDogMjBweDtcbiAgICBtYXJnaW4tdG9wOiAyMHB4O1xuICB9XG5cbiAgLm1vZGVsLWNhcmQge1xuICAgIC5tb2RlbC1kZXNjcmlwdGlvbiB7XG4gICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjYpO1xuICAgICAgbWFyZ2luOiAxMHB4IDA7XG4gICAgICBtaW4taGVpZ2h0OiA0MHB4O1xuICAgIH1cblxuICAgIC5tb2RlbC1pbmZvIHtcbiAgICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgICAgbWFyZ2luOiAxNXB4IDA7XG5cbiAgICAgIC5maWxlLXNpemUge1xuICAgICAgICBjb2xvcjogcmdiYSgwLCAwLCAwLCAwLjU0KTtcbiAgICAgICAgZm9udC1zaXplOiAwLjllbTtcbiAgICAgIH1cbiAgICB9XG5cbiAgICAuZG93bmxvYWQtcHJvZ3Jlc3Mge1xuICAgICAgbWFyZ2luOiAxNXB4IDA7XG4gICAgICBwYWRkaW5nOiAxMHB4O1xuICAgICAgYmFja2dyb3VuZDogI2Y1ZjVmNTtcbiAgICAgIGJvcmRlci1yYWRpdXM6IDRweDtcblxuICAgICAgLnByb2dyZXNzLWluZm8ge1xuICAgICAgICBkaXNwbGF5OiBmbGV4O1xuICAgICAgICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XG4gICAgICAgIG1hcmdpbi1ib3R0b206IDhweDtcbiAgICAgICAgZm9udC1zaXplOiAwLjllbTtcbiAgICAgIH1cblxuICAgICAgLnByb2dyZXNzLWJ5dGVzIHtcbiAgICAgICAgbWFyZ2luLXRvcDogOHB4O1xuICAgICAgICBmb250LXNpemU6IDAuODVlbTtcbiAgICAgICAgY29sb3I6IHJnYmEoMCwgMCwgMCwgMC41NCk7XG4gICAgICAgIHRleHQtYWxpZ246IGNlbnRlcjtcbiAgICAgIH1cbiAgICB9XG5cbiAgICBtYXQtY2FyZC1hY3Rpb25zIHtcbiAgICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgICBnYXA6IDhweDtcbiAgICAgIHBhZGRpbmc6IDhweCAxNnB4O1xuICAgIH1cbiAgfVxuXG4gIC5uby1tb2RlbHMge1xuICAgIHRleHQtYWxpZ246IGNlbnRlcjtcbiAgICBwYWRkaW5nOiA0MHB4O1xuICAgIGNvbG9yOiByZ2JhKDAsIDAsIDAsIDAuNTQpO1xuXG4gICAgbWF0LWljb24ge1xuICAgICAgZm9udC1zaXplOiA0OHB4O1xuICAgICAgd2lkdGg6IDQ4cHg7XG4gICAgICBoZWlnaHQ6IDQ4cHg7XG4gICAgICBtYXJnaW4tYm90dG9tOiAxMHB4O1xuICAgIH1cbiAgfVxufSJdfQ== */"] });


/***/ }),

/***/ "IOcM":
/*!*******************************************!*\
  !*** ./src/app/ai-chat/ai-chat.module.ts ***!
  \*******************************************/
/*! exports provided: AiChatModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AiChatModule", function() { return AiChatModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/progress-bar */ "bv9b");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _ai_chat_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./ai-chat.component */ "oQrS");
/* harmony import */ var _model_manager_component__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./model-manager.component */ "H3KX");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/core */ "fXoL");



// Angular Material










// Components




const routes = [
    {
        path: '',
        component: _ai_chat_component__WEBPACK_IMPORTED_MODULE_13__["AiChatComponent"]
    }
];
class AiChatModule {
}
AiChatModule.ɵfac = function AiChatModule_Factory(t) { return new (t || AiChatModule)(); };
AiChatModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_15__["ɵɵdefineNgModule"]({ type: AiChatModule });
AiChatModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_15__["ɵɵdefineInjector"]({ imports: [[
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _angular_forms__WEBPACK_IMPORTED_MODULE_1__["FormsModule"],
            _angular_router__WEBPACK_IMPORTED_MODULE_2__["RouterModule"].forChild(routes),
            _angular_material_card__WEBPACK_IMPORTED_MODULE_3__["MatCardModule"],
            _angular_material_button__WEBPACK_IMPORTED_MODULE_4__["MatButtonModule"],
            _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__["MatIconModule"],
            _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_6__["MatToolbarModule"],
            _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_7__["MatProgressBarModule"],
            _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__["MatProgressSpinnerModule"],
            _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__["MatChipsModule"],
            _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatFormFieldModule"],
            _angular_material_input__WEBPACK_IMPORTED_MODULE_11__["MatInputModule"],
            _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_12__["MatTooltipModule"]
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_15__["ɵɵsetNgModuleScope"](AiChatModule, { declarations: [_ai_chat_component__WEBPACK_IMPORTED_MODULE_13__["AiChatComponent"],
        _model_manager_component__WEBPACK_IMPORTED_MODULE_14__["ModelManagerComponent"]], imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _angular_forms__WEBPACK_IMPORTED_MODULE_1__["FormsModule"], _angular_router__WEBPACK_IMPORTED_MODULE_2__["RouterModule"], _angular_material_card__WEBPACK_IMPORTED_MODULE_3__["MatCardModule"],
        _angular_material_button__WEBPACK_IMPORTED_MODULE_4__["MatButtonModule"],
        _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__["MatIconModule"],
        _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_6__["MatToolbarModule"],
        _angular_material_progress_bar__WEBPACK_IMPORTED_MODULE_7__["MatProgressBarModule"],
        _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_8__["MatProgressSpinnerModule"],
        _angular_material_chips__WEBPACK_IMPORTED_MODULE_9__["MatChipsModule"],
        _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatFormFieldModule"],
        _angular_material_input__WEBPACK_IMPORTED_MODULE_11__["MatInputModule"],
        _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_12__["MatTooltipModule"]], exports: [_ai_chat_component__WEBPACK_IMPORTED_MODULE_13__["AiChatComponent"],
        _model_manager_component__WEBPACK_IMPORTED_MODULE_14__["ModelManagerComponent"]] }); })();


/***/ }),

/***/ "oQrS":
/*!**********************************************!*\
  !*** ./src/app/ai-chat/ai-chat.component.ts ***!
  \**********************************************/
/*! exports provided: AiChatComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AiChatComponent", function() { return AiChatComponent; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "kU1M");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../services/ai-chat.service */ "oRpr");
/* harmony import */ var _angular_material_toolbar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/material/toolbar */ "/t3+");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");
/* harmony import */ var _angular_material_chips__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/chips */ "A5z7");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @angular/material/tooltip */ "Qu3c");
/* harmony import */ var _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/material/form-field */ "kmnG");
/* harmony import */ var _angular_material_input__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/material/input */ "qFsG");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/forms */ "3Pt+");
/* harmony import */ var _model_manager_component__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./model-manager.component */ "H3KX");
/* harmony import */ var _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/material/progress-spinner */ "Xa2L");















const _c0 = ["scrollContainer"];
const _c1 = ["messageInput"];
function AiChatComponent_mat_chip_9_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 20);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "check_circle");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate1"](" ", ctx_r0.currentModel || "Model Loaded", " ");
} }
function AiChatComponent_mat_chip_10_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-chip", 21);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "warning");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, " No Model ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function AiChatComponent_div_17_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 22);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](1, "app-model-manager");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function AiChatComponent_div_20_p_5_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "p", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, " Please load a model first using the settings button above ");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
function AiChatComponent_div_20_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](2, "chat_bubble_outline");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](3, "p");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](4, "Start a conversation with the AI assistant");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](5, AiChatComponent_div_20_p_5_Template, 2, 0, "p", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx_r4.isModelLoaded);
} }
function AiChatComponent_div_21_mat_progress_spinner_13_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](0, "mat-progress-spinner", 34);
} }
function AiChatComponent_div_21_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "div", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](5, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](6, "span", 30);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](7);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](8, "titlecase");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](9, "span", 31);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipe"](11, "date");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](12, "div", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](13, AiChatComponent_div_21_mat_progress_spinner_13_Template, 1, 0, "mat-progress-spinner", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} if (rf & 2) {
    const message_r9 = ctx.$implicit;
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngClass", ctx_r5.getMessageClass(message_r9));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](ctx_r5.getAvatarIcon(message_r9.role));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind1"](8, 6, message_r9.role));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵpipeBind2"](11, 8, message_r9.timestamp, "short"));
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("innerHTML", ctx_r5.formatMessageContent(message_r9.content), _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵsanitizeHtml"]);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", message_r9.isStreaming);
} }
function AiChatComponent_mat_hint_27_Template(rf, ctx) { if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "mat-hint");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](1, "Load a model to start chatting");
    _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
} }
class AiChatComponent {
    constructor(aiService) {
        this.aiService = aiService;
        this.messages = [];
        this.inputMessage = '';
        this.isModelLoaded = false;
        this.currentModel = null;
        this.showModelManager = false;
        this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_0__["Subject"]();
        this.shouldScrollToBottom = false;
    }
    ngOnInit() {
        console.log('[AiChatComponent] Component initialized');
        // Log container dimensions after view is rendered
        setTimeout(() => {
            var _a, _b;
            const container = document.querySelector('.ai-chat-container');
            if (container) {
                console.log('[AiChatComponent] Container dimensions:', {
                    height: container.offsetHeight,
                    width: container.offsetWidth,
                    clientHeight: container.clientHeight,
                    scrollHeight: container.scrollHeight,
                    computedStyle: window.getComputedStyle(container).height,
                    parentElement: (_a = container.parentElement) === null || _a === void 0 ? void 0 : _a.tagName,
                    parentHeight: (_b = container.parentElement) === null || _b === void 0 ? void 0 : _b.offsetHeight
                });
            }
            const inputContainer = document.querySelector('.chat-input-container');
            if (inputContainer) {
                console.log('[AiChatComponent] Input container:', {
                    height: inputContainer.offsetHeight,
                    offsetTop: inputContainer.offsetTop,
                    bottomPosition: inputContainer.offsetTop + inputContainer.offsetHeight,
                    visible: (inputContainer.offsetTop + inputContainer.offsetHeight) <= window.innerHeight,
                    windowHeight: window.innerHeight
                });
            }
            const messageInput = document.querySelector('textarea[matInput]');
            if (messageInput) {
                console.log('[AiChatComponent] Textarea found:', {
                    height: messageInput.offsetHeight,
                    offsetTop: messageInput.offsetTop,
                    disabled: messageInput.disabled,
                    placeholder: messageInput.placeholder
                });
            }
            else {
                console.log('[AiChatComponent] WARNING: Textarea NOT found!');
            }
            // Check parent container
            const routerOutlet = document.querySelector('router-outlet');
            if (routerOutlet && routerOutlet.parentElement) {
                console.log('[AiChatComponent] Router outlet parent:', {
                    tagName: routerOutlet.parentElement.tagName,
                    className: routerOutlet.parentElement.className,
                    height: routerOutlet.parentElement.offsetHeight
                });
            }
        }, 1000);
        // Subscribe to messages
        this.aiService.messages$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(messages => {
            this.messages = messages;
            this.shouldScrollToBottom = true;
        });
        // Subscribe to model status
        this.aiService.isModelLoaded$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(loaded => {
            console.log('[AiChatComponent] Model loaded status:', loaded);
            this.isModelLoaded = loaded;
        });
        // Subscribe to current model
        this.aiService.currentModel$
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["takeUntil"])(this.destroy$))
            .subscribe(model => {
            console.log('[AiChatComponent] Current model:', model);
            this.currentModel = model;
        });
    }
    ngAfterViewChecked() {
        if (this.shouldScrollToBottom) {
            this.scrollToBottom();
            this.shouldScrollToBottom = false;
        }
    }
    ngOnDestroy() {
        this.destroy$.next();
        this.destroy$.complete();
    }
    sendMessage() {
        if (!this.inputMessage.trim() || !this.isModelLoaded)
            return;
        this.aiService.sendMessage(this.inputMessage);
        this.inputMessage = '';
        this.focusInput();
    }
    handleKeyDown(event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.sendMessage();
        }
    }
    clearChat() {
        if (confirm('Clear all messages?')) {
            this.aiService.clearMessages();
        }
    }
    toggleModelManager() {
        this.showModelManager = !this.showModelManager;
    }
    scrollToBottom() {
        try {
            this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
        }
        catch (err) { }
    }
    focusInput() {
        setTimeout(() => {
            if (this.messageInput) {
                this.messageInput.nativeElement.focus();
            }
        }, 100);
    }
    formatMessageContent(content) {
        // Basic markdown-like formatting
        return content
            .replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>')
            .replace(/`([^`]+)`/g, '<code>$1</code>')
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>')
            .replace(/\n/g, '<br>');
    }
    getMessageClass(message) {
        return `message message-${message.role}`;
    }
    getAvatarIcon(role) {
        switch (role) {
            case 'user':
                return 'person';
            case 'assistant':
                return 'smart_toy';
            case 'system':
                return 'info';
            default:
                return 'chat_bubble';
        }
    }
}
AiChatComponent.ɵfac = function AiChatComponent_Factory(t) { return new (t || AiChatComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__["AiChatService"])); };
AiChatComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineComponent"]({ type: AiChatComponent, selectors: [["app-ai-chat"]], viewQuery: function AiChatComponent_Query(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵviewQuery"](_c0, 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵviewQuery"](_c1, 1);
    } if (rf & 2) {
        let _t;
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵloadQuery"]()) && (ctx.scrollContainer = _t.first);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵloadQuery"]()) && (ctx.messageInput = _t.first);
    } }, decls: 31, vars: 9, consts: [[1, "ai-chat-container"], ["color", "primary", 1, "chat-header"], [1, "header-title"], [1, "spacer"], [1, "model-status"], ["color", "accent", "selected", "", 4, "ngIf"], ["color", "warn", 4, "ngIf"], ["mat-icon-button", "", "matTooltip", "Model Manager", 3, "click"], ["mat-icon-button", "", "matTooltip", "Clear Chat", 3, "click"], ["class", "model-manager-panel", 4, "ngIf"], [1, "chat-messages"], ["scrollContainer", ""], ["class", "no-messages", 4, "ngIf"], [3, "ngClass", 4, "ngFor", "ngForOf"], [1, "chat-input-container"], ["appearance", "outline", 1, "chat-input"], ["matInput", "", "placeholder", "Type your message... (Shift+Enter for new line)", "rows", "3", 3, "ngModel", "disabled", "ngModelChange", "keydown"], ["messageInput", ""], [4, "ngIf"], ["mat-fab", "", "color", "primary", 1, "send-button", 3, "disabled", "click"], ["color", "accent", "selected", ""], ["color", "warn"], [1, "model-manager-panel"], [1, "no-messages"], ["class", "hint", 4, "ngIf"], [1, "hint"], [3, "ngClass"], [1, "message-avatar"], [1, "message-content"], [1, "message-header"], [1, "message-role"], [1, "message-time"], [1, "message-text", 3, "innerHTML"], ["diameter", "20", "mode", "indeterminate", 4, "ngIf"], ["diameter", "20", "mode", "indeterminate"]], template: function AiChatComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](1, "mat-toolbar", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](2, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](3, "smart_toy");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](4, "span", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](5, "AI Assistant");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelement"](6, "span", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](7, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](8, "mat-chip-list");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](9, AiChatComponent_mat_chip_9_Template, 4, 1, "mat-chip", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](10, AiChatComponent_mat_chip_10_Template, 4, 0, "mat-chip", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](11, "button", 7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function AiChatComponent_Template_button_click_11_listener() { return ctx.toggleModelManager(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](12, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](13, "settings");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](14, "button", 8);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function AiChatComponent_Template_button_click_14_listener() { return ctx.clearChat(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](15, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](16, "clear_all");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](17, AiChatComponent_div_17_Template, 2, 0, "div", 9);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](18, "div", 10, 11);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](20, AiChatComponent_div_20_Template, 6, 1, "div", 12);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](21, AiChatComponent_div_21_Template, 14, 11, "div", 13);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](22, "div", 14);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](23, "mat-form-field", 15);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](24, "textarea", 16, 17);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("ngModelChange", function AiChatComponent_Template_textarea_ngModelChange_24_listener($event) { return ctx.inputMessage = $event; })("keydown", function AiChatComponent_Template_textarea_keydown_24_listener($event) { return ctx.handleKeyDown($event); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](26, "      ");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtemplate"](27, AiChatComponent_mat_hint_27_Template, 2, 0, "mat-hint", 18);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](28, "button", 19);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵlistener"]("click", function AiChatComponent_Template_button_click_28_listener() { return ctx.sendMessage(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementStart"](29, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵtext"](30, "send");
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵelementEnd"]();
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](9);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.isModelLoaded);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isModelLoaded);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](7);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.showModelManager);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", ctx.messages.length === 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngForOf", ctx.messages);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngModel", ctx.inputMessage)("disabled", !ctx.isModelLoaded);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](3);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("ngIf", !ctx.isModelLoaded);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵadvance"](1);
        _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵproperty"]("disabled", !ctx.inputMessage.trim() || !ctx.isModelLoaded);
    } }, directives: [_angular_material_toolbar__WEBPACK_IMPORTED_MODULE_4__["MatToolbar"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_5__["MatIcon"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_6__["MatChipList"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgIf"], _angular_material_button__WEBPACK_IMPORTED_MODULE_8__["MatButton"], _angular_material_tooltip__WEBPACK_IMPORTED_MODULE_9__["MatTooltip"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgForOf"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatFormField"], _angular_material_input__WEBPACK_IMPORTED_MODULE_11__["MatInput"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["DefaultValueAccessor"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgControlStatus"], _angular_forms__WEBPACK_IMPORTED_MODULE_12__["NgModel"], _angular_material_chips__WEBPACK_IMPORTED_MODULE_6__["MatChip"], _model_manager_component__WEBPACK_IMPORTED_MODULE_13__["ModelManagerComponent"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["NgClass"], _angular_material_progress_spinner__WEBPACK_IMPORTED_MODULE_14__["MatProgressSpinner"], _angular_material_form_field__WEBPACK_IMPORTED_MODULE_10__["MatHint"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_7__["TitleCasePipe"], _angular_common__WEBPACK_IMPORTED_MODULE_7__["DatePipe"]], styles: [".ai-chat-container[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  height: calc(100% - 20px);\n  margin: 10px;\n  background: #f5f5f5;\n  border-radius: 8px;\n  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-header[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);\n  z-index: 10;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-header[_ngcontent-%COMP%]   .header-title[_ngcontent-%COMP%] {\n  margin-left: 10px;\n  font-size: 1.2em;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-header[_ngcontent-%COMP%]   .spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-header[_ngcontent-%COMP%]   .model-status[_ngcontent-%COMP%] {\n  margin-right: 10px;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .model-manager-panel[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  max-height: 50vh;\n  overflow-y: auto;\n  border-bottom: 1px solid #e0e0e0;\n  background: white;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 20px;\n  background: linear-gradient(to bottom, #f5f5f5, #ffffff);\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .no-messages[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-items: center;\n  justify-content: center;\n  height: 100%;\n  color: #999;\n  text-align: center;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .no-messages[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  font-size: 64px;\n  width: 64px;\n  height: 64px;\n  opacity: 0.3;\n  margin-bottom: 20px;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .no-messages[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 5px 0;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .no-messages[_ngcontent-%COMP%]   .hint[_ngcontent-%COMP%] {\n  font-size: 0.9em;\n  color: #ff9800;\n  margin-top: 10px;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%] {\n  display: flex;\n  margin-bottom: 20px;\n  animation: slideIn 0.3s ease-out;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-avatar[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 40px;\n  height: 40px;\n  border-radius: 50%;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  margin-right: 12px;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-avatar[_ngcontent-%COMP%]   mat-icon[_ngcontent-%COMP%] {\n  color: white;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%] {\n  flex: 1;\n  background: white;\n  border-radius: 8px;\n  padding: 12px 16px;\n  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);\n  max-width: 80%;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-header[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: space-between;\n  margin-bottom: 8px;\n  font-size: 0.85em;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-header[_ngcontent-%COMP%]   .message-role[_ngcontent-%COMP%] {\n  font-weight: 600;\n  text-transform: capitalize;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-header[_ngcontent-%COMP%]   .message-time[_ngcontent-%COMP%] {\n  color: #999;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-text[_ngcontent-%COMP%] {\n  line-height: 1.5;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-text[_ngcontent-%COMP%]   pre[_ngcontent-%COMP%] {\n  background: #f5f5f5;\n  padding: 10px;\n  border-radius: 4px;\n  overflow-x: auto;\n  margin: 10px 0;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   .message-text[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  background: #f5f5f5;\n  padding: 2px 4px;\n  border-radius: 3px;\n  font-family: \"Courier New\", monospace;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%]   mat-progress-spinner[_ngcontent-%COMP%] {\n  margin-top: 10px;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-user[_ngcontent-%COMP%] {\n  flex-direction: row-reverse;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-user[_ngcontent-%COMP%]   .message-avatar[_ngcontent-%COMP%] {\n  margin-right: 0;\n  margin-left: 12px;\n  background: #3f51b5;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-user[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%] {\n  background: #e3f2fd;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-assistant[_ngcontent-%COMP%]   .message-avatar[_ngcontent-%COMP%] {\n  background: #4caf50;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-system[_ngcontent-%COMP%]   .message-avatar[_ngcontent-%COMP%] {\n  background: #ff9800;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-messages[_ngcontent-%COMP%]   .message.message-system[_ngcontent-%COMP%]   .message-content[_ngcontent-%COMP%] {\n  background: #fff3e0;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-input-container[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  display: flex;\n  padding: 20px;\n  background: white;\n  border-top: 1px solid #e0e0e0;\n  gap: 10px;\n  align-items: flex-end;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-input-container[_ngcontent-%COMP%]   .chat-input[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-input-container[_ngcontent-%COMP%]   .chat-input[_ngcontent-%COMP%]     .mat-form-field-wrapper {\n  padding-bottom: 0;\n}\n.ai-chat-container[_ngcontent-%COMP%]   .chat-input-container[_ngcontent-%COMP%]   .send-button[_ngcontent-%COMP%] {\n  margin-bottom: 4px;\n}\n@keyframes slideIn {\n  from {\n    opacity: 0;\n    transform: translateY(10px);\n  }\n  to {\n    opacity: 1;\n    transform: translateY(0);\n  }\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uLy4uLy4uL2FpLWNoYXQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSx5QkFBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0Esd0NBQUE7QUFDRjtBQUNFO0VBQ0UsY0FBQTtFQUNBLHdDQUFBO0VBQ0EsV0FBQTtBQUNKO0FBQ0k7RUFDRSxpQkFBQTtFQUNBLGdCQUFBO0FBQ047QUFFSTtFQUNFLE9BQUE7QUFBTjtBQUdJO0VBQ0Usa0JBQUE7QUFETjtBQUtFO0VBQ0UsY0FBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxnQ0FBQTtFQUNBLGlCQUFBO0FBSEo7QUFNRTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSx3REFBQTtBQUpKO0FBTUk7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxrQkFBQTtBQUpOO0FBTU07RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7QUFKUjtBQU9NO0VBQ0UsYUFBQTtBQUxSO0FBUU07RUFDRSxnQkFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQU5SO0FBVUk7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQ0FBQTtBQVJOO0FBVU07RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0Esa0JBQUE7QUFSUjtBQVVRO0VBQ0UsWUFBQTtBQVJWO0FBWU07RUFDRSxPQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0Esd0NBQUE7RUFDQSxjQUFBO0FBVlI7QUFZUTtFQUNFLGFBQUE7RUFDQSw4QkFBQTtFQUNBLGtCQUFBO0VBQ0EsaUJBQUE7QUFWVjtBQVlVO0VBQ0UsZ0JBQUE7RUFDQSwwQkFBQTtBQVZaO0FBYVU7RUFDRSxXQUFBO0FBWFo7QUFlUTtFQUNFLGdCQUFBO0FBYlY7QUFlVTtFQUNFLG1CQUFBO0VBQ0EsYUFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0FBYlo7QUFnQlU7RUFDRSxtQkFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxxQ0FBQTtBQWRaO0FBa0JRO0VBQ0UsZ0JBQUE7QUFoQlY7QUFvQk07RUFDRSwyQkFBQTtBQWxCUjtBQW9CUTtFQUNFLGVBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0FBbEJWO0FBcUJRO0VBQ0UsbUJBQUE7QUFuQlY7QUF3QlE7RUFDRSxtQkFBQTtBQXRCVjtBQTJCUTtFQUNFLG1CQUFBO0FBekJWO0FBNEJRO0VBQ0UsbUJBQUE7QUExQlY7QUFnQ0U7RUFDRSxjQUFBO0VBQ0EsYUFBQTtFQUNBLGFBQUE7RUFDQSxpQkFBQTtFQUNBLDZCQUFBO0VBQ0EsU0FBQTtFQUNBLHFCQUFBO0FBOUJKO0FBZ0NJO0VBQ0UsT0FBQTtBQTlCTjtBQWdDTTtFQUNFLGlCQUFBO0FBOUJSO0FBa0NJO0VBQ0Usa0JBQUE7QUFoQ047QUFxQ0E7RUFDRTtJQUNFLFVBQUE7SUFDQSwyQkFBQTtFQWxDRjtFQW9DQTtJQUNFLFVBQUE7SUFDQSx3QkFBQTtFQWxDRjtBQUNGIiwiZmlsZSI6ImFpLWNoYXQuY29tcG9uZW50LnNjc3MiLCJzb3VyY2VzQ29udGVudCI6WyIuYWktY2hhdC1jb250YWluZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBoZWlnaHQ6IGNhbGMoMTAwJSAtIDIwcHgpOyAvLyBVc2EgcXVhc2kgdHV0dGEgbCdhbHRlenphIGRpc3BvbmliaWxlIGNvbiB1biBwaWNjb2xvIG1hcmdpbmVcbiAgbWFyZ2luOiAxMHB4O1xuICBiYWNrZ3JvdW5kOiAjZjVmNWY1O1xuICBib3JkZXItcmFkaXVzOiA4cHg7XG4gIGJveC1zaGFkb3c6IDAgMnB4IDRweCByZ2JhKDAsMCwwLDAuMSk7XG5cbiAgLmNoYXQtaGVhZGVyIHtcbiAgICBmbGV4LXNocmluazogMDtcbiAgICBib3gtc2hhZG93OiAwIDJweCA0cHggcmdiYSgwLDAsMCwwLjEpO1xuICAgIHotaW5kZXg6IDEwO1xuXG4gICAgLmhlYWRlci10aXRsZSB7XG4gICAgICBtYXJnaW4tbGVmdDogMTBweDtcbiAgICAgIGZvbnQtc2l6ZTogMS4yZW07XG4gICAgfVxuXG4gICAgLnNwYWNlciB7XG4gICAgICBmbGV4OiAxO1xuICAgIH1cblxuICAgIC5tb2RlbC1zdGF0dXMge1xuICAgICAgbWFyZ2luLXJpZ2h0OiAxMHB4O1xuICAgIH1cbiAgfVxuXG4gIC5tb2RlbC1tYW5hZ2VyLXBhbmVsIHtcbiAgICBmbGV4LXNocmluazogMDtcbiAgICBtYXgtaGVpZ2h0OiA1MHZoO1xuICAgIG92ZXJmbG93LXk6IGF1dG87XG4gICAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICNlMGUwZTA7XG4gICAgYmFja2dyb3VuZDogd2hpdGU7XG4gIH1cblxuICAuY2hhdC1tZXNzYWdlcyB7XG4gICAgZmxleDogMTtcbiAgICBvdmVyZmxvdy15OiBhdXRvO1xuICAgIHBhZGRpbmc6IDIwcHg7XG4gICAgYmFja2dyb3VuZDogbGluZWFyLWdyYWRpZW50KHRvIGJvdHRvbSwgI2Y1ZjVmNSwgI2ZmZmZmZik7XG5cbiAgICAubm8tbWVzc2FnZXMge1xuICAgICAgZGlzcGxheTogZmxleDtcbiAgICAgIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gICAgICBhbGlnbi1pdGVtczogY2VudGVyO1xuICAgICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gICAgICBoZWlnaHQ6IDEwMCU7XG4gICAgICBjb2xvcjogIzk5OTtcbiAgICAgIHRleHQtYWxpZ246IGNlbnRlcjtcblxuICAgICAgbWF0LWljb24ge1xuICAgICAgICBmb250LXNpemU6IDY0cHg7XG4gICAgICAgIHdpZHRoOiA2NHB4O1xuICAgICAgICBoZWlnaHQ6IDY0cHg7XG4gICAgICAgIG9wYWNpdHk6IDAuMztcbiAgICAgICAgbWFyZ2luLWJvdHRvbTogMjBweDtcbiAgICAgIH1cblxuICAgICAgcCB7XG4gICAgICAgIG1hcmdpbjogNXB4IDA7XG4gICAgICB9XG5cbiAgICAgIC5oaW50IHtcbiAgICAgICAgZm9udC1zaXplOiAwLjllbTtcbiAgICAgICAgY29sb3I6ICNmZjk4MDA7XG4gICAgICAgIG1hcmdpbi10b3A6IDEwcHg7XG4gICAgICB9XG4gICAgfVxuXG4gICAgLm1lc3NhZ2Uge1xuICAgICAgZGlzcGxheTogZmxleDtcbiAgICAgIG1hcmdpbi1ib3R0b206IDIwcHg7XG4gICAgICBhbmltYXRpb246IHNsaWRlSW4gMC4zcyBlYXNlLW91dDtcblxuICAgICAgLm1lc3NhZ2UtYXZhdGFyIHtcbiAgICAgICAgZmxleC1zaHJpbms6IDA7XG4gICAgICAgIHdpZHRoOiA0MHB4O1xuICAgICAgICBoZWlnaHQ6IDQwcHg7XG4gICAgICAgIGJvcmRlci1yYWRpdXM6IDUwJTtcbiAgICAgICAgZGlzcGxheTogZmxleDtcbiAgICAgICAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgICAgICAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gICAgICAgIG1hcmdpbi1yaWdodDogMTJweDtcblxuICAgICAgICBtYXQtaWNvbiB7XG4gICAgICAgICAgY29sb3I6IHdoaXRlO1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgIC5tZXNzYWdlLWNvbnRlbnQge1xuICAgICAgICBmbGV4OiAxO1xuICAgICAgICBiYWNrZ3JvdW5kOiB3aGl0ZTtcbiAgICAgICAgYm9yZGVyLXJhZGl1czogOHB4O1xuICAgICAgICBwYWRkaW5nOiAxMnB4IDE2cHg7XG4gICAgICAgIGJveC1zaGFkb3c6IDAgMXB4IDNweCByZ2JhKDAsMCwwLDAuMSk7XG4gICAgICAgIG1heC13aWR0aDogODAlO1xuXG4gICAgICAgIC5tZXNzYWdlLWhlYWRlciB7XG4gICAgICAgICAgZGlzcGxheTogZmxleDtcbiAgICAgICAgICBqdXN0aWZ5LWNvbnRlbnQ6IHNwYWNlLWJldHdlZW47XG4gICAgICAgICAgbWFyZ2luLWJvdHRvbTogOHB4O1xuICAgICAgICAgIGZvbnQtc2l6ZTogMC44NWVtO1xuXG4gICAgICAgICAgLm1lc3NhZ2Utcm9sZSB7XG4gICAgICAgICAgICBmb250LXdlaWdodDogNjAwO1xuICAgICAgICAgICAgdGV4dC10cmFuc2Zvcm06IGNhcGl0YWxpemU7XG4gICAgICAgICAgfVxuXG4gICAgICAgICAgLm1lc3NhZ2UtdGltZSB7XG4gICAgICAgICAgICBjb2xvcjogIzk5OTtcbiAgICAgICAgICB9XG4gICAgICAgIH1cblxuICAgICAgICAubWVzc2FnZS10ZXh0IHtcbiAgICAgICAgICBsaW5lLWhlaWdodDogMS41O1xuXG4gICAgICAgICAgcHJlIHtcbiAgICAgICAgICAgIGJhY2tncm91bmQ6ICNmNWY1ZjU7XG4gICAgICAgICAgICBwYWRkaW5nOiAxMHB4O1xuICAgICAgICAgICAgYm9yZGVyLXJhZGl1czogNHB4O1xuICAgICAgICAgICAgb3ZlcmZsb3cteDogYXV0bztcbiAgICAgICAgICAgIG1hcmdpbjogMTBweCAwO1xuICAgICAgICAgIH1cblxuICAgICAgICAgIGNvZGUge1xuICAgICAgICAgICAgYmFja2dyb3VuZDogI2Y1ZjVmNTtcbiAgICAgICAgICAgIHBhZGRpbmc6IDJweCA0cHg7XG4gICAgICAgICAgICBib3JkZXItcmFkaXVzOiAzcHg7XG4gICAgICAgICAgICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xuICAgICAgICAgIH1cbiAgICAgICAgfVxuXG4gICAgICAgIG1hdC1wcm9ncmVzcy1zcGlubmVyIHtcbiAgICAgICAgICBtYXJnaW4tdG9wOiAxMHB4O1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgICYubWVzc2FnZS11c2VyIHtcbiAgICAgICAgZmxleC1kaXJlY3Rpb246IHJvdy1yZXZlcnNlO1xuXG4gICAgICAgIC5tZXNzYWdlLWF2YXRhciB7XG4gICAgICAgICAgbWFyZ2luLXJpZ2h0OiAwO1xuICAgICAgICAgIG1hcmdpbi1sZWZ0OiAxMnB4O1xuICAgICAgICAgIGJhY2tncm91bmQ6ICMzZjUxYjU7XG4gICAgICAgIH1cblxuICAgICAgICAubWVzc2FnZS1jb250ZW50IHtcbiAgICAgICAgICBiYWNrZ3JvdW5kOiAjZTNmMmZkO1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgICYubWVzc2FnZS1hc3Npc3RhbnQge1xuICAgICAgICAubWVzc2FnZS1hdmF0YXIge1xuICAgICAgICAgIGJhY2tncm91bmQ6ICM0Y2FmNTA7XG4gICAgICAgIH1cbiAgICAgIH1cblxuICAgICAgJi5tZXNzYWdlLXN5c3RlbSB7XG4gICAgICAgIC5tZXNzYWdlLWF2YXRhciB7XG4gICAgICAgICAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgICAgICAgfVxuXG4gICAgICAgIC5tZXNzYWdlLWNvbnRlbnQge1xuICAgICAgICAgIGJhY2tncm91bmQ6ICNmZmYzZTA7XG4gICAgICAgIH1cbiAgICAgIH1cbiAgICB9XG4gIH1cblxuICAuY2hhdC1pbnB1dC1jb250YWluZXIge1xuICAgIGZsZXgtc2hyaW5rOiAwO1xuICAgIGRpc3BsYXk6IGZsZXg7XG4gICAgcGFkZGluZzogMjBweDtcbiAgICBiYWNrZ3JvdW5kOiB3aGl0ZTtcbiAgICBib3JkZXItdG9wOiAxcHggc29saWQgI2UwZTBlMDtcbiAgICBnYXA6IDEwcHg7XG4gICAgYWxpZ24taXRlbXM6IGZsZXgtZW5kO1xuXG4gICAgLmNoYXQtaW5wdXQge1xuICAgICAgZmxleDogMTtcblxuICAgICAgOjpuZy1kZWVwIC5tYXQtZm9ybS1maWVsZC13cmFwcGVyIHtcbiAgICAgICAgcGFkZGluZy1ib3R0b206IDA7XG4gICAgICB9XG4gICAgfVxuXG4gICAgLnNlbmQtYnV0dG9uIHtcbiAgICAgIG1hcmdpbi1ib3R0b206IDRweDtcbiAgICB9XG4gIH1cbn1cblxuQGtleWZyYW1lcyBzbGlkZUluIHtcbiAgZnJvbSB7XG4gICAgb3BhY2l0eTogMDtcbiAgICB0cmFuc2Zvcm06IHRyYW5zbGF0ZVkoMTBweCk7XG4gIH1cbiAgdG8ge1xuICAgIG9wYWNpdHk6IDE7XG4gICAgdHJhbnNmb3JtOiB0cmFuc2xhdGVZKDApO1xuICB9XG59Il19 */"] });


/***/ }),

/***/ "oRpr":
/*!*********************************************!*\
  !*** ./src/app/services/ai-chat.service.ts ***!
  \*********************************************/
/*! exports provided: AiChatService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "AiChatService", function() { return AiChatService; });
/* harmony import */ var tslib__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! tslib */ "mrSG");
/* harmony import */ var _microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @microsoft/signalr */ "6HpG");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common/http */ "tk/3");





class AiChatService {
    constructor(http) {
        this.http = http;
        this.baseUrl = '/api/AiModels';
        this._messages$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__["BehaviorSubject"]([]);
        this.messages$ = this._messages$.asObservable();
        this._downloadProgress$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__["Subject"]();
        this.downloadProgress$ = this._downloadProgress$.asObservable();
        this._currentModel$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__["BehaviorSubject"](null);
        this.currentModel$ = this._currentModel$.asObservable();
        this._isModelLoaded$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__["BehaviorSubject"](false);
        this.isModelLoaded$ = this._isModelLoaded$.asObservable();
        this._streamingMessage$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__["Subject"]();
        this.streamingMessage$ = this._streamingMessage$.asObservable();
        this.currentStreamingMessageId = null;
        this.initializeSignalR();
    }
    initializeSignalR() {
        this.hubConnection = new _microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__["HubConnectionBuilder"]()
            .withUrl('/signalr/aichat')
            .configureLogging(_microsoft_signalr__WEBPACK_IMPORTED_MODULE_1__["LogLevel"].Information)
            .withAutomaticReconnect()
            .build();
        // Setup event handlers
        this.hubConnection.on('DownloadProgress', (progress) => {
            this._downloadProgress$.next(progress);
        });
        this.hubConnection.on('DownloadComplete', (modelId) => {
            console.log(`Model ${modelId} download complete`);
            this.getAvailableModels().subscribe(); // Refresh model list
        });
        this.hubConnection.on('DownloadError', (modelId, error) => {
            console.error(`Model ${modelId} download error:`, error);
        });
        this.hubConnection.on('ModelLoaded', (modelName) => {
            console.log('[AiChatService] Received ModelLoaded event:', modelName);
            this._currentModel$.next(modelName);
            this._isModelLoaded$.next(true);
        });
        this.hubConnection.on('ModelLoading', (modelName) => {
            console.log('[AiChatService] Received ModelLoading event:', modelName);
        });
        this.hubConnection.on('ModelLoadError', (error) => {
            console.error('[AiChatService] Received ModelLoadError event:', error);
            this._isModelLoaded$.next(false);
        });
        this.hubConnection.on('ReceiveMessage', (role, content) => {
            this.addMessage(role, content);
        });
        this.hubConnection.on('ReceiveStreamChunk', (chunk) => {
            this._streamingMessage$.next(chunk);
            this.appendToStreamingMessage(chunk);
        });
        this.hubConnection.on('StreamComplete', () => {
            this.finalizeStreamingMessage();
        });
        this.hubConnection.on('ReceiveError', (error) => {
            console.error('Chat error:', error);
            this.addMessage('system', `Error: ${error}`);
        });
        // Start connection
        this.startConnection();
    }
    startConnection() {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            try {
                yield this.hubConnection.start();
                console.log('SignalR connection established');
                yield this.getModelStatus();
            }
            catch (err) {
                console.error('Error establishing SignalR connection:', err);
                setTimeout(() => this.startConnection(), 5000);
            }
        });
    }
    // Model Management
    getAvailableModels() {
        return this.http.get(`${this.baseUrl}/available`);
    }
    getInstalledModels() {
        return this.http.get(`${this.baseUrl}/installed`);
    }
    downloadModel(modelId) {
        return this.http.post(`${this.baseUrl}/download/${modelId}`, {});
    }
    deleteModel(modelId) {
        return this.http.delete(`${this.baseUrl}/${modelId}`);
    }
    loadModel(modelId) {
        console.log('[AiChatService] loadModel() called with modelId:', modelId);
        console.log('[AiChatService] HubConnection state:', this.hubConnection.state);
        if (this.hubConnection.state === 'Connected') {
            console.log('[AiChatService] Using SignalR to load model');
            return new rxjs__WEBPACK_IMPORTED_MODULE_2__["Observable"](observer => {
                this.hubConnection.invoke('LoadModel', modelId)
                    .then((response) => {
                    console.log('[AiChatService] SignalR LoadModel success, response:', response);
                    observer.next(response);
                    observer.complete();
                })
                    .catch(err => {
                    console.error('[AiChatService] SignalR LoadModel error:', err);
                    observer.error(err);
                });
            });
        }
        console.log('[AiChatService] Using HTTP POST to load model');
        return this.http.post(`${this.baseUrl}/load/${modelId}`, {});
    }
    getModelStatus() {
        return Object(tslib__WEBPACK_IMPORTED_MODULE_0__["__awaiter"])(this, void 0, void 0, function* () {
            if (this.hubConnection.state === 'Connected') {
                try {
                    const status = yield this.hubConnection.invoke('GetModelStatus');
                    this._isModelLoaded$.next(status.isModelLoaded);
                    this._currentModel$.next(status.currentModel);
                }
                catch (err) {
                    console.error('Error getting model status:', err);
                }
            }
        });
    }
    // Chat functionality
    sendMessage(message) {
        if (!message.trim())
            return;
        // Add user message
        this.addMessage('user', message);
        // Create placeholder for assistant response
        const assistantMessageId = this.generateMessageId();
        this.currentStreamingMessageId = assistantMessageId;
        this.addMessage('assistant', '', assistantMessageId, true);
        // Send to server
        if (this.hubConnection.state === 'Connected') {
            this.hubConnection.invoke('SendMessage', message)
                .catch(err => {
                console.error('Error sending message:', err);
                this.addMessage('system', `Failed to send message: ${err}`);
            });
        }
    }
    addMessage(role, content, id, isStreaming) {
        const messages = this._messages$.value;
        const newMessage = {
            id: id || this.generateMessageId(),
            role,
            content,
            timestamp: new Date(),
            isStreaming
        };
        this._messages$.next([...messages, newMessage]);
    }
    appendToStreamingMessage(chunk) {
        if (!this.currentStreamingMessageId)
            return;
        const messages = this._messages$.value;
        const messageIndex = messages.findIndex(m => m.id === this.currentStreamingMessageId);
        if (messageIndex !== -1) {
            messages[messageIndex].content += chunk;
            this._messages$.next([...messages]);
        }
    }
    finalizeStreamingMessage() {
        if (!this.currentStreamingMessageId)
            return;
        const messages = this._messages$.value;
        const messageIndex = messages.findIndex(m => m.id === this.currentStreamingMessageId);
        if (messageIndex !== -1) {
            messages[messageIndex].isStreaming = false;
            this._messages$.next([...messages]);
        }
        this.currentStreamingMessageId = null;
    }
    clearMessages() {
        this._messages$.next([]);
    }
    generateMessageId() {
        return `msg-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }
    ngOnDestroy() {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }
}
AiChatService.ɵfac = function AiChatService_Factory(t) { return new (t || AiChatService)(_angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_4__["HttpClient"])); };
AiChatService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_3__["ɵɵdefineInjectable"]({ token: AiChatService, factory: AiChatService.ɵfac, providedIn: 'root' });


/***/ })

}]);
//# sourceMappingURL=ai-chat-ai-chat-module.js.map