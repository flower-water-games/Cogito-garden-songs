extends Node3D


@export_group("Flashlight Settings")
## Sets if the flashlight is turned on or off on ready()
@export var is_on : bool
## How much the energy depletes per second.
@export var drain_rate : float = 1
## The light node that gets toggled.
@onready var spot_light_3d = $SpotLight3D

@export_group("Audio")
@export var switch_sound : AudioStream
@export var sound_reload : AudioStream

@export_group("General Wieldable Settings")
## Item resource that this wieldable refers to.
@export var item_reference : WieldableItemPD
## Visible parts of the wieldable. Used to hide/show on equip/unequip.
@export var wieldable_mesh : Node3D

@export_group("Animations")
@onready var animation_player: AnimationPlayer = $AnimationPlayer
@onready var audio_stream_player_3d = $AudioStreamPlayer3D

@export var anim_equip: String = "equip"
@export var anim_unequip: String = "unequip"
@export var anim_action_primary: String = "action_primary"
@export var anim_action_secondary: String = "action_secondary"
@export var anim_reload: String = "reload"

### Every wieldable needs the following functions:
### equip(_player_interaction_component), unequip(), action_primary(), action_secondary(), reload()

var player_interaction_component # Stores the player interaction component
var trigger_has_been_pressed : bool = false


func _ready():
	if wieldable_mesh:
		wieldable_mesh.hide()
	if is_on:
		spot_light_3d.show()
	else:
		spot_light_3d.hide()


func _process(delta):
	if is_on:
		player_interaction_component.equipped_wieldable_item.subtract(delta * drain_rate)
		if player_interaction_component.equipped_wieldable_item.charge_current == 0:
			turn_off()
			is_on = false


# Primary action called by the Player Interaction Component when flashlight is wielded.
func action_primary(_camera_collision:Vector3, _passed_item_reference:InventoryItemPD):
	animation_player.play(anim_action_primary)
	toggle_on_off()


# Secondary action called by the Player Interaction Component when flashlight is wielded.
func action_secondary(_is_released: bool):
	print("Flashlight doesn't have a secondary action.")
	pass


# Function called when wieldable is unequipped.
func equip(_player_interaction_component: PlayerInteractionComponent):
	animation_player.play(anim_equip)
	player_interaction_component = _player_interaction_component


# Function called when wieldable is unequipped.
func unequip():
	animation_player.play(anim_unequip)
	if is_on:
		turn_off()


# Function called when wieldable reload is attempted
func reload():
	animation_player.play(anim_reload)
	audio_stream_player_3d.stream = sound_reload
	audio_stream_player_3d.play()


# Function to explicitly turn it off for use when battery is depleted.
func turn_off():
	is_on = false
	audio_stream_player_3d.stream = switch_sound
	audio_stream_player_3d.play()
	spot_light_3d.hide()


func toggle_on_off():
	audio_stream_player_3d.stream = switch_sound
	audio_stream_player_3d.play()
	
	if is_on:
		spot_light_3d.hide()
		is_on = false
	elif player_interaction_component.equipped_wieldable_item.charge_current == 0:
		player_interaction_component.send_hint(null, player_interaction_component.equipped_wieldable_item.name + " is out of battery.")
	else:
		spot_light_3d.show()
		is_on = true
