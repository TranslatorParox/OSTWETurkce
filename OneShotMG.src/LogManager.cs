using System;
using System.IO;
using System.Windows.Forms;

namespace OneShotMG.src
{
	public class LogManager
	{
		public enum LogLevel
		{
			Info,
			Warning,
			Error,
			StackDump
		}

		public bool Verbose = true;

		private StreamWriter logFile;

		public LogManager()
		{
			logFile = new StreamWriter("log.txt");
		}

		public void Log(LogLevel level, string message)
		{
			switch (level)
			{
			case LogLevel.Info:
				if (Verbose)
				{
					string value2 = "Info: " + message;
					logFile.WriteLine(value2);
					Console.WriteLine(value2);
				}
				break;
			case LogLevel.Warning:
			{
				string value = "Warning: " + message;
				logFile.WriteLine(value);
				Console.WriteLine(value);
				break;
			}
			case LogLevel.Error:
			{
				string text = "Error: " + message;
				logFile.WriteLine(text);
				Console.WriteLine(text);
				System.Windows.Forms.MessageBox.Show(text);
				break;
			}
			case LogLevel.StackDump:
				logFile.WriteLine(message);
				Console.WriteLine(message);
				break;
			}
		}

		public void Dispose()
		{
			logFile.Close();
		}
	}
}
