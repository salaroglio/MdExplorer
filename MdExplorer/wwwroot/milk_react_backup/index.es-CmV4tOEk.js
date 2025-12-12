import { $ as W, B as q, E as J, a as Q, G as X, H as Z, c as ee } from "./integration-BHt4Ncdo.js";
import { h as x, b as te, d as B, p as L, i as b, u as oe } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { u as w, a as re } from "./hooks-CCspbR-r.js";
var ne = Object.defineProperty, H = Object.getOwnPropertySymbols, ae = Object.prototype.hasOwnProperty, ie = Object.prototype.propertyIsEnumerable, M = (e, t, o) => t in e ? ne(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, le = (e, t) => {
  for (var o in t || (t = {}))
    ae.call(t, o) && M(e, o, t[o]);
  if (H)
    for (var o of H(t))
      ie.call(t, o) && M(e, o, t[o]);
  return e;
};
function _(e, t) {
  return Object.assign(e, {
    meta: le({
      package: "@milkdown/components"
    }, t)
  }), e;
}
var se = Object.defineProperty, U = Object.getOwnPropertySymbols, ce = Object.prototype.hasOwnProperty, ue = Object.prototype.propertyIsEnumerable, j = (e, t, o) => t in e ? se(e, t, { enumerable: !0, configurable: !0, writable: !0, value: o }) : e[t] = o, de = (e, t) => {
  for (var o in t || (t = {}))
    ce.call(t, o) && j(e, o, t[o]);
  if (U)
    for (var o of U(t))
      ue.call(t, o) && j(e, o, t[o]);
  return e;
};
const C = "image-block", O = J("image-block", () => ({
  inline: !1,
  group: "block",
  selectable: !0,
  draggable: !0,
  isolating: !0,
  marks: "",
  atom: !0,
  priority: 100,
  attrs: {
    src: { default: "" },
    caption: { default: "" },
    ratio: { default: 1 }
  },
  parseDOM: [
    {
      tag: `img[data-type="${C}"]`,
      getAttrs: (e) => {
        var t;
        if (!(e instanceof HTMLElement)) throw X(e);
        return {
          src: e.getAttribute("src") || "",
          caption: e.getAttribute("caption") || "",
          ratio: Number((t = e.getAttribute("ratio")) != null ? t : 1)
        };
      }
    }
  ],
  toDOM: (e) => ["img", de({ "data-type": C }, e.attrs)],
  parseMarkdown: {
    match: ({ type: e }) => e === "image-block",
    runner: (e, t, o) => {
      const u = t.url, n = t.title;
      let l = Number(t.alt || 1);
      (Number.isNaN(l) || l === 0) && (l = 1), e.addNode(o, {
        src: u,
        caption: n,
        ratio: l
      });
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "image-block",
    runner: (e, t) => {
      e.openNode("paragraph"), e.addNode("image", void 0, void 0, {
        title: t.attrs.caption,
        url: t.attrs.src,
        alt: `${Number.parseFloat(t.attrs.ratio).toFixed(2)}`
      }), e.closeNode();
    }
  }
}));
_(O.node, {
  displayName: "NodeSchema<image-block>",
  group: "ImageBlock"
});
function pe(e) {
  return Z(
    e,
    "paragraph",
    (t, o, u) => {
      var n, l;
      if (((n = t.children) == null ? void 0 : n.length) !== 1) return;
      const a = (l = t.children) == null ? void 0 : l[0];
      if (!a || a.type !== "image") return;
      const { url: c, alt: r, title: i } = a, d = {
        type: "image-block",
        url: c,
        alt: r,
        title: i
      };
      u.children.splice(o, 1, d);
    }
  );
}
const R = q(
  "remark-image-block",
  () => () => pe
);
_(R.plugin, {
  displayName: "Remark<remarkImageBlock>",
  group: "ImageBlock"
});
_(R.options, {
  displayName: "RemarkConfig<remarkImageBlock>",
  group: "ImageBlock"
});
const me = {
  imageIcon: () => "🌌",
  captionIcon: () => "💬",
  uploadButton: () => x`Upload file`,
  confirmButton: () => x`Confirm ⏎`,
  uploadPlaceholderText: "or paste the image link ...",
  captionPlaceholderText: "Image caption",
  onUpload: (e) => Promise.resolve(URL.createObjectURL(e))
}, T = W(
  me,
  "imageBlockConfigCtx"
);
_(T, {
  displayName: "Config<image-block>",
  group: "ImageBlock"
});
function ge(e, t) {
  const o = customElements.get(e);
  if (o == null) {
    customElements.define(e, t);
    return;
  }
  o !== t && console.warn(`Custom element ${e} has been defined before.`);
}
function fe({
  image: e,
  resizeHandle: t,
  ratio: o,
  setRatio: u,
  src: n,
  readonly: l
}) {
  const a = oe(), c = re(() => a.current.getRootNode(), [a]);
  L(() => {
    const r = e.current;
    r && (delete r.dataset.origin, delete r.dataset.height, r.style.height = "");
  }, [n]), L(() => {
    const r = t.current, i = e.current;
    if (!r || !i) return;
    const d = (p) => {
      p.preventDefault();
      const f = i.getBoundingClientRect().top, m = p.clientY - f, h = Number(m < 100 ? 100 : m).toFixed(2);
      i.dataset.height = h, i.style.height = `${h}px`;
    }, k = () => {
      c.removeEventListener("pointermove", d), c.removeEventListener("pointerup", k);
      const p = Number(i.dataset.origin), f = Number(i.dataset.height), m = Number.parseFloat(
        Number(f / p).toFixed(2)
      );
      Number.isNaN(m) || u(m);
    }, N = (p) => {
      l || (p.preventDefault(), c.addEventListener("pointermove", d), c.addEventListener("pointerup", k));
    }, y = (p) => {
      const f = a.current.getBoundingClientRect().width;
      if (!f) return;
      const m = p.target, h = m.height, E = m.width, I = E < f ? h : f * (h / E), P = (I * o).toFixed(2);
      i.dataset.origin = I.toFixed(2), i.dataset.height = P, i.style.height = `${P}px`;
    };
    return i.addEventListener("load", y), r.addEventListener("pointerdown", N), () => {
      i.removeEventListener("load", y), r.removeEventListener("pointerdown", N);
    };
  }, []);
}
var ve = (e, t, o) => new Promise((u, n) => {
  var l = (r) => {
    try {
      c(o.next(r));
    } catch (i) {
      n(i);
    }
  }, a = (r) => {
    try {
      c(o.throw(r));
    } catch (i) {
      n(i);
    }
  }, c = (r) => r.done ? u(r.value) : Promise.resolve(r.value).then(l, a);
  c((o = o.apply(e, t)).next());
});
let $ = 0;
const he = ee("abcdefg", 8), A = ({
  src: e = "",
  caption: t = "",
  ratio: o = 1,
  selected: u = !1,
  readonly: n = !1,
  setAttr: l,
  config: a
}) => {
  const c = B(), r = B(), i = B(), [d, k] = w(t.length > 0), [N, y] = w(e.length !== 0), [p] = w(he()), [f, m] = w(!1), [h, E] = w(e);
  fe({
    image: c,
    resizeHandle: r,
    ratio: o,
    setRatio: (s) => l == null ? void 0 : l("ratio", s),
    src: e,
    readonly: n
  }), L(() => {
    u || k(t.length > 0);
  }, [u]);
  const I = (s) => {
    const v = s.target.value;
    $ && window.clearTimeout($), $ = window.setTimeout(() => {
      l == null || l("caption", v);
    }, 1e3);
  }, P = (s) => {
    const v = s.target.value;
    $ && (window.clearTimeout($), $ = 0), l == null || l("caption", v);
  }, z = (s) => {
    const v = s.target.value;
    y(v.length !== 0), E(v);
  }, G = (s) => ve(void 0, null, function* () {
    var g;
    const v = (g = s.target.files) == null ? void 0 : g[0];
    if (!v) return;
    const S = yield a == null ? void 0 : a.onUpload(v);
    S && (l == null || l("src", S), y(!0));
  }), Y = (s) => {
    s.preventDefault(), s.stopPropagation(), !n && k((g) => !g);
  }, D = () => {
    var s, g;
    l == null || l("src", (g = (s = i.current) == null ? void 0 : s.value) != null ? g : "");
  }, K = (s) => {
    s.key === "Enter" && D();
  }, F = (s) => {
    s.preventDefault(), s.stopPropagation();
  };
  return x`<host class=${b(u && "selected")}>
    <div class=${b("image-edit", e.length > 0 && "hidden")}>
      <div class="image-icon">${a == null ? void 0 : a.imageIcon()}</div>
      <div class=${b("link-importer", f && "focus")}>
        <input
          ref=${i}
          draggable="true"
          ondragstart=${F}
          disabled=${n}
          class="link-input-area"
          value=${h}
          oninput=${z}
          onkeydown=${K}
          onfocus=${() => m(!0)}
          onblur=${() => m(!1)}
        />
        <div class=${b("placeholder", N && "hidden")}>
          <input
            disabled=${n}
            class="hidden"
            id=${p}
            type="file"
            accept="image/*"
            onchange=${G}
          />
          <label class="uploader" for=${p}>
            ${a == null ? void 0 : a.uploadButton()}
          </label>
          <span class="text" onclick=${() => {
    var s;
    return (s = i.current) == null ? void 0 : s.focus();
  }}>
            ${a == null ? void 0 : a.uploadPlaceholderText}
          </span>
        </div>
      </div>
      <div
        class=${b("confirm", h.length === 0 && "hidden")}
        onclick=${() => D()}
      >
        ${a == null ? void 0 : a.confirmButton()}
      </div>
    </div>
    <div class=${b("image-wrapper", e.length === 0 && "hidden")}>
      <div class="operation">
        <div class="operation-item" onpointerdown=${Y}>
          ${a == null ? void 0 : a.captionIcon()}
        </div>
      </div>
      <img
        ref=${c}
        data-type=${C}
        src=${e}
        alt=${t}
        ratio=${o}
      />
      <div ref=${r} class="image-resize-handle"></div>
    </div>
    <input
      draggable="true"
      ondragstart=${F}
      class=${b("caption-input", !d && "hidden")}
      placeholder=${a == null ? void 0 : a.captionPlaceholderText}
      oninput=${I}
      onblur=${P}
      value=${t}
    />
  </host>`;
};
A.props = {
  src: String,
  caption: String,
  ratio: Number,
  selected: Boolean,
  readonly: Boolean,
  setAttr: Function,
  config: Object
};
const be = te(A);
ge("milkdown-image-block", be);
const V = Q(
  O.node,
  (e) => (t, o, u) => {
    const n = document.createElement(
      "milkdown-image-block"
    ), l = e.get(T.key), a = l.proxyDomURL, c = (r) => {
      if (!a)
        n.src = r.attrs.src;
      else {
        const i = a(r.attrs.src);
        typeof i == "string" ? n.src = i : i.then((d) => {
          n.src = d;
        });
      }
      n.ratio = r.attrs.ratio, n.caption = r.attrs.caption, n.readonly = !o.editable;
    };
    return c(t), n.selected = !1, n.setAttr = (r, i) => {
      const d = u();
      d != null && o.dispatch(o.state.tr.setNodeAttribute(d, r, i));
    }, n.config = l, {
      dom: n,
      update: (r) => r.type !== t.type ? !1 : (c(r), !0),
      stopEvent: (r) => r.target instanceof HTMLInputElement,
      selectNode: () => {
        n.selected = !0;
      },
      deselectNode: () => {
        n.selected = !1;
      },
      destroy: () => {
        n.remove();
      }
    };
  }
);
_(V, {
  displayName: "NodeView<image-block>",
  group: "ImageBlock"
});
const we = [
  R,
  O,
  V,
  T
].flat();
export {
  we as a,
  O as b,
  T as i
};
