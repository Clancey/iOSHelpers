using System;
using UIKit;
using System.Drawing;
using System.Collections;

namespace iOSHelpers
{

	public class StackPanel : UIView , IEnumerable
	{
		int columns = 1;

		public int Columns {
			get{ return columns;}
			set {
				if (columns == value)
					return;
				columns = value;
				this.SetNeedsLayout();
			}
		}

		public StackPanel ()
		{
			init ();
		}

		public StackPanel (RectangleF rect) : base(rect)
		{
			init ();
		}

		void init ()
		{
			this.BackgroundColor = UIColor.Clear;
		}

		nfloat padding = 10;

		public nfloat Padding {
			get{ return padding;}
			set { 
				if (padding == value)
					return;
				padding = value;
				LayoutSubviews ();
			}
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var h = padding;
			var width = ((this.Bounds.Width - padding) / columns) - padding;
			nfloat columnH = 0;
			for (int i = 0; i < Subviews.Length; i++) {
				var col =  (i % columns);
				var view = Subviews[i];
				var frame = view.Frame;
				frame.X = padding + ((width + padding) * col);
				frame.Y = h;
				frame.Width = width;
				view.Frame = frame;

				columnH = NMath.Max(frame.Bottom + padding,columnH);
				if(col + 1 == columns)
					h = columnH;
			}
		}
	}
}

