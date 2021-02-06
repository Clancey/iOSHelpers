using System;
using System.Threading.Tasks;
using UIKit;

namespace iOSHelpers
{
	public class SimpleEntryAlert : IDisposable
	{
		readonly string title;
		readonly string details;
		readonly string placeholder;
		private readonly string defaultValue;
		UIAlertController alertController;
		UIAlertView alertView;

		public SimpleEntryAlert(string title, string details = "", string placeholder = "", string defaultValue = "")
		{
			this.title = title;
			this.details = details;
			this.placeholder = placeholder;
			this.defaultValue = defaultValue;
			alertController = UIAlertController.Create(title, details, UIAlertControllerStyle.Alert);
			setupAlertController();
			
		}

		void setupAlertController()
		{
			UITextField entryField = null;
			var cancel = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (alert) => { tcs.TrySetCanceled(); });
			var ok = UIAlertAction.Create("Login", UIAlertActionStyle.Default,
				a => { tcs.TrySetResult(entryField.Text); });

			alertController.AddTextField(field =>
			{
				field.Placeholder = placeholder;
				field.Text = defaultValue;
				entryField = field;
			});
			alertController.AddAction(ok);
			alertController.AddAction(cancel);
		}

		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

		public Task<string> GetInput(UIViewController fromController)
		{
			if (alertController != null)
			{
				fromController.PresentViewControllerAsync(alertController, true);
			}
			else
			{
				alertView.Show();
			}
			return tcs.Task;
		}

		public void Dispose()
		{
			alertController?.Dispose();
			alertView?.DismissWithClickedButtonIndex(alertView.CancelButtonIndex, true);
			alertView?.Dispose();
			tcs?.TrySetCanceled();
			tcs = null;
		}
	}
}