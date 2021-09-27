using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace MovieReservation
{
    public partial class Form2 : Form
    { 
        //Initialization
        int x, i;

        string outputTime;
        string movie = "";
        string time = "";
        string xml;
        string date = "";

        string[] listArray;

        double seatPrice = 0.0;
        double totalPrice = 0.0;
        int seatCount = 0;

        List<string> toBeReservedSeats = new List<string>();
        List<string> reservedSeats = new List<string>();
        List<string> usedSeats = new List<string>();

        List<string> timestamp = new List<string>();
        List<string> customerName = new List<string>();
        List<string> seats = new List<string>();
        List<string> status = new List<string>();

        bool cancelReservationButtonLastClick = false;
        bool isDone = false;
        bool errorFound = false;

        Timer timer1;
        Timer timer2;
        DialogResult dialogResult;

        Seats movieSeats = new Seats();
        FunctionClass fc = new FunctionClass();

        public Form2(string selectedMovie, string selectedTime, bool done, string selectedDate)
        {
            movie = selectedMovie;
            time = selectedTime;
            isDone = done;
            date = selectedDate;
            InitializeComponent();
        }

        //Seat Button Click
        private void seatButton_Click(object sender, EventArgs e)
        {
            // Get the sender as a Button.
            Button btn = sender as Button;

            if (btn.BackColor == Color.LimeGreen)
            {
                if (!cancelReservationButtonLastClick)
                {
                    btn.BackColor = Color.Gold;
                    seatCount++;
                    totalPrice += seatPrice;

                    displaySeatInfo();
                }
                else
                {
                    btn.BackColor = Color.Red;
                }
                
            }
            else if (btn.BackColor == Color.Gold)
            {
                btn.BackColor = Color.LimeGreen;
                seatCount--;
                totalPrice -= seatPrice;

                displaySeatInfo();
            }
            else if (btn.BackColor==Color.DarkGray)
            {
                MessageBox.Show("Seat is already in used", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (btn.BackColor == Color.Red)
            {
                if (!cancelReservationButtonLastClick)
                {
                    MessageBox.Show("Seat is already reserved", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    btn.BackColor = Color.LimeGreen;
                }
            }
        }

        //Reserve Button Click
        private void reserveButton_Click(object sender, EventArgs e)
        {
            //Reset
            toBeReservedSeats.Clear();

            //Get selected seats
            for (x = 1; x <= 18; x++)
            {
                Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;

                if (btn.BackColor == Color.Gold)
                {
                    toBeReservedSeats.Add(btn.Text);
                }
            }

            
            //Check if there is seats selected
            if (toBeReservedSeats.Count > 0)
            {
                //Check if name length is greater than 1
                if (customerTextBox.Text.Length > 1)
                {
                    errorFound = false;
                    //Check if textbox characters is valid for name
                    for (x = 0; x < customerTextBox.Text.Length; x++)
                    {
                        if ((!Char.IsLetter(customerTextBox.Text[x])) && (customerTextBox.Text[x] != ' '))
                        {
                            errorFound = true;
                            break;
                        }
                    }

                    if (errorFound)
                    {
                        MessageBox.Show("Name is not valid", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        //Generate message confirmation
                        string message = "Name: " + customerTextBox.Text + "\nMovie: " + movie + "\n\nSELECTED SEATS:\n";
                        
                        string toBeReservedString = "";
                        int counter = 0;

                        foreach (string str in toBeReservedSeats)
                        {
                            message += "Seat#" + str + ": " + seatPrice + "\n";
                        }

                        message += "\nTotal Amount: " + totalPrice;
                        
                        dialogResult = MessageBox.Show(message, "Confirm Reservation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

                        if (dialogResult == DialogResult.OK)
                        {
                            //Add to reserved seats
                            foreach (string str in toBeReservedSeats)
                            {
                                counter++;
                                reservedSeats.Add(time + "-" + str + "-" + customerTextBox.Text + "-" + date);

                                if (counter != toBeReservedSeats.Count)
                                    toBeReservedString += str + ",";
                                else
                                    toBeReservedString += str;
                            }

                            //For history input
                            historyDataGridView.Rows.Insert(0, DateTime.Now.ToString(), customerTextBox.Text, toBeReservedString, "Reserved");
                            FunctionClass.historyList.Add(DateTime.Now.ToString() + "|" + customerTextBox.Text + "|" + toBeReservedString + "|Reserved");

                            //Clear list
                            toBeReservedSeats.Clear();

                            //Refresh
                            RefreshForm(Color.Red, reservedSeats);
                            totalPrice = 0.0;
                            seatCount = 0;

                            displaySeatInfo();

                            //Save to XML
                            movieSeats._reservedSeats = reservedSeats;
                            saveData();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Name is not valid", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("There are no seats selected", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            ClearFunction();
        }
        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearFunction();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Are you sure you want to exit?", "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question); ;

            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void cancelReservationButton_Click(object sender, EventArgs e)
        {
            //Check if there are reservations
            if (reservedSeats.Count>0)
            {
                if (IfSameDateAndTime())
                {
                    //Disable controls and enable cancellation of reservations
                    customerTextBox.Enabled = false;
                    reserveButton.Enabled = false;
                    clearButton.Enabled = false;
                    cancelButton.Visible = true;
                    cancelReservationButton.Enabled = false;
                    backButton.Enabled = false;
                    okayButton.Visible = true;

                    for (x = 1; x <= 18; x++)
                    {
                        Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;

                        if (btn.BackColor != Color.Red)
                        {
                            btn.Enabled = false;
                        }
                        else
                        {
                            btn.FlatStyle = FlatStyle.Popup;
                        }
                    }

                    cancelReservationButtonLastClick = true;
                }
                else
                {
                    MessageBox.Show("There are no reservations", "Cancel Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
            }
            else
            {
                MessageBox.Show("There are no reservations", "Cancel Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            CancelButtonFunction();
        }

        //Okay button for cancellation of reservation
        private void okayButton_Click(object sender, EventArgs e)
        {
            //Reset values
            List<string> toBeCancelledList = new List<string>();
            List<string> toBeCancelledName = new List<string>();
            List<string> removeList = new List<string>();

            timestamp = new List<string>();
            customerName = new List<string>();
            seats = new List<string>();
            status = new List<string>();
            
            //Get selected seats
            for (x = 1; x <= 18; x++)
            {
                Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;

                if (btn.BackColor == Color.LimeGreen && btn.Enabled)
                {
                    toBeCancelledList.Add(btn.Text);
                }
            }

            if (toBeCancelledList.Count>0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to cancel these reservations?", "Cancel Reservation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    //Get names of to be cancelled seats
                   foreach(string str in toBeCancelledList)
                   {
                        foreach (string res in reservedSeats)
                        {
                            listArray = res.Split('-');

                            if (listArray[1].ToString()==str)
                            {
                                toBeCancelledName.Add(listArray[2]);
                                removeList.Add(res);
                            }
                        }
                   }

                   //Remove reserved of cancelled
                   foreach(string str in removeList)
                   {
                        reservedSeats.Remove(str);
                   }

                   //Add to list for history grid view
                    for (x = 0; x < toBeCancelledList.Count; x++)
                    {
                        //Check if name is same
                        insertToHistoryList(toBeCancelledName[x],toBeCancelledList[x],"Cancelled");
                    }

                    //Insert into History Data Grid View
                    AddToDataGridView();

                    //Save to XML
                    movieSeats._reservedSeats = reservedSeats;
                    saveData();

                    CancelButtonFunction();
                }
            }
            else
            {
                MessageBox.Show("There are no seats selected", "Cancel Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            //Check if there is xml file, read it if there is
            try
            {
                //Convert time
                outputTime = fc.ConvertTime(time);

                if (fc.checkFileExist(movie))
                {
                    xml = fc.ReadXMLFile(movie);
                    movieSeats = (Seats)XMLToObject(xml, typeof(Seats));

                    reservedSeats = movieSeats._reservedSeats;
                    usedSeats = movieSeats._usedSeats;

                    RefreshForm(Color.Red, reservedSeats);
                    RefreshForm(Color.DarkGray, usedSeats);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetValues();
            }

            string[] historyArray;

            //Disable controls
            if (isDone)
            {
                disableReservations();

                foreach (string str in reservedSeats)
                {
                    listArray = str.Split('-');

                    //Check if date is same
                    if (DateTime.Parse(date) == DateTime.Parse(listArray[3]))
                    {
                        //Check if time is done
                        if (fc.TimeIsGreaterOrEqual(fc.ConvertTime(listArray[0]), DateTime.Now.ToString("HH:mm:ss")))
                        {
                            usedSeats.Add(str);
                        }
                    }
                }

                foreach (string str in usedSeats)
                {
                    reservedSeats.Remove(str);
                }

                //Save to XML
                movieSeats._usedSeats = usedSeats;
                saveData();

                RefreshForm(Color.DarkGray, usedSeats);
            }
            else
            {
                if (DateTime.Parse(date) == DateTime.Parse(DateTime.Now.ToString("d")))
                {
                    //Start timer
                    InitTimer();
                }
            }

            //Initialize values

            //Determine seat price based on movie
            if (movie == "Ralph Breaks the Internet")
            {
                seatPrice = 250.00;
            }
            else if (movie == "Aquaman")
            {
                seatPrice = 280.00;
            }
            else if (movie == "Bumblebee")
            {
                seatPrice = 230.00;
            }
            else if (movie == "Do You Believe")
            {
                seatPrice = 200.00;
            }

            //Display values in GUI
            movieLabel.Text =  movie;
            timeLabel.Text = time;
            seatPriceLabel.Text = seatPrice.ToString() + ".00";
            dateLabel.Text = date; 

            if (FunctionClass.historyList.Count > 0)
            {
                foreach (string str in FunctionClass.historyList)
                {
                    historyArray = str.Split('|');

                    historyDataGridView.Rows.Add(historyArray[0], historyArray[1], historyArray[2], historyArray[3]);
                }
            }

            timer2 = new Timer();
            timer2.Tick += new EventHandler(timer2_Tick);
            timer2.Interval = 1000; // in miliseconds
            timer2.Start();
        }

        private void exportToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (historyDataGridView.Rows.Count > 0)
            {
                saveFileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("History is empty", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog.FileName == "")
            {
                MessageBox.Show("Filename is empty");
            }
            else
            {
                try
                {
                    //Write to file
                    StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false);
                    sw.WriteLine("Timestamp\t\tCustomer_Name\t\tSeats\t\tStatus");

                    for (int rows = 0; rows < historyDataGridView.Rows.Count; rows++)
                    {
                        for (int col = 0; col < historyDataGridView.Rows[rows].Cells.Count; col++)
                        {
                            if (col == historyDataGridView.Rows[rows].Cells.Count - 1)
                            {
                                sw.Write(historyDataGridView.Rows[rows].Cells[col].Value.ToString());
                            }   
                            else
                            {
                                sw.Write(historyDataGridView.Rows[rows].Cells[col].Value.ToString() + "\t\t");
                            }
                                
                        }
                        sw.Write(Environment.NewLine);
                    }

                    sw.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetValues();
                }
            }
        }

        private void importFromTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //Reset values
                    errorFound = false;
                    int lineCount = 1;
                    int lastIndex = 0;
                    int tryOutput = 0;
                    int index = 0;
                    string testParse = "";
                    string errorMessage = "";

                    DateTime tryDateTime;
                    timestamp = new List<string>();
                    customerName = new List<string>();
                    seats = new List<string>();
                    status = new List<string>();

                    //Check if text file is empty or not
                    if (new FileInfo(openFileDialog.FileName).Length == 0)
                    {
                        MessageBox.Show("File is empty", "File Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        //Check text file line by line
                        foreach (string line in File.ReadLines(openFileDialog.FileName))
                        {
                            if (lineCount == 1)
                            {
                                //Check header
                                if (line != "Timestamp\t\tCustomer_Name\t\tSeats\t\tStatus")
                                {
                                    errorFound = true;
                                    errorMessage = "There is something wrong in the file header. (Line#:" + lineCount + ")";
                                }
                            }
                            else
                            {
                                if (!errorFound)
                                {
                                    //Find index
                                    for (i = lastIndex; i < line.Length; i++)
                                    {
                                        if (line[i] == Convert.ToChar(9))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }

                                    //Check timestamp
                                    testParse = "";

                                    for (i = lastIndex; i < index; i++)
                                    {
                                        testParse += line[i].ToString();
                                    }

                                    if (!DateTime.TryParse(testParse, out tryDateTime))
                                    {
                                        errorFound = true;
                                        errorMessage = "There is something wrong in the timestamp. (Line#:" + lineCount + ")";
                                        break;
                                    }
                                    else
                                    {
                                        timestamp.Add(testParse);
                                    }

                                    lastIndex = index + 2;

                                    //Check customer name

                                    for (i = lastIndex; i < line.Length; i++)
                                    {
                                        if (line[i] == Convert.ToChar(9))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }

                                    testParse = "";

                                    for (i = lastIndex; i < index; i++)
                                    {
                                        testParse += line[i].ToString();
                                    }

                                    for (i = lastIndex; i < index; i++)
                                    {
                                        if (!Char.IsLetter(line[i]) && line[i]!=' ')
                                        {
                                            errorFound = true;
                                            errorMessage = "There is something wrong in the customer name. (Line#:" + lineCount + ")";
                                            break;
                                        }
                                    }

                                    if (!errorFound)
                                    {
                                        customerName.Add(testParse);
                                    }

                                    lastIndex = index + 2;

                                    //Check seats

                                    for (i = lastIndex; i < line.Length; i++)
                                    {
                                        if (line[i] == Convert.ToChar(9))
                                        {
                                            index = i;
                                            break;
                                        }
                                    }

                                    testParse = "";

                                    for (i = lastIndex; i < index; i++)
                                    {
                                        testParse += line[i].ToString();
                                    }

                                    for (i = lastIndex; i < index; i++)
                                    {
                                        if (!int.TryParse(line[i].ToString(), out tryOutput) && line[i]!=',')
                                        {
                                            errorFound = true;
                                            errorMessage = "There is something wrong in the seats. (Line#:" + lineCount + ")";
                                            break;
                                        }
                                    }

                                    if (!errorFound)
                                    {
                                        seats.Add(testParse);
                                    }

                                    lastIndex = index + 2;

                                    //Check status
                                    testParse = "";

                                    for (i = lastIndex; i < line.Length; i++)
                                    {
                                        testParse += line[i].ToString();
                                    }

                                    if (testParse != "Reserved" && testParse != "Cancelled" && testParse!="Used")
                                    {
                                        errorFound = true;
                                        errorMessage = "There is something wrong in the status. (Line#:" + lineCount + ")";
                                        break;
                                    }
                                    else
                                    {
                                        status.Add(testParse);
                                    }
                                }
                            }

                            lineCount++;
                        }

                        if (errorFound)
                        {
                            MessageBox.Show("Not a valid history file\n" + errorMessage, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            for (x = 0; x < historyDataGridView.RowCount; x++)
                            {
                                historyDataGridView.Rows.Remove(historyDataGridView.Rows[x]);
                            }
                            
                            AddToDataGridView();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ResetValues();
                }
             }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (!isDone && timer1!=null)
            {
                timer1.Stop();
            }
            timer2.Stop();

            this.Hide();
            Form1 f1 = new Form1();
            f1.Show();
        }
        private void clearHistoryButton_Click(object sender, EventArgs e)
        {
            historyDataGridView.Rows.Clear();
            FunctionClass.historyList.Clear();
        }


        //-------------------------------------------------------------
        //----------------------------GUI Functions--------------------
        //-------------------------------------------------------------

        private bool IfSameDateAndTime()
        {
            bool ret = false;
            foreach (string str in reservedSeats)
            {
                listArray = str.Split('-');
                //Check if date is same
                if (DateTime.Parse(date) == DateTime.Parse(listArray[3]))
                {
                    //Check if time is same
                    if (time == listArray[0])
                    {
                        ret = true;
                    }
                }
            }
            return ret;
        }
        
        private void ClearFunction()
        {
            List<string> removeToList = new List<string>();
            //Check if there are reserved seats
            if (reservedSeats.Count > 0)
            {
                if (IfSameDateAndTime())
                {
                    dialogResult = MessageBox.Show("Are you sure you want to cancel all reservations?", "Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        foreach (string str in reservedSeats)
                        {
                            listArray = str.Split('-');
                            //Check if date is same
                            if (DateTime.Parse(date) == DateTime.Parse(listArray[3]))
                            {
                                //Check if time is same
                                if (time == listArray[0])
                                {
                                    insertToHistoryList(listArray[2], listArray[1], "Cancelled");
                                }
                            }
                            removeToList.Add(str);
                        }

                        RefreshForm(Color.LimeGreen, reservedSeats);
                        toBeReservedSeats.Clear();

                        foreach(string str in removeToList)
                        {
                            reservedSeats.Remove(str);
                        }

                        AddToDataGridView();

                        //Save to XML
                        movieSeats._reservedSeats = reservedSeats;
                        saveData();
                    }
                }
                else
                {
                    MessageBox.Show("There are no reservations", "Cancel Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("There are no reservations", "Clear Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshForm(Color seatColor, List<string> seatList)
        {

            if (seatList.Count > 0)
            {
                foreach (string str in seatList)
                {
                    
                    listArray = str.Split('-');

                    //Check if date is same
                    if (DateTime.Parse(date)==DateTime.Parse(listArray[3]))
                    {
                        //Check if time is same
                        if (time == listArray[0])
                        {
                                Button btn = this.Controls.Find("seat" + listArray[1], true).FirstOrDefault() as Button;
                                btn.BackColor = seatColor;
                        }
                    }
                }
            }

            customerTextBox.Text = "";
        }

        public void AddToDataGridView()
        {
            for (x = 0; x < status.Count; x++)
            {
                historyDataGridView.Rows.Insert(0, timestamp[x], customerName[x], seats[x], status[x]);
                FunctionClass.historyList.Add(timestamp[x] + "|" + customerName[x] + "|" + seats[x] + "|" +  status[x]);
            }
        }
        public void saveData()
        {
            try
            {
                xml = GetXMLFromObject(movieSeats);
                fc.SaveXMLFile(xml, movie);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetValues();
            }
        }

        public void insertToHistoryList(string nameToCompare, string seatNo, string seatStatus)
        {
            if (customerName.Contains(nameToCompare))
            {
                seats[customerName.IndexOf(nameToCompare)] += "," + seatNo;
            }
            else
            {
                timestamp.Add(DateTime.Now.ToString());
                customerName.Add(nameToCompare);
                seats.Add(seatNo);
                status.Add(seatStatus);
            }
        }
        public void InitTimer()
        {
            timer1 = new Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                //Check if time is greater than or equal
                //if (fc.TimeIsGreaterOrEqual(outputTime, DateTime.Now.ToString("HH:mm:ss")))
                //{
                    //Reset
                    timestamp = new List<string>();
                    customerName = new List<string>();
                    seats = new List<string>();
                    status = new List<string>();
                    Console.WriteLine("-----------------------");
                    foreach (string str in reservedSeats)
                    {
                        Console.WriteLine(str);
                        listArray = str.Split('-');

                        

                        //Check if date is same
                        if (DateTime.Parse(DateTime.Now.ToString("d")) == DateTime.Parse(listArray[3]))
                        {
                            if (DateTime.Parse(date) == DateTime.Parse(listArray[3]))
                            {
                                //Check if time is done
                                if (fc.ConvertTime(listArray[0]) != outputTime)
                                {
                                    if (fc.TimeIsGreaterOrEqual(fc.ConvertTime(listArray[0]), DateTime.Now.ToString("HH:mm:ss")))
                                    {
                                        usedSeats.Add(str);
                                        insertToHistoryList(listArray[2], listArray[1], "Used");
                                    }
                                }
                            } 
                        }
                    }

                    foreach (string str in usedSeats)
                    {
                        reservedSeats.Remove(str);
                    }


                    AddToDataGridView();

                //Save to XML
                movieSeats._usedSeats = usedSeats;
                saveData();



                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ResetValues();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //Check if time is greater than or equal
                if (fc.TimeIsGreaterOrEqual(outputTime, DateTime.Now.ToString("HH:mm:ss")))
                {
                    //Reset
                    timestamp = new List<string>();
                    customerName = new List<string>();
                    seats = new List<string>();
                    status = new List<string>();

                    timer1.Stop();
                        RefreshForm(Color.DarkGray, reservedSeats);

                        foreach (string str in reservedSeats)
                        {
                            listArray = str.Split('-');

                            //Check if date is same
                            if (DateTime.Parse(DateTime.Now.ToString("d")) == DateTime.Parse(listArray[3]))
                            {
                                if (DateTime.Parse(date) == DateTime.Parse(listArray[3]))
                                {
                                    //Check if time is same
                                    if (time == listArray[0])
                                    {
                                        usedSeats.Add(str);
                                        insertToHistoryList(listArray[2], listArray[1], "Used");
                                    }
                                }
                            } 
                        }

                        foreach(string str in usedSeats)
                        {
                            reservedSeats.Remove(str);
                        }
                        

                        
                        AddToDataGridView();
                        disableReservations();

                        //Save to XML
                        movieSeats._usedSeats = usedSeats;
                        saveData();

                        MessageBox.Show("Time is already up! Cannot reserve seats at this time", "Reservation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Hide();
                        Form1 form1 = new Form1();
                        form1.Show();
                        
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CancelButtonFunction()
        {
            customerTextBox.Enabled = true;
            reserveButton.Enabled = true;
            clearButton.Enabled = true;
            cancelReservationButton.Enabled = true;
            cancelButton.Visible = false;
            okayButton.Visible = false;
            backButton.Enabled = true;

            for (x = 1; x <= 18; x++)
            {
                Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;

                if (btn.BackColor != Color.Red)
                {
                    btn.Enabled = true;
                }
                btn.FlatStyle = FlatStyle.Standard;
            }

            cancelReservationButtonLastClick = false;
            RefreshForm(Color.Red, reservedSeats);
        }

        public void disableReservations()
        {
                for (x = 1; x <= 18; x++)
                {
                    Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;
                    btn.Enabled = false;
                    btn.FlatStyle = FlatStyle.Standard;
                    
                    if (btn.BackColor==Color.Gold)
                    {
                        btn.BackColor = Color.LimeGreen;
                    }
                }

                customerTextBox.Enabled = false;
                reserveButton.Enabled = false;
                okayButton.Visible = false;
                cancelButton.Visible = false;

                clearButton.Enabled = false;
                cancelReservationButton.Enabled = false;
                customerTextBox.Text = "";

                seatCount = 0;
                totalPrice = 0.0;

                displaySeatInfo();
        }

        public void displaySeatInfo()
        {
            seatCountLabel.Text = seatCount.ToString();
            totalAmountLabel.Text = totalPrice + ".00";
        }

        public void ResetValues()
        {
            timer1 = new Timer();
            timer2 = new Timer();
            totalPrice = 0.0;
            seatCount = 0;

            toBeReservedSeats = new List<string>();

            timestamp = new List<string>();
            customerName = new List<string>();
            seats = new List<string>();
            status = new List<string>();

            cancelReservationButtonLastClick = false;
            errorFound = false;

            for (x = 1; x <= 18; x++)
            {
                Button btn = this.Controls.Find("seat" + x, true).FirstOrDefault() as Button;
                btn.Enabled = true;
                btn.FlatStyle = FlatStyle.Standard;

                if (btn.BackColor == Color.Gold)
                {
                    btn.BackColor = Color.LimeGreen;
                }
            }

            displaySeatInfo();
        }

        public static string GetXMLFromObject(object o)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, o);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
            }
            return sw.ToString();
        }

        public static Object XMLToObject(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            Object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception Catch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }
    }
}
