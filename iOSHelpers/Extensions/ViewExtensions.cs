using System;
using UIKit;
using Foundation;
using CoreAnimation;
using ObjCRuntime;

namespace iOSHelpers
{
	public static class ViewExtensions
	{
//		static readonly IntPtr selAccessibilityIdentifier_Handle = Selector.GetHandle ("accessibilityIdentifier");
//		static readonly IntPtr setAccessibilityIdentifier_Handle = Selector.GetHandle ("setAccessibilityIdentifier:");
//		public static UIView SetAccessibilityId(this UIView view, string id)
//		{
//			var intPtr = NSString.CreateNative (id);
//			Messaging.void_objc_msgSend_IntPtr (view.Handle, setAccessibilityIdentifier_Handle, intPtr);
//			NSString.ReleaseNative (intPtr);
//			return view;
//		}	
//
//		public static string GetAccessibilityId(this UIView view)
//		{
//			return NSString.FromHandle (Messaging.IntPtr_objc_msgSend (view.Handle, selAccessibilityIdentifier_Handle));
//		}
//		public static UIBarButtonItem SetAccessibilityId(this UIBarButtonItem view, string id)
//		{
//			var nsId = NSString.CreateNative (id);
//			Messaging.void_objc_msgSend_IntPtr (view.Handle, setAccessibilityIdentifier_Handle, nsId);
//			NSString.ReleaseNative (nsId);
//			return view;
//		}


		public static UIView AddMotion(this UIView view, nfloat min, nfloat max)
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
		public static UIView Pulse (this UIView view,nfloat max)
		{
			var transformAnimation = CAKeyFrameAnimation.FromKeyPath("transform");	
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

