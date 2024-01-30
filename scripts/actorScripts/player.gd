class_name player
extends CharacterBody3D

@onready var camera_mount = $cameraMount
@onready var camera_3d = $cameraMount/Camera3D
@onready var visuals = $visuals
@onready var animation_player = $visuals/mixamoBase/AnimationPlayer

@export var horizontalMouseSensitivity = 0.2
@export var verticalMouseSensitivity = 0.2
@export var scrollSensitivity = 0.25

var SPEED = 3.0

@export var walkingSpeed = 3.0
@export var runningSpeed = 5.0

var JUMP_VELOCITY = runningSpeed # Gives 45 degree take offs

var isRunning
var isLocked

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

func _enter_tree():
	# converting player controller will break this. Don't worry about it
	set_multiplayer_authority(str(name).to_int())


func _ready():
	# this is more stuff that will break when converting to model/controller
	if not is_multiplayer_authority(): return
	
	camera_3d.current = true;
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	
func _input(event):
	# this is more stuff that will break when converting to model/controller
	if not is_multiplayer_authority(): return
	# end stuff that will break
	
	if event is InputEventMouseMotion:
		rotate_y(deg_to_rad(-event.relative.x * horizontalMouseSensitivity))
		visuals.rotate_y(deg_to_rad(event.relative.x * horizontalMouseSensitivity))
		camera_mount.rotate_x((deg_to_rad(-event.relative.y * verticalMouseSensitivity)))
		if camera_mount.rotation.x > deg_to_rad(90):
			camera_mount.rotation.x = deg_to_rad(90)
		if camera_mount.rotation.x < deg_to_rad(-90):
			camera_mount.rotation.x = deg_to_rad(-90)
	
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_WHEEL_DOWN && camera_3d.transform.origin.z < 10:
			# Not quite right. But, something like this. camera_3d.position = camera_3d.position.lerp(position + scrollsensitivity, cameraFollowSpeed)
			camera_3d.transform.origin.z += scrollSensitivity
		
		if event.button_index == MOUSE_BUTTON_WHEEL_UP && camera_3d.transform.origin.z > 0:
			camera_3d.transform.origin.z -= scrollSensitivity
		
		# if event.button_index == MOUSE_BUTTON_MIDDLE:
			# go to next camera mode. Could set this up to toggle between first and 3rd person, like skyrim. And just not allow zoom.

func _physics_process(delta):
	# this is more stuff that will break when converting to model/controller
	if not is_multiplayer_authority(): return
	# end stuff that will break
	
	if !animation_player.is_playing():
		isLocked = false
	
	# Add the gravity.
	if not is_on_floor() and not isLocked:
		velocity.y -= gravity * delta
		# Todo: play falling animation
	
	# Handle Jump.
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY
		# need an animation here too
	
	# Toggle Running
	if Input.is_action_pressed("run"):
		isRunning = true
	else:
		isRunning = false
		
	# Adjust speed to match running / walking state if on floor	
	if is_on_floor():
		if isRunning:
			SPEED = runningSpeed
		else:
			SPEED = walkingSpeed
	
	# Here are all the animations that would override walking / running / idle / falling. Right now it is just kick.
	# There is an issue with being able to slide on the ground during a kick, but I think this is actually desired for most actions. Just not kicking.
	if Input.is_action_just_pressed("kick"):
		
		if !isLocked and animation_player.current_animation != "kick":
			animation_player.play("kick")
			isLocked = true

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir = Input.get_vector("left", "right", "forward", "backward")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		if !isLocked:
			if isRunning:
				if animation_player.current_animation != "running":
					animation_player.play("running")
			else:
				if animation_player.current_animation != "walking":
					animation_player.play("walking")
			visuals.look_at(position + direction)
		
		#if is_on_floor():
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
	else:
		if !isLocked:
			if animation_player.current_animation != "idle":
				animation_player.play("idle")
		# Need to figure out how this plays with the whole locked thing
		if is_on_floor():
			velocity.x = move_toward(velocity.x, 0, SPEED)
			velocity.z = move_toward(velocity.z, 0, SPEED)

	#if !isLocked:
	move_and_slide()
