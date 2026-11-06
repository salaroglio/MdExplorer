#!/usr/bin/env python3
"""
Genera il set grafico «mde-space» delle presentazioni di MdExplorer: sfondi 1920x1080 e icone 128x128, in SVG.
Stile: fumetto (contorno scuro spesso, colori pieni, un riflesso chiaro), palette delle card di MDE (viola, rosa,
azzurro) sul blu notte. Le animazioni sono CSS dentro l'SVG e si fermano con prefers-reduced-motion.

    python3 toolIDE/mde-space/genera.py            # scrive in MdExplorer/wwwroot/mde-space/

Le stelle sono casuali ma ripetibili (seme fisso): rigenerare non cambia i file se non cambia lo script.
"""
import math
import os
import random

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "MdExplorer", "wwwroot", "mde-space")

# Palette
NOTTE = "#0b1026"
NOTTE2 = "#151b3d"
NOTTE3 = "#24306a"
INK = "#1b1f3b"          # il contorno del fumetto
VIOLA = "#667eea"
PORPORA = "#764ba2"
ROSA = "#f5576c"
ROSA2 = "#f093fb"
AZZURRO = "#4facfe"
CIANO = "#00f2fe"
ORO = "#ffd166"
VERDE = "#7ed957"
VERDE2 = "#4caf50"
BIANCO = "#f4f6ff"
LUNA1 = "#d5d9e3"
LUNA2 = "#a9afbe"
LUNA3 = "#7d8496"
MARTE1 = "#f2a65a"
MARTE2 = "#e2673a"
MARTE3 = "#b8442a"
MARTE4 = "#7a2a1e"

CALMA = "@media (prefers-reduced-motion: reduce){*{animation:none!important}}"


def svg(w, h, body, style=""):
    css = f"<style>{style}{CALMA}</style>" if style else ""
    return (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">'
            f"{css}{body}</svg>\n")


def write(rel, text):
    path = os.path.join(ROOT, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(text)
    print(rel, len(text))


# ---------------------------------------------------------------- sfondi

TWINKLE = ("@keyframes tw{0%,100%{opacity:.25}50%{opacity:1}}"
           ".t1{animation:tw 3.2s ease-in-out infinite}.t2{animation:tw 4.6s ease-in-out infinite 1.2s}"
           ".t3{animation:tw 5.4s ease-in-out infinite 2.1s}")


def cielo(seed, n=170, top=0, bottom=1080, nebulose=True):
    """Il cielo: gradiente notte, nebulose nei colori di MDE, stelle che brillano."""
    rnd = random.Random(seed)
    out = [
        '<defs><radialGradient id="cielo" cx="50%" cy="35%" r="80%">'
        f'<stop offset="0" stop-color="{NOTTE3}"/><stop offset=".55" stop-color="{NOTTE2}"/>'
        f'<stop offset="1" stop-color="{NOTTE}"/></radialGradient>'
        f'<radialGradient id="neb1"><stop offset="0" stop-color="{PORPORA}" stop-opacity=".45"/>'
        f'<stop offset="1" stop-color="{PORPORA}" stop-opacity="0"/></radialGradient>'
        f'<radialGradient id="neb2"><stop offset="0" stop-color="{AZZURRO}" stop-opacity=".28"/>'
        f'<stop offset="1" stop-color="{AZZURRO}" stop-opacity="0"/></radialGradient>'
        f'<radialGradient id="neb3"><stop offset="0" stop-color="{ROSA2}" stop-opacity=".22"/>'
        f'<stop offset="1" stop-color="{ROSA2}" stop-opacity="0"/></radialGradient></defs>',
        '<rect width="1920" height="1080" fill="url(#cielo)"/>',
    ]
    if nebulose:
        out.append('<ellipse cx="300" cy="220" rx="620" ry="300" fill="url(#neb1)"/>'
                   '<ellipse cx="1650" cy="760" rx="560" ry="320" fill="url(#neb2)"/>'
                   '<ellipse cx="1300" cy="120" rx="420" ry="200" fill="url(#neb3)"/>')
    for i in range(n):
        x, y = rnd.uniform(0, 1920), rnd.uniform(top, bottom)
        r = rnd.choice([0.8, 1, 1.2, 1.5, 1.8, 2.4])
        cls = rnd.choice(["", "", "t1", "t2", "t3"])
        col = rnd.choice([BIANCO, BIANCO, BIANCO, "#cdd6ff", "#ffe8b0"])
        c = f' class="{cls}"' if cls else ""
        out.append(f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r}" fill="{col}"{c}/>')
    # qualche stella a croce, quelle che «si notano»
    for _ in range(9):
        x, y = rnd.uniform(60, 1860), rnd.uniform(top + 40, min(bottom, 700))
        s = rnd.uniform(7, 13)
        out.append(f'<path class="t{rnd.randint(1, 3)}" d="M{x:.0f} {y - s:.0f} Q{x:.0f} {y:.0f} {x + s:.0f} {y:.0f} '
                   f'Q{x:.0f} {y:.0f} {x:.0f} {y + s:.0f} Q{x:.0f} {y:.0f} {x - s:.0f} {y:.0f} '
                   f'Q{x:.0f} {y:.0f} {x:.0f} {y - s:.0f}Z" fill="{BIANCO}"/>')
    return "".join(out)


def crateri(rnd, n, x0, x1, y0, y1, col_fondo, col_bordo):
    out = []
    for _ in range(n):
        x, y = rnd.uniform(x0, x1), rnd.uniform(y0, y1)
        rx = rnd.uniform(18, 70) * (0.6 + (y - y0) / (y1 - y0 + 1))
        ry = rx * 0.32
        out.append(f'<ellipse cx="{x:.0f}" cy="{y:.0f}" rx="{rx:.0f}" ry="{ry:.0f}" fill="{col_fondo}" '
                   f'stroke="{col_bordo}" stroke-width="4"/>'
                   f'<ellipse cx="{x:.0f}" cy="{y - ry * 0.25:.0f}" rx="{rx * 0.8:.0f}" ry="{ry * 0.55:.0f}" '
                   f'fill="{col_bordo}" opacity=".35"/>')
    return "".join(out)


def terra(cx, cy, r, alone=True):
    # l'alone sta bene in un cielo, in un'icona sembra una cornice
    halo = f'<circle cx="{cx}" cy="{cy}" r="{r + 14}" fill="{AZZURRO}" opacity=".18"/>' if alone else ""
    return (f'<g>{halo}'
            f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{AZZURRO}" stroke="{INK}" stroke-width="6"/>'
            f'<path d="M{cx - r * .55} {cy - r * .35} q{r * .3} {-r * .35} {r * .6} {-r * .1} q{r * .1} {r * .3} '
            f'{-r * .2} {r * .45} q{-r * .3} {r * .05} {-r * .4} {-r * .35}Z" fill="{VERDE2}" stroke="{INK}" stroke-width="4"/>'
            f'<path d="M{cx + r * .15} {cy + r * .15} q{r * .35} {-r * .1} {r * .5} {r * .2} q{-r * .05} {r * .35} '
            f'{-r * .35} {r * .4} q{-r * .25} {-r * .2} {-r * .15} {-r * .6}Z" fill="{VERDE2}" stroke="{INK}" stroke-width="4"/>'
            f'<path d="M{cx - r * .7} {cy - r * .2} a{r * .75} {r * .75} 0 0 1 {r * .45} {-r * .5}" fill="none" '
            f'stroke="{BIANCO}" stroke-width="6" stroke-linecap="round" opacity=".7"/></g>')


def razzo_g(scala=1.0):
    """Il razzo, centrato in 0,0, punta verso l'alto: lungo ~260 alla scala 1. La fiamma ha la classe «fiamma»."""
    s = scala
    return (f'<g transform="scale({s})">'
            f'<g class="fiamma"><path d="M-26 92 Q0 190 26 92Z" fill="{ORO}" stroke="{INK}" stroke-width="5"/>'
            f'<path d="M-13 92 Q0 150 13 92Z" fill="{ROSA}"/></g>'
            f'<path d="M-42 40 L-78 104 L-34 92Z" fill="{ROSA}" stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>'
            f'<path d="M42 40 L78 104 L34 92Z" fill="{ROSA}" stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>'
            f'<path d="M0 -130 C44 -86 50 -20 40 96 L-40 96 C-50 -20 -44 -86 0 -130Z" fill="{BIANCO}" stroke="{INK}" '
            f'stroke-width="7" stroke-linejoin="round"/>'
            f'<path d="M0 -130 C22 -108 30 -86 33 -70 L-33 -70 C-30 -86 -22 -108 0 -130Z" fill="{ROSA}" stroke="{INK}" '
            f'stroke-width="6" stroke-linejoin="round"/>'
            f'<circle cx="0" cy="-20" r="24" fill="{AZZURRO}" stroke="{INK}" stroke-width="7"/>'
            f'<path d="M-12 -30 a14 14 0 0 1 14 -8" fill="none" stroke="{BIANCO}" stroke-width="6" stroke-linecap="round"/>'
            f'<path d="M-22 96 L-8 116 L8 116 L22 96Z" fill="{LUNA2}" stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>'
            f'<path d="M-30 30 C-34 -10 -32 -50 -20 -80" fill="none" stroke="#ffffff" stroke-width="7" '
            f'stroke-linecap="round" opacity=".6"/>'
            '</g>')


def ufo_g(scala=1.0):
    s = scala
    return (f'<g transform="scale({s})">'
            f'<path d="M-40 18 L-70 70 M40 18 L70 70" stroke="{CIANO}" stroke-width="10" opacity=".0"/>'
            f'<path d="M-46 -6 C-44 -50 44 -50 46 -6Z" fill="{CIANO}" fill-opacity=".55" stroke="{INK}" stroke-width="6"/>'
            f'<path d="M-24 -30 a28 22 0 0 1 22 -10" fill="none" stroke="#ffffff" stroke-width="6" stroke-linecap="round"/>'
            f'<ellipse cx="0" cy="6" rx="110" ry="32" fill="{VIOLA}" stroke="{INK}" stroke-width="7"/>'
            f'<ellipse cx="0" cy="-2" rx="80" ry="14" fill="{PORPORA}" opacity=".6"/>'
            + "".join(f'<circle class="luce{i % 3}" cx="{x}" cy="{14 + abs(x) * -0.05:.0f}" r="8" fill="{ORO}" '
                      f'stroke="{INK}" stroke-width="4"/>' for i, x in enumerate([-72, -36, 0, 36, 72]))
            + '</g>')


def alieno_g(scala=1.0, colore=VERDE, saluta=True):
    """Un alieno a mezzo busto, centrato in 0,0 (la testa), per spuntare dal bordo."""
    s = scala
    braccio = (f'<g class="saluto"><path d="M62 70 Q110 40 118 -10" fill="none" stroke="{INK}" stroke-width="22" '
               f'stroke-linecap="round"/><path d="M62 70 Q110 40 118 -10" fill="none" stroke="{colore}" stroke-width="12" '
               f'stroke-linecap="round"/><circle cx="120" cy="-18" r="16" fill="{colore}" stroke="{INK}" stroke-width="6"/></g>'
               if saluta else "")
    return (f'<g transform="scale({s})">'
            f'<path d="M-62 150 C-70 80 -40 60 0 60 C40 60 70 80 62 150Z" fill="{colore}" stroke="{INK}" stroke-width="7"/>'
            + braccio
            # le antenne: colore del corpo su contorno scuro, come il resto (solo il contorno spariva sul blu notte)
            + "".join(f'<path d="M{sx * 30} -60 Q{sx * 50} -110 {sx * 66} -118" fill="none" stroke="{INK}" stroke-width="14" '
                      f'stroke-linecap="round"/><path d="M{sx * 30} -60 Q{sx * 50} -110 {sx * 66} -118" fill="none" '
                      f'stroke="{colore}" stroke-width="6" stroke-linecap="round"/>' for sx in (-1, 1)) +
            f'<circle cx="-68" cy="-120" r="12" fill="{ROSA}" stroke="{INK}" stroke-width="6"/>'
            f'<circle cx="68" cy="-120" r="12" fill="{ROSA}" stroke="{INK}" stroke-width="6"/>'
            f'<path d="M0 -78 C64 -78 84 -20 72 22 C60 66 -60 66 -72 22 C-84 -20 -64 -78 0 -78Z" fill="{colore}" '
            f'stroke="{INK}" stroke-width="7"/>'
            f'<ellipse cx="-28" cy="-8" rx="22" ry="28" fill="{INK}"/><ellipse cx="28" cy="-8" rx="22" ry="28" fill="{INK}"/>'
            f'<circle cx="-20" cy="-18" r="8" fill="#ffffff"/><circle cx="36" cy="-18" r="8" fill="#ffffff"/>'
            f'<path d="M-20 34 Q0 50 20 34" fill="none" stroke="{INK}" stroke-width="7" stroke-linecap="round"/>'
            f'<path d="M-50 -44 Q-36 -64 -14 -66" fill="none" stroke="#ffffff" stroke-width="7" stroke-linecap="round" opacity=".55"/>'
            '</g>')


def pianeta_anelli_g(r, corpo, anello, ombra):
    return (f'<g><path d="M{-r * 1.9} {r * .15} A{r * 1.9} {r * .45} 0 0 1 {r * 1.9} {-r * .15}" fill="none" '
            f'stroke="{INK}" stroke-width="{r * .22 + 8:.0f}" stroke-linecap="round"/>'
            f'<path d="M{-r * 1.9} {r * .15} A{r * 1.9} {r * .45} 0 0 1 {r * 1.9} {-r * .15}" fill="none" '
            f'stroke="{anello}" stroke-width="{r * .22:.0f}" stroke-linecap="round"/>'
            f'<circle r="{r}" fill="{corpo}" stroke="{INK}" stroke-width="7"/>'
            f'<path d="M{-r * .9} {r * .1} Q0 {r * .35} {r * .95} {-r * .05}" fill="none" stroke="{ombra}" '
            f'stroke-width="{r * .16:.0f}" opacity=".6"/>'
            f'<path d="M{-r * .55} {-r * .55} a{r * .7} {r * .7} 0 0 1 {r * .45} {-r * .25}" fill="none" stroke="#ffffff" '
            f'stroke-width="{r * .1 + 2:.0f}" stroke-linecap="round" opacity=".6"/>'
            f'<path d="M{-r * 1.9} {r * .15} A{r * 1.9} {r * .45} 0 0 0 {r * 1.9} {-r * .15}" fill="none" '
            f'stroke="{INK}" stroke-width="{r * .22 + 8:.0f}" stroke-linecap="round"/>'
            f'<path d="M{-r * 1.9} {r * .15} A{r * 1.9} {r * .45} 0 0 0 {r * 1.9} {-r * .15}" fill="none" '
            f'stroke="{anello}" stroke-width="{r * .22:.0f}" stroke-linecap="round"/></g>')


def sfondo_stelle():
    return svg(1920, 1080, cielo(7), TWINKLE)


def sfondo_luna():
    rnd = random.Random(11)
    suolo = (f'<path d="M0 820 C220 780 380 840 620 800 C860 760 1020 830 1280 790 C1520 752 1700 812 1920 780 '
             f'L1920 1080 L0 1080Z" fill="{LUNA2}" stroke="{INK}" stroke-width="8"/>'
             f'<path d="M0 900 C300 870 520 930 820 890 C1120 850 1400 920 1920 880 L1920 1080 L0 1080Z" fill="{LUNA1}" '
             f'stroke="{INK}" stroke-width="8"/>'
             + crateri(rnd, 9, 80, 1840, 930, 1040, LUNA2, LUNA3)
             + crateri(rnd, 6, 120, 1800, 815, 865, LUNA3, INK))
    bandiera = (f'<g transform="translate(1560 812)"><path d="M0 0 L0 -150" stroke="{INK}" stroke-width="8"/>'
                f'<path class="sventola" d="M4 -148 Q50 -168 96 -148 Q90 -120 96 -92 Q50 -110 4 -92Z" fill="{ROSA}" '
                f'stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>'
                f'<text x="50" y="-112" font-family="sans-serif" font-weight="800" font-size="30" text-anchor="middle" '
                f'fill="#ffffff">MDE</text></g>')
    stile = TWINKLE + ("@keyframes sv{0%,100%{transform:skewY(0deg)}50%{transform:skewY(-4deg)}}"
                       ".sventola{animation:sv 2.4s ease-in-out infinite;transform-box:fill-box;transform-origin:left center}")
    return svg(1920, 1080, cielo(13, n=150, bottom=780) + terra(1775, 160, 92) + suolo + bandiera, stile)


def sfondo_marte():
    rnd = random.Random(17)
    cielo_m = (f'<defs><linearGradient id="cm" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="{NOTTE2}"/>'
               f'<stop offset=".45" stop-color="{PORPORA}"/><stop offset=".8" stop-color="{ROSA}"/>'
               f'<stop offset="1" stop-color="{MARTE1}"/></linearGradient></defs>'
               '<rect width="1920" height="1080" fill="url(#cm)"/>')
    stelle = "".join(f'<circle cx="{rnd.uniform(0, 1920):.0f}" cy="{rnd.uniform(0, 380):.0f}" r="{rnd.choice([1, 1.4, 2])}" '
                     f'fill="{BIANCO}" class="t{rnd.randint(1, 3)}"/>' for _ in range(80))
    lune = (f'<circle cx="360" cy="200" r="34" fill="{LUNA2}" stroke="{INK}" stroke-width="6"/>'
            f'<circle cx="350" cy="192" r="8" fill="{LUNA3}"/><circle cx="372" cy="212" r="5" fill="{LUNA3}"/>'
            f'<circle cx="520" cy="140" r="18" fill="{LUNA1}" stroke="{INK}" stroke-width="5"/>')
    dune = (f'<path d="M0 760 C260 690 420 760 700 720 C980 680 1120 760 1420 700 C1640 660 1800 720 1920 690 L1920 1080 L0 1080Z" '
            f'fill="{MARTE2}" stroke="{INK}" stroke-width="8"/>'
            f'<path d="M0 860 C320 820 600 900 960 850 C1280 806 1560 880 1920 840 L1920 1080 L0 1080Z" fill="{MARTE3}" '
            f'stroke="{INK}" stroke-width="8"/>'
            f'<path d="M0 960 C400 930 760 990 1200 950 C1500 924 1760 970 1920 950 L1920 1080 L0 1080Z" fill="{MARTE4}" '
            f'stroke="{INK}" stroke-width="8"/>')
    sassi = "".join(f'<path d="M{x} {y} q14 -26 34 -8 q16 6 8 20Z" fill="{MARTE3}" stroke="{INK}" stroke-width="5"/>'
                    for x, y in [(240, 900), (680, 975), (1180, 905), (1640, 990), (1500, 880)])
    rover = (f'<g transform="translate(1380 822)"><rect x="-70" y="-50" width="140" height="44" rx="10" fill="{BIANCO}" '
             f'stroke="{INK}" stroke-width="6"/><path d="M-10 -50 L-10 -96 M-10 -96 L22 -110" stroke="{INK}" stroke-width="6" '
             f'stroke-linecap="round"/><circle cx="26" cy="-112" r="12" fill="{AZZURRO}" stroke="{INK}" stroke-width="5"/>'
             f'<rect x="-46" y="-42" width="40" height="22" rx="4" fill="{AZZURRO}" stroke="{INK}" stroke-width="4"/>'
             + "".join(f'<circle cx="{x}" cy="0" r="18" fill="{LUNA3}" stroke="{INK}" stroke-width="6"/>' for x in (-50, 0, 50))
             + '</g>')
    return svg(1920, 1080, cielo_m + stelle + lune + dune + sassi + rover, TWINKLE)


def sfondo_razzo():
    stile = TWINKLE + ("@keyframes vola{0%,100%{transform:translate(1450px,560px) rotate(28deg)}"
                       "50%{transform:translate(1462px,532px) rotate(26deg)}}"
                       ".volo{animation:vola 3.6s ease-in-out infinite}"
                       "@keyframes fi{0%,100%{transform:scaleY(1)}50%{transform:scaleY(1.25)}}"
                       ".fiamma{animation:fi .35s ease-in-out infinite;transform-box:fill-box;transform-origin:center top}")
    scia = (f'<path d="M1200 1080 C1300 900 1340 780 1390 690" fill="none" stroke="{ROSA2}" stroke-width="40" '
            f'stroke-linecap="round" opacity=".18"/>'
            f'<path d="M1240 1080 C1320 920 1360 790 1400 700" fill="none" stroke="{ORO}" stroke-width="14" '
            f'stroke-linecap="round" opacity=".28" stroke-dasharray="2 34"/>')
    pianeta = f'<g transform="translate(250 900)">{pianeta_anelli_g(130, VIOLA, ROSA2, PORPORA)}</g>'
    return svg(1920, 1080, cielo(23) + scia + pianeta + f'<g class="volo">{razzo_g(1.25)}</g>', stile)


def sfondo_orbite():
    cx, cy = 1520, 560
    stile = TWINKLE + ("@keyframes giro{to{transform:rotate(360deg)}}"
                       f".o1,.o2,.o3{{transform-box:view-box;transform-origin:{cx}px {cy}px}}"
                       ".o1{animation:giro 24s linear infinite}.o2{animation:giro 40s linear infinite reverse}"
                       ".o3{animation:giro 64s linear infinite}")
    sole = (f'<defs><radialGradient id="sole"><stop offset="0" stop-color="{ORO}"/><stop offset=".6" stop-color="{MARTE1}"/>'
            f'<stop offset="1" stop-color="{ROSA}" stop-opacity="0"/></radialGradient></defs>'
            f'<circle cx="{cx}" cy="{cy}" r="190" fill="url(#sole)" opacity=".55"/>'
            f'<circle cx="{cx}" cy="{cy}" r="78" fill="{ORO}" stroke="{INK}" stroke-width="8"/>'
            f'<path d="M{cx - 42} {cy - 30} a50 50 0 0 1 36 -26" fill="none" stroke="#ffffff" stroke-width="8" '
            f'stroke-linecap="round" opacity=".7"/>')
    anelli = "".join(f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="none" stroke="{BIANCO}" stroke-width="3" '
                     f'stroke-dasharray="6 14" opacity=".35"/>' for r in (170, 280, 400))
    p1 = (f'<g class="o1"><circle cx="{cx + 170}" cy="{cy}" r="26" fill="{AZZURRO}" stroke="{INK}" stroke-width="6"/></g>')
    p2 = (f'<g class="o2"><circle cx="{cx}" cy="{cy - 280}" r="40" fill="{MARTE2}" stroke="{INK}" stroke-width="7"/>'
          f'<circle cx="{cx - 10}" cy="{cy - 290}" r="8" fill="{MARTE3}"/></g>')
    p3 = f'<g class="o3"><g transform="translate({cx - 400} {cy})">{pianeta_anelli_g(46, VIOLA, ROSA2, PORPORA)}</g></g>'
    return svg(1920, 1080, cielo(29) + anelli + sole + p1 + p2 + p3, stile)


def sfondo_alieni():
    stile = TWINKLE + ("@keyframes sb{0%,100%{transform:translateY(0)}50%{transform:translateY(-14px)}}"
                       ".sbircia{animation:sb 2.8s ease-in-out infinite}"
                       "@keyframes sal{0%,100%{transform:rotate(0deg)}50%{transform:rotate(-14deg)}}"
                       ".saluto{animation:sal 1.2s ease-in-out infinite;transform-box:fill-box;transform-origin:left bottom}"
                       "@keyframes fl{0%,100%{transform:translate(1760px,150px)}50%{transform:translate(1748px,172px)}}"
                       ".disco{animation:fl 5s ease-in-out infinite}"
                       "@keyframes lu{0%,100%{fill:" + ORO + "}50%{fill:" + CIANO + "}}"
                       ".luce0{animation:lu 1s infinite}.luce1{animation:lu 1s infinite .33s}.luce2{animation:lu 1s infinite .66s}")
    suolo = (f'<path d="M0 930 C300 900 560 950 900 920 C1240 890 1560 950 1920 915 L1920 1080 L0 1080Z" fill="{LUNA2}" '
             f'stroke="{INK}" stroke-width="8"/>')
    a1 = f'<g transform="translate(210 930)"><g class="sbircia">{alieno_g(1.05, VERDE)}</g></g>'
    a2 = f'<g transform="translate(1720 945)"><g class="sbircia" style="animation-delay:1.1s">{alieno_g(.8, ROSA2, saluta=False)}</g></g>'
    # il raggio dell'UFO scende sull'alieno di destra: fuori dalla colonna del testo, al centro della slide
    raggio = (f'<path d="M1712 186 L1640 930 L1880 930 L1808 186Z" fill="{CIANO}" opacity=".14"/>')
    return svg(1920, 1080, cielo(31, bottom=900) + raggio + suolo + a1 + a2 + f'<g class="disco">{ufo_g(.85)}</g>', stile)


# ---------------------------------------------------------------- icone (128x128)

def icona(body, style=""):
    return svg(128, 128, body, style)


def icone():
    out = {}
    out["razzo"] = icona(f'<g transform="translate(64 62) rotate(35) scale(.36)">{razzo_g()}</g>')
    out["ufo"] = icona(f'<g transform="translate(64 66) scale(.5)">{ufo_g()}</g>')
    out["alieno"] = icona(f'<g transform="translate(60 62) scale(.42)">{alieno_g(saluta=False)}</g>'
                          '<rect y="124" width="128" height="4" fill="none"/>')
    out["pianeta"] = icona(f'<g transform="translate(64 64) rotate(-18)">{pianeta_anelli_g(28, VIOLA, ROSA2, PORPORA)}</g>')
    out["luna"] = icona(f'<circle cx="64" cy="64" r="46" fill="{LUNA1}" stroke="{INK}" stroke-width="6"/>'
                        f'<circle cx="48" cy="52" r="11" fill="{LUNA2}" stroke="{LUNA3}" stroke-width="3"/>'
                        f'<circle cx="80" cy="76" r="14" fill="{LUNA2}" stroke="{LUNA3}" stroke-width="3"/>'
                        f'<circle cx="74" cy="42" r="6" fill="{LUNA2}"/><circle cx="46" cy="84" r="7" fill="{LUNA2}"/>'
                        f'<path d="M34 46 a36 36 0 0 1 20 -18" fill="none" stroke="#ffffff" stroke-width="6" '
                        f'stroke-linecap="round" opacity=".7"/>')
    out["marte"] = icona(f'<circle cx="64" cy="64" r="46" fill="{MARTE2}" stroke="{INK}" stroke-width="6"/>'
                         f'<path d="M22 58 Q64 72 106 54" fill="none" stroke="{MARTE3}" stroke-width="9" opacity=".8"/>'
                         f'<path d="M30 84 Q66 96 98 82" fill="none" stroke="{MARTE3}" stroke-width="7" opacity=".7"/>'
                         f'<circle cx="80" cy="40" r="7" fill="{MARTE3}"/>'
                         f'<path d="M36 40 a36 36 0 0 1 20 -16" fill="none" stroke="#ffffff" stroke-width="6" '
                         f'stroke-linecap="round" opacity=".6"/>')
    out["terra"] = icona(terra(64, 64, 46, alone=False))
    out["stella"] = icona(f'<path d="M64 12 L78 46 L114 48 L86 72 L96 108 L64 88 L32 108 L42 72 L14 48 L50 46Z" '
                          f'fill="{ORO}" stroke="{INK}" stroke-width="6" stroke-linejoin="round"/>'
                          f'<path d="M58 34 L52 50" stroke="#ffffff" stroke-width="6" stroke-linecap="round" opacity=".7"/>')
    out["cometa"] = icona(f'<path d="M98 30 L30 82 Q24 96 38 98 L104 46Z" fill="{AZZURRO}" opacity=".45"/>'
                          f'<path d="M100 28 L44 72" stroke="{CIANO}" stroke-width="8" stroke-linecap="round" opacity=".7"/>'
                          f'<circle cx="40" cy="88" r="20" fill="{BIANCO}" stroke="{INK}" stroke-width="6"/>'
                          f'<circle cx="34" cy="84" r="5" fill="{LUNA2}"/>')
    out["satellite"] = icona(f'<g transform="rotate(-30 64 64)">'
                             f'<rect x="10" y="52" width="34" height="24" fill="{AZZURRO}" stroke="{INK}" stroke-width="5"/>'
                             f'<rect x="84" y="52" width="34" height="24" fill="{AZZURRO}" stroke="{INK}" stroke-width="5"/>'
                             f'<path d="M27 52 V76 M101 52 V76 M44 64 H52 M76 64 H84" stroke="{INK}" stroke-width="4"/>'
                             f'<rect x="50" y="46" width="28" height="36" rx="6" fill="{LUNA1}" stroke="{INK}" stroke-width="6"/>'
                             f'<path d="M64 46 V30" stroke="{INK}" stroke-width="5"/><circle cx="64" cy="26" r="6" fill="{ROSA}" '
                             f'stroke="{INK}" stroke-width="4"/></g>')
    out["astronauta"] = icona(f'<path d="M30 122 C30 92 46 84 64 84 C82 84 98 92 98 122Z" fill="{BIANCO}" stroke="{INK}" '
                              f'stroke-width="6"/><circle cx="64" cy="54" r="38" fill="{BIANCO}" stroke="{INK}" stroke-width="6"/>'
                              f'<rect x="38" y="38" width="52" height="34" rx="16" fill="{AZZURRO}" stroke="{INK}" stroke-width="6"/>'
                              f'<path d="M46 50 a14 10 0 0 1 14 -6" fill="none" stroke="#ffffff" stroke-width="5" '
                              f'stroke-linecap="round"/><rect x="54" y="98" width="20" height="12" rx="3" fill="{ROSA}" '
                              f'stroke="{INK}" stroke-width="4"/>')
    out["telescopio"] = icona(f'<path d="M64 74 L40 118 M64 74 L88 118 M64 74 V118" stroke="{INK}" stroke-width="6" '
                              f'stroke-linecap="round"/><g transform="rotate(-28 64 60)"><rect x="22" y="48" width="78" height="24" '
                              f'rx="6" fill="{VIOLA}" stroke="{INK}" stroke-width="6"/><rect x="96" y="44" width="16" height="32" rx="4" '
                              f'fill="{PORPORA}" stroke="{INK}" stroke-width="5"/><rect x="12" y="54" width="12" height="12" rx="2" '
                              f'fill="{LUNA2}" stroke="{INK}" stroke-width="4"/></g><circle cx="64" cy="72" r="7" fill="{ORO}" '
                              f'stroke="{INK}" stroke-width="4"/>')
    out["bandiera"] = icona(f'<path d="M14 112 Q64 96 114 112" fill="{LUNA2}" stroke="{INK}" stroke-width="6"/>'
                            f'<path d="M40 104 V18" stroke="{INK}" stroke-width="6" stroke-linecap="round"/>'
                            f'<path d="M42 20 Q70 10 100 20 Q96 38 100 56 Q70 46 42 56Z" fill="{ROSA}" stroke="{INK}" '
                            f'stroke-width="5" stroke-linejoin="round"/><text x="71" y="44" font-family="sans-serif" '
                            f'font-weight="800" font-size="18" text-anchor="middle" fill="#ffffff">MDE</text>')
    out["base"] = icona(f'<path d="M8 110 H120" stroke="{INK}" stroke-width="6" stroke-linecap="round"/>'
                        f'<path d="M22 108 A42 42 0 0 1 106 108Z" fill="{LUNA1}" stroke="{INK}" stroke-width="6"/>'
                        f'<path d="M64 66 V108 M36 80 Q64 72 92 80" fill="none" stroke="{LUNA3}" stroke-width="4"/>'
                        f'<rect x="54" y="88" width="20" height="20" rx="4" fill="{AZZURRO}" stroke="{INK}" stroke-width="4"/>'
                        f'<path d="M64 66 V40" stroke="{INK}" stroke-width="5"/><circle cx="64" cy="36" r="6" fill="{ORO}" '
                        f'stroke="{INK}" stroke-width="4"/>')
    return out


def main():
    write("sfondi/stelle.svg", sfondo_stelle())
    write("sfondi/luna.svg", sfondo_luna())
    write("sfondi/marte.svg", sfondo_marte())
    write("sfondi/razzo.svg", sfondo_razzo())
    write("sfondi/orbite.svg", sfondo_orbite())
    write("sfondi/alieni.svg", sfondo_alieni())
    for name, text in icone().items():
        write(f"icone/{name}.svg", text)


if __name__ == "__main__":
    main()
