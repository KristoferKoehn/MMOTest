extends RigidBody3D

var speed = 200  # Adjust this value based on your desired speed
var direction = Vector3(0, 0, 1)  # Initial direction (right in this case)

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	move_and_collide(direction * speed * delta)
