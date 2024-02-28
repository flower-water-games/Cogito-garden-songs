extends Wieldable 

@export var water_capacity: float = 6.0
@export var drainage_rate: float = 1.0
var current_water_level: float

@onready var water_stream: GPUParticles3D = %WaterStream
@onready var interaction_raycast: RayCast3D = %WateringRaycast

@onready var target_rotation_z = rotation_degrees.z + 10
func action_primary(item, is_released):
	item_reference = item
	if is_watering:
		stop_watering()
	else:
		start_watering()

var is_watering: bool = false
var watering_timer: Timer

func _ready():
	super()
	current_water_level = water_capacity
	watering_timer = Timer.new()
	watering_timer.wait_time = 1.0  # Adjust this to change how often water is used
	watering_timer.one_shot = false
	watering_timer.timeout.connect(_on_watering_timer_timeout)
	add_child(watering_timer)

func _process(delta):
	if is_watering && equipped:
		check_for_waterable_surface()
	if current_water_level < water_capacity-1 && equipped:
		check_for_refill()
		
var watering_target;

func check_for_refill():
	if interaction_raycast.is_colliding():
		var target = interaction_raycast.get_collider()
		# scale up target if is colliding
		if target.has_method("refill"):
			print("refilling")
			current_water_level = water_capacity
			stop_watering()

func check_for_waterable_surface():
	if interaction_raycast.is_colliding():
		var target = interaction_raycast.get_collider()
		# scale up target if is colliding
		if target.has_method("water"):
			watering_target = target
			# target.water(1.0)
		else:
			watering_target = null
			

@onready var original_rotation: Vector3 = rotation_degrees

func start_watering():
	if not is_watering and current_water_level > 0:
		is_watering = true
		water_stream.emitting = true
		# Tween the rotation
		tween_down()
		watering_timer.start()
		print("Watering Can: Started watering")


func tween_down():
	var tween = create_tween()
	tween.connect("timeout", on_tween_complete)
	tween.tween_property(self, "rotation_degrees:z", target_rotation_z, .5)

func tween_up():
	var tween = create_tween()
	tween.connect("timeout", on_tween_complete)
	tween.tween_property(self, "rotation_degrees:z", original_rotation.z, .5)
	

func on_tween_complete(tween):
	tween.queue_free()

func stop_watering():
	if is_watering:
		is_watering = false
		tween_up()
		water_stream.emitting = false
		watering_timer.stop()
		print("Watering Can: Stopped watering")

func _on_watering_timer_timeout():
	use_water()

func use_water():
	if current_water_level > 0:
		current_water_level -= drainage_rate
		if watering_target:
			watering_target.water(drainage_rate)
		if current_water_level < 0:
			current_water_level = 0
			stop_watering()
		print("Watering Can: Used water. Current water level: ", current_water_level)

	else:
		print("Watering Can: Can is empty.")
		stop_watering()

var equipped = false;
# Function called when wieldable is unequipped.
func equip(_player_interaction_component: PlayerInteractionComponent):
	equipped = true
	wieldable_mesh.show()
	player_interaction_component = _player_interaction_component
	print("Wieldable equipped")
	pass


# Function called when wieldable is unequipped.
func unequip():
	equipped = false
	wieldable_mesh.hide()
	if is_watering:
		stop_watering()
	print("Wieldable unequipped")
	pass
