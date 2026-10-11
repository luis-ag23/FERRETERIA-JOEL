// FERRETERÍA JOEL — Historial de ventas.
// Modal de detalle (solo lectura) y anulación con motivo, ambos
// controlados desde la lista. Los datos del detalle provienen de la
// fachada vía handler JSON.

(function () {
    var fmt = new Intl.NumberFormat('es-BO', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    function dinero(n) {
        return fmt.format(n) + ' Bs';
    }

    function abrirModal(modal) {
        modal.classList.add('is-open');
        modal.setAttribute('aria-hidden', 'false');
        document.body.classList.add('fj-modal-open');
    }

    function cerrarModales() {
        document.querySelectorAll('.fj-modal.is-open').forEach(function (m) {
            m.classList.remove('is-open');
            m.setAttribute('aria-hidden', 'true');
        });
        document.body.classList.remove('fj-modal-open');
    }

    // ---------- Detalle (solo lectura) ----------

    var detalleModal = document.getElementById('ventasDetalleModal');
    var codigoEl = document.getElementById('ventasDetalleCodigo');
    var clienteEl = document.getElementById('ventasDetalleCliente');
    var fechaEl = document.getElementById('ventasDetalleFecha');
    var pagoEl = document.getElementById('ventasDetallePago');
    var lineasEl = document.getElementById('ventasDetalleLineas');
    var subtotalEl = document.getElementById('ventasDetalleSubtotal');
    var descuentoEl = document.getElementById('ventasDetalleDescuento');
    var totalEl = document.getElementById('ventasDetalleTotal');
    var motivoEl = document.getElementById('ventasDetalleMotivo');

    document.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-ventas-detalle]');
        if (!boton || !detalleModal) return;

        e.preventDefault();

        fetch('?handler=Detalle&idVenta=' + boton.getAttribute('data-id'), {
            headers: { 'Accept': 'application/json' }
        }).then(function (r) {
            return r.json();
        }).then(function (v) {
            if (!v || !v.lineas) return;

            codigoEl.textContent = v.codigoVenta;
            clienteEl.textContent = boton.getAttribute('data-cliente') || '';
            fechaEl.textContent = new Date(v.fecha).toLocaleString('es-BO');
            pagoEl.textContent = v.estadoPago + ' · ' + v.metodoPago;

            lineasEl.innerHTML = v.lineas.map(function (l) {
                return '<tr>' +
                    '<td><span class="fj-nombre">' + l.nombre + '</span>' +
                    '<span class="fj-nombre-sub">' + l.codigo + '</span></td>' +
                    '<td class="fj-td-cantidad">' + l.cantidad + '</td>' +
                    '<td class="fj-td-precio">' + dinero(l.precio) + '</td>' +
                    '<td class="fj-td-precio">' + dinero(l.total) + '</td></tr>';
            }).join('');

            subtotalEl.textContent = dinero(v.subtotal);
            descuentoEl.textContent = '- ' + dinero(v.descuento);
            totalEl.textContent = dinero(v.total);

            if (v.estadoVenta === 'Anulada' && v.motivoAnulacion) {
                motivoEl.hidden = false;
                motivoEl.textContent = 'Motivo de anulación: ' + v.motivoAnulacion;
            } else {
                motivoEl.hidden = true;
            }

            abrirModal(detalleModal);
        });
    });

    // ---------- Anulación con motivo ----------

    var anularModal = document.getElementById('ventasAnularModal');
    var anularBtn = document.getElementById('ventasAnularId');
    var anularContexto = document.getElementById('ventasAnularContexto');
    var anularMotivo = document.getElementById('ventasAnularMotivo');
    var anularError = document.getElementById('ventasAnularError');
    var anularForm = document.getElementById('ventasAnularForm');

    document.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-ventas-anular]');
        if (!boton || !anularModal) return;

        e.preventDefault();

        anularBtn.value = boton.getAttribute('data-id');
        anularContexto.textContent =
            '¿Anular la venta ' + boton.getAttribute('data-codigo') +
            '? El stock de los productos será restaurado.';
        anularMotivo.value = '';
        anularError.hidden = true;
        abrirModal(anularModal);
        anularMotivo.focus();
    });

    anularForm.addEventListener('submit', function (event) {
        if (!anularMotivo.value.trim()) {
            event.preventDefault();
            anularError.hidden = false;
            anularMotivo.focus();
            return;
        }
        cerrarModales();
    });

    // ---------- Cierre de modales ----------

    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('fj-modal-backdrop') ||
            e.target.closest('[data-fj-modal-close]')) {
            cerrarModales();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') cerrarModales();
    });
})();