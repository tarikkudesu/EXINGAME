using Godot;
using System;

public partial class Mutant : CharacterBody3D
{
	[Export]
	public NodePath PlayerNodePath { get; set; }
	
	[Export]
	public NodePath NavigationAgentPath { get; set; }
	
	[Export]
	public float DetectionRange = 10.0f;
	
	[Export]
	public float MinDistanceToPlayer = 1.5f;
	
	[Export]
	public float MaxDistanceFromSpawn = 20.0f;
	
	[Export]
	public float Gravity = 30.0f;
	
	[Export]
	public float Speed = 5.0f;
	
	private CharacterBody3D player;
	private NavigationAgent3D navAgent;
	private Vector3 spawnPosition;
	
	public override void _Ready()
	{
		GD.Print("=== Mutant _Ready() called ===");
		
		// Store the spawn position
		spawnPosition = GlobalPosition;
		GD.Print($"Spawn position: {spawnPosition}");
		
		// Get the player node using the exported path
		GD.Print($"PlayerNodePath: {PlayerNodePath}");
		if (PlayerNodePath != null && !PlayerNodePath.IsEmpty)
		{
			player = GetNode<CharacterBody3D>(PlayerNodePath);
			if (player == null)
			{
				GD.PrintErr("Player node not found at path: " + PlayerNodePath);
			}
			else
			{
				GD.Print("Player node found successfully");
			}
		}
		else
		{
			GD.PrintErr("PlayerNodePath is not set!");
		}
		
		// Get the NavigationAgent3D using the exported path
		GD.Print($"NavigationAgentPath: {NavigationAgentPath}");
		if (NavigationAgentPath != null && !NavigationAgentPath.IsEmpty)
		{
			GD.Print("Attempting to get NavigationAgent3D...");
			try
			{
				navAgent = GetNode<NavigationAgent3D>(NavigationAgentPath);
				GD.Print("GetNode call completed");
				if (navAgent == null)
				{
					GD.PrintErr("NavigationAgent3D not found at path: " + NavigationAgentPath);
				}
				else
				{
					GD.Print("NavigationAgent3D found successfully");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error getting NavigationAgent3D: {ex.Message}");
				GD.PrintErr($"Exception type: {ex.GetType().Name}");
				GD.PrintErr($"Stack trace: {ex.StackTrace}");
			}
		}
		else
		{
			GD.PrintErr("NavigationAgentPath is not set! Mutant will use direct movement.");
		}
		
		// Wait for navigation to be ready (important for first frame)
		if (player != null && navAgent != null)
		{
			CallDeferred(MethodName.SetupNavigation);
		}
		
		GD.Print("=== Mutant _Ready() complete ===");
	}
	
	private void SetupNavigation()
	{
		if (player != null && navAgent != null)
		{
			// Set initial target to player position
			navAgent.TargetPosition = player.GlobalPosition;
			GD.Print("Navigation setup complete");
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Early return if player is not found
		if (player == null)
		{
			GD.PrintErr("Player is null - cannot update mutant behavior");
			return;
		}
		
		// Apply gravity
		if (!IsOnFloor())
		{
			Velocity = new Vector3(Velocity.X, Velocity.Y - Gravity * (float)delta, Velocity.Z);
		}
		else
		{
			// Reset vertical velocity when on floor
			Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
		}
		
		// Calculate distance to player
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		
		// Calculate distance from spawn point
		float distanceFromSpawn = GlobalPosition.DistanceTo(spawnPosition);
		
		// Only chase if player is within detection range, not too close, and mutant hasn't strayed too far from spawn
		if (distanceToPlayer <= DetectionRange && distanceToPlayer > MinDistanceToPlayer && distanceFromSpawn < MaxDistanceFromSpawn)
		{
			// If NavigationAgent3D exists, use it for pathfinding
			if (navAgent != null)
			{
				// Update the navigation target to the player's position
				navAgent.TargetPosition = player.GlobalPosition;
				
				// Check if we have a valid path and haven't reached the target
				if (!navAgent.IsNavigationFinished())
				{
					// Get the next point in the path
					Vector3 nextTargetPosition = navAgent.GetNextPathPosition();
					
					// Calculate direction to the next point (only on XZ plane)
					Vector3 direction = (nextTargetPosition - GlobalPosition);
					direction.Y = 0; // Ignore vertical difference
					direction = direction.Normalized();
					
					// Set horizontal velocity
					Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
				}
			}
			else
			{
				// Fallback: Move directly toward player without pathfinding
				Vector3 direction = (player.GlobalPosition - GlobalPosition);
				direction.Y = 0; // Ignore vertical difference
				direction = direction.Normalized();
				
				// Set horizontal velocity
				Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
			}
		}
		else
		{
			// Stop horizontal movement when player is out of range or too close
			Velocity = new Vector3(0, Velocity.Y, 0);
		}
		
		// Move the mutant
		MoveAndSlide();
	}
}
