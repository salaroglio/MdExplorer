// Gestione del movimento delle immagini
// test di caricamento tooltip
$(function () {
    $('.resizable').mousedown( function () {
        
    });
});


// Gestione del resize delle immagini
function resizeImage(currentDiv,pathFile,linkHash, cssHash) {    
    // going inside the div
    var img = currentDiv.childNodes[0].childNodes[0];
    // getting CSSHash from attributes

    currentHash = currentDiv.attributes['md-css-hash'].value;

    var currentImageData = {
        pathFile: pathFile,
        linkHash: linkHash,
        cssHash: currentHash,
        Width: img.width,
        Height: img.height,
        ClientX: img.x,
        ClientY:img.y
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
                var test = $divs[index].attributes['md-css-hash'].value = data.cssHash;
            });
                //.attributes['md-css-hash'].value = data.cssHash
        }
    });
}


// test di caricamento tooltip
$(function () {
    //debugger;
    //tippy('[data-tippy-content]');
    //var test = tippy('#myButton', {
    //    content: 'My tooltip!',
    //});
    //var test1 = test;
});


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
    // tentativo di memorizzare la posizione corrente della pagina,perché sia riproposta dopo un refresh
    var scrollpos = localStorage.getItem('scrollpos');
    if (scrollpos) window.scrollTo(0, scrollpos);

    // inizializzazione dei datepicker

});

window.onbeforeunload = function (e) {
    localStorage.setItem('scrollpos', window.scrollY);
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

    if (el.innerText == 'ℹ️') {
        el.innerText = '🆗';
        el.title = 'approvato';

    } else
        if (el.innerText == '🆗') {
            el.innerText = '⚠️';
            el.title = 'attenzione!';
        } else
            if (el.innerText == '⚠️') {
                el.innerText = '🚧';
                el.title = 'work in progress';
            } else
                if (el.innerText == '🚧') {
                    el.innerText = '✔️';
                    el.title = 'completato';
                } else
                    if (el.innerText == '✔️') {
                        el.innerText = 'ℹ️';
                        el.title = 'in valutazione';
                    }
    var currentIndex = el.attributes['data-md-process-index'].value;
    $.get("/api/WriteMD/SetEmojiProcess?index=" + currentIndex + "&pathFile=" + pathfile + "&toReplace=" + el.innerText, function (data) {
        $(".result").html(data);
        console.log(data);
    });

}

// gestione degli emoji di priorità
function dynamicEmojiForPriority(el, index, pathfile) {

    if (el.innerText == '❓') {
        el.innerText = '❔';
        el.title = 'dubbio';
    } else
        if (el.innerText == '❔') {
            el.innerText = '❕';
            el.title = 'obbligatorio';

        } else
            if (el.innerText == '❕') {
                el.innerText = '❗';
                el.title = 'urgente';
            } else
                if (el.innerText == '❗') {
                    el.innerText = '❌';
                    el.title = 'annullata';
                } else
                    if (el.innerText == '❌') {
                        el.innerText = '⛔';
                        el.title = 'fermata';
                        var element = $('#' + el.id).parent();
                    } else
                        if (el.innerText == '⛔') {
                            el.innerText = '❎';
                            el.title = 'conclusa';
                            var element = $('#' + el.id).parent();
                            var check = element.parent().is('li');
                            if (check) {
                                element = element.parent();
                            }
                            element.fadeOut(3000);
                        } else

                            if (el.innerText == '❎') {
                                el.innerText = '❓';
                                el.title = 'da valutare';
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

}

// gestione del box di ricerca della toc
function filterToc() {
    // Declare variables
    var input, filter, ul, li, a, i, txtValue;
    input = document.getElementById('tocInputFilter');
    filter = input.value.toUpperCase();
    ul = document.getElementById("ulToc");
    li = ul.getElementsByTagName('li');

    // Loop through all list items, and hide those who don't match the search query
    for (i = 0; i < li.length; i++) {
        a = li[i].getElementsByTagName("a")[0];
        txtValue = a.textContent || a.innerText;
        if (txtValue.toUpperCase().indexOf(filter) > -1) {
            li[i].style.display = "";
        } else {
            li[i].style.display = "none";
        }
    }
}

function toggleTOC(documentPath) {

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


    $.get("/api/AppSettings/ShowToc?documentPathEncoded=" + documentPath + "&showToc=" + showToc, function (data) {
        console.log(data);
    });
}

// gestione della matitina per evidenziare la pagina
function toggleMdCanvas() {
    if (window.toggleCanvas == 'undefined') {
        window.toggleCanvas = false;
    }

    if (window.toggleCanvas) {
        window.canvas.remove();
        window.toggleCanvas = !window.toggleCanvas;
        return;
    }
    window.toggleCanvas = !window.toggleCanvas;
    window.canvas = document.createElement('canvas');
    window.canvas.setAttribute('id', 'writeCanvas');
    document.body.appendChild(canvas);

    // some hotfixes... ( ≖_≖)
    //document.body.style.margin = 0;
    canvas.style.position = 'absolute';
    canvas.style.top = 40;
    canvas.style.left = 0;

    // get canvas 2D context and set him correct size
    window.ctx = canvas.getContext('2d');
    resize();

    // last known position
    window.shiftY = -40;
    window.pos = { x: 0, y: 0 };
    window.scrollPos = { x: 0, y: window.shiftY };

    window.addEventListener('resize', resize);
    document.addEventListener('mousemove', draw);
    document.addEventListener('mousedown', setPosition);
    document.addEventListener('mouseenter', setPosition);
    document.addEventListener('scroll', scrollPosition);
}
function scrollPosition(e) {
    scrollPos.x = window.scrollX;
    scrollPos.y = window.scrollY + window.shiftY;
}
// new position from mouse event
function setPosition(e) {
    pos.x = scrollPos.x + e.clientX;
    pos.y = scrollPos.y + e.clientY;
}

// resize canvas
function resize() {
    ctx.canvas.width = window.innerWidth;
    ctx.canvas.height = document.documentElement.scrollHeight;
}

function draw(e) {
    // mouse left button must be pressed
    if (e.buttons !== 1) return;

    ctx.beginPath(); // begin

    ctx.lineWidth = 5;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#2bc02d';

    ctx.moveTo(pos.x, pos.y); // from
    setPosition(e);
    ctx.lineTo(pos.x, pos.y); // to

    ctx.stroke(); // draw it!
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