// FERRETERÍA JOEL — Pantalla de registro de venta.
// Gestiona los dos buscadores, la tabla temporal de líneas (M:N) y
// la previsualización de totales. La validación y el cálculo
// definitivos siempre ocurren en el servidor (FachadaVenta).
//
// Todo el contenido dinámico se construye con nodos del DOM y
// textContent (nunca innerHTML con datos del usuario) para evitar
// inyección de HTML/XSS.

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

    function crear(tag, clase, texto) {
        var el = document.createElement(tag);
        if (clase) el.className = clase;
        if (texto !== undefined && texto !== null) el.textContent = texto;
        return el;
    }

    // ---------- Buscadores ----------

    function errorHttp(codigo) {
        if (codigo === 401) return 'La sesión ha expirado o no se ha iniciado. Inicie sesión para continuar.';
        if (codigo === 403) return 'No tiene permisos para esta operación.';
        if (codigo === 400) return 'La solicitud no es válida. Revise los datos enviados.';
        if (codigo === 404) return 'No se encontró el servicio solicitado. Revise las rutas de Ventas.';
        if (codigo >= 500) return 'Error del servidor o de la conexión con MySQL.';
        return 'No se pudo completar la solicitud (HTTP ' + codigo + ').';
    }

    function leerJson(response) {
        var esJson = (response.headers.get('content-type') || '').includes('application/json');
        return (esJson ? response.json() : Promise.resolve(null))
            .catch(function () { return null; })
            .then(function (datos) {
                if (!response.ok) {
                    var error = new Error(datos && datos.mensaje || errorHttp(response.status));
                    error.status = response.status;
                    error.datos = datos;
                    throw error;
                }
                if (datos === null) {
                    throw new Error('El servidor no devolvió JSON válido. Revise la sesión y la ruta.');
                }
                return datos;
            });
    }

    function buscar(url, termino) {
        var sep = url.indexOf('?') === -1 ? '?' : '&';
        return fetch(url + sep + 'termino=' + encodeURIComponent(termino), {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json' }
        }).then(leerJson);
    }

    function mostrarErrorResultados(contenedor, error) {
        contenedor.replaceChildren(
            crear('p', 'fj-resultado-vacio', error.message || 'Falló la consulta al servidor.'));
        contenedor.hidden = false;
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
        el.replaceChildren();
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

        clienteSeleccionado.replaceChildren();
        var ico = crear('span', 'fj-seleccion-ico', '✓');
        ico.setAttribute('aria-hidden', 'true');
        clienteSeleccionado.appendChild(ico);
        clienteSeleccionado.appendChild(document.createTextNode(' '));
        clienteSeleccionado.appendChild(crear('strong', null, cliente.nombre));
        if (cliente.telefono) {
            clienteSeleccionado.appendChild(document.createTextNode(' · ' + cliente.telefono));
        }
        clienteSeleccionado.hidden = false;

        inputCliente.value = '';
        secuenciaClientes += 1;
        ocultar(resultadosCliente);
        actualizarBotonRegistrar();
    }

    var secuenciaClientes = 0;
    var buscarClienteRetardado = conRetardo(function () {
        var solicitud = secuenciaClientes;
        var termino = inputCliente.value.trim();
        if (termino.length < 2) {
            ocultar(resultadosCliente);
            return;
        }

        buscar(urlClientes, termino).then(function (clientes) {
            if (solicitud !== secuenciaClientes) return;
            resultadosCliente.replaceChildren();

            if (!clientes.length) {
                resultadosCliente.appendChild(
                    crear('p', 'fj-resultado-vacio', 'Sin clientes para "' + termino + '".'));
                resultadosCliente.hidden = false;
                return;
            }

            clientes.forEach(function (c) {
                var boton = crear('button', 'fj-resultado');
                boton.type = 'button';
                boton.dataset.clienteId = c.id;
                boton.dataset.clienteNombre = c.nombre;
                boton.dataset.clienteTel = c.telefono || '';
                boton.appendChild(crear('span', 'fj-resultado-titulo', c.nombre));
                boton.appendChild(
                    crear('span', 'fj-resultado-sub', c.telefono || 'Sin teléfono'));
                resultadosCliente.appendChild(boton);
            });

            resultadosCliente.hidden = false;
        }).catch(function (error) {
            if (solicitud !== secuenciaClientes) return;
            mostrarErrorResultados(resultadosCliente, error);
        });
    }, 250);

    inputCliente.addEventListener('input', function () {
        secuenciaClientes += 1;
        buscarClienteRetardado();
    });

    resultadosCliente.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-cliente-id]');
        if (!boton) return;
        mostrarCliente({
            id: boton.dataset.clienteId,
            nombre: boton.dataset.clienteNombre,
            telefono: boton.dataset.clienteTel
        });
    });

    // ---------- Producto ----------

    var inputProducto = document.getElementById('buscarProducto');
    var resultadosProducto = document.getElementById('resultadosProducto');

    var secuenciaProductos = 0;
    var buscarProductoRetardado = conRetardo(function () {
        var solicitud = secuenciaProductos;
        var termino = inputProducto.value.trim();
        if (!termino) {
            ocultar(resultadosProducto);
            return;
        }

        buscar(urlProductos, termino).then(function (productos) {
            if (solicitud !== secuenciaProductos) return;
            resultadosProducto.replaceChildren();

            if (!productos.length) {
                resultadosProducto.appendChild(
                    crear('p', 'fj-resultado-vacio', 'Sin productos disponibles.'));
                resultadosProducto.hidden = false;
                return;
            }

            productos.forEach(function (p) {
                var agotado = p.stock <= 0;
                var boton = crear('button', 'fj-resultado');
                boton.type = 'button';
                if (agotado) boton.disabled = true;
                boton.dataset.productoId = p.id;
                boton.dataset.productoCodigo = p.codigo;
                boton.dataset.productoNombre = p.nombre;
                boton.dataset.productoUnidad = p.unidad || '';
                boton.dataset.productoPrecio = p.precio;
                boton.dataset.productoStock = p.stock;
                boton.appendChild(crear('span', 'fj-resultado-titulo', p.nombre));
                boton.appendChild(crear('span', 'fj-resultado-sub',
                    p.codigo + ' · ' + dinero(p.precio) +
                    ' · Stock: ' + p.stock + (agotado ? ' · Agotado' : '')));
                resultadosProducto.appendChild(boton);
            });

            resultadosProducto.hidden = false;
        }).catch(function (error) {
            if (solicitud !== secuenciaProductos) return;
            mostrarErrorResultados(resultadosProducto, error);
        });
    }, 250);

    inputProducto.addEventListener('input', function () {
        secuenciaProductos += 1;
        buscarProductoRetardado();
    });

    resultadosProducto.addEventListener('click', function (e) {
        var boton = e.target.closest('[data-producto-id]');
        if (!boton || boton.disabled) return;

        agregarProducto({
            id: parseInt(boton.dataset.productoId, 10),
            codigo: boton.dataset.productoCodigo,
            nombre: boton.dataset.productoNombre,
            unidad: boton.dataset.productoUnidad,
            precio: parseFloat(boton.dataset.productoPrecio),
            stock: parseInt(boton.dataset.productoStock, 10)
        });

        inputProducto.value = '';
        secuenciaProductos += 1;
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
        var contenedor = document.getElementById('fj-toast-center-container');
        if (!contenedor) {
            contenedor = crear('div', 'position-fixed top-50 start-50 translate-middle');
            contenedor.id = 'fj-toast-center-container';
            contenedor.style.zIndex = '2000';
            contenedor.style.maxWidth = '420px';
            contenedor.style.width = '90%';
            document.body.appendChild(contenedor);
        }

        var toast = crear('div', 'toast align-items-center text-bg-warning border-0 show shadow-lg rounded-3');
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        var flex = crear('div', 'd-flex align-items-center p-2');
        var body = crear('div', 'toast-body fw-bold text-center flex-grow-1 fs-6', mensaje);

        var btnCerrar = crear('button', 'btn-close me-2 m-auto');
        btnCerrar.type = 'button';
        btnCerrar.setAttribute('aria-label', 'Cerrar');
        btnCerrar.addEventListener('click', function () {
            toast.remove();
        });

        flex.appendChild(body);
        flex.appendChild(btnCerrar);
        toast.appendChild(flex);

        contenedor.replaceChildren(toast);

        setTimeout(function () {
            toast.classList.remove('show');
            setTimeout(function () {
                toast.remove();
            }, 300);
        }, 3000);
    }

    function celdaPrecio(valor) {
        var td = crear('td', 'fj-td-precio');
        td.appendChild(crear('span', 'fj-precio', fmt.format(valor)));
        td.appendChild(document.createTextNode(' '));
        td.appendChild(crear('span', 'fj-currency', 'Bs'));
        return td;
    }

    function svgQuitar() {
        var svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        svg.setAttribute('viewBox', '0 0 24 24');
        svg.setAttribute('fill', 'none');
        svg.setAttribute('stroke', 'currentColor');
        svg.setAttribute('stroke-width', '1.9');
        svg.setAttribute('stroke-linecap', 'round');
        svg.setAttribute('stroke-linejoin', 'round');
        svg.setAttribute('aria-hidden', 'true');
        var p1 = document.createElementNS('http://www.w3.org/2000/svg', 'polyline');
        p1.setAttribute('points', '3 6 5 6 21 6');
        var p2 = document.createElementNS('http://www.w3.org/2000/svg', 'path');
        p2.setAttribute('d', 'M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2');
        svg.appendChild(p1);
        svg.appendChild(p2);
        return svg;
    }

    function render() {
        lineasVacio.hidden = lineas.length > 0;

        lineasBody.replaceChildren();
        lineas.forEach(function (l) {
            var tr = crear('tr');
            tr.dataset.linea = l.id;

            var tdProd = crear('td');
            tdProd.appendChild(crear('span', 'fj-nombre', l.nombre));
            tdProd.appendChild(crear('span', 'fj-nombre-sub', l.codigo));

            var tdUnidad = crear('td', 'fj-td-secundario', l.unidad);

            var tdPrecio = celdaPrecio(l.precio);

            var tdCantidad = crear('td', 'fj-td-cantidad');
            var input = crear('input', 'fj-cantidad-input');
            input.type = 'number';
            input.min = '1';
            input.max = String(l.stock);
            input.value = String(l.cantidad);
            input.dataset.cantidad = l.id;
            tdCantidad.appendChild(input);

            var tdSubtotal = celdaPrecio(redondear(l.precio * l.cantidad));

            var tdAcciones = crear('td', 'fj-td-acciones');
            var btnQuitar = crear('button', 'fj-btn fj-btn-danger fj-btn-icon');
            btnQuitar.type = 'button';
            btnQuitar.title = 'Quitar';
            btnQuitar.setAttribute('aria-label', 'Quitar producto');
            btnQuitar.dataset.quitar = l.id;
            btnQuitar.appendChild(svgQuitar());
            tdAcciones.appendChild(btnQuitar);

            tr.appendChild(tdProd);
            tr.appendChild(tdUnidad);
            tr.appendChild(tdPrecio);
            tr.appendChild(tdCantidad);
            tr.appendChild(tdSubtotal);
            tr.appendChild(tdAcciones);
            lineasBody.appendChild(tr);
        });

        lineasInputs.replaceChildren();
        lineas.forEach(function (l, i) {
            var idInput = crear('input');
            idInput.type = 'hidden';
            idInput.name = 'Lineas[' + i + '].IdProducto';
            idInput.value = l.id;
            lineasInputs.appendChild(idInput);

            var cantInput = crear('input');
            cantInput.type = 'hidden';
            cantInput.name = 'Lineas[' + i + '].Cantidad';
            cantInput.value = l.cantidad;
            lineasInputs.appendChild(cantInput);
        });

        recalcularTotales();
        actualizarBotonRegistrar();
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

    // ---------- Botón Registrar ----------

    var botonRegistrar = document.getElementById('btnRegistrar');
    var botonRegistrarTexto = document.getElementById('btnRegistrarTexto');

    function actualizarBotonRegistrar() {
        if (!botonRegistrar) return;
        var listo = lineas.length > 0 && parseInt(clienteId.value, 10) > 0;
        botonRegistrar.disabled = !listo;
    }

    function marcandoEnvio(activo) {
        if (!botonRegistrar) return;
        botonRegistrar.disabled = activo;
        if (botonRegistrarTexto) {
            botonRegistrarTexto.textContent = activo ? 'Registrando…' : 'Registrar venta';
        }
    }

    // ---------- Confirmación ----------

    var modal = document.getElementById('ventaConfirmModal');
    var mensajeModal = document.getElementById('ventaConfirmMessage');
    var botonOk = document.getElementById('ventaConfirmOk');

    function abrirModal(texto) {
        if (!modal) {
            if (window.confirm(texto)) {
                marcandoEnvio(true);
                form.submit();
            }
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

    botonRegistrar.addEventListener('click', function () {
        if (botonRegistrar.disabled) return;

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
        abrirModal(
            'Cliente: ' + clienteNombre.value + ' · ' + lineas.length +
            ' producto(s). Total: ' + dinero(resumen.total) + '. ¿Registrar la venta?');
    });

    if (botonOk) {
        botonOk.addEventListener('click', function () {
            cerrarModal();
            marcandoEnvio(true);
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

        function mostrarErrorCliente(mensaje) {
        errorCliente.replaceChildren(crear('span', null, mensaje));
        errorCliente.hidden = false;
    }

    // Reglas espejo de las del servidor (ClienteValidador). El servidor
    // vuelve a validar siempre: esto solo evita viajes innecesarios.
    var campoCliente = {
        Nombre: 'nuevoClienteNombre',
        Apellido: 'nuevoClienteApellido',
        Telefono: 'nuevoClienteTelefono'
    };
    var formatoNombre = /^\p{L}[\p{L}\s.,'\-&()]*$/u;
    var formatoTelefono = /^[5-8][0-9]{7}$/;

    function limpiarErroresCliente() {
        errorCliente.hidden = true;
        errorCliente.replaceChildren();
        Object.keys(campoCliente).forEach(function (clave) {
            var input = document.getElementById(campoCliente[clave]);
            var contenedor = input.closest('.fj-form-field');
            contenedor.classList.remove('fj-form-field-error');
            input.removeAttribute('aria-invalid');
            var aviso = contenedor.querySelector('.fj-form-error');
            if (aviso) aviso.remove();
        });
    }

    function mostrarErroresCampo(errores) {
        var primero = null;
        Object.keys(errores).forEach(function (clave) {
            var id = campoCliente[clave];
            if (!id) return;
            var input = document.getElementById(id);
            var contenedor = input.closest('.fj-form-field');
            contenedor.classList.add('fj-form-field-error');
            input.setAttribute('aria-invalid', 'true');
            var aviso = crear('p', 'fj-form-error', errores[clave]);
            aviso.setAttribute('role', 'alert');
            contenedor.appendChild(aviso);
            if (!primero) primero = input;
        });
        if (primero) primero.focus();
    }

    function validarCliente(datos) {
        var errores = {};
        var nombre = datos.nombre.trim();
        var apellido = datos.apellido.trim();
        var telefono = datos.telefono.replace(/[\s-]/g, '');

        if (!nombre) errores.Nombre = 'Los nombres del cliente son obligatorios.';
        else if (nombre.length > 100 || !formatoNombre.test(nombre))
            errores.Nombre = 'Los nombres solo admiten letras y espacios (máximo 100 caracteres).';

        if (!apellido) errores.Apellido = 'Los apellidos del cliente son obligatorios.';
        else if (apellido.length > 100 || !formatoNombre.test(apellido))
            errores.Apellido = 'Los apellidos solo admiten letras y espacios (máximo 100 caracteres).';

        if (!telefono) errores.Telefono = 'El teléfono del cliente es obligatorio.';
        else if (!formatoTelefono.test(telefono))
            errores.Telefono = 'El teléfono debe tener 8 dígitos y comenzar con 5, 6, 7 u 8.';

        return errores;
    }

    // Al reabrir el modal no deben quedar errores de un intento anterior.
    if (btnNuevoCliente) {
        btnNuevoCliente.addEventListener('click', limpiarErroresCliente);
    }

    if (formCliente) {
        formCliente.addEventListener('submit', function (e) {
            e.preventDefault();
            limpiarErroresCliente();

            var erroresLocales = validarCliente({
                nombre: document.getElementById('nuevoClienteNombre').value,
                apellido: document.getElementById('nuevoClienteApellido').value,
                telefono: document.getElementById('nuevoClienteTelefono').value
            });
            if (Object.keys(erroresLocales).length > 0) {
                mostrarErroresCampo(erroresLocales);
                return;
            }

            var enviar = document.getElementById('nuevoClienteOk');
            if (enviar) enviar.disabled = true;

            var datos = new FormData(formCliente);

            fetch(document.getElementById('nuevoClienteUrl').value, {
                method: 'POST',
                credentials: 'same-origin',
                body: datos,
                headers: { 'Accept': 'application/json' }
            }).then(leerJson).then(function (respuesta) {
                if (!respuesta.exito) {
                    mostrarErrorCliente(respuesta.mensaje);
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
            }).catch(function (error) {
                if (error.datos && error.datos.errores &&
                    Object.keys(error.datos.errores).length > 0) {
                    mostrarErroresCampo(error.datos.errores);
                    return;
                }
                mostrarErrorCliente(error.message || 'No se pudo registrar el cliente. Intente nuevamente.');
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
