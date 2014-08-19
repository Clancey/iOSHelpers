using System;
using iOSHelpers;
using Foundation;

namespace UIKit
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
			TitleLabel.AddObserver (this,(NSString)"text", Foundation.NSKeyValueObservingOptions.Old | Foundation.NSKeyValueObservingOptions.New,IntPtr.Zero);
		}

		public nfloat BorderWidth
		{
			get{ return Layer.BorderWidth; }
			set{ Layer.BorderWidth = value; }
		}
		public nfloat CornerRadius
		{
			get{ return Layer.CornerRadius; }
			set{ Layer.CornerRadius = value; }
		}

		protected override void Dispose (bool disposing)
		{
			this.InvokeOnMainThread (() => TitleLabel.RemoveObserver (this, (NSString)"text"));
			base.Dispose (disposing);
		}
	
		public override void ObserveValue (Foundation.NSString keyPath, Foundation.NSObject ofObject, Foundation.NSDictionary change, IntPtr context)
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
		public override void TouchesBegan (Foundation.NSSet touches, UIEvent evt)
		{
			orgColor = this.TintColor;
			TintColor = SelectedTintColor;
			base.TouchesBegan (touches, evt);
		}
		public override void TouchesCancelled (Foundation.NSSet touches, UIEvent evt)
		{
			TintColor = orgColor;
			base.TouchesCancelled (touches, evt);
		}
		public override void TouchesEnded (Foundation.NSSet touches, UIEvent evt)
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

