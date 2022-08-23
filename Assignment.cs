namespace RecordPro
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Controls;

	class Assignment
	{
		/// <summary>
		/// Gets or sets course name for the assignment.
		/// </summary>
		public string Course { get; set; }

		/// <summary>
		/// The details for the assignment.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// Gets or sets the amount of time spent on the assignment.
		/// </summary>
		public TimeSpan Time { get; set; }

		/// <summary>
		/// Gets or sets the grade for the assignment.
		/// </summary>
		public byte? Grade { get; set; }

		/// <summary>
		/// Any additional information regarding the assignment.
		/// </summary>
		public string Notes { get; set; }

	}

	/// <summary>
	/// Represents a list of assignments
	/// </summary>
	class Assignments : ObservableCollection<Assignment>
	{

	}
}
