extends Waterable

@export var stem_name = "Stem"

func _on_threshold_reached():
	MusicManager.enable_stem(stem_name)