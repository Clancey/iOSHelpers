using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace iOSHelpers
{
	public static class ViewExtensions
	{
		public static UIView AddMotion(this UIView view, float min, float max)
		{
			view.AddMotionEffect ( new UIInterpolatingMotionEffect ("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis) {
				MinimumRelativeValue = new NSNumber (min),
				MaximumRelativeValue = new NSNumber (max)
			});
			view.AddMotionEffect (new UIInterpolatingMotionEffect ("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis) {
				MinimumRelativeValue = new NSNumber (min),
				MaximumRelativeValue = new NSNumber (max)
			});
			return view;
		}

	}
}

