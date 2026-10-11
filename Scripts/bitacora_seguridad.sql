-- ============================================================
-- Bitácora de sesiones (login / logout) - FERRETERÍA JOEL
-- Base de datos: bdferreteria
-- Este script ÚNICAMENTE crea la tabla de bitácora de sesiones.
-- Es idempotente: puede ejecutarse varias veces sin error.
-- ============================================================

USE bdferreteria;

SET NAMES utf8mb4;

-- ------------------------------------------------------------
-- bitacora_sesion
--   IdBitacora    -> identificador autoincremental
--   Accion        -> 'login' o 'logout'
--   IdUsuario     -> id del usuario/empleado autenticado (opcional)
--   NombreUsuario -> nombre completo del usuario (opcional)
--   TipoUsuario   -> 'Usuario' o 'Empleado'
--   FechaHora     -> momento exacto del evento
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS bitacora_sesion
(
    IdBitacora    INT          NOT NULL AUTO_INCREMENT,
    Accion        VARCHAR(20)  NOT NULL COMMENT 'login | logout',
    IdUsuario     INT          NULL DEFAULT NULL,
    NombreUsuario VARCHAR(150) NULL DEFAULT NULL,
    TipoUsuario   VARCHAR(50)  NOT NULL COMMENT 'Usuario | Empleado',
    FechaHora     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (IdBitacora),
    KEY IDX_BITACORA_FECHA (FechaHora),
    KEY IDX_BITACORA_USUARIO (TipoUsuario, IdUsuario)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;
