// Usage : node design/render.js accounts/XCryptoKeys/threads/pactus-001/src/post_6.html [autres .html ...]
// Produit accounts/XCryptoKeys/threads/pactus-001/post_6.png (1200×675, exporté en x1).
// Pour les schémas de design/diagram.py, signale un texte qui déborde (« ATTENTION »).
const path = require('path');
const { chromium } = require('playwright');
(async () => {
  const b = await chromium.launch();
  const p = await b.newPage({ viewport: { width: 1200, height: 675 }, deviceScaleFactor: 1 });
  let warn = 0;
  for (const arg of process.argv.slice(2)) {
    const src = path.resolve(arg);
    const out = path.join(path.dirname(path.dirname(src)), path.basename(src).replace(/\.html$/, '.png'));
    await p.goto('file://' + src, { waitUntil: 'networkidle' });
    await p.evaluate(() => document.fonts.ready);
    const auto = await p.evaluate(() => document.documentElement.innerHTML.includes('auto-diagram'));
    if (auto) {
      await p.waitForFunction(() => document.body.dataset.ready === '1', null, { timeout: 10000 });
      const o = await p.evaluate(() => document.body.dataset.overflow);
      if (o) { warn++; console.log('ATTENTION ' + out + ' : ' + o); }
    }
    await p.screenshot({ path: out });
    console.log(out);
  }
  await b.close();
  if (warn) console.log(warn + ' image(s) à revoir');
})();
