(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["projects-projects-module"],{

/***/ "Bw61":
/*!***************************************************************************!*\
  !*** ./src/app/projects/dialogs/clone-project/clone-project.component.ts ***!
  \***************************************************************************/
/*! exports provided: CloneProjectComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CloneProjectComponent", function() { return CloneProjectComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");

class CloneProjectComponent {
    constructor() { }
    ngOnInit() {
    }
}
CloneProjectComponent.ɵfac = function CloneProjectComponent_Factory(t) { return new (t || CloneProjectComponent)(); };
CloneProjectComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: CloneProjectComponent, selectors: [["app-clone-project"]], decls: 2, vars: 0, template: function CloneProjectComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "p");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, "clone-project works!");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    } }, styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJjbG9uZS1wcm9qZWN0LmNvbXBvbmVudC5zY3NzIn0= */"] });


/***/ }),

/***/ "PK6v":
/*!***************************************************************!*\
  !*** ./src/app/projects/open-recent/open-recent.component.ts ***!
  \***************************************************************/
/*! exports provided: OpenRecentComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "OpenRecentComponent", function() { return OpenRecentComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../md-explorer/services/md-file.service */ "xmhS");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");








function OpenRecentComponent_mat_card_2_Template(rf, ctx) { if (rf & 1) {
    const _r3 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵgetCurrentView"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "mat-card");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](1, "mat-card-title");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](3, "mat-card-subtitle");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](5, "mat-card-actions");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](6, "button", 1);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_6_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r3); const element_r1 = ctx.$implicit; const ctx_r2 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](); return ctx_r2.quickOpenNotes(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelement"](7, "img", 2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](8, " Quick notes ");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](9, "button", 3);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_9_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r3); const element_r1 = ctx.$implicit; const ctx_r4 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](); return ctx_r4.openNewProject(element_r1.path); });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](10, "Open");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](11, "button", 4);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵlistener"]("click", function OpenRecentComponent_mat_card_2_Template_button_click_11_listener() { _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵrestoreView"](_r3); const element_r1 = ctx.$implicit; const ctx_r5 = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵnextContext"](); return ctx_r5.deleteProject(element_r1); });
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](12, "mat-icon");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](13, "delete");
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
} if (rf & 2) {
    const element_r1 = ctx.$implicit;
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](element_r1.name);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
    _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtextInterpolate"](element_r1.path);
} }
class OpenRecentComponent {
    constructor(projectService, mdFileService, router, route) {
        this.projectService = projectService;
        this.mdFileService = mdFileService;
        this.router = router;
        this.route = route;
    }
    ngOnInit() {
        this.projectService.fetchProjects();
        this.dataSource = this.projectService.mdProjects;
    }
    quickOpenNotes(path) {
        this.projectService.setNewFolderProjectQuickNotes(path).subscribe(_ => {
            var dateTime = new Date();
            this.router.navigate(['/main/navigation', dateTime.getTime()]);
        });
    }
    openNewProject(path) {
        this.projectService.setNewFolderProject(path).subscribe(_ => {
            var dateTime = new Date();
            this.router.navigate(['/main/navigation', dateTime.getTime()]);
        });
    }
    getProjectList(data, objectThis) {
        objectThis.projectService.fetchProjects();
    }
    ;
    deleteProject(project) {
        this.projectService.deleteProject(project, this.getProjectList, this);
    }
}
OpenRecentComponent.ɵfac = function OpenRecentComponent_Factory(t) { return new (t || OpenRecentComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_1__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_md_explorer_services_md_file_service__WEBPACK_IMPORTED_MODULE_2__["MdFileService"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_3__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_3__["ActivatedRoute"])); };
OpenRecentComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵdefineComponent"]({ type: OpenRecentComponent, selectors: [["app-open-recent"]], decls: 4, vars: 3, consts: [[4, "ngFor", "ngForOf"], ["mat-stroked-button", "", "color", "accent", 3, "click"], ["src", "/assets/run.png"], ["mat-stroked-button", "", "color", "primary", 3, "click"], ["mat-raised-button", "", "color", "warn", 3, "click"]], template: function OpenRecentComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementStart"](0, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtext"](1, "Recent Projects");
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵtemplate"](2, OpenRecentComponent_mat_card_2_Template, 14, 2, "mat-card", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipe"](3, "async");
    } if (rf & 2) {
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵadvance"](2);
        _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵproperty"]("ngForOf", _angular_core__WEBPACK_IMPORTED_MODULE_0__["ɵɵpipeBind1"](3, 1, ctx.dataSource));
    } }, directives: [_angular_common__WEBPACK_IMPORTED_MODULE_4__["NgForOf"], _angular_material_card__WEBPACK_IMPORTED_MODULE_5__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_5__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_5__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_5__["MatCardActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_6__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_7__["MatIcon"]], pipes: [_angular_common__WEBPACK_IMPORTED_MODULE_4__["AsyncPipe"]], styles: ["\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IiIsImZpbGUiOiJvcGVuLXJlY2VudC5jb21wb25lbnQuc2NzcyJ9 */"] });


/***/ }),

/***/ "Wm2z":
/*!*********************************************!*\
  !*** ./src/app/projects/projects.module.ts ***!
  \*********************************************/
/*! exports provided: ProjectsModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsModule", function() { return ProjectsModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "ofXK");
/* harmony import */ var _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./open-recent/open-recent.component */ "PK6v");
/* harmony import */ var _projects_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./projects.component */ "zUkc");
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./new-project/new-project.component */ "5Zmz");
/* harmony import */ var _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./dialogs/clone-project/clone-project.component */ "Bw61");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _shared_material_module__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../shared/material.module */ "5dmV");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/core */ "fXoL");









const routes = [
    {
        path: '', component: _projects_component__WEBPACK_IMPORTED_MODULE_2__["ProjectsComponent"],
        children: [
            { path: '', redirectTo: 'openrecent', pathMatch: 'full' },
            { path: 'openrecent', component: _open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__["OpenRecentComponent"] },
            { path: 'newproject', component: _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__["NewProjectComponent"] },
        ]
    }
];
class ProjectsModule {
    constructor() {
        console.log('constructor ProjectsModule');
    }
}
ProjectsModule.ɵfac = function ProjectsModule_Factory(t) { return new (t || ProjectsModule)(); };
ProjectsModule.ɵmod = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineNgModule"]({ type: ProjectsModule });
ProjectsModule.ɵinj = _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵdefineInjector"]({ imports: [[
            _angular_router__WEBPACK_IMPORTED_MODULE_5__["RouterModule"].forChild(routes),
            _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
            _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"],
        ]] });
(function () { (typeof ngJitMode === "undefined" || ngJitMode) && _angular_core__WEBPACK_IMPORTED_MODULE_7__["ɵɵsetNgModuleScope"](ProjectsModule, { declarations: [_open_recent_open_recent_component__WEBPACK_IMPORTED_MODULE_1__["OpenRecentComponent"],
        _projects_component__WEBPACK_IMPORTED_MODULE_2__["ProjectsComponent"],
        _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_3__["NewProjectComponent"],
        _dialogs_clone_project_clone_project_component__WEBPACK_IMPORTED_MODULE_4__["CloneProjectComponent"]], imports: [_angular_router__WEBPACK_IMPORTED_MODULE_5__["RouterModule"], _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
        _shared_material_module__WEBPACK_IMPORTED_MODULE_6__["MaterialModule"]] }); })();


/***/ }),

/***/ "zUkc":
/*!************************************************!*\
  !*** ./src/app/projects/projects.component.ts ***!
  \************************************************/
/*! exports provided: ProjectsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsComponent", function() { return ProjectsComponent; });
/* harmony import */ var _new_project_new_project_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./new-project/new-project.component */ "5Zmz");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../md-explorer/services/projects.service */ "vUCT");
/* harmony import */ var _angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! @angular/material/dialog */ "0IaG");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! @angular/router */ "tyNb");
/* harmony import */ var _signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../signalr/services/server-messages.service */ "jQFx");
/* harmony import */ var _angular_material_card__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @angular/material/card */ "Wp6s");
/* harmony import */ var _angular_material_button__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @angular/material/button */ "bTqV");
/* harmony import */ var _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! @angular/material/icon */ "NFeN");









class ProjectsComponent {
    constructor(projectService, dialog, router, signalRService) {
        this.projectService = projectService;
        this.dialog = dialog;
        this.router = router;
        this.signalRService = signalRService;
        this.dataSource1 = [{ name: 'Nome progetto', path: 'c:\folder\folder\folder' }];
    }
    ngOnInit() {
    }
    openRecent() {
        this.router.navigate(['/projects/openrecent']);
    }
    openNewFolder() {
        const dialogRef = this.dialog.open(_new_project_new_project_component__WEBPACK_IMPORTED_MODULE_0__["NewProjectComponent"], {
            width: '600px',
            data: null
        });
    }
}
ProjectsComponent.ɵfac = function ProjectsComponent_Factory(t) { return new (t || ProjectsComponent)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_md_explorer_services_projects_service__WEBPACK_IMPORTED_MODULE_2__["ProjectsService"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_material_dialog__WEBPACK_IMPORTED_MODULE_3__["MatDialog"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_angular_router__WEBPACK_IMPORTED_MODULE_4__["Router"]), _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdirectiveInject"](_signalr_services_server_messages_service__WEBPACK_IMPORTED_MODULE_5__["ServerMessagesService"])); };
ProjectsComponent.ɵcmp = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineComponent"]({ type: ProjectsComponent, selectors: [["app-projects"]], decls: 32, vars: 0, consts: [[1, "container-flex"], [1, "container-title"], [1, "item"], [1, "container-columns"], [1, "container-first-column"], ["mat-stroked-button", "", "color", "primary"], ["mat-stroked-button", "", "color", "primary", 3, "click"]], template: function ProjectsComponent_Template(rf, ctx) { if (rf & 1) {
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](0, "div", 0);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](1, "div", 1);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](2, "div", 2);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](3, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](4, "strong");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](5, "Markdown Explorer");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](6, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](7, "div", 3);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](8, "div", 4);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelement"](9, "router-outlet");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](10, "div");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](11, "h1");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](12, "Get started");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](13, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](14, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](15, "Clone a repository");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](16, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](17, "Get code from an online repository like GitLab");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](18, "mat-card-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](19, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](20, "Clone");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](21, "button", 5);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](22, "mat-icon");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](23, "build");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](24, "mat-card");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](25, "mat-card-title");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](26, "Create new project");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](27, "mat-card-subtitle");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](28, "Choose filesystem folder to get started");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](29, "mat-card-actions");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementStart"](30, "button", 6);
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵlistener"]("click", function ProjectsComponent_Template_button_click_30_listener() { return ctx.openNewFolder(); });
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵtext"](31, "Create");
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
        _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵelementEnd"]();
    } }, directives: [_angular_router__WEBPACK_IMPORTED_MODULE_4__["RouterOutlet"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCard"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardTitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardSubtitle"], _angular_material_card__WEBPACK_IMPORTED_MODULE_6__["MatCardActions"], _angular_material_button__WEBPACK_IMPORTED_MODULE_7__["MatButton"], _angular_material_icon__WEBPACK_IMPORTED_MODULE_8__["MatIcon"]], styles: [".container-flex[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  align-content: center;\n}\n\n.container-title[_ngcontent-%COMP%] {\n  display: flex;\n  justify-content: center;\n}\n\n.container-columns[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: row;\n  justify-content: center;\n}\n\n.container-first-column[_ngcontent-%COMP%] {\n  display: flex;\n  flex-direction: column;\n  row-gap: 10px;\n  column-gap: 20px;\n}\n/*# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIi4uXFwuLlxcLi5cXHByb2plY3RzLmNvbXBvbmVudC5zY3NzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiJBQUFBO0VBQ0UsYUFBQTtFQUNBLHNCQUFBO0VBQ0EscUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSx1QkFBQTtBQUNGOztBQUVBO0VBQ0UsYUFBQTtFQUNBLG1CQUFBO0VBQ0EsdUJBQUE7QUFDRjs7QUFFQTtFQUNFLGFBQUE7RUFDQSxzQkFBQTtFQUNBLGFBQUE7RUFDQSxnQkFBQTtBQUNGIiwiZmlsZSI6InByb2plY3RzLmNvbXBvbmVudC5zY3NzIiwic291cmNlc0NvbnRlbnQiOlsiLmNvbnRhaW5lci1mbGV4IHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47ICBcclxuICBhbGlnbi1jb250ZW50OmNlbnRlcjtcclxufVxyXG5cclxuLmNvbnRhaW5lci10aXRsZSB7XHJcbiAgZGlzcGxheTogZmxleDtcclxuICBqdXN0aWZ5LWNvbnRlbnQ6IGNlbnRlcjtcclxufVxyXG5cclxuLmNvbnRhaW5lci1jb2x1bW5zIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiByb3c7XHJcbiAganVzdGlmeS1jb250ZW50OiBjZW50ZXI7XHJcbn1cclxuXHJcbi5jb250YWluZXItZmlyc3QtY29sdW1uIHtcclxuICBkaXNwbGF5OiBmbGV4O1xyXG4gIGZsZXgtZGlyZWN0aW9uOiBjb2x1bW47ICAgIFxyXG4gIHJvdy1nYXA6IDEwcHg7XHJcbiAgY29sdW1uLWdhcDogMjBweDtcclxufVxyXG4iXX0= */"] });


/***/ })

}]);
//# sourceMappingURL=projects-projects-module.js.map