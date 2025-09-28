// site.js — XIROX global helpers
(function () {
    'use strict';

    // 1) Fix 100vh on mobile (sets CSS var --vh)
    function setVH() {
        var vh = (window.visualViewport ? window.visualViewport.height : window.innerHeight) * 0.01;
        document.documentElement.style.setProperty('--vh', vh + 'px');
    }
    setVH();
    window.addEventListener('resize', setVH);
    window.addEventListener('orientationchange', setVH);

    // 2) Show focus outlines only when using keyboard (Tab)
    function handleFirstTab(e) {
        if (e.key === 'Tab') {
            document.body.classList.add('user-tabbing');
            window.removeEventListener('keydown', handleFirstTab);
            window.addEventListener('mousedown', handleMouseDownOnce);
            window.addEventListener('touchstart', handleMouseDownOnce, { passive: true });
        }
    }
    function handleMouseDownOnce() {
        document.body.classList.remove('user-tabbing');
        window.removeEventListener('mousedown', handleMouseDownOnce);
        window.removeEventListener('touchstart', handleMouseDownOnce);
        window.addEventListener('keydown', handleFirstTab);
    }
    window.addEventListener('keydown', handleFirstTab);

    // 3) Reveal on scroll (matches .reveal CSS)
    var observer = (('IntersectionObserver' in window) ?
        new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('reveal-in');
                    obs.unobserve(entry.target);
                }
            });
        }, { rootMargin: '0px 0px -10% 0px', threshold: 0.05 })
        : null);

    document.addEventListener('DOMContentLoaded', function () {
        var els = document.querySelectorAll('.reveal');
        for (var i = 0; i < els.length; i++) {
            if (observer) observer.observe(els[i]);
            else els[i].classList.add('reveal-in');
        }
    });

    // 4) Smooth scroll for in-page anchors (optional)
    document.addEventListener('click', function (e) {
        var a = e.target.closest('a[href^="#"]');
        if (!a) return;
        var id = a.getAttribute('href');
        if (id.length > 1) {
            var target = document.querySelector(id);
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
        }
    });

    // 5) Register service worker (PWA cache)
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function () {
            navigator.serviceWorker.register('/sw.js')
                .then(function (reg) {
                    // Optional: auto-update hourly
                    if (reg && reg.update) {
                        setInterval(function () { reg.update(); }, 60 * 60 * 1000);
                    }
                })
                .catch(function (err) {
                    console.error('SW register failed:', err);
                });
        });
    }

    // 6) Tiny helper: [data-copy] → copy to clipboard on click
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-copy]');
        if (!btn) return;
        var text = btn.getAttribute('data-copy') || '';
        if (!text) return;
        navigator.clipboard && navigator.clipboard.writeText(text)
            .then(function () { btn.classList.add('copied'); setTimeout(function () { btn.classList.remove('copied'); }, 1200); })
            .catch(function () { /* ignore */ });
    });

})();
