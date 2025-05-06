using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Map;
using OneShotMG.src.Menus;
using OneShotMG.src.MessageBox;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class EventRunner
	{
		public enum EventCommandCode
		{
			Empty = 0,
			ShowText = 101,
			ShowChoices = 102,
			MoreText = 401,
			WhenChoice = 402,
			WhenCancel = 403,
			InputNumber = 103,
			ChangeTextOptions = 104,
			ButtonInputProcessing = 105,
			Wait = 106,
			ConditionalBranch = 111,
			Else = 411,
			BranchEnd = 412,
			BranchEndForChoices = 404,
			Loop = 112,
			RepeatAbove = 413,
			BreakLoop = 113,
			ExitEventProcessing = 115,
			EraseEvent = 116,
			CallCommonEvent = 117,
			Label = 118,
			JumpToLabel = 119,
			ControlSwitches = 121,
			ControlVariables = 122,
			ControlSelfSwitch = 123,
			ControlTimer = 124,
			ChangeGold = 125,
			ChangeItems = 126,
			ChangePartyMember = 129,
			ChangeWindowskin = 131,
			TransferPlayer = 201,
			SetEventLocation = 202,
			ScrollMap = 203,
			ChangeMapSettings = 204,
			ChangeFogColorTone = 205,
			ChangeFogOpacity = 206,
			ShowAnimation = 207,
			ChangeTransparentFlag = 208,
			SetMoveRoute = 209,
			WaitForMoveCompletion = 210,
			PrepareForTransition = 221,
			ExecuteTransition = 222,
			ChangeScreenColorTone = 223,
			ScreenFlash = 224,
			ScreenShake = 225,
			ShowPicture = 231,
			MovePicture = 232,
			RotatePicture = 233,
			ChangePictureColorTone = 234,
			ErasePicture = 235,
			SetWeatherEffects = 236,
			PlayBGM = 241,
			FadeOutBGM = 242,
			PlayBGS = 245,
			FadeOutBGS = 246,
			MemorizeBGMandBGS = 247,
			RestoreBGMandBGS = 248,
			PlayME = 249,
			PlaySE = 250,
			StopSE = 251,
			NameInputProcessing = 303,
			ChangeActorGraphic = 322,
			CallSaveScreen = 352,
			ReturnToTitleScreen = 354,
			Script = 355,
			ScriptContinuation = 655,
			Comment = 108,
			CommentContinued = 408
		}

		private readonly OneshotWindow oneshotWindow;

		private IMessageBox messageBox;

		private readonly EventCommand[] commands;

		private int commandIndex;

		private int waitTimer;

		private int buttonInputVarId;

		private int selectedChoice = -1;

		private int currentChoicesIndent = -1;

		private int triggeringEventId;

		private int triggeringEventPageNumber;

		private EventRunner innerRunner;

		private bool waitForMapTransition;

		private bool waitForModalWindow;

		private TextBox.TextBoxStyle currentTextBoxStyle;

		private TextBox.TextBoxArea currentTextBoxArea = TextBox.TextBoxArea.Down;

		private NameInputMenu nameInput;

		public bool KeepAlive = true;

		public string PasswordInput = string.Empty;

		private const int COMMON_EVENT_ELEVATOR_RUNNING_ID = 25;

		public Entity TriggeringEntity { get; private set; }

		public EventRunner(OneshotWindow osWindow, EventCommand[] list, int eventId, int eventPageNumber, Entity triggeringEntity = null)
		{
			oneshotWindow = osWindow;
			commands = list;
			TriggeringEntity = triggeringEntity;
			triggeringEventId = eventId;
			triggeringEventPageNumber = eventPageNumber;
		}

		public EventRunner(OneshotWindow osWindow, EventRunnerData savedRunner)
		{
			oneshotWindow = osWindow;
			commands = savedRunner.commands;
			commandIndex = savedRunner.commandIndex;
			waitTimer = savedRunner.waitTimer;
			buttonInputVarId = savedRunner.buttonInputVarId;
			selectedChoice = savedRunner.selectedChoice;
			currentChoicesIndent = savedRunner.currentChoicesIndent;
			TriggeringEntity = ((savedRunner.TriggeringEntityId >= 0) ? oneshotWindow.tileMapMan.GetEntityByID(savedRunner.TriggeringEntityId) : null);
			innerRunner = ((savedRunner.innerRunner != null) ? new EventRunner(oneshotWindow, savedRunner.innerRunner) : null);
			waitForMapTransition = savedRunner.waitForMapTransition;
			currentTextBoxStyle = savedRunner.currentTextBoxStyle;
			currentTextBoxArea = savedRunner.currentTextBoxArea;
			triggeringEventId = savedRunner.eventId;
			triggeringEventPageNumber = savedRunner.eventPageNumber;
		}

		public void Update()
		{
			if (waitForModalWindow)
			{
				if (oneshotWindow.IsModalWindowOpen())
				{
					return;
				}
				waitForModalWindow = false;
				EventCommand eventCommand = commands[commandIndex];
				commandIndex++;
				if (eventCommand.code == 111)
				{
					if (oneshotWindow.LastModalResponse == ModalWindow.ModalResponse.OK || oneshotWindow.LastModalResponse == ModalWindow.ModalResponse.Yes)
					{
						conditionalBranchNavigation(eventCommand, conditionResult: true);
					}
					else
					{
						conditionalBranchNavigation(eventCommand, conditionResult: false);
					}
				}
			}
			if (waitForMapTransition)
			{
				if (oneshotWindow.tileMapMan.IsChangingMap())
				{
					return;
				}
				waitForMapTransition = false;
			}
			bool flag = false;
			if (messageBox != null)
			{
				messageBox.Update();
				if (messageBox.IsFinished())
				{
					messageBox = null;
				}
				else if (!messageBox.IsReadyForMoreInput())
				{
					return;
				}
			}
			if (innerRunner != null)
			{
				innerRunner.Update();
				if (!innerRunner.IsFinished())
				{
					return;
				}
				innerRunner = null;
			}
			if (waitTimer > 0)
			{
				waitTimer--;
				return;
			}
			if (nameInput != null)
			{
				nameInput.Update();
				if (nameInput.IsOpen())
				{
					return;
				}
				nameInput = null;
			}
			if (buttonInputVarId > 0)
			{
				if (!Game1.inputMan.IsButtonPressed(InputManager.Button.OK) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Down) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Left) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Right) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Up) && !Game1.inputMan.IsAutoMashing())
				{
					return;
				}
				oneshotWindow.varMan.SetVariable(22, 1);
				buttonInputVarId = 0;
			}
			while (commandIndex < commands.Length && !flag)
			{
				EventCommand eventCommand2 = commands[commandIndex];
				commandIndex++;
				switch ((EventCommandCode)eventCommand2.code)
				{
				case EventCommandCode.ShowText:
					flag = commandShowText(eventCommand2);
					break;
				case EventCommandCode.ConditionalBranch:
					flag = commandConditionalBranch(eventCommand2);
					break;
				case EventCommandCode.Else:
					flag = commandElse(eventCommand2);
					break;
				case EventCommandCode.ControlVariables:
					flag = commandControlVariables(eventCommand2);
					break;
				case EventCommandCode.ControlSwitches:
					flag = commandControlSwitches(eventCommand2);
					break;
				case EventCommandCode.ControlSelfSwitch:
					flag = commandControlSelfSwitch(eventCommand2);
					break;
				case EventCommandCode.PlaySE:
					flag = commandPlaySE(eventCommand2);
					break;
				case EventCommandCode.Script:
					flag = commandScript(eventCommand2);
					break;
				case EventCommandCode.Wait:
					flag = commandWait(eventCommand2);
					break;
				case EventCommandCode.ButtonInputProcessing:
					flag = commandButtonInputProcessing(eventCommand2);
					break;
				case EventCommandCode.ShowPicture:
					flag = commandShowPicture(eventCommand2);
					break;
				case EventCommandCode.ErasePicture:
					flag = commandErasePicture(eventCommand2);
					break;
				case EventCommandCode.MovePicture:
					flag = commandMovePicture(eventCommand2);
					break;
				case EventCommandCode.ChangeItems:
					flag = commandChangeItems(eventCommand2);
					break;
				case EventCommandCode.PlayME:
					flag = commandPlayME(eventCommand2);
					break;
				case EventCommandCode.CallCommonEvent:
					flag = commandCallCommonEvent(eventCommand2);
					break;
				case EventCommandCode.ExitEventProcessing:
					flag = true;
					EndEvent();
					break;
				case EventCommandCode.EraseEvent:
					flag = commandEraseEvent(eventCommand2);
					break;
				case EventCommandCode.ChangeActorGraphic:
					flag = commandChangeActorGraphic(eventCommand2);
					break;
				case EventCommandCode.PlayBGM:
					flag = commandPlayBGM(eventCommand2);
					break;
				case EventCommandCode.PlayBGS:
					flag = commandPlayBGS(eventCommand2);
					break;
				case EventCommandCode.TransferPlayer:
					flag = commandTransferPlayer(eventCommand2);
					break;
				case EventCommandCode.ScrollMap:
					flag = commandScrollMap(eventCommand2);
					break;
				case EventCommandCode.Loop:
					flag = false;
					break;
				case EventCommandCode.RepeatAbove:
					flag = commandRepeatAbove(eventCommand2);
					break;
				case EventCommandCode.BreakLoop:
					flag = commandBreakLoop(eventCommand2);
					break;
				case EventCommandCode.MemorizeBGMandBGS:
					flag = commandMemorizeBGMandBGS(eventCommand2);
					break;
				case EventCommandCode.RestoreBGMandBGS:
					flag = commandRestoreBGMandBGS(eventCommand2);
					break;
				case EventCommandCode.Label:
					flag = false;
					break;
				case EventCommandCode.JumpToLabel:
					flag = commandJumpToLabel(eventCommand2);
					break;
				case EventCommandCode.ChangeMapSettings:
					flag = commandChangeMapSettings(eventCommand2);
					break;
				case EventCommandCode.ChangePartyMember:
					flag = commandChangePartyMember(eventCommand2);
					break;
				case EventCommandCode.SetMoveRoute:
					flag = commandSetMoveRoute(eventCommand2);
					break;
				case EventCommandCode.WaitForMoveCompletion:
					flag = commandWaitForMoveCompletion(eventCommand2);
					break;
				case EventCommandCode.ChangeTextOptions:
					flag = commandChangeTextOptions(eventCommand2);
					break;
				case EventCommandCode.ScreenFlash:
					flag = commandScreenFlash(eventCommand2);
					break;
				case EventCommandCode.FadeOutBGM:
					flag = commandFadeOutBGM(eventCommand2);
					break;
				case EventCommandCode.FadeOutBGS:
					flag = commandFadeOutBGS(eventCommand2);
					break;
				case EventCommandCode.ChangeScreenColorTone:
					flag = commandChangeScreenColorTone(eventCommand2);
					break;
				case EventCommandCode.ChangePictureColorTone:
					flag = commandChangePictureColorTone(eventCommand2);
					break;
				case EventCommandCode.ShowAnimation:
					flag = commandShowAnimation(eventCommand2);
					break;
				case EventCommandCode.ShowChoices:
					flag = commandShowChoices(eventCommand2);
					break;
				case EventCommandCode.WhenChoice:
					flag = commandWhenChoice(eventCommand2);
					break;
				case EventCommandCode.WhenCancel:
					flag = commandWhenCancel(eventCommand2);
					break;
				case EventCommandCode.ScreenShake:
					flag = commandScreenShake(eventCommand2);
					break;
				case EventCommandCode.SetEventLocation:
					flag = commandSetEventLocation(eventCommand2);
					break;
				case EventCommandCode.InputNumber:
					flag = commandInputNumber(eventCommand2);
					break;
				case EventCommandCode.PrepareForTransition:
					flag = commandPrepareForTransition(eventCommand2);
					break;
				case EventCommandCode.ExecuteTransition:
					flag = commandExecuteTransition(eventCommand2);
					break;
				case EventCommandCode.CallSaveScreen:
					flag = commandCallSaveScreen(eventCommand2);
					break;
				default:
				{
					LogManager logMan = Game1.logMan;
					EventCommandCode code = (EventCommandCode)eventCommand2.code;
					logMan.Log(LogManager.LogLevel.Error, "Command code " + code.ToString() + " not implemented.");
					break;
				}
				case EventCommandCode.Empty:
				case EventCommandCode.Comment:
				case EventCommandCode.ChangeWindowskin:
				case EventCommandCode.BranchEndForChoices:
				case EventCommandCode.CommentContinued:
				case EventCommandCode.BranchEnd:
				case EventCommandCode.ScriptContinuation:
					break;
				}
			}
			if (IsFinished() && TriggeringEntity != null)
			{
				TriggeringEntity.Unlock();
			}
		}

		public void checkScriptCommands()
		{
			commandIndex = 0;
			while (commandIndex < commands.Length)
			{
				EventCommand eventCommand = commands[commandIndex];
				commandIndex++;
				switch ((EventCommandCode)eventCommand.code)
				{
				case EventCommandCode.ConditionalBranch:
					if (int.Parse(eventCommand.parameters[0], CultureInfo.InvariantCulture) == 12)
					{
						ScriptParser.HandleScriptConditional(oneshotWindow, eventCommand.parameters[1], this, -1);
					}
					break;
				case EventCommandCode.Script:
					commandScript(eventCommand);
					break;
				}
				if (oneshotWindow.IsModalWindowOpen())
				{
					oneshotWindow.CloseModalWindow();
					commandIndex++;
				}
			}
		}

		public void EndEvent()
		{
			commandIndex = commands.Length;
		}

		public bool IsFinished()
		{
			return commandIndex >= commands.Length;
		}

		public void Draw()
		{
			messageBox?.Draw();
			nameInput?.Draw();
			innerRunner?.Draw();
		}

		private Entity GetEntity(int entityId)
		{
			switch (entityId)
			{
			case -1:
				return oneshotWindow.tileMapMan.GetPlayer();
			case 0:
				return TriggeringEntity;
			default:
				return oneshotWindow.tileMapMan.GetEntityByID(entityId);
			}
		}

		private int GetOperateValue(string operation, string operandType, string operand)
		{
			int num = 0;
			int num2 = int.Parse(operand, CultureInfo.InvariantCulture);
			num = ((int.Parse(operandType, CultureInfo.InvariantCulture) != 0) ? oneshotWindow.varMan.GetVariable(num2) : num2);
			if (int.Parse(operation, CultureInfo.InvariantCulture) == 1)
			{
				num = -num;
			}
			return num;
		}

		private bool commandInputNumber(EventCommand command)
		{
			int inputNumberVar = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int inputNumberDigits = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			if (messageBox == null || (messageBox is TextBox && (((TextBox)messageBox).CurrentStyle != currentTextBoxStyle || ((TextBox)messageBox).CurrentArea != currentTextBoxArea)))
			{
				messageBox = new TextBox(oneshotWindow, currentTextBoxStyle, currentTextBoxArea);
			}
			else
			{
				messageBox.Open();
				messageBox.ClearText();
			}
			messageBox.InputNumberSetup(inputNumberVar, inputNumberDigits);
			return true;
		}

		private bool commandShowText(EventCommand command)
		{
			string text = command.parameters[0];
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			text = Game1.languageMan.GetMapLocString(triggeringEventPageNumber, text);
			if (text.ToLowerInvariant().StartsWith("@ed "))
			{
				text = text.Substring("@ed ".Length);
				messageBox = new EdBox(oneshotWindow);
			}
			else if (text.ToLowerInvariant().StartsWith("@desktop "))
			{
				text = text.Substring("@desktop ".Length);
				messageBox = new DesktopBox();
			}
			else if (text.ToLowerInvariant().StartsWith("@credits "))
			{
				text = text.Substring("@credits ".Length);
				messageBox = new CreditsBox(oneshotWindow);
			}
			else if (text.ToLowerInvariant().StartsWith("@credits_small "))
			{
				text = text.Substring("@credits_small ".Length);
				messageBox = new CreditsBox(oneshotWindow, small: true);
			}
			else if (text.StartsWith("@tut ") || text.StartsWith("@tut2 "))
			{
				bool flag = text.StartsWith("@tut2 ");
				text = (flag ? text.Substring("@tut2 ".Length) : text.Substring("@tut ".Length));
				while (commandIndex < commands.Length && ((commands[commandIndex].code == 101 && commands[commandIndex].parameters[0].StartsWith("@tut ")) || commands[commandIndex].code == 401))
				{
					string originalString = commands[commandIndex].parameters[0];
					originalString = Game1.languageMan.GetMapLocString(triggeringEventPageNumber, originalString);
					if (commands[commandIndex].code == 101)
					{
						originalString = (flag ? originalString.Substring("@tut2 ".Length) : originalString.Substring("@tut ".Length));
					}
					text += originalString;
					commandIndex++;
				}
				messageBox = new TutorialBox(oneshotWindow, flag);
			}
			else if (text.StartsWith("$"))
			{
				text = text.Substring(1);
				while (commandIndex < commands.Length && commands[commandIndex].code == 101 && commands[commandIndex].parameters[0].StartsWith("$"))
				{
					string originalString2 = commands[commandIndex].parameters[0];
					originalString2 = Game1.languageMan.GetMapLocString(triggeringEventPageNumber, originalString2);
					text = text + "\n\n" + originalString2.Substring(1);
					commandIndex++;
				}
				messageBox = new DocBox(oneshotWindow);
			}
			else
			{
				bool flag2 = false;
				bool showChoices = false;
				int inputNumberDigits = -1;
				int inputNumberVar = -1;
				int num = commandIndex;
				while (num < commands.Length)
				{
					EventCommand eventCommand = commands[num];
					num++;
					if (eventCommand.code == 401)
					{
						string originalString3 = eventCommand.parameters[0];
						originalString3 = Game1.languageMan.GetMapLocString(triggeringEventPageNumber, originalString3);
						text = text + " " + originalString3;
						commandIndex = num;
						continue;
					}
					if (eventCommand.code == 103)
					{
						flag2 = true;
						inputNumberVar = int.Parse(eventCommand.parameters[0], CultureInfo.InvariantCulture);
						inputNumberDigits = int.Parse(eventCommand.parameters[1], CultureInfo.InvariantCulture);
						commandIndex = num;
						continue;
					}
					if (eventCommand.code == 102 && JsonConvert.DeserializeObject<List<string>>(eventCommand.parameters[0]).Count < 4)
					{
						showChoices = true;
					}
					break;
				}
				if (messageBox == null || (messageBox is TextBox && (((TextBox)messageBox).CurrentStyle != currentTextBoxStyle || ((TextBox)messageBox).CurrentArea != currentTextBoxArea)))
				{
					messageBox = new TextBox(oneshotWindow, currentTextBoxStyle, currentTextBoxArea);
				}
				else
				{
					messageBox.Open();
					messageBox.ClearText();
				}
				((TextBox)messageBox).ShowChoicesIsNextCommand(showChoices);
				if (flag2)
				{
					messageBox.InputNumberSetup(inputNumberVar, inputNumberDigits);
				}
			}
			messageBox.FeedText(text, oneshotWindow.gameSaveMan.GetPlayerName());
			return true;
		}

		public void OpenNameInput()
		{
			PasswordInput = string.Empty;
			nameInput = new NameInputMenu(this, oneshotWindow);
			nameInput.Open();
		}

		private bool commandShowChoices(EventCommand command)
		{
			string originalString = command.parameters[0];
			originalString = Game1.languageMan.GetMapLocString(triggeringEventPageNumber, originalString);
			List<string> list = JsonConvert.DeserializeObject<List<string>>(originalString);
			if (messageBox == null || !(messageBox is TextBox) || (messageBox is TextBox && (((TextBox)messageBox).CurrentStyle != currentTextBoxStyle || ((TextBox)messageBox).CurrentArea != currentTextBoxArea)))
			{
				messageBox = new TextBox(oneshotWindow, currentTextBoxStyle, currentTextBoxArea);
			}
			else
			{
				messageBox.Open();
				if (list.Count >= 4 || commandIndex < 2 || commands[commandIndex - 2].code != 101 || string.IsNullOrEmpty(commands[commandIndex - 2].parameters[0]))
				{
					messageBox.ClearText();
				}
			}
			int cancelValue = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			currentChoicesIndent = command.indent;
			((TextBox)messageBox).SetChoices(list, cancelValue, delegate(int choice)
			{
				selectedChoice = choice;
			}, oneshotWindow.gameSaveMan.GetPlayerName());
			return true;
		}

		private bool commandWhenChoice(EventCommand command)
		{
			if (int.Parse(command.parameters[0], CultureInfo.InvariantCulture) == selectedChoice && command.indent == currentChoicesIndent)
			{
				selectedChoice = -1;
				return false;
			}
			while (commandIndex < commands.Length && commands[commandIndex].indent != command.indent)
			{
				commandIndex++;
			}
			return false;
		}

		private bool commandWhenCancel(EventCommand command)
		{
			if (selectedChoice == 4 && command.indent == currentChoicesIndent)
			{
				selectedChoice = -1;
				return false;
			}
			while (commandIndex < commands.Length && commands[commandIndex].indent != command.indent)
			{
				commandIndex++;
			}
			return false;
		}

		private bool commandConditionalBranch(EventCommand command)
		{
			bool conditionResult = false;
			int num = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			switch (num)
			{
			case 0:
			{
				int flagIndex = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
				bool flag = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) == 0;
				conditionResult = oneshotWindow.flagMan.IsFlagSet(flagIndex) == flag;
				break;
			}
			case 1:
			{
				int variable = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[1], CultureInfo.InvariantCulture));
				bool num2 = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) != 0;
				int num3 = 0;
				num3 = ((!num2) ? int.Parse(command.parameters[3], CultureInfo.InvariantCulture) : oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[3], CultureInfo.InvariantCulture)));
				switch (int.Parse(command.parameters[4], CultureInfo.InvariantCulture))
				{
				case 0:
					conditionResult = variable == num3;
					break;
				case 1:
					conditionResult = variable >= num3;
					break;
				case 2:
					conditionResult = variable <= num3;
					break;
				case 3:
					conditionResult = variable > num3;
					break;
				case 4:
					conditionResult = variable < num3;
					break;
				case 5:
					conditionResult = variable != num3;
					break;
				}
				break;
			}
			case 2:
				conditionResult = false;
				if (TriggeringEntity != null)
				{
					conditionResult = oneshotWindow.selfSwitchMan.IsSelfSwitchSet(TriggeringEntity.GetID(), command.parameters[1]) == (int.Parse(command.parameters[2], CultureInfo.InvariantCulture) == 0);
				}
				break;
			case 4:
			{
				int actor = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
				if (int.Parse(command.parameters[2], CultureInfo.InvariantCulture) == 0)
				{
					conditionResult = oneshotWindow.followerMan.IsFollowerInParty((FollowerManager.FollowerType)actor);
				}
				break;
			}
			case 6:
			{
				int entityId = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
				Entity.Direction direction = (Entity.Direction)int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
				Entity entity = GetEntity(entityId);
				if (entity != null)
				{
					conditionResult = entity.GetDirection() == direction;
				}
				break;
			}
			case 8:
				conditionResult = oneshotWindow.menuMan.ItemMan.HasItem(int.Parse(command.parameters[1], CultureInfo.InvariantCulture));
				break;
			case 11:
			{
				InputManager.Button b = (InputManager.Button)int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
				conditionResult = Game1.inputMan.IsButtonPressed(b);
				break;
			}
			case 12:
			{
				string script = command.parameters[1];
				switch (ScriptParser.HandleScriptConditional(oneshotWindow, script, this, triggeringEventPageNumber))
				{
				case ScriptParser.ScriptConditionalResult.Yes:
					conditionResult = true;
					break;
				case ScriptParser.ScriptConditionalResult.No:
					conditionResult = false;
					break;
				case ScriptParser.ScriptConditionalResult.EdText:
					waitForModalWindow = true;
					commandIndex--;
					return true;
				}
				break;
			}
			default:
				Game1.logMan.Log(LogManager.LogLevel.Error, $"unimplemented condition type {num} in conditional branch!");
				break;
			}
			conditionalBranchNavigation(command, conditionResult);
			return false;
		}

		private void conditionalBranchNavigation(EventCommand command, bool conditionResult)
		{
			if (!conditionResult)
			{
				while (commandIndex < commands.Length && commands[commandIndex].indent != command.indent)
				{
					commandIndex++;
				}
				if (commands[commandIndex].code == 411)
				{
					commandIndex++;
				}
			}
		}

		private bool commandElse(EventCommand command)
		{
			while (commandIndex < commands.Length && commands[commandIndex].indent != command.indent)
			{
				commandIndex++;
			}
			return false;
		}

		private bool commandControlVariables(EventCommand command)
		{
			int num = 0;
			int num2 = int.Parse(command.parameters[3], CultureInfo.InvariantCulture);
			switch (num2)
			{
			case 0:
				num = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
				break;
			case 1:
				num = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[4], CultureInfo.InvariantCulture));
				break;
			case 2:
				num = MathHelper.Random(int.Parse(command.parameters[4]), int.Parse(command.parameters[5], CultureInfo.InvariantCulture));
				break;
			case 3:
				num = (oneshotWindow.menuMan.ItemMan.HasItem(int.Parse(command.parameters[4], CultureInfo.InvariantCulture)) ? 1 : 0);
				break;
			case 6:
			{
				Entity entity = GetEntity(int.Parse(command.parameters[4], CultureInfo.InvariantCulture));
				if (entity != null)
				{
					int num4 = int.Parse(command.parameters[5], CultureInfo.InvariantCulture);
					switch (num4)
					{
					case 0:
						num = entity.GetCurrentTile().X;
						break;
					case 1:
						num = entity.GetCurrentTile().Y;
						break;
					case 2:
						num = (int)entity.GetDirection();
						break;
					default:
						Game1.logMan.Log(LogManager.LogLevel.Error, $"Unrecognized entityValueSource {num4}");
						break;
					}
				}
				break;
			}
			case 7:
			{
				int num3 = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
				if (num3 == 0)
				{
					num = oneshotWindow.tileMapMan.GetMapID();
				}
				else
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, $"Unrecognized otherValueSource {num3}");
				}
				break;
			}
			default:
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Unimplemented variable source value {num2}");
				break;
			}
			int num5 = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int num6 = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			int num7 = int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
			for (int i = num5; i <= num6; i++)
			{
				int num8 = oneshotWindow.varMan.GetVariable(i);
				switch (num7)
				{
				case 0:
					num8 = num;
					break;
				case 1:
					num8 += num;
					break;
				case 2:
					num8 -= num;
					break;
				case 3:
					num8 *= num;
					break;
				case 4:
					if (num != 0)
					{
						num8 /= num;
					}
					break;
				case 5:
					if (num != 0)
					{
						num8 %= num;
					}
					break;
				}
				if (num8 > 99999999)
				{
					num8 = 99999999;
				}
				else if (num8 < -99999999)
				{
					num8 = -99999999;
				}
				oneshotWindow.varMan.SetVariable(i, num8);
			}
			return false;
		}

		private bool commandControlSwitches(EventCommand command)
		{
			int num = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int num2 = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			bool flag = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) == 0;
			for (int i = num; i <= num2; i++)
			{
				if (flag)
				{
					oneshotWindow.flagMan.SetFlag(i);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(i);
				}
			}
			Game1.windowMan.Desktop.inSolstice = oneshotWindow.flagMan.IsSolticeGlitchTime();
			return false;
		}

		private bool commandControlSelfSwitch(EventCommand command)
		{
			if (TriggeringEntity != null)
			{
				if (int.Parse(command.parameters[1], CultureInfo.InvariantCulture) == 0)
				{
					oneshotWindow.selfSwitchMan.SetSelfSwitch(TriggeringEntity.GetID(), command.parameters[0]);
				}
				else
				{
					oneshotWindow.selfSwitchMan.UnsetSelfSwitch(TriggeringEntity.GetID(), command.parameters[0]);
				}
			}
			return false;
		}

		private bool commandPlaySE(EventCommand command)
		{
			Game1.soundMan.PlaySound(command.audio_file.name, (float)command.audio_file.volume / 100f, (float)command.audio_file.pitch / 100f);
			return false;
		}

		private bool commandScript(EventCommand command)
		{
			string text = command.parameters[0];
			while (commandIndex < commands.Length && commands[commandIndex].code == 655)
			{
				string text2 = commands[commandIndex].parameters[0];
				text = text + "\n" + text2;
				commandIndex++;
			}
			bool result = ScriptParser.HandleScript(oneshotWindow, text, this, triggeringEventPageNumber);
			if (oneshotWindow.IsModalWindowOpen())
			{
				commandIndex--;
				waitForModalWindow = true;
				return true;
			}
			return result;
		}

		private bool commandWait(EventCommand command)
		{
			waitTimer = int.Parse(command.parameters[0], CultureInfo.InvariantCulture) * 2;
			return true;
		}

		private bool commandButtonInputProcessing(EventCommand command)
		{
			buttonInputVarId = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			waitTimer = 1;
			return true;
		}

		private bool commandShowPicture(EventCommand command)
		{
			int pictureSlot = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			bool num = int.Parse(command.parameters[3], CultureInfo.InvariantCulture) != 0;
			Vec2 zero = Vec2.Zero;
			if (num)
			{
				zero.X = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[4], CultureInfo.InvariantCulture));
				zero.Y = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[5], CultureInfo.InvariantCulture));
			}
			else
			{
				zero.X = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
				zero.Y = int.Parse(command.parameters[5], CultureInfo.InvariantCulture);
			}
			string pictureName = command.parameters[1];
			bool isPosCenter = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) != 0;
			float xScale = float.Parse(command.parameters[6], CultureInfo.InvariantCulture) / 100f;
			float yScale = float.Parse(command.parameters[7], CultureInfo.InvariantCulture) / 100f;
			int opacity = int.Parse(command.parameters[8], CultureInfo.InvariantCulture);
			GraphicsManager.BlendMode blendMode = ((int.Parse(command.parameters[9], CultureInfo.InvariantCulture) != 1) ? GraphicsManager.BlendMode.Normal : GraphicsManager.BlendMode.Additive);
			oneshotWindow.pictureMan.ShowPicture(pictureSlot, pictureName, isPosCenter, zero, xScale, yScale, opacity, blendMode);
			return false;
		}

		private bool commandErasePicture(EventCommand command)
		{
			int pictureSlot = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			oneshotWindow.pictureMan.ErasePicture(pictureSlot);
			return false;
		}

		private bool commandMovePicture(EventCommand command)
		{
			int pictureSlot = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			bool num = int.Parse(command.parameters[3], CultureInfo.InvariantCulture) != 0;
			Vec2 zero = Vec2.Zero;
			if (num)
			{
				zero.X = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[4], CultureInfo.InvariantCulture));
				zero.Y = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[5], CultureInfo.InvariantCulture));
			}
			else
			{
				zero.X = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
				zero.Y = int.Parse(command.parameters[5], CultureInfo.InvariantCulture);
			}
			int duration = int.Parse(command.parameters[1], CultureInfo.InvariantCulture) * 2;
			bool isPosCenter = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) != 0;
			float xScale = float.Parse(command.parameters[6], CultureInfo.InvariantCulture) / 100f;
			float yScale = float.Parse(command.parameters[7], CultureInfo.InvariantCulture) / 100f;
			int opacity = int.Parse(command.parameters[8], CultureInfo.InvariantCulture);
			GraphicsManager.BlendMode blendMode = ((int.Parse(command.parameters[9], CultureInfo.InvariantCulture) != 1) ? GraphicsManager.BlendMode.Normal : GraphicsManager.BlendMode.Additive);
			oneshotWindow.pictureMan.MovePicture(pictureSlot, duration, isPosCenter, zero, xScale, yScale, opacity, blendMode);
			return false;
		}

		private bool commandChangeItems(EventCommand command)
		{
			int operateValue = GetOperateValue(command.parameters[1], command.parameters[2], command.parameters[3]);
			if (operateValue > 0)
			{
				oneshotWindow.menuMan.ItemMan.AddItem(int.Parse(command.parameters[0], CultureInfo.InvariantCulture));
			}
			else if (operateValue < 0)
			{
				oneshotWindow.menuMan.ItemMan.RemoveItem(int.Parse(command.parameters[0], CultureInfo.InvariantCulture));
			}
			return false;
		}

		private bool commandPlayME(EventCommand command)
		{
			float volume = (float)command.audio_file.volume / 100f;
			float pitch = (float)command.audio_file.pitch / 100f;
			Game1.soundMan.PlayMusicEffect(command.audio_file.name, volume, pitch);
			return false;
		}

		private bool commandCallCommonEvent(EventCommand command)
		{
			int commonEventId = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			CommonEvent commonEvent = oneshotWindow.tileMapMan.GetCommonEvent(commonEventId);
			if (commonEvent != null)
			{
				innerRunner = new EventRunner(oneshotWindow, commonEvent.list, commonEvent.id, -1);
				innerRunner.Update();
				if (!innerRunner.IsFinished())
				{
					return true;
				}
				innerRunner = null;
			}
			return false;
		}

		private bool commandEraseEvent(EventCommand command)
		{
			if (TriggeringEntity != null)
			{
				TriggeringEntity.KillEntityAfterUpdate = true;
				TriggeringEntity = null;
			}
			return false;
		}

		private bool commandChangeActorGraphic(EventCommand command)
		{
			int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			oneshotWindow.tileMapMan.GetPlayer().SetNPCSheet(command.parameters[1], int.Parse(command.parameters[2], CultureInfo.InvariantCulture));
			return false;
		}

		private bool commandPlayBGM(EventCommand command)
		{
			if (TriggeringEntity != null && TriggeringEntity.GetName() == "music")
			{
				Game1.soundMan.QueueSong(command.audio_file.name, 0f, (float)command.audio_file.volume / 100f * 0.8f, (float)command.audio_file.pitch / 100f);
			}
			else
			{
				Game1.soundMan.PlaySong(command.audio_file.name, 0f, (float)command.audio_file.volume / 100f * 0.8f, (float)command.audio_file.pitch / 100f);
			}
			return false;
		}

		private bool commandPlayBGS(EventCommand command)
		{
			Game1.soundMan.PlayBGS(command.audio_file.name, (float)command.audio_file.volume / 100f, (float)command.audio_file.pitch / 100f);
			return false;
		}

		private bool commandTransferPlayer(EventCommand command)
		{
			bool num = int.Parse(command.parameters[0], CultureInfo.InvariantCulture) != 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			if (num)
			{
				num2 = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[1], CultureInfo.InvariantCulture));
				num3 = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[2], CultureInfo.InvariantCulture));
				num4 = oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[3], CultureInfo.InvariantCulture));
				num5 = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
			}
			else
			{
				num2 = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
				num3 = int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
				num4 = int.Parse(command.parameters[3], CultureInfo.InvariantCulture);
				num5 = int.Parse(command.parameters[4], CultureInfo.InvariantCulture);
			}
			bool flag = int.Parse(command.parameters[5], CultureInfo.InvariantCulture) == 0;
			float time = (flag ? 0.5f : 0f);
			oneshotWindow.tileMapMan.ChangeMap(num2, num3, num4, time, (Entity.Direction)num5);
			waitForMapTransition = flag;
			return flag;
		}

		private bool commandScrollMap(EventCommand command)
		{
			if (oneshotWindow.tileMapMan.IsMapScrolling())
			{
				commandIndex--;
				return true;
			}
			int direction = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int distance = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			int speed = int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
			oneshotWindow.tileMapMan.ScrollMap(direction, distance, speed);
			return false;
		}

		private bool commandRepeatAbove(EventCommand command)
		{
			commandIndex--;
			do
			{
				commandIndex--;
			}
			while (commandIndex >= 0 && (commands[commandIndex].indent != command.indent || commands[commandIndex].code != 112));
			return false;
		}

		private bool commandBreakLoop(EventCommand command)
		{
			int i;
			for (i = commandIndex; i < commands.Length && (commands[i].indent >= command.indent || commands[i].code != 413); i++)
			{
			}
			if (i + 1 < commands.Length)
			{
				commandIndex = i + 1;
			}
			return false;
		}

		private bool commandMemorizeBGMandBGS(EventCommand command)
		{
			Game1.soundMan.MemorizeBGMandBGS();
			return false;
		}

		private bool commandRestoreBGMandBGS(EventCommand command)
		{
			Game1.soundMan.RestoreBGMandBGS();
			return false;
		}

		private bool commandJumpToLabel(EventCommand command)
		{
			int i = 0;
			for (string text = command.parameters[0]; i < commands.Length && (commands[i].code != 118 || text != commands[i].parameters[0]); i++)
			{
			}
			if (i < commands.Length)
			{
				commandIndex = i;
			}
			return false;
		}

		private bool commandChangeMapSettings(EventCommand command)
		{
			switch (int.Parse(command.parameters[0], CultureInfo.InvariantCulture))
			{
			case 0:
			{
				string panorama = command.parameters[1];
				int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
				oneshotWindow.tileMapMan.SetPanorama(panorama);
				break;
			}
			case 1:
			{
				string fogName = command.parameters[1];
				int fogHue = int.Parse(command.parameters[2], CultureInfo.InvariantCulture);
				float fogOpacity = float.Parse(command.parameters[3], CultureInfo.InvariantCulture) / 255f;
				GraphicsManager.BlendMode fogBlendMode = ((int.Parse(command.parameters[4], CultureInfo.InvariantCulture) == 0) ? GraphicsManager.BlendMode.Normal : GraphicsManager.BlendMode.Additive);
				int fogScrollX = int.Parse(command.parameters[6], CultureInfo.InvariantCulture);
				int fogScrollY = int.Parse(command.parameters[7], CultureInfo.InvariantCulture);
				oneshotWindow.tileMapMan.SetFog(fogName, fogHue, fogOpacity, fogBlendMode, fogScrollX, fogScrollY);
				break;
			}
			case 2:
				Game1.logMan.Log(LogManager.LogLevel.Error, "Battleback change type not supported in ChangeMapSettings command");
				break;
			}
			return false;
		}

		private bool commandChangePartyMember(EventCommand command)
		{
			int num = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			bool flag = int.Parse(command.parameters[1], CultureInfo.InvariantCulture) == 0;
			switch ((FollowerManager.FollowerType)num)
			{
			case FollowerManager.FollowerType.Niko:
				if (flag)
				{
					oneshotWindow.tileMapMan.GetPlayer().SetNPCSheet("niko");
				}
				return false;
			case FollowerManager.FollowerType.Alula:
			case FollowerManager.FollowerType.Rue:
			case FollowerManager.FollowerType.Plight:
			case FollowerManager.FollowerType.Cedric:
			case FollowerManager.FollowerType.Proto:
				if (flag)
				{
					oneshotWindow.followerMan.AddFollower((FollowerManager.FollowerType)num);
				}
				else
				{
					oneshotWindow.followerMan.RemoveFollower((FollowerManager.FollowerType)num);
				}
				break;
			default:
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Unrecognized actorId {num}");
				break;
			}
			return false;
		}

		private bool commandSetMoveRoute(EventCommand command)
		{
			int entityId = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			GetEntity(entityId)?.ForceMoveRoute(command.move_route);
			return false;
		}

		private bool commandWaitForMoveCompletion(EventCommand command)
		{
			if (oneshotWindow.tileMapMan.AreAnyEntitiesInForcedRoutes())
			{
				commandIndex--;
				return true;
			}
			return false;
		}

		private bool commandChangeTextOptions(EventCommand command)
		{
			currentTextBoxArea = (TextBox.TextBoxArea)int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			if (int.Parse(command.parameters[1], CultureInfo.InvariantCulture) == 0)
			{
				currentTextBoxStyle = TextBox.TextBoxStyle.GoldBorder;
			}
			else
			{
				currentTextBoxStyle = TextBox.TextBoxStyle.NoBorder;
			}
			return false;
		}

		private bool commandScreenFlash(EventCommand command)
		{
			string[] array = command.parameters[0].Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
			float val = float.Parse(array[0], CultureInfo.InvariantCulture);
			float val2 = float.Parse(array[1], CultureInfo.InvariantCulture);
			float val3 = float.Parse(array[2], CultureInfo.InvariantCulture);
			float val4 = float.Parse(array[3], CultureInfo.InvariantCulture);
			val = Math.Min(Math.Max(val, 0f), 255f);
			val2 = Math.Min(Math.Max(val2, 0f), 255f);
			val3 = Math.Min(Math.Max(val3, 0f), 255f);
			val4 = Math.Min(Math.Max(val4, 0f), 255f);
			GameColor color = new GameColor((byte)val, (byte)val2, (byte)val3, (byte)val4);
			int time = int.Parse(command.parameters[1], CultureInfo.InvariantCulture) * 2;
			oneshotWindow.flashMan.StartFlash(color, time);
			return false;
		}

		private bool commandFadeOutBGM(EventCommand command)
		{
			float fadeOutTime = float.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			Game1.soundMan.FadeOutBGM(fadeOutTime);
			return false;
		}

		private bool commandFadeOutBGS(EventCommand command)
		{
			float durationInSeconds = float.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			Game1.soundMan.FadeOutBGS(durationInSeconds);
			return false;
		}

		private bool commandChangeScreenColorTone(EventCommand command)
		{
			string[] array = command.parameters[0].Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
			float val = float.Parse(array[0], CultureInfo.InvariantCulture);
			float val2 = float.Parse(array[1], CultureInfo.InvariantCulture);
			float val3 = float.Parse(array[2], CultureInfo.InvariantCulture);
			val = Math.Min(Math.Max(val, -255f), 255f);
			val2 = Math.Min(Math.Max(val2, -255f), 255f);
			val3 = Math.Min(Math.Max(val3, -255f), 255f);
			GameTone newTone = new GameTone((short)val, (short)val2, (short)val3);
			int shiftTime = int.Parse(command.parameters[1], CultureInfo.InvariantCulture) * 2;
			oneshotWindow.tileMapMan.SetScreenTone(newTone, shiftTime);
			return false;
		}

		private bool commandChangePictureColorTone(EventCommand command)
		{
			int pictureSlot = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			string[] array = command.parameters[1].Replace("(", string.Empty).Replace(")", string.Empty).Split(',');
			float val = float.Parse(array[0], CultureInfo.InvariantCulture);
			float val2 = float.Parse(array[1], CultureInfo.InvariantCulture);
			float val3 = float.Parse(array[2], CultureInfo.InvariantCulture);
			val = Math.Min(Math.Max(val, -255f), 255f);
			val2 = Math.Min(Math.Max(val2, -255f), 255f);
			val3 = Math.Min(Math.Max(val3, -255f), 255f);
			GameTone tone = new GameTone((short)val, (short)val2, (short)val3);
			int duration = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) * 2;
			oneshotWindow.pictureMan.ChangePictureTone(pictureSlot, tone, duration);
			return false;
		}

		private bool commandShowAnimation(EventCommand command)
		{
			int entityId = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int bType = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			Entity entity = GetEntity(entityId);
			if (entity == null)
			{
				return false;
			}
			oneshotWindow.tileMapMan.EntityAddEntity(new Moodbubble(oneshotWindow, (Moodbubble.BubbleType)bType, entity));
			return false;
		}

		private bool commandScreenShake(EventCommand command)
		{
			int power = int.Parse(command.parameters[0], CultureInfo.InvariantCulture);
			int speed = int.Parse(command.parameters[1], CultureInfo.InvariantCulture);
			int duration = int.Parse(command.parameters[2], CultureInfo.InvariantCulture) * 2;
			bool flag = (triggeringEventId == 25 && triggeringEventPageNumber < 0) || (TriggeringEntity != null && TriggeringEntity.GetName() == "elevator shake");
			oneshotWindow.tileMapMan.ScreenShake(power, speed, duration, !flag);
			return false;
		}

		private bool commandSetEventLocation(EventCommand command)
		{
			Entity entity = GetEntity(int.Parse(command.parameters[0], CultureInfo.InvariantCulture));
			if (entity == null)
			{
				return false;
			}
			switch (int.Parse(command.parameters[1], CultureInfo.InvariantCulture))
			{
			case 0:
			{
				Vec2 posTile = new Vec2(int.Parse(command.parameters[2], CultureInfo.InvariantCulture), int.Parse(command.parameters[3], CultureInfo.InvariantCulture));
				entity.SetPosTile(posTile);
				break;
			}
			case 1:
			{
				Vec2 posTile2 = new Vec2(oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[2], CultureInfo.InvariantCulture)), oneshotWindow.varMan.GetVariable(int.Parse(command.parameters[3], CultureInfo.InvariantCulture)));
				entity.SetPosTile(posTile2);
				break;
			}
			default:
			{
				Entity entity2 = GetEntity(int.Parse(command.parameters[2], CultureInfo.InvariantCulture));
				if (entity2 != null)
				{
					Vec2 currentTile = entity.GetCurrentTile();
					entity.SetPosTile(entity2.GetCurrentTile());
					entity2.SetPosTile(currentTile);
				}
				break;
			}
			}
			entity.SetDirection((Entity.Direction)int.Parse(command.parameters[4], CultureInfo.InvariantCulture));
			return false;
		}

		private bool commandPrepareForTransition(EventCommand command)
		{
			return false;
		}

		private bool commandExecuteTransition(EventCommand command)
		{
			oneshotWindow.tileMapMan.ExecuteMapTransition(command.parameters[0]);
			waitForMapTransition = true;
			return true;
		}

		private bool commandCallSaveScreen(EventCommand command)
		{
			oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.DebugSaveMenu);
			oneshotWindow.menuMan.DebugSaveMenu.LoadSaves = false;
			return false;
		}

		public EventRunnerData GetEventRunnerData()
		{
			return new EventRunnerData
			{
				commands = commands,
				commandIndex = commandIndex,
				waitTimer = waitTimer,
				buttonInputVarId = buttonInputVarId,
				selectedChoice = selectedChoice,
				currentChoicesIndent = currentChoicesIndent,
				TriggeringEntityId = ((TriggeringEntity != null) ? TriggeringEntity.GetID() : (-1)),
				innerRunner = ((innerRunner != null) ? innerRunner.GetEventRunnerData() : null),
				waitForMapTransition = waitForMapTransition,
				currentTextBoxArea = currentTextBoxArea,
				currentTextBoxStyle = currentTextBoxStyle,
				eventId = triggeringEventId,
				eventPageNumber = triggeringEventPageNumber
			};
		}
	}
}
