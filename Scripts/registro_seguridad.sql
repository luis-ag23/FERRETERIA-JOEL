-- ============================================================
-- Seguridad de acceso: índice de unicidad de correo del empleado
-- Base de datos: bdferreteria
-- Idempotente: puede ejecutarse varias veces sin error.
--
-- Nota: los clientes ya no inician sesión, por lo que la unicidad de
-- correo aplica únicamente a los empleados (login del personal).
-- ============================================================

USE bdferreteria;

SET NAMES utf8mb4;

-- ------------------------------------------------------------
-- Empleado: índice único de correo (si no existe)
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND INDEX_NAME = 'UNIQUE_EMPLEADO_CORREO'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE empleado ADD UNIQUE KEY UNIQUE_EMPLEADO_CORREO (CorreoElectronico)',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
