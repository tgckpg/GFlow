using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GFlow.Controls
{
	static class DrawLinq
	{
		public static Vector2 Move( this Vector2 v, float X, float Y ) => new Vector2( v.X + X, v.Y + Y );
		public static Vector2 Move( this Vector2 v, float XY ) => new Vector2( v.X + XY, v.Y + XY );
		public static Vector2 MoveX( this Vector2 v, float X ) => new Vector2( v.X + X, v.Y );
		public static Vector2 MoveY( this Vector2 v, float Y ) => new Vector2( v.X, v.Y + Y );

		public static Vector2 Set( this Vector2 v, float X, float Y ) => new Vector2( X, Y );
		public static Vector2 Set( this Vector2 v, float XY ) => new Vector2( XY, XY );
		public static Vector2 SetX( this Vector2 v, float X ) => new Vector2( X, v.Y );
		public static Vector2 SetY( this Vector2 v, float Y ) => new Vector2( v.X, Y );
	}
}