using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
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
			pixels = new byte[bitmap.PixelWidth][][];
			for (int i = 0; i < bitmap.PixelWidth; i++) {
				pixels[i] = new byte[bitmap.PixelHeight][];
				for (int j = 0; j < bitmap.PixelHeight; j++)
					pixels[i][j] = new byte[4];
			}
			height = bitmap.PixelHeight;
			width = bitmap.PixelWidth; //wymiary bitmapy
			pgBar.Maximum = height;
		}

		WriteableBitmap bitmap;
		int width, height, iterations = 255;
		byte[][][] pixels;
		double offX = 0, offY = 0, zoom = 1, actualZoom = 1, actualOffX = 0, actualOffY = 0;
		Image image;
		bool isCalcOn = false;

		private void clrBtn_Click(object sender, RoutedEventArgs e) {
			canvas.Children.Clear();
			pgBar.Value = 0;
		}

		#region zoom
		private void zoomTxtBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);

		}
		private void zoomTxtBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
			zoom *= Math.Pow(10, e.Delta / 120);
			zoomTxtBox.Text = zoom.ToString();
		}

		private void zoomBtnPLus_Click(object sender, RoutedEventArgs e) {
			zoom = double.Parse(zoomTxtBox.Text) * 10;
			zoomTxtBox.Text = zoom.ToString();
		}

		private void zoomBtnMinus_Click(object sender, RoutedEventArgs e) {
			zoom = double.Parse(zoomTxtBox.Text) / 10;
			zoomTxtBox.Text = zoom.ToString();
		}
		#endregion

		#region offX & offY txtboxes

		private void upBtn_Click(object sender, RoutedEventArgs e) {
			offY = double.Parse(offYTxtBox.Text) + 1 / zoom;
			offYTxtBox.Text = offY.ToString();
		}

		private void downBtn_Click(object sender, RoutedEventArgs e) {
			offY = double.Parse(offYTxtBox.Text) - 1 / zoom;
			offYTxtBox.Text = offY.ToString();
		}

		private void leftBtn_Click(object sender, RoutedEventArgs e) {
			offX = double.Parse(offXTxtBox.Text) - 1 / zoom;
			offXTxtBox.Text = offX.ToString();
		}



		private void rightBtn_Click(object sender, RoutedEventArgs e) {
			offX = double.Parse(offXTxtBox.Text) + 1 / zoom;
			offXTxtBox.Text = offX.ToString();
		}
		private void offXTxtBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void offYTxtBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		#endregion


		private void saveImageAs_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			SaveFileDialog dialog = new SaveFileDialog();
			if (dialog.ShowDialog() == true) {
				using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create)) {
					PngBitmapEncoder encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(bitmap));
					encoder.Save(stream);
				}
			}
		}

		private void iterationsTxtBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
			Regex regex = new Regex("[^0-9.-]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void canvas_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			offX = (e.GetPosition(canvas).X - canvas.Width / 2) / (zoom * canvas.Width / 4) + actualOffX;
			offY = -(e.GetPosition(canvas).Y - canvas.Height / 2) / (zoom * canvas.Height / 4) + actualOffY;

			offXTxtBox.Text = offX.ToString();
			offYTxtBox.Text = offY.ToString();
		}

		byte checkConvergence(double[] c, int acc) {
			double[] z = { 0, 0 };       //z0
			double[] oldZ = new double[2];
			for (int i = 0; i < acc; i++) {
				if (z[0] * z[0] + z[1] * z[1] > 4) {
					return (byte)(255 * i / acc);
				}
				oldZ[0] = z[0];
				oldZ[1] = z[1];

				z[0] = oldZ[0] * oldZ[0] - oldZ[1] * oldZ[1] + c[0];
				z[1] = 2 * oldZ[0] * oldZ[1] + c[1];                    //zN+1 = zN^2 + c
			}
			return 255;
		}

		private void CalcButton_Click(object sender, RoutedEventArgs e) {
			if (!isCalcOn) {
				isCalcOn = true;
				int count = 0;
				actualZoom = double.Parse(zoomTxtBox.Text);
				actualOffX = double.Parse(offXTxtBox.Text);
				actualOffY = double.Parse(offYTxtBox.Text);
				pgBar.Value = 0;
				//var s = Stopwatch.StartNew();
				double[] z = new double[2];
				for (int row = 0; row < height; row++) {
					for (int col = 0; col < width; col++) {

						z[0] = (double)(col - width / 2) / (actualZoom * 500d) + offX;
						z[1] = -(double)(row - height / 2) / (actualZoom * 500d) + offY;

						pixels[row][col][3] = (byte)checkConvergence(z, iterations);           //nadaje kolor (alpha) w zaleznosci od ilosci iteracji 
					}
					count++;
					if (count % 10 == 0) pgBar.Dispatcher.Invoke(() => pgBar.Value = count, DispatcherPriority.Background);
				}
				//s.Stop();
				//System.Console.WriteLine(s.ElapsedMilliseconds);
				/*for (int i = -7; i <= 7; i++) {//iksik
					pixels[width / 2 + i, height / 2 + i, 2] = 0xFF;
					pixels[width / 2 + i, height / 2 - i, 2] = 0xFF;
				}*/
				byte[] pixels1d = new byte[width * height * 4];
				int index = 0;
				for (int row = 0; row < height; row++) {
					for (int col = 0; col < width; col++) {
						for (int i = 0; i < 4; i++)
							pixels1d[index++] = pixels[row][col][i];
					}
				}

				Int32Rect rect = new Int32Rect(0, 0, width, height);
				int stride = 4 * width;
				bitmap.WritePixels(rect, pixels1d, stride, 0);

				image = new Image() {
					Source = bitmap,
					Stretch = Stretch.Fill,
					Margin = new Thickness(0),
					Width = 700,
					Height = 700
				};
				canvas.Children.Clear();
				canvas.Children.Add(image);
				isCalcOn = false;
			}
		}
	}
}
