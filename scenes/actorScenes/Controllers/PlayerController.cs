using Godot;
using MMOTest.scripts.Managers;
using Newtonsoft.Json.Linq;
using System;

public partial class PlayerController : AbstractController
{

    [Export]
    float HorizontalMouseSensitivity = 0.2f;
    [Export]
    float VerticalMouseSensitity = 0.2f;
    [Export]
    float ScrollSensitivity = 0.25f;

    [Export]
    float Speed = 5.0f;
    [Export]
    float AccelerationRate = 1.0f;
    [Export]
    float JumpVelocity = 5.0f;
    
    float gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    public Vector3 CalculatedVelocity = Vector3.Zero;

    [Export]
    float JetpackMaxFuel = 10;
    float JetPackFuel = 10;
    [Export]
    float JetpackFuelConsumptionRate = 0.1f;
    [Export]
    float JetpackFuelRefillRate = 0.5f;

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

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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

        CalculatedVelocity = CalculatedVelocity * 0.85f;

        if (Input.IsActionJustPressed("shoot_throw")) {
            Vector3 point = (DirectionMarker.GlobalPosition - Camera.GlobalPosition).Normalized();
            JObject job = new JObject
            {
                { "posx", Model.Position.X },
                { "posy", Model.Position.Y + 1.5},
                { "posz", Model.Position.Z },
                { "velx", point.X},
                { "vely", point.Y},
                { "velz", point.Z},
                { "type", "cast"},
                { "spell", "Fireball"}
            };
            this.GetParent<TestLevel>().RpcId(1,"SendMessage", job.ToString());
        }

        Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (Model.IsOnFloor())
        {
            if (JetPackFuel < JetpackMaxFuel)
            {
                JetPackFuel += JetpackFuelRefillRate;
            }

            if (direction != Vector3.Zero)
            {
                if (ModelAnimation.CurrentAnimation != "running")
                {
                    ModelAnimation.Play("running");
                }

                Model.LookAt(Model.Position + direction);
                CalculatedVelocity += direction;
            }
            
        }
        else
        {
            //this is the gravity applied to the model
            CalculatedVelocity -= new Vector3(Model.Velocity.X, 1.15f *gravity * (float)delta, Model.Velocity.Z);
        }

        Model.Velocity = CalculatedVelocity;
        Model.MoveAndSlide();
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
        CalculatedVelocity += vec;
    }
}
