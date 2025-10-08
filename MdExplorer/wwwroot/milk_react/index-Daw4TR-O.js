import { L as n, a, S as s } from "./index-B3KiKTSt-Dla4yOC_.js";
function t(e) {
  return new a(s.define(e));
}
function o(e) {
  return import("./index-fcJfYc1P.js").then((r) => r.sql({ dialect: r[e] }));
}
const m = [
  // New-style language modes
  /* @__PURE__ */ n.of({
    name: "C",
    extensions: ["c", "h", "ino"],
    load() {
      return import("./index-CaYrMNFe.js").then((e) => e.cpp());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "C++",
    alias: ["cpp"],
    extensions: ["cpp", "c++", "cc", "cxx", "hpp", "h++", "hh", "hxx"],
    load() {
      return import("./index-CaYrMNFe.js").then((e) => e.cpp());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "CQL",
    alias: ["cassandra"],
    extensions: ["cql"],
    load() {
      return o("Cassandra");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "CSS",
    extensions: ["css"],
    load() {
      return import("./index-Bjb4cgZN.js").then((e) => e.css());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Go",
    extensions: ["go"],
    load() {
      return import("./index-CxKKDT4X.js").then((e) => e.go());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "HTML",
    alias: ["xhtml"],
    extensions: ["html", "htm", "handlebars", "hbs"],
    load() {
      return import("./index-BXEC2rDE.js").then((e) => e.html());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Java",
    extensions: ["java"],
    load() {
      return import("./index-DqIzs31h.js").then((e) => e.java());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "JavaScript",
    alias: ["ecmascript", "js", "node"],
    extensions: ["js", "mjs", "cjs"],
    load() {
      return import("./index-DNsr7zpY.js").then((e) => e.javascript());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "JSON",
    alias: ["json5"],
    extensions: ["json", "map"],
    load() {
      return import("./index-DpS5s5Au.js").then((e) => e.json());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "JSX",
    extensions: ["jsx"],
    load() {
      return import("./index-DNsr7zpY.js").then((e) => e.javascript({ jsx: !0 }));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "LESS",
    extensions: ["less"],
    load() {
      return import("./index-x8Lflwxi.js").then((e) => e.less());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Liquid",
    extensions: ["liquid"],
    load() {
      return import("./index-CXD1A2ot.js").then((e) => e.liquid());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MariaDB SQL",
    load() {
      return o("MariaSQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Markdown",
    extensions: ["md", "markdown", "mkd"],
    load() {
      return import("./index-25rGbqkg.js").then((e) => e.markdown());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MS SQL",
    load() {
      return o("MSSQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MySQL",
    load() {
      return o("MySQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "PHP",
    extensions: ["php", "php3", "php4", "php5", "php7", "phtml"],
    load() {
      return import("./index-C4_lqffc.js").then((e) => e.php());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "PLSQL",
    extensions: ["pls"],
    load() {
      return o("PLSQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "PostgreSQL",
    load() {
      return o("PostgreSQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Python",
    extensions: ["BUILD", "bzl", "py", "pyw"],
    filename: /^(BUCK|BUILD)$/,
    load() {
      return import("./index-DMBvW7_g.js").then((e) => e.python());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Rust",
    extensions: ["rs"],
    load() {
      return import("./index-03LFkKCN.js").then((e) => e.rust());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Sass",
    extensions: ["sass"],
    load() {
      return import("./index-viDjrg2r.js").then((e) => e.sass({ indented: !0 }));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SCSS",
    extensions: ["scss"],
    load() {
      return import("./index-viDjrg2r.js").then((e) => e.sass());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SQL",
    extensions: ["sql"],
    load() {
      return o("StandardSQL");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SQLite",
    load() {
      return o("SQLite");
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TSX",
    extensions: ["tsx"],
    load() {
      return import("./index-DNsr7zpY.js").then((e) => e.javascript({ jsx: !0, typescript: !0 }));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TypeScript",
    alias: ["ts"],
    extensions: ["ts", "mts", "cts"],
    load() {
      return import("./index-DNsr7zpY.js").then((e) => e.javascript({ typescript: !0 }));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "WebAssembly",
    extensions: ["wat", "wast"],
    load() {
      return import("./index-BetZgtXv.js").then((e) => e.wast());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "XML",
    alias: ["rss", "wsdl", "xsd"],
    extensions: ["xml", "xsl", "xsd", "svg"],
    load() {
      return import("./index-BJD7FV-Z.js").then((e) => e.xml());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "YAML",
    alias: ["yml"],
    extensions: ["yaml", "yml"],
    load() {
      return import("./index-Db2n-tOX.js").then((e) => e.yaml());
    }
  }),
  // Legacy modes ported from CodeMirror 5
  /* @__PURE__ */ n.of({
    name: "APL",
    extensions: ["dyalog", "apl"],
    load() {
      return import("./apl-B2DGVGxc.js").then((e) => t(e.apl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "PGP",
    alias: ["asciiarmor"],
    extensions: ["asc", "pgp", "sig"],
    load() {
      return import("./asciiarmor-2LVJmxlE.js").then((e) => t(e.asciiArmor));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "ASN.1",
    extensions: ["asn", "asn1"],
    load() {
      return import("./asn1-jKiBa2Ya.js").then((e) => t(e.asn1({})));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Asterisk",
    filename: /^extensions\.conf$/i,
    load() {
      return import("./asterisk-DS281yxp.js").then((e) => t(e.asterisk));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Brainfuck",
    extensions: ["b", "bf"],
    load() {
      return import("./brainfuck-C_p9pTT8.js").then((e) => t(e.brainfuck));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Cobol",
    extensions: ["cob", "cpy"],
    load() {
      return import("./cobol-BlTKFDRj.js").then((e) => t(e.cobol));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "C#",
    alias: ["csharp", "cs"],
    extensions: ["cs"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.csharp));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Clojure",
    extensions: ["clj", "cljc", "cljx"],
    load() {
      return import("./clojure-CCKyeQKf.js").then((e) => t(e.clojure));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "ClojureScript",
    extensions: ["cljs"],
    load() {
      return import("./clojure-CCKyeQKf.js").then((e) => t(e.clojure));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Closure Stylesheets (GSS)",
    extensions: ["gss"],
    load() {
      return import("./css-bIlmDBTK.js").then((e) => t(e.gss));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "CMake",
    extensions: ["cmake", "cmake.in"],
    filename: /^CMakeLists\.txt$/,
    load() {
      return import("./cmake-CuaCgAKt.js").then((e) => t(e.cmake));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "CoffeeScript",
    alias: ["coffee", "coffee-script"],
    extensions: ["coffee"],
    load() {
      return import("./coffeescript-BVCvwO8I.js").then((e) => t(e.coffeeScript));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Common Lisp",
    alias: ["lisp"],
    extensions: ["cl", "lisp", "el"],
    load() {
      return import("./commonlisp-D_kxz07b.js").then((e) => t(e.commonLisp));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Cypher",
    extensions: ["cyp", "cypher"],
    load() {
      return import("./cypher-BMq4Fwjl.js").then((e) => t(e.cypher));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Cython",
    extensions: ["pyx", "pxd", "pxi"],
    load() {
      return import("./python-B5QdSKoL.js").then((e) => t(e.cython));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Crystal",
    extensions: ["cr"],
    load() {
      return import("./crystal-D309uH6_.js").then((e) => t(e.crystal));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "D",
    extensions: ["d"],
    load() {
      return import("./d-BZcgY6La.js").then((e) => t(e.d));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Dart",
    extensions: ["dart"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.dart));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "diff",
    extensions: ["diff", "patch"],
    load() {
      return import("./diff-Cg9d_RX2.js").then((e) => t(e.diff));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Dockerfile",
    filename: /^Dockerfile$/,
    load() {
      return import("./dockerfile-DIy8NleC.js").then((e) => t(e.dockerFile));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "DTD",
    extensions: ["dtd"],
    load() {
      return import("./dtd-CtLokQ-U.js").then((e) => t(e.dtd));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Dylan",
    extensions: ["dylan", "dyl", "intr"],
    load() {
      return import("./dylan-QYeExnWK.js").then((e) => t(e.dylan));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "EBNF",
    load() {
      return import("./ebnf-DUPDuY4r.js").then((e) => t(e.ebnf));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "ECL",
    extensions: ["ecl"],
    load() {
      return import("./ecl-CiXN-g_D.js").then((e) => t(e.ecl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "edn",
    extensions: ["edn"],
    load() {
      return import("./clojure-CCKyeQKf.js").then((e) => t(e.clojure));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Eiffel",
    extensions: ["e"],
    load() {
      return import("./eiffel-yQhjl4T1.js").then((e) => t(e.eiffel));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Elm",
    extensions: ["elm"],
    load() {
      return import("./elm-CNT9vbN0.js").then((e) => t(e.elm));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Erlang",
    extensions: ["erl"],
    load() {
      return import("./erlang-CFOYdy9e.js").then((e) => t(e.erlang));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Esper",
    load() {
      return import("./sql-mMre1Bo3.js").then((e) => t(e.esper));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Factor",
    extensions: ["factor"],
    load() {
      return import("./factor-DDOC7X6P.js").then((e) => t(e.factor));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "FCL",
    load() {
      return import("./fcl-CPC2WYrI.js").then((e) => t(e.fcl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Forth",
    extensions: ["forth", "fth", "4th"],
    load() {
      return import("./forth-BmxRyE9S.js").then((e) => t(e.forth));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Fortran",
    extensions: ["f", "for", "f77", "f90", "f95"],
    load() {
      return import("./fortran-9bvPyrOW.js").then((e) => t(e.fortran));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "F#",
    alias: ["fsharp"],
    extensions: ["fs"],
    load() {
      return import("./mllike-BSnXJBGA.js").then((e) => t(e.fSharp));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Gas",
    extensions: ["s"],
    load() {
      return import("./gas-BdfkXJT_.js").then((e) => t(e.gas));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Gherkin",
    extensions: ["feature"],
    load() {
      return import("./gherkin-CJuwpceU.js").then((e) => t(e.gherkin));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Groovy",
    extensions: ["groovy", "gradle"],
    filename: /^Jenkinsfile$/,
    load() {
      return import("./groovy-DZeT_VM-.js").then((e) => t(e.groovy));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Haskell",
    extensions: ["hs"],
    load() {
      return import("./haskell-Bvt3Qq1t.js").then((e) => t(e.haskell));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Haxe",
    extensions: ["hx"],
    load() {
      return import("./haxe-70NVW1pR.js").then((e) => t(e.haxe));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "HXML",
    extensions: ["hxml"],
    load() {
      return import("./haxe-70NVW1pR.js").then((e) => t(e.hxml));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "HTTP",
    load() {
      return import("./http-D9LttvKF.js").then((e) => t(e.http));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "IDL",
    extensions: ["pro"],
    load() {
      return import("./idl-B6TRFYjl.js").then((e) => t(e.idl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "JSON-LD",
    alias: ["jsonld"],
    extensions: ["jsonld"],
    load() {
      return import("./javascript-WMWNx-Vj.js").then((e) => t(e.jsonld));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Jinja2",
    extensions: ["j2", "jinja", "jinja2"],
    load() {
      return import("./jinja2-DnB6dQmV.js").then((e) => t(e.jinja2));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Julia",
    extensions: ["jl"],
    load() {
      return import("./julia-DpvXAuO6.js").then((e) => t(e.julia));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Kotlin",
    extensions: ["kt", "kts"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.kotlin));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "LiveScript",
    alias: ["ls"],
    extensions: ["ls"],
    load() {
      return import("./livescript-CanGTf8u.js").then((e) => t(e.liveScript));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Lua",
    extensions: ["lua"],
    load() {
      return import("./lua-XplVlWi_.js").then((e) => t(e.lua));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "mIRC",
    extensions: ["mrc"],
    load() {
      return import("./mirc-CFBPAOaF.js").then((e) => t(e.mirc));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Mathematica",
    extensions: ["m", "nb", "wl", "wls"],
    load() {
      return import("./mathematica-jaRHnSxC.js").then((e) => t(e.mathematica));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Modelica",
    extensions: ["mo"],
    load() {
      return import("./modelica-vUgVs--1.js").then((e) => t(e.modelica));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MUMPS",
    extensions: ["mps"],
    load() {
      return import("./mumps-CQoS1kWX.js").then((e) => t(e.mumps));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Mbox",
    extensions: ["mbox"],
    load() {
      return import("./mbox-BctzC1hL.js").then((e) => t(e.mbox));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Nginx",
    filename: /nginx.*\.conf$/i,
    load() {
      return import("./nginx-zDPm3Z74.js").then((e) => t(e.nginx));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "NSIS",
    extensions: ["nsh", "nsi"],
    load() {
      return import("./nsis-fePjrhq7.js").then((e) => t(e.nsis));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "NTriples",
    extensions: ["nt", "nq"],
    load() {
      return import("./ntriples-CsNjv2QF.js").then((e) => t(e.ntriples));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Objective-C",
    alias: ["objective-c", "objc"],
    extensions: ["m"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.objectiveC));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Objective-C++",
    alias: ["objective-c++", "objc++"],
    extensions: ["mm"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.objectiveCpp));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "OCaml",
    extensions: ["ml", "mli", "mll", "mly"],
    load() {
      return import("./mllike-BSnXJBGA.js").then((e) => t(e.oCaml));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Octave",
    extensions: ["m"],
    load() {
      return import("./octave-C8PmmSRH.js").then((e) => t(e.octave));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Oz",
    extensions: ["oz"],
    load() {
      return import("./oz-Ce8aN8oE.js").then((e) => t(e.oz));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Pascal",
    extensions: ["p", "pas"],
    load() {
      return import("./pascal-De0D6mP7.js").then((e) => t(e.pascal));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Perl",
    extensions: ["pl", "pm"],
    load() {
      return import("./perl-B4bSCe1C.js").then((e) => t(e.perl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Pig",
    extensions: ["pig"],
    load() {
      return import("./pig-D24Z8EXi.js").then((e) => t(e.pig));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "PowerShell",
    extensions: ["ps1", "psd1", "psm1"],
    load() {
      return import("./powershell-DkYVfTzP.js").then((e) => t(e.powerShell));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Properties files",
    alias: ["ini", "properties"],
    extensions: ["properties", "ini", "in"],
    load() {
      return import("./properties-Dn9wna3M.js").then((e) => t(e.properties));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "ProtoBuf",
    extensions: ["proto"],
    load() {
      return import("./protobuf-BPIjwpzm.js").then((e) => t(e.protobuf));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Pug",
    alias: ["jade"],
    extensions: ["pug", "jade"],
    load() {
      return import("./pug-BqUR2bBq.js").then((e) => t(e.pug));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Puppet",
    extensions: ["pp"],
    load() {
      return import("./puppet-nyd4dhjf.js").then((e) => t(e.puppet));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Q",
    extensions: ["q"],
    load() {
      return import("./q-DXjKs-tC.js").then((e) => t(e.q));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "R",
    alias: ["rscript"],
    extensions: ["r", "R"],
    load() {
      return import("./r-LKEuhEGI.js").then((e) => t(e.r));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "RPM Changes",
    load() {
      return import("./rpm-IznJm2Xc.js").then((e) => t(e.rpmChanges));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "RPM Spec",
    extensions: ["spec"],
    load() {
      return import("./rpm-IznJm2Xc.js").then((e) => t(e.rpmSpec));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Ruby",
    alias: ["jruby", "macruby", "rake", "rb", "rbx"],
    extensions: ["rb"],
    filename: /^(Gemfile|Rakefile)$/,
    load() {
      return import("./ruby-CcYfvIk6.js").then((e) => t(e.ruby));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SAS",
    extensions: ["sas"],
    load() {
      return import("./sas-7E8yHoCW.js").then((e) => t(e.sas));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Scala",
    extensions: ["scala"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.scala));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Scheme",
    extensions: ["scm", "ss"],
    load() {
      return import("./scheme-DjibxsNh.js").then((e) => t(e.scheme));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Shell",
    alias: ["bash", "sh", "zsh"],
    extensions: ["sh", "ksh", "bash"],
    filename: /^PKGBUILD$/,
    load() {
      return import("./shell-C0C2sNA_.js").then((e) => t(e.shell));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Sieve",
    extensions: ["siv", "sieve"],
    load() {
      return import("./sieve-Bwz7vjP5.js").then((e) => t(e.sieve));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Smalltalk",
    extensions: ["st"],
    load() {
      return import("./smalltalk-Bhddl2pB.js").then((e) => t(e.smalltalk));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Solr",
    load() {
      return import("./solr-BNlsLglM.js").then((e) => t(e.solr));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SML",
    extensions: ["sml", "sig", "fun", "smackspec"],
    load() {
      return import("./mllike-BSnXJBGA.js").then((e) => t(e.sml));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SPARQL",
    alias: ["sparul"],
    extensions: ["rq", "sparql"],
    load() {
      return import("./sparql-FarWu_Gb.js").then((e) => t(e.sparql));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Spreadsheet",
    alias: ["excel", "formula"],
    load() {
      return import("./spreadsheet-C-cy4P5N.js").then((e) => t(e.spreadsheet));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Squirrel",
    extensions: ["nut"],
    load() {
      return import("./clike-BUuHEmgZ.js").then((e) => t(e.squirrel));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Stylus",
    extensions: ["styl"],
    load() {
      return import("./stylus-DRcbY16X.js").then((e) => t(e.stylus));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Swift",
    extensions: ["swift"],
    load() {
      return import("./swift-DSxqR9R6.js").then((e) => t(e.swift));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "sTeX",
    load() {
      return import("./stex-92raWT1r.js").then((e) => t(e.stex));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "LaTeX",
    alias: ["tex"],
    extensions: ["text", "ltx", "tex"],
    load() {
      return import("./stex-92raWT1r.js").then((e) => t(e.stex));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "SystemVerilog",
    extensions: ["v", "sv", "svh"],
    load() {
      return import("./verilog-C4VGD9n1.js").then((e) => t(e.verilog));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Tcl",
    extensions: ["tcl"],
    load() {
      return import("./tcl-xfoLljhY.js").then((e) => t(e.tcl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Textile",
    extensions: ["textile"],
    load() {
      return import("./textile-D1AWE-pc.js").then((e) => t(e.textile));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TiddlyWiki",
    load() {
      return import("./tiddlywiki-5wqsXtSk.js").then((e) => t(e.tiddlyWiki));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Tiki wiki",
    load() {
      return import("./tiki-__Kn3CeS.js").then((e) => t(e.tiki));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TOML",
    extensions: ["toml"],
    load() {
      return import("./toml-BHiuTcfn.js").then((e) => t(e.toml));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Troff",
    extensions: ["1", "2", "3", "4", "5", "6", "7", "8", "9"],
    load() {
      return import("./troff-D2UO-fKf.js").then((e) => t(e.troff));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TTCN",
    extensions: ["ttcn", "ttcn3", "ttcnpp"],
    load() {
      return import("./ttcn-Bsa4sfRm.js").then((e) => t(e.ttcn));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "TTCN_CFG",
    extensions: ["cfg"],
    load() {
      return import("./ttcn-cfg-Bac_acMi.js").then((e) => t(e.ttcnCfg));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Turtle",
    extensions: ["ttl"],
    load() {
      return import("./turtle-xwJUxoPV.js").then((e) => t(e.turtle));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Web IDL",
    extensions: ["webidl"],
    load() {
      return import("./webidl-CjfDENEo.js").then((e) => t(e.webIDL));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "VB.NET",
    extensions: ["vb"],
    load() {
      return import("./vb-c2kQGd6-.js").then((e) => t(e.vb));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "VBScript",
    extensions: ["vbs"],
    load() {
      return import("./vbscript-Dz1TtKsy.js").then((e) => t(e.vbScript));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Velocity",
    extensions: ["vtl"],
    load() {
      return import("./velocity-DJd0pTTC.js").then((e) => t(e.velocity));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Verilog",
    extensions: ["v"],
    load() {
      return import("./verilog-C4VGD9n1.js").then((e) => t(e.verilog));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "VHDL",
    extensions: ["vhd", "vhdl"],
    load() {
      return import("./vhdl-T9HkrbI2.js").then((e) => t(e.vhdl));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "XQuery",
    extensions: ["xy", "xquery"],
    load() {
      return import("./xquery-BUQdORAS.js").then((e) => t(e.xQuery));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Yacas",
    extensions: ["ys"],
    load() {
      return import("./yacas-C0absKBh.js").then((e) => t(e.yacas));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Z80",
    extensions: ["z80"],
    load() {
      return import("./z80-C8rPtw-0.js").then((e) => t(e.z80));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MscGen",
    extensions: ["mscgen", "mscin", "msc"],
    load() {
      return import("./mscgen-Cpl0NYLN.js").then((e) => t(e.mscgen));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Xù",
    extensions: ["xu"],
    load() {
      return import("./mscgen-Cpl0NYLN.js").then((e) => t(e.xu));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "MsGenny",
    extensions: ["msgenny"],
    load() {
      return import("./mscgen-Cpl0NYLN.js").then((e) => t(e.msgenny));
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Vue",
    extensions: ["vue"],
    load() {
      return import("./index-sTs0qsI6.js").then((e) => e.vue());
    }
  }),
  /* @__PURE__ */ n.of({
    name: "Angular Template",
    load() {
      return import("./index-BE0sKD7p.js").then((e) => e.angular());
    }
  })
];
export {
  m as languages
};
