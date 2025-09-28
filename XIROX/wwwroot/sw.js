/* sw.js — cache only same-origin assets */
const CACHE = "xirox-v2"; // نسخه جدید برای بی‌اثر کردن کش قدیمی
const ASSETS = [
    "/css/site.css",
    "/js/site.js",
    "/js/particles-config.js",
    "/js/particles.fixed.js"
];

self.addEventListener("install", e => {
    e.waitUntil(caches.open(CACHE).then(c => c.addAll(ASSETS)));
});

self.addEventListener("activate", e => {
    e.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k !== CACHE).map(k => caches.delete(k)))
        )
    );
});

// فقط درخواست‌های same-origin را کش/هندل کن
self.addEventListener("fetch", e => {
    const req = e.request;
    const url = new URL(req.url);
    const sameOrigin = url.origin === self.location.origin;

    // اگر هم‌ریشه نیست، اجازه بده مرورگر خودش مدیریت کند (ضروری برای CSS/JS از CDN)
    if (!sameOrigin) return;

    // استاتیک‌ها: cache-first
    if (req.url.match(/\.(css|js|woff2?|ttf|png|jpg|jpeg|webp|avif|svg)$/i)) {
        e.respondWith(
            caches.match(req).then(res => res || fetch(req).then(r => {
                const copy = r.clone();
                caches.open(CACHE).then(c => c.put(req, copy));
                return r;
            }))
        );
        return;
    }

    // HTML: network-first
    if (req.headers.get("accept")?.includes("text/html")) {
        e.respondWith(
            fetch(req).then(r => {
                const copy = r.clone();
                caches.open(CACHE).then(c => c.put(req, copy));
                return r;
            }).catch(() => caches.match(req))
        );
    }
});
