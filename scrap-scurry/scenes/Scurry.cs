using Godot;
using System;

public partial class Scurry : CharacterBody2D
{
	public const float Speed = 150.0f;
	public const float JumpVelocity = -400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero) // Liikkuminen X-akselilla
		{
			velocity.X = direction.X * Speed;
		}
		
		else // Pysähtyminen X-akselilla
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (direction != Vector2.Zero) // Liikkuminen Y-akselilla
		{
			velocity.Y = direction.Y * Speed;
		}
		else // Pysähtyminen X-akselilla
		{
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}
}


//		// Add the gravity.
//		if (!IsOnFloor())
//		{
//			velocity += GetGravity() * (float)delta;
//		}
//
//		// Handle Jump.
//		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
//		{
//			velocity.Y = JumpVelocity;
//		}
