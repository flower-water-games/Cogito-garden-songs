extends Node3D

class_name Waterable


var moisture_level = 0.0
@export var max_moisture_level = 3.1
@export var threshold = 3.0
@onready var initial_scale = scale

func water(amount):
	moisture_level += amount
	scale_up_tween()
	print('watering')
	update_moisture_level()
	if moisture_level >= max_moisture_level:
		moisture_level = max_moisture_level
    # Implement additional logic here (e.g., visual feedback, triggering growth

var scale_up_factor = .2
func scale_up_tween():
	var normalized_moisture_level = moisture_level / max_moisture_level
	var new_scale = Vector3(initial_scale.x, initial_scale.y, initial_scale.z) + Vector3(scale_up_factor, scale_up_factor, scale_up_factor) * normalized_moisture_level
	# TODO tween the scale 
	var tween = create_tween()
	tween.tween_property(self, "scale", new_scale, .5)
	# set an ease that bounces on complete
	tween.set_ease( Tween.EASE_OUT)

func update_moisture_level():
	# Implement logic to update the visual representation of the moisture level here
	if moisture_level > threshold:
		# swap cube texture
		_on_threshold_reached()

func _on_threshold_reached():
	# Implement logic to trigger growth here
	print('warning: not implemented -> threshold reached')