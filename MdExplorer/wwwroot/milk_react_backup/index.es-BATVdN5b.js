import { $ as V, o as ft, q as ot, T as q, j as B, M as N } from "./integration-BHt4Ncdo.js";
import { b as it, h as R, d as kt, p as _t, i as mt } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { u as wt } from "./hooks-CCspbR-r.js";
import { t as nt, T as rt } from "./index.es-Cho9Kr7B.js";
var yt = Object.defineProperty, G = Object.getOwnPropertySymbols, Pt = Object.prototype.hasOwnProperty, gt = Object.prototype.propertyIsEnumerable, H = (e, t, o) => t in e ? yt(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, $t = (e, t) => {
  for (var o in t || (t = {}))
    Pt.call(t, o) && H(e, o, t[o]);
  if (G)
    for (var o of G(t))
      gt.call(t, o) && H(e, o, t[o]);
  return e;
};
function P(e, t) {
  return Object.assign(e, {
    meta: $t({
      package: "@milkdown/components"
    }, t)
  }), e;
}
var Et = Object.defineProperty, X = Object.getOwnPropertySymbols, Ot = Object.prototype.hasOwnProperty, Lt = Object.prototype.propertyIsEnumerable, z = (e, t, o) => t in e ? Et(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, j = (e, t) => {
  for (var o in t || (t = {}))
    Ot.call(t, o) && z(e, o, t[o]);
  if (X)
    for (var o of X(t))
      Lt.call(t, o) && z(e, o, t[o]);
  return e;
};
const Tt = {
  mode: "preview"
}, v = V(j({}, Tt), "linkTooltipStateCtx");
P(v, {
  displayName: "State<link-tooltip>",
  group: "LinkTooltip"
});
const St = {
  addLink: () => {
  },
  editLink: () => {
  },
  removeLink: () => {
  }
}, b = V(j({}, St), "linkTooltipAPICtx");
P(v, {
  displayName: "API<link-tooltip>",
  group: "LinkTooltip"
});
const Ct = {
  linkIcon: () => "🔗",
  editButton: () => "✎",
  removeButton: () => "⌫",
  confirmButton: () => R`Confirm ⏎`,
  onCopyLink: () => {
  },
  inputPlaceholder: "Paste link..."
}, K = V(
  j({}, Ct),
  "linkTooltipConfigCtx"
);
P(v, {
  displayName: "Config<link-tooltip>",
  group: "LinkTooltip"
});
const T = nt("LINK_PREVIEW");
P(T[0], {
  displayName: "PreviewTooltipSpec<link-tooltip>",
  group: "LinkTooltip"
});
P(T[1], {
  displayName: "PreviewTooltipPlugin<link-tooltip>",
  group: "LinkTooltip"
});
const A = nt("LINK_EDIT");
P(A[0], {
  displayName: "EditTooltipSpec<link-tooltip>",
  group: "LinkTooltip"
});
P(A[1], {
  displayName: "EditTooltipPlugin<link-tooltip>",
  group: "LinkTooltip"
});
function Mt(e, t, o, i, r) {
  let n = { start: -1, end: -1 };
  return o.nodesBetween(i, r, (l, s) => {
    if (n.start > -1) return !1;
    n.start === -1 && e.isInSet(l.marks) && t === l && (n = {
      start: s,
      end: s + Math.max(l.textContent.length, 1)
    });
  }), n;
}
function It(e, t, o) {
  const i = t.posAtCoords({ left: o.clientX, top: o.clientY });
  if (!i) return;
  const { pos: r } = i, n = t.state.doc.nodeAt(r);
  if (!n) return;
  const l = n.marks.find(
    (c) => c.type === N.mark.type(e)
  );
  if (!(!l || !T.pluginKey()))
    return { show: !0, pos: r, node: n, mark: l };
}
function st(e, t) {
  const o = customElements.get(e);
  if (o == null) {
    customElements.define(e, t);
    return;
  }
  o !== t && console.warn(`Custom element ${e} has been defined before.`);
}
const at = ({
  config: e,
  src: t,
  onEdit: o,
  onRemove: i
}) => {
  const r = (s) => {
    s.stopPropagation(), s.preventDefault(), o == null || o();
  }, n = (s) => {
    s.stopPropagation(), s.preventDefault(), i == null || i();
  };
  return R`
    <host>
      <div class="link-preview" onmousedown=${(s) => {
    s.preventDefault(), navigator.clipboard && t && navigator.clipboard.writeText(t).then(() => {
      e == null || e.onCopyLink(t);
    }).catch((c) => {
      throw c;
    });
  }}>
        <span class="link-icon"> ${e == null ? void 0 : e.linkIcon()} </span>
        <a href=${t} target="_blank" class="link-display">${t}</a>
        <span class="button link-edit-button" onmousedown=${r}>
          ${e == null ? void 0 : e.editButton()}
        </span>
        <span
          class="button link-remove-button"
          onmousedown=${n}
        >
          ${e == null ? void 0 : e.removeButton()}
        </span>
      </div>
    </host>
  `;
};
at.props = {
  config: Object,
  src: String,
  onEdit: Function,
  onRemove: Function
};
const lt = it(at);
var pt = (e) => {
  throw TypeError(e);
}, ct = (e, t, o) => t.has(e) || pt("Cannot " + o), a = (e, t, o) => (ct(e, t, "read from private field"), o ? o.call(e) : t.get(e)), k = (e, t, o) => t.has(e) ? pt("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, o), S = (e, t, o, i) => (ct(e, t, "write to private field"), t.set(e, o), o), _, u, E, O, I, x, W, L;
class xt {
  constructor(t, o) {
    this.ctx = t, k(this, _, new lt()), k(this, u), k(this, E, this.ctx.use(v.key)), k(this, O, !1), k(this, I, ({ mode: i }) => {
      i === "edit" && a(this, L).call(this);
    }), k(this, x, () => {
      S(this, O, !0);
    }), k(this, W, () => {
      S(this, O, !1);
    }), k(this, L, () => {
      a(this, u).hide(), a(this, u).element.removeEventListener("mouseenter", a(this, x)), a(this, u).element.removeEventListener("mouseleave", a(this, W));
    }), this.show = (i, r, n, l) => {
      a(this, _).config = this.ctx.get(K.key), a(this, _).src = i.attrs.href, a(this, _).onEdit = () => {
        this.ctx.get(b.key).editLink(i, r, n);
      }, a(this, _).onRemove = () => {
        this.ctx.get(b.key).removeLink(r, n), a(this, L).call(this);
      }, a(this, u).show({
        getBoundingClientRect: () => l
      }), a(this, u).element.addEventListener("mouseenter", a(this, x)), a(this, u).element.addEventListener("mouseleave", a(this, W));
    }, this.hide = () => {
      a(this, O) || a(this, L).call(this);
    }, this.update = () => {
    }, this.destroy = () => {
      a(this, E).off(a(this, I)), a(this, u).destroy(), a(this, _).remove();
    }, S(this, u, new rt({
      debounce: 0,
      content: a(this, _),
      shouldShow: () => !1
    })), a(this, u).update(o), S(this, E, t.use(v.key)), a(this, E).on(a(this, I));
  }
}
_ = /* @__PURE__ */ new WeakMap();
u = /* @__PURE__ */ new WeakMap();
E = /* @__PURE__ */ new WeakMap();
O = /* @__PURE__ */ new WeakMap();
I = /* @__PURE__ */ new WeakMap();
x = /* @__PURE__ */ new WeakMap();
W = /* @__PURE__ */ new WeakMap();
L = /* @__PURE__ */ new WeakMap();
st("milkdown-link-preview", lt);
function Wt(e) {
  let t;
  const i = ft((n, l) => {
    if (!t || !n.hasFocus() || e.get(v.key).mode === "edit") return;
    const c = It(e, n, l);
    if (c) {
      const d = n.state.doc.resolve(c.pos), f = Mt(
        c.mark,
        c.node,
        n.state.doc,
        d.before(),
        d.after()
      ), $ = f.start, Y = f.end;
      t.show(
        c.mark,
        $,
        Y,
        ot(n, $, Y)
      );
      return;
    }
    t.hide();
  }, 200), r = () => {
    setTimeout(() => {
      t == null || t.hide();
    }, 200);
  };
  e.set(T.key, {
    props: {
      handleDOMEvents: {
        mousemove: i,
        mouseleave: r
      }
    },
    view: (n) => (t = new xt(e, n), t)
  });
}
const dt = ({
  src: e,
  onConfirm: t,
  onCancel: o,
  config: i
}) => {
  const r = kt(), [n, l] = wt(e);
  _t(() => {
    l(e ?? "");
  }, [e]);
  const s = () => {
    var d, f;
    t == null || t((f = (d = r.current) == null ? void 0 : d.value) != null ? f : "");
  }, c = (d) => {
    var f, $;
    d.stopPropagation(), d.key === "Enter" && (t == null || t(($ = (f = r.current) == null ? void 0 : f.value) != null ? $ : ""), d.preventDefault()), d.key === "Escape" && (o == null || o(), d.preventDefault());
  };
  return R`
    <host>
      <div class="link-edit">
        <input
          class="input-area"
          placeholder=${i == null ? void 0 : i.inputPlaceholder}
          ref=${r}
          onkeydown=${c}
          oninput=${(d) => l(d.target.value)}
          value=${n}
        />
        <span
          class=${mt("button confirm", !n && "hidden")}
          onclick=${s}
        >
          ${i == null ? void 0 : i.confirmButton()}
        </span>
      </div>
    </host>
  `;
};
dt.props = {
  config: Object,
  src: String,
  onConfirm: Function,
  onCancel: Function
};
const ut = it(dt);
var Dt = Object.defineProperty, bt = Object.defineProperties, At = Object.getOwnPropertyDescriptors, J = Object.getOwnPropertySymbols, Bt = Object.prototype.hasOwnProperty, Nt = Object.prototype.propertyIsEnumerable, ht = (e) => {
  throw TypeError(e);
}, Q = (e, t, o) => t in e ? Dt(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, C = (e, t) => {
  for (var o in t || (t = {}))
    Bt.call(t, o) && Q(e, o, t[o]);
  if (J)
    for (var o of J(t))
      Nt.call(t, o) && Q(e, o, t[o]);
  return e;
}, U = (e, t) => bt(e, At(t)), vt = (e, t, o) => t.has(e) || ht("Cannot " + o), p = (e, t, o) => (vt(e, t, "read from private field"), o ? o.call(e) : t.get(e)), g = (e, t, o) => t.has(e) ? ht("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, o), M = (e, t, o, i) => (vt(e, t, "write to private field"), t.set(e, o), o), h, m, w, y, F, D;
const Z = {
  from: -1,
  to: -1,
  mark: null
};
class Ft {
  constructor(t, o) {
    this.ctx = t, g(this, h, new ut()), g(this, m), g(this, w, C({}, Z)), g(this, y, () => {
      p(this, m).hide(), this.ctx.update(v.key, (i) => U(C({}, i), {
        mode: "preview"
      })), M(this, w, C({}, Z));
    }), g(this, F, (i) => {
      const r = this.ctx.get(B), { from: n, to: l, mark: s } = p(this, w), c = N.type(this.ctx);
      if (s && s.attrs.href === i) {
        p(this, y).call(this);
        return;
      }
      const d = r.state.tr;
      s && d.removeMark(n, l, s), d.addMark(n, l, c.create({ href: i })), r.dispatch(d), p(this, y).call(this);
    }), g(this, D, (i, r, n) => {
      const l = this.ctx.get(K.key);
      p(this, h).config = l, p(this, h).src = i, this.ctx.update(v.key, (c) => U(C({}, c), {
        mode: "edit"
      }));
      const s = this.ctx.get(B);
      s.dispatch(
        s.state.tr.setSelection(q.create(s.state.doc, r, n))
      ), p(this, m).show({
        getBoundingClientRect: () => ot(s, r, n)
      }), requestAnimationFrame(() => {
        var c;
        (c = p(this, h).querySelector("input")) == null || c.focus();
      });
    }), this.update = (i) => {
      const { state: r } = i, { selection: n } = r;
      if (!(n instanceof q)) return;
      const { from: l, to: s } = n;
      l === p(this, w).from && s === p(this, w).to || p(this, y).call(this);
    }, this.destroy = () => {
      p(this, m).destroy(), p(this, h).remove();
    }, this.addLink = (i, r) => {
      M(this, w, {
        from: i,
        to: r,
        mark: null
      }), p(this, D).call(this, "", i, r);
    }, this.editLink = (i, r, n) => {
      M(this, w, {
        from: r,
        to: n,
        mark: i
      }), p(this, D).call(this, i.attrs.href, r, n);
    }, this.removeLink = (i, r) => {
      const n = this.ctx.get(B), l = n.state.tr;
      l.removeMark(i, r, N.type(this.ctx)), n.dispatch(l), p(this, y).call(this);
    }, M(this, m, new rt({
      content: p(this, h),
      debounce: 0,
      shouldShow: () => !1
    })), p(this, m).onHide = () => {
      p(this, h).update().catch((i) => {
        throw i;
      }), o.dom.focus({ preventScroll: !0 });
    }, p(this, m).update(o), p(this, h).onConfirm = p(this, F), p(this, h).onCancel = p(this, y);
  }
}
h = /* @__PURE__ */ new WeakMap();
m = /* @__PURE__ */ new WeakMap();
w = /* @__PURE__ */ new WeakMap();
y = /* @__PURE__ */ new WeakMap();
F = /* @__PURE__ */ new WeakMap();
D = /* @__PURE__ */ new WeakMap();
var Vt = Object.defineProperty, Rt = Object.defineProperties, jt = Object.getOwnPropertyDescriptors, tt = Object.getOwnPropertySymbols, Kt = Object.prototype.hasOwnProperty, Yt = Object.prototype.propertyIsEnumerable, et = (e, t, o) => t in e ? Vt(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, qt = (e, t) => {
  for (var o in t || (t = {}))
    Kt.call(t, o) && et(e, o, t[o]);
  if (tt)
    for (var o of tt(t))
      Yt.call(t, o) && et(e, o, t[o]);
  return e;
}, Gt = (e, t) => Rt(e, jt(t));
st("milkdown-link-edit", ut);
function Ht(e) {
  let t;
  e.update(b.key, (o) => Gt(qt({}, o), {
    addLink: (i, r) => {
      t == null || t.addLink(i, r);
    },
    editLink: (i, r, n) => {
      t == null || t.editLink(i, r, n);
    },
    removeLink: (i, r) => {
      t == null || t.removeLink(i, r);
    }
  })), e.set(A.key, {
    view: (o) => (t = new Ft(e, o), t)
  });
}
function Ut(e) {
  Wt(e), Ht(e);
}
const Zt = [
  v,
  b,
  K,
  T,
  A
].flat();
export {
  Zt as a,
  b,
  Ut as c,
  K as l
};
