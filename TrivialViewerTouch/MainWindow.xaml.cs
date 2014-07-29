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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrivialViewerTouch
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const string DEFAULT_IMAGE_PATH = "DoNotBuy.jpg";

		public MainWindow()
		{
			string appDir = System.AppDomain.CurrentDomain.BaseDirectory;
			appDir += "\\..\\..\\";
			string imagePath = appDir + DEFAULT_IMAGE_PATH;
			init(imagePath);
			InitializeComponent();
		}

		protected void init(string imagePath)
		{
			var bg = new BitmapImage(new Uri(imagePath));
			this.Background = new ImageBrush(bg);
		}
	}
}
