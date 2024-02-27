extends Node3D

@onready var particles : GPUParticles3D = %WaterStream

@export var fountain : Node3D;
func interact(any):
	particles.emitting = !particles.emitting 
	var y = fountain.transform.origin.y
	if (y <= -100):
		y = -4
	else:
		y = -100
	fountain.transform.origin.y = y
