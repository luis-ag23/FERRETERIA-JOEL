-- ============================================================
-- Compatibilidad de credenciales con el hasheador actual (Argon2)
-- Base de datos: bdferreteria
--
-- El SQL de esquema guarda hashes PBKDF2 (comentario antiguo), pero el
-- ServicioInicioSesion actual reconoce hashes Argon2 o texto plano.
-- Este script deja las contrasenas seed en texto plano para que, al primer
-- login correcto, ServicioInicioSesion las vuelva a hashear con Argon2.
-- ============================================================

USE bdferreteria;

-- Desactiva temporalmente el modo seguro de MySQL Workbench para permitir
-- la actualizacion masiva de la tabla empleado.
SET SQL_SAFE_UPDATES = 0;

UPDATE empleado
SET Contrasena = 'admin123'
WHERE CorreoElectronico = 'joel.rios@ferreteria.com';

UPDATE empleado
SET Contrasena = 'clave123'
WHERE Rol = 'EmpleadoVenta'
  AND IdEmpleado > 0;

SET SQL_SAFE_UPDATES = 1;

SELECT CorreoElectronico, Rol, Contrasena
FROM empleado
ORDER BY IdEmpleado;
