// Usage : node design/render.js threads/pactus-001/img/06.html
// Produit le PNG à côté du HTML (1200×675, exporté en x2 = 2400×1350).
const path = require('path');
const { chromium } = require('playwright');
(async () => {
  const src = path.resolve(process.argv[2]);
  const out = src.replace(/\.html$/, '.png');
  const b = await chromium.launch();
  const p = await b.newPage({ viewport: { width: 1200, height: 675 }, deviceScaleFactor: 2 });
  await p.goto('file://' + src, { waitUntil: 'networkidle' });
  await p.evaluate(() => document.fonts.ready);
  await p.screenshot({ path: out });
  await b.close();
  console.log(out);
})();
