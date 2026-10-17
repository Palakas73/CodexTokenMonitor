# Validación v1.0.0

## Alcance

Este documento resume las pruebas de la versión estable sin incluir valores reales de cuotas, identidad de cuenta, rutas de perfil ni transcripciones de la sesión de desarrollo. El historial de desarrollo detallado se conserva únicamente como artefacto local excluido de Git.

## Resultados de desarrollo

- Compilaciones Release de las fases 2–5 correctas, sin advertencias ni errores; 13 pruebas iniciales superadas en cada etapa.
- Batería ampliada de 33 pruebas de lógica, persistencia e integración con un app-server falso local.
- Batería final de **43 pruebas** con renderizado de los tres controles y regresión del gradiente del gauge.
- Publicación Windows x64 autocontenida, single-file y sin trimming ejecutada correctamente.

## Cobertura automatizada

Conversión consumido/restante y límites; identificación de ventanas por duración y bucket; cuotas ausentes; JSON-RPC incompleto o malformado; clasificación de errores y falta de autenticación simulada; desconexión/reconexión; timeout, cancelación y cierre del PID propio; consultas concurrentes; datos obsoletos; persistencia, sustitución atómica, preferencias dañadas y normalización; geometría con monitores retirados, negativos o pequeños y escalas simuladas; renderizado STA en límites de color y ausencia de datos.

## Pruebas manuales realmente realizadas

- Lectura real desde Codex y contraste de porcentajes consumidos/restantes, sin conservar datos de cuenta en el repositorio.
- Digital, gauge y RPG inspeccionados con datos reales, también en la distribución autocontenida.
- Doble clic, rueda en ambas direcciones y arrastre comprobados.
- Persistencia de modo y geometría tras reabrir.
- Ocultación desde menú y recuperación mediante segunda ejecución, sin duplicar la ventana.
- Refresco automático de 120 segundos observado sin pulsar actualización manual.
- Creación y retirada del acceso directo de Inicio desde el menú, verificando destino y argumento de arranque oculto. Se deja desactivado.
- Cierre normal antes de recompilar y apertura desde una carpeta con únicamente el EXE.

## Problemas corregidos durante validación

La rueda era capturada por ScrollViewer; se cambió a PreviewMouseWheel y se comprobó. El gradiente del gauge se fijó al arco completo para evitar verde en cuota baja, con prueba de regresión. Se mejoró el manejo de preferencias dañadas. Una publicación bloqueada por DLL en uso se resolvió cerrando normalmente la aplicación y repitiendo el script.

## No ejecutadas y límites

- Traslado físico entre monitores, cambio real de DPI y desconexión física de pantallas.
- Interacción directa con el icono de bandeja mediante automatización.
- Nuevo inicio de sesión de Windows.
- Todas las combinaciones manuales de tamaño, opacidad e intervalo.
- Fallos del servicio remoto o cierre de sesión real: los errores se provocaron con FakeServer.
- Prueba en un segundo equipo sin runtime .NET instalado.

No se declaran superadas estas pruebas. No hay instalador, firma digital ni actualizador automático.

## Revalidación para publicación

Comprobación previa al commit inicial de v1.0.0:

- `dotnet build CodexTokenMonitor.slnx -c Release --no-restore`: correcto, **0 advertencias y 0 errores**.
- `scripts/publish.ps1`: código 0; **43 pruebas superadas, 0 fallidas, 0 omitidas**; publicación x64 autocontenida y ZIP generados.
- Metadatos del EXE: FileVersion `1.0.0.0`, ProductVersion `1.0.0`.
- ZIP verificado sin errores y con tres entradas: ejecutable, README y notas de versión. Su EXE coincide byte a byte con el resultado de publicación.
- Revisión de los 42 archivos fuente/documentación previstos para el commit: no se detectaron claves privadas, tokens, correos personales, rutas de perfil ni asignaciones sensibles. Se excluyen salidas de compilación, cachés, logs, preferencias y datos de cuenta.
- Los valores reales de cuotas y el historial local detallado no forman parte del código publicado. El commit utiliza identidad noreply de GitHub.
- Esta preparación solo cambia metadatos de versión, documentación, exclusiones y empaquetado; no añade funcionalidades ni cambia el comportamiento del widget.
