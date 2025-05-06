using System.Collections.Generic;

namespace OneShotMG.src.TWM.Filesystem
{
	public class TWMFolder : TWMFileNode
	{
		public List<TWMFileNode> contents = new List<TWMFileNode>();

		public string Path => parentPath + name + "/";

		private static string getIcon(string folderName)
		{
			string result = "folder";
			switch (folderName)
			{
			case "themes_foldername":
				result = "theme";
				break;
			case "wallpapers_foldername":
				result = "wallpapers_folder";
				break;
			case "help_foldername":
				result = "help";
				break;
			}
			return result;
		}

		public TWMFolder(string folderName)
			: base(getIcon(folderName), folderName)
		{
			deleteRestricted = true;
		}
	}
}
