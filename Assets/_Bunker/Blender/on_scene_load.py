# on_scene_load.py - Bunker Subterraneo (Camano E. / Gonzalez E.)
# Registrado dentro de Bunker_Kit.blend (Text > Register).
# Cuando Unity importa el .blend, Blender abre el archivo en modo background;
# este script elimina las colecciones auxiliares (REF_, GEN_, AUX_) y las luces
# o camaras de prueba, para que Unity solo reciba los modulos de KIT_Modulos.
# Requiere: Blender > Preferencias > Guardar y cargar > "Ejecutar automaticamente scripts Python".
import bpy
from bpy.app.handlers import persistent

PREFIJOS = ("REF_", "GEN_", "AUX_")


def limpiar_auxiliares():
    for col in list(bpy.data.collections):
        if col.name.startswith(PREFIJOS):
            for ob in list(col.all_objects):
                bpy.data.objects.remove(ob, do_unlink=True)
            bpy.data.collections.remove(col)
    for ob in list(bpy.data.objects):
        if ob.type in {"LIGHT", "CAMERA"}:
            bpy.data.objects.remove(ob, do_unlink=True)


@persistent
def on_scene_load(_dummy=None):
    # Solo en la importacion automatica (sin interfaz); al editar no toca nada.
    if bpy.app.background:
        limpiar_auxiliares()
        print("[on_scene_load] auxiliares eliminados para Unity")


if on_scene_load not in bpy.app.handlers.load_post:
    bpy.app.handlers.load_post.append(on_scene_load)
on_scene_load()
