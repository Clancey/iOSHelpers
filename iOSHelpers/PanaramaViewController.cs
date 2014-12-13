using System;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;

namespace iOSHelpers
{
	public class PanaramaViewController : UIViewController
	{
		UIViewController[] viewControllers = new UIViewController[0];

		public UIViewController[] ViewControllers {
			get {
				return viewControllers;
			}
			set {
				if (view != null)
					view.Reset ();
				viewControllers = value;
				if (view != null)
					view.SetContent ();
			}
		}

		public PanaramaViewController ()
		{
			init ();
		}

		void init ()
		{
			HeaderHeight = 40;
		}

		nfloat topOffset;

		public nfloat TopOffset {
			get {
				return topOffset;
			}
			set {
				if (topOffset == value)
					return;
				topOffset = value;
				if (view != null)
					view.SetNeedsLayout ();
			}
		}

		nfloat headerHeight;

		public nfloat HeaderHeight {
			get {
				return headerHeight;
			}
			set {
				if (headerHeight == value)
					return;
				headerHeight = value;
				if (view != null)
					view.SetNeedsLayout ();
			}
		}

		UIFont titleFont;

		public UIFont TitleFont {
			get {
				return titleFont;
			}
			set {
				titleFont = value;
				if (view != null)
					view.UpdateButtons ();
			}
		}

		UIColor titleColor;

		public UIColor TitleColor {
			get {
				return titleColor;
			}
			set {
				titleColor = value;
				if (view != null)
					view.UpdateButtons ();
			}
		}

		PanaramaView view;

		public override void LoadView ()
		{
			View = view = new PanaramaView (this);
			if (viewControllers.Length > 0) {
				view.SetContent ();
			}
		}

		public class PanaramaView : UIView
		{
			readonly CustomScroller scroller;
			List<UIButton> Buttons = new List<UIButton> ();
			List<UIView> Views = new List<UIView> ();
			WeakReference _parent;

			PanaramaViewController Parent {
				get{ return _parent == null ? null : _parent.Target as PanaramaViewController; }
				set { _parent = new WeakReference (value); }
			}

			public PanaramaView (PanaramaViewController viewController)
			{
				Parent = viewController;
				scroller = new CustomScroller () {
					ScrollsToTop = false,
					PagingEnabled = true,
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
				scroller.Scrolled += (sender, args) => {
					var scroll = sender as CustomScroller;
					var p = scroll.Superview as PanaramaView;
					p.SetTopScroll ();
				};
				scroller.DecelerationEnded += (sender, args) => {
					var scroll = sender as CustomScroller;
					var p = scroll.Superview as PanaramaView;
					p.SetTopScroll ();
				};
				Add (scroller);
			}

			void SetScrollToTop ()
			{
				foreach (var vc in Parent.ViewControllers.OfType<UITableViewController>()) {
					SetScrollToTop (vc);
				}
			}

			void SetScrollToTop (UITableViewController tvc)
			{
				if (tvc == null)
					return;
				var offset = scroller.ContentOffset;
				offset.Y += Parent.TopOffset;
				var enabled = tvc.View.Frame.Contains (offset);
				tvc.TableView.ScrollsToTop = enabled;
			}

			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var frame = Bounds;
				frame.Height -= Parent.TopOffset;
				frame.Y = Parent.TopOffset;
				scroller.Frame = frame;
				SetScrollContent ();
			}

			nfloat scrollWidth;

			void SetScrollContent ()
			{
				var bounds = scroller.Bounds;
				bounds.Height -= Parent.HeaderHeight;
				bounds.Y = Parent.HeaderHeight;
				bounds.X = 0;
				scrollWidth = bounds.Width;
				foreach (var view in Views) {
					view.Frame = bounds;
					bounds.X += scrollWidth;
				}

				scroller.ContentSize = new CGSize (bounds.X, bounds.Height);
				SetTopScroll ();

			}

			void SetTopScroll ()
			{
				var half = scrollWidth / 2;
				var halfOffset = scroller.ContentOffset.X / 2;
				var labelCenter = new CGPoint (half + halfOffset, Parent.HeaderHeight / 2f);

				foreach (var button in Buttons) {
					button.Center = labelCenter;
					labelCenter.X += half;
				}

			}

			public void ScrollTo (int index)
			{
				var view = Views [index];
				scroller.ScrollRectToVisible (view.Frame, true);
			}

			public void Reset ()
			{
				Buttons.Clear ();
				Views.Clear ();
				scroller.Subviews.ForEach (x => x.RemoveFromSuperview ());
				if (Parent != null)
					Parent.ViewControllers.ForEach (x => x.RemoveFromParentViewController ());
			}

			public void SetContent ()
			{
				if (Parent != null) {
					nint tag = 0;
					Parent.ViewControllers.ForEach (x => {
						var button = new SimpleButton {
							Title = x.Title,
							Tag = tag++,
							Tapped = (b) => {
								var s = b.Superview as CustomScroller;
								var p = s.Superview as PanaramaView;
								p.ScrollTo ((int)b.Tag);
							}
						};
						if (Parent.TitleFont != null)
							button.Font = Parent.TitleFont;
						if (Parent.TitleColor != null)
							button.TitleColor = Parent.TitleColor;
						button.SizeToFit ();
						Buttons.Add (button);
						Views.Add (x.View);
						scroller.Add (button);
						scroller.Add (x.View);
						Parent.AddChildViewController (x);
						SetScrollToTop (x as UITableViewController);
					});
					SetScrollContent ();
				}
			}

			public void UpdateButtons ()
			{
				Buttons.ForEach (x => {
					if (Parent.TitleColor != null)
						x.SetTitleColor (Parent.TitleColor, UIControlState.Normal);
					if (Parent.TitleFont != null)
						x.Font = Parent.TitleFont;
				});
			}
		}

		public class CustomScroller : UIScrollView
		{
			public bool DisableScrolling{ get; set; }

			public CustomScroller ()
			{

			}

			public override CoreGraphics.CGPoint ContentOffset {
				get {
					return base.ContentOffset;
				}
				set {
					if (DisableScrolling)
						return;
					base.ContentOffset = value;
				}
			}
		}
	}
}

