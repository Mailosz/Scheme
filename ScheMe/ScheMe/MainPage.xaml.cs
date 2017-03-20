using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScheMe
{
    public enum ToolType { Normal, AddNode, Line }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>


    public sealed partial class MainPage : Page
    {
        public ToolType Tool = ToolType.Normal;
        Scheme current;
        public Scheme Current
        {
            get { return current; }
            set
            {
                if (current != null)
                {

                }
                current = value;
                if (current != null)
                {
                    titleBox.Text = current.Name;
                    Invalidate();
                }
            }
        }

        public Canvas Canvas { get => mainCanvas; }

        public MainPage()
        {
            this.InitializeComponent();
            Current = new Scheme();

            UserAction.OnAction += Scheme_OnAction;
            UserAction.OnUndo += Scheme_OnUndo;

            KeyDown += MainPage_KeyDown;
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var n = Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftControl);
            if (n != CoreVirtualKeyStates.None)
            {
                switch (e.Key)
                {
                    case VirtualKey.N: //dodaj
                        Tool = ToolType.AddNode;

                        strokeInfoRect.Visibility = Visibility.Visible;
                        createNodeLabel.Visibility = Visibility.Visible;
                        titleBox.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void Scheme_OnAction(Scheme scheme, UserAction action)
        {
            if (scheme == Current)//to ten
            {

            }
        }

        private void Scheme_OnUndo(Scheme scheme, UserAction action)
        {
            if (scheme == Current)//to ten
            {

            }
        }

        public void Invalidate()
        {
            var t = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                mainCanvas.Children.Clear();

                titleBox.Text = current.Name;

                foreach (var node in Current.Nodes)
                {
                    mainCanvas.Children.Add(node);
                    node.UpdateLayout();
                }
                foreach (var con in Current.Connections)
                {
                    mainCanvas.Children.Add(con);
                    con.Invalidate();
                }
            });
        }




        private void createNodeButton_Click(object sender, RoutedEventArgs e)
        {
            Tool = ToolType.AddNode;

            strokeInfoRect.Visibility = Visibility.Visible;
            createNodeLabel.Visibility = Visibility.Visible;
            titleBox.Visibility = Visibility.Collapsed;
        }

        private void mainCanvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            switch (Tool)
            {
                case ToolType.Normal:
                    Node.SelectedNode = null;
                    Point p = e.GetPosition(titleBox);
                    if (p.X > 0 && p.Y > 0 && p.Y < titleBox.ActualHeight && p.X < titleBox.ActualWidth)
                    {
                        titleBox.SimulateTap(e);
                    }
                    break;
                case ToolType.AddNode:
                    Node.SelectedNode = null;
                    var act = new NodeCreationAction(e.GetPosition(mainCanvas).X -32, e.GetPosition(mainCanvas).Y-32);
                    act.Perform(Current, mainCanvas);
                    Tool = ToolType.Normal;
                    //
                    strokeInfoRect.Visibility = Visibility.Collapsed;
                    createNodeLabel.Visibility = Visibility.Collapsed;
                    titleBox.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Tool = ToolType.Normal;

            strokeInfoRect.Visibility = Visibility.Collapsed;
            createNodeLabel.Visibility = Visibility.Collapsed;
            titleBox.Visibility = Visibility.Visible;
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current.File == null)
            {
                Current.File = await PickFileForSave();
            }
            var b = Current.SaveAsync(Current.File);
        }

        public async Task<StorageFile> PickFileForSave()
        {
            FileSavePicker fsp = new FileSavePicker();
            fsp.FileTypeChoices.Add("ScheMe xml file",new List<string>() { ".scheme" });
            fsp.FileTypeChoices.Add("Xml file", new List<string>() { ".xml" });
            //fsp.FileTypeChoices.Add("SVG image", new List<string>() { ".svg" });
            fsp.SuggestedFileName = Current.Name;
            return await fsp.PickSaveFileAsync();
        }

        public async Task<StorageFile> PickFileForOpen()
        {
            FileOpenPicker fop = new FileOpenPicker();
            fop.FileTypeFilter.Add(".scheme");
            fop.FileTypeFilter.Add(".xml");
            return await fop.PickSingleFileAsync();
        }

        private async void openSchemeButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await PickFileForOpen();
            if (file != null)
            {
                var scheme = await Scheme.LoadAsync(file);

                if (scheme != null)
                {
                    Current = scheme;
                }
            }
        }

        //titlebox
        private void titleBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(titleBox, 20);
            titleBox.Opacity = 1;
        }

        private void titleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Current != null)
            {
                Current.Name = titleBox.Text;
            }
            Canvas.SetZIndex(titleBox, -10);
            titleBox.Opacity = 0.7;
        }
    }

    public class TitleBox : TextBox
    {
        public void SimulateTap(TappedRoutedEventArgs e)
        {
            Focus(FocusState.Pointer);
            OnTapped(e);
        }
    }

}
