using System;
using MonoTouch.Foundation;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace iOSHelpers
{
	public class BackgroundDownload
	{
		static BackgroundDownload()
		{
			session = InitBackgroundSession ();
		}
		static Dictionary<string,TaskCompletionSource<bool> > Tasks = new Dictionary<string, TaskCompletionSource<bool> >();
		static Dictionary<string,List<BackgroundDownload> > Controllers = new Dictionary<string, List<BackgroundDownload>>();
		static object locker = new object();
		static List<BackgroundDownload> GetControllers(string url)
		{
			List<BackgroundDownload> list;
			lock (locker)
				if (!Controllers.TryGetValue (url, out list)) {
					Controllers.Add (url, list = new List<BackgroundDownload> ());
					if(!Tasks.ContainsKey(url))
						Tasks.Add (url, new TaskCompletionSource<bool>());
				}
			return list;
		}
		static void AddController(string url, BackgroundDownload controller)
		{
			lock (locker) {
				var list = GetControllers (url);
				list.Add (controller);
			}
		}
		static void RemoveUrl(string url)
		{
			lock (locker)
				Controllers [url] = null;
		}

		public BackgroundDownload ()
		{
		}

		NSUrlSessionDownloadTask downloadTask;
		static NSUrlSession session;
		public static NSAction BackgroundSessionCompletionHandler;
		public async Task DownloadFileAsync (Uri url, string destination)
		{
			this.url = url.AbsoluteUri;
			if (downloadTask != null)
				return;
			if (session == null)
				session = InitBackgroundSession ();
			Destination = destination;
			if(!Tasks.TryGetValue(url.AbsoluteUri , out Tcs))
			{
				Tcs = new TaskCompletionSource<bool> ();
				Tasks.Add (url.AbsoluteUri, Tcs);
				using (var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri))) {
					downloadTask = session.CreateDownloadTask (request);
					downloadTask.Resume ();
				}
			}

			AddController (this.url,this);

			await Tcs.Task;
		}

		string url;

		public event Action<float> ProgressChanged;

		float progress;

		public float Progress {
			get {
				return progress;
			}
			private set {
				if (Math.Abs (progress - value) < float.Epsilon)
					return;
				progress = value;
				if (ProgressChanged != null)
					ProgressChanged (progress);

			}
		}

		static NSUrlSession InitBackgroundSession ()
		{
			Console.WriteLine ("InitBackgroundSession");
			using (var configuration = NSUrlSessionConfiguration.BackgroundSessionConfiguration ("async.background.downloader")) {
				return NSUrlSession.FromConfiguration (configuration, new UrlSessionDelegate (), null);
			}
		}

		protected string Destination;
		protected TaskCompletionSource<bool> Tcs;

		public class UrlSessionDelegate : NSUrlSessionDownloadDelegate
		{

			public UrlSessionDelegate ()
			{
			}

			public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
			{
				float progress = totalBytesWritten / (float)totalBytesExpectedToWrite;
				Console.WriteLine (string.Format ("DownloadTask: {0}  progress: {1}", downloadTask.Handle.ToString(), progress));
				var url = downloadTask.OriginalRequest.Url.AbsoluteString;
				var controllers = GetControllers (url);
				controllers.ForEach (x => InvokeOnMainThread (() => x.Progress = progress));
			}

			public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
			{
				Console.WriteLine ("File downloaded in : {0}", location);
				NSFileManager fileManager = NSFileManager.DefaultManager;

				var URLs = fileManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User);
				NSUrl documentsDictionry = URLs [0];

				NSError errorCopy = null;
				string url = downloadTask.OriginalRequest.Url.AbsoluteString;
				foreach (var controller in GetControllers(url)) {
					NSUrl originalURL = downloadTask.OriginalRequest.Url;
					NSUrl destinationURL = NSUrl.FromFilename (controller.Destination);
					NSError removeCopy;

					fileManager.Remove (destinationURL, out removeCopy);
					var success = fileManager.Copy (location, destinationURL, out errorCopy);

				}
				TaskCompletionSource<bool> t;
				if (Tasks.TryGetValue (url, out t)) {
					if (errorCopy == null) {
						if (!t.TrySetResult (true))
							Console.WriteLine ("ERROR");
					} else {

						t.TrySetException (new Exception (string.Format ("Error during the copy: {0}", errorCopy.LocalizedDescription)));
					}
					Tasks.Remove (url);
				}

			}

			public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
			{

				float progress = task.BytesReceived / (float)task.BytesExpectedToReceive;
				GetControllers(task.OriginalRequest.Url.AbsoluteString).ForEach(controller =>
					InvokeOnMainThread (() => controller.Progress = progress));

				TaskCompletionSource<bool>  t;
				if (Tasks.TryGetValue (task.OriginalRequest.Url.AbsoluteString, out t)) {
					if (error == null) {
						if (!t.TrySetResult (true))
							Console.WriteLine ("ERROR");
					} else {

						t.TrySetException (new Exception (string.Format ("Error during the copy: {0}", error.LocalizedDescription)));
					}
					Tasks.Remove (task.OriginalRequest.Url.AbsoluteString);
				}



			}

			public override void DidResume (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
			{
				Console.WriteLine ("DidResume");
			}

			public override void DidFinishEventsForBackgroundSession (NSUrlSession session)
			{
				Console.WriteLine ("All tasks are finished");
				if (BackgroundSessionCompletionHandler != null)
					BackgroundSessionCompletionHandler ();
				BackgroundSessionCompletionHandler = null;
			}
		}
	}
}

