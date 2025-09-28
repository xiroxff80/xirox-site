document.addEventListener("DOMContentLoaded", function () {
    particlesJS("particles-js", {
        particles: {
            number: {
                value: 60,
                density: { enable: true, value_area: 800 }
            },
            color: {
                value: "#ff0000"
            },
            shape: {
                type: "circle",
                stroke: {
                    width: 1,
                    color: "#ff0000"
                },
                polygon: {
                    nb_sides: 5
                },
                image: {
                    src: "",
                    width: 100,
                    height: 100
                }
            },
            opacity: {
                value: 0.6,
                random: true,
                anim: { enable: true, speed: 0.5, opacity_min: 0.2, sync: false }
            },
            size: {
                value: 4,
                random: true,
                anim: { enable: true, speed: 3, size_min: 1, sync: false }
            },
            line_linked: {
                enable: true,
                distance: 150,
                color: "#ff0000",
                opacity: 0.4,
                width: 1
            },
            move: {
                enable: true,
                speed: 3,
                direction: "none",
                random: false,
                straight: false,
                out_mode: "out",
                bounce: false,
                attract: { enable: false }
            }
        },
        interactivity: {
            detect_on: "canvas",
            events: {
                onhover: { enable: true, mode: "grab" },
                onclick: { enable: true, mode: "push" },
                resize: true
            },
            modes: {
                grab: { distance: 140, line_linked: { opacity: 1 } },
                bubble: { distance: 200, size: 8, duration: 2, opacity: 0.8 },
                repulse: { distance: 100, duration: 0.4 },
                push: { particles_nb: 4 },
                remove: { particles_nb: 2 }
            }
        },
        retina_detect: true
    });
});
