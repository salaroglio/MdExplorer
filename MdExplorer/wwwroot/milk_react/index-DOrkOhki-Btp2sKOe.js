import { i as s, a as u } from "./index-D6fLMv29-BbUkr3pX.js";
import { $ as d, f, P as p, g as m, D as h, b as P } from "./integration-BHt4Ncdo.js";
import { f as g } from "./index-jM38O6Kt.js";
function C(n) {
  var e;
  return n.childCount <= 1 && !((e = n.firstChild) != null && e.content.size);
}
function b(n, e) {
  const { selection: r } = n;
  if (!r.empty) return null;
  const o = r.$anchor, t = o.parent;
  if (t.content.size > 0 || g((a) => a.type.name === "table")(o)) return null;
  const i = o.before();
  return P.node(i, i + t.nodeSize, {
    class: "crepe-placeholder",
    "data-placeholder": e
  });
}
const c = d(
  {
    text: "Please enter...",
    mode: "block"
  },
  "placeholderConfigCtx"
), y = f((n) => new p({
  key: new m("CREPE_PLACEHOLDER"),
  props: {
    decorations: (e) => {
      var r;
      const o = n.get(c.key);
      if (o.mode === "doc" && !C(e.doc) || s(e.selection) || u(e.selection))
        return null;
      const t = (r = o.text) != null ? r : "Please enter...", l = b(e, t);
      return l ? h.create(e.doc, [l]) : null;
    }
  }
})), E = (n, e) => {
  n.config((r) => {
    e && r.update(c.key, (o) => ({
      ...o,
      ...e
    }));
  }).use(y).use(c);
};
export {
  E as defineFeature,
  c as placeholderConfig,
  y as placeholderPlugin
};
