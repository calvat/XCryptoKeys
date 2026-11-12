#!/usr/bin/env python3
"""Schéma sur mesure pour un post, décrit dans <campagne>/visuals.json.

Usage : python3 design/diagram.py accounts/XCryptoKeys/threads/pactus-001 [index ...] [--png]

visuals.json associe un numéro de post à un schéma :
  {"7": {"type": "timeline", "events": [...]}, "8": {...}}
Chaque type est décrit dans STYLE-GRAPHIQUE.md (section « Types de schéma »).
Champs communs : "title" (sinon card.title du post).

Écrit <campagne>/src/post_N.html avec la marque "auto-diagram" ; un fichier
écrit à la main (sans marque "auto-card" ni "auto-diagram") n'est jamais
écrasé. card.py ne touche pas aux fichiers "auto-diagram" (pas de marque
"auto-card"). Avec --png, produit aussi les PNG via design/render.js.
"""
import json, sys, os, html, re, math, random, subprocess

E = html.escape
DASH = 'stroke-dasharray="7 6"'
GLOWL = 'filter="url(#glowL)"'
HOT = 'stroke="#5cfcfa" stroke-opacity=".9" stroke-width="2"'


def t(s):
    """Texte court avec **gras cyan** et `code` autorisés."""
    s = E(str(s))
    s = re.sub(r'\*\*(.+?)\*\*', r'<b class="c">\1</b>', s)
    s = re.sub(r'`(.+?)`', r'<span class="m">\1</span>', s)
    return s


def kup(h):
    """Majuscules des petites étiquettes (.k, .th) sans toucher aux symboles :
    seuls les mots de 2 lettres ou plus (et « a ») passent en capitales ; les
    lettres seules (d, m, i'), les lettres grecques, le `code` et le texte entre
    guillemets gardent leur casse."""
    out, code, quote = [], 0, False
    for part in re.split(r'(<[^>]+>|&quot;|&amp;|&lt;|&gt;|[“”«»])', h):
        if not part: continue
        if part.startswith('<'):
            if 'class="m"' in part: code += 1
            elif part == '</span>' and code: code -= 1
            out.append(part); continue
        if part in ('&quot;', '“', '”', '«', '»'):
            quote = not quote; out.append(part); continue
        if part.startswith('&') or code or quote:
            out.append(part); continue
        part = part.replace('&#x27;', "'")
        part = re.sub(r"(?<![\w'’/\\.])([A-Za-z][a-z]+(?:['’][a-z]+)?|a)(?![\w'’])", lambda m: m.group(1).upper(), part)
        out.append(re.sub(r"(?<=[A-Z0-9])(['’]s)\b", lambda m: m.group(1).upper(), part).replace("'", '&#x27;'))
    return ''.join(out)


def tk(s):
    return kup(t(s))


def cls(item, base='card'):
    c = [base]
    if item.get('hl'): c.append('hl')
    if item.get('alt'): c.append('alt')
    if item.get('bad'): c.append('bad')
    if item.get('dim'): c.append('dim')
    return ' '.join(c)


def box(item, base='card', extra='', idattr=''):
    if isinstance(item, str): item = {'label': item}
    k = f'<div class="k">{tk(item["k"])}</div>' if item.get('k') else ''
    lab = f'<div class="lbl">{t(item["label"])}</div>' if item.get('label') else ''
    big = f'<div class="bigv">{t(item["big"])}</div>' if item.get('big') else ''
    sub = f'<div class="sub">{t(item["sub"])}</div>' if item.get('sub') else ''
    lines = ''.join(f'<div class="ml">{t(l)}</div>' for l in item.get('lines', []))
    return f'<div class="{cls(item, base)}" {idattr} style="{extra}">{k}{big}{lab}{lines}{sub}</div>'


def note(v):
    """Carte « idée clé » en bas : {"k": "...", "v": "..."}."""
    if not v: return ''
    if isinstance(v, str): v = {'v': v}
    k = f'<div class="k">{tk(v["k"])}</div>' if v.get('k') else ''
    return f'<div class="card hl note">{k}<div class="nv">{t(v["v"])}</div></div>'


# ---------------------------------------------------------------- types

def L_flow(s):
    steps = s['steps']
    out = '<div class="col fill"><div class="row grow center gap-arrow">'
    for i, st in enumerate(steps):
        if isinstance(st, str): st = {'label': st}
        out += box(st, base='card fl', extra='flex:1', idattr=f'id="f{i}"')
    out += '</div>' + note(s.get('note')) + '</div>'
    links = [[f'f{i}', f'f{i+1}', 'alt' if (steps[i+1].get('alt') if isinstance(steps[i+1], dict) else False) else 'a', 'h'] for i in range(len(steps) - 1)]
    if s.get('loop'): links.append([f'f{len(steps)-1}', 'f0', 'alt', 'loop'])
    return out, links


def L_split(s):
    src = s['src']; to = s['to']
    out = '<div class="row fill gap-arrow">'
    out += '<div class="col" style="width:%dpx;justify-content:center">' % s.get('src_w', 420)
    out += box(src, idattr='id="src"')
    if s.get('src_note'): out += f'<div class="sub pad" style="margin-top:12px">{t(s["src_note"])}</div>'
    out += '</div><div class="col grow" style="justify-content:center;gap:18px">'
    for i, x in enumerate(to):
        if isinstance(x, str): x = {'label': x}
        out += box(x, idattr=f'id="t{i}"')
    out += note(s.get('note')) + '</div></div>'
    links = [['src', f't{i}', 'alt' if (isinstance(x, dict) and (x.get('alt') or x.get('bad'))) else 'a', 'h'] for i, x in enumerate(to)]
    if s.get('reverse'):
        links = [[b, a, k, 'h'] for a, b, k, _ in links]
    return out, links


def L_compare(s):
    def side(x, tone):
        items = ''.join(f'<li class="{tone}">{t(i)}</li>' for i in x.get('items', []))
        big = f'<div class="bigv">{t(x["big"])}</div>' if x.get('big') else ''
        sub = f'<div class="sub">{t(x["sub"])}</div>' if x.get('sub') else ''
        c = 'card side ' + ('hl' if x.get('hl') else ('alt' if tone == 'v' else ''))
        return f'<div class="{c}"><div class="k {tone}">{tk(x["head"])}</div>{big}<ul>{items}</ul>{sub}</div>'
    out = '<div class="col fill"><div class="row grow" style="gap:70px;position:relative;align-items:center">'
    out += side(s['left'], 'c') + side(s['right'], 'v')
    vs = s.get("vs", "VS")
    out += f'<div class="vs"{" style=\"font-size:30px;color:#5cfcfa\"" if len(vs) == 1 else ""}>{E(vs)}</div></div>' + note(s.get('note')) + '</div>'
    return out, []


def L_stack(s):
    layers = s['layers']
    out = '<div class="row fill" style="gap:80px">'
    out += f'<div class="col" style="width:{s.get("w", 560)}px;gap:10px;justify-content:center">'
    if s.get('head'): out += f'<div class="k" style="margin-left:4px">{tk(s["head"])}</div>'
    for i, l in enumerate(layers):
        if isinstance(l, str): l = {'label': l}
        right = f'<span class="m dimtxt">{t(l["r"])}</span>' if l.get('r') else ''
        sub = f'<div class="sub">{t(l["sub"])}</div>' if l.get('sub') else ''
        out += f'<div class="{cls(l)} layer" id="l{i}"><div class="rowsb"><span class="lbl">{t(l["label"])}</span>{right}</div>{sub}</div>'
    out += '</div><div class="col grow" style="justify-content:center;gap:18px">'
    links = []
    for j, c in enumerate(s.get('side', [])):
        out += box(c, idattr=f'id="c{j}"')
        if 'from' in c: links.append([f'l{c["from"]}', f'c{j}', 'alt' if c.get('alt') else 'a', 'h'])
    out += note(s.get('note')) + '</div></div>'
    return out, links


def L_segments(s):
    items = s['items']
    tot = sum(i['value'] for i in items)
    out = '<div class="col fill" style="justify-content:center;gap:26px">'
    if s.get('head'): out += f'<div class="rowsb"><span class="k">{tk(s["head"])}</span><span class="k c">{tk(s.get("total", ""))}</span></div>'
    out += '<div class="segbar">'
    for i, it in enumerate(items):
        frac = it['value'] / tot
        c = 'seg ' + ('hl' if it.get('hl') else 'alt' if it.get('alt') else 's%d' % (i % 3))
        fits = lambda x: x and frac * 1080 > len(re.sub(r'[*`]', '', str(x))) * 9.5 + 16
        lab = t(it['label']) if fits(it['label']) else t(it.get('short') or it.get('display', '')) if fits(it.get('short') or it.get('display', '')) else ''
        out += f'<div class="{c}" style="flex:{it["value"]}"><span>{lab}</span></div>'
    out += '</div><div class="segleg">'
    for i, it in enumerate(items if s.get('legend', True) else []):
        d = it.get('display', '')
        cc = 'c' if it.get('hl') else ('v' if it.get('alt') else '')
        out += f'<div class="legi"><span class="dot {cc}"></span><span class="lbl sm">{t(it["label"])}</span> <span class="m {cc}">{t(d)}</span></div>'
    out += '</div>'
    if s.get('cards'):
        out += '<div class="row" style="gap:20px">' + ''.join(box(c, extra='flex:1') for c in s['cards']) + '</div>'
    out += note(s.get('note')) + '</div>'
    return out, []


def L_timeline(s):
    ev = s['events']; n = len(ev)
    span = s.get('span')
    out = '<div class="col fill"><div class="tl grow">'
    out += '<div class="axis"></div>'
    for i, e in enumerate(ev):
        at = e.get('at', (i + .5) / n)
        # avec une durée (span) sous l'axe, toutes les cartes vont au-dessus
        up = True if span else ((i % 2 == 0) if s.get('alternate', True) else True)
        c = 'hl' if e.get('hl') else ('alt' if e.get('alt') or e.get('bad') else '')
        sub = f'<div class="sub">{t(e["sub"])}</div>' if e.get('sub') else ''
        w = s.get('w', min(300, int(1000 / max(n - 1, 1) * 1.6)))
        x = at * 1080
        w = max(min(w, int(2 * x) + 60, int(2 * (1080 - x)) + 60), 180)  # carte centrée sur son point autant que possible
        lx = min(max(x - w / 2, 0), 1080 - w)
        out += f'<div class="ev {"up" if up else "dn"}" style="left:{lx:.0f}px;width:{w}px">'
        out += f'<div class="tick {c}"><span class="m">{t(e.get("t", ""))}</span></div>'
        out += f'<div class="card {c} evc"><div class="lbl sm">{t(e["label"])}</div>{sub}</div></div>'
        out += f'<div class="stem {"up" if up else "dn"} {c}" style="left:{at*100:.1f}%"></div>'
        out += f'<div class="pt {c}" style="left:{at*100:.1f}%"></div>'
    if span:
        a, b, lab = span
        out += f'<div class="span" style="left:{a*100}%;width:{(b-a)*100}%"><span>{t(lab)}</span></div>'
    out += '</div>' + note(s.get('note')) + '</div>'
    return out, []


def L_bars(s):
    items = s['items']; mx = max(i['value'] for i in items)
    log = s.get('log')
    lo = s.get('log_min', min(i['value'] for i in items) / 3)
    def sc(v): return (math.log10(v / lo) / math.log10(mx / lo)) if log else v / mx
    dense = len(items) > 4
    out = f'<div class="col fill{" dense" if dense else ""}" style="justify-content:center;gap:{12 if dense else 24}px">'
    if s.get('head'): out += f'<div class="k">{tk(s["head"])}</div>'
    # place réservée au texte de valeur : la longueur des barres reste proportionnelle
    rsv = max(len(re.sub(r'[*`]', '', str(it.get('display', it['value'])))) for it in items) * 12.7 + 18
    for it in items:
        c = 'hl' if it.get('hl') else ('alt' if it.get('alt') else '')
        f = max(sc(it['value']), 0.012)
        out += f'<div class="barrow"><div class="bl lbl sm">{t(it["label"])}</div><div class="bt"><div class="bar {c}" style="width:calc((100% - {rsv:.0f}px) * {f:.4f})"></div><span class="bv m {c}">{t(it.get("display", it["value"]))}</span></div></div>'
    out += note(s.get('note')) + '</div>'
    return out, []


def L_formula(s):
    out = '<div class="col fill" style="justify-content:center;gap:28px">'
    k = f'<div class="k">{tk(s["k"])}</div>' if s.get('k') else ''
    fs = f' style="font-size:{s["size"]}px"' if s.get('size') else ''
    out += f'<div class="card hl formula">{k}<div class="expr"{fs}>{t(s["expr"])}</div></div>'
    if s.get('terms'):
        out += '<div class="row" style="gap:20px">'
        for x in s['terms']:
            out += f'<div class="card term" style="flex:1"><div class="sym">{t(x["k"])}</div><div class="sub">{t(x["v"])}</div></div>'
        out += '</div>'
    if s.get('foot'): out += f'<div class="sub center">{t(s["foot"])}</div>'
    out += '</div>'
    return out, []


def L_fields(s):
    st = s['struct']
    kw = max(len(l.get('key', '') if isinstance(l, dict) else l[0]) for l in st['lines']) + 2
    out = f'<div class="row fill gap-arrow" style="--kw:{kw}ch">'
    out += f'<div class="card code" style="width:{s.get("w", 500)}px" id="code"><div class="k">{tk(st.get("name", ""))}</div>'
    for i, ln in enumerate(st['lines']):
        if isinstance(ln, dict):
            key, val, c = ln.get('key', ''), ln.get('val', ''), ('hlln' if ln.get('hl') else 'altln' if ln.get('alt') else '')
        else:
            key, val, c = ln[0], ln[1] if len(ln) > 1 else '', ''
        out += f'<div class="cl {c}" id="ln{i}"><span class="ck">{E(key)}</span><span class="cv">{E(str(val))}</span></div>'
    if st.get('foot'): out += f'<div class="sub" style="margin-top:10px">{t(st["foot"])}</div>'
    out += '</div><div class="col grow" style="justify-content:center;gap:18px">'
    links = []
    for j, c in enumerate(s.get('side', [])):
        out += box(c, idattr=f'id="c{j}"')
        if 'from' in c: links.append([f'ln{c["from"]}', f'c{j}', 'alt' if c.get('alt') else 'a', 'h'])
    out += note(s.get('note')) + '</div></div>'
    return out, links


def L_cycle(s):
    nodes = s['nodes']; n = len(nodes)
    cx, cy, rx, ry = s.get('cx', 540), 220, s.get('rx', 400), 160
    out = '<div class="abs fill">'
    svg = '<svg class="ov" width="1080" height="440"><g fill="none" stroke-width="2.5" filter="url(#glow)">'
    pts = []
    for i in range(n):
        a = -math.pi / 2 + 2 * math.pi * i / n
        pts.append((cx + rx * math.cos(a), cy + ry * math.sin(a)))
    for i in range(n):
        a1 = -math.pi / 2 + 2 * math.pi * (i + .28) / n
        a2 = -math.pi / 2 + 2 * math.pi * (i + .72) / n
        x1, y1 = cx + rx * math.cos(a1), cy + ry * math.sin(a1)
        x2, y2 = cx + rx * math.cos(a2), cy + ry * math.sin(a2)
        am = (a1 + a2) / 2
        qx, qy = cx + rx * 1.12 * math.cos(am), cy + ry * 1.12 * math.sin(am)
        nxt = nodes[(i + 1) % n]
        alt = isinstance(nxt, dict) and nxt.get('alt')
        col = '#ad59e9' if alt else '#5cfcfa'
        svg += f'<path d="M{x1:.0f},{y1:.0f} Q{qx:.0f},{qy:.0f} {x2:.0f},{y2:.0f}" stroke="{col}" {DASH if alt else ""} marker-end="url(#{"av" if alt else "a"})"/>'
    svg += '</g></svg>'
    out += svg
    w = s.get('node_w', 250)
    for i, (x, y) in enumerate(pts):
        nd = nodes[i] if isinstance(nodes[i], dict) else {'label': nodes[i]}
        out += box(nd, extra=f'position:absolute;left:{x-w/2:.0f}px;top:{y:.0f}px;width:{w}px;transform:translateY(-50%)')
    if s.get('center'):
        c = s['center'] if isinstance(s['center'], dict) else {'label': s['center']}
        out += f'<div class="ctr" style="left:{cx-150}px;top:{cy}px">{box({**c, "hl": c.get("hl", False)}, extra="width:300px;text-align:center;background:none;border:none;box-shadow:none")}</div>'
    out += '</div>'
    return out, []


def L_tree(s):
    """Arbre de Merkle : feuilles en bas, racine en haut. Un nœud impair est dupliqué (pointillé)."""
    leaves = s['leaves']; n = len(leaves)
    x0 = s.get('x0', 0); W = 1080 - x0 - s.get('x1', 0); H = 440
    depth = max(1, math.ceil(math.log2(n))) if n > 1 else 1
    slots = 2 ** depth
    sw = W / slots
    counts = [n]
    while counts[-1] > 1: counts.append((counts[-1] + 1) // 2)
    nl = len(counts)
    rowh = (H - 60) / max(nl - 1, 1)
    flat = s.get('flat')  # « flat » : toutes les feuilles directement sous la racine
    if flat:
        sw = W / n; counts = [n, 1]; nl = 2; rowh = H - 140
    def pos(li, j):
        if flat:
            return (x0 + (j + .5) * sw if li == 0 else x0 + W / 2), H - 60 - li * rowh
        span = 2 ** li
        return x0 + (j * span + span / 2) * sw, H - 30 - li * rowh
    svg = '<svg class="ov" width="1080" height="440"><g fill="none" stroke-width="2">'
    nodes = ''
    hlset = set(s.get('hl', []))
    proof = set()  # « proof » : les nœuds frères du chemin feuille → racine (preuve de Merkle)
    if s.get('proof') and hlset:
        j = min(hlset)
        for li in range(nl - 1):
            if (j ^ 1) < counts[li]: proof.add((li, j ^ 1))
            j //= 2
    for li in range(nl):
        for j in range(counts[li]):
            x, y = pos(li, j)
            if li == 0:
                c = 'card tn leaf' + (' hl' if j in hlset else ' alt' if (li, j) in proof else ''); lab = leaves[j]
            elif li == nl - 1:
                c = 'card tn hl root'; lab = s.get('root', 'Merkle root')
            else:
                c = 'card tn inner' + (' alt' if (li, j) in proof else ''); lab = s.get('inner', 'hash')
            nodes += f'<div class="{c}" style="left:{x:.0f}px;top:{y:.0f}px">{t(lab)}</div>'
            if li == 0: continue
            for kk in (range(n) if flat else (2 * j, 2 * j + 1)):
                kx, ky = pos(li - 1, kk)
                dup = kk >= counts[li - 1]
                col = '#ad59e9' if dup else '#46b6e9'
                svg += f'<path d="M{x:.0f},{y+16:.0f} C{x:.0f},{(y+ky)/2:.0f} {kx:.0f},{(y+ky)/2:.0f} {kx:.0f},{ky-16:.0f}" stroke="{col}" stroke-opacity=".85" {DASH if dup else ""}/>'
                if dup:
                    lab = (str(leaves[-1]) + ' copy') if li == 1 else 'copy'
                    nodes += f'<div class="card tn alt leaf" style="left:{kx:.0f}px;top:{ky:.0f}px">{t(lab)}</div>'
    svg += '</g></svg>'
    cap = ''
    if s.get('side'):
        side_left = s.get('side_pos', 'right') == 'left'
        sw_ = s.get('side_w', 330)
        cap = f'<div class="col" style="position:absolute;{"left" if side_left else "right"}:0;top:0;width:{sw_}px;gap:14px">' + ''.join(box(c) for c in s['side']) + note(s.get('note')) + '</div>'
    return f'<div class="abs fill">{svg}{nodes}{cap}</div>', []


def L_network(s, n_seed):
    rnd = random.Random(s.get('seed', n_seed))
    N = s.get('nodes', 24); hl = s.get('hl', 0); bad = s.get('bad', 0)
    x0 = s.get('x0', 420); W = 1080 - x0; H = 440
    pts = []
    tries = 0
    while len(pts) < N and tries < 5000:
        tries += 1
        x, y = x0 + 30 + rnd.random() * (W - 60), 25 + rnd.random() * (H - 50)
        if all((x - a) ** 2 + (y - b) ** 2 > (min(W, H) / math.sqrt(N) * .8) ** 2 for a, b in pts): pts.append((x, y))
    roles = ['h'] * hl + ['b'] * bad + ['n'] * (len(pts) - hl - bad)
    rnd.shuffle(roles)
    if s.get('cluster') and hl:
        # les nœuds « bad » sont les plus proches des nœuds « hl » (une grappe liée)
        hs = [i for i, r in enumerate(roles) if r == 'h']
        roles = ['h' if i in hs else 'n' for i in range(len(pts))]
        near = sorted((i for i in range(len(pts)) if i not in hs), key=lambda i: min((pts[i][0] - pts[h][0]) ** 2 + (pts[i][1] - pts[h][1]) ** 2 for h in hs))
        for i in near[:bad]: roles[i] = 'b'
    edges = []
    for i, (x, y) in enumerate(pts):
        d = sorted(range(len(pts)), key=lambda j: (pts[j][0] - x) ** 2 + (pts[j][1] - y) ** 2)[1:1 + s.get('degree', 3)]
        for j in d:
            if j > i or i not in sorted(range(len(pts)), key=lambda k: (pts[k][0] - pts[j][0]) ** 2 + (pts[k][1] - pts[j][1]) ** 2)[1:4]:
                edges.append((i, j))
    if s.get('isolate'):  # chaque nœud « hl » est relié à ses voisins « bad » les plus proches
        for h in [i for i, r in enumerate(roles) if r == 'h']:
            near = sorted((i for i, r in enumerate(roles) if r == 'b'), key=lambda i: (pts[i][0] - pts[h][0]) ** 2 + (pts[i][1] - pts[h][1]) ** 2)
            edges += [(h, b) for b in near[:s.get('degree', 3)] if (h, b) not in edges and (b, h) not in edges]
    path_edges = set()
    if s.get('path'):
        # « path » : les nœuds « hl » forment un chemin continu, du plus à gauche au plus à droite
        adj = {i: set() for i in range(len(pts))}
        for a, b in edges: adj[a].add(b); adj[b].add(a)
        a0 = min(range(len(pts)), key=lambda i: pts[i][0]); b0 = max(range(len(pts)), key=lambda i: pts[i][0])
        prev, q = {a0: None}, [a0]
        while q:
            u = q.pop(0)
            for w in sorted(adj[u]):
                if w not in prev: prev[w] = u; q.append(w)
        roles = ['b' if r == 'b' else 'n' for r in roles]
        u = b0 if b0 in prev else a0
        while u is not None:
            roles[u] = 'h'
            if prev[u] is not None: path_edges.add(frozenset((u, prev[u])))
            u = prev[u]
    svg = '<svg class="ov" width="1080" height="440"><g stroke="#2169af" stroke-opacity=".55" stroke-width="1.3">'
    for i, j in edges:
        x, y = pts[i]
        if s.get('isolate') and 'h' in (roles[i], roles[j]) and {roles[i], roles[j]} != {'h', 'b'}: continue  # le nœud « hl » n'a que des pairs « bad »
        hot = (frozenset((i, j)) in path_edges) if s.get('path') else (roles[i] == 'h' and roles[j] == 'h' and s.get('hl_links', True))
        svg += f'<line x1="{x:.0f}" y1="{y:.0f}" x2="{pts[j][0]:.0f}" y2="{pts[j][1]:.0f}" {HOT if hot else ""}/>'
    svg += '</g>'
    r = s.get('r', 9)
    for (x, y), ro in zip(pts, roles):
        if ro == 'h': svg += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r+2}" fill="#5cfcfa" filter="url(#glow)"/>'
        elif ro == 'b': svg += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r+1}" fill="#ad59e9" filter="url(#glow)"/>'
        else: svg += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r}" fill="#0b2053" stroke="#488fdb" stroke-width="2"/>'
    svg += '</svg>'
    side = f'<div class="col" style="position:absolute;left:0;top:0;bottom:0;width:{x0-40}px;justify-content:center;gap:16px">'
    for c in s.get('side', []): side += box(c)
    leg = ''
    for lab, cc in ((s.get('hl_label'), 'c'), (s.get('bad_label'), 'v'), (s.get('n_label'), '')):
        if lab: leg += f'<div class="legi"><span class="dot {cc}"></span><span class="sub">{t(lab)}</span></div>'
    if leg: side += f'<div class="col" style="gap:6px">{leg}</div>'
    side += note(s.get('note')) + '</div>'
    return f'<div class="abs fill">{svg}{side}</div>', []


def L_gauge(s):
    pct = s.get('pct', 0); R = 150; C = 2 * math.pi * R  # sans pct : anneau neutre, sans fraction
    col = 'url(#edgeD)'
    svg = f'''<svg width="380" height="380" viewBox="0 0 380 380"><circle cx="190" cy="190" r="{R}" fill="none" stroke="#172667" stroke-width="30"/>
<circle cx="190" cy="190" r="{R}" fill="none" stroke="{col}" stroke-width="30" stroke-linecap="round" {"" if pct else 'stroke-opacity="0"'} stroke-dasharray="{C*pct/100:.1f} {C:.1f}" transform="rotate(-90 190 190)" filter="url(#glow)"/>'''
    if s.get('mark') is not None:
        a = -math.pi / 2 + 2 * math.pi * s['mark'] / 100
        svg += f'<line x1="{190+(R-26)*math.cos(a):.0f}" y1="{190+(R-26)*math.sin(a):.0f}" x2="{190+(R+26)*math.cos(a):.0f}" y2="{190+(R+26)*math.sin(a):.0f}" stroke="#ad59e9" stroke-width="4"/>'
    svg += '</svg>'
    out = '<div class="row fill" style="gap:70px;align-items:center">'
    out += f'<div class="gauge">{svg}<div class="gin"><div class="gbig" style="font-size:{58 if len(s.get("big", "xxx")) <= 5 else 46 if len(s.get("big", "")) <= 7 else 36}px">{t(s.get("big", str(pct)+"%"))}</div><div class="sub">{t(s.get("label", ""))}</div></div></div>'
    out += '<div class="col grow" style="gap:18px;justify-content:center">' + ''.join(box(c) for c in s.get('side', [])) + note(s.get('note')) + '</div></div>'
    return out, []


def L_checklist(s):
    out = '<div class="row fill" style="gap:60px">'
    out += '<div class="col grow" style="gap:14px;justify-content:center">'
    if s.get('head'): out += f'<div class="k">{tk(s["head"])}</div>'
    for c in s['checks']:
        ok = c.get('ok', True)
        mark = '✓' if ok is True else ('✗' if ok is False else str(ok))
        cc = 'ok' if ok is True else ('no' if ok is False else 'q')
        sub = f'<div class="sub">{t(c["sub"])}</div>' if c.get('sub') else ''
        out += f'<div class="chk"><div class="mk {cc}">{E(mark)}</div><div><div class="lbl sm">{t(c["text"])}</div>{sub}</div></div>'
    out += '</div>'
    if s.get('side') or s.get('note'):
        out += f'<div class="col" style="width:{s.get("side_w", 400)}px;gap:18px;justify-content:center">' + ''.join(box(c) for c in s.get('side', [])) + note(s.get('note')) + '</div>'
    out += '</div>'
    return out, []


def L_hash(s):
    ins = s['inputs']
    out = '<div class="col fill"><div class="row grow gap-arrow" style="align-items:center">'
    out += '<div class="col" style="width:330px;gap:12px">'
    for i, x in enumerate(ins):
        if isinstance(x, str): x = {'label': x}
        out += box(x, base='card inp', idattr=f'id="i{i}"')
    out += '</div>'
    out += f'<div class="fn" id="fn"><div class="k">{tk(s.get("fn_k", "FUNCTION"))}</div><div class="fnl">{t(s["fn"])}</div></div>'
    o = s['out'] if isinstance(s['out'], dict) else {'label': s['out']}
    out += box(o, extra='flex:1', idattr='id="out"')
    out += '</div>' + note(s.get('note')) + '</div>'
    links = [[f'i{i}', 'fn', 'alt' if isinstance(x, dict) and x.get('alt') else 'a', 'h'] for i, x in enumerate(ins)] + [['fn', 'out', 'a', 'h']]
    return out, links


def L_chain(s):
    blocks = s['blocks']; br = s.get('branch')
    out = '<div class="col fill" style="gap:40px;justify-content:center">'
    out += '<div class="row gap-arrow">'
    for i, b in enumerate(blocks):
        if isinstance(b, str): b = {'label': b}
        k = b.get('k', '')
        sub = f'<div class="sub">{t(b["sub"])}</div>' if b.get('sub') else ''
        lines = ''.join(f'<div class="ml">{t(l)}</div>' for l in b.get('lines', []))
        out += f'<div class="{cls(b)} blk" id="b{i}" style="flex:1"><div class="bh m">{t(k)}</div><div class="lbl">{t(b["label"])}</div>{lines}{sub}</div>'
    out += '</div>'
    links = [[f'b{i}', f'b{i+1}', 'alt' if isinstance(blocks[i+1], dict) and (blocks[i+1].get('alt') or blocks[i+1].get('bad')) else 'a', 'h'] for i in range(len(blocks) - 1)]
    if br:
        at = br['at']; n = len(blocks)
        out += f'<div class="row gap-arrow" style="padding-left:{(at+1)*100/n:.1f}%">'
        for j, b in enumerate(br['blocks']):
            if isinstance(b, str): b = {'label': b}
            b = {**b, 'alt': b.get('alt', True)}
            sub = f'<div class="sub">{t(b["sub"])}</div>' if b.get('sub') else ''
            out += f'<div class="{cls(b)} blk" id="x{j}" style="width:{(1080-(n-1)*70)/n:.0f}px;flex:none"><div class="bh m">{t(b.get("k", ""))}</div><div class="lbl">{t(b["label"])}</div>{sub}</div>'
            links.append([f'b{at}' if j == 0 else f'x{j-1}', f'x{j}', 'alt', 'h' if j else 'down'])
        out += '</div>'
    out += note(s.get('note')) + '</div>'
    links += s.get('links', [])  # liens en plus : [de, vers, "a"|"alt", "h"], ids b0…, x0…
    return out, links


def L_steps(s):
    st = s['steps']
    out = '<div class="row fill" style="gap:60px">'
    out += '<div class="col grow steps" style="justify-content:center">'
    for i, x in enumerate(st):
        if isinstance(x, str): x = {'label': x}
        c = 'hl' if x.get('hl') else ('alt' if x.get('alt') or x.get('bad') else '')
        sub = f'<div class="sub">{t(x["sub"])}</div>' if x.get('sub') else ''
        num = x.get('n', i + 1)
        out += f'<div class="st"><div class="num {c}">{E(str(num))}</div><div class="stb"><div class="lbl sm">{t(x["label"])}</div>{sub}</div></div>'
    out += '</div>'
    if s.get('side') or s.get('note'):
        out += f'<div class="col" style="width:{s.get("side_w", 400)}px;gap:18px;justify-content:center">' + ''.join(box(c) for c in s.get('side', [])) + note(s.get('note')) + '</div>'
    out += '</div>'
    return out, []


def L_table(s):
    head = s['head']; rows = s['rows']
    def cell(v):
        v = str(v)
        if v in ('✓', 'yes'): return '<span class="ok">✓</span>'
        if v in ('✗', 'no'): return '<span class="no">✗</span>'
        return t(v)
    hlc = s.get('hl_col')
    cols = s.get('cols') or ' '.join(['1.3fr'] + ['1fr'] * (len(head) - 1))
    out = f'<div class="col fill" style="justify-content:center;gap:22px"><div class="card tbl" style="grid-template-columns:{cols}">'
    for j, h in enumerate(head):
        out += f'<div class="th {"hlc" if j == hlc else ""}">{tk(h)}</div>'
    for r in rows:
        for j, v in enumerate(r):
            out += f'<div class="td {"first" if j == 0 else ""} {"hlc" if j == hlc else ""}">{cell(v)}</div>'
    out += '</div>' + note(s.get('note')) + '</div>'
    return out, []


def L_curve(s):
    """Courbe(s) y = f(x) : {"curves": [{"expr": "100/x", "label": "x · y = k", "hl": true}],
    "xr": [0, 10], "yr": [0, 10], "points": [{"x": 2, "y": 50, "label": "…", "hl": true}],
    "vlines": [{"x": 4, "label": "kink"}], "xl": "axe x", "yl": "axe y", "w": 600, "side": [...], "note": {...}}
    expr : expression Python en x (module math disponible : sqrt, log, exp…)."""
    W = s.get('w', 600); H = 440; L, R, T, B = 56, 20, 40, 44
    x0, x1 = s.get('xr', [0, 10]); y0, y1 = s.get('yr', [0, 10])
    px = lambda x: L + (x - x0) / (x1 - x0) * (W - L - R)
    py = lambda y: H - B - (y - y0) / (y1 - y0) * (H - T - B)
    env = {k: getattr(math, k) for k in dir(math) if not k.startswith('_')}
    svg = f'<svg width="{W}" height="{H}" style="overflow:visible">'
    svg += f'<line x1="{L}" y1="{H-B}" x2="{W-R}" y2="{H-B}" stroke="#488fdb" stroke-width="2"/><line x1="{L}" y1="{T}" x2="{L}" y2="{H-B}" stroke="#488fdb" stroke-width="2"/>'
    if s.get('xl'): svg += f'<text x="{W-R}" y="{H-12}" text-anchor="end" class="ax">{E(s["xl"])}</text>'
    if s.get('yl'): svg += f'<text x="{L-8}" y="{T-16}" class="ax">{E(s["yl"])}</text>'
    for v in s.get('vlines', []):
        X = px(v['x'])
        svg += f'<line x1="{X:.1f}" y1="{T}" x2="{X:.1f}" y2="{H-B}" stroke="#ad59e9" stroke-width="2" {DASH}/>'
        if v.get('label'): svg += f'<text x="{X+8:.1f}" y="{T+14}" class="ax v">{E(v["label"])}</text>'
    for c in s.get('curves', []):
        pts = []
        for i in range(401):
            x = x0 + (x1 - x0) * i / 400
            try: y = eval(c['expr'], {'__builtins__': {}}, dict(env, x=x))
            except (ZeroDivisionError, ValueError, OverflowError): continue
            if isinstance(y, complex) or y != y: continue
            if y0 - (y1 - y0) * .05 <= y <= y1 + (y1 - y0) * .05: pts.append(f'{px(x):.1f},{py(y):.1f}')
            elif pts and c.get('clip', True): pts.append(f'{px(x):.1f},{py(min(max(y, y0), y1)):.1f}'); break
        col = '#5cfcfa' if c.get('hl') else '#ad59e9' if c.get('alt') or c.get('bad') else '#488fdb'
        dash = DASH if c.get('dash') else ''
        glow = GLOWL if c.get('hl') else ''
        svg += f'<polyline points="{" ".join(pts)}" fill="none" stroke="{col}" stroke-width="{4 if c.get("hl") else 3}" {dash} {glow}/>'
        if c.get('label') and pts:
            lx, ly = pts[min(int(len(pts) * c.get('label_at', .75)), len(pts) - 1)].split(',')
            svg += f'<text x="{float(lx)+c.get("label_dx", 10):.1f}" y="{float(ly)+c.get("label_dy", -12):.1f}" class="cl" fill="{col}">{E(c["label"])}</text>'
    for p in s.get('points', []):
        X, Y = px(p['x']), py(p['y'])
        col = '#5cfcfa' if p.get('hl') else '#ad59e9' if p.get('alt') or p.get('bad') else '#eaf6ff'
        svg += f'<circle cx="{X:.1f}" cy="{Y:.1f}" r="8" fill="{col}" filter="url(#glow)"/>'
        if p.get('label'):
            dx, anc = (14, 'start') if p.get('pos', 'r') == 'r' else (-14, 'end')
            svg += f'<text x="{X+dx:.1f}" y="{Y+p.get("dy", -12):.1f}" text-anchor="{anc}" class="cl" fill="{col}">{E(p["label"])}</text>'
    svg += '</svg>'
    out = f'<div class="row fill" style="gap:50px;align-items:center"><div style="width:{W}px;height:{H}px;flex:none">{svg}</div>'
    out += '<div class="col grow" style="gap:18px;justify-content:center">' + ''.join(box(c) for c in s.get('side', [])) + note(s.get('note')) + '</div></div>'
    return out, []


def L_big(s):
    """Chiffre clé + carte(s) explicative(s) : pour les posts « un nombre »."""
    out = '<div class="row fill" style="gap:60px;align-items:center">'
    out += f'<div class="col" style="width:{s.get("w", 470)}px;gap:8px"><div class="k">{tk(s.get("k", ""))}</div><div class="huge" style="font-size:{110 if len(s["big"]) <= 6 else 84 if len(s["big"]) <= 9 else 64 if len(s["big"]) <= 12 else 50}px">{t(s["big"])}</div><div class="sub lg">{t(s.get("label", ""))}</div></div>'
    out += '<div class="col grow" style="gap:18px;justify-content:center">' + ''.join(box(c) for c in s.get('side', [])) + note(s.get('note')) + '</div></div>'
    return out, []


LAYOUTS = {k[2:]: v for k, v in globals().items() if k.startswith('L_')}

# ---------------------------------------------------------------- page

CSS = '''
*{box-sizing:border-box}
body{margin:0;background:#040823;width:1200px;height:675px;overflow:hidden;font-family:Inter,sans-serif;color:#eaf6ff}
.page{position:relative;width:1200px;height:675px;overflow:hidden}
.bgsvg,.ov{position:absolute;left:0;top:0}
.t{font-family:'Exo 2',Inter,sans-serif}
.m,.ml,.k,.ck,.cv,.sig,.pre{font-family:'JetBrains Mono',monospace}
.pre{position:absolute;left:60px;top:50px;color:#5cfcfa;font-size:20px;font-weight:700;letter-spacing:2px;text-shadow:0 0 10px rgba(92,252,250,.7)}
.ttl{position:absolute;left:60px;top:80px;width:1080px;font-family:'Exo 2',sans-serif;font-weight:800;font-size:44px;line-height:1.1;white-space:nowrap}
.sig{position:absolute;right:60px;top:624px;color:#488fdb;font-size:15px}
.area{position:absolute;left:60px;top:165px;width:1080px;height:440px;display:flex}
.fill{width:100%;height:100%}
.ax{font-family:'JetBrains Mono',monospace;font-size:15px;fill:#8fa8dd}.ax.v{fill:#ad59e9}
.cl{font-family:Inter,sans-serif;font-size:18px;font-weight:700}
.abs{position:relative}
.row{display:flex;flex-direction:row}
.col{display:flex;flex-direction:column}
.grow{flex:1;min-width:0;min-height:0}
.center{align-items:center}
.gap-arrow{gap:70px}
.rowsb{display:flex;justify-content:space-between;align-items:baseline;gap:12px}
.card{background:linear-gradient(180deg,rgba(23,38,103,.85),rgba(11,32,83,.7));border:1.5px solid #2169af;border-radius:14px;padding:16px 20px;position:relative}
.card.hl{border:2.5px solid transparent;background:linear-gradient(#0b2053,#0b2053) padding-box,linear-gradient(90deg,#5cfcfa,#ad59e9) border-box;box-shadow:0 0 22px rgba(92,252,250,.28),0 0 30px rgba(173,89,233,.18)}
.card.alt{border:1.5px dashed #8565e4}
.card.bad{border:2px solid #ad59e9;box-shadow:0 0 18px rgba(173,89,233,.35)}
.card.dim{opacity:.55}
.k{font-size:14px;letter-spacing:2.5px;color:#488fdb;margin-bottom:6px}
.card.hl .k,.k.c{color:#5cfcfa}
.k.v,.card.alt .k,.card.bad .k{color:#ad59e9}
.lbl{font-family:'Exo 2',Inter,sans-serif;font-weight:700;font-size:23px;line-height:1.2;color:#eaf6ff}
.lbl.sm{font-size:21px}
.card.hl>.lbl{color:#5cfcfa}
.card.bad>.lbl{color:#c98af5}
.sub{font-size:16px;line-height:1.35;color:#8fa8dd;margin-top:5px}
.sub.lg{font-size:20px}
.ml{font-size:16px;line-height:1.55;color:#d6e8ff}
.bigv{font-family:'Exo 2',sans-serif;font-weight:800;font-size:44px;line-height:1.05;color:#5cfcfa;text-shadow:0 0 14px rgba(92,252,250,.45)}
b.c{color:#5cfcfa;font-weight:700}
.c{color:#5cfcfa}.v{color:#ad59e9}
.dimtxt{color:#7f9cd6;font-size:15px}
.note{margin-top:22px;padding:14px 22px}
.note .nv{font-family:'Exo 2',sans-serif;font-weight:800;font-size:26px;line-height:1.2}
.side{flex:1;padding:24px 28px}
.side .bigv{font-size:60px;margin:6px 0 4px}
.side.alt .bigv{color:#c98af5;text-shadow:0 0 14px rgba(173,89,233,.45)}
.side ul{margin:10px 0 0;padding:0;list-style:none}
.side li{font-size:19px;line-height:1.35;margin:9px 0;padding-left:24px;position:relative;color:#dbe8ff}
.side li:before{content:'';position:absolute;left:0;top:8px;width:10px;height:10px;border-radius:50%}
li.c:before{background:#5cfcfa;box-shadow:0 0 8px #5cfcfa}
li.v:before{background:#ad59e9;box-shadow:0 0 8px #ad59e9}
.vs{position:absolute;left:50%;top:50%;transform:translate(-50%,-50%);width:56px;height:56px;border-radius:50%;display:flex;align-items:center;justify-content:center;background:#081039;border:2px solid #4a7ce1;font-family:'JetBrains Mono';font-weight:700;font-size:15px;color:#8fa8dd}
.fl{min-height:150px;display:flex;flex-direction:column;justify-content:center;text-align:center;padding:20px 22px}
.fl .lbl{font-size:25px}
.layer{padding:11px 18px}
.layer .lbl{font-size:20px}
.segbar{display:flex;height:96px;border-radius:14px;overflow:hidden;border:1.5px solid #2169af;gap:3px;background:#2169af}
.seg{display:flex;align-items:center;justify-content:center;font-family:'JetBrains Mono';font-size:15px;color:#eaf6ff;white-space:nowrap;overflow:hidden;padding:0 4px}
.seg.s0{background:#172667}.seg.s1{background:#1d3a80}.seg.s2{background:#11285f}
.seg.hl{background:linear-gradient(90deg,rgba(92,252,250,.55),rgba(173,89,233,.55));color:#fff;font-weight:700}
.seg.alt{background:rgba(173,89,233,.4)}
.segleg{display:flex;flex-wrap:wrap;gap:12px 34px}
.legi{display:flex;align-items:center;gap:10px}
.dot{width:12px;height:12px;border-radius:50%;background:#488fdb;flex:none}
.dot.c{background:#5cfcfa;box-shadow:0 0 8px #5cfcfa}.dot.v{background:#ad59e9;box-shadow:0 0 8px #ad59e9}
.tl{position:relative}
.axis{position:absolute;left:0;right:0;top:50%;height:3px;background:linear-gradient(90deg,#2169af,#5cfcfa,#ad59e9);box-shadow:0 0 10px rgba(92,252,250,.5)}
.pt{position:absolute;top:50%;width:18px;height:18px;margin:-8px 0 0 -9px;border-radius:50%;background:#081039;border:3px solid #46b6e9}
.pt.hl{background:#5cfcfa;border-color:#5cfcfa;box-shadow:0 0 14px #5cfcfa}
.pt.alt{border-color:#ad59e9}
.ev{position:absolute;display:flex;flex-direction:column;align-items:center;text-align:center}
.ev.up{bottom:calc(50% + 20px)}.ev.dn{top:calc(50% + 20px);flex-direction:column-reverse}
.tick{margin:6px 0;color:#46b6e9;font-size:16px}
.tick.hl{color:#5cfcfa}.tick.alt{color:#ad59e9}
.evc{padding:12px 14px;width:100%}
.span{position:absolute;top:calc(50% + 22px);height:46px;border:1.5px dashed #ad59e9;border-top:none;border-radius:0 0 10px 10px;display:flex;align-items:center;justify-content:center}
.span span{font-family:'JetBrains Mono';font-size:16px;color:#c98af5;padding:0 10px}
.stem{position:absolute;width:2px;margin-left:-1px;height:20px;background:#46b6e9;opacity:.8}
.stem.up{bottom:50%}.stem.dn{top:50%}
.stem.hl{background:#5cfcfa}.stem.alt{background:#ad59e9}
.barrow{display:flex;align-items:center;gap:20px}
.bl{width:280px;font-size:22px;text-align:right;flex:none}
.bt{flex:1;display:flex;align-items:center;gap:14px}
.bar{flex:none;height:46px;border-radius:8px;background:linear-gradient(90deg,#2169af,#488fdb)}
.dense .bar{height:32px}.dense .note{margin-top:10px}
.bar.hl{background:linear-gradient(90deg,#5cfcfa,#ad59e9);box-shadow:0 0 16px rgba(92,252,250,.45)}
.bar.alt{background:linear-gradient(90deg,#8565e4,#ad59e9)}
.bv{font-size:21px;color:#d6e8ff;white-space:nowrap}
.formula{text-align:center;padding:30px 30px}
.expr{font-family:'Exo 2',sans-serif;font-weight:800;font-size:54px;line-height:1.15}
.term{padding:16px 20px}
.sym{font-family:'JetBrains Mono';font-weight:700;font-size:22px;color:#5cfcfa}
.center{text-align:center}
.code{padding:20px 24px;align-self:center}
.cl{display:flex;gap:16px;font-size:17px;line-height:1.5;padding:3px 10px;margin:0 -10px;border-radius:6px}
.ck{color:#46b6e9;min-width:var(--kw,12ch);flex:none}.cv{color:#d6e8ff}
.cl.hlln{background:rgba(92,252,250,.12);box-shadow:inset 3px 0 #5cfcfa}.cl.hlln .cv{color:#5cfcfa}
.cl.altln{background:rgba(173,89,233,.14);box-shadow:inset 3px 0 #ad59e9}.cl.altln .cv{color:#c98af5}
.ctr{position:absolute;transform:translateY(-50%)}
.tn{position:absolute;transform:translate(-50%,-50%);padding:7px 12px;font-family:'JetBrains Mono';font-size:15px;white-space:nowrap;text-align:center}
.tn.root{font-family:'Exo 2';font-weight:800;font-size:20px;color:#5cfcfa;padding:10px 20px}
.tn.inner{color:#8fa8dd;font-size:14px}
.gauge{position:relative;width:380px;height:380px;flex:none}
.gin{position:absolute;inset:0;display:flex;flex-direction:column;align-items:center;justify-content:center;text-align:center;padding:0 70px}
.gbig{white-space:nowrap;font-family:'Exo 2';font-weight:800;font-size:58px;color:#5cfcfa;text-shadow:0 0 14px rgba(92,252,250,.5);line-height:1}
.chk{display:flex;gap:18px;align-items:flex-start}
.mk{width:38px;height:38px;border-radius:50%;flex:none;display:flex;align-items:center;justify-content:center;font-weight:800;font-size:21px}
.mk.ok{color:#040823;background:#5cfcfa;box-shadow:0 0 12px rgba(92,252,250,.6)}
.mk.no{color:#fff;background:#ad59e9;box-shadow:0 0 12px rgba(173,89,233,.6)}
.mk.q{color:#eaf6ff;border:2px solid #488fdb;font-size:16px;font-family:'JetBrains Mono'}
.fn{width:200px;height:200px;flex:none;border-radius:50%;display:flex;flex-direction:column;align-items:center;justify-content:center;text-align:center;border:2.5px solid transparent;background:radial-gradient(#172667,#0b2053) padding-box,linear-gradient(135deg,#5cfcfa,#ad59e9) border-box;box-shadow:0 0 30px rgba(92,252,250,.3);padding:20px}
.fnl{font-family:'Exo 2';font-weight:800;font-size:24px;color:#eaf6ff;line-height:1.15}
.inp{padding:12px 18px}.inp .lbl{font-size:19px}
.blk{padding:0 0 14px;overflow:hidden}
.blk .bh{background:rgba(33,105,175,.35);padding:7px 16px;font-size:14px;letter-spacing:2px;color:#8fa8dd;margin-bottom:10px;min-height:32px}
.blk.hl .bh{background:linear-gradient(90deg,rgba(92,252,250,.25),rgba(173,89,233,.25));color:#5cfcfa}
.blk.alt .bh,.blk.bad .bh{background:rgba(173,89,233,.25);color:#c98af5}
.blk>.lbl,.blk>.sub,.blk>.ml{padding:0 16px}
.steps{gap:0}
.st{display:flex;gap:20px;position:relative;padding-bottom:20px}
.st:not(:last-child):after{content:'';position:absolute;left:21px;top:46px;bottom:2px;width:2px;background:linear-gradient(#46b6e9,rgba(70,182,233,.2))}
.num{width:44px;height:44px;border-radius:50%;flex:none;display:flex;align-items:center;justify-content:center;font-family:'JetBrains Mono';font-weight:700;font-size:18px;color:#5cfcfa;border:2px solid #2169af;background:#081039}
.num.hl{background:#5cfcfa;color:#040823;border-color:#5cfcfa;box-shadow:0 0 14px rgba(92,252,250,.6)}
.num.alt{border-color:#ad59e9;color:#ad59e9}
.stb{padding-top:7px}
.tbl{display:grid;padding:6px 0;gap:0}
.th{font-family:'JetBrains Mono';font-size:14px;letter-spacing:2px;color:#488fdb;padding:12px 18px;border-bottom:1.5px solid #2169af}
.td{font-size:18px;padding:11px 18px;border-bottom:1px solid rgba(33,105,175,.35);color:#dbe8ff}
.td.first{font-family:'Exo 2';font-weight:700;color:#eaf6ff}
.hlc{background:rgba(92,252,250,.07)}.th.hlc{color:#5cfcfa}
.ok{color:#5cfcfa;font-weight:800;font-size:22px;text-shadow:0 0 8px rgba(92,252,250,.6)}
.no{color:#ad59e9;font-weight:800;font-size:22px}
.huge{white-space:nowrap;font-family:'Exo 2';font-weight:800;font-size:110px;line-height:1;background:linear-gradient(90deg,#5cfcfa,#ad59e9);-webkit-background-clip:text;color:transparent;filter:drop-shadow(0 0 14px rgba(92,252,250,.35))}
.pad{padding:0 6px}
'''

DEFS = '''<defs>
 <radialGradient id="bg" cx="{cx}%" cy="{cy}%" r="85%"><stop offset="0" stop-color="#10235b"/><stop offset=".55" stop-color="#081039"/><stop offset="1" stop-color="#040823"/></radialGradient>
 <linearGradient id="edgeD" x1="0" x2="1" y1="0" y2="1"><stop offset="0" stop-color="#5cfcfa"/><stop offset="1" stop-color="#ad59e9"/></linearGradient>
 <filter id="glow" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="5" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
 <filter id="soft" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="60"/></filter>
 <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse"><path d="M40 0H0V40" fill="none" stroke="#2169af" stroke-opacity=".12"/></pattern>
 <marker id="a" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M0,0L10,5L0,10z" fill="#5cfcfa"/></marker>
 <marker id="av" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M0,0L10,5L0,10z" fill="#ad59e9"/></marker>
</defs>'''

TRACES = [
    ('M0 610H120L150 580H330', (330, 580)), ('M0 640H200L225 615H400', (400, 615)),
    ('M1200 140H1150L1120 110H1010', (1010, 110)), ('M1200 600H1100L1070 572H960', (960, 572)),
    ('M1200 40H1090L1060 70H980', (980, 70)), ('M700 675V650L725 625H840', (840, 625)),
    ('M0 150H30L55 175', (55, 175)), ('M1200 330H1170L1150 350', (1150, 350)),
]
BITS = ['0x7061637475732d73', '01001011 0x00 10110', '0x 1001 0110 tx[0]', '0xdeadbeef 1101', 'sha256(0x00…)', '0b1011 0x1f 0x2a', 'h = H(l ‖ r)', '0x9f2c 0110 01']


def bg_svg(n, clear_bottom=False):
    rnd = random.Random(n * 7919)
    cx, cy = rnd.choice([(55, 45), (70, 50), (40, 60), (60, 35), (50, 55)])
    s = f'<svg class="bgsvg" width="1200" height="675">{DEFS.format(cx=cx, cy=cy)}'
    s += '<rect width="1200" height="675" fill="url(#bg)"/><rect width="1200" height="675" fill="url(#grid)"/>'
    h1 = rnd.choice([(250, 420), (300, 500), (180, 300), (600, 520)])
    h2 = rnd.choice([(1000, 200), (950, 450), (1050, 120), (850, 300)])
    s += f'<circle cx="{h1[0]}" cy="{h1[1]}" r="170" fill="#2169af" opacity=".33" filter="url(#soft)"/>'
    s += f'<circle cx="{h2[0]}" cy="{h2[1]}" r="160" fill="#8565e4" opacity=".28" filter="url(#soft)"/>'
    tr = rnd.sample(TRACES, 3)
    if clear_bottom:  # pas de trace décorative sous un schéma qui descend jusqu'en bas (arbre)
        tr = [x for x in tr if not x[0].startswith(('M0 610', 'M0 640'))]
    s += '<g fill="none" stroke="#46b6e9" stroke-opacity=".3" stroke-width="1.5">' + ''.join(f'<path d="{p}"/>' for p, _ in tr) + '</g>'
    s += '<g fill="#5cfcfa" opacity=".55">' + ''.join(f'<circle cx="{x}" cy="{y}" r="3"/>' for _, (x, y) in tr) + '</g>'
    b = rnd.sample(BITS, 2)
    s += f'<g font-family="JetBrains Mono,monospace" font-size="13" fill="#46b6e9" opacity=".2"><text x="1140" y="36" text-anchor="end">{E(b[0])}</text><text x="40" y="662">{E(b[1])}</text></g>'
    return s + '</svg>'


JS = '''
document.fonts.ready.then(()=>{
 const L = %s; const ov = document.getElementById('links'); const P = document.querySelector('.page').getBoundingClientRect();
 for (const [a,b,kind,dir] of L){
  const A=document.getElementById(a), B=document.getElementById(b); if(!A||!B) continue;
  const ra=A.getBoundingClientRect(), rb=B.getBoundingClientRect(); let d;
  if (dir==='down'){ const x1=ra.left+ra.width/2-P.left, y1=ra.bottom-P.top+4, x2=rb.left-P.left-6, y2=rb.top+rb.height/2-P.top;
    d=`M${x1},${y1} C${x1},${y2} ${x1},${y2} ${x2},${y2}`; }
  else if (dir==='loop'){ const x1=ra.left+ra.width/2-P.left, y1=ra.bottom-P.top+4, x2=rb.left+rb.width/2-P.left, y2=rb.bottom-P.top+8;
    const yy=Math.max(y1,y2)+38; d=`M${x1},${y1} C${x1},${yy} ${x2},${yy} ${x2},${y2}`; }
  else if (rb.right < ra.left) { const x1=ra.left-P.left-2, y1=ra.top+ra.height/2-P.top, x2=rb.right-P.left+6, y2=rb.top+rb.height/2-P.top, m=(x1+x2)/2;
    d=`M${x1},${y1} C${m},${y1} ${m},${y2} ${x2},${y2}`; }
  else { const x1=ra.right-P.left+2, y1=ra.top+ra.height/2-P.top, x2=rb.left-P.left-6, y2=rb.top+rb.height/2-P.top, m=(x1+x2)/2;
    d=`M${x1},${y1} C${m},${y1} ${m},${y2} ${x2},${y2}`; }
  const p=document.createElementNS('http://www.w3.org/2000/svg','path'); p.setAttribute('d',d);
  p.setAttribute('stroke', kind==='alt'?'#ad59e9':'#5cfcfa'); if(kind==='alt') p.setAttribute('stroke-dasharray','7 6');
  p.setAttribute('marker-end', kind==='alt'?'url(#av)':'url(#a)'); ov.appendChild(p);
 }
 // contrôle de débordement, lu par render.js
 const bad=[]; const ar=document.querySelector('.area').getBoundingClientRect();
 const ttl=document.querySelector('.ttl'); if (ttl.scrollWidth>ttl.clientWidth+1) bad.push('title too long');
 document.querySelectorAll('.area *').forEach(e=>{const r=e.getBoundingClientRect(); if(!r.width) return;
   if(r.right>ar.right+12||r.bottom>ar.bottom+14||r.left<ar.left-12||r.top<ar.top-14) bad.push('outside: '+(e.className||e.tagName)+' '+(e.textContent||'').slice(0,30));
   if((e.classList.contains('card'))&&(e.scrollHeight>e.clientHeight+2||e.scrollWidth>e.clientWidth+2)) bad.push('card overflow: '+e.textContent.slice(0,30)); });
 document.body.dataset.overflow = [...new Set(bad)].slice(0,5).join(' | ');
 document.body.dataset.ready='1';
});
'''


def page(prefix, n, title, spec, handle):
    typ = spec['type']
    fn = LAYOUTS[typ]
    inner, links = (fn(spec, n) if typ == 'network' else fn(spec))
    ts = 44 if len(title) <= 40 else 40 if len(title) <= 45 else 36
    return f'''<!doctype html><html><head><meta charset="utf-8"><!-- auto-diagram -->
<link href="https://fonts.googleapis.com/css2?family=Exo+2:wght@600;700;800&family=Inter:wght@400;600&family=JetBrains+Mono:wght@400;700&display=block" rel="stylesheet">
<style>{CSS}</style></head><body><div class="page">
{bg_svg(n, typ in ('tree', 'network'))}
<div class="pre">{E(prefix.upper())} #{n}</div>
<div class="ttl" style="font-size:{ts}px">{E(title)}</div>
<div class="area">{inner}</div>
<svg class="ov" width="1200" height="675" style="pointer-events:none"><defs><filter id="glowL" filterUnits="userSpaceOnUse" x="0" y="0" width="1200" height="675"><feGaussianBlur stdDeviation="5" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter></defs><g id="links" fill="none" stroke-width="2.5" filter="url(#glowL)"></g></svg>
<div class="sig">{E(handle)}</div>
</div><script>{JS % json.dumps(links)}</script></body></html>'''


def check_unique(vis):
    """Règle : jamais deux fois la même illustration dans une campagne.
    Erreurs = schémas identiques (hors titre) ; avertissements = textes presque identiques
    ou même type 3 posts de suite."""
    def texts(v):
        if isinstance(v, dict): return [x for k, w in v.items() if k not in ('title', 'seed') for x in texts(w)]
        if isinstance(v, list): return [x for w in v for x in texts(w)]
        return [str(v).strip().lower()] if isinstance(v, str) else []
    errs, warns = [], []
    keys = sorted(vis, key=int)
    canon = {k: json.dumps({a: b for a, b in vis[k].items() if a not in ('title', 'seed')}, sort_keys=True, ensure_ascii=False) for k in keys}
    sets = {k: set(texts(vis[k])) for k in keys}
    for i, a in enumerate(keys):
        for b in keys[i + 1:]:
            if canon[a] == canon[b]:
                errs.append(f'posts {a} et {b} : illustration identique')
            elif vis[a]['type'] == vis[b]['type'] and sets[a] and sets[b]:
                j = len(sets[a] & sets[b]) / len(sets[a] | sets[b])
                if j >= .6: warns.append(f'posts {a} et {b} : même type ({vis[a]["type"]}) et {j:.0%} des textes en commun')
    for i in range(len(keys) - 2):
        if vis[keys[i]]['type'] == vis[keys[i + 1]]['type'] == vis[keys[i + 2]]['type']:
            warns.append(f'posts {keys[i]}–{keys[i + 2]} : trois « {vis[keys[i]]["type"]} » de suite, varier')
    return errs, warns


def main():
    camp = sys.argv[1].rstrip('/\\')
    args = [a for a in sys.argv[2:] if not a.startswith('--')]
    name = re.sub(r'-\d{3}$', '', os.path.basename(camp))
    account = os.path.dirname(os.path.dirname(os.path.abspath(camp)))
    conf = json.load(open(os.path.join(account, 'card.json'), encoding='utf-8'))
    posts = {p['index']: p for p in json.load(open(os.path.join(camp, 'posts.json'), encoding='utf-8'))}
    vis = json.load(open(os.path.join(camp, 'visuals.json'), encoding='utf-8'))
    errs, warns = check_unique(vis)
    for w in warns: print('AVERTISSEMENT ' + w)
    if errs:
        for e in errs: print('ERREUR ' + e)
        sys.exit('Illustrations répétées dans la campagne : rien n\'a été généré. Chaque post doit avoir son propre schéma.')
    os.makedirs(os.path.join(camp, 'src'), exist_ok=True)
    want = set(int(a) for a in args)
    out = []
    for k, spec in vis.items():
        n = int(k)
        if want and n not in want: continue
        if n not in posts: continue
        path = os.path.join(camp, 'src', f'post_{n}.html')
        if os.path.exists(path):
            cur = open(path, encoding='utf-8').read()
            if 'auto-card' not in cur and 'auto-diagram' not in cur: continue  # fait main
        title = spec.get('title') or posts[n].get('card', {}).get('title', '')
        open(path, 'w', encoding='utf-8').write(page(conf['prefixes'][name], n, title, spec, conf['handle']))
        out.append(path)
    if '--png' in sys.argv and out:
        here = os.path.dirname(os.path.abspath(__file__))
        subprocess.run(['node', os.path.join(here, 'render.js')] + out, check=True)
    print('\n'.join(out))


if __name__ == '__main__':
    main()
