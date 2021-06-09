using System;
using UIKit;

namespace iOSHelpers
{
	public class UIBlurView : UIView
	{
		UIView blurView;

		public UIBlurView() : this (UIBlurEffectStyle.Dark)
		{
		}
		public UIBlurView(UIBlurEffectStyle style)
		{
		
			var blur = UIBlurEffect.FromStyle(style);
			blurView = new UIVisualEffectView(blur);
			
			Add(blurView);
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (blurView != null)
				blurView.Frame = Bounds;
		}
	}
}

