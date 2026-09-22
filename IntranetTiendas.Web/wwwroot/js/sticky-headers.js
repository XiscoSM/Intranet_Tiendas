// Sticky column headers for data tables, Excel "freeze panes" style.
//
// A header group is one or more consecutive rows that stick together below the
// site header while their table is on screen:
//   - every <tr class="CabeceraTablas"> (the legacy column-title row) or <thead> row;
//   - plus the rows immediately above it that are also headers: all-<th> rows
//     (e.g. the sort row in InventariosLinDetallado2) or rows whose non-empty
//     cells hold a direct <div class="CabeceraTablas"> (group titles such as
//     "Pedido / Enviado" in PedidosCentralLinPreparacion).
// Tables that repeat the title row per section (MovFechaProgLin) get one group
// per section: each new group slides over the previous one as you scroll.
// Positioning is pure CSS (position: sticky); this script only marks the rows
// and keeps the vertical offsets in sync with the header height.
(function () {
    'use strict';

    var groups = [];

    function isTitleRow(tr) {
        return tr.classList.contains('CabeceraTablas') ||
            (tr.parentElement && tr.parentElement.tagName === 'THEAD');
    }

    // A row just above a title row that also belongs to the header.
    function isLeadingHeaderRow(tr) {
        var cells = tr.cells, meaningful = 0;
        if (!cells.length) return false;
        var allTh = true;
        for (var i = 0; i < cells.length; i++) {
            if (cells[i].tagName !== 'TH') allTh = false;
        }
        if (allTh) return true;
        for (var j = 0; j < cells.length; j++) {
            var c = cells[j];
            if (c.textContent.trim() === '') continue;
            if (!c.querySelector(':scope > div.CabeceraTablas')) return false;
            meaningful++;
        }
        return meaningful > 0;
    }

    function collectGroups() {
        var tables = document.querySelectorAll('table');
        for (var t = 0; t < tables.length; t++) {
            var rows = tables[t].rows; // own rows only, never nested tables
            var taken = [];
            for (var r = 0; r < rows.length; r++) {
                if (!isTitleRow(rows[r]) || taken[r]) continue;
                var start = r;
                while (start > 0 && !taken[start - 1] && isLeadingHeaderRow(rows[start - 1])) start--;
                var end = r;
                while (end + 1 < rows.length && isTitleRow(rows[end + 1])) end++;
                var group = [];
                for (var k = start; k <= end; k++) {
                    rows[k].classList.add('sticky-head');
                    taken[k] = true;
                    group.push(rows[k]);
                }
                groups.push(group);
                r = end;
            }
        }
    }

    function layout() {
        var header = document.querySelector('header.site-header');
        var top = header ? header.getBoundingClientRect().height : 0;
        document.documentElement.style.setProperty('--sticky-top', top + 'px');
        for (var g = 0; g < groups.length; g++) {
            var offset = 0;
            for (var i = 0; i < groups[g].length; i++) {
                groups[g][i].style.setProperty('--row-offset', offset + 'px');
                offset += groups[g][i].getBoundingClientRect().height;
            }
        }
    }

    function init() {
        collectGroups();
        if (!groups.length) return;
        layout();
        var pending = false;
        var schedule = function () {
            if (pending) return;
            pending = true;
            requestAnimationFrame(function () { pending = false; layout(); });
        };
        window.addEventListener('resize', schedule);
        var header = document.querySelector('header.site-header');
        if (header && window.ResizeObserver) new ResizeObserver(schedule).observe(header);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
