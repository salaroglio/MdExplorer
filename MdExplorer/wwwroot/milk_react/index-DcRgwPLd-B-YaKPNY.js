import { r as p, t as v, e as m, v as s } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { c as k, l as c, a as B } from "./index.es-BPFC3TNa.js";
const y = (r, l) => {
  r.config(k).config((a) => {
    a.update(c.key, (d) => {
      var n, o, t, e, u, i;
      return {
        ...d,
        linkIcon: (n = l == null ? void 0 : l.linkIcon) != null ? n : () => s,
        editButton: (o = l == null ? void 0 : l.editButton) != null ? o : () => m,
        removeButton: (t = l == null ? void 0 : l.removeButton) != null ? t : () => v,
        confirmButton: (e = l == null ? void 0 : l.confirmButton) != null ? e : () => p,
        inputPlaceholder: (u = l == null ? void 0 : l.inputPlaceholder) != null ? u : "Paste link...",
        onCopyLink: (i = l == null ? void 0 : l.onCopyLink) != null ? i : () => {
        }
      };
    });
  }).use(B);
};
export {
  y as defineFeature
};
