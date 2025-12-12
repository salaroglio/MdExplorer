import { P as mt, E as T, F as H, V as Je, S as me, R as Gt, g as Ns, a as F, b as Oe, G as Rs, D as A, c as We, d as Jt, e as ai, l as Ot, W as Qt, f as Yi, h as Et, i as x, T as On, A as En, j as Dt, k as Ne, C as Fs, m as Ws, n as rt, s as Nn, o as ee, p as be, q as Rn, r as K, t as qs, u as Xi, v as zs, w as Zt, M as xn, x as $s, y as Us, z as _s, B as js, H as Hs, I as Vs, J as Ks, K as Gs, L as Js, N as Qs, O as Zs, Q as Ys } from "./index.es-Dganzb1x.js";
import { c as Xs, s as eo, a as to, e as no, h as er } from "./functions-Bsik6ikd-DhlhsyMB.js";
const io = 1024;
let ro = 0;
class ne {
  constructor(e, t) {
    this.from = e, this.to = t;
  }
}
class S {
  /**
  Create a new node prop type.
  */
  constructor(e = {}) {
    this.id = ro++, this.perNode = !!e.perNode, this.deserialize = e.deserialize || (() => {
      throw new Error("This node type doesn't define a deserialize function");
    });
  }
  /**
  This is meant to be used with
  [`NodeSet.extend`](#common.NodeSet.extend) or
  [`LRParser.configure`](#lr.ParserConfig.props) to compute
  prop values for each node type in the set. Takes a [match
  object](#common.NodeType^match) or function that returns undefined
  if the node type doesn't get this prop, and the prop's value if
  it does.
  */
  add(e) {
    if (this.perNode)
      throw new RangeError("Can't add per-node props to node types");
    return typeof e != "function" && (e = G.match(e)), (t) => {
      let i = e(t);
      return i === void 0 ? null : [this, i];
    };
  }
}
S.closedBy = new S({ deserialize: (n) => n.split(" ") });
S.openedBy = new S({ deserialize: (n) => n.split(" ") });
S.group = new S({ deserialize: (n) => n.split(" ") });
S.isolate = new S({ deserialize: (n) => {
  if (n && n != "rtl" && n != "ltr" && n != "auto")
    throw new RangeError("Invalid value for isolate: " + n);
  return n || "auto";
} });
S.contextHash = new S({ perNode: !0 });
S.lookAhead = new S({ perNode: !0 });
S.mounted = new S({ perNode: !0 });
class st {
  constructor(e, t, i) {
    this.tree = e, this.overlay = t, this.parser = i;
  }
  /**
  @internal
  */
  static get(e) {
    return e && e.props && e.props[S.mounted.id];
  }
}
const so = /* @__PURE__ */ Object.create(null);
class G {
  /**
  @internal
  */
  constructor(e, t, i, r = 0) {
    this.name = e, this.props = t, this.id = i, this.flags = r;
  }
  /**
  Define a node type.
  */
  static define(e) {
    let t = e.props && e.props.length ? /* @__PURE__ */ Object.create(null) : so, i = (e.top ? 1 : 0) | (e.skipped ? 2 : 0) | (e.error ? 4 : 0) | (e.name == null ? 8 : 0), r = new G(e.name || "", t, e.id, i);
    if (e.props) {
      for (let s of e.props)
        if (Array.isArray(s) || (s = s(r)), s) {
          if (s[0].perNode)
            throw new RangeError("Can't store a per-node prop on a node type");
          t[s[0].id] = s[1];
        }
    }
    return r;
  }
  /**
  Retrieves a node prop for this type. Will return `undefined` if
  the prop isn't present on this node.
  */
  prop(e) {
    return this.props[e.id];
  }
  /**
  True when this is the top node of a grammar.
  */
  get isTop() {
    return (this.flags & 1) > 0;
  }
  /**
  True when this node is produced by a skip rule.
  */
  get isSkipped() {
    return (this.flags & 2) > 0;
  }
  /**
  Indicates whether this is an error node.
  */
  get isError() {
    return (this.flags & 4) > 0;
  }
  /**
  When true, this node type doesn't correspond to a user-declared
  named node, for example because it is used to cache repetition.
  */
  get isAnonymous() {
    return (this.flags & 8) > 0;
  }
  /**
  Returns true when this node's name or one of its
  [groups](#common.NodeProp^group) matches the given string.
  */
  is(e) {
    if (typeof e == "string") {
      if (this.name == e)
        return !0;
      let t = this.prop(S.group);
      return t ? t.indexOf(e) > -1 : !1;
    }
    return this.id == e;
  }
  /**
  Create a function from node types to arbitrary values by
  specifying an object whose property names are node or
  [group](#common.NodeProp^group) names. Often useful with
  [`NodeProp.add`](#common.NodeProp.add). You can put multiple
  names, separated by spaces, in a single property name to map
  multiple node names to a single value.
  */
  static match(e) {
    let t = /* @__PURE__ */ Object.create(null);
    for (let i in e)
      for (let r of i.split(" "))
        t[r] = e[i];
    return (i) => {
      for (let r = i.prop(S.group), s = -1; s < (r ? r.length : 0); s++) {
        let o = t[s < 0 ? i.name : r[s]];
        if (o)
          return o;
      }
    };
  }
}
G.none = new G(
  "",
  /* @__PURE__ */ Object.create(null),
  0,
  8
  /* NodeFlag.Anonymous */
);
class Fn {
  /**
  Create a set with the given types. The `id` property of each
  type should correspond to its position within the array.
  */
  constructor(e) {
    this.types = e;
    for (let t = 0; t < e.length; t++)
      if (e[t].id != t)
        throw new RangeError("Node type ids should correspond to array positions when creating a node set");
  }
  /**
  Create a copy of this set with some node properties added. The
  arguments to this method can be created with
  [`NodeProp.add`](#common.NodeProp.add).
  */
  extend(...e) {
    let t = [];
    for (let i of this.types) {
      let r = null;
      for (let s of e) {
        let o = s(i);
        o && (r || (r = Object.assign({}, i.props)), r[o[0].id] = o[1]);
      }
      t.push(r ? new G(i.name, r, i.id, i.flags) : i);
    }
    return new Fn(t);
  }
}
const vt = /* @__PURE__ */ new WeakMap(), ci = /* @__PURE__ */ new WeakMap();
var R;
(function(n) {
  n[n.ExcludeBuffers = 1] = "ExcludeBuffers", n[n.IncludeAnonymous = 2] = "IncludeAnonymous", n[n.IgnoreMounts = 4] = "IgnoreMounts", n[n.IgnoreOverlays = 8] = "IgnoreOverlays";
})(R || (R = {}));
class O {
  /**
  Construct a new tree. See also [`Tree.build`](#common.Tree^build).
  */
  constructor(e, t, i, r, s) {
    if (this.type = e, this.children = t, this.positions = i, this.length = r, this.props = null, s && s.length) {
      this.props = /* @__PURE__ */ Object.create(null);
      for (let [o, l] of s)
        this.props[typeof o == "number" ? o : o.id] = l;
    }
  }
  /**
  @internal
  */
  toString() {
    let e = st.get(this);
    if (e && !e.overlay)
      return e.tree.toString();
    let t = "";
    for (let i of this.children) {
      let r = i.toString();
      r && (t && (t += ","), t += r);
    }
    return this.type.name ? (/\W/.test(this.type.name) && !this.type.isError ? JSON.stringify(this.type.name) : this.type.name) + (t.length ? "(" + t + ")" : "") : t;
  }
  /**
  Get a [tree cursor](#common.TreeCursor) positioned at the top of
  the tree. Mode can be used to [control](#common.IterMode) which
  nodes the cursor visits.
  */
  cursor(e = 0) {
    return new Nt(this.topNode, e);
  }
  /**
  Get a [tree cursor](#common.TreeCursor) pointing into this tree
  at the given position and side (see
  [`moveTo`](#common.TreeCursor.moveTo).
  */
  cursorAt(e, t = 0, i = 0) {
    let r = vt.get(this) || this.topNode, s = new Nt(r);
    return s.moveTo(e, t), vt.set(this, s._tree), s;
  }
  /**
  Get a [syntax node](#common.SyntaxNode) object for the top of the
  tree.
  */
  get topNode() {
    return new j(this, 0, 0, null);
  }
  /**
  Get the [syntax node](#common.SyntaxNode) at the given position.
  If `side` is -1, this will move into nodes that end at the
  position. If 1, it'll move into nodes that start at the
  position. With 0, it'll only enter nodes that cover the position
  from both sides.
  
  Note that this will not enter
  [overlays](#common.MountedTree.overlay), and you often want
  [`resolveInner`](#common.Tree.resolveInner) instead.
  */
  resolve(e, t = 0) {
    let i = ot(vt.get(this) || this.topNode, e, t, !1);
    return vt.set(this, i), i;
  }
  /**
  Like [`resolve`](#common.Tree.resolve), but will enter
  [overlaid](#common.MountedTree.overlay) nodes, producing a syntax node
  pointing into the innermost overlaid tree at the given position
  (with parent links going through all parent structure, including
  the host trees).
  */
  resolveInner(e, t = 0) {
    let i = ot(ci.get(this) || this.topNode, e, t, !0);
    return ci.set(this, i), i;
  }
  /**
  In some situations, it can be useful to iterate through all
  nodes around a position, including those in overlays that don't
  directly cover the position. This method gives you an iterator
  that will produce all nodes, from small to big, around the given
  position.
  */
  resolveStack(e, t = 0) {
    return ao(this, e, t);
  }
  /**
  Iterate over the tree and its children, calling `enter` for any
  node that touches the `from`/`to` region (if given) before
  running over such a node's children, and `leave` (if given) when
  leaving the node. When `enter` returns `false`, that node will
  not have its children iterated over (or `leave` called).
  */
  iterate(e) {
    let { enter: t, leave: i, from: r = 0, to: s = this.length } = e, o = e.mode || 0, l = (o & R.IncludeAnonymous) > 0;
    for (let a = this.cursor(o | R.IncludeAnonymous); ; ) {
      let c = !1;
      if (a.from <= s && a.to >= r && (!l && a.type.isAnonymous || t(a) !== !1)) {
        if (a.firstChild())
          continue;
        c = !0;
      }
      for (; c && i && (l || !a.type.isAnonymous) && i(a), !a.nextSibling(); ) {
        if (!a.parent())
          return;
        c = !0;
      }
    }
  }
  /**
  Get the value of the given [node prop](#common.NodeProp) for this
  node. Works with both per-node and per-type props.
  */
  prop(e) {
    return e.perNode ? this.props ? this.props[e.id] : void 0 : this.type.prop(e);
  }
  /**
  Returns the node's [per-node props](#common.NodeProp.perNode) in a
  format that can be passed to the [`Tree`](#common.Tree)
  constructor.
  */
  get propValues() {
    let e = [];
    if (this.props)
      for (let t in this.props)
        e.push([+t, this.props[t]]);
    return e;
  }
  /**
  Balance the direct children of this tree, producing a copy of
  which may have children grouped into subtrees with type
  [`NodeType.none`](#common.NodeType^none).
  */
  balance(e = {}) {
    return this.children.length <= 8 ? this : zn(G.none, this.children, this.positions, 0, this.children.length, 0, this.length, (t, i, r) => new O(this.type, t, i, r, this.propValues), e.makeTree || ((t, i, r) => new O(G.none, t, i, r)));
  }
  /**
  Build a tree from a postfix-ordered buffer of node information,
  or a cursor over such a buffer.
  */
  static build(e) {
    return co(e);
  }
}
O.empty = new O(G.none, [], [], 0);
class Wn {
  constructor(e, t) {
    this.buffer = e, this.index = t;
  }
  get id() {
    return this.buffer[this.index - 4];
  }
  get start() {
    return this.buffer[this.index - 3];
  }
  get end() {
    return this.buffer[this.index - 2];
  }
  get size() {
    return this.buffer[this.index - 1];
  }
  get pos() {
    return this.index;
  }
  next() {
    this.index -= 4;
  }
  fork() {
    return new Wn(this.buffer, this.index);
  }
}
class Ce {
  /**
  Create a tree buffer.
  */
  constructor(e, t, i) {
    this.buffer = e, this.length = t, this.set = i;
  }
  /**
  @internal
  */
  get type() {
    return G.none;
  }
  /**
  @internal
  */
  toString() {
    let e = [];
    for (let t = 0; t < this.buffer.length; )
      e.push(this.childString(t)), t = this.buffer[t + 3];
    return e.join(",");
  }
  /**
  @internal
  */
  childString(e) {
    let t = this.buffer[e], i = this.buffer[e + 3], r = this.set.types[t], s = r.name;
    if (/\W/.test(s) && !r.isError && (s = JSON.stringify(s)), e += 4, i == e)
      return s;
    let o = [];
    for (; e < i; )
      o.push(this.childString(e)), e = this.buffer[e + 3];
    return s + "(" + o.join(",") + ")";
  }
  /**
  @internal
  */
  findChild(e, t, i, r, s) {
    let { buffer: o } = this, l = -1;
    for (let a = e; a != t && !(tr(s, r, o[a + 1], o[a + 2]) && (l = a, i > 0)); a = o[a + 3])
      ;
    return l;
  }
  /**
  @internal
  */
  slice(e, t, i) {
    let r = this.buffer, s = new Uint16Array(t - e), o = 0;
    for (let l = e, a = 0; l < t; ) {
      s[a++] = r[l++], s[a++] = r[l++] - i;
      let c = s[a++] = r[l++] - i;
      s[a++] = r[l++] - e, o = Math.max(o, c);
    }
    return new Ce(s, o, this.set);
  }
}
function tr(n, e, t, i) {
  switch (n) {
    case -2:
      return t < e;
    case -1:
      return i >= e && t < e;
    case 0:
      return t < e && i > e;
    case 1:
      return t <= e && i > e;
    case 2:
      return i > e;
    case 4:
      return !0;
  }
}
function ot(n, e, t, i) {
  for (var r; n.from == n.to || (t < 1 ? n.from >= e : n.from > e) || (t > -1 ? n.to <= e : n.to < e); ) {
    let o = !i && n instanceof j && n.index < 0 ? null : n.parent;
    if (!o)
      return n;
    n = o;
  }
  let s = i ? 0 : R.IgnoreOverlays;
  if (i)
    for (let o = n, l = o.parent; l; o = l, l = o.parent)
      o instanceof j && o.index < 0 && ((r = l.enter(e, t, s)) === null || r === void 0 ? void 0 : r.from) != o.from && (n = l);
  for (; ; ) {
    let o = n.enter(e, t, s);
    if (!o)
      return n;
    n = o;
  }
}
class nr {
  cursor(e = 0) {
    return new Nt(this, e);
  }
  getChild(e, t = null, i = null) {
    let r = fi(this, e, t, i);
    return r.length ? r[0] : null;
  }
  getChildren(e, t = null, i = null) {
    return fi(this, e, t, i);
  }
  resolve(e, t = 0) {
    return ot(this, e, t, !1);
  }
  resolveInner(e, t = 0) {
    return ot(this, e, t, !0);
  }
  matchContext(e) {
    return kn(this.parent, e);
  }
  enterUnfinishedNodesBefore(e) {
    let t = this.childBefore(e), i = this;
    for (; t; ) {
      let r = t.lastChild;
      if (!r || r.to != t.to)
        break;
      r.type.isError && r.from == r.to ? (i = t, t = r.prevSibling) : t = r;
    }
    return i;
  }
  get node() {
    return this;
  }
  get next() {
    return this.parent;
  }
}
class j extends nr {
  constructor(e, t, i, r) {
    super(), this._tree = e, this.from = t, this.index = i, this._parent = r;
  }
  get type() {
    return this._tree.type;
  }
  get name() {
    return this._tree.type.name;
  }
  get to() {
    return this.from + this._tree.length;
  }
  nextChild(e, t, i, r, s = 0) {
    for (let o = this; ; ) {
      for (let { children: l, positions: a } = o._tree, c = t > 0 ? l.length : -1; e != c; e += t) {
        let f = l[e], h = a[e] + o.from;
        if (tr(r, i, h, h + f.length)) {
          if (f instanceof Ce) {
            if (s & R.ExcludeBuffers)
              continue;
            let u = f.findChild(0, f.buffer.length, t, i - h, r);
            if (u > -1)
              return new ue(new oo(o, f, e, h), null, u);
          } else if (s & R.IncludeAnonymous || !f.type.isAnonymous || qn(f)) {
            let u;
            if (!(s & R.IgnoreMounts) && (u = st.get(f)) && !u.overlay)
              return new j(u.tree, h, e, o);
            let d = new j(f, h, e, o);
            return s & R.IncludeAnonymous || !d.type.isAnonymous ? d : d.nextChild(t < 0 ? f.children.length - 1 : 0, t, i, r);
          }
        }
      }
      if (s & R.IncludeAnonymous || !o.type.isAnonymous || (o.index >= 0 ? e = o.index + t : e = t < 0 ? -1 : o._parent._tree.children.length, o = o._parent, !o))
        return null;
    }
  }
  get firstChild() {
    return this.nextChild(
      0,
      1,
      0,
      4
      /* Side.DontCare */
    );
  }
  get lastChild() {
    return this.nextChild(
      this._tree.children.length - 1,
      -1,
      0,
      4
      /* Side.DontCare */
    );
  }
  childAfter(e) {
    return this.nextChild(
      0,
      1,
      e,
      2
      /* Side.After */
    );
  }
  childBefore(e) {
    return this.nextChild(
      this._tree.children.length - 1,
      -1,
      e,
      -2
      /* Side.Before */
    );
  }
  enter(e, t, i = 0) {
    let r;
    if (!(i & R.IgnoreOverlays) && (r = st.get(this._tree)) && r.overlay) {
      let s = e - this.from;
      for (let { from: o, to: l } of r.overlay)
        if ((t > 0 ? o <= s : o < s) && (t < 0 ? l >= s : l > s))
          return new j(r.tree, r.overlay[0].from + this.from, -1, this);
    }
    return this.nextChild(0, 1, e, t, i);
  }
  nextSignificantParent() {
    let e = this;
    for (; e.type.isAnonymous && e._parent; )
      e = e._parent;
    return e;
  }
  get parent() {
    return this._parent ? this._parent.nextSignificantParent() : null;
  }
  get nextSibling() {
    return this._parent && this.index >= 0 ? this._parent.nextChild(
      this.index + 1,
      1,
      0,
      4
      /* Side.DontCare */
    ) : null;
  }
  get prevSibling() {
    return this._parent && this.index >= 0 ? this._parent.nextChild(
      this.index - 1,
      -1,
      0,
      4
      /* Side.DontCare */
    ) : null;
  }
  get tree() {
    return this._tree;
  }
  toTree() {
    return this._tree;
  }
  /**
  @internal
  */
  toString() {
    return this._tree.toString();
  }
}
function fi(n, e, t, i) {
  let r = n.cursor(), s = [];
  if (!r.firstChild())
    return s;
  if (t != null) {
    for (let o = !1; !o; )
      if (o = r.type.is(t), !r.nextSibling())
        return s;
  }
  for (; ; ) {
    if (i != null && r.type.is(i))
      return s;
    if (r.type.is(e) && s.push(r.node), !r.nextSibling())
      return i == null ? s : [];
  }
}
function kn(n, e, t = e.length - 1) {
  for (let i = n; t >= 0; i = i.parent) {
    if (!i)
      return !1;
    if (!i.type.isAnonymous) {
      if (e[t] && e[t] != i.name)
        return !1;
      t--;
    }
  }
  return !0;
}
class oo {
  constructor(e, t, i, r) {
    this.parent = e, this.buffer = t, this.index = i, this.start = r;
  }
}
class ue extends nr {
  get name() {
    return this.type.name;
  }
  get from() {
    return this.context.start + this.context.buffer.buffer[this.index + 1];
  }
  get to() {
    return this.context.start + this.context.buffer.buffer[this.index + 2];
  }
  constructor(e, t, i) {
    super(), this.context = e, this._parent = t, this.index = i, this.type = e.buffer.set.types[e.buffer.buffer[i]];
  }
  child(e, t, i) {
    let { buffer: r } = this.context, s = r.findChild(this.index + 4, r.buffer[this.index + 3], e, t - this.context.start, i);
    return s < 0 ? null : new ue(this.context, this, s);
  }
  get firstChild() {
    return this.child(
      1,
      0,
      4
      /* Side.DontCare */
    );
  }
  get lastChild() {
    return this.child(
      -1,
      0,
      4
      /* Side.DontCare */
    );
  }
  childAfter(e) {
    return this.child(
      1,
      e,
      2
      /* Side.After */
    );
  }
  childBefore(e) {
    return this.child(
      -1,
      e,
      -2
      /* Side.Before */
    );
  }
  enter(e, t, i = 0) {
    if (i & R.ExcludeBuffers)
      return null;
    let { buffer: r } = this.context, s = r.findChild(this.index + 4, r.buffer[this.index + 3], t > 0 ? 1 : -1, e - this.context.start, t);
    return s < 0 ? null : new ue(this.context, this, s);
  }
  get parent() {
    return this._parent || this.context.parent.nextSignificantParent();
  }
  externalSibling(e) {
    return this._parent ? null : this.context.parent.nextChild(
      this.context.index + e,
      e,
      0,
      4
      /* Side.DontCare */
    );
  }
  get nextSibling() {
    let { buffer: e } = this.context, t = e.buffer[this.index + 3];
    return t < (this._parent ? e.buffer[this._parent.index + 3] : e.buffer.length) ? new ue(this.context, this._parent, t) : this.externalSibling(1);
  }
  get prevSibling() {
    let { buffer: e } = this.context, t = this._parent ? this._parent.index + 4 : 0;
    return this.index == t ? this.externalSibling(-1) : new ue(this.context, this._parent, e.findChild(
      t,
      this.index,
      -1,
      0,
      4
      /* Side.DontCare */
    ));
  }
  get tree() {
    return null;
  }
  toTree() {
    let e = [], t = [], { buffer: i } = this.context, r = this.index + 4, s = i.buffer[this.index + 3];
    if (s > r) {
      let o = i.buffer[this.index + 1];
      e.push(i.slice(r, s, o)), t.push(0);
    }
    return new O(this.type, e, t, this.to - this.from);
  }
  /**
  @internal
  */
  toString() {
    return this.context.buffer.childString(this.index);
  }
}
function ir(n) {
  if (!n.length)
    return null;
  let e = 0, t = n[0];
  for (let s = 1; s < n.length; s++) {
    let o = n[s];
    (o.from > t.from || o.to < t.to) && (t = o, e = s);
  }
  let i = t instanceof j && t.index < 0 ? null : t.parent, r = n.slice();
  return i ? r[e] = i : r.splice(e, 1), new lo(r, t);
}
class lo {
  constructor(e, t) {
    this.heads = e, this.node = t;
  }
  get next() {
    return ir(this.heads);
  }
}
function ao(n, e, t) {
  let i = n.resolveInner(e, t), r = null;
  for (let s = i instanceof j ? i : i.context.parent; s; s = s.parent)
    if (s.index < 0) {
      let o = s.parent;
      (r || (r = [i])).push(o.resolve(e, t)), s = o;
    } else {
      let o = st.get(s.tree);
      if (o && o.overlay && o.overlay[0].from <= e && o.overlay[o.overlay.length - 1].to >= e) {
        let l = new j(o.tree, o.overlay[0].from + s.from, -1, s);
        (r || (r = [i])).push(ot(l, e, t, !1));
      }
    }
  return r ? ir(r) : i;
}
class Nt {
  /**
  Shorthand for `.type.name`.
  */
  get name() {
    return this.type.name;
  }
  /**
  @internal
  */
  constructor(e, t = 0) {
    if (this.mode = t, this.buffer = null, this.stack = [], this.index = 0, this.bufferNode = null, e instanceof j)
      this.yieldNode(e);
    else {
      this._tree = e.context.parent, this.buffer = e.context;
      for (let i = e._parent; i; i = i._parent)
        this.stack.unshift(i.index);
      this.bufferNode = e, this.yieldBuf(e.index);
    }
  }
  yieldNode(e) {
    return e ? (this._tree = e, this.type = e.type, this.from = e.from, this.to = e.to, !0) : !1;
  }
  yieldBuf(e, t) {
    this.index = e;
    let { start: i, buffer: r } = this.buffer;
    return this.type = t || r.set.types[r.buffer[e]], this.from = i + r.buffer[e + 1], this.to = i + r.buffer[e + 2], !0;
  }
  /**
  @internal
  */
  yield(e) {
    return e ? e instanceof j ? (this.buffer = null, this.yieldNode(e)) : (this.buffer = e.context, this.yieldBuf(e.index, e.type)) : !1;
  }
  /**
  @internal
  */
  toString() {
    return this.buffer ? this.buffer.buffer.childString(this.index) : this._tree.toString();
  }
  /**
  @internal
  */
  enterChild(e, t, i) {
    if (!this.buffer)
      return this.yield(this._tree.nextChild(e < 0 ? this._tree._tree.children.length - 1 : 0, e, t, i, this.mode));
    let { buffer: r } = this.buffer, s = r.findChild(this.index + 4, r.buffer[this.index + 3], e, t - this.buffer.start, i);
    return s < 0 ? !1 : (this.stack.push(this.index), this.yieldBuf(s));
  }
  /**
  Move the cursor to this node's first child. When this returns
  false, the node has no child, and the cursor has not been moved.
  */
  firstChild() {
    return this.enterChild(
      1,
      0,
      4
      /* Side.DontCare */
    );
  }
  /**
  Move the cursor to this node's last child.
  */
  lastChild() {
    return this.enterChild(
      -1,
      0,
      4
      /* Side.DontCare */
    );
  }
  /**
  Move the cursor to the first child that ends after `pos`.
  */
  childAfter(e) {
    return this.enterChild(
      1,
      e,
      2
      /* Side.After */
    );
  }
  /**
  Move to the last child that starts before `pos`.
  */
  childBefore(e) {
    return this.enterChild(
      -1,
      e,
      -2
      /* Side.Before */
    );
  }
  /**
  Move the cursor to the child around `pos`. If side is -1 the
  child may end at that position, when 1 it may start there. This
  will also enter [overlaid](#common.MountedTree.overlay)
  [mounted](#common.NodeProp^mounted) trees unless `overlays` is
  set to false.
  */
  enter(e, t, i = this.mode) {
    return this.buffer ? i & R.ExcludeBuffers ? !1 : this.enterChild(1, e, t) : this.yield(this._tree.enter(e, t, i));
  }
  /**
  Move to the node's parent node, if this isn't the top node.
  */
  parent() {
    if (!this.buffer)
      return this.yieldNode(this.mode & R.IncludeAnonymous ? this._tree._parent : this._tree.parent);
    if (this.stack.length)
      return this.yieldBuf(this.stack.pop());
    let e = this.mode & R.IncludeAnonymous ? this.buffer.parent : this.buffer.parent.nextSignificantParent();
    return this.buffer = null, this.yieldNode(e);
  }
  /**
  @internal
  */
  sibling(e) {
    if (!this.buffer)
      return this._tree._parent ? this.yield(this._tree.index < 0 ? null : this._tree._parent.nextChild(this._tree.index + e, e, 0, 4, this.mode)) : !1;
    let { buffer: t } = this.buffer, i = this.stack.length - 1;
    if (e < 0) {
      let r = i < 0 ? 0 : this.stack[i] + 4;
      if (this.index != r)
        return this.yieldBuf(t.findChild(
          r,
          this.index,
          -1,
          0,
          4
          /* Side.DontCare */
        ));
    } else {
      let r = t.buffer[this.index + 3];
      if (r < (i < 0 ? t.buffer.length : t.buffer[this.stack[i] + 3]))
        return this.yieldBuf(r);
    }
    return i < 0 ? this.yield(this.buffer.parent.nextChild(this.buffer.index + e, e, 0, 4, this.mode)) : !1;
  }
  /**
  Move to this node's next sibling, if any.
  */
  nextSibling() {
    return this.sibling(1);
  }
  /**
  Move to this node's previous sibling, if any.
  */
  prevSibling() {
    return this.sibling(-1);
  }
  atLastNode(e) {
    let t, i, { buffer: r } = this;
    if (r) {
      if (e > 0) {
        if (this.index < r.buffer.buffer.length)
          return !1;
      } else
        for (let s = 0; s < this.index; s++)
          if (r.buffer.buffer[s + 3] < this.index)
            return !1;
      ({ index: t, parent: i } = r);
    } else
      ({ index: t, _parent: i } = this._tree);
    for (; i; { index: t, _parent: i } = i)
      if (t > -1)
        for (let s = t + e, o = e < 0 ? -1 : i._tree.children.length; s != o; s += e) {
          let l = i._tree.children[s];
          if (this.mode & R.IncludeAnonymous || l instanceof Ce || !l.type.isAnonymous || qn(l))
            return !1;
        }
    return !0;
  }
  move(e, t) {
    if (t && this.enterChild(
      e,
      0,
      4
      /* Side.DontCare */
    ))
      return !0;
    for (; ; ) {
      if (this.sibling(e))
        return !0;
      if (this.atLastNode(e) || !this.parent())
        return !1;
    }
  }
  /**
  Move to the next node in a
  [pre-order](https://en.wikipedia.org/wiki/Tree_traversal#Pre-order,_NLR)
  traversal, going from a node to its first child or, if the
  current node is empty or `enter` is false, its next sibling or
  the next sibling of the first parent node that has one.
  */
  next(e = !0) {
    return this.move(1, e);
  }
  /**
  Move to the next node in a last-to-first pre-order traversal. A
  node is followed by its last child or, if it has none, its
  previous sibling or the previous sibling of the first parent
  node that has one.
  */
  prev(e = !0) {
    return this.move(-1, e);
  }
  /**
  Move the cursor to the innermost node that covers `pos`. If
  `side` is -1, it will enter nodes that end at `pos`. If it is 1,
  it will enter nodes that start at `pos`.
  */
  moveTo(e, t = 0) {
    for (; (this.from == this.to || (t < 1 ? this.from >= e : this.from > e) || (t > -1 ? this.to <= e : this.to < e)) && this.parent(); )
      ;
    for (; this.enterChild(1, e, t); )
      ;
    return this;
  }
  /**
  Get a [syntax node](#common.SyntaxNode) at the cursor's current
  position.
  */
  get node() {
    if (!this.buffer)
      return this._tree;
    let e = this.bufferNode, t = null, i = 0;
    if (e && e.context == this.buffer)
      e: for (let r = this.index, s = this.stack.length; s >= 0; ) {
        for (let o = e; o; o = o._parent)
          if (o.index == r) {
            if (r == this.index)
              return o;
            t = o, i = s + 1;
            break e;
          }
        r = this.stack[--s];
      }
    for (let r = i; r < this.stack.length; r++)
      t = new ue(this.buffer, t, this.stack[r]);
    return this.bufferNode = new ue(this.buffer, t, this.index);
  }
  /**
  Get the [tree](#common.Tree) that represents the current node, if
  any. Will return null when the node is in a [tree
  buffer](#common.TreeBuffer).
  */
  get tree() {
    return this.buffer ? null : this._tree._tree;
  }
  /**
  Iterate over the current node and all its descendants, calling
  `enter` when entering a node and `leave`, if given, when leaving
  one. When `enter` returns `false`, any children of that node are
  skipped, and `leave` isn't called for it.
  */
  iterate(e, t) {
    for (let i = 0; ; ) {
      let r = !1;
      if (this.type.isAnonymous || e(this) !== !1) {
        if (this.firstChild()) {
          i++;
          continue;
        }
        this.type.isAnonymous || (r = !0);
      }
      for (; ; ) {
        if (r && t && t(this), r = this.type.isAnonymous, !i)
          return;
        if (this.nextSibling())
          break;
        this.parent(), i--, r = !0;
      }
    }
  }
  /**
  Test whether the current node matches a given context—a sequence
  of direct parent node names. Empty strings in the context array
  are treated as wildcards.
  */
  matchContext(e) {
    if (!this.buffer)
      return kn(this.node.parent, e);
    let { buffer: t } = this.buffer, { types: i } = t.set;
    for (let r = e.length - 1, s = this.stack.length - 1; r >= 0; s--) {
      if (s < 0)
        return kn(this._tree, e, r);
      let o = i[t.buffer[this.stack[s]]];
      if (!o.isAnonymous) {
        if (e[r] && e[r] != o.name)
          return !1;
        r--;
      }
    }
    return !0;
  }
}
function qn(n) {
  return n.children.some((e) => e instanceof Ce || !e.type.isAnonymous || qn(e));
}
function co(n) {
  var e;
  let { buffer: t, nodeSet: i, maxBufferLength: r = io, reused: s = [], minRepeatType: o = i.types.length } = n, l = Array.isArray(t) ? new Wn(t, t.length) : t, a = i.types, c = 0, f = 0;
  function h(k, B, b, q, L, z) {
    let { id: M, start: w, end: E, size: N } = l, U = f, ke = c;
    for (; N < 0; )
      if (l.next(), N == -1) {
        let ye = s[M];
        b.push(ye), q.push(w - k);
        return;
      } else if (N == -3) {
        c = M;
        return;
      } else if (N == -4) {
        f = M;
        return;
      } else
        throw new RangeError(`Unrecognized record size: ${N}`);
    let Xe = a[M], kt, Te, oi = w - k;
    if (E - w <= r && (Te = C(l.pos - B, L))) {
      let ye = new Uint16Array(Te.size - Te.skip), X = l.pos - Te.size, ae = ye.length;
      for (; l.pos > X; )
        ae = P(Te.start, ye, ae);
      kt = new Ce(ye, E - Te.start, i), oi = Te.start - k;
    } else {
      let ye = l.pos - N;
      l.next();
      let X = [], ae = [], Ie = M >= o ? M : -1, qe = 0, wt = E;
      for (; l.pos > ye; )
        Ie >= 0 && l.id == Ie && l.size >= 0 ? (l.end <= wt - r && (y(X, ae, w, qe, l.end, wt, Ie, U, ke), qe = X.length, wt = l.end), l.next()) : z > 2500 ? u(w, ye, X, ae) : h(w, ye, X, ae, Ie, z + 1);
      if (Ie >= 0 && qe > 0 && qe < X.length && y(X, ae, w, qe, w, wt, Ie, U, ke), X.reverse(), ae.reverse(), Ie > -1 && qe > 0) {
        let li = d(Xe, ke);
        kt = zn(Xe, X, ae, 0, X.length, 0, E - w, li, li);
      } else
        kt = g(Xe, X, ae, E - w, U - E, ke);
    }
    b.push(kt), q.push(oi);
  }
  function u(k, B, b, q) {
    let L = [], z = 0, M = -1;
    for (; l.pos > B; ) {
      let { id: w, start: E, end: N, size: U } = l;
      if (U > 4)
        l.next();
      else {
        if (M > -1 && E < M)
          break;
        M < 0 && (M = N - r), L.push(w, E, N), z++, l.next();
      }
    }
    if (z) {
      let w = new Uint16Array(z * 4), E = L[L.length - 2];
      for (let N = L.length - 3, U = 0; N >= 0; N -= 3)
        w[U++] = L[N], w[U++] = L[N + 1] - E, w[U++] = L[N + 2] - E, w[U++] = U;
      b.push(new Ce(w, L[2] - E, i)), q.push(E - k);
    }
  }
  function d(k, B) {
    return (b, q, L) => {
      let z = 0, M = b.length - 1, w, E;
      if (M >= 0 && (w = b[M]) instanceof O) {
        if (!M && w.type == k && w.length == L)
          return w;
        (E = w.prop(S.lookAhead)) && (z = q[M] + w.length + E);
      }
      return g(k, b, q, L, z, B);
    };
  }
  function y(k, B, b, q, L, z, M, w, E) {
    let N = [], U = [];
    for (; k.length > q; )
      N.push(k.pop()), U.push(B.pop() + b - L);
    k.push(g(i.types[M], N, U, z - L, w - z, E)), B.push(L - b);
  }
  function g(k, B, b, q, L, z, M) {
    if (z) {
      let w = [S.contextHash, z];
      M = M ? [w].concat(M) : [w];
    }
    if (L > 25) {
      let w = [S.lookAhead, L];
      M = M ? [w].concat(M) : [w];
    }
    return new O(k, B, b, q, M);
  }
  function C(k, B) {
    let b = l.fork(), q = 0, L = 0, z = 0, M = b.end - r, w = { size: 0, start: 0, skip: 0 };
    e: for (let E = b.pos - k; b.pos > E; ) {
      let N = b.size;
      if (b.id == B && N >= 0) {
        w.size = q, w.start = L, w.skip = z, z += 4, q += 4, b.next();
        continue;
      }
      let U = b.pos - N;
      if (N < 0 || U < E || b.start < M)
        break;
      let ke = b.id >= o ? 4 : 0, Xe = b.start;
      for (b.next(); b.pos > U; ) {
        if (b.size < 0)
          if (b.size == -3)
            ke += 4;
          else
            break e;
        else b.id >= o && (ke += 4);
        b.next();
      }
      L = Xe, q += N, z += ke;
    }
    return (B < 0 || q == k) && (w.size = q, w.start = L, w.skip = z), w.size > 4 ? w : void 0;
  }
  function P(k, B, b) {
    let { id: q, start: L, end: z, size: M } = l;
    if (l.next(), M >= 0 && q < o) {
      let w = b;
      if (M > 4) {
        let E = l.pos - (M - 4);
        for (; l.pos > E; )
          b = P(k, B, b);
      }
      B[--b] = w, B[--b] = z - k, B[--b] = L - k, B[--b] = q;
    } else M == -3 ? c = q : M == -4 && (f = q);
    return b;
  }
  let v = [], I = [];
  for (; l.pos > 0; )
    h(n.start || 0, n.bufferStart || 0, v, I, -1, 0);
  let W = (e = n.length) !== null && e !== void 0 ? e : v.length ? I[0] + v[0].length : 0;
  return new O(a[n.topID], v.reverse(), I.reverse(), W);
}
const hi = /* @__PURE__ */ new WeakMap();
function Lt(n, e) {
  if (!n.isAnonymous || e instanceof Ce || e.type != n)
    return 1;
  let t = hi.get(e);
  if (t == null) {
    t = 1;
    for (let i of e.children) {
      if (i.type != n || !(i instanceof O)) {
        t = 1;
        break;
      }
      t += Lt(n, i);
    }
    hi.set(e, t);
  }
  return t;
}
function zn(n, e, t, i, r, s, o, l, a) {
  let c = 0;
  for (let y = i; y < r; y++)
    c += Lt(n, e[y]);
  let f = Math.ceil(
    c * 1.5 / 8
    /* Balance.BranchFactor */
  ), h = [], u = [];
  function d(y, g, C, P, v) {
    for (let I = C; I < P; ) {
      let W = I, k = g[I], B = Lt(n, y[I]);
      for (I++; I < P; I++) {
        let b = Lt(n, y[I]);
        if (B + b >= f)
          break;
        B += b;
      }
      if (I == W + 1) {
        if (B > f) {
          let b = y[W];
          d(b.children, b.positions, 0, b.children.length, g[W] + v);
          continue;
        }
        h.push(y[W]);
      } else {
        let b = g[I - 1] + y[I - 1].length - k;
        h.push(zn(n, y, g, W, I, k, b, null, a));
      }
      u.push(k + v - s);
    }
  }
  return d(e, t, i, r, 0), (l || a)(h, u, o);
}
class wf {
  constructor() {
    this.map = /* @__PURE__ */ new WeakMap();
  }
  setBuffer(e, t, i) {
    let r = this.map.get(e);
    r || this.map.set(e, r = /* @__PURE__ */ new Map()), r.set(t, i);
  }
  getBuffer(e, t) {
    let i = this.map.get(e);
    return i && i.get(t);
  }
  /**
  Set the value for this syntax node.
  */
  set(e, t) {
    e instanceof ue ? this.setBuffer(e.context.buffer, e.index, t) : e instanceof j && this.map.set(e.tree, t);
  }
  /**
  Retrieve value for this syntax node, if it exists in the map.
  */
  get(e) {
    return e instanceof ue ? this.getBuffer(e.context.buffer, e.index) : e instanceof j ? this.map.get(e.tree) : void 0;
  }
  /**
  Set the value for the node that a cursor currently points to.
  */
  cursorSet(e, t) {
    e.buffer ? this.setBuffer(e.buffer.buffer, e.index, t) : this.map.set(e.tree, t);
  }
  /**
  Retrieve the value for the node that a cursor currently points
  to.
  */
  cursorGet(e) {
    return e.buffer ? this.getBuffer(e.buffer.buffer, e.index) : this.map.get(e.tree);
  }
}
class xe {
  /**
  Construct a tree fragment. You'll usually want to use
  [`addTree`](#common.TreeFragment^addTree) and
  [`applyChanges`](#common.TreeFragment^applyChanges) instead of
  calling this directly.
  */
  constructor(e, t, i, r, s = !1, o = !1) {
    this.from = e, this.to = t, this.tree = i, this.offset = r, this.open = (s ? 1 : 0) | (o ? 2 : 0);
  }
  /**
  Whether the start of the fragment represents the start of a
  parse, or the end of a change. (In the second case, it may not
  be safe to reuse some nodes at the start, depending on the
  parsing algorithm.)
  */
  get openStart() {
    return (this.open & 1) > 0;
  }
  /**
  Whether the end of the fragment represents the end of a
  full-document parse, or the start of a change.
  */
  get openEnd() {
    return (this.open & 2) > 0;
  }
  /**
  Create a set of fragments from a freshly parsed tree, or update
  an existing set of fragments by replacing the ones that overlap
  with a tree with content from the new tree. When `partial` is
  true, the parse is treated as incomplete, and the resulting
  fragment has [`openEnd`](#common.TreeFragment.openEnd) set to
  true.
  */
  static addTree(e, t = [], i = !1) {
    let r = [new xe(0, e.length, e, 0, !1, i)];
    for (let s of t)
      s.to > e.length && r.push(s);
    return r;
  }
  /**
  Apply a set of edits to an array of fragments, removing or
  splitting fragments as necessary to remove edited ranges, and
  adjusting offsets for fragments that moved.
  */
  static applyChanges(e, t, i = 128) {
    if (!t.length)
      return e;
    let r = [], s = 1, o = e.length ? e[0] : null;
    for (let l = 0, a = 0, c = 0; ; l++) {
      let f = l < t.length ? t[l] : null, h = f ? f.fromA : 1e9;
      if (h - a >= i)
        for (; o && o.from < h; ) {
          let u = o;
          if (a >= u.from || h <= u.to || c) {
            let d = Math.max(u.from, a) - c, y = Math.min(u.to, h) - c;
            u = d >= y ? null : new xe(d, y, u.tree, u.offset + c, l > 0, !!f);
          }
          if (u && r.push(u), o.to > h)
            break;
          o = s < e.length ? e[s++] : null;
        }
      if (!f)
        break;
      a = f.toA, c = f.toA - f.toB;
    }
    return r;
  }
}
class rr {
  /**
  Start a parse, returning a [partial parse](#common.PartialParse)
  object. [`fragments`](#common.TreeFragment) can be passed in to
  make the parse incremental.
  
  By default, the entire input is parsed. You can pass `ranges`,
  which should be a sorted array of non-empty, non-overlapping
  ranges, to parse only those ranges. The tree returned in that
  case will start at `ranges[0].from`.
  */
  startParse(e, t, i) {
    return typeof e == "string" && (e = new fo(e)), i = i ? i.length ? i.map((r) => new ne(r.from, r.to)) : [new ne(0, 0)] : [new ne(0, e.length)], this.createParse(e, t || [], i);
  }
  /**
  Run a full parse, returning the resulting tree.
  */
  parse(e, t, i) {
    let r = this.startParse(e, t, i);
    for (; ; ) {
      let s = r.advance();
      if (s)
        return s;
    }
  }
}
class fo {
  constructor(e) {
    this.string = e;
  }
  get length() {
    return this.string.length;
  }
  chunk(e) {
    return this.string.slice(e);
  }
  get lineChunks() {
    return !1;
  }
  read(e, t) {
    return this.string.slice(e, t);
  }
}
function vf(n) {
  return (e, t, i, r) => new uo(e, n, t, i, r);
}
class ui {
  constructor(e, t, i, r, s) {
    this.parser = e, this.parse = t, this.overlay = i, this.target = r, this.from = s;
  }
}
function di(n) {
  if (!n.length || n.some((e) => e.from >= e.to))
    throw new RangeError("Invalid inner parse ranges given: " + JSON.stringify(n));
}
class ho {
  constructor(e, t, i, r, s, o, l) {
    this.parser = e, this.predicate = t, this.mounts = i, this.index = r, this.start = s, this.target = o, this.prev = l, this.depth = 0, this.ranges = [];
  }
}
const wn = new S({ perNode: !0 });
class uo {
  constructor(e, t, i, r, s) {
    this.nest = t, this.input = i, this.fragments = r, this.ranges = s, this.inner = [], this.innerDone = 0, this.baseTree = null, this.stoppedAt = null, this.baseParse = e;
  }
  advance() {
    if (this.baseParse) {
      let i = this.baseParse.advance();
      if (!i)
        return null;
      if (this.baseParse = null, this.baseTree = i, this.startInner(), this.stoppedAt != null)
        for (let r of this.inner)
          r.parse.stopAt(this.stoppedAt);
    }
    if (this.innerDone == this.inner.length) {
      let i = this.baseTree;
      return this.stoppedAt != null && (i = new O(i.type, i.children, i.positions, i.length, i.propValues.concat([[wn, this.stoppedAt]]))), i;
    }
    let e = this.inner[this.innerDone], t = e.parse.advance();
    if (t) {
      this.innerDone++;
      let i = Object.assign(/* @__PURE__ */ Object.create(null), e.target.props);
      i[S.mounted.id] = new st(t, e.overlay, e.parser), e.target.props = i;
    }
    return null;
  }
  get parsedPos() {
    if (this.baseParse)
      return 0;
    let e = this.input.length;
    for (let t = this.innerDone; t < this.inner.length; t++)
      this.inner[t].from < e && (e = Math.min(e, this.inner[t].parse.parsedPos));
    return e;
  }
  stopAt(e) {
    if (this.stoppedAt = e, this.baseParse)
      this.baseParse.stopAt(e);
    else
      for (let t = this.innerDone; t < this.inner.length; t++)
        this.inner[t].parse.stopAt(e);
  }
  startInner() {
    let e = new go(this.fragments), t = null, i = null, r = new Nt(new j(this.baseTree, this.ranges[0].from, 0, null), R.IncludeAnonymous | R.IgnoreMounts);
    e: for (let s, o; ; ) {
      let l = !0, a;
      if (this.stoppedAt != null && r.from >= this.stoppedAt)
        l = !1;
      else if (e.hasNode(r)) {
        if (t) {
          let c = t.mounts.find((f) => f.frag.from <= r.from && f.frag.to >= r.to && f.mount.overlay);
          if (c)
            for (let f of c.mount.overlay) {
              let h = f.from + c.pos, u = f.to + c.pos;
              h >= r.from && u <= r.to && !t.ranges.some((d) => d.from < u && d.to > h) && t.ranges.push({ from: h, to: u });
            }
        }
        l = !1;
      } else if (i && (o = po(i.ranges, r.from, r.to)))
        l = o != 2;
      else if (!r.type.isAnonymous && (s = this.nest(r, this.input)) && (r.from < r.to || !s.overlay)) {
        r.tree || mo(r);
        let c = e.findMounts(r.from, s.parser);
        if (typeof s.overlay == "function")
          t = new ho(s.parser, s.overlay, c, this.inner.length, r.from, r.tree, t);
        else {
          let f = gi(this.ranges, s.overlay || (r.from < r.to ? [new ne(r.from, r.to)] : []));
          f.length && di(f), (f.length || !s.overlay) && this.inner.push(new ui(s.parser, f.length ? s.parser.startParse(this.input, yi(c, f), f) : s.parser.startParse(""), s.overlay ? s.overlay.map((h) => new ne(h.from - r.from, h.to - r.from)) : null, r.tree, f.length ? f[0].from : r.from)), s.overlay ? f.length && (i = { ranges: f, depth: 0, prev: i }) : l = !1;
        }
      } else if (t && (a = t.predicate(r)) && (a === !0 && (a = new ne(r.from, r.to)), a.from < a.to)) {
        let c = t.ranges.length - 1;
        c >= 0 && t.ranges[c].to == a.from ? t.ranges[c] = { from: t.ranges[c].from, to: a.to } : t.ranges.push(a);
      }
      if (l && r.firstChild())
        t && t.depth++, i && i.depth++;
      else
        for (; !r.nextSibling(); ) {
          if (!r.parent())
            break e;
          if (t && !--t.depth) {
            let c = gi(this.ranges, t.ranges);
            c.length && (di(c), this.inner.splice(t.index, 0, new ui(t.parser, t.parser.startParse(this.input, yi(t.mounts, c), c), t.ranges.map((f) => new ne(f.from - t.start, f.to - t.start)), t.target, c[0].from))), t = t.prev;
          }
          i && !--i.depth && (i = i.prev);
        }
    }
  }
}
function po(n, e, t) {
  for (let i of n) {
    if (i.from >= t)
      break;
    if (i.to > e)
      return i.from <= e && i.to >= t ? 2 : 1;
  }
  return 0;
}
function pi(n, e, t, i, r, s) {
  if (e < t) {
    let o = n.buffer[e + 1];
    i.push(n.slice(e, t, o)), r.push(o - s);
  }
}
function mo(n) {
  let { node: e } = n, t = [], i = e.context.buffer;
  do
    t.push(n.index), n.parent();
  while (!n.tree);
  let r = n.tree, s = r.children.indexOf(i), o = r.children[s], l = o.buffer, a = [s];
  function c(f, h, u, d, y, g) {
    let C = t[g], P = [], v = [];
    pi(o, f, C, P, v, d);
    let I = l[C + 1], W = l[C + 2];
    a.push(P.length);
    let k = g ? c(C + 4, l[C + 3], o.set.types[l[C]], I, W - I, g - 1) : e.toTree();
    return P.push(k), v.push(I - d), pi(o, l[C + 3], h, P, v, d), new O(u, P, v, y);
  }
  r.children[s] = c(0, l.length, G.none, 0, o.length, t.length - 1);
  for (let f of a) {
    let h = n.tree.children[f], u = n.tree.positions[f];
    n.yield(new j(h, u + n.from, f, n._tree));
  }
}
class mi {
  constructor(e, t) {
    this.offset = t, this.done = !1, this.cursor = e.cursor(R.IncludeAnonymous | R.IgnoreMounts);
  }
  // Move to the first node (in pre-order) that starts at or after `pos`.
  moveTo(e) {
    let { cursor: t } = this, i = e - this.offset;
    for (; !this.done && t.from < i; )
      t.to >= e && t.enter(i, 1, R.IgnoreOverlays | R.ExcludeBuffers) || t.next(!1) || (this.done = !0);
  }
  hasNode(e) {
    if (this.moveTo(e.from), !this.done && this.cursor.from + this.offset == e.from && this.cursor.tree)
      for (let t = this.cursor.tree; ; ) {
        if (t == e.tree)
          return !0;
        if (t.children.length && t.positions[0] == 0 && t.children[0] instanceof O)
          t = t.children[0];
        else
          break;
      }
    return !1;
  }
}
class go {
  constructor(e) {
    var t;
    if (this.fragments = e, this.curTo = 0, this.fragI = 0, e.length) {
      let i = this.curFrag = e[0];
      this.curTo = (t = i.tree.prop(wn)) !== null && t !== void 0 ? t : i.to, this.inner = new mi(i.tree, -i.offset);
    } else
      this.curFrag = this.inner = null;
  }
  hasNode(e) {
    for (; this.curFrag && e.from >= this.curTo; )
      this.nextFrag();
    return this.curFrag && this.curFrag.from <= e.from && this.curTo >= e.to && this.inner.hasNode(e);
  }
  nextFrag() {
    var e;
    if (this.fragI++, this.fragI == this.fragments.length)
      this.curFrag = this.inner = null;
    else {
      let t = this.curFrag = this.fragments[this.fragI];
      this.curTo = (e = t.tree.prop(wn)) !== null && e !== void 0 ? e : t.to, this.inner = new mi(t.tree, -t.offset);
    }
  }
  findMounts(e, t) {
    var i;
    let r = [];
    if (this.inner) {
      this.inner.cursor.moveTo(e, 1);
      for (let s = this.inner.cursor.node; s; s = s.parent) {
        let o = (i = s.tree) === null || i === void 0 ? void 0 : i.prop(S.mounted);
        if (o && o.parser == t)
          for (let l = this.fragI; l < this.fragments.length; l++) {
            let a = this.fragments[l];
            if (a.from >= s.to)
              break;
            a.tree == this.curFrag.tree && r.push({
              frag: a,
              pos: s.from - a.offset,
              mount: o
            });
          }
      }
    }
    return r;
  }
}
function gi(n, e) {
  let t = null, i = e;
  for (let r = 1, s = 0; r < n.length; r++) {
    let o = n[r - 1].to, l = n[r].from;
    for (; s < i.length; s++) {
      let a = i[s];
      if (a.from >= l)
        break;
      a.to <= o || (t || (i = t = e.slice()), a.from < o ? (t[s] = new ne(a.from, o), a.to > l && t.splice(s + 1, 0, new ne(l, a.to))) : a.to > l ? t[s--] = new ne(l, a.to) : t.splice(s--, 1));
    }
  }
  return i;
}
function yo(n, e, t, i) {
  let r = 0, s = 0, o = !1, l = !1, a = -1e9, c = [];
  for (; ; ) {
    let f = r == n.length ? 1e9 : o ? n[r].to : n[r].from, h = s == e.length ? 1e9 : l ? e[s].to : e[s].from;
    if (o != l) {
      let u = Math.max(a, t), d = Math.min(f, h, i);
      u < d && c.push(new ne(u, d));
    }
    if (a = Math.min(f, h), a == 1e9)
      break;
    f == a && (o ? (o = !1, r++) : o = !0), h == a && (l ? (l = !1, s++) : l = !0);
  }
  return c;
}
function yi(n, e) {
  let t = [];
  for (let { pos: i, mount: r, frag: s } of n) {
    let o = i + (r.overlay ? r.overlay[0].from : 0), l = o + r.tree.length, a = Math.max(s.from, o), c = Math.min(s.to, l);
    if (r.overlay) {
      let f = r.overlay.map((u) => new ne(u.from + i, u.to + i)), h = yo(e, f, a, c);
      for (let u = 0, d = a; ; u++) {
        let y = u == h.length, g = y ? c : h[u].from;
        if (g > d && t.push(new xe(d, g, r.tree, -o, s.from >= d || s.openStart, s.to <= g || s.openEnd)), y)
          break;
        d = h[u].to;
      }
    } else
      t.push(new xe(a, c, r.tree, -o, s.from >= o || s.openStart, s.to <= l || s.openEnd));
  }
  return t;
}
let bo = 0;
class te {
  /**
  @internal
  */
  constructor(e, t, i, r) {
    this.name = e, this.set = t, this.base = i, this.modified = r, this.id = bo++;
  }
  toString() {
    let { name: e } = this;
    for (let t of this.modified)
      t.name && (e = `${t.name}(${e})`);
    return e;
  }
  static define(e, t) {
    let i = typeof e == "string" ? e : "?";
    if (e instanceof te && (t = e), t != null && t.base)
      throw new Error("Can not derive from a modified tag");
    let r = new te(i, [], null, []);
    if (r.set.push(r), t)
      for (let s of t.set)
        r.set.push(s);
    return r;
  }
  /**
  Define a tag _modifier_, which is a function that, given a tag,
  will return a tag that is a subtag of the original. Applying the
  same modifier to a twice tag will return the same value (`m1(t1)
  == m1(t1)`) and applying multiple modifiers will, regardless or
  order, produce the same tag (`m1(m2(t1)) == m2(m1(t1))`).
  
  When multiple modifiers are applied to a given base tag, each
  smaller set of modifiers is registered as a parent, so that for
  example `m1(m2(m3(t1)))` is a subtype of `m1(m2(t1))`,
  `m1(m3(t1)`, and so on.
  */
  static defineModifier(e) {
    let t = new Rt(e);
    return (i) => i.modified.indexOf(t) > -1 ? i : Rt.get(i.base || i, i.modified.concat(t).sort((r, s) => r.id - s.id));
  }
}
let xo = 0;
class Rt {
  constructor(e) {
    this.name = e, this.instances = [], this.id = xo++;
  }
  static get(e, t) {
    if (!t.length)
      return e;
    let i = t[0].instances.find((l) => l.base == e && ko(t, l.modified));
    if (i)
      return i;
    let r = [], s = new te(e.name, r, e, t);
    for (let l of t)
      l.instances.push(s);
    let o = wo(t);
    for (let l of e.set)
      if (!l.modified.length)
        for (let a of o)
          r.push(Rt.get(l, a));
    return s;
  }
}
function ko(n, e) {
  return n.length == e.length && n.every((t, i) => t == e[i]);
}
function wo(n) {
  let e = [[]];
  for (let t = 0; t < n.length; t++)
    for (let i = 0, r = e.length; i < r; i++)
      e.push(e[i].concat(n[t]));
  return e.sort((t, i) => i.length - t.length);
}
function vo(n) {
  let e = /* @__PURE__ */ Object.create(null);
  for (let t in n) {
    let i = n[t];
    Array.isArray(i) || (i = [i]);
    for (let r of t.split(" "))
      if (r) {
        let s = [], o = 2, l = r;
        for (let h = 0; ; ) {
          if (l == "..." && h > 0 && h + 3 == r.length) {
            o = 1;
            break;
          }
          let u = /^"(?:[^"\\]|\\.)*?"|[^\/!]+/.exec(l);
          if (!u)
            throw new RangeError("Invalid path: " + r);
          if (s.push(u[0] == "*" ? "" : u[0][0] == '"' ? JSON.parse(u[0]) : u[0]), h += u[0].length, h == r.length)
            break;
          let d = r[h++];
          if (h == r.length && d == "!") {
            o = 0;
            break;
          }
          if (d != "/")
            throw new RangeError("Invalid path: " + r);
          l = r.slice(h);
        }
        let a = s.length - 1, c = s[a];
        if (!c)
          throw new RangeError("Invalid path: " + r);
        let f = new Ft(i, o, a > 0 ? s.slice(0, a) : null);
        e[c] = f.sort(e[c]);
      }
  }
  return sr.add(e);
}
const sr = new S();
class Ft {
  constructor(e, t, i, r) {
    this.tags = e, this.mode = t, this.context = i, this.next = r;
  }
  get opaque() {
    return this.mode == 0;
  }
  get inherit() {
    return this.mode == 1;
  }
  sort(e) {
    return !e || e.depth < this.depth ? (this.next = e, this) : (e.next = this.sort(e.next), e);
  }
  get depth() {
    return this.context ? this.context.length : 0;
  }
}
Ft.empty = new Ft([], 2, null);
function or(n, e) {
  let t = /* @__PURE__ */ Object.create(null);
  for (let s of n)
    if (!Array.isArray(s.tag))
      t[s.tag.id] = s.class;
    else
      for (let o of s.tag)
        t[o.id] = s.class;
  let { scope: i, all: r = null } = e || {};
  return {
    style: (s) => {
      let o = r;
      for (let l of s)
        for (let a of l.set) {
          let c = t[a.id];
          if (c) {
            o = o ? o + " " + c : c;
            break;
          }
        }
      return o;
    },
    scope: i
  };
}
function So(n, e) {
  let t = null;
  for (let i of n) {
    let r = i.style(e);
    r && (t = t ? t + " " + r : r);
  }
  return t;
}
function Co(n, e, t, i = 0, r = n.length) {
  let s = new Ao(i, Array.isArray(e) ? e : [e], t);
  s.highlightRange(n.cursor(), i, r, "", s.highlighters), s.flush(r);
}
class Ao {
  constructor(e, t, i) {
    this.at = e, this.highlighters = t, this.span = i, this.class = "";
  }
  startSpan(e, t) {
    t != this.class && (this.flush(e), e > this.at && (this.at = e), this.class = t);
  }
  flush(e) {
    e > this.at && this.class && this.span(this.at, e, this.class);
  }
  highlightRange(e, t, i, r, s) {
    let { type: o, from: l, to: a } = e;
    if (l >= i || a <= t)
      return;
    o.isTop && (s = this.highlighters.filter((d) => !d.scope || d.scope(o)));
    let c = r, f = Mo(e) || Ft.empty, h = So(s, f.tags);
    if (h && (c && (c += " "), c += h, f.mode == 1 && (r += (r ? " " : "") + h)), this.startSpan(Math.max(t, l), c), f.opaque)
      return;
    let u = e.tree && e.tree.prop(S.mounted);
    if (u && u.overlay) {
      let d = e.node.enter(u.overlay[0].from + l, 1), y = this.highlighters.filter((C) => !C.scope || C.scope(u.tree.type)), g = e.firstChild();
      for (let C = 0, P = l; ; C++) {
        let v = C < u.overlay.length ? u.overlay[C] : null, I = v ? v.from + l : a, W = Math.max(t, P), k = Math.min(i, I);
        if (W < k && g)
          for (; e.from < k && (this.highlightRange(e, W, k, r, s), this.startSpan(Math.min(k, e.to), c), !(e.to >= I || !e.nextSibling())); )
            ;
        if (!v || I > i)
          break;
        P = v.to + l, P > t && (this.highlightRange(d.cursor(), Math.max(t, v.from + l), Math.min(i, P), "", y), this.startSpan(Math.min(i, P), c));
      }
      g && e.parent();
    } else if (e.firstChild()) {
      u && (r = "");
      do
        if (!(e.to <= t)) {
          if (e.from >= i)
            break;
          this.highlightRange(e, t, i, r, s), this.startSpan(Math.min(i, e.to), c);
        }
      while (e.nextSibling());
      e.parent();
    }
  }
}
function Mo(n) {
  let e = n.type.prop(sr);
  for (; e && e.context && !n.matchContext(e.context); )
    e = e.next;
  return e || null;
}
const p = te.define, St = p(), we = p(), bi = p(we), xi = p(we), ve = p(), Ct = p(ve), an = p(ve), he = p(), Pe = p(he), ce = p(), fe = p(), vn = p(), et = p(vn), At = p(), m = {
  /**
  A comment.
  */
  comment: St,
  /**
  A line [comment](#highlight.tags.comment).
  */
  lineComment: p(St),
  /**
  A block [comment](#highlight.tags.comment).
  */
  blockComment: p(St),
  /**
  A documentation [comment](#highlight.tags.comment).
  */
  docComment: p(St),
  /**
  Any kind of identifier.
  */
  name: we,
  /**
  The [name](#highlight.tags.name) of a variable.
  */
  variableName: p(we),
  /**
  A type [name](#highlight.tags.name).
  */
  typeName: bi,
  /**
  A tag name (subtag of [`typeName`](#highlight.tags.typeName)).
  */
  tagName: p(bi),
  /**
  A property or field [name](#highlight.tags.name).
  */
  propertyName: xi,
  /**
  An attribute name (subtag of [`propertyName`](#highlight.tags.propertyName)).
  */
  attributeName: p(xi),
  /**
  The [name](#highlight.tags.name) of a class.
  */
  className: p(we),
  /**
  A label [name](#highlight.tags.name).
  */
  labelName: p(we),
  /**
  A namespace [name](#highlight.tags.name).
  */
  namespace: p(we),
  /**
  The [name](#highlight.tags.name) of a macro.
  */
  macroName: p(we),
  /**
  A literal value.
  */
  literal: ve,
  /**
  A string [literal](#highlight.tags.literal).
  */
  string: Ct,
  /**
  A documentation [string](#highlight.tags.string).
  */
  docString: p(Ct),
  /**
  A character literal (subtag of [string](#highlight.tags.string)).
  */
  character: p(Ct),
  /**
  An attribute value (subtag of [string](#highlight.tags.string)).
  */
  attributeValue: p(Ct),
  /**
  A number [literal](#highlight.tags.literal).
  */
  number: an,
  /**
  An integer [number](#highlight.tags.number) literal.
  */
  integer: p(an),
  /**
  A floating-point [number](#highlight.tags.number) literal.
  */
  float: p(an),
  /**
  A boolean [literal](#highlight.tags.literal).
  */
  bool: p(ve),
  /**
  Regular expression [literal](#highlight.tags.literal).
  */
  regexp: p(ve),
  /**
  An escape [literal](#highlight.tags.literal), for example a
  backslash escape in a string.
  */
  escape: p(ve),
  /**
  A color [literal](#highlight.tags.literal).
  */
  color: p(ve),
  /**
  A URL [literal](#highlight.tags.literal).
  */
  url: p(ve),
  /**
  A language keyword.
  */
  keyword: ce,
  /**
  The [keyword](#highlight.tags.keyword) for the self or this
  object.
  */
  self: p(ce),
  /**
  The [keyword](#highlight.tags.keyword) for null.
  */
  null: p(ce),
  /**
  A [keyword](#highlight.tags.keyword) denoting some atomic value.
  */
  atom: p(ce),
  /**
  A [keyword](#highlight.tags.keyword) that represents a unit.
  */
  unit: p(ce),
  /**
  A modifier [keyword](#highlight.tags.keyword).
  */
  modifier: p(ce),
  /**
  A [keyword](#highlight.tags.keyword) that acts as an operator.
  */
  operatorKeyword: p(ce),
  /**
  A control-flow related [keyword](#highlight.tags.keyword).
  */
  controlKeyword: p(ce),
  /**
  A [keyword](#highlight.tags.keyword) that defines something.
  */
  definitionKeyword: p(ce),
  /**
  A [keyword](#highlight.tags.keyword) related to defining or
  interfacing with modules.
  */
  moduleKeyword: p(ce),
  /**
  An operator.
  */
  operator: fe,
  /**
  An [operator](#highlight.tags.operator) that dereferences something.
  */
  derefOperator: p(fe),
  /**
  Arithmetic-related [operator](#highlight.tags.operator).
  */
  arithmeticOperator: p(fe),
  /**
  Logical [operator](#highlight.tags.operator).
  */
  logicOperator: p(fe),
  /**
  Bit [operator](#highlight.tags.operator).
  */
  bitwiseOperator: p(fe),
  /**
  Comparison [operator](#highlight.tags.operator).
  */
  compareOperator: p(fe),
  /**
  [Operator](#highlight.tags.operator) that updates its operand.
  */
  updateOperator: p(fe),
  /**
  [Operator](#highlight.tags.operator) that defines something.
  */
  definitionOperator: p(fe),
  /**
  Type-related [operator](#highlight.tags.operator).
  */
  typeOperator: p(fe),
  /**
  Control-flow [operator](#highlight.tags.operator).
  */
  controlOperator: p(fe),
  /**
  Program or markup punctuation.
  */
  punctuation: vn,
  /**
  [Punctuation](#highlight.tags.punctuation) that separates
  things.
  */
  separator: p(vn),
  /**
  Bracket-style [punctuation](#highlight.tags.punctuation).
  */
  bracket: et,
  /**
  Angle [brackets](#highlight.tags.bracket) (usually `<` and `>`
  tokens).
  */
  angleBracket: p(et),
  /**
  Square [brackets](#highlight.tags.bracket) (usually `[` and `]`
  tokens).
  */
  squareBracket: p(et),
  /**
  Parentheses (usually `(` and `)` tokens). Subtag of
  [bracket](#highlight.tags.bracket).
  */
  paren: p(et),
  /**
  Braces (usually `{` and `}` tokens). Subtag of
  [bracket](#highlight.tags.bracket).
  */
  brace: p(et),
  /**
  Content, for example plain text in XML or markup documents.
  */
  content: he,
  /**
  [Content](#highlight.tags.content) that represents a heading.
  */
  heading: Pe,
  /**
  A level 1 [heading](#highlight.tags.heading).
  */
  heading1: p(Pe),
  /**
  A level 2 [heading](#highlight.tags.heading).
  */
  heading2: p(Pe),
  /**
  A level 3 [heading](#highlight.tags.heading).
  */
  heading3: p(Pe),
  /**
  A level 4 [heading](#highlight.tags.heading).
  */
  heading4: p(Pe),
  /**
  A level 5 [heading](#highlight.tags.heading).
  */
  heading5: p(Pe),
  /**
  A level 6 [heading](#highlight.tags.heading).
  */
  heading6: p(Pe),
  /**
  A prose [content](#highlight.tags.content) separator (such as a horizontal rule).
  */
  contentSeparator: p(he),
  /**
  [Content](#highlight.tags.content) that represents a list.
  */
  list: p(he),
  /**
  [Content](#highlight.tags.content) that represents a quote.
  */
  quote: p(he),
  /**
  [Content](#highlight.tags.content) that is emphasized.
  */
  emphasis: p(he),
  /**
  [Content](#highlight.tags.content) that is styled strong.
  */
  strong: p(he),
  /**
  [Content](#highlight.tags.content) that is part of a link.
  */
  link: p(he),
  /**
  [Content](#highlight.tags.content) that is styled as code or
  monospace.
  */
  monospace: p(he),
  /**
  [Content](#highlight.tags.content) that has a strike-through
  style.
  */
  strikethrough: p(he),
  /**
  Inserted text in a change-tracking format.
  */
  inserted: p(),
  /**
  Deleted text.
  */
  deleted: p(),
  /**
  Changed text.
  */
  changed: p(),
  /**
  An invalid or unsyntactic element.
  */
  invalid: p(),
  /**
  Metadata or meta-instruction.
  */
  meta: At,
  /**
  [Metadata](#highlight.tags.meta) that applies to the entire
  document.
  */
  documentMeta: p(At),
  /**
  [Metadata](#highlight.tags.meta) that annotates or adds
  attributes to a given syntactic element.
  */
  annotation: p(At),
  /**
  Processing instruction or preprocessor directive. Subtag of
  [meta](#highlight.tags.meta).
  */
  processingInstruction: p(At),
  /**
  [Modifier](#highlight.Tag^defineModifier) that indicates that a
  given element is being defined. Expected to be used with the
  various [name](#highlight.tags.name) tags.
  */
  definition: te.defineModifier("definition"),
  /**
  [Modifier](#highlight.Tag^defineModifier) that indicates that
  something is constant. Mostly expected to be used with
  [variable names](#highlight.tags.variableName).
  */
  constant: te.defineModifier("constant"),
  /**
  [Modifier](#highlight.Tag^defineModifier) used to indicate that
  a [variable](#highlight.tags.variableName) or [property
  name](#highlight.tags.propertyName) is being called or defined
  as a function.
  */
  function: te.defineModifier("function"),
  /**
  [Modifier](#highlight.Tag^defineModifier) that can be applied to
  [names](#highlight.tags.name) to indicate that they belong to
  the language's standard environment.
  */
  standard: te.defineModifier("standard"),
  /**
  [Modifier](#highlight.Tag^defineModifier) that indicates a given
  [names](#highlight.tags.name) is local to some scope.
  */
  local: te.defineModifier("local"),
  /**
  A generic variant [modifier](#highlight.Tag^defineModifier) that
  can be used to tag language-specific alternative variants of
  some common tag. It is recommended for themes to define special
  forms of at least the [string](#highlight.tags.string) and
  [variable name](#highlight.tags.variableName) tags, since those
  come up a lot.
  */
  special: te.defineModifier("special")
};
for (let n in m) {
  let e = m[n];
  e instanceof te && (e.name = n);
}
or([
  { tag: m.link, class: "tok-link" },
  { tag: m.heading, class: "tok-heading" },
  { tag: m.emphasis, class: "tok-emphasis" },
  { tag: m.strong, class: "tok-strong" },
  { tag: m.keyword, class: "tok-keyword" },
  { tag: m.atom, class: "tok-atom" },
  { tag: m.bool, class: "tok-bool" },
  { tag: m.url, class: "tok-url" },
  { tag: m.labelName, class: "tok-labelName" },
  { tag: m.inserted, class: "tok-inserted" },
  { tag: m.deleted, class: "tok-deleted" },
  { tag: m.literal, class: "tok-literal" },
  { tag: m.string, class: "tok-string" },
  { tag: m.number, class: "tok-number" },
  { tag: [m.regexp, m.escape, m.special(m.string)], class: "tok-string2" },
  { tag: m.variableName, class: "tok-variableName" },
  { tag: m.local(m.variableName), class: "tok-variableName tok-local" },
  { tag: m.definition(m.variableName), class: "tok-variableName tok-definition" },
  { tag: m.special(m.variableName), class: "tok-variableName2" },
  { tag: m.definition(m.propertyName), class: "tok-propertyName tok-definition" },
  { tag: m.typeName, class: "tok-typeName" },
  { tag: m.namespace, class: "tok-namespace" },
  { tag: m.className, class: "tok-className" },
  { tag: m.macroName, class: "tok-macroName" },
  { tag: m.propertyName, class: "tok-propertyName" },
  { tag: m.operator, class: "tok-operator" },
  { tag: m.comment, class: "tok-comment" },
  { tag: m.meta, class: "tok-meta" },
  { tag: m.invalid, class: "tok-invalid" },
  { tag: m.punctuation, class: "tok-punctuation" }
]);
var cn;
const De = /* @__PURE__ */ new S();
function lr(n) {
  return H.define({
    combine: n ? (e) => e.concat(n) : void 0
  });
}
const To = /* @__PURE__ */ new S();
class ie {
  /**
  Construct a language object. If you need to invoke this
  directly, first define a data facet with
  [`defineLanguageFacet`](https://codemirror.net/6/docs/ref/#language.defineLanguageFacet), and then
  configure your parser to [attach](https://codemirror.net/6/docs/ref/#language.languageDataProp) it
  to the language's outer syntax node.
  */
  constructor(e, t, i = [], r = "") {
    this.data = e, this.name = r, Oe.prototype.hasOwnProperty("tree") || Object.defineProperty(Oe.prototype, "tree", { get() {
      return _(this);
    } }), this.parser = t, this.extension = [
      Ae.of(this),
      Oe.languageData.of((s, o, l) => {
        let a = ki(s, o, l), c = a.type.prop(De);
        if (!c)
          return [];
        let f = s.facet(c), h = a.type.prop(To);
        if (h) {
          let u = a.resolve(o - a.from, l);
          for (let d of h)
            if (d.test(u, s)) {
              let y = s.facet(d.facet);
              return d.type == "replace" ? y : y.concat(f);
            }
        }
        return f;
      })
    ].concat(i);
  }
  /**
  Query whether this language is active at the given position.
  */
  isActiveAt(e, t, i = -1) {
    return ki(e, t, i).type.prop(De) == this.data;
  }
  /**
  Find the document regions that were parsed using this language.
  The returned regions will _include_ any nested languages rooted
  in this language, when those exist.
  */
  findRegions(e) {
    let t = e.facet(Ae);
    if ((t == null ? void 0 : t.data) == this.data)
      return [{ from: 0, to: e.doc.length }];
    if (!t || !t.allowsNesting)
      return [];
    let i = [], r = (s, o) => {
      if (s.prop(De) == this.data) {
        i.push({ from: o, to: o + s.length });
        return;
      }
      let l = s.prop(S.mounted);
      if (l) {
        if (l.tree.prop(De) == this.data) {
          if (l.overlay)
            for (let a of l.overlay)
              i.push({ from: a.from + o, to: a.to + o });
          else
            i.push({ from: o, to: o + s.length });
          return;
        } else if (l.overlay) {
          let a = i.length;
          if (r(l.tree, l.overlay[0].from + o), i.length > a)
            return;
        }
      }
      for (let a = 0; a < s.children.length; a++) {
        let c = s.children[a];
        c instanceof O && r(c, s.positions[a] + o);
      }
    };
    return r(_(e), 0), i;
  }
  /**
  Indicates whether this language allows nested languages. The
  default implementation returns true.
  */
  get allowsNesting() {
    return !0;
  }
}
ie.setState = /* @__PURE__ */ F.define();
function ki(n, e, t) {
  let i = n.facet(Ae), r = _(n).topNode;
  if (!i || i.allowsNesting)
    for (let s = r; s; s = s.enter(e, t, R.ExcludeBuffers))
      s.type.isTop && (r = s);
  return r;
}
class Sn extends ie {
  constructor(e, t, i) {
    super(e, t, [], i), this.parser = t;
  }
  /**
  Define a language from a parser.
  */
  static define(e) {
    let t = lr(e.languageData);
    return new Sn(t, e.parser.configure({
      props: [De.add((i) => i.isTop ? t : void 0)]
    }), e.name);
  }
  /**
  Create a new instance of this language with a reconfigured
  version of its parser and optionally a new name.
  */
  configure(e, t) {
    return new Sn(this.data, this.parser.configure(e), t || this.name);
  }
  get allowsNesting() {
    return this.parser.hasWrappers();
  }
}
function _(n) {
  let e = n.field(ie.state, !1);
  return e ? e.tree : O.empty;
}
class Io {
  /**
  Create an input object for the given document.
  */
  constructor(e) {
    this.doc = e, this.cursorPos = 0, this.string = "", this.cursor = e.iter();
  }
  get length() {
    return this.doc.length;
  }
  syncTo(e) {
    return this.string = this.cursor.next(e - this.cursorPos).value, this.cursorPos = e + this.string.length, this.cursorPos - this.string.length;
  }
  chunk(e) {
    return this.syncTo(e), this.string;
  }
  get lineChunks() {
    return !0;
  }
  read(e, t) {
    let i = this.cursorPos - this.string.length;
    return e < i || t >= this.cursorPos ? this.doc.sliceString(e, t) : this.string.slice(e - i, t - i);
  }
}
let tt = null;
class He {
  constructor(e, t, i = [], r, s, o, l, a) {
    this.parser = e, this.state = t, this.fragments = i, this.tree = r, this.treeLen = s, this.viewport = o, this.skipped = l, this.scheduleOn = a, this.parse = null, this.tempSkipped = [];
  }
  /**
  @internal
  */
  static create(e, t, i) {
    return new He(e, t, [], O.empty, 0, i, [], null);
  }
  startParse() {
    return this.parser.startParse(new Io(this.state.doc), this.fragments);
  }
  /**
  @internal
  */
  work(e, t) {
    return t != null && t >= this.state.doc.length && (t = void 0), this.tree != O.empty && this.isDone(t ?? this.state.doc.length) ? (this.takeTree(), !0) : this.withContext(() => {
      var i;
      if (typeof e == "number") {
        let r = Date.now() + e;
        e = () => Date.now() > r;
      }
      for (this.parse || (this.parse = this.startParse()), t != null && (this.parse.stoppedAt == null || this.parse.stoppedAt > t) && t < this.state.doc.length && this.parse.stopAt(t); ; ) {
        let r = this.parse.advance();
        if (r)
          if (this.fragments = this.withoutTempSkipped(xe.addTree(r, this.fragments, this.parse.stoppedAt != null)), this.treeLen = (i = this.parse.stoppedAt) !== null && i !== void 0 ? i : this.state.doc.length, this.tree = r, this.parse = null, this.treeLen < (t ?? this.state.doc.length))
            this.parse = this.startParse();
          else
            return !0;
        if (e())
          return !1;
      }
    });
  }
  /**
  @internal
  */
  takeTree() {
    let e, t;
    this.parse && (e = this.parse.parsedPos) >= this.treeLen && ((this.parse.stoppedAt == null || this.parse.stoppedAt > e) && this.parse.stopAt(e), this.withContext(() => {
      for (; !(t = this.parse.advance()); )
        ;
    }), this.treeLen = e, this.tree = t, this.fragments = this.withoutTempSkipped(xe.addTree(this.tree, this.fragments, !0)), this.parse = null);
  }
  withContext(e) {
    let t = tt;
    tt = this;
    try {
      return e();
    } finally {
      tt = t;
    }
  }
  withoutTempSkipped(e) {
    for (let t; t = this.tempSkipped.pop(); )
      e = wi(e, t.from, t.to);
    return e;
  }
  /**
  @internal
  */
  changes(e, t) {
    let { fragments: i, tree: r, treeLen: s, viewport: o, skipped: l } = this;
    if (this.takeTree(), !e.empty) {
      let a = [];
      if (e.iterChangedRanges((c, f, h, u) => a.push({ fromA: c, toA: f, fromB: h, toB: u })), i = xe.applyChanges(i, a), r = O.empty, s = 0, o = { from: e.mapPos(o.from, -1), to: e.mapPos(o.to, 1) }, this.skipped.length) {
        l = [];
        for (let c of this.skipped) {
          let f = e.mapPos(c.from, 1), h = e.mapPos(c.to, -1);
          f < h && l.push({ from: f, to: h });
        }
      }
    }
    return new He(this.parser, t, i, r, s, o, l, this.scheduleOn);
  }
  /**
  @internal
  */
  updateViewport(e) {
    if (this.viewport.from == e.from && this.viewport.to == e.to)
      return !1;
    this.viewport = e;
    let t = this.skipped.length;
    for (let i = 0; i < this.skipped.length; i++) {
      let { from: r, to: s } = this.skipped[i];
      r < e.to && s > e.from && (this.fragments = wi(this.fragments, r, s), this.skipped.splice(i--, 1));
    }
    return this.skipped.length >= t ? !1 : (this.reset(), !0);
  }
  /**
  @internal
  */
  reset() {
    this.parse && (this.takeTree(), this.parse = null);
  }
  /**
  Notify the parse scheduler that the given region was skipped
  because it wasn't in view, and the parse should be restarted
  when it comes into view.
  */
  skipUntilInView(e, t) {
    this.skipped.push({ from: e, to: t });
  }
  /**
  Returns a parser intended to be used as placeholder when
  asynchronously loading a nested parser. It'll skip its input and
  mark it as not-really-parsed, so that the next update will parse
  it again.
  
  When `until` is given, a reparse will be scheduled when that
  promise resolves.
  */
  static getSkippingParser(e) {
    return new class extends rr {
      createParse(t, i, r) {
        let s = r[0].from, o = r[r.length - 1].to;
        return {
          parsedPos: s,
          advance() {
            let a = tt;
            if (a) {
              for (let c of r)
                a.tempSkipped.push(c);
              e && (a.scheduleOn = a.scheduleOn ? Promise.all([a.scheduleOn, e]) : e);
            }
            return this.parsedPos = o, new O(G.none, [], [], o - s);
          },
          stoppedAt: null,
          stopAt() {
          }
        };
      }
    }();
  }
  /**
  @internal
  */
  isDone(e) {
    e = Math.min(e, this.state.doc.length);
    let t = this.fragments;
    return this.treeLen >= e && t.length && t[0].from == 0 && t[0].to >= e;
  }
  /**
  Get the context for the current parse, or `null` if no editor
  parse is in progress.
  */
  static get() {
    return tt;
  }
}
function wi(n, e, t) {
  return xe.applyChanges(n, [{ fromA: e, toA: t, fromB: e, toB: t }]);
}
class Ve {
  constructor(e) {
    this.context = e, this.tree = e.tree;
  }
  apply(e) {
    if (!e.docChanged && this.tree == this.context.tree)
      return this;
    let t = this.context.changes(e.changes, e.state), i = this.context.treeLen == e.startState.doc.length ? void 0 : Math.max(e.changes.mapPos(this.context.treeLen), t.viewport.to);
    return t.work(20, i) || t.takeTree(), new Ve(t);
  }
  static init(e) {
    let t = Math.min(3e3, e.doc.length), i = He.create(e.facet(Ae).parser, e, { from: 0, to: t });
    return i.work(20, t) || i.takeTree(), new Ve(i);
  }
}
ie.state = /* @__PURE__ */ me.define({
  create: Ve.init,
  update(n, e) {
    for (let t of e.effects)
      if (t.is(ie.setState))
        return t.value;
    return e.startState.facet(Ae) != e.state.facet(Ae) ? Ve.init(e.state) : n.apply(e);
  }
});
let ar = (n) => {
  let e = setTimeout(
    () => n(),
    500
    /* Work.MaxPause */
  );
  return () => clearTimeout(e);
};
typeof requestIdleCallback < "u" && (ar = (n) => {
  let e = -1, t = setTimeout(
    () => {
      e = requestIdleCallback(n, {
        timeout: 400
        /* Work.MinPause */
      });
    },
    100
    /* Work.MinPause */
  );
  return () => e < 0 ? clearTimeout(t) : cancelIdleCallback(e);
});
const fn = typeof navigator < "u" && (!((cn = navigator.scheduling) === null || cn === void 0) && cn.isInputPending) ? () => navigator.scheduling.isInputPending() : null, Po = /* @__PURE__ */ Je.fromClass(class {
  constructor(e) {
    this.view = e, this.working = null, this.workScheduled = 0, this.chunkEnd = -1, this.chunkBudget = -1, this.work = this.work.bind(this), this.scheduleWork();
  }
  update(e) {
    let t = this.view.state.field(ie.state).context;
    (t.updateViewport(e.view.viewport) || this.view.viewport.to > t.treeLen) && this.scheduleWork(), (e.docChanged || e.selectionSet) && (this.view.hasFocus && (this.chunkBudget += 50), this.scheduleWork()), this.checkAsyncSchedule(t);
  }
  scheduleWork() {
    if (this.working)
      return;
    let { state: e } = this.view, t = e.field(ie.state);
    (t.tree != t.context.tree || !t.context.isDone(e.doc.length)) && (this.working = ar(this.work));
  }
  work(e) {
    this.working = null;
    let t = Date.now();
    if (this.chunkEnd < t && (this.chunkEnd < 0 || this.view.hasFocus) && (this.chunkEnd = t + 3e4, this.chunkBudget = 3e3), this.chunkBudget <= 0)
      return;
    let { state: i, viewport: { to: r } } = this.view, s = i.field(ie.state);
    if (s.tree == s.context.tree && s.context.isDone(
      r + 1e5
      /* Work.MaxParseAhead */
    ))
      return;
    let o = Date.now() + Math.min(this.chunkBudget, 100, e && !fn ? Math.max(25, e.timeRemaining() - 5) : 1e9), l = s.context.treeLen < r && i.doc.length > r + 1e3, a = s.context.work(() => fn && fn() || Date.now() > o, r + (l ? 0 : 1e5));
    this.chunkBudget -= Date.now() - t, (a || this.chunkBudget <= 0) && (s.context.takeTree(), this.view.dispatch({ effects: ie.setState.of(new Ve(s.context)) })), this.chunkBudget > 0 && !(a && !l) && this.scheduleWork(), this.checkAsyncSchedule(s.context);
  }
  checkAsyncSchedule(e) {
    e.scheduleOn && (this.workScheduled++, e.scheduleOn.then(() => this.scheduleWork()).catch((t) => Ot(this.view.state, t)).then(() => this.workScheduled--), e.scheduleOn = null);
  }
  destroy() {
    this.working && this.working();
  }
  isWorking() {
    return !!(this.working || this.workScheduled > 0);
  }
}, {
  eventHandlers: { focus() {
    this.scheduleWork();
  } }
}), Ae = /* @__PURE__ */ H.define({
  combine(n) {
    return n.length ? n[0] : null;
  },
  enables: (n) => [
    ie.state,
    Po,
    T.contentAttributes.compute([n], (e) => {
      let t = e.facet(n);
      return t && t.name ? { "data-language": t.name } : {};
    })
  ]
});
class Cf {
  /**
  Create a language support object.
  */
  constructor(e, t = []) {
    this.language = e, this.support = t, this.extension = [e, t];
  }
}
class cr {
  constructor(e, t, i, r, s, o = void 0) {
    this.name = e, this.alias = t, this.extensions = i, this.filename = r, this.loadFunc = s, this.support = o, this.loading = null;
  }
  /**
  Start loading the the language. Will return a promise that
  resolves to a [`LanguageSupport`](https://codemirror.net/6/docs/ref/#language.LanguageSupport)
  object when the language successfully loads.
  */
  load() {
    return this.loading || (this.loading = this.loadFunc().then((e) => this.support = e, (e) => {
      throw this.loading = null, e;
    }));
  }
  /**
  Create a language description.
  */
  static of(e) {
    let { load: t, support: i } = e;
    if (!t) {
      if (!i)
        throw new RangeError("Must pass either 'load' or 'support' to LanguageDescription.of");
      t = () => Promise.resolve(i);
    }
    return new cr(e.name, (e.alias || []).concat(e.name).map((r) => r.toLowerCase()), e.extensions || [], e.filename, t, i);
  }
  /**
  Look for a language in the given array of descriptions that
  matches the filename. Will first match
  [`filename`](https://codemirror.net/6/docs/ref/#language.LanguageDescription.filename) patterns,
  and then [extensions](https://codemirror.net/6/docs/ref/#language.LanguageDescription.extensions),
  and return the first language that matches.
  */
  static matchFilename(e, t) {
    for (let r of e)
      if (r.filename && r.filename.test(t))
        return r;
    let i = /\.([^.]+)$/.exec(t);
    if (i) {
      for (let r of e)
        if (r.extensions.indexOf(i[1]) > -1)
          return r;
    }
    return null;
  }
  /**
  Look for a language whose name or alias matches the the given
  name (case-insensitively). If `fuzzy` is true, and no direct
  matchs is found, this'll also search for a language whose name
  or alias occurs in the string (for names shorter than three
  characters, only when surrounded by non-word characters).
  */
  static matchLanguageName(e, t, i = !0) {
    t = t.toLowerCase();
    for (let r of e)
      if (r.alias.some((s) => s == t))
        return r;
    if (i)
      for (let r of e)
        for (let s of r.alias) {
          let o = t.indexOf(s);
          if (o > -1 && (s.length > 2 || !/\w/.test(t[o - 1]) && !/\w/.test(t[o + s.length])))
            return r;
        }
    return null;
  }
}
const Bo = /* @__PURE__ */ H.define(), Yt = /* @__PURE__ */ H.define({
  combine: (n) => {
    if (!n.length)
      return "  ";
    let e = n[0];
    if (!e || /\S/.test(e) || Array.from(e).some((t) => t != e[0]))
      throw new Error("Invalid indent unit: " + JSON.stringify(n[0]));
    return e;
  }
});
function Re(n) {
  let e = n.facet(Yt);
  return e.charCodeAt(0) == 9 ? n.tabSize * e.length : e.length;
}
function lt(n, e) {
  let t = "", i = n.tabSize, r = n.facet(Yt)[0];
  if (r == "	") {
    for (; e >= i; )
      t += "	", e -= i;
    r = " ";
  }
  for (let s = 0; s < e; s++)
    t += r;
  return t;
}
function $n(n, e) {
  n instanceof Oe && (n = new Xt(n));
  for (let i of n.state.facet(Bo)) {
    let r = i(n, e);
    if (r !== void 0)
      return r;
  }
  let t = _(n.state);
  return t.length >= e ? Do(n, t, e) : null;
}
class Xt {
  /**
  Create an indent context.
  */
  constructor(e, t = {}) {
    this.state = e, this.options = t, this.unit = Re(e);
  }
  /**
  Get a description of the line at the given position, taking
  [simulated line
  breaks](https://codemirror.net/6/docs/ref/#language.IndentContext.constructor^options.simulateBreak)
  into account. If there is such a break at `pos`, the `bias`
  argument determines whether the part of the line line before or
  after the break is used.
  */
  lineAt(e, t = 1) {
    let i = this.state.doc.lineAt(e), { simulateBreak: r, simulateDoubleBreak: s } = this.options;
    return r != null && r >= i.from && r <= i.to ? s && r == e ? { text: "", from: e } : (t < 0 ? r < e : r <= e) ? { text: i.text.slice(r - i.from), from: r } : { text: i.text.slice(0, r - i.from), from: i.from } : i;
  }
  /**
  Get the text directly after `pos`, either the entire line
  or the next 100 characters, whichever is shorter.
  */
  textAfterPos(e, t = 1) {
    if (this.options.simulateDoubleBreak && e == this.options.simulateBreak)
      return "";
    let { text: i, from: r } = this.lineAt(e, t);
    return i.slice(e - r, Math.min(i.length, e + 100 - r));
  }
  /**
  Find the column for the given position.
  */
  column(e, t = 1) {
    let { text: i, from: r } = this.lineAt(e, t), s = this.countColumn(i, e - r), o = this.options.overrideIndentation ? this.options.overrideIndentation(r) : -1;
    return o > -1 && (s += o - this.countColumn(i, i.search(/\S|$/))), s;
  }
  /**
  Find the column position (taking tabs into account) of the given
  position in the given string.
  */
  countColumn(e, t = e.length) {
    return Jt(e, this.state.tabSize, t);
  }
  /**
  Find the indentation column of the line at the given point.
  */
  lineIndent(e, t = 1) {
    let { text: i, from: r } = this.lineAt(e, t), s = this.options.overrideIndentation;
    if (s) {
      let o = s(r);
      if (o > -1)
        return o;
    }
    return this.countColumn(i, i.search(/\S|$/));
  }
  /**
  Returns the [simulated line
  break](https://codemirror.net/6/docs/ref/#language.IndentContext.constructor^options.simulateBreak)
  for this context, if any.
  */
  get simulatedBreak() {
    return this.options.simulateBreak || null;
  }
}
const fr = /* @__PURE__ */ new S();
function Do(n, e, t) {
  let i = e.resolveStack(t), r = e.resolveInner(t, -1).resolve(t, 0).enterUnfinishedNodesBefore(t);
  if (r != i.node) {
    let s = [];
    for (let o = r; o && !(o.from == i.node.from && o.type == i.node.type); o = o.parent)
      s.push(o);
    for (let o = s.length - 1; o >= 0; o--)
      i = { node: s[o], next: i };
  }
  return hr(i, n, t);
}
function hr(n, e, t) {
  for (let i = n; i; i = i.next) {
    let r = Oo(i.node);
    if (r)
      return r(Un.create(e, t, i));
  }
  return 0;
}
function Lo(n) {
  return n.pos == n.options.simulateBreak && n.options.simulateDoubleBreak;
}
function Oo(n) {
  let e = n.type.prop(fr);
  if (e)
    return e;
  let t = n.firstChild, i;
  if (t && (i = t.type.prop(S.closedBy))) {
    let r = n.lastChild, s = r && i.indexOf(r.name) > -1;
    return (o) => ur(o, !0, 1, void 0, s && !Lo(o) ? r.from : void 0);
  }
  return n.parent == null ? Eo : null;
}
function Eo() {
  return 0;
}
class Un extends Xt {
  constructor(e, t, i) {
    super(e.state, e.options), this.base = e, this.pos = t, this.context = i;
  }
  /**
  The syntax tree node to which the indentation strategy
  applies.
  */
  get node() {
    return this.context.node;
  }
  /**
  @internal
  */
  static create(e, t, i) {
    return new Un(e, t, i);
  }
  /**
  Get the text directly after `this.pos`, either the entire line
  or the next 100 characters, whichever is shorter.
  */
  get textAfter() {
    return this.textAfterPos(this.pos);
  }
  /**
  Get the indentation at the reference line for `this.node`, which
  is the line on which it starts, unless there is a node that is
  _not_ a parent of this node covering the start of that line. If
  so, the line at the start of that node is tried, again skipping
  on if it is covered by another such node.
  */
  get baseIndent() {
    return this.baseIndentFor(this.node);
  }
  /**
  Get the indentation for the reference line of the given node
  (see [`baseIndent`](https://codemirror.net/6/docs/ref/#language.TreeIndentContext.baseIndent)).
  */
  baseIndentFor(e) {
    let t = this.state.doc.lineAt(e.from);
    for (; ; ) {
      let i = e.resolve(t.from);
      for (; i.parent && i.parent.from == i.from; )
        i = i.parent;
      if (No(i, e))
        break;
      t = this.state.doc.lineAt(i.from);
    }
    return this.lineIndent(t.from);
  }
  /**
  Continue looking for indentations in the node's parent nodes,
  and return the result of that.
  */
  continue() {
    return hr(this.context.next, this.base, this.pos);
  }
}
function No(n, e) {
  for (let t = e; t; t = t.parent)
    if (n == t)
      return !0;
  return !1;
}
function Ro(n) {
  let e = n.node, t = e.childAfter(e.from), i = e.lastChild;
  if (!t)
    return null;
  let r = n.options.simulateBreak, s = n.state.doc.lineAt(t.from), o = r == null || r <= s.from ? s.to : Math.min(s.to, r);
  for (let l = t.to; ; ) {
    let a = e.childAfter(l);
    if (!a || a == i)
      return null;
    if (!a.type.isSkipped) {
      if (a.from >= o)
        return null;
      let c = /^ */.exec(s.text.slice(t.to - s.from))[0].length;
      return { from: t.from, to: t.to + c };
    }
    l = a.to;
  }
}
function Af({ closing: n, align: e = !0, units: t = 1 }) {
  return (i) => ur(i, e, t, n);
}
function ur(n, e, t, i, r) {
  let s = n.textAfter, o = s.match(/^\s*/)[0].length, l = i && s.slice(o, o + i.length) == i || r == n.pos + o, a = e ? Ro(n) : null;
  return a ? l ? n.column(a.from) : n.column(a.to) : n.baseIndent + (l ? 0 : n.unit * t);
}
const Mf = (n) => n.baseIndent;
function Tf({ except: n, units: e = 1 } = {}) {
  return (t) => {
    let i = n && n.test(t.textAfter);
    return t.baseIndent + (i ? 0 : e * t.unit);
  };
}
const Fo = 200;
function Wo() {
  return Oe.transactionFilter.of((n) => {
    if (!n.docChanged || !n.isUserEvent("input.type") && !n.isUserEvent("input.complete"))
      return n;
    let e = n.startState.languageDataAt("indentOnInput", n.startState.selection.main.head);
    if (!e.length)
      return n;
    let t = n.newDoc, { head: i } = n.newSelection.main, r = t.lineAt(i);
    if (i > r.from + Fo)
      return n;
    let s = t.sliceString(r.from, i);
    if (!e.some((c) => c.test(s)))
      return n;
    let { state: o } = n, l = -1, a = [];
    for (let { head: c } of o.selection.ranges) {
      let f = o.doc.lineAt(c);
      if (f.from == l)
        continue;
      l = f.from;
      let h = $n(o, f.from);
      if (h == null)
        continue;
      let u = /^\s*/.exec(f.text)[0], d = lt(o, h);
      u != d && a.push({ from: f.from, to: f.from + u.length, insert: d });
    }
    return a.length ? [n, { changes: a, sequential: !0 }] : n;
  });
}
const qo = /* @__PURE__ */ H.define(), zo = /* @__PURE__ */ new S();
function If(n) {
  let e = n.firstChild, t = n.lastChild;
  return e && e.to < t.from ? { from: e.to, to: t.type.isError ? n.to : t.from } : null;
}
function $o(n, e, t) {
  let i = _(n);
  if (i.length < t)
    return null;
  let r = i.resolveStack(t, 1), s = null;
  for (let o = r; o; o = o.next) {
    let l = o.node;
    if (l.to <= t || l.from > t)
      continue;
    if (s && l.from < e)
      break;
    let a = l.type.prop(zo);
    if (a && (l.to < i.length - 50 || i.length == n.doc.length || !Uo(l))) {
      let c = a(l, n);
      c && c.from <= t && c.from >= e && c.to > t && (s = c);
    }
  }
  return s;
}
function Uo(n) {
  let e = n.lastChild;
  return e && e.to == n.to && e.type.isError;
}
function Wt(n, e, t) {
  for (let i of n.facet(qo)) {
    let r = i(n, e, t);
    if (r)
      return r;
  }
  return $o(n, e, t);
}
function dr(n, e) {
  let t = e.mapPos(n.from, 1), i = e.mapPos(n.to, -1);
  return t >= i ? void 0 : { from: t, to: i };
}
const en = /* @__PURE__ */ F.define({ map: dr }), gt = /* @__PURE__ */ F.define({ map: dr });
function pr(n) {
  let e = [];
  for (let { head: t } of n.state.selection.ranges)
    e.some((i) => i.from <= t && i.to >= t) || e.push(n.lineBlockAt(t));
  return e;
}
const Fe = /* @__PURE__ */ me.define({
  create() {
    return A.none;
  },
  update(n, e) {
    n = n.map(e.changes);
    for (let t of e.effects)
      if (t.is(en) && !_o(n, t.value.from, t.value.to)) {
        let { preparePlaceholder: i } = e.state.facet(yr), r = i ? A.replace({ widget: new Qo(i(e.state, t.value)) }) : vi;
        n = n.update({ add: [r.range(t.value.from, t.value.to)] });
      } else t.is(gt) && (n = n.update({
        filter: (i, r) => t.value.from != i || t.value.to != r,
        filterFrom: t.value.from,
        filterTo: t.value.to
      }));
    if (e.selection) {
      let t = !1, { head: i } = e.selection.main;
      n.between(i, i, (r, s) => {
        r < i && s > i && (t = !0);
      }), t && (n = n.update({
        filterFrom: i,
        filterTo: i,
        filter: (r, s) => s <= i || r >= i
      }));
    }
    return n;
  },
  provide: (n) => T.decorations.from(n),
  toJSON(n, e) {
    let t = [];
    return n.between(0, e.doc.length, (i, r) => {
      t.push(i, r);
    }), t;
  },
  fromJSON(n) {
    if (!Array.isArray(n) || n.length % 2)
      throw new RangeError("Invalid JSON for fold state");
    let e = [];
    for (let t = 0; t < n.length; ) {
      let i = n[t++], r = n[t++];
      if (typeof i != "number" || typeof r != "number")
        throw new RangeError("Invalid JSON for fold state");
      e.push(vi.range(i, r));
    }
    return A.set(e, !0);
  }
});
function qt(n, e, t) {
  var i;
  let r = null;
  return (i = n.field(Fe, !1)) === null || i === void 0 || i.between(e, t, (s, o) => {
    (!r || r.from > s) && (r = { from: s, to: o });
  }), r;
}
function _o(n, e, t) {
  let i = !1;
  return n.between(e, e, (r, s) => {
    r == e && s == t && (i = !0);
  }), i;
}
function mr(n, e) {
  return n.field(Fe, !1) ? e : e.concat(F.appendConfig.of(br()));
}
const jo = (n) => {
  for (let e of pr(n)) {
    let t = Wt(n.state, e.from, e.to);
    if (t)
      return n.dispatch({ effects: mr(n.state, [en.of(t), gr(n, t)]) }), !0;
  }
  return !1;
}, Ho = (n) => {
  if (!n.state.field(Fe, !1))
    return !1;
  let e = [];
  for (let t of pr(n)) {
    let i = qt(n.state, t.from, t.to);
    i && e.push(gt.of(i), gr(n, i, !1));
  }
  return e.length && n.dispatch({ effects: e }), e.length > 0;
};
function gr(n, e, t = !0) {
  let i = n.state.doc.lineAt(e.from).number, r = n.state.doc.lineAt(e.to).number;
  return T.announce.of(`${n.state.phrase(t ? "Folded lines" : "Unfolded lines")} ${i} ${n.state.phrase("to")} ${r}.`);
}
const Vo = (n) => {
  let { state: e } = n, t = [];
  for (let i = 0; i < e.doc.length; ) {
    let r = n.lineBlockAt(i), s = Wt(e, r.from, r.to);
    s && t.push(en.of(s)), i = (s ? n.lineBlockAt(s.to) : r).to + 1;
  }
  return t.length && n.dispatch({ effects: mr(n.state, t) }), !!t.length;
}, Ko = (n) => {
  let e = n.state.field(Fe, !1);
  if (!e || !e.size)
    return !1;
  let t = [];
  return e.between(0, n.state.doc.length, (i, r) => {
    t.push(gt.of({ from: i, to: r }));
  }), n.dispatch({ effects: t }), !0;
}, Go = [
  { key: "Ctrl-Shift-[", mac: "Cmd-Alt-[", run: jo },
  { key: "Ctrl-Shift-]", mac: "Cmd-Alt-]", run: Ho },
  { key: "Ctrl-Alt-[", run: Vo },
  { key: "Ctrl-Alt-]", run: Ko }
], Jo = {
  placeholderDOM: null,
  preparePlaceholder: null,
  placeholderText: "…"
}, yr = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, Jo);
  }
});
function br(n) {
  return [Fe, Xo];
}
function xr(n, e) {
  let { state: t } = n, i = t.facet(yr), r = (o) => {
    let l = n.lineBlockAt(n.posAtDOM(o.target)), a = qt(n.state, l.from, l.to);
    a && n.dispatch({ effects: gt.of(a) }), o.preventDefault();
  };
  if (i.placeholderDOM)
    return i.placeholderDOM(n, r, e);
  let s = document.createElement("span");
  return s.textContent = i.placeholderText, s.setAttribute("aria-label", t.phrase("folded code")), s.title = t.phrase("unfold"), s.className = "cm-foldPlaceholder", s.onclick = r, s;
}
const vi = /* @__PURE__ */ A.replace({ widget: /* @__PURE__ */ new class extends Qt {
  toDOM(n) {
    return xr(n, null);
  }
}() });
class Qo extends Qt {
  constructor(e) {
    super(), this.value = e;
  }
  eq(e) {
    return this.value == e.value;
  }
  toDOM(e) {
    return xr(e, this.value);
  }
}
const Zo = {
  openText: "⌄",
  closedText: "›",
  markerDOM: null,
  domEventHandlers: {},
  foldingChanged: () => !1
};
class hn extends Rs {
  constructor(e, t) {
    super(), this.config = e, this.open = t;
  }
  eq(e) {
    return this.config == e.config && this.open == e.open;
  }
  toDOM(e) {
    if (this.config.markerDOM)
      return this.config.markerDOM(this.open);
    let t = document.createElement("span");
    return t.textContent = this.open ? this.config.openText : this.config.closedText, t.title = e.state.phrase(this.open ? "Fold line" : "Unfold line"), t;
  }
}
function Yo(n = {}) {
  let e = Object.assign(Object.assign({}, Zo), n), t = new hn(e, !0), i = new hn(e, !1), r = Je.fromClass(class {
    constructor(o) {
      this.from = o.viewport.from, this.markers = this.buildMarkers(o);
    }
    update(o) {
      (o.docChanged || o.viewportChanged || o.startState.facet(Ae) != o.state.facet(Ae) || o.startState.field(Fe, !1) != o.state.field(Fe, !1) || _(o.startState) != _(o.state) || e.foldingChanged(o)) && (this.markers = this.buildMarkers(o.view));
    }
    buildMarkers(o) {
      let l = new Gt();
      for (let a of o.viewportLineBlocks) {
        let c = qt(o.state, a.from, a.to) ? i : Wt(o.state, a.from, a.to) ? t : null;
        c && l.add(a.from, a.from, c);
      }
      return l.finish();
    }
  }), { domEventHandlers: s } = e;
  return [
    r,
    Ns({
      class: "cm-foldGutter",
      markers(o) {
        var l;
        return ((l = o.plugin(r)) === null || l === void 0 ? void 0 : l.markers) || Yi.empty;
      },
      initialSpacer() {
        return new hn(e, !1);
      },
      domEventHandlers: Object.assign(Object.assign({}, s), { click: (o, l, a) => {
        if (s.click && s.click(o, l, a))
          return !0;
        let c = qt(o.state, l.from, l.to);
        if (c)
          return o.dispatch({ effects: gt.of(c) }), !0;
        let f = Wt(o.state, l.from, l.to);
        return f ? (o.dispatch({ effects: en.of(f) }), !0) : !1;
      } })
    }),
    br()
  ];
}
const Xo = /* @__PURE__ */ T.baseTheme({
  ".cm-foldPlaceholder": {
    backgroundColor: "#eee",
    border: "1px solid #ddd",
    color: "#888",
    borderRadius: ".2em",
    margin: "0 1px",
    padding: "0 1px",
    cursor: "pointer"
  },
  ".cm-foldGutter span": {
    padding: "0 1px",
    cursor: "pointer"
  }
});
class tn {
  constructor(e, t) {
    this.specs = e;
    let i;
    function r(l) {
      let a = ai.newName();
      return (i || (i = /* @__PURE__ */ Object.create(null)))["." + a] = l, a;
    }
    const s = typeof t.all == "string" ? t.all : t.all ? r(t.all) : void 0, o = t.scope;
    this.scope = o instanceof ie ? (l) => l.prop(De) == o.data : o ? (l) => l == o : void 0, this.style = or(e.map((l) => ({
      tag: l.tag,
      class: l.class || r(Object.assign({}, l, { tag: null }))
    })), {
      all: s
    }).style, this.module = i ? new ai(i) : null, this.themeType = t.themeType;
  }
  /**
  Create a highlighter style that associates the given styles to
  the given tags. The specs must be objects that hold a style tag
  or array of tags in their `tag` property, and either a single
  `class` property providing a static CSS class (for highlighter
  that rely on external styling), or a
  [`style-mod`](https://github.com/marijnh/style-mod#documentation)-style
  set of CSS properties (which define the styling for those tags).
  
  The CSS rules created for a highlighter will be emitted in the
  order of the spec's properties. That means that for elements that
  have multiple tags associated with them, styles defined further
  down in the list will have a higher CSS precedence than styles
  defined earlier.
  */
  static define(e, t) {
    return new tn(e, t || {});
  }
}
const Cn = /* @__PURE__ */ H.define(), kr = /* @__PURE__ */ H.define({
  combine(n) {
    return n.length ? [n[0]] : null;
  }
});
function un(n) {
  let e = n.facet(Cn);
  return e.length ? e : n.facet(kr);
}
function el(n, e) {
  let t = [nl], i;
  return n instanceof tn && (n.module && t.push(T.styleModule.of(n.module)), i = n.themeType), e != null && e.fallback ? t.push(kr.of(n)) : i ? t.push(Cn.computeN([T.darkTheme], (r) => r.facet(T.darkTheme) == (i == "dark") ? [n] : [])) : t.push(Cn.of(n)), t;
}
class tl {
  constructor(e) {
    this.markCache = /* @__PURE__ */ Object.create(null), this.tree = _(e.state), this.decorations = this.buildDeco(e, un(e.state)), this.decoratedTo = e.viewport.to;
  }
  update(e) {
    let t = _(e.state), i = un(e.state), r = i != un(e.startState), { viewport: s } = e.view, o = e.changes.mapPos(this.decoratedTo, 1);
    t.length < s.to && !r && t.type == this.tree.type && o >= s.to ? (this.decorations = this.decorations.map(e.changes), this.decoratedTo = o) : (t != this.tree || e.viewportChanged || r) && (this.tree = t, this.decorations = this.buildDeco(e.view, i), this.decoratedTo = s.to);
  }
  buildDeco(e, t) {
    if (!t || !this.tree.length)
      return A.none;
    let i = new Gt();
    for (let { from: r, to: s } of e.visibleRanges)
      Co(this.tree, t, (o, l, a) => {
        i.add(o, l, this.markCache[a] || (this.markCache[a] = A.mark({ class: a })));
      }, r, s);
    return i.finish();
  }
}
const nl = /* @__PURE__ */ mt.high(/* @__PURE__ */ Je.fromClass(tl, {
  decorations: (n) => n.decorations
})), il = /* @__PURE__ */ tn.define([
  {
    tag: m.meta,
    color: "#404740"
  },
  {
    tag: m.link,
    textDecoration: "underline"
  },
  {
    tag: m.heading,
    textDecoration: "underline",
    fontWeight: "bold"
  },
  {
    tag: m.emphasis,
    fontStyle: "italic"
  },
  {
    tag: m.strong,
    fontWeight: "bold"
  },
  {
    tag: m.strikethrough,
    textDecoration: "line-through"
  },
  {
    tag: m.keyword,
    color: "#708"
  },
  {
    tag: [m.atom, m.bool, m.url, m.contentSeparator, m.labelName],
    color: "#219"
  },
  {
    tag: [m.literal, m.inserted],
    color: "#164"
  },
  {
    tag: [m.string, m.deleted],
    color: "#a11"
  },
  {
    tag: [m.regexp, m.escape, /* @__PURE__ */ m.special(m.string)],
    color: "#e40"
  },
  {
    tag: /* @__PURE__ */ m.definition(m.variableName),
    color: "#00f"
  },
  {
    tag: /* @__PURE__ */ m.local(m.variableName),
    color: "#30a"
  },
  {
    tag: [m.typeName, m.namespace],
    color: "#085"
  },
  {
    tag: m.className,
    color: "#167"
  },
  {
    tag: [/* @__PURE__ */ m.special(m.variableName), m.macroName],
    color: "#256"
  },
  {
    tag: /* @__PURE__ */ m.definition(m.propertyName),
    color: "#00c"
  },
  {
    tag: m.comment,
    color: "#940"
  },
  {
    tag: m.invalid,
    color: "#f00"
  }
]), rl = /* @__PURE__ */ T.baseTheme({
  "&.cm-focused .cm-matchingBracket": { backgroundColor: "#328c8252" },
  "&.cm-focused .cm-nonmatchingBracket": { backgroundColor: "#bb555544" }
}), wr = 1e4, vr = "()[]{}", Sr = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, {
      afterCursor: !0,
      brackets: vr,
      maxScanDistance: wr,
      renderMatch: ll
    });
  }
}), sl = /* @__PURE__ */ A.mark({ class: "cm-matchingBracket" }), ol = /* @__PURE__ */ A.mark({ class: "cm-nonmatchingBracket" });
function ll(n) {
  let e = [], t = n.matched ? sl : ol;
  return e.push(t.range(n.start.from, n.start.to)), n.end && e.push(t.range(n.end.from, n.end.to)), e;
}
const al = /* @__PURE__ */ me.define({
  create() {
    return A.none;
  },
  update(n, e) {
    if (!e.docChanged && !e.selection)
      return n;
    let t = [], i = e.state.facet(Sr);
    for (let r of e.state.selection.ranges) {
      if (!r.empty)
        continue;
      let s = de(e.state, r.head, -1, i) || r.head > 0 && de(e.state, r.head - 1, 1, i) || i.afterCursor && (de(e.state, r.head, 1, i) || r.head < e.state.doc.length && de(e.state, r.head + 1, -1, i));
      s && (t = t.concat(i.renderMatch(s, e.state)));
    }
    return A.set(t, !0);
  },
  provide: (n) => T.decorations.from(n)
}), cl = [
  al,
  rl
];
function fl(n = {}) {
  return [Sr.of(n), cl];
}
const hl = /* @__PURE__ */ new S();
function An(n, e, t) {
  let i = n.prop(e < 0 ? S.openedBy : S.closedBy);
  if (i)
    return i;
  if (n.name.length == 1) {
    let r = t.indexOf(n.name);
    if (r > -1 && r % 2 == (e < 0 ? 1 : 0))
      return [t[r + e]];
  }
  return null;
}
function Mn(n) {
  let e = n.type.prop(hl);
  return e ? e(n.node) : n;
}
function de(n, e, t, i = {}) {
  let r = i.maxScanDistance || wr, s = i.brackets || vr, o = _(n), l = o.resolveInner(e, t);
  for (let a = l; a; a = a.parent) {
    let c = An(a.type, t, s);
    if (c && a.from < a.to) {
      let f = Mn(a);
      if (f && (t > 0 ? e >= f.from && e < f.to : e > f.from && e <= f.to))
        return ul(n, e, t, a, f, c, s);
    }
  }
  return dl(n, e, t, o, l.type, r, s);
}
function ul(n, e, t, i, r, s, o) {
  let l = i.parent, a = { from: r.from, to: r.to }, c = 0, f = l == null ? void 0 : l.cursor();
  if (f && (t < 0 ? f.childBefore(i.from) : f.childAfter(i.to)))
    do
      if (t < 0 ? f.to <= i.from : f.from >= i.to) {
        if (c == 0 && s.indexOf(f.type.name) > -1 && f.from < f.to) {
          let h = Mn(f);
          return { start: a, end: h ? { from: h.from, to: h.to } : void 0, matched: !0 };
        } else if (An(f.type, t, o))
          c++;
        else if (An(f.type, -t, o)) {
          if (c == 0) {
            let h = Mn(f);
            return {
              start: a,
              end: h && h.from < h.to ? { from: h.from, to: h.to } : void 0,
              matched: !1
            };
          }
          c--;
        }
      }
    while (t < 0 ? f.prevSibling() : f.nextSibling());
  return { start: a, matched: !1 };
}
function dl(n, e, t, i, r, s, o) {
  let l = t < 0 ? n.sliceDoc(e - 1, e) : n.sliceDoc(e, e + 1), a = o.indexOf(l);
  if (a < 0 || a % 2 == 0 != t > 0)
    return null;
  let c = { from: t < 0 ? e - 1 : e, to: t > 0 ? e + 1 : e }, f = n.doc.iterRange(e, t > 0 ? n.doc.length : 0), h = 0;
  for (let u = 0; !f.next().done && u <= s; ) {
    let d = f.value;
    t < 0 && (u += d.length);
    let y = e + u * t;
    for (let g = t > 0 ? 0 : d.length - 1, C = t > 0 ? d.length : -1; g != C; g += t) {
      let P = o.indexOf(d[g]);
      if (!(P < 0 || i.resolveInner(y + g, 1).type != r))
        if (P % 2 == 0 == t > 0)
          h++;
        else {
          if (h == 1)
            return { start: c, end: { from: y + g, to: y + g + 1 }, matched: P >> 1 == a >> 1 };
          h--;
        }
    }
    t > 0 && (u += d.length);
  }
  return f.done ? { start: c, matched: !1 } : null;
}
function Si(n, e, t, i = 0, r = 0) {
  e == null && (e = n.search(/[^\s\u00a0]/), e == -1 && (e = n.length));
  let s = r;
  for (let o = i; o < e; o++)
    n.charCodeAt(o) == 9 ? s += t - s % t : s++;
  return s;
}
class Cr {
  /**
  Create a stream.
  */
  constructor(e, t, i, r) {
    this.string = e, this.tabSize = t, this.indentUnit = i, this.overrideIndent = r, this.pos = 0, this.start = 0, this.lastColumnPos = 0, this.lastColumnValue = 0;
  }
  /**
  True if we are at the end of the line.
  */
  eol() {
    return this.pos >= this.string.length;
  }
  /**
  True if we are at the start of the line.
  */
  sol() {
    return this.pos == 0;
  }
  /**
  Get the next code unit after the current position, or undefined
  if we're at the end of the line.
  */
  peek() {
    return this.string.charAt(this.pos) || void 0;
  }
  /**
  Read the next code unit and advance `this.pos`.
  */
  next() {
    if (this.pos < this.string.length)
      return this.string.charAt(this.pos++);
  }
  /**
  Match the next character against the given string, regular
  expression, or predicate. Consume and return it if it matches.
  */
  eat(e) {
    let t = this.string.charAt(this.pos), i;
    if (typeof e == "string" ? i = t == e : i = t && (e instanceof RegExp ? e.test(t) : e(t)), i)
      return ++this.pos, t;
  }
  /**
  Continue matching characters that match the given string,
  regular expression, or predicate function. Return true if any
  characters were consumed.
  */
  eatWhile(e) {
    let t = this.pos;
    for (; this.eat(e); )
      ;
    return this.pos > t;
  }
  /**
  Consume whitespace ahead of `this.pos`. Return true if any was
  found.
  */
  eatSpace() {
    let e = this.pos;
    for (; /[\s\u00a0]/.test(this.string.charAt(this.pos)); )
      ++this.pos;
    return this.pos > e;
  }
  /**
  Move to the end of the line.
  */
  skipToEnd() {
    this.pos = this.string.length;
  }
  /**
  Move to directly before the given character, if found on the
  current line.
  */
  skipTo(e) {
    let t = this.string.indexOf(e, this.pos);
    if (t > -1)
      return this.pos = t, !0;
  }
  /**
  Move back `n` characters.
  */
  backUp(e) {
    this.pos -= e;
  }
  /**
  Get the column position at `this.pos`.
  */
  column() {
    return this.lastColumnPos < this.start && (this.lastColumnValue = Si(this.string, this.start, this.tabSize, this.lastColumnPos, this.lastColumnValue), this.lastColumnPos = this.start), this.lastColumnValue;
  }
  /**
  Get the indentation column of the current line.
  */
  indentation() {
    var e;
    return (e = this.overrideIndent) !== null && e !== void 0 ? e : Si(this.string, null, this.tabSize);
  }
  /**
  Match the input against the given string or regular expression
  (which should start with a `^`). Return true or the regexp match
  if it matches.
  
  Unless `consume` is set to `false`, this will move `this.pos`
  past the matched text.
  
  When matching a string `caseInsensitive` can be set to true to
  make the match case-insensitive.
  */
  match(e, t, i) {
    if (typeof e == "string") {
      let r = (o) => i ? o.toLowerCase() : o, s = this.string.substr(this.pos, e.length);
      return r(s) == r(e) ? (t !== !1 && (this.pos += e.length), !0) : null;
    } else {
      let r = this.string.slice(this.pos).match(e);
      return r && r.index > 0 ? null : (r && t !== !1 && (this.pos += r[0].length), r);
    }
  }
  /**
  Get the current token.
  */
  current() {
    return this.string.slice(this.start, this.pos);
  }
}
function pl(n) {
  return {
    name: n.name || "",
    token: n.token,
    blankLine: n.blankLine || (() => {
    }),
    startState: n.startState || (() => !0),
    copyState: n.copyState || ml,
    indent: n.indent || (() => null),
    languageData: n.languageData || {},
    tokenTable: n.tokenTable || jn,
    mergeTokens: n.mergeTokens !== !1
  };
}
function ml(n) {
  if (typeof n != "object")
    return n;
  let e = {};
  for (let t in n) {
    let i = n[t];
    e[t] = i instanceof Array ? i.slice() : i;
  }
  return e;
}
const Ci = /* @__PURE__ */ new WeakMap();
class Ar extends ie {
  constructor(e) {
    let t = lr(e.languageData), i = pl(e), r, s = new class extends rr {
      createParse(o, l, a) {
        return new yl(r, o, l, a);
      }
    }();
    super(t, s, [], e.name), this.topNode = kl(t, this), r = this, this.streamParser = i, this.stateAfter = new S({ perNode: !0 }), this.tokenTable = e.tokenTable ? new Pr(i.tokenTable) : xl;
  }
  /**
  Define a stream language.
  */
  static define(e) {
    return new Ar(e);
  }
  /**
  @internal
  */
  getIndent(e) {
    let t, { overrideIndentation: i } = e.options;
    i && (t = Ci.get(e.state), t != null && t < e.pos - 1e4 && (t = void 0));
    let r = _n(this, e.node.tree, e.node.from, e.node.from, t ?? e.pos), s, o;
    if (r ? (o = r.state, s = r.pos + 1) : (o = this.streamParser.startState(e.unit), s = e.node.from), e.pos - s > 1e4)
      return null;
    for (; s < e.pos; ) {
      let a = e.state.doc.lineAt(s), c = Math.min(e.pos, a.to);
      if (a.length) {
        let f = i ? i(a.from) : -1, h = new Cr(a.text, e.state.tabSize, e.unit, f < 0 ? void 0 : f);
        for (; h.pos < c - a.from; )
          Tr(this.streamParser.token, h, o);
      } else
        this.streamParser.blankLine(o, e.unit);
      if (c == e.pos)
        break;
      s = a.to + 1;
    }
    let l = e.lineAt(e.pos);
    return i && t == null && Ci.set(e.state, l.from), this.streamParser.indent(o, /^\s*(.*)/.exec(l.text)[1], e);
  }
  get allowsNesting() {
    return !1;
  }
}
function _n(n, e, t, i, r) {
  let s = t >= i && t + e.length <= r && e.prop(n.stateAfter);
  if (s)
    return { state: n.streamParser.copyState(s), pos: t + e.length };
  for (let o = e.children.length - 1; o >= 0; o--) {
    let l = e.children[o], a = t + e.positions[o], c = l instanceof O && a < r && _n(n, l, a, i, r);
    if (c)
      return c;
  }
  return null;
}
function Mr(n, e, t, i, r) {
  if (r && t <= 0 && i >= e.length)
    return e;
  !r && t == 0 && e.type == n.topNode && (r = !0);
  for (let s = e.children.length - 1; s >= 0; s--) {
    let o = e.positions[s], l = e.children[s], a;
    if (o < i && l instanceof O) {
      if (!(a = Mr(n, l, t - o, i - o, r)))
        break;
      return r ? new O(e.type, e.children.slice(0, s).concat(a), e.positions.slice(0, s + 1), o + a.length) : a;
    }
  }
  return null;
}
function gl(n, e, t, i, r) {
  for (let s of e) {
    let o = s.from + (s.openStart ? 25 : 0), l = s.to - (s.openEnd ? 25 : 0), a = o <= t && l > t && _n(n, s.tree, 0 - s.offset, t, l), c;
    if (a && a.pos <= i && (c = Mr(n, s.tree, t + s.offset, a.pos + s.offset, !1)))
      return { state: a.state, tree: c };
  }
  return { state: n.streamParser.startState(r ? Re(r) : 4), tree: O.empty };
}
class yl {
  constructor(e, t, i, r) {
    this.lang = e, this.input = t, this.fragments = i, this.ranges = r, this.stoppedAt = null, this.chunks = [], this.chunkPos = [], this.chunk = [], this.chunkReused = void 0, this.rangeIndex = 0, this.to = r[r.length - 1].to;
    let s = He.get(), o = r[0].from, { state: l, tree: a } = gl(e, i, o, this.to, s == null ? void 0 : s.state);
    this.state = l, this.parsedPos = this.chunkStart = o + a.length;
    for (let c = 0; c < a.children.length; c++)
      this.chunks.push(a.children[c]), this.chunkPos.push(a.positions[c]);
    s && this.parsedPos < s.viewport.from - 1e5 && r.some((c) => c.from <= s.viewport.from && c.to >= s.viewport.from) && (this.state = this.lang.streamParser.startState(Re(s.state)), s.skipUntilInView(this.parsedPos, s.viewport.from), this.parsedPos = s.viewport.from), this.moveRangeIndex();
  }
  advance() {
    let e = He.get(), t = this.stoppedAt == null ? this.to : Math.min(this.to, this.stoppedAt), i = Math.min(
      t,
      this.chunkStart + 2048
      /* C.ChunkSize */
    );
    for (e && (i = Math.min(i, e.viewport.to)); this.parsedPos < i; )
      this.parseLine(e);
    return this.chunkStart < this.parsedPos && this.finishChunk(), this.parsedPos >= t ? this.finish() : e && this.parsedPos >= e.viewport.to ? (e.skipUntilInView(this.parsedPos, t), this.finish()) : null;
  }
  stopAt(e) {
    this.stoppedAt = e;
  }
  lineAfter(e) {
    let t = this.input.chunk(e);
    if (this.input.lineChunks)
      t == `
` && (t = "");
    else {
      let i = t.indexOf(`
`);
      i > -1 && (t = t.slice(0, i));
    }
    return e + t.length <= this.to ? t : t.slice(0, this.to - e);
  }
  nextLine() {
    let e = this.parsedPos, t = this.lineAfter(e), i = e + t.length;
    for (let r = this.rangeIndex; ; ) {
      let s = this.ranges[r].to;
      if (s >= i || (t = t.slice(0, s - (i - t.length)), r++, r == this.ranges.length))
        break;
      let o = this.ranges[r].from, l = this.lineAfter(o);
      t += l, i = o + l.length;
    }
    return { line: t, end: i };
  }
  skipGapsTo(e, t, i) {
    for (; ; ) {
      let r = this.ranges[this.rangeIndex].to, s = e + t;
      if (i > 0 ? r > s : r >= s)
        break;
      let o = this.ranges[++this.rangeIndex].from;
      t += o - r;
    }
    return t;
  }
  moveRangeIndex() {
    for (; this.ranges[this.rangeIndex].to < this.parsedPos; )
      this.rangeIndex++;
  }
  emitToken(e, t, i, r) {
    let s = 4;
    if (this.ranges.length > 1) {
      r = this.skipGapsTo(t, r, 1), t += r;
      let l = this.chunk.length;
      r = this.skipGapsTo(i, r, -1), i += r, s += this.chunk.length - l;
    }
    let o = this.chunk.length - 4;
    return this.lang.streamParser.mergeTokens && s == 4 && o >= 0 && this.chunk[o] == e && this.chunk[o + 2] == t ? this.chunk[o + 2] = i : this.chunk.push(e, t, i, s), r;
  }
  parseLine(e) {
    let { line: t, end: i } = this.nextLine(), r = 0, { streamParser: s } = this.lang, o = new Cr(t, e ? e.state.tabSize : 4, e ? Re(e.state) : 2);
    if (o.eol())
      s.blankLine(this.state, o.indentUnit);
    else
      for (; !o.eol(); ) {
        let l = Tr(s.token, o, this.state);
        if (l && (r = this.emitToken(this.lang.tokenTable.resolve(l), this.parsedPos + o.start, this.parsedPos + o.pos, r)), o.start > 1e4)
          break;
      }
    this.parsedPos = i, this.moveRangeIndex(), this.parsedPos < this.to && this.parsedPos++;
  }
  finishChunk() {
    let e = O.build({
      buffer: this.chunk,
      start: this.chunkStart,
      length: this.parsedPos - this.chunkStart,
      nodeSet: bl,
      topID: 0,
      maxBufferLength: 2048,
      reused: this.chunkReused
    });
    e = new O(e.type, e.children, e.positions, e.length, [[this.lang.stateAfter, this.lang.streamParser.copyState(this.state)]]), this.chunks.push(e), this.chunkPos.push(this.chunkStart - this.ranges[0].from), this.chunk = [], this.chunkReused = void 0, this.chunkStart = this.parsedPos;
  }
  finish() {
    return new O(this.lang.topNode, this.chunks, this.chunkPos, this.parsedPos - this.ranges[0].from).balance();
  }
}
function Tr(n, e, t) {
  e.start = e.pos;
  for (let i = 0; i < 10; i++) {
    let r = n(e, t);
    if (e.pos > e.start)
      return r;
  }
  throw new Error("Stream parser failed to advance stream.");
}
const jn = /* @__PURE__ */ Object.create(null), at = [G.none], bl = /* @__PURE__ */ new Fn(at), Ai = [], Mi = /* @__PURE__ */ Object.create(null), Ir = /* @__PURE__ */ Object.create(null);
for (let [n, e] of [
  ["variable", "variableName"],
  ["variable-2", "variableName.special"],
  ["string-2", "string.special"],
  ["def", "variableName.definition"],
  ["tag", "tagName"],
  ["attribute", "attributeName"],
  ["type", "typeName"],
  ["builtin", "variableName.standard"],
  ["qualifier", "modifier"],
  ["error", "invalid"],
  ["header", "heading"],
  ["property", "propertyName"]
])
  Ir[n] = /* @__PURE__ */ Br(jn, e);
class Pr {
  constructor(e) {
    this.extra = e, this.table = Object.assign(/* @__PURE__ */ Object.create(null), Ir);
  }
  resolve(e) {
    return e ? this.table[e] || (this.table[e] = Br(this.extra, e)) : 0;
  }
}
const xl = /* @__PURE__ */ new Pr(jn);
function dn(n, e) {
  Ai.indexOf(n) > -1 || (Ai.push(n), console.warn(e));
}
function Br(n, e) {
  let t = [];
  for (let l of e.split(" ")) {
    let a = [];
    for (let c of l.split(".")) {
      let f = n[c] || m[c];
      f ? typeof f == "function" ? a.length ? a = a.map(f) : dn(c, `Modifier ${c} used at start of tag`) : a.length ? dn(c, `Tag ${c} used as modifier`) : a = Array.isArray(f) ? f : [f] : dn(c, `Unknown highlighting tag ${c}`);
    }
    for (let c of a)
      t.push(c);
  }
  if (!t.length)
    return 0;
  let i = e.replace(/ /g, "_"), r = i + " " + t.map((l) => l.id), s = Mi[r];
  if (s)
    return s.id;
  let o = Mi[r] = G.define({
    id: at.length,
    name: i,
    props: [vo({ [i]: t })]
  });
  return at.push(o), o.id;
}
function kl(n, e) {
  let t = G.define({ id: at.length, name: "Document", props: [
    De.add(() => n),
    fr.add(() => (i) => e.getIndent(i))
  ], top: !0 });
  return at.push(t), t;
}
Et.RTL, Et.LTR;
const wl = (n) => {
  let { state: e } = n, t = e.doc.lineAt(e.selection.main.from), i = Vn(n.state, t.from);
  return i.line ? vl(n) : i.block ? Cl(n) : !1;
};
function Hn(n, e) {
  return ({ state: t, dispatch: i }) => {
    if (t.readOnly)
      return !1;
    let r = n(e, t);
    return r ? (i(t.update(r)), !0) : !1;
  };
}
const vl = /* @__PURE__ */ Hn(
  Tl,
  0
  /* CommentOption.Toggle */
), Sl = /* @__PURE__ */ Hn(
  Dr,
  0
  /* CommentOption.Toggle */
), Cl = /* @__PURE__ */ Hn(
  (n, e) => Dr(n, e, Ml(e)),
  0
  /* CommentOption.Toggle */
);
function Vn(n, e) {
  let t = n.languageDataAt("commentTokens", e);
  return t.length ? t[0] : {};
}
const nt = 50;
function Al(n, { open: e, close: t }, i, r) {
  let s = n.sliceDoc(i - nt, i), o = n.sliceDoc(r, r + nt), l = /\s*$/.exec(s)[0].length, a = /^\s*/.exec(o)[0].length, c = s.length - l;
  if (s.slice(c - e.length, c) == e && o.slice(a, a + t.length) == t)
    return {
      open: { pos: i - l, margin: l && 1 },
      close: { pos: r + a, margin: a && 1 }
    };
  let f, h;
  r - i <= 2 * nt ? f = h = n.sliceDoc(i, r) : (f = n.sliceDoc(i, i + nt), h = n.sliceDoc(r - nt, r));
  let u = /^\s*/.exec(f)[0].length, d = /\s*$/.exec(h)[0].length, y = h.length - d - t.length;
  return f.slice(u, u + e.length) == e && h.slice(y, y + t.length) == t ? {
    open: {
      pos: i + u + e.length,
      margin: /\s/.test(f.charAt(u + e.length)) ? 1 : 0
    },
    close: {
      pos: r - d - t.length,
      margin: /\s/.test(h.charAt(y - 1)) ? 1 : 0
    }
  } : null;
}
function Ml(n) {
  let e = [];
  for (let t of n.selection.ranges) {
    let i = n.doc.lineAt(t.from), r = t.to <= i.to ? i : n.doc.lineAt(t.to);
    r.from > i.from && r.from == t.to && (r = t.to == i.to + 1 ? i : n.doc.lineAt(t.to - 1));
    let s = e.length - 1;
    s >= 0 && e[s].to > i.from ? e[s].to = r.to : e.push({ from: i.from + /^\s*/.exec(i.text)[0].length, to: r.to });
  }
  return e;
}
function Dr(n, e, t = e.selection.ranges) {
  let i = t.map((s) => Vn(e, s.from).block);
  if (!i.every((s) => s))
    return null;
  let r = t.map((s, o) => Al(e, i[o], s.from, s.to));
  if (n != 2 && !r.every((s) => s))
    return { changes: e.changes(t.map((s, o) => r[o] ? [] : [{ from: s.from, insert: i[o].open + " " }, { from: s.to, insert: " " + i[o].close }])) };
  if (n != 1 && r.some((s) => s)) {
    let s = [];
    for (let o = 0, l; o < r.length; o++)
      if (l = r[o]) {
        let a = i[o], { open: c, close: f } = l;
        s.push({ from: c.pos - a.open.length, to: c.pos + c.margin }, { from: f.pos - f.margin, to: f.pos + a.close.length });
      }
    return { changes: s };
  }
  return null;
}
function Tl(n, e, t = e.selection.ranges) {
  let i = [], r = -1;
  for (let { from: s, to: o } of t) {
    let l = i.length, a = 1e9, c = Vn(e, s).line;
    if (c) {
      for (let f = s; f <= o; ) {
        let h = e.doc.lineAt(f);
        if (h.from > r && (s == o || o > h.from)) {
          r = h.from;
          let u = /^\s*/.exec(h.text)[0].length, d = u == h.length, y = h.text.slice(u, u + c.length) == c ? u : -1;
          u < h.text.length && u < a && (a = u), i.push({ line: h, comment: y, token: c, indent: u, empty: d, single: !1 });
        }
        f = h.to + 1;
      }
      if (a < 1e9)
        for (let f = l; f < i.length; f++)
          i[f].indent < i[f].line.text.length && (i[f].indent = a);
      i.length == l + 1 && (i[l].single = !0);
    }
  }
  if (n != 2 && i.some((s) => s.comment < 0 && (!s.empty || s.single))) {
    let s = [];
    for (let { line: l, token: a, indent: c, empty: f, single: h } of i)
      (h || !f) && s.push({ from: l.from + c, insert: a + " " });
    let o = e.changes(s);
    return { changes: o, selection: e.selection.map(o, 1) };
  } else if (n != 1 && i.some((s) => s.comment >= 0)) {
    let s = [];
    for (let { line: o, comment: l, token: a } of i)
      if (l >= 0) {
        let c = o.from + l, f = c + a.length;
        o.text[f - o.from] == " " && f++, s.push({ from: c, to: f });
      }
    return { changes: s };
  }
  return null;
}
const Tn = /* @__PURE__ */ En.define(), Il = /* @__PURE__ */ En.define(), Pl = /* @__PURE__ */ H.define(), Lr = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, {
      minDepth: 100,
      newGroupDelay: 500,
      joinToEvent: (e, t) => t
    }, {
      minDepth: Math.max,
      newGroupDelay: Math.min,
      joinToEvent: (e, t) => (i, r) => e(i, r) || t(i, r)
    });
  }
}), Or = /* @__PURE__ */ me.define({
  create() {
    return pe.empty;
  },
  update(n, e) {
    let t = e.state.facet(Lr), i = e.annotation(Tn);
    if (i) {
      let a = Q.fromTransaction(e, i.selection), c = i.side, f = c == 0 ? n.undone : n.done;
      return a ? f = zt(f, f.length, t.minDepth, a) : f = Rr(f, e.startState.selection), new pe(c == 0 ? i.rest : f, c == 0 ? f : i.rest);
    }
    let r = e.annotation(Il);
    if ((r == "full" || r == "before") && (n = n.isolate()), e.annotation(Dt.addToHistory) === !1)
      return e.changes.empty ? n : n.addMapping(e.changes.desc);
    let s = Q.fromTransaction(e), o = e.annotation(Dt.time), l = e.annotation(Dt.userEvent);
    return s ? n = n.addChanges(s, o, l, t, e) : e.selection && (n = n.addSelection(e.startState.selection, o, l, t.newGroupDelay)), (r == "full" || r == "after") && (n = n.isolate()), n;
  },
  toJSON(n) {
    return { done: n.done.map((e) => e.toJSON()), undone: n.undone.map((e) => e.toJSON()) };
  },
  fromJSON(n) {
    return new pe(n.done.map(Q.fromJSON), n.undone.map(Q.fromJSON));
  }
});
function Bl(n = {}) {
  return [
    Or,
    Lr.of(n),
    T.domEventHandlers({
      beforeinput(e, t) {
        let i = e.inputType == "historyUndo" ? Er : e.inputType == "historyRedo" ? In : null;
        return i ? (e.preventDefault(), i(t)) : !1;
      }
    })
  ];
}
function nn(n, e) {
  return function({ state: t, dispatch: i }) {
    if (!e && t.readOnly)
      return !1;
    let r = t.field(Or, !1);
    if (!r)
      return !1;
    let s = r.pop(n, t, e);
    return s ? (i(s), !0) : !1;
  };
}
const Er = /* @__PURE__ */ nn(0, !1), In = /* @__PURE__ */ nn(1, !1), Dl = /* @__PURE__ */ nn(0, !0), Ll = /* @__PURE__ */ nn(1, !0);
class Q {
  constructor(e, t, i, r, s) {
    this.changes = e, this.effects = t, this.mapped = i, this.startSelection = r, this.selectionsAfter = s;
  }
  setSelAfter(e) {
    return new Q(this.changes, this.effects, this.mapped, this.startSelection, e);
  }
  toJSON() {
    var e, t, i;
    return {
      changes: (e = this.changes) === null || e === void 0 ? void 0 : e.toJSON(),
      mapped: (t = this.mapped) === null || t === void 0 ? void 0 : t.toJSON(),
      startSelection: (i = this.startSelection) === null || i === void 0 ? void 0 : i.toJSON(),
      selectionsAfter: this.selectionsAfter.map((r) => r.toJSON())
    };
  }
  static fromJSON(e) {
    return new Q(e.changes && Fs.fromJSON(e.changes), [], e.mapped && Ws.fromJSON(e.mapped), e.startSelection && x.fromJSON(e.startSelection), e.selectionsAfter.map(x.fromJSON));
  }
  // This does not check `addToHistory` and such, it assumes the
  // transaction needs to be converted to an item. Returns null when
  // there are no changes or effects in the transaction.
  static fromTransaction(e, t) {
    let i = re;
    for (let r of e.startState.facet(Pl)) {
      let s = r(e);
      s.length && (i = i.concat(s));
    }
    return !i.length && e.changes.empty ? null : new Q(e.changes.invert(e.startState.doc), i, void 0, t || e.startState.selection, re);
  }
  static selection(e) {
    return new Q(void 0, re, void 0, void 0, e);
  }
}
function zt(n, e, t, i) {
  let r = e + 1 > t + 20 ? e - t - 1 : 0, s = n.slice(r, e);
  return s.push(i), s;
}
function Ol(n, e) {
  let t = [], i = !1;
  return n.iterChangedRanges((r, s) => t.push(r, s)), e.iterChangedRanges((r, s, o, l) => {
    for (let a = 0; a < t.length; ) {
      let c = t[a++], f = t[a++];
      l >= c && o <= f && (i = !0);
    }
  }), i;
}
function El(n, e) {
  return n.ranges.length == e.ranges.length && n.ranges.filter((t, i) => t.empty != e.ranges[i].empty).length === 0;
}
function Nr(n, e) {
  return n.length ? e.length ? n.concat(e) : n : e;
}
const re = [], Nl = 200;
function Rr(n, e) {
  if (n.length) {
    let t = n[n.length - 1], i = t.selectionsAfter.slice(Math.max(0, t.selectionsAfter.length - Nl));
    return i.length && i[i.length - 1].eq(e) ? n : (i.push(e), zt(n, n.length - 1, 1e9, t.setSelAfter(i)));
  } else
    return [Q.selection([e])];
}
function Rl(n) {
  let e = n[n.length - 1], t = n.slice();
  return t[n.length - 1] = e.setSelAfter(e.selectionsAfter.slice(0, e.selectionsAfter.length - 1)), t;
}
function pn(n, e) {
  if (!n.length)
    return n;
  let t = n.length, i = re;
  for (; t; ) {
    let r = Fl(n[t - 1], e, i);
    if (r.changes && !r.changes.empty || r.effects.length) {
      let s = n.slice(0, t);
      return s[t - 1] = r, s;
    } else
      e = r.mapped, t--, i = r.selectionsAfter;
  }
  return i.length ? [Q.selection(i)] : re;
}
function Fl(n, e, t) {
  let i = Nr(n.selectionsAfter.length ? n.selectionsAfter.map((l) => l.map(e)) : re, t);
  if (!n.changes)
    return Q.selection(i);
  let r = n.changes.map(e), s = e.mapDesc(n.changes, !0), o = n.mapped ? n.mapped.composeDesc(s) : s;
  return new Q(r, F.mapEffects(n.effects, e), o, n.startSelection.map(s), i);
}
const Wl = /^(input\.type|delete)($|\.)/;
class pe {
  constructor(e, t, i = 0, r = void 0) {
    this.done = e, this.undone = t, this.prevTime = i, this.prevUserEvent = r;
  }
  isolate() {
    return this.prevTime ? new pe(this.done, this.undone) : this;
  }
  addChanges(e, t, i, r, s) {
    let o = this.done, l = o[o.length - 1];
    return l && l.changes && !l.changes.empty && e.changes && (!i || Wl.test(i)) && (!l.selectionsAfter.length && t - this.prevTime < r.newGroupDelay && r.joinToEvent(s, Ol(l.changes, e.changes)) || // For compose (but not compose.start) events, always join with previous event
    i == "input.type.compose") ? o = zt(o, o.length - 1, r.minDepth, new Q(e.changes.compose(l.changes), Nr(F.mapEffects(e.effects, l.changes), l.effects), l.mapped, l.startSelection, re)) : o = zt(o, o.length, r.minDepth, e), new pe(o, re, t, i);
  }
  addSelection(e, t, i, r) {
    let s = this.done.length ? this.done[this.done.length - 1].selectionsAfter : re;
    return s.length > 0 && t - this.prevTime < r && i == this.prevUserEvent && i && /^select($|\.)/.test(i) && El(s[s.length - 1], e) ? this : new pe(Rr(this.done, e), this.undone, t, i);
  }
  addMapping(e) {
    return new pe(pn(this.done, e), pn(this.undone, e), this.prevTime, this.prevUserEvent);
  }
  pop(e, t, i) {
    let r = e == 0 ? this.done : this.undone;
    if (r.length == 0)
      return null;
    let s = r[r.length - 1], o = s.selectionsAfter[0] || t.selection;
    if (i && s.selectionsAfter.length)
      return t.update({
        selection: s.selectionsAfter[s.selectionsAfter.length - 1],
        annotations: Tn.of({ side: e, rest: Rl(r), selection: o }),
        userEvent: e == 0 ? "select.undo" : "select.redo",
        scrollIntoView: !0
      });
    if (s.changes) {
      let l = r.length == 1 ? re : r.slice(0, r.length - 1);
      return s.mapped && (l = pn(l, s.mapped)), t.update({
        changes: s.changes,
        selection: s.startSelection,
        effects: s.effects,
        annotations: Tn.of({ side: e, rest: l, selection: o }),
        filter: !1,
        userEvent: e == 0 ? "undo" : "redo",
        scrollIntoView: !0
      });
    } else
      return null;
  }
}
pe.empty = /* @__PURE__ */ new pe(re, re);
const ql = [
  { key: "Mod-z", run: Er, preventDefault: !0 },
  { key: "Mod-y", mac: "Mod-Shift-z", run: In, preventDefault: !0 },
  { linux: "Ctrl-Shift-z", run: In, preventDefault: !0 },
  { key: "Mod-u", run: Dl, preventDefault: !0 },
  { key: "Alt-u", mac: "Mod-Shift-u", run: Ll, preventDefault: !0 }
];
function Qe(n, e) {
  return x.create(n.ranges.map(e), n.mainIndex);
}
function ge(n, e) {
  return n.update({ selection: e, scrollIntoView: !0, userEvent: "select" });
}
function le({ state: n, dispatch: e }, t) {
  let i = Qe(n.selection, t);
  return i.eq(n.selection, !0) ? !1 : (e(ge(n, i)), !0);
}
function rn(n, e) {
  return x.cursor(e ? n.to : n.from);
}
function Fr(n, e) {
  return le(n, (t) => t.empty ? n.moveByChar(t, e) : rn(t, e));
}
function V(n) {
  return n.textDirectionAt(n.state.selection.main.head) == Et.LTR;
}
const Wr = (n) => Fr(n, !V(n)), qr = (n) => Fr(n, V(n));
function zr(n, e) {
  return le(n, (t) => t.empty ? n.moveByGroup(t, e) : rn(t, e));
}
const zl = (n) => zr(n, !V(n)), $l = (n) => zr(n, V(n));
function Ul(n, e, t) {
  if (e.type.prop(t))
    return !0;
  let i = e.to - e.from;
  return i && (i > 2 || /[^\s,.;:]/.test(n.sliceDoc(e.from, e.to))) || e.firstChild;
}
function sn(n, e, t) {
  let i = _(n).resolveInner(e.head), r = t ? S.closedBy : S.openedBy;
  for (let a = e.head; ; ) {
    let c = t ? i.childAfter(a) : i.childBefore(a);
    if (!c)
      break;
    Ul(n, c, r) ? i = c : a = t ? c.to : c.from;
  }
  let s = i.type.prop(r), o, l;
  return s && (o = t ? de(n, i.from, 1) : de(n, i.to, -1)) && o.matched ? l = t ? o.end.to : o.end.from : l = t ? i.to : i.from, x.cursor(l, t ? -1 : 1);
}
const _l = (n) => le(n, (e) => sn(n.state, e, !V(n))), jl = (n) => le(n, (e) => sn(n.state, e, V(n)));
function $r(n, e) {
  return le(n, (t) => {
    if (!t.empty)
      return rn(t, e);
    let i = n.moveVertically(t, e);
    return i.head != t.head ? i : n.moveToLineBoundary(t, e);
  });
}
const Ur = (n) => $r(n, !1), _r = (n) => $r(n, !0);
function jr(n) {
  let e = n.scrollDOM.clientHeight < n.scrollDOM.scrollHeight - 2, t = 0, i = 0, r;
  if (e) {
    for (let s of n.state.facet(T.scrollMargins)) {
      let o = s(n);
      o != null && o.top && (t = Math.max(o == null ? void 0 : o.top, t)), o != null && o.bottom && (i = Math.max(o == null ? void 0 : o.bottom, i));
    }
    r = n.scrollDOM.clientHeight - t - i;
  } else
    r = (n.dom.ownerDocument.defaultView || window).innerHeight;
  return {
    marginTop: t,
    marginBottom: i,
    selfScroll: e,
    height: Math.max(n.defaultLineHeight, r - 5)
  };
}
function Hr(n, e) {
  let t = jr(n), { state: i } = n, r = Qe(i.selection, (o) => o.empty ? n.moveVertically(o, e, t.height) : rn(o, e));
  if (r.eq(i.selection))
    return !1;
  let s;
  if (t.selfScroll) {
    let o = n.coordsAtPos(i.selection.main.head), l = n.scrollDOM.getBoundingClientRect(), a = l.top + t.marginTop, c = l.bottom - t.marginBottom;
    o && o.top > a && o.bottom < c && (s = T.scrollIntoView(r.main.head, { y: "start", yMargin: o.top - a }));
  }
  return n.dispatch(ge(i, r), { effects: s }), !0;
}
const Ti = (n) => Hr(n, !1), Pn = (n) => Hr(n, !0);
function Me(n, e, t) {
  let i = n.lineBlockAt(e.head), r = n.moveToLineBoundary(e, t);
  if (r.head == e.head && r.head != (t ? i.to : i.from) && (r = n.moveToLineBoundary(e, t, !1)), !t && r.head == i.from && i.length) {
    let s = /^\s*/.exec(n.state.sliceDoc(i.from, Math.min(i.from + 100, i.to)))[0].length;
    s && e.head != i.from + s && (r = x.cursor(i.from + s));
  }
  return r;
}
const Hl = (n) => le(n, (e) => Me(n, e, !0)), Vl = (n) => le(n, (e) => Me(n, e, !1)), Kl = (n) => le(n, (e) => Me(n, e, !V(n))), Gl = (n) => le(n, (e) => Me(n, e, V(n))), Jl = (n) => le(n, (e) => x.cursor(n.lineBlockAt(e.head).from, 1)), Ql = (n) => le(n, (e) => x.cursor(n.lineBlockAt(e.head).to, -1));
function Zl(n, e, t) {
  let i = !1, r = Qe(n.selection, (s) => {
    let o = de(n, s.head, -1) || de(n, s.head, 1) || s.head > 0 && de(n, s.head - 1, 1) || s.head < n.doc.length && de(n, s.head + 1, -1);
    if (!o || !o.end)
      return s;
    i = !0;
    let l = o.start.from == s.head ? o.end.to : o.end.from;
    return x.cursor(l);
  });
  return i ? (e(ge(n, r)), !0) : !1;
}
const Yl = ({ state: n, dispatch: e }) => Zl(n, e);
function oe(n, e) {
  let t = Qe(n.state.selection, (i) => {
    let r = e(i);
    return x.range(i.anchor, r.head, r.goalColumn, r.bidiLevel || void 0);
  });
  return t.eq(n.state.selection) ? !1 : (n.dispatch(ge(n.state, t)), !0);
}
function Vr(n, e) {
  return oe(n, (t) => n.moveByChar(t, e));
}
const Kr = (n) => Vr(n, !V(n)), Gr = (n) => Vr(n, V(n));
function Jr(n, e) {
  return oe(n, (t) => n.moveByGroup(t, e));
}
const Xl = (n) => Jr(n, !V(n)), ea = (n) => Jr(n, V(n)), ta = (n) => oe(n, (e) => sn(n.state, e, !V(n))), na = (n) => oe(n, (e) => sn(n.state, e, V(n)));
function Qr(n, e) {
  return oe(n, (t) => n.moveVertically(t, e));
}
const Zr = (n) => Qr(n, !1), Yr = (n) => Qr(n, !0);
function Xr(n, e) {
  return oe(n, (t) => n.moveVertically(t, e, jr(n).height));
}
const Ii = (n) => Xr(n, !1), Pi = (n) => Xr(n, !0), ia = (n) => oe(n, (e) => Me(n, e, !0)), ra = (n) => oe(n, (e) => Me(n, e, !1)), sa = (n) => oe(n, (e) => Me(n, e, !V(n))), oa = (n) => oe(n, (e) => Me(n, e, V(n))), la = (n) => oe(n, (e) => x.cursor(n.lineBlockAt(e.head).from)), aa = (n) => oe(n, (e) => x.cursor(n.lineBlockAt(e.head).to)), Bi = ({ state: n, dispatch: e }) => (e(ge(n, { anchor: 0 })), !0), Di = ({ state: n, dispatch: e }) => (e(ge(n, { anchor: n.doc.length })), !0), Li = ({ state: n, dispatch: e }) => (e(ge(n, { anchor: n.selection.main.anchor, head: 0 })), !0), Oi = ({ state: n, dispatch: e }) => (e(ge(n, { anchor: n.selection.main.anchor, head: n.doc.length })), !0), ca = ({ state: n, dispatch: e }) => (e(n.update({ selection: { anchor: 0, head: n.doc.length }, userEvent: "select" })), !0), fa = ({ state: n, dispatch: e }) => {
  let t = on(n).map(({ from: i, to: r }) => x.range(i, Math.min(r + 1, n.doc.length)));
  return e(n.update({ selection: x.create(t), userEvent: "select" })), !0;
}, ha = ({ state: n, dispatch: e }) => {
  let t = Qe(n.selection, (i) => {
    let r = _(n), s = r.resolveStack(i.from, 1);
    if (i.empty) {
      let o = r.resolveStack(i.from, -1);
      o.node.from >= s.node.from && o.node.to <= s.node.to && (s = o);
    }
    for (let o = s; o; o = o.next) {
      let { node: l } = o;
      if ((l.from < i.from && l.to >= i.to || l.to > i.to && l.from <= i.from) && o.next)
        return x.range(l.to, l.from);
    }
    return i;
  });
  return t.eq(n.selection) ? !1 : (e(ge(n, t)), !0);
}, ua = ({ state: n, dispatch: e }) => {
  let t = n.selection, i = null;
  return t.ranges.length > 1 ? i = x.create([t.main]) : t.main.empty || (i = x.create([x.cursor(t.main.head)])), i ? (e(ge(n, i)), !0) : !1;
};
function yt(n, e) {
  if (n.state.readOnly)
    return !1;
  let t = "delete.selection", { state: i } = n, r = i.changeByRange((s) => {
    let { from: o, to: l } = s;
    if (o == l) {
      let a = e(s);
      a < o ? (t = "delete.backward", a = Mt(n, a, !1)) : a > o && (t = "delete.forward", a = Mt(n, a, !0)), o = Math.min(o, a), l = Math.max(l, a);
    } else
      o = Mt(n, o, !1), l = Mt(n, l, !0);
    return o == l ? { range: s } : { changes: { from: o, to: l }, range: x.cursor(o, o < s.head ? -1 : 1) };
  });
  return r.changes.empty ? !1 : (n.dispatch(i.update(r, {
    scrollIntoView: !0,
    userEvent: t,
    effects: t == "delete.selection" ? T.announce.of(i.phrase("Selection deleted")) : void 0
  })), !0);
}
function Mt(n, e, t) {
  if (n instanceof T)
    for (let i of n.state.facet(T.atomicRanges).map((r) => r(n)))
      i.between(e, e, (r, s) => {
        r < e && s > e && (e = t ? s : r);
      });
  return e;
}
const es = (n, e, t) => yt(n, (i) => {
  let r = i.from, { state: s } = n, o = s.doc.lineAt(r), l, a;
  if (t && !e && r > o.from && r < o.from + 200 && !/[^ \t]/.test(l = o.text.slice(0, r - o.from))) {
    if (l[l.length - 1] == "	")
      return r - 1;
    let c = Jt(l, s.tabSize), f = c % Re(s) || Re(s);
    for (let h = 0; h < f && l[l.length - 1 - h] == " "; h++)
      r--;
    a = r;
  } else
    a = Ne(o.text, r - o.from, e, e) + o.from, a == r && o.number != (e ? s.doc.lines : 1) ? a += e ? 1 : -1 : !e && /[\ufe00-\ufe0f]/.test(o.text.slice(a - o.from, r - o.from)) && (a = Ne(o.text, a - o.from, !1, !1) + o.from);
  return a;
}), Bn = (n) => es(n, !1, !0), ts = (n) => es(n, !0, !1), ns = (n, e) => yt(n, (t) => {
  let i = t.head, { state: r } = n, s = r.doc.lineAt(i), o = r.charCategorizer(i);
  for (let l = null; ; ) {
    if (i == (e ? s.to : s.from)) {
      i == t.head && s.number != (e ? r.doc.lines : 1) && (i += e ? 1 : -1);
      break;
    }
    let a = Ne(s.text, i - s.from, e) + s.from, c = s.text.slice(Math.min(i, a) - s.from, Math.max(i, a) - s.from), f = o(c);
    if (l != null && f != l)
      break;
    (c != " " || i != t.head) && (l = f), i = a;
  }
  return i;
}), is = (n) => ns(n, !1), da = (n) => ns(n, !0), pa = (n) => yt(n, (e) => {
  let t = n.lineBlockAt(e.head).to;
  return e.head < t ? t : Math.min(n.state.doc.length, e.head + 1);
}), ma = (n) => yt(n, (e) => {
  let t = n.moveToLineBoundary(e, !1).head;
  return e.head > t ? t : Math.max(0, e.head - 1);
}), ga = (n) => yt(n, (e) => {
  let t = n.moveToLineBoundary(e, !0).head;
  return e.head < t ? t : Math.min(n.state.doc.length, e.head + 1);
}), ya = ({ state: n, dispatch: e }) => {
  if (n.readOnly)
    return !1;
  let t = n.changeByRange((i) => ({
    changes: { from: i.from, to: i.to, insert: On.of(["", ""]) },
    range: x.cursor(i.from)
  }));
  return e(n.update(t, { scrollIntoView: !0, userEvent: "input" })), !0;
}, ba = ({ state: n, dispatch: e }) => {
  if (n.readOnly)
    return !1;
  let t = n.changeByRange((i) => {
    if (!i.empty || i.from == 0 || i.from == n.doc.length)
      return { range: i };
    let r = i.from, s = n.doc.lineAt(r), o = r == s.from ? r - 1 : Ne(s.text, r - s.from, !1) + s.from, l = r == s.to ? r + 1 : Ne(s.text, r - s.from, !0) + s.from;
    return {
      changes: { from: o, to: l, insert: n.doc.slice(r, l).append(n.doc.slice(o, r)) },
      range: x.cursor(l)
    };
  });
  return t.changes.empty ? !1 : (e(n.update(t, { scrollIntoView: !0, userEvent: "move.character" })), !0);
};
function on(n) {
  let e = [], t = -1;
  for (let i of n.selection.ranges) {
    let r = n.doc.lineAt(i.from), s = n.doc.lineAt(i.to);
    if (!i.empty && i.to == s.from && (s = n.doc.lineAt(i.to - 1)), t >= r.number) {
      let o = e[e.length - 1];
      o.to = s.to, o.ranges.push(i);
    } else
      e.push({ from: r.from, to: s.to, ranges: [i] });
    t = s.number + 1;
  }
  return e;
}
function rs(n, e, t) {
  if (n.readOnly)
    return !1;
  let i = [], r = [];
  for (let s of on(n)) {
    if (t ? s.to == n.doc.length : s.from == 0)
      continue;
    let o = n.doc.lineAt(t ? s.to + 1 : s.from - 1), l = o.length + 1;
    if (t) {
      i.push({ from: s.to, to: o.to }, { from: s.from, insert: o.text + n.lineBreak });
      for (let a of s.ranges)
        r.push(x.range(Math.min(n.doc.length, a.anchor + l), Math.min(n.doc.length, a.head + l)));
    } else {
      i.push({ from: o.from, to: s.from }, { from: s.to, insert: n.lineBreak + o.text });
      for (let a of s.ranges)
        r.push(x.range(a.anchor - l, a.head - l));
    }
  }
  return i.length ? (e(n.update({
    changes: i,
    scrollIntoView: !0,
    selection: x.create(r, n.selection.mainIndex),
    userEvent: "move.line"
  })), !0) : !1;
}
const xa = ({ state: n, dispatch: e }) => rs(n, e, !1), ka = ({ state: n, dispatch: e }) => rs(n, e, !0);
function ss(n, e, t) {
  if (n.readOnly)
    return !1;
  let i = [];
  for (let r of on(n))
    t ? i.push({ from: r.from, insert: n.doc.slice(r.from, r.to) + n.lineBreak }) : i.push({ from: r.to, insert: n.lineBreak + n.doc.slice(r.from, r.to) });
  return e(n.update({ changes: i, scrollIntoView: !0, userEvent: "input.copyline" })), !0;
}
const wa = ({ state: n, dispatch: e }) => ss(n, e, !1), va = ({ state: n, dispatch: e }) => ss(n, e, !0), Sa = (n) => {
  if (n.state.readOnly)
    return !1;
  let { state: e } = n, t = e.changes(on(e).map(({ from: r, to: s }) => (r > 0 ? r-- : s < e.doc.length && s++, { from: r, to: s }))), i = Qe(e.selection, (r) => {
    let s;
    if (n.lineWrapping) {
      let o = n.lineBlockAt(r.head), l = n.coordsAtPos(r.head, r.assoc || 1);
      l && (s = o.bottom + n.documentTop - l.bottom + n.defaultLineHeight / 2);
    }
    return n.moveVertically(r, !0, s);
  }).map(t);
  return n.dispatch({ changes: t, selection: i, scrollIntoView: !0, userEvent: "delete.line" }), !0;
};
function Ca(n, e) {
  if (/\(\)|\[\]|\{\}/.test(n.sliceDoc(e - 1, e + 1)))
    return { from: e, to: e };
  let t = _(n).resolveInner(e), i = t.childBefore(e), r = t.childAfter(e), s;
  return i && r && i.to <= e && r.from >= e && (s = i.type.prop(S.closedBy)) && s.indexOf(r.name) > -1 && n.doc.lineAt(i.to).from == n.doc.lineAt(r.from).from && !/\S/.test(n.sliceDoc(i.to, r.from)) ? { from: i.to, to: r.from } : null;
}
const Ei = /* @__PURE__ */ os(!1), Aa = /* @__PURE__ */ os(!0);
function os(n) {
  return ({ state: e, dispatch: t }) => {
    if (e.readOnly)
      return !1;
    let i = e.changeByRange((r) => {
      let { from: s, to: o } = r, l = e.doc.lineAt(s), a = !n && s == o && Ca(e, s);
      n && (s = o = (o <= l.to ? l : e.doc.lineAt(o)).to);
      let c = new Xt(e, { simulateBreak: s, simulateDoubleBreak: !!a }), f = $n(c, s);
      for (f == null && (f = Jt(/^\s*/.exec(e.doc.lineAt(s).text)[0], e.tabSize)); o < l.to && /\s/.test(l.text[o - l.from]); )
        o++;
      a ? { from: s, to: o } = a : s > l.from && s < l.from + 100 && !/\S/.test(l.text.slice(0, s)) && (s = l.from);
      let h = ["", lt(e, f)];
      return a && h.push(lt(e, c.lineIndent(l.from, -1))), {
        changes: { from: s, to: o, insert: On.of(h) },
        range: x.cursor(s + 1 + h[1].length)
      };
    });
    return t(e.update(i, { scrollIntoView: !0, userEvent: "input" })), !0;
  };
}
function Kn(n, e) {
  let t = -1;
  return n.changeByRange((i) => {
    let r = [];
    for (let o = i.from; o <= i.to; ) {
      let l = n.doc.lineAt(o);
      l.number > t && (i.empty || i.to > l.from) && (e(l, r, i), t = l.number), o = l.to + 1;
    }
    let s = n.changes(r);
    return {
      changes: r,
      range: x.range(s.mapPos(i.anchor, 1), s.mapPos(i.head, 1))
    };
  });
}
const Ma = ({ state: n, dispatch: e }) => {
  if (n.readOnly)
    return !1;
  let t = /* @__PURE__ */ Object.create(null), i = new Xt(n, { overrideIndentation: (s) => {
    let o = t[s];
    return o ?? -1;
  } }), r = Kn(n, (s, o, l) => {
    let a = $n(i, s.from);
    if (a == null)
      return;
    /\S/.test(s.text) || (a = 0);
    let c = /^\s*/.exec(s.text)[0], f = lt(n, a);
    (c != f || l.from < s.from + c.length) && (t[s.from] = a, o.push({ from: s.from, to: s.from + c.length, insert: f }));
  });
  return r.changes.empty || e(n.update(r, { userEvent: "indent" })), !0;
}, ls = ({ state: n, dispatch: e }) => n.readOnly ? !1 : (e(n.update(Kn(n, (t, i) => {
  i.push({ from: t.from, insert: n.facet(Yt) });
}), { userEvent: "input.indent" })), !0), as = ({ state: n, dispatch: e }) => n.readOnly ? !1 : (e(n.update(Kn(n, (t, i) => {
  let r = /^\s*/.exec(t.text)[0];
  if (!r)
    return;
  let s = Jt(r, n.tabSize), o = 0, l = lt(n, Math.max(0, s - Re(n)));
  for (; o < r.length && o < l.length && r.charCodeAt(o) == l.charCodeAt(o); )
    o++;
  i.push({ from: t.from + o, to: t.from + r.length, insert: l.slice(o) });
}), { userEvent: "delete.dedent" })), !0), Ta = (n) => (n.setTabFocusMode(), !0), Ia = [
  { key: "Ctrl-b", run: Wr, shift: Kr, preventDefault: !0 },
  { key: "Ctrl-f", run: qr, shift: Gr },
  { key: "Ctrl-p", run: Ur, shift: Zr },
  { key: "Ctrl-n", run: _r, shift: Yr },
  { key: "Ctrl-a", run: Jl, shift: la },
  { key: "Ctrl-e", run: Ql, shift: aa },
  { key: "Ctrl-d", run: ts },
  { key: "Ctrl-h", run: Bn },
  { key: "Ctrl-k", run: pa },
  { key: "Ctrl-Alt-h", run: is },
  { key: "Ctrl-o", run: ya },
  { key: "Ctrl-t", run: ba },
  { key: "Ctrl-v", run: Pn }
], Pa = /* @__PURE__ */ [
  { key: "ArrowLeft", run: Wr, shift: Kr, preventDefault: !0 },
  { key: "Mod-ArrowLeft", mac: "Alt-ArrowLeft", run: zl, shift: Xl, preventDefault: !0 },
  { mac: "Cmd-ArrowLeft", run: Kl, shift: sa, preventDefault: !0 },
  { key: "ArrowRight", run: qr, shift: Gr, preventDefault: !0 },
  { key: "Mod-ArrowRight", mac: "Alt-ArrowRight", run: $l, shift: ea, preventDefault: !0 },
  { mac: "Cmd-ArrowRight", run: Gl, shift: oa, preventDefault: !0 },
  { key: "ArrowUp", run: Ur, shift: Zr, preventDefault: !0 },
  { mac: "Cmd-ArrowUp", run: Bi, shift: Li },
  { mac: "Ctrl-ArrowUp", run: Ti, shift: Ii },
  { key: "ArrowDown", run: _r, shift: Yr, preventDefault: !0 },
  { mac: "Cmd-ArrowDown", run: Di, shift: Oi },
  { mac: "Ctrl-ArrowDown", run: Pn, shift: Pi },
  { key: "PageUp", run: Ti, shift: Ii },
  { key: "PageDown", run: Pn, shift: Pi },
  { key: "Home", run: Vl, shift: ra, preventDefault: !0 },
  { key: "Mod-Home", run: Bi, shift: Li },
  { key: "End", run: Hl, shift: ia, preventDefault: !0 },
  { key: "Mod-End", run: Di, shift: Oi },
  { key: "Enter", run: Ei, shift: Ei },
  { key: "Mod-a", run: ca },
  { key: "Backspace", run: Bn, shift: Bn },
  { key: "Delete", run: ts },
  { key: "Mod-Backspace", mac: "Alt-Backspace", run: is },
  { key: "Mod-Delete", mac: "Alt-Delete", run: da },
  { mac: "Mod-Backspace", run: ma },
  { mac: "Mod-Delete", run: ga }
].concat(/* @__PURE__ */ Ia.map((n) => ({ mac: n.key, run: n.run, shift: n.shift }))), cs = /* @__PURE__ */ [
  { key: "Alt-ArrowLeft", mac: "Ctrl-ArrowLeft", run: _l, shift: ta },
  { key: "Alt-ArrowRight", mac: "Ctrl-ArrowRight", run: jl, shift: na },
  { key: "Alt-ArrowUp", run: xa },
  { key: "Shift-Alt-ArrowUp", run: wa },
  { key: "Alt-ArrowDown", run: ka },
  { key: "Shift-Alt-ArrowDown", run: va },
  { key: "Escape", run: ua },
  { key: "Mod-Enter", run: Aa },
  { key: "Alt-l", mac: "Ctrl-l", run: fa },
  { key: "Mod-i", run: ha, preventDefault: !0 },
  { key: "Mod-[", run: as },
  { key: "Mod-]", run: ls },
  { key: "Mod-Alt-\\", run: Ma },
  { key: "Shift-Mod-k", run: Sa },
  { key: "Shift-Mod-\\", run: Yl },
  { key: "Mod-/", run: wl },
  { key: "Alt-A", run: Sl },
  { key: "Ctrl-m", mac: "Shift-Alt-m", run: Ta }
].concat(Pa), Ba = { key: "Tab", run: ls, shift: as };
function D() {
  var n = arguments[0];
  typeof n == "string" && (n = document.createElement(n));
  var e = 1, t = arguments[1];
  if (t && typeof t == "object" && t.nodeType == null && !Array.isArray(t)) {
    for (var i in t) if (Object.prototype.hasOwnProperty.call(t, i)) {
      var r = t[i];
      typeof r == "string" ? n.setAttribute(i, r) : r != null && (n[i] = r);
    }
    e++;
  }
  for (; e < arguments.length; e++) fs(n, arguments[e]);
  return n;
}
function fs(n, e) {
  if (typeof e == "string")
    n.appendChild(document.createTextNode(e));
  else if (e != null) if (e.nodeType != null)
    n.appendChild(e);
  else if (Array.isArray(e))
    for (var t = 0; t < e.length; t++) fs(n, e[t]);
  else
    throw new RangeError("Unsupported child node: " + e);
}
const Ni = typeof String.prototype.normalize == "function" ? (n) => n.normalize("NFKD") : (n) => n;
class Ke {
  /**
  Create a text cursor. The query is the search string, `from` to
  `to` provides the region to search.
  
  When `normalize` is given, it will be called, on both the query
  string and the content it is matched against, before comparing.
  You can, for example, create a case-insensitive search by
  passing `s => s.toLowerCase()`.
  
  Text is always normalized with
  [`.normalize("NFKD")`](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/normalize)
  (when supported).
  */
  constructor(e, t, i = 0, r = e.length, s, o) {
    this.test = o, this.value = { from: 0, to: 0 }, this.done = !1, this.matches = [], this.buffer = "", this.bufferPos = 0, this.iter = e.iterRange(i, r), this.bufferStart = i, this.normalize = s ? (l) => s(Ni(l)) : Ni, this.query = this.normalize(t);
  }
  peek() {
    if (this.bufferPos == this.buffer.length) {
      if (this.bufferStart += this.buffer.length, this.iter.next(), this.iter.done)
        return -1;
      this.bufferPos = 0, this.buffer = this.iter.value;
    }
    return ee(this.buffer, this.bufferPos);
  }
  /**
  Look for the next match. Updates the iterator's
  [`value`](https://codemirror.net/6/docs/ref/#search.SearchCursor.value) and
  [`done`](https://codemirror.net/6/docs/ref/#search.SearchCursor.done) properties. Should be called
  at least once before using the cursor.
  */
  next() {
    for (; this.matches.length; )
      this.matches.pop();
    return this.nextOverlapping();
  }
  /**
  The `next` method will ignore matches that partially overlap a
  previous match. This method behaves like `next`, but includes
  such matches.
  */
  nextOverlapping() {
    for (; ; ) {
      let e = this.peek();
      if (e < 0)
        return this.done = !0, this;
      let t = Rn(e), i = this.bufferStart + this.bufferPos;
      this.bufferPos += be(e);
      let r = this.normalize(t);
      if (r.length)
        for (let s = 0, o = i; ; s++) {
          let l = r.charCodeAt(s), a = this.match(l, o, this.bufferPos + this.bufferStart);
          if (s == r.length - 1) {
            if (a)
              return this.value = a, this;
            break;
          }
          o == i && s < t.length && t.charCodeAt(s) == l && o++;
        }
    }
  }
  match(e, t, i) {
    let r = null;
    for (let s = 0; s < this.matches.length; s += 2) {
      let o = this.matches[s], l = !1;
      this.query.charCodeAt(o) == e && (o == this.query.length - 1 ? r = { from: this.matches[s + 1], to: i } : (this.matches[s]++, l = !0)), l || (this.matches.splice(s, 2), s -= 2);
    }
    return this.query.charCodeAt(0) == e && (this.query.length == 1 ? r = { from: t, to: i } : this.matches.push(1, t)), r && this.test && !this.test(r.from, r.to, this.buffer, this.bufferStart) && (r = null), r;
  }
}
typeof Symbol < "u" && (Ke.prototype[Symbol.iterator] = function() {
  return this;
});
const hs = { from: -1, to: -1, match: /* @__PURE__ */ /.*/.exec("") }, Gn = "gm" + (/x/.unicode == null ? "" : "u");
class us {
  /**
  Create a cursor that will search the given range in the given
  document. `query` should be the raw pattern (as you'd pass it to
  `new RegExp`).
  */
  constructor(e, t, i, r = 0, s = e.length) {
    if (this.text = e, this.to = s, this.curLine = "", this.done = !1, this.value = hs, /\\[sWDnr]|\n|\r|\[\^/.test(t))
      return new ds(e, t, i, r, s);
    this.re = new RegExp(t, Gn + (i != null && i.ignoreCase ? "i" : "")), this.test = i == null ? void 0 : i.test, this.iter = e.iter();
    let o = e.lineAt(r);
    this.curLineStart = o.from, this.matchPos = $t(e, r), this.getLine(this.curLineStart);
  }
  getLine(e) {
    this.iter.next(e), this.iter.lineBreak ? this.curLine = "" : (this.curLine = this.iter.value, this.curLineStart + this.curLine.length > this.to && (this.curLine = this.curLine.slice(0, this.to - this.curLineStart)), this.iter.next());
  }
  nextLine() {
    this.curLineStart = this.curLineStart + this.curLine.length + 1, this.curLineStart > this.to ? this.curLine = "" : this.getLine(0);
  }
  /**
  Move to the next match, if there is one.
  */
  next() {
    for (let e = this.matchPos - this.curLineStart; ; ) {
      this.re.lastIndex = e;
      let t = this.matchPos <= this.to && this.re.exec(this.curLine);
      if (t) {
        let i = this.curLineStart + t.index, r = i + t[0].length;
        if (this.matchPos = $t(this.text, r + (i == r ? 1 : 0)), i == this.curLineStart + this.curLine.length && this.nextLine(), (i < r || i > this.value.to) && (!this.test || this.test(i, r, t)))
          return this.value = { from: i, to: r, match: t }, this;
        e = this.matchPos - this.curLineStart;
      } else if (this.curLineStart + this.curLine.length < this.to)
        this.nextLine(), e = 0;
      else
        return this.done = !0, this;
    }
  }
}
const mn = /* @__PURE__ */ new WeakMap();
class _e {
  constructor(e, t) {
    this.from = e, this.text = t;
  }
  get to() {
    return this.from + this.text.length;
  }
  static get(e, t, i) {
    let r = mn.get(e);
    if (!r || r.from >= i || r.to <= t) {
      let l = new _e(t, e.sliceString(t, i));
      return mn.set(e, l), l;
    }
    if (r.from == t && r.to == i)
      return r;
    let { text: s, from: o } = r;
    return o > t && (s = e.sliceString(t, o) + s, o = t), r.to < i && (s += e.sliceString(r.to, i)), mn.set(e, new _e(o, s)), new _e(t, s.slice(t - o, i - o));
  }
}
class ds {
  constructor(e, t, i, r, s) {
    this.text = e, this.to = s, this.done = !1, this.value = hs, this.matchPos = $t(e, r), this.re = new RegExp(t, Gn + (i != null && i.ignoreCase ? "i" : "")), this.test = i == null ? void 0 : i.test, this.flat = _e.get(e, r, this.chunkEnd(
      r + 5e3
      /* Chunk.Base */
    ));
  }
  chunkEnd(e) {
    return e >= this.to ? this.to : this.text.lineAt(e).to;
  }
  next() {
    for (; ; ) {
      let e = this.re.lastIndex = this.matchPos - this.flat.from, t = this.re.exec(this.flat.text);
      if (t && !t[0] && t.index == e && (this.re.lastIndex = e + 1, t = this.re.exec(this.flat.text)), t) {
        let i = this.flat.from + t.index, r = i + t[0].length;
        if ((this.flat.to >= this.to || t.index + t[0].length <= this.flat.text.length - 10) && (!this.test || this.test(i, r, t)))
          return this.value = { from: i, to: r, match: t }, this.matchPos = $t(this.text, r + (i == r ? 1 : 0)), this;
      }
      if (this.flat.to == this.to)
        return this.done = !0, this;
      this.flat = _e.get(this.text, this.flat.from, this.chunkEnd(this.flat.from + this.flat.text.length * 2));
    }
  }
}
typeof Symbol < "u" && (us.prototype[Symbol.iterator] = ds.prototype[Symbol.iterator] = function() {
  return this;
});
function Da(n) {
  try {
    return new RegExp(n, Gn), !0;
  } catch {
    return !1;
  }
}
function $t(n, e) {
  if (e >= n.length)
    return e;
  let t = n.lineAt(e), i;
  for (; e < t.to && (i = t.text.charCodeAt(e - t.from)) >= 56320 && i < 57344; )
    e++;
  return e;
}
function Dn(n) {
  let e = String(n.state.doc.lineAt(n.state.selection.main.head).number), t = D("input", { class: "cm-textfield", name: "line", value: e }), i = D("form", {
    class: "cm-gotoLine",
    onkeydown: (s) => {
      s.keyCode == 27 ? (s.preventDefault(), n.dispatch({ effects: it.of(!1) }), n.focus()) : s.keyCode == 13 && (s.preventDefault(), r());
    },
    onsubmit: (s) => {
      s.preventDefault(), r();
    }
  }, D("label", n.state.phrase("Go to line"), ": ", t), " ", D("button", { class: "cm-button", type: "submit" }, n.state.phrase("go")), D("button", {
    name: "close",
    onclick: () => {
      n.dispatch({ effects: it.of(!1) }), n.focus();
    },
    "aria-label": n.state.phrase("close"),
    type: "button"
  }, ["×"]));
  function r() {
    let s = /^([+-])?(\d+)?(:\d+)?(%)?$/.exec(t.value);
    if (!s)
      return;
    let { state: o } = n, l = o.doc.lineAt(o.selection.main.head), [, a, c, f, h] = s, u = f ? +f.slice(1) : 0, d = c ? +c : l.number;
    if (c && h) {
      let C = d / 100;
      a && (C = C * (a == "-" ? -1 : 1) + l.number / o.doc.lines), d = Math.round(o.doc.lines * C);
    } else c && a && (d = d * (a == "-" ? -1 : 1) + l.number);
    let y = o.doc.line(Math.max(1, Math.min(o.doc.lines, d))), g = x.cursor(y.from + Math.max(0, Math.min(u, y.length)));
    n.dispatch({
      effects: [it.of(!1), T.scrollIntoView(g.from, { y: "center" })],
      selection: g
    }), n.focus();
  }
  return { dom: i };
}
const it = /* @__PURE__ */ F.define(), Ri = /* @__PURE__ */ me.define({
  create() {
    return !0;
  },
  update(n, e) {
    for (let t of e.effects)
      t.is(it) && (n = t.value);
    return n;
  },
  provide: (n) => Nn.from(n, (e) => e ? Dn : null)
}), La = (n) => {
  let e = rt(n, Dn);
  if (!e) {
    let t = [it.of(!0)];
    n.state.field(Ri, !1) == null && t.push(F.appendConfig.of([Ri, Oa])), n.dispatch({ effects: t }), e = rt(n, Dn);
  }
  return e && e.dom.querySelector("input").select(), !0;
}, Oa = /* @__PURE__ */ T.baseTheme({
  ".cm-panel.cm-gotoLine": {
    padding: "2px 6px 4px",
    position: "relative",
    "& label": { fontSize: "80%" },
    "& [name=close]": {
      position: "absolute",
      top: "0",
      bottom: "0",
      right: "4px",
      backgroundColor: "inherit",
      border: "none",
      font: "inherit",
      padding: "0"
    }
  }
}), Ea = {
  highlightWordAroundCursor: !1,
  minSelectionLength: 1,
  maxMatches: 100,
  wholeWords: !1
}, Na = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, Ea, {
      highlightWordAroundCursor: (e, t) => e || t,
      minSelectionLength: Math.min,
      maxMatches: Math.min
    });
  }
});
function Ra(n) {
  return [$a, za];
}
const Fa = /* @__PURE__ */ A.mark({ class: "cm-selectionMatch" }), Wa = /* @__PURE__ */ A.mark({ class: "cm-selectionMatch cm-selectionMatch-main" });
function Fi(n, e, t, i) {
  return (t == 0 || n(e.sliceDoc(t - 1, t)) != K.Word) && (i == e.doc.length || n(e.sliceDoc(i, i + 1)) != K.Word);
}
function qa(n, e, t, i) {
  return n(e.sliceDoc(t, t + 1)) == K.Word && n(e.sliceDoc(i - 1, i)) == K.Word;
}
const za = /* @__PURE__ */ Je.fromClass(class {
  constructor(n) {
    this.decorations = this.getDeco(n);
  }
  update(n) {
    (n.selectionSet || n.docChanged || n.viewportChanged) && (this.decorations = this.getDeco(n.view));
  }
  getDeco(n) {
    let e = n.state.facet(Na), { state: t } = n, i = t.selection;
    if (i.ranges.length > 1)
      return A.none;
    let r = i.main, s, o = null;
    if (r.empty) {
      if (!e.highlightWordAroundCursor)
        return A.none;
      let a = t.wordAt(r.head);
      if (!a)
        return A.none;
      o = t.charCategorizer(r.head), s = t.sliceDoc(a.from, a.to);
    } else {
      let a = r.to - r.from;
      if (a < e.minSelectionLength || a > 200)
        return A.none;
      if (e.wholeWords) {
        if (s = t.sliceDoc(r.from, r.to), o = t.charCategorizer(r.head), !(Fi(o, t, r.from, r.to) && qa(o, t, r.from, r.to)))
          return A.none;
      } else if (s = t.sliceDoc(r.from, r.to), !s)
        return A.none;
    }
    let l = [];
    for (let a of n.visibleRanges) {
      let c = new Ke(t.doc, s, a.from, a.to);
      for (; !c.next().done; ) {
        let { from: f, to: h } = c.value;
        if ((!o || Fi(o, t, f, h)) && (r.empty && f <= r.from && h >= r.to ? l.push(Wa.range(f, h)) : (f >= r.to || h <= r.from) && l.push(Fa.range(f, h)), l.length > e.maxMatches))
          return A.none;
      }
    }
    return A.set(l);
  }
}, {
  decorations: (n) => n.decorations
}), $a = /* @__PURE__ */ T.baseTheme({
  ".cm-selectionMatch": { backgroundColor: "#99ff7780" },
  ".cm-searchMatch .cm-selectionMatch": { backgroundColor: "transparent" }
}), Ua = ({ state: n, dispatch: e }) => {
  let { selection: t } = n, i = x.create(t.ranges.map((r) => n.wordAt(r.head) || x.cursor(r.head)), t.mainIndex);
  return i.eq(t) ? !1 : (e(n.update({ selection: i })), !0);
};
function _a(n, e) {
  let { main: t, ranges: i } = n.selection, r = n.wordAt(t.head), s = r && r.from == t.from && r.to == t.to;
  for (let o = !1, l = new Ke(n.doc, e, i[i.length - 1].to); ; )
    if (l.next(), l.done) {
      if (o)
        return null;
      l = new Ke(n.doc, e, 0, Math.max(0, i[i.length - 1].from - 1)), o = !0;
    } else {
      if (o && i.some((a) => a.from == l.value.from))
        continue;
      if (s) {
        let a = n.wordAt(l.value.from);
        if (!a || a.from != l.value.from || a.to != l.value.to)
          continue;
      }
      return l.value;
    }
}
const ja = ({ state: n, dispatch: e }) => {
  let { ranges: t } = n.selection;
  if (t.some((s) => s.from === s.to))
    return Ua({ state: n, dispatch: e });
  let i = n.sliceDoc(t[0].from, t[0].to);
  if (n.selection.ranges.some((s) => n.sliceDoc(s.from, s.to) != i))
    return !1;
  let r = _a(n, i);
  return r ? (e(n.update({
    selection: n.selection.addRange(x.range(r.from, r.to), !1),
    effects: T.scrollIntoView(r.to)
  })), !0) : !1;
}, Ze = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, {
      top: !1,
      caseSensitive: !1,
      literal: !1,
      regexp: !1,
      wholeWord: !1,
      createPanel: (e) => new nc(e),
      scrollToMatch: (e) => T.scrollIntoView(e)
    });
  }
});
class ps {
  /**
  Create a query object.
  */
  constructor(e) {
    this.search = e.search, this.caseSensitive = !!e.caseSensitive, this.literal = !!e.literal, this.regexp = !!e.regexp, this.replace = e.replace || "", this.valid = !!this.search && (!this.regexp || Da(this.search)), this.unquoted = this.unquote(this.search), this.wholeWord = !!e.wholeWord;
  }
  /**
  @internal
  */
  unquote(e) {
    return this.literal ? e : e.replace(/\\([nrt\\])/g, (t, i) => i == "n" ? `
` : i == "r" ? "\r" : i == "t" ? "	" : "\\");
  }
  /**
  Compare this query to another query.
  */
  eq(e) {
    return this.search == e.search && this.replace == e.replace && this.caseSensitive == e.caseSensitive && this.regexp == e.regexp && this.wholeWord == e.wholeWord;
  }
  /**
  @internal
  */
  create() {
    return this.regexp ? new Ga(this) : new Va(this);
  }
  /**
  Get a search cursor for this query, searching through the given
  range in the given state.
  */
  getCursor(e, t = 0, i) {
    let r = e.doc ? e : Oe.create({ doc: e });
    return i == null && (i = r.doc.length), this.regexp ? $e(this, r, t, i) : ze(this, r, t, i);
  }
}
class ms {
  constructor(e) {
    this.spec = e;
  }
}
function ze(n, e, t, i) {
  return new Ke(e.doc, n.unquoted, t, i, n.caseSensitive ? void 0 : (r) => r.toLowerCase(), n.wholeWord ? Ha(e.doc, e.charCategorizer(e.selection.main.head)) : void 0);
}
function Ha(n, e) {
  return (t, i, r, s) => ((s > t || s + r.length < i) && (s = Math.max(0, t - 2), r = n.sliceString(s, Math.min(n.length, i + 2))), (e(Ut(r, t - s)) != K.Word || e(_t(r, t - s)) != K.Word) && (e(_t(r, i - s)) != K.Word || e(Ut(r, i - s)) != K.Word));
}
class Va extends ms {
  constructor(e) {
    super(e);
  }
  nextMatch(e, t, i) {
    let r = ze(this.spec, e, i, e.doc.length).nextOverlapping();
    if (r.done) {
      let s = Math.min(e.doc.length, t + this.spec.unquoted.length);
      r = ze(this.spec, e, 0, s).nextOverlapping();
    }
    return r.done || r.value.from == t && r.value.to == i ? null : r.value;
  }
  // Searching in reverse is, rather than implementing an inverted search
  // cursor, done by scanning chunk after chunk forward.
  prevMatchInRange(e, t, i) {
    for (let r = i; ; ) {
      let s = Math.max(t, r - 1e4 - this.spec.unquoted.length), o = ze(this.spec, e, s, r), l = null;
      for (; !o.nextOverlapping().done; )
        l = o.value;
      if (l)
        return l;
      if (s == t)
        return null;
      r -= 1e4;
    }
  }
  prevMatch(e, t, i) {
    let r = this.prevMatchInRange(e, 0, t);
    return r || (r = this.prevMatchInRange(e, Math.max(0, i - this.spec.unquoted.length), e.doc.length)), r && (r.from != t || r.to != i) ? r : null;
  }
  getReplacement(e) {
    return this.spec.unquote(this.spec.replace);
  }
  matchAll(e, t) {
    let i = ze(this.spec, e, 0, e.doc.length), r = [];
    for (; !i.next().done; ) {
      if (r.length >= t)
        return null;
      r.push(i.value);
    }
    return r;
  }
  highlight(e, t, i, r) {
    let s = ze(this.spec, e, Math.max(0, t - this.spec.unquoted.length), Math.min(i + this.spec.unquoted.length, e.doc.length));
    for (; !s.next().done; )
      r(s.value.from, s.value.to);
  }
}
function $e(n, e, t, i) {
  return new us(e.doc, n.search, {
    ignoreCase: !n.caseSensitive,
    test: n.wholeWord ? Ka(e.charCategorizer(e.selection.main.head)) : void 0
  }, t, i);
}
function Ut(n, e) {
  return n.slice(Ne(n, e, !1), e);
}
function _t(n, e) {
  return n.slice(e, Ne(n, e));
}
function Ka(n) {
  return (e, t, i) => !i[0].length || (n(Ut(i.input, i.index)) != K.Word || n(_t(i.input, i.index)) != K.Word) && (n(_t(i.input, i.index + i[0].length)) != K.Word || n(Ut(i.input, i.index + i[0].length)) != K.Word);
}
class Ga extends ms {
  nextMatch(e, t, i) {
    let r = $e(this.spec, e, i, e.doc.length).next();
    return r.done && (r = $e(this.spec, e, 0, t).next()), r.done ? null : r.value;
  }
  prevMatchInRange(e, t, i) {
    for (let r = 1; ; r++) {
      let s = Math.max(
        t,
        i - r * 1e4
        /* FindPrev.ChunkSize */
      ), o = $e(this.spec, e, s, i), l = null;
      for (; !o.next().done; )
        l = o.value;
      if (l && (s == t || l.from > s + 10))
        return l;
      if (s == t)
        return null;
    }
  }
  prevMatch(e, t, i) {
    return this.prevMatchInRange(e, 0, t) || this.prevMatchInRange(e, i, e.doc.length);
  }
  getReplacement(e) {
    return this.spec.unquote(this.spec.replace).replace(/\$([$&]|\d+)/g, (t, i) => {
      if (i == "&")
        return e.match[0];
      if (i == "$")
        return "$";
      for (let r = i.length; r > 0; r--) {
        let s = +i.slice(0, r);
        if (s > 0 && s < e.match.length)
          return e.match[s] + i.slice(r);
      }
      return t;
    });
  }
  matchAll(e, t) {
    let i = $e(this.spec, e, 0, e.doc.length), r = [];
    for (; !i.next().done; ) {
      if (r.length >= t)
        return null;
      r.push(i.value);
    }
    return r;
  }
  highlight(e, t, i, r) {
    let s = $e(this.spec, e, Math.max(
      0,
      t - 250
      /* RegExp.HighlightMargin */
    ), Math.min(i + 250, e.doc.length));
    for (; !s.next().done; )
      r(s.value.from, s.value.to);
  }
}
const ct = /* @__PURE__ */ F.define(), Jn = /* @__PURE__ */ F.define(), Se = /* @__PURE__ */ me.define({
  create(n) {
    return new gn(Ln(n).create(), null);
  },
  update(n, e) {
    for (let t of e.effects)
      t.is(ct) ? n = new gn(t.value.create(), n.panel) : t.is(Jn) && (n = new gn(n.query, t.value ? Qn : null));
    return n;
  },
  provide: (n) => Nn.from(n, (e) => e.panel)
});
class gn {
  constructor(e, t) {
    this.query = e, this.panel = t;
  }
}
const Ja = /* @__PURE__ */ A.mark({ class: "cm-searchMatch" }), Qa = /* @__PURE__ */ A.mark({ class: "cm-searchMatch cm-searchMatch-selected" }), Za = /* @__PURE__ */ Je.fromClass(class {
  constructor(n) {
    this.view = n, this.decorations = this.highlight(n.state.field(Se));
  }
  update(n) {
    let e = n.state.field(Se);
    (e != n.startState.field(Se) || n.docChanged || n.selectionSet || n.viewportChanged) && (this.decorations = this.highlight(e));
  }
  highlight({ query: n, panel: e }) {
    if (!e || !n.spec.valid)
      return A.none;
    let { view: t } = this, i = new Gt();
    for (let r = 0, s = t.visibleRanges, o = s.length; r < o; r++) {
      let { from: l, to: a } = s[r];
      for (; r < o - 1 && a > s[r + 1].from - 2 * 250; )
        a = s[++r].to;
      n.highlight(t.state, l, a, (c, f) => {
        let h = t.state.selection.ranges.some((u) => u.from == c && u.to == f);
        i.add(c, f, h ? Qa : Ja);
      });
    }
    return i.finish();
  }
}, {
  decorations: (n) => n.decorations
});
function bt(n) {
  return (e) => {
    let t = e.state.field(Se, !1);
    return t && t.query.spec.valid ? n(e, t) : bs(e);
  };
}
const jt = /* @__PURE__ */ bt((n, { query: e }) => {
  let { to: t } = n.state.selection.main, i = e.nextMatch(n.state, t, t);
  if (!i)
    return !1;
  let r = x.single(i.from, i.to), s = n.state.facet(Ze);
  return n.dispatch({
    selection: r,
    effects: [Zn(n, i), s.scrollToMatch(r.main, n)],
    userEvent: "select.search"
  }), ys(n), !0;
}), Ht = /* @__PURE__ */ bt((n, { query: e }) => {
  let { state: t } = n, { from: i } = t.selection.main, r = e.prevMatch(t, i, i);
  if (!r)
    return !1;
  let s = x.single(r.from, r.to), o = n.state.facet(Ze);
  return n.dispatch({
    selection: s,
    effects: [Zn(n, r), o.scrollToMatch(s.main, n)],
    userEvent: "select.search"
  }), ys(n), !0;
}), Ya = /* @__PURE__ */ bt((n, { query: e }) => {
  let t = e.matchAll(n.state, 1e3);
  return !t || !t.length ? !1 : (n.dispatch({
    selection: x.create(t.map((i) => x.range(i.from, i.to))),
    userEvent: "select.search.matches"
  }), !0);
}), Xa = ({ state: n, dispatch: e }) => {
  let t = n.selection;
  if (t.ranges.length > 1 || t.main.empty)
    return !1;
  let { from: i, to: r } = t.main, s = [], o = 0;
  for (let l = new Ke(n.doc, n.sliceDoc(i, r)); !l.next().done; ) {
    if (s.length > 1e3)
      return !1;
    l.value.from == i && (o = s.length), s.push(x.range(l.value.from, l.value.to));
  }
  return e(n.update({
    selection: x.create(s, o),
    userEvent: "select.search.matches"
  })), !0;
}, Wi = /* @__PURE__ */ bt((n, { query: e }) => {
  let { state: t } = n, { from: i, to: r } = t.selection.main;
  if (t.readOnly)
    return !1;
  let s = e.nextMatch(t, i, i);
  if (!s)
    return !1;
  let o = s, l = [], a, c, f = [];
  if (o.from == i && o.to == r && (c = t.toText(e.getReplacement(o)), l.push({ from: o.from, to: o.to, insert: c }), o = e.nextMatch(t, o.from, o.to), f.push(T.announce.of(t.phrase("replaced match on line $", t.doc.lineAt(i).number) + "."))), o) {
    let h = l.length == 0 || l[0].from >= s.to ? 0 : s.to - s.from - c.length;
    a = x.single(o.from - h, o.to - h), f.push(Zn(n, o)), f.push(t.facet(Ze).scrollToMatch(a.main, n));
  }
  return n.dispatch({
    changes: l,
    selection: a,
    effects: f,
    userEvent: "input.replace"
  }), !0;
}), ec = /* @__PURE__ */ bt((n, { query: e }) => {
  if (n.state.readOnly)
    return !1;
  let t = e.matchAll(n.state, 1e9).map((r) => {
    let { from: s, to: o } = r;
    return { from: s, to: o, insert: e.getReplacement(r) };
  });
  if (!t.length)
    return !1;
  let i = n.state.phrase("replaced $ matches", t.length) + ".";
  return n.dispatch({
    changes: t,
    effects: T.announce.of(i),
    userEvent: "input.replace.all"
  }), !0;
});
function Qn(n) {
  return n.state.facet(Ze).createPanel(n);
}
function Ln(n, e) {
  var t, i, r, s, o;
  let l = n.selection.main, a = l.empty || l.to > l.from + 100 ? "" : n.sliceDoc(l.from, l.to);
  if (e && !a)
    return e;
  let c = n.facet(Ze);
  return new ps({
    search: ((t = e == null ? void 0 : e.literal) !== null && t !== void 0 ? t : c.literal) ? a : a.replace(/\n/g, "\\n"),
    caseSensitive: (i = e == null ? void 0 : e.caseSensitive) !== null && i !== void 0 ? i : c.caseSensitive,
    literal: (r = e == null ? void 0 : e.literal) !== null && r !== void 0 ? r : c.literal,
    regexp: (s = e == null ? void 0 : e.regexp) !== null && s !== void 0 ? s : c.regexp,
    wholeWord: (o = e == null ? void 0 : e.wholeWord) !== null && o !== void 0 ? o : c.wholeWord
  });
}
function gs(n) {
  let e = rt(n, Qn);
  return e && e.dom.querySelector("[main-field]");
}
function ys(n) {
  let e = gs(n);
  e && e == n.root.activeElement && e.select();
}
const bs = (n) => {
  let e = n.state.field(Se, !1);
  if (e && e.panel) {
    let t = gs(n);
    if (t && t != n.root.activeElement) {
      let i = Ln(n.state, e.query.spec);
      i.valid && n.dispatch({ effects: ct.of(i) }), t.focus(), t.select();
    }
  } else
    n.dispatch({ effects: [
      Jn.of(!0),
      e ? ct.of(Ln(n.state, e.query.spec)) : F.appendConfig.of(rc)
    ] });
  return !0;
}, xs = (n) => {
  let e = n.state.field(Se, !1);
  if (!e || !e.panel)
    return !1;
  let t = rt(n, Qn);
  return t && t.dom.contains(n.root.activeElement) && n.focus(), n.dispatch({ effects: Jn.of(!1) }), !0;
}, tc = [
  { key: "Mod-f", run: bs, scope: "editor search-panel" },
  { key: "F3", run: jt, shift: Ht, scope: "editor search-panel", preventDefault: !0 },
  { key: "Mod-g", run: jt, shift: Ht, scope: "editor search-panel", preventDefault: !0 },
  { key: "Escape", run: xs, scope: "editor search-panel" },
  { key: "Mod-Shift-l", run: Xa },
  { key: "Mod-Alt-g", run: La },
  { key: "Mod-d", run: ja, preventDefault: !0 }
];
class nc {
  constructor(e) {
    this.view = e;
    let t = this.query = e.state.field(Se).query.spec;
    this.commit = this.commit.bind(this), this.searchField = D("input", {
      value: t.search,
      placeholder: Z(e, "Find"),
      "aria-label": Z(e, "Find"),
      class: "cm-textfield",
      name: "search",
      form: "",
      "main-field": "true",
      onchange: this.commit,
      onkeyup: this.commit
    }), this.replaceField = D("input", {
      value: t.replace,
      placeholder: Z(e, "Replace"),
      "aria-label": Z(e, "Replace"),
      class: "cm-textfield",
      name: "replace",
      form: "",
      onchange: this.commit,
      onkeyup: this.commit
    }), this.caseField = D("input", {
      type: "checkbox",
      name: "case",
      form: "",
      checked: t.caseSensitive,
      onchange: this.commit
    }), this.reField = D("input", {
      type: "checkbox",
      name: "re",
      form: "",
      checked: t.regexp,
      onchange: this.commit
    }), this.wordField = D("input", {
      type: "checkbox",
      name: "word",
      form: "",
      checked: t.wholeWord,
      onchange: this.commit
    });
    function i(r, s, o) {
      return D("button", { class: "cm-button", name: r, onclick: s, type: "button" }, o);
    }
    this.dom = D("div", { onkeydown: (r) => this.keydown(r), class: "cm-search" }, [
      this.searchField,
      i("next", () => jt(e), [Z(e, "next")]),
      i("prev", () => Ht(e), [Z(e, "previous")]),
      i("select", () => Ya(e), [Z(e, "all")]),
      D("label", null, [this.caseField, Z(e, "match case")]),
      D("label", null, [this.reField, Z(e, "regexp")]),
      D("label", null, [this.wordField, Z(e, "by word")]),
      ...e.state.readOnly ? [] : [
        D("br"),
        this.replaceField,
        i("replace", () => Wi(e), [Z(e, "replace")]),
        i("replaceAll", () => ec(e), [Z(e, "replace all")])
      ],
      D("button", {
        name: "close",
        onclick: () => xs(e),
        "aria-label": Z(e, "close"),
        type: "button"
      }, ["×"])
    ]);
  }
  commit() {
    let e = new ps({
      search: this.searchField.value,
      caseSensitive: this.caseField.checked,
      regexp: this.reField.checked,
      wholeWord: this.wordField.checked,
      replace: this.replaceField.value
    });
    e.eq(this.query) || (this.query = e, this.view.dispatch({ effects: ct.of(e) }));
  }
  keydown(e) {
    qs(this.view, e, "search-panel") ? e.preventDefault() : e.keyCode == 13 && e.target == this.searchField ? (e.preventDefault(), (e.shiftKey ? Ht : jt)(this.view)) : e.keyCode == 13 && e.target == this.replaceField && (e.preventDefault(), Wi(this.view));
  }
  update(e) {
    for (let t of e.transactions)
      for (let i of t.effects)
        i.is(ct) && !i.value.eq(this.query) && this.setQuery(i.value);
  }
  setQuery(e) {
    this.query = e, this.searchField.value = e.search, this.replaceField.value = e.replace, this.caseField.checked = e.caseSensitive, this.reField.checked = e.regexp, this.wordField.checked = e.wholeWord;
  }
  mount() {
    this.searchField.select();
  }
  get pos() {
    return 80;
  }
  get top() {
    return this.view.state.facet(Ze).top;
  }
}
function Z(n, e) {
  return n.state.phrase(e);
}
const Tt = 30, It = /[\s\.,:;?!]/;
function Zn(n, { from: e, to: t }) {
  let i = n.state.doc.lineAt(e), r = n.state.doc.lineAt(t).to, s = Math.max(i.from, e - Tt), o = Math.min(r, t + Tt), l = n.state.sliceDoc(s, o);
  if (s != i.from) {
    for (let a = 0; a < Tt; a++)
      if (!It.test(l[a + 1]) && It.test(l[a])) {
        l = l.slice(a);
        break;
      }
  }
  if (o != r) {
    for (let a = l.length - 1; a > l.length - Tt; a--)
      if (!It.test(l[a - 1]) && It.test(l[a])) {
        l = l.slice(0, a);
        break;
      }
  }
  return T.announce.of(`${n.state.phrase("current match")}. ${l} ${n.state.phrase("on line")} ${i.number}.`);
}
const ic = /* @__PURE__ */ T.baseTheme({
  ".cm-panel.cm-search": {
    padding: "2px 6px 4px",
    position: "relative",
    "& [name=close]": {
      position: "absolute",
      top: "0",
      right: "4px",
      backgroundColor: "inherit",
      border: "none",
      font: "inherit",
      padding: 0,
      margin: 0
    },
    "& input, & button, & label": {
      margin: ".2em .6em .2em 0"
    },
    "& input[type=checkbox]": {
      marginRight: ".2em"
    },
    "& label": {
      fontSize: "80%",
      whiteSpace: "pre"
    }
  },
  "&light .cm-searchMatch": { backgroundColor: "#ffff0054" },
  "&dark .cm-searchMatch": { backgroundColor: "#00ffff8a" },
  "&light .cm-searchMatch-selected": { backgroundColor: "#ff6a0054" },
  "&dark .cm-searchMatch-selected": { backgroundColor: "#ff00ff8a" }
}), rc = [
  Se,
  /* @__PURE__ */ mt.low(Za),
  ic
];
class ks {
  /**
  Create a new completion context. (Mostly useful for testing
  completion sources—in the editor, the extension will create
  these for you.)
  */
  constructor(e, t, i, r) {
    this.state = e, this.pos = t, this.explicit = i, this.view = r, this.abortListeners = [], this.abortOnDocChange = !1;
  }
  /**
  Get the extent, content, and (if there is a token) type of the
  token before `this.pos`.
  */
  tokenBefore(e) {
    let t = _(this.state).resolveInner(this.pos, -1);
    for (; t && e.indexOf(t.name) < 0; )
      t = t.parent;
    return t ? {
      from: t.from,
      to: this.pos,
      text: this.state.sliceDoc(t.from, this.pos),
      type: t.type
    } : null;
  }
  /**
  Get the match of the given expression directly before the
  cursor.
  */
  matchBefore(e) {
    let t = this.state.doc.lineAt(this.pos), i = Math.max(t.from, this.pos - 250), r = t.text.slice(i - t.from, this.pos - t.from), s = r.search(ws(e, !1));
    return s < 0 ? null : { from: i + s, to: this.pos, text: r.slice(s) };
  }
  /**
  Yields true when the query has been aborted. Can be useful in
  asynchronous queries to avoid doing work that will be ignored.
  */
  get aborted() {
    return this.abortListeners == null;
  }
  /**
  Allows you to register abort handlers, which will be called when
  the query is
  [aborted](https://codemirror.net/6/docs/ref/#autocomplete.CompletionContext.aborted).
  
  By default, running queries will not be aborted for regular
  typing or backspacing, on the assumption that they are likely to
  return a result with a
  [`validFor`](https://codemirror.net/6/docs/ref/#autocomplete.CompletionResult.validFor) field that
  allows the result to be used after all. Passing `onDocChange:
  true` will cause this query to be aborted for any document
  change.
  */
  addEventListener(e, t, i) {
    e == "abort" && this.abortListeners && (this.abortListeners.push(t), i && i.onDocChange && (this.abortOnDocChange = !0));
  }
}
function qi(n) {
  let e = Object.keys(n).join(""), t = /\w/.test(e);
  return t && (e = e.replace(/\w/g, "")), `[${t ? "\\w" : ""}${e.replace(/[^\w\s]/g, "\\$&")}]`;
}
function sc(n) {
  let e = /* @__PURE__ */ Object.create(null), t = /* @__PURE__ */ Object.create(null);
  for (let { label: r } of n) {
    e[r[0]] = !0;
    for (let s = 1; s < r.length; s++)
      t[r[s]] = !0;
  }
  let i = qi(e) + qi(t) + "*$";
  return [new RegExp("^" + i), new RegExp(i)];
}
function oc(n) {
  let e = n.map((r) => typeof r == "string" ? { label: r } : r), [t, i] = e.every((r) => /^\w+$/.test(r.label)) ? [/\w*$/, /\w+$/] : sc(e);
  return (r) => {
    let s = r.matchBefore(i);
    return s || r.explicit ? { from: s ? s.from : r.pos, options: e, validFor: t } : null;
  };
}
function Pf(n, e) {
  return (t) => {
    for (let i = _(t.state).resolveInner(t.pos, -1); i; i = i.parent) {
      if (n.indexOf(i.name) > -1)
        return null;
      if (i.type.isTop)
        break;
    }
    return e(t);
  };
}
class zi {
  constructor(e, t, i, r) {
    this.completion = e, this.source = t, this.match = i, this.score = r;
  }
}
function Ee(n) {
  return n.selection.main.from;
}
function ws(n, e) {
  var t;
  let { source: i } = n, r = e && i[0] != "^", s = i[i.length - 1] != "$";
  return !r && !s ? n : new RegExp(`${r ? "^" : ""}(?:${i})${s ? "$" : ""}`, (t = n.flags) !== null && t !== void 0 ? t : n.ignoreCase ? "i" : "");
}
const Yn = /* @__PURE__ */ En.define();
function lc(n, e, t, i) {
  let { main: r } = n.selection, s = t - r.from, o = i - r.from;
  return Object.assign(Object.assign({}, n.changeByRange((l) => {
    if (l != r && t != i && n.sliceDoc(l.from + s, l.from + o) != n.sliceDoc(t, i))
      return { range: l };
    let a = n.toText(e);
    return {
      changes: { from: l.from + s, to: i == r.from ? l.to : l.from + o, insert: a },
      range: x.cursor(l.from + s + a.length)
    };
  })), { scrollIntoView: !0, userEvent: "input.complete" });
}
const $i = /* @__PURE__ */ new WeakMap();
function ac(n) {
  if (!Array.isArray(n))
    return n;
  let e = $i.get(n);
  return e || $i.set(n, e = oc(n)), e;
}
const Vt = /* @__PURE__ */ F.define(), ft = /* @__PURE__ */ F.define();
class cc {
  constructor(e) {
    this.pattern = e, this.chars = [], this.folded = [], this.any = [], this.precise = [], this.byWord = [], this.score = 0, this.matched = [];
    for (let t = 0; t < e.length; ) {
      let i = ee(e, t), r = be(i);
      this.chars.push(i);
      let s = e.slice(t, t + r), o = s.toUpperCase();
      this.folded.push(ee(o == s ? s.toLowerCase() : o, 0)), t += r;
    }
    this.astral = e.length != this.chars.length;
  }
  ret(e, t) {
    return this.score = e, this.matched = t, this;
  }
  // Matches a given word (completion) against the pattern (input).
  // Will return a boolean indicating whether there was a match and,
  // on success, set `this.score` to the score, `this.matched` to an
  // array of `from, to` pairs indicating the matched parts of `word`.
  //
  // The score is a number that is more negative the worse the match
  // is. See `Penalty` above.
  match(e) {
    if (this.pattern.length == 0)
      return this.ret(-100, []);
    if (e.length < this.pattern.length)
      return null;
    let { chars: t, folded: i, any: r, precise: s, byWord: o } = this;
    if (t.length == 1) {
      let v = ee(e, 0), I = be(v), W = I == e.length ? 0 : -100;
      if (v != t[0]) if (v == i[0])
        W += -200;
      else
        return null;
      return this.ret(W, [0, I]);
    }
    let l = e.indexOf(this.pattern);
    if (l == 0)
      return this.ret(e.length == this.pattern.length ? 0 : -100, [0, this.pattern.length]);
    let a = t.length, c = 0;
    if (l < 0) {
      for (let v = 0, I = Math.min(e.length, 200); v < I && c < a; ) {
        let W = ee(e, v);
        (W == t[c] || W == i[c]) && (r[c++] = v), v += be(W);
      }
      if (c < a)
        return null;
    }
    let f = 0, h = 0, u = !1, d = 0, y = -1, g = -1, C = /[a-z]/.test(e), P = !0;
    for (let v = 0, I = Math.min(e.length, 200), W = 0; v < I && h < a; ) {
      let k = ee(e, v);
      l < 0 && (f < a && k == t[f] && (s[f++] = v), d < a && (k == t[d] || k == i[d] ? (d == 0 && (y = v), g = v + 1, d++) : d = 0));
      let B, b = k < 255 ? k >= 48 && k <= 57 || k >= 97 && k <= 122 ? 2 : k >= 65 && k <= 90 ? 1 : 0 : (B = Rn(k)) != B.toLowerCase() ? 1 : B != B.toUpperCase() ? 2 : 0;
      (!v || b == 1 && C || W == 0 && b != 0) && (t[h] == k || i[h] == k && (u = !0) ? o[h++] = v : o.length && (P = !1)), W = b, v += be(k);
    }
    return h == a && o[0] == 0 && P ? this.result(-100 + (u ? -200 : 0), o, e) : d == a && y == 0 ? this.ret(-200 - e.length + (g == e.length ? 0 : -100), [0, g]) : l > -1 ? this.ret(-700 - e.length, [l, l + this.pattern.length]) : d == a ? this.ret(-900 - e.length, [y, g]) : h == a ? this.result(-100 + (u ? -200 : 0) + -700 + (P ? 0 : -1100), o, e) : t.length == 2 ? null : this.result((r[0] ? -700 : 0) + -200 + -1100, r, e);
  }
  result(e, t, i) {
    let r = [], s = 0;
    for (let o of t) {
      let l = o + (this.astral ? be(ee(i, o)) : 1);
      s && r[s - 1] == o ? r[s - 1] = l : (r[s++] = o, r[s++] = l);
    }
    return this.ret(e - i.length, r);
  }
}
class fc {
  constructor(e) {
    this.pattern = e, this.matched = [], this.score = 0, this.folded = e.toLowerCase();
  }
  match(e) {
    if (e.length < this.pattern.length)
      return null;
    let t = e.slice(0, this.pattern.length), i = t == this.pattern ? 0 : t.toLowerCase() == this.folded ? -200 : null;
    return i == null ? null : (this.matched = [0, t.length], this.score = i + (e.length == this.pattern.length ? 0 : -100), this);
  }
}
const $ = /* @__PURE__ */ H.define({
  combine(n) {
    return We(n, {
      activateOnTyping: !0,
      activateOnCompletion: () => !1,
      activateOnTypingDelay: 100,
      selectOnOpen: !0,
      override: null,
      closeOnBlur: !0,
      maxRenderedOptions: 100,
      defaultKeymap: !0,
      tooltipClass: () => "",
      optionClass: () => "",
      aboveCursor: !1,
      icons: !0,
      addToOptions: [],
      positionInfo: hc,
      filterStrict: !1,
      compareCompletions: (e, t) => e.label.localeCompare(t.label),
      interactionDelay: 75,
      updateSyncTime: 100
    }, {
      defaultKeymap: (e, t) => e && t,
      closeOnBlur: (e, t) => e && t,
      icons: (e, t) => e && t,
      tooltipClass: (e, t) => (i) => Ui(e(i), t(i)),
      optionClass: (e, t) => (i) => Ui(e(i), t(i)),
      addToOptions: (e, t) => e.concat(t),
      filterStrict: (e, t) => e || t
    });
  }
});
function Ui(n, e) {
  return n ? e ? n + " " + e : n : e;
}
function hc(n, e, t, i, r, s) {
  let o = n.textDirection == Et.RTL, l = o, a = !1, c = "top", f, h, u = e.left - r.left, d = r.right - e.right, y = i.right - i.left, g = i.bottom - i.top;
  if (l && u < Math.min(y, d) ? l = !1 : !l && d < Math.min(y, u) && (l = !0), y <= (l ? u : d))
    f = Math.max(r.top, Math.min(t.top, r.bottom - g)) - e.top, h = Math.min(400, l ? u : d);
  else {
    a = !0, h = Math.min(
      400,
      (o ? e.right : r.right - e.left) - 30
      /* Info.Margin */
    );
    let v = r.bottom - e.bottom;
    v >= g || v > e.top ? f = t.bottom - e.top : (c = "bottom", f = e.bottom - t.top);
  }
  let C = (e.bottom - e.top) / s.offsetHeight, P = (e.right - e.left) / s.offsetWidth;
  return {
    style: `${c}: ${f / C}px; max-width: ${h / P}px`,
    class: "cm-completionInfo-" + (a ? o ? "left-narrow" : "right-narrow" : l ? "left" : "right")
  };
}
function uc(n) {
  let e = n.addToOptions.slice();
  return n.icons && e.push({
    render(t) {
      let i = document.createElement("div");
      return i.classList.add("cm-completionIcon"), t.type && i.classList.add(...t.type.split(/\s+/g).map((r) => "cm-completionIcon-" + r)), i.setAttribute("aria-hidden", "true"), i;
    },
    position: 20
  }), e.push({
    render(t, i, r, s) {
      let o = document.createElement("span");
      o.className = "cm-completionLabel";
      let l = t.displayLabel || t.label, a = 0;
      for (let c = 0; c < s.length; ) {
        let f = s[c++], h = s[c++];
        f > a && o.appendChild(document.createTextNode(l.slice(a, f)));
        let u = o.appendChild(document.createElement("span"));
        u.appendChild(document.createTextNode(l.slice(f, h))), u.className = "cm-completionMatchedText", a = h;
      }
      return a < l.length && o.appendChild(document.createTextNode(l.slice(a))), o;
    },
    position: 50
  }, {
    render(t) {
      if (!t.detail)
        return null;
      let i = document.createElement("span");
      return i.className = "cm-completionDetail", i.textContent = t.detail, i;
    },
    position: 80
  }), e.sort((t, i) => t.position - i.position).map((t) => t.render);
}
function yn(n, e, t) {
  if (n <= t)
    return { from: 0, to: n };
  if (e < 0 && (e = 0), e <= n >> 1) {
    let r = Math.floor(e / t);
    return { from: r * t, to: (r + 1) * t };
  }
  let i = Math.floor((n - e) / t);
  return { from: n - (i + 1) * t, to: n - i * t };
}
class dc {
  constructor(e, t, i) {
    this.view = e, this.stateField = t, this.applyCompletion = i, this.info = null, this.infoDestroy = null, this.placeInfoReq = {
      read: () => this.measureInfo(),
      write: (a) => this.placeInfo(a),
      key: this
    }, this.space = null, this.currentClass = "";
    let r = e.state.field(t), { options: s, selected: o } = r.open, l = e.state.facet($);
    this.optionContent = uc(l), this.optionClass = l.optionClass, this.tooltipClass = l.tooltipClass, this.range = yn(s.length, o, l.maxRenderedOptions), this.dom = document.createElement("div"), this.dom.className = "cm-tooltip-autocomplete", this.updateTooltipClass(e.state), this.dom.addEventListener("mousedown", (a) => {
      let { options: c } = e.state.field(t).open;
      for (let f = a.target, h; f && f != this.dom; f = f.parentNode)
        if (f.nodeName == "LI" && (h = /-(\d+)$/.exec(f.id)) && +h[1] < c.length) {
          this.applyCompletion(e, c[+h[1]]), a.preventDefault();
          return;
        }
    }), this.dom.addEventListener("focusout", (a) => {
      let c = e.state.field(this.stateField, !1);
      c && c.tooltip && e.state.facet($).closeOnBlur && a.relatedTarget != e.contentDOM && e.dispatch({ effects: ft.of(null) });
    }), this.showOptions(s, r.id);
  }
  mount() {
    this.updateSel();
  }
  showOptions(e, t) {
    this.list && this.list.remove(), this.list = this.dom.appendChild(this.createListBox(e, t, this.range)), this.list.addEventListener("scroll", () => {
      this.info && this.view.requestMeasure(this.placeInfoReq);
    });
  }
  update(e) {
    var t;
    let i = e.state.field(this.stateField), r = e.startState.field(this.stateField);
    if (this.updateTooltipClass(e.state), i != r) {
      let { options: s, selected: o, disabled: l } = i.open;
      (!r.open || r.open.options != s) && (this.range = yn(s.length, o, e.state.facet($).maxRenderedOptions), this.showOptions(s, i.id)), this.updateSel(), l != ((t = r.open) === null || t === void 0 ? void 0 : t.disabled) && this.dom.classList.toggle("cm-tooltip-autocomplete-disabled", !!l);
    }
  }
  updateTooltipClass(e) {
    let t = this.tooltipClass(e);
    if (t != this.currentClass) {
      for (let i of this.currentClass.split(" "))
        i && this.dom.classList.remove(i);
      for (let i of t.split(" "))
        i && this.dom.classList.add(i);
      this.currentClass = t;
    }
  }
  positioned(e) {
    this.space = e, this.info && this.view.requestMeasure(this.placeInfoReq);
  }
  updateSel() {
    let e = this.view.state.field(this.stateField), t = e.open;
    if ((t.selected > -1 && t.selected < this.range.from || t.selected >= this.range.to) && (this.range = yn(t.options.length, t.selected, this.view.state.facet($).maxRenderedOptions), this.showOptions(t.options, e.id)), this.updateSelectedOption(t.selected)) {
      this.destroyInfo();
      let { completion: i } = t.options[t.selected], { info: r } = i;
      if (!r)
        return;
      let s = typeof r == "string" ? document.createTextNode(r) : r(i);
      if (!s)
        return;
      "then" in s ? s.then((o) => {
        o && this.view.state.field(this.stateField, !1) == e && this.addInfoPane(o, i);
      }).catch((o) => Ot(this.view.state, o, "completion info")) : this.addInfoPane(s, i);
    }
  }
  addInfoPane(e, t) {
    this.destroyInfo();
    let i = this.info = document.createElement("div");
    if (i.className = "cm-tooltip cm-completionInfo", e.nodeType != null)
      i.appendChild(e), this.infoDestroy = null;
    else {
      let { dom: r, destroy: s } = e;
      i.appendChild(r), this.infoDestroy = s || null;
    }
    this.dom.appendChild(i), this.view.requestMeasure(this.placeInfoReq);
  }
  updateSelectedOption(e) {
    let t = null;
    for (let i = this.list.firstChild, r = this.range.from; i; i = i.nextSibling, r++)
      i.nodeName != "LI" || !i.id ? r-- : r == e ? i.hasAttribute("aria-selected") || (i.setAttribute("aria-selected", "true"), t = i) : i.hasAttribute("aria-selected") && i.removeAttribute("aria-selected");
    return t && mc(this.list, t), t;
  }
  measureInfo() {
    let e = this.dom.querySelector("[aria-selected]");
    if (!e || !this.info)
      return null;
    let t = this.dom.getBoundingClientRect(), i = this.info.getBoundingClientRect(), r = e.getBoundingClientRect(), s = this.space;
    if (!s) {
      let o = this.dom.ownerDocument.documentElement;
      s = { left: 0, top: 0, right: o.clientWidth, bottom: o.clientHeight };
    }
    return r.top > Math.min(s.bottom, t.bottom) - 10 || r.bottom < Math.max(s.top, t.top) + 10 ? null : this.view.state.facet($).positionInfo(this.view, t, r, i, s, this.dom);
  }
  placeInfo(e) {
    this.info && (e ? (e.style && (this.info.style.cssText = e.style), this.info.className = "cm-tooltip cm-completionInfo " + (e.class || "")) : this.info.style.cssText = "top: -1e6px");
  }
  createListBox(e, t, i) {
    const r = document.createElement("ul");
    r.id = t, r.setAttribute("role", "listbox"), r.setAttribute("aria-expanded", "true"), r.setAttribute("aria-label", this.view.state.phrase("Completions")), r.addEventListener("mousedown", (o) => {
      o.target == r && o.preventDefault();
    });
    let s = null;
    for (let o = i.from; o < i.to; o++) {
      let { completion: l, match: a } = e[o], { section: c } = l;
      if (c) {
        let u = typeof c == "string" ? c : c.name;
        if (u != s && (o > i.from || i.from == 0))
          if (s = u, typeof c != "string" && c.header)
            r.appendChild(c.header(c));
          else {
            let d = r.appendChild(document.createElement("completion-section"));
            d.textContent = u;
          }
      }
      const f = r.appendChild(document.createElement("li"));
      f.id = t + "-" + o, f.setAttribute("role", "option");
      let h = this.optionClass(l);
      h && (f.className = h);
      for (let u of this.optionContent) {
        let d = u(l, this.view.state, this.view, a);
        d && f.appendChild(d);
      }
    }
    return i.from && r.classList.add("cm-completionListIncompleteTop"), i.to < e.length && r.classList.add("cm-completionListIncompleteBottom"), r;
  }
  destroyInfo() {
    this.info && (this.infoDestroy && this.infoDestroy(), this.info.remove(), this.info = null);
  }
  destroy() {
    this.destroyInfo();
  }
}
function pc(n, e) {
  return (t) => new dc(t, n, e);
}
function mc(n, e) {
  let t = n.getBoundingClientRect(), i = e.getBoundingClientRect(), r = t.height / n.offsetHeight;
  i.top < t.top ? n.scrollTop -= (t.top - i.top) / r : i.bottom > t.bottom && (n.scrollTop += (i.bottom - t.bottom) / r);
}
function _i(n) {
  return (n.boost || 0) * 100 + (n.apply ? 10 : 0) + (n.info ? 5 : 0) + (n.type ? 1 : 0);
}
function gc(n, e) {
  let t = [], i = null, r = (c) => {
    t.push(c);
    let { section: f } = c.completion;
    if (f) {
      i || (i = []);
      let h = typeof f == "string" ? f : f.name;
      i.some((u) => u.name == h) || i.push(typeof f == "string" ? { name: h } : f);
    }
  }, s = e.facet($);
  for (let c of n)
    if (c.hasResult()) {
      let f = c.result.getMatch;
      if (c.result.filter === !1)
        for (let h of c.result.options)
          r(new zi(h, c.source, f ? f(h) : [], 1e9 - t.length));
      else {
        let h = e.sliceDoc(c.from, c.to), u, d = s.filterStrict ? new fc(h) : new cc(h);
        for (let y of c.result.options)
          if (u = d.match(y.label)) {
            let g = y.displayLabel ? f ? f(y, u.matched) : [] : u.matched;
            r(new zi(y, c.source, g, u.score + (y.boost || 0)));
          }
      }
    }
  if (i) {
    let c = /* @__PURE__ */ Object.create(null), f = 0, h = (u, d) => {
      var y, g;
      return ((y = u.rank) !== null && y !== void 0 ? y : 1e9) - ((g = d.rank) !== null && g !== void 0 ? g : 1e9) || (u.name < d.name ? -1 : 1);
    };
    for (let u of i.sort(h))
      f -= 1e5, c[u.name] = f;
    for (let u of t) {
      let { section: d } = u.completion;
      d && (u.score += c[typeof d == "string" ? d : d.name]);
    }
  }
  let o = [], l = null, a = s.compareCompletions;
  for (let c of t.sort((f, h) => h.score - f.score || a(f.completion, h.completion))) {
    let f = c.completion;
    !l || l.label != f.label || l.detail != f.detail || l.type != null && f.type != null && l.type != f.type || l.apply != f.apply || l.boost != f.boost ? o.push(c) : _i(c.completion) > _i(l) && (o[o.length - 1] = c), l = c.completion;
  }
  return o;
}
class Ue {
  constructor(e, t, i, r, s, o) {
    this.options = e, this.attrs = t, this.tooltip = i, this.timestamp = r, this.selected = s, this.disabled = o;
  }
  setSelected(e, t) {
    return e == this.selected || e >= this.options.length ? this : new Ue(this.options, ji(t, e), this.tooltip, this.timestamp, e, this.disabled);
  }
  static build(e, t, i, r, s, o) {
    if (r && !o && e.some((c) => c.isPending))
      return r.setDisabled();
    let l = gc(e, t);
    if (!l.length)
      return r && e.some((c) => c.isPending) ? r.setDisabled() : null;
    let a = t.facet($).selectOnOpen ? 0 : -1;
    if (r && r.selected != a && r.selected != -1) {
      let c = r.options[r.selected].completion;
      for (let f = 0; f < l.length; f++)
        if (l[f].completion == c) {
          a = f;
          break;
        }
    }
    return new Ue(l, ji(i, a), {
      pos: e.reduce((c, f) => f.hasResult() ? Math.min(c, f.from) : c, 1e8),
      create: vc,
      above: s.aboveCursor
    }, r ? r.timestamp : Date.now(), a, !1);
  }
  map(e) {
    return new Ue(this.options, this.attrs, Object.assign(Object.assign({}, this.tooltip), { pos: e.mapPos(this.tooltip.pos) }), this.timestamp, this.selected, this.disabled);
  }
  setDisabled() {
    return new Ue(this.options, this.attrs, this.tooltip, this.timestamp, this.selected, !0);
  }
}
class Kt {
  constructor(e, t, i) {
    this.active = e, this.id = t, this.open = i;
  }
  static start() {
    return new Kt(kc, "cm-ac-" + Math.floor(Math.random() * 2e6).toString(36), null);
  }
  update(e) {
    let { state: t } = e, i = t.facet($), s = (i.override || t.languageDataAt("autocomplete", Ee(t)).map(ac)).map((a) => (this.active.find((f) => f.source == a) || new se(
      a,
      this.active.some(
        (f) => f.state != 0
        /* State.Inactive */
      ) ? 1 : 0
      /* State.Inactive */
    )).update(e, i));
    s.length == this.active.length && s.every((a, c) => a == this.active[c]) && (s = this.active);
    let o = this.open, l = e.effects.some((a) => a.is(Xn));
    o && e.docChanged && (o = o.map(e.changes)), e.selection || s.some((a) => a.hasResult() && e.changes.touchesRange(a.from, a.to)) || !yc(s, this.active) || l ? o = Ue.build(s, t, this.id, o, i, l) : o && o.disabled && !s.some((a) => a.isPending) && (o = null), !o && s.every((a) => !a.isPending) && s.some((a) => a.hasResult()) && (s = s.map((a) => a.hasResult() ? new se(
      a.source,
      0
      /* State.Inactive */
    ) : a));
    for (let a of e.effects)
      a.is(Ss) && (o = o && o.setSelected(a.value, this.id));
    return s == this.active && o == this.open ? this : new Kt(s, this.id, o);
  }
  get tooltip() {
    return this.open ? this.open.tooltip : null;
  }
  get attrs() {
    return this.open ? this.open.attrs : this.active.length ? bc : xc;
  }
}
function yc(n, e) {
  if (n == e)
    return !0;
  for (let t = 0, i = 0; ; ) {
    for (; t < n.length && !n[t].hasResult(); )
      t++;
    for (; i < e.length && !e[i].hasResult(); )
      i++;
    let r = t == n.length, s = i == e.length;
    if (r || s)
      return r == s;
    if (n[t++].result != e[i++].result)
      return !1;
  }
}
const bc = {
  "aria-autocomplete": "list"
}, xc = {};
function ji(n, e) {
  let t = {
    "aria-autocomplete": "list",
    "aria-haspopup": "listbox",
    "aria-controls": n
  };
  return e > -1 && (t["aria-activedescendant"] = n + "-" + e), t;
}
const kc = [];
function vs(n, e) {
  if (n.isUserEvent("input.complete")) {
    let i = n.annotation(Yn);
    if (i && e.activateOnCompletion(i))
      return 12;
  }
  let t = n.isUserEvent("input.type");
  return t && e.activateOnTyping ? 5 : t ? 1 : n.isUserEvent("delete.backward") ? 2 : n.selection ? 8 : n.docChanged ? 16 : 0;
}
class se {
  constructor(e, t, i = !1) {
    this.source = e, this.state = t, this.explicit = i;
  }
  hasResult() {
    return !1;
  }
  get isPending() {
    return this.state == 1;
  }
  update(e, t) {
    let i = vs(e, t), r = this;
    (i & 8 || i & 16 && this.touches(e)) && (r = new se(
      r.source,
      0
      /* State.Inactive */
    )), i & 4 && r.state == 0 && (r = new se(
      this.source,
      1
      /* State.Pending */
    )), r = r.updateFor(e, i);
    for (let s of e.effects)
      if (s.is(Vt))
        r = new se(r.source, 1, s.value);
      else if (s.is(ft))
        r = new se(
          r.source,
          0
          /* State.Inactive */
        );
      else if (s.is(Xn))
        for (let o of s.value)
          o.source == r.source && (r = o);
    return r;
  }
  updateFor(e, t) {
    return this.map(e.changes);
  }
  map(e) {
    return this;
  }
  touches(e) {
    return e.changes.touchesRange(Ee(e.state));
  }
}
class je extends se {
  constructor(e, t, i, r, s, o) {
    super(e, 3, t), this.limit = i, this.result = r, this.from = s, this.to = o;
  }
  hasResult() {
    return !0;
  }
  updateFor(e, t) {
    var i;
    if (!(t & 3))
      return this.map(e.changes);
    let r = this.result;
    r.map && !e.changes.empty && (r = r.map(r, e.changes));
    let s = e.changes.mapPos(this.from), o = e.changes.mapPos(this.to, 1), l = Ee(e.state);
    if (l > o || !r || t & 2 && (Ee(e.startState) == this.from || l < this.limit))
      return new se(
        this.source,
        t & 4 ? 1 : 0
        /* State.Inactive */
      );
    let a = e.changes.mapPos(this.limit);
    return wc(r.validFor, e.state, s, o) ? new je(this.source, this.explicit, a, r, s, o) : r.update && (r = r.update(r, s, o, new ks(e.state, l, !1))) ? new je(this.source, this.explicit, a, r, r.from, (i = r.to) !== null && i !== void 0 ? i : Ee(e.state)) : new se(this.source, 1, this.explicit);
  }
  map(e) {
    return e.empty ? this : (this.result.map ? this.result.map(this.result, e) : this.result) ? new je(this.source, this.explicit, e.mapPos(this.limit), this.result, e.mapPos(this.from), e.mapPos(this.to, 1)) : new se(
      this.source,
      0
      /* State.Inactive */
    );
  }
  touches(e) {
    return e.changes.touchesRange(this.from, this.to);
  }
}
function wc(n, e, t, i) {
  if (!n)
    return !1;
  let r = e.sliceDoc(t, i);
  return typeof n == "function" ? n(r, t, i, e) : ws(n, !0).test(r);
}
const Xn = /* @__PURE__ */ F.define({
  map(n, e) {
    return n.map((t) => t.map(e));
  }
}), Ss = /* @__PURE__ */ F.define(), J = /* @__PURE__ */ me.define({
  create() {
    return Kt.start();
  },
  update(n, e) {
    return n.update(e);
  },
  provide: (n) => [
    zs.from(n, (e) => e.tooltip),
    T.contentAttributes.from(n, (e) => e.attrs)
  ]
});
function ei(n, e) {
  const t = e.completion.apply || e.completion.label;
  let i = n.state.field(J).active.find((r) => r.source == e.source);
  return i instanceof je ? (typeof t == "string" ? n.dispatch(Object.assign(Object.assign({}, lc(n.state, t, i.from, i.to)), { annotations: Yn.of(e.completion) })) : t(n, e.completion, i.from, i.to), !0) : !1;
}
const vc = /* @__PURE__ */ pc(J, ei);
function Pt(n, e = "option") {
  return (t) => {
    let i = t.state.field(J, !1);
    if (!i || !i.open || i.open.disabled || Date.now() - i.open.timestamp < t.state.facet($).interactionDelay)
      return !1;
    let r = 1, s;
    e == "page" && (s = Xi(t, i.open.tooltip)) && (r = Math.max(2, Math.floor(s.dom.offsetHeight / s.dom.querySelector("li").offsetHeight) - 1));
    let { length: o } = i.open.options, l = i.open.selected > -1 ? i.open.selected + r * (n ? 1 : -1) : n ? 0 : o - 1;
    return l < 0 ? l = e == "page" ? 0 : o - 1 : l >= o && (l = e == "page" ? o - 1 : 0), t.dispatch({ effects: Ss.of(l) }), !0;
  };
}
const Sc = (n) => {
  let e = n.state.field(J, !1);
  return n.state.readOnly || !e || !e.open || e.open.selected < 0 || e.open.disabled || Date.now() - e.open.timestamp < n.state.facet($).interactionDelay ? !1 : ei(n, e.open.options[e.open.selected]);
}, Hi = (n) => n.state.field(J, !1) ? (n.dispatch({ effects: Vt.of(!0) }), !0) : !1, Cc = (n) => {
  let e = n.state.field(J, !1);
  return !e || !e.active.some(
    (t) => t.state != 0
    /* State.Inactive */
  ) ? !1 : (n.dispatch({ effects: ft.of(null) }), !0);
};
class Ac {
  constructor(e, t) {
    this.active = e, this.context = t, this.time = Date.now(), this.updates = [], this.done = void 0;
  }
}
const Mc = 50, Tc = 1e3, Ic = /* @__PURE__ */ Je.fromClass(class {
  constructor(n) {
    this.view = n, this.debounceUpdate = -1, this.running = [], this.debounceAccept = -1, this.pendingStart = !1, this.composing = 0;
    for (let e of n.state.field(J).active)
      e.isPending && this.startQuery(e);
  }
  update(n) {
    let e = n.state.field(J), t = n.state.facet($);
    if (!n.selectionSet && !n.docChanged && n.startState.field(J) == e)
      return;
    let i = n.transactions.some((s) => {
      let o = vs(s, t);
      return o & 8 || (s.selection || s.docChanged) && !(o & 3);
    });
    for (let s = 0; s < this.running.length; s++) {
      let o = this.running[s];
      if (i || o.context.abortOnDocChange && n.docChanged || o.updates.length + n.transactions.length > Mc && Date.now() - o.time > Tc) {
        for (let l of o.context.abortListeners)
          try {
            l();
          } catch (a) {
            Ot(this.view.state, a);
          }
        o.context.abortListeners = null, this.running.splice(s--, 1);
      } else
        o.updates.push(...n.transactions);
    }
    this.debounceUpdate > -1 && clearTimeout(this.debounceUpdate), n.transactions.some((s) => s.effects.some((o) => o.is(Vt))) && (this.pendingStart = !0);
    let r = this.pendingStart ? 50 : t.activateOnTypingDelay;
    if (this.debounceUpdate = e.active.some((s) => s.isPending && !this.running.some((o) => o.active.source == s.source)) ? setTimeout(() => this.startUpdate(), r) : -1, this.composing != 0)
      for (let s of n.transactions)
        s.isUserEvent("input.type") ? this.composing = 2 : this.composing == 2 && s.selection && (this.composing = 3);
  }
  startUpdate() {
    this.debounceUpdate = -1, this.pendingStart = !1;
    let { state: n } = this.view, e = n.field(J);
    for (let t of e.active)
      t.isPending && !this.running.some((i) => i.active.source == t.source) && this.startQuery(t);
    this.running.length && e.open && e.open.disabled && (this.debounceAccept = setTimeout(() => this.accept(), this.view.state.facet($).updateSyncTime));
  }
  startQuery(n) {
    let { state: e } = this.view, t = Ee(e), i = new ks(e, t, n.explicit, this.view), r = new Ac(n, i);
    this.running.push(r), Promise.resolve(n.source(i)).then((s) => {
      r.context.aborted || (r.done = s || null, this.scheduleAccept());
    }, (s) => {
      this.view.dispatch({ effects: ft.of(null) }), Ot(this.view.state, s);
    });
  }
  scheduleAccept() {
    this.running.every((n) => n.done !== void 0) ? this.accept() : this.debounceAccept < 0 && (this.debounceAccept = setTimeout(() => this.accept(), this.view.state.facet($).updateSyncTime));
  }
  // For each finished query in this.running, try to create a result
  // or, if appropriate, restart the query.
  accept() {
    var n;
    this.debounceAccept > -1 && clearTimeout(this.debounceAccept), this.debounceAccept = -1;
    let e = [], t = this.view.state.facet($), i = this.view.state.field(J);
    for (let r = 0; r < this.running.length; r++) {
      let s = this.running[r];
      if (s.done === void 0)
        continue;
      if (this.running.splice(r--, 1), s.done) {
        let l = Ee(s.updates.length ? s.updates[0].startState : this.view.state), a = Math.min(l, s.done.from + (s.active.explicit ? 0 : 1)), c = new je(s.active.source, s.active.explicit, a, s.done, s.done.from, (n = s.done.to) !== null && n !== void 0 ? n : l);
        for (let f of s.updates)
          c = c.update(f, t);
        if (c.hasResult()) {
          e.push(c);
          continue;
        }
      }
      let o = i.active.find((l) => l.source == s.active.source);
      if (o && o.isPending)
        if (s.done == null) {
          let l = new se(
            s.active.source,
            0
            /* State.Inactive */
          );
          for (let a of s.updates)
            l = l.update(a, t);
          l.isPending || e.push(l);
        } else
          this.startQuery(o);
    }
    (e.length || i.open && i.open.disabled) && this.view.dispatch({ effects: Xn.of(e) });
  }
}, {
  eventHandlers: {
    blur(n) {
      let e = this.view.state.field(J, !1);
      if (e && e.tooltip && this.view.state.facet($).closeOnBlur) {
        let t = e.open && Xi(this.view, e.open.tooltip);
        (!t || !t.dom.contains(n.relatedTarget)) && setTimeout(() => this.view.dispatch({ effects: ft.of(null) }), 10);
      }
    },
    compositionstart() {
      this.composing = 1;
    },
    compositionend() {
      this.composing == 3 && setTimeout(() => this.view.dispatch({ effects: Vt.of(!1) }), 20), this.composing = 0;
    }
  }
}), Pc = typeof navigator == "object" && /* @__PURE__ */ /Win/.test(navigator.platform), Bc = /* @__PURE__ */ mt.highest(/* @__PURE__ */ T.domEventHandlers({
  keydown(n, e) {
    let t = e.state.field(J, !1);
    if (!t || !t.open || t.open.disabled || t.open.selected < 0 || n.key.length > 1 || n.ctrlKey && !(Pc && n.altKey) || n.metaKey)
      return !1;
    let i = t.open.options[t.open.selected], r = t.active.find((o) => o.source == i.source), s = i.completion.commitCharacters || r.result.commitCharacters;
    return s && s.indexOf(n.key) > -1 && ei(e, i), !1;
  }
})), Cs = /* @__PURE__ */ T.baseTheme({
  ".cm-tooltip.cm-tooltip-autocomplete": {
    "& > ul": {
      fontFamily: "monospace",
      whiteSpace: "nowrap",
      overflow: "hidden auto",
      maxWidth_fallback: "700px",
      maxWidth: "min(700px, 95vw)",
      minWidth: "250px",
      maxHeight: "10em",
      height: "100%",
      listStyle: "none",
      margin: 0,
      padding: 0,
      "& > li, & > completion-section": {
        padding: "1px 3px",
        lineHeight: 1.2
      },
      "& > li": {
        overflowX: "hidden",
        textOverflow: "ellipsis",
        cursor: "pointer"
      },
      "& > completion-section": {
        display: "list-item",
        borderBottom: "1px solid silver",
        paddingLeft: "0.5em",
        opacity: 0.7
      }
    }
  },
  "&light .cm-tooltip-autocomplete ul li[aria-selected]": {
    background: "#17c",
    color: "white"
  },
  "&light .cm-tooltip-autocomplete-disabled ul li[aria-selected]": {
    background: "#777"
  },
  "&dark .cm-tooltip-autocomplete ul li[aria-selected]": {
    background: "#347",
    color: "white"
  },
  "&dark .cm-tooltip-autocomplete-disabled ul li[aria-selected]": {
    background: "#444"
  },
  ".cm-completionListIncompleteTop:before, .cm-completionListIncompleteBottom:after": {
    content: '"···"',
    opacity: 0.5,
    display: "block",
    textAlign: "center"
  },
  ".cm-tooltip.cm-completionInfo": {
    position: "absolute",
    padding: "3px 9px",
    width: "max-content",
    maxWidth: "400px",
    boxSizing: "border-box",
    whiteSpace: "pre-line"
  },
  ".cm-completionInfo.cm-completionInfo-left": { right: "100%" },
  ".cm-completionInfo.cm-completionInfo-right": { left: "100%" },
  ".cm-completionInfo.cm-completionInfo-left-narrow": { right: "30px" },
  ".cm-completionInfo.cm-completionInfo-right-narrow": { left: "30px" },
  "&light .cm-snippetField": { backgroundColor: "#00000022" },
  "&dark .cm-snippetField": { backgroundColor: "#ffffff22" },
  ".cm-snippetFieldPosition": {
    verticalAlign: "text-top",
    width: 0,
    height: "1.15em",
    display: "inline-block",
    margin: "0 -0.7px -.7em",
    borderLeft: "1.4px dotted #888"
  },
  ".cm-completionMatchedText": {
    textDecoration: "underline"
  },
  ".cm-completionDetail": {
    marginLeft: "0.5em",
    fontStyle: "italic"
  },
  ".cm-completionIcon": {
    fontSize: "90%",
    width: ".8em",
    display: "inline-block",
    textAlign: "center",
    paddingRight: ".6em",
    opacity: "0.6",
    boxSizing: "content-box"
  },
  ".cm-completionIcon-function, .cm-completionIcon-method": {
    "&:after": { content: "'ƒ'" }
  },
  ".cm-completionIcon-class": {
    "&:after": { content: "'○'" }
  },
  ".cm-completionIcon-interface": {
    "&:after": { content: "'◌'" }
  },
  ".cm-completionIcon-variable": {
    "&:after": { content: "'𝑥'" }
  },
  ".cm-completionIcon-constant": {
    "&:after": { content: "'𝐶'" }
  },
  ".cm-completionIcon-type": {
    "&:after": { content: "'𝑡'" }
  },
  ".cm-completionIcon-enum": {
    "&:after": { content: "'∪'" }
  },
  ".cm-completionIcon-property": {
    "&:after": { content: "'□'" }
  },
  ".cm-completionIcon-keyword": {
    "&:after": { content: "'🔑︎'" }
    // Disable emoji rendering
  },
  ".cm-completionIcon-namespace": {
    "&:after": { content: "'▢'" }
  },
  ".cm-completionIcon-text": {
    "&:after": { content: "'abc'", fontSize: "50%", verticalAlign: "middle" }
  }
});
class Dc {
  constructor(e, t, i, r) {
    this.field = e, this.line = t, this.from = i, this.to = r;
  }
}
class ti {
  constructor(e, t, i) {
    this.field = e, this.from = t, this.to = i;
  }
  map(e) {
    let t = e.mapPos(this.from, -1, xn.TrackDel), i = e.mapPos(this.to, 1, xn.TrackDel);
    return t == null || i == null ? null : new ti(this.field, t, i);
  }
}
class ni {
  constructor(e, t) {
    this.lines = e, this.fieldPositions = t;
  }
  instantiate(e, t) {
    let i = [], r = [t], s = e.doc.lineAt(t), o = /^\s*/.exec(s.text)[0];
    for (let a of this.lines) {
      if (i.length) {
        let c = o, f = /^\t*/.exec(a)[0].length;
        for (let h = 0; h < f; h++)
          c += e.facet(Yt);
        r.push(t + c.length - f), a = c + a.slice(f);
      }
      i.push(a), t += a.length + 1;
    }
    let l = this.fieldPositions.map((a) => new ti(a.field, r[a.line] + a.from, r[a.line] + a.to));
    return { text: i, ranges: l };
  }
  static parse(e) {
    let t = [], i = [], r = [], s;
    for (let o of e.split(/\r\n?|\n/)) {
      for (; s = /[#$]\{(?:(\d+)(?::([^}]*))?|((?:\\[{}]|[^}])*))\}/.exec(o); ) {
        let l = s[1] ? +s[1] : null, a = s[2] || s[3] || "", c = -1, f = a.replace(/\\[{}]/g, (h) => h[1]);
        for (let h = 0; h < t.length; h++)
          (l != null ? t[h].seq == l : f && t[h].name == f) && (c = h);
        if (c < 0) {
          let h = 0;
          for (; h < t.length && (l == null || t[h].seq != null && t[h].seq < l); )
            h++;
          t.splice(h, 0, { seq: l, name: f }), c = h;
          for (let u of r)
            u.field >= c && u.field++;
        }
        r.push(new Dc(c, i.length, s.index, s.index + f.length)), o = o.slice(0, s.index) + a + o.slice(s.index + s[0].length);
      }
      o = o.replace(/\\([{}])/g, (l, a, c) => {
        for (let f of r)
          f.line == i.length && f.from > c && (f.from--, f.to--);
        return a;
      }), i.push(o);
    }
    return new ni(i, r);
  }
}
let Lc = /* @__PURE__ */ A.widget({ widget: /* @__PURE__ */ new class extends Qt {
  toDOM() {
    let n = document.createElement("span");
    return n.className = "cm-snippetFieldPosition", n;
  }
  ignoreEvent() {
    return !1;
  }
}() }), Oc = /* @__PURE__ */ A.mark({ class: "cm-snippetField" });
class Ye {
  constructor(e, t) {
    this.ranges = e, this.active = t, this.deco = A.set(e.map((i) => (i.from == i.to ? Lc : Oc).range(i.from, i.to)));
  }
  map(e) {
    let t = [];
    for (let i of this.ranges) {
      let r = i.map(e);
      if (!r)
        return null;
      t.push(r);
    }
    return new Ye(t, this.active);
  }
  selectionInsideField(e) {
    return e.ranges.every((t) => this.ranges.some((i) => i.field == this.active && i.from <= t.from && i.to >= t.to));
  }
}
const xt = /* @__PURE__ */ F.define({
  map(n, e) {
    return n && n.map(e);
  }
}), Ec = /* @__PURE__ */ F.define(), ht = /* @__PURE__ */ me.define({
  create() {
    return null;
  },
  update(n, e) {
    for (let t of e.effects) {
      if (t.is(xt))
        return t.value;
      if (t.is(Ec) && n)
        return new Ye(n.ranges, t.value);
    }
    return n && e.docChanged && (n = n.map(e.changes)), n && e.selection && !n.selectionInsideField(e.selection) && (n = null), n;
  },
  provide: (n) => T.decorations.from(n, (e) => e ? e.deco : A.none)
});
function ii(n, e) {
  return x.create(n.filter((t) => t.field == e).map((t) => x.range(t.from, t.to)));
}
function Nc(n) {
  let e = ni.parse(n);
  return (t, i, r, s) => {
    let { text: o, ranges: l } = e.instantiate(t.state, r), { main: a } = t.state.selection, c = {
      changes: { from: r, to: s == a.from ? a.to : s, insert: On.of(o) },
      scrollIntoView: !0,
      annotations: i ? [Yn.of(i), Dt.userEvent.of("input.complete")] : void 0
    };
    if (l.length && (c.selection = ii(l, 0)), l.some((f) => f.field > 0)) {
      let f = new Ye(l, 0), h = c.effects = [xt.of(f)];
      t.state.field(ht, !1) === void 0 && h.push(F.appendConfig.of([ht, zc, $c, Cs]));
    }
    t.dispatch(t.state.update(c));
  };
}
function As(n) {
  return ({ state: e, dispatch: t }) => {
    let i = e.field(ht, !1);
    if (!i || n < 0 && i.active == 0)
      return !1;
    let r = i.active + n, s = n > 0 && !i.ranges.some((o) => o.field == r + n);
    return t(e.update({
      selection: ii(i.ranges, r),
      effects: xt.of(s ? null : new Ye(i.ranges, r)),
      scrollIntoView: !0
    })), !0;
  };
}
const Rc = ({ state: n, dispatch: e }) => n.field(ht, !1) ? (e(n.update({ effects: xt.of(null) })), !0) : !1, Fc = /* @__PURE__ */ As(1), Wc = /* @__PURE__ */ As(-1), qc = [
  { key: "Tab", run: Fc, shift: Wc },
  { key: "Escape", run: Rc }
], Vi = /* @__PURE__ */ H.define({
  combine(n) {
    return n.length ? n[0] : qc;
  }
}), zc = /* @__PURE__ */ mt.highest(/* @__PURE__ */ Zt.compute([Vi], (n) => n.facet(Vi)));
function Bf(n, e) {
  return Object.assign(Object.assign({}, e), { apply: Nc(n) });
}
const $c = /* @__PURE__ */ T.domEventHandlers({
  mousedown(n, e) {
    let t = e.state.field(ht, !1), i;
    if (!t || (i = e.posAtCoords({ x: n.clientX, y: n.clientY })) == null)
      return !1;
    let r = t.ranges.find((s) => s.from <= i && s.to >= i);
    return !r || r.field == t.active ? !1 : (e.dispatch({
      selection: ii(t.ranges, r.field),
      effects: xt.of(t.ranges.some((s) => s.field > r.field) ? new Ye(t.ranges, r.field) : null),
      scrollIntoView: !0
    }), !0);
  }
}), ut = {
  brackets: ["(", "[", "{", "'", '"'],
  before: ")]}:;>",
  stringPrefixes: []
}, Le = /* @__PURE__ */ F.define({
  map(n, e) {
    let t = e.mapPos(n, -1, xn.TrackAfter);
    return t ?? void 0;
  }
}), ri = /* @__PURE__ */ new class extends $s {
}();
ri.startSide = 1;
ri.endSide = -1;
const Ms = /* @__PURE__ */ me.define({
  create() {
    return Yi.empty;
  },
  update(n, e) {
    if (n = n.map(e.changes), e.selection) {
      let t = e.state.doc.lineAt(e.selection.main.head);
      n = n.update({ filter: (i) => i >= t.from && i <= t.to });
    }
    for (let t of e.effects)
      t.is(Le) && (n = n.update({ add: [ri.range(t.value, t.value + 1)] }));
    return n;
  }
});
function Uc() {
  return [jc, Ms];
}
const bn = "()[]{}<>«»»«［］｛｝";
function Ts(n) {
  for (let e = 0; e < bn.length; e += 2)
    if (bn.charCodeAt(e) == n)
      return bn.charAt(e + 1);
  return Rn(n < 128 ? n : n + 1);
}
function Is(n, e) {
  return n.languageDataAt("closeBrackets", e)[0] || ut;
}
const _c = typeof navigator == "object" && /* @__PURE__ */ /Android\b/.test(navigator.userAgent), jc = /* @__PURE__ */ T.inputHandler.of((n, e, t, i) => {
  if ((_c ? n.composing : n.compositionStarted) || n.state.readOnly)
    return !1;
  let r = n.state.selection.main;
  if (i.length > 2 || i.length == 2 && be(ee(i, 0)) == 1 || e != r.from || t != r.to)
    return !1;
  let s = Kc(n.state, i);
  return s ? (n.dispatch(s), !0) : !1;
}), Hc = ({ state: n, dispatch: e }) => {
  if (n.readOnly)
    return !1;
  let i = Is(n, n.selection.main.head).brackets || ut.brackets, r = null, s = n.changeByRange((o) => {
    if (o.empty) {
      let l = Gc(n.doc, o.head);
      for (let a of i)
        if (a == l && ln(n.doc, o.head) == Ts(ee(a, 0)))
          return {
            changes: { from: o.head - a.length, to: o.head + a.length },
            range: x.cursor(o.head - a.length)
          };
    }
    return { range: r = o };
  });
  return r || e(n.update(s, { scrollIntoView: !0, userEvent: "delete.backward" })), !r;
}, Vc = [
  { key: "Backspace", run: Hc }
];
function Kc(n, e) {
  let t = Is(n, n.selection.main.head), i = t.brackets || ut.brackets;
  for (let r of i) {
    let s = Ts(ee(r, 0));
    if (e == r)
      return s == r ? Zc(n, r, i.indexOf(r + r + r) > -1, t) : Jc(n, r, s, t.before || ut.before);
    if (e == s && Ps(n, n.selection.main.from))
      return Qc(n, r, s);
  }
  return null;
}
function Ps(n, e) {
  let t = !1;
  return n.field(Ms).between(0, n.doc.length, (i) => {
    i == e && (t = !0);
  }), t;
}
function ln(n, e) {
  let t = n.sliceString(e, e + 2);
  return t.slice(0, be(ee(t, 0)));
}
function Gc(n, e) {
  let t = n.sliceString(e - 2, e);
  return be(ee(t, 0)) == t.length ? t : t.slice(1);
}
function Jc(n, e, t, i) {
  let r = null, s = n.changeByRange((o) => {
    if (!o.empty)
      return {
        changes: [{ insert: e, from: o.from }, { insert: t, from: o.to }],
        effects: Le.of(o.to + e.length),
        range: x.range(o.anchor + e.length, o.head + e.length)
      };
    let l = ln(n.doc, o.head);
    return !l || /\s/.test(l) || i.indexOf(l) > -1 ? {
      changes: { insert: e + t, from: o.head },
      effects: Le.of(o.head + e.length),
      range: x.cursor(o.head + e.length)
    } : { range: r = o };
  });
  return r ? null : n.update(s, {
    scrollIntoView: !0,
    userEvent: "input.type"
  });
}
function Qc(n, e, t) {
  let i = null, r = n.changeByRange((s) => s.empty && ln(n.doc, s.head) == t ? {
    changes: { from: s.head, to: s.head + t.length, insert: t },
    range: x.cursor(s.head + t.length)
  } : i = { range: s });
  return i ? null : n.update(r, {
    scrollIntoView: !0,
    userEvent: "input.type"
  });
}
function Zc(n, e, t, i) {
  let r = i.stringPrefixes || ut.stringPrefixes, s = null, o = n.changeByRange((l) => {
    if (!l.empty)
      return {
        changes: [{ insert: e, from: l.from }, { insert: e, from: l.to }],
        effects: Le.of(l.to + e.length),
        range: x.range(l.anchor + e.length, l.head + e.length)
      };
    let a = l.head, c = ln(n.doc, a), f;
    if (c == e) {
      if (Ki(n, a))
        return {
          changes: { insert: e + e, from: a },
          effects: Le.of(a + e.length),
          range: x.cursor(a + e.length)
        };
      if (Ps(n, a)) {
        let u = t && n.sliceDoc(a, a + e.length * 3) == e + e + e ? e + e + e : e;
        return {
          changes: { from: a, to: a + u.length, insert: u },
          range: x.cursor(a + u.length)
        };
      }
    } else {
      if (t && n.sliceDoc(a - 2 * e.length, a) == e + e && (f = Gi(n, a - 2 * e.length, r)) > -1 && Ki(n, f))
        return {
          changes: { insert: e + e + e + e, from: a },
          effects: Le.of(a + e.length),
          range: x.cursor(a + e.length)
        };
      if (n.charCategorizer(a)(c) != K.Word && Gi(n, a, r) > -1 && !Yc(n, a, e, r))
        return {
          changes: { insert: e + e, from: a },
          effects: Le.of(a + e.length),
          range: x.cursor(a + e.length)
        };
    }
    return { range: s = l };
  });
  return s ? null : n.update(o, {
    scrollIntoView: !0,
    userEvent: "input.type"
  });
}
function Ki(n, e) {
  let t = _(n).resolveInner(e + 1);
  return t.parent && t.from == e;
}
function Yc(n, e, t, i) {
  let r = _(n).resolveInner(e, -1), s = i.reduce((o, l) => Math.max(o, l.length), 0);
  for (let o = 0; o < 5; o++) {
    let l = n.sliceDoc(r.from, Math.min(r.to, r.from + t.length + s)), a = l.indexOf(t);
    if (!a || a > -1 && i.indexOf(l.slice(0, a)) > -1) {
      let f = r.firstChild;
      for (; f && f.from == r.from && f.to - f.from > t.length + a; ) {
        if (n.sliceDoc(f.to - t.length, f.to) == t)
          return !1;
        f = f.firstChild;
      }
      return !0;
    }
    let c = r.to == e && r.parent;
    if (!c)
      break;
    r = c;
  }
  return !1;
}
function Gi(n, e, t) {
  let i = n.charCategorizer(e);
  if (i(n.sliceDoc(e - 1, e)) != K.Word)
    return e;
  for (let r of t) {
    let s = e - r.length;
    if (n.sliceDoc(s, e) == r && i(n.sliceDoc(s - 1, s)) != K.Word)
      return s;
  }
  return -1;
}
function Xc(n = {}) {
  return [
    Bc,
    J,
    $.of(n),
    Ic,
    ef,
    Cs
  ];
}
const Bs = [
  { key: "Ctrl-Space", run: Hi },
  { mac: "Alt-`", run: Hi },
  { key: "Escape", run: Cc },
  { key: "ArrowDown", run: /* @__PURE__ */ Pt(!0) },
  { key: "ArrowUp", run: /* @__PURE__ */ Pt(!1) },
  { key: "PageDown", run: /* @__PURE__ */ Pt(!0, "page") },
  { key: "PageUp", run: /* @__PURE__ */ Pt(!1, "page") },
  { key: "Enter", run: Sc }
], ef = /* @__PURE__ */ mt.highest(/* @__PURE__ */ Zt.computeN([$], (n) => n.facet($).defaultKeymap ? [Bs] : []));
class Ji {
  constructor(e, t, i) {
    this.from = e, this.to = t, this.diagnostic = i;
  }
}
class Be {
  constructor(e, t, i) {
    this.diagnostics = e, this.panel = t, this.selected = i;
  }
  static init(e, t, i) {
    let r = i.facet(dt).markerFilter;
    r && (e = r(e, i));
    let s = e.slice().sort((f, h) => f.from - h.from || f.to - h.to), o = new Gt(), l = [], a = 0;
    for (let f = 0; ; ) {
      let h = f == s.length ? null : s[f];
      if (!h && !l.length)
        break;
      let u, d;
      for (l.length ? (u = a, d = l.reduce((g, C) => Math.min(g, C.to), h && h.from > u ? h.from : 1e8)) : (u = h.from, d = h.to, l.push(h), f++); f < s.length; ) {
        let g = s[f];
        if (g.from == u && (g.to > g.from || g.to == u))
          l.push(g), f++, d = Math.min(g.to, d);
        else {
          d = Math.min(g.from, d);
          break;
        }
      }
      let y = pf(l);
      if (l.some((g) => g.from == g.to || g.from == g.to - 1 && i.doc.lineAt(g.from).to == g.from))
        o.add(u, u, A.widget({
          widget: new ff(y),
          diagnostics: l.slice()
        }));
      else {
        let g = l.reduce((C, P) => P.markClass ? C + " " + P.markClass : C, "");
        o.add(u, d, A.mark({
          class: "cm-lintRange cm-lintRange-" + y + g,
          diagnostics: l.slice(),
          inclusiveEnd: l.some((C) => C.to > d)
        }));
      }
      a = d;
      for (let g = 0; g < l.length; g++)
        l[g].to <= a && l.splice(g--, 1);
    }
    let c = o.finish();
    return new Be(c, t, Ge(c));
  }
}
function Ge(n, e = null, t = 0) {
  let i = null;
  return n.between(t, 1e9, (r, s, { spec: o }) => {
    if (!(e && o.diagnostics.indexOf(e) < 0))
      if (!i)
        i = new Ji(r, s, e || o.diagnostics[0]);
      else {
        if (o.diagnostics.indexOf(i.diagnostic) < 0)
          return !1;
        i = new Ji(i.from, s, i.diagnostic);
      }
  }), i;
}
function tf(n, e) {
  let t = e.pos, i = e.end || t, r = n.state.facet(dt).hideOn(n, t, i);
  if (r != null)
    return r;
  let s = n.startState.doc.lineAt(e.pos);
  return !!(n.effects.some((o) => o.is(Ds)) || n.changes.touchesRange(s.from, Math.max(s.to, i)));
}
function nf(n, e) {
  return n.field(Y, !1) ? e : e.concat(F.appendConfig.of(mf));
}
const Ds = /* @__PURE__ */ F.define(), si = /* @__PURE__ */ F.define(), Ls = /* @__PURE__ */ F.define(), Y = /* @__PURE__ */ me.define({
  create() {
    return new Be(A.none, null, null);
  },
  update(n, e) {
    if (e.docChanged && n.diagnostics.size) {
      let t = n.diagnostics.map(e.changes), i = null, r = n.panel;
      if (n.selected) {
        let s = e.changes.mapPos(n.selected.from, 1);
        i = Ge(t, n.selected.diagnostic, s) || Ge(t, null, s);
      }
      !t.size && r && e.state.facet(dt).autoPanel && (r = null), n = new Be(t, r, i);
    }
    for (let t of e.effects)
      if (t.is(Ds)) {
        let i = e.state.facet(dt).autoPanel ? t.value.length ? pt.open : null : n.panel;
        n = Be.init(t.value, i, e.state);
      } else t.is(si) ? n = new Be(n.diagnostics, t.value ? pt.open : null, n.selected) : t.is(Ls) && (n = new Be(n.diagnostics, n.panel, t.value));
    return n;
  },
  provide: (n) => [
    Nn.from(n, (e) => e.panel),
    T.decorations.from(n, (e) => e.diagnostics)
  ]
}), rf = /* @__PURE__ */ A.mark({ class: "cm-lintRange cm-lintRange-active" });
function sf(n, e, t) {
  let { diagnostics: i } = n.state.field(Y), r, s = -1, o = -1;
  i.between(e - (t < 0 ? 1 : 0), e + (t > 0 ? 1 : 0), (a, c, { spec: f }) => {
    if (e >= a && e <= c && (a == c || (e > a || t > 0) && (e < c || t < 0)))
      return r = f.diagnostics, s = a, o = c, !1;
  });
  let l = n.state.facet(dt).tooltipFilter;
  return r && l && (r = l(r, n.state)), r ? {
    pos: s,
    end: o,
    above: n.state.doc.lineAt(s).to < o,
    create() {
      return { dom: of(n, r) };
    }
  } : null;
}
function of(n, e) {
  return D("ul", { class: "cm-tooltip-lint" }, e.map((t) => Es(n, t, !1)));
}
const lf = (n) => {
  let e = n.state.field(Y, !1);
  (!e || !e.panel) && n.dispatch({ effects: nf(n.state, [si.of(!0)]) });
  let t = rt(n, pt.open);
  return t && t.dom.querySelector(".cm-panel-lint ul").focus(), !0;
}, Qi = (n) => {
  let e = n.state.field(Y, !1);
  return !e || !e.panel ? !1 : (n.dispatch({ effects: si.of(!1) }), !0);
}, af = (n) => {
  let e = n.state.field(Y, !1);
  if (!e)
    return !1;
  let t = n.state.selection.main, i = e.diagnostics.iter(t.to + 1);
  return !i.value && (i = e.diagnostics.iter(0), !i.value || i.from == t.from && i.to == t.to) ? !1 : (n.dispatch({ selection: { anchor: i.from, head: i.to }, scrollIntoView: !0 }), !0);
}, cf = [
  { key: "Mod-Shift-m", run: lf, preventDefault: !0 },
  { key: "F8", run: af }
], dt = /* @__PURE__ */ H.define({
  combine(n) {
    return Object.assign({ sources: n.map((e) => e.source).filter((e) => e != null) }, We(n.map((e) => e.config), {
      delay: 750,
      markerFilter: null,
      tooltipFilter: null,
      needsRefresh: null,
      hideOn: () => null
    }, {
      needsRefresh: (e, t) => e ? t ? (i) => e(i) || t(i) : e : t
    }));
  }
});
function Os(n) {
  let e = [];
  if (n)
    e: for (let { name: t } of n) {
      for (let i = 0; i < t.length; i++) {
        let r = t[i];
        if (/[a-zA-Z]/.test(r) && !e.some((s) => s.toLowerCase() == r.toLowerCase())) {
          e.push(r);
          continue e;
        }
      }
      e.push("");
    }
  return e;
}
function Es(n, e, t) {
  var i;
  let r = t ? Os(e.actions) : [];
  return D("li", { class: "cm-diagnostic cm-diagnostic-" + e.severity }, D("span", { class: "cm-diagnosticText" }, e.renderMessage ? e.renderMessage(n) : e.message), (i = e.actions) === null || i === void 0 ? void 0 : i.map((s, o) => {
    let l = !1, a = (u) => {
      if (u.preventDefault(), l)
        return;
      l = !0;
      let d = Ge(n.state.field(Y).diagnostics, e);
      d && s.apply(n, d.from, d.to);
    }, { name: c } = s, f = r[o] ? c.indexOf(r[o]) : -1, h = f < 0 ? c : [
      c.slice(0, f),
      D("u", c.slice(f, f + 1)),
      c.slice(f + 1)
    ];
    return D("button", {
      type: "button",
      class: "cm-diagnosticAction",
      onclick: a,
      onmousedown: a,
      "aria-label": ` Action: ${c}${f < 0 ? "" : ` (access key "${r[o]})"`}.`
    }, h);
  }), e.source && D("div", { class: "cm-diagnosticSource" }, e.source));
}
class ff extends Qt {
  constructor(e) {
    super(), this.sev = e;
  }
  eq(e) {
    return e.sev == this.sev;
  }
  toDOM() {
    return D("span", { class: "cm-lintPoint cm-lintPoint-" + this.sev });
  }
}
class Zi {
  constructor(e, t) {
    this.diagnostic = t, this.id = "item_" + Math.floor(Math.random() * 4294967295).toString(16), this.dom = Es(e, t, !0), this.dom.id = this.id, this.dom.setAttribute("role", "option");
  }
}
class pt {
  constructor(e) {
    this.view = e, this.items = [];
    let t = (r) => {
      if (r.keyCode == 27)
        Qi(this.view), this.view.focus();
      else if (r.keyCode == 38 || r.keyCode == 33)
        this.moveSelection((this.selectedIndex - 1 + this.items.length) % this.items.length);
      else if (r.keyCode == 40 || r.keyCode == 34)
        this.moveSelection((this.selectedIndex + 1) % this.items.length);
      else if (r.keyCode == 36)
        this.moveSelection(0);
      else if (r.keyCode == 35)
        this.moveSelection(this.items.length - 1);
      else if (r.keyCode == 13)
        this.view.focus();
      else if (r.keyCode >= 65 && r.keyCode <= 90 && this.selectedIndex >= 0) {
        let { diagnostic: s } = this.items[this.selectedIndex], o = Os(s.actions);
        for (let l = 0; l < o.length; l++)
          if (o[l].toUpperCase().charCodeAt(0) == r.keyCode) {
            let a = Ge(this.view.state.field(Y).diagnostics, s);
            a && s.actions[l].apply(e, a.from, a.to);
          }
      } else
        return;
      r.preventDefault();
    }, i = (r) => {
      for (let s = 0; s < this.items.length; s++)
        this.items[s].dom.contains(r.target) && this.moveSelection(s);
    };
    this.list = D("ul", {
      tabIndex: 0,
      role: "listbox",
      "aria-label": this.view.state.phrase("Diagnostics"),
      onkeydown: t,
      onclick: i
    }), this.dom = D("div", { class: "cm-panel-lint" }, this.list, D("button", {
      type: "button",
      name: "close",
      "aria-label": this.view.state.phrase("close"),
      onclick: () => Qi(this.view)
    }, "×")), this.update();
  }
  get selectedIndex() {
    let e = this.view.state.field(Y).selected;
    if (!e)
      return -1;
    for (let t = 0; t < this.items.length; t++)
      if (this.items[t].diagnostic == e.diagnostic)
        return t;
    return -1;
  }
  update() {
    let { diagnostics: e, selected: t } = this.view.state.field(Y), i = 0, r = !1, s = null, o = /* @__PURE__ */ new Set();
    for (e.between(0, this.view.state.doc.length, (l, a, { spec: c }) => {
      for (let f of c.diagnostics) {
        if (o.has(f))
          continue;
        o.add(f);
        let h = -1, u;
        for (let d = i; d < this.items.length; d++)
          if (this.items[d].diagnostic == f) {
            h = d;
            break;
          }
        h < 0 ? (u = new Zi(this.view, f), this.items.splice(i, 0, u), r = !0) : (u = this.items[h], h > i && (this.items.splice(i, h - i), r = !0)), t && u.diagnostic == t.diagnostic ? u.dom.hasAttribute("aria-selected") || (u.dom.setAttribute("aria-selected", "true"), s = u) : u.dom.hasAttribute("aria-selected") && u.dom.removeAttribute("aria-selected"), i++;
      }
    }); i < this.items.length && !(this.items.length == 1 && this.items[0].diagnostic.from < 0); )
      r = !0, this.items.pop();
    this.items.length == 0 && (this.items.push(new Zi(this.view, {
      from: -1,
      to: -1,
      severity: "info",
      message: this.view.state.phrase("No diagnostics")
    })), r = !0), s ? (this.list.setAttribute("aria-activedescendant", s.id), this.view.requestMeasure({
      key: this,
      read: () => ({ sel: s.dom.getBoundingClientRect(), panel: this.list.getBoundingClientRect() }),
      write: ({ sel: l, panel: a }) => {
        let c = a.height / this.list.offsetHeight;
        l.top < a.top ? this.list.scrollTop -= (a.top - l.top) / c : l.bottom > a.bottom && (this.list.scrollTop += (l.bottom - a.bottom) / c);
      }
    })) : this.selectedIndex < 0 && this.list.removeAttribute("aria-activedescendant"), r && this.sync();
  }
  sync() {
    let e = this.list.firstChild;
    function t() {
      let i = e;
      e = i.nextSibling, i.remove();
    }
    for (let i of this.items)
      if (i.dom.parentNode == this.list) {
        for (; e != i.dom; )
          t();
        e = i.dom.nextSibling;
      } else
        this.list.insertBefore(i.dom, e);
    for (; e; )
      t();
  }
  moveSelection(e) {
    if (this.selectedIndex < 0)
      return;
    let t = this.view.state.field(Y), i = Ge(t.diagnostics, this.items[e].diagnostic);
    i && this.view.dispatch({
      selection: { anchor: i.from, head: i.to },
      scrollIntoView: !0,
      effects: Ls.of(i)
    });
  }
  static open(e) {
    return new pt(e);
  }
}
function hf(n, e = 'viewBox="0 0 40 40"') {
  return `url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" ${e}>${encodeURIComponent(n)}</svg>')`;
}
function Bt(n) {
  return hf(`<path d="m0 2.5 l2 -1.5 l1 0 l2 1.5 l1 0" stroke="${n}" fill="none" stroke-width=".7"/>`, 'width="6" height="3"');
}
const uf = /* @__PURE__ */ T.baseTheme({
  ".cm-diagnostic": {
    padding: "3px 6px 3px 8px",
    marginLeft: "-1px",
    display: "block",
    whiteSpace: "pre-wrap"
  },
  ".cm-diagnostic-error": { borderLeft: "5px solid #d11" },
  ".cm-diagnostic-warning": { borderLeft: "5px solid orange" },
  ".cm-diagnostic-info": { borderLeft: "5px solid #999" },
  ".cm-diagnostic-hint": { borderLeft: "5px solid #66d" },
  ".cm-diagnosticAction": {
    font: "inherit",
    border: "none",
    padding: "2px 4px",
    backgroundColor: "#444",
    color: "white",
    borderRadius: "3px",
    marginLeft: "8px",
    cursor: "pointer"
  },
  ".cm-diagnosticSource": {
    fontSize: "70%",
    opacity: 0.7
  },
  ".cm-lintRange": {
    backgroundPosition: "left bottom",
    backgroundRepeat: "repeat-x",
    paddingBottom: "0.7px"
  },
  ".cm-lintRange-error": { backgroundImage: /* @__PURE__ */ Bt("#d11") },
  ".cm-lintRange-warning": { backgroundImage: /* @__PURE__ */ Bt("orange") },
  ".cm-lintRange-info": { backgroundImage: /* @__PURE__ */ Bt("#999") },
  ".cm-lintRange-hint": { backgroundImage: /* @__PURE__ */ Bt("#66d") },
  ".cm-lintRange-active": { backgroundColor: "#ffdd9980" },
  ".cm-tooltip-lint": {
    padding: 0,
    margin: 0
  },
  ".cm-lintPoint": {
    position: "relative",
    "&:after": {
      content: '""',
      position: "absolute",
      bottom: 0,
      left: "-2px",
      borderLeft: "3px solid transparent",
      borderRight: "3px solid transparent",
      borderBottom: "4px solid #d11"
    }
  },
  ".cm-lintPoint-warning": {
    "&:after": { borderBottomColor: "orange" }
  },
  ".cm-lintPoint-info": {
    "&:after": { borderBottomColor: "#999" }
  },
  ".cm-lintPoint-hint": {
    "&:after": { borderBottomColor: "#66d" }
  },
  ".cm-panel.cm-panel-lint": {
    position: "relative",
    "& ul": {
      maxHeight: "100px",
      overflowY: "auto",
      "& [aria-selected]": {
        backgroundColor: "#ddd",
        "& u": { textDecoration: "underline" }
      },
      "&:focus [aria-selected]": {
        background_fallback: "#bdf",
        backgroundColor: "Highlight",
        color_fallback: "white",
        color: "HighlightText"
      },
      "& u": { textDecoration: "none" },
      padding: 0,
      margin: 0
    },
    "& [name=close]": {
      position: "absolute",
      top: "0",
      right: "2px",
      background: "inherit",
      border: "none",
      font: "inherit",
      padding: 0,
      margin: 0
    }
  }
});
function df(n) {
  return n == "error" ? 4 : n == "warning" ? 3 : n == "info" ? 2 : 1;
}
function pf(n) {
  let e = "hint", t = 1;
  for (let i of n) {
    let r = df(i.severity);
    r > t && (t = r, e = i.severity);
  }
  return e;
}
const mf = [
  Y,
  /* @__PURE__ */ T.decorations.compute([Y], (n) => {
    let { selected: e, panel: t } = n.field(Y);
    return !e || !t || e.from == e.to ? A.none : A.set([
      rf.range(e.from, e.to)
    ]);
  }),
  /* @__PURE__ */ Us(sf, { hideOn: tf }),
  uf
], gf = [
  _s(),
  js(),
  Hs(),
  Bl(),
  Yo(),
  Vs(),
  Ks(),
  Oe.allowMultipleSelections.of(!0),
  Wo(),
  el(il, { fallback: !0 }),
  fl(),
  Uc(),
  Xc(),
  Gs(),
  Js(),
  Qs(),
  Ra(),
  Zt.of([
    ...Vc,
    ...cs,
    ...tc,
    ...ql,
    ...Go,
    ...Bs,
    ...cf
  ])
], yf = er`
  <svg
    xmlns="http://www.w3.org/2000/svg"
    height="24px"
    viewBox="0 -960 960 960"
    width="24px"
  >
    <path
      d="m644-428-58-58q9-47-27-88t-93-32l-58-58q17-8 34.5-12t37.5-4q75 0 127.5 52.5T660-500q0 20-4 37.5T644-428Zm128 126-58-56q38-29 67.5-63.5T832-500q-50-101-143.5-160.5T480-720q-29 0-57 4t-55 12l-62-62q41-17 84-25.5t90-8.5q151 0 269 83.5T920-500q-23 59-60.5 109.5T772-302Zm20 246L624-222q-35 11-70.5 16.5T480-200q-151 0-269-83.5T40-500q21-53 53-98.5t73-81.5L56-792l56-56 736 736-56 56ZM222-624q-29 26-53 57t-41 67q50 101 143.5 160.5T480-280q20 0 39-2.5t39-5.5l-36-38q-11 3-21 4.5t-21 1.5q-75 0-127.5-52.5T300-500q0-11 1.5-21t4.5-21l-84-82Zm319 93Zm-151 75Z"
    />
  </svg>
`, bf = (n, e = {}) => {
  n.config(async (t) => {
    let { languages: i, theme: r } = e;
    if (!i) {
      const { languages: s } = await import("./index-Daw4TR-O.js");
      i = s;
    }
    if (!r) {
      const { oneDark: s } = await import("./index-DJKHFWAZ.js");
      r = s;
    }
    t.update(Zs.key, (s) => {
      var o;
      return {
        extensions: [
          Zt.of(cs.concat(Ba)),
          gf,
          r,
          ...(o = e == null ? void 0 : e.extensions) != null ? o : []
        ],
        languages: i,
        expandIcon: e.expandIcon || (() => to),
        searchIcon: e.searchIcon || (() => eo),
        clearSearchIcon: e.clearSearchIcon || (() => Xs),
        searchPlaceholder: e.searchPlaceholder || "Search language",
        noResultText: e.noResultText || "No result",
        renderLanguage: e.renderLanguage || s.renderLanguage,
        renderPreview: e.renderPreview || s.renderPreview,
        previewToggleButton: (l) => {
          var a, c;
          return er`
            ${((a = e.previewToggleIcon) == null ? void 0 : a.call(e, l)) || (l ? no : yf)}
            ${((c = e.previewToggleText) == null ? void 0 : c.call(e, l)) || (l ? "Edit" : "Hide")}
          `;
        },
        previewLabel: e.previewLabel || s.previewLabel
      };
    });
  }).use(Ys);
}, Df = /* @__PURE__ */ Object.freeze(/* @__PURE__ */ Object.defineProperty({
  __proto__: null,
  defineFeature: bf
}, Symbol.toStringTag, { value: "Module" }));
export {
  hl as A,
  To as B,
  ks as C,
  io as D,
  Df as E,
  tn as H,
  R as I,
  cr as L,
  wf as N,
  rr as P,
  Ar as S,
  te as T,
  Cf as a,
  Sn as b,
  vo as c,
  Tf as d,
  Pf as e,
  zo as f,
  oc as g,
  _ as h,
  fr as i,
  Af as j,
  Mf as k,
  If as l,
  Bf as m,
  G as n,
  S as o,
  vf as p,
  Fn as q,
  O as r,
  el as s,
  m as t,
  ie as u,
  lr as v,
  qo as w,
  Yt as x,
  De as y,
  He as z
};
