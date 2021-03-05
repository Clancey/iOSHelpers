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
		private readonly string okString;
		private readonly string cancelString;
		UIAlertController alertController;
		UIAlertView alertView;

		public SimpleEntryAlert(string title, string details = "", string placeholder = "", string defaultValue = "", string okString = "Ok",string cancelString = "Cancel")
		{
			this.title = title;
			this.details = details;
			this.placeholder = placeholder;
			this.defaultValue = defaultValue;
			this.okString = okString;
			this.cancelString = cancelString;
			alertController = UIAlertController.Create(title, details, UIAlertControllerStyle.Alert);
			setupAlertController();
			
		}

		void setupAlertController()
		{
			UITextField entryField = null;
			var cancel = UIAlertAction.Create(cancelString, UIAlertActionStyle.Cancel, (alert) => { tcs.TrySetCanceled(); });
			var ok = UIAlertAction.Create(okString, UIAlertActionStyle.Default,
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