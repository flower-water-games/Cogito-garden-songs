extends Node3D

class_name Waterable


var moisture_level = 0.0
var max_moisture_level = 10.0
var threshold = 4.0

func water(amount):
	moisture_level += amount
	print('watering')
	update_moisture_level()
	if moisture_level > max_moisture_level:
		moisture_level = max_moisture_level
    # Implement additional logic here (e.g., visual feedback, triggering growth

func update_moisture_level():
	# Implement logic to update the visual representation of the moisture level here
	if moisture_level > threshold:
		# swap cube texture
		_on_threshold_reached()
		scale = Vector3(1.5, 1.5, 1.5)

func _on_threshold_reached():
	# Implement logic to trigger growth here
	print('warning: not implemented -> threshold reached')