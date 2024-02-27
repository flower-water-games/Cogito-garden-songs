extends Waterable

@export var stem_name = "Stem"

@onready var flower_cube_mesh:MeshInstance3D = %FlowerCubeMesh

@export var my_happy_material:StandardMaterial3D = null
@export var my_sad_material:StandardMaterial3D = null

# Called when the node enters the scene tree for the first time.
func _ready():
	#set the material to the sad material
	flower_cube_mesh.material_override = my_sad_material

func _on_threshold_reached():
	#need to switch the instance of the mesh's material to the grown material
	flower_cube_mesh.material_override = my_happy_material
	MusicManager.enable_stem(stem_name)
