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
        public Color color = Color.Red;
        public int margin = 5;
        public int padding = 5;

        public int x, y;
        public int width, height;
        public int maxWidth, maxHeight;

        public int clientX { get { return x + margin + padding; } }
        public int clientY { get { return y + margin + padding; } }
        public int clientWidth { get { return  width - padding * 2 - margin * 2; } }
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

        public virtual void computeSize() { }
    }

    public class DockPane : Drawable
    {
        public string name;

        public override void Draw(Graphics g)
        {
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
    }

    public enum DockSplitDirection
    {
        Vertical,
        Hortizonal
    }

    public class DockSplit : Drawable
    {
        public DockSplitDirection direction;
        public DockPane one, two;

        public DockSplit(DockPaneLocation location, DockPane parent, DockPane child)
        {
            margin = 0;
            padding = 0;
            x = parent.x;
            y = parent.y;
            width = parent.width;
            height = parent.height;

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
            one.Draw(g);
            two.Draw(g);
        }

        private void SplitVertical(DockPane left, DockPane right)
        {
            left.x = clientX;
            left.y = clientY;
            left.height = clientHeight;
            left.width = clientWidth / 2;

            right.x =left.x + left.width;
            right.y = clientY;
            right.width = clientWidth - left.width;
            right.height = clientHeight;

            one = left;
            two = right;
            direction = DockSplitDirection.Vertical;
        }

        private void SplitHotizontal(DockPane top, DockPane bottom)
        {
            top.x = clientX;
            top.y = clientY;
            top.height = clientHeight / 2;
            top.width = clientWidth;

            bottom.x = clientX;
            bottom.y = top.y + top.height;
            bottom.width = clientWidth;
            bottom.height = clientHeight - top.height;

            one = top;
            two = bottom;
            direction = DockSplitDirection.Hortizonal;
        }

        
    }

    public enum DockPaneLocation { Top, Left, Right, Bottom }

    public class WindowManager
    {
        public readonly static WindowManager instance = new WindowManager();

        public DocumentContainer main = new DocumentContainer() { name = "Main Window" };

        //public LinkedList<DockSplit> splits = new LinkedList<DockSplit>();
        public DockSplit mainSplit;

        public Font font;
        public Brush marginColor;
        public Brush paddingColor;
        public Brush backgroundColor;

        public Pen borderColor;
        public Pen fontColor;

        public void init(Form form)
        {
            font = form.Font;

            marginColor = new SolidBrush(Color.Red);
            paddingColor = new SolidBrush(Color.Green);
            backgroundColor = new SolidBrush(Color.Black);

            //borderColor = new Pen(Color.Orange);
            fontColor = new Pen(Color.White);

            Resize(form);
        }

        public void Draw(Graphics g)
        {
            mainSplit.Draw(g);
            //main.Draw(g);
        }

        public void Resize(Form form)
        {
            main.x = 0;
            main.y = 0;
            main.width = form.ClientRectangle.Width;
            main.height = form.ClientRectangle.Height;
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
            
            if (mainSplit == null)
                mainSplit = split;
        }
    }

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();

            WindowManager.instance.init(this);

            var solution = new DockPane() { name = "Solution", width = 250, height = 250 };
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
