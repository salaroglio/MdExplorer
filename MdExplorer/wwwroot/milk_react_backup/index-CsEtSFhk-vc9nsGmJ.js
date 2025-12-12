import { d as st } from "./index-D6fLMv29-BbUkr3pX.js";
import { b as nt, K as at, p as lt, i as y, L as it, M as rt, N as ct, H as dt, O as ut, J as mt, h as D } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { m as R } from "./inline-latex-C9IGAXXQ-nhqqOT-l.js";
import { T as P, s as ht, C as pt, I as vt, J as ft, K as yt, L as bt, M as G, j as A, N as Q, O as H, Q as kt, R as _t, U as wt, V as gt } from "./integration-BHt4Ncdo.js";
import { t as $t, T as Ct } from "./index.es-Cho9Kr7B.js";
import { b as j } from "./index.es-BATVdN5b.js";
const Y = ({
  ctx: t,
  hide: s,
  show: n,
  config: o,
  selection: c
}) => {
  var d, m, h, _, w, g, $, C, I, S, T, E;
  const W = at();
  lt(() => {
    W();
  }, [n]);
  const p = (e) => (l) => {
    l.preventDefault(), t && e(t), W();
  }, v = (e) => {
    if (!t || !c) return !1;
    const l = t.get(A), {
      state: { doc: i }
    } = l;
    return i.rangeHasMark(c.from, c.to, e);
  }, V = (e) => {
    if (!t || !c) return !1;
    const l = t.get(A), {
      state: { doc: i }
    } = l;
    if (c instanceof Q)
      return c.node.type === e;
    const { from: f, to: L } = c;
    let b = !1;
    return i.nodesBetween(f, L, (B) => B.type === e ? (b = !0, !1) : !0), b;
  }, J = t == null ? void 0 : t.get(ht), et = J == null ? void 0 : J.includes(pt.Latex), ot = (e) => {
    const l = V(R.type(e)), i = e.get(A), { selection: f, doc: L, tr: b } = i.state;
    if (!l) {
      const O = L.textBetween(f.from, f.to);
      let M = b.replaceSelectionWith(
        R.type(e).create({
          value: O
        })
      );
      i.dispatch(
        M.setSelection(Q.create(M.doc, f.from))
      );
      return;
    }
    const { from: B, to: K } = f;
    let k = -1, N = null;
    if (L.nodesBetween(B, K, (O, M) => N ? !1 : O.type === R.type(e) ? (k = M, N = O, !1) : !0), !N || k < 0) return;
    let F = b.delete(k, k + 1);
    const U = N.attrs.value;
    F = F.insertText(U, k), i.dispatch(
      F.setSelection(
        P.create(F.doc, B, K + U.length - 1)
      )
    );
  };
  return D`<host>
    <button
      type="button"
      class=${y(
    "toolbar-item",
    t && v(vt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(H).call(kt.key);
  })}
    >
      ${(m = (d = o == null ? void 0 : o.boldIcon) == null ? void 0 : d.call(o)) != null ? m : it}
    </button>
    <button
      type="button"
      class=${y(
    "toolbar-item",
    t && v(ft.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(H).call(_t.key);
  })}
    >
      ${(_ = (h = o == null ? void 0 : o.italicIcon) == null ? void 0 : h.call(o)) != null ? _ : rt}
    </button>
    <button
      type="button"
      class=${y(
    "toolbar-item",
    t && v(yt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(H).call(wt.key);
  })}
    >
      ${(g = (w = o == null ? void 0 : o.strikethroughIcon) == null ? void 0 : w.call(o)) != null ? g : ct}
    </button>
    <div class="divider"></div>
    <button
      type="button"
      class=${y(
    "toolbar-item",
    t && v(bt.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    e.get(H).call(gt.key);
  })}
    >
      ${(C = ($ = o == null ? void 0 : o.codeIcon) == null ? void 0 : $.call(o)) != null ? C : dt}
    </button>
    ${et && D`<button
      type="button"
      class=${y(
    "toolbar-item",
    t && V(R.type(t)) && "active"
  )}
      onmousedown=${p(ot)}
    >
      ${(S = (I = o == null ? void 0 : o.latexIcon) == null ? void 0 : I.call(o)) != null ? S : mt}
    </button>`}
    <button
      type="button"
      class=${y(
    "toolbar-item",
    t && v(G.type(t)) && "active"
  )}
      onmousedown=${p((e) => {
    const l = e.get(A), { selection: i } = l.state;
    if (v(G.type(e))) {
      e.get(j.key).removeLink(i.from, i.to);
      return;
    }
    e.get(j.key).addLink(i.from, i.to), s == null || s();
  })}
    >
      ${(E = (T = o == null ? void 0 : o.linkIcon) == null ? void 0 : T.call(o)) != null ? E : ut}
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
}, tt = (t, s, n) => s.has(t) || x("Cannot " + n), a = (t, s, n) => (tt(t, s, "read from private field"), n ? n.call(t) : s.get(t)), q = (t, s, n) => s.has(t) ? x("Cannot add the same private member more than once") : s instanceof WeakSet ? s.add(t) : s.set(t, n), z = (t, s, n, o) => (tt(t, s, "write to private field"), s.set(t, n), n), u, r;
const X = $t("CREPE_TOOLBAR");
class It {
  constructor(s, n, o) {
    q(this, u), q(this, r), this.update = (d, m) => {
      a(this, u).update(d, m), a(this, r).selection = d.state.selection;
    }, this.destroy = () => {
      a(this, u).destroy(), a(this, r).remove();
    }, this.hide = () => {
      a(this, u).hide();
    };
    const c = new Z();
    z(this, r, c), a(this, r).ctx = s, a(this, r).hide = this.hide, a(this, r).config = o, a(this, r).selection = n.state.selection, z(this, u, new Ct({
      content: a(this, r),
      debounce: 20,
      offset: 10,
      shouldShow(d) {
        const { doc: m, selection: h } = d.state, { empty: _, from: w, to: g } = h, $ = !m.textBetween(w, g).length && h instanceof P, C = !(h instanceof P), I = d.dom.getRootNode().activeElement, S = c.contains(I), T = !d.hasFocus() && !S, E = !d.editable;
        return !(T || C || _ || $ || E);
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
const Ft = (t, s) => {
  t.config((n) => {
    n.set(X.key, {
      view: (o) => new It(n, o, s)
    });
  }).use(X);
};
export {
  Ft as defineFeature
};
