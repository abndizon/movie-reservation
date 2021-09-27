using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieReservation
{
    public partial class Form1 : Form
    {
        string[,] timeFrame = new string[4, 5]
        {
            {"8:00 AM", "10:15 AM", "4:32 PM", "2:45 PM", "5:00 PM" },
            {"8:00 AM", "10:45 PM", "1:30 PM", "4:15 PM", "7:00 PM" },
            {"8:00 AM", "10:20 AM", "12:40 PM", "3:00 PM", "5:20 PM" },
            {"8:00 AM", "10:25 AM", "12:55 PM", "2:10 PM", "5:25 PM" }
        };

        int x;
        bool done = false;

        FunctionClass fc = new FunctionClass();
        DialogResult dialog;

        public Form1()
        {
            InitializeComponent();
        }

        private void movieComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (movieComboBox.SelectedIndex > -1)
            {
                timeComboBox.SelectedIndex = -1;
                timeComboBox.Items.Clear();

                for (x = 0; x < 5; x++)
                {
                    timeComboBox.Items.Add(timeFrame[movieComboBox.SelectedIndex,x]);
                }
            }
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (movieComboBox.SelectedIndex > -1 && timeComboBox.SelectedIndex > -1)
                {
                    if (DateTime.Parse(dateTimePicker1.Value.ToString("d")) == DateTime.Parse(DateTime.Now.ToString("d")))
                    {
                        string outputTime = fc.ConvertTime(timeComboBox.SelectedItem.ToString());
                        if (fc.TimeIsGreaterOrEqual(outputTime, DateTime.Now.ToString("HH:mm:ss")))
                        {
                            dialog = MessageBox.Show("Time is already done. Cannot reserve seats in this time. You can only view the seats used.", "Reservation Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            done = true;

                            if (dialog == DialogResult.OK)
                            {
                                openMovieForm();
                            }
                            else if (dialog == DialogResult.Cancel)
                            {
                                done = false;
                            }
                        }
                        else
                        {
                            openMovieForm();
                        }
                    }
                    else
                    {
                        openMovieForm();
                    }
                }
                else
                {
                    MessageBox.Show("Input is not valid", "Select failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //-------------------------MY FUNCTIONS-------------------------------------

        private void openMovieForm ()
        {
            this.Hide();
            Form2 reservationForm = new Form2(movieComboBox.SelectedItem.ToString(), timeComboBox.SelectedItem.ToString(), done, dateTimePicker1.Value.ToString("d"));
            reservationForm.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dateTimePicker1.MinDate = DateTime.Today;
            dateTimePicker1.MaxDate = DateTime.Today.AddDays(7);

            Console.WriteLine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }
    }
}
