using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

public class HiddenTab : TabControl
{

    public int DesignerIndex
    {
        get { return SelectedIndex; }
        set
        {
            if (DesignMode)
            {
                SelectedIndex = value;
            }
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 4904)
        {
            m.Result = IntPtr.Zero;
        }
        else
        {
            base.WndProc(ref m);
        }
    }

}
public class ListView : System.Windows.Forms.ListView
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);
    protected override void CreateHandle()
    {
        base.CreateHandle();
        SetWindowTheme(this.Handle, "explorer", null);
    }
    public ListView()
    {
        this.DoubleBuffered = true;
        this.View = System.Windows.Forms.View.Details;
        this.FullRowSelect = true;
    }
}
class SmoothLabel : Control
{

	private Bitmap TextBitmap;

	private Graphics TextGraphics;
	public SmoothLabel()
	{
		SetStyle((ControlStyles)139270, true);

		TextBitmap = new Bitmap(1, 1);
		TextGraphics = Graphics.FromImage(TextBitmap);

		BackColor = Color.FromArgb(250, 250, 200);
		ForeColor = Color.FromArgb(150, 150, 120);
		P1 = new Pen(Color.FromArgb(200, 200, 160));
	}

	private Pen P1 = Pens.Black;
	public Color BorderColor {
		get { return P1.Color; }
		set {
			P1 = new Pen(value);
			Invalidate();
		}
	}

	private SolidBrush B1;
	public override Color ForeColor {
		get { return base.ForeColor; }
		set {
			base.ForeColor = value;
			B1 = new SolidBrush(value);
			Invalidate();
		}
	}

	private Size TextSize;
	public override string Text {
		get { return base.Text; }
		set {
			base.Text = value;
			TextSize = TextGraphics.MeasureString(base.Text, base.Font).ToSize();
			Invalidate();
		}
	}

	public override Font Font {
		get { return base.Font; }
		set {
			base.Font = value;
			TextSize = TextGraphics.MeasureString(base.Text, base.Font).ToSize();
			Invalidate();
		}
	}

	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
	{
		e.Graphics.Clear(BackColor);
		e.Graphics.DrawString(Text, Font, B1, Width / 2 - TextSize.Width / 2, Height / 2 - TextSize.Height / 2);
		e.Graphics.DrawRectangle(P1, 0, 0, Width - 1, Height - 1);
	}

}
public class ChangerControl : Control
{
    #region " Control Help "
    public GraphicsPath RoundRect(Rectangle Rectangle, int Curve)
    {
        GraphicsPath P = new GraphicsPath();
        int ArcRectangleWidth = Curve * 2;
        P.AddArc(new Rectangle(Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -180, 90);
        P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), -90, 90);
        P.AddArc(new Rectangle(Rectangle.Width - ArcRectangleWidth + Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 0, 90);
        P.AddArc(new Rectangle(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y, ArcRectangleWidth, ArcRectangleWidth), 90, 90);
        P.AddLine(new Point(Rectangle.X, Rectangle.Height - ArcRectangleWidth + Rectangle.Y), new Point(Rectangle.X, Curve + Rectangle.Y));
        return P;
    }
    public GraphicsPath RoundRect(int X, int Y, int Width, int Height, int Curve)
    {
        return RoundRect(new Rectangle(X, Y, Width, Height), Curve);
    }
    public enum MouseState : byte
    {
        None = 0,
        Over = 1,
        Down = 2,
        Block = 3
    }
    #endregion

    #region " Variables "
    public TextBox tbChange;
    private bool editing = false;
    #endregion
    MouseState State = new MouseState();

    #region " Constructor "
    public ChangerControl()
        : base()
    {
        tbChange = new TextBox
        {
            Text = Text,
            Size = Size,
            ForeColor = ForeColor,
            Visible = false,
            Width = 150
        };
        SetStyle(ControlStyles.UserPaint, true);
        //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.FixedHeight, true);
        DoubleBuffered = true;
        ForeColor = Color.Black;
        Width = 200;
        Controls.Add(tbChange);
        tbChange.KeyUp += new KeyEventHandler(OnKeyUp);
    }
    #endregion

    #region " Methods "

    protected override void CreateHandle()
    {
        base.CreateHandle();
        tbChange.Text = Text;
    }
    protected override void OnResize(System.EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
        Height = 23;
    }
    protected override void OnTextChanged(System.EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
    protected override void OnForeColorChanged(System.EventArgs e)
    {
        base.OnForeColorChanged(e);
        Invalidate();
    }
    protected override void OnBackColorChanged(System.EventArgs e)
    {
        base.OnBackColorChanged(e);
        Invalidate();
    }
    protected override void OnFontChanged(System.EventArgs e)
    {
        base.OnFontChanged(e);
        Invalidate();
    }
    protected override void OnParentBackColorChanged(System.EventArgs e)
    {
        base.OnParentBackColorChanged(e);
        Invalidate();
    }
    protected override void OnMouseEnter(System.EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }
    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }
    protected override void OnMouseLeave(System.EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }
    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        base.OnPaint(e);
        Font fontButtons = new Font("Marlett", 8.5F, FontStyle.Regular);
        Bitmap B = new Bitmap(Width, Height);
        Graphics G = Graphics.FromImage(B);
        {
            G.SmoothingMode = SmoothingMode.HighQuality;
            G.Clear(BackColor);
            if (!editing)
            {
                if (State == MouseState.Over)
                {
                    G.FillPath(new SolidBrush(Color.FromArgb(220, 220, 220)), RoundRect(new Rectangle(0, 3, (int)G.MeasureString(Text, Font).Width + 1, Height - 5), 3));
                }
            }
            G.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(1, 6), StringFormat.GenericDefault);



            LinearGradientBrush buttonGrad = new LinearGradientBrush(new Rectangle(0, 2, Height - 7, Height - 1), Color.White, Color.FromArgb(200, 200, 200), 90);
            if (editing)
            {
                G.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
                G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                G.FillEllipse(buttonGrad, new Rectangle(tbChange.Size.Width + 4, 3, Height - 7, Height - 7));
                G.DrawEllipse(Pens.Black, new Rectangle(tbChange.Size.Width + 4, 3, Height - 7, Height - 7));
                G.DrawString("a", new Font(fontButtons.FontFamily, 14), Brushes.Black, new Point(tbChange.Size.Width + 1, 1));

                G.FillEllipse(buttonGrad, new Rectangle(tbChange.Size.Width + 5 + (Height - 7) + 3, 3, Height - 7, Height - 7));
                G.DrawEllipse(Pens.Black, new Rectangle(tbChange.Size.Width + 5 + (Height - 7) + 3, 3, Height - 7, Height - 7));
                G.DrawString("r", new Font(fontButtons.FontFamily, 9), Brushes.Black, new Point(tbChange.Size.Width + 5 + (Height - 7) + 4, 5));
            }
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            e.Graphics.DrawImage(B, new Point(0, 0));
            G.Dispose();
            B.Dispose();
        }

    }
    protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
    {
        base.OnMouseClick(e);
        Point mouseL = e.Location;

        {
            tbChange.Location = new Point(0, 3);
            tbChange.Size = new Size(Width - 50, Height);
            tbChange.Visible = true;
            editing = tbChange.Visible;
            tbChange.Focus();
            tbChange.SelectAll();
        }

        if ((editing))
        {
            if (mouseL.X >= (Width - 45) && mouseL.X <= (Width - 45) + 18)
            {
                editing = false;
                Text = tbChange.Text;
            }
            else if (mouseL.X >= (Width - 45) + 18)
            {
                editing = false;
            }
            tbChange.Visible = editing;
        }

        Invalidate();
    }
    protected void OnKeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (editing)
        {
            if (e.KeyCode == Keys.Enter)
            {
                editing = false;
                Text = tbChange.Text;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                editing = false;
            }

            tbChange.Visible = editing;
        }

        Invalidate();
    }

    #endregion
}

//    #region " Sort "
//    /// <summary>
//    /// This class is an implementation of the 'IComparer' interface.
//    /// </summary>
//    public class ListViewColumnSorter : IComparer
//    {
//        /// <summary>
//        /// Specifies the column to be sorted
//        /// </summary>
//        private int ColumnToSort;
//        /// <summary>
//        /// Specifies the order in which to sort (i.e. 'Ascending').
//        /// </summary>
//        private SortOrder OrderOfSort;
//        /// <summary>
//        /// Case insensitive comparer object
//        /// </summary>
//        //private CaseInsensitiveComparer ObjectCompare;
//        private NumberCaseInsensitiveComparer ObjectCompare;
//        private ImageTextComparer FirstObjectCompare;

//        /// <summary>
//        /// Class constructor.  Initializes various elements
//        /// </summary>
//        public ListViewColumnSorter()
//        {
//            // Initialize the column to '0'
//            ColumnToSort = 0;

//            // Initialize the sort order to 'none'
//            //OrderOfSort = SortOrder.None;
//            OrderOfSort = SortOrder.Ascending;

//            // Initialize the CaseInsensitiveComparer object
//            ObjectCompare = new NumberCaseInsensitiveComparer();//CaseInsensitiveComparer();
//            FirstObjectCompare = new ImageTextComparer();
//        }

//        /// <summary>
//        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
//        /// </summary>
//        /// <param name="x">First object to be compared</param>
//        /// <param name="y">Second object to be compared</param>
//        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
//        public int Compare(object x, object y)
//        {
//            int compareResult;
//            ListViewItem listviewX, listviewY;

//            // Cast the objects to be compared to ListViewItem objects
//            listviewX = (ListViewItem)x;
//            listviewY = (ListViewItem)y;

//            if (ColumnToSort == 0)
//            {
//                compareResult = FirstObjectCompare.Compare(x, y);
//            }
//            else
//            {
//                // Compare the two items
//                compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
//            }

//            // Calculate correct return value based on object comparison
//            if (OrderOfSort == SortOrder.Ascending)
//            {
//                // Ascending sort is selected, return normal result of compare operation
//                return compareResult;
//            }
//            else if (OrderOfSort == SortOrder.Descending)
//            {
//                // Descending sort is selected, return negative result of compare operation
//                return (-compareResult);
//            }
//            else
//            {
//                // Return '0' to indicate they are equal
//                return 0;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
//        /// </summary>
//        public int SortColumn
//        {
//            set
//            {
//                ColumnToSort = value;
//            }
//            get
//            {
//                return ColumnToSort;
//            }
//        }

//        /// <summary>
//        /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
//        /// </summary>
//        public SortOrder Order
//        {
//            set
//            {
//                OrderOfSort = value;
//            }
//            get
//            {
//                return OrderOfSort;
//            }
//        }

//    }

//    public class ImageTextComparer : IComparer
//    {
//        //private CaseInsensitiveComparer ObjectCompare;
//        private NumberCaseInsensitiveComparer ObjectCompare;

//        public ImageTextComparer()
//        {
//            // Initialize the CaseInsensitiveComparer object
//            ObjectCompare = new NumberCaseInsensitiveComparer();//CaseInsensitiveComparer();
//        }

//        public int Compare(object x, object y)
//        {
//            //int compareResult;
//            int image1, image2;
//            ListViewItem listviewX, listviewY;

//            // Cast the objects to be compared to ListViewItem objects
//            listviewX = (ListViewItem)x;
//            image1 = listviewX.ImageIndex;
//            listviewY = (ListViewItem)y;
//            image2 = listviewY.ImageIndex;

//            if (image1 < image2)
//            {
//                return -1;
//            }
//            else if (image1 == image2)
//            {
//                return ObjectCompare.Compare(listviewX.Text, listviewY.Text);
//            }
//            else
//            {
//                return 1;
//            }
//        }
//    }

//    public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer
//    {
//        public NumberCaseInsensitiveComparer()
//        {

//        }

//        public new int Compare(object x, object y)
//        {
//            if ((x is System.String) && IsWholeNumber((string)x) && (y is System.String) && IsWholeNumber((string)y))
//            {
//                return base.Compare(System.Convert.ToInt32(x), System.Convert.ToInt32(y));
//            }
//            else
//            {
//                return base.Compare(x, y);
//            }
//        }

//        private bool IsWholeNumber(string strNumber)
//        {
//            Regex objNotWholePattern = new Regex("[^0-9]");
//            return !objNotWholePattern.IsMatch(strNumber);
//        }
//    }
//    #endregion
//}
