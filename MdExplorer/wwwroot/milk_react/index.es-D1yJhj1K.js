import { au as ne, av as oe, aw as re, ax as j, ay as ie, az as D, aA as ae, aB as H, E as I, v as le, y as z } from "./integration-UpWIHG9l.js";
import { h as N, b as ce, u as de, d as S, f as M, g as ue, i as $, j as he } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { u as R, a as B } from "./hooks-CsyH2QfV.js";
import { c as pe } from "./floating-ui.dom-C8djGRJz.js";
var ve = Object.defineProperty, K = Object.getOwnPropertySymbols, fe = Object.prototype.hasOwnProperty, me = Object.prototype.propertyIsEnumerable, U = (s, e, t) => e in s ? ve(s, e, { enumerable: !0, configurable: !0, writable: !0, value: t }) : s[e] = t, we = (s, e) => {
  for (var t in e || (e = {}))
    fe.call(e, t) && U(s, t, e[t]);
  if (K)
    for (var t of K(e))
      me.call(e, t) && U(s, t, e[t]);
  return s;
};
function q(s, e) {
  return Object.assign(s, {
    meta: we({
      package: "@milkdown/components"
    }, e)
  }), s;
}
const ge = {
  extensions: [],
  languages: [],
  expandIcon: () => "⬇",
  searchIcon: () => "🔍",
  clearSearchIcon: () => "⌫",
  searchPlaceholder: "Search language",
  noResultText: "No result",
  renderLanguage: (s) => N`${s}`,
  renderPreview: () => null,
  previewToggleButton: (s) => s ? "Edit" : "Hide",
  previewLabel: () => "Preview"
}, A = ne(ge, "codeBlockConfigCtx");
q(A, {
  displayName: "Config<code-block>",
  group: "CodeBlock"
});
function ye(s, e) {
  const t = customElements.get(s);
  if (t == null) {
    customElements.define(s, e);
    return;
  }
  t !== e && console.warn(`Custom element ${s} has been defined before.`);
}
var W = Object.getOwnPropertySymbols, Ce = Object.prototype.hasOwnProperty, be = Object.prototype.propertyIsEnumerable, $e = (s, e) => {
  var t = {};
  for (var n in s)
    Ce.call(s, n) && e.indexOf(n) < 0 && (t[n] = s[n]);
  if (s != null && W)
    for (var n of W(s))
      e.indexOf(n) < 0 && be.call(s, n) && (t[n] = s[n]);
  return t;
};
class ke {
  constructor(e, t, n, i, o) {
    this.node = e, this.view = t, this.getPos = n, this.loader = i, this.config = o, this.updating = !1, this.languageName = "", this.forwardUpdate = (l) => {
      var c;
      if (this.updating || !this.cm.hasFocus) return;
      let d = ((c = this.getPos()) != null ? c : 0) + 1;
      const { main: h } = l.state.selection, u = d + h.from, v = d + h.to, f = this.view.state.selection;
      if (l.docChanged || f.from !== u || f.to !== v) {
        const w = this.view.state.tr;
        l.changes.iterChanges((y, k, E, g, C) => {
          C.length ? w.replaceWith(
            d + y,
            d + k,
            this.view.state.schema.text(C.toString())
          ) : w.delete(d + y, d + k), d += g - E - (k - y);
        }), w.setSelection(j.create(w.doc, u, v)), this.view.dispatch(w);
      }
    }, this.codeMirrorKeymap = () => {
      const l = this.view;
      return [
        { key: "ArrowUp", run: () => this.maybeEscape("line", -1) },
        { key: "ArrowLeft", run: () => this.maybeEscape("char", -1) },
        { key: "ArrowDown", run: () => this.maybeEscape("line", 1) },
        { key: "ArrowRight", run: () => this.maybeEscape("char", 1) },
        {
          key: "Mod-Enter",
          run: () => ae(l.state, l.dispatch) ? (l.focus(), !0) : !1
        },
        { key: "Mod-z", run: () => ie(l.state, l.dispatch) },
        { key: "Shift-Mod-z", run: () => D(l.state, l.dispatch) },
        { key: "Mod-y", run: () => D(l.state, l.dispatch) },
        {
          key: "Backspace",
          run: () => {
            var c;
            const d = this.cm.state.selection.ranges;
            if (d.length > 1) return !1;
            const h = d[0];
            if (h && (!h.empty || h.anchor > 0) || this.cm.state.doc.lines >= 2) return !1;
            const u = this.view.state, v = (c = this.getPos()) != null ? c : 0, f = u.tr.replaceWith(
              v,
              v + this.node.nodeSize,
              u.schema.nodes.paragraph.createChecked({}, this.node.content)
            );
            return f.setSelection(j.near(f.doc.resolve(v))), this.view.dispatch(f), this.view.focus(), !0;
          }
        }
      ];
    }, this.maybeEscape = (l, c) => {
      var d;
      const { state: h } = this.cm;
      let u = h.selection.main;
      if (!u.empty || (l === "line" && (u = h.doc.lineAt(u.head)), c < 0 ? u.from > 0 : u.to < h.doc.length)) return !1;
      const v = ((d = this.getPos()) != null ? d : 0) + (c < 0 ? 0 : this.node.nodeSize), f = j.near(
        this.view.state.doc.resolve(v),
        c
      ), w = this.view.state.tr.setSelection(f).scrollIntoView();
      return this.view.dispatch(w), this.view.focus(), !0;
    }, this.setLanguage = (l) => {
      var c;
      this.view.dispatch(
        this.view.state.tr.setNodeAttribute(
          (c = this.getPos()) != null ? c : 0,
          "language",
          l
        )
      );
    }, this.getAllLanguages = () => this.loader.getAll(), this.languageConf = new H(), this.readOnlyConf = new H(), this.cm = new I({
      doc: this.node.textContent,
      root: this.view.root,
      extensions: [
        this.readOnlyConf.of(z.readOnly.of(!this.view.editable)),
        le.of(this.codeMirrorKeymap()),
        this.languageConf.of([]),
        ...o.extensions,
        I.updateListener.of(this.forwardUpdate)
      ]
    }), this.dom = this.createDom(), this.updateLanguage();
  }
  createDom() {
    const e = document.createElement("milkdown-code-block");
    e.codemirror = this.cm, e.getAllLanguages = this.getAllLanguages, e.setLanguage = this.setLanguage, e.isEditorReadonly = () => !this.view.editable, e.text = this.node.textContent;
    const t = this.config, {
      languages: n,
      extensions: i
    } = t, o = $e(t, [
      "languages",
      "extensions"
    ]);
    return e.config = o, e;
  }
  updateLanguage() {
    const e = this.node.attrs.language;
    if (e === this.languageName) return;
    this.dom.language = e, this.loader.load(e ?? "").then((n) => {
      n && (this.cm.dispatch({
        effects: this.languageConf.reconfigure(n)
      }), this.languageName = e);
    });
  }
  setSelection(e, t) {
    this.cm.dom.isConnected && (this.cm.focus(), this.updating = !0, this.cm.dispatch({ selection: { anchor: e, head: t } }), this.updating = !1);
  }
  update(e) {
    if (e.type !== this.node.type) return !1;
    if (this.updating) return !0;
    this.node = e, this.dom.text = e.textContent, this.updateLanguage(), this.view.editable === this.cm.state.readOnly && this.cm.dispatch({
      effects: this.readOnlyConf.reconfigure(
        z.readOnly.of(!this.view.editable)
      )
    });
    const t = _e(this.cm.state.doc.toString(), e.textContent);
    return t && (this.updating = !0, this.cm.dispatch({
      changes: { from: t.from, to: t.to, insert: t.text }
    }), this.updating = !1), !0;
  }
  selectNode() {
    this.dom.selected = !0, this.cm.focus();
  }
  deselectNode() {
    this.dom.selected = !1;
  }
  stopEvent() {
    return !0;
  }
  destroy() {
    this.cm.destroy();
  }
}
function _e(s, e) {
  if (s === e) return null;
  let t = 0, n = s.length, i = e.length;
  for (; t < n && s.charCodeAt(t) === e.charCodeAt(t); )
    ++t;
  for (; n > t && i > t && s.charCodeAt(n - 1) === e.charCodeAt(i - 1); )
    n--, i--;
  return { from: t, to: n, text: e.slice(t, i) };
}
class xe {
  constructor(e) {
    this.languages = e, this.map = {}, e.forEach((t) => {
      t.alias.forEach((n) => {
        this.map[n] = t;
      });
    });
  }
  getAll() {
    return this.languages.map((e) => ({
      name: e.name,
      alias: e.alias
    }));
  }
  load(e) {
    const n = this.map[e.toLowerCase()];
    return n ? n.support ? Promise.resolve(n.support) : n.load() : Promise.resolve(void 0);
  }
}
const G = ({
  selected: s = !1,
  codemirror: e,
  getAllLanguages: t,
  setLanguage: n,
  language: i,
  config: o,
  isEditorReadonly: l,
  text: c
}) => {
  var d, h, u, v, f;
  const w = de(), y = S(), k = S(), E = S(), g = S(), [C, P] = R(""), [_, T] = R(!1), [x, Q] = R(!1), F = B(() => w.current.getRootNode(), [w]);
  M(() => {
    var r;
    const a = (r = t == null ? void 0 : t()) == null ? void 0 : r.find(
      (p) => p.alias.some(
        (m) => m.toLowerCase() === (i == null ? void 0 : i.toLowerCase())
      )
    );
    a && a.name !== i && (n == null || n(a.name));
  }, [i]), M(() => {
    T(!1);
  }, [i]), M(() => {
    const r = (a) => {
      const p = a.target;
      if (y.current && y.current.contains(p)) return;
      const m = k.current;
      m && m.dataset.expanded === "true" && (m.contains(p) || T(!1));
    };
    return F.addEventListener("click", r), () => {
      F.removeEventListener("click", r);
    };
  }, []), ue(() => {
    P("");
    const r = y.current, a = k.current;
    !r || !a || pe(r, a, {
      placement: "bottom-start"
    }).then(({ x: p, y: m }) => {
      Object.assign(a.style, {
        left: `${p}px`,
        top: `${m}px`
      });
    });
  }, [_]);
  const O = B(() => {
    var r;
    if (!_) return [];
    const a = (r = t == null ? void 0 : t()) != null ? r : [], p = a.find(
      (L) => L.name.toLowerCase() === (i == null ? void 0 : i.toLowerCase())
    ), m = a.filter((L) => (L.name.toLowerCase().includes(C.toLowerCase()) || L.alias.some(
      (se) => se.toLowerCase().includes(C.toLowerCase())
    )) && L !== p);
    return m.length === 0 ? [] : p ? [p, ...m] : m;
  }, [C, _, i]), X = (r) => {
    const a = r.target;
    P(a.value);
  }, Y = (r) => {
    r.preventDefault(), r.stopPropagation(), !(l != null && l()) && T((a) => (a || setTimeout(() => {
      var p;
      return (p = E.current) == null ? void 0 : p.focus();
    }, 0), !a));
  }, Z = (r) => {
    r.preventDefault(), P("");
  }, V = (r) => {
    r.key === "Escape" && P("");
  }, ee = (r) => {
    if (r.key === "Enter") {
      const a = document.activeElement;
      a instanceof HTMLElement && a.dataset.language && (n == null || n(a.dataset.language));
    }
  }, te = B(() => O != null && O.length ? O.map(
    (r) => {
      var a;
      return N`<li
          role="listitem"
          tabindex="0"
          class="language-list-item"
          aria-selected=${r.name.toLowerCase() === (i == null ? void 0 : i.toLowerCase())}
          data-language=${r.name}
          onclick=${() => n == null ? void 0 : n(r.name)}
        >
          ${(a = o == null ? void 0 : o.renderLanguage) == null ? void 0 : a.call(
        o,
        r.name,
        r.name.toLowerCase() === (i == null ? void 0 : i.toLowerCase())
      )}
        </li>`;
    }
  ) : N`<li class="language-list-item no-result">
        ${o == null ? void 0 : o.noResultText}
      </li>`, [O]), b = B(() => {
    var r;
    return (r = o == null ? void 0 : o.renderPreview) == null ? void 0 : r.call(o, i ?? "", c ?? "");
  }, [i, c]);
  return M(() => {
    if (g.current) {
      for (; g.current.firstChild; )
        g.current.removeChild(g.current.firstChild);
      typeof b == "string" ? g.current.innerHTML = b : b instanceof HTMLElement && g.current.appendChild(b);
    }
  }, [b]), N`<host class=${$(s && "selected")}>
    <div class="tools">
      <button
        type="button"
        ref=${y}
        class="language-button"
        onpointerdown=${Y}
        data-expanded=${_}
      >
        ${i || "Text"}
        <div class="expand-icon">${(d = o == null ? void 0 : o.expandIcon) == null ? void 0 : d.call(o)}</div>
      </button>
      <div
        ref=${k}
        data-expanded=${_}
        class=${$("language-picker", _ && "show")}
      >
        <div class="list-wrapper">
          <div class="search-box">
            <div class="search-icon">${(h = o == null ? void 0 : o.searchIcon) == null ? void 0 : h.call(o)}</div>
            <input
              ref=${E}
              class="search-input"
              placeholder=${o == null ? void 0 : o.searchPlaceholder}
              value=${C}
              oninput=${X}
              onkeydown=${V}
            />
            <div
              class=${$("clear-icon", C.length === 0 && "hidden")}
              onmousedown=${Z}
            >
              ${(u = o == null ? void 0 : o.clearSearchIcon) == null ? void 0 : u.call(o)}
            </div>
          </div>
          <ul class="language-list" role="listbox" onkeydown=${ee}>
            ${te}
          </ul>
        </div>
      </div>
      <button
        class=${$("preview-toggle-button", !b && "hidden")}
        onclick=${() => Q(!x)}
      >
        ${(v = o == null ? void 0 : o.previewToggleButton) == null ? void 0 : v.call(o, x)}
      </button>
    </div>
    <div
      class=${$("codemirror-host", b && x && "hidden")}
    >
      ${he(e == null ? void 0 : e.dom, {})}
    </div>
    <div class=${$("preview-panel", !b && "hidden")}>
      <div class=${$("preview-divider", x && "hidden")}></div>
      <div class=${$("preview-label", x && "hidden")}>
        ${(f = o == null ? void 0 : o.previewLabel) == null ? void 0 : f.call(o)}
      </div>
      <div ref=${g} class="preview"></div>
    </div>
  </host>`;
};
G.props = {
  selected: Boolean,
  codemirror: Object,
  language: String,
  getAllLanguages: Function,
  setLanguage: Function,
  isEditorReadonly: Function,
  config: Object,
  text: String
};
const Le = ce(G);
ye("milkdown-code-block", Le);
const J = oe(
  re.node,
  (s) => {
    const e = s.get(A.key), t = new xe(e.languages);
    return (n, i, o) => new ke(n, i, o, t, e);
  }
);
q(J, {
  displayName: "NodeView<code-block>",
  group: "CodeBlock"
});
const Me = [
  J,
  A
];
export {
  Me as a,
  A as c
};
