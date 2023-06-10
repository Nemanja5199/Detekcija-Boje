using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        int Red, Green, Blue;
        private string selectedPort;
        SqlConnection conn;



        public Form1()
        {
            InitializeComponent();
            
            panel2.Visible = false;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            try
            {
                conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Boja;Integrated Security=True");
                conn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska prilikom povezivanja sa bazaom:" + ex.Message);
            }
            


            
            PopulateDataGridView();
        }


       


        private void PopulateDataGridView()
        {

            try
            {
                string query = "SELECT * FROM Boja";
                SqlCommand command = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(command);


                DataTable dataTable = new DataTable();


                adapter.Fill(dataTable);


                dataGridView1.DataSource = dataTable;
            }

            catch(Exception ex) { 

                MessageBox.Show("Greska prilikom Citanja iz baze:"+ex.Message);
            }   
            
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                if (comboBox1.SelectedItem != null)
                {

                
                        
                try
                {
                   
                    serialPort1.PortName = selectedPort;
                    serialPort1.Open();
                }
                catch (Exception ex)
                {
                    // Handle the exception here
                    MessageBox.Show("Greska prilikom otvaranje porta: " + ex.Message);
                }

                }

                else
                {
                    MessageBox.Show("Port nije selektovan");
                }






            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPort = comboBox1.SelectedItem.ToString();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    string data = serialPort1.ReadLine();
                    Invoke(new Action(() => textBox1.AppendText(data)));
                    Parsedata(data);

                    Invoke(new Action(() => Boja.BackColor = Color.FromArgb(Red, Green, Blue)));

                    DateTime date = DateTime.Now;


                    Insert(Red, Green, Blue, date);

                }
                catch(IOException ex)
                {
                    MessageBox.Show("Port je zatvoren:");
                }


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            panel2.Visible = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
          
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void Insert(int red, int green, int blue, DateTime dateTime)
        {
            try
            {

                string query = "INSERT INTO Boja (R, G, B, date) VALUES (@Red, @Green, @Blue, @DateTime)";

                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    
                    command.Parameters.AddWithValue("@Red", red);
                    command.Parameters.AddWithValue("@Green", green);
                    command.Parameters.AddWithValue("@Blue", blue);
                    command.Parameters.AddWithValue("@DateTime", dateTime);

                    
                    command.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Greska prilikom Unosa podataka u bazu:" + ex.Message);
            }
          
            
        }

        private void Parsedata(string data)
        {
            int redIndex = data.IndexOf("Red:");
            int greenIndex = data.IndexOf("Green:");
            int blueIndex = data.IndexOf("Blue:");

            if (redIndex >= 0 && greenIndex >= 0 && blueIndex >= 0)
            {
                int redValueStartIndex = redIndex + 5;
                int greenValueStartIndex = greenIndex + 7;
                int blueValueStartIndex = blueIndex + 6;

                // Find the end indices for the values
                int redValueEndIndex = data.IndexOf(" ", redValueStartIndex);
                int greenValueEndIndex = data.IndexOf(" ", greenValueStartIndex);
                int blueValueEndIndex = data.Length;

                // Extract the values
                int redValue;
                int greenValue;
                int blueValue;

                if (redValueEndIndex > redValueStartIndex)
                    redValue = int.Parse(data.Substring(redValueStartIndex, redValueEndIndex - redValueStartIndex));
                else
                    redValue = 0;

                if (greenValueEndIndex > greenValueStartIndex)
                    greenValue = int.Parse(data.Substring(greenValueStartIndex, greenValueEndIndex - greenValueStartIndex));
                else
                    greenValue = 0;

                if (blueValueEndIndex > blueValueStartIndex)
                    blueValue = int.Parse(data.Substring(blueValueStartIndex, blueValueEndIndex - blueValueStartIndex));
                else
                    blueValue = 0;

                    Red = redValue;
                    Green = greenValue;
                    Blue = blueValue;

                // Update the values in your application
                BeginInvoke(new Action(() => UpdateLabels(redValue, greenValue, blueValue)));
            }
        }

        private void UpdateLabels(int red, int green, int blue)
        {
            label1.Text = "R: " + red.ToString();
            label2.Text = "G: " + green.ToString();
            label3.Text = "B: " + blue.ToString();
        }
    }
}
