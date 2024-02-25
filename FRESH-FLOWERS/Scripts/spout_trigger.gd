extends Node3D

@onready var particles : GPUParticles3D = %WaterStream
func interact(any):
	MusicManager.play("Music", "Stage2")
	particles.emitting = true
