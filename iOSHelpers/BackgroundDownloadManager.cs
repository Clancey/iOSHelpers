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
		public static string BaseDir = Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString ();
		public class CompletedArgs : EventArgs
		{
			public BackgroundDownloadFile File { get; set; }
		}
		public static event EventHandler<CompletedArgs> FileCompleted;
		static Dictionary<string, BackgroundDownloadFile> files;
		internal static Dictionary<string, BackgroundDownloadFile> Files {
			get {
				if (files == null)
					loadState ();
				return files;
			}
			set {
				files = value;
			}
		}
		private static string stateFile = Path.Combine (System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal), "downloaderState");

		public static Action<Dictionary<string,BackgroundDownloadFile>> SaveState { get; set; }
		public static Func<Dictionary<string,BackgroundDownloadFile>> LoadState { get; set; }
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

		public static void Reload ()
		{
			loadState ();
		}

		private static void loadState()
		{
			lock (locker) {
				if (LoadState != null) {
					try {
						LoadState ();
						return;
					} catch (Exception e) {
						Console.WriteLine (e);
					}
				}
				if (!File.Exists (stateFile)) {
					Files = new Dictionary<string,BackgroundDownloadFile> ();
					return;
				}

				var formatter = new BinaryFormatter ();
				using (var stream = new FileStream (stateFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
					Files = (Dictionary<string,BackgroundDownloadFile>)formatter.Deserialize (stream);
					stream.Close ();
				}
			}

		}
		private static void saveState()
		{
			lock (locker) {
				if(SaveState != null)
				{
					try{
						SaveState(Files);
						return;
					}
					catch (Exception e){
						Console.WriteLine(e);
					}
				}
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
			public string SessionId { get; set; }
			public string Url { get; set; }
			public string Destination { get; set; }
			public float Percent { get; set; }
			public string Error { get; internal set; }
			public string TempLocation {get;set;}
			public FileStatus Status {get;set;}
			public enum FileStatus{
				Downloading,
				Completed,
				Error,
				Canceled,
				Temporary,
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
		public static Action<BackgroundDownloadFile> DownloadError {get;set;}
		public static void Errored(NSUrlSessionDownloadTask downloadTask)
		{
			BackgroundDownloadFile download;
			var url = downloadTask.OriginalRequest.Url.AbsoluteString;
			if (!Files.TryGetValue (url, out download))
				return;
			if (DownloadError != null)
				DownloadError (download);

			RemoveUrl (url);
		}
		internal static void AddController(string url, BackgroundDownload controller)
		{
			lock (locker) {
				var list = GetControllers (url) ?? new List<BackgroundDownload>();
				list.Add (controller);
			}
			BackgroundDownloadFile file;
			if (!Files.TryGetValue (url, out file)) {
				Console.WriteLine ("Adding URL {0}", url);
				Files [url] = file = new BackgroundDownloadFile {
					Destination = MakeRelativePath(BaseDir, controller.Destination),
					Url = url,
					SessionId = controller.SessionId
				};
			}
			saveState ();

		}

		public static String MakeRelativePath(String fromPath, String toPath)
		{
			return toPath.Replace (fromPath, "");
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
		internal static void UpdateProgress(NSUrlSessionDownloadTask downloadTask, nfloat progress)
		{
			try{
				var url = downloadTask.OriginalRequest.Url.AbsoluteString;
				DownloadTasks [url] = downloadTask;
				UpdateProgress (url, progress);
				Files[url].Percent = (float)progress;
			}
			catch(Exception ex) {
				Console.WriteLine (ex);
			}

		}
		internal static void UpdateProgress(string url, nfloat progress)
		{
			var controllers = BackgroundDownloadManager.GetControllers (url);
			controllers.ForEach (x => Device.EnsureInvokedOnMainThread (() => x.Progress = progress));
		}
		public static void Remove(string url)
		{
			Console.WriteLine ("Removing URL: {0}", url);
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
		public static bool AutoProcess = true;
		public static void Completed(NSUrlSessionTask downloadTask,NSUrl location)
		{

			NSFileManager fileManager = NSFileManager.DefaultManager;
			var url = downloadTask.OriginalRequest.Url.AbsoluteString;
			Console.WriteLine ("Looking for: {0}",url);
			NSError errorCopy = null;
			foreach (var f in Files) {
				Console.WriteLine ("Existing file: {0}", f.Key);
			}
			if(Files.ContainsKey(url))
			{
				var file = Files [url];
				file.Status = BackgroundDownloadFile.FileStatus.Temporary;
				file.Percent = 1;
				if (!AutoProcess) {
					var sharedFolder = fileManager.GetContainerUrl (BackgroundDownload.SharedContainerIdentifier);
					fileManager.CreateDirectory (sharedFolder, true, null,out errorCopy);
					var fileName = Path.GetFileName (file.Destination);
					var newTemp = Path.Combine (sharedFolder.RelativePath,fileName);

					var success1 = fileManager.Copy (location, NSUrl.FromFilename(newTemp), out errorCopy);
					Console.WriteLine ("Success: {0} {1}", success1,errorCopy);
					file.TempLocation = fileName;
					saveState ();
					return;
				}
				NSUrl originalURL = downloadTask.OriginalRequest.Url;
				var dest = Path.Combine (BaseDir + file.Destination);
				NSUrl destinationURL = NSUrl.FromFilename (dest);
				NSError removeCopy;

				fileManager.Remove (destinationURL, out removeCopy);
				Console.WriteLine ("Trying to copy to {0}", dest);
				var success = fileManager.Copy (location, destinationURL, out errorCopy);
				if (success)
					file.Status = BackgroundDownloadFile.FileStatus.Completed;
				Console.WriteLine ("Success: {0} {1}", success,errorCopy);
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
			nfloat progress = task.BytesReceived / (nfloat)task.BytesExpectedToReceive;
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

