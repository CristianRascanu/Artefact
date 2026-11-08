using Godot;
using System;

public partial class Camera : Camera3D
{
	[Export] public float MouseSensitivity = 0.2f;
	[Export] public float MoveSpeed = 5.0f;
	[Export] public float SprintMultiplier = 2.0f;

	private float yaw= 0.0f;
	private float pitch = 0.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event){
		if(@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured){
			yaw -= motion.Relative.X * MouseSensitivity;
			pitch -= motion.Relative.Y * MouseSensitivity;

			pitch = Mathf.Clamp(pitch, -89f, 89f);

			RotationDegrees = new Vector3(pitch, yaw, 0);
		}

		if(@event.IsActionPressed("ui_cancel")){
			Input.MouseMode = Input.MouseMode== Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}
	}

	public override void _Process(double delta){
		Vector3 direction =  Vector3.Zero;

		if(Input.IsActionPressed("move_forward"))
			direction -= Transform.Basis.Z;
		if(Input.IsActionPressed("move_backward"))
			direction += Transform.Basis.Z;
		if(Input.IsActionPressed("move_right"))
			direction += Transform.Basis.X;
		if(Input.IsActionPressed("move_left"))
			direction -= Transform.Basis.X;
		if(Input.IsActionPressed("move_up"))
			direction += Transform.Basis.Y;
		if(Input.IsActionPressed("move_down"))
			direction -= Transform.Basis.Y;

		float speed = MoveSpeed;
		if(Input.IsActionPressed("sprint"))
			speed *= SprintMultiplier;
		if(direction != Vector3.Zero)
			Translate(direction.Normalized() * speed * (float)delta);
	}
}
