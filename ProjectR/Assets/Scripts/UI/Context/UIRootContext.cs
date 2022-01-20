using M4u;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIRootContext : M4uContext
{
	private M4uProperty<MainHUDContext> mainHUD = new M4uProperty<MainHUDContext>();
	private M4uProperty<CommandPanelContext> commandPanel = new M4uProperty<CommandPanelContext>();

	public MainHUDContext MainHUD { get => mainHUD.Value; set => mainHUD.Value = value; }

	public CommandPanelContext CommandPanel { get => commandPanel.Value; set => commandPanel.Value = value; }

	public UIRootContext()
	{
		MainHUD = new MainHUDContext();
		CommandPanel = new CommandPanelContext();
	}
}

public class CommonButton : M4uContext
{
	private M4uProperty<string> name = new M4uProperty<string>();
	private M4uProperty<Sprite> sprite = new M4uProperty<Sprite>();

	public string Name { get => name.Value; set => name.Value = value; }
	public Sprite Sprite { get => sprite.Value; set => sprite.Value = value; }

	public event Action onButtonClick;

	public void OnButtonClick()
	{
		onButtonClick?.Invoke();
	}
}

public class MainHUDContext : M4uContext
{
	private M4uProperty<List<CommonButton>> buttons = new M4uProperty<List<CommonButton>>(new List<CommonButton>());
	public List<CommonButton> Buttons { get => buttons.Value; set => buttons.Value = value; }

	public MainHUDContext()
	{
		var allPlanningDescs = TableManager.GetTable<StructurePlanningTable>().All();

		foreach (var planningDesc in allPlanningDescs)
		{
			CommonButton commonButton = new CommonButton();
			commonButton.Name = planningDesc.Name;
			commonButton.onButtonClick += () =>
			{
				PlanningManager.Instance.SetPlan(new StructurePlanning(planningDesc));
			};

			Buttons.Add(commonButton);
		}
	}
}

public class CommandPanelContext : M4uContext
{
	private M4uProperty<List<CommonButton>> commandButtons = new M4uProperty<List<CommonButton>>(new List<CommonButton>());
	public List<CommonButton> CommandButtons { get => commandButtons.Value; set => commandButtons.Value = value; }

	public CommandPanelContext()
	{
		var allPlanningDescs = TableManager.GetTable<StructurePlanningTable>().All();

		foreach (var planningDesc in allPlanningDescs)
		{
			CommonButton commonButton = new CommonButton();
			commonButton.Name = planningDesc.Name;
			commonButton.onButtonClick += () =>
			{
				PlanningManager.Instance.SetPlan(new StructurePlanning(planningDesc));
			};

			CommandButtons.Add(commonButton);
		}
	}

	public void SetCommands(List<Command> commands)
	{
		CommandButtons.Clear();

		foreach (var command in commands)
		{
			CommonButton commandButton = new CommonButton();
			commandButton.Name = command.name;
			commandButton.Sprite = command.sprite;
			commandButton.onButtonClick += command.action;
			CommandButtons.Add(commandButton);
		}
	}
}