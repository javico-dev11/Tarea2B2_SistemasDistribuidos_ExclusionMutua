using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Tarea2B2_SistemasDistribuidos
{
    public partial class FrmServer : Form
    {

        #region variables
        private TcpListener tcpListener;
        private Thread listenThread;
        private object lockObject = new object();
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private bool isListening = false;
        private IPAddress serverIPAddress;

        #endregion

        #region métodos y procedimientos

        private IPAddress GetLocalIPv4()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address;
                }
            }

            return IPAddress.Loopback;
        }

        private void ConnectServer()
        {
            if (!isListening)
            {

                tcpListener = new TcpListener(serverIPAddress, Convert.ToInt32(txtPort.Text));
                listenThread = new Thread(new ThreadStart(ListenForClients));
                listenThread.Start();
                txtLog.AppendText("Servidor iniciado. Esperando clientes..." + Environment.NewLine);
                btnConectar.Text = "&Detener";
                isListening = true;
            }
        }

        private void StopServer()
        {
            if (isListening)
            {
                isListening = false;
                tcpListener.Stop();
                listenThread.Join();

                // Cerrar y desconectar todos los clientes conectados.
                lock (lockObject)
                {
                    foreach (TcpClient client in connectedClients)
                    {
                        client.GetStream().Close();
                        client.Close();
                    }
                    connectedClients.Clear();
                }

                txtLog.AppendText("Servidor detenido." + Environment.NewLine);
                btnConectar.Text = "&Iniciar";
            }
        }

        private void ListenForClients()
        {
            tcpListener.Start();

            while (isListening)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                lock (lockObject)
                {
                    connectedClients.Add(client);
                }

                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientCommunication));
                clientThread.Start(client);
            }
        }

        private void HandleClientCommunication(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            string clientIPAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                try
                {
                    bytesRead = clientStream.Read(message, 0, message.Length);

                    if (bytesRead == 0)
                    {
                        break; // Cliente se desconectó, salimos del bucle
                    }

                    byte[] data = new byte[bytesRead];
                    Array.Copy(message, data, bytesRead);
                    string clientMessage = Encoding.UTF8.GetString(data);
                    DisplayMessage($"Cliente {clientIPAddress}: {clientMessage}");

                    string responseMessage = "Mensaje recibido: " + clientMessage;
                    byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);
                    lock (lockObject) // Exclusión mutua con lock
                    {
                        clientStream.Write(responseData, 0, responseData.Length);
                    }
                }
                catch (Exception)
                {
                    // Eliminamos al cliente desconectado de la lista y salimos del bucle.
                    lock (lockObject)
                    {
                        connectedClients.Remove(tcpClient);
                    }
                    break;
                }
            }

            clientStream.Close();
            tcpClient.Close();
        }

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
        public FrmServer()
        {
            InitializeComponent();

            serverIPAddress = GetLocalIPv4();
            txtIP.Text = serverIPAddress.ToString();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            if (btnConectar.Text == "&Iniciar")
            {
                ConnectServer();
            }
            else if (btnConectar.Text == "&Detener")
            {
                StopServer();
            }
        }
    }
}
