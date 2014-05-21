using System;
using MonoTouch.UIKit;

namespace iOSHelpers
{
	public class UIBlurView : UIView
	{
		UIToolbar toolbar;
		public UIBlurView ()
		{
			BackgroundColor = UIColor.Clear;
			Add(toolbar = new UIToolbar {
				Translucent  =true,
			});
		}
		public UIBarStyle Style
		{
			get{ return toolbar.BarStyle; }
			set{ toolbar.BarStyle = value; }
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			toolbar.Frame = Bounds;
		}
	}
}

