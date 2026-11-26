using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GastroPOS
{
    public partial class Form1 : Form
    {
        Panel ustBar = new Panel();
        Panel yanMenu = new Panel();
        Panel cizimAlani = new Panel();
        string suAnkiBolum = "Salon 1";

        public static Dictionary<string, string> MasaDurumlari = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, int>> MasaSiparisleri = new Dictionary<string, Dictionary<string, int>>();

        public class Urun
        {
            public int ID { get; set; }
            public string Ad { get; set; } = "";
            public int Fiyat { get; set; }
            public string Kategori { get; set; } = "";
            public Image? Gorsel { get; set; } // Ürün görseli için yeni alan
        }
        public static List<Urun> GlobalUrunListesi = new List<Urun>();

        public Form1()
        {
            InitializeComponent();
            VarsayilanUrunleriYukle();
            this.Text = "GastroPOS - Salon Krokisi";
            this.Size = new Size(1450, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            BarTasarla();
            MenuTasarla();
            SemaTasarla();
        }

        private void VarsayilanUrunleriYukle()
        {
            if (GlobalUrunListesi.Count == 0)
            {
                GlobalUrunListesi.Add(new Urun { ID = 1, Ad = "İskender Kebap", Kategori = "Ana Yemekler", Fiyat = 250 });
                GlobalUrunListesi.Add(new Urun { ID = 2, Ad = "Ayran", Kategori = "İçecekler", Fiyat = 30 });
                GlobalUrunListesi.Add(new Urun { ID = 3, Ad = "Künefe", Kategori = "Tatlılar", Fiyat = 120 });
                GlobalUrunListesi.Add(new Urun { ID = 4, Ad = "Mercimek Çorbası", Kategori = "Çorbalar", Fiyat = 75 });
            }
        }

        private void BarTasarla()
        {
            ustBar.Dock = DockStyle.Top; ustBar.Height = 60; ustBar.BackColor = Color.FromArgb(45, 52, 71);
            this.Controls.Add(ustBar);
            Button btnMenu = new Button { Text = "≡", Size = new Size(60, 60), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, Font = new Font("Segoe UI", 22, FontStyle.Bold) };
            ustBar.Controls.Add(btnMenu);
            Label lblBaslik = new Label { Text = "GastroPOS - Salon Krokisi", ForeColor = Color.White, Font = new Font("Segoe UI", 15, FontStyle.Bold), Location = new Point(80, 15), AutoSize = true };
            ustBar.Controls.Add(lblBaslik);
        }

        private void MenuTasarla()
        {
            yanMenu.Dock = DockStyle.Left; yanMenu.Width = 220; yanMenu.BackColor = Color.FromArgb(52, 59, 72);
            this.Controls.Add(yanMenu);
            string[] butonlar = { "Ürün Yönetimi", "Bahçe", "Teras", "Salon 1" };
            foreach (string isim in butonlar)
            {
                Button b = new Button { Text = isim, Dock = DockStyle.Top, Height = 70, FlatStyle = FlatStyle.Flat, ForeColor = (isim == "Ürün Yönetimi") ? Color.Gold : Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Tag = isim };
                b.Click += (s, e) => {
                    suAnkiBolum = b.Tag.ToString() ?? "Salon 1";
                    if (suAnkiBolum == "Ürün Yönetimi") { new YoneticiForm().ShowDialog(); return; }
                    cizimAlani.Controls.Clear(); MasalariYerlestir();
                };
                yanMenu.Controls.Add(b);
            }
        }

        private void SemaTasarla()
        {
            cizimAlani.Dock = DockStyle.Fill; cizimAlani.BackColor = Color.FromArgb(189, 195, 199);
            cizimAlani.Paint += (s, e) => {
                Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias; Pen p = new Pen(Color.FromArgb(127, 140, 141), 5);
                g.DrawLine(p, 150, 250, 150, 820); g.DrawLine(p, 150, 820, 1250, 820); g.DrawLine(p, 1250, 820, 1250, 50); g.DrawLine(p, 400, 50, 1250, 50);
            };
            this.Controls.Add(cizimAlani); MasalariYerlestir();
        }

        private void MasalariYerlestir()
        {
            int startX = 280, startY = 130, gapX = 195, gapY = 175;
            if (suAnkiBolum == "Salon 1")
            {
                for (int satir = 0; satir < 4; satir++)
                {
                    for (int sutun = 0; sutun < 5; sutun++)
                    {
                        int no = (satir * 5) + sutun + 1;
                        if (satir == 3 && sutun == 2 || satir == 0 && sutun == 0) continue;
                        cizimAlani.Controls.Add(MasaOlustur(startX + (sutun * gapX), startY + (satir * gapY), "S1-M" + no));
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++) cizimAlani.Controls.Add(MasaOlustur(startX + 150 + (i * 200), startY + 200, suAnkiBolum + "-M" + i));
            }
        }

        private Masa MasaOlustur(int x, int y, string id)
        {
            Masa m = new Masa { Location = new Point(x, y), Size = new Size(130, 130), Tag = id };
            if (!MasaDurumlari.ContainsKey(id)) MasaDurumlari[id] = "BOŞ";
            m.BackColor = MasaDurumlari[id] == "DOLU" ? Color.FromArgb(231, 76, 60) : (MasaDurumlari[id] == "REZERVE" ? Color.FromArgb(241, 196, 15) : Color.FromArgb(46, 204, 113));
            m.Text = MasaDurumlari[id];
            m.Click += (s, e) => {
                if (MasaDurumlari[id] == "BOŞ")
                {
                    var res = MessageBox.Show("Sipariş için EVET, Rezervasyon için HAYIR.", id, MessageBoxButtons.YesNoCancel);
                    if (res == DialogResult.Yes) { if (new SiparisEkrani(id).ShowDialog() == DialogResult.Yes) MasaDurumlari[id] = "DOLU"; }
                    else if (res == DialogResult.No) MasaDurumlari[id] = "REZERVE";
                }
                else if (new SiparisEkrani(id).ShowDialog() == DialogResult.OK) MasaDurumlari[id] = "BOŞ";
                cizimAlani.Controls.Clear(); MasalariYerlestir();
            };
            return m;
        }
    }

    // --- ÜRÜN YÖNETİM PANELİ (Görsel Seçme Destekli) ---
    public class YoneticiForm : Form
    {
        DataGridView dg = new DataGridView();
        TextBox txtAd = new TextBox();
        ComboBox cbKat = new ComboBox();
        NumericUpDown numFiyat = new NumericUpDown();
        PictureBox pbGorsel = new PictureBox(); // Seçilen görseli önizleme alanı
        Image? secilenResim = null;

        public YoneticiForm()
        {
            this.Text = "GastroPOS - Ürün Yönetim Paneli"; this.Size = new Size(1100, 700); this.StartPosition = FormStartPosition.CenterScreen; this.BackColor = Color.FromArgb(45, 52, 71);

            dg.Location = new Point(20, 100); dg.Size = new Size(550, 450); dg.BackgroundColor = Color.White; dg.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dg);

            Panel pnlInput = new Panel { Location = new Point(590, 100), Size = new Size(450, 500), BackColor = Color.Transparent };
            Label l1 = new Label { Text = "Ürün Adı:", ForeColor = Color.White, Location = new Point(10, 20), AutoSize = true };
            txtAd.Location = new Point(120, 17); txtAd.Width = 280;
            Label l2 = new Label { Text = "Kategori:", ForeColor = Color.White, Location = new Point(10, 60), AutoSize = true };
            cbKat.Location = new Point(120, 57); cbKat.Width = 280; cbKat.Items.AddRange(new string[] { "Çorbalar", "Ana Yemekler", "Tatlılar", "İçecekler" });
            Label l3 = new Label { Text = "Fiyat (TL):", ForeColor = Color.White, Location = new Point(10, 100), AutoSize = true };
            numFiyat.Location = new Point(120, 97); numFiyat.Width = 280; numFiyat.Maximum = 10000;

            // Resim Yükleme Butonu ve PictureBox
            pbGorsel.Location = new Point(120, 150); pbGorsel.Size = new Size(150, 120); pbGorsel.BorderStyle = BorderStyle.FixedSingle; pbGorsel.SizeMode = PictureBoxSizeMode.StretchImage;
            Button btnResim = new Button { Text = "Resim Yükle", Location = new Point(10, 150), Size = new Size(100, 40), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnResim.Click += (s, e) => {
                OpenFileDialog ofd = new OpenFileDialog { Filter = "Resim Dosyaları|*.jpg;*.png;*.jpeg" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    secilenResim = Image.FromFile(ofd.FileName);
                    pbGorsel.Image = secilenResim;
                }
            };

            Button btnEkle = new Button { Text = "Ekle", Location = new Point(10, 400), Size = new Size(130, 50), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnEkle.Click += (s, e) => {
                Form1.GlobalUrunListesi.Add(new Form1.Urun { ID = Form1.GlobalUrunListesi.Count + 1, Ad = txtAd.Text, Kategori = cbKat.Text, Fiyat = (int)numFiyat.Value, Gorsel = secilenResim });
                GuncelleDG();
            };

            Button btnSil = new Button { Text = "Sil", Location = new Point(270, 400), Size = new Size(130, 50), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSil.Click += (s, e) => { if (dg.CurrentRow != null) { Form1.GlobalUrunListesi.RemoveAt(dg.CurrentRow.Index); GuncelleDG(); } };

            pnlInput.Controls.AddRange(new Control[] { l1, txtAd, l2, cbKat, l3, numFiyat, pbGorsel, btnResim, btnEkle, btnSil });
            this.Controls.Add(pnlInput); GuncelleDG();
        }
        private void GuncelleDG() { dg.DataSource = null; dg.DataSource = Form1.GlobalUrunListesi.Select(u => new { u.ID, u.Ad, u.Kategori, u.Fiyat }).ToList(); }
    }

    // --- SİPARİŞ EKRANI (Görseller Buton Üzerinde) ---
    public class SiparisEkrani : Form
    {
        ListBox lstAdisyon = new ListBox(); Label lblTotal = new Label(); FlowLayoutPanel pnlUrun = new FlowLayoutPanel(); string masaKey;
        public SiparisEkrani(string id)
        {
            this.masaKey = id; this.Text = "Sipariş: " + id; this.Size = new Size(1100, 750); this.StartPosition = FormStartPosition.CenterScreen; this.BackColor = Color.FromArgb(45, 52, 71);
            FlowLayoutPanel pnlK = new FlowLayoutPanel { Location = new Point(20, 20), Size = new Size(650, 80) };
            string[] kats = { "Çorbalar", "Ana Yemekler", "Tatlılar", "İçecekler" };
            for (int i = 0; i < kats.Length; i++)
            {
                Button b = new Button { Text = kats[i], Size = new Size(150, 60), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
                b.Click += (s, e) => Goster(b.Text); pnlK.Controls.Add(b);
            }
            pnlUrun.Location = new Point(20, 110); pnlUrun.Size = new Size(650, 580); this.Controls.Add(pnlK); this.Controls.Add(pnlUrun);
            Panel pSag = new Panel { Location = new Point(700, 20), Size = new Size(360, 660), BackColor = Color.White };
            lstAdisyon.Dock = DockStyle.Top; lstAdisyon.Height = 450; pSag.Controls.Add(lstAdisyon);
            lblTotal.Font = new Font("Segoe UI", 18, FontStyle.Bold); lblTotal.Location = new Point(20, 500); lblTotal.AutoSize = true; pSag.Controls.Add(lblTotal);
            Button btnK = new Button { Text = "Siparişi Kaydet", Location = new Point(10, 580), Size = new Size(170, 60), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnK.Click += (s, e) => { this.DialogResult = DialogResult.Yes; this.Close(); };
            Button btnO = new Button { Text = "Ödeme Al", Location = new Point(185, 580), Size = new Size(170, 60), BackColor = Color.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnO.Click += (s, e) => { Form1.MasaSiparisleri.Remove(masaKey); this.DialogResult = DialogResult.OK; this.Close(); };
            pSag.Controls.AddRange(new Control[] { btnK, btnO }); this.Controls.Add(pSag); Goster("Çorbalar"); Yukle();
        }
        private void Goster(string k)
        {
            pnlUrun.Controls.Clear();
            foreach (var u in Form1.GlobalUrunListesi.Where(x => x.Kategori == k))
            {
                Button b = new Button
                {
                    Text = u.Ad + "\n" + u.Fiyat + " TL",
                    Size = new Size(180, 130),
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    TextImageRelation = TextImageRelation.ImageAboveText, // Resmi metnin üstüne koy
                    ImageAlign = ContentAlignment.TopCenter,
                    TextAlign = ContentAlignment.BottomCenter
                };
                if (u.Gorsel != null) b.Image = new Bitmap(u.Gorsel, new Size(80, 60)); // Resmi butona sığdır
                b.Click += (s, e) => {
                    if (!Form1.MasaSiparisleri.ContainsKey(masaKey)) Form1.MasaSiparisleri[masaKey] = new Dictionary<string, int>();
                    if (Form1.MasaSiparisleri[masaKey].ContainsKey(u.Ad)) Form1.MasaSiparisleri[masaKey][u.Ad]++; else Form1.MasaSiparisleri[masaKey][u.Ad] = 1; Yukle();
                };
                pnlUrun.Controls.Add(b);
            }
        }
        private void Yukle()
        {
            lstAdisyon.Items.Clear(); int t = 0;
            if (Form1.MasaSiparisleri.ContainsKey(masaKey))
            {
                foreach (var s in Form1.MasaSiparisleri[masaKey])
                {
                    int f = Form1.GlobalUrunListesi.First(x => x.Ad == s.Key).Fiyat;
                    lstAdisyon.Items.Add($"{s.Key} ({s.Value}) ... {s.Value * f} TL"); t += s.Value * f;
                }
            }
            lblTotal.Text = "TOPLAM: " + t + " TL";
        }
    }

    public class Masa : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Sekil { get; set; } = "Kare";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Kapasite { get; set; } = 4;
        public Masa() { this.FlatStyle = FlatStyle.Flat; this.FlatAppearance.BorderSize = 0; }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; if (this.Parent != null) e.Graphics.Clear(this.Parent.BackColor);
            Pen p = new Pen(Color.FromArgb(90, 90, 90), 6);
            if (Kapasite >= 2) { e.Graphics.DrawLine(p, 10, 65, 30, 65); e.Graphics.DrawLine(p, 100, 65, 120, 65); }
            if (Kapasite == 4) { e.Graphics.DrawLine(p, 65, 10, 65, 30); e.Graphics.DrawLine(p, 65, 100, 65, 120); }
            Rectangle r = new Rectangle(30, 30, 70, 70); using (Brush b = new SolidBrush(this.BackColor))
            {
                if (Sekil == "Daire") e.Graphics.FillEllipse(b, r); else { GraphicsPath path = new GraphicsPath(); int sz = 15; path.AddArc(r.X, r.Y, sz, sz, 180, 90); path.AddArc(r.Right - sz, r.Y, sz, sz, 270, 90); path.AddArc(r.Right - sz, r.Bottom - sz, sz, sz, 0, 90); path.AddArc(r.X, r.Bottom - sz, sz, sz, 90, 90); e.Graphics.FillPath(b, path); }
            }
            TextRenderer.DrawText(e.Graphics, this.Text, new Font("Segoe UI", 9, FontStyle.Bold), r, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}