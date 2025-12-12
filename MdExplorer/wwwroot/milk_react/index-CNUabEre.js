import { aK as y, aL as Y } from "./integration-Fz_xek3V.js";
var T, _;
function Z() {
  if (_) return T;
  _ = 1;
  var x = "Expected a function", I = NaN, C = "[object Symbol]", W = /^\s+|\s+$/g, F = /^[-+]0x[0-9a-f]+$/i, M = /^0b[01]+$/i, N = /^0o[0-7]+$/i, R = parseInt, $ = typeof y == "object" && y && y.Object === Object && y, A = typeof self == "object" && self && self.Object === Object && self, B = $ || A || Function("return this")(), q = Object.prototype, D = q.toString, G = Math.max, P = Math.min, h = function() {
    return B.Date.now();
  };
  function H(t, r, n) {
    var a, o, g, c, i, u, s = 0, E = !1, l = !1, j = !0;
    if (typeof t != "function")
      throw new TypeError(x);
    r = O(r) || 0, m(n) && (E = !!n.leading, l = "maxWait" in n, g = l ? G(O(n.maxWait) || 0, r) : g, j = "trailing" in n ? !!n.trailing : j);
    function p(e) {
      var f = a, d = o;
      return a = o = void 0, s = e, c = t.apply(d, f), c;
    }
    function z(e) {
      return s = e, i = setTimeout(b, r), E ? p(e) : c;
    }
    function J(e) {
      var f = e - u, d = e - s, k = r - f;
      return l ? P(k, g - d) : k;
    }
    function S(e) {
      var f = e - u, d = e - s;
      return u === void 0 || f >= r || f < 0 || l && d >= g;
    }
    function b() {
      var e = h();
      if (S(e))
        return L(e);
      i = setTimeout(b, J(e));
    }
    function L(e) {
      return i = void 0, j && a ? p(e) : (a = o = void 0, c);
    }
    function Q() {
      i !== void 0 && clearTimeout(i), s = 0, a = u = o = i = void 0;
    }
    function V() {
      return i === void 0 ? c : L(h());
    }
    function v() {
      var e = h(), f = S(e);
      if (a = arguments, o = this, u = e, f) {
        if (i === void 0)
          return z(u);
        if (l)
          return i = setTimeout(b, r), p(u);
      }
      return i === void 0 && (i = setTimeout(b, r)), c;
    }
    return v.cancel = Q, v.flush = V, v;
  }
  function K(t, r, n) {
    var a = !0, o = !0;
    if (typeof t != "function")
      throw new TypeError(x);
    return m(n) && (a = "leading" in n ? !!n.leading : a, o = "trailing" in n ? !!n.trailing : o), H(t, r, {
      leading: a,
      maxWait: r,
      trailing: o
    });
  }
  function m(t) {
    var r = typeof t;
    return !!t && (r == "object" || r == "function");
  }
  function U(t) {
    return !!t && typeof t == "object";
  }
  function X(t) {
    return typeof t == "symbol" || U(t) && D.call(t) == C;
  }
  function O(t) {
    if (typeof t == "number")
      return t;
    if (X(t))
      return I;
    if (m(t)) {
      var r = typeof t.valueOf == "function" ? t.valueOf() : t;
      t = m(r) ? r + "" : r;
    }
    if (typeof t != "string")
      return t === 0 ? t : +t;
    t = t.replace(W, "");
    var n = M.test(t);
    return n || N.test(t) ? R(t.slice(2), n ? 2 : 8) : F.test(t) ? I : +t;
  }
  return T = K, T;
}
var w = Z();
const et = /* @__PURE__ */ Y(w);
export {
  et as t
};
