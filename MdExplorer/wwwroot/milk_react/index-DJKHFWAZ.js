import { E as d } from "./index.es-Dganzb1x.js";
import { s as m, H as s, t as o } from "./index-B3KiKTSt-Dla4yOC_.js";
const g = "#e5c07b", t = "#e06c75", b = "#56b6c2", p = "#ffffff", r = "#abb2bf", a = "#7d8799", f = "#61afef", u = "#98c379", c = "#d19a66", k = "#c678dd", h = "#21252b", l = "#2c313a", n = "#282c34", e = "#353a42", C = "#3E4451", i = "#528bff", y = /* @__PURE__ */ d.theme({
  "&": {
    color: r,
    backgroundColor: n
  },
  ".cm-content": {
    caretColor: i
  },
  ".cm-cursor, .cm-dropCursor": { borderLeftColor: i },
  "&.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection": { backgroundColor: C },
  ".cm-panels": { backgroundColor: h, color: r },
  ".cm-panels.cm-panels-top": { borderBottom: "2px solid black" },
  ".cm-panels.cm-panels-bottom": { borderTop: "2px solid black" },
  ".cm-searchMatch": {
    backgroundColor: "#72a1ff59",
    outline: "1px solid #457dff"
  },
  ".cm-searchMatch.cm-searchMatch-selected": {
    backgroundColor: "#6199ff2f"
  },
  ".cm-activeLine": { backgroundColor: "#6699ff0b" },
  ".cm-selectionMatch": { backgroundColor: "#aafe661a" },
  "&.cm-focused .cm-matchingBracket, &.cm-focused .cm-nonmatchingBracket": {
    backgroundColor: "#bad0f847"
  },
  ".cm-gutters": {
    backgroundColor: n,
    color: a,
    border: "none"
  },
  ".cm-activeLineGutter": {
    backgroundColor: l
  },
  ".cm-foldPlaceholder": {
    backgroundColor: "transparent",
    border: "none",
    color: "#ddd"
  },
  ".cm-tooltip": {
    border: "none",
    backgroundColor: e
  },
  ".cm-tooltip .cm-tooltip-arrow:before": {
    borderTopColor: "transparent",
    borderBottomColor: "transparent"
  },
  ".cm-tooltip .cm-tooltip-arrow:after": {
    borderTopColor: e,
    borderBottomColor: e
  },
  ".cm-tooltip-autocomplete": {
    "& > ul > li[aria-selected]": {
      backgroundColor: l,
      color: r
    }
  }
}, { dark: !0 }), B = /* @__PURE__ */ s.define([
  {
    tag: o.keyword,
    color: k
  },
  {
    tag: [o.name, o.deleted, o.character, o.propertyName, o.macroName],
    color: t
  },
  {
    tag: [/* @__PURE__ */ o.function(o.variableName), o.labelName],
    color: f
  },
  {
    tag: [o.color, /* @__PURE__ */ o.constant(o.name), /* @__PURE__ */ o.standard(o.name)],
    color: c
  },
  {
    tag: [/* @__PURE__ */ o.definition(o.name), o.separator],
    color: r
  },
  {
    tag: [o.typeName, o.className, o.number, o.changed, o.annotation, o.modifier, o.self, o.namespace],
    color: g
  },
  {
    tag: [o.operator, o.operatorKeyword, o.url, o.escape, o.regexp, o.link, /* @__PURE__ */ o.special(o.string)],
    color: b
  },
  {
    tag: [o.meta, o.comment],
    color: a
  },
  {
    tag: o.strong,
    fontWeight: "bold"
  },
  {
    tag: o.emphasis,
    fontStyle: "italic"
  },
  {
    tag: o.strikethrough,
    textDecoration: "line-through"
  },
  {
    tag: o.link,
    color: a,
    textDecoration: "underline"
  },
  {
    tag: o.heading,
    fontWeight: "bold",
    color: t
  },
  {
    tag: [o.atom, o.bool, /* @__PURE__ */ o.special(o.variableName)],
    color: c
  },
  {
    tag: [o.processingInstruction, o.string, o.inserted],
    color: u
  },
  {
    tag: o.invalid,
    color: p
  }
]), N = [y, /* @__PURE__ */ m(B)];
export {
  N as oneDark,
  B as oneDarkHighlightStyle,
  y as oneDarkTheme
};
