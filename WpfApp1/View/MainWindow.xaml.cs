using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Controls;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GitControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :  Window
    {
        private bool isDraggingObject = false;
        private bool isPanningScene = false;
        private UIElement selectedObject = null;
        private Point lastMousePosition;

        public MainWindow()
        {
            InitializeComponent();

            //CreateSampleObjects();
        }

        //private void CreateSampleObjects()
        //{
        //    // Add some sample shapes to the canvas
        //    for (int i = 0; i < 5; i++)
        //    {
        //        var rectangle = new Rectangle
        //        {
        //            Width = 50,
        //            Height = 50,
        //            Fill = Brushes.Blue,
        //            Stroke = Brushes.Black,
        //            StrokeThickness = 2
        //        };

        //        Canvas.SetLeft(rectangle, 100 + i * 60);
        //        Canvas.SetTop(rectangle, 100);

        //        // Add MouseDown event to detect object dragging
        //        rectangle.MouseLeftButtonDown += Object_MouseLeftButtonDown;
        //        MainCanvas.Children.Add(rectangle);
        //    }
        //}

        //private void Object_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    isDraggingObject = true;
        //    selectedObject = sender as UIElement;
        //    lastMousePosition = e.GetPosition(MainCanvas);
        //    e.Handled = true; // Prevent triggering the canvas MouseDown event
        //}

        //private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Start panning the scene if no object is selected
        //    if (!isDraggingObject)
        //    {
        //        isPanningScene = true;
        //        lastMousePosition = e.GetPosition(this);
        //    }
        //}

        //private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (isDraggingObject && selectedObject != null)
        //    {
        //        Point currentMousePosition = e.GetPosition(MainCanvas);
        //        double offsetX = currentMousePosition.X - lastMousePosition.X;
        //        double offsetY = currentMousePosition.Y - lastMousePosition.Y;

        //        // Przesuń obiekt
        //        double left = Canvas.GetLeft(selectedObject);
        //        double top = Canvas.GetTop(selectedObject);
        //        Canvas.SetLeft(selectedObject, left + offsetX);
        //        Canvas.SetTop(selectedObject, top + offsetY);

        //        // Dopasuj rozmiar Canvas
        //        AdjustCanvasSize(selectedObject);

        //        lastMousePosition = currentMousePosition;
        //    }
        //}

        //private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // Stop dragging or panning
        //    isDraggingObject = false;
        //    isPanningScene = false;
        //    selectedObject = null;
        //}
        //private void AdjustCanvasSize(UIElement element)
        //{
        //    double elementRight = Canvas.GetLeft(element) + ((FrameworkElement)element).ActualWidth;
        //    double elementBottom = Canvas.GetTop(element) + ((FrameworkElement)element).ActualHeight;

        //    if (elementRight > MainCanvas.Width)
        //        MainCanvas.Width = elementRight + 50; // Dodaj margines
        //    if (elementBottom > MainCanvas.Height)
        //        MainCanvas.Height = elementBottom + 50; // Dodaj margines
        //}
        //private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    // Zoomowanie przy pomocy scrolla myszy
        //    Point mousePosition = e.GetPosition(MainCanvas);

        //    double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;

        //    // Skalowanie wokół punktu wskazanego przez użytkownika
        //    double scaleX = ScaleTransform.ScaleX;
        //    double scaleY = ScaleTransform.ScaleY;
        //    double newScaleX = scaleX * zoomFactor;
        //    double newScaleY = scaleY * zoomFactor;

        //    if (newScaleX >= 0.2 && newScaleX <= 10) // Zakres skalowania
        //    {
        //        ScaleTransform.ScaleX = newScaleX;
        //        ScaleTransform.ScaleY = newScaleY;

        //        // Przesunięcie tak, aby punkt pod myszą pozostał w tym samym miejscu
        //        TranslateTransform.X -= (mousePosition.X * (zoomFactor - 1));
        //        TranslateTransform.Y -= (mousePosition.Y * (zoomFactor - 1));
        //    }
        //}
    
    //public RelayCommand UpdateCommand { get; set; }
    //    public MainWindow()
    //    {
    //        InitializeComponent();
    //        UpdateCommand = new RelayCommand(UpdateCommandExecute);
    //    }

    //    private void UpdateCommandExecute()
    //    {
    //        //try to read user password from config file
    //        MessageBox.Show("Hello to you too!", "My App");

    //    }

    //    public bool OnlyTesterCanExecute()
    //    {
    //        bool canExecute = false;

    //        canExecute = true;

    //        return true;
    //    }

    //    private void Button_Click(object sender, RoutedEventArgs e)
    //    {
    //        string pass = pswBox.Password;
    //        var passBytes = Encoding.Default.GetBytes(pass);
    //        var passUtf8 = Encoding.UTF8.GetString(Encoding.Default.GetBytes(pass));
    //        var passUtf8Bytes = Encoding.UTF8.GetBytes(passUtf8);
    //        //Console.OutputEncoding = Encoding.UTF8;
    //        foreach (EncodingInfo ei in Encoding.GetEncodings())
    //        {
    //            Encoding es = ei.GetEncoding();

    //            Console.Write("{0,-6} {1,-25} ", ei.CodePage, ei.Name);
    //            Console.Write("{0,-6} ", es.WindowsCodePage);

    //            // Mark the ones that are different.
    //            if (ei.CodePage != es.WindowsCodePage)
    //                Console.Write("*");

    //            Console.WriteLine();
    //        }
    //        Console.WriteLine(Encoding.Default.WindowsCodePage);


    //    }
    }
}
