using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DockPanelTest
{
    public class Drawable
    {
        public Drawable parent;

        public Color color = Color.Red;
        public int margin = 5;
        public int padding = 5;

        public int x, y;
        public int width, height;
        public int maxWidth, maxHeight;

        public int clientX { get { return x + margin + padding; } }
        public int clientY { get { return y + margin + padding; } }
        public int clientWidth { get { return width - padding * 2 - margin * 2; } }
        public int clientHeight { get { return height - padding * 2 - margin * 2; } }

        public bool computed = false;

        public virtual void Draw(Graphics g)
        {
            g.FillRectangle(WindowManager.instance.marginColor, new Rectangle(x, y, width, height));
            g.FillRectangle(WindowManager.instance.paddingColor, new Rectangle(x + padding, y + padding, width - padding - padding, height - padding - padding));
            //g.DrawRectangle(WindowManager.instance.borderColor, new Rectangle(x + padding, y + padding, width - padding * 2, height - padding * 2));
            g.FillRectangle(WindowManager.instance.backgroundColor, new Rectangle(x + padding + margin, y + padding + margin, width - padding * 2 - margin * 2, height - padding * 2 - margin * 2));
        }

        public void Update(int x, int y, int width, int height)
        {
            //this.x = x;
            //this.y = y;
            //this.width = width;
            //this.height = height;
        }

        public virtual void computeMaxSize(ref int _width, ref int _height)
        {
            if (parent != null)
                parent.computeMaxSize(ref _width, ref _height);
        }


        public void SetParent(Drawable parent)
        {
            this.parent = parent;
        }
    }

    public class DockPane : Drawable
    {
        public string name;

        public override void Draw(Graphics g)
        {
            if (parent != null)
                parent.Draw(g);

            base.Draw(g);
            g.DrawString(name, WindowManager.instance.font, Brushes.White, clientX, clientY);
        }
     
    }

    public class DocumentContainer : DockPane
    {
        public DocumentContainer()
        {
            computed = true;
        }

        public override void computeMaxSize(ref int _width, ref int _height)
        {
            base.computeMaxSize(ref _width, ref _height);

            width = WindowManager.instance.maxWidth - _width;
            height = WindowManager.instance.maxHeight - _height;

        }
    }

    public enum DockSplitDirection
    {
        Vertical,
        Hortizonal
    }

    public class DockSplit : Drawable
    {
        public DockSplitDirection direction;
        public DockPane target, child;

        public DockSplit(DockPaneLocation location, DockPane parent, DockPane child)
        {
            child.width = Math.Max(WindowManager.instance.main.width / 2, child.width);
            child.height = Math.Max(WindowManager.instance.main.height / 2, child.height);

            margin = 0;
            padding = 0;
            x = parent.x;
            y = parent.y;
            width = parent.width;
            height = parent.height;

            WindowManager.instance.main.width -= child.width;
            WindowManager.instance.main.height -= child.height;

            switch (location)
            {
                case DockPaneLocation.Left: SplitVertical(child, parent); break;
                case DockPaneLocation.Right: SplitVertical(parent, child); break;
                case DockPaneLocation.Top: SplitHotizontal(child, parent); break;
                case DockPaneLocation.Bottom: SplitHotizontal(parent, child); break;
            }
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            target.Draw(g);
            //child.Draw(g);
        }

        public override void computeMaxSize(ref int _maxWidth, ref int _maxHeight)
        {
            if (direction != DockSplitDirection.Hortizonal)
                _maxWidth += target.width;
            else
                _maxHeight += target.height;

            base.computeMaxSize(ref _maxWidth, ref _maxHeight);

        }

        private void SplitVertical(DockPane left, DockPane right)
        {
            left.x = clientX;
            left.y = clientY;
            left.height = clientHeight;
            //left.width = clientWidth / 2;

            right.x = left.x + left.width;
            right.y = clientY;
            //right.width = clientWidth - left.width;
            right.height = clientHeight;

            target = left;
            child = right;
            direction = DockSplitDirection.Vertical;
        }

        private void SplitHotizontal(DockPane top, DockPane bottom)
        {
            top.x = clientX;
            top.y = clientY;
            //top.height = clientHeight / 2;
            top.width = clientWidth;

            bottom.x = clientX;
            bottom.y = top.y + top.height;
            bottom.width = clientWidth;
            //bottom.height = clientHeight - top.height;

            target = top;
            child = bottom;
            direction = DockSplitDirection.Hortizonal;
        }


    }

    public enum DockPaneLocation { Top, Left, Right, Bottom }

    public class WindowManager
    {
        public readonly static WindowManager instance = new WindowManager();

        public int maxWidth = 0;
        public int maxHeight = 0;

        public DocumentContainer main = new DocumentContainer() { name = "Main Window" };

        //public LinkedList<DockSplit> splits = new LinkedList<DockSplit>();
        //public DockSplit mainSplit;

        public Font font;
        public Brush marginColor;
        public Brush paddingColor;
        public Brush backgroundColor;

        public Pen borderColor;
        public Pen fontColor;

        private int lastWidth = 0;
        private int lastHeight = 0;

        public void init(Form form)
        {
            font = form.Font;

            marginColor = new SolidBrush(Color.Red);
            paddingColor = new SolidBrush(Color.Green);
            backgroundColor = new SolidBrush(Color.Black);

            //borderColor = new Pen(Color.Orange);
            fontColor = new Pen(Color.White);

            lastWidth = form.ClientRectangle.Width;
            lastHeight = form.ClientRectangle.Height;
            main.width = form.ClientRectangle.Width;
            main.height = form.ClientRectangle.Height;

            Resize(form);
        }

        public void Draw(Graphics g)
        {
            //mainSplit.Draw(g);
            main.Draw(g);
        }

        public void Resize(Form form)
        {
            maxWidth = form.ClientRectangle.Width;
            maxHeight = form.ClientRectangle.Height;

            ////var newWidth = main.width 

            //var diffX = form.ClientRectangle.Width - lastWidth;
            //var diffy = form.ClientRectangle.Height - lastHeight;

            //var newWidth = main.width + diffX;
            //var newHeight = main.height + diffy;
           
            //lastWidth = form.ClientRectangle.Width;
            //lastHeight = form.ClientRectangle.Height;

            //main.x = 0;
            //main.y = 0;
            //main.width = newWidth;
            //main.height = form.ClientRectangle.Height;

            int _maxWidth = 0;
            int _maxHeight = 0;
            main.computeMaxSize(ref _maxWidth, ref _maxHeight);

            //if (_maxHeight == 0) _maxHeight = form.ClientRectangle.Height;
            //else if (_maxHeight > form.ClientRectangle.Height) return;

            //if (_maxWidth == 0) _maxHeight = form.ClientRectangle.Width;
            //else if (_maxWidth > form.ClientRectangle.Width) return;

            //main.width = form.ClientRectangle.Width - _maxWidth;
            //main.height = form.ClientRectangle.Height - _maxHeight;
        }

        public DockSplitDirection GetDirection(DockPaneLocation location)
        {
            if (location == DockPaneLocation.Bottom || location == DockPaneLocation.Top)
                return DockSplitDirection.Vertical;

            return DockSplitDirection.Hortizonal;
        }

        public void AddDockPane(DockPane parent, DockPane child, DockPaneLocation location)
        {
            var split = new DockSplit(location, parent, child);

            parent.SetParent(split);
        }

        

    }

    public partial class Form1 : Form
    {

        public Form1()
        {
            WindowManager.instance.init(this);

            InitializeComponent();


            var solution = new DockPane() { name = "Solution", width = 250, height = 250, maxWidth = 250, maxHeight = 250 };
            WindowManager.instance.AddDockPane(WindowManager.instance.main, solution, DockPaneLocation.Left);

            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(Color.Black);
            WindowManager.instance.Draw(e.Graphics);
            //e.Graphics.DrawRectangle(Pens.Red, new Rectangle(10, 10, this.ClientRectangle.Width - 20, this.ClientRectangle.Height - 20));
            //e.Graphics.DrawString("hello world", this.Font, Brushes.White, 0, 0, StringFormat.GenericDefault);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            WindowManager.instance.Resize(this);
        }
    }
}
