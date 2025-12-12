import { h as p, b as $, u as B, d as x, f as C, g, i as h, j as O, k as L } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { $ as P, a as E, l as S, T as w } from "./integration-BHt4Ncdo.js";
var M = Object.defineProperty, k = Object.getOwnPropertySymbols, T = Object.prototype.hasOwnProperty, j = Object.prototype.propertyIsEnumerable, y = (n, e, t) => e in n ? M(n, e, { enumerable: !0, configurable: !0, writable: !0, value: t }) : n[e] = t, F = (n, e) => {
  for (var t in e || (e = {}))
    T.call(e, t) && y(n, t, e[t]);
  if (k)
    for (var t of k(e))
      j.call(e, t) && y(n, t, e[t]);
  return n;
};
function I(n, e) {
  return Object.assign(n, {
    meta: F({
      package: "@milkdown/components"
    }, e)
  }), n;
}
function A(n, e) {
  const t = customElements.get(n);
  if (t == null) {
    customElements.define(n, e);
    return;
  }
  t !== e && console.warn(`Custom element ${n} has been defined before.`);
}
const v = ({
  selected: n,
  label: e = "",
  listType: t = "",
  checked: o,
  onMount: s,
  setAttr: r,
  config: u,
  readonly: d
}) => {
  const a = B(), l = x();
  C(() => {
    const f = l.current;
    if (!f) return;
    const b = a.current.querySelector("[data-content-dom]");
    b && (f.appendChild(b), s == null || s());
  }, []);
  const i = () => {
    o != null && (r == null || r("checked", !o));
  }, c = {
    label: e,
    listType: t,
    checked: o,
    readonly: d
  };
  return p`<host class=${n && "ProseMirror-selectednode"}>
    <li class="list-item">
      <div
        class="label-wrapper"
        onclick=${i}
        contenteditable="false"
      >
        ${u == null ? void 0 : u.renderLabel(c)}
      </div>
      <div class="children" ref=${l}></div>
    </li>
  </host>`;
};
v.props = {
  label: String,
  checked: Boolean,
  readonly: Boolean,
  listType: String,
  config: Object,
  selected: Boolean,
  setAttr: Function,
  onMount: Function
};
const V = $(v), q = {
  renderLabel: ({ label: n, listType: e, checked: t, readonly: o }) => t == null ? p`<span class="label"
        >${e === "bullet" ? "⦿" : n}</span
      >` : p`<input
      disabled=${o}
      class="label"
      type="checkbox"
      checked=${t}
    />`
}, m = P(
  q,
  "listItemBlockConfigCtx"
);
I(m, {
  displayName: "Config<list-item-block>",
  group: "ListItemBlock"
});
A("milkdown-list-item-block", V);
const _ = E(
  S.node,
  (n) => (e, t, o) => {
    const s = document.createElement(
      "milkdown-list-item-block"
    ), r = document.createElement("div");
    r.setAttribute("data-content-dom", "true"), r.classList.add("content-dom");
    const u = n.get(m.key), d = (l) => {
      s.listType = l.attrs.listType, s.label = l.attrs.label, s.checked = l.attrs.checked, s.readonly = !t.editable;
    };
    d(e), s.appendChild(r), s.selected = !1, s.setAttr = (l, i) => {
      const c = o();
      c != null && t.dispatch(t.state.tr.setNodeAttribute(c, l, i));
    }, s.onMount = () => {
      const { anchor: l, head: i } = t.state.selection;
      t.hasFocus() && setTimeout(() => {
        const c = t.state.doc.resolve(l), f = t.state.doc.resolve(i);
        t.dispatch(
          t.state.tr.setSelection(new w(c, f))
        );
      });
    };
    let a = e;
    return s.config = u, {
      dom: s,
      contentDOM: r,
      update: (l) => l.type !== e.type ? !1 : (l.sameMarkup(a) && l.content.eq(a.content) || (a = l, d(l)), !0),
      ignoreMutation: (l) => !s || !r ? !0 : l.type === "selection" ? !1 : r === l.target && l.type === "attributes" ? !0 : !r.contains(l.target),
      selectNode: () => {
        s.selected = !0;
      },
      deselectNode: () => {
        s.selected = !1;
      },
      destroy: () => {
        s.remove(), r.remove();
      }
    };
  }
);
I(_, {
  displayName: "NodeView<list-item-block>",
  group: "ListItemBlock"
});
const D = [
  m,
  _
];
function R(n, e) {
  n.set(m.key, {
    renderLabel: ({ label: t, listType: o, checked: s, readonly: r }) => {
      var u, d, a, l, i, c;
      return s == null ? o === "bullet" ? p`<span class="label"
            >${(d = (u = e == null ? void 0 : e.bulletIcon) == null ? void 0 : u.call(e)) != null ? d : g}</span
          >` : p`<span class="label">${t}</span>` : s ? p`<span
          class=${h("label checkbox", r && "readonly")}
          >${(l = (a = e == null ? void 0 : e.checkBoxCheckedIcon) == null ? void 0 : a.call(e)) != null ? l : O}</span
        >` : p`<span class=${h("label checkbox", r && "readonly")}
        >${(c = (i = e == null ? void 0 : e.checkBoxUncheckedIcon) == null ? void 0 : i.call(e)) != null ? c : L}</span
      >`;
    }
  });
}
const W = (n, e) => {
  n.config((t) => R(t, e)).use(D);
};
export {
  W as defineFeature
};
