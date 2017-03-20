using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ScheMe
{
    public abstract class UserAction
    {
        public delegate void UserActionEventArgs(Scheme scheme, UserAction action);
        public static event UserActionEventArgs OnAction;
        public static event UserActionEventArgs OnUndo;

        public bool Perform()
        {
            var page = (Window.Current.Content as Frame)?.Content as MainPage;
            if (page != null)
            {
                return Perform(page.Current, page.Canvas);
            }
            else return false;
        }

        public bool Perform(Scheme scheme, Canvas canvas)
        {
            if (perform(scheme, canvas))
            {
                OnAction?.Invoke(scheme, this);
                return true;
            }
            else return false;
        }


        public void Undo(Scheme scheme, Canvas canvas)
        {
            undo(scheme, canvas);
            OnUndo?.Invoke(scheme, this);
        }

        protected abstract bool perform(Scheme scheme, Canvas canvas);
        protected abstract void undo(Scheme scheme, Canvas canvas);
    }

    public class LineCreationAction : UserAction
    {
        Connection line;
        public LineCreationAction(Node from, Node to)
        {
            line = new Connection(from, to);
        }

        protected override bool perform(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Add(line);
            scheme.Connections.Add(line);
            line.From.Connections.Add(line.To, line);
            line.To.Connections.Add(line.From, line);
            return true;
        }

        protected override void undo(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Remove(line);
            scheme.Connections.Remove(line);
            line.From.Connections.Remove(line.To);
            line.To.Connections.Remove(line.From);
        }
    }

    public class NodeCreationAction : UserAction
    {
        Node node;
        public NodeCreationAction(double x, double y)
        {
            node = new Node();
            Canvas.SetLeft(node, x);
            Canvas.SetTop(node, y);
        }

        protected override bool perform(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Add(node);
            scheme.Nodes.Add(node);
            return true;
        }

        protected override void undo(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Remove(node);
            scheme.Nodes.Remove(node);
        }
    }

    public class NodeRemoveAction : UserAction
    {
        Node node;
        public NodeRemoveAction(Node node)
        {
            this.node = node;
        }

        protected override bool perform(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Remove(node);
            scheme.Nodes.Remove(node);

            foreach (var con in node.Connections)
            {
                canvas.Children.Remove(con.Value);
                scheme.Connections.Remove(con.Value);
            }
            
            return true;
        }

        protected override void undo(Scheme scheme, Canvas canvas)
        {
            canvas.Children.Add(node);
            scheme.Nodes.Add(node);

            foreach (var con in node.Connections)
            {
                canvas.Children.Add(con.Value);
                scheme.Connections.Add(con.Value);
            }
        }
    }
}
