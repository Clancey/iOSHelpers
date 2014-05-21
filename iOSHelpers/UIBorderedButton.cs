using System;
using iOSHelpers;

namespace MonoTouch.UIKit
{
	public class UIBorderedButton : SimpleButton
	{
		public UIBorderedButton()
		{
			init ();
		}
		void init()
		{
			TitleColor = TintColor;
			BorderWidth = .5f;
			CornerRadius = 5f;
			TitleLabel.AddObserver (this,new MonoTouch.Foundation.NSString("text"), MonoTouch.Foundation.NSKeyValueObservingOptions.Old | MonoTouch.Foundation.NSKeyValueObservingOptions.New,IntPtr.Zero);
		}

		public float BorderWidth
		{
			get{ return Layer.BorderWidth; }
			set{ Layer.BorderWidth = value; }
		}
		public float CornerRadius
		{
			get{ return Layer.CornerRadius; }
			set{ Layer.CornerRadius = value; }
		}
	
		public override void ObserveValue (MonoTouch.Foundation.NSString keyPath, MonoTouch.Foundation.NSObject ofObject, MonoTouch.Foundation.NSDictionary change, IntPtr context)
		{
			if (ofObject == TitleLabel)
				SetColor ();
		}

		void SetColor()
		{
			Layer.BorderColor = CurrentTitleColor.CGColor;
		}

		public new UIColor TitleColor
		{
			get{ return TitleColor (UIControlState.Normal); }
			set{ 
				if (TintColor.Description != value.Description)
					TintColor = value;
				SetTitleColor (value, UIControlState.Normal);
			}
		}

		public UIColor SelectedTintColor {get;set;}
		UIColor orgColor;
		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			orgColor = this.TintColor;
			TintColor = SelectedTintColor;
			base.TouchesBegan (touches, evt);
		}
		public override void TouchesCancelled (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			TintColor = orgColor;
			base.TouchesCancelled (touches, evt);
		}
		public override void TouchesEnded (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			TintColor = orgColor;
			base.TouchesEnded (touches, evt);
		}
		public new UIImage Image
		{
			get { return BackgroundImageForState(UIControlState.Normal); }
			set
			{
				SetBackgroundImage(value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
				//SetBackgroundImage(value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Highlighted);
			}
		}

		public override void TintColorDidChange ()
		{
			base.TintColorDidChange ();
			TitleColor = TintColor;
			Layer.BorderColor = TintColor.CGColor;
		}

		public override void SizeToFit ()
		{
			base.SizeToFit ();
			var frame = Frame;
			frame.Width += 10;
			Frame = frame;
		}
	}
}

