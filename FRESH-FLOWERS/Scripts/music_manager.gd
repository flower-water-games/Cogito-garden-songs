extends Node3D

class_name GardenMusicManager

# two area 3ds, one for stage 1 music, stage 2 music, 
@export var stage1_area : Area3D
@export var stage2_area : Area3D
@export var party_cube : Node3D

# when entered / exits, triggers stage 1 vs stage 2 respectively
func _ready():
	# connect the area entered signal
	party_cube.hide()
	stage1_area.connect("body_entered", _stage_1_entered)
	stage1_area.connect("body_exited", _on_area_exit)

	stage2_area.connect("body_entered", _stage_2_entered)
	stage2_area.connect("body_exited", _on_area_exit)
	await get_tree().create_timer(300).timeout
	party_cube.show()

func _on_area_exit(node):
	# if stage 3 is not playing, stop the music
	if not is_stage_3:
		print("stop playing")
		MusicManager.stop(.1)


func _stage_1_entered(node):
	# play stage 1 music
	# if the body is in the group "player"
	if node.is_in_group("Player"):
		# enable all stems for stage 1 that are playing already

		MusicManager.play("Music", "Stage1", .1)
		for stem in stage1_stems:
			MusicManager.enable_stem(stem)
		print("stage 1 music playing")


func _stage_2_entered(node):
	# play stage 2 music if player entered
	if node.is_in_group("Player"):
		MusicManager.play("Music", "Stage2", .1)
		for stem in stage2_stems:
			MusicManager.enable_stem(stem)
		print("stage 2 music playing")
	

# two dictionaries per stage, to remember which stems are playing
var stage1_total_additional_stems = 4 
var stage2_total_additional_stems = 6

var stage1_all_stems_found = false;
var stage2_all_stems_found = false;

var stage1_stems = {}
var stage2_stems = {}

func _is_stage_playing(num: int) -> bool:
	return MusicManager.is_playing("Music", "Stage" + str(num))

func enable_stem(stem_name: String):
	if _is_stage_playing(1):
		# first check if the stem is already playing
		if stem_name in stage1_stems:
			return
		# if not, add it to the dictionary
		stage1_stems[stem_name] = true
		if len(stage1_stems) == stage1_total_additional_stems:
			# if all stems are playing, all_playing is true
			stage1_all_stems_found = true

	elif _is_stage_playing(2):
		if stem_name in stage2_stems:
			return
		stage2_stems[stem_name] = true
		if len(stage2_stems) == stage2_total_additional_stems:
			# if all stems are playing, all_playing is true
			stage2_all_stems_found = true
	MusicManager.enable_stem(stem_name)

var is_stage_3 = false

func stage3():
	# wait 30 seconds
	if (is_stage_3):
		return
	is_stage_3 = true
	await get_tree().create_timer(10).timeout 
	print("stage 3 party time")
	MusicManager.stop(10)
	MusicManager.play("Music", "Stage3", 10)

func _process(delta):
	# if all stems are playing, stop the music
	if stage1_all_stems_found and stage2_all_stems_found:
		stage1_all_stems_found = false
		stage3()
	pass
