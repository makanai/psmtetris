using System;
using System.Collections.Generic;

using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;

using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;


namespace psmtetris
{
	public class FreeBlock
	{
		static int	BlockPaternSize = 4;
		static int	FieldWidth = 10;
		static int	FieldHeight = 22;
		static float WidthPixel = 8*2;
		
		static float	ConvertScreenX(int x)
		{
			return (x * WidthPixel) + 960/2 - WidthPixel*5;
		}
		static float	ConvertScreenY(int  y)
		{
			return (-y * WidthPixel + 544 - WidthPixel*8);
		}
		
		class LineBuffer
		{
			SpriteList		spriteList;
			SpriteTile[]	sprite = new SpriteTile[FieldWidth];
			ushort[]		buffer = new ushort[FieldWidth];
			public LineBuffer(int y)
			{
				spriteList = Support.TiledSpriteListFromFile("/Application/resources/Player.png", 1, 1);
				for(int i=0;i<FieldWidth;++i) {
					sprite[i] = Support.TiledSpriteFromFile("/Application/resources/Player.png", 1, 1);
					sprite[i].Quad.T.X = ConvertScreenX(i);
					sprite[i].Quad.T.Y = ConvertScreenY(y);
					sprite[i].Quad.S.X = WidthPixel;
					sprite[i].Quad.S.Y = WidthPixel;
				}
				Director.Instance.CurrentScene.AddChild(spriteList,100);
			}
			public void Cleanup()
			{
				Director.Instance.CurrentScene.RemoveChild(spriteList,true);
			}
			public void SetY(int y)
			{
				for(int i=0;i<FieldWidth;++i) {
					sprite[i].Quad.T.X = ConvertScreenX(i);
					sprite[i].Quad.T.Y = ConvertScreenY(y);
				}
			}
			public bool IsCollision(int index)
			{
				if( buffer[index]!=0 )return true;
				return false;
			}
			public void SetBit(int index, ushort bit,BlockPatern.Type type)
			{
				if(bit==1 && buffer[index]!=bit) {
					spriteList.AddChild(sprite[index]);
				}
				buffer[index] = bit;
			}
			public bool	IsLineCopmplete()
			{
				bool	isComplete = true;
				for(int i=0;i<FieldWidth;++i){
					if(buffer[i]==0){
						isComplete = false;
						break;
					}
				}
				return isComplete;
			}
		}
		class FieldBuffer
		{
			public LineBuffer[]	lineBuffer = new LineBuffer[FieldHeight];
			public FieldBuffer()
			{
				for(int i=0;i<FieldHeight;++i) {
					lineBuffer[i] = new LineBuffer(i);
				}
			}
		}
		static FieldBuffer	fieldBuffer;
		static SpriteTile	spriteShadow;
		static Label		label;

		public static void Initialize()
		{
			fieldBuffer = new FieldBuffer();

			spriteShadow = Support.TiledSpriteFromFile("/Application/resources/bg00.png", 1, 1);
			spriteShadow.Quad.T.X = ConvertScreenX(0);
			spriteShadow.Quad.T.Y = ConvertScreenY(-1);
			spriteShadow.Quad.S.X = FieldWidth * WidthPixel;
			spriteShadow.Quad.S.Y = -FieldHeight * WidthPixel;
			Director.Instance.CurrentScene.AddChild(spriteShadow,500);

			label = new Label();
			label.Scale = new Vector2(2,2);
			label.Position = new Vector2(300,540-128);
			Director.Instance.CurrentScene.AddChild(label);
		}
		
		public struct Location
		{
			public Vector2i		position;
			public int			paternIndex;

			public void TurnRight()
			{
				paternIndex++;
				if(paternIndex>=4)paternIndex=0;
			}
			public void TurnLeft()
			{
				paternIndex--;
				if(paternIndex<0)paternIndex=3;
			}
			public void MoveRight()
			{
				position.X++;
			}
			public void MoveLeft()
			{
				position.X--;
			}
			public void MoveDown()
			{
				position.Y++;
			}
			public void MoveUp()
			{
				position.Y--;
			}
		}
		class BlockSprite
		{
			SpriteTile[]			sprite = new SpriteTile[4];
			public SpriteList		spriteList;

			public BlockSprite()
			{
				spriteList = Support.TiledSpriteListFromFile("/Application/resources/Player.png", 1, 1);
				for(int i=0;i<4;i++){
					sprite[i] = Support.TiledSpriteFromFile("/Application/resources/Player.png", 1, 1);
					sprite[i].Quad.S.X = WidthPixel;
					sprite[i].Quad.S.Y = WidthPixel;
					spriteList.AddChild(sprite[i]);
				}
				Director.Instance.CurrentScene.AddChild(spriteList,100);
			}
			public void SetInner(int index, float x, float y)
			{
				sprite[index].Quad.T.X = x;
				sprite[index].Quad.T.Y = y;
			}
		}
		
		public class Mover
		{
			BlockPatern.Type	shapeType;
			
			BlockSprite			blockSprite = new BlockSprite();
			Location			current;
			int					downTime;
			int					counter;
			
			public Mover(BlockPatern.Type type,int x,int y,int time=60)
			{
				current.position.X = x;
				current.position.Y = y;
				shapeType = type;
				blockSprite.spriteList.Color = BlockPatern.GetColor(type);
				downTime = time;
				counter = time;
			}
			public void Cleanup()
			{
				Director.Instance.CurrentScene.RemoveChild(blockSprite.spriteList,true);
			}
			
			
			public void UpdateSprite()
			{
				ushort[,] patern = getPattern();
				
				int	count=0;
				for(int  i=0;i<BlockPaternSize;i++) {
					for(int j=0;j<BlockPaternSize;j++) {
						ushort	isBits = patern[current.paternIndex,i*BlockPaternSize + j];
						if(isBits==0)continue;
						
						float	x = ConvertScreenX(j+current.position.X);
						float	y = ConvertScreenY(i+current.position.Y);
						
						blockSprite.SetInner(count,x,y);
						count++;
					}
				}
			}
			
			public bool CollisionDetect()
			{
				return CollisionDetect(current);
			}

			public bool CollisionDetect(Location locate)
			{
				ushort[,] patern = getPattern();
				
				for(int  i=0;i<BlockPaternSize;i++) {
					for(int j=0;j<BlockPaternSize;j++) {
						ushort	isBits = patern[locate.paternIndex,i*BlockPaternSize + j];
						if(isBits==0)continue;
						
						int x = locate.position.X + j;
						int	y = locate.position.Y + i;
						if(x<0)return true;
						if(x>=FieldWidth)return true;
						if(y<0)return true;
						if(y>=FieldHeight)return true;
						
						if(FreeBlock.fieldBuffer.lineBuffer[y].IsCollision(x))return true;
					}
				}
				return false;
			}

			//MoveをFieldに焼き付ける（ようするにブロックを固定化するということ）
			void BakeToField()
			{
				ushort[,] patern = getPattern();
				
				for(int  i=0;i<BlockPaternSize;i++) {
					for(int j=0;j<BlockPaternSize;j++) {
						ushort	isBits = patern[current.paternIndex,i*BlockPaternSize + j];
						if(isBits==0)continue;
						
						int x = current.position.X + j;
						int	y = current.position.Y + i;
						if(x<0)continue;
						if(x>=FieldWidth)continue;
						if(y<0)continue;
						if(y>=FieldHeight)continue;
						
						FreeBlock.fieldBuffer.lineBuffer[y].SetBit(x,1,shapeType);
					}
				}
			}
			
			public bool Update()
			{
				bool	isTimeout = false;
				var gamePadData = Input2.GamePad.GetData(0);

				Location	next = current;
				if(gamePadData.Circle.Press)next.TurnRight();
				if(gamePadData.Cross.Press)next.TurnLeft();

				bool	isDown = false;
				if(gamePadData.Right.Down)next.MoveRight();
				if(gamePadData.Left.Down)next.MoveLeft();
				
				//先に回転、左右移動によるコリジョンを行う
				if(CollisionDetect(next)==false){
					current = next;
				}
				next = current;
				
				//if(gamePadData.Up.Down)next.MoveUp();

				//落下処理はパッドとインターバルに応じて行う
				if(gamePadData.Down.Down)isDown = true;
				counter--;
				if(counter<=0){
					isDown = true;
				}
				if(isDown){
					next.MoveDown();
					counter=downTime;
				}
				//落下処理後に何かに当たっている場合、それは下限ブロックのはず
				bool	isCollision = CollisionDetect(next);
				if(isCollision==false){
					current = next;
				}else{
					if(isDown) {
						BakeToField();
						isTimeout = true;
					}
				}

				UpdateSprite();
				return isTimeout;
			}
			
			private ushort[,] getPattern()
			{
				return BlockPatern.GetPatern(shapeType);
			}
		}
		
		enum State{
			DOWN,
			BAKE_INTERVAL,
			GAMEOVER,
		}
		State	state = State.DOWN;
		int		interval = 0;
		public void Update()
		{
			switch(state){
			case State.DOWN:
				if(mover.Update()==true){
					state = State.BAKE_INTERVAL;
					interval = 60;
					mover.Cleanup();
				}
				break;
			case State.BAKE_INTERVAL:
				interval--;
				if(interval>0)break;
				NewMover();
				if(mover.CollisionDetect()==true){
					state = State.GAMEOVER;
				}else{
					UpdateLine ();
					state = State.DOWN;
				}
				break;
			default:
				break;
			}

//			Director.Instance.CurrentScene.AddChild(label);
			label.Text = "SCORE\n";
			label.Text += "32150\n";
			label.Text += "\n";
			label.Text += "LINES\n";
			label.Text += "    5\n";
			label.Text += "\n";
			label.Text += "LEVEL\n";
			label.Text += "    2\n";
			label.Text += "\n";
		}

		bool CheckLineComplete()
		{
			for(int i=FreeBlock.FieldHeight-1;i>=0;--i) {
				if(fieldBuffer.lineBuffer[i].IsLineCopmplete()){
					return true;
				}
			}
			return false;
		}
		public void UpdateLine()
		{
			int	deleteLine=0;
			for(int i=FreeBlock.FieldHeight-1;i>=0;--i) {
				if(fieldBuffer.lineBuffer[i].IsLineCopmplete()){
					//最大４ライン消える
					fieldBuffer.lineBuffer[i].Cleanup();
					for(int j=i;j>=1;--j){
						fieldBuffer.lineBuffer[j] = fieldBuffer.lineBuffer[j-1];
						fieldBuffer.lineBuffer[j].SetY (j);
					}
					fieldBuffer.lineBuffer[0] = new LineBuffer(0);
					++deleteLine;
					if(deleteLine>=4)break;
					++i;//ラインがつまったので
				}
			}
		}

		Mover mover;
		public void	NewMover()
		{
			if(mover!=null)mover.Cleanup();
			int	downInterval = 60;
			mover = new Mover(BlockPatern.GetRandomType(),2,0,downInterval);
			mover.UpdateSprite();
		}
	}
}
