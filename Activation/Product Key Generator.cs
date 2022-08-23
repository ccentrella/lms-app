using System;
using System.Collections.Generic;
class Program
{
	// © 2017 Autosoft Corporation. All rights reserved.
	static void Main()
	{
		Console.Clear();
		Console.Title = "Record Pro Product Key Generator";
		Console.WriteLine("Welcome to the Record Pro Product Key Generator.");
		Console.WriteLine("THIS PROGRAM IS ONLY INTENDED FOR AUTOSOFT EMPLOYEES, AND IS USED FOR GENERATING PRODUCT KEYS FOR RECORD PRO.");
		Console.WriteLine("Do you agree with the Autosoft terms and conditions?");
		if (Console.ReadLine().ToUpper() == "YES")
			CreateKey();
		else
			Main();
	}


	static void CreateKey()
	{
		Console.WriteLine("Enter GUID");
		string guid = Console.ReadLine();
		if (guid.Length != 38)
		{
			Console.WriteLine("Incorrect GUID. The GUID should be surrounded by curly braces, brackets, or parenthesis.");
			Main();
		}

		// Now generate the key		
		char[] guidArray = guid.ToCharArray();
		List<char> guidList = new List<char> { guidArray[3], guidArray[15], 'b',
			guidArray[22], '-', guidArray[29],guidArray[26],guidArray[2],'F','-',
			guidArray[35],guidArray[21] ,guidArray[5],guidArray[8],'-',
			guidArray[18],guidArray[32],guidArray[6],guidArray[27]};
		string productKey = string.Join("", guidList.ToArray());
		Console.WriteLine("Product Key: {0}", productKey);
		CreateKey();
	}
}