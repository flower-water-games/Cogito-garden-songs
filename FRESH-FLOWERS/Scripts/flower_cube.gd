extends Waterable

@export var stem_name = "Stem"

@onready var flower_cube_mesh:MeshInstance3D = %FlowerCubeMesh

@export var my_grown_material:StandardMaterial3D = null

@onready var grown_material = my_grown_material
func _on_threshold_reached():
	#need to switch the instance of the mesh's material to the grown material
	flower_cube_mesh.material_override = grown_material
	MusicManager.enable_stem(stem_name)
