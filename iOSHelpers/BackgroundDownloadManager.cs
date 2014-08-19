using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Foundation;
using System.Linq;

namespace iOSHelpers
{
	public static class BackgroundDownloadManager
	{
		public class CompletedArgs : EventArgs
		{
			public BackgroundDownloadFile File { get; set; }
		}
		public static event EventHandler<CompletedArgs> FileCompleted;
		static Dictionary<string,BackgroundDownloadFile> Files = new Dictionary<string,BackgroundDownloadFile> ();
		private static string stateFile = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "downloaderState");
		static BackgroundDownloadManager()
		{
			try{
				loadState();
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
				Files = new Dictionary<string,BackgroundDownloadFile> ();
			}
		}
		public static bool RemoveCompletedAutomatically { get; set; }
		public static void RemoveCompleted()
		{
			var completed = BackgroundDownloadManager.CurrentDownloads.Where (x => x.Status == BackgroundDownloadManager.BackgroundDownloadFile.FileStatus.Completed).ToList ();
			completed.ForEach(x=> Remove(x.Url));
		}
		private static void loadState()
		{
			if(!File.Exists(stateFile))
			{
				Files = new Dictionary<string,BackgroundDownloadFile> ();
				return;
			}

			var formatter = new BinaryFormatter();
			using(var stream = new FileStream(stateFile,FileMode.Open, FileAccess.Read, FileShare.Read)){
				Files = (Dictionary<string,BackgroundDownloadFile>) formatter.Deserialize(stream);
				stream.Close();
			}

		}
		private static void saveState()
		{
			lock (locker) {
				var formatter = new BinaryFormatter ();
				using (var stream = new FileStream (stateFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
					formatter.Serialize (stream, Files);
					stream.Close ();
				}
			}
		}
		[Serializable]
		public class BackgroundDownloadFile
		{
			public string Url { get; set; }
			public string Destination { get; set; }
			public float Percent { get; set; }
			public string Error { get; internal set; }
			public FileStatus Status {get;set;}
			public enum FileStatus{
				Downloading,
				Completed,
				Error,
				Canceled,
			}
			public override string ToString ()
			{
				return string.Format ("[BackgroundDownloadFile: Url={0}, Destination={1}, Percent={2}, Error={3}, Status={4}]", Url, Destination, Percent, Error, Status);
			}
		}

		static object locker = new object();
		internal static Dictionary<string,TaskCompletionSource<bool> > Tasks = new Dictionary<string, TaskCompletionSource<bool> >();
		static Dictionary<string,List<BackgroundDownload> > Controllers = new Dictionary<string, List<BackgroundDownload>>();
		static Dictionary<string,NSUrlSessionDownloadTask> DownloadTasks = new Dictionary<string, NSUrlSessionDownloadTask>();

		public static BackgroundDownloadFile[] CurrentDownloads
		{
			get{ return Files.Values.ToArray (); }
		}
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
		public static BackgroundDownload Download(string url, string destination)
		{
			return Download (new Uri (url), destination);
		}

		public static BackgroundDownload Download(Uri url, string destination)
		{
			var download = new BackgroundDownload ();
			//We dont want to await this. Just get it started
			#pragma warning disable 4014
			download.DownloadFileAsync (url, destination);
			#pragma warning restore 4014
			return download;
		}
		internal static void AddController(string url, BackgroundDownload controller)
		{
			lock (locker) {
				var list = GetControllers (url) ?? new List<BackgroundDownload>();
				list.Add (controller);
			}
			BackgroundDownloadFile file;
			if (!Files.TryGetValue (url, out file)) {
				Files [url] = file = new BackgroundDownloadFile {
					Destination = controller.Destination,
					Url = url,
				};
				saveState ();
			}

		}

		static void RemoveUrl(string url)
		{
			lock (locker) {
				if (Controllers.ContainsKey (url)) {
					Controllers [url] = null;
					Controllers.Remove (url);
				}
				if (DownloadTasks.ContainsKey (url)) {
					DownloadTasks [url] = null;
					DownloadTasks.Remove (url);
				}
				if(Tasks.ContainsKey(url))
					Tasks.Remove (url);
			}
		}
		internal static void UpdateProgress(NSUrlSessionDownloadTask downloadTask, float progress)
		{
			try{
				var url = downloadTask.OriginalRequest.Url.AbsoluteString;
				DownloadTasks [url] = downloadTask;
				UpdateProgress (url, progress);
				Files[url].Percent = progress;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}

		}
		internal static void UpdateProgress(string url, float progress)
		{
			var controllers = BackgroundDownloadManager.GetControllers (url);
			controllers.ForEach (x => Device.EnsureInvokedOnMainThread (() => x.Progress = progress));
		}
		public static void Remove(string url)
		{
			BackgroundDownloadFile file;
			if (Files.TryGetValue (url,out file)) {
				if (file.Status == BackgroundDownloadFile.FileStatus.Downloading)
					Cancel (url);
			}
			Files.Remove (url);
			saveState ();
		}
		public static void Cancel(string url)
		{
			NSUrlSessionDownloadTask task;
			if (DownloadTasks.TryGetValue (url, out task) && task != null) {
				task.Cancel ();
			}
			BackgroundDownloadFile file;
			if (Files.TryGetValue (url,out file)) {
				file.Status = BackgroundDownloadFile.FileStatus.Canceled;
			}
			RemoveUrl (url);
		}
		public static void Completed(NSUrlSessionTask downloadTask,NSUrl location)
		{

			NSFileManager fileManager = NSFileManager.DefaultManager;
			var url = downloadTask.OriginalRequest.Url.AbsoluteString;
			NSError errorCopy = null;
			if(Files.ContainsKey(url))
			{
				var file = Files [url];
				file.Status = BackgroundDownloadFile.FileStatus.Completed;
				file.Percent = 1;
				NSUrl originalURL = downloadTask.OriginalRequest.Url;
				NSUrl destinationURL = NSUrl.FromFilename (file.Destination);
				NSError removeCopy;

				fileManager.Remove (destinationURL, out removeCopy);
				var success = fileManager.Copy (location, destinationURL, out errorCopy);
				Console.WriteLine ("Success: {0}", success);
			}
			else
				Console.WriteLine ("Could not find the file!");

			TaskCompletionSource<bool> t;
			if (Tasks.TryGetValue (url, out t)) {
				if (errorCopy == null) {
					if (!t.TrySetResult (true))
						Console.WriteLine ("ERROR");
				} else {
					var file = Files [url];
					file.Status = BackgroundDownloadFile.FileStatus.Error;
					file.Error = string.Format("Error during the copy: {0}", errorCopy.LocalizedDescription);
					t.TrySetException (new Exception (file.Error));
				}
				BackgroundDownloadManager.Tasks.Remove (url);
			}
			RemoveUrl (url);
			saveState ();
			Console.WriteLine ("Tasks: {0}, Downloads {1}, Controllers {2} ", Tasks.Count, DownloadTasks.Count, Controllers.Count);
			foreach (var f in Files) {
				Console.WriteLine (f);
			}
			var evt = FileCompleted;
			if (evt != null)
				evt (downloadTask, new CompletedArgs{ File = Files [url]});
			if (RemoveCompletedAutomatically)
				Remove (url);
		}
		public static void Failed(NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			if (error != null)
				Console.WriteLine (error.LocalizedDescription);
			float progress = task.BytesReceived / (float)task.BytesExpectedToReceive;
			var url = task.OriginalRequest.Url.AbsoluteString;
			DownloadTasks.Remove (url);
			UpdateProgress (url, progress);
			TaskCompletionSource<bool> t;
			if (BackgroundDownloadManager.Tasks.TryGetValue (task.OriginalRequest.Url.AbsoluteString, out t)) {
				if (error == null) {
					if (!t.TrySetResult (true))
						Console.WriteLine ("ERROR");
				} else {

					t.TrySetException (new Exception (string.Format ("Error during the copy: {0}", error.LocalizedDescription)));
				}
				BackgroundDownloadManager.Tasks.Remove (task.OriginalRequest.Url.AbsoluteString);
			}

			RemoveUrl (url);
		}

	}
}

