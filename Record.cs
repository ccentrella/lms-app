using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordPro
{
	/// <summary>
	/// Represents a student's record
	/// </summary>
	class Record
	{
		/// <summary>
		/// The name of the student
		/// </summary>
		public string StudentName { get; set; }

		/// <summary>
		/// The grade level which this record represents
		/// </summary>
		public string GradeLevel { get; set; }
	}

	/// <summary>
	/// Represents a list of records
	/// </summary>
	class Records : ObservableCollection<Record>
	{

	}
}
