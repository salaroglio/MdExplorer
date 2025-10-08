import { U as a, V as c, K as i, W as l } from "./functions-Bsik6ikd-DhlhsyMB.js";
class f extends Array {
  /**
   *
   * @param {any} initialState
   * @param {(nextState: any, state:any[], mount: boolean )=>void} mapState
   */
  constructor(u, n) {
    let s = !0;
    const o = (e) => {
      try {
        n(e, this, s);
      } finally {
        s = !1;
      }
    };
    super(void 0, o, n), o(u);
  }
  /**
   * The following code allows a mutable approach to useState
   * and useProp this with the idea of allowing an alternative
   * approach similar to Vue or Qwik of state management
   * @todo pending review with the community
   */
  // get value() {
  //     return this[0];
  // }
  // set value(nextState) {
  //     this[2](nextState, this);
  // }
}
const p = (r) => {
  const u = i();
  return a(
    (n = new f(r, (s, o, e) => {
      s = l(s) ? s(o[0]) : s, s !== o[0] && (o[0] = s, e || u());
    })) => n
  );
}, t = (r, u) => {
  const [n] = a(([s, o, e = 0] = []) => ((!o || o && !c(o, u)) && (s = r()), [s, u, e]));
  return n;
}, y = (r, u) => t(() => r, u);
export {
  t as a,
  y as b,
  p as u
};
