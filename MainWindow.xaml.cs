using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Fractals {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			bitmap = new WriteableBitmap(2000, 2000, 500, 500, PixelFormats.Bgra32, null);
			pixels = new byte[2000, 2000, 4];
		}

		WriteableBitmap bitmap;
		byte[,,] pixels;
		Window1 pgWindow;
		BackgroundWorker calcWorker = null;

		int checkConvergence(Complex c, int acc) {
			Complex z = Complex.Zero;   //z0
			int color = 0;
			for(int i = 0; i < acc; i++) {
				if (z.Magnitude >= 2) {
					return 0;
				}
				z = Complex.Pow(z, 2);
				z += c;                 //zN+1 = zN^2 + C 
				color++;
			}
			return color;
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			pgWindow = new Window1();
			pgWindow.pgBar.Value = 0;
			pgWindow.Show();
			if (calcWorker == null) {
				calcWorker = new BackgroundWorker();
				calcWorker.DoWork += new DoWorkEventHandler(calcWorker_DoWork);
				calcWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(calcWorker_RunWorkerCompleted);
				calcWorker.ProgressChanged += new ProgressChangedEventHandler(calcWorker_ProgressChanged);
				calcWorker.WorkerReportsProgress = true;
				calcWorker.WorkerSupportsCancellation = true;
				if (!calcWorker.IsBusy) {
					calcWorker.RunWorkerAsync();
				}
			}
		}

		private void calcWorker_DoWork(object sender, DoWorkEventArgs e) {
			int height = 2000, width = 2000;	//wymiary bitmapy
			int count = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					for (int i = 0; i < 3; i++) pixels[row, col, i] = 0;	//R, G i B na 0
					count++;
					Complex z = new Complex((float)(col - 1650) / 900f, (float)(row - 1050) / 900f);
					if (checkConvergence(z, 100)) {
						pixels[row, col, 3] = 255;
					} else { pixels[row, col, 3] = 0; }
					calcWorker.ReportProgress((int)(100 * count / (float)(width * height)));
				}
			}
		}

		private void calcWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			pgWindow.pgBar.Value = e.ProgressPercentage;
		}

		private void calcWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			pgWindow.Close();
			byte[] pixels1d = new byte[2000 * 2000 * 4];
			int index = 0;
			for (int row = 0; row < 2000; row++) {
				for (int col = 0; col < 2000; col++) {
					for (int i = 0; i < 4; i++)
						pixels1d[index++] = pixels[row, col, i];
				}
			}

			Int32Rect rect = new Int32Rect(0, 0, 2000, 2000);
			int stride = 4 * 2000;
			bitmap.WritePixels(rect, pixels1d, stride, 0);

			Image image = new Image() {
				Source = bitmap,
				Stretch = Stretch.None,
				Margin = new Thickness(0)
			};
			canvas.Children.Add(image);
		}

	}
}
