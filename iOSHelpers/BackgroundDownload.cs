using System;
using MonoTouch.Foundation;
using System.Threading.Tasks;
using MonoTouch.UIKit;
using System.Collections.Generic;

namespace iOSHelpers
{
	public class BackgroundDownload
	{

		static BackgroundDownload ()
		{
			session = InitBackgroundSession ();
		}
		public static void Initalize()
		{

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
			if (!BackgroundDownloadManager.Tasks.TryGetValue (url.AbsoluteUri, out Tcs)) {
				Tcs = new TaskCompletionSource<bool> ();
				BackgroundDownloadManager.Tasks.Add (url.AbsoluteUri, Tcs);
				using (var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri))) {
					downloadTask = session.CreateDownloadTask (request);
					downloadTask.Resume ();

				}
			}

			BackgroundDownloadManager.AddController (this.url, this);

			await Tcs.Task;
		}

		string url;

		public event Action<float> ProgressChanged;

		float progress;

		public float Progress {
			get {
				return progress;
			}
			internal set {
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

		internal string Destination;
		protected TaskCompletionSource<bool> Tcs;

		public class UrlSessionDelegate : NSUrlSessionDownloadDelegate
		{
			public UrlSessionDelegate ()
			{
			}

			public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
			{
				float progress = totalBytesWritten / (float)totalBytesExpectedToWrite;
				Console.WriteLine (string.Format ("DownloadTask: {0}  progress: {1}", downloadTask.Handle.ToString (), progress));
				BackgroundDownloadManager.UpdateProgress (downloadTask, progress);
			}

			public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
			{
				Console.WriteLine ("File downloaded in : {0}", location);
				BackgroundDownloadManager.Completed (downloadTask, location);
			}

			public override void DidCompleteWithError (NSUrlSession session, NSUrlSessionTask task, NSError error)
			{
				Console.WriteLine ("DidCompleteWithError");
				if (error != null) {
					Console.WriteLine (error.LocalizedDescription);
					BackgroundDownloadManager.Failed (session, task, error);
				} else {
					Console.WriteLine ("False positive");
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

