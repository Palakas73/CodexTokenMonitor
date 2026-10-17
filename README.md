# Codex Token Monitor v1.0.0

Widget nativo para Windows 11, C# / .NET 10 / WPF / MVVM. Consulta cuotas reales mediante el app-server oficial de Codex; no genera turnos ni accede directamente a credenciales.

## Ejecutar

Descarga `CodexTokenMonitor-v1.0.0-win-x64.zip` desde la [Release v1.0.0](https://github.com/Palakas73/CodexTokenMonitor/releases/tag/v1.0.0), extráelo en una carpeta permanente y abre `CodexTokenMonitor.App.exe`. El repositorio y sus descargas son privados y requieren acceso autorizado de GitHub.

La distribución es Windows x64 autocontenida, sin instalación de .NET en el equipo destino. Codex CLI debe estar instalado y autenticado con una cuenta que ofrezca cuotas de ChatGPT. No se requiere una clave API. Si compilas desde el código, el ejecutable se genera en `artifacts\win-x64`.

```powershell
.\artifacts\win-x64\CodexTokenMonitor.App.exe
.\artifacts\win-x64\CodexTokenMonitor.App.exe --simulate
.\artifacts\win-x64\CodexTokenMonitor.App.exe --codex "C:\ruta\codex.exe" --timeout-seconds 45
```

`--simulate` muestra datos expresamente etiquetados como simulación. Nunca se activa automáticamente. `--start-hidden` inicia en bandeja. Solo se permite una instancia por sesión de Windows: volver a ejecutar restaura el widget existente. Para cambiar argumentos, sal primero de la instancia activa.

## Uso

- Arrastra con el botón izquierdo. Doble clic alterna display de siete segmentos, gauge y barra RPG.
- Rueda arriba: cinco horas; abajo: semanal. Selecciona **Cuota → Ambas** en el menú para verlas juntas.
- Clic derecho: visualización, cuota, opacidad, fondo transparente, tamaño, intervalo, inicio automático, actualización, mostrar, ocultar y salir.
- Bandeja: doble clic restaura; menú con Mostrar, Ocultar, Actualizar y Salir. Si Windows oculta el icono, búscalo en los iconos desbordados de la bandeja. También puedes ejecutar de nuevo la aplicación para recuperarla.
- El widget permanece encima de otras ventanas y no se activa al consultar datos. La esquina inferior derecha permite redimensionar.
- Por defecto refresca cada 120 segundos; menú con 15, 60, 120, 300 y 600 segundos. Botón y menú permiten refresco manual. No se realizan consultas simultáneas.
- Los porcentajes son cuota **restante**: verde ≥70, amarillo ≥30, naranja ≥10 y rojo <10. Se calcula `clamp(100 - usedPercent, 0, 100)`.
- Una cuota ausente muestra `--`; se distingue cinco horas (300 minutos) de semanal (10080) por duración, nunca por posición. Se mantienen separados los buckets de cuenta.
- Si falla una lectura, se conservan los últimos valores y aparece **DESACTUALIZADO**. La última lectura correcta y el restablecimiento se muestran en hora local. Una ventana de cuota no disponible no equivale a 0 %.

## Preferencias

`%LOCALAPPDATA%\CodexTokenMonitor\preferences.json`, escritura mediante archivo temporal y sustitución. Guarda posición, tamaño, modo, cuota, opacidad, fondo, intervalo, timeout y preferencia de inicio. Las posiciones fuera de pantalla se ajustan al área útil de los monitores disponibles. Manifiesto PerMonitorV2 para escalado DPI.

El inicio automático está desactivado inicialmente. Al activarlo se crea exclusivamente `CodexTokenMonitor.lnk` en la carpeta Inicio del usuario; al desactivarlo se elimina ese acceso directo propio. No se modifica el registro. Actívalo desde la ubicación definitiva del ejecutable; si lo mueves, desactiva y activa la opción desde la nueva ubicación.

## Compilar, probar y publicar

SDK .NET 10 x64; Visual Studio 2026 para abrir `CodexTokenMonitor.slnx`. No hay dependencias externas de interfaz; xUnit y su adaptador se usan solo para pruebas.

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' build CodexTokenMonitor.slnx -c Release
& 'C:\Program Files\dotnet\dotnet.exe' test CodexTokenMonitor.slnx -c Release
powershell -NoProfile -File scripts/publish.ps1
```

Cierra las instancias ejecutadas desde `bin` y el ejecutable publicado antes de compilar/publicar para evitar archivos bloqueados. El script usa SDK x64, caché y CLI home bajo `artifacts`, ejecuta pruebas y publica `win-x64` autocontenido con archivo único y sin trimming. El runtime puede extraer bibliotecas nativas en la caché temporal de .NET al arrancar: es funcionamiento normal de single-file, sin instalador. Resultado de pruebas: `artifacts/TestResults/publish.trx`.

Comprobación independiente opcional (requiere Python ya instalado): `python scripts/probe-codex.py`. Solo consulta cuotas, sin imprimir identidad ni credenciales. El usuario normal necesita acceso a su directorio de estado de Codex; el aislamiento de un agente puede impedirlo.

## Validación y límites

Véase `docs/validation.md` para resultados reales, fallos corregidos y pruebas no ejecutadas. Los tests de geometría simulan monitores y escalas; no equivalen a mover físicamente una ventana entre pantallas de DPI distinto. No se ha probado un inicio de sesión nuevo de Windows. No se incluye instalador, firma digital ni actualizador.

## Fuentes

Protocolo oficial verificado el 17/10/2026:

- https://developers.openai.com/codex/app-server
- https://github.com/openai/codex/tree/main/codex-rs/app-server
- https://github.com/openai/codex/blob/main/codex-rs/app-server-protocol/schema/typescript/v2/RateLimitWindow.ts
- https://github.com/openai/codex/blob/main/codex-rs/app-server-protocol/schema/typescript/v2/GetAccountRateLimitsResponse.ts

JSONL por stdio, sin campo `jsonrpc`; `initialize` → `initialized` → `account/rateLimits/read`. Se prioriza `rateLimitsByLimitId` y se admite `rateLimits` para versiones anteriores. `resetsAt` está en segundos Unix. El protocolo es experimental: errores o cambios incompatibles se muestran sin inventar porcentajes.
