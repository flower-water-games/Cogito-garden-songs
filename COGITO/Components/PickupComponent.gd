extends InteractionComponent
class_name PickupComponent

@export var slot_data : InventorySlotPD

var player_interaction_component : PlayerInteractionComponent

@export var auto_equip = true

func interact(_player_interaction_component):
			
	if _player_interaction_component.get_parent().inventory_data.pick_up_slot_data(slot_data):
		_player_interaction_component.send_hint(slot_data.inventory_item.icon, slot_data.inventory_item.name + " added to inventory.")
		Audio.play_sound(slot_data.inventory_item.sound_pickup)
		if auto_equip:
			_player_interaction_component.equip_wieldable(slot_data.inventory_item)

		self.get_parent().queue_free()


func get_item_type() -> int:
	if slot_data and slot_data.inventory_item:
		return slot_data.inventory_item.item_type
	else:
		return -1
