using Godot;
using System;

public partial class Mutant : Node3D
{
	private AnimationPlayer _animationPlayer;
	private Timer _stretchTimer;
	private bool _isStretching = false;
	
	// Animation names
	private const string ANIM_IDLE = "Swiping";  // Looping idle animation
	private const string ANIM_STRETCH = "Stretch";  // Plays every 5 seconds
	
	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("Stretch");
		
		if (_animationPlayer == null)
		{
			GD.PrintErr("AnimationPlayer not found for Mutant!");
			return;
		}
		
		// List available animations for debugging
		var animations = _animationPlayer.GetAnimationList();
		GD.Print($"Available Mutant animations: {string.Join(", ", animations)}");
		
		// Start with Idle animation looping
		PlayIdleAnimation();
		
		// Setup timer for stretch animation every 5 seconds
		_stretchTimer = new Timer();
		AddChild(_stretchTimer);
		_stretchTimer.WaitTime = 5.0;
		_stretchTimer.Timeout += OnStretchTimerTimeout;
		_stretchTimer.Start();
		
		// Connect to animation finished signal to return to idle after stretch
		_animationPlayer.AnimationFinished += OnAnimationFinished;
	}
	
	private void PlayIdleAnimation()
	{
		if (_animationPlayer.HasAnimation(ANIM_IDLE))
		{
			_animationPlayer.Play(ANIM_IDLE);
			GD.Print("Playing Idle animation");
		}
	}
	
	private void OnStretchTimerTimeout()
	{
		// Play stretch animation
		if (_animationPlayer.HasAnimation(ANIM_STRETCH) && !_isStretching)
		{
			_isStretching = true;
			_animationPlayer.Play(ANIM_STRETCH);
			GD.Print("Playing Stretch animation");
		}
	}
	
	private void OnAnimationFinished(StringName animName)
	{
		// When stretch finishes, return to idle
		if (animName == ANIM_STRETCH)
		{
			_isStretching = false;
			PlayIdleAnimation();
		}
	}
	
	private AnimationPlayer FindAnimationPlayer(Node node)
	{
		if (node is AnimationPlayer player)
			return player;
			
		foreach (Node child in node.GetChildren())
		{
			var result = FindAnimationPlayer(child);
			if (result != null)
				return result;
		}
		return null;
	}
	
	public override void _ExitTree()
	{
		// Cleanup
		if (_animationPlayer != null)
		{
			_animationPlayer.AnimationFinished -= OnAnimationFinished;
		}
		
		if (_stretchTimer != null)
		{
			_stretchTimer.Timeout -= OnStretchTimerTimeout;
		}
	}
}
