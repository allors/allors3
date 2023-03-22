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
            this.button1 = new Button();
            this.personFormControllerBindingSource = new BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)this.personFormControllerBindingSource).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DataBindings.Add(new Binding("Command", this.personFormControllerBindingSource, "ShowDialogCommand", true));
            this.button1.Location = new Point(470, 90);
            this.button1.Name = "button1";
            this.button1.Size = new Size(94, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Dialog ?";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // personFormControllerBindingSource
            // 
            this.personFormControllerBindingSource.DataSource = typeof(Controllers.PersonFormController);
            // 
            // PersonForm
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1041, 501);
            this.Controls.Add(this.button1);
            this.Name = "PersonForm";
            this.Text = "PersonForm";
            this.DataContextChanged += this.PersonForm_DataContextChanged;
            ((System.ComponentModel.ISupportInitialize)this.personFormControllerBindingSource).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private BindingSource personFormControllerBindingSource;
    }
}