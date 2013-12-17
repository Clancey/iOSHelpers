using System;
using MonoTouch.UIKit;
using iOSHelpers;
using MonoTouch.Foundation;
using System.IO;

namespace Samples
{
	public class RootViewController : UIViewController
	{

		public static readonly string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString ();
		public static readonly string DocumentsFolder = BaseDir + "/Documents/";
		StackPanel stackPanel;
		public RootViewController ()
		{
			var downloadButton = 
			View = stackPanel = new StackPanel () {
				new SimpleButton {
					Title = "Simple ActionSheet",
					BackgroundColor = UIColor.Gray,
					Tapped = (btn) => {
						var popup = new SimpleActionSheet () {
							{"Red",UIColor.Red,()=> Console.WriteLine("red")},
							{"Blue", UIColor.Blue, ()=> Console.WriteLine("Blue")},
							{"Black", UIColor.Black, ()=> Console.WriteLine("Black")},
							{"Green", UIColor.Green, ()=> Console.WriteLine("Green")},
							{"Cancel",()=> Console.WriteLine("Cancel")}
						};
						popup.ShowInView(View);
					}
				},
				new SimpleButton{
					Title = "I move on tilt",
					BackgroundColor = UIColor.Gray,
				}.AddMotion(-100,100),
				new SimpleButton{
					Title = "Click to download",
					BackgroundColor = UIColor.Gray,
					Tapped = async (btn)=>{
						btn.Enabled = false;
						var endPath = Path.Combine(DocumentsFolder,"test.zip");
						if(File.Exists(endPath))
							File.Delete(endPath);
						var downloader = new BackgroundDownload();
						downloader.ProgressChanged += (float obj) => Device.EnsureInvokedOnMainThread(()=> btn.Title = obj.ToString());
						await downloader.DownloadFileAsync(new Uri("http://ipv4.download.thinkbroadband.com/5MB.zip"),endPath );
						btn.Title = "Click to download";
						btn.Enabled = true;
					}
				},
				new UIImageView(UIImage.FromBundle("monkey").Blur(30))
				{
					ContentMode =  UIViewContentMode.ScaleAspectFill
				},
			};
		}

	}
}

