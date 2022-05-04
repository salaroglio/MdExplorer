(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["common"],{

/***/ "aS6m":
/*!***********************************************!*\
  !*** ./src/app/md-explorer/models/md-file.ts ***!
  \***********************************************/
/*! exports provided: MdFile */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MdFile", function() { return MdFile; });
class MdFile {
    constructor(name, path, level, expandable) {
        this.name = name;
        this.path = path;
        this.level = level;
        this.expandable = expandable;
    }
}


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
        this.foundMd = false;
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
    changeDataStoreMdFiles(oldFile, newFile) {
        debugger;
        this.exploreMdFiles(this.dataStore.mdFiles, oldFile, newFile);
        this._mdFiles.next(Object.assign({}, this.dataStore).mdFiles);
    }
    exploreMdFiles(arrayMd, oldFile, newFile) {
        if (arrayMd.length == 0) {
            return;
        }
        var thatFile = arrayMd.find(_ => _.fullPath == oldFile.fullPath);
        if (thatFile == undefined) {
            arrayMd.map(_ => {
                if (!this.foundMd) {
                    this.exploreMdFiles(_.childrens, oldFile, newFile);
                }
            });
        }
        else {
            debugger;
            this.foundMd = true;
            thatFile.name = newFile.name;
            thatFile.path = newFile.path;
        }
    }
}
MdFileService.ɵfac = function MdFileService_Factory(t) { return new (t || MdFileService)(_angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵinject"](_angular_common_http__WEBPACK_IMPORTED_MODULE_0__["HttpClient"])); };
MdFileService.ɵprov = _angular_core__WEBPACK_IMPORTED_MODULE_2__["ɵɵdefineInjectable"]({ token: MdFileService, factory: MdFileService.ɵfac, providedIn: 'root' });


/***/ })

}]);
//# sourceMappingURL=common.js.map