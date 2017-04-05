using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using Sistema = System.Windows.Forms; // Esto es necesario por la ambigüedad que produce el System.Windows.Forms con el new Message de agsXMPP.
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using agsXMPP; // Esta es la DLL importante.
//https://www.ag-software.net/agsxmpp-sdk/download/

namespace ChatLoL
{
    public partial class Form1 : Sistema.Form
    {

        public XmppClientConnection xmpp = new XmppClientConnection("pvp.net");
        public ArrayList Lista; //Links .jpg de las fotos.
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e) // Boton para establecer la conexión.
        {
            //La información es oficial, y se encuentra en http://leagueoflegends.wikia.com/wiki/User_blog:Sevenix/Connecting_to_the_LoL_chat_using_XMPP
            xmpp.UseSSL = true;
            xmpp.Port = 5223;
            xmpp.AutoPresence = true;
            xmpp.AutoRoster = true;
            xmpp.ConnectServer = "chat.la2.lol.riotgames.com";
            xmpp.Open(textBox1.Text, "AIR_" + textBox2.Text);
            xmpp.OnLogin += delegate(object o) { Sistema.MessageBox.Show("Conectado"); };
            xmpp.OnRosterItem += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
            xmpp.OnPresence += new PresenceHandler(xmppCon_OnPresence);
        }

        private void button2_Click(object sender, EventArgs e) // Boton para enviar mensaje.
        {
            {
                agsXMPP.Jid JID = new Jid("sum"+ textBox3.Text + "@pvp.net"); // Se crea un objeto tipo JID(usuario).
                agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message(); // Se crea un objeto tipo mensaje.
                msg.Type = MessageType.chat; // Se define el tipo de mensaje.
                msg.To = JID; // Se define el destinatario.
                msg.Body = richTextBox1.Text; // Se define el mensaje en sí.
                xmpp.Send(msg); // Se envía el mensaje.
                textBox3.Clear(); // Limpia el textbox.
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            xmpp.Close();
            Sistema.Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e) //Comandos extraidos de la documentación.
        {
            listBox1.Items.Add(Lista[0].ToString());
        }
        void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item)
        {
            //Sistema.MessageBox.Show(Lista[0].ToString());
        }
        void xmppCon_OnPresence(object sender, Presence pres)
        {
            Lista = new ArrayList();
            Lista.Add(pres.From);
        }
    }

}
