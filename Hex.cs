using System;
using Gdk;

namespace RiverHex
{
	/// <summary>
	/// Current state value for hex
	/// </summary>
	public enum HexState { None, Red, Blue };
	
	/// <summary>
	/// Class for a Hexagon in the game
	/// </summary>
	public class Hex
	{
		/// <value>
		/// Hex size
		/// </value>
		internal static int Width = 30;
		internal static int Height = 40;

		/// <value>
		/// Hex color
		/// </value>
		internal static Gdk.Color GridColor = new Gdk.Color(0, 0, 0);
		internal static Gdk.Color RedColor = new Gdk.Color(0xff, 0, 0);
		internal static Gdk.Color BlueColor = new Gdk.Color(0, 0, 0xff);

		/// <value>
		/// State of the hex: free or used by a player
		/// </value>
		public HexState State
		{
			get { return _state; }
			set { _state = value; }
		}
		private HexState _state;

		/// <summary>
		/// Vertex for the hex
		///    1 ___ 2
		///   0 /   \ 3
		///   5 \___/ 4
		/// </summary>
		private Gdk.Point[] _vertex;
		
		/// <summary>
		/// Get a vertex position (X,Y)
		/// </summary>
		public Gdk.Point Position
		{
			get { return _vertex[0]; }
			set { ComputePosition(value); }
		}
		public Point MiddleLeftVertex
		{
			get { return _vertex[0]; }
		}
		public Point TopLeftVertex
		{
			get { return _vertex[1]; }
		}
		public Point TopRightVertex
		{
			get { return _vertex[2]; }
		}
		public Point MiddleRightVertex
		{
			get { return _vertex[3]; }
		}
		public Point BottomRightVertex
		{
			get { return _vertex[4]; }
		}
		public Point BottomLeftVertex
		{
			get { return _vertex[5]; }
		}
		public Point Middle
		{
			get { return new Gdk.Point(MiddleLeftVertex.X+Hex.Width,MiddleLeftVertex.Y); }
		}
		
		/// <summary>
		/// Constructor, create the hex
		/// </summary>
		/// <param name="position">init position</param>
		public Hex(Gdk.Point position)
		{
			// Initialize hex
			_state = HexState.None;
			_vertex = new Gdk.Point[6];
			
			// Compute position
			ComputePosition(position);
		}
		
		/// <summary>
		/// Compute position for hex
		/// </summary>
		/// <param name="position">position for vertex 0</param>
		private void ComputePosition(Gdk.Point position)
		{		
			// Compute vertex
			int width = Hex.Width;
			int miwidth = width/2;
			int height = Hex.Height;
			int miheight = height/2;
			_vertex[0] = position;
			_vertex[1] = new Gdk.Point(position.X+miwidth, position.Y-miheight);
			_vertex[2] = new Gdk.Point(_vertex[1].X+width, _vertex[1].Y);
			_vertex[3] = new Gdk.Point(_vertex[2].X+miwidth, _vertex[2].Y+miheight);
			_vertex[4] = new Gdk.Point(_vertex[2].X, _vertex[2].Y+height);
			_vertex[5] = new Gdk.Point(_vertex[1].X, _vertex[4].Y);			
		}
		
		/// <summary>
		/// Draw the hex in the Graphic Context
		/// </summary>
		/// <param name="window">window to use</param>
		/// <param name="gc">context to use</param>
		public void Draw(Gdk.Window window, Gdk.GC gc)
		{
			// Create point array
			Gdk.Point[] hex = new Gdk.Point[7];
			for (int i = 0; i < 6; i++)
				hex[i] = _vertex[i];
			hex[6] = _vertex[0];
			
			// Set line attribute
			gc.SetLineAttributes(3, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
			
			// Draw filled hex
			Gdk.Colormap colormap = Gdk.Colormap.System;
			if (_state != HexState.None)
			{
				Gdk.Color fillcolor = Hex.BlueColor;
				if (_state == HexState.Red)
					fillcolor = Hex.RedColor;
				colormap.AllocColor(ref fillcolor, true, true);		
				gc.Background = fillcolor;
				gc.Foreground = fillcolor;
				window.DrawPolygon(gc, true, hex);
			}
			
			// Draw grid
			Gdk.Color gridcolor = Hex.GridColor;
			colormap.AllocColor(ref gridcolor, true, true);		
			gc.Foreground = gridcolor;			
			window.DrawLines(gc, hex);		
		}
		
		/// <summary>
		/// Test if a point is in the hex.
		/// </summary>
		/// <param name="point">point to test</param>
		/// <returns>true if the point is in the hex</returns>
		public bool IsInHex(Gdk.Point point)
		{
			// Not in the containing rectangle
			if (point.X < MiddleLeftVertex.X || point.X > MiddleRightVertex.X
			    || point.Y < TopLeftVertex.Y || point.Y > BottomLeftVertex.Y )
				return false;
			
			// Test left and right hex side using line equation
			if (ComparePointToLine(point, MiddleLeftVertex, TopLeftVertex) >= 0
			    || ComparePointToLine(point, MiddleLeftVertex, BottomLeftVertex) <= 0
			    || ComparePointToLine(point, TopRightVertex, MiddleRightVertex) >= 0
			    || ComparePointToLine(point, MiddleRightVertex, BottomRightVertex) >= 0)
				return false;
			
			return true;
		}
		
		/// <summary>
		/// Compare a point to a line to find if the point is up or down the line.
		/// </summary>
		/// <param name="point">point to test</param>
		/// <param name="p0">start point for the line</param>
		/// <param name="p1">end point for the line</param>
		/// <returns>the line equation value using the point as X,Y</returns>
		private int ComparePointToLine(Gdk.Point point, Gdk.Point p0, Gdk.Point p1)
		{
			int a = p1.Y - p0.Y;
			int b = p0.X - p1.X;
			int c = -b * p0.Y - a*p0.X;
			return a*point.X+b*point.Y+c;
		}
				
	}
}
