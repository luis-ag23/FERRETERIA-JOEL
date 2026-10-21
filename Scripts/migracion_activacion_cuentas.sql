-- ============================================================
-- Migracion: activacion de cuentas de empleado por correo
-- Base de datos: bdferreteria
-- Idempotente: puede ejecutarse varias veces sin error.
--
-- Cambios en la tabla empleado:
--   - Contrasena pasa a ser NULL: la cuenta nace sin contrasena y el
--     empleado la define al abrir el enlace de activacion.
--   - TokenActivacion: token de un solo uso generado al crear la cuenta.
--   - TokenExpira: vencimiento del token (10 minutos).
--   - TokenUsado: marca el token como consumido (evita reuso).
--
-- Semantica de Estado a partir de esta migracion:
--   -1 = cuenta creada, enlace de activacion enviado, nunca activada.
--    0 = cuenta dada de baja (deshabilitada por el administrador).
--    1 = cuenta activada correctamente (uso el enlace y definio contrasena).
--
-- Nota: los empleados existentes (semillas) conservan su contrasena y su
-- Estado = 1; solo las cuentas NUEVAS usan el flujo de activacion.
-- ============================================================

USE bdferreteria;

SET NAMES utf8mb4;

-- ------------------------------------------------------------
-- Contrasena: permitir NULL (cuenta sin activar)
-- ------------------------------------------------------------
SET @esNula := (
    SELECT IS_NULLABLE
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND COLUMN_NAME = 'Contrasena'
);

SET @sql := IF(
    @esNula = 'NO',
    'ALTER TABLE empleado MODIFY COLUMN Contrasena VARCHAR(255) NULL DEFAULT NULL',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ------------------------------------------------------------
-- TokenActivacion
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND COLUMN_NAME = 'TokenActivacion'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE empleado ADD COLUMN TokenActivacion VARCHAR(128) NULL DEFAULT NULL AFTER Contrasena',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ------------------------------------------------------------
-- TokenExpira
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND COLUMN_NAME = 'TokenExpira'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE empleado ADD COLUMN TokenExpira DATETIME NULL DEFAULT NULL AFTER TokenActivacion',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ------------------------------------------------------------
-- TokenUsado
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND COLUMN_NAME = 'TokenUsado'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE empleado ADD COLUMN TokenUsado TINYINT(1) NOT NULL DEFAULT 0 AFTER TokenExpira',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ------------------------------------------------------------
-- Indice del token para busquedas por enlace
-- ------------------------------------------------------------
SET @existe := (
    SELECT COUNT(*)
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'empleado'
      AND INDEX_NAME = 'IDX_EMPLEADO_TOKEN'
);

SET @sql := IF(
    @existe = 0,
    'ALTER TABLE empleado ADD KEY IDX_EMPLEADO_TOKEN (TokenActivacion)',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- Verificacion rapida
-- ============================================================
SELECT
    COLUMN_NAME,
    COLUMN_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = 'empleado'
  AND COLUMN_NAME IN (
      'Contrasena',
      'TokenActivacion',
      'TokenExpira',
      'TokenUsado'
  )
ORDER BY ORDINAL_POSITION;
