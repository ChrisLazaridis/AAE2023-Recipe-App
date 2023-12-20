using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AAE2023_P22083_M3
{
    public partial class Form1 : Form
    {
        private const string ConnectionString = "Data source=Recipes.db;Version=3";
        private readonly SQLiteConnection _connection = new SQLiteConnection(ConnectionString);
        private int _calories, _portions, _calLowerBound, _calUpperBound;
        private string _foundTitle;

        public Form1()
        {
            InitializeComponent();
            checkedListBox1.CheckOnClick = false;
            SQLiteFunction.RegisterFunction(typeof(LevenshteinDistanceExtension.LevenshteinDistanceFunction));

        }


        private async void buttonFindAll_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = true;
            checkedListBox1.Enabled = true;

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    const string selectSql = "SELECT Title FROM Recipes";
                    SQLiteCommand command = new SQLiteCommand(selectSql, conn);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            checkedListBox1.Items.Add(reader["Title"]);
                        });
                    }
                }
            });
        }

        private async void buttonSearchByCalories_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = true;
            checkedListBox1.Enabled = true;
            if (textBoxCaloriesLowerBound.Text == "" || !int.TryParse(textBoxCaloriesLowerBound.Text, out _calLowerBound))
            {
                _calLowerBound = 0;
            }

            if (textBoxCaloriesUpperBound.Text == "" || !int.TryParse(textBoxCaloriesUpperBound.Text, out _calUpperBound))
            {
                _calUpperBound = 90000;
            }

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    const string selectSql = "SELECT Title FROM Recipes WHERE Calories BETWEEN @CalLowerBound AND @CalUpperBound";
                    SQLiteCommand command = new SQLiteCommand(selectSql, conn);
                    command.Parameters.AddWithValue("@CalLowerBound", _calLowerBound);
                    command.Parameters.AddWithValue("@CalUpperBound", _calUpperBound);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            checkedListBox1.Items.Add(reader["Title"]);
                        });
                    }
                }
            });
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            _connection.Open();
            _foundTitle = checkedListBox1.SelectedItem.ToString();
            const string selectSql = "SELECT * FROM Recipes WHERE Title = @Title";
            SQLiteCommand command = new SQLiteCommand(selectSql, _connection);
            command.Parameters.AddWithValue("@Title", _foundTitle);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                richTextBox3.Text = @"Τίτλος: ";
                richTextBox3.Text += reader["Title"].ToString();
                richTextBox3.Text += "\n\n";
                richTextBox3.Text = @"Υλικά: ";
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += reader["Ingredients"].ToString();
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += @"Εκτέλεση: ";
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += reader["Description"].ToString();
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += @"Μερίδες: ";
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += reader["Portions"].ToString();
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += @"Κατηγορία: ";
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += reader["Category"].ToString();
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += @"Θερμίδες: ";
                richTextBox3.Text += "\n\n";
                richTextBox3.Text += reader["Calories"].ToString();
                richTextBox3.Text += "\n\n";
                pictureBox1.Image = null;
                if (reader["Photo"].ToString() != "")
                {
                    pictureBox1.Load(reader["Photo"].ToString());
                }
            }
            _connection.Close();
            buttonEdit.Enabled = true;
            buttonDelete.Enabled = true;


        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            _connection.Open();
            const string selectSql = "SELECT * FROM Recipes WHERE Title = @Title";
            SQLiteCommand command = new SQLiteCommand(selectSql, _connection);
            command.Parameters.AddWithValue("@Title", _foundTitle);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                textBoxTitle.Text = reader["Title"].ToString();
                richTextBoxIngredients.Text = reader["Ingredients"].ToString();
                richTextBoxDescription.Text = reader["Description"].ToString();
                textBoxPortions.Text = reader["Portions"].ToString();
                comboBox1.Text = reader["Category"].ToString();
                textBoxCalories.Text = reader["Calories"].ToString();
                if (reader["Photo"].ToString() != "")
                {
                    textBoxPhoto.Text = reader["Photo"].ToString();
                }
            }
            _connection.Close();
            buttonChange.Enabled = true;
            buttonSubmit.Enabled = false;
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            _connection.Open();
            const string deleteSql = "DELETE FROM Recipes WHERE Title = @Title";
            SQLiteCommand command = new SQLiteCommand(deleteSql, _connection);
            command.Parameters.AddWithValue("@Title", _foundTitle);
            command.ExecuteNonQuery();
            var dialogResult = MessageBox.Show(@"Η συνταγή διαγράφτηκε!");
            if (dialogResult == DialogResult.OK)
            {
                _connection.Close();
            }
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            buttonChange.Enabled = false;
            buttonSubmit.Enabled = true;
            _connection.Open();

            if (textBoxTitle.Text != "" && richTextBoxIngredients.Text != "" && richTextBoxDescription.Text != "" &&
                textBoxPortions.Text != "" && comboBox1.Text != "" && textBoxCalories.Text != "" &&
                int.TryParse(textBoxCalories.Text, out _calories) && int.TryParse(textBoxPortions.Text, out _portions))
            {
                const string updateSql = "UPDATE Recipes SET " +
                                         "Ingredients = COALESCE(@Ingredients, Ingredients)," +
                                         "Description = COALESCE(@Description, Description)," +
                                         "Portions = COALESCE(@Portions, Portions)," +
                                         "Category = COALESCE(@Category, Category)," +
                                         "Calories = COALESCE(@Calories, Calories)," +
                                         "Photo = COALESCE(@Photo, Photo) " +
                                         "WHERE Title = @Title";

                SQLiteCommand command = new SQLiteCommand(updateSql, _connection);
                command.Parameters.AddWithValue("@Title", textBoxTitle.Text);
                command.Parameters.AddWithValue("@Ingredients", string.IsNullOrWhiteSpace(richTextBoxIngredients.Text) ? null : richTextBoxIngredients.Text);
                command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(richTextBoxDescription.Text) ? null : richTextBoxDescription.Text);
                command.Parameters.AddWithValue("@Portions", _portions);
                command.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(comboBox1.Text) ? null : comboBox1.Text);
                command.Parameters.AddWithValue("@Calories", _calories);
                command.Parameters.AddWithValue("@Photo", string.IsNullOrWhiteSpace(textBoxPhoto.Text) ? null : (object)textBoxPhoto.Text);
                command.ExecuteNonQuery();

                var dialogResult = MessageBox.Show(@"Η συνταγή ανανεώθηκε!");
                checkedListBox1.Items.Clear();

                if (dialogResult == DialogResult.OK)
                {
                    _connection.Close();
                }

                if (textBoxPhoto.Text != "")
                {
                    pictureBox1.Load(textBoxPhoto.Text);
                }
            }
            else
            {
                var dialogResult = MessageBox.Show(@"Παρακαλώ συμπληρώστε σωστά όλα τα υποχρεωτικά πεδία (διάβασε info -> about");

                if (dialogResult == DialogResult.OK)
                {
                    _connection.Close();
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message =
                "Αυτή η εφαρμογή είναι η τρίτη εργασία στο Μάθημα Αντικειμενοστρεφής Ανάπτυξη εφαρμογών απο τον φοιτητή Χρήστο Λαζαρίδη με ΑΜ: P22083" +
                "\n\n είναι συνδεδεμένη με μια βάση SQLite με τα εξής πεδία" +
                "\n\n περιέχει τα εξής υποχρεωτικά πεδία:" +
                "\n\n Τίτλος, Υλικά, Εκτέλεση, Μερίδες, Κατηγορία, Θερμίδες" +
                "\n\n και τα εξής προαιρετικά πεδία:" +
                "\n\n Φωτογραφία" +
                "\n\n Η εφαρμογή επιτρέπει την προσθήκη, την αναζήτηση, την επεξεργασία και την διαγραφή συνταγών" +
                "\n\n Η αναζήτηση γίνεται με:" +
                "\n\n τη χρήση διαστήματος θερμίδων (default 0-90000)" +
                "τη χρήση κατηγορίας συνταγής και του τίτλου";
            MessageBox.Show(message);


        }


        private async void buttonSearchByTitle_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = true;
            checkedListBox1.Enabled = true;

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    const string selectSql = "SELECT Title FROM Recipes WHERE LevenshteinDistance(@Title, LOWER(Title)) <= 6";

                    SQLiteCommand command = new SQLiteCommand(selectSql, conn);
                    this.Invoke((MethodInvoker)delegate{
                        command.Parameters.AddWithValue("@Title", textBoxSearchByTitle.Text.ToLower());
                    });
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            checkedListBox1.Items.Add(reader["Title"]);
                        });
                    }
                }
            });
        }


        private async void buttonSearchByCategory_Click(object sender, EventArgs e)
        {
            checkedListBox1.Items.Clear();
            checkedListBox1.Visible = true;
            checkedListBox1.Enabled = true;

            await Task.Run(() =>
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    const string selectSql = "SELECT Title FROM Recipes WHERE Category = @Category";
                    SQLiteCommand command = new SQLiteCommand(selectSql, conn);
                    this.Invoke((MethodInvoker)delegate
                    {
                        command.Parameters.AddWithValue("@Category", comboBoxSearchByCategory.Text);
                    });
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            checkedListBox1.Items.Add(reader["Title"]);
                        });
                    }
                }
            });
        }


        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            _connection.Open();
            if (textBoxTitle.Text != "" && richTextBoxIngredients.Text != "" && richTextBoxDescription.Text != "" && textBoxPortions.Text != "" &&
               comboBox1.Text != "" && textBoxCalories.Text != "" && int.TryParse(textBoxCalories.Text, out _calories) && int.TryParse(textBoxPortions.Text, out _portions))
            {
                const string selectSql = "SELECT COUNT(*) FROM Recipes WHERE Title COLLATE NOCASE = @Title";
                SQLiteCommand selectCommand = new SQLiteCommand(selectSql, _connection);
                selectCommand.Parameters.AddWithValue("@Title", textBoxTitle.Text);
                int count = Convert.ToInt32(selectCommand.ExecuteScalar());
                if (count > 0)
                {
                    DialogResult result = MessageBox.Show(@"Υπάρχει ήδη μια συνταγή με αυτό τον τίτλο. Θέλετε να ενημερώσετε τη συνταγή;", @"Επιβεβαίωση", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        const string updateSql = "UPDATE Recipes SET Ingredients = @Ingredients, " +
                                                 "Description = @Description, " +
                                                 "Portions = @Portions, " +
                                                 "Category = @Category, " +
                                                 "Calories = @Calories, " +
                                                 "Photo = @Photo " +
                                                 "WHERE Title = @Title";
                        SQLiteCommand updateCommand = new SQLiteCommand(updateSql, _connection);
                        updateCommand.Parameters.AddWithValue("@Title", textBoxTitle.Text);
                        updateCommand.Parameters.AddWithValue("@Ingredients", richTextBoxIngredients.Text);
                        updateCommand.Parameters.AddWithValue("@Description", richTextBoxDescription.Text);
                        updateCommand.Parameters.AddWithValue("@Portions", _portions);
                        updateCommand.Parameters.AddWithValue("@Category", comboBox1.Text);
                        updateCommand.Parameters.AddWithValue("@Calories", _calories);
                        updateCommand.Parameters.AddWithValue("@Photo", string.IsNullOrEmpty(textBoxPhoto.Text) ? DBNull.Value : (object)textBoxPhoto.Text);
                        updateCommand.ExecuteNonQuery();

                        MessageBox.Show(@"Η συνταγή ενημερώθηκε!");
                    }
                    else
                    {
                        textBoxTitle.Text = "";
                        richTextBoxIngredients.Text = "";
                        richTextBoxDescription.Text = "";
                        textBoxPortions.Text = "";
                        comboBox1.Text = "";
                        textBoxCalories.Text = "";
                        textBoxPhoto.Text = "";
                    }
                }
                else
                {
                    const string insertSql = "INSERT INTO Recipes (Title," +
                                             "Ingredients," +
                                             "Description," +
                                             "Portions," +
                                             "Category," +
                                             "Calories," +
                                             "Photo) VALUES (" +
                                             "@Title," +
                                             "@Ingredients," +
                                             "@Description," +
                                             "@Portions," +
                                             "@Category," +
                                             "@Calories," +
                                             "@Photo)";

                    SQLiteCommand insertCommand = new SQLiteCommand(insertSql, _connection);
                    insertCommand.Parameters.AddWithValue("@Title", textBoxTitle.Text);
                    insertCommand.Parameters.AddWithValue("@Ingredients", richTextBoxIngredients.Text);
                    insertCommand.Parameters.AddWithValue("@Description", richTextBoxDescription.Text);
                    insertCommand.Parameters.AddWithValue("@Portions", _portions);
                    insertCommand.Parameters.AddWithValue("@Category", comboBox1.Text);
                    insertCommand.Parameters.AddWithValue("@Calories", _calories);
                    insertCommand.Parameters.AddWithValue("@Photo", string.IsNullOrEmpty(textBoxPhoto.Text) ? DBNull.Value : (object)textBoxPhoto.Text);
                    insertCommand.ExecuteNonQuery();

                    MessageBox.Show(@"Η συνταγή αποθηκεύτηκε!");
                    checkedListBox1.Items.Clear();
                }
                _connection.Close();

                if (textBoxPhoto.Text != "")
                {
                    try
                    {
                        pictureBox1.Load(textBoxPhoto.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($@"Error: φόρτωση εικόνας: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Παρακαλώ συμπληρώστε σωστά όλα τα υποχρεωτικά πεδία (διάβασε info -> about");
                _connection.Close();
            }
        }
        




    }
}
