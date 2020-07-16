using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ControlsApp
{

    public partial class MainWindow : Window
    {
        gtr gtr = new gtr();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length == 0)
            {
                gtr.n = 0;
            }
            else
                gtr.n = Int32.Parse(textBox.Text);

        }
        string[][] z;
        private void TextBox_TextChanged1(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length == 0)
            {
                gtr.m = 0;
            }
            else
                gtr.m = Int32.Parse(textBox.Text);


            z = new string[gtr.m][];
            for (int i = 0; i < gtr.m; i++)
            {
                z[i] = new string[4];
                for (int j = 0; j < 4; j++)
                    z[i][j] = "1";
            }
            grid.ItemsSource = z;
        }


        public void putGrid(DataGrid dataGrid, long[][] a)
        {
            long[][] temp = new long[gtr.n][];
            for (int i = 0; i < gtr.n; i++)
            {
                temp[i] = new long[gtr.n + 1];
                temp[i][0] = i + 1;
                for (int j = 0; j < gtr.n; j++)
                {

                    temp[i][j + 1] = a[i][j];
                }
            }


            dataGrid.Columns.Clear();
            Console.WriteLine(2);
            Style style = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "Your tool tip here"));
            style.Setters.Add(new Setter
            {
                Property = BackgroundProperty,
                Value
                = Brushes.Cyan
            });


            for (int i = 0; i < gtr.n + 1; i++)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Binding = new Binding(string.Format("[" + i.ToString() + "]"));
                col.Header = string.Format("[" + i.ToString() + "]");
                col.HeaderStyle = style;

                dataGrid.Columns.Add(col);

            }

            dataGrid.Columns[0].CanUserResize = false;
            dataGrid.Columns[0].IsReadOnly = true;
            dataGrid.Columns[0].CellStyle = new Style(typeof(DataGridCell));
            dataGrid.Columns[0].CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.Cyan)));
            dataGrid.ItemsSource = temp;

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gtr.initArr();
            for (int i = 0; i < z.Length; i++)
            {

                gtr.capacity[int.Parse(z[i][0]) - 1][int.Parse(z[i][1]) - 1] = int.Parse(z[i][2]);
                gtr.сurent_flow[int.Parse(z[i][0]) - 1, int.Parse(z[i][1]) - 1] = int.Parse(z[i][3]);
            }

            putGrid(grid1, gtr.capacity);
            gtr.max_flow();
            putGrid(grid2, gtr.flow);




        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            gtr.initArr();
            for (int i = 0; i < z.Length; i++)
            {

                gtr.capacity[int.Parse(z[i][0]) - 1][int.Parse(z[i][1]) - 1] = int.Parse(z[i][2]);
                gtr.сurent_flow[int.Parse(z[i][0]) - 1, int.Parse(z[i][1]) - 1] = int.Parse(z[i][3]);
            }
            putGrid(grid1, gtr.capacity);
            gtr.min_flow();
            putGrid(grid3, gtr.minflowmass);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsDigit(e.Text, 0));
        }
    }




    public class gtr
    {
        static long delta = long.MaxValue;
        public static int n = 10;
        public static int m = 0;
        public long[][] capacity; //значения дуг
        public long[,] сurent_flow;//поток по дугам(для поиска минимального пути)
        public long[][] flow;//поток по дугам(матрица смежности)
        public long[][] minflowmass;
        int[] color;//Отметки
        int[] pred;// поток по дугам сети
        int head, tail;
        int[] q;//
        int WHITE = 0;//
        int GREY = 1;//
        int BLACK = 2;//

        long maxflow = 0;
        long minflow = 0;


        public void initArr()
        {
            capacity = new long[n][];
            flow = new long[n][];
            сurent_flow = new long[n, n];
            minflowmass = new long[n][];
            pred = new int[n];
            color = new int[n];
            q = new int[n];
            for (int j = 0; j < gtr.n; j++)
            {
                capacity[j] = new long[n];
                flow[j] = new long[n];
                minflowmass[j] = new long[n];
            }
        }

        long min(long x, long y) // функция поиска минимума
        {
            if (x < y) return x; else return y;
        }

        void enque(int x)
        {
            q[tail] = x;
            tail++;
            color[x] = GREY;
        }

        int deque() // берет текущую вершину красит в черный 
        {
            int x = q[head];
            head++;
            color[x] = BLACK;
            return x;
        }

        int bfs(int start, int end)
        {
            int u;
            for (int i = 0; i < n; i++) // помечаем все вершины белым цветом(ничего не помечено)
                color[i] = WHITE;

            for (int i = 0; i < n; i++) // устанавливаем нулевой поток по дугам сети 
            {
                pred[i] = 0;
            }

            head = 0;
            tail = 0;

            enque(start); // начальную вершину помечаем серым
            pred[start] = -1; // поток на начальной вершине стави -1

            while (head != tail)
            {
                u = deque(); //выбирается следующая вершина помечается черным
                for (int v = 0; v < n; v++)
                {
                    if (color[v] == WHITE && (capacity[u][v] - flow[u][v]) > 0)
                    {
                        enque(v);
                        pred[v] = u;
                    }
                }
            }
            if (color[end] == BLACK)
                return 0;
            else return 1;
        }
        public void path(int vv)
        {
            if (vv != 0)
            {
                for (int i = 0; i < n; i++)
                {
                    if (i == vv && pred[i] != -1)
                    {
                        delta = min(delta, (capacity[pred[i]][i] - flow[pred[i]][i]));
                        path(pred[i]);
                        flow[pred[i]][i] += delta;
                        flow[i][pred[i]] -= delta;
                    }
                }

            }
        }
        public long max_flow()
        {

            while (bfs(0, n - 1) == 0)
            {
                delta = long.MaxValue;
                path(n - 1);
                maxflow += delta;
            }
            return maxflow;
        }
        public void min_flow()
        {


            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    capacity[i][j] = сurent_flow[i, j] - capacity[i][j];
                }
            }

            max_flow();

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    minflowmass[i][j] = сurent_flow[i, j] - flow[i][j];
                }
            }





        }
    }

}