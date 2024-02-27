extends Node3D

class_name Waterable


var moisture_level = 0.0
@export var max_moisture_level = 10.0
@export var threshold = 2.0

func water(amount):
	moisture_level += amount
	print('watering')
	update_moisture_level()
	if moisture_level > max_moisture_level:
		moisture_level = max_moisture_level
    # Implement additional logic here (e.g., visual feedback, triggering growth

func update_moisture_level():
	var normalized_moisture_level = moisture_level / max_moisture_level
	var new_scale = Vector3(scale.x, scale.y, scale.z) + Vector3(0.5, 0.5, 0.5) * normalized_moisture_level
	# TODO tween the scale 
	scale = new_scale

	# Implement logic to update the visual representation of the moisture level here
	if moisture_level > threshold:
		# swap cube texture
		_on_threshold_reached()

func _on_threshold_reached():
	# Implement logic to trigger growth here
	print('warning: not implemented -> threshold reached')