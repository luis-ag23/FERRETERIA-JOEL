-- Seeder: tabla marca y su uso exclusivo en la tabla producto
-- Base de datos: bdferreteria
-- Uso: mysql -u root -p bdferreteria < Scripts/marcas.sql

CREATE TABLE IF NOT EXISTS marca
(
    IdMarca           INT            NOT NULL AUTO_INCREMENT,
    Nombre            VARCHAR(100)   NOT NULL,
    Estado            TINYINT        NOT NULL DEFAULT 1,
    FechaRegistro     DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion DATETIME      NULL DEFAULT NULL,
    PRIMARY KEY  (IdMarca),
    UNIQUE KEY   UNIQUE_NOMBRE_MARCA (Nombre)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

INSERT IGNORE INTO marca (Nombre, Estado) VALUES
    ('Sin marca',      1),
    ('Bosch',          1),
    ('Stanley',        1),
    ('Truper',         1),
    ('DeWalt',         1),
    ('Makita',         1),
    ('Milwaukee',      1),
    ('Craftsman',      1),
    ('Black+Decker',   1),
    ('Hitachi',        1),
    ('Hilti',          1),
    ('3M',             1),
    ('Teflon',         1),
    ('Sika',           1),
    ('Mapei',          1),
    ('Arauco',         1),
    ('Cemex',          1),
    ('Fischer',        1),
    ('Rothenberger',   1),
    ('Gedore',         1);

SET @columna_marca := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME   = 'producto'
      AND COLUMN_NAME  = 'Marca');

SET @sql := IF(
    @columna_marca = 0,
    'ALTER TABLE producto ADD COLUMN Marca INT NOT NULL DEFAULT 1 AFTER Descripcion',
    'SELECT ''producto.Marca ya existe'' AS estado');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @fk_marca := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME        = 'producto'
      AND CONSTRAINT_NAME   = 'FK_producto_marca');

SET @sql := IF(
    @fk_marca = 0,
    'ALTER TABLE producto ADD CONSTRAINT FK_producto_marca FOREIGN KEY (Marca) REFERENCES marca (IdMarca)',
    'SELECT ''FK_producto_marca ya existe'' AS estado');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT IdMarca, Nombre, Estado FROM marca ORDER BY IdMarca;
