#!/usr/bin/env python3
"""Carte simple pour un post, dans le style STYLE-GRAPHIQUE.md.

Usage : python3 design/card.py threads/pactus-001 [index ...]
Lit posts.json, prend le champ "card" de chaque post et écrit la source
threads/<campagne>/src/post_N.html. Avec --png, produit aussi l'image
threads/<campagne>/post_N.png (via design/render.js).
Un post qui a déjà un schéma sur mesure (src/post_N.html sans la marque
"auto-card") n'est pas écrasé, mais son PNG est régénéré.
"""
import json, sys, os, html, re, subprocess

PREFIX = {
    'pactus': 'Inside #Pactus', 'bitcoin': '#Bitcoin Internals', 'ethereum': '#Ethereum Deep Dive',
    'crypto-math': '#Crypto Math', 'hacks': 'Crypto #Hack Autopsy', 'consensus': '#Blockchain Consensus',
    'layer2': 'Crypto #Layer2 Unlocked', 'mev': 'Crypto #MEV Dark Forest', 'defi': '#DeFi Mechanics',
    'wallets': 'Crypto #Wallet Security', 'anonymity': 'Crypto #Anonymity Myths',
}

def wrap(text, width):
    words, lines, cur = text.split(), [], ''
    for w in words:
        if cur and len(cur) + 1 + len(w) > width:
            lines.append(cur); cur = w
        else:
            cur = (cur + ' ' + w).strip()
    if cur: lines.append(cur)
    return lines

def card_svg(prefix, n, card):
    e = html.escape
    title = wrap(card['title'], 22)
    sub = wrap(card.get('sub', ''), 44)
    big = card.get('big', '')
    y = 215
    parts = []
    if big:
        size = 120 if len(big) <= 6 else 92 if len(big) <= 9 else 70
        parts.append(f'<text class="t" x="60" y="{y+70}" font-size="{size}" font-weight="800" fill="url(#edgeT)" filter="url(#glow)">{e(big)}</text>')
        y += 70 + 75
    for l in title:
        parts.append(f'<text class="t" x="60" y="{y}" font-size="50" font-weight="800" fill="#eaf6ff">{e(l)}</text>'); y += 60
    y += 10
    for l in sub:
        parts.append(f'<text x="60" y="{y}" font-size="24" fill="#8fa8dd">{e(l)}</text>'); y += 34
    body = '\n'.join(parts)
    return f'''<!doctype html><html><head><meta charset="utf-8"><!-- auto-card -->
<link href="https://fonts.googleapis.com/css2?family=Exo+2:wght@600;800&display=block" rel="stylesheet">
<style>body{{margin:0;background:#040823}}.t{{font-family:'Exo 2',Inter,sans-serif}}.m{{font-family:'JetBrains Mono',monospace}}</style></head><body>
<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="675" viewBox="0 0 1200 675" font-family="Inter,sans-serif">
<defs>
 <radialGradient id="bg" cx="70%" cy="50%" r="85%"><stop offset="0" stop-color="#10235b"/><stop offset=".55" stop-color="#081039"/><stop offset="1" stop-color="#040823"/></radialGradient>
 <linearGradient id="edgeT" x1="0" x2="1"><stop offset="0" stop-color="#5cfcfa"/><stop offset="1" stop-color="#ad59e9"/></linearGradient>
 <linearGradient id="cubeA" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#5cfcfa" stop-opacity=".35"/><stop offset="1" stop-color="#2169af" stop-opacity=".15"/></linearGradient>
 <linearGradient id="cubeB" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#ad59e9" stop-opacity=".35"/><stop offset="1" stop-color="#4a7ce1" stop-opacity=".15"/></linearGradient>
 <filter id="glow" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="6" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
 <filter id="soft" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="60"/></filter>
 <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse"><path d="M40 0H0V40" fill="none" stroke="#2169af" stroke-opacity=".12"/></pattern>
 <g id="cube"><path d="M0,30 L52,0 L104,30 L104,92 L52,122 L0,92Z" stroke-width="2"/><path d="M0,30 L52,60 L104,30 M52,60 V122" fill="none" stroke-width="1.5" stroke-opacity=".7"/></g>
</defs>
<rect width="1200" height="675" fill="url(#bg)"/><rect width="1200" height="675" fill="url(#grid)"/>
<circle cx="930" cy="360" r="190" fill="#2169af" opacity=".35" filter="url(#soft)"/>
<circle cx="1080" cy="160" r="140" fill="#8565e4" opacity=".3" filter="url(#soft)"/>
<g fill="none" stroke="#46b6e9" stroke-opacity=".35" stroke-width="1.5"><path d="M1200 600H1100L1070 572H960"/><path d="M1200 130H1150L1120 100H1010"/><path d="M700 675V640L730 610H820"/></g>
<g fill="#5cfcfa" opacity=".6"><circle cx="960" cy="572" r="3"/><circle cx="1010" cy="100" r="3"/><circle cx="820" cy="610" r="3"/></g>
<g filter="url(#glow)">
 <path d="M790,395 L870,350 M950,305 L1020,265" stroke="#5cfcfa" stroke-width="3" stroke-dasharray="2 8" stroke-linecap="round"/>
 <use href="#cube" x="700" y="360" fill="url(#cubeA)" stroke="#5cfcfa"/>
 <use href="#cube" x="860" y="270" fill="url(#cubeB)" stroke="#ad59e9"/>
 <use href="#cube" x="1020" y="180" fill="url(#cubeA)" stroke="#46b6e9"/>
</g>
<g class="m" font-size="12" fill="#5cfcfa" opacity=".55" text-anchor="middle"><text x="726" y="420">0x{n:02x}</text><text x="886" y="330">1011</text><text x="1046" y="240">0x{n*7%256:02x}</text></g>
<text class="m" x="60" y="92" fill="#5cfcfa" font-size="22" font-weight="700" letter-spacing="2" filter="url(#glow)">{e(prefix.upper())} #{n}</text>
{body}
<text class="m" x="1140" y="640" fill="#488fdb" font-size="16" text-anchor="end">@xcryptokeys</text>
</svg></body></html>'''

def main():
    camp = sys.argv[1].rstrip('/')
    args = [a for a in sys.argv[2:] if a != '--png']
    name = re.sub(r'-\d{3}$', '', os.path.basename(camp))
    posts = json.load(open(os.path.join(camp, 'posts.json')))
    os.makedirs(os.path.join(camp, 'src'), exist_ok=True)
    want = set(int(a) for a in args)
    out = []
    for p in posts:
        if want and p['index'] not in want: continue
        if 'card' not in p: continue
        path = os.path.join(camp, 'src', f"post_{p['index']}.html")
        if not (os.path.exists(path) and 'auto-card' not in open(path).read()):
            open(path, 'w').write(card_svg(PREFIX[name], p['index'], p['card']))
        out.append(path)
    if '--png' in sys.argv:
        here = os.path.dirname(os.path.abspath(__file__))
        for path in out: subprocess.run(['node', os.path.join(here, 'render.js'), path], check=True)
    print('\n'.join(out))

if __name__ == '__main__':
    main()
