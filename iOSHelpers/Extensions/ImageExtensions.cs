using System;
using MonoTouch.UIKit;
using MonoTouch.CoreImage;
using System.Drawing;
using MonoTouch.CoreGraphics;
using System.Threading.Tasks;

namespace iOSHelpers
{
	public static class ImageExtensions
	{
		public static Task<UIImage> BlurAsync(this UIImage image, float radius)
		{
			return Task.Factory.StartNew (() => image.Blur(radius));
		}

		static CIContext context;
		public static UIImage Blur(this UIImage image, float radius)
		{
			if (image == null)
				return image;
			try
			{
				var imageToBlur = CIImage.FromCGImage(image.CGImage);

				if(imageToBlur == null)
					return image;
				var transform = new CIAffineClamp();
				transform.Transform = CGAffineTransform.MakeIdentity();
				transform.Image = imageToBlur;


				var gaussianBlurFilter = new CIGaussianBlur();

				gaussianBlurFilter.Image = transform.OutputImage;
				gaussianBlurFilter.Radius = radius;
				if (context == null)
					context = CIContext.FromOptions(null);

				var resultImage = gaussianBlurFilter.OutputImage;

				var finalImage = UIImage.FromImage(context.CreateCGImage(resultImage, new RectangleF(PointF.Empty, image.Size)), 1, UIImageOrientation.Up);
				return finalImage;

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return image;
			}
		}
	}
}

