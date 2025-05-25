import { P as v, d as S, k as V, F as C, S as y, T as b, N as x, D as T, b as O, e as p, $ as D, f as k } from "./integration-BHt4Ncdo.js";
function N(i = {}) {
  return new v({
    view(e) {
      return new R(e, i);
    }
  });
}
class R {
  constructor(e, t) {
    var o;
    this.editorView = e, this.cursorPos = null, this.element = null, this.timeout = -1, this.width = (o = t.width) !== null && o !== void 0 ? o : 1, this.color = t.color === !1 ? void 0 : t.color || "black", this.class = t.class, this.handlers = ["dragover", "dragend", "drop", "dragleave"].map((r) => {
      let l = (s) => {
        this[r](s);
      };
      return e.dom.addEventListener(r, l), { name: r, handler: l };
    });
  }
  destroy() {
    this.handlers.forEach(({ name: e, handler: t }) => this.editorView.dom.removeEventListener(e, t));
  }
  update(e, t) {
    this.cursorPos != null && t.doc != e.state.doc && (this.cursorPos > e.state.doc.content.size ? this.setCursor(null) : this.updateOverlay());
  }
  setCursor(e) {
    e != this.cursorPos && (this.cursorPos = e, e == null ? (this.element.parentNode.removeChild(this.element), this.element = null) : this.updateOverlay());
  }
  updateOverlay() {
    let e = this.editorView.state.doc.resolve(this.cursorPos), t = !e.parent.inlineContent, o;
    if (t) {
      let n = e.nodeBefore, d = e.nodeAfter;
      if (n || d) {
        let c = this.editorView.nodeDOM(this.cursorPos - (n ? n.nodeSize : 0));
        if (c) {
          let u = c.getBoundingClientRect(), f = n ? u.bottom : u.top;
          n && d && (f = (f + this.editorView.nodeDOM(this.cursorPos).getBoundingClientRect().top) / 2), o = { left: u.left, right: u.right, top: f - this.width / 2, bottom: f + this.width / 2 };
        }
      }
    }
    if (!o) {
      let n = this.editorView.coordsAtPos(this.cursorPos);
      o = { left: n.left - this.width / 2, right: n.left + this.width / 2, top: n.top, bottom: n.bottom };
    }
    let r = this.editorView.dom.offsetParent;
    this.element || (this.element = r.appendChild(document.createElement("div")), this.class && (this.element.className = this.class), this.element.style.cssText = "position: absolute; z-index: 50; pointer-events: none;", this.color && (this.element.style.backgroundColor = this.color)), this.element.classList.toggle("prosemirror-dropcursor-block", t), this.element.classList.toggle("prosemirror-dropcursor-inline", !t);
    let l, s;
    if (!r || r == document.body && getComputedStyle(r).position == "static")
      l = -pageXOffset, s = -pageYOffset;
    else {
      let n = r.getBoundingClientRect();
      l = n.left - r.scrollLeft, s = n.top - r.scrollTop;
    }
    this.element.style.left = o.left - l + "px", this.element.style.top = o.top - s + "px", this.element.style.width = o.right - o.left + "px", this.element.style.height = o.bottom - o.top + "px";
  }
  scheduleRemoval(e) {
    clearTimeout(this.timeout), this.timeout = setTimeout(() => this.setCursor(null), e);
  }
  dragover(e) {
    if (!this.editorView.editable)
      return;
    let t = this.editorView.posAtCoords({ left: e.clientX, top: e.clientY }), o = t && t.inside >= 0 && this.editorView.state.doc.nodeAt(t.inside), r = o && o.type.spec.disableDropCursor, l = typeof r == "function" ? r(this.editorView, t, e) : r;
    if (t && !l) {
      let s = t.pos;
      if (this.editorView.dragging && this.editorView.dragging.slice) {
        let n = S(this.editorView.state.doc, s, this.editorView.dragging.slice);
        n != null && (s = n);
      }
      this.setCursor(s), this.scheduleRemoval(5e3);
    }
  }
  dragend() {
    this.scheduleRemoval(20);
  }
  drop() {
    this.scheduleRemoval(20);
  }
  dragleave(e) {
    (e.target == this.editorView.dom || !this.editorView.dom.contains(e.relatedTarget)) && this.setCursor(null);
  }
}
class a extends p {
  /**
  Create a gap cursor.
  */
  constructor(e) {
    super(e, e);
  }
  map(e, t) {
    let o = e.resolve(t.map(this.head));
    return a.valid(o) ? new a(o) : p.near(o);
  }
  content() {
    return y.empty;
  }
  eq(e) {
    return e instanceof a && e.head == this.head;
  }
  toJSON() {
    return { type: "gapcursor", pos: this.head };
  }
  /**
  @internal
  */
  static fromJSON(e, t) {
    if (typeof t.pos != "number")
      throw new RangeError("Invalid input for GapCursor.fromJSON");
    return new a(e.resolve(t.pos));
  }
  /**
  @internal
  */
  getBookmark() {
    return new g(this.anchor);
  }
  /**
  @internal
  */
  static valid(e) {
    let t = e.parent;
    if (t.isTextblock || !B(e) || !F(e))
      return !1;
    let o = t.type.spec.allowGapCursor;
    if (o != null)
      return o;
    let r = t.contentMatchAt(e.index()).defaultType;
    return r && r.isTextblock;
  }
  /**
  @internal
  */
  static findGapCursorFrom(e, t, o = !1) {
    e: for (; ; ) {
      if (!o && a.valid(e))
        return e;
      let r = e.pos, l = null;
      for (let s = e.depth; ; s--) {
        let n = e.node(s);
        if (t > 0 ? e.indexAfter(s) < n.childCount : e.index(s) > 0) {
          l = n.child(t > 0 ? e.indexAfter(s) : e.index(s) - 1);
          break;
        } else if (s == 0)
          return null;
        r += t;
        let d = e.doc.resolve(r);
        if (a.valid(d))
          return d;
      }
      for (; ; ) {
        let s = t > 0 ? l.firstChild : l.lastChild;
        if (!s) {
          if (l.isAtom && !l.isText && !x.isSelectable(l)) {
            e = e.doc.resolve(r + l.nodeSize * t), o = !1;
            continue e;
          }
          break;
        }
        l = s, r += t;
        let n = e.doc.resolve(r);
        if (a.valid(n))
          return n;
      }
      return null;
    }
  }
}
a.prototype.visible = !1;
a.findFrom = a.findGapCursorFrom;
p.jsonID("gapcursor", a);
class g {
  constructor(e) {
    this.pos = e;
  }
  map(e) {
    return new g(e.map(this.pos));
  }
  resolve(e) {
    let t = e.resolve(this.pos);
    return a.valid(t) ? new a(t) : p.near(t);
  }
}
function B(i) {
  for (let e = i.depth; e >= 0; e--) {
    let t = i.index(e), o = i.node(e);
    if (t == 0) {
      if (o.type.spec.isolating)
        return !0;
      continue;
    }
    for (let r = o.child(t - 1); ; r = r.lastChild) {
      if (r.childCount == 0 && !r.inlineContent || r.isAtom || r.type.spec.isolating)
        return !0;
      if (r.inlineContent)
        return !1;
    }
  }
  return !0;
}
function F(i) {
  for (let e = i.depth; e >= 0; e--) {
    let t = i.indexAfter(e), o = i.node(e);
    if (t == o.childCount) {
      if (o.type.spec.isolating)
        return !0;
      continue;
    }
    for (let r = o.child(t); ; r = r.firstChild) {
      if (r.childCount == 0 && !r.inlineContent || r.isAtom || r.type.spec.isolating)
        return !0;
      if (r.inlineContent)
        return !1;
    }
  }
  return !0;
}
function E() {
  return new v({
    props: {
      decorations: J,
      createSelectionBetween(i, e, t) {
        return e.pos == t.pos && a.valid(t) ? new a(t) : null;
      },
      handleClick: M,
      handleKeyDown: L,
      handleDOMEvents: { beforeinput: z }
    }
  });
}
const L = V({
  ArrowLeft: h("horiz", -1),
  ArrowRight: h("horiz", 1),
  ArrowUp: h("vert", -1),
  ArrowDown: h("vert", 1)
});
function h(i, e) {
  const t = i == "vert" ? e > 0 ? "down" : "up" : e > 0 ? "right" : "left";
  return function(o, r, l) {
    let s = o.selection, n = e > 0 ? s.$to : s.$from, d = s.empty;
    if (s instanceof b) {
      if (!l.endOfTextblock(t) || n.depth == 0)
        return !1;
      d = !1, n = o.doc.resolve(e > 0 ? n.after() : n.before());
    }
    let c = a.findGapCursorFrom(n, e, d);
    return c ? (r && r(o.tr.setSelection(new a(c))), !0) : !1;
  };
}
function M(i, e, t) {
  if (!i || !i.editable)
    return !1;
  let o = i.state.doc.resolve(e);
  if (!a.valid(o))
    return !1;
  let r = i.posAtCoords({ left: t.clientX, top: t.clientY });
  return r && r.inside > -1 && x.isSelectable(i.state.doc.nodeAt(r.inside)) ? !1 : (i.dispatch(i.state.tr.setSelection(new a(o))), !0);
}
function z(i, e) {
  if (e.inputType != "insertCompositionText" || !(i.state.selection instanceof a))
    return !1;
  let { $from: t } = i.state.selection, o = t.parent.contentMatchAt(t.index()).findWrapping(i.state.schema.nodes.text);
  if (!o)
    return !1;
  let r = C.empty;
  for (let s = o.length - 1; s >= 0; s--)
    r = C.from(o[s].createAndFill(null, r));
  let l = i.state.tr.replace(t.pos, t.pos, new y(r, 0, 0));
  return l.setSelection(b.near(l.doc.resolve(t.pos + 1))), i.dispatch(l), !1;
}
function J(i) {
  if (!(i.selection instanceof a))
    return null;
  let e = document.createElement("div");
  return e.className = "ProseMirror-gapcursor", T.create(i.doc, [O.widget(i.selection.head, e, { key: "gapcursor" })]);
}
function w(i, e) {
  return Object.assign(i, {
    meta: {
      package: "@milkdown/plugin-cursor",
      ...e
    }
  }), i;
}
const m = D(
  {},
  "dropCursorConfig"
);
w(m, {
  displayName: "Ctx<dropCursor>"
});
const A = k(
  (i) => N(i.get(m.key))
);
w(A, {
  displayName: "Prose<dropCursor>"
});
const P = k(() => E());
w(P, {
  displayName: "Prose<gapCursor>"
});
const X = [
  m,
  A,
  P
], I = (i, e) => {
  i.config((t) => {
    t.update(m.key, () => {
      var o, r;
      return {
        class: "crepe-drop-cursor",
        width: (o = e == null ? void 0 : e.width) != null ? o : 4,
        color: (r = e == null ? void 0 : e.color) != null ? r : !1
      };
    });
  }).use(X);
};
export {
  I as defineFeature
};
