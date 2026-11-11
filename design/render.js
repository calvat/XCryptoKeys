// Usage : node design/render.js accounts/XCryptoKeys/threads/pactus-001/src/post_6.html
// Produit accounts/XCryptoKeys/threads/pactus-001/post_6.png (1200×675, exporté en x1).
const path = require('path');
const { chromium } = require('playwright');
(async () => {
  const src = path.resolve(process.argv[2]);
  const out = path.join(path.dirname(path.dirname(src)), path.basename(src).replace(/\.html$/, '.png'));
  const b = await chromium.launch();
  const p = await b.newPage({ viewport: { width: 1200, height: 675 }, deviceScaleFactor: 1 });
  await p.goto('file://' + src, { waitUntil: 'networkidle' });
  await p.evaluate(() => document.fonts.ready);
  await p.screenshot({ path: out });
  await b.close();
  console.log(out);
})();
