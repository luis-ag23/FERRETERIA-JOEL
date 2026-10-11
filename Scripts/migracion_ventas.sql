-- ============================================================
-- Migracion: modulo de VENTAS
-- Base de datos destino: bdferreteria_dev (NO ejecutar en la principal)
-- Ejecutar UNA sola vez sobre una base ya creada con esquema.sql.
--
-- Cambios:
--   venta    : estado de venta, auditoria de anulacion, idempotencia,
--              snapshot de descuento y CodigoVenta diferido (NULL + UPDATE)
--   cliente  : Contrasena opcional + auditoria del empleado que lo registra
-- ============================================================

USE bdferreteria_dev;

-- ------------------------------------------------------------
-- VENTA
-- ------------------------------------------------------------

-- El correlativo se asigna DESPUES del INSERT, dentro de la misma
-- transaccion, a partir del IdVenta autoincremental. Por eso se
-- permite NULL (MySQL admite varios NULL en un UNIQUE).
ALTER TABLE venta
    MODIFY COLUMN CodigoVenta VARCHAR(50) NULL DEFAULT NULL;

ALTER TABLE venta
    ADD COLUMN EstadoVenta        VARCHAR(20)   NOT NULL DEFAULT 'Registrada'
        AFTER CodigoVenta,
    ADD COLUMN Subtotal           DECIMAL(12,2) NOT NULL DEFAULT 0
        AFTER MontoTotal,
    ADD COLUMN MontoDescuento     DECIMAL(12,2) NOT NULL DEFAULT 0
        AFTER Subtotal,
    ADD COLUMN PorcentajeDescuento DECIMAL(5,2) NOT NULL DEFAULT 0
        AFTER MontoDescuento,
    ADD COLUMN ClaveIdempotencia  VARCHAR(64)   NULL DEFAULT NULL
        AFTER PorcentajeDescuento,
    ADD COLUMN FechaAnulacion     DATETIME      NULL DEFAULT NULL
        AFTER FechaActualizacion,
    ADD COLUMN MotivoAnulacion    VARCHAR(255)  NULL DEFAULT NULL
        AFTER FechaAnulacion,
    ADD COLUMN IdEmpleadoAnulacion INT          NULL DEFAULT NULL
        AFTER MotivoAnulacion;

ALTER TABLE venta
    ADD UNIQUE KEY UNIQUE_VENTA_IDEMPOTENCIA (ClaveIdempotencia);

ALTER TABLE venta
    ADD CONSTRAINT FK_venta_empleado_anulacion
        FOREIGN KEY (IdEmpleadoAnulacion) REFERENCES empleado (IdEmpleado);

-- ------------------------------------------------------------
-- CLIENTE
-- ------------------------------------------------------------

-- Cliente de mostrador: la contrasena no aplica.
ALTER TABLE cliente
    MODIFY COLUMN Contrasena VARCHAR(255) NULL DEFAULT NULL;

-- Auditoria: empleado que registro el cliente.
ALTER TABLE cliente
    ADD COLUMN IdEmpleadoResponsable INT NULL DEFAULT NULL
        AFTER IdClienteResponsable;

ALTER TABLE cliente
    ADD CONSTRAINT FK_cliente_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado);
