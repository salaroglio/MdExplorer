import { d as st } from "./index-D6fLMv29-BbUkr3pX.js";
import { b as nt, m as at, f as lt, i as f, P as it, Q as rt, R as ct, M as dt, S as ut, O as mt, h as U } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { m as R } from "./inline-latex-C9IGAXXQ-BriK6rst.js";
import { ax as M, b3 as ht, b4 as pt, bd as vt, be as bt, bf as ft, bg as yt, aG as j, aF as W, aW as q, bh as A, bi as kt, bj as _t, bk as wt, bl as gt } from "./integration-x1Kt76xC.js";
import { t as $t, T as St } from "./index.es-fbu30WqE.js";
import { b as z } from "./index.es-uKEMwpp1.js";
const Y = ({
  ctx: t,
  hide: s,
  show: n,
  config: o,
  selection: c
}) => {
  var d, m, h, _, w, g, $, S, C, I, E, T;
  const H = at();
  lt(() => {
    H();
  }, [n]);
  const p = (e) => (l) => {
    l.preventDefault(), t && e(t), H();
  }, v = (e) => {
    if (!t || !c) return !1;
    const l = t.get(W), {
      state: { doc: i }
    } = l;
    return i.rangeHasMark(c.from, c.to, e);
  }, G = (e) => {
    if (!t || !c) return !1;
    const l = t.get(W), {
      state: { doc: i }
    } = l;
    if (c instanceof q)
      return c.node.type === e;
    const { from: b, to: B } = c;
    let y = !1;
    return i.nodesBetween(b, B, (F) => F.type === e ? (y = !0, !1) : !0), y;
  }, V = t == null ? void 0 : t.get(ht), et = V == null ? void 0 : V.includes(pt.Latex), ot = (e) => {
    const l = G(R.type(e)), i = e.get(W), { selection: b, doc: B, tr: y } = i.state;
    if (!l) {
      const O = B.textBetween(b.from, b.to);
      let P = y.replaceSelectionWith(
        R.type(e).create({
          value: O
        })
      );
      i.dispatch(
        P.setSelection(q.create(P.doc, b.from))
      );
      return;
    }
    const { from: F, to: D } = b;
    let k = -1, L = null;
    if (B.nodesBetween(F, D, (O, P) => L ? !1 : O.type === R.type(e) ? (k = P, L = O, !1) : !0), !L || k < 0) return;
    let N = y.delete(k, k + 1);
    const Q = L.attrs.value;
    N = N.insertText(Q, k), i.dispatch(
      N.setSelection(
        M.create(N.doc, F, D + Q.length - 1)
      )
    );
  };
  return U`<host>
    <button
      type="button"
      class=${f(
    "toolbar-item",
    t && v(vt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(A).call(kt.key);
  })}
    >
      ${(m = (d = o == null ? void 0 : o.boldIcon) == null ? void 0 : d.call(o)) != null ? m : it}
    </button>
    <button
      type="button"
      class=${f(
    "toolbar-item",
    t && v(bt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(A).call(_t.key);
  })}
    >
      ${(_ = (h = o == null ? void 0 : o.italicIcon) == null ? void 0 : h.call(o)) != null ? _ : rt}
    </button>
    <button
      type="button"
      class=${f(
    "toolbar-item",
    t && v(ft.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(A).call(wt.key);
  })}
    >
      ${(g = (w = o == null ? void 0 : o.strikethroughIcon) == null ? void 0 : w.call(o)) != null ? g : ct}
    </button>
    <div class="divider"></div>
    <button
      type="button"
      class=${f(
    "toolbar-item",
    t && v(yt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(A).call(gt.key);
  })}
    >
      ${(S = ($ = o == null ? void 0 : o.codeIcon) == null ? void 0 : $.call(o)) != null ? S : dt}
    </button>
    ${et && U`<button
      type="button"
      class=${f(
    "toolbar-item",
    t && G(R.type(t)) && "active"
  )}
      onmousedown=${p(ot)}
    >
      ${(I = (C = o == null ? void 0 : o.latexIcon) == null ? void 0 : C.call(o)) != null ? I : mt}
    </button>`}
    <button
      type="button"
      class=${f(
    "toolbar-item",
    t && v(j.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    const l = e.get(W), { selection: i } = l.state;
    if (v(j.type(e))) {
      e.get(z.key).removeLink(i.from, i.to);
      return;
    }
    e.get(z.key).addLink(i.from, i.to), s == null || s();
  })}
    >
      ${(T = (E = o == null ? void 0 : o.linkIcon) == null ? void 0 : E.call(o)) != null ? T : ut}
    </button>
  </host>`;
};
Y.props = {
  ctx: Object,
  hide: Function,
  show: Boolean,
  config: Object,
  selection: Object
};
const Z = nt(Y);
var x = (t) => {
  throw TypeError(t);
}, tt = (t, s, n) => s.has(t) || x("Cannot " + n), a = (t, s, n) => (tt(t, s, "read from private field"), n ? n.call(t) : s.get(t)), J = (t, s, n) => s.has(t) ? x("Cannot add the same private member more than once") : s instanceof WeakSet ? s.add(t) : s.set(t, n), K = (t, s, n, o) => (tt(t, s, "write to private field"), s.set(t, n), n), u, r;
const X = $t("CREPE_TOOLBAR");
class Ct {
  constructor(s, n, o) {
    J(this, u), J(this, r), this.update = (d, m) => {
      a(this, u).update(d, m), a(this, r).selection = d.state.selection;
    }, this.destroy = () => {
      a(this, u).destroy(), a(this, r).remove();
    }, this.hide = () => {
      a(this, u).hide();
    };
    const c = new Z();
    K(this, r, c), a(this, r).ctx = s, a(this, r).hide = this.hide, a(this, r).config = o, a(this, r).selection = n.state.selection, K(this, u, new St({
      content: a(this, r),
      debounce: 20,
      offset: 10,
      shouldShow(d) {
        const { doc: m, selection: h } = d.state, { empty: _, from: w, to: g } = h, $ = !m.textBetween(w, g).length && h instanceof M, S = !(h instanceof M), C = d.dom.getRootNode().activeElement, I = c.contains(C), E = !d.hasFocus() && !I, T = !d.editable;
        return !(E || S || _ || $ || T);
      }
    })), a(this, u).onShow = () => {
      a(this, r).show = !0;
    }, a(this, u).onHide = () => {
      a(this, r).show = !1;
    }, this.update(n);
  }
}
u = /* @__PURE__ */ new WeakMap();
r = /* @__PURE__ */ new WeakMap();
st("milkdown-toolbar", Z);
const Nt = (t, s) => {
  t.config((n) => {
    n.set(X.key, {
      view: (o) => new Ct(n, o, s)
    });
  }).use(X);
};
export {
  Nt as defineFeature
};
