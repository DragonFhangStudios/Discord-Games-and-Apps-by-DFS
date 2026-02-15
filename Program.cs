using System;
using System.Windows;

namespace Discord_Games_and_Apps_by_DFS
{
	public class Program
	{
		[STAThread] // Required for WPF
		public static void Main()
		{
			try
			{
				Application app = new Application();
				app.Run(new MainWindow()); // Start the WPF UI
			}
			catch (Exception)
			{
				// In a production environment, consider logging the exception detail (ex.ToString())
				// to a secure log file or telemetry service.
				MessageBox.Show("A critical error occurred, and the application will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Environment.Exit(1);
			}
		}
	}
}
