// EDITOR H1

editorH1CurrentIndex = 0;

function editH1(e) {    
    //debugger;    
    editorH1CurrentIndex = e;
    var test$ = $('div.hiddendataforeditorh1[md-itemmatchindex=' + editorH1CurrentIndex + ']'); 
    $(".edith1-popup-overlay, .popup-content").addClass("active");        
    $('#editH1').val(test$[0].innerText);
    var canvas$ = $('#canvas');
    var toc$ = $('#toc');
    canvas$.addClass("hidecanvas");
    toc$.addClass("hidetoc");   
    $('#editH1').highlightWithinTextarea('update');
    if ($('#TOC').is(":hidden")) {
    } else {
        tocWasShown = true;
    }
    hideTocForEditH1();
}

tocWasShown = false; 
function hideTocForEditH1() {
    if ($('#TOC').is(":hidden")) {        
        if (tocWasShown) {
            var $page = $('#page');
            $page.attr('class', 'col-9');
            setTimeout(function () {
                var $toc = $('#TOC');
                $toc.attr('class', 'col-3');
                $toc.show();

            }, 500);
        }
    } else {        
        var $toc = $('#TOC');
        $toc.hide();
        $toc.removeAttr('class');
        var $page = $('#page');
        $page.attr('class', 'col-12');        
    }
}

// configuration of editH1
$(function () {    
    var editorH1$ = $('#editH1');
    editorH1$.highlightWithinTextarea({
        highlight: [
            {
                highlight: '#',
                className: 'red'
            },
            {
                highlight: ':ok:',
                className: 'blue'
            },
            {
                highlight: ':exclamation:',
                className: 'red'
            },
            {
                highlight: '-',
                className: 'yellow'
            }
        ]
    });



    var closeButton$ = $(".close, .popup-overlay");

    closeButton$.on("click", function () {
        $(".edith1-popup-overlay, .popup-content").removeClass("active");
        $('#canvas').removeClass("hidecanvas");
        $('#toc').removeClass("hidetoc");
        hideTocForEditH1();
    });

    var operButton$ = $(".save, .popup-overlay");

    operButton$.on("click", function () {

        var oldData$ = $('div.hiddendataforeditorh1[md-itemmatchindex=' + editorH1CurrentIndex + ']');
        var textArea$ = $('#editH1');   
        var oldMd = oldData$[0].innerText;
        var newMd = textArea$.val();
        var pathFile = oldData$.attr("md-path-file");
        var indexStart = parseInt(oldData$.attr("md-itemmatchindex"));
        var indexEnd = parseInt(oldData$.attr("md-itemmatchindex-end"));
        

        let toStringify = { oldMd: oldMd, newMd: newMd, pathFile: pathFile, indexStart: indexStart, indexEnd: indexEnd };
        $.ajax({
            url: "/api/WriteMD/SetEditorH1",
            type: "POST",
            data: JSON.stringify(toStringify),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                console.log(data);
            }
        });

        $(".edith1-popup-overlay, .popup-content").removeClass("active");
        $('#canvas').removeClass("hidecanvas");
        $('#toc').removeClass("hidetoc");
        hideTocForEditH1();
    });


});

editorIsShown = false;
function toggleEditor() {
    var arrayOfEditorH1$ = $(".editorH1");
    if (!editorIsShown) {
        //Set click for editorH1        
        arrayOfEditorH1$.on("click", function () {
            let currentTag$ = $(this);
            let index = currentTag$.attr("md-itemmatchindex");
            editH1(index);
        });
        arrayOfEditorH1$.addClass("showAreaH1");
    } else {
        var arrayOfEditorH1$ = $(".editorH1");
        arrayOfEditorH1$.off("click");
        arrayOfEditorH1$.removeClass("showAreaH1");
    }
    editorIsShown = !editorIsShown;
}

/////// end editH1

const cyrb53 = function (str, seed = 0) {
    let h1 = 0xdeadbeef ^ seed, h2 = 0x41c6ce57 ^ seed;
    for (let i = 0, ch; i < str.length; i++) {
        ch = str.charCodeAt(i);
        h1 = Math.imul(h1 ^ ch, 2654435761);
        h2 = Math.imul(h2 ^ ch, 1597334677);
    }
    h1 = Math.imul(h1 ^ (h1 >>> 16), 2246822507) ^ Math.imul(h2 ^ (h2 >>> 13), 3266489909);
    h2 = Math.imul(h2 ^ (h2 >>> 16), 2246822507) ^ Math.imul(h1 ^ (h1 >>> 13), 3266489909);
    return 4294967296 * (2097151 & h2) + (h1 >>> 0);
};


//Open link directly in the application
function openApplication(fullpath) {
    
    let toStringify = { fullPath: fullpath };
    $.ajax({
        url: "/api/MdFiles/OpenFileInApplication",
        type: "POST",
        data: JSON.stringify(toStringify),//'{"linkHash": "1234", "cssHash": "5678", "Width": "100px", "Height": "50px","ClientX":"","ClientY":"" }', //
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            console.log(data);            
        }
    });

}

// gestione tocbot
$(function () {
    tocbot.init({
        tocSelector: '.js-toc',
        orderedList: true,
        hasInnerContainers: true,
        scrollSmooth: true,
        headingSelector: 'h1, h2, h3, h4, h5, h6',
        // Smooth scroll duration.
        scrollSmoothDuration: 220,
        positionFixedClass: 'is-position-fixed',

    });
    setTimeout(tocbot.refresh());
    //console.log('Tocbot initialized');
});

// array di tutti gli tooltip con emoji
let tippyDictPriority = {};
let tippyDictProcess = {};
// gestione tippy (tooltip), modo sbrigativo di associarli
$(function () {

    tippyDictPriority = tippy('[id*="Priority"][data-tippy-content]', {
        placement: 'left',
    });
    tippyDictProcess = tippy('[id*="Process"][data-tippy-content]', {
        placement: 'left'
    });

    tippyDictProcess.forEach(_ => {
        let tippyReferenceProcess = _.reference;
        tippyReferenceProcess.setAttribute('data-tippy-process-id', tippyDictProcess.indexOf(_));
        setTippyTypeProcess(tippyReferenceProcess, _);
    });

    tippyDictPriority.forEach(_ => {
        let tippyReferencePriority = _.reference;
        tippyReferencePriority.setAttribute('data-tippy-priority-id', tippyDictPriority.indexOf(_));
        setTippyTypePriority(tippyReferencePriority, _);
    });
});



function setTippyTypePriority(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'urgente') {
        _.setProps({ theme: 'priorityUrgente' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'annullata') {
        _.setProps({ theme: 'priorityAnnullato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'fermata') {
        _.setProps({ theme: 'priorityFermata' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'conclusa') {
        _.setProps({ theme: 'priorityConclusa' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'dubbio urgente') {
        _.setProps({ theme: 'priorityDaCapireUrgentemente' });
    }

    if (tippyReference.getAttribute('data-tippy-content') == 'da valutare') {
        _.setProps({ theme: 'priorityDaValutare' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'obbligatorio') {
        _.setProps({ theme: 'priorityObbligatorio' });
    }

    

}


function setTippyTypeProcess(tippyReference, _) {
    if (tippyReference.getAttribute('data-tippy-content') == 'approvato') {
        _.setProps({ theme: 'processok' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'work in progress') {
        _.setProps({ theme: 'processWIP' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'completato') {
        _.setProps({ theme: 'processCompletato' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'Info') {
        _.setProps({ theme: 'processInfo' });
    }
    if (tippyReference.getAttribute('data-tippy-content') == 'attenzione') {
        _.setProps({ theme: 'processAttenzione' });
    }

    
}


// Manage Images

// function to manage readability 
var arrayReadabilityToggle = [];
function toggleSeeMe(stringMatchedHash) {

    var $box = $('#' + stringMatchedHash);
    var buttonPressed = arrayReadabilityToggle.find(data => data.id == $box.id);
    if (buttonPressed == undefined) {
        var dataToStore = { id: $box.id, style: $box.attr('style') };

        $box.attr('style', 'width:100%;height:100%;');
        arrayReadabilityToggle.push(dataToStore);
    }
    else {
        $box.attr('style', buttonPressed.style);
        var currentIndex = arrayReadabilityToggle.findIndex(data => data.id == $box.id);
        arrayReadabilityToggle.splice(currentIndex, 1);
    }
}

// function to show/hide image's toolbar

function showImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    var divStyle = getComputedStyle($element[0]);
    var rect = $element[0].getBoundingClientRect();
    var test = rect.top;
    $element.attr("style", "display:block; position:absolute;");
}

function hideImageToolbar(referenceId) {
    var $element = $('#' + referenceId);
    $element.attr("style", "display:none;");
}


//Function Called by button to activate Move!
var arrayLinksMoveToggle = [];
var moving = false;
var image;

function activateMove(currentObject, linkHash, referenceId) {
    var toSend = currentObject.parentElement.parentElement;
    $movable = $(toSend);
    var buttonPressed = arrayLinksMoveToggle.find(data => data == linkHash);
    if (buttonPressed == undefined) {
        var newClass = $movable.attr('class', 'movable');
        arrayLinksMoveToggle.push(linkHash);
    } else {

        var currentIndex = arrayLinksMoveToggle.findIndex(data => data == linkHash);
        arrayLinksMoveToggle.splice(currentIndex, 1);
        var possibleMatch = currentObject.parentElement.nextSibling;
        resizeImage(possibleMatch);
        $movable.attr('class', 'movedAndFixed');

    }

    initialClick(toSend, referenceId);
}

function initialClick(currentObject, referenceId) {

    if (moving) {
        document.removeEventListener("mousemove", move);
        moving = !moving;
        return;
    }

    moving = !moving;
    image = currentObject;

    document.addEventListener("mousemove", move, false);

}

function move(e) {

    var newX = e.clientX - 76;
    var newY = e.clientY - 18;

    image.style.left = newX + "px";
    image.style.top = newY + "px";


}


// function called by button to activate resize
var arrayLinksResizeToggle = [];
function activateResize(linkHash) {
    // Find nodes    
    var buttonPressed = arrayLinksResizeToggle.find(data => data == linkHash);
    var $links = $('div[md-link-hash=' + linkHash + ']'); // shold exist only one link. I'm using each() because i'm lazy :-)
    $links.each(function (index) {
        if (buttonPressed == undefined) {
            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue + ' resizable';
            arrayLinksResizeToggle.push(linkHash);
        }
        else {


            var oldValue = $links[index].attributes['class'].value;
            $links[index].attributes['class'].value = oldValue.replace(' resizable', '');
            var currentIndex = arrayLinksResizeToggle.findIndex(data => data == linkHash);
            arrayLinksResizeToggle.splice(currentIndex, 1);

        }

    });
}

var cumulativeOffset = function (element) {
    var top = 0, left = 0;
    do {
        top += element.offsetTop || 0;
        left += element.offsetLeft || 0;
        element = element.offsetParent;
    } while (element);

    return {
        top: top,
        left: left
    };
};



// Function called by onMouseUp event to 
// write down on MD the new image dimension values.
function resizeImage(currentDiv) {

    // going inside the div
    var img = currentDiv.childNodes[0].childNodes[0];
    var divStyle = getComputedStyle(img.parentElement.parentElement.parentElement);
    var position = divStyle.position;// == "" ? "none" : img.style.position;
    // getting infos from attributes

    var currentHash = currentDiv.attributes['md-css-hash'].value;
    var pathFile = currentDiv.attributes['md-path-file'].value;
    var linkHash = currentDiv.attributes['md-link-hash'].value;
    var CurrentQueryRequest = currentDiv.attributes['md-CurrentQueryRequest'].value;

    var currentImageData = {
        pathFile: pathFile,
        linkHash: linkHash,
        cssHash: currentHash,
        Width: currentDiv.clientWidth,
        Height: currentDiv.scrollHeight,
        ClientX: cumulativeOffset(currentDiv).left,
        ClientY: cumulativeOffset(currentDiv).top,
        Position: position,
        CurrentQueryRequest: CurrentQueryRequest
    };
    $.ajax({
        url: "/api/WriteMD/SaveImgPositionAndSize",
        type: "POST",
        data: JSON.stringify(currentImageData),//'{"linkHash": "1234", "cssHash": "5678", "Width": "100px", "Height": "50px","ClientX":"","ClientY":"" }', //
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
            currentDiv.attributes['md-css-hash'].value = data.cssHash;
            var $divs = $("div[md-css-hash='" + currentHash + "']");
            $divs.each(function (index) {
                $divs[index].attributes['md-css-hash'].value = data.cssHash;
            });
            //.attributes['md-css-hash'].value = data.cssHash
        }
    });
}



// gestione del floppy_disk e salvataggio
function activateSaveCopy(el, path) {
    $.get("/api/WriteMD/ActivateSaveCopy?pathFile=" + path, function () {
        alert('done');
    });
}

// gestione del sortable dentro le icone di priorità
$(function () {
    $(".sortable").sortable();
    $(".sortable").disableSelection();

    $(".sortable").on("sortstop", function (event, ui) {
        // controllo che l'item selezionato abbia il simbolo di priorità
        var previousNodeIndex = null;
        var nextNodeIndex = null;
        var currentNodeIndex = null;

        // nodo corrente
        currentNode = ui.item[0].childNodes[0];
        currentNodeIndex = currentNode.attributes['data-md-card-index'].value;
        currentTableGameIndex = currentNode.attributes['data-md-table-game-index'].value;

        // nodo precedente
        if (ui.item[0].previousElementSibling != null) {
            previousNode = ui.item[0].previousElementSibling.childNodes[0];
            previousNodeIndex = previousNode.attributes['data-md-card-index'].value;
        }
        // nodo successivo
        if (ui.item[0].nextElementSibling != null) {
            nextNode = ui.item[0].nextSibling.childNodes[0];
            nextNodeIndex = nextNode.attributes['data-md-card-index'].value;
        }

        var currentPathFile = ui.item[0].childNodes[0].attributes['data-md-pathfile'].value;
        var queryPreviousNodeIndex = previousNodeIndex == null ? '' : previousNodeIndex;
        var queryNextNodeIndex = nextNodeIndex == null ? '' : nextNodeIndex;
        $.get("/api/WriteMD/SetEmojiOrderPriority?currentNodeIndex=" + currentNodeIndex +
            "&previousNodeIndex=" + queryPreviousNodeIndex +
            "&nextNodeIndex=" + queryNextNodeIndex +
            "&pathFile=" + currentPathFile +
            "&tableGameIndex=" + currentTableGameIndex,
            function (data) {
                currentNode.attributes['data-md-card-index'].value = data.currentNodeIndex;
                if (data.previousNodeIndex != null) {
                    previousNode.attributes['data-md-card-index'].value = data.previousNodeIndex;
                }
                if (data.nextNodeIndex != null) {
                    nextNode.attributes['data-md-card-index'].value = data.nextNodeIndex;
                }
                // reconstruct process IDs
                var listProcess = $("span[id*='emojiProcess']");
                listProcess.each(function (index, element) {
                    element.attributes['data-md-process-index'].value = index;// listProcess.index(element);
                });
                var listPriority = $("span[id*='mojiPriority']");
                listPriority.each(function (index, element) {
                    element.attributes['data-md-priority-index'].value = index;// listProcess.index(element);
                });
                // Reconstruct index of table-card into table-game, foreach table-game
                var iTable = 0;
                var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                while (listPriorityItableGame.length > 0) {
                    listPriorityItableGame.each(function (index, element) {
                        element.attributes['data-md-card-index'].value = index;// listProcess.index(element);
                    });
                    iTable++;
                    var listPriorityItableGame = $("span[id*='mojiPriority'][data-md-table-game-index=" + iTable.toString() + "]");
                }




                console.log(data);
            });
        console.log('sortstop parents Event = ', event, '  ui = ', ui);
        console.log(ui.item);

        //do sort of parents
    });
});
// serve per impostare il date-picker di boostrap ed evitare quello di jquery-ui
$.fn.datepicker.noConflict = function () {
    $.fn.datepicker = old;
    return this;
};

// funzione che memorizza l'ultima posizione della pagina
document.addEventListener("DOMContentLoaded", function (event) {
    // Memorizza la posizione corrente della pagina,perché sia riproposta dopo un refresh    
    var test3 = document.location.href;
    var position = test3.indexOf('?');
    var position2 = test3.substring(0, position);
    var test2 = cyrb53(position2);
    var scrollpos1 = localStorage.getItem(test2);
    if (scrollpos1) window.scrollTo(0, scrollpos1);

});

// gestione ultima posizione dello scroll
window.onbeforeunload = function (e) {
    var test3 = document.location.href;
    var position = test3.indexOf('?');
    var position2 = test3.substring(0, position);
    var test2 = cyrb53(position2);
    localStorage.setItem(test2, window.scrollY);
};

// gestione dell'emoji :calendar:
function activateCalendar(el, index, target, dateformat, pathfile) {

    var data = $('#' + el.id).data('datepicker');
    if (data) { // esiste già il datepicker, lo uso        
        $('#' + el.id).datepicker('show');
    } else { // il datepicker non esiste, lo creo e lo inizializzo
        var currentDatePicker = $('#' + el.id).datepicker({
            format: dateformat, //'dd-mm-yyyy'
            todayHighlight: true,
            todayBtn: "linked"
        });

        currentDatePicker.on('changeDate', function (ev) {
            var toReplace = $('#' + el.id).data('datepicker').getFormattedDate(dateformat);
            $('#' + target).text(toReplace);
            currentDatePicker.datepicker('hide');
            $.get("/api/WriteMD/SetCalendar?index=" + index + "&pathFile=" + pathfile + "&toReplace=" + toReplace, function (data) {
                $(".result").html(data);
                console.log(data);
            });
        });
        currentDatePicker.datepicker('show');
    }
}



// gestione degli emoji di processo
function dynamicEmojiForProcess(el, index, pathfile) {
    


    let dataToSet;
    el.removeAttribute('data-tippy-content');
    if (el.innerText.trim() == 'ℹ️') {
        el.innerText = '🆗';
        el.setAttribute('data-tippy-content', 'approvato');
        dataToSet = 'approvato';
    } else
        if (el.innerText.trim() == '🆗') {
            el.innerText = '⚠️';
            el.setAttribute('data-tippy-content', 'attenzione');
            dataToSet = 'attenzione';

        } else
            if (el.innerText.trim() == '⚠️') {
                el.innerText = '🚧';
                el.setAttribute('data-tippy-content', 'work in progress');
                dataToSet = 'work in progress';
            } else
                if (el.innerText.trim() == '🚧') {
                    el.innerText = '✔️';
                    el.setAttribute('data-tippy-content', 'completato');
                    dataToSet = 'completato';
                } else
                    if (el.innerText.trim() == '✔️') {
                        el.innerText = 'ℹ️';
                        el.setAttribute('data-tippy-content', 'in valutazione');
                        dataToSet = 'Info';
                    }
    var currentIndex = el.attributes['data-md-process-index'].value;
    $.get("/api/WriteMD/SetEmojiProcess?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText.trim(), function (data) {
        $(".result").html(data);
        console.log(data);
    });
    setTooltipProcess(dataToSet, el);

}

function setTooltipPriority(dataToSet, el) {    
    
    let currentPriority = tippyDictPriority[el.attributes[8].value];
    currentPriority.setContent(dataToSet);
    currentPriority.reference.setAttribute('data-tippy-content',dataToSet)
    setTippyTypePriority(currentPriority.reference, currentPriority);
    currentPriority.show();
}

function setTooltipProcess(dataToSet, el) {
    
    let current = tippyDictProcess[el.attributes[4].value]; //data-tippy-process-id
    current.setContent(dataToSet);    
    current.reference.setAttribute('data-tippy-content', dataToSet)
    setTippyTypeProcess(current.reference, current);
    current.show();
}


// gestione degli emoji di priorità
function dynamicEmojiForPriority(el, index, pathfile) {
    
    if (el.attributes['data-md-table-game-index'] == undefined) {
        return;
    }
    if (el.innerText == '❓') {
        el.innerText = '❔';
        //el.title = 'da valutare';
        dataToSet = 'da valutare';
    } else
        if (el.innerText == '❔') {
            el.innerText = '❕';
            //el.title = 'obbligatorio';
            dataToSet = 'obbligatorio';

        } else
            if (el.innerText == '❕') {
                el.innerText = '❗';
                //el.title = 'urgente';
                dataToSet = 'urgente';
            } else
                if (el.innerText == '❗') {
                    el.innerText = '❌';
                    //el.title = 'annullata';
                    dataToSet = 'annullata';
                } else
                    if (el.innerText == '❌') {
                        el.innerText = '⛔';
                        //el.title = 'fermata';
                        dataToSet = 'fermata';
                        var element = $('#' + el.id).parent();
                    } else
                        if (el.innerText == '⛔') {
                            el.innerText = '❎';
                            //el.title = 'conclusa';
                            dataToSet = 'conclusa';
                            var element = $('#' + el.id).parent();
                            var check = element.parent().is('li');
                            if (check) {
                                element = element.parent();
                            }
                            element.fadeOut(3000);
                        } else

                            if (el.innerText == '❎') {
                                el.innerText = '❓';
                                //el.title = 'da valutare';
                                dataToSet = 'dubbio urgente';
                                var element = $('#' + el.id).parent();
                                var check = element.is('li');
                                if (!check) {
                                    element = element.parent();
                                }
                                element.stop();
                                element.fadeIn();
                            }

    var currentIndex = el.attributes['data-md-priority-index'].value;
    $.get("/api/WriteMD/SetEmojiPriority?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText, function (data) {
        $(".result").html(data);
        console.log(data);
    });
    setTooltipPriority(dataToSet, el);
}

// gestione del box di ricerca della toc
//function filterToc() {
//    // Declare variables
//    var input, filter, ul, li, a, i, txtValue;
//    input = document.getElementById('tocInputFilter');
//    filter = input.value.toUpperCase();
//    ul = document.getElementById("ulToc");
//    li = ul.getElementsByTagName('li');

//    // Loop through all list items, and hide those who don't match the search query
//    for (i = 0; i < li.length; i++) {
//        a = li[i].getElementsByTagName("a")[0];
//        txtValue = a.textContent || a.innerText;
//        if (txtValue.toUpperCase().indexOf(filter) > -1) {
//            li[i].style.display = "";
//        } else {
//            li[i].style.display = "none";
//        }
//    }
//}

function toggleTOC(documentPath) {

    var showToc = false;
    showToc = toggleToc2();

    $.get("/api/AppSettings/ShowToc?documentPathEncoded=" + documentPath + "&showToc=" + showToc, function (data) {
        console.log(data);
    });

    
}

function toggleToc2() {
    var showToc = false;
    if ($('#TOC').is(":hidden")) {
        var $page = $('#page');
        $page.attr('class', 'col-9');
        setTimeout(function () {
            var $toc = $('#TOC');
            $toc.attr('class', 'col-3');
            $toc.show();

        }, 500);
        showToc = true;

    } else {
        var $toc = $('#TOC');
        $toc.hide();
        $toc.removeAttr('class');
        var $page = $('#page');
        $page.attr('class', 'col-12');
        showToc = false;
    }
    return showToc;
}

// inizializzazione, al caricamnto della pagina,
// del canvas, tela per la matitina, fuori dal campo visivo dell'utente
$(function () {
    console.log("Create canvas");
    if (window.toggleCanvas == 'undefined') {
        window.toggleCanvas = false;
    }

    //console.log('initialize canvas');

    window.toggleCanvas = !window.toggleCanvas;
    window.canvas = document.createElement('canvas');
    window.canvas.setAttribute('id', 'writeCanvas');
    window.canvas.setAttribute('class', 'canvasForWriting'); // setting z-index to 100
    document.body.appendChild(canvas);

    // some hotfixes... ( ≖_≖)
    //document.body.style.margin = 0;
    window.canvas.setAttribute('hidden', 'hidden');
    window.canvas.style.position = 'absolute';
    window.canvas.style.top = 0;
    window.canvas.style.left = window.innerWidth - 40; // qui è dove si imposta il canvas FUORI dal campo visivo

    // get canvas 2D context and set him correct size
    window.ctx = canvas.getContext('2d');
    resize();

    // last known position
    console.log()
    window.shiftY = 0;
    window.shiftX = -40;
    window.pos = { x: 0, y: 0 };
    window.scrollPos = { x: window.shiftX, y: window.shiftY };

    window.addEventListener('resize', resize);
    document.addEventListener('mousemove', draw);
    document.addEventListener('mousedown', setPosition);
    document.addEventListener('mouseenter', setPosition);
    document.addEventListener('scroll', scrollPosition);
});

// gestione della matitina per evidenziare la pagina
function toggleMdCanvas(me) {
    debugger;
    if (window.toggleCanvas) {
        me.children[0].src = "/assets/drawAnimated.gif";
        $(window.canvas).removeAttr('hidden');
        $(window.canvas).animate({
            left: 40,
        }, function () {
        });

    } else {
        me.children[0].src = "/assets/drawStatic.png";
        $(window.canvas).animate({
            left: window.innerWidth,
        }, function () {
            window.canvas.setAttribute('hidden', 'hidden');
        });

    }
    window.toggleCanvas = !window.toggleCanvas;
}

function scrollPosition(e) {
    scrollPos.x = window.scrollX + window.shiftX;
    scrollPos.y = window.scrollY + window.shiftY;
}
// new position from mouse event
function setPosition(e) {
    pos.x = scrollPos.x + e.clientX;
    pos.y = scrollPos.y + e.clientY;
}

// resize canvas
function resize() {
    window.ctx.canvas.width = window.innerWidth;
    window.ctx.canvas.height = document.documentElement.scrollHeight;
}


function draw(e) {
    if (!window.toggleCanvas) {

        
        // mouse left button must be pressed
        if (e.buttons !== 1) return;

        console.log("draw");
        window.ctx.beginPath(); // begin

        window.ctx.lineWidth = 5;
        window.ctx.lineCap = 'round';
        window.ctx.strokeStyle = '#2bc02d';

        window.ctx.moveTo(pos.x, pos.y); // from
        setPosition(e);
        window.ctx.lineTo(pos.x, pos.y); // to

        window.ctx.stroke(); // draw it!
    }

}

//Gestione Clipboard *************
async function copyToClipboard(objectThis, relativePathFile, hashFile, step) {
    const test = await $.get("/api/plantumlextensions/GetPng?pathFile=" + relativePathFile +
        "&hashFile=" + hashFile +
        "&step=" + step, function (data) {
            console.log(data);
        });
    const response = await fetch(objectThis);  //'/assets/ConnectionLost.png'
    const blob = await response.blob();
    setToClipboard(blob);
}

const setToClipboard = async blob => {
    const data = [new ClipboardItem({ [blob.type]: blob })];
    await navigator.clipboard.write(data);
}
// *******************************

//Gestione presentazione Plantuml

async function presentationSVG(relativePathFile, hashFile) {
    var $forwardArrow = $('#forwardArrow' + hashFile);
    var trueStep = parseInt($forwardArrow.attr("data-step"));
    const result = await $.get("/api/plantumlextensions/PresentationSVG?pathFile=" + relativePathFile +
        "&hashFile=" + hashFile +
        "&step=" + trueStep);

    var totalStep = result.totalStep;
    const response = await fetch(result.generatedFileName);
    const text = await response.text();
    var nodeSvg = $.parseHTML(text);
    var $parent = $('#' + hashFile);
    var mySvg = $parent.find('svg'); // svg
    //var mySvg = childrens[0];
    mySvg.remove();

    $parent.append(nodeSvg);
    var $forwardArrow = $('#forwardArrow' + hashFile);
    trueStep = trueStep + 1;
    if (trueStep >= totalStep) {
        trueStep = 0;
    }
    $forwardArrow.attr('data-step', trueStep);
}

//presentationSVG