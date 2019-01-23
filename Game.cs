using System;
using System.Collections.Generic;
using System.IO;

namespace RiverHex
{
	/// <summary>
	/// Class for a RiverHex Game
	/// </summary>	
	public class Game
	{
		/// <summary>
		/// Type of player
		/// </summary>
		public enum PlayerType { Human, Computer };
		
		/// <summary>
		/// Playing level of computer
		/// </summary>		
		public enum Level { Easy, Medium, Hard };	
		
		/// <value>
		/// Size of the game
		/// </value>
		public int Size
		{
			get { return _size; }
		}
		private int _size;
		
		/// <summary>
		/// Board of game
		/// </summary>
		private Hex[,] _board;
		
		/// <summary>
		/// Get value of an hex.
		/// </summary>
		/// <param name="line">line to test</param>
		/// <param name="column">column to test</param>
		/// <returns>hex state, none if hex don't exist</returns>
		public HexState Board(int line, int column)
		{
			// Check valid 
			if (line < 0 || column < 0 || line > _size-1 || column > _size-1)
			    return HexState.None;
			
			return _board[line,column].State;
		}

		/// <value>
		/// First player color
		/// </value>
		public HexState FirstPlayer
		{
			get { return _firstPlayer; }
		}
		private HexState _firstPlayer;
		
		/// <value>
		/// Current player color
		/// </value>
		public HexState CurrentPlayer
		{
			get { return _currentPlayer; }
		}
		private HexState _currentPlayer;
		
		/// <value>
		/// Red player type
		/// </value>
		public PlayerType RedPlayerType
		{
			get { return _redPlayerType; }
		}
		private PlayerType _redPlayerType;
		
		/// <value>
		/// Bue player type
		/// </value>
		public PlayerType BluePlayerType
		{
			get { return _bluePlayerType; }
		}
		private PlayerType _bluePlayerType;
		
		/// <summary>
		/// Get player type (Human or Computer) for a color.
		/// </summary>
		/// <param name="color">Color to check (None is processed like Red)</param>
		/// <returns>
		/// A <see cref="PlayerType"/>
		/// </returns>
		public PlayerType GetPlayerType(HexState color)
		{
			if (color == HexState.Blue)
				return _bluePlayerType;
			return _redPlayerType;
		}
		
		/// <value>
		/// Computer player level
		/// </value>
		public Level ComputerLevel
		{
			get { return _computerLevel; }
		}
		private Level _computerLevel;
		
		/// <summary>
		/// Coordinate of previous play
		/// </summary>
		private List<HexCoord> _redHexCoord;
		public List<HexCoord> RedHistoric
		{
			get { return _redHexCoord; }
		}		
		private List<HexCoord> _blueHexCoord;
		public List<HexCoord> BlueHistoric
		{
			get { return _blueHexCoord; }
		}	
		
		/// <summary>
		/// Game is ended 
		/// </summary>
		private bool _ended;
		
		/// <summary>
		/// Winner color 
		/// </summary>
		private HexState _winner;
		
#region Brain stuff
		/// <summary>
		/// Strategic context for computer player
		/// </summary>
		private ComputerStrategy _redStrategy;
		private ComputerStrategy _blueStrategy;
#endregion
		
#region Drawing stuff
		/// <value>
		/// Color used for winner path
		/// </value>
		internal static Gdk.Color WinPathColor = new Gdk.Color(255, 255, 0);
		
		/// <summary>
		/// Drawing used for winning path
		/// </summary>
		private Gdk.Point[] _winningPath;
		
		/// <summary>
		/// Drawing used for border lines
		/// </summary>
		private Gdk.Point[] redLeftLine;
		private Gdk.Point[] redRightLine;
		private Gdk.Point[] blueLeftLine;
		private Gdk.Point[] blueRightLine;		
#endregion	
	
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="size">size of the game</param>
		/// <param name="redType">type of red player</param>
		/// <param name="blueType">type of blue player</param>
		/// <param name="level">game level of computer player (if need)</param>
		/// <param name="firstPlayer">first player color</param>
		/// <param name="area">drawing rectangle</param>
		public Game(int size, PlayerType redType, PlayerType blueType, Level level, HexState firstPlayer, Gdk.Rectangle area)
		{
			// Create grid
			_size = size;
			_board = new Hex[_size,_size];
			_redHexCoord = new List<HexCoord>();
			_blueHexCoord = new List<HexCoord>();
			_ended = false;
			_winner = HexState.None;
			_winningPath = null;
			_redStrategy = null;
			_blueStrategy = null;
			_firstPlayer = firstPlayer;
			_currentPlayer = _firstPlayer;
			_redPlayerType = redType;
			_bluePlayerType = blueType;
			_computerLevel = level;
			for (int i = 0 ; i < _size ; i++)
				for (int j = 0 ; j < _size ; j++)
					_board[i,j] = null;
			redLeftLine = new Gdk.Point[_size*2];
			redRightLine = new Gdk.Point[_size*2];
			blueLeftLine = new Gdk.Point[_size*2+1];
			blueRightLine = new Gdk.Point[_size*2+1];
			
			// Compute hex in grid
			ComputeGrid(area);		
		}
		
		/// <summary>
		/// Private constructor.
		/// </summary>
		private Game()
		{
		}
		
		/// <summary>
		/// Create a game from a Stream.
		/// </summary>
		/// <param name="sr">stream to use</param>
		/// <returns>a game if success, null else</returns>
		static public Game CreateFromStream(Stream sr)
		{
			// Prepare to read
			BinaryReader br = new BinaryReader(sr);
			
			// Read game
			Game _game = new Game();
			int _size = br.ReadInt32();
			_game._size = _size;
			_game._board = new Hex[_size,_size];	
			for (int i = 0 ; i < _size ; i++)
			{
				for (int j = 0 ; j < _size ; j++)
				{
					_game._board[i,j] = new Hex(new Gdk.Point(0,0));
					_game._board[i,j].State = (HexState)br.ReadByte();
				}
			}
			
			// Read properties
			_game._redPlayerType = (PlayerType)br.ReadByte();
			_game._bluePlayerType = (PlayerType)br.ReadByte();
			_game._computerLevel = (Level)br.ReadByte();
			_game._firstPlayer = (HexState)br.ReadByte();			
			
			// Read historic
			int count = br.ReadInt32();
			_game._redHexCoord = new List<HexCoord>();
			for (int i = 0 ; i < count ; i++)
				_game._redHexCoord.Add(HexCoord.CreateFromStream(br));
			count = br.ReadInt32();
			_game._blueHexCoord = new List<HexCoord>();
			for (int i = 0 ; i < count ; i++)
				_game._blueHexCoord.Add(HexCoord.CreateFromStream(br));
			
			// Read strategic context
			_game._redStrategy = null;
			bool hasContext = br.ReadBoolean();
			if (hasContext)
				_game._redStrategy = ComputerStrategy.CreateFromStream(_game, br);
			_game._blueStrategy = null;
			hasContext = br.ReadBoolean();
			if (hasContext)
				_game._blueStrategy = ComputerStrategy.CreateFromStream(_game, br);
			
			// Read play context
			_game._currentPlayer = (HexState)br.ReadByte();
			_game._ended = br.ReadBoolean();
			_game._winner = (HexState)br.ReadByte();
			
			// Close reader
			br.Close();

			// Compute winner if need
			_game.redLeftLine = new Gdk.Point[_size*2];
			_game.redRightLine = new Gdk.Point[_size*2];
			_game.blueLeftLine = new Gdk.Point[_size*2+1];
			_game.blueRightLine = new Gdk.Point[_size*2+1];
			if (_game._ended)
			{
				_game._ended = false;
				_game.CheckEndOfGame();
			}

			return _game;
		}
		
		/// <summary>
		/// Compute grid and hex size
		/// </summary>
		/// <param name="area">area for the board</param>
		private void ComputeGrid(Gdk.Rectangle area)
		{		
			// Initialize border line
			int rlCount = 0;
			int rrCount = 0;
			int blCount = 0;
			int brCount = 0;
			
			// Compute optimal size
			int horiz = (_size-1)*2+1;
			horiz += (horiz+1)/2;
			Hex.Width = area.Width/horiz;
			Hex.Height = 4*Hex.Width/3;
			
			// Compute margin to center grid
			int leftMargin = (area.Width-Hex.Width*horiz)/2;
			
			// Create grid
			Gdk.Point p0 = new Gdk.Point(leftMargin, area.Height/2);	
			for (int i = 0 ; i < _size ; i++)
			{
				Gdk.Point p1 = p0;
				for (int j = 0 ; j < _size ; j++)
				{
					Hex current = _board[j,i];
					if (current == null)
					{
						current = new Hex(p1);
						_board[j,i] = current;
					}
					else
						current.Position = p1;
					p1.X = p1.X+Hex.Width+Hex.Width/2;
					p1.Y = p1.Y+Hex.Height/2;
					if (j == 0)
					{
						blueLeftLine[blCount++] = current.MiddleLeftVertex;
						blueLeftLine[blCount++] = current.TopLeftVertex;
						if (i == _size-1)
							blueLeftLine[blCount++] = current.TopRightVertex;
					}
					else if (j == _size-1)
					{
						blueRightLine[brCount++] = current.BottomLeftVertex;
						blueRightLine[brCount++] = current.BottomRightVertex;
						if (i == _size-1)
							blueRightLine[brCount++] = current.MiddleRightVertex;
					}					
					if (i == 0)
					{
						redLeftLine[rlCount++] = current.MiddleLeftVertex;
						redLeftLine[rlCount++] = current.BottomLeftVertex;						
					}
					else if (i == _size-1)
					{
						redRightLine[rrCount++] = current.TopRightVertex;
						redRightLine[rrCount++] = current.MiddleRightVertex;
					}					
				}
				p0.X = p0.X+Hex.Width+Hex.Width/2;
				p0.Y = p0.Y-Hex.Height/2;
			}
		}
		
		/// <summary>
		///  Resize board area
		/// </summary>
		/// <param name="area">new area size</param>
		public void Resize(Gdk.Rectangle area)
		{
			ComputeGrid(area);
		}

		/// <summary>
		/// Find hex coordinate containing a point
		/// </summary>
		/// <param name="position">position to inspect</param>
		/// <param name="line">line found</param>
		/// <param name="column">column found</param>
		/// <returns>true if find, false else</returns>
		public bool FindHex(Gdk.Point position, ref int line, ref int column)
		{
			// Look in each hex
			for (int i = 0; i < _size ; i++)
			{
				for (int j = 0; j < _size ; j++)
				{
					Hex current = _board[i,j];
					if (current.IsInHex(position))
					{
						line = i;
						column = j;
						return true;
					}
				}
			}
			
			// Not found
			return false;
		}
		
		/// <summary>
		/// Test if an hexagon is playable (i.e. exist and free).
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		/// <returns>true if playable</returns>
		public bool IsPlayable(int line, int column)
		{	
			// Check valid 
			if (line < 0 || column < 0 || line > _size-1 || column > _size-1)
			    return false;
			
			// Check avaibility
			if (_board[line,column].State != HexState.None)
				return false;
			
			return true;
		}
		
		/// <summary>
		/// The human player click in an hex.
		/// If valid set the hex new color, change player, test end of game.
		/// </summary>
		/// <param name="line">line to play</param>
		/// <param name="column">color to play</param>
		/// <returns>true if valid, false else</returns>
		public bool HumanPlay(int line, int column)
		{
			// Check condition: player type and end of play
			if (GetPlayerType(_currentPlayer) != PlayerType.Human || _ended)
				return false;
			
			// Check valid 
			if (!IsPlayable(line, column))
				return false;
			
			// Change state
			_board[line,column].State = _currentPlayer;
			
			// Save play change current player
			HexCoord play = new HexCoord(line, column);
			if (_currentPlayer == HexState.Blue)
			{
				_blueHexCoord.Add(play);
				_currentPlayer = HexState.Red;
			}
			else
			{
				_redHexCoord.Add(play);
				_currentPlayer = HexState.Blue;
			}
			
			// Test end of game
			CheckEndOfGame();
			
			return true;
		}

		/// <summary>
		/// Ask computer to play in an hex.
		/// Set the new hex color, change player, test end of game.
		/// </summary>
		public void ComputerPlay()
		{
			// Check condition: player type and end of play
			if (GetPlayerType(_currentPlayer) != PlayerType.Computer || _ended)
				return;
			
			// Get or create strategic context
			ComputerStrategy strategy = null;
			if (_currentPlayer == HexState.Red)
			{
				if (_redStrategy == null)
					_redStrategy = new ComputerStrategy(this, HexState.Red, _computerLevel);
				strategy = _redStrategy;
			}
			else
			{
				if (_blueStrategy == null)
					_blueStrategy = new ComputerStrategy(this, HexState.Blue, _computerLevel);
				strategy = _blueStrategy;
			}
			
			// Play
			HexCoord play = strategy.ComputePlay();
			_board[play.i,play.j].State = _currentPlayer;
			
			// Save play and change current player	
			strategy.Historic.Add(play);
			if (_currentPlayer == HexState.Blue)
				_currentPlayer = HexState.Red;
			else
				_currentPlayer = HexState.Blue;	
			
			// Test end of game
			CheckEndOfGame();			
		}
		
		/// <summary>
		/// Force state for an hex.
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		/// <param name="color">new color</param>
		/// <returns>true if set is valid (line and column exist)</returns>
		public bool SetHexState(int line, int column, HexState color)
		{
			// Check valid 
			if (line < 0 || column < 0 || line > _size-1 || column > _size-1)
				return false;
			
			// Change state
			_board[line,column].State = color;
			
			return true;
		}
	
		/// <summary>
		/// Get current state for an hex.
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		/// <returns>current hex state, free if invalid (line and column don't exist)</returns>		
		public HexState GetHexState(int line, int column)
		{
			// Check valid 
			if (line < 0 || column < 0 || line > _size-1 || column > _size-1)
				return HexState.None;
			
			// curent state
			return _board[line,column].State;		
		}

		/// <summary>
		/// Get the number of busy hex.
		/// </summary>
		/// <returns>a number between 0 and Size x Size</returns>
		private int BusyHexCount()
		{
			int count = 0;
			for (int i = 0 ; i < _size ; i++)
				for (int j = 0 ; j < _size ; j++)
					if (_board[i,j].State != HexState.None)
						count++;
			return count;
		}
		
		/// <summary>
		/// Test if the game is ended. Look for a winner.
		/// </summary>
		/// <param name="winner">winner color</param>
		/// <param name="winningPath">winners point array</param>
		/// <returns>true if the game is end</returns>
		public bool IsEnded(ref HexState winner)
		{
			// Check: no need, most of the time
			CheckEndOfGame();
			
			// Return winner
			winner = _winner;
			return _ended;
		}
		
		/// <summary>
		/// Test if the game board is empty.
		/// </summary>
		/// <returns>true if no hex is busy</returns>
		public bool IsEmpty()
		{
			return BusyHexCount() == 0;
		}
		
		/// <summary>
		/// Check for the end of the game
		/// </summary>
		private void CheckEndOfGame()
		{
			// Already a winner
			if (_ended)
				return;
			
			// Init winners array
			int length = (_size*_size)/2+1;
			Hex[] winners = new Hex[length];
			for (int i = 0 ; i < length ; i++)
				winners[i] = null;
			
			// Look for winners starting from border hex
			for (int i = 0 ; i < _size ; i++)
			{
				// Red win ?
				if (LookForWinningPath(HexState.Red, i, 0, 0, winners))
				{
					_winner = HexState.Red;
					_ended = true;
					break;
				}
				
				// Blue win ?
				else if (LookForWinningPath(HexState.Blue, 0, i, 0, winners))
				{
					_winner = HexState.Blue;
					_ended = true;
					break;
				}
			}
			
			// No winner
			if (!_ended)
				return;
			_currentPlayer = HexState.None;
			
			// Compute winner path
			int reallength;
			for (reallength = 0 ; winners[reallength] != null ; reallength++);
			_winningPath = new Gdk.Point[reallength];
			for (int i = 0 ; i < reallength ; i++)
				_winningPath[i] = winners[i].Middle;
		}
		
		/// <summary>
		/// Look recursively for a winner of a type
		/// </summary>
		/// <param name="type">color to explore</param>
		/// <param name="i">line</param>
		/// <param name="j">column</param>
		/// <param name="count">current index</param>
		/// <param name="winners">winner hex array</param>
		/// <returns>true if find</returns>
		private bool LookForWinningPath(HexState type, int i, int j, int count, Hex[] winners)
		{
			// Step 1: Test for unexisting hex
			if (i < 0 || j < 0 || i == _size || j == _size)
				return false;
			Hex current = _board[i,j];
			
			// Step 2: Check for already visited hex
			for (int k = 0 ; k < count ; k++)
			{
				if (current == winners[k])
					return false;
			}
			winners[count] = current;
			winners[count+1] = null;
				  
			// Step 3: Check the color 
			if (current.State != type)
				return false;
			
			// Step 4: Check if current hex touch border line
			if (type == HexState.Red && j == _size-1
			    || type == HexState.Blue && i == _size-1)
			{
				return true;
			}
			
			// Step 5: Continue on other hexs
			if ( LookForWinningPath(type, i+1, j, count+1, winners)
			    || LookForWinningPath(type, i-1, j, count+1, winners) 
			    || LookForWinningPath(type, i, j+1, count+1, winners)
			    || LookForWinningPath(type, i, j-1, count+1, winners)
			    || LookForWinningPath(type, i-1, j+1, count+1, winners)
			    || LookForWinningPath(type, i+1, j-1, count+1, winners))
			{
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Draw the board in the Graphic Context
		/// </summary>
		/// <param name="window">window to use</param>
		/// <param name="gc">context to use</param>
		public void Draw(Gdk.Window window, Gdk.GC gc)
		{
			// Draw all hex
			for (int i = 0 ; i < _size ; i++)
				for (int j = 0 ; j < _size ; j++)
					_board[i,j].Draw(window, gc);
			
			// Draw border lines
			gc.SetLineAttributes(4, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
			Gdk.Colormap colormap = Gdk.Colormap.System;
			Gdk.Color linecolor = Hex.BlueColor;
			colormap.AllocColor(ref linecolor, true, true);
			gc.Foreground = linecolor;
			window.DrawLines(gc, blueLeftLine);
			window.DrawLines(gc,  blueRightLine);			
			linecolor = Hex.RedColor;
			colormap.AllocColor(ref linecolor, true, true);
			gc.Foreground = linecolor;
			window.DrawLines(gc, redLeftLine);
			window.DrawLines(gc, redRightLine);
					
			// Draw winning path
			if (_ended)
			{
				Gdk.Color wincolor = Game.WinPathColor;
				colormap.AllocColor(ref wincolor, true, true);		
				gc.Foreground = wincolor;			
				window.DrawLines(gc, _winningPath);					
			}
			
			// Draw current player 
			if (_currentPlayer == HexState.Red)
			{
				// Create a red hex at left
				Hex hex = new Hex(new Gdk.Point(_board[0,0].MiddleLeftVertex.X, _board[_size-1,0].MiddleLeftVertex.Y));
				hex.State = HexState.Red;
				hex.Draw(window, gc);
				
				// Create a red hex at right
				hex = new Hex(new Gdk.Point(_board[_size-1,_size-1].MiddleLeftVertex.X, _board[0,_size-1].MiddleLeftVertex.Y));
				hex.State = HexState.Red;
				hex.Draw(window, gc);				
			}
			else if (_currentPlayer == HexState.Blue)
			{
				// Create a blue hex at left
				Hex hex = new Hex(new Gdk.Point(_board[0,0].MiddleLeftVertex.X, _board[0,_size-1].MiddleLeftVertex.Y));
				hex.State = HexState.Blue;
				hex.Draw(window, gc);
				
				// Create a blue hex at right
				hex = new Hex(new Gdk.Point(_board[_size-1,_size-1].MiddleLeftVertex.X, _board[_size-1,0].MiddleLeftVertex.Y));
				hex.State = HexState.Blue;
				hex.Draw(window, gc);					
			}
		}
		
		/// <summary>
		/// Save the game in a Stream.
		/// </summary>
		/// <param name="sw">stream to use</param>
		public void Save(Stream sw)
		{
			// Prepare to write
			BinaryWriter bw = new BinaryWriter(sw);
			
			// Write board
			bw.Write(_size);
			for (int i = 0 ; i < _size ; i++)
				for (int j = 0 ; j < _size ; j++)
					bw.Write((byte)_board[i,j].State);
			
			// Write properties
			bw.Write((byte)_redPlayerType);
			bw.Write((byte)_bluePlayerType);
			bw.Write((byte)_computerLevel);
			bw.Write((byte)_firstPlayer);
			
			// Write historic
			int count = _redHexCoord.Count;
			bw.Write(count);
			for (int i = 0 ; i < count ; i++)
				_redHexCoord[i].Save(bw);
			count = _blueHexCoord.Count;
			bw.Write(count);
			for (int i = 0 ; i < count ; i++)
				_blueHexCoord[i].Save(bw);
			
			// Write strategic context
			bw.Write((bool)(_redStrategy != null));
			if (_redStrategy != null)
				_redStrategy.Save(bw);
			bw.Write((bool)(_blueStrategy != null));
			if (_blueStrategy != null)
				_blueStrategy.Save(bw);
			
			// Write play context
			bw.Write((byte)_currentPlayer);
			bw.Write(_ended);
			bw.Write((byte)_winner);

			// End of write
			bw.Close();
		}
	}
}
