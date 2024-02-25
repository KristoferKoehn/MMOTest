using Godot;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System;

public partial class PlayerController : AbstractController
{

    [Export] float HorizontalMouseSensitivity = 0.2f;
    [Export] float VerticalMouseSensitity = 0.2f;
    [Export] float ScrollSensitivity = 0.25f;

    Vector3 gravity = (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector") * (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    [Export] private float mass = 80; // kilograms. Default for all characters.
    [Export] private float realMass = 100; // Total mass with armor, TODO: set this from stats or something somehow
    
    // Alot easier to give accelerations than velocities. I might be able to swap these for max velocity with some extra math later.
    [Export] private float groundAcceleration = 1.25f;
    [Export] private float airAcceleration = 1f; // Left flexible for tuning. might need to be set porportional to ground speed, with the porportion being an "air control" value different characters have.
    [Export] private float waterAcceleration = 0.5f; // Not testable yet.
    [Export] private float jumpHeight = 3.0f;

    // Calculated from exports
    private float instantaneousSprintForce;
    private float jumpVelocity;
    private float instantaneousAirManueverForce;
    private float instantaneousSwimForce;


    private Vector3 totalForce = new Vector3();
    private Vector3 externalForce = new Vector3();
    private float surfaceForce; // Set to one of the other force values, depending on which surface we are moving on / through
    // Actually, might want to seperate the whole on / through thing.

    [Export] float JetpackMaxFuel = 10;
    float JetPackFuel = 10;
    [Export] float JetpackFuelConsumptionRate = 0.1f;
    [Export] float JetpackFuelRefillRate = 0.5f;
    [Export] private float jetPackForce = 1500; // Arbitray. Might turn into a calculation later. Give it a better handle 

    bool IsPositionLocked = false;

    AbstractModel Model;
    AnimationPlayer ModelAnimation;

    //model node paths
	Node3D CameraVerticalRotationPoint;
    Node3D DirectionMarker;
    Camera3D Camera;

    public override void _EnterTree()
    {
        CameraVerticalRotationPoint = GetNode<Node3D>("CameraVerticalRotationPoint");
        DirectionMarker = GetNode<Node3D>("CameraVerticalRotationPoint/Marker3D");
        Camera = GetNode<Camera3D>("CameraVerticalRotationPoint/Camera3D");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // So this whole bit feels alittle backwards. The forces are ultimately what moves the model, we are inferring what those forces should be from exported variables that are more intuitive to set.
        instantaneousSprintForce = mass * groundAcceleration;
        jumpVelocity = (float)Math.Sqrt(jumpHeight * 2 * -gravity.Y); // This one is alittle bit of a doozy. Has to do with velocity averages and calculating time to max height
        instantaneousAirManueverForce = mass * airAcceleration;
        instantaneousSwimForce = mass * waterAcceleration;
        // At some point, it might make sense to derive these from like, strength. Its how much force you can exert to move in a direction. What you get out of that has to do with friction though. which means, mass, normal forces, etc.
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) // move controller to position of model, tween for smooth camera movement
	{
        if(Model != null)
        {
            Tween t = CreateTween();
            t.TweenProperty(this, "position", Model.GlobalPosition + new Vector3(0, 1.4f, 0), 0.016f);
            t.Play();
        }
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

        Vector2 inputDirection = Input.GetVector("left", "right", "forward", "backward");
        Vector3 internalForce = (Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y)).Normalized();
        
        // Update to better function in the future when other surface detections are implemented, add logic for water, ice, etc.
        if (Model.IsOnFloor())
        {
            surfaceForce = instantaneousSprintForce;
        }
        else
        {
            surfaceForce = instantaneousAirManueverForce; 
        }
        // Set internal force after selecting surface force
        internalForce *= surfaceForce;

        if (Input.IsActionJustPressed("jump_dodge") & Model.IsOnFloor())
        {
            Model.Velocity += Vector3.Up * jumpVelocity;
        }

        if (Input.IsActionPressed("movementAbility"))
        {
            // Change logic to reflect kit. For now -> mage hover
            ApplyImpulse(Vector3.Up * jetPackForce);
            // Do fuel and stuff
        }

        totalForce += internalForce;
        totalForce += externalForce;
        externalForce = Vector3.Zero;


        Model.Velocity = Model.Velocity + ((float)delta * ((totalForce / realMass) + gravity));
        totalForce = Vector3.Zero;
        
        Model.MoveAndSlide();

        

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
        //else
        //{
        //    //this is the gravity applied to the model
        //    //AppliedGravity = //new Vector3(Model.Velocity.X, 1.15f *gravity * (float)delta, Model.Velocity.Z);
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
        externalForce += vec;
    }
}
