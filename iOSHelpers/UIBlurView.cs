using System;
using UIKit;

namespace iOSHelpers
{
	public class UIBlurView : UIView
	{
		UIView blurView;
		public UIBlurView()
		{
		
			var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
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

