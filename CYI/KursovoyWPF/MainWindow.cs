using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KursovoyWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string full_name_of_image;
        private string new_full_name;
        private string tmp_Image;
        private Bitmap bmp1;
        private Bitmap bmp2;
        private BitmapImage bmp_I;

        public static Image global_sender;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            pictureBox2.Source = null;
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG; *.DNG;)|*.BMP;*.JPG;*.GIF;*.PNG;*.DNG;";
            if (open_dialog.ShowDialog() == true)
            {
                try
                {
                    full_name_of_image = open_dialog.FileName;//Записываем полное расположение нашего изображения

                    BitmapImage bmpdec = new BitmapImage();//BitmapImage способен работать с форматами Raw
                    bmpdec.BeginInit();
                    bmpdec.UriSource = new Uri(full_name_of_image);
                    pictureBox1.Source = bmpdec;
                    bmpdec.EndInit();

                    bmp1 = BitmapImage2Bitmap(bmpdec);//Конвертируем наше изображение в обычный Битмап

                    tmp_Image = @"C:\Users\Public\Pictures\tmp_Image.jpg";
                    bmp1.Save(tmp_Image, ImageFormat.Jpeg);//Создаем временное изображение в формате BMP, этот тип поддерживается обычным битмапом.
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть файл",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Конвертируем из типа BitmapImage(может читать RAW, DNG...) в тип Bitmap (Позволяет без запар сжимать и изменять изображения)
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        //Конвертация из Bitmap в BitmapImage
        public BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        //Сохраняем обработанное изображение в нужном нам формате
        private void Menu_Save_As(object sender, RoutedEventArgs e)
        {
            if (pictureBox2.Source != null)
            {
                SaveFileDialog savedialog = new SaveFileDialog();

                savedialog.Title = "Сохранить картинку как ...";
                savedialog.OverwritePrompt = true;
                savedialog.CheckPathExists = true;
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF) | *.GIF | Image Files(*.PNG) | *.PNG | All files(*.*) | *.* ";

                if (savedialog.ShowDialog() == true)
                {
                    try
                    {
                        bmp2.Save(savedialog.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Menu_About(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Программа для сжатия изображений.\n\n" +
                "В случае заедания ползунка, следует \n" +
                "нажать на саму шкалу посередине");
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (pictureBox1.Source != null)
            {

                try
                {
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    Encoder myEncoder = Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, (long)trackBar1.Value);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    new_full_name = tmp_Image.Remove(tmp_Image.Length - 4) + "_Cropped.jpg";

                    try
                    {
                        bmp1.Save(new_full_name, jpgEncoder, myEncoderParameters);
                    }
                    catch
                    {
                        bmp2.Dispose();

                        File.Delete(new_full_name);
                        bmp1.Save(new_full_name, jpgEncoder, myEncoderParameters);
                    }

                    //BitmapImage способен работать с форматами Raw
                    bmp2 = new Bitmap(new_full_name);
                    bmp_I = ToBitmapImage(bmp2);
                    pictureBox2.Source = bmp_I;
                    //long length = new System.IO.FileInfo(path).Length;

                    float lenght_of_image1 = new FileInfo(full_name_of_image).Length;
                    float lenght_of_image2 = new FileInfo(new_full_name).Length;



                    LSize1.Content = (lenght_of_image1 / 1024).ToString("0.00");
                    LSize2.Content = (lenght_of_image2 / 1024).ToString("0.00");
                }
                catch
                {
                    MessageBox.Show("Невозможно обработать изображение", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Отсутствует обрабатываемое изображение", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
