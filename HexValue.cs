using System;
using System.IO;


namespace RiverHex
{
	/// <summary>
	/// Class used to attribute a value to each playable hex
	/// </summary>
	public class HexValue : HexCoord
	{
		/// <summary>
		/// Value of the hex
		/// </summary>		
		public int value;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		/// <param name="val">value</param>
		public HexValue(int line, int column, int val) : base(line, column)
		{
			value = val;
		}	
		
		/// <summary>
		/// Save the coordinate into a stream.
		/// </summary>
		/// <param name="sw">stream to write</param>
		public override void Save(BinaryWriter sw)
		{
			sw.Write((byte)2);
			sw.Write(i);
			sw.Write(j);
			sw.Write(value);
		}
		
		/// <summary>
		/// Add a hex coordinate to an hex value. Need to transform relative coordinate to absolute coordinate. 
		/// </summary>
		/// <param name="first">first coordinate (usually base)</param>
		/// <param name="second">second coordinate (usually relative to the base)</param>
		/// <returns>a new coordinate where both lines and columns are added, value come from the hex value</returns>
		public static HexValue operator +(HexValue first, HexCoord second)
		{
			return new HexValue(first.i+second.i, first.j+second.j, first.value);
		}		
	}
	
	/// <summary>
	/// Structure to attribute a value to a distant hex coordinate. 
	/// A distant hex is an hex linkable to another hex with some preconditions.
	/// </summary>
	public class FarHexValue : HexValue
	{
		/// <summary>
		/// Preconditions: relative hexs coordinates which should stay free to keep the link
		/// </summary>			
		public HexCoord[] precond;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		/// <param name="val">value</param>	
		/// <param name="preconditions">preconditions</param>
		public FarHexValue(int line, int column, int value, HexCoord[] preconditions) : base(line, column, value)
		{
			precond = preconditions;
		}
		
		/// <summary>
		/// Save the coordinate into a stream.
		/// </summary>
		/// <param name="sw">stream to write</param>
		public override void Save(BinaryWriter sw)
		{
			sw.Write((byte)3);
			sw.Write(i);
			sw.Write(j);
			sw.Write(value);
			int len = precond.Length;
			sw.Write(len);
			for (int k = 0 ; k < len ; k++)
				precond[k].Save(sw);			
		}
		
		/// <summary>
		/// Add a hex coordinate to a far hex value. Need to transform relative coordinate to absolute coordinate. 
		/// </summary>
		/// <param name="first">first coordinate (usually base)</param>
		/// <param name="second">second coordinate (usually relative to the base)</param>
		/// <returns>a new coordinate where both lines and columns are added, both value 
		/// and preconditions comes from the far hex value</returns>
		public static FarHexValue operator +(FarHexValue first, HexCoord second)
		{
			return new FarHexValue(first.i+second.i, first.j+second.j, first.value, first.precond);
		}		
	}	
}
