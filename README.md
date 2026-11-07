# Bunker Subterráneo

Proyecto semestral de **Animación Digital y Producción** (FISC-7000) — Universidad Tecnológica de Panamá, grupo 1SF241, II Semestre 2026.
Facilitador: Ing. Ezequiel Aguilar · Estudiantes: Edward Camaño y Emanuel González.

Ha estallado la Tercera Guerra Mundial y los misiles impactan en 3 minutos. En primera persona, el jugador cruza un bunker subterráneo y debe sellar la Cámara B antes de que el cronómetro llegue a 0.

## Requisitos

| Software | Versión |
| --- | --- |
| Unity | **6000.3.28f1** (6.3 LTS), plantilla Universal 3D (URP) |
| Blender | **5.1 o 5.2 LTS** — obligatorio: Unity lo usa para importar `Bunker_Kit.blend` |

Sin Blender instalado, la escena abre sin modelos.

## Abrir el proyecto

1. Clonar el repositorio.
2. Unity Hub → **Add project from disk** → seleccionar la carpeta clonada.
3. Abrir con 6000.3.28f1. La primera vez tarda varios minutos (reconstruye `Library`).
4. Abrir la escena `Assets/_Bunker/Scenes/Bunker.unity` y pulsar Play.

## Controles

| Acción | Teclado | Mando |
| --- | --- | --- |
| Moverse / mirar | WASD / ratón | Sticks |
| Interactuar (puerta, botón, cofre, objetos, cama) | E | Botón sur |
| Cerrar la puerta desde dentro | L | Botón norte |
| Linterna | F | Botón oeste |
| Correr | Shift | Clic stick izquierdo |
| Reintentar | R | Select |
| Liberar cursor | Esc | Start |

## Flujo de juego

1. **Alarma** (luces rojas, sirena, 3:00) → abrir la puerta blindada con E.
2. **Puerta abierta** (naranja) → leer el aviso y pulsar el botón rojo.
3. **Asegurada** (verde) → cerrar la puerta desde dentro con L.
4. **Sellada** → al llegar a 0 empieza el bombardeo: con el bunker sellado se sobrevive; si no, R para reintentar.

## Estructura

```
Assets/_Bunker/
├── Blender/    Bunker_Kit.blend (módulos, Geometry Nodes, Asset Browser), on_scene_load.py
├── Graybox/    blocking ProBuilder exportado (.obj)
├── Textures/   mapas horneados BaseColor / Normal / AO / Metallic-Smoothness
├── Shaders/    SG_BunkerPBR.shadergraph
├── Materials/  Prefabs/  Scripts/  Input/  Settings/
└── Scenes/     Bunker.unity (final), Bunker_Graybox.unity (prototipo)
```

## Flujo de trabajo en equipo

- Hacer `git pull` antes de empezar y `git push` al terminar.
- **No editar la misma escena (`Bunker.unity`) ni el `.blend` al mismo tiempo**: son difíciles de fusionar. Avisarse antes.
- Ejecutable: `File → Build Profiles → Windows → Build` hacia la carpeta `Build/` (no se sube al repositorio).
