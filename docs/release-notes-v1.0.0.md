# Codex Token Monitor v1.0.0

Primera versión estable de un widget nativo para Windows que muestra la cuota restante de OpenAI Codex mediante el app-server oficial local.

## Funcionalidades

- Widget siempre visible, sin bordes, arrastrable y redimensionable; opacidad y fondo configurables.
- Display LED de siete segmentos, gauge semicircular y barra RPG con animaciones breves.
- Cuotas de cinco horas, semanal o ambas; porcentajes restantes y restablecimiento local.
- Refresco automático configurable (120 segundos por defecto) y manual, sin consultas simultáneas.
- Indicación de errores y datos obsoletos sin inventar porcentajes.
- Preferencias persistentes, icono de bandeja, recuperación de instancia existente e inicio automático opcional mediante acceso directo.
- Sin generación de turnos ni lectura directa de credenciales por el monitor.

## Requisitos

- Windows 11 x64.
- Codex CLI instalado y autenticado con una cuenta compatible con cuotas de ChatGPT.
- Acceso al estado normal de Codex y conectividad para consultar las cuotas.
- La distribución es autocontenida: no hace falta instalar .NET para ejecutarla. Para compilar: SDK .NET 10 x64.

## Instalación y uso

1. Descarga `CodexTokenMonitor-v1.0.0-win-x64.zip` de esta Release y extráelo en una carpeta permanente.
2. Ejecuta `CodexTokenMonitor.App.exe` con tu usuario habitual.
3. Doble clic cambia de visualización; la rueda selecciona cuota; clic derecho abre las opciones.
4. Activa inicio con Windows solo si lo deseas. Si mueves el ejecutable, vuelve a activar esa opción desde la ubicación definitiva.

No hay instalador ni cambios en el registro. Los ajustes se guardan en `%LOCALAPPDATA%\CodexTokenMonitor`. Para probar sin cuenta, ejecuta con `--simulate`; los datos se etiquetan como simulación.

## Validación

Compilación Release y 43 pruebas xUnit. Cobertura de errores RPC, autenticación simulada, reconexión, timeout, cancelación y cierre del proceso, concurrencia, cuotas ausentes, preferencias, geometría de monitores y renderizado de los tres controles. Validación manual de las tres visualizaciones con datos reales, arrastre, rueda, persistencia, ocultación/recuperación, refresco automático y creación/retirada del acceso directo de inicio.

## Limitaciones conocidas

- El protocolo app-server de Codex es experimental; cambios incompatibles pueden requerir actualizar el monitor.
- Las pruebas de monitores y DPI son simuladas: no se ha verificado el traslado físico entre pantallas con escalas distintas.
- No se ha realizado una prueba manual directa del menú del icono en la bandeja ni un nuevo inicio de sesión de Windows. Se comprobó la recuperación alternativa ejecutando de nuevo la aplicación y la creación/retirada del acceso directo.
- No se probaron manualmente todas las combinaciones de tamaños, opacidades e intervalos.
- La distribución no tiene firma de código, instalador ni actualizador automático. No se ha probado en un segundo equipo sin .NET.
- Las cuotas dependen de la cuenta: una ventana ausente se muestra como no disponible.

Consulta el README, `docs/architecture.md` y `docs/validation.md` para más detalles.
