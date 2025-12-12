import { k as a, l as c, m as i, n as l } from "./functions-Bsik6ikd-DXwZ6YmW.js";
class f extends Array {
  /**
   *
   * @param {any} initialState
   * @param {(nextState: any, state:any[], mount: boolean )=>void} mapState
   */
  constructor(u, r) {
    let s = !0;
    const o = (e) => {
      try {
        r(e, this, s);
      } finally {
        s = !1;
      }
    };
    super(void 0, o, r), o(u);
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
const m = (n) => {
  const u = i();
  return a(
    (r = new f(n, (s, o, e) => {
      s = l(s) ? s(o[0]) : s, s !== o[0] && (o[0] = s, e || u());
    })) => r
  );
}, t = (n, u) => {
  const [r] = a(([s, o, e = 0] = []) => ((!o || o && !c(o, u)) && (s = n()), [s, u, e]));
  return r;
}, p = (n, u) => t(() => n, u);
export {
  t as a,
  p as b,
  m as u
};
