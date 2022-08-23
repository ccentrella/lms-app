using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordPro
{
	public class User
	{
		public Gender Gender { get; set; }
		public string Name { get; set; }
		public string UserName { get; set; }
		public DateTime BirthDate { get; set; }
		public string Image { get; set; }
		public bool AcceptsConditions { get; set; }
		public string Password { get; set; }
	}
}
