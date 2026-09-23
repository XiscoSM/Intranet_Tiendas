// Teclado virtual en pantalla para terminales táctiles.
// - Añade un icono de teclado dentro de cada input de texto/número.
// - Al pulsarlo abre un teclado acoplado abajo, enlazado a ese campo.
// - Modo numérico por defecto (la intranet trabaja con códigos: EAN, producto,
//   gama, cantidades, usuario/contraseña) con conmutador a ABC.
// Sin dependencias externas.
(function () {
    'use strict';

    var SEL = 'input[type="text"], input[type="password"], input[type="search"], input[type="tel"], input[type="number"], input:not([type])';

    var ICON = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' +
        '<rect x="2" y="6" width="20" height="12" rx="2"/>' +
        '<path d="M6 10h0M10 10h0M14 10h0M18 10h0M8 14h8"/></svg>';

    var NUM = [['1', '2', '3'], ['4', '5', '6'], ['7', '8', '9'], ['.', '0', '⌫']];
    var ABC = [
        ['1', '2', '3', '4', '5', '6', '7', '8', '9', '0'],
        ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'],
        ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'ñ'],
        ['⇧', 'z', 'x', 'c', 'v', 'b', 'n', 'm', '⌫']
    ];

    var panel, keysEl, titleEl, current = null, layout = 'num', caps = false;

    function build() {
        panel = document.createElement('div');
        panel.className = 'vk-panel';
        panel.innerHTML =
            '<div class="vk-head">' +
                '<div class="vk-modes">' +
                    '<button type="button" class="vk-mode" data-mode="num">123</button>' +
                    '<button type="button" class="vk-mode" data-mode="abc">ABC</button>' +
                '</div>' +
                '<span class="vk-title"></span>' +
                '<button type="button" class="vk-close" title="Cerrar">✕</button>' +
            '</div>' +
            '<div class="vk-keys"></div>' +
            '<div class="vk-foot">' +
                '<button type="button" class="vk-k vk-wide vk-space" data-k=" ">espacio</button>' +
                '<button type="button" class="vk-k vk-clear">Borrar</button>' +
                '<button type="button" class="vk-k vk-enter">Intro</button>' +
            '</div>';
        keysEl = panel.querySelector('.vk-keys');
        titleEl = panel.querySelector('.vk-title');
        document.body.appendChild(panel);
        // No robar el foco del input al pulsar teclas
        panel.addEventListener('mousedown', function (e) { e.preventDefault(); });
        panel.addEventListener('click', onClick);
        document.addEventListener('click', function (e) {
            if (!panel.classList.contains('open')) return;
            if (e.target.closest('.vk-panel') || e.target.closest('.vk-btn')) return;
            close();
        });
    }

    function render() {
        var rows = layout === 'num' ? NUM : ABC;
        panel.className = 'vk-panel open vk-' + layout;
        keysEl.innerHTML = rows.map(function (r) {
            return '<div class="vk-row">' + r.map(keyHtml).join('') + '</div>';
        }).join('');
        var modes = panel.querySelectorAll('.vk-mode');
        for (var i = 0; i < modes.length; i++) {
            modes[i].classList.toggle('on', modes[i].getAttribute('data-mode') === layout);
        }
    }

    function keyHtml(k) {
        if (k === '⌫') return '<button type="button" class="vk-k vk-act" data-act="bs">⌫</button>';
        if (k === '⇧') return '<button type="button" class="vk-k vk-act vk-shift' + (caps ? ' on' : '') + '" data-act="shift">⇧</button>';
        var ch = (layout === 'abc' && caps) ? k.toUpperCase() : k;
        return '<button type="button" class="vk-k" data-k="' + ch + '">' + ch + '</button>';
    }

    function onClick(e) {
        var b = e.target.closest('button');
        if (!b) return;
        if (b.classList.contains('vk-close')) { close(); return; }
        if (b.classList.contains('vk-mode')) { layout = b.getAttribute('data-mode'); render(); return; }
        if (b.getAttribute('data-act') === 'shift') { caps = !caps; render(); return; }
        if (b.getAttribute('data-act') === 'bs') { backspace(); return; }
        if (b.classList.contains('vk-clear')) { setValue('', 0); return; }
        if (b.classList.contains('vk-enter')) { enter(); return; }
        var k = b.getAttribute('data-k');
        if (k !== null) insert(k);
    }

    function caret() {
        var s = current.selectionStart, e = current.selectionEnd;
        if (s === null || s === undefined) { s = e = current.value.length; }
        return [s, e];
    }
    function focusAt(pos) {
        try { current.focus({ preventScroll: true }); current.setSelectionRange(pos, pos); } catch (_) {}
    }
    function fire() { current.dispatchEvent(new Event('input', { bubbles: true })); }

    function insert(ch) {
        if (!current) return;
        var c = caret(), s = c[0], e = c[1];
        var v = current.value.slice(0, s) + ch + current.value.slice(e);
        var max = current.maxLength;
        if (max && max > 0 && v.length > max) return;
        current.value = v;
        focusAt(s + ch.length);
        fire();
    }
    function backspace() {
        if (!current) return;
        var c = caret(), s = c[0], e = c[1];
        if (s === e && s > 0) { current.value = current.value.slice(0, s - 1) + current.value.slice(e); s--; }
        else { current.value = current.value.slice(0, s) + current.value.slice(e); }
        focusAt(s);
        fire();
    }
    function setValue(v, pos) {
        if (!current) return;
        current.value = v;
        focusAt(pos);
        fire();
    }
    function enter() {
        if (!current) return;
        // Behave like a physical Enter: if a page handler cancels it (e.g. moving
        // from user to password), follow the focus and keep the keyboard open there.
        var ev = new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', bubbles: true, cancelable: true });
        if (!current.dispatchEvent(ev)) {
            var next = document.activeElement;
            if (next && next !== current && next.closest && next.closest('.vk-field')) open(next);
            else close();
            return;
        }
        var form = current.form;
        close();
        if (form) { if (form.requestSubmit) form.requestSubmit(); else form.submit(); }
    }

    function open(input) {
        if (!panel) build();
        current = input;
        layout = isText(input) ? 'abc' : 'num';
        caps = false;
        titleEl.textContent = labelOf(input);
        render();
        try { input.focus({ preventScroll: true }); } catch (_) {}
    }
    function close() {
        if (panel) panel.classList.remove('open');
        current = null;
    }

    function isText(i) {
        var im = (i.getAttribute('inputmode') || '').toLowerCase();
        return im === 'text' || i.getAttribute('data-vk') === 'abc';
    }
    function labelOf(i) {
        // Associated <label>, then an enclosing one, then a <label> right before the field.
        var prev = (i.closest('.vk-field') || i).previousElementSibling;
        var lab = (i.id && document.querySelector('label[for="' + i.id + '"]')) || i.closest('label') ||
            (prev && prev.tagName === 'LABEL' ? prev : null);
        var t = lab ? lab.textContent : (i.getAttribute('placeholder') || i.name || '');
        return (t || '').replace(/\s+/g, ' ').trim().slice(0, 40);
    }

    function enhance(input) {
        if (input.dataset.vk === 'off' || input.readOnly || input.disabled) return;
        if (input.closest('.vk-field')) return;
        var wrap = document.createElement('span');
        wrap.className = 'vk-field' + (input.closest('.login-box') ? ' vk-field--block' : '');
        input.parentNode.insertBefore(wrap, input);
        // The wrapper takes over the input's margins so its height matches the
        // input box and the icon stays vertically centred on it.
        var cs = getComputedStyle(input);
        wrap.style.margin = cs.marginTop + ' ' + cs.marginRight + ' ' + cs.marginBottom + ' ' + cs.marginLeft;
        input.style.margin = '0';
        wrap.appendChild(input);
        input.classList.add('vk-on');
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'vk-btn';
        btn.tabIndex = -1;
        btn.setAttribute('aria-label', 'Teclado en pantalla');
        btn.innerHTML = ICON;
        btn.addEventListener('click', function () {
            if (panel && panel.classList.contains('open') && current === input) close();
            else open(input);
        });
        wrap.appendChild(btn);
    }

    function init() {
        var list = document.querySelectorAll(SEL);
        for (var i = 0; i < list.length; i++) enhance(list[i]);
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
})();
