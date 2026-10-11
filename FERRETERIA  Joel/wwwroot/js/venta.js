// FERRETERÍA JOEL — Pantalla de registro de venta.
// Gestiona los dos buscadores, la tabla temporal de líneas (M:N) y
// la previsualización de totales. La validación y el cálculo
// definitivos siempre ocurren en el servidor (FachadaVenta).

(function () {
    var app = document.getElementById('ventaApp');
    if (!app) return;

    var form = document.getElementById('ventaForm');
    var lineasBody = document.getElementById('lineasBody');
    var lineasVacio = document.getElementById('lineasVacio');
    var lineasInputs = document.getElementById('lineasInputs');

    var urlClientes = app.getAttribute('data-url-clientes');
    var urlProductos = app.getAttribute('data-url-productos');

    var cfg = {
        montoMin: parseFloat(app.getAttribute('data-descuento-monto-min')) || 0,
        montoPct: parseFloat(app.getAttribute('data-descuento-monto-pct')) || 0,
        cantMin: parseInt(app.getAttribute('data-descuento-cant-min'), 10) || 0,
        cantPct: parseFloat(app.getAttribute('data-descuento-cant-pct')) || 0
    };

    var fmt = new Intl.NumberFormat('es-BO', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    var lineas = [];

    function dinero(n) {
        return fmt.format(n) + ' Bs';
    }

    function redondear(n) {
        return Math.round((n + Number.EPSILON) * 100) / 100;
    }

    // ---------- Buscadores ----------

    function buscar(url, termino) {
        var sep = url.indexOf('?') === -1 ? '?' : '&';
        return fetch(url + sep + 'termino=' + encodeURIComponent(termino), {
            headers: { 'Accept': 'application/json' }
        }).then(function (r) {
            return r.ok ? r.json() : [];
        }).catch(function () {
            return [];
        });
    }

    function conRetardo(fn, ms) {
        var t = null;
        return function () {
            var args = arguments;
            var self = this;
            clearTimeout(t);
            t = setTimeout(function () {
                fn.apply(self, args);
            }, ms);
        };
    }

    function ocultar(el) {
        el.hidden = true;
        el.innerHTML = '';
    }

    // ---------- Cliente ----------

    var inputCliente = document.getElementById('buscarCliente');
    var resultadosCliente = document.getElementById('resultadosCliente');
    var clienteId = document.getElementById('clienteId');
    var clienteNombre = document.getElementById('clienteNombre');
    var clienteSeleccionado = document.getElementById('clienteSeleccionado');

    function mostrarCliente(cliente) {
        clienteId.value = cliente.id;
        clienteNombre.value = cliente.nombre;
        clienteSeleccionado.hidden = false;
        clienteSeleccionado.innerHTML =
            '<span class="fj-seleccion-ico" aria-hidden="true">✓</span> ' +
            '<strong>' + cliente.nombre + '</strong>' +
            (cliente.telefono ? ' · ' + cliente.telefono : '');
        inputCliente.value = '';
        ocultar(resultadosCliente);
    }

    var buscarClienteRetardado = conRetardo(function () {
        var termino = inputCliente.value.trim();
        if (termino.length < 2) {
            ocultar(resultadosCliente);
            return;
        }

        buscar(urlClientes, termino).then(function (clientes) {
            if (!clientes.length) {
                resultadosCliente.hidden = false;
                resultadosCliente.innerHTML =
                    '<p class="fj-resultado-vacio">Sin clientes para "' + termino + '".</p>';
                return;
            }

            resultadosCliente.innerHTML = clientes.map(function (c) {
                return '<button type="button" class="fj-resultado" ' +
                    'data-cliente-id="' + c.id + '" ' +
                    'data-cliente-nombre="' + c.nombre.replace(/"/g, '&quot;') + '" ' +
                    'data-cliente-tel="' + (c.telefono || '').replace(/"/g, '&quot;') + '">' +
                    '<span class="fj-resultado-titulo">' + c.nombre + '</span>' +
                    '<span class="fj-resultado-sub">' + (c.telefono || 'Sin teléfono') + '</span>' +
                    '</button>';
            }).join('');
            resultadosCliente.hidden = false;
        });
    }, 250);

    inputCliente.addEventListener('input', buscarClienteRetardado);

    resultadosCliente.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-cliente-id]');
        if (!boton) return;
        mostrarCliente({
            id: boton.getAttribute('data-cliente-id'),
            nombre: boton.getAttribute('data-cliente-nombre'),
            telefono: boton.getAttribute('data-cliente-tel')
        });
    });

    // ---------- Producto ----------

    var inputProducto = document.getElementById('buscarProducto');
    var resultadosProducto = document.getElementById('resultadosProducto');

    var buscarProductoRetardado = conRetardo(function () {
        var termino = inputProducto.value.trim();

        buscar(urlProductos, termino).then(function (productos) {
            if (!productos.length) {
                resultadosProducto.hidden = false;
                resultadosProducto.innerHTML =
                    '<p class="fj-resultado-vacio">Sin productos disponibles.</p>';
                return;
            }

            resultadosProducto.innerHTML = productos.map(function (p) {
                var agotado = p.stock <= 0;
                return '<button type="button" class="fj-resultado" ' +
                    (agotado ? 'disabled ' : '') +
                    'data-producto-id="' + p.id + '" ' +
                    'data-producto-codigo="' + p.codigo.replace(/"/g, '&quot;') + '" ' +
                    'data-producto-nombre="' + p.nombre.replace(/"/g, '&quot;') + '" ' +
                    'data-producto-unidad="' + (p.unidad || '').replace(/"/g, '&quot;') + '" ' +
                    'data-producto-precio="' + p.precio + '" ' +
                    'data-producto-stock="' + p.stock + '">' +
                    '<span class="fj-resultado-titulo">' + p.nombre + '</span>' +
                    '<span class="fj-resultado-sub">' +
                    p.codigo + ' · ' + dinero(p.precio) +
                    ' · Stock: ' + p.stock + (agotado ? ' · Agotado' : '') +
                    '</span></button>';
            }).join('');
            resultadosProducto.hidden = false;
        });
    }, 250);

    inputProducto.addEventListener('input', buscarProductoRetardado);

    resultadosProducto.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-producto-id]');
        if (!boton || boton.disabled) return;

        agregarProducto({
            id: parseInt(boton.getAttribute('data-producto-id'), 10),
            codigo: boton.getAttribute('data-producto-codigo'),
            nombre: boton.getAttribute('data-producto-nombre'),
            unidad: boton.getAttribute('data-producto-unidad'),
            precio: parseFloat(boton.getAttribute('data-producto-precio')),
            stock: parseInt(boton.getAttribute('data-producto-stock'), 10)
        });

        inputProducto.value = '';
        inputProducto.focus();
        ocultar(resultadosProducto);
    });

    // ---------- Líneas ----------

    function buscarLinea(idProducto) {
        return lineas.filter(function (l) {
            return l.id === idProducto;
        })[0];
    }

    function agregarProducto(p) {
        var existente = buscarLinea(p.id);

        if (existente) {
            if (existente.cantidad + 1 > p.stock) {
                alerta('Solo hay ' + p.stock + ' unidades disponibles de "' + p.nombre + '".');
                return;
            }
            existente.cantidad += 1;
        } else {
            lineas.push({
                id: p.id,
                codigo: p.codigo,
                nombre: p.nombre,
                unidad: p.unidad,
                precio: p.precio,
                stock: p.stock,
                cantidad: 1
            });
        }

        render();
    }

    function quitarLinea(idProducto) {
        lineas = lineas.filter(function (l) {
            return l.id !== idProducto;
        });
        render();
    }

    function cambiarCantidad(idProducto, cantidad) {
        var linea = buscarLinea(idProducto);
        if (!linea) return;

        if (isNaN(cantidad) || cantidad < 1) {
            cantidad = 1;
        }
        if (cantidad > linea.stock) {
            cantidad = linea.stock;
            alerta('Solo hay ' + linea.stock + ' unidades disponibles de "' + linea.nombre + '".');
        }

        linea.cantidad = Math.floor(cantidad);
        render();
    }

    function alerta(mensaje) {
        window.alert(mensaje);
    }

    function render() {
        lineasVacio.hidden = lineas.length > 0;

        lineasBody.innerHTML = lineas.map(function (l) {
            return '<tr data-linea="' + l.id + '">' +
                '<td><span class="fj-nombre">' + l.nombre + '</span>' +
                '<span class="fj-nombre-sub">' + l.codigo + '</span></td>' +
                '<td class="fj-td-secundario">' + l.unidad + '</td>' +
                '<td class="fj-td-precio"><span class="fj-precio">' + fmt.format(l.precio) + '</span> <span class="fj-currency">Bs</span></td>' +
                '<td class="fj-td-cantidad"><input type="number" class="fj-cantidad-input" ' +
                'min="1" max="' + l.stock + '" value="' + l.cantidad + '" ' +
                'data-cantidad="' + l.id + '" /></td>' +
                '<td class="fj-td-precio"><span class="fj-precio">' + fmt.format(redondear(l.precio * l.cantidad)) + '</span> <span class="fj-currency">Bs</span></td>' +
                '<td class="fj-td-acciones"><button type="button" class="fj-btn fj-btn-danger fj-btn-icon" ' +
                'data-quitar="' + l.id + '" title="Quitar" aria-label="Quitar producto">' +
                '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" ' +
                'stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
                '<polyline points="3 6 5 6 21 6" /><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2" /></svg>' +
                '</button></td></tr>';
        }).join('');

        lineasInputs.innerHTML = lineas.map(function (l, i) {
            return '<input type="hidden" name="Lineas[' + i + '].IdProducto" value="' + l.id + '" />' +
                '<input type="hidden" name="Lineas[' + i + '].Cantidad" value="' + l.cantidad + '" />';
        }).join('');

        recalcularTotales();
    }

    lineasBody.addEventListener('input', function (e) {
        var input = e.target.closest('[data-cantidad]');
        if (!input) return;
        cambiarCantidad(parseInt(input.getAttribute('data-cantidad'), 10), parseInt(input.value, 10));
    });

    lineasBody.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-quitar]');
        if (!boton) return;
        quitarLinea(parseInt(boton.getAttribute('data-quitar'), 10));
    });

    // ---------- Totales ----------

    function calcularDescuento(subtotal, unidades) {
        var mejor = { pct: 0, monto: 0, politica: '' };

        if (cfg.montoMin > 0 && subtotal >= cfg.montoMin) {
            var monto = redondear(subtotal * cfg.montoPct / 100);
            if (monto > mejor.monto) {
                mejor = { pct: cfg.montoPct, monto: monto, politica: 'Descuento por monto' };
            }
        }

        if (cfg.cantMin > 0 && unidades >= cfg.cantMin) {
            var montoCant = redondear(subtotal * cfg.cantPct / 100);
            if (montoCant > mejor.monto) {
                mejor = { pct: cfg.cantPct, monto: montoCant, politica: 'Descuento por cantidad' };
            }
        }

        return mejor;
    }

    function recalcularTotales() {
        var subtotal = lineas.reduce(function (suma, l) {
            return suma + redondear(l.precio * l.cantidad);
        }, 0);
        var unidades = lineas.reduce(function (suma, l) {
            return suma + l.cantidad;
        }, 0);

        var descuento = calcularDescuento(subtotal, unidades);
        var total = redondear(subtotal - descuento.monto);

        document.getElementById('totalSubtotal').textContent = dinero(subtotal);
        document.getElementById('totalDescuento').textContent = '- ' + dinero(descuento.monto);
        document.getElementById('totalDescPct').textContent =
            descuento.monto > 0 ? '(' + descuento.pct + '%)' : '';
        document.getElementById('totalFinal').textContent = dinero(total);
        document.getElementById('politicaDescuento').textContent =
            descuento.monto > 0 ? descuento.politica + ' aplicado.' : '';

        return { subtotal: subtotal, descuento: descuento, total: total, unidades: unidades };
    }

    // ---------- Confirmación ----------

    var modal = document.getElementById('ventaConfirmModal');
    var mensajeModal = document.getElementById('ventaConfirmMessage');
    var botonOk = document.getElementById('ventaConfirmOk');

    function abrirModal(texto) {
        if (!modal) {
            if (window.confirm(texto)) form.submit();
            return;
        }
        mensajeModal.textContent = texto;
        modal.classList.add('is-open');
        modal.setAttribute('aria-hidden', 'false');
        document.body.classList.add('fj-modal-open');
        if (botonOk) botonOk.focus();
    }

    function cerrarModal() {
        if (!modal) return;
        modal.classList.remove('is-open');
        modal.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('fj-modal-open');
    }

    var botonRegistrar = document.getElementById('btnRegistrar');

    botonRegistrar.addEventListener('click', function () {
        if (!lineas.length) {
            alerta('Agregue al menos un producto antes de registrar la venta.');
            return;
        }

        if (parseInt(clienteId.value, 10) <= 0) {
            alerta('Debe seleccionar un cliente.');
            inputCliente.focus();
            return;
        }

        var resumen = recalcularTotales();
        var nombres = lineas.map(function (l) {
            return l.cantidad + '× ' + l.nombre;
        }).join(', ');

        abrirModal(
            'Cliente: ' + clienteNombre.value + ' · ' + lineas.length +
            ' producto(s). Total: ' + dinero(resumen.total) + '. ¿Registrar la venta?');
    });

    if (botonOk) {
        botonOk.addEventListener('click', function () {
            cerrarModal();
            form.submit();
        });
    }

    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target.classList.contains('fj-modal-backdrop') ||
                e.target.closest('[data-fj-modal-close]')) {
                cerrarModal();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                cerrarModal();
                cerrarNuevoCliente();
            }
        });
    }

    // ---------- Nuevo cliente (alta sin perder la venta) ----------

    var modalCliente = document.getElementById('nuevoClienteModal');
    var btnNuevoCliente = document.getElementById('btnNuevoCliente');
    var formCliente = document.getElementById('nuevoClienteForm');
    var errorCliente = document.getElementById('nuevoClienteError');

    function abrirNuevoCliente() {
        if (!modalCliente) return;
        modalCliente.classList.add('is-open');
        modalCliente.setAttribute('aria-hidden', 'false');
        document.body.classList.add('fj-modal-open');
        var primero = document.getElementById('nuevoClienteNombre');
        if (primero) primero.focus();
    }

    function cerrarNuevoCliente() {
        if (!modalCliente) return;
        modalCliente.classList.remove('is-open');
        modalCliente.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('fj-modal-open');
    }

    if (btnNuevoCliente) {
        btnNuevoCliente.addEventListener('click', abrirNuevoCliente);
    }

    if (modalCliente) {
        modalCliente.addEventListener('click', function (e) {
            if (e.target.classList.contains('fj-modal-backdrop') ||
                e.target.closest('[data-fj-modal-close]')) {
                cerrarNuevoCliente();
            }
        });
    }

    if (formCliente) {
        formCliente.addEventListener('submit', function (e) {
            e.preventDefault();
            errorCliente.hidden = true;
            errorCliente.innerHTML = '';

            var enviar = document.getElementById('nuevoClienteOk');
            if (enviar) enviar.disabled = true;

            var datos = new FormData(formCliente);

            fetch(document.getElementById('nuevoClienteUrl').value, {
                method: 'POST',
                body: datos,
                headers: { 'Accept': 'application/json' }
            }).then(function (r) {
                return r.json();
            }).then(function (respuesta) {
                if (!respuesta.exito) {
                    errorCliente.hidden = false;
                    errorCliente.innerHTML = '<span>' + respuesta.mensaje + '</span>';
                    return;
                }

                // El cliente queda seleccionado y las líneas de la venta
                // se conservan porque nunca se recargó la página.
                mostrarCliente({
                    id: respuesta.id,
                    nombre: respuesta.nombre,
                    telefono: respuesta.telefono
                });
                cerrarNuevoCliente();
                formCliente.reset();
            }).catch(function () {
                errorCliente.hidden = false;
                errorCliente.innerHTML = '<span>No se pudo registrar el cliente. Intente nuevamente.</span>';
            }).finally(function () {
                if (enviar) enviar.disabled = false;
            });
        });
    }

    // Cerrar resultados al hacer clic fuera
    document.addEventListener('click', function (e) {
        if (!e.target.closest('#buscarCliente') && !e.target.closest('#resultadosCliente')) {
            ocultar(resultadosCliente);
        }
        if (!e.target.closest('#buscarProducto') && !e.target.closest('#resultadosProducto')) {
            ocultar(resultadosProducto);
        }
    });

    render();
})();
