using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace ScheMe
{
    public class Scheme
    {
        public bool Saved { get; private set; } = true;
        public StorageFile File;

        public string Name = "";
        public List<Node> Nodes = new List<Node>();
        public List<Connection> Connections = new List<Connection>();

        public Scheme()
        {
            UserAction.OnAction += UserAction_OnAction;
            UserAction.OnUndo += UserAction_OnUndo;
        }

        private void UserAction_OnAction(Scheme scheme, UserAction action)
        {
            if (scheme == this) Saved = false;
        }
        private void UserAction_OnUndo(Scheme scheme, UserAction action)
        {
            if (scheme == this) Saved = false;
        }

        public async Task<bool> SaveAsync(StorageFile file)
        {
            if (file == null) return false;
            Stream stream = await file.OpenStreamForWriteAsync();
            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                writer.WriteStartDocument();
                //
                writer.WriteStartElement("scheme");
                if (Name != "") writer.WriteAttributeString("name", Name);
                var ver = Package.Current.Id.Version;
                writer.WriteAttributeString("version", ver.Major.ToString() + "." + ver.Minor.ToString() + ver.Build.ToString() + "." + ver.Revision.ToString());
                //nodes
                writer.WriteStartElement("nodes");
                foreach (var node in Nodes)
                {
                    writer.WriteStartElement("node");
                    writer.WriteAttributeString("name", node.NodeName);
                    writer.WriteAttributeString("x", Canvas.GetLeft(node).ToString());
                    writer.WriteAttributeString("y", Canvas.GetTop(node).ToString());
                    writer.WriteString(node.Description);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteStartElement("connections");
                foreach (var con in Connections)
                {
                    writer.WriteStartElement("con");
                    writer.WriteAttributeString("from", Nodes.IndexOf(con.From).ToString());
                    writer.WriteAttributeString("to", Nodes.IndexOf(con.To).ToString());

                    if (con.Bends.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var point in con.Bends)
                        {
                            sb.Append(point.X);
                            sb.Append(',');
                            sb.Append(point.Y);
                            sb.Append(';');
                        }
                        writer.WriteAttributeString("points", sb.ToString());
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }
            stream.SetLength(stream.Position);
            stream.Dispose();
            Saved = true;

            return true;
        }

        public static async Task<Scheme> LoadAsync(StorageFile file)
        {
            if (file == null) return null;

            Scheme scheme = new Scheme();
            scheme.File = file;
            using (var stream = await file.OpenStreamForReadAsync())
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);

                if (doc.DocumentElement.HasAttribute("name"))
                {
                    scheme.Name = doc.DocumentElement.GetAttribute("name");
                }

                var nodes = doc.DocumentElement.GetElementsByTagName("node");
                foreach (XmlElement node in nodes)
                {
                    Node newNode = new Node();

                    if (node.HasAttribute("x") && double.TryParse(node.GetAttribute("x"), out double x)) Canvas.SetLeft(newNode, x);
                    if (node.HasAttribute("y") && double.TryParse(node.GetAttribute("y"), out double y)) Canvas.SetTop(newNode, y);
                    if (node.HasAttribute("name")) newNode.NodeName = node.GetAttribute("name");
                    newNode.Description = node.InnerText;

                    scheme.Nodes.Add(newNode);
                }

                var connections = doc.DocumentElement.GetElementsByTagName("con");
                foreach (XmlElement con in connections)
                {
                    if (con.HasAttribute("from") && int.TryParse(con.GetAttribute("from"), out int from) && from >= 0 && from < scheme.Nodes.Count 
                        && con.HasAttribute("to") && int.TryParse(con.GetAttribute("to"), out int to) && to >= 0 && to < scheme.Nodes.Count)
                    {
                        if (con.HasAttribute("points"))
                        {
                            string coords = con.GetAttribute("points");
                        }
                        Connection connection = new Connection(scheme.Nodes[from], scheme.Nodes[to]);
                        scheme.Nodes[from].Connections.Add(scheme.Nodes[to], connection);
                        scheme.Nodes[to].Connections.Add(scheme.Nodes[from], connection);
                        scheme.Connections.Add(connection);
                    }
                }
            }

            return scheme;
        }
    }
}
