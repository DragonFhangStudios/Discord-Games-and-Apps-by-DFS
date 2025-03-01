using System;
using System.Windows;

namespace Discord_Games_and_Apps_by_DFS
{
	public class Program
	{
		[STAThread] // Required for WPF
		public static void Main()
		{
			Application app = new Application();
			app.Run(new MainWindow()); // Start the WPF UI
		}
	}
}
