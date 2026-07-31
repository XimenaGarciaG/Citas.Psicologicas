/**
 * UTTT – Inicialización global de DataTables
 */

document.addEventListener('DOMContentLoaded', function () {
  // Configuración en español
  const langES = {
    decimal: ',',
    emptyTable: 'No hay datos disponibles',
    info: 'Mostrando _START_ a _END_ de _TOTAL_ registros',
    infoEmpty: 'Mostrando 0 a 0 de 0 registros',
    infoFiltered: '(filtrado de _MAX_ registros totales)',
    lengthMenu: 'Mostrar _MENU_ registros',
    loadingRecords: 'Cargando...',
    processing: 'Procesando...',
    search: 'Buscar:',
    zeroRecords: 'No se encontraron resultados',
    paginate: { first: '«', last: '»', next: '›', previous: '‹' }
  };

  // Inicializar todas las tablas con clase .datatable
  document.querySelectorAll('table.datatable').forEach(table => {
    if ($.fn.DataTable.isDataTable(table)) return;

    const options = {
      language: langES,
      pageLength: parseInt(table.dataset.pageLength || '10'),
      order: JSON.parse(table.dataset.order || '[[0,"asc"]]'),
      responsive: true,
      dom: '<"row"<"col-md-6"l><"col-md-6"f>>rt<"row"<"col-md-6"i><"col-md-6"p>>',
      columnDefs: [{
        targets: table.dataset.noSort ? JSON.parse(table.dataset.noSort) : [-1],
        orderable: false
      }]
    };

    $(table).DataTable(options);
  });
});
