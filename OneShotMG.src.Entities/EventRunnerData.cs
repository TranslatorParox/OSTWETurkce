using OneShotMG.src.MessageBox;

namespace OneShotMG.src.Entities
{
	public class EventRunnerData
	{
		public EventCommand[] commands;

		public int commandIndex;

		public int waitTimer;

		public int buttonInputVarId;

		public int selectedChoice = -1;

		public int currentChoicesIndent = -1;

		public int TriggeringEntityId = -1;

		public EventRunnerData innerRunner;

		public bool waitForMapTransition;

		public TextBox.TextBoxStyle currentTextBoxStyle;

		public TextBox.TextBoxArea currentTextBoxArea = TextBox.TextBoxArea.Down;

		public int eventId;

		public int eventPageNumber;
	}
}
