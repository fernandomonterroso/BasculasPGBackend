# Proyecto BásculasPG - Backend

Este proyecto constituye el **backend** de la aplicación web para el manejo de cargas en depósitos aduaneros. Desarrollado con **.NET Core 8**, el sistema proporciona una **API RESTful** para gestionar operaciones relacionadas con básculas, guías de carga y el registro de pesos.

## Tabla de Contenidos

1. [Características](#características)
2. [Tecnologías Utilizadas](#tecnologías-utilizadas)
3. [Requisitos Previos](#requisitos-previos)
4. [Instalación](#instalación)
5. [Ejecución del Proyecto](#ejecución-del-proyecto)
6. [API Endpoints](#api-endpoints)
7. [Descripción de los Sistemas y Componentes](#descripción-de-los-sistemas-y-componentes)
8. [Configuración Avanzada](#configuración-avanzada)
9. [Manejo de Errores](#manejo-de-errores)
10. [Contribución](#contribución)
11. [Licencia](#licencia)

---

## Características

- **Gestión de Básculas**: Control y monitoreo de las básculas conectadas a través de puertos seriales, obteniendo y procesando los pesos registrados.
- **Manejo de Guías de Carga**: Consulta y administración de guías de importaciones, exportaciones y courier, incluyendo la generación y actualización de información relevante.
- **Registro de Pesos**: Captura, validación y almacenamiento de pesos provenientes de básculas.
- **Autenticación y Autorización**: Validación de usuarios mediante gafetes para acceder a las funcionalidades del sistema.
- **Integración con MySQL**: Uso de **Dapper** para la interacción eficiente con la base de datos MySQL.
- **Manejo de Errores**: Gestión de excepciones y errores comunes, proporcionando respuestas claras y descriptivas al frontend para facilitar el diagnóstico y la resolución de problemas.

## Tecnologías Utilizadas

- **.NET Core 8**: Framework principal para el desarrollo del backend.
- **ASP.NET Core Web API**: Para la creación de endpoints RESTful.
- **Dapper**: Micro ORM para la gestión de consultas a la base de datos.
- **MySQL**: Sistema de gestión de bases de datos utilizado para almacenar información.
- **SerialPort**: Para la comunicación con las básculas a través de puertos seriales.
- **JWT (JSON Web Tokens)**: Para la autenticación y autorización de usuarios.

## Requisitos Previos

Antes de ejecutar este proyecto, asegúrate de tener instalado:

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) o superior con soporte para .NET Core.
- [.NET Core 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://www.mysql.com/downloads/) (v8.0 o superior)
- [Node.js](https://nodejs.org/) (para la interacción con el frontend, opcional)
- [Angular CLI](https://angular.io/cli) (para la interacción con el frontend, opcional)

## Instalación

### 1. Clonar el Repositorio

Clona el repositorio del backend a tu máquina local:

```bash
git clone https://github.com/fernandomonterroso/BasculasPGBackend.git
```

### 2. Acceder al Directorio del Proyecto

```bash
cd BasculasPGBackend
```

### 3. Configurar la Base de Datos

- Asegúrate de tener MySQL instalado y en ejecución.
- Crea una base de datos llamada `combex` o la que prefieras.
- Ejecuta los scripts SQL necesarios para crear las tablas requeridas por el proyecto. Estos scripts deberían estar incluidos en la carpeta `Database` del repositorio.

### 4. Configurar Variables de Entorno

Crea un archivo `appsettings.json` en el directorio raíz del proyecto con la siguiente configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=combex;User Id=tu_usuario;Password=tu_contraseña;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "TuClaveSecretaParaJWT",
    "Issuer": "tu_dominio.com",
    "Audience": "tu_dominio.com"
  }
}
```

**Notas:**

- Reemplaza `tu_usuario` y `tu_contraseña` con las credenciales de tu base de datos MySQL.
- Configura la sección `Jwt` con una clave secreta robusta y los valores apropiados para `Issuer` y `Audience`.

### 5. Instalar Dependencias

Ejecuta el siguiente comando para restaurar las dependencias del proyecto:

```bash
dotnet restore
```

## Ejecución del Proyecto

Para ejecutar el proyecto en un entorno de desarrollo, utiliza el siguiente comando:

```bash
dotnet run
```

Esto iniciará el servidor en la URL `https://localhost:5001/` o `http://localhost:5000/` dependiendo de tu configuración.

## API Endpoints

### 1. **BasculaController**

#### **GET /api/Bascula/v2**

Obtiene el peso desde una báscula específica.

**Parámetros de Consulta:**

- `port` (string): Puerto serial al que está conectada la báscula (ejemplo: `COM3`).
- `command` (string): Comando para solicitar el peso a la báscula.

**Respuesta Exitosa:**

```json
{
  "peso": "746.73 Kg"
}
```

**Errores Comunes:**

- `400 Bad Request`: Faltan parámetros o error en la lectura del puerto serial.
- `504 Gateway Timeout`: Tiempo de espera excedido para la lectura del puerto serial.

#### **GET /api/Bascula/v1**

Obtiene un peso aleatorio (utilizado para pruebas).

**Respuesta Exitosa:**

```json
{
  "peso": "472 Kg"
}
```

### 2. **Otros Endpoints**

El backend incluye funcionalidades adicionales para:

- **Gestión de Guías**: Obtener información de guías específicas mediante métodos como `GetKeysGuia` y `GetGuiaXManifiesto`.
- **Registro de Pesos**: Almacenar pesos capturados desde las básculas mediante el método `PostPeso`.
- **Autenticación de Usuarios**: Validar usuarios mediante gafetes para acceso seguro (implementado en `UserService`).

## Descripción de los Sistemas y Componentes

### a. **Controladores**

#### **BasculaController**

Encargado de manejar las solicitudes relacionadas con las básculas. Incluye métodos para obtener pesos desde las básculas conectadas y para obtener pesos aleatorios para pruebas.

- **Métodos Principales:**
  - `GetPeso`: Lee el peso desde una báscula específica utilizando un puerto serial.
  - `GetRandomWeight`: Retorna un peso aleatorio para pruebas.

### b. **Handlers**

#### **DataHandler**

Clase responsable de manejar la lógica de negocio y las operaciones de acceso a datos. Utiliza **Dapper** para interactuar con la base de datos MySQL.

- **Métodos Principales:**
  - `getBasculasByBod`: Obtiene las básculas asociadas a una bodega específica.
  - `GetKeysGuia`: Obtiene las claves de una guía específica.
  - `GetGuiaXManifiesto`: Obtiene guías basadas en un manifiesto específico.
  - `GetPesos`: Obtiene los pesos registrados para una guía.
  - `PostPeso`: Registra nuevos pesos en la base de datos.

### c. **Modelos**

#### **Entities**

Clases que representan las entidades de la base de datos, como `Peso`, `Guia`, `Bodega`, etc. Estas clases se utilizan para mapear los datos de la base de datos a objetos en el código.

### d. **Data Access**

#### **MySqlDbManager**

Clase encargada de manejar las conexiones y consultas a la base de datos MySQL utilizando **Dapper**. Proporciona métodos para ejecutar consultas y comandos SQL de manera eficiente.

### e. **Autenticación y Seguridad**

#### **JWT (JSON Web Tokens)**

Implementación de JWT para asegurar que solo usuarios autenticados puedan acceder a las funcionalidades del backend. Configurado en el archivo `appsettings.json` y manejado en los servicios de autenticación.

### f. **Comunicación con Básculas**

#### **SerialPort**

Uso de la clase `SerialPort` para comunicar el backend con las básculas físicas conectadas a través de puertos seriales. Permite enviar comandos y recibir datos de peso desde las básculas.

### g. **Manejo de Errores**

Implementación de manejo de excepciones para capturar y responder adecuadamente a errores comunes, como fallos en la lectura de puertos seriales o errores de base de datos.

## Configuración Avanzada

### **Serial Port Configuration**

El controlador `BasculaController` utiliza la clase `SerialPort` para comunicarse con las básculas. Asegúrate de que:

- El puerto serial especificado (`port`) esté disponible y correctamente configurado en tu sistema.
- La velocidad de transmisión (`9600 bps`), paridad, bits de datos y bits de parada coincidan con la configuración de tu báscula.

### **Manejo de Errores**

El sistema está diseñado para manejar excepciones comunes, como:

- **ArgumentNullException**: Cuando faltan parámetros en la solicitud.
- **TimeoutException**: Cuando se excede el tiempo de espera para leer del puerto serial.
- **MySqlException**: Errores relacionados con la base de datos MySQL.

Estos errores se devuelven al cliente con mensajes descriptivos para facilitar el diagnóstico.

## Contribución

Si deseas contribuir a este proyecto, sigue estos pasos:

1. **Haz un Fork** del repositorio.
2. **Crea una nueva rama** para tu feature:
   ```bash
   git checkout -b feature-nueva-funcionalidad
   ```
3. **Haz commit** de tus cambios:
   ```bash
   git commit -m "Agrega nueva funcionalidad"
   ```
4. **Haz push** a tu rama:
   ```bash
   git push origin feature-nueva-funcionalidad
   ```
5. **Abre un Pull Request** en el repositorio original.

## Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo `LICENSE` para obtener más detalles.

---

¡Gracias por utilizar el sistema de pesaje de básculas para depósitos aduaneros!
