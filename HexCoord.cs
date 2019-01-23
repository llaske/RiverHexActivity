using System;
using System.IO;


namespace RiverHex
{
	/// <summary>
	/// Class for an hex coordinate
	/// </summary>
	public class HexCoord
	{
		/// <summary>
		///  Line: count from 0 to the left side of the board
		/// </summary>
		public int i;
		
		/// <summary>
		///  Column: count from 0 to the upper side of the board
		/// </summary>		
		public int j;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="line">line</param>
		/// <param name="column">column</param>
		public HexCoord(int line, int column)
		{
			i = line;
			j = column;
		}

		/// <summary>
		/// Create the coordinate from a stream. Could create derivated class (HexValue, FarHexValue) also.
		/// </summary>
		/// <param name="br">stream to read</param>		
		public static HexCoord CreateFromStream(BinaryReader br)
		{
			// Read type
			byte type = br.ReadByte();
			
			// Read an hex coord
			int line = br.ReadInt32();
			int column = br.ReadInt32();			
			if (type == 1)
				return new HexCoord(line, column);
			
			// Read an hex value
			int value = br.ReadInt32();
			if (type == 2)
				return new HexValue(line, column, value);

			// Unknow type
			if (type != 3)
				throw new NotImplementedException("Not implemented hex type");
			
			// Read a far hex value
			int len = br.ReadInt32();
			HexCoord[] precond = new HexCoord[len];
			for (int k = 0 ; k < len ; k++)
				precond[k] = HexCoord.CreateFromStream(br);
			
			return new FarHexValue(line, column, value, precond);
		}
		
		/// <summary>
		/// Save the coordinate into a stream.
		/// </summary>
		/// <param name="sw">stream to write</param>
		public virtual void Save(BinaryWriter sw)
		{
			sw.Write((byte)1);
			sw.Write(i);
			sw.Write(j);
		}
		                         
		/// <summary>
		/// Add two hex coordinate. Need to transform relative coordinate to absolute coordinate.
		/// </summary>
		/// <param name="first">first coordinate (usually base)</param>
		/// <param name="second">second coordinate (usually relative to the base)</param>
		/// <returns>a new coordinate where both lines and columns are added</returns>
		public static HexCoord operator +(HexCoord first, HexCoord second)
		{
			return new HexCoord(first.i+second.i, first.j+second.j);
		}
	}
}
