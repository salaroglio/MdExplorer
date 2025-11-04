/**
 * MdExplorer - PlantUML Integration
 * ==================================
 * Manages PlantUML diagram presentations and clipboard operations
 *
 * Features:
 * - Step-through SVG presentation mode
 * - Copy diagram to clipboard as PNG
 * - Async fetch and DOM manipulation
 * - Circular step navigation (loops back to 0)
 *
 * Backend API:
 * - GET /api/plantumlextensions/PresentationSVG
 * - GET /api/plantumlextensions/GetPng
 *
 * DOM:
 * - #forwardArrow{hashFile}: Navigation button with data-step attribute
 * - #{hashFile}: Container for SVG diagram
 */

/**
 * Navigate to next step in PlantUML presentation
 * Fetches SVG for current step, replaces existing SVG, increments step counter
 * Loops back to step 0 when reaching totalStep
 *
 * @param {string} relativePathFile - Relative path to markdown file
 * @param {string} hashFile - Unique hash identifier for the diagram
 */
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

/**
 * Copy PlantUML diagram to clipboard as PNG
 * Fetches PNG from backend, converts to blob, writes to clipboard
 *
 * @param {string} objectThis - URL of the PNG image
 * @param {string} relativePathFile - Relative path to markdown file
 * @param {string} hashFile - Unique hash identifier for the diagram
 * @param {number} step - Current step in presentation
 */
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

/**
 * Write blob to system clipboard
 * Uses Clipboard API with ClipboardItem
 *
 * @param {Blob} blob - Image blob to copy
 */
const setToClipboard = async blob => {
    const data = [new ClipboardItem({ [blob.type]: blob })];
    await navigator.clipboard.write(data);
}
