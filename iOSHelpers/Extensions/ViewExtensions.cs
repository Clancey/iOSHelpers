using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;

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
		public static UIView Pulse (this UIView view,float max)
		{
			var transformAnimation = CAKeyFrameAnimation.GetFromKeyPath("transform");	
			transformAnimation.CalculationMode = CAAnimation.AnimationPaced;
			transformAnimation.FillMode = CAFillMode.Forwards;
			transformAnimation.TimingFunction = CAMediaTimingFunction.FromName(CAMediaTimingFunction.EaseOut);
			//			pathAnimation.RemovedOnCompletion = false;
			transformAnimation.Duration = .2;

			var transform = CATransform3D.MakeScale (max, max, 1);
			transformAnimation.Values = new [] {
				NSValue.FromCATransform3D(CATransform3D.Identity),
				NSValue.FromCATransform3D(transform),
				NSValue.FromCATransform3D(CATransform3D.Identity),
			};
			view.Layer.AddAnimation (transformAnimation, "pulse");
			return view;
		}

	}
}

