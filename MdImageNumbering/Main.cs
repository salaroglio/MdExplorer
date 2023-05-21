using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Text.Json;

namespace MdImageNumbering
{
    public enum CurrentState
    {
        Numbering,
        Selecting,
        Moving,
        None
    }

    public partial class Main : Form
    {
        private Point _offset;
        private Image _imageMask;
        private int _counter = 1;
        private Button _numberingButton;
        private Button _movingButton;
        private int _shiftOfBox = 20;
        private CurrentState _currentState = CurrentState.None;
        private readonly string?[] _args;
        private string _fullPathFile = string.Empty;

        public Main(string?[] args)
        {
            InitializeComponent();
            _args = args;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Construct an image object from a file in the local directory.
            // ... This file must exist in the solution.
             _fullPathFile = Path.GetFullPath(_args[0].ToString());            
            
            if (_args.Count() > 0)
            {
                _imageMask = Image.FromFile(_fullPathFile);
            }
            else
            {
                this.Close();
            }

            // Set the PictureBox image property to this image.
            // ... Then, adjust its height and width properties.
            pictureBox1.Image = _imageMask;
            pictureBox1.Height = _imageMask.Height;
            pictureBox1.Width = _imageMask.Width;
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            _currentState = CurrentState.Numbering;
            CreateNewButton();
            _numberingButton.Visible = false;
            this.Refresh();

        }

        private void CreateNewButton()
        {
            _numberingButton = new Button();
            _numberingButton.Text = _counter.ToString();
            _numberingButton.Width = _shiftOfBox * 2;
            _numberingButton.Height = _shiftOfBox * 2;
            _numberingButton.AllowDrop = true;
            _numberingButton.MouseEnter += _currentButton_MouseEnter;
            _numberingButton.MouseLeave += _currentButton_MouseLeave;
            _numberingButton.MouseDown += _currentButton_MouseDown;
            _numberingButton.MouseUp += _currentButton_MouseUp;
            _numberingButton.MouseMove += _numberingButton_MouseMove;

            pictureBox1.Controls.Add(_numberingButton);
            _numberingButton.BringToFront();
        }

        private void _numberingButton_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_currentState == CurrentState.Moving)
            {
                _movingButton.Left = e.X + _movingButton.Left - _offset.X;
                _movingButton.Top = e.Y + _movingButton.Top - _offset.Y;
            }
        }

        private void _currentButton_MouseUp(object? sender, MouseEventArgs e)
        {
            _currentState = CurrentState.Selecting;
        }

        private void _currentButton_MouseDown(object? sender, MouseEventArgs e)
        {
            _currentState = CurrentState.Moving;
            _offset = e.Location;
        }

        private void _currentButton_MouseLeave(object? sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
            _numberingButton.Visible = true;
            _currentState = CurrentState.Numbering;
        }

        private void _currentButton_MouseEnter(object? sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
            _numberingButton.Visible = false;
            _currentState = CurrentState.Selecting;
            _movingButton = sender as Button;
        }

        

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (_currentState == CurrentState.Numbering)
            {
                _counter++;
                CreateNewButton();
            }

        }

        private void InsertNewBox(int x, int y, string text)
        {
            //var mouseEventArgs = e as MouseEventArgs;

            var boxXStart = x - _shiftOfBox;
            var boxYStart = y - _shiftOfBox;
            var boxXEnd = x + _shiftOfBox;
            var boxYEnd = y + _shiftOfBox;

            RectangleF srcRect = new RectangleF(boxXStart, boxYStart, boxXEnd, boxYEnd);
            var units = GraphicsUnit.Pixel;

            var graphicsMask = Graphics.FromImage(_imageMask);
            // Draw image to screen.
            DrawBox(boxXStart, boxYStart, boxXEnd, boxYEnd, srcRect, units, graphicsMask, _shiftOfBox, text);

            pictureBox1.Refresh();
        }

        private void DrawBox(int boxXStart, int boxYStart, int boxXEnd, int boxYEnd,
            RectangleF srcRect, GraphicsUnit units, Graphics graphicsMask, int shiftOfBox, string text)
        {

            SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 255));

            Pen pen = new Pen(Color.FromArgb(255, 255, 255, 255), 3);
            graphicsMask.FillRectangle(brush, boxXStart, boxYStart, shiftOfBox * 2, shiftOfBox * 2);
            graphicsMask.DrawLine(pen, boxXStart, boxYStart, boxXEnd, boxYStart);
            graphicsMask.DrawLine(pen, boxXEnd, boxYStart, boxXEnd, boxYEnd);
            graphicsMask.DrawLine(pen, boxXStart, boxYEnd, boxXEnd, boxYEnd);
            graphicsMask.DrawLine(pen, boxXStart, boxYEnd, boxXStart, boxYStart);

            var drawString = text;
            Font drawFont = new System.Drawing.Font("Arial", 16);
            StringFormat drawFormat = new System.Drawing.StringFormat();
            SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            graphicsMask.DrawString(drawString, drawFont, drawBrush, boxXStart, boxYStart, drawFormat);

        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            if (_numberingButton == null)
            {
                return;
            }
            if (!_numberingButton.Visible)
            {
                _numberingButton.Visible = true;
            }

        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_numberingButton == null)
            {
                return;
            }
            if (_currentState == CurrentState.Numbering)
            {
                setPositionOfButton(e, _numberingButton);
            }
        }

        private void setPositionOfButton(MouseEventArgs e, Button localButton)
        {
            var mouseEventArgs = e;
            localButton.Top = mouseEventArgs.Y - (_shiftOfBox * 2 + 5);
            localButton.Left = mouseEventArgs.X - (_shiftOfBox * 2 + 5);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

            foreach (Button item in pictureBox1.Controls.OfType<Button>())
            {
                InsertNewBox(item.Left + _shiftOfBox, item.Top + _shiftOfBox, item.Text);
            }
            pictureBox1.Controls.Clear();

            this.Refresh();
            var fullDirectory = Path.GetDirectoryName(_fullPathFile);
            var fileName = Path.GetFileNameWithoutExtension(_fullPathFile);
            var numberedImageFullPath = fileName + "_numbered.png";
            _imageMask.Save(fullDirectory + Path.DirectorySeparatorChar + numberedImageFullPath);
            //SendToMdExplorer(numberedImageFullPath);
        }

        private void SendToMdExplorer(string numberedImageFullPath)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(_args[1].ToString());
            var request = new HttpRequestMessage(HttpMethod.Post, "mdfiles/SetNumberedImage");
            var requestBody = JsonSerializer.Serialize(new
            {
                FullPath = _fullPathFile,
                MarkdownToReplace = _args[2].ToString(),
                NumberedImageFullPath = numberedImageFullPath
            });
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.SendAsync(request).Result;
         
        }
    }
}