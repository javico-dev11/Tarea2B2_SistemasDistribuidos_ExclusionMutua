using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FrmCliente : Form
    {

        #region variables
        private TcpClient tcpClient;
        private NetworkStream clientStream;
        #endregion

        #region métodos y procedimientos
        private void DisplayMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(DisplayMessage), new object[] { message });
            }
            else
            {
                txtLog.AppendText(message + Environment.NewLine);
            }
        }
        #endregion

        #region constructor
        public FrmCliente()
        {
            InitializeComponent();
            btnSend.Enabled = false;
        }
        #endregion

        private void FrmCliente_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessageToSend.Text;
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientStream.Write(data, 0, data.Length);

                data = new byte[4096];
                int bytesRead = clientStream.Read(data, 0, data.Length);

                string response = Encoding.UTF8.GetString(data, 0, bytesRead);
                DisplayMessage("Respuesta del servidor: " + response);

                txtMessageToSend.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar al enviar mensaje al server: " + ex.Message);
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(txtIP.Text, Convert.ToInt32(txtPort.Text));
                clientStream = tcpClient.GetStream();
                btnSend.Enabled = true;
                btnConectar.Enabled = false;
                DisplayMessage("Conexión exitosa al servidor." + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar al servidor: " + ex.Message);
            }
        }

        private void txtMessageToSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnSend_Click(sender, e);
                 
                
            }
        }

        private void txtMessageToSend_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                btnSend_Click(sender, e);
                

            }
        }
    }
}
