namespace OneShotMG.src
{
	public class SaveRequest
	{
		public readonly string fileName;

		public readonly string data;

		public bool backupsEnabled = true;

		public SaveRequest(string fName, string fData, bool backup = true)
		{
			fileName = fName;
			data = fData;
			backupsEnabled = backup;
		}
	}
}
