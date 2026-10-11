-- ============================================================
-- Migración: hash de contraseñas de empleados (PBKDF2-HMAC-SHA256)
-- Base de datos: bdferreteria_dev
-- Objetivo: reemplazar las contraseñas de semilla en texto plano por
--           hashes PBKDF2 (formato pbkdf2$iteraciones$sal$hash).
-- Nota: el sistema también migra automáticamente cualquier contraseña
--       heredada en texto plano al primer inicio de sesión; este script
--       es idempotente y puede ejecutarse varias veces sin efectos dañinos.
-- ============================================================

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$i6K/10J1uVUy7aaxu05n6Q==$3D4C9TUHOOT08JtcWtVN9msWrt9otzyRNaXCCeEPNzk='
WHERE CorreoElectronico = 'joel.rios@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$EiFO247GganlHDHRxibZDw==$ovflhHg31hgKKpNQZVQ3tZjGB1w/w8XDZrnb5axXvm4='
WHERE CorreoElectronico = 'maria.lopez@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$ayUjMsgyfL17cWAkvjwSiA==$J3qQUfUFBYMAB3ylW0NOgMvAifND8c7EbnxWFnPPKYM='
WHERE CorreoElectronico = 'carlos.mendoza@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$KXtZXQhBSn5/LDoMSA1J/w==$48er+pEZ2cVPdYmKYZ6CsabCgwesfQWZkNmI86CjX+A='
WHERE CorreoElectronico = 'ana.gutierrez@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$CFNYcmqMCgdqeSA09yxzcA==$/Om+zTZhiKWd93h3XrZE7POI4QK7Dym8YOkhzwgt5Do='
WHERE CorreoElectronico = 'luis.fernandez@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$QlYdGoL14VOiIwBvuXr1SA==$xWtOKNuCO6Ww+Pi2ak7kylnmTrrB7crk972Z8N5AzuE='
WHERE CorreoElectronico = 'sofia.ramirez@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$kNf5ZyOi23tYQJwSbhxbvg==$xEUvYHu4mK/gQya8j+ZIxfBAkZTbfm93bHw2Cx3HlU8='
WHERE CorreoElectronico = 'diego.torres@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$72lYf8pr8D50FmGTnHaaHw==$Pym0xZNlP3gLWsolMIFE9AaYkPb+eKCdnT05Vr84jy4='
WHERE CorreoElectronico = 'lucia.vargas@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$TJfoEKh9Yji7JlavWOwDlg==$hFZpDnK6VhKn0dyHmEY0RGX4eC4HROsAt5EdVCvmvA8='
WHERE CorreoElectronico = 'pedro.castillo@ferreteria.com';

UPDATE empleado
SET Contrasena = 'pbkdf2$100000$271ELSu8jKyojeVGCV5b3w==$+cyOdhoBHX5E4XGFZBOWZoEjFvNhibHGJsBrXOQ0z9U='
WHERE CorreoElectronico = 'elena.rojas@ferreteria.com';
