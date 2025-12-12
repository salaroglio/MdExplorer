import { h as f, b as R, d as T, i as h, l as D, n as U, o as S } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { $ as j, a as F, i as N, c as V } from "./integration-BHt4Ncdo.js";
import { u as I } from "./hooks-CCspbR-r.js";
import { i as A, a as H } from "./index.es-CmV4tOEk.js";
var M = Object.defineProperty, b = Object.getOwnPropertySymbols, K = Object.prototype.hasOwnProperty, W = Object.prototype.propertyIsEnumerable, P = (l, e, o) => e in l ? M(l, e, { enumerable: !0, configurable: !0, writable: !0, value: o }) : l[e] = o, q = (l, e) => {
  for (var o in e || (e = {}))
    K.call(e, o) && P(l, o, e[o]);
  if (b)
    for (var o of b(e))
      W.call(e, o) && P(l, o, e[o]);
  return l;
};
function g(l, e) {
  return Object.assign(l, {
    meta: q({
      package: "@milkdown/components"
    }, e)
  }), l;
}
const z = {
  imageIcon: () => "🌌",
  uploadButton: () => f`Upload`,
  confirmButton: () => f`⏎`,
  uploadPlaceholderText: "/Paste",
  onUpload: (l) => Promise.resolve(URL.createObjectURL(l))
}, y = j(
  z,
  "inlineImageConfigCtx"
);
g(y, {
  displayName: "Config<image-inline>",
  group: "ImageInline"
});
function G(l, e) {
  const o = customElements.get(l);
  if (o == null) {
    customElements.define(l, e);
    return;
  }
  o !== e && console.warn(`Custom element ${l} has been defined before.`);
}
var J = (l, e, o) => new Promise((d, n) => {
  var a = (t) => {
    try {
      i(o.next(t));
    } catch (r) {
      n(r);
    }
  }, s = (t) => {
    try {
      i(o.throw(t));
    } catch (r) {
      n(r);
    }
  }, i = (t) => t.done ? d(t.value) : Promise.resolve(t.value).then(a, s);
  i((o = o.apply(l, e)).next());
});
const Q = V("abcdefg", 8), x = ({
  src: l = "",
  selected: e = !1,
  alt: o,
  title: d,
  setAttr: n,
  config: a
}) => {
  const s = T(), [i] = I(Q()), [t, r] = I(!1), [c, m] = I(l.length !== 0), [_, w] = I(l), L = (u) => {
    const v = u.target.value;
    m(v.length !== 0), w(v);
  }, B = (u) => J(void 0, null, function* () {
    var p;
    const v = (p = u.target.files) == null ? void 0 : p[0];
    if (!v) return;
    const $ = yield a == null ? void 0 : a.onUpload(v);
    $ && (n == null || n("src", $), m(!0));
  }), k = () => {
    var u, p;
    n == null || n("src", (p = (u = s.current) == null ? void 0 : u.value) != null ? p : "");
  }, O = (u) => {
    u.key === "Enter" && k();
  }, E = (u) => {
    u.preventDefault(), u.stopPropagation();
  };
  return f`<host class=${h(e && "selected", !l && "empty")}>
    ${l ? f`<img class="image-inline" src=${l} alt=${o} title=${d} />` : f`<div class="empty-image-inline">
          <div class="image-icon">${a == null ? void 0 : a.imageIcon()}</div>
          <div class=${h("link-importer", t && "focus")}>
            <input
              draggable="true"
              ref=${s}
              ondragstart=${E}
              class="link-input-area"
              value=${_}
              oninput=${L}
              onkeydown=${O}
              onfocus=${() => r(!0)}
              onblur=${() => r(!1)}
            />
            <div class=${h("placeholder", c && "hidden")}>
              <input
                class="hidden"
                id=${i}
                type="file"
                accept="image/*"
                onchange=${B}
              />
              <label class="uploader" for=${i}>
                ${a == null ? void 0 : a.uploadButton()}
              </label>
              <span class="text" onclick=${() => {
    var u;
    return (u = s.current) == null ? void 0 : u.focus();
  }}>
                ${a == null ? void 0 : a.uploadPlaceholderText}
              </span>
            </div>
          </div>
          <div
            class=${h("confirm", _.length === 0 && "hidden")}
            onclick=${() => k()}
          >
            ${a == null ? void 0 : a.confirmButton()}
          </div>
        </div>`}
  </host>`;
};
x.props = {
  src: String,
  alt: String,
  title: String,
  selected: Boolean,
  setAttr: Function,
  config: Object
};
const X = R(x);
G("milkdown-image-inline", X);
const C = F(
  N.node,
  (l) => (e, o, d) => {
    const n = document.createElement(
      "milkdown-image-inline"
    ), a = l.get(y.key), s = a.proxyDomURL, i = (t) => {
      if (!s)
        n.src = t.attrs.src;
      else {
        const r = s(t.attrs.src);
        typeof r == "string" ? n.src = r : r.then((c) => {
          n.src = c;
        });
      }
      n.alt = t.attrs.alt, n.title = t.attrs.title;
    };
    return i(e), n.selected = !1, n.setAttr = (t, r) => {
      const c = d();
      c != null && o.dispatch(o.state.tr.setNodeAttribute(c, t, r));
    }, n.config = a, {
      dom: n,
      update: (t) => t.type !== e.type ? !1 : (i(t), !0),
      stopEvent: (t) => !!(n.selected && t.target instanceof HTMLInputElement),
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
g(C, {
  displayName: "NodeView<image-inline>",
  group: "ImageInline"
});
const Y = [
  y,
  C
], te = (l, e) => {
  l.config((o) => {
    o.update(y.key, (d) => {
      var n, a, s, i, t, r;
      return {
        uploadButton: (n = e == null ? void 0 : e.inlineUploadButton) != null ? n : () => "Upload",
        imageIcon: (a = e == null ? void 0 : e.inlineImageIcon) != null ? a : () => U,
        confirmButton: (s = e == null ? void 0 : e.inlineConfirmButton) != null ? s : () => D,
        uploadPlaceholderText: (i = e == null ? void 0 : e.inlineUploadPlaceholderText) != null ? i : "or paste link",
        onUpload: (r = (t = e == null ? void 0 : e.inlineOnUpload) != null ? t : e == null ? void 0 : e.onUpload) != null ? r : d.onUpload,
        proxyDomURL: e == null ? void 0 : e.proxyDomURL
      };
    }), o.update(A.key, (d) => {
      var n, a, s, i, t, r, c, m;
      return {
        uploadButton: (n = e == null ? void 0 : e.blockUploadButton) != null ? n : () => "Upload file",
        imageIcon: (a = e == null ? void 0 : e.blockImageIcon) != null ? a : () => U,
        captionIcon: (s = e == null ? void 0 : e.blockCaptionIcon) != null ? s : () => S,
        confirmButton: (i = e == null ? void 0 : e.blockConfirmButton) != null ? i : () => "Confirm",
        captionPlaceholderText: (t = e == null ? void 0 : e.blockCaptionPlaceholderText) != null ? t : "Write Image Caption",
        uploadPlaceholderText: (r = e == null ? void 0 : e.blockUploadPlaceholderText) != null ? r : "or paste link",
        onUpload: (m = (c = e == null ? void 0 : e.blockOnUpload) != null ? c : e == null ? void 0 : e.onUpload) != null ? m : d.onUpload,
        proxyDomURL: e == null ? void 0 : e.proxyDomURL
      };
    });
  }).use(H).use(Y);
};
export {
  te as defineFeature
};
