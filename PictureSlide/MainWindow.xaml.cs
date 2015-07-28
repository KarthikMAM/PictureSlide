using Microsoft.Win32;
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
using System.Windows.Threading;

namespace PictureSlide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image bufferImage;
        List<Image> imageList = new List<Image>();
        List<string> imageURL = new List<string>();
        bool areEnoughPictures = false, isPlaying = false;
        int sleepTime = 0;

        Key currentKey = Key.Right, playDirection = Key.Right;

        static double MARGIN_CHANGE_PER_CYCLE = 0.25;
        static double HEIGHT_CHANGE_PER_CYCLE = 0.25;
        static double OPACITY_CHANGE_PER_CYCLE = 0.0125;
        static double[] HEIGHT = { 50, 70, 90, 110, 130, 150 };
        static int SLEEP_INTERVAL = 300;


        DispatcherTimer animationTimer = new DispatcherTimer(DispatcherPriority.Send);

        public MainWindow()
        {
            InitializeComponent();

            imageList.Add(Image0);
            imageList.Add(Image1);
            imageList.Add(Image2);
            imageList.Add(Image3);

            animationTimer.Interval = TimeSpan.FromMilliseconds(15);
            animationTimer.Tick += animationTimer_Tick;
        }

        void animationTimer_Tick(object sender, EventArgs e)
        {
            if (sleepTime == 0)
            {
                foreach (var image in imageList)
                {
                    if (currentKey == Key.Right)
                    {
                        var margin = image.Margin;
                        margin.Left += MARGIN_CHANGE_PER_CYCLE;
                        if (margin.Left > HEIGHT[3] && margin.Left < HEIGHT[4])
                        {
                            image.Opacity -= OPACITY_CHANGE_PER_CYCLE;
                        }
                        else if (margin.Left < HEIGHT[1])
                        {
                            image.Opacity += OPACITY_CHANGE_PER_CYCLE;
                        }
                        image.Margin = margin;
                        image.Height -= HEIGHT_CHANGE_PER_CYCLE;
                        if (margin.Left > HEIGHT[4])
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                imageList[i].Opacity = 1;
                            }
                            imageList[3].Opacity = 0;

                            imageURL.Insert(0, image.Source.ToString());

                            animationTimer.Stop();
                        }
                    }
                    else if (currentKey == Key.Left)
                    {
                        var margin = image.Margin;
                        margin.Left -= MARGIN_CHANGE_PER_CYCLE;
                        if (margin.Left < HEIGHT[1] && margin.Left > HEIGHT[0])
                        {
                            image.Opacity -= OPACITY_CHANGE_PER_CYCLE;
                        }
                        else if (margin.Left > HEIGHT[3])
                        {
                            image.Opacity += OPACITY_CHANGE_PER_CYCLE;
                        }
                        image.Margin = margin;
                        image.Height += HEIGHT_CHANGE_PER_CYCLE;
                        if (margin.Left < HEIGHT[0])
                        {
                            imageList[0].Opacity = 0;
                            for (int i = 1; i < 4; i++)
                            {
                                imageList[i].Opacity = 1;
                            }

                            imageURL.Insert(imageURL.Count, image.Source.ToString());

                            animationTimer.Stop();
                        }
                    }
                }
            }
            else
            {
                sleepTime--;
            }
            if (isPlaying == true && animationTimer.IsEnabled == false)
            {
                if (sleepTime == 0)
                {
                    NextImage(playDirection);
                    sleepTime = SLEEP_INTERVAL;
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right || e.Key == Key.Left)
            {
                NextImage(e.Key);
            }
        }

        private void NextImage(Key direction)
        {
            if(isPlaying == true && animationTimer.IsEnabled == true)
            {
                playDirection = direction;
            }
            else if (areEnoughPictures == false)
            {
                OpenImages();
            }
            else if (animationTimer.IsEnabled == false)
            {
                switch (direction)
                {
                    case Key.Right:
                        {
                            if (currentKey == direction)
                            {
                                bufferImage = imageList.ElementAt(3);
                            }
                            else
                            {
                                bufferImage = imageList.ElementAt(0);
                            }

                            var bufferImageMargin = bufferImage.Margin;
                            bufferImageMargin.Left = HEIGHT[0];
                            bufferImage.Margin = bufferImageMargin;
                            //bufferImage.Opacity = 0;
                            bufferImage.Height = HEIGHT[5];

                            ImageGrid.Children.Remove(bufferImage);
                            ImageGrid.Children.Insert(3, bufferImage);
                            imageList.Remove(bufferImage);
                            imageList.Insert(0, bufferImage);

                            bufferImage.Source = new BitmapImage(new Uri(imageURL[imageURL.Count - 1]));
                            imageURL.RemoveAt(imageURL.Count - 1);

                            currentKey = direction;
                            animationTimer.Start();

                            break;
                        }
                    case Key.Left:
                        {
                            if (currentKey == direction)
                            {
                                bufferImage = imageList.ElementAt(0);
                            }
                            else
                            {
                                bufferImage = imageList.ElementAt(3);

                            }

                            var bufferImageMargin = bufferImage.Margin;
                            bufferImageMargin.Left = HEIGHT[4];
                            bufferImage.Margin = bufferImageMargin;
                            bufferImage.Height = HEIGHT[1];
                            bufferImage.Opacity = 0;

                            ImageGrid.Children.Remove(bufferImage);
                            ImageGrid.Children.Insert(0, bufferImage);
                            imageList.Remove(bufferImage);
                            imageList.Insert(3, bufferImage);

                            bufferImage.Source = new BitmapImage(new Uri(imageURL[0]));
                            imageURL.RemoveAt(0);

                            currentKey = direction;
                            animationTimer.Start();

                            break;
                        }
                }
            }
        }

        void OpenImages()
        {
            OpenFileDialog Open = new OpenFileDialog();
            Open.Title = "Select The Images";
            Open.DefaultExt = ".jpg";
            Open.Multiselect = true;
            Open.ShowDialog();
            if (Open.FileNames.Length > 0)
            {
                if (Open.FileNames.Length > 3)
                {
                    imageURL.Clear();
                    foreach (var URL in Open.FileNames)
                    {
                        imageURL.Add(URL);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        imageList[i].Source = new BitmapImage(new Uri(imageURL[0]));
                        imageURL.RemoveAt(0);
                    }

                    areEnoughPictures = true;
                }
                else
                {
                    MessageBox.Show("Sorry! We require 4 images to start a slideshow");
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType().ToString() == "System.Windows.Controls.Image")
            {
                switch ((sender as Image).Name)
                {
                    case "Open":
                        OpenImages();
                        break;
                    case "Play":
                        if (areEnoughPictures)
                        {
                            sleepTime = SLEEP_INTERVAL;
                            Play.Name = "Pause";
                            Play.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Resources\btn_pause_P.png"));
                            isPlaying = true;
                            playDirection = currentKey;
                            NextImage(playDirection);
                            break;
                        }
                        else
                        {
                            goto case "Open";
                        }
                    case "Right":
                        NextImage(Key.Right);
                        break;
                    case "Left":
                        NextImage(Key.Left);
                        break;
                    case "Close":
                        this.Close();
                        break;
                    case "Pause":
                        Play.Name = "Play";
                        Play.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + @"\Resources\btn_play_P.png"));
                        isPlaying = false;
                        break;
                }
            }
            else
            {
                this.DragMove();
            }
        }

        private void Buttons_MouseEnter(object sender, MouseEventArgs e)
        {
            Image buttonImage = sender as Image;
            string buttonSource = buttonImage.Source.ToString();
            buttonSource = buttonSource.Substring(0, buttonSource.Length - 5) + "H" + ".png";
            buttonImage.Source = new BitmapImage(new Uri(buttonSource));
        }

        private void Buttons_MouseLeave(object sender, MouseEventArgs e)
        {
            Image buttonImage = sender as Image;
            string buttonSource = buttonImage.Source.ToString();
            buttonSource = buttonSource.Substring(0, buttonSource.Length - 5) + "P" + ".png";
            buttonImage.Source = new BitmapImage(new Uri(buttonSource));
        }
    }
}