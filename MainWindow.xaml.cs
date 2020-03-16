using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
			pgBar.Maximum = height;
		}

		WriteableBitmap bitmap;
		int width, height, iterations = 255;
		byte[,,] pixels;
		double offX = 0, offY = 0, zoom = 1;

		private void offYTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
			double oldOffY = offY;
			if (!double.TryParse(offYTxtBox.Text, out offY)) {
				offY = oldOffY;
				MessageBox.Show("Don't.");
			}
		}

		private void zoomBtnPLus_Click(object sender, RoutedEventArgs e) {
			zoom *= 10;
			zoomTxtBox.Text = zoom.ToString();
		}

		private void zoomBtnMinus_Click(object sender, RoutedEventArgs e) {
			zoom /= 10;
			zoomTxtBox.Text = zoom.ToString();
		}

		private void clrBtn_Click(object sender, RoutedEventArgs e) {
			canvas.Children.Clear();
		}

		private void upBtn_Click(object sender, RoutedEventArgs e) {
			offY += 1 / zoom;
			offYTxtBox.Text = offY.ToString();
		}

		private void downBtn_Click(object sender, RoutedEventArgs e) {
			offY -= 1 / zoom;
			offYTxtBox.Text = offY.ToString();
		}

		private void leftBtn_Click(object sender, RoutedEventArgs e) {
			offX -= 1 / zoom;
			offXTxtBox.Text = offX.ToString();
		}

		private void rightBtn_Click(object sender, RoutedEventArgs e) {
			offX += 1 / zoom;
			offXTxtBox.Text = offX.ToString();
		}

		private void iterationsTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
			int oldIterations = iterations;
			if (!int.TryParse(iterationsTxtBox.Text, out iterations)) {
				iterations = oldIterations;
				MessageBox.Show("Don't.");
			}
		}

		private void zoomTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
			double oldZoom = zoom;
			if (!double.TryParse(zoomTxtBox.Text, out zoom)) {
				zoom = oldZoom;
				MessageBox.Show("Don't.");
			}
		}

		private void offXTxtBox_TextChanged(object sender, TextChangedEventArgs e) {
			double oldOffX = offX;
			if (!double.TryParse(offXTxtBox.Text, out offX)) {
				offX = oldOffX;
				MessageBox.Show("Don't.");
			}
		}

		byte checkConvergence(double[] c, int acc) {
			double[] z = { 0, 0 };       //z0
			int count = 0;
			for (int i = 0; i < acc; i++) {
				if (z[0] * z[0] + z[1] * z[1] > 4) {
					return (byte)(255 * i / acc);
				}
				double[] oldZ = new double[2];
				oldZ[0] = z[0];
				oldZ[1] = z[1];
				//bo Zuze
				z[0] = oldZ[0] * oldZ[0] - oldZ[1] * oldZ[1] + c[0];
				z[1] = 2 * oldZ[0] * oldZ[1] + c[1];      //zN+1 = zN^2 + c
				count++;
			}
			return 255;
		}

		private void CalcButton_Click(object sender, RoutedEventArgs e) {

			pgBar.Value = 0;
			int count = 0;
			for (int row = 0; row < height; row++) {
				for (int col = 0; col < width; col++) {
					pixels[row, col, 0] = 0;
					pixels[row, col, 1] = 0;
					pixels[row, col, 2] = 0;

					double[] z = new double[2];
					z[0] = (double)(col - width / 2) / (zoom * 200d) + offX;
					z[1] = -(double)(row - height / 2) / (zoom * 200d) + offY;

					pixels[row, col, 3] = (byte)checkConvergence(z, iterations);           //nadaje kolor (alpha) w zaleznosci od 
																						   //ilosci iteracji 
				}
				count++;
				pgBar.Dispatcher.Invoke(() => pgBar.Value = count, DispatcherPriority.Background);
			}


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
				Stretch = Stretch.Fill,
				Margin = new Thickness(0),
				Width = 700,
				Height = 700
			};
			canvas.Children.Clear();
			canvas.Children.Add(image);
		}
	}
}
