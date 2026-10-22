-- ############################################################
-- SETUP COMPLETO DE BASE DE DATOS - FERRETERIA JOEL
-- Generado para ejecucion en MySQL 8+ (MySQL Workbench o mysql CLI)
-- Orden recomendado por README: esquema -> registro de seguridad -> bitacora
-- La migracion_ventas NO se incluye porque es solo para bases antiguas.
-- ############################################################

-- Esquema completo: FERRETERIA JOEL
-- Base de datos: bdferreteria
-- Crea todas las tablas (con sus llaves foraneas) e inserta 10 registros
-- en: empleado, marca, categoria, producto y proveedor.

CREATE DATABASE IF NOT EXISTS bdferreteria
    DEFAULT CHARACTER SET utf8mb4
    DEFAULT COLLATE utf8mb4_unicode_ci;

USE bdferreteria;

SET NAMES utf8mb4;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS historico_precio;
DROP TABLE IF EXISTS detalle_venta;
DROP TABLE IF EXISTS venta;
DROP TABLE IF EXISTS cliente;
DROP TABLE IF EXISTS producto;
DROP TABLE IF EXISTS proveedor;
DROP TABLE IF EXISTS categoria;
DROP TABLE IF EXISTS marca;
DROP TABLE IF EXISTS empleado;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================
-- EMPLEADO (IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE empleado
(
    IdEmpleado            INT          NOT NULL AUTO_INCREMENT,
    Nombre                VARCHAR(100) NOT NULL,
    Apellido              VARCHAR(100) NOT NULL,
    Rol                   VARCHAR(50)  NOT NULL,
    Telefono              VARCHAR(20)  NOT NULL,
    CorreoElectronico     VARCHAR(150) NULL DEFAULT NULL,
    Contrasena            VARCHAR(255) NULL DEFAULT NULL,
    TokenActivacion       VARCHAR(128) NULL DEFAULT NULL,
    TokenExpira           DATETIME     NULL DEFAULT NULL,
    TokenUsado            TINYINT(1)   NOT NULL DEFAULT 0,
    Estado                TINYINT      NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME     NULL DEFAULT NULL,
    IdEmpleadoResponsable INT          NULL DEFAULT NULL,
    PRIMARY KEY (IdEmpleado),
    UNIQUE KEY UNIQUE_EMPLEADO_CORREO (CorreoElectronico),
    KEY IDX_EMPLEADO_TOKEN (TokenActivacion),
    CONSTRAINT FK_empleado_responsable
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- MARCA (IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE marca
(
    IdMarca               INT          NOT NULL AUTO_INCREMENT,
    Nombre                VARCHAR(100) NOT NULL,
    Descripcion           VARCHAR(255) NULL DEFAULT NULL,
    PaisOrigen            VARCHAR(100) NOT NULL DEFAULT '',
    Calidad               VARCHAR(50)  NOT NULL DEFAULT '',
    Estado                TINYINT      NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME     NULL DEFAULT NULL,
    IdEmpleadoResponsable INT          NOT NULL,
    PRIMARY KEY (IdMarca),
    UNIQUE KEY UNIQUE_NOMBRE_MARCA (Nombre),
    CONSTRAINT FK_marca_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- CATEGORIA (IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE categoria
(
    IdCategoria           INT           NOT NULL AUTO_INCREMENT,
    Codigo                VARCHAR(50)   NOT NULL,
    Nombre                VARCHAR(100)  NOT NULL,
    Descripcion           VARCHAR(255)  NULL DEFAULT NULL,
    PorcentajeGanancia    DECIMAL(5,2)  NOT NULL DEFAULT 0,
    Estado                TINYINT       NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME      NULL DEFAULT NULL,
    IdEmpleadoResponsable INT           NOT NULL,
    PRIMARY KEY (IdCategoria),
    UNIQUE KEY UNIQUE_CATEGORIA_CODIGO (Codigo),
    CONSTRAINT FK_categoria_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- PROVEEDOR (IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE proveedor
(
    IdProveedor           INT          NOT NULL AUTO_INCREMENT,
    RazonSocial           VARCHAR(150) NOT NULL,
    NombreComercial       VARCHAR(150) NOT NULL,
    NombreContacto        VARCHAR(150) NOT NULL,
    Telefono              VARCHAR(20)  NOT NULL,
    CorreoElectronico     VARCHAR(150) NULL DEFAULT NULL,
    Direccion             VARCHAR(255) NULL DEFAULT NULL,
    Estado                TINYINT      NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME     NULL DEFAULT NULL,
    IdEmpleadoResponsable INT          NOT NULL,
    PRIMARY KEY (IdProveedor),
    UNIQUE KEY UNIQUE_PROVEEDOR_RAZON (RazonSocial),
    CONSTRAINT FK_proveedor_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- PRODUCTO (IdCategoria -> categoria, Marca -> marca,
--           IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE producto
(
    IdProducto            INT           NOT NULL AUTO_INCREMENT,
    IdCategoria           INT           NOT NULL,
    Codigo                VARCHAR(50)   NOT NULL,
    Nombre                VARCHAR(100)  NOT NULL,
    Descripcion           VARCHAR(255)  NULL DEFAULT NULL,
    Marca                 INT           NOT NULL,
    UnidadMedida          VARCHAR(50)   NOT NULL,
    PrecioVenta           DECIMAL(12,2) NOT NULL DEFAULT 0,
    CantidadStock         INT           NOT NULL DEFAULT 0,
    Estado                TINYINT       NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME      NULL DEFAULT NULL,
    IdEmpleadoResponsable INT           NOT NULL,
    PRIMARY KEY (IdProducto),
    UNIQUE KEY UNIQUE_PRODUCTO_CODIGO (Codigo),
    CONSTRAINT FK_producto_categoria
        FOREIGN KEY (IdCategoria) REFERENCES categoria (IdCategoria),
    CONSTRAINT FK_producto_marca
        FOREIGN KEY (Marca) REFERENCES marca (IdMarca),
    CONSTRAINT FK_producto_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- CLIENTE (IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE cliente
(
    IdCliente             INT          NOT NULL AUTO_INCREMENT,
    Nombre                VARCHAR(100) NOT NULL,
    Apellido              VARCHAR(100) NOT NULL,
    Telefono              VARCHAR(20)  NOT NULL,
    Estado                TINYINT      NOT NULL DEFAULT 1,
    FechaRegistro         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME     NULL DEFAULT NULL,
    IdEmpleadoResponsable INT          NULL DEFAULT NULL,
    PRIMARY KEY (IdCliente),
    CONSTRAINT FK_cliente_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- VENTA (IdCliente -> cliente, IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE venta
(
    IdVenta               INT           NOT NULL AUTO_INCREMENT,
    CodigoVenta           VARCHAR(50)   NULL DEFAULT NULL,
    EstadoVenta           VARCHAR(20)   NOT NULL DEFAULT 'Registrada',
    IdCliente             INT           NOT NULL,
    MetodoPago            VARCHAR(50)   NOT NULL,
    EstadoPago            VARCHAR(50)   NOT NULL,
    MontoTotal            DECIMAL(12,2) NOT NULL DEFAULT 0,
    Subtotal              DECIMAL(12,2) NOT NULL DEFAULT 0,
    MontoDescuento        DECIMAL(12,2) NOT NULL DEFAULT 0,
    PorcentajeDescuento   DECIMAL(5,2)  NOT NULL DEFAULT 0,
    FechaRegistro         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion    DATETIME      NULL DEFAULT NULL,
    FechaAnulacion        DATETIME      NULL DEFAULT NULL,
    MotivoAnulacion       VARCHAR(255)  NULL DEFAULT NULL,
    IdEmpleadoAnulacion   INT           NULL DEFAULT NULL,
    IdEmpleadoResponsable INT           NOT NULL,
    PRIMARY KEY (IdVenta),
    UNIQUE KEY UNIQUE_CODIGO_VENTA (CodigoVenta),
    CONSTRAINT FK_venta_cliente
        FOREIGN KEY (IdCliente) REFERENCES cliente (IdCliente),
    CONSTRAINT FK_venta_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado),
    CONSTRAINT FK_venta_empleado_anulacion
        FOREIGN KEY (IdEmpleadoAnulacion) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- DETALLE_VENTA (IdVenta -> venta, IdProducto -> producto,
--                IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE detalle_venta
(
    IdDetalleVenta        INT           NOT NULL AUTO_INCREMENT,
    IdVenta               INT           NOT NULL,
    IdProducto            INT           NOT NULL,
    Cantidad              INT           NOT NULL,
    PrecioUnitario        DECIMAL(12,2) NOT NULL,
    TotalProducto         DECIMAL(12,2) NOT NULL,
    FechaRegistro         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IdEmpleadoResponsable INT           NOT NULL,
    PRIMARY KEY (IdDetalleVenta),
    CONSTRAINT FK_detalleventa_venta
        FOREIGN KEY (IdVenta) REFERENCES venta (IdVenta),
    CONSTRAINT FK_detalleventa_producto
        FOREIGN KEY (IdProducto) REFERENCES producto (IdProducto),
    CONSTRAINT FK_detalleventa_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- HISTORICO_PRECIO (IdProducto -> producto,
--                   IdEmpleadoResponsable -> empleado)
-- ============================================================
CREATE TABLE historico_precio
(
    IdHistoricoPrecio     INT           NOT NULL AUTO_INCREMENT,
    IdProducto            INT           NOT NULL,
    Precio                DECIMAL(12,2) NOT NULL,
    FechaInicioVigencia   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaFinVigencia      DATETIME      NULL DEFAULT NULL,
    MotivoCambio          VARCHAR(255)  NOT NULL DEFAULT '',
    FechaRegistro         DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IdEmpleadoResponsable INT           NOT NULL,
    PRIMARY KEY (IdHistoricoPrecio),
    CONSTRAINT FK_historico_producto
        FOREIGN KEY (IdProducto) REFERENCES producto (IdProducto),
    CONSTRAINT FK_historico_empleado
        FOREIGN KEY (IdEmpleadoResponsable) REFERENCES empleado (IdEmpleado)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- DATOS: 10 empleados
-- Las contraseñas se almacenan como hashes PBKDF2-HMAC-SHA256
-- (formato pbkdf2$iteraciones$sal$hash). No hay texto plano.
-- Credenciales de prueba: Administrador -> admin123 / EmpleadoVenta -> clave123
-- ============================================================
INSERT INTO empleado
    (Nombre, Apellido, Rol, Telefono, CorreoElectronico, Contrasena, Estado, IdEmpleadoResponsable)
VALUES
    ('Joel',   'Ríos',      'Administrador', '70000001', 'joel.rios@ferreteria.com',    'pbkdf2$100000$i6K/10J1uVUy7aaxu05n6Q==$3D4C9TUHOOT08JtcWtVN9msWrt9otzyRNaXCCeEPNzk=',   1, NULL);

INSERT INTO empleado
    (Nombre, Apellido, Rol, Telefono, CorreoElectronico, Contrasena, Estado, IdEmpleadoResponsable)
VALUES
    ('María',    'López',     'EmpleadoVenta', '70000002', 'maria.lopez@ferreteria.com',    'pbkdf2$100000$EiFO247GganlHDHRxibZDw==$ovflhHg31hgKKpNQZVQ3tZjGB1w/w8XDZrnb5axXvm4=', 1, 1),
    ('Carlos',   'Mendoza',   'EmpleadoVenta', '70000003', 'carlos.mendoza@ferreteria.com', 'pbkdf2$100000$ayUjMsgyfL17cWAkvjwSiA==$J3qQUfUFBYMAB3ylW0NOgMvAifND8c7EbnxWFnPPKYM=', 1, 1),
    ('Ana',      'Gutiérrez', 'EmpleadoVenta',   '70000004', 'ana.gutierrez@ferreteria.com',  'pbkdf2$100000$KXtZXQhBSn5/LDoMSA1J/w==$48er+pEZ2cVPdYmKYZ6CsabCgwesfQWZkNmI86CjX+A=', 1, 1),
    ('Luis',     'Fernández', 'EmpleadoVenta',  '70000005', 'luis.fernandez@ferreteria.com', 'pbkdf2$100000$CFNYcmqMCgdqeSA09yxzcA==$/Om+zTZhiKWd93h3XrZE7POI4QK7Dym8YOkhzwgt5Do=', 1, 1),
    ('Sofía',    'Ramírez',   'EmpleadoVenta', '70000006', 'sofia.ramirez@ferreteria.com',  'pbkdf2$100000$QlYdGoL14VOiIwBvuXr1SA==$xWtOKNuCO6Ww+Pi2ak7kylnmTrrB7crk972Z8N5AzuE=', 1, 1),
    ('Diego',    'Torres',    'EmpleadoVenta',   '70000007', 'diego.torres@ferreteria.com',   'pbkdf2$100000$kNf5ZyOi23tYQJwSbhxbvg==$xEUvYHu4mK/gQya8j+ZIxfBAkZTbfm93bHw2Cx3HlU8=', 1, 1),
    ('Lucía',    'Vargas',    'EmpleadoVenta',  '70000008', 'lucia.vargas@ferreteria.com',   'pbkdf2$100000$72lYf8pr8D50FmGTnHaaHw==$Pym0xZNlP3gLWsolMIFE9AaYkPb+eKCdnT05Vr84jy4=', 1, 1),
    ('Pedro',    'Castillo',  'EmpleadoVenta','70000009','pedro.castillo@ferreteria.com', 'pbkdf2$100000$TJfoEKh9Yji7JlavWOwDlg==$hFZpDnK6VhKn0dyHmEY0RGX4eC4HROsAt5EdVCvmvA8=', 1, 1),
    ('Elena',    'Rojas',     'EmpleadoVenta', '70000010', 'elena.rojas@ferreteria.com',    'pbkdf2$100000$271ELSu8jKyojeVGCV5b3w==$+cyOdhoBHX5E4XGFZBOWZoEjFvNhibHGJsBrXOQ0z9U=', 1, 1);

-- ============================================================
-- DATOS: 5 clientes (IdEmpleadoResponsable -> empleado)
-- ============================================================
INSERT INTO cliente
    (Nombre, Apellido, Telefono, Estado, IdEmpleadoResponsable)
VALUES
    ('Juan',   'Pérez',    '71234567', 1, 1),
    ('María',  'Quispe',   '72345678', 1, 1),
    ('Carlos', 'Ticona',   '73456789', 1, 1),
    ('Ana',    'Mamani',   '74567890', 1, 1),
    ('Luis',   'Choque',   '75678901', 1, 1);

-- ============================================================
-- DATOS: 10 marcas
-- ============================================================
INSERT INTO marca
    (Nombre, Descripcion, PaisOrigen, Calidad, Estado, IdEmpleadoResponsable)
VALUES
    ('Bosch',       'Herramientas eléctricas',            'Alemania',    'Alta',  1, 1),
    ('Stanley',     'Herramientas manuales',              'Estados Unidos', 'Alta', 1, 1),
    ('Truper',      'Herramientas y ferreteria general',  'México',      'Media', 1, 1),
    ('DeWalt',      'Herramientas eléctricas profesionales','Estados Unidos','Alta', 1, 1),
    ('Makita',      'Herramientas eléctricas',            'Japón',       'Alta',  1, 1),
    ('Milwaukee',   'Herramientas eléctricas',            'Estados Unidos','Alta', 1, 1),
    ('3M',          'Seguridad industrial',               'Estados Unidos','Alta', 1, 1),
    ('Sika',        'Adhesivos y sellantes',              'Suiza',       'Alta',  1, 1),
    ('Fischer',     'Fijaciones y anclajes',              'Alemania',    'Alta',  1, 1),
    ('Cemex',       'Materiales de construcción',         'México',      'Alta',  1, 1);

-- ============================================================
-- DATOS: 10 categorias
-- ============================================================
INSERT INTO categoria
    (Codigo, Nombre, Descripcion, PorcentajeGanancia, Estado, IdEmpleadoResponsable)
VALUES
    ('HER-01',  'Herramientas Manuales',   'Martillos, desarmadores y llaves',   25.00, 1, 1),
    ('HER-02',  'Herramientas Eléctricas', 'Taladros, esmeriles y sierras',      30.00, 1, 1),
    ('PLM-01',  'Plomería',                'Tuberia, conexiones y griferia',     20.00, 1, 1),
    ('ELE-01',  'Electricidad',            'Cables, interruptores y contactos',  22.00, 1, 1),
    ('PIN-01',  'Pinturas',                'Pinturas, brochas y rodillos',       35.00, 1, 1),
    ('FIJ-01',  'Fijaciones y Tornillería','Tornillos, taquetes y anclajes',     40.00, 1, 1),
    ('SEG-01',  'Seguridad Industrial',    'Cascos, guantes y respiradores',     28.00, 1, 1),
    ('JAR-01',  'Jardinería',              'Herramientas y accesorios de jardín',30.00, 1, 1),
    ('CON-01',  'Construcción',            'Cemento, varilla y agregados',       18.00, 1, 1),
    ('ADP-01',  'Adhesivos y Sellantes',   'Silicona, pegamento y selladores',   32.00, 1, 1);

-- ============================================================
-- DATOS: 10 productos
-- ============================================================
INSERT INTO producto
    (IdCategoria, Codigo, Nombre, Descripcion, Marca, UnidadMedida, PrecioVenta, CantidadStock, Estado, IdEmpleadoResponsable)
VALUES
    (1,  'PROD-001', 'Martillo de una 16 oz',        'Mango de fibra de vidrio',        3, 'Pieza', 185.50,  40, 1, 1),
    (1,  'PROD-002', 'Juego de desarmadores 6 pzas', 'Puntas plana y estrella',         2, 'Juego', 320.00,  25, 1, 1),
    (2,  'PROD-003', 'Taladro percutor 650W',        'Portabrocas 1/2 pulgada',         1, 'Pieza', 1450.00, 15, 1, 1),
    (2,  'PROD-004', 'Esmeriladora angular 4-1/2"',  'Velocidad variable',              4, 'Pieza', 1680.00, 12, 1, 1),
    (2,  'PROD-005', 'Sierra circular 7-1/4"',       'Incluye disco de corte',          5, 'Pieza', 2100.00, 10, 1, 1),
    (6,  'PROD-006', 'Ancla expansiva 3/8"',         'Caja con 50 piezas',              9, 'Caja',  420.00,  30, 1, 1),
    (10, 'PROD-007', 'Silicona transparente 300ml',  'Sellador multipropósito',         8, 'Pieza', 95.00,   60, 1, 1),
    (7,  'PROD-008', 'Respirador media cara',        'Incluye dos filtros',             7, 'Pieza', 540.00,  20, 1, 1),
    (9,  'PROD-009', 'Cemento gris 50 kg',           'Uso general en construcción',    10, 'Saco',  280.00,  50, 1, 1),
    (2,  'PROD-010', 'Rotomartillo SDS Plus 800W',   'Empuñadura antivibración',        6, 'Pieza', 2650.00,  8, 1, 1);

-- ============================================================
-- DATOS: 10 proveedores
-- ============================================================
INSERT INTO proveedor
    (RazonSocial, NombreComercial, NombreContacto, Telefono, CorreoElectronico, Direccion, Estado, IdEmpleadoResponsable)
VALUES
    ('Distribuidora Ferretera del Norte S.A. de C.V.', 'FerreNorte',   'Jorge Salinas',  '81000001', 'ventas@ferrenorte.com',   'Av. Industrial 100, Monterrey', 1, 1),
    ('Herramientas Profesionales del Bajío',           'ProfesBajio',  'Rosa Herrera',   '81000002', 'contacto@profesbajio.com','Blvd. del Trabajo 250, Leon',   1, 1),
    ('Suministros Eléctricos MX',                      'ElectroSum',   'Hugo Marrero',   '81000003', 'info@electrosum.com',     'Calle Voltios 45, CDMX',         1, 1),
    ('Plomería y Tubería Nacional',                    'PlomNal',      'Sara Duarte',    '81000004', 'ventas@plomnal.com',      'Av. Canales 88, Guadalajara',   1, 1),
    ('Pinturas y Acabados del Sur',                    'PintuSur',     'Mario Cano',     '81000005', 'pedidos@pintusur.com',    'Calle Color 12, Puebla',         1, 1),
    ('Fijaciones Técnicas del Centro',                 'FijaTec',      'Nadia Ríos',     '81000006', 'contacto@fijatec.com',    'Av. Tornillos 300, Queretaro',  1, 1),
    ('Seguridad Industrial Integral',                  'SeguridadInt', 'Raúl Pineda',    '81000007', 'ventas@seguridadint.com', 'Blvd. Proteccion 77, Tijuana',  1, 1),
    ('Adhesivos y Químicos del Golfo',                 'AdhiGolfo',    'Irene Solís',    '81000008', 'info@adhigolfo.com',      'Zona Portuaria 21, Veracruz',   1, 1),
    ('Materiales de Construcción del Pacífico',        'ConsPacifico', 'Tomás Lira',     '81000009', 'ventas@conspacifico.com', 'Av. Concreto 500, Mazatlan',    1, 1),
    ('Importadora de Herramientas Globales',           'ImportGlobal', 'Karla Mena',     '81000010', 'contacto@importglobal.com','Calle Comercio 9, Merida',     1, 1);

-- ============================================================
-- 2. Seguridad: indice unico de correo para empleados
-- ============================================================

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

-- ============================================================
-- 3. Bitacora de sesiones (login/logout)
-- ============================================================

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
--   TipoUsuario   -> 'Administrador' o 'EmpleadoVenta'
--   FechaHora     -> momento exacto del evento
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS bitacora_sesion
(
    IdBitacora    INT          NOT NULL AUTO_INCREMENT,
    Accion        VARCHAR(20)  NOT NULL COMMENT 'login | logout',
    IdUsuario     INT          NULL DEFAULT NULL,
    NombreUsuario VARCHAR(150) NULL DEFAULT NULL,
    TipoUsuario   VARCHAR(50)  NOT NULL COMMENT 'Administrador | EmpleadoVenta',
    FechaHora     DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (IdBitacora),
    KEY IDX_BITACORA_FECHA (FechaHora),
    KEY IDX_BITACORA_USUARIO (TipoUsuario, IdUsuario)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

-- ============================================================
-- Verificacion rapida
-- ============================================================
SELECT COUNT(*) AS total_empleados FROM empleado;
SELECT COUNT(*) AS total_clientes FROM cliente;
SELECT COUNT(*) AS total_productos FROM producto;
SELECT COUNT(*) AS total_ventas FROM venta;
SELECT COUNT(*) AS total_bitacora FROM bitacora_sesion;
