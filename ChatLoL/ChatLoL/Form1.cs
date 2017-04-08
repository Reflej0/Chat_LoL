using System;
using System.IO;
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
using agsXMPP; // Esta es la DLL importante.
using agsXMPP.protocol.iq.roster;
//https://www.ag-software.net/agsxmpp-sdk/download/

namespace ChatLoL
{
    public partial class Form1 : Sistema.Form
    {
        /*
         * Las variables globales me simplifican un par de cosas.
         */
        public XmppClientConnection xmpp = new XmppClientConnection("pvp.net");
        public List<string> ContactosConectados = new List<string>(); // En este List voy a guardar la lista de mis amigos, diferenciando si estan conectados o no.
        public List<RosterItem> Contactos = new List<RosterItem>(); // En este List voy a guardar la lista de mis amigos, sin diferenciar si estan conectados o no.
        public string ContactoSeleccionado; // En esta variable voy a guardar el ID del contacto que seleccione en el listbox.
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
            xmpp.OnMessage += new agsXMPP.protocol.client.MessageHandler(xmpp_OnMessage);
            xmpp.OnRosterItem += new XmppClientConnection.RosterHandler(xmppCon_OnRosterItem);
            xmpp.OnPresence += new PresenceHandler(xmppCon_OnPresence);
        }


        private void button2_Click(object sender, EventArgs e) // Boton para enviar mensaje.
        {
            {
                agsXMPP.Jid JID = new Jid(ContactoSeleccionado); // Se crea un objeto tipo JID(usuario).
                agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message(); // Se crea un objeto tipo mensaje.
                msg.Type = MessageType.chat; // Se define el tipo de mensaje.
                msg.To = JID; // Se define el destinatario.
                msg.Body = richTextBox1.Text; // Se define el mensaje en sí.
                xmpp.Send(msg); // Se envía el mensaje.
            }
        }

        private void button3_Click(object sender, EventArgs e) // Boton para salir.
        {
            xmpp.Close(); // Cierro la sesion del chat.
            Sistema.Application.Exit(); // Salgo de la aplicacion.
        }

        private void button4_Click(object sender, EventArgs e) //Al tocar el boton actualizar contactos.
        {
            /*
             * Esto es un poco dificil de entender, el tema es que tengo que recurrir a esto debido a que
             * Yo quiero saber los usuarios que estan activos para enviarles un mensaje.
             * La lista Contactos, tiene la lista de TODOS los usuarios que yo agregue sin importar si estan conectados o no.
             * La lista ContactosConectados, tiene la lista de los ID de usuarios que estan conectados
             * yo quiero mostrar el nombre de los conectados, es como hacer un INNERJOIN en SQL.
             * quiero mostrar los datos de Contactos, pero filtrando por Contactos conectados.
             * Lamentablemente el tipo de variable Presence, no tiene nombre de usuario.
             * Espero que se haya entendido.
            */
            List<RosterItem> ContactosAgregados = new List<RosterItem>(); // De este LIST voy a alimentar al listbox.
            foreach (RosterItem i in Contactos) // Recorro TODOS los usuarios.
            {
                string ID = i.Jid.ToString(); //El ID obviamente esta cambiando a cada rato.
                foreach(string j in ContactosConectados){ //Recorro SOLO los usuarios conectados.
                    
                if (j == ID.Remove(ID.Length - 8, 8)) //Esto esta explicado en xmpp_OnMessage
                {
                    ContactosAgregados.Add(i); //Finalmente agrego los usuarios conectados.
                }
                }
            }
            listBox1.DataSource = ContactosAgregados; // De que variable se alimenta el Listbox.
            listBox1.DisplayMember = "Name"; // De todos los atributos de Contactos, el nombre es el que aparece en el listbox.
        }
        private void xmppCon_OnRosterItem(object sender, agsXMPP.protocol.iq.roster.RosterItem item) // Este evento me trae TODOS mis contactos.
        {
            Contactos.Add(item); // Agrego la lista de usuarios.
        }
        private void xmppCon_OnPresence(object sender, Presence pres) // Este evento me trae los usuarios conectados.
        {
            ContactosConectados.Add(pres.From.User); // Agrego los usuarios conectados al List.
        }
        private void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg) // Este evento recibe los mensajes.
        {
            string quienmeenvia = ""; // Esta variable contiene el NOMBRE del invocador y no el ID.
            foreach (RosterItem i in Contactos) //Recorro los contactos.
            {
                string ID = i.Jid.ToString(); //El ID obviamente esta cambiando a cada rato.
                if (msg.From.User == ID.Remove(ID.Length-8,8)) // Esto es porque el From.User me da sum_xxxxx y el ID tiene un sum_xxxx@pvp.net
                {
                    quienmeenvia = i.Name.ToString(); //Ahora si, tengo el NOMBRE del invocador que me envio un mensaje.
                }
            }
            if (msg.Type == MessageType.chat) // Chequeo el tipo de mensaje.
            {
                Sistema.MessageBox.Show(quienmeenvia + ": " + msg.Body); // Le muestro quien le envia y que dice.

                /*Message newMsg = new Message(msg.From, "hello user");
                xmpp.Send(newMsg);
                ESTO NO SE PARA QUE ESTA TIPO FUNCIONA IGUAL*/
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) //Cuando clickeo en un contacto.
        {
            foreach(RosterItem i in Contactos){ //Recorro el list de contactos.
                if (i.Name == listBox1.Text)
                {
                    textBox3.Text = i.Name.ToString(); // Al usuario le muestro el amigo que selecciono, aunque es algo medio obvio.
                    ContactoSeleccionado = i.Jid.ToString(); // La variable publica ContactoSeleccionado tiene el ID del amigo al que le quiero mandar el mensaje.
                }
            }
            
            
        }
}

}
