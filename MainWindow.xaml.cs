namespace RecordPro
{
	using Record_Pro_Functions;
	using System;
	using System.IO;
	using System.Security;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Interop;
	using System.Windows.Media.Imaging;
	using System.Windows.Shell;
	using System.Windows.Threading;
    using System.Windows.Controls;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		delegate void mainDelegate(); // Used for asynchronous programming
		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Show the avatar popup
			AvatarPopup.IsOpen = true;
		}

		private void AvatarPopup_Opened(object sender, EventArgs e)
		{
			string currentUser = (string)App.Current.Properties["Current User"];

			// If no user is logged in, load the avatar pane.
			// Otherwise, load the home pane. 
			if (currentUser == null || currentUser == "None")
			{
				SmallPane.Navigate(new SignIn1());
			}
			else
			{
				SmallPane.Navigate(new HomePane());

			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Enable the program to have access to the main window.
			App.mWindow = this;
		}

		private void SystemCommands_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
		{
			if (e.Command == SystemCommands.CloseWindowCommand | e.Command == SystemCommands.MinimizeWindowCommand)
				e.CanExecute = true;
			else if (e.Command == SystemCommands.RestoreWindowCommand)
			{
				if (this.WindowState == WindowState.Maximized)
					e.CanExecute = true;
				else
					e.CanExecute = false;
			}
			else if (e.Command == SystemCommands.MaximizeWindowCommand)
			{
				if (this.WindowState == WindowState.Maximized)
					e.CanExecute = false;
				else
					e.CanExecute = true;
			}
		}

		private void SystemCommands_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			if (e.Command == SystemCommands.MinimizeWindowCommand)
				SystemCommands.MinimizeWindow(this);
			else if (e.Command == SystemCommands.RestoreWindowCommand)
				SystemCommands.RestoreWindow(this);
			else if (e.Command == SystemCommands.MaximizeWindowCommand)
				SystemCommands.MaximizeWindow(this);
			else if (e.Command == SystemCommands.CloseWindowCommand)
				SystemCommands.CloseWindow(this);
		}

		private void mainWindow_StateChanged(object sender, EventArgs e)
		{
			// Update the placement of the chrome buttons
			if (this.WindowState == WindowState.Maximized)
			{
				chrome.Margin = new Thickness(0);
			}
			else
			{
				chrome.Margin = new Thickness(0, -8, -8, 0);
			}
		}

		private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.Source != Progress)
				this.DragMove(); // Move the window
		}

		private void AvatarPopup_Closed(object sender, EventArgs e)
		{
			string currentUser = (string)App.Current.Properties["Current User"];

			// If no user is logged in, load the default image and update the label
			if (currentUser == null || currentUser == "None")
			{
				RecordProFunctions.LoadDefaultImage(Gender.Unknown);
				UserHeader.Content = "Sign In";
			}
		}

		private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Warn anyone attempting to update the record
			if (MainFrame.NavigationService.Content.GetType() == typeof(UpdateRecord) &&
				NativeMethods.TaskDialog(new WindowInteropHelper(this).Handle, IntPtr.Zero, "Warning - Record Pro",
					"If you close this program, your changes will not be saved.", "Are you sure you would like to continue?",
					NativeMethods.TaskDialogButtons.Yes | NativeMethods.TaskDialogButtons.No,
					 NativeMethods.TaskDialogIcon.Warning) == NativeMethods.TaskDialogResult.No)
			{
				e.Cancel = true;
			}
		}

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var page = (Page)e.Content;
            this.Title = page.Title + " - Record Pro 2017";
        }
    }
}
