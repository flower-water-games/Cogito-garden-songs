extends Node3D


# two area 3ds, one for stage 1 music, stage 2 music, 
@export var stage1_area : Area3D
@export var stage2_area : Area3D


# when entered / exits, triggers stage 1 vs stage 2 respectively
func _ready():
	# connect the area entered signal
	stage1_area.connect("body_entered", _stage_1_entered)
	stage1_area.connect("body_exited", _on_area_exit)

	stage2_area.connect("body_entered", _stage_2_entered)
	stage2_area.connect("body_exited", _on_area_exit)

func _on_area_exit(node):
	print("stop playing")
	MusicManager.stop(3)


func _stage_1_entered(node):
	# play stage 1 music
	# if the body is in the group "player"
	if node.is_in_group("Player"):
		MusicManager.play("Music", "Stage1", 1)
		print("stage 1 music playing")


func _stage_2_entered(node):
	# play stage 2 music if player entered
	if node.is_in_group("Player"):
		MusicManager.play("Music", "Stage2", 1)
		print("stage 2 music playing")
	

