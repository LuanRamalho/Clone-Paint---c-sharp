using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private Label statusLabel;
        private Panel outerBackground;
        private Random random = new Random(); // Para o spray

        // Margem fixa de 130px do topo, conforme você definiu.
        private const int CANVAS_TOP_MARGIN = 130; 

        public MainForm()
        {
            this.Text = "Paint Clone Pro - Windows 11 Style";
            this.Size = new Size(1200, 850);
            this.BackColor = Color.FromArgb(243, 243, 243);

            // Inicialização prévia
            canvas = new PictureBox { 
                BackColor = Color.White, 
                Cursor = Cursors.Cross, 
                BorderStyle = BorderStyle.None 
            };
            
            statusLabel = new Label { 
                Dock = DockStyle.Bottom, 
                Height = 25, 
                Text = "Ready", 
                Padding = new Padding(5, 0, 0, 0),
                BackColor = Color.White 
            };

            SetupCanvas(1000, 600);
            InitializeComponents();
            
            currentPen = new Pen(primaryColor, penSize) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            this.Controls.Add(statusLabel);
        }

        private void InitializeComponents()
        {
            // --- RIBBON SUPERIOR ---
            Panel ribbon = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.White, Padding = new Padding(5) };
            FlowLayoutPanel mainFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };

            mainFlow.Controls.Add(CreateGroupHeader("File", new Control[] {
                CreateIconButton("💾", (s, e) => SaveImage()),
                CreateIconButton("🗑️", (s, e) => SetupCanvas(canvas.Width, canvas.Height))
            }));

            mainFlow.Controls.Add(CreateGroupHeader("Tools", new Control[] {
                CreateIconButton("✏️", (s, e) => currentTool = DrawingTool.Pencil),
                CreateIconButton("🧽", (s, e) => currentTool = DrawingTool.Eraser),
                CreateIconButton("🪣", (s, e) => currentTool = DrawingTool.Fill)
            }));

            // --- GRUPO BRUSHES (Pincéis, incluindo o Spray) ---
            mainFlow.Controls.Add(CreateGroupHeader("Brushes", new Control[] {
                CreateIconButton("🖌️", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Standard; }),
                CreateIconButton("🖋️", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Calligraphy; }),
                CreateIconButton("💨", (s, e) => { currentTool = DrawingTool.Pencil; currentBrush = BrushType.Spray; }) // Spray
            }));

            mainFlow.Controls.Add(CreateGroupHeader("Shapes", new Control[] {
                CreateIconButton("╱", (s, e) => currentTool = DrawingTool.Line),
                CreateIconButton("▭", (s, e) => currentTool = DrawingTool.Rectangle),
                CreateIconButton("⬜", (s, e) => currentTool = DrawingTool.Square),
                CreateIconButton("◯", (s, e) => currentTool = DrawingTool.Ellipse),
                CreateIconButton("△", (s, e) => currentTool = DrawingTool.Triangle),
                CreateIconButton("⬠", (s, e) => currentTool = DrawingTool.Pentagon),
                CreateIconButton("⬡", (s, e) => currentTool = DrawingTool.Hexagon),
                CreateIconButton("⭐", (s, e) => currentTool = DrawingTool.Star),
                CreateIconButton("➔", (s, e) => currentTool = DrawingTool.Arrow)
            }));

            // --- ORGANIZAÇÃO DO GRUPO STYLE (Alinhado) ---
            TableLayoutPanel styleGrid = new TableLayoutPanel { RowCount = 2, ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            NumericUpDown sizePicker = new NumericUpDown { Value = penSize, Minimum = 1, Width = 50 };
            sizePicker.ValueChanged += (s, e) => penSize = (int)sizePicker.Value;
            
            Button btnColor = new Button { BackColor = primaryColor, Width = 30, Height = 30, FlatStyle = FlatStyle.Flat, Margin = new Padding(5, 0, 0, 0) };
            btnColor.Click += (s, e) => {
                using (ColorDialog cd = new ColorDialog())
                    if (cd.ShowDialog() == DialogResult.OK) { primaryColor = cd.Color; btnColor.BackColor = cd.Color; }
            };

            styleGrid.Controls.Add(new Label { Text = "Size", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            styleGrid.Controls.Add(sizePicker, 1, 0);
            styleGrid.Controls.Add(new Label { Text = "Color", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            styleGrid.Controls.Add(btnColor, 1, 1);

            mainFlow.Controls.Add(CreateGroupHeader("Style", new Control[] { styleGrid }));
            ribbon.Controls.Add(mainFlow);
            this.Controls.Add(ribbon);

            // --- ÁREA DE FUNDO CINZA ---
            outerBackground = new Panel { 
                Dock = DockStyle.Fill, 
                AutoScroll = true, 
                BackColor = Color.FromArgb(232, 232, 232) 
            };

            outerBackground.Controls.Add(canvas);
            this.Controls.Add(outerBackground);

            // Eventos para centralização horizontal e margem fixa no topo
            this.Load += (s, e) => CenterCanvas();
            this.Resize += (s, e) => CenterCanvas();

            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseUp += Canvas_MouseUp;
            canvas.Paint += Canvas_Paint;
        }

        private void CenterCanvas()
        {
            if (outerBackground == null || canvas == null) return;

            // Centraliza apenas horizontalmente. O topo é fixo.
            int x = (outerBackground.Width - canvas.Width) / 2;
            canvas.Location = new Point(Math.Max(20, x), CANVAS_TOP_MARGIN);
        }

        private void SetupCanvas(int w, int h)
        {
            canvasBitmap = new Bitmap(w, h);
            graphics = Graphics.FromImage(canvasBitmap);
            graphics.Clear(Color.White);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            canvas.Size = new Size(w, h);
            canvas.Image = canvasBitmap;
            CenterCanvas();
        }

        private Control CreateGroupHeader(string title, Control[] controls)
        {
            FlowLayoutPanel group = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, Margin = new Padding(10, 0, 10, 0) };
            FlowLayoutPanel iconContainer = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            foreach (var c in controls) iconContainer.Controls.Add(c);
            Label lbl = new Label { Text = title, TextAlign = ContentAlignment.MiddleCenter, Width = iconContainer.Width, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };
            group.Controls.Add(iconContainer);
            group.Controls.Add(lbl);
            return group;
        }

        private Button CreateIconButton(string text, EventHandler onClick)
        {
            Button b = new Button { Text = text, Width = 45, Height = 45, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Symbol", 14), Margin = new Padding(2) };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        private void SaveImage()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                sfd.Title = "Salvar Desenho";
                sfd.FileName = "MeuDesenho";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ImageFormat format = ImageFormat.Png;
                        
                        // Define o formato baseado na extensão escolhida
                        switch (System.IO.Path.GetExtension(sfd.FileName).ToLower())
                        {
                            case ".jpg":
                                format = ImageFormat.Jpeg;
                                break;
                            case ".bmp":
                                format = ImageFormat.Bmp;
                                break;
                        }

                        // Salva o bitmap que contém o desenho
                        canvasBitmap.Save(sfd.FileName, format);
                        MessageBox.Show("Imagem salva com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erro ao salvar imagem: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e) {
            if (currentTool == DrawingTool.Fill) { DrawingHelper.FloodFill(canvasBitmap, e.Location, canvasBitmap.GetPixel(e.X, e.Y), primaryColor); canvas.Invalidate(); return; }
            isDrawing = true; startPoint = e.Location;
        }

        // --- CORREÇÃO DO SPRAY (AQUI ESTÁ A LÓGICA DO SPRAY) ---
        private void Canvas_MouseMove(object sender, MouseEventArgs e) {
            statusLabel.Text = $"Coordinates: {e.X}, {e.Y}";
            if (!isDrawing) return;
            currentPoint = e.Location;

            if (currentTool == DrawingTool.Pencil || currentTool == DrawingTool.Eraser) {
                if (currentTool == DrawingTool.Pencil && currentBrush == BrushType.Spray) {
                    // Lógica do Spray
                    using (SolidBrush brush = new SolidBrush(primaryColor)) {
                        int sprayRadius = penSize * 2; // Ajuste o raio conforme necessário
                        int dotsPerMove = penSize * 2;  // Quantidade de pontos por movimento do mouse

                        for (int i = 0; i < dotsPerMove; i++) {
                            // Gera uma posição aleatória dentro do círculo de spray
                            double angle = random.NextDouble() * Math.PI * 2;
                            double radius = random.NextDouble() * sprayRadius;
                            int dotX = currentPoint.X + (int)(radius * Math.Cos(angle));
                            int dotY = currentPoint.Y + (int)(radius * Math.Sin(angle));

                            // Desenha um ponto (pequeno círculo)
                            graphics.FillEllipse(brush, dotX, dotY, 2, 2); // Pontos de tamanho fixo
                        }
                    }
                } else {
                    // Lógica para Pincel Padrão, Caneta Caligráfica e Borracha
                    using (Pen p = new Pen(currentTool == DrawingTool.Eraser ? Color.White : primaryColor, penSize)) {
                        if (currentTool == DrawingTool.Pencil && currentBrush == BrushType.Calligraphy) {
                            p.Width = penSize / 2; // Caneta mais fina
                            p.StartCap = p.EndCap = LineCap.Square; // Capa quadrada para efeito caligráfico
                        } else {
                            p.StartCap = p.EndCap = LineCap.Round; // Capa arredondada para o padrão
                        }
                        graphics.DrawLine(p, startPoint, currentPoint);
                    }
                }
                startPoint = currentPoint; canvas.Invalidate();
            }
        }

        private void Canvas_MouseUp(object sender, MouseEventArgs e) {
            if (!isDrawing) return; isDrawing = false;
            currentPen.Color = primaryColor; currentPen.Width = penSize;
            var rect = GetRect(startPoint, e.Location);
            switch (currentTool) {
                case DrawingTool.Line: graphics.DrawLine(currentPen, startPoint, e.Location); break;
                case DrawingTool.Rectangle: graphics.DrawRectangle(currentPen, rect); break;
                case DrawingTool.Square: int s = Math.Max(rect.Width, rect.Height); graphics.DrawRectangle(currentPen, rect.X, rect.Y, s, s); break;
                case DrawingTool.Ellipse: graphics.DrawEllipse(currentPen, rect); break;
                case DrawingTool.Triangle: graphics.DrawPolygon(currentPen, new Point[] { new Point(rect.Left + rect.Width/2, rect.Top), new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom) }); break;
                case DrawingTool.Pentagon: DrawingHelper.DrawPolygon(graphics, currentPen, rect, 5); break;
                case DrawingTool.Hexagon: DrawingHelper.DrawPolygon(graphics, currentPen, rect, 6); break;
                case DrawingTool.Star: DrawingHelper.DrawStar(graphics, currentPen, rect); break;
                case DrawingTool.Arrow: DrawingHelper.DrawArrow(graphics, currentPen, startPoint, e.Location); break;
            }
            canvas.Invalidate();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e) {
            if (!isDrawing) return;
            var rect = GetRect(startPoint, currentPoint);
            using (Pen p = new Pen(primaryColor, penSize)) {
                if (currentTool == DrawingTool.Line) e.Graphics.DrawLine(p, startPoint, currentPoint);
                else if (currentTool == DrawingTool.Rectangle) e.Graphics.DrawRectangle(p, rect);
                else if (currentTool == DrawingTool.Ellipse) e.Graphics.DrawEllipse(p, rect);
            }
        }

        private Rectangle GetRect(Point p1, Point p2) => new Rectangle(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
    }
}
