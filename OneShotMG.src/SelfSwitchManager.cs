using System.Collections.Generic;
using System.Linq;
using OneShotMG.src.TWM;

namespace OneShotMG.src
{
	public class SelfSwitchManager
	{
		private readonly OneshotWindow oneshotWindow;

		private HashSet<string> selfSwitches;

		public SelfSwitchManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			selfSwitches = new HashSet<string>();
		}

		private string makeSelfSwitchId(int eventId, string switchName)
		{
			return oneshotWindow.tileMapMan.GetMapFileName() + "_" + eventId + "_" + switchName;
		}

		public void SetSelfSwitch(int eventId, string switchName)
		{
			selfSwitches.Add(makeSelfSwitchId(eventId, switchName));
		}

		public void UnsetSelfSwitch(int eventId, string switchName)
		{
			selfSwitches.Remove(makeSelfSwitchId(eventId, switchName));
		}

		public bool IsSelfSwitchSet(int eventId, string switchName)
		{
			return selfSwitches.Contains(makeSelfSwitchId(eventId, switchName));
		}

		public List<string> GetSelfSwitchesData()
		{
			return selfSwitches.ToList();
		}

		public void LoadSelfSwitchesData(List<string> loadedSwitches)
		{
			selfSwitches.Clear();
			foreach (string loadedSwitch in loadedSwitches)
			{
				selfSwitches.Add(loadedSwitch);
			}
		}
	}
}
