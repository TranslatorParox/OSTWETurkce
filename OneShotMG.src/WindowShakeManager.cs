using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class WindowShakeManager
	{
		private OneshotWindow oneshotWindow;

		private int totalShakeTime;

		private int shakeTimer;

		private Vec2 startPosition = Vec2.Zero;

		public WindowShakeManager(OneshotWindow oneshotWindow)
		{
			this.oneshotWindow = oneshotWindow;
		}

		public void Update()
		{
			if (totalShakeTime > 0)
			{
				shakeTimer++;
				if (shakeTimer >= totalShakeTime)
				{
					oneshotWindow.Pos = startPosition;
					shakeTimer = 0;
					totalShakeTime = 0;
				}
				else
				{
					int num = (totalShakeTime - shakeTimer) / 2;
					oneshotWindow.Pos = new Vec2(startPosition.X + MathHelper.Random(-num, num), startPosition.Y + MathHelper.Random(-num, num));
				}
			}
		}

		public void Shake(int shakeTime)
		{
			shakeTimer = 0;
			totalShakeTime = shakeTime;
			startPosition = oneshotWindow.Pos;
		}
	}
}
