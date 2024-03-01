extends Waterable
@export var is_stem_flower = true
@export var is_party_cube = false
@export var stem_name = "Stem"

@onready var flower_cube_mesh:MeshInstance3D = %FlowerCubeMesh

@export var my_happy_material:StandardMaterial3D = null
@export var my_sad_material:StandardMaterial3D = null

# %GardenMusicManager

var garden_music_manager : GardenMusicManager = null


# Called when the node enters the scene tree for the first time.
func _ready():
	#set the material to the sad material
	flower_cube_mesh.material_override = my_sad_material
	garden_music_manager = get_parent().get_node("/root/flower_songs_scene/GardenMusicManager")

func _on_threshold_reached():
	#need to switch the instance of the mesh's material to the grown material
	flower_cube_mesh.material_override = my_happy_material
	if is_party_cube:
		garden_music_manager.stage3()
	if is_stem_flower:
		garden_music_manager.enable_stem(stem_name)

		
