using Godot;
using System;

public partial class Mutant : CharacterBody3D
{
	[Export]
	public NodePath PlayerNodePath { get; set; }
	
	[Export]
	public float DetectionRange = 10.0f;
	
	[Export]
	public float Gravity = 9.8f;
	
	private CharacterBody3D player;
	private NavigationAgent3D navAgent;
	private const float Speed = 5.0f;
	
	public override void _Ready()
	{
		// Get the player node using the exported path
		player = GetNode<CharacterBody3D>(PlayerNodePath);
		
		// Get the NavigationAgent3D child node
		navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		
		// Wait for navigation to be ready (important for first frame)
		CallDeferred(MethodName.SetupNavigation);
	}
	
	private void SetupNavigation()
	{
		// Set initial target to player position
		navAgent.TargetPosition = player.GlobalPosition;
		GD.Print("Navigation setup complete");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Apply gravity
		if (!IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, Velocity.Y - Gravity * (float)delta, Velocity.Z);
			GD.Print("Mutant is falling");
		}
		else
		{
			// Reset vertical velocity when on floor
			Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
		}
		
		// Calculate distance to player
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		GD.Print($"Distance to player: {distanceToPlayer:F2} | Detection Range: {DetectionRange}");
		
		// Only chase if player is within detection range and navigation is ready
		if (distanceToPlayer <= DetectionRange && navAgent.IsNavigationFinished() == false)
		{
			// Update the navigation target to the player's position
			navAgent.TargetPosition = player.GlobalPosition;
			
			// Check if we have a valid path
			if (navAgent.IsNavigationFinished() == false)
			{
				// Get the next point in the path
				Vector3 nextTargetPosition = navAgent.GetNextPathPosition();
				
				// Calculate direction to the next point (only on XZ plane)
				Vector3 direction = (nextTargetPosition - GlobalPosition);
				direction.Y = 0; // Ignore vertical difference
				direction = direction.Normalized();
				
				// Set horizontal velocity
				Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
				
				GD.Print($"Chasing player - Velocity: {Velocity}");
			}
		}
		else if (distanceToPlayer <= DetectionRange)
		{
			// Player is in range, update target even if nav isn't ready yet
			navAgent.TargetPosition = player.GlobalPosition;
			GD.Print("Player in range, waiting for navigation...");
		}
		else
		{
			GD.Print("Player out of range");
		}
		
		// Move the mutant
		MoveAndSlide();
	}
}
