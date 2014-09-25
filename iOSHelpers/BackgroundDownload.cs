using System;
using Foundation;
using System.Threading.Tasks;
using UIKit;
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

		public event Action<nfloat> ProgressChanged;

		nfloat progress;

		public nfloat Progress {
			get {
				return progress;
			}
			internal set {
				if (Math.Abs (progress - value) < nfloat.Epsilon)
					return;
				progress = value;
				if (ProgressChanged != null)
					ProgressChanged (progress);

			}
		}

		static Dictionary<string,NSUrlSession> backgroundSessions = new Dictionary<string, NSUrlSession>();
		static Dictionary<string,Action> backgroundSessionCompletion = new Dictionary<string, Action>();
		public static void RepairFromBackground(string sessionIdentifier,Action action)
		{
			if (!backgroundSessions.ContainsKey (sessionIdentifier)) {
				backgroundSessions [sessionIdentifier] = InitBackgroundSession (sessionIdentifier);
				backgroundSessionCompletion [sessionIdentifier] = action;
			}
		}
		static void CompletBackgroundSession(string identifier)
		{
			backgroundSessions.Remove (identifier);
			backgroundSessionCompletion.Remove (identifier);
		}

		public static string Identifier = "async.background.downloader";
		public static string SharedContainerIdentifier { get; set; }
		static NSUrlSession InitBackgroundSession ()
		{
			return InitBackgroundSession (Identifier);
		}

		static NSUrlSession InitBackgroundSession (string identifier)
		{
			Console.WriteLine ("InitBackgroundSession");
			using (var configuration = NSUrlSessionConfiguration.BackgroundSessionConfiguration (identifier)) {
				if (!string.IsNullOrEmpty (SharedContainerIdentifier))
					configuration.SharedContainerIdentifier = SharedContainerIdentifier;
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
				nfloat progress = totalBytesWritten / (nfloat)totalBytesExpectedToWrite;
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
				Action action;
				if (backgroundSessionCompletion.TryGetValue (session.Configuration.Identifier, out action) && action != null)
					action ();
				CompletBackgroundSession (session.Configuration.Identifier);
			}
		}
	}
}

