using Microsoft.Xna.Framework.Graphics;

namespace OneShotMG.src.EngineSpecificCode
{
	public class TempTexture
	{
		public const int TEXT_X_MARGIN = 2;

		public const int DEFAULT_FRAMES_TO_PERSIST = 5;

		public bool isValid;

		public RenderTarget2D renderTarget;

		public int framesLeftToPersist = 5;

		public void KeepAlive(int aliveFrames = 5)
		{
			framesLeftToPersist = aliveFrames;
		}
	}
}
