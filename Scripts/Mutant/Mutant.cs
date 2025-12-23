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
	
	[Export]
	public float AttackRange = 3.0f;
	
	private CharacterBody3D player;
	private NavigationAgent3D navAgent;
	private AnimationTree animationTree;
	private Vector3 spawnPosition;
	private bool useSwipe = true; // Alternate between swipe and punch
	private float attackTimer = 0.0f;
	private float attackCooldown = 1.5f; // Time between attacks
	
	public override void _Ready()
	{
		// Store the spawn position
		spawnPosition = GlobalPosition;
		
		// Get the player node using the exported path
		if (PlayerNodePath != null && !PlayerNodePath.IsEmpty)
		{
			player = GetNode<CharacterBody3D>(PlayerNodePath);
			if (player == null)
			{
				GD.PrintErr("Player node not found at path: " + PlayerNodePath);
			}
		}
		else
		{
			GD.PrintErr("PlayerNodePath is not set!");
		}
		
		// Get the NavigationAgent3D using the exported path
		if (NavigationAgentPath != null && !NavigationAgentPath.IsEmpty)
		{
			try
			{
				var node = GetNode(NavigationAgentPath);
				navAgent = node as NavigationAgent3D;
				
				if (navAgent == null)
				{
					GD.PrintErr($"NavigationAgentPath points to a {node.GetType().Name}, not a NavigationAgent3D!");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error getting NavigationAgent3D: {ex.Message}");
			}
		}
		
		// Get the AnimationTree child node
		animationTree = GetNodeOrNull<AnimationTree>("AnimationTree");
		if (animationTree == null)
		{
			GD.PrintErr("AnimationTree not found as child of Mutant!");
		}
		
		// Wait for navigation to be ready (important for first frame)
		if (player != null && navAgent != null)
		{
			CallDeferred(MethodName.SetupNavigation);
		}
	}
	
	private void SetupNavigation()
	{
		if (player != null && navAgent != null)
		{
			navAgent.TargetPosition = player.GlobalPosition;
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		// Early return if player is not found
		if (player == null)
		{
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
		
		// Check if player is in attack range
		bool inAttackRange = distanceToPlayer <= AttackRange;
		
		// Rotate to face the player when in detection range
		if (distanceToPlayer <= DetectionRange)
		{
			Vector3 lookTarget = new Vector3(player.GlobalPosition.X, GlobalPosition.Y, player.GlobalPosition.Z);
			LookAt(lookTarget, Vector3.Up);
		}
		
		// Only chase if player is within detection range, not in attack range, and mutant hasn't strayed too far from spawn
		if (distanceToPlayer <= DetectionRange && !inAttackRange && distanceToPlayer > MinDistanceToPlayer && distanceFromSpawn < MaxDistanceFromSpawn)
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
				else
				{
					// Stop if navigation finished
					Velocity = new Vector3(0, Velocity.Y, 0);
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
			// Stop horizontal movement when player is out of range, too close, or mutant is too far from spawn
			Velocity = new Vector3(0, Velocity.Y, 0);
		}
		
		// Move the mutant
		MoveAndSlide();
		
		// Update animation tree conditions
		UpdateAnimationTree();
	}
	
	private bool IsTargetInRange()
	{
		if (player == null)
			return false;
			
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		return distanceToPlayer <= AttackRange;
	}
	
	private void UpdateAnimationTree()
	{
		if (animationTree == null)
			return;
		
		bool isRunning = Velocity.Length() > 0.1f; // Check if mutant is moving
		bool inAttackRange = IsTargetInRange();
		
		// Set run condition (only run when moving and not attacking)
		animationTree.Set("parameters/conditions/run", isRunning && !inAttackRange);
		
		// Set stretch condition to return to idle when not running and not attacking
		animationTree.Set("parameters/conditions/stretch", !isRunning && !inAttackRange);
		
		// Handle attack animations with cooldown
		if (inAttackRange)
		{
			// Increment attack timer
			attackTimer += (float)GetPhysicsProcessDeltaTime();
			
			// Trigger attack when cooldown is ready
			if (attackTimer >= attackCooldown)
			{
				if (useSwipe)
				{
					animationTree.Set("parameters/conditions/swipe", true);
				}
				else
				{
					animationTree.Set("parameters/conditions/punch", true);
				}
				
				// Toggle for next attack and reset timer
				useSwipe = !useSwipe;
				attackTimer = 0.0f;
			}
		}
		else
		{
			// Reset attack timer when not in range
			attackTimer = 0.0f;
		}
	}
}
