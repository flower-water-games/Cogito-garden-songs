extends Node3D

@onready var particles : GPUParticles3D = %WaterStream
func interact(any):
	particles.emitting = true
