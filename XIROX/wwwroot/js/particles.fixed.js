/* =========================================================
   particles.fixed.js — Gentle pointer attractor for particles.js
   - Respects prefers-reduced-motion
   - Pauses when tab hidden
   - Adaptive power on low FPS
   - Safe start after particles.js ready
   ========================================================= */
(function () {
    'use strict';

    var prefersReduced = false;
    try {
        var mq = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)');
        prefersReduced = !!(mq && mq.matches);
        mq && mq.addEventListener && mq.addEventListener('change', function (e) {
            prefersReduced = e.matches;
        });
    } catch (_) { }

    if (prefersReduced) return;

    var running = true;              // stop when hidden
    var powerScale = 1;              // adaptive based on FPS
    var pointer = { x: null, y: null, active: false };

    function getAttractDistance() {
        var vw = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
        return vw <= 600 ? 120 : 160;
    }
    var ATTRACT_DISTANCE = getAttractDistance();
    var BASE_POWER = 0.07;

    window.addEventListener('resize', throttle(function () {
        ATTRACT_DISTANCE = getAttractDistance();
    }, 200));

    document.addEventListener('visibilitychange', function () {
        running = !document.hidden;
    });

    function setPointer(x, y, a) { pointer.x = x; pointer.y = y; pointer.active = a; }

    window.addEventListener('mousemove', function (e) {
        setPointer(e.clientX, e.clientY, true);
    }, { passive: true });

    window.addEventListener('mouseleave', function () {
        setPointer(null, null, false);
    });

    window.addEventListener('touchstart', function (e) {
        if (e.touches && e.touches[0]) {
            setPointer(e.touches[0].clientX, e.touches[0].clientY, true);
        }
    }, { passive: true });

    window.addEventListener('touchmove', function (e) {
        if (e.touches && e.touches[0]) {
            setPointer(e.touches[0].clientX, e.touches[0].clientY, true);
        }
    }, { passive: true });

    window.addEventListener('touchend', function () {
        setPointer(null, null, false);
    });

    var lastFrame = performance.now();
    var ema = 16.7; // exponential moving average for frame time (ms)
    function updatePerf(delta) {
        ema = 0.8 * ema + 0.2 * delta;
        if (ema > 50) powerScale = 0;         // extremely heavy → disable
        else if (ema > 32) powerScale = 0.5;  // somewhat heavy → reduce
        else powerScale = 1;                  // normal
    }

    function throttle(fn, wait) {
        var last = 0, timer = null;
        return function () {
            var now = Date.now();
            var remaining = wait - (now - last);
            var ctx = this, args = arguments;
            if (remaining <= 0) {
                clearTimeout(timer); timer = null; last = now;
                fn.apply(ctx, args);
            } else if (!timer) {
                timer = setTimeout(function () {
                    last = Date.now(); timer = null;
                    fn.apply(ctx, args);
                }, remaining);
            }
        };
    }

    function waitForParticles() {
        var tries = 0;
        (function loop() {
            if (window.pJSDom && window.pJSDom[0] && window.pJSDom[0].pJS) startTick();
            else if (tries++ < 120) setTimeout(loop, 50);
        })();
    }
    waitForParticles();

    function startTick() { requestAnimationFrame(tick); }

    function tick(now) {
        var delta = now - lastFrame;
        lastFrame = now;
        updatePerf(delta);

        if (running && pointer.active && powerScale > 0 && window.pJSDom && window.pJSDom[0]) {
            var dom = window.pJSDom[0];
            var particles = dom && dom.pJS && dom.pJS.particles && dom.pJS.particles.array;
            if (particles && particles.length) {
                var MAX_V = 2.5;
                var distLimit = ATTRACT_DISTANCE;
                var minDist = 7;

                for (var i = 0, n = particles.length; i < n; i++) {
                    var p = particles[i];
                    var dx = pointer.x - p.x;
                    var dy = pointer.y - p.y;
                    var dist = Math.hypot(dx, dy);

                    if (dist < distLimit && dist > minDist) {
                        var t = 1 - (dist - minDist) / (distLimit - minDist);
                        t = 1 - (1 - t) * (1 - t); // easeOutQuad
                        var f = BASE_POWER * powerScale * t;
                        var nx = dx / dist;
                        var ny = dy / dist;

                        p.vx += nx * f;
                        p.vy += ny * f;

                        if (p.vx > MAX_V) p.vx = MAX_V;
                        else if (p.vx < -MAX_V) p.vx = -MAX_V;

                        if (p.vy > MAX_V) p.vy = MAX_V;
                        else if (p.vy < -MAX_V) p.vy = -MAX_V;
                    }
                }
            }
        }
        requestAnimationFrame(tick);
    }
})();
