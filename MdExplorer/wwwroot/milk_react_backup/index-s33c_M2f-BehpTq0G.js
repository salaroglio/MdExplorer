import { m as D, k as J, a as K } from "./inline-latex-C9IGAXXQ-nhqqOT-l.js";
import { a8 as E, a9 as Q, s as j, C as tt, B, aa as V, y as A, ab as et, H as nt, N as at, ac as rt, ad as it, ae as ot, af as ut, ag as q, ah as lt } from "./integration-BHt4Ncdo.js";
import { d as st } from "./index-D6fLMv29-BbUkr3pX.js";
import { l as ht, b as ct, T as mt, h as pt } from "./functions-Bsik6ikd-DhlhsyMB.js";
import { n as dt } from "./index-jM38O6Kt.js";
import { O as xt } from "./index.es-Dganzb1x.js";
import { t as wt, T as ft } from "./index.es-Cho9Kr7B.js";
function kt() {
  return {
    enter: {
      mathFlow: t,
      mathFlowFenceMeta: e,
      mathText: l
    },
    exit: {
      mathFlow: n,
      mathFlowFence: r,
      mathFlowFenceMeta: a,
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
  function a() {
    const i = this.resume(), c = this.stack[this.stack.length - 1];
    E(c.type === "math"), c.meta = i;
  }
  function r() {
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
function gt(t) {
  let e = (t || {}).singleDollarTextMath;
  return e == null && (e = !0), r.peek = n, {
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
    handlers: { math: a, inlineMath: r }
  };
  function a(l, s, h, i) {
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
  function r(l, s, h) {
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
function S(t, e, a, r) {
  const n = r ? r - 1 : Number.POSITIVE_INFINITY;
  let l = 0;
  return s;
  function s(i) {
    return P(i) ? (t.enter(a), h(i)) : e(i);
  }
  function h(i) {
    return P(i) && l++ < n ? (t.consume(i), h) : (t.exit(a), e(i));
  }
}
const Mt = {
  tokenize: Ft,
  concrete: !0,
  name: "mathFlow"
}, L = {
  tokenize: yt,
  partial: !0
};
function Ft(t, e, a) {
  const r = this, n = r.events[r.events.length - 1], l = n && n[1].type === "linePrefix" ? n[2].sliceSerialize(n[1], !0).length : 0;
  let s = 0;
  return h;
  function h(o) {
    return t.enter("mathFlow"), t.enter("mathFlowFence"), t.enter("mathFlowFenceSequence"), i(o);
  }
  function i(o) {
    return o === 36 ? (t.consume(o), s++, i) : s < 2 ? a(o) : (t.exit("mathFlowFenceSequence"), S(t, c, "whitespace")(o));
  }
  function c(o) {
    return o === null || F(o) ? d(o) : (t.enter("mathFlowFenceMeta"), t.enter("chunkString", {
      contentType: "string"
    }), m(o));
  }
  function m(o) {
    return o === null || F(o) ? (t.exit("chunkString"), t.exit("mathFlowFenceMeta"), d(o)) : o === 36 ? a(o) : (t.consume(o), m);
  }
  function d(o) {
    return t.exit("mathFlowFence"), r.interrupt ? e(o) : t.attempt(L, x, C)(o);
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
  function Y(o, G, b) {
    let z = 0;
    return S(o, U, "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4);
    function U(k) {
      return o.enter("mathFlowFence"), o.enter("mathFlowFenceSequence"), N(k);
    }
    function N(k) {
      return k === 36 ? (z++, o.consume(k), N) : z < s ? b(k) : (o.exit("mathFlowFenceSequence"), S(o, Z, "whitespace")(k));
    }
    function Z(k) {
      return k === null || F(k) ? (o.exit("mathFlowFence"), G(k)) : b(k);
    }
  }
}
function yt(t, e, a) {
  const r = this;
  return n;
  function n(s) {
    return s === null ? e(s) : (t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), l);
  }
  function l(s) {
    return r.parser.lazy[r.now().line] ? a(s) : e(s);
  }
}
function vt(t) {
  let a = (t || {}).singleDollarTextMath;
  return a == null && (a = !0), {
    tokenize: r,
    resolve: Et,
    previous: Tt,
    name: "mathText"
  };
  function r(n, l, s) {
    let h = 0, i, c;
    return m;
    function m(u) {
      return n.enter("mathText"), n.enter("mathTextSequence"), d(u);
    }
    function d(u) {
      return u === 36 ? (n.consume(u), h++, d) : h < 2 && !a ? s(u) : (n.exit("mathTextSequence"), x(u));
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
  let e = t.length - 4, a = 3, r, n;
  if ((t[a][1].type === "lineEnding" || t[a][1].type === "space") && (t[e][1].type === "lineEnding" || t[e][1].type === "space")) {
    for (r = a; ++r < e; )
      if (t[r][1].type === "mathTextData") {
        t[e][1].type = "mathTextPadding", t[a][1].type = "mathTextPadding", a += 2, e -= 2;
        break;
      }
  }
  for (r = a - 1, e++; ++r <= e; )
    n === void 0 ? r !== e && t[r][1].type !== "lineEnding" && (n = r) : (r === e || t[r][1].type === "lineEnding") && (t[n][1].type = "mathTextData", r !== n + 2 && (t[n][1].end = t[r - 1][1].end, t.splice(n + 2, r - n - 2), e -= r - n - 2, r = n + 2), n = void 0);
  return t;
}
function Tt(t) {
  return t !== 36 || this.events[this.events.length - 1][1].type === "characterEscape";
}
function Ct(t) {
  return {
    flow: {
      36: Mt
    },
    text: {
      36: vt(t)
    }
  };
}
const St = {};
function $t(t) {
  const e = (
    /** @type {Processor} */
    this
  ), a = t || St, r = e.data(), n = r.micromarkExtensions || (r.micromarkExtensions = []), l = r.fromMarkdownExtensions || (r.fromMarkdownExtensions = []), s = r.toMarkdownExtensions || (r.toMarkdownExtensions = []);
  n.push(Ct(a)), l.push(kt()), s.push(gt(a));
}
const _t = B(
  "remarkMath",
  () => $t
);
function It(t) {
  return nt(
    t,
    "math",
    (e, a, r) => {
      const { value: n } = e, l = {
        type: "code",
        lang: "LaTeX",
        value: n
      };
      r.children.splice(a, 1, l);
    }
  );
}
const bt = B(
  "remarkMathBlock",
  () => () => It
), W = ({
  config: t,
  innerView: e,
  updateValue: a
}) => {
  var r;
  const n = (l) => {
    l.preventDefault(), a == null || a();
  };
  return pt`
    <host>
      <div class="container">
        ${e && mt(e.dom, {})}
        <button onmousedown=${n}>
          ${(r = t == null ? void 0 : t.inlineEditConfirm) == null ? void 0 : r.call(t)}
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
const R = ct(W), O = wt("INLINE_LATEX");
var X = (t) => {
  throw TypeError(t);
}, H = (t, e, a) => e.has(t) || X("Cannot " + a), f = (t, e, a) => (H(t, e, "read from private field"), a ? a.call(t) : e.get(t)), y = (t, e, a) => e.has(t) ? X("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), T = (t, e, a, r) => (H(t, e, "write to private field"), e.set(t, a), a), g, v, $, M, _, I;
class zt {
  constructor(e, a, r) {
    this.ctx = e, y(this, g, new R()), y(this, v), y(this, $), y(this, M), y(this, _, () => {
      f(this, M) && (f(this, M).destroy(), T(this, M, null));
    }), y(this, I, (n) => {
      const s = (() => {
        const { selection: h, schema: i } = n.state;
        if (h.empty || !(h instanceof at)) return !1;
        const c = h.node;
        if (c.type.name !== K) return !1;
        const m = h.from, d = i.nodes.paragraph.create(
          null,
          i.text(c.attrs.value)
        ), x = new rt(f(this, $), {
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
                  return (w = (p = f(this, g)).updateValue) == null || w.call(p), !0;
                }
              })
            ]
          })
        });
        return T(this, M, x), f(this, g).innerView = f(this, M), f(this, g).updateValue = () => {
          const { tr: p } = n.state;
          p.setNodeAttribute(m, "value", x.state.doc.textContent), n.dispatch(p), requestAnimationFrame(() => {
            n.focus();
          });
        }, !0;
      })();
      return s || f(this, _).call(this), s;
    }), this.update = (n, l) => {
      f(this, v).update(n, l);
    }, this.destroy = () => {
      f(this, v).destroy(), f(this, g).remove();
    }, T(this, v, new ft({
      debounce: 0,
      content: f(this, g),
      shouldShow: f(this, I),
      offset: 10,
      floatingUIOptions: {
        placement: "bottom"
      }
    })), f(this, g).config = r, f(this, v).update(a), T(this, $, document.createElement("div")), T(this, M, null);
  }
}
g = /* @__PURE__ */ new WeakMap();
v = /* @__PURE__ */ new WeakMap();
$ = /* @__PURE__ */ new WeakMap();
M = /* @__PURE__ */ new WeakMap();
_ = /* @__PURE__ */ new WeakMap();
I = /* @__PURE__ */ new WeakMap();
const Nt = V(
  (t) => dt(/(?:\$)([^$]+)(?:\$)$/, D.type(t), {
    getAttr: (e) => {
      var a;
      return {
        value: (a = e[1]) != null ? a : ""
      };
    }
  })
), qt = V(
  (t) => et(/^\$\$[\s\n]$/, A.type(t), () => ({
    language: "LaTeX"
  }))
), Pt = A.extendSchema((t) => (e) => {
  const a = t(e);
  return {
    ...a,
    toMarkdown: {
      match: a.toMarkdown.match,
      runner: (r, n) => {
        var l, s;
        if (((l = n.attrs.language) != null ? l : "").toLowerCase() === "latex")
          r.addNode(
            "math",
            void 0,
            ((s = n.content.firstChild) == null ? void 0 : s.text) || ""
          );
        else
          return a.toMarkdown.runner(r, n);
      }
    }
  };
});
st("milkdown-latex-inline-edit", R);
const Xt = (t, e) => {
  t.config((a) => {
    if (!a.get(j).includes(tt.CodeMirror))
      throw new Error("You need to enable CodeMirror to use LaTeX feature");
    a.update(xt.key, (l) => ({
      ...l,
      renderPreview: (s, h) => {
        if (s.toLowerCase() === "latex" && h.length > 0)
          return Lt(h, e == null ? void 0 : e.katexOptions);
        const i = l.renderPreview;
        return i(s, h);
      }
    })), a.set(O.key, {
      view: (l) => {
        var s;
        return new zt(a, l, {
          inlineEditConfirm: (s = e == null ? void 0 : e.inlineEditConfirm) != null ? s : () => ht,
          ...e
        });
      }
    });
  }).use(_t).use(bt).use(D).use(O).use(Nt).use(qt).use(Pt);
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
