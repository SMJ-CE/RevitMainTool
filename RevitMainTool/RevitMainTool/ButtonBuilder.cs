using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RevitMainTool
{
    internal class ButtonBuilder
    {
        private string _name;
        private string _text = string.Empty;
        private string _assemblyName = Assembly.GetExecutingAssembly().Location;
        private string _imagePath = string.Empty;
        private string _className;
        private string _tooltip = string.Empty;
        private PushButton _pushButton;

        public ButtonBuilder(string name, Type cmdType)
        {
            _name = name;
            _className = cmdType.FullName;
        }

        public ButtonBuilder Text(string text)
        {
            _text = text;
            return this;
        }

        public ButtonBuilder ImagePath(string imagePath)
        {
            _imagePath = imagePath;
            return this;
        }

        public ButtonBuilder Tooltip(string tooltip)
        {
            _tooltip = tooltip;
            return this;
        }

        public void Build(RibbonPanel panel)
        {
            var buttonData = new PushButtonData(
                _name,
                _text,
                _assemblyName,
                _className);

            _pushButton = panel.AddItem(buttonData) as PushButton;
            if (!string.IsNullOrEmpty(_tooltip))
            {
                _pushButton.ToolTip = _tooltip;
            }

            //image needs to be DPI 96 and 32x32 for perfect size
            if (!string.IsNullOrEmpty(_imagePath))
            {
                BitmapImage pb1Image = new BitmapImage(new Uri(_imagePath));
                _pushButton.LargeImage = pb1Image;
            }
        }

        public void Build(SplitButton splitButton)
        {
            var buttonData = new PushButtonData(
                _name,
                _text,
                _assemblyName,
                _className);

            _pushButton = splitButton.AddPushButton(buttonData);
            if (!string.IsNullOrEmpty(_tooltip))
            {
                _pushButton.ToolTip = _tooltip;
            }

            //image needs to be DPI 96 and 32x32 for perfect size
            if (!string.IsNullOrEmpty(_imagePath))
            {
                BitmapImage pb1Image = new BitmapImage(new Uri(_imagePath));
                _pushButton.LargeImage = pb1Image;
            }
        }


        public PushButton GetPushButton()
        {
            return _pushButton;
        }
    }
}
