namespace RecordPro
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading.Tasks;

	public static class NativeMethods
	{
		public enum TaskDialogResult
		{
			Ok = 1,
			Cancel = 2,
			Retry = 4,
			Yes = 6,
			No = 7,
			Close = 8
		}

		[Flags]
		public enum TaskDialogButtons
		{
			OK = 0x0001,
			Yes = 0x002,
			No = 0x004,
			Cancel = 0x0008,
			Retry = 0x0010,
			Close = 0x0020
		}

		public enum TaskDialogIcon
		{
			Warning = 65535,
			Error = 65534,
			Information = 65533,
			Shield = 65532

		}

		/// <summary>
		/// Shows a dialog to the user consisting of a heading and instructions, in addition to an icon and buttons.
		/// </summary>
		/// <param name="hwndParent">The parent handle to use.</param>
		/// <param name="hInstance">The handle instance to use.</param>
		/// <param name="title">The text to be displayed in the title bar.</param>
		/// <param name="mainInstruction">The text to be displayed in the heading.</param>
		/// <param name="content">The instructions to be displayed.</param>
		/// <param name="buttons">An enumeration of different buttons that can be shown.</param>
		/// <param name="icon">The icon that will be shown.</param>
		/// <returns>A TaskDialogResult, indicating which button the user clicked.</returns>
		[DllImport("comctl32.dll", PreserveSig = false, CharSet = CharSet.Unicode)]
		internal static extern TaskDialogResult TaskDialog(
			IntPtr hwndParent,
			IntPtr hInstance,
			string title,
			string mainInstruction,
			string content,
			TaskDialogButtons buttons,
			TaskDialogIcon icon);

	}
}