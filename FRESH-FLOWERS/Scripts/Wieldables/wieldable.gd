extends Node3D

class_name Wieldable

@export_group("General Wieldable Settings")
## Item resource that this wieldable refers to.
@export var item_reference : WieldableItemPD
## Visible parts of the wieldable. Used to hide/show on equip/unequip.
@export var wieldable_mesh : Node3D

func _ready():
	wieldable_mesh.hide()

func action_primary(_camera_collision:Vector3, _passed_item_reference:InventoryItemPD):
	pass
	
func action_secondary(_is_released: bool):
	pass

# Function called when wieldable reload is attempted
func reload():
	pass

# Function called when wieldable is unequipped.
func equip(_player_interaction_component: PlayerInteractionComponent):
	wieldable_mesh.show()
	pass


# Function called when wieldable is unequipped.
func unequip():
	wieldable_mesh.hide()
	pass