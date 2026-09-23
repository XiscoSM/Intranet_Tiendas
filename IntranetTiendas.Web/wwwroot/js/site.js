// Sustituye a Scripts_1.js del ASP clásico.

// Exportar la página actual a Excel: añade handler=Excel a la URL actual (Razor handler).
// Descarga directa (el archivo viene como attachment, no navega fuera de la página);
// evita el bloqueo de pop-ups y la pestaña en blanco que dejaba window.open.
function ExportPage() {
    var url = new URL(location.href);
    url.searchParams.set('handler', 'Excel');
    var a = document.createElement('a');
    a.href = url.toString();
    a.download = '';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

// Mostrar/ocultar un elemento por id (sustituye a MM_showHideLayers)
function toggle(id) {
    var el = document.getElementById(id);
    if (el) el.style.display = (el.style.display === 'none') ? '' : 'none';
}

// Validación de extensiones en subida de ficheros (heredada de upload2.asp)
function compruebaExtension(form, archivo) {
    var permitidas = ['.rtf', '.txt', '.jpg', '.doc', '.xls', '.pdf', '.docx', '.xlsx'];
    if (!archivo) { alert('No has seleccionado ningún archivo'); return false; }
    var ext = archivo.substring(archivo.lastIndexOf('.')).toLowerCase();
    if (permitidas.indexOf(ext) === -1) {
        alert('Comprueba la extensión del archivo a cargar.\nSólo se pueden subir: ' + permitidas.join());
        return false;
    }
    form.submit();
    return true;
}

// Enter on a field marked data-enter-next="<selector>" moves focus to that field
// instead of submitting the form (e.g. login: user -> password).
document.addEventListener('keydown', function (e) {
    if (e.key !== 'Enter') return;
    var sel = e.target && e.target.getAttribute && e.target.getAttribute('data-enter-next');
    if (!sel) return;
    var next = document.querySelector(sel);
    if (!next) return;
    e.preventDefault();
    next.focus();
});
