namespace OneShotMG.src.MessageBox
{
	public interface IMessageBox
	{
		void Update();

		void Draw();

		bool IsReadyForMoreInput();

		bool IsFinished();

		void FeedText(string text, string playerName);

		void Open();

		void ClearText();

		void InputNumberSetup(int inputNumberVar, int inputNumberDigits);
	}
}
