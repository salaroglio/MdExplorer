import { i as Ln, a as Wn, d as Qe } from "./index-D6fLMv29-BbUkr3pX.js";
import { b as Ye, d as Xe, f as Zt, y as En, z as Tn, h as ct, u as Bn, A as xn, B as Rn, C as Hn, D as Dn, E as An, F as Pn, G as On, H as Nn, I as Gn, J as Fn, K as zn, L as qn, w as Un, M as Kn, N as Vn, O as Jn } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { a as Fe, u as Qn, b as V } from "./hooks-CsyH2QfV.js";
import { au as at, aH as Ze, aJ as je, aI as tn, a_ as Yn, aW as ze, aF as C, a$ as Xn, b0 as Ct, aD as Zn, ax as ee, b1 as jn, aE as ts, b2 as en, b3 as es, b4 as Jt, b5 as J, b6 as ns, b7 as ss, b8 as as, b9 as os, aC as ls, aw as qe, ba as is, bb as rs } from "./integration-yhAlFHEe.js";
import { t as cs } from "./index-BMitY_3H.js";
import { f as nn, o as sn, c as an } from "./floating-ui.dom-C8djGRJz.js";
import { f as ds } from "./index-4u43usGn.js";
import { b as us } from "./index.es-H4vdqDby.js";
var on = (n) => {
  throw TypeError(n);
}, ne = (n, t, e) => t.has(n) || on("Cannot " + e), i = (n, t, e) => (ne(n, t, "read from private field"), e ? e.call(n) : t.get(n)), y = (n, t, e) => t.has(n) ? on("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(n) : t.set(n, e), p = (n, t, e, s) => (ne(n, t, "write to private field"), t.set(n, e), e), hs = (n, t, e) => (ne(n, t, "access private method"), e), yt, St, Q, H, dt, D, z, ln, ut, Y, X, Lt, ht, pt, mt, Wt, Z, W, O, et, nt, Gt, vt, Et, Tt, ft, _t, kt, jt, rn;
function gt(n, t) {
  return Object.assign(n, {
    meta: {
      package: "@milkdown/plugin-block",
      ...t
    }
  }), n;
}
const ps = (n) => !Yn((e) => e.type.name === "table")(n), Ft = at(
  { filterNodes: ps },
  "blockConfig"
);
gt(Ft, {
  displayName: "Ctx<blockConfig>"
});
function ms(n, t, e) {
  var s;
  if (!n.dom.parentElement) return null;
  try {
    const a = (s = n.posAtCoords({
      left: t.x,
      top: t.y
    })) == null ? void 0 : s.inside;
    if (a == null || a < 0) return null;
    let l = n.state.doc.resolve(a), c = n.state.doc.nodeAt(a), u = n.nodeDOM(a);
    const f = (b) => {
      const L = l.depth >= 1 && l.index(l.depth) === 0;
      if (!(b || L)) return;
      const I = l.before(l.depth);
      c = n.state.doc.nodeAt(I), u = n.nodeDOM(I), l = n.state.doc.resolve(I), e(l, c) || f(!0);
    }, g = e(l, c);
    return f(!g), !u || !c ? null : { node: c, $pos: l, el: u };
  } catch {
    return null;
  }
}
let Ue = null;
function vs() {
  return Ue || (Ue = document.implementation.createHTMLDocument("title"));
}
const fs = {
  thead: ["table"],
  tbody: ["table"],
  tfoot: ["table"],
  caption: ["table"],
  colgroup: ["table"],
  col: ["table", "colgroup"],
  tr: ["table", "tbody"],
  td: ["table", "tbody", "tr"],
  th: ["table", "tbody", "tr"]
};
function _s(n, t) {
  const e = [];
  let { openStart: s, openEnd: o, content: a } = t;
  for (; s > 1 && o > 1 && a.childCount === 1 && a.firstChild.childCount === 1; ) {
    s -= 1, o -= 1;
    const _ = a.firstChild;
    e.push(
      _.type.name,
      _.attrs !== _.type.defaultAttrs ? _.attrs : null
    ), a = _.content;
  }
  const l = n.someProp("clipboardSerializer") || Xn.fromSchema(n.state.schema), c = vs(), u = c.createElement("div");
  u.appendChild(l.serializeFragment(a, { document: c }));
  let f = u.firstChild, g, b = 0;
  for (; f && f.nodeType === 1 && (g = fs[f.nodeName.toLowerCase()]); ) {
    for (let _ = g.length - 1; _ >= 0; _--) {
      const I = c.createElement(g[_]);
      for (; u.firstChild; ) I.appendChild(u.firstChild);
      u.appendChild(I), b++;
    }
    f = u.firstChild;
  }
  f && f.nodeType === 1 && f.setAttribute(
    "data-pm-slice",
    `${s} ${o}${b ? ` -${b}` : ""} ${JSON.stringify(e)}`
  );
  const L = n.someProp("clipboardTextSerializer", (_) => _(t, n)) || t.content.textBetween(0, t.content.size, `

`);
  return { dom: u, text: L };
}
const Ke = Ct.ie && Ct.ie_version < 15 || Ct.ios && Ct.webkit_version < 604, Ve = 20;
class ks {
  constructor() {
    y(this, z), y(this, yt), y(this, St), y(this, Q), y(this, H), y(this, dt), y(this, D), y(this, Y), y(this, X), y(this, Lt), y(this, ht), y(this, pt), y(this, mt), y(this, Wt), y(this, Z), p(this, St, () => {
      if (!i(this, H)) return null;
      const t = i(this, H), e = i(this, z, ut);
      if (e && ze.isSelectable(t.node)) {
        const s = ze.create(
          e.state.doc,
          t.$pos.pos
        );
        return e.dispatch(e.state.tr.setSelection(s)), e.focus(), p(this, Q, s), s;
      }
      return null;
    }), p(this, Q, null), p(this, H, null), p(this, dt, void 0), p(this, D, !1), p(this, X, () => {
      var t;
      (t = i(this, Y)) == null || t.call(this, { type: "hide" }), p(this, H, null);
    }), p(this, Lt, (t) => {
      var e;
      p(this, H, t), (e = i(this, Y)) == null || e.call(this, { type: "show", active: t });
    }), this.bind = (t, e) => {
      p(this, yt, t), p(this, Y, e);
    }, this.addEvent = (t) => {
      t.addEventListener("mousedown", i(this, ht)), t.addEventListener("mouseup", i(this, pt)), t.addEventListener("dragstart", i(this, mt));
    }, this.removeEvent = (t) => {
      t.removeEventListener("mousedown", i(this, ht)), t.removeEventListener("mouseup", i(this, pt)), t.removeEventListener("dragstart", i(this, mt));
    }, this.unBind = () => {
      p(this, Y, void 0);
    }, p(this, ht, () => {
      var t;
      p(this, dt, (t = i(this, H)) == null ? void 0 : t.el.getBoundingClientRect()), i(this, St).call(this);
    }), p(this, pt, () => {
      if (!i(this, D)) {
        requestAnimationFrame(() => {
          var t;
          i(this, dt) && ((t = i(this, z, ut)) == null || t.focus());
        });
        return;
      }
      p(this, D, !1), p(this, Q, null);
    }), p(this, mt, (t) => {
      var e;
      p(this, D, !0);
      const s = i(this, z, ut);
      if (!s) return;
      s.dom.dataset.dragging = "true";
      const o = i(this, Q);
      if (t.dataTransfer && o) {
        const a = o.content();
        t.dataTransfer.effectAllowed = "copyMove";
        const { dom: l, text: c } = _s(s, a);
        t.dataTransfer.clearData(), t.dataTransfer.setData(
          Ke ? "Text" : "text/html",
          l.innerHTML
        ), Ke || t.dataTransfer.setData("text/plain", c);
        const u = (e = i(this, H)) == null ? void 0 : e.el;
        u && t.dataTransfer.setDragImage(u, 0, 0), s.dragging = {
          slice: a,
          move: !0
        };
      }
    }), this.keydownCallback = (t) => (i(this, X).call(this), p(this, D, !1), t.dom.dataset.dragging = "false", !1), p(this, Wt, cs((t, e) => {
      if (!t.editable) return;
      const s = t.dom.getBoundingClientRect(), o = s.left + s.width / 2;
      if (!(t.root.elementFromPoint(o, e.clientY) instanceof Element)) {
        i(this, X).call(this);
        return;
      }
      const l = i(this, z, ln);
      if (!l) return;
      const c = ms(
        t,
        { x: o, y: e.clientY },
        l
      );
      if (!c) {
        i(this, X).call(this);
        return;
      }
      i(this, Lt).call(this, c);
    }, 200)), this.mousemoveCallback = (t, e) => (t.composing || !t.editable || i(this, Wt).call(this, t, e), !1), this.dragoverCallback = (t, e) => {
      var s;
      if (i(this, D)) {
        const o = (s = i(this, z, ut)) == null ? void 0 : s.dom.parentElement;
        if (!o) return !1;
        const a = o.scrollHeight > o.clientHeight, l = o.getBoundingClientRect();
        if (a) {
          if (o.scrollTop > 0 && Math.abs(e.y - l.y) < Ve) {
            const f = o.scrollTop > 10 ? o.scrollTop - 10 : 0;
            return o.scrollTop = f, !1;
          }
          const c = Math.round(t.dom.getBoundingClientRect().height);
          if (Math.round(o.scrollTop + l.height) < c && Math.abs(e.y - (l.height + l.y)) < Ve) {
            const f = o.scrollTop + 10;
            return o.scrollTop = f, !1;
          }
        }
      }
      return !1;
    }, this.dragenterCallback = (t) => {
      t.dragging && (p(this, D, !0), t.dom.dataset.dragging = "true");
    }, this.dragleaveCallback = (t, e) => {
      const s = e.clientX, o = e.clientY;
      (s < 0 || o < 0 || s > window.innerWidth || o > window.innerHeight) && (p(this, H, null), i(this, Z).call(this, t));
    }, this.dropCallback = (t) => (i(this, Z).call(this, t), !1), this.dragendCallback = (t) => {
      i(this, Z).call(this, t);
    }, p(this, Z, (t) => {
      p(this, D, !1), t.dom.dataset.dragging = "false";
    });
  }
}
yt = /* @__PURE__ */ new WeakMap();
St = /* @__PURE__ */ new WeakMap();
Q = /* @__PURE__ */ new WeakMap();
H = /* @__PURE__ */ new WeakMap();
dt = /* @__PURE__ */ new WeakMap();
D = /* @__PURE__ */ new WeakMap();
z = /* @__PURE__ */ new WeakSet();
ln = function() {
  var n;
  return (n = i(this, yt)) == null ? void 0 : n.get(Ft.key).filterNodes;
};
ut = function() {
  var n;
  return (n = i(this, yt)) == null ? void 0 : n.get(C);
};
Y = /* @__PURE__ */ new WeakMap();
X = /* @__PURE__ */ new WeakMap();
Lt = /* @__PURE__ */ new WeakMap();
ht = /* @__PURE__ */ new WeakMap();
pt = /* @__PURE__ */ new WeakMap();
mt = /* @__PURE__ */ new WeakMap();
Wt = /* @__PURE__ */ new WeakMap();
Z = /* @__PURE__ */ new WeakMap();
const se = at(() => new ks(), "blockService"), zt = at(
  {},
  "blockServiceInstance"
);
gt(se, {
  displayName: "Ctx<blockService>"
});
gt(zt, {
  displayName: "Ctx<blockServiceInstance>"
});
const qt = at({}, "blockSpec");
gt(qt, {
  displayName: "Ctx<blockSpec>"
});
const ae = Ze((n) => {
  const t = new je("MILKDOWN_BLOCK"), s = n.get(se.key)();
  n.set(zt.key, s);
  const o = n.get(qt.key);
  return new tn({
    key: t,
    ...o,
    props: {
      ...o.props,
      handleDOMEvents: {
        drop: (a) => s.dropCallback(a),
        pointermove: (a, l) => s.mousemoveCallback(a, l),
        keydown: (a) => s.keydownCallback(a),
        dragover: (a, l) => s.dragoverCallback(a, l),
        dragleave: (a, l) => s.dragleaveCallback(a, l),
        dragenter: (a) => s.dragenterCallback(a),
        dragend: (a) => s.dragendCallback(a)
      }
    }
  });
});
gt(ae, {
  displayName: "Prose<block>"
});
class ws {
  constructor(t) {
    y(this, jt), y(this, W), y(this, O), y(this, et), y(this, nt), y(this, Gt), y(this, vt), y(this, Et), y(this, Tt), y(this, ft), y(this, _t), y(this, kt), p(this, nt, null), p(this, vt, !1), this.update = () => {
      requestAnimationFrame(() => {
        if (!i(this, vt))
          try {
            hs(this, jt, rn).call(this), p(this, vt, !0);
          } catch {
          }
      });
    }, this.destroy = () => {
      var e, s;
      (e = i(this, et)) == null || e.unBind(), (s = i(this, et)) == null || s.removeEvent(i(this, W)), i(this, W).remove();
    }, this.show = (e) => {
      const s = e.el, o = i(this, O).get(C).dom, a = {
        ctx: i(this, O),
        active: e,
        editorDom: o,
        blockDom: i(this, W)
      }, l = {
        contextElement: s,
        getBoundingClientRect: () => i(this, _t) ? i(this, _t).call(this, a) : s.getBoundingClientRect()
      }, c = [nn()];
      if (i(this, ft)) {
        const u = i(this, ft).call(this, a), f = sn(u);
        c.push(f);
      }
      an(l, i(this, W), {
        placement: i(this, kt) ? i(this, kt).call(this, a) : "left",
        middleware: [...c, ...i(this, Et)],
        ...i(this, Tt)
      }).then(({ x: u, y: f }) => {
        Object.assign(i(this, W).style, {
          left: `${u}px`,
          top: `${f}px`
        }), i(this, W).dataset.show = "true";
      });
    }, this.hide = () => {
      i(this, W).dataset.show = "false";
    }, p(this, O, t.ctx), p(this, W, t.content), p(this, ft, t.getOffset), p(this, _t, t.getPosition), p(this, kt, t.getPlacement), p(this, Et, t.middleware ?? []), p(this, Tt, t.floatingUIOptions ?? {}), p(this, Gt, t.root), this.hide();
  }
  /// The context of current active node.
  get active() {
    return i(this, nt);
  }
}
W = /* @__PURE__ */ new WeakMap();
O = /* @__PURE__ */ new WeakMap();
et = /* @__PURE__ */ new WeakMap();
nt = /* @__PURE__ */ new WeakMap();
Gt = /* @__PURE__ */ new WeakMap();
vt = /* @__PURE__ */ new WeakMap();
Et = /* @__PURE__ */ new WeakMap();
Tt = /* @__PURE__ */ new WeakMap();
ft = /* @__PURE__ */ new WeakMap();
_t = /* @__PURE__ */ new WeakMap();
kt = /* @__PURE__ */ new WeakMap();
jt = /* @__PURE__ */ new WeakSet();
rn = function() {
  const n = i(this, O).get(C);
  (i(this, Gt) ?? n.dom.parentElement ?? document.body).appendChild(i(this, W));
  const e = i(this, O).get(zt.key);
  e.bind(i(this, O), (s) => {
    s.type === "hide" ? (this.hide(), p(this, nt, null)) : s.type === "show" && (this.show(s.active), p(this, nt, s.active));
  }), p(this, et, e), i(this, et).addEvent(i(this, W)), i(this, W).draggable = !0;
};
const Ut = [
  qt,
  Ft,
  se,
  zt,
  ae
];
Ut.key = qt.key;
Ut.pluginKey = ae.key;
var cn = (n) => {
  throw TypeError(n);
}, oe = (n, t, e) => t.has(n) || cn("Cannot " + e), T = (n, t, e) => (oe(n, t, "read from private field"), t.get(n)), x = (n, t, e) => t.has(n) ? cn("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(n) : t.set(n, e), R = (n, t, e, s) => (oe(n, t, "write to private field"), t.set(n, e), e), ys = (n, t, e) => (oe(n, t, "access private method"), e), wt, Bt, xt, Rt, Ht, st, Dt, At, Pt, te, dn;
function gs(n) {
  const t = at(
    {},
    `${n}_SLASH_SPEC`
  ), e = Ze((o) => {
    const a = o.get(t.key);
    return new tn({
      key: new je(`${n}_SLASH`),
      ...a
    });
  }), s = [t, e];
  return s.key = t.key, s.pluginKey = e.key, t.meta = {
    package: "@milkdown/plugin-slash",
    displayName: `Ctx<slashSpec>|${n}`
  }, e.meta = {
    package: "@milkdown/plugin-slash",
    displayName: `Prose<slash>|${n}`
  }, s;
}
class bs {
  constructor(t) {
    x(this, te), x(this, wt), x(this, Bt), x(this, xt), x(this, Rt), x(this, Ht), x(this, st), x(this, Dt), x(this, At), x(this, Pt), R(this, wt, !1), this.onShow = () => {
    }, this.onHide = () => {
    }, R(this, Pt, (e, s) => {
      const { state: o, composing: a } = e, { selection: l, doc: c } = o, { ranges: u } = l, f = Math.min(...u.map((_) => _.$from.pos)), g = Math.max(...u.map((_) => _.$to.pos)), b = s && s.doc.eq(c) && s.selection.eq(l);
      if (T(this, wt) || ((T(this, Rt) ?? e.dom.parentElement ?? document.body).appendChild(this.element), R(this, wt, !0)), a || b) return;
      if (!T(this, Dt).call(this, e, s)) {
        this.hide();
        return;
      }
      an({
        getBoundingClientRect: () => ts(e, f, g)
      }, this.element, {
        placement: "bottom-start",
        middleware: [nn(), sn(T(this, At)), ...T(this, Bt)],
        ...T(this, xt)
      }).then(({ x: _, y: I }) => {
        Object.assign(this.element.style, {
          left: `${_}px`,
          top: `${I}px`
        });
      }), this.show();
    }), this.update = (e, s) => {
      Zn(T(this, Pt), T(this, Ht))(e, s);
    }, this.getContent = (e, s = (o) => o.type.name === "paragraph") => {
      const { selection: o } = e.state, { empty: a, $from: l } = o, c = e.state.selection instanceof ee, u = this.element.contains(document.activeElement), f = !e.hasFocus() && !u, g = !e.editable, L = !jn(s)(e.state.selection);
      if (!(f || g || !a || !c || L))
        return l.parent.textBetween(
          Math.max(0, l.parentOffset - 500),
          l.parentOffset,
          void 0,
          "￼"
        );
    }, this.destroy = () => {
    }, this.show = () => {
      this.element.dataset.show = "true", this.onShow();
    }, this.hide = () => {
      this.element.dataset.show = "false", this.onHide();
    }, this.element = t.content, R(this, Ht, t.debounce ?? 200), R(this, Dt, t.shouldShow ?? ys(this, te, dn)), R(this, st, t.trigger ?? "/"), R(this, At, t.offset), R(this, Bt, t.middleware ?? []), R(this, xt, t.floatingUIOptions ?? {}), R(this, Rt, t.root);
  }
}
wt = /* @__PURE__ */ new WeakMap();
Bt = /* @__PURE__ */ new WeakMap();
xt = /* @__PURE__ */ new WeakMap();
Rt = /* @__PURE__ */ new WeakMap();
Ht = /* @__PURE__ */ new WeakMap();
st = /* @__PURE__ */ new WeakMap();
Dt = /* @__PURE__ */ new WeakMap();
At = /* @__PURE__ */ new WeakMap();
Pt = /* @__PURE__ */ new WeakMap();
te = /* @__PURE__ */ new WeakSet();
dn = function(n) {
  const t = this.getContent(n);
  if (!t) return !1;
  const e = t.at(-1);
  return e ? Array.isArray(T(this, st)) ? T(this, st).includes(e) : T(this, st) === e : !1;
};
var un = (n) => {
  throw TypeError(n);
}, hn = (n, t, e) => t.has(n) || un("Cannot " + e), it = (n, t, e) => (hn(n, t, "read from private field"), e ? e.call(n) : t.get(n)), Je = (n, t, e) => t.has(n) ? un("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(n) : t.set(n, e), Ms = (n, t, e, s) => (hn(n, t, "write to private field"), t.set(n, e), e), j, Ot;
class Cs {
  constructor() {
    Je(this, j, []), this.clear = () => (Ms(this, j, []), this), Je(this, Ot, (t) => {
      const e = {
        group: t,
        addItem: (s, o) => {
          const a = { key: s, ...o };
          return t.items.push(a), e;
        },
        clear: () => (t.items = [], e)
      };
      return e;
    }), this.addGroup = (t, e) => {
      const o = {
        key: t,
        label: e,
        items: []
      };
      return it(this, j).push(o), it(this, Ot).call(this, o);
    }, this.getGroup = (t) => {
      const e = it(this, j).find((s) => s.key === t);
      if (!e) throw new Error(`Group with key ${t} not found`);
      return it(this, Ot).call(this, e);
    }, this.build = () => it(this, j);
  }
}
j = /* @__PURE__ */ new WeakMap();
Ot = /* @__PURE__ */ new WeakMap();
function Kt(n) {
  const { $from: t, $to: e } = n.selection, { pos: s } = t, { pos: o } = e;
  return n = n.deleteRange(s - t.node().content.size, o), n;
}
function Is(n, t, e = null) {
  const { from: s, to: o } = n.selection;
  return n.setBlockType(s, o, t, e);
}
function $s(n, t, e = null) {
  const { $from: s, $to: o } = n.selection, a = s.blockRange(o), l = a && rs(a, t, e);
  return l ? n.wrap(a, l) : null;
}
function Ss(n, t, e = null) {
  const s = t.createAndFill(e);
  return s ? n.replaceSelectionWith(s) : null;
}
function F(n, t = null) {
  return (e, s) => {
    if (s) {
      const o = Is(Kt(e.tr), n, t);
      s(o.scrollIntoView());
    }
    return !0;
  };
}
function It(n, t = null) {
  return (e, s) => {
    const o = $s(Kt(e.tr), n, t);
    return o ? (s && s(o.scrollIntoView()), !0) : !1;
  };
}
function $t(n, t = null) {
  return (e, s) => {
    const o = Ss(Kt(e.tr), n, t);
    return o ? (s && s(o.scrollIntoView()), !0) : !1;
  };
}
function Ls(n, t, e) {
  var s, o, a, l, c, u, f, g, b, L, _, I, N, G, bt, m, d, w, S, B, ie, re, ce, de, ue, he, pe, me, ve, fe, _e, ke, we, ye, ge, be, Me, Ce, Ie, $e, Se, Le, We, Ee, Te, Be, xe, Re, He, De, Ae, Pe;
  const K = e == null ? void 0 : e.get(es), bn = K == null ? void 0 : K.includes(Jt.Latex), Mn = K == null ? void 0 : K.includes(Jt.ImageBlock), Cn = K == null ? void 0 : K.includes(Jt.Table), ot = new Cs();
  ot.addGroup("text", (s = t == null ? void 0 : t.slashMenuTextGroupLabel) != null ? s : "Text").addItem("text", {
    label: (o = t == null ? void 0 : t.slashMenuTextGroupLabel) != null ? o : "Text",
    icon: (l = (a = t == null ? void 0 : t.slashMenuTextIcon) == null ? void 0 : a.call(t)) != null ? l : xn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(en.type(r))(k, v);
    }
  }).addItem("h1", {
    label: (c = t == null ? void 0 : t.slashMenuH1Label) != null ? c : "Heading 1",
    icon: (f = (u = t == null ? void 0 : t.slashMenuH1Icon) == null ? void 0 : u.call(t)) != null ? f : Rn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 1
      })(k, v);
    }
  }).addItem("h2", {
    label: (g = t == null ? void 0 : t.slashMenuH2Label) != null ? g : "Heading 2",
    icon: (L = (b = t == null ? void 0 : t.slashMenuH2Icon) == null ? void 0 : b.call(t)) != null ? L : Hn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 2
      })(k, v);
    }
  }).addItem("h3", {
    label: (_ = t == null ? void 0 : t.slashMenuH3Label) != null ? _ : "Heading 3",
    icon: (N = (I = t == null ? void 0 : t.slashMenuH3Icon) == null ? void 0 : I.call(t)) != null ? N : Dn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 3
      })(k, v);
    }
  }).addItem("h4", {
    label: (G = t == null ? void 0 : t.slashMenuH4Label) != null ? G : "Heading 4",
    icon: (m = (bt = t == null ? void 0 : t.slashMenuH4Icon) == null ? void 0 : bt.call(t)) != null ? m : An,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 4
      })(k, v);
    }
  }).addItem("h5", {
    label: (d = t == null ? void 0 : t.slashMenuH5Label) != null ? d : "Heading 5",
    icon: (S = (w = t == null ? void 0 : t.slashMenuH5Icon) == null ? void 0 : w.call(t)) != null ? S : Pn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 5
      })(k, v);
    }
  }).addItem("h6", {
    label: (B = t == null ? void 0 : t.slashMenuH6Label) != null ? B : "Heading 6",
    icon: (re = (ie = t == null ? void 0 : t.slashMenuH6Icon) == null ? void 0 : ie.call(t)) != null ? re : On,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      F(J.type(r), {
        level: 6
      })(k, v);
    }
  }).addItem("quote", {
    label: (ce = t == null ? void 0 : t.slashMenuQuoteLabel) != null ? ce : "Quote",
    icon: (ue = (de = t == null ? void 0 : t.slashMenuQuoteIcon) == null ? void 0 : de.call(t)) != null ? ue : Nn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      It(
        ns.type(r)
      )(k, v);
    }
  }).addItem("divider", {
    label: (he = t == null ? void 0 : t.slashMenuDividerLabel) != null ? he : "Divider",
    icon: (me = (pe = t == null ? void 0 : t.slashMenuDividerIcon) == null ? void 0 : pe.call(t)) != null ? me : Gn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      $t(ss.type(r))(k, v);
    }
  }), ot.addGroup("list", (ve = t == null ? void 0 : t.slashMenuListGroupLabel) != null ? ve : "List").addItem("bullet-list", {
    label: (fe = t == null ? void 0 : t.slashMenuBulletListLabel) != null ? fe : "Bullet List",
    icon: (ke = (_e = t == null ? void 0 : t.slashMenuBulletListIcon) == null ? void 0 : _e.call(t)) != null ? ke : Fn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      It(
        as.type(r)
      )(k, v);
    }
  }).addItem("ordered-list", {
    label: (we = t == null ? void 0 : t.slashMenuOrderedListLabel) != null ? we : "Ordered List",
    icon: (ge = (ye = t == null ? void 0 : t.slashMenuOrderedListIcon) == null ? void 0 : ye.call(t)) != null ? ge : zn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      It(
        os.type(r)
      )(k, v);
    }
  }).addItem("todo-list", {
    label: (be = t == null ? void 0 : t.slashMenuTaskListLabel) != null ? be : "Todo List",
    icon: (Ce = (Me = t == null ? void 0 : t.slashMenuTaskListIcon) == null ? void 0 : Me.call(t)) != null ? Ce : qn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      It(
        ls.type(r),
        { checked: !1 }
      )(k, v);
    }
  });
  const Mt = ot.addGroup(
    "advanced",
    (Ie = t == null ? void 0 : t.slashMenuAdvancedGroupLabel) != null ? Ie : "Advanced"
  );
  Mn && Mt.addItem("image", {
    label: ($e = t == null ? void 0 : t.slashMenuImageLabel) != null ? $e : "Image",
    icon: (Le = (Se = t == null ? void 0 : t.slashMenuImageIcon) == null ? void 0 : Se.call(t)) != null ? Le : Un,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      $t(us.type(r))(k, v);
    }
  }), Mt.addItem("code", {
    label: (We = t == null ? void 0 : t.slashMenuCodeBlockLabel) != null ? We : "Code",
    icon: (Te = (Ee = t == null ? void 0 : t.slashMenuCodeBlockIcon) == null ? void 0 : Ee.call(t)) != null ? Te : Kn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      $t(qe.type(r))(k, v);
    }
  }), Cn && Mt.addItem("table", {
    label: (Be = t == null ? void 0 : t.slashMenuTableLabel) != null ? Be : "Table",
    icon: (Re = (xe = t == null ? void 0 : t.slashMenuTableIcon) == null ? void 0 : xe.call(t)) != null ? Re : Vn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      let { tr: M } = k;
      M = Kt(M);
      const Vt = M.selection.from, In = is(r, 3, 3);
      M = M.replaceSelectionWith(In), v(M), requestAnimationFrame(() => {
        const Ne = h.state.doc.content.size, $n = h.state.doc.resolve(
          Vt > Ne ? Ne : Vt < 0 ? 0 : Vt
        ), Sn = ee.near($n), Ge = h.state.tr;
        Ge.setSelection(Sn), v(Ge.scrollIntoView());
      });
    }
  }), bn && Mt.addItem("math", {
    label: (He = t == null ? void 0 : t.slashMenuMathLabel) != null ? He : "Math",
    icon: (Ae = (De = t == null ? void 0 : t.slashMenuMathIcon) == null ? void 0 : De.call(t)) != null ? Ae : Jn,
    onRun: (r) => {
      const h = r.get(C), { dispatch: v, state: k } = h;
      $t(qe.type(r), {
        language: "LaTex"
      })(k, v);
    }
  }), (Pe = t == null ? void 0 : t.buildMenu) == null || Pe.call(t, ot);
  let lt = ot.build();
  n && (lt = lt.map((r) => {
    const h = r.items.filter(
      (v) => v.label.toLowerCase().includes(n.toLowerCase())
    );
    return {
      ...r,
      items: h
    };
  }).filter((r) => r.items.length > 0));
  const Oe = lt.flatMap((r) => r.items);
  return Oe.forEach((r, h) => {
    Object.assign(r, { index: h });
  }), lt.reduce((r, h) => {
    const v = r + h.items.length;
    return Object.assign(h, {
      range: [r, v]
    }), v;
  }, 0), {
    groups: lt,
    size: Oe.length
  };
}
const pn = ({
  show: n,
  hide: t,
  ctx: e,
  filter: s,
  config: o
}) => {
  const { groups: a, size: l } = Fe(
    () => Ls(s, o, e),
    [s]
  ), c = Bn(), [u, f] = Qn(0), g = Fe(() => c.current.getRootNode(), [c]), b = Xe({ x: -999, y: -999 }), L = V((m) => {
    const d = b.current;
    if (!d) return;
    const { x: w, y: S } = m;
    d.x = w, d.y = S;
  }, []);
  Zt(() => {
    l === 0 && n ? t == null || t() : u >= l && f(0);
  }, [l, n]);
  const _ = V(
    (m, d) => {
      f((w) => {
        const S = typeof m == "function" ? m(w) : m;
        return d == null || d(S), S;
      });
    },
    []
  ), I = V((m) => {
    const d = c.current.querySelector(
      `[data-index="${m}"]`
    ), w = c.current.querySelector(".menu-groups");
    !d || !w || (w.scrollTop = d.offsetTop - w.offsetTop);
  }, []), N = V(
    (m) => {
      const d = a.flatMap((w) => w.items).at(m);
      d && e && d.onRun(e), t == null || t();
    },
    [a]
  ), G = V(
    (m) => {
      if (m.key === "Escape") {
        m.preventDefault(), t == null || t();
        return;
      }
      if (m.key === "ArrowDown")
        return m.preventDefault(), _(
          (d) => d < l - 1 ? d + 1 : d,
          I
        );
      if (m.key === "ArrowUp")
        return m.preventDefault(), _(
          (d) => d <= 0 ? d : d - 1,
          I
        );
      if (m.key === "ArrowLeft")
        return m.preventDefault(), _((d) => {
          const w = a.find(
            (B) => B.range[0] <= d && B.range[1] > d
          );
          if (!w) return d;
          const S = a[a.indexOf(w) - 1];
          return S ? S.range[1] - 1 : d;
        }, I);
      if (m.key === "ArrowRight")
        return m.preventDefault(), _((d) => {
          const w = a.find(
            (B) => B.range[0] <= d && B.range[1] > d
          );
          if (!w) return d;
          const S = a[a.indexOf(w) + 1];
          return S ? S.range[0] : d;
        }, I);
      m.key === "Enter" && (m.preventDefault(), N(u));
    },
    [t, a, u]
  ), bt = V((m) => (d) => {
    const w = b.current;
    if (!w) return;
    const { x: S, y: B } = d;
    S === w.x && B === w.y || _(m);
  }, []);
  return Zt(() => (n ? g.addEventListener("keydown", G, { capture: !0 }) : g.removeEventListener("keydown", G, { capture: !0 }), () => {
    g.removeEventListener("keydown", G, { capture: !0 });
  }), [n, G]), ct`
    <host onmousedown=${(m) => m.preventDefault()}>
      <nav class="tab-group">
        <ul>
          ${a.map(
    (m) => ct`<li
                key=${m.key}
                onmousedown=${() => _(m.range[0], I)}
                class=${u >= m.range[0] && u < m.range[1] ? "selected" : ""}
              >
                ${m.label}
              </li>`
  )}
        </ul>
      </nav>
      <div class="menu-groups" onmousemove=${L}>
        ${a.map((m) => ct`
            <div key=${m.key} class="menu-group">
              <h6>${m.label}</h6>
              <ul>
                ${m.items.map(
    (d) => ct`<li
                      key=${d.key}
                      data-index=${d.index}
                      class=${u === d.index ? "hover" : ""}
                      onmouseenter=${bt(d.index)}
                      onmousedown=${() => {
      var w;
      (w = c.current.querySelector(`[data-index="${d.index}"]`)) == null || w.classList.add("active");
    }}
                      onmouseup=${() => {
      var w;
      (w = c.current.querySelector(`[data-index="${d.index}"]`)) == null || w.classList.remove("active"), N(d.index);
    }}
                    >
                      ${d.icon}
                      <span>${d.label}</span>
                    </li>`
  )}
              </ul>
            </div>
          `)}
      </div>
    </host>
  `;
};
pn.props = {
  ctx: Object,
  config: Object,
  show: Boolean,
  filter: String,
  hide: Function
};
const mn = Ye(pn);
var vn = (n) => {
  throw TypeError(n);
}, fn = (n, t, e) => t.has(n) || vn("Cannot " + e), $ = (n, t, e) => (fn(n, t, "read from private field"), e ? e.call(n) : t.get(n)), Qt = (n, t, e) => t.has(n) ? vn("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(n) : t.set(n, e), rt = (n, t, e, s) => (fn(n, t, "write to private field"), t.set(n, e), e), E, P, tt;
const _n = gs("CREPE_MENU"), le = at(
  {
    show: () => {
    },
    hide: () => {
    }
  },
  "menuAPICtx"
);
Qe("milkdown-slash-menu", mn);
function Ws(n, t) {
  n.set(_n.key, {
    view: (e) => new Es(n, e, t)
  });
}
class Es {
  constructor(t, e, s) {
    Qt(this, E), Qt(this, P), Qt(this, tt, null), this.update = (a) => {
      $(this, P).update(a);
    }, this.show = (a) => {
      rt(this, tt, a), $(this, E).filter = "", $(this, P).show();
    }, this.hide = () => {
      rt(this, tt, null), $(this, P).hide();
    }, this.destroy = () => {
      $(this, P).destroy(), $(this, E).remove();
    }, rt(this, E, new mn()), $(this, E).hide = this.hide, $(this, E).ctx = t, $(this, E).config = s;
    const o = this;
    rt(this, P, new bs({
      content: $(this, E),
      debounce: 20,
      shouldShow(a) {
        if (Ln(a.state.selection) || Wn(a.state.selection))
          return !1;
        const l = this.getContent(
          a,
          (u) => ["paragraph", "heading"].includes(u.type.name)
        );
        if (l == null) return !1;
        const c = $(o, tt);
        return $(o, E).filter = l.startsWith("/") ? l.slice(1) : l, typeof c == "number" ? a.state.doc.resolve(c).node() !== a.state.doc.resolve(a.state.selection.from).node() ? (rt(o, tt, null), !1) : !0 : !!l.startsWith("/");
      },
      offset: 10
    })), $(this, P).onShow = () => {
      $(this, E).show = !0;
    }, $(this, P).onHide = () => {
      $(this, E).show = !1;
    }, this.update(e), t.set(le.key, {
      show: (a) => this.show(a),
      hide: () => this.hide()
    });
  }
}
E = /* @__PURE__ */ new WeakMap();
P = /* @__PURE__ */ new WeakMap();
tt = /* @__PURE__ */ new WeakMap();
const kn = ({
  onAdd: n,
  addIcon: t,
  handleIcon: e
}) => {
  const s = Xe();
  return Zt(() => {
    var l;
    (l = s.current) == null || l.classList.remove("active");
  }), ct`
    <host>
      <div
        ref=${s}
        onmousedown=${(l) => {
    var c;
    l.preventDefault(), l.stopPropagation(), (c = s.current) == null || c.classList.add("active");
  }}
        onmouseup=${(l) => {
    var c;
    l.preventDefault(), l.stopPropagation(), n == null || n(), (c = s.current) == null || c.classList.remove("active");
  }}
        class="operation-item"
      >
        ${(t == null ? void 0 : t()) || En}
      </div>
      <div class="operation-item">${(e == null ? void 0 : e()) || Tn}</div>
    </host>
  `;
};
kn.props = {
  show: Boolean,
  onAdd: Function,
  addIcon: Function,
  handleIcon: Function
};
const wn = Ye(kn);
var yn = (n) => {
  throw TypeError(n);
}, gn = (n, t, e) => t.has(n) || yn("Cannot " + e), A = (n, t, e) => (gn(n, t, "read from private field"), e ? e.call(n) : t.get(n)), Yt = (n, t, e) => t.has(n) ? yn("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(n) : t.set(n, e), Xt = (n, t, e, s) => (gn(n, t, "write to private field"), t.set(n, e), e), q, U, Nt;
class Ts {
  constructor(t, e) {
    Yt(this, q), Yt(this, U), Yt(this, Nt), this.update = () => {
      A(this, U).update();
    }, this.destroy = () => {
      A(this, U).destroy(), A(this, q).remove();
    }, this.onAdd = () => {
      const o = A(this, Nt), a = o.get(C);
      a.hasFocus() || a.focus();
      const { state: l, dispatch: c } = a, u = A(this, U).active;
      if (!u) return;
      const g = u.$pos.pos + u.node.nodeSize;
      let b = l.tr.insert(g, en.type(o).create());
      b = b.setSelection(ee.near(b.doc.resolve(g))), c(b.scrollIntoView()), A(this, U).hide(), o.get(le.key).show(b.selection.from);
    }, Xt(this, Nt, t);
    const s = new wn();
    Xt(this, q, s), A(this, q).onAdd = this.onAdd, A(this, q).addIcon = e == null ? void 0 : e.handleAddIcon, A(this, q).handleIcon = e == null ? void 0 : e.handleDragIcon, Xt(this, U, new ws({
      ctx: t,
      content: s,
      getOffset: () => 16,
      getPlacement: ({ active: o, blockDom: a }) => {
        if (o.node.type.name === "heading") return "left";
        let l = 0;
        o.node.descendants((N) => {
          l += N.childCount;
        });
        const c = o.el, u = c.getBoundingClientRect(), f = a.getBoundingClientRect(), g = window.getComputedStyle(c), b = Number.parseInt(g.paddingTop, 10) || 0, L = Number.parseInt(g.paddingBottom, 10) || 0, _ = u.height - b - L, I = f.height;
        return l > 2 || I < _ ? "left-start" : "left";
      }
    })), this.update();
  }
}
q = /* @__PURE__ */ new WeakMap();
U = /* @__PURE__ */ new WeakMap();
Nt = /* @__PURE__ */ new WeakMap();
Qe("milkdown-block-handle", wn);
function Bs(n, t) {
  n.set(Ft.key, {
    filterNodes: (e) => !ds(
      (o) => ["table", "blockquote", "math_inline"].includes(o.type.name)
    )(e)
  }), n.set(Ut.key, {
    view: () => new Ts(n, t)
  });
}
const Gs = (n, t) => {
  n.config((e) => Bs(e, t)).config((e) => Ws(e, t)).use(le).use(Ut).use(_n);
};
export {
  Gs as defineFeature
};
