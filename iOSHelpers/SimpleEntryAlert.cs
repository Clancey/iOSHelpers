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
		protected UIAlertController alertController;
		public UITextField TextField;

		public SimpleEntryAlert(string title, string details = "", string placeholder = "", string defaultValue = "", string okString = "Ok",string cancelString = "Cancel")
		{
			this.title = title;
			this.details = details;
			this.placeholder = placeholder;
			this.defaultValue = defaultValue;
			this.okString = okString;
			this.cancelString = cancelString;
			alertController = UIAlertController.Create(title, details, UIAlertControllerStyle.Alert);

			var cancel = UIAlertAction.Create(cancelString, UIAlertActionStyle.Cancel, (alert) => { tcs.TrySetCanceled(); });
			var ok = UIAlertAction.Create(okString, UIAlertActionStyle.Default,
				a => { tcs.TrySetResult(TextField.Text); });

			alertController.AddTextField(field =>
			{
				field.Placeholder = placeholder;
				field.Text = defaultValue;
				TextField = field;
			});
			alertController.AddAction(ok);
			alertController.AddAction(cancel);


		}

		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

		public Task<string> GetInput(UIViewController fromController)
		{

			fromController.PresentViewControllerAsync(alertController, true);
			return tcs.Task;
		}

		bool isDisposed;
		public void Dispose()
		{
			if(isDisposed)
				OnDispose();
			isDisposed = true;
		}
		protected virtual void OnDispose()
		{
			alertController?.Dispose();
			tcs?.TrySetCanceled();
			tcs = null;
		}
	}
}