using System;
using System.Collections.Generic;

using Sce.PlayStation.Core;
using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;

namespace psmtetris
{
	public class BlockPatern
	{
		public enum Type
		{
			Red,
			Yellow,
			Cyan,//水色
			Blue,
			Orange,
			Green,
			Magenta,//紫
		};

		static ushort[,]	redPatern = {
			{
				0,0,1,0,
				0,0,1,0,
				0,0,1,0,
				0,0,1,0,
			},
			{
				0,0,0,0,
				1,1,1,1,
				0,0,0,0,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,0,1,0,
				0,0,1,0,
				0,0,1,0,
			},
			{
				0,0,0,0,
				1,1,1,1,
				0,0,0,0,
				0,0,0,0,
			},
		};
		static ushort[,]	cyanPatern = {
			{
				0,0,0,0,
				0,1,1,1,
				0,0,1,0,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,0,1,1,
				0,0,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,0,1,0,
				0,1,1,1,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,1,1,0,
				0,0,1,0,
				0,0,0,0,
			},
		};
		static ushort[,]	greenPatern = {
			{
				0,0,0,0,
				0,0,1,1,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,1,0,0,
				0,1,1,0,
				0,0,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,0,1,1,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,1,0,0,
				0,1,1,0,
				0,0,1,0,
				0,0,0,0,
			},
		};
		static ushort[,]	magentaPatern = {
			{
				0,0,0,0,
				1,1,0,0,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,1,1,0,
				0,1,0,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				1,1,0,0,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,1,1,0,
				0,1,0,0,
				0,0,0,0,
			},
		};
		static ushort[,]	yellowPatern = {
			{
				0,0,0,0,
				0,1,1,0,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,1,1,0,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,1,1,0,
				0,1,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,1,1,0,
				0,1,1,0,
				0,0,0,0,
			},
		};
		static ushort[,]	orangePatern = {
			{
				0,0,0,0,
				0,1,1,1,
				0,1,0,0,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,0,1,0,
				0,0,1,1,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,0,0,1,
				0,1,1,1,
				0,0,0,0,
			},
			{
				0,1,1,0,
				0,0,1,0,
				0,0,1,0,
				0,0,0,0,
			},
		};
		static ushort[,]	bluePatern = {
			{
				0,0,0,0,
				0,1,1,1,
				0,0,0,1,
				0,0,0,0,
			},
			{
				0,0,1,1,
				0,0,1,0,
				0,0,1,0,
				0,0,0,0,
			},
			{
				0,0,0,0,
				0,1,0,0,
				0,1,1,1,
				0,0,0,0,
			},
			{
				0,0,1,0,
				0,0,1,0,
				0,1,1,0,
				0,0,0,0,
			},
		};
		
		public static Type GetRandomType()
		{
			int	index = Game.Instance.Random.Next()%7;
			switch(index){
			case 0:
				return Type.Red;
			case 1:
				return Type.Green;
			case 2:
				return Type.Magenta;
			case 3:
				return Type.Yellow;
			case 4:
				return Type.Cyan;
			case 5:
				return Type.Orange;
			case 6:
				return Type.Blue;
			}
			return Type.Red;
		}
		
		public static Vector4 GetColor(Type shapeType)
		{
			switch(shapeType){
			case Type.Red:
				return Colors.Red;
			case Type.Green:
				return Colors.Green;
			case Type.Magenta:
				return Colors.Magenta;
			case Type.Yellow:
				return Colors.Yellow;
			case Type.Cyan:
				return Colors.Cyan;
			case Type.Orange:
				return Colors.Orange;
			case Type.Blue:
				return Colors.Blue;
			}
			return Colors.Red;
		}

		public static ushort[,] GetPatern(Type shapeType)
		{
			switch(shapeType){
			case Type.Red:
				return redPatern;
			case Type.Green:
				return greenPatern;
			case Type.Magenta:
				return magentaPatern;
			case Type.Yellow:
				return yellowPatern;
			case Type.Cyan:
				return cyanPatern;
			case Type.Orange:
				return orangePatern;
			case Type.Blue:
				return bluePatern;
			}
			return redPatern;
		}
		
		public BlockPatern ()
		{
		}
	}
}

