namespace Workspace.ViewModels.WinForms.Forms
{
    partial class PersonForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(PersonForm));
            this.personFormControllerBindingSource = new BindingSource(this.components);
            this.toolStrip1 = new ToolStrip();
            this.loadToolStripButton = new ToolStripButton();
            this.saveToolStripButton = new ToolStripButton();
            this.showDialogToolStripButton = new ToolStripButton();
            this.splitContainer1 = new SplitContainer();
            this.dataGridView1 = new DataGridView();
            this.dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            this.peopleBindingSource = new BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)this.personFormControllerBindingSource).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.peopleBindingSource).BeginInit();
            this.SuspendLayout();
            // 
            // personFormControllerBindingSource
            // 
            this.personFormControllerBindingSource.DataSource = typeof(Controllers.PersonFormController);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new Size(20, 20);
            this.toolStrip1.Items.AddRange(new ToolStripItem[] { this.loadToolStripButton, this.saveToolStripButton, this.showDialogToolStripButton });
            this.toolStrip1.Location = new Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new Size(1041, 27);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // loadToolStripButton
            // 
            this.loadToolStripButton.DataBindings.Add(new Binding("Command", this.personFormControllerBindingSource, "LoadCommand", true));
            this.loadToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.loadToolStripButton.Image = (Image)resources.GetObject("loadToolStripButton.Image");
            this.loadToolStripButton.ImageTransparentColor = Color.Magenta;
            this.loadToolStripButton.Name = "loadToolStripButton";
            this.loadToolStripButton.Size = new Size(46, 24);
            this.loadToolStripButton.Text = "Load";
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.DataBindings.Add(new Binding("Command", this.personFormControllerBindingSource, "SaveCommand", true));
            this.saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.saveToolStripButton.Image = (Image)resources.GetObject("saveToolStripButton.Image");
            this.saveToolStripButton.ImageTransparentColor = Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new Size(44, 24);
            this.saveToolStripButton.Text = "Save";
            // 
            // showDialogToolStripButton
            // 
            this.showDialogToolStripButton.DataBindings.Add(new Binding("Command", this.personFormControllerBindingSource, "ShowDialogCommand", true));
            this.showDialogToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.showDialogToolStripButton.Image = (Image)resources.GetObject("showDialogToolStripButton.Image");
            this.showDialogToolStripButton.ImageTransparentColor = Color.Magenta;
            this.showDialogToolStripButton.Name = "showDialogToolStripButton";
            this.showDialogToolStripButton.Size = new Size(98, 24);
            this.showDialogToolStripButton.Text = "Show Dialog";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            this.splitContainer1.Size = new Size(1041, 474);
            this.splitContainer1.SplitterDistance = 347;
            this.splitContainer1.TabIndex = 2;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new DataGridViewColumn[] { this.dataGridViewTextBoxColumn1, this.dataGridViewTextBoxColumn2 });
            this.dataGridView1.DataSource = this.peopleBindingSource;
            this.dataGridView1.Dock = DockStyle.Fill;
            this.dataGridView1.Location = new Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 29;
            this.dataGridView1.Size = new Size(347, 474);
            this.dataGridView1.TabIndex = 0;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "FirstName";
            this.dataGridViewTextBoxColumn1.HeaderText = "FirstName";
            this.dataGridViewTextBoxColumn1.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "LastName";
            this.dataGridViewTextBoxColumn2.HeaderText = "LastName";
            this.dataGridViewTextBoxColumn2.MinimumWidth = 6;
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Width = 125;
            // 
            // peopleBindingSource
            // 
            this.peopleBindingSource.DataMember = "People";
            this.peopleBindingSource.DataSource = this.personFormControllerBindingSource;
            // 
            // PersonForm
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1041, 501);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "PersonForm";
            this.Text = "PersonForm";
            this.DataContextChanged += this.PersonForm_DataContextChanged;
            ((System.ComponentModel.ISupportInitialize)this.personFormControllerBindingSource).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.peopleBindingSource).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
        private BindingSource personFormControllerBindingSource;
        private ToolStrip toolStrip1;
        private ToolStripButton showDialogToolStripButton;
        private SplitContainer splitContainer1;
        private DataGridView dataGridView1;
        private DataGridViewTextBoxColumn firstNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn lastNameDataGridViewTextBoxColumn;
        private ToolStripButton loadToolStripButton;
        private ToolStripButton saveToolStripButton;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private BindingSource peopleBindingSource;
    }
}
