import { i as s, a as u } from "./index-D6fLMv29-BbUkr3pX.js";
import { au as d, aH as f, aI as p, aJ as m, aX as h, aY as P } from "./integration-DEwUyN3-.js";
import { f as C } from "./index-CDW_diKV.js";
function g(n) {
  var e;
  return n.childCount <= 1 && !((e = n.firstChild) != null && e.content.size);
}
function y(n, e) {
  const { selection: r } = n;
  if (!r.empty) return null;
  const o = r.$anchor, t = o.parent;
  if (t.content.size > 0 || C((i) => i.type.name === "table")(o)) return null;
  const c = o.before();
  return P.node(c, c + t.nodeSize, {
    class: "crepe-placeholder",
    "data-placeholder": e
  });
}
const a = d(
  {
    text: "Please enter...",
    mode: "block"
  },
  "placeholderConfigCtx"
), b = f((n) => new p({
  key: new m("CREPE_PLACEHOLDER"),
  props: {
    decorations: (e) => {
      var r;
      const o = n.get(a.key);
      if (o.mode === "doc" && !g(e.doc) || s(e.selection) || u(e.selection))
        return null;
      const t = (r = o.text) != null ? r : "Please enter...", l = y(e, t);
      return l ? h.create(e.doc, [l]) : null;
    }
  }
})), E = (n, e) => {
  n.config((r) => {
    e && r.update(a.key, (o) => ({
      ...o,
      ...e
    }));
  }).use(b).use(a);
};
export {
  E as defineFeature,
  a as placeholderConfig,
  b as placeholderPlugin
};
