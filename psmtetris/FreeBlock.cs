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
		static FieldBuffer	fieldBuffer;
		static SpriteTile	spriteShadow;
		static Label		label;
		
		enum State{
			DOWN,
			BAKE_INTERVAL,
			GAMEOVER,
		}
		State	state = State.DOWN;
		int		interval = 0;
		Mover	mover;

		class Profile {
			public int	level;
			public int	line;
			public int	score;

			public int	deleteLine;
			public int	bakedTetrimino;
			
			public Profile()
			{
				level = 4;
				line = 0;
				score = 0;
				deleteLine = 0;
				bakedTetrimino = 0;
			}
		}
		static Profile	profile;
		
		//スクリーン開始位置
		static float	ConvertScreenX(int x)
		{
			return (x * WidthPixel) + 960/2 - WidthPixel*5;
		}
		static float	ConvertScreenY(int  y)
		{
			return (-y * WidthPixel + 544 - WidthPixel*8);
		}
		
		//ラインを管理するバッファ
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
		//フィールド全体を管理するバッファ
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
			
			profile = new Profile();
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
		
		//ブロック表示に用いるスプライト
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

		//移動するブロック
		public class Mover
		{
			BlockPatern.Type	shapeType;
			
			BlockSprite			blockSprite = new BlockSprite();
			Location			current;
			int					downTime;
			int					downInterval;
			int					bakeInterval;
			
			public Mover(BlockPatern.Type type,int x,int y,int time=60)
			{
				current.position.X = x;
				current.position.Y = y;
				shapeType = type;
				blockSprite.spriteList.Color = BlockPatern.GetColor(type);
				downTime = time;
				downInterval = time;
				bakeInterval = 30;
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

			//MoverをFieldに焼き付ける（ようするにブロックを固定化するということ）
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
				int	addScore = 0;
				bool	isTimeout = false;
				var gamePadData = Input2.GamePad.GetData(0);

				Location	next = current;

				if(Game.Instance.pad.Circle.Press())next.TurnRight();
				if(Game.Instance.pad.Cross.Press())next.TurnLeft();

				bool	isDown = false;
				if(Game.Instance.pad.Right.Repeat())next.MoveRight();
				if(Game.Instance.pad.Left.Repeat())next.MoveLeft();
				
				//先に回転、左右移動が可能かを判定する
				if(CollisionDetect(next)==false){
					current = next;
				}
				next = current;

				//落下処理はパッドとインターバルに応じて行う
				if(Game.Instance.pad.Down.Down())isDown = true;
				downInterval--;
				bakeInterval--;
				if(downInterval<=0){
					isDown = true;
				}
				if(isDown){
					next.MoveDown();
				}
				//落下処理後に何かに当たっている場合、それは下限ブロックのはず
				bool	isCollision = CollisionDetect(next);
				if(isCollision==false){
					if(isDown && downInterval<=0) {
						downInterval = downTime;
						bakeInterval = 30;
					}
					if(Game.Instance.pad.Down.Down()){
						addScore += 1;
					}
					current = next;
				}else{
					if(isDown && bakeInterval<=0) {
						downInterval = downTime;
						bakeInterval = 30;
						BakeToField();
						isTimeout = true;
					}
				}
				FreeBlock.profile.score += addScore;
				UpdateSprite();
				return isTimeout;
			}
			
			private ushort[,] getPattern()
			{
				return BlockPatern.GetPatern(shapeType);
			}
		}
		
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

			label.Text = "SCORE\n";
			label.Text += FreeBlock.profile.score.ToString().PadLeft(6) + "\n";
			label.Text += "\n";
			label.Text += "LINE\n";
			label.Text += FreeBlock.profile.line.ToString().PadLeft(6) + "\n";
			label.Text += "\n";
			label.Text += "LEVEL\n";
			label.Text += (FreeBlock.profile.level).ToString().PadLeft(6) + "\n";
			label.Text += "\n";
		}

		//そろったラインの判定
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
			if (deleteLine>0) {
				FreeBlock.profile.line += deleteLine;
				FreeBlock.profile.deleteLine += deleteLine;
			}else{
				FreeBlock.profile.bakedTetrimino++;
			}
			if (FreeBlock.profile.deleteLine>=5 || FreeBlock.profile.bakedTetrimino>=10) {
				FreeBlock.profile.level++;
				FreeBlock.profile.deleteLine = 0;
				FreeBlock.profile.bakedTetrimino = 0;
			}
		}

		//新しいMoverの生成
		public void	NewMover()
		{
			if(mover!=null)mover.Cleanup();
			int	downInterval = 0;
			switch(FreeBlock.profile.level){
			case 1:	downInterval = 120;	break;
			case 2:	downInterval =  90;	break;
			case 3:	downInterval =  60;	break;
			case 4:	downInterval =  30;	break;
			case 5:	downInterval =  20;	break;
			case 6:	downInterval =  10;	break;
			case 7:	downInterval =   5;	break;
			case 8:	downInterval =   3;	break;
			case 9:	downInterval =   1;	break;
			case 10:downInterval =  30;	break;
			case 12:downInterval =  10;	break;
			case 13:downInterval =   5;	break;
			case 14:downInterval =   1;	break;
			}
			mover = new Mover(BlockPatern.GetRandomType(),3,0,downInterval);
			mover.UpdateSprite();
		}
	}
}

