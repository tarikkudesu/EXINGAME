using Godot;
using System;

public partial class Mutant : CharacterBody3D
{
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
	
	[Export]
	public string Label { get; set; } = "";

	private CharacterBody3D player;
	private NavigationAgent3D navAgent;
	private AnimationTree animationTree;
	private Vector3 spawnPosition;
	private bool useSwipe = true;
	private float attackTimer = 0.0f;
	private float attackCooldown = 1.5f; // Time between attacks
	
	public override void _Ready()
	{
		// Store the spawn position
		spawnPosition = GlobalPosition;
		
		// Get the player from global group
		player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody3D;
		if (player == null)
		{
			GD.PrintErr("Player not found in 'Player' group!");
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
		if (player == null)
			return;
		if (!IsOnFloor())
			Velocity = new Vector3(Velocity.X, Velocity.Y - Gravity * (float)delta, Velocity.Z);
		else
			Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
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

		if (inAttackRange) {
			if (useSwipe) {
				animationTree.Set("parameters/conditions/swipe", true);
				animationTree.Set("parameters/conditions/punch", false);
			} else {
				animationTree.Set("parameters/conditions/punch", true);
				animationTree.Set("parameters/conditions/swipe", false);
			}
			useSwipe = !useSwipe;
		}
		else if (distanceToPlayer <= DetectionRange && distanceToPlayer > MinDistanceToPlayer && distanceFromSpawn < MaxDistanceFromSpawn)
		{
			if (navAgent != null)
			{
				navAgent.TargetPosition = player.GlobalPosition;
				if (!navAgent.IsNavigationFinished())
				{
					Vector3 nextTargetPosition = navAgent.GetNextPathPosition();
					Vector3 direction = (nextTargetPosition - GlobalPosition);
					direction.Y = 0;
					direction = direction.Normalized();
					Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
				}
				else
				{
					Velocity = new Vector3(0, Velocity.Y, 0);
				}
			}
			else
			{
				Vector3 direction = (player.GlobalPosition - GlobalPosition);
				direction.Y = 0;
				direction = direction.Normalized();
				Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
			}
			animationTree.Set("parameters/conditions/punch", false);
			animationTree.Set("parameters/conditions/swipe", false);
			animationTree.Set("parameters/conditions/run", true);
		}
		else
		{
			Velocity = new Vector3(0, Velocity.Y, 0);
			animationTree.Set("parameters/conditions/run", false);
			animationTree.Set("parameters/conditions/punch", false);
			animationTree.Set("parameters/conditions/swipe", false);
		}
		MoveAndSlide();
	}
	
	private bool IsTargetInRange()
	{
		if (player == null)
			return false;
			
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		return distanceToPlayer <= AttackRange;
	}
}
