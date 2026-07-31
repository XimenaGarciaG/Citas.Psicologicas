/**
 * UTTT – Sistema de Citas Psicológicas
 * JavaScript Global
 */

// ─── Inicialización ─────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
  initSidebar();
  initTooltips();
  initToasts();
  initAutoHideAlerts();
  initTableSearch();
});

// ─── Sidebar ────────────────────────────────────────────────────────────────
function initSidebar() {
  const sidebar = document.getElementById('sidebar');
  const mainContent = document.getElementById('mainContent');
  const topbar = document.getElementById('topbar');
  const toggleBtn = document.getElementById('sidebarToggle');
  const mobileToggle = document.getElementById('mobileSidebarToggle');

  if (!sidebar) return;

  // Restaurar estado guardado
  const collapsed = localStorage.getItem('sidebar-collapsed') === 'true';
  if (collapsed) {
    sidebar.classList.add('collapsed');
    if (mainContent) mainContent.classList.add('sidebar-collapsed');
    if (topbar) topbar.classList.add('sidebar-collapsed');
  }

  // Toggle desktop
  if (toggleBtn) {
    toggleBtn.addEventListener('click', function () {
      sidebar.classList.toggle('collapsed');
      if (mainContent) mainContent.classList.toggle('sidebar-collapsed');
      if (topbar) topbar.classList.toggle('sidebar-collapsed');
      localStorage.setItem('sidebar-collapsed', sidebar.classList.contains('collapsed'));
    });
  }

  // Toggle mobile
  if (mobileToggle) {
    mobileToggle.addEventListener('click', function () {
      sidebar.classList.toggle('mobile-open');
    });
  }

  // Cerrar sidebar mobile al hacer click fuera
  document.addEventListener('click', function (e) {
    if (window.innerWidth <= 768 &&
        sidebar.classList.contains('mobile-open') &&
        !sidebar.contains(e.target) &&
        !mobileToggle?.contains(e.target)) {
      sidebar.classList.remove('mobile-open');
    }
  });

  // Marcar item activo
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-item-link').forEach(link => {
    const href = link.getAttribute('href')?.toLowerCase() || '';
    if (href && currentPath.startsWith(href) && href !== '/') {
      link.classList.add('active');
    }
  });
}

// ─── Tooltips Bootstrap ─────────────────────────────────────────────────────
function initTooltips() {
  const tooltipTriggers = document.querySelectorAll('[data-bs-toggle="tooltip"]');
  tooltipTriggers.forEach(el => new bootstrap.Tooltip(el));
}

// ─── Toast Notifications ─────────────────────────────────────────────────────
function initToasts() {
  document.querySelectorAll('.toast-auto').forEach(toastEl => {
    const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
    toast.show();
  });
}

function showToast(message, type = 'success') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const icons = { success: 'fa-check-circle', danger: 'fa-times-circle',
                  warning: 'fa-exclamation-triangle', info: 'fa-info-circle' };
  const icon = icons[type] || 'fa-info-circle';

  const id = 'toast-' + Date.now();
  const html = `
    <div id="${id}" class="toast align-items-center text-bg-${type} border-0 toast-auto" role="alert">
      <div class="d-flex">
        <div class="toast-body d-flex align-items-center gap-2">
          <i class="fa ${icon}"></i> ${message}
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>
    </div>`;

  container.insertAdjacentHTML('beforeend', html);
  const toastEl = document.getElementById(id);
  const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
  toast.show();
  toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}

// ─── Auto-hide Alerts ────────────────────────────────────────────────────────
function initAutoHideAlerts() {
  document.querySelectorAll('.alert-auto-hide').forEach(alert => {
    setTimeout(() => {
      alert.style.transition = 'opacity .5s';
      alert.style.opacity = '0';
      setTimeout(() => alert.remove(), 500);
    }, 4000);
  });
}

// ─── Table Search (client-side simple) ──────────────────────────────────────
function initTableSearch() {
  document.querySelectorAll('[data-search-table]').forEach(input => {
    const tableId = input.getAttribute('data-search-table');
    const table = document.getElementById(tableId);
    if (!table) return;

    input.addEventListener('input', function () {
      const query = this.value.toLowerCase();
      table.querySelectorAll('tbody tr').forEach(row => {
        row.style.display = row.textContent.toLowerCase().includes(query) ? '' : 'none';
      });
    });
  });
}

// ─── Confirm Delete (SweetAlert2) ───────────────────────────────────────────
function confirmDelete(formId, itemName) {
  Swal.fire({
    title: '¿Eliminar registro?',
    html: `¿Está seguro de que desea eliminar <strong>${itemName}</strong>? Esta acción no se puede deshacer.`,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#DC2626',
    cancelButtonColor: '#6B7280',
    confirmButtonText: '<i class="fa fa-trash me-1"></i> Sí, eliminar',
    cancelButtonText: 'Cancelar',
    reverseButtons: true
  }).then(result => {
    if (result.isConfirmed) document.getElementById(formId).submit();
  });
}

// ─── Confirm Action ──────────────────────────────────────────────────────────
function confirmAction(formId, title, text, icon, confirmText, confirmColor) {
  Swal.fire({
    title,
    text,
    icon: icon || 'question',
    showCancelButton: true,
    confirmButtonColor: confirmColor || '#2563EB',
    cancelButtonColor: '#6B7280',
    confirmButtonText: confirmText || 'Confirmar',
    cancelButtonText: 'Cancelar',
    reverseButtons: true
  }).then(result => {
    if (result.isConfirmed) document.getElementById(formId).submit();
  });
}

// ─── Toggle Password Visibility ─────────────────────────────────────────────
function togglePassword(inputId, btnId) {
  const input = document.getElementById(inputId);
  const btn = document.getElementById(btnId);
  if (!input || !btn) return;
  const isPassword = input.type === 'password';
  input.type = isPassword ? 'text' : 'password';
  btn.innerHTML = isPassword ? '<i class="fa fa-eye-slash"></i>' : '<i class="fa fa-eye"></i>';
}

// ─── Character Counter ───────────────────────────────────────────────────────
document.querySelectorAll('[data-max-length]').forEach(el => {
  const max = parseInt(el.getAttribute('data-max-length'));
  const counterId = el.getAttribute('data-counter');
  const counter = counterId ? document.getElementById(counterId) : null;
  if (!counter) return;

  el.addEventListener('input', () => {
    const len = el.value.length;
    counter.textContent = `${len}/${max}`;
    counter.style.color = len > max * 0.9 ? '#DC2626' : '#64748B';
  });
});

// ─── Loading Overlay ─────────────────────────────────────────────────────────
function showLoading(text = 'Cargando...') {
  const overlay = document.createElement('div');
  overlay.id = 'loading-overlay';
  overlay.style.cssText = `
    position:fixed;inset:0;background:rgba(15,23,42,.6);z-index:9999;
    display:flex;align-items:center;justify-content:center;backdrop-filter:blur(4px);`;
  overlay.innerHTML = `
    <div style="background:white;border-radius:16px;padding:32px 48px;text-align:center;box-shadow:0 20px 60px rgba(0,0,0,.3)">
      <div class="spinner-border text-primary mb-3" style="width:2.5rem;height:2.5rem"></div>
      <p style="margin:0;font-weight:600;color:#1E293B">${text}</p>
    </div>`;
  document.body.appendChild(overlay);
}

function hideLoading() {
  const overlay = document.getElementById('loading-overlay');
  if (overlay) overlay.remove();
}

// ─── Form Submit Loading ──────────────────────────────────────────────────────
document.querySelectorAll('form[data-loading]').forEach(form => {
  form.addEventListener('submit', () => showLoading(form.getAttribute('data-loading') || 'Procesando...'));
});
