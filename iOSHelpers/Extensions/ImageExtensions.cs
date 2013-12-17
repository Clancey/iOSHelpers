using System;
using MonoTouch.UIKit;
using MonoTouch.CoreImage;
using System.Drawing;

namespace iOSHelpers
{
	public static class ImageExtensions
	{
		public static UIImage Blur(this UIImage image, float radius)
		{
			try{
				CIImage imageToBlur = CIImage.FromCGImage(image.CGImage);
				var gaussianBlurFilter = new CIGaussianBlur();

				gaussianBlurFilter.Image = imageToBlur;
				gaussianBlurFilter.Radius = radius;
				CIContext context = CIContext.FromOptions(null);
				CIImage resultImage = gaussianBlurFilter.OutputImage;

				UIImage finalImage = UIImage.FromImage(context.CreateCGImage(resultImage, new RectangleF(PointF.Empty,image.Size)), 1, UIImageOrientation.Up);
				return finalImage;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
				return image;
			}
		}
	}
}

