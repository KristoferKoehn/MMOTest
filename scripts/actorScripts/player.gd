class_name player
extends CharacterBody3D

@onready var camera_mount = $cameraMount
@onready var camera_3d = $cameraMount/Camera3D
@onready var rayCast_3d = $cameraMount/Camera3D/RayCast3D
@onready var visuals = $visuals
@onready var animation_player = $visuals/mixamoBase/AnimationPlayer

@export var horizontalMouseSensitivity = 0.2
@export var verticalMouseSensitivity = 0.2
@export var scrollSensitivity = 0.25

@export var SPEED = 5.0
@export var JUMP_VELOCITY = 5.0
@export var jetpackAcceleration = 0.25
# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

@export var jetpackMaxFuel = 10
var jetpackFuel = jetpackMaxFuel
@export var jetpackFuelConsumptionRate = 0.1
@export var jetpackRefuelRate = 0.5

var isPositionLocked = false

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

func _physics_process(delta):
	# this is more stuff that will break when converting to model/controller
	if not is_multiplayer_authority(): return
	# end stuff that will break
	
	if !animation_player.is_playing():
		isPositionLocked = false
	
	# Handle Jump.
	if Input.is_action_just_pressed("jump_dodge") and is_on_floor():
		velocity.y = JUMP_VELOCITY
		# need an animation here too
		
	if Input.is_action_pressed("movementAbility"):
		if jetpackFuel > 0:
			jetpackFuel -= jetpackFuelConsumptionRate
			velocity.y += jetpackAcceleration
		
	# Here are all the animations that would override movement. Right now it is just kick.
	if Input.is_action_just_pressed("melee"):
		if !isPositionLocked and animation_player.current_animation != "kick": # and is_on_floor(): # We can add this and is on floor for our own sanity. It depends on what kind of canned animation we are doing.
			animation_player.play("kick")
			var arr = [0.0,0.0,0.0,0.0]
			get_node("../../").CastAbility("test", arr).rpc()
			isPositionLocked = true

	# Get the input direction and handle the movement/deceleration.
	# As good practice, you should replace UI actions with custom gameplay actions.
	var input_dir = Input.get_vector("left", "right", "forward", "backward")
	var direction = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	
	if !isPositionLocked:
		if not is_on_floor():
			velocity.y -= gravity * delta
			# TODO: Replace with "falling"
			if animation_player.current_animation != "idle":
				animation_player.play("idle")
			
			if direction:
				visuals.look_at(position + direction)
				velocity.x = direction.x * SPEED
				velocity.z = direction.z * SPEED
			else:
				velocity.x = move_toward(velocity.x, 0, SPEED)
				velocity.z = move_toward(velocity.z, 0, SPEED)
			
		else:
			if jetpackFuel < jetpackMaxFuel:
				jetpackFuel += jetpackRefuelRate
				
			if direction:
				if animation_player.current_animation != "running":
					animation_player.play("running")
				visuals.look_at(position + direction)
				velocity.x = direction.x * SPEED
				velocity.z = direction.z * SPEED
			
			else:
				if animation_player.current_animation != "idle":
					animation_player.play("idle")
				velocity.x = move_toward(velocity.x, 0, SPEED)
				velocity.z = move_toward(velocity.z, 0, SPEED)
		
		# If other, not locking action pressed, execute action and over write animation / merge animation
		if Input.is_action_just_pressed("shoot_throw"):
			# Spawn projectile
			var target = rayCast_3d.get_collision_point()
			
			
		
		move_and_slide()
