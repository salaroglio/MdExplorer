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




function PromptLabAgentCardComponent_div_9_div_17_span_1_Template(rf, ctx) {
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
function PromptLabAgentCardComponent_div_9_div_17_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 13);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, PromptLabAgentCardComponent_div_9_div_17_span_1_Template, 2, 1, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r1 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", ctx_r1.agentDefinition.tools);
  }
}
function PromptLabAgentCardComponent_div_9_Template(rf, ctx) {
  if (rf & 1) {
    const _r5 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 7)(1, "div", 8)(2, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "Identit\u00E0");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_9_Template_textarea_blur_4_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r4.onFieldBlur("identity", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "div", 8)(6, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](7, "Obiettivi");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](8, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_9_Template_textarea_blur_8_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r6.onFieldBlur("objectives", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "div", 8)(10, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](11, "Regole");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "textarea", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_9_Template_textarea_blur_12_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r7.onFieldBlur("rules", $event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](13, "div", 8)(14, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](15, "Strumenti");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](16, "textarea", 11);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("blur", function PromptLabAgentCardComponent_div_9_Template_textarea_blur_16_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r5);
      const ctx_r8 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵresetView"](ctx_r8.onToolsBlur($event.target.value));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](17, PromptLabAgentCardComponent_div_9_div_17_Template, 2, 1, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.identity);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.objectives);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.rules);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngModel", ctx_r0.agentDefinition.tools.join(", "));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](1);
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
      decls: 10,
      vars: 3,
      consts: [[1, "agent-card"], [1, "agent-card-header"], [1, "agent-icon"], [1, "agent-title"], [1, "spacer"], [1, "collapse-btn", 3, "click"], ["class", "agent-card-body", 4, "ngIf"], [1, "agent-card-body"], [1, "agent-field"], [1, "agent-label"], ["rows", "2", 1, "agent-value", 3, "ngModel", "blur"], ["rows", "2", "placeholder", "Separati da virgola: tool1, tool2, ...", 1, "agent-value", 3, "ngModel", "blur"], ["class", "agent-tools", 4, "ngIf"], [1, "agent-tools"], ["class", "agent-tool", 4, "ngFor", "ngForOf"], [1, "agent-tool"]],
      template: function PromptLabAgentCardComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "span", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](3, "\u2699");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](4, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](5, "Definizione Agente");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](6, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "button", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function PromptLabAgentCardComponent_Template_button_click_7_listener() {
            return ctx.toggleCollapse();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8, "\u25BC");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](9, PromptLabAgentCardComponent_div_9_Template, 18, 5, "div", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵclassProp"]("collapsed", ctx.collapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](9);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx.collapsed);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_1__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_1__.NgIf, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_2__.NgModel],
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
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/core */ 2560);
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! rxjs */ 228);
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! rxjs/operators */ 8951);
/* harmony import */ var _commons_components_show_file_system_show_file_system_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-system.component */ 4699);
/* harmony import */ var _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../../commons/components/show-file-system/show-file-metadata */ 4625);
/* harmony import */ var _angular_platform_browser__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! @angular/platform-browser */ 4497);
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @angular/common/http */ 8987);
/* harmony import */ var _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/material/legacy-dialog */ 8446);
/* harmony import */ var _services_promptlab_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/promptlab.service */ 3819);
/* harmony import */ var _services_promptlab_distillation_service__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../services/promptlab-distillation.service */ 6479);
/* harmony import */ var _services_ai_chat_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../../services/ai-chat.service */ 9109);
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../../../md-explorer/services/projects.service */ 9753);
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! @angular/forms */ 2508);
















const _c0 = ["messagesContainer"];
function PromptLabCardComponent_div_10_ng_container_1_span_1_Template(rf, ctx) {
  if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span", 43)(1, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "input", 44, 45);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("input", function PromptLabCardComponent_div_10_ng_container_1_span_1_Template_input_input_3_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r20.editingParamValue = $event.target.value);
    })("keydown", function PromptLabCardComponent_div_10_ng_container_1_span_1_Template_input_keydown_3_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const param_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]().$implicit;
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r22.onParamEditKeydown($event, param_r15));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "button", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_div_10_ng_container_1_span_1_Template_button_click_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const param_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]().$implicit;
      const ctx_r24 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r24.confirmParamEdit(param_r15));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "\u2713");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const param_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]().$implicit;
    const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r17.getParamIcon(param_r15.type));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("value", ctx_r17.editingParamValue);
  }
}
function PromptLabCardComponent_div_10_ng_container_1_span_2_span_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, " = ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](2, "b");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const param_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2).$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](param_r15.value);
  }
}
function PromptLabCardComponent_div_10_ng_container_1_span_2_Template(rf, ctx) {
  if (rf & 1) {
    const _r31 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_div_10_ng_container_1_span_2_Template_span_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r31);
      const ctx_r30 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      const param_r15 = ctx_r30.$implicit;
      const i_r16 = ctx_r30.index;
      const ctx_r29 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r29.onParameterClick(param_r15, i_r16));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](2, PromptLabCardComponent_div_10_ng_container_1_span_2_span_2_Template, 4, 1, "span", 34);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const param_r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]().$implicit;
    const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngClass", ctx_r18.getParamDisplayClass(param_r15));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate2"](" ", ctx_r18.getParamIcon(param_r15.type), " ", param_r15.name, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", param_r15.value);
  }
}
function PromptLabCardComponent_div_10_ng_container_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementContainerStart"](0);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, PromptLabCardComponent_div_10_ng_container_1_span_1_Template, 7, 2, "span", 41);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](2, PromptLabCardComponent_div_10_ng_container_1_span_2_Template, 3, 4, "span", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementContainerEnd"]();
  }
  if (rf & 2) {
    const i_r16 = ctx.index;
    const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r14.editingParamIndex === i_r16);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r14.editingParamIndex !== i_r16);
  }
}
function PromptLabCardComponent_div_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](1, PromptLabCardComponent_div_10_ng_container_1_Template, 3, 2, "ng-container", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r0 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r0.card.parameters);
  }
}
function PromptLabCardComponent_span_18_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "span", 48);
  }
}
function PromptLabCardComponent_span_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "span", 48);
  }
}
function PromptLabCardComponent_div_23_div_21_div_4_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 59)(1, "span", 60);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 61);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "=");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "span", 62);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const key_r35 = ctx.$implicit;
    const ctx_r34 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](key_r35);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r34.card.lastRun.resolvedParameters[key_r35]);
  }
}
function PromptLabCardComponent_div_23_div_21_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 50)(1, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "Parametri risolti");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "div", 57);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](4, PromptLabCardComponent_div_23_div_21_div_4_Template, 7, 2, "div", 58);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r33 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx_r33.getRunParamKeys());
  }
}
function PromptLabCardComponent_div_23_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 49)(1, "div", 50)(2, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3, "Metadata esecuzione");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "div", 52)(5, "span", 53)(6, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](7, "Data:");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](9, "span", 53)(10, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](11, "Provider:");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](13, "span", 53)(14, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](15, "Modello:");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](17, "span", 53)(18, "strong");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "Durata:");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](20);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](21, PromptLabCardComponent_div_23_div_21_Template, 5, 1, "div", 54);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](22, "div", 50)(23, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](24, "Prompt inviato al LLM");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](25, "div", 55);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](26);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](27, "div", 50)(28, "div", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](29, "Output prodotto");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](30, "div", 56);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](31);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
  }
  if (rf & 2) {
    const ctx_r3 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r3.formatRunDate(ctx_r3.card.lastRun.executedAt), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r3.card.lastRun.provider, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r3.card.lastRun.model, "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r3.formatDuration(ctx_r3.card.lastRun.duration), "");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r3.getRunParamKeys().length > 0);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r3.card.lastRun.promptSent);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r3.card.lastRun.output);
  }
}
function PromptLabCardComponent_div_24_div_7_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 72);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Generazione diagramma...");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_div_24_div_8_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](0, "div", 73);
  }
  if (rf & 2) {
    const ctx_r37 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("innerHTML", ctx_r37.diagramSvg, _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵsanitizeHtml"]);
  }
}
function PromptLabCardComponent_div_24_pre_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "pre", 74);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r38 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r38.diagramPlantUml);
  }
}
function PromptLabCardComponent_div_24_div_10_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 75);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "Nessun diagramma generato.");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_div_24_Template(rf, ctx) {
  if (rf & 1) {
    const _r41 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 63)(1, "div", 64)(2, "span", 65);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](4, "button", 66);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_div_24_Template_button_click_4_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r41);
      const ctx_r40 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r40.copyDiagram());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](5, "Copia PlantUML");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "div", 67);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](7, PromptLabCardComponent_div_24_div_7_Template, 2, 0, "div", 68);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](8, PromptLabCardComponent_div_24_div_8_Template, 1, 1, "div", 69);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, PromptLabCardComponent_div_24_pre_9_Template, 2, 1, "pre", 70);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, PromptLabCardComponent_div_24_div_10_Template, 2, 0, "div", 71);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r4.activeDiagram === "sequence" ? "Sequence Diagram" : "Workflow Diagram");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", !ctx_r4.diagramPlantUml);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx_r4.isDiagramLoading);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx_r4.isDiagramLoading && ctx_r4.diagramSvg);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx_r4.isDiagramLoading && !ctx_r4.diagramSvg && ctx_r4.diagramPlantUml);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx_r4.isDiagramLoading && !ctx_r4.diagramSvg && !ctx_r4.diagramPlantUml);
  }
}
function PromptLabCardComponent_div_34_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 76)(1, "div", 77);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 78);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "div", 79);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const message_r42 = ctx.$implicit;
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngClass", message_r42.role);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", message_r42.role === "user" ? "Utente" : "Assistente", " ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r6.formatTime(message_r42.timestamp));
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](message_r42.content);
  }
}
function PromptLabCardComponent_div_35_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 76)(1, "div", 80);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, "Assistente");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "div", 81);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](5, "span", 82);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const ctx_r7 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r7.streamingContent);
  }
}
function PromptLabCardComponent_div_36_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 83);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, " Nessun messaggio. Scrivi qualcosa per iniziare la conversazione. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_div_37_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 84);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](1, "span", 85);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, " Distillazione in corso... ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_span_48_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "\u25B6 Mostra chat");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_span_49_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, "\u25C0 Nascondi chat");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabCardComponent_div_53_Template(rf, ctx) {
  if (rf & 1) {
    const _r44 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 86);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_div_53_Template_div_click_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r44);
      const ctx_r43 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r43.startEditingPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("innerHTML", ctx_r12.renderedPrompt, _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵsanitizeHtml"]);
  }
}
function PromptLabCardComponent_textarea_54_Template(rf, ctx) {
  if (rf & 1) {
    const _r47 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "textarea", 87, 88);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabCardComponent_textarea_54_Template_textarea_ngModelChange_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r47);
      const ctx_r46 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r46.editingPromptText = $event);
    })("blur", function PromptLabCardComponent_textarea_54_Template_textarea_blur_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r47);
      const ctx_r48 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r48.finishEditingPrompt());
    })("keydown.escape", function PromptLabCardComponent_textarea_54_Template_textarea_keydown_escape_0_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r47);
      const ctx_r49 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r49.finishEditingPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
  if (rf & 2) {
    const ctx_r13 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r13.editingPromptText);
  }
}
class PromptLabCardComponent {
  constructor(cdr, elRef, sanitizer, http, dialog, promptLabService, distillationService, aiChatService, projectsService) {
    this.cdr = cdr;
    this.elRef = elRef;
    this.sanitizer = sanitizer;
    this.http = http;
    this.dialog = dialog;
    this.promptLabService = promptLabService;
    this.distillationService = distillationService;
    this.aiChatService = aiChatService;
    this.projectsService = projectsService;
    this.isSingleCard = false;
    this.cardDeleted = new _angular_core__WEBPACK_IMPORTED_MODULE_7__.EventEmitter();
    this.cardChanged = new _angular_core__WEBPACK_IMPORTED_MODULE_7__.EventEmitter();
    this.chatCollapsed = false;
    this.chatInputText = '';
    this.chatColumnWidth = null;
    this.isDragging = false;
    /** Streaming state */
    this.isStreaming = false;
    this.streamingContent = '';
    /** Index of the parameter currently being inline-edited (text type only) */
    this.editingParamIndex = null;
    this.editingParamValue = '';
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_8__.Subject();
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
    this.promptLabService.getCardStream$(this.card.id).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$)).subscribe(event => {
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
    this.distillationService.getDistillationResult$(this.card.id).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$)).subscribe(result => {
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
    const title = this.card.generatedTitle || 'questa card';
    if (confirm(`Eliminare "${title}"?\n\nQuesta azione non può essere annullata.`)) {
      this.cardDeleted.emit(this.card.id);
    }
  }
  // ── Parameters ──
  getParamIcon(type) {
    switch (type) {
      case 'file':
        return '\uD83D\uDCC4';
      case 'directory':
        return '\uD83D\uDCC2';
      case 'text':
        return '\u270E';
      default:
        return '';
    }
  }
  getParamDisplayClass(param) {
    const base = param.type === 'directory' ? 'directory' : param.type;
    const state = param.value ? 'filled' : 'empty';
    return `${base} ${state}`;
  }
  onParameterClick(param, index) {
    if (param.type === 'text') {
      this.editingParamIndex = index;
      this.editingParamValue = param.value || '';
      this.cdr.markForCheck();
    } else {
      // file or directory — open MdExplorer file system dialog
      const data = new _commons_components_show_file_system_show_file_metadata__WEBPACK_IMPORTED_MODULE_2__.ShowFileMetadata();
      // Start from project root to keep selection within workspace
      const project = this.projectsService.currentProjects$.getValue();
      data.start = project?.path || 'project';
      if (param.type === 'directory') {
        data.title = `Seleziona cartella per "${param.name}"`;
        data.typeOfSelection = 'Folders';
        data.buttonText = 'Seleziona cartella';
      } else {
        data.title = `Seleziona file per "${param.name}"`;
        data.typeOfSelection = 'FoldersAndFiles';
        data.buttonText = 'Seleziona file';
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
    this.isDiagramLoading = true;
    this.cdr.markForCheck();
    this.generateDiagram(type);
  }
  generateDiagram(type) {
    var _this = this;
    return (0,C_sviluppo_mdExplorer_MdExplorer_client2_node_modules_babel_runtime_helpers_esm_asyncToGenerator_js__WEBPACK_IMPORTED_MODULE_0__["default"])(function* () {
      _this.cleanupDiagramSubscription();
      const channelId = `card-${_this.card.id}-diagram`;
      _this.aiChatService.clearChannelHistory(channelId);
      const colorDirectives = `
Use a clean, professional color scheme with these PlantUML skinparam directives at the top:
skinparam backgroundColor #FEFEFE
skinparam shadowing false
skinparam defaultFontName "Segoe UI"
skinparam roundCorner 8`;
      const systemPrompt = type === 'sequence' ? `Generate a PlantUML sequence diagram that shows the interaction flow described in this prompt. Show actors (User, LLM), messages exchanged, and data flow. Include parameter values if available. ${colorDirectives}\nUse colored participants: actor User #E3F2FD, participant LLM #FFF3E0, participant FileSystem #E8F5E9. Return ONLY the PlantUML code between @startuml and @enduml, nothing else.` : `Generate a PlantUML activity diagram that shows the workflow steps described in this prompt. Show input, processing steps, decisions, and output. Include parameter values if available. ${colorDirectives}\nUse colored partitions: #E3F2FD for input steps, #FFF3E0 for processing, #E8F5E9 for output. Use start/stop nodes. Return ONLY the PlantUML code between @startuml and @enduml, nothing else.`;
      const message = `${systemPrompt}\n\n--- Prompt ---\n${_this.card.distilledPrompt || '(nessun prompt distillato)'}`;
      let accumulated = '';
      _this.diagramSubscription = _this.aiChatService.getChannelStream$(channelId).pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(_this.destroy$)).subscribe(event => {
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
      // Ensure chat mode is set before sending
      yield _this.promptLabService.ensureChatModePublic();
      _this.aiChatService.sendMessageToChannel(message, channelId);
    })();
  }
  renderPlantUmlToSvg(plantUml) {
    if (!plantUml) {
      this.isDiagramLoading = false;
      this.cdr.markForCheck();
      return;
    }
    this.http.post('/api/plantumlextensions/RenderSvg', {
      plantUmlCode: plantUml
    }).subscribe({
      next: response => {
        if (response?.svg) {
          this.diagramSvg = this.sanitizer.bypassSecurityTrustHtml(response.svg);
        }
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      },
      error: err => {
        console.error('[PromptLabCard] Error rendering PlantUML:', err);
        // Fallback: show raw PlantUML code
        this.diagramSvg = '';
        this.isDiagramLoading = false;
        this.cdr.markForCheck();
      }
    });
  }
  extractPlantUml(text) {
    const match = text.match(/@startuml[\s\S]*?@enduml/);
    return match ? match[0] : text.trim();
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
      return new (t || PromptLabCardComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_7__.ChangeDetectorRef), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_7__.ElementRef), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_platform_browser__WEBPACK_IMPORTED_MODULE_10__.DomSanitizer), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_11__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_12__.MatLegacyDialog), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_promptlab_service__WEBPACK_IMPORTED_MODULE_3__.PromptLabService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_promptlab_distillation_service__WEBPACK_IMPORTED_MODULE_4__.PromptLabDistillationService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_5__.AiChatService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_6__.ProjectsService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({
      type: PromptLabCardComponent,
      selectors: [["app-promptlab-card"]],
      viewQuery: function PromptLabCardComponent_Query(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵviewQuery"](_c0, 5);
        }
        if (rf & 2) {
          let _t;
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵqueryRefresh"](_t = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵloadQuery"]()) && (ctx.messagesContainer = _t.first);
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
      decls: 57,
      vars: 42,
      consts: [[1, "card"], [1, "card-header"], [1, "card-title-row"], [1, "pin"], ["contenteditable", "true", 1, "title", 3, "blur"], [1, "spacer"], ["title", "Elimina card", 1, "delete-btn", 3, "click"], ["class", "card-params", 4, "ngIf"], [1, "card-actions"], [1, "play-btn", 3, "disabled", "click"], [1, "lastrun-btn", 3, "disabled", "click"], [1, "action-separator"], [1, "diagram-btn", 3, "disabled", "click"], ["class", "diagram-spinner", 4, "ngIf"], ["class", "ultimo-run", 4, "ngIf"], ["class", "diagram-panel", 4, "ngIf"], [1, "card-body"], [1, "chat-column", 3, "ngStyle"], [1, "column-label"], [1, "label-text"], ["title", "Azzera la chat", 1, "reset-chat-btn", 3, "disabled", "click"], [1, "chat-messages"], ["messagesContainer", ""], ["class", "msg", 4, "ngFor", "ngForOf"], ["class", "msg", 4, "ngIf"], ["class", "chat-empty", 4, "ngIf"], ["class", "distillation-indicator", 4, "ngIf"], [1, "chat-input"], ["placeholder", "Scrivi un messaggio...", "rows", "4", 3, "ngModel", "disabled", "ngModelChange", "keydown"], [3, "disabled", "click"], [1, "column-splitter", 3, "mousedown"], [1, "grip"], [1, "prompt-column"], [1, "collapse-chat-btn", 3, "click"], [4, "ngIf"], [1, "prompt-content"], ["class", "prompt-rendered", 3, "innerHTML", "click", 4, "ngIf"], ["class", "prompt-editor", 3, "ngModel", "ngModelChange", "blur", "keydown.escape", 4, "ngIf"], [1, "prompt-note"], [1, "card-params"], [4, "ngFor", "ngForOf"], ["class", "inline-editor", 4, "ngIf"], ["class", "param-btn", 3, "ngClass", "click", 4, "ngIf"], [1, "inline-editor"], [3, "value", "input", "keydown"], ["paramInput", ""], [1, "confirm", 3, "click"], [1, "param-btn", 3, "ngClass", "click"], [1, "diagram-spinner"], [1, "ultimo-run"], [1, "run-section"], [1, "run-section-title"], [1, "run-meta"], [1, "run-meta-item"], ["class", "run-section", 4, "ngIf"], [1, "run-prompt"], [1, "run-output"], [1, "run-params"], ["class", "run-param", 4, "ngFor", "ngForOf"], [1, "run-param"], [1, "run-param-name"], [1, "run-param-eq"], [1, "run-param-val"], [1, "diagram-panel"], [1, "diagram-header"], [1, "diagram-title"], ["title", "Copia codice PlantUML", 1, "diagram-copy-btn", 3, "disabled", "click"], [1, "diagram-body"], ["class", "diagram-loading", 4, "ngIf"], ["class", "diagram-svg", 3, "innerHTML", 4, "ngIf"], ["class", "diagram-code", 4, "ngIf"], ["class", "diagram-empty", 4, "ngIf"], [1, "diagram-loading"], [1, "diagram-svg", 3, "innerHTML"], [1, "diagram-code"], [1, "diagram-empty"], [1, "msg"], [1, "author", 3, "ngClass"], [1, "time"], [1, "text"], [1, "author", "assistant"], [1, "text", "streaming"], [1, "streaming-cursor"], [1, "chat-empty"], [1, "distillation-indicator"], [1, "distill-spinner"], [1, "prompt-rendered", 3, "innerHTML", "click"], [1, "prompt-editor", 3, "ngModel", "ngModelChange", "blur", "keydown.escape"], ["promptEditor", ""]],
      template: function PromptLabCardComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 0)(1, "div", 1)(2, "div", 2)(3, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "\uF4CC");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "span", 4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("blur", function PromptLabCardComponent_Template_span_blur_5_listener($event) {
            return ctx.onTitleEdit($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](7, "span", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](8, "button", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_8_listener() {
            return ctx.deleteCard();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](9, "\uF5D1");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](10, PromptLabCardComponent_div_10_Template, 2, 1, "div", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](11, "div", 8)(12, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_12_listener() {
            return ctx.onPlay();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](14, "button", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_14_listener() {
            return ctx.toggleLastRun();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](15);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](16, "span", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](17, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_17_listener() {
            return ctx.toggleDiagram("sequence");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](18, PromptLabCardComponent_span_18_Template, 1, 0, "span", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](20, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_20_listener() {
            return ctx.toggleDiagram("workflow");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](21, PromptLabCardComponent_span_21_Template, 1, 0, "span", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](22);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](23, PromptLabCardComponent_div_23_Template, 32, 7, "div", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](24, PromptLabCardComponent_div_24_Template, 11, 6, "div", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](25, "div", 16)(26, "div", 17)(27, "div", 18)(28, "span", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](29, "Chat con LLM");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](30, "button", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_30_listener() {
            return ctx.resetChat();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](31, "\u21BB Reset");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](32, "div", 21, 22);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](34, PromptLabCardComponent_div_34_Template, 7, 4, "div", 23);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](35, PromptLabCardComponent_div_35_Template, 6, 1, "div", 24);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](36, PromptLabCardComponent_div_36_Template, 2, 0, "div", 25);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](37, PromptLabCardComponent_div_37_Template, 3, 0, "div", 26);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](38, "div", 27)(39, "textarea", 28);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabCardComponent_Template_textarea_ngModelChange_39_listener($event) {
            return ctx.chatInputText = $event;
          })("keydown", function PromptLabCardComponent_Template_textarea_keydown_39_listener($event) {
            return ctx.onChatInputKeydown($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](40, "button", 29);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_40_listener() {
            return ctx.sendMessage();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](41);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](42, "div", 30);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("mousedown", function PromptLabCardComponent_Template_div_mousedown_42_listener($event) {
            return ctx.onSplitterMouseDown($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](43, "span", 31);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](44, "\u22EE");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](45, "div", 32)(46, "div", 18)(47, "button", 33);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabCardComponent_Template_button_click_47_listener() {
            return ctx.toggleChat();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](48, PromptLabCardComponent_span_48_Template, 2, 0, "span", 34);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](49, PromptLabCardComponent_span_49_Template, 2, 0, "span", 34);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](50, "span", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](51, "Prompt Distillato");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](52, "div", 35);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](53, PromptLabCardComponent_div_53_Template, 1, 1, "div", 36);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](54, PromptLabCardComponent_textarea_54_Template, 2, 1, "textarea", 37);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](55, "div", 38);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](56);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()()();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("single-card", ctx.isSingleCard);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵattribute"]("data-placeholder", "Titolo card...");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.card.generatedTitle);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.card.parameters.length > 0);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isStreaming || ctx.isExecuting);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isExecuting ? "..." : "\u25B6 Play", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.showLastRun)("has-run", !!ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", !ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" \uF441 Ultimo run ", ctx.showLastRun ? "\u25B2" : "\u25BC", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.activeDiagram === "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isDiagramLoading && ctx.activeDiagram !== "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "sequence");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isDiagramLoading && ctx.activeDiagram === "sequence" ? "Generazione..." : "&#9776; Sequence", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.activeDiagram === "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isDiagramLoading && ctx.activeDiagram !== "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isDiagramLoading && ctx.activeDiagram === "workflow");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isDiagramLoading && ctx.activeDiagram === "workflow" ? "Generazione..." : "&#9881; Workflow", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.showLastRun && ctx.card.lastRun);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.activeDiagram);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("chat-collapsed", ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngStyle", ctx.getChatColumnStyle());
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.card.conversation);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.card.conversation.length === 0 && !ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isDistilling);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.chatInputText)("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isStreaming);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isStreaming ? "..." : "Invia", " ");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx.chatCollapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", !ctx.isEditingPrompt);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isEditingPrompt);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" \u2139 ", ctx.isEditingPrompt ? "Modifica in corso \u2014 clicca fuori per salvare." : "Clicca sul testo per modificare il prompt.", " ");
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_13__.NgClass, _angular_common__WEBPACK_IMPORTED_MODULE_13__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_13__.NgIf, _angular_common__WEBPACK_IMPORTED_MODULE_13__.NgStyle, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_14__.NgModel],
      styles: ["@charset \"UTF-8\";\n[_nghost-%COMP%] {\n  display: block;\n}\n\n\n.card[_ngcontent-%COMP%] {\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 8px;\n  display: flex;\n  flex-direction: column;\n  overflow: hidden;\n  height: calc(100vh - 350px);\n  min-height: 300px;\n}\n.card.single-card[_ngcontent-%COMP%] {\n  flex: 1;\n  min-height: 0;\n}\n\n\n.card-header[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n\n.card-title-row[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 8px;\n  margin-bottom: 8px;\n}\n.card-title-row[_ngcontent-%COMP%]   .pin[_ngcontent-%COMP%] {\n  color: #ff9800;\n  font-size: 16px;\n  flex-shrink: 0;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%] {\n  font-size: 14px;\n  color: #fff;\n  font-weight: 600;\n  outline: none;\n  border-radius: 3px;\n  padding: 2px 4px;\n  transition: background 0.15s;\n  min-width: 60px;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]:focus {\n  background: #2a2a2a;\n  box-shadow: 0 0 0 1px #555 inset;\n}\n.card-title-row[_ngcontent-%COMP%]   .title[_ngcontent-%COMP%]:empty::before {\n  content: attr(data-placeholder);\n  color: #555;\n}\n.card-title-row[_ngcontent-%COMP%]   .spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.card-title-row[_ngcontent-%COMP%]   .delete-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #666;\n  font-size: 14px;\n  cursor: pointer;\n  padding: 4px 6px;\n  border-radius: 3px;\n  transition: all 0.15s;\n}\n.card-title-row[_ngcontent-%COMP%]   .delete-btn[_ngcontent-%COMP%]:hover {\n  color: #ef5350;\n  background: rgba(239, 83, 80, 0.1);\n}\n\n\n.card-params[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  flex-wrap: wrap;\n  margin-bottom: 10px;\n}\n\n.param-btn[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  padding: 4px 10px;\n  border-radius: 14px;\n  font-size: 11px;\n  cursor: pointer;\n  border: 1px solid;\n  transition: all 0.2s;\n  white-space: nowrap;\n  \n  \n  \n}\n.param-btn[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.2);\n}\n.param-btn.file.empty[_ngcontent-%COMP%] {\n  background: #1a2733;\n  border-color: #2979ff;\n  border-style: dashed;\n  color: #5c8cc7;\n}\n.param-btn.file.filled[_ngcontent-%COMP%] {\n  background: #1e3a5f;\n  border-color: #2979ff;\n  color: #82b1ff;\n}\n.param-btn.directory.empty[_ngcontent-%COMP%] {\n  background: #2a2320;\n  border-color: #8d6e63;\n  border-style: dashed;\n  color: #9c7d6d;\n}\n.param-btn.directory.filled[_ngcontent-%COMP%] {\n  background: #3e2723;\n  border-color: #8d6e63;\n  color: #d7ccc8;\n}\n.param-btn.text.empty[_ngcontent-%COMP%] {\n  background: #33302a;\n  border-color: #ff9800;\n  border-style: dashed;\n  color: #a68c5c;\n}\n.param-btn.text.filled[_ngcontent-%COMP%] {\n  background: #3d3520;\n  border-color: #ff9800;\n  color: #ffe0b2;\n}\n\n\n.inline-editor[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 4px;\n  background: #3d3520;\n  border: 1px solid #ff9800;\n  border-radius: 16px;\n  padding: 4px 8px;\n}\n.inline-editor[_ngcontent-%COMP%]   input[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #ffe0b2;\n  font-size: 12px;\n  width: 100px;\n  outline: none;\n}\n.inline-editor[_ngcontent-%COMP%]   .confirm[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  border-radius: 10px;\n  width: 20px;\n  height: 20px;\n  font-size: 12px;\n  cursor: pointer;\n  line-height: 20px;\n  text-align: center;\n  flex-shrink: 0;\n}\n.inline-editor[_ngcontent-%COMP%]   .confirm[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.1);\n}\n\n\n.card-actions[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 8px;\n  align-items: center;\n}\n\n.play-btn[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  font-weight: 600;\n  cursor: pointer;\n  transition: filter 0.15s;\n}\n.play-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  filter: brightness(1.1);\n}\n.play-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n\n.lastrun-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  padding: 5px 12px;\n  border-radius: 4px;\n  font-size: 11px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.lastrun-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #888;\n  color: #ccc;\n}\n.lastrun-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n.lastrun-btn.has-run[_ngcontent-%COMP%] {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n.lastrun-btn.active[_ngcontent-%COMP%] {\n  background: #33280a;\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n.action-separator[_ngcontent-%COMP%] {\n  width: 1px;\n  height: 20px;\n  background: #3c3c3c;\n  margin: 0 4px;\n  flex-shrink: 0;\n}\n\n.diagram-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #444;\n  color: #aaa;\n  padding: 4px 10px;\n  border-radius: 4px;\n  font-size: 10px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.diagram-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #888;\n  color: #ccc;\n}\n.diagram-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n.diagram-btn.active[_ngcontent-%COMP%] {\n  border-color: #82b1ff;\n  color: #82b1ff;\n  background: #1a2733;\n}\n\n.diagram-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 10px;\n  height: 10px;\n  border: 2px solid #555;\n  border-top-color: #82b1ff;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_diagram-spin 0.8s linear infinite;\n  margin-right: 4px;\n  vertical-align: middle;\n}\n\n@keyframes _ngcontent-%COMP%_diagram-spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n\n.card-body[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  min-height: 0;\n  overflow: hidden;\n  \n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .chat-column[_ngcontent-%COMP%] {\n  display: none;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .column-splitter[_ngcontent-%COMP%] {\n  display: none;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .prompt-column[_ngcontent-%COMP%] {\n  flex: 1;\n}\n.card-body.chat-collapsed[_ngcontent-%COMP%]   .collapse-chat-btn[_ngcontent-%COMP%] {\n  background: #33280a;\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n\n.chat-column[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 200px;\n  min-height: 0;\n  overflow: hidden;\n}\n\n\n.column-label[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #888;\n  padding: 8px 12px;\n  background: #2a2a2a;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  gap: 6px;\n}\n.column-label[_ngcontent-%COMP%]   .label-text[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n.reset-chat-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 9px;\n  cursor: pointer;\n  padding: 2px 8px;\n  border-radius: 3px;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.reset-chat-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ef5350;\n  color: #ef5350;\n}\n\n.collapse-chat-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 9px;\n  cursor: pointer;\n  padding: 2px 8px;\n  border-radius: 3px;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.collapse-chat-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n\n.chat-messages[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 12px;\n}\n\n.msg[_ngcontent-%COMP%] {\n  margin-bottom: 12px;\n}\n.msg[_ngcontent-%COMP%]   .author[_ngcontent-%COMP%] {\n  font-size: 11px;\n  font-weight: 600;\n  margin-bottom: 2px;\n}\n.msg[_ngcontent-%COMP%]   .author.user[_ngcontent-%COMP%] {\n  color: #82b1ff;\n}\n.msg[_ngcontent-%COMP%]   .author.assistant[_ngcontent-%COMP%] {\n  color: #c792ea;\n}\n.msg[_ngcontent-%COMP%]   .author[_ngcontent-%COMP%]   .time[_ngcontent-%COMP%] {\n  color: #666;\n  font-weight: 400;\n  font-size: 10px;\n  margin-left: 6px;\n}\n.msg[_ngcontent-%COMP%]   .text[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.5;\n  color: #ccc;\n  white-space: pre-wrap;\n}\n.msg[_ngcontent-%COMP%]   .text.streaming[_ngcontent-%COMP%]   .streaming-cursor[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 6px;\n  height: 14px;\n  background: #c792ea;\n  margin-left: 2px;\n  vertical-align: text-bottom;\n  animation: _ngcontent-%COMP%_blink-cursor 0.8s step-end infinite;\n}\n\n@keyframes _ngcontent-%COMP%_blink-cursor {\n  0%, 100% {\n    opacity: 1;\n  }\n  50% {\n    opacity: 0;\n  }\n}\n.chat-empty[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 12px;\n  font-style: italic;\n  text-align: center;\n  padding: 24px 12px;\n}\n\n\n.chat-input[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: flex-end;\n  padding: 8px 12px;\n  border-top: 1px solid #3c3c3c;\n  gap: 8px;\n  flex-shrink: 0;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%] {\n  flex: 1;\n  background: #3c3c3c;\n  border: 1px solid #555;\n  border-radius: 4px;\n  padding: 6px 10px;\n  color: #ccc;\n  font-size: 12px;\n  font-family: \"Segoe UI\", sans-serif;\n  outline: none;\n  resize: none;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%]:focus {\n  border-color: #888;\n}\n.chat-input[_ngcontent-%COMP%]   textarea[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%] {\n  background: #ff9800;\n  border: none;\n  color: #000;\n  padding: 6px 12px;\n  border-radius: 4px;\n  font-size: 12px;\n  cursor: pointer;\n  font-weight: 600;\n  transition: filter 0.15s;\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:hover:not(:disabled) {\n  filter: brightness(1.1);\n}\n.chat-input[_ngcontent-%COMP%]   button[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n\n\n.column-splitter[_ngcontent-%COMP%] {\n  width: 5px;\n  background: #2a2a2a;\n  cursor: col-resize;\n  flex-shrink: 0;\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  transition: background 0.15s;\n  -webkit-user-select: none;\n          user-select: none;\n}\n.column-splitter[_ngcontent-%COMP%]:hover {\n  background: #ff9800;\n}\n.column-splitter[_ngcontent-%COMP%]:hover   .grip[_ngcontent-%COMP%] {\n  color: #000;\n}\n.column-splitter[_ngcontent-%COMP%]   .grip[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 10px;\n  writing-mode: vertical-lr;\n  letter-spacing: 2px;\n  pointer-events: none;\n}\n\n\n.prompt-column[_ngcontent-%COMP%] {\n  flex: 1;\n  display: flex;\n  flex-direction: column;\n  min-width: 200px;\n  min-height: 0;\n  overflow: hidden;\n}\n\n.prompt-content[_ngcontent-%COMP%] {\n  flex: 1;\n  padding: 16px;\n  overflow-y: auto;\n  min-height: 0;\n}\n\n.prompt-rendered[_ngcontent-%COMP%] {\n  font-size: 13px;\n  line-height: 1.7;\n  color: #e0e0e0;\n  cursor: pointer;\n  padding: 4px;\n  border-radius: 4px;\n  transition: background 0.15s;\n}\n.prompt-rendered[_ngcontent-%COMP%]:hover {\n  background: #2a2a2a;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h2[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  color: #fff;\n  margin: 12px 0 6px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h2[_ngcontent-%COMP%] {\n  font-size: 16px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h3[_ngcontent-%COMP%] {\n  font-size: 14px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   h4[_ngcontent-%COMP%] {\n  font-size: 13px;\n}\n.prompt-rendered[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: #fff;\n}\n.prompt-rendered[_ngcontent-%COMP%]   em[_ngcontent-%COMP%] {\n  color: #ccc;\n  font-style: italic;\n}\n.prompt-rendered[_ngcontent-%COMP%]   code[_ngcontent-%COMP%] {\n  background: #1e1e1e;\n  border: 1px solid #3c3c3c;\n  border-radius: 3px;\n  padding: 1px 4px;\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n  color: #ce9178;\n}\n.prompt-rendered[_ngcontent-%COMP%]   ul[_ngcontent-%COMP%], .prompt-rendered[_ngcontent-%COMP%]   ol[_ngcontent-%COMP%] {\n  padding-left: 20px;\n  margin: 0;\n}\n.prompt-rendered[_ngcontent-%COMP%]   li[_ngcontent-%COMP%] {\n  margin: 0;\n  padding: 0;\n  line-height: 1.4;\n}\n.prompt-rendered[_ngcontent-%COMP%]   p[_ngcontent-%COMP%] {\n  margin: 0;\n}\n.prompt-rendered[_ngcontent-%COMP%]   .param-highlight[_ngcontent-%COMP%] {\n  background: #33302a;\n  border: 1px solid #ff9800;\n  border-radius: 3px;\n  padding: 1px 4px;\n  color: #ffcc80;\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n}\n\n.prompt-editor[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 200px;\n  height: 100%;\n  background: #1e1e1e;\n  border: 1px solid #ff9800;\n  border-radius: 4px;\n  padding: 8px;\n  color: #e0e0e0;\n  font-size: 13px;\n  font-family: \"Courier New\", monospace;\n  line-height: 1.7;\n  resize: none;\n  outline: none;\n  box-sizing: border-box;\n}\n\n\n.distillation-indicator[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 6px;\n  padding: 6px 12px;\n  font-size: 10px;\n  color: #ff9800;\n  border-top: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.distill-spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 10px;\n  height: 10px;\n  border: 2px solid #555;\n  border-top-color: #ff9800;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_distill-spin 0.8s linear infinite;\n}\n\n@keyframes _ngcontent-%COMP%_distill-spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n.prompt-text[_ngcontent-%COMP%] {\n  font-size: 13px;\n  line-height: 1.8;\n  color: #e0e0e0;\n  white-space: pre-wrap;\n  outline: none;\n  border-radius: 4px;\n  padding: 4px;\n  transition: background 0.15s;\n  min-height: 60px;\n}\n.prompt-text[_ngcontent-%COMP%]:focus {\n  background: #2a2a2a;\n  box-shadow: 0 0 0 1px #ff9800 inset;\n}\n\n.prompt-note[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n  font-size: 10px;\n  color: #666;\n  flex-shrink: 0;\n}\n\n\n.ultimo-run[_ngcontent-%COMP%] {\n  border-top: 1px solid #3c3c3c;\n  background: #1e1e1e;\n  flex-shrink: 0;\n  max-height: 350px;\n  overflow-y: auto;\n}\n\n.run-section[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n  border-bottom: 1px solid #2a2a2a;\n}\n.run-section[_ngcontent-%COMP%]:last-child {\n  border-bottom: none;\n}\n\n.run-section-title[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #888;\n  margin-bottom: 8px;\n}\n\n.run-meta[_ngcontent-%COMP%] {\n  display: flex;\n  gap: 16px;\n  flex-wrap: wrap;\n}\n\n.run-meta-item[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: #aaa;\n}\n.run-meta-item[_ngcontent-%COMP%]   strong[_ngcontent-%COMP%] {\n  color: #ccc;\n}\n\n.run-params[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 4px;\n}\n\n.run-param[_ngcontent-%COMP%] {\n  font-size: 12px;\n  display: flex;\n  align-items: center;\n  gap: 6px;\n}\n\n.run-param-name[_ngcontent-%COMP%] {\n  color: #888;\n}\n\n.run-param-eq[_ngcontent-%COMP%] {\n  color: #555;\n}\n\n.run-param-val[_ngcontent-%COMP%] {\n  color: #e0e0e0;\n  font-weight: 500;\n}\n\n.run-prompt[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.7;\n  color: #ccc;\n  background: #252526;\n  border-radius: 4px;\n  padding: 12px;\n  white-space: pre-wrap;\n  border: 1px solid #3c3c3c;\n}\n\n.run-output[_ngcontent-%COMP%] {\n  font-size: 12px;\n  line-height: 1.7;\n  color: #ccc;\n  background: #252526;\n  border-radius: 4px;\n  padding: 12px;\n  white-space: pre-wrap;\n  border: 1px solid #3c3c3c;\n  max-height: 200px;\n  overflow-y: auto;\n}\n\n\n.diagram-panel[_ngcontent-%COMP%] {\n  border-top: 1px solid #3c3c3c;\n  background: #1a1d21;\n  flex-shrink: 0;\n}\n\n.diagram-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 8px 16px;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.diagram-title[_ngcontent-%COMP%] {\n  font-size: 10px;\n  text-transform: uppercase;\n  letter-spacing: 1px;\n  color: #82b1ff;\n  flex: 1;\n}\n\n.diagram-copy-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 10px;\n  padding: 2px 8px;\n  border-radius: 3px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.diagram-copy-btn[_ngcontent-%COMP%]:hover:not(:disabled) {\n  border-color: #82b1ff;\n  color: #82b1ff;\n}\n.diagram-copy-btn[_ngcontent-%COMP%]:disabled {\n  opacity: 0.4;\n  cursor: not-allowed;\n}\n\n.diagram-body[_ngcontent-%COMP%] {\n  padding: 12px 16px;\n}\n\n.diagram-svg[_ngcontent-%COMP%] {\n  background: #fff;\n  border-radius: 4px;\n  padding: 12px;\n  overflow: auto;\n  max-height: 400px;\n  text-align: center;\n}\n.diagram-svg[_ngcontent-%COMP%]     svg {\n  max-width: 100%;\n  height: auto;\n}\n\n.diagram-code[_ngcontent-%COMP%] {\n  font-family: \"Courier New\", monospace;\n  font-size: 12px;\n  color: #ccc;\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 4px;\n  padding: 12px;\n  overflow-x: auto;\n  white-space: pre;\n  max-height: 300px;\n  overflow-y: auto;\n  margin: 0;\n}\n\n.diagram-loading[_ngcontent-%COMP%] {\n  color: #888;\n  font-size: 12px;\n  font-style: italic;\n}\n\n.diagram-empty[_ngcontent-%COMP%] {\n  color: #555;\n  font-size: 12px;\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiLWNhcmQvcHJvbXB0bGFiLWNhcmQuY29tcG9uZW50LnNjc3MiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IkFBQUEsZ0JBQWdCO0FBQWhCO0VBQ0UsY0FBQTtBQUVGOztBQUNBLDJCQUFBO0FBRUE7RUFDRSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0Esc0JBQUE7RUFDQSxnQkFBQTtFQUdBLDJCQUFBO0VBQ0EsaUJBQUE7QUFERjtBQUdFO0VBQ0UsT0FBQTtFQUNBLGFBQUE7QUFESjs7QUFLQSxtQkFBQTtBQUVBO0VBQ0Usa0JBQUE7RUFDQSxnQ0FBQTtFQUNBLGNBQUE7QUFIRjs7QUFNQSxjQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0Esa0JBQUE7QUFIRjtBQUtFO0VBQ0UsY0FBQTtFQUNBLGVBQUE7RUFDQSxjQUFBO0FBSEo7QUFNRTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7RUFDQSxhQUFBO0VBQ0Esa0JBQUE7RUFDQSxnQkFBQTtFQUNBLDRCQUFBO0VBQ0EsZUFBQTtBQUpKO0FBTUk7RUFDRSxtQkFBQTtFQUNBLGdDQUFBO0FBSk47QUFPSTtFQUNFLCtCQUFBO0VBQ0EsV0FBQTtBQUxOO0FBU0U7RUFDRSxPQUFBO0FBUEo7QUFVRTtFQUNFLGdCQUFBO0VBQ0EsWUFBQTtFQUNBLFdBQUE7RUFDQSxlQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxxQkFBQTtBQVJKO0FBVUk7RUFDRSxjQUFBO0VBQ0Esa0NBQUE7QUFSTjs7QUFhQSxtQkFBQTtBQUNBO0VBQ0UsYUFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsbUJBQUE7QUFWRjs7QUFhQTtFQUNFLG9CQUFBO0VBQ0EsbUJBQUE7RUFDQSxRQUFBO0VBQ0EsaUJBQUE7RUFDQSxtQkFBQTtFQUNBLGVBQUE7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QUE7RUFDQSxpQkFBQTtFQUNBLGdCQUFBO0FBekJGOztBQTRCQSxxQ0FBQTtBQUVBO0VBQ0UsNkJBQUE7RUFDQSxtQkFBQTtFQUNBLGNBQUE7QUExQkY7O0FBNkJBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsaUJBQUE7RUFDQSxnQ0FBQTtBQTFCRjs7QUE2QkE7RUFDRSxlQUFBO0VBQ0EseUJBQUE7RUFDQSxtQkFBQTtFQUNBLGNBQUE7RUFDQSxPQUFBO0FBMUJGOztBQTZCQTtFQUNFLGdCQUFBO0VBQ0Esc0JBQUE7RUFDQSxXQUFBO0VBQ0EsZUFBQTtFQUNBLGdCQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EscUJBQUE7QUExQkY7QUE0QkU7RUFDRSxxQkFBQTtFQUNBLGNBQUE7QUExQko7QUE2QkU7RUFDRSxZQUFBO0VBQ0EsbUJBQUE7QUEzQko7O0FBK0JBO0VBQ0Usa0JBQUE7QUE1QkY7O0FBK0JBO0VBQ0UsZ0JBQUE7RUFDQSxrQkFBQTtFQUNBLGFBQUE7RUFDQSxjQUFBO0VBQ0EsaUJBQUE7RUFDQSxrQkFBQTtBQTVCRjtBQThCRTtFQUNFLGVBQUE7RUFDQSxZQUFBO0FBNUJKOztBQWdDQTtFQUNFLHFDQUFBO0VBQ0EsZUFBQTtFQUNBLFdBQUE7RUFDQSxtQkFBQTtFQUNBLHlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxhQUFBO0VBQ0EsZ0JBQUE7RUFDQSxnQkFBQTtFQUNBLGlCQUFBO0VBQ0EsZ0JBQUE7RUFDQSxTQUFBO0FBN0JGOztBQWdDQTtFQUNFLFdBQUE7RUFDQSxlQUFBO0VBQ0Esa0JBQUE7QUE3QkY7O0FBZ0NBO0VBQ0UsV0FBQTtFQUNBLGVBQUE7QUE3QkYiLCJzb3VyY2VzQ29udGVudCI6WyI6aG9zdCB7XG4gIGRpc3BsYXk6IGJsb2NrO1xufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgQ2FyZCBjb250YWluZXIgw6LClcKQw6LClcKQw6LClcKQICovXG5cbi5jYXJkIHtcbiAgYmFja2dyb3VuZDogIzI1MjUyNjtcbiAgYm9yZGVyOiAxcHggc29saWQgIzNjM2MzYztcbiAgYm9yZGVyLXJhZGl1czogOHB4O1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBvdmVyZmxvdzogaGlkZGVuO1xuICAvLyBGaXhlZCBoZWlnaHQgZm9yIHRoZSBjYXJkIGJvZHkgYXJlYSDDosKAwpQgY29sdW1ucyBzY3JvbGwgaW50ZXJuYWxseVxuICAvLyAzNTBweCBhY2NvdW50cyBmb3I6IHRpdGxlIGJhcigzMCkgKyB0b29sYmFyKDQ4KSArIHNlc3Npb24gaGVhZGVyKDQ0KSArIGRvYyBwYW5lbCh+MTEwKSArIGNhcmQgaGVhZGVyKH4xMTApICsgcGFkZGluZ1xuICBoZWlnaHQ6IGNhbGMoMTAwdmggLSAzNTBweCk7XG4gIG1pbi1oZWlnaHQ6IDMwMHB4O1xuXG4gICYuc2luZ2xlLWNhcmQge1xuICAgIGZsZXg6IDE7XG4gICAgbWluLWhlaWdodDogMDtcbiAgfVxufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgSEVBREVSIMOiwpXCkMOiwpXCkMOiwpXCkCAqL1xuXG4uY2FyZC1oZWFkZXIge1xuICBwYWRkaW5nOiAxMnB4IDE2cHg7XG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjM2MzYzNjO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLyogVGl0bGUgcm93ICovXG4uY2FyZC10aXRsZS1yb3cge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDhweDtcbiAgbWFyZ2luLWJvdHRvbTogOHB4O1xuXG4gIC5waW4ge1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgIGZvbnQtc2l6ZTogMTZweDtcbiAgICBmbGV4LXNocmluazogMDtcbiAgfVxuXG4gIC50aXRsZSB7XG4gICAgZm9udC1zaXplOiAxNHB4O1xuICAgIGNvbG9yOiAjZmZmO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgb3V0bGluZTogbm9uZTtcbiAgICBib3JkZXItcmFkaXVzOiAzcHg7XG4gICAgcGFkZGluZzogMnB4IDRweDtcbiAgICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuMTVzO1xuICAgIG1pbi13aWR0aDogNjBweDtcblxuICAgICY6Zm9jdXMge1xuICAgICAgYmFja2dyb3VuZDogIzJhMmEyYTtcbiAgICAgIGJveC1zaGFkb3c6IDAgMCAwIDFweCAjNTU1IGluc2V0O1xuICAgIH1cblxuICAgICY6ZW1wdHk6OmJlZm9yZSB7XG4gICAgICBjb250ZW50OiBhdHRyKGRhdGEtcGxhY2Vob2xkZXIpO1xuICAgICAgY29sb3I6ICM1NTU7XG4gICAgfVxuICB9XG5cbiAgLnNwYWNlciB7XG4gICAgZmxleDogMTtcbiAgfVxuXG4gIC5kZWxldGUtYnRuIHtcbiAgICBiYWNrZ3JvdW5kOiBub25lO1xuICAgIGJvcmRlcjogbm9uZTtcbiAgICBjb2xvcjogIzY2NjtcbiAgICBmb250LXNpemU6IDE0cHg7XG4gICAgY3Vyc29yOiBwb2ludGVyO1xuICAgIHBhZGRpbmc6IDRweCA2cHg7XG4gICAgYm9yZGVyLXJhZGl1czogM3B4O1xuICAgIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAgICY6aG92ZXIge1xuICAgICAgY29sb3I6ICNlZjUzNTA7XG4gICAgICBiYWNrZ3JvdW5kOiByZ2JhKDIzOSwgODMsIDgwLCAwLjEpO1xuICAgIH1cbiAgfVxufVxuXG4vKiBQYXJhbWV0ZXJzIHJvdyAqL1xuLmNhcmQtcGFyYW1zIHtcbiAgZGlzcGxheTogZmxleDtcbiAgZ2FwOiA4cHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcbiAgbWFyZ2luLWJvdHRvbTogMTBweDtcbn1cblxuLnBhcmFtLWJ0biB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDRweDtcbiAgcGFkZGluZzogNHB4IDEwcHg7XG4gIGJvcmRlci1yYWRpdXM6IDE0cHg7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBib3JkZXI6IDFweCBzb2xpZDtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMnM7XG4gIHdoaXRlLXNwYWNlOiBub3dyYXA7XG5cbiAgJjpob3ZlciB7XG4gICAgZmlsdGVyOiBicmlnaHRuZXNzKDEuMik7XG4gIH1cblxuICAvKiBGaWxlIHR5cGUgKi9cbiAgJi5maWxlIHtcbiAgICAmLmVtcHR5IHtcbiAgICAgIGJhY2tncm91bmQ6ICMxYTI3MzM7XG4gICAgICBib3JkZXItY29sb3I6ICMyOTc5ZmY7XG4gICAgICBib3JkZXItc3R5bGU6IGRhc2hlZDtcbiAgICAgIGNvbG9yOiAjNWM4Y2M3O1xuICAgIH1cbiAgICAmLmZpbGxlZCB7XG4gICAgICBiYWNrZ3JvdW5kOiAjMWUzYTVmO1xuICAgICAgYm9yZGVyLWNvbG9yOiAjMjk3OWZmO1xuICAgICAgY29sb3I6ICM4MmIxZmY7XG4gICAgfVxuICB9XG5cbiAgLyogRGlyZWN0b3J5IHR5cGUgKi9cbiAgJi5kaXJlY3Rvcnkge1xuICAgICYuZW1wdHkge1xuICAgICAgYmFja2dyb3VuZDogIzJhMjMyMDtcbiAgICAgIGJvcmRlci1jb2xvcjogIzhkNmU2MztcbiAgICAgIGJvcmRlci1zdHlsZTogZGFzaGVkO1xuICAgICAgY29sb3I6ICM5YzdkNmQ7XG4gICAgfVxuICAgICYuZmlsbGVkIHtcbiAgICAgIGJhY2tncm91bmQ6ICMzZTI3MjM7XG4gICAgICBib3JkZXItY29sb3I6ICM4ZDZlNjM7XG4gICAgICBjb2xvcjogI2Q3Y2NjODtcbiAgICB9XG4gIH1cblxuICAvKiBUZXh0IHR5cGUgKi9cbiAgJi50ZXh0IHtcbiAgICAmLmVtcHR5IHtcbiAgICAgIGJhY2tncm91bmQ6ICMzMzMwMmE7XG4gICAgICBib3JkZXItY29sb3I6ICNmZjk4MDA7XG4gICAgICBib3JkZXItc3R5bGU6IGRhc2hlZDtcbiAgICAgIGNvbG9yOiAjYTY4YzVjO1xuICAgIH1cbiAgICAmLmZpbGxlZCB7XG4gICAgICBiYWNrZ3JvdW5kOiAjM2QzNTIwO1xuICAgICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgICAgY29sb3I6ICNmZmUwYjI7XG4gICAgfVxuICB9XG59XG5cbi8qIElubGluZSBlZGl0b3IgZm9yIHRleHQgcGFyYW1zICovXG4uaW5saW5lLWVkaXRvciB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDRweDtcbiAgYmFja2dyb3VuZDogIzNkMzUyMDtcbiAgYm9yZGVyOiAxcHggc29saWQgI2ZmOTgwMDtcbiAgYm9yZGVyLXJhZGl1czogMTZweDtcbiAgcGFkZGluZzogNHB4IDhweDtcblxuICBpbnB1dCB7XG4gICAgYmFja2dyb3VuZDogbm9uZTtcbiAgICBib3JkZXI6IG5vbmU7XG4gICAgY29sb3I6ICNmZmUwYjI7XG4gICAgZm9udC1zaXplOiAxMnB4O1xuICAgIHdpZHRoOiAxMDBweDtcbiAgICBvdXRsaW5lOiBub25lO1xuICB9XG5cbiAgLmNvbmZpcm0ge1xuICAgIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gICAgY29sb3I6ICMwMDA7XG4gICAgYm9yZGVyOiBub25lO1xuICAgIGJvcmRlci1yYWRpdXM6IDEwcHg7XG4gICAgd2lkdGg6IDIwcHg7XG4gICAgaGVpZ2h0OiAyMHB4O1xuICAgIGZvbnQtc2l6ZTogMTJweDtcbiAgICBjdXJzb3I6IHBvaW50ZXI7XG4gICAgbGluZS1oZWlnaHQ6IDIwcHg7XG4gICAgdGV4dC1hbGlnbjogY2VudGVyO1xuICAgIGZsZXgtc2hyaW5rOiAwO1xuXG4gICAgJjpob3ZlciB7XG4gICAgICBmaWx0ZXI6IGJyaWdodG5lc3MoMS4xKTtcbiAgICB9XG4gIH1cbn1cblxuLyogQWN0aW9ucyByb3cgKi9cbi5jYXJkLWFjdGlvbnMge1xuICBkaXNwbGF5OiBmbGV4O1xuICBnYXA6IDhweDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbn1cblxuLnBsYXktYnRuIHtcbiAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgY29sb3I6ICMwMDA7XG4gIGJvcmRlcjogbm9uZTtcbiAgcGFkZGluZzogNnB4IDE2cHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBmb250LXdlaWdodDogNjAwO1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGZpbHRlciAwLjE1cztcblxuICAmOmhvdmVyOm5vdCg6ZGlzYWJsZWQpIHtcbiAgICBmaWx0ZXI6IGJyaWdodG5lc3MoMS4xKTtcbiAgfVxuXG4gICY6ZGlzYWJsZWQge1xuICAgIG9wYWNpdHk6IDAuNTtcbiAgICBjdXJzb3I6IG5vdC1hbGxvd2VkO1xuICB9XG59XG5cbi5sYXN0cnVuLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGNvbG9yOiAjYWFhO1xuICBwYWRkaW5nOiA1cHggMTJweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBmb250LXNpemU6IDExcHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICY6aG92ZXI6bm90KDpkaXNhYmxlZCkge1xuICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgICBjb2xvcjogI2NjYztcbiAgfVxuXG4gICY6ZGlzYWJsZWQge1xuICAgIG9wYWNpdHk6IDAuNDtcbiAgICBjdXJzb3I6IG5vdC1hbGxvd2VkO1xuICB9XG5cbiAgJi5oYXMtcnVuIHtcbiAgICBib3JkZXItY29sb3I6ICNmZjk4MDA7XG4gICAgY29sb3I6ICNmZjk4MDA7XG4gIH1cblxuICAmLmFjdGl2ZSB7XG4gICAgYmFja2dyb3VuZDogIzMzMjgwYTtcbiAgICBib3JkZXItY29sb3I6ICNmZjk4MDA7XG4gICAgY29sb3I6ICNmZjk4MDA7XG4gIH1cbn1cblxuLmFjdGlvbi1zZXBhcmF0b3Ige1xuICB3aWR0aDogMXB4O1xuICBoZWlnaHQ6IDIwcHg7XG4gIGJhY2tncm91bmQ6ICMzYzNjM2M7XG4gIG1hcmdpbjogMCA0cHg7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uZGlhZ3JhbS1idG4ge1xuICBiYWNrZ3JvdW5kOiBub25lO1xuICBib3JkZXI6IDFweCBzb2xpZCAjNDQ0O1xuICBjb2xvcjogI2FhYTtcbiAgcGFkZGluZzogNHB4IDEwcHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgZm9udC1zaXplOiAxMHB4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyOm5vdCg6ZGlzYWJsZWQpIHtcbiAgICBib3JkZXItY29sb3I6ICM4ODg7XG4gICAgY29sb3I6ICNjY2M7XG4gIH1cblxuICAmOmRpc2FibGVkIHtcbiAgICBvcGFjaXR5OiAwLjQ7XG4gICAgY3Vyc29yOiBub3QtYWxsb3dlZDtcbiAgfVxuXG4gICYuYWN0aXZlIHtcbiAgICBib3JkZXItY29sb3I6ICM4MmIxZmY7XG4gICAgY29sb3I6ICM4MmIxZmY7XG4gICAgYmFja2dyb3VuZDogIzFhMjczMztcbiAgfVxufVxuXG4uZGlhZ3JhbS1zcGlubmVyIHtcbiAgZGlzcGxheTogaW5saW5lLWJsb2NrO1xuICB3aWR0aDogMTBweDtcbiAgaGVpZ2h0OiAxMHB4O1xuICBib3JkZXI6IDJweCBzb2xpZCAjNTU1O1xuICBib3JkZXItdG9wLWNvbG9yOiAjODJiMWZmO1xuICBib3JkZXItcmFkaXVzOiA1MCU7XG4gIGFuaW1hdGlvbjogZGlhZ3JhbS1zcGluIDAuOHMgbGluZWFyIGluZmluaXRlO1xuICBtYXJnaW4tcmlnaHQ6IDRweDtcbiAgdmVydGljYWwtYWxpZ246IG1pZGRsZTtcbn1cblxuQGtleWZyYW1lcyBkaWFncmFtLXNwaW4ge1xuICB0byB7IHRyYW5zZm9ybTogcm90YXRlKDM2MGRlZyk7IH1cbn1cblxuLyogw6LClcKQw6LClcKQw6LClcKQIEJPRFkgw6LClcKQw6LClcKQw6LClcKQICovXG5cbi5jYXJkLWJvZHkge1xuICBmbGV4OiAxO1xuICBkaXNwbGF5OiBmbGV4O1xuICBtaW4taGVpZ2h0OiAwO1xuICBvdmVyZmxvdzogaGlkZGVuO1xuXG4gIC8qIENvbGxhcHNlZCBjaGF0IHN0YXRlICovXG4gICYuY2hhdC1jb2xsYXBzZWQge1xuICAgIC5jaGF0LWNvbHVtbiB7XG4gICAgICBkaXNwbGF5OiBub25lO1xuICAgIH1cbiAgICAuY29sdW1uLXNwbGl0dGVyIHtcbiAgICAgIGRpc3BsYXk6IG5vbmU7XG4gICAgfVxuICAgIC5wcm9tcHQtY29sdW1uIHtcbiAgICAgIGZsZXg6IDE7XG4gICAgfVxuICAgIC5jb2xsYXBzZS1jaGF0LWJ0biB7XG4gICAgICBiYWNrZ3JvdW5kOiAjMzMyODBhO1xuICAgICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgICAgY29sb3I6ICNmZjk4MDA7XG4gICAgfVxuICB9XG59XG5cbi8qIENoYXQgY29sdW1uICovXG4uY2hhdC1jb2x1bW4ge1xuICBmbGV4OiAxO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBtaW4td2lkdGg6IDIwMHB4O1xuICBtaW4taGVpZ2h0OiAwO1xuICBvdmVyZmxvdzogaGlkZGVuO1xufVxuXG4vKiBDb2x1bW4gbGFiZWwgYmFyICovXG4uY29sdW1uLWxhYmVsIHtcbiAgZm9udC1zaXplOiAxMHB4O1xuICB0ZXh0LXRyYW5zZm9ybTogdXBwZXJjYXNlO1xuICBsZXR0ZXItc3BhY2luZzogMXB4O1xuICBjb2xvcjogIzg4ODtcbiAgcGFkZGluZzogOHB4IDEycHg7XG4gIGJhY2tncm91bmQ6ICMyYTJhMmE7XG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjM2MzYzNjO1xuICBmbGV4LXNocmluazogMDtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA2cHg7XG5cbiAgLmxhYmVsLXRleHQge1xuICAgIGZsZXg6IDE7XG4gIH1cbn1cblxuLnJlc2V0LWNoYXQtYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIGZvbnQtc2l6ZTogOXB4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHBhZGRpbmc6IDJweCA4cHg7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgd2hpdGUtc3BhY2U6IG5vd3JhcDtcbiAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICY6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogI2VmNTM1MDtcbiAgICBjb2xvcjogI2VmNTM1MDtcbiAgfVxufVxuXG4uY29sbGFwc2UtY2hhdC1idG4ge1xuICBiYWNrZ3JvdW5kOiBub25lO1xuICBib3JkZXI6IDFweCBzb2xpZCAjNTU1O1xuICBjb2xvcjogI2FhYTtcbiAgZm9udC1zaXplOiA5cHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcbiAgcGFkZGluZzogMnB4IDhweDtcbiAgYm9yZGVyLXJhZGl1czogM3B4O1xuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJjpob3ZlciB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICB9XG59XG5cbi8qIENoYXQgbWVzc2FnZXMgKi9cbi5jaGF0LW1lc3NhZ2VzIHtcbiAgZmxleDogMTtcbiAgb3ZlcmZsb3cteTogYXV0bztcbiAgcGFkZGluZzogMTJweDtcbn1cblxuLm1zZyB7XG4gIG1hcmdpbi1ib3R0b206IDEycHg7XG5cbiAgLmF1dGhvciB7XG4gICAgZm9udC1zaXplOiAxMXB4O1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgbWFyZ2luLWJvdHRvbTogMnB4O1xuXG4gICAgJi51c2VyIHtcbiAgICAgIGNvbG9yOiAjODJiMWZmO1xuICAgIH1cblxuICAgICYuYXNzaXN0YW50IHtcbiAgICAgIGNvbG9yOiAjYzc5MmVhO1xuICAgIH1cblxuICAgIC50aW1lIHtcbiAgICAgIGNvbG9yOiAjNjY2O1xuICAgICAgZm9udC13ZWlnaHQ6IDQwMDtcbiAgICAgIGZvbnQtc2l6ZTogMTBweDtcbiAgICAgIG1hcmdpbi1sZWZ0OiA2cHg7XG4gICAgfVxuICB9XG5cbiAgLnRleHQge1xuICAgIGZvbnQtc2l6ZTogMTJweDtcbiAgICBsaW5lLWhlaWdodDogMS41O1xuICAgIGNvbG9yOiAjY2NjO1xuICAgIHdoaXRlLXNwYWNlOiBwcmUtd3JhcDtcblxuICAgICYuc3RyZWFtaW5nIHtcbiAgICAgIC5zdHJlYW1pbmctY3Vyc29yIHtcbiAgICAgICAgZGlzcGxheTogaW5saW5lLWJsb2NrO1xuICAgICAgICB3aWR0aDogNnB4O1xuICAgICAgICBoZWlnaHQ6IDE0cHg7XG4gICAgICAgIGJhY2tncm91bmQ6ICNjNzkyZWE7XG4gICAgICAgIG1hcmdpbi1sZWZ0OiAycHg7XG4gICAgICAgIHZlcnRpY2FsLWFsaWduOiB0ZXh0LWJvdHRvbTtcbiAgICAgICAgYW5pbWF0aW9uOiBibGluay1jdXJzb3IgMC44cyBzdGVwLWVuZCBpbmZpbml0ZTtcbiAgICAgIH1cbiAgICB9XG4gIH1cbn1cblxuQGtleWZyYW1lcyBibGluay1jdXJzb3Ige1xuICAwJSwgMTAwJSB7IG9wYWNpdHk6IDE7IH1cbiAgNTAlIHsgb3BhY2l0eTogMDsgfVxufVxuXG4uY2hhdC1lbXB0eSB7XG4gIGNvbG9yOiAjNTU1O1xuICBmb250LXNpemU6IDEycHg7XG4gIGZvbnQtc3R5bGU6IGl0YWxpYztcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xuICBwYWRkaW5nOiAyNHB4IDEycHg7XG59XG5cbi8qIENoYXQgaW5wdXQgKi9cbi5jaGF0LWlucHV0IHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGZsZXgtZW5kO1xuICBwYWRkaW5nOiA4cHggMTJweDtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGdhcDogOHB4O1xuICBmbGV4LXNocmluazogMDtcblxuICB0ZXh0YXJlYSB7XG4gICAgZmxleDogMTtcbiAgICBiYWNrZ3JvdW5kOiAjM2MzYzNjO1xuICAgIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gICAgYm9yZGVyLXJhZGl1czogNHB4O1xuICAgIHBhZGRpbmc6IDZweCAxMHB4O1xuICAgIGNvbG9yOiAjY2NjO1xuICAgIGZvbnQtc2l6ZTogMTJweDtcbiAgICBmb250LWZhbWlseTogJ1NlZ29lIFVJJywgc2Fucy1zZXJpZjtcbiAgICBvdXRsaW5lOiBub25lO1xuICAgIHJlc2l6ZTogbm9uZTtcblxuICAgICY6Zm9jdXMge1xuICAgICAgYm9yZGVyLWNvbG9yOiAjODg4O1xuICAgIH1cblxuICAgICY6ZGlzYWJsZWQge1xuICAgICAgb3BhY2l0eTogMC41O1xuICAgICAgY3Vyc29yOiBub3QtYWxsb3dlZDtcbiAgICB9XG4gIH1cblxuICBidXR0b24ge1xuICAgIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gICAgYm9yZGVyOiBub25lO1xuICAgIGNvbG9yOiAjMDAwO1xuICAgIHBhZGRpbmc6IDZweCAxMnB4O1xuICAgIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgICBmb250LXNpemU6IDEycHg7XG4gICAgY3Vyc29yOiBwb2ludGVyO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG4gICAgdHJhbnNpdGlvbjogZmlsdGVyIDAuMTVzO1xuXG4gICAgJjpob3Zlcjpub3QoOmRpc2FibGVkKSB7XG4gICAgICBmaWx0ZXI6IGJyaWdodG5lc3MoMS4xKTtcbiAgICB9XG5cbiAgICAmOmRpc2FibGVkIHtcbiAgICAgIG9wYWNpdHk6IDAuNTtcbiAgICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gICAgfVxuICB9XG59XG5cbi8qIFNwbGl0dGVyICovXG4uY29sdW1uLXNwbGl0dGVyIHtcbiAgd2lkdGg6IDVweDtcbiAgYmFja2dyb3VuZDogIzJhMmEyYTtcbiAgY3Vyc29yOiBjb2wtcmVzaXplO1xuICBmbGV4LXNocmluazogMDtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHRyYW5zaXRpb246IGJhY2tncm91bmQgMC4xNXM7XG4gIHVzZXItc2VsZWN0OiBub25lO1xuXG4gICY6aG92ZXIge1xuICAgIGJhY2tncm91bmQ6ICNmZjk4MDA7XG5cbiAgICAuZ3JpcCB7XG4gICAgICBjb2xvcjogIzAwMDtcbiAgICB9XG4gIH1cblxuICAuZ3JpcCB7XG4gICAgY29sb3I6ICM1NTU7XG4gICAgZm9udC1zaXplOiAxMHB4O1xuICAgIHdyaXRpbmctbW9kZTogdmVydGljYWwtbHI7XG4gICAgbGV0dGVyLXNwYWNpbmc6IDJweDtcbiAgICBwb2ludGVyLWV2ZW50czogbm9uZTtcbiAgfVxufVxuXG4vKiBQcm9tcHQgY29sdW1uICovXG4ucHJvbXB0LWNvbHVtbiB7XG4gIGZsZXg6IDE7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIG1pbi13aWR0aDogMjAwcHg7XG4gIG1pbi1oZWlnaHQ6IDA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG59XG5cbi5wcm9tcHQtY29udGVudCB7XG4gIGZsZXg6IDE7XG4gIHBhZGRpbmc6IDE2cHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIG1pbi1oZWlnaHQ6IDA7XG59XG5cbi5wcm9tcHQtcmVuZGVyZWQge1xuICBmb250LXNpemU6IDEzcHg7XG4gIGxpbmUtaGVpZ2h0OiAxLjc7XG4gIGNvbG9yOiAjZTBlMGUwO1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHBhZGRpbmc6IDRweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuMTVzO1xuXG4gICY6aG92ZXIgeyBiYWNrZ3JvdW5kOiAjMmEyYTJhOyB9XG5cbiAgaDIsIGgzLCBoNCB7IGNvbG9yOiAjZmZmOyBtYXJnaW46IDEycHggMCA2cHg7IH1cbiAgaDIgeyBmb250LXNpemU6IDE2cHg7IH1cbiAgaDMgeyBmb250LXNpemU6IDE0cHg7IH1cbiAgaDQgeyBmb250LXNpemU6IDEzcHg7IH1cbiAgc3Ryb25nIHsgY29sb3I6ICNmZmY7IH1cbiAgZW0geyBjb2xvcjogI2NjYzsgZm9udC1zdHlsZTogaXRhbGljOyB9XG4gIGNvZGUgeyBiYWNrZ3JvdW5kOiAjMWUxZTFlOyBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjOyBib3JkZXItcmFkaXVzOiAzcHg7IHBhZGRpbmc6IDFweCA0cHg7IGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7IGZvbnQtc2l6ZTogMTJweDsgY29sb3I6ICNjZTkxNzg7IH1cbiAgdWwsIG9sIHsgcGFkZGluZy1sZWZ0OiAyMHB4OyBtYXJnaW46IDA7IH1cbiAgbGkgeyBtYXJnaW46IDA7IHBhZGRpbmc6IDA7IGxpbmUtaGVpZ2h0OiAxLjQ7IH1cbiAgcCB7IG1hcmdpbjogMDsgfVxuXG4gIC5wYXJhbS1oaWdobGlnaHQge1xuICAgIGJhY2tncm91bmQ6ICMzMzMwMmE7XG4gICAgYm9yZGVyOiAxcHggc29saWQgI2ZmOTgwMDtcbiAgICBib3JkZXItcmFkaXVzOiAzcHg7XG4gICAgcGFkZGluZzogMXB4IDRweDtcbiAgICBjb2xvcjogI2ZmY2M4MDtcbiAgICBmb250LWZhbWlseTogJ0NvdXJpZXIgTmV3JywgbW9ub3NwYWNlO1xuICAgIGZvbnQtc2l6ZTogMTJweDtcbiAgfVxufVxuXG4ucHJvbXB0LWVkaXRvciB7XG4gIHdpZHRoOiAxMDAlO1xuICBtaW4taGVpZ2h0OiAyMDBweDtcbiAgaGVpZ2h0OiAxMDAlO1xuICBiYWNrZ3JvdW5kOiAjMWUxZTFlO1xuICBib3JkZXI6IDFweCBzb2xpZCAjZmY5ODAwO1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIHBhZGRpbmc6IDhweDtcbiAgY29sb3I6ICNlMGUwZTA7XG4gIGZvbnQtc2l6ZTogMTNweDtcbiAgZm9udC1mYW1pbHk6ICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcbiAgbGluZS1oZWlnaHQ6IDEuNztcbiAgcmVzaXplOiBub25lO1xuICBvdXRsaW5lOiBub25lO1xuICBib3gtc2l6aW5nOiBib3JkZXItYm94O1xufVxuXG4vKiBEaXN0aWxsYXRpb24gaW5kaWNhdG9yICovXG4uZGlzdGlsbGF0aW9uLWluZGljYXRvciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogNnB4O1xuICBwYWRkaW5nOiA2cHggMTJweDtcbiAgZm9udC1zaXplOiAxMHB4O1xuICBjb2xvcjogI2ZmOTgwMDtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGZsZXgtc2hyaW5rOiAwO1xufVxuXG4uZGlzdGlsbC1zcGlubmVyIHtcbiAgZGlzcGxheTogaW5saW5lLWJsb2NrO1xuICB3aWR0aDogMTBweDtcbiAgaGVpZ2h0OiAxMHB4O1xuICBib3JkZXI6IDJweCBzb2xpZCAjNTU1O1xuICBib3JkZXItdG9wLWNvbG9yOiAjZmY5ODAwO1xuICBib3JkZXItcmFkaXVzOiA1MCU7XG4gIGFuaW1hdGlvbjogZGlzdGlsbC1zcGluIDAuOHMgbGluZWFyIGluZmluaXRlO1xufVxuXG5Aa2V5ZnJhbWVzIGRpc3RpbGwtc3BpbiB7XG4gIHRvIHsgdHJhbnNmb3JtOiByb3RhdGUoMzYwZGVnKTsgfVxufVxuXG4ucHJvbXB0LXRleHQge1xuICBmb250LXNpemU6IDEzcHg7XG4gIGxpbmUtaGVpZ2h0OiAxLjg7XG4gIGNvbG9yOiAjZTBlMGUwO1xuICB3aGl0ZS1zcGFjZTogcHJlLXdyYXA7XG4gIG91dGxpbmU6IG5vbmU7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgcGFkZGluZzogNHB4O1xuICB0cmFuc2l0aW9uOiBiYWNrZ3JvdW5kIDAuMTVzO1xuICBtaW4taGVpZ2h0OiA2MHB4O1xuXG4gICY6Zm9jdXMge1xuICAgIGJhY2tncm91bmQ6ICMyYTJhMmE7XG4gICAgYm94LXNoYWRvdzogMCAwIDAgMXB4ICNmZjk4MDAgaW5zZXQ7XG4gIH1cbn1cblxuLnByb21wdC1ub3RlIHtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xuICBib3JkZXItdG9wOiAxcHggc29saWQgIzNjM2MzYztcbiAgZm9udC1zaXplOiAxMHB4O1xuICBjb2xvcjogIzY2NjtcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi8qIMOiwpXCkMOiwpXCkMOiwpXCkCBVbHRpbW8gUnVuIFBhbmVsIChUYXNrIDguMikgw6LClcKQw6LClcKQw6LClcKQICovXG5cbi51bHRpbW8tcnVuIHtcbiAgYm9yZGVyLXRvcDogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGJhY2tncm91bmQ6ICMxZTFlMWU7XG4gIGZsZXgtc2hyaW5rOiAwO1xuICBtYXgtaGVpZ2h0OiAzNTBweDtcbiAgb3ZlcmZsb3cteTogYXV0bztcbn1cblxuLnJ1bi1zZWN0aW9uIHtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzJhMmEyYTtcblxuICAmOmxhc3QtY2hpbGQge1xuICAgIGJvcmRlci1ib3R0b206IG5vbmU7XG4gIH1cbn1cblxuLnJ1bi1zZWN0aW9uLXRpdGxlIHtcbiAgZm9udC1zaXplOiAxMHB4O1xuICB0ZXh0LXRyYW5zZm9ybTogdXBwZXJjYXNlO1xuICBsZXR0ZXItc3BhY2luZzogMXB4O1xuICBjb2xvcjogIzg4ODtcbiAgbWFyZ2luLWJvdHRvbTogOHB4O1xufVxuXG4ucnVuLW1ldGEge1xuICBkaXNwbGF5OiBmbGV4O1xuICBnYXA6IDE2cHg7XG4gIGZsZXgtd3JhcDogd3JhcDtcbn1cblxuLnJ1bi1tZXRhLWl0ZW0ge1xuICBmb250LXNpemU6IDExcHg7XG4gIGNvbG9yOiAjYWFhO1xuXG4gIHN0cm9uZyB7XG4gICAgY29sb3I6ICNjY2M7XG4gIH1cbn1cblxuLnJ1bi1wYXJhbXMge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDRweDtcbn1cblxuLnJ1bi1wYXJhbSB7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgZ2FwOiA2cHg7XG59XG5cbi5ydW4tcGFyYW0tbmFtZSB7XG4gIGNvbG9yOiAjODg4O1xufVxuXG4ucnVuLXBhcmFtLWVxIHtcbiAgY29sb3I6ICM1NTU7XG59XG5cbi5ydW4tcGFyYW0tdmFsIHtcbiAgY29sb3I6ICNlMGUwZTA7XG4gIGZvbnQtd2VpZ2h0OiA1MDA7XG59XG5cbi5ydW4tcHJvbXB0IHtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBsaW5lLWhlaWdodDogMS43O1xuICBjb2xvcjogI2NjYztcbiAgYmFja2dyb3VuZDogIzI1MjUyNjtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBwYWRkaW5nOiAxMnB4O1xuICB3aGl0ZS1zcGFjZTogcHJlLXdyYXA7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG59XG5cbi5ydW4tb3V0cHV0IHtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBsaW5lLWhlaWdodDogMS43O1xuICBjb2xvcjogI2NjYztcbiAgYmFja2dyb3VuZDogIzI1MjUyNjtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBwYWRkaW5nOiAxMnB4O1xuICB3aGl0ZS1zcGFjZTogcHJlLXdyYXA7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG4gIG1heC1oZWlnaHQ6IDIwMHB4O1xuICBvdmVyZmxvdy15OiBhdXRvO1xufVxuXG4vKiDDosKVwpDDosKVwpDDosKVwpAgRGlhZ3JhbSBQYW5lbCAoVGFzayA5LjEpIMOiwpXCkMOiwpXCkMOiwpXCkCAqL1xuXG4uZGlhZ3JhbS1wYW5lbCB7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBiYWNrZ3JvdW5kOiAjMWExZDIxO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmRpYWdyYW0taGVhZGVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAgcGFkZGluZzogOHB4IDE2cHg7XG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjM2MzYzNjO1xufVxuXG4uZGlhZ3JhbS10aXRsZSB7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgdGV4dC10cmFuc2Zvcm06IHVwcGVyY2FzZTtcbiAgbGV0dGVyLXNwYWNpbmc6IDFweDtcbiAgY29sb3I6ICM4MmIxZmY7XG4gIGZsZXg6IDE7XG59XG5cbi5kaWFncmFtLWNvcHktYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIGZvbnQtc2l6ZTogMTBweDtcbiAgcGFkZGluZzogMnB4IDhweDtcbiAgYm9yZGVyLXJhZGl1czogM3B4O1xuICBjdXJzb3I6IHBvaW50ZXI7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyOm5vdCg6ZGlzYWJsZWQpIHtcbiAgICBib3JkZXItY29sb3I6ICM4MmIxZmY7XG4gICAgY29sb3I6ICM4MmIxZmY7XG4gIH1cblxuICAmOmRpc2FibGVkIHtcbiAgICBvcGFjaXR5OiAwLjQ7XG4gICAgY3Vyc29yOiBub3QtYWxsb3dlZDtcbiAgfVxufVxuXG4uZGlhZ3JhbS1ib2R5IHtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xufVxuXG4uZGlhZ3JhbS1zdmcge1xuICBiYWNrZ3JvdW5kOiAjZmZmO1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIHBhZGRpbmc6IDEycHg7XG4gIG92ZXJmbG93OiBhdXRvO1xuICBtYXgtaGVpZ2h0OiA0MDBweDtcbiAgdGV4dC1hbGlnbjogY2VudGVyO1xuXG4gIDo6bmctZGVlcCBzdmcge1xuICAgIG1heC13aWR0aDogMTAwJTtcbiAgICBoZWlnaHQ6IGF1dG87XG4gIH1cbn1cblxuLmRpYWdyYW0tY29kZSB7XG4gIGZvbnQtZmFtaWx5OiAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgY29sb3I6ICNjY2M7XG4gIGJhY2tncm91bmQ6ICMyNTI1MjY7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgcGFkZGluZzogMTJweDtcbiAgb3ZlcmZsb3cteDogYXV0bztcbiAgd2hpdGUtc3BhY2U6IHByZTtcbiAgbWF4LWhlaWdodDogMzAwcHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIG1hcmdpbjogMDtcbn1cblxuLmRpYWdyYW0tbG9hZGluZyB7XG4gIGNvbG9yOiAjODg4O1xuICBmb250LXNpemU6IDEycHg7XG4gIGZvbnQtc3R5bGU6IGl0YWxpYztcbn1cblxuLmRpYWdyYW0tZW1wdHkge1xuICBjb2xvcjogIzU1NTtcbiAgZm9udC1zaXplOiAxMnB4O1xufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"],
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


function PromptLabDocPanelComponent_div_13_div_1_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 12);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, " Nessuno slot. Aggiungi card alla sessione. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
  }
}
function PromptLabDocPanelComponent_div_13_div_2_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 13)(1, "span", 14);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "div", 15)(4, "div", 16)(5, "span", 17);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](6);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](7, "div", 18);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "span", 19);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10, "\u25B6");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
  }
  if (rf & 2) {
    const card_r3 = ctx.$implicit;
    const i_r4 = ctx.index;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](i_r4 + 1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](card_r3.generatedTitle || "Card " + (i_r4 + 1));
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](card_r3.distilledPrompt || "(prompt vuoto)");
  }
}
function PromptLabDocPanelComponent_div_13_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "div", 9);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](1, PromptLabDocPanelComponent_div_13_div_1_Template, 2, 0, "div", 10);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](2, PromptLabDocPanelComponent_div_13_div_2_Template, 11, 3, "div", 11);
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
      decls: 14,
      vars: 5,
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
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10, " prompt ");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "button", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function PromptLabDocPanelComponent_Template_button_click_11_listener() {
            return ctx.toggleCollapse();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](12, "\u25BC");
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](13, PromptLabDocPanelComponent_div_13_Template, 3, 2, "div", 8);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵclassProp"]("collapsed", ctx.collapsed);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.templateName);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](ctx.cards.length);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](4);
          _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngIf", !ctx.collapsed);
        }
      },
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_1__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_1__.NgIf],
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
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @angular/common */ 4666);
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @angular/forms */ 2508);
/* harmony import */ var _promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../promptlab-doc-panel/promptlab-doc-panel.component */ 1381);
/* harmony import */ var _promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../promptlab-agent-card/promptlab-agent-card.component */ 4060);
/* harmony import */ var _promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../promptlab-card/promptlab-card.component */ 748);













function PromptLabComponent_option_8_Template(rf, ctx) {
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
function PromptLabComponent_span_9_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "span", 24);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](1, "span", 25);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2, " Loading models... ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabComponent_app_promptlab_agent_card_25_Template(rf, ctx) {
  if (rf & 1) {
    const _r9 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "app-promptlab-agent-card", 26);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("agentDefinitionChange", function PromptLabComponent_app_promptlab_agent_card_25_Template_app_promptlab_agent_card_agentDefinitionChange_0_listener($event) {
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
function PromptLabComponent_div_28_Template(rf, ctx) {
  if (rf & 1) {
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 27);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](1, " Nessuna card. Clicca '+ Card' per iniziare. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
  }
}
function PromptLabComponent_app_promptlab_card_29_Template(rf, ctx) {
  if (rf & 1) {
    const _r12 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "app-promptlab-card", 28);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("cardDeleted", function PromptLabComponent_app_promptlab_card_29_Template_app_promptlab_card_cardDeleted_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r12);
      const ctx_r11 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r11.onCardDeleted($event));
    })("cardChanged", function PromptLabComponent_app_promptlab_card_29_Template_app_promptlab_card_cardChanged_0_listener($event) {
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
function PromptLabComponent_div_30_Template(rf, ctx) {
  if (rf & 1) {
    const _r15 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 29);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_30_Template_div_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r15);
      const ctx_r14 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r14.onSettingsBackdropClick($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 30)(2, "div", 31)(3, "span", 32);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "Impostazioni Sessione");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "button", 33);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_30_Template_button_click_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r15);
      const ctx_r16 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r16.closeSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "\u2715");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "div", 34)(8, "div", 35)(9, "div", 36)(10, "label", 37);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](11, "System Prompt");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](12, "button", 38);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_30_Template_button_click_12_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r15);
      const ctx_r17 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r17.resetSystemPrompt());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](13, "\u21BA Default");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](14, "p", 39);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](15, " Istruzioni di sistema inviate al LLM all'inizio di ogni conversazione nella chat. Definisce il ruolo dell'LLM come assistente alla progettazione di prompt. ");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](16, "textarea", 40);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_div_30_Template_textarea_ngModelChange_16_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r15);
      const ctx_r18 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r18.onSystemPromptChange($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](17, "div", 41)(18, "button", 42);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_30_Template_button_click_18_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r15);
      const ctx_r19 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r19.closeSettings());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "Chiudi");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](16);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx_r5.systemPrompt);
  }
}
function PromptLabComponent_div_31_Template(rf, ctx) {
  if (rf & 1) {
    const _r21 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 43);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_31_Template_div_click_0_listener($event) {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r20 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r20.onBuildBackdropClick($event));
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](1, "div", 44)(2, "div", 45)(3, "span", 46);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "Build Output");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](5, "button", 47);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_31_Template_button_click_5_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r22 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r22.closeBuildOverlay());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](6, "\u2715");
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](7, "div", 48)(8, "pre", 49);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](10, "div", 50)(11, "button", 51);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_div_31_Template_button_click_11_listener() {
      _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵrestoreView"](_r21);
      const ctx_r23 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
      return _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵresetView"](ctx_r23.copyBuildOutput());
    });
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](12);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()()()();
  }
  if (rf & 2) {
    const ctx_r6 = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵnextContext"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](9);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx_r6.buildOutput);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](3);
    _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx_r6.buildCopied ? "Copiato!" : "Copia", " ");
  }
}
class PromptLabComponent {
  static {
    this.cachedModels = null;
  }
  constructor(cdr, http, promptLabService, mdFileService, aiChatService) {
    this.cdr = cdr;
    this.http = http;
    this.promptLabService = promptLabService;
    this.mdFileService = mdFileService;
    this.aiChatService = aiChatService;
    this.mode = 'prompt';
    this.selectedModel = 'claude-sonnet-4';
    this.sessionTitle = 'Nuova Sessione';
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
    this.models = PromptLabComponent.cachedModels || [];
    this.isLoadingModels = !PromptLabComponent.cachedModels;
    this.destroy$ = new rxjs__WEBPACK_IMPORTED_MODULE_8__.Subject();
  }
  ngOnInit() {
    // 1. Subscribe to session$ to keep local state in sync
    this.promptLabService.session$.pipe((0,rxjs_operators__WEBPACK_IMPORTED_MODULE_9__.takeUntil)(this.destroy$)).subscribe(session => {
      if (session) {
        this.sessionTitle = session.title || 'Nuova Sessione';
        this.selectedModel = session.model || 'gpt-4o';
        this.mode = session.mode || 'prompt';
        this.cards = session.cards || [];
        this.templateName = session.templatePath ? session.templatePath.split(/[/\\]/).pop() || 'template.md' : 'template.md';
        this.systemPrompt = session.systemPrompt || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT;
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
    // If already cached, use immediately — no loading state, but still refresh in background
    if (PromptLabComponent.cachedModels?.length) {
      this.models = PromptLabComponent.cachedModels;
      this.isLoadingModels = false;
    } else {
      this.isLoadingModels = true;
    }
    // Always call CLI (first time: blocking with spinner, subsequent: silent background refresh)
    this.aiChatService.refreshCopilotCliModels().subscribe({
      next: response => {
        const modelList = response?.models || [];
        if (modelList.length) {
          this.models = modelList.map(m => ({
            value: m.id || m.Id || m,
            label: m.name || m.Name || m.id || m.Id || m
          }));
          PromptLabComponent.cachedModels = this.models;
          if (!this.models.find(mod => mod.value === this.selectedModel) && this.models.length > 0) {
            this.selectedModel = this.models[0].value;
            this.promptLabService.setModel(this.selectedModel);
          }
        }
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.models = [];
        this.isLoadingModels = false;
        this.cdr.markForCheck();
      }
    });
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
  onSettingsBackdropClick(event) {
    if (event.target.classList.contains('modal-backdrop')) {
      this.closeSettings();
    }
  }
  static {
    this.ɵfac = function PromptLabComponent_Factory(t) {
      return new (t || PromptLabComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_core__WEBPACK_IMPORTED_MODULE_7__.ChangeDetectorRef), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_11__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_promptlab_service__WEBPACK_IMPORTED_MODULE_1__.PromptLabService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__.MdFileService), _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdirectiveInject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_3__.AiChatService));
    };
  }
  static {
    this.ɵcmp = /*@__PURE__*/_angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineComponent"]({
      type: PromptLabComponent,
      selectors: [["app-promptlab"]],
      decls: 32,
      vars: 24,
      consts: [[1, "session-header"], [1, "session-title"], [1, "badge"], [1, "spacer"], [1, "model-selector-wrapper"], [1, "model-selector", 3, "ngModel", "ngModelChange"], [3, "value", 4, "ngFor", "ngForOf"], ["class", "model-loading", 4, "ngIf"], [1, "mode-toggle"], [1, "mode-btn", 3, "click"], [1, "separator"], [1, "session-action-btn", "add", 3, "click"], [1, "session-action-btn", "build", 3, "click"], [1, "session-action-btn", "exec", 3, "disabled", "click"], ["title", "Impostazioni sessione", 1, "session-action-btn", "settings", 3, "click"], [1, "session-content"], [3, "agentDefinition", "agentDefinitionChange", 4, "ngIf"], [3, "cards", "templateName"], [1, "cards-area"], ["class", "cards-placeholder", 4, "ngIf"], [3, "card", "isSingleCard", "cardDeleted", "cardChanged", 4, "ngFor", "ngForOf", "ngForTrackBy"], ["class", "modal-backdrop", 3, "click", 4, "ngIf"], ["class", "build-overlay-backdrop", 3, "click", 4, "ngIf"], [3, "value"], [1, "model-loading"], [1, "spinner"], [3, "agentDefinition", "agentDefinitionChange"], [1, "cards-placeholder"], [3, "card", "isSingleCard", "cardDeleted", "cardChanged"], [1, "modal-backdrop", 3, "click"], [1, "modal-card", "settings-modal"], [1, "modal-header"], [1, "modal-title"], [1, "modal-close", 3, "click"], [1, "modal-body"], [1, "settings-section"], [1, "settings-section-header"], [1, "settings-label"], ["title", "Ripristina default", 1, "settings-reset-btn", 3, "click"], [1, "settings-hint"], ["rows", "12", "spellcheck", "false", 1, "settings-textarea", 3, "ngModel", "ngModelChange"], [1, "modal-footer"], [1, "modal-btn", 3, "click"], [1, "build-overlay-backdrop", 3, "click"], [1, "build-overlay-card"], [1, "build-overlay-header"], [1, "build-overlay-title"], [1, "build-overlay-close", 3, "click"], [1, "build-overlay-body"], [1, "build-overlay-pre"], [1, "build-overlay-footer"], [1, "build-overlay-copy", 3, "click"]],
      template: function PromptLabComponent_Template(rf, ctx) {
        if (rf & 1) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](0, "div", 0)(1, "span", 1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](3, "span", 2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](4, "PromptLab");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](5, "span", 3);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](6, "div", 4)(7, "select", 5);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("ngModelChange", function PromptLabComponent_Template_select_ngModelChange_7_listener($event) {
            return ctx.selectedModel = $event;
          })("ngModelChange", function PromptLabComponent_Template_select_ngModelChange_7_listener($event) {
            return ctx.onModelChange($event);
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](8, PromptLabComponent_option_8_Template, 2, 2, "option", 6);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](9, PromptLabComponent_span_9_Template, 3, 0, "span", 7);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](10, "div", 8)(11, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_11_listener() {
            return ctx.toggleMode("prompt");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](12, "Prompt");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](13, "button", 9);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_13_listener() {
            return ctx.toggleMode("agent");
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](14, "Agente");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](15, "span", 10);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](16, "button", 11);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_16_listener() {
            return ctx.addCard();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](17, "+ Card");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](18, "button", 12);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_18_listener() {
            return ctx.build();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](19, "Build");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](20, "button", 13);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_20_listener() {
            return ctx.executeAll();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](21);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](22, "button", 14);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵlistener"]("click", function PromptLabComponent_Template_button_click_22_listener() {
            return ctx.openSettings();
          });
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtext"](23, "\u2699");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](24, "div", 15);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](25, PromptLabComponent_app_promptlab_agent_card_25_Template, 1, 1, "app-promptlab-agent-card", 16);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelement"](26, "app-promptlab-doc-panel", 17);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementStart"](27, "div", 18);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](28, PromptLabComponent_div_28_Template, 2, 0, "div", 19);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](29, PromptLabComponent_app_promptlab_card_29_Template, 1, 2, "app-promptlab-card", 20);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵelementEnd"]()();
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](30, PromptLabComponent_div_30_Template, 20, 1, "div", 21);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtemplate"](31, PromptLabComponent_div_31_Template, 13, 2, "div", 22);
        }
        if (rf & 2) {
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate"](ctx.sessionTitle);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](5);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵstyleProp"]("display", ctx.isLoadingModels ? "none" : "inline-block");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngModel", ctx.selectedModel);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngForOf", ctx.models);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("ngIf", ctx.isLoadingModels);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.mode === "prompt");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](2);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵclassProp"]("active", ctx.mode === "agent");
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](7);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵproperty"]("disabled", ctx.isExecutingAll);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵadvance"](1);
          _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵtextInterpolate1"](" ", ctx.isExecutingAll ? "Esecuzione..." : "Esegui Tutto", " ");
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
      dependencies: [_angular_common__WEBPACK_IMPORTED_MODULE_12__.NgForOf, _angular_common__WEBPACK_IMPORTED_MODULE_12__.NgIf, _angular_forms__WEBPACK_IMPORTED_MODULE_13__.NgSelectOption, _angular_forms__WEBPACK_IMPORTED_MODULE_13__["ɵNgSelectMultipleOption"], _angular_forms__WEBPACK_IMPORTED_MODULE_13__.DefaultValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_13__.SelectControlValueAccessor, _angular_forms__WEBPACK_IMPORTED_MODULE_13__.NgControlStatus, _angular_forms__WEBPACK_IMPORTED_MODULE_13__.NgModel, _promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_4__.PromptLabDocPanelComponent, _promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_5__.PromptLabAgentCardComponent, _promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_6__.PromptLabCardComponent],
      styles: ["[_nghost-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  flex: 1 1 auto;\n  height: 100%;\n  min-height: 0;\n  min-width: 0;\n  overflow: hidden;\n  background: #1e1e1e;\n  color: #ccc;\n  font-family: \"Segoe UI\", sans-serif;\n  font-size: 13px;\n}\n\n\n.session-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  gap: 10px;\n  padding: 10px 16px;\n  background: #2d2d2d;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n  flex-wrap: wrap;\n}\n\n.session-title[_ngcontent-%COMP%] {\n  font-size: 18px;\n  color: #fff;\n  font-weight: 600;\n}\n\n.badge[_ngcontent-%COMP%] {\n  font-size: 10px;\n  background: #ff9800;\n  color: #000;\n  padding: 2px 8px;\n  border-radius: 10px;\n  font-weight: 600;\n}\n\n.spacer[_ngcontent-%COMP%] {\n  flex: 1;\n}\n\n\n.model-selector[_ngcontent-%COMP%] {\n  background: #3c3c3c;\n  border: 1px solid #555;\n  border-radius: 4px;\n  color: #ccc;\n  font-size: 11px;\n  padding: 4px 8px;\n  cursor: pointer;\n  outline: none;\n}\n.model-selector[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n}\n.model-selector[_ngcontent-%COMP%]   option[_ngcontent-%COMP%] {\n  background: #3c3c3c;\n  color: #ccc;\n}\n\n.model-selector-wrapper[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n}\n\n.model-loading[_ngcontent-%COMP%] {\n  display: inline-flex;\n  align-items: center;\n  gap: 6px;\n  font-size: 11px;\n  color: #888;\n}\n\n.spinner[_ngcontent-%COMP%] {\n  display: inline-block;\n  width: 12px;\n  height: 12px;\n  border: 2px solid #555;\n  border-top-color: #ff9800;\n  border-radius: 50%;\n  animation: _ngcontent-%COMP%_spin 0.8s linear infinite;\n}\n\n@keyframes _ngcontent-%COMP%_spin {\n  to {\n    transform: rotate(360deg);\n  }\n}\n\n.mode-toggle[_ngcontent-%COMP%] {\n  display: flex;\n  border: 1px solid #555;\n  border-radius: 4px;\n  overflow: hidden;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 11px;\n  padding: 4px 12px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn.active[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  font-weight: 600;\n}\n.mode-toggle[_ngcontent-%COMP%]   .mode-btn[_ngcontent-%COMP%]:not(.active):hover {\n  background: #3c3c3c;\n  color: #ccc;\n}\n\n\n.separator[_ngcontent-%COMP%] {\n  width: 1px;\n  height: 24px;\n  background: #555;\n  flex-shrink: 0;\n}\n\n\n.session-action-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  font-size: 11px;\n  padding: 4px 12px;\n  border-radius: 4px;\n  cursor: pointer;\n  white-space: nowrap;\n  transition: all 0.15s;\n}\n.session-action-btn.add[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n.session-action-btn.build[_ngcontent-%COMP%]:hover {\n  border-color: #82b1ff;\n  color: #82b1ff;\n}\n.session-action-btn.exec[_ngcontent-%COMP%] {\n  border-color: #ff9800;\n  color: #ff9800;\n  font-weight: 600;\n}\n.session-action-btn.exec[_ngcontent-%COMP%]:hover:not(:disabled) {\n  background: #ff9800;\n  color: #000;\n}\n.session-action-btn.exec[_ngcontent-%COMP%]:disabled {\n  opacity: 0.5;\n  cursor: not-allowed;\n}\n.session-action-btn.settings[_ngcontent-%COMP%] {\n  font-size: 14px;\n  padding: 4px 8px;\n}\n.session-action-btn.settings[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n  color: #ccc;\n}\n\n\n.modal-backdrop[_ngcontent-%COMP%] {\n  position: fixed;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background: rgba(0, 0, 0, 0.6);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  z-index: 1000;\n}\n\n.modal-card[_ngcontent-%COMP%] {\n  background: #2d2d2d;\n  border: 1px solid #555;\n  border-radius: 6px;\n  min-width: 400px;\n  max-width: 600px;\n  max-height: 80vh;\n  display: flex;\n  flex-direction: column;\n  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);\n}\n.modal-card.settings-modal[_ngcontent-%COMP%] {\n  min-width: 600px;\n  max-width: 800px;\n}\n\n.modal-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n}\n\n.modal-title[_ngcontent-%COMP%] {\n  flex: 1;\n  font-size: 14px;\n  font-weight: 600;\n  color: #e0e0e0;\n}\n\n.modal-close[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 16px;\n  cursor: pointer;\n  padding: 2px 6px;\n}\n.modal-close[_ngcontent-%COMP%]:hover {\n  color: #fff;\n}\n\n.modal-body[_ngcontent-%COMP%] {\n  padding: 20px;\n  overflow-y: auto;\n  flex: 1;\n}\n\n.modal-footer[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  gap: 8px;\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n}\n\n.modal-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #aaa;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  cursor: pointer;\n}\n.modal-btn[_ngcontent-%COMP%]:hover {\n  border-color: #888;\n  color: #e0e0e0;\n}\n\n.settings-section[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  gap: 8px;\n}\n\n.settings-section-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: space-between;\n}\n\n.settings-label[_ngcontent-%COMP%] {\n  font-size: 13px;\n  font-weight: 600;\n  color: #e0e0e0;\n}\n\n.settings-reset-btn[_ngcontent-%COMP%] {\n  background: none;\n  border: 1px solid #555;\n  color: #888;\n  font-size: 11px;\n  padding: 3px 10px;\n  border-radius: 3px;\n  cursor: pointer;\n  transition: all 0.15s;\n}\n.settings-reset-btn[_ngcontent-%COMP%]:hover {\n  border-color: #ff9800;\n  color: #ff9800;\n}\n\n.settings-hint[_ngcontent-%COMP%] {\n  font-size: 11px;\n  color: #888;\n  margin: 0;\n  line-height: 1.5;\n}\n\n.settings-textarea[_ngcontent-%COMP%] {\n  width: 100%;\n  min-height: 200px;\n  background: #1e1e1e;\n  border: 1px solid #3c3c3c;\n  border-radius: 4px;\n  color: #ccc;\n  font-family: \"Cascadia Code\", \"Consolas\", \"Courier New\", monospace;\n  font-size: 12px;\n  line-height: 1.5;\n  padding: 12px;\n  resize: vertical;\n  outline: none;\n  box-sizing: border-box;\n}\n.settings-textarea[_ngcontent-%COMP%]:focus {\n  border-color: #ff9800;\n}\n\n\n.session-content[_ngcontent-%COMP%] {\n  flex: 1 1 0;\n  min-height: 0;\n  overflow-y: auto;\n  overflow-x: hidden;\n  padding: 16px 24px;\n  display: flex;\n  flex-direction: column;\n  gap: 12px;\n}\n\n\n.cards-area[_ngcontent-%COMP%] {\n  flex: 0 0 auto;\n  display: flex;\n  flex-direction: column;\n  gap: 16px;\n}\n\n.cards-placeholder[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  height: 200px;\n  color: #555;\n  font-size: 14px;\n  font-style: italic;\n  border: 1px dashed #3c3c3c;\n  border-radius: 8px;\n}\n\n\n.build-overlay-backdrop[_ngcontent-%COMP%] {\n  position: fixed;\n  top: 0;\n  left: 0;\n  right: 0;\n  bottom: 0;\n  background: rgba(0, 0, 0, 0.6);\n  display: flex;\n  align-items: center;\n  justify-content: center;\n  z-index: 1000;\n}\n\n.build-overlay-card[_ngcontent-%COMP%] {\n  background: #252526;\n  border: 1px solid #3c3c3c;\n  border-radius: 8px;\n  width: 80%;\n  max-width: 800px;\n  max-height: 80vh;\n  display: flex;\n  flex-direction: column;\n  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);\n}\n\n.build-overlay-header[_ngcontent-%COMP%] {\n  display: flex;\n  align-items: center;\n  padding: 12px 16px;\n  border-bottom: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.build-overlay-title[_ngcontent-%COMP%] {\n  font-size: 14px;\n  font-weight: 600;\n  color: #fff;\n  flex: 1;\n}\n\n.build-overlay-close[_ngcontent-%COMP%] {\n  background: none;\n  border: none;\n  color: #888;\n  font-size: 16px;\n  cursor: pointer;\n  padding: 4px 8px;\n  border-radius: 3px;\n  transition: all 0.15s;\n}\n.build-overlay-close[_ngcontent-%COMP%]:hover {\n  color: #ef5350;\n  background: rgba(239, 83, 80, 0.1);\n}\n\n.build-overlay-body[_ngcontent-%COMP%] {\n  flex: 1;\n  overflow-y: auto;\n  padding: 16px;\n  min-height: 0;\n}\n\n.build-overlay-pre[_ngcontent-%COMP%] {\n  font-family: \"Cascadia Code\", \"Consolas\", \"Courier New\", monospace;\n  font-size: 12px;\n  line-height: 1.6;\n  color: #ccc;\n  white-space: pre-wrap;\n  word-break: break-word;\n  margin: 0;\n}\n\n.build-overlay-footer[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: flex-end;\n  padding: 12px 16px;\n  border-top: 1px solid #3c3c3c;\n  flex-shrink: 0;\n}\n\n.build-overlay-copy[_ngcontent-%COMP%] {\n  background: #ff9800;\n  color: #000;\n  border: none;\n  padding: 6px 16px;\n  border-radius: 4px;\n  font-size: 12px;\n  font-weight: 600;\n  cursor: pointer;\n  transition: filter 0.15s;\n  min-width: 80px;\n}\n.build-overlay-copy[_ngcontent-%COMP%]:hover {\n  filter: brightness(1.1);\n}\n/*# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8uL3NyYy9hcHAvcHJvbXB0bGFiL2NvbXBvbmVudHMvcHJvbXB0bGFiL3Byb21wdGxhYi5jb21wb25lbnQuc2NzcyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGNBQUE7RUFDQSxZQUFBO0VBQ0EsYUFBQTtFQUNBLFlBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsV0FBQTtFQUNBLG1DQUFBO0VBQ0EsZUFBQTtBQUNGOztBQUVBLHVCQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0EsbUJBQUE7RUFDQSxTQUFBO0VBQ0Esa0JBQUE7RUFDQSxtQkFBQTtFQUNBLGdDQUFBO0VBQ0EsY0FBQTtFQUNBLGVBQUE7QUFDRjs7QUFFQTtFQUNFLGVBQUE7RUFDQSxXQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLGVBQUE7RUFDQSxtQkFBQTtFQUNBLFdBQUE7RUFDQSxnQkFBQTtFQUNBLG1CQUFBO0VBQ0EsZ0JBQUE7QUFDRjs7QUFFQTtFQUNFLE9BQUE7QUFDRjs7QUFFQSxtQkFBQTtBQUNBO0VBQ0UsbUJBQUE7RUFDQSxzQkFBQTtFQUNBLGtCQUFBO0VBQ0EsV0FBQTtFQUNBLGVBQUE7RUFDQSxnQkFBQTtFQUNBLGVBQUE7RUFDQSxhQUFBO0FBQ0Y7QUFDRTtFQUNFLGtCQUFBO0FBQ0o7QUFFRTtFQUNFLG1CQUFBO0VBQ0EsV0FBQTtBQUFKOztBQUlBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtBQURGOztBQUlBO0VBQ0Usb0JBQUE7RUFDQSxtQkFBQTtFQUNBLFFBQUE7RUFDQSxlQUFBO0VBQ0EsV0FBQTtBQURGOztBQUlBO0VBQ0UscUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLHNCQUFBO0VBQ0EseUJBQUE7RUFDQSxrQkFBQTtFQUNBLG9DQUFBO0FBREY7O0FBSUE7RUFDRTtJQUFLLHlCQUFBO0VBQUw7QUFDRjtBQUVBLGdCQUFBO0FBQ0E7RUFDRSxhQUFBO0VBQ0Esc0JB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QUE7RUFDQSxjQUFBO0FBZEY7O0FBaUJBO0VBQ0UsbUJBQUE7RUFDQSxXQUFBO0VBQ0EsWUFBQTtFQUNBLGlCQUFBO0VBQ0Esa0JBQUE7RUFDQSxlQUFBO0VBQ0EsZ0JBQUE7RUFDQSxlQUFBO0VBQ0Esd0JBQUE7RUFDQSxlQUFBO0FBZEY7QUFnQkU7RUFDRSx1QkFBQTtBQWRKIiwic291cmNlc0NvbnRlbnQiOlsiOmhvc3Qge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBmbGV4OiAxIDEgYXV0bztcbiAgaGVpZ2h0OiAxMDAlO1xuICBtaW4taGVpZ2h0OiAwO1xuICBtaW4td2lkdGg6IDA7XG4gIG92ZXJmbG93OiBoaWRkZW47XG4gIGJhY2tncm91bmQ6ICMxZTFlMWU7XG4gIGNvbG9yOiAjY2NjO1xuICBmb250LWZhbWlseTogJ1NlZ29lIFVJJywgc2Fucy1zZXJpZjtcbiAgZm9udC1zaXplOiAxM3B4O1xufVxuXG4vKiBTZXNzaW9uIEhlYWRlciBCYXIgKi9cbi5zZXNzaW9uLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGdhcDogMTBweDtcbiAgcGFkZGluZzogMTBweCAxNnB4O1xuICBiYWNrZ3JvdW5kOiAjMmQyZDJkO1xuICBib3JkZXItYm90dG9tOiAxcHggc29saWQgIzNjM2MzYztcbiAgZmxleC1zaHJpbms6IDA7XG4gIGZsZXgtd3JhcDogd3JhcDtcbn1cblxuLnNlc3Npb24tdGl0bGUge1xuICBmb250LXNpemU6IDE4cHg7XG4gIGNvbG9yOiAjZmZmO1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4uYmFkZ2Uge1xuICBmb250LXNpemU6IDEwcHg7XG4gIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gIGNvbG9yOiAjMDAwO1xuICBwYWRkaW5nOiAycHggOHB4O1xuICBib3JkZXItcmFkaXVzOiAxMHB4O1xuICBmb250LXdlaWdodDogNjAwO1xufVxuXG4uc3BhY2VyIHtcbiAgZmxleDogMTtcbn1cblxuLyogTW9kZWwgc2VsZWN0b3IgKi9cbi5tb2RlbC1zZWxlY3RvciB7XG4gIGJhY2tncm91bmQ6ICMzYzNjM2M7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgY29sb3I6ICNjY2M7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogNHB4IDhweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBvdXRsaW5lOiBub25lO1xuXG4gICY6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgfVxuXG4gIG9wdGlvbiB7XG4gICAgYmFja2dyb3VuZDogIzNjM2MzYztcbiAgICBjb2xvcjogI2NjYztcbiAgfVxufVxuXG4ubW9kZWwtc2VsZWN0b3Itd3JhcHBlciB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xufVxuXG4ubW9kZWwtbG9hZGluZyB7XG4gIGRpc3BsYXk6IGlubGluZS1mbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBnYXA6IDZweDtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBjb2xvcjogIzg4ODtcbn1cblxuLnNwaW5uZXIge1xuICBkaXNwbGF5OiBpbmxpbmUtYmxvY2s7XG4gIHdpZHRoOiAxMnB4O1xuICBoZWlnaHQ6IDEycHg7XG4gIGJvcmRlcjogMnB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci10b3AtY29sb3I6ICNmZjk4MDA7XG4gIGJvcmRlci1yYWRpdXM6IDUwJTtcbiAgYW5pbWF0aW9uOiBzcGluIDAuOHMgbGluZWFyIGluZmluaXRlO1xufVxuXG5Aa2V5ZnJhbWVzIHNwaW4ge1xuICB0byB7IHRyYW5zZm9ybTogcm90YXRlKDM2MGRlZyk7IH1cbn1cblxuLyogTW9kZSB0b2dnbGUgKi9cbi5tb2RlLXRvZ2dsZSB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgb3ZlcmZsb3c6IGhpZGRlbjtcblxuICAubW9kZS1idG4ge1xuICAgIGJhY2tncm91bmQ6IG5vbmU7XG4gICAgYm9yZGVyOiBub25lO1xuICAgIGNvbG9yOiAjODg4O1xuICAgIGZvbnQtc2l6ZTogMTFweDtcbiAgICBwYWRkaW5nOiA0cHggMTJweDtcbiAgICBjdXJzb3I6IHBvaW50ZXI7XG4gICAgdHJhbnNpdGlvbjogYWxsIDAuMTVzO1xuXG4gICAgJi5hY3RpdmUge1xuICAgICAgYmFja2dyb3VuZDogI2ZmOTgwMDtcbiAgICAgIGNvbG9yOiAjMDAwO1xuICAgICAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgICB9XG5cbiAgICAmOm5vdCguYWN0aXZlKTpob3ZlciB7XG4gICAgICBiYWNrZ3JvdW5kOiAjM2MzYzNjO1xuICAgICAgY29sb3I6ICNjY2M7XG4gICAgfVxuICB9XG59XG5cbi8qIFNlcGFyYXRvciAqL1xuLnNlcGFyYXRvciB7XG4gIHdpZHRoOiAxcHg7XG4gIGhlaWdodDogMjRweDtcbiAgYmFja2dyb3VuZDogIzU1NTtcbiAgZmxleC1zaHJpbms6IDA7XG59XG5cbi8qIEFjdGlvbiBidXR0b25zICovXG4uc2Vzc2lvbi1hY3Rpb24tYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICNhYWE7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogNHB4IDEycHg7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB3aGl0ZS1zcGFjZTogbm93cmFwO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJi5hZGQ6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgICBjb2xvcjogI2ZmOTgwMDtcbiAgfVxuXG4gICYuYnVpbGQ6aG92ZXIge1xuICAgIGJvcmRlci1jb2xvcjogIzgyYjFmZjtcbiAgICBjb2xvcjogIzgyYjFmZjtcbiAgfVxuXG4gICYuZXhlYyB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICAgIGZvbnQtd2VpZ2h0OiA2MDA7XG5cbiAgICAmOmhvdmVyOm5vdCg6ZGlzYWJsZWQpIHtcbiAgICAgIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gICAgICBjb2xvcjogIzAwMDtcbiAgICB9XG5cbiAgICAmOmRpc2FibGVkIHtcbiAgICAgIG9wYWNpdHk6IDAuNTtcbiAgICAgIGN1cnNvcjogbm90LWFsbG93ZWQ7XG4gICAgfVxuICB9XG5cbiAgJi5zZXR0aW5ncyB7XG4gICAgZm9udC1zaXplOiAxNHB4O1xuICAgIHBhZGRpbmc6IDRweCA4cHg7XG5cbiAgICAmOmhvdmVyIHtcbiAgICAgIGJvcmRlci1jb2xvcjogIzg4ODtcbiAgICAgIGNvbG9yOiAjY2NjO1xuICAgIH1cbiAgfVxufVxuXG4vKiBNb2RhbCAocmV1c2FibGUgZm9yIFNldHRpbmdzLCBldGMuKSAqL1xuLm1vZGFsLWJhY2tkcm9wIHtcbiAgcG9zaXRpb246IGZpeGVkO1xuICB0b3A6IDA7XG4gIGxlZnQ6IDA7XG4gIHJpZ2h0OiAwO1xuICBib3R0b206IDA7XG4gIGJhY2tncm91bmQ6IHJnYmEoMCwgMCwgMCwgMC42KTtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHotaW5kZXg6IDEwMDA7XG59XG5cbi5tb2RhbC1jYXJkIHtcbiAgYmFja2dyb3VuZDogIzJkMmQyZDtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgYm9yZGVyLXJhZGl1czogNnB4O1xuICBtaW4td2lkdGg6IDQwMHB4O1xuICBtYXgtd2lkdGg6IDYwMHB4O1xuICBtYXgtaGVpZ2h0OiA4MHZoO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBib3gtc2hhZG93OiAwIDhweCAzMnB4IHJnYmEoMCwgMCwgMCwgMC41KTtcblxuICAmLnNldHRpbmdzLW1vZGFsIHtcbiAgICBtaW4td2lkdGg6IDYwMHB4O1xuICAgIG1heC13aWR0aDogODAwcHg7XG4gIH1cbn1cblxuLm1vZGFsLWhlYWRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIHBhZGRpbmc6IDEycHggMTZweDtcbiAgYm9yZGVyLWJvdHRvbTogMXB4IHNvbGlkICMzYzNjM2M7XG59XG5cbi5tb2RhbC10aXRsZSB7XG4gIGZsZXg6IDE7XG4gIGZvbnQtc2l6ZTogMTRweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgY29sb3I6ICNlMGUwZTA7XG59XG5cbi5tb2RhbC1jbG9zZSB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogbm9uZTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTZweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiAycHggNnB4O1xuXG4gICY6aG92ZXIgeyBjb2xvcjogI2ZmZjsgfVxufVxuXG4ubW9kYWwtYm9keSB7XG4gIHBhZGRpbmc6IDIwcHg7XG4gIG92ZXJmbG93LXk6IGF1dG87XG4gIGZsZXg6IDE7XG59XG5cbi5tb2RhbC1mb290ZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBqdXN0aWZ5LWNvbnRlbnQ6IGZsZXgtZW5kO1xuICBnYXA6IDhweDtcbiAgcGFkZGluZzogMTJweCAxNnB4O1xuICBib3JkZXItdG9wOiAxcHggc29saWQgIzNjM2MzYztcbn1cblxuLm1vZGFsLWJ0biB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICM1NTU7XG4gIGNvbG9yOiAjYWFhO1xuICBwYWRkaW5nOiA2cHggMTZweDtcbiAgYm9yZGVyLXJhZGl1czogNHB4O1xuICBmb250LXNpemU6IDEycHg7XG4gIGN1cnNvcjogcG9pbnRlcjtcblxuICAmOmhvdmVyIHtcbiAgICBib3JkZXItY29sb3I6ICM4ODg7XG4gICAgY29sb3I6ICNlMGUwZTA7XG4gIH1cbn1cblxuLnNldHRpbmdzLXNlY3Rpb24ge1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDhweDtcbn1cblxuLnNldHRpbmdzLXNlY3Rpb24taGVhZGVyIHtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBzcGFjZS1iZXR3ZWVuO1xufVxuXG4uc2V0dGluZ3MtbGFiZWwge1xuICBmb250LXNpemU6IDEzcHg7XG4gIGZvbnQtd2VpZ2h0OiA2MDA7XG4gIGNvbG9yOiAjZTBlMGUwO1xufVxuXG4uc2V0dGluZ3MtcmVzZXQtYnRuIHtcbiAgYmFja2dyb3VuZDogbm9uZTtcbiAgYm9yZGVyOiAxcHggc29saWQgIzU1NTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTFweDtcbiAgcGFkZGluZzogM3B4IDEwcHg7XG4gIGJvcmRlci1yYWRpdXM6IDNweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB0cmFuc2l0aW9uOiBhbGwgMC4xNXM7XG5cbiAgJjpob3ZlciB7XG4gICAgYm9yZGVyLWNvbG9yOiAjZmY5ODAwO1xuICAgIGNvbG9yOiAjZmY5ODAwO1xuICB9XG59XG5cbi5zZXR0aW5ncy1oaW50IHtcbiAgZm9udC1zaXplOiAxMXB4O1xuICBjb2xvcjogIzg4ODtcbiAgbWFyZ2luOiAwO1xuICBsaW5lLWhlaWdodDogMS41O1xufVxuXG4uc2V0dGluZ3MtdGV4dGFyZWEge1xuICB3aWR0aDogMTAwJTtcbiAgbWluLWhlaWdodDogMjAwcHg7XG4gIGJhY2tncm91bmQ6ICMxZTFlMWU7XG4gIGJvcmRlcjogMXB4IHNvbGlkICMzYzNjM2M7XG4gIGJvcmRlci1yYWRpdXM6IDRweDtcbiAgY29sb3I6ICNjY2M7XG4gIGZvbnQtZmFtaWx5OiAnQ2FzY2FkaWEgQ29kZScsICdDb25zb2xhcycsICdDb3VyaWVyIE5ldycsIG1vbm9zcGFjZTtcbiAgZm9udC1zaXplOiAxMnB4O1xuICBsaW5lLWhlaWdodDogMS41O1xuICBwYWRkaW5nOiAxMnB4O1xuICByZXNpemU6IHZlcnRpY2FsO1xuICBvdXRsaW5lOiBub25lO1xuICBib3gtc2l6aW5nOiBib3JkZXItYm94O1xuXG4gICY6Zm9jdXMge1xuICAgIGJvcmRlci1jb2xvcjogI2ZmOTgwMDtcbiAgfVxufVxuXG4vKiBTY3JvbGxhYmxlIHNlc3Npb24gY29udGVudCAoZXZlcnl0aGluZyBiZWxvdyB0aGUgaGVhZGVyKSAqL1xuLnNlc3Npb24tY29udGVudCB7XG4gIGZsZXg6IDEgMSAwO1xuICBtaW4taGVpZ2h0OiAwO1xuICBvdmVyZmxvdy15OiBhdXRvO1xuICBvdmVyZmxvdy14OiBoaWRkZW47XG4gIHBhZGRpbmc6IDE2cHggMjRweDtcbiAgZGlzcGxheTogZmxleDtcbiAgZmxleC1kaXJlY3Rpb246IGNvbHVtbjtcbiAgZ2FwOiAxMnB4O1xufVxuXG4vKiBDYXJkcyBhcmVhICovXG4uY2FyZHMtYXJlYSB7XG4gIGZsZXg6IDAgMCBhdXRvO1xuICBkaXNwbGF5OiBmbGV4O1xuICBmbGV4LWRpcmVjdGlvbjogY29sdW1uO1xuICBnYXA6IDE2cHg7XG59XG5cbi5jYXJkcy1wbGFjZWhvbGRlciB7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGFsaWduLWl0ZW1zOiBjZW50ZXI7XG4gIGp1c3RpZnktY29udGVudDogY2VudGVyO1xuICBoZWlnaHQ6IDIwMHB4O1xuICBjb2xvcjogIzU1NTtcbiAgZm9udC1zaXplOiAxNHB4O1xuICBmb250LXN0eWxlOiBpdGFsaWM7XG4gIGJvcmRlcjogMXB4IGRhc2hlZCAjM2MzYzNjO1xuICBib3JkZXItcmFkaXVzOiA4cHg7XG59XG5cbi8qIEJ1aWxkIE91dHB1dCBPdmVybGF5IChUYXNrIDguMSkgKi9cbi5idWlsZC1vdmVybGF5LWJhY2tkcm9wIHtcbiAgcG9zaXRpb246IGZpeGVkO1xuICB0b3A6IDA7XG4gIGxlZnQ6IDA7XG4gIHJpZ2h0OiAwO1xuICBib3R0b206IDA7XG4gIGJhY2tncm91bmQ6IHJnYmEoMCwgMCwgMCwgMC42KTtcbiAgZGlzcGxheTogZmxleDtcbiAgYWxpZ24taXRlbXM6IGNlbnRlcjtcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XG4gIHotaW5kZXg6IDEwMDA7XG59XG5cbi5idWlsZC1vdmVybGF5LWNhcmQge1xuICBiYWNrZ3JvdW5kOiAjMjUyNTI2O1xuICBib3JkZXI6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBib3JkZXItcmFkaXVzOiA4cHg7XG4gIHdpZHRoOiA4MCU7XG4gIG1heC13aWR0aDogODAwcHg7XG4gIG1heC1oZWlnaHQ6IDgwdmg7XG4gIGRpc3BsYXk6IGZsZXg7XG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47XG4gIGJveC1zaGFkb3c6IDAgOHB4IDMycHggcmdiYSgwLCAwLCAwLCAwLjUpO1xufVxuXG4uYnVpbGQtb3ZlcmxheS1oZWFkZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBhbGlnbi1pdGVtczogY2VudGVyO1xuICBwYWRkaW5nOiAxMnB4IDE2cHg7XG4gIGJvcmRlci1ib3R0b206IDFweCBzb2xpZCAjM2MzYzNjO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmJ1aWxkLW92ZXJsYXktdGl0bGUge1xuICBmb250LXNpemU6IDE0cHg7XG4gIGZvbnQtd2VpZ2h0OiA2MDA7XG4gIGNvbG9yOiAjZmZmO1xuICBmbGV4OiAxO1xufVxuXG4uYnVpbGQtb3ZlcmxheS1jbG9zZSB7XG4gIGJhY2tncm91bmQ6IG5vbmU7XG4gIGJvcmRlcjogbm9uZTtcbiAgY29sb3I6ICM4ODg7XG4gIGZvbnQtc2l6ZTogMTZweDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICBwYWRkaW5nOiA0cHggOHB4O1xuICBib3JkZXItcmFkaXVzOiAzcHg7XG4gIHRyYW5zaXRpb246IGFsbCAwLjE1cztcblxuICAmOmhvdmVyIHtcbiAgICBjb2xvcjogI2VmNTM1MDtcbiAgICBiYWNrZ3JvdW5kOiByZ2JhKDIzOSwgODMsIDgwLCAwLjEpO1xuICB9XG59XG5cbi5idWlsZC1vdmVybGF5LWJvZHkge1xuICBmbGV4OiAxO1xuICBvdmVyZmxvdy15OiBhdXRvO1xuICBwYWRkaW5nOiAxNnB4O1xuICBtaW4taGVpZ2h0OiAwO1xufVxuXG4uYnVpbGQtb3ZlcmxheS1wcmUge1xuICBmb250LWZhbWlseTogJ0Nhc2NhZGlhIENvZGUnLCAnQ29uc29sYXMnLCAnQ291cmllciBOZXcnLCBtb25vc3BhY2U7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgbGluZS1oZWlnaHQ6IDEuNjtcbiAgY29sb3I6ICNjY2M7XG4gIHdoaXRlLXNwYWNlOiBwcmUtd3JhcDtcbiAgd29yZC1icmVhazogYnJlYWstd29yZDtcbiAgbWFyZ2luOiAwO1xufVxuXG4uYnVpbGQtb3ZlcmxheS1mb290ZXIge1xuICBkaXNwbGF5OiBmbGV4O1xuICBqdXN0aWZ5LWNvbnRlbnQ6IGZsZXgtZW5kO1xuICBwYWRkaW5nOiAxMnB4IDE2cHg7XG4gIGJvcmRlci10b3A6IDFweCBzb2xpZCAjM2MzYzNjO1xuICBmbGV4LXNocmluazogMDtcbn1cblxuLmJ1aWxkLW92ZXJsYXktY29weSB7XG4gIGJhY2tncm91bmQ6ICNmZjk4MDA7XG4gIGNvbG9yOiAjMDAwO1xuICBib3JkZXI6IG5vbmU7XG4gIHBhZGRpbmc6IDZweCAxNnB4O1xuICBib3JkZXItcmFkaXVzOiA0cHg7XG4gIGZvbnQtc2l6ZTogMTJweDtcbiAgZm9udC13ZWlnaHQ6IDYwMDtcbiAgY3Vyc29yOiBwb2ludGVyO1xuICB0cmFuc2l0aW9uOiBmaWx0ZXIgMC4xNXM7XG4gIG1pbi13aWR0aDogODBweDtcblxuICAmOmhvdmVyIHtcbiAgICBmaWx0ZXI6IGJyaWdodG5lc3MoMS4xKTtcbiAgfVxufVxuIl0sInNvdXJjZVJvb3QiOiIifQ== */"],
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
/* harmony export */   "DEFAULT_SYSTEM_PROMPT": () => (/* binding */ DEFAULT_SYSTEM_PROMPT)
/* harmony export */ });
const DEFAULT_SYSTEM_PROMPT = `You are a **prompt design assistant**. The user is building a structured prompt that will later be sent to an LLM for execution.

Your role:
- Help the user **formulate, refine, and improve** the prompt they are designing.
- When the user describes what the prompt should do, respond with suggestions on how to phrase it, structure it, or improve it.
- Use \`{{paramName}}\` placeholders for variable parts (file paths, directories, configurable values). NEVER substitute concrete values.
- Keep the prompt **generic and reusable** — it must work with any input matching the parameter types.

Critical rules:
- **DO NOT execute the instructions** the user describes. You are designing the prompt, not running it.
- **DO NOT read, list, or access** files, folders, or any real data. The prompt will do that when executed later.
- **DO NOT generate concrete output** (tables, lists, reports). Generate the **instructions** that will produce that output.
- If the user says "read files from a folder and make a table", your job is to write a prompt that says "Read all files in {{sourceDir}} and generate a table with columns: ..." — NOT to actually read files and make the table.

Think of yourself as a ghostwriter: you write the script, someone else performs it.`;

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
      imports: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.CommonModule, _angular_forms__WEBPACK_IMPORTED_MODULE_7__.FormsModule, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_8__.MatLegacyDialogModule, _promptlab_routing_module__WEBPACK_IMPORTED_MODULE_0__.PromptLabRoutingModule]
    });
  }
}
(function () {
  (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_5__["ɵɵsetNgModuleScope"](PromptLabModule, {
    declarations: [_components_promptlab_promptlab_component__WEBPACK_IMPORTED_MODULE_1__.PromptLabComponent, _components_promptlab_doc_panel_promptlab_doc_panel_component__WEBPACK_IMPORTED_MODULE_2__.PromptLabDocPanelComponent, _components_promptlab_agent_card_promptlab_agent_card_component__WEBPACK_IMPORTED_MODULE_3__.PromptLabAgentCardComponent, _components_promptlab_card_promptlab_card_component__WEBPACK_IMPORTED_MODULE_4__.PromptLabCardComponent],
    imports: [_angular_common__WEBPACK_IMPORTED_MODULE_6__.CommonModule, _angular_forms__WEBPACK_IMPORTED_MODULE_7__.FormsModule, _angular_material_legacy_dialog__WEBPACK_IMPORTED_MODULE_8__.MatLegacyDialogModule, _promptlab_routing_module__WEBPACK_IMPORTED_MODULE_0__.PromptLabRoutingModule]
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
- For each parameter, specify its type: "file" (a document to read), "directory" (an output location), or "text" (a free-form value).
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
    // Parse system prompt section (if present)
    const systemPromptResult = this.parseSystemPromptSection(body);
    let remainingBody = systemPromptResult.remaining;
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
      systemPrompt: systemPromptResult.systemPrompt,
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
    lines.push(`created: ${this.formatDate(session.createdAt)}`);
    lines.push(`updated: ${this.formatDate(session.updatedAt)}`);
    lines.push('---');
    lines.push('');
    // System prompt section (only if non-default or explicitly customized)
    if (session.systemPrompt) {
      lines.push('## System Prompt');
      lines.push('');
      lines.push(session.systemPrompt);
      lines.push('');
      lines.push('---');
      lines.push('');
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
  // System Prompt section parsing
  // ---------------------------------------------------------------------------
  parseSystemPromptSection(body) {
    const headingRegex = /^## System Prompt\s*$/m;
    const match = headingRegex.exec(body);
    if (!match) {
      // No system prompt section found — use default
      return {
        systemPrompt: _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT,
        remaining: body
      };
    }
    const afterHeading = body.substring(match.index + match[0].length);
    // Content ends at the first `---` separator or first `## ` heading
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
      systemPrompt: content || _models_promptlab_models__WEBPACK_IMPORTED_MODULE_0__.DEFAULT_SYSTEM_PROMPT,
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
      cards.push({
        id,
        generatedTitle: title,
        parameters,
        distilledPrompt,
        conversation: [],
        lastRun: undefined
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









class PromptLabService {
  constructor(aiChatService, persistenceService, http, serverMessages) {
    this.aiChatService = aiChatService;
    this.persistenceService = persistenceService;
    this.http = http;
    this.serverMessages = serverMessages;
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
      generatedTitle: 'Nuova Card',
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
      return new (t || PromptLabService)(_angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_services_ai_chat_service__WEBPACK_IMPORTED_MODULE_2__.AiChatService), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_promptlab_persistence_service__WEBPACK_IMPORTED_MODULE_3__.PromptLabPersistenceService), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_13__.HttpClient), _angular_core__WEBPACK_IMPORTED_MODULE_12__["ɵɵinject"](_signalR_services_server_messages_service__WEBPACK_IMPORTED_MODULE_4__.MdServerMessagesService));
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