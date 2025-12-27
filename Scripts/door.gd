extends Node3D

@export var object_to_duplicate: Node3D  # Reference the node you want to duplicate
@export var number_of_duplicates := 10
@export var spawn_area := Vector3(10, 0, 10)  # Random range for X, Y, Z
@export var spawn_center := Vector3.ZERO  # Center point of spawn area

func _ready():
	randomize()  # Initialize random seed
	spawn_random_objects()

func spawn_random_objects():
	if object_to_duplicate == null:
		push_error("No object assigned to duplicate!")
		return
	
	for i in range(number_of_duplicates):
		var duplicate = object_to_duplicate.duplicate()
		add_child(duplicate)
		
		# Random position within spawn area
		var random_pos = Vector3(
			randf_range(-spawn_area.x / 2, spawn_area.x / 2),
			randf_range(-spawn_area.y / 2, spawn_area.y / 2),
			randf_range(-spawn_area.z / 2, spawn_area.z / 2)
		)
		
		duplicate.position = spawn_center + random_pos
		duplicate.visible = true  # Make sure it's visible
		
		# Optional: Random rotation
		# duplicate.rotation_degrees.y = randf_range(0, 360)
