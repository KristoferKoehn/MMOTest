using Godot;
using MMOTest.scripts.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public partial class PlayerController : AbstractController
{
    //model node paths
    AbstractModel Model;
    AnimationPlayer ModelAnimation;

    Node3D CameraVerticalRotationPoint;
    Node3D DirectionMarker;
    Camera3D Camera;

    [Export] float HorizontalMouseSensitivity = 0.2f;
    [Export] float VerticalMouseSensitity = 0.2f;
    [Export] float ScrollSensitivity = 0.25f;

    // Used if position needs to be halted for animations
    bool IsPositionLocked = false;

    // Physics "constants"
    Vector3 gravity = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector") * (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    private float airDensity = 1.293f; // Should probably come from project settings
    private float waterDensity = 998f; // Should probably come from project settings
    private float fluidDensity; // 1.293 for air, 998 for water.

    private float dragCoefficient = 1.15f; // Ranges between 1.0 and 1.3 for a person. https://en.wikipedia.org/wiki/Drag_coefficient
    private float staticFrictionCoefficient = 1f; // best guess. Leather on wood, with the grain is 0.61. So a leather shoe on stone or dirt? idk. a bit higher. // Needs a setter and surface detection of some kind? so we can switch to an ice coefficient? Also, kinetic friction at some point? https://www.engineeringtoolbox.com/friction-coefficients-d_778.html 
    private float kineticFrictionCoefficient = 0.9f; // Also a guess 

    // Physics exports
    // These are about the character itself
    [Export] private float mass = 80f; // kilograms. Default for all characters.
    [Export] private float realMass = 100f; // Total mass with armor, TODO: set this from stats or something somehow
    [Export] private float modelProjectedArea = 0.7f; // Used for air resistance, https://www.ntnu.no/documents/11601816/b830b9bd-d256-42c4-9dfc-5726c0ae3596
    [Export] private float modelVolume = 0.075f; // cubic meters, surprisingly.

    // These are about the desired behavior of the character
    [Export] private float jumpHeight = 3f; // 3 meters is way higher than people can jump but 0.3 feels bad because you cant pick up your legs to clear a fence.
    [Export] private float maxSprintSpeed = 10f;
    [Export] private float sprintAcceleration = 1.25f; // 10 meters a second. Ballpark of olympic athletes in 200m races TODO: Use this somehow.
    [Export] private float maxSwimSpeed = 2.2f; // 2.2 Meters per second. https://www.wired.com/2012/08/olympics-physics-swimming/
    [Export] private float maxFlySpeed = 0f; // People cant fly. Should be zero, but having it at 10 helps a bit.The air thrust force needs to be calculated differently. Drag doesnt make sense.
    [Export] private float angleOfAttack = (float)(Math.PI / 4f);// How much you glide while falling;

    // These are derived from the exported values and are actually used in calculations
    private float jumpVelocity;
    private float jumpForce;
    private float angleOfAttackThrustForce; // Derived from angle of attack and drag
    private float airThrustForce ; // Force generated to fly, like a bird.
    private float swimThrustForce; // Force generated to swim.
    private float thrustForce; // total Thrust force

    // Forces
    // Internal Force
    private Vector2 inputDirection = new Vector2(); // Captures movement key presses
    private Vector3 currentRunningSpeedVector = new Vector3(); // How the character is running in a given direction
    private Vector3 runningSpeedAccelerationVector = new Vector3();
    private Vector3 internalForceVector = new Vector3(); // Represents the force the character is exerting inorder to move in a certain direction.
    private Vector3 thrustForceVector = new Vector3(); // Force used to move in a given direction in a fluid (air, water) and not on the ground. Internal force = thrust force when off the ground.
    private Vector3 runningForceVector = new Vector3(); // thrustForce On ground counterpart. Used to move in a given direction on the ground.
    
    // External Force 
    private Vector3 externalForceVector = new Vector3(); // Stuff that pushes on the model. Explosions, Wind, etc. (Wind would technically impact drag too)

    // Resistance Forces
    private Vector3 dragForceVector = new Vector3(); // Friction while moving through a fluid (off the ground)
    private Vector3 frictionForceVector = new Vector3(); // Dampens movement of objects sliding on the ground.
    private float normalForce; // Used to calculate friction
    private Vector3 movementResistanceForceVector = new Vector3(); // Represents forces from the environment that slow the character down. Friction and drag.
    private Vector3 buoyantForceVector = new Vector3(); // Used to float in fluids (or sink)

    // Total
    private Vector3 totalForceVector = new Vector3(); // Accumulates all forces and applies it to the model.
    
    
    // Jetpack stuff, mage only
    [Export] private float JetpackMaxFuel = 10f;
    private float JetPackFuel = 10f;
    [Export] private float JetpackFuelConsumptionRate = 0.1f;
    [Export] private float JetpackFuelRefillRate = 0.5f;
    [Export] private float jetPackForce = 1500f; // Arbitrary. Might turn into a calculation later. Give it a better handle
    [Export] private float propulsionThrustForce = 1500f;

    
    public override void _EnterTree()
    {
        CameraVerticalRotationPoint = GetNode<Node3D>("CameraVerticalRotationPoint");
        DirectionMarker = GetNode<Node3D>("CameraVerticalRotationPoint/Marker3D");
        Camera = GetNode<Camera3D>("CameraVerticalRotationPoint/Camera3D");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
 
        // So this whole bit feels a little backwards. The forces are ultimately what moves the model, we are inferring what those forces should be from exported variables that are more intuitive to set.
        jumpVelocity = (float)Math.Sqrt(jumpHeight * 2 * -gravity.Y); // This one is a little bit of a doozy. Has to do with velocity averages and calculating time to max height
        jumpForce = jumpVelocity * mass * 60; // 60 for 60fps. This will be multiplied by delta later, so the 60 is here to cancel it out.
        swimThrustForce = 0.5f * waterDensity * maxSwimSpeed * maxSwimSpeed * dragCoefficient * modelProjectedArea; // Needs to be equal to drag at max speed.
        //airThrustForce = 0.5f * airDensity * maxFlySpeed * maxFlySpeed * dragCoefficient * modelProjectedArea; // Needs to be equal to drag at max speed.
        
        fluidDensity = airDensity; // Air by default. Should probably make a check here
        thrustForce = airThrustForce;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (Input.IsActionJustPressed("pause"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
        
	}

    public void AttachModel(AbstractModel model)
    {
        this.Model = model;
        this.ModelAnimation = model.GetNode<AnimationPlayer>("AnimationPlayer");
        Model.AttachController(this);
        PlayerUI p = GD.Load<PackedScene>("res://scenes/PlayerUI/PlayerUI.tscn").Instantiate<PlayerUI>();
        p.initialize(model.GetActorID());
        this.AddChild(p);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Model == null) { return; }

        if (Model.IsDead) {
            totalForceVector = Vector3.Zero;
            externalForceVector = Vector3.Zero;
            internalForceVector = Vector3.Zero;
            return;
        }

        this.GlobalPosition = this.Model.GlobalPosition;

        if (Input.IsActionJustPressed("shoot_throw")) {
            Vector3 point = (DirectionMarker.GlobalPosition - Camera.GlobalPosition).Normalized();
            JObject job = new JObject
            {
                { "spell", "Fireball"},
                { "type", "cast"},
                { "posx", Model.Position.X },
                { "posy", Model.Position.Y + 1.5},
                { "posz", Model.Position.Z },
                { "velx", point.X},
                { "vely", point.Y},
                { "velz", point.Z},
                { "SourceID", Model.GetActorID()}
            };
            MessageQueue.GetInstance().RpcId(1, "AddMessage", JsonConvert.SerializeObject(job));
            //this.GetParent<MainLevel>().RpcId(1,"SendMessage", job.ToString());
        }

        inputDirection = Input.GetVector("left", "right", "forward", "backward");
        internalForceVector = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();
        
        // Update to better function in the future when other surface detections are implemented, add logic for water, ice, etc.
        if (Model.IsOnFloor())
        {
            // Logic for running

            // Spin up "engine"
            runningSpeedAccelerationVector = internalForceVector * (sprintAcceleration * (float)delta);
            currentRunningSpeedVector += runningSpeedAccelerationVector;
            if (runningSpeedAccelerationVector == Vector3.Zero) // Ramp down (foot off gas)
            {
                if (currentRunningSpeedVector.Length() > (sprintAcceleration * (float)delta))
                {
                    runningSpeedAccelerationVector = Vector3.Zero;
                }
                else
                {
                    Vector3 decelerationVector = -currentRunningSpeedVector.Normalized() * (sprintAcceleration * (float)delta);
                    currentRunningSpeedVector += decelerationVector;
                }
            }

            if (currentRunningSpeedVector.Length() > maxSprintSpeed)
            { 
                currentRunningSpeedVector = currentRunningSpeedVector.Normalized() * maxSprintSpeed;
            }

            currentRunningSpeedVector.Y = Model.Velocity.Y; // Setting equal here means that the y component of the velocity wont be considered when calculating attempted acceleration
            float attemptedAcceleration = Math.Abs(currentRunningSpeedVector.Length() - Model.Velocity.Length()) / (float)delta;
            float runningForce = realMass * attemptedAcceleration;
            
            normalForce = Model.GetFloorNormal().Normalized().Y * (realMass * Math.Abs(gravity.Y));
            float maxStaticFrictionForce = staticFrictionCoefficient * normalForce;
            if (runningForce >= maxStaticFrictionForce)
            {
                runningForce = kineticFrictionCoefficient * normalForce; // Sliding out
                
                // Friction added to sliding
                frictionForceVector = -Model.Velocity.Normalized() * (kineticFrictionCoefficient * normalForce);
            }
            else
            {
                frictionForceVector = new Vector3();
            }

            //if (runningForce > 1500)
            //{
            //    GD.Print(runningForce);
            //}

            runningForceVector = currentRunningSpeedVector.Normalized() * runningForce;
            internalForceVector = runningForceVector;

            movementResistanceForceVector = frictionForceVector;
        }
        else
        {
            // Drag equation
            dragForceVector = -Model.Velocity.Normalized() * (0.5f * fluidDensity * Model.Velocity.Length() * Model.Velocity.Length() * dragCoefficient * modelProjectedArea);
            if (inputDirection.Y != 0 || inputDirection.X != 0) // There is input.
            {
                angleOfAttackThrustForce = Math.Abs(dragForceVector.Y) * (float)Math.Cos((double)angleOfAttack);
                dragForceVector.Y *= (float)Math.Sin(angleOfAttack);
            }
            
            movementResistanceForceVector = dragForceVector;

            // Because we aren't considering pressure, this works the same in air and water. 
            thrustForceVector = internalForceVector * (thrustForce + angleOfAttackThrustForce + propulsionThrustForce); 
            internalForceVector = thrustForceVector;
        }

        // Jump
        if (Input.IsActionJustPressed("jump_dodge") & Model.IsOnFloor())
        {
            internalForceVector += Vector3.Up * jumpForce;
        }

        if (Input.IsActionPressed("movementAbility"))
        {
            // Change logic to reflect kit. For now -> mage hover
            internalForceVector += Vector3.Up * jetPackForce;
            // Do fuel and stuff
        }

        //buoyantForceVector = Vector3.Up * (-1 * fluidDensity * gravity.Y * modelVolume);
        //externalForceVector += buoyantForceVector;


        // NOT PHYSICAL ADDED FOR GAME FEEL
        // Internal force is scaled to be up to twice as strong if it is in a direction opposite current velocity.
        float dotProduct = internalForceVector.Normalized().Dot(Model.Velocity.Normalized());
        dotProduct = (dotProduct / -2f) + 1.5f; // Magic numbers to convert range of [-1,1] to [1,2] but reversed
        internalForceVector *= dotProduct;

        // Sum up all forces. Internal, external, and friction
        totalForceVector += internalForceVector;
        totalForceVector += externalForceVector;
        externalForceVector = Vector3.Zero; // Reset to begin accumulation again.
        totalForceVector += movementResistanceForceVector;
        
        // Update Model velocity. V_next = v_current + (time * acceleration). Acceleration = force / mass. Gravity is an acceleration value, so it is added to acceleration.
        Model.Velocity = Model.Velocity + ((float)delta * ((totalForceVector / realMass) + gravity));
        totalForceVector = Vector3.Zero; // Reset force to recalculate next frame.
        
        Model.MoveAndSlide(); // Move the model according to its new velocity.

        
        // Kept for reference on setting animations, rotating the model, and jet pack logic.
        //if (direction != Vector3.Zero)
        //{
        //    if (ModelAnimation.CurrentAnimation != "running")
        //    {
        //        ModelAnimation.Play("running");
        //    }

        //    Model.LookAt(Model.Position + direction);
        //    //CalculatedVelocity += direction;
        //}

        //Model.Velocity = CalculatedVelocity + AppliedGravity + (direction * Speed);
        //Model.MoveAndSlide();
        //if (Model.IsOnFloor())
        //{
        //    AppliedGravity = Vector3.Zero;
        //    CalculatedVelocity = new Vector3(CalculatedVelocity.X, 0, CalculatedVelocity.Z);
        //    if (JetPackFuel < JetpackMaxFuel)
        //    {
        //        JetPackFuel += JetpackFuelRefillRate;
        //    }

        //}
    }


    public override void _Input(InputEvent @event)
    {

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            InputEventMouseButton button = @event as InputEventMouseButton;
            if (button != null)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            } else
            {
                return;
            }
        }

        InputEventMouseMotion motion = @event as InputEventMouseMotion;
		if (motion != null)
		{
            this.RotateY(Mathf.DegToRad(-motion.Relative.X * HorizontalMouseSensitivity));
            CameraVerticalRotationPoint.RotateX(Mathf.DegToRad(-motion.Relative.Y * VerticalMouseSensitity));
            if (CameraVerticalRotationPoint.Rotation.X > Mathf.DegToRad(90))
            {
                CameraVerticalRotationPoint.RotateX(Mathf.DegToRad(90) - CameraVerticalRotationPoint.Rotation.X);
            }
            if (CameraVerticalRotationPoint.Rotation.X < Mathf.DegToRad(-90))
            {
                CameraVerticalRotationPoint.RotateX(Mathf.DegToRad(-90) - CameraVerticalRotationPoint.Rotation.X);
            }
		}
    }

    public override void ApplyImpulse(Vector3 vec)
    {
        externalForceVector += vec;
    }

    public void SetWaterFlag(bool flag)
    {
        if (flag)
        {
            fluidDensity = waterDensity;
            thrustForce = swimThrustForce;
        }
        else
        {
            fluidDensity = airDensity;
            thrustForce = airThrustForce;
        }
    }
}
