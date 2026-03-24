using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging; // Necessário para formatos de imagem
using System.Windows.Forms;
using System.Collections.Generic;

namespace Win11PaintClone
{
    public class MainForm : Form
    {
        private Bitmap canvasBitmap;
        private Graphics graphics;
        private bool isDrawing = false;
        private Point startPoint, currentPoint;
        
        private DrawingTool currentTool = DrawingTool.Pencil;
        private BrushType currentBrush = BrushType.Standard;
        private Color primaryColor = Color.Black;
        private int penSize = 5;
        private Pen currentPen;

        private PictureBox canvas;
        private ToolStripStatusLabel statusLabel;

        public MainForm()
        {
            this.Text = "Paint Clone Pro - Windows 11 Style";
            this.Size = new Size(1200, 850);
            this.BackColor = Color.FromArgb(243, 243, 243);
            
            InitializeComponents();
            SetupCanvas(1000, 600);
            
            currentPen = new Pen(primaryColor, penSize) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        }

        private void InitializeComponents()
        {
            Panel ribbon = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.White, Padding = new Padding(10) };
            FlowLayoutPanel flow = new FlowLayoutPanel { Dock = DockStyle.Fill };

            // --- GRUPO: FILE (NOVO) ---
            flow.Controls.Add(CreateGroupLabel("File"));
            flow.Controls.Add(CreateIconButton("💾", (s, e) => SaveImage()));
            flow.Controls.Add(CreateIconButton("🗑️", (s, e) => SetupCanvas(canvas.Width, canvas.Height)));

            // Grupo: Tools
            flow.Controls.Add(CreateGroupLabel("Tools"));
            flow.Controls.Add(CreateIconButton("✏️", (s, e) => currentTool = DrawingTool.Pencil));
            flow.Controls.Add(CreateIconButton("🧽", (s, e) => currentTool = DrawingTool.Eraser));
            flow.Controls.Add(CreateIconButton("🪣", (s, e) => currentTool = DrawingTool.Fill));

            // Grupo: Brushes
            flow.Controls.Add(CreateGroupLabel("Brushes"));
            flow.Controls.Add(CreateIconButton("🖌️", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Standard; }));
            flow.Controls.Add(CreateIconButton("🖋️", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Calligraphy; }));
            flow.Controls.Add(CreateIconButton("💨", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Spray; }));

            // Grupo: Shapes
            flow.Controls.Add(CreateGroupLabel("Shapes"));
            flow.Controls.Add(CreateIconButton("╱", (s, e) => currentTool = DrawingTool.Line));
            flow.Controls.Add(CreateIconButton("▭", (s, e) => currentTool = DrawingTool.Rectangle));
            flow.Controls.Add(CreateIconButton("◯", (s, e) => currentTool = DrawingTool.Ellipse));
            flow.Controls.Add(CreateIconButton("△", (s, e) => currentTool = DrawingTool.Triangle));
            flow.Controls.Add(CreateIconButton("⭐", (s, e) => currentTool = DrawingTool.Star));
            flow.Controls.Add(CreateIconButton("➔", (s, e) => currentTool = DrawingTool.Arrow));

            // Tamanho e Cor
            NumericUpDown sizePicker = new NumericUpDown { Value = 5, Minimum = 1, Width = 50 };
            sizePicker.ValueChanged += (s, e) => penSize = (int)sizePicker.Value;
            flow.Controls.Add(new Label { Text = "Size:", Margin = new Padding(10, 15, 0, 0), AutoSize = true });
            flow.Controls.Add(sizePicker);

            Button btnColor = new Button { Text = "Color", BackColor = primaryColor, ForeColor = Color.White, Width = 60, Height = 40, Margin = new Padding(5, 5, 0, 0) };
            btnColor.Click += (s, e) => {
                using (ColorDialog cd = new ColorDialog())
                    if (cd.ShowDialog() == DialogResult.OK) { primaryColor = cd.Color; btnColor.BackColor = cd.Color; }
            };
            flow.Controls.Add(btnColor);

            ribbon.Controls.Add(flow);
            this.Controls.Add(ribbon);

            canvas = new PictureBox { BackColor = Color.White, Location = new Point(20, 140), Cursor = Cursors.Cross, BorderStyle = BorderStyle.FixedSingle };
            Panel wrapper = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(230, 230, 230), Padding = new Padding(20) };
            wrapper.Controls.Add(canvas);
            this.Controls.Add(wrapper);

            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.Paint += Canvas_Paint;

            StatusStrip ss = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            ss.Items.Add(statusLabel);
            this.Controls.Add(ss);
        }

        private Control CreateGroupLabel(string text) => new Label { Text = text, Font = new Font(this.Font, FontStyle.Bold), Margin = new Padding(10, 15, 5, 0), AutoSize = true };

        private Button CreateIconButton(string text, EventHandler onClick)
        {
            Button b = new Button { Text = text, Width = 45, Height = 45, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Symbol", 14), Margin = new Padding(2) };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void SetupCanvas(int w, int h)
        {
            canvasBitmap = new Bitmap(w, h);
            graphics = Graphics.FromImage(canvasBitmap);
            graphics.Clear(Color.White);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.Size = new Size(w, h);
            canvas.Image = canvasBitmap;
            canvas.Invalidate();
        }

        // --- LÓGICA DE SALVAMENTO ---
        private void SaveImage()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save your masterpiece";
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif";
                sfd.DefaultExt = "png";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat format = ImageFormat.Png;
                    string extension = System.IO.Path.GetExtension(sfd.FileName).ToLower();

                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg": format = ImageFormat.Jpeg; break;
                        case ".bmp": format = ImageFormat.Bmp; break;
                        case ".gif": format = ImageFormat.Gif; break;
                    }

                    canvasBitmap.Save(sfd.FileName, format);
                    MessageBox.Show("Image saved successfully!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // (Mantenha os métodos Canvas_MouseDown, Canvas_MouseMove, Canvas_MouseUp, Canvas_Paint e GetRect que enviamos anteriormente)
        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTool == DrawingTool.Fill)
            {
                Color target = canvasBitmap.GetPixel(e.X, e.Y);
                DrawingHelper.FloodFill(canvasBitmap, e.Location, target, primaryColor);
                canvas.Invalidate();
                return;
            }
            isDrawing = true;
            startPoint = e.Location;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;
            currentPoint = e.Location;
            statusLabel.Text = $"Coordinates: {e.X}, {e.Y}";

            if (currentTool == DrawingTool.Pencil || currentTool == DrawingTool.Eraser)
            {
                Pen p = new Pen(currentTool == DrawingTool.Eraser ? Color.White : primaryColor, penSize);
                if (currentBrush == BrushType.Spray && currentTool != DrawingTool.Eraser)
                {
                    Random rnd = new Random();
                    for(int i=0; i<20; i++)
                        graphics.FillRectangle(new SolidBrush(primaryColor), e.X + rnd.Next(-penSize, penSize), e.Y + rnd.Next(-penSize, penSize), 1, 1);
                }
                else
                {
                    p.StartCap = p.EndCap = LineCap.Round;
                    if (currentBrush == BrushType.Calligraphy) p.Width = penSize * 2;
                    graphics.DrawLine(p, startPoint, currentPoint);
                }
                startPoint = currentPoint;
                canvas.Invalidate();
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;
            isDrawing = false;
            currentPen.Color = primaryColor;
            currentPen.Width = penSize;
            var rect = GetRect(startPoint, e.Location);
            switch (currentTool)
            {
                case DrawingTool.Line: graphics.DrawLine(currentPen, startPoint, e.Location); break;
                case DrawingTool.Rectangle: graphics.DrawRectangle(currentPen, rect); break;
                case DrawingTool.Ellipse: graphics.DrawEllipse(currentPen, rect); break;
                case DrawingTool.Triangle: graphics.DrawPolygon(currentPen, new Point[] { new Point(rect.Left + rect.Width/2, rect.Top), new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom) }); break;
                case DrawingTool.Star: DrawingHelper.DrawStar(graphics, currentPen, rect); break;
                case DrawingTool.Arrow: DrawingHelper.DrawArrow(graphics, currentPen, startPoint, e.Location); break;
            }
            canvas.Invalidate();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isDrawing) return;
            var rect = GetRect(startPoint, currentPoint);
            Pen p = new Pen(primaryColor, penSize);
            if (currentTool == DrawingTool.Line) e.Graphics.DrawLine(p, startPoint, currentPoint);
            else if (currentTool == DrawingTool.Rectangle) e.Graphics.DrawRectangle(p, rect);
            else if (currentTool == DrawingTool.Ellipse) e.Graphics.DrawEllipse(p, rect);
        }

        private Rectangle GetRect(Point p1, Point p2) => new Rectangle(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
    }
}