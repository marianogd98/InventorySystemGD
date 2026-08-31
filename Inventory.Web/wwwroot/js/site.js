// Inventory.Web/wwwroot/js/site.js

document.addEventListener('DOMContentLoaded', () => {
    const themeToggleBtn = document.getElementById('themeToggleBtn');
    const themeIcon = document.getElementById('themeIcon');
    const themeText = document.getElementById('themeText');

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);

        if (theme === 'dark') {
            if (themeIcon) {
                themeIcon.className = 'bi bi-sun-fill text-warning';
            }
            if (themeText) {
                themeText.textContent = 'Modo Claro';
            }
            if (themeToggleBtn) {
                themeToggleBtn.classList.remove('btn-outline-secondary');
                themeToggleBtn.classList.add('btn-outline-warning');
            }
        } else {
            if (themeIcon) {
                themeIcon.className = 'bi bi-moon-stars-fill text-primary';
            }
            if (themeText) {
                themeText.textContent = 'Modo Oscuro';
            }
            if (themeToggleBtn) {
                themeToggleBtn.classList.remove('btn-outline-warning');
                themeToggleBtn.classList.add('btn-outline-secondary');
            }
        }
    }

    // Inicializar el estado visual del botón según el tema actual
    const currentTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
    applyTheme(currentTheme);

    // Event listener para alternar el tema al hacer clic
    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', () => {
            const activeTheme = document.documentElement.getAttribute('data-bs-theme') || 'light';
            const nextTheme = activeTheme === 'dark' ? 'light' : 'dark';
            applyTheme(nextTheme);
        });
    }

    // Escuchar cambios de preferencia del sistema si el usuario no ha forzado una elección
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
            applyTheme(e.matches ? 'dark' : 'light');
        }
    });
});

