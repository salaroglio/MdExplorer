var Eg = Object.defineProperty;
var Og = (t, e, n) => e in t ? Eg(t, e, { enumerable: !0, configurable: !0, writable: !0, value: n }) : t[e] = n;
var Qn = (t, e, n) => Og(t, typeof e != "symbol" ? e + "" : e, n);
var Ue = /* @__PURE__ */ ((t) => (t.docTypeError = "docTypeError", t.contextNotFound = "contextNotFound", t.timerNotFound = "timerNotFound", t.ctxCallOutOfScope = "ctxCallOutOfScope", t.createNodeInParserFail = "createNodeInParserFail", t.stackOverFlow = "stackOverFlow", t.parserMatchError = "parserMatchError", t.serializerMatchError = "serializerMatchError", t.getAtomFromSchemaFail = "getAtomFromSchemaFail", t.expectDomTypeError = "expectDomTypeError", t.callCommandBeforeEditorView = "callCommandBeforeEditorView", t.missingRootElement = "missingRootElement", t.missingNodeInSchema = "missingNodeInSchema", t.missingMarkInSchema = "missingMarkInSchema", t.ctxNotBind = "ctxNotBind", t.missingYjsDoc = "missingYjsDoc", t))(Ue || {});
let it = class extends Error {
  constructor(e, n) {
    super(n), this.name = "MilkdownError", this.code = e;
  }
};
const Dg = (t, e) => typeof e == "function" ? "[Function]" : e, xr = (t) => JSON.stringify(t, Dg);
function Rg(t) {
  return new it(
    Ue.docTypeError,
    `Doc type error, unsupported type: ${xr(t)}`
  );
}
function vg(t) {
  return new it(
    Ue.contextNotFound,
    `Context "${t}" not found, do you forget to inject it?`
  );
}
function Pg(t) {
  return new it(
    Ue.timerNotFound,
    `Timer "${t}" not found, do you forget to record it?`
  );
}
function ha() {
  return new it(
    Ue.ctxCallOutOfScope,
    "Should not call a context out of the plugin."
  );
}
function zg(...t) {
  const e = t.reduce((n, r) => {
    if (!r) return n;
    const i = (o) => Array.isArray(o) ? o.map((s) => i(s)).join(", ") : o.toJSON ? xr(
      o.toJSON()
    ) : o.spec ? xr(o.spec) : o.toString();
    return `${n}, ${i(r)}`;
  }, "Create prosemirror node from remark failed in parser");
  return new it(Ue.createNodeInParserFail, e);
}
function uf() {
  return new it(
    Ue.stackOverFlow,
    "Stack over flow, cannot pop on an empty stack."
  );
}
function Lg(t) {
  return new it(
    Ue.parserMatchError,
    `Cannot match target parser for node: ${xr(t)}.`
  );
}
function Fg(t) {
  return new it(
    Ue.serializerMatchError,
    `Cannot match target serializer for node: ${xr(t)}.`
  );
}
function Et(t) {
  return new it(
    Ue.expectDomTypeError,
    `Expect to be a dom, but get: ${xr(t)}.`
  );
}
function Bg() {
  return new it(
    Ue.callCommandBeforeEditorView,
    "You're trying to call a command before editor view initialized, make sure to get commandManager from ctx after editor view has been initialized"
  );
}
function _g(t) {
  return new it(
    Ue.missingNodeInSchema,
    `Missing node in schema, milkdown cannot find "${t}" in schema.`
  );
}
function $g(t) {
  return new it(
    Ue.missingMarkInSchema,
    `Missing mark in schema, milkdown cannot find "${t}" in schema.`
  );
}
var hf = (t) => {
  throw TypeError(t);
}, ff = (t, e, n) => e.has(t) || hf("Cannot " + n), P = (t, e, n) => (ff(t, e, "read from private field"), n ? n.call(t) : e.get(t)), ue = (t, e, n) => e.has(t) ? hf("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, n), Q = (t, e, n, r) => (ff(t, e, "write to private field"), e.set(t, n), n), kt, Kt, _r, so, lo, ao, er, $r, Mn, Vr, Wr, Hr, Jt, Ut, co, ct, tr, Nn, jr, Gt, qr, uo;
class df {
  constructor() {
    this.sliceMap = /* @__PURE__ */ new Map(), this.get = (e) => {
      const n = typeof e == "string" ? [...this.sliceMap.values()].find((r) => r.type.name === e) : this.sliceMap.get(e.id);
      if (!n) {
        const r = typeof e == "string" ? e : e.name;
        throw vg(r);
      }
      return n;
    }, this.remove = (e) => {
      const n = typeof e == "string" ? [...this.sliceMap.values()].find((r) => r.type.name === e) : this.sliceMap.get(e.id);
      n && this.sliceMap.delete(n.type.id);
    }, this.has = (e) => typeof e == "string" ? [...this.sliceMap.values()].some((n) => n.type.name === e) : this.sliceMap.has(e.id);
  }
}
let Vg = class {
  /// @internal
  constructor(e, n, r) {
    ue(this, kt), ue(this, Kt), ue(this, _r), Q(this, kt, []), Q(this, _r, () => {
      P(this, kt).forEach((i) => i(P(this, Kt)));
    }), this.set = (i) => {
      Q(this, Kt, i), P(this, _r).call(this);
    }, this.get = () => P(this, Kt), this.update = (i) => {
      Q(this, Kt, i(P(this, Kt))), P(this, _r).call(this);
    }, this.type = r, Q(this, Kt, n), e.set(r.id, this);
  }
  /// Add a watcher for changes in the slice.
  /// Returns a function to remove the watcher.
  on(e) {
    return P(this, kt).push(e), () => {
      Q(this, kt, P(this, kt).filter((n) => n !== e));
    };
  }
  /// Add a one-time watcher for changes in the slice.
  /// The watcher will be removed after it is called.
  /// Returns a function to remove the watcher.
  once(e) {
    const n = this.on((r) => {
      e(r), n();
    });
    return n;
  }
  /// Remove a watcher.
  off(e) {
    Q(this, kt, P(this, kt).filter((n) => n !== e));
  }
  /// Remove all watchers.
  offAll() {
    Q(this, kt, []);
  }
};
kt = /* @__PURE__ */ new WeakMap();
Kt = /* @__PURE__ */ new WeakMap();
_r = /* @__PURE__ */ new WeakMap();
class Wg {
  /// Create a slice type with a default value and a name.
  /// The name should be unique in the container.
  constructor(e, n) {
    this.id = Symbol(`Context-${n}`), this.name = n, this._defaultValue = e, this._typeInfo = () => {
      throw ha();
    };
  }
  /// Create a slice with a container.
  /// You can also pass a value to override the default value.
  create(e, n = this._defaultValue) {
    return new Vg(e, n, this);
  }
}
const Y = (t, e) => new Wg(t, e);
class Hg {
  /// Create an inspector with container, clock and metadata.
  constructor(e, n, r) {
    ue(this, so), ue(this, lo), ue(this, ao), ue(this, er), ue(this, $r), ue(this, Mn), ue(this, Vr), ue(this, Wr), ue(this, Hr), Q(this, er, /* @__PURE__ */ new Set()), Q(this, $r, /* @__PURE__ */ new Set()), Q(this, Mn, /* @__PURE__ */ new Map()), Q(this, Vr, /* @__PURE__ */ new Map()), this.read = () => ({
      metadata: P(this, so),
      injectedSlices: [...P(this, er)].map((i) => ({
        name: typeof i == "string" ? i : i.name,
        value: P(this, Wr).call(this, i)
      })),
      consumedSlices: [...P(this, $r)].map((i) => ({
        name: typeof i == "string" ? i : i.name,
        value: P(this, Wr).call(this, i)
      })),
      recordedTimers: [...P(this, Mn)].map(
        ([i, { duration: o }]) => ({
          name: i.name,
          duration: o,
          status: P(this, Hr).call(this, i)
        })
      ),
      waitTimers: [...P(this, Vr)].map(([i, { duration: o }]) => ({
        name: i.name,
        duration: o,
        status: P(this, Hr).call(this, i)
      }))
    }), this.onRecord = (i) => {
      P(this, Mn).set(i, { start: Date.now(), duration: 0 });
    }, this.onClear = (i) => {
      P(this, Mn).delete(i);
    }, this.onDone = (i) => {
      const o = P(this, Mn).get(i);
      o && (o.duration = Date.now() - o.start);
    }, this.onWait = (i, o) => {
      const s = Date.now();
      o.finally(() => {
        P(this, Vr).set(i, { duration: Date.now() - s });
      });
    }, this.onInject = (i) => {
      P(this, er).add(i);
    }, this.onRemove = (i) => {
      P(this, er).delete(i);
    }, this.onUse = (i) => {
      P(this, $r).add(i);
    }, Q(this, Wr, (i) => P(this, lo).get(i).get()), Q(this, Hr, (i) => P(this, ao).get(i).status), Q(this, lo, e), Q(this, ao, n), Q(this, so, r);
  }
}
so = /* @__PURE__ */ new WeakMap();
lo = /* @__PURE__ */ new WeakMap();
ao = /* @__PURE__ */ new WeakMap();
er = /* @__PURE__ */ new WeakMap();
$r = /* @__PURE__ */ new WeakMap();
Mn = /* @__PURE__ */ new WeakMap();
Vr = /* @__PURE__ */ new WeakMap();
Wr = /* @__PURE__ */ new WeakMap();
Hr = /* @__PURE__ */ new WeakMap();
const jg = class pf {
  /// Create a ctx object with container and clock.
  constructor(e, n, r) {
    ue(this, Jt), ue(this, Ut), ue(this, co), ue(this, ct), this.produce = (i) => i && Object.keys(i).length ? new pf(P(this, Jt), P(this, Ut), { ...i }) : this, this.inject = (i, o) => {
      var s;
      const l = i.create(P(this, Jt).sliceMap);
      return o != null && l.set(o), (s = P(this, ct)) == null || s.onInject(i), this;
    }, this.remove = (i) => {
      var o;
      return P(this, Jt).remove(i), (o = P(this, ct)) == null || o.onRemove(i), this;
    }, this.record = (i) => {
      var o;
      return i.create(P(this, Ut).store), (o = P(this, ct)) == null || o.onRecord(i), this;
    }, this.clearTimer = (i) => {
      var o;
      return P(this, Ut).remove(i), (o = P(this, ct)) == null || o.onClear(i), this;
    }, this.isInjected = (i) => P(this, Jt).has(i), this.isRecorded = (i) => P(this, Ut).has(i), this.use = (i) => {
      var o;
      return (o = P(this, ct)) == null || o.onUse(i), P(this, Jt).get(i);
    }, this.get = (i) => this.use(i).get(), this.set = (i, o) => this.use(i).set(o), this.update = (i, o) => this.use(i).update(o), this.timer = (i) => P(this, Ut).get(i), this.done = (i) => {
      var o;
      this.timer(i).done(), (o = P(this, ct)) == null || o.onDone(i);
    }, this.wait = (i) => {
      var o;
      const s = this.timer(i).start();
      return (o = P(this, ct)) == null || o.onWait(i, s), s;
    }, this.waitTimers = async (i) => {
      await Promise.all(this.get(i).map((o) => this.wait(o)));
    }, Q(this, Jt, e), Q(this, Ut, n), Q(this, co, r), r && Q(this, ct, new Hg(e, n, r));
  }
  /// Get metadata of the ctx.
  get meta() {
    return P(this, co);
  }
  /// Get the inspector of the ctx.
  get inspector() {
    return P(this, ct);
  }
};
Jt = /* @__PURE__ */ new WeakMap();
Ut = /* @__PURE__ */ new WeakMap();
co = /* @__PURE__ */ new WeakMap();
ct = /* @__PURE__ */ new WeakMap();
let qg = jg;
class Kg {
  constructor() {
    this.store = /* @__PURE__ */ new Map(), this.get = (e) => {
      const n = this.store.get(e.id);
      if (!n) throw Pg(e.name);
      return n;
    }, this.remove = (e) => {
      this.store.delete(e.id);
    }, this.has = (e) => this.store.has(e.id);
  }
}
class Jg {
  /// @internal
  constructor(e, n) {
    ue(this, tr), ue(this, Nn), ue(this, jr), ue(this, Gt), ue(this, qr), ue(this, uo), Q(this, tr, null), Q(this, Nn, null), Q(this, Gt, "pending"), this.start = () => (P(this, tr) ?? Q(this, tr, new Promise((r, i) => {
      Q(this, Nn, (o) => {
        o instanceof CustomEvent && o.detail.id === P(this, jr) && (Q(this, Gt, "resolved"), P(this, qr).call(this), o.stopImmediatePropagation(), r());
      }), P(this, uo).call(this, () => {
        P(this, Gt) === "pending" && Q(this, Gt, "rejected"), P(this, qr).call(this), i(new Error(`Timing ${this.type.name} timeout.`));
      }), Q(this, Gt, "pending"), addEventListener(this.type.name, P(this, Nn));
    })), P(this, tr)), this.done = () => {
      const r = new CustomEvent(this.type.name, {
        detail: { id: P(this, jr) }
      });
      dispatchEvent(r);
    }, Q(this, qr, () => {
      P(this, Nn) && removeEventListener(this.type.name, P(this, Nn));
    }), Q(this, uo, (r) => {
      setTimeout(() => {
        r();
      }, this.type.timeout);
    }), Q(this, jr, Symbol(n.name)), this.type = n, e.set(n.id, this);
  }
  /// The status of the timer.
  /// Can be `pending`, `resolved` or `rejected`.
  get status() {
    return P(this, Gt);
  }
}
tr = /* @__PURE__ */ new WeakMap();
Nn = /* @__PURE__ */ new WeakMap();
jr = /* @__PURE__ */ new WeakMap();
Gt = /* @__PURE__ */ new WeakMap();
qr = /* @__PURE__ */ new WeakMap();
uo = /* @__PURE__ */ new WeakMap();
class Ug {
  /// Create a timer type with a name and a timeout.
  /// The name should be unique in the clock.
  constructor(e, n = 3e3) {
    this.create = (r) => new Jg(r, this), this.id = Symbol(`Timer-${e}`), this.name = e, this.timeout = n;
  }
}
const pn = (t, e = 3e3) => new Ug(t, e);
function we(t) {
  this.content = t;
}
we.prototype = {
  constructor: we,
  find: function(t) {
    for (var e = 0; e < this.content.length; e += 2)
      if (this.content[e] === t) return e;
    return -1;
  },
  // :: (string) → ?any
  // Retrieve the value stored under `key`, or return undefined when
  // no such key exists.
  get: function(t) {
    var e = this.find(t);
    return e == -1 ? void 0 : this.content[e + 1];
  },
  // :: (string, any, ?string) → OrderedMap
  // Create a new map by replacing the value of `key` with a new
  // value, or adding a binding to the end of the map. If `newKey` is
  // given, the key of the binding will be replaced with that key.
  update: function(t, e, n) {
    var r = n && n != t ? this.remove(n) : this, i = r.find(t), o = r.content.slice();
    return i == -1 ? o.push(n || t, e) : (o[i + 1] = e, n && (o[i] = n)), new we(o);
  },
  // :: (string) → OrderedMap
  // Return a map with the given key removed, if it existed.
  remove: function(t) {
    var e = this.find(t);
    if (e == -1) return this;
    var n = this.content.slice();
    return n.splice(e, 2), new we(n);
  },
  // :: (string, any) → OrderedMap
  // Add a new key to the start of the map.
  addToStart: function(t, e) {
    return new we([t, e].concat(this.remove(t).content));
  },
  // :: (string, any) → OrderedMap
  // Add a new key to the end of the map.
  addToEnd: function(t, e) {
    var n = this.remove(t).content.slice();
    return n.push(t, e), new we(n);
  },
  // :: (string, string, any) → OrderedMap
  // Add a key after the given key. If `place` is not found, the new
  // key is added to the end.
  addBefore: function(t, e, n) {
    var r = this.remove(e), i = r.content.slice(), o = r.find(t);
    return i.splice(o == -1 ? i.length : o, 0, e, n), new we(i);
  },
  // :: ((key: string, value: any))
  // Call the given function for each key/value pair in the map, in
  // order.
  forEach: function(t) {
    for (var e = 0; e < this.content.length; e += 2)
      t(this.content[e], this.content[e + 1]);
  },
  // :: (union<Object, OrderedMap>) → OrderedMap
  // Create a new map by prepending the keys in this map that don't
  // appear in `map` before the keys in `map`.
  prepend: function(t) {
    return t = we.from(t), t.size ? new we(t.content.concat(this.subtract(t).content)) : this;
  },
  // :: (union<Object, OrderedMap>) → OrderedMap
  // Create a new map by appending the keys in this map that don't
  // appear in `map` after the keys in `map`.
  append: function(t) {
    return t = we.from(t), t.size ? new we(this.subtract(t).content.concat(t.content)) : this;
  },
  // :: (union<Object, OrderedMap>) → OrderedMap
  // Create a map containing all the keys in this map that don't
  // appear in `map`.
  subtract: function(t) {
    var e = this;
    t = we.from(t);
    for (var n = 0; n < t.content.length; n += 2)
      e = e.remove(t.content[n]);
    return e;
  },
  // :: () → Object
  // Turn ordered map into a plain object.
  toObject: function() {
    var t = {};
    return this.forEach(function(e, n) {
      t[e] = n;
    }), t;
  },
  // :: number
  // The amount of keys in this map.
  get size() {
    return this.content.length >> 1;
  }
};
we.from = function(t) {
  if (t instanceof we) return t;
  var e = [];
  if (t) for (var n in t) e.push(n, t[n]);
  return new we(e);
};
function mf(t, e, n) {
  for (let r = 0; ; r++) {
    if (r == t.childCount || r == e.childCount)
      return t.childCount == e.childCount ? null : n;
    let i = t.child(r), o = e.child(r);
    if (i == o) {
      n += i.nodeSize;
      continue;
    }
    if (!i.sameMarkup(o))
      return n;
    if (i.isText && i.text != o.text) {
      for (let s = 0; i.text[s] == o.text[s]; s++)
        n++;
      return n;
    }
    if (i.content.size || o.content.size) {
      let s = mf(i.content, o.content, n + 1);
      if (s != null)
        return s;
    }
    n += i.nodeSize;
  }
}
function gf(t, e, n, r) {
  for (let i = t.childCount, o = e.childCount; ; ) {
    if (i == 0 || o == 0)
      return i == o ? null : { a: n, b: r };
    let s = t.child(--i), l = e.child(--o), a = s.nodeSize;
    if (s == l) {
      n -= a, r -= a;
      continue;
    }
    if (!s.sameMarkup(l))
      return { a: n, b: r };
    if (s.isText && s.text != l.text) {
      let c = 0, u = Math.min(s.text.length, l.text.length);
      for (; c < u && s.text[s.text.length - c - 1] == l.text[l.text.length - c - 1]; )
        c++, n--, r--;
      return { a: n, b: r };
    }
    if (s.content.size || l.content.size) {
      let c = gf(s.content, l.content, n - 1, r - 1);
      if (c)
        return c;
    }
    n -= a, r -= a;
  }
}
class T {
  /**
  @internal
  */
  constructor(e, n) {
    if (this.content = e, this.size = n || 0, n == null)
      for (let r = 0; r < e.length; r++)
        this.size += e[r].nodeSize;
  }
  /**
  Invoke a callback for all descendant nodes between the given two
  positions (relative to start of this fragment). Doesn't descend
  into a node when the callback returns `false`.
  */
  nodesBetween(e, n, r, i = 0, o) {
    for (let s = 0, l = 0; l < n; s++) {
      let a = this.content[s], c = l + a.nodeSize;
      if (c > e && r(a, i + l, o || null, s) !== !1 && a.content.size) {
        let u = l + 1;
        a.nodesBetween(Math.max(0, e - u), Math.min(a.content.size, n - u), r, i + u);
      }
      l = c;
    }
  }
  /**
  Call the given callback for every descendant node. `pos` will be
  relative to the start of the fragment. The callback may return
  `false` to prevent traversal of a given node's children.
  */
  descendants(e) {
    this.nodesBetween(0, this.size, e);
  }
  /**
  Extract the text between `from` and `to`. See the same method on
  [`Node`](https://prosemirror.net/docs/ref/#model.Node.textBetween).
  */
  textBetween(e, n, r, i) {
    let o = "", s = !0;
    return this.nodesBetween(e, n, (l, a) => {
      let c = l.isText ? l.text.slice(Math.max(e, a) - a, n - a) : l.isLeaf ? i ? typeof i == "function" ? i(l) : i : l.type.spec.leafText ? l.type.spec.leafText(l) : "" : "";
      l.isBlock && (l.isLeaf && c || l.isTextblock) && r && (s ? s = !1 : o += r), o += c;
    }, 0), o;
  }
  /**
  Create a new fragment containing the combined content of this
  fragment and the other.
  */
  append(e) {
    if (!e.size)
      return this;
    if (!this.size)
      return e;
    let n = this.lastChild, r = e.firstChild, i = this.content.slice(), o = 0;
    for (n.isText && n.sameMarkup(r) && (i[i.length - 1] = n.withText(n.text + r.text), o = 1); o < e.content.length; o++)
      i.push(e.content[o]);
    return new T(i, this.size + e.size);
  }
  /**
  Cut out the sub-fragment between the two given positions.
  */
  cut(e, n = this.size) {
    if (e == 0 && n == this.size)
      return this;
    let r = [], i = 0;
    if (n > e)
      for (let o = 0, s = 0; s < n; o++) {
        let l = this.content[o], a = s + l.nodeSize;
        a > e && ((s < e || a > n) && (l.isText ? l = l.cut(Math.max(0, e - s), Math.min(l.text.length, n - s)) : l = l.cut(Math.max(0, e - s - 1), Math.min(l.content.size, n - s - 1))), r.push(l), i += l.nodeSize), s = a;
      }
    return new T(r, i);
  }
  /**
  @internal
  */
  cutByIndex(e, n) {
    return e == n ? T.empty : e == 0 && n == this.content.length ? this : new T(this.content.slice(e, n));
  }
  /**
  Create a new fragment in which the node at the given index is
  replaced by the given node.
  */
  replaceChild(e, n) {
    let r = this.content[e];
    if (r == n)
      return this;
    let i = this.content.slice(), o = this.size + n.nodeSize - r.nodeSize;
    return i[e] = n, new T(i, o);
  }
  /**
  Create a new fragment by prepending the given node to this
  fragment.
  */
  addToStart(e) {
    return new T([e].concat(this.content), this.size + e.nodeSize);
  }
  /**
  Create a new fragment by appending the given node to this
  fragment.
  */
  addToEnd(e) {
    return new T(this.content.concat(e), this.size + e.nodeSize);
  }
  /**
  Compare this fragment to another one.
  */
  eq(e) {
    if (this.content.length != e.content.length)
      return !1;
    for (let n = 0; n < this.content.length; n++)
      if (!this.content[n].eq(e.content[n]))
        return !1;
    return !0;
  }
  /**
  The first child of the fragment, or `null` if it is empty.
  */
  get firstChild() {
    return this.content.length ? this.content[0] : null;
  }
  /**
  The last child of the fragment, or `null` if it is empty.
  */
  get lastChild() {
    return this.content.length ? this.content[this.content.length - 1] : null;
  }
  /**
  The number of child nodes in this fragment.
  */
  get childCount() {
    return this.content.length;
  }
  /**
  Get the child node at the given index. Raise an error when the
  index is out of range.
  */
  child(e) {
    let n = this.content[e];
    if (!n)
      throw new RangeError("Index " + e + " out of range for " + this);
    return n;
  }
  /**
  Get the child node at the given index, if it exists.
  */
  maybeChild(e) {
    return this.content[e] || null;
  }
  /**
  Call `f` for every child node, passing the node, its offset
  into this parent node, and its index.
  */
  forEach(e) {
    for (let n = 0, r = 0; n < this.content.length; n++) {
      let i = this.content[n];
      e(i, r, n), r += i.nodeSize;
    }
  }
  /**
  Find the first position at which this fragment and another
  fragment differ, or `null` if they are the same.
  */
  findDiffStart(e, n = 0) {
    return mf(this, e, n);
  }
  /**
  Find the first position, searching from the end, at which this
  fragment and the given fragment differ, or `null` if they are
  the same. Since this position will not be the same in both
  nodes, an object with two separate positions is returned.
  */
  findDiffEnd(e, n = this.size, r = e.size) {
    return gf(this, e, n, r);
  }
  /**
  Find the index and inner offset corresponding to a given relative
  position in this fragment. The result object will be reused
  (overwritten) the next time the function is called. @internal
  */
  findIndex(e, n = -1) {
    if (e == 0)
      return Gi(0, e);
    if (e == this.size)
      return Gi(this.content.length, e);
    if (e > this.size || e < 0)
      throw new RangeError(`Position ${e} outside of fragment (${this})`);
    for (let r = 0, i = 0; ; r++) {
      let o = this.child(r), s = i + o.nodeSize;
      if (s >= e)
        return s == e || n > 0 ? Gi(r + 1, s) : Gi(r, i);
      i = s;
    }
  }
  /**
  Return a debugging string that describes this fragment.
  */
  toString() {
    return "<" + this.toStringInner() + ">";
  }
  /**
  @internal
  */
  toStringInner() {
    return this.content.join(", ");
  }
  /**
  Create a JSON-serializeable representation of this fragment.
  */
  toJSON() {
    return this.content.length ? this.content.map((e) => e.toJSON()) : null;
  }
  /**
  Deserialize a fragment from its JSON representation.
  */
  static fromJSON(e, n) {
    if (!n)
      return T.empty;
    if (!Array.isArray(n))
      throw new RangeError("Invalid input for Fragment.fromJSON");
    return new T(n.map(e.nodeFromJSON));
  }
  /**
  Build a fragment from an array of nodes. Ensures that adjacent
  text nodes with the same marks are joined together.
  */
  static fromArray(e) {
    if (!e.length)
      return T.empty;
    let n, r = 0;
    for (let i = 0; i < e.length; i++) {
      let o = e[i];
      r += o.nodeSize, i && o.isText && e[i - 1].sameMarkup(o) ? (n || (n = e.slice(0, i)), n[n.length - 1] = o.withText(n[n.length - 1].text + o.text)) : n && n.push(o);
    }
    return new T(n || e, r);
  }
  /**
  Create a fragment from something that can be interpreted as a
  set of nodes. For `null`, it returns the empty fragment. For a
  fragment, the fragment itself. For a node or array of nodes, a
  fragment containing those nodes.
  */
  static from(e) {
    if (!e)
      return T.empty;
    if (e instanceof T)
      return e;
    if (Array.isArray(e))
      return this.fromArray(e);
    if (e.attrs)
      return new T([e], e.nodeSize);
    throw new RangeError("Can not convert " + e + " to a Fragment" + (e.nodesBetween ? " (looks like multiple versions of prosemirror-model were loaded)" : ""));
  }
}
T.empty = new T([], 0);
const Is = { index: 0, offset: 0 };
function Gi(t, e) {
  return Is.index = t, Is.offset = e, Is;
}
function Vo(t, e) {
  if (t === e)
    return !0;
  if (!(t && typeof t == "object") || !(e && typeof e == "object"))
    return !1;
  let n = Array.isArray(t);
  if (Array.isArray(e) != n)
    return !1;
  if (n) {
    if (t.length != e.length)
      return !1;
    for (let r = 0; r < t.length; r++)
      if (!Vo(t[r], e[r]))
        return !1;
  } else {
    for (let r in t)
      if (!(r in e) || !Vo(t[r], e[r]))
        return !1;
    for (let r in e)
      if (!(r in t))
        return !1;
  }
  return !0;
}
class G {
  /**
  @internal
  */
  constructor(e, n) {
    this.type = e, this.attrs = n;
  }
  /**
  Given a set of marks, create a new set which contains this one as
  well, in the right position. If this mark is already in the set,
  the set itself is returned. If any marks that are set to be
  [exclusive](https://prosemirror.net/docs/ref/#model.MarkSpec.excludes) with this mark are present,
  those are replaced by this one.
  */
  addToSet(e) {
    let n, r = !1;
    for (let i = 0; i < e.length; i++) {
      let o = e[i];
      if (this.eq(o))
        return e;
      if (this.type.excludes(o.type))
        n || (n = e.slice(0, i));
      else {
        if (o.type.excludes(this.type))
          return e;
        !r && o.type.rank > this.type.rank && (n || (n = e.slice(0, i)), n.push(this), r = !0), n && n.push(o);
      }
    }
    return n || (n = e.slice()), r || n.push(this), n;
  }
  /**
  Remove this mark from the given set, returning a new set. If this
  mark is not in the set, the set itself is returned.
  */
  removeFromSet(e) {
    for (let n = 0; n < e.length; n++)
      if (this.eq(e[n]))
        return e.slice(0, n).concat(e.slice(n + 1));
    return e;
  }
  /**
  Test whether this mark is in the given set of marks.
  */
  isInSet(e) {
    for (let n = 0; n < e.length; n++)
      if (this.eq(e[n]))
        return !0;
    return !1;
  }
  /**
  Test whether this mark has the same type and attributes as
  another mark.
  */
  eq(e) {
    return this == e || this.type == e.type && Vo(this.attrs, e.attrs);
  }
  /**
  Convert this mark to a JSON-serializeable representation.
  */
  toJSON() {
    let e = { type: this.type.name };
    for (let n in this.attrs) {
      e.attrs = this.attrs;
      break;
    }
    return e;
  }
  /**
  Deserialize a mark from JSON.
  */
  static fromJSON(e, n) {
    if (!n)
      throw new RangeError("Invalid input for Mark.fromJSON");
    let r = e.marks[n.type];
    if (!r)
      throw new RangeError(`There is no mark type ${n.type} in this schema`);
    let i = r.create(n.attrs);
    return r.checkAttrs(i.attrs), i;
  }
  /**
  Test whether two sets of marks are identical.
  */
  static sameSet(e, n) {
    if (e == n)
      return !0;
    if (e.length != n.length)
      return !1;
    for (let r = 0; r < e.length; r++)
      if (!e[r].eq(n[r]))
        return !1;
    return !0;
  }
  /**
  Create a properly sorted mark set from null, a single mark, or an
  unsorted array of marks.
  */
  static setFrom(e) {
    if (!e || Array.isArray(e) && e.length == 0)
      return G.none;
    if (e instanceof G)
      return [e];
    let n = e.slice();
    return n.sort((r, i) => r.type.rank - i.type.rank), n;
  }
}
G.none = [];
class Wo extends Error {
}
class E {
  /**
  Create a slice. When specifying a non-zero open depth, you must
  make sure that there are nodes of at least that depth at the
  appropriate side of the fragment—i.e. if the fragment is an
  empty paragraph node, `openStart` and `openEnd` can't be greater
  than 1.
  
  It is not necessary for the content of open nodes to conform to
  the schema's content constraints, though it should be a valid
  start/end/middle for such a node, depending on which sides are
  open.
  */
  constructor(e, n, r) {
    this.content = e, this.openStart = n, this.openEnd = r;
  }
  /**
  The size this slice would add when inserted into a document.
  */
  get size() {
    return this.content.size - this.openStart - this.openEnd;
  }
  /**
  @internal
  */
  insertAt(e, n) {
    let r = kf(this.content, e + this.openStart, n);
    return r && new E(r, this.openStart, this.openEnd);
  }
  /**
  @internal
  */
  removeBetween(e, n) {
    return new E(yf(this.content, e + this.openStart, n + this.openStart), this.openStart, this.openEnd);
  }
  /**
  Tests whether this slice is equal to another slice.
  */
  eq(e) {
    return this.content.eq(e.content) && this.openStart == e.openStart && this.openEnd == e.openEnd;
  }
  /**
  @internal
  */
  toString() {
    return this.content + "(" + this.openStart + "," + this.openEnd + ")";
  }
  /**
  Convert a slice to a JSON-serializable representation.
  */
  toJSON() {
    if (!this.content.size)
      return null;
    let e = { content: this.content.toJSON() };
    return this.openStart > 0 && (e.openStart = this.openStart), this.openEnd > 0 && (e.openEnd = this.openEnd), e;
  }
  /**
  Deserialize a slice from its JSON representation.
  */
  static fromJSON(e, n) {
    if (!n)
      return E.empty;
    let r = n.openStart || 0, i = n.openEnd || 0;
    if (typeof r != "number" || typeof i != "number")
      throw new RangeError("Invalid input for Slice.fromJSON");
    return new E(T.fromJSON(e, n.content), r, i);
  }
  /**
  Create a slice from a fragment by taking the maximum possible
  open value on both side of the fragment.
  */
  static maxOpen(e, n = !0) {
    let r = 0, i = 0;
    for (let o = e.firstChild; o && !o.isLeaf && (n || !o.type.spec.isolating); o = o.firstChild)
      r++;
    for (let o = e.lastChild; o && !o.isLeaf && (n || !o.type.spec.isolating); o = o.lastChild)
      i++;
    return new E(e, r, i);
  }
}
E.empty = new E(T.empty, 0, 0);
function yf(t, e, n) {
  let { index: r, offset: i } = t.findIndex(e), o = t.maybeChild(r), { index: s, offset: l } = t.findIndex(n);
  if (i == e || o.isText) {
    if (l != n && !t.child(s).isText)
      throw new RangeError("Removing non-flat range");
    return t.cut(0, e).append(t.cut(n));
  }
  if (r != s)
    throw new RangeError("Removing non-flat range");
  return t.replaceChild(r, o.copy(yf(o.content, e - i - 1, n - i - 1)));
}
function kf(t, e, n, r) {
  let { index: i, offset: o } = t.findIndex(e), s = t.maybeChild(i);
  if (o == e || s.isText)
    return t.cut(0, e).append(n).append(t.cut(e));
  let l = kf(s.content, e - o - 1, n);
  return l && t.replaceChild(i, s.copy(l));
}
function Gg(t, e, n) {
  if (n.openStart > t.depth)
    throw new Wo("Inserted content deeper than insertion position");
  if (t.depth - n.openStart != e.depth - n.openEnd)
    throw new Wo("Inconsistent open depths");
  return bf(t, e, n, 0);
}
function bf(t, e, n, r) {
  let i = t.index(r), o = t.node(r);
  if (i == e.index(r) && r < t.depth - n.openStart) {
    let s = bf(t, e, n, r + 1);
    return o.copy(o.content.replaceChild(i, s));
  } else if (n.content.size)
    if (!n.openStart && !n.openEnd && t.depth == r && e.depth == r) {
      let s = t.parent, l = s.content;
      return Pn(s, l.cut(0, t.parentOffset).append(n.content).append(l.cut(e.parentOffset)));
    } else {
      let { start: s, end: l } = Yg(n, t);
      return Pn(o, xf(t, s, l, e, r));
    }
  else return Pn(o, Ho(t, e, r));
}
function wf(t, e) {
  if (!e.type.compatibleContent(t.type))
    throw new Wo("Cannot join " + e.type.name + " onto " + t.type.name);
}
function bl(t, e, n) {
  let r = t.node(n);
  return wf(r, e.node(n)), r;
}
function vn(t, e) {
  let n = e.length - 1;
  n >= 0 && t.isText && t.sameMarkup(e[n]) ? e[n] = t.withText(e[n].text + t.text) : e.push(t);
}
function li(t, e, n, r) {
  let i = (e || t).node(n), o = 0, s = e ? e.index(n) : i.childCount;
  t && (o = t.index(n), t.depth > n ? o++ : t.textOffset && (vn(t.nodeAfter, r), o++));
  for (let l = o; l < s; l++)
    vn(i.child(l), r);
  e && e.depth == n && e.textOffset && vn(e.nodeBefore, r);
}
function Pn(t, e) {
  return t.type.checkContent(e), t.copy(e);
}
function xf(t, e, n, r, i) {
  let o = t.depth > i && bl(t, e, i + 1), s = r.depth > i && bl(n, r, i + 1), l = [];
  return li(null, t, i, l), o && s && e.index(i) == n.index(i) ? (wf(o, s), vn(Pn(o, xf(t, e, n, r, i + 1)), l)) : (o && vn(Pn(o, Ho(t, e, i + 1)), l), li(e, n, i, l), s && vn(Pn(s, Ho(n, r, i + 1)), l)), li(r, null, i, l), new T(l);
}
function Ho(t, e, n) {
  let r = [];
  if (li(null, t, n, r), t.depth > n) {
    let i = bl(t, e, n + 1);
    vn(Pn(i, Ho(t, e, n + 1)), r);
  }
  return li(e, null, n, r), new T(r);
}
function Yg(t, e) {
  let n = e.depth - t.openStart, i = e.node(n).copy(t.content);
  for (let o = n - 1; o >= 0; o--)
    i = e.node(o).copy(T.from(i));
  return {
    start: i.resolveNoCache(t.openStart + n),
    end: i.resolveNoCache(i.content.size - t.openEnd - n)
  };
}
class yi {
  /**
  @internal
  */
  constructor(e, n, r) {
    this.pos = e, this.path = n, this.parentOffset = r, this.depth = n.length / 3 - 1;
  }
  /**
  @internal
  */
  resolveDepth(e) {
    return e == null ? this.depth : e < 0 ? this.depth + e : e;
  }
  /**
  The parent node that the position points into. Note that even if
  a position points into a text node, that node is not considered
  the parent—text nodes are ‘flat’ in this model, and have no content.
  */
  get parent() {
    return this.node(this.depth);
  }
  /**
  The root node in which the position was resolved.
  */
  get doc() {
    return this.node(0);
  }
  /**
  The ancestor node at the given level. `p.node(p.depth)` is the
  same as `p.parent`.
  */
  node(e) {
    return this.path[this.resolveDepth(e) * 3];
  }
  /**
  The index into the ancestor at the given level. If this points
  at the 3rd node in the 2nd paragraph on the top level, for
  example, `p.index(0)` is 1 and `p.index(1)` is 2.
  */
  index(e) {
    return this.path[this.resolveDepth(e) * 3 + 1];
  }
  /**
  The index pointing after this position into the ancestor at the
  given level.
  */
  indexAfter(e) {
    return e = this.resolveDepth(e), this.index(e) + (e == this.depth && !this.textOffset ? 0 : 1);
  }
  /**
  The (absolute) position at the start of the node at the given
  level.
  */
  start(e) {
    return e = this.resolveDepth(e), e == 0 ? 0 : this.path[e * 3 - 1] + 1;
  }
  /**
  The (absolute) position at the end of the node at the given
  level.
  */
  end(e) {
    return e = this.resolveDepth(e), this.start(e) + this.node(e).content.size;
  }
  /**
  The (absolute) position directly before the wrapping node at the
  given level, or, when `depth` is `this.depth + 1`, the original
  position.
  */
  before(e) {
    if (e = this.resolveDepth(e), !e)
      throw new RangeError("There is no position before the top-level node");
    return e == this.depth + 1 ? this.pos : this.path[e * 3 - 1];
  }
  /**
  The (absolute) position directly after the wrapping node at the
  given level, or the original position when `depth` is `this.depth + 1`.
  */
  after(e) {
    if (e = this.resolveDepth(e), !e)
      throw new RangeError("There is no position after the top-level node");
    return e == this.depth + 1 ? this.pos : this.path[e * 3 - 1] + this.path[e * 3].nodeSize;
  }
  /**
  When this position points into a text node, this returns the
  distance between the position and the start of the text node.
  Will be zero for positions that point between nodes.
  */
  get textOffset() {
    return this.pos - this.path[this.path.length - 1];
  }
  /**
  Get the node directly after the position, if any. If the position
  points into a text node, only the part of that node after the
  position is returned.
  */
  get nodeAfter() {
    let e = this.parent, n = this.index(this.depth);
    if (n == e.childCount)
      return null;
    let r = this.pos - this.path[this.path.length - 1], i = e.child(n);
    return r ? e.child(n).cut(r) : i;
  }
  /**
  Get the node directly before the position, if any. If the
  position points into a text node, only the part of that node
  before the position is returned.
  */
  get nodeBefore() {
    let e = this.index(this.depth), n = this.pos - this.path[this.path.length - 1];
    return n ? this.parent.child(e).cut(0, n) : e == 0 ? null : this.parent.child(e - 1);
  }
  /**
  Get the position at the given index in the parent node at the
  given depth (which defaults to `this.depth`).
  */
  posAtIndex(e, n) {
    n = this.resolveDepth(n);
    let r = this.path[n * 3], i = n == 0 ? 0 : this.path[n * 3 - 1] + 1;
    for (let o = 0; o < e; o++)
      i += r.child(o).nodeSize;
    return i;
  }
  /**
  Get the marks at this position, factoring in the surrounding
  marks' [`inclusive`](https://prosemirror.net/docs/ref/#model.MarkSpec.inclusive) property. If the
  position is at the start of a non-empty node, the marks of the
  node after it (if any) are returned.
  */
  marks() {
    let e = this.parent, n = this.index();
    if (e.content.size == 0)
      return G.none;
    if (this.textOffset)
      return e.child(n).marks;
    let r = e.maybeChild(n - 1), i = e.maybeChild(n);
    if (!r) {
      let l = r;
      r = i, i = l;
    }
    let o = r.marks;
    for (var s = 0; s < o.length; s++)
      o[s].type.spec.inclusive === !1 && (!i || !o[s].isInSet(i.marks)) && (o = o[s--].removeFromSet(o));
    return o;
  }
  /**
  Get the marks after the current position, if any, except those
  that are non-inclusive and not present at position `$end`. This
  is mostly useful for getting the set of marks to preserve after a
  deletion. Will return `null` if this position is at the end of
  its parent node or its parent node isn't a textblock (in which
  case no marks should be preserved).
  */
  marksAcross(e) {
    let n = this.parent.maybeChild(this.index());
    if (!n || !n.isInline)
      return null;
    let r = n.marks, i = e.parent.maybeChild(e.index());
    for (var o = 0; o < r.length; o++)
      r[o].type.spec.inclusive === !1 && (!i || !r[o].isInSet(i.marks)) && (r = r[o--].removeFromSet(r));
    return r;
  }
  /**
  The depth up to which this position and the given (non-resolved)
  position share the same parent nodes.
  */
  sharedDepth(e) {
    for (let n = this.depth; n > 0; n--)
      if (this.start(n) <= e && this.end(n) >= e)
        return n;
    return 0;
  }
  /**
  Returns a range based on the place where this position and the
  given position diverge around block content. If both point into
  the same textblock, for example, a range around that textblock
  will be returned. If they point into different blocks, the range
  around those blocks in their shared ancestor is returned. You can
  pass in an optional predicate that will be called with a parent
  node to see if a range into that parent is acceptable.
  */
  blockRange(e = this, n) {
    if (e.pos < this.pos)
      return e.blockRange(this);
    for (let r = this.depth - (this.parent.inlineContent || this.pos == e.pos ? 1 : 0); r >= 0; r--)
      if (e.pos <= this.end(r) && (!n || n(this.node(r))))
        return new Cf(this, e, r);
    return null;
  }
  /**
  Query whether the given position shares the same parent node.
  */
  sameParent(e) {
    return this.pos - this.parentOffset == e.pos - e.parentOffset;
  }
  /**
  Return the greater of this and the given position.
  */
  max(e) {
    return e.pos > this.pos ? e : this;
  }
  /**
  Return the smaller of this and the given position.
  */
  min(e) {
    return e.pos < this.pos ? e : this;
  }
  /**
  @internal
  */
  toString() {
    let e = "";
    for (let n = 1; n <= this.depth; n++)
      e += (e ? "/" : "") + this.node(n).type.name + "_" + this.index(n - 1);
    return e + ":" + this.parentOffset;
  }
  /**
  @internal
  */
  static resolve(e, n) {
    if (!(n >= 0 && n <= e.content.size))
      throw new RangeError("Position " + n + " out of range");
    let r = [], i = 0, o = n;
    for (let s = e; ; ) {
      let { index: l, offset: a } = s.content.findIndex(o), c = o - a;
      if (r.push(s, l, i + a), !c || (s = s.child(l), s.isText))
        break;
      o = c - 1, i += a + 1;
    }
    return new yi(n, r, o);
  }
  /**
  @internal
  */
  static resolveCached(e, n) {
    let r = ru.get(e);
    if (r)
      for (let o = 0; o < r.elts.length; o++) {
        let s = r.elts[o];
        if (s.pos == n)
          return s;
      }
    else
      ru.set(e, r = new Qg());
    let i = r.elts[r.i] = yi.resolve(e, n);
    return r.i = (r.i + 1) % Xg, i;
  }
}
class Qg {
  constructor() {
    this.elts = [], this.i = 0;
  }
}
const Xg = 12, ru = /* @__PURE__ */ new WeakMap();
class Cf {
  /**
  Construct a node range. `$from` and `$to` should point into the
  same node until at least the given `depth`, since a node range
  denotes an adjacent set of nodes in a single parent node.
  */
  constructor(e, n, r) {
    this.$from = e, this.$to = n, this.depth = r;
  }
  /**
  The position at the start of the range.
  */
  get start() {
    return this.$from.before(this.depth + 1);
  }
  /**
  The position at the end of the range.
  */
  get end() {
    return this.$to.after(this.depth + 1);
  }
  /**
  The parent node that the range points into.
  */
  get parent() {
    return this.$from.node(this.depth);
  }
  /**
  The start index of the range in the parent node.
  */
  get startIndex() {
    return this.$from.index(this.depth);
  }
  /**
  The end index of the range in the parent node.
  */
  get endIndex() {
    return this.$to.indexAfter(this.depth);
  }
}
const Zg = /* @__PURE__ */ Object.create(null);
class dt {
  /**
  @internal
  */
  constructor(e, n, r, i = G.none) {
    this.type = e, this.attrs = n, this.marks = i, this.content = r || T.empty;
  }
  /**
  The array of this node's child nodes.
  */
  get children() {
    return this.content.content;
  }
  /**
  The size of this node, as defined by the integer-based [indexing
  scheme](https://prosemirror.net/docs/guide/#doc.indexing). For text nodes, this is the
  amount of characters. For other leaf nodes, it is one. For
  non-leaf nodes, it is the size of the content plus two (the
  start and end token).
  */
  get nodeSize() {
    return this.isLeaf ? 1 : 2 + this.content.size;
  }
  /**
  The number of children that the node has.
  */
  get childCount() {
    return this.content.childCount;
  }
  /**
  Get the child node at the given index. Raises an error when the
  index is out of range.
  */
  child(e) {
    return this.content.child(e);
  }
  /**
  Get the child node at the given index, if it exists.
  */
  maybeChild(e) {
    return this.content.maybeChild(e);
  }
  /**
  Call `f` for every child node, passing the node, its offset
  into this parent node, and its index.
  */
  forEach(e) {
    this.content.forEach(e);
  }
  /**
  Invoke a callback for all descendant nodes recursively between
  the given two positions that are relative to start of this
  node's content. The callback is invoked with the node, its
  position relative to the original node (method receiver),
  its parent node, and its child index. When the callback returns
  false for a given node, that node's children will not be
  recursed over. The last parameter can be used to specify a
  starting position to count from.
  */
  nodesBetween(e, n, r, i = 0) {
    this.content.nodesBetween(e, n, r, i, this);
  }
  /**
  Call the given callback for every descendant node. Doesn't
  descend into a node when the callback returns `false`.
  */
  descendants(e) {
    this.nodesBetween(0, this.content.size, e);
  }
  /**
  Concatenates all the text nodes found in this fragment and its
  children.
  */
  get textContent() {
    return this.isLeaf && this.type.spec.leafText ? this.type.spec.leafText(this) : this.textBetween(0, this.content.size, "");
  }
  /**
  Get all text between positions `from` and `to`. When
  `blockSeparator` is given, it will be inserted to separate text
  from different block nodes. If `leafText` is given, it'll be
  inserted for every non-text leaf node encountered, otherwise
  [`leafText`](https://prosemirror.net/docs/ref/#model.NodeSpec^leafText) will be used.
  */
  textBetween(e, n, r, i) {
    return this.content.textBetween(e, n, r, i);
  }
  /**
  Returns this node's first child, or `null` if there are no
  children.
  */
  get firstChild() {
    return this.content.firstChild;
  }
  /**
  Returns this node's last child, or `null` if there are no
  children.
  */
  get lastChild() {
    return this.content.lastChild;
  }
  /**
  Test whether two nodes represent the same piece of document.
  */
  eq(e) {
    return this == e || this.sameMarkup(e) && this.content.eq(e.content);
  }
  /**
  Compare the markup (type, attributes, and marks) of this node to
  those of another. Returns `true` if both have the same markup.
  */
  sameMarkup(e) {
    return this.hasMarkup(e.type, e.attrs, e.marks);
  }
  /**
  Check whether this node's markup correspond to the given type,
  attributes, and marks.
  */
  hasMarkup(e, n, r) {
    return this.type == e && Vo(this.attrs, n || e.defaultAttrs || Zg) && G.sameSet(this.marks, r || G.none);
  }
  /**
  Create a new node with the same markup as this node, containing
  the given content (or empty, if no content is given).
  */
  copy(e = null) {
    return e == this.content ? this : new dt(this.type, this.attrs, e, this.marks);
  }
  /**
  Create a copy of this node, with the given set of marks instead
  of the node's own marks.
  */
  mark(e) {
    return e == this.marks ? this : new dt(this.type, this.attrs, this.content, e);
  }
  /**
  Create a copy of this node with only the content between the
  given positions. If `to` is not given, it defaults to the end of
  the node.
  */
  cut(e, n = this.content.size) {
    return e == 0 && n == this.content.size ? this : this.copy(this.content.cut(e, n));
  }
  /**
  Cut out the part of the document between the given positions, and
  return it as a `Slice` object.
  */
  slice(e, n = this.content.size, r = !1) {
    if (e == n)
      return E.empty;
    let i = this.resolve(e), o = this.resolve(n), s = r ? 0 : i.sharedDepth(n), l = i.start(s), c = i.node(s).content.cut(i.pos - l, o.pos - l);
    return new E(c, i.depth - s, o.depth - s);
  }
  /**
  Replace the part of the document between the given positions with
  the given slice. The slice must 'fit', meaning its open sides
  must be able to connect to the surrounding content, and its
  content nodes must be valid children for the node they are placed
  into. If any of this is violated, an error of type
  [`ReplaceError`](https://prosemirror.net/docs/ref/#model.ReplaceError) is thrown.
  */
  replace(e, n, r) {
    return Gg(this.resolve(e), this.resolve(n), r);
  }
  /**
  Find the node directly after the given position.
  */
  nodeAt(e) {
    for (let n = this; ; ) {
      let { index: r, offset: i } = n.content.findIndex(e);
      if (n = n.maybeChild(r), !n)
        return null;
      if (i == e || n.isText)
        return n;
      e -= i + 1;
    }
  }
  /**
  Find the (direct) child node after the given offset, if any,
  and return it along with its index and offset relative to this
  node.
  */
  childAfter(e) {
    let { index: n, offset: r } = this.content.findIndex(e);
    return { node: this.content.maybeChild(n), index: n, offset: r };
  }
  /**
  Find the (direct) child node before the given offset, if any,
  and return it along with its index and offset relative to this
  node.
  */
  childBefore(e) {
    if (e == 0)
      return { node: null, index: 0, offset: 0 };
    let { index: n, offset: r } = this.content.findIndex(e);
    if (r < e)
      return { node: this.content.child(n), index: n, offset: r };
    let i = this.content.child(n - 1);
    return { node: i, index: n - 1, offset: r - i.nodeSize };
  }
  /**
  Resolve the given position in the document, returning an
  [object](https://prosemirror.net/docs/ref/#model.ResolvedPos) with information about its context.
  */
  resolve(e) {
    return yi.resolveCached(this, e);
  }
  /**
  @internal
  */
  resolveNoCache(e) {
    return yi.resolve(this, e);
  }
  /**
  Test whether a given mark or mark type occurs in this document
  between the two given positions.
  */
  rangeHasMark(e, n, r) {
    let i = !1;
    return n > e && this.nodesBetween(e, n, (o) => (r.isInSet(o.marks) && (i = !0), !i)), i;
  }
  /**
  True when this is a block (non-inline node)
  */
  get isBlock() {
    return this.type.isBlock;
  }
  /**
  True when this is a textblock node, a block node with inline
  content.
  */
  get isTextblock() {
    return this.type.isTextblock;
  }
  /**
  True when this node allows inline content.
  */
  get inlineContent() {
    return this.type.inlineContent;
  }
  /**
  True when this is an inline node (a text node or a node that can
  appear among text).
  */
  get isInline() {
    return this.type.isInline;
  }
  /**
  True when this is a text node.
  */
  get isText() {
    return this.type.isText;
  }
  /**
  True when this is a leaf node.
  */
  get isLeaf() {
    return this.type.isLeaf;
  }
  /**
  True when this is an atom, i.e. when it does not have directly
  editable content. This is usually the same as `isLeaf`, but can
  be configured with the [`atom` property](https://prosemirror.net/docs/ref/#model.NodeSpec.atom)
  on a node's spec (typically used when the node is displayed as
  an uneditable [node view](https://prosemirror.net/docs/ref/#view.NodeView)).
  */
  get isAtom() {
    return this.type.isAtom;
  }
  /**
  Return a string representation of this node for debugging
  purposes.
  */
  toString() {
    if (this.type.spec.toDebugString)
      return this.type.spec.toDebugString(this);
    let e = this.type.name;
    return this.content.size && (e += "(" + this.content.toStringInner() + ")"), Sf(this.marks, e);
  }
  /**
  Get the content match in this node at the given index.
  */
  contentMatchAt(e) {
    let n = this.type.contentMatch.matchFragment(this.content, 0, e);
    if (!n)
      throw new Error("Called contentMatchAt on a node with invalid content");
    return n;
  }
  /**
  Test whether replacing the range between `from` and `to` (by
  child index) with the given replacement fragment (which defaults
  to the empty fragment) would leave the node's content valid. You
  can optionally pass `start` and `end` indices into the
  replacement fragment.
  */
  canReplace(e, n, r = T.empty, i = 0, o = r.childCount) {
    let s = this.contentMatchAt(e).matchFragment(r, i, o), l = s && s.matchFragment(this.content, n);
    if (!l || !l.validEnd)
      return !1;
    for (let a = i; a < o; a++)
      if (!this.type.allowsMarks(r.child(a).marks))
        return !1;
    return !0;
  }
  /**
  Test whether replacing the range `from` to `to` (by index) with
  a node of the given type would leave the node's content valid.
  */
  canReplaceWith(e, n, r, i) {
    if (i && !this.type.allowsMarks(i))
      return !1;
    let o = this.contentMatchAt(e).matchType(r), s = o && o.matchFragment(this.content, n);
    return s ? s.validEnd : !1;
  }
  /**
  Test whether the given node's content could be appended to this
  node. If that node is empty, this will only return true if there
  is at least one node type that can appear in both nodes (to avoid
  merging completely incompatible nodes).
  */
  canAppend(e) {
    return e.content.size ? this.canReplace(this.childCount, this.childCount, e.content) : this.type.compatibleContent(e.type);
  }
  /**
  Check whether this node and its descendants conform to the
  schema, and raise an exception when they do not.
  */
  check() {
    this.type.checkContent(this.content), this.type.checkAttrs(this.attrs);
    let e = G.none;
    for (let n = 0; n < this.marks.length; n++) {
      let r = this.marks[n];
      r.type.checkAttrs(r.attrs), e = r.addToSet(e);
    }
    if (!G.sameSet(e, this.marks))
      throw new RangeError(`Invalid collection of marks for node ${this.type.name}: ${this.marks.map((n) => n.type.name)}`);
    this.content.forEach((n) => n.check());
  }
  /**
  Return a JSON-serializeable representation of this node.
  */
  toJSON() {
    let e = { type: this.type.name };
    for (let n in this.attrs) {
      e.attrs = this.attrs;
      break;
    }
    return this.content.size && (e.content = this.content.toJSON()), this.marks.length && (e.marks = this.marks.map((n) => n.toJSON())), e;
  }
  /**
  Deserialize a node from its JSON representation.
  */
  static fromJSON(e, n) {
    if (!n)
      throw new RangeError("Invalid input for Node.fromJSON");
    let r;
    if (n.marks) {
      if (!Array.isArray(n.marks))
        throw new RangeError("Invalid mark data for Node.fromJSON");
      r = n.marks.map(e.markFromJSON);
    }
    if (n.type == "text") {
      if (typeof n.text != "string")
        throw new RangeError("Invalid text node in JSON");
      return e.text(n.text, r);
    }
    let i = T.fromJSON(e, n.content), o = e.nodeType(n.type).create(n.attrs, i, r);
    return o.type.checkAttrs(o.attrs), o;
  }
}
dt.prototype.text = void 0;
class jo extends dt {
  /**
  @internal
  */
  constructor(e, n, r, i) {
    if (super(e, n, null, i), !r)
      throw new RangeError("Empty text nodes are not allowed");
    this.text = r;
  }
  toString() {
    return this.type.spec.toDebugString ? this.type.spec.toDebugString(this) : Sf(this.marks, JSON.stringify(this.text));
  }
  get textContent() {
    return this.text;
  }
  textBetween(e, n) {
    return this.text.slice(e, n);
  }
  get nodeSize() {
    return this.text.length;
  }
  mark(e) {
    return e == this.marks ? this : new jo(this.type, this.attrs, this.text, e);
  }
  withText(e) {
    return e == this.text ? this : new jo(this.type, this.attrs, e, this.marks);
  }
  cut(e = 0, n = this.text.length) {
    return e == 0 && n == this.text.length ? this : this.withText(this.text.slice(e, n));
  }
  eq(e) {
    return this.sameMarkup(e) && this.text == e.text;
  }
  toJSON() {
    let e = super.toJSON();
    return e.text = this.text, e;
  }
}
function Sf(t, e) {
  for (let n = t.length - 1; n >= 0; n--)
    e = t[n].type.name + "(" + e + ")";
  return e;
}
class Vn {
  /**
  @internal
  */
  constructor(e) {
    this.validEnd = e, this.next = [], this.wrapCache = [];
  }
  /**
  @internal
  */
  static parse(e, n) {
    let r = new ey(e, n);
    if (r.next == null)
      return Vn.empty;
    let i = Mf(r);
    r.next && r.err("Unexpected trailing text");
    let o = ly(sy(i));
    return ay(o, r), o;
  }
  /**
  Match a node type, returning a match after that node if
  successful.
  */
  matchType(e) {
    for (let n = 0; n < this.next.length; n++)
      if (this.next[n].type == e)
        return this.next[n].next;
    return null;
  }
  /**
  Try to match a fragment. Returns the resulting match when
  successful.
  */
  matchFragment(e, n = 0, r = e.childCount) {
    let i = this;
    for (let o = n; i && o < r; o++)
      i = i.matchType(e.child(o).type);
    return i;
  }
  /**
  @internal
  */
  get inlineContent() {
    return this.next.length != 0 && this.next[0].type.isInline;
  }
  /**
  Get the first matching node type at this match position that can
  be generated.
  */
  get defaultType() {
    for (let e = 0; e < this.next.length; e++) {
      let { type: n } = this.next[e];
      if (!(n.isText || n.hasRequiredAttrs()))
        return n;
    }
    return null;
  }
  /**
  @internal
  */
  compatible(e) {
    for (let n = 0; n < this.next.length; n++)
      for (let r = 0; r < e.next.length; r++)
        if (this.next[n].type == e.next[r].type)
          return !0;
    return !1;
  }
  /**
  Try to match the given fragment, and if that fails, see if it can
  be made to match by inserting nodes in front of it. When
  successful, return a fragment of inserted nodes (which may be
  empty if nothing had to be inserted). When `toEnd` is true, only
  return a fragment if the resulting match goes to the end of the
  content expression.
  */
  fillBefore(e, n = !1, r = 0) {
    let i = [this];
    function o(s, l) {
      let a = s.matchFragment(e, r);
      if (a && (!n || a.validEnd))
        return T.from(l.map((c) => c.createAndFill()));
      for (let c = 0; c < s.next.length; c++) {
        let { type: u, next: h } = s.next[c];
        if (!(u.isText || u.hasRequiredAttrs()) && i.indexOf(h) == -1) {
          i.push(h);
          let f = o(h, l.concat(u));
          if (f)
            return f;
        }
      }
      return null;
    }
    return o(this, []);
  }
  /**
  Find a set of wrapping node types that would allow a node of the
  given type to appear at this position. The result may be empty
  (when it fits directly) and will be null when no such wrapping
  exists.
  */
  findWrapping(e) {
    for (let r = 0; r < this.wrapCache.length; r += 2)
      if (this.wrapCache[r] == e)
        return this.wrapCache[r + 1];
    let n = this.computeWrapping(e);
    return this.wrapCache.push(e, n), n;
  }
  /**
  @internal
  */
  computeWrapping(e) {
    let n = /* @__PURE__ */ Object.create(null), r = [{ match: this, type: null, via: null }];
    for (; r.length; ) {
      let i = r.shift(), o = i.match;
      if (o.matchType(e)) {
        let s = [];
        for (let l = i; l.type; l = l.via)
          s.push(l.type);
        return s.reverse();
      }
      for (let s = 0; s < o.next.length; s++) {
        let { type: l, next: a } = o.next[s];
        !l.isLeaf && !l.hasRequiredAttrs() && !(l.name in n) && (!i.type || a.validEnd) && (r.push({ match: l.contentMatch, type: l, via: i }), n[l.name] = !0);
      }
    }
    return null;
  }
  /**
  The number of outgoing edges this node has in the finite
  automaton that describes the content expression.
  */
  get edgeCount() {
    return this.next.length;
  }
  /**
  Get the _n_​th outgoing edge from this node in the finite
  automaton that describes the content expression.
  */
  edge(e) {
    if (e >= this.next.length)
      throw new RangeError(`There's no ${e}th edge in this content match`);
    return this.next[e];
  }
  /**
  @internal
  */
  toString() {
    let e = [];
    function n(r) {
      e.push(r);
      for (let i = 0; i < r.next.length; i++)
        e.indexOf(r.next[i].next) == -1 && n(r.next[i].next);
    }
    return n(this), e.map((r, i) => {
      let o = i + (r.validEnd ? "*" : " ") + " ";
      for (let s = 0; s < r.next.length; s++)
        o += (s ? ", " : "") + r.next[s].type.name + "->" + e.indexOf(r.next[s].next);
      return o;
    }).join(`
`);
  }
}
Vn.empty = new Vn(!0);
class ey {
  constructor(e, n) {
    this.string = e, this.nodeTypes = n, this.inline = null, this.pos = 0, this.tokens = e.split(/\s*(?=\b|\W|$)/), this.tokens[this.tokens.length - 1] == "" && this.tokens.pop(), this.tokens[0] == "" && this.tokens.shift();
  }
  get next() {
    return this.tokens[this.pos];
  }
  eat(e) {
    return this.next == e && (this.pos++ || !0);
  }
  err(e) {
    throw new SyntaxError(e + " (in content expression '" + this.string + "')");
  }
}
function Mf(t) {
  let e = [];
  do
    e.push(ty(t));
  while (t.eat("|"));
  return e.length == 1 ? e[0] : { type: "choice", exprs: e };
}
function ty(t) {
  let e = [];
  do
    e.push(ny(t));
  while (t.next && t.next != ")" && t.next != "|");
  return e.length == 1 ? e[0] : { type: "seq", exprs: e };
}
function ny(t) {
  let e = oy(t);
  for (; ; )
    if (t.eat("+"))
      e = { type: "plus", expr: e };
    else if (t.eat("*"))
      e = { type: "star", expr: e };
    else if (t.eat("?"))
      e = { type: "opt", expr: e };
    else if (t.eat("{"))
      e = ry(t, e);
    else
      break;
  return e;
}
function iu(t) {
  /\D/.test(t.next) && t.err("Expected number, got '" + t.next + "'");
  let e = Number(t.next);
  return t.pos++, e;
}
function ry(t, e) {
  let n = iu(t), r = n;
  return t.eat(",") && (t.next != "}" ? r = iu(t) : r = -1), t.eat("}") || t.err("Unclosed braced range"), { type: "range", min: n, max: r, expr: e };
}
function iy(t, e) {
  let n = t.nodeTypes, r = n[e];
  if (r)
    return [r];
  let i = [];
  for (let o in n) {
    let s = n[o];
    s.isInGroup(e) && i.push(s);
  }
  return i.length == 0 && t.err("No node type or group '" + e + "' found"), i;
}
function oy(t) {
  if (t.eat("(")) {
    let e = Mf(t);
    return t.eat(")") || t.err("Missing closing paren"), e;
  } else if (/\W/.test(t.next))
    t.err("Unexpected token '" + t.next + "'");
  else {
    let e = iy(t, t.next).map((n) => (t.inline == null ? t.inline = n.isInline : t.inline != n.isInline && t.err("Mixing inline and block content"), { type: "name", value: n }));
    return t.pos++, e.length == 1 ? e[0] : { type: "choice", exprs: e };
  }
}
function sy(t) {
  let e = [[]];
  return i(o(t, 0), n()), e;
  function n() {
    return e.push([]) - 1;
  }
  function r(s, l, a) {
    let c = { term: a, to: l };
    return e[s].push(c), c;
  }
  function i(s, l) {
    s.forEach((a) => a.to = l);
  }
  function o(s, l) {
    if (s.type == "choice")
      return s.exprs.reduce((a, c) => a.concat(o(c, l)), []);
    if (s.type == "seq")
      for (let a = 0; ; a++) {
        let c = o(s.exprs[a], l);
        if (a == s.exprs.length - 1)
          return c;
        i(c, l = n());
      }
    else if (s.type == "star") {
      let a = n();
      return r(l, a), i(o(s.expr, a), a), [r(a)];
    } else if (s.type == "plus") {
      let a = n();
      return i(o(s.expr, l), a), i(o(s.expr, a), a), [r(a)];
    } else {
      if (s.type == "opt")
        return [r(l)].concat(o(s.expr, l));
      if (s.type == "range") {
        let a = l;
        for (let c = 0; c < s.min; c++) {
          let u = n();
          i(o(s.expr, a), u), a = u;
        }
        if (s.max == -1)
          i(o(s.expr, a), a);
        else
          for (let c = s.min; c < s.max; c++) {
            let u = n();
            r(a, u), i(o(s.expr, a), u), a = u;
          }
        return [r(a)];
      } else {
        if (s.type == "name")
          return [r(l, void 0, s.value)];
        throw new Error("Unknown expr type");
      }
    }
  }
}
function Nf(t, e) {
  return e - t;
}
function ou(t, e) {
  let n = [];
  return r(e), n.sort(Nf);
  function r(i) {
    let o = t[i];
    if (o.length == 1 && !o[0].term)
      return r(o[0].to);
    n.push(i);
    for (let s = 0; s < o.length; s++) {
      let { term: l, to: a } = o[s];
      !l && n.indexOf(a) == -1 && r(a);
    }
  }
}
function ly(t) {
  let e = /* @__PURE__ */ Object.create(null);
  return n(ou(t, 0));
  function n(r) {
    let i = [];
    r.forEach((s) => {
      t[s].forEach(({ term: l, to: a }) => {
        if (!l)
          return;
        let c;
        for (let u = 0; u < i.length; u++)
          i[u][0] == l && (c = i[u][1]);
        ou(t, a).forEach((u) => {
          c || i.push([l, c = []]), c.indexOf(u) == -1 && c.push(u);
        });
      });
    });
    let o = e[r.join(",")] = new Vn(r.indexOf(t.length - 1) > -1);
    for (let s = 0; s < i.length; s++) {
      let l = i[s][1].sort(Nf);
      o.next.push({ type: i[s][0], next: e[l.join(",")] || n(l) });
    }
    return o;
  }
}
function ay(t, e) {
  for (let n = 0, r = [t]; n < r.length; n++) {
    let i = r[n], o = !i.validEnd, s = [];
    for (let l = 0; l < i.next.length; l++) {
      let { type: a, next: c } = i.next[l];
      s.push(a.name), o && !(a.isText || a.hasRequiredAttrs()) && (o = !1), r.indexOf(c) == -1 && r.push(c);
    }
    o && e.err("Only non-generatable nodes (" + s.join(", ") + ") in a required position (see https://prosemirror.net/docs/guide/#generatable)");
  }
}
function Tf(t) {
  let e = /* @__PURE__ */ Object.create(null);
  for (let n in t) {
    let r = t[n];
    if (!r.hasDefault)
      return null;
    e[n] = r.default;
  }
  return e;
}
function If(t, e) {
  let n = /* @__PURE__ */ Object.create(null);
  for (let r in t) {
    let i = e && e[r];
    if (i === void 0) {
      let o = t[r];
      if (o.hasDefault)
        i = o.default;
      else
        throw new RangeError("No value supplied for attribute " + r);
    }
    n[r] = i;
  }
  return n;
}
function Af(t, e, n, r) {
  for (let i in e)
    if (!(i in t))
      throw new RangeError(`Unsupported attribute ${i} for ${n} of type ${i}`);
  for (let i in t) {
    let o = t[i];
    o.validate && o.validate(e[i]);
  }
}
function Ef(t, e) {
  let n = /* @__PURE__ */ Object.create(null);
  if (e)
    for (let r in e)
      n[r] = new uy(t, r, e[r]);
  return n;
}
let qo = class Of {
  /**
  @internal
  */
  constructor(e, n, r) {
    this.name = e, this.schema = n, this.spec = r, this.markSet = null, this.groups = r.group ? r.group.split(" ") : [], this.attrs = Ef(e, r.attrs), this.defaultAttrs = Tf(this.attrs), this.contentMatch = null, this.inlineContent = null, this.isBlock = !(r.inline || e == "text"), this.isText = e == "text";
  }
  /**
  True if this is an inline type.
  */
  get isInline() {
    return !this.isBlock;
  }
  /**
  True if this is a textblock type, a block that contains inline
  content.
  */
  get isTextblock() {
    return this.isBlock && this.inlineContent;
  }
  /**
  True for node types that allow no content.
  */
  get isLeaf() {
    return this.contentMatch == Vn.empty;
  }
  /**
  True when this node is an atom, i.e. when it does not have
  directly editable content.
  */
  get isAtom() {
    return this.isLeaf || !!this.spec.atom;
  }
  /**
  Return true when this node type is part of the given
  [group](https://prosemirror.net/docs/ref/#model.NodeSpec.group).
  */
  isInGroup(e) {
    return this.groups.indexOf(e) > -1;
  }
  /**
  The node type's [whitespace](https://prosemirror.net/docs/ref/#model.NodeSpec.whitespace) option.
  */
  get whitespace() {
    return this.spec.whitespace || (this.spec.code ? "pre" : "normal");
  }
  /**
  Tells you whether this node type has any required attributes.
  */
  hasRequiredAttrs() {
    for (let e in this.attrs)
      if (this.attrs[e].isRequired)
        return !0;
    return !1;
  }
  /**
  Indicates whether this node allows some of the same content as
  the given node type.
  */
  compatibleContent(e) {
    return this == e || this.contentMatch.compatible(e.contentMatch);
  }
  /**
  @internal
  */
  computeAttrs(e) {
    return !e && this.defaultAttrs ? this.defaultAttrs : If(this.attrs, e);
  }
  /**
  Create a `Node` of this type. The given attributes are
  checked and defaulted (you can pass `null` to use the type's
  defaults entirely, if no required attributes exist). `content`
  may be a `Fragment`, a node, an array of nodes, or
  `null`. Similarly `marks` may be `null` to default to the empty
  set of marks.
  */
  create(e = null, n, r) {
    if (this.isText)
      throw new Error("NodeType.create can't construct text nodes");
    return new dt(this, this.computeAttrs(e), T.from(n), G.setFrom(r));
  }
  /**
  Like [`create`](https://prosemirror.net/docs/ref/#model.NodeType.create), but check the given content
  against the node type's content restrictions, and throw an error
  if it doesn't match.
  */
  createChecked(e = null, n, r) {
    return n = T.from(n), this.checkContent(n), new dt(this, this.computeAttrs(e), n, G.setFrom(r));
  }
  /**
  Like [`create`](https://prosemirror.net/docs/ref/#model.NodeType.create), but see if it is
  necessary to add nodes to the start or end of the given fragment
  to make it fit the node. If no fitting wrapping can be found,
  return null. Note that, due to the fact that required nodes can
  always be created, this will always succeed if you pass null or
  `Fragment.empty` as content.
  */
  createAndFill(e = null, n, r) {
    if (e = this.computeAttrs(e), n = T.from(n), n.size) {
      let s = this.contentMatch.fillBefore(n);
      if (!s)
        return null;
      n = s.append(n);
    }
    let i = this.contentMatch.matchFragment(n), o = i && i.fillBefore(T.empty, !0);
    return o ? new dt(this, e, n.append(o), G.setFrom(r)) : null;
  }
  /**
  Returns true if the given fragment is valid content for this node
  type.
  */
  validContent(e) {
    let n = this.contentMatch.matchFragment(e);
    if (!n || !n.validEnd)
      return !1;
    for (let r = 0; r < e.childCount; r++)
      if (!this.allowsMarks(e.child(r).marks))
        return !1;
    return !0;
  }
  /**
  Throws a RangeError if the given fragment is not valid content for this
  node type.
  @internal
  */
  checkContent(e) {
    if (!this.validContent(e))
      throw new RangeError(`Invalid content for node ${this.name}: ${e.toString().slice(0, 50)}`);
  }
  /**
  @internal
  */
  checkAttrs(e) {
    Af(this.attrs, e, "node", this.name);
  }
  /**
  Check whether the given mark type is allowed in this node.
  */
  allowsMarkType(e) {
    return this.markSet == null || this.markSet.indexOf(e) > -1;
  }
  /**
  Test whether the given set of marks are allowed in this node.
  */
  allowsMarks(e) {
    if (this.markSet == null)
      return !0;
    for (let n = 0; n < e.length; n++)
      if (!this.allowsMarkType(e[n].type))
        return !1;
    return !0;
  }
  /**
  Removes the marks that are not allowed in this node from the given set.
  */
  allowedMarks(e) {
    if (this.markSet == null)
      return e;
    let n;
    for (let r = 0; r < e.length; r++)
      this.allowsMarkType(e[r].type) ? n && n.push(e[r]) : n || (n = e.slice(0, r));
    return n ? n.length ? n : G.none : e;
  }
  /**
  @internal
  */
  static compile(e, n) {
    let r = /* @__PURE__ */ Object.create(null);
    e.forEach((o, s) => r[o] = new Of(o, n, s));
    let i = n.spec.topNode || "doc";
    if (!r[i])
      throw new RangeError("Schema is missing its top node type ('" + i + "')");
    if (!r.text)
      throw new RangeError("Every schema needs a 'text' type");
    for (let o in r.text.attrs)
      throw new RangeError("The text node type should not have attributes");
    return r;
  }
};
function cy(t, e, n) {
  let r = n.split("|");
  return (i) => {
    let o = i === null ? "null" : typeof i;
    if (r.indexOf(o) < 0)
      throw new RangeError(`Expected value of type ${r} for attribute ${e} on type ${t}, got ${o}`);
  };
}
class uy {
  constructor(e, n, r) {
    this.hasDefault = Object.prototype.hasOwnProperty.call(r, "default"), this.default = r.default, this.validate = typeof r.validate == "string" ? cy(e, n, r.validate) : r.validate;
  }
  get isRequired() {
    return !this.hasDefault;
  }
}
class is {
  /**
  @internal
  */
  constructor(e, n, r, i) {
    this.name = e, this.rank = n, this.schema = r, this.spec = i, this.attrs = Ef(e, i.attrs), this.excluded = null;
    let o = Tf(this.attrs);
    this.instance = o ? new G(this, o) : null;
  }
  /**
  Create a mark of this type. `attrs` may be `null` or an object
  containing only some of the mark's attributes. The others, if
  they have defaults, will be added.
  */
  create(e = null) {
    return !e && this.instance ? this.instance : new G(this, If(this.attrs, e));
  }
  /**
  @internal
  */
  static compile(e, n) {
    let r = /* @__PURE__ */ Object.create(null), i = 0;
    return e.forEach((o, s) => r[o] = new is(o, i++, n, s)), r;
  }
  /**
  When there is a mark of this type in the given set, a new set
  without it is returned. Otherwise, the input set is returned.
  */
  removeFromSet(e) {
    for (var n = 0; n < e.length; n++)
      e[n].type == this && (e = e.slice(0, n).concat(e.slice(n + 1)), n--);
    return e;
  }
  /**
  Tests whether there is a mark of this type in the given set.
  */
  isInSet(e) {
    for (let n = 0; n < e.length; n++)
      if (e[n].type == this)
        return e[n];
  }
  /**
  @internal
  */
  checkAttrs(e) {
    Af(this.attrs, e, "mark", this.name);
  }
  /**
  Queries whether a given mark type is
  [excluded](https://prosemirror.net/docs/ref/#model.MarkSpec.excludes) by this one.
  */
  excludes(e) {
    return this.excluded.indexOf(e) > -1;
  }
}
class hy {
  /**
  Construct a schema from a schema [specification](https://prosemirror.net/docs/ref/#model.SchemaSpec).
  */
  constructor(e) {
    this.linebreakReplacement = null, this.cached = /* @__PURE__ */ Object.create(null);
    let n = this.spec = {};
    for (let i in e)
      n[i] = e[i];
    n.nodes = we.from(e.nodes), n.marks = we.from(e.marks || {}), this.nodes = qo.compile(this.spec.nodes, this), this.marks = is.compile(this.spec.marks, this);
    let r = /* @__PURE__ */ Object.create(null);
    for (let i in this.nodes) {
      if (i in this.marks)
        throw new RangeError(i + " can not be both a node and a mark");
      let o = this.nodes[i], s = o.spec.content || "", l = o.spec.marks;
      if (o.contentMatch = r[s] || (r[s] = Vn.parse(s, this.nodes)), o.inlineContent = o.contentMatch.inlineContent, o.spec.linebreakReplacement) {
        if (this.linebreakReplacement)
          throw new RangeError("Multiple linebreak nodes defined");
        if (!o.isInline || !o.isLeaf)
          throw new RangeError("Linebreak replacement nodes must be inline leaf nodes");
        this.linebreakReplacement = o;
      }
      o.markSet = l == "_" ? null : l ? su(this, l.split(" ")) : l == "" || !o.inlineContent ? [] : null;
    }
    for (let i in this.marks) {
      let o = this.marks[i], s = o.spec.excludes;
      o.excluded = s == null ? [o] : s == "" ? [] : su(this, s.split(" "));
    }
    this.nodeFromJSON = this.nodeFromJSON.bind(this), this.markFromJSON = this.markFromJSON.bind(this), this.topNodeType = this.nodes[this.spec.topNode || "doc"], this.cached.wrappings = /* @__PURE__ */ Object.create(null);
  }
  /**
  Create a node in this schema. The `type` may be a string or a
  `NodeType` instance. Attributes will be extended with defaults,
  `content` may be a `Fragment`, `null`, a `Node`, or an array of
  nodes.
  */
  node(e, n = null, r, i) {
    if (typeof e == "string")
      e = this.nodeType(e);
    else if (e instanceof qo) {
      if (e.schema != this)
        throw new RangeError("Node type from different schema used (" + e.name + ")");
    } else throw new RangeError("Invalid node type: " + e);
    return e.createChecked(n, r, i);
  }
  /**
  Create a text node in the schema. Empty text nodes are not
  allowed.
  */
  text(e, n) {
    let r = this.nodes.text;
    return new jo(r, r.defaultAttrs, e, G.setFrom(n));
  }
  /**
  Create a mark with the given type and attributes.
  */
  mark(e, n) {
    return typeof e == "string" && (e = this.marks[e]), e.create(n);
  }
  /**
  Deserialize a node from its JSON representation. This method is
  bound.
  */
  nodeFromJSON(e) {
    return dt.fromJSON(this, e);
  }
  /**
  Deserialize a mark from its JSON representation. This method is
  bound.
  */
  markFromJSON(e) {
    return G.fromJSON(this, e);
  }
  /**
  @internal
  */
  nodeType(e) {
    let n = this.nodes[e];
    if (!n)
      throw new RangeError("Unknown node type: " + e);
    return n;
  }
}
function su(t, e) {
  let n = [];
  for (let r = 0; r < e.length; r++) {
    let i = e[r], o = t.marks[i], s = o;
    if (o)
      n.push(o);
    else
      for (let l in t.marks) {
        let a = t.marks[l];
        (i == "_" || a.spec.group && a.spec.group.split(" ").indexOf(i) > -1) && n.push(s = a);
      }
    if (!s)
      throw new SyntaxError("Unknown mark type: '" + e[r] + "'");
  }
  return n;
}
function fy(t) {
  return t.tag != null;
}
function dy(t) {
  return t.style != null;
}
class Wn {
  /**
  Create a parser that targets the given schema, using the given
  parsing rules.
  */
  constructor(e, n) {
    this.schema = e, this.rules = n, this.tags = [], this.styles = [];
    let r = this.matchedStyles = [];
    n.forEach((i) => {
      if (fy(i))
        this.tags.push(i);
      else if (dy(i)) {
        let o = /[^=]*/.exec(i.style)[0];
        r.indexOf(o) < 0 && r.push(o), this.styles.push(i);
      }
    }), this.normalizeLists = !this.tags.some((i) => {
      if (!/^(ul|ol)\b/.test(i.tag) || !i.node)
        return !1;
      let o = e.nodes[i.node];
      return o.contentMatch.matchType(o);
    });
  }
  /**
  Parse a document from the content of a DOM node.
  */
  parse(e, n = {}) {
    let r = new au(this, n, !1);
    return r.addAll(e, G.none, n.from, n.to), r.finish();
  }
  /**
  Parses the content of the given DOM node, like
  [`parse`](https://prosemirror.net/docs/ref/#model.DOMParser.parse), and takes the same set of
  options. But unlike that method, which produces a whole node,
  this one returns a slice that is open at the sides, meaning that
  the schema constraints aren't applied to the start of nodes to
  the left of the input and the end of nodes at the end.
  */
  parseSlice(e, n = {}) {
    let r = new au(this, n, !0);
    return r.addAll(e, G.none, n.from, n.to), E.maxOpen(r.finish());
  }
  /**
  @internal
  */
  matchTag(e, n, r) {
    for (let i = r ? this.tags.indexOf(r) + 1 : 0; i < this.tags.length; i++) {
      let o = this.tags[i];
      if (gy(e, o.tag) && (o.namespace === void 0 || e.namespaceURI == o.namespace) && (!o.context || n.matchesContext(o.context))) {
        if (o.getAttrs) {
          let s = o.getAttrs(e);
          if (s === !1)
            continue;
          o.attrs = s || void 0;
        }
        return o;
      }
    }
  }
  /**
  @internal
  */
  matchStyle(e, n, r, i) {
    for (let o = i ? this.styles.indexOf(i) + 1 : 0; o < this.styles.length; o++) {
      let s = this.styles[o], l = s.style;
      if (!(l.indexOf(e) != 0 || s.context && !r.matchesContext(s.context) || // Test that the style string either precisely matches the prop,
      // or has an '=' sign after the prop, followed by the given
      // value.
      l.length > e.length && (l.charCodeAt(e.length) != 61 || l.slice(e.length + 1) != n))) {
        if (s.getAttrs) {
          let a = s.getAttrs(n);
          if (a === !1)
            continue;
          s.attrs = a || void 0;
        }
        return s;
      }
    }
  }
  /**
  @internal
  */
  static schemaRules(e) {
    let n = [];
    function r(i) {
      let o = i.priority == null ? 50 : i.priority, s = 0;
      for (; s < n.length; s++) {
        let l = n[s];
        if ((l.priority == null ? 50 : l.priority) < o)
          break;
      }
      n.splice(s, 0, i);
    }
    for (let i in e.marks) {
      let o = e.marks[i].spec.parseDOM;
      o && o.forEach((s) => {
        r(s = cu(s)), s.mark || s.ignore || s.clearMark || (s.mark = i);
      });
    }
    for (let i in e.nodes) {
      let o = e.nodes[i].spec.parseDOM;
      o && o.forEach((s) => {
        r(s = cu(s)), s.node || s.ignore || s.mark || (s.node = i);
      });
    }
    return n;
  }
  /**
  Construct a DOM parser using the parsing rules listed in a
  schema's [node specs](https://prosemirror.net/docs/ref/#model.NodeSpec.parseDOM), reordered by
  [priority](https://prosemirror.net/docs/ref/#model.ParseRule.priority).
  */
  static fromSchema(e) {
    return e.cached.domParser || (e.cached.domParser = new Wn(e, Wn.schemaRules(e)));
  }
}
const Df = {
  address: !0,
  article: !0,
  aside: !0,
  blockquote: !0,
  canvas: !0,
  dd: !0,
  div: !0,
  dl: !0,
  fieldset: !0,
  figcaption: !0,
  figure: !0,
  footer: !0,
  form: !0,
  h1: !0,
  h2: !0,
  h3: !0,
  h4: !0,
  h5: !0,
  h6: !0,
  header: !0,
  hgroup: !0,
  hr: !0,
  li: !0,
  noscript: !0,
  ol: !0,
  output: !0,
  p: !0,
  pre: !0,
  section: !0,
  table: !0,
  tfoot: !0,
  ul: !0
}, py = {
  head: !0,
  noscript: !0,
  object: !0,
  script: !0,
  style: !0,
  title: !0
}, Rf = { ol: !0, ul: !0 }, ki = 1, wl = 2, ho = 4;
function lu(t, e, n) {
  return e != null ? (e ? ki : 0) | (e === "full" ? wl : 0) : t && t.whitespace == "pre" ? ki | wl : n & -5;
}
class Yi {
  constructor(e, n, r, i, o, s) {
    this.type = e, this.attrs = n, this.marks = r, this.solid = i, this.options = s, this.content = [], this.activeMarks = G.none, this.match = o || (s & ho ? null : e.contentMatch);
  }
  findWrapping(e) {
    if (!this.match) {
      if (!this.type)
        return [];
      let n = this.type.contentMatch.fillBefore(T.from(e));
      if (n)
        this.match = this.type.contentMatch.matchFragment(n);
      else {
        let r = this.type.contentMatch, i;
        return (i = r.findWrapping(e.type)) ? (this.match = r, i) : null;
      }
    }
    return this.match.findWrapping(e.type);
  }
  finish(e) {
    if (!(this.options & ki)) {
      let r = this.content[this.content.length - 1], i;
      if (r && r.isText && (i = /[ \t\r\n\u000c]+$/.exec(r.text))) {
        let o = r;
        r.text.length == i[0].length ? this.content.pop() : this.content[this.content.length - 1] = o.withText(o.text.slice(0, o.text.length - i[0].length));
      }
    }
    let n = T.from(this.content);
    return !e && this.match && (n = n.append(this.match.fillBefore(T.empty, !0))), this.type ? this.type.create(this.attrs, n, this.marks) : n;
  }
  inlineContext(e) {
    return this.type ? this.type.inlineContent : this.content.length ? this.content[0].isInline : e.parentNode && !Df.hasOwnProperty(e.parentNode.nodeName.toLowerCase());
  }
}
class au {
  constructor(e, n, r) {
    this.parser = e, this.options = n, this.isOpen = r, this.open = 0, this.localPreserveWS = !1;
    let i = n.topNode, o, s = lu(null, n.preserveWhitespace, 0) | (r ? ho : 0);
    i ? o = new Yi(i.type, i.attrs, G.none, !0, n.topMatch || i.type.contentMatch, s) : r ? o = new Yi(null, null, G.none, !0, null, s) : o = new Yi(e.schema.topNodeType, null, G.none, !0, null, s), this.nodes = [o], this.find = n.findPositions, this.needsBlock = !1;
  }
  get top() {
    return this.nodes[this.open];
  }
  // Add a DOM node to the content. Text is inserted as text node,
  // otherwise, the node is passed to `addElement` or, if it has a
  // `style` attribute, `addElementWithStyles`.
  addDOM(e, n) {
    e.nodeType == 3 ? this.addTextNode(e, n) : e.nodeType == 1 && this.addElement(e, n);
  }
  addTextNode(e, n) {
    let r = e.nodeValue, i = this.top, o = i.options & wl ? "full" : this.localPreserveWS || (i.options & ki) > 0;
    if (o === "full" || i.inlineContext(e) || /[^ \t\r\n\u000c]/.test(r)) {
      if (o)
        o !== "full" ? r = r.replace(/\r?\n|\r/g, " ") : r = r.replace(/\r\n?/g, `
`);
      else if (r = r.replace(/[ \t\r\n\u000c]+/g, " "), /^[ \t\r\n\u000c]/.test(r) && this.open == this.nodes.length - 1) {
        let s = i.content[i.content.length - 1], l = e.previousSibling;
        (!s || l && l.nodeName == "BR" || s.isText && /[ \t\r\n\u000c]$/.test(s.text)) && (r = r.slice(1));
      }
      r && this.insertNode(this.parser.schema.text(r), n), this.findInText(e);
    } else
      this.findInside(e);
  }
  // Try to find a handler for the given tag and use that to parse. If
  // none is found, the element's content nodes are added directly.
  addElement(e, n, r) {
    let i = this.localPreserveWS, o = this.top;
    (e.tagName == "PRE" || /pre/.test(e.style && e.style.whiteSpace)) && (this.localPreserveWS = !0);
    let s = e.nodeName.toLowerCase(), l;
    Rf.hasOwnProperty(s) && this.parser.normalizeLists && my(e);
    let a = this.options.ruleFromNode && this.options.ruleFromNode(e) || (l = this.parser.matchTag(e, this, r));
    e: if (a ? a.ignore : py.hasOwnProperty(s))
      this.findInside(e), this.ignoreFallback(e, n);
    else if (!a || a.skip || a.closeParent) {
      a && a.closeParent ? this.open = Math.max(0, this.open - 1) : a && a.skip.nodeType && (e = a.skip);
      let c, u = this.needsBlock;
      if (Df.hasOwnProperty(s))
        o.content.length && o.content[0].isInline && this.open && (this.open--, o = this.top), c = !0, o.type || (this.needsBlock = !0);
      else if (!e.firstChild) {
        this.leafFallback(e, n);
        break e;
      }
      let h = a && a.skip ? n : this.readStyles(e, n);
      h && this.addAll(e, h), c && this.sync(o), this.needsBlock = u;
    } else {
      let c = this.readStyles(e, n);
      c && this.addElementByRule(e, a, c, a.consuming === !1 ? l : void 0);
    }
    this.localPreserveWS = i;
  }
  // Called for leaf DOM nodes that would otherwise be ignored
  leafFallback(e, n) {
    e.nodeName == "BR" && this.top.type && this.top.type.inlineContent && this.addTextNode(e.ownerDocument.createTextNode(`
`), n);
  }
  // Called for ignored nodes
  ignoreFallback(e, n) {
    e.nodeName == "BR" && (!this.top.type || !this.top.type.inlineContent) && this.findPlace(this.parser.schema.text("-"), n);
  }
  // Run any style parser associated with the node's styles. Either
  // return an updated array of marks, or null to indicate some of the
  // styles had a rule with `ignore` set.
  readStyles(e, n) {
    let r = e.style;
    if (r && r.length)
      for (let i = 0; i < this.parser.matchedStyles.length; i++) {
        let o = this.parser.matchedStyles[i], s = r.getPropertyValue(o);
        if (s)
          for (let l = void 0; ; ) {
            let a = this.parser.matchStyle(o, s, this, l);
            if (!a)
              break;
            if (a.ignore)
              return null;
            if (a.clearMark ? n = n.filter((c) => !a.clearMark(c)) : n = n.concat(this.parser.schema.marks[a.mark].create(a.attrs)), a.consuming === !1)
              l = a;
            else
              break;
          }
      }
    return n;
  }
  // Look up a handler for the given node. If none are found, return
  // false. Otherwise, apply it, use its return value to drive the way
  // the node's content is wrapped, and return true.
  addElementByRule(e, n, r, i) {
    let o, s;
    if (n.node)
      if (s = this.parser.schema.nodes[n.node], s.isLeaf)
        this.insertNode(s.create(n.attrs), r) || this.leafFallback(e, r);
      else {
        let a = this.enter(s, n.attrs || null, r, n.preserveWhitespace);
        a && (o = !0, r = a);
      }
    else {
      let a = this.parser.schema.marks[n.mark];
      r = r.concat(a.create(n.attrs));
    }
    let l = this.top;
    if (s && s.isLeaf)
      this.findInside(e);
    else if (i)
      this.addElement(e, r, i);
    else if (n.getContent)
      this.findInside(e), n.getContent(e, this.parser.schema).forEach((a) => this.insertNode(a, r));
    else {
      let a = e;
      typeof n.contentElement == "string" ? a = e.querySelector(n.contentElement) : typeof n.contentElement == "function" ? a = n.contentElement(e) : n.contentElement && (a = n.contentElement), this.findAround(e, a, !0), this.addAll(a, r), this.findAround(e, a, !1);
    }
    o && this.sync(l) && this.open--;
  }
  // Add all child nodes between `startIndex` and `endIndex` (or the
  // whole node, if not given). If `sync` is passed, use it to
  // synchronize after every block element.
  addAll(e, n, r, i) {
    let o = r || 0;
    for (let s = r ? e.childNodes[r] : e.firstChild, l = i == null ? null : e.childNodes[i]; s != l; s = s.nextSibling, ++o)
      this.findAtPoint(e, o), this.addDOM(s, n);
    this.findAtPoint(e, o);
  }
  // Try to find a way to fit the given node type into the current
  // context. May add intermediate wrappers and/or leave non-solid
  // nodes that we're in.
  findPlace(e, n) {
    let r, i;
    for (let o = this.open; o >= 0; o--) {
      let s = this.nodes[o], l = s.findWrapping(e);
      if (l && (!r || r.length > l.length) && (r = l, i = s, !l.length) || s.solid)
        break;
    }
    if (!r)
      return null;
    this.sync(i);
    for (let o = 0; o < r.length; o++)
      n = this.enterInner(r[o], null, n, !1);
    return n;
  }
  // Try to insert the given node, adjusting the context when needed.
  insertNode(e, n) {
    if (e.isInline && this.needsBlock && !this.top.type) {
      let i = this.textblockFromContext();
      i && (n = this.enterInner(i, null, n));
    }
    let r = this.findPlace(e, n);
    if (r) {
      this.closeExtra();
      let i = this.top;
      i.match && (i.match = i.match.matchType(e.type));
      let o = G.none;
      for (let s of r.concat(e.marks))
        (i.type ? i.type.allowsMarkType(s.type) : uu(s.type, e.type)) && (o = s.addToSet(o));
      return i.content.push(e.mark(o)), !0;
    }
    return !1;
  }
  // Try to start a node of the given type, adjusting the context when
  // necessary.
  enter(e, n, r, i) {
    let o = this.findPlace(e.create(n), r);
    return o && (o = this.enterInner(e, n, r, !0, i)), o;
  }
  // Open a node of the given type
  enterInner(e, n, r, i = !1, o) {
    this.closeExtra();
    let s = this.top;
    s.match = s.match && s.match.matchType(e);
    let l = lu(e, o, s.options);
    s.options & ho && s.content.length == 0 && (l |= ho);
    let a = G.none;
    return r = r.filter((c) => (s.type ? s.type.allowsMarkType(c.type) : uu(c.type, e)) ? (a = c.addToSet(a), !1) : !0), this.nodes.push(new Yi(e, n, a, i, null, l)), this.open++, r;
  }
  // Make sure all nodes above this.open are finished and added to
  // their parents
  closeExtra(e = !1) {
    let n = this.nodes.length - 1;
    if (n > this.open) {
      for (; n > this.open; n--)
        this.nodes[n - 1].content.push(this.nodes[n].finish(e));
      this.nodes.length = this.open + 1;
    }
  }
  finish() {
    return this.open = 0, this.closeExtra(this.isOpen), this.nodes[0].finish(!!(this.isOpen || this.options.topOpen));
  }
  sync(e) {
    for (let n = this.open; n >= 0; n--) {
      if (this.nodes[n] == e)
        return this.open = n, !0;
      this.localPreserveWS && (this.nodes[n].options |= ki);
    }
    return !1;
  }
  get currentPos() {
    this.closeExtra();
    let e = 0;
    for (let n = this.open; n >= 0; n--) {
      let r = this.nodes[n].content;
      for (let i = r.length - 1; i >= 0; i--)
        e += r[i].nodeSize;
      n && e++;
    }
    return e;
  }
  findAtPoint(e, n) {
    if (this.find)
      for (let r = 0; r < this.find.length; r++)
        this.find[r].node == e && this.find[r].offset == n && (this.find[r].pos = this.currentPos);
  }
  findInside(e) {
    if (this.find)
      for (let n = 0; n < this.find.length; n++)
        this.find[n].pos == null && e.nodeType == 1 && e.contains(this.find[n].node) && (this.find[n].pos = this.currentPos);
  }
  findAround(e, n, r) {
    if (e != n && this.find)
      for (let i = 0; i < this.find.length; i++)
        this.find[i].pos == null && e.nodeType == 1 && e.contains(this.find[i].node) && n.compareDocumentPosition(this.find[i].node) & (r ? 2 : 4) && (this.find[i].pos = this.currentPos);
  }
  findInText(e) {
    if (this.find)
      for (let n = 0; n < this.find.length; n++)
        this.find[n].node == e && (this.find[n].pos = this.currentPos - (e.nodeValue.length - this.find[n].offset));
  }
  // Determines whether the given context string matches this context.
  matchesContext(e) {
    if (e.indexOf("|") > -1)
      return e.split(/\s*\|\s*/).some(this.matchesContext, this);
    let n = e.split("/"), r = this.options.context, i = !this.isOpen && (!r || r.parent.type == this.nodes[0].type), o = -(r ? r.depth + 1 : 0) + (i ? 0 : 1), s = (l, a) => {
      for (; l >= 0; l--) {
        let c = n[l];
        if (c == "") {
          if (l == n.length - 1 || l == 0)
            continue;
          for (; a >= o; a--)
            if (s(l - 1, a))
              return !0;
          return !1;
        } else {
          let u = a > 0 || a == 0 && i ? this.nodes[a].type : r && a >= o ? r.node(a - o).type : null;
          if (!u || u.name != c && !u.isInGroup(c))
            return !1;
          a--;
        }
      }
      return !0;
    };
    return s(n.length - 1, this.open);
  }
  textblockFromContext() {
    let e = this.options.context;
    if (e)
      for (let n = e.depth; n >= 0; n--) {
        let r = e.node(n).contentMatchAt(e.indexAfter(n)).defaultType;
        if (r && r.isTextblock && r.defaultAttrs)
          return r;
      }
    for (let n in this.parser.schema.nodes) {
      let r = this.parser.schema.nodes[n];
      if (r.isTextblock && r.defaultAttrs)
        return r;
    }
  }
}
function my(t) {
  for (let e = t.firstChild, n = null; e; e = e.nextSibling) {
    let r = e.nodeType == 1 ? e.nodeName.toLowerCase() : null;
    r && Rf.hasOwnProperty(r) && n ? (n.appendChild(e), e = n) : r == "li" ? n = e : r && (n = null);
  }
}
function gy(t, e) {
  return (t.matches || t.msMatchesSelector || t.webkitMatchesSelector || t.mozMatchesSelector).call(t, e);
}
function cu(t) {
  let e = {};
  for (let n in t)
    e[n] = t[n];
  return e;
}
function uu(t, e) {
  let n = e.schema.nodes;
  for (let r in n) {
    let i = n[r];
    if (!i.allowsMarkType(t))
      continue;
    let o = [], s = (l) => {
      o.push(l);
      for (let a = 0; a < l.edgeCount; a++) {
        let { type: c, next: u } = l.edge(a);
        if (c == e || o.indexOf(u) < 0 && s(u))
          return !0;
      }
    };
    if (s(i.contentMatch))
      return !0;
  }
}
class Kn {
  /**
  Create a serializer. `nodes` should map node names to functions
  that take a node and return a description of the corresponding
  DOM. `marks` does the same for mark names, but also gets an
  argument that tells it whether the mark's content is block or
  inline content (for typical use, it'll always be inline). A mark
  serializer may be `null` to indicate that marks of that type
  should not be serialized.
  */
  constructor(e, n) {
    this.nodes = e, this.marks = n;
  }
  /**
  Serialize the content of this fragment to a DOM fragment. When
  not in the browser, the `document` option, containing a DOM
  document, should be passed so that the serializer can create
  nodes.
  */
  serializeFragment(e, n = {}, r) {
    r || (r = As(n).createDocumentFragment());
    let i = r, o = [];
    return e.forEach((s) => {
      if (o.length || s.marks.length) {
        let l = 0, a = 0;
        for (; l < o.length && a < s.marks.length; ) {
          let c = s.marks[a];
          if (!this.marks[c.type.name]) {
            a++;
            continue;
          }
          if (!c.eq(o[l][0]) || c.type.spec.spanning === !1)
            break;
          l++, a++;
        }
        for (; l < o.length; )
          i = o.pop()[1];
        for (; a < s.marks.length; ) {
          let c = s.marks[a++], u = this.serializeMark(c, s.isInline, n);
          u && (o.push([c, i]), i.appendChild(u.dom), i = u.contentDOM || u.dom);
        }
      }
      i.appendChild(this.serializeNodeInner(s, n));
    }), r;
  }
  /**
  @internal
  */
  serializeNodeInner(e, n) {
    let { dom: r, contentDOM: i } = fo(As(n), this.nodes[e.type.name](e), null, e.attrs);
    if (i) {
      if (e.isLeaf)
        throw new RangeError("Content hole not allowed in a leaf node spec");
      this.serializeFragment(e.content, n, i);
    }
    return r;
  }
  /**
  Serialize this node to a DOM node. This can be useful when you
  need to serialize a part of a document, as opposed to the whole
  document. To serialize a whole document, use
  [`serializeFragment`](https://prosemirror.net/docs/ref/#model.DOMSerializer.serializeFragment) on
  its [content](https://prosemirror.net/docs/ref/#model.Node.content).
  */
  serializeNode(e, n = {}) {
    let r = this.serializeNodeInner(e, n);
    for (let i = e.marks.length - 1; i >= 0; i--) {
      let o = this.serializeMark(e.marks[i], e.isInline, n);
      o && ((o.contentDOM || o.dom).appendChild(r), r = o.dom);
    }
    return r;
  }
  /**
  @internal
  */
  serializeMark(e, n, r = {}) {
    let i = this.marks[e.type.name];
    return i && fo(As(r), i(e, n), null, e.attrs);
  }
  static renderSpec(e, n, r = null, i) {
    return fo(e, n, r, i);
  }
  /**
  Build a serializer using the [`toDOM`](https://prosemirror.net/docs/ref/#model.NodeSpec.toDOM)
  properties in a schema's node and mark specs.
  */
  static fromSchema(e) {
    return e.cached.domSerializer || (e.cached.domSerializer = new Kn(this.nodesFromSchema(e), this.marksFromSchema(e)));
  }
  /**
  Gather the serializers in a schema's node specs into an object.
  This can be useful as a base to build a custom serializer from.
  */
  static nodesFromSchema(e) {
    let n = hu(e.nodes);
    return n.text || (n.text = (r) => r.text), n;
  }
  /**
  Gather the serializers in a schema's mark specs into an object.
  */
  static marksFromSchema(e) {
    return hu(e.marks);
  }
}
function hu(t) {
  let e = {};
  for (let n in t) {
    let r = t[n].spec.toDOM;
    r && (e[n] = r);
  }
  return e;
}
function As(t) {
  return t.document || window.document;
}
const fu = /* @__PURE__ */ new WeakMap();
function yy(t) {
  let e = fu.get(t);
  return e === void 0 && fu.set(t, e = ky(t)), e;
}
function ky(t) {
  let e = null;
  function n(r) {
    if (r && typeof r == "object")
      if (Array.isArray(r))
        if (typeof r[0] == "string")
          e || (e = []), e.push(r);
        else
          for (let i = 0; i < r.length; i++)
            n(r[i]);
      else
        for (let i in r)
          n(r[i]);
  }
  return n(t), e;
}
function fo(t, e, n, r) {
  if (typeof e == "string")
    return { dom: t.createTextNode(e) };
  if (e.nodeType != null)
    return { dom: e };
  if (e.dom && e.dom.nodeType != null)
    return e;
  let i = e[0], o;
  if (typeof i != "string")
    throw new RangeError("Invalid array passed to renderSpec");
  if (r && (o = yy(r)) && o.indexOf(e) > -1)
    throw new RangeError("Using an array from an attribute object as a DOM spec. This may be an attempted cross site scripting attack.");
  let s = i.indexOf(" ");
  s > 0 && (n = i.slice(0, s), i = i.slice(s + 1));
  let l, a = n ? t.createElementNS(n, i) : t.createElement(i), c = e[1], u = 1;
  if (c && typeof c == "object" && c.nodeType == null && !Array.isArray(c)) {
    u = 2;
    for (let h in c)
      if (c[h] != null) {
        let f = h.indexOf(" ");
        f > 0 ? a.setAttributeNS(h.slice(0, f), h.slice(f + 1), c[h]) : a.setAttribute(h, c[h]);
      }
  }
  for (let h = u; h < e.length; h++) {
    let f = e[h];
    if (f === 0) {
      if (h < e.length - 1 || h > u)
        throw new RangeError("Content hole must be the only child of its parent node");
      return { dom: a, contentDOM: a };
    } else {
      let { dom: d, contentDOM: p } = fo(t, f, n, r);
      if (a.appendChild(d), p) {
        if (l)
          throw new RangeError("Multiple content holes");
        l = p;
      }
    }
  }
  return { dom: a, contentDOM: l };
}
const by = {};
function wy(t, e) {
  const n = by, r = typeof n.includeImageAlt == "boolean" ? n.includeImageAlt : !0, i = typeof n.includeHtml == "boolean" ? n.includeHtml : !0;
  return vf(t, r, i);
}
function vf(t, e, n) {
  if (xy(t)) {
    if ("value" in t)
      return t.type === "html" && !n ? "" : t.value;
    if (e && "alt" in t && t.alt)
      return t.alt;
    if ("children" in t)
      return du(t.children, e, n);
  }
  return Array.isArray(t) ? du(t, e, n) : "";
}
function du(t, e, n) {
  const r = [];
  let i = -1;
  for (; ++i < t.length; )
    r[i] = vf(t[i], e, n);
  return r.join("");
}
function xy(t) {
  return !!(t && typeof t == "object");
}
const pu = document.createElement("i");
function ss(t) {
  const e = "&" + t + ";";
  pu.innerHTML = e;
  const n = pu.textContent;
  return (
    // @ts-expect-error: TypeScript is wrong that `textContent` on elements can
    // yield `null`.
    n.charCodeAt(n.length - 1) === 59 && t !== "semi" || n === e ? !1 : n
  );
}
function Mt(t, e, n, r) {
  const i = t.length;
  let o = 0, s;
  if (e < 0 ? e = -e > i ? 0 : i + e : e = e > i ? i : e, n = n > 0 ? n : 0, r.length < 1e4)
    s = Array.from(r), s.unshift(e, n), t.splice(...s);
  else
    for (n && t.splice(e, n); o < r.length; )
      s = r.slice(o, o + 1e4), s.unshift(e, 0), t.splice(...s), o += 1e4, e += 1e4;
}
function et(t, e) {
  return t.length > 0 ? (Mt(t, t.length, 0, e), t) : e;
}
const mu = {}.hasOwnProperty;
function Cy(t) {
  const e = {};
  let n = -1;
  for (; ++n < t.length; )
    Sy(e, t[n]);
  return e;
}
function Sy(t, e) {
  let n;
  for (n in e) {
    const i = (mu.call(t, n) ? t[n] : void 0) || (t[n] = {}), o = e[n];
    let s;
    if (o)
      for (s in o) {
        mu.call(i, s) || (i[s] = []);
        const l = o[s];
        My(
          // @ts-expect-error Looks like a list.
          i[s],
          Array.isArray(l) ? l : l ? [l] : []
        );
      }
  }
}
function My(t, e) {
  let n = -1;
  const r = [];
  for (; ++n < e.length; )
    (e[n].add === "after" ? t : r).push(e[n]);
  Mt(t, 0, 0, r);
}
function Pf(t, e) {
  const n = Number.parseInt(t, e);
  return (
    // C0 except for HT, LF, FF, CR, space.
    n < 9 || n === 11 || n > 13 && n < 32 || // Control character (DEL) of C0, and C1 controls.
    n > 126 && n < 160 || // Lone high surrogates and low surrogates.
    n > 55295 && n < 57344 || // Noncharacters.
    n > 64975 && n < 65008 || /* eslint-disable no-bitwise */
    (n & 65535) === 65535 || (n & 65535) === 65534 || /* eslint-enable no-bitwise */
    // Out of range
    n > 1114111 ? "�" : String.fromCodePoint(n)
  );
}
function dr(t) {
  return t.replace(/[\t\n\r ]+/g, " ").replace(/^ | $/g, "").toLowerCase().toUpperCase();
}
const Ct = mn(/[A-Za-z]/), ft = mn(/[\dA-Za-z]/), Ny = mn(/[#-'*+\--9=?A-Z^-~]/);
function xl(t) {
  return (
    // Special whitespace codes (which have negative values), C0 and Control
    // character DEL
    t !== null && (t < 32 || t === 127)
  );
}
const Cl = mn(/\d/), Ty = mn(/[\dA-Fa-f]/), Iy = mn(/[!-/:-@[-`{-~]/);
function F(t) {
  return t !== null && t < -2;
}
function Fe(t) {
  return t !== null && (t < 0 || t === 32);
}
function Z(t) {
  return t === -2 || t === -1 || t === 32;
}
const Ay = mn(new RegExp("\\p{P}|\\p{S}", "u")), Ey = mn(/\s/);
function mn(t) {
  return e;
  function e(n) {
    return n !== null && n > -1 && t.test(String.fromCharCode(n));
  }
}
function re(t, e, n, r) {
  const i = r ? r - 1 : Number.POSITIVE_INFINITY;
  let o = 0;
  return s;
  function s(a) {
    return Z(a) ? (t.enter(n), l(a)) : e(a);
  }
  function l(a) {
    return Z(a) && o++ < i ? (t.consume(a), l) : (t.exit(n), e(a));
  }
}
const Oy = {
  tokenize: Dy
};
function Dy(t) {
  const e = t.attempt(this.parser.constructs.contentInitial, r, i);
  let n;
  return e;
  function r(l) {
    if (l === null) {
      t.consume(l);
      return;
    }
    return t.enter("lineEnding"), t.consume(l), t.exit("lineEnding"), re(t, e, "linePrefix");
  }
  function i(l) {
    return t.enter("paragraph"), o(l);
  }
  function o(l) {
    const a = t.enter("chunkText", {
      contentType: "text",
      previous: n
    });
    return n && (n.next = a), n = a, s(l);
  }
  function s(l) {
    if (l === null) {
      t.exit("chunkText"), t.exit("paragraph"), t.consume(l);
      return;
    }
    return F(l) ? (t.consume(l), t.exit("chunkText"), o) : (t.consume(l), s);
  }
}
const Ry = {
  tokenize: vy
}, gu = {
  tokenize: Py
};
function vy(t) {
  const e = this, n = [];
  let r = 0, i, o, s;
  return l;
  function l(C) {
    if (r < n.length) {
      const v = n[r];
      return e.containerState = v[1], t.attempt(v[0].continuation, a, c)(C);
    }
    return c(C);
  }
  function a(C) {
    if (r++, e.containerState._closeFlow) {
      e.containerState._closeFlow = void 0, i && N();
      const v = e.events.length;
      let I = v, w;
      for (; I--; )
        if (e.events[I][0] === "exit" && e.events[I][1].type === "chunkFlow") {
          w = e.events[I][1].end;
          break;
        }
      g(r);
      let z = v;
      for (; z < e.events.length; )
        e.events[z][1].end = {
          ...w
        }, z++;
      return Mt(e.events, I + 1, 0, e.events.slice(v)), e.events.length = z, c(C);
    }
    return l(C);
  }
  function c(C) {
    if (r === n.length) {
      if (!i)
        return f(C);
      if (i.currentConstruct && i.currentConstruct.concrete)
        return p(C);
      e.interrupt = !!(i.currentConstruct && !i._gfmTableDynamicInterruptHack);
    }
    return e.containerState = {}, t.check(gu, u, h)(C);
  }
  function u(C) {
    return i && N(), g(r), f(C);
  }
  function h(C) {
    return e.parser.lazy[e.now().line] = r !== n.length, s = e.now().offset, p(C);
  }
  function f(C) {
    return e.containerState = {}, t.attempt(gu, d, p)(C);
  }
  function d(C) {
    return r++, n.push([e.currentConstruct, e.containerState]), f(C);
  }
  function p(C) {
    if (C === null) {
      i && N(), g(0), t.consume(C);
      return;
    }
    return i = i || e.parser.flow(e.now()), t.enter("chunkFlow", {
      _tokenizer: i,
      contentType: "flow",
      previous: o
    }), m(C);
  }
  function m(C) {
    if (C === null) {
      y(t.exit("chunkFlow"), !0), g(0), t.consume(C);
      return;
    }
    return F(C) ? (t.consume(C), y(t.exit("chunkFlow")), r = 0, e.interrupt = void 0, l) : (t.consume(C), m);
  }
  function y(C, v) {
    const I = e.sliceStream(C);
    if (v && I.push(null), C.previous = o, o && (o.next = C), o = C, i.defineSkip(C.start), i.write(I), e.parser.lazy[C.start.line]) {
      let w = i.events.length;
      for (; w--; )
        if (
          // The token starts before the line ending…
          i.events[w][1].start.offset < s && // …and either is not ended yet…
          (!i.events[w][1].end || // …or ends after it.
          i.events[w][1].end.offset > s)
        )
          return;
      const z = e.events.length;
      let W = z, O, x;
      for (; W--; )
        if (e.events[W][0] === "exit" && e.events[W][1].type === "chunkFlow") {
          if (O) {
            x = e.events[W][1].end;
            break;
          }
          O = !0;
        }
      for (g(r), w = z; w < e.events.length; )
        e.events[w][1].end = {
          ...x
        }, w++;
      Mt(e.events, W + 1, 0, e.events.slice(z)), e.events.length = w;
    }
  }
  function g(C) {
    let v = n.length;
    for (; v-- > C; ) {
      const I = n[v];
      e.containerState = I[1], I[0].exit.call(e, t);
    }
    n.length = C;
  }
  function N() {
    i.write([null]), o = void 0, i = void 0, e.containerState._closeFlow = void 0;
  }
}
function Py(t, e, n) {
  return re(t, t.attempt(this.parser.constructs.document, e, n), "linePrefix", this.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4);
}
function yu(t) {
  if (t === null || Fe(t) || Ey(t))
    return 1;
  if (Ay(t))
    return 2;
}
function fa(t, e, n) {
  const r = [];
  let i = -1;
  for (; ++i < t.length; ) {
    const o = t[i].resolveAll;
    o && !r.includes(o) && (e = o(e, n), r.push(o));
  }
  return e;
}
const Sl = {
  name: "attention",
  resolveAll: zy,
  tokenize: Ly
};
function zy(t, e) {
  let n = -1, r, i, o, s, l, a, c, u;
  for (; ++n < t.length; )
    if (t[n][0] === "enter" && t[n][1].type === "attentionSequence" && t[n][1]._close) {
      for (r = n; r--; )
        if (t[r][0] === "exit" && t[r][1].type === "attentionSequence" && t[r][1]._open && // If the markers are the same:
        e.sliceSerialize(t[r][1]).charCodeAt(0) === e.sliceSerialize(t[n][1]).charCodeAt(0)) {
          if ((t[r][1]._close || t[n][1]._open) && (t[n][1].end.offset - t[n][1].start.offset) % 3 && !((t[r][1].end.offset - t[r][1].start.offset + t[n][1].end.offset - t[n][1].start.offset) % 3))
            continue;
          a = t[r][1].end.offset - t[r][1].start.offset > 1 && t[n][1].end.offset - t[n][1].start.offset > 1 ? 2 : 1;
          const h = {
            ...t[r][1].end
          }, f = {
            ...t[n][1].start
          };
          ku(h, -a), ku(f, a), s = {
            type: a > 1 ? "strongSequence" : "emphasisSequence",
            start: h,
            end: {
              ...t[r][1].end
            }
          }, l = {
            type: a > 1 ? "strongSequence" : "emphasisSequence",
            start: {
              ...t[n][1].start
            },
            end: f
          }, o = {
            type: a > 1 ? "strongText" : "emphasisText",
            start: {
              ...t[r][1].end
            },
            end: {
              ...t[n][1].start
            }
          }, i = {
            type: a > 1 ? "strong" : "emphasis",
            start: {
              ...s.start
            },
            end: {
              ...l.end
            }
          }, t[r][1].end = {
            ...s.start
          }, t[n][1].start = {
            ...l.end
          }, c = [], t[r][1].end.offset - t[r][1].start.offset && (c = et(c, [["enter", t[r][1], e], ["exit", t[r][1], e]])), c = et(c, [["enter", i, e], ["enter", s, e], ["exit", s, e], ["enter", o, e]]), c = et(c, fa(e.parser.constructs.insideSpan.null, t.slice(r + 1, n), e)), c = et(c, [["exit", o, e], ["enter", l, e], ["exit", l, e], ["exit", i, e]]), t[n][1].end.offset - t[n][1].start.offset ? (u = 2, c = et(c, [["enter", t[n][1], e], ["exit", t[n][1], e]])) : u = 0, Mt(t, r - 1, n - r + 3, c), n = r + c.length - u - 2;
          break;
        }
    }
  for (n = -1; ++n < t.length; )
    t[n][1].type === "attentionSequence" && (t[n][1].type = "data");
  return t;
}
function Ly(t, e) {
  const n = this.parser.constructs.attentionMarkers.null, r = this.previous, i = yu(r);
  let o;
  return s;
  function s(a) {
    return o = a, t.enter("attentionSequence"), l(a);
  }
  function l(a) {
    if (a === o)
      return t.consume(a), l;
    const c = t.exit("attentionSequence"), u = yu(a), h = !u || u === 2 && i || n.includes(a), f = !i || i === 2 && u || n.includes(r);
    return c._open = !!(o === 42 ? h : h && (i || !f)), c._close = !!(o === 42 ? f : f && (u || !h)), e(a);
  }
}
function ku(t, e) {
  t.column += e, t.offset += e, t._bufferIndex += e;
}
const Fy = {
  name: "autolink",
  tokenize: By
};
function By(t, e, n) {
  let r = 0;
  return i;
  function i(d) {
    return t.enter("autolink"), t.enter("autolinkMarker"), t.consume(d), t.exit("autolinkMarker"), t.enter("autolinkProtocol"), o;
  }
  function o(d) {
    return Ct(d) ? (t.consume(d), s) : d === 64 ? n(d) : c(d);
  }
  function s(d) {
    return d === 43 || d === 45 || d === 46 || ft(d) ? (r = 1, l(d)) : c(d);
  }
  function l(d) {
    return d === 58 ? (t.consume(d), r = 0, a) : (d === 43 || d === 45 || d === 46 || ft(d)) && r++ < 32 ? (t.consume(d), l) : (r = 0, c(d));
  }
  function a(d) {
    return d === 62 ? (t.exit("autolinkProtocol"), t.enter("autolinkMarker"), t.consume(d), t.exit("autolinkMarker"), t.exit("autolink"), e) : d === null || d === 32 || d === 60 || xl(d) ? n(d) : (t.consume(d), a);
  }
  function c(d) {
    return d === 64 ? (t.consume(d), u) : Ny(d) ? (t.consume(d), c) : n(d);
  }
  function u(d) {
    return ft(d) ? h(d) : n(d);
  }
  function h(d) {
    return d === 46 ? (t.consume(d), r = 0, u) : d === 62 ? (t.exit("autolinkProtocol").type = "autolinkEmail", t.enter("autolinkMarker"), t.consume(d), t.exit("autolinkMarker"), t.exit("autolink"), e) : f(d);
  }
  function f(d) {
    if ((d === 45 || ft(d)) && r++ < 63) {
      const p = d === 45 ? f : h;
      return t.consume(d), p;
    }
    return n(d);
  }
}
const ls = {
  partial: !0,
  tokenize: _y
};
function _y(t, e, n) {
  return r;
  function r(o) {
    return Z(o) ? re(t, i, "linePrefix")(o) : i(o);
  }
  function i(o) {
    return o === null || F(o) ? e(o) : n(o);
  }
}
const zf = {
  continuation: {
    tokenize: Vy
  },
  exit: Wy,
  name: "blockQuote",
  tokenize: $y
};
function $y(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    if (s === 62) {
      const l = r.containerState;
      return l.open || (t.enter("blockQuote", {
        _container: !0
      }), l.open = !0), t.enter("blockQuotePrefix"), t.enter("blockQuoteMarker"), t.consume(s), t.exit("blockQuoteMarker"), o;
    }
    return n(s);
  }
  function o(s) {
    return Z(s) ? (t.enter("blockQuotePrefixWhitespace"), t.consume(s), t.exit("blockQuotePrefixWhitespace"), t.exit("blockQuotePrefix"), e) : (t.exit("blockQuotePrefix"), e(s));
  }
}
function Vy(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return Z(s) ? re(t, o, "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4)(s) : o(s);
  }
  function o(s) {
    return t.attempt(zf, e, n)(s);
  }
}
function Wy(t) {
  t.exit("blockQuote");
}
const Lf = {
  name: "characterEscape",
  tokenize: Hy
};
function Hy(t, e, n) {
  return r;
  function r(o) {
    return t.enter("characterEscape"), t.enter("escapeMarker"), t.consume(o), t.exit("escapeMarker"), i;
  }
  function i(o) {
    return Iy(o) ? (t.enter("characterEscapeValue"), t.consume(o), t.exit("characterEscapeValue"), t.exit("characterEscape"), e) : n(o);
  }
}
const Ff = {
  name: "characterReference",
  tokenize: jy
};
function jy(t, e, n) {
  const r = this;
  let i = 0, o, s;
  return l;
  function l(h) {
    return t.enter("characterReference"), t.enter("characterReferenceMarker"), t.consume(h), t.exit("characterReferenceMarker"), a;
  }
  function a(h) {
    return h === 35 ? (t.enter("characterReferenceMarkerNumeric"), t.consume(h), t.exit("characterReferenceMarkerNumeric"), c) : (t.enter("characterReferenceValue"), o = 31, s = ft, u(h));
  }
  function c(h) {
    return h === 88 || h === 120 ? (t.enter("characterReferenceMarkerHexadecimal"), t.consume(h), t.exit("characterReferenceMarkerHexadecimal"), t.enter("characterReferenceValue"), o = 6, s = Ty, u) : (t.enter("characterReferenceValue"), o = 7, s = Cl, u(h));
  }
  function u(h) {
    if (h === 59 && i) {
      const f = t.exit("characterReferenceValue");
      return s === ft && !ss(r.sliceSerialize(f)) ? n(h) : (t.enter("characterReferenceMarker"), t.consume(h), t.exit("characterReferenceMarker"), t.exit("characterReference"), e);
    }
    return s(h) && i++ < o ? (t.consume(h), u) : n(h);
  }
}
const bu = {
  partial: !0,
  tokenize: Ky
}, wu = {
  concrete: !0,
  name: "codeFenced",
  tokenize: qy
};
function qy(t, e, n) {
  const r = this, i = {
    partial: !0,
    tokenize: I
  };
  let o = 0, s = 0, l;
  return a;
  function a(w) {
    return c(w);
  }
  function c(w) {
    const z = r.events[r.events.length - 1];
    return o = z && z[1].type === "linePrefix" ? z[2].sliceSerialize(z[1], !0).length : 0, l = w, t.enter("codeFenced"), t.enter("codeFencedFence"), t.enter("codeFencedFenceSequence"), u(w);
  }
  function u(w) {
    return w === l ? (s++, t.consume(w), u) : s < 3 ? n(w) : (t.exit("codeFencedFenceSequence"), Z(w) ? re(t, h, "whitespace")(w) : h(w));
  }
  function h(w) {
    return w === null || F(w) ? (t.exit("codeFencedFence"), r.interrupt ? e(w) : t.check(bu, m, v)(w)) : (t.enter("codeFencedFenceInfo"), t.enter("chunkString", {
      contentType: "string"
    }), f(w));
  }
  function f(w) {
    return w === null || F(w) ? (t.exit("chunkString"), t.exit("codeFencedFenceInfo"), h(w)) : Z(w) ? (t.exit("chunkString"), t.exit("codeFencedFenceInfo"), re(t, d, "whitespace")(w)) : w === 96 && w === l ? n(w) : (t.consume(w), f);
  }
  function d(w) {
    return w === null || F(w) ? h(w) : (t.enter("codeFencedFenceMeta"), t.enter("chunkString", {
      contentType: "string"
    }), p(w));
  }
  function p(w) {
    return w === null || F(w) ? (t.exit("chunkString"), t.exit("codeFencedFenceMeta"), h(w)) : w === 96 && w === l ? n(w) : (t.consume(w), p);
  }
  function m(w) {
    return t.attempt(i, v, y)(w);
  }
  function y(w) {
    return t.enter("lineEnding"), t.consume(w), t.exit("lineEnding"), g;
  }
  function g(w) {
    return o > 0 && Z(w) ? re(t, N, "linePrefix", o + 1)(w) : N(w);
  }
  function N(w) {
    return w === null || F(w) ? t.check(bu, m, v)(w) : (t.enter("codeFlowValue"), C(w));
  }
  function C(w) {
    return w === null || F(w) ? (t.exit("codeFlowValue"), N(w)) : (t.consume(w), C);
  }
  function v(w) {
    return t.exit("codeFenced"), e(w);
  }
  function I(w, z, W) {
    let O = 0;
    return x;
    function x(K) {
      return w.enter("lineEnding"), w.consume(K), w.exit("lineEnding"), R;
    }
    function R(K) {
      return w.enter("codeFencedFence"), Z(K) ? re(w, D, "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4)(K) : D(K);
    }
    function D(K) {
      return K === l ? (w.enter("codeFencedFenceSequence"), J(K)) : W(K);
    }
    function J(K) {
      return K === l ? (O++, w.consume(K), J) : O >= s ? (w.exit("codeFencedFenceSequence"), Z(K) ? re(w, te, "whitespace")(K) : te(K)) : W(K);
    }
    function te(K) {
      return K === null || F(K) ? (w.exit("codeFencedFence"), z(K)) : W(K);
    }
  }
}
function Ky(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return s === null ? n(s) : (t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), o);
  }
  function o(s) {
    return r.parser.lazy[r.now().line] ? n(s) : e(s);
  }
}
const Es = {
  name: "codeIndented",
  tokenize: Uy
}, Jy = {
  partial: !0,
  tokenize: Gy
};
function Uy(t, e, n) {
  const r = this;
  return i;
  function i(c) {
    return t.enter("codeIndented"), re(t, o, "linePrefix", 5)(c);
  }
  function o(c) {
    const u = r.events[r.events.length - 1];
    return u && u[1].type === "linePrefix" && u[2].sliceSerialize(u[1], !0).length >= 4 ? s(c) : n(c);
  }
  function s(c) {
    return c === null ? a(c) : F(c) ? t.attempt(Jy, s, a)(c) : (t.enter("codeFlowValue"), l(c));
  }
  function l(c) {
    return c === null || F(c) ? (t.exit("codeFlowValue"), s(c)) : (t.consume(c), l);
  }
  function a(c) {
    return t.exit("codeIndented"), e(c);
  }
}
function Gy(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return r.parser.lazy[r.now().line] ? n(s) : F(s) ? (t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), i) : re(t, o, "linePrefix", 5)(s);
  }
  function o(s) {
    const l = r.events[r.events.length - 1];
    return l && l[1].type === "linePrefix" && l[2].sliceSerialize(l[1], !0).length >= 4 ? e(s) : F(s) ? i(s) : n(s);
  }
}
const Yy = {
  name: "codeText",
  previous: Xy,
  resolve: Qy,
  tokenize: Zy
};
function Qy(t) {
  let e = t.length - 4, n = 3, r, i;
  if ((t[n][1].type === "lineEnding" || t[n][1].type === "space") && (t[e][1].type === "lineEnding" || t[e][1].type === "space")) {
    for (r = n; ++r < e; )
      if (t[r][1].type === "codeTextData") {
        t[n][1].type = "codeTextPadding", t[e][1].type = "codeTextPadding", n += 2, e -= 2;
        break;
      }
  }
  for (r = n - 1, e++; ++r <= e; )
    i === void 0 ? r !== e && t[r][1].type !== "lineEnding" && (i = r) : (r === e || t[r][1].type === "lineEnding") && (t[i][1].type = "codeTextData", r !== i + 2 && (t[i][1].end = t[r - 1][1].end, t.splice(i + 2, r - i - 2), e -= r - i - 2, r = i + 2), i = void 0);
  return t;
}
function Xy(t) {
  return t !== 96 || this.events[this.events.length - 1][1].type === "characterEscape";
}
function Zy(t, e, n) {
  let r = 0, i, o;
  return s;
  function s(h) {
    return t.enter("codeText"), t.enter("codeTextSequence"), l(h);
  }
  function l(h) {
    return h === 96 ? (t.consume(h), r++, l) : (t.exit("codeTextSequence"), a(h));
  }
  function a(h) {
    return h === null ? n(h) : h === 32 ? (t.enter("space"), t.consume(h), t.exit("space"), a) : h === 96 ? (o = t.enter("codeTextSequence"), i = 0, u(h)) : F(h) ? (t.enter("lineEnding"), t.consume(h), t.exit("lineEnding"), a) : (t.enter("codeTextData"), c(h));
  }
  function c(h) {
    return h === null || h === 32 || h === 96 || F(h) ? (t.exit("codeTextData"), a(h)) : (t.consume(h), c);
  }
  function u(h) {
    return h === 96 ? (t.consume(h), i++, u) : i === r ? (t.exit("codeTextSequence"), t.exit("codeText"), e(h)) : (o.type = "codeTextData", c(h));
  }
}
class ek {
  /**
   * @param {ReadonlyArray<T> | null | undefined} [initial]
   *   Initial items (optional).
   * @returns
   *   Splice buffer.
   */
  constructor(e) {
    this.left = e ? [...e] : [], this.right = [];
  }
  /**
   * Array access;
   * does not move the cursor.
   *
   * @param {number} index
   *   Index.
   * @return {T}
   *   Item.
   */
  get(e) {
    if (e < 0 || e >= this.left.length + this.right.length)
      throw new RangeError("Cannot access index `" + e + "` in a splice buffer of size `" + (this.left.length + this.right.length) + "`");
    return e < this.left.length ? this.left[e] : this.right[this.right.length - e + this.left.length - 1];
  }
  /**
   * The length of the splice buffer, one greater than the largest index in the
   * array.
   */
  get length() {
    return this.left.length + this.right.length;
  }
  /**
   * Remove and return `list[0]`;
   * moves the cursor to `0`.
   *
   * @returns {T | undefined}
   *   Item, optional.
   */
  shift() {
    return this.setCursor(0), this.right.pop();
  }
  /**
   * Slice the buffer to get an array;
   * does not move the cursor.
   *
   * @param {number} start
   *   Start.
   * @param {number | null | undefined} [end]
   *   End (optional).
   * @returns {Array<T>}
   *   Array of items.
   */
  slice(e, n) {
    const r = n ?? Number.POSITIVE_INFINITY;
    return r < this.left.length ? this.left.slice(e, r) : e > this.left.length ? this.right.slice(this.right.length - r + this.left.length, this.right.length - e + this.left.length).reverse() : this.left.slice(e).concat(this.right.slice(this.right.length - r + this.left.length).reverse());
  }
  /**
   * Mimics the behavior of Array.prototype.splice() except for the change of
   * interface necessary to avoid segfaults when patching in very large arrays.
   *
   * This operation moves cursor is moved to `start` and results in the cursor
   * placed after any inserted items.
   *
   * @param {number} start
   *   Start;
   *   zero-based index at which to start changing the array;
   *   negative numbers count backwards from the end of the array and values
   *   that are out-of bounds are clamped to the appropriate end of the array.
   * @param {number | null | undefined} [deleteCount=0]
   *   Delete count (default: `0`);
   *   maximum number of elements to delete, starting from start.
   * @param {Array<T> | null | undefined} [items=[]]
   *   Items to include in place of the deleted items (default: `[]`).
   * @return {Array<T>}
   *   Any removed items.
   */
  splice(e, n, r) {
    const i = n || 0;
    this.setCursor(Math.trunc(e));
    const o = this.right.splice(this.right.length - i, Number.POSITIVE_INFINITY);
    return r && Lr(this.left, r), o.reverse();
  }
  /**
   * Remove and return the highest-numbered item in the array, so
   * `list[list.length - 1]`;
   * Moves the cursor to `length`.
   *
   * @returns {T | undefined}
   *   Item, optional.
   */
  pop() {
    return this.setCursor(Number.POSITIVE_INFINITY), this.left.pop();
  }
  /**
   * Inserts a single item to the high-numbered side of the array;
   * moves the cursor to `length`.
   *
   * @param {T} item
   *   Item.
   * @returns {undefined}
   *   Nothing.
   */
  push(e) {
    this.setCursor(Number.POSITIVE_INFINITY), this.left.push(e);
  }
  /**
   * Inserts many items to the high-numbered side of the array.
   * Moves the cursor to `length`.
   *
   * @param {Array<T>} items
   *   Items.
   * @returns {undefined}
   *   Nothing.
   */
  pushMany(e) {
    this.setCursor(Number.POSITIVE_INFINITY), Lr(this.left, e);
  }
  /**
   * Inserts a single item to the low-numbered side of the array;
   * Moves the cursor to `0`.
   *
   * @param {T} item
   *   Item.
   * @returns {undefined}
   *   Nothing.
   */
  unshift(e) {
    this.setCursor(0), this.right.push(e);
  }
  /**
   * Inserts many items to the low-numbered side of the array;
   * moves the cursor to `0`.
   *
   * @param {Array<T>} items
   *   Items.
   * @returns {undefined}
   *   Nothing.
   */
  unshiftMany(e) {
    this.setCursor(0), Lr(this.right, e.reverse());
  }
  /**
   * Move the cursor to a specific position in the array. Requires
   * time proportional to the distance moved.
   *
   * If `n < 0`, the cursor will end up at the beginning.
   * If `n > length`, the cursor will end up at the end.
   *
   * @param {number} n
   *   Position.
   * @return {undefined}
   *   Nothing.
   */
  setCursor(e) {
    if (!(e === this.left.length || e > this.left.length && this.right.length === 0 || e < 0 && this.left.length === 0))
      if (e < this.left.length) {
        const n = this.left.splice(e, Number.POSITIVE_INFINITY);
        Lr(this.right, n.reverse());
      } else {
        const n = this.right.splice(this.left.length + this.right.length - e, Number.POSITIVE_INFINITY);
        Lr(this.left, n.reverse());
      }
  }
}
function Lr(t, e) {
  let n = 0;
  if (e.length < 1e4)
    t.push(...e);
  else
    for (; n < e.length; )
      t.push(...e.slice(n, n + 1e4)), n += 1e4;
}
function Bf(t) {
  const e = {};
  let n = -1, r, i, o, s, l, a, c;
  const u = new ek(t);
  for (; ++n < u.length; ) {
    for (; n in e; )
      n = e[n];
    if (r = u.get(n), n && r[1].type === "chunkFlow" && u.get(n - 1)[1].type === "listItemPrefix" && (a = r[1]._tokenizer.events, o = 0, o < a.length && a[o][1].type === "lineEndingBlank" && (o += 2), o < a.length && a[o][1].type === "content"))
      for (; ++o < a.length && a[o][1].type !== "content"; )
        a[o][1].type === "chunkText" && (a[o][1]._isInFirstContentOfListItem = !0, o++);
    if (r[0] === "enter")
      r[1].contentType && (Object.assign(e, tk(u, n)), n = e[n], c = !0);
    else if (r[1]._container) {
      for (o = n, i = void 0; o--; )
        if (s = u.get(o), s[1].type === "lineEnding" || s[1].type === "lineEndingBlank")
          s[0] === "enter" && (i && (u.get(i)[1].type = "lineEndingBlank"), s[1].type = "lineEnding", i = o);
        else if (!(s[1].type === "linePrefix" || s[1].type === "listItemIndent")) break;
      i && (r[1].end = {
        ...u.get(i)[1].start
      }, l = u.slice(i, n), l.unshift(r), u.splice(i, n - i + 1, l));
    }
  }
  return Mt(t, 0, Number.POSITIVE_INFINITY, u.slice(0)), !c;
}
function tk(t, e) {
  const n = t.get(e)[1], r = t.get(e)[2];
  let i = e - 1;
  const o = [];
  let s = n._tokenizer;
  s || (s = r.parser[n.contentType](n.start), n._contentTypeTextTrailing && (s._contentTypeTextTrailing = !0));
  const l = s.events, a = [], c = {};
  let u, h, f = -1, d = n, p = 0, m = 0;
  const y = [m];
  for (; d; ) {
    for (; t.get(++i)[1] !== d; )
      ;
    o.push(i), d._tokenizer || (u = r.sliceStream(d), d.next || u.push(null), h && s.defineSkip(d.start), d._isInFirstContentOfListItem && (s._gfmTasklistFirstContentOfListItem = !0), s.write(u), d._isInFirstContentOfListItem && (s._gfmTasklistFirstContentOfListItem = void 0)), h = d, d = d.next;
  }
  for (d = n; ++f < l.length; )
    // Find a void token that includes a break.
    l[f][0] === "exit" && l[f - 1][0] === "enter" && l[f][1].type === l[f - 1][1].type && l[f][1].start.line !== l[f][1].end.line && (m = f + 1, y.push(m), d._tokenizer = void 0, d.previous = void 0, d = d.next);
  for (s.events = [], d ? (d._tokenizer = void 0, d.previous = void 0) : y.pop(), f = y.length; f--; ) {
    const g = l.slice(y[f], y[f + 1]), N = o.pop();
    a.push([N, N + g.length - 1]), t.splice(N, 2, g);
  }
  for (a.reverse(), f = -1; ++f < a.length; )
    c[p + a[f][0]] = p + a[f][1], p += a[f][1] - a[f][0] - 1;
  return c;
}
const nk = {
  resolve: ik,
  tokenize: ok
}, rk = {
  partial: !0,
  tokenize: sk
};
function ik(t) {
  return Bf(t), t;
}
function ok(t, e) {
  let n;
  return r;
  function r(l) {
    return t.enter("content"), n = t.enter("chunkContent", {
      contentType: "content"
    }), i(l);
  }
  function i(l) {
    return l === null ? o(l) : F(l) ? t.check(rk, s, o)(l) : (t.consume(l), i);
  }
  function o(l) {
    return t.exit("chunkContent"), t.exit("content"), e(l);
  }
  function s(l) {
    return t.consume(l), t.exit("chunkContent"), n.next = t.enter("chunkContent", {
      contentType: "content",
      previous: n
    }), n = n.next, i;
  }
}
function sk(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return t.exit("chunkContent"), t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), re(t, o, "linePrefix");
  }
  function o(s) {
    if (s === null || F(s))
      return n(s);
    const l = r.events[r.events.length - 1];
    return !r.parser.constructs.disable.null.includes("codeIndented") && l && l[1].type === "linePrefix" && l[2].sliceSerialize(l[1], !0).length >= 4 ? e(s) : t.interrupt(r.parser.constructs.flow, n, e)(s);
  }
}
function _f(t, e, n, r, i, o, s, l, a) {
  const c = a || Number.POSITIVE_INFINITY;
  let u = 0;
  return h;
  function h(g) {
    return g === 60 ? (t.enter(r), t.enter(i), t.enter(o), t.consume(g), t.exit(o), f) : g === null || g === 32 || g === 41 || xl(g) ? n(g) : (t.enter(r), t.enter(s), t.enter(l), t.enter("chunkString", {
      contentType: "string"
    }), m(g));
  }
  function f(g) {
    return g === 62 ? (t.enter(o), t.consume(g), t.exit(o), t.exit(i), t.exit(r), e) : (t.enter(l), t.enter("chunkString", {
      contentType: "string"
    }), d(g));
  }
  function d(g) {
    return g === 62 ? (t.exit("chunkString"), t.exit(l), f(g)) : g === null || g === 60 || F(g) ? n(g) : (t.consume(g), g === 92 ? p : d);
  }
  function p(g) {
    return g === 60 || g === 62 || g === 92 ? (t.consume(g), d) : d(g);
  }
  function m(g) {
    return !u && (g === null || g === 41 || Fe(g)) ? (t.exit("chunkString"), t.exit(l), t.exit(s), t.exit(r), e(g)) : u < c && g === 40 ? (t.consume(g), u++, m) : g === 41 ? (t.consume(g), u--, m) : g === null || g === 32 || g === 40 || xl(g) ? n(g) : (t.consume(g), g === 92 ? y : m);
  }
  function y(g) {
    return g === 40 || g === 41 || g === 92 ? (t.consume(g), m) : m(g);
  }
}
function $f(t, e, n, r, i, o) {
  const s = this;
  let l = 0, a;
  return c;
  function c(d) {
    return t.enter(r), t.enter(i), t.consume(d), t.exit(i), t.enter(o), u;
  }
  function u(d) {
    return l > 999 || d === null || d === 91 || d === 93 && !a || // To do: remove in the future once we’ve switched from
    // `micromark-extension-footnote` to `micromark-extension-gfm-footnote`,
    // which doesn’t need this.
    // Hidden footnotes hook.
    /* c8 ignore next 3 */
    d === 94 && !l && "_hiddenFootnoteSupport" in s.parser.constructs ? n(d) : d === 93 ? (t.exit(o), t.enter(i), t.consume(d), t.exit(i), t.exit(r), e) : F(d) ? (t.enter("lineEnding"), t.consume(d), t.exit("lineEnding"), u) : (t.enter("chunkString", {
      contentType: "string"
    }), h(d));
  }
  function h(d) {
    return d === null || d === 91 || d === 93 || F(d) || l++ > 999 ? (t.exit("chunkString"), u(d)) : (t.consume(d), a || (a = !Z(d)), d === 92 ? f : h);
  }
  function f(d) {
    return d === 91 || d === 92 || d === 93 ? (t.consume(d), l++, h) : h(d);
  }
}
function Vf(t, e, n, r, i, o) {
  let s;
  return l;
  function l(f) {
    return f === 34 || f === 39 || f === 40 ? (t.enter(r), t.enter(i), t.consume(f), t.exit(i), s = f === 40 ? 41 : f, a) : n(f);
  }
  function a(f) {
    return f === s ? (t.enter(i), t.consume(f), t.exit(i), t.exit(r), e) : (t.enter(o), c(f));
  }
  function c(f) {
    return f === s ? (t.exit(o), a(s)) : f === null ? n(f) : F(f) ? (t.enter("lineEnding"), t.consume(f), t.exit("lineEnding"), re(t, c, "linePrefix")) : (t.enter("chunkString", {
      contentType: "string"
    }), u(f));
  }
  function u(f) {
    return f === s || f === null || F(f) ? (t.exit("chunkString"), c(f)) : (t.consume(f), f === 92 ? h : u);
  }
  function h(f) {
    return f === s || f === 92 ? (t.consume(f), u) : u(f);
  }
}
function ai(t, e) {
  let n;
  return r;
  function r(i) {
    return F(i) ? (t.enter("lineEnding"), t.consume(i), t.exit("lineEnding"), n = !0, r) : Z(i) ? re(t, r, n ? "linePrefix" : "lineSuffix")(i) : e(i);
  }
}
const lk = {
  name: "definition",
  tokenize: ck
}, ak = {
  partial: !0,
  tokenize: uk
};
function ck(t, e, n) {
  const r = this;
  let i;
  return o;
  function o(d) {
    return t.enter("definition"), s(d);
  }
  function s(d) {
    return $f.call(
      r,
      t,
      l,
      // Note: we don’t need to reset the way `markdown-rs` does.
      n,
      "definitionLabel",
      "definitionLabelMarker",
      "definitionLabelString"
    )(d);
  }
  function l(d) {
    return i = dr(r.sliceSerialize(r.events[r.events.length - 1][1]).slice(1, -1)), d === 58 ? (t.enter("definitionMarker"), t.consume(d), t.exit("definitionMarker"), a) : n(d);
  }
  function a(d) {
    return Fe(d) ? ai(t, c)(d) : c(d);
  }
  function c(d) {
    return _f(
      t,
      u,
      // Note: we don’t need to reset the way `markdown-rs` does.
      n,
      "definitionDestination",
      "definitionDestinationLiteral",
      "definitionDestinationLiteralMarker",
      "definitionDestinationRaw",
      "definitionDestinationString"
    )(d);
  }
  function u(d) {
    return t.attempt(ak, h, h)(d);
  }
  function h(d) {
    return Z(d) ? re(t, f, "whitespace")(d) : f(d);
  }
  function f(d) {
    return d === null || F(d) ? (t.exit("definition"), r.parser.defined.push(i), e(d)) : n(d);
  }
}
function uk(t, e, n) {
  return r;
  function r(l) {
    return Fe(l) ? ai(t, i)(l) : n(l);
  }
  function i(l) {
    return Vf(t, o, n, "definitionTitle", "definitionTitleMarker", "definitionTitleString")(l);
  }
  function o(l) {
    return Z(l) ? re(t, s, "whitespace")(l) : s(l);
  }
  function s(l) {
    return l === null || F(l) ? e(l) : n(l);
  }
}
const hk = {
  name: "hardBreakEscape",
  tokenize: fk
};
function fk(t, e, n) {
  return r;
  function r(o) {
    return t.enter("hardBreakEscape"), t.consume(o), i;
  }
  function i(o) {
    return F(o) ? (t.exit("hardBreakEscape"), e(o)) : n(o);
  }
}
const dk = {
  name: "headingAtx",
  resolve: pk,
  tokenize: mk
};
function pk(t, e) {
  let n = t.length - 2, r = 3, i, o;
  return t[r][1].type === "whitespace" && (r += 2), n - 2 > r && t[n][1].type === "whitespace" && (n -= 2), t[n][1].type === "atxHeadingSequence" && (r === n - 1 || n - 4 > r && t[n - 2][1].type === "whitespace") && (n -= r + 1 === n ? 2 : 4), n > r && (i = {
    type: "atxHeadingText",
    start: t[r][1].start,
    end: t[n][1].end
  }, o = {
    type: "chunkText",
    start: t[r][1].start,
    end: t[n][1].end,
    contentType: "text"
  }, Mt(t, r, n - r + 1, [["enter", i, e], ["enter", o, e], ["exit", o, e], ["exit", i, e]])), t;
}
function mk(t, e, n) {
  let r = 0;
  return i;
  function i(u) {
    return t.enter("atxHeading"), o(u);
  }
  function o(u) {
    return t.enter("atxHeadingSequence"), s(u);
  }
  function s(u) {
    return u === 35 && r++ < 6 ? (t.consume(u), s) : u === null || Fe(u) ? (t.exit("atxHeadingSequence"), l(u)) : n(u);
  }
  function l(u) {
    return u === 35 ? (t.enter("atxHeadingSequence"), a(u)) : u === null || F(u) ? (t.exit("atxHeading"), e(u)) : Z(u) ? re(t, l, "whitespace")(u) : (t.enter("atxHeadingText"), c(u));
  }
  function a(u) {
    return u === 35 ? (t.consume(u), a) : (t.exit("atxHeadingSequence"), l(u));
  }
  function c(u) {
    return u === null || u === 35 || Fe(u) ? (t.exit("atxHeadingText"), l(u)) : (t.consume(u), c);
  }
}
const gk = [
  "address",
  "article",
  "aside",
  "base",
  "basefont",
  "blockquote",
  "body",
  "caption",
  "center",
  "col",
  "colgroup",
  "dd",
  "details",
  "dialog",
  "dir",
  "div",
  "dl",
  "dt",
  "fieldset",
  "figcaption",
  "figure",
  "footer",
  "form",
  "frame",
  "frameset",
  "h1",
  "h2",
  "h3",
  "h4",
  "h5",
  "h6",
  "head",
  "header",
  "hr",
  "html",
  "iframe",
  "legend",
  "li",
  "link",
  "main",
  "menu",
  "menuitem",
  "nav",
  "noframes",
  "ol",
  "optgroup",
  "option",
  "p",
  "param",
  "search",
  "section",
  "summary",
  "table",
  "tbody",
  "td",
  "tfoot",
  "th",
  "thead",
  "title",
  "tr",
  "track",
  "ul"
], xu = ["pre", "script", "style", "textarea"], yk = {
  concrete: !0,
  name: "htmlFlow",
  resolveTo: wk,
  tokenize: xk
}, kk = {
  partial: !0,
  tokenize: Sk
}, bk = {
  partial: !0,
  tokenize: Ck
};
function wk(t) {
  let e = t.length;
  for (; e-- && !(t[e][0] === "enter" && t[e][1].type === "htmlFlow"); )
    ;
  return e > 1 && t[e - 2][1].type === "linePrefix" && (t[e][1].start = t[e - 2][1].start, t[e + 1][1].start = t[e - 2][1].start, t.splice(e - 2, 2)), t;
}
function xk(t, e, n) {
  const r = this;
  let i, o, s, l, a;
  return c;
  function c(b) {
    return u(b);
  }
  function u(b) {
    return t.enter("htmlFlow"), t.enter("htmlFlowData"), t.consume(b), h;
  }
  function h(b) {
    return b === 33 ? (t.consume(b), f) : b === 47 ? (t.consume(b), o = !0, m) : b === 63 ? (t.consume(b), i = 3, r.interrupt ? e : k) : Ct(b) ? (t.consume(b), s = String.fromCharCode(b), y) : n(b);
  }
  function f(b) {
    return b === 45 ? (t.consume(b), i = 2, d) : b === 91 ? (t.consume(b), i = 5, l = 0, p) : Ct(b) ? (t.consume(b), i = 4, r.interrupt ? e : k) : n(b);
  }
  function d(b) {
    return b === 45 ? (t.consume(b), r.interrupt ? e : k) : n(b);
  }
  function p(b) {
    const He = "CDATA[";
    return b === He.charCodeAt(l++) ? (t.consume(b), l === He.length ? r.interrupt ? e : D : p) : n(b);
  }
  function m(b) {
    return Ct(b) ? (t.consume(b), s = String.fromCharCode(b), y) : n(b);
  }
  function y(b) {
    if (b === null || b === 47 || b === 62 || Fe(b)) {
      const He = b === 47, Dt = s.toLowerCase();
      return !He && !o && xu.includes(Dt) ? (i = 1, r.interrupt ? e(b) : D(b)) : gk.includes(s.toLowerCase()) ? (i = 6, He ? (t.consume(b), g) : r.interrupt ? e(b) : D(b)) : (i = 7, r.interrupt && !r.parser.lazy[r.now().line] ? n(b) : o ? N(b) : C(b));
    }
    return b === 45 || ft(b) ? (t.consume(b), s += String.fromCharCode(b), y) : n(b);
  }
  function g(b) {
    return b === 62 ? (t.consume(b), r.interrupt ? e : D) : n(b);
  }
  function N(b) {
    return Z(b) ? (t.consume(b), N) : x(b);
  }
  function C(b) {
    return b === 47 ? (t.consume(b), x) : b === 58 || b === 95 || Ct(b) ? (t.consume(b), v) : Z(b) ? (t.consume(b), C) : x(b);
  }
  function v(b) {
    return b === 45 || b === 46 || b === 58 || b === 95 || ft(b) ? (t.consume(b), v) : I(b);
  }
  function I(b) {
    return b === 61 ? (t.consume(b), w) : Z(b) ? (t.consume(b), I) : C(b);
  }
  function w(b) {
    return b === null || b === 60 || b === 61 || b === 62 || b === 96 ? n(b) : b === 34 || b === 39 ? (t.consume(b), a = b, z) : Z(b) ? (t.consume(b), w) : W(b);
  }
  function z(b) {
    return b === a ? (t.consume(b), a = null, O) : b === null || F(b) ? n(b) : (t.consume(b), z);
  }
  function W(b) {
    return b === null || b === 34 || b === 39 || b === 47 || b === 60 || b === 61 || b === 62 || b === 96 || Fe(b) ? I(b) : (t.consume(b), W);
  }
  function O(b) {
    return b === 47 || b === 62 || Z(b) ? C(b) : n(b);
  }
  function x(b) {
    return b === 62 ? (t.consume(b), R) : n(b);
  }
  function R(b) {
    return b === null || F(b) ? D(b) : Z(b) ? (t.consume(b), R) : n(b);
  }
  function D(b) {
    return b === 45 && i === 2 ? (t.consume(b), ce) : b === 60 && i === 1 ? (t.consume(b), he) : b === 62 && i === 4 ? (t.consume(b), Pe) : b === 63 && i === 3 ? (t.consume(b), k) : b === 93 && i === 5 ? (t.consume(b), lt) : F(b) && (i === 6 || i === 7) ? (t.exit("htmlFlowData"), t.check(kk, ze, J)(b)) : b === null || F(b) ? (t.exit("htmlFlowData"), J(b)) : (t.consume(b), D);
  }
  function J(b) {
    return t.check(bk, te, ze)(b);
  }
  function te(b) {
    return t.enter("lineEnding"), t.consume(b), t.exit("lineEnding"), K;
  }
  function K(b) {
    return b === null || F(b) ? J(b) : (t.enter("htmlFlowData"), D(b));
  }
  function ce(b) {
    return b === 45 ? (t.consume(b), k) : D(b);
  }
  function he(b) {
    return b === 47 ? (t.consume(b), s = "", Oe) : D(b);
  }
  function Oe(b) {
    if (b === 62) {
      const He = s.toLowerCase();
      return xu.includes(He) ? (t.consume(b), Pe) : D(b);
    }
    return Ct(b) && s.length < 8 ? (t.consume(b), s += String.fromCharCode(b), Oe) : D(b);
  }
  function lt(b) {
    return b === 93 ? (t.consume(b), k) : D(b);
  }
  function k(b) {
    return b === 62 ? (t.consume(b), Pe) : b === 45 && i === 2 ? (t.consume(b), k) : D(b);
  }
  function Pe(b) {
    return b === null || F(b) ? (t.exit("htmlFlowData"), ze(b)) : (t.consume(b), Pe);
  }
  function ze(b) {
    return t.exit("htmlFlow"), e(b);
  }
}
function Ck(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return F(s) ? (t.enter("lineEnding"), t.consume(s), t.exit("lineEnding"), o) : n(s);
  }
  function o(s) {
    return r.parser.lazy[r.now().line] ? n(s) : e(s);
  }
}
function Sk(t, e, n) {
  return r;
  function r(i) {
    return t.enter("lineEnding"), t.consume(i), t.exit("lineEnding"), t.attempt(ls, e, n);
  }
}
const Mk = {
  name: "htmlText",
  tokenize: Nk
};
function Nk(t, e, n) {
  const r = this;
  let i, o, s;
  return l;
  function l(k) {
    return t.enter("htmlText"), t.enter("htmlTextData"), t.consume(k), a;
  }
  function a(k) {
    return k === 33 ? (t.consume(k), c) : k === 47 ? (t.consume(k), I) : k === 63 ? (t.consume(k), C) : Ct(k) ? (t.consume(k), W) : n(k);
  }
  function c(k) {
    return k === 45 ? (t.consume(k), u) : k === 91 ? (t.consume(k), o = 0, p) : Ct(k) ? (t.consume(k), N) : n(k);
  }
  function u(k) {
    return k === 45 ? (t.consume(k), d) : n(k);
  }
  function h(k) {
    return k === null ? n(k) : k === 45 ? (t.consume(k), f) : F(k) ? (s = h, he(k)) : (t.consume(k), h);
  }
  function f(k) {
    return k === 45 ? (t.consume(k), d) : h(k);
  }
  function d(k) {
    return k === 62 ? ce(k) : k === 45 ? f(k) : h(k);
  }
  function p(k) {
    const Pe = "CDATA[";
    return k === Pe.charCodeAt(o++) ? (t.consume(k), o === Pe.length ? m : p) : n(k);
  }
  function m(k) {
    return k === null ? n(k) : k === 93 ? (t.consume(k), y) : F(k) ? (s = m, he(k)) : (t.consume(k), m);
  }
  function y(k) {
    return k === 93 ? (t.consume(k), g) : m(k);
  }
  function g(k) {
    return k === 62 ? ce(k) : k === 93 ? (t.consume(k), g) : m(k);
  }
  function N(k) {
    return k === null || k === 62 ? ce(k) : F(k) ? (s = N, he(k)) : (t.consume(k), N);
  }
  function C(k) {
    return k === null ? n(k) : k === 63 ? (t.consume(k), v) : F(k) ? (s = C, he(k)) : (t.consume(k), C);
  }
  function v(k) {
    return k === 62 ? ce(k) : C(k);
  }
  function I(k) {
    return Ct(k) ? (t.consume(k), w) : n(k);
  }
  function w(k) {
    return k === 45 || ft(k) ? (t.consume(k), w) : z(k);
  }
  function z(k) {
    return F(k) ? (s = z, he(k)) : Z(k) ? (t.consume(k), z) : ce(k);
  }
  function W(k) {
    return k === 45 || ft(k) ? (t.consume(k), W) : k === 47 || k === 62 || Fe(k) ? O(k) : n(k);
  }
  function O(k) {
    return k === 47 ? (t.consume(k), ce) : k === 58 || k === 95 || Ct(k) ? (t.consume(k), x) : F(k) ? (s = O, he(k)) : Z(k) ? (t.consume(k), O) : ce(k);
  }
  function x(k) {
    return k === 45 || k === 46 || k === 58 || k === 95 || ft(k) ? (t.consume(k), x) : R(k);
  }
  function R(k) {
    return k === 61 ? (t.consume(k), D) : F(k) ? (s = R, he(k)) : Z(k) ? (t.consume(k), R) : O(k);
  }
  function D(k) {
    return k === null || k === 60 || k === 61 || k === 62 || k === 96 ? n(k) : k === 34 || k === 39 ? (t.consume(k), i = k, J) : F(k) ? (s = D, he(k)) : Z(k) ? (t.consume(k), D) : (t.consume(k), te);
  }
  function J(k) {
    return k === i ? (t.consume(k), i = void 0, K) : k === null ? n(k) : F(k) ? (s = J, he(k)) : (t.consume(k), J);
  }
  function te(k) {
    return k === null || k === 34 || k === 39 || k === 60 || k === 61 || k === 96 ? n(k) : k === 47 || k === 62 || Fe(k) ? O(k) : (t.consume(k), te);
  }
  function K(k) {
    return k === 47 || k === 62 || Fe(k) ? O(k) : n(k);
  }
  function ce(k) {
    return k === 62 ? (t.consume(k), t.exit("htmlTextData"), t.exit("htmlText"), e) : n(k);
  }
  function he(k) {
    return t.exit("htmlTextData"), t.enter("lineEnding"), t.consume(k), t.exit("lineEnding"), Oe;
  }
  function Oe(k) {
    return Z(k) ? re(t, lt, "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4)(k) : lt(k);
  }
  function lt(k) {
    return t.enter("htmlTextData"), s(k);
  }
}
const da = {
  name: "labelEnd",
  resolveAll: Ek,
  resolveTo: Ok,
  tokenize: Dk
}, Tk = {
  tokenize: Rk
}, Ik = {
  tokenize: vk
}, Ak = {
  tokenize: Pk
};
function Ek(t) {
  let e = -1;
  const n = [];
  for (; ++e < t.length; ) {
    const r = t[e][1];
    if (n.push(t[e]), r.type === "labelImage" || r.type === "labelLink" || r.type === "labelEnd") {
      const i = r.type === "labelImage" ? 4 : 2;
      r.type = "data", e += i;
    }
  }
  return t.length !== n.length && Mt(t, 0, t.length, n), t;
}
function Ok(t, e) {
  let n = t.length, r = 0, i, o, s, l;
  for (; n--; )
    if (i = t[n][1], o) {
      if (i.type === "link" || i.type === "labelLink" && i._inactive)
        break;
      t[n][0] === "enter" && i.type === "labelLink" && (i._inactive = !0);
    } else if (s) {
      if (t[n][0] === "enter" && (i.type === "labelImage" || i.type === "labelLink") && !i._balanced && (o = n, i.type !== "labelLink")) {
        r = 2;
        break;
      }
    } else i.type === "labelEnd" && (s = n);
  const a = {
    type: t[o][1].type === "labelLink" ? "link" : "image",
    start: {
      ...t[o][1].start
    },
    end: {
      ...t[t.length - 1][1].end
    }
  }, c = {
    type: "label",
    start: {
      ...t[o][1].start
    },
    end: {
      ...t[s][1].end
    }
  }, u = {
    type: "labelText",
    start: {
      ...t[o + r + 2][1].end
    },
    end: {
      ...t[s - 2][1].start
    }
  };
  return l = [["enter", a, e], ["enter", c, e]], l = et(l, t.slice(o + 1, o + r + 3)), l = et(l, [["enter", u, e]]), l = et(l, fa(e.parser.constructs.insideSpan.null, t.slice(o + r + 4, s - 3), e)), l = et(l, [["exit", u, e], t[s - 2], t[s - 1], ["exit", c, e]]), l = et(l, t.slice(s + 1)), l = et(l, [["exit", a, e]]), Mt(t, o, t.length, l), t;
}
function Dk(t, e, n) {
  const r = this;
  let i = r.events.length, o, s;
  for (; i--; )
    if ((r.events[i][1].type === "labelImage" || r.events[i][1].type === "labelLink") && !r.events[i][1]._balanced) {
      o = r.events[i][1];
      break;
    }
  return l;
  function l(f) {
    return o ? o._inactive ? h(f) : (s = r.parser.defined.includes(dr(r.sliceSerialize({
      start: o.end,
      end: r.now()
    }))), t.enter("labelEnd"), t.enter("labelMarker"), t.consume(f), t.exit("labelMarker"), t.exit("labelEnd"), a) : n(f);
  }
  function a(f) {
    return f === 40 ? t.attempt(Tk, u, s ? u : h)(f) : f === 91 ? t.attempt(Ik, u, s ? c : h)(f) : s ? u(f) : h(f);
  }
  function c(f) {
    return t.attempt(Ak, u, h)(f);
  }
  function u(f) {
    return e(f);
  }
  function h(f) {
    return o._balanced = !0, n(f);
  }
}
function Rk(t, e, n) {
  return r;
  function r(h) {
    return t.enter("resource"), t.enter("resourceMarker"), t.consume(h), t.exit("resourceMarker"), i;
  }
  function i(h) {
    return Fe(h) ? ai(t, o)(h) : o(h);
  }
  function o(h) {
    return h === 41 ? u(h) : _f(t, s, l, "resourceDestination", "resourceDestinationLiteral", "resourceDestinationLiteralMarker", "resourceDestinationRaw", "resourceDestinationString", 32)(h);
  }
  function s(h) {
    return Fe(h) ? ai(t, a)(h) : u(h);
  }
  function l(h) {
    return n(h);
  }
  function a(h) {
    return h === 34 || h === 39 || h === 40 ? Vf(t, c, n, "resourceTitle", "resourceTitleMarker", "resourceTitleString")(h) : u(h);
  }
  function c(h) {
    return Fe(h) ? ai(t, u)(h) : u(h);
  }
  function u(h) {
    return h === 41 ? (t.enter("resourceMarker"), t.consume(h), t.exit("resourceMarker"), t.exit("resource"), e) : n(h);
  }
}
function vk(t, e, n) {
  const r = this;
  return i;
  function i(l) {
    return $f.call(r, t, o, s, "reference", "referenceMarker", "referenceString")(l);
  }
  function o(l) {
    return r.parser.defined.includes(dr(r.sliceSerialize(r.events[r.events.length - 1][1]).slice(1, -1))) ? e(l) : n(l);
  }
  function s(l) {
    return n(l);
  }
}
function Pk(t, e, n) {
  return r;
  function r(o) {
    return t.enter("reference"), t.enter("referenceMarker"), t.consume(o), t.exit("referenceMarker"), i;
  }
  function i(o) {
    return o === 93 ? (t.enter("referenceMarker"), t.consume(o), t.exit("referenceMarker"), t.exit("reference"), e) : n(o);
  }
}
const zk = {
  name: "labelStartImage",
  resolveAll: da.resolveAll,
  tokenize: Lk
};
function Lk(t, e, n) {
  const r = this;
  return i;
  function i(l) {
    return t.enter("labelImage"), t.enter("labelImageMarker"), t.consume(l), t.exit("labelImageMarker"), o;
  }
  function o(l) {
    return l === 91 ? (t.enter("labelMarker"), t.consume(l), t.exit("labelMarker"), t.exit("labelImage"), s) : n(l);
  }
  function s(l) {
    return l === 94 && "_hiddenFootnoteSupport" in r.parser.constructs ? n(l) : e(l);
  }
}
const Fk = {
  name: "labelStartLink",
  resolveAll: da.resolveAll,
  tokenize: Bk
};
function Bk(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return t.enter("labelLink"), t.enter("labelMarker"), t.consume(s), t.exit("labelMarker"), t.exit("labelLink"), o;
  }
  function o(s) {
    return s === 94 && "_hiddenFootnoteSupport" in r.parser.constructs ? n(s) : e(s);
  }
}
const Os = {
  name: "lineEnding",
  tokenize: _k
};
function _k(t, e) {
  return n;
  function n(r) {
    return t.enter("lineEnding"), t.consume(r), t.exit("lineEnding"), re(t, e, "linePrefix");
  }
}
const po = {
  name: "thematicBreak",
  tokenize: $k
};
function $k(t, e, n) {
  let r = 0, i;
  return o;
  function o(c) {
    return t.enter("thematicBreak"), s(c);
  }
  function s(c) {
    return i = c, l(c);
  }
  function l(c) {
    return c === i ? (t.enter("thematicBreakSequence"), a(c)) : r >= 3 && (c === null || F(c)) ? (t.exit("thematicBreak"), e(c)) : n(c);
  }
  function a(c) {
    return c === i ? (t.consume(c), r++, a) : (t.exit("thematicBreakSequence"), Z(c) ? re(t, l, "whitespace")(c) : l(c));
  }
}
const Le = {
  continuation: {
    tokenize: jk
  },
  exit: Kk,
  name: "list",
  tokenize: Hk
}, Vk = {
  partial: !0,
  tokenize: Jk
}, Wk = {
  partial: !0,
  tokenize: qk
};
function Hk(t, e, n) {
  const r = this, i = r.events[r.events.length - 1];
  let o = i && i[1].type === "linePrefix" ? i[2].sliceSerialize(i[1], !0).length : 0, s = 0;
  return l;
  function l(d) {
    const p = r.containerState.type || (d === 42 || d === 43 || d === 45 ? "listUnordered" : "listOrdered");
    if (p === "listUnordered" ? !r.containerState.marker || d === r.containerState.marker : Cl(d)) {
      if (r.containerState.type || (r.containerState.type = p, t.enter(p, {
        _container: !0
      })), p === "listUnordered")
        return t.enter("listItemPrefix"), d === 42 || d === 45 ? t.check(po, n, c)(d) : c(d);
      if (!r.interrupt || d === 49)
        return t.enter("listItemPrefix"), t.enter("listItemValue"), a(d);
    }
    return n(d);
  }
  function a(d) {
    return Cl(d) && ++s < 10 ? (t.consume(d), a) : (!r.interrupt || s < 2) && (r.containerState.marker ? d === r.containerState.marker : d === 41 || d === 46) ? (t.exit("listItemValue"), c(d)) : n(d);
  }
  function c(d) {
    return t.enter("listItemMarker"), t.consume(d), t.exit("listItemMarker"), r.containerState.marker = r.containerState.marker || d, t.check(
      ls,
      // Can’t be empty when interrupting.
      r.interrupt ? n : u,
      t.attempt(Vk, f, h)
    );
  }
  function u(d) {
    return r.containerState.initialBlankLine = !0, o++, f(d);
  }
  function h(d) {
    return Z(d) ? (t.enter("listItemPrefixWhitespace"), t.consume(d), t.exit("listItemPrefixWhitespace"), f) : n(d);
  }
  function f(d) {
    return r.containerState.size = o + r.sliceSerialize(t.exit("listItemPrefix"), !0).length, e(d);
  }
}
function jk(t, e, n) {
  const r = this;
  return r.containerState._closeFlow = void 0, t.check(ls, i, o);
  function i(l) {
    return r.containerState.furtherBlankLines = r.containerState.furtherBlankLines || r.containerState.initialBlankLine, re(t, e, "listItemIndent", r.containerState.size + 1)(l);
  }
  function o(l) {
    return r.containerState.furtherBlankLines || !Z(l) ? (r.containerState.furtherBlankLines = void 0, r.containerState.initialBlankLine = void 0, s(l)) : (r.containerState.furtherBlankLines = void 0, r.containerState.initialBlankLine = void 0, t.attempt(Wk, e, s)(l));
  }
  function s(l) {
    return r.containerState._closeFlow = !0, r.interrupt = void 0, re(t, t.attempt(Le, e, n), "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4)(l);
  }
}
function qk(t, e, n) {
  const r = this;
  return re(t, i, "listItemIndent", r.containerState.size + 1);
  function i(o) {
    const s = r.events[r.events.length - 1];
    return s && s[1].type === "listItemIndent" && s[2].sliceSerialize(s[1], !0).length === r.containerState.size ? e(o) : n(o);
  }
}
function Kk(t) {
  t.exit(this.containerState.type);
}
function Jk(t, e, n) {
  const r = this;
  return re(t, i, "listItemPrefixWhitespace", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 5);
  function i(o) {
    const s = r.events[r.events.length - 1];
    return !Z(o) && s && s[1].type === "listItemPrefixWhitespace" ? e(o) : n(o);
  }
}
const Cu = {
  name: "setextUnderline",
  resolveTo: Uk,
  tokenize: Gk
};
function Uk(t, e) {
  let n = t.length, r, i, o;
  for (; n--; )
    if (t[n][0] === "enter") {
      if (t[n][1].type === "content") {
        r = n;
        break;
      }
      t[n][1].type === "paragraph" && (i = n);
    } else
      t[n][1].type === "content" && t.splice(n, 1), !o && t[n][1].type === "definition" && (o = n);
  const s = {
    type: "setextHeading",
    start: {
      ...t[r][1].start
    },
    end: {
      ...t[t.length - 1][1].end
    }
  };
  return t[i][1].type = "setextHeadingText", o ? (t.splice(i, 0, ["enter", s, e]), t.splice(o + 1, 0, ["exit", t[r][1], e]), t[r][1].end = {
    ...t[o][1].end
  }) : t[r][1] = s, t.push(["exit", s, e]), t;
}
function Gk(t, e, n) {
  const r = this;
  let i;
  return o;
  function o(c) {
    let u = r.events.length, h;
    for (; u--; )
      if (r.events[u][1].type !== "lineEnding" && r.events[u][1].type !== "linePrefix" && r.events[u][1].type !== "content") {
        h = r.events[u][1].type === "paragraph";
        break;
      }
    return !r.parser.lazy[r.now().line] && (r.interrupt || h) ? (t.enter("setextHeadingLine"), i = c, s(c)) : n(c);
  }
  function s(c) {
    return t.enter("setextHeadingLineSequence"), l(c);
  }
  function l(c) {
    return c === i ? (t.consume(c), l) : (t.exit("setextHeadingLineSequence"), Z(c) ? re(t, a, "lineSuffix")(c) : a(c));
  }
  function a(c) {
    return c === null || F(c) ? (t.exit("setextHeadingLine"), e(c)) : n(c);
  }
}
const Yk = {
  tokenize: Qk
};
function Qk(t) {
  const e = this, n = t.attempt(
    // Try to parse a blank line.
    ls,
    r,
    // Try to parse initial flow (essentially, only code).
    t.attempt(this.parser.constructs.flowInitial, i, re(t, t.attempt(this.parser.constructs.flow, i, t.attempt(nk, i)), "linePrefix"))
  );
  return n;
  function r(o) {
    if (o === null) {
      t.consume(o);
      return;
    }
    return t.enter("lineEndingBlank"), t.consume(o), t.exit("lineEndingBlank"), e.currentConstruct = void 0, n;
  }
  function i(o) {
    if (o === null) {
      t.consume(o);
      return;
    }
    return t.enter("lineEnding"), t.consume(o), t.exit("lineEnding"), e.currentConstruct = void 0, n;
  }
}
const Xk = {
  resolveAll: Hf()
}, Zk = Wf("string"), e1 = Wf("text");
function Wf(t) {
  return {
    resolveAll: Hf(t === "text" ? t1 : void 0),
    tokenize: e
  };
  function e(n) {
    const r = this, i = this.parser.constructs[t], o = n.attempt(i, s, l);
    return s;
    function s(u) {
      return c(u) ? o(u) : l(u);
    }
    function l(u) {
      if (u === null) {
        n.consume(u);
        return;
      }
      return n.enter("data"), n.consume(u), a;
    }
    function a(u) {
      return c(u) ? (n.exit("data"), o(u)) : (n.consume(u), a);
    }
    function c(u) {
      if (u === null)
        return !0;
      const h = i[u];
      let f = -1;
      if (h)
        for (; ++f < h.length; ) {
          const d = h[f];
          if (!d.previous || d.previous.call(r, r.previous))
            return !0;
        }
      return !1;
    }
  }
}
function Hf(t) {
  return e;
  function e(n, r) {
    let i = -1, o;
    for (; ++i <= n.length; )
      o === void 0 ? n[i] && n[i][1].type === "data" && (o = i, i++) : (!n[i] || n[i][1].type !== "data") && (i !== o + 2 && (n[o][1].end = n[i - 1][1].end, n.splice(o + 2, i - o - 2), i = o + 2), o = void 0);
    return t ? t(n, r) : n;
  }
}
function t1(t, e) {
  let n = 0;
  for (; ++n <= t.length; )
    if ((n === t.length || t[n][1].type === "lineEnding") && t[n - 1][1].type === "data") {
      const r = t[n - 1][1], i = e.sliceStream(r);
      let o = i.length, s = -1, l = 0, a;
      for (; o--; ) {
        const c = i[o];
        if (typeof c == "string") {
          for (s = c.length; c.charCodeAt(s - 1) === 32; )
            l++, s--;
          if (s) break;
          s = -1;
        } else if (c === -2)
          a = !0, l++;
        else if (c !== -1) {
          o++;
          break;
        }
      }
      if (e._contentTypeTextTrailing && n === t.length && (l = 0), l) {
        const c = {
          type: n === t.length || a || l < 2 ? "lineSuffix" : "hardBreakTrailing",
          start: {
            _bufferIndex: o ? s : r.start._bufferIndex + s,
            _index: r.start._index + o,
            line: r.end.line,
            column: r.end.column - l,
            offset: r.end.offset - l
          },
          end: {
            ...r.end
          }
        };
        r.end = {
          ...c.start
        }, r.start.offset === r.end.offset ? Object.assign(r, c) : (t.splice(n, 0, ["enter", c, e], ["exit", c, e]), n += 2);
      }
      n++;
    }
  return t;
}
const n1 = {
  42: Le,
  43: Le,
  45: Le,
  48: Le,
  49: Le,
  50: Le,
  51: Le,
  52: Le,
  53: Le,
  54: Le,
  55: Le,
  56: Le,
  57: Le,
  62: zf
}, r1 = {
  91: lk
}, i1 = {
  [-2]: Es,
  [-1]: Es,
  32: Es
}, o1 = {
  35: dk,
  42: po,
  45: [Cu, po],
  60: yk,
  61: Cu,
  95: po,
  96: wu,
  126: wu
}, s1 = {
  38: Ff,
  92: Lf
}, l1 = {
  [-5]: Os,
  [-4]: Os,
  [-3]: Os,
  33: zk,
  38: Ff,
  42: Sl,
  60: [Fy, Mk],
  91: Fk,
  92: [hk, Lf],
  93: da,
  95: Sl,
  96: Yy
}, a1 = {
  null: [Sl, Xk]
}, c1 = {
  null: [42, 95]
}, u1 = {
  null: []
}, h1 = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  attentionMarkers: c1,
  contentInitial: r1,
  disable: u1,
  document: n1,
  flow: o1,
  flowInitial: i1,
  insideSpan: a1,
  string: s1,
  text: l1
}, Symbol.toStringTag, { value: "Module" }));
function f1(t, e, n) {
  let r = {
    _bufferIndex: -1,
    _index: 0,
    line: n && n.line || 1,
    column: n && n.column || 1,
    offset: n && n.offset || 0
  };
  const i = {}, o = [];
  let s = [], l = [];
  const a = {
    attempt: z(I),
    check: z(w),
    consume: N,
    enter: C,
    exit: v,
    interrupt: z(w, {
      interrupt: !0
    })
  }, c = {
    code: null,
    containerState: {},
    defineSkip: m,
    events: [],
    now: p,
    parser: t,
    previous: null,
    sliceSerialize: f,
    sliceStream: d,
    write: h
  };
  let u = e.tokenize.call(c, a);
  return e.resolveAll && o.push(e), c;
  function h(R) {
    return s = et(s, R), y(), s[s.length - 1] !== null ? [] : (W(e, 0), c.events = fa(o, c.events, c), c.events);
  }
  function f(R, D) {
    return p1(d(R), D);
  }
  function d(R) {
    return d1(s, R);
  }
  function p() {
    const {
      _bufferIndex: R,
      _index: D,
      line: J,
      column: te,
      offset: K
    } = r;
    return {
      _bufferIndex: R,
      _index: D,
      line: J,
      column: te,
      offset: K
    };
  }
  function m(R) {
    i[R.line] = R.column, x();
  }
  function y() {
    let R;
    for (; r._index < s.length; ) {
      const D = s[r._index];
      if (typeof D == "string")
        for (R = r._index, r._bufferIndex < 0 && (r._bufferIndex = 0); r._index === R && r._bufferIndex < D.length; )
          g(D.charCodeAt(r._bufferIndex));
      else
        g(D);
    }
  }
  function g(R) {
    u = u(R);
  }
  function N(R) {
    F(R) ? (r.line++, r.column = 1, r.offset += R === -3 ? 2 : 1, x()) : R !== -1 && (r.column++, r.offset++), r._bufferIndex < 0 ? r._index++ : (r._bufferIndex++, r._bufferIndex === // Points w/ non-negative `_bufferIndex` reference
    // strings.
    /** @type {string} */
    s[r._index].length && (r._bufferIndex = -1, r._index++)), c.previous = R;
  }
  function C(R, D) {
    const J = D || {};
    return J.type = R, J.start = p(), c.events.push(["enter", J, c]), l.push(J), J;
  }
  function v(R) {
    const D = l.pop();
    return D.end = p(), c.events.push(["exit", D, c]), D;
  }
  function I(R, D) {
    W(R, D.from);
  }
  function w(R, D) {
    D.restore();
  }
  function z(R, D) {
    return J;
    function J(te, K, ce) {
      let he, Oe, lt, k;
      return Array.isArray(te) ? (
        /* c8 ignore next 1 */
        ze(te)
      ) : "tokenize" in te ? (
        // Looks like a construct.
        ze([
          /** @type {Construct} */
          te
        ])
      ) : Pe(te);
      function Pe(fe) {
        return ie;
        function ie(ke) {
          const Ye = ke !== null && fe[ke], Rt = ke !== null && fe.null, Ui = [
            // To do: add more extension tests.
            /* c8 ignore next 2 */
            ...Array.isArray(Ye) ? Ye : Ye ? [Ye] : [],
            ...Array.isArray(Rt) ? Rt : Rt ? [Rt] : []
          ];
          return ze(Ui)(ke);
        }
      }
      function ze(fe) {
        return he = fe, Oe = 0, fe.length === 0 ? ce : b(fe[Oe]);
      }
      function b(fe) {
        return ie;
        function ie(ke) {
          return k = O(), lt = fe, fe.partial || (c.currentConstruct = fe), fe.name && c.parser.constructs.disable.null.includes(fe.name) ? Dt() : fe.tokenize.call(
            // If we do have fields, create an object w/ `context` as its
            // prototype.
            // This allows a “live binding”, which is needed for `interrupt`.
            D ? Object.assign(Object.create(c), D) : c,
            a,
            He,
            Dt
          )(ke);
        }
      }
      function He(fe) {
        return R(lt, k), K;
      }
      function Dt(fe) {
        return k.restore(), ++Oe < he.length ? b(he[Oe]) : ce;
      }
    }
  }
  function W(R, D) {
    R.resolveAll && !o.includes(R) && o.push(R), R.resolve && Mt(c.events, D, c.events.length - D, R.resolve(c.events.slice(D), c)), R.resolveTo && (c.events = R.resolveTo(c.events, c));
  }
  function O() {
    const R = p(), D = c.previous, J = c.currentConstruct, te = c.events.length, K = Array.from(l);
    return {
      from: te,
      restore: ce
    };
    function ce() {
      r = R, c.previous = D, c.currentConstruct = J, c.events.length = te, l = K, x();
    }
  }
  function x() {
    r.line in i && r.column < 2 && (r.column = i[r.line], r.offset += i[r.line] - 1);
  }
}
function d1(t, e) {
  const n = e.start._index, r = e.start._bufferIndex, i = e.end._index, o = e.end._bufferIndex;
  let s;
  if (n === i)
    s = [t[n].slice(r, o)];
  else {
    if (s = t.slice(n, i), r > -1) {
      const l = s[0];
      typeof l == "string" ? s[0] = l.slice(r) : s.shift();
    }
    o > 0 && s.push(t[i].slice(0, o));
  }
  return s;
}
function p1(t, e) {
  let n = -1;
  const r = [];
  let i;
  for (; ++n < t.length; ) {
    const o = t[n];
    let s;
    if (typeof o == "string")
      s = o;
    else switch (o) {
      case -5: {
        s = "\r";
        break;
      }
      case -4: {
        s = `
`;
        break;
      }
      case -3: {
        s = `\r
`;
        break;
      }
      case -2: {
        s = e ? " " : "	";
        break;
      }
      case -1: {
        if (!e && i) continue;
        s = " ";
        break;
      }
      default:
        s = String.fromCharCode(o);
    }
    i = o === -2, r.push(s);
  }
  return r.join("");
}
function m1(t) {
  const r = {
    constructs: (
      /** @type {FullNormalizedExtension} */
      Cy([h1, ...(t || {}).extensions || []])
    ),
    content: i(Oy),
    defined: [],
    document: i(Ry),
    flow: i(Yk),
    lazy: {},
    string: i(Zk),
    text: i(e1)
  };
  return r;
  function i(o) {
    return s;
    function s(l) {
      return f1(r, o, l);
    }
  }
}
function g1(t) {
  for (; !Bf(t); )
    ;
  return t;
}
const Su = /[\0\t\n\r]/g;
function y1() {
  let t = 1, e = "", n = !0, r;
  return i;
  function i(o, s, l) {
    const a = [];
    let c, u, h, f, d;
    for (o = e + (typeof o == "string" ? o.toString() : new TextDecoder(s || void 0).decode(o)), h = 0, e = "", n && (o.charCodeAt(0) === 65279 && h++, n = void 0); h < o.length; ) {
      if (Su.lastIndex = h, c = Su.exec(o), f = c && c.index !== void 0 ? c.index : o.length, d = o.charCodeAt(f), !c) {
        e = o.slice(h);
        break;
      }
      if (d === 10 && h === f && r)
        a.push(-3), r = void 0;
      else
        switch (r && (a.push(-5), r = void 0), h < f && (a.push(o.slice(h, f)), t += f - h), d) {
          case 0: {
            a.push(65533), t++;
            break;
          }
          case 9: {
            for (u = Math.ceil(t / 4) * 4, a.push(-2); t++ < u; ) a.push(-1);
            break;
          }
          case 10: {
            a.push(-4), t = 1;
            break;
          }
          default:
            r = !0, t = 1;
        }
      h = f + 1;
    }
    return l && (r && a.push(-5), e && a.push(e), a.push(null)), a;
  }
}
const k1 = /\\([!-/:-@[-`{-~])|&(#(?:\d{1,7}|x[\da-f]{1,6})|[\da-z]{1,31});/gi;
function b1(t) {
  return t.replace(k1, w1);
}
function w1(t, e, n) {
  if (e)
    return e;
  if (n.charCodeAt(0) === 35) {
    const i = n.charCodeAt(1), o = i === 120 || i === 88;
    return Pf(n.slice(o ? 2 : 1), o ? 16 : 10);
  }
  return ss(n) || t;
}
function mo(t) {
  return !t || typeof t != "object" ? "" : "position" in t || "type" in t ? Mu(t.position) : "start" in t || "end" in t ? Mu(t) : "line" in t || "column" in t ? Ml(t) : "";
}
function Ml(t) {
  return Nu(t && t.line) + ":" + Nu(t && t.column);
}
function Mu(t) {
  return Ml(t && t.start) + "-" + Ml(t && t.end);
}
function Nu(t) {
  return t && typeof t == "number" ? t : 1;
}
const jf = {}.hasOwnProperty;
function x1(t, e, n) {
  return typeof e != "string" && (n = e, e = void 0), C1(n)(g1(m1(n).document().write(y1()(t, e, !0))));
}
function C1(t) {
  const e = {
    transforms: [],
    canContainEols: ["emphasis", "fragment", "heading", "paragraph", "strong"],
    enter: {
      autolink: o(tu),
      autolinkProtocol: O,
      autolinkEmail: O,
      atxHeading: o(Xc),
      blockQuote: o(Rt),
      characterEscape: O,
      characterReference: O,
      codeFenced: o(Ui),
      codeFencedFenceInfo: s,
      codeFencedFenceMeta: s,
      codeIndented: o(Ui, s),
      codeText: o(wg, s),
      codeTextData: O,
      data: O,
      codeFlowValue: O,
      definition: o(xg),
      definitionDestinationString: s,
      definitionLabelString: s,
      definitionTitleString: s,
      emphasis: o(Cg),
      hardBreakEscape: o(Zc),
      hardBreakTrailing: o(Zc),
      htmlFlow: o(eu, s),
      htmlFlowData: O,
      htmlText: o(eu, s),
      htmlTextData: O,
      image: o(Sg),
      label: s,
      link: o(tu),
      listItem: o(Mg),
      listItemValue: f,
      listOrdered: o(nu, h),
      listUnordered: o(nu),
      paragraph: o(Ng),
      reference: b,
      referenceString: s,
      resourceDestinationString: s,
      resourceTitleString: s,
      setextHeading: o(Xc),
      strong: o(Tg),
      thematicBreak: o(Ag)
    },
    exit: {
      atxHeading: a(),
      atxHeadingSequence: I,
      autolink: a(),
      autolinkEmail: Ye,
      autolinkProtocol: ke,
      blockQuote: a(),
      characterEscapeValue: x,
      characterReferenceMarkerHexadecimal: Dt,
      characterReferenceMarkerNumeric: Dt,
      characterReferenceValue: fe,
      characterReference: ie,
      codeFenced: a(y),
      codeFencedFence: m,
      codeFencedFenceInfo: d,
      codeFencedFenceMeta: p,
      codeFlowValue: x,
      codeIndented: a(g),
      codeText: a(K),
      codeTextData: x,
      data: x,
      definition: a(),
      definitionDestinationString: v,
      definitionLabelString: N,
      definitionTitleString: C,
      emphasis: a(),
      hardBreakEscape: a(D),
      hardBreakTrailing: a(D),
      htmlFlow: a(J),
      htmlFlowData: x,
      htmlText: a(te),
      htmlTextData: x,
      image: a(he),
      label: lt,
      labelText: Oe,
      lineEnding: R,
      link: a(ce),
      listItem: a(),
      listOrdered: a(),
      listUnordered: a(),
      paragraph: a(),
      referenceString: He,
      resourceDestinationString: k,
      resourceTitleString: Pe,
      resource: ze,
      setextHeading: a(W),
      setextHeadingLineSequence: z,
      setextHeadingText: w,
      strong: a(),
      thematicBreak: a()
    }
  };
  qf(e, (t || {}).mdastExtensions || []);
  const n = {};
  return r;
  function r(M) {
    let A = {
      type: "root",
      children: []
    };
    const V = {
      stack: [A],
      tokenStack: [],
      config: e,
      enter: l,
      exit: c,
      buffer: s,
      resume: u,
      data: n
    }, U = [];
    let ne = -1;
    for (; ++ne < M.length; )
      if (M[ne][1].type === "listOrdered" || M[ne][1].type === "listUnordered")
        if (M[ne][0] === "enter")
          U.push(ne);
        else {
          const at = U.pop();
          ne = i(M, at, ne);
        }
    for (ne = -1; ++ne < M.length; ) {
      const at = e[M[ne][0]];
      jf.call(at, M[ne][1].type) && at[M[ne][1].type].call(Object.assign({
        sliceSerialize: M[ne][2].sliceSerialize
      }, V), M[ne][1]);
    }
    if (V.tokenStack.length > 0) {
      const at = V.tokenStack[V.tokenStack.length - 1];
      (at[1] || Tu).call(V, void 0, at[0]);
    }
    for (A.position = {
      start: qt(M.length > 0 ? M[0][1].start : {
        line: 1,
        column: 1,
        offset: 0
      }),
      end: qt(M.length > 0 ? M[M.length - 2][1].end : {
        line: 1,
        column: 1,
        offset: 0
      })
    }, ne = -1; ++ne < e.transforms.length; )
      A = e.transforms[ne](A) || A;
    return A;
  }
  function i(M, A, V) {
    let U = A - 1, ne = -1, at = !1, xn, vt, Pr, zr;
    for (; ++U <= V; ) {
      const je = M[U];
      switch (je[1].type) {
        case "listUnordered":
        case "listOrdered":
        case "blockQuote": {
          je[0] === "enter" ? ne++ : ne--, zr = void 0;
          break;
        }
        case "lineEndingBlank": {
          je[0] === "enter" && (xn && !zr && !ne && !Pr && (Pr = U), zr = void 0);
          break;
        }
        case "linePrefix":
        case "listItemValue":
        case "listItemMarker":
        case "listItemPrefix":
        case "listItemPrefixWhitespace":
          break;
        default:
          zr = void 0;
      }
      if (!ne && je[0] === "enter" && je[1].type === "listItemPrefix" || ne === -1 && je[0] === "exit" && (je[1].type === "listUnordered" || je[1].type === "listOrdered")) {
        if (xn) {
          let Yn = U;
          for (vt = void 0; Yn--; ) {
            const Pt = M[Yn];
            if (Pt[1].type === "lineEnding" || Pt[1].type === "lineEndingBlank") {
              if (Pt[0] === "exit") continue;
              vt && (M[vt][1].type = "lineEndingBlank", at = !0), Pt[1].type = "lineEnding", vt = Yn;
            } else if (!(Pt[1].type === "linePrefix" || Pt[1].type === "blockQuotePrefix" || Pt[1].type === "blockQuotePrefixWhitespace" || Pt[1].type === "blockQuoteMarker" || Pt[1].type === "listItemIndent")) break;
          }
          Pr && (!vt || Pr < vt) && (xn._spread = !0), xn.end = Object.assign({}, vt ? M[vt][1].start : je[1].end), M.splice(vt || U, 0, ["exit", xn, je[2]]), U++, V++;
        }
        if (je[1].type === "listItemPrefix") {
          const Yn = {
            type: "listItem",
            _spread: !1,
            start: Object.assign({}, je[1].start),
            // @ts-expect-error: we’ll add `end` in a second.
            end: void 0
          };
          xn = Yn, M.splice(U, 0, ["enter", Yn, je[2]]), U++, V++, Pr = void 0, zr = !0;
        }
      }
    }
    return M[A][1]._spread = at, V;
  }
  function o(M, A) {
    return V;
    function V(U) {
      l.call(this, M(U), U), A && A.call(this, U);
    }
  }
  function s() {
    this.stack.push({
      type: "fragment",
      children: []
    });
  }
  function l(M, A, V) {
    this.stack[this.stack.length - 1].children.push(M), this.stack.push(M), this.tokenStack.push([A, V || void 0]), M.position = {
      start: qt(A.start),
      // @ts-expect-error: `end` will be patched later.
      end: void 0
    };
  }
  function a(M) {
    return A;
    function A(V) {
      M && M.call(this, V), c.call(this, V);
    }
  }
  function c(M, A) {
    const V = this.stack.pop(), U = this.tokenStack.pop();
    if (U)
      U[0].type !== M.type && (A ? A.call(this, M, U[0]) : (U[1] || Tu).call(this, M, U[0]));
    else throw new Error("Cannot close `" + M.type + "` (" + mo({
      start: M.start,
      end: M.end
    }) + "): it’s not open");
    V.position.end = qt(M.end);
  }
  function u() {
    return wy(this.stack.pop());
  }
  function h() {
    this.data.expectingFirstListItemValue = !0;
  }
  function f(M) {
    if (this.data.expectingFirstListItemValue) {
      const A = this.stack[this.stack.length - 2];
      A.start = Number.parseInt(this.sliceSerialize(M), 10), this.data.expectingFirstListItemValue = void 0;
    }
  }
  function d() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.lang = M;
  }
  function p() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.meta = M;
  }
  function m() {
    this.data.flowCodeInside || (this.buffer(), this.data.flowCodeInside = !0);
  }
  function y() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.value = M.replace(/^(\r?\n|\r)|(\r?\n|\r)$/g, ""), this.data.flowCodeInside = void 0;
  }
  function g() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.value = M.replace(/(\r?\n|\r)$/g, "");
  }
  function N(M) {
    const A = this.resume(), V = this.stack[this.stack.length - 1];
    V.label = A, V.identifier = dr(this.sliceSerialize(M)).toLowerCase();
  }
  function C() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.title = M;
  }
  function v() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.url = M;
  }
  function I(M) {
    const A = this.stack[this.stack.length - 1];
    if (!A.depth) {
      const V = this.sliceSerialize(M).length;
      A.depth = V;
    }
  }
  function w() {
    this.data.setextHeadingSlurpLineEnding = !0;
  }
  function z(M) {
    const A = this.stack[this.stack.length - 1];
    A.depth = this.sliceSerialize(M).codePointAt(0) === 61 ? 1 : 2;
  }
  function W() {
    this.data.setextHeadingSlurpLineEnding = void 0;
  }
  function O(M) {
    const V = this.stack[this.stack.length - 1].children;
    let U = V[V.length - 1];
    (!U || U.type !== "text") && (U = Ig(), U.position = {
      start: qt(M.start),
      // @ts-expect-error: we’ll add `end` later.
      end: void 0
    }, V.push(U)), this.stack.push(U);
  }
  function x(M) {
    const A = this.stack.pop();
    A.value += this.sliceSerialize(M), A.position.end = qt(M.end);
  }
  function R(M) {
    const A = this.stack[this.stack.length - 1];
    if (this.data.atHardBreak) {
      const V = A.children[A.children.length - 1];
      V.position.end = qt(M.end), this.data.atHardBreak = void 0;
      return;
    }
    !this.data.setextHeadingSlurpLineEnding && e.canContainEols.includes(A.type) && (O.call(this, M), x.call(this, M));
  }
  function D() {
    this.data.atHardBreak = !0;
  }
  function J() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.value = M;
  }
  function te() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.value = M;
  }
  function K() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.value = M;
  }
  function ce() {
    const M = this.stack[this.stack.length - 1];
    if (this.data.inReference) {
      const A = this.data.referenceType || "shortcut";
      M.type += "Reference", M.referenceType = A, delete M.url, delete M.title;
    } else
      delete M.identifier, delete M.label;
    this.data.referenceType = void 0;
  }
  function he() {
    const M = this.stack[this.stack.length - 1];
    if (this.data.inReference) {
      const A = this.data.referenceType || "shortcut";
      M.type += "Reference", M.referenceType = A, delete M.url, delete M.title;
    } else
      delete M.identifier, delete M.label;
    this.data.referenceType = void 0;
  }
  function Oe(M) {
    const A = this.sliceSerialize(M), V = this.stack[this.stack.length - 2];
    V.label = b1(A), V.identifier = dr(A).toLowerCase();
  }
  function lt() {
    const M = this.stack[this.stack.length - 1], A = this.resume(), V = this.stack[this.stack.length - 1];
    if (this.data.inReference = !0, V.type === "link") {
      const U = M.children;
      V.children = U;
    } else
      V.alt = A;
  }
  function k() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.url = M;
  }
  function Pe() {
    const M = this.resume(), A = this.stack[this.stack.length - 1];
    A.title = M;
  }
  function ze() {
    this.data.inReference = void 0;
  }
  function b() {
    this.data.referenceType = "collapsed";
  }
  function He(M) {
    const A = this.resume(), V = this.stack[this.stack.length - 1];
    V.label = A, V.identifier = dr(this.sliceSerialize(M)).toLowerCase(), this.data.referenceType = "full";
  }
  function Dt(M) {
    this.data.characterReferenceType = M.type;
  }
  function fe(M) {
    const A = this.sliceSerialize(M), V = this.data.characterReferenceType;
    let U;
    V ? (U = Pf(A, V === "characterReferenceMarkerNumeric" ? 10 : 16), this.data.characterReferenceType = void 0) : U = ss(A);
    const ne = this.stack[this.stack.length - 1];
    ne.value += U;
  }
  function ie(M) {
    const A = this.stack.pop();
    A.position.end = qt(M.end);
  }
  function ke(M) {
    x.call(this, M);
    const A = this.stack[this.stack.length - 1];
    A.url = this.sliceSerialize(M);
  }
  function Ye(M) {
    x.call(this, M);
    const A = this.stack[this.stack.length - 1];
    A.url = "mailto:" + this.sliceSerialize(M);
  }
  function Rt() {
    return {
      type: "blockquote",
      children: []
    };
  }
  function Ui() {
    return {
      type: "code",
      lang: null,
      meta: null,
      value: ""
    };
  }
  function wg() {
    return {
      type: "inlineCode",
      value: ""
    };
  }
  function xg() {
    return {
      type: "definition",
      identifier: "",
      label: null,
      title: null,
      url: ""
    };
  }
  function Cg() {
    return {
      type: "emphasis",
      children: []
    };
  }
  function Xc() {
    return {
      type: "heading",
      // @ts-expect-error `depth` will be set later.
      depth: 0,
      children: []
    };
  }
  function Zc() {
    return {
      type: "break"
    };
  }
  function eu() {
    return {
      type: "html",
      value: ""
    };
  }
  function Sg() {
    return {
      type: "image",
      title: null,
      url: "",
      alt: null
    };
  }
  function tu() {
    return {
      type: "link",
      title: null,
      url: "",
      children: []
    };
  }
  function nu(M) {
    return {
      type: "list",
      ordered: M.type === "listOrdered",
      start: null,
      spread: M._spread,
      children: []
    };
  }
  function Mg(M) {
    return {
      type: "listItem",
      spread: M._spread,
      checked: null,
      children: []
    };
  }
  function Ng() {
    return {
      type: "paragraph",
      children: []
    };
  }
  function Tg() {
    return {
      type: "strong",
      children: []
    };
  }
  function Ig() {
    return {
      type: "text",
      value: ""
    };
  }
  function Ag() {
    return {
      type: "thematicBreak"
    };
  }
}
function qt(t) {
  return {
    line: t.line,
    column: t.column,
    offset: t.offset
  };
}
function qf(t, e) {
  let n = -1;
  for (; ++n < e.length; ) {
    const r = e[n];
    Array.isArray(r) ? qf(t, r) : S1(t, r);
  }
}
function S1(t, e) {
  let n;
  for (n in e)
    if (jf.call(e, n))
      switch (n) {
        case "canContainEols": {
          const r = e[n];
          r && t[n].push(...r);
          break;
        }
        case "transforms": {
          const r = e[n];
          r && t[n].push(...r);
          break;
        }
        case "enter":
        case "exit": {
          const r = e[n];
          r && Object.assign(t[n], r);
          break;
        }
      }
}
function Tu(t, e) {
  throw t ? new Error("Cannot close `" + t.type + "` (" + mo({
    start: t.start,
    end: t.end
  }) + "): a different token (`" + e.type + "`, " + mo({
    start: e.start,
    end: e.end
  }) + ") is open") : new Error("Cannot close document, a token (`" + e.type + "`, " + mo({
    start: e.start,
    end: e.end
  }) + ") is still open");
}
function Nl(t) {
  const e = this;
  e.parser = n;
  function n(r) {
    return x1(r, {
      ...e.data("settings"),
      ...t,
      // Note: these options are not in the readme.
      // The goal is for them to be set by plugins on `data` instead of being
      // passed by users.
      extensions: e.data("micromarkExtensions") || [],
      mdastExtensions: e.data("fromMarkdownExtensions") || []
    });
  }
}
const Iu = {}.hasOwnProperty;
function M1(t, e) {
  const n = e || {};
  function r(i, ...o) {
    let s = r.invalid;
    const l = r.handlers;
    if (i && Iu.call(i, t)) {
      const a = String(i[t]);
      s = Iu.call(l, a) ? l[a] : r.unknown;
    }
    if (s)
      return s.call(this, i, ...o);
  }
  return r.handlers = n.handlers || {}, r.invalid = n.invalid, r.unknown = n.unknown, r;
}
const N1 = {}.hasOwnProperty;
function Kf(t, e) {
  let n = -1, r;
  if (e.extensions)
    for (; ++n < e.extensions.length; )
      Kf(t, e.extensions[n]);
  for (r in e)
    if (N1.call(e, r))
      switch (r) {
        case "extensions":
          break;
        /* c8 ignore next 4 */
        case "unsafe": {
          Au(t[r], e[r]);
          break;
        }
        case "join": {
          Au(t[r], e[r]);
          break;
        }
        case "handlers": {
          T1(t[r], e[r]);
          break;
        }
        default:
          t.options[r] = e[r];
      }
  return t;
}
function Au(t, e) {
  e && t.push(...e);
}
function T1(t, e) {
  e && Object.assign(t, e);
}
function I1(t, e, n, r) {
  const i = n.enter("blockquote"), o = n.createTracker(r);
  o.move("> "), o.shift(2);
  const s = n.indentLines(
    n.containerFlow(t, o.current()),
    A1
  );
  return i(), s;
}
function A1(t, e, n) {
  return ">" + (n ? "" : " ") + t;
}
function Jf(t, e) {
  return Eu(t, e.inConstruct, !0) && !Eu(t, e.notInConstruct, !1);
}
function Eu(t, e, n) {
  if (typeof e == "string" && (e = [e]), !e || e.length === 0)
    return n;
  let r = -1;
  for (; ++r < e.length; )
    if (t.includes(e[r]))
      return !0;
  return !1;
}
function Ou(t, e, n, r) {
  let i = -1;
  for (; ++i < n.unsafe.length; )
    if (n.unsafe[i].character === `
` && Jf(n.stack, n.unsafe[i]))
      return /[ \t]/.test(r.before) ? "" : " ";
  return `\\
`;
}
function E1(t, e) {
  const n = String(t);
  let r = n.indexOf(e), i = r, o = 0, s = 0;
  if (typeof e != "string")
    throw new TypeError("Expected substring");
  for (; r !== -1; )
    r === i ? ++o > s && (s = o) : o = 1, i = r + e.length, r = n.indexOf(e, i);
  return s;
}
function Tl(t, e) {
  return !!(e.options.fences === !1 && t.value && // If there’s no info…
  !t.lang && // And there’s a non-whitespace character…
  /[^ \r\n]/.test(t.value) && // And the value doesn’t start or end in a blank…
  !/^[\t ]*(?:[\r\n]|$)|(?:^|[\r\n])[\t ]*$/.test(t.value));
}
function O1(t) {
  const e = t.options.fence || "`";
  if (e !== "`" && e !== "~")
    throw new Error(
      "Cannot serialize code with `" + e + "` for `options.fence`, expected `` ` `` or `~`"
    );
  return e;
}
function D1(t, e, n, r) {
  const i = O1(n), o = t.value || "", s = i === "`" ? "GraveAccent" : "Tilde";
  if (Tl(t, n)) {
    const h = n.enter("codeIndented"), f = n.indentLines(o, R1);
    return h(), f;
  }
  const l = n.createTracker(r), a = i.repeat(Math.max(E1(o, i) + 1, 3)), c = n.enter("codeFenced");
  let u = l.move(a);
  if (t.lang) {
    const h = n.enter(`codeFencedLang${s}`);
    u += l.move(
      n.safe(t.lang, {
        before: u,
        after: " ",
        encode: ["`"],
        ...l.current()
      })
    ), h();
  }
  if (t.lang && t.meta) {
    const h = n.enter(`codeFencedMeta${s}`);
    u += l.move(" "), u += l.move(
      n.safe(t.meta, {
        before: u,
        after: `
`,
        encode: ["`"],
        ...l.current()
      })
    ), h();
  }
  return u += l.move(`
`), o && (u += l.move(o + `
`)), u += l.move(a), c(), u;
}
function R1(t, e, n) {
  return (n ? "" : "    ") + t;
}
function pa(t) {
  const e = t.options.quote || '"';
  if (e !== '"' && e !== "'")
    throw new Error(
      "Cannot serialize title with `" + e + "` for `options.quote`, expected `\"`, or `'`"
    );
  return e;
}
function v1(t, e, n, r) {
  const i = pa(n), o = i === '"' ? "Quote" : "Apostrophe", s = n.enter("definition");
  let l = n.enter("label");
  const a = n.createTracker(r);
  let c = a.move("[");
  return c += a.move(
    n.safe(n.associationId(t), {
      before: c,
      after: "]",
      ...a.current()
    })
  ), c += a.move("]: "), l(), // If there’s no url, or…
  !t.url || // If there are control characters or whitespace.
  /[\0- \u007F]/.test(t.url) ? (l = n.enter("destinationLiteral"), c += a.move("<"), c += a.move(
    n.safe(t.url, { before: c, after: ">", ...a.current() })
  ), c += a.move(">")) : (l = n.enter("destinationRaw"), c += a.move(
    n.safe(t.url, {
      before: c,
      after: t.title ? " " : `
`,
      ...a.current()
    })
  )), l(), t.title && (l = n.enter(`title${o}`), c += a.move(" " + i), c += a.move(
    n.safe(t.title, {
      before: c,
      after: i,
      ...a.current()
    })
  ), c += a.move(i), l()), s(), c;
}
function P1(t) {
  const e = t.options.emphasis || "*";
  if (e !== "*" && e !== "_")
    throw new Error(
      "Cannot serialize emphasis with `" + e + "` for `options.emphasis`, expected `*`, or `_`"
    );
  return e;
}
function hn(t) {
  return "&#x" + t.toString(16).toUpperCase() + ";";
}
function z1(t) {
  return t !== null && (t < 0 || t === 32);
}
const L1 = Uf(new RegExp("\\p{P}|\\p{S}", "u")), F1 = Uf(/\s/);
function Uf(t) {
  return e;
  function e(n) {
    return n !== null && n > -1 && t.test(String.fromCharCode(n));
  }
}
function Du(t) {
  if (t === null || z1(t) || F1(t))
    return 1;
  if (L1(t))
    return 2;
}
function Ko(t, e, n) {
  const r = Du(t), i = Du(e);
  return r === void 0 ? i === void 0 ? (
    // Letter inside:
    // we have to encode *both* letters for `_` as it is looser.
    // it already forms for `*` (and GFMs `~`).
    n === "_" ? { inside: !0, outside: !0 } : { inside: !1, outside: !1 }
  ) : i === 1 ? (
    // Whitespace inside: encode both (letter, whitespace).
    { inside: !0, outside: !0 }
  ) : (
    // Punctuation inside: encode outer (letter)
    { inside: !1, outside: !0 }
  ) : r === 1 ? i === void 0 ? (
    // Letter inside: already forms.
    { inside: !1, outside: !1 }
  ) : i === 1 ? (
    // Whitespace inside: encode both (whitespace).
    { inside: !0, outside: !0 }
  ) : (
    // Punctuation inside: already forms.
    { inside: !1, outside: !1 }
  ) : i === void 0 ? (
    // Letter inside: already forms.
    { inside: !1, outside: !1 }
  ) : i === 1 ? (
    // Whitespace inside: encode inner (whitespace).
    { inside: !0, outside: !1 }
  ) : (
    // Punctuation inside: already forms.
    { inside: !1, outside: !1 }
  );
}
Gf.peek = B1;
function Gf(t, e, n, r) {
  const i = P1(n), o = n.enter("emphasis"), s = n.createTracker(r), l = s.move(i);
  let a = s.move(
    n.containerPhrasing(t, {
      after: i,
      before: l,
      ...s.current()
    })
  );
  const c = a.charCodeAt(0), u = Ko(
    r.before.charCodeAt(r.before.length - 1),
    c,
    i
  );
  u.inside && (a = hn(c) + a.slice(1));
  const h = a.charCodeAt(a.length - 1), f = Ko(r.after.charCodeAt(0), h, i);
  f.inside && (a = a.slice(0, -1) + hn(h));
  const d = s.move(i);
  return o(), n.attentionEncodeSurroundingInfo = {
    after: f.outside,
    before: u.outside
  }, l + a + d;
}
function B1(t, e, n) {
  return n.options.emphasis || "*";
}
const as = (
  // Note: overloads in JSDoc can’t yet use different `@template`s.
  /**
   * @type {(
   *   (<Condition extends string>(test: Condition) => (node: unknown, index?: number | null | undefined, parent?: Parent | null | undefined, context?: unknown) => node is Node & {type: Condition}) &
   *   (<Condition extends Props>(test: Condition) => (node: unknown, index?: number | null | undefined, parent?: Parent | null | undefined, context?: unknown) => node is Node & Condition) &
   *   (<Condition extends TestFunction>(test: Condition) => (node: unknown, index?: number | null | undefined, parent?: Parent | null | undefined, context?: unknown) => node is Node & Predicate<Condition, Node>) &
   *   ((test?: null | undefined) => (node?: unknown, index?: number | null | undefined, parent?: Parent | null | undefined, context?: unknown) => node is Node) &
   *   ((test?: Test) => Check)
   * )}
   */
  /**
   * @param {Test} [test]
   * @returns {Check}
   */
  function(t) {
    if (t == null)
      return W1;
    if (typeof t == "function")
      return cs(t);
    if (typeof t == "object")
      return Array.isArray(t) ? _1(t) : $1(t);
    if (typeof t == "string")
      return V1(t);
    throw new Error("Expected function, string, or object as test");
  }
);
function _1(t) {
  const e = [];
  let n = -1;
  for (; ++n < t.length; )
    e[n] = as(t[n]);
  return cs(r);
  function r(...i) {
    let o = -1;
    for (; ++o < e.length; )
      if (e[o].apply(this, i)) return !0;
    return !1;
  }
}
function $1(t) {
  const e = (
    /** @type {Record<string, unknown>} */
    t
  );
  return cs(n);
  function n(r) {
    const i = (
      /** @type {Record<string, unknown>} */
      /** @type {unknown} */
      r
    );
    let o;
    for (o in t)
      if (i[o] !== e[o]) return !1;
    return !0;
  }
}
function V1(t) {
  return cs(e);
  function e(n) {
    return n && n.type === t;
  }
}
function cs(t) {
  return e;
  function e(n, r, i) {
    return !!(H1(n) && t.call(
      this,
      n,
      typeof r == "number" ? r : void 0,
      i || void 0
    ));
  }
}
function W1() {
  return !0;
}
function H1(t) {
  return t !== null && typeof t == "object" && "type" in t;
}
const Yf = [], j1 = !0, Il = !1, Al = "skip";
function Qf(t, e, n, r) {
  let i;
  typeof e == "function" && typeof n != "function" ? (r = n, n = e) : i = e;
  const o = as(i), s = r ? -1 : 1;
  l(t, void 0, [])();
  function l(a, c, u) {
    const h = (
      /** @type {Record<string, unknown>} */
      a && typeof a == "object" ? a : {}
    );
    if (typeof h.type == "string") {
      const d = (
        // `hast`
        typeof h.tagName == "string" ? h.tagName : (
          // `xast`
          typeof h.name == "string" ? h.name : void 0
        )
      );
      Object.defineProperty(f, "name", {
        value: "node (" + (a.type + (d ? "<" + d + ">" : "")) + ")"
      });
    }
    return f;
    function f() {
      let d = Yf, p, m, y;
      if ((!e || o(a, c, u[u.length - 1] || void 0)) && (d = q1(n(a, u)), d[0] === Il))
        return d;
      if ("children" in a && a.children) {
        const g = (
          /** @type {UnistParent} */
          a
        );
        if (g.children && d[0] !== Al)
          for (m = (r ? g.children.length : -1) + s, y = u.concat(g); m > -1 && m < g.children.length; ) {
            const N = g.children[m];
            if (p = l(N, m, y)(), p[0] === Il)
              return p;
            m = typeof p[1] == "number" ? p[1] : m + s;
          }
      }
      return d;
    }
  }
}
function q1(t) {
  return Array.isArray(t) ? t : typeof t == "number" ? [j1, t] : t == null ? Yf : [t];
}
function Jn(t, e, n, r) {
  let i, o, s;
  typeof e == "function" && typeof n != "function" ? (o = void 0, s = e, i = n) : (o = e, s = n, i = r), Qf(t, o, l, i);
  function l(a, c) {
    const u = c[c.length - 1], h = u ? u.children.indexOf(a) : void 0;
    return s(a, h, u);
  }
}
const K1 = {};
function Xf(t, e) {
  const n = K1, r = typeof n.includeImageAlt == "boolean" ? n.includeImageAlt : !0, i = typeof n.includeHtml == "boolean" ? n.includeHtml : !0;
  return Zf(t, r, i);
}
function Zf(t, e, n) {
  if (J1(t)) {
    if ("value" in t)
      return t.type === "html" && !n ? "" : t.value;
    if (e && "alt" in t && t.alt)
      return t.alt;
    if ("children" in t)
      return Ru(t.children, e, n);
  }
  return Array.isArray(t) ? Ru(t, e, n) : "";
}
function Ru(t, e, n) {
  const r = [];
  let i = -1;
  for (; ++i < t.length; )
    r[i] = Zf(t[i], e, n);
  return r.join("");
}
function J1(t) {
  return !!(t && typeof t == "object");
}
function ed(t, e) {
  let n = !1;
  return Jn(t, function(r) {
    if ("value" in r && /\r?\n|\r/.test(r.value) || r.type === "break")
      return n = !0, Il;
  }), !!((!t.depth || t.depth < 3) && Xf(t) && (e.options.setext || n));
}
function U1(t, e, n, r) {
  const i = Math.max(Math.min(6, t.depth || 1), 1), o = n.createTracker(r);
  if (ed(t, n)) {
    const u = n.enter("headingSetext"), h = n.enter("phrasing"), f = n.containerPhrasing(t, {
      ...o.current(),
      before: `
`,
      after: `
`
    });
    return h(), u(), f + `
` + (i === 1 ? "=" : "-").repeat(
      // The whole size…
      f.length - // Minus the position of the character after the last EOL (or
      // 0 if there is none)…
      (Math.max(f.lastIndexOf("\r"), f.lastIndexOf(`
`)) + 1)
    );
  }
  const s = "#".repeat(i), l = n.enter("headingAtx"), a = n.enter("phrasing");
  o.move(s + " ");
  let c = n.containerPhrasing(t, {
    before: "# ",
    after: `
`,
    ...o.current()
  });
  return /^[\t ]/.test(c) && (c = hn(c.charCodeAt(0)) + c.slice(1)), c = c ? s + " " + c : s, n.options.closeAtx && (c += " " + s), a(), l(), c;
}
td.peek = G1;
function td(t) {
  return t.value || "";
}
function G1() {
  return "<";
}
nd.peek = Y1;
function nd(t, e, n, r) {
  const i = pa(n), o = i === '"' ? "Quote" : "Apostrophe", s = n.enter("image");
  let l = n.enter("label");
  const a = n.createTracker(r);
  let c = a.move("![");
  return c += a.move(
    n.safe(t.alt, { before: c, after: "]", ...a.current() })
  ), c += a.move("]("), l(), // If there’s no url but there is a title…
  !t.url && t.title || // If there are control characters or whitespace.
  /[\0- \u007F]/.test(t.url) ? (l = n.enter("destinationLiteral"), c += a.move("<"), c += a.move(
    n.safe(t.url, { before: c, after: ">", ...a.current() })
  ), c += a.move(">")) : (l = n.enter("destinationRaw"), c += a.move(
    n.safe(t.url, {
      before: c,
      after: t.title ? " " : ")",
      ...a.current()
    })
  )), l(), t.title && (l = n.enter(`title${o}`), c += a.move(" " + i), c += a.move(
    n.safe(t.title, {
      before: c,
      after: i,
      ...a.current()
    })
  ), c += a.move(i), l()), c += a.move(")"), s(), c;
}
function Y1() {
  return "!";
}
rd.peek = Q1;
function rd(t, e, n, r) {
  const i = t.referenceType, o = n.enter("imageReference");
  let s = n.enter("label");
  const l = n.createTracker(r);
  let a = l.move("![");
  const c = n.safe(t.alt, {
    before: a,
    after: "]",
    ...l.current()
  });
  a += l.move(c + "]["), s();
  const u = n.stack;
  n.stack = [], s = n.enter("reference");
  const h = n.safe(n.associationId(t), {
    before: a,
    after: "]",
    ...l.current()
  });
  return s(), n.stack = u, o(), i === "full" || !c || c !== h ? a += l.move(h + "]") : i === "shortcut" ? a = a.slice(0, -1) : a += l.move("]"), a;
}
function Q1() {
  return "!";
}
id.peek = X1;
function id(t, e, n) {
  let r = t.value || "", i = "`", o = -1;
  for (; new RegExp("(^|[^`])" + i + "([^`]|$)").test(r); )
    i += "`";
  for (/[^ \r\n]/.test(r) && (/^[ \r\n]/.test(r) && /[ \r\n]$/.test(r) || /^`|`$/.test(r)) && (r = " " + r + " "); ++o < n.unsafe.length; ) {
    const s = n.unsafe[o], l = n.compilePattern(s);
    let a;
    if (s.atBreak)
      for (; a = l.exec(r); ) {
        let c = a.index;
        r.charCodeAt(c) === 10 && r.charCodeAt(c - 1) === 13 && c--, r = r.slice(0, c) + " " + r.slice(a.index + 1);
      }
  }
  return i + r + i;
}
function X1() {
  return "`";
}
function od(t, e) {
  const n = Xf(t);
  return !!(!e.options.resourceLink && // If there’s a url…
  t.url && // And there’s a no title…
  !t.title && // And the content of `node` is a single text node…
  t.children && t.children.length === 1 && t.children[0].type === "text" && // And if the url is the same as the content…
  (n === t.url || "mailto:" + n === t.url) && // And that starts w/ a protocol…
  /^[a-z][a-z+.-]+:/i.test(t.url) && // And that doesn’t contain ASCII control codes (character escapes and
  // references don’t work), space, or angle brackets…
  !/[\0- <>\u007F]/.test(t.url));
}
sd.peek = Z1;
function sd(t, e, n, r) {
  const i = pa(n), o = i === '"' ? "Quote" : "Apostrophe", s = n.createTracker(r);
  let l, a;
  if (od(t, n)) {
    const u = n.stack;
    n.stack = [], l = n.enter("autolink");
    let h = s.move("<");
    return h += s.move(
      n.containerPhrasing(t, {
        before: h,
        after: ">",
        ...s.current()
      })
    ), h += s.move(">"), l(), n.stack = u, h;
  }
  l = n.enter("link"), a = n.enter("label");
  let c = s.move("[");
  return c += s.move(
    n.containerPhrasing(t, {
      before: c,
      after: "](",
      ...s.current()
    })
  ), c += s.move("]("), a(), // If there’s no url but there is a title…
  !t.url && t.title || // If there are control characters or whitespace.
  /[\0- \u007F]/.test(t.url) ? (a = n.enter("destinationLiteral"), c += s.move("<"), c += s.move(
    n.safe(t.url, { before: c, after: ">", ...s.current() })
  ), c += s.move(">")) : (a = n.enter("destinationRaw"), c += s.move(
    n.safe(t.url, {
      before: c,
      after: t.title ? " " : ")",
      ...s.current()
    })
  )), a(), t.title && (a = n.enter(`title${o}`), c += s.move(" " + i), c += s.move(
    n.safe(t.title, {
      before: c,
      after: i,
      ...s.current()
    })
  ), c += s.move(i), a()), c += s.move(")"), l(), c;
}
function Z1(t, e, n) {
  return od(t, n) ? "<" : "[";
}
ld.peek = eb;
function ld(t, e, n, r) {
  const i = t.referenceType, o = n.enter("linkReference");
  let s = n.enter("label");
  const l = n.createTracker(r);
  let a = l.move("[");
  const c = n.containerPhrasing(t, {
    before: a,
    after: "]",
    ...l.current()
  });
  a += l.move(c + "]["), s();
  const u = n.stack;
  n.stack = [], s = n.enter("reference");
  const h = n.safe(n.associationId(t), {
    before: a,
    after: "]",
    ...l.current()
  });
  return s(), n.stack = u, o(), i === "full" || !c || c !== h ? a += l.move(h + "]") : i === "shortcut" ? a = a.slice(0, -1) : a += l.move("]"), a;
}
function eb() {
  return "[";
}
function ma(t) {
  const e = t.options.bullet || "*";
  if (e !== "*" && e !== "+" && e !== "-")
    throw new Error(
      "Cannot serialize items with `" + e + "` for `options.bullet`, expected `*`, `+`, or `-`"
    );
  return e;
}
function tb(t) {
  const e = ma(t), n = t.options.bulletOther;
  if (!n)
    return e === "*" ? "-" : "*";
  if (n !== "*" && n !== "+" && n !== "-")
    throw new Error(
      "Cannot serialize items with `" + n + "` for `options.bulletOther`, expected `*`, `+`, or `-`"
    );
  if (n === e)
    throw new Error(
      "Expected `bullet` (`" + e + "`) and `bulletOther` (`" + n + "`) to be different"
    );
  return n;
}
function nb(t) {
  const e = t.options.bulletOrdered || ".";
  if (e !== "." && e !== ")")
    throw new Error(
      "Cannot serialize items with `" + e + "` for `options.bulletOrdered`, expected `.` or `)`"
    );
  return e;
}
function ad(t) {
  const e = t.options.rule || "*";
  if (e !== "*" && e !== "-" && e !== "_")
    throw new Error(
      "Cannot serialize rules with `" + e + "` for `options.rule`, expected `*`, `-`, or `_`"
    );
  return e;
}
function rb(t, e, n, r) {
  const i = n.enter("list"), o = n.bulletCurrent;
  let s = t.ordered ? nb(n) : ma(n);
  const l = t.ordered ? s === "." ? ")" : "." : tb(n);
  let a = e && n.bulletLastUsed ? s === n.bulletLastUsed : !1;
  if (!t.ordered) {
    const u = t.children ? t.children[0] : void 0;
    if (
      // Bullet could be used as a thematic break marker:
      (s === "*" || s === "-") && // Empty first list item:
      u && (!u.children || !u.children[0]) && // Directly in two other list items:
      n.stack[n.stack.length - 1] === "list" && n.stack[n.stack.length - 2] === "listItem" && n.stack[n.stack.length - 3] === "list" && n.stack[n.stack.length - 4] === "listItem" && // That are each the first child.
      n.indexStack[n.indexStack.length - 1] === 0 && n.indexStack[n.indexStack.length - 2] === 0 && n.indexStack[n.indexStack.length - 3] === 0 && (a = !0), ad(n) === s && u
    ) {
      let h = -1;
      for (; ++h < t.children.length; ) {
        const f = t.children[h];
        if (f && f.type === "listItem" && f.children && f.children[0] && f.children[0].type === "thematicBreak") {
          a = !0;
          break;
        }
      }
    }
  }
  a && (s = l), n.bulletCurrent = s;
  const c = n.containerFlow(t, r);
  return n.bulletLastUsed = s, n.bulletCurrent = o, i(), c;
}
function ib(t) {
  const e = t.options.listItemIndent || "one";
  if (e !== "tab" && e !== "one" && e !== "mixed")
    throw new Error(
      "Cannot serialize items with `" + e + "` for `options.listItemIndent`, expected `tab`, `one`, or `mixed`"
    );
  return e;
}
function ob(t, e, n, r) {
  const i = ib(n);
  let o = n.bulletCurrent || ma(n);
  e && e.type === "list" && e.ordered && (o = (typeof e.start == "number" && e.start > -1 ? e.start : 1) + (n.options.incrementListMarker === !1 ? 0 : e.children.indexOf(t)) + o);
  let s = o.length + 1;
  (i === "tab" || i === "mixed" && (e && e.type === "list" && e.spread || t.spread)) && (s = Math.ceil(s / 4) * 4);
  const l = n.createTracker(r);
  l.move(o + " ".repeat(s - o.length)), l.shift(s);
  const a = n.enter("listItem"), c = n.indentLines(
    n.containerFlow(t, l.current()),
    u
  );
  return a(), c;
  function u(h, f, d) {
    return f ? (d ? "" : " ".repeat(s)) + h : (d ? o : o + " ".repeat(s - o.length)) + h;
  }
}
function sb(t, e, n, r) {
  const i = n.enter("paragraph"), o = n.enter("phrasing"), s = n.containerPhrasing(t, r);
  return o(), i(), s;
}
const lb = (
  /** @type {(node?: unknown) => node is Exclude<PhrasingContent, Html>} */
  as([
    "break",
    "delete",
    "emphasis",
    // To do: next major: removed since footnotes were added to GFM.
    "footnote",
    "footnoteReference",
    "image",
    "imageReference",
    "inlineCode",
    // Enabled by `mdast-util-math`:
    "inlineMath",
    "link",
    "linkReference",
    // Enabled by `mdast-util-mdx`:
    "mdxJsxTextElement",
    // Enabled by `mdast-util-mdx`:
    "mdxTextExpression",
    "strong",
    "text",
    // Enabled by `mdast-util-directive`:
    "textDirective"
  ])
);
function ab(t, e, n, r) {
  return (t.children.some(function(s) {
    return lb(s);
  }) ? n.containerPhrasing : n.containerFlow).call(n, t, r);
}
function cb(t) {
  const e = t.options.strong || "*";
  if (e !== "*" && e !== "_")
    throw new Error(
      "Cannot serialize strong with `" + e + "` for `options.strong`, expected `*`, or `_`"
    );
  return e;
}
cd.peek = ub;
function cd(t, e, n, r) {
  const i = cb(n), o = n.enter("strong"), s = n.createTracker(r), l = s.move(i + i);
  let a = s.move(
    n.containerPhrasing(t, {
      after: i,
      before: l,
      ...s.current()
    })
  );
  const c = a.charCodeAt(0), u = Ko(
    r.before.charCodeAt(r.before.length - 1),
    c,
    i
  );
  u.inside && (a = hn(c) + a.slice(1));
  const h = a.charCodeAt(a.length - 1), f = Ko(r.after.charCodeAt(0), h, i);
  f.inside && (a = a.slice(0, -1) + hn(h));
  const d = s.move(i + i);
  return o(), n.attentionEncodeSurroundingInfo = {
    after: f.outside,
    before: u.outside
  }, l + a + d;
}
function ub(t, e, n) {
  return n.options.strong || "*";
}
function hb(t, e, n, r) {
  return n.safe(t.value, r);
}
function fb(t) {
  const e = t.options.ruleRepetition || 3;
  if (e < 3)
    throw new Error(
      "Cannot serialize rules with repetition `" + e + "` for `options.ruleRepetition`, expected `3` or more"
    );
  return e;
}
function db(t, e, n) {
  const r = (ad(n) + (n.options.ruleSpaces ? " " : "")).repeat(fb(n));
  return n.options.ruleSpaces ? r.slice(0, -1) : r;
}
const ga = {
  blockquote: I1,
  break: Ou,
  code: D1,
  definition: v1,
  emphasis: Gf,
  hardBreak: Ou,
  heading: U1,
  html: td,
  image: nd,
  imageReference: rd,
  inlineCode: id,
  link: sd,
  linkReference: ld,
  list: rb,
  listItem: ob,
  paragraph: sb,
  root: ab,
  strong: cd,
  text: hb,
  thematicBreak: db
}, pb = [mb];
function mb(t, e, n, r) {
  if (e.type === "code" && Tl(e, r) && (t.type === "list" || t.type === e.type && Tl(t, r)))
    return !1;
  if ("spread" in n && typeof n.spread == "boolean")
    return t.type === "paragraph" && // Two paragraphs.
    (t.type === e.type || e.type === "definition" || // Paragraph followed by a setext heading.
    e.type === "heading" && ed(e, r)) ? void 0 : n.spread ? 1 : 0;
}
const Cn = [
  "autolink",
  "destinationLiteral",
  "destinationRaw",
  "reference",
  "titleQuote",
  "titleApostrophe"
], gb = [
  { character: "	", after: "[\\r\\n]", inConstruct: "phrasing" },
  { character: "	", before: "[\\r\\n]", inConstruct: "phrasing" },
  {
    character: "	",
    inConstruct: ["codeFencedLangGraveAccent", "codeFencedLangTilde"]
  },
  {
    character: "\r",
    inConstruct: [
      "codeFencedLangGraveAccent",
      "codeFencedLangTilde",
      "codeFencedMetaGraveAccent",
      "codeFencedMetaTilde",
      "destinationLiteral",
      "headingAtx"
    ]
  },
  {
    character: `
`,
    inConstruct: [
      "codeFencedLangGraveAccent",
      "codeFencedLangTilde",
      "codeFencedMetaGraveAccent",
      "codeFencedMetaTilde",
      "destinationLiteral",
      "headingAtx"
    ]
  },
  { character: " ", after: "[\\r\\n]", inConstruct: "phrasing" },
  { character: " ", before: "[\\r\\n]", inConstruct: "phrasing" },
  {
    character: " ",
    inConstruct: ["codeFencedLangGraveAccent", "codeFencedLangTilde"]
  },
  // An exclamation mark can start an image, if it is followed by a link or
  // a link reference.
  {
    character: "!",
    after: "\\[",
    inConstruct: "phrasing",
    notInConstruct: Cn
  },
  // A quote can break out of a title.
  { character: '"', inConstruct: "titleQuote" },
  // A number sign could start an ATX heading if it starts a line.
  { atBreak: !0, character: "#" },
  { character: "#", inConstruct: "headingAtx", after: `(?:[\r
]|$)` },
  // Dollar sign and percentage are not used in markdown.
  // An ampersand could start a character reference.
  { character: "&", after: "[#A-Za-z]", inConstruct: "phrasing" },
  // An apostrophe can break out of a title.
  { character: "'", inConstruct: "titleApostrophe" },
  // A left paren could break out of a destination raw.
  { character: "(", inConstruct: "destinationRaw" },
  // A left paren followed by `]` could make something into a link or image.
  {
    before: "\\]",
    character: "(",
    inConstruct: "phrasing",
    notInConstruct: Cn
  },
  // A right paren could start a list item or break out of a destination
  // raw.
  { atBreak: !0, before: "\\d+", character: ")" },
  { character: ")", inConstruct: "destinationRaw" },
  // An asterisk can start thematic breaks, list items, emphasis, strong.
  { atBreak: !0, character: "*", after: `(?:[ 	\r
*])` },
  { character: "*", inConstruct: "phrasing", notInConstruct: Cn },
  // A plus sign could start a list item.
  { atBreak: !0, character: "+", after: `(?:[ 	\r
])` },
  // A dash can start thematic breaks, list items, and setext heading
  // underlines.
  { atBreak: !0, character: "-", after: `(?:[ 	\r
-])` },
  // A dot could start a list item.
  { atBreak: !0, before: "\\d+", character: ".", after: `(?:[ 	\r
]|$)` },
  // Slash, colon, and semicolon are not used in markdown for constructs.
  // A less than can start html (flow or text) or an autolink.
  // HTML could start with an exclamation mark (declaration, cdata, comment),
  // slash (closing tag), question mark (instruction), or a letter (tag).
  // An autolink also starts with a letter.
  // Finally, it could break out of a destination literal.
  { atBreak: !0, character: "<", after: "[!/?A-Za-z]" },
  {
    character: "<",
    after: "[!/?A-Za-z]",
    inConstruct: "phrasing",
    notInConstruct: Cn
  },
  { character: "<", inConstruct: "destinationLiteral" },
  // An equals to can start setext heading underlines.
  { atBreak: !0, character: "=" },
  // A greater than can start block quotes and it can break out of a
  // destination literal.
  { atBreak: !0, character: ">" },
  { character: ">", inConstruct: "destinationLiteral" },
  // Question mark and at sign are not used in markdown for constructs.
  // A left bracket can start definitions, references, labels,
  { atBreak: !0, character: "[" },
  { character: "[", inConstruct: "phrasing", notInConstruct: Cn },
  { character: "[", inConstruct: ["label", "reference"] },
  // A backslash can start an escape (when followed by punctuation) or a
  // hard break (when followed by an eol).
  // Note: typical escapes are handled in `safe`!
  { character: "\\", after: "[\\r\\n]", inConstruct: "phrasing" },
  // A right bracket can exit labels.
  { character: "]", inConstruct: ["label", "reference"] },
  // Caret is not used in markdown for constructs.
  // An underscore can start emphasis, strong, or a thematic break.
  { atBreak: !0, character: "_" },
  { character: "_", inConstruct: "phrasing", notInConstruct: Cn },
  // A grave accent can start code (fenced or text), or it can break out of
  // a grave accent code fence.
  { atBreak: !0, character: "`" },
  {
    character: "`",
    inConstruct: ["codeFencedLangGraveAccent", "codeFencedMetaGraveAccent"]
  },
  { character: "`", inConstruct: "phrasing", notInConstruct: Cn },
  // Left brace, vertical bar, right brace are not used in markdown for
  // constructs.
  // A tilde can start code (fenced).
  { atBreak: !0, character: "~" }
];
function yb(t, e) {
  const n = Number.parseInt(t, e);
  return (
    // C0 except for HT, LF, FF, CR, space.
    n < 9 || n === 11 || n > 13 && n < 32 || // Control character (DEL) of C0, and C1 controls.
    n > 126 && n < 160 || // Lone high surrogates and low surrogates.
    n > 55295 && n < 57344 || // Noncharacters.
    n > 64975 && n < 65008 || /* eslint-disable no-bitwise */
    (n & 65535) === 65535 || (n & 65535) === 65534 || /* eslint-enable no-bitwise */
    // Out of range
    n > 1114111 ? "�" : String.fromCodePoint(n)
  );
}
const kb = /\\([!-/:-@[-`{-~])|&(#(?:\d{1,7}|x[\da-f]{1,6})|[\da-z]{1,31});/gi;
function bb(t) {
  return t.replace(kb, wb);
}
function wb(t, e, n) {
  if (e)
    return e;
  if (n.charCodeAt(0) === 35) {
    const i = n.charCodeAt(1), o = i === 120 || i === 88;
    return yb(n.slice(o ? 2 : 1), o ? 16 : 10);
  }
  return ss(n) || t;
}
function xb(t) {
  return t.label || !t.identifier ? t.label || "" : bb(t.identifier);
}
function Cb(t) {
  if (!t._compiled) {
    const e = (t.atBreak ? "[\\r\\n][\\t ]*" : "") + (t.before ? "(?:" + t.before + ")" : "");
    t._compiled = new RegExp(
      (e ? "(" + e + ")" : "") + (/[|\\{}()[\]^$+*?.-]/.test(t.character) ? "\\" : "") + t.character + (t.after ? "(?:" + t.after + ")" : ""),
      "g"
    );
  }
  return t._compiled;
}
function Sb(t, e, n) {
  const r = e.indexStack, i = t.children || [], o = [];
  let s = -1, l = n.before, a;
  r.push(-1);
  let c = e.createTracker(n);
  for (; ++s < i.length; ) {
    const u = i[s];
    let h;
    if (r[r.length - 1] = s, s + 1 < i.length) {
      let p = e.handle.handlers[i[s + 1].type];
      p && p.peek && (p = p.peek), h = p ? p(i[s + 1], t, e, {
        before: "",
        after: "",
        ...c.current()
      }).charAt(0) : "";
    } else
      h = n.after;
    o.length > 0 && (l === "\r" || l === `
`) && u.type === "html" && (o[o.length - 1] = o[o.length - 1].replace(
      /(\r?\n|\r)$/,
      " "
    ), l = " ", c = e.createTracker(n), c.move(o.join("")));
    let f = e.handle(u, t, e, {
      ...c.current(),
      after: h,
      before: l
    });
    a && a === f.slice(0, 1) && (f = hn(a.charCodeAt(0)) + f.slice(1));
    const d = e.attentionEncodeSurroundingInfo;
    e.attentionEncodeSurroundingInfo = void 0, a = void 0, d && (o.length > 0 && d.before && l === o[o.length - 1].slice(-1) && (o[o.length - 1] = o[o.length - 1].slice(0, -1) + hn(l.charCodeAt(0))), d.after && (a = h)), c.move(f), o.push(f), l = f.slice(-1);
  }
  return r.pop(), o.join("");
}
function Mb(t, e, n) {
  const r = e.indexStack, i = t.children || [], o = e.createTracker(n), s = [];
  let l = -1;
  for (r.push(-1); ++l < i.length; ) {
    const a = i[l];
    r[r.length - 1] = l, s.push(
      o.move(
        e.handle(a, t, e, {
          before: `
`,
          after: `
`,
          ...o.current()
        })
      )
    ), a.type !== "list" && (e.bulletLastUsed = void 0), l < i.length - 1 && s.push(
      o.move(Nb(a, i[l + 1], t, e))
    );
  }
  return r.pop(), s.join("");
}
function Nb(t, e, n, r) {
  let i = r.join.length;
  for (; i--; ) {
    const o = r.join[i](t, e, n, r);
    if (o === !0 || o === 1)
      break;
    if (typeof o == "number")
      return `
`.repeat(1 + o);
    if (o === !1)
      return `

<!---->

`;
  }
  return `

`;
}
const Tb = /\r?\n|\r/g;
function Ib(t, e) {
  const n = [];
  let r = 0, i = 0, o;
  for (; o = Tb.exec(t); )
    s(t.slice(r, o.index)), n.push(o[0]), r = o.index + o[0].length, i++;
  return s(t.slice(r)), n.join("");
  function s(l) {
    n.push(e(l, i, !l));
  }
}
function Ab(t, e, n) {
  const r = (n.before || "") + (e || "") + (n.after || ""), i = [], o = [], s = {};
  let l = -1;
  for (; ++l < t.unsafe.length; ) {
    const u = t.unsafe[l];
    if (!Jf(t.stack, u))
      continue;
    const h = t.compilePattern(u);
    let f;
    for (; f = h.exec(r); ) {
      const d = "before" in u || !!u.atBreak, p = "after" in u, m = f.index + (d ? f[1].length : 0);
      i.includes(m) ? (s[m].before && !d && (s[m].before = !1), s[m].after && !p && (s[m].after = !1)) : (i.push(m), s[m] = { before: d, after: p });
    }
  }
  i.sort(Eb);
  let a = n.before ? n.before.length : 0;
  const c = r.length - (n.after ? n.after.length : 0);
  for (l = -1; ++l < i.length; ) {
    const u = i[l];
    u < a || u >= c || u + 1 < c && i[l + 1] === u + 1 && s[u].after && !s[u + 1].before && !s[u + 1].after || i[l - 1] === u - 1 && s[u].before && !s[u - 1].before && !s[u - 1].after || (a !== u && o.push(vu(r.slice(a, u), "\\")), a = u, /[!-/:-@[-`{-~]/.test(r.charAt(u)) && (!n.encode || !n.encode.includes(r.charAt(u))) ? o.push("\\") : (o.push(hn(r.charCodeAt(u))), a++));
  }
  return o.push(vu(r.slice(a, c), n.after)), o.join("");
}
function Eb(t, e) {
  return t - e;
}
function vu(t, e) {
  const n = /\\(?=[!-/:-@[-`{-~])/g, r = [], i = [], o = t + e;
  let s = -1, l = 0, a;
  for (; a = n.exec(o); )
    r.push(a.index);
  for (; ++s < r.length; )
    l !== r[s] && i.push(t.slice(l, r[s])), i.push("\\"), l = r[s];
  return i.push(t.slice(l)), i.join("");
}
function Ob(t) {
  const e = t || {}, n = e.now || {};
  let r = e.lineShift || 0, i = n.line || 1, o = n.column || 1;
  return { move: a, current: s, shift: l };
  function s() {
    return { now: { line: i, column: o }, lineShift: r };
  }
  function l(c) {
    r += c;
  }
  function a(c) {
    const u = c || "", h = u.split(/\r?\n|\r/g), f = h[h.length - 1];
    return i += h.length - 1, o = h.length === 1 ? o + f.length : 1 + f.length + r, u;
  }
}
function Db(t, e) {
  const n = e || {}, r = {
    associationId: xb,
    containerPhrasing: zb,
    containerFlow: Lb,
    createTracker: Ob,
    compilePattern: Cb,
    enter: o,
    // @ts-expect-error: GFM / frontmatter are typed in `mdast` but not defined
    // here.
    handlers: { ...ga },
    // @ts-expect-error: add `handle` in a second.
    handle: void 0,
    indentLines: Ib,
    indexStack: [],
    join: [...pb],
    options: {},
    safe: Fb,
    stack: [],
    unsafe: [...gb]
  };
  Kf(r, n), r.options.tightDefinitions && r.join.push(Pb), r.handle = M1("type", {
    invalid: Rb,
    unknown: vb,
    handlers: r.handlers
  });
  let i = r.handle(t, void 0, r, {
    before: `
`,
    after: `
`,
    now: { line: 1, column: 1 },
    lineShift: 0
  });
  return i && i.charCodeAt(i.length - 1) !== 10 && i.charCodeAt(i.length - 1) !== 13 && (i += `
`), i;
  function o(s) {
    return r.stack.push(s), l;
    function l() {
      r.stack.pop();
    }
  }
}
function Rb(t) {
  throw new Error("Cannot handle value `" + t + "`, expected node");
}
function vb(t) {
  const e = (
    /** @type {Nodes} */
    t
  );
  throw new Error("Cannot handle unknown node `" + e.type + "`");
}
function Pb(t, e) {
  if (t.type === "definition" && t.type === e.type)
    return 0;
}
function zb(t, e) {
  return Sb(t, this, e);
}
function Lb(t, e) {
  return Mb(t, this, e);
}
function Fb(t, e) {
  return Ab(this, t, e);
}
function El(t) {
  const e = this;
  e.compiler = n;
  function n(r) {
    return Db(r, {
      ...e.data("settings"),
      ...t,
      // Note: this option is not in the readme.
      // The goal is for it to be set by plugins on `data` instead of being
      // passed by users.
      extensions: e.data("toMarkdownExtensions") || []
    });
  }
}
function Pu(t) {
  if (t)
    throw t;
}
var Qi = typeof globalThis < "u" ? globalThis : typeof window < "u" ? window : typeof global < "u" ? global : typeof self < "u" ? self : {};
function ud(t) {
  return t && t.__esModule && Object.prototype.hasOwnProperty.call(t, "default") ? t.default : t;
}
var Ds, zu;
function Bb() {
  if (zu) return Ds;
  zu = 1;
  var t = Object.prototype.hasOwnProperty, e = Object.prototype.toString, n = Object.defineProperty, r = Object.getOwnPropertyDescriptor, i = function(c) {
    return typeof Array.isArray == "function" ? Array.isArray(c) : e.call(c) === "[object Array]";
  }, o = function(c) {
    if (!c || e.call(c) !== "[object Object]")
      return !1;
    var u = t.call(c, "constructor"), h = c.constructor && c.constructor.prototype && t.call(c.constructor.prototype, "isPrototypeOf");
    if (c.constructor && !u && !h)
      return !1;
    var f;
    for (f in c)
      ;
    return typeof f > "u" || t.call(c, f);
  }, s = function(c, u) {
    n && u.name === "__proto__" ? n(c, u.name, {
      enumerable: !0,
      configurable: !0,
      value: u.newValue,
      writable: !0
    }) : c[u.name] = u.newValue;
  }, l = function(c, u) {
    if (u === "__proto__")
      if (t.call(c, u)) {
        if (r)
          return r(c, u).value;
      } else return;
    return c[u];
  };
  return Ds = function a() {
    var c, u, h, f, d, p, m = arguments[0], y = 1, g = arguments.length, N = !1;
    for (typeof m == "boolean" && (N = m, m = arguments[1] || {}, y = 2), (m == null || typeof m != "object" && typeof m != "function") && (m = {}); y < g; ++y)
      if (c = arguments[y], c != null)
        for (u in c)
          h = l(m, u), f = l(c, u), m !== f && (N && f && (o(f) || (d = i(f))) ? (d ? (d = !1, p = h && i(h) ? h : []) : p = h && o(h) ? h : {}, s(m, { name: u, newValue: a(N, p, f) })) : typeof f < "u" && s(m, { name: u, newValue: f }));
    return m;
  }, Ds;
}
var _b = Bb();
const Rs = /* @__PURE__ */ ud(_b);
function _T() {
}
function Ol(t) {
  if (typeof t != "object" || t === null)
    return !1;
  const e = Object.getPrototypeOf(t);
  return (e === null || e === Object.prototype || Object.getPrototypeOf(e) === null) && !(Symbol.toStringTag in t) && !(Symbol.iterator in t);
}
function $b() {
  const t = [], e = { run: n, use: r };
  return e;
  function n(...i) {
    let o = -1;
    const s = i.pop();
    if (typeof s != "function")
      throw new TypeError("Expected function as last argument, not " + s);
    l(null, ...i);
    function l(a, ...c) {
      const u = t[++o];
      let h = -1;
      if (a) {
        s(a);
        return;
      }
      for (; ++h < i.length; )
        (c[h] === null || c[h] === void 0) && (c[h] = i[h]);
      i = c, u ? Vb(u, l)(...c) : s(null, ...c);
    }
  }
  function r(i) {
    if (typeof i != "function")
      throw new TypeError(
        "Expected `middelware` to be a function, not " + i
      );
    return t.push(i), e;
  }
}
function Vb(t, e) {
  let n;
  return r;
  function r(...s) {
    const l = t.length > s.length;
    let a;
    l && s.push(i);
    try {
      a = t.apply(this, s);
    } catch (c) {
      const u = (
        /** @type {Error} */
        c
      );
      if (l && n)
        throw u;
      return i(u);
    }
    l || (a && a.then && typeof a.then == "function" ? a.then(o, i) : a instanceof Error ? i(a) : o(a));
  }
  function i(s, ...l) {
    n || (n = !0, e(s, ...l));
  }
  function o(s) {
    i(null, s);
  }
}
function Wb(t) {
  return !t || typeof t != "object" ? "" : "position" in t || "type" in t ? Lu(t.position) : "start" in t || "end" in t ? Lu(t) : "line" in t || "column" in t ? Dl(t) : "";
}
function Dl(t) {
  return Fu(t && t.line) + ":" + Fu(t && t.column);
}
function Lu(t) {
  return Dl(t && t.start) + "-" + Dl(t && t.end);
}
function Fu(t) {
  return t && typeof t == "number" ? t : 1;
}
class $e extends Error {
  /**
   * Create a message for `reason`.
   *
   * > 🪦 **Note**: also has obsolete signatures.
   *
   * @overload
   * @param {string} reason
   * @param {Options | null | undefined} [options]
   * @returns
   *
   * @overload
   * @param {string} reason
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @overload
   * @param {string} reason
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @overload
   * @param {string} reason
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {string | null | undefined} [origin]
   * @returns
   *
   * @param {Error | VFileMessage | string} causeOrReason
   *   Reason for message, should use markdown.
   * @param {Node | NodeLike | Options | Point | Position | string | null | undefined} [optionsOrParentOrPlace]
   *   Configuration (optional).
   * @param {string | null | undefined} [origin]
   *   Place in code where the message originates (example:
   *   `'my-package:my-rule'` or `'my-rule'`).
   * @returns
   *   Instance of `VFileMessage`.
   */
  // eslint-disable-next-line complexity
  constructor(e, n, r) {
    super(), typeof n == "string" && (r = n, n = void 0);
    let i = "", o = {}, s = !1;
    if (n && ("line" in n && "column" in n ? o = { place: n } : "start" in n && "end" in n ? o = { place: n } : "type" in n ? o = {
      ancestors: [n],
      place: n.position
    } : o = { ...n }), typeof e == "string" ? i = e : !o.cause && e && (s = !0, i = e.message, o.cause = e), !o.ruleId && !o.source && typeof r == "string") {
      const a = r.indexOf(":");
      a === -1 ? o.ruleId = r : (o.source = r.slice(0, a), o.ruleId = r.slice(a + 1));
    }
    if (!o.place && o.ancestors && o.ancestors) {
      const a = o.ancestors[o.ancestors.length - 1];
      a && (o.place = a.position);
    }
    const l = o.place && "start" in o.place ? o.place.start : o.place;
    this.ancestors = o.ancestors || void 0, this.cause = o.cause || void 0, this.column = l ? l.column : void 0, this.fatal = void 0, this.file, this.message = i, this.line = l ? l.line : void 0, this.name = Wb(o.place) || "1:1", this.place = o.place || void 0, this.reason = this.message, this.ruleId = o.ruleId || void 0, this.source = o.source || void 0, this.stack = s && o.cause && typeof o.cause.stack == "string" ? o.cause.stack : "", this.actual, this.expected, this.note, this.url;
  }
}
$e.prototype.file = "";
$e.prototype.name = "";
$e.prototype.reason = "";
$e.prototype.message = "";
$e.prototype.stack = "";
$e.prototype.column = void 0;
$e.prototype.line = void 0;
$e.prototype.ancestors = void 0;
$e.prototype.cause = void 0;
$e.prototype.fatal = void 0;
$e.prototype.place = void 0;
$e.prototype.ruleId = void 0;
$e.prototype.source = void 0;
const wt = { basename: Hb, dirname: jb, extname: qb, join: Kb, sep: "/" };
function Hb(t, e) {
  if (e !== void 0 && typeof e != "string")
    throw new TypeError('"ext" argument must be a string');
  Di(t);
  let n = 0, r = -1, i = t.length, o;
  if (e === void 0 || e.length === 0 || e.length > t.length) {
    for (; i--; )
      if (t.codePointAt(i) === 47) {
        if (o) {
          n = i + 1;
          break;
        }
      } else r < 0 && (o = !0, r = i + 1);
    return r < 0 ? "" : t.slice(n, r);
  }
  if (e === t)
    return "";
  let s = -1, l = e.length - 1;
  for (; i--; )
    if (t.codePointAt(i) === 47) {
      if (o) {
        n = i + 1;
        break;
      }
    } else
      s < 0 && (o = !0, s = i + 1), l > -1 && (t.codePointAt(i) === e.codePointAt(l--) ? l < 0 && (r = i) : (l = -1, r = s));
  return n === r ? r = s : r < 0 && (r = t.length), t.slice(n, r);
}
function jb(t) {
  if (Di(t), t.length === 0)
    return ".";
  let e = -1, n = t.length, r;
  for (; --n; )
    if (t.codePointAt(n) === 47) {
      if (r) {
        e = n;
        break;
      }
    } else r || (r = !0);
  return e < 0 ? t.codePointAt(0) === 47 ? "/" : "." : e === 1 && t.codePointAt(0) === 47 ? "//" : t.slice(0, e);
}
function qb(t) {
  Di(t);
  let e = t.length, n = -1, r = 0, i = -1, o = 0, s;
  for (; e--; ) {
    const l = t.codePointAt(e);
    if (l === 47) {
      if (s) {
        r = e + 1;
        break;
      }
      continue;
    }
    n < 0 && (s = !0, n = e + 1), l === 46 ? i < 0 ? i = e : o !== 1 && (o = 1) : i > -1 && (o = -1);
  }
  return i < 0 || n < 0 || // We saw a non-dot character immediately before the dot.
  o === 0 || // The (right-most) trimmed path component is exactly `..`.
  o === 1 && i === n - 1 && i === r + 1 ? "" : t.slice(i, n);
}
function Kb(...t) {
  let e = -1, n;
  for (; ++e < t.length; )
    Di(t[e]), t[e] && (n = n === void 0 ? t[e] : n + "/" + t[e]);
  return n === void 0 ? "." : Jb(n);
}
function Jb(t) {
  Di(t);
  const e = t.codePointAt(0) === 47;
  let n = Ub(t, !e);
  return n.length === 0 && !e && (n = "."), n.length > 0 && t.codePointAt(t.length - 1) === 47 && (n += "/"), e ? "/" + n : n;
}
function Ub(t, e) {
  let n = "", r = 0, i = -1, o = 0, s = -1, l, a;
  for (; ++s <= t.length; ) {
    if (s < t.length)
      l = t.codePointAt(s);
    else {
      if (l === 47)
        break;
      l = 47;
    }
    if (l === 47) {
      if (!(i === s - 1 || o === 1)) if (i !== s - 1 && o === 2) {
        if (n.length < 2 || r !== 2 || n.codePointAt(n.length - 1) !== 46 || n.codePointAt(n.length - 2) !== 46) {
          if (n.length > 2) {
            if (a = n.lastIndexOf("/"), a !== n.length - 1) {
              a < 0 ? (n = "", r = 0) : (n = n.slice(0, a), r = n.length - 1 - n.lastIndexOf("/")), i = s, o = 0;
              continue;
            }
          } else if (n.length > 0) {
            n = "", r = 0, i = s, o = 0;
            continue;
          }
        }
        e && (n = n.length > 0 ? n + "/.." : "..", r = 2);
      } else
        n.length > 0 ? n += "/" + t.slice(i + 1, s) : n = t.slice(i + 1, s), r = s - i - 1;
      i = s, o = 0;
    } else l === 46 && o > -1 ? o++ : o = -1;
  }
  return n;
}
function Di(t) {
  if (typeof t != "string")
    throw new TypeError(
      "Path must be a string. Received " + JSON.stringify(t)
    );
}
const Gb = { cwd: Yb };
function Yb() {
  return "/";
}
function Rl(t) {
  return !!(t !== null && typeof t == "object" && "href" in t && t.href && "protocol" in t && t.protocol && // @ts-expect-error: indexing is fine.
  t.auth === void 0);
}
function Qb(t) {
  if (typeof t == "string")
    t = new URL(t);
  else if (!Rl(t)) {
    const e = new TypeError(
      'The "path" argument must be of type string or an instance of URL. Received `' + t + "`"
    );
    throw e.code = "ERR_INVALID_ARG_TYPE", e;
  }
  if (t.protocol !== "file:") {
    const e = new TypeError("The URL must be of scheme file");
    throw e.code = "ERR_INVALID_URL_SCHEME", e;
  }
  return Xb(t);
}
function Xb(t) {
  if (t.hostname !== "") {
    const r = new TypeError(
      'File URL host must be "localhost" or empty on darwin'
    );
    throw r.code = "ERR_INVALID_FILE_URL_HOST", r;
  }
  const e = t.pathname;
  let n = -1;
  for (; ++n < e.length; )
    if (e.codePointAt(n) === 37 && e.codePointAt(n + 1) === 50) {
      const r = e.codePointAt(n + 2);
      if (r === 70 || r === 102) {
        const i = new TypeError(
          "File URL path must not include encoded / characters"
        );
        throw i.code = "ERR_INVALID_FILE_URL_PATH", i;
      }
    }
  return decodeURIComponent(e);
}
const vs = (
  /** @type {const} */
  [
    "history",
    "path",
    "basename",
    "stem",
    "extname",
    "dirname"
  ]
);
class Zb {
  /**
   * Create a new virtual file.
   *
   * `options` is treated as:
   *
   * *   `string` or `Uint8Array` — `{value: options}`
   * *   `URL` — `{path: options}`
   * *   `VFile` — shallow copies its data over to the new file
   * *   `object` — all fields are shallow copied over to the new file
   *
   * Path related fields are set in the following order (least specific to
   * most specific): `history`, `path`, `basename`, `stem`, `extname`,
   * `dirname`.
   *
   * You cannot set `dirname` or `extname` without setting either `history`,
   * `path`, `basename`, or `stem` too.
   *
   * @param {Compatible | null | undefined} [value]
   *   File value.
   * @returns
   *   New instance.
   */
  constructor(e) {
    let n;
    e ? Rl(e) ? n = { path: e } : typeof e == "string" || e0(e) ? n = { value: e } : n = e : n = {}, this.cwd = "cwd" in n ? "" : Gb.cwd(), this.data = {}, this.history = [], this.messages = [], this.value, this.map, this.result, this.stored;
    let r = -1;
    for (; ++r < vs.length; ) {
      const o = vs[r];
      o in n && n[o] !== void 0 && n[o] !== null && (this[o] = o === "history" ? [...n[o]] : n[o]);
    }
    let i;
    for (i in n)
      vs.includes(i) || (this[i] = n[i]);
  }
  /**
   * Get the basename (including extname) (example: `'index.min.js'`).
   *
   * @returns {string | undefined}
   *   Basename.
   */
  get basename() {
    return typeof this.path == "string" ? wt.basename(this.path) : void 0;
  }
  /**
   * Set basename (including extname) (`'index.min.js'`).
   *
   * Cannot contain path separators (`'/'` on unix, macOS, and browsers, `'\'`
   * on windows).
   * Cannot be nullified (use `file.path = file.dirname` instead).
   *
   * @param {string} basename
   *   Basename.
   * @returns {undefined}
   *   Nothing.
   */
  set basename(e) {
    zs(e, "basename"), Ps(e, "basename"), this.path = wt.join(this.dirname || "", e);
  }
  /**
   * Get the parent path (example: `'~'`).
   *
   * @returns {string | undefined}
   *   Dirname.
   */
  get dirname() {
    return typeof this.path == "string" ? wt.dirname(this.path) : void 0;
  }
  /**
   * Set the parent path (example: `'~'`).
   *
   * Cannot be set if there’s no `path` yet.
   *
   * @param {string | undefined} dirname
   *   Dirname.
   * @returns {undefined}
   *   Nothing.
   */
  set dirname(e) {
    Bu(this.basename, "dirname"), this.path = wt.join(e || "", this.basename);
  }
  /**
   * Get the extname (including dot) (example: `'.js'`).
   *
   * @returns {string | undefined}
   *   Extname.
   */
  get extname() {
    return typeof this.path == "string" ? wt.extname(this.path) : void 0;
  }
  /**
   * Set the extname (including dot) (example: `'.js'`).
   *
   * Cannot contain path separators (`'/'` on unix, macOS, and browsers, `'\'`
   * on windows).
   * Cannot be set if there’s no `path` yet.
   *
   * @param {string | undefined} extname
   *   Extname.
   * @returns {undefined}
   *   Nothing.
   */
  set extname(e) {
    if (Ps(e, "extname"), Bu(this.dirname, "extname"), e) {
      if (e.codePointAt(0) !== 46)
        throw new Error("`extname` must start with `.`");
      if (e.includes(".", 1))
        throw new Error("`extname` cannot contain multiple dots");
    }
    this.path = wt.join(this.dirname, this.stem + (e || ""));
  }
  /**
   * Get the full path (example: `'~/index.min.js'`).
   *
   * @returns {string}
   *   Path.
   */
  get path() {
    return this.history[this.history.length - 1];
  }
  /**
   * Set the full path (example: `'~/index.min.js'`).
   *
   * Cannot be nullified.
   * You can set a file URL (a `URL` object with a `file:` protocol) which will
   * be turned into a path with `url.fileURLToPath`.
   *
   * @param {URL | string} path
   *   Path.
   * @returns {undefined}
   *   Nothing.
   */
  set path(e) {
    Rl(e) && (e = Qb(e)), zs(e, "path"), this.path !== e && this.history.push(e);
  }
  /**
   * Get the stem (basename w/o extname) (example: `'index.min'`).
   *
   * @returns {string | undefined}
   *   Stem.
   */
  get stem() {
    return typeof this.path == "string" ? wt.basename(this.path, this.extname) : void 0;
  }
  /**
   * Set the stem (basename w/o extname) (example: `'index.min'`).
   *
   * Cannot contain path separators (`'/'` on unix, macOS, and browsers, `'\'`
   * on windows).
   * Cannot be nullified (use `file.path = file.dirname` instead).
   *
   * @param {string} stem
   *   Stem.
   * @returns {undefined}
   *   Nothing.
   */
  set stem(e) {
    zs(e, "stem"), Ps(e, "stem"), this.path = wt.join(this.dirname || "", e + (this.extname || ""));
  }
  // Normal prototypal methods.
  /**
   * Create a fatal message for `reason` associated with the file.
   *
   * The `fatal` field of the message is set to `true` (error; file not usable)
   * and the `file` field is set to the current file path.
   * The message is added to the `messages` field on `file`.
   *
   * > 🪦 **Note**: also has obsolete signatures.
   *
   * @overload
   * @param {string} reason
   * @param {MessageOptions | null | undefined} [options]
   * @returns {never}
   *
   * @overload
   * @param {string} reason
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @overload
   * @param {string} reason
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @overload
   * @param {string} reason
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {string | null | undefined} [origin]
   * @returns {never}
   *
   * @param {Error | VFileMessage | string} causeOrReason
   *   Reason for message, should use markdown.
   * @param {Node | NodeLike | MessageOptions | Point | Position | string | null | undefined} [optionsOrParentOrPlace]
   *   Configuration (optional).
   * @param {string | null | undefined} [origin]
   *   Place in code where the message originates (example:
   *   `'my-package:my-rule'` or `'my-rule'`).
   * @returns {never}
   *   Never.
   * @throws {VFileMessage}
   *   Message.
   */
  fail(e, n, r) {
    const i = this.message(e, n, r);
    throw i.fatal = !0, i;
  }
  /**
   * Create an info message for `reason` associated with the file.
   *
   * The `fatal` field of the message is set to `undefined` (info; change
   * likely not needed) and the `file` field is set to the current file path.
   * The message is added to the `messages` field on `file`.
   *
   * > 🪦 **Note**: also has obsolete signatures.
   *
   * @overload
   * @param {string} reason
   * @param {MessageOptions | null | undefined} [options]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @param {Error | VFileMessage | string} causeOrReason
   *   Reason for message, should use markdown.
   * @param {Node | NodeLike | MessageOptions | Point | Position | string | null | undefined} [optionsOrParentOrPlace]
   *   Configuration (optional).
   * @param {string | null | undefined} [origin]
   *   Place in code where the message originates (example:
   *   `'my-package:my-rule'` or `'my-rule'`).
   * @returns {VFileMessage}
   *   Message.
   */
  info(e, n, r) {
    const i = this.message(e, n, r);
    return i.fatal = void 0, i;
  }
  /**
   * Create a message for `reason` associated with the file.
   *
   * The `fatal` field of the message is set to `false` (warning; change may be
   * needed) and the `file` field is set to the current file path.
   * The message is added to the `messages` field on `file`.
   *
   * > 🪦 **Note**: also has obsolete signatures.
   *
   * @overload
   * @param {string} reason
   * @param {MessageOptions | null | undefined} [options]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {string} reason
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Node | NodeLike | null | undefined} parent
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {Point | Position | null | undefined} place
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @overload
   * @param {Error | VFileMessage} cause
   * @param {string | null | undefined} [origin]
   * @returns {VFileMessage}
   *
   * @param {Error | VFileMessage | string} causeOrReason
   *   Reason for message, should use markdown.
   * @param {Node | NodeLike | MessageOptions | Point | Position | string | null | undefined} [optionsOrParentOrPlace]
   *   Configuration (optional).
   * @param {string | null | undefined} [origin]
   *   Place in code where the message originates (example:
   *   `'my-package:my-rule'` or `'my-rule'`).
   * @returns {VFileMessage}
   *   Message.
   */
  message(e, n, r) {
    const i = new $e(
      // @ts-expect-error: the overloads are fine.
      e,
      n,
      r
    );
    return this.path && (i.name = this.path + ":" + i.name, i.file = this.path), i.fatal = !1, this.messages.push(i), i;
  }
  /**
   * Serialize the file.
   *
   * > **Note**: which encodings are supported depends on the engine.
   * > For info on Node.js, see:
   * > <https://nodejs.org/api/util.html#whatwg-supported-encodings>.
   *
   * @param {string | null | undefined} [encoding='utf8']
   *   Character encoding to understand `value` as when it’s a `Uint8Array`
   *   (default: `'utf-8'`).
   * @returns {string}
   *   Serialized file.
   */
  toString(e) {
    return this.value === void 0 ? "" : typeof this.value == "string" ? this.value : new TextDecoder(e || void 0).decode(this.value);
  }
}
function Ps(t, e) {
  if (t && t.includes(wt.sep))
    throw new Error(
      "`" + e + "` cannot be a path: did not expect `" + wt.sep + "`"
    );
}
function zs(t, e) {
  if (!t)
    throw new Error("`" + e + "` cannot be empty");
}
function Bu(t, e) {
  if (!t)
    throw new Error("Setting `" + e + "` requires `path` to be set too");
}
function e0(t) {
  return !!(t && typeof t == "object" && "byteLength" in t && "byteOffset" in t);
}
const t0 = (
  /**
   * @type {new <Parameters extends Array<unknown>, Result>(property: string | symbol) => (...parameters: Parameters) => Result}
   */
  /** @type {unknown} */
  /**
   * @this {Function}
   * @param {string | symbol} property
   * @returns {(...parameters: Array<unknown>) => unknown}
   */
  function(t) {
    const r = (
      /** @type {Record<string | symbol, Function>} */
      // Prototypes do exist.
      // type-coverage:ignore-next-line
      this.constructor.prototype
    ), i = r[t], o = function() {
      return i.apply(o, arguments);
    };
    return Object.setPrototypeOf(o, r), o;
  }
), n0 = {}.hasOwnProperty;
class ya extends t0 {
  /**
   * Create a processor.
   */
  constructor() {
    super("copy"), this.Compiler = void 0, this.Parser = void 0, this.attachers = [], this.compiler = void 0, this.freezeIndex = -1, this.frozen = void 0, this.namespace = {}, this.parser = void 0, this.transformers = $b();
  }
  /**
   * Copy a processor.
   *
   * @deprecated
   *   This is a private internal method and should not be used.
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *   New *unfrozen* processor ({@linkcode Processor}) that is
   *   configured to work the same as its ancestor.
   *   When the descendant processor is configured in the future it does not
   *   affect the ancestral processor.
   */
  copy() {
    const e = (
      /** @type {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>} */
      new ya()
    );
    let n = -1;
    for (; ++n < this.attachers.length; ) {
      const r = this.attachers[n];
      e.use(...r);
    }
    return e.data(Rs(!0, {}, this.namespace)), e;
  }
  /**
   * Configure the processor with info available to all plugins.
   * Information is stored in an object.
   *
   * Typically, options can be given to a specific plugin, but sometimes it
   * makes sense to have information shared with several plugins.
   * For example, a list of HTML elements that are self-closing, which is
   * needed during all phases.
   *
   * > **Note**: setting information cannot occur on *frozen* processors.
   * > Call the processor first to create a new unfrozen processor.
   *
   * > **Note**: to register custom data in TypeScript, augment the
   * > {@linkcode Data} interface.
   *
   * @example
   *   This example show how to get and set info:
   *
   *   ```js
   *   import {unified} from 'unified'
   *
   *   const processor = unified().data('alpha', 'bravo')
   *
   *   processor.data('alpha') // => 'bravo'
   *
   *   processor.data() // => {alpha: 'bravo'}
   *
   *   processor.data({charlie: 'delta'})
   *
   *   processor.data() // => {charlie: 'delta'}
   *   ```
   *
   * @template {keyof Data} Key
   *
   * @overload
   * @returns {Data}
   *
   * @overload
   * @param {Data} dataset
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *
   * @overload
   * @param {Key} key
   * @returns {Data[Key]}
   *
   * @overload
   * @param {Key} key
   * @param {Data[Key]} value
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *
   * @param {Data | Key} [key]
   *   Key to get or set, or entire dataset to set, or nothing to get the
   *   entire dataset (optional).
   * @param {Data[Key]} [value]
   *   Value to set (optional).
   * @returns {unknown}
   *   The current processor when setting, the value at `key` when getting, or
   *   the entire dataset when getting without key.
   */
  data(e, n) {
    return typeof e == "string" ? arguments.length === 2 ? (Bs("data", this.frozen), this.namespace[e] = n, this) : n0.call(this.namespace, e) && this.namespace[e] || void 0 : e ? (Bs("data", this.frozen), this.namespace = e, this) : this.namespace;
  }
  /**
   * Freeze a processor.
   *
   * Frozen processors are meant to be extended and not to be configured
   * directly.
   *
   * When a processor is frozen it cannot be unfrozen.
   * New processors working the same way can be created by calling the
   * processor.
   *
   * It’s possible to freeze processors explicitly by calling `.freeze()`.
   * Processors freeze automatically when `.parse()`, `.run()`, `.runSync()`,
   * `.stringify()`, `.process()`, or `.processSync()` are called.
   *
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *   The current processor.
   */
  freeze() {
    if (this.frozen)
      return this;
    const e = (
      /** @type {Processor} */
      /** @type {unknown} */
      this
    );
    for (; ++this.freezeIndex < this.attachers.length; ) {
      const [n, ...r] = this.attachers[this.freezeIndex];
      if (r[0] === !1)
        continue;
      r[0] === !0 && (r[0] = void 0);
      const i = n.call(e, ...r);
      typeof i == "function" && this.transformers.use(i);
    }
    return this.frozen = !0, this.freezeIndex = Number.POSITIVE_INFINITY, this;
  }
  /**
   * Parse text to a syntax tree.
   *
   * > **Note**: `parse` freezes the processor if not already *frozen*.
   *
   * > **Note**: `parse` performs the parse phase, not the run phase or other
   * > phases.
   *
   * @param {Compatible | undefined} [file]
   *   file to parse (optional); typically `string` or `VFile`; any value
   *   accepted as `x` in `new VFile(x)`.
   * @returns {ParseTree extends undefined ? Node : ParseTree}
   *   Syntax tree representing `file`.
   */
  parse(e) {
    this.freeze();
    const n = Xi(e), r = this.parser || this.Parser;
    return Ls("parse", r), r(String(n), n);
  }
  /**
   * Process the given file as configured on the processor.
   *
   * > **Note**: `process` freezes the processor if not already *frozen*.
   *
   * > **Note**: `process` performs the parse, run, and stringify phases.
   *
   * @overload
   * @param {Compatible | undefined} file
   * @param {ProcessCallback<VFileWithOutput<CompileResult>>} done
   * @returns {undefined}
   *
   * @overload
   * @param {Compatible | undefined} [file]
   * @returns {Promise<VFileWithOutput<CompileResult>>}
   *
   * @param {Compatible | undefined} [file]
   *   File (optional); typically `string` or `VFile`]; any value accepted as
   *   `x` in `new VFile(x)`.
   * @param {ProcessCallback<VFileWithOutput<CompileResult>> | undefined} [done]
   *   Callback (optional).
   * @returns {Promise<VFile> | undefined}
   *   Nothing if `done` is given.
   *   Otherwise a promise, rejected with a fatal error or resolved with the
   *   processed file.
   *
   *   The parsed, transformed, and compiled value is available at
   *   `file.value` (see note).
   *
   *   > **Note**: unified typically compiles by serializing: most
   *   > compilers return `string` (or `Uint8Array`).
   *   > Some compilers, such as the one configured with
   *   > [`rehype-react`][rehype-react], return other values (in this case, a
   *   > React tree).
   *   > If you’re using a compiler that doesn’t serialize, expect different
   *   > result values.
   *   >
   *   > To register custom results in TypeScript, add them to
   *   > {@linkcode CompileResultMap}.
   *
   *   [rehype-react]: https://github.com/rehypejs/rehype-react
   */
  process(e, n) {
    const r = this;
    return this.freeze(), Ls("process", this.parser || this.Parser), Fs("process", this.compiler || this.Compiler), n ? i(void 0, n) : new Promise(i);
    function i(o, s) {
      const l = Xi(e), a = (
        /** @type {HeadTree extends undefined ? Node : HeadTree} */
        /** @type {unknown} */
        r.parse(l)
      );
      r.run(a, l, function(u, h, f) {
        if (u || !h || !f)
          return c(u);
        const d = (
          /** @type {CompileTree extends undefined ? Node : CompileTree} */
          /** @type {unknown} */
          h
        ), p = r.stringify(d, f);
        i0(p) ? f.value = p : f.result = p, c(
          u,
          /** @type {VFileWithOutput<CompileResult>} */
          f
        );
      });
      function c(u, h) {
        u || !h ? s(u) : o ? o(h) : n(void 0, h);
      }
    }
  }
  /**
   * Process the given file as configured on the processor.
   *
   * An error is thrown if asynchronous transforms are configured.
   *
   * > **Note**: `processSync` freezes the processor if not already *frozen*.
   *
   * > **Note**: `processSync` performs the parse, run, and stringify phases.
   *
   * @param {Compatible | undefined} [file]
   *   File (optional); typically `string` or `VFile`; any value accepted as
   *   `x` in `new VFile(x)`.
   * @returns {VFileWithOutput<CompileResult>}
   *   The processed file.
   *
   *   The parsed, transformed, and compiled value is available at
   *   `file.value` (see note).
   *
   *   > **Note**: unified typically compiles by serializing: most
   *   > compilers return `string` (or `Uint8Array`).
   *   > Some compilers, such as the one configured with
   *   > [`rehype-react`][rehype-react], return other values (in this case, a
   *   > React tree).
   *   > If you’re using a compiler that doesn’t serialize, expect different
   *   > result values.
   *   >
   *   > To register custom results in TypeScript, add them to
   *   > {@linkcode CompileResultMap}.
   *
   *   [rehype-react]: https://github.com/rehypejs/rehype-react
   */
  processSync(e) {
    let n = !1, r;
    return this.freeze(), Ls("processSync", this.parser || this.Parser), Fs("processSync", this.compiler || this.Compiler), this.process(e, i), $u("processSync", "process", n), r;
    function i(o, s) {
      n = !0, Pu(o), r = s;
    }
  }
  /**
   * Run *transformers* on a syntax tree.
   *
   * > **Note**: `run` freezes the processor if not already *frozen*.
   *
   * > **Note**: `run` performs the run phase, not other phases.
   *
   * @overload
   * @param {HeadTree extends undefined ? Node : HeadTree} tree
   * @param {RunCallback<TailTree extends undefined ? Node : TailTree>} done
   * @returns {undefined}
   *
   * @overload
   * @param {HeadTree extends undefined ? Node : HeadTree} tree
   * @param {Compatible | undefined} file
   * @param {RunCallback<TailTree extends undefined ? Node : TailTree>} done
   * @returns {undefined}
   *
   * @overload
   * @param {HeadTree extends undefined ? Node : HeadTree} tree
   * @param {Compatible | undefined} [file]
   * @returns {Promise<TailTree extends undefined ? Node : TailTree>}
   *
   * @param {HeadTree extends undefined ? Node : HeadTree} tree
   *   Tree to transform and inspect.
   * @param {(
   *   RunCallback<TailTree extends undefined ? Node : TailTree> |
   *   Compatible
   * )} [file]
   *   File associated with `node` (optional); any value accepted as `x` in
   *   `new VFile(x)`.
   * @param {RunCallback<TailTree extends undefined ? Node : TailTree>} [done]
   *   Callback (optional).
   * @returns {Promise<TailTree extends undefined ? Node : TailTree> | undefined}
   *   Nothing if `done` is given.
   *   Otherwise, a promise rejected with a fatal error or resolved with the
   *   transformed tree.
   */
  run(e, n, r) {
    _u(e), this.freeze();
    const i = this.transformers;
    return !r && typeof n == "function" && (r = n, n = void 0), r ? o(void 0, r) : new Promise(o);
    function o(s, l) {
      const a = Xi(n);
      i.run(e, a, c);
      function c(u, h, f) {
        const d = (
          /** @type {TailTree extends undefined ? Node : TailTree} */
          h || e
        );
        u ? l(u) : s ? s(d) : r(void 0, d, f);
      }
    }
  }
  /**
   * Run *transformers* on a syntax tree.
   *
   * An error is thrown if asynchronous transforms are configured.
   *
   * > **Note**: `runSync` freezes the processor if not already *frozen*.
   *
   * > **Note**: `runSync` performs the run phase, not other phases.
   *
   * @param {HeadTree extends undefined ? Node : HeadTree} tree
   *   Tree to transform and inspect.
   * @param {Compatible | undefined} [file]
   *   File associated with `node` (optional); any value accepted as `x` in
   *   `new VFile(x)`.
   * @returns {TailTree extends undefined ? Node : TailTree}
   *   Transformed tree.
   */
  runSync(e, n) {
    let r = !1, i;
    return this.run(e, n, o), $u("runSync", "run", r), i;
    function o(s, l) {
      Pu(s), i = l, r = !0;
    }
  }
  /**
   * Compile a syntax tree.
   *
   * > **Note**: `stringify` freezes the processor if not already *frozen*.
   *
   * > **Note**: `stringify` performs the stringify phase, not the run phase
   * > or other phases.
   *
   * @param {CompileTree extends undefined ? Node : CompileTree} tree
   *   Tree to compile.
   * @param {Compatible | undefined} [file]
   *   File associated with `node` (optional); any value accepted as `x` in
   *   `new VFile(x)`.
   * @returns {CompileResult extends undefined ? Value : CompileResult}
   *   Textual representation of the tree (see note).
   *
   *   > **Note**: unified typically compiles by serializing: most compilers
   *   > return `string` (or `Uint8Array`).
   *   > Some compilers, such as the one configured with
   *   > [`rehype-react`][rehype-react], return other values (in this case, a
   *   > React tree).
   *   > If you’re using a compiler that doesn’t serialize, expect different
   *   > result values.
   *   >
   *   > To register custom results in TypeScript, add them to
   *   > {@linkcode CompileResultMap}.
   *
   *   [rehype-react]: https://github.com/rehypejs/rehype-react
   */
  stringify(e, n) {
    this.freeze();
    const r = Xi(n), i = this.compiler || this.Compiler;
    return Fs("stringify", i), _u(e), i(e, r);
  }
  /**
   * Configure the processor to use a plugin, a list of usable values, or a
   * preset.
   *
   * If the processor is already using a plugin, the previous plugin
   * configuration is changed based on the options that are passed in.
   * In other words, the plugin is not added a second time.
   *
   * > **Note**: `use` cannot be called on *frozen* processors.
   * > Call the processor first to create a new unfrozen processor.
   *
   * @example
   *   There are many ways to pass plugins to `.use()`.
   *   This example gives an overview:
   *
   *   ```js
   *   import {unified} from 'unified'
   *
   *   unified()
   *     // Plugin with options:
   *     .use(pluginA, {x: true, y: true})
   *     // Passing the same plugin again merges configuration (to `{x: true, y: false, z: true}`):
   *     .use(pluginA, {y: false, z: true})
   *     // Plugins:
   *     .use([pluginB, pluginC])
   *     // Two plugins, the second with options:
   *     .use([pluginD, [pluginE, {}]])
   *     // Preset with plugins and settings:
   *     .use({plugins: [pluginF, [pluginG, {}]], settings: {position: false}})
   *     // Settings only:
   *     .use({settings: {position: false}})
   *   ```
   *
   * @template {Array<unknown>} [Parameters=[]]
   * @template {Node | string | undefined} [Input=undefined]
   * @template [Output=Input]
   *
   * @overload
   * @param {Preset | null | undefined} [preset]
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *
   * @overload
   * @param {PluggableList} list
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *
   * @overload
   * @param {Plugin<Parameters, Input, Output>} plugin
   * @param {...(Parameters | [boolean])} parameters
   * @returns {UsePlugin<ParseTree, HeadTree, TailTree, CompileTree, CompileResult, Input, Output>}
   *
   * @param {PluggableList | Plugin | Preset | null | undefined} value
   *   Usable value.
   * @param {...unknown} parameters
   *   Parameters, when a plugin is given as a usable value.
   * @returns {Processor<ParseTree, HeadTree, TailTree, CompileTree, CompileResult>}
   *   Current processor.
   */
  use(e, ...n) {
    const r = this.attachers, i = this.namespace;
    if (Bs("use", this.frozen), e != null) if (typeof e == "function")
      a(e, n);
    else if (typeof e == "object")
      Array.isArray(e) ? l(e) : s(e);
    else
      throw new TypeError("Expected usable value, not `" + e + "`");
    return this;
    function o(c) {
      if (typeof c == "function")
        a(c, []);
      else if (typeof c == "object")
        if (Array.isArray(c)) {
          const [u, ...h] = (
            /** @type {PluginTuple<Array<unknown>>} */
            c
          );
          a(u, h);
        } else
          s(c);
      else
        throw new TypeError("Expected usable value, not `" + c + "`");
    }
    function s(c) {
      if (!("plugins" in c) && !("settings" in c))
        throw new Error(
          "Expected usable value but received an empty preset, which is probably a mistake: presets typically come with `plugins` and sometimes with `settings`, but this has neither"
        );
      l(c.plugins), c.settings && (i.settings = Rs(!0, i.settings, c.settings));
    }
    function l(c) {
      let u = -1;
      if (c != null) if (Array.isArray(c))
        for (; ++u < c.length; ) {
          const h = c[u];
          o(h);
        }
      else
        throw new TypeError("Expected a list of plugins, not `" + c + "`");
    }
    function a(c, u) {
      let h = -1, f = -1;
      for (; ++h < r.length; )
        if (r[h][0] === c) {
          f = h;
          break;
        }
      if (f === -1)
        r.push([c, ...u]);
      else if (u.length > 0) {
        let [d, ...p] = u;
        const m = r[f][1];
        Ol(m) && Ol(d) && (d = Rs(!0, m, d)), r[f] = [c, d, ...p];
      }
    }
  }
}
const vl = new ya().freeze();
function Ls(t, e) {
  if (typeof e != "function")
    throw new TypeError("Cannot `" + t + "` without `parser`");
}
function Fs(t, e) {
  if (typeof e != "function")
    throw new TypeError("Cannot `" + t + "` without `compiler`");
}
function Bs(t, e) {
  if (e)
    throw new Error(
      "Cannot call `" + t + "` on a frozen processor.\nCreate a new processor first, by calling it: use `processor()` instead of `processor`."
    );
}
function _u(t) {
  if (!Ol(t) || typeof t.type != "string")
    throw new TypeError("Expected node, got `" + t + "`");
}
function $u(t, e, n) {
  if (!n)
    throw new Error(
      "`" + t + "` finished async. Use `" + e + "` instead"
    );
}
function Xi(t) {
  return r0(t) ? t : new Zb(t);
}
function r0(t) {
  return !!(t && typeof t == "object" && "message" in t && "messages" in t);
}
function i0(t) {
  return typeof t == "string" || o0(t);
}
function o0(t) {
  return !!(t && typeof t == "object" && "byteLength" in t && "byteOffset" in t);
}
var hd = (t) => {
  throw TypeError(t);
}, fd = (t, e, n) => e.has(t) || hd("Cannot " + n), q = (t, e, n) => (fd(t, e, "read from private field"), n ? n.call(t) : e.get(t)), de = (t, e, n) => e.has(t) ? hd("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, n), oe = (t, e, n, r) => (fd(t, e, "write to private field"), e.set(t, n), n), bt, Kr, go, yo, ko, Jr, Ur, Lt, Gr, bo, wo, Yr, xo, Qr, Co, So, nr, Tn, Mo, Xr;
class dd {
}
class pd {
  constructor() {
    this.elements = [], this.size = () => this.elements.length, this.top = () => this.elements.at(-1), this.push = (e) => {
      var n;
      (n = this.top()) == null || n.push(e);
    }, this.open = (e) => {
      this.elements.push(e);
    }, this.close = () => {
      const e = this.elements.pop();
      if (!e) throw uf();
      return e;
    };
  }
}
class ka extends dd {
  constructor(e, n, r) {
    super(), this.type = e, this.content = n, this.attrs = r;
  }
  push(e, ...n) {
    this.content.push(e, ...n);
  }
  pop() {
    return this.content.pop();
  }
  static create(e, n, r) {
    return new ka(e, n, r);
  }
}
const Pl = class extends pd {
  /// @internal
  constructor(e) {
    super(), de(this, bt), de(this, Kr), de(this, go), de(this, yo), de(this, ko), de(this, Jr), de(this, Ur), oe(this, bt, G.none), oe(this, Kr, (n) => n.isText), oe(this, go, (n, r) => {
      if (q(this, Kr).call(this, n) && q(this, Kr).call(this, r) && G.sameSet(n.marks, r.marks))
        return this.schema.text(n.text + r.text, n.marks);
    }), oe(this, yo, (n) => {
      const r = Object.values({
        ...this.schema.nodes,
        ...this.schema.marks
      }).find((i) => i.spec.parseMarkdown.match(n));
      if (!r) throw Lg(n);
      return r;
    }), oe(this, ko, (n) => {
      const r = q(this, yo).call(this, n);
      r.spec.parseMarkdown.runner(this, n, r);
    }), this.injectRoot = (n, r, i) => (this.openNode(r, i), this.next(n.children), this), this.openNode = (n, r) => (this.open(ka.create(n, [], r)), this), oe(this, Jr, () => {
      oe(this, bt, G.none);
      const n = this.close();
      return q(this, Ur).call(this, n.type, n.attrs, n.content);
    }), this.closeNode = () => (q(this, Jr).call(this), this), oe(this, Ur, (n, r, i) => {
      const o = n.createAndFill(r, i, q(this, bt));
      if (!o) throw zg(n, r, i);
      return this.push(o), o;
    }), this.addNode = (n, r, i) => (q(this, Ur).call(this, n, r, i), this), this.openMark = (n, r) => {
      const i = n.create(r);
      return oe(this, bt, i.addToSet(q(this, bt))), this;
    }, this.closeMark = (n) => (oe(this, bt, n.removeFromSet(q(this, bt))), this), this.addText = (n) => {
      const r = this.top();
      if (!r) throw uf();
      const i = r.pop(), o = this.schema.text(n, q(this, bt));
      if (!i)
        return r.push(o), this;
      const s = q(this, go).call(this, i, o);
      return s ? (r.push(s), this) : (r.push(i, o), this);
    }, this.build = () => {
      let n;
      do
        n = q(this, Jr).call(this);
      while (this.size());
      return n;
    }, this.next = (n = []) => ([n].flat().forEach((r) => q(this, ko).call(this, r)), this), this.toDoc = () => this.build(), this.run = (n, r) => {
      const i = n.runSync(
        n.parse(r),
        r
      );
      return this.next(i), this;
    }, this.schema = e;
  }
};
bt = /* @__PURE__ */ new WeakMap();
Kr = /* @__PURE__ */ new WeakMap();
go = /* @__PURE__ */ new WeakMap();
yo = /* @__PURE__ */ new WeakMap();
ko = /* @__PURE__ */ new WeakMap();
Jr = /* @__PURE__ */ new WeakMap();
Ur = /* @__PURE__ */ new WeakMap();
Pl.create = (t, e) => {
  const n = new Pl(t);
  return (r) => (n.run(e, r), n.toDoc());
};
let s0 = Pl;
const zl = class extends dd {
  constructor(e, n, r, i = {}) {
    super(), this.type = e, this.children = n, this.value = r, this.props = i, this.push = (o, ...s) => {
      this.children || (this.children = []), this.children.push(o, ...s);
    }, this.pop = () => {
      var o;
      return (o = this.children) == null ? void 0 : o.pop();
    };
  }
};
zl.create = (t, e, n, r = {}) => new zl(t, e, n, r);
let Vu = zl;
const l0 = (t) => Object.prototype.hasOwnProperty.call(t, "size"), Ll = class extends pd {
  /// @internal
  constructor(e) {
    super(), de(this, Lt), de(this, Gr), de(this, bo), de(this, wo), de(this, Yr), de(this, xo), de(this, Qr), de(this, Co), de(this, So), de(this, nr), de(this, Tn), de(this, Mo), de(this, Xr), oe(this, Lt, G.none), oe(this, Gr, (n) => {
      const r = Object.values({
        ...this.schema.nodes,
        ...this.schema.marks
      }).find((i) => i.spec.toMarkdown.match(n));
      if (!r) throw Fg(n.type);
      return r;
    }), oe(this, bo, (n) => q(this, Gr).call(this, n).spec.toMarkdown.runner(this, n)), oe(this, wo, (n, r) => q(this, Gr).call(this, n).spec.toMarkdown.runner(this, n, r)), oe(this, Yr, (n) => {
      const { marks: r } = n, i = (l) => l.type.spec.priority ?? 50;
      [...r].sort((l, a) => i(l) - i(a)).every((l) => !q(this, wo).call(this, l, n)) && q(this, bo).call(this, n), r.forEach((l) => q(this, Xr).call(this, l));
    }), oe(this, xo, (n, r) => {
      var i;
      if (n.type === r || ((i = n.children) == null ? void 0 : i.length) !== 1) return n;
      const o = (c) => {
        var u;
        if (c.type === r) return c;
        if (((u = c.children) == null ? void 0 : u.length) !== 1) return null;
        const [h] = c.children;
        return h ? o(h) : null;
      }, s = o(n);
      if (!s) return n;
      const l = s.children ? [...s.children] : void 0, a = { ...n, children: l };
      return a.children = l, s.children = [a], s;
    }), oe(this, Qr, (n) => {
      const { children: r } = n;
      return r && (n.children = r.reduce((i, o, s) => {
        if (s === 0) return [o];
        const l = i.at(-1);
        if (l && l.isMark && o.isMark) {
          o = q(this, xo).call(this, o, l.type);
          const { children: a, ...c } = o, { children: u, ...h } = l;
          if (o.type === l.type && a && u && JSON.stringify(c) === JSON.stringify(h)) {
            const f = {
              ...h,
              children: [...u, ...a]
            };
            return i.slice(0, -1).concat(q(this, Qr).call(this, f));
          }
        }
        return i.concat(o);
      }, [])), n;
    }), oe(this, Co, (n) => {
      const r = {
        ...n.props,
        type: n.type
      };
      return n.children && (r.children = n.children), n.value && (r.value = n.value), r;
    }), this.openNode = (n, r, i) => (this.open(Vu.create(n, void 0, r, i)), this), oe(this, So, (n, r) => {
      let i = "", o = "";
      const s = n.children;
      let l = -1, a = -1;
      const c = (h) => {
        h && h.forEach((f, d) => {
          f.type === "text" && f.value && (l < 0 && (l = d), a = d);
        });
      };
      if (s) {
        c(s);
        const h = s == null ? void 0 : s[a], f = s == null ? void 0 : s[l];
        h && h.value.endsWith(" ") && (o = h.value.match(/ +$/)[0], h.value = h.value.trimEnd()), f && f.value.startsWith(" ") && (i = f.value.match(/^ +/)[0], f.value = f.value.trimStart());
      }
      i.length && q(this, Tn).call(this, "text", void 0, i);
      const u = r();
      return o.length && q(this, Tn).call(this, "text", void 0, o), u;
    }), oe(this, nr, (n = !1) => {
      const r = this.close(), i = () => q(this, Tn).call(this, r.type, r.children, r.value, r.props);
      return n ? q(this, So).call(this, r, i) : i();
    }), this.closeNode = () => (q(this, nr).call(this), this), oe(this, Tn, (n, r, i, o) => {
      const s = Vu.create(n, r, i, o), l = q(this, Qr).call(this, q(this, Co).call(this, s));
      return this.push(l), l;
    }), this.addNode = (n, r, i, o) => (q(this, Tn).call(this, n, r, i, o), this), oe(this, Mo, (n, r, i, o) => n.isInSet(q(this, Lt)) ? this : (oe(this, Lt, n.addToSet(q(this, Lt))), this.openNode(r, i, { ...o, isMark: !0 }))), oe(this, Xr, (n) => {
      n.isInSet(q(this, Lt)) && (oe(this, Lt, n.type.removeFromSet(q(this, Lt))), q(this, nr).call(this, !0));
    }), this.withMark = (n, r, i, o) => (q(this, Mo).call(this, n, r, i, o), this), this.closeMark = (n) => (q(this, Xr).call(this, n), this), this.build = () => {
      let n = null;
      do
        n = q(this, nr).call(this);
      while (this.size());
      return n;
    }, this.next = (n) => l0(n) ? (n.forEach((r) => {
      q(this, Yr).call(this, r);
    }), this) : (q(this, Yr).call(this, n), this), this.toString = (n) => n.stringify(this.build()), this.run = (n) => (this.next(n), this), this.schema = e;
  }
};
Lt = /* @__PURE__ */ new WeakMap();
Gr = /* @__PURE__ */ new WeakMap();
bo = /* @__PURE__ */ new WeakMap();
wo = /* @__PURE__ */ new WeakMap();
Yr = /* @__PURE__ */ new WeakMap();
xo = /* @__PURE__ */ new WeakMap();
Qr = /* @__PURE__ */ new WeakMap();
Co = /* @__PURE__ */ new WeakMap();
So = /* @__PURE__ */ new WeakMap();
nr = /* @__PURE__ */ new WeakMap();
Tn = /* @__PURE__ */ new WeakMap();
Mo = /* @__PURE__ */ new WeakMap();
Xr = /* @__PURE__ */ new WeakMap();
Ll.create = (t, e) => {
  const n = new Ll(t);
  return (r) => (n.run(r), n.toString(e));
};
let a0 = Ll;
const md = 65535, gd = Math.pow(2, 16);
function c0(t, e) {
  return t + e * gd;
}
function Wu(t) {
  return t & md;
}
function u0(t) {
  return (t - (t & md)) / gd;
}
const yd = 1, kd = 2, No = 4, bd = 8;
class Fl {
  /**
  @internal
  */
  constructor(e, n, r) {
    this.pos = e, this.delInfo = n, this.recover = r;
  }
  /**
  Tells you whether the position was deleted, that is, whether the
  step removed the token on the side queried (via the `assoc`)
  argument from the document.
  */
  get deleted() {
    return (this.delInfo & bd) > 0;
  }
  /**
  Tells you whether the token before the mapped position was deleted.
  */
  get deletedBefore() {
    return (this.delInfo & (yd | No)) > 0;
  }
  /**
  True when the token after the mapped position was deleted.
  */
  get deletedAfter() {
    return (this.delInfo & (kd | No)) > 0;
  }
  /**
  Tells whether any of the steps mapped through deletes across the
  position (including both the token before and after the
  position).
  */
  get deletedAcross() {
    return (this.delInfo & No) > 0;
  }
}
class Ke {
  /**
  Create a position map. The modifications to the document are
  represented as an array of numbers, in which each group of three
  represents a modified chunk as `[start, oldSize, newSize]`.
  */
  constructor(e, n = !1) {
    if (this.ranges = e, this.inverted = n, !e.length && Ke.empty)
      return Ke.empty;
  }
  /**
  @internal
  */
  recover(e) {
    let n = 0, r = Wu(e);
    if (!this.inverted)
      for (let i = 0; i < r; i++)
        n += this.ranges[i * 3 + 2] - this.ranges[i * 3 + 1];
    return this.ranges[r * 3] + n + u0(e);
  }
  mapResult(e, n = 1) {
    return this._map(e, n, !1);
  }
  map(e, n = 1) {
    return this._map(e, n, !0);
  }
  /**
  @internal
  */
  _map(e, n, r) {
    let i = 0, o = this.inverted ? 2 : 1, s = this.inverted ? 1 : 2;
    for (let l = 0; l < this.ranges.length; l += 3) {
      let a = this.ranges[l] - (this.inverted ? i : 0);
      if (a > e)
        break;
      let c = this.ranges[l + o], u = this.ranges[l + s], h = a + c;
      if (e <= h) {
        let f = c ? e == a ? -1 : e == h ? 1 : n : n, d = a + i + (f < 0 ? 0 : u);
        if (r)
          return d;
        let p = e == (n < 0 ? a : h) ? null : c0(l / 3, e - a), m = e == a ? kd : e == h ? yd : No;
        return (n < 0 ? e != a : e != h) && (m |= bd), new Fl(d, m, p);
      }
      i += u - c;
    }
    return r ? e + i : new Fl(e + i, 0, null);
  }
  /**
  @internal
  */
  touches(e, n) {
    let r = 0, i = Wu(n), o = this.inverted ? 2 : 1, s = this.inverted ? 1 : 2;
    for (let l = 0; l < this.ranges.length; l += 3) {
      let a = this.ranges[l] - (this.inverted ? r : 0);
      if (a > e)
        break;
      let c = this.ranges[l + o], u = a + c;
      if (e <= u && l == i * 3)
        return !0;
      r += this.ranges[l + s] - c;
    }
    return !1;
  }
  /**
  Calls the given function on each of the changed ranges included in
  this map.
  */
  forEach(e) {
    let n = this.inverted ? 2 : 1, r = this.inverted ? 1 : 2;
    for (let i = 0, o = 0; i < this.ranges.length; i += 3) {
      let s = this.ranges[i], l = s - (this.inverted ? o : 0), a = s + (this.inverted ? 0 : o), c = this.ranges[i + n], u = this.ranges[i + r];
      e(l, l + c, a, a + u), o += u - c;
    }
  }
  /**
  Create an inverted version of this map. The result can be used to
  map positions in the post-step document to the pre-step document.
  */
  invert() {
    return new Ke(this.ranges, !this.inverted);
  }
  /**
  @internal
  */
  toString() {
    return (this.inverted ? "-" : "") + JSON.stringify(this.ranges);
  }
  /**
  Create a map that moves all positions by offset `n` (which may be
  negative). This can be useful when applying steps meant for a
  sub-document to a larger document, or vice-versa.
  */
  static offset(e) {
    return e == 0 ? Ke.empty : new Ke(e < 0 ? [0, -e, 0] : [0, 0, e]);
  }
}
Ke.empty = new Ke([]);
class bi {
  /**
  Create a new mapping with the given position maps.
  */
  constructor(e, n, r = 0, i = e ? e.length : 0) {
    this.mirror = n, this.from = r, this.to = i, this._maps = e || [], this.ownData = !(e || n);
  }
  /**
  The step maps in this mapping.
  */
  get maps() {
    return this._maps;
  }
  /**
  Create a mapping that maps only through a part of this one.
  */
  slice(e = 0, n = this.maps.length) {
    return new bi(this._maps, this.mirror, e, n);
  }
  /**
  Add a step map to the end of this mapping. If `mirrors` is
  given, it should be the index of the step map that is the mirror
  image of this one.
  */
  appendMap(e, n) {
    this.ownData || (this._maps = this._maps.slice(), this.mirror = this.mirror && this.mirror.slice(), this.ownData = !0), this.to = this._maps.push(e), n != null && this.setMirror(this._maps.length - 1, n);
  }
  /**
  Add all the step maps in a given mapping to this one (preserving
  mirroring information).
  */
  appendMapping(e) {
    for (let n = 0, r = this._maps.length; n < e._maps.length; n++) {
      let i = e.getMirror(n);
      this.appendMap(e._maps[n], i != null && i < n ? r + i : void 0);
    }
  }
  /**
  Finds the offset of the step map that mirrors the map at the
  given offset, in this mapping (as per the second argument to
  `appendMap`).
  */
  getMirror(e) {
    if (this.mirror) {
      for (let n = 0; n < this.mirror.length; n++)
        if (this.mirror[n] == e)
          return this.mirror[n + (n % 2 ? -1 : 1)];
    }
  }
  /**
  @internal
  */
  setMirror(e, n) {
    this.mirror || (this.mirror = []), this.mirror.push(e, n);
  }
  /**
  Append the inverse of the given mapping to this one.
  */
  appendMappingInverted(e) {
    for (let n = e.maps.length - 1, r = this._maps.length + e._maps.length; n >= 0; n--) {
      let i = e.getMirror(n);
      this.appendMap(e._maps[n].invert(), i != null && i > n ? r - i - 1 : void 0);
    }
  }
  /**
  Create an inverted version of this mapping.
  */
  invert() {
    let e = new bi();
    return e.appendMappingInverted(this), e;
  }
  /**
  Map a position through this mapping.
  */
  map(e, n = 1) {
    if (this.mirror)
      return this._map(e, n, !0);
    for (let r = this.from; r < this.to; r++)
      e = this._maps[r].map(e, n);
    return e;
  }
  /**
  Map a position through this mapping, returning a mapping
  result.
  */
  mapResult(e, n = 1) {
    return this._map(e, n, !1);
  }
  /**
  @internal
  */
  _map(e, n, r) {
    let i = 0;
    for (let o = this.from; o < this.to; o++) {
      let s = this._maps[o], l = s.mapResult(e, n);
      if (l.recover != null) {
        let a = this.getMirror(o);
        if (a != null && a > o && a < this.to) {
          o = a, e = this._maps[a].recover(l.recover);
          continue;
        }
      }
      i |= l.delInfo, e = l.pos;
    }
    return r ? e : new Fl(e, i, null);
  }
}
const _s = /* @__PURE__ */ Object.create(null);
class Ee {
  /**
  Get the step map that represents the changes made by this step,
  and which can be used to transform between positions in the old
  and the new document.
  */
  getMap() {
    return Ke.empty;
  }
  /**
  Try to merge this step with another one, to be applied directly
  after it. Returns the merged step when possible, null if the
  steps can't be merged.
  */
  merge(e) {
    return null;
  }
  /**
  Deserialize a step from its JSON representation. Will call
  through to the step class' own implementation of this method.
  */
  static fromJSON(e, n) {
    if (!n || !n.stepType)
      throw new RangeError("Invalid input for Step.fromJSON");
    let r = _s[n.stepType];
    if (!r)
      throw new RangeError(`No step type ${n.stepType} defined`);
    return r.fromJSON(e, n);
  }
  /**
  To be able to serialize steps to JSON, each step needs a string
  ID to attach to its JSON representation. Use this method to
  register an ID for your step classes. Try to pick something
  that's unlikely to clash with steps from other modules.
  */
  static jsonID(e, n) {
    if (e in _s)
      throw new RangeError("Duplicate use of step JSON ID " + e);
    return _s[e] = n, n.prototype.jsonID = e, n;
  }
}
class pe {
  /**
  @internal
  */
  constructor(e, n) {
    this.doc = e, this.failed = n;
  }
  /**
  Create a successful step result.
  */
  static ok(e) {
    return new pe(e, null);
  }
  /**
  Create a failed step result.
  */
  static fail(e) {
    return new pe(null, e);
  }
  /**
  Call [`Node.replace`](https://prosemirror.net/docs/ref/#model.Node.replace) with the given
  arguments. Create a successful result if it succeeds, and a
  failed one if it throws a `ReplaceError`.
  */
  static fromReplace(e, n, r, i) {
    try {
      return pe.ok(e.replace(n, r, i));
    } catch (o) {
      if (o instanceof Wo)
        return pe.fail(o.message);
      throw o;
    }
  }
}
function ba(t, e, n) {
  let r = [];
  for (let i = 0; i < t.childCount; i++) {
    let o = t.child(i);
    o.content.size && (o = o.copy(ba(o.content, e, o))), o.isInline && (o = e(o, n, i)), r.push(o);
  }
  return T.fromArray(r);
}
class $t extends Ee {
  /**
  Create a mark step.
  */
  constructor(e, n, r) {
    super(), this.from = e, this.to = n, this.mark = r;
  }
  apply(e) {
    let n = e.slice(this.from, this.to), r = e.resolve(this.from), i = r.node(r.sharedDepth(this.to)), o = new E(ba(n.content, (s, l) => !s.isAtom || !l.type.allowsMarkType(this.mark.type) ? s : s.mark(this.mark.addToSet(s.marks)), i), n.openStart, n.openEnd);
    return pe.fromReplace(e, this.from, this.to, o);
  }
  invert() {
    return new St(this.from, this.to, this.mark);
  }
  map(e) {
    let n = e.mapResult(this.from, 1), r = e.mapResult(this.to, -1);
    return n.deleted && r.deleted || n.pos >= r.pos ? null : new $t(n.pos, r.pos, this.mark);
  }
  merge(e) {
    return e instanceof $t && e.mark.eq(this.mark) && this.from <= e.to && this.to >= e.from ? new $t(Math.min(this.from, e.from), Math.max(this.to, e.to), this.mark) : null;
  }
  toJSON() {
    return {
      stepType: "addMark",
      mark: this.mark.toJSON(),
      from: this.from,
      to: this.to
    };
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.from != "number" || typeof n.to != "number")
      throw new RangeError("Invalid input for AddMarkStep.fromJSON");
    return new $t(n.from, n.to, e.markFromJSON(n.mark));
  }
}
Ee.jsonID("addMark", $t);
class St extends Ee {
  /**
  Create a mark-removing step.
  */
  constructor(e, n, r) {
    super(), this.from = e, this.to = n, this.mark = r;
  }
  apply(e) {
    let n = e.slice(this.from, this.to), r = new E(ba(n.content, (i) => i.mark(this.mark.removeFromSet(i.marks)), e), n.openStart, n.openEnd);
    return pe.fromReplace(e, this.from, this.to, r);
  }
  invert() {
    return new $t(this.from, this.to, this.mark);
  }
  map(e) {
    let n = e.mapResult(this.from, 1), r = e.mapResult(this.to, -1);
    return n.deleted && r.deleted || n.pos >= r.pos ? null : new St(n.pos, r.pos, this.mark);
  }
  merge(e) {
    return e instanceof St && e.mark.eq(this.mark) && this.from <= e.to && this.to >= e.from ? new St(Math.min(this.from, e.from), Math.max(this.to, e.to), this.mark) : null;
  }
  toJSON() {
    return {
      stepType: "removeMark",
      mark: this.mark.toJSON(),
      from: this.from,
      to: this.to
    };
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.from != "number" || typeof n.to != "number")
      throw new RangeError("Invalid input for RemoveMarkStep.fromJSON");
    return new St(n.from, n.to, e.markFromJSON(n.mark));
  }
}
Ee.jsonID("removeMark", St);
class on extends Ee {
  /**
  Create a node mark step.
  */
  constructor(e, n) {
    super(), this.pos = e, this.mark = n;
  }
  apply(e) {
    let n = e.nodeAt(this.pos);
    if (!n)
      return pe.fail("No node at mark step's position");
    let r = n.type.create(n.attrs, null, this.mark.addToSet(n.marks));
    return pe.fromReplace(e, this.pos, this.pos + 1, new E(T.from(r), 0, n.isLeaf ? 0 : 1));
  }
  invert(e) {
    let n = e.nodeAt(this.pos);
    if (n) {
      let r = this.mark.addToSet(n.marks);
      if (r.length == n.marks.length) {
        for (let i = 0; i < n.marks.length; i++)
          if (!n.marks[i].isInSet(r))
            return new on(this.pos, n.marks[i]);
        return new on(this.pos, this.mark);
      }
    }
    return new Cr(this.pos, this.mark);
  }
  map(e) {
    let n = e.mapResult(this.pos, 1);
    return n.deletedAfter ? null : new on(n.pos, this.mark);
  }
  toJSON() {
    return { stepType: "addNodeMark", pos: this.pos, mark: this.mark.toJSON() };
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.pos != "number")
      throw new RangeError("Invalid input for AddNodeMarkStep.fromJSON");
    return new on(n.pos, e.markFromJSON(n.mark));
  }
}
Ee.jsonID("addNodeMark", on);
class Cr extends Ee {
  /**
  Create a mark-removing step.
  */
  constructor(e, n) {
    super(), this.pos = e, this.mark = n;
  }
  apply(e) {
    let n = e.nodeAt(this.pos);
    if (!n)
      return pe.fail("No node at mark step's position");
    let r = n.type.create(n.attrs, null, this.mark.removeFromSet(n.marks));
    return pe.fromReplace(e, this.pos, this.pos + 1, new E(T.from(r), 0, n.isLeaf ? 0 : 1));
  }
  invert(e) {
    let n = e.nodeAt(this.pos);
    return !n || !this.mark.isInSet(n.marks) ? this : new on(this.pos, this.mark);
  }
  map(e) {
    let n = e.mapResult(this.pos, 1);
    return n.deletedAfter ? null : new Cr(n.pos, this.mark);
  }
  toJSON() {
    return { stepType: "removeNodeMark", pos: this.pos, mark: this.mark.toJSON() };
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.pos != "number")
      throw new RangeError("Invalid input for RemoveNodeMarkStep.fromJSON");
    return new Cr(n.pos, e.markFromJSON(n.mark));
  }
}
Ee.jsonID("removeNodeMark", Cr);
class xe extends Ee {
  /**
  The given `slice` should fit the 'gap' between `from` and
  `to`—the depths must line up, and the surrounding nodes must be
  able to be joined with the open sides of the slice. When
  `structure` is true, the step will fail if the content between
  from and to is not just a sequence of closing and then opening
  tokens (this is to guard against rebased replace steps
  overwriting something they weren't supposed to).
  */
  constructor(e, n, r, i = !1) {
    super(), this.from = e, this.to = n, this.slice = r, this.structure = i;
  }
  apply(e) {
    return this.structure && Bl(e, this.from, this.to) ? pe.fail("Structure replace would overwrite content") : pe.fromReplace(e, this.from, this.to, this.slice);
  }
  getMap() {
    return new Ke([this.from, this.to - this.from, this.slice.size]);
  }
  invert(e) {
    return new xe(this.from, this.from + this.slice.size, e.slice(this.from, this.to));
  }
  map(e) {
    let n = e.mapResult(this.from, 1), r = e.mapResult(this.to, -1);
    return n.deletedAcross && r.deletedAcross ? null : new xe(n.pos, Math.max(n.pos, r.pos), this.slice);
  }
  merge(e) {
    if (!(e instanceof xe) || e.structure || this.structure)
      return null;
    if (this.from + this.slice.size == e.from && !this.slice.openEnd && !e.slice.openStart) {
      let n = this.slice.size + e.slice.size == 0 ? E.empty : new E(this.slice.content.append(e.slice.content), this.slice.openStart, e.slice.openEnd);
      return new xe(this.from, this.to + (e.to - e.from), n, this.structure);
    } else if (e.to == this.from && !this.slice.openStart && !e.slice.openEnd) {
      let n = this.slice.size + e.slice.size == 0 ? E.empty : new E(e.slice.content.append(this.slice.content), e.slice.openStart, this.slice.openEnd);
      return new xe(e.from, this.to, n, this.structure);
    } else
      return null;
  }
  toJSON() {
    let e = { stepType: "replace", from: this.from, to: this.to };
    return this.slice.size && (e.slice = this.slice.toJSON()), this.structure && (e.structure = !0), e;
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.from != "number" || typeof n.to != "number")
      throw new RangeError("Invalid input for ReplaceStep.fromJSON");
    return new xe(n.from, n.to, E.fromJSON(e, n.slice), !!n.structure);
  }
}
Ee.jsonID("replace", xe);
class Ae extends Ee {
  /**
  Create a replace-around step with the given range and gap.
  `insert` should be the point in the slice into which the content
  of the gap should be moved. `structure` has the same meaning as
  it has in the [`ReplaceStep`](https://prosemirror.net/docs/ref/#transform.ReplaceStep) class.
  */
  constructor(e, n, r, i, o, s, l = !1) {
    super(), this.from = e, this.to = n, this.gapFrom = r, this.gapTo = i, this.slice = o, this.insert = s, this.structure = l;
  }
  apply(e) {
    if (this.structure && (Bl(e, this.from, this.gapFrom) || Bl(e, this.gapTo, this.to)))
      return pe.fail("Structure gap-replace would overwrite content");
    let n = e.slice(this.gapFrom, this.gapTo);
    if (n.openStart || n.openEnd)
      return pe.fail("Gap is not a flat range");
    let r = this.slice.insertAt(this.insert, n.content);
    return r ? pe.fromReplace(e, this.from, this.to, r) : pe.fail("Content does not fit in gap");
  }
  getMap() {
    return new Ke([
      this.from,
      this.gapFrom - this.from,
      this.insert,
      this.gapTo,
      this.to - this.gapTo,
      this.slice.size - this.insert
    ]);
  }
  invert(e) {
    let n = this.gapTo - this.gapFrom;
    return new Ae(this.from, this.from + this.slice.size + n, this.from + this.insert, this.from + this.insert + n, e.slice(this.from, this.to).removeBetween(this.gapFrom - this.from, this.gapTo - this.from), this.gapFrom - this.from, this.structure);
  }
  map(e) {
    let n = e.mapResult(this.from, 1), r = e.mapResult(this.to, -1), i = this.from == this.gapFrom ? n.pos : e.map(this.gapFrom, -1), o = this.to == this.gapTo ? r.pos : e.map(this.gapTo, 1);
    return n.deletedAcross && r.deletedAcross || i < n.pos || o > r.pos ? null : new Ae(n.pos, r.pos, i, o, this.slice, this.insert, this.structure);
  }
  toJSON() {
    let e = {
      stepType: "replaceAround",
      from: this.from,
      to: this.to,
      gapFrom: this.gapFrom,
      gapTo: this.gapTo,
      insert: this.insert
    };
    return this.slice.size && (e.slice = this.slice.toJSON()), this.structure && (e.structure = !0), e;
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.from != "number" || typeof n.to != "number" || typeof n.gapFrom != "number" || typeof n.gapTo != "number" || typeof n.insert != "number")
      throw new RangeError("Invalid input for ReplaceAroundStep.fromJSON");
    return new Ae(n.from, n.to, n.gapFrom, n.gapTo, E.fromJSON(e, n.slice), n.insert, !!n.structure);
  }
}
Ee.jsonID("replaceAround", Ae);
function Bl(t, e, n) {
  let r = t.resolve(e), i = n - e, o = r.depth;
  for (; i > 0 && o > 0 && r.indexAfter(o) == r.node(o).childCount; )
    o--, i--;
  if (i > 0) {
    let s = r.node(o).maybeChild(r.indexAfter(o));
    for (; i > 0; ) {
      if (!s || s.isLeaf)
        return !0;
      s = s.firstChild, i--;
    }
  }
  return !1;
}
function h0(t, e, n, r) {
  let i = [], o = [], s, l;
  t.doc.nodesBetween(e, n, (a, c, u) => {
    if (!a.isInline)
      return;
    let h = a.marks;
    if (!r.isInSet(h) && u.type.allowsMarkType(r.type)) {
      let f = Math.max(c, e), d = Math.min(c + a.nodeSize, n), p = r.addToSet(h);
      for (let m = 0; m < h.length; m++)
        h[m].isInSet(p) || (s && s.to == f && s.mark.eq(h[m]) ? s.to = d : i.push(s = new St(f, d, h[m])));
      l && l.to == f ? l.to = d : o.push(l = new $t(f, d, r));
    }
  }), i.forEach((a) => t.step(a)), o.forEach((a) => t.step(a));
}
function f0(t, e, n, r) {
  let i = [], o = 0;
  t.doc.nodesBetween(e, n, (s, l) => {
    if (!s.isInline)
      return;
    o++;
    let a = null;
    if (r instanceof is) {
      let c = s.marks, u;
      for (; u = r.isInSet(c); )
        (a || (a = [])).push(u), c = u.removeFromSet(c);
    } else r ? r.isInSet(s.marks) && (a = [r]) : a = s.marks;
    if (a && a.length) {
      let c = Math.min(l + s.nodeSize, n);
      for (let u = 0; u < a.length; u++) {
        let h = a[u], f;
        for (let d = 0; d < i.length; d++) {
          let p = i[d];
          p.step == o - 1 && h.eq(i[d].style) && (f = p);
        }
        f ? (f.to = c, f.step = o) : i.push({ style: h, from: Math.max(l, e), to: c, step: o });
      }
    }
  }), i.forEach((s) => t.step(new St(s.from, s.to, s.style)));
}
function wa(t, e, n, r = n.contentMatch, i = !0) {
  let o = t.doc.nodeAt(e), s = [], l = e + 1;
  for (let a = 0; a < o.childCount; a++) {
    let c = o.child(a), u = l + c.nodeSize, h = r.matchType(c.type);
    if (!h)
      s.push(new xe(l, u, E.empty));
    else {
      r = h;
      for (let f = 0; f < c.marks.length; f++)
        n.allowsMarkType(c.marks[f].type) || t.step(new St(l, u, c.marks[f]));
      if (i && c.isText && n.whitespace != "pre") {
        let f, d = /\r?\n|\r/g, p;
        for (; f = d.exec(c.text); )
          p || (p = new E(T.from(n.schema.text(" ", n.allowedMarks(c.marks))), 0, 0)), s.push(new xe(l + f.index, l + f.index + f[0].length, p));
      }
    }
    l = u;
  }
  if (!r.validEnd) {
    let a = r.fillBefore(T.empty, !0);
    t.replace(l, l, new E(a, 0, 0));
  }
  for (let a = s.length - 1; a >= 0; a--)
    t.step(s[a]);
}
function d0(t, e, n) {
  return (e == 0 || t.canReplace(e, t.childCount)) && (n == t.childCount || t.canReplace(0, n));
}
function us(t) {
  let n = t.parent.content.cutByIndex(t.startIndex, t.endIndex);
  for (let r = t.depth; ; --r) {
    let i = t.$from.node(r), o = t.$from.index(r), s = t.$to.indexAfter(r);
    if (r < t.depth && i.canReplace(o, s, n))
      return r;
    if (r == 0 || i.type.spec.isolating || !d0(i, o, s))
      break;
  }
  return null;
}
function p0(t, e, n) {
  let { $from: r, $to: i, depth: o } = e, s = r.before(o + 1), l = i.after(o + 1), a = s, c = l, u = T.empty, h = 0;
  for (let p = o, m = !1; p > n; p--)
    m || r.index(p) > 0 ? (m = !0, u = T.from(r.node(p).copy(u)), h++) : a--;
  let f = T.empty, d = 0;
  for (let p = o, m = !1; p > n; p--)
    m || i.after(p + 1) < i.end(p) ? (m = !0, f = T.from(i.node(p).copy(f)), d++) : c++;
  t.step(new Ae(a, c, s, l, new E(u.append(f), h, d), u.size - h, !0));
}
function wd(t, e, n = null, r = t) {
  let i = m0(t, e), o = i && g0(r, e);
  return o ? i.map(Hu).concat({ type: e, attrs: n }).concat(o.map(Hu)) : null;
}
function Hu(t) {
  return { type: t, attrs: null };
}
function m0(t, e) {
  let { parent: n, startIndex: r, endIndex: i } = t, o = n.contentMatchAt(r).findWrapping(e);
  if (!o)
    return null;
  let s = o.length ? o[0] : e;
  return n.canReplaceWith(r, i, s) ? o : null;
}
function g0(t, e) {
  let { parent: n, startIndex: r, endIndex: i } = t, o = n.child(r), s = e.contentMatch.findWrapping(o.type);
  if (!s)
    return null;
  let a = (s.length ? s[s.length - 1] : e).contentMatch;
  for (let c = r; a && c < i; c++)
    a = a.matchType(n.child(c).type);
  return !a || !a.validEnd ? null : s;
}
function y0(t, e, n) {
  let r = T.empty;
  for (let s = n.length - 1; s >= 0; s--) {
    if (r.size) {
      let l = n[s].type.contentMatch.matchFragment(r);
      if (!l || !l.validEnd)
        throw new RangeError("Wrapper type given to Transform.wrap does not form valid content of its parent wrapper");
    }
    r = T.from(n[s].type.create(n[s].attrs, r));
  }
  let i = e.start, o = e.end;
  t.step(new Ae(i, o, i, o, new E(r, 0, 0), n.length, !0));
}
function k0(t, e, n, r, i) {
  if (!r.isTextblock)
    throw new RangeError("Type given to setBlockType should be a textblock");
  let o = t.steps.length;
  t.doc.nodesBetween(e, n, (s, l) => {
    let a = typeof i == "function" ? i(s) : i;
    if (s.isTextblock && !s.hasMarkup(r, a) && b0(t.doc, t.mapping.slice(o).map(l), r)) {
      let c = null;
      if (r.schema.linebreakReplacement) {
        let d = r.whitespace == "pre", p = !!r.contentMatch.matchType(r.schema.linebreakReplacement);
        d && !p ? c = !1 : !d && p && (c = !0);
      }
      c === !1 && Cd(t, s, l, o), wa(t, t.mapping.slice(o).map(l, 1), r, void 0, c === null);
      let u = t.mapping.slice(o), h = u.map(l, 1), f = u.map(l + s.nodeSize, 1);
      return t.step(new Ae(h, f, h + 1, f - 1, new E(T.from(r.create(a, null, s.marks)), 0, 0), 1, !0)), c === !0 && xd(t, s, l, o), !1;
    }
  });
}
function xd(t, e, n, r) {
  e.forEach((i, o) => {
    if (i.isText) {
      let s, l = /\r?\n|\r/g;
      for (; s = l.exec(i.text); ) {
        let a = t.mapping.slice(r).map(n + 1 + o + s.index);
        t.replaceWith(a, a + 1, e.type.schema.linebreakReplacement.create());
      }
    }
  });
}
function Cd(t, e, n, r) {
  e.forEach((i, o) => {
    if (i.type == i.type.schema.linebreakReplacement) {
      let s = t.mapping.slice(r).map(n + 1 + o);
      t.replaceWith(s, s + 1, e.type.schema.text(`
`));
    }
  });
}
function b0(t, e, n) {
  let r = t.resolve(e), i = r.index();
  return r.parent.canReplaceWith(i, i + 1, n);
}
function w0(t, e, n, r, i) {
  let o = t.doc.nodeAt(e);
  if (!o)
    throw new RangeError("No node at given position");
  n || (n = o.type);
  let s = n.create(r, null, i || o.marks);
  if (o.isLeaf)
    return t.replaceWith(e, e + o.nodeSize, s);
  if (!n.validContent(o.content))
    throw new RangeError("Invalid content for node type " + n.name);
  t.step(new Ae(e, e + o.nodeSize, e + 1, e + o.nodeSize - 1, new E(T.from(s), 0, 0), 1, !0));
}
function ci(t, e, n = 1, r) {
  let i = t.resolve(e), o = i.depth - n, s = r && r[r.length - 1] || i.parent;
  if (o < 0 || i.parent.type.spec.isolating || !i.parent.canReplace(i.index(), i.parent.childCount) || !s.type.validContent(i.parent.content.cutByIndex(i.index(), i.parent.childCount)))
    return !1;
  for (let c = i.depth - 1, u = n - 2; c > o; c--, u--) {
    let h = i.node(c), f = i.index(c);
    if (h.type.spec.isolating)
      return !1;
    let d = h.content.cutByIndex(f, h.childCount), p = r && r[u + 1];
    p && (d = d.replaceChild(0, p.type.create(p.attrs)));
    let m = r && r[u] || h;
    if (!h.canReplace(f + 1, h.childCount) || !m.type.validContent(d))
      return !1;
  }
  let l = i.indexAfter(o), a = r && r[0];
  return i.node(o).canReplaceWith(l, l, a ? a.type : i.node(o + 1).type);
}
function x0(t, e, n = 1, r) {
  let i = t.doc.resolve(e), o = T.empty, s = T.empty;
  for (let l = i.depth, a = i.depth - n, c = n - 1; l > a; l--, c--) {
    o = T.from(i.node(l).copy(o));
    let u = r && r[c];
    s = T.from(u ? u.type.create(u.attrs, s) : i.node(l).copy(s));
  }
  t.step(new xe(e, e, new E(o.append(s), n, n), !0));
}
function hs(t, e) {
  let n = t.resolve(e), r = n.index();
  return S0(n.nodeBefore, n.nodeAfter) && n.parent.canReplace(r, r + 1);
}
function C0(t, e) {
  e.content.size || t.type.compatibleContent(e.type);
  let n = t.contentMatchAt(t.childCount), { linebreakReplacement: r } = t.type.schema;
  for (let i = 0; i < e.childCount; i++) {
    let o = e.child(i), s = o.type == r ? t.type.schema.nodes.text : o.type;
    if (n = n.matchType(s), !n || !t.type.allowsMarks(o.marks))
      return !1;
  }
  return n.validEnd;
}
function S0(t, e) {
  return !!(t && e && !t.isLeaf && C0(t, e));
}
function M0(t, e, n) {
  let r = null, { linebreakReplacement: i } = t.doc.type.schema, o = t.doc.resolve(e - n), s = o.node().type;
  if (i && s.inlineContent) {
    let u = s.whitespace == "pre", h = !!s.contentMatch.matchType(i);
    u && !h ? r = !1 : !u && h && (r = !0);
  }
  let l = t.steps.length;
  if (r === !1) {
    let u = t.doc.resolve(e + n);
    Cd(t, u.node(), u.before(), l);
  }
  s.inlineContent && wa(t, e + n - 1, s, o.node().contentMatchAt(o.index()), r == null);
  let a = t.mapping.slice(l), c = a.map(e - n);
  if (t.step(new xe(c, a.map(e + n, -1), E.empty, !0)), r === !0) {
    let u = t.doc.resolve(c);
    xd(t, u.node(), u.before(), t.steps.length);
  }
  return t;
}
function N0(t, e, n) {
  let r = t.resolve(e);
  if (r.parent.canReplaceWith(r.index(), r.index(), n))
    return e;
  if (r.parentOffset == 0)
    for (let i = r.depth - 1; i >= 0; i--) {
      let o = r.index(i);
      if (r.node(i).canReplaceWith(o, o, n))
        return r.before(i + 1);
      if (o > 0)
        return null;
    }
  if (r.parentOffset == r.parent.content.size)
    for (let i = r.depth - 1; i >= 0; i--) {
      let o = r.indexAfter(i);
      if (r.node(i).canReplaceWith(o, o, n))
        return r.after(i + 1);
      if (o < r.node(i).childCount)
        return null;
    }
  return null;
}
function T0(t, e, n) {
  let r = t.resolve(e);
  if (!n.content.size)
    return e;
  let i = n.content;
  for (let o = 0; o < n.openStart; o++)
    i = i.firstChild.content;
  for (let o = 1; o <= (n.openStart == 0 && n.size ? 2 : 1); o++)
    for (let s = r.depth; s >= 0; s--) {
      let l = s == r.depth ? 0 : r.pos <= (r.start(s + 1) + r.end(s + 1)) / 2 ? -1 : 1, a = r.index(s) + (l > 0 ? 1 : 0), c = r.node(s), u = !1;
      if (o == 1)
        u = c.canReplace(a, a, i);
      else {
        let h = c.contentMatchAt(a).findWrapping(i.firstChild.type);
        u = h && c.canReplaceWith(a, a, h[0]);
      }
      if (u)
        return l == 0 ? r.pos : l < 0 ? r.before(s + 1) : r.after(s + 1);
    }
  return null;
}
function xa(t, e, n = e, r = E.empty) {
  if (e == n && !r.size)
    return null;
  let i = t.resolve(e), o = t.resolve(n);
  return Sd(i, o, r) ? new xe(e, n, r) : new I0(i, o, r).fit();
}
function Sd(t, e, n) {
  return !n.openStart && !n.openEnd && t.start() == e.start() && t.parent.canReplace(t.index(), e.index(), n.content);
}
class I0 {
  constructor(e, n, r) {
    this.$from = e, this.$to = n, this.unplaced = r, this.frontier = [], this.placed = T.empty;
    for (let i = 0; i <= e.depth; i++) {
      let o = e.node(i);
      this.frontier.push({
        type: o.type,
        match: o.contentMatchAt(e.indexAfter(i))
      });
    }
    for (let i = e.depth; i > 0; i--)
      this.placed = T.from(e.node(i).copy(this.placed));
  }
  get depth() {
    return this.frontier.length - 1;
  }
  fit() {
    for (; this.unplaced.size; ) {
      let c = this.findFittable();
      c ? this.placeNodes(c) : this.openMore() || this.dropNode();
    }
    let e = this.mustMoveInline(), n = this.placed.size - this.depth - this.$from.depth, r = this.$from, i = this.close(e < 0 ? this.$to : r.doc.resolve(e));
    if (!i)
      return null;
    let o = this.placed, s = r.depth, l = i.depth;
    for (; s && l && o.childCount == 1; )
      o = o.firstChild.content, s--, l--;
    let a = new E(o, s, l);
    return e > -1 ? new Ae(r.pos, e, this.$to.pos, this.$to.end(), a, n) : a.size || r.pos != this.$to.pos ? new xe(r.pos, i.pos, a) : null;
  }
  // Find a position on the start spine of `this.unplaced` that has
  // content that can be moved somewhere on the frontier. Returns two
  // depths, one for the slice and one for the frontier.
  findFittable() {
    let e = this.unplaced.openStart;
    for (let n = this.unplaced.content, r = 0, i = this.unplaced.openEnd; r < e; r++) {
      let o = n.firstChild;
      if (n.childCount > 1 && (i = 0), o.type.spec.isolating && i <= r) {
        e = r;
        break;
      }
      n = o.content;
    }
    for (let n = 1; n <= 2; n++)
      for (let r = n == 1 ? e : this.unplaced.openStart; r >= 0; r--) {
        let i, o = null;
        r ? (o = $s(this.unplaced.content, r - 1).firstChild, i = o.content) : i = this.unplaced.content;
        let s = i.firstChild;
        for (let l = this.depth; l >= 0; l--) {
          let { type: a, match: c } = this.frontier[l], u, h = null;
          if (n == 1 && (s ? c.matchType(s.type) || (h = c.fillBefore(T.from(s), !1)) : o && a.compatibleContent(o.type)))
            return { sliceDepth: r, frontierDepth: l, parent: o, inject: h };
          if (n == 2 && s && (u = c.findWrapping(s.type)))
            return { sliceDepth: r, frontierDepth: l, parent: o, wrap: u };
          if (o && c.matchType(o.type))
            break;
        }
      }
  }
  openMore() {
    let { content: e, openStart: n, openEnd: r } = this.unplaced, i = $s(e, n);
    return !i.childCount || i.firstChild.isLeaf ? !1 : (this.unplaced = new E(e, n + 1, Math.max(r, i.size + n >= e.size - r ? n + 1 : 0)), !0);
  }
  dropNode() {
    let { content: e, openStart: n, openEnd: r } = this.unplaced, i = $s(e, n);
    if (i.childCount <= 1 && n > 0) {
      let o = e.size - n <= n + i.size;
      this.unplaced = new E(Zr(e, n - 1, 1), n - 1, o ? n - 1 : r);
    } else
      this.unplaced = new E(Zr(e, n, 1), n, r);
  }
  // Move content from the unplaced slice at `sliceDepth` to the
  // frontier node at `frontierDepth`. Close that frontier node when
  // applicable.
  placeNodes({ sliceDepth: e, frontierDepth: n, parent: r, inject: i, wrap: o }) {
    for (; this.depth > n; )
      this.closeFrontierNode();
    if (o)
      for (let m = 0; m < o.length; m++)
        this.openFrontierNode(o[m]);
    let s = this.unplaced, l = r ? r.content : s.content, a = s.openStart - e, c = 0, u = [], { match: h, type: f } = this.frontier[n];
    if (i) {
      for (let m = 0; m < i.childCount; m++)
        u.push(i.child(m));
      h = h.matchFragment(i);
    }
    let d = l.size + e - (s.content.size - s.openEnd);
    for (; c < l.childCount; ) {
      let m = l.child(c), y = h.matchType(m.type);
      if (!y)
        break;
      c++, (c > 1 || a == 0 || m.content.size) && (h = y, u.push(Md(m.mark(f.allowedMarks(m.marks)), c == 1 ? a : 0, c == l.childCount ? d : -1)));
    }
    let p = c == l.childCount;
    p || (d = -1), this.placed = ei(this.placed, n, T.from(u)), this.frontier[n].match = h, p && d < 0 && r && r.type == this.frontier[this.depth].type && this.frontier.length > 1 && this.closeFrontierNode();
    for (let m = 0, y = l; m < d; m++) {
      let g = y.lastChild;
      this.frontier.push({ type: g.type, match: g.contentMatchAt(g.childCount) }), y = g.content;
    }
    this.unplaced = p ? e == 0 ? E.empty : new E(Zr(s.content, e - 1, 1), e - 1, d < 0 ? s.openEnd : e - 1) : new E(Zr(s.content, e, c), s.openStart, s.openEnd);
  }
  mustMoveInline() {
    if (!this.$to.parent.isTextblock)
      return -1;
    let e = this.frontier[this.depth], n;
    if (!e.type.isTextblock || !Vs(this.$to, this.$to.depth, e.type, e.match, !1) || this.$to.depth == this.depth && (n = this.findCloseLevel(this.$to)) && n.depth == this.depth)
      return -1;
    let { depth: r } = this.$to, i = this.$to.after(r);
    for (; r > 1 && i == this.$to.end(--r); )
      ++i;
    return i;
  }
  findCloseLevel(e) {
    e: for (let n = Math.min(this.depth, e.depth); n >= 0; n--) {
      let { match: r, type: i } = this.frontier[n], o = n < e.depth && e.end(n + 1) == e.pos + (e.depth - (n + 1)), s = Vs(e, n, i, r, o);
      if (s) {
        for (let l = n - 1; l >= 0; l--) {
          let { match: a, type: c } = this.frontier[l], u = Vs(e, l, c, a, !0);
          if (!u || u.childCount)
            continue e;
        }
        return { depth: n, fit: s, move: o ? e.doc.resolve(e.after(n + 1)) : e };
      }
    }
  }
  close(e) {
    let n = this.findCloseLevel(e);
    if (!n)
      return null;
    for (; this.depth > n.depth; )
      this.closeFrontierNode();
    n.fit.childCount && (this.placed = ei(this.placed, n.depth, n.fit)), e = n.move;
    for (let r = n.depth + 1; r <= e.depth; r++) {
      let i = e.node(r), o = i.type.contentMatch.fillBefore(i.content, !0, e.index(r));
      this.openFrontierNode(i.type, i.attrs, o);
    }
    return e;
  }
  openFrontierNode(e, n = null, r) {
    let i = this.frontier[this.depth];
    i.match = i.match.matchType(e), this.placed = ei(this.placed, this.depth, T.from(e.create(n, r))), this.frontier.push({ type: e, match: e.contentMatch });
  }
  closeFrontierNode() {
    let n = this.frontier.pop().match.fillBefore(T.empty, !0);
    n.childCount && (this.placed = ei(this.placed, this.frontier.length, n));
  }
}
function Zr(t, e, n) {
  return e == 0 ? t.cutByIndex(n, t.childCount) : t.replaceChild(0, t.firstChild.copy(Zr(t.firstChild.content, e - 1, n)));
}
function ei(t, e, n) {
  return e == 0 ? t.append(n) : t.replaceChild(t.childCount - 1, t.lastChild.copy(ei(t.lastChild.content, e - 1, n)));
}
function $s(t, e) {
  for (let n = 0; n < e; n++)
    t = t.firstChild.content;
  return t;
}
function Md(t, e, n) {
  if (e <= 0)
    return t;
  let r = t.content;
  return e > 1 && (r = r.replaceChild(0, Md(r.firstChild, e - 1, r.childCount == 1 ? n - 1 : 0))), e > 0 && (r = t.type.contentMatch.fillBefore(r).append(r), n <= 0 && (r = r.append(t.type.contentMatch.matchFragment(r).fillBefore(T.empty, !0)))), t.copy(r);
}
function Vs(t, e, n, r, i) {
  let o = t.node(e), s = i ? t.indexAfter(e) : t.index(e);
  if (s == o.childCount && !n.compatibleContent(o.type))
    return null;
  let l = r.fillBefore(o.content, !0, s);
  return l && !A0(n, o.content, s) ? l : null;
}
function A0(t, e, n) {
  for (let r = n; r < e.childCount; r++)
    if (!t.allowsMarks(e.child(r).marks))
      return !0;
  return !1;
}
function E0(t) {
  return t.spec.defining || t.spec.definingForContent;
}
function O0(t, e, n, r) {
  if (!r.size)
    return t.deleteRange(e, n);
  let i = t.doc.resolve(e), o = t.doc.resolve(n);
  if (Sd(i, o, r))
    return t.step(new xe(e, n, r));
  let s = Td(i, t.doc.resolve(n));
  s[s.length - 1] == 0 && s.pop();
  let l = -(i.depth + 1);
  s.unshift(l);
  for (let f = i.depth, d = i.pos - 1; f > 0; f--, d--) {
    let p = i.node(f).type.spec;
    if (p.defining || p.definingAsContext || p.isolating)
      break;
    s.indexOf(f) > -1 ? l = f : i.before(f) == d && s.splice(1, 0, -f);
  }
  let a = s.indexOf(l), c = [], u = r.openStart;
  for (let f = r.content, d = 0; ; d++) {
    let p = f.firstChild;
    if (c.push(p), d == r.openStart)
      break;
    f = p.content;
  }
  for (let f = u - 1; f >= 0; f--) {
    let d = c[f], p = E0(d.type);
    if (p && !d.sameMarkup(i.node(Math.abs(l) - 1)))
      u = f;
    else if (p || !d.type.isTextblock)
      break;
  }
  for (let f = r.openStart; f >= 0; f--) {
    let d = (f + u + 1) % (r.openStart + 1), p = c[d];
    if (p)
      for (let m = 0; m < s.length; m++) {
        let y = s[(m + a) % s.length], g = !0;
        y < 0 && (g = !1, y = -y);
        let N = i.node(y - 1), C = i.index(y - 1);
        if (N.canReplaceWith(C, C, p.type, p.marks))
          return t.replace(i.before(y), g ? o.after(y) : n, new E(Nd(r.content, 0, r.openStart, d), d, r.openEnd));
      }
  }
  let h = t.steps.length;
  for (let f = s.length - 1; f >= 0 && (t.replace(e, n, r), !(t.steps.length > h)); f--) {
    let d = s[f];
    d < 0 || (e = i.before(d), n = o.after(d));
  }
}
function Nd(t, e, n, r, i) {
  if (e < n) {
    let o = t.firstChild;
    t = t.replaceChild(0, o.copy(Nd(o.content, e + 1, n, r, o)));
  }
  if (e > r) {
    let o = i.contentMatchAt(0), s = o.fillBefore(t).append(t);
    t = s.append(o.matchFragment(s).fillBefore(T.empty, !0));
  }
  return t;
}
function D0(t, e, n, r) {
  if (!r.isInline && e == n && t.doc.resolve(e).parent.content.size) {
    let i = N0(t.doc, e, r.type);
    i != null && (e = n = i);
  }
  t.replaceRange(e, n, new E(T.from(r), 0, 0));
}
function R0(t, e, n) {
  let r = t.doc.resolve(e), i = t.doc.resolve(n), o = Td(r, i);
  for (let s = 0; s < o.length; s++) {
    let l = o[s], a = s == o.length - 1;
    if (a && l == 0 || r.node(l).type.contentMatch.validEnd)
      return t.delete(r.start(l), i.end(l));
    if (l > 0 && (a || r.node(l - 1).canReplace(r.index(l - 1), i.indexAfter(l - 1))))
      return t.delete(r.before(l), i.after(l));
  }
  for (let s = 1; s <= r.depth && s <= i.depth; s++)
    if (e - r.start(s) == r.depth - s && n > r.end(s) && i.end(s) - n != i.depth - s && r.start(s - 1) == i.start(s - 1) && r.node(s - 1).canReplace(r.index(s - 1), i.index(s - 1)))
      return t.delete(r.before(s), n);
  t.delete(e, n);
}
function Td(t, e) {
  let n = [], r = Math.min(t.depth, e.depth);
  for (let i = r; i >= 0; i--) {
    let o = t.start(i);
    if (o < t.pos - (t.depth - i) || e.end(i) > e.pos + (e.depth - i) || t.node(i).type.spec.isolating || e.node(i).type.spec.isolating)
      break;
    (o == e.start(i) || i == t.depth && i == e.depth && t.parent.inlineContent && e.parent.inlineContent && i && e.start(i - 1) == o - 1) && n.push(i);
  }
  return n;
}
class pr extends Ee {
  /**
  Construct an attribute step.
  */
  constructor(e, n, r) {
    super(), this.pos = e, this.attr = n, this.value = r;
  }
  apply(e) {
    let n = e.nodeAt(this.pos);
    if (!n)
      return pe.fail("No node at attribute step's position");
    let r = /* @__PURE__ */ Object.create(null);
    for (let o in n.attrs)
      r[o] = n.attrs[o];
    r[this.attr] = this.value;
    let i = n.type.create(r, null, n.marks);
    return pe.fromReplace(e, this.pos, this.pos + 1, new E(T.from(i), 0, n.isLeaf ? 0 : 1));
  }
  getMap() {
    return Ke.empty;
  }
  invert(e) {
    return new pr(this.pos, this.attr, e.nodeAt(this.pos).attrs[this.attr]);
  }
  map(e) {
    let n = e.mapResult(this.pos, 1);
    return n.deletedAfter ? null : new pr(n.pos, this.attr, this.value);
  }
  toJSON() {
    return { stepType: "attr", pos: this.pos, attr: this.attr, value: this.value };
  }
  static fromJSON(e, n) {
    if (typeof n.pos != "number" || typeof n.attr != "string")
      throw new RangeError("Invalid input for AttrStep.fromJSON");
    return new pr(n.pos, n.attr, n.value);
  }
}
Ee.jsonID("attr", pr);
class wi extends Ee {
  /**
  Construct an attribute step.
  */
  constructor(e, n) {
    super(), this.attr = e, this.value = n;
  }
  apply(e) {
    let n = /* @__PURE__ */ Object.create(null);
    for (let i in e.attrs)
      n[i] = e.attrs[i];
    n[this.attr] = this.value;
    let r = e.type.create(n, e.content, e.marks);
    return pe.ok(r);
  }
  getMap() {
    return Ke.empty;
  }
  invert(e) {
    return new wi(this.attr, e.attrs[this.attr]);
  }
  map(e) {
    return this;
  }
  toJSON() {
    return { stepType: "docAttr", attr: this.attr, value: this.value };
  }
  static fromJSON(e, n) {
    if (typeof n.attr != "string")
      throw new RangeError("Invalid input for DocAttrStep.fromJSON");
    return new wi(n.attr, n.value);
  }
}
Ee.jsonID("docAttr", wi);
let Sr = class extends Error {
};
Sr = function t(e) {
  let n = Error.call(this, e);
  return n.__proto__ = t.prototype, n;
};
Sr.prototype = Object.create(Error.prototype);
Sr.prototype.constructor = Sr;
Sr.prototype.name = "TransformError";
class Id {
  /**
  Create a transform that starts with the given document.
  */
  constructor(e) {
    this.doc = e, this.steps = [], this.docs = [], this.mapping = new bi();
  }
  /**
  The starting document.
  */
  get before() {
    return this.docs.length ? this.docs[0] : this.doc;
  }
  /**
  Apply a new step in this transform, saving the result. Throws an
  error when the step fails.
  */
  step(e) {
    let n = this.maybeStep(e);
    if (n.failed)
      throw new Sr(n.failed);
    return this;
  }
  /**
  Try to apply a step in this transformation, ignoring it if it
  fails. Returns the step result.
  */
  maybeStep(e) {
    let n = e.apply(this.doc);
    return n.failed || this.addStep(e, n.doc), n;
  }
  /**
  True when the document has been changed (when there are any
  steps).
  */
  get docChanged() {
    return this.steps.length > 0;
  }
  /**
  @internal
  */
  addStep(e, n) {
    this.docs.push(this.doc), this.steps.push(e), this.mapping.appendMap(e.getMap()), this.doc = n;
  }
  /**
  Replace the part of the document between `from` and `to` with the
  given `slice`.
  */
  replace(e, n = e, r = E.empty) {
    let i = xa(this.doc, e, n, r);
    return i && this.step(i), this;
  }
  /**
  Replace the given range with the given content, which may be a
  fragment, node, or array of nodes.
  */
  replaceWith(e, n, r) {
    return this.replace(e, n, new E(T.from(r), 0, 0));
  }
  /**
  Delete the content between the given positions.
  */
  delete(e, n) {
    return this.replace(e, n, E.empty);
  }
  /**
  Insert the given content at the given position.
  */
  insert(e, n) {
    return this.replaceWith(e, e, n);
  }
  /**
  Replace a range of the document with a given slice, using
  `from`, `to`, and the slice's
  [`openStart`](https://prosemirror.net/docs/ref/#model.Slice.openStart) property as hints, rather
  than fixed start and end points. This method may grow the
  replaced area or close open nodes in the slice in order to get a
  fit that is more in line with WYSIWYG expectations, by dropping
  fully covered parent nodes of the replaced region when they are
  marked [non-defining as
  context](https://prosemirror.net/docs/ref/#model.NodeSpec.definingAsContext), or including an
  open parent node from the slice that _is_ marked as [defining
  its content](https://prosemirror.net/docs/ref/#model.NodeSpec.definingForContent).
  
  This is the method, for example, to handle paste. The similar
  [`replace`](https://prosemirror.net/docs/ref/#transform.Transform.replace) method is a more
  primitive tool which will _not_ move the start and end of its given
  range, and is useful in situations where you need more precise
  control over what happens.
  */
  replaceRange(e, n, r) {
    return O0(this, e, n, r), this;
  }
  /**
  Replace the given range with a node, but use `from` and `to` as
  hints, rather than precise positions. When from and to are the same
  and are at the start or end of a parent node in which the given
  node doesn't fit, this method may _move_ them out towards a parent
  that does allow the given node to be placed. When the given range
  completely covers a parent node, this method may completely replace
  that parent node.
  */
  replaceRangeWith(e, n, r) {
    return D0(this, e, n, r), this;
  }
  /**
  Delete the given range, expanding it to cover fully covered
  parent nodes until a valid replace is found.
  */
  deleteRange(e, n) {
    return R0(this, e, n), this;
  }
  /**
  Split the content in the given range off from its parent, if there
  is sibling content before or after it, and move it up the tree to
  the depth specified by `target`. You'll probably want to use
  [`liftTarget`](https://prosemirror.net/docs/ref/#transform.liftTarget) to compute `target`, to make
  sure the lift is valid.
  */
  lift(e, n) {
    return p0(this, e, n), this;
  }
  /**
  Join the blocks around the given position. If depth is 2, their
  last and first siblings are also joined, and so on.
  */
  join(e, n = 1) {
    return M0(this, e, n), this;
  }
  /**
  Wrap the given [range](https://prosemirror.net/docs/ref/#model.NodeRange) in the given set of wrappers.
  The wrappers are assumed to be valid in this position, and should
  probably be computed with [`findWrapping`](https://prosemirror.net/docs/ref/#transform.findWrapping).
  */
  wrap(e, n) {
    return y0(this, e, n), this;
  }
  /**
  Set the type of all textblocks (partly) between `from` and `to` to
  the given node type with the given attributes.
  */
  setBlockType(e, n = e, r, i = null) {
    return k0(this, e, n, r, i), this;
  }
  /**
  Change the type, attributes, and/or marks of the node at `pos`.
  When `type` isn't given, the existing node type is preserved,
  */
  setNodeMarkup(e, n, r = null, i) {
    return w0(this, e, n, r, i), this;
  }
  /**
  Set a single attribute on a given node to a new value.
  The `pos` addresses the document content. Use `setDocAttribute`
  to set attributes on the document itself.
  */
  setNodeAttribute(e, n, r) {
    return this.step(new pr(e, n, r)), this;
  }
  /**
  Set a single attribute on the document to a new value.
  */
  setDocAttribute(e, n) {
    return this.step(new wi(e, n)), this;
  }
  /**
  Add a mark to the node at position `pos`.
  */
  addNodeMark(e, n) {
    return this.step(new on(e, n)), this;
  }
  /**
  Remove a mark (or a mark of the given type) from the node at
  position `pos`.
  */
  removeNodeMark(e, n) {
    if (!(n instanceof G)) {
      let r = this.doc.nodeAt(e);
      if (!r)
        throw new RangeError("No node at position " + e);
      if (n = n.isInSet(r.marks), !n)
        return this;
    }
    return this.step(new Cr(e, n)), this;
  }
  /**
  Split the node at the given position, and optionally, if `depth` is
  greater than one, any number of nodes above that. By default, the
  parts split off will inherit the node type of the original node.
  This can be changed by passing an array of types and attributes to
  use after the split (with the outermost nodes coming first).
  */
  split(e, n = 1, r) {
    return x0(this, e, n, r), this;
  }
  /**
  Add the given mark to the inline content between `from` and `to`.
  */
  addMark(e, n, r) {
    return h0(this, e, n, r), this;
  }
  /**
  Remove marks from inline nodes between `from` and `to`. When
  `mark` is a single mark, remove precisely that mark. When it is
  a mark type, remove all marks of that type. When it is null,
  remove all marks of any type.
  */
  removeMark(e, n, r) {
    return f0(this, e, n, r), this;
  }
  /**
  Removes all marks and nodes from the content of the node at
  `pos` that don't match the given new parent node type. Accepts
  an optional starting [content match](https://prosemirror.net/docs/ref/#model.ContentMatch) as
  third argument.
  */
  clearIncompatible(e, n, r) {
    return wa(this, e, n, r), this;
  }
}
const Ws = /* @__PURE__ */ Object.create(null);
class $ {
  /**
  Initialize a selection with the head and anchor and ranges. If no
  ranges are given, constructs a single range across `$anchor` and
  `$head`.
  */
  constructor(e, n, r) {
    this.$anchor = e, this.$head = n, this.ranges = r || [new Ad(e.min(n), e.max(n))];
  }
  /**
  The selection's anchor, as an unresolved position.
  */
  get anchor() {
    return this.$anchor.pos;
  }
  /**
  The selection's head.
  */
  get head() {
    return this.$head.pos;
  }
  /**
  The lower bound of the selection's main range.
  */
  get from() {
    return this.$from.pos;
  }
  /**
  The upper bound of the selection's main range.
  */
  get to() {
    return this.$to.pos;
  }
  /**
  The resolved lower  bound of the selection's main range.
  */
  get $from() {
    return this.ranges[0].$from;
  }
  /**
  The resolved upper bound of the selection's main range.
  */
  get $to() {
    return this.ranges[0].$to;
  }
  /**
  Indicates whether the selection contains any content.
  */
  get empty() {
    let e = this.ranges;
    for (let n = 0; n < e.length; n++)
      if (e[n].$from.pos != e[n].$to.pos)
        return !1;
    return !0;
  }
  /**
  Get the content of this selection as a slice.
  */
  content() {
    return this.$from.doc.slice(this.from, this.to, !0);
  }
  /**
  Replace the selection with a slice or, if no slice is given,
  delete the selection. Will append to the given transaction.
  */
  replace(e, n = E.empty) {
    let r = n.content.lastChild, i = null;
    for (let l = 0; l < n.openEnd; l++)
      i = r, r = r.lastChild;
    let o = e.steps.length, s = this.ranges;
    for (let l = 0; l < s.length; l++) {
      let { $from: a, $to: c } = s[l], u = e.mapping.slice(o);
      e.replaceRange(u.map(a.pos), u.map(c.pos), l ? E.empty : n), l == 0 && Ku(e, o, (r ? r.isInline : i && i.isTextblock) ? -1 : 1);
    }
  }
  /**
  Replace the selection with the given node, appending the changes
  to the given transaction.
  */
  replaceWith(e, n) {
    let r = e.steps.length, i = this.ranges;
    for (let o = 0; o < i.length; o++) {
      let { $from: s, $to: l } = i[o], a = e.mapping.slice(r), c = a.map(s.pos), u = a.map(l.pos);
      o ? e.deleteRange(c, u) : (e.replaceRangeWith(c, u, n), Ku(e, r, n.isInline ? -1 : 1));
    }
  }
  /**
  Find a valid cursor or leaf node selection starting at the given
  position and searching back if `dir` is negative, and forward if
  positive. When `textOnly` is true, only consider cursor
  selections. Will return null when no valid selection position is
  found.
  */
  static findFrom(e, n, r = !1) {
    let i = e.parent.inlineContent ? new H(e) : rr(e.node(0), e.parent, e.pos, e.index(), n, r);
    if (i)
      return i;
    for (let o = e.depth - 1; o >= 0; o--) {
      let s = n < 0 ? rr(e.node(0), e.node(o), e.before(o + 1), e.index(o), n, r) : rr(e.node(0), e.node(o), e.after(o + 1), e.index(o) + 1, n, r);
      if (s)
        return s;
    }
    return null;
  }
  /**
  Find a valid cursor or leaf node selection near the given
  position. Searches forward first by default, but if `bias` is
  negative, it will search backwards first.
  */
  static near(e, n = 1) {
    return this.findFrom(e, n) || this.findFrom(e, -n) || new Be(e.node(0));
  }
  /**
  Find the cursor or leaf node selection closest to the start of
  the given document. Will return an
  [`AllSelection`](https://prosemirror.net/docs/ref/#state.AllSelection) if no valid position
  exists.
  */
  static atStart(e) {
    return rr(e, e, 0, 0, 1) || new Be(e);
  }
  /**
  Find the cursor or leaf node selection closest to the end of the
  given document.
  */
  static atEnd(e) {
    return rr(e, e, e.content.size, e.childCount, -1) || new Be(e);
  }
  /**
  Deserialize the JSON representation of a selection. Must be
  implemented for custom classes (as a static class method).
  */
  static fromJSON(e, n) {
    if (!n || !n.type)
      throw new RangeError("Invalid input for Selection.fromJSON");
    let r = Ws[n.type];
    if (!r)
      throw new RangeError(`No selection type ${n.type} defined`);
    return r.fromJSON(e, n);
  }
  /**
  To be able to deserialize selections from JSON, custom selection
  classes must register themselves with an ID string, so that they
  can be disambiguated. Try to pick something that's unlikely to
  clash with classes from other modules.
  */
  static jsonID(e, n) {
    if (e in Ws)
      throw new RangeError("Duplicate use of selection JSON ID " + e);
    return Ws[e] = n, n.prototype.jsonID = e, n;
  }
  /**
  Get a [bookmark](https://prosemirror.net/docs/ref/#state.SelectionBookmark) for this selection,
  which is a value that can be mapped without having access to a
  current document, and later resolved to a real selection for a
  given document again. (This is used mostly by the history to
  track and restore old selections.) The default implementation of
  this method just converts the selection to a text selection and
  returns the bookmark for that.
  */
  getBookmark() {
    return H.between(this.$anchor, this.$head).getBookmark();
  }
}
$.prototype.visible = !0;
class Ad {
  /**
  Create a range.
  */
  constructor(e, n) {
    this.$from = e, this.$to = n;
  }
}
let ju = !1;
function qu(t) {
  !ju && !t.parent.inlineContent && (ju = !0, console.warn("TextSelection endpoint not pointing into a node with inline content (" + t.parent.type.name + ")"));
}
class H extends $ {
  /**
  Construct a text selection between the given points.
  */
  constructor(e, n = e) {
    qu(e), qu(n), super(e, n);
  }
  /**
  Returns a resolved position if this is a cursor selection (an
  empty text selection), and null otherwise.
  */
  get $cursor() {
    return this.$anchor.pos == this.$head.pos ? this.$head : null;
  }
  map(e, n) {
    let r = e.resolve(n.map(this.head));
    if (!r.parent.inlineContent)
      return $.near(r);
    let i = e.resolve(n.map(this.anchor));
    return new H(i.parent.inlineContent ? i : r, r);
  }
  replace(e, n = E.empty) {
    if (super.replace(e, n), n == E.empty) {
      let r = this.$from.marksAcross(this.$to);
      r && e.ensureMarks(r);
    }
  }
  eq(e) {
    return e instanceof H && e.anchor == this.anchor && e.head == this.head;
  }
  getBookmark() {
    return new fs(this.anchor, this.head);
  }
  toJSON() {
    return { type: "text", anchor: this.anchor, head: this.head };
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.anchor != "number" || typeof n.head != "number")
      throw new RangeError("Invalid input for TextSelection.fromJSON");
    return new H(e.resolve(n.anchor), e.resolve(n.head));
  }
  /**
  Create a text selection from non-resolved positions.
  */
  static create(e, n, r = n) {
    let i = e.resolve(n);
    return new this(i, r == n ? i : e.resolve(r));
  }
  /**
  Return a text selection that spans the given positions or, if
  they aren't text positions, find a text selection near them.
  `bias` determines whether the method searches forward (default)
  or backwards (negative number) first. Will fall back to calling
  [`Selection.near`](https://prosemirror.net/docs/ref/#state.Selection^near) when the document
  doesn't contain a valid text position.
  */
  static between(e, n, r) {
    let i = e.pos - n.pos;
    if ((!r || i) && (r = i >= 0 ? 1 : -1), !n.parent.inlineContent) {
      let o = $.findFrom(n, r, !0) || $.findFrom(n, -r, !0);
      if (o)
        n = o.$head;
      else
        return $.near(n, r);
    }
    return e.parent.inlineContent || (i == 0 ? e = n : (e = ($.findFrom(e, -r, !0) || $.findFrom(e, r, !0)).$anchor, e.pos < n.pos != i < 0 && (e = n))), new H(e, n);
  }
}
$.jsonID("text", H);
class fs {
  constructor(e, n) {
    this.anchor = e, this.head = n;
  }
  map(e) {
    return new fs(e.map(this.anchor), e.map(this.head));
  }
  resolve(e) {
    return H.between(e.resolve(this.anchor), e.resolve(this.head));
  }
}
class _ extends $ {
  /**
  Create a node selection. Does not verify the validity of its
  argument.
  */
  constructor(e) {
    let n = e.nodeAfter, r = e.node(0).resolve(e.pos + n.nodeSize);
    super(e, r), this.node = n;
  }
  map(e, n) {
    let { deleted: r, pos: i } = n.mapResult(this.anchor), o = e.resolve(i);
    return r ? $.near(o) : new _(o);
  }
  content() {
    return new E(T.from(this.node), 0, 0);
  }
  eq(e) {
    return e instanceof _ && e.anchor == this.anchor;
  }
  toJSON() {
    return { type: "node", anchor: this.anchor };
  }
  getBookmark() {
    return new Ca(this.anchor);
  }
  /**
  @internal
  */
  static fromJSON(e, n) {
    if (typeof n.anchor != "number")
      throw new RangeError("Invalid input for NodeSelection.fromJSON");
    return new _(e.resolve(n.anchor));
  }
  /**
  Create a node selection from non-resolved positions.
  */
  static create(e, n) {
    return new _(e.resolve(n));
  }
  /**
  Determines whether the given node may be selected as a node
  selection.
  */
  static isSelectable(e) {
    return !e.isText && e.type.spec.selectable !== !1;
  }
}
_.prototype.visible = !1;
$.jsonID("node", _);
class Ca {
  constructor(e) {
    this.anchor = e;
  }
  map(e) {
    let { deleted: n, pos: r } = e.mapResult(this.anchor);
    return n ? new fs(r, r) : new Ca(r);
  }
  resolve(e) {
    let n = e.resolve(this.anchor), r = n.nodeAfter;
    return r && _.isSelectable(r) ? new _(n) : $.near(n);
  }
}
class Be extends $ {
  /**
  Create an all-selection over the given document.
  */
  constructor(e) {
    super(e.resolve(0), e.resolve(e.content.size));
  }
  replace(e, n = E.empty) {
    if (n == E.empty) {
      e.delete(0, e.doc.content.size);
      let r = $.atStart(e.doc);
      r.eq(e.selection) || e.setSelection(r);
    } else
      super.replace(e, n);
  }
  toJSON() {
    return { type: "all" };
  }
  /**
  @internal
  */
  static fromJSON(e) {
    return new Be(e);
  }
  map(e) {
    return new Be(e);
  }
  eq(e) {
    return e instanceof Be;
  }
  getBookmark() {
    return v0;
  }
}
$.jsonID("all", Be);
const v0 = {
  map() {
    return this;
  },
  resolve(t) {
    return new Be(t);
  }
};
function rr(t, e, n, r, i, o = !1) {
  if (e.inlineContent)
    return H.create(t, n);
  for (let s = r - (i > 0 ? 0 : 1); i > 0 ? s < e.childCount : s >= 0; s += i) {
    let l = e.child(s);
    if (l.isAtom) {
      if (!o && _.isSelectable(l))
        return _.create(t, n - (i < 0 ? l.nodeSize : 0));
    } else {
      let a = rr(t, l, n + i, i < 0 ? l.childCount : 0, i, o);
      if (a)
        return a;
    }
    n += l.nodeSize * i;
  }
  return null;
}
function Ku(t, e, n) {
  let r = t.steps.length - 1;
  if (r < e)
    return;
  let i = t.steps[r];
  if (!(i instanceof xe || i instanceof Ae))
    return;
  let o = t.mapping.maps[r], s;
  o.forEach((l, a, c, u) => {
    s == null && (s = u);
  }), t.setSelection($.near(t.doc.resolve(s), n));
}
const Ju = 1, Uu = 2, Gu = 4;
class P0 extends Id {
  /**
  @internal
  */
  constructor(e) {
    super(e.doc), this.curSelectionFor = 0, this.updated = 0, this.meta = /* @__PURE__ */ Object.create(null), this.time = Date.now(), this.curSelection = e.selection, this.storedMarks = e.storedMarks;
  }
  /**
  The transaction's current selection. This defaults to the editor
  selection [mapped](https://prosemirror.net/docs/ref/#state.Selection.map) through the steps in the
  transaction, but can be overwritten with
  [`setSelection`](https://prosemirror.net/docs/ref/#state.Transaction.setSelection).
  */
  get selection() {
    return this.curSelectionFor < this.steps.length && (this.curSelection = this.curSelection.map(this.doc, this.mapping.slice(this.curSelectionFor)), this.curSelectionFor = this.steps.length), this.curSelection;
  }
  /**
  Update the transaction's current selection. Will determine the
  selection that the editor gets when the transaction is applied.
  */
  setSelection(e) {
    if (e.$from.doc != this.doc)
      throw new RangeError("Selection passed to setSelection must point at the current document");
    return this.curSelection = e, this.curSelectionFor = this.steps.length, this.updated = (this.updated | Ju) & -3, this.storedMarks = null, this;
  }
  /**
  Whether the selection was explicitly updated by this transaction.
  */
  get selectionSet() {
    return (this.updated & Ju) > 0;
  }
  /**
  Set the current stored marks.
  */
  setStoredMarks(e) {
    return this.storedMarks = e, this.updated |= Uu, this;
  }
  /**
  Make sure the current stored marks or, if that is null, the marks
  at the selection, match the given set of marks. Does nothing if
  this is already the case.
  */
  ensureMarks(e) {
    return G.sameSet(this.storedMarks || this.selection.$from.marks(), e) || this.setStoredMarks(e), this;
  }
  /**
  Add a mark to the set of stored marks.
  */
  addStoredMark(e) {
    return this.ensureMarks(e.addToSet(this.storedMarks || this.selection.$head.marks()));
  }
  /**
  Remove a mark or mark type from the set of stored marks.
  */
  removeStoredMark(e) {
    return this.ensureMarks(e.removeFromSet(this.storedMarks || this.selection.$head.marks()));
  }
  /**
  Whether the stored marks were explicitly set for this transaction.
  */
  get storedMarksSet() {
    return (this.updated & Uu) > 0;
  }
  /**
  @internal
  */
  addStep(e, n) {
    super.addStep(e, n), this.updated = this.updated & -3, this.storedMarks = null;
  }
  /**
  Update the timestamp for the transaction.
  */
  setTime(e) {
    return this.time = e, this;
  }
  /**
  Replace the current selection with the given slice.
  */
  replaceSelection(e) {
    return this.selection.replace(this, e), this;
  }
  /**
  Replace the selection with the given node. When `inheritMarks` is
  true and the content is inline, it inherits the marks from the
  place where it is inserted.
  */
  replaceSelectionWith(e, n = !0) {
    let r = this.selection;
    return n && (e = e.mark(this.storedMarks || (r.empty ? r.$from.marks() : r.$from.marksAcross(r.$to) || G.none))), r.replaceWith(this, e), this;
  }
  /**
  Delete the selection.
  */
  deleteSelection() {
    return this.selection.replace(this), this;
  }
  /**
  Replace the given range, or the selection if no range is given,
  with a text node containing the given string.
  */
  insertText(e, n, r) {
    let i = this.doc.type.schema;
    if (n == null)
      return e ? this.replaceSelectionWith(i.text(e), !0) : this.deleteSelection();
    {
      if (r == null && (r = n), r = r ?? n, !e)
        return this.deleteRange(n, r);
      let o = this.storedMarks;
      if (!o) {
        let s = this.doc.resolve(n);
        o = r == n ? s.marks() : s.marksAcross(this.doc.resolve(r));
      }
      return this.replaceRangeWith(n, r, i.text(e, o)), this.selection.empty || this.setSelection($.near(this.selection.$to)), this;
    }
  }
  /**
  Store a metadata property in this transaction, keyed either by
  name or by plugin.
  */
  setMeta(e, n) {
    return this.meta[typeof e == "string" ? e : e.key] = n, this;
  }
  /**
  Retrieve a metadata property for a given name or plugin.
  */
  getMeta(e) {
    return this.meta[typeof e == "string" ? e : e.key];
  }
  /**
  Returns true if this transaction doesn't contain any metadata,
  and can thus safely be extended.
  */
  get isGeneric() {
    for (let e in this.meta)
      return !1;
    return !0;
  }
  /**
  Indicate that the editor should scroll the selection into view
  when updated to the state produced by this transaction.
  */
  scrollIntoView() {
    return this.updated |= Gu, this;
  }
  /**
  True when this transaction has had `scrollIntoView` called on it.
  */
  get scrolledIntoView() {
    return (this.updated & Gu) > 0;
  }
}
function Yu(t, e) {
  return !e || !t ? t : t.bind(e);
}
class ti {
  constructor(e, n, r) {
    this.name = e, this.init = Yu(n.init, r), this.apply = Yu(n.apply, r);
  }
}
const z0 = [
  new ti("doc", {
    init(t) {
      return t.doc || t.schema.topNodeType.createAndFill();
    },
    apply(t) {
      return t.doc;
    }
  }),
  new ti("selection", {
    init(t, e) {
      return t.selection || $.atStart(e.doc);
    },
    apply(t) {
      return t.selection;
    }
  }),
  new ti("storedMarks", {
    init(t) {
      return t.storedMarks || null;
    },
    apply(t, e, n, r) {
      return r.selection.$cursor ? t.storedMarks : null;
    }
  }),
  new ti("scrollToSelection", {
    init() {
      return 0;
    },
    apply(t, e) {
      return t.scrolledIntoView ? e + 1 : e;
    }
  })
];
class Hs {
  constructor(e, n) {
    this.schema = e, this.plugins = [], this.pluginsByKey = /* @__PURE__ */ Object.create(null), this.fields = z0.slice(), n && n.forEach((r) => {
      if (this.pluginsByKey[r.key])
        throw new RangeError("Adding different instances of a keyed plugin (" + r.key + ")");
      this.plugins.push(r), this.pluginsByKey[r.key] = r, r.spec.state && this.fields.push(new ti(r.key, r.spec.state, r));
    });
  }
}
class fr {
  /**
  @internal
  */
  constructor(e) {
    this.config = e;
  }
  /**
  The schema of the state's document.
  */
  get schema() {
    return this.config.schema;
  }
  /**
  The plugins that are active in this state.
  */
  get plugins() {
    return this.config.plugins;
  }
  /**
  Apply the given transaction to produce a new state.
  */
  apply(e) {
    return this.applyTransaction(e).state;
  }
  /**
  @internal
  */
  filterTransaction(e, n = -1) {
    for (let r = 0; r < this.config.plugins.length; r++)
      if (r != n) {
        let i = this.config.plugins[r];
        if (i.spec.filterTransaction && !i.spec.filterTransaction.call(i, e, this))
          return !1;
      }
    return !0;
  }
  /**
  Verbose variant of [`apply`](https://prosemirror.net/docs/ref/#state.EditorState.apply) that
  returns the precise transactions that were applied (which might
  be influenced by the [transaction
  hooks](https://prosemirror.net/docs/ref/#state.PluginSpec.filterTransaction) of
  plugins) along with the new state.
  */
  applyTransaction(e) {
    if (!this.filterTransaction(e))
      return { state: this, transactions: [] };
    let n = [e], r = this.applyInner(e), i = null;
    for (; ; ) {
      let o = !1;
      for (let s = 0; s < this.config.plugins.length; s++) {
        let l = this.config.plugins[s];
        if (l.spec.appendTransaction) {
          let a = i ? i[s].n : 0, c = i ? i[s].state : this, u = a < n.length && l.spec.appendTransaction.call(l, a ? n.slice(a) : n, c, r);
          if (u && r.filterTransaction(u, s)) {
            if (u.setMeta("appendedTransaction", e), !i) {
              i = [];
              for (let h = 0; h < this.config.plugins.length; h++)
                i.push(h < s ? { state: r, n: n.length } : { state: this, n: 0 });
            }
            n.push(u), r = r.applyInner(u), o = !0;
          }
          i && (i[s] = { state: r, n: n.length });
        }
      }
      if (!o)
        return { state: r, transactions: n };
    }
  }
  /**
  @internal
  */
  applyInner(e) {
    if (!e.before.eq(this.doc))
      throw new RangeError("Applying a mismatched transaction");
    let n = new fr(this.config), r = this.config.fields;
    for (let i = 0; i < r.length; i++) {
      let o = r[i];
      n[o.name] = o.apply(e, this[o.name], this, n);
    }
    return n;
  }
  /**
  Start a [transaction](https://prosemirror.net/docs/ref/#state.Transaction) from this state.
  */
  get tr() {
    return new P0(this);
  }
  /**
  Create a new state.
  */
  static create(e) {
    let n = new Hs(e.doc ? e.doc.type.schema : e.schema, e.plugins), r = new fr(n);
    for (let i = 0; i < n.fields.length; i++)
      r[n.fields[i].name] = n.fields[i].init(e, r);
    return r;
  }
  /**
  Create a new state based on this one, but with an adjusted set
  of active plugins. State fields that exist in both sets of
  plugins are kept unchanged. Those that no longer exist are
  dropped, and those that are new are initialized using their
  [`init`](https://prosemirror.net/docs/ref/#state.StateField.init) method, passing in the new
  configuration object..
  */
  reconfigure(e) {
    let n = new Hs(this.schema, e.plugins), r = n.fields, i = new fr(n);
    for (let o = 0; o < r.length; o++) {
      let s = r[o].name;
      i[s] = this.hasOwnProperty(s) ? this[s] : r[o].init(e, i);
    }
    return i;
  }
  /**
  Serialize this state to JSON. If you want to serialize the state
  of plugins, pass an object mapping property names to use in the
  resulting JSON object to plugin objects. The argument may also be
  a string or number, in which case it is ignored, to support the
  way `JSON.stringify` calls `toString` methods.
  */
  toJSON(e) {
    let n = { doc: this.doc.toJSON(), selection: this.selection.toJSON() };
    if (this.storedMarks && (n.storedMarks = this.storedMarks.map((r) => r.toJSON())), e && typeof e == "object")
      for (let r in e) {
        if (r == "doc" || r == "selection")
          throw new RangeError("The JSON fields `doc` and `selection` are reserved");
        let i = e[r], o = i.spec.state;
        o && o.toJSON && (n[r] = o.toJSON.call(i, this[i.key]));
      }
    return n;
  }
  /**
  Deserialize a JSON representation of a state. `config` should
  have at least a `schema` field, and should contain array of
  plugins to initialize the state with. `pluginFields` can be used
  to deserialize the state of plugins, by associating plugin
  instances with the property names they use in the JSON object.
  */
  static fromJSON(e, n, r) {
    if (!n)
      throw new RangeError("Invalid input for EditorState.fromJSON");
    if (!e.schema)
      throw new RangeError("Required config field 'schema' missing");
    let i = new Hs(e.schema, e.plugins), o = new fr(i);
    return i.fields.forEach((s) => {
      if (s.name == "doc")
        o.doc = dt.fromJSON(e.schema, n.doc);
      else if (s.name == "selection")
        o.selection = $.fromJSON(o.doc, n.selection);
      else if (s.name == "storedMarks")
        n.storedMarks && (o.storedMarks = n.storedMarks.map(e.schema.markFromJSON));
      else {
        if (r)
          for (let l in r) {
            let a = r[l], c = a.spec.state;
            if (a.key == s.name && c && c.fromJSON && Object.prototype.hasOwnProperty.call(n, l)) {
              o[s.name] = c.fromJSON.call(a, e, n[l], o);
              return;
            }
          }
        o[s.name] = s.init(e, o);
      }
    }), o;
  }
}
function Ed(t, e, n) {
  for (let r in t) {
    let i = t[r];
    i instanceof Function ? i = i.bind(e) : r == "handleDOMEvents" && (i = Ed(i, e, {})), n[r] = i;
  }
  return n;
}
class Ne {
  /**
  Create a plugin.
  */
  constructor(e) {
    this.spec = e, this.props = {}, e.props && Ed(e.props, this, this.props), this.key = e.key ? e.key.key : Od("plugin");
  }
  /**
  Extract the plugin's state field from an editor state.
  */
  getState(e) {
    return e[this.key];
  }
}
const js = /* @__PURE__ */ Object.create(null);
function Od(t) {
  return t in js ? t + "$" + ++js[t] : (js[t] = 0, t + "$");
}
class ge {
  /**
  Create a plugin key.
  */
  constructor(e = "key") {
    this.key = Od(e);
  }
  /**
  Get the active plugin with this key, if any, from an editor
  state.
  */
  get(e) {
    return e.config.pluginsByKey[this.key];
  }
  /**
  Get the plugin's state from an editor state.
  */
  getState(e) {
    return e[this.key];
  }
}
class Ge {
  /**
  Create an input rule. The rule applies when the user typed
  something and the text directly in front of the cursor matches
  `match`, which should end with `$`.
  
  The `handler` can be a string, in which case the matched text, or
  the first matched group in the regexp, is replaced by that
  string.
  
  Or a it can be a function, which will be called with the match
  array produced by
  [`RegExp.exec`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/RegExp/exec),
  as well as the start and end of the matched range, and which can
  return a [transaction](https://prosemirror.net/docs/ref/#state.Transaction) that describes the
  rule's effect, or null to indicate the input was not handled.
  */
  constructor(e, n, r = {}) {
    this.match = e, this.match = e, this.handler = typeof n == "string" ? L0(n) : n, this.undoable = r.undoable !== !1, this.inCode = r.inCode || !1, this.inCodeMark = r.inCodeMark !== !1;
  }
}
function L0(t) {
  return function(e, n, r, i) {
    let o = t;
    if (n[1]) {
      let s = n[0].lastIndexOf(n[1]);
      o += n[0].slice(s + n[1].length), r += s;
      let l = r - i;
      l > 0 && (o = n[0].slice(s - l, s) + o, r = i);
    }
    return e.tr.insertText(o, r, i);
  };
}
const F0 = (t, e) => {
  let n = t.plugins;
  for (let r = 0; r < n.length; r++) {
    let i = n[r], o;
    if (i.spec.isInputRules && (o = i.getState(t))) {
      if (e) {
        let s = t.tr, l = o.transform;
        for (let a = l.steps.length - 1; a >= 0; a--)
          s.step(l.steps[a].invert(l.docs[a]));
        if (o.text) {
          let a = s.doc.resolve(o.from).marks();
          s.replaceWith(o.from, o.to, t.schema.text(o.text, a));
        } else
          s.delete(o.from, o.to);
        e(s);
      }
      return !0;
    }
  }
  return !1;
};
new Ge(/--$/, "—", { inCodeMark: !1 });
new Ge(/\.\.\.$/, "…", { inCodeMark: !1 });
new Ge(/(?:^|[\s\{\[\(\<'"\u2018\u201C])(")$/, "“", { inCodeMark: !1 });
new Ge(/"$/, "”", { inCodeMark: !1 });
new Ge(/(?:^|[\s\{\[\(\<'"\u2018\u201C])(')$/, "‘", { inCodeMark: !1 });
new Ge(/'$/, "’", { inCodeMark: !1 });
function Sa(t, e, n = null, r) {
  return new Ge(t, (i, o, s, l) => {
    let a = n instanceof Function ? n(o) : n, c = i.tr.delete(s, l), u = c.doc.resolve(s), h = u.blockRange(), f = h && wd(h, e, a);
    if (!f)
      return null;
    c.wrap(h, f);
    let d = c.doc.resolve(s - 1).nodeBefore;
    return d && d.type == e && hs(c.doc, s - 1) && (!r || r(o, d)) && c.join(s - 1), c;
  });
}
function Dd(t, e, n = null) {
  return new Ge(t, (r, i, o, s) => {
    let l = r.doc.resolve(o), a = n instanceof Function ? n(i) : n;
    return l.node(-1).canReplaceWith(l.index(-1), l.indexAfter(-1), e) ? r.tr.delete(o, s).setBlockType(o, o, e, a) : null;
  });
}
var Rd = /* @__PURE__ */ ((t) => (t.docTypeError = "docTypeError", t.contextNotFound = "contextNotFound", t.timerNotFound = "timerNotFound", t.ctxCallOutOfScope = "ctxCallOutOfScope", t.createNodeInParserFail = "createNodeInParserFail", t.stackOverFlow = "stackOverFlow", t.parserMatchError = "parserMatchError", t.serializerMatchError = "serializerMatchError", t.getAtomFromSchemaFail = "getAtomFromSchemaFail", t.expectDomTypeError = "expectDomTypeError", t.callCommandBeforeEditorView = "callCommandBeforeEditorView", t.missingRootElement = "missingRootElement", t.missingNodeInSchema = "missingNodeInSchema", t.missingMarkInSchema = "missingMarkInSchema", t.ctxNotBind = "ctxNotBind", t.missingYjsDoc = "missingYjsDoc", t))(Rd || {});
class B0 extends Error {
  constructor(e, n) {
    super(n), this.name = "MilkdownError", this.code = e;
  }
}
function _0(t, e) {
  return new B0(
    Rd.getAtomFromSchemaFail,
    `Cannot get ${t}: ${e} from schema.`
  );
}
const fn = typeof navigator < "u" ? navigator : null, Qu = typeof document < "u" ? document : null, gn = fn && fn.userAgent || "", _l = /Edge\/(\d+)/.exec(gn), vd = /MSIE \d/.exec(gn), $l = /Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(gn), ds = !!(vd || $l || _l), $0 = vd ? document.documentMode : $l ? +$l[1] : _l ? +_l[1] : 0, Pd = !ds && /gecko\/(\d+)/i.test(gn), V0 = Pd && +(/Firefox\/(\d+)/.exec(gn) || [0, 0])[1], Vl = !ds && /Chrome\/(\d+)/.exec(gn), W0 = !!Vl, H0 = Vl ? +Vl[1] : 0, zd = !ds && !!fn && /Apple Computer/.test(fn.vendor), Ld = zd && (/Mobile\/\w+/.test(gn) || !!fn && fn.maxTouchPoints > 2), j0 = Ld || (fn ? /Mac/.test(fn.platform) : !1), q0 = /Android \d/.test(gn), Fd = !!Qu && "webkitFontSmoothing" in Qu.documentElement.style, K0 = Fd ? +(/\bAppleWebKit\/(\d+)/.exec(navigator.userAgent) || [0, 0])[1] : 0;
var HT = /* @__PURE__ */ Object.freeze({
  __proto__: null,
  android: q0,
  chrome: W0,
  chrome_version: H0,
  gecko: Pd,
  gecko_version: V0,
  ie: ds,
  ie_version: $0,
  ios: Ld,
  mac: j0,
  safari: zd,
  webkit: Fd,
  webkit_version: K0
});
function qs(t, e, n, r, i, o) {
  if (t.composing) return !1;
  const s = t.state, l = s.doc.resolve(e);
  if (l.parent.type.spec.code) return !1;
  const a = l.parent.textBetween(
    Math.max(0, l.parentOffset - 500),
    l.parentOffset,
    void 0,
    "￼"
  ) + r;
  for (let c of i) {
    const u = c, h = u.match.exec(a), f = h && h[0] && u.handler(s, h, e - (h[0].length - r.length), n);
    if (f)
      return u.undoable !== !1 && f.setMeta(o, { transform: f, from: e, to: n, text: r }), t.dispatch(f), !0;
  }
  return !1;
}
const J0 = new ge("MILKDOWN_CUSTOM_INPUTRULES");
function U0({ rules: t }) {
  const e = new Ne({
    key: J0,
    isInputRules: !0,
    state: {
      init() {
        return null;
      },
      apply(n, r) {
        const i = n.getMeta(this);
        return i || (n.selectionSet || n.docChanged ? null : r);
      }
    },
    props: {
      handleTextInput(n, r, i, o) {
        return qs(n, r, i, o, t, e);
      },
      handleDOMEvents: {
        compositionend: (n) => (setTimeout(() => {
          const { $cursor: r } = n.state.selection;
          r && qs(n, r.pos, r.pos, "", t, e);
        }), !1)
      },
      handleKeyDown(n, r) {
        if (r.key !== "Enter") return !1;
        const { $cursor: i } = n.state.selection;
        return i ? qs(n, i.pos, i.pos, `
`, t, e) : !1;
      }
    }
  });
  return e;
}
function Ri(t, e, n = {}) {
  return new Ge(t, (r, i, o, s) => {
    var l, a, c, u;
    const { tr: h } = r, f = i.length;
    let d = i[f - 1], p = i[0], m = [], y = s;
    const g = {
      group: d,
      fullMatch: p,
      start: o,
      end: s
    }, N = (l = n.updateCaptured) == null ? void 0 : l.call(n, g);
    if (Object.assign(g, N), { group: d, fullMatch: p, start: o, end: s } = g, p === null || (d == null ? void 0 : d.trim()) === "") return null;
    if (d) {
      const C = p.search(/\S/), v = o + p.indexOf(d), I = v + d.length;
      m = (a = h.storedMarks) != null ? a : [], I < s && h.delete(I, s), v > o && h.delete(o + C, v), y = o + C + d.length;
      const w = (c = n.getAttr) == null ? void 0 : c.call(n, i);
      h.addMark(o, y, e.create(w)), h.setStoredMarks(m), (u = n.beforeDispatch) == null || u.call(n, { match: i, start: o, end: s, tr: h });
    }
    return h;
  });
}
var G0 = Object.defineProperty, Y0 = Object.defineProperties, Q0 = Object.getOwnPropertyDescriptors, Xu = Object.getOwnPropertySymbols, X0 = Object.prototype.hasOwnProperty, Z0 = Object.prototype.propertyIsEnumerable, Zu = (t, e, n) => e in t ? G0(t, e, { enumerable: !0, configurable: !0, writable: !0, value: n }) : t[e] = n, ew = (t, e) => {
  for (var n in e || (e = {}))
    X0.call(e, n) && Zu(t, n, e[n]);
  if (Xu)
    for (var n of Xu(e))
      Z0.call(e, n) && Zu(t, n, e[n]);
  return t;
}, tw = (t, e) => Y0(t, Q0(e));
function eh(t = 0, e = 0, n = 0) {
  return Math.min(Math.max(t, e), n);
}
function jT(t, e, n) {
  const i = t.state.doc.content.size, o = eh(e, 0, i), s = eh(n, 0, i), l = t.coordsAtPos(o), a = t.coordsAtPos(s, -1), c = Math.min(l.top, a.top), u = Math.max(l.bottom, a.bottom), h = Math.min(l.left, a.left), f = Math.max(l.right, a.right), d = f - h, p = u - c, g = {
    top: c,
    bottom: u,
    left: h,
    right: f,
    width: d,
    height: p,
    x: h,
    y: c
  };
  return tw(ew({}, g), {
    toJSON: () => g
  });
}
function ps(t) {
  return Object.assign(Object.create(t), t).setTime(Date.now());
}
function nw(t, e) {
  return Array.isArray(t) && t.includes(e.type) || e.type === t;
}
function rw(t) {
  return (e) => {
    for (let n = e.depth; n > 0; n -= 1) {
      const r = e.node(n);
      if (t(r)) {
        const i = e.before(n), o = e.after(n);
        return {
          from: i,
          to: o,
          node: r
        };
      }
    }
  };
}
function iw(t, e) {
  return rw((n) => n.type === e)(t);
}
function ow(t, e) {
  const n = e.nodes[t];
  if (!n) throw _0("node", t);
  return n;
}
function Ma(t) {
  return (e) => {
    for (let n = e.depth; n > 0; n--) {
      const r = e.node(n);
      if (t(r))
        return {
          pos: n > 0 ? e.before(n) : 0,
          start: e.start(n),
          depth: n,
          node: r
        };
    }
  };
}
function qT(t) {
  return (e) => Ma(t)(e.$from);
}
function sw(t, e) {
  if (!(t instanceof _)) return;
  const { node: n, $from: r } = t;
  if (nw(e, n))
    return {
      node: n,
      pos: r.pos,
      start: r.start(r.depth),
      depth: r.depth
    };
}
const Na = (t, e) => t.selection.empty ? !1 : (e && e(t.tr.deleteSelection().scrollIntoView()), !0);
function lw(t, e) {
  let { $cursor: n } = t.selection;
  return !n || (e ? !e.endOfTextblock("backward", t) : n.parentOffset > 0) ? null : n;
}
const Bd = (t, e, n) => {
  let r = lw(t, n);
  if (!r)
    return !1;
  let i = $d(r);
  if (!i) {
    let s = r.blockRange(), l = s && us(s);
    return l == null ? !1 : (e && e(t.tr.lift(s, l).scrollIntoView()), !0);
  }
  let o = i.nodeBefore;
  if (Wd(t, i, e, -1))
    return !0;
  if (r.parent.content.size == 0 && (Mr(o, "end") || _.isSelectable(o)))
    for (let s = r.depth; ; s--) {
      let l = xa(t.doc, r.before(s), r.after(s), E.empty);
      if (l && l.slice.size < l.to - l.from) {
        if (e) {
          let a = t.tr.step(l);
          a.setSelection(Mr(o, "end") ? $.findFrom(a.doc.resolve(a.mapping.map(i.pos, -1)), -1) : _.create(a.doc, i.pos - o.nodeSize)), e(a.scrollIntoView());
        }
        return !0;
      }
      if (s == 1 || r.node(s - 1).childCount > 1)
        break;
    }
  return o.isAtom && i.depth == r.depth - 1 ? (e && e(t.tr.delete(i.pos - o.nodeSize, i.pos).scrollIntoView()), !0) : !1;
};
function Mr(t, e, n = !1) {
  for (let r = t; r; r = e == "start" ? r.firstChild : r.lastChild) {
    if (r.isTextblock)
      return !0;
    if (n && r.childCount != 1)
      return !1;
  }
  return !1;
}
const _d = (t, e, n) => {
  let { $head: r, empty: i } = t.selection, o = r;
  if (!i)
    return !1;
  if (r.parent.isTextblock) {
    if (n ? !n.endOfTextblock("backward", t) : r.parentOffset > 0)
      return !1;
    o = $d(r);
  }
  let s = o && o.nodeBefore;
  return !s || !_.isSelectable(s) ? !1 : (e && e(t.tr.setSelection(_.create(t.doc, o.pos - s.nodeSize)).scrollIntoView()), !0);
};
function $d(t) {
  if (!t.parent.type.spec.isolating)
    for (let e = t.depth - 1; e >= 0; e--) {
      if (t.index(e) > 0)
        return t.doc.resolve(t.before(e + 1));
      if (t.node(e).type.spec.isolating)
        break;
    }
  return null;
}
function aw(t, e) {
  let { $cursor: n } = t.selection;
  return !n || (e ? !e.endOfTextblock("forward", t) : n.parentOffset < n.parent.content.size) ? null : n;
}
const cw = (t, e, n) => {
  let r = aw(t, n);
  if (!r)
    return !1;
  let i = Vd(r);
  if (!i)
    return !1;
  let o = i.nodeAfter;
  if (Wd(t, i, e, 1))
    return !0;
  if (r.parent.content.size == 0 && (Mr(o, "start") || _.isSelectable(o))) {
    let s = xa(t.doc, r.before(), r.after(), E.empty);
    if (s && s.slice.size < s.to - s.from) {
      if (e) {
        let l = t.tr.step(s);
        l.setSelection(Mr(o, "start") ? $.findFrom(l.doc.resolve(l.mapping.map(i.pos)), 1) : _.create(l.doc, l.mapping.map(i.pos))), e(l.scrollIntoView());
      }
      return !0;
    }
  }
  return o.isAtom && i.depth == r.depth - 1 ? (e && e(t.tr.delete(i.pos, i.pos + o.nodeSize).scrollIntoView()), !0) : !1;
}, uw = (t, e, n) => {
  let { $head: r, empty: i } = t.selection, o = r;
  if (!i)
    return !1;
  if (r.parent.isTextblock) {
    if (n ? !n.endOfTextblock("forward", t) : r.parentOffset < r.parent.content.size)
      return !1;
    o = Vd(r);
  }
  let s = o && o.nodeAfter;
  return !s || !_.isSelectable(s) ? !1 : (e && e(t.tr.setSelection(_.create(t.doc, o.pos)).scrollIntoView()), !0);
};
function Vd(t) {
  if (!t.parent.type.spec.isolating)
    for (let e = t.depth - 1; e >= 0; e--) {
      let n = t.node(e);
      if (t.index(e) + 1 < n.childCount)
        return t.doc.resolve(t.after(e + 1));
      if (n.type.spec.isolating)
        break;
    }
  return null;
}
const hw = (t, e) => {
  let { $head: n, $anchor: r } = t.selection;
  return !n.parent.type.spec.code || !n.sameParent(r) ? !1 : (e && e(t.tr.insertText(`
`).scrollIntoView()), !0);
};
function Ta(t) {
  for (let e = 0; e < t.edgeCount; e++) {
    let { type: n } = t.edge(e);
    if (n.isTextblock && !n.hasRequiredAttrs())
      return n;
  }
  return null;
}
const fw = (t, e) => {
  let { $head: n, $anchor: r } = t.selection;
  if (!n.parent.type.spec.code || !n.sameParent(r))
    return !1;
  let i = n.node(-1), o = n.indexAfter(-1), s = Ta(i.contentMatchAt(o));
  if (!s || !i.canReplaceWith(o, o, s))
    return !1;
  if (e) {
    let l = n.after(), a = t.tr.replaceWith(l, l, s.createAndFill());
    a.setSelection($.near(a.doc.resolve(l), 1)), e(a.scrollIntoView());
  }
  return !0;
}, dw = (t, e) => {
  let n = t.selection, { $from: r, $to: i } = n;
  if (n instanceof Be || r.parent.inlineContent || i.parent.inlineContent)
    return !1;
  let o = Ta(i.parent.contentMatchAt(i.indexAfter()));
  if (!o || !o.isTextblock)
    return !1;
  if (e) {
    let s = (!r.parentOffset && i.index() < i.parent.childCount ? r : i).pos, l = t.tr.insert(s, o.createAndFill());
    l.setSelection(H.create(l.doc, s + 1)), e(l.scrollIntoView());
  }
  return !0;
}, pw = (t, e) => {
  let { $cursor: n } = t.selection;
  if (!n || n.parent.content.size)
    return !1;
  if (n.depth > 1 && n.after() != n.end(-1)) {
    let o = n.before();
    if (ci(t.doc, o))
      return e && e(t.tr.split(o).scrollIntoView()), !0;
  }
  let r = n.blockRange(), i = r && us(r);
  return i == null ? !1 : (e && e(t.tr.lift(r, i).scrollIntoView()), !0);
};
function mw(t) {
  return (e, n) => {
    let { $from: r, $to: i } = e.selection;
    if (e.selection instanceof _ && e.selection.node.isBlock)
      return !r.parentOffset || !ci(e.doc, r.pos) ? !1 : (n && n(e.tr.split(r.pos).scrollIntoView()), !0);
    if (!r.depth)
      return !1;
    let o = [], s, l, a = !1, c = !1;
    for (let d = r.depth; ; d--)
      if (r.node(d).isBlock) {
        a = r.end(d) == r.pos + (r.depth - d), c = r.start(d) == r.pos - (r.depth - d), l = Ta(r.node(d - 1).contentMatchAt(r.indexAfter(d - 1))), o.unshift(a && l ? { type: l } : null), s = d;
        break;
      } else {
        if (d == 1)
          return !1;
        o.unshift(null);
      }
    let u = e.tr;
    (e.selection instanceof H || e.selection instanceof Be) && u.deleteSelection();
    let h = u.mapping.map(r.pos), f = ci(u.doc, h, o.length, o);
    if (f || (o[0] = l ? { type: l } : null, f = ci(u.doc, h, o.length, o)), u.split(h, o.length, o), !a && c && r.node(s).type != l) {
      let d = u.mapping.map(r.before(s)), p = u.doc.resolve(d);
      l && r.node(s - 1).canReplaceWith(p.index(), p.index() + 1, l) && u.setNodeMarkup(u.mapping.map(r.before(s)), l);
    }
    return n && n(u.scrollIntoView()), !0;
  };
}
const gw = mw(), yw = (t, e) => (e && e(t.tr.setSelection(new Be(t.doc))), !0);
function kw(t, e, n) {
  let r = e.nodeBefore, i = e.nodeAfter, o = e.index();
  return !r || !i || !r.type.compatibleContent(i.type) ? !1 : !r.content.size && e.parent.canReplace(o - 1, o) ? (n && n(t.tr.delete(e.pos - r.nodeSize, e.pos).scrollIntoView()), !0) : !e.parent.canReplace(o, o + 1) || !(i.isTextblock || hs(t.doc, e.pos)) ? !1 : (n && n(t.tr.join(e.pos).scrollIntoView()), !0);
}
function Wd(t, e, n, r) {
  let i = e.nodeBefore, o = e.nodeAfter, s, l, a = i.type.spec.isolating || o.type.spec.isolating;
  if (!a && kw(t, e, n))
    return !0;
  let c = !a && e.parent.canReplace(e.index(), e.index() + 1);
  if (c && (s = (l = i.contentMatchAt(i.childCount)).findWrapping(o.type)) && l.matchType(s[0] || o.type).validEnd) {
    if (n) {
      let d = e.pos + o.nodeSize, p = T.empty;
      for (let g = s.length - 1; g >= 0; g--)
        p = T.from(s[g].create(null, p));
      p = T.from(i.copy(p));
      let m = t.tr.step(new Ae(e.pos - 1, d, e.pos, d, new E(p, 1, 0), s.length, !0)), y = m.doc.resolve(d + 2 * s.length);
      y.nodeAfter && y.nodeAfter.type == i.type && hs(m.doc, y.pos) && m.join(y.pos), n(m.scrollIntoView());
    }
    return !0;
  }
  let u = o.type.spec.isolating || r > 0 && a ? null : $.findFrom(e, 1), h = u && u.$from.blockRange(u.$to), f = h && us(h);
  if (f != null && f >= e.depth)
    return n && n(t.tr.lift(h, f).scrollIntoView()), !0;
  if (c && Mr(o, "start", !0) && Mr(i, "end")) {
    let d = i, p = [];
    for (; p.push(d), !d.isTextblock; )
      d = d.lastChild;
    let m = o, y = 1;
    for (; !m.isTextblock; m = m.firstChild)
      y++;
    if (d.canReplace(d.childCount, d.childCount, m.content)) {
      if (n) {
        let g = T.empty;
        for (let C = p.length - 1; C >= 0; C--)
          g = T.from(p[C].copy(g));
        let N = t.tr.step(new Ae(e.pos - p.length, e.pos + o.nodeSize, e.pos + y, e.pos + o.nodeSize - y, new E(g, p.length, 0), 0, !0));
        n(N.scrollIntoView());
      }
      return !0;
    }
  }
  return !1;
}
function Hd(t) {
  return function(e, n) {
    let r = e.selection, i = t < 0 ? r.$from : r.$to, o = i.depth;
    for (; i.node(o).isInline; ) {
      if (!o)
        return !1;
      o--;
    }
    return i.node(o).isTextblock ? (n && n(e.tr.setSelection(H.create(e.doc, t < 0 ? i.start(o) : i.end(o)))), !0) : !1;
  };
}
const bw = Hd(-1), ww = Hd(1);
function Ia(t, e = null) {
  return function(n, r) {
    let { $from: i, $to: o } = n.selection, s = i.blockRange(o), l = s && wd(s, t, e);
    return l ? (r && r(n.tr.wrap(s, l).scrollIntoView()), !0) : !1;
  };
}
function xi(t, e = null) {
  return function(n, r) {
    let i = !1;
    for (let o = 0; o < n.selection.ranges.length && !i; o++) {
      let { $from: { pos: s }, $to: { pos: l } } = n.selection.ranges[o];
      n.doc.nodesBetween(s, l, (a, c) => {
        if (i)
          return !1;
        if (!(!a.isTextblock || a.hasMarkup(t, e)))
          if (a.type == t)
            i = !0;
          else {
            let u = n.doc.resolve(c), h = u.index();
            i = u.parent.canReplaceWith(h, h + 1, t);
          }
      });
    }
    if (!i)
      return !1;
    if (r) {
      let o = n.tr;
      for (let s = 0; s < n.selection.ranges.length; s++) {
        let { $from: { pos: l }, $to: { pos: a } } = n.selection.ranges[s];
        o.setBlockType(l, a, t, e);
      }
      r(o.scrollIntoView());
    }
    return !0;
  };
}
function xw(t, e, n, r) {
  for (let i = 0; i < e.length; i++) {
    let { $from: o, $to: s } = e[i], l = o.depth == 0 ? t.inlineContent && t.type.allowsMarkType(n) : !1;
    if (t.nodesBetween(o.pos, s.pos, (a, c) => {
      if (l)
        return !1;
      l = a.inlineContent && a.type.allowsMarkType(n);
    }), l)
      return !0;
  }
  return !1;
}
function ms(t, e = null, n) {
  return function(r, i) {
    let { empty: o, $cursor: s, ranges: l } = r.selection;
    if (o && !s || !xw(r.doc, l, t))
      return !1;
    if (i)
      if (s)
        t.isInSet(r.storedMarks || s.marks()) ? i(r.tr.removeStoredMark(t)) : i(r.tr.addStoredMark(t.create(e)));
      else {
        let a, c = r.tr;
        a = !l.some((u) => r.doc.rangeHasMark(u.$from.pos, u.$to.pos, t));
        for (let u = 0; u < l.length; u++) {
          let { $from: h, $to: f } = l[u];
          if (!a)
            c.removeMark(h.pos, f.pos, t);
          else {
            let d = h.pos, p = f.pos, m = h.nodeAfter, y = f.nodeBefore, g = m && m.isText ? /^\s*/.exec(m.text)[0].length : 0, N = y && y.isText ? /\s*$/.exec(y.text)[0].length : 0;
            d + g < p && (d += g, p -= N), c.addMark(d, p, t.create(e));
          }
        }
        i(c.scrollIntoView());
      }
    return !0;
  };
}
function gs(...t) {
  return function(e, n, r) {
    for (let i = 0; i < t.length; i++)
      if (t[i](e, n, r))
        return !0;
    return !1;
  };
}
let Ks = gs(Na, Bd, _d), th = gs(Na, cw, uw);
const _t = {
  Enter: gs(hw, dw, pw, gw),
  "Mod-Enter": fw,
  Backspace: Ks,
  "Mod-Backspace": Ks,
  "Shift-Backspace": Ks,
  Delete: th,
  "Mod-Delete": th,
  "Mod-a": yw
}, jd = {
  "Ctrl-h": _t.Backspace,
  "Alt-Backspace": _t["Mod-Backspace"],
  "Ctrl-d": _t.Delete,
  "Ctrl-Alt-Backspace": _t["Mod-Delete"],
  "Alt-Delete": _t["Mod-Delete"],
  "Alt-d": _t["Mod-Delete"],
  "Ctrl-a": bw,
  "Ctrl-e": ww
};
for (let t in _t)
  jd[t] = _t[t];
const Cw = typeof navigator < "u" ? /Mac|iP(hone|[oa]d)/.test(navigator.platform) : typeof os < "u" && os.platform ? os.platform() == "darwin" : !1, Sw = Cw ? jd : _t;
var dn = {
  8: "Backspace",
  9: "Tab",
  10: "Enter",
  12: "NumLock",
  13: "Enter",
  16: "Shift",
  17: "Control",
  18: "Alt",
  20: "CapsLock",
  27: "Escape",
  32: " ",
  33: "PageUp",
  34: "PageDown",
  35: "End",
  36: "Home",
  37: "ArrowLeft",
  38: "ArrowUp",
  39: "ArrowRight",
  40: "ArrowDown",
  44: "PrintScreen",
  45: "Insert",
  46: "Delete",
  59: ";",
  61: "=",
  91: "Meta",
  92: "Meta",
  106: "*",
  107: "+",
  108: ",",
  109: "-",
  110: ".",
  111: "/",
  144: "NumLock",
  145: "ScrollLock",
  160: "Shift",
  161: "Shift",
  162: "Control",
  163: "Control",
  164: "Alt",
  165: "Alt",
  173: "-",
  186: ";",
  187: "=",
  188: ",",
  189: "-",
  190: ".",
  191: "/",
  192: "`",
  219: "[",
  220: "\\",
  221: "]",
  222: "'"
}, Jo = {
  48: ")",
  49: "!",
  50: "@",
  51: "#",
  52: "$",
  53: "%",
  54: "^",
  55: "&",
  56: "*",
  57: "(",
  59: ":",
  61: "+",
  173: "_",
  186: ":",
  187: "+",
  188: "<",
  189: "_",
  190: ">",
  191: "?",
  192: "~",
  219: "{",
  220: "|",
  221: "}",
  222: '"'
}, Mw = typeof navigator < "u" && /Mac/.test(navigator.platform), Nw = typeof navigator < "u" && /MSIE \d|Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(navigator.userAgent);
for (var Ce = 0; Ce < 10; Ce++) dn[48 + Ce] = dn[96 + Ce] = String(Ce);
for (var Ce = 1; Ce <= 24; Ce++) dn[Ce + 111] = "F" + Ce;
for (var Ce = 65; Ce <= 90; Ce++)
  dn[Ce] = String.fromCharCode(Ce + 32), Jo[Ce] = String.fromCharCode(Ce);
for (var Js in dn) Jo.hasOwnProperty(Js) || (Jo[Js] = dn[Js]);
function Tw(t) {
  var e = Mw && t.metaKey && t.shiftKey && !t.ctrlKey && !t.altKey || Nw && t.shiftKey && t.key && t.key.length == 1 || t.key == "Unidentified", n = !e && t.key || (t.shiftKey ? Jo : dn)[t.keyCode] || t.key || "Unidentified";
  return n == "Esc" && (n = "Escape"), n == "Del" && (n = "Delete"), n == "Left" && (n = "ArrowLeft"), n == "Up" && (n = "ArrowUp"), n == "Right" && (n = "ArrowRight"), n == "Down" && (n = "ArrowDown"), n;
}
const Iw = typeof navigator < "u" ? /Mac|iP(hone|[oa]d)/.test(navigator.platform) : !1;
function Aw(t) {
  let e = t.split(/-(?!$)/), n = e[e.length - 1];
  n == "Space" && (n = " ");
  let r, i, o, s;
  for (let l = 0; l < e.length - 1; l++) {
    let a = e[l];
    if (/^(cmd|meta|m)$/i.test(a))
      s = !0;
    else if (/^a(lt)?$/i.test(a))
      r = !0;
    else if (/^(c|ctrl|control)$/i.test(a))
      i = !0;
    else if (/^s(hift)?$/i.test(a))
      o = !0;
    else if (/^mod$/i.test(a))
      Iw ? s = !0 : i = !0;
    else
      throw new Error("Unrecognized modifier name: " + a);
  }
  return r && (n = "Alt-" + n), i && (n = "Ctrl-" + n), s && (n = "Meta-" + n), o && (n = "Shift-" + n), n;
}
function Ew(t) {
  let e = /* @__PURE__ */ Object.create(null);
  for (let n in t)
    e[Aw(n)] = t[n];
  return e;
}
function Us(t, e, n = !0) {
  return e.altKey && (t = "Alt-" + t), e.ctrlKey && (t = "Ctrl-" + t), e.metaKey && (t = "Meta-" + t), n && e.shiftKey && (t = "Shift-" + t), t;
}
function qd(t) {
  return new Ne({ props: { handleKeyDown: Kd(t) } });
}
function Kd(t) {
  let e = Ew(t);
  return function(n, r) {
    let i = Tw(r), o, s = e[Us(i, r)];
    if (s && s(n.state, n.dispatch, n))
      return !0;
    if (i.length == 1 && i != " ") {
      if (r.shiftKey) {
        let l = e[Us(i, r, !1)];
        if (l && l(n.state, n.dispatch, n))
          return !0;
      }
      if ((r.shiftKey || r.altKey || r.metaKey || i.charCodeAt(0) > 127) && (o = dn[r.keyCode]) && o != i) {
        let l = e[Us(o, r)];
        if (l && l(n.state, n.dispatch, n))
          return !0;
      }
    }
    return !1;
  };
}
const Se = function(t) {
  for (var e = 0; ; e++)
    if (t = t.previousSibling, !t)
      return e;
}, Nr = function(t) {
  let e = t.assignedSlot || t.parentNode;
  return e && e.nodeType == 11 ? e.host : e;
};
let Wl = null;
const Bt = function(t, e, n) {
  let r = Wl || (Wl = document.createRange());
  return r.setEnd(t, n ?? t.nodeValue.length), r.setStart(t, e || 0), r;
}, Ow = function() {
  Wl = null;
}, Hn = function(t, e, n, r) {
  return n && (nh(t, e, n, r, -1) || nh(t, e, n, r, 1));
}, Dw = /^(img|br|input|textarea|hr)$/i;
function nh(t, e, n, r, i) {
  for (; ; ) {
    if (t == n && e == r)
      return !0;
    if (e == (i < 0 ? 0 : tt(t))) {
      let o = t.parentNode;
      if (!o || o.nodeType != 1 || vi(t) || Dw.test(t.nodeName) || t.contentEditable == "false")
        return !1;
      e = Se(t) + (i < 0 ? 0 : 1), t = o;
    } else if (t.nodeType == 1) {
      if (t = t.childNodes[e + (i < 0 ? -1 : 0)], t.contentEditable == "false")
        return !1;
      e = i < 0 ? tt(t) : 0;
    } else
      return !1;
  }
}
function tt(t) {
  return t.nodeType == 3 ? t.nodeValue.length : t.childNodes.length;
}
function Rw(t, e) {
  for (; ; ) {
    if (t.nodeType == 3 && e)
      return t;
    if (t.nodeType == 1 && e > 0) {
      if (t.contentEditable == "false")
        return null;
      t = t.childNodes[e - 1], e = tt(t);
    } else if (t.parentNode && !vi(t))
      e = Se(t), t = t.parentNode;
    else
      return null;
  }
}
function vw(t, e) {
  for (; ; ) {
    if (t.nodeType == 3 && e < t.nodeValue.length)
      return t;
    if (t.nodeType == 1 && e < t.childNodes.length) {
      if (t.contentEditable == "false")
        return null;
      t = t.childNodes[e], e = 0;
    } else if (t.parentNode && !vi(t))
      e = Se(t) + 1, t = t.parentNode;
    else
      return null;
  }
}
function Pw(t, e, n) {
  for (let r = e == 0, i = e == tt(t); r || i; ) {
    if (t == n)
      return !0;
    let o = Se(t);
    if (t = t.parentNode, !t)
      return !1;
    r = r && o == 0, i = i && o == tt(t);
  }
}
function vi(t) {
  let e;
  for (let n = t; n && !(e = n.pmViewDesc); n = n.parentNode)
    ;
  return e && e.node && e.node.isBlock && (e.dom == t || e.contentDOM == t);
}
const ys = function(t) {
  return t.focusNode && Hn(t.focusNode, t.focusOffset, t.anchorNode, t.anchorOffset);
};
function En(t, e) {
  let n = document.createEvent("Event");
  return n.initEvent("keydown", !0, !0), n.keyCode = t, n.key = n.code = e, n;
}
function zw(t) {
  let e = t.activeElement;
  for (; e && e.shadowRoot; )
    e = e.shadowRoot.activeElement;
  return e;
}
function Lw(t, e, n) {
  if (t.caretPositionFromPoint)
    try {
      let r = t.caretPositionFromPoint(e, n);
      if (r)
        return { node: r.offsetNode, offset: Math.min(tt(r.offsetNode), r.offset) };
    } catch {
    }
  if (t.caretRangeFromPoint) {
    let r = t.caretRangeFromPoint(e, n);
    if (r)
      return { node: r.startContainer, offset: Math.min(tt(r.startContainer), r.startOffset) };
  }
}
const Nt = typeof navigator < "u" ? navigator : null, rh = typeof document < "u" ? document : null, yn = Nt && Nt.userAgent || "", Hl = /Edge\/(\d+)/.exec(yn), Jd = /MSIE \d/.exec(yn), jl = /Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(yn), _e = !!(Jd || jl || Hl), an = Jd ? document.documentMode : jl ? +jl[1] : Hl ? +Hl[1] : 0, mt = !_e && /gecko\/(\d+)/i.test(yn);
mt && +(/Firefox\/(\d+)/.exec(yn) || [0, 0])[1];
const ql = !_e && /Chrome\/(\d+)/.exec(yn), Ie = !!ql, Ud = ql ? +ql[1] : 0, De = !_e && !!Nt && /Apple Computer/.test(Nt.vendor), Tr = De && (/Mobile\/\w+/.test(yn) || !!Nt && Nt.maxTouchPoints > 2), Ze = Tr || (Nt ? /Mac/.test(Nt.platform) : !1), Fw = Nt ? /Win/.test(Nt.platform) : !1, Vt = /Android \d/.test(yn), Pi = !!rh && "webkitFontSmoothing" in rh.documentElement.style, Bw = Pi ? +(/\bAppleWebKit\/(\d+)/.exec(navigator.userAgent) || [0, 0])[1] : 0;
function _w(t) {
  let e = t.defaultView && t.defaultView.visualViewport;
  return e ? {
    left: 0,
    right: e.width,
    top: 0,
    bottom: e.height
  } : {
    left: 0,
    right: t.documentElement.clientWidth,
    top: 0,
    bottom: t.documentElement.clientHeight
  };
}
function zt(t, e) {
  return typeof t == "number" ? t : t[e];
}
function $w(t) {
  let e = t.getBoundingClientRect(), n = e.width / t.offsetWidth || 1, r = e.height / t.offsetHeight || 1;
  return {
    left: e.left,
    right: e.left + t.clientWidth * n,
    top: e.top,
    bottom: e.top + t.clientHeight * r
  };
}
function ih(t, e, n) {
  let r = t.someProp("scrollThreshold") || 0, i = t.someProp("scrollMargin") || 5, o = t.dom.ownerDocument;
  for (let s = n || t.dom; s; ) {
    if (s.nodeType != 1) {
      s = Nr(s);
      continue;
    }
    let l = s, a = l == o.body, c = a ? _w(o) : $w(l), u = 0, h = 0;
    if (e.top < c.top + zt(r, "top") ? h = -(c.top - e.top + zt(i, "top")) : e.bottom > c.bottom - zt(r, "bottom") && (h = e.bottom - e.top > c.bottom - c.top ? e.top + zt(i, "top") - c.top : e.bottom - c.bottom + zt(i, "bottom")), e.left < c.left + zt(r, "left") ? u = -(c.left - e.left + zt(i, "left")) : e.right > c.right - zt(r, "right") && (u = e.right - c.right + zt(i, "right")), u || h)
      if (a)
        o.defaultView.scrollBy(u, h);
      else {
        let d = l.scrollLeft, p = l.scrollTop;
        h && (l.scrollTop += h), u && (l.scrollLeft += u);
        let m = l.scrollLeft - d, y = l.scrollTop - p;
        e = { left: e.left - m, top: e.top - y, right: e.right - m, bottom: e.bottom - y };
      }
    let f = a ? "fixed" : getComputedStyle(s).position;
    if (/^(fixed|sticky)$/.test(f))
      break;
    s = f == "absolute" ? s.offsetParent : Nr(s);
  }
}
function Vw(t) {
  let e = t.dom.getBoundingClientRect(), n = Math.max(0, e.top), r, i;
  for (let o = (e.left + e.right) / 2, s = n + 1; s < Math.min(innerHeight, e.bottom); s += 5) {
    let l = t.root.elementFromPoint(o, s);
    if (!l || l == t.dom || !t.dom.contains(l))
      continue;
    let a = l.getBoundingClientRect();
    if (a.top >= n - 20) {
      r = l, i = a.top;
      break;
    }
  }
  return { refDOM: r, refTop: i, stack: Gd(t.dom) };
}
function Gd(t) {
  let e = [], n = t.ownerDocument;
  for (let r = t; r && (e.push({ dom: r, top: r.scrollTop, left: r.scrollLeft }), t != n); r = Nr(r))
    ;
  return e;
}
function Ww({ refDOM: t, refTop: e, stack: n }) {
  let r = t ? t.getBoundingClientRect().top : 0;
  Yd(n, r == 0 ? 0 : r - e);
}
function Yd(t, e) {
  for (let n = 0; n < t.length; n++) {
    let { dom: r, top: i, left: o } = t[n];
    r.scrollTop != i + e && (r.scrollTop = i + e), r.scrollLeft != o && (r.scrollLeft = o);
  }
}
let Xn = null;
function Hw(t) {
  if (t.setActive)
    return t.setActive();
  if (Xn)
    return t.focus(Xn);
  let e = Gd(t);
  t.focus(Xn == null ? {
    get preventScroll() {
      return Xn = { preventScroll: !0 }, !0;
    }
  } : void 0), Xn || (Xn = !1, Yd(e, 0));
}
function Qd(t, e) {
  let n, r = 2e8, i, o = 0, s = e.top, l = e.top, a, c;
  for (let u = t.firstChild, h = 0; u; u = u.nextSibling, h++) {
    let f;
    if (u.nodeType == 1)
      f = u.getClientRects();
    else if (u.nodeType == 3)
      f = Bt(u).getClientRects();
    else
      continue;
    for (let d = 0; d < f.length; d++) {
      let p = f[d];
      if (p.top <= s && p.bottom >= l) {
        s = Math.max(p.bottom, s), l = Math.min(p.top, l);
        let m = p.left > e.left ? p.left - e.left : p.right < e.left ? e.left - p.right : 0;
        if (m < r) {
          n = u, r = m, i = m && n.nodeType == 3 ? {
            left: p.right < e.left ? p.right : p.left,
            top: e.top
          } : e, u.nodeType == 1 && m && (o = h + (e.left >= (p.left + p.right) / 2 ? 1 : 0));
          continue;
        }
      } else p.top > e.top && !a && p.left <= e.left && p.right >= e.left && (a = u, c = { left: Math.max(p.left, Math.min(p.right, e.left)), top: p.top });
      !n && (e.left >= p.right && e.top >= p.top || e.left >= p.left && e.top >= p.bottom) && (o = h + 1);
    }
  }
  return !n && a && (n = a, i = c, r = 0), n && n.nodeType == 3 ? jw(n, i) : !n || r && n.nodeType == 1 ? { node: t, offset: o } : Qd(n, i);
}
function jw(t, e) {
  let n = t.nodeValue.length, r = document.createRange();
  for (let i = 0; i < n; i++) {
    r.setEnd(t, i + 1), r.setStart(t, i);
    let o = Yt(r, 1);
    if (o.top != o.bottom && Aa(e, o))
      return { node: t, offset: i + (e.left >= (o.left + o.right) / 2 ? 1 : 0) };
  }
  return { node: t, offset: 0 };
}
function Aa(t, e) {
  return t.left >= e.left - 1 && t.left <= e.right + 1 && t.top >= e.top - 1 && t.top <= e.bottom + 1;
}
function qw(t, e) {
  let n = t.parentNode;
  return n && /^li$/i.test(n.nodeName) && e.left < t.getBoundingClientRect().left ? n : t;
}
function Kw(t, e, n) {
  let { node: r, offset: i } = Qd(e, n), o = -1;
  if (r.nodeType == 1 && !r.firstChild) {
    let s = r.getBoundingClientRect();
    o = s.left != s.right && n.left > (s.left + s.right) / 2 ? 1 : -1;
  }
  return t.docView.posFromDOM(r, i, o);
}
function Jw(t, e, n, r) {
  let i = -1;
  for (let o = e, s = !1; o != t.dom; ) {
    let l = t.docView.nearestDesc(o, !0), a;
    if (!l)
      return null;
    if (l.dom.nodeType == 1 && (l.node.isBlock && l.parent || !l.contentDOM) && // Ignore elements with zero-size bounding rectangles
    ((a = l.dom.getBoundingClientRect()).width || a.height) && (l.node.isBlock && l.parent && (!s && a.left > r.left || a.top > r.top ? i = l.posBefore : (!s && a.right < r.left || a.bottom < r.top) && (i = l.posAfter), s = !0), !l.contentDOM && i < 0 && !l.node.isText))
      return (l.node.isBlock ? r.top < (a.top + a.bottom) / 2 : r.left < (a.left + a.right) / 2) ? l.posBefore : l.posAfter;
    o = l.dom.parentNode;
  }
  return i > -1 ? i : t.docView.posFromDOM(e, n, -1);
}
function Xd(t, e, n) {
  let r = t.childNodes.length;
  if (r && n.top < n.bottom)
    for (let i = Math.max(0, Math.min(r - 1, Math.floor(r * (e.top - n.top) / (n.bottom - n.top)) - 2)), o = i; ; ) {
      let s = t.childNodes[o];
      if (s.nodeType == 1) {
        let l = s.getClientRects();
        for (let a = 0; a < l.length; a++) {
          let c = l[a];
          if (Aa(e, c))
            return Xd(s, e, c);
        }
      }
      if ((o = (o + 1) % r) == i)
        break;
    }
  return t;
}
function Uw(t, e) {
  let n = t.dom.ownerDocument, r, i = 0, o = Lw(n, e.left, e.top);
  o && ({ node: r, offset: i } = o);
  let s = (t.root.elementFromPoint ? t.root : n).elementFromPoint(e.left, e.top), l;
  if (!s || !t.dom.contains(s.nodeType != 1 ? s.parentNode : s)) {
    let c = t.dom.getBoundingClientRect();
    if (!Aa(e, c) || (s = Xd(t.dom, e, c), !s))
      return null;
  }
  if (De)
    for (let c = s; r && c; c = Nr(c))
      c.draggable && (r = void 0);
  if (s = qw(s, e), r) {
    if (mt && r.nodeType == 1 && (i = Math.min(i, r.childNodes.length), i < r.childNodes.length)) {
      let u = r.childNodes[i], h;
      u.nodeName == "IMG" && (h = u.getBoundingClientRect()).right <= e.left && h.bottom > e.top && i++;
    }
    let c;
    Pi && i && r.nodeType == 1 && (c = r.childNodes[i - 1]).nodeType == 1 && c.contentEditable == "false" && c.getBoundingClientRect().top >= e.top && i--, r == t.dom && i == r.childNodes.length - 1 && r.lastChild.nodeType == 1 && e.top > r.lastChild.getBoundingClientRect().bottom ? l = t.state.doc.content.size : (i == 0 || r.nodeType != 1 || r.childNodes[i - 1].nodeName != "BR") && (l = Jw(t, r, i, e));
  }
  l == null && (l = Kw(t, s, e));
  let a = t.docView.nearestDesc(s, !0);
  return { pos: l, inside: a ? a.posAtStart - a.border : -1 };
}
function oh(t) {
  return t.top < t.bottom || t.left < t.right;
}
function Yt(t, e) {
  let n = t.getClientRects();
  if (n.length) {
    let r = n[e < 0 ? 0 : n.length - 1];
    if (oh(r))
      return r;
  }
  return Array.prototype.find.call(n, oh) || t.getBoundingClientRect();
}
const Gw = /[\u0590-\u05f4\u0600-\u06ff\u0700-\u08ac]/;
function Zd(t, e, n) {
  let { node: r, offset: i, atom: o } = t.docView.domFromPos(e, n < 0 ? -1 : 1), s = Pi || mt;
  if (r.nodeType == 3)
    if (s && (Gw.test(r.nodeValue) || (n < 0 ? !i : i == r.nodeValue.length))) {
      let a = Yt(Bt(r, i, i), n);
      if (mt && i && /\s/.test(r.nodeValue[i - 1]) && i < r.nodeValue.length) {
        let c = Yt(Bt(r, i - 1, i - 1), -1);
        if (c.top == a.top) {
          let u = Yt(Bt(r, i, i + 1), -1);
          if (u.top != a.top)
            return Fr(u, u.left < c.left);
        }
      }
      return a;
    } else {
      let a = i, c = i, u = n < 0 ? 1 : -1;
      return n < 0 && !i ? (c++, u = -1) : n >= 0 && i == r.nodeValue.length ? (a--, u = 1) : n < 0 ? a-- : c++, Fr(Yt(Bt(r, a, c), u), u < 0);
    }
  if (!t.state.doc.resolve(e - (o || 0)).parent.inlineContent) {
    if (o == null && i && (n < 0 || i == tt(r))) {
      let a = r.childNodes[i - 1];
      if (a.nodeType == 1)
        return Gs(a.getBoundingClientRect(), !1);
    }
    if (o == null && i < tt(r)) {
      let a = r.childNodes[i];
      if (a.nodeType == 1)
        return Gs(a.getBoundingClientRect(), !0);
    }
    return Gs(r.getBoundingClientRect(), n >= 0);
  }
  if (o == null && i && (n < 0 || i == tt(r))) {
    let a = r.childNodes[i - 1], c = a.nodeType == 3 ? Bt(a, tt(a) - (s ? 0 : 1)) : a.nodeType == 1 && (a.nodeName != "BR" || !a.nextSibling) ? a : null;
    if (c)
      return Fr(Yt(c, 1), !1);
  }
  if (o == null && i < tt(r)) {
    let a = r.childNodes[i];
    for (; a.pmViewDesc && a.pmViewDesc.ignoreForCoords; )
      a = a.nextSibling;
    let c = a ? a.nodeType == 3 ? Bt(a, 0, s ? 0 : 1) : a.nodeType == 1 ? a : null : null;
    if (c)
      return Fr(Yt(c, -1), !0);
  }
  return Fr(Yt(r.nodeType == 3 ? Bt(r) : r, -n), n >= 0);
}
function Fr(t, e) {
  if (t.width == 0)
    return t;
  let n = e ? t.left : t.right;
  return { top: t.top, bottom: t.bottom, left: n, right: n };
}
function Gs(t, e) {
  if (t.height == 0)
    return t;
  let n = e ? t.top : t.bottom;
  return { top: n, bottom: n, left: t.left, right: t.right };
}
function ep(t, e, n) {
  let r = t.state, i = t.root.activeElement;
  r != e && t.updateState(e), i != t.dom && t.focus();
  try {
    return n();
  } finally {
    r != e && t.updateState(r), i != t.dom && i && i.focus();
  }
}
function Yw(t, e, n) {
  let r = e.selection, i = n == "up" ? r.$from : r.$to;
  return ep(t, e, () => {
    let { node: o } = t.docView.domFromPos(i.pos, n == "up" ? -1 : 1);
    for (; ; ) {
      let l = t.docView.nearestDesc(o, !0);
      if (!l)
        break;
      if (l.node.isBlock) {
        o = l.contentDOM || l.dom;
        break;
      }
      o = l.dom.parentNode;
    }
    let s = Zd(t, i.pos, 1);
    for (let l = o.firstChild; l; l = l.nextSibling) {
      let a;
      if (l.nodeType == 1)
        a = l.getClientRects();
      else if (l.nodeType == 3)
        a = Bt(l, 0, l.nodeValue.length).getClientRects();
      else
        continue;
      for (let c = 0; c < a.length; c++) {
        let u = a[c];
        if (u.bottom > u.top + 1 && (n == "up" ? s.top - u.top > (u.bottom - s.top) * 2 : u.bottom - s.bottom > (s.bottom - u.top) * 2))
          return !1;
      }
    }
    return !0;
  });
}
const Qw = /[\u0590-\u08ac]/;
function Xw(t, e, n) {
  let { $head: r } = e.selection;
  if (!r.parent.isTextblock)
    return !1;
  let i = r.parentOffset, o = !i, s = i == r.parent.content.size, l = t.domSelection();
  return l ? !Qw.test(r.parent.textContent) || !l.modify ? n == "left" || n == "backward" ? o : s : ep(t, e, () => {
    let { focusNode: a, focusOffset: c, anchorNode: u, anchorOffset: h } = t.domSelectionRange(), f = l.caretBidiLevel;
    l.modify("move", n, "character");
    let d = r.depth ? t.docView.domAfterPos(r.before()) : t.dom, { focusNode: p, focusOffset: m } = t.domSelectionRange(), y = p && !d.contains(p.nodeType == 1 ? p : p.parentNode) || a == p && c == m;
    try {
      l.collapse(u, h), a && (a != u || c != h) && l.extend && l.extend(a, c);
    } catch {
    }
    return f != null && (l.caretBidiLevel = f), y;
  }) : r.pos == r.start() || r.pos == r.end();
}
let sh = null, lh = null, ah = !1;
function Zw(t, e, n) {
  return sh == e && lh == n ? ah : (sh = e, lh = n, ah = n == "up" || n == "down" ? Yw(t, e, n) : Xw(t, e, n));
}
const nt = 0, ch = 1, Dn = 2, Tt = 3;
class zi {
  constructor(e, n, r, i) {
    this.parent = e, this.children = n, this.dom = r, this.contentDOM = i, this.dirty = nt, r.pmViewDesc = this;
  }
  // Used to check whether a given description corresponds to a
  // widget/mark/node.
  matchesWidget(e) {
    return !1;
  }
  matchesMark(e) {
    return !1;
  }
  matchesNode(e, n, r) {
    return !1;
  }
  matchesHack(e) {
    return !1;
  }
  // When parsing in-editor content (in domchange.js), we allow
  // descriptions to determine the parse rules that should be used to
  // parse them.
  parseRule() {
    return null;
  }
  // Used by the editor's event handler to ignore events that come
  // from certain descs.
  stopEvent(e) {
    return !1;
  }
  // The size of the content represented by this desc.
  get size() {
    let e = 0;
    for (let n = 0; n < this.children.length; n++)
      e += this.children[n].size;
    return e;
  }
  // For block nodes, this represents the space taken up by their
  // start/end tokens.
  get border() {
    return 0;
  }
  destroy() {
    this.parent = void 0, this.dom.pmViewDesc == this && (this.dom.pmViewDesc = void 0);
    for (let e = 0; e < this.children.length; e++)
      this.children[e].destroy();
  }
  posBeforeChild(e) {
    for (let n = 0, r = this.posAtStart; ; n++) {
      let i = this.children[n];
      if (i == e)
        return r;
      r += i.size;
    }
  }
  get posBefore() {
    return this.parent.posBeforeChild(this);
  }
  get posAtStart() {
    return this.parent ? this.parent.posBeforeChild(this) + this.border : 0;
  }
  get posAfter() {
    return this.posBefore + this.size;
  }
  get posAtEnd() {
    return this.posAtStart + this.size - 2 * this.border;
  }
  localPosFromDOM(e, n, r) {
    if (this.contentDOM && this.contentDOM.contains(e.nodeType == 1 ? e : e.parentNode))
      if (r < 0) {
        let o, s;
        if (e == this.contentDOM)
          o = e.childNodes[n - 1];
        else {
          for (; e.parentNode != this.contentDOM; )
            e = e.parentNode;
          o = e.previousSibling;
        }
        for (; o && !((s = o.pmViewDesc) && s.parent == this); )
          o = o.previousSibling;
        return o ? this.posBeforeChild(s) + s.size : this.posAtStart;
      } else {
        let o, s;
        if (e == this.contentDOM)
          o = e.childNodes[n];
        else {
          for (; e.parentNode != this.contentDOM; )
            e = e.parentNode;
          o = e.nextSibling;
        }
        for (; o && !((s = o.pmViewDesc) && s.parent == this); )
          o = o.nextSibling;
        return o ? this.posBeforeChild(s) : this.posAtEnd;
      }
    let i;
    if (e == this.dom && this.contentDOM)
      i = n > Se(this.contentDOM);
    else if (this.contentDOM && this.contentDOM != this.dom && this.dom.contains(this.contentDOM))
      i = e.compareDocumentPosition(this.contentDOM) & 2;
    else if (this.dom.firstChild) {
      if (n == 0)
        for (let o = e; ; o = o.parentNode) {
          if (o == this.dom) {
            i = !1;
            break;
          }
          if (o.previousSibling)
            break;
        }
      if (i == null && n == e.childNodes.length)
        for (let o = e; ; o = o.parentNode) {
          if (o == this.dom) {
            i = !0;
            break;
          }
          if (o.nextSibling)
            break;
        }
    }
    return i ?? r > 0 ? this.posAtEnd : this.posAtStart;
  }
  nearestDesc(e, n = !1) {
    for (let r = !0, i = e; i; i = i.parentNode) {
      let o = this.getDesc(i), s;
      if (o && (!n || o.node))
        if (r && (s = o.nodeDOM) && !(s.nodeType == 1 ? s.contains(e.nodeType == 1 ? e : e.parentNode) : s == e))
          r = !1;
        else
          return o;
    }
  }
  getDesc(e) {
    let n = e.pmViewDesc;
    for (let r = n; r; r = r.parent)
      if (r == this)
        return n;
  }
  posFromDOM(e, n, r) {
    for (let i = e; i; i = i.parentNode) {
      let o = this.getDesc(i);
      if (o)
        return o.localPosFromDOM(e, n, r);
    }
    return -1;
  }
  // Find the desc for the node after the given pos, if any. (When a
  // parent node overrode rendering, there might not be one.)
  descAt(e) {
    for (let n = 0, r = 0; n < this.children.length; n++) {
      let i = this.children[n], o = r + i.size;
      if (r == e && o != r) {
        for (; !i.border && i.children.length; )
          for (let s = 0; s < i.children.length; s++) {
            let l = i.children[s];
            if (l.size) {
              i = l;
              break;
            }
          }
        return i;
      }
      if (e < o)
        return i.descAt(e - r - i.border);
      r = o;
    }
  }
  domFromPos(e, n) {
    if (!this.contentDOM)
      return { node: this.dom, offset: 0, atom: e + 1 };
    let r = 0, i = 0;
    for (let o = 0; r < this.children.length; r++) {
      let s = this.children[r], l = o + s.size;
      if (l > e || s instanceof np) {
        i = e - o;
        break;
      }
      o = l;
    }
    if (i)
      return this.children[r].domFromPos(i - this.children[r].border, n);
    for (let o; r && !(o = this.children[r - 1]).size && o instanceof tp && o.side >= 0; r--)
      ;
    if (n <= 0) {
      let o, s = !0;
      for (; o = r ? this.children[r - 1] : null, !(!o || o.dom.parentNode == this.contentDOM); r--, s = !1)
        ;
      return o && n && s && !o.border && !o.domAtom ? o.domFromPos(o.size, n) : { node: this.contentDOM, offset: o ? Se(o.dom) + 1 : 0 };
    } else {
      let o, s = !0;
      for (; o = r < this.children.length ? this.children[r] : null, !(!o || o.dom.parentNode == this.contentDOM); r++, s = !1)
        ;
      return o && s && !o.border && !o.domAtom ? o.domFromPos(0, n) : { node: this.contentDOM, offset: o ? Se(o.dom) : this.contentDOM.childNodes.length };
    }
  }
  // Used to find a DOM range in a single parent for a given changed
  // range.
  parseRange(e, n, r = 0) {
    if (this.children.length == 0)
      return { node: this.contentDOM, from: e, to: n, fromOffset: 0, toOffset: this.contentDOM.childNodes.length };
    let i = -1, o = -1;
    for (let s = r, l = 0; ; l++) {
      let a = this.children[l], c = s + a.size;
      if (i == -1 && e <= c) {
        let u = s + a.border;
        if (e >= u && n <= c - a.border && a.node && a.contentDOM && this.contentDOM.contains(a.contentDOM))
          return a.parseRange(e, n, u);
        e = s;
        for (let h = l; h > 0; h--) {
          let f = this.children[h - 1];
          if (f.size && f.dom.parentNode == this.contentDOM && !f.emptyChildAt(1)) {
            i = Se(f.dom) + 1;
            break;
          }
          e -= f.size;
        }
        i == -1 && (i = 0);
      }
      if (i > -1 && (c > n || l == this.children.length - 1)) {
        n = c;
        for (let u = l + 1; u < this.children.length; u++) {
          let h = this.children[u];
          if (h.size && h.dom.parentNode == this.contentDOM && !h.emptyChildAt(-1)) {
            o = Se(h.dom);
            break;
          }
          n += h.size;
        }
        o == -1 && (o = this.contentDOM.childNodes.length);
        break;
      }
      s = c;
    }
    return { node: this.contentDOM, from: e, to: n, fromOffset: i, toOffset: o };
  }
  emptyChildAt(e) {
    if (this.border || !this.contentDOM || !this.children.length)
      return !1;
    let n = this.children[e < 0 ? 0 : this.children.length - 1];
    return n.size == 0 || n.emptyChildAt(e);
  }
  domAfterPos(e) {
    let { node: n, offset: r } = this.domFromPos(e, 0);
    if (n.nodeType != 1 || r == n.childNodes.length)
      throw new RangeError("No node after pos " + e);
    return n.childNodes[r];
  }
  // View descs are responsible for setting any selection that falls
  // entirely inside of them, so that custom implementations can do
  // custom things with the selection. Note that this falls apart when
  // a selection starts in such a node and ends in another, in which
  // case we just use whatever domFromPos produces as a best effort.
  setSelection(e, n, r, i = !1) {
    let o = Math.min(e, n), s = Math.max(e, n);
    for (let d = 0, p = 0; d < this.children.length; d++) {
      let m = this.children[d], y = p + m.size;
      if (o > p && s < y)
        return m.setSelection(e - p - m.border, n - p - m.border, r, i);
      p = y;
    }
    let l = this.domFromPos(e, e ? -1 : 1), a = n == e ? l : this.domFromPos(n, n ? -1 : 1), c = r.root.getSelection(), u = r.domSelectionRange(), h = !1;
    if ((mt || De) && e == n) {
      let { node: d, offset: p } = l;
      if (d.nodeType == 3) {
        if (h = !!(p && d.nodeValue[p - 1] == `
`), h && p == d.nodeValue.length)
          for (let m = d, y; m; m = m.parentNode) {
            if (y = m.nextSibling) {
              y.nodeName == "BR" && (l = a = { node: y.parentNode, offset: Se(y) + 1 });
              break;
            }
            let g = m.pmViewDesc;
            if (g && g.node && g.node.isBlock)
              break;
          }
      } else {
        let m = d.childNodes[p - 1];
        h = m && (m.nodeName == "BR" || m.contentEditable == "false");
      }
    }
    if (mt && u.focusNode && u.focusNode != a.node && u.focusNode.nodeType == 1) {
      let d = u.focusNode.childNodes[u.focusOffset];
      d && d.contentEditable == "false" && (i = !0);
    }
    if (!(i || h && De) && Hn(l.node, l.offset, u.anchorNode, u.anchorOffset) && Hn(a.node, a.offset, u.focusNode, u.focusOffset))
      return;
    let f = !1;
    if ((c.extend || e == n) && !h) {
      c.collapse(l.node, l.offset);
      try {
        e != n && c.extend(a.node, a.offset), f = !0;
      } catch {
      }
    }
    if (!f) {
      if (e > n) {
        let p = l;
        l = a, a = p;
      }
      let d = document.createRange();
      d.setEnd(a.node, a.offset), d.setStart(l.node, l.offset), c.removeAllRanges(), c.addRange(d);
    }
  }
  ignoreMutation(e) {
    return !this.contentDOM && e.type != "selection";
  }
  get contentLost() {
    return this.contentDOM && this.contentDOM != this.dom && !this.dom.contains(this.contentDOM);
  }
  // Remove a subtree of the element tree that has been touched
  // by a DOM change, so that the next update will redraw it.
  markDirty(e, n) {
    for (let r = 0, i = 0; i < this.children.length; i++) {
      let o = this.children[i], s = r + o.size;
      if (r == s ? e <= s && n >= r : e < s && n > r) {
        let l = r + o.border, a = s - o.border;
        if (e >= l && n <= a) {
          this.dirty = e == r || n == s ? Dn : ch, e == l && n == a && (o.contentLost || o.dom.parentNode != this.contentDOM) ? o.dirty = Tt : o.markDirty(e - l, n - l);
          return;
        } else
          o.dirty = o.dom == o.contentDOM && o.dom.parentNode == this.contentDOM && !o.children.length ? Dn : Tt;
      }
      r = s;
    }
    this.dirty = Dn;
  }
  markParentsDirty() {
    let e = 1;
    for (let n = this.parent; n; n = n.parent, e++) {
      let r = e == 1 ? Dn : ch;
      n.dirty < r && (n.dirty = r);
    }
  }
  get domAtom() {
    return !1;
  }
  get ignoreForCoords() {
    return !1;
  }
  isText(e) {
    return !1;
  }
}
class tp extends zi {
  constructor(e, n, r, i) {
    let o, s = n.type.toDOM;
    if (typeof s == "function" && (s = s(r, () => {
      if (!o)
        return i;
      if (o.parent)
        return o.parent.posBeforeChild(o);
    })), !n.type.spec.raw) {
      if (s.nodeType != 1) {
        let l = document.createElement("span");
        l.appendChild(s), s = l;
      }
      s.contentEditable = "false", s.classList.add("ProseMirror-widget");
    }
    super(e, [], s, null), this.widget = n, this.widget = n, o = this;
  }
  matchesWidget(e) {
    return this.dirty == nt && e.type.eq(this.widget.type);
  }
  parseRule() {
    return { ignore: !0 };
  }
  stopEvent(e) {
    let n = this.widget.spec.stopEvent;
    return n ? n(e) : !1;
  }
  ignoreMutation(e) {
    return e.type != "selection" || this.widget.spec.ignoreSelection;
  }
  destroy() {
    this.widget.type.destroy(this.dom), super.destroy();
  }
  get domAtom() {
    return !0;
  }
  get side() {
    return this.widget.type.side;
  }
}
class ex extends zi {
  constructor(e, n, r, i) {
    super(e, [], n, null), this.textDOM = r, this.text = i;
  }
  get size() {
    return this.text.length;
  }
  localPosFromDOM(e, n) {
    return e != this.textDOM ? this.posAtStart + (n ? this.size : 0) : this.posAtStart + n;
  }
  domFromPos(e) {
    return { node: this.textDOM, offset: e };
  }
  ignoreMutation(e) {
    return e.type === "characterData" && e.target.nodeValue == e.oldValue;
  }
}
class jn extends zi {
  constructor(e, n, r, i, o) {
    super(e, [], r, i), this.mark = n, this.spec = o;
  }
  static create(e, n, r, i) {
    let o = i.nodeViews[n.type.name], s = o && o(n, i, r);
    return (!s || !s.dom) && (s = Kn.renderSpec(document, n.type.spec.toDOM(n, r), null, n.attrs)), new jn(e, n, s.dom, s.contentDOM || s.dom, s);
  }
  parseRule() {
    return this.dirty & Tt || this.mark.type.spec.reparseInView ? null : { mark: this.mark.type.name, attrs: this.mark.attrs, contentElement: this.contentDOM };
  }
  matchesMark(e) {
    return this.dirty != Tt && this.mark.eq(e);
  }
  markDirty(e, n) {
    if (super.markDirty(e, n), this.dirty != nt) {
      let r = this.parent;
      for (; !r.node; )
        r = r.parent;
      r.dirty < this.dirty && (r.dirty = this.dirty), this.dirty = nt;
    }
  }
  slice(e, n, r) {
    let i = jn.create(this.parent, this.mark, !0, r), o = this.children, s = this.size;
    n < s && (o = Jl(o, n, s, r)), e > 0 && (o = Jl(o, 0, e, r));
    for (let l = 0; l < o.length; l++)
      o[l].parent = i;
    return i.children = o, i;
  }
  ignoreMutation(e) {
    return this.spec.ignoreMutation ? this.spec.ignoreMutation(e) : super.ignoreMutation(e);
  }
  destroy() {
    this.spec.destroy && this.spec.destroy(), super.destroy();
  }
}
class cn extends zi {
  constructor(e, n, r, i, o, s, l, a, c) {
    super(e, [], o, s), this.node = n, this.outerDeco = r, this.innerDeco = i, this.nodeDOM = l;
  }
  // By default, a node is rendered using the `toDOM` method from the
  // node type spec. But client code can use the `nodeViews` spec to
  // supply a custom node view, which can influence various aspects of
  // the way the node works.
  //
  // (Using subclassing for this was intentionally decided against,
  // since it'd require exposing a whole slew of finicky
  // implementation details to the user code that they probably will
  // never need.)
  static create(e, n, r, i, o, s) {
    let l = o.nodeViews[n.type.name], a, c = l && l(n, o, () => {
      if (!a)
        return s;
      if (a.parent)
        return a.parent.posBeforeChild(a);
    }, r, i), u = c && c.dom, h = c && c.contentDOM;
    if (n.isText) {
      if (!u)
        u = document.createTextNode(n.text);
      else if (u.nodeType != 3)
        throw new RangeError("Text must be rendered as a DOM text node");
    } else u || ({ dom: u, contentDOM: h } = Kn.renderSpec(document, n.type.spec.toDOM(n), null, n.attrs));
    !h && !n.isText && u.nodeName != "BR" && (u.hasAttribute("contenteditable") || (u.contentEditable = "false"), n.type.spec.draggable && (u.draggable = !0));
    let f = u;
    return u = op(u, r, n), c ? a = new tx(e, n, r, i, u, h || null, f, c, o, s + 1) : n.isText ? new ks(e, n, r, i, u, f, o) : new cn(e, n, r, i, u, h || null, f, o, s + 1);
  }
  parseRule() {
    if (this.node.type.spec.reparseInView)
      return null;
    let e = { node: this.node.type.name, attrs: this.node.attrs };
    if (this.node.type.whitespace == "pre" && (e.preserveWhitespace = "full"), !this.contentDOM)
      e.getContent = () => this.node.content;
    else if (!this.contentLost)
      e.contentElement = this.contentDOM;
    else {
      for (let n = this.children.length - 1; n >= 0; n--) {
        let r = this.children[n];
        if (this.dom.contains(r.dom.parentNode)) {
          e.contentElement = r.dom.parentNode;
          break;
        }
      }
      e.contentElement || (e.getContent = () => T.empty);
    }
    return e;
  }
  matchesNode(e, n, r) {
    return this.dirty == nt && e.eq(this.node) && Uo(n, this.outerDeco) && r.eq(this.innerDeco);
  }
  get size() {
    return this.node.nodeSize;
  }
  get border() {
    return this.node.isLeaf ? 0 : 1;
  }
  // Syncs `this.children` to match `this.node.content` and the local
  // decorations, possibly introducing nesting for marks. Then, in a
  // separate step, syncs the DOM inside `this.contentDOM` to
  // `this.children`.
  updateChildren(e, n) {
    let r = this.node.inlineContent, i = n, o = e.composing ? this.localCompositionInfo(e, n) : null, s = o && o.pos > -1 ? o : null, l = o && o.pos < 0, a = new rx(this, s && s.node, e);
    sx(this.node, this.innerDeco, (c, u, h) => {
      c.spec.marks ? a.syncToMarks(c.spec.marks, r, e) : c.type.side >= 0 && !h && a.syncToMarks(u == this.node.childCount ? G.none : this.node.child(u).marks, r, e), a.placeWidget(c, e, i);
    }, (c, u, h, f) => {
      a.syncToMarks(c.marks, r, e);
      let d;
      a.findNodeMatch(c, u, h, f) || l && e.state.selection.from > i && e.state.selection.to < i + c.nodeSize && (d = a.findIndexWithChild(o.node)) > -1 && a.updateNodeAt(c, u, h, d, e) || a.updateNextNode(c, u, h, e, f, i) || a.addNode(c, u, h, e, i), i += c.nodeSize;
    }), a.syncToMarks([], r, e), this.node.isTextblock && a.addTextblockHacks(), a.destroyRest(), (a.changed || this.dirty == Dn) && (s && this.protectLocalComposition(e, s), rp(this.contentDOM, this.children, e), Tr && lx(this.dom));
  }
  localCompositionInfo(e, n) {
    let { from: r, to: i } = e.state.selection;
    if (!(e.state.selection instanceof H) || r < n || i > n + this.node.content.size)
      return null;
    let o = e.input.compositionNode;
    if (!o || !this.dom.contains(o.parentNode))
      return null;
    if (this.node.inlineContent) {
      let s = o.nodeValue, l = ax(this.node.content, s, r - n, i - n);
      return l < 0 ? null : { node: o, pos: l, text: s };
    } else
      return { node: o, pos: -1, text: "" };
  }
  protectLocalComposition(e, { node: n, pos: r, text: i }) {
    if (this.getDesc(n))
      return;
    let o = n;
    for (; o.parentNode != this.contentDOM; o = o.parentNode) {
      for (; o.previousSibling; )
        o.parentNode.removeChild(o.previousSibling);
      for (; o.nextSibling; )
        o.parentNode.removeChild(o.nextSibling);
      o.pmViewDesc && (o.pmViewDesc = void 0);
    }
    let s = new ex(this, o, n, i);
    e.input.compositionNodes.push(s), this.children = Jl(this.children, r, r + i.length, e, s);
  }
  // If this desc must be updated to match the given node decoration,
  // do so and return true.
  update(e, n, r, i) {
    return this.dirty == Tt || !e.sameMarkup(this.node) ? !1 : (this.updateInner(e, n, r, i), !0);
  }
  updateInner(e, n, r, i) {
    this.updateOuterDeco(n), this.node = e, this.innerDeco = r, this.contentDOM && this.updateChildren(i, this.posAtStart), this.dirty = nt;
  }
  updateOuterDeco(e) {
    if (Uo(e, this.outerDeco))
      return;
    let n = this.nodeDOM.nodeType != 1, r = this.dom;
    this.dom = ip(this.dom, this.nodeDOM, Kl(this.outerDeco, this.node, n), Kl(e, this.node, n)), this.dom != r && (r.pmViewDesc = void 0, this.dom.pmViewDesc = this), this.outerDeco = e;
  }
  // Mark this node as being the selected node.
  selectNode() {
    this.nodeDOM.nodeType == 1 && this.nodeDOM.classList.add("ProseMirror-selectednode"), (this.contentDOM || !this.node.type.spec.draggable) && (this.dom.draggable = !0);
  }
  // Remove selected node marking from this node.
  deselectNode() {
    this.nodeDOM.nodeType == 1 && (this.nodeDOM.classList.remove("ProseMirror-selectednode"), (this.contentDOM || !this.node.type.spec.draggable) && this.dom.removeAttribute("draggable"));
  }
  get domAtom() {
    return this.node.isAtom;
  }
}
function uh(t, e, n, r, i) {
  op(r, e, t);
  let o = new cn(void 0, t, e, n, r, r, r, i, 0);
  return o.contentDOM && o.updateChildren(i, 0), o;
}
class ks extends cn {
  constructor(e, n, r, i, o, s, l) {
    super(e, n, r, i, o, null, s, l, 0);
  }
  parseRule() {
    let e = this.nodeDOM.parentNode;
    for (; e && e != this.dom && !e.pmIsDeco; )
      e = e.parentNode;
    return { skip: e || !0 };
  }
  update(e, n, r, i) {
    return this.dirty == Tt || this.dirty != nt && !this.inParent() || !e.sameMarkup(this.node) ? !1 : (this.updateOuterDeco(n), (this.dirty != nt || e.text != this.node.text) && e.text != this.nodeDOM.nodeValue && (this.nodeDOM.nodeValue = e.text, i.trackWrites == this.nodeDOM && (i.trackWrites = null)), this.node = e, this.dirty = nt, !0);
  }
  inParent() {
    let e = this.parent.contentDOM;
    for (let n = this.nodeDOM; n; n = n.parentNode)
      if (n == e)
        return !0;
    return !1;
  }
  domFromPos(e) {
    return { node: this.nodeDOM, offset: e };
  }
  localPosFromDOM(e, n, r) {
    return e == this.nodeDOM ? this.posAtStart + Math.min(n, this.node.text.length) : super.localPosFromDOM(e, n, r);
  }
  ignoreMutation(e) {
    return e.type != "characterData" && e.type != "selection";
  }
  slice(e, n, r) {
    let i = this.node.cut(e, n), o = document.createTextNode(i.text);
    return new ks(this.parent, i, this.outerDeco, this.innerDeco, o, o, r);
  }
  markDirty(e, n) {
    super.markDirty(e, n), this.dom != this.nodeDOM && (e == 0 || n == this.nodeDOM.nodeValue.length) && (this.dirty = Tt);
  }
  get domAtom() {
    return !1;
  }
  isText(e) {
    return this.node.text == e;
  }
}
class np extends zi {
  parseRule() {
    return { ignore: !0 };
  }
  matchesHack(e) {
    return this.dirty == nt && this.dom.nodeName == e;
  }
  get domAtom() {
    return !0;
  }
  get ignoreForCoords() {
    return this.dom.nodeName == "IMG";
  }
}
class tx extends cn {
  constructor(e, n, r, i, o, s, l, a, c, u) {
    super(e, n, r, i, o, s, l, c, u), this.spec = a;
  }
  // A custom `update` method gets to decide whether the update goes
  // through. If it does, and there's a `contentDOM` node, our logic
  // updates the children.
  update(e, n, r, i) {
    if (this.dirty == Tt)
      return !1;
    if (this.spec.update && (this.node.type == e.type || this.spec.multiType)) {
      let o = this.spec.update(e, n, r);
      return o && this.updateInner(e, n, r, i), o;
    } else return !this.contentDOM && !e.isLeaf ? !1 : super.update(e, n, r, i);
  }
  selectNode() {
    this.spec.selectNode ? this.spec.selectNode() : super.selectNode();
  }
  deselectNode() {
    this.spec.deselectNode ? this.spec.deselectNode() : super.deselectNode();
  }
  setSelection(e, n, r, i) {
    this.spec.setSelection ? this.spec.setSelection(e, n, r.root) : super.setSelection(e, n, r, i);
  }
  destroy() {
    this.spec.destroy && this.spec.destroy(), super.destroy();
  }
  stopEvent(e) {
    return this.spec.stopEvent ? this.spec.stopEvent(e) : !1;
  }
  ignoreMutation(e) {
    return this.spec.ignoreMutation ? this.spec.ignoreMutation(e) : super.ignoreMutation(e);
  }
}
function rp(t, e, n) {
  let r = t.firstChild, i = !1;
  for (let o = 0; o < e.length; o++) {
    let s = e[o], l = s.dom;
    if (l.parentNode == t) {
      for (; l != r; )
        r = hh(r), i = !0;
      r = r.nextSibling;
    } else
      i = !0, t.insertBefore(l, r);
    if (s instanceof jn) {
      let a = r ? r.previousSibling : t.lastChild;
      rp(s.contentDOM, s.children, n), r = a ? a.nextSibling : t.firstChild;
    }
  }
  for (; r; )
    r = hh(r), i = !0;
  i && n.trackWrites == t && (n.trackWrites = null);
}
const ui = function(t) {
  t && (this.nodeName = t);
};
ui.prototype = /* @__PURE__ */ Object.create(null);
const Rn = [new ui()];
function Kl(t, e, n) {
  if (t.length == 0)
    return Rn;
  let r = n ? Rn[0] : new ui(), i = [r];
  for (let o = 0; o < t.length; o++) {
    let s = t[o].type.attrs;
    if (s) {
      s.nodeName && i.push(r = new ui(s.nodeName));
      for (let l in s) {
        let a = s[l];
        a != null && (n && i.length == 1 && i.push(r = new ui(e.isInline ? "span" : "div")), l == "class" ? r.class = (r.class ? r.class + " " : "") + a : l == "style" ? r.style = (r.style ? r.style + ";" : "") + a : l != "nodeName" && (r[l] = a));
      }
    }
  }
  return i;
}
function ip(t, e, n, r) {
  if (n == Rn && r == Rn)
    return e;
  let i = e;
  for (let o = 0; o < r.length; o++) {
    let s = r[o], l = n[o];
    if (o) {
      let a;
      l && l.nodeName == s.nodeName && i != t && (a = i.parentNode) && a.nodeName.toLowerCase() == s.nodeName || (a = document.createElement(s.nodeName), a.pmIsDeco = !0, a.appendChild(i), l = Rn[0]), i = a;
    }
    nx(i, l || Rn[0], s);
  }
  return i;
}
function nx(t, e, n) {
  for (let r in e)
    r != "class" && r != "style" && r != "nodeName" && !(r in n) && t.removeAttribute(r);
  for (let r in n)
    r != "class" && r != "style" && r != "nodeName" && n[r] != e[r] && t.setAttribute(r, n[r]);
  if (e.class != n.class) {
    let r = e.class ? e.class.split(" ").filter(Boolean) : [], i = n.class ? n.class.split(" ").filter(Boolean) : [];
    for (let o = 0; o < r.length; o++)
      i.indexOf(r[o]) == -1 && t.classList.remove(r[o]);
    for (let o = 0; o < i.length; o++)
      r.indexOf(i[o]) == -1 && t.classList.add(i[o]);
    t.classList.length == 0 && t.removeAttribute("class");
  }
  if (e.style != n.style) {
    if (e.style) {
      let r = /\s*([\w\-\xa1-\uffff]+)\s*:(?:"(?:\\.|[^"])*"|'(?:\\.|[^'])*'|\(.*?\)|[^;])*/g, i;
      for (; i = r.exec(e.style); )
        t.style.removeProperty(i[1]);
    }
    n.style && (t.style.cssText += n.style);
  }
}
function op(t, e, n) {
  return ip(t, t, Rn, Kl(e, n, t.nodeType != 1));
}
function Uo(t, e) {
  if (t.length != e.length)
    return !1;
  for (let n = 0; n < t.length; n++)
    if (!t[n].type.eq(e[n].type))
      return !1;
  return !0;
}
function hh(t) {
  let e = t.nextSibling;
  return t.parentNode.removeChild(t), e;
}
class rx {
  constructor(e, n, r) {
    this.lock = n, this.view = r, this.index = 0, this.stack = [], this.changed = !1, this.top = e, this.preMatch = ix(e.node.content, e);
  }
  // Destroy and remove the children between the given indices in
  // `this.top`.
  destroyBetween(e, n) {
    if (e != n) {
      for (let r = e; r < n; r++)
        this.top.children[r].destroy();
      this.top.children.splice(e, n - e), this.changed = !0;
    }
  }
  // Destroy all remaining children in `this.top`.
  destroyRest() {
    this.destroyBetween(this.index, this.top.children.length);
  }
  // Sync the current stack of mark descs with the given array of
  // marks, reusing existing mark descs when possible.
  syncToMarks(e, n, r) {
    let i = 0, o = this.stack.length >> 1, s = Math.min(o, e.length);
    for (; i < s && (i == o - 1 ? this.top : this.stack[i + 1 << 1]).matchesMark(e[i]) && e[i].type.spec.spanning !== !1; )
      i++;
    for (; i < o; )
      this.destroyRest(), this.top.dirty = nt, this.index = this.stack.pop(), this.top = this.stack.pop(), o--;
    for (; o < e.length; ) {
      this.stack.push(this.top, this.index + 1);
      let l = -1;
      for (let a = this.index; a < Math.min(this.index + 3, this.top.children.length); a++) {
        let c = this.top.children[a];
        if (c.matchesMark(e[o]) && !this.isLocked(c.dom)) {
          l = a;
          break;
        }
      }
      if (l > -1)
        l > this.index && (this.changed = !0, this.destroyBetween(this.index, l)), this.top = this.top.children[this.index];
      else {
        let a = jn.create(this.top, e[o], n, r);
        this.top.children.splice(this.index, 0, a), this.top = a, this.changed = !0;
      }
      this.index = 0, o++;
    }
  }
  // Try to find a node desc matching the given data. Skip over it and
  // return true when successful.
  findNodeMatch(e, n, r, i) {
    let o = -1, s;
    if (i >= this.preMatch.index && (s = this.preMatch.matches[i - this.preMatch.index]).parent == this.top && s.matchesNode(e, n, r))
      o = this.top.children.indexOf(s, this.index);
    else
      for (let l = this.index, a = Math.min(this.top.children.length, l + 5); l < a; l++) {
        let c = this.top.children[l];
        if (c.matchesNode(e, n, r) && !this.preMatch.matched.has(c)) {
          o = l;
          break;
        }
      }
    return o < 0 ? !1 : (this.destroyBetween(this.index, o), this.index++, !0);
  }
  updateNodeAt(e, n, r, i, o) {
    let s = this.top.children[i];
    return s.dirty == Tt && s.dom == s.contentDOM && (s.dirty = Dn), s.update(e, n, r, o) ? (this.destroyBetween(this.index, i), this.index++, !0) : !1;
  }
  findIndexWithChild(e) {
    for (; ; ) {
      let n = e.parentNode;
      if (!n)
        return -1;
      if (n == this.top.contentDOM) {
        let r = e.pmViewDesc;
        if (r) {
          for (let i = this.index; i < this.top.children.length; i++)
            if (this.top.children[i] == r)
              return i;
        }
        return -1;
      }
      e = n;
    }
  }
  // Try to update the next node, if any, to the given data. Checks
  // pre-matches to avoid overwriting nodes that could still be used.
  updateNextNode(e, n, r, i, o, s) {
    for (let l = this.index; l < this.top.children.length; l++) {
      let a = this.top.children[l];
      if (a instanceof cn) {
        let c = this.preMatch.matched.get(a);
        if (c != null && c != o)
          return !1;
        let u = a.dom, h, f = this.isLocked(u) && !(e.isText && a.node && a.node.isText && a.nodeDOM.nodeValue == e.text && a.dirty != Tt && Uo(n, a.outerDeco));
        if (!f && a.update(e, n, r, i))
          return this.destroyBetween(this.index, l), a.dom != u && (this.changed = !0), this.index++, !0;
        if (!f && (h = this.recreateWrapper(a, e, n, r, i, s)))
          return this.destroyBetween(this.index, l), this.top.children[this.index] = h, h.contentDOM && (h.dirty = Dn, h.updateChildren(i, s + 1), h.dirty = nt), this.changed = !0, this.index++, !0;
        break;
      }
    }
    return !1;
  }
  // When a node with content is replaced by a different node with
  // identical content, move over its children.
  recreateWrapper(e, n, r, i, o, s) {
    if (e.dirty || n.isAtom || !e.children.length || !e.node.content.eq(n.content) || !Uo(r, e.outerDeco) || !i.eq(e.innerDeco))
      return null;
    let l = cn.create(this.top, n, r, i, o, s);
    if (l.contentDOM) {
      l.children = e.children, e.children = [];
      for (let a of l.children)
        a.parent = l;
    }
    return e.destroy(), l;
  }
  // Insert the node as a newly created node desc.
  addNode(e, n, r, i, o) {
    let s = cn.create(this.top, e, n, r, i, o);
    s.contentDOM && s.updateChildren(i, o + 1), this.top.children.splice(this.index++, 0, s), this.changed = !0;
  }
  placeWidget(e, n, r) {
    let i = this.index < this.top.children.length ? this.top.children[this.index] : null;
    if (i && i.matchesWidget(e) && (e == i.widget || !i.widget.type.toDOM.parentNode))
      this.index++;
    else {
      let o = new tp(this.top, e, n, r);
      this.top.children.splice(this.index++, 0, o), this.changed = !0;
    }
  }
  // Make sure a textblock looks and behaves correctly in
  // contentEditable.
  addTextblockHacks() {
    let e = this.top.children[this.index - 1], n = this.top;
    for (; e instanceof jn; )
      n = e, e = n.children[n.children.length - 1];
    (!e || // Empty textblock
    !(e instanceof ks) || /\n$/.test(e.node.text) || this.view.requiresGeckoHackNode && /\s$/.test(e.node.text)) && ((De || Ie) && e && e.dom.contentEditable == "false" && this.addHackNode("IMG", n), this.addHackNode("BR", this.top));
  }
  addHackNode(e, n) {
    if (n == this.top && this.index < n.children.length && n.children[this.index].matchesHack(e))
      this.index++;
    else {
      let r = document.createElement(e);
      e == "IMG" && (r.className = "ProseMirror-separator", r.alt = ""), e == "BR" && (r.className = "ProseMirror-trailingBreak");
      let i = new np(this.top, [], r, null);
      n != this.top ? n.children.push(i) : n.children.splice(this.index++, 0, i), this.changed = !0;
    }
  }
  isLocked(e) {
    return this.lock && (e == this.lock || e.nodeType == 1 && e.contains(this.lock.parentNode));
  }
}
function ix(t, e) {
  let n = e, r = n.children.length, i = t.childCount, o = /* @__PURE__ */ new Map(), s = [];
  e: for (; i > 0; ) {
    let l;
    for (; ; )
      if (r) {
        let c = n.children[r - 1];
        if (c instanceof jn)
          n = c, r = c.children.length;
        else {
          l = c, r--;
          break;
        }
      } else {
        if (n == e)
          break e;
        r = n.parent.children.indexOf(n), n = n.parent;
      }
    let a = l.node;
    if (a) {
      if (a != t.child(i - 1))
        break;
      --i, o.set(l, i), s.push(l);
    }
  }
  return { index: i, matched: o, matches: s.reverse() };
}
function ox(t, e) {
  return t.type.side - e.type.side;
}
function sx(t, e, n, r) {
  let i = e.locals(t), o = 0;
  if (i.length == 0) {
    for (let c = 0; c < t.childCount; c++) {
      let u = t.child(c);
      r(u, i, e.forChild(o, u), c), o += u.nodeSize;
    }
    return;
  }
  let s = 0, l = [], a = null;
  for (let c = 0; ; ) {
    let u, h;
    for (; s < i.length && i[s].to == o; ) {
      let y = i[s++];
      y.widget && (u ? (h || (h = [u])).push(y) : u = y);
    }
    if (u)
      if (h) {
        h.sort(ox);
        for (let y = 0; y < h.length; y++)
          n(h[y], c, !!a);
      } else
        n(u, c, !!a);
    let f, d;
    if (a)
      d = -1, f = a, a = null;
    else if (c < t.childCount)
      d = c, f = t.child(c++);
    else
      break;
    for (let y = 0; y < l.length; y++)
      l[y].to <= o && l.splice(y--, 1);
    for (; s < i.length && i[s].from <= o && i[s].to > o; )
      l.push(i[s++]);
    let p = o + f.nodeSize;
    if (f.isText) {
      let y = p;
      s < i.length && i[s].from < y && (y = i[s].from);
      for (let g = 0; g < l.length; g++)
        l[g].to < y && (y = l[g].to);
      y < p && (a = f.cut(y - o), f = f.cut(0, y - o), p = y, d = -1);
    } else
      for (; s < i.length && i[s].to < p; )
        s++;
    let m = f.isInline && !f.isLeaf ? l.filter((y) => !y.inline) : l.slice();
    r(f, m, e.forChild(o, f), d), o = p;
  }
}
function lx(t) {
  if (t.nodeName == "UL" || t.nodeName == "OL") {
    let e = t.style.cssText;
    t.style.cssText = e + "; list-style: square !important", window.getComputedStyle(t).listStyle, t.style.cssText = e;
  }
}
function ax(t, e, n, r) {
  for (let i = 0, o = 0; i < t.childCount && o <= r; ) {
    let s = t.child(i++), l = o;
    if (o += s.nodeSize, !s.isText)
      continue;
    let a = s.text;
    for (; i < t.childCount; ) {
      let c = t.child(i++);
      if (o += c.nodeSize, !c.isText)
        break;
      a += c.text;
    }
    if (o >= n) {
      if (o >= r && a.slice(r - e.length - l, r - l) == e)
        return r - e.length;
      let c = l < r ? a.lastIndexOf(e, r - l - 1) : -1;
      if (c >= 0 && c + e.length + l >= n)
        return l + c;
      if (n == r && a.length >= r + e.length - l && a.slice(r - l, r - l + e.length) == e)
        return r;
    }
  }
  return -1;
}
function Jl(t, e, n, r, i) {
  let o = [];
  for (let s = 0, l = 0; s < t.length; s++) {
    let a = t[s], c = l, u = l += a.size;
    c >= n || u <= e ? o.push(a) : (c < e && o.push(a.slice(0, e - c, r)), i && (o.push(i), i = void 0), u > n && o.push(a.slice(n - c, a.size, r)));
  }
  return o;
}
function Ea(t, e = null) {
  let n = t.domSelectionRange(), r = t.state.doc;
  if (!n.focusNode)
    return null;
  let i = t.docView.nearestDesc(n.focusNode), o = i && i.size == 0, s = t.docView.posFromDOM(n.focusNode, n.focusOffset, 1);
  if (s < 0)
    return null;
  let l = r.resolve(s), a, c;
  if (ys(n)) {
    for (a = s; i && !i.node; )
      i = i.parent;
    let h = i.node;
    if (i && h.isAtom && _.isSelectable(h) && i.parent && !(h.isInline && Pw(n.focusNode, n.focusOffset, i.dom))) {
      let f = i.posBefore;
      c = new _(s == f ? l : r.resolve(f));
    }
  } else {
    if (n instanceof t.dom.ownerDocument.defaultView.Selection && n.rangeCount > 1) {
      let h = s, f = s;
      for (let d = 0; d < n.rangeCount; d++) {
        let p = n.getRangeAt(d);
        h = Math.min(h, t.docView.posFromDOM(p.startContainer, p.startOffset, 1)), f = Math.max(f, t.docView.posFromDOM(p.endContainer, p.endOffset, -1));
      }
      if (h < 0)
        return null;
      [a, s] = f == t.state.selection.anchor ? [f, h] : [h, f], l = r.resolve(s);
    } else
      a = t.docView.posFromDOM(n.anchorNode, n.anchorOffset, 1);
    if (a < 0)
      return null;
  }
  let u = r.resolve(a);
  if (!c) {
    let h = e == "pointer" || t.state.selection.head < l.pos && !o ? 1 : -1;
    c = Oa(t, u, l, h);
  }
  return c;
}
function sp(t) {
  return t.editable ? t.hasFocus() : ap(t) && document.activeElement && document.activeElement.contains(t.dom);
}
function Wt(t, e = !1) {
  let n = t.state.selection;
  if (lp(t, n), !!sp(t)) {
    if (!e && t.input.mouseDown && t.input.mouseDown.allowDefault && Ie) {
      let r = t.domSelectionRange(), i = t.domObserver.currentSelection;
      if (r.anchorNode && i.anchorNode && Hn(r.anchorNode, r.anchorOffset, i.anchorNode, i.anchorOffset)) {
        t.input.mouseDown.delayedSelectionSync = !0, t.domObserver.setCurSelection();
        return;
      }
    }
    if (t.domObserver.disconnectSelection(), t.cursorWrapper)
      ux(t);
    else {
      let { anchor: r, head: i } = n, o, s;
      fh && !(n instanceof H) && (n.$from.parent.inlineContent || (o = dh(t, n.from)), !n.empty && !n.$from.parent.inlineContent && (s = dh(t, n.to))), t.docView.setSelection(r, i, t, e), fh && (o && ph(o), s && ph(s)), n.visible ? t.dom.classList.remove("ProseMirror-hideselection") : (t.dom.classList.add("ProseMirror-hideselection"), "onselectionchange" in document && cx(t));
    }
    t.domObserver.setCurSelection(), t.domObserver.connectSelection();
  }
}
const fh = De || Ie && Ud < 63;
function dh(t, e) {
  let { node: n, offset: r } = t.docView.domFromPos(e, 0), i = r < n.childNodes.length ? n.childNodes[r] : null, o = r ? n.childNodes[r - 1] : null;
  if (De && i && i.contentEditable == "false")
    return Ys(i);
  if ((!i || i.contentEditable == "false") && (!o || o.contentEditable == "false")) {
    if (i)
      return Ys(i);
    if (o)
      return Ys(o);
  }
}
function Ys(t) {
  return t.contentEditable = "true", De && t.draggable && (t.draggable = !1, t.wasDraggable = !0), t;
}
function ph(t) {
  t.contentEditable = "false", t.wasDraggable && (t.draggable = !0, t.wasDraggable = null);
}
function cx(t) {
  let e = t.dom.ownerDocument;
  e.removeEventListener("selectionchange", t.input.hideSelectionGuard);
  let n = t.domSelectionRange(), r = n.anchorNode, i = n.anchorOffset;
  e.addEventListener("selectionchange", t.input.hideSelectionGuard = () => {
    (n.anchorNode != r || n.anchorOffset != i) && (e.removeEventListener("selectionchange", t.input.hideSelectionGuard), setTimeout(() => {
      (!sp(t) || t.state.selection.visible) && t.dom.classList.remove("ProseMirror-hideselection");
    }, 20));
  });
}
function ux(t) {
  let e = t.domSelection(), n = document.createRange();
  if (!e)
    return;
  let r = t.cursorWrapper.dom, i = r.nodeName == "IMG";
  i ? n.setStart(r.parentNode, Se(r) + 1) : n.setStart(r, 0), n.collapse(!0), e.removeAllRanges(), e.addRange(n), !i && !t.state.selection.visible && _e && an <= 11 && (r.disabled = !0, r.disabled = !1);
}
function lp(t, e) {
  if (e instanceof _) {
    let n = t.docView.descAt(e.from);
    n != t.lastSelectedViewDesc && (mh(t), n && n.selectNode(), t.lastSelectedViewDesc = n);
  } else
    mh(t);
}
function mh(t) {
  t.lastSelectedViewDesc && (t.lastSelectedViewDesc.parent && t.lastSelectedViewDesc.deselectNode(), t.lastSelectedViewDesc = void 0);
}
function Oa(t, e, n, r) {
  return t.someProp("createSelectionBetween", (i) => i(t, e, n)) || H.between(e, n, r);
}
function gh(t) {
  return t.editable && !t.hasFocus() ? !1 : ap(t);
}
function ap(t) {
  let e = t.domSelectionRange();
  if (!e.anchorNode)
    return !1;
  try {
    return t.dom.contains(e.anchorNode.nodeType == 3 ? e.anchorNode.parentNode : e.anchorNode) && (t.editable || t.dom.contains(e.focusNode.nodeType == 3 ? e.focusNode.parentNode : e.focusNode));
  } catch {
    return !1;
  }
}
function hx(t) {
  let e = t.docView.domFromPos(t.state.selection.anchor, 0), n = t.domSelectionRange();
  return Hn(e.node, e.offset, n.anchorNode, n.anchorOffset);
}
function Ul(t, e) {
  let { $anchor: n, $head: r } = t.selection, i = e > 0 ? n.max(r) : n.min(r), o = i.parent.inlineContent ? i.depth ? t.doc.resolve(e > 0 ? i.after() : i.before()) : null : i;
  return o && $.findFrom(o, e);
}
function Zt(t, e) {
  return t.dispatch(t.state.tr.setSelection(e).scrollIntoView()), !0;
}
function yh(t, e, n) {
  let r = t.state.selection;
  if (r instanceof H)
    if (n.indexOf("s") > -1) {
      let { $head: i } = r, o = i.textOffset ? null : e < 0 ? i.nodeBefore : i.nodeAfter;
      if (!o || o.isText || !o.isLeaf)
        return !1;
      let s = t.state.doc.resolve(i.pos + o.nodeSize * (e < 0 ? -1 : 1));
      return Zt(t, new H(r.$anchor, s));
    } else if (r.empty) {
      if (t.endOfTextblock(e > 0 ? "forward" : "backward")) {
        let i = Ul(t.state, e);
        return i && i instanceof _ ? Zt(t, i) : !1;
      } else if (!(Ze && n.indexOf("m") > -1)) {
        let i = r.$head, o = i.textOffset ? null : e < 0 ? i.nodeBefore : i.nodeAfter, s;
        if (!o || o.isText)
          return !1;
        let l = e < 0 ? i.pos - o.nodeSize : i.pos;
        return o.isAtom || (s = t.docView.descAt(l)) && !s.contentDOM ? _.isSelectable(o) ? Zt(t, new _(e < 0 ? t.state.doc.resolve(i.pos - o.nodeSize) : i)) : Pi ? Zt(t, new H(t.state.doc.resolve(e < 0 ? l : l + o.nodeSize))) : !1 : !1;
      }
    } else return !1;
  else {
    if (r instanceof _ && r.node.isInline)
      return Zt(t, new H(e > 0 ? r.$to : r.$from));
    {
      let i = Ul(t.state, e);
      return i ? Zt(t, i) : !1;
    }
  }
}
function Go(t) {
  return t.nodeType == 3 ? t.nodeValue.length : t.childNodes.length;
}
function hi(t, e) {
  let n = t.pmViewDesc;
  return n && n.size == 0 && (e < 0 || t.nextSibling || t.nodeName != "BR");
}
function Zn(t, e) {
  return e < 0 ? fx(t) : dx(t);
}
function fx(t) {
  let e = t.domSelectionRange(), n = e.focusNode, r = e.focusOffset;
  if (!n)
    return;
  let i, o, s = !1;
  for (mt && n.nodeType == 1 && r < Go(n) && hi(n.childNodes[r], -1) && (s = !0); ; )
    if (r > 0) {
      if (n.nodeType != 1)
        break;
      {
        let l = n.childNodes[r - 1];
        if (hi(l, -1))
          i = n, o = --r;
        else if (l.nodeType == 3)
          n = l, r = n.nodeValue.length;
        else
          break;
      }
    } else {
      if (cp(n))
        break;
      {
        let l = n.previousSibling;
        for (; l && hi(l, -1); )
          i = n.parentNode, o = Se(l), l = l.previousSibling;
        if (l)
          n = l, r = Go(n);
        else {
          if (n = n.parentNode, n == t.dom)
            break;
          r = 0;
        }
      }
    }
  s ? Gl(t, n, r) : i && Gl(t, i, o);
}
function dx(t) {
  let e = t.domSelectionRange(), n = e.focusNode, r = e.focusOffset;
  if (!n)
    return;
  let i = Go(n), o, s;
  for (; ; )
    if (r < i) {
      if (n.nodeType != 1)
        break;
      let l = n.childNodes[r];
      if (hi(l, 1))
        o = n, s = ++r;
      else
        break;
    } else {
      if (cp(n))
        break;
      {
        let l = n.nextSibling;
        for (; l && hi(l, 1); )
          o = l.parentNode, s = Se(l) + 1, l = l.nextSibling;
        if (l)
          n = l, r = 0, i = Go(n);
        else {
          if (n = n.parentNode, n == t.dom)
            break;
          r = i = 0;
        }
      }
    }
  o && Gl(t, o, s);
}
function cp(t) {
  let e = t.pmViewDesc;
  return e && e.node && e.node.isBlock;
}
function px(t, e) {
  for (; t && e == t.childNodes.length && !vi(t); )
    e = Se(t) + 1, t = t.parentNode;
  for (; t && e < t.childNodes.length; ) {
    let n = t.childNodes[e];
    if (n.nodeType == 3)
      return n;
    if (n.nodeType == 1 && n.contentEditable == "false")
      break;
    t = n, e = 0;
  }
}
function mx(t, e) {
  for (; t && !e && !vi(t); )
    e = Se(t), t = t.parentNode;
  for (; t && e; ) {
    let n = t.childNodes[e - 1];
    if (n.nodeType == 3)
      return n;
    if (n.nodeType == 1 && n.contentEditable == "false")
      break;
    t = n, e = t.childNodes.length;
  }
}
function Gl(t, e, n) {
  if (e.nodeType != 3) {
    let o, s;
    (s = px(e, n)) ? (e = s, n = 0) : (o = mx(e, n)) && (e = o, n = o.nodeValue.length);
  }
  let r = t.domSelection();
  if (!r)
    return;
  if (ys(r)) {
    let o = document.createRange();
    o.setEnd(e, n), o.setStart(e, n), r.removeAllRanges(), r.addRange(o);
  } else r.extend && r.extend(e, n);
  t.domObserver.setCurSelection();
  let { state: i } = t;
  setTimeout(() => {
    t.state == i && Wt(t);
  }, 50);
}
function kh(t, e) {
  let n = t.state.doc.resolve(e);
  if (!(Ie || Fw) && n.parent.inlineContent) {
    let i = t.coordsAtPos(e);
    if (e > n.start()) {
      let o = t.coordsAtPos(e - 1), s = (o.top + o.bottom) / 2;
      if (s > i.top && s < i.bottom && Math.abs(o.left - i.left) > 1)
        return o.left < i.left ? "ltr" : "rtl";
    }
    if (e < n.end()) {
      let o = t.coordsAtPos(e + 1), s = (o.top + o.bottom) / 2;
      if (s > i.top && s < i.bottom && Math.abs(o.left - i.left) > 1)
        return o.left > i.left ? "ltr" : "rtl";
    }
  }
  return getComputedStyle(t.dom).direction == "rtl" ? "rtl" : "ltr";
}
function bh(t, e, n) {
  let r = t.state.selection;
  if (r instanceof H && !r.empty || n.indexOf("s") > -1 || Ze && n.indexOf("m") > -1)
    return !1;
  let { $from: i, $to: o } = r;
  if (!i.parent.inlineContent || t.endOfTextblock(e < 0 ? "up" : "down")) {
    let s = Ul(t.state, e);
    if (s && s instanceof _)
      return Zt(t, s);
  }
  if (!i.parent.inlineContent) {
    let s = e < 0 ? i : o, l = r instanceof Be ? $.near(s, e) : $.findFrom(s, e);
    return l ? Zt(t, l) : !1;
  }
  return !1;
}
function wh(t, e) {
  if (!(t.state.selection instanceof H))
    return !0;
  let { $head: n, $anchor: r, empty: i } = t.state.selection;
  if (!n.sameParent(r))
    return !0;
  if (!i)
    return !1;
  if (t.endOfTextblock(e > 0 ? "forward" : "backward"))
    return !0;
  let o = !n.textOffset && (e < 0 ? n.nodeBefore : n.nodeAfter);
  if (o && !o.isText) {
    let s = t.state.tr;
    return e < 0 ? s.delete(n.pos - o.nodeSize, n.pos) : s.delete(n.pos, n.pos + o.nodeSize), t.dispatch(s), !0;
  }
  return !1;
}
function xh(t, e, n) {
  t.domObserver.stop(), e.contentEditable = n, t.domObserver.start();
}
function gx(t) {
  if (!De || t.state.selection.$head.parentOffset > 0)
    return !1;
  let { focusNode: e, focusOffset: n } = t.domSelectionRange();
  if (e && e.nodeType == 1 && n == 0 && e.firstChild && e.firstChild.contentEditable == "false") {
    let r = e.firstChild;
    xh(t, r, "true"), setTimeout(() => xh(t, r, "false"), 20);
  }
  return !1;
}
function yx(t) {
  let e = "";
  return t.ctrlKey && (e += "c"), t.metaKey && (e += "m"), t.altKey && (e += "a"), t.shiftKey && (e += "s"), e;
}
function kx(t, e) {
  let n = e.keyCode, r = yx(e);
  if (n == 8 || Ze && n == 72 && r == "c")
    return wh(t, -1) || Zn(t, -1);
  if (n == 46 && !e.shiftKey || Ze && n == 68 && r == "c")
    return wh(t, 1) || Zn(t, 1);
  if (n == 13 || n == 27)
    return !0;
  if (n == 37 || Ze && n == 66 && r == "c") {
    let i = n == 37 ? kh(t, t.state.selection.from) == "ltr" ? -1 : 1 : -1;
    return yh(t, i, r) || Zn(t, i);
  } else if (n == 39 || Ze && n == 70 && r == "c") {
    let i = n == 39 ? kh(t, t.state.selection.from) == "ltr" ? 1 : -1 : 1;
    return yh(t, i, r) || Zn(t, i);
  } else {
    if (n == 38 || Ze && n == 80 && r == "c")
      return bh(t, -1, r) || Zn(t, -1);
    if (n == 40 || Ze && n == 78 && r == "c")
      return gx(t) || bh(t, 1, r) || Zn(t, 1);
    if (r == (Ze ? "m" : "c") && (n == 66 || n == 73 || n == 89 || n == 90))
      return !0;
  }
  return !1;
}
function Da(t, e) {
  t.someProp("transformCopied", (d) => {
    e = d(e, t);
  });
  let n = [], { content: r, openStart: i, openEnd: o } = e;
  for (; i > 1 && o > 1 && r.childCount == 1 && r.firstChild.childCount == 1; ) {
    i--, o--;
    let d = r.firstChild;
    n.push(d.type.name, d.attrs != d.type.defaultAttrs ? d.attrs : null), r = d.content;
  }
  let s = t.someProp("clipboardSerializer") || Kn.fromSchema(t.state.schema), l = mp(), a = l.createElement("div");
  a.appendChild(s.serializeFragment(r, { document: l }));
  let c = a.firstChild, u, h = 0;
  for (; c && c.nodeType == 1 && (u = pp[c.nodeName.toLowerCase()]); ) {
    for (let d = u.length - 1; d >= 0; d--) {
      let p = l.createElement(u[d]);
      for (; a.firstChild; )
        p.appendChild(a.firstChild);
      a.appendChild(p), h++;
    }
    c = a.firstChild;
  }
  c && c.nodeType == 1 && c.setAttribute("data-pm-slice", `${i} ${o}${h ? ` -${h}` : ""} ${JSON.stringify(n)}`);
  let f = t.someProp("clipboardTextSerializer", (d) => d(e, t)) || e.content.textBetween(0, e.content.size, `

`);
  return { dom: a, text: f, slice: e };
}
function up(t, e, n, r, i) {
  let o = i.parent.type.spec.code, s, l;
  if (!n && !e)
    return null;
  let a = e && (r || o || !n);
  if (a) {
    if (t.someProp("transformPastedText", (f) => {
      e = f(e, o || r, t);
    }), o)
      return e ? new E(T.from(t.state.schema.text(e.replace(/\r\n?/g, `
`))), 0, 0) : E.empty;
    let h = t.someProp("clipboardTextParser", (f) => f(e, i, r, t));
    if (h)
      l = h;
    else {
      let f = i.marks(), { schema: d } = t.state, p = Kn.fromSchema(d);
      s = document.createElement("div"), e.split(/(?:\r\n?|\n)+/).forEach((m) => {
        let y = s.appendChild(document.createElement("p"));
        m && y.appendChild(p.serializeNode(d.text(m, f)));
      });
    }
  } else
    t.someProp("transformPastedHTML", (h) => {
      n = h(n, t);
    }), s = Cx(n), Pi && Sx(s);
  let c = s && s.querySelector("[data-pm-slice]"), u = c && /^(\d+) (\d+)(?: -(\d+))? (.*)/.exec(c.getAttribute("data-pm-slice") || "");
  if (u && u[3])
    for (let h = +u[3]; h > 0; h--) {
      let f = s.firstChild;
      for (; f && f.nodeType != 1; )
        f = f.nextSibling;
      if (!f)
        break;
      s = f;
    }
  if (l || (l = (t.someProp("clipboardParser") || t.someProp("domParser") || Wn.fromSchema(t.state.schema)).parseSlice(s, {
    preserveWhitespace: !!(a || u),
    context: i,
    ruleFromNode(f) {
      return f.nodeName == "BR" && !f.nextSibling && f.parentNode && !bx.test(f.parentNode.nodeName) ? { ignore: !0 } : null;
    }
  })), u)
    l = Mx(Ch(l, +u[1], +u[2]), u[4]);
  else if (l = E.maxOpen(wx(l.content, i), !0), l.openStart || l.openEnd) {
    let h = 0, f = 0;
    for (let d = l.content.firstChild; h < l.openStart && !d.type.spec.isolating; h++, d = d.firstChild)
      ;
    for (let d = l.content.lastChild; f < l.openEnd && !d.type.spec.isolating; f++, d = d.lastChild)
      ;
    l = Ch(l, h, f);
  }
  return t.someProp("transformPasted", (h) => {
    l = h(l, t);
  }), l;
}
const bx = /^(a|abbr|acronym|b|cite|code|del|em|i|ins|kbd|label|output|q|ruby|s|samp|span|strong|sub|sup|time|u|tt|var)$/i;
function wx(t, e) {
  if (t.childCount < 2)
    return t;
  for (let n = e.depth; n >= 0; n--) {
    let i = e.node(n).contentMatchAt(e.index(n)), o, s = [];
    if (t.forEach((l) => {
      if (!s)
        return;
      let a = i.findWrapping(l.type), c;
      if (!a)
        return s = null;
      if (c = s.length && o.length && fp(a, o, l, s[s.length - 1], 0))
        s[s.length - 1] = c;
      else {
        s.length && (s[s.length - 1] = dp(s[s.length - 1], o.length));
        let u = hp(l, a);
        s.push(u), i = i.matchType(u.type), o = a;
      }
    }), s)
      return T.from(s);
  }
  return t;
}
function hp(t, e, n = 0) {
  for (let r = e.length - 1; r >= n; r--)
    t = e[r].create(null, T.from(t));
  return t;
}
function fp(t, e, n, r, i) {
  if (i < t.length && i < e.length && t[i] == e[i]) {
    let o = fp(t, e, n, r.lastChild, i + 1);
    if (o)
      return r.copy(r.content.replaceChild(r.childCount - 1, o));
    if (r.contentMatchAt(r.childCount).matchType(i == t.length - 1 ? n.type : t[i + 1]))
      return r.copy(r.content.append(T.from(hp(n, t, i + 1))));
  }
}
function dp(t, e) {
  if (e == 0)
    return t;
  let n = t.content.replaceChild(t.childCount - 1, dp(t.lastChild, e - 1)), r = t.contentMatchAt(t.childCount).fillBefore(T.empty, !0);
  return t.copy(n.append(r));
}
function Yl(t, e, n, r, i, o) {
  let s = e < 0 ? t.firstChild : t.lastChild, l = s.content;
  return t.childCount > 1 && (o = 0), i < r - 1 && (l = Yl(l, e, n, r, i + 1, o)), i >= n && (l = e < 0 ? s.contentMatchAt(0).fillBefore(l, o <= i).append(l) : l.append(s.contentMatchAt(s.childCount).fillBefore(T.empty, !0))), t.replaceChild(e < 0 ? 0 : t.childCount - 1, s.copy(l));
}
function Ch(t, e, n) {
  return e < t.openStart && (t = new E(Yl(t.content, -1, e, t.openStart, 0, t.openEnd), e, t.openEnd)), n < t.openEnd && (t = new E(Yl(t.content, 1, n, t.openEnd, 0, 0), t.openStart, n)), t;
}
const pp = {
  thead: ["table"],
  tbody: ["table"],
  tfoot: ["table"],
  caption: ["table"],
  colgroup: ["table"],
  col: ["table", "colgroup"],
  tr: ["table", "tbody"],
  td: ["table", "tbody", "tr"],
  th: ["table", "tbody", "tr"]
};
let Sh = null;
function mp() {
  return Sh || (Sh = document.implementation.createHTMLDocument("title"));
}
let Qs = null;
function xx(t) {
  let e = window.trustedTypes;
  return e ? (Qs || (Qs = e.defaultPolicy || e.createPolicy("ProseMirrorClipboard", { createHTML: (n) => n })), Qs.createHTML(t)) : t;
}
function Cx(t) {
  let e = /^(\s*<meta [^>]*>)*/.exec(t);
  e && (t = t.slice(e[0].length));
  let n = mp().createElement("div"), r = /<([a-z][^>\s]+)/i.exec(t), i;
  if ((i = r && pp[r[1].toLowerCase()]) && (t = i.map((o) => "<" + o + ">").join("") + t + i.map((o) => "</" + o + ">").reverse().join("")), n.innerHTML = xx(t), i)
    for (let o = 0; o < i.length; o++)
      n = n.querySelector(i[o]) || n;
  return n;
}
function Sx(t) {
  let e = t.querySelectorAll(Ie ? "span:not([class]):not([style])" : "span.Apple-converted-space");
  for (let n = 0; n < e.length; n++) {
    let r = e[n];
    r.childNodes.length == 1 && r.textContent == " " && r.parentNode && r.parentNode.replaceChild(t.ownerDocument.createTextNode(" "), r);
  }
}
function Mx(t, e) {
  if (!t.size)
    return t;
  let n = t.content.firstChild.type.schema, r;
  try {
    r = JSON.parse(e);
  } catch {
    return t;
  }
  let { content: i, openStart: o, openEnd: s } = t;
  for (let l = r.length - 2; l >= 0; l -= 2) {
    let a = n.nodes[r[l]];
    if (!a || a.hasRequiredAttrs())
      break;
    i = T.from(a.create(r[l + 1], i)), o++, s++;
  }
  return new E(i, o, s);
}
const Re = {}, ve = {}, Nx = { touchstart: !0, touchmove: !0 };
class Tx {
  constructor() {
    this.shiftKey = !1, this.mouseDown = null, this.lastKeyCode = null, this.lastKeyCodeTime = 0, this.lastClick = { time: 0, x: 0, y: 0, type: "", button: 0 }, this.lastSelectionOrigin = null, this.lastSelectionTime = 0, this.lastIOSEnter = 0, this.lastIOSEnterFallbackTimeout = -1, this.lastFocus = 0, this.lastTouch = 0, this.lastChromeDelete = 0, this.composing = !1, this.compositionNode = null, this.composingTimeout = -1, this.compositionNodes = [], this.compositionEndedAt = -2e8, this.compositionID = 1, this.compositionPendingChanges = 0, this.domChangeCount = 0, this.eventHandlers = /* @__PURE__ */ Object.create(null), this.hideSelectionGuard = null;
  }
}
function Ix(t) {
  for (let e in Re) {
    let n = Re[e];
    t.dom.addEventListener(e, t.input.eventHandlers[e] = (r) => {
      Ex(t, r) && !Ra(t, r) && (t.editable || !(r.type in ve)) && n(t, r);
    }, Nx[e] ? { passive: !0 } : void 0);
  }
  De && t.dom.addEventListener("input", () => null), Ql(t);
}
function sn(t, e) {
  t.input.lastSelectionOrigin = e, t.input.lastSelectionTime = Date.now();
}
function Ax(t) {
  t.domObserver.stop();
  for (let e in t.input.eventHandlers)
    t.dom.removeEventListener(e, t.input.eventHandlers[e]);
  clearTimeout(t.input.composingTimeout), clearTimeout(t.input.lastIOSEnterFallbackTimeout);
}
function Ql(t) {
  t.someProp("handleDOMEvents", (e) => {
    for (let n in e)
      t.input.eventHandlers[n] || t.dom.addEventListener(n, t.input.eventHandlers[n] = (r) => Ra(t, r));
  });
}
function Ra(t, e) {
  return t.someProp("handleDOMEvents", (n) => {
    let r = n[e.type];
    return r ? r(t, e) || e.defaultPrevented : !1;
  });
}
function Ex(t, e) {
  if (!e.bubbles)
    return !0;
  if (e.defaultPrevented)
    return !1;
  for (let n = e.target; n != t.dom; n = n.parentNode)
    if (!n || n.nodeType == 11 || n.pmViewDesc && n.pmViewDesc.stopEvent(e))
      return !1;
  return !0;
}
function Ox(t, e) {
  !Ra(t, e) && Re[e.type] && (t.editable || !(e.type in ve)) && Re[e.type](t, e);
}
ve.keydown = (t, e) => {
  let n = e;
  if (t.input.shiftKey = n.keyCode == 16 || n.shiftKey, !yp(t, n) && (t.input.lastKeyCode = n.keyCode, t.input.lastKeyCodeTime = Date.now(), !(Vt && Ie && n.keyCode == 13)))
    if (n.keyCode != 229 && t.domObserver.forceFlush(), Tr && n.keyCode == 13 && !n.ctrlKey && !n.altKey && !n.metaKey) {
      let r = Date.now();
      t.input.lastIOSEnter = r, t.input.lastIOSEnterFallbackTimeout = setTimeout(() => {
        t.input.lastIOSEnter == r && (t.someProp("handleKeyDown", (i) => i(t, En(13, "Enter"))), t.input.lastIOSEnter = 0);
      }, 200);
    } else t.someProp("handleKeyDown", (r) => r(t, n)) || kx(t, n) ? n.preventDefault() : sn(t, "key");
};
ve.keyup = (t, e) => {
  e.keyCode == 16 && (t.input.shiftKey = !1);
};
ve.keypress = (t, e) => {
  let n = e;
  if (yp(t, n) || !n.charCode || n.ctrlKey && !n.altKey || Ze && n.metaKey)
    return;
  if (t.someProp("handleKeyPress", (i) => i(t, n))) {
    n.preventDefault();
    return;
  }
  let r = t.state.selection;
  if (!(r instanceof H) || !r.$from.sameParent(r.$to)) {
    let i = String.fromCharCode(n.charCode);
    !/[\r\n]/.test(i) && !t.someProp("handleTextInput", (o) => o(t, r.$from.pos, r.$to.pos, i)) && t.dispatch(t.state.tr.insertText(i).scrollIntoView()), n.preventDefault();
  }
};
function bs(t) {
  return { left: t.clientX, top: t.clientY };
}
function Dx(t, e) {
  let n = e.x - t.clientX, r = e.y - t.clientY;
  return n * n + r * r < 100;
}
function va(t, e, n, r, i) {
  if (r == -1)
    return !1;
  let o = t.state.doc.resolve(r);
  for (let s = o.depth + 1; s > 0; s--)
    if (t.someProp(e, (l) => s > o.depth ? l(t, n, o.nodeAfter, o.before(s), i, !0) : l(t, n, o.node(s), o.before(s), i, !1)))
      return !0;
  return !1;
}
function mr(t, e, n) {
  if (t.focused || t.focus(), t.state.selection.eq(e))
    return;
  let r = t.state.tr.setSelection(e);
  r.setMeta("pointer", !0), t.dispatch(r);
}
function Rx(t, e) {
  if (e == -1)
    return !1;
  let n = t.state.doc.resolve(e), r = n.nodeAfter;
  return r && r.isAtom && _.isSelectable(r) ? (mr(t, new _(n)), !0) : !1;
}
function vx(t, e) {
  if (e == -1)
    return !1;
  let n = t.state.selection, r, i;
  n instanceof _ && (r = n.node);
  let o = t.state.doc.resolve(e);
  for (let s = o.depth + 1; s > 0; s--) {
    let l = s > o.depth ? o.nodeAfter : o.node(s);
    if (_.isSelectable(l)) {
      r && n.$from.depth > 0 && s >= n.$from.depth && o.before(n.$from.depth + 1) == n.$from.pos ? i = o.before(n.$from.depth) : i = o.before(s);
      break;
    }
  }
  return i != null ? (mr(t, _.create(t.state.doc, i)), !0) : !1;
}
function Px(t, e, n, r, i) {
  return va(t, "handleClickOn", e, n, r) || t.someProp("handleClick", (o) => o(t, e, r)) || (i ? vx(t, n) : Rx(t, n));
}
function zx(t, e, n, r) {
  return va(t, "handleDoubleClickOn", e, n, r) || t.someProp("handleDoubleClick", (i) => i(t, e, r));
}
function Lx(t, e, n, r) {
  return va(t, "handleTripleClickOn", e, n, r) || t.someProp("handleTripleClick", (i) => i(t, e, r)) || Fx(t, n, r);
}
function Fx(t, e, n) {
  if (n.button != 0)
    return !1;
  let r = t.state.doc;
  if (e == -1)
    return r.inlineContent ? (mr(t, H.create(r, 0, r.content.size)), !0) : !1;
  let i = r.resolve(e);
  for (let o = i.depth + 1; o > 0; o--) {
    let s = o > i.depth ? i.nodeAfter : i.node(o), l = i.before(o);
    if (s.inlineContent)
      mr(t, H.create(r, l + 1, l + 1 + s.content.size));
    else if (_.isSelectable(s))
      mr(t, _.create(r, l));
    else
      continue;
    return !0;
  }
}
function Pa(t) {
  return Yo(t);
}
const gp = Ze ? "metaKey" : "ctrlKey";
Re.mousedown = (t, e) => {
  let n = e;
  t.input.shiftKey = n.shiftKey;
  let r = Pa(t), i = Date.now(), o = "singleClick";
  i - t.input.lastClick.time < 500 && Dx(n, t.input.lastClick) && !n[gp] && t.input.lastClick.button == n.button && (t.input.lastClick.type == "singleClick" ? o = "doubleClick" : t.input.lastClick.type == "doubleClick" && (o = "tripleClick")), t.input.lastClick = { time: i, x: n.clientX, y: n.clientY, type: o, button: n.button };
  let s = t.posAtCoords(bs(n));
  s && (o == "singleClick" ? (t.input.mouseDown && t.input.mouseDown.done(), t.input.mouseDown = new Bx(t, s, n, !!r)) : (o == "doubleClick" ? zx : Lx)(t, s.pos, s.inside, n) ? n.preventDefault() : sn(t, "pointer"));
};
class Bx {
  constructor(e, n, r, i) {
    this.view = e, this.pos = n, this.event = r, this.flushed = i, this.delayedSelectionSync = !1, this.mightDrag = null, this.startDoc = e.state.doc, this.selectNode = !!r[gp], this.allowDefault = r.shiftKey;
    let o, s;
    if (n.inside > -1)
      o = e.state.doc.nodeAt(n.inside), s = n.inside;
    else {
      let u = e.state.doc.resolve(n.pos);
      o = u.parent, s = u.depth ? u.before() : 0;
    }
    const l = i ? null : r.target, a = l ? e.docView.nearestDesc(l, !0) : null;
    this.target = a && a.dom.nodeType == 1 ? a.dom : null;
    let { selection: c } = e.state;
    (r.button == 0 && o.type.spec.draggable && o.type.spec.selectable !== !1 || c instanceof _ && c.from <= s && c.to > s) && (this.mightDrag = {
      node: o,
      pos: s,
      addAttr: !!(this.target && !this.target.draggable),
      setUneditable: !!(this.target && mt && !this.target.hasAttribute("contentEditable"))
    }), this.target && this.mightDrag && (this.mightDrag.addAttr || this.mightDrag.setUneditable) && (this.view.domObserver.stop(), this.mightDrag.addAttr && (this.target.draggable = !0), this.mightDrag.setUneditable && setTimeout(() => {
      this.view.input.mouseDown == this && this.target.setAttribute("contentEditable", "false");
    }, 20), this.view.domObserver.start()), e.root.addEventListener("mouseup", this.up = this.up.bind(this)), e.root.addEventListener("mousemove", this.move = this.move.bind(this)), sn(e, "pointer");
  }
  done() {
    this.view.root.removeEventListener("mouseup", this.up), this.view.root.removeEventListener("mousemove", this.move), this.mightDrag && this.target && (this.view.domObserver.stop(), this.mightDrag.addAttr && this.target.removeAttribute("draggable"), this.mightDrag.setUneditable && this.target.removeAttribute("contentEditable"), this.view.domObserver.start()), this.delayedSelectionSync && setTimeout(() => Wt(this.view)), this.view.input.mouseDown = null;
  }
  up(e) {
    if (this.done(), !this.view.dom.contains(e.target))
      return;
    let n = this.pos;
    this.view.state.doc != this.startDoc && (n = this.view.posAtCoords(bs(e))), this.updateAllowDefault(e), this.allowDefault || !n ? sn(this.view, "pointer") : Px(this.view, n.pos, n.inside, e, this.selectNode) ? e.preventDefault() : e.button == 0 && (this.flushed || // Safari ignores clicks on draggable elements
    De && this.mightDrag && !this.mightDrag.node.isAtom || // Chrome will sometimes treat a node selection as a
    // cursor, but still report that the node is selected
    // when asked through getSelection. You'll then get a
    // situation where clicking at the point where that
    // (hidden) cursor is doesn't change the selection, and
    // thus doesn't get a reaction from ProseMirror. This
    // works around that.
    Ie && !this.view.state.selection.visible && Math.min(Math.abs(n.pos - this.view.state.selection.from), Math.abs(n.pos - this.view.state.selection.to)) <= 2) ? (mr(this.view, $.near(this.view.state.doc.resolve(n.pos))), e.preventDefault()) : sn(this.view, "pointer");
  }
  move(e) {
    this.updateAllowDefault(e), sn(this.view, "pointer"), e.buttons == 0 && this.done();
  }
  updateAllowDefault(e) {
    !this.allowDefault && (Math.abs(this.event.x - e.clientX) > 4 || Math.abs(this.event.y - e.clientY) > 4) && (this.allowDefault = !0);
  }
}
Re.touchstart = (t) => {
  t.input.lastTouch = Date.now(), Pa(t), sn(t, "pointer");
};
Re.touchmove = (t) => {
  t.input.lastTouch = Date.now(), sn(t, "pointer");
};
Re.contextmenu = (t) => Pa(t);
function yp(t, e) {
  return t.composing ? !0 : De && Math.abs(e.timeStamp - t.input.compositionEndedAt) < 500 ? (t.input.compositionEndedAt = -2e8, !0) : !1;
}
const _x = Vt ? 5e3 : -1;
ve.compositionstart = ve.compositionupdate = (t) => {
  if (!t.composing) {
    t.domObserver.flush();
    let { state: e } = t, n = e.selection.$to;
    if (e.selection instanceof H && (e.storedMarks || !n.textOffset && n.parentOffset && n.nodeBefore.marks.some((r) => r.type.spec.inclusive === !1)))
      t.markCursor = t.state.storedMarks || n.marks(), Yo(t, !0), t.markCursor = null;
    else if (Yo(t, !e.selection.empty), mt && e.selection.empty && n.parentOffset && !n.textOffset && n.nodeBefore.marks.length) {
      let r = t.domSelectionRange();
      for (let i = r.focusNode, o = r.focusOffset; i && i.nodeType == 1 && o != 0; ) {
        let s = o < 0 ? i.lastChild : i.childNodes[o - 1];
        if (!s)
          break;
        if (s.nodeType == 3) {
          let l = t.domSelection();
          l && l.collapse(s, s.nodeValue.length);
          break;
        } else
          i = s, o = -1;
      }
    }
    t.input.composing = !0;
  }
  kp(t, _x);
};
ve.compositionend = (t, e) => {
  t.composing && (t.input.composing = !1, t.input.compositionEndedAt = e.timeStamp, t.input.compositionPendingChanges = t.domObserver.pendingRecords().length ? t.input.compositionID : 0, t.input.compositionNode = null, t.input.compositionPendingChanges && Promise.resolve().then(() => t.domObserver.flush()), t.input.compositionID++, kp(t, 20));
};
function kp(t, e) {
  clearTimeout(t.input.composingTimeout), e > -1 && (t.input.composingTimeout = setTimeout(() => Yo(t), e));
}
function bp(t) {
  for (t.composing && (t.input.composing = !1, t.input.compositionEndedAt = Vx()); t.input.compositionNodes.length > 0; )
    t.input.compositionNodes.pop().markParentsDirty();
}
function $x(t) {
  let e = t.domSelectionRange();
  if (!e.focusNode)
    return null;
  let n = Rw(e.focusNode, e.focusOffset), r = vw(e.focusNode, e.focusOffset);
  if (n && r && n != r) {
    let i = r.pmViewDesc, o = t.domObserver.lastChangedTextNode;
    if (n == o || r == o)
      return o;
    if (!i || !i.isText(r.nodeValue))
      return r;
    if (t.input.compositionNode == r) {
      let s = n.pmViewDesc;
      if (!(!s || !s.isText(n.nodeValue)))
        return r;
    }
  }
  return n || r;
}
function Vx() {
  let t = document.createEvent("Event");
  return t.initEvent("event", !0, !0), t.timeStamp;
}
function Yo(t, e = !1) {
  if (!(Vt && t.domObserver.flushingSoon >= 0)) {
    if (t.domObserver.forceFlush(), bp(t), e || t.docView && t.docView.dirty) {
      let n = Ea(t), r = t.state.selection;
      return n && !n.eq(r) ? t.dispatch(t.state.tr.setSelection(n)) : (t.markCursor || e) && !r.$from.node(r.$from.sharedDepth(r.to)).inlineContent ? t.dispatch(t.state.tr.deleteSelection()) : t.updateState(t.state), !0;
    }
    return !1;
  }
}
function Wx(t, e) {
  if (!t.dom.parentNode)
    return;
  let n = t.dom.parentNode.appendChild(document.createElement("div"));
  n.appendChild(e), n.style.cssText = "position: fixed; left: -10000px; top: 10px";
  let r = getSelection(), i = document.createRange();
  i.selectNodeContents(e), t.dom.blur(), r.removeAllRanges(), r.addRange(i), setTimeout(() => {
    n.parentNode && n.parentNode.removeChild(n), t.focus();
  }, 50);
}
const Ci = _e && an < 15 || Tr && Bw < 604;
Re.copy = ve.cut = (t, e) => {
  let n = e, r = t.state.selection, i = n.type == "cut";
  if (r.empty)
    return;
  let o = Ci ? null : n.clipboardData, s = r.content(), { dom: l, text: a } = Da(t, s);
  o ? (n.preventDefault(), o.clearData(), o.setData("text/html", l.innerHTML), o.setData("text/plain", a)) : Wx(t, l), i && t.dispatch(t.state.tr.deleteSelection().scrollIntoView().setMeta("uiEvent", "cut"));
};
function Hx(t) {
  return t.openStart == 0 && t.openEnd == 0 && t.content.childCount == 1 ? t.content.firstChild : null;
}
function jx(t, e) {
  if (!t.dom.parentNode)
    return;
  let n = t.input.shiftKey || t.state.selection.$from.parent.type.spec.code, r = t.dom.parentNode.appendChild(document.createElement(n ? "textarea" : "div"));
  n || (r.contentEditable = "true"), r.style.cssText = "position: fixed; left: -10000px; top: 10px", r.focus();
  let i = t.input.shiftKey && t.input.lastKeyCode != 45;
  setTimeout(() => {
    t.focus(), r.parentNode && r.parentNode.removeChild(r), n ? Si(t, r.value, null, i, e) : Si(t, r.textContent, r.innerHTML, i, e);
  }, 50);
}
function Si(t, e, n, r, i) {
  let o = up(t, e, n, r, t.state.selection.$from);
  if (t.someProp("handlePaste", (a) => a(t, i, o || E.empty)))
    return !0;
  if (!o)
    return !1;
  let s = Hx(o), l = s ? t.state.tr.replaceSelectionWith(s, r) : t.state.tr.replaceSelection(o);
  return t.dispatch(l.scrollIntoView().setMeta("paste", !0).setMeta("uiEvent", "paste")), !0;
}
function wp(t) {
  let e = t.getData("text/plain") || t.getData("Text");
  if (e)
    return e;
  let n = t.getData("text/uri-list");
  return n ? n.replace(/\r?\n/g, " ") : "";
}
ve.paste = (t, e) => {
  let n = e;
  if (t.composing && !Vt)
    return;
  let r = Ci ? null : n.clipboardData, i = t.input.shiftKey && t.input.lastKeyCode != 45;
  r && Si(t, wp(r), r.getData("text/html"), i, n) ? n.preventDefault() : jx(t, n);
};
class xp {
  constructor(e, n, r) {
    this.slice = e, this.move = n, this.node = r;
  }
}
const qx = Ze ? "altKey" : "ctrlKey";
function Cp(t, e) {
  let n = t.someProp("dragCopies", (r) => !r(e));
  return n ?? !e[qx];
}
Re.dragstart = (t, e) => {
  let n = e, r = t.input.mouseDown;
  if (r && r.done(), !n.dataTransfer)
    return;
  let i = t.state.selection, o = i.empty ? null : t.posAtCoords(bs(n)), s;
  if (!(o && o.pos >= i.from && o.pos <= (i instanceof _ ? i.to - 1 : i.to))) {
    if (r && r.mightDrag)
      s = _.create(t.state.doc, r.mightDrag.pos);
    else if (n.target && n.target.nodeType == 1) {
      let h = t.docView.nearestDesc(n.target, !0);
      h && h.node.type.spec.draggable && h != t.docView && (s = _.create(t.state.doc, h.posBefore));
    }
  }
  let l = (s || t.state.selection).content(), { dom: a, text: c, slice: u } = Da(t, l);
  (!n.dataTransfer.files.length || !Ie || Ud > 120) && n.dataTransfer.clearData(), n.dataTransfer.setData(Ci ? "Text" : "text/html", a.innerHTML), n.dataTransfer.effectAllowed = "copyMove", Ci || n.dataTransfer.setData("text/plain", c), t.dragging = new xp(u, Cp(t, n), s);
};
Re.dragend = (t) => {
  let e = t.dragging;
  window.setTimeout(() => {
    t.dragging == e && (t.dragging = null);
  }, 50);
};
ve.dragover = ve.dragenter = (t, e) => e.preventDefault();
ve.drop = (t, e) => {
  let n = e, r = t.dragging;
  if (t.dragging = null, !n.dataTransfer)
    return;
  let i = t.posAtCoords(bs(n));
  if (!i)
    return;
  let o = t.state.doc.resolve(i.pos), s = r && r.slice;
  s ? t.someProp("transformPasted", (p) => {
    s = p(s, t);
  }) : s = up(t, wp(n.dataTransfer), Ci ? null : n.dataTransfer.getData("text/html"), !1, o);
  let l = !!(r && Cp(t, n));
  if (t.someProp("handleDrop", (p) => p(t, n, s || E.empty, l))) {
    n.preventDefault();
    return;
  }
  if (!s)
    return;
  n.preventDefault();
  let a = s ? T0(t.state.doc, o.pos, s) : o.pos;
  a == null && (a = o.pos);
  let c = t.state.tr;
  if (l) {
    let { node: p } = r;
    p ? p.replace(c) : c.deleteSelection();
  }
  let u = c.mapping.map(a), h = s.openStart == 0 && s.openEnd == 0 && s.content.childCount == 1, f = c.doc;
  if (h ? c.replaceRangeWith(u, u, s.content.firstChild) : c.replaceRange(u, u, s), c.doc.eq(f))
    return;
  let d = c.doc.resolve(u);
  if (h && _.isSelectable(s.content.firstChild) && d.nodeAfter && d.nodeAfter.sameMarkup(s.content.firstChild))
    c.setSelection(new _(d));
  else {
    let p = c.mapping.map(a);
    c.mapping.maps[c.mapping.maps.length - 1].forEach((m, y, g, N) => p = N), c.setSelection(Oa(t, d, c.doc.resolve(p)));
  }
  t.focus(), t.dispatch(c.setMeta("uiEvent", "drop"));
};
Re.focus = (t) => {
  t.input.lastFocus = Date.now(), t.focused || (t.domObserver.stop(), t.dom.classList.add("ProseMirror-focused"), t.domObserver.start(), t.focused = !0, setTimeout(() => {
    t.docView && t.hasFocus() && !t.domObserver.currentSelection.eq(t.domSelectionRange()) && Wt(t);
  }, 20));
};
Re.blur = (t, e) => {
  let n = e;
  t.focused && (t.domObserver.stop(), t.dom.classList.remove("ProseMirror-focused"), t.domObserver.start(), n.relatedTarget && t.dom.contains(n.relatedTarget) && t.domObserver.currentSelection.clear(), t.focused = !1);
};
Re.beforeinput = (t, e) => {
  if (Ie && Vt && e.inputType == "deleteContentBackward") {
    t.domObserver.flushSoon();
    let { domChangeCount: r } = t.input;
    setTimeout(() => {
      if (t.input.domChangeCount != r || (t.dom.blur(), t.focus(), t.someProp("handleKeyDown", (o) => o(t, En(8, "Backspace")))))
        return;
      let { $cursor: i } = t.state.selection;
      i && i.pos > 0 && t.dispatch(t.state.tr.delete(i.pos - 1, i.pos).scrollIntoView());
    }, 50);
  }
};
for (let t in ve)
  Re[t] = ve[t];
function Mi(t, e) {
  if (t == e)
    return !0;
  for (let n in t)
    if (t[n] !== e[n])
      return !1;
  for (let n in e)
    if (!(n in t))
      return !1;
  return !0;
}
class Qo {
  constructor(e, n) {
    this.toDOM = e, this.spec = n || zn, this.side = this.spec.side || 0;
  }
  map(e, n, r, i) {
    let { pos: o, deleted: s } = e.mapResult(n.from + i, this.side < 0 ? -1 : 1);
    return s ? null : new Me(o - r, o - r, this);
  }
  valid() {
    return !0;
  }
  eq(e) {
    return this == e || e instanceof Qo && (this.spec.key && this.spec.key == e.spec.key || this.toDOM == e.toDOM && Mi(this.spec, e.spec));
  }
  destroy(e) {
    this.spec.destroy && this.spec.destroy(e);
  }
}
class un {
  constructor(e, n) {
    this.attrs = e, this.spec = n || zn;
  }
  map(e, n, r, i) {
    let o = e.map(n.from + i, this.spec.inclusiveStart ? -1 : 1) - r, s = e.map(n.to + i, this.spec.inclusiveEnd ? 1 : -1) - r;
    return o >= s ? null : new Me(o, s, this);
  }
  valid(e, n) {
    return n.from < n.to;
  }
  eq(e) {
    return this == e || e instanceof un && Mi(this.attrs, e.attrs) && Mi(this.spec, e.spec);
  }
  static is(e) {
    return e.type instanceof un;
  }
  destroy() {
  }
}
class za {
  constructor(e, n) {
    this.attrs = e, this.spec = n || zn;
  }
  map(e, n, r, i) {
    let o = e.mapResult(n.from + i, 1);
    if (o.deleted)
      return null;
    let s = e.mapResult(n.to + i, -1);
    return s.deleted || s.pos <= o.pos ? null : new Me(o.pos - r, s.pos - r, this);
  }
  valid(e, n) {
    let { index: r, offset: i } = e.content.findIndex(n.from), o;
    return i == n.from && !(o = e.child(r)).isText && i + o.nodeSize == n.to;
  }
  eq(e) {
    return this == e || e instanceof za && Mi(this.attrs, e.attrs) && Mi(this.spec, e.spec);
  }
  destroy() {
  }
}
class Me {
  /**
  @internal
  */
  constructor(e, n, r) {
    this.from = e, this.to = n, this.type = r;
  }
  /**
  @internal
  */
  copy(e, n) {
    return new Me(e, n, this.type);
  }
  /**
  @internal
  */
  eq(e, n = 0) {
    return this.type.eq(e.type) && this.from + n == e.from && this.to + n == e.to;
  }
  /**
  @internal
  */
  map(e, n, r) {
    return this.type.map(e, this, n, r);
  }
  /**
  Creates a widget decoration, which is a DOM node that's shown in
  the document at the given position. It is recommended that you
  delay rendering the widget by passing a function that will be
  called when the widget is actually drawn in a view, but you can
  also directly pass a DOM node. `getPos` can be used to find the
  widget's current document position.
  */
  static widget(e, n, r) {
    return new Me(e, e, new Qo(n, r));
  }
  /**
  Creates an inline decoration, which adds the given attributes to
  each inline node between `from` and `to`.
  */
  static inline(e, n, r, i) {
    return new Me(e, n, new un(r, i));
  }
  /**
  Creates a node decoration. `from` and `to` should point precisely
  before and after a node in the document. That node, and only that
  node, will receive the given attributes.
  */
  static node(e, n, r, i) {
    return new Me(e, n, new za(r, i));
  }
  /**
  The spec provided when creating this decoration. Can be useful
  if you've stored extra information in that object.
  */
  get spec() {
    return this.type.spec;
  }
  /**
  @internal
  */
  get inline() {
    return this.type instanceof un;
  }
  /**
  @internal
  */
  get widget() {
    return this.type instanceof Qo;
  }
}
const ir = [], zn = {};
class le {
  /**
  @internal
  */
  constructor(e, n) {
    this.local = e.length ? e : ir, this.children = n.length ? n : ir;
  }
  /**
  Create a set of decorations, using the structure of the given
  document. This will consume (modify) the `decorations` array, so
  you must make a copy if you want need to preserve that.
  */
  static create(e, n) {
    return n.length ? Xo(n, e, 0, zn) : Te;
  }
  /**
  Find all decorations in this set which touch the given range
  (including decorations that start or end directly at the
  boundaries) and match the given predicate on their spec. When
  `start` and `end` are omitted, all decorations in the set are
  considered. When `predicate` isn't given, all decorations are
  assumed to match.
  */
  find(e, n, r) {
    let i = [];
    return this.findInner(e ?? 0, n ?? 1e9, i, 0, r), i;
  }
  findInner(e, n, r, i, o) {
    for (let s = 0; s < this.local.length; s++) {
      let l = this.local[s];
      l.from <= n && l.to >= e && (!o || o(l.spec)) && r.push(l.copy(l.from + i, l.to + i));
    }
    for (let s = 0; s < this.children.length; s += 3)
      if (this.children[s] < n && this.children[s + 1] > e) {
        let l = this.children[s] + 1;
        this.children[s + 2].findInner(e - l, n - l, r, i + l, o);
      }
  }
  /**
  Map the set of decorations in response to a change in the
  document.
  */
  map(e, n, r) {
    return this == Te || e.maps.length == 0 ? this : this.mapInner(e, n, 0, 0, r || zn);
  }
  /**
  @internal
  */
  mapInner(e, n, r, i, o) {
    let s;
    for (let l = 0; l < this.local.length; l++) {
      let a = this.local[l].map(e, r, i);
      a && a.type.valid(n, a) ? (s || (s = [])).push(a) : o.onRemove && o.onRemove(this.local[l].spec);
    }
    return this.children.length ? Kx(this.children, s || [], e, n, r, i, o) : s ? new le(s.sort(Ln), ir) : Te;
  }
  /**
  Add the given array of decorations to the ones in the set,
  producing a new set. Consumes the `decorations` array. Needs
  access to the current document to create the appropriate tree
  structure.
  */
  add(e, n) {
    return n.length ? this == Te ? le.create(e, n) : this.addInner(e, n, 0) : this;
  }
  addInner(e, n, r) {
    let i, o = 0;
    e.forEach((l, a) => {
      let c = a + r, u;
      if (u = Mp(n, l, c)) {
        for (i || (i = this.children.slice()); o < i.length && i[o] < a; )
          o += 3;
        i[o] == a ? i[o + 2] = i[o + 2].addInner(l, u, c + 1) : i.splice(o, 0, a, a + l.nodeSize, Xo(u, l, c + 1, zn)), o += 3;
      }
    });
    let s = Sp(o ? Np(n) : n, -r);
    for (let l = 0; l < s.length; l++)
      s[l].type.valid(e, s[l]) || s.splice(l--, 1);
    return new le(s.length ? this.local.concat(s).sort(Ln) : this.local, i || this.children);
  }
  /**
  Create a new set that contains the decorations in this set, minus
  the ones in the given array.
  */
  remove(e) {
    return e.length == 0 || this == Te ? this : this.removeInner(e, 0);
  }
  removeInner(e, n) {
    let r = this.children, i = this.local;
    for (let o = 0; o < r.length; o += 3) {
      let s, l = r[o] + n, a = r[o + 1] + n;
      for (let u = 0, h; u < e.length; u++)
        (h = e[u]) && h.from > l && h.to < a && (e[u] = null, (s || (s = [])).push(h));
      if (!s)
        continue;
      r == this.children && (r = this.children.slice());
      let c = r[o + 2].removeInner(s, l + 1);
      c != Te ? r[o + 2] = c : (r.splice(o, 3), o -= 3);
    }
    if (i.length) {
      for (let o = 0, s; o < e.length; o++)
        if (s = e[o])
          for (let l = 0; l < i.length; l++)
            i[l].eq(s, n) && (i == this.local && (i = this.local.slice()), i.splice(l--, 1));
    }
    return r == this.children && i == this.local ? this : i.length || r.length ? new le(i, r) : Te;
  }
  forChild(e, n) {
    if (this == Te)
      return this;
    if (n.isLeaf)
      return le.empty;
    let r, i;
    for (let l = 0; l < this.children.length; l += 3)
      if (this.children[l] >= e) {
        this.children[l] == e && (r = this.children[l + 2]);
        break;
      }
    let o = e + 1, s = o + n.content.size;
    for (let l = 0; l < this.local.length; l++) {
      let a = this.local[l];
      if (a.from < s && a.to > o && a.type instanceof un) {
        let c = Math.max(o, a.from) - o, u = Math.min(s, a.to) - o;
        c < u && (i || (i = [])).push(a.copy(c, u));
      }
    }
    if (i) {
      let l = new le(i.sort(Ln), ir);
      return r ? new nn([l, r]) : l;
    }
    return r || Te;
  }
  /**
  @internal
  */
  eq(e) {
    if (this == e)
      return !0;
    if (!(e instanceof le) || this.local.length != e.local.length || this.children.length != e.children.length)
      return !1;
    for (let n = 0; n < this.local.length; n++)
      if (!this.local[n].eq(e.local[n]))
        return !1;
    for (let n = 0; n < this.children.length; n += 3)
      if (this.children[n] != e.children[n] || this.children[n + 1] != e.children[n + 1] || !this.children[n + 2].eq(e.children[n + 2]))
        return !1;
    return !0;
  }
  /**
  @internal
  */
  locals(e) {
    return La(this.localsInner(e));
  }
  /**
  @internal
  */
  localsInner(e) {
    if (this == Te)
      return ir;
    if (e.inlineContent || !this.local.some(un.is))
      return this.local;
    let n = [];
    for (let r = 0; r < this.local.length; r++)
      this.local[r].type instanceof un || n.push(this.local[r]);
    return n;
  }
  forEachSet(e) {
    e(this);
  }
}
le.empty = new le([], []);
le.removeOverlap = La;
const Te = le.empty;
class nn {
  constructor(e) {
    this.members = e;
  }
  map(e, n) {
    const r = this.members.map((i) => i.map(e, n, zn));
    return nn.from(r);
  }
  forChild(e, n) {
    if (n.isLeaf)
      return le.empty;
    let r = [];
    for (let i = 0; i < this.members.length; i++) {
      let o = this.members[i].forChild(e, n);
      o != Te && (o instanceof nn ? r = r.concat(o.members) : r.push(o));
    }
    return nn.from(r);
  }
  eq(e) {
    if (!(e instanceof nn) || e.members.length != this.members.length)
      return !1;
    for (let n = 0; n < this.members.length; n++)
      if (!this.members[n].eq(e.members[n]))
        return !1;
    return !0;
  }
  locals(e) {
    let n, r = !0;
    for (let i = 0; i < this.members.length; i++) {
      let o = this.members[i].localsInner(e);
      if (o.length)
        if (!n)
          n = o;
        else {
          r && (n = n.slice(), r = !1);
          for (let s = 0; s < o.length; s++)
            n.push(o[s]);
        }
    }
    return n ? La(r ? n : n.sort(Ln)) : ir;
  }
  // Create a group for the given array of decoration sets, or return
  // a single set when possible.
  static from(e) {
    switch (e.length) {
      case 0:
        return Te;
      case 1:
        return e[0];
      default:
        return new nn(e.every((n) => n instanceof le) ? e : e.reduce((n, r) => n.concat(r instanceof le ? r : r.members), []));
    }
  }
  forEachSet(e) {
    for (let n = 0; n < this.members.length; n++)
      this.members[n].forEachSet(e);
  }
}
function Kx(t, e, n, r, i, o, s) {
  let l = t.slice();
  for (let c = 0, u = o; c < n.maps.length; c++) {
    let h = 0;
    n.maps[c].forEach((f, d, p, m) => {
      let y = m - p - (d - f);
      for (let g = 0; g < l.length; g += 3) {
        let N = l[g + 1];
        if (N < 0 || f > N + u - h)
          continue;
        let C = l[g] + u - h;
        d >= C ? l[g + 1] = f <= C ? -2 : -1 : f >= u && y && (l[g] += y, l[g + 1] += y);
      }
      h += y;
    }), u = n.maps[c].map(u, -1);
  }
  let a = !1;
  for (let c = 0; c < l.length; c += 3)
    if (l[c + 1] < 0) {
      if (l[c + 1] == -2) {
        a = !0, l[c + 1] = -1;
        continue;
      }
      let u = n.map(t[c] + o), h = u - i;
      if (h < 0 || h >= r.content.size) {
        a = !0;
        continue;
      }
      let f = n.map(t[c + 1] + o, -1), d = f - i, { index: p, offset: m } = r.content.findIndex(h), y = r.maybeChild(p);
      if (y && m == h && m + y.nodeSize == d) {
        let g = l[c + 2].mapInner(n, y, u + 1, t[c] + o + 1, s);
        g != Te ? (l[c] = h, l[c + 1] = d, l[c + 2] = g) : (l[c + 1] = -2, a = !0);
      } else
        a = !0;
    }
  if (a) {
    let c = Jx(l, t, e, n, i, o, s), u = Xo(c, r, 0, s);
    e = u.local;
    for (let h = 0; h < l.length; h += 3)
      l[h + 1] < 0 && (l.splice(h, 3), h -= 3);
    for (let h = 0, f = 0; h < u.children.length; h += 3) {
      let d = u.children[h];
      for (; f < l.length && l[f] < d; )
        f += 3;
      l.splice(f, 0, u.children[h], u.children[h + 1], u.children[h + 2]);
    }
  }
  return new le(e.sort(Ln), l);
}
function Sp(t, e) {
  if (!e || !t.length)
    return t;
  let n = [];
  for (let r = 0; r < t.length; r++) {
    let i = t[r];
    n.push(new Me(i.from + e, i.to + e, i.type));
  }
  return n;
}
function Jx(t, e, n, r, i, o, s) {
  function l(a, c) {
    for (let u = 0; u < a.local.length; u++) {
      let h = a.local[u].map(r, i, c);
      h ? n.push(h) : s.onRemove && s.onRemove(a.local[u].spec);
    }
    for (let u = 0; u < a.children.length; u += 3)
      l(a.children[u + 2], a.children[u] + c + 1);
  }
  for (let a = 0; a < t.length; a += 3)
    t[a + 1] == -1 && l(t[a + 2], e[a] + o + 1);
  return n;
}
function Mp(t, e, n) {
  if (e.isLeaf)
    return null;
  let r = n + e.nodeSize, i = null;
  for (let o = 0, s; o < t.length; o++)
    (s = t[o]) && s.from > n && s.to < r && ((i || (i = [])).push(s), t[o] = null);
  return i;
}
function Np(t) {
  let e = [];
  for (let n = 0; n < t.length; n++)
    t[n] != null && e.push(t[n]);
  return e;
}
function Xo(t, e, n, r) {
  let i = [], o = !1;
  e.forEach((l, a) => {
    let c = Mp(t, l, a + n);
    if (c) {
      o = !0;
      let u = Xo(c, l, n + a + 1, r);
      u != Te && i.push(a, a + l.nodeSize, u);
    }
  });
  let s = Sp(o ? Np(t) : t, -n).sort(Ln);
  for (let l = 0; l < s.length; l++)
    s[l].type.valid(e, s[l]) || (r.onRemove && r.onRemove(s[l].spec), s.splice(l--, 1));
  return s.length || i.length ? new le(s, i) : Te;
}
function Ln(t, e) {
  return t.from - e.from || t.to - e.to;
}
function La(t) {
  let e = t;
  for (let n = 0; n < e.length - 1; n++) {
    let r = e[n];
    if (r.from != r.to)
      for (let i = n + 1; i < e.length; i++) {
        let o = e[i];
        if (o.from == r.from) {
          o.to != r.to && (e == t && (e = t.slice()), e[i] = o.copy(o.from, r.to), Mh(e, i + 1, o.copy(r.to, o.to)));
          continue;
        } else {
          o.from < r.to && (e == t && (e = t.slice()), e[n] = r.copy(r.from, o.from), Mh(e, i, r.copy(o.from, r.to)));
          break;
        }
      }
  }
  return e;
}
function Mh(t, e, n) {
  for (; e < t.length && Ln(n, t[e]) > 0; )
    e++;
  t.splice(e, 0, n);
}
function Xs(t) {
  let e = [];
  return t.someProp("decorations", (n) => {
    let r = n(t.state);
    r && r != Te && e.push(r);
  }), t.cursorWrapper && e.push(le.create(t.state.doc, [t.cursorWrapper.deco])), nn.from(e);
}
const Ux = {
  childList: !0,
  characterData: !0,
  characterDataOldValue: !0,
  attributes: !0,
  attributeOldValue: !0,
  subtree: !0
}, Gx = _e && an <= 11;
class Yx {
  constructor() {
    this.anchorNode = null, this.anchorOffset = 0, this.focusNode = null, this.focusOffset = 0;
  }
  set(e) {
    this.anchorNode = e.anchorNode, this.anchorOffset = e.anchorOffset, this.focusNode = e.focusNode, this.focusOffset = e.focusOffset;
  }
  clear() {
    this.anchorNode = this.focusNode = null;
  }
  eq(e) {
    return e.anchorNode == this.anchorNode && e.anchorOffset == this.anchorOffset && e.focusNode == this.focusNode && e.focusOffset == this.focusOffset;
  }
}
class Qx {
  constructor(e, n) {
    this.view = e, this.handleDOMChange = n, this.queue = [], this.flushingSoon = -1, this.observer = null, this.currentSelection = new Yx(), this.onCharData = null, this.suppressingSelectionUpdates = !1, this.lastChangedTextNode = null, this.observer = window.MutationObserver && new window.MutationObserver((r) => {
      for (let i = 0; i < r.length; i++)
        this.queue.push(r[i]);
      _e && an <= 11 && r.some((i) => i.type == "childList" && i.removedNodes.length || i.type == "characterData" && i.oldValue.length > i.target.nodeValue.length) ? this.flushSoon() : this.flush();
    }), Gx && (this.onCharData = (r) => {
      this.queue.push({ target: r.target, type: "characterData", oldValue: r.prevValue }), this.flushSoon();
    }), this.onSelectionChange = this.onSelectionChange.bind(this);
  }
  flushSoon() {
    this.flushingSoon < 0 && (this.flushingSoon = window.setTimeout(() => {
      this.flushingSoon = -1, this.flush();
    }, 20));
  }
  forceFlush() {
    this.flushingSoon > -1 && (window.clearTimeout(this.flushingSoon), this.flushingSoon = -1, this.flush());
  }
  start() {
    this.observer && (this.observer.takeRecords(), this.observer.observe(this.view.dom, Ux)), this.onCharData && this.view.dom.addEventListener("DOMCharacterDataModified", this.onCharData), this.connectSelection();
  }
  stop() {
    if (this.observer) {
      let e = this.observer.takeRecords();
      if (e.length) {
        for (let n = 0; n < e.length; n++)
          this.queue.push(e[n]);
        window.setTimeout(() => this.flush(), 20);
      }
      this.observer.disconnect();
    }
    this.onCharData && this.view.dom.removeEventListener("DOMCharacterDataModified", this.onCharData), this.disconnectSelection();
  }
  connectSelection() {
    this.view.dom.ownerDocument.addEventListener("selectionchange", this.onSelectionChange);
  }
  disconnectSelection() {
    this.view.dom.ownerDocument.removeEventListener("selectionchange", this.onSelectionChange);
  }
  suppressSelectionUpdates() {
    this.suppressingSelectionUpdates = !0, setTimeout(() => this.suppressingSelectionUpdates = !1, 50);
  }
  onSelectionChange() {
    if (gh(this.view)) {
      if (this.suppressingSelectionUpdates)
        return Wt(this.view);
      if (_e && an <= 11 && !this.view.state.selection.empty) {
        let e = this.view.domSelectionRange();
        if (e.focusNode && Hn(e.focusNode, e.focusOffset, e.anchorNode, e.anchorOffset))
          return this.flushSoon();
      }
      this.flush();
    }
  }
  setCurSelection() {
    this.currentSelection.set(this.view.domSelectionRange());
  }
  ignoreSelectionChange(e) {
    if (!e.focusNode)
      return !0;
    let n = /* @__PURE__ */ new Set(), r;
    for (let o = e.focusNode; o; o = Nr(o))
      n.add(o);
    for (let o = e.anchorNode; o; o = Nr(o))
      if (n.has(o)) {
        r = o;
        break;
      }
    let i = r && this.view.docView.nearestDesc(r);
    if (i && i.ignoreMutation({
      type: "selection",
      target: r.nodeType == 3 ? r.parentNode : r
    }))
      return this.setCurSelection(), !0;
  }
  pendingRecords() {
    if (this.observer)
      for (let e of this.observer.takeRecords())
        this.queue.push(e);
    return this.queue;
  }
  flush() {
    let { view: e } = this;
    if (!e.docView || this.flushingSoon > -1)
      return;
    let n = this.pendingRecords();
    n.length && (this.queue = []);
    let r = e.domSelectionRange(), i = !this.suppressingSelectionUpdates && !this.currentSelection.eq(r) && gh(e) && !this.ignoreSelectionChange(r), o = -1, s = -1, l = !1, a = [];
    if (e.editable)
      for (let u = 0; u < n.length; u++) {
        let h = this.registerMutation(n[u], a);
        h && (o = o < 0 ? h.from : Math.min(h.from, o), s = s < 0 ? h.to : Math.max(h.to, s), h.typeOver && (l = !0));
      }
    if (mt && a.length) {
      let u = a.filter((h) => h.nodeName == "BR");
      if (u.length == 2) {
        let [h, f] = u;
        h.parentNode && h.parentNode.parentNode == f.parentNode ? f.remove() : h.remove();
      } else {
        let { focusNode: h } = this.currentSelection;
        for (let f of u) {
          let d = f.parentNode;
          d && d.nodeName == "LI" && (!h || eC(e, h) != d) && f.remove();
        }
      }
    }
    let c = null;
    o < 0 && i && e.input.lastFocus > Date.now() - 200 && Math.max(e.input.lastTouch, e.input.lastClick.time) < Date.now() - 300 && ys(r) && (c = Ea(e)) && c.eq($.near(e.state.doc.resolve(0), 1)) ? (e.input.lastFocus = 0, Wt(e), this.currentSelection.set(r), e.scrollToSelection()) : (o > -1 || i) && (o > -1 && (e.docView.markDirty(o, s), Xx(e)), this.handleDOMChange(o, s, l, a), e.docView && e.docView.dirty ? e.updateState(e.state) : this.currentSelection.eq(r) || Wt(e), this.currentSelection.set(r));
  }
  registerMutation(e, n) {
    if (n.indexOf(e.target) > -1)
      return null;
    let r = this.view.docView.nearestDesc(e.target);
    if (e.type == "attributes" && (r == this.view.docView || e.attributeName == "contenteditable" || // Firefox sometimes fires spurious events for null/empty styles
    e.attributeName == "style" && !e.oldValue && !e.target.getAttribute("style")) || !r || r.ignoreMutation(e))
      return null;
    if (e.type == "childList") {
      for (let u = 0; u < e.addedNodes.length; u++) {
        let h = e.addedNodes[u];
        n.push(h), h.nodeType == 3 && (this.lastChangedTextNode = h);
      }
      if (r.contentDOM && r.contentDOM != r.dom && !r.contentDOM.contains(e.target))
        return { from: r.posBefore, to: r.posAfter };
      let i = e.previousSibling, o = e.nextSibling;
      if (_e && an <= 11 && e.addedNodes.length)
        for (let u = 0; u < e.addedNodes.length; u++) {
          let { previousSibling: h, nextSibling: f } = e.addedNodes[u];
          (!h || Array.prototype.indexOf.call(e.addedNodes, h) < 0) && (i = h), (!f || Array.prototype.indexOf.call(e.addedNodes, f) < 0) && (o = f);
        }
      let s = i && i.parentNode == e.target ? Se(i) + 1 : 0, l = r.localPosFromDOM(e.target, s, -1), a = o && o.parentNode == e.target ? Se(o) : e.target.childNodes.length, c = r.localPosFromDOM(e.target, a, 1);
      return { from: l, to: c };
    } else return e.type == "attributes" ? { from: r.posAtStart - r.border, to: r.posAtEnd + r.border } : (this.lastChangedTextNode = e.target, {
      from: r.posAtStart,
      to: r.posAtEnd,
      // An event was generated for a text change that didn't change
      // any text. Mark the dom change to fall back to assuming the
      // selection was typed over with an identical value if it can't
      // find another change.
      typeOver: e.target.nodeValue == e.oldValue
    });
  }
}
let Nh = /* @__PURE__ */ new WeakMap(), Th = !1;
function Xx(t) {
  if (!Nh.has(t) && (Nh.set(t, null), ["normal", "nowrap", "pre-line"].indexOf(getComputedStyle(t.dom).whiteSpace) !== -1)) {
    if (t.requiresGeckoHackNode = mt, Th)
      return;
    console.warn("ProseMirror expects the CSS white-space property to be set, preferably to 'pre-wrap'. It is recommended to load style/prosemirror.css from the prosemirror-view package."), Th = !0;
  }
}
function Ih(t, e) {
  let n = e.startContainer, r = e.startOffset, i = e.endContainer, o = e.endOffset, s = t.domAtPos(t.state.selection.anchor);
  return Hn(s.node, s.offset, i, o) && ([n, r, i, o] = [i, o, n, r]), { anchorNode: n, anchorOffset: r, focusNode: i, focusOffset: o };
}
function Zx(t, e) {
  if (e.getComposedRanges) {
    let i = e.getComposedRanges(t.root)[0];
    if (i)
      return Ih(t, i);
  }
  let n;
  function r(i) {
    i.preventDefault(), i.stopImmediatePropagation(), n = i.getTargetRanges()[0];
  }
  return t.dom.addEventListener("beforeinput", r, !0), document.execCommand("indent"), t.dom.removeEventListener("beforeinput", r, !0), n ? Ih(t, n) : null;
}
function eC(t, e) {
  for (let n = e.parentNode; n && n != t.dom; n = n.parentNode) {
    let r = t.docView.nearestDesc(n, !0);
    if (r && r.node.isBlock)
      return n;
  }
  return null;
}
function tC(t, e, n) {
  let { node: r, fromOffset: i, toOffset: o, from: s, to: l } = t.docView.parseRange(e, n), a = t.domSelectionRange(), c, u = a.anchorNode;
  if (u && t.dom.contains(u.nodeType == 1 ? u : u.parentNode) && (c = [{ node: u, offset: a.anchorOffset }], ys(a) || c.push({ node: a.focusNode, offset: a.focusOffset })), Ie && t.input.lastKeyCode === 8)
    for (let y = o; y > i; y--) {
      let g = r.childNodes[y - 1], N = g.pmViewDesc;
      if (g.nodeName == "BR" && !N) {
        o = y;
        break;
      }
      if (!N || N.size)
        break;
    }
  let h = t.state.doc, f = t.someProp("domParser") || Wn.fromSchema(t.state.schema), d = h.resolve(s), p = null, m = f.parse(r, {
    topNode: d.parent,
    topMatch: d.parent.contentMatchAt(d.index()),
    topOpen: !0,
    from: i,
    to: o,
    preserveWhitespace: d.parent.type.whitespace == "pre" ? "full" : !0,
    findPositions: c,
    ruleFromNode: nC,
    context: d
  });
  if (c && c[0].pos != null) {
    let y = c[0].pos, g = c[1] && c[1].pos;
    g == null && (g = y), p = { anchor: y + s, head: g + s };
  }
  return { doc: m, sel: p, from: s, to: l };
}
function nC(t) {
  let e = t.pmViewDesc;
  if (e)
    return e.parseRule();
  if (t.nodeName == "BR" && t.parentNode) {
    if (De && /^(ul|ol)$/i.test(t.parentNode.nodeName)) {
      let n = document.createElement("div");
      return n.appendChild(document.createElement("li")), { skip: n };
    } else if (t.parentNode.lastChild == t || De && /^(tr|table)$/i.test(t.parentNode.nodeName))
      return { ignore: !0 };
  } else if (t.nodeName == "IMG" && t.getAttribute("mark-placeholder"))
    return { ignore: !0 };
  return null;
}
const rC = /^(a|abbr|acronym|b|bd[io]|big|br|button|cite|code|data(list)?|del|dfn|em|i|ins|kbd|label|map|mark|meter|output|q|ruby|s|samp|small|span|strong|su[bp]|time|u|tt|var)$/i;
function iC(t, e, n, r, i) {
  let o = t.input.compositionPendingChanges || (t.composing ? t.input.compositionID : 0);
  if (t.input.compositionPendingChanges = 0, e < 0) {
    let O = t.input.lastSelectionTime > Date.now() - 50 ? t.input.lastSelectionOrigin : null, x = Ea(t, O);
    if (x && !t.state.selection.eq(x)) {
      if (Ie && Vt && t.input.lastKeyCode === 13 && Date.now() - 100 < t.input.lastKeyCodeTime && t.someProp("handleKeyDown", (D) => D(t, En(13, "Enter"))))
        return;
      let R = t.state.tr.setSelection(x);
      O == "pointer" ? R.setMeta("pointer", !0) : O == "key" && R.scrollIntoView(), o && R.setMeta("composition", o), t.dispatch(R);
    }
    return;
  }
  let s = t.state.doc.resolve(e), l = s.sharedDepth(n);
  e = s.before(l + 1), n = t.state.doc.resolve(n).after(l + 1);
  let a = t.state.selection, c = tC(t, e, n), u = t.state.doc, h = u.slice(c.from, c.to), f, d;
  t.input.lastKeyCode === 8 && Date.now() - 100 < t.input.lastKeyCodeTime ? (f = t.state.selection.to, d = "end") : (f = t.state.selection.from, d = "start"), t.input.lastKeyCode = null;
  let p = lC(h.content, c.doc.content, c.from, f, d);
  if (p && t.input.domChangeCount++, (Tr && t.input.lastIOSEnter > Date.now() - 225 || Vt) && i.some((O) => O.nodeType == 1 && !rC.test(O.nodeName)) && (!p || p.endA >= p.endB) && t.someProp("handleKeyDown", (O) => O(t, En(13, "Enter")))) {
    t.input.lastIOSEnter = 0;
    return;
  }
  if (!p)
    if (r && a instanceof H && !a.empty && a.$head.sameParent(a.$anchor) && !t.composing && !(c.sel && c.sel.anchor != c.sel.head))
      p = { start: a.from, endA: a.to, endB: a.to };
    else {
      if (c.sel) {
        let O = Ah(t, t.state.doc, c.sel);
        if (O && !O.eq(t.state.selection)) {
          let x = t.state.tr.setSelection(O);
          o && x.setMeta("composition", o), t.dispatch(x);
        }
      }
      return;
    }
  t.state.selection.from < t.state.selection.to && p.start == p.endB && t.state.selection instanceof H && (p.start > t.state.selection.from && p.start <= t.state.selection.from + 2 && t.state.selection.from >= c.from ? p.start = t.state.selection.from : p.endA < t.state.selection.to && p.endA >= t.state.selection.to - 2 && t.state.selection.to <= c.to && (p.endB += t.state.selection.to - p.endA, p.endA = t.state.selection.to)), _e && an <= 11 && p.endB == p.start + 1 && p.endA == p.start && p.start > c.from && c.doc.textBetween(p.start - c.from - 1, p.start - c.from + 1) == "  " && (p.start--, p.endA--, p.endB--);
  let m = c.doc.resolveNoCache(p.start - c.from), y = c.doc.resolveNoCache(p.endB - c.from), g = u.resolve(p.start), N = m.sameParent(y) && m.parent.inlineContent && g.end() >= p.endA, C;
  if ((Tr && t.input.lastIOSEnter > Date.now() - 225 && (!N || i.some((O) => O.nodeName == "DIV" || O.nodeName == "P")) || !N && m.pos < c.doc.content.size && (!m.sameParent(y) || !m.parent.inlineContent) && !/\S/.test(c.doc.textBetween(m.pos, y.pos, "", "")) && (C = $.findFrom(c.doc.resolve(m.pos + 1), 1, !0)) && C.head > m.pos) && t.someProp("handleKeyDown", (O) => O(t, En(13, "Enter")))) {
    t.input.lastIOSEnter = 0;
    return;
  }
  if (t.state.selection.anchor > p.start && sC(u, p.start, p.endA, m, y) && t.someProp("handleKeyDown", (O) => O(t, En(8, "Backspace")))) {
    Vt && Ie && t.domObserver.suppressSelectionUpdates();
    return;
  }
  Ie && p.endB == p.start && (t.input.lastChromeDelete = Date.now()), Vt && !N && m.start() != y.start() && y.parentOffset == 0 && m.depth == y.depth && c.sel && c.sel.anchor == c.sel.head && c.sel.head == p.endA && (p.endB -= 2, y = c.doc.resolveNoCache(p.endB - c.from), setTimeout(() => {
    t.someProp("handleKeyDown", function(O) {
      return O(t, En(13, "Enter"));
    });
  }, 20));
  let v = p.start, I = p.endA, w, z, W;
  if (N) {
    if (m.pos == y.pos)
      _e && an <= 11 && m.parentOffset == 0 && (t.domObserver.suppressSelectionUpdates(), setTimeout(() => Wt(t), 20)), w = t.state.tr.delete(v, I), z = u.resolve(p.start).marksAcross(u.resolve(p.endA));
    else if (
      // Adding or removing a mark
      p.endA == p.endB && (W = oC(m.parent.content.cut(m.parentOffset, y.parentOffset), g.parent.content.cut(g.parentOffset, p.endA - g.start())))
    )
      w = t.state.tr, W.type == "add" ? w.addMark(v, I, W.mark) : w.removeMark(v, I, W.mark);
    else if (m.parent.child(m.index()).isText && m.index() == y.index() - (y.textOffset ? 0 : 1)) {
      let O = m.parent.textBetween(m.parentOffset, y.parentOffset);
      if (t.someProp("handleTextInput", (x) => x(t, v, I, O)))
        return;
      w = t.state.tr.insertText(O, v, I);
    }
  }
  if (w || (w = t.state.tr.replace(v, I, c.doc.slice(p.start - c.from, p.endB - c.from))), c.sel) {
    let O = Ah(t, w.doc, c.sel);
    O && !(Ie && t.composing && O.empty && (p.start != p.endB || t.input.lastChromeDelete < Date.now() - 100) && (O.head == v || O.head == w.mapping.map(I) - 1) || _e && O.empty && O.head == v) && w.setSelection(O);
  }
  z && w.ensureMarks(z), o && w.setMeta("composition", o), t.dispatch(w.scrollIntoView());
}
function Ah(t, e, n) {
  return Math.max(n.anchor, n.head) > e.content.size ? null : Oa(t, e.resolve(n.anchor), e.resolve(n.head));
}
function oC(t, e) {
  let n = t.firstChild.marks, r = e.firstChild.marks, i = n, o = r, s, l, a;
  for (let u = 0; u < r.length; u++)
    i = r[u].removeFromSet(i);
  for (let u = 0; u < n.length; u++)
    o = n[u].removeFromSet(o);
  if (i.length == 1 && o.length == 0)
    l = i[0], s = "add", a = (u) => u.mark(l.addToSet(u.marks));
  else if (i.length == 0 && o.length == 1)
    l = o[0], s = "remove", a = (u) => u.mark(l.removeFromSet(u.marks));
  else
    return null;
  let c = [];
  for (let u = 0; u < e.childCount; u++)
    c.push(a(e.child(u)));
  if (T.from(c).eq(t))
    return { mark: l, type: s };
}
function sC(t, e, n, r, i) {
  if (
    // The content must have shrunk
    n - e <= i.pos - r.pos || // newEnd must point directly at or after the end of the block that newStart points into
    Zs(r, !0, !1) < i.pos
  )
    return !1;
  let o = t.resolve(e);
  if (!r.parent.isTextblock) {
    let l = o.nodeAfter;
    return l != null && n == e + l.nodeSize;
  }
  if (o.parentOffset < o.parent.content.size || !o.parent.isTextblock)
    return !1;
  let s = t.resolve(Zs(o, !0, !0));
  return !s.parent.isTextblock || s.pos > n || Zs(s, !0, !1) < n ? !1 : r.parent.content.cut(r.parentOffset).eq(s.parent.content);
}
function Zs(t, e, n) {
  let r = t.depth, i = e ? t.end() : t.pos;
  for (; r > 0 && (e || t.indexAfter(r) == t.node(r).childCount); )
    r--, i++, e = !1;
  if (n) {
    let o = t.node(r).maybeChild(t.indexAfter(r));
    for (; o && !o.isLeaf; )
      o = o.firstChild, i++;
  }
  return i;
}
function lC(t, e, n, r, i) {
  let o = t.findDiffStart(e, n);
  if (o == null)
    return null;
  let { a: s, b: l } = t.findDiffEnd(e, n + t.size, n + e.size);
  if (i == "end") {
    let a = Math.max(0, o - Math.min(s, l));
    r -= s + a - o;
  }
  if (s < o && t.size < e.size) {
    let a = r <= o && r >= s ? o - r : 0;
    o -= a, o && o < e.size && Eh(e.textBetween(o - 1, o + 1)) && (o += a ? 1 : -1), l = o + (l - s), s = o;
  } else if (l < o) {
    let a = r <= o && r >= l ? o - r : 0;
    o -= a, o && o < t.size && Eh(t.textBetween(o - 1, o + 1)) && (o += a ? 1 : -1), s = o + (s - l), l = o;
  }
  return { start: o, endA: s, endB: l };
}
function Eh(t) {
  if (t.length != 2)
    return !1;
  let e = t.charCodeAt(0), n = t.charCodeAt(1);
  return e >= 56320 && e <= 57343 && n >= 55296 && n <= 56319;
}
class aC {
  /**
  Create a view. `place` may be a DOM node that the editor should
  be appended to, a function that will place it into the document,
  or an object whose `mount` property holds the node to use as the
  document container. If it is `null`, the editor will not be
  added to the document.
  */
  constructor(e, n) {
    this._root = null, this.focused = !1, this.trackWrites = null, this.mounted = !1, this.markCursor = null, this.cursorWrapper = null, this.lastSelectedViewDesc = void 0, this.input = new Tx(), this.prevDirectPlugins = [], this.pluginViews = [], this.requiresGeckoHackNode = !1, this.dragging = null, this._props = n, this.state = n.state, this.directPlugins = n.plugins || [], this.directPlugins.forEach(Ph), this.dispatch = this.dispatch.bind(this), this.dom = e && e.mount || document.createElement("div"), e && (e.appendChild ? e.appendChild(this.dom) : typeof e == "function" ? e(this.dom) : e.mount && (this.mounted = !0)), this.editable = Rh(this), Dh(this), this.nodeViews = vh(this), this.docView = uh(this.state.doc, Oh(this), Xs(this), this.dom, this), this.domObserver = new Qx(this, (r, i, o, s) => iC(this, r, i, o, s)), this.domObserver.start(), Ix(this), this.updatePluginViews();
  }
  /**
  Holds `true` when a
  [composition](https://w3c.github.io/uievents/#events-compositionevents)
  is active.
  */
  get composing() {
    return this.input.composing;
  }
  /**
  The view's current [props](https://prosemirror.net/docs/ref/#view.EditorProps).
  */
  get props() {
    if (this._props.state != this.state) {
      let e = this._props;
      this._props = {};
      for (let n in e)
        this._props[n] = e[n];
      this._props.state = this.state;
    }
    return this._props;
  }
  /**
  Update the view's props. Will immediately cause an update to
  the DOM.
  */
  update(e) {
    e.handleDOMEvents != this._props.handleDOMEvents && Ql(this);
    let n = this._props;
    this._props = e, e.plugins && (e.plugins.forEach(Ph), this.directPlugins = e.plugins), this.updateStateInner(e.state, n);
  }
  /**
  Update the view by updating existing props object with the object
  given as argument. Equivalent to `view.update(Object.assign({},
  view.props, props))`.
  */
  setProps(e) {
    let n = {};
    for (let r in this._props)
      n[r] = this._props[r];
    n.state = this.state;
    for (let r in e)
      n[r] = e[r];
    this.update(n);
  }
  /**
  Update the editor's `state` prop, without touching any of the
  other props.
  */
  updateState(e) {
    this.updateStateInner(e, this._props);
  }
  updateStateInner(e, n) {
    var r;
    let i = this.state, o = !1, s = !1;
    e.storedMarks && this.composing && (bp(this), s = !0), this.state = e;
    let l = i.plugins != e.plugins || this._props.plugins != n.plugins;
    if (l || this._props.plugins != n.plugins || this._props.nodeViews != n.nodeViews) {
      let d = vh(this);
      uC(d, this.nodeViews) && (this.nodeViews = d, o = !0);
    }
    (l || n.handleDOMEvents != this._props.handleDOMEvents) && Ql(this), this.editable = Rh(this), Dh(this);
    let a = Xs(this), c = Oh(this), u = i.plugins != e.plugins && !i.doc.eq(e.doc) ? "reset" : e.scrollToSelection > i.scrollToSelection ? "to selection" : "preserve", h = o || !this.docView.matchesNode(e.doc, c, a);
    (h || !e.selection.eq(i.selection)) && (s = !0);
    let f = u == "preserve" && s && this.dom.style.overflowAnchor == null && Vw(this);
    if (s) {
      this.domObserver.stop();
      let d = h && (_e || Ie) && !this.composing && !i.selection.empty && !e.selection.empty && cC(i.selection, e.selection);
      if (h) {
        let p = Ie ? this.trackWrites = this.domSelectionRange().focusNode : null;
        this.composing && (this.input.compositionNode = $x(this)), (o || !this.docView.update(e.doc, c, a, this)) && (this.docView.updateOuterDeco(c), this.docView.destroy(), this.docView = uh(e.doc, c, a, this.dom, this)), p && !this.trackWrites && (d = !0);
      }
      d || !(this.input.mouseDown && this.domObserver.currentSelection.eq(this.domSelectionRange()) && hx(this)) ? Wt(this, d) : (lp(this, e.selection), this.domObserver.setCurSelection()), this.domObserver.start();
    }
    this.updatePluginViews(i), !((r = this.dragging) === null || r === void 0) && r.node && !i.doc.eq(e.doc) && this.updateDraggedNode(this.dragging, i), u == "reset" ? this.dom.scrollTop = 0 : u == "to selection" ? this.scrollToSelection() : f && Ww(f);
  }
  /**
  @internal
  */
  scrollToSelection() {
    let e = this.domSelectionRange().focusNode;
    if (!(!e || !this.dom.contains(e.nodeType == 1 ? e : e.parentNode))) {
      if (!this.someProp("handleScrollToSelection", (n) => n(this))) if (this.state.selection instanceof _) {
        let n = this.docView.domAfterPos(this.state.selection.from);
        n.nodeType == 1 && ih(this, n.getBoundingClientRect(), e);
      } else
        ih(this, this.coordsAtPos(this.state.selection.head, 1), e);
    }
  }
  destroyPluginViews() {
    let e;
    for (; e = this.pluginViews.pop(); )
      e.destroy && e.destroy();
  }
  updatePluginViews(e) {
    if (!e || e.plugins != this.state.plugins || this.directPlugins != this.prevDirectPlugins) {
      this.prevDirectPlugins = this.directPlugins, this.destroyPluginViews();
      for (let n = 0; n < this.directPlugins.length; n++) {
        let r = this.directPlugins[n];
        r.spec.view && this.pluginViews.push(r.spec.view(this));
      }
      for (let n = 0; n < this.state.plugins.length; n++) {
        let r = this.state.plugins[n];
        r.spec.view && this.pluginViews.push(r.spec.view(this));
      }
    } else
      for (let n = 0; n < this.pluginViews.length; n++) {
        let r = this.pluginViews[n];
        r.update && r.update(this, e);
      }
  }
  updateDraggedNode(e, n) {
    let r = e.node, i = -1;
    if (this.state.doc.nodeAt(r.from) == r.node)
      i = r.from;
    else {
      let o = r.from + (this.state.doc.content.size - n.doc.content.size);
      (o > 0 && this.state.doc.nodeAt(o)) == r.node && (i = o);
    }
    this.dragging = new xp(e.slice, e.move, i < 0 ? void 0 : _.create(this.state.doc, i));
  }
  someProp(e, n) {
    let r = this._props && this._props[e], i;
    if (r != null && (i = n ? n(r) : r))
      return i;
    for (let s = 0; s < this.directPlugins.length; s++) {
      let l = this.directPlugins[s].props[e];
      if (l != null && (i = n ? n(l) : l))
        return i;
    }
    let o = this.state.plugins;
    if (o)
      for (let s = 0; s < o.length; s++) {
        let l = o[s].props[e];
        if (l != null && (i = n ? n(l) : l))
          return i;
      }
  }
  /**
  Query whether the view has focus.
  */
  hasFocus() {
    if (_e) {
      let e = this.root.activeElement;
      if (e == this.dom)
        return !0;
      if (!e || !this.dom.contains(e))
        return !1;
      for (; e && this.dom != e && this.dom.contains(e); ) {
        if (e.contentEditable == "false")
          return !1;
        e = e.parentElement;
      }
      return !0;
    }
    return this.root.activeElement == this.dom;
  }
  /**
  Focus the editor.
  */
  focus() {
    this.domObserver.stop(), this.editable && Hw(this.dom), Wt(this), this.domObserver.start();
  }
  /**
  Get the document root in which the editor exists. This will
  usually be the top-level `document`, but might be a [shadow
  DOM](https://developer.mozilla.org/en-US/docs/Web/Web_Components/Shadow_DOM)
  root if the editor is inside one.
  */
  get root() {
    let e = this._root;
    if (e == null) {
      for (let n = this.dom.parentNode; n; n = n.parentNode)
        if (n.nodeType == 9 || n.nodeType == 11 && n.host)
          return n.getSelection || (Object.getPrototypeOf(n).getSelection = () => n.ownerDocument.getSelection()), this._root = n;
    }
    return e || document;
  }
  /**
  When an existing editor view is moved to a new document or
  shadow tree, call this to make it recompute its root.
  */
  updateRoot() {
    this._root = null;
  }
  /**
  Given a pair of viewport coordinates, return the document
  position that corresponds to them. May return null if the given
  coordinates aren't inside of the editor. When an object is
  returned, its `pos` property is the position nearest to the
  coordinates, and its `inside` property holds the position of the
  inner node that the position falls inside of, or -1 if it is at
  the top level, not in any node.
  */
  posAtCoords(e) {
    return Uw(this, e);
  }
  /**
  Returns the viewport rectangle at a given document position.
  `left` and `right` will be the same number, as this returns a
  flat cursor-ish rectangle. If the position is between two things
  that aren't directly adjacent, `side` determines which element
  is used. When < 0, the element before the position is used,
  otherwise the element after.
  */
  coordsAtPos(e, n = 1) {
    return Zd(this, e, n);
  }
  /**
  Find the DOM position that corresponds to the given document
  position. When `side` is negative, find the position as close as
  possible to the content before the position. When positive,
  prefer positions close to the content after the position. When
  zero, prefer as shallow a position as possible.
  
  Note that you should **not** mutate the editor's internal DOM,
  only inspect it (and even that is usually not necessary).
  */
  domAtPos(e, n = 0) {
    return this.docView.domFromPos(e, n);
  }
  /**
  Find the DOM node that represents the document node after the
  given position. May return `null` when the position doesn't point
  in front of a node or if the node is inside an opaque node view.
  
  This is intended to be able to call things like
  `getBoundingClientRect` on that DOM node. Do **not** mutate the
  editor DOM directly, or add styling this way, since that will be
  immediately overriden by the editor as it redraws the node.
  */
  nodeDOM(e) {
    let n = this.docView.descAt(e);
    return n ? n.nodeDOM : null;
  }
  /**
  Find the document position that corresponds to a given DOM
  position. (Whenever possible, it is preferable to inspect the
  document structure directly, rather than poking around in the
  DOM, but sometimes—for example when interpreting an event
  target—you don't have a choice.)
  
  The `bias` parameter can be used to influence which side of a DOM
  node to use when the position is inside a leaf node.
  */
  posAtDOM(e, n, r = -1) {
    let i = this.docView.posFromDOM(e, n, r);
    if (i == null)
      throw new RangeError("DOM position not inside the editor");
    return i;
  }
  /**
  Find out whether the selection is at the end of a textblock when
  moving in a given direction. When, for example, given `"left"`,
  it will return true if moving left from the current cursor
  position would leave that position's parent textblock. Will apply
  to the view's current state by default, but it is possible to
  pass a different state.
  */
  endOfTextblock(e, n) {
    return Zw(this, n || this.state, e);
  }
  /**
  Run the editor's paste logic with the given HTML string. The
  `event`, if given, will be passed to the
  [`handlePaste`](https://prosemirror.net/docs/ref/#view.EditorProps.handlePaste) hook.
  */
  pasteHTML(e, n) {
    return Si(this, "", e, !1, n || new ClipboardEvent("paste"));
  }
  /**
  Run the editor's paste logic with the given plain-text input.
  */
  pasteText(e, n) {
    return Si(this, e, null, !0, n || new ClipboardEvent("paste"));
  }
  /**
  Serialize the given slice as it would be if it was copied from
  this editor. Returns a DOM element that contains a
  representation of the slice as its children, a textual
  representation, and the transformed slice (which can be
  different from the given input due to hooks like
  [`transformCopied`](https://prosemirror.net/docs/ref/#view.EditorProps.transformCopied)).
  */
  serializeForClipboard(e) {
    return Da(this, e);
  }
  /**
  Removes the editor from the DOM and destroys all [node
  views](https://prosemirror.net/docs/ref/#view.NodeView).
  */
  destroy() {
    this.docView && (Ax(this), this.destroyPluginViews(), this.mounted ? (this.docView.update(this.state.doc, [], Xs(this), this), this.dom.textContent = "") : this.dom.parentNode && this.dom.parentNode.removeChild(this.dom), this.docView.destroy(), this.docView = null, Ow());
  }
  /**
  This is true when the view has been
  [destroyed](https://prosemirror.net/docs/ref/#view.EditorView.destroy) (and thus should not be
  used anymore).
  */
  get isDestroyed() {
    return this.docView == null;
  }
  /**
  Used for testing.
  */
  dispatchEvent(e) {
    return Ox(this, e);
  }
  /**
  Dispatch a transaction. Will call
  [`dispatchTransaction`](https://prosemirror.net/docs/ref/#view.DirectEditorProps.dispatchTransaction)
  when given, and otherwise defaults to applying the transaction to
  the current state and calling
  [`updateState`](https://prosemirror.net/docs/ref/#view.EditorView.updateState) with the result.
  This method is bound to the view instance, so that it can be
  easily passed around.
  */
  dispatch(e) {
    let n = this._props.dispatchTransaction;
    n ? n.call(this, e) : this.updateState(this.state.apply(e));
  }
  /**
  @internal
  */
  domSelectionRange() {
    let e = this.domSelection();
    return e ? De && this.root.nodeType === 11 && zw(this.dom.ownerDocument) == this.dom && Zx(this, e) || e : { focusNode: null, focusOffset: 0, anchorNode: null, anchorOffset: 0 };
  }
  /**
  @internal
  */
  domSelection() {
    return this.root.getSelection();
  }
}
function Oh(t) {
  let e = /* @__PURE__ */ Object.create(null);
  return e.class = "ProseMirror", e.contenteditable = String(t.editable), t.someProp("attributes", (n) => {
    if (typeof n == "function" && (n = n(t.state)), n)
      for (let r in n)
        r == "class" ? e.class += " " + n[r] : r == "style" ? e.style = (e.style ? e.style + ";" : "") + n[r] : !e[r] && r != "contenteditable" && r != "nodeName" && (e[r] = String(n[r]));
  }), e.translate || (e.translate = "no"), [Me.node(0, t.state.doc.content.size, e)];
}
function Dh(t) {
  if (t.markCursor) {
    let e = document.createElement("img");
    e.className = "ProseMirror-separator", e.setAttribute("mark-placeholder", "true"), e.setAttribute("alt", ""), t.cursorWrapper = { dom: e, deco: Me.widget(t.state.selection.from, e, { raw: !0, marks: t.markCursor }) };
  } else
    t.cursorWrapper = null;
}
function Rh(t) {
  return !t.someProp("editable", (e) => e(t.state) === !1);
}
function cC(t, e) {
  let n = Math.min(t.$anchor.sharedDepth(t.head), e.$anchor.sharedDepth(e.head));
  return t.$anchor.start(n) != e.$anchor.start(n);
}
function vh(t) {
  let e = /* @__PURE__ */ Object.create(null);
  function n(r) {
    for (let i in r)
      Object.prototype.hasOwnProperty.call(e, i) || (e[i] = r[i]);
  }
  return t.someProp("nodeViews", n), t.someProp("markViews", n), e;
}
function uC(t, e) {
  let n = 0, r = 0;
  for (let i in t) {
    if (t[i] != e[i])
      return !0;
    n++;
  }
  for (let i in e)
    r++;
  return n != r;
}
function Ph(t) {
  if (t.spec.state || t.spec.filterTransaction || t.spec.appendTransaction)
    throw new RangeError("Plugins passed directly to the view must not have a state component");
}
var Tp = (t) => {
  throw TypeError(t);
}, Ip = (t, e, n) => e.has(t) || Tp("Cannot " + n), L = (t, e, n) => (Ip(t, e, "read from private field"), n ? n.call(t) : e.get(t)), be = (t, e, n) => e.has(t) ? Tp("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, n), ae = (t, e, n, r) => (Ip(t, e, "write to private field"), e.set(t, n), n), or, In, sr, Xe, Qt, ni, To, Io, qe, Xt, lr, Ao, ar, ri, Eo, An, ii;
function kn(t, e) {
  return t.meta = {
    package: "@milkdown/core",
    group: "System",
    ...e
  }, t;
}
const Ap = {
  strong: (t, e, n, r) => {
    const i = t.marker || n.options.strong || "*", o = n.enter("strong"), s = n.createTracker(r);
    let l = s.move(i + i);
    return l += s.move(
      n.containerPhrasing(t, {
        before: l,
        after: i,
        ...s.current()
      })
    ), l += s.move(i + i), o(), l;
  },
  emphasis: (t, e, n, r) => {
    const i = t.marker || n.options.emphasis || "*", o = n.enter("emphasis"), s = n.createTracker(r);
    let l = s.move(i);
    return l += s.move(
      n.containerPhrasing(t, {
        before: l,
        after: i,
        ...s.current()
      })
    ), l += s.move(i), o(), l;
  }
}, gr = Y({}, "editorView"), oi = Y({}, "editorState"), el = Y([], "initTimer"), zh = Y({}, "editor"), Ni = Y([], "inputRules"), It = Y([], "prosePlugins"), Ti = Y(
  [],
  "remarkPlugins"
), Ii = Y([], "nodeView"), Ai = Y([], "markView"), Fn = Y(
  vl().use(Nl).use(El),
  "remark"
), fi = Y(
  {
    handlers: Ap
  },
  "remarkStringifyOptions"
), Oo = pn("ConfigReady");
function hC(t) {
  const e = (n) => (n.record(Oo), async () => (await t(n), n.done(Oo), () => {
    n.clearTimer(Oo);
  }));
  return kn(e, {
    displayName: "Config"
  }), e;
}
const Bn = pn("InitReady");
function fC(t) {
  const e = (n) => (n.inject(zh, t).inject(It, []).inject(Ti, []).inject(Ni, []).inject(Ii, []).inject(Ai, []).inject(fi, {
    handlers: Ap
  }).inject(Fn, vl().use(Nl).use(El)).inject(el, [Oo]).record(Bn), async () => {
    await n.waitTimers(el);
    const r = n.get(fi);
    return n.set(
      Fn,
      vl().use(Nl).use(El, r)
    ), n.done(Bn), () => {
      n.remove(zh).remove(It).remove(Ti).remove(Ni).remove(Ii).remove(Ai).remove(fi).remove(Fn).remove(el).clearTimer(Bn);
    };
  });
  return kn(e, {
    displayName: "Init"
  }), e;
}
const pt = pn("SchemaReady"), tl = Y([], "schemaTimer"), Ht = Y({}, "schema"), yr = Y([], "nodes"), kr = Y([], "marks");
function Lh(t) {
  var e;
  return {
    ...t,
    parseDOM: (e = t.parseDOM) == null ? void 0 : e.map((n) => ({ priority: t.priority, ...n }))
  };
}
const Ep = (t) => (t.inject(Ht, {}).inject(yr, []).inject(kr, []).inject(tl, [Bn]).record(pt), async () => {
  await t.waitTimers(tl);
  const e = t.get(Fn), r = t.get(Ti).reduce(
    (l, a) => l.use(a.plugin, a.options),
    e
  );
  t.set(Fn, r);
  const i = Object.fromEntries(
    t.get(yr).map(([l, a]) => [l, Lh(a)])
  ), o = Object.fromEntries(
    t.get(kr).map(([l, a]) => [l, Lh(a)])
  ), s = new hy({ nodes: i, marks: o });
  return t.set(Ht, s), t.done(pt), () => {
    t.remove(Ht).remove(yr).remove(kr).remove(tl).clearTimer(pt);
  };
});
kn(Ep, {
  displayName: "Schema"
});
class Op {
  constructor() {
    be(this, or), be(this, In), ae(this, or, new df()), ae(this, In, null), this.setCtx = (e) => {
      ae(this, In, e);
    };
  }
  get ctx() {
    return L(this, In);
  }
  /// Register a command into the manager.
  create(e, n) {
    const r = e.create(L(this, or).sliceMap);
    return r.set(n), r;
  }
  get(e) {
    return L(this, or).get(e).get();
  }
  remove(e) {
    return L(this, or).remove(e);
  }
  call(e, n) {
    if (L(this, In) == null) throw Bg();
    const i = this.get(e)(n), o = L(this, In).get(gr);
    return i(o.state, o.dispatch, o);
  }
}
or = /* @__PURE__ */ new WeakMap();
In = /* @__PURE__ */ new WeakMap();
function dC(t = "cmdKey") {
  return Y(() => () => !1, t);
}
const X = Y(new Op(), "commands"), nl = Y([pt], "commandsTimer"), di = pn("CommandsReady"), Dp = (t) => {
  const e = new Op();
  return e.setCtx(t), t.inject(X, e).inject(nl, [pt]).record(di), async () => (await t.waitTimers(nl), t.done(di), () => {
    t.remove(X).remove(nl).clearTimer(di);
  });
};
kn(Dp, {
  displayName: "Commands"
});
const Do = pn("ParserReady"), Rp = () => {
  throw ha();
}, pi = Y(Rp, "parser"), rl = Y([], "parserTimer"), vp = (t) => (t.inject(pi, Rp).inject(rl, [pt]).record(Do), async () => {
  await t.waitTimers(rl);
  const e = t.get(Fn), n = t.get(Ht);
  return t.set(pi, s0.create(n, e)), t.done(Do), () => {
    t.remove(pi).remove(rl).clearTimer(Do);
  };
});
kn(vp, {
  displayName: "Parser"
});
const mi = pn("SerializerReady"), il = Y(
  [],
  "serializerTimer"
), Pp = () => {
  throw ha();
}, br = Y(
  Pp,
  "serializer"
), zp = (t) => (t.inject(br, Pp).inject(il, [pt]).record(mi), async () => {
  await t.waitTimers(il);
  const e = t.get(Fn), n = t.get(Ht);
  return t.set(br, a0.create(n, e)), t.done(mi), () => {
    t.remove(br).remove(il).clearTimer(mi);
  };
});
kn(zp, {
  displayName: "Serializer"
});
const Ro = Y("", "defaultValue"), ol = Y(
  (t) => t,
  "stateOptions"
), sl = Y(
  [],
  "editorStateTimer"
), vo = pn("EditorStateReady");
function pC(t, e, n) {
  if (typeof t == "string") return e(t);
  if (t.type === "html")
    return Wn.fromSchema(n).parse(t.dom);
  if (t.type === "json")
    return dt.fromJSON(n, t.value);
  throw Rg(t);
}
const mC = new ge("MILKDOWN_STATE_TRACKER");
function gC(t) {
  const e = gs(
    F0,
    Na,
    Bd,
    _d
  );
  return t.Backspace = e, t;
}
const Lp = (t) => (t.inject(Ro, "").inject(oi, {}).inject(ol, (e) => e).inject(sl, [Do, mi, di]).record(vo), async () => {
  await t.waitTimers(sl);
  const e = t.get(Ht), n = t.get(pi), r = t.get(Ni), i = t.get(ol), o = t.get(It), s = t.get(Ro), l = pC(s, n, e), a = [
    ...o,
    new Ne({
      key: mC,
      state: {
        init: () => {
        },
        apply: (h, f, d, p) => {
          t.set(oi, p);
        }
      }
    }),
    U0({ rules: r }),
    qd(gC(Sw))
  ];
  t.set(It, a);
  const c = i({
    schema: e,
    doc: l,
    plugins: a
  }), u = fr.create(c);
  return t.set(oi, u), t.done(vo), () => {
    t.remove(Ro).remove(oi).remove(ol).remove(sl).clearTimer(vo);
  };
});
kn(Lp, {
  displayName: "EditorState"
});
const Po = pn("EditorViewReady"), ll = Y(
  [],
  "editorViewTimer"
), gi = Y(
  {},
  "editorViewOptions"
), zo = Y(null, "root"), Xl = Y(null, "rootDOM"), Zl = Y(
  {},
  "rootAttrs"
);
function yC(t, e) {
  const n = document.createElement("div");
  n.className = "milkdown", t.appendChild(n), e.set(Xl, n);
  const r = e.get(Zl);
  return Object.entries(r).forEach(
    ([i, o]) => n.setAttribute(i, o)
  ), n;
}
function kC(t) {
  t.classList.add("editor"), t.setAttribute("role", "textbox");
}
const bC = new ge("MILKDOWN_VIEW_CLEAR"), Fp = (t) => (t.inject(zo, document.body).inject(gr, {}).inject(gi, {}).inject(Xl, null).inject(Zl, {}).inject(ll, [vo]).record(Po), async () => {
  await t.wait(Bn);
  const e = t.get(zo) || document.body, n = typeof e == "string" ? document.querySelector(e) : e;
  t.update(It, (a) => [
    new Ne({
      key: bC,
      view: (c) => {
        const u = n ? yC(n, t) : void 0;
        return (() => {
          if (u && n) {
            const f = c.dom;
            n.replaceChild(u, f), u.appendChild(f);
          }
        })(), {
          destroy: () => {
            u != null && u.parentNode && (u == null || u.parentNode.replaceChild(c.dom, u)), u == null || u.remove();
          }
        };
      }
    }),
    ...a
  ]), await t.waitTimers(ll);
  const r = t.get(oi), i = t.get(gi), o = Object.fromEntries(t.get(Ii)), s = Object.fromEntries(t.get(Ai)), l = new aC(n, {
    state: r,
    nodeViews: o,
    markViews: s,
    ...i
  });
  return kC(l.dom), t.set(gr, l), t.done(Po), () => {
    l == null || l.destroy(), t.remove(zo).remove(gr).remove(gi).remove(Xl).remove(Zl).remove(ll).clearTimer(Po);
  };
});
kn(Fp, {
  displayName: "EditorView"
});
var Bp = /* @__PURE__ */ ((t) => (t.Idle = "Idle", t.OnCreate = "OnCreate", t.Created = "Created", t.OnDestroy = "OnDestroy", t.Destroyed = "Destroyed", t))(Bp || {});
const wC = class _p {
  constructor() {
    be(this, sr), be(this, Xe), be(this, Qt), be(this, ni), be(this, To), be(this, Io), be(this, qe), be(this, Xt), be(this, lr), be(this, Ao), be(this, ar), be(this, ri), be(this, Eo), be(this, An), be(this, ii), ae(this, sr, !1), ae(this, Xe, "Idle"), ae(this, Qt, []), ae(this, ni, () => {
    }), ae(this, To, new df()), ae(this, Io, new Kg()), ae(this, qe, /* @__PURE__ */ new Map()), ae(this, Xt, /* @__PURE__ */ new Map()), ae(this, lr, new qg(L(this, To), L(this, Io))), ae(this, Ao, () => {
      const e = hC(async (r) => {
        await Promise.all(L(this, Qt).map((i) => i(r)));
      }), n = [
        Ep,
        vp,
        zp,
        Dp,
        Lp,
        Fp,
        fC(this),
        e
      ];
      L(this, ar).call(this, n, L(this, Xt));
    }), ae(this, ar, (e, n) => {
      e.forEach((r) => {
        const i = L(this, lr).produce(
          L(this, sr) ? r.meta : void 0
        ), o = r(i);
        n.set(r, { ctx: i, handler: o, cleanup: void 0 });
      });
    }), ae(this, ri, (e, n = !1) => Promise.all(
      [e].flat().map((r) => {
        const i = L(this, qe).get(r), o = i == null ? void 0 : i.cleanup;
        return n ? L(this, qe).delete(r) : L(this, qe).set(r, {
          ctx: void 0,
          handler: void 0,
          cleanup: void 0
        }), typeof o == "function" ? o() : o;
      })
    )), ae(this, Eo, async () => {
      await Promise.all(
        [...L(this, Xt).entries()].map(([e, { cleanup: n }]) => typeof n == "function" ? n() : n)
      ), L(this, Xt).clear();
    }), ae(this, An, (e) => {
      ae(this, Xe, e), L(this, ni).call(this, e);
    }), ae(this, ii, (e) => [...e.entries()].map(async ([n, r]) => {
      const { ctx: i, handler: o } = r;
      if (!o) return;
      const s = await o();
      e.set(n, { ctx: i, handler: o, cleanup: s });
    })), this.enableInspector = (e = !0) => (ae(this, sr, e), this), this.onStatusChange = (e) => (ae(this, ni, e), this), this.config = (e) => (L(this, Qt).push(e), this), this.removeConfig = (e) => (ae(this, Qt, L(this, Qt).filter((n) => n !== e)), this), this.use = (e) => {
      const n = [e].flat();
      return n.flat().forEach((r) => {
        L(this, qe).set(r, {
          ctx: void 0,
          handler: void 0,
          cleanup: void 0
        });
      }), L(this, Xe) === "Created" && L(this, ar).call(this, n, L(this, qe)), this;
    }, this.remove = async (e) => L(this, Xe) === "OnCreate" ? (console.warn(
      "[Milkdown]: You are trying to remove plugins when the editor is creating, this is not recommended, please check your code."
    ), new Promise((n) => {
      setTimeout(() => {
        n(this.remove(e));
      }, 50);
    })) : (await L(this, ri).call(this, [e].flat(), !0), this), this.create = async () => L(this, Xe) === "OnCreate" ? this : (L(this, Xe) === "Created" && await this.destroy(), L(this, An).call(this, "OnCreate"), L(this, Ao).call(this), L(this, ar).call(this, [...L(this, qe).keys()], L(this, qe)), await Promise.all(
      [
        L(this, ii).call(this, L(this, Xt)),
        L(this, ii).call(this, L(this, qe))
      ].flat()
    ), L(this, An).call(this, "Created"), this), this.destroy = async (e = !1) => L(this, Xe) === "Destroyed" || L(this, Xe) === "OnDestroy" ? this : L(this, Xe) === "OnCreate" ? new Promise((n) => {
      setTimeout(() => {
        n(this.destroy(e));
      }, 50);
    }) : (e && ae(this, Qt, []), L(this, An).call(this, "OnDestroy"), await L(this, ri).call(this, [...L(this, qe).keys()], e), await L(this, Eo).call(this), L(this, An).call(this, "Destroyed"), this), this.action = (e) => e(L(this, lr)), this.inspect = () => L(this, sr) ? [...L(this, Xt).values(), ...L(this, qe).values()].map(({ ctx: e }) => {
      var n;
      return (n = e == null ? void 0 : e.inspector) == null ? void 0 : n.read();
    }).filter((e) => !!e) : (console.warn(
      "[Milkdown]: You are trying to collect inspection when inspector is disabled, please enable inspector by `editor.enableInspector()` first."
    ), []);
  }
  /// Create a new editor instance.
  static make() {
    return new _p();
  }
  /// Get the ctx of the editor.
  get ctx() {
    return L(this, lr);
  }
  /// Get the status of the editor.
  get status() {
    return L(this, Xe);
  }
};
sr = /* @__PURE__ */ new WeakMap();
Xe = /* @__PURE__ */ new WeakMap();
Qt = /* @__PURE__ */ new WeakMap();
ni = /* @__PURE__ */ new WeakMap();
To = /* @__PURE__ */ new WeakMap();
Io = /* @__PURE__ */ new WeakMap();
qe = /* @__PURE__ */ new WeakMap();
Xt = /* @__PURE__ */ new WeakMap();
lr = /* @__PURE__ */ new WeakMap();
Ao = /* @__PURE__ */ new WeakMap();
ar = /* @__PURE__ */ new WeakMap();
ri = /* @__PURE__ */ new WeakMap();
Eo = /* @__PURE__ */ new WeakMap();
An = /* @__PURE__ */ new WeakMap();
ii = /* @__PURE__ */ new WeakMap();
let xC = wC, CC = (t) => crypto.getRandomValues(new Uint8Array(t)), SC = (t, e, n) => {
  let r = (2 << Math.log2(t.length - 1)) - 1, i = -~(1.6 * r * e / t.length);
  return (o = e) => {
    let s = "";
    for (; ; ) {
      let l = n(i), a = i | 0;
      for (; a--; )
        if (s += t[l[a] & r] || "", s.length >= o) return s;
    }
  };
}, MC = (t, e = 21) => SC(t, e | 0, CC);
MC("abcedfghicklmn", 10);
function j(t, e) {
  const n = dC(t), r = (i) => async () => {
    r.key = n, await i.wait(di);
    const o = e(i);
    return i.get(X).create(n, o), r.run = (s) => i.get(X).call(t, s), () => {
      i.get(X).remove(n);
    };
  };
  return r;
}
function Ve(t) {
  const e = (n) => async () => {
    await n.wait(pt);
    const r = t(n);
    return n.update(Ni, (i) => [...i, r]), e.inputRule = r, () => {
      n.update(Ni, (i) => i.filter((o) => o !== r));
    };
  };
  return e;
}
function NC(t, e) {
  const n = (r) => async () => {
    const i = e(r);
    return r.update(kr, (o) => [
      ...o.filter((s) => s[0] !== t),
      [t, i]
    ]), n.id = t, n.schema = i, () => {
      r.update(kr, (o) => o.filter(([s]) => s !== t));
    };
  };
  return n.type = (r) => {
    const i = r.get(Ht).marks[t];
    if (!i) throw $g(t);
    return i;
  }, n;
}
function Fa(t, e) {
  const n = (r) => async () => {
    const i = e(r);
    return r.update(yr, (o) => [
      ...o.filter((s) => s[0] !== t),
      [t, i]
    ]), n.id = t, n.schema = i, () => {
      r.update(yr, (o) => o.filter(([s]) => s !== t));
    };
  };
  return n.type = (r) => {
    const i = r.get(Ht).nodes[t];
    if (!i) throw _g(t);
    return i;
  }, n;
}
function ot(t) {
  let e;
  const n = (r) => async () => (await r.wait(pt), e = t(r), r.update(It, (i) => [...i, e]), () => {
    r.update(It, (i) => i.filter((o) => o !== e));
  });
  return n.plugin = () => e, n.key = () => e.spec.key, n;
}
function $p(t) {
  const e = (n) => async () => {
    await n.wait(pt);
    const r = t(n), i = qd(r);
    return n.update(It, (o) => [...o, i]), e.keymap = r, () => {
      n.update(It, (o) => o.filter((s) => s !== i));
    };
  };
  return e;
}
function KT(t, e) {
  const n = (r) => async () => {
    await r.wait(pt);
    const i = e(r);
    return t.type(r) instanceof qo ? r.update(Ii, (o) => [
      ...o,
      [t.id, i]
    ]) : r.update(Ai, (o) => [
      ...o,
      [t.id, i]
    ]), n.view = i, n.type = t, () => {
      t.type(r) instanceof qo ? r.update(Ii, (o) => o.filter((s) => s[0] !== t.id)) : r.update(Ai, (o) => o.filter((s) => s[0] !== t.id));
    };
  };
  return n;
}
function gt(t, e) {
  const n = Y(t, e), r = (i) => (i.inject(n), () => () => {
    i.remove(n);
  });
  return r.key = n, r;
}
function ye(t, e) {
  const n = gt(e, t), r = Fa(t, (o) => o.get(n.key)(o)), i = [n, r];
  return i.id = r.id, i.node = r, i.type = (o) => r.type(o), i.schema = r.schema, i.ctx = n, i.key = n.key, i.extendSchema = (o) => (s) => () => {
    const l = s.get(n.key), c = o(l)(s);
    s.update(yr, (u) => [
      ...u.filter((h) => h[0] !== t),
      [t, c]
    ]), i.schema = c;
  }, i;
}
function Li(t, e) {
  const n = gt(e, t), r = NC(t, (o) => o.get(n.key)(o)), i = [n, r];
  return i.id = r.id, i.mark = r, i.type = r.type, i.schema = r.schema, i.ctx = n, i.key = n.key, i.extendSchema = (o) => (s) => () => {
    const l = s.get(n.key), c = o(l)(s);
    s.update(kr, (u) => [
      ...u.filter((h) => h[0] !== t),
      [t, c]
    ]), i.schema = c;
  }, i;
}
function We(t, e) {
  const n = Object.fromEntries(
    Object.entries(e).map(([s, { shortcuts: l }]) => [s, l])
  ), r = gt(n, `${t}Keymap`), i = $p((s) => {
    const l = s.get(r.key), a = Object.entries(e).flatMap(
      ([c, { command: u }]) => [l[c]].flat().map((f) => [f, u(s)])
    );
    return Object.fromEntries(a);
  }), o = [r, i];
  return o.ctx = r, o.shortcuts = i, o.key = r.key, o.keymap = i.keymap, o;
}
const yt = (t, e = () => ({})) => gt(e, `${t}Attr`), Fi = (t, e = () => ({})) => gt(e, `${t}Attr`);
function Un(t, e, n) {
  const r = gt({}, t), i = (s) => async () => {
    await s.wait(Bn);
    const a = {
      plugin: e(s),
      options: s.get(r.key)
    };
    return s.update(Ti, (c) => [...c, a]), () => {
      s.update(Ti, (c) => c.filter((u) => u !== a));
    };
  }, o = [r, i];
  return o.id = t, o.plugin = i, o.options = r, o;
}
function TC() {
  return (t) => {
    const e = t.get(gr);
    return t.get(br)(e.state.doc);
  };
}
function IC(t, e) {
  return function(n, r) {
    let { $from: i, $to: o, node: s } = n.selection;
    if (s && s.isBlock || i.depth < 2 || !i.sameParent(o))
      return !1;
    let l = i.node(-1);
    if (l.type != t)
      return !1;
    if (i.parent.content.size == 0 && i.node(-1).childCount == i.indexAfter(-1)) {
      if (i.depth == 3 || i.node(-3).type != t || i.index(-2) != i.node(-2).childCount - 1)
        return !1;
      if (r) {
        let h = T.empty, f = i.index(-1) ? 1 : i.index(-2) ? 2 : 3;
        for (let g = i.depth - f; g >= i.depth - 3; g--)
          h = T.from(i.node(g).copy(h));
        let d = i.indexAfter(-1) < i.node(-2).childCount ? 1 : i.indexAfter(-2) < i.node(-3).childCount ? 2 : 3;
        h = h.append(T.from(t.createAndFill()));
        let p = i.before(i.depth - (f - 1)), m = n.tr.replace(p, i.after(-d), new E(h, 4 - f, 0)), y = -1;
        m.doc.nodesBetween(p, m.doc.content.size, (g, N) => {
          if (y > -1)
            return !1;
          g.isTextblock && g.content.size == 0 && (y = N + 1);
        }), y > -1 && m.setSelection($.near(m.doc.resolve(y))), r(m.scrollIntoView());
      }
      return !0;
    }
    let a = o.pos == i.end() ? l.contentMatchAt(0).defaultType : null, c = n.tr.delete(i.pos, o.pos), u = a ? [null, { type: a }] : void 0;
    return ci(c.doc, i.pos, 2, u) ? (r && r(c.split(i.pos, 2, u).scrollIntoView()), !0) : !1;
  };
}
function Vp(t) {
  return function(e, n) {
    let { $from: r, $to: i } = e.selection, o = r.blockRange(i, (s) => s.childCount > 0 && s.firstChild.type == t);
    return o ? n ? r.node(o.depth - 1).type == t ? AC(e, n, t, o) : EC(e, n, o) : !0 : !1;
  };
}
function AC(t, e, n, r) {
  let i = t.tr, o = r.end, s = r.$to.end(r.depth);
  o < s && (i.step(new Ae(o - 1, s, o, s, new E(T.from(n.create(null, r.parent.copy())), 1, 0), 1, !0)), r = new Cf(i.doc.resolve(r.$from.pos), i.doc.resolve(s), r.depth));
  const l = us(r);
  if (l == null)
    return !1;
  i.lift(r, l);
  let a = i.doc.resolve(i.mapping.map(o, -1) - 1);
  return hs(i.doc, a.pos) && a.nodeBefore.type == a.nodeAfter.type && i.join(a.pos), e(i.scrollIntoView()), !0;
}
function EC(t, e, n) {
  let r = t.tr, i = n.parent;
  for (let d = n.end, p = n.endIndex - 1, m = n.startIndex; p > m; p--)
    d -= i.child(p).nodeSize, r.delete(d - 1, d + 1);
  let o = r.doc.resolve(n.start), s = o.nodeAfter;
  if (r.mapping.map(n.end) != n.start + o.nodeAfter.nodeSize)
    return !1;
  let l = n.startIndex == 0, a = n.endIndex == i.childCount, c = o.node(-1), u = o.index(-1);
  if (!c.canReplace(u + (l ? 0 : 1), u + 1, s.content.append(a ? T.empty : T.from(i))))
    return !1;
  let h = o.pos, f = h + s.nodeSize;
  return r.step(new Ae(h - (l ? 1 : 0), f + (a ? 1 : 0), h + 1, f - 1, new E((l ? T.empty : T.from(i.copy(T.empty))).append(a ? T.empty : T.from(i.copy(T.empty))), l ? 0 : 1, a ? 0 : 1), l ? 0 : 1)), e(r.scrollIntoView()), !0;
}
function OC(t) {
  return function(e, n) {
    let { $from: r, $to: i } = e.selection, o = r.blockRange(i, (c) => c.childCount > 0 && c.firstChild.type == t);
    if (!o)
      return !1;
    let s = o.startIndex;
    if (s == 0)
      return !1;
    let l = o.parent, a = l.child(s - 1);
    if (a.type != t)
      return !1;
    if (n) {
      let c = a.lastChild && a.lastChild.type == l.type, u = T.from(c ? t.create() : null), h = new E(T.from(t.create(null, T.from(l.type.create(null, u)))), c ? 3 : 1, 0), f = o.start, d = o.end;
      n(e.tr.step(new Ae(f - (c ? 3 : 1), d, f, d, h, 1, !0)).scrollIntoView());
    }
    return !0;
  };
}
function DC(t) {
  const e = /* @__PURE__ */ new Map();
  if (!t || !t.type)
    throw new Error("mdast-util-definitions expected node");
  return Jn(t, "definition", function(r) {
    const i = Fh(r.identifier);
    i && !e.get(i) && e.set(i, r);
  }), n;
  function n(r) {
    const i = Fh(r);
    return e.get(i);
  }
}
function Fh(t) {
  return String(t || "").toUpperCase();
}
function RC() {
  return function(t) {
    const e = DC(t);
    Jn(t, function(n, r, i) {
      if (n.type === "definition" && i !== void 0 && typeof r == "number")
        return i.children.splice(r, 1), [Al, r];
      if (n.type === "imageReference" || n.type === "linkReference") {
        const o = e(n.identifier);
        if (o && i && typeof r == "number")
          return i.children[r] = n.type === "imageReference" ? { type: "image", url: o.url, title: o.title, alt: n.alt } : {
            type: "link",
            url: o.url,
            title: o.title,
            children: n.children
          }, [Al, r];
      }
    });
  };
}
function Wp(t, e) {
  var n;
  if (!(e.childCount >= 1 && ((n = e.lastChild) == null ? void 0 : n.type.name) === "hardbreak")) {
    t.next(e.content);
    return;
  }
  const i = [];
  e.content.forEach((o, s, l) => {
    l !== e.childCount - 1 && i.push(o);
  }), t.next(T.fromArray(i));
}
function S(t, e) {
  return Object.assign(t, {
    meta: {
      package: "@milkdown/preset-commonmark",
      ...e
    }
  }), t;
}
const Ba = Fi("emphasis");
S(Ba, {
  displayName: "Attr<emphasis>",
  group: "Emphasis"
});
const Ar = Li("emphasis", (t) => ({
  attrs: {
    marker: {
      default: t.get(fi).emphasis || "*"
    }
  },
  parseDOM: [
    { tag: "i" },
    { tag: "em" },
    { style: "font-style", getAttrs: (e) => e === "italic" }
  ],
  toDOM: (e) => ["em", t.get(Ba.key)(e)],
  parseMarkdown: {
    match: (e) => e.type === "emphasis",
    runner: (e, n, r) => {
      e.openMark(r, { marker: n.marker }), e.next(n.children), e.closeMark(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "emphasis",
    runner: (e, n) => {
      e.withMark(n, "emphasis", void 0, {
        marker: n.attrs.marker
      });
    }
  }
}));
S(Ar.mark, {
  displayName: "MarkSchema<emphasis>",
  group: "Emphasis"
});
S(Ar.ctx, {
  displayName: "MarkSchemaCtx<emphasis>",
  group: "Emphasis"
});
const _a = j("ToggleEmphasis", (t) => () => ms(Ar.type(t)));
S(_a, {
  displayName: "Command<toggleEmphasisCommand>",
  group: "Emphasis"
});
const Hp = Ve((t) => Ri(/(?:^|[^*])\*([^*]+)\*$/, Ar.type(t), {
  getAttr: () => ({
    marker: "*"
  }),
  updateCaptured: ({ fullMatch: e, start: n }) => e.startsWith("*") ? {} : { fullMatch: e.slice(1), start: n + 1 }
}));
S(Hp, {
  displayName: "InputRule<emphasis>|Star",
  group: "Emphasis"
});
const jp = Ve((t) => Ri(/(?:^|[^_])_([^_]+)_$/, Ar.type(t), {
  getAttr: () => ({
    marker: "_"
  }),
  updateCaptured: ({ fullMatch: e, start: n }) => e.startsWith("_") ? {} : { fullMatch: e.slice(1), start: n + 1 }
}));
S(jp, {
  displayName: "InputRule<emphasis>|Underscore",
  group: "Emphasis"
});
const $a = We("emphasisKeymap", {
  ToggleEmphasis: {
    shortcuts: "Mod-i",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(_a.key);
    }
  }
});
S($a.ctx, {
  displayName: "KeymapCtx<emphasis>",
  group: "Emphasis"
});
S($a.shortcuts, {
  displayName: "Keymap<emphasis>",
  group: "Emphasis"
});
const Va = Fi("strong");
S(Va, {
  displayName: "Attr<strong>",
  group: "Strong"
});
const Bi = Li("strong", (t) => ({
  attrs: {
    marker: {
      default: t.get(fi).strong || "*"
    }
  },
  parseDOM: [
    { tag: "b" },
    { tag: "strong" },
    { style: "font-style", getAttrs: (e) => e === "bold" }
  ],
  toDOM: (e) => ["strong", t.get(Va.key)(e)],
  parseMarkdown: {
    match: (e) => e.type === "strong",
    runner: (e, n, r) => {
      e.openMark(r, { marker: n.marker }), e.next(n.children), e.closeMark(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "strong",
    runner: (e, n) => {
      e.withMark(n, "strong", void 0, {
        marker: n.attrs.marker
      });
    }
  }
}));
S(Bi.mark, {
  displayName: "MarkSchema<strong>",
  group: "Strong"
});
S(Bi.ctx, {
  displayName: "MarkSchemaCtx<strong>",
  group: "Strong"
});
const Wa = j("ToggleStrong", (t) => () => ms(Bi.type(t)));
S(Wa, {
  displayName: "Command<toggleStrongCommand>",
  group: "Strong"
});
const qp = Ve((t) => Ri(/(?:\*\*|__)([^*_]+)(?:\*\*|__)$/, Bi.type(t), {
  getAttr: (e) => ({
    marker: e[0].startsWith("*") ? "*" : "_"
  })
}));
S(qp, {
  displayName: "InputRule<strong>",
  group: "Strong"
});
const Ha = We("strongKeymap", {
  ToggleBold: {
    shortcuts: ["Mod-b"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Wa.key);
    }
  }
});
S(Ha.ctx, {
  displayName: "KeymapCtx<strong>",
  group: "Strong"
});
S(Ha.shortcuts, {
  displayName: "Keymap<strong>",
  group: "Strong"
});
const ja = Fi("inlineCode");
S(ja, {
  displayName: "Attr<inlineCode>",
  group: "InlineCode"
});
const ln = Li("inlineCode", (t) => ({
  priority: 100,
  code: !0,
  inclusive: !1,
  parseDOM: [{ tag: "code" }],
  toDOM: (e) => ["code", t.get(ja.key)(e)],
  parseMarkdown: {
    match: (e) => e.type === "inlineCode",
    runner: (e, n, r) => {
      e.openMark(r), e.addText(n.value), e.closeMark(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "inlineCode",
    runner: (e, n, r) => {
      e.withMark(n, "inlineCode", r.text || "");
    }
  }
}));
S(ln.mark, {
  displayName: "MarkSchema<inlineCode>",
  group: "InlineCode"
});
S(ln.ctx, {
  displayName: "MarkSchemaCtx<inlineCode>",
  group: "InlineCode"
});
const qa = j(
  "ToggleInlineCode",
  (t) => () => (e, n) => {
    const { selection: r, tr: i } = e;
    if (r.empty) return !1;
    const { from: o, to: s } = r;
    return e.doc.rangeHasMark(o, s, ln.type(t)) ? (n == null || n(i.removeMark(o, s, ln.type(t))), !0) : (Object.keys(e.schema.marks).filter(
      (c) => c !== ln.type.name
    ).map((c) => e.schema.marks[c]).forEach((c) => {
      i.removeMark(o, s, c);
    }), n == null || n(i.addMark(o, s, ln.type(t).create())), !0);
  }
);
S(qa, {
  displayName: "Command<toggleInlineCodeCommand>",
  group: "InlineCode"
});
const Kp = Ve((t) => Ri(/(?:`)([^`]+)(?:`)$/, ln.type(t)));
S(Kp, {
  displayName: "InputRule<inlineCodeInputRule>",
  group: "InlineCode"
});
const Ka = We("inlineCodeKeymap", {
  ToggleInlineCode: {
    shortcuts: "Mod-e",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(qa.key);
    }
  }
});
S(Ka.ctx, {
  displayName: "KeymapCtx<inlineCode>",
  group: "InlineCode"
});
S(Ka.shortcuts, {
  displayName: "Keymap<inlineCode>",
  group: "InlineCode"
});
const Ja = Fi("link");
S(Ja, {
  displayName: "Attr<link>",
  group: "Link"
});
const wr = Li("link", (t) => ({
  attrs: {
    href: {},
    title: { default: null }
  },
  parseDOM: [
    {
      tag: "a[href]",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return {
          href: e.getAttribute("href"),
          title: e.getAttribute("title")
        };
      }
    }
  ],
  toDOM: (e) => ["a", { ...t.get(Ja.key)(e), ...e.attrs }],
  parseMarkdown: {
    match: (e) => e.type === "link",
    runner: (e, n, r) => {
      const i = n.url, o = n.title;
      e.openMark(r, { href: i, title: o }), e.next(n.children), e.closeMark(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "link",
    runner: (e, n) => {
      e.withMark(n, "link", void 0, {
        title: n.attrs.title,
        url: n.attrs.href
      });
    }
  }
}));
S(wr.mark, {
  displayName: "MarkSchema<link>",
  group: "Link"
});
const Jp = j(
  "ToggleLink",
  (t) => (e = {}) => ms(wr.type(t), e)
);
S(Jp, {
  displayName: "Command<toggleLinkCommand>",
  group: "Link"
});
const Up = j(
  "UpdateLink",
  (t) => (e = {}) => (n, r) => {
    if (!r) return !1;
    let i, o = -1;
    const { selection: s } = n, { from: l, to: a } = s;
    if (n.doc.nodesBetween(l, l === a ? a + 1 : a, (p, m) => {
      if (wr.type(t).isInSet(p.marks))
        return i = p, o = m, !1;
    }), !i) return !1;
    const c = i.marks.find(({ type: p }) => p === wr.type(t));
    if (!c) return !1;
    const u = o, h = o + i.nodeSize, { tr: f } = n, d = wr.type(t).create({ ...c.attrs, ...e });
    return d ? (r(
      f.removeMark(u, h, c).addMark(u, h, d).setSelection(new H(f.selection.$anchor)).scrollIntoView()
    ), !0) : !1;
  }
);
S(Up, {
  displayName: "Command<updateLinkCommand>",
  group: "Link"
});
const Gp = Fa("doc", () => ({
  content: "block+",
  parseMarkdown: {
    match: ({ type: t }) => t === "root",
    runner: (t, e, n) => {
      t.injectRoot(e, n);
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "doc",
    runner: (t, e) => {
      t.openNode("root"), t.next(e.content);
    }
  }
}));
S(Gp, {
  displayName: "NodeSchema<doc>",
  group: "Doc"
});
function vC(t) {
  return Jn(t, "paragraph", (e) => {
    var n, r;
    if (((n = e.children) == null ? void 0 : n.length) !== 1) return;
    const i = (r = e.children) == null ? void 0 : r[0];
    if (!i || i.type !== "html") return;
    const { value: o } = i;
    ["<br />", "<br>", "<br/>"].includes(o) && e.children.splice(0, 1);
  });
}
const ws = Un(
  "remark-preserve-empty-line",
  () => () => vC
);
S(ws.plugin, {
  displayName: "Remark<remarkPreserveEmptyLine>",
  group: "Remark"
});
S(ws.options, {
  displayName: "RemarkConfig<remarkPreserveEmptyLine>",
  group: "Remark"
});
const Ua = yt("paragraph");
S(Ua, {
  displayName: "Attr<paragraph>",
  group: "Paragraph"
});
const bn = ye("paragraph", (t) => ({
  content: "inline*",
  group: "block",
  parseDOM: [{ tag: "p" }],
  toDOM: (e) => ["p", t.get(Ua.key)(e), 0],
  parseMarkdown: {
    match: (e) => e.type === "paragraph",
    runner: (e, n, r) => {
      e.openNode(r), n.children ? e.next(n.children) : e.addText(n.value || ""), e.closeNode();
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "paragraph",
    runner: (e, n) => {
      e.openNode("paragraph"), (!n.content || n.content.size === 0) && PC(t) ? e.addNode("html", void 0, "<br />") : Wp(e, n), e.closeNode();
    }
  }
}));
function PC(t) {
  let e = !1;
  try {
    t.get(ws.id), e = !0;
  } catch {
    e = !1;
  }
  return e;
}
S(bn.node, {
  displayName: "NodeSchema<paragraph>",
  group: "Paragraph"
});
S(bn.ctx, {
  displayName: "NodeSchemaCtx<paragraph>",
  group: "Paragraph"
});
const Ga = j(
  "TurnIntoText",
  (t) => () => xi(bn.type(t))
);
S(Ga, {
  displayName: "Command<turnIntoTextCommand>",
  group: "Paragraph"
});
const Ya = We("paragraphKeymap", {
  TurnIntoText: {
    shortcuts: "Mod-Alt-0",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Ga.key);
    }
  }
});
S(Ya.ctx, {
  displayName: "KeymapCtx<paragraph>",
  group: "Paragraph"
});
S(Ya.shortcuts, {
  displayName: "Keymap<paragraph>",
  group: "Paragraph"
});
const zC = Array(6).fill(0).map((t, e) => e + 1);
function LC(t) {
  return t.textContent.toLowerCase().trim().replace(/[^\w\s-]/g, "").replace(/\s+/g, "-");
}
const xs = gt(
  LC,
  "headingIdGenerator"
);
S(xs, {
  displayName: "Ctx<HeadingIdGenerator>",
  group: "Heading"
});
const Qa = yt("heading");
S(Qa, {
  displayName: "Attr<heading>",
  group: "Heading"
});
const Gn = ye("heading", (t) => {
  const e = t.get(xs.key);
  return {
    content: "inline*",
    group: "block",
    defining: !0,
    attrs: {
      id: {
        default: ""
      },
      level: {
        default: 1
      }
    },
    parseDOM: zC.map((n) => ({
      tag: `h${n}`,
      getAttrs: (r) => {
        if (!(r instanceof HTMLElement)) throw Et(r);
        return { level: n, id: r.id };
      }
    })),
    toDOM: (n) => [
      `h${n.attrs.level}`,
      {
        ...t.get(Qa.key)(n),
        id: n.attrs.id || e(n)
      },
      0
    ],
    parseMarkdown: {
      match: ({ type: n }) => n === "heading",
      runner: (n, r, i) => {
        const o = r.depth;
        n.openNode(i, { level: o }), n.next(r.children), n.closeNode();
      }
    },
    toMarkdown: {
      match: (n) => n.type.name === "heading",
      runner: (n, r) => {
        n.openNode("heading", void 0, { depth: r.attrs.level }), Wp(n, r), n.closeNode();
      }
    }
  };
});
S(Gn.node, {
  displayName: "NodeSchema<heading>",
  group: "Heading"
});
S(Gn.ctx, {
  displayName: "NodeSchemaCtx<heading>",
  group: "Heading"
});
const Yp = Ve((t) => Dd(
  /^(?<hashes>#+)\s$/,
  Gn.type(t),
  (e) => {
    var n, r;
    const i = ((r = (n = e.groups) == null ? void 0 : n.hashes) == null ? void 0 : r.length) || 0, o = t.get(gr), { $from: s } = o.state.selection, l = s.node();
    if (l.type.name === "heading") {
      let a = Number(l.attrs.level) + Number(i);
      return a > 6 && (a = 6), { level: a };
    }
    return { level: i };
  }
));
S(Yp, {
  displayName: "InputRule<wrapInHeadingInputRule>",
  group: "Heading"
});
const en = j("WrapInHeading", (t) => (e) => (e ?? (e = 1), e < 1 ? xi(bn.type(t)) : xi(Gn.type(t), { level: e })));
S(en, {
  displayName: "Command<wrapInHeadingCommand>",
  group: "Heading"
});
const Xa = j(
  "DowngradeHeading",
  (t) => () => (e, n, r) => {
    const { $from: i } = e.selection, o = i.node();
    if (o.type !== Gn.type(t) || !e.selection.empty || i.parentOffset !== 0)
      return !1;
    const s = o.attrs.level - 1;
    return s ? (n == null || n(
      e.tr.setNodeMarkup(e.selection.$from.before(), void 0, {
        ...o.attrs,
        level: s
      })
    ), !0) : xi(bn.type(t))(e, n, r);
  }
);
S(Xa, {
  displayName: "Command<downgradeHeadingCommand>",
  group: "Heading"
});
const Za = We("headingKeymap", {
  TurnIntoH1: {
    shortcuts: "Mod-Alt-1",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 1);
    }
  },
  TurnIntoH2: {
    shortcuts: "Mod-Alt-2",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 2);
    }
  },
  TurnIntoH3: {
    shortcuts: "Mod-Alt-3",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 3);
    }
  },
  TurnIntoH4: {
    shortcuts: "Mod-Alt-4",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 4);
    }
  },
  TurnIntoH5: {
    shortcuts: "Mod-Alt-5",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 5);
    }
  },
  TurnIntoH6: {
    shortcuts: "Mod-Alt-6",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(en.key, 6);
    }
  },
  DowngradeHeading: {
    shortcuts: ["Delete", "Backspace"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Xa.key);
    }
  }
});
S(Za.ctx, {
  displayName: "KeymapCtx<heading>",
  group: "Heading"
});
S(Za.shortcuts, {
  displayName: "Keymap<heading>",
  group: "Heading"
});
const ec = yt("blockquote");
S(ec, {
  displayName: "Attr<blockquote>",
  group: "Blockquote"
});
const _i = ye(
  "blockquote",
  (t) => ({
    content: "block+",
    group: "block",
    defining: !0,
    parseDOM: [{ tag: "blockquote" }],
    toDOM: (e) => ["blockquote", t.get(ec.key)(e), 0],
    parseMarkdown: {
      match: ({ type: e }) => e === "blockquote",
      runner: (e, n, r) => {
        e.openNode(r).next(n.children).closeNode();
      }
    },
    toMarkdown: {
      match: (e) => e.type.name === "blockquote",
      runner: (e, n) => {
        e.openNode("blockquote").next(n.content).closeNode();
      }
    }
  })
);
S(_i.node, {
  displayName: "NodeSchema<blockquote>",
  group: "Blockquote"
});
S(_i.ctx, {
  displayName: "NodeSchemaCtx<blockquote>",
  group: "Blockquote"
});
const Qp = Ve(
  (t) => Sa(/^\s*>\s$/, _i.type(t))
);
S(Qp, {
  displayName: "InputRule<wrapInBlockquoteInputRule>",
  group: "Blockquote"
});
const tc = j(
  "WrapInBlockquote",
  (t) => () => Ia(_i.type(t))
);
S(tc, {
  displayName: "Command<wrapInBlockquoteCommand>",
  group: "Blockquote"
});
const nc = We("blockquoteKeymap", {
  WrapInBlockquote: {
    shortcuts: "Mod-Shift-b",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(tc.key);
    }
  }
});
S(nc.ctx, {
  displayName: "KeymapCtx<blockquote>",
  group: "Blockquote"
});
S(nc.shortcuts, {
  displayName: "Keymap<blockquote>",
  group: "Blockquote"
});
const rc = yt("codeBlock", () => ({
  pre: {},
  code: {}
}));
S(rc, {
  displayName: "Attr<codeBlock>",
  group: "CodeBlock"
});
const $i = ye("code_block", (t) => ({
  content: "text*",
  group: "block",
  marks: "",
  defining: !0,
  code: !0,
  attrs: {
    language: {
      default: ""
    }
  },
  parseDOM: [
    {
      tag: "pre",
      preserveWhitespace: "full",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return { language: e.dataset.language };
      }
    }
  ],
  toDOM: (e) => {
    const n = t.get(rc.key)(e);
    return [
      "pre",
      {
        ...n.pre,
        "data-language": e.attrs.language
      },
      ["code", n.code, 0]
    ];
  },
  parseMarkdown: {
    match: ({ type: e }) => e === "code",
    runner: (e, n, r) => {
      const i = n.lang, o = n.value;
      e.openNode(r, { language: i }), o && e.addText(o), e.closeNode();
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "code_block",
    runner: (e, n) => {
      var r;
      e.addNode("code", void 0, ((r = n.content.firstChild) == null ? void 0 : r.text) || "", {
        lang: n.attrs.language
      });
    }
  }
}));
S($i.node, {
  displayName: "NodeSchema<codeBlock>",
  group: "CodeBlock"
});
S($i.ctx, {
  displayName: "NodeSchemaCtx<codeBlock>",
  group: "CodeBlock"
});
const Xp = Ve(
  (t) => Dd(
    /^```(?<language>[a-z]*)?[\s\n]$/,
    $i.type(t),
    (e) => {
      var n;
      return {
        language: ((n = e.groups) == null ? void 0 : n.language) ?? ""
      };
    }
  )
);
S(Xp, {
  displayName: "InputRule<createCodeBlockInputRule>",
  group: "CodeBlock"
});
const ic = j(
  "CreateCodeBlock",
  (t) => (e = "") => xi($i.type(t), { language: e })
);
S(ic, {
  displayName: "Command<createCodeBlockCommand>",
  group: "CodeBlock"
});
const FC = j(
  "UpdateCodeBlockLanguage",
  () => ({ pos: t, language: e } = {
    pos: -1,
    language: ""
  }) => (n, r) => t >= 0 ? (r == null || r(n.tr.setNodeAttribute(t, "language", e)), !0) : !1
);
S(FC, {
  displayName: "Command<updateCodeBlockLanguageCommand>",
  group: "CodeBlock"
});
const oc = We("codeBlockKeymap", {
  CreateCodeBlock: {
    shortcuts: "Mod-Alt-c",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(ic.key);
    }
  }
});
S(oc.ctx, {
  displayName: "KeymapCtx<codeBlock>",
  group: "CodeBlock"
});
S(oc.shortcuts, {
  displayName: "Keymap<codeBlock>",
  group: "CodeBlock"
});
const sc = yt("image");
S(sc, {
  displayName: "Attr<image>",
  group: "Image"
});
const Er = ye("image", (t) => ({
  inline: !0,
  group: "inline",
  selectable: !0,
  draggable: !0,
  marks: "",
  atom: !0,
  defining: !0,
  isolating: !0,
  attrs: {
    src: { default: "" },
    alt: { default: "" },
    title: { default: "" }
  },
  parseDOM: [
    {
      tag: "img[src]",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return {
          src: e.getAttribute("src") || "",
          alt: e.getAttribute("alt") || "",
          title: e.getAttribute("title") || e.getAttribute("alt") || ""
        };
      }
    }
  ],
  toDOM: (e) => ["img", { ...t.get(sc.key)(e), ...e.attrs }],
  parseMarkdown: {
    match: ({ type: e }) => e === "image",
    runner: (e, n, r) => {
      const i = n.url, o = n.alt, s = n.title;
      e.addNode(r, {
        src: i,
        alt: o,
        title: s
      });
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "image",
    runner: (e, n) => {
      e.addNode("image", void 0, void 0, {
        title: n.attrs.title,
        url: n.attrs.src,
        alt: n.attrs.alt
      });
    }
  }
}));
S(Er.node, {
  displayName: "NodeSchema<image>",
  group: "Image"
});
S(Er.ctx, {
  displayName: "NodeSchemaCtx<image>",
  group: "Image"
});
const Zp = j(
  "InsertImage",
  (t) => (e = {}) => (n, r) => {
    if (!r) return !0;
    const { src: i = "", alt: o = "", title: s = "" } = e, l = Er.type(t).create({ src: i, alt: o, title: s });
    return l && r(n.tr.replaceSelectionWith(l).scrollIntoView()), !0;
  }
);
S(Zp, {
  displayName: "Command<insertImageCommand>",
  group: "Image"
});
const em = j(
  "UpdateImage",
  (t) => (e = {}) => (n, r) => {
    const i = sw(
      n.selection,
      Er.type(t)
    );
    if (!i) return !1;
    const { node: o, pos: s } = i, l = { ...o.attrs }, { src: a, alt: c, title: u } = e;
    return a !== void 0 && (l.src = a), c !== void 0 && (l.alt = c), u !== void 0 && (l.title = u), r == null || r(
      n.tr.setNodeMarkup(s, void 0, l).scrollIntoView()
    ), !0;
  }
);
S(em, {
  displayName: "Command<updateImageCommand>",
  group: "Image"
});
const BC = Ve(
  (t) => new Ge(
    /!\[(?<alt>.*?)]\((?<filename>.*?)\s*(?="|\))"?(?<title>[^"]+)?"?\)/,
    (e, n, r, i) => {
      const [o, s, l = "", a] = n;
      return o ? e.tr.replaceWith(
        r,
        i,
        Er.type(t).create({ src: l, alt: s, title: a })
      ) : null;
    }
  )
);
S(BC, {
  displayName: "InputRule<insertImageInputRule>",
  group: "Image"
});
const Zo = yt("hardbreak", (t) => ({
  "data-type": "hardbreak",
  "data-is-inline": t.attrs.isInline
}));
S(Zo, {
  displayName: "Attr<hardbreak>",
  group: "Hardbreak"
});
const _n = ye("hardbreak", (t) => ({
  inline: !0,
  group: "inline",
  attrs: {
    isInline: {
      default: !1
    }
  },
  selectable: !1,
  parseDOM: [
    { tag: "br" },
    {
      tag: 'span[data-type="hardbreak"]',
      getAttrs: () => ({ isInline: !0 })
    }
  ],
  toDOM: (e) => e.attrs.isInline ? ["span", t.get(Zo.key)(e), " "] : ["br", t.get(Zo.key)(e)],
  parseMarkdown: {
    match: ({ type: e }) => e === "break",
    runner: (e, n, r) => {
      var i;
      e.addNode(r, {
        isInline: !!((i = n.data) != null && i.isInline)
      });
    }
  },
  leafText: () => `
`,
  toMarkdown: {
    match: (e) => e.type.name === "hardbreak",
    runner: (e, n) => {
      n.attrs.isInline ? e.addNode("text", void 0, `
`) : e.addNode("break");
    }
  }
}));
S(_n.node, {
  displayName: "NodeSchema<hardbreak>",
  group: "Hardbreak"
});
S(_n.ctx, {
  displayName: "NodeSchemaCtx<hardbreak>",
  group: "Hardbreak"
});
const lc = j(
  "InsertHardbreak",
  (t) => () => (e, n) => {
    var r;
    const { selection: i, tr: o } = e;
    if (!(i instanceof H)) return !1;
    if (i.empty) {
      const s = i.$from.node();
      if (s.childCount > 0 && ((r = s.lastChild) == null ? void 0 : r.type.name) === "hardbreak")
        return n == null || n(
          o.replaceRangeWith(
            i.to - 1,
            i.to,
            e.schema.node("paragraph")
          ).setSelection($.near(o.doc.resolve(i.to))).scrollIntoView()
        ), !0;
    }
    return n == null || n(
      o.setMeta("hardbreak", !0).replaceSelectionWith(_n.type(t).create()).scrollIntoView()
    ), !0;
  }
);
S(lc, {
  displayName: "Command<insertHardbreakCommand>",
  group: "Hardbreak"
});
const ac = We("hardbreakKeymap", {
  InsertHardbreak: {
    shortcuts: "Shift-Enter",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(lc.key);
    }
  }
});
S(ac.ctx, {
  displayName: "KeymapCtx<hardbreak>",
  group: "Hardbreak"
});
S(ac.shortcuts, {
  displayName: "Keymap<hardbreak>",
  group: "Hardbreak"
});
const cc = yt("hr");
S(cc, {
  displayName: "Attr<hr>",
  group: "Hr"
});
const Vi = ye("hr", (t) => ({
  group: "block",
  parseDOM: [{ tag: "hr" }],
  toDOM: (e) => ["hr", t.get(cc.key)(e)],
  parseMarkdown: {
    match: ({ type: e }) => e === "thematicBreak",
    runner: (e, n, r) => {
      e.addNode(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "hr",
    runner: (e) => {
      e.addNode("thematicBreak");
    }
  }
}));
S(Vi.node, {
  displayName: "NodeSchema<hr>",
  group: "Hr"
});
S(Vi.ctx, {
  displayName: "NodeSchemaCtx<hr>",
  group: "Hr"
});
const tm = Ve(
  (t) => new Ge(/^(?:---|___\s|\*\*\*\s)$/, (e, n, r, i) => {
    const { tr: o } = e;
    return n[0] && o.replaceWith(r - 1, i, Vi.type(t).create()), o;
  })
);
S(tm, {
  displayName: "InputRule<insertHrInputRule>",
  group: "Hr"
});
const nm = j(
  "InsertHr",
  (t) => () => (e, n) => {
    if (!n) return !0;
    const r = bn.node.type(t).create(), { tr: i, selection: o } = e, { from: s } = o, l = Vi.type(t).create();
    if (!l) return !0;
    const a = i.replaceSelectionWith(l).insert(s, r), c = $.findFrom(a.doc.resolve(s), 1, !0);
    return c && n(a.setSelection(c).scrollIntoView()), !0;
  }
);
S(nm, {
  displayName: "Command<insertHrCommand>",
  group: "Hr"
});
const uc = yt("bulletList");
S(uc, {
  displayName: "Attr<bulletList>",
  group: "BulletList"
});
const Or = ye("bullet_list", (t) => ({
  content: "listItem+",
  group: "block",
  attrs: {
    spread: {
      default: !1
    }
  },
  parseDOM: [
    {
      tag: "ul",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return {
          spread: e.dataset.spread
        };
      }
    }
  ],
  toDOM: (e) => [
    "ul",
    {
      ...t.get(uc.key)(e),
      "data-spread": e.attrs.spread
    },
    0
  ],
  parseMarkdown: {
    match: ({ type: e, ordered: n }) => e === "list" && !n,
    runner: (e, n, r) => {
      const i = n.spread != null ? `${n.spread}` : "false";
      e.openNode(r, { spread: i }).next(n.children).closeNode();
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "bullet_list",
    runner: (e, n) => {
      e.openNode("list", void 0, {
        ordered: !1,
        spread: n.attrs.spread === "true"
      }).next(n.content).closeNode();
    }
  }
}));
S(Or.node, {
  displayName: "NodeSchema<bulletList>",
  group: "BulletList"
});
S(Or.ctx, {
  displayName: "NodeSchemaCtx<bulletList>",
  group: "BulletList"
});
const rm = Ve(
  (t) => Sa(/^\s*([-+*])\s$/, Or.type(t))
);
S(rm, {
  displayName: "InputRule<wrapInBulletListInputRule>",
  group: "BulletList"
});
const hc = j(
  "WrapInBulletList",
  (t) => () => Ia(Or.type(t))
);
S(hc, {
  displayName: "Command<wrapInBulletListCommand>",
  group: "BulletList"
});
const fc = We("bulletListKeymap", {
  WrapInBulletList: {
    shortcuts: "Mod-Alt-8",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(hc.key);
    }
  }
});
S(fc.ctx, {
  displayName: "KeymapCtx<bulletListKeymap>",
  group: "BulletList"
});
S(fc.shortcuts, {
  displayName: "Keymap<bulletListKeymap>",
  group: "BulletList"
});
const dc = yt("orderedList");
S(dc, {
  displayName: "Attr<orderedList>",
  group: "OrderedList"
});
const Dr = ye("ordered_list", (t) => ({
  content: "listItem+",
  group: "block",
  attrs: {
    order: {
      default: 1
    },
    spread: {
      default: !1
    }
  },
  parseDOM: [
    {
      tag: "ol",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return {
          spread: e.dataset.spread,
          order: e.hasAttribute("start") ? Number(e.getAttribute("start")) : 1
        };
      }
    }
  ],
  toDOM: (e) => [
    "ol",
    {
      ...t.get(dc.key)(e),
      ...e.attrs.order === 1 ? {} : e.attrs.order,
      "data-spread": e.attrs.spread
    },
    0
  ],
  parseMarkdown: {
    match: ({ type: e, ordered: n }) => e === "list" && !!n,
    runner: (e, n, r) => {
      const i = n.spread != null ? `${n.spread}` : "true";
      e.openNode(r, { spread: i }).next(n.children).closeNode();
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "ordered_list",
    runner: (e, n) => {
      e.openNode("list", void 0, {
        ordered: !0,
        start: 1,
        spread: n.attrs.spread === "true"
      }), e.next(n.content), e.closeNode();
    }
  }
}));
S(Dr.node, {
  displayName: "NodeSchema<orderedList>",
  group: "OrderedList"
});
S(Dr.ctx, {
  displayName: "NodeSchemaCtx<orderedList>",
  group: "OrderedList"
});
const im = Ve(
  (t) => Sa(
    /^\s*(\d+)\.\s$/,
    Dr.type(t),
    (e) => ({ order: Number(e[1]) }),
    (e, n) => n.childCount + n.attrs.order === Number(e[1])
  )
);
S(im, {
  displayName: "InputRule<wrapInOrderedListInputRule>",
  group: "OrderedList"
});
const pc = j(
  "WrapInOrderedList",
  (t) => () => Ia(Dr.type(t))
);
S(pc, {
  displayName: "Command<wrapInOrderedListCommand>",
  group: "OrderedList"
});
const mc = We("orderedListKeymap", {
  WrapInOrderedList: {
    shortcuts: "Mod-Alt-7",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(pc.key);
    }
  }
});
S(mc.ctx, {
  displayName: "KeymapCtx<orderedList>",
  group: "OrderedList"
});
S(mc.shortcuts, {
  displayName: "Keymap<orderedList>",
  group: "OrderedList"
});
const gc = yt("listItem");
S(gc, {
  displayName: "Attr<listItem>",
  group: "ListItem"
});
const At = ye("list_item", (t) => ({
  group: "listItem",
  content: "paragraph block*",
  attrs: {
    label: {
      default: "•"
    },
    listType: {
      default: "bullet"
    },
    spread: {
      default: "true"
    }
  },
  defining: !0,
  parseDOM: [
    {
      tag: "li",
      getAttrs: (e) => {
        if (!(e instanceof HTMLElement)) throw Et(e);
        return {
          label: e.dataset.label,
          listType: e.dataset.listType,
          spread: e.dataset.spread
        };
      }
    }
  ],
  toDOM: (e) => [
    "li",
    {
      ...t.get(gc.key)(e),
      "data-label": e.attrs.label,
      "data-list-type": e.attrs.listType,
      "data-spread": e.attrs.spread
    },
    0
  ],
  parseMarkdown: {
    match: ({ type: e }) => e === "listItem",
    runner: (e, n, r) => {
      const i = n.label != null ? `${n.label}.` : "•", o = n.label != null ? "ordered" : "bullet", s = n.spread != null ? `${n.spread}` : "true";
      e.openNode(r, { label: i, listType: o, spread: s }), e.next(n.children), e.closeNode();
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "list_item",
    runner: (e, n) => {
      e.openNode("listItem", void 0, {
        spread: n.attrs.spread === "true"
      }), e.next(n.content), e.closeNode();
    }
  }
}));
S(At.node, {
  displayName: "NodeSchema<listItem>",
  group: "ListItem"
});
S(At.ctx, {
  displayName: "NodeSchemaCtx<listItem>",
  group: "ListItem"
});
const yc = j(
  "SinkListItem",
  (t) => () => OC(At.type(t))
);
S(yc, {
  displayName: "Command<sinkListItemCommand>",
  group: "ListItem"
});
const kc = j(
  "LiftListItem",
  (t) => () => Vp(At.type(t))
);
S(kc, {
  displayName: "Command<liftListItemCommand>",
  group: "ListItem"
});
const bc = j(
  "SplitListItem",
  (t) => () => IC(At.type(t))
);
S(bc, {
  displayName: "Command<splitListItemCommand>",
  group: "ListItem"
});
function _C(t) {
  return (e, n, r) => {
    const { selection: i } = e;
    if (!(i instanceof H)) return !1;
    const { empty: o, $from: s } = i;
    if (!o || s.parentOffset !== 0) return !1;
    const l = s.node(-1);
    return l.type !== At.type(t) || l.firstChild !== s.node() || s.node(-2).childCount > 1 ? !1 : Vp(At.type(t))(e, n, r);
  };
}
const wc = j(
  "LiftFirstListItem",
  (t) => () => _C(t)
);
S(wc, {
  displayName: "Command<liftFirstListItemCommand>",
  group: "ListItem"
});
const xc = We("listItemKeymap", {
  NextListItem: {
    shortcuts: "Enter",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(bc.key);
    }
  },
  SinkListItem: {
    shortcuts: ["Tab", "Mod-]"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(yc.key);
    }
  },
  LiftListItem: {
    shortcuts: ["Shift-Tab", "Mod-["],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(kc.key);
    }
  },
  LiftFirstListItem: {
    shortcuts: ["Backspace", "Delete"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(wc.key);
    }
  }
});
S(xc.ctx, {
  displayName: "KeymapCtx<listItem>",
  group: "ListItem"
});
S(xc.shortcuts, {
  displayName: "Keymap<listItem>",
  group: "ListItem"
});
const om = Fa("text", () => ({
  group: "inline",
  parseMarkdown: {
    match: ({ type: t }) => t === "text",
    runner: (t, e) => {
      t.addText(e.value);
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "text",
    runner: (t, e) => {
      t.addNode("text", void 0, e.text);
    }
  }
}));
S(om, {
  displayName: "NodeSchema<text>",
  group: "Text"
});
const Cc = yt("html");
S(Cc, {
  displayName: "Attr<html>",
  group: "Html"
});
const Sc = ye("html", (t) => ({
  atom: !0,
  group: "inline",
  inline: !0,
  attrs: {
    value: {
      default: ""
    }
  },
  toDOM: (e) => {
    const n = document.createElement("span"), r = {
      ...t.get(Cc.key)(e),
      "data-value": e.attrs.value,
      "data-type": "html"
    };
    return n.textContent = e.attrs.value, ["span", r, e.attrs.value];
  },
  parseDOM: [
    {
      tag: 'span[data-type="html"]',
      getAttrs: (e) => ({
        value: e.dataset.value ?? ""
      })
    }
  ],
  parseMarkdown: {
    match: ({ type: e }) => e === "html",
    runner: (e, n, r) => {
      e.addNode(r, { value: n.value });
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "html",
    runner: (e, n) => {
      e.addNode("html", void 0, n.attrs.value);
    }
  }
}));
S(Sc.node, {
  displayName: "NodeSchema<html>",
  group: "Html"
});
S(Sc.ctx, {
  displayName: "NodeSchemaCtx<html>",
  group: "Html"
});
const $C = [
  Gp,
  Ua,
  bn,
  xs,
  Qa,
  Gn,
  Zo,
  _n,
  ec,
  _i,
  rc,
  $i,
  cc,
  Vi,
  sc,
  Er,
  uc,
  Or,
  dc,
  Dr,
  gc,
  At,
  Ba,
  Ar,
  Va,
  Bi,
  ja,
  ln,
  Ja,
  wr,
  Cc,
  Sc,
  om
].flat(), VC = [
  Qp,
  rm,
  im,
  Xp,
  tm,
  Yp
].flat(), WC = [
  Hp,
  jp,
  Kp,
  qp
], HC = [
  Ga,
  tc,
  en,
  Xa,
  ic,
  lc,
  nm,
  Zp,
  em,
  pc,
  hc,
  yc,
  bc,
  kc,
  wc,
  _a,
  qa,
  Wa,
  Jp,
  Up
], jC = [
  nc,
  oc,
  ac,
  Za,
  xc,
  mc,
  fc,
  Ya,
  $a,
  Ka,
  Ha
].flat(), Mc = Un(
  "remarkAddOrderInList",
  () => () => (t) => {
    Jn(t, "list", (e) => {
      if (e.ordered) {
        const n = e.start ?? 1;
        e.children.forEach((r, i) => {
          r.label = i + n;
        });
      }
    });
  }
);
S(Mc.plugin, {
  displayName: "Remark<remarkAddOrderInListPlugin>",
  group: "Remark"
});
S(Mc.options, {
  displayName: "RemarkConfig<remarkAddOrderInListPlugin>",
  group: "Remark"
});
const Nc = Un(
  "remarkLineBreak",
  () => () => (t) => {
    const e = /[\t ]*(?:\r?\n|\r)/g;
    Jn(
      t,
      "text",
      (n, r, i) => {
        if (!n.value || typeof n.value != "string") return;
        const o = [];
        let s = 0;
        e.lastIndex = 0;
        let l = e.exec(n.value);
        for (; l; ) {
          const c = l.index;
          s !== c && o.push({
            type: "text",
            value: n.value.slice(s, c)
          }), o.push({ type: "break", data: { isInline: !0 } }), s = c + l[0].length, l = e.exec(n.value);
        }
        if (o.length > 0 && i && typeof r == "number")
          return s < n.value.length && o.push({ type: "text", value: n.value.slice(s) }), i.children.splice(r, 1, ...o), r + o.length;
      }
    );
  }
);
S(Nc.plugin, {
  displayName: "Remark<remarkLineBreak>",
  group: "Remark"
});
S(Nc.options, {
  displayName: "RemarkConfig<remarkLineBreak>",
  group: "Remark"
});
const Tc = Un(
  "remarkInlineLink",
  () => RC
);
S(Tc.plugin, {
  displayName: "Remark<remarkInlineLinkPlugin>",
  group: "Remark"
});
S(Tc.options, {
  displayName: "RemarkConfig<remarkInlineLinkPlugin>",
  group: "Remark"
});
const qC = (t) => !!t.children, KC = (t) => t.type === "html";
function JC(t, e) {
  return n(t, 0, null)[0];
  function n(r, i, o) {
    if (qC(r)) {
      const s = [];
      for (let l = 0, a = r.children.length; l < a; l++) {
        const c = r.children[l];
        if (c) {
          const u = n(c, l, r);
          if (u)
            for (let h = 0, f = u.length; h < f; h++) {
              const d = u[h];
              d && s.push(d);
            }
        }
      }
      r.children = s;
    }
    return e(r, i, o);
  }
}
const Ic = Un(
  "remarkHTMLTransformer",
  () => () => (t) => {
    JC(t, (e, n, r) => KC(e) ? ((r == null ? void 0 : r.type) === "root" && (e.children = [{ ...e }], delete e.value, e.type = "paragraph"), [e]) : [e]);
  }
);
S(Ic.plugin, {
  displayName: "Remark<remarkHtmlTransformer>",
  group: "Remark"
});
S(Ic.options, {
  displayName: "RemarkConfig<remarkHtmlTransformer>",
  group: "Remark"
});
const Ac = Un(
  "remarkMarker",
  () => () => (t, e) => {
    const n = (r) => e.value.charAt(r.position.start.offset);
    Jn(
      t,
      (r) => ["strong", "emphasis"].includes(r.type),
      (r) => {
        r.marker = n(r);
      }
    );
  }
);
S(Ac.plugin, {
  displayName: "Remark<remarkMarker>",
  group: "Remark"
});
S(Ac.options, {
  displayName: "RemarkConfig<remarkMarker>",
  group: "Remark"
});
const sm = ot(() => {
  let t = !1;
  const e = new ge(
    "MILKDOWN_INLINE_NODES_CURSOR"
  ), n = new Ne({
    key: e,
    state: {
      init() {
        return !1;
      },
      apply(r) {
        if (!r.selection.empty) return !1;
        const i = r.selection.$from, o = i.nodeBefore, s = i.nodeAfter;
        return !!(o && s && o.isInline && !o.isText && s.isInline && !s.isText);
      }
    },
    props: {
      handleDOMEvents: {
        compositionend: (r, i) => t ? (t = !1, requestAnimationFrame(() => {
          if (n.getState(r.state)) {
            const s = r.state.selection.from;
            i.preventDefault(), r.dispatch(r.state.tr.insertText(i.data || "", s));
          }
        }), !0) : !1,
        compositionstart: (r) => (n.getState(r.state) && (t = !0), !1),
        beforeinput: (r, i) => {
          if (n.getState(r.state) && i instanceof InputEvent && i.data && !t) {
            const s = r.state.selection.from;
            return i.preventDefault(), r.dispatch(r.state.tr.insertText(i.data || "", s)), !0;
          }
          return !1;
        }
      },
      decorations(r) {
        if (n.getState(r)) {
          const s = r.selection.$from.pos, l = document.createElement("span"), a = Me.widget(s, l, {
            side: -1
          }), c = document.createElement("span"), u = Me.widget(s, c);
          return setTimeout(() => {
            l.contentEditable = "true", c.contentEditable = "true";
          }), le.create(r.doc, [a, u]);
        }
        return le.empty;
      }
    }
  });
  return n;
});
S(sm, {
  displayName: "Prose<inlineNodesCursorPlugin>",
  group: "Prose"
});
const lm = ot((t) => new Ne({
  key: new ge("MILKDOWN_HARDBREAK_MARKS"),
  appendTransaction: (e, n, r) => {
    if (!e.length) return;
    const [i] = e;
    if (!i) return;
    const [o] = i.steps;
    if (i.getMeta("hardbreak")) {
      if (!(o instanceof xe)) return;
      const { from: a } = o;
      return r.tr.setNodeMarkup(
        a,
        _n.type(t),
        void 0,
        []
      );
    }
    if (o instanceof $t) {
      let a = r.tr;
      const { from: c, to: u } = o;
      return r.doc.nodesBetween(c, u, (h, f) => {
        h.type === _n.type(t) && (a = a.setNodeMarkup(
          f,
          _n.type(t),
          void 0,
          []
        ));
      }), a;
    }
  }
}));
S(lm, {
  displayName: "Prose<hardbreakClearMarkPlugin>",
  group: "Prose"
});
const Ec = gt(
  ["table", "code_block"],
  "hardbreakFilterNodes"
);
S(Ec, {
  displayName: "Ctx<hardbreakFilterNodes>",
  group: "Prose"
});
const am = ot((t) => {
  const e = t.get(Ec.key);
  return new Ne({
    key: new ge("MILKDOWN_HARDBREAK_FILTER"),
    filterTransaction: (n, r) => {
      const i = n.getMeta("hardbreak"), [o] = n.steps;
      if (i && o) {
        const { from: s } = o, l = r.doc.resolve(s);
        let a = l.depth, c = !0;
        for (; a > 0; )
          e.includes(l.node(a).type.name) && (c = !1), a--;
        return c;
      }
      return !0;
    }
  });
});
S(am, {
  displayName: "Prose<hardbreakFilterPlugin>",
  group: "Prose"
});
const cm = ot((t) => {
  const e = new ge("MILKDOWN_HEADING_ID"), n = (r) => {
    if (r.composing) return;
    const i = t.get(xs.key), o = r.state.tr.setMeta("addToHistory", !1);
    let s = !1;
    const l = {};
    r.state.doc.descendants((a, c) => {
      if (a.type === Gn.type(t)) {
        if (a.textContent.trim().length === 0) return;
        const u = a.attrs;
        let h = i(a);
        l[h] ? (l[h] += 1, h += `-#${l[h]}`) : l[h] = 1, u.id !== h && (s = !0, o.setMeta(e, !0).setNodeMarkup(c, void 0, {
          ...u,
          id: h
        }));
      }
    }), s && r.dispatch(o);
  };
  return new Ne({
    key: e,
    view: (r) => (n(r), {
      update: (i, o) => {
        i.state.doc.eq(o.doc) || n(i);
      }
    })
  });
});
S(cm, {
  displayName: "Prose<syncHeadingIdPlugin>",
  group: "Prose"
});
const um = ot((t) => {
  const e = (n) => {
    if (n.composing || !n.editable) return;
    const r = Dr.type(t), i = Or.type(t), o = At.type(t), s = n.state, l = (u, h) => {
      let f = !1;
      const d = `${h + 1}.`;
      return u.label !== d && (u.label = d, f = !0), f;
    };
    let a = s.tr, c = !1;
    s.doc.descendants((u, h, f, d) => {
      if (u.type === i) {
        const p = u.maybeChild(0);
        (p == null ? void 0 : p.type) === o && p.attrs.listType === "ordered" && (c = !0, a.setNodeMarkup(h, r, { spread: "true" }), u.descendants((m, y, g, N) => {
          if (m.type === o) {
            const C = { ...m.attrs };
            l(C, N) && (a = a.setNodeMarkup(y, void 0, C));
          }
          return !1;
        }));
      } else if (u.type === o && (f == null ? void 0 : f.type) === r) {
        const p = { ...u.attrs };
        let m = !1;
        p.listType !== "ordered" && (p.listType = "ordered", m = !0), (f == null ? void 0 : f.maybeChild(0)) && (m = l(p, d)), m && (a = a.setNodeMarkup(h, void 0, p), c = !0);
      }
    }), c && n.dispatch(a.setMeta("addToHistory", !1));
  };
  return new Ne({
    key: new ge("MILKDOWN_KEEP_LIST_ORDER"),
    view: (n) => (e(n), {
      update: (r) => {
        e(r);
      }
    })
  });
});
S(um, {
  displayName: "Prose<syncListOrderPlugin>",
  group: "Prose"
});
const UC = [
  lm,
  Ec,
  am,
  sm,
  Mc,
  Tc,
  Nc,
  Ic,
  Ac,
  ws,
  cm,
  um
].flat(), GC = [
  $C,
  VC,
  WC,
  HC,
  jC,
  UC
].flat();
var ea, ta;
if (typeof WeakMap < "u") {
  let t = /* @__PURE__ */ new WeakMap();
  ea = (e) => t.get(e), ta = (e, n) => (t.set(e, n), n);
} else {
  const t = [];
  let n = 0;
  ea = (r) => {
    for (let i = 0; i < t.length; i += 2)
      if (t[i] == r) return t[i + 1];
  }, ta = (r, i) => (n == 10 && (n = 0), t[n++] = r, t[n++] = i);
}
var ee = class {
  constructor(t, e, n, r) {
    this.width = t, this.height = e, this.map = n, this.problems = r;
  }
  // Find the dimensions of the cell at the given position.
  findCell(t) {
    for (let e = 0; e < this.map.length; e++) {
      const n = this.map[e];
      if (n != t) continue;
      const r = e % this.width, i = e / this.width | 0;
      let o = r + 1, s = i + 1;
      for (let l = 1; o < this.width && this.map[e + l] == n; l++)
        o++;
      for (let l = 1; s < this.height && this.map[e + this.width * l] == n; l++)
        s++;
      return { left: r, top: i, right: o, bottom: s };
    }
    throw new RangeError(`No cell with offset ${t} found`);
  }
  // Find the left side of the cell at the given position.
  colCount(t) {
    for (let e = 0; e < this.map.length; e++)
      if (this.map[e] == t)
        return e % this.width;
    throw new RangeError(`No cell with offset ${t} found`);
  }
  // Find the next cell in the given direction, starting from the cell
  // at `pos`, if any.
  nextCell(t, e, n) {
    const { left: r, right: i, top: o, bottom: s } = this.findCell(t);
    return e == "horiz" ? (n < 0 ? r == 0 : i == this.width) ? null : this.map[o * this.width + (n < 0 ? r - 1 : i)] : (n < 0 ? o == 0 : s == this.height) ? null : this.map[r + this.width * (n < 0 ? o - 1 : s)];
  }
  // Get the rectangle spanning the two given cells.
  rectBetween(t, e) {
    const {
      left: n,
      right: r,
      top: i,
      bottom: o
    } = this.findCell(t), {
      left: s,
      right: l,
      top: a,
      bottom: c
    } = this.findCell(e);
    return {
      left: Math.min(n, s),
      top: Math.min(i, a),
      right: Math.max(r, l),
      bottom: Math.max(o, c)
    };
  }
  // Return the position of all cells that have the top left corner in
  // the given rectangle.
  cellsInRect(t) {
    const e = [], n = {};
    for (let r = t.top; r < t.bottom; r++)
      for (let i = t.left; i < t.right; i++) {
        const o = r * this.width + i, s = this.map[o];
        n[s] || (n[s] = !0, !(i == t.left && i && this.map[o - 1] == s || r == t.top && r && this.map[o - this.width] == s) && e.push(s));
      }
    return e;
  }
  // Return the position at which the cell at the given row and column
  // starts, or would start, if a cell started there.
  positionAt(t, e, n) {
    for (let r = 0, i = 0; ; r++) {
      const o = i + n.child(r).nodeSize;
      if (r == t) {
        let s = e + t * this.width;
        const l = (t + 1) * this.width;
        for (; s < l && this.map[s] < i; ) s++;
        return s == l ? o - 1 : this.map[s];
      }
      i = o;
    }
  }
  // Find the table map for the given table node.
  static get(t) {
    return ea(t) || ta(t, YC(t));
  }
};
function YC(t) {
  if (t.type.spec.tableRole != "table")
    throw new RangeError("Not a table node: " + t.type.name);
  const e = QC(t), n = t.childCount, r = [];
  let i = 0, o = null;
  const s = [];
  for (let c = 0, u = e * n; c < u; c++) r[c] = 0;
  for (let c = 0, u = 0; c < n; c++) {
    const h = t.child(c);
    u++;
    for (let p = 0; ; p++) {
      for (; i < r.length && r[i] != 0; ) i++;
      if (p == h.childCount) break;
      const m = h.child(p), { colspan: y, rowspan: g, colwidth: N } = m.attrs;
      for (let C = 0; C < g; C++) {
        if (C + c >= n) {
          (o || (o = [])).push({
            type: "overlong_rowspan",
            pos: u,
            n: g - C
          });
          break;
        }
        const v = i + C * e;
        for (let I = 0; I < y; I++) {
          r[v + I] == 0 ? r[v + I] = u : (o || (o = [])).push({
            type: "collision",
            row: c,
            pos: u,
            n: y - I
          });
          const w = N && N[I];
          if (w) {
            const z = (v + I) % e * 2, W = s[z];
            W == null || W != w && s[z + 1] == 1 ? (s[z] = w, s[z + 1] = 1) : W == w && s[z + 1]++;
          }
        }
      }
      i += y, u += m.nodeSize;
    }
    const f = (c + 1) * e;
    let d = 0;
    for (; i < f; ) r[i++] == 0 && d++;
    d && (o || (o = [])).push({ type: "missing", row: c, n: d }), u++;
  }
  (e === 0 || n === 0) && (o || (o = [])).push({ type: "zero_sized" });
  const l = new ee(e, n, r, o);
  let a = !1;
  for (let c = 0; !a && c < s.length; c += 2)
    s[c] != null && s[c + 1] < n && (a = !0);
  return a && XC(l, s, t), l;
}
function QC(t) {
  let e = -1, n = !1;
  for (let r = 0; r < t.childCount; r++) {
    const i = t.child(r);
    let o = 0;
    if (n)
      for (let s = 0; s < r; s++) {
        const l = t.child(s);
        for (let a = 0; a < l.childCount; a++) {
          const c = l.child(a);
          s + c.attrs.rowspan > r && (o += c.attrs.colspan);
        }
      }
    for (let s = 0; s < i.childCount; s++) {
      const l = i.child(s);
      o += l.attrs.colspan, l.attrs.rowspan > 1 && (n = !0);
    }
    e == -1 ? e = o : e != o && (e = Math.max(e, o));
  }
  return e;
}
function XC(t, e, n) {
  t.problems || (t.problems = []);
  const r = {};
  for (let i = 0; i < t.map.length; i++) {
    const o = t.map[i];
    if (r[o]) continue;
    r[o] = !0;
    const s = n.nodeAt(o);
    if (!s)
      throw new RangeError(`No cell with offset ${o} found`);
    let l = null;
    const a = s.attrs;
    for (let c = 0; c < a.colspan; c++) {
      const u = (i + c) % t.width, h = e[u * 2];
      h != null && (!a.colwidth || a.colwidth[c] != h) && ((l || (l = ZC(a)))[c] = h);
    }
    l && t.problems.unshift({
      type: "colwidth mismatch",
      pos: o,
      colwidth: l
    });
  }
}
function ZC(t) {
  if (t.colwidth) return t.colwidth.slice();
  const e = [];
  for (let n = 0; n < t.colspan; n++) e.push(0);
  return e;
}
function Bh(t, e) {
  if (typeof t == "string")
    return {};
  const n = t.getAttribute("data-colwidth"), r = n && /^\d+(,\d+)*$/.test(n) ? n.split(",").map((s) => Number(s)) : null, i = Number(t.getAttribute("colspan") || 1), o = {
    colspan: i,
    rowspan: Number(t.getAttribute("rowspan") || 1),
    colwidth: r && r.length == i ? r : null
  };
  for (const s in e) {
    const l = e[s].getFromDOM, a = l && l(t);
    a != null && (o[s] = a);
  }
  return o;
}
function _h(t, e) {
  const n = {};
  t.attrs.colspan != 1 && (n.colspan = t.attrs.colspan), t.attrs.rowspan != 1 && (n.rowspan = t.attrs.rowspan), t.attrs.colwidth && (n["data-colwidth"] = t.attrs.colwidth.join(","));
  for (const r in e) {
    const i = e[r].setDOMAttr;
    i && i(t.attrs[r], n);
  }
  return n;
}
function eS(t) {
  if (t !== null) {
    if (!Array.isArray(t))
      throw new TypeError("colwidth must be null or an array");
    for (const e of t)
      if (typeof e != "number")
        throw new TypeError("colwidth must be null or an array of numbers");
  }
}
function tS(t) {
  const e = t.cellAttributes || {}, n = {
    colspan: { default: 1, validate: "number" },
    rowspan: { default: 1, validate: "number" },
    colwidth: { default: null, validate: eS }
  };
  for (const r in e)
    n[r] = {
      default: e[r].default,
      validate: e[r].validate
    };
  return {
    table: {
      content: "table_row+",
      tableRole: "table",
      isolating: !0,
      group: t.tableGroup,
      parseDOM: [{ tag: "table" }],
      toDOM() {
        return ["table", ["tbody", 0]];
      }
    },
    table_row: {
      content: "(table_cell | table_header)*",
      tableRole: "row",
      parseDOM: [{ tag: "tr" }],
      toDOM() {
        return ["tr", 0];
      }
    },
    table_cell: {
      content: t.cellContent,
      attrs: n,
      tableRole: "cell",
      isolating: !0,
      parseDOM: [
        { tag: "td", getAttrs: (r) => Bh(r, e) }
      ],
      toDOM(r) {
        return ["td", _h(r, e), 0];
      }
    },
    table_header: {
      content: t.cellContent,
      attrs: n,
      tableRole: "header_cell",
      isolating: !0,
      parseDOM: [
        { tag: "th", getAttrs: (r) => Bh(r, e) }
      ],
      toDOM(r) {
        return ["th", _h(r, e), 0];
      }
    }
  };
}
function rt(t) {
  let e = t.cached.tableNodeTypes;
  if (!e) {
    e = t.cached.tableNodeTypes = {};
    for (const n in t.nodes) {
      const r = t.nodes[n], i = r.spec.tableRole;
      i && (e[i] = r);
    }
  }
  return e;
}
var rn = new ge("selectingCells");
function Wi(t) {
  for (let e = t.depth - 1; e > 0; e--)
    if (t.node(e).type.spec.tableRole == "row")
      return t.node(0).resolve(t.before(e + 1));
  return null;
}
function st(t) {
  const e = t.selection.$head;
  for (let n = e.depth; n > 0; n--)
    if (e.node(n).type.spec.tableRole == "row") return !0;
  return !1;
}
function Cs(t) {
  const e = t.selection;
  if ("$anchorCell" in e && e.$anchorCell)
    return e.$anchorCell.pos > e.$headCell.pos ? e.$anchorCell : e.$headCell;
  if ("node" in e && e.node && e.node.type.spec.tableRole == "cell")
    return e.$anchor;
  const n = Wi(e.$head) || nS(e.$head);
  if (n)
    return n;
  throw new RangeError(`No cell found around position ${e.head}`);
}
function nS(t) {
  for (let e = t.nodeAfter, n = t.pos; e; e = e.firstChild, n++) {
    const r = e.type.spec.tableRole;
    if (r == "cell" || r == "header_cell") return t.doc.resolve(n);
  }
  for (let e = t.nodeBefore, n = t.pos; e; e = e.lastChild, n--) {
    const r = e.type.spec.tableRole;
    if (r == "cell" || r == "header_cell")
      return t.doc.resolve(n - e.nodeSize);
  }
}
function na(t) {
  return t.parent.type.spec.tableRole == "row" && !!t.nodeAfter;
}
function rS(t) {
  return t.node(0).resolve(t.pos + t.nodeAfter.nodeSize);
}
function Oc(t, e) {
  return t.depth == e.depth && t.pos >= e.start(-1) && t.pos <= e.end(-1);
}
function hm(t, e, n) {
  const r = t.node(-1), i = ee.get(r), o = t.start(-1), s = i.nextCell(t.pos - o, e, n);
  return s == null ? null : t.node(0).resolve(o + s);
}
function qn(t, e, n = 1) {
  const r = { ...t, colspan: t.colspan - n };
  return r.colwidth && (r.colwidth = r.colwidth.slice(), r.colwidth.splice(e, n), r.colwidth.some((i) => i > 0) || (r.colwidth = null)), r;
}
function iS(t, e, n = 1) {
  const r = { ...t, colspan: t.colspan + n };
  if (r.colwidth) {
    r.colwidth = r.colwidth.slice();
    for (let i = 0; i < n; i++) r.colwidth.splice(e, 0, 0);
  }
  return r;
}
function oS(t, e, n) {
  const r = rt(e.type.schema).header_cell;
  for (let i = 0; i < t.height; i++)
    if (e.nodeAt(t.map[n + i * t.width]).type != r)
      return !1;
  return !0;
}
var se = class Ft extends $ {
  // A table selection is identified by its anchor and head cells. The
  // positions given to this constructor should point _before_ two
  // cells in the same table. They may be the same, to select a single
  // cell.
  constructor(e, n = e) {
    const r = e.node(-1), i = ee.get(r), o = e.start(-1), s = i.rectBetween(
      e.pos - o,
      n.pos - o
    ), l = e.node(0), a = i.cellsInRect(s).filter((u) => u != n.pos - o);
    a.unshift(n.pos - o);
    const c = a.map((u) => {
      const h = r.nodeAt(u);
      if (!h)
        throw RangeError(`No cell with offset ${u} found`);
      const f = o + u + 1;
      return new Ad(
        l.resolve(f),
        l.resolve(f + h.content.size)
      );
    });
    super(c[0].$from, c[0].$to, c), this.$anchorCell = e, this.$headCell = n;
  }
  map(e, n) {
    const r = e.resolve(n.map(this.$anchorCell.pos)), i = e.resolve(n.map(this.$headCell.pos));
    if (na(r) && na(i) && Oc(r, i)) {
      const o = this.$anchorCell.node(-1) != r.node(-1);
      return o && this.isRowSelection() ? Ft.rowSelection(r, i) : o && this.isColSelection() ? Ft.colSelection(r, i) : new Ft(r, i);
    }
    return H.between(r, i);
  }
  // Returns a rectangular slice of table rows containing the selected
  // cells.
  content() {
    const e = this.$anchorCell.node(-1), n = ee.get(e), r = this.$anchorCell.start(-1), i = n.rectBetween(
      this.$anchorCell.pos - r,
      this.$headCell.pos - r
    ), o = {}, s = [];
    for (let a = i.top; a < i.bottom; a++) {
      const c = [];
      for (let u = a * n.width + i.left, h = i.left; h < i.right; h++, u++) {
        const f = n.map[u];
        if (o[f]) continue;
        o[f] = !0;
        const d = n.findCell(f);
        let p = e.nodeAt(f);
        if (!p)
          throw RangeError(`No cell with offset ${f} found`);
        const m = i.left - d.left, y = d.right - i.right;
        if (m > 0 || y > 0) {
          let g = p.attrs;
          if (m > 0 && (g = qn(g, 0, m)), y > 0 && (g = qn(
            g,
            g.colspan - y,
            y
          )), d.left < i.left) {
            if (p = p.type.createAndFill(g), !p)
              throw RangeError(
                `Could not create cell with attrs ${JSON.stringify(g)}`
              );
          } else
            p = p.type.create(g, p.content);
        }
        if (d.top < i.top || d.bottom > i.bottom) {
          const g = {
            ...p.attrs,
            rowspan: Math.min(d.bottom, i.bottom) - Math.max(d.top, i.top)
          };
          d.top < i.top ? p = p.type.createAndFill(g) : p = p.type.create(g, p.content);
        }
        c.push(p);
      }
      s.push(e.child(a).copy(T.from(c)));
    }
    const l = this.isColSelection() && this.isRowSelection() ? e : s;
    return new E(T.from(l), 1, 1);
  }
  replace(e, n = E.empty) {
    const r = e.steps.length, i = this.ranges;
    for (let s = 0; s < i.length; s++) {
      const { $from: l, $to: a } = i[s], c = e.mapping.slice(r);
      e.replace(
        c.map(l.pos),
        c.map(a.pos),
        s ? E.empty : n
      );
    }
    const o = $.findFrom(
      e.doc.resolve(e.mapping.slice(r).map(this.to)),
      -1
    );
    o && e.setSelection(o);
  }
  replaceWith(e, n) {
    this.replace(e, new E(T.from(n), 0, 0));
  }
  forEachCell(e) {
    const n = this.$anchorCell.node(-1), r = ee.get(n), i = this.$anchorCell.start(-1), o = r.cellsInRect(
      r.rectBetween(
        this.$anchorCell.pos - i,
        this.$headCell.pos - i
      )
    );
    for (let s = 0; s < o.length; s++)
      e(n.nodeAt(o[s]), i + o[s]);
  }
  // True if this selection goes all the way from the top to the
  // bottom of the table.
  isColSelection() {
    const e = this.$anchorCell.index(-1), n = this.$headCell.index(-1);
    if (Math.min(e, n) > 0) return !1;
    const r = e + this.$anchorCell.nodeAfter.attrs.rowspan, i = n + this.$headCell.nodeAfter.attrs.rowspan;
    return Math.max(r, i) == this.$headCell.node(-1).childCount;
  }
  // Returns the smallest column selection that covers the given anchor
  // and head cell.
  static colSelection(e, n = e) {
    const r = e.node(-1), i = ee.get(r), o = e.start(-1), s = i.findCell(e.pos - o), l = i.findCell(n.pos - o), a = e.node(0);
    return s.top <= l.top ? (s.top > 0 && (e = a.resolve(o + i.map[s.left])), l.bottom < i.height && (n = a.resolve(
      o + i.map[i.width * (i.height - 1) + l.right - 1]
    ))) : (l.top > 0 && (n = a.resolve(o + i.map[l.left])), s.bottom < i.height && (e = a.resolve(
      o + i.map[i.width * (i.height - 1) + s.right - 1]
    ))), new Ft(e, n);
  }
  // True if this selection goes all the way from the left to the
  // right of the table.
  isRowSelection() {
    const e = this.$anchorCell.node(-1), n = ee.get(e), r = this.$anchorCell.start(-1), i = n.colCount(this.$anchorCell.pos - r), o = n.colCount(this.$headCell.pos - r);
    if (Math.min(i, o) > 0) return !1;
    const s = i + this.$anchorCell.nodeAfter.attrs.colspan, l = o + this.$headCell.nodeAfter.attrs.colspan;
    return Math.max(s, l) == n.width;
  }
  eq(e) {
    return e instanceof Ft && e.$anchorCell.pos == this.$anchorCell.pos && e.$headCell.pos == this.$headCell.pos;
  }
  // Returns the smallest row selection that covers the given anchor
  // and head cell.
  static rowSelection(e, n = e) {
    const r = e.node(-1), i = ee.get(r), o = e.start(-1), s = i.findCell(e.pos - o), l = i.findCell(n.pos - o), a = e.node(0);
    return s.left <= l.left ? (s.left > 0 && (e = a.resolve(
      o + i.map[s.top * i.width]
    )), l.right < i.width && (n = a.resolve(
      o + i.map[i.width * (l.top + 1) - 1]
    ))) : (l.left > 0 && (n = a.resolve(o + i.map[l.top * i.width])), s.right < i.width && (e = a.resolve(
      o + i.map[i.width * (s.top + 1) - 1]
    ))), new Ft(e, n);
  }
  toJSON() {
    return {
      type: "cell",
      anchor: this.$anchorCell.pos,
      head: this.$headCell.pos
    };
  }
  static fromJSON(e, n) {
    return new Ft(e.resolve(n.anchor), e.resolve(n.head));
  }
  static create(e, n, r = n) {
    return new Ft(e.resolve(n), e.resolve(r));
  }
  getBookmark() {
    return new sS(this.$anchorCell.pos, this.$headCell.pos);
  }
};
se.prototype.visible = !1;
$.jsonID("cell", se);
var sS = class fm {
  constructor(e, n) {
    this.anchor = e, this.head = n;
  }
  map(e) {
    return new fm(e.map(this.anchor), e.map(this.head));
  }
  resolve(e) {
    const n = e.resolve(this.anchor), r = e.resolve(this.head);
    return n.parent.type.spec.tableRole == "row" && r.parent.type.spec.tableRole == "row" && n.index() < n.parent.childCount && r.index() < r.parent.childCount && Oc(n, r) ? new se(n, r) : $.near(r, 1);
  }
};
function lS(t) {
  if (!(t.selection instanceof se)) return null;
  const e = [];
  return t.selection.forEachCell((n, r) => {
    e.push(
      Me.node(r, r + n.nodeSize, { class: "selectedCell" })
    );
  }), le.create(t.doc, e);
}
function aS({ $from: t, $to: e }) {
  if (t.pos == e.pos || t.pos < e.pos - 6) return !1;
  let n = t.pos, r = e.pos, i = t.depth;
  for (; i >= 0 && !(t.after(i + 1) < t.end(i)); i--, n++)
    ;
  for (let o = e.depth; o >= 0 && !(e.before(o + 1) > e.start(o)); o--, r--)
    ;
  return n == r && /row|table/.test(t.node(i).type.spec.tableRole);
}
function cS({ $from: t, $to: e }) {
  let n, r;
  for (let i = t.depth; i > 0; i--) {
    const o = t.node(i);
    if (o.type.spec.tableRole === "cell" || o.type.spec.tableRole === "header_cell") {
      n = o;
      break;
    }
  }
  for (let i = e.depth; i > 0; i--) {
    const o = e.node(i);
    if (o.type.spec.tableRole === "cell" || o.type.spec.tableRole === "header_cell") {
      r = o;
      break;
    }
  }
  return n !== r && e.parentOffset === 0;
}
function uS(t, e, n) {
  const r = (e || t).selection, i = (e || t).doc;
  let o, s;
  if (r instanceof _ && (s = r.node.type.spec.tableRole)) {
    if (s == "cell" || s == "header_cell")
      o = se.create(i, r.from);
    else if (s == "row") {
      const l = i.resolve(r.from + 1);
      o = se.rowSelection(l, l);
    } else if (!n) {
      const l = ee.get(r.node), a = r.from + 1, c = a + l.map[l.width * l.height - 1];
      o = se.create(i, a + 1, c);
    }
  } else r instanceof H && aS(r) ? o = H.create(i, r.from) : r instanceof H && cS(r) && (o = H.create(i, r.$from.start(), r.$from.end()));
  return o && (e || (e = t.tr)).setSelection(o), e;
}
var hS = new ge("fix-tables");
function dm(t, e, n, r) {
  const i = t.childCount, o = e.childCount;
  e: for (let s = 0, l = 0; s < o; s++) {
    const a = e.child(s);
    for (let c = l, u = Math.min(i, s + 3); c < u; c++)
      if (t.child(c) == a) {
        l = c + 1, n += a.nodeSize;
        continue e;
      }
    r(a, n), l < i && t.child(l).sameMarkup(a) ? dm(t.child(l), a, n + 1, r) : a.nodesBetween(0, a.content.size, r, n + 1), n += a.nodeSize;
  }
}
function fS(t, e) {
  let n;
  const r = (i, o) => {
    i.type.spec.tableRole == "table" && (n = dS(t, i, o, n));
  };
  return e ? e.doc != t.doc && dm(e.doc, t.doc, 0, r) : t.doc.descendants(r), n;
}
function dS(t, e, n, r) {
  const i = ee.get(e);
  if (!i.problems) return r;
  r || (r = t.tr);
  const o = [];
  for (let a = 0; a < i.height; a++) o.push(0);
  for (let a = 0; a < i.problems.length; a++) {
    const c = i.problems[a];
    if (c.type == "collision") {
      const u = e.nodeAt(c.pos);
      if (!u) continue;
      const h = u.attrs;
      for (let f = 0; f < h.rowspan; f++) o[c.row + f] += c.n;
      r.setNodeMarkup(
        r.mapping.map(n + 1 + c.pos),
        null,
        qn(h, h.colspan - c.n, c.n)
      );
    } else if (c.type == "missing")
      o[c.row] += c.n;
    else if (c.type == "overlong_rowspan") {
      const u = e.nodeAt(c.pos);
      if (!u) continue;
      r.setNodeMarkup(r.mapping.map(n + 1 + c.pos), null, {
        ...u.attrs,
        rowspan: u.attrs.rowspan - c.n
      });
    } else if (c.type == "colwidth mismatch") {
      const u = e.nodeAt(c.pos);
      if (!u) continue;
      r.setNodeMarkup(r.mapping.map(n + 1 + c.pos), null, {
        ...u.attrs,
        colwidth: c.colwidth
      });
    } else if (c.type == "zero_sized") {
      const u = r.mapping.map(n);
      r.delete(u, u + e.nodeSize);
    }
  }
  let s, l;
  for (let a = 0; a < o.length; a++)
    o[a] && (s == null && (s = a), l = a);
  for (let a = 0, c = n + 1; a < i.height; a++) {
    const u = e.child(a), h = c + u.nodeSize, f = o[a];
    if (f > 0) {
      let d = "cell";
      u.firstChild && (d = u.firstChild.type.spec.tableRole);
      const p = [];
      for (let y = 0; y < f; y++) {
        const g = rt(t.schema)[d].createAndFill();
        g && p.push(g);
      }
      const m = (a == 0 || s == a - 1) && l == a ? c + 1 : h - 1;
      r.insert(r.mapping.map(m), p);
    }
    c = h;
  }
  return r.setMeta(hS, { fixTables: !0 });
}
function wn(t) {
  const e = t.selection, n = Cs(t), r = n.node(-1), i = n.start(-1), o = ee.get(r);
  return { ...e instanceof se ? o.rectBetween(
    e.$anchorCell.pos - i,
    e.$headCell.pos - i
  ) : o.findCell(n.pos - i), tableStart: i, map: o, table: r };
}
function pm(t, { map: e, tableStart: n, table: r }, i) {
  let o = i > 0 ? -1 : 0;
  oS(e, r, i + o) && (o = i == 0 || i == e.width ? null : 0);
  for (let s = 0; s < e.height; s++) {
    const l = s * e.width + i;
    if (i > 0 && i < e.width && e.map[l - 1] == e.map[l]) {
      const a = e.map[l], c = r.nodeAt(a);
      t.setNodeMarkup(
        t.mapping.map(n + a),
        null,
        iS(c.attrs, i - e.colCount(a))
      ), s += c.attrs.rowspan - 1;
    } else {
      const a = o == null ? rt(r.type.schema).cell : r.nodeAt(e.map[l + o]).type, c = e.positionAt(s, i, r);
      t.insert(t.mapping.map(n + c), a.createAndFill());
    }
  }
  return t;
}
function pS(t, e) {
  if (!st(t)) return !1;
  if (e) {
    const n = wn(t);
    e(pm(t.tr, n, n.left));
  }
  return !0;
}
function mS(t, e) {
  if (!st(t)) return !1;
  if (e) {
    const n = wn(t);
    e(pm(t.tr, n, n.right));
  }
  return !0;
}
function gS(t, { map: e, table: n, tableStart: r }, i) {
  const o = t.mapping.maps.length;
  for (let s = 0; s < e.height; ) {
    const l = s * e.width + i, a = e.map[l], c = n.nodeAt(a), u = c.attrs;
    if (i > 0 && e.map[l - 1] == a || i < e.width - 1 && e.map[l + 1] == a)
      t.setNodeMarkup(
        t.mapping.slice(o).map(r + a),
        null,
        qn(u, i - e.colCount(a))
      );
    else {
      const h = t.mapping.slice(o).map(r + a);
      t.delete(h, h + c.nodeSize);
    }
    s += u.rowspan;
  }
}
function yS(t, e) {
  if (!st(t)) return !1;
  if (e) {
    const n = wn(t), r = t.tr;
    if (n.left == 0 && n.right == n.map.width) return !1;
    for (let i = n.right - 1; gS(r, n, i), i != n.left; i--) {
      const o = n.tableStart ? r.doc.nodeAt(n.tableStart - 1) : r.doc;
      if (!o)
        throw RangeError("No table found");
      n.table = o, n.map = ee.get(o);
    }
    e(r);
  }
  return !0;
}
function kS(t, { map: e, table: n, tableStart: r }, i) {
  let o = 0;
  for (let c = 0; c < i; c++) o += n.child(c).nodeSize;
  const s = o + n.child(i).nodeSize, l = t.mapping.maps.length;
  t.delete(o + r, s + r);
  const a = /* @__PURE__ */ new Set();
  for (let c = 0, u = i * e.width; c < e.width; c++, u++) {
    const h = e.map[u];
    if (!a.has(h)) {
      if (a.add(h), i > 0 && h == e.map[u - e.width]) {
        const f = n.nodeAt(h).attrs;
        t.setNodeMarkup(t.mapping.slice(l).map(h + r), null, {
          ...f,
          rowspan: f.rowspan - 1
        }), c += f.colspan - 1;
      } else if (i < e.height && h == e.map[u + e.width]) {
        const f = n.nodeAt(h), d = f.attrs, p = f.type.create(
          { ...d, rowspan: f.attrs.rowspan - 1 },
          f.content
        ), m = e.positionAt(i + 1, c, n);
        t.insert(t.mapping.slice(l).map(r + m), p), c += d.colspan - 1;
      }
    }
  }
}
function bS(t, e) {
  if (!st(t)) return !1;
  if (e) {
    const n = wn(t), r = t.tr;
    if (n.top == 0 && n.bottom == n.map.height) return !1;
    for (let i = n.bottom - 1; kS(r, n, i), i != n.top; i--) {
      const o = n.tableStart ? r.doc.nodeAt(n.tableStart - 1) : r.doc;
      if (!o)
        throw RangeError("No table found");
      n.table = o, n.map = ee.get(n.table);
    }
    e(r);
  }
  return !0;
}
function wS(t, e) {
  return function(n, r) {
    if (!st(n)) return !1;
    const i = Cs(n);
    if (i.nodeAfter.attrs[t] === e) return !1;
    if (r) {
      const o = n.tr;
      n.selection instanceof se ? n.selection.forEachCell((s, l) => {
        s.attrs[t] !== e && o.setNodeMarkup(l, null, {
          ...s.attrs,
          [t]: e
        });
      }) : o.setNodeMarkup(i.pos, null, {
        ...i.nodeAfter.attrs,
        [t]: e
      }), r(o);
    }
    return !0;
  };
}
function xS(t) {
  return function(e, n) {
    if (!st(e)) return !1;
    if (n) {
      const r = rt(e.schema), i = wn(e), o = e.tr, s = i.map.cellsInRect(
        t == "column" ? {
          left: i.left,
          top: 0,
          right: i.right,
          bottom: i.map.height
        } : t == "row" ? {
          left: 0,
          top: i.top,
          right: i.map.width,
          bottom: i.bottom
        } : i
      ), l = s.map((a) => i.table.nodeAt(a));
      for (let a = 0; a < s.length; a++)
        l[a].type == r.header_cell && o.setNodeMarkup(
          i.tableStart + s[a],
          r.cell,
          l[a].attrs
        );
      if (o.steps.length == 0)
        for (let a = 0; a < s.length; a++)
          o.setNodeMarkup(
            i.tableStart + s[a],
            r.header_cell,
            l[a].attrs
          );
      n(o);
    }
    return !0;
  };
}
function $h(t, e, n) {
  const r = e.map.cellsInRect({
    left: 0,
    top: 0,
    right: t == "row" ? e.map.width : 1,
    bottom: t == "column" ? e.map.height : 1
  });
  for (let i = 0; i < r.length; i++) {
    const o = e.table.nodeAt(r[i]);
    if (o && o.type !== n.header_cell)
      return !1;
  }
  return !0;
}
function Dc(t, e) {
  return e = e || { useDeprecatedLogic: !1 }, e.useDeprecatedLogic ? xS(t) : function(n, r) {
    if (!st(n)) return !1;
    if (r) {
      const i = rt(n.schema), o = wn(n), s = n.tr, l = $h("row", o, i), a = $h(
        "column",
        o,
        i
      ), u = (t === "column" ? l : t === "row" ? a : !1) ? 1 : 0, h = t == "column" ? {
        left: 0,
        top: u,
        right: 1,
        bottom: o.map.height
      } : t == "row" ? {
        left: u,
        top: 0,
        right: o.map.width,
        bottom: 1
      } : o, f = t == "column" ? a ? i.cell : i.header_cell : t == "row" ? l ? i.cell : i.header_cell : i.cell;
      o.map.cellsInRect(h).forEach((d) => {
        const p = d + o.tableStart, m = s.doc.nodeAt(p);
        m && s.setNodeMarkup(p, f, m.attrs);
      }), r(s);
    }
    return !0;
  };
}
Dc("row", {
  useDeprecatedLogic: !0
});
Dc("column", {
  useDeprecatedLogic: !0
});
Dc("cell", {
  useDeprecatedLogic: !0
});
function CS(t, e) {
  if (e < 0) {
    const n = t.nodeBefore;
    if (n) return t.pos - n.nodeSize;
    for (let r = t.index(-1) - 1, i = t.before(); r >= 0; r--) {
      const o = t.node(-1).child(r), s = o.lastChild;
      if (s)
        return i - 1 - s.nodeSize;
      i -= o.nodeSize;
    }
  } else {
    if (t.index() < t.parent.childCount - 1)
      return t.pos + t.nodeAfter.nodeSize;
    const n = t.node(-1);
    for (let r = t.indexAfter(-1), i = t.after(); r < n.childCount; r++) {
      const o = n.child(r);
      if (o.childCount) return i + 1;
      i += o.nodeSize;
    }
  }
  return null;
}
function mm(t) {
  return function(e, n) {
    if (!st(e)) return !1;
    const r = CS(Cs(e), t);
    if (r == null) return !1;
    if (n) {
      const i = e.doc.resolve(r);
      n(
        e.tr.setSelection(H.between(i, rS(i))).scrollIntoView()
      );
    }
    return !0;
  };
}
function SS(t, e) {
  const n = t.selection.$anchor;
  for (let r = n.depth; r > 0; r--)
    if (n.node(r).type.spec.tableRole == "table")
      return e && e(
        t.tr.delete(n.before(r), n.after(r)).scrollIntoView()
      ), !0;
  return !1;
}
function Zi(t, e) {
  const n = t.selection;
  if (!(n instanceof se)) return !1;
  if (e) {
    const r = t.tr, i = rt(t.schema).cell.createAndFill().content;
    n.forEachCell((o, s) => {
      o.content.eq(i) || r.replace(
        r.mapping.map(s + 1),
        r.mapping.map(s + o.nodeSize - 1),
        new E(i, 0, 0)
      );
    }), r.docChanged && e(r);
  }
  return !0;
}
function MS(t) {
  if (!t.size) return null;
  let { content: e, openStart: n, openEnd: r } = t;
  for (; e.childCount == 1 && (n > 0 && r > 0 || e.child(0).type.spec.tableRole == "table"); )
    n--, r--, e = e.child(0).content;
  const i = e.child(0), o = i.type.spec.tableRole, s = i.type.schema, l = [];
  if (o == "row")
    for (let a = 0; a < e.childCount; a++) {
      let c = e.child(a).content;
      const u = a ? 0 : Math.max(0, n - 1), h = a < e.childCount - 1 ? 0 : Math.max(0, r - 1);
      (u || h) && (c = ra(
        rt(s).row,
        new E(c, u, h)
      ).content), l.push(c);
    }
  else if (o == "cell" || o == "header_cell")
    l.push(
      n || r ? ra(
        rt(s).row,
        new E(e, n, r)
      ).content : e
    );
  else
    return null;
  return NS(s, l);
}
function NS(t, e) {
  const n = [];
  for (let i = 0; i < e.length; i++) {
    const o = e[i];
    for (let s = o.childCount - 1; s >= 0; s--) {
      const { rowspan: l, colspan: a } = o.child(s).attrs;
      for (let c = i; c < i + l; c++)
        n[c] = (n[c] || 0) + a;
    }
  }
  let r = 0;
  for (let i = 0; i < n.length; i++) r = Math.max(r, n[i]);
  for (let i = 0; i < n.length; i++)
    if (i >= e.length && e.push(T.empty), n[i] < r) {
      const o = rt(t).cell.createAndFill(), s = [];
      for (let l = n[i]; l < r; l++)
        s.push(o);
      e[i] = e[i].append(T.from(s));
    }
  return { height: e.length, width: r, rows: e };
}
function ra(t, e) {
  const n = t.createAndFill();
  return new Id(n).replace(0, n.content.size, e).doc;
}
function TS({ width: t, height: e, rows: n }, r, i) {
  if (t != r) {
    const o = [], s = [];
    for (let l = 0; l < n.length; l++) {
      const a = n[l], c = [];
      for (let u = o[l] || 0, h = 0; u < r; h++) {
        let f = a.child(h % a.childCount);
        u + f.attrs.colspan > r && (f = f.type.createChecked(
          qn(
            f.attrs,
            f.attrs.colspan,
            u + f.attrs.colspan - r
          ),
          f.content
        )), c.push(f), u += f.attrs.colspan;
        for (let d = 1; d < f.attrs.rowspan; d++)
          o[l + d] = (o[l + d] || 0) + f.attrs.colspan;
      }
      s.push(T.from(c));
    }
    n = s, t = r;
  }
  if (e != i) {
    const o = [];
    for (let s = 0, l = 0; s < i; s++, l++) {
      const a = [], c = n[l % e];
      for (let u = 0; u < c.childCount; u++) {
        let h = c.child(u);
        s + h.attrs.rowspan > i && (h = h.type.create(
          {
            ...h.attrs,
            rowspan: Math.max(1, i - h.attrs.rowspan)
          },
          h.content
        )), a.push(h);
      }
      o.push(T.from(a));
    }
    n = o, e = i;
  }
  return { width: t, height: e, rows: n };
}
function IS(t, e, n, r, i, o, s) {
  const l = t.doc.type.schema, a = rt(l);
  let c, u;
  if (i > e.width)
    for (let h = 0, f = 0; h < e.height; h++) {
      const d = n.child(h);
      f += d.nodeSize;
      const p = [];
      let m;
      d.lastChild == null || d.lastChild.type == a.cell ? m = c || (c = a.cell.createAndFill()) : m = u || (u = a.header_cell.createAndFill());
      for (let y = e.width; y < i; y++) p.push(m);
      t.insert(t.mapping.slice(s).map(f - 1 + r), p);
    }
  if (o > e.height) {
    const h = [];
    for (let p = 0, m = (e.height - 1) * e.width; p < Math.max(e.width, i); p++) {
      const y = p >= e.width ? !1 : n.nodeAt(e.map[m + p]).type == a.header_cell;
      h.push(
        y ? u || (u = a.header_cell.createAndFill()) : c || (c = a.cell.createAndFill())
      );
    }
    const f = a.row.create(null, T.from(h)), d = [];
    for (let p = e.height; p < o; p++) d.push(f);
    t.insert(t.mapping.slice(s).map(r + n.nodeSize - 2), d);
  }
  return !!(c || u);
}
function Vh(t, e, n, r, i, o, s, l) {
  if (s == 0 || s == e.height) return !1;
  let a = !1;
  for (let c = i; c < o; c++) {
    const u = s * e.width + c, h = e.map[u];
    if (e.map[u - e.width] == h) {
      a = !0;
      const f = n.nodeAt(h), { top: d, left: p } = e.findCell(h);
      t.setNodeMarkup(t.mapping.slice(l).map(h + r), null, {
        ...f.attrs,
        rowspan: s - d
      }), t.insert(
        t.mapping.slice(l).map(e.positionAt(s, p, n)),
        f.type.createAndFill({
          ...f.attrs,
          rowspan: d + f.attrs.rowspan - s
        })
      ), c += f.attrs.colspan - 1;
    }
  }
  return a;
}
function Wh(t, e, n, r, i, o, s, l) {
  if (s == 0 || s == e.width) return !1;
  let a = !1;
  for (let c = i; c < o; c++) {
    const u = c * e.width + s, h = e.map[u];
    if (e.map[u - 1] == h) {
      a = !0;
      const f = n.nodeAt(h), d = e.colCount(h), p = t.mapping.slice(l).map(h + r);
      t.setNodeMarkup(
        p,
        null,
        qn(
          f.attrs,
          s - d,
          f.attrs.colspan - (s - d)
        )
      ), t.insert(
        p + f.nodeSize,
        f.type.createAndFill(
          qn(f.attrs, 0, s - d)
        )
      ), c += f.attrs.rowspan - 1;
    }
  }
  return a;
}
function Hh(t, e, n, r, i) {
  let o = n ? t.doc.nodeAt(n - 1) : t.doc;
  if (!o)
    throw new Error("No table found");
  let s = ee.get(o);
  const { top: l, left: a } = r, c = a + i.width, u = l + i.height, h = t.tr;
  let f = 0;
  function d() {
    if (o = n ? h.doc.nodeAt(n - 1) : h.doc, !o)
      throw new Error("No table found");
    s = ee.get(o), f = h.mapping.maps.length;
  }
  IS(h, s, o, n, c, u, f) && d(), Vh(h, s, o, n, a, c, l, f) && d(), Vh(h, s, o, n, a, c, u, f) && d(), Wh(h, s, o, n, l, u, a, f) && d(), Wh(h, s, o, n, l, u, c, f) && d();
  for (let p = l; p < u; p++) {
    const m = s.positionAt(p, a, o), y = s.positionAt(p, c, o);
    h.replace(
      h.mapping.slice(f).map(m + n),
      h.mapping.slice(f).map(y + n),
      new E(i.rows[p - l], 0, 0)
    );
  }
  d(), h.setSelection(
    new se(
      h.doc.resolve(n + s.positionAt(l, a, o)),
      h.doc.resolve(n + s.positionAt(u - 1, c - 1, o))
    )
  ), e(h);
}
var AS = Kd({
  ArrowLeft: eo("horiz", -1),
  ArrowRight: eo("horiz", 1),
  ArrowUp: eo("vert", -1),
  ArrowDown: eo("vert", 1),
  "Shift-ArrowLeft": to("horiz", -1),
  "Shift-ArrowRight": to("horiz", 1),
  "Shift-ArrowUp": to("vert", -1),
  "Shift-ArrowDown": to("vert", 1),
  Backspace: Zi,
  "Mod-Backspace": Zi,
  Delete: Zi,
  "Mod-Delete": Zi
});
function Lo(t, e, n) {
  return n.eq(t.selection) ? !1 : (e && e(t.tr.setSelection(n).scrollIntoView()), !0);
}
function eo(t, e) {
  return (n, r, i) => {
    if (!i) return !1;
    const o = n.selection;
    if (o instanceof se)
      return Lo(
        n,
        r,
        $.near(o.$headCell, e)
      );
    if (t != "horiz" && !o.empty) return !1;
    const s = gm(i, t, e);
    if (s == null) return !1;
    if (t == "horiz")
      return Lo(
        n,
        r,
        $.near(n.doc.resolve(o.head + e), e)
      );
    {
      const l = n.doc.resolve(s), a = hm(l, t, e);
      let c;
      return a ? c = $.near(a, 1) : e < 0 ? c = $.near(n.doc.resolve(l.before(-1)), -1) : c = $.near(n.doc.resolve(l.after(-1)), 1), Lo(n, r, c);
    }
  };
}
function to(t, e) {
  return (n, r, i) => {
    if (!i) return !1;
    const o = n.selection;
    let s;
    if (o instanceof se)
      s = o;
    else {
      const a = gm(i, t, e);
      if (a == null) return !1;
      s = new se(n.doc.resolve(a));
    }
    const l = hm(s.$headCell, t, e);
    return l ? Lo(
      n,
      r,
      new se(s.$anchorCell, l)
    ) : !1;
  };
}
function ES(t, e) {
  const n = t.state.doc, r = Wi(n.resolve(e));
  return r ? (t.dispatch(t.state.tr.setSelection(new se(r))), !0) : !1;
}
function OS(t, e, n) {
  if (!st(t.state)) return !1;
  let r = MS(n);
  const i = t.state.selection;
  if (i instanceof se) {
    r || (r = {
      width: 1,
      height: 1,
      rows: [
        T.from(
          ra(rt(t.state.schema).cell, n)
        )
      ]
    });
    const o = i.$anchorCell.node(-1), s = i.$anchorCell.start(-1), l = ee.get(o).rectBetween(
      i.$anchorCell.pos - s,
      i.$headCell.pos - s
    );
    return r = TS(r, l.right - l.left, l.bottom - l.top), Hh(t.state, t.dispatch, s, l, r), !0;
  } else if (r) {
    const o = Cs(t.state), s = o.start(-1);
    return Hh(
      t.state,
      t.dispatch,
      s,
      ee.get(o.node(-1)).findCell(o.pos - s),
      r
    ), !0;
  } else
    return !1;
}
function DS(t, e) {
  var n;
  if (e.ctrlKey || e.metaKey) return;
  const r = jh(t, e.target);
  let i;
  if (e.shiftKey && t.state.selection instanceof se)
    o(t.state.selection.$anchorCell, e), e.preventDefault();
  else if (e.shiftKey && r && (i = Wi(t.state.selection.$anchor)) != null && ((n = al(t, e)) == null ? void 0 : n.pos) != i.pos)
    o(i, e), e.preventDefault();
  else if (!r)
    return;
  function o(a, c) {
    let u = al(t, c);
    const h = rn.getState(t.state) == null;
    if (!u || !Oc(a, u))
      if (h) u = a;
      else return;
    const f = new se(a, u);
    if (h || !t.state.selection.eq(f)) {
      const d = t.state.tr.setSelection(f);
      h && d.setMeta(rn, a.pos), t.dispatch(d);
    }
  }
  function s() {
    t.root.removeEventListener("mouseup", s), t.root.removeEventListener("dragstart", s), t.root.removeEventListener("mousemove", l), rn.getState(t.state) != null && t.dispatch(t.state.tr.setMeta(rn, -1));
  }
  function l(a) {
    const c = a, u = rn.getState(t.state);
    let h;
    if (u != null)
      h = t.state.doc.resolve(u);
    else if (jh(t, c.target) != r && (h = al(t, e), !h))
      return s();
    h && o(h, c);
  }
  t.root.addEventListener("mouseup", s), t.root.addEventListener("dragstart", s), t.root.addEventListener("mousemove", l);
}
function gm(t, e, n) {
  if (!(t.state.selection instanceof H)) return null;
  const { $head: r } = t.state.selection;
  for (let i = r.depth - 1; i >= 0; i--) {
    const o = r.node(i);
    if ((n < 0 ? r.index(i) : r.indexAfter(i)) != (n < 0 ? 0 : o.childCount)) return null;
    if (o.type.spec.tableRole == "cell" || o.type.spec.tableRole == "header_cell") {
      const l = r.before(i), a = e == "vert" ? n > 0 ? "down" : "up" : n > 0 ? "right" : "left";
      return t.endOfTextblock(a) ? l : null;
    }
  }
  return null;
}
function jh(t, e) {
  for (; e && e != t.dom; e = e.parentNode)
    if (e.nodeName == "TD" || e.nodeName == "TH")
      return e;
  return null;
}
function al(t, e) {
  const n = t.posAtCoords({
    left: e.clientX,
    top: e.clientY
  });
  return n && n ? Wi(t.state.doc.resolve(n.pos)) : null;
}
var RS = class {
  constructor(t, e) {
    this.node = t, this.defaultCellMinWidth = e, this.dom = document.createElement("div"), this.dom.className = "tableWrapper", this.table = this.dom.appendChild(document.createElement("table")), this.table.style.setProperty(
      "--default-cell-min-width",
      `${e}px`
    ), this.colgroup = this.table.appendChild(document.createElement("colgroup")), ia(t, this.colgroup, this.table, e), this.contentDOM = this.table.appendChild(document.createElement("tbody"));
  }
  update(t) {
    return t.type != this.node.type ? !1 : (this.node = t, ia(
      t,
      this.colgroup,
      this.table,
      this.defaultCellMinWidth
    ), !0);
  }
  ignoreMutation(t) {
    return t.type == "attributes" && (t.target == this.table || this.colgroup.contains(t.target));
  }
};
function ia(t, e, n, r, i, o) {
  var s;
  let l = 0, a = !0, c = e.firstChild;
  const u = t.firstChild;
  if (u) {
    for (let h = 0, f = 0; h < u.childCount; h++) {
      const { colspan: d, colwidth: p } = u.child(h).attrs;
      for (let m = 0; m < d; m++, f++) {
        const y = i == f ? o : p && p[m], g = y ? y + "px" : "";
        if (l += y || r, y || (a = !1), c)
          c.style.width != g && (c.style.width = g), c = c.nextSibling;
        else {
          const N = document.createElement("col");
          N.style.width = g, e.appendChild(N);
        }
      }
    }
    for (; c; ) {
      const h = c.nextSibling;
      (s = c.parentNode) == null || s.removeChild(c), c = h;
    }
    a ? (n.style.width = l + "px", n.style.minWidth = "") : (n.style.width = "", n.style.minWidth = l + "px");
  }
}
var Je = new ge(
  "tableColumnResizing"
);
function vS({
  handleWidth: t = 5,
  cellMinWidth: e = 25,
  defaultCellMinWidth: n = 100,
  View: r = RS,
  lastColumnResizable: i = !0
} = {}) {
  const o = new Ne({
    key: Je,
    state: {
      init(s, l) {
        var a, c;
        const u = (c = (a = o.spec) == null ? void 0 : a.props) == null ? void 0 : c.nodeViews, h = rt(l.schema).table.name;
        return r && u && (u[h] = (f, d) => new r(f, n, d)), new PS(-1, !1);
      },
      apply(s, l) {
        return l.apply(s);
      }
    },
    props: {
      attributes: (s) => {
        const l = Je.getState(s);
        return l && l.activeHandle > -1 ? { class: "resize-cursor" } : {};
      },
      handleDOMEvents: {
        mousemove: (s, l) => {
          zS(s, l, t, i);
        },
        mouseleave: (s) => {
          LS(s);
        },
        mousedown: (s, l) => {
          FS(s, l, e, n);
        }
      },
      decorations: (s) => {
        const l = Je.getState(s);
        if (l && l.activeHandle > -1)
          return WS(s, l.activeHandle);
      },
      nodeViews: {}
    }
  });
  return o;
}
var PS = class Fo {
  constructor(e, n) {
    this.activeHandle = e, this.dragging = n;
  }
  apply(e) {
    const n = this, r = e.getMeta(Je);
    if (r && r.setHandle != null)
      return new Fo(r.setHandle, !1);
    if (r && r.setDragging !== void 0)
      return new Fo(n.activeHandle, r.setDragging);
    if (n.activeHandle > -1 && e.docChanged) {
      let i = e.mapping.map(n.activeHandle, -1);
      return na(e.doc.resolve(i)) || (i = -1), new Fo(i, n.dragging);
    }
    return n;
  }
};
function zS(t, e, n, r) {
  if (!t.editable) return;
  const i = Je.getState(t.state);
  if (i && !i.dragging) {
    const o = _S(e.target);
    let s = -1;
    if (o) {
      const { left: l, right: a } = o.getBoundingClientRect();
      e.clientX - l <= n ? s = qh(t, e, "left", n) : a - e.clientX <= n && (s = qh(t, e, "right", n));
    }
    if (s != i.activeHandle) {
      if (!r && s !== -1) {
        const l = t.state.doc.resolve(s), a = l.node(-1), c = ee.get(a), u = l.start(-1);
        if (c.colCount(l.pos - u) + l.nodeAfter.attrs.colspan - 1 == c.width - 1)
          return;
      }
      ym(t, s);
    }
  }
}
function LS(t) {
  if (!t.editable) return;
  const e = Je.getState(t.state);
  e && e.activeHandle > -1 && !e.dragging && ym(t, -1);
}
function FS(t, e, n, r) {
  var i;
  if (!t.editable) return !1;
  const o = (i = t.dom.ownerDocument.defaultView) != null ? i : window, s = Je.getState(t.state);
  if (!s || s.activeHandle == -1 || s.dragging)
    return !1;
  const l = t.state.doc.nodeAt(s.activeHandle), a = BS(t, s.activeHandle, l.attrs);
  t.dispatch(
    t.state.tr.setMeta(Je, {
      setDragging: { startX: e.clientX, startWidth: a }
    })
  );
  function c(h) {
    o.removeEventListener("mouseup", c), o.removeEventListener("mousemove", u);
    const f = Je.getState(t.state);
    f != null && f.dragging && ($S(
      t,
      f.activeHandle,
      Kh(f.dragging, h, n)
    ), t.dispatch(
      t.state.tr.setMeta(Je, { setDragging: null })
    ));
  }
  function u(h) {
    if (!h.which) return c(h);
    const f = Je.getState(t.state);
    if (f && f.dragging) {
      const d = Kh(f.dragging, h, n);
      Jh(
        t,
        f.activeHandle,
        d,
        r
      );
    }
  }
  return Jh(
    t,
    s.activeHandle,
    a,
    r
  ), o.addEventListener("mouseup", c), o.addEventListener("mousemove", u), e.preventDefault(), !0;
}
function BS(t, e, { colspan: n, colwidth: r }) {
  const i = r && r[r.length - 1];
  if (i) return i;
  const o = t.domAtPos(e);
  let l = o.node.childNodes[o.offset].offsetWidth, a = n;
  if (r)
    for (let c = 0; c < n; c++)
      r[c] && (l -= r[c], a--);
  return l / a;
}
function _S(t) {
  for (; t && t.nodeName != "TD" && t.nodeName != "TH"; )
    t = t.classList && t.classList.contains("ProseMirror") ? null : t.parentNode;
  return t;
}
function qh(t, e, n, r) {
  const i = n == "right" ? -r : r, o = t.posAtCoords({
    left: e.clientX + i,
    top: e.clientY
  });
  if (!o) return -1;
  const { pos: s } = o, l = Wi(t.state.doc.resolve(s));
  if (!l) return -1;
  if (n == "right") return l.pos;
  const a = ee.get(l.node(-1)), c = l.start(-1), u = a.map.indexOf(l.pos - c);
  return u % a.width == 0 ? -1 : c + a.map[u - 1];
}
function Kh(t, e, n) {
  const r = e.clientX - t.startX;
  return Math.max(n, t.startWidth + r);
}
function ym(t, e) {
  t.dispatch(
    t.state.tr.setMeta(Je, { setHandle: e })
  );
}
function $S(t, e, n) {
  const r = t.state.doc.resolve(e), i = r.node(-1), o = ee.get(i), s = r.start(-1), l = o.colCount(r.pos - s) + r.nodeAfter.attrs.colspan - 1, a = t.state.tr;
  for (let c = 0; c < o.height; c++) {
    const u = c * o.width + l;
    if (c && o.map[u] == o.map[u - o.width]) continue;
    const h = o.map[u], f = i.nodeAt(h).attrs, d = f.colspan == 1 ? 0 : l - o.colCount(h);
    if (f.colwidth && f.colwidth[d] == n) continue;
    const p = f.colwidth ? f.colwidth.slice() : VS(f.colspan);
    p[d] = n, a.setNodeMarkup(s + h, null, { ...f, colwidth: p });
  }
  a.docChanged && t.dispatch(a);
}
function Jh(t, e, n, r) {
  const i = t.state.doc.resolve(e), o = i.node(-1), s = i.start(-1), l = ee.get(o).colCount(i.pos - s) + i.nodeAfter.attrs.colspan - 1;
  let a = t.domAtPos(i.start(-1)).node;
  for (; a && a.nodeName != "TABLE"; )
    a = a.parentNode;
  a && ia(
    o,
    a.firstChild,
    a,
    r,
    l,
    n
  );
}
function VS(t) {
  return Array(t).fill(0);
}
function WS(t, e) {
  var n;
  const r = [], i = t.doc.resolve(e), o = i.node(-1);
  if (!o)
    return le.empty;
  const s = ee.get(o), l = i.start(-1), a = s.colCount(i.pos - l) + i.nodeAfter.attrs.colspan - 1;
  for (let c = 0; c < s.height; c++) {
    const u = a + c * s.width;
    if ((a == s.width - 1 || s.map[u] != s.map[u + 1]) && (c == 0 || s.map[u] != s.map[u - s.width])) {
      const h = s.map[u], f = l + h + o.nodeAt(h).nodeSize - 1, d = document.createElement("div");
      d.className = "column-resize-handle", (n = Je.getState(t)) != null && n.dragging && r.push(
        Me.node(
          l + h,
          l + h + o.nodeAt(h).nodeSize,
          {
            class: "column-resize-dragging"
          }
        )
      ), r.push(Me.widget(f, d));
    }
  }
  return le.create(t.doc, r);
}
function HS({
  allowTableNodeSelection: t = !1
} = {}) {
  return new Ne({
    key: rn,
    // This piece of state is used to remember when a mouse-drag
    // cell-selection is happening, so that it can continue even as
    // transactions (which might move its anchor cell) come in.
    state: {
      init() {
        return null;
      },
      apply(e, n) {
        const r = e.getMeta(rn);
        if (r != null) return r == -1 ? null : r;
        if (n == null || !e.docChanged) return n;
        const { deleted: i, pos: o } = e.mapping.mapResult(n);
        return i ? null : o;
      }
    },
    props: {
      decorations: lS,
      handleDOMEvents: {
        mousedown: DS
      },
      createSelectionBetween(e) {
        return rn.getState(e.state) != null ? e.state.selection : null;
      },
      handleTripleClick: ES,
      handleKeyDown: AS,
      handlePaste: OS
    },
    appendTransaction(e, n, r) {
      return uS(
        r,
        fS(r, n),
        t
      );
    }
  });
}
var es = typeof navigator < "u" ? navigator : null, Rc = es && es.userAgent || "", jS = /Edge\/(\d+)/.exec(Rc), qS = /MSIE \d/.exec(Rc), KS = /Trident\/(?:[7-9]|\d{2,})\..*rv:(\d+)/.exec(Rc), JS = !!(qS || KS || jS), US = !JS && !!es && /Apple Computer/.test(es.vendor), km = new ge("safari-ime-span"), oa = !1, GS = {
  key: km,
  props: {
    decorations: YS,
    handleDOMEvents: {
      compositionstart: () => {
        oa = !0;
      },
      compositionend: () => {
        oa = !1;
      }
    }
  }
};
function YS(t) {
  const { $from: e, $to: n, to: r } = t.selection;
  if (oa && e.sameParent(n)) {
    const i = Me.widget(r, QS, {
      ignoreSelection: !0,
      key: "safari-ime-span"
    });
    return le.create(t.doc, [i]);
  }
}
function QS(t) {
  const e = t.dom.ownerDocument.createElement("span");
  return e.className = "ProseMirror-safari-ime-span", e;
}
var XS = new Ne(US ? GS : { key: km });
function Uh(t, e) {
  const n = String(t);
  if (typeof e != "string")
    throw new TypeError("Expected character");
  let r = 0, i = n.indexOf(e);
  for (; i !== -1; )
    r++, i = n.indexOf(e, i + e.length);
  return r;
}
const ZS = bm(new RegExp("\\p{P}|\\p{S}", "u")), eM = bm(/\s/);
function bm(t) {
  return e;
  function e(n) {
    return n !== null && n > -1 && t.test(String.fromCharCode(n));
  }
}
function tM(t) {
  if (typeof t != "string")
    throw new TypeError("Expected a string");
  return t.replace(/[|\\{}()[\]^$+*?.]/g, "\\$&").replace(/-/g, "\\x2d");
}
function nM(t, e, n) {
  const i = as((n || {}).ignore || []), o = rM(e);
  let s = -1;
  for (; ++s < o.length; )
    Qf(t, "text", l);
  function l(c, u) {
    let h = -1, f;
    for (; ++h < u.length; ) {
      const d = u[h], p = f ? f.children : void 0;
      if (i(
        d,
        p ? p.indexOf(d) : void 0,
        f
      ))
        return;
      f = d;
    }
    if (f)
      return a(c, u);
  }
  function a(c, u) {
    const h = u[u.length - 1], f = o[s][0], d = o[s][1];
    let p = 0;
    const y = h.children.indexOf(c);
    let g = !1, N = [];
    f.lastIndex = 0;
    let C = f.exec(c.value);
    for (; C; ) {
      const v = C.index, I = {
        index: C.index,
        input: C.input,
        stack: [...u, c]
      };
      let w = d(...C, I);
      if (typeof w == "string" && (w = w.length > 0 ? { type: "text", value: w } : void 0), w === !1 ? f.lastIndex = v + 1 : (p !== v && N.push({
        type: "text",
        value: c.value.slice(p, v)
      }), Array.isArray(w) ? N.push(...w) : w && N.push(w), p = v + C[0].length, g = !0), !f.global)
        break;
      C = f.exec(c.value);
    }
    return g ? (p < c.value.length && N.push({ type: "text", value: c.value.slice(p) }), h.children.splice(y, 1, ...N)) : N = [c], y + N.length;
  }
}
function rM(t) {
  const e = [];
  if (!Array.isArray(t))
    throw new TypeError("Expected find and replace tuple or list of tuples");
  const n = !t[0] || Array.isArray(t[0]) ? t : [t];
  let r = -1;
  for (; ++r < n.length; ) {
    const i = n[r];
    e.push([iM(i[0]), oM(i[1])]);
  }
  return e;
}
function iM(t) {
  return typeof t == "string" ? new RegExp(tM(t), "g") : t;
}
function oM(t) {
  return typeof t == "function" ? t : function() {
    return t;
  };
}
const cl = "phrasing", ul = ["autolink", "link", "image", "label"];
function sM() {
  return {
    transforms: [dM],
    enter: {
      literalAutolink: aM,
      literalAutolinkEmail: hl,
      literalAutolinkHttp: hl,
      literalAutolinkWww: hl
    },
    exit: {
      literalAutolink: fM,
      literalAutolinkEmail: hM,
      literalAutolinkHttp: cM,
      literalAutolinkWww: uM
    }
  };
}
function lM() {
  return {
    unsafe: [
      {
        character: "@",
        before: "[+\\-.\\w]",
        after: "[\\-.\\w]",
        inConstruct: cl,
        notInConstruct: ul
      },
      {
        character: ".",
        before: "[Ww]",
        after: "[\\-.\\w]",
        inConstruct: cl,
        notInConstruct: ul
      },
      {
        character: ":",
        before: "[ps]",
        after: "\\/",
        inConstruct: cl,
        notInConstruct: ul
      }
    ]
  };
}
function aM(t) {
  this.enter({ type: "link", title: null, url: "", children: [] }, t);
}
function hl(t) {
  this.config.enter.autolinkProtocol.call(this, t);
}
function cM(t) {
  this.config.exit.autolinkProtocol.call(this, t);
}
function uM(t) {
  this.config.exit.data.call(this, t);
  const e = this.stack[this.stack.length - 1];
  e.type, e.url = "http://" + this.sliceSerialize(t);
}
function hM(t) {
  this.config.exit.autolinkEmail.call(this, t);
}
function fM(t) {
  this.exit(t);
}
function dM(t) {
  nM(
    t,
    [
      [/(https?:\/\/|www(?=\.))([-.\w]+)([^ \t\r\n]*)/gi, pM],
      [new RegExp("(?<=^|\\s|\\p{P}|\\p{S})([-.\\w+]+)@([-\\w]+(?:\\.[-\\w]+)+)", "gu"), mM]
    ],
    { ignore: ["link", "linkReference"] }
  );
}
function pM(t, e, n, r, i) {
  let o = "";
  if (!wm(i) || (/^w/i.test(e) && (n = e + n, e = "", o = "http://"), !gM(n)))
    return !1;
  const s = yM(n + r);
  if (!s[0]) return !1;
  const l = {
    type: "link",
    title: null,
    url: o + e + s[0],
    children: [{ type: "text", value: e + s[0] }]
  };
  return s[1] ? [l, { type: "text", value: s[1] }] : l;
}
function mM(t, e, n, r) {
  return (
    // Not an expected previous character.
    !wm(r, !0) || // Label ends in not allowed character.
    /[-\d_]$/.test(n) ? !1 : {
      type: "link",
      title: null,
      url: "mailto:" + e + "@" + n,
      children: [{ type: "text", value: e + "@" + n }]
    }
  );
}
function gM(t) {
  const e = t.split(".");
  return !(e.length < 2 || e[e.length - 1] && (/_/.test(e[e.length - 1]) || !/[a-zA-Z\d]/.test(e[e.length - 1])) || e[e.length - 2] && (/_/.test(e[e.length - 2]) || !/[a-zA-Z\d]/.test(e[e.length - 2])));
}
function yM(t) {
  const e = /[!"&'),.:;<>?\]}]+$/.exec(t);
  if (!e)
    return [t, void 0];
  t = t.slice(0, e.index);
  let n = e[0], r = n.indexOf(")");
  const i = Uh(t, "(");
  let o = Uh(t, ")");
  for (; r !== -1 && i > o; )
    t += n.slice(0, r + 1), n = n.slice(r + 1), r = n.indexOf(")"), o++;
  return [t, n];
}
function wm(t, e) {
  const n = t.input.charCodeAt(t.index - 1);
  return (t.index === 0 || eM(n) || ZS(n)) && // If it’s an email, the previous character should not be a slash.
  (!e || n !== 47);
}
function xm(t) {
  return t.replace(/[\t\n\r ]+/g, " ").replace(/^ | $/g, "").toLowerCase().toUpperCase();
}
Cm.peek = TM;
function kM() {
  this.buffer();
}
function bM(t) {
  this.enter({ type: "footnoteReference", identifier: "", label: "" }, t);
}
function wM() {
  this.buffer();
}
function xM(t) {
  this.enter(
    { type: "footnoteDefinition", identifier: "", label: "", children: [] },
    t
  );
}
function CM(t) {
  const e = this.resume(), n = this.stack[this.stack.length - 1];
  n.type, n.identifier = xm(
    this.sliceSerialize(t)
  ).toLowerCase(), n.label = e;
}
function SM(t) {
  this.exit(t);
}
function MM(t) {
  const e = this.resume(), n = this.stack[this.stack.length - 1];
  n.type, n.identifier = xm(
    this.sliceSerialize(t)
  ).toLowerCase(), n.label = e;
}
function NM(t) {
  this.exit(t);
}
function TM() {
  return "[";
}
function Cm(t, e, n, r) {
  const i = n.createTracker(r);
  let o = i.move("[^");
  const s = n.enter("footnoteReference"), l = n.enter("reference");
  return o += i.move(
    n.safe(n.associationId(t), { after: "]", before: o })
  ), l(), s(), o += i.move("]"), o;
}
function IM() {
  return {
    enter: {
      gfmFootnoteCallString: kM,
      gfmFootnoteCall: bM,
      gfmFootnoteDefinitionLabelString: wM,
      gfmFootnoteDefinition: xM
    },
    exit: {
      gfmFootnoteCallString: CM,
      gfmFootnoteCall: SM,
      gfmFootnoteDefinitionLabelString: MM,
      gfmFootnoteDefinition: NM
    }
  };
}
function AM(t) {
  let e = !1;
  return t && t.firstLineBlank && (e = !0), {
    handlers: { footnoteDefinition: n, footnoteReference: Cm },
    // This is on by default already.
    unsafe: [{ character: "[", inConstruct: ["label", "phrasing", "reference"] }]
  };
  function n(r, i, o, s) {
    const l = o.createTracker(s);
    let a = l.move("[^");
    const c = o.enter("footnoteDefinition"), u = o.enter("label");
    return a += l.move(
      o.safe(o.associationId(r), { before: a, after: "]" })
    ), u(), a += l.move("]:"), r.children && r.children.length > 0 && (l.shift(4), a += l.move(
      (e ? `
` : " ") + o.indentLines(
        o.containerFlow(r, l.current()),
        e ? Sm : EM
      )
    )), c(), a;
  }
}
function EM(t, e, n) {
  return e === 0 ? t : Sm(t, e, n);
}
function Sm(t, e, n) {
  return (n ? "" : "    ") + t;
}
const OM = [
  "autolink",
  "destinationLiteral",
  "destinationRaw",
  "reference",
  "titleQuote",
  "titleApostrophe"
];
Mm.peek = zM;
function DM() {
  return {
    canContainEols: ["delete"],
    enter: { strikethrough: vM },
    exit: { strikethrough: PM }
  };
}
function RM() {
  return {
    unsafe: [
      {
        character: "~",
        inConstruct: "phrasing",
        notInConstruct: OM
      }
    ],
    handlers: { delete: Mm }
  };
}
function vM(t) {
  this.enter({ type: "delete", children: [] }, t);
}
function PM(t) {
  this.exit(t);
}
function Mm(t, e, n, r) {
  const i = n.createTracker(r), o = n.enter("strikethrough");
  let s = i.move("~~");
  return s += n.containerPhrasing(t, {
    ...i.current(),
    before: s,
    after: "~"
  }), s += i.move("~~"), o(), s;
}
function zM() {
  return "~";
}
function LM(t) {
  return t.length;
}
function FM(t, e) {
  const n = e || {}, r = (n.align || []).concat(), i = n.stringLength || LM, o = [], s = [], l = [], a = [];
  let c = 0, u = -1;
  for (; ++u < t.length; ) {
    const m = [], y = [];
    let g = -1;
    for (t[u].length > c && (c = t[u].length); ++g < t[u].length; ) {
      const N = BM(t[u][g]);
      if (n.alignDelimiters !== !1) {
        const C = i(N);
        y[g] = C, (a[g] === void 0 || C > a[g]) && (a[g] = C);
      }
      m.push(N);
    }
    s[u] = m, l[u] = y;
  }
  let h = -1;
  if (typeof r == "object" && "length" in r)
    for (; ++h < c; )
      o[h] = Gh(r[h]);
  else {
    const m = Gh(r);
    for (; ++h < c; )
      o[h] = m;
  }
  h = -1;
  const f = [], d = [];
  for (; ++h < c; ) {
    const m = o[h];
    let y = "", g = "";
    m === 99 ? (y = ":", g = ":") : m === 108 ? y = ":" : m === 114 && (g = ":");
    let N = n.alignDelimiters === !1 ? 1 : Math.max(
      1,
      a[h] - y.length - g.length
    );
    const C = y + "-".repeat(N) + g;
    n.alignDelimiters !== !1 && (N = y.length + N + g.length, N > a[h] && (a[h] = N), d[h] = N), f[h] = C;
  }
  s.splice(1, 0, f), l.splice(1, 0, d), u = -1;
  const p = [];
  for (; ++u < s.length; ) {
    const m = s[u], y = l[u];
    h = -1;
    const g = [];
    for (; ++h < c; ) {
      const N = m[h] || "";
      let C = "", v = "";
      if (n.alignDelimiters !== !1) {
        const I = a[h] - (y[h] || 0), w = o[h];
        w === 114 ? C = " ".repeat(I) : w === 99 ? I % 2 ? (C = " ".repeat(I / 2 + 0.5), v = " ".repeat(I / 2 - 0.5)) : (C = " ".repeat(I / 2), v = C) : v = " ".repeat(I);
      }
      n.delimiterStart !== !1 && !h && g.push("|"), n.padding !== !1 && // Don’t add the opening space if we’re not aligning and the cell is
      // empty: there will be a closing space.
      !(n.alignDelimiters === !1 && N === "") && (n.delimiterStart !== !1 || h) && g.push(" "), n.alignDelimiters !== !1 && g.push(C), g.push(N), n.alignDelimiters !== !1 && g.push(v), n.padding !== !1 && g.push(" "), (n.delimiterEnd !== !1 || h !== c - 1) && g.push("|");
    }
    p.push(
      n.delimiterEnd === !1 ? g.join("").replace(/ +$/, "") : g.join("")
    );
  }
  return p.join(`
`);
}
function BM(t) {
  return t == null ? "" : String(t);
}
function Gh(t) {
  const e = typeof t == "string" ? t.codePointAt(0) : 0;
  return e === 67 || e === 99 ? 99 : e === 76 || e === 108 ? 108 : e === 82 || e === 114 ? 114 : 0;
}
function _M() {
  return {
    enter: {
      table: $M,
      tableData: Yh,
      tableHeader: Yh,
      tableRow: WM
    },
    exit: {
      codeText: HM,
      table: VM,
      tableData: fl,
      tableHeader: fl,
      tableRow: fl
    }
  };
}
function $M(t) {
  const e = t._align;
  this.enter(
    {
      type: "table",
      align: e.map(function(n) {
        return n === "none" ? null : n;
      }),
      children: []
    },
    t
  ), this.data.inTable = !0;
}
function VM(t) {
  this.exit(t), this.data.inTable = void 0;
}
function WM(t) {
  this.enter({ type: "tableRow", children: [] }, t);
}
function fl(t) {
  this.exit(t);
}
function Yh(t) {
  this.enter({ type: "tableCell", children: [] }, t);
}
function HM(t) {
  let e = this.resume();
  this.data.inTable && (e = e.replace(/\\([\\|])/g, jM));
  const n = this.stack[this.stack.length - 1];
  n.type, n.value = e, this.exit(t);
}
function jM(t, e) {
  return e === "|" ? e : t;
}
function qM(t) {
  const e = t || {}, n = e.tableCellPadding, r = e.tablePipeAlign, i = e.stringLength, o = n ? " " : "|";
  return {
    unsafe: [
      { character: "\r", inConstruct: "tableCell" },
      { character: `
`, inConstruct: "tableCell" },
      // A pipe, when followed by a tab or space (padding), or a dash or colon
      // (unpadded delimiter row), could result in a table.
      { atBreak: !0, character: "|", after: "[	 :-]" },
      // A pipe in a cell must be encoded.
      { character: "|", inConstruct: "tableCell" },
      // A colon must be followed by a dash, in which case it could start a
      // delimiter row.
      { atBreak: !0, character: ":", after: "-" },
      // A delimiter row can also start with a dash, when followed by more
      // dashes, a colon, or a pipe.
      // This is a stricter version than the built in check for lists, thematic
      // breaks, and setex heading underlines though:
      // <https://github.com/syntax-tree/mdast-util-to-markdown/blob/51a2038/lib/unsafe.js#L57>
      { atBreak: !0, character: "-", after: "[:|-]" }
    ],
    handlers: {
      inlineCode: f,
      table: s,
      tableCell: a,
      tableRow: l
    }
  };
  function s(d, p, m, y) {
    return c(u(d, m, y), d.align);
  }
  function l(d, p, m, y) {
    const g = h(d, m, y), N = c([g]);
    return N.slice(0, N.indexOf(`
`));
  }
  function a(d, p, m, y) {
    const g = m.enter("tableCell"), N = m.enter("phrasing"), C = m.containerPhrasing(d, {
      ...y,
      before: o,
      after: o
    });
    return N(), g(), C;
  }
  function c(d, p) {
    return FM(d, {
      align: p,
      // @ts-expect-error: `markdown-table` types should support `null`.
      alignDelimiters: r,
      // @ts-expect-error: `markdown-table` types should support `null`.
      padding: n,
      // @ts-expect-error: `markdown-table` types should support `null`.
      stringLength: i
    });
  }
  function u(d, p, m) {
    const y = d.children;
    let g = -1;
    const N = [], C = p.enter("table");
    for (; ++g < y.length; )
      N[g] = h(y[g], p, m);
    return C(), N;
  }
  function h(d, p, m) {
    const y = d.children;
    let g = -1;
    const N = [], C = p.enter("tableRow");
    for (; ++g < y.length; )
      N[g] = a(y[g], d, p, m);
    return C(), N;
  }
  function f(d, p, m) {
    let y = ga.inlineCode(d, p, m);
    return m.stack.includes("tableCell") && (y = y.replace(/\|/g, "\\$&")), y;
  }
}
function KM() {
  return {
    exit: {
      taskListCheckValueChecked: Qh,
      taskListCheckValueUnchecked: Qh,
      paragraph: UM
    }
  };
}
function JM() {
  return {
    unsafe: [{ atBreak: !0, character: "-", after: "[:|-]" }],
    handlers: { listItem: GM }
  };
}
function Qh(t) {
  const e = this.stack[this.stack.length - 2];
  e.type, e.checked = t.type === "taskListCheckValueChecked";
}
function UM(t) {
  const e = this.stack[this.stack.length - 2];
  if (e && e.type === "listItem" && typeof e.checked == "boolean") {
    const n = this.stack[this.stack.length - 1];
    n.type;
    const r = n.children[0];
    if (r && r.type === "text") {
      const i = e.children;
      let o = -1, s;
      for (; ++o < i.length; ) {
        const l = i[o];
        if (l.type === "paragraph") {
          s = l;
          break;
        }
      }
      s === n && (r.value = r.value.slice(1), r.value.length === 0 ? n.children.shift() : n.position && r.position && typeof r.position.start.offset == "number" && (r.position.start.column++, r.position.start.offset++, n.position.start = Object.assign({}, r.position.start)));
    }
  }
  this.exit(t);
}
function GM(t, e, n, r) {
  const i = t.children[0], o = typeof t.checked == "boolean" && i && i.type === "paragraph", s = "[" + (t.checked ? "x" : " ") + "] ", l = n.createTracker(r);
  o && l.move(s);
  let a = ga.listItem(t, e, n, {
    ...r,
    ...l.current()
  });
  return o && (a = a.replace(/^(?:[*+-]|\d+\.)([\r\n]| {1,3})/, c)), a;
  function c(u) {
    return u + s;
  }
}
function YM() {
  return [
    sM(),
    IM(),
    DM(),
    _M(),
    KM()
  ];
}
function QM(t) {
  return {
    extensions: [
      lM(),
      AM(t),
      RM(),
      qM(t),
      JM()
    ]
  };
}
function XM(t, e, n, r) {
  const i = t.length;
  let o = 0, s;
  if (e < 0 ? e = -e > i ? 0 : i + e : e = e > i ? i : e, n = n > 0 ? n : 0, r.length < 1e4)
    s = Array.from(r), s.unshift(e, n), t.splice(...s);
  else
    for (n && t.splice(e, n); o < r.length; )
      s = r.slice(o, o + 1e4), s.unshift(e, 0), t.splice(...s), o += 1e4, e += 1e4;
}
const Xh = {}.hasOwnProperty;
function ZM(t) {
  const e = {};
  let n = -1;
  for (; ++n < t.length; )
    eN(e, t[n]);
  return e;
}
function eN(t, e) {
  let n;
  for (n in e) {
    const i = (Xh.call(t, n) ? t[n] : void 0) || (t[n] = {}), o = e[n];
    let s;
    if (o)
      for (s in o) {
        Xh.call(i, s) || (i[s] = []);
        const l = o[s];
        tN(
          // @ts-expect-error Looks like a list.
          i[s],
          Array.isArray(l) ? l : l ? [l] : []
        );
      }
  }
}
function tN(t, e) {
  let n = -1;
  const r = [];
  for (; ++n < e.length; )
    (e[n].add === "after" ? t : r).push(e[n]);
  XM(t, 0, 0, r);
}
const Ei = Ss(/[A-Za-z]/), vc = Ss(/[\dA-Za-z]/);
function nN(t) {
  return (
    // Special whitespace codes (which have negative values), C0 and Control
    // character DEL
    t !== null && (t < 32 || t === 127)
  );
}
function Ir(t) {
  return t !== null && (t < 0 || t === 32);
}
const Nm = Ss(new RegExp("\\p{P}|\\p{S}", "u")), Oi = Ss(/\s/);
function Ss(t) {
  return e;
  function e(n) {
    return n !== null && n > -1 && t.test(String.fromCharCode(n));
  }
}
const rN = {
  tokenize: cN,
  partial: !0
}, Tm = {
  tokenize: uN,
  partial: !0
}, Im = {
  tokenize: hN,
  partial: !0
}, Am = {
  tokenize: fN,
  partial: !0
}, iN = {
  tokenize: dN,
  partial: !0
}, Em = {
  name: "wwwAutolink",
  tokenize: lN,
  previous: Dm
}, Om = {
  name: "protocolAutolink",
  tokenize: aN,
  previous: Rm
}, jt = {
  name: "emailAutolink",
  tokenize: sN,
  previous: vm
}, Ot = {};
function oN() {
  return {
    text: Ot
  };
}
let Sn = 48;
for (; Sn < 123; )
  Ot[Sn] = jt, Sn++, Sn === 58 ? Sn = 65 : Sn === 91 && (Sn = 97);
Ot[43] = jt;
Ot[45] = jt;
Ot[46] = jt;
Ot[95] = jt;
Ot[72] = [jt, Om];
Ot[104] = [jt, Om];
Ot[87] = [jt, Em];
Ot[119] = [jt, Em];
function sN(t, e, n) {
  const r = this;
  let i, o;
  return s;
  function s(h) {
    return !sa(h) || !vm.call(r, r.previous) || Pc(r.events) ? n(h) : (t.enter("literalAutolink"), t.enter("literalAutolinkEmail"), l(h));
  }
  function l(h) {
    return sa(h) ? (t.consume(h), l) : h === 64 ? (t.consume(h), a) : n(h);
  }
  function a(h) {
    return h === 46 ? t.check(iN, u, c)(h) : h === 45 || h === 95 || vc(h) ? (o = !0, t.consume(h), a) : u(h);
  }
  function c(h) {
    return t.consume(h), i = !0, a;
  }
  function u(h) {
    return o && i && Ei(r.previous) ? (t.exit("literalAutolinkEmail"), t.exit("literalAutolink"), e(h)) : n(h);
  }
}
function lN(t, e, n) {
  const r = this;
  return i;
  function i(s) {
    return s !== 87 && s !== 119 || !Dm.call(r, r.previous) || Pc(r.events) ? n(s) : (t.enter("literalAutolink"), t.enter("literalAutolinkWww"), t.check(rN, t.attempt(Tm, t.attempt(Im, o), n), n)(s));
  }
  function o(s) {
    return t.exit("literalAutolinkWww"), t.exit("literalAutolink"), e(s);
  }
}
function aN(t, e, n) {
  const r = this;
  let i = "", o = !1;
  return s;
  function s(h) {
    return (h === 72 || h === 104) && Rm.call(r, r.previous) && !Pc(r.events) ? (t.enter("literalAutolink"), t.enter("literalAutolinkHttp"), i += String.fromCodePoint(h), t.consume(h), l) : n(h);
  }
  function l(h) {
    if (Ei(h) && i.length < 5)
      return i += String.fromCodePoint(h), t.consume(h), l;
    if (h === 58) {
      const f = i.toLowerCase();
      if (f === "http" || f === "https")
        return t.consume(h), a;
    }
    return n(h);
  }
  function a(h) {
    return h === 47 ? (t.consume(h), o ? c : (o = !0, a)) : n(h);
  }
  function c(h) {
    return h === null || nN(h) || Ir(h) || Oi(h) || Nm(h) ? n(h) : t.attempt(Tm, t.attempt(Im, u), n)(h);
  }
  function u(h) {
    return t.exit("literalAutolinkHttp"), t.exit("literalAutolink"), e(h);
  }
}
function cN(t, e, n) {
  let r = 0;
  return i;
  function i(s) {
    return (s === 87 || s === 119) && r < 3 ? (r++, t.consume(s), i) : s === 46 && r === 3 ? (t.consume(s), o) : n(s);
  }
  function o(s) {
    return s === null ? n(s) : e(s);
  }
}
function uN(t, e, n) {
  let r, i, o;
  return s;
  function s(c) {
    return c === 46 || c === 95 ? t.check(Am, a, l)(c) : c === null || Ir(c) || Oi(c) || c !== 45 && Nm(c) ? a(c) : (o = !0, t.consume(c), s);
  }
  function l(c) {
    return c === 95 ? r = !0 : (i = r, r = void 0), t.consume(c), s;
  }
  function a(c) {
    return i || r || !o ? n(c) : e(c);
  }
}
function hN(t, e) {
  let n = 0, r = 0;
  return i;
  function i(s) {
    return s === 40 ? (n++, t.consume(s), i) : s === 41 && r < n ? o(s) : s === 33 || s === 34 || s === 38 || s === 39 || s === 41 || s === 42 || s === 44 || s === 46 || s === 58 || s === 59 || s === 60 || s === 63 || s === 93 || s === 95 || s === 126 ? t.check(Am, e, o)(s) : s === null || Ir(s) || Oi(s) ? e(s) : (t.consume(s), i);
  }
  function o(s) {
    return s === 41 && r++, t.consume(s), i;
  }
}
function fN(t, e, n) {
  return r;
  function r(l) {
    return l === 33 || l === 34 || l === 39 || l === 41 || l === 42 || l === 44 || l === 46 || l === 58 || l === 59 || l === 63 || l === 95 || l === 126 ? (t.consume(l), r) : l === 38 ? (t.consume(l), o) : l === 93 ? (t.consume(l), i) : (
      // `<` is an end.
      l === 60 || // So is whitespace.
      l === null || Ir(l) || Oi(l) ? e(l) : n(l)
    );
  }
  function i(l) {
    return l === null || l === 40 || l === 91 || Ir(l) || Oi(l) ? e(l) : r(l);
  }
  function o(l) {
    return Ei(l) ? s(l) : n(l);
  }
  function s(l) {
    return l === 59 ? (t.consume(l), r) : Ei(l) ? (t.consume(l), s) : n(l);
  }
}
function dN(t, e, n) {
  return r;
  function r(o) {
    return t.consume(o), i;
  }
  function i(o) {
    return vc(o) ? n(o) : e(o);
  }
}
function Dm(t) {
  return t === null || t === 40 || t === 42 || t === 95 || t === 91 || t === 93 || t === 126 || Ir(t);
}
function Rm(t) {
  return !Ei(t);
}
function vm(t) {
  return !(t === 47 || sa(t));
}
function sa(t) {
  return t === 43 || t === 45 || t === 46 || t === 95 || vc(t);
}
function Pc(t) {
  let e = t.length, n = !1;
  for (; e--; ) {
    const r = t[e][1];
    if ((r.type === "labelLink" || r.type === "labelImage") && !r._balanced) {
      n = !0;
      break;
    }
    if (r._gfmAutolinkLiteralWalkedInto) {
      n = !1;
      break;
    }
  }
  return t.length > 0 && !n && (t[t.length - 1][1]._gfmAutolinkLiteralWalkedInto = !0), n;
}
function pN(t) {
  return t !== null && t < -2;
}
function ts(t) {
  return t !== null && (t < 0 || t === 32);
}
function la(t) {
  return t === -2 || t === -1 || t === 32;
}
function zc(t, e, n, r) {
  const i = r ? r - 1 : Number.POSITIVE_INFINITY;
  let o = 0;
  return s;
  function s(a) {
    return la(a) ? (t.enter(n), l(a)) : e(a);
  }
  function l(a) {
    return la(a) && o++ < i ? (t.consume(a), l) : (t.exit(n), e(a));
  }
}
const mN = {
  partial: !0,
  tokenize: gN
};
function gN(t, e, n) {
  return r;
  function r(o) {
    return la(o) ? zc(t, i, "linePrefix")(o) : i(o);
  }
  function i(o) {
    return o === null || pN(o) ? e(o) : n(o);
  }
}
function Lc(t) {
  return t.replace(/[\t\n\r ]+/g, " ").replace(/^ | $/g, "").toLowerCase().toUpperCase();
}
const yN = {
  tokenize: NN,
  partial: !0
};
function kN() {
  return {
    document: {
      91: {
        name: "gfmFootnoteDefinition",
        tokenize: CN,
        continuation: {
          tokenize: SN
        },
        exit: MN
      }
    },
    text: {
      91: {
        name: "gfmFootnoteCall",
        tokenize: xN
      },
      93: {
        name: "gfmPotentialFootnoteCall",
        add: "after",
        tokenize: bN,
        resolveTo: wN
      }
    }
  };
}
function bN(t, e, n) {
  const r = this;
  let i = r.events.length;
  const o = r.parser.gfmFootnotes || (r.parser.gfmFootnotes = []);
  let s;
  for (; i--; ) {
    const a = r.events[i][1];
    if (a.type === "labelImage") {
      s = a;
      break;
    }
    if (a.type === "gfmFootnoteCall" || a.type === "labelLink" || a.type === "label" || a.type === "image" || a.type === "link")
      break;
  }
  return l;
  function l(a) {
    if (!s || !s._balanced)
      return n(a);
    const c = Lc(r.sliceSerialize({
      start: s.end,
      end: r.now()
    }));
    return c.codePointAt(0) !== 94 || !o.includes(c.slice(1)) ? n(a) : (t.enter("gfmFootnoteCallLabelMarker"), t.consume(a), t.exit("gfmFootnoteCallLabelMarker"), e(a));
  }
}
function wN(t, e) {
  let n = t.length;
  for (; n--; )
    if (t[n][1].type === "labelImage" && t[n][0] === "enter") {
      t[n][1];
      break;
    }
  t[n + 1][1].type = "data", t[n + 3][1].type = "gfmFootnoteCallLabelMarker";
  const r = {
    type: "gfmFootnoteCall",
    start: Object.assign({}, t[n + 3][1].start),
    end: Object.assign({}, t[t.length - 1][1].end)
  }, i = {
    type: "gfmFootnoteCallMarker",
    start: Object.assign({}, t[n + 3][1].end),
    end: Object.assign({}, t[n + 3][1].end)
  };
  i.end.column++, i.end.offset++, i.end._bufferIndex++;
  const o = {
    type: "gfmFootnoteCallString",
    start: Object.assign({}, i.end),
    end: Object.assign({}, t[t.length - 1][1].start)
  }, s = {
    type: "chunkString",
    contentType: "string",
    start: Object.assign({}, o.start),
    end: Object.assign({}, o.end)
  }, l = [
    // Take the `labelImageMarker` (now `data`, the `!`)
    t[n + 1],
    t[n + 2],
    ["enter", r, e],
    // The `[`
    t[n + 3],
    t[n + 4],
    // The `^`.
    ["enter", i, e],
    ["exit", i, e],
    // Everything in between.
    ["enter", o, e],
    ["enter", s, e],
    ["exit", s, e],
    ["exit", o, e],
    // The ending (`]`, properly parsed and labelled).
    t[t.length - 2],
    t[t.length - 1],
    ["exit", r, e]
  ];
  return t.splice(n, t.length - n + 1, ...l), t;
}
function xN(t, e, n) {
  const r = this, i = r.parser.gfmFootnotes || (r.parser.gfmFootnotes = []);
  let o = 0, s;
  return l;
  function l(h) {
    return t.enter("gfmFootnoteCall"), t.enter("gfmFootnoteCallLabelMarker"), t.consume(h), t.exit("gfmFootnoteCallLabelMarker"), a;
  }
  function a(h) {
    return h !== 94 ? n(h) : (t.enter("gfmFootnoteCallMarker"), t.consume(h), t.exit("gfmFootnoteCallMarker"), t.enter("gfmFootnoteCallString"), t.enter("chunkString").contentType = "string", c);
  }
  function c(h) {
    if (
      // Too long.
      o > 999 || // Closing brace with nothing.
      h === 93 && !s || // Space or tab is not supported by GFM for some reason.
      // `\n` and `[` not being supported makes sense.
      h === null || h === 91 || ts(h)
    )
      return n(h);
    if (h === 93) {
      t.exit("chunkString");
      const f = t.exit("gfmFootnoteCallString");
      return i.includes(Lc(r.sliceSerialize(f))) ? (t.enter("gfmFootnoteCallLabelMarker"), t.consume(h), t.exit("gfmFootnoteCallLabelMarker"), t.exit("gfmFootnoteCall"), e) : n(h);
    }
    return ts(h) || (s = !0), o++, t.consume(h), h === 92 ? u : c;
  }
  function u(h) {
    return h === 91 || h === 92 || h === 93 ? (t.consume(h), o++, c) : c(h);
  }
}
function CN(t, e, n) {
  const r = this, i = r.parser.gfmFootnotes || (r.parser.gfmFootnotes = []);
  let o, s = 0, l;
  return a;
  function a(p) {
    return t.enter("gfmFootnoteDefinition")._container = !0, t.enter("gfmFootnoteDefinitionLabel"), t.enter("gfmFootnoteDefinitionLabelMarker"), t.consume(p), t.exit("gfmFootnoteDefinitionLabelMarker"), c;
  }
  function c(p) {
    return p === 94 ? (t.enter("gfmFootnoteDefinitionMarker"), t.consume(p), t.exit("gfmFootnoteDefinitionMarker"), t.enter("gfmFootnoteDefinitionLabelString"), t.enter("chunkString").contentType = "string", u) : n(p);
  }
  function u(p) {
    if (
      // Too long.
      s > 999 || // Closing brace with nothing.
      p === 93 && !l || // Space or tab is not supported by GFM for some reason.
      // `\n` and `[` not being supported makes sense.
      p === null || p === 91 || ts(p)
    )
      return n(p);
    if (p === 93) {
      t.exit("chunkString");
      const m = t.exit("gfmFootnoteDefinitionLabelString");
      return o = Lc(r.sliceSerialize(m)), t.enter("gfmFootnoteDefinitionLabelMarker"), t.consume(p), t.exit("gfmFootnoteDefinitionLabelMarker"), t.exit("gfmFootnoteDefinitionLabel"), f;
    }
    return ts(p) || (l = !0), s++, t.consume(p), p === 92 ? h : u;
  }
  function h(p) {
    return p === 91 || p === 92 || p === 93 ? (t.consume(p), s++, u) : u(p);
  }
  function f(p) {
    return p === 58 ? (t.enter("definitionMarker"), t.consume(p), t.exit("definitionMarker"), i.includes(o) || i.push(o), zc(t, d, "gfmFootnoteDefinitionWhitespace")) : n(p);
  }
  function d(p) {
    return e(p);
  }
}
function SN(t, e, n) {
  return t.check(mN, e, t.attempt(yN, e, n));
}
function MN(t) {
  t.exit("gfmFootnoteDefinition");
}
function NN(t, e, n) {
  const r = this;
  return zc(t, i, "gfmFootnoteDefinitionIndent", 5);
  function i(o) {
    const s = r.events[r.events.length - 1];
    return s && s[1].type === "gfmFootnoteDefinitionIndent" && s[2].sliceSerialize(s[1], !0).length === 4 ? e(o) : n(o);
  }
}
function dl(t, e, n, r) {
  const i = t.length;
  let o = 0, s;
  if (e < 0 ? e = -e > i ? 0 : i + e : e = e > i ? i : e, n = n > 0 ? n : 0, r.length < 1e4)
    s = Array.from(r), s.unshift(e, n), t.splice(...s);
  else
    for (n && t.splice(e, n); o < r.length; )
      s = r.slice(o, o + 1e4), s.unshift(e, 0), t.splice(...s), o += 1e4, e += 1e4;
}
function TN(t) {
  return t !== null && (t < 0 || t === 32);
}
const IN = Pm(new RegExp("\\p{P}|\\p{S}", "u")), AN = Pm(/\s/);
function Pm(t) {
  return e;
  function e(n) {
    return n !== null && n > -1 && t.test(String.fromCharCode(n));
  }
}
function Zh(t) {
  if (t === null || TN(t) || AN(t))
    return 1;
  if (IN(t))
    return 2;
}
function EN(t, e, n) {
  const r = [];
  let i = -1;
  for (; ++i < t.length; ) {
    const o = t[i].resolveAll;
    o && !r.includes(o) && (e = o(e, n), r.push(o));
  }
  return e;
}
function ON(t) {
  let n = (t || {}).singleTilde;
  const r = {
    name: "strikethrough",
    tokenize: o,
    resolveAll: i
  };
  return n == null && (n = !0), {
    text: {
      126: r
    },
    insideSpan: {
      null: [r]
    },
    attentionMarkers: {
      null: [126]
    }
  };
  function i(s, l) {
    let a = -1;
    for (; ++a < s.length; )
      if (s[a][0] === "enter" && s[a][1].type === "strikethroughSequenceTemporary" && s[a][1]._close) {
        let c = a;
        for (; c--; )
          if (s[c][0] === "exit" && s[c][1].type === "strikethroughSequenceTemporary" && s[c][1]._open && // If the sizes are the same:
          s[a][1].end.offset - s[a][1].start.offset === s[c][1].end.offset - s[c][1].start.offset) {
            s[a][1].type = "strikethroughSequence", s[c][1].type = "strikethroughSequence";
            const u = {
              type: "strikethrough",
              start: Object.assign({}, s[c][1].start),
              end: Object.assign({}, s[a][1].end)
            }, h = {
              type: "strikethroughText",
              start: Object.assign({}, s[c][1].end),
              end: Object.assign({}, s[a][1].start)
            }, f = [["enter", u, l], ["enter", s[c][1], l], ["exit", s[c][1], l], ["enter", h, l]], d = l.parser.constructs.insideSpan.null;
            d && dl(f, f.length, 0, EN(d, s.slice(c + 1, a), l)), dl(f, f.length, 0, [["exit", h, l], ["enter", s[a][1], l], ["exit", s[a][1], l], ["exit", u, l]]), dl(s, c - 1, a - c + 3, f), a = c + f.length - 2;
            break;
          }
      }
    for (a = -1; ++a < s.length; )
      s[a][1].type === "strikethroughSequenceTemporary" && (s[a][1].type = "data");
    return s;
  }
  function o(s, l, a) {
    const c = this.previous, u = this.events;
    let h = 0;
    return f;
    function f(p) {
      return c === 126 && u[u.length - 1][1].type !== "characterEscape" ? a(p) : (s.enter("strikethroughSequenceTemporary"), d(p));
    }
    function d(p) {
      const m = Zh(c);
      if (p === 126)
        return h > 1 ? a(p) : (s.consume(p), h++, d);
      if (h < 2 && !n) return a(p);
      const y = s.exit("strikethroughSequenceTemporary"), g = Zh(p);
      return y._open = !g || g === 2 && !!m, y._close = !m || m === 2 && !!g, l(p);
    }
  }
}
function no(t) {
  return t !== null && t < -2;
}
function ef(t) {
  return t !== null && (t < 0 || t === 32);
}
function On(t) {
  return t === -2 || t === -1 || t === 32;
}
function Br(t, e, n, r) {
  const i = r ? r - 1 : Number.POSITIVE_INFINITY;
  let o = 0;
  return s;
  function s(a) {
    return On(a) ? (t.enter(n), l(a)) : e(a);
  }
  function l(a) {
    return On(a) && o++ < i ? (t.consume(a), l) : (t.exit(n), e(a));
  }
}
class DN {
  /**
   * Create a new edit map.
   */
  constructor() {
    this.map = [];
  }
  /**
   * Create an edit: a remove and/or add at a certain place.
   *
   * @param {number} index
   * @param {number} remove
   * @param {Array<Event>} add
   * @returns {undefined}
   */
  add(e, n, r) {
    RN(this, e, n, r);
  }
  // To do: add this when moving to `micromark`.
  // /**
  //  * Create an edit: but insert `add` before existing additions.
  //  *
  //  * @param {number} index
  //  * @param {number} remove
  //  * @param {Array<Event>} add
  //  * @returns {undefined}
  //  */
  // addBefore(index, remove, add) {
  //   addImplementation(this, index, remove, add, true)
  // }
  /**
   * Done, change the events.
   *
   * @param {Array<Event>} events
   * @returns {undefined}
   */
  consume(e) {
    if (this.map.sort(function(o, s) {
      return o[0] - s[0];
    }), this.map.length === 0)
      return;
    let n = this.map.length;
    const r = [];
    for (; n > 0; )
      n -= 1, r.push(e.slice(this.map[n][0] + this.map[n][1]), this.map[n][2]), e.length = this.map[n][0];
    r.push(e.slice()), e.length = 0;
    let i = r.pop();
    for (; i; ) {
      for (const o of i)
        e.push(o);
      i = r.pop();
    }
    this.map.length = 0;
  }
}
function RN(t, e, n, r) {
  let i = 0;
  if (!(n === 0 && r.length === 0)) {
    for (; i < t.map.length; ) {
      if (t.map[i][0] === e) {
        t.map[i][1] += n, t.map[i][2].push(...r);
        return;
      }
      i += 1;
    }
    t.map.push([e, n, r]);
  }
}
function vN(t, e) {
  let n = !1;
  const r = [];
  for (; e < t.length; ) {
    const i = t[e];
    if (n) {
      if (i[0] === "enter")
        i[1].type === "tableContent" && r.push(t[e + 1][1].type === "tableDelimiterMarker" ? "left" : "none");
      else if (i[1].type === "tableContent") {
        if (t[e - 1][1].type === "tableDelimiterMarker") {
          const o = r.length - 1;
          r[o] = r[o] === "left" ? "center" : "right";
        }
      } else if (i[1].type === "tableDelimiterRow")
        break;
    } else i[0] === "enter" && i[1].type === "tableDelimiterRow" && (n = !0);
    e += 1;
  }
  return r;
}
function PN() {
  return {
    flow: {
      null: {
        name: "table",
        tokenize: zN,
        resolveAll: LN
      }
    }
  };
}
function zN(t, e, n) {
  const r = this;
  let i = 0, o = 0, s;
  return l;
  function l(x) {
    let R = r.events.length - 1;
    for (; R > -1; ) {
      const te = r.events[R][1].type;
      if (te === "lineEnding" || // Note: markdown-rs uses `whitespace` instead of `linePrefix`
      te === "linePrefix") R--;
      else break;
    }
    const D = R > -1 ? r.events[R][1].type : null, J = D === "tableHead" || D === "tableRow" ? w : a;
    return J === w && r.parser.lazy[r.now().line] ? n(x) : J(x);
  }
  function a(x) {
    return t.enter("tableHead"), t.enter("tableRow"), c(x);
  }
  function c(x) {
    return x === 124 || (s = !0, o += 1), u(x);
  }
  function u(x) {
    return x === null ? n(x) : no(x) ? o > 1 ? (o = 0, r.interrupt = !0, t.exit("tableRow"), t.enter("lineEnding"), t.consume(x), t.exit("lineEnding"), d) : n(x) : On(x) ? Br(t, u, "whitespace")(x) : (o += 1, s && (s = !1, i += 1), x === 124 ? (t.enter("tableCellDivider"), t.consume(x), t.exit("tableCellDivider"), s = !0, u) : (t.enter("data"), h(x)));
  }
  function h(x) {
    return x === null || x === 124 || ef(x) ? (t.exit("data"), u(x)) : (t.consume(x), x === 92 ? f : h);
  }
  function f(x) {
    return x === 92 || x === 124 ? (t.consume(x), h) : h(x);
  }
  function d(x) {
    return r.interrupt = !1, r.parser.lazy[r.now().line] ? n(x) : (t.enter("tableDelimiterRow"), s = !1, On(x) ? Br(t, p, "linePrefix", r.parser.constructs.disable.null.includes("codeIndented") ? void 0 : 4)(x) : p(x));
  }
  function p(x) {
    return x === 45 || x === 58 ? y(x) : x === 124 ? (s = !0, t.enter("tableCellDivider"), t.consume(x), t.exit("tableCellDivider"), m) : I(x);
  }
  function m(x) {
    return On(x) ? Br(t, y, "whitespace")(x) : y(x);
  }
  function y(x) {
    return x === 58 ? (o += 1, s = !0, t.enter("tableDelimiterMarker"), t.consume(x), t.exit("tableDelimiterMarker"), g) : x === 45 ? (o += 1, g(x)) : x === null || no(x) ? v(x) : I(x);
  }
  function g(x) {
    return x === 45 ? (t.enter("tableDelimiterFiller"), N(x)) : I(x);
  }
  function N(x) {
    return x === 45 ? (t.consume(x), N) : x === 58 ? (s = !0, t.exit("tableDelimiterFiller"), t.enter("tableDelimiterMarker"), t.consume(x), t.exit("tableDelimiterMarker"), C) : (t.exit("tableDelimiterFiller"), C(x));
  }
  function C(x) {
    return On(x) ? Br(t, v, "whitespace")(x) : v(x);
  }
  function v(x) {
    return x === 124 ? p(x) : x === null || no(x) ? !s || i !== o ? I(x) : (t.exit("tableDelimiterRow"), t.exit("tableHead"), e(x)) : I(x);
  }
  function I(x) {
    return n(x);
  }
  function w(x) {
    return t.enter("tableRow"), z(x);
  }
  function z(x) {
    return x === 124 ? (t.enter("tableCellDivider"), t.consume(x), t.exit("tableCellDivider"), z) : x === null || no(x) ? (t.exit("tableRow"), e(x)) : On(x) ? Br(t, z, "whitespace")(x) : (t.enter("data"), W(x));
  }
  function W(x) {
    return x === null || x === 124 || ef(x) ? (t.exit("data"), z(x)) : (t.consume(x), x === 92 ? O : W);
  }
  function O(x) {
    return x === 92 || x === 124 ? (t.consume(x), W) : W(x);
  }
}
function LN(t, e) {
  let n = -1, r = !0, i = 0, o = [0, 0, 0, 0], s = [0, 0, 0, 0], l = !1, a = 0, c, u, h;
  const f = new DN();
  for (; ++n < t.length; ) {
    const d = t[n], p = d[1];
    d[0] === "enter" ? p.type === "tableHead" ? (l = !1, a !== 0 && (tf(f, e, a, c, u), u = void 0, a = 0), c = {
      type: "table",
      start: Object.assign({}, p.start),
      // Note: correct end is set later.
      end: Object.assign({}, p.end)
    }, f.add(n, 0, [["enter", c, e]])) : p.type === "tableRow" || p.type === "tableDelimiterRow" ? (r = !0, h = void 0, o = [0, 0, 0, 0], s = [0, n + 1, 0, 0], l && (l = !1, u = {
      type: "tableBody",
      start: Object.assign({}, p.start),
      // Note: correct end is set later.
      end: Object.assign({}, p.end)
    }, f.add(n, 0, [["enter", u, e]])), i = p.type === "tableDelimiterRow" ? 2 : u ? 3 : 1) : i && (p.type === "data" || p.type === "tableDelimiterMarker" || p.type === "tableDelimiterFiller") ? (r = !1, s[2] === 0 && (o[1] !== 0 && (s[0] = s[1], h = ro(f, e, o, i, void 0, h), o = [0, 0, 0, 0]), s[2] = n)) : p.type === "tableCellDivider" && (r ? r = !1 : (o[1] !== 0 && (s[0] = s[1], h = ro(f, e, o, i, void 0, h)), o = s, s = [o[1], n, 0, 0])) : p.type === "tableHead" ? (l = !0, a = n) : p.type === "tableRow" || p.type === "tableDelimiterRow" ? (a = n, o[1] !== 0 ? (s[0] = s[1], h = ro(f, e, o, i, n, h)) : s[1] !== 0 && (h = ro(f, e, s, i, n, h)), i = 0) : i && (p.type === "data" || p.type === "tableDelimiterMarker" || p.type === "tableDelimiterFiller") && (s[3] = n);
  }
  for (a !== 0 && tf(f, e, a, c, u), f.consume(e.events), n = -1; ++n < e.events.length; ) {
    const d = e.events[n];
    d[0] === "enter" && d[1].type === "table" && (d[1]._align = vN(e.events, n));
  }
  return t;
}
function ro(t, e, n, r, i, o) {
  const s = r === 1 ? "tableHeader" : r === 2 ? "tableDelimiter" : "tableData", l = "tableContent";
  n[0] !== 0 && (o.end = Object.assign({}, cr(e.events, n[0])), t.add(n[0], 0, [["exit", o, e]]));
  const a = cr(e.events, n[1]);
  if (o = {
    type: s,
    start: Object.assign({}, a),
    // Note: correct end is set later.
    end: Object.assign({}, a)
  }, t.add(n[1], 0, [["enter", o, e]]), n[2] !== 0) {
    const c = cr(e.events, n[2]), u = cr(e.events, n[3]), h = {
      type: l,
      start: Object.assign({}, c),
      end: Object.assign({}, u)
    };
    if (t.add(n[2], 0, [["enter", h, e]]), r !== 2) {
      const f = e.events[n[2]], d = e.events[n[3]];
      if (f[1].end = Object.assign({}, d[1].end), f[1].type = "chunkText", f[1].contentType = "text", n[3] > n[2] + 1) {
        const p = n[2] + 1, m = n[3] - n[2] - 1;
        t.add(p, m, []);
      }
    }
    t.add(n[3] + 1, 0, [["exit", h, e]]);
  }
  return i !== void 0 && (o.end = Object.assign({}, cr(e.events, i)), t.add(i, 0, [["exit", o, e]]), o = void 0), o;
}
function tf(t, e, n, r, i) {
  const o = [], s = cr(e.events, n);
  i && (i.end = Object.assign({}, s), o.push(["exit", i, e])), r.end = Object.assign({}, s), o.push(["exit", r, e]), t.add(n + 1, 0, o);
}
function cr(t, e) {
  const n = t[e], r = n[0] === "enter" ? "start" : "end";
  return n[1][r];
}
function FN(t) {
  return t !== null && t < -2;
}
function BN(t) {
  return t !== null && (t < 0 || t === 32);
}
function aa(t) {
  return t === -2 || t === -1 || t === 32;
}
function _N(t, e, n, r) {
  const i = Number.POSITIVE_INFINITY;
  let o = 0;
  return s;
  function s(a) {
    return aa(a) ? (t.enter(n), l(a)) : e(a);
  }
  function l(a) {
    return aa(a) && o++ < i ? (t.consume(a), l) : (t.exit(n), e(a));
  }
}
const $N = {
  name: "tasklistCheck",
  tokenize: WN
};
function VN() {
  return {
    text: {
      91: $N
    }
  };
}
function WN(t, e, n) {
  const r = this;
  return i;
  function i(a) {
    return (
      // Exit if there’s stuff before.
      r.previous !== null || // Exit if not in the first content that is the first child of a list
      // item.
      !r._gfmTasklistFirstContentOfListItem ? n(a) : (t.enter("taskListCheck"), t.enter("taskListCheckMarker"), t.consume(a), t.exit("taskListCheckMarker"), o)
    );
  }
  function o(a) {
    return BN(a) ? (t.enter("taskListCheckValueUnchecked"), t.consume(a), t.exit("taskListCheckValueUnchecked"), s) : a === 88 || a === 120 ? (t.enter("taskListCheckValueChecked"), t.consume(a), t.exit("taskListCheckValueChecked"), s) : n(a);
  }
  function s(a) {
    return a === 93 ? (t.enter("taskListCheckMarker"), t.consume(a), t.exit("taskListCheckMarker"), t.exit("taskListCheck"), l) : n(a);
  }
  function l(a) {
    return FN(a) ? e(a) : aa(a) ? t.check({
      tokenize: HN
    }, e, n)(a) : n(a);
  }
}
function HN(t, e, n) {
  return _N(t, r, "whitespace");
  function r(i) {
    return i === null ? n(i) : e(i);
  }
}
function jN(t) {
  return ZM([
    oN(),
    kN(),
    ON(t),
    PN(),
    VN()
  ]);
}
const qN = {};
function KN(t) {
  const e = (
    /** @type {Processor<Root>} */
    this
  ), n = t || qN, r = e.data(), i = r.micromarkExtensions || (r.micromarkExtensions = []), o = r.fromMarkdownExtensions || (r.fromMarkdownExtensions = []), s = r.toMarkdownExtensions || (r.toMarkdownExtensions = []);
  i.push(jN(n)), o.push(YM()), s.push(QM(n));
}
function B(t, e) {
  return Object.assign(t, {
    meta: {
      package: "@milkdown/preset-gfm",
      ...e
    }
  }), t;
}
const Fc = Fi("strike_through");
B(Fc, {
  displayName: "Attr<strikethrough>",
  group: "Strikethrough"
});
const Hi = Li("strike_through", (t) => ({
  parseDOM: [
    { tag: "del" },
    {
      style: "text-decoration",
      getAttrs: (e) => e === "line-through"
    }
  ],
  toDOM: (e) => ["del", t.get(Fc.key)(e)],
  parseMarkdown: {
    match: (e) => e.type === "delete",
    runner: (e, n, r) => {
      e.openMark(r), e.next(n.children), e.closeMark(r);
    }
  },
  toMarkdown: {
    match: (e) => e.type.name === "strike_through",
    runner: (e, n) => {
      e.withMark(n, "delete");
    }
  }
}));
B(Hi.mark, {
  displayName: "MarkSchema<strikethrough>",
  group: "Strikethrough"
});
B(Hi.ctx, {
  displayName: "MarkSchemaCtx<strikethrough>",
  group: "Strikethrough"
});
const Bc = j(
  "ToggleStrikeThrough",
  (t) => () => ms(Hi.type(t))
);
B(Bc, {
  displayName: "Command<ToggleStrikethrough>",
  group: "Strikethrough"
});
const zm = Ve((t) => Ri(/~([^~]+)~$/, Hi.type(t)));
B(zm, {
  displayName: "InputRule<strikethrough>",
  group: "Strikethrough"
});
const _c = We("strikeThroughKeymap", {
  ToggleStrikethrough: {
    shortcuts: "Mod-Alt-x",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Bc.key);
    }
  }
});
B(_c.ctx, {
  displayName: "KeymapCtx<strikethrough>",
  group: "Strikethrough"
});
B(_c.shortcuts, {
  displayName: "Keymap<strikethrough>",
  group: "Strikethrough"
});
const ji = tS({
  tableGroup: "block",
  cellContent: "paragraph",
  cellAttributes: {
    alignment: {
      default: "left",
      getFromDOM: (t) => t.style.textAlign || "left",
      setDOMAttr: (t, e) => {
        e.style = `text-align: ${t || "left"}`;
      }
    }
  }
}), Rr = ye("table", () => ({
  ...ji.table,
  content: "table_header_row table_row+",
  disableDropCursor: !0,
  parseMarkdown: {
    match: (t) => t.type === "table",
    runner: (t, e, n) => {
      const r = e.align, i = e.children.map((o, s) => ({
        ...o,
        align: r,
        isHeader: s === 0
      }));
      t.openNode(n), t.next(i), t.closeNode();
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "table",
    runner: (t, e) => {
      var n;
      const r = (n = e.content.firstChild) == null ? void 0 : n.content;
      if (!r) return;
      const i = [];
      r.forEach((o) => {
        i.push(o.attrs.alignment);
      }), t.openNode("table", void 0, { align: i }), t.next(e.content), t.closeNode();
    }
  }
}));
B(Rr.node, {
  displayName: "NodeSchema<table>",
  group: "Table"
});
B(Rr.ctx, {
  displayName: "NodeSchemaCtx<table>",
  group: "Table"
});
const Ms = ye("table_header_row", () => ({
  ...ji.table_row,
  disableDropCursor: !0,
  content: "(table_header)*",
  parseDOM: [{ tag: "tr[data-is-header]" }],
  toDOM() {
    return ["tr", { "data-is-header": !0 }, 0];
  },
  parseMarkdown: {
    match: (t) => !!(t.type === "tableRow" && t.isHeader),
    runner: (t, e, n) => {
      const r = e.align, i = e.children.map((o, s) => ({
        ...o,
        align: r[s],
        isHeader: e.isHeader
      }));
      t.openNode(n), t.next(i), t.closeNode();
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "table_header_row",
    runner: (t, e) => {
      t.openNode("tableRow", void 0, { isHeader: !0 }), t.next(e.content), t.closeNode();
    }
  }
}));
B(Ms.node, {
  displayName: "NodeSchema<tableHeaderRow>",
  group: "Table"
});
B(Ms.ctx, {
  displayName: "NodeSchemaCtx<tableHeaderRow>",
  group: "Table"
});
const qi = ye("table_row", () => ({
  ...ji.table_row,
  disableDropCursor: !0,
  content: "(table_cell)*",
  parseMarkdown: {
    match: (t) => t.type === "tableRow",
    runner: (t, e, n) => {
      const r = e.align, i = e.children.map((o, s) => ({
        ...o,
        align: r[s]
      }));
      t.openNode(n), t.next(i), t.closeNode();
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "table_row",
    runner: (t, e) => {
      t.openNode("tableRow"), t.next(e.content), t.closeNode();
    }
  }
}));
B(qi.node, {
  displayName: "NodeSchema<tableRow>",
  group: "Table"
});
B(qi.ctx, {
  displayName: "NodeSchemaCtx<tableRow>",
  group: "Table"
});
const Ki = ye("table_cell", () => ({
  ...ji.table_cell,
  disableDropCursor: !0,
  parseMarkdown: {
    match: (t) => t.type === "tableCell" && !t.isHeader,
    runner: (t, e, n) => {
      const r = e.align;
      t.openNode(n, { alignment: r }).openNode(t.schema.nodes.paragraph).next(e.children).closeNode().closeNode();
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "table_cell",
    runner: (t, e) => {
      t.openNode("tableCell").next(e.content).closeNode();
    }
  }
}));
B(Ki.node, {
  displayName: "NodeSchema<tableCell>",
  group: "Table"
});
B(Ki.ctx, {
  displayName: "NodeSchemaCtx<tableCell>",
  group: "Table"
});
const Ns = ye("table_header", () => ({
  ...ji.table_header,
  disableDropCursor: !0,
  parseMarkdown: {
    match: (t) => t.type === "tableCell" && !!t.isHeader,
    runner: (t, e, n) => {
      const r = e.align;
      t.openNode(n, { alignment: r }), t.openNode(t.schema.nodes.paragraph), t.next(e.children), t.closeNode(), t.closeNode();
    }
  },
  toMarkdown: {
    match: (t) => t.type.name === "table_header",
    runner: (t, e) => {
      t.openNode("tableCell"), t.next(e.content), t.closeNode();
    }
  }
}));
B(Ns.node, {
  displayName: "NodeSchema<tableHeader>",
  group: "Table"
});
B(Ns.ctx, {
  displayName: "NodeSchemaCtx<tableHeader>",
  group: "Table"
});
function Lm(t, e = 3, n = 3) {
  const r = Array(n).fill(0).map(() => Ki.type(t).createAndFill()), i = Array(n).fill(0).map(() => Ns.type(t).createAndFill()), o = Array(e).fill(0).map(
    (s, l) => l === 0 ? Ms.type(t).create(null, i) : qi.type(t).create(null, r)
  );
  return Rr.type(t).create(null, o);
}
function Ji(t) {
  return Ma(
    (e) => e.type.spec.tableRole === "table"
  )(t);
}
function ur(t, e) {
  const n = Ji(e.$from);
  if (!n) return;
  const r = ee.get(n.node);
  if (!(t < 0 || t >= r.width))
    return r.cellsInRect({
      left: t,
      right: t + 1,
      top: 0,
      bottom: r.height
    }).map((i) => {
      const o = n.node.nodeAt(i);
      if (!o) return;
      const s = i + n.start;
      return {
        pos: s,
        start: s + 1,
        node: o
      };
    }).filter((i) => i != null);
}
function hr(t, e) {
  const n = Ji(e.$from);
  if (!n) return;
  const r = ee.get(n.node);
  if (!(t < 0 || t >= r.height))
    return r.cellsInRect({
      left: 0,
      right: r.width,
      top: t,
      bottom: t + 1
    }).map((i) => {
      const o = n.node.nodeAt(i);
      if (!o) return;
      const s = i + n.start;
      return {
        pos: s,
        start: s + 1,
        node: o
      };
    }).filter((i) => i != null);
}
function JN(t) {
  const e = Ji(t.$from);
  if (!e) return;
  const n = ee.get(e.node);
  return n.cellsInRect({
    left: 0,
    right: n.width,
    top: 0,
    bottom: n.height
  }).map((i) => {
    const o = e.node.nodeAt(i), s = i + e.start;
    return { pos: s, start: s + 1, node: o };
  });
}
function UN(t) {
  const e = JN(t.selection);
  if (e && e[0]) {
    const n = t.doc.resolve(e[0].pos), r = e[e.length - 1];
    if (r) {
      const i = t.doc.resolve(r.pos);
      return ps(t.setSelection(new se(i, n)));
    }
  }
  return t;
}
function Fm(t, e, { map: n, tableStart: r, table: i }, o) {
  const s = Array(o).fill(0).reduce((a, c, u) => a + i.child(u).nodeSize, r), l = Array(n.width).fill(0).map((a, c) => {
    const u = i.nodeAt(n.map[c]);
    return Ki.type(t).createAndFill({ alignment: u == null ? void 0 : u.attrs.alignment });
  });
  return e.insert(s, qi.type(t).create(null, l)), e;
}
function Bm(t) {
  return (e, n) => (r) => {
    n = n ?? r.selection.from;
    const i = r.doc.resolve(n), o = Ma(
      (a) => a.type.name === "table"
    )(i), s = o ? {
      node: o.node,
      from: o.start
    } : void 0, l = t === "row";
    if (s) {
      const a = ee.get(s.node);
      if (e >= 0 && e < (l ? a.height : a.width)) {
        const c = a.positionAt(
          l ? e : a.height - 1,
          l ? a.width - 1 : e,
          s.node
        ), u = r.doc.resolve(s.from + c), h = l ? se.rowSelection : se.colSelection, f = a.positionAt(
          l ? e : 0,
          l ? 0 : e,
          s.node
        ), d = r.doc.resolve(s.from + f);
        return ps(
          r.setSelection(
            h(u, d)
          )
        );
      }
    }
    return r;
  };
}
const GN = Bm("row"), YN = Bm("col");
function nf(t) {
  return t[0].map((e, n) => t.map((r) => r[n]));
}
function _m(t, e) {
  const n = [], r = ee.get(t);
  for (let o = 0; o < r.height; o++) {
    const s = t.child(o), l = [];
    for (let a = 0; a < r.width; a++) {
      if (!e[o][a]) continue;
      const c = r.map[o * r.width + a], u = e[o][a], f = t.nodeAt(c).type.createChecked(
        Object.assign({}, u.attrs),
        u.content,
        u.marks
      );
      l.push(f);
    }
    n.push(s.type.createChecked(s.attrs, l, s.marks));
  }
  return t.type.createChecked(
    t.attrs,
    n,
    t.marks
  );
}
function $m(t) {
  const e = ee.get(t), n = [];
  for (let r = 0; r < e.height; r++) {
    const i = [], o = {};
    for (let s = 0; s < e.width; s++) {
      const l = e.map[r * e.width + s], a = t.nodeAt(l), c = e.findCell(l);
      if (o[l] || c.top !== r) {
        i.push(null);
        continue;
      }
      o[l] = !0, i.push(a);
    }
    n.push(i);
  }
  return n;
}
function Vm(t, e, n, r) {
  const i = e[0] > n[0] ? -1 : 1, o = t.splice(e[0], e.length), s = o.length % 2 === 0 ? 1 : 0;
  let l;
  return l = i === -1 ? n[0] : n[n.length - 1] - s, t.splice(l, 0, ...o), t;
}
function QN(t, e, n, r) {
  let i = nf($m(t.node));
  return i = Vm(i, e, n), i = nf(i), _m(t.node, i);
}
function XN(t, e, n, r) {
  let i = $m(t.node);
  return i = Vm(i, e, n), _m(t.node, i);
}
function rf(t, e) {
  let n = t, r = t;
  for (let u = t; u >= 0; u--) {
    const h = ur(u, e.selection);
    h && h.forEach((f) => {
      const d = f.node.attrs.colspan + u - 1;
      d >= n && (n = u), d > r && (r = d);
    });
  }
  for (let u = t; u <= r; u++) {
    const h = ur(u, e.selection);
    h && h.forEach((f) => {
      const d = f.node.attrs.colspan + u - 1;
      f.node.attrs.colspan > 1 && d > r && (r = d);
    });
  }
  const i = [];
  for (let u = n; u <= r; u++) {
    const h = ur(u, e.selection);
    h && h.length && i.push(u);
  }
  n = i[0], r = i[i.length - 1];
  const o = ur(n, e.selection), s = hr(0, e.selection), l = e.doc.resolve(
    o[o.length - 1].pos
  );
  let a;
  for (let u = r; u >= n; u--) {
    const h = ur(u, e.selection);
    if (h && h.length) {
      for (let f = s.length - 1; f >= 0; f--)
        if (s[f].pos === h[0].pos) {
          a = h[0];
          break;
        }
      if (a) break;
    }
  }
  const c = e.doc.resolve(a.pos);
  return { $anchor: l, $head: c, indexes: i };
}
function of(t, e) {
  let n = t, r = t;
  for (let u = t; u >= 0; u--)
    hr(u, e.selection).forEach((f) => {
      const d = f.node.attrs.rowspan + u - 1;
      d >= n && (n = u), d > r && (r = d);
    });
  for (let u = t; u <= r; u++)
    hr(u, e.selection).forEach((f) => {
      const d = f.node.attrs.rowspan + u - 1;
      f.node.attrs.rowspan > 1 && d > r && (r = d);
    });
  const i = [];
  for (let u = n; u <= r; u++) {
    const h = hr(u, e.selection);
    h && h.length && i.push(u);
  }
  n = i[0], r = i[i.length - 1];
  const o = hr(n, e.selection), s = ur(0, e.selection), l = e.doc.resolve(
    o[o.length - 1].pos
  );
  let a;
  for (let u = r; u >= n; u--) {
    const h = hr(u, e.selection);
    if (h && h.length) {
      for (let f = s.length - 1; f >= 0; f--)
        if (s[f].pos === h[0].pos) {
          a = h[0];
          break;
        }
      if (a) break;
    }
  }
  const c = e.doc.resolve(a.pos);
  return { $anchor: l, $head: c, indexes: i };
}
function ZN(t) {
  const { tr: e, origin: n, target: r, select: i = !0, pos: o } = t, s = o != null ? e.doc.resolve(o) : e.selection.$from, l = Ji(s);
  if (!l) return e;
  const { indexes: a } = rf(n, e), { indexes: c } = rf(r, e);
  if (a.includes(r)) return e;
  const u = QN(
    l,
    a,
    c
  ), h = ps(e).replaceWith(
    l.pos,
    l.pos + l.node.nodeSize,
    u
  );
  if (!i) return h;
  const f = ee.get(u), d = l.start, p = r, m = f.positionAt(f.height - 1, p, u), y = h.doc.resolve(d + m), g = se.colSelection, N = f.positionAt(0, p, u), C = h.doc.resolve(d + N);
  return h.setSelection(g(y, C));
}
function eT(t) {
  const { tr: e, origin: n, target: r, select: i = !0, pos: o } = t, s = o != null ? e.doc.resolve(o) : e.selection.$from, l = Ji(s);
  if (!l) return e;
  const { indexes: a } = of(n, e), { indexes: c } = of(r, e);
  if (a.includes(r)) return e;
  const u = XN(l, a, c), h = ps(e).replaceWith(
    l.pos,
    l.pos + l.node.nodeSize,
    u
  );
  if (!i) return h;
  const f = ee.get(u), d = l.start, p = r, m = f.positionAt(p, f.width - 1, u), y = h.doc.resolve(d + m), g = se.rowSelection, N = f.positionAt(p, 0, u), C = h.doc.resolve(d + N);
  return h.setSelection(g(y, C));
}
const $c = j(
  "GoToPrevTableCell",
  () => () => mm(-1)
);
B($c, {
  displayName: "Command<goToPrevTableCellCommand>",
  group: "Table"
});
const Vc = j(
  "GoToNextTableCell",
  () => () => mm(1)
);
B(Vc, {
  displayName: "Command<goToNextTableCellCommand>",
  group: "Table"
});
const Wc = j(
  "ExitTable",
  (t) => () => (e, n) => {
    if (!st(e)) return !1;
    const { $head: r } = e.selection, i = iw(r, Rr.type(t));
    if (!i) return !1;
    const { to: o } = i, s = e.tr.replaceWith(
      o,
      o,
      bn.type(t).createAndFill()
    );
    return s.setSelection($.near(s.doc.resolve(o), 1)).scrollIntoView(), n == null || n(s), !0;
  }
);
B(Wc, {
  displayName: "Command<breakTableCommand>",
  group: "Table"
});
const Wm = j(
  "InsertTable",
  (t) => ({ row: e, col: n } = {}) => (r, i) => {
    const { selection: o, tr: s } = r, { from: l } = o, a = Lm(t, e, n), c = s.replaceSelectionWith(a), u = $.findFrom(c.doc.resolve(l), 1, !0);
    return u && c.setSelection(u), i == null || i(c), !0;
  }
);
B(Wm, {
  displayName: "Command<insertTableCommand>",
  group: "Table"
});
const Hm = j(
  "MoveRow",
  () => ({ from: t, to: e, pos: n } = {}) => (r, i) => {
    const { tr: o } = r;
    return !!(i == null ? void 0 : i(
      eT({ tr: o, origin: t ?? 0, target: e ?? 0, pos: n, select: !0 })
    ));
  }
);
B(Hm, {
  displayName: "Command<moveRowCommand>",
  group: "Table"
});
const jm = j(
  "MoveCol",
  () => ({ from: t, to: e, pos: n } = {}) => (r, i) => {
    const { tr: o } = r;
    return !!(i == null ? void 0 : i(
      ZN({ tr: o, origin: t ?? 0, target: e ?? 0, pos: n, select: !0 })
    ));
  }
);
B(jm, {
  displayName: "Command<moveColCommand>",
  group: "Table"
});
const qm = j(
  "SelectRow",
  () => (t = { index: 0 }) => (e, n) => {
    const { tr: r } = e;
    return !!(n == null ? void 0 : n(GN(t.index, t.pos)(r)));
  }
);
B(qm, {
  displayName: "Command<selectRowCommand>",
  group: "Table"
});
const Km = j(
  "SelectCol",
  () => (t = { index: 0 }) => (e, n) => {
    const { tr: r } = e;
    return !!(n == null ? void 0 : n(YN(t.index, t.pos)(r)));
  }
);
B(Km, {
  displayName: "Command<selectColCommand>",
  group: "Table"
});
const Jm = j(
  "SelectTable",
  () => () => (t, e) => {
    const { tr: n } = t;
    return !!(e == null ? void 0 : e(UN(n)));
  }
);
B(Jm, {
  displayName: "Command<selectTableCommand>",
  group: "Table"
});
const Um = j(
  "DeleteSelectedCells",
  () => () => (t, e) => {
    const { selection: n } = t;
    if (!(n instanceof se)) return !1;
    const r = n.isRowSelection(), i = n.isColSelection();
    return r && i ? SS(t, e) : i ? yS(t, e) : bS(t, e);
  }
);
B(Um, {
  displayName: "Command<deleteSelectedCellsCommand>",
  group: "Table"
});
const Gm = j(
  "AddColBefore",
  () => () => pS
);
B(Gm, {
  displayName: "Command<addColBeforeCommand>",
  group: "Table"
});
const Ym = j(
  "AddColAfter",
  () => () => mS
);
B(Ym, {
  displayName: "Command<addColAfterCommand>",
  group: "Table"
});
const Qm = j(
  "AddRowBefore",
  (t) => () => (e, n) => {
    if (!st(e)) return !1;
    if (n) {
      const r = wn(e);
      n(Fm(t, e.tr, r, r.top));
    }
    return !0;
  }
);
B(Qm, {
  displayName: "Command<addRowBeforeCommand>",
  group: "Table"
});
const Xm = j(
  "AddRowAfter",
  (t) => () => (e, n) => {
    if (!st(e)) return !1;
    if (n) {
      const r = wn(e);
      n(Fm(t, e.tr, r, r.bottom));
    }
    return !0;
  }
);
B(Xm, {
  displayName: "Command<addRowAfterCommand>",
  group: "Table"
});
const Zm = j(
  "SetAlign",
  () => (t = "left") => wS("alignment", t)
);
B(Zm, {
  displayName: "Command<setAlignCommand>",
  group: "Table"
});
const eg = Ve(
  (t) => new Ge(
    /^\|(?<col>\d+)[xX](?<row>\d+)\|\s$/,
    (e, n, r, i) => {
      var o, s;
      const l = e.doc.resolve(r);
      if (!l.node(-1).canReplaceWith(
        l.index(-1),
        l.indexAfter(-1),
        Rr.type(t)
      ))
        return null;
      const a = Math.max(Number(((o = n.groups) == null ? void 0 : o.row) ?? 0), 2), c = Lm(t, a, Number((s = n.groups) == null ? void 0 : s.col)), u = e.tr.replaceRangeWith(r, i, c);
      return u.setSelection(H.create(u.doc, r + 3)).scrollIntoView();
    }
  )
);
B(eg, {
  displayName: "InputRule<insertTableInputRule>",
  group: "Table"
});
const Hc = We("tableKeymap", {
  NextCell: {
    shortcuts: ["Mod-]", "Tab"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Vc.key);
    }
  },
  PrevCell: {
    shortcuts: ["Mod-[", "Shift-Tab"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call($c.key);
    }
  },
  ExitTable: {
    shortcuts: ["Mod-Enter", "Enter"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Wc.key);
    }
  }
});
B(Hc.ctx, {
  displayName: "KeymapCtx<table>",
  group: "Table"
});
B(Hc.shortcuts, {
  displayName: "Keymap<table>",
  group: "Table"
});
const pl = "footnote_definition", sf = "footnoteDefinition", jc = ye(
  "footnote_definition",
  () => ({
    group: "block",
    content: "block+",
    defining: !0,
    attrs: {
      label: {
        default: ""
      }
    },
    parseDOM: [
      {
        tag: `dl[data-type="${pl}"]`,
        getAttrs: (t) => {
          if (!(t instanceof HTMLElement)) throw Et(t);
          return {
            label: t.dataset.label
          };
        },
        contentElement: "dd"
      }
    ],
    toDOM: (t) => {
      const e = t.attrs.label;
      return [
        "dl",
        {
          // TODO: add a prosemirror plugin to sync label on change
          "data-label": e,
          "data-type": pl
        },
        ["dt", e],
        ["dd", 0]
      ];
    },
    parseMarkdown: {
      match: ({ type: t }) => t === sf,
      runner: (t, e, n) => {
        t.openNode(n, {
          label: e.label
        }).next(e.children).closeNode();
      }
    },
    toMarkdown: {
      match: (t) => t.type.name === pl,
      runner: (t, e) => {
        t.openNode(sf, void 0, {
          label: e.attrs.label,
          identifier: e.attrs.label
        }).next(e.content).closeNode();
      }
    }
  })
);
B(jc.ctx, {
  displayName: "NodeSchemaCtx<footnodeDef>",
  group: "footnote"
});
B(jc.node, {
  displayName: "NodeSchema<footnodeDef>",
  group: "footnote"
});
const ml = "footnote_reference", qc = ye(
  "footnote_reference",
  () => ({
    group: "inline",
    inline: !0,
    atom: !0,
    attrs: {
      label: {
        default: ""
      }
    },
    parseDOM: [
      {
        tag: `sup[data-type="${ml}"]`,
        getAttrs: (t) => {
          if (!(t instanceof HTMLElement)) throw Et(t);
          return {
            label: t.dataset.label
          };
        }
      }
    ],
    toDOM: (t) => {
      const e = t.attrs.label;
      return [
        "sup",
        {
          // TODO: add a prosemirror plugin to sync label on change
          "data-label": e,
          "data-type": ml
        },
        e
      ];
    },
    parseMarkdown: {
      match: ({ type: t }) => t === "footnoteReference",
      runner: (t, e, n) => {
        t.addNode(n, {
          label: e.label
        });
      }
    },
    toMarkdown: {
      match: (t) => t.type.name === ml,
      runner: (t, e) => {
        t.addNode("footnoteReference", void 0, void 0, {
          label: e.attrs.label,
          identifier: e.attrs.label
        });
      }
    }
  })
);
B(qc.ctx, {
  displayName: "NodeSchemaCtx<footnodeRef>",
  group: "footnote"
});
B(qc.node, {
  displayName: "NodeSchema<footnodeRef>",
  group: "footnote"
});
const tg = At.extendSchema(
  (t) => (e) => {
    const n = t(e);
    return {
      ...n,
      attrs: {
        ...n.attrs,
        checked: {
          default: null
        }
      },
      parseDOM: [
        {
          tag: 'li[data-item-type="task"]',
          getAttrs: (r) => {
            if (!(r instanceof HTMLElement)) throw Et(r);
            return {
              label: r.dataset.label,
              listType: r.dataset.listType,
              spread: r.dataset.spread,
              checked: r.dataset.checked ? r.dataset.checked === "true" : null
            };
          }
        },
        ...(n == null ? void 0 : n.parseDOM) || []
      ],
      toDOM: (r) => n.toDOM && r.attrs.checked == null ? n.toDOM(r) : [
        "li",
        {
          "data-item-type": "task",
          "data-label": r.attrs.label,
          "data-list-type": r.attrs.listType,
          "data-spread": r.attrs.spread,
          "data-checked": r.attrs.checked
        },
        0
      ],
      parseMarkdown: {
        match: ({ type: r }) => r === "listItem",
        runner: (r, i, o) => {
          if (i.checked == null) {
            n.parseMarkdown.runner(r, i, o);
            return;
          }
          const s = i.label != null ? `${i.label}.` : "•", l = i.checked != null ? !!i.checked : null, a = i.label != null ? "ordered" : "bullet", c = i.spread != null ? `${i.spread}` : "true";
          r.openNode(o, { label: s, listType: a, spread: c, checked: l }), r.next(i.children), r.closeNode();
        }
      },
      toMarkdown: {
        match: (r) => r.type.name === "list_item",
        runner: (r, i) => {
          if (i.attrs.checked == null) {
            n.toMarkdown.runner(r, i);
            return;
          }
          const o = i.attrs.label, s = i.attrs.listType, l = i.attrs.spread === "true", a = i.attrs.checked;
          r.openNode("listItem", void 0, {
            label: o,
            listType: s,
            spread: l,
            checked: a
          }), r.next(i.content), r.closeNode();
        }
      }
    };
  }
);
B(tg, {
  displayName: "NodeSchema<listItem>",
  group: "ListItem"
});
const ng = Ve(() => new Ge(
  /^\[(?<checked>\s|x)\]\s$/,
  (t, e, n, r) => {
    var i;
    const o = t.doc.resolve(n);
    let s = 0, l = o.node(s);
    for (; l && l.type.name !== "list_item"; )
      s--, l = o.node(s);
    if (!l || l.attrs.checked != null) return null;
    const a = ((i = e.groups) == null ? void 0 : i.checked) === "x", c = o.before(s), u = t.tr;
    return u.deleteRange(n, r).setNodeMarkup(c, void 0, {
      ...l.attrs,
      checked: a
    }), u;
  }
));
B(ng, {
  displayName: "InputRule<wrapInTaskListInputRule>",
  group: "ListItem"
});
const tT = [
  _c,
  Hc
].flat(), nT = [
  eg,
  ng
], rT = [zm], rg = ot(() => XS);
B(rg, {
  displayName: "Prose<autoInsertSpanPlugin>",
  group: "Prose"
});
const iT = ot(() => vS({}));
B(iT, {
  displayName: "Prose<columnResizingPlugin>",
  group: "Prose"
});
const ig = ot(
  () => HS({ allowTableNodeSelection: !0 })
);
B(ig, {
  displayName: "Prose<tableEditingPlugin>",
  group: "Prose"
});
const Kc = Un("remarkGFM", () => KN);
B(Kc.plugin, {
  displayName: "Remark<remarkGFMPlugin>",
  group: "Remark"
});
B(Kc.options, {
  displayName: "RemarkConfig<remarkGFMPlugin>",
  group: "Remark"
});
const oT = new ge("MILKDOWN_KEEP_TABLE_ALIGN_PLUGIN");
function sT(t, e) {
  let n = 0;
  return e.forEach((r, i, o) => {
    r === t && (n = o);
  }), n;
}
const og = ot(() => new Ne({
  key: oT,
  appendTransaction: (t, e, n) => {
    let r;
    const i = (o, s) => {
      if (r || (r = n.tr), o.type.name !== "table_cell") return;
      const l = n.doc.resolve(s), a = l.node(l.depth), u = l.node(l.depth - 1).firstChild;
      if (!u) return;
      const h = sT(o, a), f = u.maybeChild(h);
      if (!f) return;
      const d = f.attrs.alignment, p = o.attrs.alignment;
      d !== p && r.setNodeMarkup(s, void 0, { ...o.attrs, alignment: d });
    };
    return e.doc !== n.doc && n.doc.descendants(i), r;
  }
}));
B(og, {
  displayName: "Prose<keepTableAlignPlugin>",
  group: "Prose"
});
const lT = [
  og,
  rg,
  Kc,
  ig
].flat(), aT = [
  tg,
  Rr,
  Ms,
  qi,
  Ns,
  Ki,
  jc,
  qc,
  Fc,
  Hi
].flat(), cT = [
  Vc,
  $c,
  Wc,
  Wm,
  Hm,
  jm,
  qm,
  Km,
  Jm,
  Um,
  Qm,
  Xm,
  Gm,
  Ym,
  Zm,
  Bc
], uT = [
  aT,
  nT,
  rT,
  tT,
  cT,
  lT
].flat();
var ns = 200, me = function() {
};
me.prototype.append = function(e) {
  return e.length ? (e = me.from(e), !this.length && e || e.length < ns && this.leafAppend(e) || this.length < ns && e.leafPrepend(this) || this.appendInner(e)) : this;
};
me.prototype.prepend = function(e) {
  return e.length ? me.from(e).append(this) : this;
};
me.prototype.appendInner = function(e) {
  return new hT(this, e);
};
me.prototype.slice = function(e, n) {
  return e === void 0 && (e = 0), n === void 0 && (n = this.length), e >= n ? me.empty : this.sliceInner(Math.max(0, e), Math.min(this.length, n));
};
me.prototype.get = function(e) {
  if (!(e < 0 || e >= this.length))
    return this.getInner(e);
};
me.prototype.forEach = function(e, n, r) {
  n === void 0 && (n = 0), r === void 0 && (r = this.length), n <= r ? this.forEachInner(e, n, r, 0) : this.forEachInvertedInner(e, n, r, 0);
};
me.prototype.map = function(e, n, r) {
  n === void 0 && (n = 0), r === void 0 && (r = this.length);
  var i = [];
  return this.forEach(function(o, s) {
    return i.push(e(o, s));
  }, n, r), i;
};
me.from = function(e) {
  return e instanceof me ? e : e && e.length ? new sg(e) : me.empty;
};
var sg = /* @__PURE__ */ function(t) {
  function e(r) {
    t.call(this), this.values = r;
  }
  t && (e.__proto__ = t), e.prototype = Object.create(t && t.prototype), e.prototype.constructor = e;
  var n = { length: { configurable: !0 }, depth: { configurable: !0 } };
  return e.prototype.flatten = function() {
    return this.values;
  }, e.prototype.sliceInner = function(i, o) {
    return i == 0 && o == this.length ? this : new e(this.values.slice(i, o));
  }, e.prototype.getInner = function(i) {
    return this.values[i];
  }, e.prototype.forEachInner = function(i, o, s, l) {
    for (var a = o; a < s; a++)
      if (i(this.values[a], l + a) === !1)
        return !1;
  }, e.prototype.forEachInvertedInner = function(i, o, s, l) {
    for (var a = o - 1; a >= s; a--)
      if (i(this.values[a], l + a) === !1)
        return !1;
  }, e.prototype.leafAppend = function(i) {
    if (this.length + i.length <= ns)
      return new e(this.values.concat(i.flatten()));
  }, e.prototype.leafPrepend = function(i) {
    if (this.length + i.length <= ns)
      return new e(i.flatten().concat(this.values));
  }, n.length.get = function() {
    return this.values.length;
  }, n.depth.get = function() {
    return 0;
  }, Object.defineProperties(e.prototype, n), e;
}(me);
me.empty = new sg([]);
var hT = /* @__PURE__ */ function(t) {
  function e(n, r) {
    t.call(this), this.left = n, this.right = r, this.length = n.length + r.length, this.depth = Math.max(n.depth, r.depth) + 1;
  }
  return t && (e.__proto__ = t), e.prototype = Object.create(t && t.prototype), e.prototype.constructor = e, e.prototype.flatten = function() {
    return this.left.flatten().concat(this.right.flatten());
  }, e.prototype.getInner = function(r) {
    return r < this.left.length ? this.left.get(r) : this.right.get(r - this.left.length);
  }, e.prototype.forEachInner = function(r, i, o, s) {
    var l = this.left.length;
    if (i < l && this.left.forEachInner(r, i, Math.min(o, l), s) === !1 || o > l && this.right.forEachInner(r, Math.max(i - l, 0), Math.min(this.length, o) - l, s + l) === !1)
      return !1;
  }, e.prototype.forEachInvertedInner = function(r, i, o, s) {
    var l = this.left.length;
    if (i > l && this.right.forEachInvertedInner(r, i - l, Math.max(o, l) - l, s + l) === !1 || o < l && this.left.forEachInvertedInner(r, Math.min(i, l), o, s) === !1)
      return !1;
  }, e.prototype.sliceInner = function(r, i) {
    if (r == 0 && i == this.length)
      return this;
    var o = this.left.length;
    return i <= o ? this.left.slice(r, i) : r >= o ? this.right.slice(r - o, i - o) : this.left.slice(r, o).append(this.right.slice(0, i - o));
  }, e.prototype.leafAppend = function(r) {
    var i = this.right.leafAppend(r);
    if (i)
      return new e(this.left, i);
  }, e.prototype.leafPrepend = function(r) {
    var i = this.left.leafPrepend(r);
    if (i)
      return new e(i, this.right);
  }, e.prototype.appendInner = function(r) {
    return this.left.depth >= Math.max(this.right.depth, r.depth) + 1 ? new e(this.left, new e(this.right, r)) : new e(this, r);
  }, e;
}(me);
const fT = 500;
class ht {
  constructor(e, n) {
    this.items = e, this.eventCount = n;
  }
  // Pop the latest event off the branch's history and apply it
  // to a document transform.
  popEvent(e, n) {
    if (this.eventCount == 0)
      return null;
    let r = this.items.length;
    for (; ; r--)
      if (this.items.get(r - 1).selection) {
        --r;
        break;
      }
    let i, o;
    n && (i = this.remapping(r, this.items.length), o = i.maps.length);
    let s = e.tr, l, a, c = [], u = [];
    return this.items.forEach((h, f) => {
      if (!h.step) {
        i || (i = this.remapping(r, f + 1), o = i.maps.length), o--, u.push(h);
        return;
      }
      if (i) {
        u.push(new xt(h.map));
        let d = h.step.map(i.slice(o)), p;
        d && s.maybeStep(d).doc && (p = s.mapping.maps[s.mapping.maps.length - 1], c.push(new xt(p, void 0, void 0, c.length + u.length))), o--, p && i.appendMap(p, o);
      } else
        s.maybeStep(h.step);
      if (h.selection)
        return l = i ? h.selection.map(i.slice(o)) : h.selection, a = new ht(this.items.slice(0, r).append(u.reverse().concat(c)), this.eventCount - 1), !1;
    }, this.items.length, 0), { remaining: a, transform: s, selection: l };
  }
  // Create a new branch with the given transform added.
  addTransform(e, n, r, i) {
    let o = [], s = this.eventCount, l = this.items, a = !i && l.length ? l.get(l.length - 1) : null;
    for (let u = 0; u < e.steps.length; u++) {
      let h = e.steps[u].invert(e.docs[u]), f = new xt(e.mapping.maps[u], h, n), d;
      (d = a && a.merge(f)) && (f = d, u ? o.pop() : l = l.slice(0, l.length - 1)), o.push(f), n && (s++, n = void 0), i || (a = f);
    }
    let c = s - r.depth;
    return c > pT && (l = dT(l, c), s -= c), new ht(l.append(o), s);
  }
  remapping(e, n) {
    let r = new bi();
    return this.items.forEach((i, o) => {
      let s = i.mirrorOffset != null && o - i.mirrorOffset >= e ? r.maps.length - i.mirrorOffset : void 0;
      r.appendMap(i.map, s);
    }, e, n), r;
  }
  addMaps(e) {
    return this.eventCount == 0 ? this : new ht(this.items.append(e.map((n) => new xt(n))), this.eventCount);
  }
  // When the collab module receives remote changes, the history has
  // to know about those, so that it can adjust the steps that were
  // rebased on top of the remote changes, and include the position
  // maps for the remote changes in its array of items.
  rebased(e, n) {
    if (!this.eventCount)
      return this;
    let r = [], i = Math.max(0, this.items.length - n), o = e.mapping, s = e.steps.length, l = this.eventCount;
    this.items.forEach((f) => {
      f.selection && l--;
    }, i);
    let a = n;
    this.items.forEach((f) => {
      let d = o.getMirror(--a);
      if (d == null)
        return;
      s = Math.min(s, d);
      let p = o.maps[d];
      if (f.step) {
        let m = e.steps[d].invert(e.docs[d]), y = f.selection && f.selection.map(o.slice(a + 1, d));
        y && l++, r.push(new xt(p, m, y));
      } else
        r.push(new xt(p));
    }, i);
    let c = [];
    for (let f = n; f < s; f++)
      c.push(new xt(o.maps[f]));
    let u = this.items.slice(0, i).append(c).append(r), h = new ht(u, l);
    return h.emptyItemCount() > fT && (h = h.compress(this.items.length - r.length)), h;
  }
  emptyItemCount() {
    let e = 0;
    return this.items.forEach((n) => {
      n.step || e++;
    }), e;
  }
  // Compressing a branch means rewriting it to push the air (map-only
  // items) out. During collaboration, these naturally accumulate
  // because each remote change adds one. The `upto` argument is used
  // to ensure that only the items below a given level are compressed,
  // because `rebased` relies on a clean, untouched set of items in
  // order to associate old items with rebased steps.
  compress(e = this.items.length) {
    let n = this.remapping(0, e), r = n.maps.length, i = [], o = 0;
    return this.items.forEach((s, l) => {
      if (l >= e)
        i.push(s), s.selection && o++;
      else if (s.step) {
        let a = s.step.map(n.slice(r)), c = a && a.getMap();
        if (r--, c && n.appendMap(c, r), a) {
          let u = s.selection && s.selection.map(n.slice(r));
          u && o++;
          let h = new xt(c.invert(), a, u), f, d = i.length - 1;
          (f = i.length && i[d].merge(h)) ? i[d] = f : i.push(h);
        }
      } else s.map && r--;
    }, this.items.length, 0), new ht(me.from(i.reverse()), o);
  }
}
ht.empty = new ht(me.empty, 0);
function dT(t, e) {
  let n;
  return t.forEach((r, i) => {
    if (r.selection && e-- == 0)
      return n = i, !1;
  }), t.slice(n);
}
class xt {
  constructor(e, n, r, i) {
    this.map = e, this.step = n, this.selection = r, this.mirrorOffset = i;
  }
  merge(e) {
    if (this.step && e.step && !e.selection) {
      let n = e.step.merge(this.step);
      if (n)
        return new xt(n.getMap().invert(), n, this.selection);
    }
  }
}
class tn {
  constructor(e, n, r, i, o) {
    this.done = e, this.undone = n, this.prevRanges = r, this.prevTime = i, this.prevComposition = o;
  }
}
const pT = 20;
function mT(t, e, n, r) {
  let i = n.getMeta($n), o;
  if (i)
    return i.historyState;
  n.getMeta(kT) && (t = new tn(t.done, t.undone, null, 0, -1));
  let s = n.getMeta("appendedTransaction");
  if (n.steps.length == 0)
    return t;
  if (s && s.getMeta($n))
    return s.getMeta($n).redo ? new tn(t.done.addTransform(n, void 0, r, Bo(e)), t.undone, lf(n.mapping.maps), t.prevTime, t.prevComposition) : new tn(t.done, t.undone.addTransform(n, void 0, r, Bo(e)), null, t.prevTime, t.prevComposition);
  if (n.getMeta("addToHistory") !== !1 && !(s && s.getMeta("addToHistory") === !1)) {
    let l = n.getMeta("composition"), a = t.prevTime == 0 || !s && t.prevComposition != l && (t.prevTime < (n.time || 0) - r.newGroupDelay || !gT(n, t.prevRanges)), c = s ? gl(t.prevRanges, n.mapping) : lf(n.mapping.maps);
    return new tn(t.done.addTransform(n, a ? e.selection.getBookmark() : void 0, r, Bo(e)), ht.empty, c, n.time, l ?? t.prevComposition);
  } else return (o = n.getMeta("rebased")) ? new tn(t.done.rebased(n, o), t.undone.rebased(n, o), gl(t.prevRanges, n.mapping), t.prevTime, t.prevComposition) : new tn(t.done.addMaps(n.mapping.maps), t.undone.addMaps(n.mapping.maps), gl(t.prevRanges, n.mapping), t.prevTime, t.prevComposition);
}
function gT(t, e) {
  if (!e)
    return !1;
  if (!t.docChanged)
    return !0;
  let n = !1;
  return t.mapping.maps[0].forEach((r, i) => {
    for (let o = 0; o < e.length; o += 2)
      r <= e[o + 1] && i >= e[o] && (n = !0);
  }), n;
}
function lf(t) {
  let e = [];
  for (let n = t.length - 1; n >= 0 && e.length == 0; n--)
    t[n].forEach((r, i, o, s) => e.push(o, s));
  return e;
}
function gl(t, e) {
  if (!t)
    return null;
  let n = [];
  for (let r = 0; r < t.length; r += 2) {
    let i = e.map(t[r], 1), o = e.map(t[r + 1], -1);
    i <= o && n.push(i, o);
  }
  return n;
}
function yT(t, e, n) {
  let r = Bo(e), i = $n.get(e).spec.config, o = (n ? t.undone : t.done).popEvent(e, r);
  if (!o)
    return null;
  let s = o.selection.resolve(o.transform.doc), l = (n ? t.done : t.undone).addTransform(o.transform, e.selection.getBookmark(), i, r), a = new tn(n ? l : o.remaining, n ? o.remaining : l, null, 0, -1);
  return o.transform.setSelection(s).setMeta($n, { redo: n, historyState: a });
}
let yl = !1, af = null;
function Bo(t) {
  let e = t.plugins;
  if (af != e) {
    yl = !1, af = e;
    for (let n = 0; n < e.length; n++)
      if (e[n].spec.historyPreserveItems) {
        yl = !0;
        break;
      }
  }
  return yl;
}
const $n = new ge("history"), kT = new ge("closeHistory");
function bT(t = {}) {
  return t = {
    depth: t.depth || 100,
    newGroupDelay: t.newGroupDelay || 500
  }, new Ne({
    key: $n,
    state: {
      init() {
        return new tn(ht.empty, ht.empty, null, 0, -1);
      },
      apply(e, n, r) {
        return mT(n, r, e, t);
      }
    },
    config: t,
    props: {
      handleDOMEvents: {
        beforeinput(e, n) {
          let r = n.inputType, i = r == "historyUndo" ? ag : r == "historyRedo" ? cg : null;
          return i ? (n.preventDefault(), i(e.state, e.dispatch)) : !1;
        }
      }
    }
  });
}
function lg(t, e) {
  return (n, r) => {
    let i = $n.getState(n);
    if (!i || (t ? i.undone : i.done).eventCount == 0)
      return !1;
    if (r) {
      let o = yT(i, n, t);
      o && r(e ? o.scrollIntoView() : o);
    }
    return !0;
  };
}
const ag = lg(!1, !0), cg = lg(!0, !0);
function vr(t, e) {
  return Object.assign(t, {
    meta: {
      package: "@milkdown/plugin-history",
      ...e
    }
  }), t;
}
const Jc = j("Undo", () => () => ag);
vr(Jc, {
  displayName: "Command<undo>"
});
const Uc = j("Redo", () => () => cg);
vr(Uc, {
  displayName: "Command<redo>"
});
const Gc = gt({}, "historyProviderConfig");
vr(Gc, {
  displayName: "Ctx<historyProviderConfig>"
});
const ug = ot(
  (t) => bT(t.get(Gc.key))
);
vr(ug, {
  displayName: "Ctx<historyProviderPlugin>"
});
const Yc = We("historyKeymap", {
  Undo: {
    shortcuts: "Mod-z",
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Jc.key);
    }
  },
  Redo: {
    shortcuts: ["Mod-y", "Shift-Mod-z"],
    command: (t) => {
      const e = t.get(X);
      return () => e.call(Uc.key);
    }
  }
});
vr(Yc.ctx, {
  displayName: "KeymapCtx<history>"
});
vr(Yc.shortcuts, {
  displayName: "Keymap<history>"
});
const wT = [
  Gc,
  ug,
  Yc,
  Jc,
  Uc
].flat();
function xT(t, e) {
  const { doc: n, selection: r } = t;
  if (!n || !r || !(r instanceof H || r instanceof Be))
    return t;
  const { to: i } = r, o = e.type === "space" ? Array(e.size).fill(" ").join("") : "	";
  return t.insertText(o, i);
}
const Ts = gt(
  { type: "space", size: 2 },
  "indentConfig"
);
Ts.meta = {
  package: "@milkdown/plugin-indent",
  displayName: "Ctx<indentConfig>"
};
const hg = $p((t) => ({
  Tab: (e, n) => {
    const r = t.get(Ts.key), { tr: i } = e, o = xT(i, r);
    return o.docChanged ? (n == null || n(o), !0) : !1;
  }
}));
hg.meta = {
  package: "@milkdown/plugin-indent",
  displayName: "Shortcut<indent>"
};
const CT = [Ts, hg];
function ca(t) {
  if (!t) return !1;
  if (Array.isArray(t))
    return t.length > 1 ? !1 : ca(t[0]);
  const e = t.content;
  return e ? ca(e) : t.type === "text";
}
function ST(t) {
  if (t.content.childCount === 1) {
    const e = t.content.firstChild;
    if ((e == null ? void 0 : e.type.name) === "text" && e.marks.length === 0) return e;
    if ((e == null ? void 0 : e.type.name) === "paragraph" && e.childCount === 1) {
      const n = e.firstChild;
      if ((n == null ? void 0 : n.type.name) === "text" && n.marks.length === 0) return n;
    }
  }
  return !1;
}
const fg = ot((t) => {
  const e = t.get(Ht);
  t.update(gi, (i) => ({
    ...i,
    editable: i.editable ?? (() => !0)
  }));
  const n = new ge("MILKDOWN_CLIPBOARD");
  return new Ne({
    key: n,
    props: {
      handlePaste: (i, o) => {
        var s, l;
        const a = t.get(pi), c = (l = (s = i.props).editable) == null ? void 0 : l.call(s, i.state), { clipboardData: u } = o;
        if (!c || !u || i.state.selection.$from.node().type.spec.code) return !1;
        const f = u.getData("text/plain"), d = u.getData("vscode-editor-data");
        if (d) {
          const C = JSON.parse(d), v = C == null ? void 0 : C.mode;
          if (f && v) {
            const { tr: I } = i.state, w = ow("code_block", e);
            return I.replaceSelectionWith(w.create({ language: v })).setSelection(
              H.near(
                I.doc.resolve(Math.max(0, I.selection.from - 2))
              )
            ).insertText(f.replace(/\r\n?/g, `
`)), i.dispatch(I), !0;
          }
        }
        const p = u.getData("text/html");
        if (p.length === 0 && f.length === 0) return !1;
        const m = Wn.fromSchema(e);
        let y;
        if (p.length === 0) {
          const C = a(f);
          if (!C || typeof C == "string") return !1;
          y = Kn.fromSchema(e).serializeFragment(
            C.content
          );
        } else {
          const C = document.createElement("template");
          C.innerHTML = p, y = C.content.cloneNode(!0), C.remove();
        }
        const g = m.parseSlice(y), N = ST(g);
        return N ? (i.dispatch(i.state.tr.replaceSelectionWith(N, !0)), !0) : (i.dispatch(i.state.tr.replaceSelection(g)), !0);
      },
      clipboardTextSerializer: (i) => {
        const o = t.get(br);
        if (ca(i.content.toJSON()))
          return i.content.textBetween(
            0,
            i.content.size,
            `

`
          );
        const l = e.topNodeType.createAndFill(void 0, i.content);
        return l ? o(l) : "";
      }
    }
  });
});
fg.meta = {
  displayName: "Prose<clipboard>",
  package: "@milkdown/plugin-clipboard"
};
const Qc = gt(
  {
    shouldAppend: (t) => !(!t || ["heading", "paragraph"].includes(t.type.name)),
    getNode: (t) => t.schema.nodes.paragraph.create()
  },
  "trailingConfig"
);
Qc.meta = {
  package: "@milkdown/plugin-trailing",
  displayName: "Ctx<trailingConfig>"
};
const dg = ot((t) => {
  const e = new ge("MILKDOWN_TRAILING"), { shouldAppend: n, getNode: r } = t.get(Qc.key), i = new Ne({
    key: e,
    state: {
      init: (o, s) => {
        const l = s.tr.doc.lastChild;
        return n(l, s);
      },
      apply: (o, s, l, a) => {
        if (!o.docChanged) return s;
        const c = o.doc.lastChild;
        return n(c, a);
      }
    },
    appendTransaction: (o, s, l) => {
      const { doc: a, tr: c } = l, u = r == null ? void 0 : r(l), h = i.getState(l), f = a.content.size;
      if (!(!h || !u))
        return c.insert(f, u);
    }
  });
  return i;
});
dg.meta = {
  package: "@milkdown/plugin-trailing",
  displayName: "Prose<trailing>"
};
const MT = [Qc, dg];
var kl, cf;
function NT() {
  if (cf) return kl;
  cf = 1;
  var t = "Expected a function", e = NaN, n = "[object Symbol]", r = /^\s+|\s+$/g, i = /^[-+]0x[0-9a-f]+$/i, o = /^0b[01]+$/i, s = /^0o[0-7]+$/i, l = parseInt, a = typeof Qi == "object" && Qi && Qi.Object === Object && Qi, c = typeof self == "object" && self && self.Object === Object && self, u = a || c || Function("return this")(), h = Object.prototype, f = h.toString, d = Math.max, p = Math.min, m = function() {
    return u.Date.now();
  };
  function y(I, w, z) {
    var W, O, x, R, D, J, te = 0, K = !1, ce = !1, he = !0;
    if (typeof I != "function")
      throw new TypeError(t);
    w = v(w) || 0, g(z) && (K = !!z.leading, ce = "maxWait" in z, x = ce ? d(v(z.maxWait) || 0, w) : x, he = "trailing" in z ? !!z.trailing : he);
    function Oe(ie) {
      var ke = W, Ye = O;
      return W = O = void 0, te = ie, R = I.apply(Ye, ke), R;
    }
    function lt(ie) {
      return te = ie, D = setTimeout(ze, w), K ? Oe(ie) : R;
    }
    function k(ie) {
      var ke = ie - J, Ye = ie - te, Rt = w - ke;
      return ce ? p(Rt, x - Ye) : Rt;
    }
    function Pe(ie) {
      var ke = ie - J, Ye = ie - te;
      return J === void 0 || ke >= w || ke < 0 || ce && Ye >= x;
    }
    function ze() {
      var ie = m();
      if (Pe(ie))
        return b(ie);
      D = setTimeout(ze, k(ie));
    }
    function b(ie) {
      return D = void 0, he && W ? Oe(ie) : (W = O = void 0, R);
    }
    function He() {
      D !== void 0 && clearTimeout(D), te = 0, W = J = O = D = void 0;
    }
    function Dt() {
      return D === void 0 ? R : b(m());
    }
    function fe() {
      var ie = m(), ke = Pe(ie);
      if (W = arguments, O = this, J = ie, ke) {
        if (D === void 0)
          return lt(J);
        if (ce)
          return D = setTimeout(ze, w), Oe(J);
      }
      return D === void 0 && (D = setTimeout(ze, w)), R;
    }
    return fe.cancel = He, fe.flush = Dt, fe;
  }
  function g(I) {
    var w = typeof I;
    return !!I && (w == "object" || w == "function");
  }
  function N(I) {
    return !!I && typeof I == "object";
  }
  function C(I) {
    return typeof I == "symbol" || N(I) && f.call(I) == n;
  }
  function v(I) {
    if (typeof I == "number")
      return I;
    if (C(I))
      return e;
    if (g(I)) {
      var w = typeof I.valueOf == "function" ? I.valueOf() : I;
      I = g(w) ? w + "" : w;
    }
    if (typeof I != "string")
      return I === 0 ? I : +I;
    I = I.replace(r, "");
    var z = o.test(I);
    return z || s.test(I) ? l(I.slice(2), z ? 2 : 8) : i.test(I) ? e : +I;
  }
  return kl = y, kl;
}
var TT = NT();
const IT = /* @__PURE__ */ ud(TT);
class pg {
  constructor() {
    this.beforeMountedListeners = [], this.mountedListeners = [], this.updatedListeners = [], this.markdownUpdatedListeners = [], this.blurListeners = [], this.focusListeners = [], this.destroyListeners = [], this.beforeMount = (e) => (this.beforeMountedListeners.push(e), this), this.mounted = (e) => (this.mountedListeners.push(e), this), this.updated = (e) => (this.updatedListeners.push(e), this);
  }
  /// A getter to get all [subscribers](#interface-subscribers). You should not use this method directly.
  get listeners() {
    return {
      beforeMount: this.beforeMountedListeners,
      mounted: this.mountedListeners,
      updated: this.updatedListeners,
      markdownUpdated: this.markdownUpdatedListeners,
      blur: this.blurListeners,
      focus: this.focusListeners,
      destroy: this.destroyListeners
    };
  }
  /// Subscribe to the markdownUpdated event.
  /// This event will be triggered after the editor state is updated and **the document is changed**.
  /// The second parameter is the current markdown and the third parameter is the previous markdown.
  markdownUpdated(e) {
    return this.markdownUpdatedListeners.push(e), this;
  }
  /// Subscribe to the blur event.
  /// This event will be triggered when the editor is blurred.
  blur(e) {
    return this.blurListeners.push(e), this;
  }
  /// Subscribe to the focus event.
  /// This event will be triggered when the editor is focused.
  focus(e) {
    return this.focusListeners.push(e), this;
  }
  /// Subscribe to the destroy event.
  /// This event will be triggered before the editor is destroyed.
  destroy(e) {
    return this.destroyListeners.push(e), this;
  }
}
const rs = Y(
  new pg(),
  "listener"
), AT = new ge("MILKDOWN_LISTENER"), mg = (t) => (t.inject(rs, new pg()), async () => {
  await t.wait(Bn);
  const e = t.get(rs), { listeners: n } = e;
  n.beforeMount.forEach((l) => l(t)), await t.wait(mi);
  const r = t.get(br);
  let i = null, o = null;
  const s = new Ne({
    key: AT,
    view: () => ({
      destroy: () => {
        n.destroy.forEach((l) => l(t));
      }
    }),
    props: {
      handleDOMEvents: {
        focus: () => (n.focus.forEach((l) => l(t)), !1),
        blur: () => (n.blur.forEach((l) => l(t)), !1)
      }
    },
    state: {
      init: (l, a) => {
        i = a.doc, o = r(a.doc);
      },
      apply: (l) => !l.docChanged || l.getMeta("addToHistory") === !1 ? void 0 : IT(() => {
        const { doc: c } = l;
        if (n.updated.length > 0 && i && !i.eq(c) && n.updated.forEach((u) => {
          u(t, c, i);
        }), n.markdownUpdated.length > 0 && i && !i.eq(c)) {
          const u = r(c);
          n.markdownUpdated.forEach((h) => {
            h(t, u, o);
          }), o = u;
        }
        i = c;
      }, 200)()
    }
  });
  t.update(It, (l) => l.concat(s)), await t.wait(Po), n.mounted.forEach((l) => l(t));
});
mg.meta = {
  package: "@milkdown/plugin-listener",
  displayName: "Listener"
};
var gg = /* @__PURE__ */ ((t) => (t.CodeMirror = "code-mirror", t.ListItem = "list-item", t.LinkTooltip = "link-tooltip", t.Cursor = "cursor", t.ImageBlock = "image-block", t.BlockEdit = "block-edit", t.Toolbar = "toolbar", t.Placeholder = "placeholder", t.Table = "table", t.Latex = "latex", t))(gg || {});
const ET = {
  cursor: !0,
  "list-item": !0,
  "link-tooltip": !0,
  "image-block": !0,
  "block-edit": !0,
  placeholder: !0,
  toolbar: !0,
  "code-mirror": !0,
  table: !0,
  latex: !0
};
async function OT(t, e, n) {
  switch (t) {
    case "code-mirror": {
      const { defineFeature: r } = await import("./index-B3KiKTSt-Dla4yOC_.js").then((i) => i.E);
      return r(e, n);
    }
    case "list-item": {
      const { defineFeature: r } = await import("./index-Cuk7cL-r-CPLW1_wK.js");
      return r(e, n);
    }
    case "link-tooltip": {
      const { defineFeature: r } = await import("./index-DcRgwPLd-BWI7DgDg.js");
      return r(e, n);
    }
    case "image-block": {
      const { defineFeature: r } = await import("./index-BFsG6770-DHtlwtpY.js");
      return r(e, n);
    }
    case "cursor": {
      const { defineFeature: r } = await import("./index-CxJ9fxJm-BE0w9TAT.js");
      return r(e, n);
    }
    case "block-edit": {
      const { defineFeature: r } = await import("./index-uHWmZXqe-BIlmB7hx.js");
      return r(e, n);
    }
    case "placeholder": {
      const { defineFeature: r } = await import("./index-DOrkOhki-Btp2sKOe.js");
      return r(e, n);
    }
    case "toolbar": {
      const { defineFeature: r } = await import("./index-CsEtSFhk-vc9nsGmJ.js");
      return r(e, n);
    }
    case "table": {
      const { defineFeature: r } = await import("./index-CBrOT1fW-CbrMgMIz.js");
      return r(e, n);
    }
    case "latex": {
      const { defineFeature: r } = await import("./index-s33c_M2f-BehpTq0G.js");
      return r(e, n);
    }
  }
}
const DT = Y([], "FeaturesCtx"), RT = Y({}, "CrepeCtx");
function vT(t) {
  return (e) => {
    e.inject(DT, t);
  };
}
var yg = (t) => {
  throw TypeError(t);
}, kg = (t, e, n) => e.has(t) || yg("Cannot " + n), Qe = (t, e, n) => (kg(t, e, "read from private field"), n ? n.call(t) : e.get(t)), io = (t, e, n) => e.has(t) ? yg("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, n), oo = (t, e, n, r) => (kg(t, e, "write to private field"), e.set(t, n), n), ut, si, _o, $o;
class ua {
  constructor({
    root: e,
    features: n = {},
    featureConfigs: r = {},
    defaultValue: i = ""
  }) {
    io(this, ut), io(this, si), io(this, _o), io(this, $o, !0);
    var o;
    const s = Object.entries({
      ...ET,
      ...n
    }).filter(([, a]) => a).map(([a]) => a);
    oo(this, _o, (o = typeof e == "string" ? document.querySelector(e) : e) != null ? o : document.body), oo(this, ut, xC.make().config((a) => {
      a.inject(RT, this);
    }).config(vT(s)).config((a) => {
      a.set(zo, Qe(this, _o)), a.set(Ro, i), a.set(gi, {
        editable: () => Qe(this, $o)
      }), a.update(Ts.key, (c) => ({
        ...c,
        size: 4
      }));
    }).use(GC).use(mg).use(wT).use(CT).use(MT).use(fg).use(uT));
    const l = [];
    s.forEach((a) => {
      const c = r[a];
      l.push(OT(a, Qe(this, ut), c));
    }), oo(this, si, Promise.all(l));
  }
  async create() {
    return await Qe(this, si), Qe(this, ut).create();
  }
  async destroy() {
    return await Qe(this, si), Qe(this, ut).destroy();
  }
  get editor() {
    return Qe(this, ut);
  }
  setReadonly(e) {
    return oo(this, $o, !e), this;
  }
  getMarkdown() {
    return Qe(this, ut).action(TC());
  }
  on(e) {
    return Qe(this, ut).status !== Bp.Created ? (Qe(this, ut).config((n) => {
      const r = n.get(rs);
      e(r);
    }), this) : (Qe(this, ut).action((n) => {
      const r = n.get(rs);
      e(r);
    }), this);
  }
}
ut = /* @__PURE__ */ new WeakMap();
si = /* @__PURE__ */ new WeakMap();
_o = /* @__PURE__ */ new WeakMap();
$o = /* @__PURE__ */ new WeakMap();
ua.Feature = gg;
class bg extends HTMLElement {
  constructor() {
    super(...arguments);
    Qn(this, "editor", null);
    Qn(this, "editorContainer", null);
    Qn(this, "shadowRoot", null);
    Qn(this, "defaultMarkdown", "<calc>");
    // Useremmo questa proprietà in futuro se avessimo bisogno di una gestione più complessa degli eventi
    // private editorEvents: CustomEvent[] = [];
    // Flag per indicare che l'inizializzazione è in corso
    Qn(this, "isInitializing", !1);
  }
  // Osserva gli attributi per reagire ai cambiamenti
  static get observedAttributes() {
    return ["markdown"];
  }
  // Quando un attributo cambia
  attributeChangedCallback(n, r, i) {
    var o;
    if (n === "markdown" && r !== i) {
      if (this.isInitializing || typeof i != "string" || !this.editor)
        return;
      if (this.editorContainer)
        try {
          this.isInitializing = !0, console.log("Aggiornamento del contenuto markdown:", i.substring(0, 20) + "...");
          const s = this.editorContainer.parentNode;
          this.editor.destroy(), this.editor = null, s && this.editorContainer && s.removeChild(this.editorContainer), this.editorContainer = document.createElement("div"), this.editorContainer.className = "docs-pilot-editor", (o = this.shadowRoot) == null || o.appendChild(this.editorContainer), this.editor = new ua({
            root: this.editorContainer,
            defaultValue: i
          }), this.setupEditorListeners(), this.editor.create().then(() => {
            console.log("Editor ricreato con successo"), this.isInitializing = !1;
          }).catch((l) => {
            console.error("Errore nella ricostruzione dell'editor:", l), this.isInitializing = !1;
          });
        } catch (s) {
          console.error("Errore durante l'aggiornamento dell'editor:", s), this.isInitializing = !1;
        }
    }
  }
  // Getter per il markdown attuale
  get markdown() {
    return this.editor && typeof this.editor.getMarkdown == "function" ? this.editor.getMarkdown() : this.getAttribute("markdown") || this.defaultMarkdown;
  }
  // Setter per il markdown
  set markdown(n) {
    this.setAttribute("markdown", n);
  }
  // Quando l'elemento viene aggiunto al DOM
  connectedCallback() {
    this.isInitializing = !0, this.shadowRoot = this.attachShadow({ mode: "open" });
    const n = document.createElement("link");
    n.setAttribute("rel", "stylesheet");
    let r;
    const i = document.currentScript;
    if (i && i.src) {
      const o = new URL(".", i.src).href;
      r = new URL("assets/css/milkdown-all.css", o).href, console.log("[DocsPilotElement] CSS Path calcolato (da currentScript):", r), console.log("[DocsPilotElement] document.currentScript.src:", i.src);
    } else {
      console.warn("[DocsPilotElement] Impossibile determinare il percorso dello script corrente. Tentativo con percorso fisso relativo alla pagina.");
      const o = "../../../milk_react/assets/css/milkdown-all.css";
      r = new URL(o, window.location.href).href, console.log("[DocsPilotElement] CSS Path (fallback con percorso fisso calcolato):", r);
    }
    n.setAttribute("href", r), this.shadowRoot.appendChild(n), this.editorContainer = document.createElement("div"), this.editorContainer.className = "docs-pilot-editor", this.shadowRoot.appendChild(this.editorContainer), setTimeout(() => {
      this.initEditor();
    }, 50);
  }
  // Configura gli eventi dell'editor
  setupEditorListeners() {
    this.editor && this.editor.on((n) => {
      n.markdownUpdated((r) => {
        let i;
        if (typeof r == "string")
          i = r;
        else if (r && typeof r == "object")
          try {
            if (r.toString && typeof r.toString == "function" && r.toString() !== "[object Object]")
              i = r.toString();
            else if (r.content && typeof r.content == "string")
              i = r.content;
            else if (r.text && typeof r.text == "string")
              i = r.text;
            else if (r.stringify && typeof r.stringify == "function")
              i = r.stringify();
            else if (r.constructor && r.constructor.name === "Jh") {
              i = "";
              return;
            } else
              console.warn("Impossibile convertire l'oggetto markdown in stringa:", r), i = "";
          } catch (l) {
            console.error("Errore durante la conversione dell'oggetto markdown:", l), i = "";
          }
        else
          console.warn("Markdown non è una stringa né un oggetto:", r), i = "";
        this.getAttribute("markdown") !== i && (this.isInitializing = !0, this.setAttribute("markdown", i), setTimeout(() => {
          this.isInitializing = !1;
        }, 10));
        const s = new CustomEvent("markdownChange", {
          detail: { markdown: i },
          bubbles: !0,
          composed: !0
        });
        this.dispatchEvent(s);
      });
    });
  }
  // Inizializza l'editor Milkdown
  async initEditor() {
    if (this.editorContainer)
      try {
        this.isInitializing = !0, console.log(
          "Inizializzazione editor con markdown:",
          this.markdown && this.markdown.substring(0, 20) + "..." || "vuoto"
        ), this.editor = new ua({
          root: this.editorContainer,
          defaultValue: this.markdown
        }), this.setupEditorListeners(), await this.editor.create(), console.log("Editor inizializzato con successo"), setTimeout(() => {
          this.isInitializing = !1;
        }, 50);
      } catch (n) {
        console.error("Errore nell'inizializzazione dell'editor Milkdown:", n), this.isInitializing = !1;
      }
  }
  // Quando l'elemento viene rimosso dal DOM
  disconnectedCallback() {
    this.editor && (this.editor.destroy(), this.editor = null);
  }
}
typeof window < "u" && typeof customElements < "u" && (customElements.get("docs-pilot") || customElements.define("docs-pilot", bg));
function PT(t) {
  const e = document.createElement("docs-pilot");
  return t.appendChild(e), e;
}
const zT = {
  // Utilizzo come WebComponent HTML
  html: `
    <!-- Inserisci direttamente il tag nel tuo HTML -->
    <docs-pilot></docs-pilot>
  `,
  // Utilizzo come script
  script: `
    <script type="module">
      import { initDocsPilot } from 'path/to/integration.js';
      
      // Inizializza DocsPilot in un elemento specifico
      const container = document.getElementById('container');
      initDocsPilot(container);
    <\/script>
  `,
  // Utilizzo con React
  react: `
    import DocsPilotWebComponent from 'path/to/DocsPilotWebComponent';
    
    function MyComponent() {
      return <DocsPilotWebComponent />;
    }
  `
}, JT = {
  DocsPilotElement: bg,
  initDocsPilot: PT,
  usage: zT
};
export {
  gt as $,
  wd as A,
  Un as B,
  gg as C,
  le as D,
  ye as E,
  T as F,
  Et as G,
  Jn as H,
  Bi as I,
  Ar as J,
  Hi as K,
  ln as L,
  wr as M,
  _ as N,
  X as O,
  Ne as P,
  Wa as Q,
  _a as R,
  E as S,
  H as T,
  Bc as U,
  qa as V,
  Rr as W,
  se as X,
  Ji as Y,
  qm as Z,
  Xm as _,
  KT as a,
  Qm as a0,
  Km as a1,
  Ym as a2,
  Gm as a3,
  Um as a4,
  Zm as a5,
  jm as a6,
  Hm as a7,
  _T as a8,
  E1 as a9,
  Ve as aa,
  Dd as ab,
  aC as ac,
  fr as ad,
  qd as ae,
  hy as af,
  cg as ag,
  ag as ah,
  Tw as ai,
  dn as aj,
  Jo as ak,
  fw as al,
  Ge as am,
  Qi as an,
  ud as ao,
  bg as ap,
  PT as aq,
  zT as ar,
  JT as as,
  Me as b,
  MC as c,
  T0 as d,
  $ as e,
  ot as f,
  ge as g,
  rw as h,
  Er as i,
  gr as j,
  Kd as k,
  At as l,
  Kn as m,
  HT as n,
  IT as o,
  qT as p,
  jT as q,
  bn as r,
  DT as s,
  Gn as t,
  _i as u,
  Vi as v,
  Or as w,
  Dr as x,
  $i as y,
  Lm as z
};
