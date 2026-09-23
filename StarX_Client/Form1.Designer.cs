namespace StarX_Client;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private LuxuryBackground main = null!;
    private Panel pageSettings = null!;
    private CardPanel cardLicense = null!;
    private Label lblLicStatus = null!;
    private GlassTextBox txtKey = null!;
    private GlassButton btnActivate = null!;
    private CardPanel cardOptions = null!;
    private LuxuryCheckBox chkStartup = null!;
    private LuxuryCheckBox chkTray = null!;
    private LuxuryCheckBox chkCloud = null!;
    private CardPanel cardLang = null!;
    private LuxuryComboBox cmbLang = null!;
    private GlassButton btnLangSave = null!;
    private TitleBar titleBar = null!;
    private UpdateIconButton btnUpd = null!;
    private Panel pagePro = null!;
    private Label lblProNote = null!;
    private ExitIconButton exitIcon = null!;
    private Label lblExit = null!;
    private Panel pageOwner = null!;
    private LuxuryButton btnAgent = null!;
    private Label lblAgentSub = null!;
    private Panel pageCleaner = null!;
    private LuxuryButton btnScan = null!;
    private LuxuryButton btnCancel = null!;
    private ProgressBar progressScan = null!;
    private Label lblScanStatus = null!;
    private LuxuryListView lvJunk = null!;
    private LuxuryButton btnSelectAll = null!;
    private LuxuryButton btnSelectNone = null!;
    private LuxuryButton btnSelectSafe = null!;
    private LuxuryButton btnExcludes = null!;
    private Label lblAiSummary = null!;
    private Label lblTotal = null!;
    private LuxuryButton btnClean = null!;
    private LuxuryButton btnCleanerBack = null!;
    private Label lblOnlineH = null!;
    private LuxuryListView lvOnline = null!;
    private Label lblLicH = null!;
    private LuxuryListView lvLicenses = null!;
    private LuxuryComboBox cmbDays = null!;
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
        cardLicense = new CardPanel();
        lblLicStatus = new Label();
        txtKey = new GlassTextBox();
        btnActivate = new GlassButton();
        cardOptions = new CardPanel();
        chkStartup = new LuxuryCheckBox();
        chkTray = new LuxuryCheckBox();
        chkCloud = new LuxuryCheckBox();
        cardLang = new CardPanel();
        cmbLang = new LuxuryComboBox();
        btnLangSave = new GlassButton();
        titleBar = new TitleBar();
        pagePro = new Panel();
        lblProNote = new Label();
        exitIcon = new ExitIconButton();
        lblExit = new Label();
        pageOwner = new Panel();
        btnAgent = new LuxuryButton();
        lblAgentSub = new Label();
        pageCleaner = new Panel();
        btnScan = new LuxuryButton();
        btnCancel = new LuxuryButton();
        progressScan = new ProgressBar();
        lblScanStatus = new Label();
        lvJunk = new LuxuryListView();
        btnSelectAll = new LuxuryButton();
        btnSelectNone = new LuxuryButton();
        btnSelectSafe = new LuxuryButton();
        btnExcludes = new LuxuryButton();
        lblAiSummary = new Label();
        lblTotal = new Label();
        btnClean = new LuxuryButton();
        btnCleanerBack = new LuxuryButton();
        lblOnlineH = new Label();
        lvOnline = new LuxuryListView();
        lblLicH = new Label();
        lvLicenses = new LuxuryListView();
        cmbDays = new LuxuryComboBox();
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
        cardLicense.SuspendLayout();
        cardOptions.SuspendLayout();
        cardLang.SuspendLayout();
        pagePro.SuspendLayout();
        pageOwner.SuspendLayout();
        pageCleaner.SuspendLayout();
        SuspendLayout();
        //
        // main (مسرح فخم متحرك يملأ الواجهة)
        //
        main.Controls.Add(pageSettings);
        main.Controls.Add(pagePro);
        main.Controls.Add(pageOwner);
        main.Controls.Add(btnAgent);
        main.Controls.Add(lblAgentSub);
        main.Controls.Add(pageCleaner);
        main.Location = new Point(0, 0);
        main.Name = "main";
        main.Size = new Size(1000, 650);
        main.TabIndex = 2;
        //
        // pageSettings (صفحة الإعدادات — ترتيب من فوق لتحت: خيارات / فحص / لغة / تحديث)
        //
        pageSettings.AutoScroll = true;
        pageSettings.BackColor = Color.Black;
        pageSettings.Controls.Add(cardLicense);
        cardLicense.Controls.Add(lblLicStatus);
        cardLicense.Controls.Add(txtKey);
        cardLicense.Controls.Add(btnActivate);
        pageSettings.Controls.Add(cardOptions);
        cardOptions.Controls.Add(chkStartup);
        cardOptions.Controls.Add(chkTray);
        cardOptions.Controls.Add(chkCloud);
        pageSettings.Controls.Add(cardLang);
        cardLang.Controls.Add(cmbLang);
        cardLang.Controls.Add(btnLangSave);
        pageSettings.Dock = DockStyle.Fill;
        pageSettings.Location = new Point(0, 0);
        pageSettings.Name = "pageSettings";
        pageSettings.Size = new Size(1000, 584);
        pageSettings.TabIndex = 1;
        pageSettings.Visible = false;
        //
        // cardLicense (الترخيص)
        //
        cardLicense.BackColor = Color.Black;
        cardLicense.HeaderText = "الترخيص";
        cardLicense.Location = new Point(190, 12);
        cardLicense.Name = "cardLicense";
        cardLicense.Size = new Size(620, 170);
        cardLicense.TabIndex = 0;
        //
        // lblLicStatus
        //
        lblLicStatus.AutoSize = true;
        lblLicStatus.BackColor = Color.Transparent;
        lblLicStatus.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLicStatus.ForeColor = Color.White;
        lblLicStatus.Location = new Point(28, 56);
        lblLicStatus.Name = "lblLicStatus";
        lblLicStatus.Size = new Size(140, 19);
        lblLicStatus.TabIndex = 2;
        lblLicStatus.Text = "…";
        //
        // txtKey
        //
        txtKey.BackColor = Color.Black;
        txtKey.BorderStyle = BorderStyle.FixedSingle;
        txtKey.Font = new Font("Segoe UI", 11F);
        txtKey.ForeColor = Color.White;
        txtKey.Location = new Point(28, 88);
        txtKey.Name = "txtKey";
        txtKey.PlaceholderText = "أدخل مفتاح التفعيل";
        txtKey.Size = new Size(360, 34);
        txtKey.TabIndex = 3;
        //
        // btnActivate
        //
        btnActivate.BackColor = Color.Black;
        btnActivate.Cursor = Cursors.Hand;
        btnActivate.FlatAppearance.BorderSize = 0;
        btnActivate.FlatStyle = FlatStyle.Flat;
        btnActivate.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnActivate.ForeColor = Color.Black;
        btnActivate.Location = new Point(400, 88);
        btnActivate.Name = "btnActivate";
        btnActivate.Primary = true;
        btnActivate.Size = new Size(192, 34);
        btnActivate.TabIndex = 4;
        btnActivate.Text = "تفعيل";
        btnActivate.UseVisualStyleBackColor = false;
        //
        // cardOptions (الخيارات: بدء التشغيل + البقاء في الدرج)
        //
        cardOptions.BackColor = Color.Black;
        cardOptions.HeaderText = "الخيارات";
        cardOptions.Location = new Point(190, 12);
        cardOptions.Name = "cardOptions";
        cardOptions.Size = new Size(620, 156);
        cardOptions.TabIndex = 0;
        //
        // chkStartup
        //
        chkStartup.AutoSize = false;
        chkStartup.BackColor = Color.Transparent;
        chkStartup.Font = new Font("Segoe UI", 10F);
        chkStartup.ForeColor = Color.White;
        chkStartup.Location = new Point(20, 52);
        chkStartup.Name = "chkStartup";
        chkStartup.Size = new Size(580, 32);
        chkStartup.TabIndex = 0;
        chkStartup.UseVisualStyleBackColor = false;
        //
        // chkTray
        //
        chkTray.AutoSize = false;
        chkTray.BackColor = Color.Transparent;
        chkTray.Font = new Font("Segoe UI", 10F);
        chkTray.ForeColor = Color.White;
        chkTray.Location = new Point(20, 88);
        chkTray.Name = "chkTray";
        chkTray.Size = new Size(580, 32);
        chkTray.TabIndex = 1;
        chkTray.UseVisualStyleBackColor = false;
        //
        // chkCloud
        //
        chkCloud.AutoSize = false;
        chkCloud.BackColor = Color.Transparent;
        chkCloud.Font = new Font("Segoe UI", 10F);
        chkCloud.ForeColor = Color.White;
        chkCloud.Location = new Point(20, 124);
        chkCloud.Name = "chkCloud";
        chkCloud.Size = new Size(580, 32);
        chkCloud.TabIndex = 2;
        chkCloud.UseVisualStyleBackColor = false;
        //
        // cardLang
        //
        cardLang.BackColor = Color.Black;
        cardLang.HeaderText = "اللغة";
        cardLang.Location = new Point(190, 146);
        cardLang.Name = "cardLang";
        cardLang.Size = new Size(620, 108);
        cardLang.TabIndex = 7;
        //
        // cmbLang
        //
        cmbLang.BackColor = Color.Black;
        cmbLang.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbLang.FlatStyle = FlatStyle.Flat;
        cmbLang.Font = new Font("Segoe UI", 10F);
        cmbLang.ForeColor = Color.White;
        cmbLang.FormattingEnabled = true;
        cmbLang.Location = new Point(28, 54);
        cmbLang.Name = "cmbLang";
        cmbLang.Size = new Size(250, 30);
        cmbLang.TabIndex = 7;
        //
        // btnLangSave
        //
        btnLangSave.BackColor = Color.Black;
        btnLangSave.Cursor = Cursors.Hand;
        btnLangSave.FlatAppearance.BorderSize = 0;
        btnLangSave.FlatStyle = FlatStyle.Flat;
        btnLangSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnLangSave.ForeColor = Color.Black;
        btnLangSave.Location = new Point(292, 48);
        btnLangSave.Name = "btnLangSave";
        btnLangSave.Size = new Size(128, 38);
        btnLangSave.TabIndex = 8;
        btnLangSave.Text = "حفظ";
        btnLangSave.UseVisualStyleBackColor = false;
        //
        // pagePro (صفحة البرو)
        //
        pagePro.BackColor = Color.Black;
        pagePro.Controls.Add(lblProNote);
        pagePro.Controls.Add(exitIcon);
        pagePro.Controls.Add(lblExit);
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
        lblProNote.BackColor = Color.Black;
        lblProNote.Font = new Font("Segoe UI", 10F);
        lblProNote.ForeColor = Color.White;
        lblProNote.Location = new Point(408, 170);
        lblProNote.Name = "lblProNote";
        lblProNote.Size = new Size(180, 19);
        lblProNote.TabIndex = 2;
        lblProNote.Text = "مميزات البرو — قريباً";
        //
        // exitIcon (زر تسجيل الخروج الأحمر فوق شمال صفحة البرو)
        //
        exitIcon.Location = new Point(14, 12);
        exitIcon.Name = "exitIcon";
        exitIcon.Size = new Size(36, 36);
        exitIcon.TabIndex = 3;
        //
        // lblExit (نص تسجيل الخروج بجانب الأيقونة)
        //
        lblExit.AutoSize = true;
        lblExit.BackColor = Color.Black;
        lblExit.Cursor = Cursors.Hand;
        lblExit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblExit.ForeColor = Color.FromArgb(229, 57, 53);
        lblExit.Location = new Point(58, 20);
        lblExit.Name = "lblExit";
        lblExit.Size = new Size(100, 19);
        lblExit.TabIndex = 4;
        lblExit.Text = "تسجيل الخروج";
        //
        // pageOwner (لوحة المالك)
        //
        pageOwner.BackColor = Color.Black;
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
        lblOnlineH.BackColor = Color.Black;
        lblOnlineH.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblOnlineH.ForeColor = Color.White;
        lblOnlineH.Location = new Point(430, 76);
        lblOnlineH.Name = "lblOnlineH";
        lblOnlineH.Size = new Size(140, 19);
        lblOnlineH.TabIndex = 2;
        lblOnlineH.Text = "المتصلون الآن";
        //
        // lvOnline
        //
        lvOnline.BackColor = Color.Black;
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
        lblLicH.BackColor = Color.Black;
        lblLicH.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblLicH.ForeColor = Color.White;
        lblLicH.Location = new Point(447, 192);
        lblLicH.Name = "lblLicH";
        lblLicH.Size = new Size(110, 19);
        lblLicH.TabIndex = 4;
        lblLicH.Text = "المفاتيح";
        //
        // lvLicenses
        //
        lvLicenses.BackColor = Color.Black;
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
        btnCreate.BackColor = Color.Black;
        btnCreate.FlatAppearance.BorderColor = Color.White;
        btnCreate.Cursor = Cursors.Hand;
        btnCreate.FlatAppearance.BorderSize = 1;
        btnCreate.FlatStyle = FlatStyle.Flat;
        btnCreate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnCreate.ForeColor = Color.White;
        btnCreate.Location = new Point(275, 308);
        btnCreate.Name = "btnCreate";
        btnCreate.Size = new Size(140, 34);
        btnCreate.TabIndex = 6;
        btnCreate.Text = "إنشاء مفتاح";
        btnCreate.UseVisualStyleBackColor = false;
        //
        // txtNote
        //
        txtNote.BackColor = Color.Black;
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
        cmbDays.BackColor = Color.Black;
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
        txtNewKey.BackColor = Color.Black;
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
        btnRefresh.BackColor = Color.Black;
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.FlatAppearance.BorderColor = Color.White;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.FlatAppearance.MouseDownBackColor = Color.Black;
        btnRefresh.FlatAppearance.MouseOverBackColor = Color.Black;
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
        btnDelete.BackColor = Color.Black;
        btnDelete.Cursor = Cursors.Hand;
        btnDelete.FlatAppearance.BorderColor = Color.White;
        btnDelete.FlatAppearance.BorderSize = 1;
        btnDelete.FlatAppearance.MouseDownBackColor = Color.Black;
        btnDelete.FlatAppearance.MouseOverBackColor = Color.Black;
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
        lblOwnStatus.BackColor = Color.Black;
        lblOwnStatus.Font = new Font("Segoe UI", 9F);
        lblOwnStatus.ForeColor = Color.White;
        lblOwnStatus.Location = new Point(380, 380);
        lblOwnStatus.Name = "lblOwnStatus";
        lblOwnStatus.Size = new Size(60, 15);
        lblOwnStatus.TabIndex = 12;
        lblOwnStatus.Text = "…";
        //
        // lblPubH
        //
        lblPubH.AutoSize = true;
        lblPubH.BackColor = Color.Black;
        lblPubH.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblPubH.ForeColor = Color.White;
        lblPubH.Location = new Point(435, 404);
        lblPubH.Name = "lblPubH";
        lblPubH.Size = new Size(130, 20);
        lblPubH.TabIndex = 13;
        lblPubH.Text = "نشر تحديث جديد";
        //
        // txtPubVersion
        //
        txtPubVersion.BackColor = Color.Black;
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
        txtToken.BackColor = Color.Black;
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
        txtPubNotes.BackColor = Color.Black;
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
        btnPublish.BackColor = Color.Black;
        btnPublish.FlatAppearance.BorderColor = Color.White;
        btnPublish.Cursor = Cursors.Hand;
        btnPublish.FlatAppearance.BorderSize = 1;
        btnPublish.FlatStyle = FlatStyle.Flat;
        btnPublish.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnPublish.ForeColor = Color.White;
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
        lblPubStatus.BackColor = Color.Black;
        lblPubStatus.Font = new Font("Segoe UI", 9F);
        lblPubStatus.ForeColor = Color.White;
        lblPubStatus.Location = new Point(380, 508);
        lblPubStatus.MaximumSize = new Size(640, 20);
        lblPubStatus.Name = "lblPubStatus";
        lblPubStatus.Size = new Size(60, 15);
        lblPubStatus.TabIndex = 18;
        lblPubStatus.Text = "…";
        //
        // btnAgent (زر الوكيل الذكي في الهوم)
        //
        btnAgent.BackColor = Color.Black;
        btnAgent.Cursor = Cursors.Hand;
        btnAgent.FlatAppearance.BorderSize = 0;
        btnAgent.FlatStyle = FlatStyle.Flat;
        btnAgent.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        btnAgent.ForeColor = Color.Black;
        btnAgent.Location = new Point(350, 220);
        btnAgent.Name = "btnAgent";
        btnAgent.Size = new Size(300, 64);
        btnAgent.TabIndex = 5;
        btnAgent.Text = "فحص ذكي للجهاز";
        btnAgent.UseVisualStyleBackColor = false;
        //
        // lblAgentSub
        //
        lblAgentSub.AutoSize = true;
        lblAgentSub.BackColor = Color.Black;
        lblAgentSub.Font = new Font("Segoe UI", 10F);
        lblAgentSub.ForeColor = Color.White;
        lblAgentSub.Location = new Point(330, 292);
        lblAgentSub.Name = "lblAgentSub";
        lblAgentSub.Size = new Size(340, 19);
        lblAgentSub.TabIndex = 6;
        lblAgentSub.Text = "نظف الملفات المؤقتة بأمان — أنت تختار ما يُمسح";
        //
        // pageCleaner (صفحة الفحص الذكي)
        //
        pageCleaner.BackColor = Color.Black;
        pageCleaner.Controls.Add(btnScan);
        pageCleaner.Controls.Add(btnCancel);
        pageCleaner.Controls.Add(progressScan);
        pageCleaner.Controls.Add(lblScanStatus);
        pageCleaner.Controls.Add(lvJunk);
        pageCleaner.Controls.Add(btnSelectAll);
        pageCleaner.Controls.Add(btnSelectNone);
        pageCleaner.Controls.Add(btnSelectSafe);
        pageCleaner.Controls.Add(btnExcludes);
        pageCleaner.Controls.Add(lblTotal);
        pageCleaner.Controls.Add(btnClean);
        pageCleaner.Controls.Add(lblAiSummary);
        pageCleaner.Controls.Add(btnCleanerBack);
        pageCleaner.Dock = DockStyle.Fill;
        pageCleaner.Location = new Point(0, 0);
        pageCleaner.Name = "pageCleaner";
        pageCleaner.Size = new Size(1000, 584);
        pageCleaner.TabIndex = 6;
        pageCleaner.Visible = false;
        //
        // btnScan
        //
        btnScan.BackColor = Color.Black;
        btnScan.Cursor = Cursors.Hand;
        btnScan.FlatAppearance.BorderSize = 0;
        btnScan.FlatStyle = FlatStyle.Flat;
        btnScan.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnScan.ForeColor = Color.Black;
        btnScan.Location = new Point(390, 24);
        btnScan.Name = "btnScan";
        btnScan.Size = new Size(220, 42);
        btnScan.TabIndex = 0;
        btnScan.Text = "بدء الفحص الذكي";
        btnScan.UseVisualStyleBackColor = false;
        //
        // btnCancel
        //
        btnCancel.BackColor = Color.Black;
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.FlatAppearance.BorderColor = Color.White;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.Font = new Font("Segoe UI", 10F);
        btnCancel.ForeColor = Color.White;
        btnCancel.Location = new Point(390, 24);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(220, 42);
        btnCancel.TabIndex = 1;
        btnCancel.Text = "إلغاء";
        btnCancel.UseVisualStyleBackColor = false;
        btnCancel.Primary = false;
        btnCancel.Visible = false;
        //
        // progressScan
        //
        progressScan.Location = new Point(220, 76);
        progressScan.Name = "progressScan";
        progressScan.Size = new Size(560, 16);
        progressScan.TabIndex = 2;
        progressScan.TabStop = false;
        //
        // lblScanStatus
        //
        lblScanStatus.AutoSize = true;
        lblScanStatus.BackColor = Color.Black;
        lblScanStatus.Font = new Font("Segoe UI", 9F);
        lblScanStatus.ForeColor = Color.White;
        lblScanStatus.Location = new Point(380, 98);
        lblScanStatus.MaximumSize = new Size(640, 30);
        lblScanStatus.Name = "lblScanStatus";
        lblScanStatus.Size = new Size(60, 15);
        lblScanStatus.TabIndex = 3;
        lblScanStatus.Text = "…";
        //
        // lvJunk
        //
        lvJunk.BackColor = Color.Black;
        lvJunk.BorderStyle = BorderStyle.FixedSingle;
        lvJunk.CheckBoxes = true;
        lvJunk.Font = new Font("Segoe UI", 9F);
        lvJunk.ForeColor = Color.White;
        lvJunk.FullRowSelect = true;
        lvJunk.GridLines = true;
        lvJunk.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        lvJunk.Location = new Point(120, 128);
        lvJunk.MultiSelect = false;
        lvJunk.Name = "lvJunk";
        lvJunk.Size = new Size(760, 220);
        lvJunk.TabIndex = 4;
        lvJunk.UseCompatibleStateImageBehavior = false;
        lvJunk.View = View.Details;
        //
        // btnSelectAll
        //
        btnSelectAll.BackColor = Color.Black;
        btnSelectAll.Cursor = Cursors.Hand;
        btnSelectAll.FlatAppearance.BorderColor = Color.White;
        btnSelectAll.FlatAppearance.BorderSize = 1;
        btnSelectAll.FlatStyle = FlatStyle.Flat;
        btnSelectAll.Font = new Font("Segoe UI", 9F);
        btnSelectAll.ForeColor = Color.White;
        btnSelectAll.Location = new Point(300, 356);
        btnSelectAll.Name = "btnSelectAll";
        btnSelectAll.Size = new Size(130, 32);
        btnSelectAll.TabIndex = 5;
        btnSelectAll.Text = "تحديد الكل";
        btnSelectAll.UseVisualStyleBackColor = false;
        btnSelectAll.Primary = false;
        //
        // btnSelectNone
        //
        btnSelectNone.BackColor = Color.Black;
        btnSelectNone.Cursor = Cursors.Hand;
        btnSelectNone.FlatAppearance.BorderColor = Color.White;
        btnSelectNone.FlatAppearance.BorderSize = 1;
        btnSelectNone.FlatStyle = FlatStyle.Flat;
        btnSelectNone.Font = new Font("Segoe UI", 9F);
        btnSelectNone.ForeColor = Color.White;
        btnSelectNone.Location = new Point(440, 356);
        btnSelectNone.Name = "btnSelectNone";
        btnSelectNone.Size = new Size(130, 32);
        btnSelectNone.TabIndex = 6;
        btnSelectNone.Text = "إلغاء التحديد";
        btnSelectNone.UseVisualStyleBackColor = false;
        btnSelectNone.Primary = false;
        //
        // btnSelectSafe (تحديد ما يوصي به الذكاء)
        //
        btnSelectSafe.BackColor = Color.White;
        btnSelectSafe.Cursor = Cursors.Hand;
        btnSelectSafe.FlatAppearance.BorderSize = 0;
        btnSelectSafe.FlatStyle = FlatStyle.Flat;
        btnSelectSafe.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSelectSafe.ForeColor = Color.Black;
        btnSelectSafe.Location = new Point(580, 356);
        btnSelectSafe.Name = "btnSelectSafe";
        btnSelectSafe.Size = new Size(170, 32);
        btnSelectSafe.TabIndex = 12;
        btnSelectSafe.Text = "تحديد الآمن ✓";
        btnSelectSafe.UseVisualStyleBackColor = false;
        //
        // btnExcludes (المجلدات المستثناة — لا يمسها الفحص)
        //
        btnExcludes.BackColor = Color.Black;
        btnExcludes.Cursor = Cursors.Hand;
        btnExcludes.FlatAppearance.BorderColor = Color.White;
        btnExcludes.FlatAppearance.BorderSize = 1;
        btnExcludes.FlatStyle = FlatStyle.Flat;
        btnExcludes.Font = new Font("Segoe UI", 9F);
        btnExcludes.ForeColor = Color.White;
        btnExcludes.Location = new Point(200, 356);
        btnExcludes.Name = "btnExcludes";
        btnExcludes.Primary = false;
        btnExcludes.Size = new Size(150, 32);
        btnExcludes.TabIndex = 14;
        btnExcludes.Text = "استثناءات…";
        btnExcludes.UseVisualStyleBackColor = false;
        //
        // lblTotal
        //
        lblTotal.AutoSize = true;
        lblTotal.BackColor = Color.Black;
        lblTotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTotal.ForeColor = Color.White;
        lblTotal.Location = new Point(420, 396);
        lblTotal.Name = "lblTotal";
        lblTotal.Size = new Size(160, 20);
        lblTotal.TabIndex = 7;
        lblTotal.Text = "المحدد: 0 B";
        //
        // btnClean
        //
        btnClean.BackColor = Color.Black;
        btnClean.Cursor = Cursors.Hand;
        btnClean.FlatAppearance.BorderSize = 0;
        btnClean.FlatStyle = FlatStyle.Flat;
        btnClean.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnClean.ForeColor = Color.Black;
        btnClean.Location = new Point(390, 424);
        btnClean.Name = "btnClean";
        btnClean.Size = new Size(220, 42);
        btnClean.TabIndex = 8;
        btnClean.Text = "مسح المحدد";
        btnClean.UseVisualStyleBackColor = false;
        //
        // lblAiSummary (ملخص الوكيل الذكي)
        //
        lblAiSummary.AutoSize = true;
        lblAiSummary.BackColor = Color.Black;
        lblAiSummary.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblAiSummary.ForeColor = Color.White;
        lblAiSummary.Location = new Point(300, 478);
        lblAiSummary.MaximumSize = new Size(700, 40);
        lblAiSummary.Name = "lblAiSummary";
        lblAiSummary.Size = new Size(60, 19);
        lblAiSummary.TabIndex = 13;
        lblAiSummary.Text = "";
        //
        // btnCleanerBack
        //
        btnCleanerBack.BackColor = Color.Black;
        btnCleanerBack.Cursor = Cursors.Hand;
        btnCleanerBack.FlatAppearance.BorderColor = Color.White;
        btnCleanerBack.FlatAppearance.BorderSize = 1;
        btnCleanerBack.FlatStyle = FlatStyle.Flat;
        btnCleanerBack.Font = new Font("Segoe UI", 9F);
        btnCleanerBack.ForeColor = Color.White;
        btnCleanerBack.Location = new Point(884, 12);
        btnCleanerBack.Name = "btnCleanerBack";
        btnCleanerBack.Size = new Size(100, 30);
        btnCleanerBack.TabIndex = 9;
        btnCleanerBack.Text = "رجوع";
        btnCleanerBack.UseVisualStyleBackColor = false;
        btnCleanerBack.Primary = false;
        //
        // floatingDock (فقاعة سفلية عائمة)
        //
        floatingDock.Location = new Point(426, 568);
        floatingDock.Name = "floatingDock";
        floatingDock.Size = new Size(140, 60);
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
        Controls.Add(titleBar);
        FormBorderStyle = FormBorderStyle.None;
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
        cardLicense.ResumeLayout(false);
        cardLicense.PerformLayout();
        cardOptions.ResumeLayout(false);
        cardOptions.PerformLayout();
        cardLang.ResumeLayout(false);
        cardLang.PerformLayout();
        pagePro.ResumeLayout(false);
        pagePro.PerformLayout();
        pageOwner.ResumeLayout(false);
        pageOwner.PerformLayout();
        pageCleaner.ResumeLayout(false);
        pageCleaner.PerformLayout();
        ResumeLayout(false);
    }

    #endregion
}
