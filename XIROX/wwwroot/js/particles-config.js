/* =========================================================
   particles-config.js — Smart initialization for particles.js
   - Single init guard on #particles-js
   - Mobile + prefers-reduced-motion aware
   - Adaptive counts/speeds for smooth performance
   ========================================================= */
(function () {
    'use strict';

    if (window.__xiroxParticlesInit) return;
    window.__xiroxParticlesInit = true;

    var containerId = 'particles-js';
    var container = document.getElementById(containerId);
    if (!container) { window.__xiroxParticlesInit = false; return; }

    if (typeof window.particlesJS !== 'function') {
        var retry = 0;
        (function waitForLib() {
            if (typeof window.particlesJS === 'function') { init(); }
            else if (retry++ < 20) { setTimeout(waitForLib, 50); }
            else { window.__xiroxParticlesInit = false; }
        })();
        return;
    }

    init();

    function init() {
        var reduced = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        var vw = Math.max(document.documentElement.clientWidth, window.innerWidth || 0);
        var isMobile = vw <= 600;
        var dpr = Math.min(2, Math.max(1, window.devicePixelRatio || 1));

        var baseCount = isMobile ? 35 : 60;
        var scaledCount = Math.round(baseCount * (dpr > 1 ? 0.9 : 1));
        var baseSpeed = reduced ? 1.2 : (isMobile ? 1.7 : 2.1);

        var config = {
            particles: {
                number: { value: scaledCount, density: { enable: true, value_area: 900 } },
                color: { value: '#ff2323' },
                shape: {
                    type: 'circle',
                    stroke: { width: 1, color: '#ff2323' },
                    polygon: { nb_sides: 5 }
                },
                opacity: {
                    value: reduced ? 0.4 : 0.64,
                    random: true,
                    anim: { enable: !reduced, speed: 0.5, opacity_min: 0.2, sync: false }
                },
                size: {
                    value: isMobile ? 3 : 4,
                    random: true,
                    anim: { enable: !reduced, speed: 3, size_min: 1, sync: false }
                },
                line_linked: {
                    enable: !reduced,
                    distance: 140,
                    color: '#ff2323',
                    opacity: 0.33,
                    width: 1.1
                },
                move: {
                    enable: true,
                    speed: baseSpeed,
                    direction: 'none',
                    random: false,
                    straight: false,
                    out_mode: 'out',
                    bounce: false,
                    attract: { enable: false }
                }
            },
            interactivity: {
                detect_on: 'canvas',
                events: {
                    onhover: { enable: !reduced, mode: 'grab' },
                    onclick: { enable: !reduced, mode: 'push' },
                    resize: true
                },
                modes: {
                    grab: { distance: 200, line_linked: { opacity: 0.75 } },
                    bubble: { distance: 200, size: 8, duration: 2, opacity: 0.8 },
                    repulse: { distance: 150, duration: 0.4 },
                    push: { particles_nb: isMobile ? 2 : 4 },
                    remove: { particles_nb: 2 }
                }
            },
            retina_detect: true
        };

        window.particlesJS(containerId, config);

        // React to motion preference changes
        if (window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').addEventListener) {
            window.matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change', function () {
                try { destroyParticles(containerId); window.__xiroxParticlesInit = false; init(); } catch (_) { }
            });
        }

        // Re-init lightly on strong resize (e.g., orientation change)
        var resizeTimer;
        window.addEventListener('resize', function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                try { destroyParticles(containerId); window.__xiroxParticlesInit = false; init(); } catch (_) { }
            }, 400);
        });
    }

    function destroyParticles(id) {
        if (!window.pJSDom || !window.pJSDom.length) return;
        for (var i = window.pJSDom.length - 1; i >= 0; i--) {
            var item = window.pJSDom[i];
            if (item && item.pJS && item.pJS.canvas && item.pJS.canvas.el && item.pJS.canvas.el.id === id) {
                try { item.pJS.fn.vendors.destroypJS(); } catch (_) { }
                window.pJSDom.splice(i, 1);
            }
        }
        var canvas = document.querySelector('#' + id + ' > canvas');
        if (canvas && canvas.parentNode) canvas.parentNode.removeChild(canvas);
    }
})();
