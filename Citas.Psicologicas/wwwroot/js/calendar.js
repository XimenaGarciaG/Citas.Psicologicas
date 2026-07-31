/**
 * UTTT – Calendario de Citas (FullCalendar)
 */

function initCalendar(citasJson) {
  const calendarEl = document.getElementById('calendar');
  if (!calendarEl || typeof FullCalendar === 'undefined') return;

  const citas = JSON.parse(citasJson || '[]');

  const events = citas.map(c => ({
    id: c.id,
    title: `${c.horaInicio} – ${c.nombreEstudiante}`,
    start: `${c.fecha.split('T')[0]}T${c.horaInicio}`,
    end:   `${c.fecha.split('T')[0]}T${c.horaFin}`,
    color: c.colorCalendario || '#2563EB',
    extendedProps: c
  }));

  const calendar = new FullCalendar.Calendar(calendarEl, {
    initialView: 'dayGridMonth',
    locale: 'es',
    headerToolbar: {
      left:   'prev,next today',
      center: 'title',
      right:  'dayGridMonth,timeGridWeek,timeGridDay'
    },
    buttonText: { today: 'Hoy', month: 'Mes', week: 'Semana', day: 'Día' },
    events,
    eventClick: function (info) {
      const props = info.event.extendedProps;
      showCitaModal(props);
    },
    eventDidMount: function (info) {
      const tooltip = new bootstrap.Tooltip(info.el, {
        title: `${info.event.title} | ${info.event.extendedProps.estado || ''}`,
        placement: 'top',
        trigger: 'hover'
      });
    },
    dayCellClassNames: function (arg) {
      return arg.isToday ? ['fc-today-highlight'] : [];
    },
    height: 'auto',
    businessHours: {
      daysOfWeek: [1, 2, 3, 4, 5],
      startTime: '08:00',
      endTime:   '18:00'
    },
    firstDay: 1,
    nowIndicator: true
  });

  calendar.render();
  return calendar;
}

function showCitaModal(cita) {
  const estadoBadge = {
    'RESERVADA':  'badge-reservada',
    'CONFIRMADA': 'badge-confirmada',
    'CANCELADA':  'badge-cancelada',
    'CONCLUIDA':  'badge-concluida',
    'REAGENDADA': 'badge-reagendada'
  };

  const badgeClass = estadoBadge[cita.estado?.toUpperCase()] || 'badge-secondary';

  document.getElementById('modal-cita-estudiante').textContent = cita.nombreEstudiante || '-';
  document.getElementById('modal-cita-psicologo').textContent  = cita.nombrePsicologo  || '-';
  document.getElementById('modal-cita-fecha').textContent      = new Date(cita.fecha).toLocaleDateString('es-MX', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  document.getElementById('modal-cita-hora').textContent       = `${cita.horaInicio} – ${cita.horaFin}`;
  document.getElementById('modal-cita-estado').innerHTML       = `<span class="badge-pill ${badgeClass}">${cita.estado}</span>`;
  document.getElementById('modal-cita-link').href              = `/Citas/Details/${cita.id}`;

  const modal = new bootstrap.Modal(document.getElementById('modalCita'));
  modal.show();
}
