using System.ComponentModel;
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
			bitmap = new WriteableBitmap(2000, 2000, 500d, 500d, PixelFormats.Bgra32, null);
			pixels = new byte[bitmap.PixelWidth, bitmap.PixelHeight, 4];
			height = bitmap.PixelHeight;
			width = bitmap.PixelWidth; //wymiary bitmapy

			/*if (calcWorker == null) {
				calcWorker = new BackgroundWorker();
				calcWorker.DoWork += new DoWorkEventHandler(calcWorker_DoWork);
				calcWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(calcWorker_RunWorkerCompleted);
				calcWorker.ProgressChanged += new ProgressChangedEventHandler(calcWorker_ProgressChanged);
				calcWorker.WorkerReportsProgress = true;
				calcWorker.WorkerSupportsCancellation = true;
			}*/
		}


		WriteableBitmap bitmap;
		int width, height;
		byte[,,] pixels;
		Window1 pgWindow;
		//BackgroundWorker calcWorker = null;

		byte checkConvergence(float[] c, int acc) {
			float[] z = { 0, 0 };       //z0
			for (int i = 0; i < acc; i++) {
				if (z[0] * z[0] + z[1] * z[1] > 4) {
					return 0;
				}
				float[] tmp = new float[2];
				tmp[0] = z[0];
				tmp[1] = z[1];
				//bo Zuze
				z[0] = tmp[0] * tmp[0] - tmp[1] * tmp[1] + c[0];
				z[1] = 2 * tmp[0] * tmp[1] + c[1];      //zN+1 = zN^2 + c
			}
			return 0xFF;
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			pgWindow = new Window1();
			pgWindow.pgBar.Value = 0;
			pgWindow.Show();

			int count = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					for (int i = 0; i < 3; i++) pixels[row, col, i] = 0;    //R, G i B na 0

					float[] z = new float[2];
					z[0] = (float)(col - 3 * width / 4) / 900f;
					z[1] = (float)(row - height / 2) / 900f;

					pixels[row, col, 3] = (byte)checkConvergence(z, 100);           //nadaje kolor (alpha) w zaleznosci od 
																					//ilosci iteracji (chyba)

					count++;
					pgWindow.pgBar.Value = (int)(100 * count / (float)(width * height));
					//calcWorker.ReportProgress((int)(100 * count / (float)(width * height)));
				}
			}


			pgWindow.Close();
			byte[] pixels1d = new byte[width * height * 4];
			int index = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					for (int i = 0; i < 4; i++)
						pixels1d[index++] = pixels[row, col, i];
				}
			}

			Int32Rect rect = new Int32Rect(0, 0, width, height);
			int stride = 4 * width;
			bitmap.WritePixels(rect, pixels1d, stride, 0);

			Image image = new Image() {
				Source = bitmap,
				Stretch = Stretch.None,
				Margin = new Thickness(0),
				Width = 790,
				Height = 718
			};
			canvas.Children.Add(image);
			//if (!calcWorker.IsBusy) {
			//		calcWorker.RunWorkerAsync();
			//}

		}

		/*private void calcWorker_DoWork(object sender, DoWorkEventArgs e) {
			int count = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					for (int i = 0; i < 3; i++) pixels[row, col, i] = 0;    //R, G i B na 0

					float[] z = new float[2];
					z[0] = (float)(col - 3 * width / 4) / 900f;
					z[1] = (float)(row - height / 2) / 900f;

					pixels[row, col, 3] = (byte)checkConvergence(z, 100);           //nadaje kolor (alpha) w zaleznosci od 
																					//ilosci iteracji (chyba)

					count++;
					calcWorker.ReportProgress((int)(100 * count / (float)(width * height)));
				}
			}
			calcWorker.ReportProgress(100);
		}

		private void calcWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			pgWindow.pgBar.Value = e.ProgressPercentage;
		}

		private void calcWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			pgWindow.Close();
			byte[] pixels1d = new byte[width * height * 4];
			int index = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					for (int i = 0; i < 4; i++)
						pixels1d[index++] = pixels[row, col, i];
				}
			}

			Int32Rect rect = new Int32Rect(0, 0, width, height);
			int stride = 4 * width;
			bitmap.WritePixels(rect, pixels1d, stride, 0);

			Image image = new Image() {
				Source = bitmap,
				Stretch = Stretch.None,
				Margin = new Thickness(0),
				Width = 790,
				Height = 718
			};
			canvas.Children.Add(image);
		}
	*/
	}
}
