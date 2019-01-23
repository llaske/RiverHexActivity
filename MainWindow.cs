using Gtk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sugar;
using Mono.Unix;

namespace RiverHex
{
	/// <summary>
	/// Main Window for RiverHex Activity
	/// </summary>
	public class MainWindow : Sugar.Window
	{	
		/// <summary>
		/// Activity, Bundle Id and Object Id 
		/// </summary>
		public new static string activityId="";
		public new static string bundleId="";
		public static string objectId="";

		/// <summary>
		/// Default game size
		/// </summary>
		public const int DefaultGameSize = 7;
		
		/// <summary>
		/// Default player type
		/// </summary>
		public const Game.PlayerType DefaultRedType = Game.PlayerType.Human;
		public const Game.PlayerType DefaultBlueType = Game.PlayerType.Computer;
		public const HexState DefaultFirstPlayer = HexState.Red;
		public const Game.Level DefautComputerLevel = Game.Level.Easy;
		
		/// <summary>
		/// Drawing Area
		/// </summary>
		private DrawingArea _drawing;

		/// <summary>
		/// Toolbar buttons
		/// </summary>		
		private ToolButton _hex7x7Button;
		private ToolButton _hex9x9Button;
		private ToolButton _hex11x11Button;
		private ToolButton _1playerButton;
		private ToolButton _2playerButton;		
		
		/// <summary>
		/// Board Game
		/// </summary>
		private Game _game;
		
		/// <summary>
		/// Constructor, Create the window
		/// </summary>
		/// <param name="activityId">activity id</param>
		/// <param name="bundleId">bundle id</param>
		/// <param name="objectId">object id</param>
		public MainWindow(string activityId, string bundleId, string objectId) : base("RiverHex",activityId, bundleId)
		{
			// Initialize variables
			_game = LoadFromJournal();
			
			// Set Icon window
			this.Icon=new Gdk.Pixbuf(null,"RiverHex.png");
			this.SetDefaultSize(400, 400);
			this.ModifyBg(Gtk.StateType.Normal, new Gdk.Color(0, 0, 0));
			this.Maximize();
			this.DeleteEvent += new DeleteEventHandler(OnMainWindowDelete);

			// Create VBox to contain toolbar and game board
			VBox vbox=new VBox();
			vbox.BorderWidth = 8;
		
			// Create Icon for Toolbar
			IconFactory iconFactory = new IconFactory();
			AddIcon(iconFactory, "hex7x7", "hex7x7.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "hex7x7_r", "hex7x7_r.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "hex9x9", "hex9x9.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "hex9x9_r", "hex9x9_r.png");
			iconFactory.AddDefault ();			
			AddIcon(iconFactory, "hex11x11", "hex11x11.png");
			iconFactory.AddDefault ();			
			AddIcon(iconFactory, "hex11x11_r", "hex11x11_r.png");
			iconFactory.AddDefault ();			
			AddIcon(iconFactory, "1playerL1", "1playerL1.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "1playerL1_r", "1playerL1_r.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "1playerL2", "1playerL2.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "1playerL2_r", "1playerL2_r.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "1playerL3", "1playerL3.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "1playerL3_r", "1playerL3_r.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "2player", "2player.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "2player_r", "2player_r.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "reverse", "reverse.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "tojournal", "tojournal.png");
			iconFactory.AddDefault ();
			AddIcon(iconFactory, "quit", "quit.png");
			iconFactory.AddDefault ();
			
			// Create Toolbar
			Gtk.Toolbar toolbar=new Gtk.Toolbar();
			toolbar.BorderWidth=2;
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
			toolbar.IconSize = IconSize.Dialog;
			toolbar.IconSizeSet = true; 		
			ToolButton tbutton = _hex7x7Button = new ToolButton("hex7x7");
			tbutton.TooltipText = Catalog.GetString("Game 7x7");
			tbutton.Clicked += new EventHandler(Button7x7Clicked);
			toolbar.Insert(tbutton, -1);
			tbutton = _hex9x9Button = new ToolButton("hex9x9");
			tbutton.TooltipText = Catalog.GetString("Game 9x9");
			tbutton.Clicked += new EventHandler(Button9x9Clicked);
			toolbar.Insert (tbutton, -1);
			tbutton = _hex11x11Button = new ToolButton("hex11x11");
			tbutton.TooltipText = Catalog.GetString("Game 11x11");
			tbutton.Clicked += new EventHandler(Button11x11Clicked);
			toolbar.Insert(tbutton, -1);
			toolbar.Insert(new Gtk.SeparatorToolItem(), -1);	
			tbutton = _1playerButton = new ToolButton("1playerL1");
			tbutton.TooltipText = Catalog.GetString("1 player game - click to change level");
			tbutton.Clicked += new EventHandler(Button1PlayerClicked);
			toolbar.Insert(tbutton, -1);
			tbutton = _2playerButton = new ToolButton("2player");
			tbutton.TooltipText = Catalog.GetString("2 players game");
			tbutton.Clicked += new EventHandler(Button2PlayerClicked);
			toolbar.Insert(tbutton, -1);
			tbutton = new ToolButton("reverse");
			tbutton.TooltipText = Catalog.GetString("Exchange color");
			tbutton.Clicked += new EventHandler(ButtonReverseClicked);
			toolbar.Insert(tbutton, -1);
			toolbar.Insert(new Gtk.SeparatorToolItem(), -1);
			tbutton = new ToolButton("tojournal");
			tbutton.TooltipText = Catalog.GetString("Save to journal");
			tbutton.Clicked += new EventHandler(SaveToJournalClicked);
			toolbar.Insert(tbutton, -1);			
			tbutton = new ToolButton("quit");
			tbutton.TooltipText = Catalog.GetString("Quit activity");
			tbutton.Clicked += new EventHandler(QuitButtonClicked);
			toolbar.Insert(tbutton, -1);
			toolbar.FocusChild = tbutton; // Remove focus !
			vbox.PackStart(toolbar,false,false,0);
		
			// Add drawing area
			_drawing = new DrawingArea();	
			_drawing.Events |= Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask;
			_drawing.ExposeEvent += OnDrawingExpose;
			_drawing.SizeAllocated += OnDrawingSizeAllocated;
			_drawing.ButtonReleaseEvent += OnDrawingButtonRelease;
			vbox.Add(_drawing);		

			// Add the vbox to the main window
			this.Add(vbox);
			
			// Draw
			ShowAll();
			if (_game != null)
				UpdateToolbar();
		}
	
		/// <summary>
		/// Raised when the window is deleted
		/// </summary>
		/// <param name="sender">sender</param>
		/// <param name="a">event</param>
		void OnMainWindowDelete (object sender, DeleteEventArgs a)
		{
			Application.Quit();
			a.RetVal = true;
		}
	
		/// <summary>
		/// Event raised when Drawing size changed
		/// </summary>
		/// <param name="sender">sender</param>
		/// <param name="args">args</param>
		void OnDrawingSizeAllocated(object sender, SizeAllocatedArgs args)
		{			
			// Create game
			if (_game == null) {
				_game = new Game(DefaultGameSize, DefaultRedType, DefaultBlueType, DefautComputerLevel, DefaultFirstPlayer, args.Allocation);
				UpdateToolbar();
			}
			else
				_game.Resize(args.Allocation);
		}
		
		/// <summary>
		/// Raised when the drawing area need paint
		/// </summary>
		/// <param name="sender">sender</param>
		/// <param name="args">event</param>
		void OnDrawingExpose(object sender, ExposeEventArgs args)
		{
			// Create GC
			Gdk.EventExpose ev = args.Event;
			Gdk.Window window = ev.Window;
			Gdk.GC gc = new Gdk.GC(window);
			
			// Draw Board
			_game.Draw(window, gc);
		}

		/// <summary>
		/// Event raised when a mouse button is clicked in the drawing area.
		/// </summary>
		/// <param name="o">sender</param>
		/// <param name="args">args</param>
		public void OnDrawingButtonRelease(object o, ButtonReleaseEventArgs args)
		{
			// Click with left button to set an hex
			if (args.Event.Button == 1)
			{
				// Find hex at position
				int line = -1;
				int column = -1;
				if (_game.FindHex(new Gdk.Point((int)args.Event.X, (int)args.Event.Y), ref line, ref column))
				{
					// Process human play
					if (_game.HumanPlay(line, column))
					{
						// Redraw board
						QueueDraw();
					
						// Play for computer if need
						if (_game.GetPlayerType(_game.CurrentPlayer) == Game.PlayerType.Computer)
							_game.ComputerPlay();
						
						// Redraw again
						QueueDraw();
					}
				}
			}
				
		}
		
		/// <summary>
		/// Entry point
		/// </summary>
		/// <param name="args">params</param>
		public static void Main(string[] args)
		{
			Catalog.Init("org.olpcfrance.RiverHexActivity", "locale/");
			
			// Debug only
			System.Console.Out.WriteLine(Catalog.GetString("RiverHex Activity for OLPC"));
			
			// Need to process Id args
			if (args.Length>0) {
				IEnumerator en= args.GetEnumerator();
				while (en.MoveNext()) {
					if (en.Current.ToString().Equals("-sugarActivityId")) {
						if (en.MoveNext()) {
							activityId=en.Current.ToString();
						}
					}
					else if (en.Current.ToString().Equals("-sugarBundleId")) {
						if (en.MoveNext()) {
							bundleId=en.Current.ToString();
						}
					}
					else if (en.Current.ToString().Equals("-objectId")) {
						if (en.MoveNext()) {
							objectId=en.Current.ToString();
						}
					}

				}
			}		
			
			// Launch activity
			Application.Init();
			new MainWindow(activityId, bundleId, objectId);
			Application.Run();
		}
	
		/// <summary>
		///  Add an icon to the factory used for toolbar
		/// </summary>
		/// <param name="stock">toolbar stock</param>
		/// <param name="stockid">id for new item</param>
		/// <param name="resource">resource name for new item<</param>
		private void AddIcon (IconFactory stock, string stockid, string resource)
		{
			Gtk.IconSet iconset = stock.Lookup(stockid);
		
			if (iconset != null)
				return;

			iconset = new Gtk.IconSet();
			Gdk.Pixbuf img = Gdk.Pixbuf.LoadFromResource(resource);
			IconSource source = new IconSource();
			source.Pixbuf = img;
			iconset.AddSource(source);
			stock.Add(stockid, iconset);		
		}	
	
		/// <summary>
		/// Raised when button Quit is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void QuitButtonClicked(object o, EventArgs args)
		{
			SaveToJournal(_game);
			Application.Quit();
		}
	
		/// <summary>
		/// Raised when 7x7 button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void Button7x7Clicked(object o, EventArgs args)
		{
			// Change size only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// Create game
			    _game = new Game(7, _game.RedPlayerType, _game.BluePlayerType, _game.ComputerLevel, _game.FirstPlayer, _drawing.Allocation);
				
				// Play for computer if need
				if (_game.GetPlayerType(_game.CurrentPlayer) == Game.PlayerType.Computer)
					_game.ComputerPlay();
				
				// Redraw
				QueueDraw();
				UpdateToolbar();
			}		
		}		
	
		/// <summary>
		/// Raised when 9x9 button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void Button9x9Clicked(object o, EventArgs args)
		{
			// Change size only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// Create game
			    _game = new Game(9, _game.RedPlayerType, _game.BluePlayerType, _game.ComputerLevel, _game.FirstPlayer, _drawing.Allocation);
				
				// Play for computer if need
				if (_game.GetPlayerType(_game.CurrentPlayer) == Game.PlayerType.Computer)
					_game.ComputerPlay();
				
				// Redraw
				QueueDraw();
				UpdateToolbar();
			}
		}		
	
		/// <summary>
		/// Raised when 11x11 button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void Button11x11Clicked(object o, EventArgs args)
		{
			// Change size only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// Create game
			    _game = new Game(11, _game.RedPlayerType, _game.BluePlayerType, _game.ComputerLevel, _game.FirstPlayer, _drawing.Allocation);
				
				// Play for computer if need
				if (_game.GetPlayerType(_game.CurrentPlayer) == Game.PlayerType.Computer)
					_game.ComputerPlay();
				
				// Redraw
				QueueDraw();
				UpdateToolbar();
			}
		}
	
		/// <summary>
		/// Raised when 1 player button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void Button1PlayerClicked(object o, EventArgs args)
		{	
			// Change players type only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// If click multiple time on the "1 player" button, change level
				Game.Level level = _game.ComputerLevel;
				if (_game.RedPlayerType == Game.PlayerType.Computer || _game.BluePlayerType == Game.PlayerType.Computer)
				{
					if (level == Game.Level.Easy)
						level = Game.Level.Medium;
					else if (level == Game.Level.Medium)
						level = Game.Level.Hard;
					else if (level == Game.Level.Hard)
						level = Game.Level.Easy;
				}

				// Create game
			    _game = new Game(_game.Size, Game.PlayerType.Human, Game.PlayerType.Computer, level, _game.FirstPlayer, _drawing.Allocation);				
				
				// Redraw
				QueueDraw();			
				UpdateToolbar();
			}				
		}
	
		/// <summary>
		/// Raised when 2 players button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void Button2PlayerClicked(object o, EventArgs args)
		{
			// Change players type only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// Create game
			    _game = new Game(_game.Size, Game.PlayerType.Human, Game.PlayerType.Human, _game.ComputerLevel, _game.FirstPlayer, _drawing.Allocation);
				
				// Redraw
				QueueDraw();
				UpdateToolbar();
			}			
		}
		
	
		/// <summary>
		/// Raised when reverse button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void ButtonReverseClicked(object o, EventArgs args)
		{
			// Reverse player color only at the end of the game or when the board is empty
			HexState winner = HexState.None;			
			if (_game.IsEmpty() || _game.IsEnded(ref winner))
			{
				// Change type
				Game.PlayerType redPlayer = _game.RedPlayerType;
				Game.PlayerType bluePlayer = _game.BluePlayerType;
				
				// If two player reverse, else just let the computer play
				if (redPlayer == Game.PlayerType.Human && bluePlayer == Game.PlayerType.Human)
				{
					HexState color = (_game.FirstPlayer == HexState.Red ? HexState.Blue : HexState.Red);
					_game = new Game(_game.Size, redPlayer, bluePlayer, _game.ComputerLevel, color, _drawing.Allocation);
				}
				else
					_game = new Game(_game.Size, bluePlayer, redPlayer, _game.ComputerLevel, _game.FirstPlayer, _drawing.Allocation);
				
				// Play for computer if need
				if (_game.GetPlayerType(_game.CurrentPlayer) == Game.PlayerType.Computer)
					_game.ComputerPlay();
				
				// Redraw
				QueueDraw();
				UpdateToolbar();
			}			
		}
	
		/// <summary>
		/// Raised when save to journal button is clicked
		/// </summary>
		/// <param name="o">object</param>
		/// <param name="args">args</param>
		public void SaveToJournalClicked(object o, EventArgs args)
		{
			SaveToJournal(_game);
		}
		
		/// <summary>
		/// Update Toolbar button icons
		/// </summary>
		private void UpdateToolbar()
		{
			_hex7x7Button.StockId = (_game.Size == 7 ? "hex7x7_r" : "hex7x7");
			_hex9x9Button.StockId = (_game.Size == 9 ? "hex9x9_r" : "hex9x9");
			_hex11x11Button.StockId = (_game.Size == 11 ? "hex11x11_r" : "hex11x11");
			bool onePlayer = (_game.RedPlayerType == Game.PlayerType.Computer || _game.BluePlayerType == Game.PlayerType.Computer);
			int level = 0;
			switch(_game.ComputerLevel) {
			case Game.Level.Easy:
				level = 1;
				break;
			case Game.Level.Medium:
				level = 2;
				break;
			case Game.Level.Hard:
				level = 3;
				break;
			}
			_1playerButton.StockId = String.Format("1playerL{0}{1}", level, (onePlayer?"_r":""));
			_2playerButton.StockId = String.Format("2player{0}", (!onePlayer?"_r":""));
		}
		
		/// <summary>
		/// Save information in journal
		/// </summary>
		/// <param name="toSave">game to save</param>
		void SaveToJournal(Game toSave)
		{
			// Compute directory
			String tmpDir=System.Environment.GetEnvironmentVariable("SUGAR_ACTIVITY_ROOT");
			if (tmpDir==null)
				tmpDir = "./";
			else
				tmpDir += "/instance";

			// Save board in a file
			UnixFileInfo t = new UnixFileInfo(tmpDir+"/data.rvx");
			StreamWriter sw = new StreamWriter(t.FullName); 
			toSave.Save(sw.BaseStream);
			sw.Close();
			
			// Save info in data store
			DSObject dsobject=Datastore.Create();
			dsobject.Metadata.setItem("title", Catalog.GetString("RiverHex Activity"));
			dsobject.Metadata.setItem("activity", "org.olpcfrance.RiverHexActivity");
			dsobject.Metadata.setItem("mime_type", "application/riverhexactivity");
			byte[] preview=this.getScreenShot();
			dsobject.Metadata.setItem("preview", preview);	
			dsobject.FilePath=t.FullName;
			Datastore.write(dsobject);
		}

		/// <summary>
		/// Load board from Journal
		/// </summary>
		Game LoadFromJournal()
		{
			// Nothing to read
			if (objectId == null || objectId.Length == 0)
				return null;
			
			// Read file
			DSObject result = Datastore.get(objectId);
			StreamReader sr = new StreamReader(result.FilePath);
			Game currentGame = Game.CreateFromStream(sr.BaseStream);
			sr.Close();
			
			return currentGame;
		}		
	}
}
