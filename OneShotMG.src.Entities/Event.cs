namespace OneShotMG.src.Entities
{
	public class Event
	{
		public class Page
		{
			public class Condition
			{
				public bool switch1_valid;

				public bool switch2_valid;

				public bool variable_valid;

				public bool self_switch_valid;

				public int switch1_id;

				public int switch2_id;

				public int variable_id;

				public int variable_value;

				public string self_switch_ch = string.Empty;
			}

			public class Graphic
			{
				public int tile_id;

				public string character_name = string.Empty;

				public int character_hue;

				public int direction;

				public int pattern;

				public int opacity;

				public int blend_type;
			}

			public Condition condition;

			public Graphic graphic;

			public int move_type;

			public int move_speed = 2;

			public int move_frequency = 2;

			public MoveRoute move_route;

			public bool walk_anime;

			public bool step_anime;

			public bool direction_fix;

			public bool through;

			public bool always_on_top;

			public bool always_on_bottom;

			public int trigger;

			public EventCommand[] list;
		}

		public int id;

		public string name = string.Empty;

		public int x;

		public int y;

		public Page[] pages;
	}
}
