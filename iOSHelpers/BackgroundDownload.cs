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

		}
		public static void Initalize()
		{
			session = InitBackgroundSession ();
		}
		public async void FindSessions()
		{

			var tasks = await session.GetTasksAsync();
			foreach (var t in tasks.DownloadTasks) {
				if (t.State == NSUrlSessionTaskState.Suspended)
					t.Resume ();
			}
			Console.WriteLine (tasks);
		}

		public BackgroundDownload ()
		{
		}
		public BackgroundDownload (NSUrlSessionDownloadTask task)
		{
			downloadTask = task;
			Url = task.OriginalRequest.Url.AbsoluteString;
		}

		NSUrlSessionDownloadTask downloadTask;
		static NSUrlSession session;
		public string SessionId { get; set; }
		public async Task<nuint> DownloadFileAsync (Uri url, string destination)
		{
			this.Url = url.AbsoluteUri;
			if (downloadTask != null)
				return downloadTask.TaskIdentifier;
			if (session == null) {
				Initalize ();
			}
			Destination = destination;

			SessionId = session.Configuration.Identifier;
			if (!BackgroundDownloadManager.Tasks.TryGetValue (url.AbsoluteUri, out Tcs)) {
				Tcs = new TaskCompletionSource<bool> ();
				BackgroundDownloadManager.Tasks.Add (url.AbsoluteUri, Tcs);
				using (var request = new NSUrlRequest (new NSUrl (url.AbsoluteUri))) {
					downloadTask = session.CreateDownloadTask (request);
					downloadTask.Resume ();
				}
			}

			BackgroundDownloadManager.AddController (this.Url, this);

			await Tcs.Task;

			return downloadTask.TaskIdentifier; 
		}

		public string Url{ get; private set; }

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
		public static async void RepairFromBackground(string sessionIdentifier,Action action)
		{
			//REset the files so they load from the disc if it was restored from a group session.
			BackgroundDownloadManager.Reload ();
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
		public static Action<List<BackgroundDownload>> RestoredDownloads;
		static NSUrlSession InitBackgroundSession (string identifier)
		{
			Console.WriteLine ("InitBackgroundSession");
			using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (identifier)) {
				if (!string.IsNullOrEmpty (SharedContainerIdentifier))
					configuration.SharedContainerIdentifier = SharedContainerIdentifier;
				var ses = NSUrlSession.FromConfiguration (configuration, new UrlSessionDelegate (), null);
				ses.GetTasks ((data, upload, downloads) => {
					restoreTasks(ses,data,upload,downloads);
				});
				return ses;
			}
		}

		async static void restoreTasks(NSUrlSession ses, NSUrlSessionDataTask[] sessions, NSUrlSessionUploadTask[] uploads, NSUrlSessionDownloadTask[] downloads)
		{
			List<BackgroundDownload> restoredDownloads = new List<BackgroundDownload>();
			foreach(var d in downloads)
			{
				var url = d.OriginalRequest.Url.AbsoluteString;
				if(d.Error != null)
				{
					BackgroundDownloadManager.Errored(d);
				}
				BackgroundDownload download;
				BackgroundDownloadManager.BackgroundDownloadFile downloadFile;
				download = new BackgroundDownload (d) {
					SessionId = ses.Configuration.Identifier,
				};
				foreach(var key in BackgroundDownloadManager.Files)
				{
					Console.WriteLine(key.Key);
				}
				if(BackgroundDownloadManager.Files.TryGetValue(url, out downloadFile))
					download.Destination = downloadFile.Destination;
				BackgroundDownloadManager.AddController (url, download);
				restoredDownloads.Add(download);

			}
			if(RestoredDownloads != null)
			{
				RestoredDownloads(restoredDownloads);
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

