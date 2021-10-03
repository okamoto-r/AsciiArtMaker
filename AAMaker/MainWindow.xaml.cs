using System;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Color = System.Drawing.Color;
using Brushes = System.Drawing.Brushes;

namespace AAMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Bitmap img, new_img, save_img; // 入力画像, AA画像, AA画像(サイズ圧縮)
        static Graphics graphics; // AA画像描画
        static Font font = new Font("MS　ゴシック", 7); // AA画像文字フォント
        static int width = 0, height = 0, expansion = 10; // 入力画像幅, 入力画像高さ, 文字間隔
        static char c; // AA文字
        static bool flag = false; // 文字数設定用フラグ
        static String path; // 入力画像パス

        //アスキー文字一覧
        static char[] ch = { ' ', '-', '\'', '.', '~', '_', '\"', '<', '!', '>', '`', ':', '+', '\\', '/', ';', '^',
            '=', '|', '?', '*', ']', 'L', '[', 'o', 'v', 'T', 'r', 'u', 'J', 'z', 'x', 'Y', '}', ',', 'c', '{', '7',
            't', '9', 'I', ')', 's', 'f', 'n', '4', '(', 'y', '1', 'V', 'F', 'U', 'l', 'a', 'H', 'k', '2', 'K', 'e',
            'X', 'Z', 'C', 'h', '%', 'A', '8', 'O', 'w', '3', 'N', 'E', 'P', 'i', 'q', 'j', 'G', 'b', '@', 'd', '5',
            'S', '#', 'D', '6', 'p', 'R', 'M', 'm', '&', 'W', 'g', 'B', '$', '0', 'Q' };

        //ASCII文字の白黒比の正規化
        public static double[] per = { 0.0, 0.17232899647703093, 0.20932889183438558, 0.23405507691234434, 0.2611706023928285,
            0.2864461613589592, 0.31501360354389757, 0.32087795179462136, 0.33689699675607804, 0.3382529910356134,
            0.3386148801841711, 0.35658289441557084, 0.36516359133558896, 0.39528759287034776, 0.3970490774006767,
            0.39829606892462244, 0.4018277582057274, 0.4113415187135931, 0.44405978583138583, 0.44771791830897484,
            0.4543670863999442, 0.4609246921762182, 0.4813082074714849, 0.5010202657923193, 0.505376015905682,
            0.5088379434232098, 0.518787714953434, 0.5254892043670863, 0.5325220621577313, 0.5335466880602743,
            0.5434659388189333, 0.5446780494610903, 0.5486283093236597, 0.5495308521399421, 0.5539694443475531,
            0.5557658097596707, 0.5588745683490879, 0.5630297533921658, 0.5743529596428198, 0.5782814189542712,
            0.5938208517911333, 0.5978975548501867, 0.5985210506121594, 0.6140692036694688, 0.617356726778053,
            0.618494715546409, 0.6226542606997105, 0.6286886532491541, 0.6327958770797726, 0.638293976071715,
            0.6397633332170637, 0.6470665178415711, 0.6660286372039484, 0.6812977432069482, 0.6902141686141826,
            0.6994619623984095, 0.7105933237992256, 0.7281602078900554, 0.729577243712721, 0.7326162405385608,
            0.7329214482542119, 0.7408786494122571, 0.7479681886358087, 0.7513995953817714, 0.7527294289999652,
            0.7530215563849454, 0.7556419826293209, 0.7630280093480764, 0.770549199483763, 0.7736056367504971,
            0.7875536293557501, 0.7931476507726115, 0.7964656946527608, 0.7985498273396351, 0.7988899159370749,
            0.802050995849175, 0.8084952387596359, 0.809737870173358, 0.811329310404967, 0.8224475914751126,
            0.8311111304893788, 0.8533433325194462, 0.8653379957445324, 0.873029230178939, 0.8833234504168266,
            0.8858217935749416, 0.8941975653144512, 0.8947818200844118, 0.9024730545188183, 0.9195734068157243,
            0.9318340367644494, 0.9539615961491507, 0.9712930342879068, 0.9736823746904323, 1.0 };

        public MainWindow()
        {
            InitializeComponent();
            EnableDragAndDrop(ImageRectangle);
        }

        // 画像ドラッグアンドドロップ設定
        private void EnableDragAndDrop(System.Windows.Shapes.Rectangle control)
        {
            control.AllowDrop = true;
            control.PreviewDragOver += (s, e) =>
            {
                e.Effects = (e.Data.GetDataPresent(DataFormats.FileDrop)) ? DragDropEffects.Copy : e.Effects = DragDropEffects.None;
                e.Handled = true;
            };

            control.PreviewDrop += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    width = 0;
                    height = 0;

                    path = (e.Data.GetData(DataFormats.FileDrop) as String[])[0];
                    if (Path.GetExtension(path) != ".jpg" && Path.GetExtension(path) != ".png" && Path.GetExtension(path) != ".gif")
                    {
                        MessageBox.Show("jpg, png, gif ではありません");
                        return;
                    }

                    OutputTextBox.Text = Path.GetFileNameWithoutExtension(path);

                    img = Bitmap.FromFile(path) as Bitmap;
                    DragAndDrop.Source = new BitmapImage(new Uri(path));

                    width = img.Width;
                    WidthTextBox.Text = String.Concat(width);
                    height = img.Height;
                    HeightTextBox.Text = String.Concat(height);
                }
            };
        }

        // 画像リセットボタン設定
        private void ImageResetButton_Click(object sender, RoutedEventArgs e)
        {
            DragAndDrop.Source = null;
            WidthTextBox.Text = "";
            HeightTextBox.Text = "";
            OutputTextBox.Text = "";

            img.Dispose();
        }

        // ピクセル数入力時の挙動
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (img == null)
            {
                MessageBox.Show("初めに画像を入力してください");
                e.Handled = true;
            }

            if (!Regex.IsMatch(e.Text, @"[0-9]")) e.Handled = true;
        }

        // 幅入力時の挙動
        private void WidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (img == null) return;

            if (flag)
            {
                flag = false;
                return;
            }


            if (WidthTextBox.Text == "")
            {
                flag = true;
                HeightTextBox.Text = "";
                return;
            }

            flag = true;
            HeightTextBox.Text = String.Concat(height * Int32.Parse(WidthTextBox.Text) / width);
        }

        // 高さ入力時の挙動
        private void HeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (img == null) return;

            if (flag)
            {
                flag = false;
                return;
            }

            if (HeightTextBox.Text == "")
            {
                flag = true;
                WidthTextBox.Text = "";
                return;
            }

            flag = true;
            WidthTextBox.Text = String.Concat(width * Int32.Parse(HeightTextBox.Text) / height);
        }

        // ピクセル数リセット
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            WidthTextBox.Text = String.Concat(width);
            HeightTextBox.Text = String.Concat(height);
        }

        // 変換ボタン設定
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (img == null)
            {
                MessageBox.Show("画像を入力してください");
                return;
            }

            if (OutputTextBox.Text == "")
            {
                MessageBox.Show("出力ファイル名を入力してください");
                return;
            }

            if (WidthTextBox.Text == "" || HeightTextBox.Text == "")
            {
                MessageBox.Show("文字数を入力してください");
                return;
            }
            
            if (!Regex.IsMatch(WidthTextBox.Text, @"^[1-9][0-9]*") || !Regex.IsMatch(HeightTextBox.Text, @"^[1-9][0-9]*"))
            {
                MessageBox.Show("文字数は自然数です");
                return;
            }
            
            if (Int32.Parse(WidthTextBox.Text) > 1000 || Int32.Parse(HeightTextBox.Text) > 1000)
            {
                MessageBoxResult result = MessageBox.Show("画像サイズが大きくなりますがよろしいですか？\n(文字数は幅・高さともに 1000 以下を推奨します)", "警告", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel) return;
            }

            if (Button.IsEnabled)
            {
                ContentsIsEnabele();

                make();

                ContentsIsEnabele();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (img != null) img.Dispose();
            if (new_img != null) new_img.Dispose();
            if (save_img != null) save_img.Dispose();
            if (graphics != null) graphics.Dispose();

            font.Dispose();
        }

        // IsEnable の切り替え
        private void ContentsIsEnabele()
        {
            ImageRectangle.AllowDrop = !ImageRectangle.AllowDrop;
            Button.IsEnabled = !Button.IsEnabled;
            TypePanel.IsEnabled = !TypePanel.IsEnabled;
            SizePanel.IsEnabled = !SizePanel.IsEnabled;
            OutputTextBox.IsEnabled = !OutputTextBox.IsEnabled;
        }

    

        // 使用文字の選択
        private char select_char(double x)
        {

            int i = 0;

            while (per[i] < x) i++;

            if (i == 0 || ((bool)ColorButton.IsChecked && i == 1)) return ch[i];

            return per[i] - x < x - per[i - 1] ? ch[i] : ch[i - 1];

        }

        // アスキーアート変換
        private void make()
        {
            width = Int32.Parse(WidthTextBox.Text);
            height = Int32.Parse(HeightTextBox.Text);
            img = new Bitmap(img, new System.Drawing.Size(width, height));

            try
            {
                new_img = new Bitmap(width * expansion, height * expansion + expansion, PixelFormat.Format24bppRgb);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("文字数が多すぎます");
                return;
            }

            graphics = Graphics.FromImage(new_img);
            graphics.FillRectangle(System.Drawing.Brushes.White, graphics.VisibleClipBounds);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = img.GetPixel(x, y);

                    c = select_char(1 - (color.R + color.G + color.B) / (3 * 256.0));

                    if ((bool)ColorButton.IsChecked) graphics.DrawString(c.ToString(), font, new SolidBrush(color), x * expansion, y * expansion + 10);
                    else graphics.DrawString(c.ToString(), font, Brushes.Black, x * expansion, y * expansion + 10);
                }

            }

            if ((bool)ColorButton.IsChecked) save_img = new_img.Clone(new RectangleF(0, 0, width * expansion, height * expansion), PixelFormat.Format8bppIndexed);
            else save_img = new_img.Clone(new RectangleF(0, 0, width * expansion, height * expansion), PixelFormat.Format1bppIndexed);

            save_img.Save(Path.GetDirectoryName(path) + "/" + OutputTextBox.Text + "_" + ((bool)ColorButton.IsChecked ? "color" : "monochrome") + ".png");
        }
    }
}
