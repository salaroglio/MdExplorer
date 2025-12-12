import { h as p, b as $, u as B, d as x, g as C, o as g, i as h, p as O, q as L } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { au as P, av as E, aC as S, ax as w } from "./integration-Bxed0ynC.js";
var M = Object.defineProperty, k = Object.getOwnPropertySymbols, F = Object.prototype.hasOwnProperty, T = Object.prototype.propertyIsEnumerable, y = (n, e, t) => e in n ? M(n, e, { enumerable: !0, configurable: !0, writable: !0, value: t }) : n[e] = t, j = (n, e) => {
  for (var t in e || (e = {}))
    F.call(e, t) && y(n, t, e[t]);
  if (k)
    for (var t of k(e))
      T.call(e, t) && y(n, t, e[t]);
  return n;
};
function v(n, e) {
  return Object.assign(n, {
    meta: j({
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
const I = ({
  selected: n,
  label: e = "",
  listType: t = "",
  checked: r,
  onMount: s,
  setAttr: o,
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
    r != null && (o == null || o("checked", !r));
  }, c = {
    label: e,
    listType: t,
    checked: r,
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
I.props = {
  label: String,
  checked: Boolean,
  readonly: Boolean,
  listType: String,
  config: Object,
  selected: Boolean,
  setAttr: Function,
  onMount: Function
};
const q = $(I), V = {
  renderLabel: ({ label: n, listType: e, checked: t, readonly: r }) => t == null ? p`<span class="label"
        >${e === "bullet" ? "⦿" : n}</span
      >` : p`<input
      disabled=${r}
      class="label"
      type="checkbox"
      checked=${t}
    />`
}, m = P(
  V,
  "listItemBlockConfigCtx"
);
v(m, {
  displayName: "Config<list-item-block>",
  group: "ListItemBlock"
});
A("milkdown-list-item-block", q);
const _ = E(
  S.node,
  (n) => (e, t, r) => {
    const s = document.createElement(
      "milkdown-list-item-block"
    ), o = document.createElement("div");
    o.setAttribute("data-content-dom", "true"), o.classList.add("content-dom");
    const u = n.get(m.key), d = (l) => {
      s.listType = l.attrs.listType, s.label = l.attrs.label, s.checked = l.attrs.checked, s.readonly = !t.editable;
    };
    d(e), s.appendChild(o), s.selected = !1, s.setAttr = (l, i) => {
      const c = r();
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
      contentDOM: o,
      update: (l) => l.type !== e.type ? !1 : (l.sameMarkup(a) && l.content.eq(a.content) || (a = l, d(l)), !0),
      ignoreMutation: (l) => !s || !o ? !0 : l.type === "selection" ? !1 : o === l.target && l.type === "attributes" ? !0 : !o.contains(l.target),
      selectNode: () => {
        s.selected = !0;
      },
      deselectNode: () => {
        s.selected = !1;
      },
      destroy: () => {
        s.remove(), o.remove();
      }
    };
  }
);
v(_, {
  displayName: "NodeView<list-item-block>",
  group: "ListItemBlock"
});
const D = [
  m,
  _
];
function R(n, e) {
  n.set(m.key, {
    renderLabel: ({ label: t, listType: r, checked: s, readonly: o }) => {
      var u, d, a, l, i, c;
      return s == null ? r === "bullet" ? p`<span class="label"
            >${(d = (u = e == null ? void 0 : e.bulletIcon) == null ? void 0 : u.call(e)) != null ? d : g}</span
          >` : p`<span class="label">${t}</span>` : s ? p`<span
          class=${h("label checkbox", o && "readonly")}
          >${(l = (a = e == null ? void 0 : e.checkBoxCheckedIcon) == null ? void 0 : a.call(e)) != null ? l : O}</span
        >` : p`<span class=${h("label checkbox", o && "readonly")}
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
