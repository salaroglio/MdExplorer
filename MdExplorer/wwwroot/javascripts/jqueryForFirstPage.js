function dynamicEmojiForProcess(el, index, pathfile) {

    if (el.innerText == 'ℹ️') {
        el.innerText = '🆗';
        el.title = 'approvato';
        
    }else 
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
    
    $.get("/api/WriteMD/SetEmoji?index=" + index + "&pathFile=" + pathfile + "&toReplace=" + el.innerText, function (data) {
        $(".result").html(data);
        console.log(data);
    });

}

function dynamicEmojiForPriority(el, index, pathfile) {

    if (el.innerText == '❓') {
        el.innerText = '❔';
        el.title = 'dubbio';
    }else
    if (el.innerText == '❔') {
        el.innerText = '❕';
        el.title = 'obbligatorio';
    }else
    if (el.innerText == '❕') {
        el.innerText = '❗';
        el.title = 'urgente';
        
    }else
    if (el.innerText == '❗') {
        el.innerText = '❌';
        el.title = 'annullata';
        
    }else
    if (el.innerText == '❌') {
        el.innerText = '⛔';
        el.title = 'fermata';
        
    }else

    if (el.innerText == '⛔') {
        el.innerText = '❓';
        el.title = 'da valutare';
    }

    
    $.get("/api/WriteMD/SetEmoji?index=" + index + "&pathFile=" + pathfile + "&toReplace=" + el.innerText, function (data) {
        $(".result").html(data);
        console.log(data);
    });

}


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