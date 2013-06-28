using System;

using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;

using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;

namespace psmtetris
{
	public class Game
	{
		public static Game Instance;
		
		public Random Random = new Random();
		public Scene TitleScene;
		public Scene GameScene;

		static FreeBlock	freeBlock = new FreeBlock();

		
		public Game()
		{
		}

		public void Initialize()
		{
			TitleScene = new Scene();

			GameScene = new Scene();

			Vector2 ideal_screen_size = new Vector2(960.0f, 544.0f);
			Camera2D title_camera = TitleScene.Camera as Camera2D;
			title_camera.SetViewFromHeightAndCenter(ideal_screen_size.Y, ideal_screen_size / 2.0f);


//			Director.Instance.RunWithScene(new Scene(),true);
			Director.Instance.RunWithScene(TitleScene,true);

			// force tick so the scene is set
			Director.Instance.Update();

			StartTitle();
		}

		SpriteTile	spriteBg;
		public void StartTitle()
		{
			Sce.PlayStation.HighLevel.GameEngine2D.Scheduler.Instance.Unschedule(GameScene, TickGame);
			Sce.PlayStation.HighLevel.GameEngine2D.Scheduler.Instance.Schedule(TitleScene, TickTitle, 0.0f, false);

			spriteBg = Support.TiledSpriteFromFile("/Application/resources/bg01.png", 1, 1);
			Director.Instance.CurrentScene.AddChild(spriteBg,1000);
			
			FreeBlock.Initialize();
			freeBlock.NewMover();
			
//			var transition = new TransitionSolidFade(TitleScene) { PreviousScene = Director.Instance.CurrentScene, Duration = 1.5f, Tween = Sce.PlayStation.HighLevel.GameEngine2D.Base.Math.Linear	 };
//			Director.Instance.ReplaceScene(transition);

		}

		public void TickTitle(float dt)
		{
			//			debugString.Clear();

			// wait for transition
			if (Director.Instance.CurrentScene != TitleScene)
			{
				return;
			}
			freeBlock.Update();
		}

		public void StartGame()
		{
			Sce.PlayStation.HighLevel.GameEngine2D.Scheduler.Instance.Unschedule(TitleScene, TickTitle);
			Sce.PlayStation.HighLevel.GameEngine2D.Scheduler.Instance.Schedule(GameScene, TickGame, 0.0f, false);

			var transition = new TransitionSolidFade(GameScene) { PreviousScene = Director.Instance.CurrentScene, Duration = 1.5f, Tween = Sce.PlayStation.HighLevel.GameEngine2D.Base.Math.Linear };
			Director.Instance.ReplaceScene(transition);
		}

		public void TickGame(float dt)
		{
			freeBlock.Update();
		}
	}
}

