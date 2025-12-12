import { t as I } from "./index-BL75rrnU.js";
import { c as x, f as P, o as S, s as q } from "./floating-ui.dom-C8djGRJz.js";
import { $ as B, f as F, P as H, g as R, q as U, T as K } from "./integration-BHt4Ncdo.js";
var W = (e) => {
  throw TypeError(e);
}, M = (e, t, s) => t.has(e) || W("Cannot " + s), i = (e, t, s) => (M(e, t, "read from private field"), t.get(e)), a = (e, t, s) => t.has(e) ? W("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, s), n = (e, t, s, o) => (M(e, t, "write to private field"), t.set(e, s), s), L = (e, t, s) => (M(e, t, "access private method"), s), u, _, w, c, g, p, d, k, $, v;
class G {
  constructor(t) {
    a(this, $), a(this, u), a(this, _), a(this, w), a(this, c), a(this, g), a(this, p), a(this, d), a(this, k), n(this, p, !1), this.onShow = () => {
    }, this.onHide = () => {
    }, n(this, k, (s, o) => {
      const { state: h, composing: r } = s, { selection: m, doc: y } = h, { ranges: f } = m, T = Math.min(...f.map((l) => l.$from.pos)), O = Math.max(...f.map((l) => l.$to.pos)), C = o && o.doc.eq(y) && o.selection.eq(m);
      if (i(this, p) || ((i(this, g) ?? s.dom.parentElement ?? document.body).appendChild(this.element), n(this, p, !0)), r || C) return;
      if (!i(this, _).call(this, s, o)) {
        this.hide();
        return;
      }
      x({
        getBoundingClientRect: () => U(s, T, O)
      }, this.element, {
        placement: i(this, c).placement ?? "top",
        middleware: [P(), S(i(this, d)), q(), ...i(this, w)]
      }).then(({ x: l, y: E }) => {
        Object.assign(this.element.style, {
          left: `${l}px`,
          top: `${E}px`
        });
      }), this.show();
    }), this.update = (s, o) => {
      I(i(this, k), i(this, u))(s, o);
    }, this.destroy = () => {
    }, this.show = (s) => {
      this.element.dataset.show = "true", s && x(s, this.element, {
        placement: "top",
        middleware: [P(), S(i(this, d))],
        ...i(this, c)
      }).then(({ x: o, y: h }) => {
        Object.assign(this.element.style, {
          left: `${o}px`,
          top: `${h}px`
        });
      }), this.onShow();
    }, this.hide = () => {
      this.element.dataset.show !== "false" && (this.element.dataset.show = "false", this.onHide());
    }, this.element = t.content, n(this, u, t.debounce ?? 200), n(this, _, t.shouldShow ?? L(this, $, v)), n(this, d, t.offset), n(this, w, t.middleware ?? []), n(this, c, t.floatingUIOptions ?? {}), n(this, g, t.root), this.element.dataset.show = "false";
  }
}
u = /* @__PURE__ */ new WeakMap();
_ = /* @__PURE__ */ new WeakMap();
w = /* @__PURE__ */ new WeakMap();
c = /* @__PURE__ */ new WeakMap();
g = /* @__PURE__ */ new WeakMap();
p = /* @__PURE__ */ new WeakMap();
d = /* @__PURE__ */ new WeakMap();
k = /* @__PURE__ */ new WeakMap();
$ = /* @__PURE__ */ new WeakSet();
v = function(e) {
  const { doc: t, selection: s } = e.state, { empty: o, from: h, to: r } = s, m = !t.textBetween(h, r).length && e.state.selection instanceof K, y = this.element.contains(document.activeElement), f = !e.hasFocus() && !y, T = !e.editable;
  return !(f || o || m || T);
};
function J(e) {
  const t = B(
    {},
    `${e}_TOOLTIP_SPEC`
  ), s = F((h) => {
    const r = h.get(t.key);
    return new H({
      key: new R(`${e}_TOOLTIP`),
      ...r
    });
  }), o = [t, s];
  return o.key = t.key, o.pluginKey = s.key, t.meta = {
    package: "@milkdown/plugin-tooltip",
    displayName: `Ctx<tooltipSpec>|${e}`
  }, s.meta = {
    package: "@milkdown/plugin-tooltip",
    displayName: `Prose<tooltip>|${e}`
  }, o;
}
export {
  G as T,
  J as t
};
