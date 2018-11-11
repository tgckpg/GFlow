using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GFlow.Controls.GraphElements
{
	using BasicElements;
	using EventsArgs;

	class GFTextButton : GFButton
	{
		public CanvasTextFormat LabelFormat { get; set; } = new CanvasTextFormat() { FontSize = 18 };

		private Color IdleBgFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
		private Color IdleFgFill;

		private Color ActiveBgFill = Color.FromArgb( 0xFF, 0xD0, 0xD0, 0xD0 );
		private Color ActiveFgFill;

		protected string _Label = "Text Label";
		public string Label
		{
			get { return LabelOwner?.Label ?? _Label; }
			set { _Label = value; }
		}

		private IGFLabelOwner LabelOwner;

		public GFTextButton()
			:base()
		{
			BgFill = IdleBgFill;
			IdleFgFill = ActiveFgFill = FgFill;
			MouseOver = _MouseOver;
			MouseOut = _MouseOut;
		}

		private static void _MouseOver( object sender, GFPointerEventArgs e )
		{
			GFTextButton Target = ( GFTextButton ) e.Target;
			Target.BgFill = Target.ActiveBgFill;
			Target.FgFill = Target.ActiveFgFill;
		}

		private static void _MouseOut( object sender, GFPointerEventArgs e )
		{
			GFTextButton Target = ( GFTextButton ) e.Target;
			Target.BgFill = Target.IdleBgFill;
			Target.FgFill = Target.IdleFgFill;
		}

		public void SetLabelOwner( IGFLabelOwner LabelOwner )
		{
			this.LabelOwner = LabelOwner;
		}

		public override void Draw( CanvasDrawingSession ds, GFElement Parent, GFElement Prev )
		{
			base.Draw( ds, Parent, Prev );

			CanvasTextLayout TL = new CanvasTextLayout( ds, Label, LabelFormat, Bounds.W, Bounds.H );
			ds.DrawTextLayout( TL, ActualBounds.X + Padding.Left, ActualBounds.Y + Padding.Top, FgFill );
		}

		public void SetDarkTheme( uint C )
		{
			byte A = ( byte ) ( C >> 24 & 0xFF );
			byte R = ( byte ) ( C >> 16 & 0xFF );
			byte G = ( byte ) ( C >> 8 & 0xFF );
			byte B = ( byte ) ( C & 0xFF );

			IdleBgFill = Color.FromArgb( A, R, G, B );
			ActiveBgFill = Color.FromArgb( A, ( byte ) ( R + 10 ), ( byte ) ( G + 10 ), ( byte ) ( B + 10 ) );

			IdleFgFill = Colors.White;
			ActiveFgFill = Colors.White;

			BgFill = IdleBgFill;
			FgFill = IdleFgFill;
		}

		public void SetLightTheme( uint C )
		{
			byte A = ( byte ) ( C >> 24 & 0xFF );
			byte R = ( byte ) ( C >> 16 & 0xFF );
			byte G = ( byte ) ( C >> 8 & 0xFF );
			byte B = ( byte ) ( C & 0xFF );

			IdleBgFill = Color.FromArgb( A, R, G, B );
			ActiveBgFill = Color.FromArgb( A, ( byte ) ( R - 10 ), ( byte ) ( G - 10 ), ( byte ) ( B - 10 ) );

			IdleFgFill = Colors.Black;
			ActiveFgFill = Colors.Black;

			BgFill = IdleBgFill;
			FgFill = IdleFgFill;
		}

		public void SetLightThemeAlt( uint C )
		{
			byte A = ( byte ) ( C >> 24 & 0xFF );
			byte R = ( byte ) ( C >> 16 & 0xFF );
			byte G = ( byte ) ( C >> 8 & 0xFF );
			byte B = ( byte ) ( C & 0xFF );

			IdleBgFill = Color.FromArgb( 0xFF, 0xF0, 0xF0, 0xF0 );
			IdleFgFill = Colors.Black;

			ActiveBgFill = Color.FromArgb( A, R, G, B );
			ActiveFgFill = Colors.White;

			BgFill = IdleBgFill;
			FgFill = IdleFgFill;
		}
	}
}