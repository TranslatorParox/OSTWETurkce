namespace OneShotMG.src
{
	public class VariableManager
	{
		public const int GEORGE_VAR = 47;

		public const int MAP_EVENT_VAR = 4;

		private int[] variables;

		public readonly int TotalVariables = 125;

		public const int CLOCK_SECONDS_2ND_DIGIT = 101;

		public const int CLOCK_SECONDS_1ST_DIGIT = 102;

		public const int CLOCK_MINUTES_2ND_DIGIT = 103;

		public const int CLOCK_MINUTES_1ST_DIGIT = 104;

		public const int CLOCK_HOURS_2ND_DIGIT = 105;

		public const int CLOCK_HOURS_1ST_DIGIT = 106;

		public const int CLOCK_DAYS_3RD_DIGIT = 107;

		public const int CLOCK_DAYS_2ND_DIGIT = 108;

		public const int CLOCK_DAYS_1ST_DIGIT = 109;

		public VariableManager(bool permaVars = false)
		{
			if (permaVars)
			{
				TotalVariables = 25;
			}
			variables = new int[TotalVariables];
			for (int i = 0; i < TotalVariables; i++)
			{
				variables[i] = 0;
			}
		}

		public int GetVariable(int varIndex)
		{
			if (varIndex < 0 || varIndex >= TotalVariables)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"tried to get variable '{varIndex}' but it's out of range!");
				return 0;
			}
			return variables[varIndex];
		}

		public bool SetVariable(int varIndex, int newVal)
		{
			if (varIndex < 0 || varIndex >= TotalVariables)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"tried to get variable '{varIndex}' but it's out of range!");
				return false;
			}
			variables[varIndex] = newVal;
			return true;
		}

		public int[] GetRawVarData()
		{
			return variables;
		}

		public void SetRawVarData(int[] savedVars)
		{
			variables = savedVars;
		}
	}
}
