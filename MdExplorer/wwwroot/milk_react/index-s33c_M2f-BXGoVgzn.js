import { m as O, k as J, a as K } from "./inline-latex-C9IGAXXQ-CNXYiJvx.js";
import { bz as E, bA as Q, b3 as j, b4 as tt, aO as B, bB as V, aw as A, bC as et, aR as nt, aW as rt, bD as at, bE as it, bF as ot, bG as ut, az as q, ay as lt } from "./integration-yhAlFHEe.js";
import { d as st } from "./index-D6fLMv29-BbUkr3pX.js";
import { r as ht, b as ct, j as mt, h as pt } from "./functions-Bsik6ikd-DXwZ6YmW.js";
import { n as dt } from "./index-4u43usGn.js";
import { c as xt } from "./index.es-BKDbnmQM.js";
import { t as wt, T as ft } from "./index.es-0Ra4NHnb.js";
function kt() {
  return {
    enter: {
      mathFlow: t,
      mathFlowFenceMeta: e,
      mathText: l
    },
    exit: {
      mathFlow: n,
      mathFlowFence: a,
      mathFlowFenceMeta: r,
      mathFlowValue: h,
      mathText: s,
      mathTextData: h
    }
  };
  function t(i) {
    const c = {
      type: "element",
      tagName: "code",
      properties: { className: ["language-math", "math-display"] },
      children: []
    };
    this.enter(
      {
        type: "math",
        meta: null,
        value: "",
        data: { hName: "pre", hChildren: [c] }
      },
      i
    );
  }
  function e() {
    this.buffer();
  }
  function r() {
    const i = this.resume(), c = this.stack[this.stack.length - 1];
    E(c.type === "math"), c.meta = i;
  }
  function a() {
    this.data.mathFlowInside || (this.buffer(), this.data.mathFlowInside = !0);
  }
  function n(i) {
    const c = this.resume().replace(/^(\r?\n|\r)|(\r?\n|\r)$/g, ""), m = this.stack[this.stack.length - 1];
    E(m.type === "math"), this.exit(i), m.value = c;
    const d = (
      /** @type {HastElement} */
      m.data.hChildren[0]
    );
    E(d.type === "element"), E(d.tagName === "code"), d.children.push({ type: "text", value: c }), this.data.mathFlowInside = void 0;
  }
  function l(i) {
    this.enter(
      {
        type: "inlineMath",
        value: "",
        data: {
          hName: "code",
          hProperties: { className: ["language-math", "math-inline"] },
          hChildren: []
        }
      },
      i
    ), this.buffer();
  }
  function s(i) {
    const c = this.resume(), m = this.stack[this.stack.length - 1];
    E(m.type === "inlineMath"), this.exit(i), m.value = c, /** @type {Array<HastElementContent>} */
    // @ts-expect-error: we defined it in `enterMathFlow`.
    m.data.hChildren.push({ type: "text", value: c });
  }
  function h(i) {
    this.config.enter.data.call(this, i), this.config.exit.data.call(this, i);
  }
}
function Mt(t) {
  let e = (t || {}).singleDollarTextMath;
  return e == null && (e = !0), a.peek = n, {
    unsafe: [
      { character: "\r", inConstruct: "mathFlowMeta" },
      { character: `
`, inConstruct: "mathFlowMeta" },
      {
        character: "$",
        after: e ? void 0 : "\\$",
        inConstruct: "phrasing"
      },
      { character: "$", inConstruct: "mathFlowMeta" },
      { atBreak: !0, character: "$", after: "\\$" }
    ],
    handlers: { math: r, inlineMath: a }
  };
  function r(l, s, h, i) {
    const c = l.value || "", m = h.createTracker(i), d = "$".repeat(Math.max(Q(c, "$") + 1, 2)), x = h.enter("mathFlow");
    let p = m.move(d);
    if (l.meta) {
      const w = h.enter("mathFlowMeta");
      p += m.move(
        h.safe(l.meta, {
          after: `
`,
          before: p,
          encode: ["$"],
          ...m.current()
        })
      ), w();
    }
    return p += m.move(`
`), c && (p += m.move(c + `
`)), p += m.move(d), x(), p;
  }
  function a(l, s, h) {
    let i = l.value || "", c = 1;
    for (e || c++; new RegExp("(^|[^$])" + "\\$".repeat(c) + "([^$]|$)").test(i); )
      c++;
    const m = "$".repeat(c);
    // Contains non-space.
    /[^ \r\n]/.test(i) && // Starts with space and ends with space.
    (/^[ \r\n]/.test(i) && /[ \r\n]$/.test(i) || // Starts or ends with dollar.
    /^\$|\$$/.test(i)) && (i = " " + i + " ");
    let d = -1;
    for (; ++d < h.unsafe.length; ) {
      const x = h.unsafe[d];
      if (!x.atBreak) continue;
      const p = h.compilePattern(x);
      let w;
      for (; w = p.exec(i); ) {
        let u = w.index;
        i.codePointAt(u) === 10 && i.codePointAt(u - 1) === 13 && u--, i = i.slice(0, u) + " " + i.slice(w.index + 1);
      }
    }
    return m + i + m;
  }
  function n() {
    return "$";
  }
}
function F(t) {
  return t !== null && t < -2;
}
function P(t) {
  return t === -2 || t === -1 || t === 32;
}
function S(t, e, r, a) {
  const n = a ? a - 1 : Number.POSITIVE_INFINITY;
  let l = 0;
  return s;
  function s(i) {
    return P(i) ? (t.enter(r), h(i)) : e(i);
  }
  function h(i) {
    return P(i) && l++ < n ? (t.consume(i), h) : (t.exit(r), e(i));
  }
}
const gt = {
  tokenize: Ft,
  concrete: !0,
  name: "mathFlow"
}, L = {
  tokenize: yt,
  partial: !0
};
function Ft(t, e, r) {
  const a = this, n = a.events[a.events.length - 1], l = n && n[1].type === "linePrefix" ? n[2].sliceSerialize(n[1], !0).length : 0;
  let s = 0;
  return h;
  function h(o) {
    return t.enter("mathFlow"), t.enter("mathFlowFence"), t.enter("mathFlowFenceSequence"), i(o);
  }
  function i(o) {
    return o === 36 ? (t.consume(o), s++, i) : s < 2 ? r(o) : (t.exit("mathFlowFenceSequence"), S(t, c, "whitespace")(o));
  }
  function c(o) {
    return o === null || F(o) ? d(o) : (t.enter("mathFlowFenceMeta"), t.enter("chunkString", {
      contentType: "string"
    }), m(o));
  }
  function m(o) {
    return o === null || F(o) ? (t.exit("chunkString"), t.exit("mathFlowFenceMeta"), d(o)) : o === 36 ? r(o) : (t.consume(o), m);
  }
  function d(o) {
    return t.exit("mathFlowFence"), a.interrupt ? e(o) : t.attempt(L, x, C)(o);
  }
  function x(o) {
    return t.attempt({
      tokenize: Y,
      partial: !0
    }, C, p)(o);
  }
  function p(o) {
    return (l ? S(t, w, "linePrefix", l + 1) : w)(o);
  }
  function w(o) {
    return o === null ? C(o) : F(o) ? t.attempt(L, x, C)(o) : (t.enter("mathFlowValue"), u(o));
  }
  function u(o) {
    return o === null || F(o) ? (t.exit("mathFlowValue"), w(o)) : (t.consume(o), u);
  }
  function C(o) {
    return t.exit("mathFlow"), e(o);
  }
  function Y(o, H, I) {
    let z = 0;
    return S(o, U, "linePrefix", a.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4);
    function U(k) {
      return o.enter("mathFlowFence"), o.enter("mathFlowFenceSequence"), N(k);
    }
    function N(k) {
      return k === 36 ? (z++, o.consume(k), N) : z < s ? I(k) : (o.exit("mathFlowFenceSequence"), S(o, Z, "whitespace")(k));
    }
    function Z(k) {
      return k === null || F(k) ? (o.exit("mathFlowFence"), H(k)) : I(k);
    }
  }
}
function yt(t, e, r) {
  const a = this;
  return n;
  function n(s) {
    return s === null ? e(s) : (t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), l);
  }
  function l(s) {
    return a.parser.lazy[a.now().line] ? r(s) : e(s);
  }
}
function vt(t) {
  let r = (t || {}).singleDollarTextMath;
  return r == null && (r = !0), {
    tokenize: a,
    resolve: Et,
    previous: Tt,
    name: "mathText"
  };
  function a(n, l, s) {
    let h = 0, i, c;
    return m;
    function m(u) {
      return n.enter("mathText"), n.enter("mathTextSequence"), d(u);
    }
    function d(u) {
      return u === 36 ? (n.consume(u), h++, d) : h < 2 && !r ? s(u) : (n.exit("mathTextSequence"), x(u));
    }
    function x(u) {
      return u === null ? s(u) : u === 36 ? (c = n.enter("mathTextSequence"), i = 0, w(u)) : u === 32 ? (n.enter("space"), n.consume(u), n.exit("space"), x) : F(u) ? (n.enter("lineEnding"), n.consume(u), n.exit("lineEnding"), x) : (n.enter("mathTextData"), p(u));
    }
    function p(u) {
      return u === null || u === 32 || u === 36 || F(u) ? (n.exit("mathTextData"), x(u)) : (n.consume(u), p);
    }
    function w(u) {
      return u === 36 ? (n.consume(u), i++, w) : i === h ? (n.exit("mathTextSequence"), n.exit("mathText"), l(u)) : (c.type = "mathTextData", p(u));
    }
  }
}
function Et(t) {
  let e = t.length - 4, r = 3, a, n;
  if ((t[r][1].type === "lineEnding" || t[r][1].type === "space") && (t[e][1].type === "lineEnding" || t[e][1].type === "space")) {
    for (a = r; ++a < e; )
      if (t[a][1].type === "mathTextData") {
        t[e][1].type = "mathTextPadding", t[r][1].type = "mathTextPadding", r += 2, e -= 2;
        break;
      }
  }
  for (a = r - 1, e++; ++a <= e; )
    n === void 0 ? a !== e && t[a][1].type !== "lineEnding" && (n = a) : (a === e || t[a][1].type === "lineEnding") && (t[n][1].type = "mathTextData", a !== n + 2 && (t[n][1].end = t[a - 1][1].end, t.splice(n + 2, a - n - 2), e -= a - n - 2, a = n + 2), n = void 0);
  return t;
}
function Tt(t) {
  return t !== 36 || this.events[this.events.length - 1][1].type === "characterEscape";
}
function Ct(t) {
  return {
    flow: {
      36: gt
    },
    text: {
      36: vt(t)
    }
  };
}
const St = {};
function bt(t) {
  const e = (
    /** @type {Processor} */
    this
  ), r = t || St, a = e.data(), n = a.micromarkExtensions || (a.micromarkExtensions = []), l = a.fromMarkdownExtensions || (a.fromMarkdownExtensions = []), s = a.toMarkdownExtensions || (a.toMarkdownExtensions = []);
  n.push(Ct(r)), l.push(kt()), s.push(Mt(r));
}
const $t = B(
  "remarkMath",
  () => bt
);
function _t(t) {
  return nt(
    t,
    "math",
    (e, r, a) => {
      const { value: n } = e, l = {
        type: "code",
        lang: "LaTeX",
        value: n
      };
      a.children.splice(r, 1, l);
    }
  );
}
const It = B(
  "remarkMathBlock",
  () => () => _t
), W = ({
  config: t,
  innerView: e,
  updateValue: r
}) => {
  var a;
  const n = (l) => {
    l.preventDefault(), r == null || r();
  };
  return pt`
    <host>
      <div class="container">
        ${e && mt(e.dom, {})}
        <button onmousedown=${n}>
          ${(a = t == null ? void 0 : t.inlineEditConfirm) == null ? void 0 : a.call(t)}
        </button>
      </div>
    </host>
  `;
};
W.props = {
  config: Object,
  innerView: Object,
  updateValue: Function
};
const R = ct(W), D = wt("INLINE_LATEX");
var X = (t) => {
  throw TypeError(t);
}, G = (t, e, r) => e.has(t) || X("Cannot " + r), f = (t, e, r) => (G(t, e, "read from private field"), r ? r.call(t) : e.get(t)), y = (t, e, r) => e.has(t) ? X("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), T = (t, e, r, a) => (G(t, e, "write to private field"), e.set(t, r), r), M, v, b, g, $, _;
class zt {
  constructor(e, r, a) {
    this.ctx = e, y(this, M, new R()), y(this, v), y(this, b), y(this, g), y(this, $, () => {
      f(this, g) && (f(this, g).destroy(), T(this, g, null));
    }), y(this, _, (n) => {
      const s = (() => {
        const { selection: h, schema: i } = n.state;
        if (h.empty || !(h instanceof rt)) return !1;
        const c = h.node;
        if (c.type.name !== K) return !1;
        const m = h.from, d = i.nodes.paragraph.create(
          null,
          i.text(c.attrs.value)
        ), x = new at(f(this, b), {
          state: it.create({
            doc: d,
            schema: new ut({
              nodes: {
                doc: {
                  content: "block+"
                },
                paragraph: {
                  content: "inline*",
                  group: "block",
                  parseDOM: [{ tag: "p" }],
                  toDOM() {
                    return ["p", 0];
                  }
                },
                text: {
                  group: "inline"
                }
              }
            }),
            plugins: [
              ot({
                "Mod-z": lt,
                "Mod-Z": q,
                "Mod-y": q,
                Enter: () => {
                  var p, w;
                  return (w = (p = f(this, M)).updateValue) == null || w.call(p), !0;
                }
              })
            ]
          })
        });
        return T(this, g, x), f(this, M).innerView = f(this, g), f(this, M).updateValue = () => {
          const { tr: p } = n.state;
          p.setNodeAttribute(m, "value", x.state.doc.textContent), n.dispatch(p), requestAnimationFrame(() => {
            n.focus();
          });
        }, !0;
      })();
      return s || f(this, $).call(this), s;
    }), this.update = (n, l) => {
      f(this, v).update(n, l);
    }, this.destroy = () => {
      f(this, v).destroy(), f(this, M).remove();
    }, T(this, v, new ft({
      debounce: 0,
      content: f(this, M),
      shouldShow: f(this, _),
      offset: 10,
      floatingUIOptions: {
        placement: "bottom"
      }
    })), f(this, M).config = a, f(this, v).update(r), T(this, b, document.createElement("div")), T(this, g, null);
  }
}
M = /* @__PURE__ */ new WeakMap();
v = /* @__PURE__ */ new WeakMap();
b = /* @__PURE__ */ new WeakMap();
g = /* @__PURE__ */ new WeakMap();
$ = /* @__PURE__ */ new WeakMap();
_ = /* @__PURE__ */ new WeakMap();
const Nt = V(
  (t) => dt(/(?:\$)([^$]+)(?:\$)$/, O.type(t), {
    getAttr: (e) => {
      var r;
      return {
        value: (r = e[1]) != null ? r : ""
      };
    }
  })
), qt = V(
  (t) => et(/^\$\$[\s\n]$/, A.type(t), () => ({
    language: "LaTeX"
  }))
), Pt = A.extendSchema((t) => (e) => {
  const r = t(e);
  return {
    ...r,
    toMarkdown: {
      match: r.toMarkdown.match,
      runner: (a, n) => {
        var l, s;
        if (((l = n.attrs.language) != null ? l : "").toLowerCase() === "latex")
          a.addNode(
            "math",
            void 0,
            ((s = n.content.firstChild) == null ? void 0 : s.text) || ""
          );
        else
          return r.toMarkdown.runner(a, n);
      }
    }
  };
});
st("milkdown-latex-inline-edit", R);
const Xt = (t, e) => {
  t.config((r) => {
    if (!r.get(j).includes(tt.CodeMirror))
      throw new Error("You need to enable CodeMirror to use LaTeX feature");
    r.update(xt.key, (l) => ({
      ...l,
      renderPreview: (s, h) => {
        if (s.toLowerCase() === "latex" && h.length > 0)
          return Lt(h, e == null ? void 0 : e.katexOptions);
        const i = l.renderPreview;
        return i(s, h);
      }
    })), r.set(D.key, {
      view: (l) => {
        var s;
        return new zt(r, l, {
          inlineEditConfirm: (s = e == null ? void 0 : e.inlineEditConfirm) != null ? s : () => ht,
          ...e
        });
      }
    });
  }).use($t).use(It).use(O).use(D).use(Nt).use(qt).use(Pt);
};
function Lt(t, e) {
  return J.renderToString(t, {
    ...e,
    throwOnError: !1,
    displayMode: !0
  });
}
export {
  Xt as defineFeature
};
