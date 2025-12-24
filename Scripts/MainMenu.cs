using Godot;
using System;

public partial class UIManager : Node
{
	public static UIManager Instance { get; private set; }
	
	private Control mainMenu;
	private Control pauseMenu;
	private Control storyPanel;
	private Control analyticsPanel;
	
	public override void _Ready()
	{
		Instance = this;
		
		// Get references to UI panels
		mainMenu = GetNode<Control>("MainMenu");
		// pauseMenu = GetNode<Control>("PauseMenu");
		// storyPanel = GetNode<Control>("StoryPanel");
		// analyticsPanel = GetNode<Control>("AnalyticsPanel");
		
		// Hide all panels initially
		HideAllPanels();
	}
	
	public void ShowMainMenu()
	{
		HideAllPanels();
		mainMenu.Visible = true;
		GetTree().Paused = false;
	}
	
	// public void ShowPauseMenu()
	// {
	// 	pauseMenu.Visible = true;
	// 	GetTree().Paused = true;
	// }
	
	// public void HidePauseMenu()
	// {
	// 	pauseMenu.Visible = false;
	// 	GetTree().Paused = false;
	// }
	
	// public void TogglePause()
	// {
	// 	if (pauseMenu.Visible)
	// 		HidePauseMenu();
	// 	else
	// 		ShowPauseMenu();
	// }
	
	private void HideAllPanels()
	{
		mainMenu.Visible = false;
		// pauseMenu.Visible = false;
		// storyPanel.Visible = false;
		// analyticsPanel.Visible = false;
	}
}
