using System;
using System.IO;
using System.Collections.Generic;

namespace RiverHex
{
	/// <summary>
	/// Class to hold the computer strategy of play.
	/// </summary>	
	public class ComputerStrategy
	{
		/// <summary>
		/// Current strategy
		/// </summary>
		public enum StrategicMode { Expand, Consolidate };
		
		/// <summary>
		/// Game
		/// </summary>
		private Game _game;
		
		/// <summary>
		/// Color for computer
		/// </summary>
		private HexState _color;
		
		/// <summary>
		///  Historic of play for the computer and opponent
		/// </summary>
		private List<HexCoord> _historic;
		public List<HexCoord> Historic
		{
			get { return _historic; }
		}		
		private List<HexCoord> _opponent;

		/// <summary>
		/// Current strategy
		/// </summary>
		private Game.Level _level;
		
		/// <summary>
		///  Current strategy
		/// </summary>		
		private StrategicMode _mode;
		
		/// <summary>
		///  Current direction of expansion
		/// </summary>		
		private int _currentDirection;
		
		// Debug
		private const bool IsDebug = false;
		
		// Playable hex value near the current one
		static internal HexValue[] RedNearHexValue = { new HexValue(0,-1,3), new HexValue(0,1,3), new HexValue(-1,0,2), new HexValue(1,0,2), new HexValue(-1,1,3), new HexValue(1,-1,3) };
		static internal HexValue[] BlueNearHexValue = { new HexValue(0,-1,2), new HexValue(0,1,2), new HexValue(-1,0,3), new HexValue(1,0,3), new HexValue(-1,1,3), new HexValue(1,-1,3) };
		
		// Playable hex value far of the current one
		static internal HexCoord[] _precond1 = { new HexCoord(0,-1), new HexCoord(1,-1)};
		static internal HexCoord[] _precond2 = { new HexCoord(1,-1), new HexCoord(1,0)};
		static internal HexCoord[] _precond3 = { new HexCoord(1,0), new HexCoord(0,1)};
		static internal HexCoord[] _precond4 = { new HexCoord(-1,1), new HexCoord(0,1)};
		static internal HexCoord[] _precond5 = { new HexCoord(-1,1), new HexCoord(-1,0)};
		static internal HexCoord[] _precond6 = { new HexCoord(-1,0), new HexCoord(0,-1)};
		static internal FarHexValue[] RedFarHexValue = {
		 	new FarHexValue(1,-2,6,_precond1),
			new FarHexValue(2,-1,4,_precond2),
			new FarHexValue(1,1,5,_precond3),
			new FarHexValue(-1,2,6,_precond4),
			new FarHexValue(-2,1,4,_precond5),
			new FarHexValue(-1,-1,5,_precond6)
		};
		static internal FarHexValue[] BlueFarHexValue = {
		 	new FarHexValue(1,-2,4,_precond1),
			new FarHexValue(2,-1,6,_precond2),
			new FarHexValue(1,1,5,_precond3),
			new FarHexValue(-1,2,4,_precond4),
			new FarHexValue(-2,1,6,_precond5),
			new FarHexValue(-1,-1,5,_precond6)
		}; 
		
		// Values for hex free around
		static internal int[] RedPlusAroundValues = {1,3,2,2,1,3};
		static internal int[] RedMoinsAroundValues = {3,1,2,2,3,1};
		static internal int[] BluePlusAroundValues = {2,1,1,3,3,2};
		static internal int[] BlueMoinsAroundValues = {2,3,3,1,1,2};		
		
		/// <summary>
		/// Create a strategy.
		/// </summary>
		/// <param name="game">Game</param>
		/// <param name="color">Color to use</param>
		/// <param name="level">Playing level</param>
		public ComputerStrategy(Game game, HexState color, Game.Level level)
		{	
			_game = game;
			_color = color;
			_level = level;
			if (_color == HexState.Red) {
				_historic = _game.RedHistoric;
				_opponent = _game.BlueHistoric;
			} else {
				_historic = _game.BlueHistoric;
				_opponent = _game.RedHistoric;
			};
			_mode = StrategicMode.Expand;
			_currentDirection = 0;
		}
		
		/// <summary>
		/// Private constructor.
		/// </summary>
		private ComputerStrategy()
		{
		}
		
		/// <summary>
		/// Create the strategy from a stream.
		/// </summary>
		/// <param name="game">game to use</param>
		/// <param name="br">stream to read</param>		
		public static ComputerStrategy CreateFromStream(Game game, BinaryReader br)
		{
			ComputerStrategy strategy = new ComputerStrategy();
			strategy._game = game;
			strategy._color = (HexState)br.ReadByte();
			strategy._level = (Game.Level)br.ReadByte();
			strategy._mode = (StrategicMode)br.ReadByte();
			strategy._currentDirection = br.ReadInt32();
			if (strategy._color == HexState.Red) {
				strategy._historic = game.RedHistoric;
				strategy._opponent = game.BlueHistoric;
			} else {
				strategy._historic = game.BlueHistoric;
				strategy._opponent = game.RedHistoric;
			};
			return strategy;
		}
		
		/// <summary>
		/// Compute a play.
		/// </summary>
		/// <returns>Hex coordoninate</returns>
		public HexCoord ComputePlay()
		{
			// Use a different strategy at first play
			HexCoord play;
			if (_historic.Count == 0)
			{
				play = FirstPlayStrategy();
			}
			
			// Other play
			else
			{
				play = GeneralStrategy();
			}			
			
			// HACK: avoid cheat, i.e. computer play in a non free hex !
			if (_game.Board(play.i,play.j) != HexState.None)
				throw new InvalidOperationException("Computer cheat !!!!");
			return play;
		}
	
		/// <summary>
		/// Compute the strategy for the first play.
		/// </summary>
		/// <returns>hex coordinate</returns>
		private HexCoord FirstPlayStrategy()
		{
			// Choose a strategy of expansion
			Random seed = new Random(DateTime.Now.Millisecond);
			int direction = seed.Next(2);
			if (direction == 0)
				_currentDirection = -1;
			else
				_currentDirection = 1;
			
			// Play in the middle diagonal
			HexCoord play = new HexCoord((_game.Size-1)/2, (_game.Size-1)/2);
			while (_game.Board(play.i,play.j) != HexState.None)
			{
				if (_color == HexState.Blue)
					play.j = play.j - 1 * _currentDirection;
				else
					play.i = play.i - 1 * _currentDirection;
			}
			DebugDecision(null, play);
			
			return play;
		}
		
		/// <summary>
		/// Compute the general strategy.
		/// </summary>
		/// <returns>hex coordinate</returns>	
		private HexCoord GeneralStrategy()
		{					
			// Protect previous far hex link
			HexCoord farlink = CheckFarHexLink();
			if (farlink != null)
				return farlink;
			
			// Once on target, change direction if need
			UpdateDirectionOfPlay();
			
			// For each previous hex
			int count = _historic.Count;
			List<HexValue> potential = new List<HexValue>();
			for (int i = 0 ; i < count ; i++)
			{
				// Compute values of playable hex from the last hex
				potential.AddRange(EvaluatePlayableHex(_historic[i]));
			}
			
			// Choose the playable hex with the best value
			count = potential.Count;
			if (count == 0)
				throw new NotImplementedException();
			HexValue decision = new HexValue(0, 0, -1);
			for (int i = 0 ; i < count ; i++)
				if (potential[i].value > decision.value)
					decision = potential[i];
			
			DebugDecision(potential, decision);
			return decision;
		}
		
		/// <summary>
		/// Check that far link are yet playable (not occupied by opponent).
		/// Force a play in it else.
		/// </summary>
		/// <returns>null if no far links are playable, the play to do else</returns>
		private HexCoord CheckFarHexLink()
		{
			// No need at easy level
			if (_level == Game.Level.Easy)
				return null;
			
			// For each previous play
			int count = _historic.Count;
			HexCoord linktodo = null;
			for (int i = 0 ; i < count ; i++)
			{
				// If it's a far hex
				HexCoord previous = _historic[i];
				if (previous.GetType() != typeof(FarHexValue))
					continue;

				// Compute preconditions state
				FarHexValue farhex = (FarHexValue)previous;
				HexCoord precond1 = farhex.precond[0];
				HexState color1 = _game.Board(precond1.i, precond1.j);
				HexCoord precond2 = farhex.precond[1];
				HexState color2 = _game.Board(precond2.i, precond2.j);
				
				// Check that precond are yet playable. Play it else or remind it if in Consolidate mode
				HexState opponent = (_color == HexState.Red ? HexState.Blue : HexState.Red);
				if (color1 == opponent && color2 == HexState.None) {
					DebugMessage("!Far link alert");
					DebugMessage(String.Format("->({0},{1}) ", precond2.i, precond2.j));
					return precond2;
				} else if (color2 == opponent && color1 == HexState.None) {
					DebugMessage("!Far link alert");
					DebugMessage(String.Format("->({0},{1}) ", precond1.i, precond1.j));					
					return precond1;
				} else if (color1 == HexState.None && color2 == HexState.None)
					linktodo = precond1;
			}
			
			// In ConsolidateMode, force link between far hex
			if (_mode == StrategicMode.Consolidate)
			{
				if (linktodo != null)
				{
					DebugMessage("!Consolidate far link");
					DebugMessage(String.Format("->({0},{1}) ", linktodo.i, linktodo.j));					
					return linktodo;
				}
			}
			
			return null;
		}
		
		/// <summary>
		/// Evaluate values for each playable hex around an hex using the right strategy.
		/// </summary>
		/// <param name="basehex">hex to evaluate</param>
		/// <returns>the list of playable hex with their values</returns>
		private List<HexValue> EvaluatePlayableHex(HexCoord basehex)
		{
			// Init values array
			HexValue[] NearHex = (_color == HexState.Red ? RedNearHexValue : BlueNearHexValue);
			FarHexValue[] FarHex = (_color == HexState.Red ? RedFarHexValue : BlueFarHexValue);
				
			// Construct a list of playable hex around the hex
			List<HexValue> potential = new List<HexValue>();
			int len = NearHex.Length;
			for (int i = 0 ; i < len ; i++)
			{
				HexValue test = NearHex[i] + basehex;
				if (_game.IsPlayable(test.i,test.j)) {
					potential.Add(test);
				}
			}
			
			// Add playable far hex
			if (_level != Game.Level.Easy)
			{
				len = FarHex.Length;
				for (int i = 0 ; i < len ; i++)
				{
					// Test if far hex is playable
					FarHexValue test = FarHex[i] + basehex;
					if (!_game.IsPlayable(test.i,test.j))
						continue;
				
					// Test if preconditions are playable too
					HexCoord precond1 = FarHex[i].precond[0] + basehex;
					HexCoord precond2 = FarHex[i].precond[1] + basehex;
					test.precond = new HexCoord[] { precond1, precond2 };
					if (_game.IsPlayable(precond1.i,precond1.j) && _game.IsPlayable(precond2.i,precond2.j))
						potential.Add(test);
				}
			}
			
			// Add to hex values the distance to the target line
			len = potential.Count;
			for (int i = 0 ; i < len ; i++)
			{
				HexValue current = potential[i];
				if (_color == HexState.Red) {
					current.value += (_currentDirection < 0 ? _game.Size-current.j : current.j);
					if ((current.j == 0 && _currentDirection < 0) || (current.j == _game.Size-1 && _currentDirection > 0))
					    current.value += 10;
				}
				else {
					current.value += (_currentDirection < 0 ? _game.Size-current.i : current.i);
					if ((current.i == 0 && _currentDirection < 0) || (current.i == _game.Size-1 && _currentDirection > 0))
					    current.value += 10;					
				}
			}

			// Add to hex values for free hex around
			if (_level == Game.Level.Hard)
			{
				len = potential.Count;
				for (int i = 0 ; i < len ; i++)
				{
					HexValue current = potential[i];
					if (_color == HexState.Red) {
						if (_currentDirection > 0)
							current.value += EvaluateFreeAround(current, RedPlusAroundValues, 1);
						else
							current.value += EvaluateFreeAround(current, RedMoinsAroundValues, -1);
					}
					else {
						if (_currentDirection > 0)
							current.value += EvaluateFreeAround(current, BluePlusAroundValues, 1);
						else
							current.value += EvaluateFreeAround(current, BlueMoinsAroundValues, -1);					
					}
				}
			}
			return potential;
		}
		
		/// <summary>
		///  Add values for free hex around.
		/// </summary>
		/// <param name="basehex">hex to test</param>
		/// <param name="hexvalues">list of values for each hex around</param>
		/// <param name="direction">current direction of play</param>
		/// <returns>total value to add</returns>
		private int EvaluateFreeAround(HexCoord basehex, int[] hexvalues, int direction)
		{
			// Init hex around coordinate array
			int finalvalue = 0;
			HexValue[] nearhex = RedNearHexValue; // HACK: use it only for coordinate of hex around (not values)
			int count = nearhex.Length; 
			
			// For each hex around
			for (int i = 0 ; i < count ; i++)
			{			
				int line = basehex.i+nearhex[i].i;
				int column = basehex.j+nearhex[i].j;
				if (_game.Board(line, column) == HexState.None) 
				{
					// Hex is free, add a value depending of color and direction
					finalvalue += hexvalues[i];
					
					// Add a bonus for target side
					if (_color == HexState.Red && ((column == 0 && direction < 0) || (column == _game.Size-1 && direction > 0)))
					    finalvalue += 6;
					else if (_color == HexState.Blue && ((line == 0 && direction < 0) || (line == _game.Size-1 && direction > 0)))
					    finalvalue += 6;	
				}
			}

			return finalvalue;
		}
		
		/// <summary>
		/// Update direction of expansion if need.
		/// </summary>
		private void UpdateDirectionOfPlay()
		{
			// Get last plays
			HexCoord last = _historic[_historic.Count-1];
			HexCoord opponent = _opponent[_opponent.Count-1];
			
			// Test if change is need because a side was touched
			bool needChange = false;
			if (_color == HexState.Red) {
				if ((_currentDirection < 0 && last.j == 0) || (_currentDirection > 0 && last.j == _game.Size-1))
					needChange = true;
			} else	{
				if ((_currentDirection < 0 && last.i == 0) || (_currentDirection > 0 && last.i == _game.Size-1))
					needChange = true;
			}
			
			// Change is need, do change and test if both side was touches
			if (needChange)
			{
				_currentDirection = -_currentDirection*2;
				if (Math.Abs(_currentDirection) == 4) {
					DebugMessage("! Both side touched, change mode");
					_mode = StrategicMode.Consolidate;
				}
				else {
					DebugMessage("! Side touch direction changed");
				}
			}
			
			// Adapt direction to opponent last play (only if no side touched yet)	
			else if (_level != Game.Level.Medium && Math.Abs(_currentDirection) < 2)
			{
				int coord = (_color == HexState.Red ? opponent.j : opponent.i);
				int middle = (_game.Size/2);
				if ((coord < middle && _currentDirection > 0)
				    || (coord > middle && _currentDirection < 0))
				{
					_currentDirection = -_currentDirection;
					DebugMessage("! Change direction to match opponent");
				}
			}
		}
		
		/// <summary>
		/// For debug, write to console the playable hex and the choice.
		/// </summary>
		/// <param name="playable">list of possible hexagone</param>
		/// <param name="choice">choosed hex</param>
		private void DebugDecision(List<HexValue> playable, HexCoord choice)
		{
			if (!IsDebug)
				return;
			if (playable != null)
			{
				int len = playable.Count;
				for (int i = 0 ; i < len ; i++)
					Console.Out.Write("({0},{1},{2}) ", playable[i].i, playable[i].j, playable[i].value);
				Console.Out.WriteLine();
			}
			Console.Out.WriteLine("->({0},{1}) ", choice.i, choice.j);
			Console.Out.Flush();
		}
		
		/// <summary>
		/// For debug, write a debug message to console.
		/// </summary>
		/// <param name="playable">list of possible hexagone</param>
		/// <param name="choice">choosed hex</param>
		private void DebugMessage(String message)
		{
			if (IsDebug)
				Console.Out.WriteLine(message);
		}
		
		/// <summary>
		/// Save the computer strategy in a Stream.
		/// </summary>
		/// <param name="sw">stream to use</param>
		public void Save(BinaryWriter sw)
		{	
			sw.Write((byte)_color);
			sw.Write((byte)_level);
			sw.Write((byte)_mode);
			sw.Write(_currentDirection);
		}		
	}
}
