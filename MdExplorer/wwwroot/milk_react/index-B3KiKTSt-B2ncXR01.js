import { H as he, J as Z, E as x, h as d, a3 as tt, a4 as nt, r as Be, a5 as re, G as rt, C as ot, F as ge, W as ue, a6 as O, g as we, k as Se, S as D, a7 as rn, a8 as on, a1 as sn, a9 as W, aa as be, w as ln, V as it, u as cn, ab as J, K as y, ac as Te, M as an, Q as hn, _ as un, a0 as C, y as st, ad as lt, ae as fn, af as dn, Z as mn, ag as pn, ah as gn, ai as yn, aj as xn, ak as kn, al as Sn, am as bn, an as An, ao as Cn, ap as Mn, aq as vn, ar as Ln, v as ct, as as Dn, at as Bn } from "./integration-p001bnNq.js";
import { a as wn, b as Tn, d as Rn, e as En } from "./index-BgSfle5-.js";
import { c as In, s as Pn, a as On, e as qn, h as at } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { c as Fn, a as Wn } from "./index.es-DclFhKjZ.js";
const Nn = (t) => {
  let { state: e } = t, r = e.doc.lineAt(e.selection.main.from), n = Ee(t.state, r.from);
  return n.line ? $n(t) : n.block ? Un(t) : !1;
};
function Re(t, e) {
  return ({ state: r, dispatch: n }) => {
    if (r.readOnly)
      return !1;
    let o = t(e, r);
    return o ? (n(r.update(o)), !0) : !1;
  };
}
const $n = /* @__PURE__ */ Re(
  Hn,
  0
  /* CommentOption.Toggle */
), Vn = /* @__PURE__ */ Re(
  ht,
  0
  /* CommentOption.Toggle */
), Un = /* @__PURE__ */ Re(
  (t, e) => ht(t, e, Gn(e)),
  0
  /* CommentOption.Toggle */
);
function Ee(t, e) {
  let r = t.languageDataAt("commentTokens", e);
  return r.length ? r[0] : {};
}
const G = 50;
function zn(t, { open: e, close: r }, n, o) {
  let i = t.sliceDoc(n - G, n), s = t.sliceDoc(o, o + G), l = /\s*$/.exec(i)[0].length, c = /^\s*/.exec(s)[0].length, h = i.length - l;
  if (i.slice(h - e.length, h) == e && s.slice(c, c + r.length) == r)
    return {
      open: { pos: n - l, margin: l && 1 },
      close: { pos: o + c, margin: c && 1 }
    };
  let a, u;
  o - n <= 2 * G ? a = u = t.sliceDoc(n, o) : (a = t.sliceDoc(n, n + G), u = t.sliceDoc(o - G, o));
  let f = /^\s*/.exec(a)[0].length, m = /\s*$/.exec(u)[0].length, L = u.length - m - r.length;
  return a.slice(f, f + e.length) == e && u.slice(L, L + r.length) == r ? {
    open: {
      pos: n + f + e.length,
      margin: /\s/.test(a.charAt(f + e.length)) ? 1 : 0
    },
    close: {
      pos: o - m - r.length,
      margin: /\s/.test(u.charAt(L - 1)) ? 1 : 0
    }
  } : null;
}
function Gn(t) {
  let e = [];
  for (let r of t.selection.ranges) {
    let n = t.doc.lineAt(r.from), o = r.to <= n.to ? n : t.doc.lineAt(r.to);
    o.from > n.from && o.from == r.to && (o = r.to == n.to + 1 ? n : t.doc.lineAt(r.to - 1));
    let i = e.length - 1;
    i >= 0 && e[i].to > n.from ? e[i].to = o.to : e.push({ from: n.from + /^\s*/.exec(n.text)[0].length, to: o.to });
  }
  return e;
}
function ht(t, e, r = e.selection.ranges) {
  let n = r.map((i) => Ee(e, i.from).block);
  if (!n.every((i) => i))
    return null;
  let o = r.map((i, s) => zn(e, n[s], i.from, i.to));
  if (t != 2 && !o.every((i) => i))
    return { changes: e.changes(r.map((i, s) => o[s] ? [] : [{ from: i.from, insert: n[s].open + " " }, { from: i.to, insert: " " + n[s].close }])) };
  if (t != 1 && o.some((i) => i)) {
    let i = [];
    for (let s = 0, l; s < o.length; s++)
      if (l = o[s]) {
        let c = n[s], { open: h, close: a } = l;
        i.push({ from: h.pos - c.open.length, to: h.pos + h.margin }, { from: a.pos - a.margin, to: a.pos + c.close.length });
      }
    return { changes: i };
  }
  return null;
}
function Hn(t, e, r = e.selection.ranges) {
  let n = [], o = -1;
  for (let { from: i, to: s } of r) {
    let l = n.length, c = 1e9, h = Ee(e, i).line;
    if (h) {
      for (let a = i; a <= s; ) {
        let u = e.doc.lineAt(a);
        if (u.from > o && (i == s || s > u.from)) {
          o = u.from;
          let f = /^\s*/.exec(u.text)[0].length, m = f == u.length, L = u.text.slice(f, f + h.length) == h ? f : -1;
          f < u.text.length && f < c && (c = f), n.push({ line: u, comment: L, token: h, indent: f, empty: m, single: !1 });
        }
        a = u.to + 1;
      }
      if (c < 1e9)
        for (let a = l; a < n.length; a++)
          n[a].indent < n[a].line.text.length && (n[a].indent = c);
      n.length == l + 1 && (n[l].single = !0);
    }
  }
  if (t != 2 && n.some((i) => i.comment < 0 && (!i.empty || i.single))) {
    let i = [];
    for (let { line: l, token: c, indent: h, empty: a, single: u } of n)
      (u || !a) && i.push({ from: l.from + h, insert: c + " " });
    let s = e.changes(i);
    return { changes: s, selection: e.selection.map(s, 1) };
  } else if (t != 1 && n.some((i) => i.comment >= 0)) {
    let i = [];
    for (let { line: s, comment: l, token: c } of n)
      if (l >= 0) {
        let h = s.from + l, a = h + c.length;
        s.text[a - s.from] == " " && a++, i.push({ from: h, to: a });
      }
    return { changes: i };
  }
  return null;
}
const Ae = /* @__PURE__ */ ot.define(), Jn = /* @__PURE__ */ ot.define(), _n = /* @__PURE__ */ Z.define(), ut = /* @__PURE__ */ Z.define({
  combine(t) {
    return ue(t, {
      minDepth: 100,
      newGroupDelay: 500,
      joinToEvent: (e, r) => r
    }, {
      minDepth: Math.max,
      newGroupDelay: Math.min,
      joinToEvent: (e, r) => (n, o) => e(n, o) || r(n, o)
    });
  }
}), ft = /* @__PURE__ */ he.define({
  create() {
    return T.empty;
  },
  update(t, e) {
    let r = e.state.facet(ut), n = e.annotation(Ae);
    if (n) {
      let c = S.fromTransaction(e, n.selection), h = n.side, a = h == 0 ? t.undone : t.done;
      return c ? a = oe(a, a.length, r.minDepth, c) : a = pt(a, e.startState.selection), new T(h == 0 ? n.rest : a, h == 0 ? a : n.rest);
    }
    let o = e.annotation(Jn);
    if ((o == "full" || o == "before") && (t = t.isolate()), e.annotation(ge.addToHistory) === !1)
      return e.changes.empty ? t : t.addMapping(e.changes.desc);
    let i = S.fromTransaction(e), s = e.annotation(ge.time), l = e.annotation(ge.userEvent);
    return i ? t = t.addChanges(i, s, l, r, e) : e.selection && (t = t.addSelection(e.startState.selection, s, l, r.newGroupDelay)), (o == "full" || o == "after") && (t = t.isolate()), t;
  },
  toJSON(t) {
    return { done: t.done.map((e) => e.toJSON()), undone: t.undone.map((e) => e.toJSON()) };
  },
  fromJSON(t) {
    return new T(t.done.map(S.fromJSON), t.undone.map(S.fromJSON));
  }
});
function Kn(t = {}) {
  return [
    ft,
    ut.of(t),
    x.domEventHandlers({
      beforeinput(e, r) {
        let n = e.inputType == "historyUndo" ? dt : e.inputType == "historyRedo" ? Ce : null;
        return n ? (e.preventDefault(), n(r)) : !1;
      }
    })
  ];
}
function fe(t, e) {
  return function({ state: r, dispatch: n }) {
    if (!e && r.readOnly)
      return !1;
    let o = r.field(ft, !1);
    if (!o)
      return !1;
    let i = o.pop(t, r, e);
    return i ? (n(i), !0) : !1;
  };
}
const dt = /* @__PURE__ */ fe(0, !1), Ce = /* @__PURE__ */ fe(1, !1), Qn = /* @__PURE__ */ fe(0, !0), Zn = /* @__PURE__ */ fe(1, !0);
class S {
  constructor(e, r, n, o, i) {
    this.changes = e, this.effects = r, this.mapped = n, this.startSelection = o, this.selectionsAfter = i;
  }
  setSelAfter(e) {
    return new S(this.changes, this.effects, this.mapped, this.startSelection, e);
  }
  toJSON() {
    var e, r, n;
    return {
      changes: (e = this.changes) === null || e === void 0 ? void 0 : e.toJSON(),
      mapped: (r = this.mapped) === null || r === void 0 ? void 0 : r.toJSON(),
      startSelection: (n = this.startSelection) === null || n === void 0 ? void 0 : n.toJSON(),
      selectionsAfter: this.selectionsAfter.map((o) => o.toJSON())
    };
  }
  static fromJSON(e) {
    return new S(e.changes && rn.fromJSON(e.changes), [], e.mapped && on.fromJSON(e.mapped), e.startSelection && d.fromJSON(e.startSelection), e.selectionsAfter.map(d.fromJSON));
  }
  // This does not check `addToHistory` and such, it assumes the
  // transaction needs to be converted to an item. Returns null when
  // there are no changes or effects in the transaction.
  static fromTransaction(e, r) {
    let n = M;
    for (let o of e.startState.facet(_n)) {
      let i = o(e);
      i.length && (n = n.concat(i));
    }
    return !n.length && e.changes.empty ? null : new S(e.changes.invert(e.startState.doc), n, void 0, r || e.startState.selection, M);
  }
  static selection(e) {
    return new S(void 0, M, void 0, void 0, e);
  }
}
function oe(t, e, r, n) {
  let o = e + 1 > r + 20 ? e - r - 1 : 0, i = t.slice(o, e);
  return i.push(n), i;
}
function jn(t, e) {
  let r = [], n = !1;
  return t.iterChangedRanges((o, i) => r.push(o, i)), e.iterChangedRanges((o, i, s, l) => {
    for (let c = 0; c < r.length; ) {
      let h = r[c++], a = r[c++];
      l >= h && s <= a && (n = !0);
    }
  }), n;
}
function Yn(t, e) {
  return t.ranges.length == e.ranges.length && t.ranges.filter((r, n) => r.empty != e.ranges[n].empty).length === 0;
}
function mt(t, e) {
  return t.length ? e.length ? t.concat(e) : t : e;
}
const M = [], Xn = 200;
function pt(t, e) {
  if (t.length) {
    let r = t[t.length - 1], n = r.selectionsAfter.slice(Math.max(0, r.selectionsAfter.length - Xn));
    return n.length && n[n.length - 1].eq(e) ? t : (n.push(e), oe(t, t.length - 1, 1e9, r.setSelAfter(n)));
  } else
    return [S.selection([e])];
}
function er(t) {
  let e = t[t.length - 1], r = t.slice();
  return r[t.length - 1] = e.setSelAfter(e.selectionsAfter.slice(0, e.selectionsAfter.length - 1)), r;
}
function ye(t, e) {
  if (!t.length)
    return t;
  let r = t.length, n = M;
  for (; r; ) {
    let o = tr(t[r - 1], e, n);
    if (o.changes && !o.changes.empty || o.effects.length) {
      let i = t.slice(0, r);
      return i[r - 1] = o, i;
    } else
      e = o.mapped, r--, n = o.selectionsAfter;
  }
  return n.length ? [S.selection(n)] : M;
}
function tr(t, e, r) {
  let n = mt(t.selectionsAfter.length ? t.selectionsAfter.map((l) => l.map(e)) : M, r);
  if (!t.changes)
    return S.selection(n);
  let o = t.changes.map(e), i = e.mapDesc(t.changes, !0), s = t.mapped ? t.mapped.composeDesc(i) : i;
  return new S(o, D.mapEffects(t.effects, e), s, t.startSelection.map(i), n);
}
const nr = /^(input\.type|delete)($|\.)/;
class T {
  constructor(e, r, n = 0, o = void 0) {
    this.done = e, this.undone = r, this.prevTime = n, this.prevUserEvent = o;
  }
  isolate() {
    return this.prevTime ? new T(this.done, this.undone) : this;
  }
  addChanges(e, r, n, o, i) {
    let s = this.done, l = s[s.length - 1];
    return l && l.changes && !l.changes.empty && e.changes && (!n || nr.test(n)) && (!l.selectionsAfter.length && r - this.prevTime < o.newGroupDelay && o.joinToEvent(i, jn(l.changes, e.changes)) || // For compose (but not compose.start) events, always join with previous event
    n == "input.type.compose") ? s = oe(s, s.length - 1, o.minDepth, new S(e.changes.compose(l.changes), mt(D.mapEffects(e.effects, l.changes), l.effects), l.mapped, l.startSelection, M)) : s = oe(s, s.length, o.minDepth, e), new T(s, M, r, n);
  }
  addSelection(e, r, n, o) {
    let i = this.done.length ? this.done[this.done.length - 1].selectionsAfter : M;
    return i.length > 0 && r - this.prevTime < o && n == this.prevUserEvent && n && /^select($|\.)/.test(n) && Yn(i[i.length - 1], e) ? this : new T(pt(this.done, e), this.undone, r, n);
  }
  addMapping(e) {
    return new T(ye(this.done, e), ye(this.undone, e), this.prevTime, this.prevUserEvent);
  }
  pop(e, r, n) {
    let o = e == 0 ? this.done : this.undone;
    if (o.length == 0)
      return null;
    let i = o[o.length - 1], s = i.selectionsAfter[0] || r.selection;
    if (n && i.selectionsAfter.length)
      return r.update({
        selection: i.selectionsAfter[i.selectionsAfter.length - 1],
        annotations: Ae.of({ side: e, rest: er(o), selection: s }),
        userEvent: e == 0 ? "select.undo" : "select.redo",
        scrollIntoView: !0
      });
    if (i.changes) {
      let l = o.length == 1 ? M : o.slice(0, o.length - 1);
      return i.mapped && (l = ye(l, i.mapped)), r.update({
        changes: i.changes,
        selection: i.startSelection,
        effects: i.effects,
        annotations: Ae.of({ side: e, rest: l, selection: s }),
        filter: !1,
        userEvent: e == 0 ? "undo" : "redo",
        scrollIntoView: !0
      });
    } else
      return null;
  }
}
T.empty = /* @__PURE__ */ new T(M, M);
const rr = [
  { key: "Mod-z", run: dt, preventDefault: !0 },
  { key: "Mod-y", mac: "Mod-Shift-z", run: Ce, preventDefault: !0 },
  { linux: "Ctrl-Shift-z", run: Ce, preventDefault: !0 },
  { key: "Mod-u", run: Qn, preventDefault: !0 },
  { key: "Alt-u", mac: "Mod-Shift-u", run: Zn, preventDefault: !0 }
];
function U(t, e) {
  return d.create(t.ranges.map(e), t.mainIndex);
}
function R(t, e) {
  return t.update({ selection: e, scrollIntoView: !0, userEvent: "select" });
}
function B({ state: t, dispatch: e }, r) {
  let n = U(t.selection, r);
  return n.eq(t.selection, !0) ? !1 : (e(R(t, n)), !0);
}
function de(t, e) {
  return d.cursor(e ? t.to : t.from);
}
function gt(t, e) {
  return B(t, (r) => r.empty ? t.moveByChar(r, e) : de(r, e));
}
function k(t) {
  return t.textDirectionAt(t.state.selection.main.head) == sn.LTR;
}
const yt = (t) => gt(t, !k(t)), xt = (t) => gt(t, k(t));
function kt(t, e) {
  return B(t, (r) => r.empty ? t.moveByGroup(r, e) : de(r, e));
}
const or = (t) => kt(t, !k(t)), ir = (t) => kt(t, k(t));
function sr(t, e, r) {
  if (e.type.prop(r))
    return !0;
  let n = e.to - e.from;
  return n && (n > 2 || /[^\s,.;:]/.test(t.sliceDoc(e.from, e.to))) || e.firstChild;
}
function me(t, e, r) {
  let n = we(t).resolveInner(e.head), o = r ? Se.closedBy : Se.openedBy;
  for (let c = e.head; ; ) {
    let h = r ? n.childAfter(c) : n.childBefore(c);
    if (!h)
      break;
    sr(t, h, o) ? n = h : c = r ? h.to : h.from;
  }
  let i = n.type.prop(o), s, l;
  return i && (s = r ? W(t, n.from, 1) : W(t, n.to, -1)) && s.matched ? l = r ? s.end.to : s.end.from : l = r ? n.to : n.from, d.cursor(l, r ? -1 : 1);
}
const lr = (t) => B(t, (e) => me(t.state, e, !k(t))), cr = (t) => B(t, (e) => me(t.state, e, k(t)));
function St(t, e) {
  return B(t, (r) => {
    if (!r.empty)
      return de(r, e);
    let n = t.moveVertically(r, e);
    return n.head != r.head ? n : t.moveToLineBoundary(r, e);
  });
}
const bt = (t) => St(t, !1), At = (t) => St(t, !0);
function Ct(t) {
  let e = t.scrollDOM.clientHeight < t.scrollDOM.scrollHeight - 2, r = 0, n = 0, o;
  if (e) {
    for (let i of t.state.facet(x.scrollMargins)) {
      let s = i(t);
      s != null && s.top && (r = Math.max(s == null ? void 0 : s.top, r)), s != null && s.bottom && (n = Math.max(s == null ? void 0 : s.bottom, n));
    }
    o = t.scrollDOM.clientHeight - r - n;
  } else
    o = (t.dom.ownerDocument.defaultView || window).innerHeight;
  return {
    marginTop: r,
    marginBottom: n,
    selfScroll: e,
    height: Math.max(t.defaultLineHeight, o - 5)
  };
}
function Mt(t, e) {
  let r = Ct(t), { state: n } = t, o = U(n.selection, (s) => s.empty ? t.moveVertically(s, e, r.height) : de(s, e));
  if (o.eq(n.selection))
    return !1;
  let i;
  if (r.selfScroll) {
    let s = t.coordsAtPos(n.selection.main.head), l = t.scrollDOM.getBoundingClientRect(), c = l.top + r.marginTop, h = l.bottom - r.marginBottom;
    s && s.top > c && s.bottom < h && (i = x.scrollIntoView(o.main.head, { y: "start", yMargin: s.top - c }));
  }
  return t.dispatch(R(n, o), { effects: i }), !0;
}
const $e = (t) => Mt(t, !1), Me = (t) => Mt(t, !0);
function I(t, e, r) {
  let n = t.lineBlockAt(e.head), o = t.moveToLineBoundary(e, r);
  if (o.head == e.head && o.head != (r ? n.to : n.from) && (o = t.moveToLineBoundary(e, r, !1)), !r && o.head == n.from && n.length) {
    let i = /^\s*/.exec(t.state.sliceDoc(n.from, Math.min(n.from + 100, n.to)))[0].length;
    i && e.head != n.from + i && (o = d.cursor(n.from + i));
  }
  return o;
}
const ar = (t) => B(t, (e) => I(t, e, !0)), hr = (t) => B(t, (e) => I(t, e, !1)), ur = (t) => B(t, (e) => I(t, e, !k(t))), fr = (t) => B(t, (e) => I(t, e, k(t))), dr = (t) => B(t, (e) => d.cursor(t.lineBlockAt(e.head).from, 1)), mr = (t) => B(t, (e) => d.cursor(t.lineBlockAt(e.head).to, -1));
function pr(t, e, r) {
  let n = !1, o = U(t.selection, (i) => {
    let s = W(t, i.head, -1) || W(t, i.head, 1) || i.head > 0 && W(t, i.head - 1, 1) || i.head < t.doc.length && W(t, i.head + 1, -1);
    if (!s || !s.end)
      return i;
    n = !0;
    let l = s.start.from == i.head ? s.end.to : s.end.from;
    return d.cursor(l);
  });
  return n ? (e(R(t, o)), !0) : !1;
}
const gr = ({ state: t, dispatch: e }) => pr(t, e);
function v(t, e) {
  let r = U(t.state.selection, (n) => {
    let o = e(n);
    return d.range(n.anchor, o.head, o.goalColumn, o.bidiLevel || void 0);
  });
  return r.eq(t.state.selection) ? !1 : (t.dispatch(R(t.state, r)), !0);
}
function vt(t, e) {
  return v(t, (r) => t.moveByChar(r, e));
}
const Lt = (t) => vt(t, !k(t)), Dt = (t) => vt(t, k(t));
function Bt(t, e) {
  return v(t, (r) => t.moveByGroup(r, e));
}
const yr = (t) => Bt(t, !k(t)), xr = (t) => Bt(t, k(t)), kr = (t) => v(t, (e) => me(t.state, e, !k(t))), Sr = (t) => v(t, (e) => me(t.state, e, k(t)));
function wt(t, e) {
  return v(t, (r) => t.moveVertically(r, e));
}
const Tt = (t) => wt(t, !1), Rt = (t) => wt(t, !0);
function Et(t, e) {
  return v(t, (r) => t.moveVertically(r, e, Ct(t).height));
}
const Ve = (t) => Et(t, !1), Ue = (t) => Et(t, !0), br = (t) => v(t, (e) => I(t, e, !0)), Ar = (t) => v(t, (e) => I(t, e, !1)), Cr = (t) => v(t, (e) => I(t, e, !k(t))), Mr = (t) => v(t, (e) => I(t, e, k(t))), vr = (t) => v(t, (e) => d.cursor(t.lineBlockAt(e.head).from)), Lr = (t) => v(t, (e) => d.cursor(t.lineBlockAt(e.head).to)), ze = ({ state: t, dispatch: e }) => (e(R(t, { anchor: 0 })), !0), Ge = ({ state: t, dispatch: e }) => (e(R(t, { anchor: t.doc.length })), !0), He = ({ state: t, dispatch: e }) => (e(R(t, { anchor: t.selection.main.anchor, head: 0 })), !0), Je = ({ state: t, dispatch: e }) => (e(R(t, { anchor: t.selection.main.anchor, head: t.doc.length })), !0), Dr = ({ state: t, dispatch: e }) => (e(t.update({ selection: { anchor: 0, head: t.doc.length }, userEvent: "select" })), !0), Br = ({ state: t, dispatch: e }) => {
  let r = pe(t).map(({ from: n, to: o }) => d.range(n, Math.min(o + 1, t.doc.length)));
  return e(t.update({ selection: d.create(r), userEvent: "select" })), !0;
}, wr = ({ state: t, dispatch: e }) => {
  let r = U(t.selection, (n) => {
    let o = we(t), i = o.resolveStack(n.from, 1);
    if (n.empty) {
      let s = o.resolveStack(n.from, -1);
      s.node.from >= i.node.from && s.node.to <= i.node.to && (i = s);
    }
    for (let s = i; s; s = s.next) {
      let { node: l } = s;
      if ((l.from < n.from && l.to >= n.to || l.to > n.to && l.from <= n.from) && s.next)
        return d.range(l.to, l.from);
    }
    return n;
  });
  return r.eq(t.selection) ? !1 : (e(R(t, r)), !0);
}, Tr = ({ state: t, dispatch: e }) => {
  let r = t.selection, n = null;
  return r.ranges.length > 1 ? n = d.create([r.main]) : r.main.empty || (n = d.create([d.cursor(r.main.head)])), n ? (e(R(t, n)), !0) : !1;
};
function j(t, e) {
  if (t.state.readOnly)
    return !1;
  let r = "delete.selection", { state: n } = t, o = n.changeByRange((i) => {
    let { from: s, to: l } = i;
    if (s == l) {
      let c = e(i);
      c < s ? (r = "delete.backward", c = X(t, c, !1)) : c > s && (r = "delete.forward", c = X(t, c, !0)), s = Math.min(s, c), l = Math.max(l, c);
    } else
      s = X(t, s, !1), l = X(t, l, !0);
    return s == l ? { range: i } : { changes: { from: s, to: l }, range: d.cursor(s, s < i.head ? -1 : 1) };
  });
  return o.changes.empty ? !1 : (t.dispatch(n.update(o, {
    scrollIntoView: !0,
    userEvent: r,
    effects: r == "delete.selection" ? x.announce.of(n.phrase("Selection deleted")) : void 0
  })), !0);
}
function X(t, e, r) {
  if (t instanceof x)
    for (let n of t.state.facet(x.atomicRanges).map((o) => o(t)))
      n.between(e, e, (o, i) => {
        o < e && i > e && (e = r ? i : o);
      });
  return e;
}
const It = (t, e, r) => j(t, (n) => {
  let o = n.from, { state: i } = t, s = i.doc.lineAt(o), l, c;
  if (r && !e && o > s.from && o < s.from + 200 && !/[^ \t]/.test(l = s.text.slice(0, o - s.from))) {
    if (l[l.length - 1] == "	")
      return o - 1;
    let h = Be(l, i.tabSize), a = h % be(i) || be(i);
    for (let u = 0; u < a && l[l.length - 1 - u] == " "; u++)
      o--;
    c = o;
  } else
    c = O(s.text, o - s.from, e, e) + s.from, c == o && s.number != (e ? i.doc.lines : 1) ? c += e ? 1 : -1 : !e && /[\ufe00-\ufe0f]/.test(s.text.slice(c - s.from, o - s.from)) && (c = O(s.text, c - s.from, !1, !1) + s.from);
  return c;
}), ve = (t) => It(t, !1, !0), Pt = (t) => It(t, !0, !1), Ot = (t, e) => j(t, (r) => {
  let n = r.head, { state: o } = t, i = o.doc.lineAt(n), s = o.charCategorizer(n);
  for (let l = null; ; ) {
    if (n == (e ? i.to : i.from)) {
      n == r.head && i.number != (e ? o.doc.lines : 1) && (n += e ? 1 : -1);
      break;
    }
    let c = O(i.text, n - i.from, e) + i.from, h = i.text.slice(Math.min(n, c) - i.from, Math.max(n, c) - i.from), a = s(h);
    if (l != null && a != l)
      break;
    (h != " " || n != r.head) && (l = a), n = c;
  }
  return n;
}), qt = (t) => Ot(t, !1), Rr = (t) => Ot(t, !0), Er = (t) => j(t, (e) => {
  let r = t.lineBlockAt(e.head).to;
  return e.head < r ? r : Math.min(t.state.doc.length, e.head + 1);
}), Ir = (t) => j(t, (e) => {
  let r = t.moveToLineBoundary(e, !1).head;
  return e.head > r ? r : Math.max(0, e.head - 1);
}), Pr = (t) => j(t, (e) => {
  let r = t.moveToLineBoundary(e, !0).head;
  return e.head < r ? r : Math.min(t.state.doc.length, e.head + 1);
}), Or = ({ state: t, dispatch: e }) => {
  if (t.readOnly)
    return !1;
  let r = t.changeByRange((n) => ({
    changes: { from: n.from, to: n.to, insert: rt.of(["", ""]) },
    range: d.cursor(n.from)
  }));
  return e(t.update(r, { scrollIntoView: !0, userEvent: "input" })), !0;
}, qr = ({ state: t, dispatch: e }) => {
  if (t.readOnly)
    return !1;
  let r = t.changeByRange((n) => {
    if (!n.empty || n.from == 0 || n.from == t.doc.length)
      return { range: n };
    let o = n.from, i = t.doc.lineAt(o), s = o == i.from ? o - 1 : O(i.text, o - i.from, !1) + i.from, l = o == i.to ? o + 1 : O(i.text, o - i.from, !0) + i.from;
    return {
      changes: { from: s, to: l, insert: t.doc.slice(o, l).append(t.doc.slice(s, o)) },
      range: d.cursor(l)
    };
  });
  return r.changes.empty ? !1 : (e(t.update(r, { scrollIntoView: !0, userEvent: "move.character" })), !0);
};
function pe(t) {
  let e = [], r = -1;
  for (let n of t.selection.ranges) {
    let o = t.doc.lineAt(n.from), i = t.doc.lineAt(n.to);
    if (!n.empty && n.to == i.from && (i = t.doc.lineAt(n.to - 1)), r >= o.number) {
      let s = e[e.length - 1];
      s.to = i.to, s.ranges.push(n);
    } else
      e.push({ from: o.from, to: i.to, ranges: [n] });
    r = i.number + 1;
  }
  return e;
}
function Ft(t, e, r) {
  if (t.readOnly)
    return !1;
  let n = [], o = [];
  for (let i of pe(t)) {
    if (r ? i.to == t.doc.length : i.from == 0)
      continue;
    let s = t.doc.lineAt(r ? i.to + 1 : i.from - 1), l = s.length + 1;
    if (r) {
      n.push({ from: i.to, to: s.to }, { from: i.from, insert: s.text + t.lineBreak });
      for (let c of i.ranges)
        o.push(d.range(Math.min(t.doc.length, c.anchor + l), Math.min(t.doc.length, c.head + l)));
    } else {
      n.push({ from: s.from, to: i.from }, { from: i.to, insert: t.lineBreak + s.text });
      for (let c of i.ranges)
        o.push(d.range(c.anchor - l, c.head - l));
    }
  }
  return n.length ? (e(t.update({
    changes: n,
    scrollIntoView: !0,
    selection: d.create(o, t.selection.mainIndex),
    userEvent: "move.line"
  })), !0) : !1;
}
const Fr = ({ state: t, dispatch: e }) => Ft(t, e, !1), Wr = ({ state: t, dispatch: e }) => Ft(t, e, !0);
function Wt(t, e, r) {
  if (t.readOnly)
    return !1;
  let n = [];
  for (let o of pe(t))
    r ? n.push({ from: o.from, insert: t.doc.slice(o.from, o.to) + t.lineBreak }) : n.push({ from: o.to, insert: t.lineBreak + t.doc.slice(o.from, o.to) });
  return e(t.update({ changes: n, scrollIntoView: !0, userEvent: "input.copyline" })), !0;
}
const Nr = ({ state: t, dispatch: e }) => Wt(t, e, !1), $r = ({ state: t, dispatch: e }) => Wt(t, e, !0), Vr = (t) => {
  if (t.state.readOnly)
    return !1;
  let { state: e } = t, r = e.changes(pe(e).map(({ from: o, to: i }) => (o > 0 ? o-- : i < e.doc.length && i++, { from: o, to: i }))), n = U(e.selection, (o) => {
    let i;
    if (t.lineWrapping) {
      let s = t.lineBlockAt(o.head), l = t.coordsAtPos(o.head, o.assoc || 1);
      l && (i = s.bottom + t.documentTop - l.bottom + t.defaultLineHeight / 2);
    }
    return t.moveVertically(o, !0, i);
  }).map(r);
  return t.dispatch({ changes: r, selection: n, scrollIntoView: !0, userEvent: "delete.line" }), !0;
};
function Ur(t, e) {
  if (/\(\)|\[\]|\{\}/.test(t.sliceDoc(e - 1, e + 1)))
    return { from: e, to: e };
  let r = we(t).resolveInner(e), n = r.childBefore(e), o = r.childAfter(e), i;
  return n && o && n.to <= e && o.from >= e && (i = n.type.prop(Se.closedBy)) && i.indexOf(o.name) > -1 && t.doc.lineAt(n.to).from == t.doc.lineAt(o.from).from && !/\S/.test(t.sliceDoc(n.to, o.from)) ? { from: n.to, to: o.from } : null;
}
const _e = /* @__PURE__ */ Nt(!1), zr = /* @__PURE__ */ Nt(!0);
function Nt(t) {
  return ({ state: e, dispatch: r }) => {
    if (e.readOnly)
      return !1;
    let n = e.changeByRange((o) => {
      let { from: i, to: s } = o, l = e.doc.lineAt(i), c = !t && i == s && Ur(e, i);
      t && (i = s = (s <= l.to ? l : e.doc.lineAt(s)).to);
      let h = new tt(e, { simulateBreak: i, simulateDoubleBreak: !!c }), a = nt(h, i);
      for (a == null && (a = Be(/^\s*/.exec(e.doc.lineAt(i).text)[0], e.tabSize)); s < l.to && /\s/.test(l.text[s - l.from]); )
        s++;
      c ? { from: i, to: s } = c : i > l.from && i < l.from + 100 && !/\S/.test(l.text.slice(0, i)) && (i = l.from);
      let u = ["", re(e, a)];
      return c && u.push(re(e, h.lineIndent(l.from, -1))), {
        changes: { from: i, to: s, insert: rt.of(u) },
        range: d.cursor(i + 1 + u[1].length)
      };
    });
    return r(e.update(n, { scrollIntoView: !0, userEvent: "input" })), !0;
  };
}
function Ie(t, e) {
  let r = -1;
  return t.changeByRange((n) => {
    let o = [];
    for (let s = n.from; s <= n.to; ) {
      let l = t.doc.lineAt(s);
      l.number > r && (n.empty || n.to > l.from) && (e(l, o, n), r = l.number), s = l.to + 1;
    }
    let i = t.changes(o);
    return {
      changes: o,
      range: d.range(i.mapPos(n.anchor, 1), i.mapPos(n.head, 1))
    };
  });
}
const Gr = ({ state: t, dispatch: e }) => {
  if (t.readOnly)
    return !1;
  let r = /* @__PURE__ */ Object.create(null), n = new tt(t, { overrideIndentation: (i) => {
    let s = r[i];
    return s ?? -1;
  } }), o = Ie(t, (i, s, l) => {
    let c = nt(n, i.from);
    if (c == null)
      return;
    /\S/.test(i.text) || (c = 0);
    let h = /^\s*/.exec(i.text)[0], a = re(t, c);
    (h != a || l.from < i.from + h.length) && (r[i.from] = c, s.push({ from: i.from, to: i.from + h.length, insert: a }));
  });
  return o.changes.empty || e(t.update(o, { userEvent: "indent" })), !0;
}, $t = ({ state: t, dispatch: e }) => t.readOnly ? !1 : (e(t.update(Ie(t, (r, n) => {
  n.push({ from: r.from, insert: t.facet(ln) });
}), { userEvent: "input.indent" })), !0), Vt = ({ state: t, dispatch: e }) => t.readOnly ? !1 : (e(t.update(Ie(t, (r, n) => {
  let o = /^\s*/.exec(r.text)[0];
  if (!o)
    return;
  let i = Be(o, t.tabSize), s = 0, l = re(t, Math.max(0, i - be(t)));
  for (; s < o.length && s < l.length && o.charCodeAt(s) == l.charCodeAt(s); )
    s++;
  n.push({ from: r.from + s, to: r.from + o.length, insert: l.slice(s) });
}), { userEvent: "delete.dedent" })), !0), Hr = (t) => (t.setTabFocusMode(), !0), Jr = [
  { key: "Ctrl-b", run: yt, shift: Lt, preventDefault: !0 },
  { key: "Ctrl-f", run: xt, shift: Dt },
  { key: "Ctrl-p", run: bt, shift: Tt },
  { key: "Ctrl-n", run: At, shift: Rt },
  { key: "Ctrl-a", run: dr, shift: vr },
  { key: "Ctrl-e", run: mr, shift: Lr },
  { key: "Ctrl-d", run: Pt },
  { key: "Ctrl-h", run: ve },
  { key: "Ctrl-k", run: Er },
  { key: "Ctrl-Alt-h", run: qt },
  { key: "Ctrl-o", run: Or },
  { key: "Ctrl-t", run: qr },
  { key: "Ctrl-v", run: Me }
], _r = /* @__PURE__ */ [
  { key: "ArrowLeft", run: yt, shift: Lt, preventDefault: !0 },
  { key: "Mod-ArrowLeft", mac: "Alt-ArrowLeft", run: or, shift: yr, preventDefault: !0 },
  { mac: "Cmd-ArrowLeft", run: ur, shift: Cr, preventDefault: !0 },
  { key: "ArrowRight", run: xt, shift: Dt, preventDefault: !0 },
  { key: "Mod-ArrowRight", mac: "Alt-ArrowRight", run: ir, shift: xr, preventDefault: !0 },
  { mac: "Cmd-ArrowRight", run: fr, shift: Mr, preventDefault: !0 },
  { key: "ArrowUp", run: bt, shift: Tt, preventDefault: !0 },
  { mac: "Cmd-ArrowUp", run: ze, shift: He },
  { mac: "Ctrl-ArrowUp", run: $e, shift: Ve },
  { key: "ArrowDown", run: At, shift: Rt, preventDefault: !0 },
  { mac: "Cmd-ArrowDown", run: Ge, shift: Je },
  { mac: "Ctrl-ArrowDown", run: Me, shift: Ue },
  { key: "PageUp", run: $e, shift: Ve },
  { key: "PageDown", run: Me, shift: Ue },
  { key: "Home", run: hr, shift: Ar, preventDefault: !0 },
  { key: "Mod-Home", run: ze, shift: He },
  { key: "End", run: ar, shift: br, preventDefault: !0 },
  { key: "Mod-End", run: Ge, shift: Je },
  { key: "Enter", run: _e, shift: _e },
  { key: "Mod-a", run: Dr },
  { key: "Backspace", run: ve, shift: ve },
  { key: "Delete", run: Pt },
  { key: "Mod-Backspace", mac: "Alt-Backspace", run: qt },
  { key: "Mod-Delete", mac: "Alt-Delete", run: Rr },
  { mac: "Mod-Backspace", run: Ir },
  { mac: "Mod-Delete", run: Pr }
].concat(/* @__PURE__ */ Jr.map((t) => ({ mac: t.key, run: t.run, shift: t.shift }))), Ut = /* @__PURE__ */ [
  { key: "Alt-ArrowLeft", mac: "Ctrl-ArrowLeft", run: lr, shift: kr },
  { key: "Alt-ArrowRight", mac: "Ctrl-ArrowRight", run: cr, shift: Sr },
  { key: "Alt-ArrowUp", run: Fr },
  { key: "Shift-Alt-ArrowUp", run: Nr },
  { key: "Alt-ArrowDown", run: Wr },
  { key: "Shift-Alt-ArrowDown", run: $r },
  { key: "Escape", run: Tr },
  { key: "Mod-Enter", run: zr },
  { key: "Alt-l", mac: "Ctrl-l", run: Br },
  { key: "Mod-i", run: wr, preventDefault: !0 },
  { key: "Mod-[", run: Vt },
  { key: "Mod-]", run: $t },
  { key: "Mod-Alt-\\", run: Gr },
  { key: "Shift-Mod-k", run: Vr },
  { key: "Shift-Mod-\\", run: gr },
  { key: "Mod-/", run: Nn },
  { key: "Alt-A", run: Vn },
  { key: "Ctrl-m", mac: "Shift-Alt-m", run: Hr }
].concat(_r), Kr = { key: "Tab", run: $t, shift: Vt };
function p() {
  var t = arguments[0];
  typeof t == "string" && (t = document.createElement(t));
  var e = 1, r = arguments[1];
  if (r && typeof r == "object" && r.nodeType == null && !Array.isArray(r)) {
    for (var n in r) if (Object.prototype.hasOwnProperty.call(r, n)) {
      var o = r[n];
      typeof o == "string" ? t.setAttribute(n, o) : o != null && (t[n] = o);
    }
    e++;
  }
  for (; e < arguments.length; e++) zt(t, arguments[e]);
  return t;
}
function zt(t, e) {
  if (typeof e == "string")
    t.appendChild(document.createTextNode(e));
  else if (e != null) if (e.nodeType != null)
    t.appendChild(e);
  else if (Array.isArray(e))
    for (var r = 0; r < e.length; r++) zt(t, e[r]);
  else
    throw new RangeError("Unsupported child node: " + e);
}
const Ke = typeof String.prototype.normalize == "function" ? (t) => t.normalize("NFKD") : (t) => t;
class $ {
  /**
  Create a text cursor. The query is the search string, `from` to
  `to` provides the region to search.
  
  When `normalize` is given, it will be called, on both the query
  string and the content it is matched against, before comparing.
  You can, for example, create a case-insensitive search by
  passing `s => s.toLowerCase()`.
  
  Text is always normalized with
  [`.normalize("NFKD")`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/normalize)
  (when supported).
  */
  constructor(e, r, n = 0, o = e.length, i, s) {
    this.test = s, this.value = { from: 0, to: 0 }, this.done = !1, this.matches = [], this.buffer = "", this.bufferPos = 0, this.iter = e.iterRange(n, o), this.bufferStart = n, this.normalize = i ? (l) => i(Ke(l)) : Ke, this.query = this.normalize(r);
  }
  peek() {
    if (this.bufferPos == this.buffer.length) {
      if (this.bufferStart += this.buffer.length, this.iter.next(), this.iter.done)
        return -1;
      this.bufferPos = 0, this.buffer = this.iter.value;
    }
    return an(this.buffer, this.bufferPos);
  }
  /**
  Look for the next match. Updates the iterator's
  [`value`](https://codemirror.net/6/docs/ref/#search.SearchCursor.value) and
  [`done`](https://codemirror.net/6/docs/ref/#search.SearchCursor.done) properties. Should be called
  at least once before using the cursor.
  */
  next() {
    for (; this.matches.length; )
      this.matches.pop();
    return this.nextOverlapping();
  }
  /**
  The `next` method will ignore matches that partially overlap a
  previous match. This method behaves like `next`, but includes
  such matches.
  */
  nextOverlapping() {
    for (; ; ) {
      let e = this.peek();
      if (e < 0)
        return this.done = !0, this;
      let r = un(e), n = this.bufferStart + this.bufferPos;
      this.bufferPos += hn(e);
      let o = this.normalize(r);
      if (o.length)
        for (let i = 0, s = n; ; i++) {
          let l = o.charCodeAt(i), c = this.match(l, s, this.bufferPos + this.bufferStart);
          if (i == o.length - 1) {
            if (c)
              return this.value = c, this;
            break;
          }
          s == n && i < r.length && r.charCodeAt(i) == l && s++;
        }
    }
  }
  match(e, r, n) {
    let o = null;
    for (let i = 0; i < this.matches.length; i += 2) {
      let s = this.matches[i], l = !1;
      this.query.charCodeAt(s) == e && (s == this.query.length - 1 ? o = { from: this.matches[i + 1], to: n } : (this.matches[i]++, l = !0)), l || (this.matches.splice(i, 2), i -= 2);
    }
    return this.query.charCodeAt(0) == e && (this.query.length == 1 ? o = { from: r, to: n } : this.matches.push(1, r)), o && this.test && !this.test(o.from, o.to, this.buffer, this.bufferStart) && (o = null), o;
  }
}
typeof Symbol < "u" && ($.prototype[Symbol.iterator] = function() {
  return this;
});
const Gt = { from: -1, to: -1, match: /* @__PURE__ */ /.*/.exec("") }, Pe = "gm" + (/x/.unicode == null ? "" : "u");
class Ht {
  /**
  Create a cursor that will search the given range in the given
  document. `query` should be the raw pattern (as you'd pass it to
  `new RegExp`).
  */
  constructor(e, r, n, o = 0, i = e.length) {
    if (this.text = e, this.to = i, this.curLine = "", this.done = !1, this.value = Gt, /\\[sWDnr]|\n|\r|\[\^/.test(r))
      return new Jt(e, r, n, o, i);
    this.re = new RegExp(r, Pe + (n != null && n.ignoreCase ? "i" : "")), this.test = n == null ? void 0 : n.test, this.iter = e.iter();
    let s = e.lineAt(o);
    this.curLineStart = s.from, this.matchPos = ie(e, o), this.getLine(this.curLineStart);
  }
  getLine(e) {
    this.iter.next(e), this.iter.lineBreak ? this.curLine = "" : (this.curLine = this.iter.value, this.curLineStart + this.curLine.length > this.to && (this.curLine = this.curLine.slice(0, this.to - this.curLineStart)), this.iter.next());
  }
  nextLine() {
    this.curLineStart = this.curLineStart + this.curLine.length + 1, this.curLineStart > this.to ? this.curLine = "" : this.getLine(0);
  }
  /**
  Move to the next match, if there is one.
  */
  next() {
    for (let e = this.matchPos - this.curLineStart; ; ) {
      this.re.lastIndex = e;
      let r = this.matchPos <= this.to && this.re.exec(this.curLine);
      if (r) {
        let n = this.curLineStart + r.index, o = n + r[0].length;
        if (this.matchPos = ie(this.text, o + (n == o ? 1 : 0)), n == this.curLineStart + this.curLine.length && this.nextLine(), (n < o || n > this.value.to) && (!this.test || this.test(n, o, r)))
          return this.value = { from: n, to: o, match: r }, this;
        e = this.matchPos - this.curLineStart;
      } else if (this.curLineStart + this.curLine.length < this.to)
        this.nextLine(), e = 0;
      else
        return this.done = !0, this;
    }
  }
}
const xe = /* @__PURE__ */ new WeakMap();
class N {
  constructor(e, r) {
    this.from = e, this.text = r;
  }
  get to() {
    return this.from + this.text.length;
  }
  static get(e, r, n) {
    let o = xe.get(e);
    if (!o || o.from >= n || o.to <= r) {
      let l = new N(r, e.sliceString(r, n));
      return xe.set(e, l), l;
    }
    if (o.from == r && o.to == n)
      return o;
    let { text: i, from: s } = o;
    return s > r && (i = e.sliceString(r, s) + i, s = r), o.to < n && (i += e.sliceString(o.to, n)), xe.set(e, new N(s, i)), new N(r, i.slice(r - s, n - s));
  }
}
class Jt {
  constructor(e, r, n, o, i) {
    this.text = e, this.to = i, this.done = !1, this.value = Gt, this.matchPos = ie(e, o), this.re = new RegExp(r, Pe + (n != null && n.ignoreCase ? "i" : "")), this.test = n == null ? void 0 : n.test, this.flat = N.get(e, o, this.chunkEnd(
      o + 5e3
      /* Chunk.Base */
    ));
  }
  chunkEnd(e) {
    return e >= this.to ? this.to : this.text.lineAt(e).to;
  }
  next() {
    for (; ; ) {
      let e = this.re.lastIndex = this.matchPos - this.flat.from, r = this.re.exec(this.flat.text);
      if (r && !r[0] && r.index == e && (this.re.lastIndex = e + 1, r = this.re.exec(this.flat.text)), r) {
        let n = this.flat.from + r.index, o = n + r[0].length;
        if ((this.flat.to >= this.to || r.index + r[0].length <= this.flat.text.length - 10) && (!this.test || this.test(n, o, r)))
          return this.value = { from: n, to: o, match: r }, this.matchPos = ie(this.text, o + (n == o ? 1 : 0)), this;
      }
      if (this.flat.to == this.to)
        return this.done = !0, this;
      this.flat = N.get(this.text, this.flat.from, this.chunkEnd(this.flat.from + this.flat.text.length * 2));
    }
  }
}
typeof Symbol < "u" && (Ht.prototype[Symbol.iterator] = Jt.prototype[Symbol.iterator] = function() {
  return this;
});
function Qr(t) {
  try {
    return new RegExp(t, Pe), !0;
  } catch {
    return !1;
  }
}
function ie(t, e) {
  if (e >= t.length)
    return e;
  let r = t.lineAt(e), n;
  for (; e < r.to && (n = r.text.charCodeAt(e - r.from)) >= 56320 && n < 57344; )
    e++;
  return e;
}
function Le(t) {
  let e = String(t.state.doc.lineAt(t.state.selection.main.head).number), r = p("input", { class: "cm-textfield", name: "line", value: e }), n = p("form", {
    class: "cm-gotoLine",
    onkeydown: (i) => {
      i.keyCode == 27 ? (i.preventDefault(), t.dispatch({ effects: H.of(!1) }), t.focus()) : i.keyCode == 13 && (i.preventDefault(), o());
    },
    onsubmit: (i) => {
      i.preventDefault(), o();
    }
  }, p("label", t.state.phrase("Go to line"), ": ", r), " ", p("button", { class: "cm-button", type: "submit" }, t.state.phrase("go")), p("button", {
    name: "close",
    onclick: () => {
      t.dispatch({ effects: H.of(!1) }), t.focus();
    },
    "aria-label": t.state.phrase("close"),
    type: "button"
  }, ["×"]));
  function o() {
    let i = /^([+-])?(\d+)?(:\d+)?(%)?$/.exec(r.value);
    if (!i)
      return;
    let { state: s } = t, l = s.doc.lineAt(s.selection.main.head), [, c, h, a, u] = i, f = a ? +a.slice(1) : 0, m = h ? +h : l.number;
    if (h && u) {
      let w = m / 100;
      c && (w = w * (c == "-" ? -1 : 1) + l.number / s.doc.lines), m = Math.round(s.doc.lines * w);
    } else h && c && (m = m * (c == "-" ? -1 : 1) + l.number);
    let L = s.doc.line(Math.max(1, Math.min(s.doc.lines, m))), g = d.cursor(L.from + Math.max(0, Math.min(f, L.length)));
    t.dispatch({
      effects: [H.of(!1), x.scrollIntoView(g.from, { y: "center" })],
      selection: g
    }), t.focus();
  }
  return { dom: n };
}
const H = /* @__PURE__ */ D.define(), Qe = /* @__PURE__ */ he.define({
  create() {
    return !0;
  },
  update(t, e) {
    for (let r of e.effects)
      r.is(H) && (t = r.value);
    return t;
  },
  provide: (t) => Te.from(t, (e) => e ? Le : null)
}), Zr = (t) => {
  let e = J(t, Le);
  if (!e) {
    let r = [H.of(!0)];
    t.state.field(Qe, !1) == null && r.push(D.appendConfig.of([Qe, jr])), t.dispatch({ effects: r }), e = J(t, Le);
  }
  return e && e.dom.querySelector("input").select(), !0;
}, jr = /* @__PURE__ */ x.baseTheme({
  ".cm-panel.cm-gotoLine": {
    padding: "2px 6px 4px",
    position: "relative",
    "& label": { fontSize: "80%" },
    "& [name=close]": {
      position: "absolute",
      top: "0",
      bottom: "0",
      right: "4px",
      backgroundColor: "inherit",
      border: "none",
      font: "inherit",
      padding: "0"
    }
  }
}), Yr = {
  highlightWordAroundCursor: !1,
  minSelectionLength: 1,
  maxMatches: 100,
  wholeWords: !1
}, Xr = /* @__PURE__ */ Z.define({
  combine(t) {
    return ue(t, Yr, {
      highlightWordAroundCursor: (e, r) => e || r,
      minSelectionLength: Math.min,
      maxMatches: Math.min
    });
  }
});
function eo(t) {
  return [io, oo];
}
const to = /* @__PURE__ */ y.mark({ class: "cm-selectionMatch" }), no = /* @__PURE__ */ y.mark({ class: "cm-selectionMatch cm-selectionMatch-main" });
function Ze(t, e, r, n) {
  return (r == 0 || t(e.sliceDoc(r - 1, r)) != C.Word) && (n == e.doc.length || t(e.sliceDoc(n, n + 1)) != C.Word);
}
function ro(t, e, r, n) {
  return t(e.sliceDoc(r, r + 1)) == C.Word && t(e.sliceDoc(n - 1, n)) == C.Word;
}
const oo = /* @__PURE__ */ it.fromClass(class {
  constructor(t) {
    this.decorations = this.getDeco(t);
  }
  update(t) {
    (t.selectionSet || t.docChanged || t.viewportChanged) && (this.decorations = this.getDeco(t.view));
  }
  getDeco(t) {
    let e = t.state.facet(Xr), { state: r } = t, n = r.selection;
    if (n.ranges.length > 1)
      return y.none;
    let o = n.main, i, s = null;
    if (o.empty) {
      if (!e.highlightWordAroundCursor)
        return y.none;
      let c = r.wordAt(o.head);
      if (!c)
        return y.none;
      s = r.charCategorizer(o.head), i = r.sliceDoc(c.from, c.to);
    } else {
      let c = o.to - o.from;
      if (c < e.minSelectionLength || c > 200)
        return y.none;
      if (e.wholeWords) {
        if (i = r.sliceDoc(o.from, o.to), s = r.charCategorizer(o.head), !(Ze(s, r, o.from, o.to) && ro(s, r, o.from, o.to)))
          return y.none;
      } else if (i = r.sliceDoc(o.from, o.to), !i)
        return y.none;
    }
    let l = [];
    for (let c of t.visibleRanges) {
      let h = new $(r.doc, i, c.from, c.to);
      for (; !h.next().done; ) {
        let { from: a, to: u } = h.value;
        if ((!s || Ze(s, r, a, u)) && (o.empty && a <= o.from && u >= o.to ? l.push(no.range(a, u)) : (a >= o.to || u <= o.from) && l.push(to.range(a, u)), l.length > e.maxMatches))
          return y.none;
      }
    }
    return y.set(l);
  }
}, {
  decorations: (t) => t.decorations
}), io = /* @__PURE__ */ x.baseTheme({
  ".cm-selectionMatch": { backgroundColor: "#99ff7780" },
  ".cm-searchMatch .cm-selectionMatch": { backgroundColor: "transparent" }
}), so = ({ state: t, dispatch: e }) => {
  let { selection: r } = t, n = d.create(r.ranges.map((o) => t.wordAt(o.head) || d.cursor(o.head)), r.mainIndex);
  return n.eq(r) ? !1 : (e(t.update({ selection: n })), !0);
};
function lo(t, e) {
  let { main: r, ranges: n } = t.selection, o = t.wordAt(r.head), i = o && o.from == r.from && o.to == r.to;
  for (let s = !1, l = new $(t.doc, e, n[n.length - 1].to); ; )
    if (l.next(), l.done) {
      if (s)
        return null;
      l = new $(t.doc, e, 0, Math.max(0, n[n.length - 1].from - 1)), s = !0;
    } else {
      if (s && n.some((c) => c.from == l.value.from))
        continue;
      if (i) {
        let c = t.wordAt(l.value.from);
        if (!c || c.from != l.value.from || c.to != l.value.to)
          continue;
      }
      return l.value;
    }
}
const co = ({ state: t, dispatch: e }) => {
  let { ranges: r } = t.selection;
  if (r.some((i) => i.from === i.to))
    return so({ state: t, dispatch: e });
  let n = t.sliceDoc(r[0].from, r[0].to);
  if (t.selection.ranges.some((i) => t.sliceDoc(i.from, i.to) != n))
    return !1;
  let o = lo(t, n);
  return o ? (e(t.update({
    selection: t.selection.addRange(d.range(o.from, o.to), !1),
    effects: x.scrollIntoView(o.to)
  })), !0) : !1;
}, z = /* @__PURE__ */ Z.define({
  combine(t) {
    return ue(t, {
      top: !1,
      caseSensitive: !1,
      literal: !1,
      regexp: !1,
      wholeWord: !1,
      createPanel: (e) => new bo(e),
      scrollToMatch: (e) => x.scrollIntoView(e)
    });
  }
});
class _t {
  /**
  Create a query object.
  */
  constructor(e) {
    this.search = e.search, this.caseSensitive = !!e.caseSensitive, this.literal = !!e.literal, this.regexp = !!e.regexp, this.replace = e.replace || "", this.valid = !!this.search && (!this.regexp || Qr(this.search)), this.unquoted = this.unquote(this.search), this.wholeWord = !!e.wholeWord;
  }
  /**
  @internal
  */
  unquote(e) {
    return this.literal ? e : e.replace(/\\([nrt\\])/g, (r, n) => n == "n" ? `
` : n == "r" ? "\r" : n == "t" ? "	" : "\\");
  }
  /**
  Compare this query to another query.
  */
  eq(e) {
    return this.search == e.search && this.replace == e.replace && this.caseSensitive == e.caseSensitive && this.regexp == e.regexp && this.wholeWord == e.wholeWord;
  }
  /**
  @internal
  */
  create() {
    return this.regexp ? new fo(this) : new ho(this);
  }
  /**
  Get a search cursor for this query, searching through the given
  range in the given state.
  */
  getCursor(e, r = 0, n) {
    let o = e.doc ? e : st.create({ doc: e });
    return n == null && (n = o.doc.length), this.regexp ? F(this, o, r, n) : q(this, o, r, n);
  }
}
class Kt {
  constructor(e) {
    this.spec = e;
  }
}
function q(t, e, r, n) {
  return new $(e.doc, t.unquoted, r, n, t.caseSensitive ? void 0 : (o) => o.toLowerCase(), t.wholeWord ? ao(e.doc, e.charCategorizer(e.selection.main.head)) : void 0);
}
function ao(t, e) {
  return (r, n, o, i) => ((i > r || i + o.length < n) && (i = Math.max(0, r - 2), o = t.sliceString(i, Math.min(t.length, n + 2))), (e(se(o, r - i)) != C.Word || e(le(o, r - i)) != C.Word) && (e(le(o, n - i)) != C.Word || e(se(o, n - i)) != C.Word));
}
class ho extends Kt {
  constructor(e) {
    super(e);
  }
  nextMatch(e, r, n) {
    let o = q(this.spec, e, n, e.doc.length).nextOverlapping();
    if (o.done) {
      let i = Math.min(e.doc.length, r + this.spec.unquoted.length);
      o = q(this.spec, e, 0, i).nextOverlapping();
    }
    return o.done || o.value.from == r && o.value.to == n ? null : o.value;
  }
  // Searching in reverse is, rather than implementing an inverted search
  // cursor, done by scanning chunk after chunk forward.
  prevMatchInRange(e, r, n) {
    for (let o = n; ; ) {
      let i = Math.max(r, o - 1e4 - this.spec.unquoted.length), s = q(this.spec, e, i, o), l = null;
      for (; !s.nextOverlapping().done; )
        l = s.value;
      if (l)
        return l;
      if (i == r)
        return null;
      o -= 1e4;
    }
  }
  prevMatch(e, r, n) {
    let o = this.prevMatchInRange(e, 0, r);
    return o || (o = this.prevMatchInRange(e, Math.max(0, n - this.spec.unquoted.length), e.doc.length)), o && (o.from != r || o.to != n) ? o : null;
  }
  getReplacement(e) {
    return this.spec.unquote(this.spec.replace);
  }
  matchAll(e, r) {
    let n = q(this.spec, e, 0, e.doc.length), o = [];
    for (; !n.next().done; ) {
      if (o.length >= r)
        return null;
      o.push(n.value);
    }
    return o;
  }
  highlight(e, r, n, o) {
    let i = q(this.spec, e, Math.max(0, r - this.spec.unquoted.length), Math.min(n + this.spec.unquoted.length, e.doc.length));
    for (; !i.next().done; )
      o(i.value.from, i.value.to);
  }
}
function F(t, e, r, n) {
  return new Ht(e.doc, t.search, {
    ignoreCase: !t.caseSensitive,
    test: t.wholeWord ? uo(e.charCategorizer(e.selection.main.head)) : void 0
  }, r, n);
}
function se(t, e) {
  return t.slice(O(t, e, !1), e);
}
function le(t, e) {
  return t.slice(e, O(t, e));
}
function uo(t) {
  return (e, r, n) => !n[0].length || (t(se(n.input, n.index)) != C.Word || t(le(n.input, n.index)) != C.Word) && (t(le(n.input, n.index + n[0].length)) != C.Word || t(se(n.input, n.index + n[0].length)) != C.Word);
}
class fo extends Kt {
  nextMatch(e, r, n) {
    let o = F(this.spec, e, n, e.doc.length).next();
    return o.done && (o = F(this.spec, e, 0, r).next()), o.done ? null : o.value;
  }
  prevMatchInRange(e, r, n) {
    for (let o = 1; ; o++) {
      let i = Math.max(
        r,
        n - o * 1e4
        /* FindPrev.ChunkSize */
      ), s = F(this.spec, e, i, n), l = null;
      for (; !s.next().done; )
        l = s.value;
      if (l && (i == r || l.from > i + 10))
        return l;
      if (i == r)
        return null;
    }
  }
  prevMatch(e, r, n) {
    return this.prevMatchInRange(e, 0, r) || this.prevMatchInRange(e, n, e.doc.length);
  }
  getReplacement(e) {
    return this.spec.unquote(this.spec.replace).replace(/\$([$&]|\d+)/g, (r, n) => {
      if (n == "&")
        return e.match[0];
      if (n == "$")
        return "$";
      for (let o = n.length; o > 0; o--) {
        let i = +n.slice(0, o);
        if (i > 0 && i < e.match.length)
          return e.match[i] + n.slice(o);
      }
      return r;
    });
  }
  matchAll(e, r) {
    let n = F(this.spec, e, 0, e.doc.length), o = [];
    for (; !n.next().done; ) {
      if (o.length >= r)
        return null;
      o.push(n.value);
    }
    return o;
  }
  highlight(e, r, n, o) {
    let i = F(this.spec, e, Math.max(
      0,
      r - 250
      /* RegExp.HighlightMargin */
    ), Math.min(n + 250, e.doc.length));
    for (; !i.next().done; )
      o(i.value.from, i.value.to);
  }
}
const _ = /* @__PURE__ */ D.define(), Oe = /* @__PURE__ */ D.define(), E = /* @__PURE__ */ he.define({
  create(t) {
    return new ke(De(t).create(), null);
  },
  update(t, e) {
    for (let r of e.effects)
      r.is(_) ? t = new ke(r.value.create(), t.panel) : r.is(Oe) && (t = new ke(t.query, r.value ? qe : null));
    return t;
  },
  provide: (t) => Te.from(t, (e) => e.panel)
});
class ke {
  constructor(e, r) {
    this.query = e, this.panel = r;
  }
}
const mo = /* @__PURE__ */ y.mark({ class: "cm-searchMatch" }), po = /* @__PURE__ */ y.mark({ class: "cm-searchMatch cm-searchMatch-selected" }), go = /* @__PURE__ */ it.fromClass(class {
  constructor(t) {
    this.view = t, this.decorations = this.highlight(t.state.field(E));
  }
  update(t) {
    let e = t.state.field(E);
    (e != t.startState.field(E) || t.docChanged || t.selectionSet || t.viewportChanged) && (this.decorations = this.highlight(e));
  }
  highlight({ query: t, panel: e }) {
    if (!e || !t.spec.valid)
      return y.none;
    let { view: r } = this, n = new lt();
    for (let o = 0, i = r.visibleRanges, s = i.length; o < s; o++) {
      let { from: l, to: c } = i[o];
      for (; o < s - 1 && c > i[o + 1].from - 2 * 250; )
        c = i[++o].to;
      t.highlight(r.state, l, c, (h, a) => {
        let u = r.state.selection.ranges.some((f) => f.from == h && f.to == a);
        n.add(h, a, u ? po : mo);
      });
    }
    return n.finish();
  }
}, {
  decorations: (t) => t.decorations
});
function Y(t) {
  return (e) => {
    let r = e.state.field(E, !1);
    return r && r.query.spec.valid ? t(e, r) : jt(e);
  };
}
const ce = /* @__PURE__ */ Y((t, { query: e }) => {
  let { to: r } = t.state.selection.main, n = e.nextMatch(t.state, r, r);
  if (!n)
    return !1;
  let o = d.single(n.from, n.to), i = t.state.facet(z);
  return t.dispatch({
    selection: o,
    effects: [Fe(t, n), i.scrollToMatch(o.main, t)],
    userEvent: "select.search"
  }), Zt(t), !0;
}), ae = /* @__PURE__ */ Y((t, { query: e }) => {
  let { state: r } = t, { from: n } = r.selection.main, o = e.prevMatch(r, n, n);
  if (!o)
    return !1;
  let i = d.single(o.from, o.to), s = t.state.facet(z);
  return t.dispatch({
    selection: i,
    effects: [Fe(t, o), s.scrollToMatch(i.main, t)],
    userEvent: "select.search"
  }), Zt(t), !0;
}), yo = /* @__PURE__ */ Y((t, { query: e }) => {
  let r = e.matchAll(t.state, 1e3);
  return !r || !r.length ? !1 : (t.dispatch({
    selection: d.create(r.map((n) => d.range(n.from, n.to))),
    userEvent: "select.search.matches"
  }), !0);
}), xo = ({ state: t, dispatch: e }) => {
  let r = t.selection;
  if (r.ranges.length > 1 || r.main.empty)
    return !1;
  let { from: n, to: o } = r.main, i = [], s = 0;
  for (let l = new $(t.doc, t.sliceDoc(n, o)); !l.next().done; ) {
    if (i.length > 1e3)
      return !1;
    l.value.from == n && (s = i.length), i.push(d.range(l.value.from, l.value.to));
  }
  return e(t.update({
    selection: d.create(i, s),
    userEvent: "select.search.matches"
  })), !0;
}, je = /* @__PURE__ */ Y((t, { query: e }) => {
  let { state: r } = t, { from: n, to: o } = r.selection.main;
  if (r.readOnly)
    return !1;
  let i = e.nextMatch(r, n, n);
  if (!i)
    return !1;
  let s = i, l = [], c, h, a = [];
  if (s.from == n && s.to == o && (h = r.toText(e.getReplacement(s)), l.push({ from: s.from, to: s.to, insert: h }), s = e.nextMatch(r, s.from, s.to), a.push(x.announce.of(r.phrase("replaced match on line $", r.doc.lineAt(n).number) + "."))), s) {
    let u = l.length == 0 || l[0].from >= i.to ? 0 : i.to - i.from - h.length;
    c = d.single(s.from - u, s.to - u), a.push(Fe(t, s)), a.push(r.facet(z).scrollToMatch(c.main, t));
  }
  return t.dispatch({
    changes: l,
    selection: c,
    effects: a,
    userEvent: "input.replace"
  }), !0;
}), ko = /* @__PURE__ */ Y((t, { query: e }) => {
  if (t.state.readOnly)
    return !1;
  let r = e.matchAll(t.state, 1e9).map((o) => {
    let { from: i, to: s } = o;
    return { from: i, to: s, insert: e.getReplacement(o) };
  });
  if (!r.length)
    return !1;
  let n = t.state.phrase("replaced $ matches", r.length) + ".";
  return t.dispatch({
    changes: r,
    effects: x.announce.of(n),
    userEvent: "input.replace.all"
  }), !0;
});
function qe(t) {
  return t.state.facet(z).createPanel(t);
}
function De(t, e) {
  var r, n, o, i, s;
  let l = t.selection.main, c = l.empty || l.to > l.from + 100 ? "" : t.sliceDoc(l.from, l.to);
  if (e && !c)
    return e;
  let h = t.facet(z);
  return new _t({
    search: ((r = e == null ? void 0 : e.literal) !== null && r !== void 0 ? r : h.literal) ? c : c.replace(/\n/g, "\\n"),
    caseSensitive: (n = e == null ? void 0 : e.caseSensitive) !== null && n !== void 0 ? n : h.caseSensitive,
    literal: (o = e == null ? void 0 : e.literal) !== null && o !== void 0 ? o : h.literal,
    regexp: (i = e == null ? void 0 : e.regexp) !== null && i !== void 0 ? i : h.regexp,
    wholeWord: (s = e == null ? void 0 : e.wholeWord) !== null && s !== void 0 ? s : h.wholeWord
  });
}
function Qt(t) {
  let e = J(t, qe);
  return e && e.dom.querySelector("[main-field]");
}
function Zt(t) {
  let e = Qt(t);
  e && e == t.root.activeElement && e.select();
}
const jt = (t) => {
  let e = t.state.field(E, !1);
  if (e && e.panel) {
    let r = Qt(t);
    if (r && r != t.root.activeElement) {
      let n = De(t.state, e.query.spec);
      n.valid && t.dispatch({ effects: _.of(n) }), r.focus(), r.select();
    }
  } else
    t.dispatch({ effects: [
      Oe.of(!0),
      e ? _.of(De(t.state, e.query.spec)) : D.appendConfig.of(Co)
    ] });
  return !0;
}, Yt = (t) => {
  let e = t.state.field(E, !1);
  if (!e || !e.panel)
    return !1;
  let r = J(t, qe);
  return r && r.dom.contains(t.root.activeElement) && t.focus(), t.dispatch({ effects: Oe.of(!1) }), !0;
}, So = [
  { key: "Mod-f", run: jt, scope: "editor search-panel" },
  { key: "F3", run: ce, shift: ae, scope: "editor search-panel", preventDefault: !0 },
  { key: "Mod-g", run: ce, shift: ae, scope: "editor search-panel", preventDefault: !0 },
  { key: "Escape", run: Yt, scope: "editor search-panel" },
  { key: "Mod-Shift-l", run: xo },
  { key: "Mod-Alt-g", run: Zr },
  { key: "Mod-d", run: co, preventDefault: !0 }
];
class bo {
  constructor(e) {
    this.view = e;
    let r = this.query = e.state.field(E).query.spec;
    this.commit = this.commit.bind(this), this.searchField = p("input", {
      value: r.search,
      placeholder: b(e, "Find"),
      "aria-label": b(e, "Find"),
      class: "cm-textfield",
      name: "search",
      form: "",
      "main-field": "true",
      onchange: this.commit,
      onkeyup: this.commit
    }), this.replaceField = p("input", {
      value: r.replace,
      placeholder: b(e, "Replace"),
      "aria-label": b(e, "Replace"),
      class: "cm-textfield",
      name: "replace",
      form: "",
      onchange: this.commit,
      onkeyup: this.commit
    }), this.caseField = p("input", {
      type: "checkbox",
      name: "case",
      form: "",
      checked: r.caseSensitive,
      onchange: this.commit
    }), this.reField = p("input", {
      type: "checkbox",
      name: "re",
      form: "",
      checked: r.regexp,
      onchange: this.commit
    }), this.wordField = p("input", {
      type: "checkbox",
      name: "word",
      form: "",
      checked: r.wholeWord,
      onchange: this.commit
    });
    function n(o, i, s) {
      return p("button", { class: "cm-button", name: o, onclick: i, type: "button" }, s);
    }
    this.dom = p("div", { onkeydown: (o) => this.keydown(o), class: "cm-search" }, [
      this.searchField,
      n("next", () => ce(e), [b(e, "next")]),
      n("prev", () => ae(e), [b(e, "previous")]),
      n("select", () => yo(e), [b(e, "all")]),
      p("label", null, [this.caseField, b(e, "match case")]),
      p("label", null, [this.reField, b(e, "regexp")]),
      p("label", null, [this.wordField, b(e, "by word")]),
      ...e.state.readOnly ? [] : [
        p("br"),
        this.replaceField,
        n("replace", () => je(e), [b(e, "replace")]),
        n("replaceAll", () => ko(e), [b(e, "replace all")])
      ],
      p("button", {
        name: "close",
        onclick: () => Yt(e),
        "aria-label": b(e, "close"),
        type: "button"
      }, ["×"])
    ]);
  }
  commit() {
    let e = new _t({
      search: this.searchField.value,
      caseSensitive: this.caseField.checked,
      regexp: this.reField.checked,
      wholeWord: this.wordField.checked,
      replace: this.replaceField.value
    });
    e.eq(this.query) || (this.query = e, this.view.dispatch({ effects: _.of(e) }));
  }
  keydown(e) {
    fn(this.view, e, "search-panel") ? e.preventDefault() : e.keyCode == 13 && e.target == this.searchField ? (e.preventDefault(), (e.shiftKey ? ae : ce)(this.view)) : e.keyCode == 13 && e.target == this.replaceField && (e.preventDefault(), je(this.view));
  }
  update(e) {
    for (let r of e.transactions)
      for (let n of r.effects)
        n.is(_) && !n.value.eq(this.query) && this.setQuery(n.value);
  }
  setQuery(e) {
    this.query = e, this.searchField.value = e.search, this.replaceField.value = e.replace, this.caseField.checked = e.caseSensitive, this.reField.checked = e.regexp, this.wordField.checked = e.wholeWord;
  }
  mount() {
    this.searchField.select();
  }
  get pos() {
    return 80;
  }
  get top() {
    return this.view.state.facet(z).top;
  }
}
function b(t, e) {
  return t.state.phrase(e);
}
const ee = 30, te = /[\s\.,:;?!]/;
function Fe(t, { from: e, to: r }) {
  let n = t.state.doc.lineAt(e), o = t.state.doc.lineAt(r).to, i = Math.max(n.from, e - ee), s = Math.min(o, r + ee), l = t.state.sliceDoc(i, s);
  if (i != n.from) {
    for (let c = 0; c < ee; c++)
      if (!te.test(l[c + 1]) && te.test(l[c])) {
        l = l.slice(c);
        break;
      }
  }
  if (s != o) {
    for (let c = l.length - 1; c > l.length - ee; c--)
      if (!te.test(l[c - 1]) && te.test(l[c])) {
        l = l.slice(0, c);
        break;
      }
  }
  return x.announce.of(`${t.state.phrase("current match")}. ${l} ${t.state.phrase("on line")} ${n.number}.`);
}
const Ao = /* @__PURE__ */ x.baseTheme({
  ".cm-panel.cm-search": {
    padding: "2px 6px 4px",
    position: "relative",
    "& [name=close]": {
      position: "absolute",
      top: "0",
      right: "4px",
      backgroundColor: "inherit",
      border: "none",
      font: "inherit",
      padding: 0,
      margin: 0
    },
    "& input, & button, & label": {
      margin: ".2em .6em .2em 0"
    },
    "& input[type=checkbox]": {
      marginRight: ".2em"
    },
    "& label": {
      fontSize: "80%",
      whiteSpace: "pre"
    }
  },
  "&light .cm-searchMatch": { backgroundColor: "#ffff0054" },
  "&dark .cm-searchMatch": { backgroundColor: "#00ffff8a" },
  "&light .cm-searchMatch-selected": { backgroundColor: "#ff6a0054" },
  "&dark .cm-searchMatch-selected": { backgroundColor: "#ff00ff8a" }
}), Co = [
  E,
  /* @__PURE__ */ cn.low(go),
  Ao
];
class Ye {
  constructor(e, r, n) {
    this.from = e, this.to = r, this.diagnostic = n;
  }
}
class P {
  constructor(e, r, n) {
    this.diagnostics = e, this.panel = r, this.selected = n;
  }
  static init(e, r, n) {
    let o = n.facet(K).markerFilter;
    o && (e = o(e, n));
    let i = e.slice().sort((a, u) => a.from - u.from || a.to - u.to), s = new lt(), l = [], c = 0;
    for (let a = 0; ; ) {
      let u = a == i.length ? null : i[a];
      if (!u && !l.length)
        break;
      let f, m;
      for (l.length ? (f = c, m = l.reduce((g, w) => Math.min(g, w.to), u && u.from > f ? u.from : 1e8)) : (f = u.from, m = u.to, l.push(u), a++); a < i.length; ) {
        let g = i[a];
        if (g.from == f && (g.to > g.from || g.to == f))
          l.push(g), a++, m = Math.min(g.to, m);
        else {
          m = Math.min(g.from, m);
          break;
        }
      }
      let L = qo(l);
      if (l.some((g) => g.from == g.to || g.from == g.to - 1 && n.doc.lineAt(g.from).to == g.from))
        s.add(f, f, y.widget({
          widget: new Eo(L),
          diagnostics: l.slice()
        }));
      else {
        let g = l.reduce((w, Ne) => Ne.markClass ? w + " " + Ne.markClass : w, "");
        s.add(f, m, y.mark({
          class: "cm-lintRange cm-lintRange-" + L + g,
          diagnostics: l.slice(),
          inclusiveEnd: l.some((w) => w.to > m)
        }));
      }
      c = m;
      for (let g = 0; g < l.length; g++)
        l[g].to <= c && l.splice(g--, 1);
    }
    let h = s.finish();
    return new P(h, r, V(h));
  }
}
function V(t, e = null, r = 0) {
  let n = null;
  return t.between(r, 1e9, (o, i, { spec: s }) => {
    if (!(e && s.diagnostics.indexOf(e) < 0))
      if (!n)
        n = new Ye(o, i, e || s.diagnostics[0]);
      else {
        if (s.diagnostics.indexOf(n.diagnostic) < 0)
          return !1;
        n = new Ye(n.from, i, n.diagnostic);
      }
  }), n;
}
function Mo(t, e) {
  let r = e.pos, n = e.end || r, o = t.state.facet(K).hideOn(t, r, n);
  if (o != null)
    return o;
  let i = t.startState.doc.lineAt(e.pos);
  return !!(t.effects.some((s) => s.is(Xt)) || t.changes.touchesRange(i.from, Math.max(i.to, n)));
}
function vo(t, e) {
  return t.field(A, !1) ? e : e.concat(D.appendConfig.of(Fo));
}
const Xt = /* @__PURE__ */ D.define(), We = /* @__PURE__ */ D.define(), en = /* @__PURE__ */ D.define(), A = /* @__PURE__ */ he.define({
  create() {
    return new P(y.none, null, null);
  },
  update(t, e) {
    if (e.docChanged && t.diagnostics.size) {
      let r = t.diagnostics.map(e.changes), n = null, o = t.panel;
      if (t.selected) {
        let i = e.changes.mapPos(t.selected.from, 1);
        n = V(r, t.selected.diagnostic, i) || V(r, null, i);
      }
      !r.size && o && e.state.facet(K).autoPanel && (o = null), t = new P(r, o, n);
    }
    for (let r of e.effects)
      if (r.is(Xt)) {
        let n = e.state.facet(K).autoPanel ? r.value.length ? Q.open : null : t.panel;
        t = P.init(r.value, n, e.state);
      } else r.is(We) ? t = new P(t.diagnostics, r.value ? Q.open : null, t.selected) : r.is(en) && (t = new P(t.diagnostics, t.panel, r.value));
    return t;
  },
  provide: (t) => [
    Te.from(t, (e) => e.panel),
    x.decorations.from(t, (e) => e.diagnostics)
  ]
}), Lo = /* @__PURE__ */ y.mark({ class: "cm-lintRange cm-lintRange-active" });
function Do(t, e, r) {
  let { diagnostics: n } = t.state.field(A), o, i = -1, s = -1;
  n.between(e - (r < 0 ? 1 : 0), e + (r > 0 ? 1 : 0), (c, h, { spec: a }) => {
    if (e >= c && e <= h && (c == h || (e > c || r > 0) && (e < h || r < 0)))
      return o = a.diagnostics, i = c, s = h, !1;
  });
  let l = t.state.facet(K).tooltipFilter;
  return o && l && (o = l(o, t.state)), o ? {
    pos: i,
    end: s,
    above: t.state.doc.lineAt(i).to < s,
    create() {
      return { dom: Bo(t, o) };
    }
  } : null;
}
function Bo(t, e) {
  return p("ul", { class: "cm-tooltip-lint" }, e.map((r) => nn(t, r, !1)));
}
const wo = (t) => {
  let e = t.state.field(A, !1);
  (!e || !e.panel) && t.dispatch({ effects: vo(t.state, [We.of(!0)]) });
  let r = J(t, Q.open);
  return r && r.dom.querySelector(".cm-panel-lint ul").focus(), !0;
}, Xe = (t) => {
  let e = t.state.field(A, !1);
  return !e || !e.panel ? !1 : (t.dispatch({ effects: We.of(!1) }), !0);
}, To = (t) => {
  let e = t.state.field(A, !1);
  if (!e)
    return !1;
  let r = t.state.selection.main, n = e.diagnostics.iter(r.to + 1);
  return !n.value && (n = e.diagnostics.iter(0), !n.value || n.from == r.from && n.to == r.to) ? !1 : (t.dispatch({ selection: { anchor: n.from, head: n.to }, scrollIntoView: !0 }), !0);
}, Ro = [
  { key: "Mod-Shift-m", run: wo, preventDefault: !0 },
  { key: "F8", run: To }
], K = /* @__PURE__ */ Z.define({
  combine(t) {
    return Object.assign({ sources: t.map((e) => e.source).filter((e) => e != null) }, ue(t.map((e) => e.config), {
      delay: 750,
      markerFilter: null,
      tooltipFilter: null,
      needsRefresh: null,
      hideOn: () => null
    }, {
      needsRefresh: (e, r) => e ? r ? (n) => e(n) || r(n) : e : r
    }));
  }
});
function tn(t) {
  let e = [];
  if (t)
    e: for (let { name: r } of t) {
      for (let n = 0; n < r.length; n++) {
        let o = r[n];
        if (/[a-zA-Z]/.test(o) && !e.some((i) => i.toLowerCase() == o.toLowerCase())) {
          e.push(o);
          continue e;
        }
      }
      e.push("");
    }
  return e;
}
function nn(t, e, r) {
  var n;
  let o = r ? tn(e.actions) : [];
  return p("li", { class: "cm-diagnostic cm-diagnostic-" + e.severity }, p("span", { class: "cm-diagnosticText" }, e.renderMessage ? e.renderMessage(t) : e.message), (n = e.actions) === null || n === void 0 ? void 0 : n.map((i, s) => {
    let l = !1, c = (f) => {
      if (f.preventDefault(), l)
        return;
      l = !0;
      let m = V(t.state.field(A).diagnostics, e);
      m && i.apply(t, m.from, m.to);
    }, { name: h } = i, a = o[s] ? h.indexOf(o[s]) : -1, u = a < 0 ? h : [
      h.slice(0, a),
      p("u", h.slice(a, a + 1)),
      h.slice(a + 1)
    ];
    return p("button", {
      type: "button",
      class: "cm-diagnosticAction",
      onclick: c,
      onmousedown: c,
      "aria-label": ` Action: ${h}${a < 0 ? "" : ` (access key "${o[s]})"`}.`
    }, u);
  }), e.source && p("div", { class: "cm-diagnosticSource" }, e.source));
}
class Eo extends mn {
  constructor(e) {
    super(), this.sev = e;
  }
  eq(e) {
    return e.sev == this.sev;
  }
  toDOM() {
    return p("span", { class: "cm-lintPoint cm-lintPoint-" + this.sev });
  }
}
class et {
  constructor(e, r) {
    this.diagnostic = r, this.id = "item_" + Math.floor(Math.random() * 4294967295).toString(16), this.dom = nn(e, r, !0), this.dom.id = this.id, this.dom.setAttribute("role", "option");
  }
}
class Q {
  constructor(e) {
    this.view = e, this.items = [];
    let r = (o) => {
      if (o.keyCode == 27)
        Xe(this.view), this.view.focus();
      else if (o.keyCode == 38 || o.keyCode == 33)
        this.moveSelection((this.selectedIndex - 1 + this.items.length) % this.items.length);
      else if (o.keyCode == 40 || o.keyCode == 34)
        this.moveSelection((this.selectedIndex + 1) % this.items.length);
      else if (o.keyCode == 36)
        this.moveSelection(0);
      else if (o.keyCode == 35)
        this.moveSelection(this.items.length - 1);
      else if (o.keyCode == 13)
        this.view.focus();
      else if (o.keyCode >= 65 && o.keyCode <= 90 && this.selectedIndex >= 0) {
        let { diagnostic: i } = this.items[this.selectedIndex], s = tn(i.actions);
        for (let l = 0; l < s.length; l++)
          if (s[l].toUpperCase().charCodeAt(0) == o.keyCode) {
            let c = V(this.view.state.field(A).diagnostics, i);
            c && i.actions[l].apply(e, c.from, c.to);
          }
      } else
        return;
      o.preventDefault();
    }, n = (o) => {
      for (let i = 0; i < this.items.length; i++)
        this.items[i].dom.contains(o.target) && this.moveSelection(i);
    };
    this.list = p("ul", {
      tabIndex: 0,
      role: "listbox",
      "aria-label": this.view.state.phrase("Diagnostics"),
      onkeydown: r,
      onclick: n
    }), this.dom = p("div", { class: "cm-panel-lint" }, this.list, p("button", {
      type: "button",
      name: "close",
      "aria-label": this.view.state.phrase("close"),
      onclick: () => Xe(this.view)
    }, "×")), this.update();
  }
  get selectedIndex() {
    let e = this.view.state.field(A).selected;
    if (!e)
      return -1;
    for (let r = 0; r < this.items.length; r++)
      if (this.items[r].diagnostic == e.diagnostic)
        return r;
    return -1;
  }
  update() {
    let { diagnostics: e, selected: r } = this.view.state.field(A), n = 0, o = !1, i = null, s = /* @__PURE__ */ new Set();
    for (e.between(0, this.view.state.doc.length, (l, c, { spec: h }) => {
      for (let a of h.diagnostics) {
        if (s.has(a))
          continue;
        s.add(a);
        let u = -1, f;
        for (let m = n; m < this.items.length; m++)
          if (this.items[m].diagnostic == a) {
            u = m;
            break;
          }
        u < 0 ? (f = new et(this.view, a), this.items.splice(n, 0, f), o = !0) : (f = this.items[u], u > n && (this.items.splice(n, u - n), o = !0)), r && f.diagnostic == r.diagnostic ? f.dom.hasAttribute("aria-selected") || (f.dom.setAttribute("aria-selected", "true"), i = f) : f.dom.hasAttribute("aria-selected") && f.dom.removeAttribute("aria-selected"), n++;
      }
    }); n < this.items.length && !(this.items.length == 1 && this.items[0].diagnostic.from < 0); )
      o = !0, this.items.pop();
    this.items.length == 0 && (this.items.push(new et(this.view, {
      from: -1,
      to: -1,
      severity: "info",
      message: this.view.state.phrase("No diagnostics")
    })), o = !0), i ? (this.list.setAttribute("aria-activedescendant", i.id), this.view.requestMeasure({
      key: this,
      read: () => ({ sel: i.dom.getBoundingClientRect(), panel: this.list.getBoundingClientRect() }),
      write: ({ sel: l, panel: c }) => {
        let h = c.height / this.list.offsetHeight;
        l.top < c.top ? this.list.scrollTop -= (c.top - l.top) / h : l.bottom > c.bottom && (this.list.scrollTop += (l.bottom - c.bottom) / h);
      }
    })) : this.selectedIndex < 0 && this.list.removeAttribute("aria-activedescendant"), o && this.sync();
  }
  sync() {
    let e = this.list.firstChild;
    function r() {
      let n = e;
      e = n.nextSibling, n.remove();
    }
    for (let n of this.items)
      if (n.dom.parentNode == this.list) {
        for (; e != n.dom; )
          r();
        e = n.dom.nextSibling;
      } else
        this.list.insertBefore(n.dom, e);
    for (; e; )
      r();
  }
  moveSelection(e) {
    if (this.selectedIndex < 0)
      return;
    let r = this.view.state.field(A), n = V(r.diagnostics, this.items[e].diagnostic);
    n && this.view.dispatch({
      selection: { anchor: n.from, head: n.to },
      scrollIntoView: !0,
      effects: en.of(n)
    });
  }
  static open(e) {
    return new Q(e);
  }
}
function Io(t, e = 'viewBox="0 0 40 40"') {
  return `url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" ${e}>${encodeURIComponent(t)}</svg>')`;
}
function ne(t) {
  return Io(`<path d="m0 2.5 l2 -1.5 l1 0 l2 1.5 l1 0" stroke="${t}" fill="none" stroke-width=".7"/>`, 'width="6" height="3"');
}
const Po = /* @__PURE__ */ x.baseTheme({
  ".cm-diagnostic": {
    padding: "3px 6px 3px 8px",
    marginLeft: "-1px",
    display: "block",
    whiteSpace: "pre-wrap"
  },
  ".cm-diagnostic-error": { borderLeft: "5px solid #d11" },
  ".cm-diagnostic-warning": { borderLeft: "5px solid orange" },
  ".cm-diagnostic-info": { borderLeft: "5px solid #999" },
  ".cm-diagnostic-hint": { borderLeft: "5px solid #66d" },
  ".cm-diagnosticAction": {
    font: "inherit",
    border: "none",
    padding: "2px 4px",
    backgroundColor: "#444",
    color: "white",
    borderRadius: "3px",
    marginLeft: "8px",
    cursor: "pointer"
  },
  ".cm-diagnosticSource": {
    fontSize: "70%",
    opacity: 0.7
  },
  ".cm-lintRange": {
    backgroundPosition: "left bottom",
    backgroundRepeat: "repeat-x",
    paddingBottom: "0.7px"
  },
  ".cm-lintRange-error": { backgroundImage: /* @__PURE__ */ ne("#d11") },
  ".cm-lintRange-warning": { backgroundImage: /* @__PURE__ */ ne("orange") },
  ".cm-lintRange-info": { backgroundImage: /* @__PURE__ */ ne("#999") },
  ".cm-lintRange-hint": { backgroundImage: /* @__PURE__ */ ne("#66d") },
  ".cm-lintRange-active": { backgroundColor: "#ffdd9980" },
  ".cm-tooltip-lint": {
    padding: 0,
    margin: 0
  },
  ".cm-lintPoint": {
    position: "relative",
    "&:after": {
      content: '""',
      position: "absolute",
      bottom: 0,
      left: "-2px",
      borderLeft: "3px solid transparent",
      borderRight: "3px solid transparent",
      borderBottom: "4px solid #d11"
    }
  },
  ".cm-lintPoint-warning": {
    "&:after": { borderBottomColor: "orange" }
  },
  ".cm-lintPoint-info": {
    "&:after": { borderBottomColor: "#999" }
  },
  ".cm-lintPoint-hint": {
    "&:after": { borderBottomColor: "#66d" }
  },
  ".cm-panel.cm-panel-lint": {
    position: "relative",
    "& ul": {
      maxHeight: "100px",
      overflowY: "auto",
      "& [aria-selected]": {
        backgroundColor: "#ddd",
        "& u": { textDecoration: "underline" }
      },
      "&:focus [aria-selected]": {
        background_fallback: "#bdf",
        backgroundColor: "Highlight",
        color_fallback: "white",
        color: "HighlightText"
      },
      "& u": { textDecoration: "none" },
      padding: 0,
      margin: 0
    },
    "& [name=close]": {
      position: "absolute",
      top: "0",
      right: "2px",
      background: "inherit",
      border: "none",
      font: "inherit",
      padding: 0,
      margin: 0
    }
  }
});
function Oo(t) {
  return t == "error" ? 4 : t == "warning" ? 3 : t == "info" ? 2 : 1;
}
function qo(t) {
  let e = "hint", r = 1;
  for (let n of t) {
    let o = Oo(n.severity);
    o > r && (r = o, e = n.severity);
  }
  return e;
}
const Fo = [
  A,
  /* @__PURE__ */ x.decorations.compute([A], (t) => {
    let { selected: e, panel: r } = t.field(A);
    return !e || !r || e.from == e.to ? y.none : y.set([
      Lo.range(e.from, e.to)
    ]);
  }),
  /* @__PURE__ */ dn(Do, { hideOn: Mo }),
  Po
], Wo = [
  pn(),
  gn(),
  yn(),
  Kn(),
  xn(),
  kn(),
  Sn(),
  st.allowMultipleSelections.of(!0),
  bn(),
  An(Dn, { fallback: !0 }),
  Cn(),
  wn(),
  Tn(),
  Mn(),
  vn(),
  Ln(),
  eo(),
  ct.of([
    ...Rn,
    ...Ut,
    ...So,
    ...rr,
    ...Bn,
    ...En,
    ...Ro
  ])
], No = at`
  <svg
    xmlns="http://www.w3.org/2000/svg"
    height="24px"
    viewBox="0 -960 960 960"
    width="24px"
  >
    <path
      d="m644-428-58-58q9-47-27-88t-93-32l-58-58q17-8 34.5-12t37.5-4q75 0 127.5 52.5T660-500q0 20-4 37.5T644-428Zm128 126-58-56q38-29 67.5-63.5T832-500q-50-101-143.5-160.5T480-720q-29 0-57 4t-55 12l-62-62q41-17 84-25.5t90-8.5q151 0 269 83.5T920-500q-23 59-60.5 109.5T772-302Zm20 246L624-222q-35 11-70.5 16.5T480-200q-151 0-269-83.5T40-500q21-53 53-98.5t73-81.5L56-792l56-56 736 736-56 56ZM222-624q-29 26-53 57t-41 67q50 101 143.5 160.5T480-280q20 0 39-2.5t39-5.5l-36-38q-11 3-21 4.5t-21 1.5q-75 0-127.5-52.5T300-500q0-11 1.5-21t4.5-21l-84-82Zm319 93Zm-151 75Z"
    />
  </svg>
`, Go = (t, e = {}) => {
  t.config(async (r) => {
    let { languages: n, theme: o } = e;
    if (!n) {
      const { languages: i } = await import("./integration-p001bnNq.js").then((s) => s.bM);
      n = i;
    }
    if (!o) {
      const { oneDark: i } = await import("./index-DuwTHRdp.js");
      o = i;
    }
    r.update(Fn.key, (i) => {
      var s;
      return {
        extensions: [
          ct.of(Ut.concat(Kr)),
          Wo,
          o,
          ...(s = e == null ? void 0 : e.extensions) != null ? s : []
        ],
        languages: n,
        expandIcon: e.expandIcon || (() => On),
        searchIcon: e.searchIcon || (() => Pn),
        clearSearchIcon: e.clearSearchIcon || (() => In),
        searchPlaceholder: e.searchPlaceholder || "Search language",
        noResultText: e.noResultText || "No result",
        renderLanguage: e.renderLanguage || i.renderLanguage,
        renderPreview: e.renderPreview || i.renderPreview,
        previewToggleButton: (l) => {
          var c, h;
          return at`
            ${((c = e.previewToggleIcon) == null ? void 0 : c.call(e, l)) || (l ? qn : No)}
            ${((h = e.previewToggleText) == null ? void 0 : h.call(e, l)) || (l ? "Edit" : "Hide")}
          `;
        },
        previewLabel: e.previewLabel || i.previewLabel
      };
    });
  }).use(Wn);
};
export {
  Go as defineFeature
};
