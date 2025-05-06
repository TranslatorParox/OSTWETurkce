using System.Collections.Generic;
using OneShotMG.src.Entities;
using OneShotMG.src.TWM;

namespace OneShotMG.src.Map
{
	public class FollowerManager
	{
		public enum FollowerType
		{
			Niko = 1,
			Alula = 2,
			Rue = 3,
			Plight = 6,
			Cedric = 7,
			Proto = 8
		}

		private readonly OneshotWindow oneshotWindow;

		private List<FollowerType> followers;

		private int lastAddedFollowerId = -1;

		private Dictionary<FollowerType, int> followerIds;

		public FollowerManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			followers = new List<FollowerType>();
			followerIds = new Dictionary<FollowerType, int>();
		}

		public string GetFollowerSheet(FollowerType followerType)
		{
			switch (followerType)
			{
			case FollowerType.Alula:
				return "green_npc_alula";
			case FollowerType.Rue:
				return "red_rue";
			case FollowerType.Plight:
				return "red_lamplighter";
			case FollowerType.Cedric:
				return "green_npc_cedric";
			case FollowerType.Proto:
				return "blue_npc_prototype";
			default:
				return null;
			}
		}

		public void AddFollower(FollowerType followerType)
		{
			if (!followers.Contains(followerType))
			{
				followers.Add(followerType);
				lastAddedFollowerId = oneshotWindow.tileMapMan.AddFollower(followerType, lastAddedFollowerId);
				followerIds[followerType] = lastAddedFollowerId;
			}
		}

		public void RemoveFollower(FollowerType followerType)
		{
			if (!followers.Contains(followerType))
			{
				return;
			}
			if (followerIds.TryGetValue(followerType, out var value))
			{
				int num = followers.IndexOf(followerType);
				if (num + 1 < followers.Count)
				{
					Follower followerWeInherit = oneshotWindow.tileMapMan.GetEntityByID(value) as Follower;
					FollowerType key = followers[num + 1];
					int entityId = followerIds[key];
					(oneshotWindow.tileMapMan.GetEntityByID(entityId) as Follower).InheritPosition(followerWeInherit);
				}
				oneshotWindow.tileMapMan.RemoveEntity(value);
				followerIds.Remove(followerType);
			}
			followers.Remove(followerType);
		}

		public List<FollowerType> GetFollowerSaveData()
		{
			return followers;
		}

		public void LoadFollowersSaveData(List<FollowerType> savedFollowers)
		{
			if (savedFollowers != null)
			{
				followers = savedFollowers;
			}
		}

		public void SpawnAllFollowers()
		{
			lastAddedFollowerId = -1;
			foreach (FollowerType follower in followers)
			{
				lastAddedFollowerId = oneshotWindow.tileMapMan.AddFollower(follower, lastAddedFollowerId);
				followerIds[follower] = lastAddedFollowerId;
			}
		}

		public bool IsFollowerInParty(FollowerType actor)
		{
			return followerIds.ContainsKey(actor);
		}
	}
}
