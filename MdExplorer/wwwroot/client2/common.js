(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["common"],{

/***/ "vUCT":
/*!**********************************************************!*\
  !*** ./src/app/md-explorer/services/projects.service.ts ***!
  \**********************************************************/
/*! exports provided: ProjectsService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ProjectsService", function() { return ProjectsService; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "fXoL");
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common/http */ "tk/3");



class ProjectsService {
    constructor(http) {
        this.http = http;
        this.dataStore = { mdProjects: [] };
        this._mdProjects = new rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]([]);
    }
    get mdProjects() {
        return this._mdProjects.asObservable();
    }
    fetchProjects() {
        const url = '../api/MdProjects/GetProjects';
        this.http.get(url)
            .subscribe(data => {
            this.dataStore.mdProjects = data;
            this._mdProjects.next(Object.assign({}, this.dataStore).mdProjects);
        }, error => {
            console.log(error);
        });
    }
    setNewFolderProjectQuickNotes(path, callback, objectThis) {
        const url = '../api/MdProjects/SetFolderProjectQuickNotes';
        this.http.post(url, { path: path }).subscribe(data => {
            callback(data, objectThis);
        });
    }
    setNewFolderProject(path, callback, objectThis) {
        const url = '../api/MdProjects/SetFolderProject';
        this.http.post(url, { path: path }).subscribe(data => {
            callback(data, objectThis);
        });
    }
    deleteProject(project, callback, objectThis) {
        const url = '../api/MdProjects/DeleteProject';
        this.http.post(url, project).subscribe(data => {
            callback(data, objectThis);
        });
    }
}
ProjectsService.ɵfac = function ProjectsService_Factory(t) { return new (t || ProjectsService)(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_2__["HttpClient"])); };
ProjectsService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_1__["ɵɵdefineInjectable"]({ token: ProjectsService, factory: ProjectsService.ɵfac, providedIn: 'root' });


/***/ }),

/***/ "xmhS":
/*!*********************************************************!*\
  !*** ./src/app/md-explorer/services/md-file.service.ts ***!
  \*********************************************************/
/*! exports provided: MdFileService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdFileService", function() { return MdFileService; });
/* harmony import */ var _angular_common_http__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common/http */ "tk/3");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "qCKp");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "fXoL");




class MdFileService {
    constructor(http) {
        this.http = http;
        this.dataStore = { mdFiles: [], mdFoldersDocument: [], mdDynFolderDocument: [] };
        this._mdFiles = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._mdFoldersDocument = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
        this._mdDynFolderDocument = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"]([]);
    }
    get mdFiles() {
        return this._mdFiles.asObservable();
    }
    get mdFoldersDocument() {
        return this._mdFoldersDocument.asObservable();
    }
    get mdDynFolderDocument() {
        return this._mdDynFolderDocument.asObservable();
    }
    loadAll(callback, objectThis) {
        const url = '../api/mdfiles/GetAllMdFiles';
        return this.http.get(url)
            .subscribe(data => {
            this.dataStore.mdFiles = data;
            this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
            if (callback != null) {
                callback(data, objectThis);
            }
        }, error => {
            console.log("failed to fetch mdfile list");
        });
    }
    loadFolders() {
        const url = '../api/mdfiles/GetFoldersDocument';
        return this.http.get(url)
            .subscribe(data => {
            this.dataStore.mdFoldersDocument = data;
            this._mdFoldersDocument.next(Object.assign({}, this.dataStore).mdFoldersDocument);
        }, error => {
            console.log("failed to fetch mdfile list");
        });
    }
    loadDynFolders(path, level) {
        const url = '../api/mdfiles/GetDynFoldersDocument';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('path', path).set('level', String(level));
        return this.http.get(url, { params })
            .subscribe(data => {
            if (this.dataStore.mdDynFolderDocument.length > 0) {
                var test = this.dataStore.mdDynFolderDocument.find(_ => _.path == path);
                test.children = data;
            }
            else {
                this.dataStore.mdDynFolderDocument = data;
            }
            this._mdDynFolderDocument.next(Object.assign({}, this.dataStore).mdDynFolderDocument);
        }, error => {
            console.log("failed to fetch mdfile list");
        });
    }
    loadDocumentFolder(path, level) {
        const url = '../api/mdfiles/GetDynFoldersDocument';
        var params = new _angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpParams"]().set('path', path).set('level', String(level));
        return this.http.get(url, { params });
    }
    GetHtml(path) {
        const url = '../api/mdexplorer/' + path;
        return this.http.get(url, { responseType: 'text' }); //, currentFile
    }
}
MdFileService.ɵfac = function MdFileService_Factory(t) { return new (t || MdFileService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"])); };
MdFileService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: MdFileService, factory: MdFileService.ɵfac, providedIn: 'root' });


/***/ })

}]);
//# sourceMappingURL=common.js.map