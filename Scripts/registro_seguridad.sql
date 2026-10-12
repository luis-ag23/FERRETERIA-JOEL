-- ============================================================
-- Registro de seguridad: índice de unicidad de correo
-- Base de datos: bdferreteria
-- Idempotente: puede ejecutarse varias veces sin error.
-- ============================================================

USE bdferreteria;

SET NAMES utf8mb4;

-- ------------------------------------------------------------
-- Cliente: índice único de correo (si no existe)
-- El esquema original no lo definía; se agrega para el auto-registro.
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'cliente'
      AND INDEX_NAME = 'UNIQUE_CLIENTE_CORREO'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE cliente ADD UNIQUE KEY UNIQUE_CLIENTE_CORREO (CorreoElectronico)',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
