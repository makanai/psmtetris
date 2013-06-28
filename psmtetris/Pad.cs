using System;
using Sce.PlayStation.Core;

using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;

namespace psmtetris
{
	public class Pad
	{
		public class ButtonStatus2
		{
			private Input2.ButtonState	State;
			private int	repeat;
			private int	repeatMax=30;
			public ButtonStatus2()
			{
			}
			public void Update(Input2.ButtonState state)
			{
				State = state;
				if( State.On ){
					if( repeat<repeatMax )
						repeat++;
				}else{
					repeat = 0;
				}
			}
			public bool	Down()
			{
				return State.Down;
			}
			public bool	On()
			{
				return State.On;
			}
			public bool	Press()
			{
				return State.Press;
			}
			public bool	Release()
			{
				return State.Release;
			}
			public bool Repeat()
			{
				if( State.Press || (repeat>=repeatMax && State.On) )return true;
				return false;
			}
		}
		public ButtonStatus2	Triangle;
		public ButtonStatus2	Cross;
		public ButtonStatus2	Circle;
		public ButtonStatus2	Square;
		public ButtonStatus2	Up;
		public ButtonStatus2	Down;
		public ButtonStatus2	Right;
		public ButtonStatus2	Left;
		
		public Pad ()
		{
			var gamePadData = Input2.GamePad.GetData(0);

			Triangle = new ButtonStatus2();
			Cross = new ButtonStatus2();
			Circle = new ButtonStatus2();
			Square = new ButtonStatus2();
			Right = new ButtonStatus2();
			Left = new ButtonStatus2();
			Up = new ButtonStatus2();
			Down = new ButtonStatus2();
		}

		public void Update()
		{
			var gamePadData = Input2.GamePad.GetData(0);
			Triangle.Update(gamePadData.Triangle);
			Cross.Update(gamePadData.Cross);
			Circle.Update(gamePadData.Circle);
			Square.Update(gamePadData.Square);
			Right.Update(gamePadData.Right);
			Left.Update(gamePadData.Left);
			Up.Update(gamePadData.Up);
			Down.Update(gamePadData.Down);
		}
	}
}

