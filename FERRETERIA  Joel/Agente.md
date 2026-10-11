Actúa como un arquitecto de software senior especializado en **C#, .NET, ASP.NET Core Razor Pages, arquitectura hexagonal, Clean Architecture, SOLID, Dependency Inversion, Repository Pattern y Factory Pattern**.

Voy a proporcionarte un proyecto existente en C#/.NET que actualmente funciona, pero cuya estructura necesita ser reorganizada para implementar correctamente una **Arquitectura Hexagonal mediante varios proyectos `.csproj` dentro de una misma solución**.

Tu trabajo NO consiste en rehacer el sistema desde cero.

Tu trabajo consiste en:

- analizar todo el proyecto existente;
- conservar su comportamiento actual;
- conservar sus patrones existentes;
- reorganizar correctamente sus responsabilidades;
- crear varios proyectos `.csproj`;
- establecer correctamente las dependencias;
- aplicar arquitectura hexagonal;
- conservar específicamente el patrón Factory existente;
- comprobar que toda la solución compile correctamente.

# 1. CONTEXTO ARQUITECTÓNICO QUE DEBES SEGUIR

Tengo como referencia académica un proyecto llamado `SistemaVentas` que utiliza arquitectura hexagonal mediante varios proyectos dentro de una misma solución.

No tendrás acceso necesariamente a ese proyecto, por lo tanto te explico exactamente su filosofía y estructura para que la reproduzcas correctamente en el proyecto que te proporcionaré.

La solución de referencia se organizaba conceptualmente de esta manera:

```text
SistemaVentas.sln
│
├── SistemaVentas.Dominio
│   ├── Entidades
│   │   └── Cliente.cs
│   │
│   └── Puertos
│       └── IClienteRepositorio.cs
│
├── SistemaVentas.Aplicacion
│   └── Servicios
│       └── ServicioCliente.cs
│
├── SistemaVentas.Infraestructura
│   └── Repositorios
│       └── RepositorioCliente.cs
│
└── SistemaVentas.Web
    ├── Pages
    ├── Program.cs
    ├── wwwroot
    └── configuración ASP.NET
```

La idea fundamental era:

```text
WEB
 ↓
APLICACION
 ↓
DOMINIO
 ↑
INFRAESTRUCTURA
```

Pero debes diferenciar correctamente el flujo de ejecución de las dependencias de código.

Las dependencias entre proyectos deben respetar principalmente:

```text
Aplicacion ──────────→ Dominio

Infraestructura ─────→ Dominio

Web ─────────────────→ Aplicacion
Web ─────────────────→ Infraestructura
```

Dominio debe ser completamente independiente.

# 2. REGLA MÁS IMPORTANTE

El proyecto:

```text
Dominio
```

es el núcleo del sistema.

Por lo tanto:

```text
Dominio
```

NO puede depender de:

```text
Aplicacion
Infraestructura
Web
MySQL
ASP.NET Core
Razor Pages
IConfiguration
MySql.Data
implementaciones concretas
Factories de Infraestructura
```

Es decir, está TERMINANTEMENTE PROHIBIDO crear dependencias como:

```text
Dominio → Aplicacion       ❌
Dominio → Infraestructura  ❌
Dominio → Web              ❌
```

Sin embargo, esto SÍ está permitido y es precisamente lo esperado:

```text
Aplicacion → Dominio        ✅

Infraestructura → Dominio   ✅
```

Porque las capas externas pueden utilizar el núcleo.

También está prohibido:

```text
Aplicacion → Infraestructura ❌
Aplicacion → Web             ❌

Infraestructura → Aplicacion ❌
Infraestructura → Web        ❌
```

Web funcionará como punto de composición y por eso podrá conocer:

```text
Web → Aplicacion
Web → Infraestructura
```

Y solamente cuando sea realmente necesario:

```text
Web → Dominio
```

# 3. OBJETIVO FINAL

Quiero pasar de una estructura similar a esta:

```text
ProyectoActual
│
├── Models
├── Repositories
├── Factories
├── Validaciones
├── ConfiguracionValidacion
├── Helpers
├── Pages
├── wwwroot
├── Program.cs
└── ProyectoActual.csproj
```

a una solución real como:

```text
ProyectoActual.sln
│
├── ProyectoActual.Dominio
│   └── ProyectoActual.Dominio.csproj
│
├── ProyectoActual.Aplicacion
│   └── ProyectoActual.Aplicacion.csproj
│
├── ProyectoActual.Infraestructura
│   └── ProyectoActual.Infraestructura.csproj
│
└── ProyectoActual.Web
    └── ProyectoActual.Web.csproj
```

IMPORTANTE:

NO quiero esto:

```text
ProyectoActual.Web
│
├── Dominio
├── Aplicacion
├── Infraestructura
└── Pages
```

porque eso sigue siendo UN SOLO PROYECTO con carpetas.

Quiero cuatro proyectos `.csproj` reales dentro de la misma solución.

# 4. NO MODIFICAR EL COMPORTAMIENTO DEL SISTEMA

La refactorización debe ser principalmente arquitectónica.

NO debes cambiar innecesariamente:

- comportamiento del sistema;
- estructura de la base de datos;
- nombres de tablas;
- campos;
- procedimientos existentes;
- consultas SQL;
- nombres importantes del dominio;
- funcionalidades CRUD;
- páginas Razor;
- rutas;
- diseño HTML;
- CSS;
- JavaScript;
- Bootstrap;
- validaciones existentes;
- mensajes;
- histórico de precios;
- protección de IDs o tokens de URL;
- configuración MySQL;
- cadena de conexión;
- Factory Pattern existente.

No reemplaces MySQL por Entity Framework.

Si actualmente se utiliza `MySql.Data`, debe seguir utilizándose.

No elimines funcionalidades.

No inventes funcionalidades nuevas.

# 5. ANALIZA PRIMERO EL PROYECTO ACTUAL

Antes de mover archivos o escribir código:

1. Recorre toda la solución.
2. Lee todos los `.csproj`.
3. Lee `Program.cs`.
4. Revisa todas las entidades/modelos.
5. Revisa todos los repositorios.
6. Revisa todas las interfaces.
7. Revisa todas las Factories.
8. Revisa todas las validaciones.
9. Revisa todos los Helpers.
10. Revisa las Razor PageModels `.cshtml.cs`.
11. Revisa las páginas `.cshtml`.
12. Revisa cómo se crea la conexión MySQL.
13. Revisa cómo se crean actualmente los repositorios.
14. Revisa Dependency Injection.
15. Revisa las relaciones entre clases.
16. Revisa qué clases dependen de cuáles.

NO empieces a mover archivos hasta comprender cómo funciona el proyecto.

# 6. ENTIDADES DEL PROYECTO ACTUAL

El proyecto actual contiene entidades/modelos equivalentes a:

```text
Categoria
Producto
Proveedor
Empleado
HistoricoPrecio
```

Debes conservarlas.

Estas entidades deberán pertenecer principalmente a:

```text
ProyectoActual.Dominio
└── Entidades
```

Ejemplo:

```text
ProyectoActual.Dominio
│
└── Entidades
    ├── Categoria.cs
    ├── Producto.cs
    ├── Proveedor.cs
    ├── Empleado.cs
    └── HistoricoPrecio.cs
```

Si al analizar el proyecto encuentras otras entidades reales, debes clasificarlas de la misma manera.

NO inventes entidades.

# 7. PROYECTO DOMINIO

Crear:

```text
ProyectoActual.Dominio
```

como una:

```text
Class Library
```

Debe contener el núcleo del sistema.

Estructura aproximada:

```text
ProyectoActual.Dominio
│
├── Entidades
│
├── Puertos
│
├── Validaciones
│
└── otras reglas puras de dominio si corresponde
```

## 7.1 Entidades

Mover aquí las entidades:

```text
Categoria
Producto
Proveedor
Empleado
HistoricoPrecio
```

y cualquier otra entidad real encontrada.

## 7.2 Puertos

Las interfaces que representan necesidades del sistema deben estar en Dominio.

El proyecto actual contiene contratos equivalentes a:

```text
IRepository<T>
IModificacionRepository<T>
```

Deben quedar en:

```text
Dominio/Puertos
```

Por ejemplo:

```text
ProyectoActual.Dominio
└── Puertos
    ├── IRepository.cs
    └── IModificacionRepository.cs
```

Si existen funcionalidades particulares que no pueden representarse correctamente mediante las interfaces genéricas, crea puertos específicos.

Ejemplo importante:

Si el repositorio de histórico de precios actualmente tiene operaciones como:

```text
ObtenerPorProducto
ObtenerPrecioVigente
CerrarPrecioVigente
```

puede ser necesario definir:

```text
IHistoricoPrecioRepository
```

dentro de:

```text
Dominio/Puertos
```

Por ejemplo:

```text
ProyectoActual.Dominio
└── Puertos
    └── IHistoricoPrecioRepository.cs
```

La interfaz define QUÉ necesita el sistema.

Infraestructura define CÓMO se hace con MySQL.

## 7.3 Restricciones de Dominio

Dentro de Dominio NO puede existir:

```csharp
using MySql.Data.MySqlClient;
```

Tampoco:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
```

Ni referencias a:

```text
ProyectoActual.Infraestructura
ProyectoActual.Aplicacion
ProyectoActual.Web
```

Dominio debe poder compilar independientemente.

# 8. VALIDACIONES

El proyecto actual posee validaciones equivalentes a:

```text
CategoriaValidaciones
ProductoValidaciones
ProveedorValidaciones
```

y posiblemente clases equivalentes a:

```text
ConfiguracionCategoria
ConfiguracionProducto
ConfiguracionProveedor
```

Debes analizarlas individualmente.

REGLA:

Si representan reglas puras del negocio y no dependen de ASP.NET, MySQL ni infraestructura, pueden ubicarse en:

```text
ProyectoActual.Dominio
└── Validaciones
```

Pero NO debes colocar automáticamente cualquier clase llamada `Configuracion...` en Dominio.

Primero analiza sus dependencias.

Si una clase de configuración utiliza:

```text
ASP.NET
Dependency Injection
IConfiguration
MySQL
Razor
DataAnnotations específicas de presentación
```

debe ubicarse en la capa apropiada y NO contaminar Dominio.

La clasificación debe hacerse por responsabilidad y dependencia, no solamente por el nombre de la carpeta original.

# 9. PROYECTO APLICACION

Crear:

```text
ProyectoActual.Aplicacion
```

como:

```text
Class Library
```

Debe tener ProjectReference solamente hacia:

```text
ProyectoActual.Dominio
```

Conceptualmente:

```text
Aplicacion
      ↓
Dominio
```

Aplicacion NO debe depender de:

```text
Infraestructura
Web
MySql.Data
MySqlConnection
MySqlCommand
MySqlDataReader
Razor Pages
PageModel
```

# 10. SERVICIOS / CASOS DE USO

Siguiendo la filosofía de `SistemaVentas`, donde existía:

```text
SistemaVentas.Aplicacion
└── Servicios
    └── ServicioCliente.cs
```

en este proyecto debes crear los servicios necesarios para las entidades reales.

Probablemente:

```text
ProyectoActual.Aplicacion
└── Servicios
    ├── ServicioProducto.cs
    ├── ServicioCategoria.cs
    ├── ServicioProveedor.cs
    ├── ServicioEmpleado.cs
    └── ServicioHistoricoPrecio.cs
```

Los nombres pueden adaptarse a las convenciones reales del proyecto.

Estos servicios representan casos de uso de la aplicación.

Ejemplos:

```text
listar productos
obtener producto
crear producto
editar producto
cambiar estado de producto

listar categorías
crear categoría
editar categoría

listar proveedores
registrar proveedor
editar proveedor

obtener histórico de precios
registrar un nuevo precio
obtener precio vigente
cerrar precio vigente
```

# 11. LOS SERVICIOS NO PUEDEN CONOCER MYSQL

CORRECTO:

```text
ServicioProducto
        ↓
IRepository<Producto>
```

porque:

```text
IRepository<Producto>
```

pertenece a Dominio.

INCORRECTO:

```text
ServicioProducto
        ↓
MySqlProductoRepository
```

porque eso generaría:

```text
Aplicacion → Infraestructura
```

y esa dependencia está prohibida.

También está prohibido dentro de Aplicacion:

```csharp
new MySqlProductoRepository(...);
```

o:

```csharp
using ProyectoActual.Infraestructura;
```

# 12. PROYECTO INFRAESTRUCTURA

Crear:

```text
ProyectoActual.Infraestructura
```

como:

```text
Class Library
```

Debe depender de:

```text
ProyectoActual.Dominio
```

Es decir:

```text
Infraestructura
       ↓
Dominio
```

Esto es correcto.

Infraestructura podrá utilizar:

```text
Entidades
Puertos
Interfaces
```

del Dominio.

# 13. REPOSITORIOS MYSQL

Mover los repositorios concretos existentes a:

```text
ProyectoActual.Infraestructura
└── Repositorios
```

Por ejemplo:

```text
MySqlCategoriaRepository
MySqlEmpleadoRepository
MySqlHistoricoPrecioRepository
MySqlProductoRepository
MySqlProveedorRepository
```

Cada repositorio debe implementar el puerto correspondiente definido en Dominio.

Ejemplo:

```text
Dominio
IRepository<Producto>
        ▲
        │ implementa
        │
Infraestructura
MySqlProductoRepository
```

La dependencia debe ser:

```text
MySqlProductoRepository
        ↓
IRepository<Producto>
```

Nunca al contrario.

# 14. MYSQL SOLO EN INFRAESTRUCTURA

La dependencia:

```text
MySql.Data
```

debe estar encapsulada principalmente dentro de:

```text
ProyectoActual.Infraestructura
```

Clases como:

```text
MySqlConnection
MySqlCommand
MySqlDataReader
```

NO deben aparecer dentro de:

```text
Dominio
Aplicacion
```

Las consultas SQL deben permanecer dentro de Infraestructura.

# 15. CONEXIÓN A BASE DE DATOS

El proyecto actual posee clases equivalentes a:

```text
IDbConnectionFactory
MySqlConnectionFactory
```

Debes conservarlas.

Organízalas en:

```text
ProyectoActual.Infraestructura
└── Conexion
    ├── IDbConnectionFactory.cs
    └── MySqlConnectionFactory.cs
```

Si `IDbConnectionFactory` depende directamente de tipos MySQL o `System.Data`, trátalo como una abstracción técnica de Infraestructura.

NO tienes que mover todas las interfaces obligatoriamente a Dominio.

Una interfaz solamente debe estar en Dominio si representa un puerto necesario por el núcleo.

Una abstracción puramente técnica para crear conexiones puede permanecer en Infraestructura.

# 16. MUY IMPORTANTE — MANTENER FACTORY PATTERN

El proyecto actual ya utiliza Factory Pattern.

DEBE CONSERVARSE.

NO debes eliminarlo.

NO debes sustituirlo completamente por Dependency Injection.

Arquitectura Hexagonal y Factory Pattern pueden coexistir perfectamente.

El proyecto actual posee clases equivalentes a:

```text
RepositoryCreator
CategoriaRepositoryCreator
ProductoRepositoryCreator
ProveedorRepositoryCreator
EmpleadoRepositoryCreator
HistoricoPrecioRepositoryCreator
```

Debes conservarlas.

Moverlas a:

```text
ProyectoActual.Infraestructura
└── Factories
```

Resultado aproximado:

```text
ProyectoActual.Infraestructura
│
├── Conexion
│   ├── IDbConnectionFactory.cs
│   └── MySqlConnectionFactory.cs
│
├── Repositorios
│   ├── MySqlCategoriaRepository.cs
│   ├── MySqlProductoRepository.cs
│   ├── MySqlProveedorRepository.cs
│   ├── MySqlEmpleadoRepository.cs
│   └── MySqlHistoricoPrecioRepository.cs
│
└── Factories
    ├── RepositoryCreator.cs
    ├── CategoriaRepositoryCreator.cs
    ├── ProductoRepositoryCreator.cs
    ├── ProveedorRepositoryCreator.cs
    ├── EmpleadoRepositoryCreator.cs
    └── HistoricoPrecioRepositoryCreator.cs
```

# 17. FUNCIONAMIENTO DE LAS FACTORIES

Los Factories pueden conocer repositorios concretos porque ambos pertenecen a Infraestructura.

Esto es válido:

```text
Infraestructura
│
├── ProductoRepositoryCreator
│       ↓
│   MySqlProductoRepository
```

También es válido que una Factory retorne una interfaz del Dominio.

Ejemplo conceptual:

```csharp
public class ProductoRepositoryCreator
{
    public IRepository<Producto> CrearRepositorio(...)
    {
        return new MySqlProductoRepository(...);
    }
}
```

Aquí:

```text
ProductoRepositoryCreator
```

pertenece a Infraestructura.

```text
MySqlProductoRepository
```

pertenece a Infraestructura.

```text
IRepository<Producto>
```

pertenece a Dominio.

Esto es correcto porque:

```text
Infraestructura → Dominio
```

está permitido.

# 18. DOMINIO NO CONOCE LOS FACTORIES

Está prohibido hacer algo como:

```text
Dominio
    ↓
ProductoRepositoryCreator
```

Los Factories son detalles externos.

Dominio no debe saber que existen.

# 19. NO ELIMINAR LOS FACTORIES POR USAR DI

ASP.NET Core utiliza Dependency Injection.

Eso NO significa que debas borrar los Factories.

Quiero mantener:

```text
Arquitectura Hexagonal
+
Repository Pattern
+
Factory Pattern
+
Dependency Injection cuando corresponda
```

Analiza la responsabilidad actual de cada Factory y consérvala.

Si necesitas adaptar su constructor o namespaces debido a la división de proyectos, hazlo.

Pero no los elimines.

# 20. PROYECTO WEB

Crear o convertir el proyecto ejecutable en:

```text
ProyectoActual.Web
```

Debe ser el proyecto ASP.NET Core Razor Pages.

Aquí deben quedar:

```text
Pages
wwwroot
Program.cs
appsettings.json
appsettings.Development.json
Properties
Helpers exclusivos de Web
```

Conservar:

```text
.cshtml
.cshtml.cs
CSS
JavaScript
Bootstrap
imágenes
layout
rutas
diseño
```

# 21. WEB ES EL COMPOSITION ROOT

Web es el punto donde se ensamblan las dependencias.

Por ello puede depender de:

```text
Aplicacion
Infraestructura
```

y, si realmente lo necesita:

```text
Dominio
```

La estructura de dependencias puede ser:

```text
ProyectoActual.Web
       │
       ├────────→ ProyectoActual.Aplicacion
       │
       ├────────→ ProyectoActual.Infraestructura
       │
       └────────→ ProyectoActual.Dominio
                  solo si es necesario
```

# 22. LAS RAZOR PAGES DEBEN USAR APLICACION

Actualmente algunas Razor Pages pueden utilizar repositorios directamente.

Quiero reducir ese acoplamiento.

Ejemplo incorrecto:

```text
ProductosModel
      ↓
MySqlProductoRepository
```

El objetivo debe ser:

```text
ProductosModel
      ↓
ServicioProducto
      ↓
IRepository<Producto>
```

Mientras Infraestructura proporciona:

```text
MySqlProductoRepository
      ↑
implementa
      │
IRepository<Producto>
```

# 23. RESPONSABILIDAD DE LAS PAGEMODELS

Las clases `.cshtml.cs` deberían encargarse principalmente de:

```text
recibir parámetros HTTP
model binding
validar ModelState
llamar al servicio/caso de uso correspondiente
preparar información para la vista
redireccionar
mostrar mensajes
manejar comportamiento específico de Razor
```

No deben encargarse directamente de:

```text
abrir conexiones MySQL
crear MySqlCommand
ejecutar SQL
crear MySqlDataReader
implementar lógica de persistencia
```

# 24. NO LLEVAR TODO A LOS SERVICIOS SIN CRITERIO

No quiero servicios artificiales creados únicamente para poder decir que existe una capa Aplicacion.

Extrae hacia Aplicacion solamente la lógica correspondiente a casos de uso y coordinación del sistema.

La lógica puramente visual debe seguir en Web.

La lógica de persistencia debe ir a Infraestructura.

Las reglas puras del negocio deben ir a Dominio.

# 25. PROBLEMA ESPECIAL: URLPROTECTOR

El proyecto actual puede tener un Helper similar a:

```text
UrlProtector
```

y algunas entidades pueden estar utilizando directamente esa funcionalidad para generar identificadores o tokens protegidos.

Debes revisar esto cuidadosamente.

Una entidad dentro de Dominio NO puede depender de:

```text
Web.Helpers.UrlProtector
```

Eso generaría:

```text
Dominio → Web
```

lo cual está prohibido.

Por tanto, si actualmente tienes algo conceptualmente parecido a:

```csharp
public string UrlToken => UrlProtector.Protect(Id);
```

dentro de una entidad del Dominio, debes reorganizarlo.

NO elimines la funcionalidad.

El token protegido debe seguir funcionando exactamente como antes.

Pero su generación debe estar en una capa adecuada.

Por ejemplo:

```text
Web
├── Helpers
│   └── UrlProtector
│
└── ViewModels / PageModels
    └── generación de UrlToken
```

o mediante otra solución compatible con la arquitectura.

El Dominio debe quedarse con datos puros como:

```text
Id
Nombre
Descripcion
Precio
Estado
etc.
```

sin conocer detalles Web.

# 26. PUERTOS Y ADAPTADORES

Quiero que interpretes correctamente la arquitectura hexagonal.

Los puertos son contratos.

Por ejemplo:

```text
Dominio
└── Puertos
    └── IRepository<Producto>
```

Infraestructura proporciona un adaptador:

```text
Infraestructura
└── Repositorios
    └── MySqlProductoRepository
```

Por tanto:

```text
PUERTO
IRepository<Producto>
        ▲
        │ implementa
        │
ADAPTADOR
MySqlProductoRepository
```

Lo mismo debe aplicarse cuando existan operaciones específicas.

# 27. DEPENDENCIAS EXACTAS ENTRE CSPROJ

Antes de terminar, revisa físicamente los `<ProjectReference>`.

## ProyectoActual.Dominio.csproj

Debe tener:

```text
NINGUNA referencia a:

ProyectoActual.Aplicacion
ProyectoActual.Infraestructura
ProyectoActual.Web
```

Conceptualmente:

```xml
<Project>
    <!-- No ProjectReference hacia capas externas -->
</Project>
```

## ProyectoActual.Aplicacion.csproj

Debe contener:

```xml
<ItemGroup>
  <ProjectReference Include="..\ProyectoActual.Dominio\ProyectoActual.Dominio.csproj" />
</ItemGroup>
```

Y NO Infraestructura.

## ProyectoActual.Infraestructura.csproj

Debe contener:

```xml
<ItemGroup>
  <ProjectReference Include="..\ProyectoActual.Dominio\ProyectoActual.Dominio.csproj" />
</ItemGroup>
```

Y NO Aplicacion ni Web.

## ProyectoActual.Web.csproj

Puede contener:

```xml
<ItemGroup>
  <ProjectReference Include="..\ProyectoActual.Aplicacion\ProyectoActual.Aplicacion.csproj" />
  <ProjectReference Include="..\ProyectoActual.Infraestructura\ProyectoActual.Infraestructura.csproj" />
</ItemGroup>
```

Y solamente si es necesario:

```xml
<ProjectReference Include="..\ProyectoActual.Dominio\ProyectoActual.Dominio.csproj" />
```

# 28. ESTA MATRIZ ES OBLIGATORIA

Utiliza esta matriz para validar las dependencias:

| Desde | Hacia Dominio | Hacia Aplicacion | Hacia Infraestructura | Hacia Web |
|---|---|---|---|---|
| Dominio | — | ❌ | ❌ | ❌ |
| Aplicacion | ✅ | — | ❌ | ❌ |
| Infraestructura | ✅ | ❌ | — | ❌ |
| Web | ✅ si hace falta | ✅ | ✅ | — |

No rompas esta regla.

# 29. EVITAR DEPENDENCIAS CIRCULARES

Comprueba que no aparezca ninguna situación como:

```text
Dominio → Aplicacion → Dominio
```

o:

```text
Aplicacion → Infraestructura → Aplicacion
```

o:

```text
Infraestructura → Web → Infraestructura
```

No debe existir ninguna dependencia circular.

# 30. DEPENDENCY INJECTION

Puedes utilizar Dependency Injection de ASP.NET Core desde Web para registrar servicios de Aplicacion y componentes necesarios.

Ejemplo:

```csharp
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
```

Pero recuerda:

NO debes eliminar los Factories existentes solamente para reemplazarlos con:

```csharp
AddScoped<IRepository<Producto>, MySqlProductoRepository>();
```

si eso destruye el patrón Factory que ya forma parte del proyecto.

Analiza cómo trabajan actualmente las Factories y haz que puedan coexistir con DI.

# 31. EJEMPLO DEL RESULTADO ARQUITECTÓNICO

La estructura podría quedar conceptualmente así:

```text
ProyectoActual.sln
│
├── ProyectoActual.Dominio
│   │
│   ├── Entidades
│   │   ├── Categoria.cs
│   │   ├── Producto.cs
│   │   ├── Proveedor.cs
│   │   ├── Empleado.cs
│   │   └── HistoricoPrecio.cs
│   │
│   ├── Puertos
│   │   ├── IRepository.cs
│   │   ├── IModificacionRepository.cs
│   │   └── IHistoricoPrecioRepository.cs
│   │
│   └── Validaciones
│       ├── CategoriaValidaciones.cs
│       ├── ProductoValidaciones.cs
│       └── ProveedorValidaciones.cs
│
├── ProyectoActual.Aplicacion
│   │
│   └── Servicios
│       ├── ServicioCategoria.cs
│       ├── ServicioProducto.cs
│       ├── ServicioProveedor.cs
│       ├── ServicioEmpleado.cs
│       └── ServicioHistoricoPrecio.cs
│
├── ProyectoActual.Infraestructura
│   │
│   ├── Conexion
│   │   ├── IDbConnectionFactory.cs
│   │   └── MySqlConnectionFactory.cs
│   │
│   ├── Repositorios
│   │   ├── MySqlCategoriaRepository.cs
│   │   ├── MySqlProductoRepository.cs
│   │   ├── MySqlProveedorRepository.cs
│   │   ├── MySqlEmpleadoRepository.cs
│   │   └── MySqlHistoricoPrecioRepository.cs
│   │
│   └── Factories
│       ├── RepositoryCreator.cs
│       ├── CategoriaRepositoryCreator.cs
│       ├── ProductoRepositoryCreator.cs
│       ├── ProveedorRepositoryCreator.cs
│       ├── EmpleadoRepositoryCreator.cs
│       └── HistoricoPrecioRepositoryCreator.cs
│
└── ProyectoActual.Web
    │
    ├── Pages
    │   ├── Categorias
    │   ├── Productos
    │   ├── Proveedores
    │   └── ...
    │
    ├── Helpers
    │   └── UrlProtector.cs
    │
    ├── wwwroot
    │
    ├── Properties
    │
    ├── Program.cs
    │
    ├── appsettings.json
    │
    └── appsettings.Development.json
```

Esto es una referencia.

Debes adaptarlo a los archivos REALES que encuentres.

NO inventes archivos innecesarios.

# 32. REGLA DE CLASIFICACIÓN DE ARCHIVOS

Ante cualquier archivo debes preguntarte:

### ¿Es una entidad o regla central del negocio?

Entonces:

```text
Dominio
```

### ¿Es un contrato que el núcleo necesita para comunicarse con el exterior?

Entonces probablemente:

```text
Dominio/Puertos
```

### ¿Es un caso de uso o coordinación de operaciones?

Entonces:

```text
Aplicacion
```

### ¿Contiene MySQL, SQL, repositorios concretos, creación de conexiones o Factories de persistencia?

Entonces:

```text
Infraestructura
```

### ¿Contiene HTTP, Razor Pages, HTML, CSS, ViewModels o comportamiento específico de UI?

Entonces:

```text
Web
```

No clasifiques simplemente según el nombre actual de la carpeta.

Analiza la responsabilidad real del archivo.

# 33. NO DUPLICAR CLASES

Cuando muevas una clase:

NO dejes también la versión antigua.

Por ejemplo, no quiero:

```text
ProyectoAntiguo/Models/Producto.cs
```

y simultáneamente:

```text
ProyectoActual.Dominio/Entidades/Producto.cs
```

Debe existir una sola fuente válida.

# 34. NAMESPACES

Después de mover todos los archivos, actualiza correctamente todos los namespaces.

Ejemplos:

```csharp
namespace ProyectoActual.Dominio.Entidades;
```

```csharp
namespace ProyectoActual.Dominio.Puertos;
```

```csharp
namespace ProyectoActual.Aplicacion.Servicios;
```

```csharp
namespace ProyectoActual.Infraestructura.Repositorios;
```

```csharp
namespace ProyectoActual.Infraestructura.Factories;
```

```csharp
namespace ProyectoActual.Infraestructura.Conexion;
```

Utiliza el nombre real de la solución.

# 35. NO USAR BIN, OBJ NI .VS

No copies ni organices como parte de la solución:

```text
bin
obj
.vs
```

Son archivos generados.

# 36. CONSERVAR TARGET FRAMEWORK

Mantén el Target Framework actual del proyecto.

Si actualmente utiliza:

```text
.NET 10
```

continúa utilizando `.NET 10`, salvo que exista una incompatibilidad técnica real.

No cambies la versión arbitrariamente.

# 37. NO REESCRIBIR LOS REPOSITORIOS SIN NECESIDAD

Si los repositorios actuales ya funcionan correctamente:

```text
MySqlProductoRepository
MySqlCategoriaRepository
MySqlProveedorRepository
etc.
```

no reescribas todas las consultas SQL simplemente por la refactorización.

Principalmente:

- muévelos;
- adapta namespaces;
- adapta interfaces;
- adapta constructores;
- corrige referencias.

Cambia SQL solamente si es estrictamente necesario para que la arquitectura funcione.

# 38. PRESERVAR HISTÓRICO DE PRECIOS

Presta especial atención al flujo del histórico de precios.

Debe conservarse cualquier comportamiento relacionado con:

```text
registrar precio
consultar histórico
obtener precio vigente
cerrar precio vigente
cambiar precio
relacionar histórico con Producto
```

No simplifiques este comportamiento.

Si necesita un puerto específico:

```text
IHistoricoPrecioRepository
```

créalo correctamente.

# 39. PRESERVAR CRUD

Verifica que después de la refactorización sigan funcionando las operaciones existentes para:

```text
Productos
Categorias
Proveedores
Empleados
HistoricoPrecio
```

y cualquier otra entidad encontrada.

# 40. NO CAMBIAR LA INTERFAZ VISUAL

No quiero rediseño.

Mantén:

```text
HTML
CSS
JavaScript
Bootstrap
layouts
formularios
tablas
botones
rutas
nombres visibles
```

salvo ajustes estrictamente necesarios por namespaces/modelos.

# 41. ORDEN DE TRABAJO OBLIGATORIO

Realiza el trabajo en este orden:

1. Analizar el proyecto completo.
2. Identificar todas las responsabilidades.
3. Identificar todas las dependencias actuales.
4. Identificar interfaces y repositorios.
5. Identificar Factories.
6. Identificar lógica de negocio.
7. Identificar lógica de aplicación.
8. Identificar lógica Web.
9. Diseñar el árbol final.
10. Crear la solución/proyectos necesarios.
11. Crear `Dominio`.
12. Mover entidades.
13. Mover/crear puertos.
14. Adaptar validaciones.
15. Crear `Aplicacion`.
16. Crear servicios/casos de uso.
17. Crear `Infraestructura`.
18. Mover conexión MySQL.
19. Mover repositorios.
20. Mover Factories.
21. Adaptar Factory Pattern.
22. Adaptar Web.
23. Modificar las PageModels.
24. Corregir `UrlProtector`.
25. Configurar Composition Root.
26. Corregir namespaces.
27. Corregir `using`.
28. Corregir `.csproj`.
29. Comprobar dependencias.
30. Compilar.
31. Corregir errores.
32. Volver a compilar.
33. Verificar arquitectura.
34. Entregar resultado.

# 42. COMPILACIÓN OBLIGATORIA

Antes de considerar terminado el trabajo ejecuta:

```bash
dotnet build
```

sobre la solución completa.

No consideres terminada la tarea simplemente porque moviste los archivos.

Debes resolver todos los errores de:

```text
namespace
using
ProjectReference
NuGet
constructor
inyección de dependencias
interfaces
implementaciones
tipos
accesibilidad
referencias circulares
```

El resultado esperado es:

```text
Build succeeded.
0 Error(s)
```

Si existen warnings, analízalos y corrige los relacionados directamente con la refactorización cuando corresponda.

# 43. VERIFICACIÓN ARQUITECTÓNICA FINAL

Antes de finalizar revisa cada `.csproj`.

Debes poder demostrar que:

```text
Dominio
   ↑
   │
Aplicacion
```

y:

```text
Dominio
   ↑
   │
Infraestructura
```

y:

```text
        Aplicacion
            ↑
            │
Web ─────────┤
            │
        Infraestructura
```

Dicho como dependencias:

```text
Aplicacion → Dominio
Infraestructura → Dominio
Web → Aplicacion
Web → Infraestructura
```

Nunca:

```text
Dominio → Infraestructura
Dominio → Aplicacion
Dominio → Web

Aplicacion → Infraestructura

Infraestructura → Aplicacion
Infraestructura → Web
```

# 44. FACTORIES — CRITERIO DE ACEPTACIÓN OBLIGATORIO

Comprueba explícitamente que continúen existiendo y funcionando los Factories originales.

No aceptaré la refactorización si simplemente desaparecen:

```text
RepositoryCreator
CategoriaRepositoryCreator
ProductoRepositoryCreator
ProveedorRepositoryCreator
EmpleadoRepositoryCreator
HistoricoPrecioRepositoryCreator
```

Si los nombres reales son ligeramente diferentes, conserva los nombres reales.

Puedes adaptar su implementación a la nueva separación de proyectos.

Pero debes conservar el patrón.

# 45. EJEMPLO CONCEPTUAL COMPLETO

El resultado conceptual para Producto debería ser parecido a:

```text
                         DOMINIO

                   ┌──────────────────┐
                   │     Producto     │
                   └──────────────────┘

                   ┌──────────────────┐
                   │ IRepository<T>   │
                   └────────▲─────────┘
                            │
                            │ implementa
                            │
                    INFRAESTRUCTURA

              ┌──────────────────────────┐
              │ MySqlProductoRepository │
              └────────────▲─────────────┘
                           │ crea
                           │
              ┌────────────┴─────────────┐
              │ProductoRepositoryCreator│
              └──────────────────────────┘
```

Mientras Aplicacion utiliza únicamente la abstracción:

```text
APLICACION

ServicioProducto
       ↓
IRepository<Producto>
       ↓
Producto
```

Y Web utiliza:

```text
WEB

ProductosModel
      ↓
ServicioProducto
```

Esto permite conservar:

```text
Hexagonal Architecture
Repository Pattern
Factory Pattern
Dependency Inversion
Razor Pages
MySQL
```

al mismo tiempo.

# 46. NO SOBREARQUITECTURAR

Aunque quiero arquitectura hexagonal correcta, tampoco quiero que conviertas un proyecto académico relativamente pequeño en una solución innecesariamente compleja.

NO agregues sin necesidad:

```text
CQRS
MediatR
AutoMapper
Domain Events
Event Bus
RabbitMQ
microservicios
Unit of Work
Entity Framework
DDD completo
Result Pattern
repositorios adicionales
interfaces artificiales
```

salvo que ya existan en el proyecto.

El objetivo es replicar la filosofía sencilla del proyecto de referencia `SistemaVentas`:

```text
Dominio
Aplicacion
Infraestructura
Web
```

con:

```text
Entidades
Puertos
Servicios
Repositorios
Factories
Razor Pages
```

# 47. SI ENCUENTRAS ALGO QUE NO ENCAJA

No elimines archivos ni funcionalidades solo porque no sabes dónde ponerlos.

Analiza:

```text
qué responsabilidad tiene
qué dependencias utiliza
quién lo consume
qué capa debería conocerlo
```

y ubícalo donde corresponda manteniendo las reglas de dependencia.

Si necesitas realizar una pequeña adaptación para evitar una dependencia incorrecta, hazla.

# 48. CRITERIOS FINALES DE ACEPTACIÓN

Antes de decir que terminaste, verifica uno por uno:

- [ ] Existen varios `.csproj` reales.
- [ ] Existe `Dominio`.
- [ ] Existe `Aplicacion`.
- [ ] Existe `Infraestructura`.
- [ ] Existe `Web`.
- [ ] Dominio no referencia Aplicacion.
- [ ] Dominio no referencia Infraestructura.
- [ ] Dominio no referencia Web.
- [ ] Aplicacion referencia Dominio.
- [ ] Aplicacion NO referencia Infraestructura.
- [ ] Aplicacion NO referencia Web.
- [ ] Infraestructura referencia Dominio.
- [ ] Infraestructura NO referencia Aplicacion.
- [ ] Infraestructura NO referencia Web.
- [ ] Web referencia Aplicacion.
- [ ] Web puede utilizar Infraestructura.
- [ ] No existen dependencias circulares.
- [ ] Las entidades están en Dominio.
- [ ] Los puertos están correctamente definidos.
- [ ] Los repositorios MySQL están en Infraestructura.
- [ ] Las consultas SQL están encapsuladas.
- [ ] `MySql.Data` no contamina Dominio.
- [ ] `MySql.Data` no contamina Aplicacion.
- [ ] Los servicios están en Aplicacion.
- [ ] Los Factories continúan existiendo.
- [ ] Los Factories están en Infraestructura.
- [ ] Los Factories continúan creando los repositorios correspondientes.
- [ ] Repository Pattern se conserva.
- [ ] Factory Pattern se conserva.
- [ ] El histórico de precios se conserva.
- [ ] Las validaciones se conservan.
- [ ] Las páginas Razor se conservan.
- [ ] El diseño Web se conserva.
- [ ] `UrlProtector` no contamina Dominio.
- [ ] Los namespaces están corregidos.
- [ ] Los `.csproj` tienen referencias correctas.
- [ ] No hay clases duplicadas innecesariamente.
- [ ] No se modificó la base de datos.
- [ ] No se reemplazó MySQL por EF.
- [ ] Se mantiene el comportamiento original.
- [ ] La solución completa compila.
- [ ] `dotnet build` termina con 0 errores.

# 49. QUÉ DEBES MOSTRARME AL FINAL

Después de realizar la refactorización, dame un informe corto y técnico con:

## 1. Árbol final

Ejemplo:

```text
ProyectoActual.sln
├── ProyectoActual.Dominio
├── ProyectoActual.Aplicacion
├── ProyectoActual.Infraestructura
└── ProyectoActual.Web
```

mostrando también las carpetas principales.

## 2. Dependencias finales

Muéstrame:

```text
Aplicacion → Dominio
Infraestructura → Dominio
Web → Aplicacion
Web → Infraestructura
```

## 3. ProjectReference

Muéstrame qué `<ProjectReference>` tiene cada `.csproj`.

## 4. Archivos movidos

Indica:

```text
archivo anterior → nueva ubicación
```

## 5. Archivos nuevos

Indica qué servicios, puertos u otras clases fue necesario crear.

## 6. Factories

Muéstrame explícitamente que se conservaron:

```text
RepositoryCreator
CategoriaRepositoryCreator
ProductoRepositoryCreator
ProveedorRepositoryCreator
EmpleadoRepositoryCreator
HistoricoPrecioRepositoryCreator
```

o los nombres equivalentes existentes realmente en el proyecto.

## 7. Servicios

Indica qué servicios/casos de uso quedaron en Aplicacion.

## 8. Puertos

Indica qué interfaces del Dominio utiliza cada repositorio.

## 9. Cambios en Razor Pages

Explica qué lógica fue extraída de las PageModels hacia Aplicacion.

## 10. UrlProtector

Explica cómo eliminaste cualquier dependencia incorrecta de Dominio hacia Web sin perder su funcionalidad.

## 11. Compilación

Muéstrame el resultado final de:

```bash
dotnet build
```

# 50. INSTRUCCIÓN FINAL

No quiero solamente recomendaciones, diagramas o ejemplos.

Quiero que MODIFIQUES REALMENTE el proyecto que te proporcionaré.

Trabaja sobre los archivos existentes.

No rehagas el proyecto desde cero salvo los `.csproj` y estructura necesarios para separar correctamente las capas.

Utiliza el código existente como fuente de verdad.

Preserva la funcionalidad.

Preserva los repositorios.

Preserva las consultas SQL.

Preserva los Factories.

Preserva las validaciones.

Preserva Razor Pages.

Aplica arquitectura hexagonal mediante varios proyectos.

La regla fundamental que nunca debes romper es:

```text
                DOMINIO
               ↗       ↖
      APLICACION       INFRAESTRUCTURA
```

que en términos de dependencia significa:

```text
Aplicacion → Dominio
Infraestructura → Dominio
```

y NUNCA:

```text
Dominio → Aplicacion
Dominio → Infraestructura
```

Finalmente, recuerda:

**Infraestructura y Aplicacion pueden depender de Dominio para utilizar sus entidades y contratos. Dominio nunca debe depender de Infraestructura ni de Aplicacion.**

Realiza toda la refactorización, compila la solución y corrige los errores antes de finalizar.