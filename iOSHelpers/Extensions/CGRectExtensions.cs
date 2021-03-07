using System;
using CoreGraphics;
using UIKit;

namespace UIKit
{
	public static  class CGRectExtensions
	{
		public static CGRect ApplyInset(this CGRect rect, UIEdgeInsets inset)
		{
			rect.X += inset.Left;
			rect.Y += inset.Top;
			rect.Height -= inset.Top + inset.Bottom;
			rect.Width -= inset.Left + inset.Right;
			return rect;
		}
	}
}
