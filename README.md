# ⚡ Z-COMMERCE — Enterprise E-Commerce Platform

![.NET](https://img.shields.io/badge/.NET%209.0-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/Neon%20Postgres-4169E1?style=for-the-badge\&logo=postgresql\&logoColor=white)
![Transbank](https://img.shields.io/badge/Webpay%20Plus-FF6C00?style=for-the-badge\&logo=cashapp\&logoColor=white)
![Brevo](https://img.shields.io/badge/Brevo%20API-00A884?style=for-the-badge\&logo=maildotru\&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap%205.3-7952B3?style=for-the-badge\&logo=bootstrap\&logoColor=white)
![Architecture](https://img.shields.io/badge/Arquitectura-MVC-008080?style=for-the-badge)
![Render](https://img.shields.io/badge/Deploy-Render-46E3B7?style=for-the-badge\&logo=render\&logoColor=black)

**Z-Commerce** es una plataforma de comercio electrónico desarrollada en **ASP.NET Core MVC**, orientada a la simulación profesional de una tienda de hardware tecnológico. El proyecto integra autenticación segura, confirmación real de correo, recuperación de contraseña, roles de usuario, panel administrativo, carrito dinámico, checkout con compra invitada o cuenta registrada, historial de compras, gestión de pedidos, persistencia en PostgreSQL Cloud y flujo de pago simulado mediante **Webpay Plus Sandbox**.

Este proyecto fue desarrollado como parte de mi portafolio profesional para demostrar conocimientos en **backend, arquitectura MVC, bases de datos relacionales, autenticación, autorización por roles, integración de APIs externas, correos transaccionales, despliegue cloud y diseño UI/UX moderno**.

---

## 🌐 Demo en Producción

🔗 **Demo:** https://z-commerce-dh8r.onrender.com/
📦 **Repositorio:** https://github.com/SebastianASR/EcommerceApp

> La aplicación está desplegada en **Render** mediante Docker y conectada a una base de datos **PostgreSQL alojada en Neon**.

---

## 📸 Vista Previa de la Interfaz

| Inicio                                                                                      | Carrito                                                                                     |
| :------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------ |
| ![Portada](https://github.com/user-attachments/assets/37009572-61a5-42ae-a574-e0a20313d865) | ![Carrito](https://github.com/user-attachments/assets/53c5bba0-938c-4868-8cdb-c9821f09e3bf) |

---

## 🔐 Accesos Demo

La aplicación incluye usuarios demo para que reclutadores, docentes o evaluadores puedan probar el sistema sin crear datos propios.

| Rol          | Usuario                   | Contraseña      | Propósito                                                                 |
| :----------- | :------------------------ | :-------------- | :------------------------------------------------------------------------ |
| Cliente Demo | `cliente@zcommerce.cl`    | `Cliente123!`   | Probar catálogo, carrito, checkout, compra con cuenta y datos precargados |
| DemoAdmin    | `demo.admin@zcommerce.cl` | `DemoAdmin123!` | Probar panel administrativo en modo seguro y limitado                     |

> El usuario **DemoAdmin** permite explorar funcionalidades del panel, pero las acciones críticas quedan bloqueadas para evitar modificaciones destructivas en la demo pública.

---

## 🚀 Características Principales

### 🛒 E-Commerce Completo

* Catálogo de productos tecnológicos.
* Carrito lateral interactivo.
* Agregar, quitar, eliminar productos y vaciar carrito.
* Contador dinámico de productos.
* Cálculo automático de subtotales y total.
* Persistencia del carrito mediante `HttpContext.Session`.

### 👤 Sistema de Usuarios con ASP.NET Core Identity

* Registro de clientes.

* Inicio y cierre de sesión.

* Confirmación real de correo electrónico.

* Recuperación de contraseña mediante token seguro.

* Reenvío de enlace de confirmación.

* Contraseñas seguras con reglas estrictas.

* Bloqueo temporal por intentos fallidos.

* Roles diferenciados:

  * `Admin`
  * `DemoAdmin`
  * `Cliente`

* Usuario personalizado `ApplicationUser` con datos adicionales:

  * Nombre
  * Apellido
  * Teléfono
  * Región
  * Comuna
  * Calle
  * Número
  * Departamento / Block / Oficina
  * Pedidos asociados

### 📧 Correos Transaccionales con Brevo API

* Envío de correo de confirmación al registrar una cuenta.
* Envío de correo para restablecer contraseña.
* Envío de nuevo enlace de confirmación si el usuario no activó su cuenta.
* Integración mediante **Brevo API** usando HTTP/HTTPS.
* Compatible con despliegue en Render Free.
* Configuración segura mediante variables de entorno.

### 🧾 Checkout Profesional

El checkout permite tres flujos principales:

1. **Compra como invitado**

   * El usuario puede comprar sin crear cuenta.
   * Debe ingresar datos de contacto y despacho.

2. **Compra creando cuenta**

   * El usuario puede marcar la opción de crear cuenta durante el checkout.
   * La cuenta queda registrada automáticamente como `Cliente`.
   * Los datos de despacho quedan asociados al usuario.

3. **Compra con cuenta registrada**

   * Si el usuario ya inició sesión, sus datos se precargan automáticamente.
   * Puede guardar cambios de dirección para futuras compras.

### 🌎 Región y Comuna Automática

* Selector de región de Chile.

* Selector de comuna dependiente de la región seleccionada.

* Implementado tanto en:

  * Registro de cliente
  * Checkout

* Mejora la experiencia del usuario y evita escribir manualmente datos sensibles de despacho.

### 💳 Integración Webpay Plus Sandbox

* Integración directa con **Webpay Plus REST API v1.2**.
* Creación de transacción.
* Redirección al formulario de pago Webpay.
* Retorno seguro desde Transbank.
* Confirmación de transacción autorizada.
* Registro del pedido en base de datos solo cuando el pago es aprobado.

> El flujo usa ambiente Sandbox de Transbank, por lo tanto no realiza cobros reales.

Datos a usar para probar flujo de compra:

```txt
Tarjeta: 4051885600446623
CVV: 123
Fecha vencimiento: cualquier fecha futura, por ejemplo 12/29
RUT autenticación: 11.111.111-1
Clave: 123
```

### 📦 Gestión de Pedidos

* Registro de pedidos en PostgreSQL.
* Asociación de pedido a usuario cuando corresponde.
* Registro de datos de despacho.
* Registro de detalle de productos comprados.
* Total de compra.
* Estado de pago.
* Estado de pedido.
* Confirmación visual posterior al pago.
* Historial de compras para clientes registrados.
* Vista de detalle de pedido con productos, cantidades, subtotales, datos de despacho e información técnica.
* Panel administrativo para visualizar compras de clientes.
* Cambio de estado de pedido para rol `Admin`.
* Eliminación de ventas para rol `Admin`.
* Modo solo lectura para `DemoAdmin`.

### 🛠️ Panel Administrativo

* Panel de gestión de productos.
* CRUD de inventario para rol administrador.
* Visualización segura para `DemoAdmin`.
* Bloqueo visual de acciones críticas para usuario demo.
* Gestión de solicitudes comerciales o leads.
* Panel de compras de clientes.
* Navegación administrativa entre:

  * Compras
  * Inventario
  * Solicitudes

### 📩 Módulo de Solicitudes / Leads

* Formulario para solicitudes especiales.

* Registro de empresas interesadas.

* Estado de solicitud:

  * Nuevo
  * Contactado
  * En evaluación
  * Cerrado

* Notas internas.

* Archivado y restauración de solicitudes.

* Exportación a Excel mediante ClosedXML.

* Navegación integrada con los módulos administrativos.

### ✅ Mensajes de Confirmación

* Mensaje visual de bienvenida al iniciar sesión.
* Mensaje visual al crear una cuenta y solicitar confirmación por correo.
* Mensaje visual al reenviar confirmación.
* Mensaje visual al solicitar recuperación de contraseña.
* Alertas tipo Bootstrap con cierre automático.
* Experiencia más cercana a una aplicación real de producción.

---

## 🛠️ Tecnologías Utilizadas

### Backend

* C#
* ASP.NET Core MVC
* ASP.NET Core Identity
* Entity Framework Core
* Npgsql Entity Framework Provider
* Webpay Plus REST API
* Brevo API
* ClosedXML

### Frontend

* Razor Views
* HTML5
* CSS3
* Bootstrap 5.3
* FontAwesome
* JavaScript Vanilla
* Fetch API

### Base de Datos

* PostgreSQL
* Neon Cloud Database

### DevOps / Deploy

* Docker
* Render
* GitHub
* Git
* Variables de entorno para configuración sensible

---

## 🧱 Arquitectura del Proyecto

El proyecto sigue el patrón **MVC**, separando responsabilidades entre modelos, vistas y controladores.

```txt
EcommerceApp/
│
├── Controllers/
│   ├── AccountController.cs
│   ├── CheckoutController.cs
│   ├── HomeController.cs
│   └── PedidosController.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Producto.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── CheckoutViewModel.cs
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ForgotPasswordViewModel.cs
│   ├── ResetPasswordViewModel.cs
│   ├── ResendEmailConfirmationViewModel.cs
│   └── SolicitudVip.cs
│
├── Views/
│   ├── Account/
│   ├── Checkout/
│   ├── Home/
│   ├── Pedidos/
│   └── Shared/
│
├── Data/
│   └── DbInitializer.cs
│
├── Services/
│   ├── BrevoEmailService.cs
│   ├── EmailSettings.cs
│   ├── IEmailService.cs
│   └── SpanishIdentityErrorDescriber.cs
│
├── Migrations/
│
├── Program.cs
└── Dockerfile
```

---

## 🔐 Seguridad Implementada

* Uso de **ASP.NET Core Identity**.
* Hash seguro de contraseñas mediante PBKDF2.
* Confirmación obligatoria de correo electrónico.
* Recuperación de contraseña mediante tokens seguros de Identity.
* Reenvío controlado de enlaces de confirmación.
* Roles y autorización por controlador.
* Bloqueo de cuenta tras múltiples intentos fallidos.
* Cookies con configuración segura.
* Uso de variables de entorno para secretos en producción.
* Archivo `appsettings.json` excluido del repositorio.
* Archivo `appsettings.Example.json` como referencia segura.
* Usuario DemoAdmin limitado para evitar operaciones destructivas en producción.
* Acciones críticas restringidas al rol `Admin`.

---

## 🧪 Flujos que se Pueden Probar

### Cliente

* Registrarse.
* Confirmar correo electrónico.
* Iniciar sesión.
* Recuperar contraseña mediante correo.
* Reenviar enlace de confirmación.
* Comprar productos.
* Usar carrito lateral.
* Ir al checkout.
* Seleccionar región y comuna.
* Pagar mediante Webpay Sandbox.
* Ver confirmación de pedido.
* Revisar historial de compras.
* Ver detalle de cada pedido.

### Invitado

* Agregar productos al carrito.
* Comprar sin cuenta.
* Ingresar datos manuales de despacho.
* Pagar mediante Webpay Sandbox.

### DemoAdmin

* Iniciar sesión con cuenta demo.
* Revisar panel administrativo.
* Visualizar inventario.
* Visualizar solicitudes/leads.
* Visualizar compras de clientes.
* Revisar detalles de pedidos.
* Probar navegación de panel sin modificar datos críticos.

### Admin

* Gestionar productos.
* Revisar solicitudes/leads.
* Visualizar compras de clientes.
* Cambiar estado de pedidos.
* Eliminar ventas registradas.
* Acceder a navegación administrativa centralizada.

---

## ⚙️ Configuración Local

### 1. Clonar el repositorio

```bash
git clone https://github.com/SebastianASR/EcommerceApp.git
cd EcommerceApp
```

### 2. Configurar `appsettings.json`

Renombra o copia el archivo de ejemplo:

```bash
cp EcommerceApp/appsettings.Example.json EcommerceApp/appsettings.json
```

Luego configura tu cadena de conexión y credenciales de correo transaccional:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "NeonConnection": "Host=TU_HOST;Database=TU_DATABASE;Username=TU_USER;Password=TU_PASSWORD;SslMode=Require;TrustServerCertificate=true"
  },
  "EmailSettings": {
    "ApiKey": "TU_API_KEY_DE_BREVO",
    "From": "correo-verificado@ejemplo.com",
    "DisplayName": "Z-Commerce"
  }
}
```

> El correo definido en `EmailSettings:From` debe estar verificado como remitente en Brevo.

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Aplicar migraciones

```bash
dotnet ef database update --project ./EcommerceApp/EcommerceApp.csproj --startup-project ./EcommerceApp/EcommerceApp.csproj
```

### 5. Ejecutar el proyecto

```bash
dotnet run --project ./EcommerceApp/EcommerceApp.csproj
```

---

## 🐳 Despliegue con Docker

El proyecto incluye un `Dockerfile` preparado para Render.

```bash
docker build -t zcommerce .
docker run -p 8080:8080 zcommerce
```

En producción, las variables sensibles deben configurarse desde el panel del proveedor cloud.

Variables recomendadas en Render:

```txt
ConnectionStrings__NeonConnection
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EmailSettings__ApiKey
EmailSettings__From
EmailSettings__DisplayName
```

---

## 💳 Webpay Plus Sandbox

El sistema usa Webpay Plus en ambiente de integración.

Flujo implementado:

```txt
Carrito → Checkout → Crear transacción Webpay → Ir a Webpay → Retorno → Commit → Confirmación → Guardado de pedido
```

El pedido solo se guarda como pagado cuando Transbank responde con estado autorizado.

---

## 📧 Brevo API

El sistema utiliza **Brevo API** para enviar correos transaccionales.

Flujos implementados:

```txt
Registro → Crear usuario → Generar token → Enviar correo → Confirmar cuenta → Login permitido
```

```txt
Olvidé mi contraseña → Generar token → Enviar correo → Restablecer contraseña → Login permitido
```

Variables necesarias:

```txt
EmailSettings__ApiKey
EmailSettings__From
EmailSettings__DisplayName
```

El remitente usado en `EmailSettings__From` debe estar verificado en Brevo para que los correos puedan enviarse correctamente.

---

## 📊 Base de Datos

La base de datos utiliza PostgreSQL con Entity Framework Core.

Tablas principales:

* `AspNetUsers`
* `AspNetRoles`
* `AspNetUserRoles`
* `Productos`
* `Pedidos`
* `PedidosDetalle`
* `SolicitudesVip`

Relaciones principales:

```txt
ApplicationUser 1 ─── N Pedidos
Pedido 1 ─── N PedidoDetalle
Producto 1 ─── N PedidoDetalle
```

---

## 📤 Exportación Excel

El módulo de solicitudes permite exportar leads a archivo `.xlsx` usando **ClosedXML**.

La exportación incluye:

* ID
* Empresa
* Correo
* Tipo de proyecto
* Estado
* Archivado
* Fecha de solicitud
* Fecha de última gestión
* Detalles
* Nota interna

---

## 🧑‍💻 Autor

Desarrollado por Sebastián Sandoval Romero
Ingeniero en Informática
Perfil orientado a desarrollo **Full-Stack**
Santiago, Chile

* GitHub: https://github.com/SebastianASR
* LinkedIn: https://www.linkedin.com/in/sebastian-andre-sandoval-romero-115710296/
* Email: [sandoval.romero.sebastian@gmail.com](mailto:sandoval.romero.sebastian@gmail.com)

---

## 📌 Estado del Proyecto

- Proyecto funcional
- Desplegado en Render
- Base de datos cloud en Neon
- Webpay Plus Sandbox integrado
- Login y registro con Identity
- Confirmación real de correo electrónico
- Recuperación de contraseña con Brevo API
- Reenvío de enlace de confirmación
- Checkout invitado y registrado
- DemoAdmin seguro
- Carrito dinámico
- Historial de compras para clientes
- Panel administrativo de pedidos
- Eliminación de ventas para Admin
- Gestión de solicitudes
- Exportación Excel
- Variables de entorno configuradas para producción

---

## 🧭 Próximas Mejoras

* Dashboard con métricas de ventas.
* Tests unitarios y de integración.

