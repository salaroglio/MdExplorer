"use strict";
(self["webpackChunkclient2"] = self["webpackChunkclient2"] || []).push([["src_app_promptlab_promptlab_module_ts"],{

/***/ 4060:
/*!*********************************************************************************************!*\
  !*** ./src/app/promptlab/components/promptlab-agent-card/promptlab-agent-card.component.ts ***!
  \*********************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabAgentCardComponent": () => (/* binding */ PromptLabAgentCardComponent)
/* harmony export */ });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @ngx-translate/core */ 8699);





function PromptLabAgentCardComponent_div_10_div_22_span_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "span", 15);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const tool_r3 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](tool_r3);
  }
}
function PromptLabAgentCardComponent_div_10_div_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, PromptLabAgentCardComponent_div_10_div_22_span_1_Template, 2, 1, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx_r1.agentDefinition.tools);
  }
}
function PromptLabAgentCardComponent_div_10_Template(rf, ctx) {
  if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 7)(1, "div", 8)(2, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_10_Template_textarea_blur_5_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r4.onFieldBlur("identity", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "div", 8)(7, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](10, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_10_Template_textarea_blur_10_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r6.onFieldBlur("objectives", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "div", 8)(12, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](15, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_10_Template_textarea_blur_15_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r7.onFieldBlur("rules", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "div", 8)(17, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](18);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](19, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](20, "textarea", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_10_Template_textarea_blur_20_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r8.onToolsBlur($event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](21, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](22, PromptLabAgentCardComponent_div_10_div_22_Template, 2, 1, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](4, 10, "PROMPTLAB_AGENT.IDENTITY"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.identity);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](9, 12, "PROMPTLAB_AGENT.OBJECTIVES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.objectives);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](14, 14, "PROMPTLAB_AGENT.RULES"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.rules);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](19, 16, "PROMPTLAB_AGENT.TOOLS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.tools.join(", "))("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](21, 18, "PROMPTLAB_AGENT.TOOLS_PLACEHOLDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx_r0.agentDefinition.tools.length > 0);
  }
}
class PromptLabAgentCardComponent {
  constructor() {
    this.agentDefinition = {
      identity: '',
      objectives: '',
      rules: '',
      tools: []
    };
    this.agentDefinitionChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__.EventEmitter();
    this.collapsed = false;
  }
  toggleCollapse() {
    this.collapsed = !this.collapsed;
  }
  onFieldBlur(field, value) {
    const updated = {
      ...this.agentDefinition,
      [field]: value
    };
    this.agentDefinitionChange.emit(updated);
  }
  onToolsBlur(value) {
    const tools = value.split(',').map(t => t.trim()).filter(t => t.length > 0);
    const updated = {
      ...this.agentDefinition,
      tools
    };
    this.agentDefinitionChange.emit(updated);
  }
  static {
    this.ɵfac = function PromptLabAgentCardComponent_Factory(t) {
      return new (t || PromptLabAgentCardComponent)();
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: PromptLabAgentCardComponent,
      selectors: [["app-promptlab-agent-card"]],
      inputs: {
        agentDefinition: "agentDefinition"
      },
      outputs: {
        agentDefinitionChange: "agentDefinitionChange"
      },
      decls: 11,
      vars: 6,
      consts: [[1, "agent-card"], [1, "agent-card-header"], [1, "agent-icon"], [1, "agent-title"], [1, "spacer"], [1, "collapse-btn", 3, "click"], ["class", "agent-card-body", 4, "ngIf"], [1, "agent-card-body"], [1, "agent-field"], [1, "agent-label"], ["rows", "2", 1, "agent-value", 3, "ngModel", "blur"], ["rows", "2", 1, "agent-value", 3, "ngModel", "placeholder", "blur"], ["class", "agent-tools", 4, "ngIf"], [1, "agent-tools"], ["class", "agent-tool", 4, "ngFor", "ngForOf"], [1, "agent-tool"]],
      template: function PromptLabAgentCardComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "span", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "\u2699");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](6, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](7, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "button", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function PromptLabAgentCardComponent_Template_button_click_8_listener() {
            return ctx.toggleCollapse();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9, "\u25BC");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](10, PromptLabAgentCardComponent_div_10_Template, 23, 20, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵclassProp"]("collapsed", ctx.collapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](6, 4, "PROMPTLAB_AGENT.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx.collapsed);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_1__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_1__.NgIf, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_3__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n  padding: 0 16px;\n  flex-shrink: 0;\n}\n\n.agent-card[_ngcontent-%COMP%] {\n  background: #1a1d21;\n  border: 2px solid #c792ea;\n  border-radius: 8px;\n  margin: 16px 0;\n  overflow: hidden;\n}\n\n.agent-card-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 10px 16px;\n  background: #252526;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.agent-icon[_ngcontent-%COMP%] {\n  color: #c792ea;\n  font-size: 16px;\n}\n\n.agent-title[_ngcontent-%COMP%] {\n  color: #fff;\n  font-size: 13px;\n  font-weight: 600;\n}\n\n.spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.collapse-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 14px;\n  cursor: pointer;\n  padding: 2px 6px;\n  border-radius: 3px;\n  transition: transform 0.2s;\n}\n.collapse-btn[_ngcontent-%COMP%]:hover {\n  color: #fff;\n  background: #3c3c3c;\n}\n\n.agent-card.collapsed[_ngcontent-%COMP%]   .collapse-btn[_ngcontent-%COMP%] {\n  transform: rotate(-90deg);\n}\n\n.agent-card-body[_ngcontent-%COMP%] {\n  padding: 16px;\n}\n\n.agent-field[_ngcontent-%COMP%] {\n  margin-bottom: 12px;\n}\n.agent-field[_ngcontent-%COMP%]:last-child {\n  margin-bottom: 0;\n}\n\n.agent-label[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #c792ea;\n  margin-bottom: 4px;\n}\n\n.agent-value[_ngcontent-%COMP%] {\n  display: block;\n  width: 100%;\n  font-size: 12px;\n  color: #e0e0e0;\n  line-height: 1.6;\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 4px;\n  padding: 8px 12px;\n  outline: none;\n  resize: vertical;\n  font-family: \"Segoe UI\", sans-serif;\n}\n.agent-value[_ngcontent-%COMP%]:focus {\n  box-shadow: 0 0 0 1px #c792ea inset;\n}\n.agent-value[_ngcontent-%COMP%]::placeholder {\n  color: #555;\n}\n\n.agent-tools[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 6px;\n  flex-wrap: wrap;\n  margin-top: 8px;\n}\n\n.agent-tool[_ngcontent-%COMP%] {\n  background: #2d1f3d;\n  border: 1px solid #7c4dff;\n  color: #b39ddb;\n  font-size: 10px;\n  padding: 3px 8px;\n  border-radius: 10px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiLWFnZW50LWNhcmQvcHJvbXB0bGFiLWFnZW50LWNhcmQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUE7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7QUFDRjs7QUFFQTtFQUNFLG1CQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxnQ0FBQTtBQUNGOztBQUVBO0VBQ0UsY0FBQTtFQUNBLGVBQUE7QUFDRjs7QUFFQTtFQUNFLFdBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7QUFDRjs7QUFFQTtFQUNFLGdCQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSwwQkFBQTtBQUNGO0FBQ0U7RUFDRSxXQUFBO0VBQ0EsbUJBQUE7QUFDSjs7QUFHQTtFQUNFLHlCQUFBO0FBQUY7O0FBR0E7RUFDRSxhQUFBO0FBQUY7O0FBR0E7RUFDRSxtQkFBQTtBQUFGO0FBRUU7RUFDRSxnQkFBQTtBQUFKOztBQUlBO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0VBQ0EsbUJBQUE7RUFDQSxjQUFBO0VBQ0Esa0JBQUE7QUFERjs7QUFJQTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLGlCQUFBO0VBQ0EsYUFBQTtFQUNBLGdCQUFBO0VBQ0EsbUNBQUE7QUFERjtBQUdFO0VBQ0UsbUNBQUE7QUFESjtBQUlFO0VBQ0UsV0FBQTtBQUZKOztBQU1BO0VBQ0UsYUFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtBQUhGOztBQU1BO0VBQ0UsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLGNBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxtQkFBQTtBQUhGIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xuICBkaXNwbGF5OiBibG9jaztcbiAgcGFkZGluZzogMCAxNnB4O1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmFnZW50LWNhcmQge1xuICBiYWNrZ3JvdW5kOiAjMWExZDIxO1xuICBib3JkZXI6IDJweCBzb2xpZCAjYzc5MmVhO1xuICBib3JkZXItcmFkaXVzOiA4cHg7XG4gIG1hcmdpbjogMTZweCAwO1xuICBvdmVyZmxvdzogaGlkZGVuO1xufVxuXG4uYWdlbnQtY2FyZC1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTBweCAxNnB4O1xuICBiYWNrZ3JvdW5kOiAjMjUyNTI2O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbn1cblxuLmFnZW50LWljb24ge1xuICBjb2xvcjogI2M3OTJlYTtcbiAgZm9udC1zaXplOiAxNnB4O1xufVxuXG4uYWdlbnQtdGl0bGUge1xuICBjb2xvcjogI2ZmZjtcbiAgZm9udC1zaXplOiAxM3B4O1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4uc3BhY2VyIHtcbiAgZmxleDogMTtcbn1cblxuLmNvbGxhcHNlLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogbm9uZTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiAycHggNnB4O1xuICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjJzO1xuXG4gICY6aG92ZXIge1xuICAgIGNvbG9yOiAjZmZmO1xuICAgIGJhY2tncm91bmQ6ICMzYzNjM2M7XG4gIH1cbn1cblxuLmFnZW50LWNhcmQuY29sbGFwc2VkIC5jb2xsYXBzZS1idG4ge1xuICB0cmFuc2Zvcm06IHJvdGF0ZSgtOTBkZWcpO1xufVxuXG4uYWdlbnQtY2FyZC1ib2R5IHtcbiAgcGFkZGluZzogMTZweDtcbn1cblxuLmFnZW50LWZpZWxkIHtcbiAgbWFyZ2luLWJvdHRvbTogMTJweDtcblxuICAmOmxhc3QtY2hpbGQge1xuICAgIG1hcmdpbi1ib3R0b206IDA7XG4gIH1cbn1cblxuLmFnZW50LWxhYmVsIHtcbiAgZm9udC1zaXplOiAxMHB4O1xuICB0ZXh0LXRyYW5zZm9ybTogdXBwZXJjYXNlO1xuICBsZXR0ZXItc3BhY2luZzogMXB4O1xuICBjb2xvcjogI2M3OTJlYTtcbiAgbWFyZ2luLWJvdHRvbTogNHB4O1xufVxuXG4uYWdlbnQtdmFsdWUge1xuICBkaXNwbGF5OiBibG9jaztcbiAgd2lkdGg6IDEwMCU7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgY29sb3I6ICNlMGUwZTA7XG4gIGxpbmUtaGVpZ2h0OiAxLjY7XG4gIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgcGFkZGluZzogOHB4IDEycHg7XG4gIG91dGxpbmU6IG5vbmU7XG4gIHJlc2l6ZTogdmVydGljYWw7XG4gIGZvbnQtZmFtaWx5OiAnU2Vnb2UgVUknLCBzYW5zLXNlcmlmO1xuXG4gICY6Zm9jdXMge1xuICAgIGJveC1zaGFkb3c6IDAgMCAwIDFweCAjYzc5MmVhIGluc2V0O1xuICB9XG5cbiAgJjo6cGxhY2Vob2xkZXIge1xuICAgIGNvbG9yOiAjNTU1O1xuICB9XG59XG5cbi5hZ2VudC10b29scyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGdhcDogNnB4O1xuICBmbGV4LXdyYXA6IHdyYXA7XG4gIG1hcmdpbi10b3A6IDhweDtcbn1cblxuLmFnZW50LXRvb2wge1xuICBiYWNrZ3JvdW5kOiAjMmQxZjNkO1xuICBib3JkZXI6IDFweCBzb2xpZCAjN2M0ZGZmO1xuICBjb2xvcjogI2IzOWRkYjtcbiAgZm9udC1zaXplOiAxMHB4O1xuICBwYWRkaW5nOiAzcHggOHB4O1xuICBib3JkZXItcmFkaXVzOiAxMHB4O1xufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"],
      changeDetection: 0
    });
  }
}

/***/ }),

/***/ 748:
/*!*********************************************************************************!*\
  !*** ./src/app/promptlab/components/promptlab-card/promptlab-card.component.ts ***!
  \*********************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabCardComponent": () => (/* binding */ PromptLabCardComponent)
/* harmony export */ });
/* harmony import */ var C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _models_promptlab_models__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../models/promptlab.models */ 241);
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/platform-browser */ 4497);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _services_promptlab_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/promptlab.service */ 3819);
/* harmony import */ var _services_promptlab_distillation_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../services/promptlab-distillation.service */ 6479);
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../services/ai-chat.service */ 9109);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! @angular/forms */ 2508);


















const _c0 = ["messagesContainer"];
function PromptLabCardComponent_div_12_ng_container_1_span_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r25 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 43)(1, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "input", 44, 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("input", function PromptLabCardComponent_div_12_ng_container_1_span_1_Template_input_input_3_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r25);
      const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r24.editingParamValue = $event.target.value);
    })("keydown", function PromptLabCardComponent_div_12_ng_container_1_span_1_Template_input_keydown_3_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r25);
      const param_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
      const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r26.onParamEditKeydown($event, param_r19));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_div_12_ng_container_1_span_1_Template_button_click_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r25);
      const param_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
      const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r28.confirmParamEdit(param_r19));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6, "\u2713");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const param_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    const ctx_r21 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r21.getParamIcon(param_r19.type));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("value", ctx_r21.editingParamValue);
  }
}
function PromptLabCardComponent_div_12_ng_container_1_span_2_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1, " = ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](2, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const param_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](param_r19.value);
  }
}
function PromptLabCardComponent_div_12_ng_container_1_span_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r35 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_div_12_ng_container_1_span_2_Template_span_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r35);
      const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      const param_r19 = ctx_r34.$implicit;
      const i_r20 = ctx_r34.index;
      const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r33.onParameterClick(param_r19, i_r20));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, PromptLabCardComponent_div_12_ng_container_1_span_2_span_2_Template, 4, 1, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const param_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]().$implicit;
    const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngClass", ctx_r22.getParamDisplayClass(param_r19));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate2"](" ", ctx_r22.getParamIcon(param_r19.type), " ", param_r19.name, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", param_r19.value);
  }
}
function PromptLabCardComponent_div_12_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, PromptLabCardComponent_div_12_ng_container_1_span_1_Template, 7, 2, "span", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](2, PromptLabCardComponent_div_12_ng_container_1_span_2_Template, 3, 4, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const i_r20 = ctx.index;
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r18.editingParamIndex === i_r20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r18.editingParamIndex !== i_r20);
  }
}
function PromptLabCardComponent_div_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](1, PromptLabCardComponent_div_12_ng_container_1_Template, 3, 2, "ng-container", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r0.card.parameters);
  }
}
function PromptLabCardComponent_span_22_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "span", 48);
  }
}
function PromptLabCardComponent_ng_container_23_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.GENERATING"));
  }
}
function PromptLabCardComponent_ng_container_24_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.SEQUENCE"));
  }
}
function PromptLabCardComponent_span_26_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "span", 48);
  }
}
function PromptLabCardComponent_ng_container_27_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.GENERATING"));
  }
}
function PromptLabCardComponent_ng_container_28_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.WORKFLOW"));
  }
}
function PromptLabCardComponent_div_29_div_26_div_5_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 59)(1, "span", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](3, "span", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "=");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const key_r39 = ctx.$implicit;
    const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](key_r39);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r38.card.lastRun.resolvedParameters[key_r39]);
  }
}
function PromptLabCardComponent_div_29_div_26_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 50)(1, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "div", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](5, PromptLabCardComponent_div_29_div_26_div_5_Template, 7, 2, "div", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 2, "PROMPTLAB_CARD.RESOLVED_PARAMS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx_r37.getRunParamKeys());
  }
}
function PromptLabCardComponent_div_29_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 49)(1, "div", 50)(2, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "div", 52)(6, "span", 53)(7, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](11, "span", 53)(12, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](16, "span", 53)(17, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](18);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](19, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](21, "span", 53)(22, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](23);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](24, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](25);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](26, PromptLabCardComponent_div_29_div_26_Template, 6, 4, "div", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](27, "div", 50)(28, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](29);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](30, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](31, "div", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](32);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](33, "div", 50)(34, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](35);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](36, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](37, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](38);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 14, "PROMPTLAB_CARD.EXEC_METADATA"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](9, 16, "PROMPTLAB_CARD.DATE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r7.formatRunDate(ctx_r7.card.lastRun.executedAt), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](14, 18, "PROMPTLAB_CARD.PROVIDER"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r7.card.lastRun.provider, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](19, 20, "PROMPTLAB_CARD.MODEL"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r7.card.lastRun.model, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](24, 22, "PROMPTLAB_CARD.DURATION"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r7.formatDuration(ctx_r7.card.lastRun.duration), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r7.getRunParamKeys().length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](30, 24, "PROMPTLAB_CARD.PROMPT_SENT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r7.card.lastRun.promptSent);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](36, 26, "PROMPTLAB_CARD.OUTPUT_PRODUCED"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r7.card.lastRun.output);
  }
}
function PromptLabCardComponent_div_41_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 63)(1, "div", 64);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "div", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const message_r40 = ctx.$implicit;
    const ctx_r9 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngClass", message_r40.role);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", message_r40.role === "user" ? _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 4, "PROMPTLAB_CARD.USER") : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 6, "PROMPTLAB_CARD.ASSISTANT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r9.formatTime(message_r40.timestamp));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](message_r40.content);
  }
}
function PromptLabCardComponent_div_42_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 63)(1, "div", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "div", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](6, "span", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r10 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 2, "PROMPTLAB_CARD.ASSISTANT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r10.streamingContent);
  }
}
function PromptLabCardComponent_div_43_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.NO_MESSAGES"), " ");
  }
}
function PromptLabCardComponent_div_44_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 71);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](1, "span", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 1, "PROMPTLAB_CARD.DISTILLING"), " ");
  }
}
function PromptLabCardComponent_span_57_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"]("\u25B6 ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.SHOW_CHAT"), "");
  }
}
function PromptLabCardComponent_span_58_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"]("\u25C0 ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.HIDE_CHAT"), "");
  }
}
function PromptLabCardComponent_div_63_Template(rf, ctx) {
  if (rf & 1) {
    const _r42 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 73);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_div_63_Template_div_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r42);
      const ctx_r41 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r41.startEditingPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("innerHTML", ctx_r15.renderedPrompt, _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵsanitizeHtml"]);
  }
}
function PromptLabCardComponent_textarea_64_Template(rf, ctx) {
  if (rf & 1) {
    const _r45 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "textarea", 74, 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function PromptLabCardComponent_textarea_64_Template_textarea_ngModelChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r45);
      const ctx_r44 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r44.editingPromptText = $event);
    })("blur", function PromptLabCardComponent_textarea_64_Template_textarea_blur_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r45);
      const ctx_r46 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r46.finishEditingPrompt());
    })("keydown.escape", function PromptLabCardComponent_textarea_64_Template_textarea_keydown_escape_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r45);
      const ctx_r47 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r47.finishEditingPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngModel", ctx_r16.editingPromptText);
  }
}
function PromptLabCardComponent_div_69_div_11_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 85);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r48.diagramStatusMessage || _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.GENERATING_DIAGRAM"), " ");
  }
}
function PromptLabCardComponent_div_69_div_12_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](0, "div", 86);
  }
  if (rf & 2) {
    const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("innerHTML", ctx_r49.diagramSvg, _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵsanitizeHtml"]);
  }
}
function PromptLabCardComponent_div_69_div_13_Template(rf, ctx) {
  if (rf & 1) {
    const _r53 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 87)(1, "div", 88);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](4, "button", 89);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_div_69_div_13_Template_button_click_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r53);
      const ctx_r52 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r52.regenerateDiagram());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](7, "pre", 90);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r50 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx_r50.diagramStatusMessage || _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](3, 3, "PROMPTLAB_CARD.SYNTAX_ERROR"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 5, "PROMPTLAB_CARD.REGENERATE"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r50.diagramPlantUml);
  }
}
function PromptLabCardComponent_div_69_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 91);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_CARD.NO_DIAGRAM"));
  }
}
function PromptLabCardComponent_div_69_Template(rf, ctx) {
  if (rf & 1) {
    const _r55 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 76)(1, "div", 77)(2, "span", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](4, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](6, "button", 79);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_div_69_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵrestoreView"](_r55);
      const ctx_r54 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵresetView"](ctx_r54.copyDiagram());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](9, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](10, "div", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](11, PromptLabCardComponent_div_69_div_11_Template, 3, 3, "div", 81);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](12, PromptLabCardComponent_div_69_div_12_Template, 1, 1, "div", 82);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](13, PromptLabCardComponent_div_69_div_13_Template, 9, 7, "div", 83);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](14, PromptLabCardComponent_div_69_div_14_Template, 3, 3, "div", 84);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx_r17.activeDiagram === "sequence" ? _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](4, 8, "PROMPTLAB_CARD.SEQUENCE_DIAGRAM") : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](5, 10, "PROMPTLAB_CARD.WORKFLOW_DIAGRAM"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](7, 12, "PROMPTLAB_CARD.COPY_PLANTUML"))("disabled", !ctx_r17.diagramPlantUml);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](9, 14, "PROMPTLAB_CARD.COPY_PLANTUML"));
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx_r17.isDiagramLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx_r17.isDiagramLoading && ctx_r17.diagramSvg);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx_r17.isDiagramLoading && !ctx_r17.diagramSvg && ctx_r17.diagramPlantUml);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx_r17.isDiagramLoading && !ctx_r17.diagramSvg && !ctx_r17.diagramPlantUml);
  }
}
class PromptLabCardComponent {
  constructor(cdr, elRef, sanitizer, http, dialog, promptLabService, distillationService, aiChatService, projectsService, translate) {
    this.cdr = cdr;
    this.elRef = elRef;
    this.sanitizer = sanitizer;
    this.http = http;
    this.dialog = dialog;
    this.promptLabService = promptLabService;
    this.distillationService = distillationService;
    this.aiChatService = aiChatService;
    this.projectsService = projectsService;
    this.translate = translate;
    this.isSingleCard = false;
    this.cardDeleted = new _angular_core__WEBPACK_IMPORTED_MODULE_8__.EventEmitter();
    this.cardChanged = new _angular_core__WEBPACK_IMPORTED_MODULE_8__.EventEmitter();
    this.chatCollapsed = false;
    this.chatInputText = '';
    this.chatColumnWidth = null;
    this.isDragging = false;
    /** Streaming state */
    this.isStreaming = false;
    this.streamingContent = '';
    /** Index of the parameter currently being inline-edited */
    this.editingParamIndex = null;
    this.editingParamValue = '';
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_9__.Subject();
    this.needsScroll = false;
    this.moveListener = null;
    this.upListener = null;
    /** True while the distillation LLM call is in progress */
    this.isDistilling = false;
    /** Prompt rendering & editing */
    this.isEditingPrompt = false;
    this.editingPromptText = '';
    this.renderedPrompt = '';
    /** Play execution state (Task 8.2) */
    this.isExecuting = false;
    this.executeStartTime = 0;
    this.executeAccumulatedOutput = '';
    /** Ultimo Run panel toggle (Task 8.2) */
    this.showLastRun = false;
    /** Diagram generation state (Task 9.1) */
    this.activeDiagram = null;
    this.diagramPlantUml = '';
    this.diagramSvg = '';
    this.isDiagramLoading = false;
    this.diagramRenderError = false;
    this.diagramRetryCount = 0;
    this.diagramStatusMessage = '';
    this.DIAGRAM_MAX_RETRIES = 3;
    this.diagramSubscription = null;
  }
  ngOnInit() {
    this.updateRenderedPrompt();
    this.subscribeToCardStream();
    this.subscribeToDistillation();
  }
  ngAfterViewChecked() {
    if (this.needsScroll) {
      this.scrollToBottom();
      this.needsScroll = false;
    }
  }
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.cleanupDragListeners();
    this.cleanupDiagramSubscription();
    this.distillationService.disposeCard(this.card.id);
  }
  // ── Stream subscription ──
  subscribeToCardStream() {
    console.log('[PromptLabCard] subscribeToCardStream for card:', this.card.id);
    this.promptLabService.getCardStream$(this.card.id).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.takeUntil)(this.destroy$)).subscribe(event => {
      console.log('[PromptLabCard] stream event:', event.type, event.data?.substring?.(0, 50) || event.data);
      switch (event.type) {
        case 'chunk':
          if (!this.isStreaming) {
            this.isStreaming = true;
            this.streamingContent = '';
          }
          this.streamingContent += event.data;
          // Accumulate output for Play execution tracking
          if (this.isExecuting) {
            this.executeAccumulatedOutput += event.data;
          }
          this.needsScroll = true;
          this.cdr.markForCheck();
          break;
        case 'complete':
          if (this.streamingContent) {
            const assistantMsg = {
              id: this.generateId(),
              role: 'assistant',
              content: this.streamingContent,
              timestamp: new Date()
            };
            this.card.conversation.push(assistantMsg);
            this.cardChanged.emit(this.card);
            // After each LLM response, trigger distillation to update
            // the prompt on the right. Skip if this was a Play execution.
            if (!this.isExecuting) {
              this.isDistilling = true;
              this.distillationService.triggerDistillation(this.card.id, this.card.conversation, this.card.distilledPrompt, this.promptLabService.getCurrentModel());
            }
          }
          // Complete Play execution — create lastRun (Task 8.2)
          if (this.isExecuting) {
            this.promptLabService.completeCardExecution(this.card.id, this.executeAccumulatedOutput, this.executeStartTime);
            this.isExecuting = false;
            this.executeAccumulatedOutput = '';
          }
          this.isStreaming = false;
          this.streamingContent = '';
          this.needsScroll = true;
          this.cdr.markForCheck();
          break;
        case 'error':
          const errorMsg = {
            id: this.generateId(),
            role: 'assistant',
            content: `Errore: ${event.data}`,
            timestamp: new Date()
          };
          this.card.conversation.push(errorMsg);
          this.isStreaming = false;
          this.streamingContent = '';
          // Reset Play execution on error
          if (this.isExecuting) {
            this.isExecuting = false;
            this.executeAccumulatedOutput = '';
          }
          this.cardChanged.emit(this.card);
          this.needsScroll = true;
          this.cdr.markForCheck();
          break;
        case 'thinking':
          // Ignore thinking events for now
          break;
      }
    });
  }
  subscribeToDistillation() {
    this.distillationService.getDistillationResult$(this.card.id).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.takeUntil)(this.destroy$)).subscribe(result => {
      this.card.distilledPrompt = result.distilledPrompt;
      this.card.parameters = result.parameters;
      this.card.generatedTitle = result.generatedTitle;
      this.isDistilling = false;
      this.updateRenderedPrompt();
      this.cardChanged.emit(this.card);
      this.cdr.markForCheck();
    });
  }
  // ── Header actions ──
  onTitleEdit(event) {
    const el = event.target;
    this.card.generatedTitle = el.innerText.trim();
    this.cardChanged.emit(this.card);
  }
  deleteCard() {
    const title = this.card.generatedTitle || this.card.id;
    if (confirm(this.translate.instant('PROMPTLAB_CARD.DELETE_CONFIRM', {
      title
    }))) {
      this.cardDeleted.emit(this.card.id);
    }
  }
  // ── Parameters ──
  getParamIcon(type) {
    switch (type) {
      case 'file':
        return '\uD83D\uDCC4';
      case 'output_file':
        return '\uD83D\uDCBE';
      case 'directory':
        return '\uD83D\uDCC2';
      case 'text':
        return '\u270E';
      default:
        return '';
    }
  }
  getParamDisplayClass(param) {
    const base = param.type === 'output_file' ? 'output-file' : param.type === 'directory' ? 'directory' : param.type;
    const state = param.value ? 'filled' : 'empty';
    return `${base} ${state}`;
  }
  onParameterClick(param, index) {
    if (param.type === 'text') {
      this.editingParamIndex = index;
      this.editingParamValue = param.value || '';
      this.cdr.markForCheck();
    } else if (param.type === 'output_file') {
      // Output file — "Save As" style: pick folder, then type filename
      this.openSaveAsDialog(param);
    } else {
      // file or directory — open MdExplorer file system dialog
      const data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_2__.ShowFileMetadata();
      const project = this.projectsService.currentProjects$.getValue();
      data.start = project?.path || 'project';
      if (param.type === 'directory') {
        data.title = this.translate.instant('PROMPTLAB_CARD.SELECT_FOLDER', {
          name: param.name
        });
        data.typeOfSelection = 'Folders';
        data.buttonText = this.translate.instant('PROMPTLAB_CARD.SELECT_FOLDER_BTN');
      } else {
        data.title = this.translate.instant('PROMPTLAB_CARD.SELECT_FILE', {
          name: param.name
        });
        data.typeOfSelection = 'FoldersAndFiles';
        data.buttonText = this.translate.instant('PROMPTLAB_CARD.SELECT_FILE_BTN');
      }
      const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__.ShowFileSystemComponent, {
        width: '800px',
        height: '600px',
        panelClass: 'resizable-dialog-container',
        data: data
      });
      dialogRef.afterClosed().subscribe(result => {
        if (result && result.data) {
          param.value = this.toRelativePath(result.data);
          this.cardChanged.emit(this.card);
          this.cdr.markForCheck();
        }
      });
    }
  }
  openSaveAsDialog(param) {
    const data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_2__.ShowFileMetadata();
    const project = this.projectsService.currentProjects$.getValue();
    data.start = project?.path || 'project';
    data.title = this.translate.instant('PROMPTLAB_CARD.SAVE_AS', {
      name: param.name
    });
    data.typeOfSelection = 'Folders';
    data.buttonText = this.translate.instant('PROMPTLAB_CARD.SAVE_BTN');
    data.saveAs = true;
    // Suggest current filename if already set
    const currentName = param.value ? param.value.split(/[/\\]/).pop() : '';
    data.defaultFileName = currentName || 'output.md';
    const dialogRef = this.dialog.open(_commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__.ShowFileSystemComponent, {
      width: '800px',
      height: '600px',
      panelClass: 'resizable-dialog-container',
      data: data
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result && result.data) {
        param.value = this.toRelativePath(result.data);
        this.cardChanged.emit(this.card);
        this.cdr.markForCheck();
      }
    });
  }
  confirmParamEdit(param) {
    param.value = this.editingParamValue;
    this.editingParamIndex = null;
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }
  cancelParamEdit() {
    this.editingParamIndex = null;
    this.cdr.markForCheck();
  }
  onParamEditKeydown(event, param) {
    if (event.key === 'Enter') {
      this.confirmParamEdit(param);
    } else if (event.key === 'Escape') {
      this.cancelParamEdit();
    }
  }
  // ── Play & Last Run (Task 8.2) ──
  onPlay() {
    if (this.isStreaming || this.isExecuting) return;
    if (!this.card.distilledPrompt?.trim()) return;
    // 1. Copy the distilled prompt into the chat as the first message of this execution
    const promptMsg = {
      id: this.generateId(),
      role: 'user',
      content: this.card.distilledPrompt,
      timestamp: new Date()
    };
    this.card.conversation.push(promptMsg);
    this.needsScroll = true;
    // 2. Start execution
    this.isExecuting = true;
    this.executeStartTime = Date.now();
    this.executeAccumulatedOutput = '';
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
    console.log('[PromptLabCard] Play — sending distilled prompt to LLM');
    this.promptLabService.executeCard(this.card.id);
  }
  toggleLastRun() {
    this.showLastRun = !this.showLastRun;
    this.cdr.markForCheck();
  }
  formatDuration(ms) {
    if (ms < 1000) return `${ms}ms`;
    const seconds = Math.round(ms / 1000);
    if (seconds < 60) return `${seconds}s`;
    const minutes = Math.floor(seconds / 60);
    const remaining = seconds % 60;
    return `${minutes}m ${remaining}s`;
  }
  formatRunDate(date) {
    if (!date) return '';
    const d = new Date(date);
    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const year = d.getFullYear();
    const hours = d.getHours().toString().padStart(2, '0');
    const minutes = d.getMinutes().toString().padStart(2, '0');
    return `${day}/${month}/${year} ${hours}:${minutes}`;
  }
  getRunParamKeys() {
    if (!this.card.lastRun) return [];
    return Object.keys(this.card.lastRun.resolvedParameters);
  }
  // ── Diagram generation (Task 9.1) ──
  toggleDiagram(type) {
    if (this.activeDiagram === type) {
      this.activeDiagram = null;
      this.cleanupDiagramSubscription();
      this.cdr.markForCheck();
      return;
    }
    this.activeDiagram = type;
    this.diagramPlantUml = '';
    this.diagramSvg = '';
    this.diagramRenderError = false;
    this.diagramRetryCount = 0;
    this.diagramStatusMessage = '';
    // Check cached diagram
    const cache = type === 'sequence' ? this.card.sequenceDiagram : this.card.workflowDiagram;
    const currentHash = this.hashPrompt(this.card.distilledPrompt || '');
    if (cache?.svgPath && cache.promptHash === currentHash) {
      // Cache hit — load SVG from file
      this.isDiagramLoading = true;
      this.diagramStatusMessage = this.translate.instant('PROMPTLAB_CARD.LOADING_CACHE');
      this.cdr.markForCheck();
      this.loadCachedSvg(cache.svgPath);
    } else {
      // Cache miss or prompt changed — regenerate via LLM
      this.isDiagramLoading = true;
      this.cdr.markForCheck();
      this.generateDiagram(type);
    }
  }
  /**
   * Load a cached SVG file via the backend file API.
   * The svgPath is relative to the template's directory.
   */
  loadCachedSvg(svgPath) {
    const session = this.promptLabService.currentSession();
    if (!session?.templatePath) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }
    // templatePath is absolute (e.g. "C:\...\promptlab\file.md")
    // svgPath is relative to template dir (e.g. "assets/card-xxx-workflow.svg")
    // The API needs a path relative to the project root.
    // templatePath relative to project = toRelativePath(templatePath) → "promptlab\file.md"
    // We replace the filename with the svgPath.
    const relativeTemplatePath = this.toRelativePath(session.templatePath);
    const relativeDir = relativeTemplatePath.replace(/[/\\][^/\\]+$/, '');
    const fileName = svgPath.split('/').pop() || svgPath;
    const relativeSvgPath = `${relativeDir}/assets/${fileName}`;
    const url = `/api/MdExplorerEditorReact/${relativeSvgPath}`;
    this.http.get(url, {
      responseType: 'text'
    }).subscribe({
      next: svgContent => {
        if (svgContent) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(svgContent);
          this.diagramStatusMessage = '';
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        // File not found — regenerate
        console.warn('[PromptLabCard] Cached SVG not found, regenerating...');
        this.generateDiagram(this.activeDiagram);
      }
    });
  }
  generateDiagram(type) {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      _this.cleanupDiagramSubscription();
      const channelId = `card-${_this.card.id}-diagram`;
      _this.aiChatService.clearChannelHistory(channelId);
      // Get diagram prompts from the session (configurable via Settings)
      const session = _this.promptLabService.currentSession();
      const diagramPrompt = type === 'sequence' ? session?.sequencePrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_3__.DEFAULT_SEQUENCE_PROMPT : session?.workflowPrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_3__.DEFAULT_WORKFLOW_PROMPT;
      const message = `${diagramPrompt}\n\n--- Prompt ---\n${_this.card.distilledPrompt || '(nessun prompt distillato)'}`;
      let accumulated = '';
      _this.diagramSubscription = _this.aiChatService.getChannelStream$(channelId).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.takeUntil)(_this.destroy$)).subscribe(event => {
        switch (event.type) {
          case 'chunk':
            accumulated += event.data;
            break;
          case 'complete':
            const plantUml = _this.extractPlantUml(accumulated);
            _this.diagramPlantUml = plantUml;
            _this.cleanupDiagramSubscription();
            // Render PlantUML to SVG via backend
            _this.renderPlantUmlToSvg(plantUml);
            break;
          case 'error':
            _this.diagramPlantUml = '';
            _this.diagramSvg = '';
            _this.isDiagramLoading = false;
            _this.cdr.markForCheck();
            _this.cleanupDiagramSubscription();
            break;
        }
      });
      // Use diagram-specific model if configured, otherwise session default
      const diagramModel = type === 'sequence' ? session?.sequencePromptModel || '' : session?.workflowPromptModel || '';
      if (diagramModel) {
        const provider = diagramModel.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
        yield _this.aiChatService.setProviderAsync(provider, diagramModel);
      } else {
        yield _this.promptLabService.ensureChatModePublic();
      }
      _this.aiChatService.sendMessageToChannel(message, channelId);
    })();
  }
  renderPlantUmlToSvg(plantUml) {
    if (!plantUml) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }
    // Build save path for caching the SVG to disk
    const savePath = this.buildSvgSavePath();
    this.http.post('/api/plantumlextensions/RenderSvg', {
      plantUmlCode: plantUml,
      savePath: savePath
    }).subscribe({
      next: response => {
        if (response?.svg) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(response.svg);
          this.diagramStatusMessage = '';
          // Update card cache reference
          this.updateDiagramCache();
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        console.error('[PromptLabCard] Error rendering PlantUML:', err);
        const errorMessage = err?.error?.error || err?.message || 'Unknown rendering error';
        if (this.diagramRetryCount < this.DIAGRAM_MAX_RETRIES) {
          // Auto-retry: send the error back to the LLM to fix
          this.diagramRetryCount++;
          this.diagramStatusMessage = this.translate.instant('PROMPTLAB_CARD.SYNTAX_ERROR_RETRY', {
            retry: this.diagramRetryCount,
            max: this.DIAGRAM_MAX_RETRIES
          });
          this.cdr.markForCheck();
          this.requestDiagramFix(plantUml, errorMessage);
        } else {
          // Max retries reached — show error + code
          this.diagramSvg = '';
          this.diagramRenderError = true;
          this.diagramStatusMessage = this.translate.instant('PROMPTLAB_CARD.ERROR_AFTER_RETRIES', {
            max: this.DIAGRAM_MAX_RETRIES
          });
          this.isDiagramLoading = false;
          this.cdr.markForCheck();
        }
      }
    });
  }
  /**
   * Send the broken PlantUML + error message back to the LLM on the diagram channel,
   * asking it to fix the syntax error. Reuses the same channel (history preserved).
   */
  requestDiagramFix(brokenCode, errorMessage) {
    var _this2 = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      _this2.cleanupDiagramSubscription();
      const channelId = `card-${_this2.card.id}-diagram`;
      const fixMessage = `The PlantUML code you generated has a syntax error. Here is the error from the PlantUML renderer:\n\n${errorMessage}\n\nHere is the broken code:\n\`\`\`\n${brokenCode}\n\`\`\`\n\nPlease fix the syntax error and return ONLY the corrected PlantUML code between @startuml and @enduml, nothing else.`;
      let accumulated = '';
      _this2.diagramSubscription = _this2.aiChatService.getChannelStream$(channelId).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.takeUntil)(_this2.destroy$)).subscribe(event => {
        switch (event.type) {
          case 'chunk':
            accumulated += event.data;
            break;
          case 'complete':
            const plantUml = _this2.extractPlantUml(accumulated);
            _this2.diagramPlantUml = plantUml;
            _this2.cleanupDiagramSubscription();
            _this2.renderPlantUmlToSvg(plantUml);
            break;
          case 'error':
            _this2.diagramSvg = '';
            _this2.diagramRenderError = true;
            _this2.diagramStatusMessage = _this2.translate.instant('PROMPTLAB_CARD.LLM_COMM_ERROR');
            _this2.isDiagramLoading = false;
            _this2.cdr.markForCheck();
            _this2.cleanupDiagramSubscription();
            break;
        }
      });
      // Use same model as the diagram generation
      const session = _this2.promptLabService.currentSession();
      const diagramModel = _this2.activeDiagram === 'sequence' ? session?.sequencePromptModel || '' : session?.workflowPromptModel || '';
      if (diagramModel) {
        const provider = diagramModel.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
        yield _this2.aiChatService.setProviderAsync(provider, diagramModel);
      } else {
        yield _this2.promptLabService.ensureChatModePublic();
      }
      _this2.aiChatService.sendMessageToChannel(fixMessage, channelId);
    })();
  }
  /**
   * Simple hash of the prompt text for cache comparison.
   * DJB2 algorithm — fast, good distribution, no crypto needed.
   */
  hashPrompt(text) {
    let hash = 5381;
    for (let i = 0; i < text.length; i++) {
      hash = (hash << 5) + hash + text.charCodeAt(i);
      hash |= 0; // Convert to 32-bit integer
    }
    return (hash >>> 0).toString(36);
  }
  /**
   * Build the absolute save path for the current diagram's SVG file.
   * Returns null if session has no templatePath.
   */
  buildSvgSavePath() {
    if (!this.activeDiagram) return null;
    const session = this.promptLabService.currentSession();
    if (!session?.templatePath) return null;
    const fileName = `${this.card.id}-${this.activeDiagram}.svg`;
    const templateDir = session.templatePath.replace(/[/\\][^/\\]+$/, '');
    const separator = templateDir.includes('/') ? '/' : '\\';
    return `${templateDir}${separator}assets${separator}${fileName}`;
  }
  /**
   * Update the card's diagram cache reference (hash + relative SVG path).
   * Triggers cardChanged → auto-save → persisted as ![hash](assets/...) in the .md.
   */
  updateDiagramCache() {
    if (!this.activeDiagram) return;
    const promptHash = this.hashPrompt(this.card.distilledPrompt || '');
    const fileName = `${this.card.id}-${this.activeDiagram}.svg`;
    const cache = {
      promptHash,
      svgPath: `assets/${fileName}`
    };
    if (this.activeDiagram === 'sequence') {
      this.card.sequenceDiagram = cache;
    } else {
      this.card.workflowDiagram = cache;
    }
    this.cardChanged.emit(this.card);
  }
  extractPlantUml(text) {
    const match = text.match(/@startuml[\s\S]*?@enduml/);
    return match ? match[0] : text.trim();
  }
  regenerateDiagram() {
    if (this.activeDiagram) {
      const type = this.activeDiagram;
      this.activeDiagram = null;
      this.cleanupDiagramSubscription();
      // Re-trigger
      setTimeout(() => this.toggleDiagram(type));
    }
  }
  copyDiagram() {
    if (this.diagramPlantUml) {
      navigator.clipboard.writeText(this.diagramPlantUml).catch(err => {
        console.error('Failed to copy diagram to clipboard:', err);
      });
    }
  }
  cleanupDiagramSubscription() {
    if (this.diagramSubscription) {
      this.diagramSubscription.unsubscribe();
      this.diagramSubscription = null;
    }
  }
  // ── Chat ──
  toggleChat() {
    this.chatCollapsed = !this.chatCollapsed;
    this.cdr.markForCheck();
  }
  resetChat() {
    this.promptLabService.resetCardChat(this.card.id);
    this.card.conversation = [];
    this.isStreaming = false;
    this.streamingContent = '';
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }
  sendMessage() {
    const text = this.chatInputText.trim();
    if (!text || this.isStreaming) {
      return;
    }
    const msg = {
      id: this.generateId(),
      role: 'user',
      content: text,
      timestamp: new Date()
    };
    this.card.conversation.push(msg);
    this.chatInputText = '';
    this.cardChanged.emit(this.card);
    this.needsScroll = true;
    this.cdr.markForCheck();
    // Send to LLM — the chat is a conversational editor for the prompt.
    // The user talks to the LLM to refine the distilled prompt.
    // After the LLM responds, distillation updates the prompt on the right.
    this.promptLabService.sendCardMessage(this.card.id, text);
  }
  onChatInputKeydown(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
  formatTime(date) {
    if (!date) {
      return '';
    }
    const d = new Date(date);
    return d.getHours().toString().padStart(2, '0') + ':' + d.getMinutes().toString().padStart(2, '0');
  }
  // ── Auto-scroll ──
  scrollToBottom() {
    if (this.messagesContainer?.nativeElement) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }
  // ── Distilled Prompt ──
  /** Convert markdown to simple HTML for display */
  updateRenderedPrompt() {
    const md = this.card.distilledPrompt || '';
    const lines = md.split('\n');
    const out = [];
    let inList = false;
    for (const raw of lines) {
      let line = raw.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
      // Inline formatting
      line = line.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>').replace(/\*(.+?)\*/g, '<em>$1</em>').replace(/`([^`]+)`/g, '<code>$1</code>').replace(/\{\{(\w+)\}\}/g, '<span class="param-highlight">{{$1}}</span>');
      // Headings
      if (/^### (.+)/.test(line)) {
        if (inList) {
          out.push('</ul>');
          inList = false;
        }
        out.push(line.replace(/^### (.+)/, '<h4>$1</h4>'));
      } else if (/^## (.+)/.test(line)) {
        if (inList) {
          out.push('</ul>');
          inList = false;
        }
        out.push(line.replace(/^## (.+)/, '<h3>$1</h3>'));
      } else if (/^# (.+)/.test(line)) {
        if (inList) {
          out.push('</ul>');
          inList = false;
        }
        out.push(line.replace(/^# (.+)/, '<h2>$1</h2>'));
      }
      // List items
      else if (/^[-*] (.+)/.test(line)) {
        if (!inList) {
          out.push('<ul>');
          inList = true;
        }
        out.push(line.replace(/^[-*] (.+)/, '<li>$1</li>'));
      } else if (/^\d+\. (.+)/.test(line)) {
        if (!inList) {
          out.push('<ul>');
          inList = true;
        }
        out.push(line.replace(/^\d+\. (.+)/, '<li>$1</li>'));
      }
      // Empty line
      else if (line.trim() === '') {
        if (inList) {
          out.push('</ul>');
          inList = false;
        }
        out.push('<br>');
      }
      // Normal text
      else {
        if (inList) {
          out.push('</ul>');
          inList = false;
        }
        out.push('<div>' + line + '</div>');
      }
    }
    if (inList) out.push('</ul>');
    this.renderedPrompt = this.sanitizer.bypassSecurityTrustHtml(out.join('\n'));
  }
  startEditingPrompt() {
    this.isEditingPrompt = true;
    this.editingPromptText = this.card.distilledPrompt || '';
    this.cdr.markForCheck();
    // Focus the textarea after render
    setTimeout(() => {
      const ta = this.elRef.nativeElement.querySelector('.prompt-editor');
      if (ta) ta.focus();
    });
  }
  finishEditingPrompt() {
    this.isEditingPrompt = false;
    this.card.distilledPrompt = this.editingPromptText;
    this.updateRenderedPrompt();
    this.cardChanged.emit(this.card);
    this.cdr.markForCheck();
  }
  // ── Splitter ──
  onSplitterMouseDown(event) {
    event.preventDefault();
    const cardBody = event.target.closest('.card-body');
    if (!cardBody) {
      return;
    }
    const chatCol = cardBody.querySelector('.chat-column');
    if (!chatCol) {
      return;
    }
    const startX = event.clientX;
    const startWidth = chatCol.offsetWidth;
    this.isDragging = true;
    this.moveListener = e => {
      const dx = e.clientX - startX;
      const newWidth = Math.max(200, startWidth + dx);
      // also limit so prompt column keeps at least 200px
      const bodyWidth = cardBody.offsetWidth;
      const splitterWidth = 5;
      const maxChatWidth = bodyWidth - splitterWidth - 200;
      this.chatColumnWidth = Math.min(newWidth, maxChatWidth);
      this.cdr.markForCheck();
      e.preventDefault();
    };
    this.upListener = () => {
      this.isDragging = false;
      this.cleanupDragListeners();
      this.cdr.markForCheck();
    };
    document.addEventListener('mousemove', this.moveListener);
    document.addEventListener('mouseup', this.upListener);
  }
  getChatColumnStyle() {
    if (this.chatColumnWidth !== null) {
      return {
        flex: `0 0 ${this.chatColumnWidth}px`
      };
    }
    return {};
  }
  // ── Helpers ──
  cleanupDragListeners() {
    if (this.moveListener) {
      document.removeEventListener('mousemove', this.moveListener);
      this.moveListener = null;
    }
    if (this.upListener) {
      document.removeEventListener('mouseup', this.upListener);
      this.upListener = null;
    }
  }
  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
  }
  toRelativePath(absolutePath) {
    const project = this.projectsService.currentProjects$.getValue();
    if (!project?.path) return absolutePath;
    const root = project.path.replace(/[\/\\]$/, '');
    const normalized = absolutePath.replace(/\//g, '\\');
    const normalizedRoot = root.replace(/\//g, '\\');
    if (normalized.toLowerCase().startsWith(normalizedRoot.toLowerCase())) {
      return normalized.substring(normalizedRoot.length).replace(/^[\\\/]/, '');
    }
    return absolutePath;
  }
  static {
    this.ɵfac = function PromptLabCardComponent_Factory(t) {
      return new (t || PromptLabCardComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_8__.ChangeDetectorRef), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_8__.ElementRef), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_11__.DomSanitizer), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_12__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_13__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_promptlab_service__WEBPACK_IMPORTED_MODULE_4__.PromptLabService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_promptlab_distillation_service__WEBPACK_IMPORTED_MODULE_5__.PromptLabDistillationService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_6__.AiChatService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_7__.ProjectsService), _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_14__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵdefineComponent"]({
      type: PromptLabCardComponent,
      selectors: [["app-promptlab-card"]],
      viewQuery: function PromptLabCardComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵviewQuery"](_c0, 5);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵloadQuery"]()) && (ctx.messagesContainer = _t.first);
        }
      },
      inputs: {
        card: "card",
        isSingleCard: "isSingleCard"
      },
      outputs: {
        cardDeleted: "cardDeleted",
        cardChanged: "cardChanged"
      },
      decls: 70,
      vars: 72,
      consts: [[1, "card"], [1, "card-header"], [1, "card-title-row"], [1, "pin"], ["contenteditable", "true", 1, "title", 3, "blur"], [1, "spacer"], [1, "delete-btn", 3, "title", "click"], ["class", "card-params", 4, "ngIf"], [1, "card-actions"], [1, "play-btn", 3, "disabled", "click"], [1, "lastrun-btn", 3, "disabled", "click"], [1, "action-separator"], [1, "diagram-btn", 3, "disabled", "click"], ["class", "diagram-spinner", 4, "ngIf"], [4, "ngIf"], ["class", "ultimo-run", 4, "ngIf"], [1, "card-body"], [1, "chat-column", 3, "ngStyle"], [1, "column-label"], [1, "label-text"], [1, "reset-chat-btn", 3, "title", "disabled", "click"], [1, "chat-messages"], ["messagesContainer", ""], ["class", "msg", 4, "ngFor", "ngForOf"], ["class", "msg", 4, "ngIf"], ["class", "chat-empty", 4, "ngIf"], ["class", "distillation-indicator", 4, "ngIf"], [1, "chat-input"], ["rows", "4", 3, "placeholder", "ngModel", "disabled", "ngModelChange", "keydown"], [3, "disabled", "click"], [1, "column-splitter", 3, "mousedown"], [1, "grip"], [1, "prompt-column"], [1, "collapse-chat-btn", 3, "click"], [1, "prompt-content"], ["class", "prompt-rendered", 3, "innerHTML", "click", 4, "ngIf"], ["class", "prompt-editor", 3, "ngModel", "ngModelChange", "blur", "keydown.escape", 4, "ngIf"], [1, "prompt-note"], ["class", "diagram-panel", 4, "ngIf"], [1, "card-params"], [4, "ngFor", "ngForOf"], ["class", "inline-editor", 4, "ngIf"], ["class", "param-btn", 3, "ngClass", "click", 4, "ngIf"], [1, "inline-editor"], [3, "value", "input", "keydown"], ["paramInput", ""], [1, "confirm", 3, "click"], [1, "param-btn", 3, "ngClass", "click"], [1, "diagram-spinner"], [1, "ultimo-run"], [1, "run-section"], [1, "run-section-title"], [1, "run-meta"], [1, "run-meta-item"], ["class", "run-section", 4, "ngIf"], [1, "run-prompt"], [1, "run-output"], [1, "run-params"], ["class", "run-param", 4, "ngFor", "ngForOf"], [1, "run-param"], [1, "run-param-name"], [1, "run-param-eq"], [1, "run-param-val"], [1, "msg"], [1, "author", 3, "ngClass"], [1, "time"], [1, "text"], [1, "author", "assistant"], [1, "text", "streaming"], [1, "streaming-cursor"], [1, "chat-empty"], [1, "distillation-indicator"], [1, "distill-spinner"], [1, "prompt-rendered", 3, "innerHTML", "click"], [1, "prompt-editor", 3, "ngModel", "ngModelChange", "blur", "keydown.escape"], ["promptEditor", ""], [1, "diagram-panel"], [1, "diagram-header"], [1, "diagram-title"], [1, "diagram-copy-btn", 3, "title", "disabled", "click"], [1, "diagram-body"], ["class", "diagram-loading", 4, "ngIf"], ["class", "diagram-svg", 3, "innerHTML", 4, "ngIf"], ["class", "diagram-error-panel", 4, "ngIf"], ["class", "diagram-empty", 4, "ngIf"], [1, "diagram-loading"], [1, "diagram-svg", 3, "innerHTML"], [1, "diagram-error-panel"], [1, "diagram-error-msg"], [1, "diagram-regen-btn", 3, "click"], [1, "diagram-code"], [1, "diagram-empty"]],
      template: function PromptLabCardComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "div", 2)(3, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](4, "\uF4CC");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](5, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("blur", function PromptLabCardComponent_Template_span_blur_5_listener($event) {
            return ctx.onTitleEdit($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](6, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](8, "span", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](9, "button", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_9_listener() {
            return ctx.deleteCard();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](10, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](11, "\uF5D1");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](12, PromptLabCardComponent_div_12_Template, 2, 1, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](13, "div", 8)(14, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_14_listener() {
            return ctx.onPlay();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](15);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](16, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](17, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_17_listener() {
            return ctx.toggleLastRun();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](18);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](19, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelement"](20, "span", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](21, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_21_listener() {
            return ctx.toggleDiagram("sequence");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](22, PromptLabCardComponent_span_22_Template, 1, 0, "span", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](23, PromptLabCardComponent_ng_container_23_Template, 3, 3, "ng-container", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](24, PromptLabCardComponent_ng_container_24_Template, 3, 3, "ng-container", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](25, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_25_listener() {
            return ctx.toggleDiagram("workflow");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](26, PromptLabCardComponent_span_26_Template, 1, 0, "span", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](27, PromptLabCardComponent_ng_container_27_Template, 3, 3, "ng-container", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](28, PromptLabCardComponent_ng_container_28_Template, 3, 3, "ng-container", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](29, PromptLabCardComponent_div_29_Template, 39, 28, "div", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](30, "div", 16)(31, "div", 17)(32, "div", 18)(33, "span", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](34);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](35, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](36, "button", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_36_listener() {
            return ctx.resetChat();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](37, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](38, "\u21BB Reset");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](39, "div", 21, 22);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](41, PromptLabCardComponent_div_41_Template, 9, 8, "div", 23);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](42, PromptLabCardComponent_div_42_Template, 7, 4, "div", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](43, PromptLabCardComponent_div_43_Template, 3, 3, "div", 25);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](44, PromptLabCardComponent_div_44_Template, 4, 3, "div", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](45, "div", 27)(46, "textarea", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("ngModelChange", function PromptLabCardComponent_Template_textarea_ngModelChange_46_listener($event) {
            return ctx.chatInputText = $event;
          })("keydown", function PromptLabCardComponent_Template_textarea_keydown_46_listener($event) {
            return ctx.onChatInputKeydown($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](47, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](48, "button", 29);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_48_listener() {
            return ctx.sendMessage();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](49);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](50, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](51, "div", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("mousedown", function PromptLabCardComponent_Template_div_mousedown_51_listener($event) {
            return ctx.onSplitterMouseDown($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](52, "span", 31);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](53, "\u22EE");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](54, "div", 32)(55, "div", 18)(56, "button", 33);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_56_listener() {
            return ctx.toggleChat();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](57, PromptLabCardComponent_span_57_Template, 3, 3, "span", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](58, PromptLabCardComponent_span_58_Template, 3, 3, "span", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](59, "span", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](60);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](61, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](62, "div", 34);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](63, PromptLabCardComponent_div_63_Template, 1, 1, "div", 35);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](64, PromptLabCardComponent_textarea_64_Template, 2, 1, "textarea", 36);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementStart"](65, "div", 37);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtext"](66);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](67, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipe"](68, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtemplate"](69, PromptLabCardComponent_div_69_Template, 15, 16, "div", 38);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("single-card", ctx.isSingleCard);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵattribute"]("data-placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](6, 50, "PROMPTLAB_CARD.TITLE_PLACEHOLDER"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](ctx.card.generatedTitle);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](10, 52, "PROMPTLAB_CARD.DELETE_CARD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.card.parameters.length > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx.isStreaming || ctx.isExecuting);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx.isExecuting ? "..." : "\u25B6 " + _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](16, 54, "PROMPTLAB_CARD.PLAY"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx.showLastRun)("has-run", !!ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", !ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate2"](" \uF441 ", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](19, 56, "PROMPTLAB_CARD.LAST_RUN"), " ", ctx.showLastRun ? "\u25B2" : "\u25BC", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx.activeDiagram === "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx.isDiagramLoading && ctx.activeDiagram !== "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !(ctx.isDiagramLoading && ctx.activeDiagram === "sequence"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("active", ctx.activeDiagram === "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx.isDiagramLoading && ctx.activeDiagram !== "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !(ctx.isDiagramLoading && ctx.activeDiagram === "workflow"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.showLastRun && ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵclassProp"]("chat-collapsed", ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngStyle", ctx.getChatColumnStyle());
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](35, 58, "PROMPTLAB_CARD.CHAT_WITH_LLM"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](37, 60, "PROMPTLAB_CARD.CLEAR_CHAT"))("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngForOf", ctx.card.conversation);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.card.conversation.length === 0 && !ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isDistilling);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("placeholder", _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](47, 62, "PROMPTLAB_CARD.CHAT_PLACEHOLDER"))("ngModel", ctx.chatInputText)("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" ", ctx.isStreaming ? "..." : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](50, 64, "PROMPTLAB_CARD.SEND"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](8);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](61, 66, "PROMPTLAB_CARD.DISTILLED_PROMPT"));
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", !ctx.isEditingPrompt);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.isEditingPrompt);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵtextInterpolate1"](" \u2139 ", ctx.isEditingPrompt ? _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](67, 68, "PROMPTLAB_CARD.EDIT_NOTE") : _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵpipeBind1"](68, 70, "PROMPTLAB_CARD.VIEW_NOTE"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_8__["ɵɵproperty"]("ngIf", ctx.activeDiagram);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_15__.NgClass, _angular_common__WEBPACK_IMPORTED_MODULE_15__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_15__.NgIf, _angular_common__WEBPACK_IMPORTED_MODULE_15__.NgStyle, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_16__.NgModel, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_14__.TranslatePipe],
      styles: ["@charset \"UTF-8\";\n[_nghost-%COMP%] {\n  display: block;\n}\n\n\n.card[_ngcontent-%COMP%] {\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 8px;\n  display: flex;\n  flex-direction: column;\n  flex: 1 1 auto;\n  min-height: 0;\n}\n\n\n.card-header[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n\n.card-title-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-bottom: 8px;\n}\n.card-title-row[_ngcontent-%COMP%]   .pin[_ngcontent-%COMP%] {\n  color: #ff9800;\n  font-size: 16px;\n  flex-shrink: 0;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%] {\n  font-size: 14px;\n  color: #fff;\n  font-weight: 600;\n  outline: none;\n  border-radius: 3px;\n  padding: 2px 4px;\n  transition: background 0.15s;\n  min-width: 60px;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]:focus {\n  background: #2a2a2a;\n  box-shadow: 0 0 0 1px #555 inset;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]:empty::before {\n  content: attr(data-placeholder);\n  color: #555;\n}\n.card-title-row[_ngcontent-%COMP%]   .spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.card-title-row[_ngcontent-%COMP%]   .delete-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #666;\n  font-size: 14px;\n  cursor: pointer;\n  padding: 4px 6px;\n  border-radius: 3px;\n  transition: all 0.15s;\n}\n.card-title-row[_ngcontent-%COMP%]   .delete-btn[_ngcontent-%COMP%]:hover {\n  color: #ef5350;\n  background: rgba(239, 83, 80, 0.1);\n}\n\n\n.card-params[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  flex-wrap: wrap;\n  margin-bottom: 10px;\n}\n\n.param-btn[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  padding: 4px 10px;\n  border-radius: 14px;\n  font-size: 11px;\n  cursor: pointer;\n  border: 1px solid;\n  transition: all 0.2s;\n  white-space: nowrap;\n  \n  \n  \n  \n}\n.param-btn[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.2);\n}\n.param-btn.file.empty[_ngcontent-%COMP%] {\n  background: #1a2733;\n  border-color: #2979ff;\n  border-style: dashed;\n  color: #5c8cc7;\n}\n.param-btn.file.filled[_ngcontent-%COMP%] {\n  background: #1e3a5f;\n  border-color: #2979ff;\n  color: #82b1ff;\n}\n.param-btn.output-file.empty[_ngcontent-%COMP%] {\n  background: #1a3320;\n  border-color: #4caf50;\n  border-style: dashed;\n  color: #6d9c70;\n}\n.param-btn.output-file.filled[_ngcontent-%COMP%] {\n  background: #1b5e20;\n  border-color: #4caf50;\n  color: #a5d6a7;\n}\n.param-btn.directory.empty[_ngcontent-%COMP%] {\n  background: #2a2320;\n  border-color: #8d6e63;\n  border-style: dashed;\n  color: #9c7d6d;\n}\n.param-btn.directory.filled[_ngcontent-%COMP%] {\n  background: #3e2723;\n  border-color: #8d6e63;\n  color: #d7ccc8;\n}\n.param-btn.text.empty[_ngcontent-%COMP%] {\n  background: #33302a;\n  border-color: #ff9800;\n  border-style: dashed;\n  color: #a68c5c;\n}\n.param-btn.text.filled[_ngcontent-%COMP%] {\n  background: #3d3520;\n  border-color: #ff9800;\n  color: #ffe0b2;\n}\n\n\n.inline-editor[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: #3d3520;\n  border: 1px solid #ff9800;\n  border-radius: 16px;\n  padding: 4px 8px;\n}\n.inline-editor[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #ffe0b2;\n  font-size: 12px;\n  width: 100px;\n  outline: none;\n}\n.inline-editor[_ngcontent-%COMP%]   .confirm[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  border-radius: 10px;\n  width: 20px;\n  height: 20px;\n  font-size: 12px;\n  cursor: pointer;\n  line-height: 20px;\n  text-align: center;\n  flex-shrink: 0;\n}\n.inline-editor[_ngcontent-%COMP%]   .confirm[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.1);\n}\n\n\n.card-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  align-items: center;\n}\n\n.play-btn[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  font-weight: 600;\n  cursor: pointer;\n  transition: filter 0.15s;\n}\n.play-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  filter: brightness(1.1);\n}\n.play-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n\n.lastrun-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  padding: 5px 12px;\n  border-radius: 4px;\n  font-size: 11px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.lastrun-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #888;\n  color: #ccc;\n}\n.lastrun-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n.lastrun-btn.has-run[_ngcontent-%COMP%] {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n.lastrun-btn.active[_ngcontent-%COMP%] {\n  background: #33280a;\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n.action-separator[_ngcontent-%COMP%] {\n  width: 1px;\n  height: 20px;\n  background: #3c3c3c;\n  margin: 0 4px;\n  flex-shrink: 0;\n}\n\n.diagram-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #444;\n  color: #aaa;\n  padding: 4px 10px;\n  border-radius: 4px;\n  font-size: 10px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.diagram-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #888;\n  color: #ccc;\n}\n.diagram-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n.diagram-btn.active[_ngcontent-%COMP%] {\n  border-color: #82b1ff;\n  color: #82b1ff;\n  background: #1a2733;\n}\n\n.diagram-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 10px;\n  height: 10px;\n  border: 2px solid #555;\n  border-top-color: #82b1ff;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_diagram-spin 0.8s linear infinite;\n  margin-right: 4px;\n  vertical-align: middle;\n}\n\n@keyframes _ngcontent-%COMP%_diagram-spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n\n.card-body[_ngcontent-%COMP%] {\n  flex: 1 1 auto;\n  display: flex;\n  min-height: 400px;\n  overflow: hidden;\n  \n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .chat-column[_ngcontent-%COMP%] {\n  display: none;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .column-splitter[_ngcontent-%COMP%] {\n  display: none;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .prompt-column[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .collapse-chat-btn[_ngcontent-%COMP%] {\n  background: #33280a;\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n\n.chat-column[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 200px;\n  min-height: 0;\n  overflow: hidden;\n}\n\n\n.column-label[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #888;\n  padding: 8px 12px;\n  background: #2a2a2a;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  gap: 6px;\n}\n.column-label[_ngcontent-%COMP%]   .label-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.reset-chat-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 9px;\n  cursor: pointer;\n  padding: 2px 8px;\n  border-radius: 3px;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.reset-chat-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ef5350;\n  color: #ef5350;\n}\n\n.collapse-chat-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 9px;\n  cursor: pointer;\n  padding: 2px 8px;\n  border-radius: 3px;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.collapse-chat-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n\n.chat-messages[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 12px;\n}\n\n.msg[_ngcontent-%COMP%] {\n  margin-bottom: 12px;\n}\n.msg[_ngcontent-%COMP%]   .author[_ngcontent-%COMP%] {\n  font-size: 11px;\n  font-weight: 600;\n  margin-bottom: 2px;\n}\n.msg[_ngcontent-%COMP%]   .author.user[_ngcontent-%COMP%] {\n  color: #82b1ff;\n}\n.msg[_ngcontent-%COMP%]   .author.assistant[_ngcontent-%COMP%] {\n  color: #c792ea;\n}\n.msg[_ngcontent-%COMP%]   .author[_ngcontent-%COMP%]   .time[_ngcontent-%COMP%] {\n  color: #666;\n  font-weight: 400;\n  font-size: 10px;\n  margin-left: 6px;\n}\n.msg[_ngcontent-%COMP%]   .text[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.5;\n  color: #ccc;\n  white-space: pre-wrap;\n}\n.msg[_ngcontent-%COMP%]   .text.streaming[_ngcontent-%COMP%]   .streaming-cursor[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 6px;\n  height: 14px;\n  background: #c792ea;\n  margin-left: 2px;\n  vertical-align: text-bottom;\n  animation: _ngcontent-%COMP%_blink-cursor 0.8s step-end infinite;\n}\n\n@keyframes _ngcontent-%COMP%_blink-cursor {\n  0%, 100% {\n    opacity: 1;\n  }\n  50% {\n    opacity: 0;\n  }\n}\n.chat-empty[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 12px;\n  font-style: italic;\n  text-align: center;\n  padding: 24px 12px;\n}\n\n\n.chat-input[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-end;\n  padding: 8px 12px;\n  border-top: 1px solid #3c3c3c;\n  gap: 8px;\n  flex-shrink: 0;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%] {\n  flex: 1;\n  background: #3c3c3c;\n  border: 1px solid #555;\n  border-radius: 4px;\n  padding: 6px 10px;\n  color: #ccc;\n  font-size: 12px;\n  font-family: \"Segoe UI\", sans-serif;\n  outline: none;\n  resize: none;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%]:focus {\n  border-color: #888;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  background: #ff9800;\n  border: none;\n  color: #000;\n  padding: 6px 12px;\n  border-radius: 4px;\n  font-size: 12px;\n  cursor: pointer;\n  font-weight: 600;\n  transition: filter 0.15s;\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:hover:not(:disabled) {\n  filter: brightness(1.1);\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n\n\n.column-splitter[_ngcontent-%COMP%] {\n  width: 5px;\n  background: #2a2a2a;\n  cursor: col-resize;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: background 0.15s;\n  -webkit-user-select: none;\n          user-select: none;\n}\n.column-splitter[_ngcontent-%COMP%]:hover {\n  background: #ff9800;\n}\n.column-splitter[_ngcontent-%COMP%]:hover   .grip[_ngcontent-%COMP%] {\n  color: #000;\n}\n.column-splitter[_ngcontent-%COMP%]   .grip[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 10px;\n  writing-mode: vertical-lr;\n  letter-spacing: 2px;\n  pointer-events: none;\n}\n\n\n.prompt-column[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 200px;\n  min-height: 0;\n  overflow: hidden;\n}\n\n.prompt-content[_ngcontent-%COMP%] {\n  flex: 1;\n  padding: 16px;\n  overflow-y: auto;\n  min-height: 0;\n}\n\n.prompt-rendered[_ngcontent-%COMP%] {\n  font-size: 13px;\n  line-height: 1.7;\n  color: #e0e0e0;\n  cursor: pointer;\n  padding: 4px;\n  border-radius: 4px;\n  transition: background 0.15s;\n}\n.prompt-rendered[_ngcontent-%COMP%]:hover {\n  background: #2a2a2a;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h2[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  color: #fff;\n  margin: 12px 0 6px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h2[_ngcontent-%COMP%] {\n  font-size: 16px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  font-size: 13px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: #fff;\n}\n.prompt-rendered[_ngcontent-%COMP%]   em[_ngcontent-%COMP%] {\n  color: #ccc;\n  font-style: italic;\n}\n.prompt-rendered[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  background: #1e1e1e;\n  border: 1px solid #3c3c3c;\n  border-radius: 3px;\n  padding: 1px 4px;\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n  color: #ce9178;\n}\n.prompt-rendered[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  padding-left: 20px;\n  margin: 0;\n}\n.prompt-rendered[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin: 0;\n  padding: 0;\n  line-height: 1.4;\n}\n.prompt-rendered[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n}\n.prompt-rendered[_ngcontent-%COMP%]   .param-highlight[_ngcontent-%COMP%] {\n  background: #33302a;\n  border: 1px solid #ff9800;\n  border-radius: 3px;\n  padding: 1px 4px;\n  color: #ffcc80;\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n}\n\n.prompt-editor[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 200px;\n  height: 100%;\n  background: #1e1e1e;\n  border: 1px solid #ff9800;\n  border-radius: 4px;\n  padding: 8px;\n  color: #e0e0e0;\n  font-size: 13px;\n  font-family: \"Courier New\", monospace;\n  line-height: 1.7;\n  resize: none;\n  outline: none;\n  box-sizing: border-box;\n}\n\n\n.distillation-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  padding: 6px 12px;\n  font-size: 10px;\n  color: #ff9800;\n  border-top: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.distill-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 10px;\n  height: 10px;\n  border: 2px solid #555;\n  border-top-color: #ff9800;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_distill-spin 0.8s linear infinite;\n}\n\n@keyframes _ngcontent-%COMP%_distill-spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n.prompt-text[_ngcontent-%COMP%] {\n  font-size: 13px;\n  line-height: 1.8;\n  color: #e0e0e0;\n  white-space: pre-wrap;\n  outline: none;\n  border-radius: 4px;\n  padding: 4px;\n  transition: background 0.15s;\n  min-height: 60px;\n}\n.prompt-text[_ngcontent-%COMP%]:focus {\n  background: #2a2a2a;\n  box-shadow: 0 0 0 1px #ff9800 inset;\n}\n\n.prompt-note[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n  font-size: 10px;\n  color: #666;\n  flex-shrink: 0;\n}\n\n\n.ultimo-run[_ngcontent-%COMP%] {\n  border-top: 1px solid #3c3c3c;\n  background: #1e1e1e;\n  flex-shrink: 0;\n  max-height: 350px;\n  overflow-y: auto;\n}\n\n.run-section[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-bottom: 1px solid #2a2a2a;\n}\n.run-section[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n\n.run-section-title[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #888;\n  margin-bottom: 8px;\n}\n\n.run-meta[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 16px;\n  flex-wrap: wrap;\n}\n\n.run-meta-item[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: #aaa;\n}\n.run-meta-item[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: #ccc;\n}\n\n.run-params[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n\n.run-param[_ngcontent-%COMP%] {\n  font-size: 12px;\n  display: flex;\n  align-items: center;\n  gap: 6px;\n}\n\n.run-param-name[_ngcontent-%COMP%] {\n  color: #888;\n}\n\n.run-param-eq[_ngcontent-%COMP%] {\n  color: #555;\n}\n\n.run-param-val[_ngcontent-%COMP%] {\n  color: #e0e0e0;\n  font-weight: 500;\n}\n\n.run-prompt[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.7;\n  color: #ccc;\n  background: #252526;\n  border-radius: 4px;\n  padding: 12px;\n  white-space: pre-wrap;\n  border: 1px solid #3c3c3c;\n}\n\n.run-output[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.7;\n  color: #ccc;\n  background: #252526;\n  border-radius: 4px;\n  padding: 12px;\n  white-space: pre-wrap;\n  border: 1px solid #3c3c3c;\n  max-height: 200px;\n  overflow-y: auto;\n}\n\n\n.diagram-panel[_ngcontent-%COMP%] {\n  border-top: 1px solid #3c3c3c;\n  background: #1a1d21;\n  flex-shrink: 0;\n}\n\n.diagram-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 8px 16px;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.diagram-title[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #82b1ff;\n  flex: 1;\n}\n\n.diagram-copy-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 10px;\n  padding: 2px 8px;\n  border-radius: 3px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.diagram-copy-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #82b1ff;\n  color: #82b1ff;\n}\n.diagram-copy-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n\n.diagram-body[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n}\n\n.diagram-svg[_ngcontent-%COMP%] {\n  background: #fff;\n  border-radius: 4px;\n  padding: 12px;\n  overflow: auto;\n  max-height: 400px;\n  text-align: center;\n}\n.diagram-svg[_ngcontent-%COMP%]     svg {\n  max-width: 100%;\n  height: auto;\n}\n\n.diagram-code[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n  color: #ccc;\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 4px;\n  padding: 12px;\n  overflow-x: auto;\n  white-space: pre;\n  max-height: 300px;\n  overflow-y: auto;\n  margin: 0;\n}\n\n.diagram-loading[_ngcontent-%COMP%] {\n  color: #888;\n  font-size: 12px;\n  font-style: italic;\n}\n\n.diagram-empty[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 12px;\n}\n\n.diagram-error-panel[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.diagram-error-msg[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  color: #ef5350;\n  font-size: 12px;\n}\n\n.diagram-regen-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #ef5350;\n  color: #ef5350;\n  font-size: 11px;\n  padding: 3px 10px;\n  border-radius: 3px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.diagram-regen-btn[_ngcontent-%COMP%]:hover {\n  background: #ef5350;\n  color: #000;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiLWNhcmQvcHJvbXB0bGFiLWNhcmQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUEsZ0JBQWdCO0FBQWhCO0VBQ0UsY0FBQTtBQUVGOztBQUNBLDJCQUFBO0FBRUE7RUFDRSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxjQUFBO0VBQ0EsYUFBQTtBQUNGOztBQUVBLG1CQUFBO0FBRUE7RUFDRSxrQkFBQTtFQUNBLGdDQUFBO0VBQ0EsY0FBQTtBQUFGOztBQUdBLGNBQUE7QUFDQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxrQkFBQTtBQUFGO0FBRUU7RUFDRSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGNBQUE7QUFBSjtBQUdFO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsNEJBQUE7RUFDQSxlQUFBO0FBREo7QUFHSTtFQUNFLG1CQUFBO0VBQ0EsZ0NBQUE7QUFETjtBQUlJO0VBQ0UsK0JBQUE7RUFDQSxXQUFBO0FBRk47QUFNRTtFQUNFLE9BQUE7QUFKSjtBQU9FO0VBQ0UsZ0JBQUE7RUFDQSxZQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLHFCQUFBO0FBTEo7QUFPSTtFQUNFLGNBQUE7RUFDQSxrQ0FBQTtBQUxOOztBQVVBLG1CQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EsUUFBQTtFQUNBLGVBQUE7RUFDQSxtQkFBQTtBQVBGOztBQVVBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxpQkFBQTtFQUNBLG9CQUFBO0VBQ0EsbUJBQUE7RUFNQSxjQUFBO0VBZUEsK0JBQUE7RUFlQSxtQkFBQTtFQWVBLGNBQUE7QUF0REY7QUFLRTtFQUNFLHVCQUFBO0FBSEo7QUFRSTtFQUNFLG1CQUFBO0VBQ0EscUJBQUE7RUFDQSxvQkFBQTtFQUNBLGNBQUE7QUFOTjtBQVFJO0VBQ0UsbUJBQUE7RUFDQSxxQkFBQTtFQUNBLGNBQUE7QUFOTjtBQVlJO0VBQ0UsbUJBQUE7RUFDQSxxQkFBQTtFQUNBLG9CQUFBO0VBQ0EsY0FBQTtBQVZOO0FBWUk7RUFDRSxtQkFBQTtFQUNBLHFCQUFBO0VBQ0EsY0FBQTtBQVZOO0FBZ0JJO0VBQ0UsbUJBQUE7RUFDQSxxQkFBQTtFQUNBLG9CQUFBO0VBQ0EsY0FBQTtBQWROO0FBZ0JJO0VBQ0UsbUJBQUE7RUFDQSxxQkFBQTtFQUNBLGNBQUE7QUFkTjtBQW9CSTtFQUNFLG1CQUFBO0VBQ0EscUJBQUE7RUFDQSxvQkFBQTtFQUNBLGNBQUE7QUFsQk47QUFvQkk7RUFDRSxtQkFBQTtFQUNBLHFCQUFBO0VBQ0EsY0FBQTtBQWxCTjs7QUF1QkEsa0NBQUE7QUFDQTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7QUFwQkY7QUFzQkU7RUFDRSxnQkFBQTtFQUNBLFlBQUE7RUFDQSxjQUFBO0VBQ0EsZUFBQTtFQUNBLFlBQUE7RUFDQSxhQUFBO0FBcEJKO0FBdUJFO0VBQ0UsbUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtFQUNBLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0FBckJKO0FBdUJJO0VBQ0UsdUJBQUE7QUFyQk47O0FBMEJBLGdCQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EsUUFBQTtFQUNBLG1CQUFBO0FBdkJGOztBQTBCQTtFQUNFLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtFQUNBLHdCQUFBO0FBdkJGO0FBeUJFO0VBQ0UsdUJBQUE7QUF2Qko7QUEwQkU7RUFDRSxZQUFBO0VBQ0EsbUJBQUE7QUF4Qko7O0FBNEJBO0VBQ0UsZ0JBQUE7RUFDQSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxxQkFBQTtBQXpCRjtBQTJCRTtFQUNFLGtCQUFBO0VBQ0EsV0FBQTtBQXpCSjtBQTRCRTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtBQTFCSjtBQTZCRTtFQUNFLHFCQUFBO0VBQ0EsY0FBQTtBQTNCSjtBQThCRTtFQUNFLG1CQUFBO0VBQ0EscUJBQUE7RUFDQSxjQUFBO0FBNUJKOztBQWdDQTtFQUNFLFVBQUE7RUFDQSxZQUFBO0VBQ0EsbUJBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtBQTdCRjs7QUFnQ0E7RUFDRSxnQkFBQTtFQUNBLHNCQUFBO0VBQ0EsV0FBQTtFQUNBLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtFQUNBLHFCQUFBO0FBN0JGO0FBK0JFO0VBQ0Usa0JBQUE7RUFDQSxXQUFBO0FBN0JKO0FBZ0NFO0VBQ0UsWUFBQTtFQUNBLG1CQUFBO0FBOUJKO0FBaUNFO0VBQ0UscUJBQUE7RUFDQSxjQUFBO0VBQ0EsbUJBQUE7QUEvQko7O0FBbUNBO0VBQ0UscUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLHNCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLDRDQUFBO0VBQ0EsaUJBQUE7RUFDQSxzQkFBQTtBQWhDRjs7QUFtQ0E7RUFDRTtJQUFLLHlCQUFBO0VBL0JMO0FBQ0Y7QUFpQ0EsaUJBQUE7QUFFQTtFQUNFLGNBQUE7RUFDQSxhQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtFQUVBLHlCQUFBO0FBakNGO0FBbUNJO0VBQ0UsYUFBQTtBQWpDTjtBQW1DSTtFQUNFLGFBQUE7QUFqQ047QUFtQ0k7RUFDRSxPQUFBO0FBakNOO0FBbUNJO0VBQ0UsbUJBQUE7RUFDQSxxQkFBQTtFQUNBLGNBQUE7QUFqQ047O0FBc0NBLGdCQUFBO0FBQ0E7RUFDRSxPQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxhQUFBO0VBQ0EsZ0JBQUE7QUFuQ0Y7O0FBc0NBLHFCQUFBO0FBQ0E7RUFDRSxlQUFBO0VBQ0EseUJBQUE7RUFDQSxtQkFBQTtFQUNBLFdBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0NBQUE7RUFDQSxjQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQW5DRjtBQXFDRTtFQUNFLE9BQUE7QUFuQ0o7O0FBdUNBO0VBQ0UsZ0JBQUE7RUFDQSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLHFCQUFBO0FBcENGO0FBc0NFO0VBQ0UscUJBQUE7RUFDQSxjQUFBO0FBcENKOztBQXdDQTtFQUNFLGdCQUFBO0VBQ0Esc0JBQUE7RUFDQSxXQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxxQkFBQTtBQXJDRjtBQXVDRTtFQUNFLHFCQUFBO0VBQ0EsY0FBQTtBQXJDSjs7QUF5Q0Esa0JBQUE7QUFDQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7QUF0Q0Y7O0FBeUNBO0VBQ0UsbUJBQUE7QUF0Q0Y7QUF3Q0U7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtBQXRDSjtBQXdDSTtFQUNFLGNBQUE7QUF0Q047QUF5Q0k7RUFDRSxjQUFBO0FBdkNOO0FBMENJO0VBQ0UsV0FBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBeENOO0FBNENFO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLHFCQUFBO0FBMUNKO0FBNkNNO0VBQ0UscUJBQUE7RUFDQSxVQUFBO0VBQ0EsWUFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7RUFDQSwyQkFBQTtFQUNBLDhDQUFBO0FBM0NSOztBQWlEQTtFQUNFO0lBQVcsVUFBQTtFQTdDWDtFQThDQTtJQUFNLFVBQUE7RUEzQ047QUFDRjtBQTZDQTtFQUNFLFdBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0FBM0NGOztBQThDQSxlQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EscUJBQUE7RUFDQSxpQkFBQTtFQUNBLDZCQUFBO0VBQ0EsUUFBQTtFQUNBLGNBQUE7QUEzQ0Y7QUE2Q0U7RUFDRSxPQUFBO0VBQ0EsbUJBQUE7RUFDQSxzQkFBQTtFQUNBLGtCQUFBO0VBQ0EsaUJBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLG1DQUFBO0VBQ0EsYUFBQTtFQUNBLFlBQUE7QUEzQ0o7QUE2Q0k7RUFDRSxrQkFBQTtBQTNDTjtBQThDSTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtBQTVDTjtBQWdERTtFQUNFLG1CQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLHdCQUFBO0FBOUNKO0FBZ0RJO0VBQ0UsdUJBQUE7QUE5Q047QUFpREk7RUFDRSxZQUFBO0VBQ0EsbUJBQUE7QUEvQ047O0FBb0RBLGFBQUE7QUFDQTtFQUNFLFVBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsY0FBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsNEJBQUE7RUFDQSx5QkFBQTtVQUFBLGlCQUFBO0FBakRGO0FBbURFO0VBQ0UsbUJBQUE7QUFqREo7QUFtREk7RUFDRSxXQUFBO0FBakROO0FBcURFO0VBQ0UsV0FBQTtFQUNBLGVBQUE7RUFDQSx5QkFBQTtFQUNBLG1CQUFBO0VBQ0Esb0JBQUE7QUFuREo7O0FBdURBLGtCQUFBO0FBQ0E7RUFDRSxPQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxhQUFBO0VBQ0EsZ0JBQUE7QUFwREY7O0FBdURBO0VBQ0UsT0FBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7QUFwREY7O0FBdURBO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7RUFDQSxZQUFBO0VBQ0Esa0JBQUE7RUFDQSw0QkFBQTtBQXBERjtBQXNERTtFQUFVLG1CQUFBO0FBbkRaO0FBcURFO0VBQWEsV0FBQTtFQUFhLGtCQUFBO0FBakQ1QjtBQWtERTtFQUFLLGVBQUE7QUEvQ1A7QUFnREU7RUFBSyxlQUFBO0FBN0NQO0FBOENFO0VBQUssZUFBQTtBQTNDUDtBQTRDRTtFQUFTLFdBQUE7QUF6Q1g7QUEwQ0U7RUFBSyxXQUFBO0VBQWEsa0JBQUE7QUF0Q3BCO0FBdUNFO0VBQU8sbUJBQUE7RUFBcUIseUJBQUE7RUFBMkIsa0JBQUE7RUFBb0IsZ0JBQUE7RUFBa0IscUNBQUE7RUFBdUMsZUFBQTtFQUFpQixjQUFBO0FBOUJ2SjtBQStCRTtFQUFTLGtCQUFBO0VBQW9CLFNBQUE7QUEzQi9CO0FBNEJFO0VBQUssU0FBQTtFQUFXLFVBQUE7RUFBWSxnQkFBQTtBQXZCOUI7QUF3QkU7RUFBSSxTQUFBO0FBckJOO0FBdUJFO0VBQ0UsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0EscUNBQUE7RUFDQSxlQUFBO0FBckJKOztBQXlCQTtFQUNFLFdBQUE7RUFDQSxpQkFBQTtFQUNBLFlBQUE7RUFDQSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxZQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7RUFDQSxxQ0FBQTtFQUNBLGdCQUFBO0VBQ0EsWUFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtBQXRCRjs7QUF5QkEsMkJBQUE7QUFDQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxpQkFBQTtFQUNBLGVBQUE7RUFDQSxjQUFBO0VBQ0EsNkJBQUE7RUFDQSxjQUFBO0FBdEJGOztBQXlCQTtFQUNFLHFCQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxzQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSw0Q0FBQTtBQXRCRjs7QUF5QkE7RUFDRTtJQUFLLHlCQUFBO0VBckJMO0FBQ0Y7QUF1QkE7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0VBQ0EscUJBQUE7RUFDQSxhQUFBO0VBQ0Esa0JBQUE7RUFDQSxZQUFBO0VBQ0EsNEJBQUE7RUFDQSxnQkFBQTtBQXJCRjtBQXVCRTtFQUNFLG1CQUFBO0VBQ0EsbUNBQUE7QUFyQko7O0FBeUJBO0VBQ0Usa0JBQUE7RUFDQSw2QkFBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsY0FBQTtBQXRCRjs7QUF5QkEsd0NBQUE7QUFFQTtFQUNFLDZCQUFBO0VBQ0EsbUJBQUE7RUFDQSxjQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtBQXZCRjs7QUEwQkE7RUFDRSxrQkFBQTtFQUNBLGdDQUFBO0FBdkJGO0FBeUJFO0VBQ0UsbUJBQUE7QUF2Qko7O0FBMkJBO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0VBQ0EsbUJBQUE7RUFDQSxXQUFBO0VBQ0Esa0JBQUE7QUF4QkY7O0FBMkJBO0VBQ0UsYUFBQTtFQUNBLFNBQUE7RUFDQSxlQUFBO0FBeEJGOztBQTJCQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0FBeEJGO0FBMEJFO0VBQ0UsV0FBQTtBQXhCSjs7QUE0QkE7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBekJGOztBQTRCQTtFQUNFLGVBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0FBekJGOztBQTRCQTtFQUNFLFdBQUE7QUF6QkY7O0FBNEJBO0VBQ0UsV0FBQTtBQXpCRjs7QUE0QkE7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7QUF6QkY7O0FBNEJBO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLG1CQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EscUJBQUE7RUFDQSx5QkFBQTtBQXpCRjs7QUE0QkE7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxXQUFBO0VBQ0EsbUJBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxxQkFBQTtFQUNBLHlCQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQkFBQTtBQXpCRjs7QUE0QkEscUNBQUE7QUFFQTtFQUNFLDZCQUFBO0VBQ0EsbUJBQUE7RUFDQSxjQUFBO0FBMUJGOztBQTZCQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0NBQUE7QUExQkY7O0FBNkJBO0VBQ0UsZUFBQTtFQUNBLHlCQUFBO0VBQ0EsbUJBQUE7RUFDQSxjQUFBO0VBQ0EsT0FBQTtBQTFCRjs7QUE2QkE7RUFDRSxnQkFBQTtFQUNBLHNCQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLHFCQUFBO0FBMUJGO0FBNEJFO0VBQ0UscUJBQUE7RUFDQSxjQUFBO0FBMUJKO0FBNkJFO0VBQ0UsWUFBQTtFQUNBLG1CQUFBO0FBM0JKOztBQStCQTtFQUNFLGtCQUFBO0FBNUJGOztBQStCQTtFQUNFLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EsY0FBQTtFQUNBLGlCQUFBO0VBQ0Esa0JBQUE7QUE1QkY7QUE4QkU7RUFDRSxlQUFBO0VBQ0EsWUFBQTtBQTVCSjs7QUFnQ0E7RUFDRSxxQ0FBQTtFQUNBLGVBQUE7RUFDQSxXQUFBO0VBQ0EsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsYUFBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0VBQ0EsU0FBQTtBQTdCRjs7QUFnQ0E7RUFDRSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGtCQUFBO0FBN0JGOztBQWdDQTtFQUNFLFdBQUE7RUFDQSxlQUFBO0FBN0JGOztBQWdDQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLFFBQUE7QUE3QkY7O0FBZ0NBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsU0FBQTtFQUNBLGNBQUE7RUFDQSxlQUFBO0FBN0JGOztBQWdDQTtFQUNFLGdCQUFBO0VBQ0EseUJBQUE7RUFDQSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EscUJBQUE7QUE3QkY7QUErQkU7RUFDRSxtQkFBQTtFQUNBLFdBQUE7QUE3QkoiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgQ2FyZCBjb250YWluZXIgw6LClcKQw6LClcKQw6LClcKQICovXG5cbi5jYXJkIHtcbiAgYmFja2dyb3VuZDogIzI1MjUyNjtcbiAgYm9yZGVyOiAxcHggc29saWQgIzNjM2MzYztcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBmbGV4OiAxIDEgYXV0bztcbiAgbWluLWhlaWdodDogMDtcbn1cblxuLyogw6LClcKQw6LClcKQw6LClcKQIEhFQURFUiDDosKVwpDDosKVwpDDosKVwpAgKi9cblxuLmNhcmQtaGVhZGVyIHtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi8qIFRpdGxlIHJvdyAqL1xuLmNhcmQtdGl0bGUtcm93IHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG4gIG1hcmdpbi1ib3R0b206IDhweDtcblxuICAucGluIHtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgICBmb250LXNpemU6IDE2cHg7XG4gICAgZmxleC1zaHJpbms6IDA7XG4gIH1cblxuICAudGl0bGUge1xuICAgIGZvbnQtc2l6ZTogMTRweDtcbiAgICBjb2xvcjogI2ZmZjtcbiAgICBmb250LXdlaWdodDogNjAwO1xuICAgIG91dGxpbmU6IG5vbmU7XG4gICAgYm9yZGVyLXJhZGl1czogM3B4O1xuICAgIHBhZGRpbmc6IDJweCA0cHg7XG4gICAgdHJhbnNpdGlvbjogYmFja2dyb3VuZCAwLjE1cztcbiAgICBtaW4td2lkdGg6IDYwcHg7XG5cbiAgICAmOmZvY3VzIHtcbiAgICAgIGJhY2tncm91bmQ6ICMyYTJhMmE7XG4gICAgICBib3gtc2hhZG93OiAwIDAgMCAxcHggIzU1NSBpbnNldDtcbiAgICB9XG5cbiAgICAmOmVtcHR5OjpiZWZvcmUge1xuICAgICAgY29udGVudDogYXR0cihkYXRhLXBsYWNlaG9sZGVyKTtcbiAgICAgIGNvbG9yOiAjNTU1O1xuICAgIH1cbiAgfVxuXG4gIC5zcGFjZXIge1xuICAgIGZsZXg6IDE7XG4gIH1cblxuICAuZGVsZXRlLWJ0biB7XG4gICAgYmFja2dyb3VuZDogbm9uZTtcbiAgICBib3JkZXI6IG5vbmU7XG4gICAgY29sb3I6ICM2NjY7XG4gICAgZm9udC1zaXplOiAxNHB4O1xuICAgIGN1cnNvcjogcG9pbnRlcjtcbiAgICBwYWRkaW5nOiA0cHggNnB4O1xuICAgIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIGNvbG9yOiAjZWY1MzUwO1xuICAgICAgYmFja2dyb3VuZDogcmdiYSgyMzksIDgzLCA4MCwgMC4xKTtcbiAgICB9XG4gIH1cbn1cblxuLyogUGFyYW1ldGVycyByb3cgKi9cbi5jYXJkLXBhcmFtcyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGdhcDogOHB4O1xuICBmbGV4LXdyYXA6IHdyYXA7XG4gIG1hcmdpbi1ib3R0b206IDEwcHg7XG59XG5cbi5wYXJhbS1idG4ge1xuICBkaXNwbGF5OiBpbmxpbmUtZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA0cHg7XG4gIHBhZGRpbmc6IDRweCAxMHB4O1xuICBib3JkZXItcmFkaXVzOiAxNHB4O1xuICBmb250LXNpemU6IDExcHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgYm9yZGVyOiAxcHggc29saWQ7XG4gIHRyYW5zaXRpb246IGFsbCAwLjJzO1xuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuXG4gICY6aG92ZXIge1xuICAgIGZpbHRlcjogYnJpZ2h0bmVzcygxLjIpO1xuICB9XG5cbiAgLyogRmlsZSB0eXBlICovXG4gICYuZmlsZSB7XG4gICAgJi5lbXB0eSB7XG4gICAgICBiYWNrZ3JvdW5kOiAjMWEyNzMzO1xuICAgICAgYm9yZGVyLWNvbG9yOiAjMjk3OWZmO1xuICAgICAgYm9yZGVyLXN0eWxlOiBkYXNoZWQ7XG4gICAgICBjb2xvcjogIzVjOGNjNztcbiAgICB9XG4gICAgJi5maWxsZWQge1xuICAgICAgYmFja2dyb3VuZDogIzFlM2E1ZjtcbiAgICAgIGJvcmRlci1jb2xvcjogIzI5NzlmZjtcbiAgICAgIGNvbG9yOiAjODJiMWZmO1xuICAgIH1cbiAgfVxuXG4gIC8qIE91dHB1dCBmaWxlIHR5cGUgKFNhdmUgQXMpICovXG4gICYub3V0cHV0LWZpbGUge1xuICAgICYuZW1wdHkge1xuICAgICAgYmFja2dyb3VuZDogIzFhMzMyMDtcbiAgICAgIGJvcmRlci1jb2xvcjogIzRjYWY1MDtcbiAgICAgIGJvcmRlci1zdHlsZTogZGFzaGVkO1xuICAgICAgY29sb3I6ICM2ZDljNzA7XG4gICAgfVxuICAgICYuZmlsbGVkIHtcbiAgICAgIGJhY2tncm91bmQ6ICMxYjVlMjA7XG4gICAgICBib3JkZXItY29sb3I6ICM0Y2FmNTA7XG4gICAgICBjb2xvcjogI2E1ZDZhNztcbiAgICB9XG4gIH1cblxuICAvKiBEaXJlY3RvcnkgdHlwZSAqL1xuICAmLmRpcmVjdG9yeSB7XG4gICAgJi5lbXB0eSB7XG4gICAgICBiYWNrZ3JvdW5kOiAjMmEyMzIwO1xuICAgICAgYm9yZGVyLWNvbG9yOiAjOGQ2ZTYzO1xuICAgICAgYm9yZGVyLXN0eWxlOiBkYXNoZWQ7XG4gICAgICBjb2xvcjogIzljN2Q2ZDtcbiAgICB9XG4gICAgJi5maWxsZWQge1xuICAgICAgYmFja2dyb3VuZDogIzNlMjcyMztcbiAgICAgIGJvcmRlci1jb2xvcjogIzhkNmU2MztcbiAgICAgIGNvbG9yOiAjZDdjY2M4O1xuICAgIH1cbiAgfVxuXG4gIC8qIFRleHQgdHlwZSAqL1xuICAmLnRleHQge1xuICAgICYuZW1wdHkge1xuICAgICAgYmFja2dyb3VuZDogIzMzMzAyYTtcbiAgICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICAgIGJvcmRlci1zdHlsZTogZGFzaGVkO1xuICAgICAgY29sb3I6ICNhNjhjNWM7XG4gICAgfVxuICAgICYuZmlsbGVkIHtcbiAgICAgIGJhY2tncm91bmQ6ICMzZDM1MjA7XG4gICAgICBib3JkZXItY29sb3I6ICNmZjk4MDA7XG4gICAgICBjb2xvcjogI2ZmZTBiMjtcbiAgICB9XG4gIH1cbn1cblxuLyogSW5saW5lIGVkaXRvciBmb3IgdGV4dCBwYXJhbXMgKi9cbi5pbmxpbmUtZWRpdG9yIHtcbiAgZGlzcGxheTogaW5saW5lLWZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogNHB4O1xuICBiYWNrZ3JvdW5kOiAjM2QzNTIwO1xuICBib3JkZXI6IDFweCBzb2xpZCAjZmY5ODAwO1xuICBib3JkZXItcmFkaXVzOiAxNnB4O1xuICBwYWRkaW5nOiA0cHggOHB4O1xuXG4gIGlucHV0IHtcbiAgICBiYWNrZ3JvdW5kOiBub25lO1xuICAgIGJvcmRlcjogbm9uZTtcbiAgICBjb2xvcjogI2ZmZTBiMjtcbiAgICBmb250LXNpemU6IDEycHg7XG4gICAgd2lkdGg6IDEwMHB4O1xuICAgIG91dGxpbmU6IG5vbmU7XG4gIH1cblxuICAuY29uZmlybSB7XG4gICAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgICBjb2xvcjogIzAwMDtcbiAgICBib3JkZXI6IG5vbmU7XG4gICAgYm9yZGVyLXJhZGl1czogMTBweDtcbiAgICB3aWR0aDogMjBweDtcbiAgICBoZWlnaHQ6IDIwcHg7XG4gICAgZm9udC1zaXplOiAxMnB4O1xuICAgIGN1cnNvcjogcG9pbnRlcjtcbiAgICBsaW5lLWhlaWdodDogMjBweDtcbiAgICB0ZXh0LWFsaWduOiBjZW50ZXI7XG4gICAgZmxleC1zaHJpbms6IDA7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIGZpbHRlcjogYnJpZ2h0bmVzcygxLjEpO1xuICAgIH1cbiAgfVxufVxuXG4vKiBBY3Rpb25zIHJvdyAqL1xuLmNhcmQtYWN0aW9ucyB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGdhcDogOHB4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xufVxuXG4ucGxheS1idG4ge1xuICBiYWNrZ3JvdW5kOiAjZmY5ODAwO1xuICBjb2xvcjogIzAwMDtcbiAgYm9yZGVyOiBub25lO1xuICBwYWRkaW5nOiA2cHggMTZweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBmb250LXNpemU6IDEycHg7XG4gIGZvbnQtd2VpZ2h0OiA2MDA7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgdHJhbnNpdGlvbjogZmlsdGVyIDAuMTVzO1xuXG4gICY6aG92ZXI6bm90KDpkaXNhYmxlZCkge1xuICAgIGZpbHRlcjogYnJpZ2h0bmVzcygxLjEpO1xuICB9XG5cbiAgJjpkaXNhYmxlZCB7XG4gICAgb3BhY2l0eTogMC41O1xuICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gIH1cbn1cblxuLmxhc3RydW4tYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIHBhZGRpbmc6IDVweCAxMnB4O1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJjpob3Zlcjpub3QoOmRpc2FibGVkKSB7XG4gICAgYm9yZGVyLWNvbG9yOiAjODg4O1xuICAgIGNvbG9yOiAjY2NjO1xuICB9XG5cbiAgJjpkaXNhYmxlZCB7XG4gICAgb3BhY2l0eTogMC40O1xuICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gIH1cblxuICAmLmhhcy1ydW4ge1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgfVxuXG4gICYuYWN0aXZlIHtcbiAgICBiYWNrZ3JvdW5kOiAjMzMyODBhO1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgfVxufVxuXG4uYWN0aW9uLXNlcGFyYXRvciB7XG4gIHdpZHRoOiAxcHg7XG4gIGhlaWdodDogMjBweDtcbiAgYmFja2dyb3VuZDogIzNjM2MzYztcbiAgbWFyZ2luOiAwIDRweDtcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi5kaWFncmFtLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM0NDQ7XG4gIGNvbG9yOiAjYWFhO1xuICBwYWRkaW5nOiA0cHggMTBweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBmb250LXNpemU6IDEwcHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICY6aG92ZXI6bm90KDpkaXNhYmxlZCkge1xuICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgICBjb2xvcjogI2NjYztcbiAgfVxuXG4gICY6ZGlzYWJsZWQge1xuICAgIG9wYWNpdHk6IDAuNDtcbiAgICBjdXJzb3I6IG5vdC1hbGxvd2VkO1xuICB9XG5cbiAgJi5hY3RpdmUge1xuICAgIGJvcmRlci1jb2xvcjogIzgyYjFmZjtcbiAgICBjb2xvcjogIzgyYjFmZjtcbiAgICBiYWNrZ3JvdW5kOiAjMWEyNzMzO1xuICB9XG59XG5cbi5kaWFncmFtLXNwaW5uZXIge1xuICBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7XG4gIHdpZHRoOiAxMHB4O1xuICBoZWlnaHQ6IDEwcHg7XG4gIGJvcmRlcjogMnB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci10b3AtY29sb3I6ICM4MmIxZmY7XG4gIGJvcmRlci1yYWRpdXM6IDUwJTtcbiAgYW5pbWF0aW9uOiBkaWFncmFtLXNwaW4gMC44cyBsaW5lYXIgaW5maW5pdGU7XG4gIG1hcmdpbi1yaWdodDogNHB4O1xuICB2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO1xufVxuXG5Aa2V5ZnJhbWVzIGRpYWdyYW0tc3BpbiB7XG4gIHRvIHsgdHJhbnNmb3JtOiByb3RhdGUoMzYwZGVnKTsgfVxufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgQk9EWSDDosKVwpDDosKVwpDDosKVwpAgKi9cblxuLmNhcmQtYm9keSB7XG4gIGZsZXg6IDEgMSBhdXRvO1xuICBkaXNwbGF5OiBmbGV4O1xuICBtaW4taGVpZ2h0OiA0MDBweDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcblxuICAvKiBDb2xsYXBzZWQgY2hhdCBzdGF0ZSAqL1xuICAmLmNoYXQtY29sbGFwc2VkIHtcbiAgICAuY2hhdC1jb2x1bW4ge1xuICAgICAgZGlzcGxheTogbm9uZTtcbiAgICB9XG4gICAgLmNvbHVtbi1zcGxpdHRlciB7XG4gICAgICBkaXNwbGF5OiBub25lO1xuICAgIH1cbiAgICAucHJvbXB0LWNvbHVtbiB7XG4gICAgICBmbGV4OiAxO1xuICAgIH1cbiAgICAuY29sbGFwc2UtY2hhdC1idG4ge1xuICAgICAgYmFja2dyb3VuZDogIzMzMjgwYTtcbiAgICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgIH1cbiAgfVxufVxuXG4vKiBDaGF0IGNvbHVtbiAqL1xuLmNoYXQtY29sdW1uIHtcbiAgZmxleDogMTtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgbWluLXdpZHRoOiAyMDBweDtcbiAgbWluLWhlaWdodDogMDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcbn1cblxuLyogQ29sdW1uIGxhYmVsIGJhciAqL1xuLmNvbHVtbi1sYWJlbCB7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgbGV0dGVyLXNwYWNpbmc6IDFweDtcbiAgY29sb3I6ICM4ODg7XG4gIHBhZGRpbmc6IDhweCAxMnB4O1xuICBiYWNrZ3JvdW5kOiAjMmEyYTJhO1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbiAgZmxleC1zaHJpbms6IDA7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogNnB4O1xuXG4gIC5sYWJlbC10ZXh0IHtcbiAgICBmbGV4OiAxO1xuICB9XG59XG5cbi5yZXNldC1jaGF0LWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGNvbG9yOiAjYWFhO1xuICBmb250LXNpemU6IDlweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiAycHggOHB4O1xuICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyIHtcbiAgICBib3JkZXItY29sb3I6ICNlZjUzNTA7XG4gICAgY29sb3I6ICNlZjUzNTA7XG4gIH1cbn1cblxuLmNvbGxhcHNlLWNoYXQtYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIGZvbnQtc2l6ZTogOXB4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHBhZGRpbmc6IDJweCA4cHg7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICY6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgfVxufVxuXG4vKiBDaGF0IG1lc3NhZ2VzICovXG4uY2hhdC1tZXNzYWdlcyB7XG4gIGZsZXg6IDE7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIHBhZGRpbmc6IDEycHg7XG59XG5cbi5tc2cge1xuICBtYXJnaW4tYm90dG9tOiAxMnB4O1xuXG4gIC5hdXRob3Ige1xuICAgIGZvbnQtc2l6ZTogMTFweDtcbiAgICBmb250LXdlaWdodDogNjAwO1xuICAgIG1hcmdpbi1ib3R0b206IDJweDtcblxuICAgICYudXNlciB7XG4gICAgICBjb2xvcjogIzgyYjFmZjtcbiAgICB9XG5cbiAgICAmLmFzc2lzdGFudCB7XG4gICAgICBjb2xvcjogI2M3OTJlYTtcbiAgICB9XG5cbiAgICAudGltZSB7XG4gICAgICBjb2xvcjogIzY2NjtcbiAgICAgIGZvbnQtd2VpZ2h0OiA0MDA7XG4gICAgICBmb250LXNpemU6IDEwcHg7XG4gICAgICBtYXJnaW4tbGVmdDogNnB4O1xuICAgIH1cbiAgfVxuXG4gIC50ZXh0IHtcbiAgICBmb250LXNpemU6IDEycHg7XG4gICAgbGluZS1oZWlnaHQ6IDEuNTtcbiAgICBjb2xvcjogI2NjYztcbiAgICB3aGl0ZS1zcGFjZTogcHJlLXdyYXA7XG5cbiAgICAmLnN0cmVhbWluZyB7XG4gICAgICAuc3RyZWFtaW5nLWN1cnNvciB7XG4gICAgICAgIGRpc3BsYXk6IGlubGluZS1ibG9jaztcbiAgICAgICAgd2lkdGg6IDZweDtcbiAgICAgICAgaGVpZ2h0OiAxNHB4O1xuICAgICAgICBiYWNrZ3JvdW5kOiAjYzc5MmVhO1xuICAgICAgICBtYXJnaW4tbGVmdDogMnB4O1xuICAgICAgICB2ZXJ0aWNhbC1hbGlnbjogdGV4dC1ib3R0b207XG4gICAgICAgIGFuaW1hdGlvbjogYmxpbmstY3Vyc29yIDAuOHMgc3RlcC1lbmQgaW5maW5pdGU7XG4gICAgICB9XG4gICAgfVxuICB9XG59XG5cbkBrZXlmcmFtZXMgYmxpbmstY3Vyc29yIHtcbiAgMCUsIDEwMCUgeyBvcGFjaXR5OiAxOyB9XG4gIDUwJSB7IG9wYWNpdHk6IDA7IH1cbn1cblxuLmNoYXQtZW1wdHkge1xuICBjb2xvcjogIzU1NTtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBmb250LXN0eWxlOiBpdGFsaWM7XG4gIHRleHQtYWxpZ246IGNlbnRlcjtcbiAgcGFkZGluZzogMjRweCAxMnB4O1xufVxuXG4vKiBDaGF0IGlucHV0ICovXG4uY2hhdC1pbnB1dCB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBmbGV4LWVuZDtcbiAgcGFkZGluZzogOHB4IDEycHg7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBnYXA6IDhweDtcbiAgZmxleC1zaHJpbms6IDA7XG5cbiAgdGV4dGFyZWEge1xuICAgIGZsZXg6IDE7XG4gICAgYmFja2dyb3VuZDogIzNjM2MzYztcbiAgICBib3JkZXI6IDFweCBzb2xpZCAjNTU1O1xuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgICBwYWRkaW5nOiA2cHggMTBweDtcbiAgICBjb2xvcjogI2NjYztcbiAgICBmb250LXNpemU6IDEycHg7XG4gICAgZm9udC1mYW1pbHk6ICdTZWdvZSBVSScsIHNhbnMtc2VyaWY7XG4gICAgb3V0bGluZTogbm9uZTtcbiAgICByZXNpemU6IG5vbmU7XG5cbiAgICAmOmZvY3VzIHtcbiAgICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgICB9XG5cbiAgICAmOmRpc2FibGVkIHtcbiAgICAgIG9wYWNpdHk6IDAuNTtcbiAgICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gICAgfVxuICB9XG5cbiAgYnV0dG9uIHtcbiAgICBiYWNrZ3JvdW5kOiAjZmY5ODAwO1xuICAgIGJvcmRlcjogbm9uZTtcbiAgICBjb2xvcjogIzAwMDtcbiAgICBwYWRkaW5nOiA2cHggMTJweDtcbiAgICBib3JkZXItcmFkaXVzOiA0cHg7XG4gICAgZm9udC1zaXplOiAxMnB4O1xuICAgIGN1cnNvcjogcG9pbnRlcjtcbiAgICBmb250LXdlaWdodDogNjAwO1xuICAgIHRyYW5zaXRpb246IGZpbHRlciAwLjE1cztcblxuICAgICY6aG92ZXI6bm90KDpkaXNhYmxlZCkge1xuICAgICAgZmlsdGVyOiBicmlnaHRuZXNzKDEuMSk7XG4gICAgfVxuXG4gICAgJjpkaXNhYmxlZCB7XG4gICAgICBvcGFjaXR5OiAwLjU7XG4gICAgICBjdXJzb3I6IG5vdC1hbGxvd2VkO1xuICAgIH1cbiAgfVxufVxuXG4vKiBTcGxpdHRlciAqL1xuLmNvbHVtbi1zcGxpdHRlciB7XG4gIHdpZHRoOiA1cHg7XG4gIGJhY2tncm91bmQ6ICMyYTJhMmE7XG4gIGN1cnNvcjogY29sLXJlc2l6ZTtcbiAgZmxleC1zaHJpbms6IDA7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuMTVzO1xuICB1c2VyLXNlbGVjdDogbm9uZTtcblxuICAmOmhvdmVyIHtcbiAgICBiYWNrZ3JvdW5kOiAjZmY5ODAwO1xuXG4gICAgLmdyaXAge1xuICAgICAgY29sb3I6ICMwMDA7XG4gICAgfVxuICB9XG5cbiAgLmdyaXAge1xuICAgIGNvbG9yOiAjNTU1O1xuICAgIGZvbnQtc2l6ZTogMTBweDtcbiAgICB3cml0aW5nLW1vZGU6IHZlcnRpY2FsLWxyO1xuICAgIGxldHRlci1zcGFjaW5nOiAycHg7XG4gICAgcG9pbnRlci1ldmVudHM6IG5vbmU7XG4gIH1cbn1cblxuLyogUHJvbXB0IGNvbHVtbiAqL1xuLnByb21wdC1jb2x1bW4ge1xuICBmbGV4OiAxO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBtaW4td2lkdGg6IDIwMHB4O1xuICBtaW4taGVpZ2h0OiAwO1xuICBvdmVyZmxvdzogaGlkZGVuO1xufVxuXG4ucHJvbXB0LWNvbnRlbnQge1xuICBmbGV4OiAxO1xuICBwYWRkaW5nOiAxNnB4O1xuICBvdmVyZmxvdy15OiBhdXRvO1xuICBtaW4taGVpZ2h0OiAwO1xufVxuXG4ucHJvbXB0LXJlbmRlcmVkIHtcbiAgZm9udC1zaXplOiAxM3B4O1xuICBsaW5lLWhlaWdodDogMS43O1xuICBjb2xvcjogI2UwZTBlMDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiA0cHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgdHJhbnNpdGlvbjogYmFja2dyb3VuZCAwLjE1cztcblxuICAmOmhvdmVyIHsgYmFja2dyb3VuZDogIzJhMmEyYTsgfVxuXG4gIGgyLCBoMywgaDQgeyBjb2xvcjogI2ZmZjsgbWFyZ2luOiAxMnB4IDAgNnB4OyB9XG4gIGgyIHsgZm9udC1zaXplOiAxNnB4OyB9XG4gIGgzIHsgZm9udC1zaXplOiAxNHB4OyB9XG4gIGg0IHsgZm9udC1zaXplOiAxM3B4OyB9XG4gIHN0cm9uZyB7IGNvbG9yOiAjZmZmOyB9XG4gIGVtIHsgY29sb3I6ICNjY2M7IGZvbnQtc3R5bGU6IGl0YWxpYzsgfVxuICBjb2RlIHsgYmFja2dyb3VuZDogIzFlMWUxZTsgYm9yZGVyOiAxcHggc29saWQgIzNjM2MzYzsgYm9yZGVyLXJhZGl1czogM3B4OyBwYWRkaW5nOiAxcHggNHB4OyBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlOyBmb250LXNpemU6IDEycHg7IGNvbG9yOiAjY2U5MTc4OyB9XG4gIHVsLCBvbCB7IHBhZGRpbmctbGVmdDogMjBweDsgbWFyZ2luOiAwOyB9XG4gIGxpIHsgbWFyZ2luOiAwOyBwYWRkaW5nOiAwOyBsaW5lLWhlaWdodDogMS40OyB9XG4gIHAgeyBtYXJnaW46IDA7IH1cblxuICAucGFyYW0taGlnaGxpZ2h0IHtcbiAgICBiYWNrZ3JvdW5kOiAjMzMzMDJhO1xuICAgIGJvcmRlcjogMXB4IHNvbGlkICNmZjk4MDA7XG4gICAgYm9yZGVyLXJhZGl1czogM3B4O1xuICAgIHBhZGRpbmc6IDFweCA0cHg7XG4gICAgY29sb3I6ICNmZmNjODA7XG4gICAgZm9udC1mYW1pbHk6ICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcbiAgICBmb250LXNpemU6IDEycHg7XG4gIH1cbn1cblxuLnByb21wdC1lZGl0b3Ige1xuICB3aWR0aDogMTAwJTtcbiAgbWluLWhlaWdodDogMjAwcHg7XG4gIGhlaWdodDogMTAwJTtcbiAgYmFja2dyb3VuZDogIzFlMWUxZTtcbiAgYm9yZGVyOiAxcHggc29saWQgI2ZmOTgwMDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBwYWRkaW5nOiA4cHg7XG4gIGNvbG9yOiAjZTBlMGUwO1xuICBmb250LXNpemU6IDEzcHg7XG4gIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XG4gIGxpbmUtaGVpZ2h0OiAxLjc7XG4gIHJlc2l6ZTogbm9uZTtcbiAgb3V0bGluZTogbm9uZTtcbiAgYm94LXNpemluZzogYm9yZGVyLWJveDtcbn1cblxuLyogRGlzdGlsbGF0aW9uIGluZGljYXRvciAqL1xuLmRpc3RpbGxhdGlvbi1pbmRpY2F0b3Ige1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDZweDtcbiAgcGFkZGluZzogNnB4IDEycHg7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgY29sb3I6ICNmZjk4MDA7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmRpc3RpbGwtc3Bpbm5lciB7XG4gIGRpc3BsYXk6IGlubGluZS1ibG9jaztcbiAgd2lkdGg6IDEwcHg7XG4gIGhlaWdodDogMTBweDtcbiAgYm9yZGVyOiAycHggc29saWQgIzU1NTtcbiAgYm9yZGVyLXRvcC1jb2xvcjogI2ZmOTgwMDtcbiAgYm9yZGVyLXJhZGl1czogNTAlO1xuICBhbmltYXRpb246IGRpc3RpbGwtc3BpbiAwLjhzIGxpbmVhciBpbmZpbml0ZTtcbn1cblxuQGtleWZyYW1lcyBkaXN0aWxsLXNwaW4ge1xuICB0byB7IHRyYW5zZm9ybTogcm90YXRlKDM2MGRlZyk7IH1cbn1cblxuLnByb21wdC10ZXh0IHtcbiAgZm9udC1zaXplOiAxM3B4O1xuICBsaW5lLWhlaWdodDogMS44O1xuICBjb2xvcjogI2UwZTBlMDtcbiAgd2hpdGUtc3BhY2U6IHByZS13cmFwO1xuICBvdXRsaW5lOiBub25lO1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIHBhZGRpbmc6IDRweDtcbiAgdHJhbnNpdGlvbjogYmFja2dyb3VuZCAwLjE1cztcbiAgbWluLWhlaWdodDogNjBweDtcblxuICAmOmZvY3VzIHtcbiAgICBiYWNrZ3JvdW5kOiAjMmEyYTJhO1xuICAgIGJveC1zaGFkb3c6IDAgMCAwIDFweCAjZmY5ODAwIGluc2V0O1xuICB9XG59XG5cbi5wcm9tcHQtbm90ZSB7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgY29sb3I6ICM2NjY7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgVWx0aW1vIFJ1biBQYW5lbCAoVGFzayA4LjIpIMOiwpXCkMOiwpXCkMOiwpXCkCAqL1xuXG4udWx0aW1vLXJ1biB7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBiYWNrZ3JvdW5kOiAjMWUxZTFlO1xuICBmbGV4LXNocmluazogMDtcbiAgbWF4LWhlaWdodDogMzUwcHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG59XG5cbi5ydW4tc2VjdGlvbiB7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICMyYTJhMmE7XG5cbiAgJjpsYXN0LWNoaWxkIHtcbiAgICBib3JkZXItYm90dG9tOiBub25lO1xuICB9XG59XG5cbi5ydW4tc2VjdGlvbi10aXRsZSB7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgbGV0dGVyLXNwYWNpbmc6IDFweDtcbiAgY29sb3I6ICM4ODg7XG4gIG1hcmdpbi1ib3R0b206IDhweDtcbn1cblxuLnJ1bi1tZXRhIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZ2FwOiAxNnB4O1xuICBmbGV4LXdyYXA6IHdyYXA7XG59XG5cbi5ydW4tbWV0YS1pdGVtIHtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBjb2xvcjogI2FhYTtcblxuICBzdHJvbmcge1xuICAgIGNvbG9yOiAjY2NjO1xuICB9XG59XG5cbi5ydW4tcGFyYW1zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgZ2FwOiA0cHg7XG59XG5cbi5ydW4tcGFyYW0ge1xuICBmb250LXNpemU6IDEycHg7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogNnB4O1xufVxuXG4ucnVuLXBhcmFtLW5hbWUge1xuICBjb2xvcjogIzg4ODtcbn1cblxuLnJ1bi1wYXJhbS1lcSB7XG4gIGNvbG9yOiAjNTU1O1xufVxuXG4ucnVuLXBhcmFtLXZhbCB7XG4gIGNvbG9yOiAjZTBlMGUwO1xuICBmb250LXdlaWdodDogNTAwO1xufVxuXG4ucnVuLXByb21wdCB7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgbGluZS1oZWlnaHQ6IDEuNztcbiAgY29sb3I6ICNjY2M7XG4gIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgcGFkZGluZzogMTJweDtcbiAgd2hpdGUtc3BhY2U6IHByZS13cmFwO1xuICBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjO1xufVxuXG4ucnVuLW91dHB1dCB7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgbGluZS1oZWlnaHQ6IDEuNztcbiAgY29sb3I6ICNjY2M7XG4gIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgcGFkZGluZzogMTJweDtcbiAgd2hpdGUtc3BhY2U6IHByZS13cmFwO1xuICBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBtYXgtaGVpZ2h0OiAyMDBweDtcbiAgb3ZlcmZsb3cteTogYXV0bztcbn1cblxuLyogw6LClcKQw6LClcKQw6LClcKQIERpYWdyYW0gUGFuZWwgKFRhc2sgOS4xKSDDosKVwpDDosKVwpDDosKVwpAgKi9cblxuLmRpYWdyYW0tcGFuZWwge1xuICBib3JkZXItdG9wOiAxcHggc29saWQgIzNjM2MzYztcbiAgYmFja2dyb3VuZDogIzFhMWQyMTtcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi5kaWFncmFtLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDhweCAxNnB4O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbn1cblxuLmRpYWdyYW0tdGl0bGUge1xuICBmb250LXNpemU6IDEwcHg7XG4gIHRleHQtdHJhbnNmb3JtOiB1cHBlcmNhc2U7XG4gIGxldHRlci1zcGFjaW5nOiAxcHg7XG4gIGNvbG9yOiAjODJiMWZmO1xuICBmbGV4OiAxO1xufVxuXG4uZGlhZ3JhbS1jb3B5LWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGNvbG9yOiAjYWFhO1xuICBmb250LXNpemU6IDEwcHg7XG4gIHBhZGRpbmc6IDJweCA4cHg7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJjpob3Zlcjpub3QoOmRpc2FibGVkKSB7XG4gICAgYm9yZGVyLWNvbG9yOiAjODJiMWZmO1xuICAgIGNvbG9yOiAjODJiMWZmO1xuICB9XG5cbiAgJjpkaXNhYmxlZCB7XG4gICAgb3BhY2l0eTogMC40O1xuICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gIH1cbn1cblxuLmRpYWdyYW0tYm9keSB7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbn1cblxuLmRpYWdyYW0tc3ZnIHtcbiAgYmFja2dyb3VuZDogI2ZmZjtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBwYWRkaW5nOiAxMnB4O1xuICBvdmVyZmxvdzogYXV0bztcbiAgbWF4LWhlaWdodDogNDAwcHg7XG4gIHRleHQtYWxpZ246IGNlbnRlcjtcblxuICA6Om5nLWRlZXAgc3ZnIHtcbiAgICBtYXgtd2lkdGg6IDEwMCU7XG4gICAgaGVpZ2h0OiBhdXRvO1xuICB9XG59XG5cbi5kaWFncmFtLWNvZGUge1xuICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xuICBmb250LXNpemU6IDEycHg7XG4gIGNvbG9yOiAjY2NjO1xuICBiYWNrZ3JvdW5kOiAjMjUyNTI2O1xuICBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIHBhZGRpbmc6IDEycHg7XG4gIG92ZXJmbG93LXg6IGF1dG87XG4gIHdoaXRlLXNwYWNlOiBwcmU7XG4gIG1heC1oZWlnaHQ6IDMwMHB4O1xuICBvdmVyZmxvdy15OiBhdXRvO1xuICBtYXJnaW46IDA7XG59XG5cbi5kaWFncmFtLWxvYWRpbmcge1xuICBjb2xvcjogIzg4ODtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBmb250LXN0eWxlOiBpdGFsaWM7XG59XG5cbi5kaWFncmFtLWVtcHR5IHtcbiAgY29sb3I6ICM1NTU7XG4gIGZvbnQtc2l6ZTogMTJweDtcbn1cblxuLmRpYWdyYW0tZXJyb3ItcGFuZWwge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDhweDtcbn1cblxuLmRpYWdyYW0tZXJyb3ItbXNnIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiAxMHB4O1xuICBjb2xvcjogI2VmNTM1MDtcbiAgZm9udC1zaXplOiAxMnB4O1xufVxuXG4uZGlhZ3JhbS1yZWdlbi1idG4ge1xuICBiYWNrZ3JvdW5kOiBub25lO1xuICBib3JkZXI6IDFweCBzb2xpZCAjZWY1MzUwO1xuICBjb2xvcjogI2VmNTM1MDtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBwYWRkaW5nOiAzcHggMTBweDtcbiAgYm9yZGVyLXJhZGl1czogM3B4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyIHtcbiAgICBiYWNrZ3JvdW5kOiAjZWY1MzUwO1xuICAgIGNvbG9yOiAjMDAwO1xuICB9XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"],
      changeDetection: 0
    });
  }
}

/***/ }),

/***/ 1381:
/*!*******************************************************************************************!*\
  !*** ./src/app/promptlab/components/promptlab-doc-panel/promptlab-doc-panel.component.ts ***!
  \*******************************************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabDocPanelComponent": () => (/* binding */ PromptLabDocPanelComponent)
/* harmony export */ });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @ngx-translate/core */ 8699);



function PromptLabDocPanelComponent_div_14_div_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](2, 1, "PROMPTLAB_DOC.NO_SLOTS"), " ");
  }
}
function PromptLabDocPanelComponent_div_14_div_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 13)(1, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "div", 15)(4, "div", 16)(5, "span", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](7, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](10, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12, "\u25B6");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const card_r3 = ctx.$implicit;
    const i_r4 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](i_r4 + 1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](card_r3.generatedTitle || _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](7, 3, "PROMPTLAB_DOC.CARD_PREFIX") + (i_r4 + 1));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](card_r3.distilledPrompt || _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](10, 5, "PROMPTLAB_DOC.EMPTY_PROMPT"));
  }
}
function PromptLabDocPanelComponent_div_14_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, PromptLabDocPanelComponent_div_14_div_1_Template, 3, 3, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](2, PromptLabDocPanelComponent_div_14_div_2_Template, 13, 7, "div", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", ctx_r0.cards.length === 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx_r0.cards);
  }
}
class PromptLabDocPanelComponent {
  constructor() {
    this.cards = [];
    this.templateName = '';
    this.collapsed = false;
  }
  toggleCollapse() {
    this.collapsed = !this.collapsed;
  }
  static {
    this.ɵfac = function PromptLabDocPanelComponent_Factory(t) {
      return new (t || PromptLabDocPanelComponent)();
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({
      type: PromptLabDocPanelComponent,
      selectors: [["app-promptlab-doc-panel"]],
      inputs: {
        cards: "cards",
        templateName: "templateName"
      },
      decls: 15,
      vars: 8,
      consts: [[1, "doc-template"], [1, "doc-template-header"], [1, "doc-icon"], [1, "doc-name"], [1, "spacer"], [1, "doc-status"], [1, "count"], [1, "collapse-btn", 3, "click"], ["class", "doc-template-body", 4, "ngIf"], [1, "doc-template-body"], ["class", "empty-slots", 4, "ngIf"], ["class", "doc-prompt-slot", 4, "ngFor", "ngForOf"], [1, "empty-slots"], [1, "doc-prompt-slot"], [1, "slot-number"], [1, "slot-content"], [1, "slot-title"], [1, "card-link"], [1, "slot-prompt"], [1, "slot-arrow"]],
      template: function PromptLabDocPanelComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "span", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "\uF4C4");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](6, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "span", 5)(8, "span", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](11, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "button", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function PromptLabDocPanelComponent_Template_button_click_12_listener() {
            return ctx.toggleCollapse();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](13, "\u25BC");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](14, PromptLabDocPanelComponent_div_14_Template, 3, 2, "div", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵclassProp"]("collapsed", ctx.collapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.templateName);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.cards.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](11, 6, "PROMPTLAB_DOC.PROMPT_UNIT"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx.collapsed);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_1__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_1__.NgIf, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_2__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: block;\n  padding: 0 16px;\n  flex-shrink: 0;\n}\n\n.doc-template[_ngcontent-%COMP%] {\n  background: #1a1d21;\n  border: 2px solid #ff9800;\n  border-radius: 8px;\n  margin: 16px 0;\n  overflow: hidden;\n}\n\n.doc-template-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  padding: 10px 16px;\n  background: #252526;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.doc-icon[_ngcontent-%COMP%] {\n  color: #ff9800;\n  font-size: 16px;\n}\n\n.doc-name[_ngcontent-%COMP%] {\n  color: #fff;\n  font-size: 13px;\n  font-weight: 600;\n}\n\n.spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.doc-status[_ngcontent-%COMP%] {\n  font-size: 10px;\n  color: #888;\n  display: flex;\n  align-items: center;\n  gap: 4px;\n}\n.doc-status[_ngcontent-%COMP%]   .count[_ngcontent-%COMP%] {\n  background: #33280a;\n  color: #ff9800;\n  padding: 1px 6px;\n  border-radius: 8px;\n  font-weight: 600;\n}\n\n.collapse-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 14px;\n  cursor: pointer;\n  padding: 2px 6px;\n  border-radius: 3px;\n  transition: transform 0.2s;\n}\n.collapse-btn[_ngcontent-%COMP%]:hover {\n  color: #fff;\n  background: #3c3c3c;\n}\n\n.doc-template.collapsed[_ngcontent-%COMP%]   .collapse-btn[_ngcontent-%COMP%] {\n  transform: rotate(-90deg);\n}\n\n.doc-template-body[_ngcontent-%COMP%] {\n  padding: 0;\n}\n\n.empty-slots[_ngcontent-%COMP%] {\n  padding: 16px;\n  color: #555;\n  font-style: italic;\n  font-size: 12px;\n  text-align: center;\n}\n\n.doc-prompt-slot[_ngcontent-%COMP%] {\n  display: flex;\n  padding: 10px 16px;\n  border-bottom: 1px solid #2a2a2a;\n  gap: 12px;\n  align-items: flex-start;\n  transition: background 0.15s;\n  cursor: pointer;\n}\n.doc-prompt-slot[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n.doc-prompt-slot[_ngcontent-%COMP%]:hover {\n  background: #252526;\n}\n\n.slot-number[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  width: 22px;\n  height: 22px;\n  border-radius: 50%;\n  background: #33280a;\n  color: #ff9800;\n  font-size: 11px;\n  font-weight: 700;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  margin-top: 2px;\n}\n\n.slot-content[_ngcontent-%COMP%] {\n  flex: 1;\n  min-width: 0;\n}\n\n.slot-title[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: #aaa;\n  margin-bottom: 4px;\n}\n.slot-title[_ngcontent-%COMP%]   .card-link[_ngcontent-%COMP%] {\n  color: #ff9800;\n  font-weight: 600;\n}\n\n.slot-prompt[_ngcontent-%COMP%] {\n  font-size: 12px;\n  color: #ccc;\n  line-height: 1.5;\n  white-space: pre-wrap;\n  overflow: hidden;\n  display: -webkit-box;\n  -webkit-line-clamp: 3;\n  -webkit-box-orient: vertical;\n}\n\n.slot-arrow[_ngcontent-%COMP%] {\n  flex-shrink: 0;\n  color: #555;\n  font-size: 14px;\n  margin-top: 4px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiLWRvYy1wYW5lbC9wcm9tcHRsYWItZG9jLXBhbmVsLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsY0FBQTtFQUNBLGVBQUE7RUFDQSxjQUFBO0FBQ0Y7O0FBRUE7RUFDRSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxrQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0NBQUE7QUFDRjs7QUFFQTtFQUNFLGNBQUE7RUFDQSxlQUFBO0FBQ0Y7O0FBRUE7RUFDRSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0FBQ0Y7O0FBRUE7RUFDRSxPQUFBO0FBQ0Y7O0FBRUE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLGFBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7QUFDRjtBQUNFO0VBQ0UsbUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQ0o7O0FBR0E7RUFDRSxnQkFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EsMEJBQUE7QUFBRjtBQUVFO0VBQ0UsV0FBQTtFQUNBLG1CQUFBO0FBQUo7O0FBSUE7RUFDRSx5QkFBQTtBQURGOztBQUlBO0VBQ0UsVUFBQTtBQURGOztBQUlBO0VBQ0UsYUFBQTtFQUNBLFdBQUE7RUFDQSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxrQkFBQTtBQURGOztBQUlBO0VBQ0UsYUFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxTQUFBO0VBQ0EsdUJBQUE7RUFDQSw0QkFBQTtFQUNBLGVBQUE7QUFERjtBQUdFO0VBQ0UsbUJBQUE7QUFESjtBQUlFO0VBQ0UsbUJBQUE7QUFGSjs7QUFNQTtFQUNFLGNBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGtCQUFBO0VBQ0EsbUJBQUE7RUFDQSxjQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxlQUFBO0FBSEY7O0FBTUE7RUFDRSxPQUFBO0VBQ0EsWUFBQTtBQUhGOztBQU1BO0VBQ0UsZUFBQTtFQUNBLFdBQUE7RUFDQSxrQkFBQTtBQUhGO0FBS0U7RUFDRSxjQUFBO0VBQ0EsZ0JBQUE7QUFISjs7QUFPQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7RUFDQSxxQkFBQTtFQUNBLGdCQUFBO0VBQ0Esb0JBQUE7RUFDQSxxQkFBQTtFQUNBLDRCQUFBO0FBSkY7O0FBT0E7RUFDRSxjQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxlQUFBO0FBSkYiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xuICBwYWRkaW5nOiAwIDE2cHg7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uZG9jLXRlbXBsYXRlIHtcbiAgYmFja2dyb3VuZDogIzFhMWQyMTtcbiAgYm9yZGVyOiAycHggc29saWQgI2ZmOTgwMDtcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xuICBtYXJnaW46IDE2cHggMDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcbn1cblxuLmRvYy10ZW1wbGF0ZS1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTBweCAxNnB4O1xuICBiYWNrZ3JvdW5kOiAjMjUyNTI2O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbn1cblxuLmRvYy1pY29uIHtcbiAgY29sb3I6ICNmZjk4MDA7XG4gIGZvbnQtc2l6ZTogMTZweDtcbn1cblxuLmRvYy1uYW1lIHtcbiAgY29sb3I6ICNmZmY7XG4gIGZvbnQtc2l6ZTogMTNweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbn1cblxuLnNwYWNlciB7XG4gIGZsZXg6IDE7XG59XG5cbi5kb2Mtc3RhdHVzIHtcbiAgZm9udC1zaXplOiAxMHB4O1xuICBjb2xvcjogIzg4ODtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA0cHg7XG5cbiAgLmNvdW50IHtcbiAgICBiYWNrZ3JvdW5kOiAjMzMyODBhO1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgIHBhZGRpbmc6IDFweCA2cHg7XG4gICAgYm9yZGVyLXJhZGl1czogOHB4O1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gIH1cbn1cblxuLmNvbGxhcHNlLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogbm9uZTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiAycHggNnB4O1xuICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIHRyYW5zaXRpb246IHRyYW5zZm9ybSAwLjJzO1xuXG4gICY6aG92ZXIge1xuICAgIGNvbG9yOiAjZmZmO1xuICAgIGJhY2tncm91bmQ6ICMzYzNjM2M7XG4gIH1cbn1cblxuLmRvYy10ZW1wbGF0ZS5jb2xsYXBzZWQgLmNvbGxhcHNlLWJ0biB7XG4gIHRyYW5zZm9ybTogcm90YXRlKC05MGRlZyk7XG59XG5cbi5kb2MtdGVtcGxhdGUtYm9keSB7XG4gIHBhZGRpbmc6IDA7XG59XG5cbi5lbXB0eS1zbG90cyB7XG4gIHBhZGRpbmc6IDE2cHg7XG4gIGNvbG9yOiAjNTU1O1xuICBmb250LXN0eWxlOiBpdGFsaWM7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xufVxuXG4uZG9jLXByb21wdC1zbG90IHtcbiAgZGlzcGxheTogZmxleDtcbiAgcGFkZGluZzogMTBweCAxNnB4O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzJhMmEyYTtcbiAgZ2FwOiAxMnB4O1xuICBhbGlnbi1pdGVtczogZmxleC1zdGFydDtcbiAgdHJhbnNpdGlvbjogYmFja2dyb3VuZCAwLjE1cztcbiAgY3Vyc29yOiBwb2ludGVyO1xuXG4gICY6bGFzdC1jaGlsZCB7XG4gICAgYm9yZGVyLWJvdHRvbTogbm9uZTtcbiAgfVxuXG4gICY6aG92ZXIge1xuICAgIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIH1cbn1cblxuLnNsb3QtbnVtYmVyIHtcbiAgZmxleC1zaHJpbms6IDA7XG4gIHdpZHRoOiAyMnB4O1xuICBoZWlnaHQ6IDIycHg7XG4gIGJvcmRlci1yYWRpdXM6IDUwJTtcbiAgYmFja2dyb3VuZDogIzMzMjgwYTtcbiAgY29sb3I6ICNmZjk4MDA7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgZm9udC13ZWlnaHQ6IDcwMDtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIG1hcmdpbi10b3A6IDJweDtcbn1cblxuLnNsb3QtY29udGVudCB7XG4gIGZsZXg6IDE7XG4gIG1pbi13aWR0aDogMDtcbn1cblxuLnNsb3QtdGl0bGUge1xuICBmb250LXNpemU6IDExcHg7XG4gIGNvbG9yOiAjYWFhO1xuICBtYXJnaW4tYm90dG9tOiA0cHg7XG5cbiAgLmNhcmQtbGluayB7XG4gICAgY29sb3I6ICNmZjk4MDA7XG4gICAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgfVxufVxuXG4uc2xvdC1wcm9tcHQge1xuICBmb250LXNpemU6IDEycHg7XG4gIGNvbG9yOiAjY2NjO1xuICBsaW5lLWhlaWdodDogMS41O1xuICB3aGl0ZS1zcGFjZTogcHJlLXdyYXA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIGRpc3BsYXk6IC13ZWJraXQtYm94O1xuICAtd2Via2l0LWxpbmUtY2xhbXA6IDM7XG4gIC13ZWJraXQtYm94LW9yaWVudDogdmVydGljYWw7XG59XG5cbi5zbG90LWFycm93IHtcbiAgZmxleC1zaHJpbms6IDA7XG4gIGNvbG9yOiAjNTU1O1xuICBmb250LXNpemU6IDE0cHg7XG4gIG1hcmdpbi10b3A6IDRweDtcbn1cbiJdLCJzb3VyY2VSb290IjoiIn0= */"],
      changeDetection: 0
    });
  }
}

/***/ }),

/***/ 6739:
/*!***********************************************************************!*\
  !*** ./src/app/promptlab/components/promptlab/promptlab.component.ts ***!
  \***********************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabComponent": () => (/* binding */ PromptLabComponent)
/* harmony export */ });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! rxjs/operators */ 116);
/* harmony import */ var _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../models/promptlab.models */ 241);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _services_promptlab_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/promptlab.service */ 3819);
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../md-explorer/services/md-file.service */ 4169);
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../services/ai-chat.service */ 9109);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../promptlab-doc-panel/promptlab-doc-panel.component */ 1381);
/* harmony import */ var _promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../promptlab-agent-card/promptlab-agent-card.component */ 4060);
/* harmony import */ var _promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../promptlab-card/promptlab-card.component */ 748);














function PromptLabComponent_option_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r7 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", m_r7.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](m_r7.label);
  }
}
function PromptLabComponent_span_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](1, "span", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](3, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](3, 1, "PROMPTLAB.LOADING_MODELS"), " ");
  }
}
function PromptLabComponent_app_promptlab_agent_card_33_Template(rf, ctx) {
  if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "app-promptlab-agent-card", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("agentDefinitionChange", function PromptLabComponent_app_promptlab_agent_card_33_Template_app_promptlab_agent_card_agentDefinitionChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r9);
      const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r8.onAgentDefinitionChange($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("agentDefinition", ctx_r2.agentDefinition);
  }
}
function PromptLabComponent_div_36_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](2, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](2, 1, "PROMPTLAB.NO_CARDS"), " ");
  }
}
function PromptLabComponent_app_promptlab_card_37_Template(rf, ctx) {
  if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "app-promptlab-card", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("cardDeleted", function PromptLabComponent_app_promptlab_card_37_Template_app_promptlab_card_cardDeleted_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12);
      const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r11.onCardDeleted($event));
    })("cardChanged", function PromptLabComponent_app_promptlab_card_37_Template_app_promptlab_card_cardChanged_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12);
      const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r13.onCardChanged($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const card_r10 = ctx.$implicit;
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("card", card_r10)("isSingleCard", ctx_r4.cards.length === 1);
  }
}
function PromptLabComponent_div_38_option_19_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r17 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", m_r17.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](m_r17.label);
  }
}
function PromptLabComponent_div_38_option_38_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r18 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", m_r18.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](m_r18.label);
  }
}
function PromptLabComponent_div_38_option_57_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "option", 23);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const m_r19 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", m_r19.value);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](m_r19.label);
  }
}
function PromptLabComponent_div_38_Template(rf, ctx) {
  if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_div_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r20.onSettingsBackdropClick($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 30)(2, "div", 31)(3, "span", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "button", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r22.closeSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7, "\u2715");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "div", 34)(9, "div", 35)(10, "div", 36)(11, "label", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](13, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](14, "div", 38)(15, "select", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_select_ngModelChange_15_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r23.onPromptModelChange("system", $event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](16, "option", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](17);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](18, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](19, PromptLabComponent_div_38_option_19_Template, 2, 2, "option", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](20, "button", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_button_click_20_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r24.resetSystemPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](21, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](22);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](23, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](24, "p", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](25);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](26, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](27, "textarea", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_textarea_ngModelChange_27_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r25 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r25.onSystemPromptChange($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](28, "div", 35)(29, "div", 36)(30, "label", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](32, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](33, "div", 38)(34, "select", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_select_ngModelChange_34_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r26 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r26.onPromptModelChange("sequence", $event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](35, "option", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](36);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](37, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](38, PromptLabComponent_div_38_option_38_Template, 2, 2, "option", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](39, "button", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_button_click_39_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r27 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r27.resetSequencePrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](40, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](42, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](43, "p", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](44);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](45, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](46, "textarea", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_textarea_ngModelChange_46_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r28 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r28.onSequencePromptChange($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](47, "div", 35)(48, "div", 36)(49, "label", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](50);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](51, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](52, "div", 38)(53, "select", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_select_ngModelChange_53_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r29.onPromptModelChange("workflow", $event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](54, "option", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](55);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](56, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](57, PromptLabComponent_div_38_option_57_Template, 2, 2, "option", 6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](58, "button", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_button_click_58_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r30.resetWorkflowPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](59, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](60);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](61, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](62, "p", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](63);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](64, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](65, "textarea", 44);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_38_Template_textarea_ngModelChange_65_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r31 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r31.onWorkflowPromptChange($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](66, "div", 45)(67, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_38_Template_button_click_67_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r32 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r32.closeSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](68);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](69, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](5, 29, "PROMPTLAB.SESSION_SETTINGS"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](13, 31, "PROMPTLAB.SYSTEM_PROMPT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.systemPromptModel);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](18, 33, "PROMPTLAB.DEFAULT"), " (", ctx_r5.selectedModel || "\u2014", ")");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r5.models);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](21, 35, "PROMPTLAB.RESET_DEFAULT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("\u21BA ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](23, 37, "PROMPTLAB.DEFAULT"), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](26, 39, "PROMPTLAB.SYSTEM_PROMPT_HINT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.systemPrompt);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](32, 41, "PROMPTLAB.SEQUENCE_DIAGRAM"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.sequencePromptModel);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](37, 43, "PROMPTLAB.DEFAULT"), " (", ctx_r5.selectedModel || "\u2014", ")");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r5.models);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](40, 45, "PROMPTLAB.RESET_DEFAULT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("\u21BA ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](42, 47, "PROMPTLAB.DEFAULT"), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](45, 49, "PROMPTLAB.SEQUENCE_HINT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.sequencePrompt);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](51, 51, "PROMPTLAB.WORKFLOW_DIAGRAM"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.workflowPromptModel);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate2"]("", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](56, 53, "PROMPTLAB.DEFAULT"), " (", ctx_r5.selectedModel || "\u2014", ")");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r5.models);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](59, 55, "PROMPTLAB.RESET_DEFAULT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"]("\u21BA ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](61, 57, "PROMPTLAB.DEFAULT"), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](64, 59, "PROMPTLAB.WORKFLOW_HINT"), " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.workflowPrompt);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](69, 61, "COMMON.CLOSE"));
  }
}
function PromptLabComponent_div_39_Template(rf, ctx) {
  if (rf & 1) {
    const _r34 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_39_Template_div_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r34);
      const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r33.onBuildBackdropClick($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 48)(2, "div", 49)(3, "span", 50);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](5, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "button", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_39_Template_button_click_6_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r34);
      const ctx_r35 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r35.closeBuildOverlay());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7, "\u2715");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "div", 52)(9, "pre", 53);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](10);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "div", 54)(12, "button", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_39_Template_button_click_12_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r34);
      const ctx_r36 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r36.copyBuildOutput());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](13);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](14, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](15, "translate");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](5, 3, "PROMPTLAB.BUILD_OUTPUT"));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r6.buildOutput);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r6.buildCopied ? _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](14, 5, "PROMPTLAB.COPIED") : _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](15, 7, "PROMPTLAB.COPY_BTN"), " ");
  }
}
class PromptLabComponent {
  constructor(cdr, http, promptLabService, mdFileService, aiChatService, translate) {
    this.cdr = cdr;
    this.http = http;
    this.promptLabService = promptLabService;
    this.mdFileService = mdFileService;
    this.aiChatService = aiChatService;
    this.translate = translate;
    this.mode = 'prompt';
    this.selectedModel = 'claude-sonnet-4';
    this.sessionTitle = '';
    this.cards = [];
    this.templateName = 'template.md';
    this.agentDefinition = {
      identity: '',
      objectives: '',
      rules: '',
      tools: []
    };
    /** Build overlay state (Task 8.1) */
    this.buildOutput = null;
    this.buildCopied = false;
    /** Execute All state (Task 8.3) */
    this.isExecutingAll = false;
    /** Settings panel */
    this.showSettings = false;
    this.systemPrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT;
    this.systemPromptModel = '';
    this.sequencePrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SEQUENCE_PROMPT;
    this.sequencePromptModel = '';
    this.workflowPrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_WORKFLOW_PROMPT;
    this.workflowPromptModel = '';
    this.models = [];
    this.isLoadingModels = true;
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_8__.Subject();
  }
  ngOnInit() {
    // Set default session title using translate
    if (!this.sessionTitle) {
      this.sessionTitle = this.translate.instant('PROMPTLAB.NEW_SESSION');
    }
    // 1. Subscribe to session$ to keep local state in sync
    this.promptLabService.session$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$)).subscribe(session => {
      if (session) {
        this.sessionTitle = session.title || this.translate.instant('PROMPTLAB.NEW_SESSION');
        this.selectedModel = session.model || 'gpt-4o';
        this.mode = session.mode || 'prompt';
        this.cards = session.cards || [];
        this.templateName = session.templatePath ? session.templatePath.split(/[/\\]/).pop() || 'template.md' : 'template.md';
        this.systemPrompt = session.systemPrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT;
        this.sequencePrompt = session.sequencePrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SEQUENCE_PROMPT;
        this.workflowPrompt = session.workflowPrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_WORKFLOW_PROMPT;
        this.systemPromptModel = session.systemPromptModel || '';
        this.sequencePromptModel = session.sequencePromptModel || '';
        this.workflowPromptModel = session.workflowPromptModel || '';
        this.agentDefinition = session.agentDefinition || {
          identity: '',
          objectives: '',
          rules: '',
          tools: []
        };
      }
      this.cdr.markForCheck();
    });
    // 1b. Subscribe to executeAll state
    this.promptLabService.isExecutingAll$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$)).subscribe(val => {
      this.isExecutingAll = val;
      this.cdr.markForCheck();
    });
    // 2. When a file is selected in the tree, load its content as a PromptLab session
    //    BUT skip if the file is already loaded (avoids wiping transient state
    //    like conversation[] when auto-save triggers a FileSystemWatcher event)
    this.mdFileService.selectedMdFileFromSideNav.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$), (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.filter)(file => !!file && !!file.fullPath), (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.filter)(file => {
      const currentSession = this.promptLabService.currentSession();
      return !currentSession || currentSession.templatePath !== file.fullPath;
    })).subscribe(file => {
      this.loadFileAsSession(file.fullPath);
    });
    // 3. Also try the currently selected file (if already set before navigation)
    const currentFile = this.mdFileService.currentSelectedMdFile;
    if (currentFile && currentFile.fullPath) {
      this.loadFileAsSession(currentFile.fullPath);
    }
    // 4. Load available models from Copilot CLI
    this.loadModels();
  }
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
  loadModels() {
    // 1. Try in-memory cache from the service (instant, no HTTP)
    const memoryCached = this.aiChatService.cachedModels;
    if (memoryCached?.length) {
      this.models = memoryCached;
      this.isLoadingModels = false;
      this.syncSelectedModel();
      this.cdr.markForCheck();
    } else {
      this.isLoadingModels = true;
    }
    // 2. Load from DB cache (fast, <50ms) — populates combo immediately
    this.aiChatService.getCachedModels().subscribe({
      next: models => {
        if (models.length) {
          this.models = models;
          this.isLoadingModels = false;
          this.syncSelectedModel();
          this.cdr.markForCheck();
        }
      },
      error: () => {}
    });
    // 3. Background refresh via Copilot CLI discovery (~5s) — updates DB + memory cache
    this.aiChatService.refreshCopilotCliModels().subscribe({
      next: () => {
        const refreshed = this.aiChatService.cachedModels;
        if (refreshed?.length) {
          this.models = refreshed;
          this.syncSelectedModel();
        }
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      }
    });
  }
  syncSelectedModel() {
    if (this.models.length && !this.models.find(m => m.value === this.selectedModel)) {
      this.selectedModel = this.models[0].value;
      this.promptLabService.setModel(this.selectedModel);
    }
  }
  loadFileAsSession(fullPath) {
    const url = `/api/MdExplorerEditorReact/${fullPath}`;
    this.http.get(url, {
      responseType: 'text'
    }).subscribe({
      next: markdown => {
        this.promptLabService.loadSession(markdown, fullPath);
      },
      error: err => {
        console.error('[PromptLab] Errore nel caricare il file:', err);
      }
    });
  }
  // ---------------------------------------------------------------------------
  // Template actions — delegate to PromptLabService
  // ---------------------------------------------------------------------------
  toggleMode(newMode) {
    this.promptLabService.setMode(newMode);
  }
  addCard() {
    this.promptLabService.addCard();
  }
  onCardDeleted(cardId) {
    this.promptLabService.removeCard(cardId);
  }
  onCardChanged(updatedCard) {
    this.promptLabService.updateCard(updatedCard);
  }
  build() {
    const result = this.promptLabService.buildSession();
    this.buildOutput = result;
    this.buildCopied = false;
    this.cdr.markForCheck();
  }
  closeBuildOverlay() {
    this.buildOutput = null;
    this.cdr.markForCheck();
  }
  copyBuildOutput() {
    if (this.buildOutput) {
      navigator.clipboard.writeText(this.buildOutput).then(() => {
        this.buildCopied = true;
        this.cdr.markForCheck();
        setTimeout(() => {
          this.buildCopied = false;
          this.cdr.markForCheck();
        }, 2000);
      });
    }
  }
  onBuildBackdropClick(event) {
    // Only close if clicking the backdrop itself, not the card
    if (event.target.classList.contains('build-overlay-backdrop')) {
      this.closeBuildOverlay();
    }
  }
  executeAll() {
    this.promptLabService.executeAll();
  }
  trackByCardId(index, card) {
    return card.id;
  }
  onAgentDefinitionChange(def) {
    this.promptLabService.setAgentDefinition(def);
  }
  onModelChange(model) {
    this.promptLabService.setModel(model);
  }
  openSettings() {
    this.showSettings = true;
    this.cdr.markForCheck();
  }
  closeSettings() {
    this.showSettings = false;
    this.cdr.markForCheck();
  }
  onSystemPromptChange(value) {
    this.systemPrompt = value;
    this.promptLabService.setSystemPrompt(value);
  }
  resetSystemPrompt() {
    this.systemPrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT;
    this.promptLabService.setSystemPrompt(_models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT);
    this.cdr.markForCheck();
  }
  onSequencePromptChange(value) {
    this.sequencePrompt = value;
    this.promptLabService.setSequencePrompt(value);
  }
  resetSequencePrompt() {
    this.sequencePrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SEQUENCE_PROMPT;
    this.promptLabService.setSequencePrompt(_models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SEQUENCE_PROMPT);
    this.cdr.markForCheck();
  }
  onWorkflowPromptChange(value) {
    this.workflowPrompt = value;
    this.promptLabService.setWorkflowPrompt(value);
  }
  resetWorkflowPrompt() {
    this.workflowPrompt = _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_WORKFLOW_PROMPT;
    this.promptLabService.setWorkflowPrompt(_models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_WORKFLOW_PROMPT);
    this.cdr.markForCheck();
  }
  onPromptModelChange(promptKey, value) {
    const session = this.promptLabService.currentSession();
    if (!session) return;
    switch (promptKey) {
      case 'system':
        this.systemPromptModel = value;
        session.systemPromptModel = value;
        break;
      case 'sequence':
        this.sequencePromptModel = value;
        session.sequencePromptModel = value;
        break;
      case 'workflow':
        this.workflowPromptModel = value;
        session.workflowPromptModel = value;
        break;
    }
    session.updatedAt = new Date();
    this.promptLabService.updateSession(session);
  }
  onSettingsBackdropClick(event) {
    if (event.target.classList.contains('modal-backdrop')) {
      this.closeSettings();
    }
  }
  static {
    this.ɵfac = function PromptLabComponent_Factory(t) {
      return new (t || PromptLabComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_7__.ChangeDetectorRef), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_11__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_promptlab_service__WEBPACK_IMPORTED_MODULE_1__.PromptLabService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__.AiChatService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__.TranslateService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({
      type: PromptLabComponent,
      selectors: [["app-promptlab"]],
      decls: 40,
      vars: 46,
      consts: [[1, "session-header"], [1, "session-title"], [1, "badge"], [1, "spacer"], [1, "model-selector-wrapper"], [1, "model-selector", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["class", "model-loading", 4, "ngIf"], [1, "mode-toggle"], [1, "mode-btn", 3, "click"], [1, "separator"], [1, "session-action-btn", "add", 3, "click"], [1, "session-action-btn", "build", 3, "click"], [1, "session-action-btn", "exec", 3, "disabled", "click"], [1, "session-action-btn", "settings", 3, "title", "click"], [1, "session-content"], [3, "agentDefinition", "agentDefinitionChange", 4, "ngIf"], [3, "cards", "templateName"], [1, "cards-area"], ["class", "cards-placeholder", 4, "ngIf"], [3, "card", "isSingleCard", "cardDeleted", "cardChanged", 4, "ngFor", "ngForOf", "ngForTrackBy"], ["class", "modal-backdrop", 3, "click", 4, "ngIf"], ["class", "build-overlay-backdrop", 3, "click", 4, "ngIf"], [3, "value"], [1, "model-loading"], [1, "spinner"], [3, "agentDefinition", "agentDefinitionChange"], [1, "cards-placeholder"], [3, "card", "isSingleCard", "cardDeleted", "cardChanged"], [1, "modal-backdrop", 3, "click"], [1, "modal-card", "settings-modal"], [1, "modal-header"], [1, "modal-title"], [1, "modal-close", 3, "click"], [1, "modal-body"], [1, "settings-section"], [1, "settings-section-header"], [1, "settings-label"], [1, "settings-section-actions"], [1, "settings-model-select", 3, "ngModel", "ngModelChange"], ["value", ""], [1, "settings-reset-btn", 3, "title", "click"], [1, "settings-hint"], ["rows", "10", "spellcheck", "false", 1, "settings-textarea", 3, "ngModel", "ngModelChange"], ["rows", "6", "spellcheck", "false", 1, "settings-textarea", 3, "ngModel", "ngModelChange"], [1, "modal-footer"], [1, "modal-btn", 3, "click"], [1, "build-overlay-backdrop", 3, "click"], [1, "build-overlay-card"], [1, "build-overlay-header"], [1, "build-overlay-title"], [1, "build-overlay-close", 3, "click"], [1, "build-overlay-body"], [1, "build-overlay-pre"], [1, "build-overlay-footer"], [1, "build-overlay-copy", 3, "click"]],
      template: function PromptLabComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 0)(1, "span", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](5, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](6, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "div", 4)(8, "select", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_Template_select_ngModelChange_8_listener($event) {
            return ctx.selectedModel = $event;
          })("ngModelChange", function PromptLabComponent_Template_select_ngModelChange_8_listener($event) {
            return ctx.onModelChange($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, PromptLabComponent_option_9_Template, 2, 2, "option", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, PromptLabComponent_span_10_Template, 4, 3, "span", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "div", 8)(12, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_12_listener() {
            return ctx.toggleMode("prompt");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](14, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](15, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_15_listener() {
            return ctx.toggleMode("agent");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](16);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](17, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](18, "span", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](19, "button", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_19_listener() {
            return ctx.addCard();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](20);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](21, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](22, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_22_listener() {
            return ctx.build();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](23);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](24, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](25, "button", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_25_listener() {
            return ctx.executeAll();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](26);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](27, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](28, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](29, "button", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_29_listener() {
            return ctx.openSettings();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipe"](30, "translate");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](31, "\u2699");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](32, "div", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](33, PromptLabComponent_app_promptlab_agent_card_33_Template, 1, 1, "app-promptlab-agent-card", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](34, "app-promptlab-doc-panel", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](35, "div", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](36, PromptLabComponent_div_36_Template, 3, 3, "div", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](37, PromptLabComponent_app_promptlab_card_37_Template, 1, 2, "app-promptlab-card", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](38, PromptLabComponent_div_38_Template, 70, 63, "div", 21);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](39, PromptLabComponent_div_39_Template, 16, 9, "div", 22);
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.sessionTitle);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](5, 30, "PROMPTLAB.TITLE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵstyleProp"]("display", ctx.isLoadingModels ? "none" : "inline-block");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.selectedModel);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.models);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isLoadingModels);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.mode === "prompt");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](14, 32, "PROMPTLAB.PROMPT_MODE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.mode === "agent");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](17, 34, "PROMPTLAB.AGENT_MODE"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](21, 36, "PROMPTLAB.ADD_CARD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](24, 38, "PROMPTLAB.BUILD"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isExecutingAll);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isExecutingAll ? _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](27, 40, "PROMPTLAB.EXECUTING") : _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](28, 42, "PROMPTLAB.EXECUTE_ALL"), " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("title", _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵpipeBind1"](30, 44, "PROMPTLAB.SESSION_SETTINGS"));
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("single-card-content", ctx.cards.length === 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.mode === "agent");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("cards", ctx.cards)("templateName", ctx.templateName);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("single-card-area", ctx.cards.length === 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.cards.length === 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.cards)("ngForTrackBy", ctx.trackByCardId);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.showSettings);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.buildOutput !== null);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_13__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_13__.NgIf, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgSelectOption, _angular_forms__WEBPACK_IMPORTED_MODULE_14__["ɵNgSelectMultipleOption"], _angular_forms__WEBPACK_IMPORTED_MODULE_14__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.SelectControlValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgModel, _promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_4__.PromptLabDocPanelComponent, _promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_5__.PromptLabAgentCardComponent, _promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_6__.PromptLabCardComponent, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_12__.TranslatePipe],
      styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  flex: 1 1 auto;\n  height: 100%;\n  min-height: 0;\n  min-width: 0;\n  overflow: hidden;\n  background: #1e1e1e;\n  color: #ccc;\n  font-family: \"Segoe UI\", sans-serif;\n  font-size: 13px;\n}\n\n\n.session-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 10px 16px;\n  background: #2d2d2d;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n  flex-wrap: wrap;\n}\n\n.session-title[_ngcontent-%COMP%] {\n  font-size: 18px;\n  color: #fff;\n  font-weight: 600;\n}\n\n.badge[_ngcontent-%COMP%] {\n  font-size: 10px;\n  background: #ff9800;\n  color: #000;\n  padding: 2px 8px;\n  border-radius: 10px;\n  font-weight: 600;\n}\n\n.spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n\n.model-selector[_ngcontent-%COMP%] {\n  background: #3c3c3c;\n  border: 1px solid #555;\n  border-radius: 4px;\n  color: #ccc;\n  font-size: 11px;\n  padding: 4px 8px;\n  cursor: pointer;\n  outline: none;\n}\n.model-selector[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n}\n.model-selector[_ngcontent-%COMP%]   option[_ngcontent-%COMP%] {\n  background: #3c3c3c;\n  color: #ccc;\n}\n\n.model-selector-wrapper[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n}\n\n.model-loading[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n  font-size: 11px;\n  color: #888;\n}\n\n.spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 12px;\n  height: 12px;\n  border: 2px solid #555;\n  border-top-color: #ff9800;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_spin 0.8s linear infinite;\n}\n\n@keyframes _ngcontent-%COMP%_spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n\n.mode-toggle[_ngcontent-%COMP%] {\n  display: flex;\n  border: 1px solid #555;\n  border-radius: 4px;\n  overflow: hidden;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 11px;\n  padding: 4px 12px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn.active[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  font-weight: 600;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn[_ngcontent-%COMP%]:not(.active):hover {\n  background: #3c3c3c;\n  color: #ccc;\n}\n\n\n.separator[_ngcontent-%COMP%] {\n  width: 1px;\n  height: 24px;\n  background: #555;\n  flex-shrink: 0;\n}\n\n\n.session-action-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 11px;\n  padding: 4px 12px;\n  border-radius: 4px;\n  cursor: pointer;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.session-action-btn.add[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n.session-action-btn.build[_ngcontent-%COMP%]:hover {\n  border-color: #82b1ff;\n  color: #82b1ff;\n}\n.session-action-btn.exec[_ngcontent-%COMP%] {\n  border-color: #ff9800;\n  color: #ff9800;\n  font-weight: 600;\n}\n.session-action-btn.exec[_ngcontent-%COMP%]:hover:not(:disabled) {\n  background: #ff9800;\n  color: #000;\n}\n.session-action-btn.exec[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.session-action-btn.settings[_ngcontent-%COMP%] {\n  font-size: 14px;\n  padding: 4px 8px;\n}\n.session-action-btn.settings[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n  color: #ccc;\n}\n\n\n.modal-backdrop[_ngcontent-%COMP%] {\n  position: fixed;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background: rgba(0, 0, 0, 0.6);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  z-index: 1000;\n}\n\n.modal-card[_ngcontent-%COMP%] {\n  background: #2d2d2d;\n  border: 1px solid #555;\n  border-radius: 6px;\n  min-width: 400px;\n  max-width: 600px;\n  max-height: 80vh;\n  display: flex;\n  flex-direction: column;\n  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);\n}\n.modal-card.settings-modal[_ngcontent-%COMP%] {\n  min-width: 600px;\n  max-width: 800px;\n}\n\n.modal-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.modal-title[_ngcontent-%COMP%] {\n  flex: 1;\n  font-size: 14px;\n  font-weight: 600;\n  color: #e0e0e0;\n}\n\n.modal-close[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 16px;\n  cursor: pointer;\n  padding: 2px 6px;\n}\n.modal-close[_ngcontent-%COMP%]:hover {\n  color: #fff;\n}\n\n.modal-body[_ngcontent-%COMP%] {\n  padding: 20px;\n  overflow-y: auto;\n  flex: 1;\n}\n\n.modal-footer[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  gap: 8px;\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n}\n\n.modal-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  cursor: pointer;\n}\n.modal-btn[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n  color: #e0e0e0;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.settings-section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n}\n\n.settings-section-actions[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n}\n\n.settings-model-select[_ngcontent-%COMP%] {\n  background: #1e1e1e;\n  border: 1px solid #3c3c3c;\n  border-radius: 3px;\n  color: #ccc;\n  font-size: 11px;\n  padding: 3px 6px;\n  outline: none;\n  max-width: 200px;\n}\n.settings-model-select[_ngcontent-%COMP%]:focus {\n  border-color: #ff9800;\n}\n.settings-model-select[_ngcontent-%COMP%]   option[_ngcontent-%COMP%] {\n  background: #252526;\n  color: #ccc;\n}\n\n.settings-label[_ngcontent-%COMP%] {\n  font-size: 13px;\n  font-weight: 600;\n  color: #e0e0e0;\n}\n\n.settings-reset-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #888;\n  font-size: 11px;\n  padding: 3px 10px;\n  border-radius: 3px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.settings-reset-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n.settings-hint[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: #888;\n  margin: 0;\n  line-height: 1.5;\n}\n\n.settings-textarea[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 200px;\n  background: #1e1e1e;\n  border: 1px solid #3c3c3c;\n  border-radius: 4px;\n  color: #ccc;\n  font-family: \"Cascadia Code\", \"Consolas\", \"Courier New\", monospace;\n  font-size: 12px;\n  line-height: 1.5;\n  padding: 12px;\n  resize: vertical;\n  outline: none;\n  box-sizing: border-box;\n}\n.settings-textarea[_ngcontent-%COMP%]:focus {\n  border-color: #ff9800;\n}\n\n\n.session-content[_ngcontent-%COMP%] {\n  flex: 1 1 0;\n  min-height: 0;\n  overflow-y: auto;\n  overflow-x: hidden;\n  padding: 16px 24px;\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n\n\n.cards-area[_ngcontent-%COMP%] {\n  flex: 0 0 auto;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.cards-placeholder[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  height: 200px;\n  color: #555;\n  font-size: 14px;\n  font-style: italic;\n  border: 1px dashed #3c3c3c;\n  border-radius: 8px;\n}\n\n\n.build-overlay-backdrop[_ngcontent-%COMP%] {\n  position: fixed;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background: rgba(0, 0, 0, 0.6);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  z-index: 1000;\n}\n\n.build-overlay-card[_ngcontent-%COMP%] {\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 8px;\n  width: 80%;\n  max-width: 800px;\n  max-height: 80vh;\n  display: flex;\n  flex-direction: column;\n  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);\n}\n\n.build-overlay-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.build-overlay-title[_ngcontent-%COMP%] {\n  font-size: 14px;\n  font-weight: 600;\n  color: #fff;\n  flex: 1;\n}\n\n.build-overlay-close[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 16px;\n  cursor: pointer;\n  padding: 4px 8px;\n  border-radius: 3px;\n  transition: all 0.15s;\n}\n.build-overlay-close[_ngcontent-%COMP%]:hover {\n  color: #ef5350;\n  background: rgba(239, 83, 80, 0.1);\n}\n\n.build-overlay-body[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 16px;\n  min-height: 0;\n}\n\n.build-overlay-pre[_ngcontent-%COMP%] {\n  font-family: \"Cascadia Code\", \"Consolas\", \"Courier New\", monospace;\n  font-size: 12px;\n  line-height: 1.6;\n  color: #ccc;\n  white-space: pre-wrap;\n  word-break: break-word;\n  margin: 0;\n}\n\n.build-overlay-footer[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.build-overlay-copy[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  font-weight: 600;\n  cursor: pointer;\n  transition: filter 0.15s;\n  min-width: 80px;\n}\n.build-overlay-copy[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.1);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiL3Byb21wdGxhYi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGNBQUE7RUFDQSxZQUFBO0VBQ0EsYUFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLG1DQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBLHVCQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGdDQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7QUFDRjs7QUFFQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGVBQUE7RUFDQSxtQkFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7QUFDRjs7QUFFQSxtQkFBQTtBQUNBO0VBQ0UsbUJBQUE7RUFDQSxzQkFBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0FBQ0Y7QUFDRTtFQUNFLGtCQUFBO0FBQ0o7QUFFRTtFQUNFLG1CQUFBO0VBQ0EsV0FBQTtBQUFKOztBQUlBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtBQURGOztBQUlBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtBQURGOztBQUlBO0VBQ0UscUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLHNCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG9DQUFBO0FBREY7O0FBSUE7RUFDRTtJQUFLLHlCQUFBO0VBQUw7QUFDRjtBQUVBLGdCQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0FBQUY7QUFFRTtFQUNFLGdCQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxlQUFBO0VBQ0EscUJBQUE7QUFBSjtBQUVJO0VBQ0UsbUJBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFBTjtBQUdJO0VBQ0UsbUJBQUE7RUFDQSxXQUFBO0FBRE47O0FBTUEsY0FBQTtBQUNBO0VBQ0UsVUFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtFQUNBLGNBQUE7QUFIRjs7QUFNQSxtQkFBQTtBQUNBO0VBQ0UsZ0JBQUE7RUFDQSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxtQkFBQTtFQUNBLHFCQUFBO0FBSEY7QUFLRTtFQUNFLHFCQUFBO0VBQ0EsY0FBQTtBQUhKO0FBTUU7RUFDRSxxQkFBQTtFQUNBLGNBQUE7QUFKSjtBQU9FO0VBQ0UscUJBQUE7RUFDQSxjQUFBO0VBQ0EsZ0JBQUE7QUFMSjtBQU9JO0VBQ0UsbUJBQUE7RUFDQSxXQUFBO0FBTE47QUFRSTtFQUNFLFlBQUE7RUFDQSxtQkFBQTtBQU5OO0FBVUU7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7QUFSSjtBQVVJO0VBQ0Usa0JBQUE7RUFDQSxXQUFBO0FBUk47O0FBYUEsd0NBQUE7QUFDQTtFQUNFLGVBQUE7RUFDQSxNQUFBO0VBQ0EsT0FBQTtFQUNBLFFBQUE7RUFDQSxTQUFBO0VBQ0EsOEJBQUE7RUFDQSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSx1QkFBQTtFQUNBLGFBQUE7QUFWRjs7QUFhQTtFQUNFLG1CQUFBO0VBQ0Esc0JBQUE7RUFDQSxrQkFBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtFQUNBLHlDQUFBO0FBVkY7QUFZRTtFQUNFLGdCQUFBO0VBQ0EsZ0JBQUE7QUFWSjs7QUFjQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7QUFYRjs7QUFjQTtFQUNFLE9BQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0FBWEY7O0FBY0E7RUFDRSxnQkFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtBQVhGO0FBYUU7RUFBVSxXQUFBO0FBVlo7O0FBYUE7RUFDRSxhQUFBO0VBQ0EsZ0JBQUE7RUFDQSxPQUFBO0FBVkY7O0FBYUE7RUFDRSxhQUFBO0VBQ0EseUJBQUE7RUFDQSxRQUFBO0VBQ0Esa0JBQUE7RUFDQSw2QkFBQTtBQVZGOztBQWFBO0VBQ0UsZ0JBQUE7RUFDQSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7QUFWRjtBQVlFO0VBQ0Usa0JBQUE7RUFDQSxjQUFBO0FBVko7O0FBY0E7RUFDRSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxRQUFBO0FBWEY7O0FBY0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSw4QkFBQTtBQVhGOztBQWNBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsUUFBQTtBQVhGOztBQWNBO0VBQ0UsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtBQVhGO0FBYUU7RUFDRSxxQkFBQTtBQVhKO0FBY0U7RUFDRSxtQkFBQTtFQUNBLFdBQUE7QUFaSjs7QUFnQkE7RUFDRSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxjQUFBO0FBYkY7O0FBZ0JBO0VBQ0UsZ0JBQUE7RUFDQSxzQkFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtFQUNBLGVBQUE7RUFDQSxxQkFBQTtBQWJGO0FBZUU7RUFDRSxxQkFBQTtFQUNBLGNBQUE7QUFiSjs7QUFpQkE7RUFDRSxlQUFBO0VBQ0EsV0FBQTtFQUNBLFNBQUE7RUFDQSxnQkFBQTtBQWRGOztBQWlCQTtFQUNFLFdBQUE7RUFDQSxpQkFBQTtFQUNBLG1CQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLFdBQUE7RUFDQSxrRUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxzQkFBQTtBQWRGO0FBZ0JFO0VBQ0UscUJBQUE7QUFkSjs7QUFrQkEsNkRBQUE7QUFDQTtFQUNFLFdBQUE7RUFDQSxhQUFBO0VBQ0EsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGtCQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQWZGOztBQWtCQSxlQUFBO0FBQ0E7RUFDRSxjQUFBO0VBQ0EsYUFBQTtFQUNBLHNCQUFBO0VBQ0EsU0FBQTtBQWZGOztBQWtCQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLHVCQUFBO0VBQ0EsYUFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7RUFDQSwwQkFBQTtFQUNBLGtCQUFBO0FBZkY7O0FBa0JBLG9DQUFBO0FBQ0E7RUFDRSxlQUFBO0VBQ0EsTUFBQTtFQUNBLE9BQUE7RUFDQSxRQUFBO0VBQ0EsU0FBQTtFQUNBLDhCQUFBO0VBQ0EsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7RUFDQSxhQUFBO0FBZkY7O0FBa0JBO0VBQ0UsbUJBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsVUFBQTtFQUNBLGdCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSx5Q0FBQTtBQWZGOztBQWtCQTtFQUNFLGFBQUE7RUFDQSxtQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZ0NBQUE7RUFDQSxjQUFBO0FBZkY7O0FBa0JBO0VBQ0UsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLE9BQUE7QUFmRjs7QUFrQkE7RUFDRSxnQkFBQTtFQUNBLFlBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGtCQUFBO0VBQ0EscUJBQUE7QUFmRjtBQWlCRTtFQUNFLGNBQUE7RUFDQSxrQ0FBQTtBQWZKOztBQW1CQTtFQUNFLE9BQUE7RUFDQSxnQkFBQTtFQUNBLGFBQUE7RUFDQSxhQUFBO0FBaEJGOztBQW1CQTtFQUNFLGtFQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsV0FBQTtFQUNBLHFCQUFBO0VBQ0Esc0JBQUE7RUFDQSxTQUFBO0FBaEJGOztBQW1CQTtFQUNFLGFBQUE7RUFDQSx5QkFBQTtFQUNBLGtCQUFBO0VBQ0EsNkJBQUE7RUFDQSxjQUFBO0FBaEJGOztBQW1CQTtFQUNFLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLFlBQUE7RUFDQSxpQkFBQTtFQUNBLGtCQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0EsZUFBQTtFQUNBLHdCQUFBO0VBQ0EsZUFBQTtBQWhCRjtBQWtCRTtFQUNFLHVCQUFBO0FBaEJKIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBmbGV4OiAxIDEgYXV0bztcbiAgaGVpZ2h0OiAxMDAlO1xuICBtaW4taGVpZ2h0OiAwO1xuICBtaW4td2lkdGg6IDA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIGJhY2tncm91bmQ6ICMxZTFlMWU7XG4gIGNvbG9yOiAjY2NjO1xuICBmb250LWZhbWlseTogJ1NlZ29lIFVJJywgc2Fucy1zZXJpZjtcbiAgZm9udC1zaXplOiAxM3B4O1xufVxuXG4vKiBTZXNzaW9uIEhlYWRlciBCYXIgKi9cbi5zZXNzaW9uLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogMTBweDtcbiAgcGFkZGluZzogMTBweCAxNnB4O1xuICBiYWNrZ3JvdW5kOiAjMmQyZDJkO1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbiAgZmxleC1zaHJpbms6IDA7XG4gIGZsZXgtd3JhcDogd3JhcDtcbn1cblxuLnNlc3Npb24tdGl0bGUge1xuICBmb250LXNpemU6IDE4cHg7XG4gIGNvbG9yOiAjZmZmO1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4uYmFkZ2Uge1xuICBmb250LXNpemU6IDEwcHg7XG4gIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gIGNvbG9yOiAjMDAwO1xuICBwYWRkaW5nOiAycHggOHB4O1xuICBib3JkZXItcmFkaXVzOiAxMHB4O1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4uc3BhY2VyIHtcbiAgZmxleDogMTtcbn1cblxuLyogTW9kZWwgc2VsZWN0b3IgKi9cbi5tb2RlbC1zZWxlY3RvciB7XG4gIGJhY2tncm91bmQ6ICMzYzNjM2M7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgY29sb3I6ICNjY2M7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogNHB4IDhweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBvdXRsaW5lOiBub25lO1xuXG4gICY6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgfVxuXG4gIG9wdGlvbiB7XG4gICAgYmFja2dyb3VuZDogIzNjM2MzYztcbiAgICBjb2xvcjogI2NjYztcbiAgfVxufVxuXG4ubW9kZWwtc2VsZWN0b3Itd3JhcHBlciB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xufVxuXG4ubW9kZWwtbG9hZGluZyB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDZweDtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBjb2xvcjogIzg4ODtcbn1cblxuLnNwaW5uZXIge1xuICBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7XG4gIHdpZHRoOiAxMnB4O1xuICBoZWlnaHQ6IDEycHg7XG4gIGJvcmRlcjogMnB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci10b3AtY29sb3I6ICNmZjk4MDA7XG4gIGJvcmRlci1yYWRpdXM6IDUwJTtcbiAgYW5pbWF0aW9uOiBzcGluIDAuOHMgbGluZWFyIGluZmluaXRlO1xufVxuXG5Aa2V5ZnJhbWVzIHNwaW4ge1xuICB0byB7IHRyYW5zZm9ybTogcm90YXRlKDM2MGRlZyk7IH1cbn1cblxuLyogTW9kZSB0b2dnbGUgKi9cbi5tb2RlLXRvZ2dsZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcblxuICAubW9kZS1idG4ge1xuICAgIGJhY2tncm91bmQ6IG5vbmU7XG4gICAgYm9yZGVyOiBub25lO1xuICAgIGNvbG9yOiAjODg4O1xuICAgIGZvbnQtc2l6ZTogMTFweDtcbiAgICBwYWRkaW5nOiA0cHggMTJweDtcbiAgICBjdXJzb3I6IHBvaW50ZXI7XG4gICAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICAgJi5hY3RpdmUge1xuICAgICAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgICAgIGNvbG9yOiAjMDAwO1xuICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgICB9XG5cbiAgICAmOm5vdCguYWN0aXZlKTpob3ZlciB7XG4gICAgICBiYWNrZ3JvdW5kOiAjM2MzYzNjO1xuICAgICAgY29sb3I6ICNjY2M7XG4gICAgfVxuICB9XG59XG5cbi8qIFNlcGFyYXRvciAqL1xuLnNlcGFyYXRvciB7XG4gIHdpZHRoOiAxcHg7XG4gIGhlaWdodDogMjRweDtcbiAgYmFja2dyb3VuZDogIzU1NTtcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi8qIEFjdGlvbiBidXR0b25zICovXG4uc2Vzc2lvbi1hY3Rpb24tYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogNHB4IDEycHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJi5hZGQ6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgfVxuXG4gICYuYnVpbGQ6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogIzgyYjFmZjtcbiAgICBjb2xvcjogIzgyYjFmZjtcbiAgfVxuXG4gICYuZXhlYyB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG5cbiAgICAmOmhvdmVyOm5vdCg6ZGlzYWJsZWQpIHtcbiAgICAgIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gICAgICBjb2xvcjogIzAwMDtcbiAgICB9XG5cbiAgICAmOmRpc2FibGVkIHtcbiAgICAgIG9wYWNpdHk6IDAuNTtcbiAgICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gICAgfVxuICB9XG5cbiAgJi5zZXR0aW5ncyB7XG4gICAgZm9udC1zaXplOiAxNHB4O1xuICAgIHBhZGRpbmc6IDRweCA4cHg7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgICAgIGNvbG9yOiAjY2NjO1xuICAgIH1cbiAgfVxufVxuXG4vKiBNb2RhbCAocmV1c2FibGUgZm9yIFNldHRpbmdzLCBldGMuKSAqL1xuLm1vZGFsLWJhY2tkcm9wIHtcbiAgcG9zaXRpb246IGZpeGVkO1xuICB0b3A6IDA7XG4gIGxlZnQ6IDA7XG4gIHJpZ2h0OiAwO1xuICBib3R0b206IDA7XG4gIGJhY2tncm91bmQ6IHJnYmEoMCwgMCwgMCwgMC42KTtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHotaW5kZXg6IDEwMDA7XG59XG5cbi5tb2RhbC1jYXJkIHtcbiAgYmFja2dyb3VuZDogIzJkMmQyZDtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgYm9yZGVyLXJhZGl1czogNnB4O1xuICBtaW4td2lkdGg6IDQwMHB4O1xuICBtYXgtd2lkdGg6IDYwMHB4O1xuICBtYXgtaGVpZ2h0OiA4MHZoO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBib3gtc2hhZG93OiAwIDhweCAzMnB4IHJnYmEoMCwgMCwgMCwgMC41KTtcblxuICAmLnNldHRpbmdzLW1vZGFsIHtcbiAgICBtaW4td2lkdGg6IDYwMHB4O1xuICAgIG1heC13aWR0aDogODAwcHg7XG4gIH1cbn1cblxuLm1vZGFsLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICMzYzNjM2M7XG59XG5cbi5tb2RhbC10aXRsZSB7XG4gIGZsZXg6IDE7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgY29sb3I6ICNlMGUwZTA7XG59XG5cbi5tb2RhbC1jbG9zZSB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogbm9uZTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTZweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiAycHggNnB4O1xuXG4gICY6aG92ZXIgeyBjb2xvcjogI2ZmZjsgfVxufVxuXG4ubW9kYWwtYm9keSB7XG4gIHBhZGRpbmc6IDIwcHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIGZsZXg6IDE7XG59XG5cbi5tb2RhbC1mb290ZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBqdXN0aWZ5LWNvbnRlbnQ6IGZsZXgtZW5kO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xuICBib3JkZXItdG9wOiAxcHggc29saWQgIzNjM2MzYztcbn1cblxuLm1vZGFsLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGNvbG9yOiAjYWFhO1xuICBwYWRkaW5nOiA2cHggMTZweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBmb250LXNpemU6IDEycHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcblxuICAmOmhvdmVyIHtcbiAgICBib3JkZXItY29sb3I6ICM4ODg7XG4gICAgY29sb3I6ICNlMGUwZTA7XG4gIH1cbn1cblxuLnNldHRpbmdzLXNlY3Rpb24ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDhweDtcbn1cblxuLnNldHRpbmdzLXNlY3Rpb24taGVhZGVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xufVxuXG4uc2V0dGluZ3Mtc2VjdGlvbi1hY3Rpb25zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA4cHg7XG59XG5cbi5zZXR0aW5ncy1tb2RlbC1zZWxlY3Qge1xuICBiYWNrZ3JvdW5kOiAjMWUxZTFlO1xuICBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIGNvbG9yOiAjY2NjO1xuICBmb250LXNpemU6IDExcHg7XG4gIHBhZGRpbmc6IDNweCA2cHg7XG4gIG91dGxpbmU6IG5vbmU7XG4gIG1heC13aWR0aDogMjAwcHg7XG5cbiAgJjpmb2N1cyB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICB9XG5cbiAgb3B0aW9uIHtcbiAgICBiYWNrZ3JvdW5kOiAjMjUyNTI2O1xuICAgIGNvbG9yOiAjY2NjO1xuICB9XG59XG5cbi5zZXR0aW5ncy1sYWJlbCB7XG4gIGZvbnQtc2l6ZTogMTNweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgY29sb3I6ICNlMGUwZTA7XG59XG5cbi5zZXR0aW5ncy1yZXNldC1idG4ge1xuICBiYWNrZ3JvdW5kOiBub25lO1xuICBib3JkZXI6IDFweCBzb2xpZCAjNTU1O1xuICBjb2xvcjogIzg4ODtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBwYWRkaW5nOiAzcHggMTBweDtcbiAgYm9yZGVyLXJhZGl1czogM3B4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyIHtcbiAgICBib3JkZXItY29sb3I6ICNmZjk4MDA7XG4gICAgY29sb3I6ICNmZjk4MDA7XG4gIH1cbn1cblxuLnNldHRpbmdzLWhpbnQge1xuICBmb250LXNpemU6IDExcHg7XG4gIGNvbG9yOiAjODg4O1xuICBtYXJnaW46IDA7XG4gIGxpbmUtaGVpZ2h0OiAxLjU7XG59XG5cbi5zZXR0aW5ncy10ZXh0YXJlYSB7XG4gIHdpZHRoOiAxMDAlO1xuICBtaW4taGVpZ2h0OiAyMDBweDtcbiAgYmFja2dyb3VuZDogIzFlMWUxZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzNjM2MzYztcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBjb2xvcjogI2NjYztcbiAgZm9udC1mYW1pbHk6ICdDYXNjYWRpYSBDb2RlJywgJ0NvbnNvbGFzJywgJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xuICBmb250LXNpemU6IDEycHg7XG4gIGxpbmUtaGVpZ2h0OiAxLjU7XG4gIHBhZGRpbmc6IDEycHg7XG4gIHJlc2l6ZTogdmVydGljYWw7XG4gIG91dGxpbmU6IG5vbmU7XG4gIGJveC1zaXppbmc6IGJvcmRlci1ib3g7XG5cbiAgJjpmb2N1cyB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICB9XG59XG5cbi8qIFNjcm9sbGFibGUgc2Vzc2lvbiBjb250ZW50IChldmVyeXRoaW5nIGJlbG93IHRoZSBoZWFkZXIpICovXG4uc2Vzc2lvbi1jb250ZW50IHtcbiAgZmxleDogMSAxIDA7XG4gIG1pbi1oZWlnaHQ6IDA7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIG92ZXJmbG93LXg6IGhpZGRlbjtcbiAgcGFkZGluZzogMTZweCAyNHB4O1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDEycHg7XG59XG5cbi8qIENhcmRzIGFyZWEgKi9cbi5jYXJkcy1hcmVhIHtcbiAgZmxleDogMCAwIGF1dG87XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGdhcDogMTZweDtcbn1cblxuLmNhcmRzLXBsYWNlaG9sZGVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIGhlaWdodDogMjAwcHg7XG4gIGNvbG9yOiAjNTU1O1xuICBmb250LXNpemU6IDE0cHg7XG4gIGZvbnQtc3R5bGU6IGl0YWxpYztcbiAgYm9yZGVyOiAxcHggZGFzaGVkICMzYzNjM2M7XG4gIGJvcmRlci1yYWRpdXM6IDhweDtcbn1cblxuLyogQnVpbGQgT3V0cHV0IE92ZXJsYXkgKFRhc2sgOC4xKSAqL1xuLmJ1aWxkLW92ZXJsYXktYmFja2Ryb3Age1xuICBwb3NpdGlvbjogZml4ZWQ7XG4gIHRvcDogMDtcbiAgbGVmdDogMDtcbiAgcmlnaHQ6IDA7XG4gIGJvdHRvbTogMDtcbiAgYmFja2dyb3VuZDogcmdiYSgwLCAwLCAwLCAwLjYpO1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcbiAgei1pbmRleDogMTAwMDtcbn1cblxuLmJ1aWxkLW92ZXJsYXktY2FyZCB7XG4gIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGJvcmRlci1yYWRpdXM6IDhweDtcbiAgd2lkdGg6IDgwJTtcbiAgbWF4LXdpZHRoOiA4MDBweDtcbiAgbWF4LWhlaWdodDogODB2aDtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgYm94LXNoYWRvdzogMCA4cHggMzJweCByZ2JhKDAsIDAsIDAsIDAuNSk7XG59XG5cbi5idWlsZC1vdmVybGF5LWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uYnVpbGQtb3ZlcmxheS10aXRsZSB7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgY29sb3I6ICNmZmY7XG4gIGZsZXg6IDE7XG59XG5cbi5idWlsZC1vdmVybGF5LWNsb3NlIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiBub25lO1xuICBjb2xvcjogIzg4ODtcbiAgZm9udC1zaXplOiAxNnB4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHBhZGRpbmc6IDRweCA4cHg7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICY6aG92ZXIge1xuICAgIGNvbG9yOiAjZWY1MzUwO1xuICAgIGJhY2tncm91bmQ6IHJnYmEoMjM5LCA4MywgODAsIDAuMSk7XG4gIH1cbn1cblxuLmJ1aWxkLW92ZXJsYXktYm9keSB7XG4gIGZsZXg6IDE7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIHBhZGRpbmc6IDE2cHg7XG4gIG1pbi1oZWlnaHQ6IDA7XG59XG5cbi5idWlsZC1vdmVybGF5LXByZSB7XG4gIGZvbnQtZmFtaWx5OiAnQ2FzY2FkaWEgQ29kZScsICdDb25zb2xhcycsICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBsaW5lLWhlaWdodDogMS42O1xuICBjb2xvcjogI2NjYztcbiAgd2hpdGUtc3BhY2U6IHByZS13cmFwO1xuICB3b3JkLWJyZWFrOiBicmVhay13b3JkO1xuICBtYXJnaW46IDA7XG59XG5cbi5idWlsZC1vdmVybGF5LWZvb3RlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGp1c3RpZnktY29udGVudDogZmxleC1lbmQ7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uYnVpbGQtb3ZlcmxheS1jb3B5IHtcbiAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgY29sb3I6ICMwMDA7XG4gIGJvcmRlcjogbm9uZTtcbiAgcGFkZGluZzogNnB4IDE2cHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBmb250LXdlaWdodDogNjAwO1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGZpbHRlciAwLjE1cztcbiAgbWluLXdpZHRoOiA4MHB4O1xuXG4gICY6aG92ZXIge1xuICAgIGZpbHRlcjogYnJpZ2h0bmVzcygxLjEpO1xuICB9XG59XG4iXSwic291cmNlUm9vdCI6IiJ9 */"],
      changeDetection: 0
    });
  }
}

/***/ }),

/***/ 241:
/*!******************************************************!*\
  !*** ./src/app/promptlab/models/promptlab.models.ts ***!
  \******************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "DEFAULT_SEQUENCE_PROMPT": () => (/* binding */ DEFAULT_SEQUENCE_PROMPT),
/* harmony export */   "DEFAULT_SYSTEM_PROMPT": () => (/* binding */ DEFAULT_SYSTEM_PROMPT),
/* harmony export */   "DEFAULT_WORKFLOW_PROMPT": () => (/* binding */ DEFAULT_WORKFLOW_PROMPT)
/* harmony export */ });
const DEFAULT_SYSTEM_PROMPT = `You are a **prompt design assistant**. The user is building a structured prompt that will later be sent to an LLM for execution.

Your role:
- Help the user **formulate, refine, and improve** the prompt they are designing.
- When the user describes what the prompt should do, respond with suggestions on how to phrase it, structure it, or improve it.
- Use \`{{paramName}}\` placeholders for variable parts (file paths, directories, configurable values). NEVER substitute concrete values.
- Distinguish between input files (type: "file") and output files (type: "output_file"). An output file is one the prompt will create or write to.
- Keep the prompt **generic and reusable** — it must work with any input matching the parameter types.

Critical rules:
- **DO NOT execute the instructions** the user describes. You are designing the prompt, not running it.
- **DO NOT read, list, or access** files, folders, or any real data. The prompt will do that when executed later.
- **DO NOT generate concrete output** (tables, lists, reports). Generate the **instructions** that will produce that output.
- If the user says "read files from a folder and make a table", your job is to write a prompt that says "Read all files in {{sourceDir}} and generate a table with columns: ..." — NOT to actually read files and make the table.

Think of yourself as a ghostwriter: you write the script, someone else performs it.`;
const DEFAULT_SEQUENCE_PROMPT = `Generate a PlantUML sequence diagram that shows the interaction flow described in this prompt. Show actors (User, LLM), messages exchanged, and data flow. Include parameter values if available.
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8
Use colored participants: actor User #E3F2FD, participant LLM #FFF3E0, participant FileSystem #E8F5E9.
Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;
const DEFAULT_WORKFLOW_PROMPT = `Generate a PlantUML activity diagram that shows the workflow steps described in this prompt. Show input, processing steps, decisions, and output. Include parameter values if available.
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8
Use colored partitions: #E3F2FD for input steps, #FFF3E0 for processing, #E8F5E9 for output. Use start/stop nodes.
Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;

/***/ }),

/***/ 5342:
/*!*******************************************************!*\
  !*** ./src/app/promptlab/promptlab-routing.module.ts ***!
  \*******************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabRoutingModule": () => (/* binding */ PromptLabRoutingModule)
/* harmony export */ });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/router */ 124);
/* harmony import */ var _components_promptlab_promptlab_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./components/promptlab/promptlab.component */ 6739);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);




const routes = [{
  path: '',
  component: _components_promptlab_promptlab_component__WEBPACK_IMPORTED_MODULE_0__.PromptLabComponent
}];
class PromptLabRoutingModule {
  static {
    this.ɵfac = function PromptLabRoutingModule_Factory(t) {
      return new (t || PromptLabRoutingModule)();
    };
  }
  static {
    this.ɵmod = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineNgModule"]({
      type: PromptLabRoutingModule
    });
  }
  static {
    this.ɵinj = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjector"]({
      imports: [_angular_router__WEBPACK_IMPORTED_MODULE_2__.RouterModule.forChild(routes), _angular_router__WEBPACK_IMPORTED_MODULE_2__.RouterModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵsetNgModuleScope"](PromptLabRoutingModule, {
    imports: [_angular_router__WEBPACK_IMPORTED_MODULE_2__.RouterModule],
    exports: [_angular_router__WEBPACK_IMPORTED_MODULE_2__.RouterModule]
  });
})();

/***/ }),

/***/ 6157:
/*!***********************************************!*\
  !*** ./src/app/promptlab/promptlab.module.ts ***!
  \***********************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabModule": () => (/* binding */ PromptLabModule)
/* harmony export */ });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! @ngx-translate/core */ 8699);
/* harmony import */ var _promptlab_routing_module__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./promptlab-routing.module */ 5342);
/* harmony import */ var _components_promptlab_promptlab_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./components/promptlab/promptlab.component */ 6739);
/* harmony import */ var _components_promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./components/promptlab-doc-panel/promptlab-doc-panel.component */ 1381);
/* harmony import */ var _components_promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./components/promptlab-agent-card/promptlab-agent-card.component */ 4060);
/* harmony import */ var _components_promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./components/promptlab-card/promptlab-card.component */ 748);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/core */ 2560);










class PromptLabModule {
  static {
    this.ɵfac = function PromptLabModule_Factory(t) {
      return new (t || PromptLabModule)();
    };
  }
  static {
    this.ɵmod = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵdefineNgModule"]({
      type: PromptLabModule
    });
  }
  static {
    this.ɵinj = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵdefineInjector"]({
      imports: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.CommonModule, _angular_forms__WEBPACK_IMPORTED_MODULE_7__.FormsModule, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_8__.MatLegacyDialogModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_9__.TranslateModule, _promptlab_routing_module__WEBPACK_IMPORTED_MODULE_0__.PromptLabRoutingModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵsetNgModuleScope"](PromptLabModule, {
    declarations: [_components_promptlab_promptlab_component__WEBPACK_IMPORTED_MODULE_1__.PromptLabComponent, _components_promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_2__.PromptLabDocPanelComponent, _components_promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_3__.PromptLabAgentCardComponent, _components_promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_4__.PromptLabCardComponent],
    imports: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.CommonModule, _angular_forms__WEBPACK_IMPORTED_MODULE_7__.FormsModule, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_8__.MatLegacyDialogModule, _ngx_translate_core__WEBPACK_IMPORTED_MODULE_9__.TranslateModule, _promptlab_routing_module__WEBPACK_IMPORTED_MODULE_0__.PromptLabRoutingModule]
  });
})();

/***/ }),

/***/ 6479:
/*!**********************************************************************!*\
  !*** ./src/app/promptlab/services/promptlab-distillation.service.ts ***!
  \**********************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabDistillationService": () => (/* binding */ PromptLabDistillationService)
/* harmony export */ });
/* harmony import */ var C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../services/ai-chat.service */ 9109);





class PromptLabDistillationService {
  constructor(aiChatService) {
    this.aiChatService = aiChatService;
    /** Per-card result streams */
    this.resultSubjects = new Map();
    /** Per-card debounce timers */
    this.debounceTimers = new Map();
    /** Per-card stream subscriptions (so we can cancel a running distillation) */
    this.activeSubscriptions = new Map();
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_2__.Subject();
  }
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.debounceTimers.forEach(t => clearTimeout(t));
    this.activeSubscriptions.forEach(s => s.unsubscribe());
  }
  // ---------------------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------------------
  /**
   * Trigger distillation for a card. Debounced at 1500 ms.
   * If called again before the timer fires, the previous call is cancelled.
   */
  triggerDistillation(cardId, conversation, currentPrompt, model) {
    // Cancel any pending debounce
    if (this.debounceTimers.has(cardId)) {
      clearTimeout(this.debounceTimers.get(cardId));
    }
    const timer = setTimeout(() => {
      this.debounceTimers.delete(cardId);
      this.executeDistillation(cardId, conversation, currentPrompt, model);
    }, 1500);
    this.debounceTimers.set(cardId, timer);
  }
  /**
   * Returns an Observable that emits whenever distillation completes for the
   * given card.
   */
  getDistillationResult$(cardId) {
    if (!this.resultSubjects.has(cardId)) {
      this.resultSubjects.set(cardId, new rxjs__WEBPACK_IMPORTED_MODULE_2__.Subject());
    }
    return this.resultSubjects.get(cardId);
  }
  /**
   * Clean up resources for a card that no longer exists.
   */
  disposeCard(cardId) {
    if (this.debounceTimers.has(cardId)) {
      clearTimeout(this.debounceTimers.get(cardId));
      this.debounceTimers.delete(cardId);
    }
    if (this.activeSubscriptions.has(cardId)) {
      this.activeSubscriptions.get(cardId).unsubscribe();
      this.activeSubscriptions.delete(cardId);
    }
    if (this.resultSubjects.has(cardId)) {
      this.resultSubjects.get(cardId).complete();
      this.resultSubjects.delete(cardId);
    }
  }
  // ---------------------------------------------------------------------------
  // Private
  // ---------------------------------------------------------------------------
  executeDistillation(cardId, conversation, currentPrompt, model) {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      // Cancel any running distillation for this card
      if (_this.activeSubscriptions.has(cardId)) {
        _this.activeSubscriptions.get(cardId).unsubscribe();
        _this.activeSubscriptions.delete(cardId);
      }
      const channelId = `card-${cardId}-distill`;
      // Clear previous distillation history on the backend
      _this.aiChatService.clearChannelHistory(channelId);
      // Build the distillation prompt
      const distillationPrompt = _this.buildDistillationPrompt(conversation, currentPrompt);
      // Collect the full response from the distillation channel
      let responseBuffer = '';
      const sub = _this.aiChatService.getChannelStream$(channelId).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_3__.takeUntil)(_this.destroy$)).subscribe(event => {
        switch (event.type) {
          case 'chunk':
            responseBuffer += event.data;
            break;
          case 'complete':
            const result = _this.parseDistillationResponse(responseBuffer);
            if (result) {
              _this.getDistillationResult$(cardId).next(result);
            }
            responseBuffer = '';
            // Unsubscribe — one-shot per distillation
            if (_this.activeSubscriptions.has(cardId)) {
              _this.activeSubscriptions.get(cardId).unsubscribe();
              _this.activeSubscriptions.delete(cardId);
            }
            break;
          case 'error':
            console.error(`[PromptLabDistillation] Error for card ${cardId}:`, event.data);
            responseBuffer = '';
            if (_this.activeSubscriptions.has(cardId)) {
              _this.activeSubscriptions.get(cardId).unsubscribe();
              _this.activeSubscriptions.delete(cardId);
            }
            break;
        }
      });
      _this.activeSubscriptions.set(cardId, sub);
      // Ensure chat mode is set before sending
      const provider = model?.toLowerCase().includes('llama') ? 'local' : 'copilotcli';
      const modelId = model || 'claude-sonnet-4';
      yield _this.aiChatService.setProviderAsync(provider, modelId);
      // Send the distillation request to the LLM
      _this.aiChatService.sendMessageToChannel(distillationPrompt, channelId);
    })();
  }
  buildDistillationPrompt(conversation, currentPrompt) {
    const conversationText = conversation.map(m => {
      // Strip the [System Instructions]...[End System Instructions] block
      // that gets prepended to the first user message — the distillation
      // LLM has its own instructions and doesn't need the chat system prompt.
      let content = m.content;
      if (m.role === 'user') {
        content = content.replace(/\[System Instructions\][\s\S]*?\[End System Instructions\]\s*/, '').trim();
      }
      return `[${m.role.toUpperCase()}]: ${content}`;
    }).filter(line => line.length > 0).join('\n\n');
    return `You are a prompt refinement assistant. The user is having a conversation with an LLM to iteratively build and improve a prompt. Your job is to produce an updated version of the prompt that incorporates all the refinements discussed in the conversation.

Rules:
- Start from the current prompt (below) and apply the changes discussed in the conversation.
- If there is no current prompt, create one from scratch based on the conversation.
- The prompt must be standalone and executable without the conversation context.
- Use {{paramName}} syntax for variable parts (input files, output directories, configurable values).
- For each parameter, specify its type: "file" (an existing document to read), "output_file" (a file to create/write — the user will choose folder + filename), "directory" (a folder), or "text" (a free-form value).
- Generate a short title (max 6 words) describing what the prompt does.
- Write the prompt in the same language the user is using in the conversation.

Respond in this EXACT format (no extra text outside the markers):
---TITLE---
[title here]
---PARAMETERS---
[paramName]|[type]
[paramName]|[type]
---PROMPT---
[the updated prompt with {{paramName}} placeholders]

Current prompt (if any):
${currentPrompt || '(none)'}

Conversation:
${conversationText}`;
  }
  /**
   * Parse the LLM response using the ---TITLE--- / ---PARAMETERS--- / ---PROMPT--- markers.
   * Handles extra whitespace and minor formatting variations.
   */
  parseDistillationResponse(raw) {
    try {
      const titleMatch = raw.match(/---TITLE---\s*([\s\S]*?)\s*---PARAMETERS---/i);
      const paramsMatch = raw.match(/---PARAMETERS---\s*([\s\S]*?)\s*---PROMPT---/i);
      const promptMatch = raw.match(/---PROMPT---\s*([\s\S]*)/i);
      if (!titleMatch || !promptMatch) {
        console.warn('[PromptLabDistillation] Could not parse response — markers not found.');
        return null;
      }
      const generatedTitle = titleMatch[1].trim();
      const distilledPrompt = promptMatch[1].trim();
      const parameters = [];
      if (paramsMatch) {
        const paramLines = paramsMatch[1].split('\n').map(l => l.trim()).filter(l => l.length > 0);
        for (const line of paramLines) {
          const parts = line.split('|').map(p => p.trim());
          if (parts.length >= 2) {
            const paramType = this.normalizeParamType(parts[1]);
            parameters.push({
              name: parts[0],
              value: '',
              type: paramType
            });
          }
        }
      }
      return {
        distilledPrompt,
        parameters,
        generatedTitle
      };
    } catch (err) {
      console.error('[PromptLabDistillation] Parse error:', err);
      return null;
    }
  }
  normalizeParamType(raw) {
    const lower = raw.toLowerCase();
    if (lower === 'file') return 'file';
    if (lower === 'output_file' || lower === 'output file' || lower === 'outputfile' || lower === 'save_as') return 'output_file';
    if (lower === 'directory' || lower === 'dir' || lower === 'folder') return 'directory';
    return 'text';
  }
  static {
    this.ɵfac = function PromptLabDistillationService_Factory(t) {
      return new (t || PromptLabDistillationService)(_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵinject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_1__.AiChatService));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_4__["ɵɵdefineInjectable"]({
      token: PromptLabDistillationService,
      factory: PromptLabDistillationService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 2349:
/*!*********************************************************************!*\
  !*** ./src/app/promptlab/services/promptlab-persistence.service.ts ***!
  \*********************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabPersistenceService": () => (/* binding */ PromptLabPersistenceService)
/* harmony export */ });
/* harmony import */ var _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../models/promptlab.models */ 241);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ 2560);


class PromptLabPersistenceService {
  // ---------------------------------------------------------------------------
  // Public API
  // ---------------------------------------------------------------------------
  /**
   * Parse a Markdown template into a PromptLabSession model.
   */
  loadTemplate(markdown) {
    const frontMatter = this.parseFrontMatter(markdown);
    const body = this.stripFrontMatter(markdown);
    const mode = frontMatter['mode'] === 'agent' ? 'agent' : 'prompt';
    // Parse prompt sections (if present)
    const systemPromptResult = this.parseNamedSection(body, 'System Prompt', _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT);
    let remainingBody = systemPromptResult.remaining;
    const sequenceResult = this.parseNamedSection(remainingBody, 'Sequence Prompt', _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SEQUENCE_PROMPT);
    remainingBody = sequenceResult.remaining;
    const workflowResult = this.parseNamedSection(remainingBody, 'Workflow Prompt', _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_WORKFLOW_PROMPT);
    remainingBody = workflowResult.remaining;
    let agentDefinition;
    if (mode === 'agent') {
      const agentResult = this.parseAgentSection(remainingBody);
      agentDefinition = agentResult.agent;
      remainingBody = agentResult.remaining;
    }
    const cards = this.parseCards(remainingBody);
    return {
      id: this.generateUUID(),
      title: frontMatter['title'] || '',
      model: frontMatter['model'] || '',
      mode,
      systemPrompt: systemPromptResult.content,
      systemPromptModel: frontMatter['system_prompt_model'] || '',
      sequencePrompt: sequenceResult.content,
      sequencePromptModel: frontMatter['sequence_prompt_model'] || '',
      workflowPrompt: workflowResult.content,
      workflowPromptModel: frontMatter['workflow_prompt_model'] || '',
      agentDefinition,
      cards,
      createdAt: frontMatter['created'] ? new Date(frontMatter['created']) : new Date(),
      updatedAt: frontMatter['updated'] ? new Date(frontMatter['updated']) : new Date(),
      templatePath: ''
    };
  }
  /**
   * Serialize a PromptLabSession model back to Markdown template format.
   */
  saveTemplate(session) {
    const lines = [];
    // Front matter
    lines.push('---');
    lines.push('promptlab: true');
    lines.push('version: 1');
    lines.push(`title: ${session.title}`);
    lines.push(`model: ${session.model}`);
    lines.push(`mode: ${session.mode}`);
    if (session.systemPromptModel) lines.push(`system_prompt_model: ${session.systemPromptModel}`);
    if (session.sequencePromptModel) lines.push(`sequence_prompt_model: ${session.sequencePromptModel}`);
    if (session.workflowPromptModel) lines.push(`workflow_prompt_model: ${session.workflowPromptModel}`);
    lines.push(`created: ${this.formatDate(session.createdAt)}`);
    lines.push(`updated: ${this.formatDate(session.updatedAt)}`);
    lines.push('---');
    lines.push('');
    // Prompt sections (only written if non-empty)
    const promptSections = [{
      heading: 'System Prompt',
      content: session.systemPrompt
    }, {
      heading: 'Sequence Prompt',
      content: session.sequencePrompt
    }, {
      heading: 'Workflow Prompt',
      content: session.workflowPrompt
    }];
    for (const section of promptSections) {
      if (section.content) {
        lines.push(`## ${section.heading}`);
        lines.push('');
        lines.push(section.content);
        lines.push('');
        lines.push('---');
        lines.push('');
      }
    }
    // Agent section
    if (session.mode === 'agent' && session.agentDefinition) {
      lines.push(...this.serializeAgentSection(session.agentDefinition));
      lines.push('---');
      lines.push('');
    }
    // Cards
    session.cards.forEach((card, index) => {
      if (index > 0) {
        lines.push('---');
        lines.push('');
      }
      lines.push(...this.serializeCard(card));
    });
    return lines.join('\n');
  }
  /**
   * Build a ready-to-execute Markdown string from a PromptLabSession.
   * Parameters in {{param}} placeholders are replaced with their actual values.
   */
  buildSession(session) {
    const sections = [];
    // Agent system prompt
    if (session.mode === 'agent' && session.agentDefinition) {
      sections.push(this.buildAgentSystemPrompt(session.agentDefinition));
    }
    // Resolved cards
    for (const card of session.cards) {
      const resolved = this.resolveParameters(card.distilledPrompt, card.parameters);
      sections.push(resolved);
    }
    return sections.join('\n\n---\n\n');
  }
  // ---------------------------------------------------------------------------
  // Front matter helpers
  // ---------------------------------------------------------------------------
  parseFrontMatter(markdown) {
    const result = {};
    const match = markdown.match(/^---\r?\n([\s\S]*?)\r?\n---/);
    if (!match) {
      return result;
    }
    const yamlBlock = match[1];
    for (const line of yamlBlock.split(/\r?\n/)) {
      const colonIndex = line.indexOf(':');
      if (colonIndex === -1) continue;
      const key = line.substring(0, colonIndex).trim();
      const value = line.substring(colonIndex + 1).trim();
      result[key] = value;
    }
    return result;
  }
  stripFrontMatter(markdown) {
    return markdown.replace(/^---\r?\n[\s\S]*?\r?\n---\r?\n?/, '');
  }
  // ---------------------------------------------------------------------------
  // Named section parsing (System Prompt, Sequence Prompt, Workflow Prompt)
  // ---------------------------------------------------------------------------
  parseNamedSection(body, heading, defaultValue) {
    const headingRegex = new RegExp(`^## ${this.escapeRegex(heading)}\\s*$`, 'm');
    const match = headingRegex.exec(body);
    if (!match) {
      return {
        content: defaultValue,
        remaining: body
      };
    }
    const afterHeading = body.substring(match.index + match[0].length);
    const endMatch = afterHeading.match(/\r?\n---\r?\n/);
    const nextH2 = afterHeading.match(/\r?\n## /);
    let endIndex;
    if (endMatch && nextH2) {
      endIndex = Math.min(endMatch.index, nextH2.index);
    } else {
      endIndex = endMatch?.index ?? nextH2?.index ?? afterHeading.length;
    }
    const content = afterHeading.substring(0, endIndex).trim();
    const remaining = endMatch && endMatch.index === endIndex ? afterHeading.substring(endIndex + endMatch[0].length) : afterHeading.substring(endIndex);
    return {
      content: content || defaultValue,
      remaining: body.substring(0, match.index) + remaining
    };
  }
  // ---------------------------------------------------------------------------
  // Agent section parsing
  // ---------------------------------------------------------------------------
  parseAgentSection(body) {
    const agentHeadingRegex = /^## Agent\s*$/m;
    const agentMatch = agentHeadingRegex.exec(body);
    if (!agentMatch) {
      return {
        agent: {
          identity: '',
          objectives: '',
          rules: '',
          tools: []
        },
        remaining: body
      };
    }
    // Find where the agent section ends: at the first `---` separator or first `## Card:`
    const afterAgent = body.substring(agentMatch.index + agentMatch[0].length);
    const endMatch = afterAgent.match(/\r?\n---\r?\n/);
    const agentContent = endMatch ? afterAgent.substring(0, endMatch.index) : afterAgent;
    const remaining = endMatch ? afterAgent.substring(endMatch.index + endMatch[0].length) : '';
    const identity = this.extractSubSection(agentContent, "Identita'");
    const objectives = this.extractSubSection(agentContent, 'Obiettivi');
    const rules = this.extractSubSection(agentContent, 'Regole');
    const toolsRaw = this.extractSubSection(agentContent, 'Strumenti');
    const tools = toolsRaw.split(/\r?\n/).map(line => line.replace(/^\s*-\s*/, '').trim()).filter(line => line.length > 0);
    return {
      agent: {
        identity,
        objectives,
        rules,
        tools
      },
      remaining
    };
  }
  extractSubSection(content, heading) {
    const regex = new RegExp(`^### ${this.escapeRegex(heading)}\\s*$`, 'm');
    const match = regex.exec(content);
    if (!match) return '';
    const after = content.substring(match.index + match[0].length);
    // Content goes until the next ### heading or end of string
    const nextHeading = after.match(/\r?\n###\s/);
    const text = nextHeading ? after.substring(0, nextHeading.index) : after;
    return text.trim();
  }
  // ---------------------------------------------------------------------------
  // Card parsing
  // ---------------------------------------------------------------------------
  parseCards(body) {
    const cards = [];
    // Split on `## Card: ` headings, keeping the heading text
    const cardRegex = /^## Card:\s*(.+)$/gm;
    const matches = [];
    let m;
    while ((m = cardRegex.exec(body)) !== null) {
      matches.push({
        title: m[1].trim(),
        index: m.index
      });
    }
    for (let i = 0; i < matches.length; i++) {
      const start = matches[i].index;
      const end = i + 1 < matches.length ? matches[i + 1].index : body.length;
      let cardContent = body.substring(start, end);
      // Strip trailing separator
      cardContent = cardContent.replace(/\r?\n---\s*$/, '');
      const title = matches[i].title;
      const id = this.extractCardId(cardContent, i + 1);
      const parameters = this.parseParameterTable(cardContent);
      const distilledPrompt = this.extractPromptText(cardContent);
      // Cross-check: find {{param}} in prompt and ensure they exist in parameters
      const promptParams = this.extractParamPlaceholders(distilledPrompt);
      for (const pName of promptParams) {
        if (!parameters.find(p => p.name === pName)) {
          parameters.push({
            name: pName,
            value: '',
            type: 'text'
          });
        }
      }
      // Parse cached diagrams
      const sequenceDiagram = this.extractDiagramCache(cardContent, 'Sequence Diagram');
      const workflowDiagram = this.extractDiagramCache(cardContent, 'Workflow Diagram');
      cards.push({
        id,
        generatedTitle: title,
        parameters,
        distilledPrompt,
        conversation: [],
        lastRun: undefined,
        sequenceDiagram,
        workflowDiagram
      });
    }
    return cards;
  }
  extractCardId(cardContent, fallbackIndex) {
    const match = cardContent.match(/<!--\s*promptlab-card-id:\s*([\w-]+)\s*-->/);
    if (match) return match[1];
    // Generate a fallback ID
    const idx = String(fallbackIndex).padStart(3, '0');
    return `card-${idx}`;
  }
  parseParameterTable(cardContent) {
    const params = [];
    // Find ### Parametri section
    const sectionMatch = cardContent.match(/^### Parametri\s*$/m);
    if (!sectionMatch) return params;
    const after = cardContent.substring(sectionMatch.index + sectionMatch[0].length);
    // Find the table rows (skip header row and separator row)
    const lines = after.split(/\r?\n/);
    let tableStarted = false;
    let headerSkipped = false;
    for (const line of lines) {
      const trimmed = line.trim();
      // Stop at next heading or end
      if (trimmed.startsWith('###') || trimmed.startsWith('## ')) break;
      if (!trimmed.startsWith('|')) {
        if (tableStarted) break; // Table ended
        continue;
      }
      tableStarted = true;
      // Skip header row
      if (!headerSkipped) {
        headerSkipped = true;
        continue;
      }
      // Skip separator row (|---|---|---|)
      if (/^\|[\s-|]+\|$/.test(trimmed)) continue;
      // Parse data row
      const cells = trimmed.split('|').map(c => c.trim()).filter(c => c.length > 0);
      if (cells.length >= 2) {
        const name = cells[0];
        const type = this.parseParameterType(cells[1]);
        const value = cells.length >= 3 ? cells[2] : '';
        params.push({
          name,
          type,
          value
        });
      }
    }
    return params;
  }
  parseParameterType(raw) {
    const normalized = raw.trim().toLowerCase();
    if (normalized === 'file') return 'file';
    if (normalized === 'output_file') return 'output_file';
    if (normalized === 'directory') return 'directory';
    return 'text';
  }
  extractPromptText(cardContent) {
    const promptMatch = cardContent.match(/^### Prompt\s*$/m);
    if (!promptMatch) return '';
    const after = cardContent.substring(promptMatch.index + promptMatch[0].length);
    // Prompt text goes until next `## `, `### `, or `---` separator, or end
    const endMatch = after.match(/\r?\n(?:---|## |### )/);
    const text = endMatch ? after.substring(0, endMatch.index) : after;
    return text.trim();
  }
  /**
   * Extract a cached diagram reference from a card section.
   * Format in Markdown:
   *   ### Sequence Diagram
   *   ![abc123](assets/card-001-sequence.svg)
   */
  extractDiagramCache(cardContent, heading) {
    const regex = new RegExp(`^### ${this.escapeRegex(heading)}\\s*$`, 'm');
    const match = regex.exec(cardContent);
    if (!match) return undefined;
    const after = cardContent.substring(match.index + match[0].length);
    const endMatch = after.match(/\r?\n(?:---|## |### )/);
    const sectionText = endMatch ? after.substring(0, endMatch.index) : after;
    // Extract ![hash](path) image link
    const imgMatch = sectionText.match(/!\[([^\]]*)\]\(([^)]+)\)/);
    if (!imgMatch) return undefined;
    return {
      promptHash: imgMatch[1],
      svgPath: imgMatch[2]
    };
  }
  extractParamPlaceholders(text) {
    const result = [];
    const regex = /\{\{(\w+)\}\}/g;
    let match;
    while ((match = regex.exec(text)) !== null) {
      if (!result.includes(match[1])) {
        result.push(match[1]);
      }
    }
    return result;
  }
  // ---------------------------------------------------------------------------
  // Serialization helpers
  // ---------------------------------------------------------------------------
  serializeAgentSection(agent) {
    const lines = [];
    lines.push("## Agent");
    lines.push('');
    lines.push("### Identita'");
    lines.push('');
    lines.push(agent.identity);
    lines.push('');
    lines.push('### Obiettivi');
    lines.push('');
    lines.push(agent.objectives);
    lines.push('');
    lines.push('### Regole');
    lines.push('');
    lines.push(agent.rules);
    lines.push('');
    lines.push('### Strumenti');
    lines.push('');
    for (const tool of agent.tools) {
      lines.push(`- ${tool}`);
    }
    lines.push('');
    return lines;
  }
  serializeCard(card) {
    const lines = [];
    lines.push(`## Card: ${card.generatedTitle}`);
    lines.push('');
    lines.push(`<!-- promptlab-card-id: ${card.id} -->`);
    lines.push('');
    lines.push('### Parametri');
    lines.push('');
    lines.push('| Nome | Tipo | Valore |');
    lines.push('|------|------|--------|');
    for (const param of card.parameters) {
      lines.push(`| ${param.name} | ${param.type} | ${param.value} |`);
    }
    lines.push('');
    lines.push('### Prompt');
    lines.push('');
    lines.push(card.distilledPrompt);
    lines.push('');
    // Cached diagram references
    if (card.sequenceDiagram?.svgPath) {
      lines.push('### Sequence Diagram');
      lines.push('');
      lines.push(`![${card.sequenceDiagram.promptHash}](${card.sequenceDiagram.svgPath})`);
      lines.push('');
    }
    if (card.workflowDiagram?.svgPath) {
      lines.push('### Workflow Diagram');
      lines.push('');
      lines.push(`![${card.workflowDiagram.promptHash}](${card.workflowDiagram.svgPath})`);
      lines.push('');
    }
    return lines;
  }
  formatDate(date) {
    if (!date || !(date instanceof Date) || isNaN(date.getTime())) {
      return new Date().toISOString().replace(/\.\d{3}Z$/, '');
    }
    return date.toISOString().replace(/\.\d{3}Z$/, '');
  }
  // ---------------------------------------------------------------------------
  // Build helpers
  // ---------------------------------------------------------------------------
  /**
   * Public accessor for buildAgentSystemPrompt, used by PromptLabService
   * when building prompts for card execution.
   */
  buildAgentSystemPromptPublic(agent) {
    return this.buildAgentSystemPrompt(agent);
  }
  buildAgentSystemPrompt(agent) {
    const parts = [];
    if (agent.identity) {
      parts.push(agent.identity);
    }
    if (agent.objectives) {
      parts.push(`Obiettivi:\n${agent.objectives}`);
    }
    if (agent.rules) {
      parts.push(`Regole:\n${agent.rules}`);
    }
    if (agent.tools && agent.tools.length > 0) {
      const toolsList = agent.tools.map(t => `- ${t}`).join('\n');
      parts.push(`Strumenti disponibili:\n${toolsList}`);
    }
    return parts.join('\n\n');
  }
  resolveParameters(prompt, parameters) {
    let resolved = prompt;
    for (const param of parameters) {
      const regex = new RegExp(`\\{\\{${this.escapeRegex(param.name)}\\}\\}`, 'g');
      resolved = resolved.replace(regex, param.value);
    }
    return resolved;
  }
  // ---------------------------------------------------------------------------
  // Utilities
  // ---------------------------------------------------------------------------
  generateUUID() {
    // Simple UUID v4 generator (no crypto dependency needed)
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : r & 0x3 | 0x8;
      return v.toString(16);
    });
  }
  escapeRegex(str) {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }
  static {
    this.ɵfac = function PromptLabPersistenceService_Factory(t) {
      return new (t || PromptLabPersistenceService)();
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({
      token: PromptLabPersistenceService,
      factory: PromptLabPersistenceService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ }),

/***/ 3819:
/*!*********************************************************!*\
  !*** ./src/app/promptlab/services/promptlab.service.ts ***!
  \*********************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "PromptLabService": () => (/* binding */ PromptLabService)
/* harmony export */ });
/* harmony import */ var C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./node_modules/@babel/runtime/helpers/esm/asyncToGenerator.js */ 1670);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! rxjs */ 6317);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! rxjs/operators */ 7260);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! rxjs/operators */ 116);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! rxjs/operators */ 1989);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! rxjs/operators */ 9295);
/* harmony import */ var _models_promptlab_models__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../models/promptlab.models */ 241);
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/ai-chat.service */ 9109);
/* harmony import */ var _promptlab_persistence_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./promptlab-persistence.service */ 2349);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../signalR/services/server-messages.service */ 8635);
/* harmony import */ var _ngx_translate_core__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @ngx-translate/core */ 8699);










class PromptLabService {
  constructor(aiChatService, persistenceService, http, serverMessages, translate) {
    this.aiChatService = aiChatService;
    this.persistenceService = persistenceService;
    this.http = http;
    this.serverMessages = serverMessages;
    this.translate = translate;
    this._session$ = new rxjs__WEBPACK_IMPORTED_MODULE_5__.BehaviorSubject(null);
    this.session$ = this._session$.asObservable();
    /** Emits true while a save is in progress. */
    this._isSaving$ = new rxjs__WEBPACK_IMPORTED_MODULE_5__.BehaviorSubject(false);
    this.isSaving$ = this._isSaving$.asObservable();
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_6__.Subject();
    /**
     * Counter that tracks how many emissions to skip for auto-save.
     * Incremented when loadSession() is called (the initial load should NOT
     * trigger a save back to disk).
     */
    this.skipNextSave = 0;
    // ---------------------------------------------------------------------------
    // Execute
    // ---------------------------------------------------------------------------
    /** Tracks which card IDs are currently executing (Play). */
    this._executingCards = new Set();
    /** Emits true while executeAll() is running. */
    this._isExecutingAll$ = new rxjs__WEBPACK_IMPORTED_MODULE_5__.BehaviorSubject(false);
    this.isExecutingAll$ = this._isExecutingAll$.asObservable();
    this._lastSentPrompts = new Map();
    this.initAutoSave();
  }
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
  // ---------------------------------------------------------------------------
  // Auto-save (Task 7.2)
  // ---------------------------------------------------------------------------
  initAutoSave() {
    this._session$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_7__.takeUntil)(this.destroy$),
    // Skip the very first null emission from BehaviorSubject constructor
    (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_8__.skip)(1),
    // Only save if we have a session with a valid templatePath
    (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.filter)(session => {
      if (!session || !session.templatePath) return false;
      // If this emission came from loadSession(), skip it
      if (this.skipNextSave > 0) {
        this.skipNextSave--;
        return false;
      }
      return true;
    }), (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_10__.debounceTime)(3000)).subscribe(session => {
      if (session) {
        this.saveSessionToFile(session);
      }
    });
  }
  saveSessionToFile(session) {
    const markdown = this.persistenceService.saveTemplate(session);
    const connectionId = this.serverMessages.connectionId || '';
    const url = `/api/MdExplorerEditorReact/UpdateMarkdown?ConnectionId=${encodeURIComponent(connectionId)}`;
    this._isSaving$.next(true);
    this.http.post(url, {
      FilePath: session.templatePath,
      MarkdownContent: markdown
    }).subscribe({
      next: () => {
        console.log('[PromptLab] Auto-save completato per:', session.templatePath);
        this._isSaving$.next(false);
      },
      error: err => {
        console.error('[PromptLab] Errore auto-save:', err);
        this._isSaving$.next(false);
      }
    });
  }
  // ---------------------------------------------------------------------------
  // Session management
  // ---------------------------------------------------------------------------
  /**
   * Create a new empty session with the given title.
   */
  createSession(title) {
    const session = {
      id: this.generateId(),
      title,
      model: '',
      mode: 'prompt',
      systemPrompt: _models_promptlab_models__WEBPACK_IMPORTED_MODULE_1__.DEFAULT_SYSTEM_PROMPT,
      systemPromptModel: '',
      sequencePrompt: _models_promptlab_models__WEBPACK_IMPORTED_MODULE_1__.DEFAULT_SEQUENCE_PROMPT,
      sequencePromptModel: '',
      workflowPrompt: _models_promptlab_models__WEBPACK_IMPORTED_MODULE_1__.DEFAULT_WORKFLOW_PROMPT,
      workflowPromptModel: '',
      agentDefinition: undefined,
      cards: [],
      createdAt: new Date(),
      updatedAt: new Date(),
      templatePath: ''
    };
    this._session$.next(session);
  }
  /**
   * Parse a markdown template and load it as the active session.
   */
  loadSession(markdown, templatePath) {
    // Mark next emission to be skipped by auto-save
    this.skipNextSave++;
    const session = this.persistenceService.loadTemplate(markdown);
    session.templatePath = templatePath;
    this._session$.next(session);
  }
  // ---------------------------------------------------------------------------
  // Card management
  // ---------------------------------------------------------------------------
  /**
   * Add an empty card to the current session.
   */
  addCard() {
    const session = this.currentSession();
    if (!session) return;
    const newCard = {
      id: this.generateId(),
      generatedTitle: this.translate.instant('PROMPTLAB.NEW_CARD'),
      parameters: [],
      distilledPrompt: '',
      conversation: []
    };
    session.cards = [...session.cards, newCard];
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  /**
   * Remove a card by id from the current session.
   * Also clears the channel history on the backend.
   */
  removeCard(cardId) {
    const session = this.currentSession();
    if (!session) return;
    // Clear backend channel history for this card
    this.aiChatService.clearChannelHistory(this.bodyChannelId(cardId));
    this.aiChatService.clearChannelHistory(this.distillChannelId(cardId));
    session.cards = session.cards.filter(c => c.id !== cardId);
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  /**
   * Update a card in-place inside the current session.
   */
  updateCard(card) {
    const session = this.currentSession();
    if (!session) return;
    const idx = session.cards.findIndex(c => c.id === card.id);
    if (idx === -1) return;
    session.cards[idx] = card;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  /**
   * Push an externally modified session object to trigger auto-save.
   */
  updateSession(session) {
    this._session$.next({
      ...session
    });
  }
  // ---------------------------------------------------------------------------
  // Model & mode
  // ---------------------------------------------------------------------------
  setModel(model) {
    const session = this.currentSession();
    if (!session) return;
    session.model = model;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  setMode(mode) {
    const session = this.currentSession();
    if (!session) return;
    session.mode = mode;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  setSystemPrompt(prompt) {
    const session = this.currentSession();
    if (!session) return;
    session.systemPrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  setSequencePrompt(prompt) {
    const session = this.currentSession();
    if (!session) return;
    session.sequencePrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  setWorkflowPrompt(prompt) {
    const session = this.currentSession();
    if (!session) return;
    session.workflowPrompt = prompt;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  setAgentDefinition(def) {
    const session = this.currentSession();
    if (!session) return;
    session.agentDefinition = def;
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  // ---------------------------------------------------------------------------
  // Chat per card
  // ---------------------------------------------------------------------------
  /**
   * Ensure the hub has the right chat mode set for the current session model.
   * Must be called before sending any message.
   */
  ensureChatModePublic() {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      return _this.ensureChatMode();
    })();
  }
  ensureChatMode() {
    var _this2 = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      const session = _this2.currentSession();
      if (!session) return;
      // Default to claude-sonnet-4 if no model is set
      if (!session.model) {
        session.model = 'claude-sonnet-4';
      }
      const model = session.model.toLowerCase();
      // Local LLama models use the 'local' provider, everything else goes through copilotcli
      const provider = model.includes('llama') ? 'local' : 'copilotcli';
      const modelId = session.model;
      yield _this2.aiChatService.setProviderAsync(provider, modelId);
    })();
  }
  /**
   * Send a message on the card's body channel.
   * The channelId follows the pattern `card-{cardId}-body`.
   *
   * On the FIRST message of a conversation (empty history on backend),
   * the session's system prompt is prepended so the LLM knows its role
   * is to help design prompts — not to execute instructions.
   */
  sendCardMessage(cardId, message) {
    var _this3 = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      const session = _this3.currentSession();
      if (!session) return;
      const channelId = _this3.bodyChannelId(cardId);
      const card = session.cards.find(c => c.id === cardId);
      // Prepend system prompt on the first user message in this conversation.
      // We check the local conversation length: if it has only 1 message
      // (the one just pushed by the card component), it's the first turn.
      const isFirstMessage = card && card.conversation.length <= 1;
      let messageToSend = message;
      if (isFirstMessage) {
        const systemPrompt = session.systemPrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_1__.DEFAULT_SYSTEM_PROMPT;
        messageToSend = `[System Instructions]\n${systemPrompt}\n[End System Instructions]\n\n${message}`;
      }
      yield _this3.ensureChatMode();
      _this3.aiChatService.sendMessageToChannel(messageToSend, channelId);
    })();
  }
  /**
   * Get the stream of events for a specific card's body channel.
   * Returns events with { type: 'chunk' | 'thinking' | 'complete' | 'error', data: any }.
   */
  getCardStream$(cardId) {
    return this.aiChatService.getChannelStream$(this.bodyChannelId(cardId));
  }
  /**
   * Get the stream of events for a specific card's distillation channel.
   */
  getCardDistillStream$(cardId) {
    return this.aiChatService.getChannelStream$(this.distillChannelId(cardId));
  }
  /**
   * Clear the backend conversation history for a card's body channel.
   */
  resetCardChat(cardId) {
    this.aiChatService.clearChannelHistory(this.bodyChannelId(cardId));
    // Also clear the local conversation in the session model
    const session = this.currentSession();
    if (!session) return;
    const card = session.cards.find(c => c.id === cardId);
    if (card) {
      card.conversation = [];
      session.updatedAt = new Date();
      this._session$.next({
        ...session
      });
    }
  }
  // ---------------------------------------------------------------------------
  // Build
  // ---------------------------------------------------------------------------
  /**
   * Resolve parameters for a single card and return the ready-to-send prompt.
   */
  buildCard(cardId) {
    const session = this.currentSession();
    if (!session) return '';
    const card = session.cards.find(c => c.id === cardId);
    if (!card) return '';
    return this.resolveParameters(card.distilledPrompt, card.parameters);
  }
  /**
   * Build the full session (all cards resolved) via the persistence service.
   */
  buildSession() {
    const session = this.currentSession();
    if (!session) return '';
    return this.persistenceService.buildSession(session);
  }
  /** Returns the full prompt that was sent to the LLM for a Play execution. */
  getLastSentPrompt(cardId) {
    return this._lastSentPrompts.get(cardId);
  }
  /** Check if a card is currently executing. */
  isCardExecuting(cardId) {
    return this._executingCards.has(cardId);
  }
  /**
   * Build a single card's prompt (with agent preamble if applicable),
   * resolve parameters, send it to the LLM, and track execution state.
   */
  executeCard(cardId) {
    const session = this.currentSession();
    if (!session) return;
    const card = session.cards.find(c => c.id === cardId);
    if (!card) return;
    // Build the resolved prompt
    let builtPrompt = this.resolveParameters(card.distilledPrompt, card.parameters);
    // If agent mode, prepend the agent system prompt
    if (session.mode === 'agent' && session.agentDefinition) {
      const agentPreamble = this.persistenceService.buildAgentSystemPromptPublic(session.agentDefinition);
      if (agentPreamble) {
        builtPrompt = agentPreamble + '\n\n---\n\n' + builtPrompt;
      }
    }
    // Track execution state
    this._executingCards.add(cardId);
    this._lastSentPrompts.set(cardId, builtPrompt);
    // Ensure provider is set, then send to LLM
    this.ensureChatMode().then(() => {
      this.aiChatService.sendMessageToChannel(builtPrompt, this.bodyChannelId(cardId));
    });
  }
  /**
   * Called by the card component when a Play execution completes.
   * Creates the lastRun object and updates the session.
   */
  completeCardExecution(cardId, output, startTime) {
    this._executingCards.delete(cardId);
    const session = this.currentSession();
    if (!session) return;
    const card = session.cards.find(c => c.id === cardId);
    if (!card) return;
    const promptSent = this._lastSentPrompts.get(cardId) || '';
    this._lastSentPrompts.delete(cardId);
    const resolvedParams = {};
    for (const p of card.parameters) {
      resolvedParams[p.name] = p.value;
    }
    card.lastRun = {
      executedAt: new Date(),
      duration: Date.now() - startTime,
      provider: session.model,
      model: session.model,
      resolvedParameters: resolvedParams,
      promptSent,
      output
    };
    session.updatedAt = new Date();
    this._session$.next({
      ...session
    });
  }
  /**
   * Execute all cards sequentially, waiting for each to complete before
   * starting the next one.
   */
  executeAll() {
    const session = this.currentSession();
    if (!session || session.cards.length === 0) return;
    this._isExecutingAll$.next(true);
    this.executeCardsSequentially([...session.cards], 0);
  }
  executeCardsSequentially(cards, index) {
    if (index >= cards.length) {
      this._isExecutingAll$.next(false);
      return;
    }
    const card = cards[index];
    const channelId = this.bodyChannelId(card.id);
    // Listen for completion on this card's channel, then proceed to next
    this.aiChatService.getChannelStream$(channelId).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.filter)(e => e.type === 'complete' || e.type === 'error'), (0,rxjs_operators__WEBPACK_IMPORTED_MODULE_11__.take)(1)).subscribe(() => {
      this.executeCardsSequentially(cards, index + 1);
    });
    this.executeCard(card.id);
  }
  // ---------------------------------------------------------------------------
  // Channel ID helpers
  // ---------------------------------------------------------------------------
  bodyChannelId(cardId) {
    return `card-${cardId}-body`;
  }
  distillChannelId(cardId) {
    return `card-${cardId}-distill`;
  }
  // ---------------------------------------------------------------------------
  // Private helpers
  // ---------------------------------------------------------------------------
  getCurrentModel() {
    return this.currentSession()?.model || 'claude-sonnet-4';
  }
  currentSession() {
    return this._session$.getValue();
  }
  generateId() {
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
  }
  resolveParameters(prompt, parameters) {
    let resolved = prompt;
    for (const param of parameters) {
      const regex = new RegExp(`\\{\\{${param.name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\}\\}`, 'g');
      resolved = resolved.replace(regex, param.value);
    }
    return resolved;
  }
  static {
    this.ɵfac = function PromptLabService_Factory(t) {
      return new (t || PromptLabService)(_angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_2__.AiChatService), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_promptlab_persistence_service__WEBPACK_IMPORTED_MODULE_3__.PromptLabPersistenceService), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_13__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__.MdServerMessagesService), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_ngx_translate_core__WEBPACK_IMPORTED_MODULE_14__.TranslateService));
    };
  }
  static {
    this.ɵprov = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵdefineInjectable"]({
      token: PromptLabService,
      factory: PromptLabService.ɵfac,
      providedIn: 'root'
    });
  }
}

/***/ })

}]);
//# sourceMappingURL=src_app_promptlab_promptlab_module_ts.js.map