# Blender headless: GLB -> FBX (Mixamo'ya yükleme için)
# Kullanım: Blender -b -P glb_to_fbx.py -- <girdi.glb> <cikti.fbx>
import bpy
import sys

argv = sys.argv[sys.argv.index("--") + 1:]
src, dst = argv

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=src)

# Dönüşümleri uygula (Mixamo ölçek/rotasyon sorunlarını önler)
for obj in bpy.data.objects:
    obj.select_set(True)
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

bpy.ops.export_scene.fbx(
    filepath=dst,
    use_selection=False,
    apply_scale_options='FBX_SCALE_ALL',
    path_mode='COPY',
    embed_textures=True,
)
print("OK:", dst)
