namespace StarX_Client;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private LuxuryBackground main = null!;
    private Panel pageSettings = null!;
    private Label lblLicStatus = null!;
    private TextBox txtKey = null!;
    private Button btnActivate = null!;
    private Label lblLang = null!;
    private ComboBox cmbLang = null!;
    private Button btnLangSave = null!;
    private Label lblUpdH = null!;
    private Label lblCurVer = null!;
    private Button btnCheck = null!;
    private Label lblUpdStatus = null!;
    private Button btnUpdateNow = null!;
    private Panel pagePro = null!;
    private Label lblProNote = null!;
    private ExitIconButton exitIcon = null!;
    private Panel pageOwner = null!;
    private Label lblOnlineH = null!;
    private ListView lvOnline = null!;
    private Label lblLicH = null!;
    private ListView lvLicenses = null!;
    private ComboBox cmbDays = null!;
    private TextBox txtNote = null!;
    private Button btnCreate = null!;
    private TextBox txtNewKey = null!;
    private Button btnRefresh = null!;
    private Button btnDelete = null!;
    private Label lblOwnStatus = null!;
    private Label lblPubH = null!;
    private TextBox txtPubVersion = null!;
    private TextBox txtToken = null!;
    private TextBox txtPubNotes = null!;
    private Button btnPublish = null!;
    private Label lblPubStatus = null!;
    private FloatingDock floatingDock = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        main = new LuxuryBackground();
        pageSettings = new Panel();
        lblLicStatus = new Label();
        txtKey = new TextBox();
        btnActivate = new Button();
        lblLang = new Label();
        cmbLang = new ComboBox();
        btnLangSave = new Button();
        lblUpdH = new Label();
        lblCurVer = new Label();
        btnCheck = new Button();
        lblUpdStatus = new Label();
        btnUpdateNow = new Button();
        pagePro = new Panel();
        lblProNote = new Label();
        exitIcon = new ExitIconButton();
        pageOwner = new Panel();
        lblOnlineH = new Label();
        lvOnline = new ListView();
        lblLicH = new Label();
        lvLicenses = new ListView();
        cmbDays = new ComboBox();
        txtNote = new TextBox();
        btnCreate = new Button();
        txtNewKey = new TextBox();
        btnRefresh = new Button();
        btnDelete = new Button();
        lblOwnStatus = new Label();
        lblPubH = new Label();
        txtPubVersion = new TextBox();
        txtToken = new TextBox();
        txtPubNotes = new TextBox();
        btnPublish = new Button();
        lblPubStatus = new Label();
        floatingDock = new FloatingDock();
        main.SuspendLayout();
        pageSettings.SuspendLayout();
        pagePro.SuspendLayout();
        pageOwner.SuspendLayout();
        SuspendLayout();
        //
        // main (مسرح فخم متحرك يملأ الواجهة)
        //
        main.Controls.Add(pageSettings);
        main.Controls.Add(pagePro);
        main.Controls.Add(pageOwner);
        main.Location = new Point(0, 0);
        main.Name = "main";
        main.Size = new Size(1000, 650);
        main.TabIndex = 2;
        //
        // pageSettings (صفحة الإعدادات — الترخيص)
        //
        pageSettings.BackColor = Color.Transparent;
        pageSettings.Controls.Add(lblLicStatus);
        pageSettings.Controls.Add(txtKey);
        pageSettings.Controls.Add(btnActivate);
        pageSettings.Controls.Add(lblLang);
        pageSettings.Controls.Add(cmbLang);
        pageSettings.Controls.Add(btnLangSave);
        pageSettings.Controls.Add(lblUpdH);
        pageSettings.Controls.Add(lblCurVer);
        pageSettings.Controls.Add(btnCheck);
        pageSettings.Controls.Add(lblUpdStatus);
        pageSettings.Controls.Add(btnUpdateNow);
        pageSettings.Dock = DockStyle.Fill;
        pageSettings.Location = new Point(0, 0);
        pageSettings.Name = "pageSettings";
        pageSettings.Size = new Size(1000, 584);
        pageSettings.TabIndex = 1;
        pageSettings.Visible = false;
        //
        // lblLicStatus
        //
        lblLicStatus.AutoSize = true;
        lblLicStatus.BackColor = Color.Transparent;
        lblLicStatus.Font = new Font("Segoe UI", 10F);
        lblLicStatus.ForeColor = Color.FromArgb(170, 170, 170);
        lblLicStatus.Location = new Point(380, 130);
        lblLicStatus.Name = "lblLicStatus";
        lblLicStatus.Size = new Size(140, 19);
        lblLicStatus.TabIndex = 2;
        lblLicStatus.Text = "…";
        //
        // txtKey
        //
        txtKey.BackColor = Color.FromArgb(22, 22, 22);
        txtKey.BorderStyle = BorderStyle.FixedSingle;
        txtKey.Font = new Font("Segoe UI", 11F);
        txtKey.ForeColor = Color.White;
        txtKey.Location = new Point(360, 165);
        txtKey.Name = "txtKey";
        txtKey.PlaceholderText = "أدخل مفتاح التفعيل";
        txtKey.Size = new Size(280, 30);
        txtKey.TabIndex = 3;
        //
        // btnActivate
        //
        btnActivate.BackColor = Color.White;
        btnActivate.Cursor = Cursors.Hand;
        btnActivate.FlatAppearance.BorderSize = 0;
        btnActivate.FlatStyle = FlatStyle.Flat;
        btnActivate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnActivate.ForeColor = Color.Black;
        btnActivate.Location = new Point(400, 212);
        btnActivate.Name = "btnActivate";
        btnActivate.Size = new Size(200, 42);
        btnActivate.TabIndex = 4;
        btnActivate.Text = "تفعيل";
        btnActivate.UseVisualStyleBackColor = false;
        //
        // lblLang
        //
        lblLang.AutoSize = true;
        lblLang.BackColor = Color.Transparent;
        lblLang.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLang.ForeColor = Color.FromArgb(170, 170, 170);
        lblLang.Location = new Point(465, 264);
        lblLang.Name = "lblLang";
        lblLang.Size = new Size(70, 19);
        lblLang.TabIndex = 5;
        lblLang.Text = "اللغة";
        //
        // cmbLang
        //
        cmbLang.BackColor = Color.FromArgb(22, 22, 22);
        cmbLang.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLang.FlatStyle = FlatStyle.Flat;
        cmbLang.Font = new Font("Segoe UI", 10F);
        cmbLang.ForeColor = Color.White;
        cmbLang.FormattingEnabled = true;
        cmbLang.Location = new Point(420, 290);
        cmbLang.Name = "cmbLang";
        cmbLang.Size = new Size(160, 25);
        cmbLang.TabIndex = 6;
        //
        // btnLangSave
        //
        btnLangSave.BackColor = Color.White;
        btnLangSave.Cursor = Cursors.Hand;
        btnLangSave.FlatAppearance.BorderSize = 0;
        btnLangSave.FlatStyle = FlatStyle.Flat;
        btnLangSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnLangSave.ForeColor = Color.Black;
        btnLangSave.Location = new Point(420, 326);
        btnLangSave.Name = "btnLangSave";
        btnLangSave.Size = new Size(160, 36);
        btnLangSave.TabIndex = 7;
        btnLangSave.Text = "حفظ";
        btnLangSave.UseVisualStyleBackColor = false;
        //
        // lblUpdH
        //
        lblUpdH.AutoSize = true;
        lblUpdH.BackColor = Color.Transparent;
        lblUpdH.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblUpdH.ForeColor = Color.FromArgb(190, 190, 190);
        lblUpdH.Location = new Point(448, 378);
        lblUpdH.Name = "lblUpdH";
        lblUpdH.Size = new Size(110, 20);
        lblUpdH.TabIndex = 8;
        lblUpdH.Text = "التحديثات";
        //
        // lblCurVer
        //
        lblCurVer.AutoSize = true;
        lblCurVer.BackColor = Color.Transparent;
        lblCurVer.Font = new Font("Segoe UI", 10F);
        lblCurVer.ForeColor = Color.FromArgb(170, 170, 170);
        lblCurVer.Location = new Point(400, 404);
        lblCurVer.Name = "lblCurVer";
        lblCurVer.Size = new Size(200, 19);
        lblCurVer.TabIndex = 9;
        lblCurVer.Text = "الإصدار الحالي: v1.0.0";
        //
        // btnCheck
        //
        btnCheck.BackColor = Color.White;
        btnCheck.Cursor = Cursors.Hand;
        btnCheck.FlatAppearance.BorderSize = 0;
        btnCheck.FlatStyle = FlatStyle.Flat;
        btnCheck.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCheck.ForeColor = Color.Black;
        btnCheck.Location = new Point(400, 428);
        btnCheck.Name = "btnCheck";
        btnCheck.Size = new Size(200, 38);
        btnCheck.TabIndex = 10;
        btnCheck.Text = "التحقق من التحديثات";
        btnCheck.UseVisualStyleBackColor = false;
        //
        // lblUpdStatus
        //
        lblUpdStatus.AutoSize = true;
        lblUpdStatus.BackColor = Color.Transparent;
        lblUpdStatus.Font = new Font("Segoe UI", 9F);
        lblUpdStatus.ForeColor = Color.FromArgb(150, 150, 150);
        lblUpdStatus.Location = new Point(380, 472);
        lblUpdStatus.MaximumSize = new Size(560, 20);
        lblUpdStatus.Name = "lblUpdStatus";
        lblUpdStatus.Size = new Size(60, 15);
        lblUpdStatus.TabIndex = 11;
        lblUpdStatus.Text = "…";
        //
        // btnUpdateNow
        //
        btnUpdateNow.BackColor = Color.White;
        btnUpdateNow.Cursor = Cursors.Hand;
        btnUpdateNow.FlatAppearance.BorderSize = 0;
        btnUpdateNow.FlatStyle = FlatStyle.Flat;
        btnUpdateNow.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnUpdateNow.ForeColor = Color.Black;
        btnUpdateNow.Location = new Point(400, 498);
        btnUpdateNow.Name = "btnUpdateNow";
        btnUpdateNow.Size = new Size(200, 36);
        btnUpdateNow.TabIndex = 12;
        btnUpdateNow.Text = "تحديث";
        btnUpdateNow.UseVisualStyleBackColor = false;
        btnUpdateNow.Visible = false;
        //
        // pagePro (صفحة البرو)
        //
        pagePro.BackColor = Color.Transparent;
        pagePro.Controls.Add(lblProNote);
        pagePro.Controls.Add(exitIcon);
        pagePro.Dock = DockStyle.Fill;
        pagePro.Location = new Point(0, 0);
        pagePro.Name = "pagePro";
        pagePro.Size = new Size(1000, 584);
        pagePro.TabIndex = 2;
        pagePro.Visible = false;
        //
        // lblProNote
        //
        lblProNote.AutoSize = true;
        lblProNote.BackColor = Color.Transparent;
        lblProNote.Font = new Font("Segoe UI", 10F);
        lblProNote.ForeColor = Color.FromArgb(170, 170, 170);
        lblProNote.Location = new Point(408, 170);
        lblProNote.Name = "lblProNote";
        lblProNote.Size = new Size(180, 19);
        lblProNote.TabIndex = 2;
        lblProNote.Text = "مميزات البرو — قريباً";
        //
        // exitIcon (أيقونة خروج حمراء فوق شمال صفحة البرو)
        //
        exitIcon.Location = new Point(14, 12);
        exitIcon.Name = "exitIcon";
        exitIcon.Size = new Size(34, 34);
        exitIcon.TabIndex = 3;
        //
        // pageOwner (لوحة المالك)
        //
        pageOwner.BackColor = Color.Transparent;
        pageOwner.Controls.Add(lblOnlineH);
        pageOwner.Controls.Add(lvOnline);
        pageOwner.Controls.Add(lblLicH);
        pageOwner.Controls.Add(lvLicenses);
        pageOwner.Controls.Add(btnCreate);
        pageOwner.Controls.Add(txtNote);
        pageOwner.Controls.Add(cmbDays);
        pageOwner.Controls.Add(txtNewKey);
        pageOwner.Controls.Add(btnRefresh);
        pageOwner.Controls.Add(btnDelete);
        pageOwner.Controls.Add(lblOwnStatus);
        pageOwner.Controls.Add(lblPubH);
        pageOwner.Controls.Add(txtPubVersion);
        pageOwner.Controls.Add(txtToken);
        pageOwner.Controls.Add(txtPubNotes);
        pageOwner.Controls.Add(btnPublish);
        pageOwner.Controls.Add(lblPubStatus);
        pageOwner.Dock = DockStyle.Fill;
        pageOwner.Location = new Point(0, 0);
        pageOwner.Name = "pageOwner";
        pageOwner.Size = new Size(1000, 584);
        pageOwner.TabIndex = 3;
        pageOwner.Visible = false;
        //
        // lblOnlineH
        //
        lblOnlineH.AutoSize = true;
        lblOnlineH.BackColor = Color.Transparent;
        lblOnlineH.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblOnlineH.ForeColor = Color.FromArgb(190, 190, 190);
        lblOnlineH.Location = new Point(430, 76);
        lblOnlineH.Name = "lblOnlineH";
        lblOnlineH.Size = new Size(140, 19);
        lblOnlineH.TabIndex = 2;
        lblOnlineH.Text = "المتصلون الآن";
        //
        // lvOnline
        //
        lvOnline.BackColor = Color.FromArgb(18, 18, 18);
        lvOnline.BorderStyle = BorderStyle.FixedSingle;
        lvOnline.Font = new Font("Segoe UI", 9F);
        lvOnline.ForeColor = Color.White;
        lvOnline.FullRowSelect = true;
        lvOnline.GridLines = true;
        lvOnline.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvOnline.Location = new Point(220, 100);
        lvOnline.MultiSelect = false;
        lvOnline.Name = "lvOnline";
        lvOnline.Size = new Size(560, 84);
        lvOnline.TabIndex = 3;
        lvOnline.UseCompatibleStateImageBehavior = false;
        lvOnline.View = View.Details;
        //
        // lblLicH
        //
        lblLicH.AutoSize = true;
        lblLicH.BackColor = Color.Transparent;
        lblLicH.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLicH.ForeColor = Color.FromArgb(190, 190, 190);
        lblLicH.Location = new Point(447, 192);
        lblLicH.Name = "lblLicH";
        lblLicH.Size = new Size(110, 19);
        lblLicH.TabIndex = 4;
        lblLicH.Text = "المفاتيح";
        //
        // lvLicenses
        //
        lvLicenses.BackColor = Color.FromArgb(18, 18, 18);
        lvLicenses.BorderStyle = BorderStyle.FixedSingle;
        lvLicenses.Font = new Font("Segoe UI", 9F);
        lvLicenses.ForeColor = Color.White;
        lvLicenses.FullRowSelect = true;
        lvLicenses.GridLines = true;
        lvLicenses.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvLicenses.Location = new Point(220, 216);
        lvLicenses.MultiSelect = false;
        lvLicenses.Name = "lvLicenses";
        lvLicenses.Size = new Size(560, 84);
        lvLicenses.TabIndex = 5;
        lvLicenses.UseCompatibleStateImageBehavior = false;
        lvLicenses.View = View.Details;
        //
        // btnCreate
        //
        btnCreate.BackColor = Color.White;
        btnCreate.Cursor = Cursors.Hand;
        btnCreate.FlatAppearance.BorderSize = 0;
        btnCreate.FlatStyle = FlatStyle.Flat;
        btnCreate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCreate.ForeColor = Color.Black;
        btnCreate.Location = new Point(275, 308);
        btnCreate.Name = "btnCreate";
        btnCreate.Size = new Size(140, 34);
        btnCreate.TabIndex = 6;
        btnCreate.Text = "إنشاء مفتاح";
        btnCreate.UseVisualStyleBackColor = false;
        //
        // txtNote
        //
        txtNote.BackColor = Color.FromArgb(22, 22, 22);
        txtNote.BorderStyle = BorderStyle.FixedSingle;
        txtNote.Font = new Font("Segoe UI", 10F);
        txtNote.ForeColor = Color.White;
        txtNote.Location = new Point(425, 310);
        txtNote.Name = "txtNote";
        txtNote.PlaceholderText = "ملاحظة (اختياري)";
        txtNote.Size = new Size(180, 27);
        txtNote.TabIndex = 7;
        //
        // cmbDays
        //
        cmbDays.BackColor = Color.FromArgb(22, 22, 22);
        cmbDays.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDays.FlatStyle = FlatStyle.Flat;
        cmbDays.Font = new Font("Segoe UI", 10F);
        cmbDays.ForeColor = Color.White;
        cmbDays.FormattingEnabled = true;
        cmbDays.Location = new Point(615, 310);
        cmbDays.Name = "cmbDays";
        cmbDays.Size = new Size(110, 25);
        cmbDays.TabIndex = 8;
        //
        // txtNewKey
        //
        txtNewKey.BackColor = Color.FromArgb(22, 22, 22);
        txtNewKey.BorderStyle = BorderStyle.FixedSingle;
        txtNewKey.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        txtNewKey.ForeColor = Color.White;
        txtNewKey.Location = new Point(245, 344);
        txtNewKey.Name = "txtNewKey";
        txtNewKey.ReadOnly = true;
        txtNewKey.Size = new Size(280, 27);
        txtNewKey.TabIndex = 9;
        txtNewKey.TextAlign = HorizontalAlignment.Center;
        //
        // btnRefresh
        //
        btnRefresh.BackColor = Color.Transparent;
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.FlatAppearance.BorderColor = Color.White;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 40);
        btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(25, 25, 25);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.Font = new Font("Segoe UI", 9F);
        btnRefresh.ForeColor = Color.White;
        btnRefresh.Location = new Point(535, 344);
        btnRefresh.Name = "btnRefresh";
        btnRefresh.Size = new Size(110, 29);
        btnRefresh.TabIndex = 10;
        btnRefresh.Text = "تحديث";
        btnRefresh.UseVisualStyleBackColor = false;
        //
        // btnDelete
        //
        btnDelete.BackColor = Color.Transparent;
        btnDelete.Cursor = Cursors.Hand;
        btnDelete.FlatAppearance.BorderColor = Color.White;
        btnDelete.FlatAppearance.BorderSize = 1;
        btnDelete.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 40);
        btnDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(25, 25, 25);
        btnDelete.FlatStyle = FlatStyle.Flat;
        btnDelete.Font = new Font("Segoe UI", 9F);
        btnDelete.ForeColor = Color.White;
        btnDelete.Location = new Point(655, 344);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(110, 29);
        btnDelete.TabIndex = 11;
        btnDelete.Text = "مسح المفتاح";
        btnDelete.UseVisualStyleBackColor = false;
        //
        // lblOwnStatus
        //
        lblOwnStatus.AutoSize = true;
        lblOwnStatus.BackColor = Color.Transparent;
        lblOwnStatus.Font = new Font("Segoe UI", 9F);
        lblOwnStatus.ForeColor = Color.FromArgb(150, 150, 150);
        lblOwnStatus.Location = new Point(380, 380);
        lblOwnStatus.Name = "lblOwnStatus";
        lblOwnStatus.Size = new Size(60, 15);
        lblOwnStatus.TabIndex = 12;
        lblOwnStatus.Text = "…";
        //
        // lblPubH
        //
        lblPubH.AutoSize = true;
        lblPubH.BackColor = Color.Transparent;
        lblPubH.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblPubH.ForeColor = Color.FromArgb(190, 190, 190);
        lblPubH.Location = new Point(435, 404);
        lblPubH.Name = "lblPubH";
        lblPubH.Size = new Size(130, 20);
        lblPubH.TabIndex = 13;
        lblPubH.Text = "نشر تحديث جديد";
        //
        // txtPubVersion
        //
        txtPubVersion.BackColor = Color.FromArgb(22, 22, 22);
        txtPubVersion.BorderStyle = BorderStyle.FixedSingle;
        txtPubVersion.Font = new Font("Segoe UI", 10F);
        txtPubVersion.ForeColor = Color.White;
        txtPubVersion.Location = new Point(255, 428);
        txtPubVersion.Name = "txtPubVersion";
        txtPubVersion.PlaceholderText = "1.4.0";
        txtPubVersion.Size = new Size(110, 27);
        txtPubVersion.TabIndex = 14;
        //
        // txtToken
        //
        txtToken.BackColor = Color.FromArgb(22, 22, 22);
        txtToken.BorderStyle = BorderStyle.FixedSingle;
        txtToken.Font = new Font("Segoe UI", 10F);
        txtToken.ForeColor = Color.White;
        txtToken.Location = new Point(375, 428);
        txtToken.Name = "txtToken";
        txtToken.PlaceholderText = "GitHub token";
        txtToken.Size = new Size(190, 27);
        txtToken.TabIndex = 15;
        txtToken.UseSystemPasswordChar = true;
        //
        // txtPubNotes
        //
        txtPubNotes.BackColor = Color.FromArgb(22, 22, 22);
        txtPubNotes.BorderStyle = BorderStyle.FixedSingle;
        txtPubNotes.Font = new Font("Segoe UI", 10F);
        txtPubNotes.ForeColor = Color.White;
        txtPubNotes.Location = new Point(575, 428);
        txtPubNotes.Name = "txtPubNotes";
        txtPubNotes.PlaceholderText = "ملاحظات";
        txtPubNotes.Size = new Size(170, 27);
        txtPubNotes.TabIndex = 16;
        //
        // btnPublish
        //
        btnPublish.BackColor = Color.White;
        btnPublish.Cursor = Cursors.Hand;
        btnPublish.FlatAppearance.BorderSize = 0;
        btnPublish.FlatStyle = FlatStyle.Flat;
        btnPublish.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnPublish.ForeColor = Color.Black;
        btnPublish.Location = new Point(400, 464);
        btnPublish.Name = "btnPublish";
        btnPublish.Size = new Size(200, 36);
        btnPublish.TabIndex = 17;
        btnPublish.Text = "نشر";
        btnPublish.UseVisualStyleBackColor = false;
        //
        // lblPubStatus
        //
        lblPubStatus.AutoSize = true;
        lblPubStatus.BackColor = Color.Transparent;
        lblPubStatus.Font = new Font("Segoe UI", 9F);
        lblPubStatus.ForeColor = Color.FromArgb(150, 150, 150);
        lblPubStatus.Location = new Point(380, 508);
        lblPubStatus.MaximumSize = new Size(640, 20);
        lblPubStatus.Name = "lblPubStatus";
        lblPubStatus.Size = new Size(60, 15);
        lblPubStatus.TabIndex = 18;
        lblPubStatus.Text = "…";
        //
        // floatingDock (فقاعة سفلية عائمة)
        //
        floatingDock.Location = new Point(426, 568);
        floatingDock.Name = "floatingDock";
        floatingDock.Size = new Size(148, 60);
        floatingDock.TabIndex = 4;
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.Black;
        ClientSize = new Size(1000, 650);
        Controls.Add(floatingDock);
        Controls.Add(main);
        MinimumSize = new Size(820, 520);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "StarX";
        Load += Form1_Load;
        Resize += Form1_Resize;
        main.ResumeLayout(false);
        main.PerformLayout();
        pageSettings.ResumeLayout(false);
        pageSettings.PerformLayout();
        pagePro.ResumeLayout(false);
        pagePro.PerformLayout();
        pageOwner.ResumeLayout(false);
        pageOwner.PerformLayout();
        ResumeLayout(false);
    }

    #endregion
}
