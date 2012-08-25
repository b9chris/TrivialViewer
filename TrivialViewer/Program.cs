using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TrivialViewer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			string[] args = Environment.GetCommandLineArgs();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (args.Length > 1)
				Application.Run(new ViewerWindow(args[1]));
			else
				Application.Run(new ViewerWindow());
		}
	}
}
