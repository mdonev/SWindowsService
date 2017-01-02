namespace SmtpWindowsService
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.spoolsProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.spools = new System.ServiceProcess.ServiceInstaller();
            // 
            // spoolsProcessInstaller
            // 
            this.spoolsProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.spoolsProcessInstaller.Password = null;
            this.spoolsProcessInstaller.Username = null;
            // 
            // spools
            // 
            this.spools.ServiceName = "spools";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.spoolsProcessInstaller,
            this.spools});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller spoolsProcessInstaller;
        private System.ServiceProcess.ServiceInstaller spools;
    }
}