using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RevitMainTool.UI
{
    /// <summary>
    /// Interaction logic for SpaceTagsFixedDistance.xaml
    /// </summary>
    public partial class SpaceTagsFixedDistance : UserControl
    {
        public UIDocument UIDoc;
        public Document doc;

        ExternalEvent M_exEvent { get; set; }
        EventPlaceTagsFixedDistance MyEvent { get; set; }

        public SpaceTagsFixedDistance(UIDocument UIdocument, ExternalEvent exEvent, EventPlaceTagsFixedDistance handler)
        {
            InitializeComponent();

            UIDoc = UIdocument;
            doc = UIDoc.Document;
            M_exEvent = exEvent;
            MyEvent = handler;


            Window AWindow = new Window()
            {
                Name = "AWindow",
                Content = Content,
                SizeToContent = SizeToContent.Manual,
                Height = 450,
                Width = 450,
            };

            IntPtr revitWindowHandle = Process.GetCurrentProcess().MainWindowHandle;

            WindowInteropHelper helper = new WindowInteropHelper(AWindow);
            helper.Owner = revitWindowHandle;
            AWindow.Show();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            string stringDistance = textBoxDistance.Text;
            double numberDistance;
            var testing = double.TryParse(stringDistance, out numberDistance);

            if (testing)
            {
                Global.EventDistance = numberDistance / 304.8;
            }



            if ((lstBoxItemHorizontalOrVertical.SelectedItem as ComboBoxItem).Content.ToString() == "Vertical")
            {
                Global.EventTagOrientation = TagOrientation.Vertical;
            }
            else
            {
                Global.EventTagOrientation = TagOrientation.Horizontal;
            }

            string alignTo = (lstBoxItemAlignTo.SelectedItem as ComboBoxItem).Content.ToString();

            if (alignTo == "Left")
            {
                Global.EventTagAlignment = Alignment.Left;
            }
            else if (alignTo == "Center")
            {
                Global.EventTagAlignment = Alignment.Center;
            }
            else if (alignTo == "Right")
            {
                Global.EventTagAlignment = Alignment.Right;
            }

            List<IndependentTag> tags = new List<IndependentTag>();

            var elementIds = UIDoc.Selection.GetElementIds();
            foreach (var elementId in elementIds)
            {
                Element ele = doc.GetElement(elementId);
                if (ele is IndependentTag)
                {
                    tags.Add((IndependentTag)ele);
                }
            }

            if (tags.Count > 0 && testing)
            {
                Global.EventTags = tags;
                M_exEvent.Raise();
            }
            else
            {
                TaskDialog.Show("No tags selected", "There are no tags selected");
            }


        }
    }
}
