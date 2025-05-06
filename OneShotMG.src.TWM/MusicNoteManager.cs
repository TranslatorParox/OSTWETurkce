using System.Collections.Generic;

namespace OneShotMG.src.TWM
{
	internal class MusicNoteManager
	{
		private List<MusicNote> notes;

		public MusicNoteManager()
		{
			notes = new List<MusicNote>();
		}

		public void Update()
		{
			List<MusicNote> list = new List<MusicNote>();
			foreach (MusicNote note in notes)
			{
				note.Update();
				if (!note.IsAlive())
				{
					list.Add(note);
				}
			}
			foreach (MusicNote item in list)
			{
				notes.Remove(item);
			}
		}

		public void Draw(Vec2 windowPos)
		{
			foreach (MusicNote note in notes)
			{
				note.Draw(windowPos);
			}
		}

		public void SpawnNote(Vec2 spawnPos)
		{
			notes.Add(new MusicNote(spawnPos));
		}
	}
}
