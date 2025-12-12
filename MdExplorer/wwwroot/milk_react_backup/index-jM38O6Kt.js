import { am as C, g as E } from "./integration-BHt4Ncdo.js";
const t = typeof navigator < "u" ? navigator : null, h = typeof document < "u" ? document : null, c = t && t.userAgent || "", g = /Edge\/(\d+)/.exec(c), A = /MSIE \d/.exec(c), m = /Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(c), p = !!(A || m || g);
A ? document.documentMode : m ? +m[1] : g && +g[1];
const I = !p && /gecko\/(\d+)/i.test(c);
I && +(/Firefox\/(\d+)/.exec(c) || [0, 0])[1];
const M = !p && /Chrome\/(\d+)/.exec(c);
M && +M[1];
const P = !p && !!t && /Apple Computer/.test(t.vendor), R = P && (/Mobile\/\w+/.test(c) || !!t && t.maxTouchPoints > 2);
R || t && /Mac/.test(t.platform);
const S = !!h && "webkitFontSmoothing" in h.documentElement.style;
S && +(/\bAppleWebKit\/(\d+)/.exec(navigator.userAgent) || [0, 0])[1];
new E("MILKDOWN_CUSTOM_INPUTRULES");
function F(s, n, e = {}) {
  return new C(s, (u, l, o, d) => {
    var v, b, x;
    const { tr: i } = u;
    let r = l[1], a = l[0];
    const f = {
      group: r,
      fullMatch: a,
      start: o,
      end: d
    }, k = (v = e.updateCaptured) == null ? void 0 : v.call(e, f);
    if (Object.assign(f, k), { group: r, fullMatch: a, start: o, end: d } = f, a === null || !r || r.trim() === "") return null;
    const w = (b = e.getAttr) == null ? void 0 : b.call(e, l), _ = n.createAndFill(w);
    return _ && (i.replaceRangeWith(
      n.isBlock ? i.doc.resolve(o).before() : o,
      d,
      _
    ), (x = e.beforeDispatch) == null || x.call(e, {
      match: [a, r ?? ""],
      start: o,
      end: d,
      tr: i
    })), i;
  });
}
function K(s) {
  return (n) => {
    for (let e = n.depth; e > 0; e -= 1) {
      const u = n.node(e);
      if (s(u)) {
        const l = n.before(e), o = n.after(e);
        return {
          from: l,
          to: o,
          node: u
        };
      }
    }
  };
}
export {
  K as f,
  F as n
};
