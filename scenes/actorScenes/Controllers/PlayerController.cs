using Godot;
using MMOTest.scripts.Managers;
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
    private float fluidDensity = 1.293f; // 1.293 for air, 998 for water. TODO: needs a setter, and a way to detect what material we are in.
    private float dragCoefficient = 1.15f; // Ranges between 1.0 and 1.3 for a person. https://en.wikipedia.org/wiki/Drag_coefficient
    private float frictionCoefficient = 0.7f; // best guess. Leather on wood, with the grain is 0.61. So a leather shoe on stone or dirt? idk. a bit higher. // Needs a setter and surface detection of some kind? so we can switch to an ice coefficient? Also, kinetic friction at some point? https://www.engineeringtoolbox.com/friction-coefficients-d_778.html 

    // Physics exports
    // These are about the character itself
    [Export] private float mass = 80f; // kilograms. Default for all characters.
    [Export] private float realMass = 100f; // Total mass with armor, TODO: set this from stats or something somehow
    [Export] private float modelProjectedArea = 0.7f; // Used for air resistance, https://www.ntnu.no/documents/11601816/b830b9bd-d256-42c4-9dfc-5726c0ae3596

    // These are about the desired behavior of the character
    [Export] private float jumpHeight = 3.0f;
    [Export] private float runningForce = 100f;
    [Export] private float fluidPropulsionForce = 1500f; // Should be changed by class or some "air maneuverability" stat. 1500 might be really high. 

    // These are derived from the exported values and are actually used in calculations
    private float jumpVelocity;
    private float jumpForce;

    // Forces
    // Internal Force
    private Vector2 inputDirection = new Vector2(); // Captures movement key presses
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

    // Total
    private Vector3 totalForceVector = new Vector3(); // Accumulates all forces and applies it to the model.
    
    
    // Jetpack stuff, mage only
    [Export] float JetpackMaxFuel = 10f;
    float JetPackFuel = 10f;
    [Export] float JetpackFuelConsumptionRate = 0.1f;
    [Export] float JetpackFuelRefillRate = 0.5f;
    [Export] private float jetPackForce = 1500f; // Arbitrary. Might turn into a calculation later. Give it a better handle 

    

    public override void _EnterTree()
    {
        CameraVerticalRotationPoint = GetNode<Node3D>("CameraVerticalRotationPoint");
        DirectionMarker = GetNode<Node3D>("CameraVerticalRotationPoint/Marker3D");
        Camera = GetNode<Camera3D>("CameraVerticalRotationPoint/Camera3D");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // So this whole bit feels a little backwards. The forces are ultimately what moves the model, we are inferring what those forces should be from exported variables that are more intuitive to set.
        jumpVelocity = (float)Math.Sqrt(jumpHeight * 2 * -gravity.Y); // This one is a little bit of a doozy. Has to do with velocity averages and calculating time to max height
        jumpForce = jumpVelocity * mass * 60; // 60 for 60fps. This will be multiplied by delta later, so the 60 is here to cancel it out.
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
	}

    public void AttachModel(AbstractModel model)
    {
        this.Model = model;
        this.ModelAnimation = model.GetNode<AnimationPlayer>("AnimationPlayer");
        Model.AttachController(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Model == null) { return; }

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
                { "velz", point.Z}
            };
            this.GetParent<TestLevel>().RpcId(1,"SendMessage", job.ToString());
        }

        inputDirection = Input.GetVector("left", "right", "forward", "backward");
        internalForceVector = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();
        
        // Update to better function in the future when other surface detections are implemented, add logic for water, ice, etc.
        if (Model.IsOnFloor())
        {
            // Logic for running
            // Currently incorrect. Just a copy and paste of the logic for how the character moves in the air for now, but with its own variables.
            runningForceVector = internalForceVector * runningForce; // This could be simplified to update the internal force vector directly. We arent really using the running vector. its just, nice to have it named correctly?
            internalForceVector = runningForceVector;

            // Friction equation. Doesnt handle static vs kinetic
            normalForce = Model.GetFloorNormal().Normalized().Y * (realMass * Math.Abs(gravity.Y));
            frictionForceVector = -Model.Velocity.Normalized() * (frictionCoefficient * normalForce);
            movementResistanceForceVector = frictionForceVector;
        }
        else
        {
            // Because we aren't considering pressure, this works the same in air and water. The difference in movement will be with drag. Will need to be reworked to couple with a swim speed of some kind later. 
            thrustForceVector = internalForceVector * fluidPropulsionForce; // This could be simplified to update the internal force vector directly. We arent really using the thrust vector. its just, nice to have it named correctly?
            internalForceVector = thrustForceVector;

            // Drag equation
            dragForceVector = -Model.Velocity.Normalized() * (0.5f * fluidDensity * Model.Velocity.Length() * Model.Velocity.Length() * dragCoefficient * modelProjectedArea);
            movementResistanceForceVector = dragForceVector;
        }
        
        // Jump
        if (Input.IsActionJustPressed("jump_dodge") & Model.IsOnFloor())
        {
            internalForceVector += Vector3.Up * jumpForce; // Should work. needs to be tested. Might need additional force from ground normal.
        }

        if (Input.IsActionPressed("movementAbility"))
        {
            // Change logic to reflect kit. For now -> mage hover
            internalForceVector += Vector3.Up * jetPackForce;
            // Do fuel and stuff
        }

        // Sum up all forces. Internal, external, and friction
        totalForceVector += internalForceVector;
        totalForceVector += externalForceVector;
        externalForceVector = Vector3.Zero; // Reset to begin accumulation again.
        totalForceVector += movementResistanceForceVector;
        
        // Update Model velocity. V_next = v_current + (time * acceleration). Acceleration = force / mace. Gravity is an acceleration value, so it is added to acceleration.
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
}
