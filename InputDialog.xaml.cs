using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RecordPro
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : Window
	{

		/// <summary>
		/// Prompts the user for information and returns a string containing the result.
		/// </summary>
		/// <param name="title">The text to be displayed in the title bar</param>
		/// <param name="instruction"> The main instruction for the user</param>
		/// <param name="defaultText">The default text in the textbox</param>
		public InputDialog(string title, string instruction, string defaultText = "")
		{
			InitializeComponent();
			this.Title = title;
			this.Instruction.Content = instruction;
			this.userInput.Text = defaultText;
			userInput.Focus(); // Put focus on the textbox

		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
			this.Close(); // Close the window
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}
	}
}
