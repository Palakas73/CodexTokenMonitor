# Arquitectura

## Componentes

- **Core**: `IRateLimitProvider`, instantáneas, ventanas por duración y bucket, conversión de porcentaje, estado obsoleto, preferencias normalizadas y cálculo puro de posición visible.
- **Infrastructure**: búsqueda del CLI, proceso privado por consulta, JSONL con correlación por ID, errores RPC sanitizados, parser estricto, proveedor simulado y repositorio JSON atómico.
- **App**: WPF/MVVM. El ViewModel administra datos y selección; los tres controles independientes dibujan con DrawingContext. El code-behind compone servicios de escritorio (ventana, bandeja, monitor, menú, ciclo de vida). Sin contenedor DI ni framework MVVM externo.
- **Tests**: xUnit, renderizado STA y app-server falso ejecutado en un proceso real; no utiliza credenciales ni endpoints de producción.

## Lectura y recuperación

Cada consulta inicia exclusivamente su propio proceso oculto, espera inicialización y consulta cuotas. Notificaciones no solicitadas se ignoran; solicitudes del servidor no soportadas reciben -32601. stderr se drena y descarta. No se almacenan transcripciones ni datos de autenticación. Las respuestas se correlacionan por ID y los campos de cuotas se validan.

Timeout configurable (30 segundos por defecto), semáforos en proveedor y UI, cancelación al cerrar. La limpieza cierra stdin, espera dos segundos y termina solo el árbol propio si no sale. La espera de drenaje también está acotada. Tras un fallo, la próxima consulta parte de un proceso nuevo. La UI conserva el último dato válido, marca el fallo y recupera el estado normal al obtener una nueva instantánea. El envejecimiento temporal también marca datos obsoletos.

La clasificación amigable de autenticación utiliza indicios del mensaje del servidor; el código RPC se conserva pero el texto remoto nunca se muestra ni registra. Un error no reconocido se presenta como rechazo RPC genérico.

## Interfaz y recursos

Ventana Topmost, sin bordes, transparente según preferencias. Restauración física del HWND dentro del área útil de monitores, con WM_DISPLAYCHANGE / WM_DPICHANGED y manifiesto PerMonitorV2. No se llama Activate al refrescar. El arrastre termina ajustando la ventana a un monitor.

Digital, gauge y RPG reciben porcentaje restante o NaN para ausencia. Animación de 350 ms al cambiar valor, sin bucles de dibujo continuo; se respeta ClientAreaAnimation. El texto de cada fila expone el valor a accesibilidad y al tooltip. La barra se ancla a la izquierda, de modo que se vacía desde la derecha.

Las escrituras de geometría se agrupan con debounce de 400 ms; el cierre fuerza guardado. Un mutex por sesión evita duplicados. Un evento con nombre restaura la instancia existente; un temporizador ligero lo comprueba sin bloquear el hilo de UI.

La bandeja se conserva cuando se oculta la ventana y se libera al salir. El cierre se aplaza mientras se cancelan y esperan las consultas. Inicio automático mediante acceso directo propio en Inicio, sin registro; solo se modifica cuando el usuario activa/desactiva la opción.

## Distribución

Windows x64 autocontenido y single-file, sin trimming para conservar compatibilidad con XAML y COM. El script ejecuta pruebas antes de publicar; no crea instaladores, repositorios ni cambios de seguridad.
